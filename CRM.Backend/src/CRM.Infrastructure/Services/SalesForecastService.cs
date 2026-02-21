// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing sales forecasts
/// </summary>
public class SalesForecastService : ISalesForecastService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<SalesForecastService> _logger;

    public SalesForecastService(ICrmDbContext context, ILogger<SalesForecastService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SalesForecast>> GetAllAsync(
        int? userId = null,
        int? teamId = null,
        int? fiscalYear = null,
        bool? isSubmitted = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Getting forecasts with filters: UserId={UserId}, TeamId={TeamId}, FiscalYear={FiscalYear}, IsSubmitted={IsSubmitted}",
            userId, teamId, fiscalYear, isSubmitted);

        var query = _context.SalesForecasts.AsNoTracking().Where(f => !f.IsDeleted);

        if (userId.HasValue)
        {
            query = query.Where(f => f.UserId == userId.Value);
        }

        if (teamId.HasValue)
        {
            query = query.Where(f => f.TeamId == teamId.Value);
        }

        if (fiscalYear.HasValue)
        {
            query = query.Where(f => f.FiscalYear == fiscalYear.Value);
        }

        if (isSubmitted.HasValue)
        {
            query = query.Where(f => f.IsSubmitted == isSubmitted.Value);
        }

        var forecasts = await query
            .OrderByDescending(f => f.SnapshotDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} forecasts", forecasts.Count);
        return forecasts;
    }

    /// <inheritdoc />
    public async Task<SalesForecast?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting forecast by ID: {ForecastId}", id);

        var forecast = await _context.SalesForecasts
            .AsNoTracking()
            .Include(f => f.LineItems.Where(li => !li.IsDeleted))
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, cancellationToken);

        if (forecast == null)
        {
            _logger.LogWarning("Forecast not found: {ForecastId}", id);
        }

        return forecast;
    }

    /// <inheritdoc />
    public async Task<SalesForecast> CreateAsync(SalesForecast forecast, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(forecast);

        _logger.LogDebug("Creating forecast: {Name} for period {Period}", forecast.Name, forecast.Period);

        forecast.CreatedAt = DateTime.UtcNow;
        forecast.UpdatedAt = DateTime.UtcNow;
        forecast.IsDeleted = false;
        forecast.SnapshotDate = DateTime.UtcNow;

        _context.SalesForecasts.Add(forecast);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created forecast with ID: {ForecastId}, Name: {Name}", forecast.Id, forecast.Name);
        return forecast;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(int id, SalesForecast forecast, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(forecast);

        _logger.LogDebug("Updating forecast: {ForecastId}", id);

        var existing = await _context.SalesForecasts
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, cancellationToken);

        if (existing == null)
        {
            _logger.LogWarning("Forecast not found for update: {ForecastId}", id);
            return false;
        }

        existing.Name = forecast.Name;
        existing.Period = forecast.Period;
        existing.PeriodStartDate = forecast.PeriodStartDate;
        existing.PeriodEndDate = forecast.PeriodEndDate;
        existing.FiscalYear = forecast.FiscalYear;
        existing.FiscalQuarter = forecast.FiscalQuarter;
        existing.FiscalMonth = forecast.FiscalMonth;
        existing.QuotaAmount = forecast.QuotaAmount;
        existing.CurrencyCode = forecast.CurrencyCode;
        existing.ClosedWonAmount = forecast.ClosedWonAmount;
        existing.CommitAmount = forecast.CommitAmount;
        existing.BestCaseAmount = forecast.BestCaseAmount;
        existing.PipelineAmount = forecast.PipelineAmount;
        existing.OmittedAmount = forecast.OmittedAmount;
        existing.ClosedWonCount = forecast.ClosedWonCount;
        existing.CommitCount = forecast.CommitCount;
        existing.BestCaseCount = forecast.BestCaseCount;
        existing.PipelineCount = forecast.PipelineCount;
        existing.AdjustedCommitAmount = forecast.AdjustedCommitAmount;
        existing.AdjustedBestCaseAmount = forecast.AdjustedBestCaseAmount;
        existing.AdjustmentNotes = forecast.AdjustmentNotes;
        existing.AdjustedById = forecast.AdjustedById;
        existing.AdjustedAt = forecast.AdjustedAt;
        existing.UserId = forecast.UserId;
        existing.TeamId = forecast.TeamId;
        existing.SalesQuotaId = forecast.SalesQuotaId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated forecast: {ForecastId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting forecast: {ForecastId}", id);

        var forecast = await _context.SalesForecasts
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, cancellationToken);

        if (forecast == null)
        {
            _logger.LogWarning("Forecast not found for deletion: {ForecastId}", id);
            return false;
        }

        // Soft delete
        forecast.IsDeleted = true;
        forecast.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted forecast: {ForecastId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> SubmitAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Submitting forecast: {ForecastId}", id);

        var forecast = await _context.SalesForecasts
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, cancellationToken);

        if (forecast == null)
        {
            _logger.LogWarning("Forecast not found for submission: {ForecastId}", id);
            return false;
        }

        forecast.IsSubmitted = true;
        forecast.SubmittedAt = DateTime.UtcNow;
        forecast.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Submitted forecast: {ForecastId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ForecastHistory>> GetHistoryAsync(
        string period,
        int? userId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting forecast history for period {Period}, UserId={UserId}", period, userId);

        var query = _context.ForecastHistories
            .AsNoTracking()
            .Where(h => h.Period == period && !h.IsDeleted);

        if (userId.HasValue)
        {
            query = query.Where(h => h.UserId == userId.Value);
        }

        var history = await query
            .OrderByDescending(h => h.SnapshotDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} forecast history records for period {Period}", history.Count, period);
        return history;
    }

    /// <inheritdoc />
    public async Task<ForecastHistory> CreateSnapshotAsync(int forecastId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Creating snapshot for forecast: {ForecastId}", forecastId);

        var forecast = await _context.SalesForecasts
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == forecastId && !f.IsDeleted, cancellationToken);

        if (forecast == null)
        {
            throw new ArgumentException($"Forecast not found: {forecastId}", nameof(forecastId));
        }

        var snapshot = new ForecastHistory
        {
            SnapshotDate = DateTime.UtcNow,
            Period = forecast.Period,
            UserId = forecast.UserId,
            TeamId = forecast.TeamId,
            QuotaAmount = forecast.QuotaAmount,
            ClosedWonAmount = forecast.ClosedWonAmount,
            CommitAmount = forecast.CommitAmount,
            BestCaseAmount = forecast.BestCaseAmount,
            PipelineAmount = forecast.PipelineAmount,
            WeeksRemaining = (int)Math.Ceiling((forecast.PeriodEndDate - DateTime.UtcNow).TotalDays / 7),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.ForecastHistories.Add(snapshot);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created forecast snapshot with ID: {SnapshotId} for forecast {ForecastId}", snapshot.Id, forecastId);
        return snapshot;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ForecastLineItem>> GetLineItemsAsync(int forecastId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting line items for forecast: {ForecastId}", forecastId);

        var lineItems = await _context.ForecastLineItems
            .AsNoTracking()
            .Where(li => li.SalesForecastId == forecastId && !li.IsDeleted)
            .OrderBy(li => li.CloseDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} line items for forecast {ForecastId}", lineItems.Count, forecastId);
        return lineItems;
    }
}

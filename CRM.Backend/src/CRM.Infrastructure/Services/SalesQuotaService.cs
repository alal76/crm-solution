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
/// Service for managing sales quotas
/// </summary>
public class SalesQuotaService : ISalesQuotaService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<SalesQuotaService> _logger;

    public SalesQuotaService(ICrmDbContext context, ILogger<SalesQuotaService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SalesQuota>> GetAllAsync(
        int? userId = null,
        int? teamId = null,
        int? fiscalYear = null,
        QuotaPeriodType? periodType = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Getting quotas with filters: UserId={UserId}, TeamId={TeamId}, FiscalYear={FiscalYear}, PeriodType={PeriodType}",
            userId, teamId, fiscalYear, periodType);

        var query = _context.SalesQuotas.AsNoTracking().Where(q => !q.IsDeleted);

        if (userId.HasValue)
        {
            query = query.Where(q => q.UserId == userId.Value);
        }

        if (teamId.HasValue)
        {
            query = query.Where(q => q.TeamId == teamId.Value);
        }

        if (fiscalYear.HasValue)
        {
            query = query.Where(q => q.FiscalYear == fiscalYear.Value);
        }

        if (periodType.HasValue)
        {
            query = query.Where(q => q.PeriodType == periodType.Value);
        }

        var quotas = await query
            .OrderByDescending(q => q.FiscalYear)
            .ThenBy(q => q.Period)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} quotas", quotas.Count);
        return quotas;
    }

    /// <inheritdoc />
    public async Task<SalesQuota?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting quota by ID: {QuotaId}", id);

        var quota = await _context.SalesQuotas
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted, cancellationToken);

        if (quota == null)
        {
            _logger.LogWarning("Quota not found: {QuotaId}", id);
        }

        return quota;
    }

    /// <inheritdoc />
    public async Task<SalesQuota> CreateAsync(SalesQuota quota, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(quota);

        _logger.LogDebug("Creating quota: {Name} for period {Period}", quota.Name, quota.Period);

        quota.CreatedAt = DateTime.UtcNow;
        quota.IsDeleted = false;

        _context.SalesQuotas.Add(quota);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created quota with ID: {QuotaId}, Name: {Name}", quota.Id, quota.Name);
        return quota;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(int id, SalesQuota quota, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(quota);

        _logger.LogDebug("Updating quota: {QuotaId}", id);

        var existing = await _context.SalesQuotas
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted, cancellationToken);

        if (existing == null)
        {
            _logger.LogWarning("Quota not found for update: {QuotaId}", id);
            return false;
        }

        existing.Name = quota.Name;
        existing.PeriodType = quota.PeriodType;
        existing.Metric = quota.Metric;
        existing.Period = quota.Period;
        existing.FiscalYear = quota.FiscalYear;
        existing.FiscalQuarter = quota.FiscalQuarter;
        existing.FiscalMonth = quota.FiscalMonth;
        existing.PeriodStartDate = quota.PeriodStartDate;
        existing.PeriodEndDate = quota.PeriodEndDate;
        existing.TargetAmount = quota.TargetAmount;
        existing.CurrencyCode = quota.CurrencyCode;
        existing.StretchTargetAmount = quota.StretchTargetAmount;
        existing.MinimumTargetAmount = quota.MinimumTargetAmount;
        existing.ActualAmount = quota.ActualAmount;
        existing.NewBusinessAmount = quota.NewBusinessAmount;
        existing.RenewalAmount = quota.RenewalAmount;
        existing.ExpansionAmount = quota.ExpansionAmount;
        existing.UserId = quota.UserId;
        existing.TeamId = quota.TeamId;
        existing.Notes = quota.Notes;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated quota: {QuotaId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting quota: {QuotaId}", id);

        var quota = await _context.SalesQuotas
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted, cancellationToken);

        if (quota == null)
        {
            _logger.LogWarning("Quota not found for deletion: {QuotaId}", id);
            return false;
        }

        // Soft delete
        quota.IsDeleted = true;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted quota: {QuotaId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SalesQuota>> GetByUserAndYearAsync(int userId, int fiscalYear, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting quotas for user {UserId}, year {FiscalYear}", userId, fiscalYear);

        var quotas = await _context.SalesQuotas
            .AsNoTracking()
            .Where(q => q.UserId == userId && q.FiscalYear == fiscalYear && !q.IsDeleted)
            .OrderBy(q => q.PeriodStartDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} quotas for user {UserId}, year {FiscalYear}", quotas.Count, userId, fiscalYear);
        return quotas;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SalesQuota>> GetByTeamAndYearAsync(int teamId, int fiscalYear, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting quotas for team {TeamId}, year {FiscalYear}", teamId, fiscalYear);

        var quotas = await _context.SalesQuotas
            .AsNoTracking()
            .Where(q => q.TeamId == teamId && q.FiscalYear == fiscalYear && !q.IsDeleted)
            .OrderBy(q => q.PeriodStartDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} quotas for team {TeamId}, year {FiscalYear}", quotas.Count, teamId, fiscalYear);
        return quotas;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAttainmentAsync(int id, decimal actualAmount, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating attainment for quota: {QuotaId}, Amount: {Amount}", id, actualAmount);

        var quota = await _context.SalesQuotas
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted, cancellationToken);

        if (quota == null)
        {
            _logger.LogWarning("Quota not found for attainment update: {QuotaId}", id);
            return false;
        }

        quota.ActualAmount = actualAmount;
        quota.LastRefreshedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated attainment for quota: {QuotaId}, Amount: {Amount}, Attainment: {Attainment}%",
            id, actualAmount, quota.AttainmentPercent);
        return true;
    }
}

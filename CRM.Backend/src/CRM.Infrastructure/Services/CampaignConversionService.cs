// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of ICampaignConversionService for campaign conversion management.
/// Handles tracking and attributing conversions to marketing campaigns.
/// </summary>
public class CampaignConversionService : ICampaignConversionService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<CampaignConversionService> _logger;

    public CampaignConversionService(ICrmDbContext context, ILogger<CampaignConversionService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<(List<CampaignConversionDto> Items, int TotalCount)> GetAllAsync(
        string? filter = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.CampaignConversions
            .Where(c => !c.IsDeleted)
            .Include(c => c.Campaign)
            .Include(c => c.Contact)
            .Include(c => c.Account)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var filterLower = filter.ToLower();
            query = query.Where(c =>
                c.ConversionType.ToLower().Contains(filterLower) ||
                (c.ExternalOrderId != null && c.ExternalOrderId.ToLower().Contains(filterLower)) ||
                (c.ExternalTransactionId != null && c.ExternalTransactionId.ToLower().Contains(filterLower)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.ConvertedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => MapToDto(c))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<CampaignConversionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var conversion = await _context.CampaignConversions
            .Where(c => c.Id == id && !c.IsDeleted)
            .Include(c => c.Campaign)
            .Include(c => c.Contact)
            .Include(c => c.Account)
            .FirstOrDefaultAsync(cancellationToken);

        return conversion == null ? null : MapToDto(conversion);
    }

    /// <inheritdoc />
    public async Task<List<CampaignConversionDto>> GetByCampaignIdAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        var conversions = await _context.CampaignConversions
            .Where(c => c.CampaignId == campaignId && !c.IsDeleted)
            .Include(c => c.Campaign)
            .Include(c => c.Contact)
            .Include(c => c.Account)
            .OrderByDescending(c => c.ConvertedAt)
            .ToListAsync(cancellationToken);

        return conversions.Select(c => MapToDto(c)).ToList();
    }

    /// <inheritdoc />
    public async Task<CampaignConversionDto> CreateAsync(CreateCampaignConversionDto dto, CancellationToken cancellationToken = default)
    {
        // Verify campaign exists
        var campaignExists = await _context.MarketingCampaigns
            .AnyAsync(c => c.Id == dto.CampaignId && !c.IsDeleted, cancellationToken);

        if (!campaignExists)
        {
            throw new InvalidOperationException($"Campaign with ID {dto.CampaignId} not found");
        }

        var conversion = new CampaignConversion
        {
            CampaignId = dto.CampaignId,
            CampaignRecipientId = dto.CampaignRecipientId,
            ContactId = dto.ContactId,
            AccountId = dto.AccountId,
            ConversionType = dto.ConversionType,
            ConversionValue = dto.ConversionValue,
            ConversionCurrency = dto.ConversionCurrency,
            AttributionModel = dto.AttributionModel,
            AttributionPercentage = dto.AttributionPercentage,
            ConversionData = dto.ConversionData,
            ConvertedAt = dto.ConvertedAt ?? DateTime.UtcNow,
            ExternalOrderId = dto.ExternalOrderId,
            ExternalTransactionId = dto.ExternalTransactionId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CampaignConversions.Add(conversion);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created campaign conversion {Id} for campaign {CampaignId}", conversion.Id, conversion.CampaignId);

        // Reload with navigation properties
        return (await GetByIdAsync(conversion.Id, cancellationToken))!;
    }

    /// <inheritdoc />
    public async Task<CampaignConversionDto?> UpdateAsync(int id, UpdateCampaignConversionDto dto, CancellationToken cancellationToken = default)
    {
        var conversion = await _context.CampaignConversions
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (conversion == null)
        {
            return null;
        }

        // Update only non-null fields
        if (dto.CampaignRecipientId.HasValue)
            conversion.CampaignRecipientId = dto.CampaignRecipientId;
        if (dto.ContactId.HasValue)
            conversion.ContactId = dto.ContactId;
        if (dto.AccountId.HasValue)
            conversion.AccountId = dto.AccountId;
        if (!string.IsNullOrEmpty(dto.ConversionType))
            conversion.ConversionType = dto.ConversionType;
        if (dto.ConversionValue.HasValue)
            conversion.ConversionValue = dto.ConversionValue;
        if (!string.IsNullOrEmpty(dto.ConversionCurrency))
            conversion.ConversionCurrency = dto.ConversionCurrency;
        if (!string.IsNullOrEmpty(dto.AttributionModel))
            conversion.AttributionModel = dto.AttributionModel;
        if (dto.AttributionPercentage.HasValue)
            conversion.AttributionPercentage = dto.AttributionPercentage.Value;
        if (dto.ConversionData != null)
            conversion.ConversionData = dto.ConversionData;
        if (dto.ConvertedAt.HasValue)
            conversion.ConvertedAt = dto.ConvertedAt.Value;
        if (dto.ExternalOrderId != null)
            conversion.ExternalOrderId = dto.ExternalOrderId;
        if (dto.ExternalTransactionId != null)
            conversion.ExternalTransactionId = dto.ExternalTransactionId;

        conversion.UpdatedAt = DateTime.UtcNow;

        _context.CampaignConversions.Update(conversion);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated campaign conversion {Id}", id);

        return await GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var conversion = await _context.CampaignConversions
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (conversion == null)
        {
            return false;
        }

        conversion.IsDeleted = true;
        conversion.UpdatedAt = DateTime.UtcNow;

        _context.CampaignConversions.Update(conversion);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Soft deleted campaign conversion {Id}", id);

        return true;
    }

    private static CampaignConversionDto MapToDto(CampaignConversion conversion)
    {
        return new CampaignConversionDto
        {
            Id = conversion.Id,
            CampaignId = conversion.CampaignId,
            CampaignName = conversion.Campaign?.Name,
            CampaignRecipientId = conversion.CampaignRecipientId,
            ContactId = conversion.ContactId,
            ContactName = conversion.Contact != null ? $"{conversion.Contact.FirstName} {conversion.Contact.LastName}".Trim() : null,
            AccountId = conversion.AccountId,
            AccountName = conversion.Account?.DisplayName,
            ConversionType = conversion.ConversionType,
            ConversionValue = conversion.ConversionValue,
            ConversionCurrency = conversion.ConversionCurrency,
            AttributionModel = conversion.AttributionModel,
            AttributionPercentage = conversion.AttributionPercentage,
            ConversionData = conversion.ConversionData,
            ConvertedAt = conversion.ConvertedAt,
            ExternalOrderId = conversion.ExternalOrderId,
            ExternalTransactionId = conversion.ExternalTransactionId,
            CreatedAt = conversion.CreatedAt,
            UpdatedAt = conversion.UpdatedAt
        };
    }
}

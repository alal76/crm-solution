// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using CRM.Core.Dtos;
using CRM.Api.Mappers;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Marketing campaign service implementation.
///
/// HEXAGONAL ARCHITECTURE:
/// - Implements ICampaignInputPort (primary/driving port)
/// - Implements IMarketingCampaignService (backward compatibility)
/// - Uses IRepository for data access (secondary/driven port)
/// </summary>
public class MarketingCampaignService : IMarketingCampaignService, ICampaignInputPort
{
    private readonly IRepository<MarketingCampaign> _repository;
    private readonly IRepository<CampaignMetric> _metricRepository;
    private readonly IRepository<CRM.Core.Entities.EntityTag> _entityTagRepository;
    private readonly IRepository<CRM.Core.Entities.CustomField> _customFieldRepository;
    private readonly NormalizationService _normalizationService;

    public MarketingCampaignService(IRepository<MarketingCampaign> repository, IRepository<CampaignMetric> metricRepository,
        IRepository<CRM.Core.Entities.EntityTag> entityTagRepository,
        IRepository<CRM.Core.Entities.CustomField> customFieldRepository,
        NormalizationService normalizationService)
    {
        _repository = repository;
        _metricRepository = metricRepository;
        _entityTagRepository = entityTagRepository;
        _customFieldRepository = customFieldRepository;
        _normalizationService = normalizationService;
    }

    public async Task<CampaignDto?> GetCampaignByIdAsync(int id)
    {
        var c = await _repository.GetByIdAsync(id);
        if (c == null) return null;
        var tags = await _normalizationService.GetTagsAsync("MarketingCampaign", c.Id);
        if (!string.IsNullOrWhiteSpace(tags)) c.Tags = tags;
        var cfs = await _normalizationService.GetCustomFieldsAsync("MarketingCampaign", c.Id);
        if (!string.IsNullOrWhiteSpace(cfs)) c.CustomFields = cfs;
        return CampaignMapper.ToDto(c);
    }

    public async Task<IEnumerable<CampaignDto>> GetAllCampaignsAsync()
    {
        var items = await _repository.GetAllAsync();
        foreach (var c in items)
        {
            var tags = await _normalizationService.GetTagsAsync("MarketingCampaign", c.Id);
            if (!string.IsNullOrWhiteSpace(tags)) c.Tags = tags;
            var cfs = await _normalizationService.GetCustomFieldsAsync("MarketingCampaign", c.Id);
            if (!string.IsNullOrWhiteSpace(cfs)) c.CustomFields = cfs;
        }
        return items.Select(CampaignMapper.ToDto);
    }

    public async Task<IEnumerable<CampaignDto>> GetActiveCampaignsAsync()
    {
        var items = await _repository.FindAsync(c => !c.IsDeleted && c.Status == CampaignStatus.Active);
        foreach (var c in items)
        {
            var tags = await _normalizationService.GetTagsAsync("MarketingCampaign", c.Id);
            if (!string.IsNullOrWhiteSpace(tags)) c.Tags = tags;
            var cfs = await _normalizationService.GetCustomFieldsAsync("MarketingCampaign", c.Id);
            if (!string.IsNullOrWhiteSpace(cfs)) c.CustomFields = cfs;
        }
        return items.Select(CampaignMapper.ToDto);
    }

    /// <summary>
    /// Validates campaign business rules
    /// </summary>
    private void ValidateCampaign(MarketingCampaign campaign)
    {
        if (string.IsNullOrWhiteSpace(campaign.Name))
        {
            throw new ArgumentException("Campaign name is required");
        }

        if (campaign.EndDate.HasValue && campaign.StartDate.HasValue &&
            campaign.EndDate.Value < campaign.StartDate.Value)
        {
            throw new ArgumentException("End date cannot be before start date");
        }

        if (campaign.Budget < 0)
        {
            throw new ArgumentException("Budget cannot be negative");
        }

        if (campaign.ActualCost < 0)
        {
            throw new ArgumentException("Actual cost cannot be negative");
        }

        if (campaign.TargetAudience < 0)
        {
            throw new ArgumentException("Target audience cannot be negative");
        }
    }

    public async Task<int> CreateCampaignAsync(CreateCampaignDto dto)
    {
        var campaign = CampaignMapper.ToEntity(dto);
        ValidateCampaign(campaign);
        campaign.CreatedAt = DateTime.UtcNow;
        await _repository.AddAsync(campaign);
        await _repository.SaveAsync();
        return campaign.Id;
    }

    public async Task UpdateCampaignAsync(int id, UpdateCampaignDto dto)
    {
        var campaign = await _repository.GetByIdAsync(id);
        if (campaign == null)
            throw new InvalidOperationException($"Campaign with ID {id} not found");
        CampaignMapper.UpdateEntity(campaign, dto);
        ValidateCampaign(campaign);
        campaign.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(campaign);
        await _repository.SaveAsync();
    }

    public async Task DeleteCampaignAsync(int id)
    {
        var campaign = await _repository.GetByIdAsync(id);
        if (campaign == null)
        {
            throw new InvalidOperationException($"Campaign with ID {id} not found");
        }

        // Soft delete by marking as deleted rather than hard delete
        // This preserves campaign history for metrics and reporting
        campaign.IsDeleted = true;
        campaign.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(campaign);
        await _repository.SaveAsync();
    }

    public async Task AddCampaignMetricAsync(CampaignMetric metric)
    {
        await _metricRepository.AddAsync(metric);
        await _metricRepository.SaveAsync();
    }
}

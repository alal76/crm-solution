using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;

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

    public async Task<MarketingCampaign?> GetCampaignByIdAsync(int id)
    {
        var c = await _repository.GetByIdAsync(id);
        if (c == null) return null;

        var tags = await _normalizationService.GetTagsAsync("MarketingCampaign", c.Id);
        if (!string.IsNullOrWhiteSpace(tags)) c.Tags = tags;

        var cfs = await _normalizationService.GetCustomFieldsAsync("MarketingCampaign", c.Id);
        if (!string.IsNullOrWhiteSpace(cfs)) c.CustomFields = cfs;

        return c;
    }

    public async Task<IEnumerable<MarketingCampaign>> GetAllCampaignsAsync()
    {
        var items = await _repository.GetAllAsync();
        foreach (var c in items)
        {
            var tags = await _normalizationService.GetTagsAsync("MarketingCampaign", c.Id);
            if (!string.IsNullOrWhiteSpace(tags)) c.Tags = tags;

            var cfs = await _normalizationService.GetCustomFieldsAsync("MarketingCampaign", c.Id);
            if (!string.IsNullOrWhiteSpace(cfs)) c.CustomFields = cfs;
        }
        return items;
    }

    public async Task<IEnumerable<MarketingCampaign>> GetActiveCampaignsAsync()
    {
        var items = await _repository.FindAsync(c => !c.IsDeleted && c.Status == CampaignStatus.Active);
        foreach (var c in items)
        {
            var tags = await _normalizationService.GetTagsAsync("MarketingCampaign", c.Id);
            if (!string.IsNullOrWhiteSpace(tags)) c.Tags = tags;

            var cfs = await _normalizationService.GetCustomFieldsAsync("MarketingCampaign", c.Id);
            if (!string.IsNullOrWhiteSpace(cfs)) c.CustomFields = cfs;
        }
        return items;
    }

    public async Task<int> CreateCampaignAsync(MarketingCampaign campaign)
    {
        await _repository.AddAsync(campaign);
        await _repository.SaveAsync();
        return campaign.Id;
    }

    public async Task UpdateCampaignAsync(MarketingCampaign campaign)
    {
        campaign.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(campaign);
        await _repository.SaveAsync();
    }

    public async Task DeleteCampaignAsync(int id)
    {
        var campaign = await _repository.GetByIdAsync(id);
        if (campaign != null)
        {
            await _repository.DeleteAsync(campaign);
            await _repository.SaveAsync();
        }
    }

    public async Task AddCampaignMetricAsync(CampaignMetric metric)
    {
        await _metricRepository.AddAsync(metric);
        await _metricRepository.SaveAsync();
    }
}

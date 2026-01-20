using CRM.Core.Entities;
using CRM.Core.Interfaces;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Marketing campaign service implementation
/// </summary>
public class MarketingCampaignService : IMarketingCampaignService
{
    private readonly IRepository<MarketingCampaign> _repository;
    private readonly IRepository<CampaignMetric> _metricRepository;

    public MarketingCampaignService(IRepository<MarketingCampaign> repository, IRepository<CampaignMetric> metricRepository)
    {
        _repository = repository;
        _metricRepository = metricRepository;
    }

    public async Task<MarketingCampaign?> GetCampaignByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<MarketingCampaign>> GetAllCampaignsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<IEnumerable<MarketingCampaign>> GetActiveCampaignsAsync()
    {
        return await _repository.FindAsync(c => !c.IsDeleted && c.Status == CampaignStatus.Active);
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

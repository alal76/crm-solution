using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Marketing campaign service interface
/// </summary>
public interface IMarketingCampaignService
{
    Task<MarketingCampaign?> GetCampaignByIdAsync(int id);
    Task<IEnumerable<MarketingCampaign>> GetAllCampaignsAsync();
    Task<IEnumerable<MarketingCampaign>> GetActiveCampaignsAsync();
    Task<int> CreateCampaignAsync(MarketingCampaign campaign);
    Task UpdateCampaignAsync(MarketingCampaign campaign);
    Task DeleteCampaignAsync(int id);
    Task AddCampaignMetricAsync(CampaignMetric metric);
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

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

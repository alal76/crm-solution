// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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

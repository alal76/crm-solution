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

namespace CRM.Infrastructure.Providers.Algolia;

public class AlgoliaConfiguration
{
    public const string SectionName = "Providers:Search:Algolia";
    public string ApplicationId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string SearchOnlyApiKey { get; set; } = string.Empty;
    public string IndexPrefix { get; set; } = "crm_";
    public int DefaultPageSize { get; set; } = 20;
    public int MaxPageSize { get; set; } = 100;
    public int TimeoutSeconds { get; set; } = 30;
    public bool EnableHighlighting { get; set; } = true;
    public bool EnableSnippets { get; set; } = true;
    public bool AutoSyncEnabled { get; set; } = true;
    public int BatchSize { get; set; } = 1000;
    public bool WaitForTasks { get; set; } = false;
    public bool EnableAnalytics { get; set; } = false;
    public bool EnablePersonalization { get; set; } = false;
}

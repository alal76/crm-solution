// CRM Solution - Pluggable Architecture
// Algolia Configuration

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

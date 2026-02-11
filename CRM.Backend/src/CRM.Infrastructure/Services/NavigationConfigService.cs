using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Features;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace CRM.Infrastructure.Services
{
    /// <summary>
    /// Service that provides navigation configuration with pluggable architecture awareness.
    /// This runs at application startup to determine which navigation items should be shown
    /// based on feature flags, provider configuration, and module availability.
    /// </summary>
    public class NavigationConfigService : INavigationConfigService
    {
        private readonly IFeatureManager _featureManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NavigationConfigService> _logger;

        // Cache for performance
        private NavigationConfig? _cachedConfig;
        private DateTime _cacheExpiry = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        public NavigationConfigService(
            IFeatureManager featureManager,
            IConfiguration configuration,
            ILogger<NavigationConfigService> logger)
        {
            _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<NavigationConfig> GetNavigationConfigAsync(CancellationToken cancellationToken = default)
        {
            // Return cached config if still valid
            if (_cachedConfig != null && DateTime.UtcNow < _cacheExpiry)
            {
                return _cachedConfig;
            }

            _logger.LogInformation("Building navigation configuration...");

            var config = new NavigationConfig
            {
                Categories = GetCategories(),
                AdminSubcategories = GetAdminSubcategories(),
                ExternalServices = await GetExternalServiceConfigsAsync(cancellationToken),
                ProviderStatus = await GetProviderStatusAsync(cancellationToken),
                FeatureFlags = await GetFeatureFlagsAsync(cancellationToken)
            };

            config.NavItems = await BuildNavItemsAsync(config, cancellationToken);

            // Cache the config
            _cachedConfig = config;
            _cacheExpiry = DateTime.UtcNow.Add(_cacheDuration);

            _logger.LogInformation("Navigation configuration built with {Count} items", config.NavItems.Count);

            return config;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<NavigationItemConfig>> GetAvailableNavItemsAsync(CancellationToken cancellationToken = default)
        {
            var config = await GetNavigationConfigAsync(cancellationToken);
            return config.NavItems.Where(n => n.Enabled && n.Visible);
        }

        /// <inheritdoc />
        public Task<Dictionary<string, ExternalServiceConfig>> GetExternalServiceConfigsAsync(CancellationToken cancellationToken = default)
        {
            var services = new Dictionary<string, ExternalServiceConfig>();

            // Integration Platform (n8n, Zapier, Make)
            var integrationProvider = _configuration["Providers:Integrations:Type"] ?? "BuiltIn";
            var n8nUrl = _configuration["Providers:Integrations:N8n:BaseUrl"];
            services["integrations"] = new ExternalServiceConfig
            {
                Enabled = !string.IsNullOrEmpty(n8nUrl) && integrationProvider.Equals("N8n", StringComparison.OrdinalIgnoreCase),
                Url = n8nUrl,
                ProviderType = integrationProvider,
                UseInternal = integrationProvider.Equals("BuiltIn", StringComparison.OrdinalIgnoreCase),
                DisplayName = GetIntegrationDisplayName(integrationProvider)
            };

            // Analytics (Superset, Metabase, Power BI)
            var analyticsProvider = _configuration["Providers:Analytics:Type"] ?? "BuiltIn";
            var supersetUrl = _configuration["Providers:Analytics:Superset:Url"];
            var metabaseUrl = _configuration["Providers:Analytics:Metabase:Url"];
            var powerBiUrl = _configuration["Providers:Analytics:PowerBI:Url"];
            services["analytics"] = new ExternalServiceConfig
            {
                Enabled = !analyticsProvider.Equals("BuiltIn", StringComparison.OrdinalIgnoreCase),
                Url = analyticsProvider switch
                {
                    "Superset" => supersetUrl,
                    "Metabase" => metabaseUrl,
                    "PowerBI" => powerBiUrl,
                    _ => null
                },
                ProviderType = analyticsProvider,
                UseInternal = analyticsProvider.Equals("BuiltIn", StringComparison.OrdinalIgnoreCase),
                DisplayName = GetAnalyticsDisplayName(analyticsProvider)
            };

            // Search (Meilisearch, Algolia, Elasticsearch)
            var searchProvider = _configuration["Providers:Search:Type"] ?? "BuiltIn";
            var meilisearchUrl = _configuration["Providers:Search:Meilisearch:Url"];
            services["search"] = new ExternalServiceConfig
            {
                Enabled = !searchProvider.Equals("BuiltIn", StringComparison.OrdinalIgnoreCase),
                Url = searchProvider.Equals("Meilisearch", StringComparison.OrdinalIgnoreCase) ? meilisearchUrl : null,
                ProviderType = searchProvider,
                UseInternal = searchProvider.Equals("BuiltIn", StringComparison.OrdinalIgnoreCase),
                DisplayName = searchProvider
            };

            // Chat (Chatwoot, Intercom)
            var chatProvider = _configuration["Providers:Chat:Type"] ?? "BuiltIn";
            var chatwootUrl = _configuration["Providers:Chat:Chatwoot:BaseUrl"];
            services["chat"] = new ExternalServiceConfig
            {
                Enabled = !chatProvider.Equals("BuiltIn", StringComparison.OrdinalIgnoreCase),
                Url = chatProvider.Equals("Chatwoot", StringComparison.OrdinalIgnoreCase) ? chatwootUrl : null,
                ProviderType = chatProvider,
                UseInternal = chatProvider.Equals("BuiltIn", StringComparison.OrdinalIgnoreCase),
                DisplayName = chatProvider
            };

            // Notifications (Novu, Twilio, SendGrid)
            var notificationProvider = _configuration["Providers:Notifications:Type"] ?? "BuiltIn";
            services["notifications"] = new ExternalServiceConfig
            {
                Enabled = !notificationProvider.Equals("BuiltIn", StringComparison.OrdinalIgnoreCase),
                ProviderType = notificationProvider,
                UseInternal = notificationProvider.Equals("BuiltIn", StringComparison.OrdinalIgnoreCase),
                DisplayName = notificationProvider
            };

            // AI/LLM (Ollama, OpenAI, Azure, etc.)
            var aiProvider = _configuration["Providers:AI:Type"] ?? "Ollama";
            var ollamaUrl = _configuration["Providers:AI:Ollama:Url"];
            services["ai"] = new ExternalServiceConfig
            {
                Enabled = true, // AI is always enabled in some form
                Url = aiProvider.Equals("Ollama", StringComparison.OrdinalIgnoreCase) ? ollamaUrl : null,
                ProviderType = aiProvider,
                UseInternal = aiProvider.Equals("Ollama", StringComparison.OrdinalIgnoreCase),
                DisplayName = GetAIDisplayName(aiProvider)
            };

            // E-Signatures (DocuSeal, DocuSign)
            var signatureProvider = _configuration["Providers:Signatures:Type"] ?? "BuiltIn";
            var docuSealUrl = _configuration["Providers:Signatures:DocuSeal:Url"];
            services["signatures"] = new ExternalServiceConfig
            {
                Enabled = !signatureProvider.Equals("BuiltIn", StringComparison.OrdinalIgnoreCase),
                Url = signatureProvider.Equals("DocuSeal", StringComparison.OrdinalIgnoreCase) ? docuSealUrl : null,
                ProviderType = signatureProvider,
                UseInternal = signatureProvider.Equals("BuiltIn", StringComparison.OrdinalIgnoreCase),
                DisplayName = signatureProvider
            };

            return Task.FromResult(services);
        }

        /// <inheritdoc />
        public async Task<Dictionary<string, ProviderStatus>> GetProviderStatusAsync(CancellationToken cancellationToken = default)
        {
            var status = new Dictionary<string, ProviderStatus>();

            var services = await GetExternalServiceConfigsAsync(cancellationToken);

            foreach (var (key, service) in services)
            {
                status[key] = new ProviderStatus
                {
                    ProviderType = service.ProviderType,
                    IsActive = service.Enabled || service.UseInternal,
                    IsHealthy = true, // TODO: Integrate with AdapterRegistry for real health checks
                    LastHealthCheck = DateTime.UtcNow,
                    Message = service.Enabled && !service.UseInternal
                        ? $"Using external provider: {service.ProviderType}"
                        : "Using internal/built-in provider"
                };
            }

            return status;
        }

        /// <summary>
        /// Builds the complete list of navigation items with provider awareness.
        /// </summary>
        private async Task<List<NavigationItemConfig>> BuildNavItemsAsync(NavigationConfig config, CancellationToken cancellationToken)
        {
            var items = new List<NavigationItemConfig>();

            // === MAIN NAVIGATION ITEMS ===

            // Main category
            items.Add(NavItem("dashboard", "Dashboard", "/dashboard", "Dashboard", "CanAccessDashboard", "main", 1));
            items.Add(NavItem("accounts", "Accounts", "/accounts", "Business", "CanAccessCustomers", "main", 2));
            items.Add(NavItem("customer-overview", "Customer 360°", "/customer-overview", "Preview", "CanAccessCustomers", "main", 3));
            items.Add(NavItem("contacts", "Contacts", "/contacts", "ContactPage", "CanAccessContacts", "main", 4));

            // Sales category
            items.Add(NavItem("leads", "Leads", "/leads", "PersonSearch", "CanAccessLeads", "sales", 1));
            items.Add(NavItem("opportunities", "Opportunities", "/opportunities", "TrendingUp", "CanAccessOpportunities", "sales", 2));
            items.Add(NavItem("quotes", "Quotes", "/quotes", "RequestQuote", "CanAccessQuotes", "sales", 3));
            items.Add(NavItem("orders", "Orders", "/orders", "ShoppingCart", "CanAccessOrders", "sales", 4));
            items.Add(NavItem("invoices", "Invoices", "/invoices", "Receipt", "CanAccessInvoices", "sales", 5));
            items.Add(NavItem("payments", "Payments", "/payments", "Payment", "CanAccessPayments", "sales", 6));
            items.Add(NavItem("contracts", "Contracts", "/contracts", "Description", "CanAccessContracts", "sales", 7));
            items.Add(NavItem("subscriptions", "Subscriptions", "/subscriptions", "Subscriptions", "CanAccessSubscriptions", "sales", 8));
            items.Add(NavItem("products", "Products", "/products", "Inventory", "CanAccessProducts", "sales", 9));
            items.Add(NavItem("commissions", "Commissions", "/commissions", "AttachMoney", "CanAccessCommissions", "sales", 10));
            items.Add(NavItem("teams", "Teams", "/teams", "Groups", "CanAccessTeams", "sales", 11));
            items.Add(NavItem("territories", "Territories", "/territories", "Map", "CanAccessTerritories", "sales", 12));

            // Support category
            items.Add(NavItem("service-requests", "Service Requests", "/service-requests", "SupportAgent", "CanAccessServiceRequests", "support", 1));
            items.Add(NavItem("knowledge-base", "Knowledge Base", "/knowledge-base", "MenuBook", "CanAccessKnowledgeBase", "support", 2));
            items.Add(NavItem("services", "Services", "/services", "Build", "CanAccessServices", "support", 3));

            // ITSM category (conditionally shown based on EnableITSM feature flag)
            var itsmEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.EnableITSM);
            if (itsmEnabled)
            {
                items.Add(NavItem("itsm-overview", "ITSM Overview", "/itsm", "Speed", "CanAccessITSM", "itsm", 1, requiredFeature: FeatureFlags.EnableITSM));
                items.Add(NavItem("itsm-metrics", "Metrics", "/itsm/metrics", "BarChart", "CanAccessITSM", "itsm", 2, requiredFeature: FeatureFlags.EnableITSM));
                items.Add(NavItem("itsm-incidents", "Incidents", "/itsm/incidents", "Warning", "CanAccessITSM", "itsm", 3, requiredFeature: FeatureFlags.EnableITSM));
                items.Add(NavItem("itsm-problems", "Problems", "/itsm/problems", "BugReport", "CanAccessITSM", "itsm", 4, requiredFeature: FeatureFlags.EnableITSM));
                items.Add(NavItem("itsm-changes", "Changes", "/itsm/changes", "ChangeCircle", "CanAccessITSM", "itsm", 5, requiredFeature: FeatureFlags.EnableITSM));
                items.Add(NavItem("itsm-cmdb", "CMDB", "/itsm/cmdb", "Storage", "CanAccessITSM", "itsm", 6, requiredFeature: FeatureFlags.EnableITSM));
                items.Add(NavItem("itsm-knowledge", "KB Articles", "/itsm/knowledge", "Article", "CanAccessITSM", "itsm", 7, requiredFeature: FeatureFlags.EnableITSM));
                items.Add(NavItem("itsm-catalog", "Service Catalog", "/itsm/catalog", "Category", "CanAccessITSM", "itsm", 8, requiredFeature: FeatureFlags.EnableITSM));
                items.Add(NavItem("itsm-sla", "SLA Dashboard", "/itsm/sla", "Schedule", "CanAccessITSM", "itsm", 9, requiredFeature: FeatureFlags.EnableITSM));
            }

            // Marketing category (conditionally shown based on EnableMarketing feature flag)
            var marketingEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.EnableMarketing);
            if (marketingEnabled)
            {
                items.Add(NavItem("campaigns", "Campaigns", "/campaigns", "Campaign", "CanAccessCampaigns", "marketing", 1, requiredFeature: FeatureFlags.EnableMarketing));
                items.Add(NavItem("email-templates", "Email Templates", "/email-templates", "Email", "CanAccessCampaigns", "marketing", 2, requiredFeature: FeatureFlags.EnableMarketing));
                items.Add(NavItem("campaign-execution", "Campaign Execution", "/campaign-execution", "PlayCircle", "CanAccessCampaigns", "marketing", 3, requiredFeature: FeatureFlags.EnableMarketing));
                items.Add(NavItem("landing-pages", "Landing Pages", "/landing-pages", "Web", "CanAccessCampaigns", "marketing", 4, requiredFeature: FeatureFlags.EnableMarketing));
                items.Add(NavItem("lead-routing", "Lead Routing", "/lead-routing", "Route", "CanAccessLeads", "marketing", 5, requiredFeature: FeatureFlags.EnableMarketing));
            }

            // Productivity category
            items.Add(NavItem("my-queue", "My Queue", "/my-queue", "Queue", "CanAccessActivities", "productivity", 1));
            items.Add(NavItem("activities", "Activities", "/activities", "Event", "CanAccessActivities", "productivity", 2));
            items.Add(NavItem("tasks", "Tasks", "/tasks", "CheckCircle", "CanAccessTasks", "productivity", 3));
            items.Add(NavItem("notes", "Notes", "/notes", "Note", "CanAccessNotes", "productivity", 4));
            items.Add(NavItem("communications", "Communications", "/communications", "Chat", "CanAccessActivities", "productivity", 5));
            items.Add(NavItem("interactions", "Interactions", "/interactions", "Forum", "CanAccessActivities", "productivity", 6));
            items.Add(NavItem("approvals", "Approvals", "/approvals", "ThumbUp", "CanAccessApprovals", "productivity", 7));
            items.Add(NavItem("relationships", "Relationships", "/relationships", "Share", "CanAccessCustomers", "productivity", 8));

            // Info category
            items.Add(NavItem("reports", "Reports", "/reports", "Assessment", "CanAccessReports", "info", 1));

            // Analytics with pluggable provider support
            var analyticsService = config.ExternalServices.GetValueOrDefault("analytics");
            if (analyticsService?.Enabled == true && !analyticsService.UseInternal)
            {
                items.Add(NavItem("analytics", analyticsService.DisplayName, analyticsService.Url ?? "/analytics", "Insights", "CanAccessReports", "info", 2, isExternal: true, externalUrl: analyticsService.Url, providerType: analyticsService.ProviderType));
            }
            else
            {
                items.Add(NavItem("analytics", "Analytics", "/analytics", "Insights", "CanAccessReports", "info", 2));
            }

            items.Add(NavItem("about", "About", "/about", "Info", null, "info", 3));
            items.Add(NavItem("help", "Help", "/help", "Help", null, "info", 4));
            items.Add(NavItem("api-documentation", "API Docs", "/help/api", "Api", null, "info", 5));
            items.Add(NavItem("licenses", "Licenses", "/licenses", "Gavel", null, "info", 6));

            // === ADMIN NAVIGATION ITEMS ===

            // Admin - System subcategory
            items.Add(AdminItem("monitoring-settings", "Monitoring", "/admin/monitoring", "MonitorHeart", "CanAccessSettings", "admin-system", 1));
            items.Add(AdminItem("deployment-settings", "Deployment", "/admin/deployment", "Cloud", "CanAccessSettings", "admin-system", 2));
            items.Add(AdminItem("security-settings", "Security", "/admin/security", "Security", "CanAccessSettings", "admin-system", 3));
            items.Add(AdminItem("feature-management", "Feature Flags", "/admin/features", "Flag", "CanAccessSettings", "admin-system", 4));
            items.Add(AdminItem("database-settings", "Database", "/admin/database-settings", "Storage", "CanAccessSettings", "admin-system", 5));
            items.Add(AdminItem("test-results", "Test Results", "/admin/test-results", "Science", "CanAccessSettings", "admin-system", 6));
            items.Add(AdminItem("llm-settings", "AI/LLM Settings", "/admin/llm", "Psychology", "CanAccessSettings", "admin-system", 7));

            // Admin - Users subcategory
            items.Add(AdminItem("user-management", "Users", "/admin/users", "People", "CanAccessUserManagement", "admin-users", 1));
            items.Add(AdminItem("user-approvals", "User Approvals", "/admin/approvals", "HowToReg", "CanAccessUserManagement", "admin-users", 2));
            items.Add(AdminItem("group-management", "Groups", "/admin/groups", "GroupWork", "CanAccessUserManagement", "admin-users", 3));
            items.Add(AdminItem("social-login", "Social Login", "/admin/social-login", "Login", "CanAccessSettings", "admin-users", 4));

            // Admin - CRM subcategory
            items.Add(AdminItem("branding-settings", "Branding", "/admin/branding", "Palette", "CanAccessSettings", "admin-crm", 1));
            items.Add(AdminItem("master-data", "Master Data", "/admin/master-data", "Dataset", "CanAccessSettings", "admin-crm", 2));
            items.Add(AdminItem("duplicate-rules", "Duplicate Rules", "/admin/duplicate-rules", "ContentCopy", "CanAccessSettings", "admin-crm", 3));
            items.Add(AdminItem("lead-score-rules", "Lead Scoring", "/admin/lead-score-rules", "Score", "CanAccessSettings", "admin-crm", 4));

            // Admin - Service subcategory
            items.Add(AdminItem("sr-definitions", "SR Definitions", "/admin/service-requests", "Assignment", "CanAccessSettings", "admin-service", 1));
            items.Add(AdminItem("dashboard-settings", "Dashboards", "/admin/dashboards", "Dashboard", "CanAccessSettings", "admin-service", 2));

            // Admin - Navigation subcategory
            items.Add(AdminItem("navigation-settings", "Navigation", "/admin/navigation", "Menu", "CanAccessSettings", "admin-navigation", 1));

            // Admin - Modules subcategory
            items.Add(AdminItem("module-fields", "Module Fields", "/admin/modules", "ViewModule", "CanAccessSettings", "admin-modules", 1));

            // Admin - Workflows subcategory
            items.Add(AdminItem("workflow-settings", "Workflow Designer", "/admin/workflows", "AccountTree", "CanAccessWorkflows", "admin-workflows", 1));

            // Integrations with pluggable provider support
            var integrationsService = config.ExternalServices.GetValueOrDefault("integrations");
            if (integrationsService?.Enabled == true && !integrationsService.UseInternal)
            {
                items.Add(AdminItem("workflow-monitor", $"{integrationsService.DisplayName} Workflows", integrationsService.Url ?? "/admin/workflows/monitor", "Timeline", "CanAccessWorkflows", "admin-workflows", 2, isExternal: true, externalUrl: integrationsService.Url, providerType: integrationsService.ProviderType));
            }
            else
            {
                items.Add(AdminItem("workflow-monitor", "Workflow Monitor", "/admin/workflows/monitor", "Timeline", "CanAccessWorkflows", "admin-workflows", 2));
            }

            items.Add(AdminItem("integrations", "Integrations", "/admin/integrations", "Extension", "CanAccessSettings", "admin-workflows", 3));
            items.Add(AdminItem("analytics-settings", "Analytics Config", "/admin/analytics", "BarChart", "CanAccessSettings", "admin-workflows", 4));

            // Admin - Channels subcategory
            items.Add(AdminItem("channel-settings", "Channels", "/admin/channels", "Forum", "CanAccessSettings", "admin-channels", 1));

            return items;
        }

        private NavigationItemConfig NavItem(
            string id, string label, string path, string icon, string? menuName,
            string category, double order, string? requiredFeature = null,
            bool isExternal = false, string? externalUrl = null, string? providerType = null)
        {
            return new NavigationItemConfig
            {
                Id = id,
                Label = label,
                Path = path,
                Icon = icon,
                MenuName = menuName ?? $"CanAccess{id.Replace("-", "")}",
                Category = category,
                Order = order,
                Visible = true,
                Enabled = true,
                RequiredFeature = requiredFeature,
                IsExternal = isExternal,
                ExternalUrl = externalUrl,
                ProviderType = providerType
            };
        }

        private NavigationItemConfig AdminItem(
            string id, string label, string path, string icon, string menuName,
            string adminSubcategory, double order,
            bool isExternal = false, string? externalUrl = null, string? providerType = null)
        {
            return new NavigationItemConfig
            {
                Id = id,
                Label = label,
                Path = path,
                Icon = icon,
                MenuName = menuName,
                Category = "admin",
                AdminSubcategory = adminSubcategory,
                Order = order,
                Visible = true,
                Enabled = true,
                IsExternal = isExternal,
                ExternalUrl = externalUrl,
                ProviderType = providerType
            };
        }

        private async Task<Dictionary<string, bool>> GetFeatureFlagsAsync(CancellationToken cancellationToken)
        {
            var flags = new Dictionary<string, bool>();

            // Check all relevant feature flags
            var featuresToCheck = new[]
            {
                FeatureFlags.EnableITSM,
                FeatureFlags.EnableMarketing,
                FeatureFlags.EnableCustomerPortal,
                FeatureFlags.EnablePartnerPortal,
                FeatureFlags.EnableKnowledgeBase,
                FeatureFlags.UseExternalChat,
                FeatureFlags.UseExternalSearch,
                FeatureFlags.UseExternalNotifications,
                FeatureFlags.UseExternalAnalytics,
                FeatureFlags.UseExternalSignatures,
                FeatureFlags.UseExternalAI,
                FeatureFlags.UseExternalIntegrations
            };

            foreach (var feature in featuresToCheck)
            {
                try
                {
                    flags[feature] = await _featureManager.IsEnabledAsync(feature);
                }
                catch
                {
                    flags[feature] = false;
                }
            }

            return flags;
        }

        private List<NavigationCategoryConfig> GetCategories()
        {
            return new List<NavigationCategoryConfig>
            {
                new() { Id = "main", Label = "Main", Order = 1, Icon = "Home" },
                new() { Id = "sales", Label = "Sales", Order = 2, Icon = "TrendingUp" },
                new() { Id = "support", Label = "Support", Order = 3, Icon = "Support" },
                new() { Id = "itsm", Label = "ITSM", Order = 4, Icon = "Settings" },
                new() { Id = "marketing", Label = "Marketing", Order = 5, Icon = "Campaign" },
                new() { Id = "productivity", Label = "Productivity", Order = 6, Icon = "Task" },
                new() { Id = "info", Label = "Information", Order = 7, Icon = "Info" },
                new() { Id = "admin", Label = "Administration", Order = 8, Icon = "AdminPanel" }
            };
        }

        private List<NavigationSubcategoryConfig> GetAdminSubcategories()
        {
            return new List<NavigationSubcategoryConfig>
            {
                new() { Id = "admin-system", Label = "System", Icon = "Settings", Order = 1 },
                new() { Id = "admin-users", Label = "Users & Access", Icon = "People", Order = 2 },
                new() { Id = "admin-crm", Label = "CRM Settings", Icon = "Business", Order = 3 },
                new() { Id = "admin-service", Label = "Service Settings", Icon = "Support", Order = 4 },
                new() { Id = "admin-navigation", Label = "Navigation", Icon = "Menu", Order = 5 },
                new() { Id = "admin-modules", Label = "Modules", Icon = "ViewModule", Order = 6 },
                new() { Id = "admin-workflows", Label = "Workflows & Integrations", Icon = "AccountTree", Order = 7 },
                new() { Id = "admin-channels", Label = "Channels", Icon = "Forum", Order = 8 }
            };
        }

        private static string GetIntegrationDisplayName(string providerType) => providerType switch
        {
            "N8n" => "n8n",
            "Zapier" => "Zapier",
            "Make" => "Make",
            "Workato" => "Workato",
            _ => "Integrations"
        };

        private static string GetAnalyticsDisplayName(string providerType) => providerType switch
        {
            "Superset" => "Apache Superset",
            "Metabase" => "Metabase",
            "PowerBI" => "Power BI",
            "Looker" => "Looker",
            _ => "Analytics"
        };

        private static string GetAIDisplayName(string providerType) => providerType switch
        {
            "Ollama" => "Ollama (Local)",
            "OpenAI" => "OpenAI",
            "AzureOpenAI" => "Azure OpenAI",
            "Anthropic" => "Anthropic Claude",
            "Bedrock" => "AWS Bedrock",
            "Gemini" => "Google Gemini",
            "OpenRouter" => "OpenRouter",
            _ => "AI Assistant"
        };
    }
}

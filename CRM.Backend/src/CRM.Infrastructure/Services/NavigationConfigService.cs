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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Features;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace CRM.Infrastructure.Services
{
    /// <summary>
    /// Service that provides navigation configuration with pluggable architecture awareness and RBAC support.
    /// This runs at application startup to determine which navigation items should be shown
    /// based on feature flags, provider configuration, module availability, and user permissions.
    /// </summary>
    public class NavigationConfigService : INavigationConfigService
    {
        private readonly IFeatureManager _featureManager;
        private readonly IConfiguration _configuration;
        private readonly ICrmDbContext _context;
        private readonly ILogger<NavigationConfigService> _logger;

        // Cache for performance
        private NavigationConfig? _cachedConfig;
        private DateTime _cacheExpiry = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        // Permission cache per user
        private readonly Dictionary<int, (UserNavigationPermissions Permissions, DateTime Expiry)> _userPermissionCache = new();
        private readonly TimeSpan _userPermissionCacheDuration = TimeSpan.FromMinutes(2);

        public NavigationConfigService(
            IFeatureManager featureManager,
            IConfiguration configuration,
            ICrmDbContext context,
            ILogger<NavigationConfigService> logger)
        {
            _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _context = context ?? throw new ArgumentNullException(nameof(context));
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
            items.Add(NavItem("dashboard", "Dashboard", "/dashboard", "Dashboard", "Dashboard", "main", 1));
            items.Add(NavItem("accounts", "Accounts", "/accounts", "Business", "Accounts", "main", 2));
            items.Add(NavItem("account-overview", "Account 360°", "/account-overview", "Preview", "Accounts", "main", 3));
            items.Add(NavItem("contacts", "Contacts", "/contacts", "ContactPage", "Contacts", "main", 4));

            // Sales category
            items.Add(NavItem("leads", "Leads", "/leads", "PersonSearch", "Leads", "sales", 1));
            items.Add(NavItem("opportunities", "Opportunities", "/opportunities", "TrendingUp", "Opportunities", "sales", 2));
            items.Add(NavItem("quotes", "Quotes", "/quotes", "RequestQuote", "Quotes", "sales", 3));
            items.Add(NavItem("orders", "Orders", "/orders", "ShoppingCart", "Quotes", "sales", 4));
            items.Add(NavItem("invoices", "Invoices", "/invoices", "Receipt", "Quotes", "sales", 5));
            items.Add(NavItem("payments", "Payments", "/payments", "Payment", "Quotes", "sales", 6));
            items.Add(NavItem("contracts", "Contracts", "/contracts", "Description", "Quotes", "sales", 7));
            items.Add(NavItem("subscriptions", "Subscriptions", "/subscriptions", "Subscriptions", "Quotes", "sales", 8));
            items.Add(NavItem("products", "Products", "/products", "Inventory", "Products", "sales", 9));
            items.Add(NavItem("commissions", "Commissions", "/commissions", "AttachMoney", "Reports", "sales", 10));
            items.Add(NavItem("teams", "Teams", "/teams", "Groups", "UserManagement", "sales", 11));
            items.Add(NavItem("territories", "Territories", "/territories", "Map", "Settings", "sales", 12));

            // Support category
            items.Add(NavItem("service-requests", "Service Requests", "/service-requests", "SupportAgent", "ServiceRequests", "support", 1));
            items.Add(NavItem("knowledge-base", "Knowledge Base", "/knowledge-base", "MenuBook", "ServiceRequests", "support", 2));
            items.Add(NavItem("services", "Services", "/services", "Build", "Services", "support", 3));

            // ITSM category (conditionally shown based on EnableITSM feature flag)
            var itsmEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.EnableITSM);
            if (itsmEnabled)
            {
                items.Add(NavItem("itsm-overview", "ITSM Overview", "/itsm", "Speed", "ITSM", "itsm", 1, requiredFeature: FeatureFlags.EnableITSM));
                items.Add(NavItem("itsm-metrics", "Metrics", "/itsm/metrics", "BarChart", "ITSM", "itsm", 2, requiredFeature: FeatureFlags.EnableITSM));
                items.Add(NavItem("itsm-incidents", "Incidents", "/itsm/incidents", "Warning", "ITSM", "itsm", 3, requiredFeature: FeatureFlags.EnableITSM));
                items.Add(NavItem("itsm-problems", "Problems", "/itsm/problems", "BugReport", "ITSM", "itsm", 4, requiredFeature: FeatureFlags.EnableITSM));
                items.Add(NavItem("itsm-changes", "Changes", "/itsm/changes", "ChangeCircle", "ITSM", "itsm", 5, requiredFeature: FeatureFlags.EnableITSM));
                items.Add(NavItem("itsm-cmdb", "CMDB", "/itsm/cmdb", "Storage", "ITSM", "itsm", 6, requiredFeature: FeatureFlags.EnableITSM));
                items.Add(NavItem("itsm-knowledge", "KB Articles", "/itsm/knowledge", "Article", "ITSM", "itsm", 7, requiredFeature: FeatureFlags.EnableITSM));
                items.Add(NavItem("itsm-catalog", "Service Catalog", "/itsm/catalog", "Category", "ITSM", "itsm", 8, requiredFeature: FeatureFlags.EnableITSM));
                items.Add(NavItem("itsm-sla", "SLA Dashboard", "/itsm/sla", "Schedule", "ITSM", "itsm", 9, requiredFeature: FeatureFlags.EnableITSM));
            }

            // Marketing category (conditionally shown based on EnableMarketing feature flag)
            var marketingEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.EnableMarketing);
            if (marketingEnabled)
            {
                items.Add(NavItem("campaigns", "Campaigns", "/campaigns", "Campaign", "Campaigns", "marketing", 1, requiredFeature: FeatureFlags.EnableMarketing));
                items.Add(NavItem("email-templates", "Email Templates", "/email-templates", "Email", "Campaigns", "marketing", 2, requiredFeature: FeatureFlags.EnableMarketing));
                items.Add(NavItem("campaign-execution", "Campaign Execution", "/campaign-execution", "PlayCircle", "Campaigns", "marketing", 3, requiredFeature: FeatureFlags.EnableMarketing));
                items.Add(NavItem("landing-pages", "Landing Pages", "/landing-pages", "Web", "Campaigns", "marketing", 4, requiredFeature: FeatureFlags.EnableMarketing));
                items.Add(NavItem("lead-routing", "Lead Routing", "/lead-routing", "Route", "Leads", "marketing", 5, requiredFeature: FeatureFlags.EnableMarketing));
            }

            // Productivity category
            items.Add(NavItem("my-queue", "My Queue", "/my-queue", "Queue", "Activities", "productivity", 1));
            items.Add(NavItem("activities", "Activities", "/activities", "Event", "Activities", "productivity", 2));
            items.Add(NavItem("tasks", "Tasks", "/tasks", "CheckCircle", "Tasks", "productivity", 3));
            items.Add(NavItem("notes", "Notes", "/notes", "Note", "Notes", "productivity", 4));
            items.Add(NavItem("communications", "Communications", "/communications", "Chat", "Activities", "productivity", 5));
            items.Add(NavItem("interactions", "Interactions", "/interactions", "Forum", "Activities", "productivity", 6));
            items.Add(NavItem("approvals", "Approvals", "/approvals", "ThumbUp", "Workflows", "productivity", 7));
            items.Add(NavItem("relationships", "Relationships", "/relationships", "Share", "Accounts", "productivity", 8));

            // Info category
            items.Add(NavItem("reports", "Reports", "/reports", "Assessment", "Reports", "info", 1));

            // Analytics with pluggable provider support
            var analyticsService = config.ExternalServices.GetValueOrDefault("analytics");
            if (analyticsService?.Enabled == true && !analyticsService.UseInternal)
            {
                items.Add(NavItem("analytics", analyticsService.DisplayName, analyticsService.Url ?? "/analytics", "Insights", "Reports", "info", 2, isExternal: true, externalUrl: analyticsService.Url, providerType: analyticsService.ProviderType));
            }
            else
            {
                items.Add(NavItem("analytics", "Analytics", "/analytics", "Insights", "Reports", "info", 2));
            }

            items.Add(NavItem("about", "About", "/about", "Info", null, "info", 3));
            items.Add(NavItem("help", "Help", "/help", "Help", null, "info", 4));
            items.Add(NavItem("api-documentation", "API Docs", "/help/api", "Api", null, "info", 5));
            items.Add(NavItem("licenses", "Licenses", "/licenses", "Gavel", null, "info", 6));

            // === ADMIN NAVIGATION ITEMS ===

            // Admin - System subcategory
            items.Add(AdminItem("monitoring-settings", "Monitoring", "/admin/monitoring", "MonitorHeart", "Settings", "admin-system", 1));
            items.Add(AdminItem("deployment-settings", "Deployment", "/admin/deployment", "Cloud", "Settings", "admin-system", 2));
            items.Add(AdminItem("security-settings", "Security", "/admin/security", "Security", "Settings", "admin-system", 3));
            items.Add(AdminItem("feature-management", "Feature Flags", "/admin/features", "Flag", "Settings", "admin-system", 4));
            items.Add(AdminItem("database-settings", "Database", "/admin/database-settings", "Storage", "Settings", "admin-system", 5));
            items.Add(AdminItem("test-results", "Test Results", "/admin/test-results", "Science", "Settings", "admin-system", 6));
            items.Add(AdminItem("llm-settings", "AI/LLM Settings", "/admin/llm", "Psychology", "Settings", "admin-system", 7));

            // Admin - Users subcategory
            items.Add(AdminItem("user-management", "Users", "/admin/users", "People", "UserManagement", "admin-users", 1));
            items.Add(AdminItem("user-approvals", "User Approvals", "/admin/approvals", "HowToReg", "UserManagement", "admin-users", 2));
            items.Add(AdminItem("group-management", "Groups", "/admin/groups", "GroupWork", "UserManagement", "admin-users", 3));
            items.Add(AdminItem("social-login", "Social Login", "/admin/social-login", "Login", "Settings", "admin-users", 4));

            // Admin - CRM subcategory
            items.Add(AdminItem("branding-settings", "Branding", "/admin/branding", "Palette", "Settings", "admin-crm", 1));
            items.Add(AdminItem("master-data", "Master Data", "/admin/master-data", "Dataset", "Settings", "admin-crm", 2));
            items.Add(AdminItem("duplicate-rules", "Duplicate Rules", "/admin/duplicate-rules", "ContentCopy", "Settings", "admin-crm", 3));
            items.Add(AdminItem("lead-score-rules", "Lead Scoring", "/admin/lead-score-rules", "Score", "Settings", "admin-crm", 4));

            // Admin - Service subcategory
            items.Add(AdminItem("sr-definitions", "SR Definitions", "/admin/service-requests", "Assignment", "Settings", "admin-service", 1));
            items.Add(AdminItem("dashboard-settings", "Dashboards", "/admin/dashboards", "Dashboard", "Settings", "admin-service", 2));

            // Admin - Navigation subcategory
            items.Add(AdminItem("navigation-settings", "Navigation", "/admin/navigation", "Menu", "Settings", "admin-navigation", 1));

            // Admin - Modules subcategory
            items.Add(AdminItem("module-fields", "Module Fields", "/admin/modules", "ViewModule", "Settings", "admin-modules", 1));

            // Admin - Workflows subcategory
            items.Add(AdminItem("workflow-settings", "Workflow Designer", "/admin/workflows", "AccountTree", "Workflows", "admin-workflows", 1));

            // Integrations with pluggable provider support
            var integrationsService = config.ExternalServices.GetValueOrDefault("integrations");
            if (integrationsService?.Enabled == true && !integrationsService.UseInternal)
            {
                items.Add(AdminItem("workflow-monitor", $"{integrationsService.DisplayName} Workflows", integrationsService.Url ?? "/admin/workflows/monitor", "Timeline", "Workflows", "admin-workflows", 2, isExternal: true, externalUrl: integrationsService.Url, providerType: integrationsService.ProviderType));
            }
            else
            {
                items.Add(AdminItem("workflow-monitor", "Workflow Monitor", "/admin/workflows/monitor", "Timeline", "Workflows", "admin-workflows", 2));
            }

            items.Add(AdminItem("integrations", "Integrations", "/admin/integrations", "Extension", "Settings", "admin-workflows", 3));
            items.Add(AdminItem("analytics-settings", "Analytics Config", "/admin/analytics", "BarChart", "Settings", "admin-workflows", 4));

            // Admin - Channels subcategory
            items.Add(AdminItem("channel-settings", "Channels", "/admin/channels", "Forum", "Settings", "admin-channels", 1));

            return items;
        }

        private NavigationItemConfig NavItem(
            string id, string label, string path, string icon, string? requiredPermission,
            string category, double order, string? requiredFeature = null,
            bool isExternal = false, string? externalUrl = null, string? providerType = null,
            bool requiresSystemAdmin = false)
        {
            return new NavigationItemConfig
            {
                Id = id,
                Label = label,
                Path = path,
                Icon = icon,
                MenuName = requiredPermission != null ? $"CanAccess{requiredPermission}" : string.Empty,
                RequiredPermission = requiredPermission,
                Category = category,
                Order = order,
                Visible = true,
                Enabled = true,
                RequiredFeature = requiredFeature,
                IsExternal = isExternal,
                ExternalUrl = externalUrl,
                ProviderType = providerType,
                RequiresSystemAdmin = requiresSystemAdmin,
                IsAdminItem = false
            };
        }

        private NavigationItemConfig AdminItem(
            string id, string label, string path, string icon, string requiredPermission,
            string adminSubcategory, double order,
            bool isExternal = false, string? externalUrl = null, string? providerType = null,
            bool requiresSystemAdmin = false)
        {
            return new NavigationItemConfig
            {
                Id = id,
                Label = label,
                Path = path,
                Icon = icon,
                MenuName = $"CanAccess{requiredPermission}",
                RequiredPermission = requiredPermission,
                Category = "admin",
                AdminSubcategory = adminSubcategory,
                Order = order,
                Visible = true,
                Enabled = true,
                IsExternal = isExternal,
                ExternalUrl = externalUrl,
                ProviderType = providerType,
                RequiresSystemAdmin = requiresSystemAdmin,
                IsAdminItem = true
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

        // ========================================
        // RBAC (Role-Based Access Control) Methods
        // ========================================

        /// <inheritdoc />
        public async Task<NavigationConfig> GetNavigationConfigForUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            // Get the base configuration
            var baseConfig = await GetNavigationConfigAsync(cancellationToken);

            // Get the user's permissions
            var userPermissions = await GetUserPermissionsAsync(userId, cancellationToken);

            // Filter nav items based on user permissions
            var filteredItems = FilterNavItemsForUser(baseConfig.NavItems, userPermissions);

            return new NavigationConfig
            {
                Categories = baseConfig.Categories,
                AdminSubcategories = baseConfig.AdminSubcategories,
                NavItems = filteredItems,
                FeatureFlags = baseConfig.FeatureFlags,
                ExternalServices = baseConfig.ExternalServices,
                ProviderStatus = baseConfig.ProviderStatus
            };
        }

        /// <inheritdoc />
        public async Task<IEnumerable<NavigationItemConfig>> GetAvailableNavItemsForUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var config = await GetNavigationConfigForUserAsync(userId, cancellationToken);
            return config.NavItems.Where(n => n.Visible && n.Enabled);
        }

        /// <inheritdoc />
        public async Task<UserNavigationPermissions> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default)
        {
            // Check cache first
            if (_userPermissionCache.TryGetValue(userId, out var cached) &&
                DateTime.UtcNow < cached.Expiry)
            {
                return cached.Permissions;
            }

            // Query user's group memberships and aggregate permissions
            var userGroups = await _context.UserGroupMembers
                .Where(ugm => ugm.UserId == userId && !ugm.IsDeleted)
                .Include(ugm => ugm.UserGroup)
                .Select(ugm => ugm.UserGroup)
                .Where(g => g != null && g.IsActive && !g.IsDeleted)
                .ToListAsync(cancellationToken);

            var permissions = new UserNavigationPermissions
            {
                UserId = userId,
                IsSystemAdmin = userGroups.Any(g => g!.IsSystemAdmin),
                GroupIds = userGroups.Select(g => g!.Id).ToList(),
                GroupNames = userGroups.Select(g => g!.Name).ToList(),
                DataAccessScope = DetermineDataAccessScope(userGroups!),
                MenuAccess = AggregateMenuPermissions(userGroups!),
                EntityPermissions = AggregateEntityPermissions(userGroups!),
                BulkOperations = AggregateBulkOperations(userGroups!)
            };

            // Cache for 2 minutes
            _userPermissionCache[userId] = (permissions, DateTime.UtcNow.AddMinutes(2));

            return permissions;
        }

        /// <inheritdoc />
        public async Task<Dictionary<string, ModuleConfig>> GetModuleConfigsAsync(CancellationToken cancellationToken = default)
        {
            var modules = new Dictionary<string, ModuleConfig>();

            // Core modules (always available)
            modules["Dashboard"] = new ModuleConfig { Name = "Dashboard", DisplayName = "Dashboard", Enabled = true, Visible = true };
            modules["Accounts"] = new ModuleConfig { Name = "Accounts", DisplayName = "Accounts", Enabled = true, Visible = true };
            modules["Contacts"] = new ModuleConfig { Name = "Contacts", DisplayName = "Contacts", Enabled = true, Visible = true };
            modules["Leads"] = new ModuleConfig { Name = "Leads", DisplayName = "Leads", Enabled = true, Visible = true };
            modules["Opportunities"] = new ModuleConfig { Name = "Opportunities", DisplayName = "Opportunities", Enabled = true, Visible = true };
            modules["Products"] = new ModuleConfig { Name = "Products", DisplayName = "Products", Enabled = true, Visible = true };
            modules["Quotes"] = new ModuleConfig { Name = "Quotes", DisplayName = "Quotes", Enabled = true, Visible = true };
            modules["Tasks"] = new ModuleConfig { Name = "Tasks", DisplayName = "Tasks", Enabled = true, Visible = true };
            modules["Activities"] = new ModuleConfig { Name = "Activities", DisplayName = "Activities", Enabled = true, Visible = true };
            modules["Notes"] = new ModuleConfig { Name = "Notes", DisplayName = "Notes", Enabled = true, Visible = true };

            // Feature-flag dependent modules
            var itsmEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.EnableITSM);
            modules["ITSM"] = new ModuleConfig
            {
                Name = "ITSM",
                DisplayName = "ITSM",
                Enabled = itsmEnabled,
                Visible = itsmEnabled,
                FeatureFlag = FeatureFlags.EnableITSM
            };

            var marketingEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.EnableMarketing);
            modules["Campaigns"] = new ModuleConfig
            {
                Name = "Campaigns",
                DisplayName = "Marketing Campaigns",
                Enabled = marketingEnabled,
                Visible = marketingEnabled,
                FeatureFlag = FeatureFlags.EnableMarketing
            };

            var kbEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.EnableKnowledgeBase);
            modules["KnowledgeBase"] = new ModuleConfig
            {
                Name = "KnowledgeBase",
                DisplayName = "Knowledge Base",
                Enabled = kbEnabled,
                Visible = kbEnabled,
                FeatureFlag = FeatureFlags.EnableKnowledgeBase
            };

            return modules;
        }

        /// <inheritdoc />
        public void InvalidateCache()
        {
            _cachedConfig = null;
            _cacheExpiry = DateTime.MinValue;
            _userPermissionCache.Clear();
        }

        // ========================================
        // RBAC Helper Methods
        // ========================================

        private List<NavigationItemConfig> FilterNavItemsForUser(List<NavigationItemConfig> items, UserNavigationPermissions permissions)
        {
            var filtered = new List<NavigationItemConfig>();

            foreach (var item in items)
            {
                // System admins see everything
                if (permissions.IsSystemAdmin)
                {
                    filtered.Add(item);
                    continue;
                }

                // Check if item requires system admin
                if (item.RequiresSystemAdmin)
                {
                    continue;
                }

                // Admin items require specific admin permission
                if (item.IsAdminItem)
                {
                    // Check Settings or UserManagement access for admin items
                    if (!permissions.MenuAccess.Settings && !permissions.MenuAccess.UserManagement)
                    {
                        continue;
                    }
                }

                // If no permission required, item is visible
                if (string.IsNullOrEmpty(item.RequiredPermission))
                {
                    filtered.Add(item);
                    continue;
                }

                // Check specific menu permission
                if (HasMenuPermission(item.RequiredPermission, permissions.MenuAccess))
                {
                    filtered.Add(item);
                }
            }

            return filtered;
        }

        private bool HasMenuPermission(string permission, MenuAccessPermissions menuAccess)
        {
            return permission switch
            {
                "Dashboard" => menuAccess.Dashboard,
                "Accounts" => menuAccess.Accounts,
                "Contacts" => menuAccess.Contacts,
                "Leads" => menuAccess.Leads,
                "Opportunities" => menuAccess.Opportunities,
                "Products" => menuAccess.Products,
                "Services" => menuAccess.Services,
                "Campaigns" => menuAccess.Campaigns,
                "Quotes" => menuAccess.Quotes,
                "Tasks" => menuAccess.Tasks,
                "Activities" => menuAccess.Activities,
                "Notes" => menuAccess.Notes,
                "Workflows" => menuAccess.Workflows,
                "ServiceRequests" => menuAccess.ServiceRequests,
                "ITSM" => menuAccess.ITSM,
                "Reports" => menuAccess.Reports,
                "Settings" => menuAccess.Settings,
                "UserManagement" => menuAccess.UserManagement,
                _ => false
            };
        }

        private string DetermineDataAccessScope(IEnumerable<UserGroup> groups)
        {
            // Use most permissive scope: all > team > own
            var scopes = groups
                .Where(g => !string.IsNullOrEmpty(g.DataAccessScope))
                .Select(g => g.DataAccessScope!)
                .Distinct()
                .ToList();

            if (scopes.Contains("all", StringComparer.OrdinalIgnoreCase))
                return "all";
            if (scopes.Contains("team", StringComparer.OrdinalIgnoreCase))
                return "team";

            return "own";
        }

        private MenuAccessPermissions AggregateMenuPermissions(IEnumerable<UserGroup> groups)
        {
            // Any group granting access = access granted (OR logic)
            return new MenuAccessPermissions
            {
                Dashboard = groups.Any(g => g.CanAccessDashboard),
                Accounts = groups.Any(g => g.CanAccessAccounts),
                Contacts = groups.Any(g => g.CanAccessContacts),
                Leads = groups.Any(g => g.CanAccessLeads),
                Opportunities = groups.Any(g => g.CanAccessOpportunities),
                Products = groups.Any(g => g.CanAccessProducts),
                Services = groups.Any(g => g.CanAccessServices),
                Campaigns = groups.Any(g => g.CanAccessCampaigns),
                Quotes = groups.Any(g => g.CanAccessQuotes),
                Tasks = groups.Any(g => g.CanAccessTasks),
                Activities = groups.Any(g => g.CanAccessActivities),
                Notes = groups.Any(g => g.CanAccessNotes),
                Workflows = groups.Any(g => g.CanAccessWorkflows),
                ServiceRequests = groups.Any(g => g.CanAccessServiceRequests),
                ITSM = groups.Any(g => g.CanAccessITSM),
                Reports = groups.Any(g => g.CanAccessReports),
                Settings = groups.Any(g => g.CanAccessSettings),
                UserManagement = groups.Any(g => g.CanAccessUserManagement)
            };
        }

        private Dictionary<string, EntityCrudPermissions> AggregateEntityPermissions(IEnumerable<UserGroup> groups)
        {
            // Aggregate CRUD permissions for each entity type
            var entities = new Dictionary<string, EntityCrudPermissions>
            {
                ["Accounts"] = new EntityCrudPermissions
                {
                    CanCreate = groups.Any(g => g.CanCreateAccounts),
                    CanRead = groups.Any(g => g.CanAccessAccounts),
                    CanUpdate = groups.Any(g => g.CanEditAccounts),
                    CanDelete = groups.Any(g => g.CanDeleteAccounts),
                    CanViewAll = groups.Any(g => g.CanViewAllAccounts)
                },
                ["Contacts"] = new EntityCrudPermissions
                {
                    CanCreate = groups.Any(g => g.CanCreateContacts),
                    CanRead = groups.Any(g => g.CanAccessContacts),
                    CanUpdate = groups.Any(g => g.CanEditContacts),
                    CanDelete = groups.Any(g => g.CanDeleteContacts),
                    CanViewAll = groups.Any(g => g.DataAccessScope == "all")
                },
                ["Leads"] = new EntityCrudPermissions
                {
                    CanCreate = groups.Any(g => g.CanCreateLeads),
                    CanRead = groups.Any(g => g.CanAccessLeads),
                    CanUpdate = groups.Any(g => g.CanEditLeads),
                    CanDelete = groups.Any(g => g.CanDeleteLeads),
                    CanViewAll = groups.Any(g => g.DataAccessScope == "all")
                },
                ["Opportunities"] = new EntityCrudPermissions
                {
                    CanCreate = groups.Any(g => g.CanCreateOpportunities),
                    CanRead = groups.Any(g => g.CanAccessOpportunities),
                    CanUpdate = groups.Any(g => g.CanEditOpportunities),
                    CanDelete = groups.Any(g => g.CanDeleteOpportunities),
                    CanViewAll = groups.Any(g => g.DataAccessScope == "all")
                },
                ["Products"] = new EntityCrudPermissions
                {
                    CanCreate = groups.Any(g => g.CanCreateProducts),
                    CanRead = groups.Any(g => g.CanAccessProducts),
                    CanUpdate = groups.Any(g => g.CanEditProducts),
                    CanDelete = groups.Any(g => g.CanDeleteProducts),
                    CanViewAll = groups.Any(g => g.DataAccessScope == "all")
                },
                ["Campaigns"] = new EntityCrudPermissions
                {
                    CanCreate = groups.Any(g => g.CanCreateCampaigns),
                    CanRead = groups.Any(g => g.CanAccessCampaigns),
                    CanUpdate = groups.Any(g => g.CanEditCampaigns),
                    CanDelete = groups.Any(g => g.CanDeleteCampaigns),
                    CanViewAll = groups.Any(g => g.DataAccessScope == "all")
                },
                ["Quotes"] = new EntityCrudPermissions
                {
                    CanCreate = groups.Any(g => g.CanCreateQuotes),
                    CanRead = groups.Any(g => g.CanAccessQuotes),
                    CanUpdate = groups.Any(g => g.CanEditQuotes),
                    CanDelete = groups.Any(g => g.CanDeleteQuotes),
                    CanViewAll = groups.Any(g => g.DataAccessScope == "all")
                },
                ["Tasks"] = new EntityCrudPermissions
                {
                    CanCreate = groups.Any(g => g.CanCreateTasks),
                    CanRead = groups.Any(g => g.CanAccessTasks),
                    CanUpdate = groups.Any(g => g.CanEditTasks),
                    CanDelete = groups.Any(g => g.CanDeleteTasks),
                    CanViewAll = groups.Any(g => g.DataAccessScope == "all")
                },
                ["Workflows"] = new EntityCrudPermissions
                {
                    CanCreate = groups.Any(g => g.CanCreateWorkflows),
                    CanRead = groups.Any(g => g.CanAccessWorkflows),
                    CanUpdate = groups.Any(g => g.CanEditWorkflows),
                    CanDelete = groups.Any(g => g.CanDeleteWorkflows),
                    CanViewAll = groups.Any(g => g.DataAccessScope == "all")
                }
            };

            return entities;
        }

        private BulkOperationPermissions AggregateBulkOperations(IEnumerable<UserGroup> groups)
        {
            return new BulkOperationPermissions
            {
                CanExport = groups.Any(g => g.CanExportData),
                CanImport = groups.Any(g => g.CanImportData),
                CanBulkEdit = groups.Any(g => g.CanBulkEdit),
                CanBulkDelete = groups.Any(g => g.CanBulkDelete)
            };
        }
    }
}

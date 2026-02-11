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

using CRM.Core.Interfaces;
using CRM.Core.Interfaces.AI;
using CRM.Core.Ports.Input;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories;
using CRM.Infrastructure.Services;
using CRM.Infrastructure.Services.AI;
using CRM.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;
using CRM.Infrastructure.DependencyInjection;
using AspNetCoreRateLimit;
using Serilog;
using System.Text;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for HTTPS
var sslCertPath = builder.Configuration["SSL_CERT_PATH"] ?? Path.Combine(Directory.GetCurrentDirectory(), "ssl", "server.pfx");
// SECURITY: SSL_CERT_PASSWORD must be set in production - see SECURITY_BEST_PRACTICES.md
var sslCertPassword = builder.Configuration["SSL_CERT_PASSWORD"]
    ?? throw new InvalidOperationException("SSL_CERT_PASSWORD environment variable is required for HTTPS. Set it or use HTTP-only mode.");
var httpsPort = int.TryParse(builder.Configuration["HTTPS_PORT"], out var hp) ? hp : 5001;
var httpPort = int.TryParse(builder.Configuration["HTTP_PORT"], out var p) ? p : 5000;

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Always listen on HTTP
    serverOptions.ListenAnyIP(httpPort);

    // Try to enable HTTPS if certificate exists
    if (File.Exists(sslCertPath))
    {
        try
        {
            var cert = new X509Certificate2(sslCertPath, sslCertPassword);
            serverOptions.ListenAnyIP(httpsPort, listenOptions =>
            {
                listenOptions.UseHttps(cert);
            });
            Console.WriteLine($"HTTPS enabled on port {httpsPort} with certificate: {cert.Subject}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not load SSL certificate from {sslCertPath}: {ex.Message}");
            Console.WriteLine("HTTPS will not be available. Server running on HTTP only.");
        }
    }
    else
    {
        Console.WriteLine($"SSL certificate not found at {sslCertPath}. Server running on HTTP only (port {httpPort}).");
        Console.WriteLine("To enable HTTPS, upload a certificate via Admin Settings or place server.pfx in the ssl folder.");
    }
});

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add caching services
builder.Services.AddMemoryCache();

// Add Feature Management
// Enables deployment-time provider selection via FeatureManagement configuration
// See: docs/architecture/ADR-001-Pluggable-Architecture-Strategy.md Section 17
builder.Services.AddFeatureManagement(builder.Configuration.GetSection("FeatureManagement"));
Log.Information("Feature Management configured - Provider selection flags loaded");

// Add Pluggable Providers
// Registers all provider factories and enables runtime provider selection
// See: docs/architecture/ADR-001-Pluggable-Architecture-Strategy.md Section 17
builder.Services.AddPluggableProviders(builder.Configuration);
Log.Information("Pluggable Providers configured - Factory pattern enabled for provider resolution");

// Configure Redis cache
var redisConfig = builder.Configuration.GetSection("Redis");
builder.Services.Configure<RedisCacheOptions>(redisConfig);

var redisEnabled = redisConfig.GetValue("Enabled", true);
var redisConnectionString = redisConfig.GetValue<string>("ConnectionString") ?? "localhost:6379";
var redisInstanceName = redisConfig.GetValue<string>("InstanceName") ?? "crm_";

if (redisEnabled && !string.IsNullOrEmpty(redisConnectionString))
{
    Log.Information("Configuring Redis cache at {ConnectionString}", redisConnectionString);
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = redisInstanceName;
    });
    builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
}
else
{
    Log.Information("Redis disabled, using in-memory distributed cache");
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
}

// Add database caching service
builder.Services.AddScoped<IDbCacheService, DbCacheService>();

// Configure monitoring service
var monitoringConfig = builder.Configuration.GetSection("Monitoring");
builder.Services.Configure<MonitoringOptions>(monitoringConfig);
builder.Services.AddScoped<IMonitoringService, MonitoringService>();
Log.Information("Monitoring configured - DeploymentType: {Type}, BuildServer: {Server}",
    monitoringConfig.GetValue<string>("DeploymentType", "docker"),
    monitoringConfig.GetValue<string>("BuildServer", "localhost"));

// Add rate limiting - configurable via appsettings.json
var isDevelopment = builder.Environment.IsDevelopment();
var rateLimitingConfig = builder.Configuration.GetSection("RateLimiting");

builder.Services.Configure<IpRateLimitOptions>(options =>
{
    // Read base settings from config with defaults
    options.EnableEndpointRateLimiting = rateLimitingConfig.GetValue("EnableEndpointRateLimiting", !isDevelopment);
    options.StackBlockedRequests = rateLimitingConfig.GetValue("StackBlockedRequests", false);
    options.RealIpHeader = rateLimitingConfig.GetValue("RealIpHeader", "X-Real-IP");
    options.ClientIdHeader = rateLimitingConfig.GetValue("ClientIdHeader", "X-ClientId");
    options.HttpStatusCode = rateLimitingConfig.GetValue("HttpStatusCode", 429);
    options.QuotaExceededResponse = new QuotaExceededResponse
    {
        Content = rateLimitingConfig.GetValue("QuotaExceededMessage", "API calls quota exceeded!"),
        ContentType = "text/plain"
    };

    // Build rules list from configuration
    var rules = new List<RateLimitRule>();

    // Add general rules from config
    var generalRulesSection = rateLimitingConfig.GetSection("GeneralRules");
    if (generalRulesSection.Exists())
    {
        foreach (var ruleSection in generalRulesSection.GetChildren())
        {
            rules.Add(new RateLimitRule
            {
                Endpoint = ruleSection.GetValue("Endpoint", "*"),
                Period = ruleSection.GetValue("Period", "1m"),
                Limit = ruleSection.GetValue("Limit", 1000)
            });
        }
    }
    else
    {
        // Default general rule if not configured
        rules.Add(new RateLimitRule { Endpoint = "*", Period = "1m", Limit = 1000 });
    }

    // Add endpoint-specific rules from config
    var endpointRulesSection = rateLimitingConfig.GetSection("EndpointRules");
    if (endpointRulesSection.Exists())
    {
        foreach (var endpointSection in endpointRulesSection.GetChildren())
        {
            var endpoint = endpointSection.Key;
            rules.Add(new RateLimitRule
            {
                Endpoint = $"*:{endpoint}*",
                Period = endpointSection.GetValue("Period", "1m"),
                Limit = endpointSection.GetValue("Limit", 100)
            });
        }
    }

    options.GeneralRules = rules;
});
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CRM Solution API",
        Version = "v2.0.0",
        Description = "Enterprise CRM Solution with Pluggable Architecture. Supports Accounts, Contacts, Leads, Opportunities, Products, Campaigns, Service Desk, ITSM, and AI/Analytics modules.",
        Contact = new OpenApiContact
        {
            Name = "CRM Solution Team",
            Email = "support@crm.local"
        },
        License = new OpenApiLicense
        {
            Name = "AGPL-3.0",
            Url = new Uri("https://www.gnu.org/licenses/agpl-3.0.html")
        }
    });

    // JWT Bearer authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments for API documentation
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Add SignalR for real-time notifications
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Register SignalR notification service
builder.Services.AddSingleton<CRM.Api.Hubs.ICrmNotificationService, CRM.Api.Hubs.CrmNotificationService>();

// Add CORS - dynamic origin handling based on deployment
var configuredOrigins = builder.Configuration["AllowedOrigins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? Array.Empty<string>();

// Get frontend port for dynamic origin building
var frontendPort = builder.Configuration["FRONTEND_EXTERNAL_PORT"] ?? "3000";

// Determine if running in production
var isProduction = builder.Environment.IsProduction();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (isProduction && configuredOrigins.Length > 0)
        {
            // PRODUCTION: Strict whitelist - only explicitly configured origins allowed
            policy.WithOrigins(configuredOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            // DEVELOPMENT/STAGING: Allow configured origins + local development
            policy.SetIsOriginAllowed(origin =>
            {
                // Always allow configured origins
                if (configuredOrigins.Any(allowed =>
                    string.Equals(allowed, origin, StringComparison.OrdinalIgnoreCase)))
                    return true;

                // Parse the origin URL
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var originUri))
                    return false;

                var host = originUri.Host;

                // Allow localhost and 127.0.0.1 (development)
                if (host == "localhost" || host == "127.0.0.1")
                    return true;

                // Allow local network IPs (192.168.x.x, 10.x.x.x, 172.16-31.x.x) - ONLY in non-production
                if (System.Net.IPAddress.TryParse(host, out var ip))
                {
                    var bytes = ip.GetAddressBytes();
                    if (bytes.Length == 4)
                    {
                        // 192.168.x.x
                        if (bytes[0] == 192 && bytes[1] == 168)
                            return true;
                        // 10.x.x.x
                        if (bytes[0] == 10)
                            return true;
                        // 172.16.x.x - 172.31.x.x
                        if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                            return true;
                    }
                }

                return false;
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        }
    });
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure Database
var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "mariadb";
// Build connection string from configuration or environment variables
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString) && (databaseProvider.ToLower() == "mysql" || databaseProvider.ToLower() == "mariadb"))
{
    var dbHost = builder.Configuration["DB_HOST"] ?? builder.Configuration["DbHost"] ?? "mariadb";
    var dbPort = builder.Configuration["DB_PORT"] ?? "3306";
    var dbName = builder.Configuration["DB_NAME"] ?? "crm_db";
    var dbUser = builder.Configuration["DB_USER"] ?? "crm_user";
    // SECURITY: DB_PASSWORD must be set in production - see SECURITY_BEST_PRACTICES.md
    var dbPass = builder.Configuration["DB_PASSWORD"] ?? builder.Configuration["DB_PASS"]
        ?? (builder.Environment.IsDevelopment() ? "crm_pass" : throw new InvalidOperationException("DB_PASSWORD environment variable is required in production"));
    connectionString = $"Server={dbHost};Port={dbPort};Database={dbName};Uid={dbUser};Pwd={dbPass};";
}

builder.Services.AddDbContext<CrmDbContext>(options =>
{
    switch (databaseProvider.ToLower())
    {
        case "postgresql":
            options.UseNpgsql(connectionString);
            break;
        case "oracle":
            options.UseOracle(connectionString);
            break;
        case "mysql":
        case "mariadb":
            // Use explicit MariaDB version to avoid connection attempts during startup
            options.UseMySql(connectionString, new MariaDbServerVersion(new Version(11, 0, 0)));
            break;
        case "sqlserver":
            options.UseSqlServer(connectionString);
            break;
        case "sqlite":
        default:
            options.UseSqlite(connectionString ?? "Data Source=crm.db");
            break;
    }
});

// Register ICrmDbContext interface with dynamic resolution
builder.Services.AddScoped<IDbContextResolver, DynamicDbContextResolver>();
builder.Services.AddScoped<ICrmDbContext>(provider =>
    provider.GetRequiredService<IDbContextResolver>().ResolveContext());

// Register Services (backward compatibility)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IOpportunityService, OpportunityService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IMarketingCampaignService, MarketingCampaignService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ITotpService, TotpService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserGroupService, UserGroupService>();
builder.Services.AddScoped<ISystemSettingsService, SystemSettingsService>();
builder.Services.AddScoped<IUserApprovalService, UserApprovalService>();
builder.Services.AddScoped<IDatabaseBackupService, DatabaseBackupService>();
builder.Services.AddHostedService<BackupSchedulerHostedService>();
builder.Services.AddScoped<IContactsService, ContactsService>();
builder.Services.AddScoped<IContactInfoService, ContactInfoService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();
builder.Services.AddScoped<IServiceRequestCategoryService, ServiceRequestCategoryService>();
builder.Services.AddScoped<IServiceRequestSubcategoryService, ServiceRequestSubcategoryService>();
builder.Services.AddScoped<IServiceRequestCustomFieldService, ServiceRequestCustomFieldService>();
builder.Services.AddScoped<IServiceRequestTypeService, ServiceRequestTypeService>();
builder.Services.AddScoped<IColorPaletteService, ColorPaletteService>();

// ITSM Services - IT Service Management (Incident, Problem, Change, CMDB, Knowledge, SLA)
builder.Services.AddScoped<CRM.Infrastructure.Services.ITSM.IBusinessHoursCalculator, CRM.Infrastructure.Services.ITSM.BusinessHoursCalculator>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IIncidentService, CRM.Infrastructure.Services.ITSM.IncidentService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IProblemService, CRM.Infrastructure.Services.ITSM.ProblemService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.ICMDBService, CRM.Infrastructure.Services.ITSM.CMDBService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IChangeManagementService, CRM.Infrastructure.Services.ITSM.ChangeManagementService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IKnowledgeManagementService, CRM.Infrastructure.Services.ITSM.KnowledgeManagementService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IServiceCatalogService, CRM.Infrastructure.Services.ITSM.ServiceCatalogService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.ISLAService, CRM.Infrastructure.Services.ITSM.SLAService>();
// ITSM Phase 4 - Advanced Automation & Integration Services
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IWebhookNotificationService, CRM.Infrastructure.Services.ITSM.WebhookNotificationService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IEmailToTicketService, CRM.Infrastructure.Services.ITSM.EmailToTicketService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IITSMDashboardService, CRM.Infrastructure.Services.ITSM.ITSMDashboardService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IMonitoringIntegrationService, CRM.Infrastructure.Services.ITSM.MonitoringIntegrationService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.ICICDIntegrationService, CRM.Infrastructure.Services.ITSM.CICDIntegrationService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.ISelfServiceChatbotService, CRM.Infrastructure.Services.ITSM.SelfServiceChatbotService>();
#if ITSM_ADVANCED
// SLA Enforcement Background Service - runs continuously to monitor and enforce SLAs
builder.Services.AddHostedService<CRM.Infrastructure.Services.ITSM.SLAEnforcementHostedService>();
// Auto-close resolved items background service (auto-closes incidents, service requests, changes, problems)
builder.Services.AddHostedService<CRM.Infrastructure.Services.ITSM.AutoCloseHostedService>();
// Escalation background service (auto-escalates incidents/service requests based on SLA thresholds)
builder.Services.AddHostedService<CRM.Infrastructure.Services.ITSM.EscalationHostedService>();
#endif
builder.Services.AddHttpClient<IColorPaletteService, ColorPaletteService>();
builder.Services.AddScoped<ModuleFieldConfigurationService>();
builder.Services.AddScoped<ModuleUIConfigService>();
builder.Services.AddScoped<SampleDataSeederService>();
// Database Sync BVT Service - runs on startup to ensure db consistency
builder.Services.AddSingleton<IDatabaseSyncService, DatabaseSyncService>();
builder.Services.AddHostedService<DatabaseSyncHostedService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IAccountService, CRM.Infrastructure.Services.AccountService>();
// Normalization helper for tags/custom fields
builder.Services.AddScoped<NormalizationService>();
// Master data - ZIP code / Postal code lookups with caching
builder.Services.AddScoped<ZipCodeService>();
builder.Services.AddScoped<IZipCodeService>(sp =>
{
    var innerService = sp.GetRequiredService<ZipCodeService>();
    var cache = sp.GetRequiredService<IMemoryCache>();
    var logger = sp.GetRequiredService<ILogger<CachedZipCodeService>>();
    return new CachedZipCodeService(innerService, cache, logger);
});
// ZIP code import service - pull from GeoNames/GitHub
builder.Services.AddScoped<IZipCodeImportService, ZipCodeImportService>();
// ZIP code import options
builder.Services.Configure<ZipCodeImportOptions>(
    builder.Configuration.GetSection(ZipCodeImportOptions.SectionName));
// ZIP code import background service (scheduled imports)
builder.Services.AddHostedService<ZipCodeImportHostedService>();
// Contact info validation service (email, phone, social media)
builder.Services.AddScoped<IContactInfoValidationService, ContactInfoValidationService>();
// Phase 1 services - Notes, Tasks, Quotes (Gap Fix Implementation)
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
// Phase 2 services - Dashboard, Pipeline, Interaction (Gap Fix Implementation)
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPipelineService, PipelineService>();
builder.Services.AddScoped<IInteractionService, InteractionService>();
// Phase 3 services - Communication, ImportExport, Webhook (Gap Fix Implementation)
builder.Services.AddScoped<ICommunicationService, CommunicationService>();
builder.Services.AddScoped<IImportExportService, ImportExportService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
// Phase 2B services - Lead Management, Form Builder, Territory Management, Approval Workflows
builder.Services.AddScoped<ILeadService, LeadService>();
builder.Services.AddScoped<ILeadRoutingService, LeadRoutingService>();
builder.Services.AddScoped<IFormBuilderService, FormBuilderService>();
builder.Services.AddScoped<ITerritoryService, TerritoryService>();
builder.Services.AddScoped<IApprovalWorkflowService, ApprovalWorkflowService>();
// Phase 4 services - Invoice, Payment, Order, Contract, Subscription, Team, Commission, EmailTemplate
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ICommissionService, CommissionService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
// Phase 5 services - Department, SalesQuota, SalesForecast, Conversation, EventAttendee (Missing Entity Services)
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ISalesQuotaService, SalesQuotaService>();
builder.Services.AddScoped<ISalesForecastService, SalesForecastService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IEventAttendeeService, EventAttendeeService>();
// Email Sequence service (drip campaigns)
builder.Services.AddScoped<CRM.Core.Interfaces.IEmailSequenceService, CRM.Infrastructure.Services.EmailSequenceService>();
// Pricing & Bundles
builder.Services.AddScoped<CRM.Core.Interfaces.IPricingService, CRM.Infrastructure.Services.PricingService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IProductBundleService, CRM.Infrastructure.Services.ProductBundleService>();
// Credit Memo service
builder.Services.AddScoped<ICreditMemoService, CreditMemoService>();
// Master data - Field-to-master-data linking service
builder.Services.AddScoped<IFieldMasterDataService, FieldMasterDataService>();
// Master data seeder - seeds ZipCodes and ColorPalettes on startup if empty
builder.Services.AddScoped<IMasterDataSeederService, MasterDataSeederService>();
// Cloud Deployment management service
builder.Services.AddScoped<ICloudDeploymentService, CloudDeploymentService>();
builder.Services.AddHttpClient();

// Workflow management services
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<WorkflowService>(); // Also register concrete type for CampaignExecutionService dependency
builder.Services.AddScoped<IWorkflowInstanceService, WorkflowInstanceService>();
builder.Services.AddScoped<WorkflowInstanceService>(); // Also register concrete type for CampaignExecutionService dependency
builder.Services.AddScoped<IHttpCalloutService, HttpCalloutService>();

// Relationship management services
builder.Services.AddScoped<RelationshipService>();

// Duplicate detection and merge services
builder.Services.AddScoped<IDuplicateDetectionService, DuplicateDetectionService>();
builder.Services.AddScoped<IMergeService, MergeService>();

// Campaign execution services
builder.Services.AddScoped<CampaignExecutionService>();

// LLM and Resilience services
builder.Services.Configure<LLMProviderOptions>(builder.Configuration.GetSection("LLMProviders"));
builder.Services.Configure<ResilienceOptions>(builder.Configuration.GetSection("Resilience"));
builder.Services.AddHttpClient<ILLMService, LLMService>();
builder.Services.AddSingleton<IResilienceService, ResilienceService>();
builder.Services.AddScoped<ILLMSettingsService, LLMSettingsService>();

// Phase 7 services - AI/Analytics Enhancements (KB search, Lead scoring, Opportunity scoring, Dashboards, Reports)
builder.Services.AddScoped<IAIKnowledgeSearchService, AIKnowledgeSearchService>();
builder.Services.AddScoped<IAILeadScoringService, AILeadScoringService>();
builder.Services.AddScoped<IAIOpportunityScoringService, AIOpportunityScoringService>();
builder.Services.AddScoped<IDashboardBuilderService, DashboardBuilderService>();
builder.Services.AddScoped<IReportBuilderService, ReportBuilderService>();
builder.Services.AddScoped<IAIPredictiveAnalyticsService, AIPredictiveAnalyticsService>();

// Allen AI Services (OLMo/Tulu models for lead scoring, insights, churn prediction)
builder.Services.Configure<AllenAIConfiguration>(builder.Configuration.GetSection("AllenAI"));
builder.Services.AddHttpClient("AllenAI", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddScoped<IAllenAIService, AllenAIService>();

// News and Social Media Feed service (NewsAPI, Twitter, LinkedIn integration)
builder.Services.Configure<NewsSocialOptions>(builder.Configuration.GetSection("NewsSocial"));
builder.Services.AddHttpClient<INewsSocialService, NewsSocialService>();

// Workflow background worker
var workflowWorkerOptions = new WorkflowWorkerOptions
{
    MaxConcurrentTasks = builder.Configuration.GetValue<int>("Workflow:MaxConcurrentTasks", 5),
    PollIntervalSeconds = builder.Configuration.GetValue<int>("Workflow:PollIntervalSeconds", 5),
    LockDurationMinutes = builder.Configuration.GetValue<int>("Workflow:LockDurationMinutes", 15),
    EnableLLMActions = builder.Configuration.GetValue<bool>("Workflow:EnableLLMActions", true)
};
builder.Services.AddSingleton(workflowWorkerOptions);
builder.Services.AddHostedService<WorkflowWorkerService>();

// Workflow Trigger Service + Scheduled Workflow Background Service
builder.Services.AddScoped<IWorkflowTriggerService, WorkflowTriggerService>();
builder.Services.AddSingleton<IEntityEventDispatcher, EntityEventDispatcher>();
builder.Services.AddHostedService<ScheduledWorkflowService>();

// Workflow Log Retention Service - purges old log entries based on level-specific retention periods
builder.Services.AddHostedService<WorkflowLogRetentionService>();

// Lead Score Decay Background Service - applies inactivity decay to lead scores
builder.Services.AddLeadScoreDecayService();

// Calendar Sync Service - OAuth2 integration with Google/Outlook calendars (G4)
builder.Services.Configure<CalendarSyncOptions>(builder.Configuration.GetSection(CalendarSyncOptions.SectionName));
builder.Services.AddScoped<ICalendarSyncService, CalendarSyncService>();
builder.Services.AddHostedService<CalendarSyncHostedService>();

// Email Sync Service - IMAP/OAuth sync for unified inbox (G5)
builder.Services.Configure<EmailSyncOptions>(builder.Configuration.GetSection(EmailSyncOptions.SectionName));
builder.Services.AddScoped<IEmailSyncService, EmailSyncService>();
builder.Services.AddHostedService<EmailSyncHostedService>();

// Landing Page Service - Visual landing page builder (G6)
builder.Services.AddScoped<ILandingPageService, LandingPageService>();

// HEXAGONAL ARCHITECTURE - Register Input Ports (Primary/Driving Ports)
// These allow controllers to depend on ports instead of concrete services
builder.Services.AddScoped<IAccountInputPort, AccountService>();
builder.Services.AddScoped<IContactInputPort, ContactsService>();
builder.Services.AddScoped<IOpportunityInputPort, OpportunityService>();
builder.Services.AddScoped<IProductInputPort, ProductService>();
builder.Services.AddScoped<ICampaignInputPort, MarketingCampaignService>();
builder.Services.AddScoped<IAuthInputPort, AuthenticationService>();
builder.Services.AddScoped<IUserInputPort, UserService>();
builder.Services.AddScoped<IUserGroupInputPort, UserGroupService>();
builder.Services.AddScoped<ISystemSettingsInputPort, SystemSettingsService>();
builder.Services.AddScoped<IServiceRequestInputPort, ServiceRequestService>();
builder.Services.AddScoped<IAccountInputPort, AccountService>();
builder.Services.AddScoped<IDatabaseBackupInputPort, DatabaseBackupService>();

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Length < 32)
{
    // Use a secure default for development only - in production, this should be configured
    if (builder.Environment.IsDevelopment())
    {
        jwtSecret = "development-only-jwt-secret-key-minimum-32-chars";
        Log.Warning("Using development JWT secret. Configure Jwt:Secret for production.");
    }
    else
    {
        throw new InvalidOperationException("JWT Secret must be configured in production. Set 'Jwt:Secret' with a secure key at least 32 characters long.");
    }
}
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CRMApp";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CRMUsers";
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
.AddJwtBearer("Bearer", options =>
{
    // Require HTTPS in production, allow HTTP in development
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = ctx =>
        {
            Log.Warning(ctx.Exception, "JWT authentication failed");
            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// Apply migrations and seed data automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
    try
    {
        // Check if database exists and has tables
        var canConnect = await db.Database.CanConnectAsync();

        // For non-SQLite databases, use EnsureCreated for dev environment to avoid migration issues
        if (databaseProvider.ToLower() != "sqlite")
        {
            try
            {
                Log.Information($"Creating schema for {databaseProvider} database using EnsureCreated...");
                await db.Database.EnsureCreatedAsync();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not create database schema using EnsureCreated");
                throw;
            }
        }
        else if (canConnect)
        {
            // Try to apply migrations if they haven't been applied
            try
            {
                var pending = await db.Database.GetPendingMigrationsAsync();
                if (pending.Any())
                {
                    Log.Information($"Applying {pending.Count()} pending migrations...");
                    await db.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not apply migrations, trying EnsureCreated...");
                // If migration fails, try creating tables from model
                await db.Database.EnsureCreatedAsync();
            }
        }
        else
        {
            Log.Information("Creating new database...");
            await db.Database.EnsureCreatedAsync();
        }

        // Apply any raw SQL migration files in CRM.Backend/migrations (useful for MySQL/MariaDB)
        try
        {
            var migrationsFolder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "migrations"));
            if (Directory.Exists(migrationsFolder))
            {
                var sqlFiles = Directory.GetFiles(migrationsFolder, "*.sql").OrderBy(f => f);
                foreach (var file in sqlFiles)
                {
                    try
                    {
                        var sql = await File.ReadAllTextAsync(file);
                        if (!string.IsNullOrWhiteSpace(sql))
                        {
                            Log.Information($"Executing SQL migration file: {Path.GetFileName(file)}");
                            await db.Database.ExecuteSqlRawAsync(sql);
                        }
                    }
                    catch (Exception innerEx)
                    {
                        Log.Warning(innerEx, $"Failed to execute SQL file {file} - continuing");
                    }
                }
            }
        }
        catch (Exception exSql)
        {
            Log.Warning(exSql, "Error while applying raw SQL migration files");
        }

        // Seed data
        await DbSeed.SeedAsync(db);
        Log.Information("Database setup completed successfully");

        // Seed master data (ZipCodes, ColorPalettes) if not already populated
        // This data persists across deployments in the database
        try
        {
            var masterDataSeeder = scope.ServiceProvider.GetRequiredService<IMasterDataSeederService>();
            await masterDataSeeder.SeedIfEmptyAsync();
            var stats = await masterDataSeeder.GetStatsAsync();
            Log.Information("Master data status: {ZipCodeCount} ZIP codes, {ColorPaletteCount} color palettes",
                stats.ZipCodeCount, stats.ColorPaletteCount);
        }
        catch (Exception masterDataEx)
        {
            Log.Warning(masterDataEx, "Failed to seed master data - continuing without");
        }

        // Auto-seed sample data if configured
        var autoSeedSampleData = builder.Configuration.GetValue<bool>("SampleData:AutoSeed", false);
        if (autoSeedSampleData)
        {
            Log.Information("Auto-seeding sample data...");
            try
            {
                var sampleSeeder = scope.ServiceProvider.GetRequiredService<SampleDataSeederService>();

                // Check if already seeded
                var isSeeded = await sampleSeeder.IsSampleDataSeededAsync();

                if (!isSeeded)
                {
                    Log.Information("Seeding production database with sample data...");
                    await sampleSeeder.SeedAllSampleDataAsync();
                    Log.Information("Sample data seeded successfully");
                }
                else
                {
                    Log.Information("Sample data already seeded");
                }
            }
            catch (Exception sampleDataEx)
            {
                Log.Warning(sampleDataEx, "Failed to auto-seed sample data - continuing without");
            }
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during database setup - continuing anyway");
        // Continue anyway - the app can still run with partial setup
    }
}

// Configure Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM Solution API v2.0.0");
        c.DocumentTitle = "CRM Solution API Documentation";
    });
}

// Only use HTTPS redirect if we have SSL enabled and not in development
// Skip redirect for health endpoints to allow Kubernetes health checks on HTTP
var forceHttpsRedirect = builder.Configuration.GetValue<bool>("ForceHttpsRedirect", false);
if (forceHttpsRedirect && File.Exists(sslCertPath))
{
    app.UseWhen(context => !context.Request.Path.StartsWithSegments("/health"), appBuilder =>
    {
        appBuilder.UseHttpsRedirection();
    });
}

// Add security headers to all responses
app.UseSecurityHeaders();

// Serve static files from wwwroot (for uploaded files)
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
app.UseStaticFiles(); // Serve from wwwroot

// Serve static files from frontend build
var frontendBuildPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "CRM.Frontend", "build");
if (Directory.Exists(frontendBuildPath))
{
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = new PhysicalFileProvider(Path.GetFullPath(frontendBuildPath)) });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(Path.GetFullPath(frontendBuildPath)) });
}

app.UseRouting();
// Use the default CORS policy globally
app.UseCors();
// Apply rate limiting before authentication
app.UseIpRateLimiting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR hubs for real-time notifications
app.MapHub<CRM.Api.Hubs.CrmNotificationHub>("/hubs/notifications");

// SPA fallback - serve index.html for unmatched routes (only if frontend build exists)
if (Directory.Exists(frontendBuildPath))
{
    app.MapFallback(context =>
    {
        context.Response.ContentType = "text/html";
        return context.Response.SendFileAsync(Path.Combine(frontendBuildPath, "index.html"));
    });
}

app.Run();

public partial class Program { }


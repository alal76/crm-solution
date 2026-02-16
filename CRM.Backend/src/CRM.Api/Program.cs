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
using CRM.Core.Options;
using CRM.Core.Ports.Input;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories;
using CRM.Infrastructure.Services;
using CRM.Infrastructure.Services.AI;
using CRM.Infrastructure.Services.Authentication;
using CRM.Infrastructure.Services.Authentication.OAuth;
using CRM.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi;
using CRM.Infrastructure.DependencyInjection;
using Serilog;
using System.Text;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for HTTPS
var sslCertPath = builder.Configuration["SSL_CERT_PATH"] ?? Path.Combine(Directory.GetCurrentDirectory(), "ssl", "server.pfx");
// SECURITY: SSL_CERT_PASSWORD must be set via environment variable - see SECURITY_BEST_PRACTICES.md
// If not provided, the server runs HTTP-only (no HTTPS). Never hardcode certificate passwords.
var sslCertPassword = builder.Configuration["SSL_CERT_PASSWORD"] ?? "";
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

// Add HttpContextAccessor for services that need current user context
builder.Services.AddHttpContextAccessor();

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

// Configure options from appsettings.json - Phase 1 through Phase 4 Authentication
builder.Services.Configure<CRM.Core.Options.LinkedInOAuthOptions>(builder.Configuration.GetSection("Phase1:LinkedIn"));
builder.Services.Configure<CRM.Core.Options.AppleOAuthOptions>(builder.Configuration.GetSection("Phase1:Apple"));
builder.Services.Configure<CRM.Core.Options.SmsOtpSettings>(builder.Configuration.GetSection("Phase1:SmsOtp"));
builder.Services.Configure<CRM.Core.Options.EmailOtpSettings>(builder.Configuration.GetSection("Phase1:EmailOtp"));
builder.Services.Configure<CRM.Core.Options.TotpOptions>(builder.Configuration.GetSection("Phase2:Totp"));
builder.Services.Configure<CRM.Core.Options.WebAuthnOptions>(builder.Configuration.GetSection("Phase3:WebAuthn"));
builder.Services.Configure<CRM.Core.Options.GoogleOAuthOptions>(builder.Configuration.GetSection("Phase4:GoogleOAuth"));
builder.Services.Configure<CRM.Core.Options.MicrosoftOAuthOptions>(builder.Configuration.GetSection("Phase4:MicrosoftOAuth"));
builder.Services.Configure<CRM.Core.Options.GitHubOAuthOptions>(builder.Configuration.GetSection("Phase4:GitHubOAuth"));
Log.Information("Authentication options configured for all phases");

// Register Authentication Services
builder.Services.AddScoped<CRM.Core.Interfaces.ITotpService, CRM.Infrastructure.Services.Authentication.TotpService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IWebAuthnService, CRM.Infrastructure.Services.Authentication.WebAuthnService>();
builder.Services.AddScoped<IGoogleOAuthProvider, GoogleOAuthProvider>();
builder.Services.AddScoped<IMicrosoftOAuthProvider, MicrosoftOAuthProvider>();
builder.Services.AddScoped<IGitHubOAuthProvider, GitHubOAuthProvider>();
Log.Information("Authentication services registered - TOTP, WebAuthn, OAuth providers");

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
    
    // Register IConnectionMultiplexer for services that require direct Redis access (like RBACService)
    builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
    {
        var configOptions = StackExchange.Redis.ConfigurationOptions.Parse(redisConnectionString);
        return StackExchange.Redis.ConnectionMultiplexer.Connect(configOptions);
    });
    
    builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
}
else
{
    Log.Information("Redis disabled, using in-memory distributed cache");
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
}

builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(2),
        LocalCacheExpiration = TimeSpan.FromSeconds(30)
    };
});

// Add database caching service
builder.Services.AddScoped<IDbCacheService, DbCacheService>();

// Storage Redis configuration status for conditional service registration
builder.Services.AddSingleton(sp => new { IsRedisEnabled = redisEnabled });

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
var rateLimitingEnabled = rateLimitingConfig.GetValue("EnableEndpointRateLimiting", !isDevelopment);
var rejectionStatusCode = rateLimitingConfig.GetValue("HttpStatusCode", 429);
var quotaMessage = rateLimitingConfig.GetValue("QuotaExceededMessage", "API calls quota exceeded!");

TimeSpan ParseRateLimitPeriod(string period)
{
    if (string.IsNullOrWhiteSpace(period))
    {
        return TimeSpan.FromMinutes(1);
    }

    var normalized = period.Trim().ToLowerInvariant();
    if (normalized.EndsWith("ms") && int.TryParse(normalized[..^2], out var ms))
    {
        return TimeSpan.FromMilliseconds(ms);
    }
    if (normalized.EndsWith("s") && int.TryParse(normalized[..^1], out var seconds))
    {
        return TimeSpan.FromSeconds(seconds);
    }
    if (normalized.EndsWith("m") && int.TryParse(normalized[..^1], out var minutes))
    {
        return TimeSpan.FromMinutes(minutes);
    }
    if (normalized.EndsWith("h") && int.TryParse(normalized[..^1], out var hours))
    {
        return TimeSpan.FromHours(hours);
    }

    return TimeSpan.FromMinutes(1);
}

var generalRuleSection = rateLimitingConfig.GetSection("GeneralRules").GetChildren().FirstOrDefault();
var generalPermitLimit = generalRuleSection?.GetValue("Limit", 1000) ?? 1000;
var generalWindow = ParseRateLimitPeriod(generalRuleSection?.GetValue("Period", "1m") ?? "1m");

var endpointRules = rateLimitingConfig.GetSection("EndpointRules")
    .GetChildren()
    .Select(section => new
    {
        Endpoint = section.Key,
        PermitLimit = section.GetValue("Limit", 100),
        Window = ParseRateLimitPeriod(section.GetValue("Period", "1m"))
    })
    .ToList();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = rejectionStatusCode;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "text/plain";
        await context.HttpContext.Response.WriteAsync(quotaMessage, token);
    };

    if (!rateLimitingEnabled)
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
            RateLimitPartition.GetNoLimiter("rate-limiting-disabled"));
        return;
    }

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var requestPath = context.Request.Path.Value ?? string.Empty;
        var endpointRule = endpointRules.FirstOrDefault(rule =>
            requestPath.Contains(rule.Endpoint, StringComparison.OrdinalIgnoreCase));

        var permitLimit = endpointRule?.PermitLimit ?? generalPermitLimit;
        var window = endpointRule?.Window ?? generalWindow;

        var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = window,
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });
});

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info = new OpenApiInfo
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
        };

        return Task.CompletedTask;
    });
    options.AddDocumentTransformer<CRM.Api.OpenApi.BearerSecuritySchemeTransformer>();
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
        case "inmemory":
            options.UseInMemoryDatabase("crm_test");
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
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IOpportunityService, OpportunityService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IMarketingCampaignService, MarketingCampaignService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITotpService, CRM.Infrastructure.Services.Authentication.TotpService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserGroupService, UserGroupService>();

// Phase 1: Social OAuth & OTP Providers
// LinkedIn OAuth provider for enterprise sign-in
builder.Services.Configure<CRM.Core.Options.LinkedInOAuthOptions>(builder.Configuration.GetSection("OAuth:LinkedIn"));
builder.Services.AddHttpClient<LinkedInOAuthProvider>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("User-Agent", "CRM-Solution/2.0");
    });
builder.Services.AddScoped<LinkedInOAuthProvider>();

// Apple OAuth provider for consumer & enterprise sign-in
builder.Services.Configure<CRM.Core.Options.AppleOAuthOptions>(builder.Configuration.GetSection("OAuth:Apple"));
builder.Services.AddHttpClient<AppleOAuthProvider>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("User-Agent", "CRM-Solution/2.0");
    });
builder.Services.AddScoped<AppleOAuthProvider>();

// SMS OTP service using Twilio
builder.Services.Configure<CRM.Core.Options.SmsOtpSettings>(builder.Configuration.GetSection("OTP:SMS"));
builder.Services.AddScoped<ISmsOtpService, SmsOtpService>();

// Email OTP service using SendGrid
builder.Services.Configure<CRM.Core.Options.EmailOtpSettings>(builder.Configuration.GetSection("OTP:Email"));
builder.Services.AddScoped<IEmailOtpService, EmailOtpService>();
builder.Services.AddScoped<ISystemSettingsService, SystemSettingsService>();
builder.Services.AddScoped<IBrandingConfigService, BrandingConfigService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IUserApprovalService, UserApprovalService>();
builder.Services.AddScoped<IDatabaseBackupService, DatabaseBackupService>();
// TEMPORARILY DISABLED - causes model building errors
// builder.Services.AddHostedService<BackupSchedulerHostedService>();
builder.Services.AddScoped<IContactsService, ContactsService>();
builder.Services.AddScoped<IContactInfoService, ContactInfoService>();
builder.Services.AddScoped<IPreferencesService, PreferencesService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();
builder.Services.AddScoped<IServiceRequestCategoryService, ServiceRequestCategoryService>();
builder.Services.AddScoped<IServiceRequestSubcategoryService, ServiceRequestSubcategoryService>();
builder.Services.AddScoped<IServiceRequestCustomFieldService, ServiceRequestCustomFieldService>();
builder.Services.AddScoped<IServiceRequestTypeService, ServiceRequestTypeService>();
builder.Services.AddScoped<IColorPaletteService, ColorPaletteService>();

// builder.Services.AddScoped<IAdminConfigurationService, AdminConfigurationService>(); // DISABLED for System Module isolation

// SYS-004: Feature Flag Management Service
builder.Services.AddScoped<IFeatureFlagManagementService, FeatureFlagManagementService>();

// SYS-010: User Interface Service  
builder.Services.AddScoped<IUserInterfaceService, UserInterfaceService>();

// SYS-011: Performance Optimization Service
builder.Services.AddScoped<IPerformanceOptimizationService, PerformanceOptimizationService>();

// SYS-002: RBAC and Permission Cache Services
// Role-Based Access Control (RBAC) with Redis-backed permission caching for optimal performance
// Only register PermissionCacheService if Redis is enabled (it requires IConnectionMultiplexer)
var redisEnabledForRBAC = builder.Configuration.GetSection("Redis").GetValue("Enabled", true);
if (redisEnabledForRBAC)
{
    builder.Services.AddScoped<IPermissionCacheService, PermissionCacheService>();
}
else
{
    // Register a no-op implementation when Redis is disabled to prevent DI errors
    builder.Services.AddScoped<IPermissionCacheService>(sp => new InMemoryPermissionCacheService());
}
builder.Services.AddScoped<IRBACService, RBACService>();

// builder.Services.AddScoped<IProviderHealthService, ProviderHealthService>(); // DISABLED for System Module isolation

// SYS-001: Admin Dashboard Service
// Already registered but listed here for clarity - depends on IProviderHealthService above
// builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>(); // Already at line 488

// SYS-006: Optional Audit Logging Service (conditional registration)
// Audit logging is disabled by default (opt-in via UseOptionalAuditLogging feature flag)
// When enabled, tracks all entity changes, deletions, and user actions for compliance/audit purposes
builder.Services.AddScoped<IOptionalAuditLoggingService, OptionalAuditLoggingService>();
Log.Information("Optional Audit Logging Service registered (enabled via UseOptionalAuditLogging feature flag)");

// ITSM Services - IT Service Management (Incident, Problem, Change, CMDB, Knowledge, SLA)
// PHASE 1: Core critical services re-enabled (Feb 16, 2026)
builder.Services.AddScoped<CRM.Infrastructure.Services.ITSM.IBusinessHoursCalculator, CRM.Infrastructure.Services.ITSM.BusinessHoursCalculator>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IIncidentService, CRM.Infrastructure.Services.ITSM.IncidentService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.ISLAService, CRM.Infrastructure.Services.ITSM.SLAService>();
Log.Information("ITSM Phase 1 Tier-1 Services registered: BusinessHoursCalculator, IncidentService, SLAService");

// PHASE 2-4: Additional ITSM services - pending implementation
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IProblemManagementService, CRM.Infrastructure.Services.ITSM.ProblemManagementService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IProblemService, CRM.Infrastructure.Services.ITSM.ProblemService>();
//builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.ICMDBService, CRM.Infrastructure.Services.ITSM.CMDBService>(); // NOT IMPLEMENTED
//builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IChangeManagementService, CRM.Infrastructure.Services.ITSM.ChangeManagementService>(); // Depends on ICMDBService
//builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IKnowledgeManagementService, CRM.Infrastructure.Services.ITSM.KnowledgeManagementService>();
//builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IServiceCatalogService, CRM.Infrastructure.Services.ITSM.ServiceCatalogService>();
// builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IEscalationRuleService, CRM.Infrastructure.Services.ITSM.EscalationRuleService>(); // DISABLED for System Module isolation
// builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IEscalationPolicyService, CRM.Infrastructure.Services.ITSM.EscalationPolicyService>(); // DISABLED for System Module isolation
// ITSM Phase 4 - Advanced Automation & Integration Services - DISABLED
//builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IWebhookNotificationService, CRM.Infrastructure.Services.ITSM.WebhookNotificationService>();
//builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IEmailToTicketService, CRM.Infrastructure.Services.ITSM.EmailToTicketService>();
//builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IITSMDashboardService, CRM.Infrastructure.Services.ITSM.ITSMDashboardService>();
//builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IMonitoringIntegrationService, CRM.Infrastructure.Services.ITSM.MonitoringIntegrationService>();
//builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.ICICDIntegrationService, CRM.Infrastructure.Services.ITSM.CICDIntegrationService>();
//builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.ISelfServiceChatbotService, CRM.Infrastructure.Services.ITSM.SelfServiceChatbotService>();
//#if ITSM_ADVANCED
// SLA Enforcement Background Service - runs continuously to monitor and enforce SLAs
// builder.Services.AddHostedService<CRM.Infrastructure.Services.ITSM.SLAEnforcementHostedService>(); // DISABLED for System Module isolation
// Auto-close resolved items background service (auto-closes incidents, service requests, changes, problems)
// builder.Services.AddHostedService<CRM.Infrastructure.Services.ITSM.AutoCloseHostedService>(); // DISABLED for System Module isolation
// Escalation background service (auto-escalates incidents/service requests based on SLA thresholds)
// builder.Services.AddHostedService<CRM.Infrastructure.Services.ITSM.EscalationHostedService>(); // DISABLED for System Module isolation
//#endif
builder.Services.AddHttpClient<IColorPaletteService, ColorPaletteService>();
builder.Services.AddScoped<ModuleFieldConfigurationService>();
builder.Services.AddScoped<ModuleUIConfigService>();
builder.Services.AddScoped<SampleDataSeederService>();
// Database Sync BVT Service - runs on startup to ensure db consistency
builder.Services.AddSingleton<IDatabaseSyncService, DatabaseSyncService>();
// TEMPORARILY DISABLED - causes model building errors
// builder.Services.AddHostedService<DatabaseSyncHostedService>();
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

// Admin Configuration Services for Sales and Service Desk Modules
// PHASE 2: Re-enabled from .disabled (Feb 16, 2026)
builder.Services.AddScoped<ICommissionRuleService, CommissionRuleService>();
builder.Services.AddScoped<IDiscountRuleService, DiscountRuleService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.ISLAPolicyAdminService, CRM.Infrastructure.Services.ITSM.SLAPolicyAdminService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IEscalationRuleAdminService, CRM.Infrastructure.Services.ITSM.EscalationRuleAdminService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IServiceQueueService, CRM.Infrastructure.Services.ITSM.ServiceQueueService>();
Log.Information("Admin Configuration Services registered: CommissionRule, DiscountRule, SLAPolicy, EscalationRule, ServiceQueue");

// Worker architecture services (queue, outbox, escalation processing, notifications)
builder.Services.AddScoped<CRM.Core.Interfaces.Workers.IWorkerQueue, CRM.Infrastructure.Workers.DbWorkerQueue>();
builder.Services.AddScoped<CRM.Core.Interfaces.Integration.IOutboxDispatcher, CRM.Infrastructure.Integration.OutboxDispatcher>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IEscalationProcessor, CRM.Infrastructure.Services.ITSM.EscalationProcessor>();
builder.Services.AddScoped<CRM.Core.Interfaces.Notifications.INotificationDispatcher, CRM.Infrastructure.Services.Notifications.NotificationDispatcher>();
// Phase 5 services - Department, SalesQuota, SalesForecast, Conversation, EventAttendee (Missing Entity Services)
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ISalesQuotaService, SalesQuotaService>();
builder.Services.AddScoped<ISalesForecastService, SalesForecastService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IEventAttendeeService, EventAttendeeService>();
// Email Sequence service (drip campaigns)
builder.Services.AddScoped<CRM.Core.Interfaces.IEmailSequenceService, CRM.Infrastructure.Services.EmailSequenceService>();

// SPRINT 1-2: TIER-1 CRITICAL SERVICES (Commission, Campaign, Email Sequence, Webhook Management)
// Commission Management Services (4 services)
builder.Services.AddScoped<ICommissionPlanService, CommissionPlanService>();
builder.Services.AddScoped<ICommissionCalculationService, CommissionCalculationService>();
builder.Services.AddScoped<ICommissionApprovalService, CommissionApprovalService>();
builder.Services.AddScoped<ICommissionPayoutService, CommissionPayoutService>();

// Campaign Management Services (3 services)
builder.Services.AddScoped<ICampaignRecipientService, CampaignRecipientService>();
builder.Services.AddScoped<ICampaignMetricsService, CampaignMetricsService>();
builder.Services.AddScoped<ICampaignExecutionService, CampaignExecutionService>();

// Email Sequence Management Service (enhanced)
builder.Services.AddScoped<IEmailSequenceManagementService, EmailSequenceManagementService>();

// Webhook Management Services (2 services)
builder.Services.AddScoped<IWebhookManagementService, WebhookManagementService>();
builder.Services.AddScoped<IWebhookDispatcherService, WebhookDispatcherService>();

// Pricing & Bundles
builder.Services.AddScoped<CRM.Core.Interfaces.IPricingService, CRM.Infrastructure.Services.PricingService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IProductBundleService, CRM.Infrastructure.Services.ProductBundleService>();
// Credit Memo service
builder.Services.AddScoped<ICreditMemoService, CreditMemoService>();
// Subscription Billing Services (SPEC-SALES-006)
// Recurring Billing Engine - background job for hourly subscription billing cycles
// builder.Services.AddScoped<IRecurringBillingEngine, RecurringBillingEngine>(); // DISABLED for System Module isolation
// Dunning Manager - payment failure recovery with 3-retry escalation
// builder.Services.AddScoped<IDunningManager, DunningManager>(); // DISABLED for System Module isolation
// Proration Calculator - 4 proration algorithms (ProRata, FullPrice, OneMonth, None)
// builder.Services.AddScoped<IProrateCalculator, ProrateCalculator>(); // DISABLED for System Module isolation
// Subscription Metrics Aggregator - MRR/ARR/churn/NRR calculations
// builder.Services.AddScoped<ISubscriptionMetricsAggregator, SubscriptionMetricsAggregator>(); // DISABLED for System Module isolation
// Master data - Field-to-master-data linking service
builder.Services.AddScoped<IFieldMasterDataService, FieldMasterDataService>();
// Master data seeder - seeds ZipCodes and ColorPalettes on startup if empty
builder.Services.AddScoped<IMasterDataSeederService, MasterDataSeederService>();
// Core data seeder - seeds departments, accounts, products, contacts, lookups, system settings (ADR-002)
builder.Services.AddScoped<ICoreDataSeederService, CoreDataSeederService>();
// Cloud Deployment management service
builder.Services.AddScoped<ICloudDeploymentService, CloudDeploymentService>();
builder.Services.AddHttpClient();

// Hangfire Background Job Processing (SPEC-SALES-006) - DISABLED FOR SYSTEM MODULE ISOLATION
// Hangfire provides reliable background job processing with retry logic,
// scheduling, and persistence - critical for recurring billing and dunning.
// TEMPORARILY DISABLED: Remove .disabled suffix from services when re-enabling
// var hangfireEnabled = builder.Configuration.GetValue<bool>("Hangfire:Enabled", true);
// if (hangfireEnabled)
// {
//     var hangfireConnectionString = builder.Configuration.GetConnectionString("HangfireConnection") 
//         ?? connectionString; // Fall back to main connection string
//     
//     Log.Information("Configuring Hangfire for background job processing");
//     
//     builder.Services.AddHangfire(config =>
//     {
//         // Use the same database provider as main app for consistency
//         switch (databaseProvider.ToLowerInvariant())
//         {
//             case "sqlserver":
//                 config.UseSqlServerStorage(hangfireConnectionString);
//                 break;
//             case "mysql":
//             case "mariadb":
//                 // MySqlStorage requires MySqlConnector.Core - for now fallback to SqlServer compatibility mode
//                 // In production, consider MariaDB-specific Hangfire storage
//                 config.UseSqlServerStorage(hangfireConnectionString);
//                 break;
//             case "postgresql":
//                 // PostgreSQL storage requires Hangfire.PostgreSql package
//                 config.UseSqlServerStorage(hangfireConnectionString);
//                 break;
//             default:
//                 // In-memory storage for SQLite/dev builds (jobs lost on restart)
//                 config.UseMemoryStorage();
//                 Log.Warning("Using in-memory Hangfire storage for {Provider} - jobs will be lost on restart", databaseProvider);
//                 break;
//         }
//         
//         config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
//         config.UseSerializerSettings(new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
//     });
//     
//     // Hangfire server - processes background jobs
//     builder.Services.AddHangfireServer(options =>
//     {
//         options.WorkerCount = builder.Configuration.GetValue<int>("Hangfire:WorkerCount", Environment.ProcessorCount);
//         options.Queues = new[] { "recurring-billing", "dunning", "default" };
//         options.SchedulePollingInterval = TimeSpan.FromSeconds(30); // Check for scheduled jobs every 30s
//     });
// }
// else
// {
//     Log.Warning("Hangfire disabled (Hangfire:Enabled=false) - background jobs will not be processed");
// }

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
builder.Services.AddScoped<IReportService, ReportService>();
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

// Navigation Configuration Service - dynamic navigation aware of pluggable architecture
builder.Services.AddScoped<INavigationConfigService, NavigationConfigService>();

// Workflow background worker
var workflowWorkerOptions = new WorkflowWorkerOptions
{
    MaxConcurrentTasks = builder.Configuration.GetValue<int>("Workflow:MaxConcurrentTasks", 5),
    PollIntervalSeconds = builder.Configuration.GetValue<int>("Workflow:PollIntervalSeconds", 5),
    LockDurationMinutes = builder.Configuration.GetValue<int>("Workflow:LockDurationMinutes", 15),
    EnableLLMActions = builder.Configuration.GetValue<bool>("Workflow:EnableLLMActions", true)
};
builder.Services.AddSingleton(workflowWorkerOptions);

// Conditionally add WorkflowWorkerService - can be disabled via SKIP_WORKFLOW_WORKER=true
var skipWorkflowWorker = builder.Configuration.GetValue<bool>("SKIP_WORKFLOW_WORKER", false);
if (!skipWorkflowWorker)
{
    builder.Services.AddHostedService<WorkflowWorkerService>();
}

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
// TEMPORARILY DISABLED - causes model building errors
// builder.Services.Configure<EmailSyncOptions>(builder.Configuration.GetSection(EmailSyncOptions.SectionName));
// builder.Services.AddScoped<IEmailSyncService, EmailSyncService>();
// builder.Services.AddHostedService<EmailSyncHostedService>();

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

// Add Authorization policies
// Default policy: Authenticated users only
builder.Services.AddAuthorization(options =>
{
    // Default policy requires authentication
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Configure Hangfire Dashboard and background job scheduling
// if (hangfireEnabled)
// {
//     Log.Information("Configuring Hangfire background job scheduling");
//     
//     // Hangfire Dashboard (admin-only, requires authentication)
//     app.UseHangfireDashboard("/hangfire", new DashboardOptions
//     {
//         Authorization = new[] { new HangfireAuthorizationFilter() },
//         IgnoreAntiforgeryToken = true, // SignalR/CORS friendly
//         DashboardTitle = "CRM Subscription Billing Dashboard"
//     });
//     
//     // Schedule recurring background jobs
//     using (var scope = app.Services.CreateScope())
//     {
//         var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
//         
//         // Recurring Billing Engine - process subscriptions due for billing
//         // Runs every hour at :00 (12 times per day)
//         recurringJobManager.AddOrUpdate(
//             "recurring-billing-engine",
//             () => scope.ServiceProvider.GetRequiredService<IRecurringBillingEngine>()
//                 .ProcessBillingCyclesAsync(CancellationToken.None),
//             Cron.Hourly(0), // Every hour at :00
//             new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
//         );
//         
//         // Dunning Manager - retry failed payments
//         // Runs twice daily at 2 AM and 2 PM UTC
//         recurringJobManager.AddOrUpdate(
//             "dunning-manager",
//             () => scope.ServiceProvider.GetRequiredService<IDunningManager>()
//                 .ProcessDunningAsync(CancellationToken.None),
//             Cron.Daily(2, 14), // 2 AM and 2 PM UTC
//             new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
//         );
//         
//         Log.Information("Hangfire background jobs scheduled successfully");
//     }
// }

// ADR-002: Unified EF Core Schema Management
// Supports EnsureCreated for fresh deployments and MigrateAsync for existing ones.
// Set SKIP_DB_MIGRATION=true to skip all migration/schema management.
// Set USE_ENSURE_CREATED=true to use EnsureCreated instead of MigrateAsync (for fresh DBs).
// Set RECREATE_DATABASE=true to drop and recreate the database completely (destructive!).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
    var skipMigration = Environment.GetEnvironmentVariable("SKIP_DB_MIGRATION") == "true";
    if (skipMigration)
    {
        Log.Information("SKIP_DB_MIGRATION=true — skipping EF Core schema management");
    }
    try
    {
        if (!skipMigration)
        {
            var useEnsureCreated = Environment.GetEnvironmentVariable("USE_ENSURE_CREATED") == "true";
            var recreateDatabase = Environment.GetEnvironmentVariable("RECREATE_DATABASE") == "true";

            if (!db.Database.IsRelational())
            {
                useEnsureCreated = true;
                Log.Information("Non-relational provider detected ({Provider}); using EnsureCreated instead of migrations", databaseProvider);
            }

            if (useEnsureCreated)
            {
                if (recreateDatabase)
                {
                    Log.Warning("RECREATE_DATABASE=true — dropping existing database for {Provider}...", databaseProvider);
                    await db.Database.EnsureDeletedAsync();
                    Log.Information("Database dropped successfully. Recreating...");
                }

                Log.Information("Creating schema for {Provider} database using EnsureCreated...", databaseProvider);
                var created = await db.Database.EnsureCreatedAsync();
                if (created)
                {
                    Log.Information("Database schema created successfully via EnsureCreated ({TableCount} entities in model)", db.Model.GetEntityTypes().Count());
                }
                else
                {
                    Log.Warning("EnsureCreated returned false — database already has tables. Schema was NOT created. Set RECREATE_DATABASE=true to force recreation.");
                }
            }
            else
            {
                Log.Information("Applying EF Core migrations for {Provider}...", databaseProvider);
                await db.Database.MigrateAsync();
                Log.Information("EF Core migrations applied successfully");
            }
        }

        // Seed essential data (SysAdmin group + admin user only)
        await DbSeed.SeedAsync(db);
        Log.Information("Database setup completed successfully");

        // Seed master data (ZipCodes, ColorPalettes) if not already populated
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
            try
            {
                var sampleSeeder = scope.ServiceProvider.GetRequiredService<SampleDataSeederService>();
                var isSeeded = await sampleSeeder.IsSampleDataSeededAsync();
                if (!isSeeded)
                {
                    Log.Information("Seeding sample data...");
                    await sampleSeeder.SeedAllSampleDataAsync();
                    Log.Information("Sample data seeded successfully");
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
    }
}

// Configure Middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/openapi/v1.json", "CRM Solution API v2.0.0");
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
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR hubs for real-time notifications
app.MapHub<CRM.Api.Hubs.CrmNotificationHub>("/hubs/notifications");
app.MapHub<CRM.Api.Hubs.AgentApprovalHub>("/hubs/agent-approvals");

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

/// <summary>
/// Partial class declaration for integration test factory configuration.
/// </summary>
public partial class Program
{
}

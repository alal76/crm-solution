// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.RateLimiting;
using CRM.Api.Infrastructure;
using CRM.Api.Middleware;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.AI;
using CRM.Core.Options;
using CRM.Core.Ports;
using CRM.Core.Ports.Input;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Data.Interceptors;
using CRM.Infrastructure.DependencyInjection;
using CRM.Infrastructure.Repositories;
using CRM.Infrastructure.Services;
using CRM.Infrastructure.AI.SK;
using CRM.Infrastructure.Services.AI;
using CRM.Infrastructure.Services.Authentication;
using CRM.Infrastructure.Services.Configuration;
using CRM.Infrastructure.Services.Authentication.OAuth;
using CRM.Infrastructure.Scripting;
using CRM.Infrastructure.Jobs;
using Hangfire;
using Hangfire.InMemory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Load appsettings.Local.json for per-developer overrides (gitignored — API keys, local ports, etc.)
// This intentionally comes after appsettings.Development.json so local values win.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Configure Kestrel for HTTPS
var sslCertPath = builder.Configuration["SSL_CERT_PATH"] ?? Path.Combine(Directory.GetCurrentDirectory(), "ssl", "server.pfx");
// SECURITY: SSL_CERT_PASSWORD must be set via environment variable - see SECURITY_BEST_PRACTICES.md
// If not provided, the server runs HTTP-only (no HTTPS). Never hardcode certificate passwords.
var sslCertPassword = builder.Configuration["SSL_CERT_PASSWORD"] ?? "";
var httpsPort = int.TryParse(builder.Configuration["HTTPS_PORT"], out var hp) ? hp : 5001;
var httpPort = int.TryParse(builder.Configuration["HTTP_PORT"], out var p) ? p : 5000;

var httpsEnabled = false;
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Always listen on HTTP
    serverOptions.ListenAnyIP(httpPort);

    // Try to enable HTTPS if certificate exists
    if (File.Exists(sslCertPath))
    {
        try
        {
#pragma warning disable SYSLIB0057
            var cert = new X509Certificate2(sslCertPath, sslCertPassword);
#pragma warning restore SYSLIB0057
            serverOptions.ListenAnyIP(httpsPort, listenOptions =>
            {
                listenOptions.UseHttps(cert);
            });
            httpsEnabled = true;
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

// Add scripting engines (Jint by default; extensible for Python/others)
builder.Services.AddScriptingEngines(builder.Configuration);
Log.Information("Scripting engines configured - ScriptEngineFactory ready");

// SARCH-028: Roslyn compiled scripting engine + registry lifecycle service
builder.Services.AddCrmScripting();
builder.Services.AddScoped<IScriptRegistryService, ScriptRegistryService>();
Log.Information("Roslyn scripting engine and script registry service registered");

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

// OAuth CSRF state validation & 2FA policy enforcement (TODO-AUTH-005, TODO-AUTH-011)
builder.Services.AddSingleton<CRM.Infrastructure.Services.Auth.IOAuthStateService, CRM.Infrastructure.Services.Auth.OAuthStateService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.Auth.ITwoFactorPolicyService, CRM.Infrastructure.Services.Auth.TwoFactorPolicyService>();

Log.Information("Authentication services registered - TOTP, WebAuthn, OAuth providers, OAuth state, 2FA policies");

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
        configOptions.AbortOnConnectFail = false; // Allow retries on connection failure
        configOptions.ConnectTimeout = 10000; // 10 second timeout
        configOptions.SyncTimeout = 10000;
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
// AP-038: Extracted monitoring sub-services (registered before MonitoringService)
builder.Services.AddScoped<IDatabaseHealthService, DatabaseHealthService>();
builder.Services.AddScoped<IDockerMonitoringService, DockerMonitoringService>();
builder.Services.AddScoped<IKubernetesMonitoringService, KubernetesMonitoringService>();
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

    // Always register the real limiter — actual enable/disable is controlled at
    // runtime via ISystemControlsService (admin toggle endpoint).
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        // Runtime override check — allows admin to toggle without restart
        var sysControls = context.RequestServices
            .GetService<CRM.Core.Interfaces.ISystemControlsService>();
        if (sysControls != null && !sysControls.GetRateLimitingStatus().Enabled)
        {
            return RateLimitPartition.GetNoLimiter("runtime-toggle-disabled");
        }

        // Startup disabled — honour original appsettings value
        if (!rateLimitingEnabled)
        {
            return RateLimitPartition.GetNoLimiter("rate-limiting-disabled");
        }

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
builder.Services.AddControllers(options =>
    {
        // Global exception filter: converts DuplicateExistsException → 409 Conflict
        options.Filters.Add<CRM.Api.Filters.DuplicateExistsExceptionFilter>();
    })
    .AddJsonOptions(options =>
    {
        // Safety net: prevent circular reference exceptions from navigation properties
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
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
                Name = "Source Available - Commercial License Required",
                Url = new Uri("https://github.com/alal76/crm-solution/blob/main/LICENSE")
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

// Register SLA SignalR notifier for real-time SLA countdown push
builder.Services.AddSingleton<CRM.Core.Interfaces.ITSM.ISLASignalRNotifier, CRM.Api.Hubs.SLASignalRNotifier>();

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

// AP-039: Database services (DbContext, ReadOnly replica, ICrmDbContext) extracted to
// Infrastructure/DatabaseServiceExtensions.cs
var (connectionString, databaseProvider) = builder.AddDatabaseServices();

// Register Services (backward compatibility)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
// AP-037: Extracted account contact sub-service (registered before AccountService)
builder.Services.AddScoped<IAccountContactService, AccountContactService>();
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

// Auth/Security Services (TODO-AUTH-013 to TODO-AUTH-018)
builder.Services.AddScoped<CRM.Core.Interfaces.ISessionManager, CRM.Infrastructure.Services.Auth.SessionManagerService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IPasswordHistoryService, CRM.Infrastructure.Services.PasswordHistoryService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IAuthAuditService, CRM.Infrastructure.Services.AuthAuditService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IMagicLinkService, CRM.Infrastructure.Services.MagicLinkService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IUserOAuthLinkService, CRM.Infrastructure.Services.UserOAuthLinkService>();

// Auth Advanced Services (TODO-AUTH-003, TODO-AUTH-004, TODO-AUTH-010, TODO-AUTH-019)
builder.Services.Configure<CRM.Core.Options.OktaSsoOptions>(builder.Configuration.GetSection("Auth:Okta"));
builder.Services.Configure<CRM.Core.Options.OpenIdConnectOptions>(builder.Configuration.GetSection("Auth:OpenIdConnect"));
builder.Services.AddScoped<CRM.Core.Interfaces.IOktaSsoService, CRM.Infrastructure.Services.Auth.OktaSsoService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IOpenIdConnectService, CRM.Infrastructure.Services.Auth.OpenIdConnectService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IBiometricAuthService, CRM.Infrastructure.Services.Auth.BiometricAuthService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITrustedDeviceService, CRM.Infrastructure.Services.Auth.TrustedDeviceService>();

// Auth Advanced Services (TODO-AUTH-021 to TODO-AUTH-024)
builder.Services.AddScoped<CRM.Core.Interfaces.ILoginAnalyticsService, CRM.Infrastructure.Services.Auth.LoginAnalyticsService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IRiskAssessmentService, CRM.Infrastructure.Services.Auth.RiskAssessmentService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IDeviceAuthorizationService, CRM.Infrastructure.Services.Auth.DeviceAuthorizationService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IGeoLocationService, CRM.Infrastructure.Services.Auth.GeoLocationService>();

// Rate Limiting Service (TODO-SYS005-002)
builder.Services.AddScoped<CRM.Core.Interfaces.IRateLimitingService, CRM.Infrastructure.Services.RateLimitingService>();

// System Controls: runtime rate-limit toggle + JWT rotation (singleton for in-memory state)
builder.Services.AddSingleton<CRM.Core.Interfaces.ISystemControlsService, CRM.Infrastructure.Services.SystemControlsService>();

// System Module Services (TODO-SYS005-001, TODO-SYS006-004)
builder.Services.AddScoped<CRM.Core.Interfaces.IBusinessHoursConfigService, CRM.Infrastructure.Services.BusinessHoursConfigService>();
builder.Services.AddScoped<CRM.Core.Ports.Input.IGdprService, CRM.Infrastructure.Services.GdprService>();

// Audit Retention & Cleanup (TODO-SYS006-006, TODO-SYS006-007)
builder.Services.AddScoped<CRM.Infrastructure.Services.IAuditRetentionService, CRM.Infrastructure.Services.AuditRetentionService>();
builder.Services.AddAuditLogCleanupJob(options =>
{
    var section = builder.Configuration.GetSection("AuditLogCleanup");
    options.Enabled = section.GetValue<bool>("Enabled", true);
    options.IntervalHours = section.GetValue<int>("IntervalHours", 24);
    options.ArchiveAfterDays = section.GetValue<int>("ArchiveAfterDays", 90);
    options.PurgeAfterDays = section.GetValue<int>("PurgeAfterDays", 365);
});

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
builder.Services.AddHostedService<BackupSchedulerHostedService>();
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

builder.Services.AddScoped<IAdminConfigurationService, AdminConfigurationService>(); // Re-enabled: EscalationRules→ITSMEscalationRules, ServiceQueue→ITSM.ServiceQueue

// Unified Configuration Management Services
builder.Services.AddScoped<IProviderConfigurationService, ProviderConfigurationService>();
builder.Services.AddScoped<ISystemConfigurationService, SystemConfigurationService>();
builder.Services.AddScoped<ICRMConfigurationService, CRMConfigurationService>();

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

builder.Services.AddScoped<IProviderHealthService, ProviderHealthService>();

// INT-002: Provider Registry Service — catalogue of all pluggable providers
builder.Services.AddScoped<IProviderRegistryService, ProviderRegistryService>();

// SYS-001: Admin Dashboard Service
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

// SYS-006: Optional Audit Logging Service (conditional registration)
// Audit logging is disabled by default (opt-in via UseOptionalAuditLogging feature flag)
// When enabled, tracks all entity changes, deletions, and user actions for compliance/audit purposes
builder.Services.AddScoped<IOptionalAuditLoggingService, OptionalAuditLoggingService>();
Log.Information("Optional Audit Logging Service registered (enabled via UseOptionalAuditLogging feature flag)");

// Data Validator Service for import field validation — register when ImportModule is enabled
builder.Services.AddScoped<CRM.Core.Ports.Input.IDataValidator, CRM.Infrastructure.Services.DataValidatorService>();
Log.Information("DataValidatorService registered (import field validation for accounts, contacts, leads, opportunities)");

// Batch Processor Service — open-generic registration needed for per-type processing
builder.Services.AddScoped(typeof(CRM.Core.Ports.Input.IBatchProcessor<>), typeof(CRM.Infrastructure.Services.BatchProcessorService<>));
Log.Information("BatchProcessorService<T> registered as open-generic scoped (import/export batch processing)");

// ITSM Services - IT Service Management (Incident, Problem, Change, CMDB, Knowledge, SLA)
// PHASE 1: Core critical services re-enabled (Feb 16, 2026)
builder.Services.AddScoped<CRM.Infrastructure.Services.ITSM.IBusinessHoursCalculator, CRM.Infrastructure.Services.ITSM.BusinessHoursCalculator>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IIncidentService, CRM.Infrastructure.Services.ITSM.IncidentService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.ISLAService, CRM.Infrastructure.Services.ITSM.SLAService>();
Log.Information("ITSM Phase 1 Tier-1 Services registered: BusinessHoursCalculator, IncidentService, SLAService");

// PHASE 2-4: Additional ITSM services - pending implementation
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IProblemManagementService, CRM.Infrastructure.Services.ITSM.ProblemManagementService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IProblemService, CRM.Infrastructure.Services.ITSM.ProblemService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.ICMDBService, CRM.Infrastructure.Services.ITSM.CMDBService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IChangeManagementService, CRM.Infrastructure.Services.ITSM.ChangeManagementService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IChangeManagementServiceEx, CRM.Infrastructure.Services.ITSM.ChangeManagementServiceEx>();
builder.Services.AddScoped<CRM.Core.Interfaces.IChangeService, CRM.Infrastructure.Services.ChangeService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IKnowledgeManagementService, CRM.Infrastructure.Services.ITSM.KnowledgeManagementService>();
// General Knowledge Base service (CRM.Core.Ports.Input.IKnowledgeBaseService)
builder.Services.AddScoped<CRM.Core.Ports.Input.IKnowledgeBaseService, CRM.Infrastructure.Services.KnowledgeBaseService>();
// KB-010/KB-011: Unified Knowledge Search facade (General KB + ITSM KB)
builder.Services.AddScoped<CRM.Core.Ports.Input.IUnifiedKnowledgeSearchService, CRM.Infrastructure.Services.UnifiedKnowledgeSearchService>();
// KB search index schema is configured by KnowledgeBaseSearchIndexService on startup
builder.Services.AddScoped<CRM.Infrastructure.Services.Search.IKnowledgeBaseSearchIndexService, CRM.Infrastructure.Services.Search.KnowledgeBaseSearchIndexService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IServiceCatalogService, CRM.Infrastructure.Services.ITSM.ServiceCatalogService>();
// IEscalationRulePolicyService is the SLA-focused service (renamed from IEscalationRuleService)
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IEscalationRulePolicyService, CRM.Infrastructure.Services.ITSM.EscalationRuleService>(); // Renamed from IEscalationRuleService
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IEscalationPolicyService, CRM.Infrastructure.Services.ITSM.EscalationPolicyService>();
// ITSM Escalation Analytics (TODO-SD005-011)
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IEscalationAnalyticsService, CRM.Infrastructure.Services.ITSM.EscalationAnalyticsService>();
// SMS Notification Service (TODO-SD005-009) — use Twilio when config present, else built-in stub
var twilioAccountSid = builder.Configuration["Providers:Notifications:Twilio:AccountSid"];
if (!string.IsNullOrWhiteSpace(twilioAccountSid))
{
    builder.Services.Configure<CRM.Infrastructure.Providers.Twilio.TwilioConfiguration>(
        builder.Configuration.GetSection(CRM.Infrastructure.Providers.Twilio.TwilioConfiguration.SectionName));
    builder.Services.AddScoped<CRM.Core.Interfaces.Notifications.ISmsNotificationService,
        CRM.Infrastructure.Providers.Twilio.TwilioSmsService>();
    Log.Information("SMS notification service: TwilioSmsService");
}
else
{
    builder.Services.AddScoped<CRM.Core.Interfaces.Notifications.ISmsNotificationService,
        CRM.Infrastructure.Services.Notifications.SmsNotificationService>();
    Log.Information("SMS notification service: SmsNotificationService (stub)");
}
// ITSM Phase 4 - Advanced Automation & Integration Services
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IWebhookNotificationService, CRM.Infrastructure.Services.ITSM.WebhookNotificationService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IEmailToTicketService, CRM.Infrastructure.Services.ITSM.EmailToTicketService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IITSMDashboardService, CRM.Infrastructure.Services.ITSM.ITSMDashboardService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IMonitoringIntegrationService, CRM.Infrastructure.Services.ITSM.MonitoringIntegrationService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.ICICDIntegrationService, CRM.Infrastructure.Services.ITSM.CICDIntegrationService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.ISelfServiceChatbotService, CRM.Infrastructure.Services.ITSM.SelfServiceChatbotService>();
// ITSM Extended Services — CAB, Calendar, Impact, Article Recommendations
builder.Services.AddScoped<CRM.Infrastructure.Services.ITSM.ICABWorkflowService, CRM.Infrastructure.Services.ITSM.CABWorkflowService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.ITSM.IChangeCalendarService, CRM.Infrastructure.Services.ITSM.ChangeCalendarService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.ITSM.IChangeImpactService, CRM.Infrastructure.Services.ITSM.ChangeImpactService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IArticleRecommendationService, CRM.Infrastructure.Services.ITSM.ArticleRecommendationService>();
// ITSM Advanced Services — Assignment, Catalog, Discovery, Impact Analysis, KCS, Asset Lifecycle
builder.Services.AddScoped<CRM.Infrastructure.Services.ITSM.IAssignmentRulesEngine, CRM.Infrastructure.Services.ITSM.AssignmentRulesEngine>();
builder.Services.AddScoped<CRM.Infrastructure.Services.ITSM.ICatalogApprovalService, CRM.Infrastructure.Services.ITSM.CatalogApprovalService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.ITSM.ICatalogFulfillmentService, CRM.Infrastructure.Services.ITSM.CatalogFulfillmentService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.ITSM.IDiscoveryService, CRM.Infrastructure.Services.ITSM.DiscoveryService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.ITSM.IImpactAnalysisService, CRM.Infrastructure.Services.ITSM.ImpactAnalysisService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.ITSM.IKCSWorkflowService, CRM.Infrastructure.Services.ITSM.KCSWorkflowService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.ITSM.IAssetLifecycleService, CRM.Infrastructure.Services.ITSM.AssetLifecycleService>();
// Slack/Teams notification channels for ITSM — add when external notification provider is configured
builder.Services.AddHttpClient<CRM.Infrastructure.Services.ITSM.SlackItsmNotificationService>();
builder.Services.AddHttpClient<CRM.Infrastructure.Services.ITSM.TeamsItsmNotificationService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IItsmNotificationChannel>(sp =>
    sp.GetRequiredService<CRM.Infrastructure.Services.ITSM.SlackItsmNotificationService>());
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IItsmNotificationChannel>(sp =>
    sp.GetRequiredService<CRM.Infrastructure.Services.ITSM.TeamsItsmNotificationService>());
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IItsmNotificationDispatcher,
    CRM.Infrastructure.Services.ITSM.ItsmNotificationDispatcher>();
Log.Information("ITSM notification channels registered: Slack, Teams (TODO-SD005-010)");
// SLA Enforcement Background Service - runs continuously to monitor and enforce SLAs
builder.Services.AddHostedService<CRM.Infrastructure.Services.ITSM.SLAEnforcementHostedService>();
// Auto-close resolved items background service (auto-closes incidents, service requests, changes, problems)
builder.Services.AddHostedService<CRM.Infrastructure.Services.ITSM.AutoCloseHostedService>();
// Escalation background service (auto-escalates incidents/service requests based on SLA thresholds)
builder.Services.AddHostedService<CRM.Infrastructure.Services.ITSM.EscalationHostedService>();
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
builder.Services.AddScoped<INormalizationService, NormalizationService>(); // PRA-016
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
// ZIP code import queue - singleton so controller can fire-and-forget to the worker
builder.Services.AddSingleton<IZipCodeImportQueue, ZipCodeImportQueue>();
// ZIP code import options
builder.Services.Configure<ZipCodeImportOptions>(
    builder.Configuration.GetSection(ZipCodeImportOptions.SectionName));
// ZIP code import background service (scheduled imports)
builder.Services.AddHostedService<ZipCodeImportHostedService>();
// Contact info validation service (email, phone, social media)
builder.Services.AddScoped<IContactInfoValidationService, ContactInfoValidationService>();
// Phase 1 services - Notes, Tasks, Quotes (Gap Fix Implementation)
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<IRecordCommentService, RecordCommentService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<ISatisfactionService, SatisfactionService>();
// Revenue Analytics (FEAT-REVENUE)
builder.Services.AddScoped<IRevenueAnalyticsService, RevenueAnalyticsService>();
// Phase 2 services - Dashboard, Pipeline, Interaction (Gap Fix Implementation)
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPipelineService, PipelineService>();
builder.Services.AddScoped<IInteractionService, InteractionService>();
// Phase 3 services - Communication, ImportExport, Webhook (Gap Fix Implementation)
builder.Services.AddScoped<ICommunicationService, CommunicationService>();
builder.Services.AddScoped<IImportExportService, ImportExportService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
// Webhook infrastructure - signature generation, retry policy, circuit breaker
builder.Services.AddSingleton<IWebhookSignatureGenerator, WebhookSignatureGenerator>();
builder.Services.AddSingleton<IWebhookRetryPolicy, WebhookRetryPolicy>();
builder.Services.AddSingleton<IWebhookCircuitBreaker, WebhookCircuitBreaker>();
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
// PRA-011: Register ContractExpirationJob so Hangfire's DI activator can instantiate it.
builder.Services.AddTransient<CRM.Infrastructure.Jobs.ContractExpirationJob>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
// AP-022: SubscriptionUsageService — extracted from SubscriptionUsageController fat-controller
builder.Services.AddScoped<CRM.Core.Interfaces.ISubscriptionUsageService, CRM.Infrastructure.Services.SubscriptionUsageService>();
// Usage record batching can be enabled via hosted service when high-volume metering is needed
builder.Services.AddSingleton<CRM.Core.Interfaces.IUsageRecordBatchBuffer,
    CRM.Infrastructure.Services.Billing.UsageRecordBatchBuffer>();
builder.Services.AddHostedService<CRM.Infrastructure.Services.Billing.UsageRecordBatchHostedService>();
Log.Information("UsageRecordBatchBuffer (singleton) and UsageRecordBatchHostedService registered (TODO-SALES006-024)");
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ICommissionService, CommissionService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();

// Admin Configuration Services for Sales and Service Desk Modules
// PHASE 2: Re-enabled from .disabled (Feb 16, 2026)
builder.Services.AddScoped<ICommissionRuleService, CommissionRuleService>();
builder.Services.AddScoped<IDiscountRuleService, DiscountRuleService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.ISLAPolicyAdminService, CRM.Infrastructure.Services.ITSM.SLAPolicyAdminService>();
// AP-021: SLAAnalyticsService — extracted from SLAPoliciesController fat-controller GroupBy analytics
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.ISLAAnalyticsService, CRM.Infrastructure.Services.ITSM.SLAAnalyticsService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.ITSM.EscalationRuleAdminService>(); // concrete registration
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IEscalationRuleService>(sp =>
    sp.GetRequiredService<CRM.Infrastructure.Services.ITSM.EscalationRuleAdminService>());
#pragma warning disable CS0618 // Intentional backward compat registration
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IEscalationRuleAdminService>(sp =>
    sp.GetRequiredService<CRM.Infrastructure.Services.ITSM.EscalationRuleAdminService>());
#pragma warning restore CS0618
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IServiceQueueService, CRM.Infrastructure.Services.ITSM.ServiceQueueService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IAutoAssignmentService, CRM.Infrastructure.Services.ITSM.AutoAssignmentService>();
Log.Information("Admin Configuration Services registered: CommissionRule, DiscountRule, SLAPolicy, EscalationRule, ServiceQueue, AutoAssignment");

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

// Campaign Management Services (5 services)
builder.Services.AddScoped<ICampaignRecipientService, CampaignRecipientService>();
builder.Services.AddScoped<ICampaignMetricsService, CampaignMetricsService>();
builder.Services.AddScoped<ICampaignMetricService, CampaignMetricService>();
builder.Services.AddScoped<ICampaignExecutionService, CampaignExecutionService>();
builder.Services.AddScoped<ICampaignConversionService, CampaignConversionService>();

// Email Sequence Management Service (enhanced)
builder.Services.AddScoped<IEmailSequenceManagementService, EmailSequenceManagementService>();

// Marketing Execution Engine — MKT-001, MKT-005, MKT-006
builder.Services.AddScoped<CRM.Core.Interfaces.IUnsubscribeService, CRM.Infrastructure.Services.UnsubscribeService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IUtmTrackingService, CRM.Infrastructure.Services.UtmTrackingService>();
builder.Services.AddHostedService<CRM.Infrastructure.BackgroundServices.NurtureSequenceBackgroundService>();

// Webhook Management Services (2 services)
builder.Services.AddScoped<IWebhookManagementService, WebhookManagementService>();
builder.Services.AddScoped<IWebhookDispatcherService, WebhookDispatcherService>();

// Analytics, Audit, ITSM services (previously unregistered + new)
builder.Services.AddScoped<CRM.Core.Interfaces.IAnalyticsEventService, CRM.Infrastructure.Services.AnalyticsEventService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IAuditLogService, CRM.Infrastructure.Services.AuditLogService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IAuditLogExportService, CRM.Infrastructure.Services.AuditLogExportService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.ICITypeService, CRM.Infrastructure.Services.ITSM.CITypeService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IChangeTypeService, CRM.Infrastructure.Services.ITSM.ChangeTypeService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IAIAgentUsageService, CRM.Infrastructure.Services.AIAgentUsageService>();

// Semantic Kernel AI Agent Subsystem — registers all 19 agents, 13 plugins,
// CrmKernelFactory, AgentOrchestrator, AgentExecutionService, and filters.
builder.Services.AddSemanticKernel(builder.Configuration);
builder.Services.AddScoped<CRM.Core.Interfaces.IExportJobService, CRM.Infrastructure.Services.ExportJobService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IImportJobService, CRM.Infrastructure.Services.ImportJobService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.IIncidentCategoryService, CRM.Infrastructure.Services.ITSM.IncidentCategoryService>();
builder.Services.AddScoped<CRM.Core.Interfaces.ITSM.ICatalogCategoryService, CRM.Infrastructure.Services.ITSM.CatalogCategoryService>();

// Pricing & Bundles
builder.Services.AddScoped<CRM.Core.Interfaces.IPricingService, CRM.Infrastructure.Services.PricingService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IProductBundleService, CRM.Infrastructure.Services.ProductBundleService>();

// CRM Core & Sales Features P2 (TODO-CRM002-*, TODO-CRM003-*, TODO-GAP-*)
builder.Services.AddScoped<ILeadSourceConfigService, LeadSourceConfigService>();
builder.Services.AddScoped<IWebToLeadFormService, WebToLeadFormService>();
builder.Services.AddScoped<ILeadAgingAlertService, LeadAgingAlertService>();
builder.Services.AddScoped<ILeadAlertService, LeadAlertService>();
builder.Services.AddScoped<IWinLossAnalysisService, WinLossAnalysisService>();
builder.Services.AddScoped<ITerritoryAssignmentService, TerritoryAssignmentService>();
builder.Services.AddScoped<IDynamicPricingEngine, DynamicPricingEngine>();
builder.Services.AddScoped<IPricingRulesService, PricingRulesService>();
builder.Services.AddScoped<ICompetitorService, CompetitorService>();
builder.Services.AddScoped<ILeadCaptureService, LeadCaptureService>();
builder.Services.AddScoped<ILeadQualificationService, LeadQualificationService>();

// Credit Memo service
builder.Services.AddScoped<ICreditMemoService, CreditMemoService>();

// SPRINT 3: Commission & Contract Enhancement Services (TODO-SALES004-011, TODO-GAP-BACKEND-005, TODO-SALES005-014, TODO-GAP-SALES-001)
builder.Services.AddScoped<ICommissionRulesEngine, CommissionRulesEngine>();
builder.Services.AddScoped<IOrderReturnService, OrderReturnService>();
builder.Services.AddScoped<IContractExportService, ContractExportService>();
builder.Services.AddScoped<IPaymentTokenizationService, PaymentTokenizationService>();
builder.Services.AddScoped<StripeIntegrationService>();
Log.Information("Commission & Contract Enhancement Services registered: CommissionRulesEngine, OrderReturn, ContractExport, PaymentTokenization, StripeIntegration");

// Subscription Billing Services (SPEC-SALES-006)
// Recurring Billing Engine - background job for hourly subscription billing cycles
builder.Services.AddScoped<IRecurringBillingEngine, RecurringBillingEngine>();
// Dunning Manager - payment failure recovery with 3-retry escalation (TODO-SALES003-012)
builder.Services.AddScoped<IDunningManager, DunningManager>();
// Dunning Scheduler - runs every 4 hours, uses IServiceScopeFactory for scoped IDunningManager (TODO-SALES003-012)
builder.Services.AddHostedService<DunningSchedulerService>();
// Dunning Schedule Service - CRUD for dunning schedule steps (BACK-010)
builder.Services.AddScoped<IDunningScheduleService, CRM.Infrastructure.Services.DunningScheduleService>();
// Proration Calculator - 4 proration algorithms (ProRata, FullPrice, OneMonth, None)
builder.Services.AddScoped<CRM.Infrastructure.Services.IProrateCalculator, ProrateCalculator>();
// Billing Timezone Service - timezone-aware billing date calculations (TODO-SALES006-023)
builder.Services.AddScoped<CRM.Core.Interfaces.IBillingTimezoneService, CRM.Infrastructure.Services.BillingTimezoneService>();
// Subscription Metrics Aggregator - MRR/ARR/churn/NRR calculations
builder.Services.AddScoped<CRM.Infrastructure.Services.ISubscriptionMetricsAggregator, SubscriptionMetricsAggregator>();
// Master data - Field-to-master-data linking service
builder.Services.AddScoped<IFieldMasterDataService, FieldMasterDataService>();
// Master data seeder - seeds ZipCodes and ColorPalettes on startup if empty
builder.Services.AddScoped<IMasterDataSeederService, MasterDataSeederService>();
// Core data seeder - seeds departments, accounts, products, contacts, lookups, system settings (ADR-002)
builder.Services.AddScoped<ICoreDataSeederService, CoreDataSeederService>();
// Cloud Deployment management service
builder.Services.AddScoped<ICloudDeploymentService, CloudDeploymentService>();
builder.Services.AddHttpClient();

// Hangfire Background Job Processing (SPEC-SALES-006 / TODO-INFRA-01)
// Hangfire provides reliable background job processing with retry logic,
// scheduling, and persistence - critical for recurring billing and dunning.
var hangfireEnabled = builder.Configuration.GetValue<bool>("Hangfire:Enabled", true);
if (hangfireEnabled)
{
    var hangfireConnectionString = builder.Configuration.GetConnectionString("HangfireConnection")
        ?? connectionString; // Fall back to main connection string

    Log.Information("Configuring Hangfire for background job processing (provider: {Provider})", databaseProvider);

    builder.Services.AddHangfire(config =>
    {
        config.SetDataCompatibilityLevel(Hangfire.CompatibilityLevel.Version_180)
              .UseSimpleAssemblyNameTypeSerializer()
              .UseRecommendedSerializerSettings();

        // Use SQL Server persistent storage when available; in-memory for all other providers
        // (jobs will be lost on restart with in-memory storage — acceptable for dev/non-SQL-Server deploys).
        switch (databaseProvider.ToLowerInvariant())
        {
            case "sqlserver":
                config.UseSqlServerStorage(hangfireConnectionString);
                Log.Information("Hangfire using SQL Server persistent storage");
                break;
            default:
                // MariaDB/MySQL, PostgreSQL, SQLite, InMemory — use Hangfire.InMemory storage.
                // For production MariaDB deployments, consider adding Hangfire.MySqlStorage.
                config.UseInMemoryStorage();
                Log.Warning("Hangfire using in-memory storage for '{Provider}' — jobs will not persist across restarts", databaseProvider);
                break;
        }
    });

    // Hangfire server — processes background jobs
    builder.Services.AddHangfireServer(options =>
    {
        options.WorkerCount = builder.Configuration.GetValue<int>("Hangfire:WorkerCount", Environment.ProcessorCount);
        options.Queues = ["recurring-billing", "dunning", "default"];
        options.SchedulePollingInterval = TimeSpan.FromSeconds(30); // Check for scheduled jobs every 30s
    });

    Log.Information("Hangfire server registered with {Workers} worker(s)", Environment.ProcessorCount);
}
else
{
    Log.Warning("Hangfire disabled (Hangfire:Enabled=false) — background jobs will not be processed");
}

// PRA-004: Workflow field schema service — serves entity/related-entity schemas to WorkflowController.
builder.Services.AddSingleton<IWorkflowFieldSchemaService, WorkflowFieldSchemaService>();

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
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/data/dp-keys"))
    .SetApplicationName("CrmSolution");
builder.Services.AddSingleton<IEncryptionService, CRM.Infrastructure.Services.EncryptionService>();
builder.Services.AddScoped<ILLMSettingsService, LLMSettingsService>();
builder.Services.AddHttpClient<ILLMService, LLMService>();
builder.Services.AddSingleton<IResilienceService, ResilienceService>();

// Phase 7 services - AI/Analytics Enhancements (KB search, Lead scoring, Opportunity scoring, Dashboards, Reports)
builder.Services.AddScoped<IAIKnowledgeSearchService, AIKnowledgeSearchService>();
builder.Services.AddScoped<IAILeadScoringService, AILeadScoringService>();
// FEAT-AISCORING: Lead Score History service
builder.Services.AddScoped<ILeadScoreHistoryService, CRM.Infrastructure.Services.LeadScoreHistoryService>();
builder.Services.AddHostedService<CRM.Infrastructure.Services.LeadScoreHistoryDecayBackgroundService>();
builder.Services.AddScoped<IAIOpportunityScoringService, AIOpportunityScoringService>();
builder.Services.AddScoped<IDashboardBuilderService, DashboardBuilderService>();
builder.Services.AddScoped<IReportBuilderService, ReportBuilderService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.IReportSharingService, CRM.Infrastructure.Services.ReportSharingService>();
builder.Services.AddScoped<CRM.Api.Hubs.IDashboardHubService, CRM.Api.Hubs.DashboardHubService>();
builder.Services.AddScoped<IAIPredictiveAnalyticsService, AIPredictiveAnalyticsService>();

// Allen AI Services (OLMo/Tulu models for lead scoring, insights, churn prediction)
builder.Services.Configure<AllenAIConfiguration>(builder.Configuration.GetSection("AllenAI"));
builder.Services.AddHttpClient("AllenAI", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddScoped<IAllenAIService, AllenAIService>();

// AI Insight Services (TODO-AI-03, AI-04, AI-07, AI-08, AI-09, AI-10)
builder.Services.AddScoped<CRM.Core.Interfaces.IChurnPredictionService, CRM.Infrastructure.Services.AI.ChurnPredictionService>();
builder.Services.AddScoped<CRM.Core.Interfaces.INextBestActionService, CRM.Infrastructure.Services.AI.NextBestActionService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IEmailSentimentService, CRM.Infrastructure.Services.AI.EmailSentimentService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IMeetingSummaryService, CRM.Infrastructure.Services.AI.MeetingSummaryService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IDealRiskService, CRM.Infrastructure.Services.AI.DealRiskService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IRevenueForecastService, CRM.Infrastructure.Services.AI.RevenueForecastService>();

// Integration Services (TODO-INT-08, INT-09, INT-10, INT-11)
builder.Services.AddScoped<CRM.Core.Ports.Input.IAccountingSyncService, CRM.Infrastructure.Services.Integrations.AccountingSyncService>();
builder.Services.AddScoped<CRM.Core.Ports.Input.IMarketingSyncService, CRM.Infrastructure.Services.Integrations.MarketingSyncService>();
builder.Services.AddScoped<CRM.Core.Ports.Input.ILinkedInSalesNavService, CRM.Infrastructure.Services.Integrations.LinkedInSalesNavService>();
builder.Services.AddScoped<CRM.Core.Ports.Input.ISchedulingIntegrationService, CRM.Infrastructure.Services.Integrations.SchedulingIntegrationService>();

// INT-001: QuickBooks & Xero OAuth2 accounting integrations
builder.Services.Configure<CRM.Core.Configuration.QuickBooksOptions>(
    builder.Configuration.GetSection(CRM.Core.Configuration.QuickBooksOptions.SectionName));
builder.Services.Configure<CRM.Core.Configuration.XeroOptions>(
    builder.Configuration.GetSection(CRM.Core.Configuration.XeroOptions.SectionName));
builder.Services.AddSingleton<CRM.Infrastructure.Services.Integrations.IntegrationTokenStore>();
builder.Services.AddHttpClient<CRM.Core.Interfaces.IQuickBooksService, CRM.Infrastructure.Services.Integrations.QuickBooksService>();
builder.Services.AddHttpClient<CRM.Core.Interfaces.IXeroService, CRM.Infrastructure.Services.Integrations.XeroService>();

// INT-002: Mailchimp contact-list sync & HubSpot bidirectional sync
builder.Services.Configure<CRM.Core.Configuration.MailchimpOptions>(
    builder.Configuration.GetSection(CRM.Core.Configuration.MailchimpOptions.SectionName));
builder.Services.AddHttpClient<CRM.Core.Interfaces.IMailchimpService, CRM.Infrastructure.Services.Integrations.MailchimpService>();
builder.Services.Configure<CRM.Core.Configuration.HubSpotOptions>(
    builder.Configuration.GetSection(CRM.Core.Configuration.HubSpotOptions.SectionName));
builder.Services.AddHttpClient<CRM.Core.Interfaces.IHubSpotService, CRM.Infrastructure.Services.Integrations.HubSpotService>();
// INT-004: Calendly scheduling webhook integration
builder.Services.Configure<CRM.Core.Configuration.CalendlyOptions>(
    builder.Configuration.GetSection(CRM.Core.Configuration.CalendlyOptions.SectionName));
builder.Services.AddHttpClient<CRM.Core.Interfaces.ICalendlyService, CRM.Infrastructure.Services.Integrations.CalendlyService>();
builder.Services.Configure<NewsSocialOptions>(builder.Configuration.GetSection("NewsSocial"));
builder.Services.AddHttpClient<INewsSocialService, NewsSocialService>();

// Navigation Configuration Service - dynamic navigation aware of pluggable architecture
builder.Services.AddScoped<INavigationConfigService, NavigationConfigService>();

// Field-level audit trail tracking (TODO-SYS006-001)
builder.Services.AddScoped<CRM.Infrastructure.Services.IFieldChangeTracker, CRM.Infrastructure.Services.FieldChangeTracker>();

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
builder.Services.AddScoped<WorkflowEventInterceptor>();
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

// Duplicate Review Worker - assigns pending duplicate candidates to account managers
builder.Services.AddHostedService<DuplicateReviewWorkerService>();

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

// Multi-currency service (TODO-GAP-05)
builder.Services.AddScoped<CRM.Core.Ports.Input.ICurrencyService, CRM.Infrastructure.Services.CurrencyService>();

// PDF generation service stub (TODO-SALES003-010)
builder.Services.AddScoped<CRM.Core.Ports.Input.IPdfGenerationService, CRM.Infrastructure.Services.PdfGenerationService>();
builder.Services.AddScoped<IUserInputPort, UserService>();
builder.Services.AddScoped<IUserGroupInputPort, UserGroupService>();
builder.Services.AddScoped<ISystemSettingsInputPort, SystemSettingsService>();
builder.Services.AddScoped<IServiceRequestInputPort, ServiceRequestService>();
builder.Services.AddScoped<IAccountInputPort, AccountService>();
builder.Services.AddScoped<IDatabaseBackupInputPort, DatabaseBackupService>();

// Custom Fields, Formula Engine & Rollup Fields (CUST-01/02/07/08/09)
builder.Services.AddScoped<CRM.Infrastructure.Services.ICustomFieldValidationService, CRM.Infrastructure.Services.CustomFieldValidationService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.IFormulaFieldEngine, CRM.Infrastructure.Services.FormulaFieldEngine>();
builder.Services.AddScoped<CRM.Infrastructure.Services.IRollupFieldService, CRM.Infrastructure.Services.RollupFieldService>();

// Event Sourcing & Saga Orchestration (INFRA-04/05)
builder.Services.AddScoped<CRM.Infrastructure.Services.EventSourcing.IEventStore, CRM.Infrastructure.Services.EventSourcing.EventStore>();
builder.Services.AddSingleton<CRM.Infrastructure.Services.Saga.ISagaOrchestrator, CRM.Infrastructure.Services.Saga.SagaOrchestrator>();

// Messaging Infrastructure - Redis Streams & Dead Letter Queue (INFRA-06/07)
builder.Services.AddScoped<CRM.Infrastructure.Services.Messaging.IRedisStreamService, CRM.Infrastructure.Services.Messaging.RedisStreamService>();
builder.Services.AddScoped<CRM.Infrastructure.Services.Messaging.IDeadLetterQueueService, CRM.Infrastructure.Services.Messaging.DeadLetterQueueService>();

// FLAG-005: Async audit log consumer — reads from crm:audit:stream and batch-writes to AuditLogs table
builder.Services.AddHostedService<CRM.Infrastructure.Services.Messaging.AuditLogConsumerHostedService>();

// Search Analytics (INFRA-10)
builder.Services.AddSingleton<CRM.Infrastructure.Services.Search.ISearchAnalyticsService, CRM.Infrastructure.Services.Search.SearchAnalyticsService>();

// Customer Portal (FEAT-PORTAL)
builder.Services.AddScoped<IPortalAuthService, PortalAuthService>();
builder.Services.AddScoped<IPortalService, PortalService>();
builder.Services.AddScoped<IPortalAdminService, PortalAdminService>();
builder.Services.AddScoped<IPartnerPortalService, PartnerPortalService>(); // PORTAL-025

// Configurable Enums (ENUM-BE-012)
builder.Services.AddScoped<IEnumManagementService, EnumManagementService>();

// AP-039: JWT authentication and authorization extracted to Infrastructure/JwtAuthServiceExtensions.cs
builder.Services.AddJwtAuthServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// Configure Hangfire Dashboard (TODO-INFRA-01)
if (hangfireEnabled)
{
    Log.Information("Enabling Hangfire dashboard at /hangfire");

    // Hangfire Dashboard — restricted to local requests in all environments.
    // For production admin access, replace LocalRequestsOnlyAuthorizationFilter with
    // a custom IDashboardAuthorizationFilter that enforces the Admin role.
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter()],
        IgnoreAntiforgeryToken = true, // NOSONAR - S4502: JWT-only API using Bearer tokens, no cookie authentication, CSRF is not applicable
        DashboardTitle = "CRM Background Jobs"
    });

    // Recurring billing/dunning jobs are wired here once RecurringBillingEngine
    // and DunningManager are re-enabled (see SOLUTION_GAPS_REMEDIATION_PLAN.md).
    // IRecurringBillingEngine and IDunningManager are ready for scheduling — enable via feature flag

    // PRA-011: Wire ContractExpirationJob to Hangfire — runs daily at 1:00 AM UTC.
    // ContractExpirationJob.JobId = "contract-expiration-job"
    RecurringJob.AddOrUpdate<CRM.Infrastructure.Jobs.ContractExpirationJob>(
        CRM.Infrastructure.Jobs.ContractExpirationJob.JobId,
        job => job.ExecuteAsync(CancellationToken.None),
        CRM.Infrastructure.Jobs.ContractExpirationJob.CronExpression,
        new Hangfire.RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
    app.Services.RegisterContractExpirationJob();
}

// AP-019: Schema management and startup seeding extracted to Infrastructure/DatabaseStartupExtensions.cs.
// All GetRequiredService/GetService calls are encapsulated in a properly scoped extension method.
await app.RunStartupSeedingAsync(databaseProvider);


// Configure Middleware
// AP-032: ErrorHandlingMiddleware MUST be first — catches all unhandled exceptions and maps to HTTP responses
app.UseMiddleware<ErrorHandlingMiddleware>();

// Security headers (HSTS, X-Content-Type-Options, X-Frame-Options, etc.)
app.UseSecurityHeaders();

// HTTPS redirection — only active when HTTPS is actually available (certificate loaded).
// Without a cert, Kestrel runs HTTP-only and redirecting to HTTPS would cause a connection loop.
if (httpsEnabled)
{
    app.UseHttpsRedirection();
}

// CORS — must be registered before authentication/authorization
app.UseCors();

// Rate limiter (feature-flag driven via RateLimiting:EnableEndpointRateLimiting in appsettings)
if (rateLimitingEnabled)
{
    app.UseRateLimiter();
}

// Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Request instrumentation — after auth so user identity is available in telemetry
app.UseInstrumentation(verbose: isDevelopment);

// Map API controllers
app.MapControllers();

// Map SignalR hubs for real-time notifications
app.MapHub<CRM.Api.Hubs.DashboardHub>("/hubs/dashboard");
app.MapHub<CRM.Api.Hubs.CrmNotificationHub>("/hubs/notifications");
app.MapHub<CRM.Api.Hubs.AgentApprovalHub>("/hubs/agentapproval");
app.MapHub<CRM.Api.Hubs.SLACountdownHub>("/hubs/slacountdown");

// OpenAPI document endpoint (used by Scalar UI, client code generators, etc.)
app.MapOpenApi();

// Start the application
await app.RunAsync();

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
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using AspNetCoreRateLimit;
using Serilog;
using System.Text;
using System.IO;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add rate limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.RealIpHeader = "X-Real-IP";
    options.ClientIdHeader = "X-ClientId";
    options.HttpStatusCode = 429;
    options.GeneralRules = new List<RateLimitRule>
    {
        // General API limit
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 100
        },
        // Stricter limits for auth endpoints to prevent brute force attacks
        new RateLimitRule
        {
            Endpoint = "*:/api/auth/login",
            Period = "1m",
            Limit = 10
        },
        new RateLimitRule
        {
            Endpoint = "*:/api/auth/register",
            Period = "1h",
            Limit = 5
        },
        new RateLimitRule
        {
            Endpoint = "*:/api/auth/verify-2fa",
            Period = "1m",
            Limit = 5
        },
        new RateLimitRule
        {
            Endpoint = "*:/api/auth/forgot-password",
            Period = "1h",
            Limit = 3
        }
    };
});
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS - configurable origins for production security
var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(',') 
    ?? new[] { "http://localhost:3000", "https://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
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
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString) && (databaseProvider.ToLower() == "mysql" || databaseProvider.ToLower() == "mariadb"))
{
    var dbHost = builder.Configuration["DB_HOST"] ?? builder.Configuration["DbHost"] ?? "mariadb";
    var dbPort = builder.Configuration["DB_PORT"] ?? "3306";
    var dbName = builder.Configuration["DB_NAME"] ?? "crm_db";
    var dbUser = builder.Configuration["DB_USER"] ?? "crm_user";
    var dbPass = builder.Configuration["DB_PASSWORD"] ?? builder.Configuration["DB_PASS"] ?? "crm_pass";
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

// Register ICrmDbContext interface
builder.Services.AddScoped<ICrmDbContext>(provider => provider.GetRequiredService<CrmDbContext>());

// Register Services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICustomerService, CustomerService>();
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
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IContactsService, ContactsService>();
builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();
builder.Services.AddScoped<IServiceRequestCategoryService, ServiceRequestCategoryService>();
builder.Services.AddScoped<IServiceRequestSubcategoryService, ServiceRequestSubcategoryService>();
builder.Services.AddScoped<IServiceRequestCustomFieldService, ServiceRequestCustomFieldService>();
builder.Services.AddScoped<IColorPaletteService, ColorPaletteService>();
builder.Services.AddHttpClient<IColorPaletteService, ColorPaletteService>();
builder.Services.AddScoped<ModuleFieldConfigurationService>();
builder.Services.AddScoped<ModuleUIConfigService>();
builder.Services.AddScoped<DemoDataSeederService>();
builder.Services.AddScoped<CRM.Core.Interfaces.IAccountService, CRM.Infrastructure.Services.AccountService>();
// Normalization helper for tags/custom fields
builder.Services.AddScoped<NormalizationService>();

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
        
        // For non-SQLite databases, try applying migrations first; fall back to EnsureCreated
        if (databaseProvider.ToLower() != "sqlite")
        {
            try
            {
                Log.Information($"Attempting to apply migrations for {databaseProvider} database...");
                await db.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not apply migrations for non-sqlite provider, falling back to EnsureCreated...");
                await db.Database.EnsureCreatedAsync();
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
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM API V1"));
}

app.UseHttpsRedirection();

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


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
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS - configurable origins for production security
var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(',') 
    ?? new[] { "http://localhost:3000", "https://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development: Allow any origin for easier testing
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            // Production: Restrict to configured origins
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// Configure Database
var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "sqlite";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=crm.db";

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

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "your-super-secret-key-that-is-at-least-32-characters-long!!!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CRMApp";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CRMUsers";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
.AddJwtBearer("Bearer", options =>
{
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
        
        // For non-SQLite databases, use EnsureCreated (migrations are SQLite-specific)
        if (databaseProvider.ToLower() != "sqlite")
        {
            Log.Information($"Using EnsureCreated for {databaseProvider} database...");
            await db.Database.EnsureCreatedAsync();
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
app.UseCors("AllowAll");
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


using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories;
using CRM.Infrastructure.Services;
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

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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
        case "sqlite":
            options.UseSqlite(connectionString);
            break;
        case "sqlserver":
        default:
            options.UseSqlite("Data Source=crm.db");
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
builder.Services.AddScoped<IUserApprovalService, UserApprovalService>();
builder.Services.AddScoped<IDatabaseBackupService, DatabaseBackupService>();

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "your-super-secret-key-that-is-at-least-32-characters-long!!!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CRMApp";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CRMUsers";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
});

// Note: JwtBearer authentication requires Microsoft.AspNetCore.Authentication.JwtBearer NuGet package
// For now, authentication is configured but the JWT bearer middleware will need to be added manually

var app = builder.Build();

// Apply migrations and seed data automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        await DbSeed.SeedAsync(db);
        Log.Information("Database migrations and seeding completed successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error applying migrations or seeding database");
    }
}

// Configure Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM API V1"));
}

app.UseHttpsRedirection();

// Serve static files from frontend build
var frontendBuildPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "CRM.Frontend", "build");
if (Directory.Exists(frontendBuildPath))
{
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = new PhysicalFileProvider(Path.GetFullPath(frontendBuildPath)) });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(Path.GetFullPath(frontendBuildPath)) });
}

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

// SPA fallback - serve index.html for unmatched routes
app.MapFallback(context =>
{
    context.Response.ContentType = "text/html";
    return context.Response.SendFileAsync(Path.Combine(frontendBuildPath, "index.html"));
});

app.Run();


using CRM.ServiceDefaults;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using CRM.Infrastructure.Repositories;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults (logging, auth, CORS, health checks)
builder.AddServiceDefaults();

// Add controllers from this assembly explicitly
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Program).Assembly);

// Add database context
builder.Services.AddMariaDbContext<CrmDbContext>(builder.Configuration);

// Register generic repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Register shared/utility services required by marketing services
builder.Services.AddScoped<NormalizationService>();
builder.Services.AddScoped<IContactInfoService, ContactInfoService>();

// Register marketing-related services
builder.Services.AddScoped<IMarketingCampaignService, MarketingCampaignService>();

// Register input ports (Hexagonal Architecture)
builder.Services.AddScoped<ICampaignInputPort, MarketingCampaignService>();

var app = builder.Build();

// Use service defaults pipeline
app.UseServiceDefaults();

Log.Information("CRM Marketing Service starting on port 5004");
app.Run();

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

// Register shared/utility services required by sales services
builder.Services.AddScoped<NormalizationService>();
builder.Services.AddScoped<IContactInfoService, ContactInfoService>();

// Register sales-related services
builder.Services.AddScoped<IOpportunityService, OpportunityService>();

// Register input ports (Hexagonal Architecture)
builder.Services.AddScoped<IOpportunityInputPort, OpportunityService>();

var app = builder.Build();

// Use service defaults pipeline
app.UseServiceDefaults();

Log.Information("CRM Sales Service starting on port 5003");
app.Run();

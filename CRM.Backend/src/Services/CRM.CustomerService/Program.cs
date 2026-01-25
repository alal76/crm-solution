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

// Register customer-related services
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IContactsService, ContactsService>();
builder.Services.AddScoped<IContactInfoService, ContactInfoService>();
builder.Services.AddScoped<IAccountService, AccountService>();

// Register input ports (Hexagonal Architecture)
builder.Services.AddScoped<ICustomerInputPort, CustomerService>();
builder.Services.AddScoped<IContactInputPort, ContactsService>();
builder.Services.AddScoped<IAccountInputPort, AccountService>();

var app = builder.Build();

// Use service defaults pipeline
app.UseServiceDefaults();

Log.Information("CRM Customer Service starting on port 5002");
app.Run();

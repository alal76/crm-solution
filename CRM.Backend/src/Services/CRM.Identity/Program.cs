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

// Register identity-related services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ITotpService, TotpService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserGroupService, UserGroupService>();
builder.Services.AddScoped<IUserApprovalService, UserApprovalService>();

// Register input ports (Hexagonal Architecture)
builder.Services.AddScoped<IAuthInputPort, AuthenticationService>();
builder.Services.AddScoped<IUserInputPort, UserService>();
builder.Services.AddScoped<IUserGroupInputPort, UserGroupService>();

var app = builder.Build();

// Use service defaults pipeline
app.UseServiceDefaults();

Log.Information("CRM Identity Service starting on port 5001");
app.Run();

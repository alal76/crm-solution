// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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

// Register shared/utility services required by service desk services
builder.Services.AddScoped<NormalizationService>();
builder.Services.AddScoped<IContactInfoService, ContactInfoService>();

// Register service desk related services
builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();
builder.Services.AddScoped<IServiceRequestCategoryService, ServiceRequestCategoryService>();
builder.Services.AddScoped<IServiceRequestSubcategoryService, ServiceRequestSubcategoryService>();
builder.Services.AddScoped<IServiceRequestCustomFieldService, ServiceRequestCustomFieldService>();
builder.Services.AddScoped<IServiceRequestTypeService, ServiceRequestTypeService>();

// Register workflow services
builder.Services.AddScoped<WorkflowService>();
builder.Services.AddScoped<WorkflowInstanceService>();

// Register LLM services for AI-powered workflows
builder.Services.AddHttpClient<ILLMService, LLMService>();
builder.Services.AddScoped<ILLMSettingsService, LLMSettingsService>();

// Register input ports (Hexagonal Architecture)
builder.Services.AddScoped<IServiceRequestInputPort, ServiceRequestService>();

var app = builder.Build();

// Use service defaults pipeline
app.UseServiceDefaults();

Log.Information("CRM Service Desk Service starting on port 5005");
app.Run();

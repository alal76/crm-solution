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

// Register shared/utility services
builder.Services.AddScoped<NormalizationService>();
builder.Services.AddScoped<IContactInfoService, ContactInfoService>();

// Register customer-related services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IContactsService, ContactsService>();
builder.Services.AddScoped<IAccountService, AccountService>();

// Register input ports (Hexagonal Architecture)
builder.Services.AddScoped<ICustomerInputPort, AccountService>();
builder.Services.AddScoped<IContactInputPort, ContactsService>();
builder.Services.AddScoped<IAccountInputPort, AccountService>();

var app = builder.Build();

// Use service defaults pipeline
app.UseServiceDefaults();

Log.Information("CRM Customer Service starting on port 5002");
app.Run();

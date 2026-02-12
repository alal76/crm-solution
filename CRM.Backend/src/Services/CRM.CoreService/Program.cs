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

// Register core services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISystemSettingsService, SystemSettingsService>();
builder.Services.AddScoped<IDatabaseBackupService, DatabaseBackupService>();
builder.Services.AddScoped<IColorPaletteService, ColorPaletteService>();
builder.Services.AddHttpClient<IColorPaletteService, ColorPaletteService>();
builder.Services.AddScoped<ModuleFieldConfigurationService>();
builder.Services.AddScoped<ModuleUIConfigService>();
builder.Services.AddScoped<IZipCodeService, ZipCodeService>();
builder.Services.AddScoped<IFieldMasterDataService, FieldMasterDataService>();
builder.Services.AddScoped<NormalizationService>();
// Core data seeder - seeds departments, accounts, products, contacts, lookups, system settings (ADR-002)
builder.Services.AddScoped<ICoreDataSeederService, CoreDataSeederService>();

// Register input ports (Hexagonal Architecture)
builder.Services.AddScoped<IProductInputPort, ProductService>();
builder.Services.AddScoped<ISystemSettingsInputPort, SystemSettingsService>();
builder.Services.AddScoped<IDatabaseBackupInputPort, DatabaseBackupService>();

var app = builder.Build();

// Use service defaults pipeline
app.UseServiceDefaults();

Log.Information("CRM Core Service starting on port 5006");
app.Run();

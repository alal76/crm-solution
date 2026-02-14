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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CRM.Tests.Helpers;

public class ApiTestFactory : WebApplicationFactory<Program>
{
    public ApiTestFactory()
    {
        // Program.cs has production guards that throw before ConfigureWebHost runs.
        // UseEnvironment("Testing") makes IsDevelopment()=false, so provide real values
        // to bypass all guard checks.
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        Environment.SetEnvironmentVariable("DatabaseProvider", "inmemory");
        Environment.SetEnvironmentVariable("DB_PASSWORD", "test_password");
        Environment.SetEnvironmentVariable("Jwt__Secret", "BVT-test-jwt-secret-key-at-least-32-characters-long!");
        Environment.SetEnvironmentVariable("FeatureManagement__UseExternalSearch", "false");
        Environment.SetEnvironmentVariable("FeatureManagement__UseExternalChat", "false");
        Environment.SetEnvironmentVariable("FeatureManagement__UseExternalNotifications", "false");
        Environment.SetEnvironmentVariable("FeatureManagement__UseExternalAnalytics", "false");
        Environment.SetEnvironmentVariable("FeatureManagement__UseExternalSignatures", "false");
        Environment.SetEnvironmentVariable("FeatureManagement__UseExternalAI", "false");
        Environment.SetEnvironmentVariable("FeatureManagement__UseExternalIntegrations", "false");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseProvider"] = "inmemory"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<CrmDbContext>));
            services.RemoveAll(typeof(CrmDbContext));
            services.RemoveAll(typeof(ICrmDbContext));

            services.AddDbContext<CrmDbContext>(options =>
            {
                options.UseInMemoryDatabase("crm_bvt");
            });
            services.AddScoped<ICrmDbContext>(sp => sp.GetRequiredService<CrmDbContext>());

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            services.AddAuthorization();
        });
    }
}

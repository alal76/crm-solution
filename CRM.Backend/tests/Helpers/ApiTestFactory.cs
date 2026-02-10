using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CRM.Tests.Helpers;

public class ApiTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

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

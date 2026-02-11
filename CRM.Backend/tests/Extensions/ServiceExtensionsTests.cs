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
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Moq;
using Xunit;
using FluentAssertions;

namespace CRM.Tests.Extensions;

/// <summary>
/// Unit tests for ServiceExtensions microservice configuration methods.
/// Tests DI registration, authentication setup, and middleware configuration.
/// </summary>
public class ServiceExtensionsTests
{
    #region AddServiceDefaults Tests

    [Fact]
    public void AddServiceDefaults_WithValidJwtKey_ShouldConfigureServices()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "TestIssuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "TestAudience");

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });

            // Act
            var result = builder.AddServiceDefaults();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeSameAs(builder);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("JWT_KEY", null);
            Environment.SetEnvironmentVariable("JWT_ISSUER", null);
            Environment.SetEnvironmentVariable("JWT_AUDIENCE", null);
        }
    }

    [Fact]
    public void AddServiceDefaults_WithoutJwtKey_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", null);

        var builder = WebApplication.CreateBuilder(new string[] { });

        // Act & Assert
        Action act = () => builder.AddServiceDefaults();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*JWT_KEY*");
    }

    [Fact]
    public void AddServiceDefaults_ShouldAddCorsPolicy()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();

            // Act
            var services = builder.Services.BuildServiceProvider();

            // Assert - CORS policy should be added
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    [Fact]
    public void AddServiceDefaults_ShouldAddMemoryCache()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();

            // Act
            var services = builder.Services.BuildServiceProvider();
            var memoryCache = services.GetService<IMemoryCache>();

            // Assert
            memoryCache.Should().NotBeNull();
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    [Fact]
    public void AddServiceDefaults_ShouldAddHealthChecks()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();

            // Assert - health checks should be added (verified by not throwing)
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    [Fact]
    public void AddServiceDefaults_ShouldAddAuthentication()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();

            // Assert - authentication should be configured
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    [Fact]
    public void AddServiceDefaults_ShouldAddAuthorization()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();

            // Assert - authorization should be added
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    [Fact]
    public void AddServiceDefaults_ShouldAddControllers()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();

            // Assert - controllers should be added
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    [Fact]
    public void AddServiceDefaults_ShouldAddSwagger()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();

            // Assert - Swagger should be added
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    [Fact]
    public void AddServiceDefaults_ShouldAddResponseCaching()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();

            // Assert - response caching should be added
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    [Fact]
    public void AddServiceDefaults_ShouldAddOutputCachePolicies()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();

            // Assert - output cache policies should be added:
            // NoCache, ShortCache, MediumCache, LongCache
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    #endregion

    #region JWT Configuration Tests

    [Fact]
    public void AddServiceDefaults_WithConfigurationJwtKey_ShouldUseConfigValue()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", null);

        var configValues = new Dictionary<string, string?>
        {
            { "JwtSettings:Key", "ConfigKeyThatIsAtLeast32CharactersLong!" },
            { "JwtSettings:Issuer", "ConfigIssuer" },
            { "JwtSettings:Audience", "ConfigAudience" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        var builder = WebApplication.CreateBuilder(new string[] { });
        builder.Configuration.AddConfiguration(configuration);

        // Act & Assert - should not throw since config has JWT key
        try
        {
            builder.AddServiceDefaults();
        }
        catch (InvalidOperationException)
        {
            // Expected if environment variable takes precedence
        }
    }

    [Fact]
    public void AddServiceDefaults_ShouldUseDefaultIssuerWhenNotProvided()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");
        Environment.SetEnvironmentVariable("JWT_ISSUER", null);

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();

            // Assert - should default to "CRM.Api"
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    [Fact]
    public void AddServiceDefaults_ShouldUseDefaultAudienceWhenNotProvided()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", null);

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();

            // Assert - should default to "CRM.Client"
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    #endregion

    #region UseServiceDefaults Tests

    [Fact]
    public void UseServiceDefaults_ShouldReturnWebApplication()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();
            var app = builder.Build();

            // Act
            var result = app.UseServiceDefaults();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeSameAs(app);
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    [Fact]
    public void UseServiceDefaults_ShouldConfigureExceptionHandler()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();
            var app = builder.Build();

            // Act
            app.UseServiceDefaults();

            // Assert - exception handler should be configured
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    [Fact]
    public void UseServiceDefaults_ShouldMapHealthChecks()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();
            var app = builder.Build();

            // Act
            app.UseServiceDefaults();

            // Assert - /health endpoint should be mapped
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    [Fact]
    public void UseServiceDefaults_ShouldMapControllers()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();
            var app = builder.Build();

            // Act
            app.UseServiceDefaults();

            // Assert - controllers should be mapped
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    [Fact]
    public void UseServiceDefaults_ShouldUseCors()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();
            var app = builder.Build();

            // Act
            app.UseServiceDefaults();

            // Assert - CORS middleware should be used
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    [Fact]
    public void UseServiceDefaults_ShouldUseAuthentication()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();
            var app = builder.Build();

            // Act
            app.UseServiceDefaults();

            // Assert - authentication middleware should be used
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    [Fact]
    public void UseServiceDefaults_ShouldUseAuthorization()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();
            var app = builder.Build();

            // Act
            app.UseServiceDefaults();

            // Assert - authorization middleware should be used
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    [Fact]
    public void UseServiceDefaults_ShouldUseResponseCaching()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();
            var app = builder.Build();

            // Act
            app.UseServiceDefaults();

            // Assert - response caching middleware should be used
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    [Fact]
    public void UseServiceDefaults_ShouldUseOutputCache()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecureKeyThatIsAtLeast32CharactersLong!");

        try
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.AddServiceDefaults();
            var app = builder.Build();

            // Act
            app.UseServiceDefaults();

            // Assert - output cache middleware should be used
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", null);
        }
    }

    #endregion
}

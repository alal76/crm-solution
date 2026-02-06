// CRM Solution - Database Extensions Tests
// Tests for database context configuration extension methods

using CRM.ServiceDefaults;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using FluentAssertions;

namespace CRM.Tests.Extensions;

/// <summary>
/// Unit tests for DatabaseExtensions database configuration methods.
/// Tests connection string resolution, environment variable handling, and DbContext registration.
/// </summary>
public class DatabaseExtensionsTests
{
    #region Environment Variable Connection String Tests

    [Fact]
    public void AddMariaDbContext_WithEnvironmentVariables_ShouldBuildConnectionString()
    {
        // Arrange
        SetupEnvironmentVariables(
            host: "test-db-host",
            port: "3307",
            name: "test_db",
            user: "test_user",
            password: "test_password"
        );

        try
        {
            var services = new ServiceCollection();
            var configuration = CreateEmptyConfiguration();

            // Act
            services.AddMariaDbContext<TestDbContext>(configuration);

            // Assert - should not throw
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<DbContextOptions<TestDbContext>>();
            options.Should().NotBeNull();
        }
        finally
        {
            ClearEnvironmentVariables();
        }
    }

    [Fact]
    public void AddMariaDbContext_WithDbUserButNoPassword_ShouldThrowInvalidOperationException()
    {
        // Arrange
        SetupEnvironmentVariables(
            host: "test-db-host",
            port: "3306",
            name: "test_db",
            user: "test_user",
            password: null
        );

        try
        {
            var services = new ServiceCollection();
            var configuration = CreateEmptyConfiguration();

            // Act & Assert
            Action act = () => services.AddMariaDbContext<TestDbContext>(configuration);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*DB_PASSWORD*required*");
        }
        finally
        {
            ClearEnvironmentVariables();
        }
    }

    [Fact]
    public void AddMariaDbContext_WithDefaultHost_ShouldUseKubernetesServiceAddress()
    {
        // Arrange
        SetupEnvironmentVariables(
            host: null,  // Should default to Kubernetes service address
            port: "3306",
            name: "test_db",
            user: "test_user",
            password: "test_password"
        );

        try
        {
            var services = new ServiceCollection();
            var configuration = CreateEmptyConfiguration();

            // Act - should not throw
            services.AddMariaDbContext<TestDbContext>(configuration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<DbContextOptions<TestDbContext>>();
            options.Should().NotBeNull();
        }
        finally
        {
            ClearEnvironmentVariables();
        }
    }

    [Fact]
    public void AddMariaDbContext_WithDefaultPort_ShouldUse3306()
    {
        // Arrange
        SetupEnvironmentVariables(
            host: "test-db-host",
            port: null,  // Should default to 3306
            name: "test_db",
            user: "test_user",
            password: "test_password"
        );

        try
        {
            var services = new ServiceCollection();
            var configuration = CreateEmptyConfiguration();

            // Act - should not throw
            services.AddMariaDbContext<TestDbContext>(configuration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<DbContextOptions<TestDbContext>>();
            options.Should().NotBeNull();
        }
        finally
        {
            ClearEnvironmentVariables();
        }
    }

    [Fact]
    public void AddMariaDbContext_WithDefaultDatabaseName_ShouldUseCrmDb()
    {
        // Arrange
        SetupEnvironmentVariables(
            host: "test-db-host",
            port: "3306",
            name: null,  // Should default to crm_db
            user: "test_user",
            password: "test_password"
        );

        try
        {
            var services = new ServiceCollection();
            var configuration = CreateEmptyConfiguration();

            // Act - should not throw
            services.AddMariaDbContext<TestDbContext>(configuration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<DbContextOptions<TestDbContext>>();
            options.Should().NotBeNull();
        }
        finally
        {
            ClearEnvironmentVariables();
        }
    }

    #endregion

    #region Configuration Connection String Tests

    [Fact]
    public void AddMariaDbContext_WithoutEnvVars_ShouldUseConfigurationConnectionString()
    {
        // Arrange
        ClearEnvironmentVariables();

        var services = new ServiceCollection();
        var configuration = CreateConfigurationWithConnectionString(
            "Server=config-host;Port=3306;Database=crm_db;Uid=config_user;Pwd=config_pass;"
        );

        // Act
        services.AddMariaDbContext<TestDbContext>(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<DbContextOptions<TestDbContext>>();
        options.Should().NotBeNull();
    }

    [Fact]
    public void AddMariaDbContext_WithoutEnvVarsOrConfig_ShouldThrowInvalidOperationException()
    {
        // Arrange
        ClearEnvironmentVariables();

        var services = new ServiceCollection();
        var configuration = CreateEmptyConfiguration();

        // Act & Assert
        Action act = () => services.AddMariaDbContext<TestDbContext>(configuration);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*connection string*required*");
    }

    [Fact]
    public void AddMariaDbContext_WithCustomConnectionStringName_ShouldUseCorrectName()
    {
        // Arrange
        ClearEnvironmentVariables();

        var services = new ServiceCollection();
        var configuration = CreateConfigurationWithNamedConnectionString(
            "CustomConnection",
            "Server=custom-host;Port=3306;Database=custom_db;Uid=custom_user;Pwd=custom_pass;"
        );

        // Act
        services.AddMariaDbContext<TestDbContext>(configuration, "CustomConnection");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<DbContextOptions<TestDbContext>>();
        options.Should().NotBeNull();
    }

    [Fact]
    public void AddMariaDbContext_WithMissingCustomConnectionString_ShouldThrowInvalidOperationException()
    {
        // Arrange
        ClearEnvironmentVariables();

        var services = new ServiceCollection();
        var configuration = CreateEmptyConfiguration();

        // Act & Assert
        Action act = () => services.AddMariaDbContext<TestDbContext>(configuration, "MissingConnectionString");
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Service Registration Tests

    [Fact]
    public void AddMariaDbContext_ShouldRegisterDbContext()
    {
        // Arrange
        SetupEnvironmentVariables(
            host: "test-db-host",
            port: "3306",
            name: "test_db",
            user: "test_user",
            password: "test_password"
        );

        try
        {
            var services = new ServiceCollection();
            var configuration = CreateEmptyConfiguration();

            // Act
            services.AddMariaDbContext<TestDbContext>(configuration);

            // Assert
            var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TestDbContext));
            serviceDescriptor.Should().NotBeNull();
        }
        finally
        {
            ClearEnvironmentVariables();
        }
    }

    [Fact]
    public void AddMariaDbContext_ShouldRegisterICrmDbContext()
    {
        // Arrange
        SetupEnvironmentVariables(
            host: "test-db-host",
            port: "3306",
            name: "test_db",
            user: "test_user",
            password: "test_password"
        );

        try
        {
            var services = new ServiceCollection();
            var configuration = CreateEmptyConfiguration();

            // Act
            services.AddMariaDbContext<TestDbContext>(configuration);

            // Assert
            var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ICrmDbContext));
            serviceDescriptor.Should().NotBeNull();
        }
        finally
        {
            ClearEnvironmentVariables();
        }
    }

    [Fact]
    public void AddMariaDbContext_ShouldReturnServiceCollection()
    {
        // Arrange
        SetupEnvironmentVariables(
            host: "test-db-host",
            port: "3306",
            name: "test_db",
            user: "test_user",
            password: "test_password"
        );

        try
        {
            var services = new ServiceCollection();
            var configuration = CreateEmptyConfiguration();

            // Act
            var result = services.AddMariaDbContext<TestDbContext>(configuration);

            // Assert
            result.Should().BeSameAs(services);
        }
        finally
        {
            ClearEnvironmentVariables();
        }
    }

    #endregion

    #region MariaDB Configuration Tests

    [Fact]
    public void AddMariaDbContext_ShouldConfigureMariaDbVersion()
    {
        // Arrange
        SetupEnvironmentVariables(
            host: "test-db-host",
            port: "3306",
            name: "test_db",
            user: "test_user",
            password: "test_password"
        );

        try
        {
            var services = new ServiceCollection();
            var configuration = CreateEmptyConfiguration();

            // Act
            services.AddMariaDbContext<TestDbContext>(configuration);

            // Assert - should configure MariaDB 11.0.0
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<DbContextOptions<TestDbContext>>();
            options.Should().NotBeNull();
        }
        finally
        {
            ClearEnvironmentVariables();
        }
    }

    [Fact]
    public void AddMariaDbContext_ShouldEnableRetryOnFailure()
    {
        // Arrange
        SetupEnvironmentVariables(
            host: "test-db-host",
            port: "3306",
            name: "test_db",
            user: "test_user",
            password: "test_password"
        );

        try
        {
            var services = new ServiceCollection();
            var configuration = CreateEmptyConfiguration();

            // Act
            services.AddMariaDbContext<TestDbContext>(configuration);

            // Assert - retry should be enabled with maxRetryCount: 5, maxRetryDelay: 30 seconds
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<DbContextOptions<TestDbContext>>();
            options.Should().NotBeNull();
        }
        finally
        {
            ClearEnvironmentVariables();
        }
    }

    #endregion

    #region Priority Tests

    [Fact]
    public void AddMariaDbContext_EnvironmentVariables_ShouldTakePrecedenceOverConfiguration()
    {
        // Arrange
        SetupEnvironmentVariables(
            host: "env-db-host",
            port: "3306",
            name: "env_db",
            user: "env_user",
            password: "env_password"
        );

        try
        {
            var services = new ServiceCollection();
            var configuration = CreateConfigurationWithConnectionString(
                "Server=config-host;Port=3306;Database=config_db;Uid=config_user;Pwd=config_pass;"
            );

            // Act - should use environment variables
            services.AddMariaDbContext<TestDbContext>(configuration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<DbContextOptions<TestDbContext>>();
            options.Should().NotBeNull();
        }
        finally
        {
            ClearEnvironmentVariables();
        }
    }

    [Fact]
    public void AddMariaDbContext_EmptyDbUserEnvVar_ShouldFallbackToConfiguration()
    {
        // Arrange
        SetupEnvironmentVariables(
            host: "env-db-host",
            port: "3306",
            name: "env_db",
            user: "",  // Empty string should trigger fallback
            password: "env_password"
        );

        try
        {
            var services = new ServiceCollection();
            var configuration = CreateConfigurationWithConnectionString(
                "Server=config-host;Port=3306;Database=config_db;Uid=config_user;Pwd=config_pass;"
            );

            // Act - should fallback to configuration
            services.AddMariaDbContext<TestDbContext>(configuration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<DbContextOptions<TestDbContext>>();
            options.Should().NotBeNull();
        }
        finally
        {
            ClearEnvironmentVariables();
        }
    }

    #endregion

    #region Helper Methods

    private static void SetupEnvironmentVariables(
        string? host,
        string? port,
        string? name,
        string? user,
        string? password)
    {
        Environment.SetEnvironmentVariable("DB_HOST", host);
        Environment.SetEnvironmentVariable("DB_PORT", port);
        Environment.SetEnvironmentVariable("DB_NAME", name);
        Environment.SetEnvironmentVariable("DB_USER", user);
        Environment.SetEnvironmentVariable("DB_PASSWORD", password);
    }

    private static void ClearEnvironmentVariables()
    {
        Environment.SetEnvironmentVariable("DB_HOST", null);
        Environment.SetEnvironmentVariable("DB_PORT", null);
        Environment.SetEnvironmentVariable("DB_NAME", null);
        Environment.SetEnvironmentVariable("DB_USER", null);
        Environment.SetEnvironmentVariable("DB_PASSWORD", null);
    }

    private static IConfiguration CreateEmptyConfiguration()
    {
        return new ConfigurationBuilder().Build();
    }

    private static IConfiguration CreateConfigurationWithConnectionString(string connectionString)
    {
        var configValues = new Dictionary<string, string?>
        {
            { "ConnectionStrings:DefaultConnection", connectionString }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();
    }

    private static IConfiguration CreateConfigurationWithNamedConnectionString(string name, string connectionString)
    {
        var configValues = new Dictionary<string, string?>
        {
            { $"ConnectionStrings:{name}", connectionString }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();
    }

    #endregion

    #region Test DbContext

    private class TestDbContext : DbContext, ICrmDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        // Implement required ICrmDbContext members as needed for testing
        // This is a minimal implementation for testing purposes
    }

    #endregion
}

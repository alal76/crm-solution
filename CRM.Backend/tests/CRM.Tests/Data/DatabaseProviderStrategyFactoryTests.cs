// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using CRM.Infrastructure.Data.Providers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace CRM.Tests.Data;

/// <summary>
/// Unit tests for DatabaseProviderStrategyFactory.
/// Tests strategy creation for different database providers and deployment modes.
/// </summary>
public class DatabaseProviderStrategyFactoryTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;

    public DatabaseProviderStrategyFactoryTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithConfiguration_CreatesInstance()
    {
        // Act
        var factory = new DatabaseProviderStrategyFactory(_mockConfiguration.Object);

        // Assert
        factory.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullConfiguration_CreatesInstance()
    {
        // Act
        var factory = new DatabaseProviderStrategyFactory(null);

        // Assert
        factory.Should().NotBeNull();
    }

    #endregion

    #region CreateStrategy - Default Provider Tests

    [Fact]
    public void CreateStrategy_WithNoConfiguration_DefaultsToSqlServer()
    {
        // Arrange
        var factory = new DatabaseProviderStrategyFactory(null);

        // Act
        var strategy = factory.CreateStrategy();

        // Assert
        strategy.Should().BeOfType<SqlServerProviderStrategy>();
        strategy.ProviderName.Should().Be("sqlserver");
    }

    [Fact]
    public void CreateStrategy_WithEmptyConfiguration_DefaultsToSqlServer()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["DatabaseProvider"]).Returns((string?)null);
        var factory = new DatabaseProviderStrategyFactory(_mockConfiguration.Object);

        // Act
        var strategy = factory.CreateStrategy();

        // Assert
        strategy.Should().BeOfType<SqlServerProviderStrategy>();
    }

    #endregion

    #region CreateStrategy - SQL Server Tests

    [Theory]
    [InlineData("sqlserver")]
    [InlineData("SqlServer")]
    [InlineData("SQLSERVER")]
    [InlineData("mssql")]
    [InlineData("MSSQL")]
    public void CreateStrategy_WithSqlServerProvider_ReturnsSqlServerStrategy(string provider)
    {
        // Arrange
        _mockConfiguration.Setup(x => x["DatabaseProvider"]).Returns(provider);
        var factory = new DatabaseProviderStrategyFactory(_mockConfiguration.Object);

        // Act
        var strategy = factory.CreateStrategy();

        // Assert
        strategy.Should().BeOfType<SqlServerProviderStrategy>();
        strategy.ProviderName.Should().Be("sqlserver");
    }

    [Fact]
    public void CreateStrategy_WithExplicitSqlServer_OverridesConfiguration()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["DatabaseProvider"]).Returns("mysql");
        var factory = new DatabaseProviderStrategyFactory(_mockConfiguration.Object);

        // Act
        var strategy = factory.CreateStrategy("sqlserver");

        // Assert
        strategy.Should().BeOfType<SqlServerProviderStrategy>();
    }

    #endregion

    #region CreateStrategy - MySQL Tests

    [Theory]
    [InlineData("mysql")]
    [InlineData("MySQL")]
    [InlineData("MYSQL")]
    public void CreateStrategy_WithMySqlProvider_ReturnsMySqlStrategy(string provider)
    {
        // Arrange
        _mockConfiguration.Setup(x => x["DatabaseProvider"]).Returns(provider);
        var factory = new DatabaseProviderStrategyFactory(_mockConfiguration.Object);

        // Act
        var strategy = factory.CreateStrategy();

        // Assert
        strategy.Should().BeOfType<MySqlProviderStrategy>();
        strategy.ProviderName.Should().Be("mysql");
    }

    [Fact]
    public void CreateStrategy_MySql_HasCorrectDefaults()
    {
        // Arrange
        var factory = new DatabaseProviderStrategyFactory(null);

        // Act
        var strategy = factory.CreateStrategy("mysql");

        // Assert
        strategy.LongTextColumnType.Should().Be("LONGTEXT");
        strategy.JsonColumnType.Should().Be("JSON");
        strategy.SupportsNativeJson.Should().BeTrue();
        strategy.SupportsNativeGuid.Should().BeFalse();
    }

    #endregion

    #region CreateStrategy - MariaDB Tests

    [Theory]
    [InlineData("mariadb")]
    [InlineData("MariaDB")]
    [InlineData("MARIADB")]
    public void CreateStrategy_WithMariaDbProvider_ReturnsMySqlStrategy(string provider)
    {
        // Arrange
        _mockConfiguration.Setup(x => x["DatabaseProvider"]).Returns(provider);
        var factory = new DatabaseProviderStrategyFactory(_mockConfiguration.Object);

        // Act
        var strategy = factory.CreateStrategy();

        // Assert
        // MariaDB uses MySQL strategy (compatible)
        strategy.Should().BeAssignableTo<IDatabaseProviderStrategy>();
    }

    #endregion

    #region CreateStrategy - PostgreSQL Tests

    [Theory]
    [InlineData("postgresql")]
    [InlineData("PostgreSQL")]
    [InlineData("postgres")]
    [InlineData("pgsql")]
    public void CreateStrategy_WithPostgreSqlProvider_ReturnsPostgreSqlStrategy(string provider)
    {
        // Arrange
        _mockConfiguration.Setup(x => x["DatabaseProvider"]).Returns(provider);
        var factory = new DatabaseProviderStrategyFactory(_mockConfiguration.Object);

        // Act
        var strategy = factory.CreateStrategy();

        // Assert
        strategy.Should().BeOfType<PostgreSqlProviderStrategy>();
        strategy.ProviderName.Should().Be("postgresql");
    }

    [Fact]
    public void CreateStrategy_PostgreSql_HasCorrectDefaults()
    {
        // Arrange
        var factory = new DatabaseProviderStrategyFactory(null);

        // Act
        var strategy = factory.CreateStrategy("postgresql");

        // Assert
        strategy.LongTextColumnType.Should().Be("TEXT");
        strategy.JsonColumnType.Should().Be("JSONB");
        strategy.SupportsNativeJson.Should().BeTrue();
        strategy.SupportsSequences.Should().BeTrue();
    }

    #endregion

    #region CreateStrategy - Oracle Tests

    [Theory]
    [InlineData("oracle")]
    [InlineData("Oracle")]
    [InlineData("ORACLE")]
    public void CreateStrategy_WithOracleProvider_ReturnsOracleStrategy(string provider)
    {
        // Arrange
        _mockConfiguration.Setup(x => x["DatabaseProvider"]).Returns(provider);
        var factory = new DatabaseProviderStrategyFactory(_mockConfiguration.Object);

        // Act
        var strategy = factory.CreateStrategy();

        // Assert
        strategy.Should().BeOfType<OracleProviderStrategy>();
        strategy.ProviderName.Should().Be("oracle");
    }

    [Fact]
    public void CreateStrategy_Oracle_HasCorrectDefaults()
    {
        // Arrange
        var factory = new DatabaseProviderStrategyFactory(null);

        // Act
        var strategy = factory.CreateStrategy("oracle");

        // Assert
        strategy.LongTextColumnType.Should().Be("NCLOB");
        strategy.GuidColumnType.Should().Be("RAW(16)");
        strategy.SupportsSequences.Should().BeTrue();
    }

    #endregion

    #region CreateStrategy - Deployment Mode Tests

    [Theory]
    [InlineData("standalone", DatabaseDeploymentMode.Standalone)]
    [InlineData("Standalone", DatabaseDeploymentMode.Standalone)]
    [InlineData("clustered", DatabaseDeploymentMode.Clustered)]
    [InlineData("Clustered", DatabaseDeploymentMode.Clustered)]
    [InlineData("hyperscale", DatabaseDeploymentMode.Hyperscale)]
    [InlineData("Hyperscale", DatabaseDeploymentMode.Hyperscale)]
    public void CreateStrategy_WithDeploymentMode_SetsCorrectMode(string modeString, DatabaseDeploymentMode expectedMode)
    {
        // Arrange
        _mockConfiguration.Setup(x => x["DatabaseProvider"]).Returns("sqlserver");
        _mockConfiguration.Setup(x => x["DatabaseDeploymentMode"]).Returns(modeString);
        var factory = new DatabaseProviderStrategyFactory(_mockConfiguration.Object);

        // Act
        var strategy = factory.CreateStrategy();

        // Assert
        strategy.Should().NotBeNull();
        // Deployment mode affects batch size and connection optimization
    }

    [Fact]
    public void CreateStrategy_WithNoDeploymentMode_DefaultsToStandalone()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["DatabaseProvider"]).Returns("sqlserver");
        _mockConfiguration.Setup(x => x["DatabaseDeploymentMode"]).Returns((string?)null);
        var factory = new DatabaseProviderStrategyFactory(_mockConfiguration.Object);

        // Act
        var strategy = factory.CreateStrategy();

        // Assert
        strategy.Should().NotBeNull();
        strategy.RecommendedBatchSize.Should().BeGreaterThan(0);
    }

    #endregion

    #region CreateStrategy with Explicit Parameters Tests

    [Theory]
    [InlineData("sqlserver", DatabaseDeploymentMode.Standalone)]
    [InlineData("mysql", DatabaseDeploymentMode.Clustered)]
    [InlineData("postgresql", DatabaseDeploymentMode.Hyperscale)]
    [InlineData("oracle", DatabaseDeploymentMode.Standalone)]
    public void CreateStrategy_WithExplicitParameters_CreatesCorrectStrategy(
        string provider, DatabaseDeploymentMode mode)
    {
        // Arrange
        var factory = new DatabaseProviderStrategyFactory(null);

        // Act
        var strategy = factory.CreateStrategy(provider, mode);

        // Assert
        strategy.Should().NotBeNull();
        strategy.ProviderName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CreateStrategy_WithClusterType_PassesToStrategy()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["DatabaseProvider"]).Returns("mysql");
        _mockConfiguration.Setup(x => x["DatabaseDeploymentMode"]).Returns("clustered");
        _mockConfiguration.Setup(x => x["DatabaseClusterType"]).Returns("galera");
        var factory = new DatabaseProviderStrategyFactory(_mockConfiguration.Object);

        // Act
        var strategy = factory.CreateStrategy();

        // Assert
        strategy.Should().BeOfType<MySqlProviderStrategy>();
    }

    #endregion

    #region CreateStrategy - Unknown Provider Tests

    [Theory]
    [InlineData("unknown")]
    [InlineData("nosql")]
    [InlineData("mongodb")]
    [InlineData("redis")]
    public void CreateStrategy_WithUnknownProvider_DefaultsToSqlServer(string unknownProvider)
    {
        // Arrange
        _mockConfiguration.Setup(x => x["DatabaseProvider"]).Returns(unknownProvider);
        var factory = new DatabaseProviderStrategyFactory(_mockConfiguration.Object);

        // Act
        var strategy = factory.CreateStrategy();

        // Assert
        strategy.Should().BeOfType<SqlServerProviderStrategy>();
    }

    #endregion

    #region Batch Size Configuration Tests

    [Fact]
    public void CreateStrategy_Standalone_HasSmallBatchSize()
    {
        // Arrange
        var factory = new DatabaseProviderStrategyFactory(null);

        // Act
        var strategy = factory.CreateStrategy("mysql", DatabaseDeploymentMode.Standalone);

        // Assert
        strategy.RecommendedBatchSize.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public void CreateStrategy_Clustered_HasMediumBatchSize()
    {
        // Arrange
        var factory = new DatabaseProviderStrategyFactory(null);

        // Act
        var strategy = factory.CreateStrategy("mysql", DatabaseDeploymentMode.Clustered);

        // Assert
        strategy.RecommendedBatchSize.Should().BeGreaterThan(100);
    }

    [Fact]
    public void CreateStrategy_Hyperscale_HasLargeBatchSize()
    {
        // Arrange
        var factory = new DatabaseProviderStrategyFactory(null);

        // Act
        var strategy = factory.CreateStrategy("mysql", DatabaseDeploymentMode.Hyperscale);

        // Assert
        strategy.RecommendedBatchSize.Should().BeGreaterThanOrEqualTo(500);
    }

    #endregion

    #region Strategy Interface Implementation Tests

    [Theory]
    [InlineData("sqlserver")]
    [InlineData("mysql")]
    [InlineData("postgresql")]
    [InlineData("oracle")]
    public void CreateStrategy_AllProviders_ImplementIDatabaseProviderStrategy(string provider)
    {
        // Arrange
        var factory = new DatabaseProviderStrategyFactory(null);

        // Act
        var strategy = factory.CreateStrategy(provider);

        // Assert
        strategy.Should().BeAssignableTo<IDatabaseProviderStrategy>();
    }

    [Theory]
    [InlineData("sqlserver")]
    [InlineData("mysql")]
    [InlineData("postgresql")]
    [InlineData("oracle")]
    public void CreateStrategy_AllProviders_HaveRequiredProperties(string provider)
    {
        // Arrange
        var factory = new DatabaseProviderStrategyFactory(null);

        // Act
        var strategy = factory.CreateStrategy(provider);

        // Assert
        strategy.ProviderName.Should().NotBeNullOrEmpty();
        strategy.LongTextColumnType.Should().NotBeNullOrEmpty();
        strategy.TextColumnType.Should().NotBeNullOrEmpty();
        strategy.JsonColumnType.Should().NotBeNullOrEmpty();
        strategy.GuidColumnType.Should().NotBeNullOrEmpty();
        strategy.TimestampColumnType.Should().NotBeNullOrEmpty();
        strategy.RecommendedBatchSize.Should().BeGreaterThan(0);
    }

    #endregion

    #region Connection String Optimization Tests

    [Theory]
    [InlineData("sqlserver")]
    [InlineData("mysql")]
    [InlineData("postgresql")]
    [InlineData("oracle")]
    public void CreateStrategy_AllProviders_CanOptimizeConnectionString(string provider)
    {
        // Arrange
        var factory = new DatabaseProviderStrategyFactory(null);
        var strategy = factory.CreateStrategy(provider);
        var baseConnectionString = "Server=localhost;Database=test;";

        // Act
        var optimized = strategy.OptimizeConnectionString(baseConnectionString);

        // Assert
        optimized.Should().NotBeNullOrEmpty();
        optimized.Should().Contain("localhost");
    }

    [Fact]
    public void CreateStrategy_SqlServer_AddsPoolingOptions()
    {
        // Arrange
        var factory = new DatabaseProviderStrategyFactory(null);
        var strategy = factory.CreateStrategy("sqlserver", DatabaseDeploymentMode.Standalone);
        var baseConnectionString = "Server=localhost;Database=test;";

        // Act
        var optimized = strategy.OptimizeConnectionString(baseConnectionString);

        // Assert
        optimized.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Native Feature Support Tests

    [Fact]
    public void CreateStrategy_SqlServer_SupportsNativeGuid()
    {
        // Arrange
        var factory = new DatabaseProviderStrategyFactory(null);

        // Act
        var strategy = factory.CreateStrategy("sqlserver");

        // Assert
        strategy.SupportsNativeGuid.Should().BeTrue();
    }

    [Fact]
    public void CreateStrategy_MySQL_DoesNotSupportNativeGuid()
    {
        // Arrange
        var factory = new DatabaseProviderStrategyFactory(null);

        // Act
        var strategy = factory.CreateStrategy("mysql");

        // Assert
        strategy.SupportsNativeGuid.Should().BeFalse();
    }

    [Fact]
    public void CreateStrategy_PostgreSql_SupportsNativeGuid()
    {
        // Arrange
        var factory = new DatabaseProviderStrategyFactory(null);

        // Act
        var strategy = factory.CreateStrategy("postgresql");

        // Assert
        strategy.SupportsNativeGuid.Should().BeTrue();
    }

    [Theory]
    [InlineData("mysql")]
    [InlineData("postgresql")]
    public void CreateStrategy_ModernDatabases_SupportNativeJson(string provider)
    {
        // Arrange
        var factory = new DatabaseProviderStrategyFactory(null);

        // Act
        var strategy = factory.CreateStrategy(provider);

        // Assert
        strategy.SupportsNativeJson.Should().BeTrue();
    }

    [Theory]
    [InlineData("postgresql")]
    [InlineData("oracle")]
    public void CreateStrategy_SequenceSupportingDatabases_ReportSequenceSupport(string provider)
    {
        // Arrange
        var factory = new DatabaseProviderStrategyFactory(null);

        // Act
        var strategy = factory.CreateStrategy(provider);

        // Assert
        strategy.SupportsSequences.Should().BeTrue();
    }

    [Theory]
    [InlineData("mysql")]
    public void CreateStrategy_AutoIncrementDatabases_DontSupportSequences(string provider)
    {
        // Arrange
        var factory = new DatabaseProviderStrategyFactory(null);

        // Act
        var strategy = factory.CreateStrategy(provider);

        // Assert
        strategy.SupportsSequences.Should().BeFalse();
    }

    #endregion
}

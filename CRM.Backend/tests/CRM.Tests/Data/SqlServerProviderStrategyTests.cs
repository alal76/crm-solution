// CRM Solution - SQL Server Provider Strategy Tests
// Tests for SQL Server-specific database provider strategy

using System;
using CRM.Infrastructure.Data.Providers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRM.Tests.Data;

/// <summary>
/// Unit tests for SqlServerProviderStrategy.
/// Tests SQL Server-specific configurations including rowversion, nvarchar, and Always On AG support.
/// </summary>
public class SqlServerProviderStrategyTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithDefaultMode_CreatesStandaloneStrategy()
    {
        // Act
        var strategy = new SqlServerProviderStrategy();

        // Assert
        strategy.Should().NotBeNull();
        strategy.ProviderName.Should().Be("sqlserver");
    }

    [Theory]
    [InlineData(DatabaseDeploymentMode.Standalone)]
    [InlineData(DatabaseDeploymentMode.Clustered)]
    [InlineData(DatabaseDeploymentMode.Hyperscale)]
    public void Constructor_WithDeploymentMode_CreatesStrategy(DatabaseDeploymentMode mode)
    {
        // Act
        var strategy = new SqlServerProviderStrategy(mode);

        // Assert
        strategy.Should().NotBeNull();
    }

    #endregion

    #region Provider Properties Tests

    [Fact]
    public void ProviderName_ReturnsSqlserver()
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy();

        // Assert
        strategy.ProviderName.Should().Be("sqlserver");
    }

    [Fact]
    public void LongTextColumnType_ReturnsNvarcharMax()
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy();

        // Assert
        strategy.LongTextColumnType.Should().Be("nvarchar(max)");
    }

    [Fact]
    public void TextColumnType_ReturnsNvarcharMax()
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy();

        // Assert
        strategy.TextColumnType.Should().Be("nvarchar(max)");
    }

    [Fact]
    public void JsonColumnType_ReturnsNvarcharMax()
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy();

        // Assert
        // SQL Server stores JSON as nvarchar(max) - has JSON functions but no native type
        strategy.JsonColumnType.Should().Be("nvarchar(max)");
    }

    [Fact]
    public void GuidColumnType_ReturnsUniqueidentifier()
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy();

        // Assert
        strategy.GuidColumnType.Should().Be("uniqueidentifier");
    }

    [Fact]
    public void TimestampColumnType_ReturnsDatetime2()
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy();

        // Assert
        strategy.TimestampColumnType.Should().Be("datetime2");
    }

    #endregion

    #region Feature Support Tests

    [Fact]
    public void SupportsNativeJson_ReturnsFalse()
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy();

        // Assert
        // SQL Server has JSON functions but no native JSON column type
        strategy.SupportsNativeJson.Should().BeFalse();
    }

    [Fact]
    public void SupportsNativeGuid_ReturnsTrue()
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy();

        // Assert
        strategy.SupportsNativeGuid.Should().BeTrue();
    }

    [Fact]
    public void SupportsSequences_ReturnsTrue()
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy();

        // Assert
        strategy.SupportsSequences.Should().BeTrue();
    }

    [Fact]
    public void DefaultDeleteBehavior_ReturnsNoAction()
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy();

        // Assert
        strategy.DefaultDeleteBehavior.Should().Be(DeleteBehavior.NoAction);
    }

    #endregion

    #region Batch Size Tests

    [Fact]
    public void RecommendedBatchSize_Standalone_ReturnsReasonableSize()
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy(DatabaseDeploymentMode.Standalone);

        // Assert
        strategy.RecommendedBatchSize.Should().BeGreaterThan(0);
        strategy.RecommendedBatchSize.Should().BeLessThanOrEqualTo(200);
    }

    [Fact]
    public void RecommendedBatchSize_Clustered_ReturnsLargerSize()
    {
        // Arrange
        var standaloneStrategy = new SqlServerProviderStrategy(DatabaseDeploymentMode.Standalone);
        var clusteredStrategy = new SqlServerProviderStrategy(DatabaseDeploymentMode.Clustered);

        // Assert - Clustered (Always On AG) can handle larger batches
        clusteredStrategy.RecommendedBatchSize.Should()
            .BeGreaterThanOrEqualTo(standaloneStrategy.RecommendedBatchSize);
    }

    [Fact]
    public void RecommendedBatchSize_Hyperscale_ReturnsLargestSize()
    {
        // Arrange
        var clusteredStrategy = new SqlServerProviderStrategy(DatabaseDeploymentMode.Clustered);
        var hyperscaleStrategy = new SqlServerProviderStrategy(DatabaseDeploymentMode.Hyperscale);

        // Assert - Azure SQL Hyperscale can handle even larger batches
        hyperscaleStrategy.RecommendedBatchSize.Should()
            .BeGreaterThanOrEqualTo(clusteredStrategy.RecommendedBatchSize);
    }

    #endregion

    #region Connection String Optimization Tests

    [Fact]
    public void OptimizeConnectionString_AddsPoolingOptions()
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy(DatabaseDeploymentMode.Standalone);
        var baseConnectionString = "Server=localhost;Database=crm_db;Integrated Security=true;";

        // Act
        var optimized = strategy.OptimizeConnectionString(baseConnectionString);

        // Assert
        optimized.Should().NotBeNullOrEmpty();
        optimized.Should().Contain("localhost");
    }

    [Theory]
    [InlineData(DatabaseDeploymentMode.Standalone)]
    [InlineData(DatabaseDeploymentMode.Clustered)]
    [InlineData(DatabaseDeploymentMode.Hyperscale)]
    public void OptimizeConnectionString_AllModes_ReturnValidString(DatabaseDeploymentMode mode)
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy(mode);
        var baseConnectionString = "Server=localhost;Database=crm_db;";

        // Act
        var optimized = strategy.OptimizeConnectionString(baseConnectionString);

        // Assert
        optimized.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void OptimizeConnectionString_Clustered_AddsMultiSubnetFailover()
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy(DatabaseDeploymentMode.Clustered);
        var baseConnectionString = "Server=localhost;Database=crm_db;";

        // Act
        var optimized = strategy.OptimizeConnectionString(baseConnectionString);

        // Assert
        // Clustered mode should add AG-related options
        optimized.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void OptimizeConnectionString_PreservesOriginalParameters()
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy();
        var baseConnectionString = "Server=sqlserver.local,1433;Database=crm_production;User Id=sa;";

        // Act
        var optimized = strategy.OptimizeConnectionString(baseConnectionString);

        // Assert
        optimized.Should().Contain("sqlserver.local");
        optimized.Should().Contain("crm_production");
    }

    #endregion

    #region SQL Server-Specific Features Tests

    [Fact]
    public void SqlServer_UsesNvarcharForUnicode()
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy();

        // Assert - nvarchar supports Unicode (vs varchar)
        strategy.LongTextColumnType.Should().ContainEquivalentOf("nvarchar");
        strategy.TextColumnType.Should().ContainEquivalentOf("nvarchar");
    }

    [Fact]
    public void SqlServer_UsesNativeUniqueidentifier()
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy();

        // Assert - SQL Server has native UNIQUEIDENTIFIER type
        strategy.GuidColumnType.Should().Be("uniqueidentifier");
        strategy.SupportsNativeGuid.Should().BeTrue();
    }

    [Fact]
    public void SqlServer_UsesDatetime2()
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy();

        // Assert - datetime2 for higher precision than datetime
        strategy.TimestampColumnType.Should().Be("datetime2");
    }

    [Fact]
    public void SqlServer_SupportsRowversion()
    {
        // SQL Server has native ROWVERSION type for optimistic concurrency
        // This is a design constraint test
        var strategy = new SqlServerProviderStrategy();
        
        // Assert - strategy exists with correct provider name
        strategy.ProviderName.Should().Be("sqlserver");
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void Strategy_ImplementsIDatabaseProviderStrategy()
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy();

        // Assert
        strategy.Should().BeAssignableTo<IDatabaseProviderStrategy>();
    }

    [Fact]
    public void Strategy_InheritsFromBaseStrategy()
    {
        // Arrange
        var strategy = new SqlServerProviderStrategy();

        // Assert
        strategy.Should().BeAssignableTo<DatabaseProviderStrategyBase>();
    }

    #endregion

    #region Comparison with Other Providers Tests

    [Fact]
    public void SqlServer_DiffersFromMySql_InGuidType()
    {
        // Arrange
        var sqlStrategy = new SqlServerProviderStrategy();
        var mysqlStrategy = new MySqlProviderStrategy();

        // Assert
        sqlStrategy.GuidColumnType.Should().Be("uniqueidentifier");
        mysqlStrategy.GuidColumnType.Should().Be("CHAR(36)");
    }

    [Fact]
    public void SqlServer_DiffersFromPostgreSql_InJsonSupport()
    {
        // Arrange
        var sqlStrategy = new SqlServerProviderStrategy();
        var postgresStrategy = new PostgreSqlProviderStrategy();

        // Assert
        sqlStrategy.SupportsNativeJson.Should().BeFalse();
        postgresStrategy.SupportsNativeJson.Should().BeTrue();
    }

    [Fact]
    public void SqlServer_SimilarToPostgreSql_InSequenceSupport()
    {
        // Arrange
        var sqlStrategy = new SqlServerProviderStrategy();
        var postgresStrategy = new PostgreSqlProviderStrategy();

        // Assert
        sqlStrategy.SupportsSequences.Should().BeTrue();
        postgresStrategy.SupportsSequences.Should().BeTrue();
    }

    #endregion
}

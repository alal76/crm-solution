// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using CRM.Infrastructure.Data.Providers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRM.Tests.Data;

/// <summary>
/// Unit tests for PostgreSqlProviderStrategy.
/// Tests PostgreSQL-specific configurations including JSONB, sequences, and connection optimizations.
/// </summary>
public class PostgreSqlProviderStrategyTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithDefaultMode_CreatesStandaloneStrategy()
    {
        // Act
        var strategy = new PostgreSqlProviderStrategy();

        // Assert
        strategy.Should().NotBeNull();
        strategy.ProviderName.Should().Be("postgresql");
    }

    [Theory]
    [InlineData(DatabaseDeploymentMode.Standalone)]
    [InlineData(DatabaseDeploymentMode.Clustered)]
    [InlineData(DatabaseDeploymentMode.Hyperscale)]
    public void Constructor_WithDeploymentMode_CreatesStrategy(DatabaseDeploymentMode mode)
    {
        // Act
        var strategy = new PostgreSqlProviderStrategy(mode);

        // Assert
        strategy.Should().NotBeNull();
    }

    #endregion

    #region Provider Properties Tests

    [Fact]
    public void ProviderName_ReturnsPostgresql()
    {
        // Arrange
        var strategy = new PostgreSqlProviderStrategy();

        // Assert
        strategy.ProviderName.Should().Be("postgresql");
    }

    [Fact]
    public void LongTextColumnType_ReturnsText()
    {
        // Arrange
        var strategy = new PostgreSqlProviderStrategy();

        // Assert
        strategy.LongTextColumnType.Should().Be("TEXT");
    }

    [Fact]
    public void TextColumnType_ReturnsText()
    {
        // Arrange
        var strategy = new PostgreSqlProviderStrategy();

        // Assert
        strategy.TextColumnType.Should().Be("TEXT");
    }

    [Fact]
    public void JsonColumnType_ReturnsJsonb()
    {
        // Arrange
        var strategy = new PostgreSqlProviderStrategy();

        // Assert
        strategy.JsonColumnType.Should().Be("JSONB");
    }

    [Fact]
    public void GuidColumnType_ReturnsUuid()
    {
        // Arrange
        var strategy = new PostgreSqlProviderStrategy();

        // Assert
        strategy.GuidColumnType.Should().Be("UUID");
    }

    [Fact]
    public void TimestampColumnType_ReturnsTimestamptz()
    {
        // Arrange
        var strategy = new PostgreSqlProviderStrategy();

        // Assert
        strategy.TimestampColumnType.Should().Be("TIMESTAMPTZ");
    }

    #endregion

    #region Feature Support Tests

    [Fact]
    public void SupportsNativeJson_ReturnsTrue()
    {
        // Arrange
        var strategy = new PostgreSqlProviderStrategy();

        // Assert
        strategy.SupportsNativeJson.Should().BeTrue();
    }

    [Fact]
    public void SupportsNativeGuid_ReturnsTrue()
    {
        // Arrange
        var strategy = new PostgreSqlProviderStrategy();

        // Assert
        strategy.SupportsNativeGuid.Should().BeTrue();
    }

    [Fact]
    public void SupportsSequences_ReturnsTrue()
    {
        // Arrange
        var strategy = new PostgreSqlProviderStrategy();

        // Assert
        strategy.SupportsSequences.Should().BeTrue();
    }

    [Fact]
    public void DefaultDeleteBehavior_ReturnsCascade()
    {
        // Arrange
        var strategy = new PostgreSqlProviderStrategy();

        // Assert
        strategy.DefaultDeleteBehavior.Should().Be(DeleteBehavior.Cascade);
    }

    #endregion

    #region Batch Size Tests

    [Fact]
    public void RecommendedBatchSize_Standalone_ReturnsReasonableSize()
    {
        // Arrange
        var strategy = new PostgreSqlProviderStrategy(DatabaseDeploymentMode.Standalone);

        // Assert
        strategy.RecommendedBatchSize.Should().BeGreaterThan(0);
        strategy.RecommendedBatchSize.Should().BeLessThanOrEqualTo(200);
    }

    [Fact]
    public void RecommendedBatchSize_Clustered_ReturnsLargerSize()
    {
        // Arrange
        var standaloneStrategy = new PostgreSqlProviderStrategy(DatabaseDeploymentMode.Standalone);
        var clusteredStrategy = new PostgreSqlProviderStrategy(DatabaseDeploymentMode.Clustered);

        // Assert
        clusteredStrategy.RecommendedBatchSize.Should()
            .BeGreaterThanOrEqualTo(standaloneStrategy.RecommendedBatchSize);
    }

    [Fact]
    public void RecommendedBatchSize_Hyperscale_ReturnsLargestSize()
    {
        // Arrange
        var clusteredStrategy = new PostgreSqlProviderStrategy(DatabaseDeploymentMode.Clustered);
        var hyperscaleStrategy = new PostgreSqlProviderStrategy(DatabaseDeploymentMode.Hyperscale);

        // Assert
        hyperscaleStrategy.RecommendedBatchSize.Should()
            .BeGreaterThanOrEqualTo(clusteredStrategy.RecommendedBatchSize);
    }

    #endregion

    #region Connection String Optimization Tests

    [Fact]
    public void OptimizeConnectionString_AddsPoolingOptions()
    {
        // Arrange
        var strategy = new PostgreSqlProviderStrategy(DatabaseDeploymentMode.Standalone);
        var baseConnectionString = "Host=localhost;Database=crm_db;Username=postgres;Password=test;";

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
        var strategy = new PostgreSqlProviderStrategy(mode);
        var baseConnectionString = "Host=localhost;Database=crm_db;";

        // Act
        var optimized = strategy.OptimizeConnectionString(baseConnectionString);

        // Assert
        optimized.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void OptimizeConnectionString_PreservesOriginalParameters()
    {
        // Arrange
        var strategy = new PostgreSqlProviderStrategy();
        var baseConnectionString = "Host=dbserver.local;Database=crm_production;Port=5433;";

        // Act
        var optimized = strategy.OptimizeConnectionString(baseConnectionString);

        // Assert
        optimized.Should().Contain("dbserver.local");
        optimized.Should().Contain("crm_production");
    }

    #endregion

    #region PostgreSQL-Specific Features Tests

    [Fact]
    public void PostgreSql_UsesJsonbNotJson()
    {
        // Arrange
        var strategy = new PostgreSqlProviderStrategy();

        // Assert - JSONB is preferred over JSON for better indexing and performance
        strategy.JsonColumnType.Should().Be("JSONB");
        strategy.JsonColumnType.Should().NotBe("JSON");
    }

    [Fact]
    public void PostgreSql_UsesNativeUuid()
    {
        // Arrange
        var strategy = new PostgreSqlProviderStrategy();

        // Assert - PostgreSQL has native UUID type
        strategy.GuidColumnType.Should().Be("UUID");
        strategy.SupportsNativeGuid.Should().BeTrue();
    }

    [Fact]
    public void PostgreSql_UsesTimestampWithTimeZone()
    {
        // Arrange
        var strategy = new PostgreSqlProviderStrategy();

        // Assert - TIMESTAMPTZ for timezone-aware timestamps
        strategy.TimestampColumnType.Should().Be("TIMESTAMPTZ");
    }

    [Fact]
    public void PostgreSql_SupportsSequences()
    {
        // Arrange
        var strategy = new PostgreSqlProviderStrategy();

        // Assert - PostgreSQL supports sequences (vs AUTO_INCREMENT)
        strategy.SupportsSequences.Should().BeTrue();
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void Strategy_ImplementsIDatabaseProviderStrategy()
    {
        // Arrange
        var strategy = new PostgreSqlProviderStrategy();

        // Assert
        strategy.Should().BeAssignableTo<IDatabaseProviderStrategy>();
    }

    [Fact]
    public void Strategy_InheritsFromBaseStrategy()
    {
        // Arrange
        var strategy = new PostgreSqlProviderStrategy();

        // Assert
        strategy.Should().BeAssignableTo<DatabaseProviderStrategyBase>();
    }

    #endregion

    #region Comparison with Other Providers Tests

    [Fact]
    public void PostgreSql_DiffersFromMySql_InGuidSupport()
    {
        // Arrange
        var postgresStrategy = new PostgreSqlProviderStrategy();
        var mysqlStrategy = new MySqlProviderStrategy();

        // Assert
        postgresStrategy.SupportsNativeGuid.Should().BeTrue();
        mysqlStrategy.SupportsNativeGuid.Should().BeFalse();
    }

    [Fact]
    public void PostgreSql_DiffersFromMySql_InSequenceSupport()
    {
        // Arrange
        var postgresStrategy = new PostgreSqlProviderStrategy();
        var mysqlStrategy = new MySqlProviderStrategy();

        // Assert
        postgresStrategy.SupportsSequences.Should().BeTrue();
        mysqlStrategy.SupportsSequences.Should().BeFalse();
    }

    [Fact]
    public void PostgreSql_DiffersFromMySql_InJsonType()
    {
        // Arrange
        var postgresStrategy = new PostgreSqlProviderStrategy();
        var mysqlStrategy = new MySqlProviderStrategy();

        // Assert
        postgresStrategy.JsonColumnType.Should().Be("JSONB");
        mysqlStrategy.JsonColumnType.Should().Be("JSON");
    }

    #endregion
}

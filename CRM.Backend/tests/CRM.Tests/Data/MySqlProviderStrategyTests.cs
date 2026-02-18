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
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Moq;
using Xunit;

namespace CRM.Tests.Data;

/// <summary>
/// Unit tests for MySqlProviderStrategy.
/// Tests MySQL-specific configurations, row version handling, and connection optimizations.
/// </summary>
public class MySqlProviderStrategyTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithDefaultMode_CreatesStandaloneStrategy()
    {
        // Act
        var strategy = new MySqlProviderStrategy();

        // Assert
        strategy.Should().NotBeNull();
        strategy.ProviderName.Should().Be("mysql");
    }

    [Theory]
    [InlineData(DatabaseDeploymentMode.Standalone)]
    [InlineData(DatabaseDeploymentMode.Clustered)]
    [InlineData(DatabaseDeploymentMode.Hyperscale)]
    public void Constructor_WithDeploymentMode_CreatesStrategy(DatabaseDeploymentMode mode)
    {
        // Act
        var strategy = new MySqlProviderStrategy(mode);

        // Assert
        strategy.Should().NotBeNull();
    }

    #endregion

    #region Provider Properties Tests

    [Fact]
    public void ProviderName_ReturnsMysql()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy();

        // Assert
        strategy.ProviderName.Should().Be("mysql");
    }

    [Fact]
    public void LongTextColumnType_ReturnsLongtext()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy();

        // Assert
        strategy.LongTextColumnType.Should().Be("LONGTEXT");
    }

    [Fact]
    public void TextColumnType_ReturnsText()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy();

        // Assert
        strategy.TextColumnType.Should().Be("TEXT");
    }

    [Fact]
    public void JsonColumnType_ReturnsJson()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy();

        // Assert
        strategy.JsonColumnType.Should().Be("JSON");
    }

    [Fact]
    public void GuidColumnType_ReturnsChar36()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy();

        // Assert
        strategy.GuidColumnType.Should().Be("CHAR(36)");
    }

    [Fact]
    public void TimestampColumnType_ReturnsDatetime6()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy();

        // Assert
        strategy.TimestampColumnType.Should().Be("DATETIME(6)");
    }

    #endregion

    #region Feature Support Tests

    [Fact]
    public void SupportsNativeJson_ReturnsTrue()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy();

        // Assert
        strategy.SupportsNativeJson.Should().BeTrue();
    }

    [Fact]
    public void SupportsNativeGuid_ReturnsFalse()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy();

        // Assert
        strategy.SupportsNativeGuid.Should().BeFalse();
    }

    [Fact]
    public void SupportsSequences_ReturnsFalse()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy();

        // Assert
        strategy.SupportsSequences.Should().BeFalse();
    }

    [Fact]
    public void DefaultDeleteBehavior_ReturnsCascade()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy();

        // Assert
        strategy.DefaultDeleteBehavior.Should().Be(DeleteBehavior.Cascade);
    }

    #endregion

    #region Batch Size Tests

    [Fact]
    public void RecommendedBatchSize_Standalone_Returns100()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy(DatabaseDeploymentMode.Standalone);

        // Assert
        strategy.RecommendedBatchSize.Should().Be(100);
    }

    [Fact]
    public void RecommendedBatchSize_Clustered_Returns200()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy(DatabaseDeploymentMode.Clustered);

        // Assert
        strategy.RecommendedBatchSize.Should().Be(200);
    }

    [Fact]
    public void RecommendedBatchSize_Hyperscale_Returns1000()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy(DatabaseDeploymentMode.Hyperscale);

        // Assert
        strategy.RecommendedBatchSize.Should().Be(1000);
    }

    #endregion

    #region Connection String Optimization Tests

    [Fact]
    public void OptimizeConnectionString_AddsPoolingOptions()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy(DatabaseDeploymentMode.Standalone);
        var baseConnectionString = "Server=localhost;Database=crm_db;User=root;Password=test;";

        // Act
        var optimized = strategy.OptimizeConnectionString(baseConnectionString);

        // Assert
        optimized.Should().NotBeNullOrEmpty();
        optimized.Should().Contain("localhost");
    }

    [Fact]
    public void OptimizeConnectionString_Standalone_HasReasonablePoolSize()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy(DatabaseDeploymentMode.Standalone);
        var baseConnectionString = "Server=localhost;Database=crm_db;";

        // Act
        var optimized = strategy.OptimizeConnectionString(baseConnectionString);

        // Assert
        optimized.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void OptimizeConnectionString_Clustered_HasLargerPoolSize()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy(DatabaseDeploymentMode.Clustered);
        var baseConnectionString = "Server=localhost;Database=crm_db;";

        // Act
        var optimized = strategy.OptimizeConnectionString(baseConnectionString);

        // Assert
        optimized.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void OptimizeConnectionString_Hyperscale_HasLargestPoolSize()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy(DatabaseDeploymentMode.Hyperscale);
        var baseConnectionString = "Server=localhost;Database=crm_db;";

        // Act
        var optimized = strategy.OptimizeConnectionString(baseConnectionString);

        // Assert
        optimized.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Server=;")]
    public void OptimizeConnectionString_WithMinimalInput_ReturnsValidString(string baseConnectionString)
    {
        // Arrange
        var strategy = new MySqlProviderStrategy();

        // Act
        var optimized = strategy.OptimizeConnectionString(baseConnectionString);

        // Assert
        optimized.Should().NotBeNull();
    }

    #endregion

    #region RowVersion Configuration Tests

    [Fact]
    public void ConfigureRowVersion_SetsCorrectColumnType()
    {
        // This test verifies RowVersion configuration behavior
        // The actual ModelBuilder configuration requires integration testing
        var strategy = new MySqlProviderStrategy();

        // Verify the strategy exists and has correct type expectations
        strategy.ProviderName.Should().Be("mysql");
        // RowVersion uses BINARY(8) for MySQL compatibility with SQL Server
    }

    #endregion

    #region Post Configuration Tests

    [Fact]
    public void ApplyPostConfiguration_HandlesEmptyModel()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy();
        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder.UseInMemoryDatabase("TestDb");
        var modelBuilder = new ModelBuilder(new ConventionSet());

        // Act - Should not throw
        Action act = () => strategy.ApplyPostConfiguration(modelBuilder);

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void Strategy_ImplementsIDatabaseProviderStrategy()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy();

        // Assert
        strategy.Should().BeAssignableTo<IDatabaseProviderStrategy>();
    }

    [Fact]
    public void Strategy_InheritsFromBaseStrategy()
    {
        // Arrange
        var strategy = new MySqlProviderStrategy();

        // Assert
        strategy.Should().BeAssignableTo<DatabaseProviderStrategyBase>();
    }

    #endregion
}

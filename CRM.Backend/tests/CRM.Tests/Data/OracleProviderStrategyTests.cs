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

using System;
using CRM.Infrastructure.Data.Providers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRM.Tests.Data;

/// <summary>
/// Unit tests for OracleProviderStrategy.
/// Tests Oracle-specific configurations including CLOB, RAW(16) GUIDs, and RAC support.
/// </summary>
public class OracleProviderStrategyTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithDefaultMode_CreatesStandaloneStrategy()
    {
        // Act
        var strategy = new OracleProviderStrategy();

        // Assert
        strategy.Should().NotBeNull();
        strategy.ProviderName.Should().Be("oracle");
    }

    [Theory]
    [InlineData(DatabaseDeploymentMode.Standalone)]
    [InlineData(DatabaseDeploymentMode.Clustered)]
    [InlineData(DatabaseDeploymentMode.Hyperscale)]
    public void Constructor_WithDeploymentMode_CreatesStrategy(DatabaseDeploymentMode mode)
    {
        // Act
        var strategy = new OracleProviderStrategy(mode);

        // Assert
        strategy.Should().NotBeNull();
    }

    #endregion

    #region Provider Properties Tests

    [Fact]
    public void ProviderName_ReturnsOracle()
    {
        // Arrange
        var strategy = new OracleProviderStrategy();

        // Assert
        strategy.ProviderName.Should().Be("oracle");
    }

    [Fact]
    public void LongTextColumnType_ReturnsNclob()
    {
        // Arrange
        var strategy = new OracleProviderStrategy();

        // Assert
        strategy.LongTextColumnType.Should().Be("NCLOB");
    }

    [Fact]
    public void TextColumnType_ReturnsVarchar2()
    {
        // Arrange
        var strategy = new OracleProviderStrategy();

        // Assert
        strategy.TextColumnType.Should().Contain("VARCHAR2");
    }

    [Fact]
    public void JsonColumnType_ReturnsNclob()
    {
        // Arrange
        var strategy = new OracleProviderStrategy();

        // Assert
        // Oracle 12c+ has JSON support, using NCLOB for Unicode
        strategy.JsonColumnType.Should().Be("NCLOB");
    }

    [Fact]
    public void GuidColumnType_ReturnsRaw16()
    {
        // Arrange
        var strategy = new OracleProviderStrategy();

        // Assert
        strategy.GuidColumnType.Should().Be("RAW(16)");
    }

    [Fact]
    public void TimestampColumnType_ReturnsTimestamp()
    {
        // Arrange
        var strategy = new OracleProviderStrategy();

        // Assert
        strategy.TimestampColumnType.Should().Contain("TIMESTAMP");
    }

    #endregion

    #region Feature Support Tests

    [Fact]
    public void SupportsNativeJson_ReturnsTrue()
    {
        // Arrange
        var strategy = new OracleProviderStrategy();

        // Assert
        // Oracle 12c+ has IS JSON constraint, 21c+ has native JSON type
        strategy.SupportsNativeJson.Should().BeTrue();
    }

    [Fact]
    public void SupportsNativeGuid_ReturnsFalse()
    {
        // Arrange
        var strategy = new OracleProviderStrategy();

        // Assert
        // Oracle stores GUIDs as RAW(16)
        strategy.SupportsNativeGuid.Should().BeFalse();
    }

    [Fact]
    public void SupportsSequences_ReturnsTrue()
    {
        // Arrange
        var strategy = new OracleProviderStrategy();

        // Assert
        // Oracle uses sequences (vs auto-increment)
        strategy.SupportsSequences.Should().BeTrue();
    }

    [Fact]
    public void DefaultDeleteBehavior_ReturnsCascade()
    {
        // Arrange
        var strategy = new OracleProviderStrategy();

        // Assert
        strategy.DefaultDeleteBehavior.Should().Be(DeleteBehavior.Cascade);
    }

    #endregion

    #region Batch Size Tests

    [Fact]
    public void RecommendedBatchSize_Standalone_ReturnsReasonableSize()
    {
        // Arrange
        var strategy = new OracleProviderStrategy(DatabaseDeploymentMode.Standalone);

        // Assert
        strategy.RecommendedBatchSize.Should().BeGreaterThan(0);
        strategy.RecommendedBatchSize.Should().BeLessThanOrEqualTo(200);
    }

    [Fact]
    public void RecommendedBatchSize_Clustered_ReturnsLargerSize()
    {
        // Arrange
        var standaloneStrategy = new OracleProviderStrategy(DatabaseDeploymentMode.Standalone);
        var clusteredStrategy = new OracleProviderStrategy(DatabaseDeploymentMode.Clustered);

        // Assert - RAC can handle larger batches
        clusteredStrategy.RecommendedBatchSize.Should()
            .BeGreaterThanOrEqualTo(standaloneStrategy.RecommendedBatchSize);
    }

    [Fact]
    public void RecommendedBatchSize_Hyperscale_ReturnsLargestSize()
    {
        // Arrange
        var clusteredStrategy = new OracleProviderStrategy(DatabaseDeploymentMode.Clustered);
        var hyperscaleStrategy = new OracleProviderStrategy(DatabaseDeploymentMode.Hyperscale);

        // Assert - Exadata/Autonomous can handle even larger batches
        hyperscaleStrategy.RecommendedBatchSize.Should()
            .BeGreaterThanOrEqualTo(clusteredStrategy.RecommendedBatchSize);
    }

    #endregion

    #region Connection String Optimization Tests

    [Fact]
    public void OptimizeConnectionString_ReturnsValidString()
    {
        // Arrange
        var strategy = new OracleProviderStrategy(DatabaseDeploymentMode.Standalone);
        var baseConnectionString = "Data Source=localhost:1521/ORCL;User Id=crm_user;Password=test;";

        // Act
        var optimized = strategy.OptimizeConnectionString(baseConnectionString);

        // Assert
        optimized.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(DatabaseDeploymentMode.Standalone)]
    [InlineData(DatabaseDeploymentMode.Clustered)]
    [InlineData(DatabaseDeploymentMode.Hyperscale)]
    public void OptimizeConnectionString_AllModes_ReturnValidString(DatabaseDeploymentMode mode)
    {
        // Arrange
        var strategy = new OracleProviderStrategy(mode);
        var baseConnectionString = "Data Source=localhost:1521/ORCL;";

        // Act
        var optimized = strategy.OptimizeConnectionString(baseConnectionString);

        // Assert
        optimized.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void OptimizeConnectionString_PreservesOriginalParameters()
    {
        // Arrange
        var strategy = new OracleProviderStrategy();
        var baseConnectionString = "Data Source=oracledb.local:1521/CRMPROD;User Id=app_user;";

        // Act
        var optimized = strategy.OptimizeConnectionString(baseConnectionString);

        // Assert
        optimized.Should().Contain("oracledb.local");
    }

    #endregion

    #region Oracle-Specific Features Tests

    [Fact]
    public void Oracle_UsesNclobForLargeText()
    {
        // Arrange
        var strategy = new OracleProviderStrategy();

        // Assert - NCLOB for large Unicode text (vs VARCHAR2 4000 limit)
        strategy.LongTextColumnType.Should().Be("NCLOB");
    }

    [Fact]
    public void Oracle_UsesRaw16ForGuids()
    {
        // Arrange
        var strategy = new OracleProviderStrategy();

        // Assert - RAW(16) stores 128-bit GUIDs
        strategy.GuidColumnType.Should().Be("RAW(16)");
        strategy.SupportsNativeGuid.Should().BeFalse();
    }

    [Fact]
    public void Oracle_UsesSequences()
    {
        // Arrange
        var strategy = new OracleProviderStrategy();

        // Assert - Oracle uses sequences (no auto-increment)
        strategy.SupportsSequences.Should().BeTrue();
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void Strategy_ImplementsIDatabaseProviderStrategy()
    {
        // Arrange
        var strategy = new OracleProviderStrategy();

        // Assert
        strategy.Should().BeAssignableTo<IDatabaseProviderStrategy>();
    }

    [Fact]
    public void Strategy_InheritsFromBaseStrategy()
    {
        // Arrange
        var strategy = new OracleProviderStrategy();

        // Assert
        strategy.Should().BeAssignableTo<DatabaseProviderStrategyBase>();
    }

    #endregion

    #region Comparison with Other Providers Tests

    [Fact]
    public void Oracle_DiffersFromSqlServer_InGuidType()
    {
        // Arrange
        var oracleStrategy = new OracleProviderStrategy();
        var sqlStrategy = new SqlServerProviderStrategy();

        // Assert
        oracleStrategy.GuidColumnType.Should().Be("RAW(16)");
        sqlStrategy.GuidColumnType.Should().Be("uniqueidentifier");
    }

    [Fact]
    public void Oracle_SimilarToPostgreSql_InJsonSupport()
    {
        // Arrange
        var oracleStrategy = new OracleProviderStrategy();
        var postgresStrategy = new PostgreSqlProviderStrategy();

        // Assert - both support native JSON (Oracle 12c+/21c+, PostgreSQL 9.4+)
        oracleStrategy.SupportsNativeJson.Should().BeTrue();
        postgresStrategy.SupportsNativeJson.Should().BeTrue();
    }

    [Fact]
    public void Oracle_SimilarToSqlServer_InSequenceSupport()
    {
        // Arrange
        var oracleStrategy = new OracleProviderStrategy();
        var sqlStrategy = new SqlServerProviderStrategy();

        // Assert
        oracleStrategy.SupportsSequences.Should().BeTrue();
        sqlStrategy.SupportsSequences.Should().BeTrue();
    }

    [Fact]
    public void Oracle_DiffersFromMySql_InTextType()
    {
        // Arrange
        var oracleStrategy = new OracleProviderStrategy();
        var mysqlStrategy = new MySqlProviderStrategy();

        // Assert
        oracleStrategy.LongTextColumnType.Should().Be("NCLOB");
        mysqlStrategy.LongTextColumnType.Should().Be("LONGTEXT");
    }

    #endregion
}

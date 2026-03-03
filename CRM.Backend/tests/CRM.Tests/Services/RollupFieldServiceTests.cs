// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for RollupFieldService.
/// Verifies SUM, COUNT aggregations and definition registration.
/// </summary>
public class RollupFieldServiceTests : ServiceTestFixtureBase<RollupFieldService>
{    public RollupFieldServiceTests()
    {    }

    private RollupFieldService BuildService(IEnumerable<Opportunity> opportunities)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(new List<Opportunity>(opportunities));
        MockContext.Setup(c => c.Opportunities).Returns(mockSet.Object);
        return new RollupFieldService(MockContext.Object, MockLogger.Object);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Test 1 – SUM of opportunity amounts for a parent account
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CalculateAsync_ShouldReturnSumOfAmounts_WhenFunctionIsSum()
    {
        // Arrange
        var opportunities = new List<Opportunity>
        {
            new() { Id = 1, AccountId = 1, Amount = 100m, Name = "Deal A", IsDeleted = false },
            new() { Id = 2, AccountId = 1, Amount = 200m, Name = "Deal B", IsDeleted = false },
            new() { Id = 3, AccountId = 2, Amount = 999m, Name = "Other",  IsDeleted = false }  // different parent
        };

        var service = BuildService(opportunities);

        var request = new RollupRequest
        {
            ParentEntityType = "Account",
            ParentEntityId = 1,
            ChildEntityType = "Opportunity",
            ChildFieldName = "Amount",
            ForeignKeyField = "AccountId",
            Function = RollupFunction.Sum
        };

        // Act
        var result = await service.CalculateAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.NumericValue.Should().BeApproximately(300.0, 0.001);
        result.RecordCount.Should().Be(2);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Test 2 – COUNT of child records
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CalculateAsync_ShouldReturnRecordCount_WhenFunctionIsCount()
    {
        // Arrange
        var opportunities = new List<Opportunity>
        {
            new() { Id = 1, AccountId = 5, Amount = 10m, Name = "Deal 1", IsDeleted = false },
            new() { Id = 2, AccountId = 5, Amount = 20m, Name = "Deal 2", IsDeleted = false },
            new() { Id = 3, AccountId = 5, Amount = 30m, Name = "Deal 3", IsDeleted = false }
        };

        var service = BuildService(opportunities);

        var request = new RollupRequest
        {
            ParentEntityType = "Account",
            ParentEntityId = 5,
            ChildEntityType = "Opportunity",
            ChildFieldName = "Amount",
            ForeignKeyField = "AccountId",
            Function = RollupFunction.Count
        };

        // Act
        var result = await service.CalculateAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.NumericValue.Should().Be(3);
        result.RecordCount.Should().Be(3);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Test 3 – RegisterRollupAsync stores definition retrievable by GetDefinitionsAsync
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterRollupAsync_ShouldMakeDefinitionRetrievable()
    {
        // Arrange
        var service = BuildService(new List<Opportunity>());

        var definition = new RollupDefinition
        {
            Name = "TotalRevenue",
            ParentEntityType = "Account",
            ChildEntityType = "Opportunity",
            ChildFieldName = "Amount",
            ForeignKeyField = "AccountId",
            Function = RollupFunction.Sum,
            IsActive = true
        };

        // Act
        var registered = await service.RegisterRollupAsync(definition);
        var retrieved = await service.GetDefinitionsAsync("Account");

        // Assert
        registered.Id.Should().BeGreaterThan(0);
        retrieved.Should().ContainSingle(d => d.Name == "TotalRevenue");
    }
}

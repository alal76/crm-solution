// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Relationship Management Unit Tests

using Xunit;
using FluentAssertions;
using CRM.Core.Entities;
using CRM.Core.Dtos;
using System;

namespace CRM.Tests.Services;

/// <summary>
/// Comprehensive unit tests for Relationship Management functionality
/// </summary>
public class RelationshipServiceTests
{
    #region RelationshipType Tests

    [Fact]
    public void CreateRelationshipType_Business_CreatesCorrectly()
    {
        // Arrange & Act
        var type = new RelationshipType
        {
            Id = 1,
            TypeName = "Partner",
            TypeCategory = "Business",
            Description = "Business partnership relationship",
            IsBidirectional = true,
            IsActive = true
        };

        // Assert
        type.Should().NotBeNull();
        type.TypeCategory.Should().Be("Business");
        type.TypeName.Should().Be("Partner");
        type.IsBidirectional.Should().BeTrue();
    }

    [Fact]
    public void CreateRelationshipType_Partnership_CreatesCorrectly()
    {
        // Arrange & Act
        var type = new RelationshipType
        {
            Id = 2,
            TypeName = "Strategic Alliance",
            TypeCategory = "Partnership",
            Description = "Strategic business partnership",
            IsBidirectional = true,
            ReverseTypeName = "Strategic Alliance",
            IsActive = true
        };

        // Assert
        type.TypeCategory.Should().Be("Partnership");
        type.ReverseTypeName.Should().Be("Strategic Alliance");
    }

    [Fact]
    public void CreateRelationshipType_NonBidirectional_HasReverseName()
    {
        // Arrange & Act
        var type = new RelationshipType
        {
            Id = 3,
            TypeName = "Parent Company",
            TypeCategory = "Hierarchy",
            IsBidirectional = false,
            ReverseTypeName = "Subsidiary"
        };

        // Assert
        type.IsBidirectional.Should().BeFalse();
        type.ReverseTypeName.Should().Be("Subsidiary");
    }

    [Fact]
    public void RelationshipCategory_AllCategories_Valid()
    {
        // Assert all relationship categories are valid enum values
        var categories = Enum.GetValues<RelationshipCategory>();
        categories.Should().HaveCountGreaterOrEqualTo(4);
        categories.Should().Contain(RelationshipCategory.Business);
        categories.Should().Contain(RelationshipCategory.Partnership);
        categories.Should().Contain(RelationshipCategory.Hierarchy);
        categories.Should().Contain(RelationshipCategory.Dependency);
    }

    #endregion

    #region AccountRelationship Tests

    [Fact]
    public void CreateAccountRelationship_Valid_CreatesCorrectly()
    {
        // Arrange & Act
        var relationship = new AccountRelationship
        {
            Id = 1,
            SourceCustomerId = 100,
            TargetCustomerId = 200,
            RelationshipTypeId = 1,
            Status = "Active",
            StrengthScore = 75,
            StrategicImportance = "High",
            RelationshipStartDate = DateTime.UtcNow.AddMonths(-6)
        };

        // Assert
        relationship.Should().NotBeNull();
        relationship.SourceCustomerId.Should().Be(100);
        relationship.TargetCustomerId.Should().Be(200);
        relationship.Status.Should().Be("Active");
        relationship.StrengthScore.Should().Be(75);
    }

    [Fact]
    public void CreateAccountRelationship_WithRevenueImpact_CreatesCorrectly()
    {
        // Arrange & Act
        var relationship = new AccountRelationship
        {
            Id = 2,
            SourceCustomerId = 100,
            TargetCustomerId = 300,
            RelationshipTypeId = 2,
            AnnualRevenueImpact = 150000m,
            CostSavings = 75000m,
            StrategicImportance = "Critical"
        };

        // Assert
        relationship.AnnualRevenueImpact.Should().Be(150000m);
        relationship.CostSavings.Should().Be(75000m);
        relationship.StrategicImportance.Should().Be("Critical");
    }

    [Fact]
    public void RelationshipStatus_AllStatuses_Valid()
    {
        var statuses = Enum.GetValues<RelationshipStatus>();
        statuses.Should().HaveCountGreaterOrEqualTo(4);
        statuses.Should().Contain(RelationshipStatus.Active);
        statuses.Should().Contain(RelationshipStatus.Inactive);
        statuses.Should().Contain(RelationshipStatus.Pending);
        statuses.Should().Contain(RelationshipStatus.Terminated);
    }

    [Fact]
    public void StrategicImportance_AllLevels_Valid()
    {
        var levels = Enum.GetValues<StrategicImportance>();
        levels.Should().HaveCountGreaterOrEqualTo(4);
        levels.Should().Contain(StrategicImportance.Critical);
        levels.Should().Contain(StrategicImportance.High);
        levels.Should().Contain(StrategicImportance.Medium);
        levels.Should().Contain(StrategicImportance.Low);
    }

    [Fact]
    public void UpdateAccountRelationship_ChangeStrength_UpdatesCorrectly()
    {
        // Arrange
        var relationship = new AccountRelationship
        {
            StrengthScore = 50,
            Status = "Active"
        };

        // Act
        relationship.StrengthScore = 80;

        // Assert
        relationship.StrengthScore.Should().Be(80);
    }

    [Fact]
    public void UpdateAccountRelationship_Terminate_UpdatesCorrectly()
    {
        // Arrange
        var relationship = new AccountRelationship
        {
            Status = "Active",
            RelationshipEndDate = null
        };

        // Act
        relationship.Status = "Terminated";
        relationship.RelationshipEndDate = DateTime.UtcNow;

        // Assert
        relationship.Status.Should().Be("Terminated");
        relationship.RelationshipEndDate.Should().NotBeNull();
    }

    #endregion

    #region RelationshipInteraction Tests

    [Fact]
    public void CreateRelationshipInteraction_Valid_CreatesCorrectly()
    {
        // Arrange & Act
        var interaction = new RelationshipInteraction
        {
            Id = 1,
            AccountRelationshipId = 1,
            InteractionType = "Meeting",
            Subject = "Quarterly Business Review",
            Description = "Discussed partnership expansion opportunities",
            InteractionDate = DateTime.UtcNow,
            Outcome = "Successful",
            HealthImpact = "Positive",
            SentimentScore = 75
        };

        // Assert
        interaction.Should().NotBeNull();
        interaction.InteractionType.Should().Be("Meeting");
        interaction.Outcome.Should().Be("Successful");
        interaction.SentimentScore.Should().Be(75);
    }

    [Fact]
    public void HealthImpact_AllImpacts_Valid()
    {
        var impacts = Enum.GetValues<HealthImpact>();
        impacts.Should().HaveCountGreaterOrEqualTo(4);
        impacts.Should().Contain(HealthImpact.Positive);
        impacts.Should().Contain(HealthImpact.Neutral);
        impacts.Should().Contain(HealthImpact.Negative);
        impacts.Should().Contain(HealthImpact.Critical);
    }

    [Fact]
    public void RelationshipInteraction_WithFollowUp_CreatesCorrectly()
    {
        // Arrange & Act
        var interaction = new RelationshipInteraction
        {
            Id = 2,
            AccountRelationshipId = 1,
            InteractionType = "Call",
            Subject = "Follow-up call",
            FollowUpDate = DateTime.UtcNow.AddDays(7),
            NextSteps = "Schedule demo session"
        };

        // Assert
        interaction.FollowUpDate.Should().NotBeNull();
        interaction.NextSteps.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RelationshipInteraction_NegativeOutcome_UpdatesHealth()
    {
        // Arrange & Act
        var interaction = new RelationshipInteraction
        {
            Outcome = "Failed",
            HealthImpact = "Negative",
            SentimentScore = -50
        };

        // Assert
        interaction.Outcome.Should().Be("Failed");
        interaction.SentimentScore.Should().BeLessThan(0);
    }

    #endregion

    #region AccountHealthSnapshot Tests

    [Fact]
    public void CreateAccountHealthSnapshot_Valid_CreatesCorrectly()
    {
        // Arrange & Act
        var snapshot = new AccountHealthSnapshot
        {
            Id = 1,
            CustomerId = 100,
            SnapshotDate = DateTime.UtcNow,
            OverallHealthScore = 78,
            EngagementScore = 85,
            SupportSatisfactionScore = 72,
            FinancialHealthScore = 80,
            RelationshipScore = 75,
            HealthTrend = "Improving",
            PreviousHealthScore = 73
        };

        // Assert
        snapshot.Should().NotBeNull();
        snapshot.OverallHealthScore.Should().Be(78);
        snapshot.HealthTrend.Should().Be("Improving");
        snapshot.PreviousHealthScore.Should().Be(73);
    }

    [Fact]
    public void AccountHealthSnapshot_DecliningTrend_ShowsNegative()
    {
        // Arrange & Act
        var snapshot = new AccountHealthSnapshot
        {
            OverallHealthScore = 45,
            HealthTrend = "Declining",
            PreviousHealthScore = 55
        };

        // Assert
        snapshot.HealthTrend.Should().Be("Declining");
        snapshot.OverallHealthScore.Should().BeLessThan(snapshot.PreviousHealthScore!.Value);
    }

    [Fact]
    public void HealthTrend_AllTrends_Valid()
    {
        var trends = Enum.GetValues<HealthTrend>();
        trends.Should().HaveCountGreaterOrEqualTo(4);
        trends.Should().Contain(HealthTrend.Improving);
        trends.Should().Contain(HealthTrend.Stable);
        trends.Should().Contain(HealthTrend.Declining);
        trends.Should().Contain(HealthTrend.Critical);
    }

    [Fact]
    public void AccountHealthSnapshot_WithRiskFactors_CreatesCorrectly()
    {
        // Arrange & Act
        var snapshot = new AccountHealthSnapshot
        {
            OverallHealthScore = 35,
            RiskFactors = "[\"No recent interactions\", \"Declining revenue\", \"Support issues\"]",
            WarningSignals = "[\"Late payments\", \"Reduced usage\"]"
        };

        // Assert
        snapshot.RiskFactors.Should().Contain("No recent interactions");
        snapshot.WarningSignals.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region RelationshipMap Tests

    [Fact]
    public void CreateRelationshipMap_Valid_CreatesCorrectly()
    {
        // Arrange & Act
        var map = new RelationshipMap
        {
            Id = 1,
            MapName = "Enterprise Partners Network",
            Description = "All enterprise partner relationships",
            CentralCustomerId = 100,
            RelationshipDepth = 3,
            MinRelationshipStrength = 50,
            IsPublic = false
        };

        // Assert
        map.Should().NotBeNull();
        map.MapName.Should().Be("Enterprise Partners Network");
        map.RelationshipDepth.Should().Be(3);
        map.MinRelationshipStrength.Should().Be(50);
    }

    [Fact]
    public void RelationshipMap_WithFilters_CreatesCorrectly()
    {
        // Arrange & Act
        var map = new RelationshipMap
        {
            MapName = "Filtered Map",
            IncludeRelationshipTypeIds = "[1, 2, 3]",
            ExcludeRelationshipTypeIds = "[5]",
            IncludeStatuses = "[\"Active\", \"Pending\"]",
            DateRangeStart = DateTime.UtcNow.AddYears(-1),
            DateRangeEnd = DateTime.UtcNow
        };

        // Assert
        map.IncludeRelationshipTypeIds.Should().Contain("1");
        map.IncludeStatuses.Should().Contain("Active");
        map.DateRangeStart.Should().NotBeNull();
    }

    #endregion

    #region AccountTerritory Tests

    [Fact]
    public void CreateAccountTerritory_Valid_CreatesCorrectly()
    {
        // Arrange & Act
        var territory = new AccountTerritory
        {
            Id = 1,
            TerritoryName = "North America Enterprise",
            TerritoryCode = "NA-ENT",
            Description = "North American Enterprise Accounts",
            Countries = "[\"USA\", \"Canada\"]",
            PrimaryOwnerId = 5,
            AnnualQuota = 5000000m,
            QuotaCurrency = "USD",
            IsActive = true
        };

        // Assert
        territory.Should().NotBeNull();
        territory.TerritoryName.Should().Be("North America Enterprise");
        territory.TerritoryCode.Should().Be("NA-ENT");
        territory.AnnualQuota.Should().Be(5000000m);
    }

    [Fact]
    public void AccountTerritory_WithIndustryFilter_CreatesCorrectly()
    {
        // Arrange & Act
        var territory = new AccountTerritory
        {
            TerritoryName = "Tech Sector",
            Industries = "[\"Technology\", \"Software\", \"SaaS\"]",
            CustomerTypes = "[\"Enterprise\", \"MidMarket\"]"
        };

        // Assert
        territory.Industries.Should().Contain("Technology");
        territory.CustomerTypes.Should().Contain("Enterprise");
    }

    #endregion

    #region DTO Tests

    [Fact]
    public void RelationshipTypeDto_MapsCorrectly()
    {
        // Arrange
        var type = new RelationshipType
        {
            Id = 1,
            TypeName = "Partner",
            TypeCategory = "Business",
            IsBidirectional = true,
            IsActive = true
        };

        // Act
        var dto = new RelationshipTypeDto
        {
            Id = type.Id,
            TypeName = type.TypeName,
            TypeCategory = type.TypeCategory,
            IsBidirectional = type.IsBidirectional,
            IsActive = type.IsActive
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.TypeName.Should().Be("Partner");
        dto.TypeCategory.Should().Be("Business");
    }

    [Fact]
    public void AccountRelationshipDto_MapsCorrectly()
    {
        // Arrange & Act
        var dto = new AccountRelationshipDto
        {
            Id = 1,
            SourceCustomerId = 100,
            SourceCustomerName = "Acme Corp",
            TargetCustomerId = 200,
            TargetCustomerName = "Partner Inc",
            RelationshipTypeName = "Partner",
            Status = "Active",
            StrengthScore = 80,
            StrategicImportance = "High"
        };

        // Assert
        dto.SourceCustomerName.Should().Be("Acme Corp");
        dto.TargetCustomerName.Should().Be("Partner Inc");
        dto.StrengthScore.Should().Be(80);
    }

    #endregion
}

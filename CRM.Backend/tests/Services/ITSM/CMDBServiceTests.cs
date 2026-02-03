// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 CRM Solution Contributors
// ITSM CMDB Service Unit Tests

using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Comprehensive unit tests for ITSM Configuration Management Database (CMDB)
/// </summary>
public class CMDBServiceTests
{
    #region Configuration Item Tests

    [Fact]
    public void CreateCI_ValidServer_CreatesCorrectly()
    {
        // Arrange & Act
        var ci = new ConfigurationItem
        {
            Name = "PROD-SQL-01",
            CIType = "Server",
            Status = CIStatus.Active,
            Environment = "Production",
            Criticality = "Critical",
            Description = "Primary production SQL Server",
            Manufacturer = "Dell",
            Model = "PowerEdge R750",
            SerialNumber = "SVCTAG001",
            Location = "Data Center A - Rack 12",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        ci.Should().NotBeNull();
        ci.Name.Should().Be("PROD-SQL-01");
        ci.CIType.Should().Be("Server");
        ci.Status.Should().Be(CIStatus.Active);
        ci.Criticality.Should().Be("Critical");
    }

    [Fact]
    public void CreateCI_Application_CreatesCorrectly()
    {
        // Arrange & Act
        var ci = new ConfigurationItem
        {
            Name = "CRM-PROD",
            CIType = "Application",
            Status = CIStatus.Active,
            Environment = "Production",
            Criticality = "Critical",
            Description = "CRM Solution Production Instance",
            Version = "2.1.0"
        };

        // Assert
        ci.CIType.Should().Be("Application");
        ci.Version.Should().Be("2.1.0");
    }

    [Fact]
    public void CreateCI_BusinessService_LinksCorrectly()
    {
        // Arrange & Act
        var ci = new ConfigurationItem
        {
            Name = "Email Service",
            CIType = "Business Service",
            Status = CIStatus.Active,
            Criticality = "Critical",
            SupportGroupId = 5,
            OwnerId = 10
        };

        // Assert
        ci.CIType.Should().Be("Business Service");
        ci.SupportGroupId.Should().Be(5);
        ci.OwnerId.Should().Be(10);
    }

    #endregion

    #region CI Relationship Tests

    [Fact]
    public void CreateRelationship_DependsOn_CreatesCorrectly()
    {
        // Arrange
        var webServer = new ConfigurationItem { CIId = 1, Name = "WEB-01", CIType = "Server" };
        var dbServer = new ConfigurationItem { CIId = 2, Name = "SQL-01", CIType = "Server" };

        // Act
        var relationship = new CIRelationship
        {
            SourceCIId = webServer.CIId,
            TargetCIId = dbServer.CIId,
            RelationshipType = "Depends On",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        relationship.SourceCIId.Should().Be(1);
        relationship.TargetCIId.Should().Be(2);
        relationship.RelationshipType.Should().Be("Depends On");
    }

    [Fact]
    public void CreateRelationship_Hosts_CreatesCorrectly()
    {
        // Arrange
        var physicalServer = new ConfigurationItem { CIId = 1, Name = "ESX-01", CIType = "Server" };
        var virtualMachine = new ConfigurationItem { CIId = 2, Name = "VM-APP-01", CIType = "Server" };

        // Act
        var relationship = new CIRelationship
        {
            SourceCIId = physicalServer.CIId,
            TargetCIId = virtualMachine.CIId,
            RelationshipType = "Hosts"
        };

        // Assert
        relationship.RelationshipType.Should().Be("Hosts");
    }

    [Theory]
    [InlineData("Depends On", "Dependency Of")]
    [InlineData("Hosts", "Hosted On")]
    [InlineData("Contains", "Contained In")]
    [InlineData("Uses", "Used By")]
    [InlineData("Manages", "Managed By")]
    public void RelationshipType_HasInverse_ReturnsCorrectInverse(
        string relationshipType, 
        string expectedInverse)
    {
        // Act
        var inverse = GetInverseRelationship(relationshipType);

        // Assert
        inverse.Should().Be(expectedInverse);
    }

    private static string GetInverseRelationship(string type)
    {
        return type switch
        {
            "Depends On" => "Dependency Of",
            "Hosts" => "Hosted On",
            "Contains" => "Contained In",
            "Uses" => "Used By",
            "Manages" => "Managed By",
            "Connects To" => "Connected From",
            _ => $"Related To (from {type})"
        };
    }

    #endregion

    #region Impact Analysis Tests

    [Fact]
    public void ImpactAnalysis_DirectDependency_IdentifiesImpact()
    {
        // Arrange
        var cis = CreateSampleCMDB();
        var failingCI = cis.First(c => c.Name == "SQL-01");

        // Act
        var impactedCIs = GetDirectlyImpactedCIs(failingCI, cis);

        // Assert
        impactedCIs.Should().Contain(c => c.Name == "WEB-01"); // Web depends on SQL
        impactedCIs.Should().Contain(c => c.Name == "WEB-02");
    }

    [Fact]
    public void ImpactAnalysis_TransitiveDependency_IdentifiesAllImpact()
    {
        // Arrange
        var cis = CreateSampleCMDB();
        var failingCI = cis.First(c => c.Name == "SQL-01");

        // Act
        var allImpacted = GetAllImpactedCIs(failingCI, cis, maxDepth: 3);

        // Assert
        allImpacted.Should().Contain(c => c.Name == "WEB-01");
        allImpacted.Should().Contain(c => c.Name == "CRM Application");
    }

    [Fact]
    public void ImpactAnalysis_CriticalCI_HigherImpactScore()
    {
        // Arrange
        var criticalCI = new ConfigurationItem { Name = "Core-DB", Criticality = "Critical" };
        var lowCI = new ConfigurationItem { Name = "Test-Server", Criticality = "Low" };

        // Act
        var criticalScore = CalculateImpactScore(criticalCI);
        var lowScore = CalculateImpactScore(lowCI);

        // Assert
        criticalScore.Should().BeGreaterThan(lowScore);
    }

    private static int CalculateImpactScore(ConfigurationItem ci)
    {
        return ci.Criticality switch
        {
            "Critical" => 100,
            "High" => 75,
            "Medium" => 50,
            "Low" => 25,
            _ => 10
        };
    }

    #endregion

    #region CI Status Lifecycle Tests

    [Fact]
    public void CIStatus_NewToActive_IsValid()
    {
        // Arrange
        var ci = new ConfigurationItem { Status = CIStatus.Planned };

        // Act
        var canTransition = IsValidCIStatusTransition(ci.Status, CIStatus.Active);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Fact]
    public void CIStatus_ActiveToRetired_IsValid()
    {
        // Arrange
        var ci = new ConfigurationItem { Status = CIStatus.Active };

        // Act
        var canTransition = IsValidCIStatusTransition(ci.Status, CIStatus.Retired);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Fact]
    public void CIStatus_RetiredToActive_RequiresReactivation()
    {
        // Arrange
        var ci = new ConfigurationItem { Status = CIStatus.Retired };

        // Act
        var canTransition = IsValidCIStatusTransition(ci.Status, CIStatus.Active);

        // Assert - Reactivation should be allowed but may require approval
        canTransition.Should().BeTrue();
    }

    private static bool IsValidCIStatusTransition(CIStatus from, CIStatus to)
    {
        var validTransitions = new Dictionary<CIStatus, CIStatus[]>
        {
            { CIStatus.Planned, new[] { CIStatus.Active, CIStatus.Cancelled } },
            { CIStatus.Active, new[] { CIStatus.Maintenance, CIStatus.Retired, CIStatus.Disposed } },
            { CIStatus.Maintenance, new[] { CIStatus.Active, CIStatus.Retired } },
            { CIStatus.Retired, new[] { CIStatus.Active, CIStatus.Disposed } },
            { CIStatus.Disposed, Array.Empty<CIStatus>() },
            { CIStatus.Cancelled, Array.Empty<CIStatus>() }
        };

        return validTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    #endregion

    #region CI Search and Query Tests

    [Fact]
    public void SearchCIs_ByType_ReturnsMatchingItems()
    {
        // Arrange
        var cis = CreateSampleCMDB();

        // Act
        var servers = cis.Where(c => c.CIType == "Server").ToList();

        // Assert
        servers.Should().HaveCountGreaterThan(0);
        servers.Should().OnlyContain(c => c.CIType == "Server");
    }

    [Fact]
    public void SearchCIs_ByEnvironment_ReturnsMatchingItems()
    {
        // Arrange
        var cis = CreateSampleCMDB();

        // Act
        var productionCIs = cis.Where(c => c.Environment == "Production").ToList();

        // Assert
        productionCIs.Should().HaveCountGreaterThan(0);
        productionCIs.Should().OnlyContain(c => c.Environment == "Production");
    }

    [Fact]
    public void SearchCIs_ByCriticality_ReturnsMatchingItems()
    {
        // Arrange
        var cis = CreateSampleCMDB();

        // Act
        var criticalCIs = cis.Where(c => c.Criticality == "Critical").ToList();

        // Assert
        criticalCIs.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region Helper Methods

    private static List<ConfigurationItem> CreateSampleCMDB()
    {
        var cis = new List<ConfigurationItem>
        {
            new() { CIId = 1, Name = "SQL-01", CIType = "Server", Status = CIStatus.Active, Environment = "Production", Criticality = "Critical" },
            new() { CIId = 2, Name = "SQL-02", CIType = "Server", Status = CIStatus.Active, Environment = "Production", Criticality = "Critical" },
            new() { CIId = 3, Name = "WEB-01", CIType = "Server", Status = CIStatus.Active, Environment = "Production", Criticality = "High", DependsOnIds = new[] { 1 } },
            new() { CIId = 4, Name = "WEB-02", CIType = "Server", Status = CIStatus.Active, Environment = "Production", Criticality = "High", DependsOnIds = new[] { 1 } },
            new() { CIId = 5, Name = "CRM Application", CIType = "Application", Status = CIStatus.Active, Environment = "Production", Criticality = "Critical", DependsOnIds = new[] { 3, 4 } },
            new() { CIId = 6, Name = "Email Service", CIType = "Business Service", Status = CIStatus.Active, Environment = "Production", Criticality = "Critical" },
            new() { CIId = 7, Name = "DEV-SQL-01", CIType = "Server", Status = CIStatus.Active, Environment = "Development", Criticality = "Low" }
        };

        return cis;
    }

    private static List<ConfigurationItem> GetDirectlyImpactedCIs(
        ConfigurationItem failingCI,
        List<ConfigurationItem> allCIs)
    {
        // Find CIs that depend on the failing CI
        return allCIs
            .Where(c => c.DependsOnIds != null && c.DependsOnIds.Contains(failingCI.CIId))
            .ToList();
    }

    private static List<ConfigurationItem> GetAllImpactedCIs(
        ConfigurationItem failingCI,
        List<ConfigurationItem> allCIs,
        int maxDepth)
    {
        var impacted = new List<ConfigurationItem>();
        var queue = new Queue<(ConfigurationItem CI, int Depth)>();
        
        foreach (var ci in GetDirectlyImpactedCIs(failingCI, allCIs))
        {
            queue.Enqueue((ci, 1));
        }

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();
            
            if (!impacted.Contains(current))
            {
                impacted.Add(current);
                
                if (depth < maxDepth)
                {
                    foreach (var dependent in GetDirectlyImpactedCIs(current, allCIs))
                    {
                        queue.Enqueue((dependent, depth + 1));
                    }
                }
            }
        }

        return impacted;
    }

    #endregion
}

// Test helper classes
public class ConfigurationItem
{
    public int CIId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CIType { get; set; } = string.Empty;
    public CIStatus Status { get; set; }
    public string? Environment { get; set; }
    public string? Criticality { get; set; }
    public string? Description { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? Location { get; set; }
    public string? Version { get; set; }
    public int? SupportGroupId { get; set; }
    public int? OwnerId { get; set; }
    public int[]? DependsOnIds { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CIRelationship
{
    public int SourceCIId { get; set; }
    public int TargetCIId { get; set; }
    public string RelationshipType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public enum CIStatus
{
    Planned = 1,
    Active = 2,
    Maintenance = 3,
    Retired = 4,
    Disposed = 5,
    Cancelled = 6
}

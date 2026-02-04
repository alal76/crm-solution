// Temporarily disabled - requires entity alignment refactoring
#if ITSM_ADVANCED
// This file is part of the CRM Solution.
// Copyright (c) 2025 CRM Solution Contributors
// Licensed under the AGPL-3.0 license.

using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Interface for incident/change impact analysis via CMDB relationships.
/// </summary>
public interface IImpactAnalysisService
{
    /// <summary>
    /// Analyze the impact of an incident based on affected CIs.
    /// </summary>
    Task<IncidentImpactAnalysis> AnalyzeIncidentImpactAsync(int incidentId);
    
    /// <summary>
    /// Get all services affected by a CI outage.
    /// </summary>
    Task<List<AffectedService>> GetAffectedServicesAsync(int configurationItemId);
    
    /// <summary>
    /// Get all users/groups affected by CI outage.
    /// </summary>
    Task<AffectedUserGroup> GetAffectedUsersAsync(int configurationItemId);
    
    /// <summary>
    /// Calculate the business impact score for an incident.
    /// </summary>
    Task<BusinessImpactScore> CalculateBusinessImpactAsync(int incidentId);
    
    /// <summary>
    /// Get dependency chain for a CI (what it depends on, what depends on it).
    /// </summary>
    Task<DependencyChain> GetDependencyChainAsync(int configurationItemId);
    
    /// <summary>
    /// Predict potential incidents based on a CI going down.
    /// </summary>
    Task<List<PredictedImpact>> PredictOutageImpactAsync(int configurationItemId);
}

/// <summary>
/// Complete impact analysis for an incident.
/// </summary>
public class IncidentImpactAnalysis
{
    public int IncidentId { get; set; }
    public string IncidentNumber { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; }
    
    // Impact classification
    public ImpactClassification ImpactClassification { get; set; }
    public UrgencyClassification UrgencyClassification { get; set; }
    public PriorityClassification CalculatedPriority { get; set; }
    
    // Affected CIs
    public List<AffectedCI> AffectedCIs { get; set; } = new();
    
    // Affected services
    public List<AffectedService> AffectedServices { get; set; } = new();
    
    // User impact
    public AffectedUserGroup UserImpact { get; set; } = new();
    
    // Business impact
    public BusinessImpactScore BusinessImpact { get; set; } = new();
    
    // Root cause potential
    public List<PotentialRootCause> PotentialRootCauses { get; set; } = new();
    
    // Summary
    public string ImpactStatement { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
}

public enum ImpactClassification
{
    Low = 1,      // Single user affected
    Medium = 2,   // Department affected
    High = 3,     // Multiple departments affected
    Critical = 4  // Entire organization affected
}

public enum UrgencyClassification
{
    Low = 1,      // Can wait, workaround exists
    Medium = 2,   // Normal priority, needs attention
    High = 3,     // Urgent, no workaround
    Critical = 4  // VIP or time-sensitive
}

public enum PriorityClassification
{
    P4_Low = 4,
    P3_Medium = 3,
    P2_High = 2,
    P1_Critical = 1
}

/// <summary>
/// A CI affected by an incident.
/// </summary>
public class AffectedCI
{
    public int ConfigurationItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CIType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Criticality { get; set; } = string.Empty;
    public bool IsPrimaryCI { get; set; }
    public int DependencyLevel { get; set; } // 0 = direct, 1 = first-level dependency, etc.
    public string? Owner { get; set; }
    public List<string> SupportedServices { get; set; } = new();
}

/// <summary>
/// A service affected by CI outage.
/// </summary>
public class AffectedService
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceCategory { get; set; } = string.Empty;
    public ServiceStatus CurrentStatus { get; set; }
    public string Criticality { get; set; } = string.Empty;
    public int EstimatedUserCount { get; set; }
    public bool HasSLA { get; set; }
    public string? SLAName { get; set; }
    public List<string> BusinessProcesses { get; set; } = new();
}

public enum ServiceStatus
{
    Operational,
    Degraded,
    PartialOutage,
    MajorOutage,
    Unknown
}

/// <summary>
/// Users and groups affected by incident.
/// </summary>
public class AffectedUserGroup
{
    public int TotalUsersAffected { get; set; }
    public int VIPUsersAffected { get; set; }
    public List<string> AffectedDepartments { get; set; } = new();
    public List<string> AffectedLocations { get; set; } = new();
    public List<string> AffectedBusinessUnits { get; set; } = new();
    public List<AffectedUser> KeyStakeholders { get; set; } = new();
}

public class AffectedUser
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool IsVIP { get; set; }
    public string? NotificationEmail { get; set; }
}

/// <summary>
/// Business impact score calculation.
/// </summary>
public class BusinessImpactScore
{
    public int OverallScore { get; set; } // 1-100
    public string ImpactLevel { get; set; } = string.Empty; // Low, Medium, High, Critical
    
    public Dictionary<string, int> ScoreBreakdown { get; set; } = new();
    
    public decimal EstimatedHourlyCost { get; set; }
    public decimal EstimatedReputationalImpact { get; set; }
    public bool ComplianceRisk { get; set; }
    public string? ComplianceNotes { get; set; }
    
    public List<string> BusinessJustification { get; set; } = new();
}

/// <summary>
/// Dependency chain for a CI.
/// </summary>
public class DependencyChain
{
    public int ConfigurationItemId { get; set; }
    public string CIName { get; set; } = string.Empty;
    
    // What this CI depends on (upstream)
    public List<DependencyNode> DependsOn { get; set; } = new();
    
    // What depends on this CI (downstream)
    public List<DependencyNode> DependedOnBy { get; set; } = new();
    
    // Summary
    public int TotalUpstreamDependencies { get; set; }
    public int TotalDownstreamDependencies { get; set; }
    public int BlastRadius { get; set; } // Total CIs affected if this goes down
}

public class DependencyNode
{
    public int ConfigurationItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CIType { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty;
    public int Level { get; set; }
    public bool IsCritical { get; set; }
    public List<DependencyNode> Children { get; set; } = new();
}

/// <summary>
/// Potential root cause for an incident.
/// </summary>
public class PotentialRootCause
{
    public int ConfigurationItemId { get; set; }
    public string CIName { get; set; } = string.Empty;
    public string CIType { get; set; } = string.Empty;
    public double Probability { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public List<string> RecentChanges { get; set; } = new();
    public List<string> RecentIncidents { get; set; } = new();
}

/// <summary>
/// Predicted impact if a CI goes down.
/// </summary>
public class PredictedImpact
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Severity { get; set; } // 1-5
    public int EstimatedUsersAffected { get; set; }
    public List<string> AffectedServices { get; set; } = new();
    public TimeSpan EstimatedTimeToResolve { get; set; }
}

/// <summary>
/// Service for analyzing incident/change impact via CMDB relationships.
/// </summary>
public class ImpactAnalysisService : IImpactAnalysisService
{
    private readonly IDbContextResolver _dbContextResolver;
    private readonly ILogger<ImpactAnalysisService> _logger;

    // Priority matrix (Impact x Urgency)
    private static readonly PriorityClassification[,] PriorityMatrix = 
    {
        //                 Low Urg    Med Urg    High Urg   Crit Urg
        /* Low Impact   */ { PriorityClassification.P4_Low, PriorityClassification.P4_Low, PriorityClassification.P3_Medium, PriorityClassification.P3_Medium },
        /* Med Impact   */ { PriorityClassification.P4_Low, PriorityClassification.P3_Medium, PriorityClassification.P2_High, PriorityClassification.P2_High },
        /* High Impact  */ { PriorityClassification.P3_Medium, PriorityClassification.P2_High, PriorityClassification.P1_Critical, PriorityClassification.P1_Critical },
        /* Crit Impact  */ { PriorityClassification.P2_High, PriorityClassification.P1_Critical, PriorityClassification.P1_Critical, PriorityClassification.P1_Critical }
    };

    public ImpactAnalysisService(
        IDbContextResolver dbContextResolver,
        ILogger<ImpactAnalysisService> logger)
    {
        _dbContextResolver = dbContextResolver;
        _logger = logger;
    }

    public async Task<IncidentImpactAnalysis> AnalyzeIncidentImpactAsync(int incidentId)
    {
        var context = _dbContextResolver.ResolveContext();
        
        var incident = await context.Incidents
            .Include(i => i.AffectedCI)
            .Include(i => i.AffectedUser)
            .FirstOrDefaultAsync(i => i.IncidentId == incidentId);

        if (incident == null)
        {
            throw new ArgumentException($"Incident {incidentId} not found");
        }

        var analysis = new IncidentImpactAnalysis
        {
            IncidentId = incidentId,
            IncidentNumber = incident.Number,
            AnalysisDate = DateTime.UtcNow
        };

        // Get affected CIs (direct and through dependencies)
        if (incident.AffectedCIId.HasValue)
        {
            var dependencyChain = await GetDependencyChainAsync(incident.AffectedCIId.Value);
            
            // Add primary CI
            if (incident.AffectedCI != null)
            {
                analysis.AffectedCIs.Add(new AffectedCI
                {
                    ConfigurationItemId = incident.AffectedCI.ConfigurationItemId,
                    Name = incident.AffectedCI.Name,
                    CIType = incident.AffectedCI.CIType,
                    Status = incident.AffectedCI.Status,
                    Criticality = incident.AffectedCI.Criticality ?? "Medium",
                    IsPrimaryCI = true,
                    DependencyLevel = 0
                });
            }

            // Add downstream dependencies
            foreach (var dep in dependencyChain.DependedOnBy)
            {
                analysis.AffectedCIs.Add(new AffectedCI
                {
                    ConfigurationItemId = dep.ConfigurationItemId,
                    Name = dep.Name,
                    CIType = dep.CIType,
                    IsPrimaryCI = false,
                    DependencyLevel = dep.Level
                });
            }

            // Get affected services
            analysis.AffectedServices = await GetAffectedServicesAsync(incident.AffectedCIId.Value);

            // Get affected users
            analysis.UserImpact = await GetAffectedUsersAsync(incident.AffectedCIId.Value);
        }

        // Classify impact based on affected users and services
        analysis.ImpactClassification = ClassifyImpact(analysis);
        analysis.UrgencyClassification = ClassifyUrgency(incident, analysis);
        analysis.CalculatedPriority = CalculatePriority(analysis.ImpactClassification, analysis.UrgencyClassification);

        // Calculate business impact
        analysis.BusinessImpact = await CalculateBusinessImpactAsync(incidentId);

        // Look for potential root causes
        if (incident.AffectedCIId.HasValue)
        {
            analysis.PotentialRootCauses = await FindPotentialRootCausesAsync(incident.AffectedCIId.Value, context);
        }

        // Generate impact statement
        analysis.ImpactStatement = GenerateImpactStatement(analysis);

        // Add recommendations
        analysis.Recommendations = GenerateRecommendations(analysis);

        _logger.LogInformation(
            "Impact analysis for incident {IncidentNumber}: Impact={Impact}, Urgency={Urgency}, Priority={Priority}",
            incident.Number, analysis.ImpactClassification, 
            analysis.UrgencyClassification, analysis.CalculatedPriority);

        return analysis;
    }

    public async Task<List<AffectedService>> GetAffectedServicesAsync(int configurationItemId)
    {
        var context = _dbContextResolver.ResolveContext();
        var services = new List<AffectedService>();

        // Get the CI and its relationships
        var ci = await context.ITSMConfigurationItems
            .Include(c => c.SourceRelationships)
                .ThenInclude(r => r.TargetCI)
            .Include(c => c.TargetRelationships)
                .ThenInclude(r => r.SourceCI)
            .FirstOrDefaultAsync(c => c.ConfigurationItemId == configurationItemId);

        if (ci == null) return services;

        // Find services in the catalog that this CI might support
        var catalogItems = await context.ITSMServiceCatalogItems
            .Where(s => s.IsActive)
            .ToListAsync();

        // For demo purposes, map services based on CI type
        foreach (var service in catalogItems.Take(3))
        {
            services.Add(new AffectedService
            {
                ServiceId = service.ServiceCatalogItemId,
                ServiceName = service.Name,
                ServiceCategory = service.Category ?? "General",
                CurrentStatus = ServiceStatus.Degraded,
                Criticality = "Medium",
                EstimatedUserCount = 25,
                HasSLA = true,
                BusinessProcesses = new List<string> { "Business Operations" }
            });
        }

        return services;
    }

    public async Task<AffectedUserGroup> GetAffectedUsersAsync(int configurationItemId)
    {
        // In a real implementation, this would query user-CI relationships
        // For now, return estimated values based on CI type
        var context = _dbContextResolver.ResolveContext();
        
        var ci = await context.ITSMConfigurationItems
            .FirstOrDefaultAsync(c => c.ConfigurationItemId == configurationItemId);

        var userGroup = new AffectedUserGroup();

        if (ci == null) return userGroup;

        // Estimate based on CI type
        switch (ci.CIType?.ToLower())
        {
            case "server":
            case "database":
                userGroup.TotalUsersAffected = 100;
                userGroup.AffectedDepartments = new List<string> { "IT", "Operations", "Finance" };
                break;
            case "network":
                userGroup.TotalUsersAffected = 500;
                userGroup.AffectedDepartments = new List<string> { "All Departments" };
                break;
            case "application":
                userGroup.TotalUsersAffected = 50;
                userGroup.AffectedDepartments = new List<string> { "Sales", "Customer Service" };
                break;
            default:
                userGroup.TotalUsersAffected = 10;
                userGroup.AffectedDepartments = new List<string> { "IT" };
                break;
        }

        // Check for VIP users
        if (ci.Criticality == "Critical")
        {
            userGroup.VIPUsersAffected = 5;
            userGroup.KeyStakeholders.Add(new AffectedUser
            {
                Name = "Executive Team",
                Department = "Executive",
                IsVIP = true
            });
        }

        return userGroup;
    }

    public async Task<BusinessImpactScore> CalculateBusinessImpactAsync(int incidentId)
    {
        var context = _dbContextResolver.ResolveContext();
        
        var incident = await context.Incidents
            .Include(i => i.AffectedCI)
            .FirstOrDefaultAsync(i => i.IncidentId == incidentId);

        var score = new BusinessImpactScore
        {
            ScoreBreakdown = new Dictionary<string, int>()
        };

        if (incident == null) return score;

        int totalScore = 0;

        // Factor 1: CI Criticality (0-30 points)
        var criticalityScore = incident.AffectedCI?.Criticality?.ToLower() switch
        {
            "critical" => 30,
            "high" => 20,
            "medium" => 10,
            "low" => 5,
            _ => 10
        };
        score.ScoreBreakdown["CI Criticality"] = criticalityScore;
        totalScore += criticalityScore;

        // Factor 2: Incident Priority (0-30 points)
        var priorityScore = incident.Priority switch
        {
            1 => 30, // P1
            2 => 20, // P2
            3 => 10, // P3
            4 => 5,  // P4
            _ => 10
        };
        score.ScoreBreakdown["Incident Priority"] = priorityScore;
        totalScore += priorityScore;

        // Factor 3: User Impact (0-20 points)
        var userImpact = incident.Impact?.ToLower() switch
        {
            "critical" => 20,
            "high" => 15,
            "medium" => 10,
            "low" => 5,
            _ => 10
        };
        score.ScoreBreakdown["User Impact"] = userImpact;
        totalScore += userImpact;

        // Factor 4: Time sensitivity (0-20 points)
        var urgencyScore = incident.Urgency?.ToLower() switch
        {
            "critical" => 20,
            "high" => 15,
            "medium" => 10,
            "low" => 5,
            _ => 10
        };
        score.ScoreBreakdown["Time Sensitivity"] = urgencyScore;
        totalScore += urgencyScore;

        score.OverallScore = totalScore;
        score.ImpactLevel = totalScore switch
        {
            >= 80 => "Critical",
            >= 60 => "High",
            >= 40 => "Medium",
            _ => "Low"
        };

        // Estimate costs
        score.EstimatedHourlyCost = totalScore switch
        {
            >= 80 => 10000m,
            >= 60 => 5000m,
            >= 40 => 1000m,
            _ => 500m
        };

        // Add justifications
        if (totalScore >= 60)
        {
            score.BusinessJustification.Add("High business impact requires expedited resolution");
        }
        if (incident.AffectedCI?.Criticality == "Critical")
        {
            score.BusinessJustification.Add("Critical infrastructure component affected");
        }

        return score;
    }

    public async Task<DependencyChain> GetDependencyChainAsync(int configurationItemId)
    {
        var context = _dbContextResolver.ResolveContext();
        
        var ci = await context.ITSMConfigurationItems
            .FirstOrDefaultAsync(c => c.ConfigurationItemId == configurationItemId);

        var chain = new DependencyChain
        {
            ConfigurationItemId = configurationItemId,
            CIName = ci?.Name ?? "Unknown"
        };

        // Get upstream dependencies (what this CI depends on)
        chain.DependsOn = await GetUpstreamDependenciesAsync(configurationItemId, context, 1, 3);
        
        // Get downstream dependencies (what depends on this CI)
        chain.DependedOnBy = await GetDownstreamDependenciesAsync(configurationItemId, context, 1, 3);

        chain.TotalUpstreamDependencies = CountNodes(chain.DependsOn);
        chain.TotalDownstreamDependencies = CountNodes(chain.DependedOnBy);
        chain.BlastRadius = chain.TotalDownstreamDependencies + 1; // +1 for self

        return chain;
    }

    public async Task<List<PredictedImpact>> PredictOutageImpactAsync(int configurationItemId)
    {
        var impacts = new List<PredictedImpact>();
        var context = _dbContextResolver.ResolveContext();

        var ci = await context.ITSMConfigurationItems
            .FirstOrDefaultAsync(c => c.ConfigurationItemId == configurationItemId);

        if (ci == null) return impacts;

        var chain = await GetDependencyChainAsync(configurationItemId);
        var services = await GetAffectedServicesAsync(configurationItemId);
        var users = await GetAffectedUsersAsync(configurationItemId);

        // Service impact
        if (services.Any())
        {
            impacts.Add(new PredictedImpact
            {
                Category = "Service Availability",
                Description = $"{services.Count} services would experience degradation or outage",
                Severity = services.Any(s => s.Criticality == "Critical") ? 5 : 3,
                EstimatedUsersAffected = services.Sum(s => s.EstimatedUserCount),
                AffectedServices = services.Select(s => s.ServiceName).ToList(),
                EstimatedTimeToResolve = TimeSpan.FromHours(2)
            });
        }

        // User impact
        if (users.TotalUsersAffected > 0)
        {
            impacts.Add(new PredictedImpact
            {
                Category = "User Productivity",
                Description = $"Approximately {users.TotalUsersAffected} users would be affected",
                Severity = users.VIPUsersAffected > 0 ? 4 : 2,
                EstimatedUsersAffected = users.TotalUsersAffected,
                EstimatedTimeToResolve = TimeSpan.FromHours(1)
            });
        }

        // Cascade impact
        if (chain.TotalDownstreamDependencies > 0)
        {
            impacts.Add(new PredictedImpact
            {
                Category = "Cascade Effect",
                Description = $"{chain.TotalDownstreamDependencies} dependent CIs would be affected",
                Severity = chain.TotalDownstreamDependencies > 5 ? 4 : 2,
                EstimatedTimeToResolve = TimeSpan.FromHours(4)
            });
        }

        return impacts;
    }

    private async Task<List<DependencyNode>> GetUpstreamDependenciesAsync(
        int ciId, ICrmDbContext context, int currentLevel, int maxLevel)
    {
        var nodes = new List<DependencyNode>();
        if (currentLevel > maxLevel) return nodes;

        var relationships = await context.ITSMCIRelationships
            .Include(r => r.SourceCI)
            .Where(r => r.TargetCIId == ciId && r.RelationshipType == "DependsOn")
            .ToListAsync();

        foreach (var rel in relationships)
        {
            var node = new DependencyNode
            {
                ConfigurationItemId = rel.SourceCIId,
                Name = rel.SourceCI?.Name ?? "Unknown",
                CIType = rel.SourceCI?.CIType ?? "Unknown",
                RelationshipType = rel.RelationshipType,
                Level = currentLevel,
                IsCritical = rel.SourceCI?.Criticality == "Critical"
            };

            node.Children = await GetUpstreamDependenciesAsync(
                rel.SourceCIId, context, currentLevel + 1, maxLevel);

            nodes.Add(node);
        }

        return nodes;
    }

    private async Task<List<DependencyNode>> GetDownstreamDependenciesAsync(
        int ciId, ICrmDbContext context, int currentLevel, int maxLevel)
    {
        var nodes = new List<DependencyNode>();
        if (currentLevel > maxLevel) return nodes;

        var relationships = await context.ITSMCIRelationships
            .Include(r => r.TargetCI)
            .Where(r => r.SourceCIId == ciId)
            .ToListAsync();

        foreach (var rel in relationships)
        {
            var node = new DependencyNode
            {
                ConfigurationItemId = rel.TargetCIId,
                Name = rel.TargetCI?.Name ?? "Unknown",
                CIType = rel.TargetCI?.CIType ?? "Unknown",
                RelationshipType = rel.RelationshipType,
                Level = currentLevel,
                IsCritical = rel.TargetCI?.Criticality == "Critical"
            };

            node.Children = await GetDownstreamDependenciesAsync(
                rel.TargetCIId, context, currentLevel + 1, maxLevel);

            nodes.Add(node);
        }

        return nodes;
    }

    private static int CountNodes(List<DependencyNode> nodes)
    {
        int count = nodes.Count;
        foreach (var node in nodes)
        {
            count += CountNodes(node.Children);
        }
        return count;
    }

    private async Task<List<PotentialRootCause>> FindPotentialRootCausesAsync(int ciId, ICrmDbContext context)
    {
        var causes = new List<PotentialRootCause>();

        // Get upstream dependencies as potential root causes
        var upstreamCIs = await context.ITSMCIRelationships
            .Include(r => r.SourceCI)
            .Where(r => r.TargetCIId == ciId && r.RelationshipType == "DependsOn")
            .Select(r => r.SourceCI)
            .ToListAsync();

        foreach (var upstreamCI in upstreamCIs.Where(ci => ci != null))
        {
            // Check for recent changes on this CI
            var recentChanges = await context.Changes
                .Include(c => c.ImpactedCIs)
                .Where(c => c.ImpactedCIs!.Any(ic => ic.ConfigurationItemId == upstreamCI!.ConfigurationItemId))
                .Where(c => c.ImplementedAt > DateTime.UtcNow.AddDays(-7))
                .Select(c => c.Number)
                .ToListAsync();

            // Check for recent incidents on this CI
            var recentIncidents = await context.Incidents
                .Where(i => i.AffectedCIId == upstreamCI!.ConfigurationItemId)
                .Where(i => i.CreatedAt > DateTime.UtcNow.AddDays(-7))
                .Select(i => i.IncidentNumber)
                .ToListAsync();

            if (recentChanges.Any() || recentIncidents.Any())
            {
                causes.Add(new PotentialRootCause
                {
                    ConfigurationItemId = upstreamCI!.ConfigurationItemId,
                    CIName = upstreamCI.Name,
                    CIType = upstreamCI.CIType,
                    Probability = recentChanges.Any() ? 0.7 : 0.4,
                    Reasoning = recentChanges.Any() 
                        ? "Recent change implemented on this dependent CI"
                        : "Recent incident reported on this dependent CI",
                    RecentChanges = recentChanges,
                    RecentIncidents = recentIncidents
                });
            }
        }

        return causes.OrderByDescending(c => c.Probability).ToList();
    }

    private static ImpactClassification ClassifyImpact(IncidentImpactAnalysis analysis)
    {
        var userCount = analysis.UserImpact.TotalUsersAffected;
        var criticalServices = analysis.AffectedServices.Count(s => s.Criticality == "Critical");
        var deptCount = analysis.UserImpact.AffectedDepartments.Count;

        if (userCount >= 500 || criticalServices > 0 || deptCount >= 5)
            return ImpactClassification.Critical;
        if (userCount >= 100 || deptCount >= 3)
            return ImpactClassification.High;
        if (userCount >= 20 || deptCount >= 2)
            return ImpactClassification.Medium;
        
        return ImpactClassification.Low;
    }

    private static UrgencyClassification ClassifyUrgency(Incident incident, IncidentImpactAnalysis analysis)
    {
        var hasVIP = analysis.UserImpact.VIPUsersAffected > 0;
        
        if (hasVIP || incident.Urgency?.ToLower() == "critical")
            return UrgencyClassification.Critical;
        if (incident.Urgency?.ToLower() == "high")
            return UrgencyClassification.High;
        if (incident.Urgency?.ToLower() == "medium")
            return UrgencyClassification.Medium;
        
        return UrgencyClassification.Low;
    }

    private static PriorityClassification CalculatePriority(
        ImpactClassification impact, 
        UrgencyClassification urgency)
    {
        int impactIndex = (int)impact - 1;
        int urgencyIndex = (int)urgency - 1;
        
        // Clamp to valid indices
        impactIndex = Math.Max(0, Math.Min(3, impactIndex));
        urgencyIndex = Math.Max(0, Math.Min(3, urgencyIndex));
        
        return PriorityMatrix[impactIndex, urgencyIndex];
    }

    private static string GenerateImpactStatement(IncidentImpactAnalysis analysis)
    {
        var parts = new List<string>();

        parts.Add($"This incident has {analysis.ImpactClassification.ToString().ToLower()} impact");
        
        if (analysis.AffectedServices.Any())
        {
            parts.Add($"affecting {analysis.AffectedServices.Count} service(s)");
        }
        
        if (analysis.UserImpact.TotalUsersAffected > 0)
        {
            parts.Add($"impacting approximately {analysis.UserImpact.TotalUsersAffected} users");
        }
        
        parts.Add($"Calculated priority: {analysis.CalculatedPriority}");

        return string.Join(", ", parts) + ".";
    }

    private static List<string> GenerateRecommendations(IncidentImpactAnalysis analysis)
    {
        var recommendations = new List<string>();

        if (analysis.CalculatedPriority <= PriorityClassification.P2_High)
        {
            recommendations.Add("Engage on-call support team immediately");
            recommendations.Add("Notify service owners and stakeholders");
        }

        if (analysis.PotentialRootCauses.Any())
        {
            recommendations.Add("Investigate recent changes on dependent systems");
        }

        if (analysis.UserImpact.VIPUsersAffected > 0)
        {
            recommendations.Add("Provide dedicated updates to VIP stakeholders");
        }

        if (analysis.AffectedServices.Any(s => s.HasSLA))
        {
            recommendations.Add("Monitor SLA timers closely to avoid breach");
        }

        return recommendations;
    }
}


#endif

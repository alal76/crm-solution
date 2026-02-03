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
/// Interface for analyzing the impact of changes through CMDB relationships.
/// </summary>
public interface IChangeImpactService
{
    /// <summary>
    /// Analyze the full impact of a change based on CMDB relationships.
    /// </summary>
    Task<ChangeImpactAnalysis> AnalyzeChangeImpactAsync(int changeRequestId);
    
    /// <summary>
    /// Get all CIs that would be directly or indirectly affected by changing a set of CIs.
    /// </summary>
    Task<List<ImpactedCI>> GetImpactedCIsAsync(List<int> primaryCIIds, int maxDepth = 3);
    
    /// <summary>
    /// Get all services that would be affected by a change.
    /// </summary>
    Task<List<ImpactedService>> GetImpactedServicesAsync(int changeRequestId);
    
    /// <summary>
    /// Calculate the overall risk score based on impact analysis.
    /// </summary>
    Task<RiskAssessmentResult> CalculateRiskScoreAsync(int changeRequestId);
    
    /// <summary>
    /// Generate impact notification list (who should be informed).
    /// </summary>
    Task<List<ImpactNotification>> GetImpactNotificationsAsync(int changeRequestId);
}

/// <summary>
/// Complete impact analysis for a change request.
/// </summary>
public class ChangeImpactAnalysis
{
    public int ChangeRequestId { get; set; }
    public string ChangeNumber { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; }
    
    // Direct impacts
    public List<ImpactedCI> DirectlyImpactedCIs { get; set; } = new();
    
    // Indirect impacts through relationships
    public List<ImpactedCI> IndirectlyImpactedCIs { get; set; } = new();
    
    // Services affected
    public List<ImpactedService> AffectedServices { get; set; } = new();
    
    // Users/departments affected
    public int EstimatedUsersAffected { get; set; }
    public List<string> AffectedDepartments { get; set; } = new();
    
    // Risk assessment
    public RiskAssessmentResult RiskAssessment { get; set; } = new();
    
    // Summary
    public string ImpactSummary { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Represents a CI impacted by a change.
/// </summary>
public class ImpactedCI
{
    public int ConfigurationItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CIType { get; set; } = string.Empty;
    public ImpactLevel ImpactLevel { get; set; }
    public int DepthFromSource { get; set; }
    public string RelationshipPath { get; set; } = string.Empty;
    public CICriticality Criticality { get; set; }
    public string? Owner { get; set; }
    public bool IsBusinessCritical { get; set; }
}

public enum ImpactLevel
{
    Direct,
    FirstDegree,
    SecondDegree,
    ThirdDegree
}

public enum CICriticality
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Represents a service impacted by a change.
/// </summary>
public class ImpactedService
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceCategory { get; set; } = string.Empty;
    public ServiceCriticality Criticality { get; set; }
    public string? ServiceOwner { get; set; }
    public List<string> SupportedCIs { get; set; } = new();
    public int EstimatedUserCount { get; set; }
    public List<string> AffectedBusinessProcesses { get; set; } = new();
}

public enum ServiceCriticality
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Risk assessment result based on impact analysis.
/// </summary>
public class RiskAssessmentResult
{
    public int OverallRiskScore { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public Dictionary<string, int> RiskFactors { get; set; } = new();
    public List<string> MitigationRecommendations { get; set; } = new();
    public ChangeApprovalLevel RequiredApprovalLevel { get; set; }
}

public enum RiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum ChangeApprovalLevel
{
    Standard,        // Manager approval
    Enhanced,        // CAB review
    Emergency,       // Emergency CAB
    Executive        // Executive sign-off required
}

/// <summary>
/// Notification requirement for impacted stakeholders.
/// </summary>
public class ImpactNotification
{
    public string RecipientType { get; set; } = string.Empty; // User, Group, ServiceOwner, etc.
    public string RecipientId { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public NotificationPriority Priority { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool RequiresAcknowledgement { get; set; }
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Urgent
}

/// <summary>
/// Service for analyzing change impact through CMDB relationships.
/// </summary>
public class ChangeImpactService : IChangeImpactService
{
    private readonly IDbContextResolver _dbContextResolver;
    private readonly ILogger<ChangeImpactService> _logger;

    public ChangeImpactService(
        IDbContextResolver dbContextResolver,
        ILogger<ChangeImpactService> logger)
    {
        _dbContextResolver = dbContextResolver;
        _logger = logger;
    }

    public async Task<ChangeImpactAnalysis> AnalyzeChangeImpactAsync(int changeRequestId)
    {
        var context = _dbContextResolver.ResolveContext();
        
        var change = await context.Changes
            .Include(c => c.ImpactedCIs)
                .ThenInclude(ic => ic.ConfigurationItem)
            .FirstOrDefaultAsync(c => c.ChangeRequestId == changeRequestId);

        if (change == null)
        {
            throw new ArgumentException($"Change request {changeRequestId} not found");
        }

        var analysis = new ChangeImpactAnalysis
        {
            ChangeRequestId = changeRequestId,
            ChangeNumber = change.Number,
            AnalysisDate = DateTime.UtcNow
        };

        // Get primary CI IDs
        var primaryCIIds = change.ImpactedCIs?.Select(ic => ic.ConfigurationItemId).ToList() ?? new List<int>();

        if (!primaryCIIds.Any())
        {
            analysis.ImpactSummary = "No CIs specified for this change";
            analysis.Warnings.Add("No configuration items have been linked to this change");
            return analysis;
        }

        // Get all impacted CIs
        var allImpactedCIs = await GetImpactedCIsAsync(primaryCIIds, 3);
        
        analysis.DirectlyImpactedCIs = allImpactedCIs.Where(ci => ci.ImpactLevel == ImpactLevel.Direct).ToList();
        analysis.IndirectlyImpactedCIs = allImpactedCIs.Where(ci => ci.ImpactLevel != ImpactLevel.Direct).ToList();

        // Get impacted services
        analysis.AffectedServices = await GetImpactedServicesAsync(changeRequestId);

        // Calculate risk
        analysis.RiskAssessment = await CalculateRiskScoreAsync(changeRequestId);

        // Estimate user impact
        analysis.EstimatedUsersAffected = analysis.AffectedServices.Sum(s => s.EstimatedUserCount);
        analysis.AffectedDepartments = analysis.AffectedServices
            .SelectMany(s => s.AffectedBusinessProcesses)
            .Distinct()
            .ToList();

        // Generate summary
        analysis.ImpactSummary = GenerateImpactSummary(analysis);
        
        // Add warnings
        if (analysis.DirectlyImpactedCIs.Any(ci => ci.IsBusinessCritical))
        {
            analysis.Warnings.Add("This change affects business-critical configuration items");
        }
        
        if (analysis.IndirectlyImpactedCIs.Count > 10)
        {
            analysis.Warnings.Add($"Wide-reaching impact: {analysis.IndirectlyImpactedCIs.Count} CIs indirectly affected");
        }

        // Recommendations
        if (analysis.RiskAssessment.RiskLevel >= RiskLevel.High)
        {
            analysis.Recommendations.Add("Consider implementing this change in phases");
            analysis.Recommendations.Add("Prepare detailed rollback procedures");
            analysis.Recommendations.Add("Schedule during lowest-impact time window");
        }

        _logger.LogInformation(
            "Impact analysis for change {ChangeNumber}: {DirectCount} direct, {IndirectCount} indirect CIs, Risk: {RiskLevel}",
            change.Number, analysis.DirectlyImpactedCIs.Count, 
            analysis.IndirectlyImpactedCIs.Count, analysis.RiskAssessment.RiskLevel);

        return analysis;
    }

    public async Task<List<ImpactedCI>> GetImpactedCIsAsync(List<int> primaryCIIds, int maxDepth = 3)
    {
        var context = _dbContextResolver.ResolveContext();
        var impactedCIs = new List<ImpactedCI>();
        var processedIds = new HashSet<int>();

        // Get primary CIs (direct impact)
        var primaryCIs = await context.ITSMConfigurationItems
            .Where(ci => primaryCIIds.Contains(ci.ConfigurationItemId))
            .ToListAsync();

        foreach (var ci in primaryCIs)
        {
            processedIds.Add(ci.ConfigurationItemId);
            impactedCIs.Add(new ImpactedCI
            {
                ConfigurationItemId = ci.ConfigurationItemId,
                Name = ci.Name,
                CIType = ci.CIType,
                ImpactLevel = ImpactLevel.Direct,
                DepthFromSource = 0,
                RelationshipPath = ci.Name,
                Criticality = ParseCriticality(ci.Criticality),
                IsBusinessCritical = ci.Criticality == "Critical"
            });
        }

        // Traverse relationships for indirect impact
        var currentDepthIds = primaryCIIds;
        
        for (int depth = 1; depth <= maxDepth; depth++)
        {
            var relationships = await context.ITSMCIRelationships
                .Include(r => r.SourceCI)
                .Include(r => r.TargetCI)
                .Where(r => currentDepthIds.Contains(r.SourceCIId) || currentDepthIds.Contains(r.TargetCIId))
                .ToListAsync();

            var nextDepthIds = new List<int>();

            foreach (var rel in relationships)
            {
                // Get the related CI (the one not in currentDepthIds)
                int relatedCIId;
                ITSMConfigurationItem? relatedCI;
                string relationshipPath;

                if (currentDepthIds.Contains(rel.SourceCIId))
                {
                    relatedCIId = rel.TargetCIId;
                    relatedCI = rel.TargetCI;
                    relationshipPath = $"via {rel.RelationshipType} to {relatedCI?.Name}";
                }
                else
                {
                    relatedCIId = rel.SourceCIId;
                    relatedCI = rel.SourceCI;
                    relationshipPath = $"via {rel.RelationshipType} from {relatedCI?.Name}";
                }

                if (!processedIds.Contains(relatedCIId) && relatedCI != null)
                {
                    processedIds.Add(relatedCIId);
                    nextDepthIds.Add(relatedCIId);

                    impactedCIs.Add(new ImpactedCI
                    {
                        ConfigurationItemId = relatedCIId,
                        Name = relatedCI.Name,
                        CIType = relatedCI.CIType,
                        ImpactLevel = depth switch
                        {
                            1 => ImpactLevel.FirstDegree,
                            2 => ImpactLevel.SecondDegree,
                            _ => ImpactLevel.ThirdDegree
                        },
                        DepthFromSource = depth,
                        RelationshipPath = relationshipPath,
                        Criticality = ParseCriticality(relatedCI.Criticality),
                        IsBusinessCritical = relatedCI.Criticality == "Critical"
                    });
                }
            }

            currentDepthIds = nextDepthIds;
            
            if (!nextDepthIds.Any())
                break;
        }

        return impactedCIs;
    }

    public async Task<List<ImpactedService>> GetImpactedServicesAsync(int changeRequestId)
    {
        var context = _dbContextResolver.ResolveContext();
        
        // Get the change and its CIs
        var change = await context.Changes
            .Include(c => c.ImpactedCIs)
            .FirstOrDefaultAsync(c => c.ChangeRequestId == changeRequestId);

        if (change?.ImpactedCIs == null || !change.ImpactedCIs.Any())
        {
            return new List<ImpactedService>();
        }

        var ciIds = change.ImpactedCIs.Select(ic => ic.ConfigurationItemId).ToList();

        // Get services from catalog that are related to these CIs
        // In a full implementation, there would be a ServiceCI relationship table
        var services = await context.ITSMServiceCatalogItems
            .Where(s => s.IsActive)
            .ToListAsync();

        // For demo, return services as potentially impacted
        return services.Select(s => new ImpactedService
        {
            ServiceId = s.ServiceCatalogItemId,
            ServiceName = s.Name,
            ServiceCategory = s.Category ?? "General",
            Criticality = ServiceCriticality.Medium,
            EstimatedUserCount = 50, // Would be calculated from service subscriptions
            AffectedBusinessProcesses = new List<string> { "Business Operations" }
        }).Take(5).ToList();
    }

    public async Task<RiskAssessmentResult> CalculateRiskScoreAsync(int changeRequestId)
    {
        var context = _dbContextResolver.ResolveContext();
        
        var change = await context.Changes
            .Include(c => c.ImpactedCIs)
                .ThenInclude(ic => ic.ConfigurationItem)
            .FirstOrDefaultAsync(c => c.ChangeRequestId == changeRequestId);

        if (change == null)
        {
            return new RiskAssessmentResult
            {
                OverallRiskScore = 0,
                RiskLevel = RiskLevel.Low
            };
        }

        var result = new RiskAssessmentResult
        {
            RiskFactors = new Dictionary<string, int>()
        };

        int totalScore = 0;

        // Factor 1: Change Type (0-25 points)
        var changeTypeScore = change.ChangeType switch
        {
            ChangeType.Standard => 5,
            ChangeType.Normal => 15,
            ChangeType.Emergency => 25,
            _ => 10
        };
        result.RiskFactors["Change Type"] = changeTypeScore;
        totalScore += changeTypeScore;

        // Factor 2: Number of CIs affected (0-25 points)
        var ciCount = change.ImpactedCIs?.Count ?? 0;
        var ciScore = ciCount switch
        {
            0 => 0,
            1 => 5,
            <= 3 => 10,
            <= 5 => 15,
            <= 10 => 20,
            _ => 25
        };
        result.RiskFactors["CI Count"] = ciScore;
        totalScore += ciScore;

        // Factor 3: Critical CIs (0-25 points)
        var hasCriticalCIs = change.ImpactedCIs?.Any(ic => ic.ConfigurationItem?.Criticality == "Critical") ?? false;
        var criticalScore = hasCriticalCIs ? 25 : 0;
        result.RiskFactors["Critical CIs"] = criticalScore;
        totalScore += criticalScore;

        // Factor 4: Implementation complexity (0-25 points)
        var complexityScore = 10; // Default medium complexity
        if (!string.IsNullOrEmpty(change.RiskLevel))
        {
            complexityScore = change.RiskLevel.ToLower() switch
            {
                "low" => 5,
                "medium" => 15,
                "high" => 25,
                _ => 10
            };
        }
        result.RiskFactors["Complexity"] = complexityScore;
        totalScore += complexityScore;

        result.OverallRiskScore = totalScore;
        result.RiskLevel = totalScore switch
        {
            <= 25 => RiskLevel.Low,
            <= 50 => RiskLevel.Medium,
            <= 75 => RiskLevel.High,
            _ => RiskLevel.Critical
        };

        result.RequiredApprovalLevel = result.RiskLevel switch
        {
            RiskLevel.Low => ChangeApprovalLevel.Standard,
            RiskLevel.Medium => ChangeApprovalLevel.Enhanced,
            RiskLevel.High => ChangeApprovalLevel.Enhanced,
            RiskLevel.Critical => ChangeApprovalLevel.Executive,
            _ => ChangeApprovalLevel.Standard
        };

        // Add mitigation recommendations based on factors
        if (result.RiskLevel >= RiskLevel.High)
        {
            result.MitigationRecommendations.Add("Develop detailed test plan before implementation");
            result.MitigationRecommendations.Add("Ensure rollback procedure is documented and tested");
        }
        if (hasCriticalCIs)
        {
            result.MitigationRecommendations.Add("Notify business stakeholders of potential impact");
            result.MitigationRecommendations.Add("Consider phased implementation approach");
        }
        if (ciCount > 5)
        {
            result.MitigationRecommendations.Add("Break change into smaller, manageable pieces if possible");
        }

        return result;
    }

    public async Task<List<ImpactNotification>> GetImpactNotificationsAsync(int changeRequestId)
    {
        var context = _dbContextResolver.ResolveContext();
        var notifications = new List<ImpactNotification>();

        var change = await context.Changes
            .Include(c => c.ImpactedCIs)
                .ThenInclude(ic => ic.ConfigurationItem)
            .Include(c => c.RequestedBy)
            .Include(c => c.AssignedTo)
            .FirstOrDefaultAsync(c => c.ChangeRequestId == changeRequestId);

        if (change == null) return notifications;

        // Notify change requestor
        if (change.RequestedBy != null)
        {
            notifications.Add(new ImpactNotification
            {
                RecipientType = "Requestor",
                RecipientId = change.RequestedBy.Id.ToString(),
                RecipientName = $"{change.RequestedBy.FirstName} {change.RequestedBy.LastName}".Trim(),
                Email = change.RequestedBy.Email,
                Priority = NotificationPriority.Normal,
                Reason = "You are the requestor of this change",
                RequiresAcknowledgement = false
            });
        }

        // Notify CI owners
        var ciOwners = change.ImpactedCIs?
            .Where(ic => ic.ConfigurationItem?.OwnerId != null)
            .Select(ic => ic.ConfigurationItem!)
            .GroupBy(ci => ci.OwnerId)
            .ToList() ?? new();

        foreach (var ownerGroup in ciOwners)
        {
            var ciNames = string.Join(", ", ownerGroup.Select(ci => ci.Name));
            notifications.Add(new ImpactNotification
            {
                RecipientType = "CIOwner",
                RecipientId = ownerGroup.Key.ToString()!,
                RecipientName = $"CI Owner ({ciNames})",
                Priority = NotificationPriority.High,
                Reason = $"You own the following impacted CIs: {ciNames}",
                RequiresAcknowledgement = true
            });
        }

        // Service owners based on impact
        var services = await GetImpactedServicesAsync(changeRequestId);
        foreach (var service in services.Where(s => !string.IsNullOrEmpty(s.ServiceOwner)))
        {
            notifications.Add(new ImpactNotification
            {
                RecipientType = "ServiceOwner",
                RecipientName = service.ServiceOwner!,
                Priority = service.Criticality >= ServiceCriticality.High 
                    ? NotificationPriority.Urgent 
                    : NotificationPriority.Normal,
                Reason = $"You own the service: {service.ServiceName}",
                RequiresAcknowledgement = service.Criticality >= ServiceCriticality.High
            });
        }

        return notifications;
    }

    private static CICriticality ParseCriticality(string? criticality)
    {
        return criticality?.ToLower() switch
        {
            "critical" => CICriticality.Critical,
            "high" => CICriticality.High,
            "medium" => CICriticality.Medium,
            "low" => CICriticality.Low,
            _ => CICriticality.Medium
        };
    }

    private static string GenerateImpactSummary(ChangeImpactAnalysis analysis)
    {
        var parts = new List<string>();

        var totalCIs = analysis.DirectlyImpactedCIs.Count + analysis.IndirectlyImpactedCIs.Count;
        parts.Add($"{totalCIs} configuration items affected ({analysis.DirectlyImpactedCIs.Count} direct, {analysis.IndirectlyImpactedCIs.Count} indirect)");

        if (analysis.AffectedServices.Any())
        {
            parts.Add($"{analysis.AffectedServices.Count} services impacted");
        }

        if (analysis.EstimatedUsersAffected > 0)
        {
            parts.Add($"Estimated {analysis.EstimatedUsersAffected} users affected");
        }

        parts.Add($"Risk Level: {analysis.RiskAssessment.RiskLevel}");

        return string.Join(". ", parts) + ".";
    }
}


#endif

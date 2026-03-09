// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Interface for CI/Asset lifecycle management.
/// </summary>
public interface IAssetLifecycleService
{
    /// <summary>
    /// Get the current lifecycle stage of an asset.
    /// </summary>
    Task<AssetLifecycleState> GetLifecycleStateAsync(int configurationItemId);

    /// <summary>
    /// Transition an asset to a new lifecycle stage.
    /// </summary>
    Task<AssetLifecycleTransition> TransitionAsync(int configurationItemId, LifecycleStage targetStage, int performedById, string? notes = null);

    /// <summary>
    /// Get lifecycle history for an asset.
    /// </summary>
    Task<List<AssetLifecycleTransition>> GetLifecycleHistoryAsync(int configurationItemId);

    /// <summary>
    /// Get assets approaching end-of-life or warranty expiration.
    /// </summary>
    Task<List<AssetEndOfLifeAlert>> GetEndOfLifeAlertsAsync(int daysAhead = 90);

    /// <summary>
    /// Get assets due for refresh/replacement.
    /// </summary>
    Task<List<AssetRefreshCandidate>> GetRefreshCandidatesAsync();

    /// <summary>
    /// Schedule asset retirement.
    /// </summary>
    Task<AssetRetirementSchedule> ScheduleRetirementAsync(int configurationItemId, DateTime retirementDate, int scheduledById, string reason);

    /// <summary>
    /// Get asset utilization metrics.
    /// </summary>
    Task<AssetUtilizationMetrics> GetUtilizationMetricsAsync(int configurationItemId);

    /// <summary>
    /// Get lifecycle cost analysis for an asset.
    /// </summary>
    Task<LifecycleCostAnalysis> GetCostAnalysisAsync(int configurationItemId);
}

/// <summary>
/// Lifecycle stages for assets.
/// </summary>
public enum LifecycleStage
{
    Planned,
    Ordered,
    Received,
    InStock,
    Deployed,
    InMaintenance,
    EndOfLife,
    Retired,
    Disposed
}

/// <summary>
/// Current lifecycle state of an asset.
/// </summary>
public class AssetLifecycleState
{
    public int ConfigurationItemId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public LifecycleStage CurrentStage { get; set; }
    public DateTime StageEnteredAt { get; set; }
    public int DaysInCurrentStage { get; set; }
    public List<LifecycleStage> AllowedTransitions { get; set; } = new();
    public DateTime? WarrantyExpirationDate { get; set; }
    public DateTime? EndOfSupportDate { get; set; }
    public DateTime? EndOfLifeDate { get; set; }
    public DateTime? ScheduledRetirementDate { get; set; }
    public string? HealthStatus { get; set; }
}

/// <summary>
/// Record of a lifecycle transition.
/// </summary>
public class AssetLifecycleTransition
{
    public int TransitionId { get; set; }
    public int ConfigurationItemId { get; set; }
    public LifecycleStage FromStage { get; set; }
    public LifecycleStage ToStage { get; set; }
    public DateTime TransitionedAt { get; set; }
    public int PerformedById { get; set; }
    public string? PerformedByName { get; set; }
    public string? Notes { get; set; }
    public string? RelatedChangeNumber { get; set; }
}

/// <summary>
/// Alert for assets approaching end-of-life.
/// </summary>
public class AssetEndOfLifeAlert
{
    public int ConfigurationItemId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string CIType { get; set; } = string.Empty;
    public AlertType Type { get; set; }
    public DateTime AlertDate { get; set; }
    public int DaysUntil { get; set; }
    public string Criticality { get; set; } = string.Empty;
    public string? RecommendedAction { get; set; }
    public decimal? EstimatedReplacementCost { get; set; }
}

public enum AlertType
{
    WarrantyExpiring,
    EndOfSupport,
    EndOfLife,
    ScheduledRetirement,
    HighRiskAge,
    MaintenanceDue
}

/// <summary>
/// Candidate for asset refresh/replacement.
/// </summary>
public class AssetRefreshCandidate
{
    public int ConfigurationItemId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string CIType { get; set; } = string.Empty;
    public RefreshReason PrimaryReason { get; set; }
    public int RefreshScore { get; set; } // 1-100, higher = more urgent
    public int AgeInMonths { get; set; }
    public int IncidentCount { get; set; }
    public decimal TotalMaintenanceCost { get; set; }
    public List<string> ReplacementOptions { get; set; } = new();
    public decimal EstimatedReplacementCost { get; set; }
}

public enum RefreshReason
{
    Age,
    Performance,
    HighMaintenance,
    EndOfSupport,
    CapacityLimits,
    SecurityRisk,
    BusinessRequirement
}

/// <summary>
/// Scheduled asset retirement.
/// </summary>
public class AssetRetirementSchedule
{
    public int ScheduleId { get; set; }
    public int ConfigurationItemId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public int ScheduledById { get; set; }
    public string? ScheduledByName { get; set; }
    public string Reason { get; set; } = string.Empty;
    public RetirementStatus Status { get; set; }
    public int? ReplacementCIId { get; set; }
    public string? ReplacementCIName { get; set; }
    public List<RetirementTask> Tasks { get; set; } = new();
}

public enum RetirementStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
    Deferred
}

public class RetirementTask
{
    public string TaskName { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? AssignedTo { get; set; }
}

/// <summary>
/// Asset utilization metrics.
/// </summary>
public class AssetUtilizationMetrics
{
    public int ConfigurationItemId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public DateTime MeasuredAt { get; set; }
    public double UptimePercentage { get; set; }
    public double AverageCPUUtilization { get; set; }
    public double AverageMemoryUtilization { get; set; }
    public double AverageStorageUtilization { get; set; }
    public int TotalIncidents { get; set; }
    public double MTBF { get; set; } // Mean time between failures (hours)
    public double MTTR { get; set; } // Mean time to repair (hours)
    public UtilizationStatus OverallStatus { get; set; }
}

public enum UtilizationStatus
{
    Underutilized,
    Optimal,
    HighUtilization,
    Overloaded,
    Unknown
}

/// <summary>
/// Lifecycle cost analysis for an asset.
/// </summary>
public class LifecycleCostAnalysis
{
    public int ConfigurationItemId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public decimal AcquisitionCost { get; set; }
    public decimal DeploymentCost { get; set; }
    public decimal TotalMaintenanceCost { get; set; }
    public decimal TotalOperatingCost { get; set; }
    public decimal TotalIncidentCost { get; set; }
    public decimal TotalCostOfOwnership { get; set; }
    public decimal MonthlyCost { get; set; }
    public int AgeMonths { get; set; }
    public decimal CostPerIncident { get; set; }
    public decimal ProjectedDisposalCost { get; set; }
    public decimal RemainingValue { get; set; }
}

/// <summary>
/// Service for managing CI/Asset lifecycle.
/// </summary>
public class AssetLifecycleService : IAssetLifecycleService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<AssetLifecycleService> _logger;

    // In-memory storage for transitions (would be database in production)
    private static readonly List<AssetLifecycleTransition> _transitions = new();
    private static readonly List<AssetRetirementSchedule> _retirements = new();
    private static readonly object _collectionsLock = new();
    private static int _nextTransitionId = 0;
    private static int _nextScheduleId = 0;

    // Allowed lifecycle transitions
    private static readonly Dictionary<LifecycleStage, LifecycleStage[]> AllowedTransitions = new()
    {
        { LifecycleStage.Planned, new[] { LifecycleStage.Ordered } },
        { LifecycleStage.Ordered, new[] { LifecycleStage.Received, LifecycleStage.Planned } },
        { LifecycleStage.Received, new[] { LifecycleStage.InStock, LifecycleStage.Deployed } },
        { LifecycleStage.InStock, new[] { LifecycleStage.Deployed, LifecycleStage.Disposed } },
        { LifecycleStage.Deployed, new[] { LifecycleStage.InMaintenance, LifecycleStage.EndOfLife, LifecycleStage.InStock } },
        { LifecycleStage.InMaintenance, new[] { LifecycleStage.Deployed, LifecycleStage.EndOfLife, LifecycleStage.Retired } },
        { LifecycleStage.EndOfLife, new[] { LifecycleStage.Retired, LifecycleStage.InMaintenance } },
        { LifecycleStage.Retired, new[] { LifecycleStage.Disposed } },
        { LifecycleStage.Disposed, Array.Empty<LifecycleStage>() }
    };

    public AssetLifecycleService(
        ICrmDbContext context,
        ILogger<AssetLifecycleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AssetLifecycleState> GetLifecycleStateAsync(int configurationItemId)
    {
        var ci = await _context.ConfigurationItems
            .FirstOrDefaultAsync(c => c.CIId == configurationItemId);

        if (ci == null)
        {
            throw new ArgumentException($"Configuration item {configurationItemId} not found");
        }

        var currentStage = MapStatusToLifecycleStage(ci.OperationalStatus);
        AssetLifecycleTransition? lastTransition;
        AssetRetirementSchedule? scheduledRetirement;
        lock (_collectionsLock)
        {
            lastTransition = _transitions
                .Where(t => t.ConfigurationItemId == configurationItemId)
                .OrderByDescending(t => t.TransitionedAt)
                .FirstOrDefault();
            scheduledRetirement = _retirements
                .FirstOrDefault(r => r.ConfigurationItemId == configurationItemId &&
                                    r.Status == RetirementStatus.Scheduled);
        }

        var stageEnteredAt = lastTransition?.TransitionedAt ?? ci.CreatedAt;

        return new AssetLifecycleState
        {
            ConfigurationItemId = configurationItemId,
            AssetName = ci.CIName,
            CurrentStage = currentStage,
            StageEnteredAt = stageEnteredAt,
            DaysInCurrentStage = (int)(DateTime.UtcNow - stageEnteredAt).TotalDays,
            AllowedTransitions = AllowedTransitions.TryGetValue(currentStage, out var transitions)
                ? transitions.ToList()
                : new List<LifecycleStage>(),
            WarrantyExpirationDate = ci.WarrantyExpiration,
            EndOfSupportDate = ci.EndOfSupportDate,
            EndOfLifeDate = ci.EndOfLifeDate,
            ScheduledRetirementDate = scheduledRetirement?.ScheduledDate,
            HealthStatus = DetermineHealthStatus(ci)
        };
    }

    public async Task<AssetLifecycleTransition> TransitionAsync(
        int configurationItemId,
        LifecycleStage targetStage,
        int performedById,
        string? notes = null)
    {
        var ci = await _context.ConfigurationItems
            .FirstOrDefaultAsync(c => c.CIId == configurationItemId);

        if (ci == null)
        {
            throw new ArgumentException($"Configuration item {configurationItemId} not found");
        }

        var currentStage = MapStatusToLifecycleStage(ci.OperationalStatus);

        // Validate transition
        if (!AllowedTransitions.TryGetValue(currentStage, out var allowed) ||
            !allowed.Contains(targetStage))
        {
            throw new InvalidOperationException(
                $"Transition from {currentStage} to {targetStage} is not allowed");
        }

        // Create transition record
        var transition = new AssetLifecycleTransition
        {
            TransitionId = Interlocked.Increment(ref _nextTransitionId),
            ConfigurationItemId = configurationItemId,
            FromStage = currentStage,
            ToStage = targetStage,
            TransitionedAt = DateTime.UtcNow,
            PerformedById = performedById,
            Notes = notes
        };

        lock (_collectionsLock) { _transitions.Add(transition); }

        // Update CI status
        ci.OperationalStatus = MapLifecycleStageToOperationalStatus(targetStage);
        ci.ModifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Asset {AssetName} transitioned from {From} to {To}",
            ci.CIName, currentStage, targetStage);

        return transition;
    }

    public async Task<List<AssetLifecycleTransition>> GetLifecycleHistoryAsync(int configurationItemId)
    {
        List<AssetLifecycleTransition> snapshot;
        lock (_collectionsLock) { snapshot = _transitions.ToList(); }
        return await Task.FromResult(
            snapshot
                .Where(t => t.ConfigurationItemId == configurationItemId)
                .OrderByDescending(t => t.TransitionedAt)
                .ToList());
    }

    public async Task<List<AssetEndOfLifeAlert>> GetEndOfLifeAlertsAsync(int daysAhead = 90)
    {
        var alerts = new List<AssetEndOfLifeAlert>();
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);

        var cis = await _context.ConfigurationItems
            .Where(ci => ci.OperationalStatus != OperationalStatus.Retired)
            .Where(ci =>
                (ci.WarrantyExpiration.HasValue && ci.WarrantyExpiration <= cutoffDate) ||
                (ci.EndOfSupportDate.HasValue && ci.EndOfSupportDate <= cutoffDate) ||
                (ci.EndOfLifeDate.HasValue && ci.EndOfLifeDate <= cutoffDate))
            .ToListAsync();

        foreach (var ci in cis)
        {
            if (ci.WarrantyExpiration.HasValue && ci.WarrantyExpiration <= cutoffDate)
            {
                alerts.Add(CreateAlert(ci, AlertType.WarrantyExpiring, ci.WarrantyExpiration.Value));
            }

            if (ci.EndOfSupportDate.HasValue && ci.EndOfSupportDate <= cutoffDate)
            {
                alerts.Add(CreateAlert(ci, AlertType.EndOfSupport, ci.EndOfSupportDate.Value));
            }

            if (ci.EndOfLifeDate.HasValue && ci.EndOfLifeDate <= cutoffDate)
            {
                alerts.Add(CreateAlert(ci, AlertType.EndOfLife, ci.EndOfLifeDate.Value));
            }
        }

        // Check scheduled retirements
        List<AssetRetirementSchedule> retirementsSnapshot;
        lock (_collectionsLock) { retirementsSnapshot = _retirements.ToList(); }
        foreach (var retirement in retirementsSnapshot.Where(r => r.Status == RetirementStatus.Scheduled && r.ScheduledDate <= cutoffDate))
        {
            var ci = await _context.ConfigurationItems.FindAsync(retirement.ConfigurationItemId);
            if (ci != null)
            {
                alerts.Add(CreateAlert(ci, AlertType.ScheduledRetirement, retirement.ScheduledDate));
            }
        }

        return alerts.OrderBy(a => a.AlertDate).ToList();
    }

    public async Task<List<AssetRefreshCandidate>> GetRefreshCandidatesAsync()
    {
        var candidates = new List<AssetRefreshCandidate>();

        var cis = await _context.ConfigurationItems
            .Where(ci => ci.OperationalStatus == OperationalStatus.Operational)
            .ToListAsync();

        foreach (var ci in cis)
        {
            var ageMonths = ci.PurchaseDate.HasValue
                ? (int)((DateTime.UtcNow - ci.PurchaseDate.Value).TotalDays / 30)
                : 0;

            // Get incident count
            var incidentCount = await _context.Incidents
                .CountAsync(i => i.ConfigurationItemId == ci.CIId);

            // Calculate refresh score
            var score = CalculateRefreshScore(ci, ageMonths, incidentCount);

            // Only include if score is high enough
            if (score >= 50)
            {
                var reason = DetermineRefreshReason(ci, ageMonths, incidentCount);

                candidates.Add(new AssetRefreshCandidate
                {
                    ConfigurationItemId = ci.CIId,
                    AssetName = ci.CIName,
                    CIType = ci.CIType.ToString(),
                    PrimaryReason = reason,
                    RefreshScore = score,
                    AgeInMonths = ageMonths,
                    IncidentCount = incidentCount,
                    TotalMaintenanceCost = 0, // Would come from cost tracking
                    EstimatedReplacementCost = EstimateReplacementCost(ci),
                    ReplacementOptions = GetReplacementOptions(ci)
                });
            }
        }

        return candidates.OrderByDescending(c => c.RefreshScore).ToList();
    }

    public async Task<AssetRetirementSchedule> ScheduleRetirementAsync(
        int configurationItemId,
        DateTime retirementDate,
        int scheduledById,
        string reason)
    {
        var ci = await _context.ConfigurationItems
            .FirstOrDefaultAsync(c => c.CIId == configurationItemId);

        if (ci == null)
        {
            throw new ArgumentException($"Configuration item {configurationItemId} not found");
        }

        var schedule = new AssetRetirementSchedule
        {
            ScheduleId = Interlocked.Increment(ref _nextScheduleId),
            ConfigurationItemId = configurationItemId,
            AssetName = ci.CIName,
            ScheduledDate = retirementDate,
            ScheduledById = scheduledById,
            Reason = reason,
            Status = RetirementStatus.Scheduled,
            Tasks = GetRetirementTasks(ci)
        };

        lock (_collectionsLock) { _retirements.Add(schedule); }

        _logger.LogInformation(
            "Scheduled retirement for asset {AssetName} on {Date}",
            ci.CIName, retirementDate);

        return schedule;
    }

    public async Task<AssetUtilizationMetrics> GetUtilizationMetricsAsync(int configurationItemId)
    {
        var ci = await _context.ConfigurationItems
            .FirstOrDefaultAsync(c => c.CIId == configurationItemId);

        if (ci == null)
        {
            throw new ArgumentException($"Configuration item {configurationItemId} not found");
        }

        // Get incident statistics
        var incidents = await _context.Incidents
            .Where(i => i.ConfigurationItemId == configurationItemId)
            .Where(i => i.CreatedAt > DateTime.UtcNow.AddMonths(-12))
            .ToListAsync();

        var resolvedIncidents = incidents
            .Where(i => i.ResolvedAt.HasValue)
            .ToList();

        // Calculate MTBF and MTTR
        double mtbf = 0;
        double mttr = 0;

        if (incidents.Any())
        {
            var totalHours = (DateTime.UtcNow - ci.CreatedAt).TotalHours;
            mtbf = totalHours / (incidents.Count + 1); // Mean time between failures
        }

        if (resolvedIncidents.Any())
        {
            mttr = resolvedIncidents
                .Average(i => (i.ResolvedAt!.Value - i.CreatedAt).TotalHours);
        }

        // Simulated utilization (would come from monitoring in production)
        var random = new Random(configurationItemId); // Consistent random for demo

        return new AssetUtilizationMetrics
        {
            ConfigurationItemId = configurationItemId,
            AssetName = ci.CIName,
            MeasuredAt = DateTime.UtcNow,
            UptimePercentage = 99.5 - (incidents.Count * 0.1),
            AverageCPUUtilization = 20 + random.NextDouble() * 60,
            AverageMemoryUtilization = 30 + random.NextDouble() * 50,
            AverageStorageUtilization = 40 + random.NextDouble() * 40,
            TotalIncidents = incidents.Count,
            MTBF = mtbf,
            MTTR = mttr,
            OverallStatus = DetermineUtilizationStatus(ci, incidents.Count)
        };
    }

    public async Task<LifecycleCostAnalysis> GetCostAnalysisAsync(int configurationItemId)
    {
        var ci = await _context.ConfigurationItems
            .FirstOrDefaultAsync(c => c.CIId == configurationItemId);

        if (ci == null)
        {
            throw new ArgumentException($"Configuration item {configurationItemId} not found");
        }

        // Get incident count for cost estimation
        var incidentCount = await _context.Incidents
            .CountAsync(i => i.ConfigurationItemId == configurationItemId);

        var ageMonths = ci.PurchaseDate.HasValue
            ? (int)((DateTime.UtcNow - ci.PurchaseDate.Value).TotalDays / 30)
            : 12; // Default 12 months if unknown

        // Estimate costs (would come from actual cost tracking in production)
        var acquisitionCost = ci.PurchaseCost ?? EstimateAcquisitionCost(ci);
        var monthlyOperating = EstimateMonthlyOperatingCost(ci);
        var incidentCostPerUnit = 500m; // Estimated cost per incident

        return new LifecycleCostAnalysis
        {
            ConfigurationItemId = configurationItemId,
            AssetName = ci.CIName,
            AcquisitionCost = acquisitionCost,
            DeploymentCost = acquisitionCost * 0.1m, // Estimate 10% of acquisition
            TotalMaintenanceCost = monthlyOperating * 0.3m * ageMonths,
            TotalOperatingCost = monthlyOperating * ageMonths,
            TotalIncidentCost = incidentCount * incidentCostPerUnit,
            TotalCostOfOwnership = acquisitionCost + (monthlyOperating * ageMonths) + (incidentCount * incidentCostPerUnit),
            MonthlyCost = monthlyOperating,
            AgeMonths = ageMonths,
            CostPerIncident = incidentCount > 0 ? incidentCostPerUnit : 0,
            ProjectedDisposalCost = acquisitionCost * 0.05m,
            RemainingValue = CalculateRemainingValue(acquisitionCost, ageMonths)
        };
    }

    private static LifecycleStage MapStatusToLifecycleStage(OperationalStatus status)
    {
        return status switch
        {
            OperationalStatus.InStock => LifecycleStage.InStock,
            OperationalStatus.Operational => LifecycleStage.Deployed,
            OperationalStatus.NonOperational => LifecycleStage.InMaintenance,
            OperationalStatus.UnderRepair => LifecycleStage.InMaintenance,
            OperationalStatus.Retired => LifecycleStage.Retired,
            _ => LifecycleStage.Deployed
        };
    }

    private static OperationalStatus MapLifecycleStageToOperationalStatus(LifecycleStage stage)
    {
        return stage switch
        {
            LifecycleStage.Planned => OperationalStatus.InStock,
            LifecycleStage.Ordered => OperationalStatus.InStock,
            LifecycleStage.Received => OperationalStatus.InStock,
            LifecycleStage.InStock => OperationalStatus.InStock,
            LifecycleStage.Deployed => OperationalStatus.Operational,
            LifecycleStage.InMaintenance => OperationalStatus.UnderRepair,
            LifecycleStage.EndOfLife => OperationalStatus.Retired,
            LifecycleStage.Retired => OperationalStatus.Retired,
            LifecycleStage.Disposed => OperationalStatus.Retired,
            _ => OperationalStatus.Operational
        };
    }

    private static string DetermineHealthStatus(ConfigurationItem ci)
    {
        if (ci.OperationalStatus == OperationalStatus.Retired)
            return "Inactive";

        if (ci.EndOfLifeDate.HasValue && ci.EndOfLifeDate <= DateTime.UtcNow)
            return "Critical";

        if (ci.EndOfSupportDate.HasValue && ci.EndOfSupportDate <= DateTime.UtcNow)
            return "Warning";

        if (ci.WarrantyExpiration.HasValue && ci.WarrantyExpiration <= DateTime.UtcNow)
            return "At Risk";

        return "Healthy";
    }

    private static AssetEndOfLifeAlert CreateAlert(ConfigurationItem ci, AlertType type, DateTime alertDate)
    {
        var daysUntil = (int)(alertDate - DateTime.UtcNow).TotalDays;

        return new AssetEndOfLifeAlert
        {
            ConfigurationItemId = ci.CIId,
            AssetName = ci.CIName,
            CIType = ci.CIType.ToString(),
            Type = type,
            AlertDate = alertDate,
            DaysUntil = daysUntil,
            Criticality = ci.Criticality?.ToString() ?? "Medium",
            RecommendedAction = GetRecommendedAction(type, daysUntil),
            EstimatedReplacementCost = EstimateReplacementCost(ci)
        };
    }

    private static string GetRecommendedAction(AlertType type, int daysUntil)
    {
        if (daysUntil < 0)
        {
            return type switch
            {
                AlertType.WarrantyExpiring => "Warranty has expired - consider extended warranty or replacement",
                AlertType.EndOfSupport => "End of support reached - prioritize replacement planning",
                AlertType.EndOfLife => "End of life reached - immediate replacement required",
                _ => "Review and plan replacement"
            };
        }

        return type switch
        {
            AlertType.WarrantyExpiring => daysUntil <= 30
                ? "Extend warranty now or plan replacement"
                : "Begin warranty extension or replacement planning",
            AlertType.EndOfSupport => daysUntil <= 30
                ? "Finalize migration/replacement plan"
                : "Begin planning replacement or migration",
            AlertType.EndOfLife => daysUntil <= 30
                ? "Execute replacement immediately"
                : "Accelerate replacement planning",
            AlertType.ScheduledRetirement => "Ensure retirement tasks are completed",
            _ => "Review asset status"
        };
    }

    private static int CalculateRefreshScore(ConfigurationItem ci, int ageMonths, int incidentCount)
    {
        int score = 0;

        // Age factor (up to 40 points)
        int expectedLifeMonths = GetExpectedLifeMonths(ci.CIType);
        int ageScore = (int)(40.0 * ageMonths / expectedLifeMonths);
        score += Math.Min(40, ageScore);

        // Incident factor (up to 30 points)
        int incidentScore = Math.Min(30, incidentCount * 5);
        score += incidentScore;

        // End of support/life factor (up to 30 points)
        if (ci.EndOfLifeDate.HasValue && ci.EndOfLifeDate <= DateTime.UtcNow.AddMonths(6))
            score += 30;
        else if (ci.EndOfSupportDate.HasValue && ci.EndOfSupportDate <= DateTime.UtcNow.AddMonths(6))
            score += 20;
        else if (ci.WarrantyExpiration.HasValue && ci.WarrantyExpiration <= DateTime.UtcNow)
            score += 10;

        return Math.Min(100, score);
    }

    private static int GetExpectedLifeMonths(CIType ciType)
    {
        return ciType switch
        {
            CIType.Server => 60,          // 5 years
            CIType.WorkStation => 48,     // 4 years
            CIType.NetworkDevice => 84,   // 7 years
            CIType.Storage => 60,         // 5 years
            _ => 60
        };
    }

    private static RefreshReason DetermineRefreshReason(ConfigurationItem ci, int ageMonths, int incidentCount)
    {
        if (ci.EndOfLifeDate.HasValue && ci.EndOfLifeDate <= DateTime.UtcNow.AddMonths(6))
            return RefreshReason.EndOfSupport;

        if (incidentCount >= 10)
            return RefreshReason.HighMaintenance;

        var expectedLife = GetExpectedLifeMonths(ci.CIType);
        if (ageMonths >= expectedLife)
            return RefreshReason.Age;

        return RefreshReason.Age;
    }

    private static decimal EstimateReplacementCost(ConfigurationItem ci)
    {
        return ci.CIType switch
        {
            CIType.Server => 15000m,
            CIType.WorkStation => 1500m,
            CIType.NetworkDevice => 5000m,
            CIType.Storage => 10000m,
            _ => 2500m
        };
    }

    private static List<string> GetReplacementOptions(ConfigurationItem ci)
    {
        return ci.CIType switch
        {
            CIType.Server => new List<string>
            {
                "Physical server replacement",
                "Migrate to virtual infrastructure",
                "Cloud migration (IaaS)"
            },
            CIType.WorkStation => new List<string>
            {
                "Standard replacement",
                "Virtual desktop (VDI)"
            },
            _ => new List<string> { "Standard replacement" }
        };
    }

    private static List<RetirementTask> GetRetirementTasks(ConfigurationItem ci)
    {
        var tasks = new List<RetirementTask>
        {
            new() { TaskName = "Data backup and migration" },
            new() { TaskName = "User notification" },
            new() { TaskName = "Application migration/decommission" },
            new() { TaskName = "Security credential revocation" },
            new() { TaskName = "Physical removal (if applicable)" },
            new() { TaskName = "CMDB update" },
            new() { TaskName = "Asset disposal documentation" }
        };

        if (ci.CIType is CIType.Server or CIType.Storage)
        {
            tasks.Insert(0, new RetirementTask { TaskName = "Service dependency verification" });
            tasks.Insert(1, new RetirementTask { TaskName = "Replacement system validation" });
        }

        return tasks;
    }

    private static UtilizationStatus DetermineUtilizationStatus(ConfigurationItem ci, int recentIncidents)
    {
        if (recentIncidents >= 10)
            return UtilizationStatus.Overloaded;
        if (recentIncidents >= 5)
            return UtilizationStatus.HighUtilization;
        if (recentIncidents == 0 && ci.CreatedAt > DateTime.UtcNow.AddMonths(-6))
            return UtilizationStatus.Unknown;
        if (recentIncidents <= 1)
            return UtilizationStatus.Optimal;

        return UtilizationStatus.Optimal;
    }

    private static decimal EstimateAcquisitionCost(ConfigurationItem ci)
    {
        return ci.CIType switch
        {
            CIType.Server => 12000m,
            CIType.WorkStation => 1200m,
            CIType.NetworkDevice => 4000m,
            CIType.Storage => 8000m,
            _ => 2000m
        };
    }

    private static decimal EstimateMonthlyOperatingCost(ConfigurationItem ci)
    {
        return ci.CIType switch
        {
            CIType.Server => 500m,
            CIType.WorkStation => 50m,
            CIType.NetworkDevice => 100m,
            CIType.Storage => 200m,
            _ => 100m
        };
    }

    private static decimal CalculateRemainingValue(decimal acquisitionCost, int ageMonths)
    {
        // Straight-line depreciation over 60 months
        const int depreciationMonths = 60;
        var depreciation = acquisitionCost / depreciationMonths * ageMonths;
        return Math.Max(0, acquisitionCost - depreciation);
    }
}



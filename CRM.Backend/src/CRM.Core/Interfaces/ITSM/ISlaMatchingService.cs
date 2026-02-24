// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Service for matching SLA policies to service requests based on criteria.
/// Extracted from SLAPolicyAdminService for separation of concerns.
/// TODO-SYS008-020: Create dedicated SlaMatchingService class.
/// </summary>
public interface ISlaMatchingService
{
    /// <summary>
    /// Finds the best matching SLA policy for a service request.
    /// </summary>
    /// <param name="priority">The priority level of the request.</param>
    /// <param name="categoryId">The category ID of the request.</param>
    /// <param name="customerId">Optional customer ID for customer-specific SLAs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching SLA policy, or null if none found.</returns>
    Task<SLAPolicyDto?> FindMatchingPolicyAsync(
        ServicePriority priority,
        int? categoryId = null,
        int? customerId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all applicable SLA policies for given criteria, ranked by specificity.
    /// </summary>
    /// <param name="priority">The priority level.</param>
    /// <param name="categoryId">Optional category ID.</param>
    /// <param name="customerId">Optional customer ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of applicable policies, most specific first.</returns>
    Task<IEnumerable<SLAPolicyDto>> GetApplicablePoliciesAsync(
        ServicePriority? priority = null,
        int? categoryId = null,
        int? customerId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates response and resolution due times based on SLA policy.
    /// </summary>
    /// <param name="policyId">The SLA policy ID.</param>
    /// <param name="createdAt">The creation time of the request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The calculated SLA times.</returns>
    Task<SlaTimesDto> CalculateSlaTimesAsync(
        int policyId,
        DateTime createdAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a service request meets SLA requirements.
    /// </summary>
    /// <param name="serviceRequestId">The service request ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>SLA compliance status.</returns>
    Task<SlaComplianceDto> CheckSlaComplianceAsync(
        int serviceRequestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets SLA statistics for a time period.
    /// </summary>
    /// <param name="startDate">Start of the period.</param>
    /// <param name="endDate">End of the period.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>SLA statistics.</returns>
    Task<SlaStatisticsDto> GetSlaStatisticsAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Calculated SLA due times.
/// </summary>
public class SlaTimesDto
{
    public int PolicyId { get; set; }
    public string PolicyName { get; set; } = string.Empty;
    public DateTime ResponseDueAt { get; set; }
    public DateTime ResolutionDueAt { get; set; }
    public bool UsesBusinessHours { get; set; }
    public int ResponseTimeMinutes { get; set; }
    public int ResolutionTimeMinutes { get; set; }
}

/// <summary>
/// SLA compliance status for a service request.
/// </summary>
public class SlaComplianceDto
{
    public int ServiceRequestId { get; set; }
    public int? PolicyId { get; set; }
    public string? PolicyName { get; set; }
    public bool IsCompliant { get; set; }
    public bool ResponseMet { get; set; }
    public bool ResolutionMet { get; set; }
    public DateTime? ResponseDueAt { get; set; }
    public DateTime? ResolutionDueAt { get; set; }
    public DateTime? ActualResponseAt { get; set; }
    public DateTime? ActualResolutionAt { get; set; }
    public string Status { get; set; } = string.Empty; // OnTrack, AtRisk, Breached
}

/// <summary>
/// SLA statistics for a time period.
/// </summary>
public class SlaStatisticsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalRequests { get; set; }
    public int RequestsWithSla { get; set; }
    public int ResponsesMet { get; set; }
    public int ResponsesBreached { get; set; }
    public int ResolutionsMet { get; set; }
    public int ResolutionsBreached { get; set; }
    public double ResponseComplianceRate { get; set; }
    public double ResolutionComplianceRate { get; set; }
    public double OverallComplianceRate { get; set; }
    public double AverageResponseTimeMinutes { get; set; }
    public double AverageResolutionTimeMinutes { get; set; }
    public Dictionary<string, double> ComplianceByPriority { get; set; } = new();
}

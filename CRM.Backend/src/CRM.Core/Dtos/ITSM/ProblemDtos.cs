// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities.ITSM;

namespace CRM.Core.Dtos.ITSM;

/// <summary>
/// DTO for problem record (ITSM/ITIL - Problem Management)
/// Problem = recurring or underlying issue from multiple incidents
/// </summary>
public class ProblemDto
{
    /// <summary>Unique identifier</summary>
    public int Id { get; set; }

    /// <summary>Problem ID (entity key)</summary>
    public int ProblemId { get; set; }

    /// <summary>Problem number/reference (display format)</summary>
    public string ProblemNumber { get; set; } = string.Empty;

    /// <summary>Problem number (raw)</summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>Problem title/description</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Short description (ITIL field)</summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>Detailed description</summary>
    public string? Description { get; set; }

    /// <summary>Problem state enum</summary>
    public ProblemState State { get; set; } = ProblemState.New;

    /// <summary>Problem status (Active, On Hold, Closed)</summary>
    public string Status { get; set; } = "Active";

    /// <summary>Urgency level (Low, Medium, High, Critical)</summary>
    public string Urgency { get; set; } = "Medium";

    /// <summary>Impact level (Low, Medium, High, Critical)</summary>
    public string Impact { get; set; } = "Low";

    /// <summary>Priority enum</summary>
    public ProblemPriority Priority { get; set; }

    /// <summary>Number of related incidents</summary>
    [Range(0, int.MaxValue)]
    public int RelatedIncidentCount { get; set; }

    /// <summary>Root cause analysis ID (if completed)</summary>
    public int? RCAId { get; set; }

    /// <summary>Known error flag</summary>
    public bool IsKnownError { get; set; }

    /// <summary>Known error flag (ITIL field)</summary>
    public bool KnownError { get; set; }

    /// <summary>Root cause analysis text</summary>
    public string? RootCause { get; set; }

    /// <summary>Workaround text (ITIL field)</summary>
    public string? Workaround { get; set; }

    /// <summary>Solution text</summary>
    public string? Solution { get; set; }

    /// <summary>Problem investigator user ID</summary>
    public int? ProblemInvestigatorId { get; set; }

    /// <summary>Problem investigator user name</summary>
    public string? ProblemInvestigatorName { get; set; }

    /// <summary>Change ID for permanent fix</summary>
    public int? RelatedChangeId { get; set; }

    /// <summary>Change number for display</summary>
    public string? RelatedChangeNumber { get; set; }

    /// <summary>Assigned to user/group ID</summary>
    public int? AssignedToId { get; set; }

    /// <summary>Assigned to user/group name</summary>
    public string? AssignedToName { get; set; }

    /// <summary>Work around available</summary>
    public bool HasWorkaround { get; set; }

    /// <summary>Workaround description</summary>
    [StringLength(1000)]
    public string? WorkaroundDescription { get; set; }

    /// <summary>Target resolution date</summary>
    public DateTime? TargetResolutionDate { get; set; }

    /// <summary>Actual resolution date</summary>
    public DateTime? ResolvedDate { get; set; }

    /// <summary>Actual closure date</summary>
    public DateTime? ClosedDate { get; set; }

    /// <summary>Record creation timestamp</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Record last update timestamp</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Related incidents</summary>
    public List<LinkedIncidentDto>? RelatedIncidents { get; set; }

    /// <summary>Root cause analysis details</summary>
    public ProblemImpactDto? RootCauseAnalysis { get; set; }
}

/// <summary>
/// DTO for creating a new problem
/// </summary>
public class CreateProblemDto
{
    /// <summary>Problem title</summary>
    [StringLength(255, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Short description (ITIL field)</summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>Description</summary>
    [StringLength(2000)]
    public string? Description { get; set; }

    /// <summary>Priority enum</summary>
    public ProblemPriority Priority { get; set; } = ProblemPriority.Medium;

    /// <summary>Urgency level</summary>
    [StringLength(50)]
    public string Urgency { get; set; } = "Medium";

    /// <summary>Impact level</summary>
    [StringLength(50)]
    public string Impact { get; set; } = "Low";

    /// <summary>Category ID</summary>
    public int? CategoryId { get; set; }

    /// <summary>Assigned to user/group</summary>
    [Range(1, int.MaxValue)]
    public int? AssignedToId { get; set; }

    /// <summary>Related incident IDs</summary>
    public List<int> RelatedIncidentIds { get; set; } = new();

    /// <summary>Incident IDs to relate (ITIL field)</summary>
    public List<int>? IncidentIds { get; set; }
}

/// <summary>
/// DTO for updating a problem
/// </summary>
public class UpdateProblemDto
{
    /// <summary>Updated title</summary>
    [StringLength(255, MinimumLength = 5)]
    public string? Title { get; set; }

    /// <summary>Short description (ITIL field)</summary>
    public string? ShortDescription { get; set; }

    /// <summary>Updated description</summary>
    [StringLength(2000)]
    public string? Description { get; set; }

    /// <summary>Updated state enum</summary>
    public ProblemState? State { get; set; }

    /// <summary>Updated status</summary>
    [StringLength(50)]
    public string? Status { get; set; }

    /// <summary>Updated urgency</summary>
    [StringLength(50)]
    public string? Urgency { get; set; }

    /// <summary>Updated impact</summary>
    [StringLength(50)]
    public string? Impact { get; set; }

    /// <summary>Updated assignment</summary>
    [Range(1, int.MaxValue)]
    public int? AssignedToId { get; set; }

    /// <summary>Root cause analysis</summary>
    public string? RootCause { get; set; }

    /// <summary>Workaround text (ITIL field)</summary>
    public string? Workaround { get; set; }

    /// <summary>Workaround details</summary>
    [StringLength(1000)]
    public string? WorkaroundDescription { get; set; }

    /// <summary>Solution text</summary>
    public string? Solution { get; set; }

    /// <summary>Mark as known error</summary>
    public bool? IsKnownError { get; set; }

    /// <summary>Known error flag (ITIL field)</summary>
    public bool? KnownError { get; set; }

    /// <summary>Problem investigator user ID</summary>
    public int? ProblemInvestigatorId { get; set; }

    /// <summary>Closure comments</summary>
    public string? ClosureComments { get; set; }

    /// <summary>Resolution text</summary>
    public string? Resolution { get; set; }

    /// <summary>Related change ID</summary>
    [Range(1, int.MaxValue)]
    public int? RelatedChangeId { get; set; }
}

/// <summary>
/// List DTO for problems (paginated)
/// </summary>
public class ProblemListDto
{
    /// <summary>Record ID</summary>
    public int Id { get; set; }

    /// <summary>Problem number</summary>
    public string ProblemNumber { get; set; } = string.Empty;

    /// <summary>Title</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>State</summary>
    public string State { get; set; } = "New";

    /// <summary>Status</summary>
    public string Status { get; set; } = "Active";

    /// <summary>Urgency</summary>
    public string Urgency { get; set; } = "Medium";

    /// <summary>Impact</summary>
    public string Impact { get; set; } = "Low";

    /// <summary>Priority</summary>
    public int Priority { get; set; }

    /// <summary>Related incident count</summary>
    public int RelatedIncidentCount { get; set; }

    /// <summary>Is known error</summary>
    public bool IsKnownError { get; set; }

    /// <summary>Assigned to name</summary>
    public string? AssignedToName { get; set; }

    /// <summary>Target resolution date</summary>
    public DateTime? TargetResolutionDate { get; set; }

    /// <summary>Creation date</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Linked incident reference in problem DTO
/// </summary>
public class LinkedIncidentDto
{
    /// <summary>Incident ID</summary>
    public int Id { get; set; }

    /// <summary>Incident number</summary>
    public string IncidentNumber { get; set; } = string.Empty;

    /// <summary>Short description</summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>Current state</summary>
    public string State { get; set; } = string.Empty;
}

/// <summary>
/// DTO for root cause analysis
/// Documents the discovered root cause and permanent fix path
/// </summary>
public class ProblemImpactDto
{
    /// <summary>Record ID</summary>
    public int Id { get; set; }

    /// <summary>Problem ID</summary>
    public int ProblemId { get; set; }

    /// <summary>Root cause identified</summary>
    public string? RootCause { get; set; }

    /// <summary>Systems and services affected</summary>
    [StringLength(1000)]
    public string? AffectedServices { get; set; }

    /// <summary>Business process impact</summary>
    [StringLength(1000)]
    public string? BusinessImpact { get; set; }

    /// <summary>Number of users affected (estimated or actual)</summary>
    [Range(0, int.MaxValue)]
    public int? UsersAffected { get; set; }

    /// <summary>Estimated business impact (financial)</summary>
    [Range(0, 999999999.99)]
    public decimal? EstimatedBusinessLoss { get; set; }

    /// <summary>Recommended permanent fix</summary>
    [StringLength(1000)]
    public string? RecommendedFix { get; set; }

    /// <summary>Permanent fix description</summary>
    [StringLength(2000)]
    public string? PermanentFixDescription { get; set; }

    /// <summary>Temporary workaround (if applicable)</summary>
    [StringLength(1000)]
    public string? TemporaryWorkaround { get; set; }

    /// <summary>Internal notes (root cause investigation details)</summary>
    [StringLength(2000)]
    public string? InternalNotes { get; set; }

    /// <summary>Analysis completed date</summary>
    public DateTime? AnalysisCompletedDate { get; set; }

    /// <summary>Analyzed by user ID</summary>
    public int? AnalyzedByUserId { get; set; }

    /// <summary>Analyzed by user name</summary>
    public string? AnalyzedByUserName { get; set; }

    /// <summary>Creation timestamp</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Update timestamp</summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating root cause analysis
/// </summary>
public class CreateProblemImpactDto
{
    /// <summary>Problem ID</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int ProblemId { get; set; }

    /// <summary>Root cause</summary>
    [StringLength(1000)]
    public string? RootCause { get; set; }

    /// <summary>Affected services</summary>
    [StringLength(1000)]
    public string? AffectedServices { get; set; }

    /// <summary>Business impact</summary>
    [StringLength(1000)]
    public string? BusinessImpact { get; set; }

    /// <summary>Recommended fix</summary>
    [StringLength(1000)]
    public string? RecommendedFix { get; set; }

    /// <summary>Permanent fix description</summary>
    [StringLength(2000)]
    public string? PermanentFixDescription { get; set; }
}

/// <summary>
/// RCA DTO (Root Cause Analysis)
/// Alias for ProblemImpactDto for API clarity
/// </summary>
public class RCADto : ProblemImpactDto
{
}

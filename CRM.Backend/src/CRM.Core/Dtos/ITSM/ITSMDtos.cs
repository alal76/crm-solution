// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities.ITSM;
using EscalationType = CRM.Core.Entities.KnowledgeBase.EscalationType;
using SLAMetricType = CRM.Core.Entities.KnowledgeBase.SLAMetricType;
using SLAPriority = CRM.Core.Entities.KnowledgeBase.SLAPriority;

namespace CRM.Core.Dtos.ITSM;

// ============================================================================
// Incident DTOs
// ============================================================================
public class IncidentDto
{
    public int IncidentId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CallerId { get; set; }
    public string? CallerName { get; set; }
    public ContactType ContactType { get; set; }
    public DateTime OpenedAt { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? SubcategoryId { get; set; }
    public string? SubcategoryName { get; set; }
    public IncidentImpact Impact { get; set; }
    public IncidentUrgency Urgency { get; set; }
    public int Priority { get; set; }
    public IncidentState State { get; set; }
    public int? AssignmentGroupId { get; set; }
    public string? AssignmentGroupName { get; set; }
    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public ResolutionCode? ResolutionCode { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public bool SLABreached { get; set; }
    public DateTime? ResponseDueAt { get; set; }
    public DateTime? ResolutionDueAt { get; set; }
    public bool MajorIncident { get; set; }
    public int? ProblemId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateIncidentDto
{
    [Required]
    [StringLength(160)]
    public string ShortDescription { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public int CallerId { get; set; }

    public ContactType ContactType { get; set; } = ContactType.Portal;

    public int? CategoryId { get; set; }
    public int? SubcategoryId { get; set; }
    public int? ConfigurationItemId { get; set; }

    [Required]
    public IncidentImpact Impact { get; set; }

    [Required]
    public IncidentUrgency Urgency { get; set; }
}

public class UpdateIncidentDto
{
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public int? SubcategoryId { get; set; }
    public IncidentImpact? Impact { get; set; }
    public IncidentUrgency? Urgency { get; set; }
    public IncidentState? State { get; set; }
    public int? AssignmentGroupId { get; set; }
    public int? AssignedToId { get; set; }
}

public class ResolveIncidentDto
{
    [Required]
    public ResolutionCode ResolutionCode { get; set; }

    [Required]
    public string ResolutionNotes { get; set; } = string.Empty;
}

// Problem DTOs moved to ProblemDtos.cs (namespace consolidation)

// ============================================================================
// CMDB DTOs
// ============================================================================
public class ConfigurationItemDto
{
    public int CIId { get; set; }
    public string CIName { get; set; } = string.Empty;
    public string CINumber { get; set; } = string.Empty;
    public CIType CIType { get; set; }
    public string? CISubtype { get; set; }
    public OperationalStatus OperationalStatus { get; set; }
    public string? SerialNumber { get; set; }
    public string? IPAddress { get; set; }
    public int? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCIDto
{
    [Required]
    [StringLength(200)]
    public string CIName { get; set; } = string.Empty;

    [Required]
    public CIType CIType { get; set; }

    public string? CISubtype { get; set; }
    public string? Description { get; set; }
    public string? SerialNumber { get; set; }
    public string? IPAddress { get; set; }
    public int? OwnerId { get; set; }
    public OperationalStatus OperationalStatus { get; set; } = OperationalStatus.Operational;
}

// ============================================================================
// Change DTOs
// ============================================================================
public class ChangeDto
{
    public int ChangeId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ChangeType Type { get; set; }
    public ChangeState State { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; }
    public ChangeRisk Risk { get; set; }
    public ChangeImpact Impact { get; set; }
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public string? ImplementationPlan { get; set; }
    public string? BackoutPlan { get; set; }
    public string? TestingPlan { get; set; }
    public string? RiskAssessmentNotes { get; set; }
    public bool? ChangeSuccess { get; set; }
    public int RequestorId { get; set; }
    public string? RequestorName { get; set; }
    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class CreateChangeDto
{
    [Required]
    [StringLength(160)]
    public string ShortDescription { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public ChangeType Type { get; set; }

    [Required]
    public ChangeRisk Risk { get; set; }

    [Required]
    public ChangeImpact Impact { get; set; }

    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public string? ImplementationPlan { get; set; }
    public string? BackoutPlan { get; set; }

    /// <summary>Testing plan for the change (persisted to <c>Change.TestingPlan</c>).</summary>
    public string? TestingPlan { get; set; }

    /// <summary>Impact/risk analysis notes (persisted to <c>Change.RiskAssessmentNotes</c>).</summary>
    public string? RiskAssessmentNotes { get; set; }
}

// ============================================================================
// Knowledge Article DTOs
// ============================================================================
public class KnowledgeArticleDto
{
    public int ArticleId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string ArticleBody { get; set; } = string.Empty;
    public ArticleType ArticleType { get; set; }
    public PublishingState PublishingState { get; set; }
    public int ViewCount { get; set; }
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public DateTime? PublishedDate { get; set; }
    public int AuthorId { get; set; }
    public string? AuthorName { get; set; }
}

public class CreateKnowledgeArticleDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string ArticleBody { get; set; } = string.Empty;

    [Required]
    public ArticleType ArticleType { get; set; }

    public string? ShortDescription { get; set; }
    public int? CategoryId { get; set; }
    public bool IsInternal { get; set; } = true;
}

// ============================================================================
// Service Catalog DTOs
// ============================================================================
public class CatalogItemDto
{
    public int CatalogItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool IsFeatured { get; set; }
    public decimal? Price { get; set; }
    public bool IsActive { get; set; }
    public int RequestCount { get; set; }
}

public class CreateCatalogRequestDto
{
    [Required]
    public int CatalogItemId { get; set; }

    [Required]
    public int RequestedForId { get; set; }

    public Dictionary<string, string>? VariableValues { get; set; }
}

// SLA DTOs moved to SLAPolicyDto.cs (namespace consolidation)

// ============================================================================
// Filter DTOs
// ============================================================================
public class IncidentFilterDto
{
    public string? SearchTerm { get; set; }
    public IncidentState? State { get; set; }
    public int? Priority { get; set; }
    public int? AssignedToId { get; set; }
    public int? AssignmentGroupId { get; set; }
    public bool? SLABreached { get; set; }
    public bool? MajorIncident { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ProblemFilterDto
{
    public string? SearchTerm { get; set; }
    public ProblemState? State { get; set; }
    public ProblemPriority? Priority { get; set; }
    public bool? KnownError { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ChangeFilterDto
{
    public string? SearchTerm { get; set; }
    public ChangeState? State { get; set; }
    public ChangeType? Type { get; set; }
    public ApprovalStatus? ApprovalStatus { get; set; }
    public DateTime? PlannedStartFrom { get; set; }
    public DateTime? PlannedStartTo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// ============================================================================
// Escalation Rule DTOs moved to EscalationRuleDto.cs (namespace consolidation)
// EscalationRuleFilterDto kept here as it's not in the dedicated file.

/// <summary>
/// Filter DTO for escalation rules.
/// </summary>
public class EscalationRuleFilterDto
{
    public int? SLAPolicyId { get; set; }
    public bool? IsActive { get; set; }
    public EscalationType? EscalationType { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// ============================================================================
// Escalation Policy DTOs
// ============================================================================

/// <summary>
/// DTO for escalation policy details.
/// </summary>
public class EscalationPolicyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? Priority { get; set; }
    public List<EscalationLevelDto> Levels { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for escalation level within a policy.
/// </summary>
public class EscalationLevelDto
{
    public int Id { get; set; }
    public int PolicyId { get; set; }
    public int LevelNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public int EscalateAfterMinutes { get; set; }
    public int? NotifyUserId { get; set; }
    public string? NotifyUserName { get; set; }
    public int? NotifyTeamId { get; set; }
    public string? NotifyTeamName { get; set; }
    public bool SendEmail { get; set; }
    public bool SendSms { get; set; }
    public int? EmailTemplateId { get; set; }
}

/// <summary>
/// DTO for creating an escalation policy.
/// </summary>
public class CreateEscalationPolicyDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDefault { get; set; } = false;

    public int? CategoryId { get; set; }

    public int? Priority { get; set; }

    public List<CreateEscalationLevelDto>? Levels { get; set; }
}

/// <summary>
/// DTO for updating an escalation policy.
/// </summary>
public class UpdateEscalationPolicyDto
{
    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDefault { get; set; }

    public int? CategoryId { get; set; }

    public int? Priority { get; set; }
}

/// <summary>
/// DTO for creating an escalation level.
/// </summary>
public class CreateEscalationLevelDto
{
    public int LevelNumber { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int EscalateAfterMinutes { get; set; }

    public int? NotifyUserId { get; set; }

    public int? NotifyTeamId { get; set; }

    public bool SendEmail { get; set; } = true;

    public bool SendSms { get; set; } = false;

    public int? EmailTemplateId { get; set; }
}

/// <summary>
/// DTO for escalation history record.
/// </summary>
public class EscalationHistoryDto
{
    public int Id { get; set; }
    public int PolicyId { get; set; }
    public int ServiceRequestId { get; set; }
    public int EscalationLevel { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public int? NotifyUserId { get; set; }
    public int? NotifyTeamId { get; set; }
    public string Status { get; set; } = "Pending";
}

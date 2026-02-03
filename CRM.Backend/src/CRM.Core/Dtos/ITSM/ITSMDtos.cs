// This file is part of the CRM Solution.
// Copyright (c) 2025 CRM Solution Contributors
// Licensed under the AGPL-3.0 license.

using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities.ITSM;

namespace CRM.Core.DTOs.ITSM;

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

// ============================================================================
// Problem DTOs
// ============================================================================

public class ProblemDto
{
    public int ProblemId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProblemPriority Priority { get; set; }
    public ProblemState State { get; set; }
    public string? RootCause { get; set; }
    public string? Workaround { get; set; }
    public bool KnownError { get; set; }
    public string? Solution { get; set; }
    public int? ProblemInvestigatorId { get; set; }
    public string? ProblemInvestigatorName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int RelatedIncidentCount { get; set; }
}

public class CreateProblemDto
{
    [Required]
    [StringLength(160)]
    public string ShortDescription { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public ProblemPriority Priority { get; set; }
    
    public int? CategoryId { get; set; }
    public List<int>? IncidentIds { get; set; }
}

public class UpdateProblemDto
{
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public ProblemState? State { get; set; }
    public string? RootCause { get; set; }
    public string? Workaround { get; set; }
    public string? Solution { get; set; }
    public bool? KnownError { get; set; }
    public int? ProblemInvestigatorId { get; set; }
}

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
    public ChangeType Type { get; set; }
    public ChangeState State { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; }
    public ChangeRisk Risk { get; set; }
    public ChangeImpact Impact { get; set; }
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public int RequestorId { get; set; }
    public string? RequestorName { get; set; }
    public DateTime CreatedAt { get; set; }
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

// ============================================================================
// SLA DTOs
// ============================================================================

public class SLAPolicyDto
{
    public int SLAPolicyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public SLATargetType TargetType { get; set; }
    public int? P1ResponseMinutes { get; set; }
    public int? P1ResolutionMinutes { get; set; }
    public bool UseBusinessHours { get; set; }
    public bool IsActive { get; set; }
}

public class SLAInstanceDto
{
    public int SLAInstanceId { get; set; }
    public int TargetId { get; set; }
    public SLATargetType TargetType { get; set; }
    public DateTime? ResponseDueAt { get; set; }
    public DateTime? ResolutionDueAt { get; set; }
    public bool ResponseBreached { get; set; }
    public bool ResolutionBreached { get; set; }
    public SLAState State { get; set; }
    public int? MinutesUntilResponseBreach { get; set; }
    public int? MinutesUntilResolutionBreach { get; set; }
}

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

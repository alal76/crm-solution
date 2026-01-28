// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for relationship type
/// </summary>
public class RelationshipTypeDto
{
    public int Id { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string? TypeCategory { get; set; }
    public string? Description { get; set; }
    public bool IsBidirectional { get; set; }
    public string? ReverseTypeName { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystem { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating/updating relationship type
/// </summary>
public class RelationshipTypeCreateDto
{
    public string TypeName { get; set; } = string.Empty;
    public string? TypeCategory { get; set; }
    public string? Description { get; set; }
    public bool IsBidirectional { get; set; }
    public string? ReverseTypeName { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}

/// <summary>
/// DTO for account relationship
/// </summary>
public class AccountRelationshipDto
{
    public int Id { get; set; }
    public int SourceCustomerId { get; set; }
    public int TargetCustomerId { get; set; }
    public int RelationshipTypeId { get; set; }
    
    // Expanded data
    public string? SourceCustomerName { get; set; }
    public string? TargetCustomerName { get; set; }
    public string? RelationshipTypeName { get; set; }
    public string? RelationshipTypeCategory { get; set; }
    public string? RelationshipTypeColor { get; set; }
    public string? RelationshipTypeIcon { get; set; }
    
    public string Status { get; set; } = "Active";
    public int StrengthScore { get; set; }
    public string StrategicImportance { get; set; } = "Medium";
    
    public DateTime? RelationshipStartDate { get; set; }
    public DateTime? RelationshipEndDate { get; set; }
    public DateTime? LastReviewedDate { get; set; }
    public DateTime? NextReviewDate { get; set; }
    
    public decimal? AnnualRevenueImpact { get; set; }
    public decimal? CostSavings { get; set; }
    
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? TermsConditions { get; set; }
    
    public int InteractionCount { get; set; }
    public DateTime? LastInteractionDate { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
}

/// <summary>
/// DTO for creating/updating account relationship
/// </summary>
public class AccountRelationshipCreateDto
{
    public int SourceCustomerId { get; set; }
    public int TargetCustomerId { get; set; }
    public int RelationshipTypeId { get; set; }
    
    public string Status { get; set; } = "Active";
    public int StrengthScore { get; set; } = 50;
    public string StrategicImportance { get; set; } = "Medium";
    
    public DateTime? RelationshipStartDate { get; set; }
    public DateTime? RelationshipEndDate { get; set; }
    public DateTime? NextReviewDate { get; set; }
    
    public decimal? AnnualRevenueImpact { get; set; }
    public decimal? CostSavings { get; set; }
    
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? TermsConditions { get; set; }
}

/// <summary>
/// DTO for relationship interaction
/// </summary>
public class RelationshipInteractionDto
{
    public int Id { get; set; }
    public int AccountRelationshipId { get; set; }
    public string InteractionType { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Description { get; set; }
    public DateTime InteractionDate { get; set; }
    public int? DurationMinutes { get; set; }
    
    public List<int>? ParticipantContactIds { get; set; }
    public List<int>? ParticipantUserIds { get; set; }
    public List<string>? ParticipantNames { get; set; }
    
    public string? Outcome { get; set; }
    public string? ActionItems { get; set; }
    public string? NextSteps { get; set; }
    public DateTime? FollowUpDate { get; set; }
    
    public int SentimentScore { get; set; }
    public string HealthImpact { get; set; } = "Neutral";
    
    public string? Location { get; set; }
    public string? MeetingLink { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
}

/// <summary>
/// DTO for creating/updating relationship interaction
/// </summary>
public class RelationshipInteractionCreateDto
{
    public int AccountRelationshipId { get; set; }
    public string InteractionType { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Description { get; set; }
    public DateTime InteractionDate { get; set; }
    public int? DurationMinutes { get; set; }
    
    public List<int>? ParticipantContactIds { get; set; }
    public List<int>? ParticipantUserIds { get; set; }
    
    public string? Outcome { get; set; }
    public string? ActionItems { get; set; }
    public string? NextSteps { get; set; }
    public DateTime? FollowUpDate { get; set; }
    
    public int SentimentScore { get; set; }
    public string HealthImpact { get; set; } = "Neutral";
    
    public string? Location { get; set; }
    public string? MeetingLink { get; set; }
}

/// <summary>
/// DTO for account health snapshot
/// </summary>
public class AccountHealthSnapshotDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime SnapshotDate { get; set; }
    
    public int OverallHealthScore { get; set; }
    public int EngagementScore { get; set; }
    public int ProductAdoptionScore { get; set; }
    public int SupportSatisfactionScore { get; set; }
    public int FinancialHealthScore { get; set; }
    public int RelationshipScore { get; set; }
    
    public int? ActiveUsersCount { get; set; }
    public decimal? FeatureAdoptionRate { get; set; }
    public int? SupportTicketsCount { get; set; }
    public int? SupportTicketsResolved { get; set; }
    public decimal? AverageResponseTimeHours { get; set; }
    public int? NPSScore { get; set; }
    
    public List<string>? RiskFactors { get; set; }
    public List<string>? WarningSignals { get; set; }
    public List<string>? GrowthIndicators { get; set; }
    
    public string? AnalystNotes { get; set; }
    public int? PreviousHealthScore { get; set; }
    public string HealthTrend { get; set; } = "Stable";
    
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating health snapshot
/// </summary>
public class AccountHealthSnapshotCreateDto
{
    public int CustomerId { get; set; }
    public DateTime? SnapshotDate { get; set; }
    
    public int OverallHealthScore { get; set; }
    public int EngagementScore { get; set; }
    public int ProductAdoptionScore { get; set; }
    public int SupportSatisfactionScore { get; set; }
    public int FinancialHealthScore { get; set; }
    public int RelationshipScore { get; set; }
    
    public int? ActiveUsersCount { get; set; }
    public decimal? FeatureAdoptionRate { get; set; }
    public int? SupportTicketsCount { get; set; }
    public int? SupportTicketsResolved { get; set; }
    public decimal? AverageResponseTimeHours { get; set; }
    public int? NPSScore { get; set; }
    
    public List<string>? RiskFactors { get; set; }
    public List<string>? WarningSignals { get; set; }
    public List<string>? GrowthIndicators { get; set; }
    
    public string? AnalystNotes { get; set; }
}

/// <summary>
/// DTO for relationship map
/// </summary>
public class RelationshipMapDto
{
    public int Id { get; set; }
    public string MapName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CentralCustomerId { get; set; }
    public string? CentralCustomerName { get; set; }
    public int RelationshipDepth { get; set; }
    
    public List<int>? IncludeRelationshipTypeIds { get; set; }
    public List<int>? ExcludeRelationshipTypeIds { get; set; }
    public int MinRelationshipStrength { get; set; }
    public List<string>? IncludeStatuses { get; set; }
    
    public DateTime? DateRangeStart { get; set; }
    public DateTime? DateRangeEnd { get; set; }
    
    public bool IsPublic { get; set; }
    public List<int>? SharedWithUserIds { get; set; }
    public List<int>? SharedWithGroupIds { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
}

/// <summary>
/// DTO for relationship map visualization data
/// </summary>
public class RelationshipMapVisualizationDto
{
    public List<RelationshipNodeDto> Nodes { get; set; } = new();
    public List<RelationshipEdgeDto> Edges { get; set; } = new();
}

/// <summary>
/// Node in relationship map
/// </summary>
public class RelationshipNodeDto
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Industry { get; set; }
    public int? HealthScore { get; set; }
    public string? RiskLevel { get; set; }
    public decimal? LifetimeValue { get; set; }
    public int RelationshipCount { get; set; }
    public bool IsCentral { get; set; }
}

/// <summary>
/// Edge in relationship map
/// </summary>
public class RelationshipEdgeDto
{
    public int Id { get; set; }
    public int SourceId { get; set; }
    public int TargetId { get; set; }
    public string RelationshipType { get; set; } = string.Empty;
    public string? Color { get; set; }
    public int StrengthScore { get; set; }
    public string Status { get; set; } = "Active";
}

/// <summary>
/// DTO for territory
/// </summary>
public class AccountTerritoryDto
{
    public int Id { get; set; }
    public string TerritoryName { get; set; } = string.Empty;
    public string? TerritoryCode { get; set; }
    public string? Description { get; set; }
    
    public List<string>? Countries { get; set; }
    public List<string>? Regions { get; set; }
    public List<string>? States { get; set; }
    public List<string>? Cities { get; set; }
    public List<string>? Industries { get; set; }
    public List<string>? CustomerTypes { get; set; }
    
    public decimal? RevenueRangeMin { get; set; }
    public decimal? RevenueRangeMax { get; set; }
    
    public int? PrimaryOwnerId { get; set; }
    public string? PrimaryOwnerName { get; set; }
    public List<int>? TeamMemberIds { get; set; }
    
    public decimal? AnnualQuota { get; set; }
    public string QuotaCurrency { get; set; } = "USD";
    public int? TargetAccountCount { get; set; }
    public int AssignedAccountCount { get; set; }
    
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

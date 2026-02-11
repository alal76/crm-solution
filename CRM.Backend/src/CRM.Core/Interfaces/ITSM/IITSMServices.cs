// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;

namespace CRM.Core.Interfaces.ITSM;

public interface IIncidentService
{
    Task<IncidentDto> CreateIncidentAsync(CreateIncidentDto dto, int createdById);
    Task<IncidentDto?> GetIncidentByIdAsync(int incidentId);
    Task<(IEnumerable<IncidentDto> Items, int TotalCount)> GetIncidentsAsync(IncidentFilterDto filter);
    Task<IncidentDto> UpdateIncidentAsync(int incidentId, UpdateIncidentDto dto, int modifiedById);
    Task<bool> AssignIncidentAsync(int incidentId, int? assignedToId, int? assignmentGroupId, int modifiedById);
    Task<bool> EscalateIncidentAsync(int incidentId, int modifiedById);
    Task<IncidentDto> ResolveIncidentAsync(int incidentId, ResolveIncidentDto dto, int resolvedById);
    Task<bool> CloseIncidentAsync(int incidentId, int closedById);
    Task<bool> ReopenIncidentAsync(int incidentId, int modifiedById);
    Task<bool> AddCommentAsync(int incidentId, string comment, bool isInternal, int createdById);
    Task<IEnumerable<IncidentComment>> GetCommentsAsync(int incidentId);
}

public interface IProblemService
{
    Task<ProblemDto> CreateProblemAsync(CreateProblemDto dto, int createdById);
    Task<ProblemDto?> GetProblemByIdAsync(int problemId);
    Task<(IEnumerable<ProblemDto> Items, int TotalCount)> GetProblemsAsync(ProblemFilterDto filter);
    Task<ProblemDto> UpdateProblemAsync(int problemId, UpdateProblemDto dto, int modifiedById);
    Task<bool> LinkIncidentAsync(int problemId, int incidentId, int createdById);
    Task<bool> MarkAsKnownErrorAsync(int problemId, int modifiedById);
    Task<IEnumerable<IncidentDto>> GetRelatedIncidentsAsync(int problemId);
    Task<bool> UpdateRootCauseAnalysisAsync(int problemId, string rootCause, string? workaround, int modifiedById);
}

public interface ICMDBService
{
    Task<ConfigurationItemDto> CreateCIAsync(CreateCIDto dto, int createdById);
    Task<ConfigurationItemDto?> GetCIByIdAsync(int ciId);
    Task<IEnumerable<ConfigurationItemDto>> SearchCIsAsync(string searchTerm, CIType? type, int pageNumber, int pageSize);
    Task<ConfigurationItemDto> UpdateCIAsync(int ciId, CreateCIDto dto, int modifiedById);
    Task<bool> CreateRelationshipAsync(int parentCIId, int childCIId, RelationshipType type, int createdById);
    Task<IEnumerable<ConfigurationItemDto>> GetRelatedCIsAsync(int ciId);
    Task<IEnumerable<string>> GetImpactAnalysisAsync(int ciId);
}

public interface IChangeManagementService
{
    Task<ChangeDto> CreateChangeAsync(CreateChangeDto dto, int requestorId);
    Task<ChangeDto?> GetChangeByIdAsync(int changeId);
    Task<(IEnumerable<ChangeDto> Items, int TotalCount)> GetChangesAsync(ChangeFilterDto filter);
    Task<ChangeDto> UpdateChangeAsync(int changeId, CreateChangeDto dto, int modifiedById);
    Task<bool> SubmitForApprovalAsync(int changeId, int modifiedById);
    Task<bool> ApproveChangeAsync(int changeId, int approverId, string? comments);
    Task<bool> RejectChangeAsync(int changeId, int approverId, string? comments);
    Task<bool> ScheduleChangeAsync(int changeId, DateTime plannedStart, DateTime plannedEnd, int modifiedById);
    Task<bool> CheckConflictsAsync(int changeId);
    Task<bool> AddImpactedCIAsync(int changeId, int ciId, int createdById);
    Task<IEnumerable<ConfigurationItemDto>> GetImpactedCIsAsync(int changeId);
    Task<IEnumerable<BlackoutPeriodInfo>> GetBlackoutPeriodsAsync(DateTime startDate, DateTime endDate);
    Task<BlackoutPeriodInfo> CreateBlackoutPeriodAsync(CreateBlackoutPeriodInfo dto, int createdById);
}

public class BlackoutPeriodInfo
{
    public int BlackoutPeriodId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateBlackoutPeriodInfo
{
    public string Name { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public interface IKnowledgeManagementService
{
    Task<KnowledgeArticleDto> CreateArticleAsync(CreateKnowledgeArticleDto dto, int authorId);
    Task<KnowledgeArticleDto?> GetArticleByIdAsync(int articleId);
    Task<IEnumerable<KnowledgeArticleDto>> SearchArticlesAsync(string searchTerm, int pageNumber, int pageSize);
    Task<KnowledgeArticleDto> UpdateArticleAsync(int articleId, CreateKnowledgeArticleDto dto, int modifiedById);
    Task<bool> PublishArticleAsync(int articleId, int publishedById);
    Task<bool> RetireArticleAsync(int articleId, int modifiedById);
    Task<bool> SubmitFeedbackAsync(int articleId, int? userId, bool isHelpful, string? comment);
    Task<IEnumerable<KnowledgeArticleDto>> GetSuggestedArticlesAsync(string incidentDescription);
    Task<IEnumerable<KnowledgeArticleDto>> GetPopularArticlesAsync(int count);
    Task<IEnumerable<KnowledgeArticleDto>> GetRecentArticlesAsync(int count);
    Task<IEnumerable<string>> GetCategoriesAsync();
}

public interface IServiceCatalogService
{
    Task<IEnumerable<CatalogItemDto>> GetCatalogItemsAsync(int? categoryId, bool? featuredOnly);
    Task<CatalogItemDto?> GetCatalogItemByIdAsync(int catalogItemId);
    Task<int> CreateCatalogRequestAsync(CreateCatalogRequestDto dto, int requestedById);
    Task<IEnumerable<CatalogRequest>> GetMyRequestsAsync(int userId);
    Task<IEnumerable<CatalogItemDto>> SearchCatalogAsync(string searchTerm);
    Task<IEnumerable<CatalogCategoryInfo>> GetCategoriesAsync();
    Task<int> CreateCatalogRequestForOthersAsync(CreateCatalogRequestForOthersDto dto, int requestedById);
    Task<CatalogRequest?> GetRequestByIdAsync(int requestId);
    Task<bool> CancelRequestAsync(int requestId, int userId);
}

public class CatalogCategoryInfo
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int ItemCount { get; set; }
}

public class CreateCatalogRequestForOthersDto
{
    public int CatalogItemId { get; set; }
    public int RequestedForUserId { get; set; }
    public string? Notes { get; set; }
    public Dictionary<string, string>? FormData { get; set; }
}

public interface ISLAService
{
    Task<SLAPolicyDto> CreateSLAPolicyAsync(SLAPolicyDto dto, int createdById);
    Task<IEnumerable<SLAPolicyDto>> GetSLAPoliciesAsync(SLATargetType? targetType);
    Task<SLAInstanceDto?> GetSLAInstanceAsync(int targetId, SLATargetType targetType);
    Task StartSLAAsync(int targetId, SLATargetType targetType, int priority);
    Task PauseSLAAsync(int targetId, SLATargetType targetType, string reason);
    Task ResumeSLAAsync(int targetId, SLATargetType targetType);
    Task CompleteSLAAsync(int targetId, SLATargetType targetType, bool responseComplete, bool resolutionComplete);
    Task<IEnumerable<SLAInstanceDto>> GetBreachedSLAsAsync();
    Task CheckSLABreachesAsync();
    Task<SLADashboardInfo> GetSLADashboardAsync();
    Task<IEnumerable<SLAInstanceDto>> GetAtRiskSLAsAsync(int thresholdMinutes);
    Task<SLAMetricsInfo> GetSLAMetricsAsync(DateTime startDate, DateTime endDate);
}

public class SLADashboardInfo
{
    public int TotalActiveSLAs { get; set; }
    public int BreachedCount { get; set; }
    public int AtRiskCount { get; set; }
    public int OnTrackCount { get; set; }
    public double OverallComplianceRate { get; set; }
    public IEnumerable<SLAInstanceDto> RecentBreaches { get; set; } = new List<SLAInstanceDto>();
    public IEnumerable<SLAInstanceDto> AtRiskItems { get; set; } = new List<SLAInstanceDto>();
}

public class SLAMetricsInfo
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalIncidents { get; set; }
    public int TotalBreaches { get; set; }
    public double ResponseComplianceRate { get; set; }
    public double ResolutionComplianceRate { get; set; }
    public double AverageResponseTimeMinutes { get; set; }
    public double AverageResolutionTimeMinutes { get; set; }
    public Dictionary<int, double> ComplianceByPriority { get; set; } = new();
}

// ============================================================================
// Escalation Rule Service
// ============================================================================

/// <summary>
/// Service for managing escalation rules within SLA policies.
/// </summary>
public interface IEscalationRuleService
{
    /// <summary>Get all escalation rules with optional filtering.</summary>
    Task<(IEnumerable<EscalationRuleDto> Items, int TotalCount)> GetRulesAsync(EscalationRuleFilterDto filter);

    /// <summary>Get an escalation rule by ID.</summary>
    Task<EscalationRuleDto?> GetRuleByIdAsync(int id);

    /// <summary>Get all rules for a specific SLA policy.</summary>
    Task<IEnumerable<EscalationRuleDto>> GetRulesBySLAPolicyAsync(int slaPolicyId);

    /// <summary>Create a new escalation rule.</summary>
    Task<EscalationRuleDto> CreateRuleAsync(CreateEscalationRuleDto dto, int createdById);

    /// <summary>Update an existing escalation rule.</summary>
    Task<EscalationRuleDto> UpdateRuleAsync(int id, UpdateEscalationRuleDto dto, int modifiedById);

    /// <summary>Delete an escalation rule.</summary>
    Task<bool> DeleteRuleAsync(int id);

    /// <summary>Enable an escalation rule.</summary>
    Task<EscalationRuleDto> EnableRuleAsync(int id, int modifiedById);

    /// <summary>Disable an escalation rule.</summary>
    Task<EscalationRuleDto> DisableRuleAsync(int id, int modifiedById);

    /// <summary>Get applicable rules for a service request.</summary>
    Task<IEnumerable<EscalationRuleDto>> GetApplicableRulesAsync(int serviceRequestId);

    /// <summary>Evaluate and execute applicable escalation rules.</summary>
    Task<bool> EvaluateRulesAsync(int serviceRequestId);

    /// <summary>Reorder escalation rules.</summary>
    Task<bool> ReorderRulesAsync(int slaPolicyId, IEnumerable<int> ruleIds);
}

// ============================================================================
// Escalation Policy Service
// ============================================================================

/// <summary>
/// Service for managing escalation policies.
/// </summary>
public interface IEscalationPolicyService
{
    /// <summary>Get all escalation policies.</summary>
    Task<IEnumerable<EscalationPolicyDto>> GetPoliciesAsync(bool? isActive = null);

    /// <summary>Get an escalation policy by ID.</summary>
    Task<EscalationPolicyDto?> GetPolicyByIdAsync(int id);

    /// <summary>Create a new escalation policy.</summary>
    Task<EscalationPolicyDto> CreatePolicyAsync(CreateEscalationPolicyDto dto, int createdById);

    /// <summary>Update an existing escalation policy.</summary>
    Task<EscalationPolicyDto> UpdatePolicyAsync(int id, UpdateEscalationPolicyDto dto, int modifiedById);

    /// <summary>Delete an escalation policy.</summary>
    Task<bool> DeletePolicyAsync(int id);

    /// <summary>Get all levels for a policy.</summary>
    Task<IEnumerable<EscalationLevelDto>> GetPolicyLevelsAsync(int policyId);

    /// <summary>Add a level to a policy.</summary>
    Task<EscalationLevelDto> AddLevelAsync(int policyId, CreateEscalationLevelDto dto, int createdById);

    /// <summary>Update a level.</summary>
    Task<EscalationLevelDto> UpdateLevelAsync(int levelId, CreateEscalationLevelDto dto, int modifiedById);

    /// <summary>Delete a level.</summary>
    Task<bool> DeleteLevelAsync(int levelId);

    /// <summary>Assign a policy to a service request.</summary>
    Task<bool> AssignPolicyToRequestAsync(int serviceRequestId, int policyId, int assignedById);

    /// <summary>Get the default policy for a category/priority combination.</summary>
    Task<EscalationPolicyDto?> GetDefaultPolicyAsync(int? categoryId, int? priority);

    /// <summary>Set a policy as the default for a category/priority.</summary>
    Task<bool> SetAsDefaultAsync(int policyId, int? categoryId, int? priority);
}

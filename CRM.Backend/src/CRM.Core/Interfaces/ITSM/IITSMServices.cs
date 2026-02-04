// This file is part of the CRM Solution.
// Copyright (c) 2025 CRM Solution Contributors
// Licensed under the AGPL-3.0 license.

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

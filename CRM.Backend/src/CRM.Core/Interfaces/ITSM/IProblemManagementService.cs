// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Service for managing problems in IT Service Management.
/// Handles problem lifecycle: creation, investigation, root cause analysis, resolution, and closure.
/// Includes tracking of related incidents and knowledge article creation.
/// </summary>
public interface IProblemManagementService
{
    // CRUD Operations
    /// <summary>Creates a new problem.</summary>
    Task<ProblemDto> CreateProblemAsync(CreateProblemDto dto, int createdById, CancellationToken cancellationToken = default);

    /// <summary>Gets a problem by ID.</summary>
    Task<ProblemDto> GetProblemByIdAsync(int problemId, CancellationToken cancellationToken = default);

    /// <summary>Gets all problems with filtering and pagination.</summary>
    Task<(IEnumerable<ProblemDto> Items, int TotalCount)> ListProblemsAsync(
        ProblemFilterDto filter, CancellationToken cancellationToken = default);

    /// <summary>Updates problem details.</summary>
    Task<ProblemDto> UpdateProblemAsync(int problemId, UpdateProblemDto dto, int modifiedById, CancellationToken cancellationToken = default);

    /// <summary>Deletes (soft delete) a problem.</summary>
    Task DeleteProblemAsync(int problemId, CancellationToken cancellationToken = default);

    // Workflow Operations
    /// <summary>Relates a problem to one or more incidents.</summary>
    Task RelateProblemToIncidentsAsync(int problemId, List<int> incidentIds, int createdById, CancellationToken cancellationToken = default);

    /// <summary>Removes a related incident from a problem.</summary>
    Task UnrelateProblemFromIncidentAsync(int problemId, int incidentId, CancellationToken cancellationToken = default);

    /// <summary>Gets all incidents related to a problem.</summary>
    Task<IEnumerable<IncidentDto>> GetRelatedIncidentsAsync(int problemId, CancellationToken cancellationToken = default);

    /// <summary>Marks a problem as a known error.</summary>
    Task MarkAsKnownErrorAsync(int problemId, int modifiedById, CancellationToken cancellationToken = default);

    /// <summary>Resolves a problem with root cause analysis and solution.</summary>
    Task<ProblemDto> ResolveProblemAsync(int problemId, ResolveProblemDto dto, int resolvedById, CancellationToken cancellationToken = default);

    /// <summary>Closes a resolved problem optionally creating a knowledge article.</summary>
    Task<ProblemDto> CloseProblemAsync(int problemId, CloseProblemDto dto, int closedById, CancellationToken cancellationToken = default);

    /// <summary>Reopens a closed or resolved problem.</summary>
    Task<ProblemDto> ReopenProblemAsync(int problemId, ReopenProblemDto dto, int reopenedById, CancellationToken cancellationToken = default);

    // Analysis Operations
    /// <summary>Performs root cause analysis on incidents to identify a pattern.</summary>
    Task<ProblemRootCauseAnalysisDto> AnalyzeIncidentsAsync(
        List<int> incidentIds, int createdById, CancellationToken cancellationToken = default);

    /// <summary>Determines the root cause of a problem.</summary>
    Task<ProblemDto> DetermineCauseAsync(
        int problemId, string rootCause, string? workaround, int modifiedById, CancellationToken cancellationToken = default);

    /// <summary>Identifies a temporary workaround while a permanent solution is being developed.</summary>
    Task<ProblemDto> IdentifyTemporaryWorkaroundAsync(
        int problemId, string workaround, int modifiedById, CancellationToken cancellationToken = default);

    /// <summary>Documents the problem with detailed RCA analysis.</summary>
    Task<ProblemRootCauseAnalysisDto> DocumentProblemAsync(
        int problemId, ProblemRootCauseAnalysisDto analysisDto, int modifiedById, CancellationToken cancellationToken = default);

    // Resolution Tracking
    /// <summary>Tracks the resolution progress and metrics.</summary>
    Task<ProblemMetricsDto> GetProblemMetricsAsync(
        DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a knowledge article from a resolved problem.</summary>
    Task<KnowledgeArticleDto> CreateKnowledgeArticleFromProblemAsync(
        int problemId, int authorId, CancellationToken cancellationToken = default);

    // Task Management
    /// <summary>Creates a subtask for problem investigation.</summary>
    Task<ProblemTaskDto> CreateTaskAsync(
        int problemId, CreateProblemTaskDto dto, int createdById, CancellationToken cancellationToken = default);

    /// <summary>Gets all tasks for a problem.</summary>
    Task<IEnumerable<ProblemTaskDto>> GetTasksAsync(int problemId, CancellationToken cancellationToken = default);

    /// <summary>Completes a problem task.</summary>
    Task<ProblemTaskDto> CompleteTaskAsync(int taskId, int completedById, CancellationToken cancellationToken = default);

    // Comments
    /// <summary>Adds a comment to a problem.</summary>
    Task<ProblemCommentDto> AddCommentAsync(
        int problemId, CreateProblemCommentDto dto, int createdById, CancellationToken cancellationToken = default);

    /// <summary>Gets all comments for a problem.</summary>
    Task<IEnumerable<ProblemCommentDto>> GetCommentsAsync(int problemId, CancellationToken cancellationToken = default);

    // Assignment
    /// <summary>Assigns a problem to an investigator and/or manager.</summary>
    Task<ProblemDto> AssignProblemAsync(
        int problemId, AssignProblemDto dto, int modifiedById, CancellationToken cancellationToken = default);
}

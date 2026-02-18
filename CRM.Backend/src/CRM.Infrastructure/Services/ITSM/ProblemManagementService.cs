// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Exceptions;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Service for managing problems in IT Service Management.
/// Implements the complete problem lifecycle from creation through closure,
/// including root cause analysis, incident relationship tracking, and knowledge article generation.
/// </summary>
public class ProblemManagementService : IProblemManagementService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<ProblemManagementService> _logger;

    public ProblemManagementService(
        ICrmDbContext dbContext,
        ILogger<ProblemManagementService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>Creates a new problem.</summary>
    public async Task<ProblemDto> CreateProblemAsync(CreateProblemDto dto, int createdById, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new problem: {ShortDescription}", dto.ShortDescription);

        if (string.IsNullOrWhiteSpace(dto.ShortDescription))
            throw new ValidationException(nameof(dto.ShortDescription), "Short description is required");

        var problem = new Problem
        {
            Number = await GenerateProblemNumberAsync(cancellationToken),
            ShortDescription = dto.ShortDescription,
            Description = dto.Description,
            Priority = dto.Priority,
            CategoryId = dto.CategoryId,
            State = ProblemState.New,
            CreatedAt = DateTime.UtcNow,
            CreatedById = createdById
        };

        // Relate to incidents if provided
        if (dto.IncidentIds?.Any() == true)
        {
            var incidents = await _dbContext.Incidents
                .Where(i => dto.IncidentIds.Contains(i.IncidentId) && !i.IsDeleted)
                .ToListAsync(cancellationToken);

            if (!incidents.Any())
                throw new ValidationException("incidents", "No valid incidents found for the provided IDs");

            problem.ProblemIncidents = new List<ProblemIncident>();
            foreach (var incident in incidents)
            {
                problem.ProblemIncidents.Add(new ProblemIncident
                {
                    IncidentId = incident.IncidentId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = createdById
                });
            }
        }

        _dbContext.Problems.Add(problem);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Problem created successfully: {ProblemNumber}", problem.Number);

        return await MapToDto(problem, cancellationToken);
    }

    /// <summary>Gets a problem by ID.</summary>
    public async Task<ProblemDto> GetProblemByIdAsync(int problemId, CancellationToken cancellationToken = default)
    {
        var problem = await _dbContext.Problems
            .Include(p => p.Category)
            .Include(p => p.ProblemInvestigator)
            .Include(p => p.ProblemManager)
            .Include(p => p.AssignmentGroup)
            .Include(p => p.ProblemIncidents!)
                .ThenInclude(pi => pi.Incident)
            .FirstOrDefaultAsync(p => p.ProblemId == problemId && !p.IsDeleted, cancellationToken);

        if (problem == null)
            throw new EntityNotFoundException(nameof(Problem), problemId);

        return await MapToDto(problem, cancellationToken);
    }

    /// <summary>Gets all problems with filtering and pagination.</summary>
    public async Task<(IEnumerable<ProblemDto> Items, int TotalCount)> ListProblemsAsync(
        ProblemFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Problems
            .Include(p => p.Category)
            .Include(p => p.ProblemInvestigator)
            .Include(p => p.ProblemIncidents)
            .Where(p => !p.IsDeleted);

        // Apply filters
        if (!string.IsNullOrEmpty(filter.SearchTerm))
            query = query.Where(p => p.Number.Contains(filter.SearchTerm) || p.ShortDescription.Contains(filter.SearchTerm));

        if (filter.State.HasValue)
            query = query.Where(p => p.State == filter.State.Value);

        if (filter.Priority.HasValue)
            query = query.Where(p => p.Priority == filter.Priority.Value);

        if (filter.KnownError.HasValue)
            query = query.Where(p => p.KnownError == filter.KnownError.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var problems = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = new List<ProblemDto>();
        foreach (var problem in problems)
        {
            dtos.Add(await MapToDto(problem, cancellationToken));
        }

        return (dtos, totalCount);
    }

    /// <summary>Updates problem details.</summary>
    public async Task<ProblemDto> UpdateProblemAsync(int problemId, UpdateProblemDto dto, int modifiedById, CancellationToken cancellationToken = default)
    {
        var problem = await _dbContext.Problems.FindAsync(new object[] { problemId }, cancellationToken: cancellationToken);

        if (problem == null || problem.IsDeleted)
            throw new EntityNotFoundException(nameof(Problem), problemId);

        if (!string.IsNullOrEmpty(dto.ShortDescription))
            problem.ShortDescription = dto.ShortDescription;

        if (dto.Description != null)
            problem.Description = dto.Description;

        if (dto.State.HasValue)
            problem.State = dto.State.Value;

        if (!string.IsNullOrEmpty(dto.RootCause))
            problem.RootCause = dto.RootCause;

        if (!string.IsNullOrEmpty(dto.Workaround))
            problem.Workaround = dto.Workaround;

        if (!string.IsNullOrEmpty(dto.Solution))
            problem.Solution = dto.Solution;

        if (dto.KnownError.HasValue)
            problem.KnownError = dto.KnownError.Value;

        if (dto.ProblemInvestigatorId.HasValue)
            problem.ProblemInvestigatorId = dto.ProblemInvestigatorId.Value;

        problem.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Problem {ProblemNumber} updated", problem.Number);

        return await MapToDto(problem, cancellationToken);
    }

    /// <summary>Deletes (soft delete) a problem.</summary>
    public async Task DeleteProblemAsync(int problemId, CancellationToken cancellationToken = default)
    {
        var problem = await _dbContext.Problems.FindAsync(new object[] { problemId }, cancellationToken: cancellationToken);

        if (problem == null || problem.IsDeleted)
            throw new EntityNotFoundException(nameof(Problem), problemId);

        problem.IsDeleted = true;
        problem.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Problem {ProblemNumber} deleted", problem.Number);
    }

    /// <summary>Relates a problem to one or more incidents.</summary>
    public async Task RelateProblemToIncidentsAsync(int problemId, List<int> incidentIds, int createdById, CancellationToken cancellationToken = default)
    {
        var problem = await _dbContext.Problems.FindAsync(new object[] { problemId }, cancellationToken: cancellationToken);

        if (problem == null || problem.IsDeleted)
            throw new EntityNotFoundException(nameof(Problem), problemId);

        var incidents = await _dbContext.Incidents
            .Where(i => incidentIds.Contains(i.IncidentId) && !i.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!incidents.Any())
            throw new ValidationException("incidents", "No valid incidents found");

        foreach (var incident in incidents)
        {
            var existingRelation = await _dbContext.ProblemIncidents
                .FirstOrDefaultAsync(pi => pi.ProblemId == problemId && pi.IncidentId == incident.IncidentId, cancellationToken);

            if (existingRelation == null)
            {
                _dbContext.ProblemIncidents.Add(new ProblemIncident
                {
                    ProblemId = problemId,
                    IncidentId = incident.IncidentId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = createdById
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Related {Count} incidents to problem {ProblemId}", incidents.Count, problemId);
    }

    /// <summary>Removes a related incident from a problem.</summary>
    public async Task UnrelateProblemFromIncidentAsync(int problemId, int incidentId, CancellationToken cancellationToken = default)
    {
        var relation = await _dbContext.ProblemIncidents
            .FirstOrDefaultAsync(pi => pi.ProblemId == problemId && pi.IncidentId == incidentId, cancellationToken);

        if (relation == null)
            throw new EntityNotFoundException("ProblemIncident relation");

        _dbContext.ProblemIncidents.Remove(relation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Unrelated incident {IncidentId} from problem {ProblemId}", incidentId, problemId);
    }

    /// <summary>Gets all incidents related to a problem.</summary>
    public async Task<IEnumerable<IncidentDto>> GetRelatedIncidentsAsync(int problemId, CancellationToken cancellationToken = default)
    {
        var problem = await _dbContext.Problems
            .Include(p => p.ProblemIncidents!)
                .ThenInclude(pi => pi.Incident)
            .FirstOrDefaultAsync(p => p.ProblemId == problemId && !p.IsDeleted, cancellationToken);

        if (problem == null)
            throw new EntityNotFoundException(nameof(Problem), problemId);

        var incidents = problem.ProblemIncidents?
            .Where(pi => pi.Incident != null && !pi.Incident.IsDeleted)
            .Select(pi => MapIncidentToDto(pi.Incident!))
            .ToList() ?? new List<IncidentDto>();

        return incidents;
    }

    /// <summary>Marks a problem as a known error.</summary>
    public async Task MarkAsKnownErrorAsync(int problemId, int modifiedById, CancellationToken cancellationToken = default)
    {
        var problem = await _dbContext.Problems.FindAsync(new object[] { problemId }, cancellationToken: cancellationToken);

        if (problem == null || problem.IsDeleted)
            throw new EntityNotFoundException(nameof(Problem), problemId);

        if (string.IsNullOrEmpty(problem.RootCause))
            throw new BusinessRuleException("KnownError", "Root cause analysis must be completed before marking as known error");

        problem.KnownError = true;
        problem.KnownErrorDate = DateTime.UtcNow;
        problem.State = ProblemState.KnownError;
        problem.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Problem {ProblemNumber} marked as known error", problem.Number);
    }

    /// <summary>Resolves a problem with root cause analysis and solution.</summary>
    public async Task<ProblemDto> ResolveProblemAsync(int problemId, ResolveProblemDto dto, int resolvedById, CancellationToken cancellationToken = default)
    {
        var problem = await _dbContext.Problems.FindAsync(new object[] { problemId }, cancellationToken: cancellationToken);

        if (problem == null || problem.IsDeleted)
            throw new EntityNotFoundException(nameof(Problem), problemId);

        if (string.IsNullOrWhiteSpace(dto.RootCause))
            throw new ValidationException(nameof(dto.RootCause), "Root cause is required");

        if (string.IsNullOrWhiteSpace(dto.Solution))
            throw new ValidationException(nameof(dto.Solution), "Solution is required");

        problem.RootCause = dto.RootCause;
        problem.Workaround = dto.Workaround;
        problem.Solution = dto.Solution;
        problem.ResolutionCode = dto.ResolutionCode;
        problem.State = ProblemState.Resolved;
        problem.ResolvedAt = DateTime.UtcNow;
        problem.FixVerified = true;
        problem.VerifiedAt = DateTime.UtcNow;
        problem.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Problem {ProblemNumber} resolved", problem.Number);

        return await MapToDto(problem, cancellationToken);
    }

    /// <summary>Closes a resolved problem optionally creating a knowledge article.</summary>
    public async Task<ProblemDto> CloseProblemAsync(int problemId, CloseProblemDto dto, int closedById, CancellationToken cancellationToken = default)
    {
        var problem = await _dbContext.Problems.FindAsync(new object[] { problemId }, cancellationToken: cancellationToken);

        if (problem == null || problem.IsDeleted)
            throw new EntityNotFoundException(nameof(Problem), problemId);

        if (problem.State != ProblemState.Resolved && problem.State != ProblemState.KnownError)
            throw new BusinessRuleException("CloseProblem", "Only resolved or known error problems can be closed");

        problem.State = ProblemState.Closed;
        problem.ClosedAt = DateTime.UtcNow;
        problem.ClosedById = closedById;
        problem.ClosureNotes = dto.ClosureNotes;
        problem.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Create knowledge article if requested
        if (dto.CreateKnowledgeArticle && !string.IsNullOrEmpty(problem.Solution))
        {
            await CreateKnowledgeArticleFromProblemAsync(problemId, closedById, cancellationToken);
        }

        _logger.LogInformation("Problem {ProblemNumber} closed", problem.Number);

        return await MapToDto(problem, cancellationToken);
    }

    /// <summary>Reopens a closed or resolved problem.</summary>
    public async Task<ProblemDto> ReopenProblemAsync(int problemId, ReopenProblemDto dto, int reopenedById, CancellationToken cancellationToken = default)
    {
        var problem = await _dbContext.Problems.FindAsync(new object[] { problemId }, cancellationToken: cancellationToken);

        if (problem == null || problem.IsDeleted)
            throw new EntityNotFoundException(nameof(Problem), problemId);

        if (problem.State != ProblemState.Closed && problem.State != ProblemState.Resolved)
            throw new BusinessRuleException("ReopenProblem", "Only closed or resolved problems can be reopened");

        problem.State = ProblemState.Investigating;
        problem.ModifiedAt = DateTime.UtcNow;

        // Add reopening comment
        if (!string.IsNullOrEmpty(dto.ReopenReason))
        {
            _dbContext.ProblemComments.Add(new ProblemComment
            {
                ProblemId = problemId,
                Comment = $"Problem reopened: {dto.ReopenReason}",
                IsInternal = true,
                CreatedById = reopenedById,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Problem {ProblemNumber} reopened", problem.Number);

        return await MapToDto(problem, cancellationToken);
    }

    /// <summary>Performs root cause analysis on incidents to identify a pattern.</summary>
    public async Task<ProblemRootCauseAnalysisDto> AnalyzeIncidentsAsync(
        List<int> incidentIds, int createdById, CancellationToken cancellationToken = default)
    {
        if (!incidentIds.Any())
            throw new ValidationException(nameof(incidentIds), "At least one incident ID is required");

        var incidents = await _dbContext.Incidents
            .Where(i => incidentIds.Contains(i.IncidentId) && !i.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!incidents.Any())
            throw new ValidationException(nameof(incidentIds), "No valid incidents found");

        var analysis = new ProblemRootCauseAnalysisDto
        {
            Symptoms = string.Join("; ", incidents.Select(i => i.ShortDescription).Distinct()),
            AnalysisCompletedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Analyzed {Count} incidents for pattern identification", incidents.Count);

        return analysis;
    }

    /// <summary>Determines the root cause of a problem.</summary>
    public async Task<ProblemDto> DetermineCauseAsync(
        int problemId, string rootCause, string? workaround, int modifiedById, CancellationToken cancellationToken = default)
    {
        var problem = await _dbContext.Problems.FindAsync(new object[] { problemId }, cancellationToken: cancellationToken);

        if (problem == null || problem.IsDeleted)
            throw new EntityNotFoundException(nameof(Problem), problemId);

        if (string.IsNullOrWhiteSpace(rootCause))
            throw new ValidationException(nameof(rootCause), "Root cause cannot be empty");

        problem.RootCause = rootCause;
        problem.Workaround = workaround;
        problem.State = ProblemState.RootCauseAnalysis;
        problem.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Root cause determined for problem {ProblemNumber}", problem.Number);

        return await MapToDto(problem, cancellationToken);
    }

    /// <summary>Identifies a temporary workaround while a permanent solution is being developed.</summary>
    public async Task<ProblemDto> IdentifyTemporaryWorkaroundAsync(
        int problemId, string workaround, int modifiedById, CancellationToken cancellationToken = default)
    {
        var problem = await _dbContext.Problems.FindAsync(new object[] { problemId }, cancellationToken: cancellationToken);

        if (problem == null || problem.IsDeleted)
            throw new EntityNotFoundException(nameof(Problem), problemId);

        if (string.IsNullOrWhiteSpace(workaround))
            throw new ValidationException(nameof(workaround), "Workaround cannot be empty");

        problem.Workaround = workaround;
        problem.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Temporary workaround identified for problem {ProblemNumber}", problem.Number);

        return await MapToDto(problem, cancellationToken);
    }

    /// <summary>Documents the problem with detailed RCA analysis.</summary>
    public async Task<ProblemRootCauseAnalysisDto> DocumentProblemAsync(
        int problemId, ProblemRootCauseAnalysisDto analysisDto, int modifiedById, CancellationToken cancellationToken = default)
    {
        var problem = await _dbContext.Problems.FindAsync(new object[] { problemId }, cancellationToken: cancellationToken);

        if (problem == null || problem.IsDeleted)
            throw new EntityNotFoundException(nameof(Problem), problemId);

        problem.FiveWhysAnalysis = analysisDto.FiveWhysAnalysis;
        problem.FishboneAnalysis = analysisDto.FishboneAnalysis;
        problem.Timeline = analysisDto.Timeline;
        problem.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Problem {ProblemNumber} documented with RCA analysis", problem.Number);

        return analysisDto;
    }

    /// <summary>Gets change management metrics and statistics.</summary>
    public async Task<ProblemMetricsDto> GetProblemMetricsAsync(
        DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Problems.Where(p => !p.IsDeleted);

        if (fromDate.HasValue)
            query = query.Where(p => p.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(p => p.CreatedAt <= toDate.Value);

        var problems = await query.ToListAsync(cancellationToken);

        var metrics = new ProblemMetricsDto
        {
            TotalProblems = problems.Count,
            NewProblems = problems.Count(p => p.State == ProblemState.New),
            InvestigatingProblems = problems.Count(p => p.State == ProblemState.Investigating),
            KnownErrors = problems.Count(p => p.KnownError),
            ResolvedProblems = problems.Count(p => p.State == ProblemState.Resolved),
            ReportGeneratedAt = DateTime.UtcNow
        };

        // Calculate average resolution time
        var resolvedProblems = problems.Where(p => p.ResolvedAt.HasValue && p.CreatedAt != null).ToList();
        if (resolvedProblems.Any())
        {
            metrics.AverageResolutionDays = resolvedProblems
                .Average(p => (p.ResolvedAt.Value - p.CreatedAt).TotalDays);
        }

        return metrics;
    }

    /// <summary>Creates a knowledge article from a resolved problem.</summary>
    public async Task<KnowledgeArticleDto> CreateKnowledgeArticleFromProblemAsync(
        int problemId, int authorId, CancellationToken cancellationToken = default)
    {
        var problem = await _dbContext.Problems.FindAsync(new object[] { problemId }, cancellationToken: cancellationToken);

        if (problem == null || problem.IsDeleted)
            throw new EntityNotFoundException(nameof(Problem), problemId);

        if (string.IsNullOrEmpty(problem.Solution))
            throw new BusinessRuleException("CreateArticle", "Problem must have a solution before creating knowledge article");

        // Create knowledge article
        var article = new KnowledgeArticle
        {
            Title = problem.ShortDescription,
            ArticleBody = $"Solution: {problem.Solution}\n\nRoot Cause: {problem.RootCause}\n\nWorkaround: {problem.Workaround}",
            ArticleType = ArticleType.Troubleshooting,
            PublishingState = PublishingState.Draft,
            AuthorId = authorId,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ITSMKnowledgeArticles.Add(article);
        problem.KnowledgeArticleId = article.ArticleId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Knowledge article created from problem {ProblemNumber}", problem.Number);

        return new KnowledgeArticleDto
        {
            ArticleId = article.ArticleId,
            Title = article.Title,
            ArticleBody = article.ArticleBody,
            ArticleType = article.ArticleType,
            PublishingState = article.PublishingState,
            AuthorId = authorId
        };
    }

    /// <summary>Creates a subtask for problem investigation.</summary>
    public async Task<ProblemTaskDto> CreateTaskAsync(
        int problemId, CreateProblemTaskDto dto, int createdById, CancellationToken cancellationToken = default)
    {
        var problem = await _dbContext.Problems.FindAsync(new object[] { problemId }, cancellationToken: cancellationToken);

        if (problem == null || problem.IsDeleted)
            throw new EntityNotFoundException(nameof(Problem), problemId);

        var task = new ProblemTask
        {
            ProblemId = problemId,
            TaskName = dto.TaskName,
            Description = dto.Description,
            AssignedToId = dto.AssignedToId,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ProblemTasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Task created for problem {ProblemNumber}", problem.Number);

        return MapTaskToDto(task);
    }

    /// <summary>Gets all tasks for a problem.</summary>
    public async Task<IEnumerable<ProblemTaskDto>> GetTasksAsync(int problemId, CancellationToken cancellationToken = default)
    {
        var tasks = await _dbContext.ProblemTasks
            .Include(t => t.AssignedTo)
            .Where(t => t.ProblemId == problemId && !t.IsDeleted)
            .ToListAsync(cancellationToken);

        return tasks.Select(MapTaskToDto);
    }

    /// <summary>Completes a problem task.</summary>
    public async Task<ProblemTaskDto> CompleteTaskAsync(int taskId, int completedById, CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.ProblemTasks.FindAsync(new object[] { taskId }, cancellationToken: cancellationToken);

        if (task == null || task.IsDeleted)
            throw new EntityNotFoundException("ProblemTask", taskId);

        task.IsCompleted = true;
        task.CompletedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Problem task {TaskId} completed", taskId);

        return MapTaskToDto(task);
    }

    /// <summary>Adds a comment to a problem.</summary>
    public async Task<ProblemCommentDto> AddCommentAsync(
        int problemId, CreateProblemCommentDto dto, int createdById, CancellationToken cancellationToken = default)
    {
        var problem = await _dbContext.Problems.FindAsync(new object[] { problemId }, cancellationToken: cancellationToken);

        if (problem == null || problem.IsDeleted)
            throw new EntityNotFoundException(nameof(Problem), problemId);

        var comment = new ProblemComment
        {
            ProblemId = problemId,
            Comment = dto.Comment,
            IsInternal = dto.IsInternal,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ProblemComments.Add(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Comment added to problem {ProblemNumber}", problem.Number);

        return MapCommentToDto(comment);
    }

    /// <summary>Gets all comments for a problem.</summary>
    public async Task<IEnumerable<ProblemCommentDto>> GetCommentsAsync(int problemId, CancellationToken cancellationToken = default)
    {
        var comments = await _dbContext.ProblemComments
            .Include(c => c.CreatedBy)
            .Where(c => c.ProblemId == problemId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return comments.Select(MapCommentToDto);
    }

    /// <summary>Assigns a problem to an investigator and/or manager.</summary>
    public async Task<ProblemDto> AssignProblemAsync(
        int problemId, AssignProblemDto dto, int modifiedById, CancellationToken cancellationToken = default)
    {
        var problem = await _dbContext.Problems.FindAsync(new object[] { problemId }, cancellationToken: cancellationToken);

        if (problem == null || problem.IsDeleted)
            throw new EntityNotFoundException(nameof(Problem), problemId);

        if (dto.ProblemInvestigatorId.HasValue)
            problem.ProblemInvestigatorId = dto.ProblemInvestigatorId.Value;

        if (dto.ProblemManagerId.HasValue)
            problem.ProblemManagerId = dto.ProblemManagerId.Value;

        if (dto.AssignmentGroupId.HasValue)
            problem.AssignmentGroupId = dto.AssignmentGroupId.Value;

        problem.State = ProblemState.Investigating;
        problem.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Problem {ProblemNumber} assigned", problem.Number);

        return await MapToDto(problem, cancellationToken);
    }

    // Helper Methods

    private async Task<string> GenerateProblemNumberAsync(CancellationToken cancellationToken)
    {
        var lastProblem = await _dbContext.Problems
            .OrderByDescending(p => p.ProblemId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        var nextNumber = (lastProblem?.ProblemId ?? 0) + 1;
        return $"PRB{nextNumber:000000}";
    }

    private async Task<ProblemDto> MapToDto(Problem problem, CancellationToken cancellationToken)
    {
        var relatedIncidentCount = await _dbContext.ProblemIncidents
            .CountAsync(pi => pi.ProblemId == problem.ProblemId, cancellationToken);

        return new ProblemDto
        {
            ProblemId = problem.ProblemId,
            Number = problem.Number,
            ShortDescription = problem.ShortDescription,
            Description = problem.Description,
            Priority = problem.Priority,
            State = problem.State,
            RootCause = problem.RootCause,
            Workaround = problem.Workaround,
            KnownError = problem.KnownError,
            Solution = problem.Solution,
            ProblemInvestigatorId = problem.ProblemInvestigatorId,
            ProblemInvestigatorName = problem.ProblemInvestigator?.Username,
            CreatedAt = problem.CreatedAt,
            RelatedIncidentCount = relatedIncidentCount
        };
    }

    private ProblemTaskDto MapTaskToDto(ProblemTask task)
    {
        return new ProblemTaskDto
        {
            TaskId = task.TaskId,
            ProblemId = task.ProblemId,
            TaskName = task.TaskName,
            Description = task.Description,
            AssignedToId = task.AssignedToId,
            AssignedToName = task.AssignedTo?.Username,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            CompletedAt = task.CompletedAt,
            CreatedAt = task.CreatedAt
        };
    }

    private ProblemCommentDto MapCommentToDto(ProblemComment comment)
    {
        return new ProblemCommentDto
        {
            CommentId = comment.CommentId,
            ProblemId = comment.ProblemId,
            Comment = comment.Comment,
            IsInternal = comment.IsInternal,
            CreatedById = comment.CreatedById,
            CreatedByName = comment.CreatedBy?.Username,
            CreatedAt = comment.CreatedAt
        };
    }

    private IncidentDto MapIncidentToDto(Incident incident)
    {
        return new IncidentDto
        {
            IncidentId = incident.IncidentId,
            Number = incident.Number,
            ShortDescription = incident.ShortDescription,
            CreatedAt = incident.CreatedAt
        };
    }
}

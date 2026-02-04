// This file is part of the CRM Solution.
// Copyright (c) 2025 CRM Solution Contributors
// Licensed under the AGPL-3.0 license.

using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

public class ProblemService : IProblemService
{
    private readonly IDbContextResolver _dbContextResolver;
    private readonly ILogger<ProblemService> _logger;

    public ProblemService(IDbContextResolver dbContextResolver, ILogger<ProblemService> logger)
    {
        _dbContextResolver = dbContextResolver;
        _logger = logger;
    }

    public async Task<ProblemDto> CreateProblemAsync(CreateProblemDto dto, int createdById)
    {
        var context = _dbContextResolver.ResolveContext();
        
        var problem = new Problem
        {
            Number = await GenerateProblemNumberAsync(context),
            ShortDescription = dto.ShortDescription,
            Description = dto.Description,
            Priority = dto.Priority,
            CategoryId = dto.CategoryId,
            State = ProblemState.New,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };

        context.Problems.Add(problem);
        await context.SaveChangesAsync();

        if (dto.IncidentIds != null && dto.IncidentIds.Any())
        {
            foreach (var incidentId in dto.IncidentIds)
            {
                await LinkIncidentAsync(problem.ProblemId, incidentId, createdById);
            }
        }

        _logger.LogInformation("Created problem {ProblemNumber}", problem.Number);
        return await MapToDto(problem, context);
    }

    public async Task<ProblemDto?> GetProblemByIdAsync(int problemId)
    {
        var context = _dbContextResolver.ResolveContext();
        var problem = await context.Problems
            .Include(p => p.Category)
            .Include(p => p.ProblemInvestigator)
            .FirstOrDefaultAsync(p => p.ProblemId == problemId && !p.IsDeleted);

        return problem == null ? null : await MapToDto(problem, context);
    }

    public async Task<(IEnumerable<ProblemDto> Items, int TotalCount)> GetProblemsAsync(ProblemFilterDto filter)
    {
        var context = _dbContextResolver.ResolveContext();
        var query = context.Problems
            .Include(p => p.Category)
            .Include(p => p.ProblemInvestigator)
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(p => p.Number.Contains(filter.SearchTerm) ||
                                    p.ShortDescription.Contains(filter.SearchTerm));
        }

        if (filter.State.HasValue)
            query = query.Where(p => p.State == filter.State.Value);

        if (filter.Priority.HasValue)
            query = query.Where(p => p.Priority == filter.Priority.Value);

        if (filter.KnownError.HasValue)
            query = query.Where(p => p.KnownError == filter.KnownError.Value);

        var totalCount = await query.CountAsync();

        var problems = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = new List<ProblemDto>();
        foreach (var problem in problems)
        {
            dtos.Add(await MapToDto(problem, context));
        }

        return (dtos, totalCount);
    }

    public async Task<ProblemDto> UpdateProblemAsync(int problemId, UpdateProblemDto dto, int modifiedById)
    {
        var context = _dbContextResolver.ResolveContext();
        var problem = await context.Problems.FindAsync(problemId);
        
        if (problem == null || problem.IsDeleted)
            throw new KeyNotFoundException($"Problem {problemId} not found");

        if (dto.ShortDescription != null)
            problem.ShortDescription = dto.ShortDescription;

        if (dto.Description != null)
            problem.Description = dto.Description;

        if (dto.State.HasValue)
            problem.State = dto.State.Value;

        if (dto.RootCause != null)
            problem.RootCause = dto.RootCause;

        if (dto.Workaround != null)
            problem.Workaround = dto.Workaround;

        if (dto.Solution != null)
            problem.Solution = dto.Solution;

        if (dto.KnownError.HasValue)
        {
            problem.KnownError = dto.KnownError.Value;
            if (dto.KnownError.Value && !problem.KnownErrorDate.HasValue)
                problem.KnownErrorDate = DateTime.UtcNow;
        }

        if (dto.ProblemInvestigatorId.HasValue)
            problem.ProblemInvestigatorId = dto.ProblemInvestigatorId.Value;

        problem.ModifiedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return await MapToDto(problem, context);
    }

    public async Task<bool> LinkIncidentAsync(int problemId, int incidentId, int createdById)
    {
        var context = _dbContextResolver.ResolveContext();
        
        var existing = await context.ProblemIncidents
            .AnyAsync(pi => pi.ProblemId == problemId && pi.IncidentId == incidentId);

        if (existing)
            return false;

        context.ProblemIncidents.Add(new ProblemIncident
        {
            ProblemId = problemId,
            IncidentId = incidentId,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        });

        var incident = await context.Incidents.FindAsync(incidentId);
        if (incident != null)
        {
            incident.ProblemId = problemId;
        }

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkAsKnownErrorAsync(int problemId, int modifiedById)
    {
        var context = _dbContextResolver.ResolveContext();
        var problem = await context.Problems.FindAsync(problemId);
        
        if (problem == null || problem.IsDeleted)
            return false;

        problem.KnownError = true;
        problem.KnownErrorDate = DateTime.UtcNow;
        problem.State = ProblemState.KnownError;
        problem.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        _logger.LogInformation("Marked problem {ProblemNumber} as known error", problem.Number);
        return true;
    }

    public async Task<IEnumerable<IncidentDto>> GetRelatedIncidentsAsync(int problemId)
    {
        var context = _dbContextResolver.ResolveContext();
        var incidentIds = await context.ProblemIncidents
            .Where(pi => pi.ProblemId == problemId)
            .Select(pi => pi.IncidentId)
            .ToListAsync();

        var incidents = await context.Incidents
            .Include(i => i.Caller)
            .Include(i => i.AssignedTo)
            .Where(i => incidentIds.Contains(i.IncidentId) && !i.IsDeleted)
            .ToListAsync();

        return incidents.Select(i => new IncidentDto
        {
            IncidentId = i.IncidentId,
            Number = i.Number,
            ShortDescription = i.ShortDescription,
            CallerName = i.Caller?.Username,
            State = i.State,
            Priority = i.Priority,
            CreatedAt = i.CreatedAt
        });
    }

    public async Task<bool> UpdateRootCauseAnalysisAsync(int problemId, string rootCause, string? workaround, int modifiedById)
    {
        var context = _dbContextResolver.ResolveContext();
        var problem = await context.Problems.FindAsync(problemId);
        
        if (problem == null || problem.IsDeleted)
            return false;

        problem.RootCause = rootCause;
        problem.Workaround = workaround;
        problem.ModifiedAt = DateTime.UtcNow;

        // If root cause is identified, update state to RootCauseAnalysis if still in investigation
        if (problem.State == ProblemState.Investigating)
        {
            problem.State = ProblemState.RootCauseAnalysis;
        }

        await context.SaveChangesAsync();
        _logger.LogInformation("Updated RCA for problem {ProblemNumber}", problem.Number);
        return true;
    }

    private async Task<string> GenerateProblemNumberAsync(ICrmDbContext context)
    {
        var lastProblem = await context.Problems
            .OrderByDescending(p => p.ProblemId)
            .FirstOrDefaultAsync();

        var nextNumber = lastProblem != null ? lastProblem.ProblemId + 1 : 1;
        return $"PRB{nextNumber:D7}";
    }

    private async Task<ProblemDto> MapToDto(Problem problem, ICrmDbContext context)
    {
        var incidentCount = await context.ProblemIncidents
            .CountAsync(pi => pi.ProblemId == problem.ProblemId);

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
            RelatedIncidentCount = incidentCount
        };
    }
}

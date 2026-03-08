// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;
using CRM.Core.Exceptions;
using CRM.Core.Interfaces.ITSM;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Adapter service to keep existing controllers using <see cref="IProblemService"/>
/// while delegating to the full <see cref="IProblemManagementService"/> implementation.
/// </summary>
public class ProblemService : IProblemService
{
    private readonly IProblemManagementService _problemManagementService;
    private readonly ILogger<ProblemService> _logger;

    public ProblemService(
        IProblemManagementService problemManagementService,
        ILogger<ProblemService> logger)
    {
        _problemManagementService = problemManagementService;
        _logger = logger;
    }

    public Task<ProblemDto> CreateProblemAsync(CreateProblemDto dto, int createdById)
        => _problemManagementService.CreateProblemAsync(dto, createdById);

    public async Task<ProblemDto?> GetProblemByIdAsync(int problemId)
    {
        try
        {
            return await _problemManagementService.GetProblemByIdAsync(problemId);
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning(ex, "Problem {ProblemId} not found", problemId);
            return null;
        }
    }

    public Task<(IEnumerable<ProblemDto> Items, int TotalCount)> GetProblemsAsync(ProblemFilterDto filter)
        => _problemManagementService.ListProblemsAsync(filter);

    public Task<ProblemDto> UpdateProblemAsync(int problemId, UpdateProblemDto dto, int modifiedById)
        => _problemManagementService.UpdateProblemAsync(problemId, dto, modifiedById);

    public async Task<bool> LinkIncidentAsync(int problemId, int incidentId, int createdById)
    {
        await _problemManagementService.RelateProblemToIncidentsAsync(problemId, new List<int> { incidentId }, createdById);
        return true;
    }

    public async Task<bool> MarkAsKnownErrorAsync(int problemId, int modifiedById)
    {
        await _problemManagementService.MarkAsKnownErrorAsync(problemId, modifiedById);
        return true;
    }

    public Task<IEnumerable<IncidentDto>> GetRelatedIncidentsAsync(int problemId)
        => _problemManagementService.GetRelatedIncidentsAsync(problemId);

    public async Task<bool> UpdateRootCauseAnalysisAsync(int problemId, string rootCause, string? workaround, int modifiedById)
    {
        await _problemManagementService.DetermineCauseAsync(problemId, rootCause, workaround, modifiedById);
        return true;
    }
}

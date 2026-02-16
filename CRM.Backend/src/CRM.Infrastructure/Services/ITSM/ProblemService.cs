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

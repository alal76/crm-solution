// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

public class IncidentService : IIncidentService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ISLAService _slaService;
    private readonly ILogger<IncidentService> _logger;

    public IncidentService(
        ICrmDbContext dbContext,
        ISLAService slaService,
        ILogger<IncidentService> logger)
    {
        _dbContext = dbContext;
        _slaService = slaService;
        _logger = logger;
    }

    public async Task<IncidentDto> CreateIncidentAsync(CreateIncidentDto dto, int createdById)
    {
        var incident = new Incident
        {
            Number = await GenerateIncidentNumberAsync(_dbContext),
            ShortDescription = dto.ShortDescription,
            Description = dto.Description,
            CallerId = dto.CallerId,
            ContactType = dto.ContactType,
            CategoryId = dto.CategoryId,
            SubcategoryId = dto.SubcategoryId,
            ConfigurationItemId = dto.ConfigurationItemId,
            Impact = dto.Impact,
            Urgency = dto.Urgency,
            State = IncidentState.New,
            OpenedAt = DateTime.UtcNow,
            OpenedById = createdById,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Incidents.Add(incident);
        await _dbContext.SaveChangesAsync();

        // Calculate priority from impact and urgency
        var priority = CalculatePriority(dto.Impact, dto.Urgency);

        // Start SLA tracking
        await _slaService.StartSLAAsync(incident.IncidentId, SLATargetType.Incident, priority);

        _logger.LogInformation("Created incident {IncidentNumber} by user {UserId}", incident.Number, createdById);

        return await MapToDto(incident, _dbContext);
    }

    public async Task<IncidentDto?> GetIncidentByIdAsync(int incidentId)
    {
        var incident = await _dbContext.Incidents
            .Include(i => i.Caller)
            .Include(i => i.Category)
            .Include(i => i.Subcategory)
            .Include(i => i.AssignmentGroup)
            .Include(i => i.AssignedTo)
            .FirstOrDefaultAsync(i => i.IncidentId == incidentId && !i.IsDeleted);

        return incident == null ? null : await MapToDto(incident, _dbContext);
    }

    public async Task<(IEnumerable<IncidentDto> Items, int TotalCount)> GetIncidentsAsync(IncidentFilterDto filter)
    {
        var query = _dbContext.Incidents
            .Include(i => i.Caller)
            .Include(i => i.Category)
            .Include(i => i.AssignmentGroup)
            .Include(i => i.AssignedTo)
            .Where(i => !i.IsDeleted);

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(i => i.Number.Contains(filter.SearchTerm) ||
                                    i.ShortDescription.Contains(filter.SearchTerm));
        }

        if (filter.State.HasValue)
        {
            query = query.Where(i => i.State == filter.State.Value);
        }

        if (filter.Priority.HasValue)
        {
            query = query.Where(i => i.Priority == filter.Priority.Value);
        }

        if (filter.AssignedToId.HasValue)
        {
            query = query.Where(i => i.AssignedToId == filter.AssignedToId.Value);
        }

        if (filter.AssignmentGroupId.HasValue)
        {
            query = query.Where(i => i.AssignmentGroupId == filter.AssignmentGroupId.Value);
        }

        if (filter.SLABreached.HasValue)
        {
            query = query.Where(i => i.SLABreached == filter.SLABreached.Value);
        }

        if (filter.MajorIncident.HasValue)
        {
            query = query.Where(i => i.MajorIncident == filter.MajorIncident.Value);
        }

        if (filter.CreatedFrom.HasValue)
        {
            query = query.Where(i => i.CreatedAt >= filter.CreatedFrom.Value);
        }

        if (filter.CreatedTo.HasValue)
        {
            query = query.Where(i => i.CreatedAt <= filter.CreatedTo.Value);
        }

        var totalCount = await query.CountAsync();

        var incidents = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = new List<IncidentDto>();
        foreach (var incident in incidents)
        {
            dtos.Add(await MapToDto(incident, _dbContext));
        }

        return (dtos, totalCount);
    }

    public async Task<IncidentDto> UpdateIncidentAsync(int incidentId, UpdateIncidentDto dto, int modifiedById)
    {
        var incident = await _dbContext.Incidents.FindAsync(incidentId);

        if (incident == null || incident.IsDeleted)
        {
            throw new KeyNotFoundException($"Incident {incidentId} not found");
        }

        var changes = new List<string>();

        if (dto.ShortDescription != null && dto.ShortDescription != incident.ShortDescription)
        {
            changes.Add($"ShortDescription: '{incident.ShortDescription}' -> '{dto.ShortDescription}'");
            incident.ShortDescription = dto.ShortDescription;
        }

        if (dto.Description != null)
        {
            incident.Description = dto.Description;
        }

        if (dto.CategoryId.HasValue)
        {
            incident.CategoryId = dto.CategoryId.Value;
        }

        if (dto.SubcategoryId.HasValue)
        {
            incident.SubcategoryId = dto.SubcategoryId.Value;
        }

        if (dto.Impact.HasValue)
        {
            changes.Add($"Impact: {incident.Impact} -> {dto.Impact.Value}");
            incident.Impact = dto.Impact.Value;
        }

        if (dto.Urgency.HasValue)
        {
            changes.Add($"Urgency: {incident.Urgency} -> {dto.Urgency.Value}");
            incident.Urgency = dto.Urgency.Value;
        }

        if (dto.State.HasValue)
        {
            changes.Add($"State: {incident.State} -> {dto.State.Value}");
            incident.State = dto.State.Value;

            // Handle SLA pause/resume based on state
            if (dto.State.Value == IncidentState.OnHold)
            {
                await _slaService.PauseSLAAsync(incidentId, SLATargetType.Incident, "Incident on hold");
            }
            else if (incident.State == IncidentState.OnHold && dto.State.Value != IncidentState.OnHold)
            {
                await _slaService.ResumeSLAAsync(incidentId, SLATargetType.Incident);
            }
        }

        if (dto.AssignmentGroupId.HasValue)
        {
            changes.Add($"AssignmentGroup: {incident.AssignmentGroupId} -> {dto.AssignmentGroupId.Value}");
            incident.AssignmentGroupId = dto.AssignmentGroupId.Value;
        }

        if (dto.AssignedToId.HasValue)
        {
            changes.Add($"AssignedTo: {incident.AssignedToId} -> {dto.AssignedToId.Value}");
            incident.AssignedToId = dto.AssignedToId.Value;
        }

        incident.ModifiedAt = DateTime.UtcNow;

        // Log history
        foreach (var change in changes)
        {
            _dbContext.IncidentHistory.Add(new IncidentHistory
            {
                IncidentId = incidentId,
                Field = change.Split(':')[0].Trim(),
                OldValue = change.Split("->")[0].Split(':')[1].Trim(),
                NewValue = change.Split("->")[1].Trim(),
                ChangedById = modifiedById,
                ChangedAt = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Updated incident {IncidentId} by user {UserId}", incidentId, modifiedById);

        return await MapToDto(incident, _dbContext);
    }

    public async Task<bool> AssignIncidentAsync(int incidentId, int? assignedToId, int? assignmentGroupId, int modifiedById)
    {
        var incident = await _dbContext.Incidents.FindAsync(incidentId);

        if (incident == null || incident.IsDeleted)
        {
            return false;
        }

        incident.AssignedToId = assignedToId;
        incident.AssignmentGroupId = assignmentGroupId;

        if (incident.State == IncidentState.New)
        {
            incident.State = IncidentState.Assigned;
        }

        incident.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Assigned incident {IncidentId} to user {AssignedToId} / group {GroupId}",
            incidentId, assignedToId, assignmentGroupId);

        return true;
    }

    public async Task<bool> EscalateIncidentAsync(int incidentId, int modifiedById)
    {
        var incident = await _dbContext.Incidents.FindAsync(incidentId);

        if (incident == null || incident.IsDeleted)
        {
            return false;
        }

        incident.EscalationLevel++;
        incident.State = IncidentState.InProgress;
        incident.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogWarning("Escalated incident {IncidentId} to level {Level} by user {UserId}",
            incidentId, incident.EscalationLevel, modifiedById);

        return true;
    }

    public async Task<IncidentDto> ResolveIncidentAsync(int incidentId, ResolveIncidentDto dto, int resolvedById)
    {
        var incident = await _dbContext.Incidents.FindAsync(incidentId);

        if (incident == null || incident.IsDeleted)
        {
            throw new KeyNotFoundException($"Incident {incidentId} not found");
        }

        incident.State = IncidentState.Resolved;
        incident.ResolutionCode = dto.ResolutionCode;
        incident.ResolutionNotes = dto.ResolutionNotes;
        incident.ResolvedAt = DateTime.UtcNow;
        incident.ResolvedById = resolvedById;
        incident.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        // Complete resolution SLA
        await _slaService.CompleteSLAAsync(incidentId, SLATargetType.Incident, false, true);

        _logger.LogInformation("Resolved incident {IncidentId} by user {UserId}", incidentId, resolvedById);

        return await MapToDto(incident, _dbContext);
    }

    public async Task<bool> CloseIncidentAsync(int incidentId, int closedById)
    {
        var incident = await _dbContext.Incidents.FindAsync(incidentId);

        if (incident == null || incident.IsDeleted)
        {
            return false;
        }

        if (incident.State != IncidentState.Resolved)
        {
            throw new InvalidOperationException("Cannot close incident that is not resolved");
        }

        incident.State = IncidentState.Closed;
        incident.ClosedAt = DateTime.UtcNow;
        incident.ClosedById = closedById;
        incident.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Closed incident {IncidentId} by user {UserId}", incidentId, closedById);

        return true;
    }

    public async Task<bool> ReopenIncidentAsync(int incidentId, int modifiedById)
    {
        var incident = await _dbContext.Incidents.FindAsync(incidentId);

        if (incident == null || incident.IsDeleted)
        {
            return false;
        }

        if (incident.State != IncidentState.Resolved && incident.State != IncidentState.Closed)
        {
            throw new InvalidOperationException("Can only reopen resolved or closed incidents");
        }

        incident.State = IncidentState.InProgress;
        incident.ResolvedAt = null;
        incident.ResolvedById = null;
        incident.ClosedAt = null;
        incident.ClosedById = null;
        incident.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        // Resume SLA
        await _slaService.ResumeSLAAsync(incidentId, SLATargetType.Incident);

        _logger.LogWarning("Reopened incident {IncidentId} by user {UserId}", incidentId, modifiedById);

        return true;
    }

    public async Task<bool> AddCommentAsync(int incidentId, string comment, bool isInternal, int createdById)
    {
        var incidentComment = new IncidentComment
        {
            IncidentId = incidentId,
            Comment = comment,
            IsInternal = isInternal,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.IncidentComments.Add(incidentComment);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<IncidentComment>> GetCommentsAsync(int incidentId)
    {
        return await _dbContext.IncidentComments
            .Include(c => c.CreatedBy)
            .Where(c => c.IncidentId == incidentId && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    private async Task<string> GenerateIncidentNumberAsync(ICrmDbContext context)
    {
        var lastIncident = await context.Incidents
            .OrderByDescending(i => i.IncidentId)
            .FirstOrDefaultAsync();

        var nextNumber = lastIncident != null ? lastIncident.IncidentId + 1 : 1;
        return $"INC{nextNumber:D7}";
    }

    private Task<IncidentDto> MapToDto(Incident incident, ICrmDbContext context)
    {
        return Task.FromResult(new IncidentDto
        {
            IncidentId = incident.IncidentId,
            Number = incident.Number,
            ShortDescription = incident.ShortDescription,
            Description = incident.Description,
            CallerId = incident.CallerId,
            CallerName = incident.Caller?.Username,
            ContactType = incident.ContactType,
            OpenedAt = incident.OpenedAt,
            CategoryId = incident.CategoryId,
            CategoryName = incident.Category?.Name,
            SubcategoryId = incident.SubcategoryId,
            SubcategoryName = incident.Subcategory?.Name,
            Impact = incident.Impact,
            Urgency = incident.Urgency,
            Priority = incident.Priority,
            State = incident.State,
            AssignmentGroupId = incident.AssignmentGroupId,
            AssignmentGroupName = incident.AssignmentGroup?.Name,
            AssignedToId = incident.AssignedToId,
            AssignedToName = incident.AssignedTo?.Username,
            ResolutionCode = incident.ResolutionCode,
            ResolutionNotes = incident.ResolutionNotes,
            ResolvedAt = incident.ResolvedAt,
            ClosedAt = incident.ClosedAt,
            SLABreached = incident.SLABreached,
            ResponseDueAt = incident.ResponseDueAt,
            ResolutionDueAt = incident.ResolutionDueAt,
            MajorIncident = incident.MajorIncident,
            ProblemId = incident.ProblemId,
            CreatedAt = incident.CreatedAt
        });
    }

    /// <summary>
    /// Calculate priority from impact and urgency using a matrix.
    /// </summary>
    private static int CalculatePriority(IncidentImpact impact, IncidentUrgency urgency)
    {
        // Priority matrix: Impact (rows) x Urgency (columns) = Priority (1-4)
        // High Impact + High Urgency = P1 (lowest number = highest priority)
        return ((int)impact + (int)urgency) / 2;
    }
}

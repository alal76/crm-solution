using CRM.Core.Dtos.WorkflowEngine;
using CRM.Core.Entities.WorkflowEngine;
using CRM.Core.Interfaces.WorkflowEngine;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.WorkflowEngine.Repositories;

/// <summary>
/// Repository for WorkflowDefinition entities
/// </summary>
public class WorkflowDefinitionRepository : IWorkflowDefinitionRepository
{
    private readonly CrmDbContext _context;
    private readonly ILogger<WorkflowDefinitionRepository> _logger;

    public WorkflowDefinitionRepository(
        CrmDbContext context,
        ILogger<WorkflowDefinitionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<WorkflowDefinition?> GetByIdAsync(
        int id, 
        bool includeSteps = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WorkflowDefinitions.AsQueryable();
        
        if (includeSteps)
        {
            query = query.Include(w => w.Steps.OrderBy(s => s.OrderIndex));
        }

        return await query
            .Include(w => w.CreatedByUser)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<List<WorkflowDefinition>> GetAllAsync(
        string? status = null, 
        string? triggerType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WorkflowDefinitions
            .Include(w => w.Steps)
            .Include(w => w.CreatedByUser)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(w => w.Status == status);
        }
        else
        {
            query = query.Where(w => w.Status != WorkflowDefinitionStatus.Archived);
        }

        if (!string.IsNullOrEmpty(triggerType))
        {
            query = query.Where(w => w.TriggerType == triggerType);
        }

        return await query
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<WorkflowDefinition>> GetByTriggerAsync(
        string triggerEntityType, 
        string triggerEvent,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowDefinitions
            .Include(w => w.Steps.OrderBy(s => s.OrderIndex))
            .Where(w => w.Status == WorkflowDefinitionStatus.Published 
                && w.TriggerEntityType == triggerEntityType
                && (w.TriggerEvents == null || w.TriggerEvents.Contains(triggerEvent)))
            .OrderBy(w => w.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkflowDefinition> CreateAsync(
        WorkflowDefinition definition, 
        CancellationToken cancellationToken = default)
    {
        definition.CreatedAt = DateTime.UtcNow;
        definition.UpdatedAt = DateTime.UtcNow;

        _context.WorkflowDefinitions.Add(definition);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created workflow definition {Id}: {Name}", definition.Id, definition.Name);

        return definition;
    }

    public async Task<WorkflowDefinition> UpdateAsync(
        WorkflowDefinition definition, 
        CancellationToken cancellationToken = default)
    {
        definition.UpdatedAt = DateTime.UtcNow;
        
        _context.WorkflowDefinitions.Update(definition);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated workflow definition {Id}: {Name}", definition.Id, definition.Name);

        return definition;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var definition = await _context.WorkflowDefinitions.FindAsync(new object[] { id }, cancellationToken);
        if (definition != null)
        {
            definition.Status = WorkflowDefinitionStatus.Archived;
            definition.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Archived workflow definition {Id}", id);
        }
    }

    public async Task<WorkflowDefinition> PublishAsync(
        int id, 
        int? userId = null,
        CancellationToken cancellationToken = default)
    {
        var definition = await _context.WorkflowDefinitions
            .Include(w => w.Steps)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
            
        if (definition == null)
        {
            throw new KeyNotFoundException($"Workflow definition {id} not found");
        }

        definition.Status = WorkflowDefinitionStatus.Published;
        definition.PublishedAt = DateTime.UtcNow;
        definition.UpdatedAt = DateTime.UtcNow;
        definition.VersionNumber++;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Published workflow definition {Id}", id);

        return definition;
    }
}

/// <summary>
/// Repository for WorkflowInstance entities
/// </summary>
public class WorkflowInstanceRepository : IWorkflowInstanceRepository
{
    private readonly CrmDbContext _context;
    private readonly ILogger<WorkflowInstanceRepository> _logger;

    public WorkflowInstanceRepository(
        CrmDbContext context,
        ILogger<WorkflowInstanceRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<WorkflowInstance?> GetByIdAsync(
        int id, 
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WorkflowInstances
            .Include(i => i.WorkflowDefinition)
            .ThenInclude(d => d!.Steps);

        if (includeDetails)
        {
            return await query
                .Include(i => i.ContextVariables)
                .Include(i => i.StartedByUser)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        }

        return await query.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<List<WorkflowInstance>> GetActiveInstancesAsync(
        int? limit = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.WorkflowInstances
            .Include(i => i.WorkflowDefinition)
            .Where(i => i.Status == WorkflowInstanceStatus.Running 
                || i.Status == WorkflowInstanceStatus.Pending
                || i.Status == WorkflowInstanceStatus.WaitingForInput)
            .OrderBy(i => i.Priority)
            .ThenBy(i => i.CreatedAt);

        if (limit.HasValue)
        {
            return await query.Take(limit.Value).ToListAsync(cancellationToken);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<WorkflowInstance>> GetByEntityAsync(
        string entityType, 
        int entityId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowInstances
            .Include(i => i.WorkflowDefinition)
            .Where(i => i.EntityType == entityType && i.EntityId == entityId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkflowInstance> CreateAsync(
        WorkflowInstance instance, 
        CancellationToken cancellationToken = default)
    {
        instance.CreatedAt = DateTime.UtcNow;
        instance.UpdatedAt = DateTime.UtcNow;

        _context.WorkflowInstances.Add(instance);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created workflow instance {Id} for definition {DefinitionId}",
            instance.Id, instance.WorkflowDefinitionId);

        return instance;
    }

    public async Task<WorkflowInstance> UpdateAsync(
        WorkflowInstance instance, 
        CancellationToken cancellationToken = default)
    {
        instance.UpdatedAt = DateTime.UtcNow;

        _context.WorkflowInstances.Update(instance);
        await _context.SaveChangesAsync(cancellationToken);

        return instance;
    }

    public async Task<bool> TryUpdateWithOptimisticLockAsync(
        WorkflowInstance instance, 
        CancellationToken cancellationToken = default)
    {
        instance.UpdatedAt = DateTime.UtcNow;
        instance.LockVersion++;

        try
        {
            _context.WorkflowInstances.Update(instance);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Concurrency conflict updating instance {Id}", instance.Id);
            return false;
        }
    }
}

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Workflow execution service - evaluates and executes workflow rules
/// </summary>
public interface IWorkflowService
{
    Task<bool> ExecuteWorkflowAsync(string entityType, int entityId, object entity, CancellationToken cancellationToken = default);
    Task<Workflow?> CreateWorkflowAsync(Workflow workflow, CancellationToken cancellationToken = default);
    Task<Workflow?> UpdateWorkflowAsync(int id, Workflow workflow, CancellationToken cancellationToken = default);
    Task<bool> DeleteWorkflowAsync(int id, CancellationToken cancellationToken = default);
    Task<Workflow?> GetWorkflowAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workflow>> GetWorkflowsAsync(string entityType, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowExecution>> GetExecutionHistoryAsync(int workflowId, CancellationToken cancellationToken = default);
}

public class WorkflowService : IWorkflowService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<WorkflowService> _logger;

    public WorkflowService(ICrmDbContext context, ILogger<WorkflowService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Executes workflows for an entity and transfers it to target user groups if rules match
    /// </summary>
    public async Task<bool> ExecuteWorkflowAsync(string entityType, int entityId, object entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var workflows = await _context.Workflows
                .Include(w => w.Rules)
                .ThenInclude(r => r.Conditions)
                .Include(w => w.Rules)
                .ThenInclude(r => r.TargetUserGroup)
                .Where(w => w.EntityType == entityType && w.IsActive)
                .OrderByDescending(w => w.Priority)
                .ToListAsync(cancellationToken);

            if (!workflows.Any())
            {
                _logger.LogInformation($"No workflows found for entity type: {entityType}");
                return false;
            }

            bool anyRuleMatched = false;

            foreach (var workflow in workflows)
            {
                var matchedRule = FindMatchingRule(workflow.Rules.ToList(), entity);

                if (matchedRule != null)
                {
                    anyRuleMatched = true;
                    _logger.LogInformation($"Workflow rule '{matchedRule.Name}' matched for {entityType} #{entityId}");

                    // Create execution history entry
                    var execution = new WorkflowExecution
                    {
                        WorkflowId = workflow.Id,
                        WorkflowRuleId = matchedRule.Id,
                        EntityType = entityType,
                        EntityId = entityId,
                        SourceUserGroupId = 1, // TODO: Get actual source group
                        TargetUserGroupId = matchedRule.TargetUserGroupId,
                        Status = "Success",
                        EntitySnapshotJson = SerializeEntity(entity),
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.WorkflowExecutions.Add(execution);
                }
            }

            if (anyRuleMatched)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return anyRuleMatched;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error executing workflow for {entityType} #{entityId}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Finds the first matching rule from a list of rules
    /// </summary>
    private WorkflowRule? FindMatchingRule(List<WorkflowRule> rules, object entity)
    {
        foreach (var rule in rules.Where(r => r.IsEnabled).OrderByDescending(r => r.Priority))
        {
            if (EvaluateRule(rule, entity))
            {
                return rule;
            }
        }

        return null;
    }

    /// <summary>
    /// Evaluates a workflow rule against an entity
    /// </summary>
    private bool EvaluateRule(WorkflowRule rule, object entity)
    {
        if (!rule.Conditions.Any())
        {
            return true; // No conditions means always match
        }

        var conditionResults = rule.Conditions
            .OrderBy(c => c.Priority)
            .Select(condition => EvaluateCondition(entity, condition))
            .ToList();

        if (rule.ConditionLogic.ToUpper() == "AND")
        {
            return conditionResults.All(r => r);
        }
        else // OR
        {
            return conditionResults.Any(r => r);
        }
    }

    /// <summary>
    /// Evaluates a single condition against an entity
    /// </summary>
    private bool EvaluateCondition(object entity, WorkflowRuleCondition condition)
    {
        try
        {
            var property = entity.GetType().GetProperty(condition.FieldName,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
            {
                _logger.LogWarning($"Property '{condition.FieldName}' not found on entity type");
                return false;
            }

            var value = property.GetValue(entity);
            var conditionValue = Convert.ChangeType(condition.Value, property.PropertyType);

            return EvaluateOperator(value, condition.Operator, conditionValue, condition.ValueTwo, property.PropertyType);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error evaluating condition: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Evaluates comparison operators
    /// </summary>
    private bool EvaluateOperator(object? value, string op, object? conditionValue, string? valueTwo, Type propertyType)
    {
        switch (op.ToUpper())
        {
            case "EQUALS":
            case "EQ":
                return Equals(value, conditionValue);

            case "NOTEQUALS":
            case "NEQ":
                return !Equals(value, conditionValue);

            case "GREATERTHAN":
            case "GT":
                return value != null && conditionValue != null &&
                    ((IComparable)value).CompareTo(conditionValue) > 0;

            case "LESSTHAN":
            case "LT":
                return value != null && conditionValue != null &&
                    ((IComparable)value).CompareTo(conditionValue) < 0;

            case "GREATERTHANOREQUAL":
            case "GTE":
                return value != null && conditionValue != null &&
                    ((IComparable)value).CompareTo(conditionValue) >= 0;

            case "LESSTHANOREQUAL":
            case "LTE":
                return value != null && conditionValue != null &&
                    ((IComparable)value).CompareTo(conditionValue) <= 0;

            case "CONTAINS":
                return value?.ToString()?.Contains(conditionValue?.ToString() ?? "") ?? false;

            case "IN":
                var values = conditionValue?.ToString()?.Split(',') ?? Array.Empty<string>();
                return values.Any(v => Equals(value?.ToString()?.Trim(), v.Trim()));

            case "BETWEEN":
                if (valueTwo == null || value == null || conditionValue == null)
                    return false;
                var min = Convert.ChangeType(conditionValue, propertyType);
                var max = Convert.ChangeType(valueTwo, propertyType);
                return ((IComparable)value).CompareTo(min) >= 0 &&
                       ((IComparable)value).CompareTo(max) <= 0;

            default:
                return false;
        }
    }

    /// <summary>
    /// Serializes an entity to JSON for audit trail
    /// </summary>
    private string SerializeEntity(object entity)
    {
        try
        {
            var result = new Dictionary<string, object>();
            var properties = entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                try
                {
                    var value = prop.GetValue(entity);
                    if (value != null && !prop.PropertyType.IsComplexType())
                    {
                        result[prop.Name] = value;
                    }
                }
                catch { }
            }

            return JsonSerializer.Serialize(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error serializing entity: {ex.Message}");
            return "{}";
        }
    }

    public async Task<Workflow?> CreateWorkflowAsync(Workflow workflow, CancellationToken cancellationToken = default)
    {
        _context.Workflows.Add(workflow);
        await _context.SaveChangesAsync(cancellationToken);
        return workflow;
    }

    public async Task<Workflow?> UpdateWorkflowAsync(int id, Workflow workflow, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Workflows.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        if (existing == null) return null;

        existing.Name = workflow.Name;
        existing.Description = workflow.Description;
        existing.IsActive = workflow.IsActive;
        existing.Priority = workflow.Priority;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteWorkflowAsync(int id, CancellationToken cancellationToken = default)
    {
        var workflow = await _context.Workflows.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        if (workflow == null) return false;

        _context.Workflows.Remove(workflow);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<Workflow?> GetWorkflowAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Workflows
            .Include(w => w.Rules)
            .ThenInclude(r => r.Conditions)
            .Include(w => w.Rules)
            .ThenInclude(r => r.TargetUserGroup)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Workflow>> GetWorkflowsAsync(string entityType, CancellationToken cancellationToken = default)
    {
        return await _context.Workflows
            .Include(w => w.Rules)
            .ThenInclude(r => r.Conditions)
            .Include(w => w.Rules)
            .ThenInclude(r => r.TargetUserGroup)
            .Where(w => w.EntityType == entityType)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowExecution>> GetExecutionHistoryAsync(int workflowId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowExecutions
            .Where(ex => ex.WorkflowId == workflowId)
            .OrderByDescending(ex => ex.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

public static class TypeExtensions
{
    public static bool IsComplexType(this Type type)
    {
        if (type.IsValueType || type == typeof(string))
            return false;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return false;

        return !type.IsPrimitive && !type.IsEnum && type != typeof(DateTime) && type != typeof(decimal);
    }
}

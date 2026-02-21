// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.DTOs.Workflow;

/// <summary>
/// Summary DTO for workflow definition list views
/// </summary>
public class WorkflowDefinitionDto
{
    public int Id { get; set; }
    public string WorkflowKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CurrentVersion { get; set; }
    public string? IconName { get; set; }
    public string? Color { get; set; }
    public bool IsSystem { get; set; }
    public int Priority { get; set; }
    public int MaxConcurrentInstances { get; set; }
    public int DefaultTimeoutHours { get; set; }
    public int? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public List<string>? Tags { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Detailed DTO for workflow definition with versions
/// </summary>
public class WorkflowDefinitionDetailDto : WorkflowDefinitionDto
{
    public string? Metadata { get; set; }
    public List<WorkflowVersionSummaryDto> Versions { get; set; } = new();
}

/// <summary>
/// Summary DTO for workflow version list views
/// </summary>
public class WorkflowVersionSummaryDto
{
    public int Id { get; set; }
    public int VersionNumber { get; set; }
    public string? Label { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Detailed DTO for workflow version with nodes and transitions
/// </summary>
public class WorkflowVersionDetailDto : WorkflowVersionSummaryDto
{
    public int WorkflowDefinitionId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public string? ChangeLog { get; set; }
    public string? PublishedByName { get; set; }
    public string? CanvasLayout { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<WorkflowNodeDto> Nodes { get; set; } = new();
    public List<WorkflowTransitionDto> Transitions { get; set; } = new();
}

/// <summary>
/// DTO for creating a new workflow definition
/// </summary>
public class CreateWorkflowDto
{
    public string WorkflowKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string? IconName { get; set; }
    public string? Color { get; set; }
    public int? Priority { get; set; }
    public int? MaxConcurrentInstances { get; set; }
    public int? DefaultTimeoutHours { get; set; }
    public List<string>? Tags { get; set; }
    public string? Metadata { get; set; }
}

/// <summary>
/// DTO for updating an existing workflow definition
/// </summary>
public class UpdateWorkflowDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? EntityType { get; set; }
    public string? IconName { get; set; }
    public string? Color { get; set; }
    public int? Priority { get; set; }
    public int? MaxConcurrentInstances { get; set; }
    public int? DefaultTimeoutHours { get; set; }
    public List<string>? Tags { get; set; }
    public string? Metadata { get; set; }
}

/// <summary>
/// DTO for creating a new workflow version
/// </summary>
public class CreateVersionDto
{
    public int? SourceVersionId { get; set; }
}

/// <summary>
/// DTO for saving workflow canvas layout
/// </summary>
public class SaveLayoutDto
{
    public string CanvasLayout { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating workflow version metadata
/// </summary>
public class UpdateVersionMetadataDto
{
    public string? Label { get; set; }
    public string? ChangeLog { get; set; }
}

/// <summary>
/// DTO for creating a new workflow node
/// </summary>
public class CreateNodeDto
{
    public string? NodeKey { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string NodeType { get; set; } = string.Empty;
    public string? NodeSubType { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public string? IconName { get; set; }
    public string? Color { get; set; }
    public bool IsStartNode { get; set; }
    public bool IsEndNode { get; set; }
    public string? Configuration { get; set; }
    public int? TimeoutMinutes { get; set; }
    public int? RetryCount { get; set; }
    public int? RetryDelaySeconds { get; set; }
    public bool? UseExponentialBackoff { get; set; }
    public int? ExecutionOrder { get; set; }
}

/// <summary>
/// DTO for updating an existing workflow node
/// </summary>
public class UpdateNodeDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? NodeType { get; set; }
    public string? NodeSubType { get; set; }
    public double? PositionX { get; set; }
    public double? PositionY { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public string? IconName { get; set; }
    public string? Color { get; set; }
    public bool? IsStartNode { get; set; }
    public bool? IsEndNode { get; set; }
    public string? Configuration { get; set; }
    public int? TimeoutMinutes { get; set; }
    public int? RetryCount { get; set; }
    public int? RetryDelaySeconds { get; set; }
    public bool? UseExponentialBackoff { get; set; }
    public int? ExecutionOrder { get; set; }
}

/// <summary>
/// DTO for updating node positions on the canvas
/// </summary>
public class NodePositionDto
{
    public int NodeId { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
}

/// <summary>
/// DTO for creating a new workflow transition
/// </summary>
public class CreateTransitionDto
{
    public int SourceNodeId { get; set; }
    public int TargetNodeId { get; set; }
    public string? TransitionKey { get; set; }
    public string? Label { get; set; }
    public string? Description { get; set; }
    public string? ConditionType { get; set; }
    public string? ConditionExpression { get; set; }
    public bool IsDefault { get; set; }
    public int? Priority { get; set; }
    public string? SourceHandle { get; set; }
    public string? TargetHandle { get; set; }
    public string? LineStyle { get; set; }
    public string? Color { get; set; }
    public string? AnimationStyle { get; set; }
}

/// <summary>
/// DTO for updating an existing workflow transition
/// </summary>
public class UpdateTransitionDto
{
    public string? Label { get; set; }
    public string? Description { get; set; }
    public string? ConditionType { get; set; }
    public string? ConditionExpression { get; set; }
    public bool? IsDefault { get; set; }
    public int? Priority { get; set; }
    public string? SourceHandle { get; set; }
    public string? TargetHandle { get; set; }
    public string? LineStyle { get; set; }
    public string? Color { get; set; }
    public string? AnimationStyle { get; set; }
}

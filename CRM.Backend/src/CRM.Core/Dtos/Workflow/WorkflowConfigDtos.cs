// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.DTOs.Workflow;

/// <summary>
/// Response model containing all workflow configuration metadata.
/// </summary>
public class WorkflowConfigResponse
{
    public List<ConfigOption> EntityTypes { get; set; } = new();
    public List<NodeTypeConfig> NodeTypes { get; set; } = new();
    public List<ActionTypeConfig> ActionTypes { get; set; } = new();
    public List<TriggerTypeConfig> TriggerTypes { get; set; } = new();
    public List<OperatorConfig> ConditionOperators { get; set; } = new();
    public List<StatusConfig> StatusOptions { get; set; } = new();
    public List<LLMProviderOption> LLMProviders { get; set; } = new();
    public List<LLMModelOption> LLMModels { get; set; } = new();
    public List<ConfigOption> Roles { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public List<string> IconOptions { get; set; } = new();
    public List<string> ColorOptions { get; set; } = new();
    public List<ConfigOption> FallbackActions { get; set; } = new();
    public List<EventTypeConfig> EventTypes { get; set; } = new();
    public Dictionary<string, List<EntityFieldConfig>> EntityFields { get; set; } = new();
    public Dictionary<string, List<RelatedEntityConfig>> RelatedEntities { get; set; } = new();
}

/// <summary>
/// LLM provider option (Core-layer equivalent of Infrastructure LLMProviderInfo).
/// </summary>
public class LLMProviderOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsConfigured { get; set; }
    public List<LLMModelOption> Models { get; set; } = new();
}

/// <summary>
/// LLM model option (Core-layer equivalent of Infrastructure LLMModelInfo).
/// </summary>
public class LLMModelOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

/// <summary>
/// Simple value/label configuration option used for entity types, roles, fallback actions.
/// </summary>
public class ConfigOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

/// <summary>
/// Workflow node type configuration with icon and color.
/// </summary>
public class NodeTypeConfig
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Workflow action type configuration with category and icon.
/// </summary>
public class ActionTypeConfig
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

/// <summary>
/// Workflow trigger type configuration with description and icon.
/// </summary>
public class TriggerTypeConfig
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

/// <summary>
/// Condition operator configuration indicating which field types it applies to.
/// </summary>
public class OperatorConfig
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string[] AppliesTo { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Status configuration with color theming.
/// </summary>
public class StatusConfig
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string BgColor { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

/// <summary>
/// Event type configuration with color and category.
/// </summary>
public class EventTypeConfig
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// Entity field configuration for workflow conditions and actions.
/// </summary>
public class EntityFieldConfig
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Required { get; set; }
    public List<string>? EnumValues { get; set; }
    public string? ReferenceEntity { get; set; }
    public string Group { get; set; } = string.Empty;
}

/// <summary>
/// Related entity configuration for workflow navigation.
/// </summary>
public class RelatedEntityConfig
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string RelationType { get; set; } = string.Empty;
}

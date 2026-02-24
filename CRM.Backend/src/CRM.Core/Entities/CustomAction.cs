// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

/// <summary>
/// Defines a custom button/action that can be placed on entity detail pages.
/// Supports URL navigation, API calls, and workflow triggers.
/// </summary>
public class CustomAction : BaseEntity
{
    /// <summary>
    /// Display label for the action button.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Internal unique name for the action (e.g., "send_welcome_email").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Entity type this action applies to (e.g., "Account", "Contact", "Opportunity").
    /// Null means the action is available globally.
    /// </summary>
    [MaxLength(50)]
    public string? EntityType { get; set; }

    /// <summary>
    /// The type of action to perform.
    /// </summary>
    public CustomActionType ActionType { get; set; } = CustomActionType.ApiCall;

    /// <summary>
    /// Target URL for navigation actions, or API endpoint for API call actions.
    /// Supports placeholders like {Id}, {Name}.
    /// </summary>
    [MaxLength(500)]
    public string? TargetUrl { get; set; }

    /// <summary>
    /// HTTP method for API call actions (GET, POST, PUT, DELETE).
    /// </summary>
    [MaxLength(10)]
    public string? HttpMethod { get; set; } = "POST";

    /// <summary>
    /// JSON request body template for API call actions.
    /// Supports placeholders like {Id}, {Name}.
    /// </summary>
    public string? RequestBodyTemplate { get; set; }

    /// <summary>
    /// Workflow definition ID to trigger (for workflow trigger actions).
    /// </summary>
    public int? WorkflowDefinitionId { get; set; }

    /// <summary>
    /// MUI icon name to display on the button (e.g., "Send", "Email", "Delete").
    /// </summary>
    [MaxLength(50)]
    public string? IconName { get; set; }

    /// <summary>
    /// Button color variant (primary, secondary, error, warning, info, success).
    /// </summary>
    [MaxLength(20)]
    public string Color { get; set; } = "primary";

    /// <summary>
    /// Button variant (contained, outlined, text).
    /// </summary>
    [MaxLength(20)]
    public string Variant { get; set; } = "contained";

    /// <summary>
    /// Display order for sorting actions.
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Confirmation message to show before executing. Null means no confirmation.
    /// </summary>
    [MaxLength(500)]
    public string? ConfirmationMessage { get; set; }

    /// <summary>
    /// Success message to display after execution.
    /// </summary>
    [MaxLength(500)]
    public string? SuccessMessage { get; set; }

    /// <summary>
    /// Whether the action is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Required permission to see/execute this action (e.g., "Admin", "SalesManager").
    /// Null means no permission restriction.
    /// </summary>
    [MaxLength(100)]
    public string? RequiredPermission { get; set; }

    /// <summary>
    /// Condition expression that must be true for the action to be visible.
    /// e.g., "{Status} == 'Open'" or "{Amount} > 1000".
    /// </summary>
    [MaxLength(1000)]
    public string? VisibilityCondition { get; set; }
}

/// <summary>
/// Types of custom actions.
/// </summary>
public enum CustomActionType
{
    /// <summary>Navigate to a URL.</summary>
    Navigation = 0,

    /// <summary>Make an API call.</summary>
    ApiCall = 1,

    /// <summary>Trigger a workflow.</summary>
    WorkflowTrigger = 2,

    /// <summary>Open a dialog/modal with a form.</summary>
    DialogForm = 3,

    /// <summary>Execute client-side JavaScript.</summary>
    ClientScript = 4
}

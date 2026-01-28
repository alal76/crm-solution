// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities.Workflow;

namespace CRM.Core.Entities;

/// <summary>
/// Workflow type for campaigns
/// </summary>
public enum CampaignWorkflowType
{
    TriggerBased = 0,
    Scheduled = 1,
    Sequential = 2
}

/// <summary>
/// Links campaigns to workflow definitions for automated execution
/// </summary>
public class CampaignWorkflow : BaseEntity
{
    /// <summary>
    /// The campaign
    /// </summary>
    [Required]
    public int CampaignId { get; set; }
    
    /// <summary>
    /// The workflow definition to use
    /// </summary>
    [Required]
    public int WorkflowDefinitionId { get; set; }
    
    /// <summary>
    /// Type of workflow execution
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string WorkflowType { get; set; } = "Sequential";
    
    /// <summary>
    /// Trigger event (for trigger-based workflows)
    /// </summary>
    [MaxLength(100)]
    public string? TriggerEvent { get; set; }
    
    /// <summary>
    /// Trigger conditions as JSON
    /// </summary>
    public string? TriggerConditions { get; set; }
    
    /// <summary>
    /// Whether the workflow is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Execution priority (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 0;
    
    /// <summary>
    /// Maximum executions per contact
    /// </summary>
    public int MaxExecutionsPerContact { get; set; } = 1;
    
    /// <summary>
    /// Minimum hours between executions for same contact
    /// </summary>
    public int CooldownHours { get; set; } = 0;
    
    // Navigation properties
    public virtual MarketingCampaign? Campaign { get; set; }
    public virtual WorkflowDefinition? WorkflowDefinition { get; set; }
}

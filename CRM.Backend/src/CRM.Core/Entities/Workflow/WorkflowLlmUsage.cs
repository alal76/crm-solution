// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities.Workflow;

/// <summary>
/// Tracks token usage and cost for LLM calls made by workflow nodes.
/// </summary>
public class WorkflowLlmUsage : BaseEntity
{
    /// <summary>
    /// Foreign key to the workflow instance that made the LLM call.
    /// </summary>
    public int WorkflowInstanceId { get; set; }

    /// <summary>
    /// Navigation property to the workflow instance.
    /// </summary>
    public virtual WorkflowInstance WorkflowInstance { get; set; } = null!;

    /// <summary>
    /// Optional foreign key to the specific node instance that invoked the LLM.
    /// </summary>
    public int? NodeInstanceId { get; set; }

    /// <summary>
    /// Navigation property to the node instance.
    /// </summary>
    public virtual WorkflowNodeInstance? NodeInstance { get; set; }

    /// <summary>
    /// LLM provider name (e.g., "OpenAI", "Ollama", "AzureOpenAI", "Anthropic").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Model identifier used (e.g., "gpt-4o", "llama3", "claude-3-sonnet").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Number of tokens in the input prompt.
    /// </summary>
    public int PromptTokens { get; set; }

    /// <summary>
    /// Number of tokens in the model's completion.
    /// </summary>
    public int CompletionTokens { get; set; }

    /// <summary>
    /// Total tokens consumed (prompt + completion).
    /// </summary>
    public int TotalTokens { get; set; }

    /// <summary>
    /// Estimated monetary cost of this LLM call.
    /// </summary>
    public decimal CostEstimate { get; set; }

    /// <summary>
    /// Round-trip latency of the LLM call in milliseconds.
    /// </summary>
    public int LatencyMs { get; set; }

    /// <summary>
    /// Whether the LLM call completed successfully.
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Error message if the LLM call failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

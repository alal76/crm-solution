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

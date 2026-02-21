// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Tracks AI agent usage statistics per user per day.
/// </summary>
[Table("AIAgentUsages")]
public class AIAgentUsage : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string AgentId { get; set; } = string.Empty;

    public int? UserId { get; set; }

    public int RequestCount { get; set; }

    public int Tokens { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal Cost { get; set; }

    public DateTime UsageDate { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}

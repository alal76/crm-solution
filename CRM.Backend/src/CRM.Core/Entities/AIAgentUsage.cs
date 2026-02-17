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

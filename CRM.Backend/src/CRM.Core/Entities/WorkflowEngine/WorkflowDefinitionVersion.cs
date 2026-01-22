using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.WorkflowEngine;

/// <summary>
/// Version history for workflow definitions - supports rollback and audit
/// </summary>
public class WorkflowDefinitionVersion : BaseEntity
{
    public int WorkflowDefinitionId { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Version { get; set; } = string.Empty;
    
    /// <summary>
    /// Complete snapshot of workflow definition as JSON
    /// </summary>
    [Required]
    [Column(TypeName = "longtext")]
    public string DefinitionSnapshot { get; set; } = string.Empty;
    
    /// <summary>
    /// Change description/notes
    /// </summary>
    [MaxLength(1000)]
    public string? ChangeNotes { get; set; }
    
    /// <summary>
    /// User who created this version
    /// </summary>
    public int? CreatedByUserId { get; set; }
    
    /// <summary>
    /// Whether this version was published
    /// </summary>
    public bool WasPublished { get; set; } = false;
    
    // Navigation properties
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public virtual User? CreatedByUser { get; set; }
}

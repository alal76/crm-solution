using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.ITSM;

/// <summary>
/// Represents an incident category for ITSM incident classification.
/// </summary>
[Table("IncidentCategories")]
public class IncidentCategory : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? SubCategory { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public int DefaultPriority { get; set; } = 3;

    public bool IsActive { get; set; } = true;
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Represents a data export job request.
/// </summary>
[Table("ExportJobs")]
public class ExportJob : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Entity { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Destination { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    public int? RequestedByUserId { get; set; }

    public DateTime? RequestedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public int? TotalRecords { get; set; }

    [StringLength(500)]
    public string? ErrorMessage { get; set; }

    [ForeignKey("RequestedByUserId")]
    public virtual User? RequestedByUser { get; set; }
}

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for export job response.
/// </summary>
public class ExportJobDto
{
    public int Id { get; set; }
    public string Entity { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? RequestedByUserId { get; set; }
    public DateTime? RequestedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int? TotalRecords { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating an export job.
/// </summary>
public class CreateExportJobDto
{
    public string Entity { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string? Status { get; set; }
    public int? RequestedByUserId { get; set; }
    public string? RequestedDate { get; set; }
}

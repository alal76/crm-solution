namespace CRM.Core.Dtos;

/// <summary>
/// DTO for import job response.
/// </summary>
public class ImportJobDto
{
    public int Id { get; set; }
    public string Entity { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? SubmittedByUserId { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int? TotalRecords { get; set; }
    public int? SuccessCount { get; set; }
    public int? FailureCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating an import job.
/// </summary>
public class CreateImportJobDto
{
    public string Entity { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? Status { get; set; }
    public int? SubmittedByUserId { get; set; }
    public string? SubmittedDate { get; set; }
}

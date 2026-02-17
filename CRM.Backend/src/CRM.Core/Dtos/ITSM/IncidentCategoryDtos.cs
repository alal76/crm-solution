namespace CRM.Core.Dtos.ITSM;

/// <summary>
/// DTO for incident category response.
/// </summary>
public class IncidentCategoryDto
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? SubCategory { get; set; }
    public string? Description { get; set; }
    public int DefaultPriority { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating an incident category.
/// </summary>
public class CreateIncidentCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string? SubCategory { get; set; }
    public string? Description { get; set; }
    public int DefaultPriority { get; set; } = 3;
    public bool IsActive { get; set; } = true;
}

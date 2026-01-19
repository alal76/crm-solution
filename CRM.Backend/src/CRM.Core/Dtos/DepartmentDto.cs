namespace CRM.Core.Dtos;

/// <summary>
/// DTO for Department creation and updates
/// </summary>
public class CreateDepartmentDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DepartmentCode { get; set; }
    public int? ParentDepartmentId { get; set; }
}

/// <summary>
/// DTO for Department responses
/// </summary>
public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DepartmentCode { get; set; }
    public bool IsActive { get; set; }
    public int? ParentDepartmentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserCount { get; set; }
}

/// <summary>
/// DTO for Department with details
/// </summary>
public class DepartmentDetailDto : DepartmentDto
{
    public List<UserDto> Users { get; set; } = new();
    public List<UserProfileDto> Profiles { get; set; } = new();
}

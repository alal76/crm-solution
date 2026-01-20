namespace CRM.Core.Dtos;

/// <summary>
/// DTO for assigning user profile and department
/// </summary>
public class AssignProfileDto
{
    public int UserProfileId { get; set; }
    public int? DepartmentId { get; set; }
}

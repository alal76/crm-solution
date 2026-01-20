namespace CRM.Core.Dtos;

/// <summary>
/// DTO for updating user information
/// </summary>
public class UpdateUserDto
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? Role { get; set; }
    public bool? IsActive { get; set; }
    public int? DepartmentId { get; set; }
    public int? UserProfileId { get; set; }
}

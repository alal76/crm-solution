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
    public int? ContactId { get; set; }
    public int? PrimaryGroupId { get; set; }
}

/// <summary>
/// DTO for linking/unlinking user to contact
/// </summary>
public class LinkUserContactDto
{
    public int? ContactId { get; set; }
}

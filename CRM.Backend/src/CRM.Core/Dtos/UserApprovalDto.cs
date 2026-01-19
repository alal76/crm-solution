namespace CRM.Core.Dtos;

/// <summary>
/// DTO for user approval requests
/// </summary>
public class UserApprovalRequestDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Phone { get; set; }
    public int Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByUserName { get; set; }
    public string? RejectionReason { get; set; }
}

/// <summary>
/// DTO for approving user registration
/// </summary>
public class ApproveUserRequest
{
    public int ApprovalRequestId { get; set; }
    public string? AssignedRole { get; set; } = "Sales";
    public int? DepartmentId { get; set; }
    public int? UserProfileId { get; set; }
}

/// <summary>
/// DTO for rejecting user registration
/// </summary>
public class RejectUserRequest
{
    public int ApprovalRequestId { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}

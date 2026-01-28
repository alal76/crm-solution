namespace CRM.Core.Entities;

/// <summary>
/// Approval status for new user registrations
/// </summary>
public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

/// <summary>
/// User approval request for managing new user sign-ups
/// </summary>
public class UserApprovalRequest : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Phone { get; set; }
    public string? PasswordHash { get; set; } // Store password hash for use when approved
    public int Status { get; set; } = (int)ApprovalStatus.Pending;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByUserId { get; set; }
    public string? RejectionReason { get; set; }
    public int? AssignedUserId { get; set; } // User created after approval

    // Navigation properties
    public virtual User? ReviewedByUser { get; set; }
    public virtual User? AssignedUser { get; set; }
}

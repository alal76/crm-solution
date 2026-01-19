namespace CRM.Core.Dtos;

/// <summary>
/// DTO for authentication response with JWT token
/// </summary>
public class AuthResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }

    // Profile and Department Information
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? UserProfileId { get; set; }
    public string? UserProfileName { get; set; }
    public List<string> AccessiblePages { get; set; } = new();
    public UserPermissions Permissions { get; set; } = new();
}

/// <summary>
/// User permissions object
/// </summary>
public class UserPermissions
{
    public bool CanCreateCustomers { get; set; }
    public bool CanEditCustomers { get; set; }
    public bool CanDeleteCustomers { get; set; }

    public bool CanCreateOpportunities { get; set; }
    public bool CanEditOpportunities { get; set; }
    public bool CanDeleteOpportunities { get; set; }

    public bool CanCreateProducts { get; set; }
    public bool CanEditProducts { get; set; }
    public bool CanDeleteProducts { get; set; }

    public bool CanManageCampaigns { get; set; }
    public bool CanViewReports { get; set; }
    public bool CanManageUsers { get; set; }
}

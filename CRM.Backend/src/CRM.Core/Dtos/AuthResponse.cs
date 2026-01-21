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
    public int? PrimaryGroupId { get; set; }
    public string? PrimaryGroupName { get; set; }
    public List<string> AccessiblePages { get; set; } = new();
    public UserPermissions Permissions { get; set; } = new();
    public GroupPermissionsDto? GroupPermissions { get; set; }
    
    /// <summary>
    /// Custom header color for this user (hex format)
    /// </summary>
    public string? HeaderColor { get; set; }
    
    /// <summary>
    /// URL to user's profile photo
    /// </summary>
    public string? PhotoUrl { get; set; }
    
    /// <summary>
    /// Whether 2FA verification is required to complete login
    /// </summary>
    public bool RequiresTwoFactor { get; set; } = false;
    
    /// <summary>
    /// Whether 2FA is enabled for this user (for profile display)
    /// </summary>
    public bool TwoFactorEnabled { get; set; } = false;
    
    /// <summary>
    /// Temporary token for 2FA verification (only set when RequiresTwoFactor is true)
    /// </summary>
    public string? TwoFactorToken { get; set; }
}

/// <summary>
/// User permissions object (legacy profile-based permissions)
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

/// <summary>
/// Group-based permissions for menu access and CRUD operations
/// </summary>
public class GroupPermissionsDto
{
    public bool IsSystemAdmin { get; set; }
    
    // Menu Access
    public bool CanAccessDashboard { get; set; } = true;
    public bool CanAccessCustomers { get; set; }
    public bool CanAccessContacts { get; set; }
    public bool CanAccessLeads { get; set; }
    public bool CanAccessOpportunities { get; set; }
    public bool CanAccessProducts { get; set; }
    public bool CanAccessServices { get; set; }
    public bool CanAccessCampaigns { get; set; }
    public bool CanAccessQuotes { get; set; }
    public bool CanAccessTasks { get; set; }
    public bool CanAccessActivities { get; set; }
    public bool CanAccessNotes { get; set; }
    public bool CanAccessWorkflows { get; set; }
    public bool CanAccessServiceRequests { get; set; }
    public bool CanAccessReports { get; set; }
    public bool CanAccessSettings { get; set; }
    public bool CanAccessUserManagement { get; set; }
    
    // CRUD Permissions
    public bool CanCreateCustomers { get; set; }
    public bool CanEditCustomers { get; set; }
    public bool CanDeleteCustomers { get; set; }
    public bool CanViewAllCustomers { get; set; }
    
    public bool CanCreateContacts { get; set; }
    public bool CanEditContacts { get; set; }
    public bool CanDeleteContacts { get; set; }
    
    public bool CanCreateLeads { get; set; }
    public bool CanEditLeads { get; set; }
    public bool CanDeleteLeads { get; set; }
    public bool CanConvertLeads { get; set; }
    
    public bool CanCreateOpportunities { get; set; }
    public bool CanEditOpportunities { get; set; }
    public bool CanDeleteOpportunities { get; set; }
    public bool CanCloseOpportunities { get; set; }
    
    public bool CanCreateProducts { get; set; }
    public bool CanEditProducts { get; set; }
    public bool CanDeleteProducts { get; set; }
    public bool CanManagePricing { get; set; }
    
    public bool CanCreateCampaigns { get; set; }
    public bool CanEditCampaigns { get; set; }
    public bool CanDeleteCampaigns { get; set; }
    public bool CanLaunchCampaigns { get; set; }
    
    public bool CanCreateQuotes { get; set; }
    public bool CanEditQuotes { get; set; }
    public bool CanDeleteQuotes { get; set; }
    public bool CanApproveQuotes { get; set; }
    
    public bool CanCreateTasks { get; set; }
    public bool CanEditTasks { get; set; }
    public bool CanDeleteTasks { get; set; }
    public bool CanAssignTasks { get; set; }
    
    public bool CanCreateWorkflows { get; set; }
    public bool CanEditWorkflows { get; set; }
    public bool CanDeleteWorkflows { get; set; }
    public bool CanActivateWorkflows { get; set; }
    
    // Data Access
    public string DataAccessScope { get; set; } = "own";
    public bool CanExportData { get; set; }
    public bool CanImportData { get; set; }
    public bool CanBulkEdit { get; set; }
    public bool CanBulkDelete { get; set; }
}

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for user group operations with permissions
/// </summary>
public class UserGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsSystemAdmin { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
    
    // Menu Permissions
    public bool CanAccessDashboard { get; set; }
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
    public bool CanAccessReports { get; set; }
    public bool CanAccessSettings { get; set; }
    public bool CanAccessUserManagement { get; set; }
    
    // Entity CRUD Permissions
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
    
    // Data Access Scope
    public string DataAccessScope { get; set; } = "own";
    public bool CanExportData { get; set; }
    public bool CanImportData { get; set; }
    public bool CanBulkEdit { get; set; }
    public bool CanBulkDelete { get; set; }
}

/// <summary>
/// DTO for creating/updating user groups
/// </summary>
public class CreateUserGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public bool IsSystemAdmin { get; set; } = false;
    
    // Menu Permissions
    public bool CanAccessDashboard { get; set; } = true;
    public bool CanAccessCustomers { get; set; } = false;
    public bool CanAccessContacts { get; set; } = false;
    public bool CanAccessLeads { get; set; } = false;
    public bool CanAccessOpportunities { get; set; } = false;
    public bool CanAccessProducts { get; set; } = false;
    public bool CanAccessServices { get; set; } = false;
    public bool CanAccessCampaigns { get; set; } = false;
    public bool CanAccessQuotes { get; set; } = false;
    public bool CanAccessTasks { get; set; } = false;
    public bool CanAccessActivities { get; set; } = false;
    public bool CanAccessNotes { get; set; } = false;
    public bool CanAccessWorkflows { get; set; } = false;
    public bool CanAccessReports { get; set; } = false;
    public bool CanAccessSettings { get; set; } = false;
    public bool CanAccessUserManagement { get; set; } = false;
    
    // Entity CRUD Permissions
    public bool CanCreateCustomers { get; set; } = false;
    public bool CanEditCustomers { get; set; } = false;
    public bool CanDeleteCustomers { get; set; } = false;
    public bool CanViewAllCustomers { get; set; } = false;
    
    public bool CanCreateContacts { get; set; } = false;
    public bool CanEditContacts { get; set; } = false;
    public bool CanDeleteContacts { get; set; } = false;
    
    public bool CanCreateLeads { get; set; } = false;
    public bool CanEditLeads { get; set; } = false;
    public bool CanDeleteLeads { get; set; } = false;
    public bool CanConvertLeads { get; set; } = false;
    
    public bool CanCreateOpportunities { get; set; } = false;
    public bool CanEditOpportunities { get; set; } = false;
    public bool CanDeleteOpportunities { get; set; } = false;
    public bool CanCloseOpportunities { get; set; } = false;
    
    public bool CanCreateProducts { get; set; } = false;
    public bool CanEditProducts { get; set; } = false;
    public bool CanDeleteProducts { get; set; } = false;
    public bool CanManagePricing { get; set; } = false;
    
    public bool CanCreateCampaigns { get; set; } = false;
    public bool CanEditCampaigns { get; set; } = false;
    public bool CanDeleteCampaigns { get; set; } = false;
    public bool CanLaunchCampaigns { get; set; } = false;
    
    public bool CanCreateQuotes { get; set; } = false;
    public bool CanEditQuotes { get; set; } = false;
    public bool CanDeleteQuotes { get; set; } = false;
    public bool CanApproveQuotes { get; set; } = false;
    
    public bool CanCreateTasks { get; set; } = false;
    public bool CanEditTasks { get; set; } = false;
    public bool CanDeleteTasks { get; set; } = false;
    public bool CanAssignTasks { get; set; } = false;
    
    public bool CanCreateWorkflows { get; set; } = false;
    public bool CanEditWorkflows { get; set; } = false;
    public bool CanDeleteWorkflows { get; set; } = false;
    public bool CanActivateWorkflows { get; set; } = false;
    
    // Data Access Scope
    public string DataAccessScope { get; set; } = "own";
    public bool CanExportData { get; set; } = false;
    public bool CanImportData { get; set; } = false;
    public bool CanBulkEdit { get; set; } = false;
    public bool CanBulkDelete { get; set; } = false;
}

/// <summary>
/// DTO for group member information
/// </summary>
public class UserGroupMemberDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}

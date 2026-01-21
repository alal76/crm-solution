namespace CRM.Core.Entities;

/// <summary>
/// FUNCTIONAL VIEW:
/// ================
/// User groups for organizing users and managing permissions collectively.
/// Each user must belong to at least one group.
/// Groups define which menu items, pages, and features users can access.
/// Admin groups have full system access regardless of individual permission settings.
/// 
/// PERMISSION HIERARCHY:
/// - System Admin Group: Full access to everything (IsSystemAdmin = true)
/// - Custom Groups: Access defined by MenuPermissions and EntityPermissions
/// 
/// TECHNICAL VIEW:
/// ===============
/// Menu and entity permissions stored as JSON arrays for flexibility.
/// Permission checks performed in middleware and frontend navigation.
/// </summary>
public class UserGroup : BaseEntity
{
    #region Basic Information
    
    /// <summary>
    /// Unique name of the group (e.g., "Sales Team", "Administrators")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the group's purpose
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this group is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Whether this is the default group for new users
    /// </summary>
    public bool IsDefault { get; set; } = false;
    
    /// <summary>
    /// Display order for sorting groups
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// Custom header/navigation bar color for this group (hex format)
    /// When UseGroupHeaderColor is enabled in SystemSettings, this color will be used
    /// </summary>
    public string HeaderColor { get; set; } = "#6750A4";
    
    #endregion
    
    #region System Admin Flag
    
    /// <summary>
    /// FUNCTIONAL: If true, members have full system access (Admin group)
    /// TECHNICAL: Bypasses all permission checks
    /// </summary>
    public bool IsSystemAdmin { get; set; } = false;
    
    #endregion
    
    #region Menu/Navigation Permissions
    
    /// <summary>
    /// FUNCTIONAL: List of menu items this group can access
    /// TECHNICAL: JSON array of menu identifiers
    /// Example: ["Dashboard", "Customers", "Contacts", "Opportunities"]
    /// </summary>
    public string AccessibleMenuItems { get; set; } = "[]";
    
    /// <summary>
    /// FUNCTIONAL: Whether members can see the Dashboard
    /// </summary>
    public bool CanAccessDashboard { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Whether members can see Customers
    /// </summary>
    public bool CanAccessCustomers { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Whether members can see Contacts
    /// </summary>
    public bool CanAccessContacts { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Whether members can see Leads
    /// </summary>
    public bool CanAccessLeads { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Whether members can see Opportunities
    /// </summary>
    public bool CanAccessOpportunities { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Whether members can see Products
    /// </summary>
    public bool CanAccessProducts { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Whether members can see Services
    /// </summary>
    public bool CanAccessServices { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Whether members can see Campaigns
    /// </summary>
    public bool CanAccessCampaigns { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Whether members can see Quotes
    /// </summary>
    public bool CanAccessQuotes { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Whether members can see Tasks
    /// </summary>
    public bool CanAccessTasks { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Whether members can see Activities
    /// </summary>
    public bool CanAccessActivities { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Whether members can see Notes
    /// </summary>
    public bool CanAccessNotes { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Whether members can see Workflows
    /// </summary>
    public bool CanAccessWorkflows { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Whether members can see Service Requests
    /// </summary>
    public bool CanAccessServiceRequests { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Whether members can see Reports
    /// </summary>
    public bool CanAccessReports { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Whether members can access Admin Settings
    /// </summary>
    public bool CanAccessSettings { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Whether members can access User Management
    /// </summary>
    public bool CanAccessUserManagement { get; set; } = false;
    
    #endregion
    
    #region Entity CRUD Permissions
    
    // Customer Permissions
    public bool CanCreateCustomers { get; set; } = false;
    public bool CanEditCustomers { get; set; } = false;
    public bool CanDeleteCustomers { get; set; } = false;
    public bool CanViewAllCustomers { get; set; } = false; // vs. only assigned
    
    // Contact Permissions
    public bool CanCreateContacts { get; set; } = false;
    public bool CanEditContacts { get; set; } = false;
    public bool CanDeleteContacts { get; set; } = false;
    
    // Lead Permissions
    public bool CanCreateLeads { get; set; } = false;
    public bool CanEditLeads { get; set; } = false;
    public bool CanDeleteLeads { get; set; } = false;
    public bool CanConvertLeads { get; set; } = false;
    
    // Opportunity Permissions
    public bool CanCreateOpportunities { get; set; } = false;
    public bool CanEditOpportunities { get; set; } = false;
    public bool CanDeleteOpportunities { get; set; } = false;
    public bool CanCloseOpportunities { get; set; } = false;
    
    // Product Permissions
    public bool CanCreateProducts { get; set; } = false;
    public bool CanEditProducts { get; set; } = false;
    public bool CanDeleteProducts { get; set; } = false;
    public bool CanManagePricing { get; set; } = false;
    
    // Campaign Permissions
    public bool CanCreateCampaigns { get; set; } = false;
    public bool CanEditCampaigns { get; set; } = false;
    public bool CanDeleteCampaigns { get; set; } = false;
    public bool CanLaunchCampaigns { get; set; } = false;
    
    // Quote Permissions
    public bool CanCreateQuotes { get; set; } = false;
    public bool CanEditQuotes { get; set; } = false;
    public bool CanDeleteQuotes { get; set; } = false;
    public bool CanApproveQuotes { get; set; } = false;
    
    // Task Permissions
    public bool CanCreateTasks { get; set; } = false;
    public bool CanEditTasks { get; set; } = false;
    public bool CanDeleteTasks { get; set; } = false;
    public bool CanAssignTasks { get; set; } = false;
    
    // Workflow Permissions
    public bool CanCreateWorkflows { get; set; } = false;
    public bool CanEditWorkflows { get; set; } = false;
    public bool CanDeleteWorkflows { get; set; } = false;
    public bool CanActivateWorkflows { get; set; } = false;
    
    #endregion
    
    #region Data Access Scope
    
    /// <summary>
    /// FUNCTIONAL: Data visibility scope
    /// TECHNICAL: own = only user's records, team = department records, all = everything
    /// </summary>
    public string DataAccessScope { get; set; } = "own"; // own, team, all
    
    /// <summary>
    /// Whether members can export data
    /// </summary>
    public bool CanExportData { get; set; } = false;
    
    /// <summary>
    /// Whether members can import data
    /// </summary>
    public bool CanImportData { get; set; } = false;
    
    /// <summary>
    /// Whether members can bulk edit records
    /// </summary>
    public bool CanBulkEdit { get; set; } = false;
    
    /// <summary>
    /// Whether members can bulk delete records
    /// </summary>
    public bool CanBulkDelete { get; set; } = false;
    
    #endregion
    
    #region Navigation Properties
    
    /// <summary>
    /// Members of this group (via UserGroupMember junction)
    /// </summary>
    public virtual ICollection<UserGroupMember> Members { get; set; } = new List<UserGroupMember>();
    
    /// <summary>
    /// Users who have this as their primary group
    /// </summary>
    public virtual ICollection<User> PrimaryUsers { get; set; } = new List<User>();
    
    #endregion
}

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for system settings read/update operations
/// </summary>
public class SystemSettingsDto
{
    public int Id { get; set; }
    
    #region Module Enable/Disable
    
    public bool CustomersEnabled { get; set; }
    public bool ContactsEnabled { get; set; }
    public bool LeadsEnabled { get; set; }
    public bool OpportunitiesEnabled { get; set; }
    public bool ProductsEnabled { get; set; }
    public bool ServicesEnabled { get; set; }
    public bool CampaignsEnabled { get; set; }
    public bool QuotesEnabled { get; set; }
    public bool TasksEnabled { get; set; }
    public bool ActivitiesEnabled { get; set; }
    public bool NotesEnabled { get; set; }
    public bool WorkflowsEnabled { get; set; }
    public bool ReportsEnabled { get; set; }
    public bool DashboardEnabled { get; set; }
    
    #endregion
    
    #region Company/Branding
    
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyLogoUrl { get; set; }
    public string PrimaryColor { get; set; } = "#1976d2";
    public string SecondaryColor { get; set; } = "#dc004e";
    
    #endregion
    
    #region Security Settings
    
    public bool RequireTwoFactor { get; set; }
    public int MinPasswordLength { get; set; }
    public int SessionTimeoutMinutes { get; set; }
    public bool AllowUserRegistration { get; set; }
    public bool RequireApprovalForNewUsers { get; set; }
    
    #endregion
    
    #region Feature Flags
    
    public bool ApiAccessEnabled { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public bool AuditLoggingEnabled { get; set; }
    
    #endregion
    
    #region Customization
    
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string TimeFormat { get; set; } = "HH:mm:ss";
    public string DefaultCurrency { get; set; } = "USD";
    public string DefaultTimezone { get; set; } = "UTC";
    public string DefaultLanguage { get; set; } = "en";
    
    #endregion
    
    public DateTime LastModified { get; set; }
    public int? ModifiedByUserId { get; set; }
}

/// <summary>
/// DTO for updating system settings - allows partial updates
/// </summary>
public class UpdateSystemSettingsRequest
{
    // Module Enable/Disable
    public bool? CustomersEnabled { get; set; }
    public bool? ContactsEnabled { get; set; }
    public bool? LeadsEnabled { get; set; }
    public bool? OpportunitiesEnabled { get; set; }
    public bool? ProductsEnabled { get; set; }
    public bool? ServicesEnabled { get; set; }
    public bool? CampaignsEnabled { get; set; }
    public bool? QuotesEnabled { get; set; }
    public bool? TasksEnabled { get; set; }
    public bool? ActivitiesEnabled { get; set; }
    public bool? NotesEnabled { get; set; }
    public bool? WorkflowsEnabled { get; set; }
    public bool? ReportsEnabled { get; set; }
    public bool? DashboardEnabled { get; set; }
    
    // Company/Branding
    public string? CompanyName { get; set; }
    public string? CompanyLogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    
    // Security Settings
    public bool? RequireTwoFactor { get; set; }
    public int? MinPasswordLength { get; set; }
    public int? SessionTimeoutMinutes { get; set; }
    public bool? AllowUserRegistration { get; set; }
    public bool? RequireApprovalForNewUsers { get; set; }
    
    // Feature Flags
    public bool? ApiAccessEnabled { get; set; }
    public bool? EmailNotificationsEnabled { get; set; }
    public bool? AuditLoggingEnabled { get; set; }
    
    // Customization
    public string? DateFormat { get; set; }
    public string? TimeFormat { get; set; }
    public string? DefaultCurrency { get; set; }
    public string? DefaultTimezone { get; set; }
    public string? DefaultLanguage { get; set; }
}

/// <summary>
/// Minimal DTO for frontend permission checking - what modules are enabled globally
/// </summary>
public class ModuleStatusDto
{
    public bool CustomersEnabled { get; set; }
    public bool ContactsEnabled { get; set; }
    public bool LeadsEnabled { get; set; }
    public bool OpportunitiesEnabled { get; set; }
    public bool ProductsEnabled { get; set; }
    public bool ServicesEnabled { get; set; }
    public bool CampaignsEnabled { get; set; }
    public bool QuotesEnabled { get; set; }
    public bool TasksEnabled { get; set; }
    public bool ActivitiesEnabled { get; set; }
    public bool NotesEnabled { get; set; }
    public bool WorkflowsEnabled { get; set; }
    public bool ReportsEnabled { get; set; }
    public bool DashboardEnabled { get; set; }
}

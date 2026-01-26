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
    public bool EmailEnabled { get; set; }
    public bool WhatsAppEnabled { get; set; }
    public bool SocialMediaEnabled { get; set; }
    
    #endregion
    
    #region Company/Branding
    
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyLogoUrl { get; set; }
    public string PrimaryColor { get; set; } = "#6750A4";
    public string SecondaryColor { get; set; } = "#625B71";
    public string TertiaryColor { get; set; } = "#7D5260";
    public string SurfaceColor { get; set; } = "#FFFBFE";
    public string BackgroundColor { get; set; } = "#FFFBFE";
    public bool UseGroupHeaderColor { get; set; } = false;
    public int? SelectedPaletteId { get; set; }
    public string? SelectedPaletteName { get; set; }
    public DateTime? PalettesLastRefreshed { get; set; }
    
    #endregion
    
    #region Security Settings
    
    public bool RequireTwoFactor { get; set; }
    public int MinPasswordLength { get; set; }
    public int SessionTimeoutMinutes { get; set; }
    public bool AllowUserRegistration { get; set; }
    public bool RequireApprovalForNewUsers { get; set; }
    public bool QuickAdminLoginEnabled { get; set; }
    
    #endregion
    
    #region Social Login - Google
    
    public bool GoogleAuthEnabled { get; set; }
    public string? GoogleClientId { get; set; }
    public string? GoogleClientSecret { get; set; }
    
    #endregion
    
    #region Social Login - Microsoft Account
    
    public bool MicrosoftAuthEnabled { get; set; }
    public string? MicrosoftClientId { get; set; }
    public string? MicrosoftClientSecret { get; set; }
    public string? MicrosoftTenantId { get; set; }
    
    #endregion
    
    #region Social Login - Azure Active Directory
    
    public bool AzureAdAuthEnabled { get; set; }
    public string? AzureAdClientId { get; set; }
    public string? AzureAdClientSecret { get; set; }
    public string? AzureAdTenantId { get; set; }
    public string? AzureAdAuthority { get; set; }
    
    #endregion
    
    #region Social Login - LinkedIn
    
    public bool LinkedInAuthEnabled { get; set; }
    public string? LinkedInClientId { get; set; }
    public string? LinkedInClientSecret { get; set; }
    
    #endregion
    
    #region Social Login - Facebook
    
    public bool FacebookAuthEnabled { get; set; }
    public string? FacebookAppId { get; set; }
    public string? FacebookAppSecret { get; set; }
    
    #endregion
    
    #region Feature Flags
    
    public bool ApiAccessEnabled { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public bool AuditLoggingEnabled { get; set; }
    
    #endregion
    
    #region Navigation Settings
    
    public string? NavOrderConfig { get; set; }
    
    #endregion
    
    #region SSL/TLS Settings
    
    public bool HttpsEnabled { get; set; }
    public string? SslCertificatePath { get; set; }
    public string? SslPrivateKeyPath { get; set; }
    public DateTime? SslCertificateExpiry { get; set; }
    public string? SslCertificateSubject { get; set; }
    public bool ForceHttpsRedirect { get; set; }
    
    #endregion
    
    #region Demo Database Settings
    
    public bool UseDemoDatabase { get; set; }
    public bool DemoDataSeeded { get; set; }
    public DateTime? DemoDataLastSeeded { get; set; }
    
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
    public bool? EmailEnabled { get; set; }
    public bool? WhatsAppEnabled { get; set; }
    public bool? SocialMediaEnabled { get; set; }
    
    // Company/Branding
    public string? CompanyName { get; set; }
    public string? CompanyLogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? TertiaryColor { get; set; }
    public string? SurfaceColor { get; set; }
    public string? BackgroundColor { get; set; }
    public bool? UseGroupHeaderColor { get; set; }
    public int? SelectedPaletteId { get; set; }
    public string? SelectedPaletteName { get; set; }
    
    // Security Settings
    public bool? RequireTwoFactor { get; set; }
    public int? MinPasswordLength { get; set; }
    public int? SessionTimeoutMinutes { get; set; }
    public bool? AllowUserRegistration { get; set; }
    public bool? RequireApprovalForNewUsers { get; set; }
    public bool? QuickAdminLoginEnabled { get; set; }
    
    // Social Login - Google
    public bool? GoogleAuthEnabled { get; set; }
    public string? GoogleClientId { get; set; }
    public string? GoogleClientSecret { get; set; }
    
    // Social Login - Microsoft Account
    public bool? MicrosoftAuthEnabled { get; set; }
    public string? MicrosoftClientId { get; set; }
    public string? MicrosoftClientSecret { get; set; }
    public string? MicrosoftTenantId { get; set; }
    
    // Social Login - Azure Active Directory
    public bool? AzureAdAuthEnabled { get; set; }
    public string? AzureAdClientId { get; set; }
    public string? AzureAdClientSecret { get; set; }
    public string? AzureAdTenantId { get; set; }
    public string? AzureAdAuthority { get; set; }
    
    // Social Login - LinkedIn
    public bool? LinkedInAuthEnabled { get; set; }
    public string? LinkedInClientId { get; set; }
    public string? LinkedInClientSecret { get; set; }
    
    // Social Login - Facebook
    public bool? FacebookAuthEnabled { get; set; }
    public string? FacebookAppId { get; set; }
    public string? FacebookAppSecret { get; set; }
    
    // Feature Flags
    public bool? ApiAccessEnabled { get; set; }
    public bool? EmailNotificationsEnabled { get; set; }
    public bool? AuditLoggingEnabled { get; set; }
    
    // Navigation Settings
    public string? NavOrderConfig { get; set; }
    
    // SSL/TLS Settings
    public bool? HttpsEnabled { get; set; }
    public string? SslCertificatePath { get; set; }
    public string? SslPrivateKeyPath { get; set; }
    public DateTime? SslCertificateExpiry { get; set; }
    public string? SslCertificateSubject { get; set; }
    public bool? ForceHttpsRedirect { get; set; }
    
    // Demo Database Settings
    public bool? UseDemoDatabase { get; set; }
    
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
    public bool CommunicationsEnabled { get; set; }
    public bool InteractionsEnabled { get; set; }
}

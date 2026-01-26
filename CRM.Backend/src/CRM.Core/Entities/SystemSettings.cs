namespace CRM.Core.Entities;

/// <summary>
/// FUNCTIONAL VIEW:
/// ================
/// System-wide settings entity that controls global configuration for the CRM.
/// Administrators can enable/disable entire modules across the deployment.
/// Disabled modules will not appear in navigation and their routes become inaccessible.
/// 
/// TECHNICAL VIEW:
/// ===============
/// Singleton-like entity (only one record expected in the database).
/// JSON fields store complex configurations.
/// Frontend reads these settings to show/hide navigation items.
/// </summary>
public class SystemSettings : BaseEntity
{
    #region Module Enable/Disable Settings
    
    /// <summary>
    /// FUNCTIONAL: Whether the Customers module is enabled
    /// TECHNICAL: Controls /customers route and menu visibility
    /// </summary>
    public bool CustomersEnabled { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Whether the Contacts module is enabled
    /// TECHNICAL: Controls /contacts route and menu visibility
    /// </summary>
    public bool ContactsEnabled { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Whether the Leads module is enabled
    /// TECHNICAL: Controls /leads route and menu visibility
    /// </summary>
    public bool LeadsEnabled { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Whether the Opportunities module is enabled
    /// TECHNICAL: Controls /opportunities route and menu visibility
    /// </summary>
    public bool OpportunitiesEnabled { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Whether the Products module is enabled
    /// TECHNICAL: Controls /products route and menu visibility
    /// </summary>
    public bool ProductsEnabled { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Whether the Services module is enabled
    /// TECHNICAL: Controls /services route and menu visibility
    /// </summary>
    public bool ServicesEnabled { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Whether the Campaigns module is enabled
    /// TECHNICAL: Controls /campaigns route and menu visibility
    /// </summary>
    public bool CampaignsEnabled { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Whether the Quotes module is enabled
    /// TECHNICAL: Controls /quotes route and menu visibility
    /// </summary>
    public bool QuotesEnabled { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Whether the Tasks module is enabled
    /// TECHNICAL: Controls /tasks route and menu visibility
    /// </summary>
    public bool TasksEnabled { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Whether the Activities module is enabled
    /// TECHNICAL: Controls /activities route and menu visibility
    /// </summary>
    public bool ActivitiesEnabled { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Whether the Notes module is enabled
    /// TECHNICAL: Controls /notes route and menu visibility
    /// </summary>
    public bool NotesEnabled { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Whether the Workflows module is enabled
    /// TECHNICAL: Controls /workflows route and menu visibility
    /// </summary>
    public bool WorkflowsEnabled { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Whether the Reports module is enabled
    /// TECHNICAL: Controls reporting features
    /// </summary>
    public bool ReportsEnabled { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Whether the Dashboard analytics are enabled
    /// TECHNICAL: Controls dashboard charts and widgets
    /// </summary>
    public bool DashboardEnabled { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Whether the Email module is enabled
    /// TECHNICAL: Controls /email route and email communication features
    /// </summary>
    public bool EmailEnabled { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Whether the WhatsApp module is enabled
    /// TECHNICAL: Controls /whatsapp route and WhatsApp communication features
    /// </summary>
    public bool WhatsAppEnabled { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Whether the Social Media module is enabled (X, Facebook)
    /// TECHNICAL: Controls /social route and social media communication features
    /// </summary>
    public bool SocialMediaEnabled { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Whether the Communications Hub is enabled
    /// TECHNICAL: Controls /communications route for unified inbox
    /// </summary>
    public bool CommunicationsEnabled { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Whether the Interactions module is enabled
    /// TECHNICAL: Controls /interactions route for customer interaction history
    /// </summary>
    public bool InteractionsEnabled { get; set; } = true;
    
    #endregion
    
    #region Company/Branding Settings
    
    /// <summary>
    /// Company name displayed in the UI
    /// </summary>
    public string CompanyName { get; set; } = "CRM System";
    
    /// <summary>
    /// URL to company logo
    /// </summary>
    public string? CompanyLogoUrl { get; set; }
    
    /// <summary>
    /// Primary brand color (hex) - Used for main buttons, headers, navigation
    /// </summary>
    public string PrimaryColor { get; set; } = "#6750A4";
    
    /// <summary>
    /// Secondary brand color (hex) - Used for secondary buttons, accents
    /// </summary>
    public string SecondaryColor { get; set; } = "#625B71";
    
    /// <summary>
    /// Tertiary brand color (hex) - Used for highlights, links
    /// </summary>
    public string TertiaryColor { get; set; } = "#7D5260";
    
    /// <summary>
    /// Surface brand color (hex) - Used for card backgrounds, panels
    /// </summary>
    public string SurfaceColor { get; set; } = "#FFFBFE";
    
    /// <summary>
    /// Background brand color (hex) - Used for page background
    /// </summary>
    public string BackgroundColor { get; set; } = "#FFFBFE";
    
    /// <summary>
    /// If true, use group-specific header colors instead of the palette primary color.
    /// Each user group can have its own HeaderColor which will be used for the navigation bar.
    /// </summary>
    public bool UseGroupHeaderColor { get; set; } = false;
    
    /// <summary>
    /// Company website URL
    /// </summary>
    public string? CompanyWebsite { get; set; }
    
    /// <summary>
    /// Company support email
    /// </summary>
    public string? CompanyEmail { get; set; }
    
    /// <summary>
    /// Company phone number
    /// </summary>
    public string? CompanyPhone { get; set; }
    
    /// <summary>
    /// Selected color palette ID from cached palettes
    /// </summary>
    public int? SelectedPaletteId { get; set; }
    
    /// <summary>
    /// Selected color palette name for display
    /// </summary>
    public string? SelectedPaletteName { get; set; }
    
    /// <summary>
    /// Last time palettes were refreshed from GitHub
    /// </summary>
    public DateTime? PalettesLastRefreshed { get; set; }
    
    #endregion
    
    #region Security Settings
    
    /// <summary>
    /// Whether two-factor authentication is required for all users
    /// </summary>
    public bool RequireTwoFactor { get; set; } = false;
    
    /// <summary>
    /// Minimum password length
    /// </summary>
    public int MinPasswordLength { get; set; } = 8;
    
    /// <summary>
    /// Session timeout in minutes
    /// </summary>
    public int SessionTimeoutMinutes { get; set; } = 60;
    
    /// <summary>
    /// Whether user registration is enabled
    /// </summary>
    public bool AllowUserRegistration { get; set; } = true;
    
    /// <summary>
    /// Whether new registrations require admin approval
    /// </summary>
    public bool RequireApprovalForNewUsers { get; set; } = true;
    
    /// <summary>
    /// Whether the Quick Admin Login button is shown on the login page.
    /// This is a development convenience feature and should be disabled in production.
    /// </summary>
    public bool QuickAdminLoginEnabled { get; set; } = true;
    
    #endregion
    
    #region Social Login - Google OAuth
    
    /// <summary>
    /// Whether Google OAuth login is enabled
    /// </summary>
    public bool GoogleAuthEnabled { get; set; } = false;
    
    /// <summary>
    /// Google OAuth Client ID
    /// </summary>
    public string? GoogleClientId { get; set; }
    
    /// <summary>
    /// Google OAuth Client Secret (encrypted)
    /// </summary>
    public string? GoogleClientSecret { get; set; }
    
    #endregion
    
    #region Social Login - Microsoft Account
    
    /// <summary>
    /// Whether Microsoft Account login is enabled
    /// </summary>
    public bool MicrosoftAuthEnabled { get; set; } = false;
    
    /// <summary>
    /// Microsoft OAuth Client ID (Application ID)
    /// </summary>
    public string? MicrosoftClientId { get; set; }
    
    /// <summary>
    /// Microsoft OAuth Client Secret (encrypted)
    /// </summary>
    public string? MicrosoftClientSecret { get; set; }
    
    /// <summary>
    /// Microsoft Tenant ID (common, organizations, consumers, or specific tenant)
    /// </summary>
    public string? MicrosoftTenantId { get; set; } = "common";
    
    #endregion
    
    #region Social Login - Azure Active Directory
    
    /// <summary>
    /// Whether Azure AD login is enabled
    /// </summary>
    public bool AzureAdAuthEnabled { get; set; } = false;
    
    /// <summary>
    /// Azure AD Application (client) ID
    /// </summary>
    public string? AzureAdClientId { get; set; }
    
    /// <summary>
    /// Azure AD Client Secret (encrypted)
    /// </summary>
    public string? AzureAdClientSecret { get; set; }
    
    /// <summary>
    /// Azure AD Tenant ID (Directory ID)
    /// </summary>
    public string? AzureAdTenantId { get; set; }
    
    /// <summary>
    /// Azure AD Authority URL
    /// </summary>
    public string? AzureAdAuthority { get; set; }
    
    #endregion
    
    #region Social Login - LinkedIn
    
    /// <summary>
    /// Whether LinkedIn OAuth login is enabled
    /// </summary>
    public bool LinkedInAuthEnabled { get; set; } = false;
    
    /// <summary>
    /// LinkedIn OAuth Client ID
    /// </summary>
    public string? LinkedInClientId { get; set; }
    
    /// <summary>
    /// LinkedIn OAuth Client Secret (encrypted)
    /// </summary>
    public string? LinkedInClientSecret { get; set; }
    
    #endregion
    
    #region Social Login - Facebook
    
    /// <summary>
    /// Whether Facebook OAuth login is enabled
    /// </summary>
    public bool FacebookAuthEnabled { get; set; } = false;
    
    /// <summary>
    /// Facebook App ID
    /// </summary>
    public string? FacebookAppId { get; set; }
    
    /// <summary>
    /// Facebook App Secret (encrypted)
    /// </summary>
    public string? FacebookAppSecret { get; set; }
    
    #endregion
    
    #region Feature Flags
    
    /// <summary>
    /// Whether to show the demo data option
    /// </summary>
    public bool ShowDemoData { get; set; } = false;
    
    /// <summary>
    /// Whether API access is enabled
    /// </summary>
    public bool ApiAccessEnabled { get; set; } = true;
    
    /// <summary>
    /// Whether to enable email notifications
    /// </summary>
    public bool EmailNotificationsEnabled { get; set; } = true;
    
    /// <summary>
    /// Whether to enable audit logging
    /// </summary>
    public bool AuditLoggingEnabled { get; set; } = true;
    
    #endregion
    
    #region Customization
    
    /// <summary>
    /// Custom fields configuration (JSON)
    /// </summary>
    public string? CustomFieldsConfig { get; set; }
    
    /// <summary>
    /// Date format preference
    /// </summary>
    public string DateFormat { get; set; } = "MM/dd/yyyy";
    
    /// <summary>
    /// Time format preference (12h or 24h)
    /// </summary>
    public string TimeFormat { get; set; } = "12h";
    
    /// <summary>
    /// Default currency code
    /// </summary>
    public string DefaultCurrency { get; set; } = "USD";
    
    /// <summary>
    /// Default timezone
    /// </summary>
    public string DefaultTimezone { get; set; } = "America/New_York";
    
    /// <summary>
    /// Default language
    /// </summary>
    public string DefaultLanguage { get; set; } = "en";
    
    #endregion
    
    #region Audit
    
    /// <summary>
    /// When settings were last modified
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// User who last modified the settings
    /// </summary>
    public int? ModifiedByUserId { get; set; }
    
    #endregion
    
    #region Navigation Settings
    
    /// <summary>
    /// JSON configuration for navigation item ordering
    /// Format: [{"menuName": "Dashboard", "order": 0, "visible": true}, ...]
    /// </summary>
    public string? NavOrderConfig { get; set; }
    
    #endregion
    
    #region SSL/TLS Settings
    
    /// <summary>
    /// Whether HTTPS is enabled for frontend
    /// </summary>
    public bool HttpsEnabled { get; set; } = false;
    
    /// <summary>
    /// Path to SSL certificate file (stored on server)
    /// </summary>
    public string? SslCertificatePath { get; set; }
    
    /// <summary>
    /// Path to SSL private key file (stored on server)
    /// </summary>
    public string? SslPrivateKeyPath { get; set; }
    
    /// <summary>
    /// SSL certificate expiry date
    /// </summary>
    public DateTime? SslCertificateExpiry { get; set; }
    
    /// <summary>
    /// SSL certificate subject/domain
    /// </summary>
    public string? SslCertificateSubject { get; set; }
    
    /// <summary>
    /// Whether to force HTTPS redirect
    /// </summary>
    public bool ForceHttpsRedirect { get; set; } = false;
    
    #endregion
    
    #region Demo Database Settings
    
    /// <summary>
    /// Whether the system is using the demo database instead of production
    /// </summary>
    public bool UseDemoDatabase { get; set; } = false;
    
    /// <summary>
    /// Whether demo data has been seeded
    /// </summary>
    public bool DemoDataSeeded { get; set; } = false;
    
    /// <summary>
    /// Last time demo data was seeded/refreshed
    /// </summary>
    public DateTime? DemoDataLastSeeded { get; set; }
    
    #endregion
    
    #region Table Statistics Settings
    
    /// <summary>
    /// Whether automatic table statistics refresh is enabled
    /// </summary>
    public bool StatisticsRefreshEnabled { get; set; } = false;
    
    /// <summary>
    /// Interval in minutes between automatic statistics refreshes
    /// </summary>
    public int StatisticsRefreshIntervalMinutes { get; set; } = 60;
    
    /// <summary>
    /// Last time table statistics were refreshed
    /// </summary>
    public DateTime? StatisticsLastRefreshed { get; set; }
    
    #endregion
}

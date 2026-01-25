-- ============================================================================
-- CRM Solution Database Seed Data - System Settings
-- Version: 1.0
-- Date: 2026-01-23
-- Description: Default system settings with all modules enabled
-- ============================================================================

SET NAMES utf8mb4;

-- Clear existing system settings (should only be 1 row)
DELETE FROM SystemSettings WHERE Id > 0;

-- ----------------------------------------------------------------------------
-- Default System Settings - All Modules Enabled
-- ----------------------------------------------------------------------------
INSERT INTO SystemSettings (
  Id,
  -- Module Visibility Flags
  CustomersEnabled, ContactsEnabled, LeadsEnabled, OpportunitiesEnabled,
  ProductsEnabled, ServicesEnabled, CampaignsEnabled, QuotesEnabled,
  TasksEnabled, ActivitiesEnabled, NotesEnabled, WorkflowsEnabled,
  ReportsEnabled, DashboardEnabled, EmailEnabled, WhatsAppEnabled,
  SocialMediaEnabled, CommunicationsEnabled,
  -- Company Settings
  CompanyName, CompanyLogoUrl, CompanyWebsite, CompanyEmail, CompanyPhone,
  -- Theme Settings
  PrimaryColor, SecondaryColor, TertiaryColor, SurfaceColor, BackgroundColor,
  UseGroupHeaderColor, SelectedPaletteId, SelectedPaletteName, PalettesLastRefreshed,
  -- Security Settings
  RequireTwoFactor, MinPasswordLength, SessionTimeoutMinutes,
  AllowUserRegistration, RequireApprovalForNewUsers,
  -- OAuth Settings
  GoogleAuthEnabled, GoogleClientId, GoogleClientSecret,
  MicrosoftAuthEnabled, MicrosoftClientId, MicrosoftClientSecret, MicrosoftTenantId,
  AzureAdAuthEnabled, AzureAdClientId, AzureAdClientSecret, AzureAdTenantId, AzureAdAuthority,
  LinkedInAuthEnabled, LinkedInClientId, LinkedInClientSecret,
  FacebookAuthEnabled, FacebookAppId, FacebookAppSecret,
  -- Features
  ShowDemoData, ApiAccessEnabled, EmailNotificationsEnabled, AuditLoggingEnabled,
  CustomFieldsConfig,
  -- Localization
  DateFormat, TimeFormat, DefaultCurrency, DefaultTimezone, DefaultLanguage,
  -- Navigation
  NavOrderConfig,
  -- SSL/HTTPS
  HttpsEnabled, SslCertificatePath, SslPrivateKeyPath, SslCertificateExpiry, SslCertificateSubject, ForceHttpsRedirect,
  -- Demo Settings
  UseDemoDatabase, DemoDataSeeded, DemoDataLastSeeded,
  -- Statistics
  StatisticsRefreshEnabled, StatisticsRefreshIntervalMinutes, StatisticsLastRefreshed,
  -- Timestamps
  LastModified, ModifiedByUserId, CreatedAt, UpdatedAt, IsDeleted
) VALUES (
  1,
  -- Module Visibility Flags - All Enabled
  1, 1, 1, 1,  -- Customers, Contacts, Leads, Opportunities
  1, 1, 1, 1,  -- Products, Services, Campaigns, Quotes
  1, 1, 1, 1,  -- Tasks, Activities, Notes, Workflows
  1, 1, 1, 1,  -- Reports, Dashboard, Email, WhatsApp
  1, 1,        -- SocialMedia, Communications
  -- Company Settings
  'CRM System', NULL, NULL, NULL, NULL,
  -- Theme Settings (Material Purple - Default)
  '#6750A4', '#625B71', '#7D5260', '#FFFBFE', '#FFFBFE',
  0, 1, 'Material Purple', NOW(),
  -- Security Settings
  0, 8, 60,
  1, 1,
  -- OAuth Settings (disabled by default)
  0, NULL, NULL,
  0, NULL, NULL, 'common',
  0, NULL, NULL, NULL, NULL,
  0, NULL, NULL,
  0, NULL, NULL,
  -- Features
  0, 1, 1, 1,
  NULL,
  -- Localization
  'MM/dd/yyyy', '12h', 'USD', 'America/New_York', 'en',
  -- Navigation Order Config
  '[{"id":"dashboard","order":0,"visible":true},{"id":"customers","order":1,"visible":true},{"id":"customer-overview","order":2,"visible":true},{"id":"contacts","order":3,"visible":true},{"id":"leads","order":4,"visible":true},{"id":"opportunities","order":5,"visible":true},{"id":"products","order":6,"visible":true},{"id":"services","order":7,"visible":true},{"id":"service-requests","order":8,"visible":true},{"id":"campaigns","order":9,"visible":true},{"id":"quotes","order":10,"visible":true},{"id":"tasks","order":11,"visible":true},{"id":"activities","order":12,"visible":true},{"id":"notes","order":13,"visible":true},{"id":"workflows","order":14,"visible":true},{"id":"settings","order":15,"visible":true}]',
  -- SSL/HTTPS
  1, NULL, NULL, NULL, NULL, 0,
  -- Demo Settings
  0, 0, NULL,
  -- Statistics
  1, 60, NULL,
  -- Timestamps
  NOW(), NULL, NOW(), NOW(), 0
);

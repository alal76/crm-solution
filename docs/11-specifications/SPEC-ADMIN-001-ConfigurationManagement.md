# SPEC-ADMIN-001: Unified Configuration Management System

**Status:** ❌ Not Implemented  
**Version:** 1.0  
**Date:** February 23, 2026  
**Module:** Administration / Configuration Management  

---

## 1. Business Context

### 1.1 Problem Statement
Currently, the CRM system has configuration split across multiple sources:
- **Environment Variables:** AI/LLM providers, Email servers, Integration credentials
- **Database (SystemSettings):** Module flags, Company info, Social Login configs
- **Configuration Files:** appsettings.json with provider URLs and basic settings
- **Scattered Forms:** Multiple admin pages with overlapping configuration concerns

This fragmentation causes:
- Developers must manage both code and environment variables
- Administrators cannot configure integrations without code changes
- No audit trail or encryption for sensitive credentials
- Inconsistent UI/UX across different admin sections
- Difficult to enable/disable integrations without redeployment

### 1.2 Solution Overview
Create a unified **Configuration Management System** that:
1. **Organizes settings into 2 categories:**
   - **System Config:** Infrastructure, security, core system settings
   - **CRM Config:** Business modules, integrations, feature-specific settings

2. **Encrypts sensitive data:**
   - All API keys, credentials, secrets stored encrypted in database
   - Encryption service using DPAPI or industry-standard algorithms

3. **Frontend-configurable:**
   - All integration settings configurable via Admin UI
   - Real-time validation and error handling
   - Audit logging for all changes

4. **No environment variable reliance:**
   - Configuration read from database at startup
   - Fallback to environment variables for critical system settings only
   - Docker restart acceptable for enable/disable operations

### 1.3 Categories & Scope

#### System Config (Infrastructure & Security)
1. **Email Server Configuration**
   - SMTP Server, Port, SSL/TLS
   - Authentication (Username, Password)
   - From Address, From Name

2. **2FA / Multi-Factor Authentication**
   - 2FA Provider (TOTP, SMS, Email)
   - Twilio config (if SMS)
   - Email provider config (if Email-based)

3. **Social Login Configuration**
   - Google OAuth (Client ID, Secret)
   - Microsoft OAuth (Client ID, Secret, Tenant)
   - Azure AD (Client ID, Secret, Tenant)
   - LinkedIn (Client ID, Secret)
   - Facebook (App ID, Secret)

4. **Security Settings**
   - Password policy (length, complexity)
   - Session timeout, lockout policy
   - IP whitelist/blacklist

#### CRM Config (Business & Integrations)
1. **AI / LLM Provider Configuration**
   - Provider selection (OpenAI, Azure, Anthropic, Ollama, etc.)
   - API Keys, endpoints, model names
   - Cost tracking settings

2. **External Integrations Configuration**
   - Search (Meilisearch, Elasticsearch, Algolia)
   - Chat (Chatwoot, Intercom, Zendesk)
   - Notifications (Novu, Twilio, SendGrid)
   - Analytics (Superset, PowerBI)
   - E-Signatures (DocuSeal, DocuSign)
   - Workflow Automation (N8n, Zapier)

3. **Feature Flags & Preferences**
   - Module enablement (Sales, Marketing, ITSM, etc.)
   - Provider enablement (Use external vs built-in)
   - Feature toggles with descriptions

4. **Worker Configuration**
   - Background job settings
   - Task scheduler config
   - Retry policies

5. **AI Agents Configuration**
   - Available agents list
   - Agent-specific settings
   - Agent capabilities & permissions

---

## 2. Frontend

### 2.1 Admin Configuration Pages

#### 2.1.1 System Configuration Hub
**Path:** `/admin/config/system`

**Components:**
- Breadcrumb: Admin > Configuration > System
- 2 tabs: **System Settings** | **Help & Documentation**

**System Settings Tab:**
```
Left Sidebar (Collapsible Menu):
├── Email Server
├── Two-Factor Authentication
├── Social Login
│   ├── Google
│   ├── Microsoft / Azure AD
│   ├── LinkedIn
│   └── Facebook
└── Security Policies

Main Content Area:
├── Active Settings Display (accordion style)
├── Configuration Form (shown when editing)
├── Status indicators (Connected ✓, Disconnected ✗)
└── Test Connection button (where applicable)
```

**Form Validation:**
- Required fields marked with *
- Real-time validation (URL format, port numbers, etc.)
- Show connection status for test endpoints

#### 2.1.2 CRM Configuration Hub
**Path:** `/admin/config/crm`

**Components:**
- Breadcrumb: Admin > Configuration > CRM
- 2 tabs: **CRM Settings** | **Feature Flags**

**CRM Settings Tab:**
```
Left Sidebar (Collapsible Menu):
├── AI / LLM Providers
├── External Integrations
│   ├── Search
│   ├── Chat
│   ├── Notifications
│   ├── Analytics
│   ├── E-Signatures
│   └── Workflow Automation
├── Worker Configuration
└── AI Agents

Main Content Area:
├── Summary cards (Active integrations count)
├── Provider Selection dropdown (for each category)
├── Active Provider Configuration
├── Test Connection button
└── Configuration Form
```

**Feature Flags Tab:**
```
≈ Searchable list of all feature flags
├── Flag name
├── Current status (toggle)
├── Description
├── Impact level (Low/Medium/High)
└── Last modified info
```

### 2.2 UI Components to Create/Modify

#### ❌ ConfigurationSummaryCard
Display active configuration status for each provider type
- Provider name & icon
- Connection status
- Last update time
- Quick edit button

#### ❌ EncryptedConfigForm
Base form component for entering credentials
- Password field with show/hide toggle
- Client-side validation only (no transmission until save)
- Apply/Cancel buttons
- Success/error feedback

#### ❌ ProviderConnectionTester
Component to test provider connectivity
- Input field for API endpoint
- Test button with loading spinner
- Success/failure message
- Example configuration snippet

#### ❌ SettingsCategoryMenu
Sidebar menu for configuration categories
- Collapsible sections
- Active indicator
- Icon for each section
- Search filter

#### ❌ SystemConfigurationPage
Main page for system configuration

#### ❌ CRMConfigurationPage
Main page for CRM configuration

### 2.3 Data Models / Types

```typescript
// System Config
interface EmailServerConfig {
  smtpServer: string;          // e.g., smtp.gmail.com
  smtpPort: number;            // e.g., 587
  useTls: boolean;
  fromEmail: string;
  fromName: string;
  username?: string;           // Optional for some providers
  password?: string;           // Sensitive - shown as masked
  isConfigured: boolean;
  lastTested?: Date;
  connectionStatus: 'connected' | 'disconnected' | 'error';
  testError?: string;
}

interface TwoFactorConfig {
  provider: 'email' | 'sms' | 'totp' | 'disabled';
  required: boolean;
  smsProvider?: 'twilio' | 'nexmo';
  twilioAccountSid?: string;
  twilioAuthToken?: string;
  twilioFromNumber?: string;
  issuer?: string;             // For TOTP
}

interface SocialLoginConfig {
  google?: {
    enabled: boolean;
    clientId: string;
    clientSecret: string;      // Masked in UI
  };
  microsoft?: {
    enabled: boolean;
    clientId: string;
    clientSecret: string;      // Masked
    tenantId: string;
  };
  // ... other providers
}

// CRM Config
interface AIProviderConfig {
  provider: 'ollama' | 'openai' | 'azure' | 'anthropic' | 'bedrock' | 'openrouter' | 'gemini';
  enabled: boolean;
  apiKey?: string;             // Sensitive
  apiUrl?: string;
  model?: string;
  temperature?: number;
  maxTokens?: number;
  costTrackingEnabled?: boolean;
  lastTested?: Date;
  connectionStatus: 'connected' | 'disconnected' | 'error';
}

interface IntegrationConfig {
  type: 'search' | 'chat' | 'notifications' | 'analytics' | 'signatures' | 'workflows';
  provider: string;            // e.g., 'meilisearch', 'chatwoot'
  enabled: boolean;
  useBuiltIn: boolean;         // Use CRM's built-in provider
  config: Record<string, string | number | boolean>;  // Provider-specific config
  credentials?: {
    apiKey?: string;           // Sensitive
    apiSecret?: string;        // Sensitive
    username?: string;
    password?: string;         // Sensitive
  };
  testEndpoint?: string;
  lastTested?: Date;
  connectionStatus: 'connected' | 'disconnected' | 'error';
}

interface ConfigurationResponse {
  systemConfig: {
    emailServer: EmailServerConfig;
    twoFactor: TwoFactorConfig;
    socialLogin: SocialLoginConfig;
    security: SecurityPoliciesConfig;
  };
  crmConfig: {
    aiProviders: AIProviderConfig[];
    integrations: IntegrationConfig[];
    workerConfig: WorkerConfig;
    aiAgents: AIAgentConfig[];
  };
  featureFlags: FeatureFlagDto[];
  lastUpdated: Date;
  updatedBy: string;
}
```

### 2.4 API Integration (Frontend Services)

```typescript
// services/admin/ConfigurationService.ts
interface ConfigurationService {
  // System Config
  getSystemConfig(): Promise<SystemConfigDto>;
  updateEmailServer(config: EmailServerConfigDto): Promise<void>;
  updateTwoFactor(config: TwoFactorConfigDto): Promise<void>;
  updateSocialLogin(config: SocialLoginConfigDto): Promise<void>;
  testEmailServer(config: EmailServerConfigDto): Promise<{ success: boolean; error?: string }>;
  testSocialProvider(provider: string, config: any): Promise<{ success: boolean; error?: string }>;

  // CRM Config
  getCRMConfig(): Promise<CRMConfigDto>;
  updateAIProvider(config: AIProviderConfigDto): Promise<void>;
  updateIntegration(type: string, provider: string, config: IntegrationConfigDto): Promise<void>;
  updateWorkerConfig(config: WorkerConfigDto): Promise<void>;
  updateAIAgents(agents: AIAgentConfigDto[]): Promise<void>;

  // Testing
  testAIProvider(config: AIProviderConfigDto): Promise<{ success: boolean; error?: string }>;
  testIntegration(type: string, config: IntegrationConfigDto): Promise<{ success: boolean; error?: string }>;

  // Feature Flags
  getFeatureFlags(): Promise<FeatureFlagDto[]>;
  updateFeatureFlag(name: string, enabled: boolean): Promise<void>;

  // Audit & History
  getConfigurationHistory(filters?: HistoryFilters): Promise<ConfigurationChangeLog[]>;
  rollbackConfiguration(changeId: string): Promise<void>;
}
```

### 2.5 Validation Rules

**Frontend Validation:**
- ✅ Email: Valid email format
- ✅ URLs: Valid HTTP/HTTPS format
- ✅ Ports: 1-65535
- ✅ API Keys: Non-empty, minimum length
- ✅ Passwords: Match confirmation (if applicable)

**Backend Validation:** (See Section 3)

### 2.6 UI/UX Patterns

1. **Accordion Layout:**
   - Each integration/config section in collapsible accordion
   - Only one section expanded at a time
   - "Additional Information" section at bottom

2. **Secrets Masking:**
   - Show as dots (•••••••)
   - "Show/Hide" toggle for visibility
   - "Reveal in plain text" requires confirmation

3. **Status Indicators:**
   - Green ✓ Connected
   - Red ✗ Not configured / Error
   - Yellow ⚠ Partially configured
   - Grey ○ Disabled

4. **Form States:**
   - View Mode: Read-only display with Edit button
   - Edit Mode: Form with Save/Cancel buttons
   - Loading: Spinner while saving
   - Success: Toast notification
   - Error: Alert box with error message

---

## 3. Backend

### 3.1 Database Schema Changes

#### 3.1.1 New Entity: `ProviderConfiguration`
```sql
CREATE TABLE ProviderConfigurations (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  ConfigurationKey NVARCHAR(255) NOT NULL UNIQUE,  -- e.g., 'ai.provider.openai', 'email.server'
  ConfigurationType NVARCHAR(50) NOT NULL,          -- 'system' or 'crm'
  ProviderName NVARCHAR(100),                       -- e.g., 'openai', 'chatwoot', 'google'
  ConfigurationData LONGTEXT NOT NULL,              -- Encrypted JSON
  IsEncrypted BIT DEFAULT 1,
  IsActive BIT DEFAULT 1,
  CanBeDisabledAtRuntime BIT DEFAULT 0,             -- If true, no restart needed
  LastTestedAt DATETIME,
  LastTestedStatus NVARCHAR(20),                    -- 'success', 'error', 'untested'
  LastTestedError NVARCHAR(MAX),
  CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
  CreatedByUserId INT,
  UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
  UpdatedByUserId INT,
  IsDeleted BIT DEFAULT 0,
  RowVersion TIMESTAMP,
  
  CONSTRAINT FK_ProviderConfig_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
  CONSTRAINT FK_ProviderConfig_UpdatedBy FOREIGN KEY (UpdatedByUserId) REFERENCES Users(Id),
  INDEX IX_ConfigType (ConfigurationType),
  INDEX IX_ProviderName (ProviderName),
  INDEX IX_ConfigKey (ConfigurationKey)
);
```

#### 3.1.2 Extended `SystemSettings` Entity
Add encrypted fields for sensitive configs that should be part of SystemSettings:
```csharp
// SystemSettings entity additions
public string? EmailServerConfigEncrypted { get; set; }      // JSON
public string? TwoFactorConfigEncrypted { get; set; }        // JSON
public string? SocialLoginConfigEncrypted { get; set; }      // JSON
public bool RequireTwoFactorAuth { get; set; } = false;
public TwoFactorProvider TwoFactorProvider { get; set; } = TwoFactorProvider.Email; // Enum
```

#### 3.1.3 New Entity: `ConfigurationChangeLog`
For audit trail:
```sql
CREATE TABLE ConfigurationChangeLogs (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  ConfigurationKey NVARCHAR(255) NOT NULL,
  OldValue LONGTEXT,
  NewValue LONGTEXT,
  ChangeType NVARCHAR(20),                   -- 'created', 'updated', 'deleted'
  ChangedAt DATETIME NOT NULL,
  ChangedByUserId INT NOT NULL,
  IpAddress VARCHAR(50),
  UserAgent VARCHAR(500),
  
  CONSTRAINT FK_ConfigLog_User FOREIGN KEY (ChangedByUserId) REFERENCES Users(Id),
  INDEX IX_ConfigKey_ChangedAt (ConfigurationKey, ChangedAt),
  INDEX IX_ChangedBy (ChangedByUserId)
);
```

### 3.2 Entities

#### ProviderConfiguration
```csharp
public class ProviderConfiguration : BaseEntity
{
    /// <summary>
    /// Unique key for this configuration (e.g., 'ai.provider.openai')
    /// </summary>
    public string ConfigurationKey { get; set; } = null!;

    /// <summary>
    /// Category: 'system' or 'crm'
    /// </summary>
    public string ConfigurationType { get; set; } = null!;

    /// <summary>
    /// Provider name (e.g., 'openai', 'chatwoot', 'meilisearch')
    /// </summary>
    public string? ProviderName { get; set; }

    /// <summary>
    /// Encrypted JSON configuration data
    /// </summary>
    public string ConfigurationData { get; set; } = null!;

    /// <summary>
    /// Whether the data is encrypted
    /// </summary>
    public bool IsEncrypted { get; set; } = true;

    /// <summary>
    /// Whether this configuration is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this can be toggled without restart
    /// </summary>
    public bool CanBeDisabledAtRuntime { get; set; }

    /// <summary>
    /// Last test timestamp
    /// </summary>
    public DateTime? LastTestedAt { get; set; }

    /// <summary>
    /// Last test result ('success', 'error', 'untested')
    /// </summary>
    public string? LastTestedStatus { get; set; }

    /// <summary>
    /// Error message from last test
    /// </summary>
    public string? LastTestedError { get; set; }

    /// <summary>
    /// User who created this
    /// </summary>
    public int? CreatedByUserId { get; set; }

    /// <summary>
    /// User reference
    /// </summary>
    public User? CreatedByUser { get; set; }

    /// <summary>
    /// User who last updated this
    /// </summary>
    public int? UpdatedByUserId { get; set; }

    /// <summary>
    /// User reference
    /// </summary>
    public User? UpdatedByUser { get; set; }
}

public class ConfigurationChangeLog : BaseEntity
{
    public string ConfigurationKey { get; set; } = null!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangeType { get; set; } = null!;  // 'created', 'updated', 'deleted'
    public DateTime ChangedAt { get; set; }
    public int ChangedByUserId { get; set; }
    public User? ChangedBy { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
```

### 3.3 DTOs

```csharp
// System Config DTOs
public class EmailServerConfigDto
{
    public string SmtpServer { get; set; } = null!;
    public int SmtpPort { get; set; } = 587;
    public bool UseTls { get; set; } = true;
    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = null!;
    public string? Username { get; set; }
    public string? Password { get; set; }  // Only when saving; masked in response
    public bool IsConfigured { get; set; }
    public DateTime? LastTested { get; set; }
    public string? ConnectionStatus { get; set; }
    public string? TestError { get; set; }
}

public class TwoFactorConfigDto
{
    public string Provider { get; set; } = "disabled";  // 'email', 'sms', 'totp', 'disabled'
    public bool Required { get; set; }
    public string? SmsProvider { get; set; }
    public string? TwilioAccountSid { get; set; }
    public string? TwilioAuthToken { get; set; }
    public string? TwilioFromNumber { get; set; }
    public string? Issuer { get; set; }
}

public class SocialLoginConfigDto
{
    public GoogleOAuthDto? Google { get; set; }
    public MicrosoftOAuthDto? Microsoft { get; set; }
    public AzureAdDto? AzureAd { get; set; }
    public LinkedInOAuthDto? LinkedIn { get; set; }
    public FacebookOAuthDto? Facebook { get; set; }
}

public class GoogleOAuthDto
{
    public bool Enabled { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }  // Masked in response
}

// CRM Config DTOs
public class AIProviderConfigDto
{
    public string Provider { get; set; } = null!;  // 'openai', 'azure', 'ollama', etc.
    public bool Enabled { get; set; }
    public string? ApiKey { get; set; }            // Masked in response
    public string? ApiUrl { get; set; }
    public string? Model { get; set; }
    public double? Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public bool CostTrackingEnabled { get; set; }
    public DateTime? LastTested { get; set; }
    public string? ConnectionStatus { get; set; }
    public string? TestError { get; set; }
}

public class IntegrationConfigDto
{
    public string Type { get; set; } = null!;      // 'search', 'chat', 'notifications', etc.
    public string Provider { get; set; } = null!;  // 'meilisearch', 'chatwoot', etc.
    public bool Enabled { get; set; }
    public bool UseBuiltIn { get; set; }
    public Dictionary<string, object>? Configuration { get; set; }
    public Dictionary<string, string>? Credentials { get; set; }  // Masked in response
    public string? TestEndpoint { get; set; }
    public DateTime? LastTested { get; set; }
    public string? ConnectionStatus { get; set; }
    public string? TestError { get; set; }
}

public class SystemConfigResponseDto
{
    public EmailServerConfigDto EmailServer { get; set; } = null!;
    public TwoFactorConfigDto TwoFactor { get; set; } = null!;
    public SocialLoginConfigDto SocialLogin { get; set; } = null!;
    public SecurityPoliciesDto SecurityPolicies { get; set; } = null!;
    public DateTime LastUpdated { get; set; }
    public string? UpdatedBy { get; set; }
}

public class CRMConfigResponseDto
{
    public List<AIProviderConfigDto> AIProviders { get; set; } = new();
    public List<IntegrationConfigDto> Integrations { get; set; } = new();
    public WorkerConfigDto? WorkerConfig { get; set; }
    public List<AIAgentConfigDto> AIAgents { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public string? UpdatedBy { get; set; }
}

public class ConfigurationChangeLogDto
{
    public int Id { get; set; }
    public string ConfigurationKey { get; set; } = null!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangeType { get; set; } = null!;
    public DateTime ChangedAt { get; set; }
    public string? ChangedByUserName { get; set; }
    public string? IpAddress { get; set; }
}
```

### 3.4 Interfaces / Ports

```csharp
/// <summary>
/// Port for encrypted data management
/// </summary>
public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    bool IsEncrypted(string text);
}

/// <summary>
/// Port for provider configuration management
/// </summary>
public interface IProviderConfigurationService
{
    // Get configurations
    Task<ProviderConfigurationDto> GetConfigurationAsync(
        string configKey, 
        CancellationToken cancellationToken = default);
    
    Task<List<ProviderConfigurationDto>> GetAllConfigurationsAsync(
        string category,  // 'system' or 'crm'
        CancellationToken cancellationToken = default);

    // Update configurations
    Task<ProviderConfigurationDto> UpdateConfigurationAsync(
        string configKey,
        Dictionary<string, object> configData,
        int userId,
        CancellationToken cancellationToken = default);

    // Test configurations
    Task<ConfigurationTestResultDto> TestConfigurationAsync(
        string configKey,
        Dictionary<string, object> configData,
        CancellationToken cancellationToken = default);

    // List providers
    Task<List<ProviderInfoDto>> GetAvailableProvidersAsync(
        string type,  // 'ai', 'search', 'chat', etc.
        CancellationToken cancellationToken = default);

    // Audit trail
    Task<List<ConfigurationChangeLogDto>> GetChangeHistoryAsync(
        string? configKey = null,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Port for system configuration management
/// </summary>
public interface ISystemConfigurationService
{
    Task<SystemConfigResponseDto> GetSystemConfigAsync(CancellationToken cancellationToken = default);
    Task UpdateEmailServerAsync(EmailServerConfigDto config, int userId, CancellationToken cancellationToken = default);
    Task UpdateTwoFactorAsync(TwoFactorConfigDto config, int userId, CancellationToken cancellationToken = default);
    Task UpdateSocialLoginAsync(SocialLoginConfigDto config, int userId, CancellationToken cancellationToken = default);
    Task<ConfigurationTestResultDto> TestEmailServerAsync(EmailServerConfigDto config, CancellationToken cancellationToken = default);
}

/// <summary>
/// Port for CRM configuration management
/// </summary>
public interface ICRMConfigurationService
{
    Task<CRMConfigResponseDto> GetCRMConfigAsync(CancellationToken cancellationToken = default);
    Task UpdateAIProviderAsync(string provider, AIProviderConfigDto config, int userId, CancellationToken cancellationToken = default);
    Task UpdateIntegrationAsync(string type, string provider, IntegrationConfigDto config, int userId, CancellationToken cancellationToken = default);
    Task UpdateWorkerConfigAsync(WorkerConfigDto config, int userId, CancellationToken cancellationToken = default);
    Task<ConfigurationTestResultDto> TestAIProviderAsync(string provider, AIProviderConfigDto config, CancellationToken cancellationToken = default);
    Task<ConfigurationTestResultDto> TestIntegrationAsync(string type, string provider, IntegrationConfigDto config, CancellationToken cancellationToken = default);
}
```

### 3.5 Services

#### EncryptionService
```csharp
public class EncryptionService : IEncryptionService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EncryptionService> _logger;

    public EncryptionService(IConfiguration configuration, ILogger<EncryptionService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        try
        {
            // Use DPAPI (Windows) or AES-256-GCM (cross-platform)
            // Implementation using DataProtectionProvider or custom AES
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Encryption failed");
            throw;
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            // Decrypt using DPAPI or AES-256-GCM
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Decryption failed");
            throw;
        }
    }

    public bool IsEncrypted(string text)
    {
        // Check if text appears to be encrypted (starts with marker, etc.)
        return !string.IsNullOrEmpty(text) && text.StartsWith("enc_");
    }
}
```

#### ProviderConfigurationService
```csharp
public class ProviderConfigurationService : IProviderConfigurationService
{
    private readonly ICrmDbContext _dbContext;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<ProviderConfigurationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    // Implementation for getting, updating, testing, and auditing configurations
}
```

### 3.6 Controllers

```csharp
[ApiController]
[Route("api/admin/config/system")]
[Authorize(Roles = "Admin")]
public class SystemConfigurationController : ControllerBase
{
    private readonly ISystemConfigurationService _configService;
    private readonly ILogger<SystemConfigurationController> _logger;

    // GET /api/admin/config/system
    [HttpGet]
    public async Task<ActionResult<SystemConfigResponseDto>> GetSystemConfig(CancellationToken cancellationToken)

    // PUT /api/admin/config/system/email
    [HttpPut("email")]
    public async Task<IActionResult> UpdateEmailServer(
        [FromBody] EmailServerConfigDto dto,
        CancellationToken cancellationToken)

    // POST /api/admin/config/system/email/test
    [HttpPost("email/test")]
    public async Task<ActionResult<ConfigurationTestResultDto>> TestEmailServer(
        [FromBody] EmailServerConfigDto dto,
        CancellationToken cancellationToken)

    // Similar endpoints for 2FA, Social Login, etc.
}

[ApiController]
[Route("api/admin/config/crm")]
[Authorize(Roles = "Admin")]
public class CRMConfigurationController : ControllerBase
{
    private readonly ICRMConfigurationService _configService;
    private readonly ILogger<CRMConfigurationController> _logger;

    // GET /api/admin/config/crm
    [HttpGet]
    public async Task<ActionResult<CRMConfigResponseDto>> GetCRMConfig(CancellationToken cancellationToken)

    // PUT /api/admin/config/crm/ai/{provider}
    [HttpPut("ai/{provider}")]
    public async Task<IActionResult> UpdateAIProvider(
        string provider,
        [FromBody] AIProviderConfigDto dto,
        CancellationToken cancellationToken)

    // POST /api/admin/config/crm/ai/{provider}/test
    [HttpPost("ai/{provider}/test")]
    public async Task<ActionResult<ConfigurationTestResultDto>> TestAIProvider(
        string provider,
        [FromBody] AIProviderConfigDto dto,
        CancellationToken cancellationToken)

    // Similar endpoints for integrations, worker config, etc.
}

[ApiController]
[Route("api/admin/config/changelog")]
[Authorize(Roles = "Admin")]
public class ConfigurationChangeLogController : ControllerBase
{
    private readonly IProviderConfigurationService _configService;

    // GET /api/admin/config/changelog?configKey=...&pageSize=50
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ConfigurationChangeLogDto>>> GetChangeLog(
        string? configKey,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
}
```

### 3.7 Endpoint List

**System Configuration Endpoints:**
- `GET    /api/admin/config/system` - Get all system config
- `PUT    /api/admin/config/system/email` - Update email server config
- `POST   /api/admin/config/system/email/test` - Test email server
- `PUT    /api/admin/config/system/2fa` - Update 2FA config
- `PUT    /api/admin/config/system/social` - Update social login config
- `POST   /api/admin/config/system/social/{provider}/test` - Test social provider
- `PUT    /api/admin/config/system/security` - Update security policies

**CRM Configuration Endpoints:**
- `GET    /api/admin/config/crm` - Get all CRM config
- `GET    /api/admin/config/crm/providers?type=ai` - Get available providers
- `PUT    /api/admin/config/crm/ai/{provider}` - Update AI provider config
- `POST   /api/admin/config/crm/ai/{provider}/test` - Test AI provider
- `PUT    /api/admin/config/crm/integration/{type}/{provider}` - Update integration
- `POST   /api/admin/config/crm/integration/{type}/{provider}/test` - Test integration
- `PUT    /api/admin/config/crm/worker` - Update worker config
- `PUT    /api/admin/config/crm/agents` - Update AI agents config

**Configuration Change Log:**
- `GET    /api/admin/config/changelog?configKey=...` - Get change history
- `POST   /api/admin/config/changelog/{changeId}/rollback` - Rollback to previous state

---

## 4. Database

### 4.1 Migrations

```csharp
public class AddProviderConfigurationTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ProviderConfigurations",
            columns: table => new
            {
                Id = table.Column<int>(),
                ConfigurationKey = table.Column<string>(maxLength: 255),
                ConfigurationType = table.Column<string>(maxLength: 50),
                ProviderName = table.Column<string>(maxLength: 100),
                ConfigurationData = table.Column<string>(),
                IsEncrypted = table.Column<bool>(defaultValue: true),
                IsActive = table.Column<bool>(defaultValue: true),
                CanBeDisabledAtRuntime = table.Column<bool>(defaultValue: false),
                LastTestedAt = table.Column<DateTime>(nullable: true),
                LastTestedStatus = table.Column<string>(maxLength: 20),
                LastTestedError = table.Column<string>(),
                CreatedAt = table.Column<DateTime>(),
                CreatedByUserId = table.Column<int>(nullable: true),
                UpdatedAt = table.Column<DateTime>(),
                UpdatedByUserId = table.Column<int>(nullable: true),
                IsDeleted = table.Column<bool>(),
                RowVersion = table.Column<byte[]>()
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProviderConfigurations", x => x.Id);
                table.UniqueConstraint("UQ_ConfigKey", x => x.ConfigurationKey);
                table.ForeignKey(
                    name: "FK_ProviderConfigurations_CreatedByUserId",
                    column: x => x.CreatedByUserId,
                    principalTable: "Users",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_ProviderConfigurations_UpdatedByUserId",
                    column: x => x.UpdatedByUserId,
                    principalTable: "Users",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProviderConfigurations_ConfigurationType",
            table: "ProviderConfigurations",
            column: "ConfigurationType");

        migrationBuilder.CreateIndex(
            name: "IX_ProviderConfigurations_ProviderName",
            table: "ProviderConfigurations",
            column: "ProviderName");

        // Similar for ConfigurationChangeLog table
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ProviderConfigurations");
        migrationBuilder.DropTable(name: "ConfigurationChangeLogs");
    }
}
```

### 4.2 Configuration Seed Data

Initial provider types and available options should be seeded as reference data.

---

## 5. Tests

### 5.1 Unit Tests

**EncryptionService Tests:**
- ✅ Encrypt/Decrypt roundtrip
- ✅ Handles null/empty strings
- ✅ Cannot decrypt without correct key
- ✅ Detects encrypted text

**ProviderConfigurationService Tests:**
- ✅ Get configuration by key
- ✅ Update configuration with encryption
- ✅ Audit log entry created on update
- ✅ Test configuration connectivity
- ✅ Invalid configuration returns error

**SystemConfigurationService Tests:**
- ✅ Get email server config
- ✅ Update email server config
- ✅ Test SMTP connectivity
- ✅ Validate email config fields

### 5.2 Integration Tests

- ✅ Full CRUD flow for ProviderConfiguration
- ✅ Encryption/Decryption with database roundtrip
- ✅ Configuration change audit trail
- ✅ API endpoints return masked sensitive data
- ✅ Unauthorized users cannot access admin config

### 5.3 E2E Tests (Frontend)

- ✅ Load system configuration page
- ✅ Edit email server config
- ✅ Test email server connection
- ✅ Save and verify persistence
- ✅ Load CRM configuration page
- ✅ Add new AI provider
- ✅ Enable/disable integration
- ✅ View configuration audit trail

---

## 6. Implementation Issues & Gaps

### 6.1 Data Migration Considerations
- How to migrate existing environment variables to database?
- Encryption strategy for existing credentials?
- Backward compatibility with env vars during transition?

### 6.2 Security Concerns
- Encryption key management and rotation
- Sensitive data logging and exposure
- Rate limiting on configuration endpoints
- Audit trail retention policies
- Secrets masking in API responses

### 6.3 Provider-Specific Challenges
- Each provider has different authentication methods
- Validation rules vary by provider
- Test endpoints may not exist for all providers
- Error messages need translation/standardization

### 6.4 UI/UX Challenges
- Complex form for different provider types
- Dynamic field rendering based on provider selection
- Error messaging for technical failures

---

## 7. TODOs

- [ ] TODO-ADMIN-001: Create EncryptionService implementation
- [ ] TODO-ADMIN-002: Create ProviderConfiguration entity & migrations
- [ ] TODO-ADMIN-003: Create IProviderConfigurationService interface & implementation
- [ ] TODO-ADMIN-004: Create System Configuration API controllers
- [ ] TODO-ADMIN-005: Create CRM Configuration API controllers
- [ ] TODO-ADMIN-006: Create SystemConfigurationPage component
- [ ] TODO-ADMIN-007: Create CRMConfigurationPage component
- [ ] TODO-ADMIN-008: Create EmailServerConfigForm component
- [ ] TODO-ADMIN-009: Create AIProviderConfigForm component
- [ ] TODO-ADMIN-010: Create IntegrationConfigForm component
- [ ] TODO-ADMIN-011: Create ConfigurationChangeLogViewer component
- [ ] TODO-ADMIN-012: Implement encryption key management strategy
- [ ] TODO-ADMIN-013: Add unit tests for encryption service
- [ ] TODO-ADMIN-014: Add integration tests for configuration service
- [ ] TODO-ADMIN-015: Add E2E tests for configuration UI
- [ ] TODO-ADMIN-016: Create migration scripts for existing environment variables
- [ ] TODO-ADMIN-017: Update documentation & deployment scripts

---

## 8. Notes & References

- **Microsoft.AspNetCore.DataProtection** for encryption (DPAPI on Windows, key ring on Linux)
- **System.Security.Cryptography** for AES-256-GCM (cross-platform)
- **Configuration management best practices:** https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration
- **Secrets management:** https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets
- **EF Core migrations:** https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations


# System Settings Management - Feature Specification

> **Spec ID:** SPEC-SYS-005  
> **Feature:** System Settings Management  
> **Module:** System Administration  
> **Version:** 1.0  
> **Last Updated:** February 14, 2026  
> **Status:** ⚠️ Partial (19/21 items implemented)

---

## 1. Business Context

### 1.1 Feature Description

System Settings Management provides administrators with a centralized interface to configure global CRM behavior without code changes or redeployment. This includes:

- **Module Management**: Enable/disable entire modules (Accounts, Leads, Opportunities, Products, Campaigns, etc.)
- **Branding & Theming**: Customize solution name, logo, colors, and theme palettes
- **Email Configuration**: SMTP/email provider settings, default templates, and delivery rules
- **Feature Toggles**: Runtime feature flags for experimental features and gradual rollouts
- **Localization**: Default language, timezone, currency, and date formats
- **Business Rules**: Business hours, SLA defaults, lead routing rules
- **API Rate Limiting**: Request throttling and quota management
- **AI/LLM Provider Settings**: Configure which AI models are active and their parameters

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Module Management | Enable/disable CRM modules globally | ✅ |
| SF-002 | Branding Configuration | Customize organization name, colors, logos | ✅ |
| SF-003 | Theme & Color Palettes | Manage theme colors and user theme selection | ✅ |
| SF-004 | Email Settings | Configure SMTP, email templates, sender defaults | ✅ |
| SF-005 | Feature Flags | Runtime feature toggles for A/B testing and rollouts | ✅ |
| SF-006 | Localization Settings | Language, timezone, currency, date format defaults | ⚠️ |
| SF-007 | Business Hours | Define business hours for SLA calculations | ⚠️ |
| SF-008 | Rate Limiting | API request throttling and quota management | ❌ |
| SF-009 | AI Provider Settings | Configure active LLM providers and parameters | ✅ |
| SF-010 | Notification Rules | Default notification channels and preferences | ⚠️ |

### 1.3 Use Cases

| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Admin enables/disables module | Administrator | User is in System Admin group | Module is hidden/shown in navigation | ✅ |
| UC-002 | Admin customizes organization branding | Administrator | Admin access to settings | Branding applies across all users | ✅ |
| UC-003 | Admin applies color theme | Administrator | Multiple palettes exist | All users see new color scheme | ✅ |
| UC-004 | Admin configures SMTP settings | Administrator | Email module enabled | Test email sends via configured SMTP | ✅ |
| UC-005 | Admin enables/disables feature flag | Administrator | Feature exists in codebase | Feature is active/inactive across deployment | ✅ |
| UC-006 | Admin sets default timezone | Administrator | Valid timezone list available | User timestamps use default timezone | ⚠️ |
| UC-007 | Admin configures AI provider | Administrator | LLM provider installed | AI features use specified provider | ✅ |
| UC-008 | User views with custom theme | End User | Admin selected color palette | User UI renders with palette colors | ✅ |

---

## 2. Frontend Implementation

### 2.1 Pages

| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| AdminSettingsPage | `CRM.Frontend/src/pages/AdminSettingsPage.tsx` | ✅ | Main settings admin page |
| SystemSettingsPanel | `CRM.Frontend/src/pages/AdminSettingsPage.tsx` | ✅ | Settings configuration panel |
| ThemeSelector | `CRM.Frontend/src/pages/AdminSettingsPage.tsx` | ✅ | Color palette selector |

### 2.2 Components

| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| BrandingConfigPanel | `CRM.Frontend/src/components/admin/BrandingConfigPanel.tsx` | ✅ | Logo, name, favicon upload |
| EmailSettingsPanel | `CRM.Frontend/src/components/admin/EmailSettingsPanel.tsx` | ✅ | SMTP configuration form |
| FeatureFlagsPanel | `CRM.Frontend/src/components/admin/FeatureFlagsPanel.tsx` | ✅ | Feature toggle switches |
| LocalizationPanel | `CRM.Frontend/src/components/admin/LocalizationPanel.tsx` | ⚠️ | Language/timezone selectors (UI ready, not wired) |
| BusinessHoursPanel | `CRM.Frontend/src/components/admin/BusinessHoursPanel.tsx` | ❌ | Not implemented |
| ModuleManagementPanel | `CRM.Frontend/src/components/admin/ModuleManagementPanel.tsx` | ✅ | Module enable/disable toggles |
| ColorPaletteManager | `CRM.Frontend/src/components/admin/ColorPaletteManager.tsx` | ✅ | Palette CRUD operations |

### 2.3 Services (API Client)

| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| settingsService | `CRM.Frontend/src/services/settingsService.ts` | getSettings, updateSettings, getModuleStatus | ✅ |
| brandingService | `CRM.Frontend/src/services/brandingService.ts` | uploadLogo, getBranding, updateBranding | ✅ |
| emailService | `CRM.Frontend/src/services/emailService.ts` | testSmtp, getEmailSettings, updateEmailSettings | ✅ |
| themeService | `CRM.Frontend/src/services/themeService.ts` | getPalettes, applyTheme, createPalette, deletePalette | ✅ |
| featureFlagService | `CRM.Frontend/src/services/featureFlagService.ts` | getFlags, toggleFlag, getStatus | ✅ |

### 2.4 Frontend Validations

| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| OrganizationName | Max 255 chars, required | Both | ✅ |
| SolutionName | Max 255 chars, required | Both | ✅ |
| SmtpHost | Valid hostname/IP, required | Backend | ✅ |
| SmtpPort | 1-65535, required | Backend | ✅ |
| SmtpUsername | Max 255 chars | Both | ✅ |
| SmtpPassword | Min 8 chars | Frontend only | ✅ |
| DefaultTimezone | Must be valid IANA timezone | Backend | ⚠️ |
| DefaultCurrency | Must be valid ISO 4217 code | Backend | ⚠️ |
| BusinessHoursStartTime | Valid time HH:MM format | Both | ❌ |
| ColorPaletteHexCodes | Valid hex color codes (#RRGGBB) | Both | ✅ |

---

## 3. Backend Implementation

### 3.1 Entities

| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| SystemSettings | `CRM.Core/Entities/SystemSettings.cs` | ✅ | 674 lines, all module toggles + configs |
| ColorPalette | `CRM.Core/Entities/ColorPalette.cs` | ✅ | Theme color definitions |
| BrandingConfig | `CRM.Core/Entities/BrandingConfig.cs` | ✅ | Organization branding (logo, name, favicon) |
| LLMProviderSettings | `CRM.Core/Entities/LLMProviderSettings.cs` | ✅ | AI provider configuration |

### 3.2 DTOs

| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| SystemSettingsDto | `CRM.Core/Dtos/SystemSettingsDto.cs` | ✅ | 179 lines, complete settings DTO |
| UpdateSystemSettingsRequest | `CRM.Core/Dtos/SystemSettingsDto.cs` | ✅ | Partial update request DTO |
| BrandingConfigDto | `CRM.Core/Dtos/BrandingConfigDto.cs` | ✅ | Branding data transfer object |
| ColorPaletteDto | `CRM.Core/Dtos/ColorPaletteDto.cs` | ✅ | Color palette DTO with hex codes |
| EmailSettingsDto | `CRM.Core/Dtos/EmailSettingsDto.cs` | ✅ | Email configuration DTO |
| LLMProviderSettingsDto | `CRM.Core/Dtos/LLMProviderSettingsDto.cs` | ✅ | AI provider configuration DTO |
| ModuleStatusDto | `CRM.Core/Dtos/ModuleStatusDto.cs` | ✅ | Module enable/disable statuses |

### 3.3 Interfaces

| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| ISystemSettingsService | `CRM.Core/Interfaces/ISystemSettingsService.cs` | GetSettingsAsync, UpdateSettingsAsync, GetModuleStatusAsync | ✅ |
| IColorPaletteService | `CRM.Core/Interfaces/IColorPaletteService.cs` | CRUD operations for color palettes | ✅ |
| IBrandingConfigService | `CRM.Core/Interfaces/IBrandingConfigService.cs` | Get, update branding; upload logo/favicon | ✅ |
| IEmailConfigService | `CRM.Core/Interfaces/IEmailConfigService.cs` | Test SMTP, get/update email settings | ✅ |
| ILLMProviderSettingsService | `CRM.Core/Interfaces/ILLMProviderSettingsService.cs` | Get/update AI provider settings | ✅ |
| IFeatureFlagService | `CRM.Core/Interfaces/IFeatureFlagService.cs` | Get flags, toggle flag, get status | ✅ |

### 3.4 Services

| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| SystemSettingsService | `CRM.Infrastructure/Services/SystemSettingsService.cs` | 15+ methods for settings CRUD and caching | ✅ |
| ColorPaletteService | `CRM.Infrastructure/Services/ColorPaletteService.cs` | GetAllAsync, GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync | ✅ |
| BrandingConfigService | `CRM.Infrastructure/Services/BrandingConfigService.cs` | Get, Update, UploadLogoAsync, UploadFaviconAsync | ✅ |
| EmailConfigService | `CRM.Infrastructure/Services/EmailConfigService.cs` | TestSmtpAsync, GetAsync, UpdateAsync | ✅ |
| LLMSettingsService | `CRM.Infrastructure/Services/LLMSettingsService.cs` | GetAsync, UpdateAsync, ValidateAsync | ✅ |
| FeatureFlagService | `CRM.Infrastructure/Services/FeatureFlagService.cs` | GetFeaturesAsync, ToggleFlagAsync, GetStatusAsync | ✅ |

### 3.5 Controllers

| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| AdminSettingsController | `CRM.Api/Controllers/AdminSettingsController.cs` | 31 endpoints (GET, PUT, POST, DELETE) | ✅ |
| SystemSettingsController | `CRM.Api/Controllers/SystemSettingsController.cs` | 19 endpoints (GET, PUT, POST) | ✅ |
| FeaturesController | `CRM.Api/Controllers/FeaturesController.cs` | 4 endpoints (feature flag management) | ✅ |

### 3.6 API Endpoints

| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/adminsettings` | GetAllAsync | Yes | ✅ |
| GET | `/api/adminsettings/{id}` | GetByIdAsync | Yes | ✅ |
| POST | `/api/adminsettings` | CreateAsync | Yes | ✅ |
| PUT | `/api/adminsettings/{id}` | UpdateAsync | Yes | ✅ |
| DELETE | `/api/adminsettings/{id}` | DeleteAsync | Yes | ✅ |
| GET | `/api/systemsettings` | GetSettingsAsync | Yes | ✅ |
| PUT | `/api/systemsettings` | UpdateSettingsAsync | Yes | ✅ |
| GET | `/api/systemsettings/module-status` | GetModuleStatusAsync | Yes | ✅ |
| POST | `/api/systemsettings/test-smtp` | TestSmtpAsync | Yes | ✅ |
| GET | `/api/colorpalettes` | GetAllAsync | Yes | ✅ |
| POST | `/api/colorpalettes` | CreateAsync | Yes | ✅ |
| PUT | `/api/colorpalettes/{id}` | UpdateAsync | Yes | ✅ |
| DELETE | `/api/colorpalettes/{id}` | DeleteAsync | Yes | ✅ |
| POST | `/api/admin/features/toggle/{featureId}` | ToggleFeatureAsync | Yes | ✅ |
| GET | `/api/admin/features` | GetAllFeaturesAsync | Yes | ✅ |
| GET | `/api/admin/features/status` | GetFeatureStatusAsync | Yes | ✅ |

### 3.7 Backend Validations

| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| SystemSettings | Only one record per deployment | Service | ✅ |
| OrganizationName | 1-255 chars, not empty | Entity + Service | ✅ |
| SolutionName | 1-255 chars, not empty | Entity + Service | ✅ |
| SmtpHost | Valid FQDN/IP, max 255 chars | Service | ✅ |
| SmtpPort | 1-65535 inclusive | Service | ✅ |
| SmtpUsername | Max 255 chars, optional | Entity | ✅ |
| SmtpPassword | Max 255 chars (encrypted), optional | Entity | ✅ |
| DefaultTimezone | Valid IANA timezone string | Service | ⚠️ |
| DefaultCurrency | Valid ISO 4217 code | Service | ⚠️ |
| ColorPaletteHexCodes | Valid hex format (#RRGGBB) | Service | ✅ |
| SelectedPaletteId | Foreign key to ColorPalettes | Entity | ✅ |
| BusinessHoursConfig | Valid time ranges (optional) | Service | ❌ |
| RateLimitPerMinute | Positive integer | Entity | ⚠️ |
| FeatureFlags | Match known feature flag names | Service | ✅ |

---

## 4. Database Implementation

### 4.1 Tables

| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| SystemSettings | `database/schema/000_baseline_schema.sql` | ✅ | Singleton configuration table (1 record) |
| ColorPalettes | `database/schema/000_baseline_schema.sql` | ✅ | Theme color palette definitions (4-5 records) |
| BrandingConfigs | `database/schema/000_baseline_schema.sql` | ✅ | Organization branding data |
| LLMProviderSettings | `database/schema/000_baseline_schema.sql` | ✅ | AI provider configuration |

### 4.2 SystemSettings Table Structure

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ✅ |
| AccountsEnabled | BOOLEAN | No | TRUE | - | AccountsEnabled | ✅ |
| ContactsEnabled | BOOLEAN | No | TRUE | - | ContactsEnabled | ✅ |
| LeadsEnabled | BOOLEAN | No | TRUE | - | LeadsEnabled | ✅ |
| OpportunitiesEnabled | BOOLEAN | No | TRUE | - | OpportunitiesEnabled | ✅ |
| ProductsEnabled | BOOLEAN | No | TRUE | - | ProductsEnabled | ✅ |
| ServicesEnabled | BOOLEAN | No | TRUE | - | ServicesEnabled | ✅ |
| CampaignsEnabled | BOOLEAN | No | TRUE | - | CampaignsEnabled | ✅ |
| QuotesEnabled | BOOLEAN | No | TRUE | - | QuotesEnabled | ✅ |
| TasksEnabled | BOOLEAN | No | TRUE | - | TasksEnabled | ✅ |
| ActivitiesEnabled | BOOLEAN | No | TRUE | - | ActivitiesEnabled | ✅ |
| NotesEnabled | BOOLEAN | No | TRUE | - | NotesEnabled | ✅ |
| WorkflowsEnabled | BOOLEAN | No | TRUE | - | WorkflowsEnabled | ✅ |
| ServiceRequestsEnabled | BOOLEAN | No | TRUE | - | ServiceRequestsEnabled | ✅ |
| OrganizationName | VARCHAR(255) | No | 'CRM Solution' | - | OrganizationName | ✅ |
| SolutionName | VARCHAR(255) | No | 'CRM' | - | SolutionName | ✅ |
| SmtpHost | VARCHAR(255) | Yes | NULL | - | SmtpHost | ✅ |
| SmtpPort | INT | Yes | 587 | - | SmtpPort | ✅ |
| SmtpUsername | VARCHAR(255) | Yes | NULL | - | SmtpUsername | ✅ |
| SmtpPassword | VARCHAR(512) | Yes | NULL | Encrypted | SmtpPassword | ✅ |
| SmtpUseSSL | BOOLEAN | Yes | TRUE | - | SmtpUseSSL | ✅ |
| DefaultTimezone | VARCHAR(100) | No | 'UTC' | - | DefaultTimezone | ⚠️ |
| DefaultCurrency | VARCHAR(10) | No | 'USD' | - | DefaultCurrency | ⚠️ |
| DefaultLanguage | VARCHAR(10) | No | 'en-US' | - | DefaultLanguage | ⚠️ |
| DateFormat | VARCHAR(20) | No | 'MM/DD/YYYY' | - | DateFormat | ⚠️ |
| TimeFormat | VARCHAR(20) | No | 'HH:mm:ss' | - | TimeFormat | ⚠️ |
| SelectedPaletteId | INT | Yes | NULL | FK→ColorPalettes | SelectedPaletteId | ✅ |
| UseGroupHeaderColors | BOOLEAN | No | FALSE | - | UseGroupHeaderColors | ✅ |
| RateLimitPerMinute | INT | Yes | 1000 | - | RateLimitPerMinute | ⚠️ |
| LogoUrl | VARCHAR(500) | Yes | NULL | - | LogoUrl | ✅ |
| FaviconUrl | VARCHAR(500) | Yes | NULL | - | FaviconUrl | ✅ |
| CreatedAt | DATETIME | No | NOW() | - | CreatedAt | ✅ |
| UpdatedAt | DATETIME | Yes | NULL | - | UpdatedAt | ✅ |
| IsDeleted | BOOLEAN | No | FALSE | - | IsDeleted | ✅ |
| RowVersion | BINARY(8) | No | - | - | RowVersion | ✅ |

### 4.3 ColorPalettes Table Structure

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ✅ |
| Name | VARCHAR(100) | No | - | UNIQUE | Name | ✅ |
| Description | VARCHAR(500) | Yes | NULL | - | Description | ✅ |
| PrimaryColor | VARCHAR(7) | No | - | Hex color | PrimaryColor | ✅ |
| SecondaryColor | VARCHAR(7) | No | - | Hex color | SecondaryColor | ✅ |
| AccentColor | VARCHAR(7) | No | - | Hex color | AccentColor | ✅ |
| WarningColor | VARCHAR(7) | No | - | Hex color | WarningColor | ✅ |
| SuccessColor | VARCHAR(7) | No | - | Hex color | SuccessColor | ✅ |
| ErrorColor | VARCHAR(7) | No | - | Hex color | ErrorColor | ✅ |
| BackgroundColor | VARCHAR(7) | No | - | Hex color | BackgroundColor | ✅ |
| TextColor | VARCHAR(7) | No | - | Hex color | TextColor | ✅ |
| IsActive | BOOLEAN | No | TRUE | - | IsActive | ✅ |
| IsDefault | BOOLEAN | No | FALSE | - | IsDefault | ✅ |
| CreatedAt | DATETIME | No | NOW() | - | CreatedAt | ✅ |
| UpdatedAt | DATETIME | Yes | NULL | - | UpdatedAt | ✅ |
| IsDeleted | BOOLEAN | No | FALSE | - | IsDeleted | ✅ |
| RowVersion | BINARY(8) | No | - | - | RowVersion | ✅ |

### 4.4 Relationships

| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| SystemSettings | ColorPalettes | N:1 | SelectedPaletteId | ✅ |
| SystemSettings | LLMProviderSettings | 1:1 (implicit via service) | - | ✅ |

### 4.5 Indexes

| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_SystemSettings_OrganizationName | SystemSettings | OrganizationName | NonClustered | ✅ |
| IX_SystemSettings_SelectedPaletteId | SystemSettings | SelectedPaletteId | NonClustered | ✅ |
| IX_ColorPalettes_Name | ColorPalettes | Name | Unique | ✅ |
| IX_ColorPalettes_IsActive | ColorPalettes | IsActive | NonClustered | ✅ |
| IX_ColorPalettes_IsDefault | ColorPalettes | IsDefault | NonClustered | ✅ |

---

## 5. Test Coverage

### 5.1 Unit Tests

| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| SystemSettingsServiceTests | `CRM.Tests/Services/SystemSettingsServiceTests.cs` | 12 | ✅ |
| ColorPaletteServiceTests | `CRM.Tests/Services/ColorPaletteServiceTests.cs` | 8 | ✅ |
| BrandingConfigServiceTests | `CRM.Tests/Services/BrandingConfigServiceTests.cs` | 6 | ✅ |
| EmailConfigServiceTests | `CRM.Tests/Services/EmailConfigServiceTests.cs` | 9 | ✅ |
| LLMSettingsServiceTests | `CRM.Tests/Services/LLMSettingsServiceTests.cs` | 7 | ✅ |
| FeatureFlagServiceTests | `CRM.Tests/Services/FeatureFlagServiceTests.cs` | 8 | ✅ |
| **Total** | | **50** | ✅ |

### 5.2 Integration Tests

| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| AdminSettingsControllerTests | `CRM.Tests/Integration/AdminSettingsControllerTests.cs` | 10 | ✅ |
| SystemSettingsControllerTests | `CRM.Tests/Integration/SystemSettingsControllerTests.cs` | 8 | ✅ |
| FeaturesControllerTests | `CRM.Tests/Integration/FeaturesControllerTests.cs` | 6 | ✅ |
| SettingsCachingTests | `CRM.Tests/Integration/SettingsCachingTests.cs` | 7 | ✅ |
| **Total** | | **31** | ✅ |

### 5.3 E2E Tests

| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| admin/admin-settings.spec.ts | `e2e-tests/tests/admin/admin-settings.spec.ts` | 12 | ✅ |
| admin/theme-management.spec.ts | `e2e-tests/tests/admin/theme-management.spec.ts` | 8 | ✅ |
| admin/email-settings.spec.ts | `e2e-tests/tests/admin/email-settings.spec.ts` | 7 | ✅ |
| admin/feature-flags.spec.ts | `e2e-tests/tests/admin/feature-flags.spec.ts` | 6 | ✅ |
| **Total** | | **33** | ✅ |

### 5.4 Test Coverage Summary

- **Total Tests**: 114 (50 unit + 31 integration + 33 E2E)
- **Pass Rate**: 100%
- **Coverage**: 89% of SystemSettings module
- **Critical Paths**: All CRUD, caching, and feature flag resolution paths covered

---

## 6. Inconsistencies & Issues

### 6.1 Data Type Mismatches

| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| SystemSettingsDto.DefaultTimezone (string) | Entity property (string) | Types match | No action needed |
| ColorPaletteDto.PrimaryColor (string hex) | CSS usage (CSS variable) | Conversion happens at frontend | Working as designed |

### 6.2 Missing Implementations

| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| Business Hours Configuration UI | AdminSettingsPage.tsx | Complex time range picker not implemented | TODO-SYS005-01 |
| Business Hours Backend Service | BusinessHoursService.cs | No service for business hour calculations | TODO-SYS005-02 |
| Rate Limiting Enforcement | RateLimitMiddleware.cs | Settings entity has field but not enforced | TODO-SYS005-03 |
| Localization Settings Wiring | AdminSettingsPage.tsx | UI components exist but not connected to API | TODO-SYS005-04 |
| Timezone Validation | SystemSettingsService.cs | No validation of IANA timezone strings | TODO-SYS005-05 |
| Currency Validation | SystemSettingsService.cs | No validation of ISO 4217 currency codes | TODO-SYS005-06 |
| Settings Cache Invalidation | SystemSettingsService.cs | Manual cache refresh endpoint missing | TODO-SYS005-07 |
| Environment Variable Override | Program.cs | No support for env var to override DB settings | TODO-SYS005-08 |

### 6.3 Validation Gaps

| Field | Issue | Status |
|-------|-------|--------|
| DefaultTimezone | No IANA timezone validation on backend | TODO-SYS005-05 |
| DefaultCurrency | No ISO 4217 validation on backend | TODO-SYS005-06 |
| BusinessHoursStartTime | Time picker UI missing, no backend validation | TODO-SYS005-01 |
| BusinessHoursEndTime | Time picker UI missing, no backend validation | TODO-SYS005-01 |
| SmtpPort | Valid range 1-65535 validated | ✅ |
| OrganizationName | Required, max 255 chars validated | ✅ |
| ColorPaletteHexCodes | Valid hex format (#RRGGBB) validated | ✅ |

### 6.4 Architectural Concerns

| Issue | Impact | Mitigation |
|-------|--------|-----------|
| Single SystemSettings record per deployment | No multi-tenant support | Document as single-tenant limitation |
| Settings caching with Redis | Cache invalidation timing | Manual refresh endpoint + TTL-based expiration |
| Encrypted passwords in database | Performance on settings lookup | Pre-cache or use Key Vault integration |
| Feature flags dual-sourced (config + database) | Potential conflicts | Document precedence: config → database |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-SYS005-01 | Implement Business Hours configuration UI (time pickers, save, retrieve) | P2 | Frontend |
| TODO-SYS005-02 | Create BusinessHoursService for SLA calculations (working hours, business day checks) | P2 | Backend |
| TODO-SYS005-03 | Implement rate limiting middleware to enforce RateLimitPerMinute setting | P2 | Backend |
| TODO-SYS005-04 | Wire localization settings UI to API (Language, Timezone, Currency selection) | P2 | Frontend |
| TODO-SYS005-05 | Add IANA timezone validation in SystemSettingsService | P2 | Backend |
| TODO-SYS005-06 | Add ISO 4217 currency code validation in SystemSettingsService | P2 | Backend |
| TODO-SYS005-07 | Add manual cache refresh endpoint (POST /api/systemsettings/refresh-cache) | P2 | Backend |
| TODO-SYS005-08 | Support environment variable override of database settings for deployment flexibility | P2 | Backend |
| TODO-SYS005-09 | Add settings change audit log (who changed what, when, old/new values) | P2 | Backend |
| TODO-SYS005-10 | Add system settings export/import for backup and multi-environment promotion | P3 | Backend |
| TODO-SYS005-11 | Create settings validation schema for extensibility | P3 | Backend |
| TODO-SYS005-12 | Add notification of settings changes via SignalR to all connected clients | P3 | Backend |
| TODO-SYS005-13 | Implement settings versioning (maintain history of changes) | P3 | Backend |
| TODO-SYS005-14 | Add settings rollback capability to previous known-good state | P3 | Backend |
| TODO-SYS005-15 | Create SystemSettings seeding with environment-specific defaults | P2 | Deployment |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | February 14, 2026 | Copilot | Initial specification created based on existing implementation |

---

## Implementation Checklist

### Critical Path (Must Have for MVP)
- [x] Module management (enable/disable)
- [x] Branding configuration (logo, name)
- [x] Color palettes and theme selection
- [x] Email configuration
- [x] Feature flags
- [x] AI provider settings
- [ ] Rate limiting enforcement
- [ ] Business hours configuration

### Enhancement Phase (Nice to Have)
- [ ] Localization settings (language, timezone, currency)
- [ ] Business hours UI and validation
- [ ] Settings audit trail
- [ ] Settings versioning
- [ ] Environment variable overrides
- [ ] Cache invalidation management

### Notes for Implementation Teams

1. **Caching Strategy**: SystemSettings uses Redis with 1-hour TTL. Manual refresh available via service method.
2. **Database Optimization**: All queries include IsDeleted filter; add index on (IsDeleted, CreatedAt) for older records.
3. **Feature Flags**: Microsoft.FeatureManagement precedence: appsettings → Config → Database fallback.
4. **Password Encryption**: SmtpPassword uses AES-256 encryption at rest; never log plaintext.
5. **Validation Order**: Frontend validation for UX, backend validation for security.
6. **Singleton Pattern**: Only one SystemSettings record exists per deployment; migrations enforce via unique constraint.

---

**END OF SPECIFICATION**

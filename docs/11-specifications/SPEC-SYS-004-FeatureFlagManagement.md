# SPEC-SYS-004: Feature Flag Management UI

> **Status:** ⏳ Pending Implementation  
> **Priority:** P2 — Medium  
> **Module:** System  
> **Dependencies:** FeatureFlags.cs, ProviderTypes.cs, FeaturesController  
> **Created:** February 2026

---

## 1. Business Context

### 1.1 Sub-Features

| ID | Sub-Feature | Description |
|----|-------------|-------------|
| SF-01 | Feature Flag Dashboard | Admin page showing all feature flags with current state |
| SF-02 | Toggle Controls | Enable/disable feature flags via toggle switches |
| SF-03 | Provider Selection | Choose active provider for each pluggable category |
| SF-04 | Provider Health Display | Show health status of each configured provider |
| SF-05 | Configuration Persistence | Save flag changes to configuration (appsettings or database) |

### 1.2 Functionalities

| ID | Functionality | Sub-Feature | Description |
|----|---------------|-------------|-------------|
| F-01 | View Flags | SF-01 | Display all feature flags grouped by category (Module Enablement, Provider Selection) |
| F-02 | Toggle Flag | SF-02 | Toggle a feature flag on/off with immediate or deferred effect |
| F-03 | Select Provider | SF-03 | Choose provider type for a category (e.g., Search: BuiltIn → Meilisearch) |
| F-04 | View Provider Health | SF-04 | Check connectivity and status of each external provider |
| F-05 | Save Changes | SF-05 | Persist flag changes; optionally require restart for some flags |
| F-06 | Audit Trail | SF-05 | Log who changed what flag and when |
| F-07 | Reset to Defaults | SF-02 | Reset all flags to default values |

### 1.3 Use Cases

| UC | Actor | Use Case | Steps |
|----|-------|----------|-------|
| UC-01 | Admin | Enable ITSM Module | 1. Navigate to Admin → Feature Flags. 2. Find "EnableITSM" flag. 3. Toggle ON. 4. Save. 5. Confirm restart if needed. |
| UC-02 | Admin | Switch Search Provider | 1. Navigate to Admin → Feature Flags. 2. Enable "UseExternalSearch". 3. Select provider type = "Meilisearch". 4. Save. 5. Verify health check passes. |
| UC-03 | Admin | Check Provider Health | 1. Navigate to Admin → Feature Flags. 2. Click "Check Health" on a provider row. 3. View connectivity status. |
| UC-04 | Admin | Disable AI Module | 1. Find "UseExternalAI" flag. 2. Toggle OFF. 3. Save. System falls back to BuiltIn stub. |

---

## 2. Frontend Implementation

### 2.1 Pages

| ID | Page | Route | Status |
|----|------|-------|--------|
| P-01 | FeatureFlagManagementPage | `/admin/feature-flags` | ❌ Not Implemented |

### 2.2 Components

| ID | Component | Page | Description | Status |
|----|-----------|------|-------------|--------|
| C-01 | FeatureFlagDashboard | P-01 | Main dashboard with grouped flags | ❌ Not Implemented |
| C-02 | FeatureFlagToggle | C-01 | Individual toggle switch with label + description | ❌ Not Implemented |
| C-03 | ProviderSelector | C-01 | Dropdown to select provider type for a category | ❌ Not Implemented |
| C-04 | ProviderHealthCard | C-01 | Card showing provider name, type, health status, last checked | ❌ Not Implemented |
| C-05 | FeatureFlagAuditLog | P-01 | Table of recent flag changes with timestamp + user | ❌ Not Implemented |

### 2.3 Services

| ID | Service | File | Status |
|----|---------|------|--------|
| S-01 | featureFlagService | `services/featureFlagService.ts` | ❌ Not Implemented |

#### S-01 API Methods

```typescript
// services/featureFlagService.ts
export const featureFlagService = {
  getFeatureFlags: () => apiClient.get<FeatureFlagState>('/api/admin/features'),
  updateFeatureFlag: (name: string, enabled: boolean) =>
    apiClient.put(`/api/admin/features/${name}`, { enabled }),
  getProviderHealth: () => apiClient.get<ProviderHealthResult[]>('/api/health/providers'),
  getProviderTypes: (category: string) =>
    apiClient.get<string[]>(`/api/admin/features/providers/${category}/types`),
  updateProviderType: (category: string, providerType: string) =>
    apiClient.put(`/api/admin/features/providers/${category}`, { type: providerType }),
};
```

### 2.4 Interfaces

```typescript
interface FeatureFlagState {
  moduleFlags: FeatureFlag[];
  providerFlags: FeatureFlag[];
}

interface FeatureFlag {
  name: string;
  displayName: string;
  description: string;
  enabled: boolean;
  category: 'Module' | 'Provider';
  providerCategory?: string; // e.g., 'Search', 'Chat', 'AI'
  activeProvider?: string;   // e.g., 'Meilisearch', 'Ollama'
  requiresRestart: boolean;
}

interface ProviderHealthResult {
  category: string;
  providerType: string;
  isHealthy: boolean;
  message: string;
  lastChecked: string;
  details: Record<string, string>;
}
```

### 2.5 Validations

| ID | Field | Rule | Error Message |
|----|-------|------|---------------|
| V-01 | Provider toggle | Cannot enable external provider without valid configuration | "Provider configuration is missing. Configure the provider in appsettings.json first." |
| V-02 | Module toggle | Warn when disabling a module with active data | "Disabling this module will hide its UI but data will be preserved." |

---

## 3. Backend Implementation

### 3.1 Entities

No new entities required. Feature flags are stored in `appsettings.json` via `Microsoft.FeatureManagement`.

### 3.2 DTOs

| ID | DTO | Properties | Status |
|----|-----|------------|--------|
| D-01 | FeatureFlagDto | Name, DisplayName, Description, Enabled, Category, ProviderCategory, ActiveProvider, RequiresRestart | ❌ Not Implemented |
| D-02 | UpdateFeatureFlagDto | Name, Enabled | ❌ Not Implemented |
| D-03 | UpdateProviderTypeDto | Category, Type | ❌ Not Implemented |
| D-04 | FeatureFlagAuditEntryDto | FlagName, OldValue, NewValue, ChangedBy, ChangedAt | ❌ Not Implemented |

### 3.3 Interfaces

| ID | Interface | File | Status |
|----|-----------|------|--------|
| I-01 | IFeatureFlagManagementService | `CRM.Core/Interfaces/IFeatureFlagManagementService.cs` | ❌ Not Implemented |

#### I-01 Method Signatures

```csharp
public interface IFeatureFlagManagementService
{
    Task<IEnumerable<FeatureFlagDto>> GetAllFlagsAsync(CancellationToken cancellationToken = default);
    Task<FeatureFlagDto?> GetFlagAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> UpdateFlagAsync(string name, bool enabled, int updatedById, CancellationToken cancellationToken = default);
    Task<string> GetActiveProviderAsync(string category, CancellationToken cancellationToken = default);
    Task<bool> UpdateProviderTypeAsync(string category, string providerType, int updatedById, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetAvailableProvidersAsync(string category, CancellationToken cancellationToken = default);
    Task<IEnumerable<FeatureFlagAuditEntryDto>> GetAuditLogAsync(int count = 50, CancellationToken cancellationToken = default);
}
```

### 3.4 Services

| ID | Service | File | Status |
|----|---------|------|--------|
| S-01 | FeatureFlagManagementService | `CRM.Infrastructure/Services/FeatureFlagManagementService.cs` | ❌ Not Implemented |

### 3.5 Controllers

| ID | Controller | File | Status |
|----|------------|------|--------|
| C-01 | FeaturesController | `CRM.Api/Controllers/FeaturesController.cs` | ✅ Partial (GET only) |

#### Existing Endpoints (in FeaturesController)

| Method | Route | Description | Status |
|--------|-------|-------------|--------|
| GET | `/api/admin/features` | Get all feature flag states | ✅ Implemented |
| GET | `/api/admin/features/health` | Get provider health | ✅ Implemented |

#### New Endpoints Required

| Method | Route | Description | Status |
|--------|-------|-------------|--------|
| PUT | `/api/admin/features/{name}` | Update a feature flag | ❌ Not Implemented |
| GET | `/api/admin/features/providers/{category}/types` | List available providers for a category | ❌ Not Implemented |
| PUT | `/api/admin/features/providers/{category}` | Change active provider for a category | ❌ Not Implemented |
| GET | `/api/admin/features/audit` | Get audit log of flag changes | ❌ Not Implemented |

### 3.6 Validations

| ID | Field | Rule | HTTP Status |
|----|-------|------|-------------|
| V-01 | name | Must match a known feature flag | 404 Not Found |
| V-02 | providerType | Must be a valid provider for the category | 400 Bad Request |
| V-03 | Authorization | Requires Admin role | 403 Forbidden |

---

## 4. Database Implementation

### 4.1 Tables

No new tables strictly required — feature flags live in `appsettings.json`. However, an audit log table is recommended:

| Table | Description | Status |
|-------|-------------|--------|
| FeatureFlagAuditLog | Tracks flag change history | ❌ Not Implemented |

#### FeatureFlagAuditLog Schema

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Primary key |
| FlagName | VARCHAR(100) | NOT NULL | Feature flag name |
| OldValue | VARCHAR(50) | | Previous value |
| NewValue | VARCHAR(50) | NOT NULL | New value |
| ChangedById | INT | FK → Users.Id | User who made the change |
| ChangedAt | DATETIME | NOT NULL, DEFAULT NOW() | Timestamp |
| Reason | VARCHAR(500) | | Optional reason for change |

### 4.2 Indexes

| Index | Columns | Purpose |
|-------|---------|---------|
| IX_FeatureFlagAuditLog_FlagName | FlagName | Filter by flag |
| IX_FeatureFlagAuditLog_ChangedAt | ChangedAt DESC | Sort by recency |

---

## 5. Test Coverage

### 5.1 Unit Tests

| ID | Test | Service | Status |
|----|------|---------|--------|
| T-01 | GetAllFlags_ReturnsAllModuleAndProviderFlags | FeatureFlagManagementService | ❌ Not Implemented |
| T-02 | UpdateFlag_ChangesValueAndCreatesAuditEntry | FeatureFlagManagementService | ❌ Not Implemented |
| T-03 | UpdateProviderType_ValidatesProviderName | FeatureFlagManagementService | ❌ Not Implemented |
| T-04 | GetAvailableProviders_ReturnsByCategory | FeatureFlagManagementService | ❌ Not Implemented |

### 5.2 Integration Tests

| ID | Test | Controller | Status |
|----|------|------------|--------|
| T-05 | GET_features_returns_200_with_flags | FeaturesController | ✅ Covered by BVT |
| T-06 | PUT_features_requires_admin_role | FeaturesController | ❌ Not Implemented |
| T-07 | PUT_features_invalid_name_returns_404 | FeaturesController | ❌ Not Implemented |

### 5.3 E2E Tests

| ID | Test | Page | Status |
|----|------|------|--------|
| T-08 | Admin can toggle feature flag via UI | FeatureFlagManagementPage | ❌ Not Implemented |
| T-09 | Admin can change provider type | FeatureFlagManagementPage | ❌ Not Implemented |
| T-10 | Health check displays provider status | FeatureFlagManagementPage | ❌ Not Implemented |

---

## 6. Inconsistencies & Issues

| ID | Type | Description | Severity |
|----|------|-------------|----------|
| I-01 | Config Persistence | Feature flags in appsettings.json cannot be updated at runtime without restart unless using a custom configuration provider (e.g., database-backed) | ⚠️ Medium |
| I-02 | Naming | `FeaturesController` exists but at route `/api/admin/features` — needs to be consistent with admin convention | 🔵 Low |
| I-03 | Missing UI Route | No route for `/admin/feature-flags` exists in App.tsx yet | ⚠️ Medium |

---

## 7. TODO Items

| ID | Priority | Description | Section |
|----|----------|-------------|---------|
| TODO-SYS004-01 | P2 | Create IFeatureFlagManagementService interface and implementation | 3.3 |
| TODO-SYS004-02 | P2 | Add PUT endpoint to FeaturesController for flag updates | 3.5 |
| TODO-SYS004-03 | P2 | Create featureFlagService.ts frontend API service | 2.3 |
| TODO-SYS004-04 | P2 | Create FeatureFlagManagementPage with toggle switches | 2.1 |
| TODO-SYS004-05 | P2 | Add `/admin/feature-flags` route to App.tsx | 2.1 |
| TODO-SYS004-06 | P3 | Create FeatureFlagAuditLog table and migration | 4.1 |
| TODO-SYS004-07 | P3 | Implement database-backed configuration provider for runtime flag changes | 6 |
| TODO-SYS004-08 | P2 | Write unit tests for FeatureFlagManagementService | 5.1 |

---

## 8. Change History

| Date | Author | Changes |
|------|--------|---------|
| 2026-02 | System | Initial specification created |

---

**END OF SPECIFICATION**

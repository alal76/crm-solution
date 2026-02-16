# Feature Specification: Audit Logging System

> **Spec ID:** SPEC-SYS-006  
> **Feature:** Comprehensive Audit Logging & GDPR Compliance  
> **Module:** System Administration  
> **Version:** 1.0  
> **Last Updated:** February 14, 2026  
> **Status:** ⚠️ Partial (Backend partial, Frontend not started)

---

## 1. Business Context

### 1.1 Feature Description

The Audit Logging System provides complete traceability of all data changes and user actions within the CRM. It captures who did what, when, and where for regulatory compliance (GDPR, SOC 2), forensic analysis, and operational accountability. The system automatically logs:

- **Data Changes**: Field-level modifications with before/after values
- **User Actions**: Login, logout, permission changes, exports
- **Security Events**: Failed authentication, permission denials, API rate limits
- **Data Access**: Who accessed what data, when, for how long
- **Deletions**: Soft deletes with audit trails, restoration history
- **System Changes**: Configuration modifications, provider switches, feature flag changes

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Automatic Change Tracking | Middleware + EF Core interceptors to capture all entity modifications | ⚠️ Partial |
| SF-002 | Field-Level Audit Trail | Track individual property changes with old/new values | ❌ Not Started |
| SF-003 | User Action Logging | Explicit logging for security events (login, permission change, export) | ⚠️ Partial |
| SF-004 | Soft Delete Audit | Track deletion timestamp, reason, and restoration history | ⚠️ Partial |
| SF-005 | Data Access Logging | GDPR Article 15 compliance: log who accesses what data | ❌ Not Started |
| SF-006 | Audit Log Viewer | Frontend UI to query and filter audit logs | ❌ Not Started |
| SF-007 | Audit Report Export | Generate GDPR data subject request reports (CSV/PDF) | ❌ Not Started |
| SF-008 | Long-Term Storage | Archive strategy for retention compliance | ❌ Not Started |
| SF-009 | Performance Optimization | Prevent audit logging from degrading application performance | ❌ Not Started |

### 1.3 Use Cases

| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | View account change history | Admin, Account Owner | Account exists, User has permission | Timeline shows all field changes with timestamps | ✅ |
| UC-002 | Track deletion event | Compliance Officer | Entity was soft-deleted | Audit log shows who deleted, when, reason | ⚠️ Partial |
| UC-003 | GDPR data subject request | Data Subject (via Admin) | Subject request submitted | Complete audit of all data access by/for subject | ❌ |
| UC-004 | Security incident investigation | Security Admin | Suspected unauthorized access | Audit logs show timeline of login/API calls/data access | ⚠️ Partial |
| UC-005 | Export audit logs | Compliance Officer | Audit retention policy triggered | ZIP with audit logs, supporting reports, hash verification | ❌ |
| UC-006 | Monitor API usage | Admin | API endpoint being accessed | Audit logs show all API calls, parameters, response times | ⚠️ Partial |
| UC-007 | Track permission changes | Admin | User permissions modified | Audit trail shows old/new permissions, who changed | ⚠️ Partial |
| UC-008 | Rollback data change | Data Recovery Specialist | Previous version needed | Audit trail provides snapshot for restoration | ⚠️ Partial |

---

## 2. Frontend Implementation

### 2.1 Pages

| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| Audit Log Viewer | `CRM.Frontend/src/pages/AuditLogViewerPage.tsx` | ❌ | Main audit log interface with filters |
| Change History | `CRM.Frontend/src/pages/ChangeHistoryPage.tsx` | ❌ | Entity-specific timeline view |
| Security Events Log | `CRM.Frontend/src/pages/SecurityEventsPage.tsx` | ❌ | Login, permission, API events |
| GDPR Data Export | `CRM.Frontend/src/pages/GDPRDataExportPage.tsx` | ❌ | Data subject request workflow |

### 2.2 Components

| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| AuditLogTable | `CRM.Frontend/src/components/audit/AuditLogTable.tsx` | ❌ | Paginated table with sorting/filtering |
| ChangeTimeline | `CRM.Frontend/src/components/audit/ChangeTimeline.tsx` | ❌ | Timeline visualization of changes |
| FieldDiffViewer | `CRM.Frontend/src/components/audit/FieldDiffViewer.tsx` | ❌ | Before/after value comparison |
| AuditFilterPanel | `CRM.Frontend/src/components/audit/AuditFilterPanel.tsx` | ❌ | Advanced filtering (user, entity, date, action) |
| UserActionBadge | `CRM.Frontend/src/components/audit/UserActionBadge.tsx` | ❌ | Status badge for action type (Create/Update/Delete) |
| AuditExportButton | `CRM.Frontend/src/components/audit/AuditExportButton.tsx` | ❌ | Export to CSV/PDF |
| DeletedEntityRestoreModal | `CRM.Frontend/src/components/audit/DeletedEntityRestoreModal.tsx` | ❌ | Preview and restore deleted entity |

### 2.3 Services (API Client)

| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| auditService | `CRM.Frontend/src/services/auditService.ts` | GetAuditLogs, GetEntityHistory, ExportAuditLogs, ExportGDPRData, GetDeletedEntities, RestoreEntity | ❌ |

### 2.4 Frontend Validations

| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Date Range | fromDate <= toDate, max range 90 days | Frontend/Backend | ❌ |
| Entity Type | Must be valid entity name from enum | Frontend/Backend | ❌ |
| User Filter | Must exist in system | Backend | ❌ |
| Export Format | CSV or PDF only | Frontend | ❌ |
| GDPR Data Request | Subject email verified, 30-day limit | Backend | ❌ |

### 2.5 UI/UX Specifications

#### Audit Log Viewer Layout
```
┌─────────────────────────────────────────────────────────────┐
│ Audit Log Viewer                                  [Export]   │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│ Filters:                                                     │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐        │
│ │ User     │ │Entity    │ │Action    │ │Date Range│        │
│ │ ▼        │ │ ▼        │ │ ▼        │ │ ▼        │        │
│ └──────────┘ └──────────┘ └──────────┘ └──────────┘        │
│                                                   [Search]   │
├─────────────────────────────────────────────────────────────┤
│ Timestamp         │ User        │ Entity      │ Action       │
├───────────────────┼─────────────┼─────────────┼──────────────┤
│ 2026-02-14 14:32 │ admin       │ Account:123 │ Updated      │
│ 2026-02-14 14:30 │ john.smith  │ Lead:456    │ Created      │
│ 2026-02-14 14:28 │ system      │ Setting:1   │ Modified     │
│                                                              │
│ Page: 1 / 45    [Prev] [Next]                              │
└─────────────────────────────────────────────────────────────┘
```

#### Change History Timeline
```
┌─────────────────────────────────────────────────────────────┐
│ Change History: Account #ACC-001 - Acme Corp               │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│ ●─── 2026-02-14 14:32 - john.smith [Updated]               │
│ │     Changed: Status "New" → "Active"                      │
│ │     [View Details] [Rollback?]                            │
│ │                                                            │
│ ●─── 2026-02-14 14:00 - admin [Created]                     │
│ │     New account created                                   │
│ │     [View All Fields]                                     │
│ │                                                            │
│ ●─── 2026-02-13 09:15 - system [Auto-Created]               │
│      From Lead #LEAD-789                                    │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. Backend Implementation

### 3.1 Entities

| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| AuditLog | `CRM.Core/Entities/AuditLog.cs` | ⚠️ Partial | Core audit entity with JSON change tracking |
| AuditLogEntry | `CRM.Core/Entities/AuditLogEntry.cs` | ❌ | Individual field-level changes |
| DeletedEntitySnapshot | `CRM.Core/Entities/DeletedEntitySnapshot.cs` | ❌ | Full JSON snapshot of deleted entity |
| SecurityEvent | `CRM.Core/Entities/SecurityEvent.cs` | ⚠️ Partial | Login, permission, API security events |
| DataAccessLog | `CRM.Core/Entities/DataAccessLog.cs` | ❌ | GDPR Article 15 - who accessed what data |

### 3.2 DTOs

| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| AuditLogDto | `CRM.Core/DTOs/AuditLogDto.cs` | ⚠️ Partial | API response DTO |
| AuditFilterDto | `CRM.Core/DTOs/AuditFilterDto.cs` | ❌ | Filter parameters |
| FieldChangeDto | `CRM.Core/DTOs/FieldChangeDto.cs` | ❌ | Before/after field value |
| EntityChangeHistoryDto | `CRM.Core/DTOs/EntityChangeHistoryDto.cs` | ❌ | Complete entity change timeline |
| GDPRDataExportDto | `CRM.Core/DTOs/GDPRDataExportDto.cs` | ❌ | GDPR data subject request export |

### 3.3 Interfaces

| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| IAuditLogService | `CRM.Core/Interfaces/IAuditLogService.cs` | 12+ | ❌ |
| IAuditingMiddleware | `CRM.Core/Interfaces/IAuditingMiddleware.cs` | 3 | ❌ |
| IChangeTracker | `CRM.Core/Interfaces/IChangeTracker.cs` | 5 | ❌ |

### 3.4 Services

| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| AuditLogService | `CRM.Infrastructure/Services/AuditLogService.cs` | GetAuditLogs, GetEntityHistory, LogSecurityEvent, CreateGDPRExport, RestoreDeleted, ArchiveOldLogs | ❌ |
| ChangeTrackingService | `CRM.Infrastructure/Services/ChangeTrackingService.cs` | CaptureChanges, GetFieldChanges, DetectCircularReferences | ❌ |
| SecurityEventService | `CRM.Infrastructure/Services/SecurityEventService.cs` | LogLogin, LogPermissionChange, LogApiCall, LogDataAccess | ⚠️ Partial |
| AuditExportService | `CRM.Infrastructure/Services/AuditExportService.cs` | ExportCsv, ExportPdf, ExportJson, GenerateReport | ❌ |

### 3.5 Controllers

| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| AuditController | `CRM.Api/Controllers/AuditController.cs` | 8 | ❌ |

### 3.6 API Endpoints

| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/audit/logs` | GetAuditLogs | Admin, Compliance | ❌ |
| GET | `/api/audit/logs/{id}` | GetAuditLog | Admin, Compliance | ❌ |
| GET | `/api/audit/entity/{entityType}/{entityId}/history` | GetEntityChangeHistory | Owner, Admin | ❌ |
| GET | `/api/audit/entity/{entityType}/{entityId}/timeline` | GetEntityTimeline | Owner, Admin | ❌ |
| GET | `/api/audit/deleted-entities` | GetDeletedEntities | Admin | ❌ |
| POST | `/api/audit/restore/{deletedEntityId}` | RestoreDeletedEntity | Admin | ❌ |
| GET | `/api/audit/security-events` | GetSecurityEvents | Admin, Security | ⚠️ Partial |
| POST | `/api/audit/export` | ExportAuditLogs | Admin, Compliance | ❌ |
| POST | `/api/audit/gdpr-data-export` | CreateGDPRDataExport | Data Subject, Admin | ❌ |
| GET | `/api/audit/gdpr-data-export/{requestId}` | GetGDPRDataExport | Data Subject, Admin | ❌ |

### 3.7 Backend Validations

| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| EntityType | Valid enum value (Account, Contact, Lead, etc.) | Entity/DTO | ❌ |
| UserId | Must exist in Users table | Entity | ❌ |
| OldValues | JSON serializable | Service | ❌ |
| NewValues | JSON serializable | Service | ❌ |
| Action | Valid enum (Create, Update, Delete, Export, SecurityEvent) | Entity/DTO | ❌ |
| DateRange | FromDate <= ToDate, max 90 days | Service | ❌ |
| Reason | Required for Delete action | Service | ❌ |

### 3.8 Middleware & Interceptors

#### Change Tracking Middleware
```csharp
// Location: CRM.Infrastructure/Middleware/AuditLoggingMiddleware.cs
// Captures HTTP request/response for audit trail
// Records: Method, Path, StatusCode, UserId, Timestamp, RequestBody (sanitized)
```

#### EF Core Change Interceptor
```csharp
// Location: CRM.Infrastructure/Data/AuditChangeInterceptor.cs
// Intercepts DbContext.SaveChanges to capture entity changes
// Tracks: Added, Modified, Deleted entities with field-level changes
```

#### Automatic Audit Log Creation
```csharp
// Before SaveChanges:
foreach (var entry in context.ChangeTracker.Entries())
{
    if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
    {
        var auditLog = new AuditLog
        {
            EntityType = entry.Entity.GetType().Name,
            EntityId = GetEntityId(entry),
            UserId = CurrentUserId,
            Action = entry.State == EntityState.Deleted ? AuditAction.Delete : AuditAction.Update,
            OldValues = SerializeChanges(entry.OriginalValues),
            NewValues = SerializeChanges(entry.CurrentValues),
            Timestamp = DateTime.UtcNow,
            Reason = GetChangeReason(entry) // From HTTP context or explicit parameter
        };
        context.AuditLogs.Add(auditLog);
    }
}
```

### 3.9 Data Access Logging (GDPR Article 15)

#### Protected Fields Requiring Logging
- PII: FirstName, LastName, Email, Phone
- Financial: CreditLimit, PaymentHistory
- Health/Special: Any specially protected data per data classification
- Search Queries: Any search performed on protected fields

#### Logging Points
```csharp
// In service methods when accessing protected data:
public async Task<Account> GetAccountAsync(int accountId)
{
    var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
    
    // Log data access for GDPR compliance
    await _dataAccessLogger.LogAccessAsync(
        entityType: "Account",
        entityId: accountId,
        userId: _currentUserId,
        accessType: "Read",
        fields: new[] { "FirstName", "LastName", "Email" },
        purpose: "Customer inquiry"
    );
    
    return account;
}
```

---

## 4. Database Implementation

### 4.1 Tables

| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| AuditLogs | `database/schema/009_audit_logging.sql` | ⚠️ Partial | Core audit table |
| DeletedEntitySnapshots | `database/schema/009_audit_logging.sql` | ❌ | Full JSON snapshot of deleted entity |
| DataAccessLogs | `database/schema/009_audit_logging.sql` | ❌ | GDPR Article 15 compliance |
| SecurityEvents | `database/schema/009_audit_logging.sql` | ⚠️ Partial | Login/permission/API events |

### 4.2 AuditLogs Table Structure

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ✅ |
| EntityType | VARCHAR(100) | No | - | Index | EntityType | ✅ |
| EntityId | INT | No | - | Index | EntityId | ✅ |
| UserId | INT | Yes | NULL | FK (Users.Id) | UserId | ✅ |
| Action | VARCHAR(20) | No | - | Check IN (Create,Update,Delete,Export,SecurityEvent) | Action | ✅ |
| OldValues | LONGTEXT | Yes | NULL | JSON | OldValues | ⚠️ Partial |
| NewValues | LONGTEXT | Yes | NULL | JSON | NewValues | ⚠️ Partial |
| Reason | VARCHAR(500) | Yes | NULL | - | Reason | ⚠️ Partial |
| IpAddress | VARCHAR(50) | Yes | NULL | - | IpAddress | ❌ |
| UserAgent | VARCHAR(500) | Yes | NULL | - | UserAgent | ❌ |
| Timestamp | DATETIME(6) | No | CURRENT_TIMESTAMP(6) | Index | Timestamp | ✅ |
| IsDeleted | TINYINT(1) | No | 0 | - | IsDeleted | ✅ |
| RowVersion | BINARY(8) | No | - | - | RowVersion | ✅ |

### 4.3 DeletedEntitySnapshots Table Structure

| Column | Data Type | Nullable | Default | Constraints | Status |
|--------|-----------|----------|---------|-------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | ✅ |
| EntityType | VARCHAR(100) | No | - | Index | ✅ |
| EntityId | INT | No | - | Composite Index (EntityType, EntityId) | ✅ |
| EntityJson | LONGTEXT | No | - | JSON snapshot | ❌ |
| DeletedBy | INT | Yes | NULL | FK (Users.Id) | ❌ |
| DeletedAt | DATETIME(6) | No | CURRENT_TIMESTAMP(6) | - | ❌ |
| RestoreCount | INT | No | 0 | - | ❌ |
| LastRestoredAt | DATETIME(6) | Yes | NULL | - | ❌ |
| IsDeleted | TINYINT(1) | No | 0 | - | ✅ |

### 4.4 DataAccessLogs Table Structure (GDPR Article 15)

| Column | Data Type | Nullable | Default | Constraints | Status |
|--------|-----------|----------|---------|-------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | ❌ |
| EntityType | VARCHAR(100) | No | - | Index | ❌ |
| EntityId | INT | No | - | Index | ❌ |
| UserId | INT | Yes | NULL | FK (Users.Id), Index | ❌ |
| AccessType | VARCHAR(20) | No | - | Check IN (Read,Export,Delete) | ❌ |
| FieldsAccessed | TEXT | Yes | NULL | JSON array | ❌ |
| Purpose | VARCHAR(500) | Yes | NULL | - | ❌ |
| AccessedAt | DATETIME(6) | No | CURRENT_TIMESTAMP(6) | Index | ❌ |
| DurationMs | INT | Yes | NULL | - | ❌ |

### 4.5 SecurityEvents Table Structure

| Column | Data Type | Nullable | Default | Constraints | Status |
|--------|-----------|----------|---------|-------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | ✅ |
| UserId | INT | Yes | NULL | FK (Users.Id), Index | ⚠️ Partial |
| EventType | VARCHAR(50) | No | - | Check IN (LoginSuccess,LoginFailed,PermissionDenied,Export,APIRateLimit,PasswordReset) | ⚠️ Partial |
| Description | VARCHAR(500) | Yes | NULL | - | ⚠️ Partial |
| IpAddress | VARCHAR(50) | Yes | NULL | - | ❌ |
| UserAgent | VARCHAR(500) | Yes | NULL | - | ❌ |
| SeverityLevel | INT | No | 0 | 0=Info, 1=Warning, 2=Critical | ❌ |
| Timestamp | DATETIME(6) | No | CURRENT_TIMESTAMP(6) | Index | ✅ |
| IsDeleted | TINYINT(1) | No | 0 | - | ✅ |

### 4.6 Relationships

| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| AuditLogs | Users | N:1 | UserId | ✅ |
| AuditLogs | All Entities (polymorphic) | N:1 | (EntityType, EntityId) | ⚠️ Partial |
| DeletedEntitySnapshots | Users | N:1 | DeletedBy | ❌ |
| DataAccessLogs | Users | N:1 | UserId | ❌ |
| SecurityEvents | Users | N:1 | UserId | ⚠️ Partial |

### 4.7 Indexes

| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_AuditLogs_EntityType_EntityId | AuditLogs | EntityType, EntityId | NonClustered | ✅ |
| IX_AuditLogs_UserId_Timestamp | AuditLogs | UserId, Timestamp DESC | NonClustered | ⚠️ |
| IX_AuditLogs_Timestamp | AuditLogs | Timestamp DESC | NonClustered | ⚠️ |
| IX_AuditLogs_Action | AuditLogs | Action | NonClustered | ❌ |
| IX_DeletedEntitySnapshots_EntityType_EntityId | DeletedEntitySnapshots | EntityType, EntityId | NonClustered | ❌ |
| IX_DataAccessLogs_UserId_AccessedAt | DataAccessLogs | UserId, AccessedAt DESC | NonClustered | ❌ |
| IX_SecurityEvents_UserId_Timestamp | SecurityEvents | UserId, Timestamp DESC | NonClustered | ⚠️ |
| IX_SecurityEvents_EventType | SecurityEvents | EventType | NonClustered | ❌ |

### 4.8 Partitioning Strategy (Optional for Large Deployments)

```sql
-- Partition AuditLogs by month for efficient retention
-- Monthly partitions: AuditLogs_2026_02, AuditLogs_2026_01, etc.
-- Allows fast deletion of old partitions
```

---

## 5. Test Coverage

### 5.1 Unit Tests

| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| AuditLogServiceTests | `CRM.Tests/Services/AuditLogServiceTests.cs` | 24 | ❌ |
| ChangeTrackingServiceTests | `CRM.Tests/Services/ChangeTrackingServiceTests.cs` | 18 | ❌ |
| SecurityEventServiceTests | `CRM.Tests/Services/SecurityEventServiceTests.cs` | 12 | ❌ |
| AuditExportServiceTests | `CRM.Tests/Services/AuditExportServiceTests.cs` | 10 | ❌ |

### 5.2 Integration Tests

| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| AuditChangeInterceptorIntegrationTests | `CRM.Tests/Integration/AuditChangeInterceptorIntegrationTests.cs` | 15 | ❌ |
| GDPRDataAccessLoggingIntegrationTests | `CRM.Tests/Integration/GDPRDataAccessLoggingIntegrationTests.cs` | 12 | ❌ |
| DeletedEntityRestoreIntegrationTests | `CRM.Tests/Integration/DeletedEntityRestoreIntegrationTests.cs` | 8 | ❌ |
| AuditLogQueryPerformanceTests | `CRM.Tests/Integration/AuditLogQueryPerformanceTests.cs` | 6 | ❌ |

### 5.3 E2E Tests

| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| Audit Logging E2E | `e2e-tests/tests/audit/audit-logging.spec.ts` | 10 | ❌ |
| GDPR Data Export E2E | `e2e-tests/tests/audit/gdpr-export.spec.ts` | 8 | ❌ |
| Change History Timeline E2E | `e2e-tests/tests/audit/change-timeline.spec.ts` | 6 | ❌ |

### 5.4 Test Scenarios

#### Audit Trail Completeness
```csharp
[Fact]
public async Task WhenAccountUpdated_AllFieldChangesRecorded()
{
    // Given: Account with 5 properties
    // When: Update 3 properties
    // Then: AuditLog contains only changed properties with old/new values
    // And: Unchanged properties not recorded
    // And: Timestamp captures exact update moment
}

[Fact]
public async Task WhenEntityDeleted_FullSnapshotCaptured()
{
    // Given: Account with related contacts and opportunities
    // When: Account soft-deleted
    // Then: DeletedEntitySnapshot contains full account JSON
    // And: Reason and DeletedBy recorded
    // And: Restoration possible with snapshot
}
```

#### GDPR Data Access Logging
```csharp
[Fact]
public async Task WhenPIIFieldsAccessed_DataAccessLogRecorded()
{
    // Given: User queries contacts by email (PII field)
    // When: Query executed
    // Then: DataAccessLog entry created with:
    //   - UserId, Timestamp, AccessType=Read
    //   - FieldsAccessed: ["Email", "Phone"]
    //   - Purpose: "Customer inquiry"
}

[Fact]
public async Task WhenDataExported_AllAccessLogsIncluded()
{
    // Given: GDPR data export request
    // When: Export generated
    // Then: Report shows all data accessed by/for subject in date range
    // And: Each access timestamped, purpose recorded
    // And: Export cryptographically signed for audit trail
}
```

#### Deletion Tracking
```csharp
[Fact]
public async Task WhenDeletedEntityRestored_AuditTrailMaintained()
{
    // Given: Deleted account with snapshot
    // When: Account restored from snapshot
    // Then: New AuditLog entry records restoration
    // And: Restoration timestamp captured
    // And: RestoreCount incremented in snapshot
}
```

---

## 6. Inconsistencies & Issues

### 6.1 Data Type Mismatches

| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| AuditLog.Action | SecurityEvent.EventType | Action enum doesn't match EventType enum | TODO-AUDIT-01: Consolidate action enums |
| AuditLog.OldValues | Frontend display | JSON serialization differs between EF Core and display | TODO-AUDIT-02: Standardize JSON format |
| DataAccessLog.FieldsAccessed | Actual entity fields | May reference deleted/renamed fields | TODO-AUDIT-03: Add field name validation |

### 6.2 Missing Implementations

| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| Frontend Audit Log Viewer | `/pages/AuditLogViewerPage.tsx` | Core UI for compliance officers | TODO-AUDIT-04 |
| Change History Timeline Component | `/components/audit/ChangeTimeline.tsx` | Entity-specific change visualization | TODO-AUDIT-05 |
| GDPR Data Export Service | `AuditExportService.cs` | Compliance with Article 15 | TODO-AUDIT-06 |
| Data Access Logging Interceptor | `DataAccessLoggingInterceptor.cs` | Track PII field access | TODO-AUDIT-07 |
| Audit Log Archival Service | `AuditArchivalService.cs` | Long-term retention management | TODO-AUDIT-08 |
| Audit Log Performance Testing | `AuditLogQueryPerformanceTests.cs` | Verify queries < 1s with 1M+ records | TODO-AUDIT-09 |

### 6.3 Validation Gaps

| Field | Issue | Status |
|-------|-------|--------|
| Action enum | Missing "Restore" action for deleted entity restoration | TODO-AUDIT-10 |
| OldValues/NewValues | No validation that JSON is actually different (catches no-op updates) | TODO-AUDIT-11 |
| Purpose field | Optional but should be required for SecurityEvents and DataAccessLogs | TODO-AUDIT-12 |
| Reason field | Only required for Delete, should track reason for other sensitive actions (Export, PermissionChange) | TODO-AUDIT-13 |
| IpAddress | Not captured, required for security investigation | TODO-AUDIT-14 |

### 6.4 Performance Issues

| Issue | Impact | Mitigation | Status |
|-------|--------|-----------|--------|
| Audit logs grow indefinitely | Query performance degrades, storage costs increase | Implement retention policy + archival | TODO-AUDIT-15 |
| JSON serialization on every SaveChanges | Can add 5-10% overhead to database operations | Batch serialization, async logging | TODO-AUDIT-16 |
| Full-text search on OldValues/NewValues | Queries may be slow with LONGTEXT fields | Use separate searchable index table | TODO-AUDIT-17 |
| Concurrent audit log inserts | Lock contention with high-traffic applications | Partitioned table or separate audit database | TODO-AUDIT-18 |

### 6.5 Security Concerns

| Concern | Mitigation | Status |
|---------|-----------|--------|
| Audit logs contain sensitive data (emails, phone numbers in OldValues/NewValues) | Implement PII masking in exports, restrict viewer access to Admin/Compliance | TODO-AUDIT-19 |
| Audit logs could be tampered with post-creation | Cryptographic signing of audit records, separate immutable archive | TODO-AUDIT-20 |
| GDPR data export could contain PII of other subjects | Strict filtering by UserId and entity ownership during export | TODO-AUDIT-21 |
| Deleted entity snapshots expose sensitive data | Encrypt snapshots, restrict to Admin only | TODO-AUDIT-22 |

---

## 7. TODO Items (→ Master TODO List)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-AUDIT-01 | Consolidate AuditLog.Action and SecurityEvent.EventType enums | P1 | Design |
| TODO-AUDIT-02 | Standardize JSON serialization format for OldValues/NewValues between EF Core and frontend | P1 | Design |
| TODO-AUDIT-03 | Add field name validation to prevent dangling references in DataAccessLog.FieldsAccessed | P2 | Validation |
| TODO-AUDIT-04 | Implement frontend Audit Log Viewer page with filters (user, entity, action, date range) | P1 | Frontend |
| TODO-AUDIT-05 | Implement Change Timeline component for entity-specific change visualization | P2 | Frontend |
| TODO-AUDIT-06 | Implement AuditExportService with CSV, PDF, JSON export formats for GDPR Article 15 | P1 | Backend |
| TODO-AUDIT-07 | Implement DataAccessLoggingInterceptor to track PII field access automatically | P1 | Backend |
| TODO-AUDIT-08 | Implement AuditArchivalService with retention policy (7 years for GDPR) and monthly archival | P2 | Backend |
| TODO-AUDIT-09 | Create AuditLogQueryPerformanceTests to verify queries execute < 1s with 1M+ records | P2 | Testing |
| TODO-AUDIT-10 | Add "Restore" action to AuditAction enum and AuditLog.Action validation | P1 | Database |
| TODO-AUDIT-11 | Add validation that OldValues != NewValues to catch and skip no-op updates | P2 | Validation |
| TODO-AUDIT-12 | Make Purpose field required in SecurityEvents and DataAccessLogs (currently optional) | P1 | Validation |
| TODO-AUDIT-13 | Extend Reason field requirement to Export and PermissionChange actions (currently Delete only) | P2 | Validation |
| TODO-AUDIT-14 | Add IpAddress and UserAgent capture to AuditLog, SecurityEvent tables for security investigation | P1 | Database |
| TODO-AUDIT-15 | Implement audit log retention policy (7 years for GDPR, configurable) with automatic archival | P1 | Infrastructure |
| TODO-AUDIT-16 | Optimize JSON serialization by batching, async logging, and caching serialized values | P2 | Performance |
| TODO-AUDIT-17 | Create separate AuditLogSearchIndex table for efficient full-text search on OldValues/NewValues | P3 | Performance |
| TODO-AUDIT-18 | Implement table partitioning (by month) or separate audit database for high-concurrency environments | P3 | Performance |
| TODO-AUDIT-19 | Implement PII masking in audit exports (mask emails, phone numbers, SSN) with audit logging of export access | P1 | Security |
| TODO-AUDIT-20 | Implement cryptographic signing of audit records and separate immutable audit archive | P2 | Security |
| TODO-AUDIT-21 | Add strict filtering in GDPR data export to prevent cross-subject data leakage | P1 | Security |
| TODO-AUDIT-22 | Encrypt DeletedEntitySnapshots at rest and restrict access to Admin role only | P1 | Security |
| TODO-AUDIT-23 | Create AuditLogService interface and implementation with 15+ methods (Get, Filter, Export, Restore) | P1 | Backend |
| TODO-AUDIT-24 | Implement AuditLogController with 8 endpoints: GET /api/audit/logs, POST /api/audit/export, etc. | P1 | Backend |
| TODO-AUDIT-25 | Create E2E tests for complete audit logging flow (create → read → export) | P2 | Testing |
| TODO-AUDIT-26 | Create E2E tests for GDPR data export workflow with verification | P2 | Testing |
| TODO-AUDIT-27 | Add audit logging configuration to appsettings.json with retention days, archive frequency | P2 | Configuration |
| TODO-AUDIT-28 | Create frontend service auditService.ts with methods: GetAuditLogs, GetEntityHistory, ExportAuditLogs | P1 | Frontend |
| TODO-AUDIT-29 | Create documentation for audit logging architecture, compliance aspects, query examples | P3 | Documentation |

---

## 8. Compliance & Regulatory Mapping

### GDPR Compliance

| GDPR Article | Requirement | Implementation | Status |
|--------------|-------------|-----------------|--------|
| Article 5 | Accountability principle | AuditLog records all data processing | ⚠️ Partial |
| Article 12-14 | Rights of data subject | GDPR data export generates subject access request | ❌ |
| Article 15 | Right of access | DataAccessLog tracks all data access by subject | ❌ |
| Article 17 | Right to erasure | DeletedEntitySnapshot tracks deletion with reason | ⚠️ Partial |
| Article 32 | Security of processing | Encryption, access controls, audit trail signed | ⚠️ Partial |

### SOC 2 Compliance

| Control | Requirement | Implementation | Status |
|---------|-------------|-----------------|--------|
| CC7.2 | System monitoring | SecurityEvents logs all authentication/authorization | ⚠️ Partial |
| CC8.1 | Change management | AuditLog tracks all configuration changes | ⚠️ Partial |
| L1.1 | Logical access control | DataAccessLog enforces principle of least privilege | ❌ |
| L1.2 | User authentication | SecurityEvent logs all login attempts (success/failure) | ⚠️ Partial |

---

## 9. Implementation Phases

### Phase 1 (Week 1-2): Core Audit Infrastructure
- [ ] Create AuditLog entity and table
- [ ] Implement AuditChangeInterceptor for automatic change capture
- [ ] Create AuditLogService and AuditLogController
- [ ] Add basic unit tests

### Phase 2 (Week 3-4): GDPR Data Access Logging
- [ ] Create DataAccessLog table and entity
- [ ] Implement DataAccessLoggingInterceptor
- [ ] Track PII field access
- [ ] Create integration tests

### Phase 3 (Week 5-6): Frontend & Export
- [ ] Create Audit Log Viewer UI
- [ ] Implement Change Timeline component
- [ ] Create AuditExportService (CSV/PDF/JSON)
- [ ] Implement GDPR data export workflow

### Phase 4 (Week 7-8): Performance & Archival
- [ ] Implement retention policy
- [ ] Create archival service
- [ ] Add query performance tests
- [ ] Optimize JSON serialization

### Phase 5 (Week 9-10): Security Hardening
- [ ] Implement PII masking
- [ ] Add cryptographic signing
- [ ] Encrypt sensitive snapshots
- [ ] Security audit and penetration testing

---

## 10. Configuration Examples

### appsettings.json

```json
{
  "AuditLogging": {
    "Enabled": true,
    "CaptureMode": "All",  // All, Sensitive, Manual
    "RetentionDays": 2555,  // ~7 years for GDPR
    "ArchiveFrequencyDays": 30,
    "ArchivePath": "/var/archives/audit",
    "MaskPII": true,
    "SignRecords": true,
    "BatchSize": 100,
    "MaxConcurrentWrites": 4,
    "ExportFormat": "CSV",  // CSV, PDF, JSON
    "TimeZone": "UTC"
  }
}
```

---

## 11. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-02-14 | System | Initial specification - Audit Logging System with GDPR compliance, 29 TODO items extracted |

---


# Feature Specification

> **Spec ID:** SPEC-SYS-012  
> **Feature:** Role-Based Access Control (RBAC)  
> **Module:** System  
> **Version:** 1.0  
> **Last Updated:** February 14, 2026  
> **Status:** ⚠️ Partial

---

## 1. Business Context

### 1.1 Feature Description
Defines access control for CRM features using role levels and group-based permission flags. RBAC must support both backend authorization and frontend UI gating, ensuring a consistent and auditable permission model.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Role Levels | UserRole enum with role tiers | ✅ |
| SF-002 | Group Permissions | Menu + entity permission flags | ✅ |
| SF-003 | UI Gating | Hide/show features based on permissions | ⚠️ |
| SF-004 | API Authorization | Controller-level auth | ✅ |
| SF-005 | Permission Auditing | Track access changes | ❌ |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Admin grants group permissions | Admin | Group exists | Permissions updated | ✅ |
| UC-002 | User restricted from admin settings | User | Not in admin group | Access blocked | ⚠️ |
| UC-003 | System admin bypasses checks | Admin | IsSystemAdmin = true | Full access | ✅ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| App Shell | `CRM.Frontend/src/App.tsx` | ⚠️ | Route gating partial |
| Admin Settings | `CRM.Frontend/src/pages/admin/` | ⚠️ | Role gating incomplete |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| Navigation | `CRM.Frontend/src/components/Navigation.tsx` | ⚠️ | Uses group flags |
| Profile Context | `CRM.Frontend/src/contexts/ProfileContext.tsx` | ⚠️ | Permission state |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| apiClient | `CRM.Frontend/src/services/apiClient.ts` | get/post/put | ✅ |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Access Flags | Default false when missing | Frontend | ⚠️ |
| Role Mapping | Valid role enum | Frontend | ❌ |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| UserRole | `CRM.Backend/src/CRM.Core/Entities/User.cs` | ✅ | Role enum |
| UserGroup | `CRM.Backend/src/CRM.Core/Entities/UserGroup.cs` | ✅ | Permission flags |
| UserProfile | `CRM.Backend/src/CRM.Core/Entities/UserProfile.cs` | ✅ | Per-user preferences |

### 3.2 DTOs
| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| UserGroupDto | `CRM.Backend/src/CRM.Core/Dtos/UserGroupDto.cs` | ✅ | Permissions |

### 3.3 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| IUserGroupService | `CRM.Backend/src/CRM.Core/Interfaces/IUserGroupService.cs` | 8 | ✅ |

### 3.4 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| UserGroupService | `CRM.Backend/src/CRM.Infrastructure/Services/UserGroupService.cs` | 8 | ✅ |

### 3.5 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| UserGroupsController | `CRM.Backend/src/CRM.Api/Controllers/UserGroupsController.cs` | 8 | ✅ |
| UsersController | `CRM.Backend/src/CRM.Api/Controllers/UsersController.cs` | 13 | ✅ |

### 3.6 API Endpoints
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/usergroups` | GetAll | Yes | ✅ |
| PUT | `/api/usergroups/{id}` | Update | Yes | ✅ |
| GET | `/api/users/me/preferences` | GetPreferences | Yes | ✅ |

### 3.7 Backend Validations
| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| Role | Range 0-4 | Entity | ✅ |
| IsSystemAdmin | Bypass permission checks | Service | ✅ |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| Users | `database/schema/000_baseline_schema.sql` | ✅ | Role stored as int |
| UserGroups | `database/schema/000_baseline_schema.sql` | ✅ | Permission flags |

### 4.2 Data Elements
| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Role | INT | No | 0 | Range 0-4 | Role | ✅ |
| IsSystemAdmin | BOOLEAN | No | false | - | IsSystemAdmin | ✅ |

### 4.3 Relationships
| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| Users | UserGroups | N:1 | PrimaryGroupId | ✅ |

### 4.4 Indexes
| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_UserGroups_IsSystemAdmin | UserGroups | IsSystemAdmin | NonClustered | ⚠️ |

---

## 5. Test Coverage

### 5.1 Unit Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| UserGroupServiceTests | `CRM.Backend/tests/Services/UserGroupServiceTests.cs` | 12 | ✅ |

### 5.2 Integration Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| UserGroupsControllerTests | `CRM.Backend/tests/Controllers/UserGroupsControllerTests.cs` | 6 | ⚠️ |

### 5.3 E2E Tests
| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| RBAC Guards | `e2e-tests/tests/admin/rbac.spec.ts` | 0 | ❌ |

---

## 6. Inconsistencies & Issues

### 6.1 Data Type Mismatches
| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| Group flags | Frontend nav filtering | Flag list not normalized | TODO-SYS012-002 |

### 6.2 Missing Implementations
| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| Permission audit log | `CRM.Backend/src/CRM.Infrastructure` | Not implemented | TODO-SYS012-003 |

### 6.3 Validation Gaps
| Field | Issue | Status |
|-------|-------|--------|
| Role mapping | No centralized mapping config | TODO-SYS012-001 |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-SYS012-001 | Create centralized role/permission mapping for UI guards | P1 | System |
| TODO-SYS012-002 | Normalize group permission flags with navigation filtering | P2 | System |
| TODO-SYS012-003 | Add audit logging for RBAC permission changes | P2 | System |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | February 14, 2026 | GitHub Copilot | Initial specification |

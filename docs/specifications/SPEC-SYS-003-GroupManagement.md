# Feature Specification

> **Spec ID:** SPEC-SYS-003  
> **Feature:** Group Management  
> **Module:** System  
> **Version:** 1.0  
> **Last Updated:** February 14, 2026  
> **Status:** ⚠️ Partial

---

## 1. Business Context

### 1.1 Feature Description
Manages user groups and group membership, including group-level permissions, default group assignment, and visibility in navigation. Supports the primary security model for the CRM alongside role-based access control.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Group CRUD | Create/update/delete groups | ✅ |
| SF-002 | Membership | Add/remove users from groups | ✅ |
| SF-003 | Default Group | Mark a group as default | ⚠️ |
| SF-004 | Permissions | Assign access flags per group | ✅ |
| SF-005 | Audit Log | Track membership changes | ❌ |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Admin creates a group | Admin | Admin authenticated | Group created | ✅ |
| UC-002 | Admin assigns user to group | Admin | User + group exist | Membership created | ✅ |
| UC-003 | Admin configures group permissions | Admin | Group exists | Permissions updated | ✅ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| Group Management | `CRM.Frontend/src/pages/admin/GroupManagementPage.tsx` | ⚠️ | Partial UX |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| Group Management Tab | `CRM.Frontend/src/components/settings/GroupManagementTab.tsx` | ⚠️ | Permissions editor |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| apiClient | `CRM.Frontend/src/services/apiClient.ts` | get/post/put | ✅ |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Group Name | Required, unique | Frontend | ⚠️ |
| Permission Flags | Boolean set | Frontend | ⚠️ |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| UserGroup | `CRM.Backend/src/CRM.Core/Entities/UserGroup.cs` | ✅ | Group permissions |
| UserGroupMember | `CRM.Backend/src/CRM.Core/Entities/UserGroupMember.cs` | ✅ | Membership join table |

### 3.2 DTOs
| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| UserGroupDto | `CRM.Backend/src/CRM.Core/Dtos/UserGroupDto.cs` | ✅ | Group data |
| UserGroupMemberDto | `CRM.Backend/src/CRM.Core/Dtos/UserGroupMemberDto.cs` | ✅ | Member data |

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

### 3.6 API Endpoints
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/usergroups` | GetAll | Yes | ✅ |
| POST | `/api/usergroups` | Create | Yes | ✅ |
| GET | `/api/usergroups/{id}` | GetById | Yes | ✅ |
| PUT | `/api/usergroups/{id}` | Update | Yes | ✅ |
| DELETE | `/api/usergroups/{id}` | Delete | Yes | ✅ |
| GET | `/api/usergroups/{id}/members` | GetMembers | Yes | ✅ |
| POST | `/api/usergroups/{id}/members/{userId}` | AddMember | Yes | ✅ |
| DELETE | `/api/usergroups/{id}/members/{userId}` | RemoveMember | Yes | ✅ |

### 3.7 Backend Validations
| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| Group Name | Required + unique | Service | ✅ |
| IsSystemAdmin | Only one system admin group | Service | ⚠️ |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| UserGroups | `database/schema/000_baseline_schema.sql` | ✅ | Group metadata |
| UserGroupMembers | `database/schema/000_baseline_schema.sql` | ✅ | Group membership |

### 4.2 Data Elements
| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Name | VARCHAR | No | - | Unique | Name | ✅ |
| IsSystemAdmin | BOOLEAN | No | false | - | IsSystemAdmin | ✅ |

### 4.3 Relationships
| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| UserGroupMembers | Users | N:1 | UserId | ✅ |
| UserGroupMembers | UserGroups | N:1 | UserGroupId | ✅ |

### 4.4 Indexes
| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_UserGroups_Name | UserGroups | Name | NonClustered | ✅ |

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
| Group Management | `e2e-tests/tests/admin/groups.spec.ts` | 0 | ❌ |

---

## 6. Inconsistencies & Issues

### 6.1 Data Type Mismatches
| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| UserGroup.AccessibleMenuItems | Navigation config | JSON vs list mismatch | TODO-SYS003-002 |

### 6.2 Missing Implementations
| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| Membership audit log | `CRM.Backend/src/CRM.Infrastructure` | Not implemented | TODO-SYS003-003 |

### 6.3 Validation Gaps
| Field | Issue | Status |
|-------|-------|--------|
| Default group uniqueness | Not enforced | TODO-SYS003-001 |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-SYS003-001 | Enforce single default group rule | P2 | System |
| TODO-SYS003-002 | Normalize AccessibleMenuItems with navigation config | P2 | System |
| TODO-SYS003-003 | Add membership audit logs | P3 | System |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | February 14, 2026 | GitHub Copilot | Initial specification |

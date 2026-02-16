# Feature Specification

> **Spec ID:** SPEC-SYS-001  
> **Feature:** User Management  
> **Module:** System  
> **Version:** 1.0  
> **Last Updated:** February 14, 2026  
> **Status:** ⚠️ Partial

---

## 1. Business Context

### 1.1 Feature Description
Provides CRUD operations for users, profile management, password management, and administrative user lifecycle actions (activation, reset, linking contacts). Ensures alignment with authentication, auditing, and permission enforcement.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | User CRUD | Create/update/deactivate users | ✅ |
| SF-002 | Password Management | Reset, change, password policies | ✅ |
| SF-003 | Profile Management | User profiles/preferences | ✅ |
| SF-004 | Admin UI | User management screens | ⚠️ |
| SF-005 | Audit Log | Track user changes | ❌ |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Admin creates a user | Admin | Admin authenticated | User created + email set | ✅ |
| UC-002 | User changes password | User | Logged in | Password updated | ✅ |
| UC-003 | Admin views user list | Admin | Admin authenticated | Users list rendered | ⚠️ |
| UC-004 | Admin links user to contact | Admin | Contact exists | User linked to contact | ✅ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| User Management | `CRM.Frontend/src/pages/UserManagementPage.tsx` | ⚠️ | Partial coverage |
| Admin User Settings | `CRM.Frontend/src/pages/admin/UserManagementSettingsPage.tsx` | ⚠️ | Admin-focused UI |
| User Approval | `CRM.Frontend/src/pages/admin/UserApprovalPage.tsx` | ⚠️ | Pending workflow |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| User Management Tab | `CRM.Frontend/src/components/settings/UserManagementTab.tsx` | ⚠️ | Admin settings tab |
| User Approval Tab | `CRM.Frontend/src/components/settings/UserApprovalTab.tsx` | ⚠️ | Approval UX |
| User Settings Dialog | `CRM.Frontend/src/components/UserSettingsDialog.tsx` | ⚠️ | Partial validations |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| apiClient | `CRM.Frontend/src/services/apiClient.ts` | get/post/put | ✅ |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Email | Required, valid email | Frontend | ⚠️ |
| Password | Min length + complexity | Both | ⚠️ |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| User | `CRM.Backend/src/CRM.Core/Entities/User.cs` | ✅ | Core user entity |
| UserProfile | `CRM.Backend/src/CRM.Core/Entities/UserProfile.cs` | ✅ | Preferences + permissions |

### 3.2 DTOs
| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| UserDto | `CRM.Backend/src/CRM.Core/Dtos/UserDto.cs` | ✅ | User response |
| UserProfileDto | `CRM.Backend/src/CRM.Core/Dtos/UserProfileDto.cs` | ✅ | Profile response |

### 3.3 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| IUserService | `CRM.Backend/src/CRM.Core/Interfaces/IUserService.cs` | 10 | ✅ |

### 3.4 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| UserService | `CRM.Backend/src/CRM.Infrastructure/Services/UserService.cs` | 10 | ✅ |
| AuthenticationService | `CRM.Backend/src/CRM.Infrastructure/Services/AuthenticationService.cs` | 18 | ✅ |

### 3.5 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| UsersController | `CRM.Backend/src/CRM.Api/Controllers/UsersController.cs` | 13 | ✅ |
| UserProfilesController | `CRM.Backend/src/CRM.Api/Controllers/UserProfilesController.cs` | 7 | ✅ |

### 3.6 API Endpoints
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/users` | GetAll | Yes | ✅ |
| GET | `/api/users/{id}` | GetById | Yes | ✅ |
| POST | `/api/users` | Create | Yes | ✅ |
| PUT | `/api/users/{id}` | Update | Yes | ✅ |
| DELETE | `/api/users/{id}` | Delete | Yes | ✅ |
| GET | `/api/userprofiles` | GetAll | Yes | ✅ |
| POST | `/api/userprofiles` | Create | Yes | ✅ |

### 3.7 Backend Validations
| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| Email | Required + unique | Service | ✅ |
| Password | BCrypt hash required | Service | ✅ |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| Users | `database/schema/000_baseline_schema.sql` | ✅ | Core users |
| UserProfiles | `database/schema/000_baseline_schema.sql` | ✅ | Preferences |

### 4.2 Data Elements
| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Email | VARCHAR | No | - | Unique | Email | ✅ |
| PasswordHash | VARCHAR | No | - | - | PasswordHash | ✅ |

### 4.3 Relationships
| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| Users | UserProfiles | N:1 | UserProfileId | ✅ |

### 4.4 Indexes
| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_Users_Email | Users | Email | NonClustered | ✅ |

---

## 5. Test Coverage

### 5.1 Unit Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| UserServiceTests | `CRM.Backend/tests/Services/UserServiceTests.cs` | 12 | ✅ |
| AuthenticationServiceTests | `CRM.Backend/tests/Services/AuthenticationServiceTests.cs` | 14 | ✅ |

### 5.2 Integration Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| UsersControllerTests | `CRM.Backend/tests/Controllers/UsersControllerTests.cs` | 8 | ⚠️ |

### 5.3 E2E Tests
| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| User Management | `e2e-tests/tests/admin/users.spec.ts` | 0 | ❌ |

---

## 6. Inconsistencies & Issues

### 6.1 Data Type Mismatches
| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| User.Role | RBAC permissions | Role mapping not centralized | TODO-SYS001-003 |

### 6.2 Missing Implementations
| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| Audit log for user changes | `CRM.Backend/src/CRM.Infrastructure` | Not implemented | TODO-SYS001-002 |

### 6.3 Validation Gaps
| Field | Issue | Status |
|-------|-------|--------|
| Password policy | No shared frontend validator | TODO-SYS001-001 |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-SYS001-001 | Align frontend password validation with backend policy | P1 | System |
| TODO-SYS001-002 | Add audit logging for user create/update/delete | P2 | System |
| TODO-SYS001-003 | Centralize role-to-permission mapping for UI guards | P2 | System |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | February 14, 2026 | GitHub Copilot | Initial specification |

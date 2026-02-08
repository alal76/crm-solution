# Feature Specification Template

> **Spec ID:** SPEC-XXX  
> **Feature:** [Feature Name]  
> **Module:** [Module Name]  
> **Version:** 1.0  
> **Last Updated:** [Date]  
> **Status:** ✅ Implemented | ⚠️ Partial | ❌ Not Implemented

---

## 1. Business Context

### 1.1 Feature Description
[High-level business description of what this feature does]

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | [Name] | [Description] | ✅/⚠️/❌ |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | [Name] | [Actor] | [Precondition] | [Postcondition] | ✅/⚠️/❌ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| [PageName] | `CRM.Frontend/src/pages/[File].tsx` | ✅/❌ | |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| [ComponentName] | `CRM.Frontend/src/components/[File].tsx` | ✅/❌ | |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| [serviceName] | `CRM.Frontend/src/services/[file].ts` | [methods] | ✅/❌ |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| [fieldName] | [rule] | Frontend/Backend/Both | ✅/❌ |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| [EntityName] | `CRM.Core/Entities/[File].cs` | ✅/❌ | |

### 3.2 DTOs
| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| [DtoName] | `CRM.Core/DTOs/[File].cs` | ✅/❌ | |

### 3.3 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| [IServiceName] | `CRM.Core/Interfaces/[File].cs` | [count] | ✅/❌ |

### 3.4 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| [ServiceName] | `CRM.Infrastructure/Services/[File].cs` | [count] | ✅/❌ |

### 3.5 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| [ControllerName] | `CRM.Api/Controllers/[File].cs` | [count] | ✅/❌ |

### 3.6 API Endpoints
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/[resource]` | [MethodName] | Yes/No | ✅/❌ |

### 3.7 Backend Validations
| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| [fieldName] | [rule] | Entity/DTO/Service | ✅/❌ |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| [TableName] | `database/schema/[file].sql` | ✅/❌ | |

### 4.2 Data Elements
| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ✅ |
| [ColumnName] | [Type] | Yes/No | [Default] | [FK/UK/etc] | [PropertyName] | ✅/❌ |

### 4.3 Relationships
| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| [Table1] | [Table2] | 1:N / N:M | [FKColumn] | ✅/❌ |

### 4.4 Indexes
| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| [IndexName] | [Table] | [Columns] | Clustered/NonClustered | ✅/❌ |

---

## 5. Test Coverage

### 5.1 Unit Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| [TestClass] | `CRM.Tests/[File].cs` | [count] | ✅/❌ |

### 5.2 Integration Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| [TestClass] | `CRM.Tests/Integration/[File].cs` | [count] | ✅/❌ |

### 5.3 E2E Tests
| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| [SuiteName] | `e2e-tests/tests/[file].spec.ts` | [count] | ✅/❌ |

---

## 6. Inconsistencies & Issues

### 6.1 Data Type Mismatches
| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| [Entity.Property] | [DB.Column] | [Description] | [TODO/Fixed] |

### 6.2 Missing Implementations
| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| [Item] | [Path] | [Why missing] | TODO-XXX |

### 6.3 Validation Gaps
| Field | Issue | Status |
|-------|-------|--------|
| [fieldName] | [Missing validation] | TODO-XXX |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-XXX-001 | [Description] | P0/P1/P2/P3 | [Category] |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | [Date] | [Author] | Initial specification |


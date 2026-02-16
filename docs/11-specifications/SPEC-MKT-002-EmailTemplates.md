# Feature Specification: Email Templates

> **Spec ID:** SPEC-MKT-002  
> **Feature:** Email Templates  
> **Module:** Marketing  
> **Version:** 1.0  
> **Last Updated:** February 12, 2026  
> **Status:** ✅ Implemented

---

## 1. Business Context

### 1.1 Feature Description
Email template management for marketing campaigns, sales outreach, and transactional emails. Supports HTML/text templates with merge field placeholders, template categories, versioning, and A/B testing capabilities.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Template CRUD | Create, read, update, delete templates | ✅ Implemented |
| SF-002 | Template Categories | Sales, Marketing, Service, Transactional, Newsletter | ✅ Implemented |
| SF-003 | HTML Editor | Rich text editing with preview | ✅ Implemented |
| SF-004 | Plain Text Version | Auto-generate or manual text version | ✅ Implemented |
| SF-005 | Merge Fields | Dynamic placeholders for personalization | ✅ Implemented |
| SF-006 | Template Cloning | Duplicate templates for modifications | ⚠️ Partial |
| SF-007 | Version History | Track changes with rollback capability | ⚠️ Partial |
| SF-008 | Template Preview | Preview with sample data | ⚠️ Partial |
| SF-009 | Template Testing | Send test emails to verify | ⚠️ Partial |
| SF-010 | Template Sharing | Share templates across teams | ❌ Not Implemented |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Create Template | Marketer | Logged in | Template saved | ✅ |
| UC-002 | Edit HTML Content | Marketer | Template exists | HTML updated | ✅ |
| UC-003 | Add Merge Fields | Marketer | Editing template | Fields inserted | ✅ |
| UC-004 | Preview Template | Marketer | Template exists | Preview rendered | ⚠️ |
| UC-005 | Clone Template | Marketer | Template exists | Copy created | ⚠️ |
| UC-006 | View Version History | Marketer | Template has versions | History displayed | ⚠️ |
| UC-007 | Rollback Version | Marketer | Multiple versions exist | Previous restored | ⚠️ |
| UC-008 | Send Test Email | Marketer | Template complete | Test delivered | ⚠️ |
| UC-009 | Delete Template | Admin | Template unused | Template deleted | ✅ |
| UC-010 | Organize by Category | Marketer | Templates exist | Filtered view | ✅ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| EmailTemplatesPage | `CRM.Frontend/src/pages/EmailTemplatesPage.tsx` | ✅ | Template management |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| HTMLEditor | Inline or TinyMCE | ⚠️ | Rich text editing |
| TemplatePreview | - | ❌ | Not Found |
| MergeFieldPicker | - | ❌ | Not Found |

### 2.3 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| emailTemplateService | - | - | ❌ Not Found |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| name | Required, max 255 chars | Both | ✅ |
| subject | Required | Backend | ⚠️ |
| htmlBody | Required for HTML templates | Backend | ⚠️ |
| category | Required, valid enum | Both | ✅ |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| EmailTemplate | `CRM.Core/Entities/EmailTemplate.cs` | ✅ | ~165 lines |

### 3.2 Enums
| Enum | Values | File Path | Status |
|------|--------|-----------|--------|
| EmailTemplateCategory | Sales, Marketing, Service, Transactional, Newsletter, Onboarding, Support, Notification, Promotion, Reminder, FollowUp, Welcome, Survey | EmailTemplate.cs | ✅ |
| TemplateStatus | Draft, Active, Archived, Testing | (inferred) | ✅ |

### 3.3 Entity Properties - EmailTemplate
| Property | Type | Required | Default | Notes |
|----------|------|----------|---------|-------|
| Id | int | Yes | AUTO | Primary key |
| Name | string | Yes | - | Template name |
| Subject | string | Yes | - | Email subject line |
| HtmlBody | string | Yes | - | HTML content |
| TextBody | string | No | - | Plain text alternative |
| Category | EmailTemplateCategory | Yes | - | Template category |
| Description | string | No | - | Template description |
| MergeFields | string | No | - | JSON list of merge fields |
| PreheaderText | string | No | - | Email preheader |
| FromName | string | No | - | Sender name override |
| FromEmail | string | No | - | Sender email override |
| ReplyTo | string | No | - | Reply-to address |
| IsActive | bool | Yes | true | Active status |
| Version | int | Yes | 1 | Template version |
| ParentTemplateId | int? | No | - | For versioning/cloning |
| LastUsedAt | DateTime? | No | - | Last usage timestamp |
| UsageCount | int | Yes | 0 | Usage statistics |
| OwnerId | int? | No | - | Creator user |
| CreatedAt | DateTime | Yes | NOW | Created timestamp |
| UpdatedAt | DateTime? | No | - | Modified timestamp |
| IsDeleted | bool | Yes | false | Soft delete flag |

### 3.4 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| IEmailTemplateService | `CRM.Core/Interfaces/IEmailTemplateService.cs` | Multiple | ✅ |

### 3.5 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| EmailTemplateService | `CRM.Infrastructure/Services/EmailTemplateService.cs` | - | ✅ |

### 3.6 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| EmailTemplatesController | `CRM.Api/Controllers/EmailTemplatesController.cs` | - | ⚠️ Partial |

### 3.7 API Endpoints
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/emailtemplates` | GetAll | Yes | ✅ |
| GET | `/api/emailtemplates/{id}` | GetById | Yes | ✅ |
| GET | `/api/emailtemplates/category/{category}` | GetByCategory | Yes | ⚠️ |
| POST | `/api/emailtemplates` | Create | Yes | ✅ |
| PUT | `/api/emailtemplates/{id}` | Update | Yes | ✅ |
| DELETE | `/api/emailtemplates/{id}` | Delete | Yes | ✅ |
| POST | `/api/emailtemplates/{id}/clone` | Clone | Yes | ❌ |
| POST | `/api/emailtemplates/{id}/test` | SendTest | Yes | ❌ |
| GET | `/api/emailtemplates/{id}/preview` | Preview | Yes | ❌ |
| POST | `/api/emailtemplates/{id}/render` | Render | Yes | ❌ |

### 3.8 Backend Validations
| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| Name | Required | Service | ✅ |
| Subject | Required | Service | ✅ |
| HtmlBody | Required | Service | ✅ |
| Category | Valid enum value | Service | ✅ |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | Schema File | Status | Notes |
|------------|-------------|--------|-------|
| EmailTemplates | `database/schema/002_marketing_tables.sql` | ✅ | Template storage |

### 4.2 Data Elements
| Column | Data Type | Nullable | Default | Constraints | Status |
|--------|-----------|----------|---------|-------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | ✅ |
| Name | VARCHAR(255) | No | - | - | ✅ |
| Subject | VARCHAR(500) | No | - | - | ✅ |
| HtmlBody | LONGTEXT | No | - | - | ✅ |
| TextBody | LONGTEXT | Yes | NULL | - | ✅ |
| Category | INT | No | 0 | - | ✅ |
| Description | VARCHAR(1000) | Yes | NULL | - | ✅ |
| MergeFields | TEXT | Yes | NULL | JSON | ✅ |
| PreheaderText | VARCHAR(255) | Yes | NULL | - | ✅ |
| FromName | VARCHAR(100) | Yes | NULL | - | ✅ |
| FromEmail | VARCHAR(255) | Yes | NULL | - | ✅ |
| ReplyTo | VARCHAR(255) | Yes | NULL | - | ✅ |
| IsActive | BOOLEAN | No | TRUE | - | ✅ |
| Version | INT | No | 1 | - | ✅ |
| ParentTemplateId | INT | Yes | NULL | FK→EmailTemplates | ✅ |
| OwnerId | INT | Yes | NULL | FK→Users | ✅ |
| LastUsedAt | DATETIME | Yes | NULL | - | ✅ |
| UsageCount | INT | No | 0 | - | ✅ |
| CreatedAt | DATETIME | No | CURRENT_TIMESTAMP | - | ✅ |
| UpdatedAt | DATETIME | Yes | NULL | - | ✅ |
| IsDeleted | BOOLEAN | No | FALSE | - | ✅ |

### 4.3 Indexes
| Index Name | Columns | Type | Status |
|------------|---------|------|--------|
| IX_EmailTemplates_Category | Category | Non-clustered | ✅ |
| IX_EmailTemplates_IsActive | IsActive | Non-clustered | ✅ |
| IX_EmailTemplates_OwnerId | OwnerId | Non-clustered | ✅ |

---

## 5. Tests

### 5.1 Unit Tests
| Test Class | File Path | Test Count | Status |
|------------|-----------|------------|--------|
| EmailTemplateServiceTests | - | - | ❌ Not Found |

### 5.2 Integration Tests
| Test Class | File Path | Test Count | Status |
|------------|-----------|------------|--------|
| EmailTemplatesControllerTests | - | - | ❌ Not Found |

---

## 6. Known Issues

### 6.1 Validation Gaps
| Issue | Current State | Required State | Priority |
|-------|---------------|----------------|----------|
| No merge field validation | Fields not validated | Validate {{field}} syntax | Medium |
| No HTML sanitization | Raw HTML stored | Sanitize input | High |

---

## 7. TODO Items

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-MKT002-001 | Add template preview endpoint | P2 | Backend |
| TODO-MKT002-002 | Add template clone endpoint | P2 | Backend |
| TODO-MKT002-003 | Add send test endpoint | P2 | Backend |
| TODO-MKT002-004 | Create emailTemplateService.ts | P2 | Frontend |
| TODO-MKT002-005 | Add merge field picker component | P2 | Frontend |
| TODO-MKT002-006 | Add HTML sanitization | P1 | Security |
| TODO-MKT002-007 | Create unit tests | P2 | Testing |

---

## 8. Change History

| Date | Author | Changes |
|------|--------|---------|
| 2026-02-12 | System | Initial specification created |

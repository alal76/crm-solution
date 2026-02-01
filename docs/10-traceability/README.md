# Feature Traceability Matrix

> **Last Updated:** February 1, 2026 | **Version:** 1.7.28

This document provides end-to-end traceability from business functionality to implementation code, enabling complete understanding of how features are implemented across the stack.

---

## Table of Contents

1. [How to Use This Document](#1-how-to-use-this-document)
2. [Core Modules](#2-core-modules)
3. [Sales Modules](#3-sales-modules)
4. [Marketing Modules](#4-marketing-modules)
5. [Service Modules](#5-service-modules)
6. [Administration](#6-administration)
7. [Security Features](#7-security-features)

---

## 1. How to Use This Document

### 1.1 Traceability Format

Each feature follows this structure:

```
## Feature Name

### Business Description
What the feature does for end users

### Implementation Trace
| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| Entity | Model | path/to/file.cs | Data model |
| Service | Logic | path/to/service.cs | Business logic |
| API | Endpoint | path/to/controller.cs | REST API |
| Frontend | Page | path/to/page.tsx | UI |
| Database | Table | table_name | Storage |
| Tests | E2E | path/to/test.spec.ts | Test coverage |
```

### 1.2 File Path Conventions

| Prefix | Full Path |
|--------|-----------|
| `BE:` | `CRM.Backend/src/` |
| `FE:` | `CRM.Frontend/src/` |
| `DB:` | `database/schema/` |
| `E2E:` | `e2e-tests/tests/` |

---

## 2. Core Modules

### 2.1 Customer Management

#### Business Description
Manage B2B and B2C customer accounts with full contact information, relationship tracking, and activity history.

#### Implementation Trace

| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| **Entity** | Customer | `BE:CRM.Core/Entities/Account.cs` | Customer data model |
| **Entity** | Address | `BE:CRM.Core/Entities/Address.cs` | Address data |
| **Entity** | ContactInfo | `BE:CRM.Core/Entities/ContactInfoLink.cs` | Contact info links |
| **DTO** | CustomerDto | `BE:CRM.Core/DTOs/AccountDto.cs` | Transfer object |
| **Service** | CustomerService | `BE:CRM.Infrastructure/Services/CustomerService.cs` | Business logic |
| **Interface** | ICustomerService | `BE:CRM.Core/Interfaces/ICustomerService.cs` | Service contract |
| **Controller** | AccountsController | `BE:CRM.Api/Controllers/AccountsController.cs` | REST endpoints |
| **Page** | CustomersPage | `FE:pages/Customers/CustomersPage.tsx` | Customer list |
| **Page** | CustomerDetailPage | `FE:pages/Customers/CustomerDetailPage.tsx` | Customer detail |
| **Component** | CustomerForm | `FE:components/modules/customers/CustomerForm.tsx` | Create/edit form |
| **Service** | customerService | `FE:services/customerService.ts` | API calls |
| **Database** | Customers | `DB:001_core_tables.sql` | Table definition |
| **Tests** | E2E | `E2E:customers/customer-crud.spec.ts` | E2E tests |

#### API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/accounts` | List customers |
| GET | `/api/accounts/{id}` | Get customer |
| POST | `/api/accounts` | Create customer |
| PUT | `/api/accounts/{id}` | Update customer |
| DELETE | `/api/accounts/{id}` | Delete customer |

---

### 2.2 Contact Management

#### Business Description
Manage individual contacts linked to customers, with multiple emails, phones, and addresses.

#### Implementation Trace

| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| **Entity** | Contact | `BE:CRM.Core/Entities/ContactDetail.cs` | Contact model |
| **Entity** | CustomerContact | `BE:CRM.Core/Entities/AccountContact.cs` | Link table |
| **DTO** | ContactDto | `BE:CRM.Core/DTOs/ContactDto.cs` | Transfer object |
| **Service** | ContactService | `BE:CRM.Infrastructure/Services/ContactService.cs` | Business logic |
| **Controller** | ContactsController | `BE:CRM.Api/Controllers/ContactsController.cs` | REST endpoints |
| **Page** | ContactsPage | `FE:pages/Contacts/ContactsPage.tsx` | Contact list |
| **Page** | ContactDetailPage | `FE:pages/Contacts/ContactDetailPage.tsx` | Contact detail |
| **Database** | Contacts | `DB:001_core_tables.sql` | Table definition |
| **Tests** | E2E | `E2E:contacts/contact-crud.spec.ts` | E2E tests |

---

### 2.3 Contact Information (Consolidated)

#### Business Description
Unified contact info system supporting multiple emails, phones, and addresses per entity with normalization and verification.

#### Implementation Trace

| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| **Entity** | EmailAddress | `BE:CRM.Core/Entities/EmailAddress.cs` | Email model |
| **Entity** | PhoneNumber | `BE:CRM.Core/Entities/PhoneNumber.cs` | Phone model |
| **Entity** | Address | `BE:CRM.Core/Entities/Address.cs` | Address model |
| **Entity** | EntityEmailLink | `BE:CRM.Core/Entities/EntityEmailLink.cs` | Email link |
| **Entity** | EntityPhoneLink | `BE:CRM.Core/Entities/EntityPhoneLink.cs` | Phone link |
| **Entity** | EntityAddressLink | `BE:CRM.Core/Entities/EntityAddressLink.cs` | Address link |
| **Service** | ContactInfoService | `BE:CRM.Infrastructure/Services/ContactInfoService.cs` | Info management |
| **Controller** | ContactInfoController | `BE:CRM.Api/Controllers/ContactInfoController.cs` | REST endpoints |
| **Component** | ContactInfoCard | `FE:components/common/ContactInfoCard.tsx` | Info display |
| **Database** | Multiple tables | `DB:007_consolidated_contact_info_v2.sql` | Schema |

---

## 3. Sales Modules

### 3.1 Lead Management

#### Business Description
Capture and qualify sales leads with scoring, routing, and conversion to opportunities.

#### Implementation Trace

| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| **Entity** | Lead | `BE:CRM.Core/Entities/Lead.cs` | Lead model |
| **Entity** | LeadRoutingRule | `BE:CRM.Core/Entities/LeadRoutingRule.cs` | Routing rules |
| **DTO** | LeadDto | `BE:CRM.Core/DTOs/LeadDto.cs` | Transfer object |
| **Service** | LeadService | `BE:CRM.Infrastructure/Services/LeadService.cs` | Business logic |
| **Controller** | LeadsController | `BE:CRM.Api/Controllers/LeadsController.cs` | REST endpoints |
| **Page** | LeadsPage | `FE:pages/Leads/LeadsPage.tsx` | Lead list |
| **Component** | LeadPipeline | `FE:components/modules/leads/LeadPipeline.tsx` | Pipeline view |
| **Database** | Leads | `DB:001_core_tables.sql` | Table definition |
| **Tests** | E2E | `E2E:leads/lead-crud.spec.ts` | E2E tests |

---

### 3.2 Opportunity Management

#### Business Description
Track sales opportunities through pipeline stages with probability, value, and close date.

#### Implementation Trace

| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| **Entity** | Opportunity | `BE:CRM.Core/Entities/Opportunity.cs` | Opportunity model |
| **DTO** | OpportunityDto | `BE:CRM.Core/DTOs/OpportunityDto.cs` | Transfer object |
| **Service** | OpportunityService | `BE:CRM.Infrastructure/Services/OpportunityService.cs` | Business logic |
| **Controller** | OpportunitiesController | `BE:CRM.Api/Controllers/OpportunitiesController.cs` | REST endpoints |
| **Page** | OpportunitiesPage | `FE:pages/Opportunities/OpportunitiesPage.tsx` | Opportunity list |
| **Component** | PipelineBoard | `FE:components/modules/opportunities/PipelineBoard.tsx` | Kanban view |
| **Database** | Opportunities | `DB:004_products_opportunities.sql` | Table definition |
| **Tests** | E2E | `E2E:opportunities/opportunity-crud.spec.ts` | E2E tests |

---

### 3.3 Quote Management

#### Business Description
Create and manage price quotes with line items, discounts, and approval workflow.

#### Implementation Trace

| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| **Entity** | Quote | `BE:CRM.Core/Entities/Quote.cs` | Quote model |
| **Entity** | QuoteLineItem | `BE:CRM.Core/Entities/QuoteLineItem.cs` | Line items |
| **DTO** | QuoteDto | `BE:CRM.Core/DTOs/QuoteDto.cs` | Transfer object |
| **Service** | QuoteService | `BE:CRM.Infrastructure/Services/QuoteService.cs` | Business logic |
| **Controller** | QuotesController | `BE:CRM.Api/Controllers/QuotesController.cs` | REST endpoints |
| **Page** | QuotesPage | `FE:pages/Quotes/QuotesPage.tsx` | Quote list |
| **Page** | QuoteDetailPage | `FE:pages/Quotes/QuoteDetailPage.tsx` | Quote builder |
| **Database** | Quotes, QuoteLineItems | `DB:004_products_opportunities.sql` | Tables |
| **Tests** | E2E | `E2E:quotes/quote-crud.spec.ts` | E2E tests |

---

### 3.4 Product Catalog

#### Business Description
Manage product catalog with pricing, categories, and bundles.

#### Implementation Trace

| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| **Entity** | Product | `BE:CRM.Core/Entities/Product.cs` | Product model |
| **Entity** | ProductBundle | `BE:CRM.Core/Entities/ProductBundle.cs` | Bundles |
| **DTO** | ProductDto | `BE:CRM.Core/DTOs/ProductDto.cs` | Transfer object |
| **Service** | ProductService | `BE:CRM.Infrastructure/Services/ProductService.cs` | Business logic |
| **Controller** | ProductsController | `BE:CRM.Api/Controllers/ProductsController.cs` | REST endpoints |
| **Page** | ProductsPage | `FE:pages/Products/ProductsPage.tsx` | Product list |
| **Database** | Products, ProductBundles | `DB:004_products_opportunities.sql` | Tables |

---

## 4. Marketing Modules

### 4.1 Campaign Management

#### Business Description
Create and manage marketing campaigns across multiple channels with A/B testing and analytics.

#### Implementation Trace

| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| **Entity** | MarketingCampaign | `BE:CRM.Core/Entities/MarketingCampaign.cs` | Campaign model |
| **Entity** | CampaignRecipient | `BE:CRM.Core/Entities/CampaignRecipient.cs` | Recipients |
| **Entity** | CampaignMetric | `BE:CRM.Core/Entities/CampaignMetric.cs` | Metrics |
| **Entity** | CampaignABTest | `BE:CRM.Core/Entities/CampaignABTest.cs` | A/B tests |
| **DTO** | CampaignDto | `BE:CRM.Core/DTOs/CampaignDto.cs` | Transfer object |
| **Service** | CampaignService | `BE:CRM.Infrastructure/Services/CampaignService.cs` | CRUD operations |
| **Service** | CampaignExecutionService | `BE:CRM.Infrastructure/Services/CampaignExecutionService.cs` | Execution engine |
| **Controller** | CampaignsController | `BE:CRM.Api/Controllers/CampaignsController.cs` | CRUD endpoints |
| **Controller** | CampaignExecutionController | `BE:CRM.Api/Controllers/CampaignExecutionController.cs` | Execution endpoints |
| **Page** | CampaignsPage | `FE:pages/Campaigns/CampaignsPage.tsx` | Campaign list |
| **Page** | CampaignDetailPage | `FE:pages/Campaigns/CampaignDetailPage.tsx` | Campaign detail |
| **Component** | CampaignAnalytics | `FE:components/modules/campaigns/CampaignAnalytics.tsx` | Analytics view |
| **Database** | Campaigns | `DB:004_products_opportunities.sql` | Table |
| **Tests** | E2E | `E2E:campaigns/campaign-crud.spec.ts` | E2E tests |
| **Tests** | E2E | `E2E:campaigns/campaign-bugs.spec.ts` | Bug regression tests |

---

### 4.2 Email Templates

#### Business Description
Create and manage reusable email templates with variable substitution.

#### Implementation Trace

| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| **Entity** | EmailTemplate | `BE:CRM.Core/Entities/EmailTemplate.cs` | Template model |
| **Service** | EmailTemplateService | `BE:CRM.Infrastructure/Services/EmailTemplateService.cs` | Business logic |
| **Controller** | EmailTemplatesController | `BE:CRM.Api/Controllers/EmailTemplatesController.cs` | REST endpoints |
| **Page** | EmailTemplatesPage | `FE:pages/EmailTemplates/EmailTemplatesPage.tsx` | Template list |
| **Component** | TemplateEditor | `FE:components/modules/email/TemplateEditor.tsx` | WYSIWYG editor |

---

## 5. Service Modules

### 5.1 Service Request Management

#### Business Description
Track customer support tickets with categories, SLAs, and assignment.

#### Implementation Trace

| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| **Entity** | ServiceRequest | `BE:CRM.Core/Entities/ServiceRequest.cs` | Ticket model |
| **DTO** | ServiceRequestDto | `BE:CRM.Core/DTOs/ServiceRequestDto.cs` | Transfer object |
| **Service** | ServiceRequestService | `BE:CRM.Infrastructure/Services/ServiceRequestService.cs` | Business logic |
| **Controller** | ServiceRequestsController | `BE:CRM.Api/Controllers/ServiceRequestsController.cs` | REST endpoints |
| **Page** | ServiceRequestsPage | `FE:pages/ServiceRequests/ServiceRequestsPage.tsx` | Ticket list |
| **Database** | ServiceRequests | `DB:003_service_request_tables.sql` | Table |

---

### 5.2 Task Management

#### Business Description
Create and track user tasks with due dates, priorities, and assignments.

#### Implementation Trace

| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| **Entity** | CrmTask | `BE:CRM.Core/Entities/CrmTask.cs` | Task model |
| **DTO** | TaskDto | `BE:CRM.Core/DTOs/TaskDto.cs` | Transfer object |
| **Service** | TaskService | `BE:CRM.Infrastructure/Services/TaskService.cs` | Business logic |
| **Controller** | TasksController | `BE:CRM.Api/Controllers/TasksController.cs` | REST endpoints |
| **Page** | TasksPage | `FE:pages/Tasks/TasksPage.tsx` | Task list |
| **Database** | Tasks | `DB:006_activities_communication.sql` | Table |

---

### 5.3 Notes System

#### Business Description
Add notes to any entity with rich text and attachments.

#### Implementation Trace

| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| **Entity** | Note | `BE:CRM.Core/Entities/Note.cs` | Note model |
| **DTO** | NoteDto | `BE:CRM.Core/DTOs/NoteDto.cs` | Transfer object |
| **Service** | NoteService | `BE:CRM.Infrastructure/Services/NoteService.cs` | Business logic |
| **Controller** | NotesController | `BE:CRM.Api/Controllers/NotesController.cs` | REST endpoints |
| **Page** | NotesPage | `FE:pages/Notes/NotesPage.tsx` | Notes list |
| **Component** | NotesTab | `FE:components/common/NotesTab.tsx` | Entity notes tab |
| **Database** | Notes | `DB:006_activities_communication.sql` | Table |

---

## 6. Administration

### 6.1 User Management

#### Business Description
Create and manage system users with roles, permissions, and group memberships.

#### Implementation Trace

| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| **Entity** | User | `BE:CRM.Core/Entities/User.cs` | User model |
| **Entity** | UserGroup | `BE:CRM.Core/Entities/UserGroup.cs` | Group model |
| **Entity** | UserGroupMember | `BE:CRM.Core/Entities/UserGroupMember.cs` | Membership |
| **DTO** | UserDto | `BE:CRM.Core/DTOs/UserDto.cs` | Transfer object |
| **Service** | UserService | `BE:CRM.Infrastructure/Services/UserService.cs` | Business logic |
| **Service** | UserGroupService | `BE:CRM.Infrastructure/Services/UserGroupService.cs` | Group logic |
| **Controller** | UsersController | `BE:CRM.Api/Controllers/UsersController.cs` | REST endpoints |
| **Controller** | UserGroupsController | `BE:CRM.Api/Controllers/UserGroupsController.cs` | Group endpoints |
| **Page** | SettingsPage | `FE:pages/Settings/SettingsPage.tsx` | Settings container |
| **Tab** | UserManagementTab | `FE:pages/Settings/UserManagementTab.tsx` | User management |
| **Tab** | GroupManagementTab | `FE:pages/Settings/GroupManagementTab.tsx` | Group management |
| **Database** | Users, UserGroups | `DB:001_core_tables.sql` | Tables |

---

### 6.2 System Settings

#### Business Description
Configure system-wide settings including modules, themes, and security policies.

#### Implementation Trace

| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| **Entity** | SystemSettings | `BE:CRM.Core/Entities/SystemSettings.cs` | Settings model |
| **DTO** | SystemSettingsDto | `BE:CRM.Core/DTOs/SystemSettingsDto.cs` | Transfer object |
| **Service** | SystemSettingsService | `BE:CRM.Infrastructure/Services/SystemSettingsService.cs` | Settings logic |
| **Controller** | SystemSettingsController | `BE:CRM.Api/Controllers/SystemSettingsController.cs` | REST endpoints |
| **Tab** | GeneralSettingsTab | `FE:pages/Settings/GeneralSettingsTab.tsx` | General settings |
| **Tab** | SecuritySettingsTab | `FE:pages/Settings/SecuritySettingsTab.tsx` | Security settings |
| **Tab** | ModuleSettingsTab | `FE:pages/Settings/ModuleSettingsTab.tsx` | Module config |
| **Database** | SystemSettings | `DB:002_master_data_tables.sql` | Table |

---

### 6.3 Workflow Automation

#### Business Description
Create automated workflows with triggers, conditions, and actions.

#### Implementation Trace

| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| **Entity** | Workflow | `BE:CRM.Core/Entities/Workflow/Workflow.cs` | Workflow model |
| **Entity** | WorkflowStep | `BE:CRM.Core/Entities/Workflow/WorkflowStep.cs` | Steps |
| **Entity** | WorkflowTrigger | `BE:CRM.Core/Entities/Workflow/WorkflowTrigger.cs` | Triggers |
| **Entity** | WorkflowAction | `BE:CRM.Core/Entities/Workflow/WorkflowAction.cs` | Actions |
| **Service** | WorkflowService | `BE:CRM.Infrastructure/Services/WorkflowService.cs` | Workflow logic |
| **Controller** | WorkflowController | `BE:CRM.Api/Controllers/WorkflowController.cs` | REST endpoints |
| **Page** | WorkflowsPage | `FE:pages/Workflows/WorkflowsPage.tsx` | Workflow list |
| **Component** | WorkflowBuilder | `FE:components/modules/workflows/WorkflowBuilder.tsx` | Visual builder |
| **Database** | Workflows | `DB:005_workflow_tables.sql` | Tables |

---

## 7. Security Features

### 7.1 Authentication

#### Business Description
User login with JWT tokens, refresh tokens, and session management.

#### Implementation Trace

| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| **DTO** | LoginRequest | `BE:CRM.Core/DTOs/LoginRequest.cs` | Login request |
| **DTO** | AuthResponse | `BE:CRM.Core/DTOs/AuthResponse.cs` | Auth response |
| **Service** | AuthenticationService | `BE:CRM.Infrastructure/Services/AuthenticationService.cs` | Auth logic |
| **Service** | JwtTokenService | `BE:CRM.Infrastructure/Services/JwtTokenService.cs` | Token generation |
| **Controller** | AuthController | `BE:CRM.Api/Controllers/AuthController.cs` | Auth endpoints |
| **Page** | LoginPage | `FE:pages/Login/LoginPage.tsx` | Login form |
| **Context** | AuthContext | `FE:contexts/AuthContext.tsx` | Auth state |
| **Hook** | useAuth | `FE:hooks/useAuth.ts` | Auth hook |

---

### 7.2 Password Management (New in 1.7.28)

#### Business Description
Password complexity settings, expiration policies, and first-time setup.

#### Implementation Trace

| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| **Entity** | User (fields) | `BE:CRM.Core/Entities/User.cs` | Password fields |
| **Entity** | UserGroup (fields) | `BE:CRM.Core/Entities/UserGroup.cs` | Security policies |
| **Entity** | SystemSettings (fields) | `BE:CRM.Core/Entities/SystemSettings.cs` | Complexity settings |
| **DTO** | SetPasswordRequest | `BE:CRM.Core/DTOs/SetPasswordRequest.cs` | Password setup |
| **DTO** | PasswordRequirements | `BE:CRM.Core/DTOs/SetPasswordRequest.cs` | Requirements |
| **Service** | AuthenticationService | `BE:CRM.Infrastructure/Services/AuthenticationService.cs` | Password logic |
| **Controller** | AuthController | `BE:CRM.Api/Controllers/AuthController.cs` | Endpoints |
| **Page** | SetupPasswordPage | `FE:pages/SetupPassword/SetupPasswordPage.tsx` | Password setup |
| **Tab** | SecuritySettingsTab | `FE:pages/Settings/SecuritySettingsTab.tsx` | Complexity config |
| **Tab** | GroupManagementTab | `FE:pages/Settings/GroupManagementTab.tsx` | Group policies |
| **Database** | Users, UserGroups, SystemSettings | `DB:008_security_enhancements.sql` | Schema |

#### Password Policy Fields

**User Table:**
- `PasswordLastChangedAt` - Last password change
- `MustResetPassword` - Admin-forced reset
- `PasswordNeverSet` - First-time user
- `BackupCodes` - 2FA backup codes
- `PasswordResetToken` - Reset token
- `PasswordResetTokenExpiry` - Token expiry

**UserGroups Table:**
- `PasswordExpirationDays` - Days until expiry
- `PasswordExpirationPolicy` - 0=None, 1=MustChange, 2=Alert, 3=Warn
- `PasswordExpirationWarningDays` - Warning days
- `RequireTwoFactor` - 2FA suggested
- `EnforceTwoFactor` - 2FA mandatory

**SystemSettings Table:**
- `MinPasswordLength` - Minimum length
- `MaxPasswordLength` - Maximum length
- `RequireUppercase` - Uppercase required
- `RequireLowercase` - Lowercase required
- `RequireNumbers` - Numbers required
- `RequireSpecialChars` - Special chars required
- `DefaultPasswordExpirationDays` - Default expiry

---

### 7.3 Two-Factor Authentication

#### Business Description
Optional 2FA with TOTP and backup codes.

#### Implementation Trace

| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| **Entity** | User (fields) | `BE:CRM.Core/Entities/User.cs` | 2FA fields |
| **Service** | TwoFactorService | `BE:CRM.Infrastructure/Services/TwoFactorService.cs` | 2FA logic |
| **Controller** | AuthController | `BE:CRM.Api/Controllers/AuthController.cs` | 2FA endpoints |
| **Page** | TwoFactorSetupPage | `FE:pages/TwoFactor/TwoFactorSetupPage.tsx` | 2FA setup |

---

## Updating This Document

When adding new features:

1. Add a new section following the format above
2. List all implementation files
3. Include API endpoints
4. Reference database tables
5. Link to E2E tests

When modifying features:

1. Update the relevant section
2. Add new files if applicable
3. Update version date

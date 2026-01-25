# CRM Solution Architecture Overview

This document provides a comprehensive overview of the CRM solution architecture, including all modules, entities, database tables, and their relationships.

## Quick Statistics

| Metric | Count |
|--------|-------|
| **Modules** | 17 |
| **Entities with Database Tables** | 64 |
| **Junction/Link Tables** | 12 |
| **Foreign Key Relationships** | 94 |
| **API Controllers** | 40 |
| **Frontend Pages** | 44 |

## Architecture Pattern

The solution follows a **Hexagonal Architecture** (Ports & Adapters) pattern with:

- **Core Layer**: Domain entities and business logic (`CRM.Core`)
- **Application Layer**: Use cases and DTOs (`CRM.Application`)
- **Infrastructure Layer**: Database, messaging, and external services (`CRM.Infrastructure`)
- **API Layer**: REST API controllers (`CRM.Backend`)
- **Frontend**: React SPA with Material-UI (`CRM.Frontend`)

## Modules

### 1. 👥 Customer Management
**Description**: Core customer and contact management

| Entities | Controllers | Frontend Pages |
|----------|-------------|----------------|
| Customer | CustomersController | CustomersPage |
| CustomerContact | ContactsController | ContactsPage |
| Contact | | |
| SocialMediaLink | | |

**Key Relationships**:
- Customer → Customer (self-referential: ReferredByCustomerId, ParentCustomerId)
- Customer → Lead (one-to-one: ConvertedFromLeadId)
- Customer → MarketingCampaign (many-to-one: SourceCampaignId)
- CustomerContact → Customer, Contact (junction table)

---

### 2. 🏢 Account Management
**Description**: Business accounts and B2B relationships

| Entities | Controllers | Frontend Pages |
|----------|-------------|----------------|
| Account | AccountsController | AccountsPage |

**Key Relationships**:
- Account → Customer (one-to-one)
- Account → User (many-to-one: AccountManagerId)

---

### 3. 💰 Sales Pipeline
**Description**: Lead tracking, opportunities, and quotes

| Entities | Controllers | Frontend Pages |
|----------|-------------|----------------|
| Lead | OpportunitiesController | LeadsPage |
| LeadProductInterest | QuotesController | OpportunitiesPage |
| Opportunity | PipelinesController | QuotesPage |
| OpportunityProduct | StagesController | |
| Quote | | |

**Key Relationships**:
- Lead → Customer, Campaign, User (AssignedTo)
- LeadProductInterest → Lead, Product (junction)
- Opportunity → Customer, Account (many-to-one)
- OpportunityProduct → Opportunity, Product (junction)
- Quote → Opportunity, Customer (many-to-one)

---

### 4. 📦 Product Catalog
**Description**: Product and service management

| Entities | Controllers | Frontend Pages |
|----------|-------------|----------------|
| Product | ProductsController | ProductsPage, ServicesPage |

**Key Relationships**:
- Product → LookupItem (CategoryLookupId, UnitOfMeasureLookupId)

---

### 5. 📢 Marketing
**Description**: Campaign management and metrics

| Entities | Controllers | Frontend Pages |
|----------|-------------|----------------|
| MarketingCampaign | CampaignsController | CampaignsPage |
| CampaignMetric | | |

**Key Relationships**:
- CampaignMetric → MarketingCampaign (many-to-one)
- MarketingCampaign → LookupItem (StatusLookupId, TypeLookupId)

---

### 6. 🎫 Service Requests
**Description**: Customer support and ticketing system

| Entities | Controllers | Frontend Pages |
|----------|-------------|----------------|
| ServiceRequest | ServiceRequestsController | ServiceRequestsPage |
| ServiceRequestCategory | ServiceRequestSettingsController | ServiceRequestSettingsPage |
| ServiceRequestSubcategory | | |
| ServiceRequestType | | |
| ServiceRequestCustomFieldDefinition | | |
| ServiceRequestCustomFieldValue | | |

**Key Relationships**:
- ServiceRequest → Customer, User (many-to-one)
- ServiceRequest → Category, Subcategory, Type (many-to-one)
- ServiceRequestSubcategory → Category (many-to-one)
- ServiceRequestCustomFieldValue → ServiceRequest, Definition (many-to-one)

---

### 7. ✅ Task Management
**Description**: Tasks, notes, and activity tracking

| Entities | Controllers | Frontend Pages |
|----------|-------------|----------------|
| CrmTask | TasksController | TasksPage |
| Note | NotesController | NotesPage |
| Activity | ActivitiesController | ActivitiesPage |

**Key Relationships**:
- CrmTask → User (AssignedToId, CreatedById)
- Note → User, Customer (polymorphic)
- Activity → User, Customer, Opportunity (polymorphic)

---

### 8. 💬 Communication
**Description**: Multi-channel communication management

| Entities | Controllers | Frontend Pages |
|----------|-------------|----------------|
| CommunicationChannel | CommunicationsController | CommunicationsPage |
| CommunicationMessage | InteractionsController | InteractionsPage |
| EmailTemplate | | ChannelSettingsPage |
| Conversation | | |
| Interaction | | |

**Key Relationships**:
- CommunicationMessage → Channel, User (many-to-one)
- Conversation → Customer, User (many-to-one)
- Interaction → Customer, User (many-to-one)

---

### 9. 👤 User Management
**Description**: Users, roles, departments, and permissions

| Entities | Controllers | Frontend Pages |
|----------|-------------|----------------|
| User | UsersController | UserManagementPage |
| UserGroup | UserGroupsController | ProfileManagementPage |
| UserGroupMember | UserProfilesController | DepartmentManagementPage |
| UserProfile | DepartmentsController | LoginPage |
| UserApprovalRequest | AuthController | RegisterPage |
| Department | | |
| OAuthToken | | |

**Key Relationships**:
- User → Department (many-to-one)
- UserGroupMember → User, UserGroup (junction)
- UserApprovalRequest → User (many-to-one)
- OAuthToken → User (many-to-one)

---

### 10. 📞 Contact Information
**Description**: Normalized address, phone, email, and social media data

| Entities | Controllers | Frontend Pages |
|----------|-------------|----------------|
| Address | ContactInfoController | |
| PhoneNumber | | |
| EmailAddress | | |
| SocialMediaAccount | | |
| SocialAccount | | |
| EntityAddressLink | | |
| EntityPhoneLink | | |
| EntityEmailLink | | |
| EntitySocialMediaLink | | |
| ContactDetail | | |
| ContactInfoLink | | |
| SocialMediaFollow | | |

**Key Relationships**:
- EntityAddressLink → Address (polymorphic linking)
- EntityPhoneLink → PhoneNumber (polymorphic linking)
- EntityEmailLink → EmailAddress (polymorphic linking)
- EntitySocialMediaLink → SocialMediaAccount (polymorphic linking)

---

### 11. 📊 Master Data
**Description**: Reference data, lookups, and geographic data

| Entities | Controllers | Frontend Pages |
|----------|-------------|----------------|
| LookupCategory | LookupsController | MasterDataSettingsPage |
| LookupItem | MasterDataController | |
| ZipCode | ZipCodesController | |
| Locality | FieldMasterDataController | |
| Tag | | |
| EntityTag | | |
| CustomField | | |
| FieldMasterDataLink | | |

**Key Relationships**:
- LookupItem → LookupCategory (many-to-one)
- EntityTag → Tag (polymorphic linking)
- Locality → ZipCode (many-to-one)

---

### 12. ⚙️ System Settings
**Description**: System configuration and branding

| Entities | Controllers | Frontend Pages |
|----------|-------------|----------------|
| SystemSettings | SystemSettingsController | SettingsPage |
| ColorPalette | ColorPalettesController | BrandingSettingsPage |
| ModuleFieldConfiguration | ModuleFieldConfigurationsController | ModuleFieldSettingsPage |
| ModuleUIConfig | ModuleUIConfigController | NavigationSettingsPage |
| | AdminSettingsController | SecuritySettingsPage |

**Key Relationships**:
- ModuleFieldConfiguration → LookupCategory (many-to-one)
- ColorPalette (standalone - no FKs)
- SystemSettings (standalone - no FKs)

---

### 13. 🗄️ Database Administration
**Description**: Backup, monitoring, and database management

| Entities | Controllers | Frontend Pages |
|----------|-------------|----------------|
| DatabaseBackup | DatabaseController | DatabaseSettingsPage |
| BackupSchedule | MonitoringController | MonitoringSettingsPage |

**Key Relationships**:
- DatabaseBackup → BackupSchedule (many-to-one)
- DatabaseBackup → User (CreatedById)

---

### 14. ☁️ Cloud Deployment
**Description**: Cloud provider and deployment management

| Entities | Controllers | Frontend Pages |
|----------|-------------|----------------|
| CloudProvider | CloudDeploymentController | DeploymentSettingsPage |
| CloudDeployment | | |
| DeploymentAttempt | | |
| HealthCheckLog | | |

**Key Relationships**:
- CloudDeployment → CloudProvider (many-to-one)
- DeploymentAttempt → CloudDeployment (many-to-one)
- HealthCheckLog → CloudDeployment (many-to-one)

---

### 15. 📈 Dashboard & Analytics
**Description**: Dashboard views and data analytics

| Entities | Controllers | Frontend Pages |
|----------|-------------|----------------|
| *(No dedicated entities)* | DashboardController | DashboardPage |

**Note**: Dashboard aggregates data from other modules, no dedicated entities.

---

### 16. 🔧 Infrastructure & Utilities
**Description**: File uploads, webhooks, health checks, and demo data

| Entities | Controllers | Frontend Pages |
|----------|-------------|----------------|
| *(No dedicated entities)* | FileUploadController | |
| | HealthController | |
| | DemoDataController | |
| | WebhooksController | |

**Note**: Utility controllers that operate on other modules' data.

---

### 17. 📤 Import/Export
**Description**: Data import and export functionality

| Entities | Controllers | Frontend Pages |
|----------|-------------|----------------|
| *(No dedicated entities)* | ImportExportController | ImportExportPage |

**Note**: Operates on existing module data for bulk operations.

---

## Entity-Relationship Summary

### Junction Tables (Many-to-Many Relationships)

| Junction Table | Links | Purpose |
|----------------|-------|---------|
| CustomerContact | Customer ↔ Contact | Customer-Contact relationships |
| LeadProductInterest | Lead ↔ Product | Products a lead is interested in |
| OpportunityProduct | Opportunity ↔ Product | Products in an opportunity |
| UserGroupMember | User ↔ UserGroup | User group memberships |
| EntityAddressLink | Any Entity ↔ Address | Polymorphic address linking |
| EntityPhoneLink | Any Entity ↔ PhoneNumber | Polymorphic phone linking |
| EntityEmailLink | Any Entity ↔ EmailAddress | Polymorphic email linking |
| EntitySocialMediaLink | Any Entity ↔ SocialMediaAccount | Polymorphic social linking |
| ContactInfoLink | Contact ↔ ContactDetail | Contact information linking |
| EntityTag | Any Entity ↔ Tag | Polymorphic tagging |
| FieldMasterDataLink | CustomField ↔ MasterData | Field-to-master-data linking |
| SocialMediaFollow | User ↔ SocialMediaAccount | Social follows tracking |

### Self-Referential Relationships

| Entity | FK Column | Purpose |
|--------|-----------|---------|
| Customer | ParentCustomerId | Parent company hierarchy |
| Customer | ReferredByCustomerId | Referral tracking |
| Department | ParentDepartmentId | Department hierarchy |
| ServiceRequestCategory | ParentCategoryId | Category hierarchy |

### Cross-Module Relationships

| From Entity | To Entity | Relationship | Purpose |
|-------------|-----------|--------------|---------|
| Customer | Lead | One-to-One | Lead conversion |
| Customer | MarketingCampaign | Many-to-One | Source tracking |
| Lead | MarketingCampaign | Many-to-One | Campaign attribution |
| Opportunity | Customer | Many-to-One | Sales opportunity |
| Opportunity | Account | Many-to-One | B2B opportunity |
| Quote | Opportunity | Many-to-One | Quote for opportunity |
| ServiceRequest | Customer | Many-to-One | Support ticket |
| Activity | Customer | Many-to-One | Activity tracking |
| Note | Customer | Many-to-One | Notes on customer |
| CrmTask | User | Many-to-One | Task assignment |

---

## Gap Analysis Results

### Modules Without Entities
These modules are service/utility modules that aggregate or operate on other modules' data:

1. **Dashboard & Analytics** - Aggregates data from all modules
2. **Infrastructure & Utilities** - File uploads, health checks, demo data
3. **Import/Export** - Bulk data operations

### All 64 DbSet Entities Have Database Tables ✅
Every entity with a DbSet in CrmDbContext has a corresponding database table.

### Controller Coverage ✅
All 40 controllers are now mapped to modules in the architecture diagram.

---

## Interactive Architecture Diagram

The solution includes an interactive architecture visualization accessible at:

**About Page → Architecture Tab**

Features:
- 📊 Statistics dashboard
- 🔍 Entity search
- 📋 Module overview with entities, controllers, and pages
- 🗂️ Table/Entity listing with PKs, FKs, and relationships
- 🔗 Relationship viewer showing all foreign keys

---

## Technology Stack

### Backend
- **.NET 8** with Entity Framework Core 8
- **MariaDB** database
- **JWT Authentication** with OAuth2 support
- **RESTful API** with OpenAPI/Swagger

### Frontend
- **React 18** with TypeScript
- **Material-UI (MUI)** component library
- **React Router** for navigation
- **Redux Toolkit** for state management

### Infrastructure
- **Docker** containerization
- **Kubernetes** orchestration
- **minikube** for local development

---

*Last Updated: Architecture diagram and gap analysis completed*
*View interactive diagram: Frontend → About → Architecture tab*

# CRM Solution - Database Schema Documentation

> **Last Updated:** February 1, 2026  
> **Version:** 1.0  
> **Total Tables:** ~171  
> **Supported Databases:** MariaDB/MySQL (primary), SQL Server, PostgreSQL, SQLite

---

## Table of Contents

1. [Overview](#overview)
2. [Database Categories](#database-categories)
3. [Core Tables](#core-tables)
4. [CRM Entity Tables](#crm-entity-tables)
5. [Contact Information Tables](#contact-information-tables)
6. [Marketing & Campaigns](#marketing--campaigns)
7. [Sales & Quotes](#sales--quotes)
8. [Service Desk & Workflows](#service-desk--workflows)
9. [AI & Analytics](#ai--analytics)
10. [System & Configuration](#system--configuration)

---

## Overview

The CRM database follows a normalized 3NF design with the following characteristics:

- **Primary Keys:** Auto-increment integers (`Id`)
- **Soft Deletes:** `IsDeleted` flag on most tables
- **Timestamps:** `CreatedAt`, `UpdatedAt` on all tables
- **Optimistic Concurrency:** `RowVersion` (binary(8) for MySQL, rowversion for SQL Server)
- **Character Set:** UTF-8 (utf8mb4 for MySQL/MariaDB)

---

## Database Categories

### 1. Core Tables (User/Auth)
| Table | Description |
|-------|-------------|
| `Users` | User accounts with authentication |
| `UserGroups` | Role-based permission groups |
| `UserGroupMembers` | Junction: Users ↔ Groups |
| `UserProfiles` | Extended user preferences |
| `UserApprovalRequests` | Registration pending approval |
| `Departments` | Organizational departments |
| `OAuthTokens` | OAuth provider tokens |

### 2. CRM Entity Tables
| Table | Description |
|-------|-------------|
| `Customers` | Accounts (organizations/individuals) |
| `Contacts` | Individual people |
| `AccountContacts` | Junction: Accounts ↔ Contacts |
| `Leads` | Sales leads |
| `Opportunities` | Sales opportunities |
| `OpportunityProducts` | Junction: Opportunities ↔ Products |
| `LeadProductInterests` | Junction: Leads ↔ Products |
| `Products` | Product catalog |
| `Interactions` | Customer interactions |

### 3. Contact Information
| Table | Description |
|-------|-------------|
| `Addresses` | Physical addresses |
| `PhoneNumbers` | Phone number records |
| `EmailAddresses` | Email address records |
| `SocialMediaAccounts` | Social media profiles |
| `EntityAddressLinks` | Junction: Any entity ↔ Address |
| `EntityPhoneLinks` | Junction: Any entity ↔ Phone |
| `EntityEmailLinks` | Junction: Any entity ↔ Email |
| `EntitySocialMediaLinks` | Junction: Any entity ↔ Social |
| `ContactDetails` | Legacy contact details |
| `SocialAccounts` | Legacy social accounts |
| `ContactInfoLinks` | Legacy junction table |

### 4. Marketing & Campaigns
| Table | Description |
|-------|-------------|
| `MarketingCampaigns` | Campaign definitions |
| `CampaignMetrics` | Campaign performance metrics |
| `CampaignRecipients` | Campaign target recipients |
| `CampaignLinkClicks` | Link tracking |
| `CampaignABTests` | A/B test configurations |
| `CampaignConversions` | Conversion tracking |
| `CampaignWorkflows` | Campaign automation |
| `EmailTemplates` | Email template library |
| `EmailSequences` | Drip campaign sequences |
| `EmailSequenceSteps` | Sequence step definitions |
| `EmailSequenceEnrollments` | Contact enrollments |
| `EmailSequenceStepExecutions` | Step execution logs |

### 5. Sales & Quotes
| Table | Description |
|-------|-------------|
| `Quotes` | Sales quotes |
| `QuoteLineItems` | Quote line items |
| `Orders` | Sales orders |
| `OrderLineItems` | Order line items |
| `Invoices` | Customer invoices |
| `InvoiceLineItems` | Invoice line items |
| `Payments` | Payment records |
| `Subscriptions` | Subscription records |
| `SubscriptionItems` | Subscription line items |
| `SubscriptionUsages` | Usage tracking |
| `Contracts` | Customer contracts |
| `CreditMemos` | Credit memos |
| `CreditMemoLineItems` | Credit memo lines |
| `CreditApplications` | Credit applications |

### 6. CPQ (Configure-Price-Quote)
| Table | Description |
|-------|-------------|
| `ProductBundles` | Product bundle definitions |
| `ProductBundleItems` | Bundle components |
| `ProductBundleRules` | Bundle configuration rules |
| `PriceBooks` | Price book definitions |
| `PriceBookEntries` | Price book entries |
| `PricingRules` | Dynamic pricing rules |
| `PricingRuleUsages` | Rule usage tracking |
| `DiscountApprovalMatrices` | Discount approval workflows |
| `ApprovalLevels` | Approval thresholds |
| `ApprovalGroups` | Approval group definitions |
| `ApprovalGroupMembers` | Group members |
| `ApprovalRequests` | Pending approvals |
| `ApprovalSteps` | Approval workflow steps |

### 7. Service Desk
| Table | Description |
|-------|-------------|
| `ServiceRequests` | Service/support tickets |
| `ServiceRequestCategories` | Ticket categories |
| `ServiceRequestSubcategories` | Subcategories |
| `ServiceRequestTypes` | Ticket types |
| `ServiceRequestCustomFieldDefinitions` | Custom field schemas |
| `ServiceRequestCustomFieldValues` | Custom field values |
| `KnowledgeArticles` | Knowledge base articles |
| `KnowledgeCategories` | Article categories |
| `ServiceRequestArticles` | Junction: Tickets ↔ Articles |
| `ArticleFeedbacks` | Article ratings/feedback |
| `SLAPolicies` | SLA policy definitions |
| `SLATargets` | SLA target metrics |
| `SLAInstances` | Active SLA tracking |
| `BusinessHoursConfigs` | Business hours definitions |
| `EscalationRules` | Ticket escalation rules |

### 8. Workflow Engine
| Table | Description |
|-------|-------------|
| `WorkflowDefinitions` | Workflow definitions |
| `WorkflowVersions` | Version history |
| `WorkflowNodes` | Workflow node definitions |
| `WorkflowTransitions` | Node transitions |
| `WorkflowInstances` | Running workflow instances |
| `WorkflowNodeInstances` | Node execution state |
| `WorkflowTasks` | Human tasks |
| `WorkflowLogs` | Execution logs |

### 9. Lead Management
| Table | Description |
|-------|-------------|
| `LeadRoutingRules` | Lead assignment rules |
| `LeadRoutingCriteria` | Rule criteria |
| `LeadRoutingTargets` | Assignment targets |
| `LeadRoutingLogs` | Routing audit logs |
| `DuplicateRules` | Duplicate detection rules |
| `DuplicateMatchFields` | Match field definitions |
| `DuplicateCandidates` | Potential duplicates |
| `DuplicateMergeHistories` | Merge audit logs |

### 10. Web Tracking
| Table | Description |
|-------|-------------|
| `WebVisitors` | Website visitor tracking |
| `WebSessions` | Session data |
| `WebPageViews` | Page view tracking |
| `FormDefinitions` | Web form definitions |
| `FormFields` | Form field definitions |
| `FormSubmissions` | Form submission data |
| `AttributionSettings` | Attribution model configs |
| `CampaignTouchpoints` | Multi-touch tracking |
| `CampaignAttributionSummaries` | Attribution summaries |

### 11. E-Signature
| Table | Description |
|-------|-------------|
| `ESignatureRequests` | Signature requests |
| `ESignatureSigners` | Signer definitions |
| `ESignatureDocuments` | Document attachments |
| `ESignatureAuditEvents` | Signature audit trail |

### 12. Sales Performance
| Table | Description |
|-------|-------------|
| `CommissionPlans` | Commission plan definitions |
| `CommissionTiers` | Tiered commission rates |
| `CommissionPlanAssignments` | User ↔ Plan assignments |
| `Commissions` | Earned commissions |
| `CommissionStatements` | Statement summaries |
| `SalesQuotas` | Sales quota definitions |
| `SalesForecasts` | Sales forecasts |
| `ForecastLineItems` | Forecast line items |
| `ForecastHistories` | Forecast change history |
| `Teams` | Sales team definitions |
| `TeamMembers` | Team membership |

### 13. AI & Predictions
| Table | Description |
|-------|-------------|
| `AIModels` | ML model definitions |
| `Predictions` | Prediction results |
| `LeadScores` | Lead scoring results |
| `OpportunityInsights` | Opportunity AI insights |
| `ChurnRisks` | Churn risk predictions |
| `ActionRecommendations` | AI action recommendations |
| `EmailIntelligences` | Email analysis results |

### 14. Reports & Dashboards
| Table | Description |
|-------|-------------|
| `Dashboards` | Dashboard definitions |
| `DashboardWidgets` | Widget configurations |
| `ReportDefinitions` | Report definitions |
| `ReportFolders` | Report organization |
| `ReportSchedules` | Scheduled report runs |
| `ReportExecutions` | Execution history |
| `ReportWidgetConfigs` | Widget configurations |

### 15. Relationship Management
| Table | Description |
|-------|-------------|
| `RelationshipTypes` | Relationship type definitions |
| `AccountRelationships` | Account ↔ Account relationships |
| `RelationshipInteractions` | Relationship activities |
| `AccountHealthSnapshots` | Account health history |
| `RelationshipMaps` | Visual relationship maps |
| `AccountTerritories` | Territory definitions |
| `CustomerTerritoryAssignments` | Territory assignments |

### 16. Communication
| Table | Description |
|-------|-------------|
| `CommunicationChannels` | Channel configurations |
| `CommunicationMessages` | Message history |
| `Conversations` | Conversation threads |
| `SocialMediaFollows` | Social follow tracking |

### 17. System & Configuration
| Table | Description |
|-------|-------------|
| `SystemSettings` | Global system configuration |
| `LookupCategories` | Lookup value categories |
| `LookupItems` | Lookup values |
| `Tags` | Tag definitions |
| `EntityTags` | Entity ↔ Tag assignments |
| `CustomFields` | Custom field definitions |
| `ModuleFieldConfigurations` | Field visibility configs |
| `ModuleUIConfigs` | UI module configs |
| `ColorPalettes` | Theme color palettes |
| `LLMProviderSettings` | AI provider configs |
| `FieldMasterDataLinks` | Field ↔ Master data links |

### 18. Master Data
| Table | Description |
|-------|-------------|
| `ZipCodes` | ZIP/Postal code reference |
| `Localities` | City/locality reference |

### 19. Cloud & Backup
| Table | Description |
|-------|-------------|
| `CloudProviders` | Cloud provider configs |
| `CloudDeployments` | Deployment records |
| `DeploymentAttempts` | Deployment attempts |
| `HealthCheckLogs` | Health check history |
| `DatabaseBackups` | Backup records |
| `BackupSchedules` | Backup schedules |

### 20. Tasks & Activities
| Table | Description |
|-------|-------------|
| `CrmTasks` | Task records |
| `Activities` | Activity logs |
| `Notes` | Notes attached to entities |
| `SocialMediaLinks` | Legacy social links |

---

## Core Tables Detail

### Users Table

```sql
CREATE TABLE Users (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  Username VARCHAR(100) NOT NULL UNIQUE,
  Email VARCHAR(255) NOT NULL UNIQUE,
  PasswordHash VARCHAR(512) NOT NULL,
  FirstName VARCHAR(100) NOT NULL,
  LastName VARCHAR(100) NOT NULL,
  Phone VARCHAR(50),
  Role INT NOT NULL DEFAULT 0,
  IsActive BOOLEAN DEFAULT TRUE,
  EmailVerified BOOLEAN DEFAULT FALSE,
  LastLoginAt DATETIME,
  FailedLoginAttempts INT DEFAULT 0,
  LockoutEnd DATETIME,
  RefreshToken VARCHAR(512),
  RefreshTokenExpiry DATETIME,
  TwoFactorEnabled BOOLEAN DEFAULT FALSE,
  TwoFactorSecret VARCHAR(255),
  HeaderColor VARCHAR(10),
  PhotoUrl VARCHAR(500),
  DepartmentId INT,
  UserProfileId INT,
  PrimaryGroupId INT,
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UpdatedAt DATETIME,
  IsDeleted BOOLEAN DEFAULT FALSE,
  RowVersion BINARY(8)
);
```

### Customers (Accounts) Table

```sql
CREATE TABLE Customers (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  -- Basic Info
  FirstName VARCHAR(100),
  LastName VARCHAR(100),
  Email VARCHAR(255) NOT NULL,
  Phone VARCHAR(20),
  Company VARCHAR(255),
  -- Organization Details
  LegalName VARCHAR(500),
  DbaName VARCHAR(255),
  TaxId VARCHAR(50),
  RegistrationNumber VARCHAR(100),
  -- Classification
  Category VARCHAR(50),
  AccountType VARCHAR(50),
  Industry VARCHAR(100),
  Segment VARCHAR(50),
  Tier VARCHAR(20),
  -- Status
  Status VARCHAR(50) DEFAULT 'Active',
  CustomerHealthScore INT DEFAULT 50,
  -- Relationships
  OwnerId INT,
  ReferredByAccountId INT,
  ParentAccountId INT,
  -- Timestamps
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UpdatedAt DATETIME,
  IsDeleted BOOLEAN DEFAULT FALSE,
  RowVersion BINARY(8)
);
```

### UserGroups Table (Permissions)

```sql
CREATE TABLE UserGroups (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  Name VARCHAR(100) NOT NULL,
  Description VARCHAR(500),
  IsActive BOOLEAN DEFAULT TRUE,
  IsDefault BOOLEAN DEFAULT FALSE,
  IsSystemAdmin BOOLEAN DEFAULT FALSE,
  DisplayOrder INT DEFAULT 0,
  HeaderColor VARCHAR(10),
  
  -- Menu/Page Access (18 flags)
  CanAccessDashboard BOOLEAN DEFAULT TRUE,
  CanAccessCustomers BOOLEAN DEFAULT TRUE,
  CanAccessContacts BOOLEAN DEFAULT TRUE,
  CanAccessLeads BOOLEAN DEFAULT TRUE,
  CanAccessOpportunities BOOLEAN DEFAULT TRUE,
  CanAccessProducts BOOLEAN DEFAULT TRUE,
  CanAccessServices BOOLEAN DEFAULT TRUE,
  CanAccessCampaigns BOOLEAN DEFAULT TRUE,
  CanAccessQuotes BOOLEAN DEFAULT TRUE,
  CanAccessTasks BOOLEAN DEFAULT TRUE,
  CanAccessActivities BOOLEAN DEFAULT TRUE,
  CanAccessNotes BOOLEAN DEFAULT TRUE,
  CanAccessWorkflows BOOLEAN DEFAULT TRUE,
  CanAccessServiceRequests BOOLEAN DEFAULT TRUE,
  CanAccessReports BOOLEAN DEFAULT TRUE,
  CanAccessSettings BOOLEAN DEFAULT FALSE,
  CanAccessUserManagement BOOLEAN DEFAULT FALSE,
  
  -- CRUD Permissions (40+ flags)
  CanCreateCustomers BOOLEAN DEFAULT TRUE,
  CanEditCustomers BOOLEAN DEFAULT TRUE,
  CanDeleteCustomers BOOLEAN DEFAULT FALSE,
  CanViewAllCustomers BOOLEAN DEFAULT TRUE,
  -- (similar for Contacts, Leads, Opportunities, Products, Campaigns, Quotes, Tasks, Workflows)
  
  -- Data Access
  DataAccessScope VARCHAR(20) DEFAULT 'own',
  CanExportData BOOLEAN DEFAULT FALSE,
  CanImportData BOOLEAN DEFAULT FALSE,
  CanBulkEdit BOOLEAN DEFAULT FALSE,
  CanBulkDelete BOOLEAN DEFAULT FALSE,
  
  AccessibleMenuItems TEXT,
  CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
  UpdatedAt DATETIME,
  IsDeleted BOOLEAN DEFAULT FALSE
);
```

---

## Foreign Key Relationships

### Account Relationships
```
Customers.OwnerId → Users.Id
Customers.ReferredByAccountId → Customers.Id (self-reference)
Customers.ParentAccountId → Customers.Id (self-reference)
AccountContacts.AccountId → Customers.Id
AccountContacts.ContactId → Contacts.Id
```

### Opportunity Relationships
```
Opportunities.AccountId → Customers.Id
Opportunities.LeadId → Leads.Id
Opportunities.SalesOwnerId → Users.Id
OpportunityProducts.OpportunityId → Opportunities.Id
OpportunityProducts.ProductId → Products.Id
```

### User Relationships
```
Users.DepartmentId → Departments.Id
Users.PrimaryGroupId → UserGroups.Id
Users.UserProfileId → UserProfiles.Id
UserGroupMembers.UserId → Users.Id
UserGroupMembers.UserGroupId → UserGroups.Id
```

### Service Request Relationships
```
ServiceRequests.AccountId → Customers.Id
ServiceRequests.ContactId → Contacts.Id
ServiceRequests.AssignedToId → Users.Id
ServiceRequests.CategoryId → ServiceRequestCategories.Id
ServiceRequests.SubcategoryId → ServiceRequestSubcategories.Id
```

---

## Indexes

### Performance Indexes
```sql
-- Users
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_DepartmentId ON Users(DepartmentId);

-- Customers
CREATE INDEX IX_Customers_Email ON Customers(Email);
CREATE INDEX IX_Customers_Company ON Customers(Company);
CREATE INDEX IX_Customers_Category ON Customers(Category);
CREATE INDEX IX_Customers_OwnerId ON Customers(OwnerId);

-- Leads
CREATE INDEX IX_Leads_Email ON Leads(Email);
CREATE INDEX IX_Leads_Status ON Leads(Status);
CREATE INDEX IX_Leads_OwnerId ON Leads(OwnerId);

-- Opportunities
CREATE INDEX IX_Opportunities_AccountId ON Opportunities(AccountId);
CREATE INDEX IX_Opportunities_Stage ON Opportunities(Stage);
CREATE INDEX IX_Opportunities_CloseDate ON Opportunities(ExpectedCloseDate);

-- Service Requests
CREATE INDEX IX_ServiceRequests_Status ON ServiceRequests(Status);
CREATE INDEX IX_ServiceRequests_Priority ON ServiceRequests(Priority);
CREATE INDEX IX_ServiceRequests_AssignedToId ON ServiceRequests(AssignedToId);
```

---

## Data Types by Database

| Type | MariaDB/MySQL | SQL Server | PostgreSQL | SQLite |
|------|---------------|------------|------------|--------|
| Primary Key | INT AUTO_INCREMENT | INT IDENTITY | SERIAL | INTEGER PRIMARY KEY |
| String (short) | VARCHAR(n) | NVARCHAR(n) | VARCHAR(n) | TEXT |
| String (long) | TEXT | NVARCHAR(MAX) | TEXT | TEXT |
| Boolean | TINYINT(1) | BIT | BOOLEAN | INTEGER |
| DateTime | DATETIME(6) | DATETIME2 | TIMESTAMP | TEXT |
| Decimal | DECIMAL(18,2) | DECIMAL(18,2) | NUMERIC(18,2) | REAL |
| RowVersion | BINARY(8) | ROWVERSION | BYTEA | BLOB |

---

## Migration Notes

### From Customer to Account
The solution underwent a rename from `Customer` to `Account`. The database table remains `Customers` for backward compatibility, but the Entity Framework entity is `Account`.

### MariaDB Row Size Limit
MariaDB has a 65535 byte row limit. Large tables (like MarketingCampaigns) have string columns converted from LONGTEXT to TEXT to prevent overflow.

---

*For setup instructions, see `setup-database.sh` in this directory.*

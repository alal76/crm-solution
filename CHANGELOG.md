# Changelog

All notable changes to CRM Solution will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.0.29] - 2026-02-02

### Added - Deduplication Feature (Complete)

#### Backend Services
- `IDuplicateDetectionService` - Interface for duplicate detection operations
- `IMergeService` - Interface for merge/unmerge operations
- `DuplicateDetectionService` - Full duplicate detection with fuzzy matching algorithms
- `MergeService` - Merge/unmerge with snapshots and related record relinking
- `DuplicatesController` - REST API endpoints for all deduplication operations

#### Database Tables & Migrations
- `DuplicateRules` - Configurable detection rules per entity type
- `DuplicateMatchFields` - Field-level matching configuration
- `DuplicateCandidates` - Detected duplicate pairs with match scores
- `DuplicateMergeGroups` - Tracks merged record groups
- `DuplicateMergeGroupMembers` - Individual records with JSON snapshots

#### API Endpoints
- `POST /api/duplicates/check` - Real-time duplicate check on create/edit
- `POST /api/duplicates/scan/{entityType}` - Batch duplicate scanning
- `GET /api/duplicates/candidates/{entityType}` - List pending duplicates
- `POST /api/duplicates/merge/preview` - Preview merge field selections
- `POST /api/duplicates/merge` - Execute merge with audit trail
- `POST /api/duplicates/unmerge` - Restore merged records

#### Matching Algorithms
- Exact matching (case-insensitive)
- Fuzzy matching (Levenshtein distance)
- Phonetic matching (Soundex)
- Normalized matching (phone numbers)
- Email domain extraction

#### Testing
- 29 unit tests for DuplicateDetectionService and MergeService
- E2E test suite for API endpoints
- Tested on 192.168.0.9 with MariaDB

### Fixed
- JWT claim parsing for user ID extraction (nameid claim support)
- Contact query filters for IsMergedDuplicate field

---

## [0.0.28] - 2026-02-01

### Added - Azure DevOps & Cloud Deployment

#### Azure Infrastructure as Code
- Created `azure/main.bicep` - Complete Azure infrastructure definition
- Resources: ACR, App Service, MySQL Flexible Server, Key Vault, App Insights
- Environment support: dev, staging, prod with appropriate SKUs
- Managed Identity for secure Key Vault access

#### Azure DevOps CI/CD Pipeline
- Created `azure-pipelines.yml` - Multi-stage pipeline
- Stages: Build → Test → Docker Push → Deploy Staging → Deploy Production
- Automatic triggers on main and dev branches
- Test result publishing and artifact management

#### Deployment Scripts
- `azure/deploy.sh` - Bash deployment script for local/CLI deployment
- `azure/parameters.dev.json` - Development environment parameters
- `azure/parameters.prod.json` - Production environment parameters

#### Documentation
- `azure/AZURE_DEPLOYMENT.md` - Comprehensive deployment guide
- Architecture diagrams, cost estimates, troubleshooting

---

## [0.0.27] - 2026-02-01

### Fixed - Schema Consolidation & Test Fixes

#### Customer → Account Deprecation (Complete)
- Removed all 24 CS0618 deprecation warnings
- Fixed references in: `Contact.cs`, `IOutputPorts.cs`, `DbSeed.cs`, `SampleDataSeederService.cs`, `InteractionsController.cs`
- Added pragma suppression in `CrmDbContext.cs` for intentional backward-compat alias

#### Schema Consolidation
- Created `000_baseline_schema.sql` (61KB) - complete schema for fresh deployments
- Updated `009_junction_table_improvements.sql` for incremental migrations
- Deployment strategy: Fresh install uses 000 only; existing DBs use 001-009 incrementally

#### Test Fixes (No Regressions)
- Added `HttpContext` setup to controller tests for ETag support
- Added notification service mock setup to prevent null task exceptions
- Fixed `AuthenticationServiceTests.RegisterAsync` to use in-memory DbContext
- Fixed `AIServiceHelper.GetDefaultModelForProvider` to handle empty string defaults
- Fixed `AccountsController_ShouldHaveAccountsRouteAlias` test expectation

#### BVT Enhancements
- Created `DatabaseSchemaVerificationTests.cs` for Entity↔DB alignment verification
- Tests verify all 95 DbSets have corresponding database tables
- Requires live database connection (Category=DatabaseSchema)

### Test Results
- Unit Tests: **883 passed**, 0 failed
- Integration Tests: 36 (require live database)
- Build: 0 errors, 0 deprecation warnings

---

## [0.0.26] - 2026-02-01

### Added

#### Junction Table Improvements
- **EntitySocialMediaLink**: Added `ValidFrom`, `ValidTo`, `DoNotContact`, and `IsActive` computed property
- **Tag Entity**: Extended BaseEntity, added `Color` and `Description` fields, proper navigation to EntityTags
- **EntityTag**: Renamed `Tag` to `TagName`, added `Tag` navigation property, `SortOrder`, `CreatedBy`

#### Database Indexes
- Unique composite indexes on all junction tables to prevent duplicates:
  - `EntityTags`: `(EntityType, EntityId, TagId)`
  - `UserGroupMembers`: `(UserId, UserGroupId)`
  - `AccountContacts`: `(AccountId, ContactId)`
- Performance indexes on junction tables:
  - `OpportunityProducts.CreatedAt`
  - `LeadProductInterests.CreatedAt`
  - `AccountContacts.Role`
  - `AccountContacts.IsPrimaryContact`

#### LLM Failover Configuration
- Configurable fallback order for AI providers in admin settings
- Smart provider detection - only configured providers included in fallback
- Fixed `IsConfigured()` to detect unresolved `${VAR:}` placeholders
- Added `EffectiveFallbackOrder` computed property

### Changed
- Updated NormalizationService to use new `TagName` property with Tag navigation fallback
- Enhanced CrmDbContext with proper junction table configurations
- Updated documentation (DATABASE_SCHEMA.md, ARCHITECTURE_OVERVIEW.md)

### Database
- Schema `009_junction_table_improvements.sql`:
  - Creates `Tags` and `EntityTags` tables with proper structure
  - Creates `CustomFields` table for generic field storage
  - Creates `llm_provider_settings` table for AI configuration
  - Creates `SystemSettings` table for system configuration
  - Adds all missing junction table indexes and constraints

---

## [0.0.25] - 2026-01-XX

### Added

#### Security Enhancements
- **Password Management**:
  - First-time password setup for new users (PasswordNeverSet flag)
  - Admin-forced password reset functionality (MustResetPassword flag)
  - Password change tracking (PasswordLastChangedAt timestamp)
  - Password reset tokens with expiration
  - Encrypted backup codes for 2FA recovery

- **Password Complexity Settings** (System-Wide):
  - Configurable minimum password length (default: 8)
  - Configurable maximum password length (default: 128)
  - Require uppercase letters toggle
  - Require lowercase letters toggle
  - Require numbers toggle
  - Require special characters toggle
  - Real-time password strength validation in UI

- **Group-Level Security Policies**:
  - Password expiration days per group
  - Password expiration policy options:
    - None: No action on expiration
    - MustChange: Force password reset on expiration
    - Alert: Show warning dialog on expiration
    - Warn: Show notice banner approaching expiration
  - Password expiration warning days configuration
  - Two-factor authentication preference per group
  - Two-factor authentication enforcement per group

- **Login Page Improvements**:
  - Form autocomplete support for password managers
  - Automatic redirect to password setup when required
  - Password expiration warning on login

#### Frontend Pages
- **Password Setup Page** (`/setup-password`):
  - Real-time complexity requirement validation
  - Visual progress indicators for each requirement
  - Support for first-time setup and forced resets

#### API Endpoints
- `POST /api/auth/setup-password`: Set password for first-time users or forced resets
- `GET /api/auth/password-requirements`: Retrieve current password complexity requirements

### Changed
- Updated Security Settings tab with Password Complexity Settings card
- Updated Group Management with new Security Policy tab
- Enhanced AuthResponse with password status fields

### Database
- Migration 018_security_enhancements.sql (SQL Server)
- Schema 008_security_enhancements.sql (MariaDB)
- Updated Users table with password management fields
- Updated UserGroups table with security policy fields
- Updated SystemSettings with password complexity fields

---

## [0.0.24] - 2026-01-XX

### Added

#### New Features
- **Relationship Management**: Full B2B and B2C relationship tracking with:
  - Hierarchy management (Parent/Child/Affiliate relationships)
  - Influence level tracking for decision makers
  - Relationship health metrics and scoring
  - Account ownership and engagement tracking
  
- **Campaign Execution**: Advanced campaign delivery system with:
  - Multi-channel execution (Email, SMS, Push, etc.)
  - Batch processing with configurable throttling
  - A/B testing support with automatic winner selection
  - Real-time analytics and progress monitoring
  - Workflow engine integration

#### Documentation
- **Comprehensive HowTo Guide** (`docs/HOWTO.md`):
  - Step-by-step tutorials for all features
  - Keyboard shortcuts reference
  - API integration guide
  - Troubleshooting section

#### Infrastructure
- **GUI Deployment Tool** (`CRM.Infrastructure/deployment-tool/`):
  - Cross-platform Python/Tkinter application
  - Hosting platform selection (Local/Cloud)
  - Container orchestration (Docker/Kubernetes)
  - Database configuration (PaaS/VM-hosted)
  - Script generation (Unix bash / Windows PowerShell)
  - Admin user and seed data configuration
  - JWT token generation
  - OAuth provider configuration
  - Smoke tests and verification
  - Configuration save/restore

- **Deployment Templates**:
  - Docker Compose template
  - Kubernetes manifests template
  - Environment file templates

### Changed

- **Version Reset**: Reset from 1.7.25 to 0.0.24 for fresh versioning
- **About Page**: Updated features list with Relationship Management and Campaign Execution
- **Test Results**: Updated to reflect 748 tests (747 passed, 1 pre-existing failure)
- **Help Page**: Added new tutorials for Relationship Management and Campaign Execution
- **Help Page**: Added new FAQs for new features
- **Documentation Index**: Added HOWTO.md to documentation navigation

### Technical Details

- **New Entities**:
  - `Relationship` - Core relationship entity
  - `RelationshipType` - Relationship type definitions
  - `RelationshipHierarchy` - Parent/child tracking
  - `CampaignExecution` - Campaign execution record
  - `ExecutionBatch` - Batch processing records
  - `ExecutionRecipient` - Individual recipient tracking
  - `ABTestVariant` - A/B testing configuration
  - `ExecutionAnalytics` - Real-time metrics

- **New Services**:
  - `RelationshipService` - B2B/B2C relationship management
  - `CampaignExecutionService` - Campaign execution orchestration
  - `BatchProcessingService` - Batch job management
  - `AnalyticsService` - Real-time analytics collection

- **New Controllers**:
  - `RelationshipsController` - Relationship CRUD operations
  - `CampaignExecutionsController` - Execution management
  - `ExecutionBatchesController` - Batch operations

### Default Credentials

- **Username**: sysadmin
- **Password**: Password@123

⚠️ **Important**: Change these credentials in production!

### Deployment

To deploy using the new GUI tool:

1. Navigate to `CRM.Infrastructure/`
2. Run `./run-deployment-tool.sh` (Unix) or `.\run-deployment-tool.ps1` (Windows)
3. Configure options in the GUI
4. Generate deployment scripts
5. Execute deployment
6. Run smoke tests

### Migration Notes

If upgrading from a previous version:
1. Back up your database
2. Run database migrations
3. Update environment variables
4. Restart services

---

## [Previous Versions]

Version history prior to 0.0.24 is available in git history.
This changelog will track changes from 0.0.24 forward.

---

*CRM Solution is licensed under AGPL-3.0*
*Copyright © 2024-2026 Abhishek Lal*

# Changelog

All notable changes to CRM Solution will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

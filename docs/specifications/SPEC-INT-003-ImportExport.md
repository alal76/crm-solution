# Feature Specification: Import/Export

> **Spec ID:** SPEC-INT-003  
> **Feature:** Import/Export with Batch Processing  
> **Module:** Integration & Data Management  
> **Version:** 1.0  
> **Last Updated:** February 14, 2026  
> **Status:** ❌ Not Implemented

---

## 1. Business Context

### 1.1 Feature Description

The Import/Export feature enables bulk data operations for CRM entities using multiple formats (CSV, Excel, JSON). This capability supports:

- **Bulk Import**: Load data from external sources with mapping, validation, and duplicate handling
- **Bulk Export**: Extract CRM data in multiple formats for analysis, migration, or backup
- **Async Processing**: Long-running operations execute in background with progress tracking
- **Audit Trail**: Complete history of imports/exports with error logs for compliance

The feature is critical for data migration, integration, and regulatory compliance scenarios.

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | CSV Import | Parse and import CSV files with mapping | ❌ |
| SF-002 | Excel Import | Parse and import Excel (XLSX/XLS) files | ❌ |
| SF-003 | JSON Import | Parse and import JSON data structures | ❌ |
| SF-004 | Column Mapping | UI wizard for mapping source to CRM fields | ❌ |
| SF-005 | Data Validation | Pre-import validation with error reporting | ❌ |
| SF-006 | Duplicate Detection | Identify duplicates before/during import | ❌ |
| SF-007 | Import Preview | Preview data before committing to database | ❌ |
| SF-008 | Batch Processing | Process large files (10k+ records) asynchronously | ❌ |
| SF-009 | Error Handling | Detailed error logging with rollback options | ❌ |
| SF-010 | CSV Export | Export CRM data to CSV format | ❌ |
| SF-011 | Excel Export | Export CRM data to Excel format | ❌ |
| SF-012 | JSON Export | Export CRM data to JSON format | ❌ |
| SF-013 | Export Scheduling | Schedule recurring exports (daily, weekly, monthly) | ❌ |
| SF-014 | Field Selection | Allow users to select which fields to export | ❌ |
| SF-015 | Audit Trail | Complete history of all import/export operations | ❌ |

### 1.3 Use Cases

| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Import Accounts from CSV | Sales Manager | CSV file with account data | Accounts created in CRM | ❌ |
| UC-002 | Import Contacts with mapping | Administrator | Excel file with contact columns | Contacts linked to accounts | ❌ |
| UC-003 | Preview import data | User | File uploaded, mapping defined | Preview table shown | ❌ |
| UC-004 | Handle duplicate contacts | User | Import contains duplicates | Duplicates flagged/merged per rules | ❌ |
| UC-005 | Bulk export all accounts | Analyst | Export permission granted | CSV/Excel file downloaded | ❌ |
| UC-006 | Schedule weekly lead export | Marketing Manager | Export template created | Weekly exports generated automatically | ❌ |
| UC-007 | View import history | Auditor | Admin permission granted | Import log with all operations | ❌ |
| UC-008 | Retry failed import | User | Previous import had errors | Reprocess with corrected file | ❌ |
| UC-009 | Rollback partial import | Administrator | Import in progress | Stop processing, rollback changes | ❌ |
| UC-010 | Import large file (100k rows) | Data Team | 100k+ record CSV | Processed asynchronously with progress | ❌ |

---

## 2. Frontend Implementation

### 2.1 Pages

| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| Import Wizard Page | `CRM.Frontend/src/pages/ImportWizardPage.tsx` | ❌ | Multi-step form for file upload & mapping |
| Export Wizard Page | `CRM.Frontend/src/pages/ExportWizardPage.tsx` | ❌ | Multi-step form for export configuration |
| Import History Page | `CRM.Frontend/src/pages/ImportHistoryPage.tsx` | ❌ | View past imports with details |
| Export History Page | `CRM.Frontend/src/pages/ExportHistoryPage.tsx` | ❌ | View past exports with downloads |
| Export Schedule Page | `CRM.Frontend/src/pages/ExportSchedulePage.tsx` | ❌ | Configure scheduled exports |

### 2.2 Components

| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| FileUploader | `CRM.Frontend/src/components/import/FileUploader.tsx` | ❌ | Drag & drop file upload |
| ColumnMapper | `CRM.Frontend/src/components/import/ColumnMapper.tsx` | ❌ | Map source to CRM fields |
| ImportPreview | `CRM.Frontend/src/components/import/ImportPreview.tsx` | ❌ | Preview first N rows |
| ValidationErrors | `CRM.Frontend/src/components/import/ValidationErrors.tsx` | ❌ | Display validation issues |
| DuplicateHandler | `CRM.Frontend/src/components/import/DuplicateHandler.tsx` | ❌ | Resolve duplicate records |
| ImportProgress | `CRM.Frontend/src/components/import/ImportProgress.tsx` | ❌ | Real-time import progress bar |
| ExportOptions | `CRM.Frontend/src/components/export/ExportOptions.tsx` | ❌ | Format & field selection |
| ExportScheduler | `CRM.Frontend/src/components/export/ExportScheduler.tsx` | ❌ | Configure cron schedule |
| ImportJobStatus | `CRM.Frontend/src/components/import/ImportJobStatus.tsx` | ❌ | Display job status |

### 2.3 Services (API Client)

| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| importService | `CRM.Frontend/src/services/importService.ts` | uploadFile, submitMapping, previewData, validateData, createJob, getJobStatus, cancelJob, getHistory | ❌ |
| exportService | `CRM.Frontend/src/services/exportService.ts` | getEntityFields, createExport, downloadFile, getHistory, createSchedule, updateSchedule, deleteSchedule | ❌ |
| fileService | `CRM.Frontend/src/services/fileService.ts` | parseCSV, parseExcel, parseJSON, detectEncoding | ❌ |

### 2.4 Frontend Validations

| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| File Size | Max 500 MB | Frontend & Backend | ❌ |
| File Format | CSV, XLSX, XLS, JSON only | Frontend & Backend | ❌ |
| Column Mapping | All required fields mapped | Frontend & Backend | ❌ |
| Email Format | Valid email pattern | Frontend & Backend | ❌ |
| Data Type Match | Source type matches CRM field type | Backend | ❌ |
| Required Fields | No empty values for required fields | Frontend & Backend | ❌ |
| Unique Constraints | Email/Phone uniqueness violations | Backend | ❌ |
| Export Schedule | Valid cron expression | Frontend & Backend | ❌ |

---

## 3. Backend Implementation

### 3.1 Entities

| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| ImportJob | `CRM.Core/Entities/ImportJob.cs` | ❌ | Tracks import operation metadata |
| ImportMapping | `CRM.Core/Entities/ImportMapping.cs` | ❌ | Stores column mappings |
| ImportError | `CRM.Core/Entities/ImportError.cs` | ❌ | Records validation/import errors |
| ExportJob | `CRM.Core/Entities/ExportJob.cs` | ❌ | Tracks export operation metadata |
| ExportSchedule | `CRM.Core/Entities/ExportSchedule.cs` | ❌ | Scheduled export definitions |
| ExportLog | `CRM.Core/Entities/ExportLog.cs` | ❌ | Records export execution history |

### 3.2 DTOs

| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| ImportFileDto | `CRM.Core/DTOs/Import/ImportFileDto.cs` | ❌ | File metadata & preview |
| ImportMappingDto | `CRM.Core/DTOs/Import/ImportMappingDto.cs` | ❌ | Column mapping definition |
| ImportPreviewDto | `CRM.Core/DTOs/Import/ImportPreviewDto.cs` | ❌ | Preview rows & validation |
| ImportErrorDto | `CRM.Core/DTOs/Import/ImportErrorDto.cs` | ❌ | Error with row & field info |
| ImportJobStatusDto | `CRM.Core/DTOs/Import/ImportJobStatusDto.cs` | ❌ | Job progress & stats |
| ExportOptionsDto | `CRM.Core/DTOs/Export/ExportOptionsDto.cs` | ❌ | Format, fields, filters |
| ExportScheduleDto | `CRM.Core/DTOs/Export/ExportScheduleDto.cs` | ❌ | Schedule configuration |
| ExportJobStatusDto | `CRM.Core/DTOs/Export/ExportJobStatusDto.cs` | ❌ | Export progress & link |

### 3.3 Interfaces

| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| IImportService | `CRM.Core/Interfaces/IImportService.cs` | 12+ | ❌ |
| IExportService | `CRM.Core/Interfaces/IExportService.cs` | 10+ | ❌ |
| IDataValidator | `CRM.Core/Interfaces/IDataValidator.cs` | 6+ | ❌ |
| IMappingEngine | `CRM.Core/Interfaces/IMappingEngine.cs` | 6+ | ❌ |
| IBatchProcessor | `CRM.Core/Interfaces/IBatchProcessor.cs` | 5+ | ❌ |
| IAsyncJobScheduler | `CRM.Core/Interfaces/IAsyncJobScheduler.cs` | 4+ | ❌ |
| IDuplicateDetector | `CRM.Core/Interfaces/IDuplicateDetector.cs` | 3+ | ❌ |

### 3.4 Services

| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| ImportService | `CRM.Infrastructure/Services/ImportService.cs` | 12+ | ❌ |
| ExportService | `CRM.Infrastructure/Services/ExportService.cs` | 10+ | ❌ |
| DataValidator | `CRM.Infrastructure/Services/DataValidator.cs` | 6+ | ❌ |
| MappingEngine | `CRM.Infrastructure/Services/MappingEngine.cs` | 6+ | ❌ |
| BatchProcessor | `CRM.Infrastructure/Services/BatchProcessor.cs` | 5+ | ❌ |
| AsyncJobScheduler | `CRM.Infrastructure/Services/AsyncJobScheduler.cs` | 4+ | ❌ |
| DuplicateDetector | `CRM.Infrastructure/Services/DuplicateDetector.cs` | 3+ | ❌ |
| FileParser | `CRM.Infrastructure/Services/FileParser.cs` | 5+ | ❌ |
| FileExporter | `CRM.Infrastructure/Services/FileExporter.cs` | 5+ | ❌ |

### 3.5 Controllers

| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| ImportController | `CRM.Api/Controllers/ImportController.cs` | 12 | ❌ |
| ExportController | `CRM.Api/Controllers/ExportController.cs` | 10 | ❌ |

### 3.6 API Endpoints

| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| POST | `/api/import/upload` | UploadFile | Yes | ❌ |
| POST | `/api/import/mapping` | SubmitMapping | Yes | ❌ |
| POST | `/api/import/preview` | PreviewData | Yes | ❌ |
| POST | `/api/import/validate` | ValidateData | Yes | ❌ |
| POST | `/api/import/submit` | SubmitImport | Yes | ❌ |
| GET | `/api/import/job/{jobId}` | GetJobStatus | Yes | ❌ |
| DELETE | `/api/import/job/{jobId}` | CancelJob | Yes | ❌ |
| GET | `/api/import/history` | GetImportHistory | Yes | ❌ |
| GET | `/api/import/duplicates/{jobId}` | GetDuplicates | Yes | ❌ |
| POST | `/api/import/duplicates/{jobId}/resolve` | ResolveDuplicates | Yes | ❌ |
| POST | `/api/export/fields/{entity}` | GetEntityFields | Yes | ❌ |
| POST | `/api/export/create` | CreateExport | Yes | ❌ |
| GET | `/api/export/download/{jobId}` | DownloadExport | Yes | ❌ |
| GET | `/api/export/history` | GetExportHistory | Yes | ❌ |
| POST | `/api/export/schedule` | CreateSchedule | Yes | ❌ |
| PUT | `/api/export/schedule/{scheduleId}` | UpdateSchedule | Yes | ❌ |
| DELETE | `/api/export/schedule/{scheduleId}` | DeleteSchedule | Yes | ❌ |
| GET | `/api/export/schedule/{scheduleId}/status` | GetScheduleStatus | Yes | ❌ |

### 3.7 Backend Validations

| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| File Size | Max 500 MB | ImportService | ❌ |
| File Format | CSV, XLSX, XLS, JSON | FileParser | ❌ |
| Encoding | UTF-8, Latin-1, CP-1252 supported | FileParser | ❌ |
| Required Fields | All mapped fields present | DataValidator | ❌ |
| Email Format | RFC 5322 pattern | DataValidator | ❌ |
| Phone Format | E.164 or custom pattern | DataValidator | ❌ |
| Date Format | ISO 8601 or configured format | DataValidator | ❌ |
| Data Type | Value matches CRM field type | DataValidator | ❌ |
| Unique Constraints | No duplicate key violations | DuplicateDetector | ❌ |
| Foreign Keys | Valid references to related entities | DataValidator | ❌ |
| Batch Size | Max 1000 records per batch | BatchProcessor | ❌ |

---

## 4. Database Implementation

### 4.1 Tables

| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| ImportJobs | `database/schema/050_import_export_tables.sql` | ❌ | Tracks import operations |
| ImportMappings | `database/schema/050_import_export_tables.sql` | ❌ | Column mappings for reuse |
| ImportErrors | `database/schema/050_import_export_tables.sql` | ❌ | Validation/import errors |
| ExportJobs | `database/schema/050_import_export_tables.sql` | ❌ | Tracks export operations |
| ExportSchedules | `database/schema/050_import_export_tables.sql` | ❌ | Scheduled exports |
| ExportLogs | `database/schema/050_import_export_tables.sql` | ❌ | Export execution history |

### 4.2 Data Elements - ImportJobs

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ❌ |
| JobId | BINARY(16) | No | NEWID() | UK | JobId | ❌ |
| EntityType | VARCHAR(100) | No | - | - | EntityType | ❌ |
| SourceFormat | VARCHAR(20) | No | - | - | SourceFormat | ❌ |
| FileName | VARCHAR(255) | No | - | - | FileName | ❌ |
| FileSize | BIGINT | No | - | - | FileSize | ❌ |
| Status | VARCHAR(20) | No | 'Pending' | - | Status | ❌ |
| TotalRows | INT | No | - | - | TotalRows | ❌ |
| SuccessRows | INT | No | 0 | - | SuccessRows | ❌ |
| ErrorRows | INT | No | 0 | - | ErrorRows | ❌ |
| SkippedRows | INT | No | 0 | - | SkippedRows | ❌ |
| DuplicateRows | INT | No | 0 | - | DuplicateRows | ❌ |
| MappingId | INT | Yes | - | FK | MappingId | ❌ |
| DuplicateHandlingStrategy | VARCHAR(50) | No | 'Flag' | - | DuplicateHandlingStrategy | ❌ |
| CreatedBy | INT | No | - | FK → Users | CreatedBy | ❌ |
| CreatedAt | DATETIME(6) | No | CURRENT_TIMESTAMP | - | CreatedAt | ❌ |
| StartedAt | DATETIME(6) | Yes | - | - | StartedAt | ❌ |
| CompletedAt | DATETIME(6) | Yes | - | - | CompletedAt | ❌ |
| UpdatedAt | DATETIME(6) | Yes | - | - | UpdatedAt | ❌ |
| IsDeleted | BOOLEAN | No | FALSE | - | IsDeleted | ❌ |
| RowVersion | BINARY(8) | No | - | - | RowVersion | ❌ |

### 4.3 Data Elements - ImportMappings

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ❌ |
| MappingName | VARCHAR(255) | No | - | - | MappingName | ❌ |
| EntityType | VARCHAR(100) | No | - | - | EntityType | ❌ |
| SourceFormat | VARCHAR(20) | No | - | - | SourceFormat | ❌ |
| MappingData | JSON | No | - | - | MappingData | ❌ |
| ValidationRules | JSON | No | - | - | ValidationRules | ❌ |
| IsReusable | BOOLEAN | No | TRUE | - | IsReusable | ❌ |
| CreatedBy | INT | No | - | FK → Users | CreatedBy | ❌ |
| CreatedAt | DATETIME(6) | No | CURRENT_TIMESTAMP | - | CreatedAt | ❌ |
| UpdatedAt | DATETIME(6) | Yes | - | - | UpdatedAt | ❌ |
| IsDeleted | BOOLEAN | No | FALSE | - | IsDeleted | ❌ |

### 4.4 Data Elements - ImportErrors

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ❌ |
| ImportJobId | INT | No | - | FK → ImportJobs | ImportJobId | ❌ |
| RowNumber | INT | No | - | - | RowNumber | ❌ |
| ColumnName | VARCHAR(255) | Yes | - | - | ColumnName | ❌ |
| ErrorType | VARCHAR(50) | No | - | - | ErrorType | ❌ |
| ErrorMessage | VARCHAR(500) | No | - | - | ErrorMessage | ❌ |
| SourceValue | VARCHAR(1000) | Yes | - | - | SourceValue | ❌ |
| CreatedAt | DATETIME(6) | No | CURRENT_TIMESTAMP | - | CreatedAt | ❌ |
| IsDeleted | BOOLEAN | No | FALSE | - | IsDeleted | ❌ |

### 4.5 Data Elements - ExportJobs

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ❌ |
| JobId | BINARY(16) | No | NEWID() | UK | JobId | ❌ |
| EntityType | VARCHAR(100) | No | - | - | EntityType | ❌ |
| ExportFormat | VARCHAR(20) | No | - | - | ExportFormat | ❌ |
| SelectedFields | JSON | No | - | - | SelectedFields | ❌ |
| FilterCriteria | JSON | Yes | - | - | FilterCriteria | ❌ |
| Status | VARCHAR(20) | No | 'Pending' | - | Status | ❌ |
| TotalRecords | INT | No | 0 | - | TotalRecords | ❌ |
| ExportedRecords | INT | No | 0 | - | ExportedRecords | ❌ |
| FileSize | BIGINT | Yes | - | - | FileSize | ❌ |
| FilePath | VARCHAR(500) | Yes | - | - | FilePath | ❌ |
| DownloadLink | VARCHAR(500) | Yes | - | - | DownloadLink | ❌ |
| ExpiresAt | DATETIME(6) | Yes | - | - | ExpiresAt | ❌ |
| CreatedBy | INT | No | - | FK → Users | CreatedBy | ❌ |
| CreatedAt | DATETIME(6) | No | CURRENT_TIMESTAMP | - | CreatedAt | ❌ |
| StartedAt | DATETIME(6) | Yes | - | - | StartedAt | ❌ |
| CompletedAt | DATETIME(6) | Yes | - | - | CompletedAt | ❌ |
| UpdatedAt | DATETIME(6) | Yes | - | - | UpdatedAt | ❌ |
| IsDeleted | BOOLEAN | No | FALSE | - | IsDeleted | ❌ |
| RowVersion | BINARY(8) | No | - | - | RowVersion | ❌ |

### 4.6 Data Elements - ExportSchedules

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ❌ |
| ScheduleName | VARCHAR(255) | No | - | - | ScheduleName | ❌ |
| EntityType | VARCHAR(100) | No | - | - | EntityType | ❌ |
| ExportFormat | VARCHAR(20) | No | - | - | ExportFormat | ❌ |
| SelectedFields | JSON | No | - | - | SelectedFields | ❌ |
| FilterCriteria | JSON | Yes | - | - | FilterCriteria | ❌ |
| CronExpression | VARCHAR(50) | No | - | - | CronExpression | ❌ |
| IsActive | BOOLEAN | No | TRUE | - | IsActive | ❌ |
| LastRunAt | DATETIME(6) | Yes | - | - | LastRunAt | ❌ |
| NextRunAt | DATETIME(6) | Yes | - | - | NextRunAt | ❌ |
| RecipientEmails | VARCHAR(1000) | Yes | - | - | RecipientEmails | ❌ |
| RetentionDays | INT | No | 30 | - | RetentionDays | ❌ |
| CreatedBy | INT | No | - | FK → Users | CreatedBy | ❌ |
| CreatedAt | DATETIME(6) | No | CURRENT_TIMESTAMP | - | CreatedAt | ❌ |
| UpdatedAt | DATETIME(6) | Yes | - | - | UpdatedAt | ❌ |
| IsDeleted | BOOLEAN | No | FALSE | - | IsDeleted | ❌ |

### 4.7 Relationships

| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| ImportJobs | Users | N:1 | CreatedBy | ❌ |
| ImportJobs | ImportMappings | N:1 | MappingId | ❌ |
| ImportErrors | ImportJobs | N:1 | ImportJobId | ❌ |
| ExportJobs | Users | N:1 | CreatedBy | ❌ |
| ExportSchedules | Users | N:1 | CreatedBy | ❌ |

### 4.8 Indexes

| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_ImportJobs_Status | ImportJobs | Status | NonClustered | ❌ |
| IX_ImportJobs_EntityType | ImportJobs | EntityType | NonClustered | ❌ |
| IX_ImportJobs_CreatedBy | ImportJobs | CreatedBy | NonClustered | ❌ |
| IX_ImportJobs_CreatedAt | ImportJobs | CreatedAt DESC | NonClustered | ❌ |
| IX_ImportErrors_ImportJobId | ImportErrors | ImportJobId | NonClustered | ❌ |
| IX_ImportMappings_EntityType | ImportMappings | EntityType | NonClustered | ❌ |
| IX_ExportJobs_Status | ExportJobs | Status | NonClustered | ❌ |
| IX_ExportJobs_EntityType | ExportJobs | EntityType | NonClustered | ❌ |
| IX_ExportSchedules_CronExpression | ExportSchedules | CronExpression | NonClustered | ❌ |
| IX_ExportSchedules_IsActive | ExportSchedules | IsActive | NonClustered | ❌ |

---

## 5. Test Coverage

### 5.1 Unit Tests

| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| FileParserTests | `CRM.Tests/Services/FileParserTests.cs` | 25+ | ❌ |
| ImportServiceTests | `CRM.Tests/Services/ImportServiceTests.cs` | 30+ | ❌ |
| ExportServiceTests | `CRM.Tests/Services/ExportServiceTests.cs` | 20+ | ❌ |
| DataValidatorTests | `CRM.Tests/Services/DataValidatorTests.cs` | 35+ | ❌ |
| MappingEngineTests | `CRM.Tests/Services/MappingEngineTests.cs` | 25+ | ❌ |
| DuplicateDetectorTests | `CRM.Tests/Services/DuplicateDetectorTests.cs` | 20+ | ❌ |
| BatchProcessorTests | `CRM.Tests/Services/BatchProcessorTests.cs` | 15+ | ❌ |

### 5.2 Integration Tests

| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| ImportServiceIntegrationTests | `CRM.Tests/Integration/ImportServiceIntegrationTests.cs` | 18+ | ❌ |
| ExportServiceIntegrationTests | `CRM.Tests/Integration/ExportServiceIntegrationTests.cs` | 15+ | ❌ |
| ImportExportControllerTests | `CRM.Tests/Integration/ImportExportControllerTests.cs` | 20+ | ❌ |
| BatchProcessingIntegrationTests | `CRM.Tests/Integration/BatchProcessingIntegrationTests.cs` | 12+ | ❌ |

### 5.3 E2E Tests

| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| Import Wizard Flow | `e2e-tests/tests/import-export/import-wizard.spec.ts` | 12+ | ❌ |
| Export Wizard Flow | `e2e-tests/tests/import-export/export-wizard.spec.ts` | 10+ | ❌ |
| Import History | `e2e-tests/tests/import-export/import-history.spec.ts` | 8+ | ❌ |
| Export Scheduling | `e2e-tests/tests/import-export/export-schedule.spec.ts` | 8+ | ❌ |
| Large File Import | `e2e-tests/tests/import-export/large-file-import.spec.ts` | 6+ | ❌ |

### 5.4 Test Scenarios

#### 5.4.1 CSV Import Tests
- Parse valid CSV with headers
- Handle missing columns
- Detect encoding automatically (UTF-8, Latin-1, CP-1252)
- Parse quoted fields with commas
- Handle empty rows
- Import 10k+ rows with batching

#### 5.4.2 Validation Tests
- Required field validation
- Email format validation
- Phone number format validation
- Date format validation
- Unique constraint validation
- Foreign key validation
- Type conversion validation

#### 5.4.3 Duplicate Detection Tests
- Detect exact duplicates
- Detect fuzzy duplicates (90%+ match)
- Handle duplicate resolution strategies (skip, merge, replace)
- Track duplicate counts
- Generate duplicate report

#### 5.4.4 Performance Tests
- Import 10k records in <5 minutes
- Import 100k records in <30 minutes
- Memory usage <500MB for 100k record file
- Concurrent imports don't interfere
- Export 50k records in <2 minutes

#### 5.4.5 Error Handling Tests
- Rollback on critical errors
- Partial import on non-critical errors
- Error reporting with row/column info
- Detailed error messages for troubleshooting
- Export error logs

---

## 6. Inconsistencies & Issues

### 6.1 Data Type Mismatches

| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| FileParser (string encoding) | DataValidator (field type) | Source string may not match CRM field type | Convert/cast with validation |
| ImportMapping (field names) | Database (column names) | Mapping may reference non-existent fields | Validate mapping against entity schema |
| ExportSchedule (filter criteria JSON) | ExportService (filter parsing) | JSON structure must match query builder expectations | Document filter schema strictly |

### 6.2 Memory Usage Issues

| Issue | Impact | Mitigation |
|-------|--------|-----------|
| Large CSV files (>100MB) loaded entirely into memory | OOM errors on smaller instances | Stream CSV parsing, process in batches of 1000 records |
| Excel files require full parsing before processing | Can't process files >200MB | Recommend CSV for large exports |
| JSON deserialization of large datasets | Memory spike during parsing | Stream JSON parsing, use JSON streaming libraries |

### 6.3 Encoding Detection Issues

| Issue | Impact | Mitigation |
|-------|--------|-----------|
| Auto-detect encoding may fail for mixed encodings | Garbled characters in import | Provide encoding selection UI, default to UTF-8 |
| BOM (Byte Order Mark) not handled in all files | Extra character at start of file | Strip BOM before parsing |
| Eastern European/Asian character sets may not detected | Character corruption | Support explicit encoding selection |

### 6.4 Partial Import Rollback

| Issue | Impact | Mitigation |
|-------|--------|-----------|
| Long transaction may lock database for minutes | Blocks other users during import | Use batch transactions (commit per batch of 100) |
| Rollback of 10k+ records takes time | User impatience during error recovery | Track rollback progress, show estimated time |
| Cascading deletes on rollback may be slow | Can lose related data unintentionally | Soft delete, log what would be deleted |

### 6.5 Field Data Type Mismatch

| Issue | Impact | Mitigation |
|-------|--------|-----------|
| CSV all fields are strings | Type inference failures | Provide explicit type mapping in UI |
| Excel can infer types but data may be mixed | Some rows parse, some fail | Validate type consistency before import |
| JSON allows mixed types in arrays | Type coercion errors | Require consistent types in mapping |
| Date format ambiguity (MM/DD vs DD/MM) | Incorrect date parsing | Detect format from first row, allow override |

### 6.6 Missing Implementations

| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| Excel file parsing (XLSX/XLS) | FileParser.cs | Requires EPPlus/OfficeOpenXml library | TODO-INT003-02 |
| Scheduled export execution | AsyncJobScheduler.cs | Background job framework needed (Hangfire/Quartz) | TODO-INT003-04 |
| Large file streaming | FileParser.cs | Need streaming reader implementation | TODO-INT003-06 |
| Partial import rollback | ImportService.cs | Need transaction control per batch | TODO-INT003-08 |

### 6.7 Validation Gaps

| Field | Issue | Status |
|-------|-------|--------|
| Phone Number | International format support incomplete | TODO-INT003-10 |
| Date | Multiple format support needed | TODO-INT003-11 |
| Currency | Decimal precision and rounding not defined | TODO-INT003-12 |
| Boolean | Multiple true/false representations not handled | TODO-INT003-13 |
| Enum | Custom enum mapping not implemented | TODO-INT003-14 |

---

## 7. TODOs

### 7.1 Critical Path

#### Frontend
| ID | Priority | Description |
|----|----------|-------------|
| TODO-INT003-FE-01 | P0 | Create FileUploader component with drag & drop |
| TODO-INT003-FE-02 | P0 | Create ColumnMapper component for field mapping |
| TODO-INT003-FE-03 | P0 | Create ImportPreview component |
| TODO-INT003-FE-04 | P0 | Create ValidationErrors component |
| TODO-INT003-FE-05 | P1 | Create ImportWizardPage with multi-step form |
| TODO-INT003-FE-06 | P1 | Create ExportWizardPage with format selection |
| TODO-INT003-FE-07 | P1 | Implement importService with API calls |
| TODO-INT003-FE-08 | P1 | Implement exportService with API calls |
| TODO-INT003-FE-09 | P2 | Create DuplicateHandler component |
| TODO-INT003-FE-10 | P2 | Create ImportProgress component with real-time updates |

#### Backend
| ID | Priority | Description |
|----|----------|-------------|
| TODO-INT003-BE-01 | P0 | Create ImportJob, ImportMapping, ImportError entities |
| TODO-INT003-BE-02 | P0 | Create ExportJob, ExportSchedule entities |
| TODO-INT003-BE-03 | P0 | Implement IImportService interface |
| TODO-INT003-BE-04 | P0 | Implement IExportService interface |
| TODO-INT003-BE-05 | P0 | Implement ImportService with CSV parsing |
| TODO-INT003-BE-06 | P0 | Implement FileParser for CSV format |
| TODO-INT003-BE-07 | P1 | Implement DataValidator with 15+ validation rules |
| TODO-INT003-BE-08 | P1 | Implement DuplicateDetector with fuzzy matching |
| TODO-INT003-BE-09 | P1 | Implement ExportService with CSV export |
| TODO-INT003-BE-10 | P1 | Create ImportController with 12 endpoints |
| TODO-INT003-BE-11 | P1 | Create ExportController with 10 endpoints |
| TODO-INT003-BE-12 | P2 | Implement MappingEngine for field transformation |
| TODO-INT003-BE-13 | P2 | Implement BatchProcessor for async job handling |
| TODO-INT003-BE-14 | P2 | Implement FileParser for Excel (XLSX/XLS) format |
| TODO-INT003-BE-15 | P2 | Implement FileParser for JSON format |

#### Database
| ID | Priority | Description |
|----|----------|-------------|
| TODO-INT003-DB-01 | P0 | Create ImportJobs table |
| TODO-INT003-DB-02 | P0 | Create ImportMappings table |
| TODO-INT003-DB-03 | P0 | Create ImportErrors table |
| TODO-INT003-DB-04 | P0 | Create ExportJobs table |
| TODO-INT003-DB-05 | P0 | Create ExportSchedules table |
| TODO-INT003-DB-06 | P1 | Create ExportLogs table |
| TODO-INT003-DB-07 | P1 | Add foreign key constraints |
| TODO-INT003-DB-08 | P1 | Add performance indexes |

#### Testing
| ID | Priority | Description |
|----|----------|-------------|
| TODO-INT003-TEST-01 | P1 | Create FileParserTests (25 tests) |
| TODO-INT003-TEST-02 | P1 | Create DataValidatorTests (35 tests) |
| TODO-INT003-TEST-03 | P1 | Create ImportServiceTests (30 tests) |
| TODO-INT003-TEST-04 | P1 | Create ExportServiceTests (20 tests) |
| TODO-INT003-TEST-05 | P2 | Create MappingEngineTests (25 tests) |
| TODO-INT003-TEST-06 | P2 | Create DuplicateDetectorTests (20 tests) |
| TODO-INT003-TEST-07 | P2 | Create BatchProcessorTests (15 tests) |
| TODO-INT003-TEST-08 | P2 | Create Import/Export E2E tests (40+ scenarios) |
| TODO-INT003-TEST-09 | P3 | Create large file performance tests (10k+ records) |

### 7.2 Enhancement Path

#### Features
| ID | Priority | Description |
|----|----------|-------------|
| TODO-INT003-02 | P2 | Add Excel file import (XLSX/XLS) support |
| TODO-INT003-03 | P2 | Add JSON file import/export support |
| TODO-INT003-04 | P2 | Implement scheduled exports with cron |
| TODO-INT003-05 | P2 | Add email delivery of scheduled exports |
| TODO-INT003-06 | P2 | Implement large file streaming (no size limit) |
| TODO-INT003-07 | P2 | Add import template management & reuse |
| TODO-INT003-08 | P2 | Implement partial import rollback per batch |
| TODO-INT003-09 | P3 | Add data transformation rules (calculate, concatenate, etc.) |
| TODO-INT003-10 | P3 | Implement webhook callback on import completion |
| TODO-INT003-11 | P3 | Add API key access for automated imports |

#### Validation
| ID | Priority | Description |
|----|----------|-------------|
| TODO-INT003-10 | P2 | Add international phone number validation |
| TODO-INT003-11 | P2 | Add multi-format date detection (DD/MM, MM/DD, ISO) |
| TODO-INT003-12 | P2 | Add currency format validation with rounding |
| TODO-INT003-13 | P2 | Add boolean value mapping (true/false, yes/no, 1/0) |
| TODO-INT003-14 | P2 | Add enum field custom value mapping |
| TODO-INT003-15 | P3 | Add regex pattern validation for custom fields |

#### Performance
| ID | Priority | Description |
|----|----------|-------------|
| TODO-INT003-20 | P2 | Implement parallel batch processing (4+ workers) |
| TODO-INT003-21 | P2 | Add import result caching (last 100 imports) |
| TODO-INT003-22 | P3 | Implement incremental exports (delta changes only) |
| TODO-INT003-23 | P3 | Add compression for exported files (GZIP) |

### 7.3 Infrastructure

| ID | Priority | Description |
|----|----------|-------------|
| TODO-INT003-INFRA-01 | P2 | Set up Hangfire/Quartz for background jobs |
| TODO-INT003-INFRA-02 | P2 | Configure temporary file storage for imports/exports |
| TODO-INT003-INFRA-03 | P2 | Set up file cleanup job (delete after retention period) |
| TODO-INT003-INFRA-04 | P3 | Add S3/Azure Blob storage for large file handling |
| TODO-INT003-INFRA-05 | P3 | Configure cron scheduler for scheduled exports |

---

## 8. References

### Related Specifications
- [SPEC-INT-001: Webhook Management](SPEC-INT-001-WebhookManagement.md)
- [SPEC-INT-002: Provider Integration](SPEC-INT-002-ProviderIntegration.md)
- [SPEC-SYS-005: System Settings](SPEC-SYS-005-SystemSettings.md)

### External Standards
- CSV Format: [RFC 4180](https://tools.ietf.org/html/rfc4180)
- JSON Format: [RFC 8259](https://tools.ietf.org/html/rfc8259)
- Email Validation: [RFC 5322](https://tools.ietf.org/html/rfc5322)
- Phone Numbers: [E.164 Format](https://en.wikipedia.org/wiki/E.164)

### Technology References
- EPPlus (Excel parsing): [https://www.epplussoftware.com/](https://www.epplussoftware.com/)
- CsvHelper (CSV parsing): [https://joshclose.github.io/CsvHelper/](https://joshclose.github.io/CsvHelper/)
- Hangfire (Background jobs): [https://www.hangfire.io/](https://www.hangfire.io/)
- Newtonsoft.Json (JSON): [https://www.newtonsoft.com/json](https://www.newtonsoft.com/json)

---

## Appendix A: Field Mapping Example

```json
{
  "mappingName": "Sales_Accounts_Import",
  "entityType": "Account",
  "sourceFormat": "CSV",
  "mappingData": {
    "Company": { "target": "Company", "type": "string", "required": true },
    "Email": { "target": "Email", "type": "email", "required": true },
    "Phone": { "target": "Phone", "type": "phone", "required": false },
    "Industry": { "target": "Industry", "type": "lookup", "lookupType": "Industry" },
    "AnnualRevenue": { "target": "AnnualRevenue", "type": "currency", "required": false },
    "Employees": { "target": "EmployeeCount", "type": "integer", "required": false }
  },
  "validationRules": {
    "Email": "^[^@]+@[^@]+\\.[^@]+$",
    "Phone": "^\\+?[1-9]\\d{1,14}$",
    "AnnualRevenue": { "min": 0, "max": 9999999999 }
  }
}
```

---

## Appendix B: Duplicate Detection Strategy

```csharp
public enum DuplicateHandlingStrategy
{
    Flag,      // Mark duplicates for review
    Skip,      // Skip duplicate records
    Merge,     // Merge with existing record
    Replace,   // Replace existing with new data
    Upsert     // Update if exists, insert if new
}
```

---

**Status:** ❌ **Not Implemented** — All components pending development  
**Next Review:** After Phase 4 completion  
**Owner:** Integration Team


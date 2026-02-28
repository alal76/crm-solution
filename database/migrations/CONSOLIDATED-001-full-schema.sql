-- ============================================================================
-- CONSOLIDATED-001-full-schema.sql
-- CRM Solution – Supplemental Schema Migration (all changes in one file)
-- Generated: 2026-02-28
--
-- PURPOSE:
--   Single consolidated migration that encompasses every supplemental SQL
--   migration file.  EF Core migrations (managed by CRM.Infrastructure)
--   must be applied FIRST; this file handles tables and schema changes that
--   live outside EF Core's scope.
--
-- PREREQUISITES:
--   1. EF Core migrations applied  (dotnet ef database update)
--   2. MariaDB 10.3+ or MySQL 8.0+
--   3. Database crm_db already exists
--
-- IDEMPOTENCY:
--   All CREATE TABLE statements use IF NOT EXISTS.
--   All CREATE INDEX statements use IF NOT EXISTS.
--   All ALTER TABLE … ADD COLUMN statements use IF NOT EXISTS where supported.
--   All INSERT seed data uses ON DUPLICATE KEY UPDATE / INSERT IGNORE.
--   Safe to re-run on a database where these migrations were already applied.
--
-- SOURCE FILES (in execution order):
--   fix_missing_tables.sql
--   010_itsm_module.sql
--   011_add_itsm_permission.sql
--   025_create_crmtasks_opportunities.sql
--   100_customer_to_account_migration.sql
--   20250713_add_duplicate_merge_tracking.sql
--   20260214_add_branding_configs.sql
--   20260214_add_systemsettings_palette_fk.sql
--   20260216_add_worker_control_settings.sql
--   20260216_add_worker_architecture_tables.sql
--   20260227_enum_schema_enhancements.sql
--   20260227_servicerequest_categories.sql
--   SYS-009-ServiceRequest-Fix.sql
--   20260227_entity_fk_migration.sql
--   SYS-010-RecordComments.sql
--   SYS-011-SatisfactionTracking.sql
--   SYS-014-CustomerPortalTables.sql
-- ============================================================================

SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;
SET time_zone = '+00:00';
SET FOREIGN_KEY_CHECKS = 0;

USE crm_db;



-- ╔══════════════════════════════════════════════════════════════════════════╗
-- ║  Phase 1 – Supplemental tables (not managed by EF Core migrations)       ║
-- ╚══════════════════════════════════════════════════════════════════════════╝



-- ─────────────────────────────────────────────────────────────────────────────
-- Source: fix_missing_tables.sql
-- ─────────────────────────────────────────────────────────────────────────────


-- Create missing tables for CRM database
-- These tables were part of migration 20260214195347 which partially failed

-- 1. Quotes


CREATE TABLE IF NOT EXISTS `Quotes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `QuoteNumber` varchar(50) NOT NULL,
  `ExternalQuoteId` varchar(100) NULL,
  `Version` int NOT NULL DEFAULT 1,
  `Name` varchar(255) NOT NULL,
  `Description` text NULL,
  `Status` int NOT NULL DEFAULT 0,
  `QuoteDate` datetime(6) NOT NULL,
  `ExpirationDate` datetime(6) NULL,
  `SentDate` datetime(6) NULL,
  `ViewedDate` datetime(6) NULL,
  `AcceptedDate` datetime(6) NULL,
  `RejectedDate` datetime(6) NULL,
  `Subtotal` decimal(65,30) NOT NULL DEFAULT 0,
  `Discount` decimal(65,30) NOT NULL DEFAULT 0,
  `DiscountPercent` decimal(65,30) NOT NULL DEFAULT 0,
  `DiscountReason` varchar(500) NULL,
  `Tax` decimal(65,30) NOT NULL DEFAULT 0,
  `TaxRate` decimal(65,30) NOT NULL DEFAULT 0,
  `ShippingCost` decimal(65,30) NOT NULL DEFAULT 0,
  `Total` decimal(65,30) NOT NULL DEFAULT 0,
  `CurrencyCode` varchar(3) NULL,
  `PaymentTerms` varchar(200) NULL,
  `DeliveryTerms` varchar(500) NULL,
  `TermsAndConditions` text NULL,
  `Warranty` text NULL,
  `ValidityDays` int NULL,
  `LineItems` text NULL,
  `BillingName` varchar(200) NULL,
  `BillingAddress` varchar(500) NULL,
  `BillingCity` varchar(100) NULL,
  `BillingState` varchar(100) NULL,
  `BillingZipCode` varchar(20) NULL,
  `BillingCountry` varchar(100) NULL,
  `ShippingName` varchar(200) NULL,
  `ShippingAddress` varchar(500) NULL,
  `ShippingCity` varchar(100) NULL,
  `ShippingState` varchar(100) NULL,
  `ShippingZipCode` varchar(20) NULL,
  `ShippingCountry` varchar(100) NULL,
  `ContactName` varchar(200) NULL,
  `ContactEmail` varchar(200) NULL,
  `ContactPhone` varchar(50) NULL,
  `AccountId` int NULL,
  `ContactId` int NULL,
  `OpportunityId` int NULL,
  `AssignedToUserId` int NULL,
  `CreatedByUserId` int NULL,
  `ApprovedByUserId` int NULL,
  `ParentQuoteId` int NULL,
  `RelationshipManagerId` int NULL,
  `RequiresApproval` tinyint(1) NOT NULL DEFAULT 0,
  `IsApproved` tinyint(1) NOT NULL DEFAULT 0,
  `ApprovalDate` datetime(6) NULL,
  `ApprovalNotes` varchar(2000) NULL,
  `SubmittedForApprovalDate` datetime(6) NULL,
  `IsSigned` tinyint(1) NOT NULL DEFAULT 0,
  `SignedDate` datetime(6) NULL,
  `SignedBy` varchar(200) NULL,
  `SignatureUrl` varchar(1000) NULL,
  `Notes` text NULL,
  `InternalNotes` text NULL,
  `Attachments` text NULL,
  `QuotePdfUrl` varchar(1000) NULL,
  `Tags` varchar(500) NULL,
  `Category` varchar(100) NULL,
  `ExpectedDeliveryDate` datetime(6) NULL,
  `ActualDeliveryDate` datetime(6) NULL,
  `WarrantyMonths` int NULL,
  `WarrantyEndDate` datetime(6) NULL,
  `ServiceStartDate` datetime(6) NULL,
  `ServiceEndDate` datetime(6) NULL,
  `CustomFields` text NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  `RowVersion` binary(8) NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 2. QuoteLineItems
CREATE TABLE IF NOT EXISTS `QuoteLineItems` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `QuoteId` int NOT NULL,
  `LineNumber` int NOT NULL DEFAULT 0,
  `ProductId` int NULL,
  `SKU` varchar(100) NULL,
  `Name` varchar(255) NOT NULL,
  `Description` varchar(2000) NULL,
  `Category` varchar(100) NULL,
  `Quantity` decimal(18,4) NOT NULL DEFAULT 0,
  `UnitOfMeasure` varchar(50) NULL,
  `UnitPrice` decimal(18,2) NOT NULL DEFAULT 0,
  `ListPrice` decimal(18,2) NULL,
  `CostPrice` decimal(18,2) NULL,
  `DiscountType` int NOT NULL DEFAULT 0,
  `DiscountPercent` decimal(5,2) NOT NULL DEFAULT 0,
  `DiscountAmount` decimal(18,2) NOT NULL DEFAULT 0,
  `DiscountReason` varchar(500) NULL,
  `DiscountRequiresApproval` tinyint(1) NOT NULL DEFAULT 0,
  `DiscountApproved` tinyint(1) NOT NULL DEFAULT 0,
  `TaxRate` decimal(5,2) NOT NULL DEFAULT 0,
  `IsTaxable` tinyint(1) NOT NULL DEFAULT 1,
  `TaxCode` varchar(50) NULL,
  `Subtotal` decimal(18,2) NOT NULL DEFAULT 0,
  `TotalDiscount` decimal(18,2) NOT NULL DEFAULT 0,
  `TaxAmount` decimal(18,2) NOT NULL DEFAULT 0,
  `Total` decimal(18,2) NOT NULL DEFAULT 0,
  `Margin` decimal(18,2) NULL,
  `BillingPeriod` varchar(50) NULL,
  `WarrantyMonths` int NULL,
  `DeliveryDate` datetime(6) NULL,
  `ServiceStartDate` datetime(6) NULL,
  `ServiceEndDate` datetime(6) NULL,
  `IsOptional` tinyint(1) NOT NULL DEFAULT 0,
  `IsIncluded` tinyint(1) NOT NULL DEFAULT 1,
  `ParentLineItemId` int NULL,
  `IsBundle` tinyint(1) NOT NULL DEFAULT 0,
  `InternalNotes` varchar(2000) NULL,
  `QuoteNotes` varchar(2000) NULL,
  `CustomFields` text NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  `RowVersion` binary(8) NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `FK_QuoteLineItems_Quotes_QuoteId` FOREIGN KEY (`QuoteId`) REFERENCES `Quotes` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 3. Subscriptions
CREATE TABLE IF NOT EXISTS `Subscriptions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `SubscriptionNumber` varchar(100) NOT NULL,
  `AccountId` int NOT NULL,
  `ProductId` int NULL,
  `SubscriptionStatus` int NOT NULL DEFAULT 0,
  `MRR` decimal(65,30) NULL,
  `ARR` decimal(65,30) NULL,
  `OneTimeFee` decimal(65,30) NULL,
  `Currency` varchar(10) NULL,
  `CurrencyLookupId` int NULL,
  `BillingCycle` varchar(50) NULL,
  `BillingStartDate` datetime(6) NULL,
  `BillingEndDate` datetime(6) NULL,
  `ContractReference` varchar(200) NULL,
  `ContractStartDate` datetime(6) NULL,
  `ContractEndDate` datetime(6) NULL,
  `TermCategory` int NULL,
  `ServiceTier` int NULL,
  `SLA` varchar(255) NULL,
  `ContractNotes` text NULL,
  `BillingAddress` varchar(255) NULL,
  `BillingCity` varchar(100) NULL,
  `BillingState` varchar(100) NULL,
  `BillingZip` varchar(20) NULL,
  `BillingCountry` varchar(100) NULL,
  `BillingContactName` varchar(255) NULL,
  `BillingContactEmail` varchar(255) NULL,
  `BillingContactPhone` varchar(30) NULL,
  `ContractFileName` varchar(1000) NULL,
  `ContractFilePath` varchar(2000) NULL,
  `ContractContentType` varchar(200) NULL,
  `ContractFileSize` bigint NULL,
  `IsAutoRenew` tinyint(1) NOT NULL DEFAULT 0,
  `RenewalDate` datetime(6) NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `SubscriptionOwner` varchar(255) NULL,
  `SubscriptionManagerId` int NULL,
  `Tags` varchar(500) NULL,
  `ExternalReference` varchar(50) NULL,
  `OrderId` int NULL,
  `Amount` decimal(65,30) NOT NULL DEFAULT 0,
  `StartDate` datetime(6) NULL,
  `EndDate` datetime(6) NULL,
  `NextBillingDate` datetime(6) NULL,
  `CurrentPeriodEnd` datetime(6) NULL,
  `CurrentPeriodStart` datetime(6) NULL,
  `CancelledAt` datetime(6) NULL,
  `CancellationReason` text NULL,
  `CancelAtPeriodEnd` tinyint(1) NOT NULL DEFAULT 0,
  `PausedAt` datetime(6) NULL,
  `PauseReason` text NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  `RowVersion` binary(8) NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `FK_Subscriptions_Accounts_AccountId` FOREIGN KEY (`AccountId`) REFERENCES `Accounts` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 4. Invoices
CREATE TABLE IF NOT EXISTS `Invoices` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `InvoiceNumber` varchar(50) NOT NULL,
  `ExternalInvoiceId` varchar(100) NULL,
  `ReferenceNumber` varchar(100) NULL,
  `BatchNumber` varchar(50) NULL,
  `Description` varchar(1000) NULL,
  `Status` int NOT NULL DEFAULT 0,
  `InvoiceType` int NOT NULL DEFAULT 0,
  `PaymentTerms` int NOT NULL DEFAULT 0,
  `PaymentTermsDescription` varchar(500) NULL,
  `InvoiceDate` datetime(6) NOT NULL,
  `DueDate` datetime(6) NOT NULL,
  `SentDate` datetime(6) NULL,
  `ViewedDate` datetime(6) NULL,
  `PaidDate` datetime(6) NULL,
  `VoidedDate` datetime(6) NULL,
  `ServicePeriodStart` datetime(6) NULL,
  `ServicePeriodEnd` datetime(6) NULL,
  `Subtotal` decimal(65,30) NOT NULL DEFAULT 0,
  `DiscountAmount` decimal(65,30) NOT NULL DEFAULT 0,
  `DiscountPercent` decimal(65,30) NOT NULL DEFAULT 0,
  `TaxAmount` decimal(65,30) NOT NULL DEFAULT 0,
  `TaxRate` decimal(65,30) NOT NULL DEFAULT 0,
  `ShippingAmount` decimal(65,30) NOT NULL DEFAULT 0,
  `FeesAmount` decimal(65,30) NOT NULL DEFAULT 0,
  `TotalAmount` decimal(65,30) NOT NULL DEFAULT 0,
  `AmountPaid` decimal(65,30) NOT NULL DEFAULT 0,
  `AmountCredited` decimal(65,30) NOT NULL DEFAULT 0,
  `CurrencyCode` varchar(3) NOT NULL DEFAULT 'USD',
  `ExchangeRate` decimal(65,30) NULL,
  `EarlyPaymentDiscountPercent` decimal(65,30) NULL,
  `EarlyPaymentDiscountDays` int NULL,
  `EarlyPaymentDiscountAmount` decimal(65,30) NULL,
  `LateFeePercent` decimal(65,30) NULL,
  `LateFeeAmount` decimal(65,30) NULL,
  `LateFeeTotal` decimal(65,30) NOT NULL DEFAULT 0,
  `BillingName` varchar(255) NULL,
  `BillingCompany` varchar(255) NULL,
  `BillingStreet` varchar(500) NULL,
  `BillingCity` varchar(100) NULL,
  `BillingState` varchar(100) NULL,
  `BillingPostalCode` varchar(20) NULL,
  `BillingCountry` varchar(100) NULL,
  `BillingEmail` varchar(255) NULL,
  `BillingPhone` varchar(30) NULL,
  `ReminderCount` int NOT NULL DEFAULT 0,
  `LastReminderDate` datetime(6) NULL,
  `NextReminderDate` datetime(6) NULL,
  `InCollections` tinyint(1) NOT NULL DEFAULT 0,
  `CollectionsDate` datetime(6) NULL,
  `CollectionsReference` varchar(100) NULL,
  `AccountId` int NOT NULL,
  `OrderId` int NULL,
  `SubscriptionId` int NULL,
  `ContactId` int NULL,
  `VoidedById` int NULL,
  `OriginalInvoiceId` int NULL,
  `Notes` varchar(2000) NULL,
  `InternalNotes` varchar(2000) NULL,
  `Footer` varchar(1000) NULL,
  `TermsAndConditions` text NULL,
  `VoidReason` varchar(500) NULL,
  `DisputeReason` varchar(500) NULL,
  `PdfUrl` varchar(500) NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  `RowVersion` binary(8) NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `FK_Invoices_Accounts_AccountId` FOREIGN KEY (`AccountId`) REFERENCES `Accounts` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 5. InvoiceLineItems
CREATE TABLE IF NOT EXISTS `InvoiceLineItems` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `LineNumber` int NOT NULL DEFAULT 0,
  `ExternalLineId` varchar(100) NULL,
  `Name` varchar(255) NOT NULL,
  `Description` varchar(1000) NULL,
  `SKU` varchar(50) NULL,
  `ProductCode` varchar(50) NULL,
  `Quantity` decimal(65,30) NOT NULL DEFAULT 0,
  `UnitOfMeasure` varchar(50) NULL,
  `UnitPrice` decimal(65,30) NOT NULL DEFAULT 0,
  `DiscountAmount` decimal(65,30) NOT NULL DEFAULT 0,
  `DiscountPercent` decimal(65,30) NOT NULL DEFAULT 0,
  `ExtendedAmount` decimal(65,30) NOT NULL DEFAULT 0,
  `TaxAmount` decimal(65,30) NOT NULL DEFAULT 0,
  `TaxRate` decimal(65,30) NULL,
  `TotalAmount` decimal(65,30) NOT NULL DEFAULT 0,
  `ServiceStartDate` datetime(6) NULL,
  `ServiceEndDate` datetime(6) NULL,
  `RevenueRecognitionStartDate` datetime(6) NULL,
  `RevenueRecognitionEndDate` datetime(6) NULL,
  `DeferredRevenue` decimal(65,30) NULL,
  `RecognizedRevenue` decimal(65,30) NULL,
  `InvoiceId` int NOT NULL,
  `ProductId` int NULL,
  `OrderLineItemId` int NULL,
  `SubscriptionId` int NULL,
  `Notes` varchar(1000) NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  `RowVersion` binary(8) NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `FK_InvoiceLineItems_Invoices_InvoiceId` FOREIGN KEY (`InvoiceId`) REFERENCES `Invoices` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 6. Contracts
CREATE TABLE IF NOT EXISTS `Contracts` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ContractNumber` text NOT NULL,
  `Name` text NOT NULL,
  `Description` text NULL,
  `Status` int NOT NULL DEFAULT 0,
  `ContractType` int NOT NULL DEFAULT 0,
  `AccountId` int NOT NULL,
  `ContactId` int NULL,
  `OwnerId` int NULL,
  `ParentContractId` int NULL,
  `OpportunityId` int NULL,
  `QuoteId` int NULL,
  `StartDate` datetime(6) NOT NULL,
  `EndDate` datetime(6) NOT NULL,
  `SignedDate` datetime(6) NULL,
  `ActivatedDate` datetime(6) NULL,
  `TerminatedDate` datetime(6) NULL,
  `Value` decimal(65,30) NOT NULL DEFAULT 0,
  `CurrencyCode` text NULL,
  `BillingFrequency` text NULL,
  `AutoRenew` tinyint(1) NOT NULL DEFAULT 0,
  `RenewalNoticeDays` int NOT NULL DEFAULT 30,
  `RenewalNoticeSent` tinyint(1) NOT NULL DEFAULT 0,
  `RenewalNoticeSentDate` datetime(6) NULL,
  `RenewalInitiatedAt` datetime(6) NULL,
  `RenewalCompletedAt` datetime(6) NULL,
  `Terms` text NULL,
  `SpecialConditions` text NULL,
  `TerminationClause` text NULL,
  `ContractFileUrl` text NULL,
  `ContractFileName` text NULL,
  `ContractFileSize` bigint NULL,
  `ContractFileMimeType` text NULL,
  `SignedContractFileUrl` text NULL,
  `SignedContractFileName` text NULL,
  `ApprovedByUserId` int NULL,
  `ApprovedDate` datetime(6) NULL,
  `RejectionReason` text NULL,
  `SuspensionReason` text NULL,
  `SuspendedDate` datetime(6) NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  `RowVersion` binary(8) NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `FK_Contracts_Accounts_AccountId` FOREIGN KEY (`AccountId`) REFERENCES `Accounts` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 7. Payments
CREATE TABLE IF NOT EXISTS `Payments` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `PaymentNumber` text NOT NULL,
  `ExternalPaymentId` text NULL,
  `GatewayTransactionId` text NULL,
  `GatewayReference` text NULL,
  `AuthorizationCode` text NULL,
  `CheckNumber` text NULL,
  `Description` text NULL,
  `Status` int NOT NULL DEFAULT 0,
  `PaymentMethod` int NOT NULL DEFAULT 0,
  `PaymentType` int NOT NULL DEFAULT 0,
  `Amount` decimal(65,30) NOT NULL DEFAULT 0,
  `AmountApplied` decimal(65,30) NOT NULL DEFAULT 0,
  `ProcessingFee` decimal(65,30) NOT NULL DEFAULT 0,
  `RefundedAmount` decimal(65,30) NOT NULL DEFAULT 0,
  `CurrencyCode` text NOT NULL,
  `ExchangeRate` decimal(65,30) NULL,
  `PaymentDate` datetime(6) NOT NULL,
  `ProcessedDate` datetime(6) NULL,
  `SettledDate` datetime(6) NULL,
  `RefundDate` datetime(6) NULL,
  `DepositDate` datetime(6) NULL,
  `CardBrand` text NULL,
  `CardLast4` text NULL,
  `CardExpMonth` int NULL,
  `CardExpYear` int NULL,
  `CardholderName` text NULL,
  `BankName` text NULL,
  `AccountLast4` text NULL,
  `AccountType` text NULL,
  `RoutingNumberLast4` text NULL,
  `Gateway` text NULL,
  `GatewayResponseCode` text NULL,
  `GatewayResponseMessage` text NULL,
  `AvsResponseCode` text NULL,
  `CvvResponseCode` text NULL,
  `RiskScore` decimal(65,30) NULL,
  `GatewayResponseRaw` text NULL,
  `FraudFlagged` tinyint(1) NOT NULL DEFAULT 0,
  `FraudNotes` text NULL,
  `IpAddress` text NULL,
  `DeviceFingerprint` text NULL,
  `AccountId` int NOT NULL,
  `InvoiceId` int NULL,
  `OrderId` int NULL,
  `SubscriptionId` int NULL,
  `OriginalPaymentId` int NULL,
  `ProcessedById` int NULL,
  `ScheduledDate` datetime(6) NULL,
  `RetryCount` int NOT NULL DEFAULT 0,
  `BankReference` text NULL,
  `IsReconciled` tinyint(1) NOT NULL DEFAULT 0,
  `ReconciledDate` datetime(6) NULL,
  `Notes` text NULL,
  `InternalNotes` text NULL,
  `FailureReason` text NULL,
  `RefundReason` text NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  `RowVersion` binary(8) NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `FK_Payments_Accounts_AccountId` FOREIGN KEY (`AccountId`) REFERENCES `Accounts` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Payments_Invoices_InvoiceId` FOREIGN KEY (`InvoiceId`) REFERENCES `Invoices` (`Id`),
  CONSTRAINT `FK_Payments_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `Orders` (`Id`),
  CONSTRAINT `FK_Payments_Payments_OriginalPaymentId` FOREIGN KEY (`OriginalPaymentId`) REFERENCES `Payments` (`Id`),
  CONSTRAINT `FK_Payments_Subscriptions_SubscriptionId` FOREIGN KEY (`SubscriptionId`) REFERENCES `Subscriptions` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 8. Commissions
CREATE TABLE IF NOT EXISTS `Commissions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CommissionNumber` text NOT NULL,
  `Status` int NOT NULL DEFAULT 0,
  `CommissionPeriod` text NOT NULL,
  `PeriodStartDate` datetime(6) NOT NULL,
  `PeriodEndDate` datetime(6) NOT NULL,
  `DealAmount` decimal(65,30) NOT NULL DEFAULT 0,
  `CommissionableAmount` decimal(65,30) NOT NULL DEFAULT 0,
  `CommissionRate` decimal(65,30) NOT NULL DEFAULT 0,
  `CommissionAmount` decimal(65,30) NOT NULL DEFAULT 0,
  `SplitPercent` decimal(65,30) NOT NULL DEFAULT 0,
  `FinalCommissionAmount` decimal(65,30) NOT NULL DEFAULT 0,
  `CurrencyCode` text NOT NULL,
  `QuotaAmount` decimal(65,30) NULL,
  `AttainmentPercent` decimal(65,30) NULL,
  `TierName` text NULL,
  `Multiplier` decimal(65,30) NULL,
  `EarnedDate` datetime(6) NOT NULL,
  `ApprovedDate` datetime(6) NULL,
  `PaidDate` datetime(6) NULL,
  `ClawbackEndDate` datetime(6) NULL,
  `ClawbackDate` datetime(6) NULL,
  `AdjustmentAmount` decimal(65,30) NOT NULL DEFAULT 0,
  `AdjustmentReason` text NULL,
  `ClawbackAmount` decimal(65,30) NOT NULL DEFAULT 0,
  `ClawbackReason` text NULL,
  `UserId` int NOT NULL,
  `CommissionPlanId` int NOT NULL,
  `OpportunityId` int NULL,
  `OrderId` int NULL,
  `InvoiceId` int NULL,
  `SubscriptionId` int NULL,
  `OriginalCommissionId` int NULL,
  `ApprovedById` int NULL,
  `Notes` text NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  `RowVersion` binary(8) NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `FK_Commissions_CommissionPlans_CommissionPlanId` FOREIGN KEY (`CommissionPlanId`) REFERENCES `CommissionPlans` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Commissions_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;



-- ─────────────────────────────────────────────────────────────────────────────
-- Source: 010_itsm_module.sql
-- ─────────────────────────────────────────────────────────────────────────────


-- ITSM Module Database Migration
-- This script creates all tables for the ITSM enhancement (Incident, Problem, SLA, CMDB, Change, Knowledge, Service Catalog)
-- Author: CRM Solution Contributors
-- Date: 2026-02-02
-- License: AGPL-3.0




-- ====================================
-- Phase 1.1: Incident Management
-- ====================================

CREATE TABLE IF NOT EXISTS Incidents (
    IncidentId INT PRIMARY KEY AUTO_INCREMENT,
    Number VARCHAR(20) NOT NULL UNIQUE,
    ShortDescription VARCHAR(160) NOT NULL,
    Description TEXT,
    
    -- Caller Information
    CallerId INT NOT NULL,
    ContactType INT NOT NULL,
    OpenedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    OpenedById INT,
    
    -- Classification
    CategoryId INT,
    SubcategoryId INT,
    ConfigurationItemId INT,
    ServiceId INT,
    
    -- Prioritization
    Impact INT NOT NULL,
    Urgency INT NOT NULL,
    
    -- Assignment
    State INT NOT NULL DEFAULT 1,
    AssignmentGroupId INT,
    AssignedToId INT,
    EscalationLevel INT DEFAULT 0,
    
    -- Resolution
    ResolutionCode INT,
    ResolutionNotes TEXT,
    ResolvedAt DATETIME,
    ResolvedById INT,
    ClosedAt DATETIME,
    ClosedById INT,
    
    -- SLA
    SLABreached BOOLEAN DEFAULT FALSE,
    ResponseDueAt DATETIME,
    ResolutionDueAt DATETIME,
    BusinessElapsedMinutes INT,
    
    -- Relationships
    MajorIncident BOOLEAN DEFAULT FALSE,
    ParentIncidentId INT,
    ProblemId INT,
    ChangeRequestId INT,
    
    -- Audit
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_incidents_caller (CallerId),
    INDEX idx_incidents_assigned (AssignedToId),
    INDEX idx_incidents_state (State),
    INDEX idx_incidents_priority (Impact, Urgency),
    INDEX idx_incidents_category (CategoryId),
    INDEX idx_incidents_created (CreatedAt),
    INDEX idx_incidents_sla_response (ResponseDueAt),
    INDEX idx_incidents_sla_resolution (ResolutionDueAt),
    
    FOREIGN KEY (CallerId) REFERENCES Users(UserId),
    FOREIGN KEY (OpenedById) REFERENCES Users(UserId),
    FOREIGN KEY (CategoryId) REFERENCES ServiceRequestCategories(CategoryId),
    FOREIGN KEY (SubcategoryId) REFERENCES ServiceRequestSubcategories(SubcategoryId),
    FOREIGN KEY (AssignmentGroupId) REFERENCES UserGroups(GroupId),
    FOREIGN KEY (AssignedToId) REFERENCES Users(UserId),
    FOREIGN KEY (ResolvedById) REFERENCES Users(UserId),
    FOREIGN KEY (ClosedById) REFERENCES Users(UserId),
    FOREIGN KEY (ParentIncidentId) REFERENCES Incidents(IncidentId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS IncidentComments (
    CommentId INT PRIMARY KEY AUTO_INCREMENT,
    IncidentId INT NOT NULL,
    Comment TEXT NOT NULL,
    IsInternal BOOLEAN DEFAULT FALSE,
    CreatedById INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_incident_comments_incident (IncidentId),
    INDEX idx_incident_comments_created (CreatedAt),
    
    FOREIGN KEY (IncidentId) REFERENCES Incidents(IncidentId) ON DELETE CASCADE,
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS IncidentAttachments (
    AttachmentId INT PRIMARY KEY AUTO_INCREMENT,
    IncidentId INT NOT NULL,
    FileName VARCHAR(255) NOT NULL,
    FilePath VARCHAR(500) NOT NULL,
    ContentType VARCHAR(100),
    FileSize BIGINT NOT NULL,
    UploadedById INT NOT NULL,
    UploadedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_incident_attachments_incident (IncidentId),
    
    FOREIGN KEY (IncidentId) REFERENCES Incidents(IncidentId) ON DELETE CASCADE,
    FOREIGN KEY (UploadedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS IncidentHistory (
    HistoryId INT PRIMARY KEY AUTO_INCREMENT,
    IncidentId INT NOT NULL,
    Field VARCHAR(100) NOT NULL,
    OldValue TEXT,
    NewValue TEXT,
    ChangedById INT NOT NULL,
    ChangedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    INDEX idx_incident_history_incident (IncidentId),
    INDEX idx_incident_history_changed (ChangedAt),
    
    FOREIGN KEY (IncidentId) REFERENCES Incidents(IncidentId) ON DELETE CASCADE,
    FOREIGN KEY (ChangedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ====================================
-- Phase 1.2: Problem Management
-- ====================================

CREATE TABLE IF NOT EXISTS Problems (
    ProblemId INT PRIMARY KEY AUTO_INCREMENT,
    Number VARCHAR(20) NOT NULL UNIQUE,
    ShortDescription VARCHAR(160) NOT NULL,
    Description TEXT,
    
    -- Classification
    CategoryId INT,
    SubcategoryId INT,
    ConfigurationItemId INT,
    Priority INT NOT NULL,
    
    -- Analysis
    Symptoms TEXT,
    RootCause TEXT,
    Workaround TEXT,
    KnownError BOOLEAN DEFAULT FALSE,
    KnownErrorDate DATETIME,
    
    -- Assignment
    State INT NOT NULL DEFAULT 1,
    ProblemInvestigatorId INT,
    ProblemManagerId INT,
    AssignmentGroupId INT,
    
    -- Resolution
    Solution TEXT,
    ResolutionCode VARCHAR(100),
    ResolvedAt DATETIME,
    FixVerified BOOLEAN DEFAULT FALSE,
    VerifiedAt DATETIME,
    KnowledgeArticleId INT,
    
    -- RCA Details
    FiveWhysAnalysis TEXT,
    FishboneAnalysis TEXT,
    Timeline TEXT,
    
    -- Closure
    ClosedAt DATETIME,
    ClosedById INT,
    ClosureNotes TEXT,
    
    -- Audit
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_problems_state (State),
    INDEX idx_problems_priority (Priority),
    INDEX idx_problems_category (CategoryId),
    INDEX idx_problems_known_error (KnownError),
    
    FOREIGN KEY (CategoryId) REFERENCES ServiceRequestCategories(CategoryId),
    FOREIGN KEY (SubcategoryId) REFERENCES ServiceRequestSubcategories(SubcategoryId),
    FOREIGN KEY (ProblemInvestigatorId) REFERENCES Users(UserId),
    FOREIGN KEY (ProblemManagerId) REFERENCES Users(UserId),
    FOREIGN KEY (AssignmentGroupId) REFERENCES UserGroups(GroupId),
    FOREIGN KEY (ClosedById) REFERENCES Users(UserId),
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ProblemIncidents (
    ProblemIncidentId INT PRIMARY KEY AUTO_INCREMENT,
    ProblemId INT NOT NULL,
    IncidentId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    
    INDEX idx_problem_incidents_problem (ProblemId),
    INDEX idx_problem_incidents_incident (IncidentId),
    
    FOREIGN KEY (ProblemId) REFERENCES Problems(ProblemId) ON DELETE CASCADE,
    FOREIGN KEY (IncidentId) REFERENCES Incidents(IncidentId),
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ProblemTasks (
    TaskId INT PRIMARY KEY AUTO_INCREMENT,
    ProblemId INT NOT NULL,
    TaskName VARCHAR(200) NOT NULL,
    Description TEXT,
    AssignedToId INT,
    DueDate DATETIME,
    IsCompleted BOOLEAN DEFAULT FALSE,
    CompletedAt DATETIME,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_problem_tasks_problem (ProblemId),
    INDEX idx_problem_tasks_assigned (AssignedToId),
    
    FOREIGN KEY (ProblemId) REFERENCES Problems(ProblemId) ON DELETE CASCADE,
    FOREIGN KEY (AssignedToId) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ProblemComments (
    CommentId INT PRIMARY KEY AUTO_INCREMENT,
    ProblemId INT NOT NULL,
    Comment TEXT NOT NULL,
    IsInternal BOOLEAN DEFAULT TRUE,
    CreatedById INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_problem_comments_problem (ProblemId),
    
    FOREIGN KEY (ProblemId) REFERENCES Problems(ProblemId) ON DELETE CASCADE,
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ProblemAttachments (
    AttachmentId INT PRIMARY KEY AUTO_INCREMENT,
    ProblemId INT NOT NULL,
    FileName VARCHAR(255) NOT NULL,
    FilePath VARCHAR(500) NOT NULL,
    ContentType VARCHAR(100),
    FileSize BIGINT NOT NULL,
    UploadedById INT NOT NULL,
    UploadedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_problem_attachments_problem (ProblemId),
    
    FOREIGN KEY (ProblemId) REFERENCES Problems(ProblemId) ON DELETE CASCADE,
    FOREIGN KEY (UploadedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Add FK from Incidents to Problems now that Problems table exists
ALTER TABLE Incidents ADD CONSTRAINT fk_incidents_problem 
    FOREIGN KEY (ProblemId) REFERENCES Problems(ProblemId);

-- ====================================
-- Phase 1.3: SLA Management
-- ====================================

CREATE TABLE IF NOT EXISTS BusinessHoursSchedules (
    ScheduleId INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    TimeZone VARCHAR(100) DEFAULT 'UTC',
    BusinessHours TEXT,
    Holidays TEXT,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_business_hours_active (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS SLAPolicies (
    SLAPolicyId INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    TargetType INT NOT NULL,
    
    -- Response SLA (minutes)
    P1ResponseMinutes INT DEFAULT 15,
    P2ResponseMinutes INT DEFAULT 30,
    P3ResponseMinutes INT DEFAULT 120,
    P4ResponseMinutes INT DEFAULT 480,
    
    -- Resolution SLA (minutes)
    P1ResolutionMinutes INT DEFAULT 240,
    P2ResolutionMinutes INT DEFAULT 480,
    P3ResolutionMinutes INT DEFAULT 1440,
    P4ResolutionMinutes INT DEFAULT 7200,
    
    -- Business Hours
    UseBusinessHours BOOLEAN DEFAULT TRUE,
    BusinessHoursScheduleId INT,
    
    Conditions TEXT,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_sla_policies_target_type (TargetType),
    INDEX idx_sla_policies_active (IsActive),
    
    FOREIGN KEY (BusinessHoursScheduleId) REFERENCES BusinessHoursSchedules(ScheduleId),
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS SLAInstances (
    SLAInstanceId INT PRIMARY KEY AUTO_INCREMENT,
    TargetId INT NOT NULL,
    TargetType INT NOT NULL,
    SLAPolicyId INT NOT NULL,
    
    -- Response SLA
    ResponseDueAt DATETIME,
    ResponseActualAt DATETIME,
    ResponseBreached BOOLEAN DEFAULT FALSE,
    ResponseBusinessMinutes INT,
    
    -- Resolution SLA
    ResolutionDueAt DATETIME,
    ResolutionActualAt DATETIME,
    ResolutionBreached BOOLEAN DEFAULT FALSE,
    ResolutionBusinessMinutes INT,
    
    -- Tracking
    State INT NOT NULL DEFAULT 1,
    PausedAt DATETIME,
    PausedMinutes INT DEFAULT 0,
    PauseReason TEXT,
    
    -- Notifications
    Response50PercentNotificationSent BOOLEAN DEFAULT FALSE,
    Response75PercentNotificationSent BOOLEAN DEFAULT FALSE,
    ResponseBreachNotificationSent BOOLEAN DEFAULT FALSE,
    Resolution50PercentNotificationSent BOOLEAN DEFAULT FALSE,
    Resolution75PercentNotificationSent BOOLEAN DEFAULT FALSE,
    ResolutionBreachNotificationSent BOOLEAN DEFAULT FALSE,
    
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME,
    
    INDEX idx_sla_instances_target (TargetId, TargetType),
    INDEX idx_sla_instances_policy (SLAPolicyId),
    INDEX idx_sla_instances_response_due (ResponseDueAt),
    INDEX idx_sla_instances_resolution_due (ResolutionDueAt),
    INDEX idx_sla_instances_state (State),
    
    FOREIGN KEY (SLAPolicyId) REFERENCES SLAPolicies(SLAPolicyId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ====================================
-- Phase 2.1: CMDB
-- ====================================

CREATE TABLE IF NOT EXISTS ConfigurationItems (
    CIId INT PRIMARY KEY AUTO_INCREMENT,
    CIName VARCHAR(200) NOT NULL,
    CINumber VARCHAR(50) NOT NULL UNIQUE,
    CIType INT NOT NULL,
    CISubtype VARCHAR(50),
    Description TEXT,
    
    -- Identification
    SerialNumber VARCHAR(100),
    AssetTag VARCHAR(100),
    ModelNumber VARCHAR(100),
    Manufacturer VARCHAR(200),
    Version VARCHAR(50),
    
    -- Ownership
    OwnerId INT,
    SupportGroupId INT,
    ManagedById INT,
    DepartmentId INT,
    
    -- Status
    OperationalStatus INT NOT NULL,
    Environment INT,
    Criticality INT,
    
    -- Location
    PhysicalLocation VARCHAR(500),
    DataCenterId INT,
    RackLocation VARCHAR(100),
    
    -- Financial
    PurchaseDate DATE,
    PurchaseCost DECIMAL(18,2),
    VendorId INT,
    WarrantyExpiration DATE,
    LeaseExpiration DATE,
    
    -- Technical
    IPAddress VARCHAR(50),
    MACAddress VARCHAR(50),
    OperatingSystem VARCHAR(200),
    CPU VARCHAR(100),
    RAM VARCHAR(100),
    Disk VARCHAR(100),
    LastDiscovered DATETIME,
    
    -- Extended Attributes
    ExtendedAttributes TEXT,
    
    -- Audit
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_ci_number (CINumber),
    INDEX idx_ci_type (CIType),
    INDEX idx_ci_status (OperationalStatus),
    INDEX idx_ci_owner (OwnerId),
    INDEX idx_ci_serial (SerialNumber),
    INDEX idx_ci_ip (IPAddress),
    
    FOREIGN KEY (OwnerId) REFERENCES Users(UserId),
    FOREIGN KEY (SupportGroupId) REFERENCES UserGroups(GroupId),
    FOREIGN KEY (ManagedById) REFERENCES Users(UserId),
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS CIRelationships (
    RelationshipId INT PRIMARY KEY AUTO_INCREMENT,
    ParentCIId INT NOT NULL,
    ChildCIId INT NOT NULL,
    RelationshipType INT NOT NULL,
    Description VARCHAR(500),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_ci_relationships_parent (ParentCIId),
    INDEX idx_ci_relationships_child (ChildCIId),
    INDEX idx_ci_relationships_type (RelationshipType),
    
    FOREIGN KEY (ParentCIId) REFERENCES ConfigurationItems(CIId),
    FOREIGN KEY (ChildCIId) REFERENCES ConfigurationItems(CIId),
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS Services (
    ServiceId INT PRIMARY KEY AUTO_INCREMENT,
    ServiceName VARCHAR(200) NOT NULL,
    ServiceNumber VARCHAR(50) UNIQUE,
    Description TEXT,
    ServiceType INT NOT NULL,
    
    OwnerId INT,
    TechnicalOwnerId INT,
    SupportGroupId INT,
    
    Criticality INT,
    AvailabilityTarget DECIMAL(5,2),
    
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME,
    IsActive BOOLEAN DEFAULT TRUE,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_services_type (ServiceType),
    INDEX idx_services_owner (OwnerId),
    INDEX idx_services_active (IsActive),
    
    FOREIGN KEY (OwnerId) REFERENCES Users(UserId),
    FOREIGN KEY (TechnicalOwnerId) REFERENCES Users(UserId),
    FOREIGN KEY (SupportGroupId) REFERENCES UserGroups(GroupId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ServiceCIs (
    ServiceCIId INT PRIMARY KEY AUTO_INCREMENT,
    ServiceId INT NOT NULL,
    CIId INT NOT NULL,
    DependencyType INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_service_cis_service (ServiceId),
    INDEX idx_service_cis_ci (CIId),
    
    FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId) ON DELETE CASCADE,
    FOREIGN KEY (CIId) REFERENCES ConfigurationItems(CIId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Add FK from Incidents to ConfigurationItems and Services now that tables exist
ALTER TABLE Incidents ADD CONSTRAINT fk_incidents_ci 
    FOREIGN KEY (ConfigurationItemId) REFERENCES ConfigurationItems(CIId);
    
ALTER TABLE Incidents ADD CONSTRAINT fk_incidents_service 
    FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId);

ALTER TABLE Problems ADD CONSTRAINT fk_problems_ci 
    FOREIGN KEY (ConfigurationItemId) REFERENCES ConfigurationItems(CIId);

-- ====================================
-- Phase 2.2: Change Management
-- ====================================

CREATE TABLE IF NOT EXISTS Changes (
    ChangeId INT PRIMARY KEY AUTO_INCREMENT,
    Number VARCHAR(20) NOT NULL UNIQUE,
    ShortDescription VARCHAR(160) NOT NULL,
    Description TEXT,
    Type INT NOT NULL,
    
    -- Classification
    CategoryId INT,
    ConfigurationItemId INT,
    ServiceId INT,
    
    -- Planning
    RequestorId INT NOT NULL,
    AssignedToId INT,
    ImplementationGroupId INT,
    PlannedStartDate DATETIME,
    PlannedEndDate DATETIME,
    EstimatedDurationMinutes INT,
    MaintenanceWindow BOOLEAN DEFAULT FALSE,
    
    -- Risk Assessment
    Risk INT NOT NULL,
    Impact INT NOT NULL,
    RiskAssessmentNotes TEXT,
    RiskMitigationPlan TEXT,
    
    -- Implementation
    ImplementationPlan TEXT,
    BackoutPlan TEXT,
    TestingPlan TEXT,
    ImplementationNotes TEXT,
    
    -- Approval
    ApprovalStatus INT NOT NULL DEFAULT 1,
    CABDate DATETIME,
    ApprovalNotes TEXT,
    
    -- State
    State INT NOT NULL DEFAULT 1,
    
    -- Closure
    ActualStartDate DATETIME,
    ActualEndDate DATETIME,
    ChangeSuccess BOOLEAN,
    ClosureCode VARCHAR(100),
    ClosureNotes TEXT,
    PostImplementationReview TEXT,
    ReviewDate DATETIME,
    
    -- Tracking
    ConflictDetected BOOLEAN DEFAULT FALSE,
    ConflictDetails TEXT,
    
    -- Audit
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_changes_type (Type),
    INDEX idx_changes_state (State),
    INDEX idx_changes_approval (ApprovalStatus),
    INDEX idx_changes_planned_start (PlannedStartDate),
    INDEX idx_changes_requestor (RequestorId),
    INDEX idx_changes_ci (ConfigurationItemId),
    
    FOREIGN KEY (CategoryId) REFERENCES ServiceRequestCategories(CategoryId),
    FOREIGN KEY (ConfigurationItemId) REFERENCES ConfigurationItems(CIId),
    FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId),
    FOREIGN KEY (RequestorId) REFERENCES Users(UserId),
    FOREIGN KEY (AssignedToId) REFERENCES Users(UserId),
    FOREIGN KEY (ImplementationGroupId) REFERENCES UserGroups(GroupId),
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ChangeApprovals (
    ApprovalId INT PRIMARY KEY AUTO_INCREMENT,
    ChangeId INT NOT NULL,
    ApproverId INT NOT NULL,
    ApprovalRole INT NOT NULL,
    ApprovalStatus INT NOT NULL DEFAULT 1,
    ApprovalDate DATETIME,
    Comments TEXT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_change_approvals_change (ChangeId),
    INDEX idx_change_approvals_approver (ApproverId),
    INDEX idx_change_approvals_status (ApprovalStatus),
    
    FOREIGN KEY (ChangeId) REFERENCES Changes(ChangeId) ON DELETE CASCADE,
    FOREIGN KEY (ApproverId) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ChangeBlackouts (
    BlackoutId INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(200) NOT NULL,
    Description VARCHAR(500),
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    Reason VARCHAR(500),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_change_blackouts_dates (StartDate, EndDate),
    
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ChangeImpactedCIs (
    ChangeImpactedCIId INT PRIMARY KEY AUTO_INCREMENT,
    ChangeId INT NOT NULL,
    CIId INT NOT NULL,
    Impact INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_change_impacted_cis_change (ChangeId),
    INDEX idx_change_impacted_cis_ci (CIId),
    
    FOREIGN KEY (ChangeId) REFERENCES Changes(ChangeId) ON DELETE CASCADE,
    FOREIGN KEY (CIId) REFERENCES ConfigurationItems(CIId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ChangeTasks (
    TaskId INT PRIMARY KEY AUTO_INCREMENT,
    ChangeId INT NOT NULL,
    TaskName VARCHAR(200) NOT NULL,
    Description TEXT,
    AssignedToId INT,
    PlannedStartDate DATETIME,
    PlannedEndDate DATETIME,
    ActualStartDate DATETIME,
    ActualEndDate DATETIME,
    IsCompleted BOOLEAN DEFAULT FALSE,
    DisplayOrder INT DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_change_tasks_change (ChangeId),
    INDEX idx_change_tasks_assigned (AssignedToId),
    INDEX idx_change_tasks_order (ChangeId, DisplayOrder),
    
    FOREIGN KEY (ChangeId) REFERENCES Changes(ChangeId) ON DELETE CASCADE,
    FOREIGN KEY (AssignedToId) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ChangeComments (
    CommentId INT PRIMARY KEY AUTO_INCREMENT,
    ChangeId INT NOT NULL,
    Comment TEXT NOT NULL,
    IsInternal BOOLEAN DEFAULT TRUE,
    CreatedById INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_change_comments_change (ChangeId),
    
    FOREIGN KEY (ChangeId) REFERENCES Changes(ChangeId) ON DELETE CASCADE,
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ChangeAttachments (
    AttachmentId INT PRIMARY KEY AUTO_INCREMENT,
    ChangeId INT NOT NULL,
    FileName VARCHAR(255) NOT NULL,
    FilePath VARCHAR(500) NOT NULL,
    ContentType VARCHAR(100),
    FileSize BIGINT NOT NULL,
    UploadedById INT NOT NULL,
    UploadedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_change_attachments_change (ChangeId),
    
    FOREIGN KEY (ChangeId) REFERENCES Changes(ChangeId) ON DELETE CASCADE,
    FOREIGN KEY (UploadedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Add FK from Incidents to Changes now that Changes table exists
ALTER TABLE Incidents ADD CONSTRAINT fk_incidents_change 
    FOREIGN KEY (ChangeRequestId) REFERENCES Changes(ChangeId);

-- ====================================
-- Phase 3.1: Knowledge Management
-- ====================================

CREATE TABLE IF NOT EXISTS KnowledgeArticles (
    ArticleId INT PRIMARY KEY AUTO_INCREMENT,
    Number VARCHAR(20) NOT NULL UNIQUE,
    Title VARCHAR(200) NOT NULL,
    ShortDescription VARCHAR(500),
    ArticleBody TEXT NOT NULL,
    ArticleType INT NOT NULL,
    CategoryId INT,
    SubcategoryId INT,
    
    -- Publishing
    AuthorId INT NOT NULL,
    OwnerId INT NOT NULL,
    PublishingState INT NOT NULL DEFAULT 1,
    PublishedDate DATETIME,
    PublishedById INT,
    ReviewDate DATETIME,
    ExpirationDate DATETIME,
    Version INT DEFAULT 1,
    
    -- Audience
    IsInternal BOOLEAN DEFAULT TRUE,
    IsExternal BOOLEAN DEFAULT FALSE,
    IsPublic BOOLEAN DEFAULT FALSE,
    
    -- Metadata
    Tags TEXT,
    
    -- Metrics
    ViewCount INT DEFAULT 0,
    HelpfulCount INT DEFAULT 0,
    NotHelpfulCount INT DEFAULT 0,
    AttachedToIncidentCount INT DEFAULT 0,
    LastViewedAt DATETIME,
    
    -- Audit
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME,
    ModifiedById INT,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_knowledge_number (Number),
    INDEX idx_knowledge_type (ArticleType),
    INDEX idx_knowledge_state (PublishingState),
    INDEX idx_knowledge_category (CategoryId),
    INDEX idx_knowledge_author (AuthorId),
    INDEX idx_knowledge_published (PublishedDate),
    FULLTEXT INDEX idx_knowledge_search (Title, ShortDescription, ArticleBody),
    
    FOREIGN KEY (CategoryId) REFERENCES ServiceRequestCategories(CategoryId),
    FOREIGN KEY (SubcategoryId) REFERENCES ServiceRequestSubcategories(SubcategoryId),
    FOREIGN KEY (AuthorId) REFERENCES Users(UserId),
    FOREIGN KEY (OwnerId) REFERENCES Users(UserId),
    FOREIGN KEY (PublishedById) REFERENCES Users(UserId),
    FOREIGN KEY (ModifiedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ArticleRelationships (
    RelationshipId INT PRIMARY KEY AUTO_INCREMENT,
    ArticleId INT NOT NULL,
    RelatedArticleId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_article_relationships_article (ArticleId),
    INDEX idx_article_relationships_related (RelatedArticleId),
    
    FOREIGN KEY (ArticleId) REFERENCES KnowledgeArticles(ArticleId) ON DELETE CASCADE,
    FOREIGN KEY (RelatedArticleId) REFERENCES KnowledgeArticles(ArticleId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ArticleIncidents (
    ArticleIncidentId INT PRIMARY KEY AUTO_INCREMENT,
    ArticleId INT NOT NULL,
    IncidentId INT NOT NULL,
    UsedToResolve BOOLEAN DEFAULT FALSE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_article_incidents_article (ArticleId),
    INDEX idx_article_incidents_incident (IncidentId),
    
    FOREIGN KEY (ArticleId) REFERENCES KnowledgeArticles(ArticleId) ON DELETE CASCADE,
    FOREIGN KEY (IncidentId) REFERENCES Incidents(IncidentId),
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ArticleFeedback (
    FeedbackId INT PRIMARY KEY AUTO_INCREMENT,
    ArticleId INT NOT NULL,
    UserId INT,
    IsHelpful BOOLEAN NOT NULL,
    Comment TEXT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_article_feedback_article (ArticleId),
    INDEX idx_article_feedback_user (UserId),
    
    FOREIGN KEY (ArticleId) REFERENCES KnowledgeArticles(ArticleId) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ArticleAttachments (
    AttachmentId INT PRIMARY KEY AUTO_INCREMENT,
    ArticleId INT NOT NULL,
    FileName VARCHAR(255) NOT NULL,
    FilePath VARCHAR(500) NOT NULL,
    ContentType VARCHAR(100),
    FileSize BIGINT NOT NULL,
    UploadedById INT NOT NULL,
    UploadedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_article_attachments_article (ArticleId),
    
    FOREIGN KEY (ArticleId) REFERENCES KnowledgeArticles(ArticleId) ON DELETE CASCADE,
    FOREIGN KEY (UploadedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Add FK from Problems to KnowledgeArticles now that table exists
ALTER TABLE Problems ADD CONSTRAINT fk_problems_knowledge 
    FOREIGN KEY (KnowledgeArticleId) REFERENCES KnowledgeArticles(ArticleId);

-- ====================================
-- Phase 3.2: Service Catalog
-- ====================================

CREATE TABLE IF NOT EXISTS CatalogCategories (
    CategoryId INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    IconName VARCHAR(50),
    DisplayOrder INT DEFAULT 0,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_catalog_categories_active (IsActive),
    INDEX idx_catalog_categories_order (DisplayOrder)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS CatalogItems (
    CatalogItemId INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(200) NOT NULL,
    ShortDescription VARCHAR(500),
    LongDescription TEXT,
    CategoryId INT NOT NULL,
    
    -- Display
    IconName VARCHAR(50),
    ImageUrl VARCHAR(500),
    DisplayOrder INT DEFAULT 0,
    IsFeatured BOOLEAN DEFAULT FALSE,
    
    -- Availability
    IsActive BOOLEAN DEFAULT TRUE,
    AvailableToAll BOOLEAN DEFAULT TRUE,
    RestrictedToGroups TEXT,
    
    -- Workflow
    WorkflowDefinitionId INT,
    ApprovalWorkflowId INT,
    FulfillmentTaskTemplateId INT,
    
    -- SLA
    ExpectedDeliveryDays INT,
    Priority INT DEFAULT 2,
    
    -- Pricing
    Price DECIMAL(18,2),
    RecurringCostMonthly DECIMAL(18,2),
    RequiresBudgetApproval BOOLEAN DEFAULT FALSE,
    
    -- Metrics
    RequestCount INT DEFAULT 0,
    AverageRating DECIMAL(3,2),
    
    -- Audit
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_catalog_items_category (CategoryId),
    INDEX idx_catalog_items_active (IsActive),
    INDEX idx_catalog_items_featured (IsFeatured),
    INDEX idx_catalog_items_order (DisplayOrder),
    
    FOREIGN KEY (CategoryId) REFERENCES CatalogCategories(CategoryId),
    FOREIGN KEY (WorkflowDefinitionId) REFERENCES WorkflowDefinitions(WorkflowDefinitionId),
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS CatalogVariables (
    VariableId INT PRIMARY KEY AUTO_INCREMENT,
    CatalogItemId INT NOT NULL,
    VariableName VARCHAR(100) NOT NULL,
    VariableLabel VARCHAR(200) NOT NULL,
    VariableType INT NOT NULL,
    
    -- Validation
    IsRequired BOOLEAN DEFAULT FALSE,
    ValidationRegex VARCHAR(500),
    ValidationMessage VARCHAR(500),
    MinLength INT,
    MaxLength INT,
    MinValue DECIMAL(18,2),
    MaxValue DECIMAL(18,2),
    
    -- Options
    Options TEXT,
    DefaultValue VARCHAR(500),
    
    -- Conditional display
    ShowWhen TEXT,
    
    DisplayOrder INT DEFAULT 0,
    HelpText VARCHAR(500),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_catalog_variables_item (CatalogItemId),
    INDEX idx_catalog_variables_order (CatalogItemId, DisplayOrder),
    
    FOREIGN KEY (CatalogItemId) REFERENCES CatalogItems(CatalogItemId) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS CatalogRequests (
    RequestId INT PRIMARY KEY AUTO_INCREMENT,
    CatalogItemId INT NOT NULL,
    RequestedForId INT NOT NULL,
    RequestedById INT NOT NULL,
    VariableValues TEXT,
    
    ApprovalStatus INT NOT NULL DEFAULT 1,
    State INT NOT NULL DEFAULT 1,
    
    ServiceRequestId INT,
    WorkflowInstanceId INT,
    
    -- Fulfillment
    AssignedToId INT,
    CompletedAt DATETIME,
    CompletionNotes TEXT,
    
    -- Audit
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_catalog_requests_item (CatalogItemId),
    INDEX idx_catalog_requests_requested_for (RequestedForId),
    INDEX idx_catalog_requests_requested_by (RequestedById),
    INDEX idx_catalog_requests_state (State),
    INDEX idx_catalog_requests_created (CreatedAt),
    
    FOREIGN KEY (CatalogItemId) REFERENCES CatalogItems(CatalogItemId),
    FOREIGN KEY (RequestedForId) REFERENCES Users(UserId),
    FOREIGN KEY (RequestedById) REFERENCES Users(UserId),
    FOREIGN KEY (ServiceRequestId) REFERENCES ServiceRequests(ServiceRequestId),
    FOREIGN KEY (WorkflowInstanceId) REFERENCES WorkflowInstances(WorkflowInstanceId),
    FOREIGN KEY (AssignedToId) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS CatalogRequestApprovals (
    ApprovalId INT PRIMARY KEY AUTO_INCREMENT,
    CatalogRequestId INT NOT NULL,
    ApproverId INT NOT NULL,
    ApprovalStatus INT NOT NULL DEFAULT 1,
    ApprovalDate DATETIME,
    Comments TEXT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_catalog_request_approvals_request (CatalogRequestId),
    INDEX idx_catalog_request_approvals_approver (ApproverId),
    
    FOREIGN KEY (CatalogRequestId) REFERENCES CatalogRequests(RequestId) ON DELETE CASCADE,
    FOREIGN KEY (ApproverId) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS CatalogRequestComments (
    CommentId INT PRIMARY KEY AUTO_INCREMENT,
    CatalogRequestId INT NOT NULL,
    Comment TEXT NOT NULL,
    IsInternal BOOLEAN DEFAULT FALSE,
    CreatedById INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_catalog_request_comments_request (CatalogRequestId),
    
    FOREIGN KEY (CatalogRequestId) REFERENCES CatalogRequests(RequestId) ON DELETE CASCADE,
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ====================================
-- Create Sequences for Auto-Number Generation
-- ====================================

CREATE TABLE IF NOT EXISTS ITSMNumberSequences (
    SequenceType VARCHAR(20) PRIMARY KEY,
    CurrentNumber INT NOT NULL DEFAULT 1,
    Prefix VARCHAR(10) NOT NULL,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO ITSMNumberSequences (SequenceType, CurrentNumber, Prefix) VALUES
('Incident', 1, 'INC'),
('Problem', 1, 'PRB'),
('Change', 1, 'CHG'),
('Knowledge', 1, 'KB'),
('CI', 1, 'CI'),
('Service', 1, 'SVC')
ON DUPLICATE KEY UPDATE SequenceType=SequenceType;

-- ====================================
-- Summary
-- ====================================
-- Total tables created: 38 new tables
-- - Incident Management: 4 tables
-- - Problem Management: 5 tables
-- - SLA Management: 3 tables
-- - CMDB: 4 tables
-- - Change Management: 8 tables
-- - Knowledge Management: 5 tables
-- - Service Catalog: 8 tables
-- - Supporting: 1 table (NumberSequences)



-- ─────────────────────────────────────────────────────────────────────────────
-- Source: 011_add_itsm_permission.sql
-- ─────────────────────────────────────────────────────────────────────────────


-- Migration: Add ITSM Permission to UserGroups
-- Description: Adds CanAccessITSM column to UserGroups table for role-based ITSM access control
-- Date: 2026-02-03

-- Add CanAccessITSM column to UserGroups table


ALTER TABLE UserGroups ADD COLUMN IF NOT EXISTS CanAccessITSM BOOLEAN NOT NULL DEFAULT FALSE;

-- Grant ITSM access to system admin groups (IsSystemAdmin = true)
UPDATE UserGroups SET CanAccessITSM = TRUE WHERE IsSystemAdmin = TRUE;

-- Verify the column was added



-- ─────────────────────────────────────────────────────────────────────────────
-- Source: 025_create_crmtasks_opportunities.sql
-- ─────────────────────────────────────────────────────────────────────────────


-- ============================================================================
-- Migration 025: Create CrmTasks and Opportunities tables
-- Date: 2026-02-17
-- Description: Creates the CrmTasks table (task queue) and Opportunities table
--              that were missing from the database schema. Also adds shadow FK
--              columns (ProductId, SubscriptionId, MarketingCampaignId) that
--              EF Core generates from navigation properties on related entities.
-- ============================================================================

-- CrmTasks Table (matches CRM.Core.Entities.CrmTask : BaseEntity)


CREATE TABLE IF NOT EXISTS `CrmTasks` (
    `Id`                INT NOT NULL AUTO_INCREMENT,
    `Subject`           VARCHAR(500) NOT NULL,
    `Description`       TEXT NULL,
    `TaskType`          INT NOT NULL DEFAULT 0       COMMENT 'CrmTaskType enum: 0=Call,1=Email,2=Meeting,3=FollowUp,4=Demo,5=Proposal,6=Contract,7=Research,8=Other',
    `Status`            INT NOT NULL DEFAULT 0       COMMENT 'CrmTaskStatus enum: 0=NotStarted,1=InProgress,2=Completed,3=Deferred,4=Waiting,5=Cancelled',
    `Priority`          INT NOT NULL DEFAULT 0       COMMENT 'CrmTaskPriority enum: 0=Low,1=Normal,2=High,3=Urgent',
    `DueDate`           DATETIME NULL,
    `StartDate`         DATETIME NULL,
    `CompletedDate`     DATETIME NULL,
    `ReminderDate`      DATETIME NULL,
    `HasReminder`       TINYINT(1) NOT NULL DEFAULT 0,
    `PercentComplete`   INT NOT NULL DEFAULT 0,
    `EstimatedMinutes`  INT NULL,
    `ActualMinutes`     INT NULL,
    `IsRecurring`       TINYINT(1) NOT NULL DEFAULT 0,
    `RecurrencePattern` VARCHAR(500) NULL,
    `RecurrenceEndDate` DATETIME NULL,
    `ParentTaskId`      INT NULL,
    `AccountId`         INT NULL               COMMENT 'FK to Customers table (Account entity)',
    `ContactId`         INT NULL,
    `OpportunityId`     INT NULL,
    `CampaignId`        INT NULL               COMMENT 'FK to MarketingCampaigns',
    `AssignedToUserId`  INT NULL,
    `AssignedToGroupId` INT NULL               COMMENT 'FK to UserGroups for workflow queue',
    `CreatedByUserId`   INT NULL,
    `Tags`              VARCHAR(500) NULL,
    `Category`          VARCHAR(100) NULL,
    `Attachments`       VARCHAR(5000) NULL,
    `CustomFields`      TEXT NULL,
    `IsDeleted`         TINYINT(1) NOT NULL DEFAULT 0,
    `RowVersion`        TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `CreatedAt`         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt`         DATETIME NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_CrmTasks_DueDate` (`DueDate`),
    INDEX `IX_CrmTasks_Status` (`Status`),
    INDEX `IX_CrmTasks_AssignedToUserId` (`AssignedToUserId`),
    INDEX `IX_CrmTasks_AssignedToGroupId` (`AssignedToGroupId`),
    INDEX `IX_CrmTasks_AccountId` (`AccountId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Opportunities Table (matches CRM.Core.Entities.Opportunity : BaseEntity)
CREATE TABLE IF NOT EXISTS `Opportunities` (
    `Id`                    INT NOT NULL AUTO_INCREMENT,
    `CreatedAt`             DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt`             DATETIME(6) NULL DEFAULT NULL,
    `IsDeleted`             TINYINT(1) NOT NULL DEFAULT 0,
    `RowVersion`            TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `Name`                  VARCHAR(255) NOT NULL,
    `Stage`                 INT NOT NULL DEFAULT 0       COMMENT 'OpportunityStage enum: 0=Discovery,1=Qualification,2=Proposal,3=Negotiation,4=ClosedWon,5=ClosedLost',
    `Probability`           INT NOT NULL DEFAULT 10      COMMENT 'Win probability 0-100%',
    `Amount`                DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `Currency`              VARCHAR(3) NOT NULL DEFAULT 'USD',
    `ExpectedCloseDate`     DATETIME(6) NULL DEFAULT NULL,
    `PricingModel`          INT NOT NULL DEFAULT 0       COMMENT 'OpportunityPricingModel enum: 0=Subscription,1=OneTime,2=UsageBased,3=Hybrid',
    `TermLengthMonths`      INT NOT NULL DEFAULT 12,
    `SolutionNotes`         VARCHAR(4000) NULL DEFAULT NULL,
    `QualificationReason`   INT NULL DEFAULT NULL        COMMENT 'QualificationReason enum: 0=Budget,1=Need,2=Timing,3=Authority,4=Fit',
    `QualificationNotes`    VARCHAR(4000) NULL DEFAULT NULL,
    `Region`                VARCHAR(100) NULL DEFAULT NULL,
    `AccountId`             INT NOT NULL                 COMMENT 'FK to Customers table (Account entity)',
    `PrimaryContactId`      INT NULL DEFAULT NULL,
    `SalesOwnerId`          INT NULL DEFAULT NULL,
    `LeadId`                INT NULL DEFAULT NULL,
    `MarketingCampaignId`   INT NULL DEFAULT NULL        COMMENT 'Shadow FK from MarketingCampaign.Opportunities collection',
    `ProductId`             INT NULL DEFAULT NULL        COMMENT 'Shadow FK from Product.Opportunities collection',
    `SubscriptionId`        INT NULL DEFAULT NULL        COMMENT 'Shadow FK from Subscription.Opportunities collection',
    PRIMARY KEY (`Id`),
    KEY `IX_Opportunities_Stage` (`Stage`),
    KEY `IX_Opportunities_ExpectedCloseDate` (`ExpectedCloseDate`),
    KEY `IX_Opportunities_AccountId` (`AccountId`),
    KEY `IX_Opportunities_SalesOwnerId` (`SalesOwnerId`),
    KEY `IX_Opportunities_LeadId` (`LeadId`),
    KEY `IX_Opportunities_MarketingCampaignId` (`MarketingCampaignId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Add FK constraints (optional, may fail if referenced tables don't exist)
-- CrmTasks FKs
ALTER TABLE `CrmTasks` ADD CONSTRAINT `FK_CrmTasks_Parent`
    FOREIGN KEY (`ParentTaskId`) REFERENCES `CrmTasks`(`Id`);
ALTER TABLE `CrmTasks` ADD CONSTRAINT `FK_CrmTasks_Account`
    FOREIGN KEY (`AccountId`) REFERENCES `Customers`(`Id`) ON DELETE SET NULL;
ALTER TABLE `CrmTasks` ADD CONSTRAINT `FK_CrmTasks_AssignedTo`
    FOREIGN KEY (`AssignedToUserId`) REFERENCES `Users`(`Id`) ON DELETE SET NULL;
ALTER TABLE `CrmTasks` ADD CONSTRAINT `FK_CrmTasks_AssignedToGroup`
    FOREIGN KEY (`AssignedToGroupId`) REFERENCES `UserGroups`(`Id`) ON DELETE SET NULL;
ALTER TABLE `CrmTasks` ADD CONSTRAINT `FK_CrmTasks_CreatedBy`
    FOREIGN KEY (`CreatedByUserId`) REFERENCES `Users`(`Id`);

-- Opportunities FKs
ALTER TABLE `Opportunities` ADD CONSTRAINT `FK_Opportunities_Customers_AccountId`
    FOREIGN KEY (`AccountId`) REFERENCES `Customers`(`Id`) ON DELETE CASCADE;
ALTER TABLE `Opportunities` ADD CONSTRAINT `FK_Opportunities_Contacts_PrimaryContactId`
    FOREIGN KEY (`PrimaryContactId`) REFERENCES `Contacts`(`Id`) ON DELETE SET NULL;
ALTER TABLE `Opportunities` ADD CONSTRAINT `FK_Opportunities_Users_SalesOwnerId`
    FOREIGN KEY (`SalesOwnerId`) REFERENCES `Users`(`Id`) ON DELETE SET NULL;
ALTER TABLE `Opportunities` ADD CONSTRAINT `FK_Opportunities_Leads_LeadId`
    FOREIGN KEY (`LeadId`) REFERENCES `Leads`(`Id`) ON DELETE SET NULL;



-- ╔══════════════════════════════════════════════════════════════════════════╗
-- ║  Phase 2 – Schema additions                                              ║
-- ╚══════════════════════════════════════════════════════════════════════════╝



-- ─────────────────────────────────────────────────────────────────────────────
-- Source: 100_customer_to_account_migration.sql
-- ─────────────────────────────────────────────────────────────────────────────


-- =============================================================================
-- CRM Solution: Customer to Account Migration Script
-- =============================================================================
-- Purpose: Migrate all Customer references to Account for consistency
-- Author: CRM Development Team
-- Date: 2026-01-31
-- Version: 1.0
-- =============================================================================
-- IMPORTANT: Run this script in a transaction and test in staging first!
-- =============================================================================

-- Start transaction


START TRANSACTION;

-- =============================================================================
-- PHASE 1: Rename main Customers table to Accounts
-- =============================================================================

-- Check if Accounts table already exists (from microservices hybrid schema)
-- If both exist, we need to merge or decide which is canonical

-- First, let's see what we have:
-- SELECT COUNT(*) as customer_count FROM Customers;
-- SELECT COUNT(*) as account_count FROM Accounts; -- May not exist

-- Option A: If Customers is the canonical table, rename it
-- RENAME TABLE Customers TO Accounts;

-- For now, we'll ADD AccountId columns and migrate data, keeping both temporarily

-- =============================================================================
-- PHASE 2: Add AccountId columns where missing (parallel to CustomerId)
-- =============================================================================

-- Opportunities table (already has AccountId, but may have CustomerId too)
-- ALTER TABLE Opportunities ADD COLUMN IF NOT EXISTS AccountId INT NULL;
-- UPDATE Opportunities SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- Quotes table
ALTER TABLE Quotes ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE Quotes SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- ServiceRequests table  
ALTER TABLE ServiceRequests ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE ServiceRequests SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- Notes table
ALTER TABLE Notes ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE Notes SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- Activities table
ALTER TABLE Activities ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE Activities SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- CrmTasks table
ALTER TABLE CrmTasks ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE CrmTasks SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- Interactions table
ALTER TABLE Interactions ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE Interactions SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- Conversations table
ALTER TABLE Conversations ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE Conversations SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- CommunicationMessages table
ALTER TABLE CommunicationMessages ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE CommunicationMessages SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- CampaignConversions table
ALTER TABLE CampaignConversions ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE CampaignConversions SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- CampaignRecipients table
ALTER TABLE CampaignRecipients ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE CampaignRecipients SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- =============================================================================
-- PHASE 3: Rename junction/relationship tables
-- =============================================================================

-- CustomerContacts -> AccountContacts
-- Check if table exists first
-- RENAME TABLE CustomerContacts TO AccountContacts;
-- ALTER TABLE AccountContacts CHANGE COLUMN CustomerId AccountId INT NOT NULL;
-- ALTER TABLE AccountContacts CHANGE COLUMN DepartmentAtCustomer DepartmentAtAccount VARCHAR(255);
-- ALTER TABLE AccountContacts CHANGE COLUMN PositionAtCustomer PositionAtAccount VARCHAR(255);

-- CustomerTerritoryAssignments -> AccountTerritoryAssignments
-- RENAME TABLE CustomerTerritoryAssignments TO AccountTerritoryAssignments;
-- ALTER TABLE AccountTerritoryAssignments CHANGE COLUMN CustomerId AccountId INT NOT NULL;

-- =============================================================================
-- PHASE 4: Update Contacts table
-- =============================================================================

-- Contacts already has AccountId, ensure it's populated
UPDATE Contacts SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- =============================================================================
-- PHASE 5: Update Leads table
-- =============================================================================

-- Leads already has AccountId, ensure it's populated  
UPDATE Leads SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- =============================================================================
-- PHASE 6: Create AccountId indexes (for performance)
-- =============================================================================

-- Add indexes on new AccountId columns
CREATE INDEX IF NOT EXISTS IX_Quotes_AccountId ON Quotes(AccountId);
CREATE INDEX IF NOT EXISTS IX_ServiceRequests_AccountId ON ServiceRequests(AccountId);
CREATE INDEX IF NOT EXISTS IX_Notes_AccountId ON Notes(AccountId);
CREATE INDEX IF NOT EXISTS IX_Activities_AccountId ON Activities(AccountId);
CREATE INDEX IF NOT EXISTS IX_CrmTasks_AccountId ON CrmTasks(AccountId);

-- =============================================================================
-- PHASE 7: Rename Customers table columns that should stay
-- =============================================================================

-- These columns are about the Customer entity itself, so they become Account entity:
-- CustomerType -> AccountType (but this is already the category like Individual/Organization)
-- CustomerHealthScore -> AccountHealthScore
-- ParentCustomerId -> ParentAccountId
-- ReferredByCustomerId -> ReferredByAccountId

-- We'll handle these in the entity rename phase

-- =============================================================================
-- PHASE 8: Update UserGroups permission column names (cosmetic, low priority)
-- =============================================================================

-- These are permission flags, naming can stay as-is for backward compatibility
-- CanAccessCustomers, CanCreateCustomers, CanDeleteCustomers
-- Could rename to CanAccessAccounts, etc. but not critical

-- =============================================================================
-- PHASE 9: Clean up duplicate/orphan columns
-- =============================================================================

-- Remove duplicate AccountId columns in Opportunities
-- ALTER TABLE Opportunities DROP COLUMN IF EXISTS AccountId1;

-- =============================================================================
-- VERIFICATION QUERIES
-- =============================================================================

-- Verify all CustomerId values were copied to AccountId
SELECT 'Quotes' as TableName, 
       COUNT(*) as TotalRows,
       SUM(CASE WHEN AccountId IS NOT NULL THEN 1 ELSE 0 END) as WithAccountId,
       SUM(CASE WHEN CustomerId IS NOT NULL AND AccountId IS NULL THEN 1 ELSE 0 END) as MissingAccountId
FROM Quotes
UNION ALL
SELECT 'ServiceRequests', COUNT(*), 
       SUM(CASE WHEN AccountId IS NOT NULL THEN 1 ELSE 0 END),
       SUM(CASE WHEN CustomerId IS NOT NULL AND AccountId IS NULL THEN 1 ELSE 0 END)
FROM ServiceRequests
UNION ALL
SELECT 'Notes', COUNT(*),
       SUM(CASE WHEN AccountId IS NOT NULL THEN 1 ELSE 0 END),
       SUM(CASE WHEN CustomerId IS NOT NULL AND AccountId IS NULL THEN 1 ELSE 0 END)
FROM Notes
UNION ALL
SELECT 'Activities', COUNT(*),
       SUM(CASE WHEN AccountId IS NOT NULL THEN 1 ELSE 0 END),
       SUM(CASE WHEN CustomerId IS NOT NULL AND AccountId IS NULL THEN 1 ELSE 0 END)
FROM Activities;

-- =============================================================================
-- COMMIT OR ROLLBACK
-- =============================================================================

-- If all looks good:
COMMIT;

-- If there are issues:
-- ROLLBACK;

-- =============================================================================
-- POST-MIGRATION: Drop old CustomerId columns (RUN AFTER CODE CHANGES DEPLOYED)
-- =============================================================================

-- DANGER: Only run this after all code has been updated to use AccountId
-- 
-- ALTER TABLE Quotes DROP COLUMN CustomerId;
-- ALTER TABLE ServiceRequests DROP COLUMN CustomerId;
-- ALTER TABLE Notes DROP COLUMN CustomerId;
-- ALTER TABLE Activities DROP COLUMN CustomerId;
-- ALTER TABLE CrmTasks DROP COLUMN CustomerId;
-- ALTER TABLE Contacts DROP COLUMN CustomerId;
-- ALTER TABLE Leads DROP COLUMN CustomerId;
-- ALTER TABLE Interactions DROP COLUMN CustomerId;
-- ALTER TABLE Conversations DROP COLUMN CustomerId;
-- ALTER TABLE CommunicationMessages DROP COLUMN CustomerId;
-- ALTER TABLE CampaignConversions DROP COLUMN CustomerId;
-- ALTER TABLE CampaignRecipients DROP COLUMN CustomerId;
-- ALTER TABLE Opportunities DROP COLUMN CustomerId;

-- =============================================================================
-- END OF MIGRATION SCRIPT
-- =============================================================================



-- ─────────────────────────────────────────────────────────────────────────────
-- Source: 20250713_add_duplicate_merge_tracking.sql
-- ─────────────────────────────────────────────────────────────────────────────


-- CRM Solution - Customer Relationship Management System
-- Copyright (C) 2024-2026 Abhishek Lal
-- Migration: Add Duplicate Merge Groups and Entity Merge Tracking

-- =====================================================
-- DUPLICATE MERGE GROUPS TABLE
-- Tracks groups of merged records
-- =====================================================



CREATE TABLE IF NOT EXISTS DuplicateMergeGroups (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    EntityType VARCHAR(50) NOT NULL COMMENT 'Lead, Contact, or Account',
    MasterRecordId INT NOT NULL COMMENT 'The surviving record ID',
    GroupIdentifier VARCHAR(50) NOT NULL COMMENT 'Unique identifier for the merge group',
    Status VARCHAR(20) NOT NULL DEFAULT 'Active' COMMENT 'Active, Unmerged, PartialUnmerge',
    MergedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    MergedById INT NULL COMMENT 'User who performed the merge',
    UnmergedAt DATETIME NULL,
    UnmergedById INT NULL COMMENT 'User who performed the unmerge',
    Notes TEXT NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX IX_DuplicateMergeGroups_EntityType (EntityType),
    INDEX IX_DuplicateMergeGroups_MasterRecordId (MasterRecordId),
    INDEX IX_DuplicateMergeGroups_GroupIdentifier (GroupIdentifier),
    INDEX IX_DuplicateMergeGroups_Status (Status),
    
    CONSTRAINT FK_DuplicateMergeGroups_MergedBy FOREIGN KEY (MergedById) 
        REFERENCES Users(Id) ON DELETE SET NULL,
    CONSTRAINT FK_DuplicateMergeGroups_UnmergedBy FOREIGN KEY (UnmergedById) 
        REFERENCES Users(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- DUPLICATE MERGE GROUP MEMBERS TABLE
-- Tracks individual records in a merge group
-- =====================================================

CREATE TABLE IF NOT EXISTS DuplicateMergeGroupMembers (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    MergeGroupId INT NOT NULL,
    RecordId INT NOT NULL COMMENT 'The ID of the merged record',
    RecordType VARCHAR(50) NOT NULL COMMENT 'Lead, Contact, or Account',
    IsMaster TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Is this the master record',
    RecordSnapshot LONGTEXT NULL COMMENT 'JSON snapshot of record before merge',
    FieldValuesUsed TEXT NULL COMMENT 'JSON of field values used from this record',
    RelinkedRecords TEXT NULL COMMENT 'JSON of related records relinked',
    Status VARCHAR(20) NOT NULL DEFAULT 'Merged' COMMENT 'Merged, Unmerged',
    MergedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UnmergedAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX IX_DuplicateMergeGroupMembers_MergeGroupId (MergeGroupId),
    INDEX IX_DuplicateMergeGroupMembers_RecordId (RecordId),
    INDEX IX_DuplicateMergeGroupMembers_Status (Status),
    
    CONSTRAINT FK_DuplicateMergeGroupMembers_MergeGroup FOREIGN KEY (MergeGroupId) 
        REFERENCES DuplicateMergeGroups(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- ADD MERGE TRACKING COLUMNS TO LEADS
-- =====================================================

ALTER TABLE Leads
    ADD COLUMN IF NOT EXISTS MergedIntoId INT NULL COMMENT 'ID of record this was merged into',
    ADD COLUMN IF NOT EXISTS MergeGroupId INT NULL COMMENT 'ID of the merge group',
    ADD COLUMN IF NOT EXISTS IsMergedDuplicate TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Was this merged as a duplicate',
    ADD COLUMN IF NOT EXISTS MergedAt DATETIME NULL COMMENT 'When this was merged';

ALTER TABLE Leads
    ADD INDEX IF NOT EXISTS IX_Leads_MergedIntoId (MergedIntoId),
    ADD INDEX IF NOT EXISTS IX_Leads_IsMergedDuplicate (IsMergedDuplicate);

-- =====================================================
-- ADD MERGE TRACKING COLUMNS TO CONTACTS
-- =====================================================

ALTER TABLE Contacts
    ADD COLUMN IF NOT EXISTS MergedIntoId INT NULL COMMENT 'ID of record this was merged into',
    ADD COLUMN IF NOT EXISTS MergeGroupId INT NULL COMMENT 'ID of the merge group',
    ADD COLUMN IF NOT EXISTS IsMergedDuplicate TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Was this merged as a duplicate',
    ADD COLUMN IF NOT EXISTS MergedAt DATETIME NULL COMMENT 'When this was merged';

ALTER TABLE Contacts
    ADD INDEX IF NOT EXISTS IX_Contacts_MergedIntoId (MergedIntoId),
    ADD INDEX IF NOT EXISTS IX_Contacts_IsMergedDuplicate (IsMergedDuplicate);

-- =====================================================
-- ADD MERGE TRACKING COLUMNS TO ACCOUNTS
-- =====================================================

ALTER TABLE Accounts
    ADD COLUMN IF NOT EXISTS MergedIntoId INT NULL COMMENT 'ID of record this was merged into',
    ADD COLUMN IF NOT EXISTS MergeGroupId INT NULL COMMENT 'ID of the merge group',
    ADD COLUMN IF NOT EXISTS IsMergedDuplicate TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Was this merged as a duplicate',
    ADD COLUMN IF NOT EXISTS MergedAt DATETIME NULL COMMENT 'When this was merged';

ALTER TABLE Accounts
    ADD INDEX IF NOT EXISTS IX_Accounts_MergedIntoId (MergedIntoId),
    ADD INDEX IF NOT EXISTS IX_Accounts_IsMergedDuplicate (IsMergedDuplicate);

-- =====================================================
-- SEED DEFAULT DUPLICATE DETECTION RULES (if not exists)
-- =====================================================

-- Lead duplicate detection rule
INSERT INTO DuplicateRules (Name, EntityType, Description, IsActive, MatchThreshold, Priority, CreatedAt)
SELECT 'Lead Duplicate Detection', 'Lead', 'Detects duplicate leads by email, name, and company', 1, 70, 1, NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM DuplicateRules WHERE EntityType = 'Lead' AND Name = 'Lead Duplicate Detection'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Get the Lead rule ID for adding match fields
SET @LeadRuleId = (SELECT Id FROM DuplicateRules WHERE EntityType = 'Lead' AND Name = 'Lead Duplicate Detection' LIMIT 1);

-- Lead match fields (if rule was just created)
INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @LeadRuleId, 'Email', 'Exact', 100, 'Lowercase,Trim', 0, NOW()
WHERE @LeadRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @LeadRuleId AND FieldName = 'Email'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @LeadRuleId, 'FirstName', 'Fuzzy', 40, 'Lowercase,Trim', 0, NOW()
WHERE @LeadRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @LeadRuleId AND FieldName = 'FirstName'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @LeadRuleId, 'LastName', 'Fuzzy', 50, 'Lowercase,Trim', 0, NOW()
WHERE @LeadRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @LeadRuleId AND FieldName = 'LastName'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @LeadRuleId, 'CompanyName', 'Fuzzy', 30, 'Lowercase,Trim,RemoveCompanySuffixes', 0, NOW()
WHERE @LeadRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @LeadRuleId AND FieldName = 'CompanyName'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @LeadRuleId, 'Phone', 'Normalized', 60, 'RemoveNonNumeric', 0, NOW()
WHERE @LeadRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @LeadRuleId AND FieldName = 'Phone'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Contact duplicate detection rule
INSERT INTO DuplicateRules (Name, EntityType, Description, IsActive, MatchThreshold, Priority, CreatedAt)
SELECT 'Contact Duplicate Detection', 'Contact', 'Detects duplicate contacts by email, name, and phone', 1, 70, 1, NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM DuplicateRules WHERE EntityType = 'Contact' AND Name = 'Contact Duplicate Detection'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

SET @ContactRuleId = (SELECT Id FROM DuplicateRules WHERE EntityType = 'Contact' AND Name = 'Contact Duplicate Detection' LIMIT 1);

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @ContactRuleId, 'EmailPrimary', 'Exact', 100, 'Lowercase,Trim', 0, NOW()
WHERE @ContactRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @ContactRuleId AND FieldName = 'EmailPrimary'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @ContactRuleId, 'FirstName', 'Fuzzy', 40, 'Lowercase,Trim', 0, NOW()
WHERE @ContactRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @ContactRuleId AND FieldName = 'FirstName'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @ContactRuleId, 'LastName', 'Fuzzy', 50, 'Lowercase,Trim', 0, NOW()
WHERE @ContactRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @ContactRuleId AND FieldName = 'LastName'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @ContactRuleId, 'PhonePrimary', 'Normalized', 60, 'RemoveNonNumeric', 0, NOW()
WHERE @ContactRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @ContactRuleId AND FieldName = 'PhonePrimary'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Account duplicate detection rule
INSERT INTO DuplicateRules (Name, EntityType, Description, IsActive, MatchThreshold, Priority, CreatedAt)
SELECT 'Account Duplicate Detection', 'Account', 'Detects duplicate accounts by company, email, and phone', 1, 70, 1, NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM DuplicateRules WHERE EntityType = 'Account' AND Name = 'Account Duplicate Detection'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

SET @AccountRuleId = (SELECT Id FROM DuplicateRules WHERE EntityType = 'Account' AND Name = 'Account Duplicate Detection' LIMIT 1);

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @AccountRuleId, 'Email', 'EmailDomain', 80, 'Lowercase,Trim', 0, NOW()
WHERE @AccountRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @AccountRuleId AND FieldName = 'Email'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @AccountRuleId, 'Company', 'Fuzzy', 70, 'Lowercase,Trim,RemoveCompanySuffixes', 0, NOW()
WHERE @AccountRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @AccountRuleId AND FieldName = 'Company'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @AccountRuleId, 'Phone', 'Normalized', 50, 'RemoveNonNumeric', 0, NOW()
WHERE @AccountRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @AccountRuleId AND FieldName = 'Phone'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @AccountRuleId, 'Website', 'Normalized', 60, 'Lowercase,RemoveProtocol,RemoveWWW', 0, NOW()
WHERE @AccountRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @AccountRuleId AND FieldName = 'Website'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Show created tables
SELECT 'DuplicateMergeGroups' as TableName, COUNT(*) as RowCount FROM DuplicateMergeGroups
UNION ALL
SELECT 'DuplicateMergeGroupMembers', COUNT(*) FROM DuplicateMergeGroupMembers
UNION ALL
SELECT 'DuplicateRules', COUNT(*) FROM DuplicateRules
UNION ALL
SELECT 'DuplicateMatchFields', COUNT(*) FROM DuplicateMatchFields;



-- ─────────────────────────────────────────────────────────────────────────────
-- Source: 20260214_add_branding_configs.sql
-- ─────────────────────────────────────────────────────────────────────────────


-- ============================================================================
-- Migration: Add BrandingConfigs table
-- Date: 2026-02-14
-- ============================================================================



CREATE TABLE IF NOT EXISTS `BrandingConfigs` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `SolutionName` varchar(100) NOT NULL DEFAULT 'CRM Solution',
  `CustomLogoPath` varchar(500) DEFAULT NULL,
  `CustomLogoFileName` varchar(255) DEFAULT NULL,
  `FaviconPath` varchar(500) DEFAULT NULL,
  `FaviconFileName` varchar(255) DEFAULT NULL,
  `SoftwareLogoPath` varchar(500) NOT NULL DEFAULT '/assets/logo.png',
  `IsCustomBrandingEnabled` tinyint(1) NOT NULL DEFAULT 1,
  `FaviconDataUrl` longtext DEFAULT NULL,
  `LastLogoUploadedAt` datetime(6) DEFAULT NULL,
  `LastLogoUploadedById` int(11) DEFAULT NULL,
  `LastFaviconUploadedAt` datetime(6) DEFAULT NULL,
  `LastFaviconUploadedById` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  `RowVersion` binary(8) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;



-- ─────────────────────────────────────────────────────────────────────────────
-- Source: 20260214_add_systemsettings_palette_fk.sql
-- ─────────────────────────────────────────────────────────────────────────────


-- Add SelectedPaletteId FK constraint to SystemSettings


ALTER TABLE `SystemSettings`
  ADD COLUMN `SelectedPaletteId` int(11) DEFAULT NULL AFTER `SecondaryColor`;

CREATE INDEX IF NOT EXISTS `IX_SystemSettings_SelectedPaletteId` ON `SystemSettings` (`SelectedPaletteId`);

ALTER TABLE `SystemSettings`
  ADD CONSTRAINT `FK_SystemSettings_SelectedPaletteId`
  FOREIGN KEY (`SelectedPaletteId`) REFERENCES `ColorPalettes` (`Id`) ON DELETE SET NULL;



-- ─────────────────────────────────────────────────────────────────────────────
-- Source: 20260216_add_worker_control_settings.sql
-- ─────────────────────────────────────────────────────────────────────────────


-- Add worker control settings to SystemSettings


ALTER TABLE SystemSettings
    ADD COLUMN WorkerControlState VARCHAR(50) NOT NULL DEFAULT 'Running',
    ADD COLUMN WorkerMaxInstances INT NOT NULL DEFAULT 1;



-- ─────────────────────────────────────────────────────────────────────────────
-- Source: 20260216_add_worker_architecture_tables.sql
-- ─────────────────────────────────────────────────────────────────────────────


-- Worker architecture tables



CREATE TABLE IF NOT EXISTS WorkerJobs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    JobType VARCHAR(100) NOT NULL,
    Payload LONGTEXT NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    RetryCount INT NOT NULL DEFAULT 0,
    MaxRetries INT NOT NULL DEFAULT 5,
    NextAttemptAt DATETIME NULL,
    CompletedAt DATETIME NULL,
    LastError TEXT NULL,
    CorrelationId VARCHAR(100) NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion BLOB NULL,
    INDEX IX_WorkerJobs_Status_NextAttemptAt (Status, NextAttemptAt),
    INDEX IX_WorkerJobs_JobType (JobType)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS OutboxEvents (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    EventType VARCHAR(100) NOT NULL,
    Payload LONGTEXT NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    OccurredAt DATETIME NOT NULL,
    ProcessedAt DATETIME NULL,
    CorrelationId VARCHAR(100) NULL,
    IdempotencyKey VARCHAR(100) NULL,
    RetryCount INT NOT NULL DEFAULT 0,
    MaxRetries INT NOT NULL DEFAULT 5,
    LastError TEXT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion BLOB NULL,
    INDEX IX_OutboxEvents_Status (Status),
    INDEX IX_OutboxEvents_OccurredAt (OccurredAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS WorkerExecutions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkerJobId INT NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    StartedAt DATETIME NOT NULL,
    FinishedAt DATETIME NULL,
    ErrorMessage TEXT NULL,
    NodeId VARCHAR(100) NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion BLOB NULL,
    INDEX IX_WorkerExecutions_WorkerJobId (WorkerJobId),
    CONSTRAINT FK_WorkerExecutions_WorkerJobs FOREIGN KEY (WorkerJobId) REFERENCES WorkerJobs(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;



-- ╔══════════════════════════════════════════════════════════════════════════╗
-- ║  Phase 3 – Configurable enum schema & seed data                          ║
-- ╚══════════════════════════════════════════════════════════════════════════╝



-- ─────────────────────────────────────────────────────────────────────────────
-- Source: 20260227_enum_schema_enhancements.sql
-- ─────────────────────────────────────────────────────────────────────────────


-- ============================================================================
-- Migration: SYS-008 - Enum Schema Enhancements
-- Date: 2026-02-27
-- Description: Enhance LookupCategories and LookupItems tables to support
--              configurable enums with entity mapping, validation, and transitions
-- Status: ✅ Applied to dev server (192.168.0.9:3306/crm_db) on 2026-02-27
-- ============================================================================

-- Step 1: Enhance LookupCategories table


ALTER TABLE LookupCategories
ADD COLUMN IF NOT EXISTS EntityType VARCHAR(100) NULL COMMENT 'Entity this category maps to (Lead, Opportunity, ServiceRequest)',
ADD COLUMN IF NOT EXISTS PropertyName VARCHAR(100) NULL COMMENT 'Property name on entity (Status, Stage, Priority)',
ADD COLUMN IF NOT EXISTS IsSystemManaged TINYINT(1) DEFAULT 0 COMMENT 'Managed by system vs user-customizable',
ADD COLUMN IF NOT EXISTS AllowCustomValues TINYINT(1) DEFAULT 1 COMMENT 'Allow users to add custom values',
ADD COLUMN IF NOT EXISTS ValidationSchema TEXT NULL COMMENT 'JSON schema for validation rules';

-- Step 2: Create indexes for performance
CREATE INDEX IF NOT EXISTS IX_LookupCategories_EntityType_PropertyName 
ON LookupCategories(EntityType, PropertyName);

-- Step 3: Enhance LookupItems table
ALTER TABLE LookupItems
ADD COLUMN IF NOT EXISTS IsDefault TINYINT(1) DEFAULT 0 COMMENT 'Default value for new records',
ADD COLUMN IF NOT EXISTS IsSystemValue TINYINT(1) DEFAULT 0 COMMENT 'System value (cannot be deleted)',
ADD COLUMN IF NOT EXISTS Color VARCHAR(7) NULL COMMENT 'Hex color code for UI display',
ADD COLUMN IF NOT EXISTS Icon VARCHAR(50) NULL COMMENT 'Icon identifier for UI',
ADD COLUMN IF NOT EXISTS ValidationRules TEXT NULL COMMENT 'JSON validation rules for this value';

-- Step 4: Create EnumTransitions table for state machine rules
CREATE TABLE IF NOT EXISTS EnumTransitions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CategoryId INT NOT NULL COMMENT 'FK to LookupCategories',
    FromValueId INT NULL COMMENT 'FK to LookupItems (NULL = any value)',
    ToValueId INT NOT NULL COMMENT 'FK to LookupItems',
    IsAllowed TINYINT(1) DEFAULT 1 COMMENT 'Is this transition allowed',
    RequiresApproval TINYINT(1) DEFAULT 0 COMMENT 'Requires approval workflow',
    AllowedRoles VARCHAR(500) NULL COMMENT 'Comma-separated role names',
    ValidationExpression TEXT NULL COMMENT 'Custom validation logic',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    CONSTRAINT FK_EnumTransitions_Category FOREIGN KEY (CategoryId) REFERENCES LookupCategories(Id) ON DELETE CASCADE,
    CONSTRAINT FK_EnumTransitions_FromValue FOREIGN KEY (FromValueId) REFERENCES LookupItems(Id) ON DELETE CASCADE,
    CONSTRAINT FK_EnumTransitions_ToValue FOREIGN KEY (ToValueId) REFERENCES LookupItems(Id) ON DELETE CASCADE,
    
    INDEX IX_EnumTransitions_Category (CategoryId),
    INDEX IX_EnumTransitions_FromTo (FromValueId, ToValueId),
    UNIQUE KEY UX_EnumTransitions (CategoryId, FromValueId, ToValueId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Step 5: Update existing categories with entity mappings
UPDATE LookupCategories SET EntityType = 'Lead', PropertyName = 'Status', IsSystemManaged = 1, AllowCustomValues = 1 WHERE Name = 'LeadStatus';
UPDATE LookupCategories SET EntityType = 'Opportunity', PropertyName = 'Stage', IsSystemManaged = 1, AllowCustomValues = 1 WHERE Name = 'OpportunityStage';
UPDATE LookupCategories SET EntityType = 'ServiceRequest', PropertyName = 'Status', IsSystemManaged = 1, AllowCustomValues = 1 WHERE Name = 'ServiceRequestStatus';
UPDATE LookupCategories SET EntityType = 'ServiceRequest', PropertyName = 'Priority', IsSystemManaged = 1, AllowCustomValues = 1 WHERE Name = 'ServiceRequestPriority';

-- Step 6: Mark system values as non-deletable
UPDATE LookupItems li
INNER JOIN LookupCategories lc ON li.LookupCategoryId = lc.Id
SET li.IsSystemValue = 1
WHERE lc.Name IN ('LeadStatus', 'OpportunityStage', 'ServiceRequestStatus', 'ServiceRequestPriority');

-- Step 7: Set default values
UPDATE LookupItems li
INNER JOIN LookupCategories lc ON li.LookupCategoryId = lc.Id
SET li.IsDefault = 1
WHERE (lc.Name = 'LeadStatus' AND li.`Key` = 'NEW')
   OR (lc.Name = 'OpportunityStage' AND li.`Key` = 'PROSP')
   OR (lc.Name = 'ServiceRequestStatus' AND li.`Key` = 'NEW')
   OR (lc.Name = 'ServiceRequestPriority' AND li.`Key` = 'MEDIUM');

-- Verification Queries
SELECT Name, EntityType, PropertyName, IsSystemManaged, AllowCustomValues
FROM LookupCategories
WHERE EntityType IS NOT NULL
ORDER BY EntityType, PropertyName;



-- ─────────────────────────────────────────────────────────────────────────────
-- Source: 20260227_servicerequest_categories.sql
-- ─────────────────────────────────────────────────────────────────────────────


-- ============================================================================
-- Migration: ServiceRequest Status/Priority Categories
-- Date: 2026-02-27
-- Description: Add ServiceRequestStatus and ServiceRequestPriority categories
--              to support FK migration (these were missing from seed data)
-- Status: ✅ Applied to dev server (192.168.0.9:3306/crm_db) on 2026-02-27
-- ============================================================================

-- ============================================================================
-- Create ServiceRequestStatus Category and Items
-- ============================================================================



INSERT INTO LookupCategories (Name, Description, IsActive, CreatedAt, IsDeleted, EntityType, PropertyName, IsSystemManaged, AllowCustomValues)
VALUES ('ServiceRequestStatus', 'Service Request lifecycle statuses', 1, NOW(), 0, 'ServiceRequest', 'Status', 1, 1)
ON DUPLICATE KEY UPDATE 
    Description = VALUES(Description),
    EntityType = VALUES(EntityType),
    PropertyName = VALUES(PropertyName);

-- Get the category ID
SET @srStatusCatId = (SELECT Id FROM LookupCategories WHERE Name = 'ServiceRequestStatus');

-- Create ServiceRequestStatus items
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted, IsDefault, IsSystemValue, Color) VALUES
(@srStatusCatId, 'NEW', 'New', '{"default":true}', 1, 1, NOW(), 0, 1, 1, '#9e9e9e'),
(@srStatusCatId, 'OPEN', 'Open', NULL, 2, 1, NOW(), 0, 0, 1, '#2196f3'),
(@srStatusCatId, 'IN_PROGRESS', 'In Progress', NULL, 3, 1, NOW(), 0, 0, 1, '#03a9f4'),
(@srStatusCatId, 'PENDING', 'Pending', NULL, 4, 1, NOW(), 0, 0, 1, '#ff9800'),
(@srStatusCatId, 'ON_HOLD', 'On Hold', NULL, 5, 1, NOW(), 0, 0, 1, '#ff5722'),
(@srStatusCatId, 'RESOLVED', 'Resolved', NULL, 6, 1, NOW(), 0, 0, 1, '#8bc34a'),
(@srStatusCatId, 'CLOSED', 'Closed', NULL, 7, 1, NOW(), 0, 0, 1, '#4caf50'),
(@srStatusCatId, 'CANCELLED', 'Cancelled', NULL, 8, 1, NOW(), 0, 0, 1, '#f44336')
ON DUPLICATE KEY UPDATE 
    Value = VALUES(Value),
    Color = VALUES(Color);

-- ============================================================================
-- Create ServiceRequestPriority Category and Items
-- ============================================================================

INSERT INTO LookupCategories (Name, Description, IsActive, CreatedAt, IsDeleted, EntityType, PropertyName, IsSystemManaged, AllowCustomValues)
VALUES ('ServiceRequestPriority', 'Service Request priority levels', 1, NOW(), 0, 'ServiceRequest', 'Priority', 1, 1)
ON DUPLICATE KEY UPDATE 
    Description = VALUES(Description),
    EntityType = VALUES(EntityType),
    PropertyName = VALUES(PropertyName);

-- Get the category ID
SET @srPriorityCatId = (SELECT Id FROM LookupCategories WHERE Name = 'ServiceRequestPriority');

-- Create ServiceRequestPriority items
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted, IsDefault, IsSystemValue, Color) VALUES
(@srPriorityCatId, 'LOW', 'Low', NULL, 1, 1, NOW(), 0, 0, 1, '#4caf50'),
(@srPriorityCatId, 'MEDIUM', 'Medium', '{"default":true,"slaHours":48}', 2, 1, NOW(), 0, 1, 1, '#ffeb3b'),
(@srPriorityCatId, 'HIGH', 'High', '{"slaHours":24}', 3, 1, NOW(), 0, 0, 1, '#ff9800'),
(@srPriorityCatId, 'CRITICAL', 'Critical', '{"slaHours":4}', 4, 1, NOW(), 0, 0, 1, '#f44336')
ON DUPLICATE KEY UPDATE 
    Value = VALUES(Value),
    Color = VALUES(Color),
    Meta = VALUES(Meta);

-- Verification
SELECT lc.Id, lc.Name, lc.EntityType, lc.PropertyName, COUNT(li.Id) as ItemCount
FROM LookupCategories lc
LEFT JOIN LookupItems li ON li.LookupCategoryId = lc.Id
WHERE lc.Name = 'ServiceRequestStatus'
GROUP BY lc.Id, lc.Name, lc.EntityType, lc.PropertyName;

SELECT lc.Id, lc.Name, lc.EntityType, lc.PropertyName, COUNT(li.Id) as ItemCount
FROM LookupCategories lc
LEFT JOIN LookupItems li ON li.LookupCategoryId = lc.Id
WHERE lc.Name = 'ServiceRequestPriority'
GROUP BY lc.Id, lc.Name, lc.EntityType, lc.PropertyName;

SELECT li.Id, li.`Key`, li.Value, li.Color, li.IsDefault, li.IsSystemValue
FROM LookupItems li
INNER JOIN LookupCategories lc ON li.LookupCategoryId = lc.Id
WHERE lc.Name = 'ServiceRequestStatus'
ORDER BY li.SortOrder;

SELECT li.Id, li.`Key`, li.Value, li.Color, li.IsDefault, li.IsSystemValue
FROM LookupItems li
INNER JOIN LookupCategories lc ON li.LookupCategoryId = lc.Id
WHERE lc.Name = 'ServiceRequestPriority'
ORDER BY li.SortOrder;



-- ╔══════════════════════════════════════════════════════════════════════════╗
-- ║  Phase 4 – FK backfill data migrations                                   ║
-- ╚══════════════════════════════════════════════════════════════════════════╝



-- ─────────────────────────────────────────────────────────────────────────────
-- Source: SYS-009-ServiceRequest-Fix.sql
-- ─────────────────────────────────────────────────────────────────────────────


-- =============================================================================
-- SYS-009: ServiceRequest Category & FK Migration Fix
-- =============================================================================
-- Purpose: Create missing ServiceRequestStatus and ServiceRequestPriority
--          LookupCategories and their items, then migrate FK values
-- Applied to: crm_db (192.168.0.9) - 2026-02-27
-- =============================================================================

-- =============================================================================
-- CREATE ServiceRequestStatus CATEGORY (if not exists)
-- =============================================================================


INSERT IGNORE INTO LookupCategories
    (Name, Description, EntityType, PropertyName, IsSystemManaged, AllowCustomValues, CreatedAt, UpdatedAt, IsDeleted)
VALUES
    ('ServiceRequestStatus', 'Status values for Service Requests', 'ServiceRequest', 'Status', 1, 0, NOW(), NOW(), 0);

-- COALESCE: if INSERT IGNORE matched a duplicate, LAST_INSERT_ID() returns 0;
-- fall back to a SELECT to retrieve the existing row's Id safely.
SET @status_cat_id = COALESCE(
    NULLIF(LAST_INSERT_ID(), 0),
    (SELECT Id FROM LookupCategories WHERE Name = 'ServiceRequestStatus' LIMIT 1)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert Status items
INSERT IGNORE INTO LookupItems
    (LookupCategoryId, `Key`, Value, Description, SortOrder, IsActive, IsDefault, Metadata, CreatedAt, UpdatedAt, IsDeleted)
VALUES
    (@status_cat_id, 'NEW',         'New',         'Newly created service request',                        1, 1, 1, NULL, NOW(), NOW(), 0),
    (@status_cat_id, 'OPEN',        'Open',        'Service request is open and being worked on',          2, 1, 0, NULL, NOW(), NOW(), 0),
    (@status_cat_id, 'IN_PROGRESS', 'In Progress', 'Service request is actively being worked on',          3, 1, 0, NULL, NOW(), NOW(), 0),
    (@status_cat_id, 'PENDING',     'Pending',     'Waiting for customer or third-party response',         4, 1, 0, NULL, NOW(), NOW(), 0),
    (@status_cat_id, 'ON_HOLD',     'On Hold',     'Service request is temporarily on hold',               5, 1, 0, NULL, NOW(), NOW(), 0),
    (@status_cat_id, 'RESOLVED',    'Resolved',    'Issue has been resolved, pending confirmation',        6, 1, 0, NULL, NOW(), NOW(), 0),
    (@status_cat_id, 'CLOSED',      'Closed',      'Service request is fully closed and verified',         7, 1, 0, NULL, NOW(), NOW(), 0),
    (@status_cat_id, 'CANCELLED',   'Cancelled',   'Service request was cancelled',                        8, 1, 0, NULL, NOW(), NOW(), 0);

-- =============================================================================
-- CREATE ServiceRequestPriority CATEGORY (if not exists)
-- =============================================================================
INSERT IGNORE INTO LookupCategories
    (Name, Description, EntityType, PropertyName, IsSystemManaged, AllowCustomValues, CreatedAt, UpdatedAt, IsDeleted)
VALUES
    ('ServiceRequestPriority', 'Priority levels for Service Requests', 'ServiceRequest', 'Priority', 1, 0, NOW(), NOW(), 0);

SET @priority_cat_id = COALESCE(
    NULLIF(LAST_INSERT_ID(), 0),
    (SELECT Id FROM LookupCategories WHERE Name = 'ServiceRequestPriority' LIMIT 1)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert Priority items (with SLA metadata)
INSERT IGNORE INTO LookupItems
    (LookupCategoryId, `Key`, Value, Description, SortOrder, IsActive, IsDefault, Metadata, CreatedAt, UpdatedAt, IsDeleted)
VALUES
    (@priority_cat_id, 'LOW',      'Low',      'Low priority - response within 5 business days',        1, 1, 0, '{"slaHours":120,"color":"#4CAF50"}', NOW(), NOW(), 0),
    (@priority_cat_id, 'MEDIUM',   'Medium',   'Medium priority - response within 2 business days',     2, 1, 1, '{"slaHours":48,"color":"#FF9800"}',  NOW(), NOW(), 0),
    (@priority_cat_id, 'HIGH',     'High',     'High priority - response within 4 hours',               3, 1, 0, '{"slaHours":4,"color":"#F44336"}',   NOW(), NOW(), 0),
    (@priority_cat_id, 'CRITICAL', 'Critical', 'Critical priority - immediate response required',        4, 1, 0, '{"slaHours":1,"color":"#9C27B0"}',   NOW(), NOW(), 0);

-- =============================================================================
-- MIGRATE ServiceRequests.StatusId
-- Maps ServiceRequest.Status (0=NEW, 1=OPEN, 2=IN_PROGRESS, 3=PENDING, 4=ON_HOLD, 5=RESOLVED, 6=CLOSED, 7=CANCELLED)
-- =============================================================================
-- Use @status_cat_id resolved above to avoid a per-row LookupCategories scan.
-- CASE in the JOIN ON clause means unmatched ordinals produce no join hit
-- (NULL = anything is UNKNOWN → row excluded), so no explicit WHERE guard needed.
START TRANSACTION;

UPDATE ServiceRequests sr
INNER JOIN LookupItems li
    ON  li.LookupCategoryId = @status_cat_id
    AND li.`Key` = CASE sr.`Status`
                       WHEN 0 THEN 'NEW'
                       WHEN 1 THEN 'OPEN'
                       WHEN 2 THEN 'IN_PROGRESS'
                       WHEN 3 THEN 'PENDING'
                       WHEN 4 THEN 'ON_HOLD'
                       WHEN 5 THEN 'RESOLVED'
                       WHEN 6 THEN 'CLOSED'
                       WHEN 7 THEN 'CANCELLED'
                       ELSE NULL
                   END
SET sr.StatusId = li.Id;

-- =============================================================================
-- MIGRATE ServiceRequests.PriorityId
-- Maps ServiceRequest.Priority (0=LOW, 1=MEDIUM, 2=HIGH, 3=CRITICAL)
-- =============================================================================
UPDATE ServiceRequests sr
INNER JOIN LookupItems li
    ON  li.LookupCategoryId = @priority_cat_id
    AND li.`Key` = CASE sr.`Priority`
                       WHEN 0 THEN 'LOW'
                       WHEN 1 THEN 'MEDIUM'
                       WHEN 2 THEN 'HIGH'
                       WHEN 3 THEN 'CRITICAL'
                       ELSE NULL
                   END
SET sr.PriorityId = li.Id;

COMMIT;

-- =============================================================================
-- POST-CHECK
-- =============================================================================
SELECT
    COUNT(CASE WHEN StatusId IS NULL THEN 1 END) AS StatusNulls,
    COUNT(CASE WHEN PriorityId IS NULL THEN 1 END) AS PriorityNulls,
    COUNT(*) AS Total
FROM ServiceRequests;

SELECT li.Value AS Status, COUNT(*) AS Count
FROM ServiceRequests sr
INNER JOIN LookupItems li ON sr.StatusId = li.Id
GROUP BY li.Value ORDER BY Count DESC;

SELECT li.Value AS Priority, COUNT(*) AS Count
FROM ServiceRequests sr
INNER JOIN LookupItems li ON sr.PriorityId = li.Id
GROUP BY li.Value ORDER BY Count DESC;



-- ─────────────────────────────────────────────────────────────────────────────
-- Source: 20260227_entity_fk_migration.sql
-- ─────────────────────────────────────────────────────────────────────────────


-- ============================================================================
-- Migration: SYS-009 - Entity FK Migration
-- Date: 2026-02-27
-- Description: Migrate Lead, Opportunity, ServiceRequest from enum integers
--              to foreign key references to LookupItems
-- Status: ✅ Applied to dev server (192.168.0.9:3306/crm_db) on 2026-02-27
--           ✅ Data migrated: 231 Leads, 230 Opportunities, 187 ServiceRequests
-- ============================================================================

-- ============================================================================
-- LEADS: Add StatusId FK column
-- ============================================================================


ALTER TABLE Leads
ADD COLUMN IF NOT EXISTS StatusId INT NULL COMMENT 'FK to LookupItems (LeadStatus category)';

-- Migrate existing data (enum int → FK)
-- Strategy: SortOrder in LookupItems starts at 1, but enum ints start at 0
-- So: Status 0 maps to SortOrder 1, Status 1 maps to SortOrder 2, etc.
UPDATE Leads l
INNER JOIN LookupCategories lc ON lc.Name = 'LeadStatus'
INNER JOIN LookupItems li ON li.LookupCategoryId = lc.Id
SET l.StatusId = li.Id
WHERE li.SortOrder = (l.Status + 1);

-- Add FK constraint
ALTER TABLE Leads
ADD CONSTRAINT IF NOT EXISTS FK_Leads_StatusValue 
FOREIGN KEY (StatusId) REFERENCES LookupItems(Id) ON DELETE RESTRICT;

-- Add index for performance
CREATE INDEX IF NOT EXISTS IX_Leads_StatusId ON Leads(StatusId);

-- ============================================================================
-- OPPORTUNITIES: Add StageId FK column
-- ============================================================================
ALTER TABLE Opportunities
ADD COLUMN IF NOT EXISTS StageId INT NULL COMMENT 'FK to LookupItems (OpportunityStage category)';

-- Migrate existing data
UPDATE Opportunities o
INNER JOIN LookupCategories lc ON lc.Name = 'OpportunityStage'
INNER JOIN LookupItems li ON li.LookupCategoryId = lc.Id
SET o.StageId = li.Id
WHERE li.SortOrder = (o.Stage + 1);

-- Add FK constraint
ALTER TABLE Opportunities
ADD CONSTRAINT IF NOT EXISTS FK_Opportunities_StageValue 
FOREIGN KEY (StageId) REFERENCES LookupItems(Id) ON DELETE RESTRICT;

-- Add index
CREATE INDEX IF NOT EXISTS IX_Opportunities_StageId ON Opportunities(StageId);

-- ============================================================================
-- SERVICEREQUESTS: Add StatusId and PriorityId FK columns
-- ============================================================================

-- Note: ServiceRequestStatus and ServiceRequestPriority categories must exist first
-- If they don't exist, run the creation script first (see SYS-009-ServiceRequest-Creation.sql)

ALTER TABLE ServiceRequests
ADD COLUMN IF NOT EXISTS StatusId INT NULL COMMENT 'FK to LookupItems (ServiceRequestStatus category)',
ADD COLUMN IF NOT EXISTS PriorityId INT NULL COMMENT 'FK to LookupItems (ServiceRequestPriority category)';

-- Migrate Status (using CASE mapping because Key names don't match enum exactly)
UPDATE ServiceRequests sr
INNER JOIN LookupCategories lc ON lc.Name = 'ServiceRequestStatus'
INNER JOIN LookupItems li ON li.LookupCategoryId = lc.Id
SET sr.StatusId = li.Id
WHERE li.`Key` = CASE sr.Status
    WHEN 0 THEN 'NEW'
    WHEN 1 THEN 'OPEN'
    WHEN 2 THEN 'IN_PROGRESS'
    WHEN 3 THEN 'PENDING'
    WHEN 4 THEN 'ON_HOLD'
    WHEN 5 THEN 'RESOLVED'
    WHEN 6 THEN 'CLOSED'
    WHEN 7 THEN 'CANCELLED'
    ELSE 'NEW'
END;

-- Migrate Priority
UPDATE ServiceRequests sr
INNER JOIN LookupCategories lc ON lc.Name = 'ServiceRequestPriority'
INNER JOIN LookupItems li ON li.LookupCategoryId = lc.Id
SET sr.PriorityId = li.Id
WHERE li.`Key` = CASE sr.Priority
    WHEN 0 THEN 'LOW'
    WHEN 1 THEN 'MEDIUM'
    WHEN 2 THEN 'HIGH'
    WHEN 3 THEN 'CRITICAL'
    ELSE 'MEDIUM'
END;

-- Add FK constraints
ALTER TABLE ServiceRequests
ADD CONSTRAINT IF NOT EXISTS FK_ServiceRequests_StatusValue 
FOREIGN KEY (StatusId) REFERENCES LookupItems(Id) ON DELETE RESTRICT,
ADD CONSTRAINT IF NOT EXISTS FK_ServiceRequests_PriorityValue 
FOREIGN KEY (PriorityId) REFERENCES LookupItems(Id) ON DELETE RESTRICT;

-- Add indexes
CREATE INDEX IF NOT EXISTS IX_ServiceRequests_StatusId ON ServiceRequests(StatusId);
CREATE INDEX IF NOT EXISTS IX_ServiceRequests_PriorityId ON ServiceRequests(PriorityId);

-- ============================================================================
-- VERIFICATION QUERIES
-- ============================================================================

-- Check for NULL FK values (should be 0 after migration)
SELECT 'Leads' AS Entity, COUNT(*) AS NullCount FROM Leads WHERE StatusId IS NULL
UNION ALL
SELECT 'Opportunities', COUNT(*) FROM Opportunities WHERE StageId IS NULL
UNION ALL
SELECT 'ServiceRequests (Status)', COUNT(*) FROM ServiceRequests WHERE StatusId IS NULL
UNION ALL
SELECT 'ServiceRequests (Priority)', COUNT(*) FROM ServiceRequests WHERE PriorityId IS NULL;

-- Check for invalid FK values
SELECT 'Leads' AS Entity, COUNT(*) AS InvalidCount 
FROM Leads l 
LEFT JOIN LookupItems li ON l.StatusId = li.Id 
WHERE l.StatusId IS NOT NULL AND li.Id IS NULL
UNION ALL
SELECT 'Opportunities', COUNT(*) 
FROM Opportunities o 
LEFT JOIN LookupItems li ON o.StageId = li.Id 
WHERE o.StageId IS NOT NULL AND li.Id IS NULL
UNION ALL
SELECT 'ServiceRequests (Status)', COUNT(*) 
FROM ServiceRequests sr 
LEFT JOIN LookupItems li ON sr.StatusId = li.Id 
WHERE sr.StatusId IS NOT NULL AND li.Id IS NULL
UNION ALL
SELECT 'ServiceRequests (Priority)', COUNT(*) 
FROM ServiceRequests sr 
LEFT JOIN LookupItems li ON sr.PriorityId = li.Id 
WHERE sr.PriorityId IS NOT NULL AND li.Id IS NULL;

-- Data distribution
SELECT li.`Key`, li.Value, COUNT(*) as Count
FROM Leads l
INNER JOIN LookupItems li ON l.StatusId = li.Id
GROUP BY li.`Key`, li.Value
ORDER BY COUNT(*) DESC;

SELECT li.`Key`, li.Value, COUNT(*) as Count
FROM Opportunities o
INNER JOIN LookupItems li ON o.StageId = li.Id
GROUP BY li.`Key`, li.Value
ORDER BY COUNT(*) DESC;

SELECT li.`Key`, li.Value, COUNT(*) as Count
FROM ServiceRequests sr
INNER JOIN LookupItems li ON sr.StatusId = li.Id
GROUP BY li.`Key`, li.Value
ORDER BY COUNT(*) DESC;

SELECT li.`Key`, li.Value, COUNT(*) as Count
FROM ServiceRequests sr
INNER JOIN LookupItems li ON sr.PriorityId = li.Id
GROUP BY li.`Key`, li.Value
ORDER BY COUNT(*) DESC;



-- ╔══════════════════════════════════════════════════════════════════════════╗
-- ║  Phase 5 – Feature tables                                                ║
-- ╚══════════════════════════════════════════════════════════════════════════╝



-- ─────────────────────────────────────────────────────────────────────────────
-- Source: SYS-010-RecordComments.sql
-- ─────────────────────────────────────────────────────────────────────────────


-- =============================================================================
-- SYS-010: Record Comments & @Mentions
-- Creates ThreadedComments table for entity-agnostic comment threads.
-- Supports @mentions via JSON user-ID array in MentionedUserIds column.
-- =============================================================================



CREATE TABLE IF NOT EXISTS RecordComments (
    Id                 INT            NOT NULL AUTO_INCREMENT,
    EntityType         VARCHAR(50)    NOT NULL,
    EntityId           INT            NOT NULL,
    Content            VARCHAR(4000)  NOT NULL,
    AuthorId           INT            NOT NULL,
    ParentCommentId    INT            NULL,
    MentionedUserIds   TEXT           NULL,
    CreatedAt          DATETIME(6)    NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt          DATETIME(6)    NULL     ON UPDATE CURRENT_TIMESTAMP(6),
    IsDeleted          TINYINT(1)     NOT NULL DEFAULT 0,
    RowVersion         TIMESTAMP(6)   NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    INDEX IX_RecordComments_Entity  (EntityType, EntityId),
    INDEX IX_RecordComments_Author  (AuthorId),
    INDEX IX_RecordComments_Parent  (ParentCommentId),
    CONSTRAINT FK_RecordComments_Author FOREIGN KEY (AuthorId)
        REFERENCES Users (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_RecordComments_Parent FOREIGN KEY (ParentCommentId)
        REFERENCES RecordComments (Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;



-- ─────────────────────────────────────────────────────────────────────────────
-- Source: SYS-011-SatisfactionTracking.sql
-- ─────────────────────────────────────────────────────────────────────────────


-- =============================================================================
-- SYS-011: Customer Satisfaction Tracking (CSAT / NPS / CES)
-- Creates SatisfactionSurveys and SatisfactionResponses tables.
-- SurveyType: 0=CSAT, 1=NPS, 2=CES
-- SurveyStatus: 0=Pending, 1=Sent, 2=Responded, 3=Expired, 4=Cancelled
-- SentimentType: 0=VeryPositive, 1=Positive, 2=Neutral, 3=Negative, 4=VeryNegative
-- =============================================================================



CREATE TABLE IF NOT EXISTS SatisfactionSurveys (
    Id                  INT            NOT NULL AUTO_INCREMENT,
    EntityType          VARCHAR(100)   NOT NULL,
    EntityId            INT            NOT NULL,
    Type                INT            NOT NULL DEFAULT 0,   -- SurveyType enum
    Status              INT            NOT NULL DEFAULT 0,   -- SurveyStatus enum
    ContactId           INT            NULL,
    AccountId           INT            NULL,
    SentAt              DATETIME(6)    NULL,
    ResponseReceivedAt  DATETIME(6)    NULL,
    ExpiresAt           DATETIME(6)    NULL,
    ExternalToken       VARCHAR(64)    NULL,
    Subject             VARCHAR(255)   NULL,
    CreatedAt           DATETIME(6)    NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt           DATETIME(6)    NULL     ON UPDATE CURRENT_TIMESTAMP(6),
    IsDeleted           TINYINT(1)     NOT NULL DEFAULT 0,
    RowVersion          TIMESTAMP(6)   NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE INDEX UX_SatisfactionSurveys_ExternalToken (ExternalToken),
    INDEX IX_SatisfactionSurveys_EntityType_EntityId  (EntityType, EntityId),
    INDEX IX_SatisfactionSurveys_ContactId            (ContactId),
    INDEX IX_SatisfactionSurveys_AccountId            (AccountId),
    CONSTRAINT FK_SatisfactionSurveys_Contact FOREIGN KEY (ContactId)
        REFERENCES Contacts (Id) ON DELETE SET NULL,
    CONSTRAINT FK_SatisfactionSurveys_Account FOREIGN KEY (AccountId)
        REFERENCES Customers (Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS SatisfactionResponses (
    Id           INT           NOT NULL AUTO_INCREMENT,
    SurveyId     INT           NOT NULL,
    Score        INT           NOT NULL,
    Comment      TEXT          NULL,
    Sentiment    INT           NOT NULL DEFAULT 2,   -- SentimentType enum (2=Neutral)
    IpAddress    VARCHAR(45)   NULL,
    UserAgent    VARCHAR(512)  NULL,
    RespondedAt  DATETIME(6)   NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CreatedAt    DATETIME(6)   NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt    DATETIME(6)   NULL     ON UPDATE CURRENT_TIMESTAMP(6),
    IsDeleted    TINYINT(1)    NOT NULL DEFAULT 0,
    RowVersion   TIMESTAMP(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE INDEX UX_SatisfactionResponses_SurveyId (SurveyId),
    CONSTRAINT FK_SatisfactionResponses_Survey FOREIGN KEY (SurveyId)
        REFERENCES SatisfactionSurveys (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;



-- ─────────────────────────────────────────────────────────────────────────────
-- Source: SYS-014-CustomerPortalTables.sql
-- ─────────────────────────────────────────────────────────────────────────────


-- =============================================================================
-- SYS-014: Customer Portal Tables
-- PORTAL-016: EF Core migration verification stub
-- Generated: 2026-02-28
-- Description: Creates PortalUsers and PortalConfigs tables if they are absent.
--              EF Core (CrmDbContext) is the source of truth; this script is
--              provided as a reference for environments where migrations cannot
--              be run directly.
-- =============================================================================
-- NOTE: The authoritative schema is managed by EF Core migrations.
--       Run:
--           dotnet ef migrations add AddCustomerPortal
--               --project src/CRM.Infrastructure --startup-project src/CRM.Api
--           dotnet ef database update
--               --project src/CRM.Infrastructure --startup-project src/CRM.Api
--       to generate and apply a proper migration instead of this script.
-- =============================================================================

-- PortalUsers: Customer self-service portal accounts ---



CREATE TABLE IF NOT EXISTS `PortalUsers` (
    `Id`                        INT             NOT NULL AUTO_INCREMENT,
    `Email`                     VARCHAR(255)    NOT NULL,
    `PasswordHash`              VARCHAR(512)    NOT NULL,
    `DisplayName`               VARCHAR(100)    NULL,
    `ContactId`                 INT             NULL,
    `AccountId`                 INT             NULL,
    `IsActive`                  TINYINT(1)      NOT NULL DEFAULT 1,
    `IsEmailVerified`           TINYINT(1)      NOT NULL DEFAULT 0,
    `EmailVerificationToken`    VARCHAR(128)    NULL,
    `EmailVerifiedAt`           DATETIME(6)     NULL,
    `PasswordResetToken`        VARCHAR(128)    NULL,
    `PasswordResetExpiry`       DATETIME(6)     NULL,
    `LastLoginAt`               DATETIME(6)     NULL,
    `CreatedAt`                 DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt`                 DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    `IsDeleted`                 TINYINT(1)      NOT NULL DEFAULT 0,
    `RowVersion`                BINARY(8)        NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_PortalUsers_Email` (`Email`),
    KEY `IX_PortalUsers_ContactId` (`ContactId`),
    KEY `IX_PortalUsers_AccountId` (`AccountId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- PortalConfigs: Single portal configuration row --------------------------

CREATE TABLE IF NOT EXISTS `PortalConfigs` (
    `Id`                    INT             NOT NULL AUTO_INCREMENT,
    `IsEnabled`             TINYINT(1)      NOT NULL DEFAULT 0,
    `AllowSelfRegistration` TINYINT(1)      NOT NULL DEFAULT 0,
    `PortalTitle`           VARCHAR(100)    NULL,
    `WelcomeMessage`        VARCHAR(1000)   NULL,
    `SupportEmail`          VARCHAR(200)    NULL,
    `LogoUrl`               VARCHAR(500)    NULL,
    `PrimaryColor`          VARCHAR(20)     NULL,
    `AllowedDomains`        VARCHAR(500)    NULL,
    `CreatedAt`             DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt`             DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    `IsDeleted`             TINYINT(1)      NOT NULL DEFAULT 0,
    `RowVersion`            BINARY(8)        NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;



-- ============================================================================
-- Re-enable FK checks
-- ============================================================================
SET FOREIGN_KEY_CHECKS = 1;

SELECT 'CONSOLIDATED-001 migration complete.' AS status;

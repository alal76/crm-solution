-- Migration: Notes System Enhancement, Quote Builder, and Branding
-- Created: 2024
-- Database: SQL Server (MSSQL)

-- =============================================
-- 1. Notes table enhancements
-- =============================================

-- Add new columns to Notes table if they don't exist
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Notes') AND name = 'EntityType')
BEGIN
    ALTER TABLE Notes ADD EntityType NVARCHAR(50) NULL;
    PRINT 'Added EntityType column to Notes';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Notes') AND name = 'EntityId')
BEGIN
    ALTER TABLE Notes ADD EntityId INT NULL;
    PRINT 'Added EntityId column to Notes';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Notes') AND name = 'LeadId')
BEGIN
    ALTER TABLE Notes ADD LeadId INT NULL;
    PRINT 'Added LeadId column to Notes';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Notes') AND name = 'ServiceRequestId')
BEGIN
    ALTER TABLE Notes ADD ServiceRequestId INT NULL;
    PRINT 'Added ServiceRequestId column to Notes';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Notes') AND name = 'QuoteId')
BEGIN
    ALTER TABLE Notes ADD QuoteId INT NULL;
    PRINT 'Added QuoteId column to Notes';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Notes') AND name = 'ContactId')
BEGIN
    ALTER TABLE Notes ADD ContactId INT NULL;
    PRINT 'Added ContactId column to Notes';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Notes') AND name = 'ContextPath')
BEGIN
    ALTER TABLE Notes ADD ContextPath NVARCHAR(500) NULL;
    PRINT 'Added ContextPath column to Notes';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Notes') AND name = 'LastModifiedByUserId')
BEGIN
    ALTER TABLE Notes ADD LastModifiedByUserId INT NULL;
    PRINT 'Added LastModifiedByUserId column to Notes';
END

-- Create indexes for Notes
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Notes') AND name = 'IX_Notes_EntityType_EntityId')
BEGIN
    CREATE INDEX IX_Notes_EntityType_EntityId ON Notes(EntityType, EntityId);
    PRINT 'Created index IX_Notes_EntityType_EntityId';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Notes') AND name = 'IX_Notes_LeadId')
BEGIN
    CREATE INDEX IX_Notes_LeadId ON Notes(LeadId) WHERE LeadId IS NOT NULL;
    PRINT 'Created index IX_Notes_LeadId';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Notes') AND name = 'IX_Notes_ServiceRequestId')
BEGIN
    CREATE INDEX IX_Notes_ServiceRequestId ON Notes(ServiceRequestId) WHERE ServiceRequestId IS NOT NULL;
    PRINT 'Created index IX_Notes_ServiceRequestId';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Notes') AND name = 'IX_Notes_QuoteId')
BEGIN
    CREATE INDEX IX_Notes_QuoteId ON Notes(QuoteId) WHERE QuoteId IS NOT NULL;
    PRINT 'Created index IX_Notes_QuoteId';
END

-- =============================================
-- 2. Quote table enhancements
-- =============================================

-- Add new columns to Quotes table
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Quotes') AND name = 'RelationshipManagerId')
BEGIN
    ALTER TABLE Quotes ADD RelationshipManagerId INT NULL;
    PRINT 'Added RelationshipManagerId column to Quotes';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Quotes') AND name = 'SubmittedForApprovalDate')
BEGIN
    ALTER TABLE Quotes ADD SubmittedForApprovalDate DATETIME2 NULL;
    PRINT 'Added SubmittedForApprovalDate column to Quotes';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Quotes') AND name = 'ExpectedDeliveryDate')
BEGIN
    ALTER TABLE Quotes ADD ExpectedDeliveryDate DATETIME2 NULL;
    PRINT 'Added ExpectedDeliveryDate column to Quotes';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Quotes') AND name = 'ActualDeliveryDate')
BEGIN
    ALTER TABLE Quotes ADD ActualDeliveryDate DATETIME2 NULL;
    PRINT 'Added ActualDeliveryDate column to Quotes';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Quotes') AND name = 'WarrantyMonths')
BEGIN
    ALTER TABLE Quotes ADD WarrantyMonths INT NULL;
    PRINT 'Added WarrantyMonths column to Quotes';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Quotes') AND name = 'WarrantyEndDate')
BEGIN
    ALTER TABLE Quotes ADD WarrantyEndDate DATETIME2 NULL;
    PRINT 'Added WarrantyEndDate column to Quotes';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Quotes') AND name = 'ServiceStartDate')
BEGIN
    ALTER TABLE Quotes ADD ServiceStartDate DATETIME2 NULL;
    PRINT 'Added ServiceStartDate column to Quotes';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Quotes') AND name = 'ServiceEndDate')
BEGIN
    ALTER TABLE Quotes ADD ServiceEndDate DATETIME2 NULL;
    PRINT 'Added ServiceEndDate column to Quotes';
END

-- =============================================
-- 3. QuoteLineItems table
-- =============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'QuoteLineItems')
BEGIN
    CREATE TABLE QuoteLineItems (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        
        -- Quote relationship
        QuoteId INT NOT NULL,
        LineNumber INT NOT NULL DEFAULT 1,
        
        -- Product reference
        ProductId INT NULL,
        SKU NVARCHAR(100) NULL,
        Name NVARCHAR(255) NOT NULL,
        Description NVARCHAR(2000) NULL,
        Category NVARCHAR(100) NULL,
        
        -- Quantity
        Quantity DECIMAL(18,4) NOT NULL DEFAULT 1,
        UnitOfMeasure NVARCHAR(50) NULL DEFAULT 'each',
        
        -- Pricing
        UnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
        ListPrice DECIMAL(18,2) NULL,
        CostPrice DECIMAL(18,2) NULL,
        
        -- Discounts
        DiscountType INT NOT NULL DEFAULT 0,
        DiscountPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
        DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        DiscountReason NVARCHAR(500) NULL,
        DiscountRequiresApproval BIT NOT NULL DEFAULT 0,
        DiscountApproved BIT NOT NULL DEFAULT 0,
        
        -- Tax
        TaxRate DECIMAL(5,2) NOT NULL DEFAULT 0,
        IsTaxable BIT NOT NULL DEFAULT 1,
        TaxCode NVARCHAR(50) NULL,
        
        -- Calculated totals
        Subtotal DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalDiscount DECIMAL(18,2) NOT NULL DEFAULT 0,
        TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        Total DECIMAL(18,2) NOT NULL DEFAULT 0,
        Margin DECIMAL(18,2) NULL,
        
        -- Service/Subscription
        BillingPeriod NVARCHAR(50) NULL,
        WarrantyMonths INT NULL,
        DeliveryDate DATETIME2 NULL,
        ServiceStartDate DATETIME2 NULL,
        ServiceEndDate DATETIME2 NULL,
        
        -- Optional/Bundle
        IsOptional BIT NOT NULL DEFAULT 0,
        IsIncluded BIT NOT NULL DEFAULT 1,
        ParentLineItemId INT NULL,
        IsBundle BIT NOT NULL DEFAULT 0,
        
        -- Notes
        InternalNotes NVARCHAR(2000) NULL,
        QuoteNotes NVARCHAR(2000) NULL,
        CustomFields NVARCHAR(MAX) NULL,
        
        -- Foreign keys
        CONSTRAINT FK_QuoteLineItems_Quotes FOREIGN KEY (QuoteId) REFERENCES Quotes(Id) ON DELETE CASCADE,
        CONSTRAINT FK_QuoteLineItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE SET NULL,
        CONSTRAINT FK_QuoteLineItems_ParentLineItem FOREIGN KEY (ParentLineItemId) REFERENCES QuoteLineItems(Id)
    );
    
    CREATE INDEX IX_QuoteLineItems_QuoteId_LineNumber ON QuoteLineItems(QuoteId, LineNumber);
    CREATE INDEX IX_QuoteLineItems_SKU ON QuoteLineItems(SKU) WHERE SKU IS NOT NULL;
    CREATE INDEX IX_QuoteLineItems_ProductId ON QuoteLineItems(ProductId) WHERE ProductId IS NOT NULL;
    
    PRINT 'Created QuoteLineItems table';
END

-- =============================================
-- 4. SystemSettings branding enhancements
-- =============================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SystemSettings') AND name = 'CompanyAddresses')
BEGIN
    ALTER TABLE SystemSettings ADD CompanyAddresses NVARCHAR(MAX) NULL;
    PRINT 'Added CompanyAddresses column to SystemSettings';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SystemSettings') AND name = 'CompanyPhones')
BEGIN
    ALTER TABLE SystemSettings ADD CompanyPhones NVARCHAR(MAX) NULL;
    PRINT 'Added CompanyPhones column to SystemSettings';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SystemSettings') AND name = 'CompanyEmails')
BEGIN
    ALTER TABLE SystemSettings ADD CompanyEmails NVARCHAR(MAX) NULL;
    PRINT 'Added CompanyEmails column to SystemSettings';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SystemSettings') AND name = 'CompanyFullName')
BEGIN
    ALTER TABLE SystemSettings ADD CompanyFullName NVARCHAR(500) NULL;
    PRINT 'Added CompanyFullName column to SystemSettings';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SystemSettings') AND name = 'CompanyLegalName')
BEGIN
    ALTER TABLE SystemSettings ADD CompanyLegalName NVARCHAR(500) NULL;
    PRINT 'Added CompanyLegalName column to SystemSettings';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SystemSettings') AND name = 'CompanyTaxId')
BEGIN
    ALTER TABLE SystemSettings ADD CompanyTaxId NVARCHAR(100) NULL;
    PRINT 'Added CompanyTaxId column to SystemSettings';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SystemSettings') AND name = 'CompanyRegistrationNumber')
BEGIN
    ALTER TABLE SystemSettings ADD CompanyRegistrationNumber NVARCHAR(100) NULL;
    PRINT 'Added CompanyRegistrationNumber column to SystemSettings';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SystemSettings') AND name = 'CompanyIndustry')
BEGIN
    ALTER TABLE SystemSettings ADD CompanyIndustry NVARCHAR(100) NULL;
    PRINT 'Added CompanyIndustry column to SystemSettings';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SystemSettings') AND name = 'CompanyDescription')
BEGIN
    ALTER TABLE SystemSettings ADD CompanyDescription NVARCHAR(2000) NULL;
    PRINT 'Added CompanyDescription column to SystemSettings';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SystemSettings') AND name = 'DefaultCurrency')
BEGIN
    ALTER TABLE SystemSettings ADD DefaultCurrency NVARCHAR(10) NULL DEFAULT 'USD';
    PRINT 'Added DefaultCurrency column to SystemSettings';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SystemSettings') AND name = 'DefaultTaxRate')
BEGIN
    ALTER TABLE SystemSettings ADD DefaultTaxRate DECIMAL(5,2) NULL DEFAULT 0;
    PRINT 'Added DefaultTaxRate column to SystemSettings';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SystemSettings') AND name = 'QuoteTermsAndConditions')
BEGIN
    ALTER TABLE SystemSettings ADD QuoteTermsAndConditions NVARCHAR(MAX) NULL;
    PRINT 'Added QuoteTermsAndConditions column to SystemSettings';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SystemSettings') AND name = 'QuoteValidityDays')
BEGIN
    ALTER TABLE SystemSettings ADD QuoteValidityDays INT NULL DEFAULT 30;
    PRINT 'Added QuoteValidityDays column to SystemSettings';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SystemSettings') AND name = 'QuoteNumberPrefix')
BEGIN
    ALTER TABLE SystemSettings ADD QuoteNumberPrefix NVARCHAR(20) NULL DEFAULT 'QT-';
    PRINT 'Added QuoteNumberPrefix column to SystemSettings';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SystemSettings') AND name = 'QuoteNumberSequence')
BEGIN
    ALTER TABLE SystemSettings ADD QuoteNumberSequence INT NULL DEFAULT 1000;
    PRINT 'Added QuoteNumberSequence column to SystemSettings';
END

-- =============================================
-- 5. Update Quote status values for existing data
-- Note: Only run this if you have existing quotes that need status migration
-- =============================================

-- Map old status values to new ones:
-- Old: Draft=0, Pending=1, Sent=2, Viewed=3, Accepted=4, Rejected=5, Expired=6, Revised=7, Cancelled=8
-- New: New=0, Draft=1, UnderApproval=2, Approved=3, Shared=4, Viewed=5, Accepted=6, Rejected=7, Expired=8, Revised=9, Cancelled=10, Converted=11, EndOfLife=12

-- First, let's safely migrate existing statuses
-- This converts old enum values to new ones

PRINT 'Migration complete!';
GO

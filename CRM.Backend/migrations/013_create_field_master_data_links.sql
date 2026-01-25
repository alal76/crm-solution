-- Migration: Create FieldMasterDataLinks table
-- Description: Table to link module field configurations to master data sources
-- Date: 2025-01-22

-- Create the FieldMasterDataLinks table
CREATE TABLE IF NOT EXISTS FieldMasterDataLinks (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FieldConfigurationId INT NOT NULL,
    SourceType VARCHAR(50) NOT NULL COMMENT 'LookupCategory, Table, or Api',
    SourceName VARCHAR(255) NOT NULL COMMENT 'Category name, table name, or API endpoint',
    DisplayField VARCHAR(100) DEFAULT NULL COMMENT 'Field to display to user',
    ValueField VARCHAR(100) DEFAULT NULL COMMENT 'Field to store as value',
    FilterExpression TEXT DEFAULT NULL COMMENT 'JSON filter expression for static filters',
    DependsOnField VARCHAR(100) DEFAULT NULL COMMENT 'Parent field name for cascading',
    DependsOnSourceColumn VARCHAR(100) DEFAULT NULL COMMENT 'Source column to filter by parent value',
    AllowFreeText BOOLEAN DEFAULT FALSE COMMENT 'Allow values not in master data',
    ValidationType VARCHAR(50) DEFAULT NULL COMMENT 'regex, required, custom, etc.',
    ValidationPattern VARCHAR(500) DEFAULT NULL COMMENT 'Regex pattern for validation',
    ValidationMessage VARCHAR(500) DEFAULT NULL COMMENT 'Error message for validation failure',
    SortOrder INT DEFAULT 0,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    CONSTRAINT FK_FieldMasterDataLinks_ModuleFieldConfigurations
        FOREIGN KEY (FieldConfigurationId) REFERENCES ModuleFieldConfigurations(Id)
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create indexes for better query performance
CREATE INDEX IX_FieldMasterDataLinks_FieldConfigurationId ON FieldMasterDataLinks(FieldConfigurationId);
CREATE INDEX IX_FieldMasterDataLinks_SourceType ON FieldMasterDataLinks(SourceType);
CREATE INDEX IX_FieldMasterDataLinks_IsActive ON FieldMasterDataLinks(IsActive);
CREATE INDEX IX_FieldMasterDataLinks_DependsOnField ON FieldMasterDataLinks(DependsOnField);

-- Insert some default master data links for address fields
-- These link the existing address fields to the ZipCodes table for cascading dropdowns

-- First, get the field configuration IDs for address fields
-- Note: These will be inserted only if the corresponding fields exist

INSERT INTO FieldMasterDataLinks (FieldConfigurationId, SourceType, SourceName, DisplayField, ValueField, SortOrder, IsActive)
SELECT Id, 'Table', 'ZipCodes', 'Country', 'CountryCode', 1, TRUE
FROM ModuleFieldConfigurations
WHERE FieldName = 'Country' AND ModuleName = 'Customers'
AND NOT EXISTS (
    SELECT 1 FROM FieldMasterDataLinks WHERE FieldConfigurationId = ModuleFieldConfigurations.Id AND SourceName = 'ZipCodes'
);

INSERT INTO FieldMasterDataLinks (FieldConfigurationId, SourceType, SourceName, DisplayField, ValueField, DependsOnField, DependsOnSourceColumn, SortOrder, IsActive)
SELECT Id, 'Table', 'ZipCodes', 'State', 'StateCode', 'Country', 'CountryCode', 2, TRUE
FROM ModuleFieldConfigurations
WHERE FieldName = 'State' AND ModuleName = 'Customers'
AND NOT EXISTS (
    SELECT 1 FROM FieldMasterDataLinks WHERE FieldConfigurationId = ModuleFieldConfigurations.Id AND SourceName = 'ZipCodes'
);

INSERT INTO FieldMasterDataLinks (FieldConfigurationId, SourceType, SourceName, DisplayField, ValueField, DependsOnField, DependsOnSourceColumn, SortOrder, IsActive)
SELECT Id, 'Table', 'ZipCodes', 'City', 'City', 'State', 'StateCode', 3, TRUE
FROM ModuleFieldConfigurations
WHERE FieldName = 'City' AND ModuleName = 'Customers'
AND NOT EXISTS (
    SELECT 1 FROM FieldMasterDataLinks WHERE FieldConfigurationId = ModuleFieldConfigurations.Id AND SourceName = 'ZipCodes'
);

INSERT INTO FieldMasterDataLinks (FieldConfigurationId, SourceType, SourceName, DisplayField, ValueField, DependsOnField, DependsOnSourceColumn, SortOrder, IsActive)
SELECT Id, 'Table', 'ZipCodes', 'PostalCode', 'PostalCode', 'City', 'City', 4, TRUE
FROM ModuleFieldConfigurations
WHERE FieldName IN ('PostalCode', 'ZipCode', 'Zip') AND ModuleName = 'Customers'
AND NOT EXISTS (
    SELECT 1 FROM FieldMasterDataLinks WHERE FieldConfigurationId = ModuleFieldConfigurations.Id AND SourceName = 'ZipCodes'
);

-- Link Salutation field to LookupCategory
INSERT INTO FieldMasterDataLinks (FieldConfigurationId, SourceType, SourceName, DisplayField, ValueField, SortOrder, IsActive)
SELECT Id, 'LookupCategory', 'Salutation', 'Value', 'Key', 1, TRUE
FROM ModuleFieldConfigurations
WHERE FieldName = 'Salutation' AND ModuleName IN ('Customers', 'Contacts', 'Leads')
AND NOT EXISTS (
    SELECT 1 FROM FieldMasterDataLinks WHERE FieldConfigurationId = ModuleFieldConfigurations.Id AND SourceName = 'Salutation'
);

-- Link Gender field to LookupCategory
INSERT INTO FieldMasterDataLinks (FieldConfigurationId, SourceType, SourceName, DisplayField, ValueField, SortOrder, IsActive)
SELECT Id, 'LookupCategory', 'Gender', 'Value', 'Key', 1, TRUE
FROM ModuleFieldConfigurations
WHERE FieldName = 'Gender' AND ModuleName IN ('Customers', 'Contacts', 'Leads')
AND NOT EXISTS (
    SELECT 1 FROM FieldMasterDataLinks WHERE FieldConfigurationId = ModuleFieldConfigurations.Id AND SourceName = 'Gender'
);

-- Link LifecycleStage field to LookupCategory
INSERT INTO FieldMasterDataLinks (FieldConfigurationId, SourceType, SourceName, DisplayField, ValueField, SortOrder, IsActive)
SELECT Id, 'LookupCategory', 'LifecycleStage', 'Value', 'Key', 1, TRUE
FROM ModuleFieldConfigurations
WHERE FieldName IN ('LifecycleStage', 'Stage', 'CustomerStage') AND ModuleName = 'Customers'
AND NOT EXISTS (
    SELECT 1 FROM FieldMasterDataLinks WHERE FieldConfigurationId = ModuleFieldConfigurations.Id AND SourceName = 'LifecycleStage'
);

-- Link LeadSource field to LookupCategory
INSERT INTO FieldMasterDataLinks (FieldConfigurationId, SourceType, SourceName, DisplayField, ValueField, SortOrder, IsActive)
SELECT Id, 'LookupCategory', 'LeadSource', 'Value', 'Key', 1, TRUE
FROM ModuleFieldConfigurations
WHERE FieldName IN ('LeadSource', 'Source') AND ModuleName IN ('Leads', 'Opportunities')
AND NOT EXISTS (
    SELECT 1 FROM FieldMasterDataLinks WHERE FieldConfigurationId = ModuleFieldConfigurations.Id AND SourceName = 'LeadSource'
);

-- Link LeadStatus field to LookupCategory
INSERT INTO FieldMasterDataLinks (FieldConfigurationId, SourceType, SourceName, DisplayField, ValueField, SortOrder, IsActive)
SELECT Id, 'LookupCategory', 'LeadStatus', 'Value', 'Key', 1, TRUE
FROM ModuleFieldConfigurations
WHERE FieldName IN ('LeadStatus', 'Status') AND ModuleName = 'Leads'
AND NOT EXISTS (
    SELECT 1 FROM FieldMasterDataLinks WHERE FieldConfigurationId = ModuleFieldConfigurations.Id AND SourceName = 'LeadStatus'
);

-- Link OpportunityStage field to LookupCategory
INSERT INTO FieldMasterDataLinks (FieldConfigurationId, SourceType, SourceName, DisplayField, ValueField, SortOrder, IsActive)
SELECT Id, 'LookupCategory', 'OpportunityStage', 'Value', 'Key', 1, TRUE
FROM ModuleFieldConfigurations
WHERE FieldName IN ('OpportunityStage', 'Stage') AND ModuleName = 'Opportunities'
AND NOT EXISTS (
    SELECT 1 FROM FieldMasterDataLinks WHERE FieldConfigurationId = ModuleFieldConfigurations.Id AND SourceName = 'OpportunityStage'
);

-- Link Currency field to LookupCategory
INSERT INTO FieldMasterDataLinks (FieldConfigurationId, SourceType, SourceName, DisplayField, ValueField, SortOrder, IsActive)
SELECT Id, 'LookupCategory', 'Currency', 'Value', 'Key', 1, TRUE
FROM ModuleFieldConfigurations
WHERE FieldName IN ('Currency', 'CurrencyCode') AND ModuleName IN ('Customers', 'Opportunities', 'Products')
AND NOT EXISTS (
    SELECT 1 FROM FieldMasterDataLinks WHERE FieldConfigurationId = ModuleFieldConfigurations.Id AND SourceName = 'Currency'
);

-- Link ProductCategory field to LookupCategory
INSERT INTO FieldMasterDataLinks (FieldConfigurationId, SourceType, SourceName, DisplayField, ValueField, SortOrder, IsActive)
SELECT Id, 'LookupCategory', 'ProductCategory', 'Value', 'Key', 1, TRUE
FROM ModuleFieldConfigurations
WHERE FieldName IN ('Category', 'ProductCategory') AND ModuleName = 'Products'
AND NOT EXISTS (
    SELECT 1 FROM FieldMasterDataLinks WHERE FieldConfigurationId = ModuleFieldConfigurations.Id AND SourceName = 'ProductCategory'
);

-- Link PreferredContactMethod field to LookupCategory
INSERT INTO FieldMasterDataLinks (FieldConfigurationId, SourceType, SourceName, DisplayField, ValueField, SortOrder, IsActive)
SELECT Id, 'LookupCategory', 'PreferredContactMethod', 'Value', 'Key', 1, TRUE
FROM ModuleFieldConfigurations
WHERE FieldName IN ('PreferredContactMethod', 'ContactMethod') AND ModuleName IN ('Customers', 'Contacts')
AND NOT EXISTS (
    SELECT 1 FROM FieldMasterDataLinks WHERE FieldConfigurationId = ModuleFieldConfigurations.Id AND SourceName = 'PreferredContactMethod'
);

SELECT 'Migration 013 completed successfully' AS Status;

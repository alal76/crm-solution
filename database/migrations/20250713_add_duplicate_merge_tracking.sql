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
-- ADD MERGE TRACKING COLUMNS TO ACCOUNTS (CUSTOMERS)
-- =====================================================

ALTER TABLE Customers
    ADD COLUMN IF NOT EXISTS MergedIntoId INT NULL COMMENT 'ID of record this was merged into',
    ADD COLUMN IF NOT EXISTS MergeGroupId INT NULL COMMENT 'ID of the merge group',
    ADD COLUMN IF NOT EXISTS IsMergedDuplicate TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Was this merged as a duplicate',
    ADD COLUMN IF NOT EXISTS MergedAt DATETIME NULL COMMENT 'When this was merged';

ALTER TABLE Customers
    ADD INDEX IF NOT EXISTS IX_Customers_MergedIntoId (MergedIntoId),
    ADD INDEX IF NOT EXISTS IX_Customers_IsMergedDuplicate (IsMergedDuplicate);

-- =====================================================
-- SEED DEFAULT DUPLICATE DETECTION RULES (if not exists)
-- =====================================================

-- Lead duplicate detection rule
INSERT INTO DuplicateRules (Name, EntityType, Description, IsActive, MatchThreshold, Priority, CreatedAt)
SELECT 'Lead Duplicate Detection', 'Lead', 'Detects duplicate leads by email, name, and company', 1, 70, 1, NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM DuplicateRules WHERE EntityType = 'Lead' AND Name = 'Lead Duplicate Detection'
);

-- Get the Lead rule ID for adding match fields
SET @LeadRuleId = (SELECT Id FROM DuplicateRules WHERE EntityType = 'Lead' AND Name = 'Lead Duplicate Detection' LIMIT 1);

-- Lead match fields (if rule was just created)
INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @LeadRuleId, 'Email', 'Exact', 100, 'Lowercase,Trim', 0, NOW()
WHERE @LeadRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @LeadRuleId AND FieldName = 'Email'
);

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @LeadRuleId, 'FirstName', 'Fuzzy', 40, 'Lowercase,Trim', 0, NOW()
WHERE @LeadRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @LeadRuleId AND FieldName = 'FirstName'
);

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @LeadRuleId, 'LastName', 'Fuzzy', 50, 'Lowercase,Trim', 0, NOW()
WHERE @LeadRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @LeadRuleId AND FieldName = 'LastName'
);

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @LeadRuleId, 'CompanyName', 'Fuzzy', 30, 'Lowercase,Trim,RemoveCompanySuffixes', 0, NOW()
WHERE @LeadRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @LeadRuleId AND FieldName = 'CompanyName'
);

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @LeadRuleId, 'Phone', 'Normalized', 60, 'RemoveNonNumeric', 0, NOW()
WHERE @LeadRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @LeadRuleId AND FieldName = 'Phone'
);

-- Contact duplicate detection rule
INSERT INTO DuplicateRules (Name, EntityType, Description, IsActive, MatchThreshold, Priority, CreatedAt)
SELECT 'Contact Duplicate Detection', 'Contact', 'Detects duplicate contacts by email, name, and phone', 1, 70, 1, NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM DuplicateRules WHERE EntityType = 'Contact' AND Name = 'Contact Duplicate Detection'
);

SET @ContactRuleId = (SELECT Id FROM DuplicateRules WHERE EntityType = 'Contact' AND Name = 'Contact Duplicate Detection' LIMIT 1);

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @ContactRuleId, 'EmailPrimary', 'Exact', 100, 'Lowercase,Trim', 0, NOW()
WHERE @ContactRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @ContactRuleId AND FieldName = 'EmailPrimary'
);

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @ContactRuleId, 'FirstName', 'Fuzzy', 40, 'Lowercase,Trim', 0, NOW()
WHERE @ContactRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @ContactRuleId AND FieldName = 'FirstName'
);

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @ContactRuleId, 'LastName', 'Fuzzy', 50, 'Lowercase,Trim', 0, NOW()
WHERE @ContactRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @ContactRuleId AND FieldName = 'LastName'
);

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @ContactRuleId, 'PhonePrimary', 'Normalized', 60, 'RemoveNonNumeric', 0, NOW()
WHERE @ContactRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @ContactRuleId AND FieldName = 'PhonePrimary'
);

-- Account duplicate detection rule
INSERT INTO DuplicateRules (Name, EntityType, Description, IsActive, MatchThreshold, Priority, CreatedAt)
SELECT 'Account Duplicate Detection', 'Account', 'Detects duplicate accounts by company, email, and phone', 1, 70, 1, NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM DuplicateRules WHERE EntityType = 'Account' AND Name = 'Account Duplicate Detection'
);

SET @AccountRuleId = (SELECT Id FROM DuplicateRules WHERE EntityType = 'Account' AND Name = 'Account Duplicate Detection' LIMIT 1);

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @AccountRuleId, 'Email', 'EmailDomain', 80, 'Lowercase,Trim', 0, NOW()
WHERE @AccountRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @AccountRuleId AND FieldName = 'Email'
);

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @AccountRuleId, 'Company', 'Fuzzy', 70, 'Lowercase,Trim,RemoveCompanySuffixes', 0, NOW()
WHERE @AccountRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @AccountRuleId AND FieldName = 'Company'
);

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @AccountRuleId, 'Phone', 'Normalized', 50, 'RemoveNonNumeric', 0, NOW()
WHERE @AccountRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @AccountRuleId AND FieldName = 'Phone'
);

INSERT INTO DuplicateMatchFields (RuleId, FieldName, MatchType, Weight, Transformations, IsRequired, CreatedAt)
SELECT @AccountRuleId, 'Website', 'Normalized', 60, 'Lowercase,RemoveProtocol,RemoveWWW', 0, NOW()
WHERE @AccountRuleId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM DuplicateMatchFields WHERE RuleId = @AccountRuleId AND FieldName = 'Website'
);

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

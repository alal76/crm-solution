-- Migration: Security Enhancements for Password Management and Group Policies
-- Created: Security features for password complexity, expiration, and 2FA

-- ============================================
-- Add password management fields to Users table
-- ============================================

-- Check and add PasswordLastChangedAt column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'PasswordLastChangedAt')
BEGIN
    ALTER TABLE Users ADD PasswordLastChangedAt DATETIME2(7) NULL;
    PRINT 'Added PasswordLastChangedAt column to Users table';
END
GO

-- Check and add MustResetPassword column  
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'MustResetPassword')
BEGIN
    ALTER TABLE Users ADD MustResetPassword BIT NOT NULL DEFAULT 0;
    PRINT 'Added MustResetPassword column to Users table';
END
GO

-- Check and add PasswordNeverSet column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'PasswordNeverSet')
BEGIN
    ALTER TABLE Users ADD PasswordNeverSet BIT NOT NULL DEFAULT 0;
    PRINT 'Added PasswordNeverSet column to Users table';
END
GO

-- ============================================
-- Add security policy fields to UserGroups table
-- ============================================

-- Check and add PasswordExpirationDays column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserGroups' AND COLUMN_NAME = 'PasswordExpirationDays')
BEGIN
    ALTER TABLE UserGroups ADD PasswordExpirationDays INT NULL;
    PRINT 'Added PasswordExpirationDays column to UserGroups table';
END
GO

-- Check and add PasswordExpirationPolicy column (0=None, 1=MustChange, 2=Alert, 3=Warn)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserGroups' AND COLUMN_NAME = 'PasswordExpirationPolicy')
BEGIN
    ALTER TABLE UserGroups ADD PasswordExpirationPolicy INT NOT NULL DEFAULT 0;
    PRINT 'Added PasswordExpirationPolicy column to UserGroups table';
END
GO

-- Check and add PasswordExpirationWarningDays column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserGroups' AND COLUMN_NAME = 'PasswordExpirationWarningDays')
BEGIN
    ALTER TABLE UserGroups ADD PasswordExpirationWarningDays INT NULL DEFAULT 7;
    PRINT 'Added PasswordExpirationWarningDays column to UserGroups table';
END
GO

-- Check and add RequireTwoFactor column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserGroups' AND COLUMN_NAME = 'RequireTwoFactor')
BEGIN
    ALTER TABLE UserGroups ADD RequireTwoFactor BIT NOT NULL DEFAULT 0;
    PRINT 'Added RequireTwoFactor column to UserGroups table';
END
GO

-- Check and add EnforceTwoFactor column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserGroups' AND COLUMN_NAME = 'EnforceTwoFactor')
BEGIN
    ALTER TABLE UserGroups ADD EnforceTwoFactor BIT NOT NULL DEFAULT 0;
    PRINT 'Added EnforceTwoFactor column to UserGroups table';
END
GO

-- ============================================
-- Add password complexity fields to SystemSettings table
-- ============================================

-- Check and add MaxPasswordLength column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SystemSettings' AND COLUMN_NAME = 'MaxPasswordLength')
BEGIN
    ALTER TABLE SystemSettings ADD MaxPasswordLength INT NOT NULL DEFAULT 128;
    PRINT 'Added MaxPasswordLength column to SystemSettings table';
END
GO

-- Check and add RequireUppercase column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SystemSettings' AND COLUMN_NAME = 'RequireUppercase')
BEGIN
    ALTER TABLE SystemSettings ADD RequireUppercase BIT NOT NULL DEFAULT 1;
    PRINT 'Added RequireUppercase column to SystemSettings table';
END
GO

-- Check and add RequireLowercase column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SystemSettings' AND COLUMN_NAME = 'RequireLowercase')
BEGIN
    ALTER TABLE SystemSettings ADD RequireLowercase BIT NOT NULL DEFAULT 1;
    PRINT 'Added RequireLowercase column to SystemSettings table';
END
GO

-- Check and add RequireNumbers column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SystemSettings' AND COLUMN_NAME = 'RequireNumbers')
BEGIN
    ALTER TABLE SystemSettings ADD RequireNumbers BIT NOT NULL DEFAULT 1;
    PRINT 'Added RequireNumbers column to SystemSettings table';
END
GO

-- Check and add RequireSpecialChars column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SystemSettings' AND COLUMN_NAME = 'RequireSpecialChars')
BEGIN
    ALTER TABLE SystemSettings ADD RequireSpecialChars BIT NOT NULL DEFAULT 0;
    PRINT 'Added RequireSpecialChars column to SystemSettings table';
END
GO

-- Check and add DefaultPasswordExpirationDays column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SystemSettings' AND COLUMN_NAME = 'DefaultPasswordExpirationDays')
BEGIN
    ALTER TABLE SystemSettings ADD DefaultPasswordExpirationDays INT NOT NULL DEFAULT 0;
    PRINT 'Added DefaultPasswordExpirationDays column to SystemSettings table';
END
GO

-- ============================================
-- Set PasswordLastChangedAt for existing users
-- ============================================
UPDATE Users 
SET PasswordLastChangedAt = CreatedAt 
WHERE PasswordLastChangedAt IS NULL AND PasswordHash IS NOT NULL AND PasswordHash != '';
PRINT 'Updated PasswordLastChangedAt for existing users';
GO

PRINT 'Security enhancement migration completed successfully';
GO

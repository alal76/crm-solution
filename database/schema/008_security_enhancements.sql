-- ============================================================================
-- CRM Solution Database Schema - Security Enhancements
-- Version: 1.0
-- Date: 2026
-- Description: Password management, complexity, and group security policies
-- Database: MariaDB/MySQL
-- ============================================================================

SET NAMES utf8mb4;

-- ============================================
-- Add password management fields to Users table
-- ============================================

-- PasswordLastChangedAt - Track when password was last changed
SET @column_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users' AND COLUMN_NAME = 'PasswordLastChangedAt');
SET @sql = IF(@column_exists = 0, 
    'ALTER TABLE Users ADD COLUMN PasswordLastChangedAt datetime(6) DEFAULT NULL AFTER TwoFactorSecret', 
    'SELECT "PasswordLastChangedAt already exists"');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- MustResetPassword - Admin-forced password reset
SET @column_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users' AND COLUMN_NAME = 'MustResetPassword');
SET @sql = IF(@column_exists = 0, 
    'ALTER TABLE Users ADD COLUMN MustResetPassword tinyint(1) NOT NULL DEFAULT 0 AFTER PasswordLastChangedAt', 
    'SELECT "MustResetPassword already exists"');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- PasswordNeverSet - User has never set a password (first login)
SET @column_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users' AND COLUMN_NAME = 'PasswordNeverSet');
SET @sql = IF(@column_exists = 0, 
    'ALTER TABLE Users ADD COLUMN PasswordNeverSet tinyint(1) NOT NULL DEFAULT 0 AFTER MustResetPassword', 
    'SELECT "PasswordNeverSet already exists"');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- BackupCodes - Encrypted 2FA backup codes
SET @column_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users' AND COLUMN_NAME = 'BackupCodes');
SET @sql = IF(@column_exists = 0, 
    'ALTER TABLE Users ADD COLUMN BackupCodes text DEFAULT NULL AFTER PasswordNeverSet', 
    'SELECT "BackupCodes already exists"');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- PasswordResetToken - Token for password reset flow
SET @column_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users' AND COLUMN_NAME = 'PasswordResetToken');
SET @sql = IF(@column_exists = 0, 
    'ALTER TABLE Users ADD COLUMN PasswordResetToken varchar(512) DEFAULT NULL AFTER BackupCodes', 
    'SELECT "PasswordResetToken already exists"');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- PasswordResetTokenExpiry - Expiration of reset token
SET @column_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users' AND COLUMN_NAME = 'PasswordResetTokenExpiry');
SET @sql = IF(@column_exists = 0, 
    'ALTER TABLE Users ADD COLUMN PasswordResetTokenExpiry datetime(6) DEFAULT NULL AFTER PasswordResetToken', 
    'SELECT "PasswordResetTokenExpiry already exists"');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================
-- Add security policy fields to UserGroups table
-- ============================================

-- PasswordExpirationDays - Days until password expires for group members
SET @column_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserGroups' AND COLUMN_NAME = 'PasswordExpirationDays');
SET @sql = IF(@column_exists = 0, 
    'ALTER TABLE UserGroups ADD COLUMN PasswordExpirationDays int(11) DEFAULT NULL AFTER DataAccessScope', 
    'SELECT "PasswordExpirationDays already exists"');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- PasswordExpirationPolicy - Action on expiration: 0=None, 1=MustChange, 2=Alert, 3=Warn
SET @column_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserGroups' AND COLUMN_NAME = 'PasswordExpirationPolicy');
SET @sql = IF(@column_exists = 0, 
    'ALTER TABLE UserGroups ADD COLUMN PasswordExpirationPolicy int(11) NOT NULL DEFAULT 0 COMMENT ''0=None, 1=MustChange, 2=Alert, 3=Warn'' AFTER PasswordExpirationDays', 
    'SELECT "PasswordExpirationPolicy already exists"');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- PasswordExpirationWarningDays - Days before expiration to show warning
SET @column_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserGroups' AND COLUMN_NAME = 'PasswordExpirationWarningDays');
SET @sql = IF(@column_exists = 0, 
    'ALTER TABLE UserGroups ADD COLUMN PasswordExpirationWarningDays int(11) DEFAULT 7 AFTER PasswordExpirationPolicy', 
    'SELECT "PasswordExpirationWarningDays already exists"');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- RequireTwoFactor - Group prefers two-factor authentication
SET @column_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserGroups' AND COLUMN_NAME = 'RequireTwoFactor');
SET @sql = IF(@column_exists = 0, 
    'ALTER TABLE UserGroups ADD COLUMN RequireTwoFactor tinyint(1) NOT NULL DEFAULT 0 AFTER PasswordExpirationWarningDays', 
    'SELECT "RequireTwoFactor already exists"');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- EnforceTwoFactor - Two-factor is mandatory for group members
SET @column_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserGroups' AND COLUMN_NAME = 'EnforceTwoFactor');
SET @sql = IF(@column_exists = 0, 
    'ALTER TABLE UserGroups ADD COLUMN EnforceTwoFactor tinyint(1) NOT NULL DEFAULT 0 AFTER RequireTwoFactor', 
    'SELECT "EnforceTwoFactor already exists"');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================
-- Add password complexity fields to SystemSettings table
-- ============================================

-- MaxPasswordLength - Maximum password length (0 = no limit)
SET @column_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SystemSettings' AND COLUMN_NAME = 'MaxPasswordLength');
SET @sql = IF(@column_exists = 0, 
    'ALTER TABLE SystemSettings ADD COLUMN MaxPasswordLength int(11) NOT NULL DEFAULT 128', 
    'SELECT "MaxPasswordLength already exists"');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- RequireUppercase - Require at least one uppercase letter
SET @column_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SystemSettings' AND COLUMN_NAME = 'RequireUppercase');
SET @sql = IF(@column_exists = 0, 
    'ALTER TABLE SystemSettings ADD COLUMN RequireUppercase tinyint(1) NOT NULL DEFAULT 1', 
    'SELECT "RequireUppercase already exists"');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- RequireLowercase - Require at least one lowercase letter
SET @column_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SystemSettings' AND COLUMN_NAME = 'RequireLowercase');
SET @sql = IF(@column_exists = 0, 
    'ALTER TABLE SystemSettings ADD COLUMN RequireLowercase tinyint(1) NOT NULL DEFAULT 1', 
    'SELECT "RequireLowercase already exists"');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- RequireNumbers - Require at least one digit
SET @column_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SystemSettings' AND COLUMN_NAME = 'RequireNumbers');
SET @sql = IF(@column_exists = 0, 
    'ALTER TABLE SystemSettings ADD COLUMN RequireNumbers tinyint(1) NOT NULL DEFAULT 1', 
    'SELECT "RequireNumbers already exists"');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- RequireSpecialChars - Require at least one special character
SET @column_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SystemSettings' AND COLUMN_NAME = 'RequireSpecialChars');
SET @sql = IF(@column_exists = 0, 
    'ALTER TABLE SystemSettings ADD COLUMN RequireSpecialChars tinyint(1) NOT NULL DEFAULT 0', 
    'SELECT "RequireSpecialChars already exists"');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- DefaultPasswordExpirationDays - Default expiration (0 = never)
SET @column_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SystemSettings' AND COLUMN_NAME = 'DefaultPasswordExpirationDays');
SET @sql = IF(@column_exists = 0, 
    'ALTER TABLE SystemSettings ADD COLUMN DefaultPasswordExpirationDays int(11) NOT NULL DEFAULT 0', 
    'SELECT "DefaultPasswordExpirationDays already exists"');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================
-- Set PasswordLastChangedAt for existing users
-- ============================================
UPDATE Users 
SET PasswordLastChangedAt = CreatedAt 
WHERE PasswordLastChangedAt IS NULL 
  AND PasswordHash IS NOT NULL 
  AND PasswordHash != ''; -- NOSONAR plsql:NullComparison - empty string check for MariaDB, not null comparison

SELECT 'Security enhancement migration completed successfully' AS Status;

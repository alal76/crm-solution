-- Migration: Extend User preferences with additional settings
-- Description: Adds language, timezone, date/time format, rows per page, and notification preferences
-- Date: 2026-01-29

-- Add Language column if not exists
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Language'
)
BEGIN
    ALTER TABLE Users ADD Language NVARCHAR(10) NULL;
    PRINT 'Added Language column to Users table';
END
GO

-- Add Timezone column if not exists
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Timezone'
)
BEGIN
    ALTER TABLE Users ADD Timezone NVARCHAR(100) NULL;
    PRINT 'Added Timezone column to Users table';
END
GO

-- Add DateFormat column if not exists
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'DateFormat'
)
BEGIN
    ALTER TABLE Users ADD DateFormat NVARCHAR(20) NULL;
    PRINT 'Added DateFormat column to Users table';
END
GO

-- Add TimeFormat column if not exists
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'TimeFormat'
)
BEGIN
    ALTER TABLE Users ADD TimeFormat NVARCHAR(10) NULL;
    PRINT 'Added TimeFormat column to Users table';
END
GO

-- Add RowsPerPage column if not exists
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'RowsPerPage'
)
BEGIN
    ALTER TABLE Users ADD RowsPerPage INT NULL;
    PRINT 'Added RowsPerPage column to Users table';
END
GO

-- Add EmailNotifications column if not exists
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'EmailNotifications'
)
BEGIN
    ALTER TABLE Users ADD EmailNotifications BIT NULL;
    PRINT 'Added EmailNotifications column to Users table';
END
GO

-- Add DesktopNotifications column if not exists
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'DesktopNotifications'
)
BEGIN
    ALTER TABLE Users ADD DesktopNotifications BIT NULL;
    PRINT 'Added DesktopNotifications column to Users table';
END
GO

-- Add CompactMode column if not exists
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'CompactMode'
)
BEGIN
    ALTER TABLE Users ADD CompactMode BIT NULL;
    PRINT 'Added CompactMode column to Users table';
END
GO

-- Update ThemePreference default to 'system' for existing 'light' values
-- Only update if the value is still the old default
UPDATE Users 
SET ThemePreference = 'system' 
WHERE ThemePreference = 'light' OR ThemePreference IS NULL;
PRINT 'Updated ThemePreference to system for existing users';
GO

-- Migration: Add ThemePreference column to Users table
-- Description: Stores user's preferred UI theme (light, dark, high-contrast)
-- Date: 2026-01-29

-- Check if column exists, if not, add it
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' 
    AND COLUMN_NAME = 'ThemePreference'
)
BEGIN
    ALTER TABLE Users ADD ThemePreference NVARCHAR(50) NULL;
    
    -- Set default value for existing users
    UPDATE Users SET ThemePreference = 'light' WHERE ThemePreference IS NULL;
    
    PRINT 'Added ThemePreference column to Users table';
END
ELSE
BEGIN
    PRINT 'ThemePreference column already exists';
END
GO

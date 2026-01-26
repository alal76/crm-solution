-- Add QuickAdminLoginEnabled column to SystemSettings table
-- This column controls whether the Quick Admin Login button is shown on the login page
-- Default: true (enabled for development, should be disabled in production)

ALTER TABLE SystemSettings ADD COLUMN IF NOT EXISTS QuickAdminLoginEnabled TINYINT(1) NOT NULL DEFAULT 1;

-- Verify the column was added
SELECT COLUMN_NAME, DATA_TYPE, COLUMN_DEFAULT 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'crm_db' 
AND TABLE_NAME = 'SystemSettings' 
AND COLUMN_NAME = 'QuickAdminLoginEnabled';

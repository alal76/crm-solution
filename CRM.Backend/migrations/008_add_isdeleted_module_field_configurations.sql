-- Add IsDeleted column to ModuleFieldConfigurations for existing databases
-- This will not fail if the column already exists on modern MariaDB/MySQL versions.
ALTER TABLE `ModuleFieldConfigurations`
  ADD COLUMN IF NOT EXISTS `IsDeleted` tinyint(1) NOT NULL DEFAULT 0;

-- Fallback for older servers without IF NOT EXISTS support: check information_schema
-- and add column only if missing (uncomment if needed):
-- SET @col_exists = (SELECT COUNT(*) FROM information_schema.COLUMNS
-- WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ModuleFieldConfigurations' AND COLUMN_NAME = 'IsDeleted');
-- PREPARE stmt FROM 'ALTER TABLE `ModuleFieldConfigurations` ADD COLUMN `IsDeleted` tinyint(1) NOT NULL DEFAULT 0';
-- IF @col_exists = 0 THEN
--   EXECUTE stmt;
-- END IF;
-- DEALLOCATE PREPARE stmt;

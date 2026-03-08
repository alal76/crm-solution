-- MariaDB Initialization Script
-- Creates the CRM database (single database policy)

-- Use default database
USE crm_db;

-- DEPRECATED: Demo database removed — single database policy (see copilot-instructions.md)
-- CREATE DATABASE IF NOT EXISTS crm_demodb 
--     CHARACTER SET utf8mb4 
--     COLLATE utf8mb4_unicode_ci;
-- GRANT ALL PRIVILEGES ON crm_demodb.* TO 'crm_user'@'%';
-- FLUSH PRIVILEGES;

-- Log initialization
SELECT 'CRM MariaDB initialization complete' AS Status;
SELECT NOW() AS InitTime;

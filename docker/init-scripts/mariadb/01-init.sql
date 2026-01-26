-- MariaDB Initialization Script
-- Creates the CRM database and demo database with seed data

-- Use default database
USE crm_db;

-- Create demo database
CREATE DATABASE IF NOT EXISTS crm_demodb 
    CHARACTER SET utf8mb4 
    COLLATE utf8mb4_unicode_ci;

-- Grant user access to demo database
GRANT ALL PRIVILEGES ON crm_demodb.* TO 'crm_user'@'%';
FLUSH PRIVILEGES;

-- Log initialization
SELECT 'CRM MariaDB initialization complete' AS Status;
SELECT NOW() AS InitTime;

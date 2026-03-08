-- SQL Server Initialization Script
-- Creates the CRM database (single database policy)

-- DEPRECATED: Demo database removed — single database policy (see copilot-instructions.md)
-- IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'crm_demodb')
-- BEGIN
--     CREATE DATABASE crm_demodb;
--     PRINT 'Demo database created';
-- END
-- GO

-- Use main database
USE crm_db;
GO

-- Log initialization
PRINT 'CRM SQL Server initialization complete';
SELECT GETDATE() AS InitTime;
GO

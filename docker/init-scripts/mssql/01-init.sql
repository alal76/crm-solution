-- SQL Server Initialization Script
-- Creates the CRM database and demo database with seed data

-- Create demo database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'crm_demodb')
BEGIN
    CREATE DATABASE crm_demodb;
    PRINT 'Demo database created';
END
GO

-- Use main database
USE crm_db;
GO

-- Log initialization
PRINT 'CRM SQL Server initialization complete';
SELECT GETDATE() AS InitTime;
GO

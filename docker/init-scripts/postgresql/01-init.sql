-- PostgreSQL Initialization Script
-- Creates the CRM database extensions and demo database with seed data

-- Create demo database
SELECT 'Creating demo database...' AS Status;
-- Note: CREATE DATABASE cannot run in a transaction block, so it's handled separately
-- This script runs after the main database is created

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Log initialization
SELECT 'CRM PostgreSQL initialization complete' AS Status;
SELECT NOW() AS InitTime;

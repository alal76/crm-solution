-- Migration: Add ITSM Permission to UserGroups
-- Description: Adds CanAccessITSM column to UserGroups table for role-based ITSM access control
-- Date: 2026-02-03

-- Add CanAccessITSM column to UserGroups table
ALTER TABLE UserGroups ADD COLUMN IF NOT EXISTS CanAccessITSM BOOLEAN NOT NULL DEFAULT FALSE;

-- Grant ITSM access to system admin groups (IsSystemAdmin = true)
UPDATE UserGroups SET CanAccessITSM = TRUE WHERE IsSystemAdmin = TRUE;

-- Verify the column was added
SELECT 'CanAccessITSM column added successfully' AS Status;

-- Migration: 010_fix_workflow_engine_schema.sql
-- Created: 2025-01-23
-- Description: Fixes workflow engine table schemas to match C# entities

-- Fix WorkflowDefinitions - add missing columns
ALTER TABLE WorkflowDefinitions 
  ADD COLUMN IF NOT EXISTS LastModifiedByUserId INT NULL AFTER CreatedByUserId,
  ADD COLUMN IF NOT EXISTS IsDeleted TINYINT(1) NOT NULL DEFAULT 0;

-- Fix WorkflowDefinitionVersions - add IsDeleted
ALTER TABLE WorkflowDefinitionVersions 
  ADD COLUMN IF NOT EXISTS UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP AFTER CreatedAt,
  ADD COLUMN IF NOT EXISTS IsDeleted TINYINT(1) NOT NULL DEFAULT 0;

-- Fix WorkflowSteps - add IsDeleted
ALTER TABLE WorkflowSteps 
  ADD COLUMN IF NOT EXISTS IsDeleted TINYINT(1) NOT NULL DEFAULT 0;

-- Fix WorkflowInstances - rename and add columns to match entity
ALTER TABLE WorkflowInstances 
  ADD COLUMN IF NOT EXISTS RetryCount INT NOT NULL DEFAULT 0 AFTER ErrorMessage,
  ADD COLUMN IF NOT EXISTS NextRetryAt DATETIME NULL AFTER RetryCount,
  ADD COLUMN IF NOT EXISTS ProcessingStartedAt DATETIME NULL AFTER ProcessingWorkerId,
  ADD COLUMN IF NOT EXISTS IsDeleted TINYINT(1) NOT NULL DEFAULT 0;

-- Rename InitiatedByUserId to StartedByUserId if exists
-- Note: This is handled separately if the column exists
-- ALTER TABLE WorkflowInstances CHANGE COLUMN InitiatedByUserId StartedByUserId INT NULL;

-- Fix WorkflowEngineEvents - add IsDeleted
ALTER TABLE WorkflowEngineEvents 
  ADD COLUMN IF NOT EXISTS CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER CorrelationId,
  ADD COLUMN IF NOT EXISTS UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP AFTER CreatedAt,
  ADD COLUMN IF NOT EXISTS IsDeleted TINYINT(1) NOT NULL DEFAULT 0;

-- Fix WorkflowEngineTasks - add missing columns
ALTER TABLE WorkflowEngineTasks 
  MODIFY COLUMN Priority VARCHAR(20) NOT NULL DEFAULT 'Normal',
  ADD COLUMN IF NOT EXISTS Comments LONGTEXT NULL AFTER ActionTaken,
  ADD COLUMN IF NOT EXISTS EscalationLevel INT NOT NULL DEFAULT 0 AFTER AvailableActions,
  ADD COLUMN IF NOT EXISTS ReminderCount INT NOT NULL DEFAULT 0 AFTER EscalationLevel,
  ADD COLUMN IF NOT EXISTS LastReminderAt DATETIME NULL AFTER ReminderCount,
  ADD COLUMN IF NOT EXISTS IsDeleted TINYINT(1) NOT NULL DEFAULT 0;

-- Fix WorkflowContextVariables - add IsDeleted
ALTER TABLE WorkflowContextVariables 
  ADD COLUMN IF NOT EXISTS IsDeleted TINYINT(1) NOT NULL DEFAULT 0;

-- Fix WorkflowSchedules - add missing columns
ALTER TABLE WorkflowSchedules 
  ADD COLUMN IF NOT EXISTS Name VARCHAR(200) NOT NULL DEFAULT '' AFTER WorkflowDefinitionId,
  ADD COLUMN IF NOT EXISTS Description VARCHAR(500) NULL AFTER Name,
  ADD COLUMN IF NOT EXISTS ContextData LONGTEXT NULL AFTER ExecutionCount,
  ADD COLUMN IF NOT EXISTS IsDeleted TINYINT(1) NOT NULL DEFAULT 0;

-- Fix WorkflowJobs - add missing columns and rename mismatched columns
ALTER TABLE WorkflowJobs 
  ADD COLUMN IF NOT EXISTS StepKey VARCHAR(100) NULL AFTER WorkflowInstanceId,
  ADD COLUMN IF NOT EXISTS WorkflowTaskId INT NULL AFTER StepKey,
  ADD COLUMN IF NOT EXISTS CorrelationId VARCHAR(100) NULL AFTER VisibilityTimeoutAt,
  ADD COLUMN IF NOT EXISTS IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
  MODIFY COLUMN WorkflowInstanceId INT NULL,
  MODIFY COLUMN Payload LONGTEXT NULL;

-- Rename LastError to ErrorMessage if it exists (only on fresh installs)
-- ALTER TABLE WorkflowJobs CHANGE COLUMN LastError ErrorMessage LONGTEXT NULL;
-- ALTER TABLE WorkflowJobs CHANGE COLUMN Result ResultData LONGTEXT NULL;

-- Fix WorkflowApiCredentials - add IsDeleted
ALTER TABLE WorkflowApiCredentials 
  ADD COLUMN IF NOT EXISTS IsDeleted TINYINT(1) NOT NULL DEFAULT 0;

SELECT 'Workflow Engine schema fixes applied successfully' AS Result;

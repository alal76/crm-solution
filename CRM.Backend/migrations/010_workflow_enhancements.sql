-- CRM Solution - Customer Relationship Management System
-- Copyright (C) 2024-2026 Abhishek Lal
--
-- Migration: Add workflow enhancements for idempotency, skip steps, and audit log
-- File: 010_workflow_enhancements.sql

-- =====================================================
-- 1. Add idempotency key to workflow tasks
-- =====================================================
ALTER TABLE workflow_tasks
ADD COLUMN IF NOT EXISTS idempotency_key VARCHAR(64) NULL AFTER task_type,
ADD COLUMN IF NOT EXISTS skip_reason VARCHAR(500) NULL AFTER error_message,
ADD COLUMN IF NOT EXISTS skipped_at DATETIME NULL AFTER skip_reason,
ADD COLUMN IF NOT EXISTS skipped_by VARCHAR(255) NULL AFTER skipped_at;

-- Create unique index for idempotency
CREATE UNIQUE INDEX IF NOT EXISTS idx_workflow_tasks_idempotency_key 
ON workflow_tasks(idempotency_key) 
WHERE idempotency_key IS NOT NULL;

-- =====================================================
-- 2. Add version control to workflow definitions
-- =====================================================
ALTER TABLE workflow_definitions
ADD COLUMN IF NOT EXISTS version_number INT NOT NULL DEFAULT 1 AFTER version,
ADD COLUMN IF NOT EXISTS version_notes TEXT NULL AFTER version_number,
ADD COLUMN IF NOT EXISTS parent_version_id CHAR(36) NULL AFTER version_notes,
ADD COLUMN IF NOT EXISTS owner_id CHAR(36) NULL AFTER created_by,
ADD COLUMN IF NOT EXISTS last_modified_by CHAR(36) NULL AFTER owner_id,
ADD COLUMN IF NOT EXISTS last_modified_at DATETIME NULL AFTER last_modified_by;

-- Add foreign key for version lineage
ALTER TABLE workflow_definitions
ADD CONSTRAINT fk_workflow_definitions_parent_version
FOREIGN KEY (parent_version_id) REFERENCES workflow_definitions(id)
ON DELETE SET NULL;

-- =====================================================
-- 3. Create workflow audit log table
-- =====================================================
CREATE TABLE IF NOT EXISTS workflow_audit_log (
    id CHAR(36) NOT NULL PRIMARY KEY,
    workflow_definition_id CHAR(36) NOT NULL,
    workflow_instance_id CHAR(36) NULL,
    event_type VARCHAR(50) NOT NULL,
    event_category VARCHAR(50) NOT NULL DEFAULT 'system',
    actor_id CHAR(36) NULL,
    actor_name VARCHAR(255) NULL,
    actor_type VARCHAR(50) NOT NULL DEFAULT 'user',
    target_entity VARCHAR(100) NULL,
    target_entity_id CHAR(36) NULL,
    old_value JSON NULL,
    new_value JSON NULL,
    change_summary TEXT NULL,
    ip_address VARCHAR(45) NULL,
    user_agent TEXT NULL,
    request_id VARCHAR(64) NULL,
    correlation_id VARCHAR(64) NULL,
    duration_ms INT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'success',
    error_message TEXT NULL,
    metadata JSON NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    INDEX idx_workflow_audit_log_definition (workflow_definition_id),
    INDEX idx_workflow_audit_log_instance (workflow_instance_id),
    INDEX idx_workflow_audit_log_event_type (event_type),
    INDEX idx_workflow_audit_log_event_category (event_category),
    INDEX idx_workflow_audit_log_actor (actor_id),
    INDEX idx_workflow_audit_log_created_at (created_at),
    INDEX idx_workflow_audit_log_correlation (correlation_id),
    
    CONSTRAINT fk_workflow_audit_log_definition 
    FOREIGN KEY (workflow_definition_id) REFERENCES workflow_definitions(id)
    ON DELETE CASCADE
);

-- =====================================================
-- 4. Add skip step tracking to node instances
-- =====================================================
ALTER TABLE workflow_node_instances
ADD COLUMN IF NOT EXISTS was_skipped BOOLEAN NOT NULL DEFAULT FALSE AFTER status,
ADD COLUMN IF NOT EXISTS skip_reason VARCHAR(500) NULL AFTER was_skipped,
ADD COLUMN IF NOT EXISTS skipped_by CHAR(36) NULL AFTER skip_reason,
ADD COLUMN IF NOT EXISTS skipped_at DATETIME NULL AFTER skipped_by;

-- =====================================================
-- 5. Create workflow metrics table for monitoring
-- =====================================================
CREATE TABLE IF NOT EXISTS workflow_metrics (
    id CHAR(36) NOT NULL PRIMARY KEY,
    workflow_definition_id CHAR(36) NOT NULL,
    metric_date DATE NOT NULL,
    metric_hour TINYINT NOT NULL DEFAULT 0,
    
    -- Execution metrics
    instances_started INT NOT NULL DEFAULT 0,
    instances_completed INT NOT NULL DEFAULT 0,
    instances_failed INT NOT NULL DEFAULT 0,
    instances_cancelled INT NOT NULL DEFAULT 0,
    
    -- Task metrics
    tasks_created INT NOT NULL DEFAULT 0,
    tasks_completed INT NOT NULL DEFAULT 0,
    tasks_failed INT NOT NULL DEFAULT 0,
    tasks_retried INT NOT NULL DEFAULT 0,
    tasks_dead_lettered INT NOT NULL DEFAULT 0,
    tasks_skipped INT NOT NULL DEFAULT 0,
    
    -- Human task metrics
    human_tasks_created INT NOT NULL DEFAULT 0,
    human_tasks_completed INT NOT NULL DEFAULT 0,
    human_tasks_overdue INT NOT NULL DEFAULT 0,
    human_tasks_reassigned INT NOT NULL DEFAULT 0,
    
    -- Duration metrics (in milliseconds)
    avg_instance_duration_ms BIGINT NULL,
    min_instance_duration_ms BIGINT NULL,
    max_instance_duration_ms BIGINT NULL,
    avg_task_duration_ms BIGINT NULL,
    avg_human_task_duration_ms BIGINT NULL,
    
    -- LLM metrics
    llm_calls INT NOT NULL DEFAULT 0,
    llm_tokens_used INT NOT NULL DEFAULT 0,
    llm_failures INT NOT NULL DEFAULT 0,
    llm_avg_latency_ms BIGINT NULL,
    
    -- Queue metrics
    queue_depth_snapshot INT NULL,
    dead_letter_queue_depth INT NULL,
    
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    
    UNIQUE INDEX idx_workflow_metrics_unique (workflow_definition_id, metric_date, metric_hour),
    INDEX idx_workflow_metrics_date (metric_date),
    
    CONSTRAINT fk_workflow_metrics_definition
    FOREIGN KEY (workflow_definition_id) REFERENCES workflow_definitions(id)
    ON DELETE CASCADE
);

-- =====================================================
-- 6. Create LLM usage tracking table
-- =====================================================
CREATE TABLE IF NOT EXISTS workflow_llm_usage (
    id CHAR(36) NOT NULL PRIMARY KEY,
    workflow_instance_id CHAR(36) NOT NULL,
    node_instance_id CHAR(36) NULL,
    task_id CHAR(36) NULL,
    
    provider VARCHAR(50) NOT NULL,
    model VARCHAR(100) NOT NULL,
    
    prompt_tokens INT NOT NULL DEFAULT 0,
    completion_tokens INT NOT NULL DEFAULT 0,
    total_tokens INT NOT NULL DEFAULT 0,
    
    request_duration_ms INT NULL,
    success BOOLEAN NOT NULL DEFAULT TRUE,
    error_message TEXT NULL,
    
    prompt_preview TEXT NULL,
    response_preview TEXT NULL,
    
    cost_usd DECIMAL(10, 6) NULL,
    
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    INDEX idx_workflow_llm_usage_instance (workflow_instance_id),
    INDEX idx_workflow_llm_usage_provider (provider),
    INDEX idx_workflow_llm_usage_created_at (created_at),
    
    CONSTRAINT fk_workflow_llm_usage_instance
    FOREIGN KEY (workflow_instance_id) REFERENCES workflow_instances(id)
    ON DELETE CASCADE
);

-- =====================================================
-- 7. Add permission fields to workflow nodes
-- =====================================================
ALTER TABLE workflow_nodes
ADD COLUMN IF NOT EXISTS allowed_roles JSON NULL AFTER config_json,
ADD COLUMN IF NOT EXISTS approvers JSON NULL AFTER allowed_roles,
ADD COLUMN IF NOT EXISTS require_mfa BOOLEAN NOT NULL DEFAULT FALSE AFTER approvers;

-- =====================================================
-- 8. Add subprocess support
-- =====================================================
ALTER TABLE workflow_instances
ADD COLUMN IF NOT EXISTS parent_instance_id CHAR(36) NULL AFTER workflow_definition_id,
ADD COLUMN IF NOT EXISTS parent_node_instance_id CHAR(36) NULL AFTER parent_instance_id,
ADD COLUMN IF NOT EXISTS subprocess_level INT NOT NULL DEFAULT 0 AFTER parent_node_instance_id;

-- Add foreign key for subprocess hierarchy
ALTER TABLE workflow_instances
ADD CONSTRAINT fk_workflow_instances_parent_instance
FOREIGN KEY (parent_instance_id) REFERENCES workflow_instances(id)
ON DELETE SET NULL;

-- Create index for subprocess queries
CREATE INDEX IF NOT EXISTS idx_workflow_instances_parent 
ON workflow_instances(parent_instance_id);

-- =====================================================
-- 9. Seed lifecycle workflow definitions
-- =====================================================

-- Lead Lifecycle Workflow
INSERT INTO workflow_definitions (
    id, name, description, entity_type, trigger_type, trigger_config,
    is_active, is_deleted, version, version_number, created_at, created_by
) VALUES (
    UUID(), 
    'Lead Lifecycle Management',
    'Automated workflow for managing lead lifecycle from creation to conversion or disqualification',
    'Lead',
    'OnCreate',
    '{"conditions": []}',
    FALSE,
    FALSE,
    '1.0.0',
    1,
    NOW(),
    'system'
) ON DUPLICATE KEY UPDATE updated_at = NOW();

-- Customer Onboarding Workflow
INSERT INTO workflow_definitions (
    id, name, description, entity_type, trigger_type, trigger_config,
    is_active, is_deleted, version, version_number, created_at, created_by
) VALUES (
    UUID(),
    'Customer Onboarding',
    'Automated onboarding process for new customers including welcome email, task creation, and follow-up scheduling',
    'Customer',
    'OnCreate',
    '{"conditions": []}',
    FALSE,
    FALSE,
    '1.0.0',
    1,
    NOW(),
    'system'
) ON DUPLICATE KEY UPDATE updated_at = NOW();

-- Opportunity Follow-up Workflow
INSERT INTO workflow_definitions (
    id, name, description, entity_type, trigger_type, trigger_config,
    is_active, is_deleted, version, version_number, created_at, created_by
) VALUES (
    UUID(),
    'Opportunity Follow-up Automation',
    'Automatic follow-up reminders and tasks for opportunities based on stage and last activity',
    'Opportunity',
    'OnUpdate',
    '{"conditions": [{"field": "stage", "operator": "changed"}]}',
    FALSE,
    FALSE,
    '1.0.0',
    1,
    NOW(),
    'system'
) ON DUPLICATE KEY UPDATE updated_at = NOW();

-- Ticket Escalation Workflow
INSERT INTO workflow_definitions (
    id, name, description, entity_type, trigger_type, trigger_config,
    is_active, is_deleted, version, version_number, created_at, created_by
) VALUES (
    UUID(),
    'Support Ticket Escalation',
    'Automatic escalation of support tickets based on priority, SLA violations, and response time',
    'Ticket',
    'Scheduled',
    '{"cron": "*/15 * * * *", "description": "Check every 15 minutes"}',
    FALSE,
    FALSE,
    '1.0.0',
    1,
    NOW(),
    'system'
) ON DUPLICATE KEY UPDATE updated_at = NOW();

-- Quote Approval Workflow
INSERT INTO workflow_definitions (
    id, name, description, entity_type, trigger_type, trigger_config,
    is_active, is_deleted, version, version_number, created_at, created_by
) VALUES (
    UUID(),
    'Quote Approval Process',
    'Multi-level approval workflow for quotes based on amount thresholds',
    'Quote',
    'OnCreate',
    '{"conditions": [{"field": "status", "operator": "equals", "value": "pending_approval"}]}',
    FALSE,
    FALSE,
    '1.0.0',
    1,
    NOW(),
    'system'
) ON DUPLICATE KEY UPDATE updated_at = NOW();

-- =====================================================
-- 10. Create circuit breaker state tracking table
-- =====================================================
CREATE TABLE IF NOT EXISTS workflow_circuit_breaker_state (
    id CHAR(36) NOT NULL PRIMARY KEY,
    service_name VARCHAR(100) NOT NULL,
    state VARCHAR(20) NOT NULL DEFAULT 'closed',
    failure_count INT NOT NULL DEFAULT 0,
    success_count INT NOT NULL DEFAULT 0,
    last_failure_at DATETIME NULL,
    last_success_at DATETIME NULL,
    last_state_change_at DATETIME NULL,
    last_error_message TEXT NULL,
    opened_at DATETIME NULL,
    closes_at DATETIME NULL,
    metadata JSON NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    
    UNIQUE INDEX idx_circuit_breaker_service (service_name)
);

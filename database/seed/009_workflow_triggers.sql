-- ============================================================================
-- CRM Solution Database Seed Data - Workflow Triggers
-- Version: 1.0
-- Date: 2026-02-22
-- Description: Default workflow trigger configurations for workflow definitions
-- Tables: WorkflowTriggers
-- Depends on: 008_workflow_definitions.sql (WorkflowDefinitions)
-- ============================================================================
-- TriggerType enum values:
--   Manual=0, OnCreate=1, OnUpdate=2, OnDelete=3, OnFieldChange=4,
--   Scheduled=5, OnEvent=6, OnWebhook=7, OnSLABreach=8, OnEscalation=9
-- ============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================================
-- WorkflowTriggers - Trigger configurations for each workflow definition
-- ============================================================================
INSERT INTO WorkflowTriggers (
  Id, WorkflowDefinitionId, Name, TriggerType, EntityType, EventName,
  CronExpression, FilterConditions, WatchedField, OldValue, NewValue,
  IsActive, Priority, Description,
  LastTriggeredAt, NextScheduledAt, ExecutionCount,
  DelaySeconds, RunAsync, MaxRetries, CreatedById,
  CreatedAt, UpdatedAt, IsDeleted
) VALUES

-- ============================================================================
-- Lead Management Triggers
-- ============================================================================

-- 1. New Lead Auto-Assignment (Workflow ID 1)
-- Fires immediately when a new lead is created to assign it to a sales rep
(1, 1, 'New Lead Created - Auto Assignment',
 1, -- OnCreate
 'Lead', NULL, NULL,
 NULL,
 NULL, NULL, NULL,
 1, 10,
 'Triggers automatic lead assignment when a new lead is created in the system. Routes leads based on territory, product interest, or round-robin rules.',
 NULL, NULL, 0,
 0, 1, 3, NULL,
 NOW(), NOW(), 0),

-- 2a. Lead Follow-up Reminder - OnCreate trigger
-- Creates follow-up task when a new lead is added
(2, 2, 'Lead Created - Schedule Follow-up',
 1, -- OnCreate
 'Lead', NULL, NULL,
 NULL,
 NULL, NULL, NULL,
 1, 20,
 'When a new lead is created, schedules an initial follow-up task if no contact is made within 3 days.',
 NULL, NULL, 0,
 0, 1, 3, NULL,
 NOW(), NOW(), 0),

-- 2b. Lead Follow-up Reminder - Scheduled daily check
-- Runs daily at 8 AM to check for leads needing follow-up
(3, 2, 'Daily Lead Follow-up Check',
 5, -- Scheduled
 'Lead', NULL,
 '0 8 * * *', -- Daily at 8:00 AM
 NULL,
 NULL, NULL, NULL,
 1, 50,
 'Runs daily at 8:00 AM to identify leads that have not been contacted in 3 days and creates follow-up reminder tasks.',
 NULL, NULL, 0,
 0, 1, 3, NULL,
 NOW(), NOW(), 0),

-- 3. Lead Qualification Scoring (Workflow ID 3)
-- Recalculates lead score when lead status changes
(4, 3, 'Lead Status Change - Recalculate Score',
 2, -- OnUpdate
 'Lead', NULL, NULL,
 '{"rules":[{"field":"Status","operator":"changed"}]}',
 'Status', NULL, NULL,
 1, 20,
 'Triggers lead score recalculation when the lead Status field is updated. Evaluates engagement metrics, demographics, and activity history.',
 NULL, NULL, 0,
 0, 1, 3, NULL,
 NOW(), NOW(), 0),

-- 4. Hot Lead Alert (Workflow ID 4)
-- Notifies sales manager when lead score reaches 80+
(5, 4, 'Lead Score Threshold - Hot Lead Alert',
 4, -- OnFieldChange
 'Lead', NULL, NULL,
 '{"rules":[{"field":"Score","operator":"greaterThanOrEqual","value":"80"}]}',
 'Score', NULL, NULL,
 1, 10,
 'Fires when a lead score reaches 80 or above, immediately notifying the sales manager and flagging the lead as hot for priority follow-up.',
 NULL, NULL, 0,
 0, 1, 3, NULL,
 NOW(), NOW(), 0),

-- ============================================================================
-- Sales Triggers
-- ============================================================================

-- 5. Opportunity Stage Change Notification (Workflow ID 5)
-- Notifies stakeholders when opportunity moves to a new stage
(6, 5, 'Opportunity Stage Changed - Notify Stakeholders',
 4, -- OnFieldChange
 'Opportunity', NULL, NULL,
 NULL,
 'Stage', NULL, NULL,
 1, 20,
 'Triggers notifications to relevant stakeholders (owner, manager, team) whenever an opportunity moves to a different pipeline stage.',
 NULL, NULL, 0,
 0, 1, 3, NULL,
 NOW(), NOW(), 0),

-- 6. Opportunity Close Date Reminder (Workflow ID 6)
-- Daily check at 9 AM for approaching close dates
(7, 6, 'Daily Close Date Reminder Check',
 5, -- Scheduled
 'Opportunity', NULL,
 '0 9 * * *', -- Daily at 9:00 AM
 '{"rules":[{"field":"ExpectedCloseDate","operator":"withinDays","value":"7"},{"field":"Stage","operator":"notIn","value":"Won,Lost"}]}',
 NULL, NULL, NULL,
 1, 50,
 'Runs daily at 9:00 AM to check for opportunities with close dates within the next 7 days and sends reminders to opportunity owners.',
 NULL, NULL, 0,
 0, 1, 3, NULL,
 NOW(), NOW(), 0),

-- 7. Stale Opportunity Alert (Workflow ID 7)
-- Weekly check on Monday at 8 AM for stale opportunities
(8, 7, 'Weekly Stale Opportunity Check',
 5, -- Scheduled
 'Opportunity', NULL,
 '0 8 * * 1', -- Every Monday at 8:00 AM
 '{"rules":[{"field":"LastActivityDate","operator":"olderThanDays","value":"14"},{"field":"Stage","operator":"notIn","value":"Won,Lost"}]}',
 NULL, NULL, NULL,
 1, 50,
 'Runs every Monday at 8:00 AM to identify opportunities with no activity in the last 14 days. Alerts owners and managers to take action.',
 NULL, NULL, 0,
 0, 1, 3, NULL,
 NOW(), NOW(), 0),

-- 8. Won Opportunity Processing (Workflow ID 8)
-- Triggers post-sale actions when opportunity is marked as Won
(9, 8, 'Opportunity Won - Post-Sale Processing',
 4, -- OnFieldChange
 'Opportunity', NULL, NULL,
 NULL,
 'Stage', NULL, 'Won',
 1, 10,
 'Fires when an opportunity stage changes to Won. Triggers post-sale actions including order creation, welcome email, onboarding task generation, and commission calculation.',
 NULL, NULL, 0,
 0, 1, 3, NULL,
 NOW(), NOW(), 0),

-- ============================================================================
-- Service Desk Triggers
-- ============================================================================

-- 9. New Ticket Assignment (Workflow ID 9)
-- Auto-assigns new service requests based on type and priority
(10, 9, 'New Service Request - Auto Assignment',
 1, -- OnCreate
 'ServiceRequest', NULL, NULL,
 NULL,
 NULL, NULL, NULL,
 1, 10,
 'Triggers automatic ticket assignment when a new service request is created. Routes tickets based on category, priority, and agent availability.',
 NULL, NULL, 0,
 0, 1, 3, NULL,
 NOW(), NOW(), 0),

-- 10a. SLA Breach Warning - Scheduled check every 15 minutes
(11, 10, 'SLA Breach Check - Scheduled',
 5, -- Scheduled
 'ServiceRequest', NULL,
 '*/15 * * * *', -- Every 15 minutes
 '{"rules":[{"field":"SLAStatus","operator":"in","value":"AtRisk,Warning"},{"field":"Status","operator":"notIn","value":"Resolved,Closed"}]}',
 NULL, NULL, NULL,
 1, 10,
 'Runs every 15 minutes to check for service requests approaching SLA breach. Sends warnings to assignees and managers for at-risk tickets.',
 NULL, NULL, 0,
 0, 1, 3, NULL,
 NOW(), NOW(), 0),

-- 10b. SLA Breach Warning - Event-based trigger
(12, 10, 'SLA Breach Warning - Event',
 6, -- OnEvent
 'ServiceRequest', 'SLABreachWarning', NULL,
 NULL,
 NULL, NULL, NULL,
 1, 5,
 'Fires when the SLA monitoring system raises an SLABreachWarning event for a service request that is about to breach its response or resolution SLA.',
 NULL, NULL, 0,
 0, 1, 3, NULL,
 NOW(), NOW(), 0),

-- 11. Escalation Workflow (Workflow ID 11)
-- Triggered by escalation events
(13, 11, 'Escalation Required - Event Trigger',
 6, -- OnEvent
 'ServiceRequest', 'EscalationRequired', NULL,
 NULL,
 NULL, NULL, NULL,
 1, 5,
 'Fires when an EscalationRequired event is raised for a service request. Escalates the ticket to the next support tier based on escalation rules.',
 NULL, NULL, 0,
 0, 1, 3, NULL,
 NOW(), NOW(), 0),

-- 12. Customer Satisfaction Survey (Workflow ID 12)
-- Sends survey when ticket is resolved
(14, 12, 'Ticket Resolved - Send CSAT Survey',
 4, -- OnFieldChange
 'ServiceRequest', NULL, NULL,
 NULL,
 'Status', NULL, 'Resolved',
 1, 100,
 'Fires when a service request status changes to Resolved. Sends a customer satisfaction survey to the requester after a configurable delay.',
 NULL, NULL, 0,
 300, 1, 3, NULL, -- 300 second (5 min) delay before sending survey
 NOW(), NOW(), 0),

-- ============================================================================
-- Marketing Triggers
-- ============================================================================

-- 13. Campaign Launch Checklist (Workflow ID 13)
-- Validates campaign setup when status changes to Active
(15, 13, 'Campaign Activated - Launch Checklist',
 4, -- OnFieldChange
 'Campaign', NULL, NULL,
 NULL,
 'Status', NULL, 'Active',
 1, 10,
 'Fires when a campaign status changes to Active. Runs a launch checklist to validate campaign setup, audience segments, content, and budget allocation.',
 NULL, NULL, 0,
 0, 1, 3, NULL,
 NOW(), NOW(), 0),

-- 14. Lead Nurture Sequence (Workflow ID 14)
-- Enrolls marketing-sourced leads in nurture campaigns
(16, 14, 'Marketing Lead Created - Nurture Enrollment',
 1, -- OnCreate
 'Lead', NULL, NULL,
 '{"rules":[{"field":"Source","operator":"equals","value":"Marketing"}]}',
 NULL, NULL, NULL,
 1, 20,
 'Fires when a new lead is created with Source = Marketing. Enrolls the lead in the appropriate nurture email sequence based on campaign and interest.',
 NULL, NULL, 0,
 60, 1, 3, NULL, -- 60 second delay to allow lead data to settle
 NOW(), NOW(), 0),

-- ============================================================================
-- Approval Triggers
-- ============================================================================

-- 15. Quote Approval (Workflow ID 15)
-- Routes quotes for approval when amount exceeds threshold
(17, 15, 'Quote Created - Approval Required',
 1, -- OnCreate
 'Quote', NULL, NULL,
 '{"rules":[{"field":"TotalAmount","operator":"greaterThan","value":"10000"}]}',
 NULL, NULL, NULL,
 1, 10,
 'Fires when a new quote is created with TotalAmount exceeding $10,000. Routes the quote through the approval workflow based on discount level and amount tier.',
 NULL, NULL, 0,
 0, 1, 3, NULL,
 NOW(), NOW(), 0),

-- 16. Large Deal Approval (Workflow ID 16)
-- Requires executive approval for high-value opportunities
(18, 16, 'Large Deal Detected - Executive Approval',
 4, -- OnFieldChange
 'Opportunity', NULL, NULL,
 '{"rules":[{"field":"Amount","operator":"greaterThan","value":"100000"}]}',
 'Amount', NULL, NULL,
 1, 5,
 'Fires when an opportunity amount exceeds $100,000. Requires executive-level approval before the deal can proceed to the next stage.',
 NULL, NULL, 0,
 0, 1, 3, NULL,
 NOW(), NOW(), 0),

-- ============================================================================
-- Notification Triggers
-- ============================================================================

-- 17. New Customer Welcome (Workflow ID 17)
-- Sends welcome email when a new account is created
(19, 17, 'New Account Created - Welcome Sequence',
 1, -- OnCreate
 'Account', NULL, NULL,
 NULL,
 NULL, NULL, NULL,
 1, 20,
 'Fires when a new customer account is created. Sends a welcome email, creates onboarding tasks, and notifies the account manager.',
 NULL, NULL, 0,
 0, 1, 3, NULL,
 NOW(), NOW(), 0),

-- 18. Task Due Reminder (Workflow ID 18)
-- Daily check at 7 AM for tasks due today
(20, 18, 'Daily Task Due Reminder',
 5, -- Scheduled
 'Task', NULL,
 '0 7 * * *', -- Daily at 7:00 AM
 '{"rules":[{"field":"DueDate","operator":"equals","value":"today"},{"field":"Status","operator":"notIn","value":"Completed,Cancelled"}]}',
 NULL, NULL, NULL,
 1, 50,
 'Runs daily at 7:00 AM to identify tasks due today and sends reminder notifications to assignees. Includes overdue tasks in a separate alert.',
 NULL, NULL, 0,
 0, 1, 3, NULL,
 NOW(), NOW(), 0),

-- ============================================================================
-- Data Quality Triggers
-- ============================================================================

-- 19. Duplicate Detection (Workflow ID 19)
-- Checks for duplicates when a new lead is created
(21, 19, 'New Lead Created - Duplicate Check',
 1, -- OnCreate
 'Lead', NULL, NULL,
 NULL,
 NULL, NULL, NULL,
 1, 5,
 'Fires when a new lead is created to check for potential duplicates based on email, phone, and company name matching. Alerts the creator if potential duplicates are found.',
 NULL, NULL, 0,
 5, 1, 3, NULL, -- 5 second delay to ensure lead is fully saved
 NOW(), NOW(), 0),

-- 20. Data Quality Check (Workflow ID 20)
-- Weekly data quality scan on Sunday at 2 AM
(22, 20, 'Weekly Data Quality Scan',
 5, -- Scheduled
 'Customer', NULL,
 '0 2 * * 0', -- Every Sunday at 2:00 AM
 NULL,
 NULL, NULL, NULL,
 1, 100,
 'Runs every Sunday at 2:00 AM to scan all customer records for missing required fields, invalid email formats, outdated information, and data consistency issues.',
 NULL, NULL, 0,
 0, 1, 3, NULL,
 NOW(), NOW(), 0)

ON DUPLICATE KEY UPDATE
  Name = VALUES(Name),
  Description = VALUES(Description),
  TriggerType = VALUES(TriggerType),
  EntityType = VALUES(EntityType),
  EventName = VALUES(EventName),
  CronExpression = VALUES(CronExpression),
  FilterConditions = VALUES(FilterConditions),
  WatchedField = VALUES(WatchedField),
  OldValue = VALUES(OldValue),
  NewValue = VALUES(NewValue),
  IsActive = VALUES(IsActive),
  Priority = VALUES(Priority);

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================================
-- Verification query
-- ============================================================================
-- SELECT wt.Id, wt.Name, wt.TriggerType, wt.EntityType, wt.CronExpression,
--        wt.WatchedField, wt.Priority, wd.Name AS WorkflowName
-- FROM WorkflowTriggers wt
-- JOIN WorkflowDefinitions wd ON wd.Id = wt.WorkflowDefinitionId
-- WHERE wt.IsDeleted = 0
-- ORDER BY wt.WorkflowDefinitionId, wt.Priority;
-- ============================================================================

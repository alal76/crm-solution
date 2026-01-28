-- ============================================================================
-- CRM Solution Database Seed Data - Workflow Definitions
-- Version: 1.0
-- Date: 2026-01-28
-- Description: Default workflow templates and automation rules
-- Tables: WorkflowDefinitions, WorkflowVersions, WorkflowNodes, WorkflowTransitions
-- ============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================================
-- WorkflowDefinitions - Main workflow templates
-- Status: 0=Draft, 1=Active, 2=Paused, 3=Archived, 4=Deprecated
-- ============================================================================
INSERT INTO WorkflowDefinitions (
  Id, WorkflowKey, Name, Description, Category, EntityType, Status,
  CurrentVersion, IconName, Color, IsSystem, Priority,
  MaxConcurrentInstances, DefaultTimeoutHours, Tags, Metadata, CreatedAt, IsDeleted
) VALUES
-- Lead Management Workflows
(1, 'lead-assignment', 'New Lead Assignment', 
 'Automatically assign new leads to sales representatives based on territory or round-robin', 
 'Lead Management', 'Lead', 1, 1, 'UserPlus', '#2563eb', 1, 1, 100, 24, 
 'leads,assignment,automation', NULL, NOW(), 0),

(2, 'lead-followup', 'Lead Follow-up Reminder', 
 'Create follow-up task when lead has not been contacted in 3 days', 
 'Lead Management', 'Lead', 1, 1, 'Clock', '#f59e0b', 1, 2, 500, 48, 
 'leads,followup,reminder', NULL, NOW(), 0),

(3, 'lead-scoring', 'Lead Qualification Scoring', 
 'Update lead score based on activity and engagement metrics', 
 'Lead Management', 'Lead', 1, 1, 'TrendingUp', '#10b981', 1, 2, 200, 12, 
 'leads,scoring', NULL, NOW(), 0),

(4, 'hot-lead-alert', 'Hot Lead Alert', 
 'Notify sales manager when a lead reaches high score threshold', 
 'Lead Management', 'Lead', 1, 1, 'Flame', '#ef4444', 1, 1, 50, 1, 
 'leads,alert,notification', NULL, NOW(), 0),

-- Sales Process Workflows
(5, 'opp-stage-notification', 'Opportunity Stage Change Notification', 
 'Notify relevant parties when opportunity moves to a new stage', 
 'Sales Process', 'Opportunity', 1, 1, 'ArrowRight', '#8b5cf6', 1, 2, 200, 4, 
 'sales,opportunity,notification', NULL, NOW(), 0),

(6, 'opp-close-reminder', 'Opportunity Close Date Reminder', 
 'Remind owner when expected close date is approaching', 
 'Sales Process', 'Opportunity', 1, 1, 'Calendar', '#f97316', 1, 2, 500, 48, 
 'sales,reminder', NULL, NOW(), 0),

(7, 'stale-opp-alert', 'Stale Opportunity Alert', 
 'Alert when opportunity has no activity for 14 days', 
 'Sales Process', 'Opportunity', 1, 1, 'AlertTriangle', '#eab308', 1, 2, 300, 72, 
 'sales,alert', NULL, NOW(), 0),

(8, 'won-opportunity', 'Won Opportunity Processing', 
 'Trigger post-sale actions when opportunity is marked as won', 
 'Sales Process', 'Opportunity', 1, 1, 'Trophy', '#22c55e', 1, 1, 100, 24, 
 'sales,won,automation', NULL, NOW(), 0),

-- Customer Service Workflows
(9, 'ticket-assignment', 'New Ticket Assignment', 
 'Auto-assign new service requests based on type and priority', 
 'Service Desk', 'ServiceRequest', 1, 1, 'TicketCheck', '#06b6d4', 1, 1, 200, 8, 
 'service,assignment', NULL, NOW(), 0),

(10, 'sla-breach-warning', 'SLA Breach Warning', 
 'Alert before SLA breach for high priority service requests', 
 'Service Desk', 'ServiceRequest', 1, 1, 'Clock', '#dc2626', 1, 1, 500, 1, 
 'service,sla,alert', NULL, NOW(), 0),

(11, 'ticket-escalation', 'Escalation Workflow', 
 'Escalate tickets based on priority and age thresholds', 
 'Service Desk', 'ServiceRequest', 1, 1, 'ArrowUp', '#b91c1c', 1, 1, 100, 4, 
 'service,escalation', NULL, NOW(), 0),

(12, 'csat-survey', 'Customer Satisfaction Survey', 
 'Send satisfaction survey after ticket resolution', 
 'Service Desk', 'ServiceRequest', 1, 1, 'Star', '#a855f7', 1, 3, 500, 72, 
 'service,survey', NULL, NOW(), 0),

-- Marketing Workflows
(13, 'campaign-launch', 'Campaign Launch Checklist', 
 'Validate campaign setup and requirements before launch', 
 'Marketing', 'Campaign', 1, 1, 'Rocket', '#ec4899', 1, 2, 50, 24, 
 'marketing,campaign', NULL, NOW(), 0),

(14, 'lead-nurture', 'Lead Nurture Sequence', 
 'Enroll leads in nurture campaign based on source and criteria', 
 'Marketing', 'Lead', 1, 1, 'Mail', '#14b8a6', 1, 3, 1000, 168, 
 'marketing,nurture', NULL, NOW(), 0),

-- Approval Workflows
(15, 'quote-approval', 'Quote Approval Workflow', 
 'Route quotes for approval based on discount level and amount', 
 'Approvals', 'Quote', 1, 1, 'FileCheck', '#3b82f6', 1, 1, 100, 48, 
 'sales,quote,approval', NULL, NOW(), 0),

(16, 'large-deal-approval', 'Large Deal Approval', 
 'Require executive approval for deals over threshold amount', 
 'Approvals', 'Opportunity', 1, 1, 'DollarSign', '#0ea5e9', 1, 1, 50, 72, 
 'sales,approval', NULL, NOW(), 0),

-- Notification Workflows
(17, 'customer-welcome', 'New Customer Welcome', 
 'Send welcome email and onboarding sequence when account becomes active', 
 'Notifications', 'Customer', 1, 1, 'Smile', '#22d3ee', 1, 2, 100, 24, 
 'customer,welcome,notification', NULL, NOW(), 0),

(18, 'task-due-reminder', 'Task Due Reminder', 
 'Remind assignee when task is due today', 
 'Notifications', 'Task', 1, 1, 'Bell', '#fbbf24', 1, 3, 1000, 24, 
 'task,reminder', NULL, NOW(), 0),

-- Data Management Workflows
(19, 'duplicate-detection', 'Duplicate Detection Alert', 
 'Alert when potential duplicate record is detected during creation', 
 'Data Quality', 'Lead', 1, 1, 'Copy', '#6366f1', 1, 2, 200, 4, 
 'data,duplicate', NULL, NOW(), 0),

(20, 'data-quality-check', 'Data Quality Check', 
 'Weekly check and flag records with missing required fields', 
 'Data Quality', 'Customer', 1, 1, 'CheckCircle', '#16a34a', 1, 3, 10, 168, 
 'data,quality', NULL, NOW(), 0)

ON DUPLICATE KEY UPDATE 
  Name = VALUES(Name), 
  Description = VALUES(Description),
  Category = VALUES(Category),
  Status = VALUES(Status),
  Tags = VALUES(Tags);

-- ============================================================================
-- WorkflowVersions - Version tracking for each workflow
-- Status: 1=Draft, 2=Published, 3=Deprecated
-- ============================================================================
INSERT INTO WorkflowVersions (
  Id, WorkflowDefinitionId, VersionNumber, Label, ChangeLog, Status,
  PublishedAt, CanvasLayout, CreatedAt, IsDeleted
) VALUES
(1, 1, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0),
(2, 2, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0),
(3, 3, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0),
(4, 4, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0),
(5, 5, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0),
(6, 6, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0),
(7, 7, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0),
(8, 8, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0),
(9, 9, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0),
(10, 10, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0),
(11, 11, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0),
(12, 12, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0),
(13, 13, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0),
(14, 14, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0),
(15, 15, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0),
(16, 16, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0),
(17, 17, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0),
(18, 18, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0),
(19, 19, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0),
(20, 20, 1, 'v1.0', 'Initial version', 2, NOW(), '{"zoom":1,"x":0,"y":0}', NOW(), 0)

ON DUPLICATE KEY UPDATE Label = VALUES(Label), Status = VALUES(Status);

-- ============================================================================
-- WorkflowNodes - Nodes for Lead Assignment Workflow (Version 1)
-- NodeType: 1=Start, 2=End, 3=Action, 4=Condition, 5=Wait, 6=Parallel, 7=Loop
-- ============================================================================
INSERT INTO WorkflowNodes (
  Id, WorkflowVersionId, NodeKey, Name, Description, NodeType, NodeSubType,
  PositionX, PositionY, Width, Height, IconName, Color, 
  IsStartNode, IsEndNode, Configuration, TimeoutMinutes, 
  RetryCount, RetryDelaySeconds, UseExponentialBackoff, ExecutionOrder, CreatedAt, IsDeleted
) VALUES
-- Workflow 1: Lead Assignment
(1, 1, 'start', 'Start', 'Workflow trigger on new lead', 1, 'trigger', 
 100, 100, 150, 60, 'Play', '#22c55e', 1, 0, 
 '{"trigger":"onCreate","entity":"Lead"}', 0, 0, 0, 0, 1, NOW(), 0),

(2, 1, 'check-territory', 'Check Territory', 'Evaluate lead location for territory assignment', 4, 'condition',
 300, 100, 180, 80, 'MapPin', '#3b82f6', 0, 0,
 '{"field":"State","rules":[{"values":["CA","OR","WA"],"output":"west"},{"values":["NY","NJ","MA","CT"],"output":"east"},{"default":"central"}]}', 
 5, 0, 0, 0, 2, NOW(), 0),

(3, 1, 'assign-west', 'Assign West Region', 'Round-robin assignment to west coast reps', 3, 'assign',
 500, 50, 180, 60, 'UserPlus', '#8b5cf6', 0, 0,
 '{"method":"roundRobin","group":"West Sales Team"}', 10, 3, 60, 1, 3, NOW(), 0),

(4, 1, 'assign-east', 'Assign East Region', 'Round-robin assignment to east coast reps', 3, 'assign',
 500, 150, 180, 60, 'UserPlus', '#8b5cf6', 0, 0,
 '{"method":"roundRobin","group":"East Sales Team"}', 10, 3, 60, 1, 4, NOW(), 0),

(5, 1, 'assign-central', 'Assign Central', 'Round-robin assignment to central reps', 3, 'assign',
 500, 250, 180, 60, 'UserPlus', '#8b5cf6', 0, 0,
 '{"method":"roundRobin","group":"Central Sales Team"}', 10, 3, 60, 1, 5, NOW(), 0),

(6, 1, 'create-task', 'Create Follow-up Task', 'Create initial contact task for new lead', 3, 'createRecord',
 700, 150, 180, 60, 'ClipboardList', '#f59e0b', 0, 0,
 '{"entity":"Task","template":{"Subject":"Initial Lead Contact - {{Lead.Name}}","DueIn":"24h","Priority":"High"}}', 
 5, 2, 30, 0, 6, NOW(), 0),

(7, 1, 'send-notification', 'Send Notification', 'Notify assigned rep of new lead', 3, 'notification',
 900, 150, 180, 60, 'Bell', '#06b6d4', 0, 0,
 '{"type":"email","template":"new_lead_assigned","to":"{{Lead.Owner.Email}}"}', 
 5, 3, 60, 1, 7, NOW(), 0),

(8, 1, 'end', 'End', 'Workflow complete', 2, 'complete',
 1100, 150, 120, 50, 'CheckCircle', '#22c55e', 0, 1,
 '{}', 0, 0, 0, 0, 8, NOW(), 0),

-- Workflow 5: Opportunity Stage Change (Version 5)
(20, 5, 'start', 'Start', 'Triggered on stage change', 1, 'trigger',
 100, 150, 150, 60, 'Play', '#22c55e', 1, 0,
 '{"trigger":"onUpdate","entity":"Opportunity","field":"Stage"}', 0, 0, 0, 0, 1, NOW(), 0),

(21, 5, 'check-won', 'Check If Won', 'Check if stage changed to Closed Won', 4, 'condition',
 300, 150, 180, 80, 'HelpCircle', '#8b5cf6', 0, 0,
 '{"field":"Stage","operator":"equals","value":"Closed Won"}', 5, 0, 0, 0, 2, NOW(), 0),

(22, 5, 'notify-win', 'Celebrate Win', 'Send celebration notification', 3, 'notification',
 550, 50, 180, 60, 'Trophy', '#22c55e', 0, 0,
 '{"type":"email","template":"deal_won","to":["{{Opportunity.Owner.Email}}","{{Opportunity.Owner.Manager.Email}}"]}', 
 5, 2, 60, 0, 3, NOW(), 0),

(23, 5, 'check-lost', 'Check If Lost', 'Check if stage changed to Closed Lost', 4, 'condition',
 550, 250, 180, 80, 'HelpCircle', '#ef4444', 0, 0,
 '{"field":"Stage","operator":"equals","value":"Closed Lost"}', 5, 0, 0, 0, 4, NOW(), 0),

(24, 5, 'notify-loss', 'Notify Loss', 'Notify manager for loss review', 3, 'notification',
 750, 250, 180, 60, 'AlertTriangle', '#ef4444', 0, 0,
 '{"type":"email","template":"deal_lost_review","to":"{{Opportunity.Owner.Manager.Email}}"}', 
 5, 2, 60, 0, 5, NOW(), 0),

(25, 5, 'log-activity', 'Log Stage Change', 'Record stage transition', 3, 'logActivity',
 750, 150, 180, 60, 'FileText', '#6366f1', 0, 0,
 '{"message":"Stage changed to {{Opportunity.Stage}}"}', 3, 1, 30, 0, 6, NOW(), 0),

(26, 5, 'end', 'End', 'Workflow complete', 2, 'complete',
 950, 150, 120, 50, 'CheckCircle', '#22c55e', 0, 1,
 '{}', 0, 0, 0, 0, 7, NOW(), 0),

-- Workflow 9: Ticket Assignment (Version 9)
(40, 9, 'start', 'Start', 'New service request created', 1, 'trigger',
 100, 150, 150, 60, 'Play', '#22c55e', 1, 0,
 '{"trigger":"onCreate","entity":"ServiceRequest"}', 0, 0, 0, 0, 1, NOW(), 0),

(41, 9, 'check-priority', 'Check Priority', 'Route based on ticket priority', 4, 'condition',
 300, 150, 180, 80, 'Flag', '#f59e0b', 0, 0,
 '{"field":"Priority","rules":[{"values":["Critical","High"],"output":"urgent"},{"default":"standard"}]}', 
 5, 0, 0, 0, 2, NOW(), 0),

(42, 9, 'assign-urgent', 'Assign Urgent Queue', 'Assign to senior support team', 3, 'assign',
 550, 50, 180, 60, 'Zap', '#ef4444', 0, 0,
 '{"method":"leastBusy","group":"Senior Support Team","priority":"high"}', 5, 3, 30, 1, 3, NOW(), 0),

(43, 9, 'assign-standard', 'Assign Standard Queue', 'Round-robin to support team', 3, 'assign',
 550, 250, 180, 60, 'Users', '#3b82f6', 0, 0,
 '{"method":"roundRobin","group":"Support Team"}', 10, 2, 60, 0, 4, NOW(), 0),

(44, 9, 'send-ack', 'Send Acknowledgment', 'Email customer with ticket number', 3, 'notification',
 750, 150, 180, 60, 'Mail', '#14b8a6', 0, 0,
 '{"type":"email","template":"ticket_received","to":"{{ServiceRequest.CustomerEmail}}"}', 
 5, 3, 60, 1, 5, NOW(), 0),

(45, 9, 'set-sla', 'Set SLA Timer', 'Calculate and set SLA due date', 3, 'updateField',
 950, 150, 180, 60, 'Clock', '#8b5cf6', 0, 0,
 '{"entity":"ServiceRequest","field":"SLADueDate","value":"{{CalculatedSLA}}"}', 
 3, 1, 30, 0, 6, NOW(), 0),

(46, 9, 'end', 'End', 'Assignment complete', 2, 'complete',
 1150, 150, 120, 50, 'CheckCircle', '#22c55e', 0, 1,
 '{}', 0, 0, 0, 0, 7, NOW(), 0)

ON DUPLICATE KEY UPDATE Name = VALUES(Name), Configuration = VALUES(Configuration);

-- ============================================================================
-- WorkflowTransitions - Connections between nodes
-- ConditionType: 0=Always, 1=Expression, 2=Manual
-- ============================================================================
INSERT INTO WorkflowTransitions (
  Id, WorkflowVersionId, SourceNodeId, TargetNodeId, TransitionKey, Label, Description,
  ConditionType, ConditionExpression, IsDefault, Priority, 
  SourceHandle, TargetHandle, LineStyle, Color, AnimationStyle, CreatedAt, IsDeleted
) VALUES
-- Workflow 1: Lead Assignment Transitions
(1, 1, 1, 2, 't1', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(2, 1, 2, 3, 't2-west', 'West', NULL, 1, '{"output":"west"}', 0, 1, 'right', 'left', 'smooth', '#8b5cf6', 'flow', NOW(), 0),
(3, 1, 2, 4, 't2-east', 'East', NULL, 1, '{"output":"east"}', 0, 2, 'right', 'left', 'smooth', '#8b5cf6', 'flow', NOW(), 0),
(4, 1, 2, 5, 't2-central', 'Central', NULL, 1, '{"output":"central"}', 1, 3, 'bottom', 'left', 'smooth', '#8b5cf6', 'flow', NOW(), 0),
(5, 1, 3, 6, 't3', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(6, 1, 4, 6, 't4', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(7, 1, 5, 6, 't5', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(8, 1, 6, 7, 't6', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(9, 1, 7, 8, 't7', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),

-- Workflow 5: Opportunity Stage Change Transitions
(20, 5, 20, 21, 't20', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(21, 5, 21, 22, 't21-yes', 'Won', 'Stage is Closed Won', 1, '{"result":true}', 0, 1, 'top', 'left', 'smooth', '#22c55e', 'flow', NOW(), 0),
(22, 5, 21, 23, 't21-no', 'Not Won', NULL, 1, '{"result":false}', 1, 2, 'bottom', 'left', 'smooth', '#ef4444', 'none', NOW(), 0),
(23, 5, 22, 25, 't22', NULL, NULL, 0, NULL, 1, 1, 'right', 'top', 'smooth', '#64748b', 'none', NOW(), 0),
(24, 5, 23, 24, 't23-yes', 'Lost', 'Stage is Closed Lost', 1, '{"result":true}', 0, 1, 'right', 'left', 'smooth', '#ef4444', 'flow', NOW(), 0),
(25, 5, 23, 25, 't23-no', 'Active', NULL, 1, '{"result":false}', 1, 2, 'right', 'bottom', 'smooth', '#3b82f6', 'none', NOW(), 0),
(26, 5, 24, 25, 't24', NULL, NULL, 0, NULL, 1, 1, 'right', 'bottom', 'smooth', '#64748b', 'none', NOW(), 0),
(27, 5, 25, 26, 't25', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),

-- Workflow 9: Ticket Assignment Transitions
(40, 9, 40, 41, 't40', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(41, 9, 41, 42, 't41-urgent', 'Urgent', 'High/Critical priority', 1, '{"output":"urgent"}', 0, 1, 'top', 'left', 'smooth', '#ef4444', 'flow', NOW(), 0),
(42, 9, 41, 43, 't41-std', 'Standard', NULL, 1, '{"output":"standard"}', 1, 2, 'bottom', 'left', 'smooth', '#3b82f6', 'none', NOW(), 0),
(43, 9, 42, 44, 't42', NULL, NULL, 0, NULL, 1, 1, 'right', 'top', 'smooth', '#64748b', 'none', NOW(), 0),
(44, 9, 43, 44, 't43', NULL, NULL, 0, NULL, 1, 1, 'right', 'bottom', 'smooth', '#64748b', 'none', NOW(), 0),
(45, 9, 44, 45, 't44', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(46, 9, 45, 46, 't45', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0)

ON DUPLICATE KEY UPDATE Label = VALUES(Label), ConditionExpression = VALUES(ConditionExpression);

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================================
-- Summary:
-- - 20 WorkflowDefinitions covering all major CRM processes
-- - 20 WorkflowVersions (v1.0 for each workflow)
-- - Sample nodes for 3 key workflows (Lead Assignment, Opp Stage, Ticket Assignment)
-- - Sample transitions demonstrating various routing patterns
-- ============================================================================

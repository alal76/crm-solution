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
 '{}', 0, 0, 0, 0, 7, NOW(), 0),

-- ============================================================================
-- Workflow 2: Lead Follow-up Reminder (Version 2)
-- ============================================================================
(50, 2, 'start', 'Start', 'Scheduled daily check', 1, 'trigger',
 100, 150, 150, 60, 'Play', '#22c55e', 1, 0,
 '{"trigger":"scheduled","schedule":"0 9 * * *"}', 0, 0, 0, 0, 1, NOW(), 0),

(51, 2, 'query-leads', 'Find Stale Leads', 'Query leads without activity in 3 days', 3, 'query',
 300, 150, 180, 60, 'Search', '#3b82f6', 0, 0,
 '{"entity":"Lead","filter":"LastActivityDate < NOW() - INTERVAL 3 DAY AND Status IN (''New'',''Working'')"}', 
 30, 2, 60, 0, 2, NOW(), 0),

(52, 2, 'loop-leads', 'For Each Lead', 'Process each stale lead', 7, 'forEach',
 500, 150, 150, 60, 'Repeat', '#8b5cf6', 0, 0,
 '{"collection":"{{QueryResults}}"}', 0, 0, 0, 0, 3, NOW(), 0),

(53, 2, 'create-reminder', 'Create Reminder Task', 'Create follow-up task for owner', 3, 'createRecord',
 700, 150, 180, 60, 'ClipboardList', '#f59e0b', 0, 0,
 '{"entity":"Task","template":{"Subject":"Follow up: {{Lead.Name}}","DueDate":"TODAY","Priority":"High","AssignedTo":"{{Lead.OwnerId}}"}}', 
 10, 2, 30, 0, 4, NOW(), 0),

(54, 2, 'send-reminder', 'Send Email Reminder', 'Email owner about stale lead', 3, 'notification',
 900, 150, 180, 60, 'Mail', '#06b6d4', 0, 0,
 '{"type":"email","template":"lead_followup_reminder","to":"{{Lead.Owner.Email}}"}', 
 5, 3, 60, 1, 5, NOW(), 0),

(55, 2, 'end', 'End', 'Workflow complete', 2, 'complete',
 1100, 150, 120, 50, 'CheckCircle', '#22c55e', 0, 1,
 '{}', 0, 0, 0, 0, 6, NOW(), 0),

-- ============================================================================
-- Workflow 3: Lead Qualification Scoring (Version 3)
-- ============================================================================
(60, 3, 'start', 'Start', 'Triggered on lead activity', 1, 'trigger',
 100, 150, 150, 60, 'Play', '#22c55e', 1, 0,
 '{"trigger":"onUpdate","entity":"Lead","fields":["Email","Phone","Website","Company"]}', 0, 0, 0, 0, 1, NOW(), 0),

(61, 3, 'calc-score', 'Calculate Score', 'Calculate lead score based on data completeness', 3, 'script',
 300, 150, 180, 70, 'Calculator', '#8b5cf6', 0, 0,
 '{"script":"let score = 0; if(Lead.Email) score += 20; if(Lead.Phone) score += 15; if(Lead.Company) score += 25; if(Lead.Website) score += 10; if(Lead.Title) score += 10; if(Lead.Industry) score += 10; if(Lead.AnnualRevenue > 1000000) score += 10; return score;"}', 
 10, 1, 30, 0, 2, NOW(), 0),

(62, 3, 'update-score', 'Update Lead Score', 'Save calculated score to lead', 3, 'updateField',
 500, 150, 180, 60, 'TrendingUp', '#10b981', 0, 0,
 '{"entity":"Lead","field":"LeadScore","value":"{{CalculatedScore}}"}', 
 5, 2, 30, 0, 3, NOW(), 0),

(63, 3, 'check-threshold', 'Check Score Threshold', 'Is lead score >= 80?', 4, 'condition',
 700, 150, 180, 80, 'Target', '#f59e0b', 0, 0,
 '{"field":"LeadScore","operator":"gte","value":80}', 5, 0, 0, 0, 4, NOW(), 0),

(64, 3, 'mark-qualified', 'Mark as Qualified', 'Update status to Qualified', 3, 'updateField',
 900, 80, 180, 60, 'CheckCircle', '#22c55e', 0, 0,
 '{"entity":"Lead","field":"Status","value":"Qualified"}', 5, 1, 30, 0, 5, NOW(), 0),

(65, 3, 'end', 'End', 'Scoring complete', 2, 'complete',
 1100, 150, 120, 50, 'CheckCircle', '#22c55e', 0, 1,
 '{}', 0, 0, 0, 0, 6, NOW(), 0),

-- ============================================================================
-- Workflow 4: Hot Lead Alert (Version 4)
-- ============================================================================
(70, 4, 'start', 'Start', 'Triggered on lead score update', 1, 'trigger',
 100, 150, 150, 60, 'Play', '#22c55e', 1, 0,
 '{"trigger":"onUpdate","entity":"Lead","field":"LeadScore"}', 0, 0, 0, 0, 1, NOW(), 0),

(71, 4, 'check-hot', 'Is Hot Lead?', 'Score >= 90?', 4, 'condition',
 300, 150, 180, 80, 'Flame', '#ef4444', 0, 0,
 '{"field":"LeadScore","operator":"gte","value":90}', 5, 0, 0, 0, 2, NOW(), 0),

(72, 4, 'notify-manager', 'Notify Sales Manager', 'Send priority alert', 3, 'notification',
 500, 80, 200, 60, 'AlertCircle', '#ef4444', 0, 0,
 '{"type":"email","template":"hot_lead_alert","to":["{{Lead.Owner.Manager.Email}}","sales-managers@company.com"],"priority":"high"}', 
 5, 3, 60, 1, 3, NOW(), 0),

(73, 4, 'create-urgent-task', 'Create Urgent Task', 'High priority follow-up', 3, 'createRecord',
 700, 80, 180, 60, 'Zap', '#f59e0b', 0, 0,
 '{"entity":"Task","template":{"Subject":"HOT LEAD: {{Lead.Name}} - Immediate Follow-up Required","DueDate":"TODAY","Priority":"Critical","AssignedTo":"{{Lead.OwnerId}}"}}', 
 5, 2, 30, 0, 4, NOW(), 0),

(74, 4, 'end', 'End', 'Alert complete', 2, 'complete',
 900, 150, 120, 50, 'CheckCircle', '#22c55e', 0, 1,
 '{}', 0, 0, 0, 0, 5, NOW(), 0),

-- ============================================================================
-- Workflow 8: Won Opportunity Processing (Version 8)
-- ============================================================================
(80, 8, 'start', 'Start', 'Triggered on opportunity won', 1, 'trigger',
 100, 150, 150, 60, 'Play', '#22c55e', 1, 0,
 '{"trigger":"onUpdate","entity":"Opportunity","field":"Stage","newValue":"Closed Won"}', 0, 0, 0, 0, 1, NOW(), 0),

(81, 8, 'update-customer', 'Update Customer Status', 'Mark customer as active', 3, 'updateField',
 300, 150, 180, 60, 'UserCheck', '#10b981', 0, 0,
 '{"entity":"Customer","id":"{{Opportunity.CustomerId}}","field":"Status","value":"Active"}', 
 10, 2, 30, 0, 2, NOW(), 0),

(82, 8, 'create-onboarding', 'Create Onboarding Task', 'Initiate customer onboarding', 3, 'createRecord',
 500, 80, 180, 60, 'Rocket', '#8b5cf6', 0, 0,
 '{"entity":"Task","template":{"Subject":"Customer Onboarding: {{Customer.Name}}","DueIn":"7d","Priority":"High","Type":"Onboarding"}}', 
 10, 2, 30, 0, 3, NOW(), 0),

(83, 8, 'send-welcome', 'Send Welcome Email', 'Welcome new customer', 3, 'notification',
 500, 220, 180, 60, 'Mail', '#06b6d4', 0, 0,
 '{"type":"email","template":"customer_welcome","to":"{{Customer.Email}}"}', 
 5, 3, 60, 1, 4, NOW(), 0),

(84, 8, 'notify-success', 'Celebrate in Slack', 'Post to sales channel', 3, 'webhook',
 700, 150, 180, 60, 'MessageSquare', '#7c3aed', 0, 0,
 '{"url":"{{SlackWebhookUrl}}","method":"POST","body":{"text":"🎉 New deal won! {{Opportunity.Name}} - ${{Opportunity.Amount}}"}}', 
 10, 2, 60, 0, 5, NOW(), 0),

(85, 8, 'update-forecast', 'Update Revenue Forecast', 'Add to closed revenue', 3, 'script',
 900, 150, 180, 60, 'DollarSign', '#22c55e', 0, 0,
 '{"script":"updateMonthlyForecast(Opportunity.CloseDate, Opportunity.Amount)"}', 
 10, 1, 30, 0, 6, NOW(), 0),

(86, 8, 'end', 'End', 'Processing complete', 2, 'complete',
 1100, 150, 120, 50, 'CheckCircle', '#22c55e', 0, 1,
 '{}', 0, 0, 0, 0, 7, NOW(), 0),

-- ============================================================================
-- Workflow 15: Quote Approval Workflow (Version 15)
-- ============================================================================
(100, 15, 'start', 'Start', 'Quote submitted for approval', 1, 'trigger',
 100, 150, 150, 60, 'Play', '#22c55e', 1, 0,
 '{"trigger":"onUpdate","entity":"Quote","field":"Status","newValue":"Pending Approval"}', 0, 0, 0, 0, 1, NOW(), 0),

(101, 15, 'check-discount', 'Check Discount Level', 'Evaluate discount percentage', 4, 'condition',
 300, 150, 180, 80, 'Percent', '#f59e0b', 0, 0,
 '{"field":"DiscountPercent","rules":[{"operator":"lte","value":10,"output":"auto"},{"operator":"lte","value":25,"output":"manager"},{"default":"director"}]}', 
 5, 0, 0, 0, 2, NOW(), 0),

(102, 15, 'auto-approve', 'Auto Approve', 'Approve quotes with <= 10% discount', 3, 'updateField',
 550, 50, 180, 60, 'CheckCircle', '#22c55e', 0, 0,
 '{"entity":"Quote","fields":{"Status":"Approved","ApprovedBy":"System","ApprovedAt":"NOW()"}}', 
 5, 1, 30, 0, 3, NOW(), 0),

(103, 15, 'manager-approval', 'Manager Approval', 'Request manager approval', 6, 'humanTask',
 550, 150, 180, 70, 'UserCheck', '#3b82f6', 0, 0,
 '{"taskType":"approval","title":"Quote Approval Required","assignTo":"{{Quote.Owner.Manager}}","dueIn":"48h","options":["Approve","Reject","Request Changes"]}', 
 2880, 1, 0, 0, 4, NOW(), 0),

(104, 15, 'director-approval', 'Director Approval', 'Request director approval for high discounts', 6, 'humanTask',
 550, 280, 180, 70, 'Shield', '#8b5cf6', 0, 0,
 '{"taskType":"approval","title":"High Discount Quote - Director Approval","assignTo":"sales-director","dueIn":"72h","options":["Approve","Reject"]}', 
 4320, 1, 0, 0, 5, NOW(), 0),

(105, 15, 'process-decision', 'Process Decision', 'Handle approval decision', 4, 'condition',
 800, 150, 180, 80, 'GitBranch', '#6366f1', 0, 0,
 '{"field":"ApprovalDecision","rules":[{"value":"Approve","output":"approved"},{"value":"Reject","output":"rejected"},{"default":"changes"}]}', 
 5, 0, 0, 0, 6, NOW(), 0),

(106, 15, 'mark-approved', 'Mark Approved', 'Update quote status to approved', 3, 'updateField',
 1000, 50, 180, 60, 'ThumbsUp', '#22c55e', 0, 0,
 '{"entity":"Quote","field":"Status","value":"Approved"}', 5, 1, 30, 0, 7, NOW(), 0),

(107, 15, 'mark-rejected', 'Mark Rejected', 'Update quote status to rejected', 3, 'updateField',
 1000, 150, 180, 60, 'ThumbsDown', '#ef4444', 0, 0,
 '{"entity":"Quote","field":"Status","value":"Rejected"}', 5, 1, 30, 0, 8, NOW(), 0),

(108, 15, 'request-changes', 'Request Changes', 'Send back to owner for revision', 3, 'notification',
 1000, 250, 180, 60, 'Edit', '#f59e0b', 0, 0,
 '{"type":"email","template":"quote_changes_requested","to":"{{Quote.Owner.Email}}"}', 5, 2, 60, 0, 9, NOW(), 0),

(109, 15, 'end', 'End', 'Approval workflow complete', 2, 'complete',
 1200, 150, 120, 50, 'CheckCircle', '#22c55e', 0, 1,
 '{}', 0, 0, 0, 0, 10, NOW(), 0),

-- ============================================================================
-- Workflow 17: New Customer Welcome (Version 17)
-- ============================================================================
(120, 17, 'start', 'Start', 'New customer created', 1, 'trigger',
 100, 150, 150, 60, 'Play', '#22c55e', 1, 0,
 '{"trigger":"onCreate","entity":"Customer"}', 0, 0, 0, 0, 1, NOW(), 0),

(121, 17, 'send-welcome', 'Send Welcome Email', 'Welcome email with onboarding info', 3, 'notification',
 300, 150, 180, 60, 'Mail', '#06b6d4', 0, 0,
 '{"type":"email","template":"customer_welcome","to":"{{Customer.Email}}"}', 5, 3, 60, 1, 2, NOW(), 0),

(122, 17, 'wait-1day', 'Wait 1 Day', 'Delay before next communication', 5, 'wait',
 500, 150, 150, 60, 'Clock', '#8b5cf6', 0, 0,
 '{"duration":"24h"}', 1440, 0, 0, 0, 3, NOW(), 0),

(123, 17, 'send-tips', 'Send Tips Email', 'Getting started tips', 3, 'notification',
 700, 150, 180, 60, 'Lightbulb', '#f59e0b', 0, 0,
 '{"type":"email","template":"customer_tips","to":"{{Customer.Email}}"}', 5, 3, 60, 1, 4, NOW(), 0),

(124, 17, 'wait-3days', 'Wait 3 Days', 'Delay before check-in', 5, 'wait',
 900, 150, 150, 60, 'Clock', '#8b5cf6', 0, 0,
 '{"duration":"72h"}', 4320, 0, 0, 0, 5, NOW(), 0),

(125, 17, 'create-checkin', 'Create Check-in Task', 'Schedule account manager check-in', 3, 'createRecord',
 1100, 150, 180, 60, 'Phone', '#10b981', 0, 0,
 '{"entity":"Task","template":{"Subject":"New Customer Check-in: {{Customer.Name}}","DueDate":"TODAY","Priority":"Medium","Type":"Call"}}', 
 10, 2, 30, 0, 6, NOW(), 0),

(126, 17, 'end', 'End', 'Onboarding sequence complete', 2, 'complete',
 1300, 150, 120, 50, 'CheckCircle', '#22c55e', 0, 1,
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
(46, 9, 45, 46, 't45', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),

-- Workflow 2: Lead Follow-up Reminder Transitions
(50, 2, 50, 51, 't50', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(51, 2, 51, 52, 't51', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(52, 2, 52, 53, 't52', 'Each Lead', NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#8b5cf6', 'flow', NOW(), 0),
(53, 2, 53, 54, 't53', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(54, 2, 54, 52, 't54-loop', 'Next', 'Continue loop', 0, NULL, 0, 2, 'bottom', 'bottom', 'smooth', '#8b5cf6', 'none', NOW(), 0),
(55, 2, 52, 55, 't52-end', 'Done', 'Loop complete', 0, '{"loopComplete":true}', 1, 1, 'right', 'left', 'smooth', '#22c55e', 'none', NOW(), 0),

-- Workflow 3: Lead Qualification Scoring Transitions
(60, 3, 60, 61, 't60', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(61, 3, 61, 62, 't61', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(62, 3, 62, 63, 't62', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(63, 3, 63, 64, 't63-yes', 'Qualified', 'Score >= 80', 1, '{"result":true}', 0, 1, 'top', 'left', 'smooth', '#22c55e', 'flow', NOW(), 0),
(64, 3, 63, 65, 't63-no', 'Not Yet', 'Score < 80', 1, '{"result":false}', 1, 2, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(65, 3, 64, 65, 't64', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),

-- Workflow 4: Hot Lead Alert Transitions
(70, 4, 70, 71, 't70', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(71, 4, 71, 72, 't71-yes', 'Hot!', 'Score >= 90', 1, '{"result":true}', 0, 1, 'top', 'left', 'smooth', '#ef4444', 'flow', NOW(), 0),
(72, 4, 71, 74, 't71-no', 'Not Hot', 'Score < 90', 1, '{"result":false}', 1, 2, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(73, 4, 72, 73, 't72', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(74, 4, 73, 74, 't73', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),

-- Workflow 8: Won Opportunity Processing Transitions
(80, 8, 80, 81, 't80', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(81, 8, 81, 82, 't81-a', NULL, NULL, 0, NULL, 1, 1, 'right', 'top', 'smooth', '#64748b', 'none', NOW(), 0),
(82, 8, 81, 83, 't81-b', NULL, 'Parallel: send welcome', 0, NULL, 0, 2, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(83, 8, 82, 84, 't82', NULL, NULL, 0, NULL, 1, 1, 'right', 'top', 'smooth', '#64748b', 'none', NOW(), 0),
(84, 8, 83, 84, 't83', NULL, NULL, 0, NULL, 1, 1, 'right', 'bottom', 'smooth', '#64748b', 'none', NOW(), 0),
(85, 8, 84, 85, 't84', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(86, 8, 85, 86, 't85', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),

-- Workflow 15: Quote Approval Transitions
(100, 15, 100, 101, 't100', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(101, 15, 101, 102, 't101-auto', '≤10%', 'Auto approve', 1, '{"output":"auto"}', 0, 1, 'top', 'left', 'smooth', '#22c55e', 'flow', NOW(), 0),
(102, 15, 101, 103, 't101-mgr', '11-25%', 'Manager approval', 1, '{"output":"manager"}', 0, 2, 'right', 'left', 'smooth', '#3b82f6', 'flow', NOW(), 0),
(103, 15, 101, 104, 't101-dir', '>25%', 'Director approval', 1, '{"output":"director"}', 0, 3, 'bottom', 'left', 'smooth', '#8b5cf6', 'flow', NOW(), 0),
(104, 15, 102, 109, 't102', NULL, NULL, 0, NULL, 1, 1, 'right', 'top', 'smooth', '#64748b', 'none', NOW(), 0),
(105, 15, 103, 105, 't103', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(106, 15, 104, 105, 't104', NULL, NULL, 0, NULL, 1, 1, 'right', 'bottom', 'smooth', '#64748b', 'none', NOW(), 0),
(107, 15, 105, 106, 't105-approve', 'Approved', NULL, 1, '{"output":"approved"}', 0, 1, 'top', 'left', 'smooth', '#22c55e', 'flow', NOW(), 0),
(108, 15, 105, 107, 't105-reject', 'Rejected', NULL, 1, '{"output":"rejected"}', 0, 2, 'right', 'left', 'smooth', '#ef4444', 'flow', NOW(), 0),
(109, 15, 105, 108, 't105-changes', 'Changes', NULL, 1, '{"output":"changes"}', 0, 3, 'bottom', 'left', 'smooth', '#f59e0b', 'flow', NOW(), 0),
(110, 15, 106, 109, 't106', NULL, NULL, 0, NULL, 1, 1, 'right', 'top', 'smooth', '#64748b', 'none', NOW(), 0),
(111, 15, 107, 109, 't107', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(112, 15, 108, 109, 't108', NULL, NULL, 0, NULL, 1, 1, 'right', 'bottom', 'smooth', '#64748b', 'none', NOW(), 0),

-- Workflow 17: Customer Welcome Transitions
(120, 17, 120, 121, 't120', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(121, 17, 121, 122, 't121', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(122, 17, 122, 123, 't122', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(123, 17, 123, 124, 't123', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(124, 17, 124, 125, 't124', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0),
(125, 17, 125, 126, 't125', NULL, NULL, 0, NULL, 1, 1, 'right', 'left', 'smooth', '#64748b', 'none', NOW(), 0)

ON DUPLICATE KEY UPDATE Label = VALUES(Label), ConditionExpression = VALUES(ConditionExpression);

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================================
-- Summary:
-- - 20 WorkflowDefinitions covering all major CRM processes
-- - 20 WorkflowVersions (v1.0 for each workflow)
-- - Detailed nodes for 9 key workflows:
--   * Lead Assignment (8 nodes) - Territory-based assignment with notifications
--   * Lead Follow-up Reminder (6 nodes) - Scheduled check with loop processing  
--   * Lead Qualification Scoring (6 nodes) - Score calculation and threshold check
--   * Hot Lead Alert (5 nodes) - High-score notification and urgent task
--   * Opportunity Stage Change (7 nodes) - Win/loss notification routing
--   * Won Opportunity Processing (7 nodes) - Post-sale automation sequence
--   * Ticket Assignment (7 nodes) - Priority-based queue assignment
--   * Quote Approval (10 nodes) - Multi-tier approval workflow
--   * Customer Welcome (7 nodes) - Onboarding drip sequence with delays
-- - 70+ transitions demonstrating various routing patterns
-- ============================================================================

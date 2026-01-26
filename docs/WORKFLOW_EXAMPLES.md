# Workflow Module - Example Lifecycle Workflows

This document describes example workflow templates for common CRM lifecycle processes.

## 1. Customer Onboarding Workflow

**Trigger:** New customer created

```json
{
  "workflowKey": "customer-onboarding",
  "name": "Customer Onboarding",
  "description": "Automated customer onboarding process with welcome email, data validation, and account setup",
  "entityType": "Customer",
  "category": "Lifecycle",
  "nodes": [
    {
      "nodeKey": "trigger-new-customer",
      "name": "New Customer Created",
      "nodeType": "Trigger",
      "configuration": {
        "triggerType": "EntityCreated",
        "entityType": "Customer"
      }
    },
    {
      "nodeKey": "validate-data",
      "name": "Validate Customer Data",
      "nodeType": "Action",
      "configuration": {
        "actionType": "validateEntity",
        "requiredFields": ["name", "email", "phone"]
      }
    },
    {
      "nodeKey": "check-validation",
      "name": "Data Valid?",
      "nodeType": "Condition",
      "configuration": {}
    },
    {
      "nodeKey": "send-welcome-email",
      "name": "Send Welcome Email",
      "nodeType": "Action",
      "configuration": {
        "actionType": "sendEmail",
        "template": "customer-welcome",
        "to": "{{customer.email}}"
      }
    },
    {
      "nodeKey": "create-account",
      "name": "Create Customer Account",
      "nodeType": "Action",
      "configuration": {
        "actionType": "createEntity",
        "entityType": "Account",
        "fields": {
          "name": "{{customer.name}}",
          "customerId": "{{customer.id}}"
        }
      }
    },
    {
      "nodeKey": "assign-manager",
      "name": "Assign Account Manager",
      "nodeType": "HumanTask",
      "configuration": {
        "title": "Assign Account Manager",
        "description": "Review and assign an account manager for new customer: {{customer.name}}",
        "assignToRole": "SalesManager",
        "formSchema": {
          "fields": [
            {
              "name": "accountManagerId",
              "label": "Account Manager",
              "type": "user-select",
              "required": true,
              "filter": { "role": "SalesRep" }
            },
            {
              "name": "notes",
              "label": "Assignment Notes",
              "type": "textarea"
            }
          ]
        }
      }
    },
    {
      "nodeKey": "notify-manager",
      "name": "Notify Account Manager",
      "nodeType": "Action",
      "configuration": {
        "actionType": "sendNotification",
        "to": "{{assignedManager}}",
        "message": "You have been assigned as account manager for {{customer.name}}"
      }
    },
    {
      "nodeKey": "schedule-followup",
      "name": "Wait 7 Days",
      "nodeType": "Wait",
      "configuration": {
        "waitMinutes": 10080
      }
    },
    {
      "nodeKey": "send-followup",
      "name": "Send Follow-up Email",
      "nodeType": "Action",
      "configuration": {
        "actionType": "sendEmail",
        "template": "customer-followup",
        "to": "{{customer.email}}"
      }
    },
    {
      "nodeKey": "end-success",
      "name": "Onboarding Complete",
      "nodeType": "End",
      "configuration": {
        "status": "success"
      }
    },
    {
      "nodeKey": "end-invalid-data",
      "name": "Data Validation Failed",
      "nodeType": "End",
      "configuration": {
        "status": "failed",
        "reason": "Invalid customer data"
      }
    }
  ],
  "transitions": [
    { "source": "trigger-new-customer", "target": "validate-data" },
    { "source": "validate-data", "target": "check-validation" },
    { "source": "check-validation", "target": "send-welcome-email", "condition": "isValid == true" },
    { "source": "check-validation", "target": "end-invalid-data", "condition": "isValid == false" },
    { "source": "send-welcome-email", "target": "create-account" },
    { "source": "create-account", "target": "assign-manager" },
    { "source": "assign-manager", "target": "notify-manager" },
    { "source": "notify-manager", "target": "schedule-followup" },
    { "source": "schedule-followup", "target": "send-followup" },
    { "source": "send-followup", "target": "end-success" }
  ]
}
```

## 2. Lead Qualification Workflow with AI

**Trigger:** New lead captured

```json
{
  "workflowKey": "lead-qualification-ai",
  "name": "AI-Powered Lead Qualification",
  "description": "Qualify leads using AI scoring and automated nurturing",
  "entityType": "Lead",
  "category": "Sales",
  "nodes": [
    {
      "nodeKey": "trigger-new-lead",
      "name": "New Lead Captured",
      "nodeType": "Trigger",
      "configuration": {
        "triggerType": "EntityCreated",
        "entityType": "Lead"
      }
    },
    {
      "nodeKey": "enrich-data",
      "name": "Enrich Lead Data",
      "nodeType": "Integration",
      "configuration": {
        "integrationType": "Clearbit",
        "action": "enrich",
        "email": "{{lead.email}}"
      }
    },
    {
      "nodeKey": "ai-score",
      "name": "AI Lead Scoring",
      "nodeType": "LLMAction",
      "configuration": {
        "model": "gpt-4",
        "prompt": "Analyze this lead and provide a qualification score (1-100) based on the following data:\n\nCompany: {{lead.company}}\nTitle: {{lead.title}}\nIndustry: {{enrichedData.industry}}\nCompany Size: {{enrichedData.companySize}}\nFunding: {{enrichedData.funding}}\n\nRespond with JSON: { \"score\": number, \"reasoning\": string, \"recommendedAction\": string }",
        "outputField": "aiQualification"
      }
    },
    {
      "nodeKey": "check-score",
      "name": "Evaluate Score",
      "nodeType": "Condition",
      "configuration": {}
    },
    {
      "nodeKey": "high-priority-assignment",
      "name": "Assign to Sales Rep",
      "nodeType": "Action",
      "configuration": {
        "actionType": "roundRobinAssign",
        "pool": "SalesReps",
        "priority": "high"
      }
    },
    {
      "nodeKey": "schedule-call",
      "name": "Schedule Qualification Call",
      "nodeType": "HumanTask",
      "configuration": {
        "title": "Schedule call with high-value lead",
        "description": "AI Score: {{aiQualification.score}}\nReasoning: {{aiQualification.reasoning}}",
        "formSchema": {
          "fields": [
            { "name": "callDateTime", "label": "Call Date/Time", "type": "datetime", "required": true },
            { "name": "notes", "label": "Preparation Notes", "type": "textarea" }
          ]
        }
      }
    },
    {
      "nodeKey": "add-to-nurture",
      "name": "Add to Nurture Campaign",
      "nodeType": "Action",
      "configuration": {
        "actionType": "addToCampaign",
        "campaign": "lead-nurture-sequence",
        "segment": "medium-potential"
      }
    },
    {
      "nodeKey": "send-resources",
      "name": "Send Educational Resources",
      "nodeType": "Action",
      "configuration": {
        "actionType": "sendEmail",
        "template": "lead-resources",
        "to": "{{lead.email}}"
      }
    },
    {
      "nodeKey": "wait-engagement",
      "name": "Wait for Engagement",
      "nodeType": "Wait",
      "configuration": {
        "waitMinutes": 4320,
        "exitCondition": "lead.emailOpened == true"
      }
    },
    {
      "nodeKey": "archive-lead",
      "name": "Archive Low-Quality Lead",
      "nodeType": "Action",
      "configuration": {
        "actionType": "updateEntity",
        "entityType": "Lead",
        "updates": {
          "status": "Archived",
          "archivedReason": "Low AI qualification score"
        }
      }
    },
    {
      "nodeKey": "end-qualified",
      "name": "Lead Qualified",
      "nodeType": "End"
    },
    {
      "nodeKey": "end-nurturing",
      "name": "In Nurture Campaign",
      "nodeType": "End"
    },
    {
      "nodeKey": "end-archived",
      "name": "Lead Archived",
      "nodeType": "End"
    }
  ],
  "transitions": [
    { "source": "trigger-new-lead", "target": "enrich-data" },
    { "source": "enrich-data", "target": "ai-score" },
    { "source": "ai-score", "target": "check-score" },
    { "source": "check-score", "target": "high-priority-assignment", "condition": "aiQualification.score >= 70", "label": "High Quality" },
    { "source": "check-score", "target": "add-to-nurture", "condition": "aiQualification.score >= 40 && aiQualification.score < 70", "label": "Medium" },
    { "source": "check-score", "target": "archive-lead", "condition": "aiQualification.score < 40", "label": "Low Quality" },
    { "source": "high-priority-assignment", "target": "schedule-call" },
    { "source": "schedule-call", "target": "end-qualified" },
    { "source": "add-to-nurture", "target": "send-resources" },
    { "source": "send-resources", "target": "wait-engagement" },
    { "source": "wait-engagement", "target": "end-nurturing" },
    { "source": "archive-lead", "target": "end-archived" }
  ]
}
```

## 3. Campaign Execution Workflow

**Trigger:** Campaign activated

```json
{
  "workflowKey": "campaign-execution",
  "name": "Marketing Campaign Execution",
  "description": "Automated campaign execution with multi-channel delivery and tracking",
  "entityType": "Campaign",
  "category": "Marketing",
  "nodes": [
    {
      "nodeKey": "trigger-campaign-start",
      "name": "Campaign Activated",
      "nodeType": "Trigger",
      "configuration": {
        "triggerType": "FieldChanged",
        "entityType": "Campaign",
        "field": "status",
        "value": "Active"
      }
    },
    {
      "nodeKey": "validate-campaign",
      "name": "Validate Campaign Setup",
      "nodeType": "Action",
      "configuration": {
        "actionType": "validateCampaign",
        "checks": ["hasAudience", "hasContent", "hasBudget"]
      }
    },
    {
      "nodeKey": "segment-audience",
      "name": "Build Audience Segments",
      "nodeType": "Action",
      "configuration": {
        "actionType": "buildSegments",
        "source": "{{campaign.targetAudience}}"
      }
    },
    {
      "nodeKey": "parallel-channels",
      "name": "Multi-Channel Send",
      "nodeType": "ParallelGateway",
      "configuration": {}
    },
    {
      "nodeKey": "email-channel",
      "name": "Send Email Campaign",
      "nodeType": "Action",
      "configuration": {
        "actionType": "sendBulkEmail",
        "segment": "email-subscribers",
        "template": "{{campaign.emailTemplate}}"
      }
    },
    {
      "nodeKey": "sms-channel",
      "name": "Send SMS Campaign",
      "nodeType": "Action",
      "configuration": {
        "actionType": "sendBulkSms",
        "segment": "sms-subscribers",
        "message": "{{campaign.smsMessage}}"
      }
    },
    {
      "nodeKey": "social-channel",
      "name": "Post to Social Media",
      "nodeType": "Integration",
      "configuration": {
        "integrationType": "SocialMedia",
        "platforms": ["twitter", "linkedin", "facebook"],
        "content": "{{campaign.socialContent}}"
      }
    },
    {
      "nodeKey": "join-channels",
      "name": "Wait for All Channels",
      "nodeType": "JoinGateway",
      "configuration": {
        "waitFor": "all"
      }
    },
    {
      "nodeKey": "wait-for-results",
      "name": "Wait 24 Hours",
      "nodeType": "Wait",
      "configuration": {
        "waitMinutes": 1440
      }
    },
    {
      "nodeKey": "calculate-metrics",
      "name": "Calculate Campaign Metrics",
      "nodeType": "Action",
      "configuration": {
        "actionType": "aggregateMetrics",
        "metrics": ["opens", "clicks", "conversions", "roi"]
      }
    },
    {
      "nodeKey": "ai-analysis",
      "name": "AI Performance Analysis",
      "nodeType": "LLMAction",
      "configuration": {
        "model": "gpt-4",
        "prompt": "Analyze campaign performance metrics and provide insights:\n\nMetrics: {{metrics}}\nGoals: {{campaign.goals}}\n\nProvide:\n1. Performance summary\n2. Key insights\n3. Recommendations for optimization",
        "outputField": "aiAnalysis"
      }
    },
    {
      "nodeKey": "generate-report",
      "name": "Generate Campaign Report",
      "nodeType": "Action",
      "configuration": {
        "actionType": "generateReport",
        "template": "campaign-performance",
        "includeAIInsights": true
      }
    },
    {
      "nodeKey": "notify-team",
      "name": "Notify Marketing Team",
      "nodeType": "Action",
      "configuration": {
        "actionType": "sendNotification",
        "toRole": "MarketingTeam",
        "subject": "Campaign {{campaign.name}} Report Ready"
      }
    },
    {
      "nodeKey": "end-complete",
      "name": "Campaign Complete",
      "nodeType": "End"
    }
  ],
  "transitions": [
    { "source": "trigger-campaign-start", "target": "validate-campaign" },
    { "source": "validate-campaign", "target": "segment-audience" },
    { "source": "segment-audience", "target": "parallel-channels" },
    { "source": "parallel-channels", "target": "email-channel" },
    { "source": "parallel-channels", "target": "sms-channel" },
    { "source": "parallel-channels", "target": "social-channel" },
    { "source": "email-channel", "target": "join-channels" },
    { "source": "sms-channel", "target": "join-channels" },
    { "source": "social-channel", "target": "join-channels" },
    { "source": "join-channels", "target": "wait-for-results" },
    { "source": "wait-for-results", "target": "calculate-metrics" },
    { "source": "calculate-metrics", "target": "ai-analysis" },
    { "source": "ai-analysis", "target": "generate-report" },
    { "source": "generate-report", "target": "notify-team" },
    { "source": "notify-team", "target": "end-complete" }
  ]
}
```

## 4. Sales Pipeline Workflow

**Trigger:** Opportunity stage change

```json
{
  "workflowKey": "sales-pipeline",
  "name": "Sales Pipeline Automation",
  "description": "Automate sales pipeline progression with alerts, tasks, and AI assistance",
  "entityType": "Opportunity",
  "category": "Sales",
  "nodes": [
    {
      "nodeKey": "trigger-stage-change",
      "name": "Stage Changed",
      "nodeType": "Trigger",
      "configuration": {
        "triggerType": "FieldChanged",
        "entityType": "Opportunity",
        "field": "stage"
      }
    },
    {
      "nodeKey": "route-by-stage",
      "name": "Route by Stage",
      "nodeType": "Condition"
    },
    {
      "nodeKey": "qualification-tasks",
      "name": "Create Qualification Tasks",
      "nodeType": "Action",
      "configuration": {
        "actionType": "createTasks",
        "tasks": [
          { "title": "Identify decision makers", "dueInDays": 3 },
          { "title": "Understand budget", "dueInDays": 5 },
          { "title": "Document requirements", "dueInDays": 7 }
        ]
      }
    },
    {
      "nodeKey": "proposal-assist",
      "name": "AI Proposal Assistant",
      "nodeType": "LLMAction",
      "configuration": {
        "model": "gpt-4",
        "prompt": "Generate a proposal outline for:\n\nCompany: {{opportunity.accountName}}\nProducts: {{opportunity.products}}\nBudget: {{opportunity.budget}}\nRequirements: {{opportunity.requirements}}\n\nInclude: executive summary, solution overview, pricing options, implementation timeline, ROI analysis"
      }
    },
    {
      "nodeKey": "negotiation-alert",
      "name": "Alert Sales Manager",
      "nodeType": "Action",
      "configuration": {
        "actionType": "sendNotification",
        "to": "{{opportunity.owner.manager}}",
        "priority": "high",
        "message": "Deal entering negotiation: {{opportunity.name}} - ${{opportunity.amount}}"
      }
    },
    {
      "nodeKey": "won-celebration",
      "name": "Celebrate & Notify",
      "nodeType": "ParallelGateway"
    },
    {
      "nodeKey": "notify-success",
      "name": "Send Success Notification",
      "nodeType": "Action",
      "configuration": {
        "actionType": "sendNotification",
        "toChannel": "#sales-wins",
        "message": "🎉 Deal Won! {{opportunity.name}} - ${{opportunity.amount}}"
      }
    },
    {
      "nodeKey": "create-order",
      "name": "Create Order",
      "nodeType": "Action",
      "configuration": {
        "actionType": "createEntity",
        "entityType": "Order",
        "copyFieldsFrom": "opportunity"
      }
    },
    {
      "nodeKey": "handoff-to-service",
      "name": "Initiate Service Handoff",
      "nodeType": "Subprocess",
      "configuration": {
        "workflowKey": "customer-onboarding",
        "inputMapping": {
          "customerId": "{{opportunity.accountId}}"
        }
      }
    },
    {
      "nodeKey": "join-won",
      "name": "Complete Won Process",
      "nodeType": "JoinGateway"
    },
    {
      "nodeKey": "lost-analysis",
      "name": "AI Loss Analysis",
      "nodeType": "LLMAction",
      "configuration": {
        "model": "gpt-4",
        "prompt": "Analyze why this opportunity was lost:\n\nDeal: {{opportunity.name}}\nCompetitor: {{opportunity.competitor}}\nLoss Reason: {{opportunity.lossReason}}\nActivities: {{opportunity.activities}}\n\nProvide insights and learnings for future deals."
      }
    },
    {
      "nodeKey": "update-analytics",
      "name": "Update Sales Analytics",
      "nodeType": "Action",
      "configuration": {
        "actionType": "updateAnalytics",
        "metrics": ["pipelineValue", "winRate", "avgDealSize"]
      }
    },
    {
      "nodeKey": "end-won",
      "name": "Deal Closed Won",
      "nodeType": "End"
    },
    {
      "nodeKey": "end-lost",
      "name": "Deal Closed Lost",
      "nodeType": "End"
    },
    {
      "nodeKey": "end-progressed",
      "name": "Stage Progressed",
      "nodeType": "End"
    }
  ],
  "transitions": [
    { "source": "trigger-stage-change", "target": "route-by-stage" },
    { "source": "route-by-stage", "target": "qualification-tasks", "condition": "stage == 'Qualification'" },
    { "source": "route-by-stage", "target": "proposal-assist", "condition": "stage == 'Proposal'" },
    { "source": "route-by-stage", "target": "negotiation-alert", "condition": "stage == 'Negotiation'" },
    { "source": "route-by-stage", "target": "won-celebration", "condition": "stage == 'ClosedWon'" },
    { "source": "route-by-stage", "target": "lost-analysis", "condition": "stage == 'ClosedLost'" },
    { "source": "qualification-tasks", "target": "end-progressed" },
    { "source": "proposal-assist", "target": "end-progressed" },
    { "source": "negotiation-alert", "target": "end-progressed" },
    { "source": "won-celebration", "target": "notify-success" },
    { "source": "won-celebration", "target": "create-order" },
    { "source": "won-celebration", "target": "handoff-to-service" },
    { "source": "notify-success", "target": "join-won" },
    { "source": "create-order", "target": "join-won" },
    { "source": "handoff-to-service", "target": "join-won" },
    { "source": "join-won", "target": "update-analytics" },
    { "source": "update-analytics", "target": "end-won" },
    { "source": "lost-analysis", "target": "update-analytics" }
  ]
}
```

## 5. Service Request Escalation Workflow

**Trigger:** Service request SLA breach

```json
{
  "workflowKey": "service-escalation",
  "name": "Service Request Escalation",
  "description": "Automated escalation workflow for service requests approaching or breaching SLA",
  "entityType": "ServiceRequest",
  "category": "Service",
  "nodes": [
    {
      "nodeKey": "trigger-sla-warning",
      "name": "SLA Warning Triggered",
      "nodeType": "Trigger",
      "configuration": {
        "triggerType": "Timer",
        "condition": "slaDeadline - now <= 2hours",
        "entityType": "ServiceRequest"
      }
    },
    {
      "nodeKey": "check-status",
      "name": "Check Current Status",
      "nodeType": "Condition"
    },
    {
      "nodeKey": "notify-agent",
      "name": "Notify Assigned Agent",
      "nodeType": "Action",
      "configuration": {
        "actionType": "sendNotification",
        "to": "{{serviceRequest.assignedTo}}",
        "priority": "high",
        "message": "⚠️ SLA Warning: {{serviceRequest.subject}} is approaching deadline"
      }
    },
    {
      "nodeKey": "wait-response",
      "name": "Wait 30 Minutes",
      "nodeType": "Wait",
      "configuration": {
        "waitMinutes": 30,
        "exitCondition": "serviceRequest.status == 'Resolved'"
      }
    },
    {
      "nodeKey": "check-resolved",
      "name": "Check if Resolved",
      "nodeType": "Condition"
    },
    {
      "nodeKey": "escalate-supervisor",
      "name": "Escalate to Supervisor",
      "nodeType": "Action",
      "configuration": {
        "actionType": "escalate",
        "level": 1,
        "assignTo": "{{serviceRequest.assignedTo.supervisor}}"
      }
    },
    {
      "nodeKey": "supervisor-task",
      "name": "Supervisor Review",
      "nodeType": "HumanTask",
      "configuration": {
        "title": "Review Escalated Service Request",
        "description": "Service request {{serviceRequest.id}} has breached SLA. Please review and take action.",
        "formSchema": {
          "fields": [
            { "name": "action", "label": "Action Taken", "type": "select", "options": ["Reassigned", "Resolved", "Further Escalated"] },
            { "name": "notes", "label": "Notes", "type": "textarea", "required": true }
          ]
        }
      }
    },
    {
      "nodeKey": "route-action",
      "name": "Route by Action",
      "nodeType": "Condition"
    },
    {
      "nodeKey": "escalate-manager",
      "name": "Escalate to Manager",
      "nodeType": "Action",
      "configuration": {
        "actionType": "escalate",
        "level": 2,
        "assignTo": "ServiceManager"
      }
    },
    {
      "nodeKey": "manager-notification",
      "name": "Manager Notification",
      "nodeType": "Action",
      "configuration": {
        "actionType": "sendNotification",
        "toRole": "ServiceManager",
        "priority": "urgent",
        "message": "🚨 Critical: Service request {{serviceRequest.id}} requires immediate attention"
      }
    },
    {
      "nodeKey": "update-metrics",
      "name": "Update SLA Metrics",
      "nodeType": "Action",
      "configuration": {
        "actionType": "updateMetrics",
        "metrics": ["slaBreach", "escalationCount", "resolutionTime"]
      }
    },
    {
      "nodeKey": "end-resolved",
      "name": "Request Resolved",
      "nodeType": "End"
    },
    {
      "nodeKey": "end-escalated",
      "name": "Escalation Complete",
      "nodeType": "End"
    }
  ],
  "transitions": [
    { "source": "trigger-sla-warning", "target": "check-status" },
    { "source": "check-status", "target": "end-resolved", "condition": "status == 'Resolved'" },
    { "source": "check-status", "target": "notify-agent", "condition": "status != 'Resolved'", "isDefault": true },
    { "source": "notify-agent", "target": "wait-response" },
    { "source": "wait-response", "target": "check-resolved" },
    { "source": "check-resolved", "target": "update-metrics", "condition": "status == 'Resolved'" },
    { "source": "check-resolved", "target": "escalate-supervisor", "condition": "status != 'Resolved'" },
    { "source": "escalate-supervisor", "target": "supervisor-task" },
    { "source": "supervisor-task", "target": "route-action" },
    { "source": "route-action", "target": "update-metrics", "condition": "action == 'Resolved'" },
    { "source": "route-action", "target": "escalate-manager", "condition": "action == 'Further Escalated'" },
    { "source": "route-action", "target": "update-metrics", "condition": "action == 'Reassigned'" },
    { "source": "escalate-manager", "target": "manager-notification" },
    { "source": "manager-notification", "target": "update-metrics" },
    { "source": "update-metrics", "target": "end-resolved", "condition": "status == 'Resolved'" },
    { "source": "update-metrics", "target": "end-escalated", "condition": "status != 'Resolved'" }
  ]
}
```

## Usage

These workflow templates can be imported into the CRM Workflow Module through:

1. **Admin UI**: Navigate to Administration → Workflows → Import Template
2. **API**: POST to `/api/workflows/import` with the JSON configuration
3. **Database Seeding**: Include in the MasterDataSeeder for initial setup

Each workflow can be customized after import:
- Modify node configurations
- Adjust timing and delays
- Change condition expressions
- Add or remove nodes
- Update email templates and notification settings

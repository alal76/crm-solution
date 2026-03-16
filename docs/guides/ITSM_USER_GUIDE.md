# ITSM User Guide

> **CRM Solution - IT Service Management Module**  
> **Last Updated:** March 2026  
> **Version:** 0.625.0

## Overview

The IT Service Management (ITSM) module provides a comprehensive framework for managing IT services within your organization. Built on ITIL v4 best practices, this module enables efficient handling of incidents, problems, changes, and service requests.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Incident Management](#incident-management)
3. [Problem Management](#problem-management)
4. [Change Management](#change-management)
5. [Service Requests](#service-requests)
6. [Knowledge Base](#knowledge-base)
7. [SLA Management](#sla-management)
8. [Escalation Rules](#escalation-rules)
9. [Service Queues](#service-queues)
10. [CMDB (Configuration Management Database)](#cmdb-configuration-management-database)
11. [Dashboards & Reporting](#dashboards--reporting)
12. [API Quick Reference](#api-quick-reference)
13. [Best Practices](#best-practices)

---

## Getting Started

### Accessing the ITSM Module

1. Log in to the CRM Solution
2. Navigate to **ITSM** in the main navigation menu
3. Select the appropriate sub-module (Incidents, Problems, Changes, etc.)

### User Roles

| Role | Permissions |
|------|-------------|
| **Service Desk Agent** | Create, update, resolve tickets |
| **Problem Manager** | Manage problems, link to incidents |
| **Change Manager** | Create, approve, implement changes |
| **ITSM Admin** | Full access to all ITSM features |
| **End User** | Submit requests, view status |

---

## Incident Management

### What is an Incident?

An unplanned interruption to an IT service or reduction in the quality of an IT service.

### Incident Workflow

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│    New       │────▶│ In Progress  │────▶│   Pending    │
│  (Created)   │     │  (Working)   │     │ (Awaiting)   │
└──────────────┘     └──────────────┘     └──────────────┘
                            │                    │
                            ▼                    ▼
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   Resolved   │◀────│   Escalated  │◀────│   On Hold    │
└──────────────┘     └──────────────┘     └──────────────┘
        │
        ▼
┌──────────────┐
│    Closed    │
└──────────────┘
```

### Creating an Incident

1. Click **New Incident** button
2. Fill in required fields:
   - **Title**: Brief description of the issue
   - **Description**: Detailed explanation
   - **Category**: Select from predefined categories
   - **Priority**: Urgency + Impact = Priority
   - **Affected User**: Who is impacted
3. Click **Submit**

### Incident Priority Matrix

| | **High Impact** | **Medium Impact** | **Low Impact** |
|---|---|---|---|
| **High Urgency** | P1 - Critical | P2 - High | P3 - Medium |
| **Medium Urgency** | P2 - High | P3 - Medium | P4 - Low |
| **Low Urgency** | P3 - Medium | P4 - Low | P5 - Planning |

### SLA Timers

Each priority has associated SLA targets:

| Priority | Response Time | Resolution Time |
|----------|--------------|-----------------|
| P1 - Critical | 15 minutes | 4 hours |
| P2 - High | 30 minutes | 8 hours |
| P3 - Medium | 2 hours | 24 hours |
| P4 - Low | 4 hours | 48 hours |
| P5 - Planning | 8 hours | 5 business days |

---

## Problem Management

### What is a Problem?

A cause, or potential cause, of one or more incidents. Problem management focuses on identifying root causes.

### Problem Workflow

1. **Identification**: Problem identified from recurring incidents
2. **Logging**: Document problem details
3. **Investigation**: Root cause analysis (RCA)
4. **Diagnosis**: Identify underlying cause
5. **Resolution**: Implement fix or workaround
6. **Closure**: Verify resolution, update knowledge base

### Creating a Problem Record

1. Navigate to **ITSM > Problems**
2. Click **New Problem**
3. Fill in details:
   - **Summary**: Brief problem description
   - **Description**: Full details including symptoms
   - **Related Incidents**: Link associated incidents
   - **Category**: Technical category
   - **Priority**: Based on impact assessment
4. Assign to problem manager
5. Click **Create Problem**

### Root Cause Analysis (RCA)

Document RCA findings in the problem record:

- **5 Whys Analysis**: Iterative questioning technique
- **Fishbone Diagram**: Categorize potential causes
- **Timeline Analysis**: Chronological event mapping

### Known Error Database (KEDB)

Once root cause is identified but fix is pending:

1. Mark problem as **Known Error**
2. Document **Workaround** for service desk use
3. Create **Change Request** if permanent fix needed

---

## Change Management

### What is a Change?

Addition, modification, or removal of anything that could affect IT services.

### Change Types

| Type | Description | Approval |
|------|-------------|----------|
| **Standard** | Pre-approved, low-risk changes | Auto-approved |
| **Normal** | Requires CAB review | Change Advisory Board |
| **Emergency** | Urgent production fix | Emergency CAB |

### Change Workflow

```
┌───────────┐     ┌───────────┐     ┌───────────┐
│  Request  │────▶│  Assess   │────▶│  Approve  │
│ Submitted │     │  & Plan   │     │  (CAB)    │
└───────────┘     └───────────┘     └───────────┘
                                          │
                                          ▼
┌───────────┐     ┌───────────┐     ┌───────────┐
│  Review   │◀────│ Implement │◀────│ Schedule  │
│  & Close  │     │  Change   │     │  Change   │
└───────────┘     └───────────┘     └───────────┘
```

### Creating a Change Request

1. Navigate to **ITSM > Changes**
2. Click **New Change Request**
3. Complete the change form:
   - **Summary**: Change description
   - **Business Justification**: Why is this change needed?
   - **Risk Assessment**: Impact and likelihood
   - **Implementation Plan**: Step-by-step procedure
   - **Backout Plan**: Recovery procedure if change fails
   - **Test Plan**: Validation steps
   - **Scheduled Window**: Proposed implementation time
4. Attach supporting documentation
5. Submit for approval

### Risk Assessment

| Risk Level | Criteria | Approval |
|------------|----------|----------|
| Low | Minor, tested, reversible | Line manager |
| Medium | Moderate impact, well-tested | CAB |
| High | Significant impact, complex | CAB + Senior management |
| Critical | Business-critical systems | Emergency CAB |

---

## Service Requests

### What is a Service Request?

A formal request from a user for something to be provided – for example, information, advice, password reset, or standard change.

### Common Service Requests

- Password reset
- Software installation
- Access provisioning
- Hardware request
- Information request
- Training request

### Service Request Fulfillment

1. Navigate to **ITSM > Service Requests** or **Service Catalog**
2. Select request type from catalog
3. Fill in request details
4. Submit request
5. Track status via ticket number

### Service Catalog

The service catalog organizes available services:

| Category | Services |
|----------|----------|
| **Account Management** | Password reset, access requests |
| **Hardware** | Laptop, monitor, peripherals |
| **Software** | Application installation, licensing |
| **Network** | VPN access, connectivity |
| **Communication** | Email setup, phone configuration |

---

## Knowledge Base

### Overview

The Knowledge Base stores articles for self-service and agent reference.

### Article Types

- **How-To Guides**: Step-by-step instructions
- **FAQs**: Common questions and answers
- **Known Issues**: Documented problems with workarounds
- **Reference**: Technical documentation

### Creating an Article

1. Navigate to **ITSM > Knowledge Base**
2. Click **New Article**
3. Fill in:
   - **Title**: Descriptive, searchable title
   - **Category**: Article classification
   - **Content**: Written in Markdown
   - **Keywords**: Search terms
   - **Audience**: Internal, External, or Both
4. Submit for review
5. Publish after approval

### Article Lifecycle

```
Draft → Review → Published → Archived
```

---

## SLA Management

### Understanding SLAs

Service Level Agreements define expected service levels.

### SLA Components

| Component | Description |
|-----------|-------------|
| **Response SLA** | Time to first response |
| **Resolution SLA** | Time to resolution |
| **Business Hours** | When SLA clock runs |
| **Escalation Rules** | Auto-escalation triggers |

### SLA Dashboard

Monitor SLA performance in real-time:

- **SLA Compliance %**: Tickets meeting SLA
- **Average Response Time**: Mean first response
- **Average Resolution Time**: Mean closure time
- **At Risk Tickets**: Nearing SLA breach

### Escalation Matrix

| Escalation Level | Trigger | Notified |
|-----------------|---------|----------|
| Level 1 | 50% SLA elapsed | Assigned agent |
| Level 2 | 75% SLA elapsed | Team lead |
| Level 3 | 90% SLA elapsed | Service manager |
| Level 4 | SLA breached | Director |

---

## Escalation Rules

### What are Escalation Rules?

Escalation rules define automated actions taken when a ticket is at risk of or has breached an SLA. They ensure issues are surfaced to the right stakeholders in a timely manner.

### Configuring Escalation Rules

Escalation rules are configured under **Admin → Escalation Rules** (`/admin/escalation-rules`).

| Field | Description |
|-------|-------------|
| **Name** | Descriptive label for the rule |
| **Trigger** | Percentage of SLA elapsed (e.g., 50 %, 75 %, 90 %) or on breach |
| **Target** | Who to notify (assigned agent, team lead, manager, director) |
| **Action** | Notification, reassignment, or priority change |
| **Applies To** | Specific categories, priorities, or all tickets |

### Escalation Levels

| Level | Default Trigger | Default Action |
|-------|----------------|----------------|
| 1 | 50 % SLA elapsed | Notify assigned agent |
| 2 | 75 % SLA elapsed | Notify team lead |
| 3 | 90 % SLA elapsed | Notify service manager |
| 4 | SLA breached | Notify director, change priority to Critical |

### Creating an Escalation Rule

1. Navigate to **Admin → Escalation Rules**
2. Click **Create Rule**
3. Enter name, select trigger condition
4. Choose notification recipient (role or specific user)
5. Set optional actions (reassign, change priority)
6. Define scope (all tickets or specific category/priority)
7. Click **Save**

> **API:** `POST /api/escalation-rules` — see [API Quick Reference](#api-quick-reference)

---

## Service Queues

### What are Service Queues?

Service queues organise incoming tickets into logical work groups, enabling teams to efficiently triage and distribute workload. Each queue can have its own assignment rules, SLA policies, and team membership.

### Configuring Service Queues

Queues are managed under **Admin → Service Queues** or via `AdminConfigurationController`.

| Field | Description |
|-------|-------------|
| **Name** | Queue name (e.g., "Hardware Support", "Security Incidents") |
| **Description** | Purpose of the queue |
| **Members** | Users/groups assigned to the queue |
| **SLA Policy** | Default SLA applied to tickets in this queue |
| **Auto-Assignment** | Round-robin, load-balanced, or manual |
| **Categories** | Ticket categories automatically routed to this queue |
| **Business Hours** | Override global business hours for this queue |

### Creating a Service Queue

1. Navigate to **Admin → Configuration → Service Queues**
2. Click **New Queue**
3. Enter queue name and description
4. Assign team members
5. Link an SLA policy
6. Configure routing rules (category-based, priority-based)
7. Set auto-assignment method
8. Click **Save**

### Queue-Based Routing

When a ticket is created, the system evaluates routing rules in order:

1. **Category match** — route to queue owning that category
2. **Priority match** — critical tickets may route to specialist queues
3. **Keyword match** — subject/description pattern matching
4. **Default queue** — fallback if no rule matches

> **API:** `GET /api/adminconfiguration/service-queues` — see [API Quick Reference](#api-quick-reference)

---

## CMDB (Configuration Management Database)

### What is the CMDB?

The Configuration Management Database tracks all IT assets (Configuration Items or CIs) and their relationships, enabling impact analysis and change planning.

### Configuration Items (CIs)

| CI Type | Examples |
|---------|----------|
| **Hardware** | Servers, workstations, network switches |
| **Software** | Applications, OS licenses, middleware |
| **Services** | Business services, IT services |
| **Infrastructure** | Virtual machines, containers, cloud resources |
| **Documents** | Runbooks, architecture diagrams |

### Creating a CI

1. Navigate to **ITSM → CMDB**
2. Click **New CI**
3. Select CI Type
4. Enter CI name, version, and owner
5. Add attributes (IP address, location, vendor, etc.)
6. Set status (`Active`, `Retired`, `Maintenance`)
7. Click **Save**

### CI Relationships

CIs can be linked to show dependencies:

| Relationship Type | Description |
|-------------------|-------------|
| `depends-on` | CI requires another CI to function |
| `hosts` | Server hosts an application |
| `connects-to` | Network relationship |
| `part-of` | Logical grouping |
| `impacts` | Failure of CI affects another |

### Impact Analysis

Before raising a change, run an impact analysis:

1. Open the CI in CMDB
2. Click **Impact Analysis**
3. Review the dependency graph
4. Export affected services for stakeholder notification

### Linking CIs to Incidents/Changes

- On the **Incident** form, select **Affected CIs** from the CMDB
- On the **Change Request** form, select **Impacted CIs**
- The CMDB record will show all linked incidents and changes

> **API:** `GET /api/itsm/cmdb` — see [API Quick Reference](#api-quick-reference)


---

## Escalation Rules

### What are Escalation Rules?

Escalation rules define automated actions taken when a ticket is at risk of or has breached an SLA. They ensure issues are surfaced to the right stakeholders in a timely manner.

### Configuring Escalation Rules

Escalation rules are configured under **Admin → Escalation Rules** (`/admin/escalation-rules`).

| Field | Description |
|-------|-------------|
| **Name** | Descriptive label for the rule |
| **Trigger** | Percentage of SLA elapsed (e.g., 50 %, 75 %, 90 %) or on breach |
| **Target** | Who to notify (assigned agent, team lead, manager, director) |
| **Action** | Notification, reassignment, or priority change |
| **Applies To** | Specific categories, priorities, or all tickets |

### Escalation Levels

| Level | Default Trigger | Default Action |
|-------|----------------|----------------|
| 1 | 50 % SLA elapsed | Notify assigned agent |
| 2 | 75 % SLA elapsed | Notify team lead |
| 3 | 90 % SLA elapsed | Notify service manager |
| 4 | SLA breached | Notify director, change priority to Critical |

### Creating an Escalation Rule

1. Navigate to **Admin → Escalation Rules**
2. Click **Create Rule**
3. Enter name, select trigger condition
4. Choose notification recipient (role or specific user)
5. Set optional actions (reassign, change priority)
6. Define scope (all tickets or specific category/priority)
7. Click **Save**

> **API:** `POST /api/escalation-rules` — see [API Quick Reference](#api-quick-reference)

---

## Service Queues

### What are Service Queues?

Service queues organise incoming tickets into logical work groups, enabling teams to efficiently triage and distribute workload. Each queue can have its own assignment rules, SLA policies, and team membership.

### Configuring Service Queues

Queues are managed under **Admin → Service Queues** or via `AdminConfigurationController`.

| Field | Description |
|-------|-------------|
| **Name** | Queue name (e.g., "Hardware Support", "Security Incidents") |
| **Description** | Purpose of the queue |
| **Members** | Users/groups assigned to the queue |
| **SLA Policy** | Default SLA applied to tickets in this queue |
| **Auto-Assignment** | Round-robin, load-balanced, or manual |
| **Categories** | Ticket categories automatically routed to this queue |
| **Business Hours** | Override global business hours for this queue |

### Creating a Service Queue

1. Navigate to **Admin → Configuration → Service Queues**
2. Click **New Queue**
3. Enter queue name and description
4. Assign team members
5. Link an SLA policy
6. Configure routing rules (category-based, priority-based)
7. Set auto-assignment method
8. Click **Save**

### Queue-Based Routing

When a ticket is created, the system evaluates routing rules in order:

1. **Category match** — route to queue owning that category
2. **Priority match** — critical tickets may route to specialist queues
3. **Keyword match** — subject/description pattern matching
4. **Default queue** — fallback if no rule matches

> **API:** `GET /api/adminconfiguration/service-queues` — see [API Quick Reference](#api-quick-reference)

---

## CMDB (Configuration Management Database)

### What is the CMDB?

The Configuration Management Database tracks all IT assets (Configuration Items or CIs) and their relationships, enabling impact analysis and change planning.

### Configuration Items (CIs)

| CI Type | Examples |
|---------|----------|
| **Hardware** | Servers, workstations, network switches |
| **Software** | Applications, OS licenses, middleware |
| **Services** | Business services, IT services |
| **Infrastructure** | Virtual machines, containers, cloud resources |
| **Documents** | Runbooks, architecture diagrams |

### Creating a CI

1. Navigate to **ITSM → CMDB**
2. Click **New CI**
3. Select CI Type
4. Enter CI name, version, and owner
5. Add attributes (IP address, location, vendor, etc.)
6. Set status (`Active`, `Retired`, `Maintenance`)
7. Click **Save**

### CI Relationships

CIs can be linked to show dependencies:

| Relationship Type | Description |
|-------------------|-------------|
| `depends-on` | CI requires another CI to function |
| `hosts` | Server hosts an application |
| `connects-to` | Network relationship |
| `part-of` | Logical grouping |
| `impacts` | Failure of CI affects another |

### Impact Analysis

Before raising a change, run an impact analysis:

1. Open the CI in CMDB
2. Click **Impact Analysis**
3. Review the dependency graph
4. Export affected services for stakeholder notification

### Linking CIs to Incidents/Changes

- On the **Incident** form, select **Affected CIs** from the CMDB
- On the **Change Request** form, select **Impacted CIs**
- The CMDB record will show all linked incidents and changes

> **API:** `GET /api/itsm/cmdb` — see [API Quick Reference](#api-quick-reference)


---

## Dashboards & Reporting

### ITSM Dashboard

The main ITSM dashboard provides:

- **Ticket Summary**: Open, pending, resolved counts
- **SLA Status**: Compliance overview
- **Top Categories**: Most common issue types
- **Team Workload**: Assignment distribution
- **Trend Charts**: Volume over time

### Available Reports

| Report | Description |
|--------|-------------|
| **Incident Summary** | Volume, categories, resolution times |
| **SLA Performance** | Compliance metrics by team/category |
| **Problem Trend** | Recurring issues analysis |
| **Change Success Rate** | Implementation success metrics |
| **Agent Performance** | Individual productivity metrics |

### Exporting Data

1. Navigate to desired report
2. Apply date range and filters
3. Click **Export**
4. Select format (PDF, Excel, CSV)

---

## API Quick Reference

> Full interactive documentation: **Swagger UI** at `http://<host>:5000/swagger`
> All endpoints require `Authorization: Bearer <JWT>` unless marked public.

### Incidents

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/itsm/incidents` | List incidents (paginated) |
| `GET` | `/api/itsm/incidents/{id}` | Get incident by ID |
| `POST` | `/api/itsm/incidents` | Create incident |
| `PUT` | `/api/itsm/incidents/{id}` | Update incident |
| `POST` | `/api/itsm/incidents/{id}/assign` | Assign to agent |
| `POST` | `/api/itsm/incidents/{id}/escalate` | Escalate incident |
| `POST` | `/api/itsm/incidents/{id}/resolve` | Resolve incident |
| `POST` | `/api/itsm/incidents/{id}/close` | Close incident |
| `POST` | `/api/itsm/incidents/{id}/reopen` | Reopen closed incident |
| `GET` | `/api/itsm/incidents/{id}/comments` | List comments |
| `POST` | `/api/itsm/incidents/{id}/comments` | Add comment |

### Problems

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/itsm/problems` | List problems |
| `POST` | `/api/itsm/problems` | Create problem |
| `PUT` | `/api/itsm/problems/{id}` | Update problem |
| `POST` | `/api/itsm/problems/{id}/link-incident` | Link incident to problem |
| `POST` | `/api/itsm/problems/{id}/mark-known-error` | Mark as known error |
| `GET` | `/api/itsm/problems/{id}/related-incidents` | List linked incidents |

### Changes

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/itsm/changes` | List change requests |
| `POST` | `/api/itsm/changes` | Create change request |
| `POST` | `/api/itsm/changes/{id}/submit-approval` | Submit for approval |
| `POST` | `/api/itsm/changes/{id}/approve` | Approve change |
| `POST` | `/api/itsm/changes/{id}/reject` | Reject change |
| `POST` | `/api/itsm/changes/{id}/schedule` | Schedule change |
| `GET` | `/api/itsm/changes/{id}/conflicts` | Check scheduling conflicts |
| `GET` | `/api/itsm/changes/{id}/impacted-cis` | List impacted CIs |
| `GET` | `/api/itsm/changes/blackout-periods` | Get blackout periods |

### CMDB

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/itsm/cmdb` | List configuration items |
| `GET` | `/api/itsm/cmdb/{id}` | Get CI by ID |
| `POST` | `/api/itsm/cmdb` | Create CI |
| `PUT` | `/api/itsm/cmdb/{id}` | Update CI |
| `GET` | `/api/itsm/cmdb/{id}/relationships` | List CI relationships |
| `POST` | `/api/itsm/cmdb/{id}/relationships` | Add CI relationship |
| `GET` | `/api/itsm/cmdb/{id}/impact-analysis` | Run impact analysis |

### Knowledge Base

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/itsm/knowledge` | List articles |
| `POST` | `/api/itsm/knowledge` | Create article |
| `POST` | `/api/itsm/knowledge/{id}/publish` | Publish article |
| `POST` | `/api/itsm/knowledge/{id}/retire` | Retire article |
| `POST` | `/api/itsm/knowledge/{id}/feedback` | Submit feedback/rating |
| `GET` | `/api/itsm/knowledge/suggested` | Suggested articles |
| `GET` | `/api/itsm/knowledge/popular` | Popular articles |

### SLA

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/itsm/sla/policies` | List SLA policies |
| `POST` | `/api/itsm/sla/policies` | Create SLA policy |
| `GET` | `/api/itsm/sla/instances` | List SLA instances |
| `POST` | `/api/itsm/sla/{id}/pause` | Pause SLA |
| `POST` | `/api/itsm/sla/{id}/resume` | Resume SLA |
| `GET` | `/api/itsm/sla/breached` | List breached SLAs |
| `GET` | `/api/itsm/sla/dashboard` | SLA compliance dashboard |
| `GET` | `/api/itsm/sla/at-risk` | At-risk items |

### Escalation Rules (Admin)

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/escalation-rules` | List escalation rules |
| `GET` | `/api/escalation-rules/{id}` | Get rule by ID |
| `POST` | `/api/escalation-rules` | Create rule |
| `PUT` | `/api/escalation-rules/{id}` | Update rule |
| `DELETE` | `/api/escalation-rules/{id}` | Delete rule |

### Service Queues (Admin)

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/adminconfiguration/service-queues` | List service queues |
| `GET` | `/api/adminconfiguration/service-queues/{id}` | Get queue by ID |
| `POST` | `/api/adminconfiguration/service-queues` | Create queue |
| `PUT` | `/api/adminconfiguration/service-queues/{id}` | Update queue |
| `DELETE` | `/api/adminconfiguration/service-queues/{id}` | Delete queue |

### ITSM Dashboard Analytics

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/itsm/dashboard` | Full ITSM dashboard |
| `GET` | `/api/itsm/dashboard/incident-trends` | Incident volume trends |
| `GET` | `/api/itsm/dashboard/problem-analysis` | Problem metrics |
| `GET` | `/api/itsm/dashboard/change-metrics` | Change success metrics |
| `GET` | `/api/itsm/dashboard/sla-compliance` | SLA compliance overview |
| `GET` | `/api/itsm/dashboard/team-performance` | Team performance stats |
| `GET` | `/api/itsm/dashboard/service-health` | Overall service health |


---

## API Quick Reference

> Full interactive documentation: **Swagger UI** at `http://<host>:5000/swagger`
> All endpoints require `Authorization: Bearer <JWT>` unless marked public.

### Incidents

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/itsm/incidents` | List incidents (paginated) |
| `GET` | `/api/itsm/incidents/{id}` | Get incident by ID |
| `POST` | `/api/itsm/incidents` | Create incident |
| `PUT` | `/api/itsm/incidents/{id}` | Update incident |
| `POST` | `/api/itsm/incidents/{id}/assign` | Assign to agent |
| `POST` | `/api/itsm/incidents/{id}/escalate` | Escalate incident |
| `POST` | `/api/itsm/incidents/{id}/resolve` | Resolve incident |
| `POST` | `/api/itsm/incidents/{id}/close` | Close incident |
| `POST` | `/api/itsm/incidents/{id}/reopen` | Reopen closed incident |
| `GET` | `/api/itsm/incidents/{id}/comments` | List comments |
| `POST` | `/api/itsm/incidents/{id}/comments` | Add comment |

### Problems

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/itsm/problems` | List problems |
| `POST` | `/api/itsm/problems` | Create problem |
| `PUT` | `/api/itsm/problems/{id}` | Update problem |
| `POST` | `/api/itsm/problems/{id}/link-incident` | Link incident to problem |
| `POST` | `/api/itsm/problems/{id}/mark-known-error` | Mark as known error |
| `GET` | `/api/itsm/problems/{id}/related-incidents` | List linked incidents |

### Changes

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/itsm/changes` | List change requests |
| `POST` | `/api/itsm/changes` | Create change request |
| `POST` | `/api/itsm/changes/{id}/submit-approval` | Submit for approval |
| `POST` | `/api/itsm/changes/{id}/approve` | Approve change |
| `POST` | `/api/itsm/changes/{id}/reject` | Reject change |
| `POST` | `/api/itsm/changes/{id}/schedule` | Schedule change |
| `GET` | `/api/itsm/changes/{id}/conflicts` | Check scheduling conflicts |
| `GET` | `/api/itsm/changes/{id}/impacted-cis` | List impacted CIs |
| `GET` | `/api/itsm/changes/blackout-periods` | Get blackout periods |

### CMDB

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/itsm/cmdb` | List configuration items |
| `GET` | `/api/itsm/cmdb/{id}` | Get CI by ID |
| `POST` | `/api/itsm/cmdb` | Create CI |
| `PUT` | `/api/itsm/cmdb/{id}` | Update CI |
| `GET` | `/api/itsm/cmdb/{id}/relationships` | List CI relationships |
| `POST` | `/api/itsm/cmdb/{id}/relationships` | Add CI relationship |
| `GET` | `/api/itsm/cmdb/{id}/impact-analysis` | Run impact analysis |

### Knowledge Base

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/itsm/knowledge` | List articles |
| `POST` | `/api/itsm/knowledge` | Create article |
| `POST` | `/api/itsm/knowledge/{id}/publish` | Publish article |
| `POST` | `/api/itsm/knowledge/{id}/retire` | Retire article |
| `POST` | `/api/itsm/knowledge/{id}/feedback` | Submit feedback/rating |
| `GET` | `/api/itsm/knowledge/suggested` | Suggested articles |
| `GET` | `/api/itsm/knowledge/popular` | Popular articles |

### SLA

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/itsm/sla/policies` | List SLA policies |
| `POST` | `/api/itsm/sla/policies` | Create SLA policy |
| `GET` | `/api/itsm/sla/instances` | List SLA instances |
| `POST` | `/api/itsm/sla/{id}/pause` | Pause SLA |
| `POST` | `/api/itsm/sla/{id}/resume` | Resume SLA |
| `GET` | `/api/itsm/sla/breached` | List breached SLAs |
| `GET` | `/api/itsm/sla/dashboard` | SLA compliance dashboard |
| `GET` | `/api/itsm/sla/at-risk` | At-risk items |

### Escalation Rules (Admin)

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/escalation-rules` | List escalation rules |
| `GET` | `/api/escalation-rules/{id}` | Get rule by ID |
| `POST` | `/api/escalation-rules` | Create rule |
| `PUT` | `/api/escalation-rules/{id}` | Update rule |
| `DELETE` | `/api/escalation-rules/{id}` | Delete rule |

### Service Queues (Admin)

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/adminconfiguration/service-queues` | List service queues |
| `GET` | `/api/adminconfiguration/service-queues/{id}` | Get queue by ID |
| `POST` | `/api/adminconfiguration/service-queues` | Create queue |
| `PUT` | `/api/adminconfiguration/service-queues/{id}` | Update queue |
| `DELETE` | `/api/adminconfiguration/service-queues/{id}` | Delete queue |

### ITSM Dashboard Analytics

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/itsm/dashboard` | Full ITSM dashboard |
| `GET` | `/api/itsm/dashboard/incident-trends` | Incident volume trends |
| `GET` | `/api/itsm/dashboard/problem-analysis` | Problem metrics |
| `GET` | `/api/itsm/dashboard/change-metrics` | Change success metrics |
| `GET` | `/api/itsm/dashboard/sla-compliance` | SLA compliance overview |
| `GET` | `/api/itsm/dashboard/team-performance` | Team performance stats |
| `GET` | `/api/itsm/dashboard/service-health` | Overall service health |


---

## Best Practices

### For Service Desk Agents

1. **Complete Documentation**: Record all details for future reference
2. **Consistent Categorization**: Use correct categories and priorities
3. **Regular Updates**: Keep customers informed of progress
4. **Knowledge Contribution**: Create articles from resolved tickets
5. **Follow SLA Guidelines**: Prioritize tickets approaching breach

### For Problem Managers

1. **Trend Analysis**: Regularly review incident patterns
2. **RCA Documentation**: Thorough root cause analysis
3. **Known Error Management**: Maintain KEDB accuracy
4. **Proactive Identification**: Don't wait for incidents

### For Change Managers

1. **Complete Impact Assessment**: Consider all affected services
2. **Stakeholder Communication**: Notify affected parties
3. **Testing Requirements**: Ensure adequate testing
4. **Documentation**: Maintain implementation records
5. **Post-Implementation Review**: Learn from each change

---

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl + N` | New ticket |
| `Ctrl + S` | Save changes |
| `Ctrl + Enter` | Submit form |
| `Esc` | Close dialog |
| `/` | Quick search |

---

## Support

For questions about the ITSM module:

- **Internal Support**: Submit a ticket via Service Desk
- **Documentation**: Review this guide and Knowledge Base
- **Training**: Contact your administrator for training sessions

---

**TODO-DOC-01** ✅ Implemented

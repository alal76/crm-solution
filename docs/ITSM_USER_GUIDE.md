# ITSM Module — User Guide

> **Last Updated:** February 10, 2026
> **Module Version:** 2.0.0
> **Status:** Fully Operational — 7 core services, 8 controllers, 13 frontend pages

---

## Table of Contents

1. [Overview](#1-overview)
2. [Incident Management](#2-incident-management)
3. [Problem Management](#3-problem-management)
4. [Change Management](#4-change-management)
5. [CMDB (Configuration Management Database)](#5-cmdb-configuration-management-database)
6. [Knowledge Base](#6-knowledge-base)
7. [Service Catalog](#7-service-catalog)
8. [SLA Management](#8-sla-management)
9. [ITSM Dashboard](#9-itsm-dashboard)
10. [API Reference](#10-api-reference)
11. [Common Workflows & Best Practices](#11-common-workflows--best-practices)

---

## 1. Overview

The ITSM (IT Service Management) module provides ITIL-aligned processes for managing IT services within the CRM Solution. It was implemented as part of Phase 1 of the remediation plan and is fully integrated with the existing CRM backend, frontend, and test infrastructure.

### Capabilities at a Glance

| Capability | Description |
|------------|-------------|
| Incident Management | Track and resolve unplanned service interruptions |
| Problem Management | Identify root causes and eliminate recurring incidents |
| Change Management | Control changes to IT infrastructure with approval workflows |
| CMDB | Maintain a database of configuration items and relationships |
| Knowledge Base | Author, publish, and search knowledge articles |
| Service Catalog | Browse available services and submit requests |
| SLA Management | Define, monitor, and report on service level agreements |
| ITSM Dashboard | Real-time analytics across all ITSM processes |

### Access

| Item | URL |
|------|-----|
| Frontend | `http://<host>/itsm/*` |
| API Base | `http://<host>:5000/api/itsm/*` |
| Swagger | `http://<host>:5000/swagger` |

### Authentication

All ITSM API endpoints require a valid JWT bearer token. Obtain one via the login endpoint:

```bash
# Login
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@crm.local","password":"Admin@123"}' \
  | jq -r '.accessToken')

# Use the token for all subsequent requests
curl -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/itsm/incidents
```

---

## 2. Incident Management

Incidents represent unplanned interruptions or reductions in the quality of an IT service. The goal is to restore normal service operation as quickly as possible.

### 2.1 Incident Lifecycle

```
┌──────────┐    ┌──────────┐    ┌───────────┐    ┌──────────┐    ┌──────────┐
│  Create  │───▶│  Assign  │───▶│ Escalate  │───▶│ Resolve  │───▶│  Close   │
│          │    │          │    │(optional) │    │          │    │          │
└──────────┘    └──────────┘    └───────────┘    └──────────┘    └──────────┘
                                                       │               │
                                                       │               ▼
                                                       │         ┌──────────┐
                                                       └────────▶│  Reopen  │
                                                                  └──────────┘
```

| Status | Description |
|--------|-------------|
| **New** | Incident just created, not yet assigned |
| **Assigned** | Assigned to a technician or team |
| **In Progress** | Work has started |
| **Escalated** | Escalated to a higher-tier team |
| **Resolved** | A fix has been applied, awaiting confirmation |
| **Closed** | Confirmed resolved, no further action needed |
| **Reopened** | Previously closed but the issue recurred |

### 2.2 Creating an Incident

**Frontend:** Navigate to `/itsm/incidents` → click **New Incident**.

**API:**

```bash
curl -X POST http://localhost:5000/api/itsm/incidents \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Email server not responding",
    "description": "Users unable to send or receive email since 09:00.",
    "priority": "High",
    "category": "Infrastructure",
    "impactLevel": "High",
    "urgencyLevel": "High"
  }'
```

### 2.3 Assigning an Incident

```bash
curl -X POST http://localhost:5000/api/itsm/incidents/{id}/assign \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "assigneeId": 5,
    "notes": "Assigning to network team for investigation."
  }'
```

### 2.4 Escalating an Incident

Escalation raises the incident to a higher-tier support team when the current assignee cannot resolve it within SLA targets.

```bash
curl -X POST http://localhost:5000/api/itsm/incidents/{id}/escalate \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "escalationLevel": 2,
    "reason": "Issue requires network infrastructure access."
  }'
```

### 2.5 Resolving and Closing an Incident

```bash
# Resolve
curl -X POST http://localhost:5000/api/itsm/incidents/{id}/resolve \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "resolutionNotes": "Restarted mail relay service. All queues cleared."
  }'

# Close
curl -X POST http://localhost:5000/api/itsm/incidents/{id}/close \
  -H "Authorization: Bearer $TOKEN"
```

### 2.6 Reopening an Incident

If the issue recurs after closure:

```bash
curl -X POST http://localhost:5000/api/itsm/incidents/{id}/reopen \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "reason": "Email service went down again 30 minutes after resolution."
  }'
```

### 2.7 Adding Comments

Comments provide a collaboration thread on an incident.

```bash
curl -X POST http://localhost:5000/api/itsm/incidents/{id}/comments \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "text": "Checked MX records — no DNS issues found.",
    "isInternal": true
  }'
```

---

## 3. Problem Management

Problems are the root cause of one or more incidents. Problem management focuses on identifying and eliminating root causes to prevent future incidents.

### 3.1 Problem Lifecycle

```
┌──────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌──────────┐
│  Create  │───▶│ Investigate │───▶│ Root Cause  │───▶│ Known Error │───▶│ Resolve  │
│          │    │             │    │ Identified  │    │  Recorded   │    │          │
└──────────┘    └─────────────┘    └─────────────┘    └─────────────┘    └──────────┘
                      │
                      ▼
               ┌─────────────┐
               │Link Incident│
               └─────────────┘
```

| Status | Description |
|--------|-------------|
| **New** | Problem identified, not yet investigated |
| **Under Investigation** | Root cause analysis in progress |
| **Root Cause Identified** | Root cause found but no workaround yet |
| **Known Error** | Root cause and workaround documented |
| **Resolved** | Permanent fix applied |
| **Closed** | No further action required |

### 3.2 Creating a Problem

```bash
curl -X POST http://localhost:5000/api/itsm/problems \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Recurring email relay failures",
    "description": "Mail relay service crashes every 48 hours under load.",
    "priority": "High",
    "category": "Infrastructure"
  }'
```

### 3.3 Linking an Incident to a Problem

Linking incidents to their parent problem allows tracking how many incidents share the same root cause.

```bash
curl -X POST http://localhost:5000/api/itsm/problems/{problemId}/link-incident \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "incidentId": 42
  }'
```

### 3.4 Recording Root Cause

```bash
curl -X POST http://localhost:5000/api/itsm/problems/{id}/root-cause \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "rootCause": "Memory leak in mail relay service v3.2.1 causes OOM crash under sustained load.",
    "workaround": "Scheduled restart of mail relay every 24 hours until vendor patch available."
  }'
```

### 3.5 Marking as Known Error

Once a workaround is documented, mark the problem as a known error so support agents can apply the workaround to future incidents.

```bash
curl -X POST http://localhost:5000/api/itsm/problems/{id}/mark-known-error \
  -H "Authorization: Bearer $TOKEN"
```

### 3.6 Viewing Related Incidents

```bash
curl http://localhost:5000/api/itsm/problems/{id}/related-incidents \
  -H "Authorization: Bearer $TOKEN"
```

---

## 4. Change Management

Change Management controls modifications to IT infrastructure to minimize disruption while enabling beneficial changes.

### 4.1 Change Request Lifecycle

```
┌──────────┐    ┌───────────────┐    ┌─────────────┐    ┌──────────┐    ┌───────────┐
│  Create  │───▶│ Submit for    │───▶│  Approve /  │───▶│ Schedule │───▶│ Implement │
│          │    │   Approval    │    │   Reject    │    │          │    │           │
└──────────┘    └───────────────┘    └─────────────┘    └──────────┘    └───────────┘
```

| Status | Description |
|--------|-------------|
| **Draft** | Change request created, not yet submitted |
| **Pending Approval** | Submitted for review by CAB or approvers |
| **Approved** | Change approved, ready to schedule |
| **Rejected** | Change rejected with reason |
| **Scheduled** | Implementation window set |
| **In Progress** | Change being implemented |
| **Completed** | Change successfully implemented |
| **Failed** | Change implementation failed, rollback initiated |
| **Cancelled** | Change cancelled before implementation |

### 4.2 Creating a Change Request

```bash
curl -X POST http://localhost:5000/api/itsm/changes \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Upgrade mail relay to v3.3.0",
    "description": "Upgrade mail relay service to fix memory leak (Problem PRB-101).",
    "changeType": "Normal",
    "priority": "High",
    "riskLevel": "Medium",
    "rollbackPlan": "Revert to v3.2.1 using Ansible playbook.",
    "implementationPlan": "1. Stop service. 2. Backup config. 3. Deploy v3.3.0. 4. Validate."
  }'
```

### 4.3 Submitting for Approval

```bash
curl -X POST http://localhost:5000/api/itsm/changes/{id}/submit-approval \
  -H "Authorization: Bearer $TOKEN"
```

### 4.4 Approving or Rejecting

```bash
# Approve
curl -X POST http://localhost:5000/api/itsm/changes/{id}/approve \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "approverNotes": "Approved. Schedule for Saturday maintenance window."
  }'

# Reject
curl -X POST http://localhost:5000/api/itsm/changes/{id}/reject \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "reason": "Vendor patch not yet certified. Wait for next release."
  }'
```

### 4.5 Scheduling a Change

```bash
curl -X POST http://localhost:5000/api/itsm/changes/{id}/schedule \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "scheduledStartDate": "2026-02-15T02:00:00Z",
    "scheduledEndDate": "2026-02-15T04:00:00Z"
  }'
```

### 4.6 Checking for Scheduling Conflicts

Before scheduling, check for overlapping changes or blackout periods:

```bash
curl "http://localhost:5000/api/itsm/changes/{id}/conflicts" \
  -H "Authorization: Bearer $TOKEN"
```

### 4.7 Managing Blackout Periods

Blackout periods are times when no changes should be deployed (e.g., end-of-quarter freeze).

```bash
# List blackout periods
curl http://localhost:5000/api/itsm/changes/blackout-periods \
  -H "Authorization: Bearer $TOKEN"
```

### 4.8 Impacted Configuration Items

Link CIs that will be affected by the change:

```bash
curl "http://localhost:5000/api/itsm/changes/{id}/impacted-cis" \
  -H "Authorization: Bearer $TOKEN"
```

---

## 5. CMDB (Configuration Management Database)

The CMDB maintains a record of all Configuration Items (CIs) — hardware, software, services, and their relationships.

### 5.1 Core Concepts

| Concept | Description |
|---------|-------------|
| **Configuration Item (CI)** | Any component that needs to be managed (server, application, network device, etc.) |
| **CI Relationship** | A link between two CIs (e.g., "Server A *hosts* Application B") |
| **Impact Analysis** | Determine which services and CIs are affected when a CI fails or changes |

### 5.2 Creating a Configuration Item

```bash
curl -X POST http://localhost:5000/api/itsm/cmdb \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "mail-relay-01",
    "type": "Server",
    "status": "Active",
    "environment": "Production",
    "description": "Primary mail relay server.",
    "attributes": {
      "os": "Ubuntu 22.04",
      "cpu": "4 vCPU",
      "ram": "16 GB",
      "ip": "10.0.1.50"
    }
  }'
```

### 5.3 Searching the CMDB

```bash
# Search by name or type
curl "http://localhost:5000/api/itsm/cmdb/search?query=mail&type=Server" \
  -H "Authorization: Bearer $TOKEN"
```

### 5.4 Managing CI Relationships

Relationships capture dependencies and connections between CIs.

```bash
# Add a relationship
curl -X POST http://localhost:5000/api/itsm/cmdb/{ciId}/relationships \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "relatedCiId": 15,
    "relationshipType": "Hosts",
    "direction": "Outgoing"
  }'

# Get related CIs
curl http://localhost:5000/api/itsm/cmdb/{ciId}/related \
  -H "Authorization: Bearer $TOKEN"
```

### 5.5 Impact Analysis

Impact analysis shows the upstream and downstream blast radius when a CI fails.

```bash
curl http://localhost:5000/api/itsm/cmdb/{ciId}/impact-analysis \
  -H "Authorization: Bearer $TOKEN"
```

Example response:

```json
{
  "configurationItemId": 10,
  "configurationItemName": "mail-relay-01",
  "directlyImpacted": [
    { "id": 15, "name": "Email Service", "type": "Service", "relationship": "Hosts" }
  ],
  "indirectlyImpacted": [
    { "id": 22, "name": "Customer Portal", "type": "Application", "relationship": "DependsOn → Email Service" }
  ],
  "totalImpactedItems": 2
}
```

---

## 6. Knowledge Base

The Knowledge Base stores articles that help support agents and end-users find solutions to common issues.

### 6.1 Article Lifecycle

```
┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐
│  Draft   │───▶│ Publish  │───▶│  Active  │───▶│  Retire  │
│          │    │          │    │          │    │          │
└──────────┘    └──────────┘    └──────────┘    └──────────┘
```

### 6.2 Creating an Article

```bash
curl -X POST http://localhost:5000/api/itsm/knowledge \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "How to restart the mail relay service",
    "content": "## Steps\n1. SSH to mail-relay-01\n2. Run: sudo systemctl restart mail-relay\n3. Verify: sudo systemctl status mail-relay",
    "category": "Infrastructure",
    "tags": ["email", "mail-relay", "restart"],
    "visibility": "Internal"
  }'
```

### 6.3 Publishing an Article

Only published articles are visible to end-users (if visibility allows).

```bash
curl -X POST http://localhost:5000/api/itsm/knowledge/{id}/publish \
  -H "Authorization: Bearer $TOKEN"
```

### 6.4 Retiring an Article

When an article is outdated, retire it to hide it from search results while preserving it for reference.

```bash
curl -X POST http://localhost:5000/api/itsm/knowledge/{id}/retire \
  -H "Authorization: Bearer $TOKEN"
```

### 6.5 Providing Feedback

End-users and agents can rate articles:

```bash
curl -X POST http://localhost:5000/api/itsm/knowledge/{id}/feedback \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "rating": 5,
    "comment": "This solved my issue immediately."
  }'
```

### 6.6 Finding Articles

```bash
# Suggested articles (e.g., based on an incident)
curl http://localhost:5000/api/itsm/knowledge/suggested?query=email+not+working \
  -H "Authorization: Bearer $TOKEN"

# Popular articles
curl http://localhost:5000/api/itsm/knowledge/popular \
  -H "Authorization: Bearer $TOKEN"

# Recent articles
curl http://localhost:5000/api/itsm/knowledge/recent \
  -H "Authorization: Bearer $TOKEN"

# By category
curl http://localhost:5000/api/itsm/knowledge/categories \
  -H "Authorization: Bearer $TOKEN"
```

---

## 7. Service Catalog

The Service Catalog provides a browseable catalog of IT services that users can request.

### 7.1 Browsing the Catalog

**Frontend:** Navigate to `/itsm/catalog` to view available services grouped by category.

**API:**

```bash
# List catalog items
curl http://localhost:5000/api/itsm/catalog \
  -H "Authorization: Bearer $TOKEN"

# Search catalog
curl "http://localhost:5000/api/itsm/catalog/search?query=laptop" \
  -H "Authorization: Bearer $TOKEN"

# Browse categories
curl http://localhost:5000/api/itsm/catalog/categories \
  -H "Authorization: Bearer $TOKEN"
```

### 7.2 Submitting a Service Request

```bash
curl -X POST http://localhost:5000/api/itsm/catalog/requests \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "catalogItemId": 3,
    "description": "New MacBook Pro for new hire starting March 1.",
    "priority": "Medium",
    "requiredByDate": "2026-03-01",
    "additionalInfo": {
      "employeeName": "Jane Smith",
      "department": "Engineering"
    }
  }'
```

### 7.3 Requesting on Behalf of Another User

Managers or HR can submit requests for others:

```bash
curl -X POST http://localhost:5000/api/itsm/catalog/request-for-others \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "catalogItemId": 3,
    "requestedForUserId": 12,
    "description": "Standard onboarding laptop for new hire.",
    "priority": "Medium"
  }'
```

### 7.4 Tracking and Cancelling Requests

```bash
# View request status
curl http://localhost:5000/api/itsm/catalog/requests/{id} \
  -H "Authorization: Bearer $TOKEN"

# Cancel a pending request
curl -X POST http://localhost:5000/api/itsm/catalog/requests/{id}/cancel \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "reason": "Hire start date postponed to April."
  }'
```

---

## 8. SLA Management

SLA (Service Level Agreement) Management defines, monitors, and reports on response and resolution targets.

### 8.1 Core Concepts

| Concept | Description |
|---------|-------------|
| **SLA Policy** | A set of response and resolution targets (e.g., "P1 incidents must be responded to within 15 min and resolved within 4 hours") |
| **SLA Instance** | A running SLA timer attached to a specific incident or service request |
| **Breach** | When a target is missed (response or resolution time exceeded) |
| **At-Risk** | An SLA instance approaching its target deadline |

### 8.2 Managing SLA Policies

```bash
# List all policies
curl http://localhost:5000/api/itsm/sla/policies \
  -H "Authorization: Bearer $TOKEN"

# Create a new policy
curl -X POST http://localhost:5000/api/itsm/sla/policies \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Critical Incident SLA",
    "description": "SLA for Priority 1 incidents.",
    "responseTimeMinutes": 15,
    "resolutionTimeMinutes": 240,
    "priority": "Critical",
    "businessHoursOnly": true
  }'
```

### 8.3 SLA Instance Lifecycle

```
┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐
│  Start   │───▶│  Pause   │───▶│  Resume  │───▶│ Complete │
│          │    │(optional)│    │          │    │          │
└──────────┘    └──────────┘    └──────────┘    └──────────┘
```

```bash
# Start an SLA instance
curl -X POST http://localhost:5000/api/itsm/sla/instances/{id}/start \
  -H "Authorization: Bearer $TOKEN"

# Pause (e.g., waiting on customer response)
curl -X POST http://localhost:5000/api/itsm/sla/instances/{id}/pause \
  -H "Authorization: Bearer $TOKEN"

# Resume
curl -X POST http://localhost:5000/api/itsm/sla/instances/{id}/resume \
  -H "Authorization: Bearer $TOKEN"

# Complete
curl -X POST http://localhost:5000/api/itsm/sla/instances/{id}/complete \
  -H "Authorization: Bearer $TOKEN"
```

### 8.4 Monitoring SLA Status

```bash
# View breached SLAs
curl http://localhost:5000/api/itsm/sla/breached \
  -H "Authorization: Bearer $TOKEN"

# View at-risk SLAs (approaching deadline)
curl http://localhost:5000/api/itsm/sla/at-risk \
  -H "Authorization: Bearer $TOKEN"

# SLA Dashboard summary
curl http://localhost:5000/api/itsm/sla/dashboard \
  -H "Authorization: Bearer $TOKEN"

# SLA Metrics
curl http://localhost:5000/api/itsm/sla/metrics \
  -H "Authorization: Bearer $TOKEN"
```

---

## 9. ITSM Dashboard

The ITSM Dashboard provides at-a-glance analytics across all ITSM processes.

**Frontend:** Navigate to `/itsm/dashboard` (or access individual module dashboards).

### Available Analytics

| Endpoint | Data Provided |
|----------|---------------|
| `GET /api/itsm/dashboard/incident-trends` | Incident volume over time, by priority and category |
| `GET /api/itsm/dashboard/problem-analysis` | Open problems, known errors, linked incident counts |
| `GET /api/itsm/dashboard/change-metrics` | Change success rate, approval turnaround, change volume |
| `GET /api/itsm/dashboard/sla-compliance` | SLA compliance percentage, breach trends, at-risk count |
| `GET /api/itsm/dashboard/team-performance` | Mean time to resolve, assignment distribution, workload |
| `GET /api/itsm/dashboard/service-health` | Overall service availability, CI status summary |

### Example

```bash
curl http://localhost:5000/api/itsm/dashboard/incident-trends \
  -H "Authorization: Bearer $TOKEN"
```

---

## 10. API Reference

### 10.1 Complete Endpoint Map

#### Incidents (`/api/itsm/incidents`)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/itsm/incidents` | List all incidents (paginated) |
| GET | `/api/itsm/incidents/{id}` | Get incident by ID |
| POST | `/api/itsm/incidents` | Create incident |
| PUT | `/api/itsm/incidents/{id}` | Update incident |
| DELETE | `/api/itsm/incidents/{id}` | Delete incident (soft) |
| POST | `/api/itsm/incidents/{id}/assign` | Assign to user/team |
| POST | `/api/itsm/incidents/{id}/escalate` | Escalate to higher tier |
| POST | `/api/itsm/incidents/{id}/resolve` | Mark as resolved |
| POST | `/api/itsm/incidents/{id}/close` | Close incident |
| POST | `/api/itsm/incidents/{id}/reopen` | Reopen a closed incident |
| GET | `/api/itsm/incidents/{id}/comments` | List comments |
| POST | `/api/itsm/incidents/{id}/comments` | Add a comment |

#### Problems (`/api/itsm/problems`)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/itsm/problems` | List all problems |
| GET | `/api/itsm/problems/{id}` | Get problem by ID |
| POST | `/api/itsm/problems` | Create problem |
| PUT | `/api/itsm/problems/{id}` | Update problem |
| DELETE | `/api/itsm/problems/{id}` | Delete problem (soft) |
| POST | `/api/itsm/problems/{id}/link-incident` | Link an incident |
| POST | `/api/itsm/problems/{id}/mark-known-error` | Mark as known error |
| GET | `/api/itsm/problems/{id}/related-incidents` | List linked incidents |
| POST | `/api/itsm/problems/{id}/root-cause` | Record root cause |

#### Changes (`/api/itsm/changes`)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/itsm/changes` | List all change requests |
| GET | `/api/itsm/changes/{id}` | Get change by ID |
| POST | `/api/itsm/changes` | Create change request |
| PUT | `/api/itsm/changes/{id}` | Update change request |
| DELETE | `/api/itsm/changes/{id}` | Delete change (soft) |
| POST | `/api/itsm/changes/{id}/submit-approval` | Submit for approval |
| POST | `/api/itsm/changes/{id}/approve` | Approve change |
| POST | `/api/itsm/changes/{id}/reject` | Reject change |
| POST | `/api/itsm/changes/{id}/schedule` | Schedule implementation |
| GET | `/api/itsm/changes/{id}/conflicts` | Check scheduling conflicts |
| GET | `/api/itsm/changes/{id}/impacted-cis` | List impacted CIs |
| GET | `/api/itsm/changes/blackout-periods` | List blackout periods |

#### CMDB (`/api/itsm/cmdb`)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/itsm/cmdb` | List all CIs |
| GET | `/api/itsm/cmdb/{id}` | Get CI by ID |
| POST | `/api/itsm/cmdb` | Create CI |
| PUT | `/api/itsm/cmdb/{id}` | Update CI |
| DELETE | `/api/itsm/cmdb/{id}` | Delete CI (soft) |
| GET | `/api/itsm/cmdb/search` | Search CIs |
| POST | `/api/itsm/cmdb/{id}/relationships` | Add CI relationship |
| GET | `/api/itsm/cmdb/{id}/related` | Get related CIs |
| GET | `/api/itsm/cmdb/{id}/impact-analysis` | Run impact analysis |

#### Knowledge (`/api/itsm/knowledge`)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/itsm/knowledge` | List all articles |
| GET | `/api/itsm/knowledge/{id}` | Get article by ID |
| POST | `/api/itsm/knowledge` | Create article |
| PUT | `/api/itsm/knowledge/{id}` | Update article |
| DELETE | `/api/itsm/knowledge/{id}` | Delete article (soft) |
| POST | `/api/itsm/knowledge/{id}/publish` | Publish article |
| POST | `/api/itsm/knowledge/{id}/retire` | Retire article |
| POST | `/api/itsm/knowledge/{id}/feedback` | Submit feedback |
| GET | `/api/itsm/knowledge/suggested` | Suggested articles |
| GET | `/api/itsm/knowledge/popular` | Popular articles |
| GET | `/api/itsm/knowledge/recent` | Recently published |
| GET | `/api/itsm/knowledge/categories` | Article categories |

#### Service Catalog (`/api/itsm/catalog`)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/itsm/catalog` | List catalog items |
| GET | `/api/itsm/catalog/{id}` | Get catalog item |
| GET | `/api/itsm/catalog/search` | Search catalog |
| GET | `/api/itsm/catalog/categories` | List categories |
| POST | `/api/itsm/catalog/requests` | Submit a request |
| GET | `/api/itsm/catalog/requests/{id}` | Get request status |
| POST | `/api/itsm/catalog/request-for-others` | Request for another user |
| POST | `/api/itsm/catalog/requests/{id}/cancel` | Cancel a request |

#### SLA (`/api/itsm/sla`)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/itsm/sla/policies` | List SLA policies |
| GET | `/api/itsm/sla/policies/{id}` | Get policy by ID |
| POST | `/api/itsm/sla/policies` | Create SLA policy |
| PUT | `/api/itsm/sla/policies/{id}` | Update SLA policy |
| DELETE | `/api/itsm/sla/policies/{id}` | Delete SLA policy |
| GET | `/api/itsm/sla/instances` | List SLA instances |
| POST | `/api/itsm/sla/instances/{id}/start` | Start SLA timer |
| POST | `/api/itsm/sla/instances/{id}/pause` | Pause SLA timer |
| POST | `/api/itsm/sla/instances/{id}/resume` | Resume SLA timer |
| POST | `/api/itsm/sla/instances/{id}/complete` | Complete SLA |
| GET | `/api/itsm/sla/breached` | List breached SLAs |
| GET | `/api/itsm/sla/at-risk` | List at-risk SLAs |
| GET | `/api/itsm/sla/dashboard` | SLA Dashboard summary |
| GET | `/api/itsm/sla/metrics` | SLA metrics |

#### Dashboard (`/api/itsm/dashboard`)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/itsm/dashboard/incident-trends` | Incident trend data |
| GET | `/api/itsm/dashboard/problem-analysis` | Problem analysis |
| GET | `/api/itsm/dashboard/change-metrics` | Change metrics |
| GET | `/api/itsm/dashboard/sla-compliance` | SLA compliance data |
| GET | `/api/itsm/dashboard/team-performance` | Team performance |
| GET | `/api/itsm/dashboard/service-health` | Service health overview |

---

## 11. Common Workflows & Best Practices

### 11.1 Incident → Problem → Change Workflow

This is the most common end-to-end ITSM workflow: a recurring incident leads to problem identification, which triggers a change to implement a permanent fix.

```
Step 1: Multiple incidents are logged for "Email server crashes"
        └── INC-101, INC-105, INC-112

Step 2: Support notices a pattern → Creates Problem PRB-201
        └── Links INC-101, INC-105, INC-112 to PRB-201

Step 3: Root cause analysis reveals a memory leak
        └── PRB-201 root cause recorded, marked as Known Error
        └── Workaround documented → Knowledge Article KBA-301 created

Step 4: Permanent fix requires a software upgrade
        └── Change Request CHG-401 created, linked to PRB-201
        └── CHG-401 submitted for approval → approved → scheduled

Step 5: Change implemented during maintenance window
        └── CHG-401 completed → PRB-201 resolved → KBA-301 updated
```

### 11.2 Best Practices

#### Incident Management
- **Categorize accurately** — correct categorization improves routing and reporting.
- **Set priority using Impact × Urgency** — don't inflate priority; use the matrix.
- **Link to known errors** — check the Knowledge Base before deep investigation.
- **Update the incident regularly** — stakeholders should see progress via comments.
- **Capture resolution details** — this feeds the Knowledge Base and trend analysis.

#### Problem Management
- **Don't create a problem for every incident** — wait for a pattern or high-impact event.
- **Document the workaround first** — a quick workaround reduces future incident resolution time.
- **Link all related incidents** — this tracks the true impact of the problem.
- **Create a Knowledge Article** when a known error is documented.

#### Change Management
- **Include a rollback plan** — every change request must have a tested rollback.
- **Check for conflicts** — use the conflict-checking API before scheduling.
- **Respect blackout periods** — don't schedule changes during code freezes.
- **Link impacted CIs** — this ensures the CMDB stays accurate.
- **Post-implementation review** — log whether the change succeeded or failed.

#### CMDB
- **Keep CI data current** — stale CMDB data undermines impact analysis.
- **Model relationships** — the value of a CMDB comes from knowing dependencies.
- **Review after every change** — update the CMDB as part of the change closure process.

#### Knowledge Base
- **Write for the audience** — internal KB articles can be technical; customer-facing ones should be simple.
- **Review and retire** — schedule periodic reviews to retire outdated content.
- **Use feedback** — low-rated articles need revision; high-rated articles should be promoted.

#### SLA Management
- **Define SLAs per priority** — different priorities require different response/resolution targets.
- **Use business hours** — SLA clocks should stop outside business hours unless 24/7.
- **Pause on customer wait** — pause the SLA timer when you're waiting for the customer.
- **Monitor at-risk items daily** — act before breaches occur.

---

## Related Documentation

| Document | Description |
|----------|-------------|
| [README.md](../README.md) | Solution overview with ITSM section |
| [ITSM Implementation Status](docs/status/ITSM_IMPLEMENTATION_STATUS.md) | Technical implementation tracking |
| [Solution Gaps Remediation Plan](docs/development/SOLUTION_GAPS_REMEDIATION_PLAN.md) | Overall remediation progress |
| [Specification Index](specifications/INDEX.md) | Feature specifications |
| [Architecture Overview](docs/development/ARCHITECTURE_OVERVIEW.md) | System architecture |
| [API Reference (Swagger)](http://localhost:5000/swagger) | Interactive API documentation |

---

**END OF ITSM USER GUIDE**

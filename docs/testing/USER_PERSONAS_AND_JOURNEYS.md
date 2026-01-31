# CRM Solution - User Personas, Journeys & Test Cases

**Version:** 0.0.26  
**Date:** January 2026  
**Document Type:** Test Specification & User Journey Documentation

---

## Table of Contents
1. [User Personas](#1-user-personas)
2. [Tasks & Workflows by Persona](#2-tasks--workflows-by-persona)
3. [Entry Points & Views](#3-entry-points--views)
4. [End-to-End User Journeys](#4-end-to-end-user-journeys)
5. [Test Case Matrix](#5-test-case-matrix)

---

## 1. User Personas

### 1.1 Sales Representative (UserRole: Sales)
| Attribute | Details |
|-----------|---------|
| **Role ID** | Sales (2) |
| **Department** | SALES |
| **Primary Goal** | Close deals and manage customer relationships |
| **Access Level** | Customers, Contacts, Leads, Opportunities, Quotes, Products |
| **Example User** | Mary Wilson (mwilson@company.com) |

### 1.2 Sales Manager (UserRole: Manager)
| Attribute | Details |
|-----------|---------|
| **Role ID** | Manager (1) |
| **Department** | SALES |
| **Primary Goal** | Oversee sales team, review pipeline, approve quotes |
| **Access Level** | All Sales + Reports, Dashboard, Team Management |
| **Example User** | John Smith (john.smith@company.com) |

### 1.3 Marketing Manager (UserRole: Manager)
| Attribute | Details |
|-----------|---------|
| **Role ID** | Manager (1) |
| **Department** | MKTG |
| **Primary Goal** | Execute campaigns, generate leads, track ROI |
| **Access Level** | Leads, Campaigns, Communications, Reports |
| **Example User** | Sarah Brown (sarah.brown@company.com) |

### 1.4 Support Agent (UserRole: Support)
| Attribute | Details |
|-----------|---------|
| **Role ID** | Support (3) |
| **Department** | SUPP |
| **Primary Goal** | Resolve customer issues, manage service requests |
| **Access Level** | Service Requests, Knowledge Base, Customer View |
| **Example User** | Mike Garcia (mike.garcia@company.com) |

### 1.5 System Administrator (UserRole: Admin)
| Attribute | Details |
|-----------|---------|
| **Role ID** | Admin (0) |
| **Department** | IT/ADMIN |
| **Primary Goal** | Configure system, manage users, monitor health |
| **Access Level** | Full System Access |
| **Example User** | Admin (abhi.lal@gmail.com) |

### 1.6 Guest/Read-Only User (UserRole: Guest)
| Attribute | Details |
|-----------|---------|
| **Role ID** | Guest (4) |
| **Department** | Any |
| **Primary Goal** | View reports, read-only access |
| **Access Level** | Dashboard, Read-only views |
| **Example User** | External Stakeholder |

---

## 2. Tasks & Workflows by Persona

### 2.1 Sales Representative Tasks

| Task ID | Task Name | Workflow | Priority |
|---------|-----------|----------|----------|
| SR-001 | Capture new lead | Lead → Qualify → Convert | High |
| SR-002 | Create customer | New Customer Form → Save | High |
| SR-003 | Log interaction | Customer → Add Interaction | Medium |
| SR-004 | Create opportunity | Customer → New Opportunity | High |
| SR-005 | Update opportunity stage | Opportunity → Edit Stage | High |
| SR-006 | Generate quote | Opportunity → Create Quote | High |
| SR-007 | Send quote for signature | Quote → E-Signature | Medium |
| SR-008 | Add product to quote | Quote → Add Line Items | High |
| SR-009 | View my tasks | My Queue → Task List | High |
| SR-010 | Complete task | Task → Mark Complete | Medium |
| SR-011 | Add note to record | Entity → Notes Tab → Add | Low |
| SR-012 | Search for customer | Search Bar → Results | High |

### 2.2 Sales Manager Tasks

| Task ID | Task Name | Workflow | Priority |
|---------|-----------|----------|----------|
| SM-001 | View sales dashboard | Dashboard → Sales Metrics | High |
| SM-002 | Review pipeline | Opportunities → Pipeline View | High |
| SM-003 | Approve quote discount | Quote → Approval Workflow | High |
| SM-004 | Assign lead to rep | Lead → Assign Owner | Medium |
| SM-005 | View team performance | Reports → Team Dashboard | Medium |
| SM-006 | Forecast revenue | Dashboard → Forecast Widget | High |
| SM-007 | Review overdue tasks | My Queue → Overdue Filter | Medium |

### 2.3 Marketing Manager Tasks

| Task ID | Task Name | Workflow | Priority |
|---------|-----------|----------|----------|
| MM-001 | Create marketing campaign | Campaigns → New Campaign | High |
| MM-002 | Add campaign recipients | Campaign → Target List | High |
| MM-003 | Execute campaign | Campaign → Start Execution | High |
| MM-004 | Track campaign metrics | Campaign → Analytics | Medium |
| MM-005 | Import leads | Leads → Import CSV | Medium |
| MM-006 | Score leads (AI) | Leads → AI Scoring | Medium |
| MM-007 | View conversion rates | Reports → Campaign ROI | Medium |
| MM-008 | Send email communication | Communications → New Email | High |

### 2.4 Support Agent Tasks

| Task ID | Task Name | Workflow | Priority |
|---------|-----------|----------|----------|
| SA-001 | View service queue | Service Requests → Queue | High |
| SA-002 | Create service request | Service Requests → New | High |
| SA-003 | Update request status | Request → Change Status | High |
| SA-004 | Escalate request | Request → Escalate | Medium |
| SA-005 | Search knowledge base | Knowledge Base → Search | Medium |
| SA-006 | Create KB article | Knowledge Base → New Article | Low |
| SA-007 | Log customer interaction | Interactions → New | High |
| SA-008 | View customer history | Customer → 360 View | Medium |

### 2.5 System Administrator Tasks

| Task ID | Task Name | Workflow | Priority |
|---------|-----------|----------|----------|
| AD-001 | Create new user | Admin → Users → Add | High |
| AD-002 | Approve pending user | Admin → Approvals | High |
| AD-003 | Manage user groups | Admin → Groups | Medium |
| AD-004 | Configure features | Admin → Features | Medium |
| AD-005 | Set up workflows | Admin → Workflows → Designer | Medium |
| AD-006 | Configure LLM provider | Admin → LLM Settings | Low |
| AD-007 | Monitor system health | Admin → Monitoring | High |
| AD-008 | Import master data | Admin → Master Data | Low |
| AD-009 | Manage branding | Admin → Branding | Low |
| AD-010 | Configure security | Admin → Security | High |

---

## 3. Entry Points & Views

### 3.1 Navigation Structure by Role

| Page | Sales Rep | Sales Mgr | Mktg Mgr | Support | Admin |
|------|-----------|-----------|----------|---------|-------|
| Dashboard | ✅ | ✅ | ✅ | ✅ | ✅ |
| My Queue (Tasks) | ✅ | ✅ | ✅ | ✅ | ✅ |
| Customers | ✅ | ✅ | 👁️ | 👁️ | ✅ |
| Contacts | ✅ | ✅ | ✅ | 👁️ | ✅ |
| Leads | ✅ | ✅ | ✅ | ❌ | ✅ |
| Opportunities | ✅ | ✅ | 👁️ | ❌ | ✅ |
| Quotes | ✅ | ✅ | ❌ | ❌ | ✅ |
| Products | ✅ | ✅ | 👁️ | ❌ | ✅ |
| Campaigns | ❌ | 👁️ | ✅ | ❌ | ✅ |
| Service Requests | ❌ | ❌ | ❌ | ✅ | ✅ |
| Knowledge Base | 👁️ | 👁️ | 👁️ | ✅ | ✅ |
| Reports | 👁️ | ✅ | ✅ | 👁️ | ✅ |
| Admin Settings | ❌ | ❌ | ❌ | ❌ | ✅ |

**Legend:** ✅ Full Access | 👁️ Read Only | ❌ No Access

### 3.2 Key Views per Persona

#### Sales Representative Views
1. **My Queue** - Personal task list with due dates
2. **Customer List** - All assigned customers
3. **Opportunity Pipeline** - Kanban board view
4. **Quote Builder** - CPQ interface with products

#### Sales Manager Views
1. **Sales Dashboard** - KPIs, pipeline, forecasts
2. **Team Pipeline** - All opportunities by rep
3. **Approval Queue** - Pending discount approvals
4. **Performance Reports** - Team metrics

#### Marketing Manager Views
1. **Campaign Dashboard** - Active campaigns, metrics
2. **Lead Funnel** - Lead stages and conversion
3. **Email Analytics** - Open rates, clicks
4. **Attribution Reports** - Campaign ROI

#### Support Agent Views
1. **Service Queue** - Ticket list by priority
2. **Customer 360** - Full customer history
3. **Knowledge Base** - Searchable articles
4. **SLA Dashboard** - Response time metrics

#### System Administrator Views
1. **Monitoring Dashboard** - System health
2. **User Management** - All users and roles
3. **Workflow Designer** - Visual workflow builder
4. **Feature Toggles** - Enable/disable features

---

## 4. End-to-End User Journeys

### Journey 1: Lead-to-Cash (Sales Rep)

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                        LEAD-TO-CASH JOURNEY                                   │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────┐    ┌─────────┐    ┌──────────┐    ┌───────┐    ┌─────────┐     │
│  │  Lead   │───▶│ Qualify │───▶│ Customer │───▶│ Opp   │───▶│  Quote  │     │
│  │ Capture │    │  Lead   │    │  Create  │    │Create │    │ Builder │     │
│  └─────────┘    └─────────┘    └──────────┘    └───────┘    └─────────┘     │
│       │              │              │              │              │          │
│       ▼              ▼              ▼              ▼              ▼          │
│  [API: POST     [API: PUT      [API: POST    [API: POST    [API: POST       │
│   /api/leads]   /api/leads/    /customers]   /opportunities]/quotes]        │
│                  {id}/qualify]                                               │
│                                                                              │
│  ┌─────────┐    ┌─────────┐    ┌──────────┐    ┌───────────┐               │
│  │  Send   │───▶│E-Sign   │───▶│  Order   │───▶│  Invoice  │               │
│  │ Quote   │    │Complete │    │  Create  │    │  Payment  │               │
│  └─────────┘    └─────────┘    └──────────┘    └───────────┘               │
│       │              │              │              │                         │
│       ▼              ▼              ▼              ▼                         │
│  [API: POST     [Webhook:     [API: POST    [API: POST                      │
│   /quotes/      /esign/       /orders]      /invoices]                      │
│   {id}/send]    callback]                                                    │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

**Steps:**
1. **Lead Capture** → Sales rep receives lead (manual or from campaign)
2. **Qualify Lead** → Assess fit, budget, authority, need, timeline
3. **Convert to Customer** → Create customer record from qualified lead
4. **Create Opportunity** → Define deal value, expected close date
5. **Build Quote** → Add products, apply pricing, discounts
6. **Send for Signature** → E-signature workflow
7. **Create Order** → Convert accepted quote to order
8. **Invoice & Payment** → Generate invoice, track payment

---

### Journey 2: Campaign-to-Lead (Marketing Manager)

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                      CAMPAIGN-TO-LEAD JOURNEY                                 │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐              │
│  │ Campaign │───▶│  Target  │───▶│ Execute  │───▶│  Track   │              │
│  │  Create  │    │   List   │    │ Campaign │    │ Response │              │
│  └──────────┘    └──────────┘    └──────────┘    └──────────┘              │
│       │               │               │               │                      │
│       ▼               ▼               ▼               ▼                      │
│  [POST /api/    [POST /api/    [POST /api/    [GET /api/                    │
│   campaigns]    campaigns/     campaigns/     campaigns/                    │
│                 {id}/recipients]{id}/execute]  {id}/metrics]                 │
│                                                                              │
│  ┌──────────┐    ┌──────────┐    ┌──────────┐                               │
│  │ Capture  │───▶│AI Score  │───▶│  Assign  │                               │
│  │  Leads   │    │  Leads   │    │ to Sales │                               │
│  └──────────┘    └──────────┘    └──────────┘                               │
│       │               │               │                                      │
│       ▼               ▼               ▼                                      │
│  [POST /api/    [POST /api/    [PUT /api/                                   │
│   leads]        leads/score]   leads/{id}]                                  │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

**Steps:**
1. **Create Campaign** → Define campaign type, budget, dates
2. **Build Target List** → Select contacts/leads for campaign
3. **Execute Campaign** → Send emails, track engagement
4. **Track Responses** → Monitor opens, clicks, conversions
5. **Capture Leads** → Create lead records from responses
6. **AI Score Leads** → Allen AI ranks leads by quality
7. **Assign to Sales** → Route qualified leads to sales reps

---

### Journey 3: Issue-to-Resolution (Support Agent)

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                     ISSUE-TO-RESOLUTION JOURNEY                               │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐              │
│  │ Receive  │───▶│ Triage & │───▶│Investigate│───▶│ Resolve  │              │
│  │ Request  │    │ Assign   │    │  Issue   │    │  Issue   │              │
│  └──────────┘    └──────────┘    └──────────┘    └──────────┘              │
│       │               │               │               │                      │
│       ▼               ▼               ▼               ▼                      │
│  [POST /api/    [PUT /api/     [GET /api/     [PUT /api/                    │
│   service-      service-       knowledge-     service-                      │
│   requests]     requests/{id}] base/search]   requests/{id}]                │
│                                                                              │
│  ┌──────────┐    ┌──────────┐    ┌──────────┐                               │
│  │ Customer │───▶│  Close   │───▶│  Update  │                               │
│  │ Confirm  │    │ Request  │    │    KB    │                               │
│  └──────────┘    └──────────┘    └──────────┘                               │
│       │               │               │                                      │
│       ▼               ▼               ▼                                      │
│  [Log          [PUT /api/     [POST /api/                                   │
│   Interaction] service-       knowledge-                                    │
│                requests/{id}] base/articles]                                 │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

**Steps:**
1. **Receive Request** → Customer submits issue via portal/email
2. **Triage & Assign** → Categorize, prioritize, assign to agent
3. **Investigate Issue** → Search KB, review customer history
4. **Resolve Issue** → Apply fix, update status
5. **Customer Confirmation** → Get customer acknowledgment
6. **Close Request** → Complete resolution, log time
7. **Update KB** → Document solution for future reference

---

### Journey 4: User Onboarding (System Admin)

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                      USER ONBOARDING JOURNEY                                  │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐              │
│  │  Create  │───▶│  Assign  │───▶│  Assign  │───▶│  Verify  │              │
│  │   User   │    │   Role   │    │  Profile │    │  Access  │              │
│  └──────────┘    └──────────┘    └──────────┘    └──────────┘              │
│       │               │               │               │                      │
│       ▼               ▼               ▼               ▼                      │
│  [POST /api/    [PUT /api/     [PUT /api/     [GET /api/                    │
│   users]        users/{id}]    users/{id}]    users/{id}/                   │
│                                               permissions]                   │
│                                                                              │
│  ┌──────────┐    ┌──────────┐                                               │
│  │  Send    │───▶│  User    │                                               │
│  │ Welcome  │    │  Login   │                                               │
│  └──────────┘    └──────────┘                                               │
│       │               │                                                      │
│       ▼               ▼                                                      │
│  [Email        [POST /api/                                                  │
│   Notification] auth/login]                                                  │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

### Journey 5: Opportunity Pipeline Management (Sales Manager)

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                    PIPELINE MANAGEMENT JOURNEY                                │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐              │
│  │  View    │───▶│ Identify │───▶│  Coach   │───▶│ Approve  │              │
│  │ Pipeline │    │  Risks   │    │   Rep    │    │ Discount │              │
│  └──────────┘    └──────────┘    └──────────┘    └──────────┘              │
│       │               │               │               │                      │
│       ▼               ▼               ▼               ▼                      │
│  [GET /api/     [GET /api/     [POST /api/    [PUT /api/                    │
│   opportunities  opportunities/ tasks]        quotes/{id}/                  │
│   /pipeline]     at-risk]                     approve]                      │
│                                                                              │
│  ┌──────────┐    ┌──────────┐                                               │
│  │ Generate │───▶│  Export  │                                               │
│  │ Forecast │    │  Report  │                                               │
│  └──────────┘    └──────────┘                                               │
│       │               │                                                      │
│       ▼               ▼                                                      │
│  [GET /api/     [GET /api/                                                  │
│   forecasts]    reports/                                                    │
│                 export]                                                      │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## 5. Test Case Matrix

### 5.1 API Test Cases

| Test ID | Persona | Journey | Endpoint | Method | Expected |
|---------|---------|---------|----------|--------|----------|
| API-001 | All | Auth | /api/auth/login | POST | 200 + Token |
| API-002 | All | Auth | /api/auth/logout | POST | 200 |
| API-003 | Sales | Lead Capture | /api/leads | POST | 201 + Lead ID |
| API-004 | Sales | Lead Qualify | /api/leads/{id}/qualify | PUT | 200 |
| API-005 | Sales | Customer Create | /api/customers | POST | 201 |
| API-006 | Sales | Opportunity Create | /api/opportunities | POST | 201 |
| API-007 | Sales | Quote Create | /api/quotes | POST | 201 |
| API-008 | Sales | Quote Add Line | /api/quotes/{id}/lines | POST | 201 |
| API-009 | Sales | Quote Send | /api/quotes/{id}/send | POST | 200 |
| API-010 | Mktg | Campaign Create | /api/campaigns | POST | 201 |
| API-011 | Mktg | Campaign Execute | /api/campaigns/{id}/execute | POST | 200 |
| API-012 | Mktg | Lead Import | /api/leads/import | POST | 200 |
| API-013 | Mktg | Lead Score | /api/leads/score | POST | 200 |
| API-014 | Support | Request Create | /api/service-requests | POST | 201 |
| API-015 | Support | Request Update | /api/service-requests/{id} | PUT | 200 |
| API-016 | Support | KB Search | /api/knowledge-base/search | GET | 200 |
| API-017 | Admin | User Create | /api/users | POST | 201 |
| API-018 | Admin | User Approve | /api/users/{id}/approve | PUT | 200 |
| API-019 | Admin | Feature Toggle | /api/settings/features | PUT | 200 |
| API-020 | All | Health Check | /health | GET | 200 |

### 5.2 Frontend E2E Test Cases

| Test ID | Persona | Journey | Page | Action | Expected |
|---------|---------|---------|------|--------|----------|
| E2E-001 | All | Auth | Login | Valid login | Dashboard loads |
| E2E-002 | All | Auth | Login | Invalid login | Error message |
| E2E-003 | Sales | Lead | Leads Page | Create lead | Lead in list |
| E2E-004 | Sales | Lead | Lead Detail | Qualify lead | Status updated |
| E2E-005 | Sales | Customer | Customers | Create customer | Customer in list |
| E2E-006 | Sales | Opportunity | Opportunities | Create opportunity | Opp in pipeline |
| E2E-007 | Sales | Quote | Quote Builder | Add products | Total calculated |
| E2E-008 | Mktg | Campaign | Campaigns | Create campaign | Campaign in list |
| E2E-009 | Mktg | Campaign | Campaign Detail | Execute | Status running |
| E2E-010 | Support | Service | Service Requests | Create request | Request in queue |
| E2E-011 | Support | Service | Request Detail | Update status | Status changed |
| E2E-012 | Admin | Users | User Mgmt | Create user | User in list |
| E2E-013 | Admin | Settings | Features | Toggle feature | Feature state changes |
| E2E-014 | All | Navigation | Dashboard | Click all nav | No errors |
| E2E-015 | Sales | Search | Global Search | Search customer | Results displayed |

---

## Appendix A: Test Data Requirements

### Test Users
| Username | Email | Role | Department |
|----------|-------|------|------------|
| admin | abhi.lal@gmail.com | Admin | IT |
| jsmith | john.smith@company.com | Manager | SALES |
| mwilson | mary.wilson@company.com | Sales | SALES |
| sbrown | sarah.brown@company.com | Manager | MKTG |
| mgarcia | mike.garcia@company.com | Support | SUPP |
| guest | guest@company.com | Guest | - |

### Test Data Sets
- 10+ Customers with varied data
- 20+ Contacts linked to customers
- 15+ Leads at different stages
- 10+ Opportunities in pipeline
- 5+ Campaigns with recipients
- 10+ Service Requests

---

*Document Generated: January 2026*

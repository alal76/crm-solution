#!/usr/bin/env python3
"""Batch 07: Service Desk & ITSM.

Covers: ServiceRequests, ServiceRequestCategories, Incidents, Problems, Changes,
ChangeTypes, KnowledgeArticles, SLAPolicies, EscalationRules, EscalationPolicies,
CITypes, CMDB (ConfigurationItems), IncidentCategories, ITSMDashboard.
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, ENUMS, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 07: Service Desk & ITSM")
    ts = int(time.time())
    acct_ids = load_ids("accounts")
    contact_ids = load_ids("contacts")
    user_ids = load_ids("users")

    # ---- Service Request Categories (read) ----
    log.section("ServiceRequest Settings")
    api.get("/api/service-request-settings/categories")
    api.get("/api/service-request-settings/types")

    # ---- Service Requests ----
    log.section("ServiceRequests CRUD")
    requests = [
        {"title": f"Cannot login to CRM {ts}", "description": "User reports login failure",
         "priority": 2, "status": 0, "type": "Bug",
         "contactId": contact_ids[0] if contact_ids else None,
         "accountId": acct_ids[0] if acct_ids else None},
        {"title": f"Feature request: Export to PDF {ts}", "description": "Need PDF export for reports",
         "priority": 1, "status": 0, "type": "FeatureRequest"},
        {"title": f"Performance issue on dashboard {ts}", "description": "Dashboard loading slowly",
         "priority": 3, "status": 0, "type": "Bug",
         "accountId": acct_ids[1] if len(acct_ids) > 1 else None},
    ]
    sr_ids = []
    for r in requests:
        payload = {k: v for k, v in r.items() if v is not None}
        eid = api.create_and_track("servicerequests", "/api/servicerequests", payload)
        if eid:
            sr_ids.append(eid)
    api.get("/api/servicerequests")
    if sr_ids:
        api.get(f"/api/servicerequests/{sr_ids[0]}")
        api.put(f"/api/servicerequests/{sr_ids[0]}", {**requests[0], "status": 2,
                                                       "description": "Investigating login issue"})
    # Delete test
    del_sr = {"title": f"DELETE-SR-{ts}", "description": "To be deleted", "priority": 0, "status": 0, "type": "Bug"}
    code, body, _ = api.post("/api/servicerequests", del_sr)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/servicerequests/{body['id']}")
    save_ids("servicerequests", sr_ids)

    # ---- CI Types ----
    log.section("CITypes CRUD")
    ci_type = {"typeName": f"WebServer-{ts}", "description": "Web server configuration item",
               "typeCategory": "Infrastructure", "icon": "server"}
    eid = api.create_and_track("citypes", "/api/ci-types", ci_type)
    if eid:
        api.get(f"/api/ci-types/{eid}")
        api.put(f"/api/ci-types/{eid}", {**ci_type, "description": "Updated web server CI"})
        save_ids("citypes", [eid])
    api.get("/api/ci-types")
    api.get("/api/ci-types/categories")

    # ---- Incident Categories ----
    log.section("IncidentCategories CRUD")
    ic = {"name": f"Network Issue {ts}", "description": "Network-related incidents"}
    eid = api.create_and_track("incidentcategories", "/api/incident-categories", ic)
    if eid:
        api.get(f"/api/incident-categories/{eid}")
        save_ids("incidentcategories", [eid])
    api.get("/api/incident-categories")

    # ---- Incidents ----
    log.section("Incidents CRUD")
    incidents = [
        {"shortDescription": f"Server outage - Production {ts}",
         "callerId": user_ids[0] if user_ids else 1,
         "impact": 1, "urgency": 1,
         "category": "Infrastructure"},
        {"shortDescription": f"Email service degraded {ts}",
         "callerId": user_ids[0] if user_ids else 1,
         "impact": 2, "urgency": 2,
         "category": "Application"},
    ]
    incident_ids = []
    for i in incidents:
        eid = api.create_and_track("incidents", "/api/itsm/incidents", i)
        if eid:
            incident_ids.append(eid)
    api.get("/api/itsm/incidents")
    if incident_ids:
        api.get(f"/api/itsm/incidents/{incident_ids[0]}")
        api.put(f"/api/itsm/incidents/{incident_ids[0]}", {**incidents[0],
                                                       "shortDescription": f"Server outage - Production {ts} (investigating)"})
    save_ids("incidents", incident_ids)

    # ---- Problems ----
    log.section("Problems CRUD")
    problems = [
        {"title": f"Recurring server crashes {ts}", "description": "Memory leak causing periodic crashes",
         "urgency": "High", "impact": "High"},
        {"title": f"Intermittent DB timeouts {ts}", "description": "Database connection pool exhaustion",
         "urgency": "Medium", "impact": "Medium"},
    ]
    problem_ids = []
    for p in problems:
        eid = api.create_and_track("problems", "/api/problems", p)
        if eid:
            problem_ids.append(eid)
    api.get("/api/problems")
    if problem_ids:
        api.get(f"/api/problems/{problem_ids[0]}")
        api.put(f"/api/problems/{problem_ids[0]}", {"title": problems[0]["title"],
                                                     "description": "Root cause analysis in progress",
                                                     "urgency": "High", "impact": "High"})
    save_ids("problems", problem_ids)

    # ---- Change Types ----
    log.section("ChangeTypes CRUD")
    ct = {"typeName": f"EmergencyChange-{ts}", "description": "Emergency change type",
          "requiresApproval": True, "requiresCAB": True,
          "defaultRiskLevel": "High", "leadTimeDays": 0, "isActive": True}
    eid = api.create_and_track("changetypes", "/api/change-types", ct)
    if eid:
        api.get(f"/api/change-types/{eid}")
        save_ids("changetypes", [eid])
    api.get("/api/change-types")

    # ---- Changes ----
    log.section("Changes CRUD")
    change = {"title": f"Upgrade database to MariaDB 11 {ts}",
              "description": "Planned database upgrade for performance",
              "status": 0, "priority": 2, "riskLevel": "Medium",
              "plannedStartDate": "2026-03-01T00:00:00Z",
              "plannedEndDate": "2026-03-01T06:00:00Z",
              "implementationPlan": "1. Backup 2. Upgrade 3. Verify 4. Cutover"}
    eid = api.create_and_track("changes", "/api/changes", change)
    if eid:
        api.get(f"/api/changes/{eid}")
        api.put(f"/api/changes/{eid}", {**change, "status": "Planned"})
        # Lifecycle: submit, approve
        api.post(f"/api/changes/{eid}/submit")
        api.post(f"/api/changes/{eid}/approve", {"approverNotes": "Approved by admin"})
        save_ids("changes", [eid])
    api.get("/api/changes")

    # ---- Knowledge Articles ----
    log.section("KnowledgeArticles CRUD")
    articles = [
        {"title": f"How to reset your password {ts}",
         "articleBody": "Step 1: Click Forgot Password... Step 2: Check email... Step 3: Set new password",
         "category": "Account", "articleType": 1, "status": 0, "isPublished": True},
        {"title": f"VPN Setup Guide {ts}",
         "articleBody": "Download the VPN client from... Configure with server address...",
         "category": "Network", "articleType": 1, "status": 0, "isPublished": True},
    ]
    article_ids = []
    for a in articles:
        eid = api.create_and_track("knowledgearticles", "/api/itsm/knowledge", a)
        if eid:
            article_ids.append(eid)
    api.get("/api/itsm/knowledge/articles")
    if article_ids:
        api.get(f"/api/itsm/knowledge/{article_ids[0]}")
    save_ids("knowledgearticles", article_ids)

    # ---- Escalation Rules ----
    log.section("EscalationRules CRUD")
    er = {"name": f"Critical-Escalation-{ts}", "description": "Escalate critical tickets in 1 hour",
          "isActive": True, "priority": "Critical", "ageInMinutes": 60,
          "targetType": "User", "targetName": "On-Call Engineer",
          "maxAttempts": 3, "retryIntervalMinutes": 15}
    eid = api.create_and_track("escalationrules", "/api/escalation-rules", er)
    if eid:
        api.get(f"/api/escalation-rules/{eid}")
        save_ids("escalationrules", [eid])
    api.get("/api/escalation-rules")

    # ---- Escalation Policies ----
    log.section("EscalationPolicies CRUD")
    ep = {"name": f"P1-Escalation-Policy-{ts}", "description": "Priority 1 escalation",
          "isActive": True}
    eid = api.create_and_track("escalationpolicies", "/api/itsm/escalation-policies", ep)
    if eid:
        api.get(f"/api/itsm/escalation-policies/{eid}")
        save_ids("escalationpolicies", [eid])
    api.get("/api/itsm/escalation-policies")

    # ---- ITSM Dashboard ----
    log.section("ITSM Dashboard")
    # No base GET /api/itsm/dashboard exists; use sub-paths
    api.get("/api/itsm/dashboard/metrics")
    api.get("/api/itsm/dashboard/executive-summary")

    print(f"  Batch 07 done: {log.summary_line()}")

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
    api.get("/api/servicerequestsettings/categories")
    api.get("/api/servicerequestsettings/types")

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
    ci_type = {"name": f"WebServer-{ts}", "description": "Web server configuration item",
               "category": "Infrastructure", "icon": "server"}
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
    eid = api.create_and_track("incidentcategories", "/api/incidentcategories", ic)
    if eid:
        api.get(f"/api/incidentcategories/{eid}")
        save_ids("incidentcategories", [eid])
    api.get("/api/incidentcategories")

    # ---- Incidents ----
    log.section("Incidents CRUD")
    incidents = [
        {"title": f"Server outage - Production {ts}", "description": "Main production server down",
         "impact": 1, "urgency": 1, "status": 0,
         "category": "Infrastructure"},
        {"title": f"Email service degraded {ts}", "description": "Email delivery delays",
         "impact": 2, "urgency": 2, "status": 0,
         "category": "Application"},
    ]
    incident_ids = []
    for i in incidents:
        eid = api.create_and_track("incidents", "/api/incidents", i)
        if eid:
            incident_ids.append(eid)
    api.get("/api/incidents")
    if incident_ids:
        api.get(f"/api/incidents/{incident_ids[0]}")
        api.put(f"/api/incidents/{incident_ids[0]}", {**incidents[0], "status": 1,
                                                       "description": "Investigating server outage"})
    save_ids("incidents", incident_ids)

    # ---- Problems ----
    log.section("Problems CRUD")
    problems = [
        {"title": f"Recurring server crashes {ts}", "description": "Memory leak causing periodic crashes",
         "priority": 1, "status": 0, "category": "Infrastructure"},
        {"title": f"Intermittent DB timeouts {ts}", "description": "Database connection pool exhaustion",
         "priority": 2, "status": 0, "category": "Database"},
    ]
    problem_ids = []
    for p in problems:
        eid = api.create_and_track("problems", "/api/problems", p)
        if eid:
            problem_ids.append(eid)
    api.get("/api/problems")
    if problem_ids:
        api.get(f"/api/problems/{problem_ids[0]}")
        api.put(f"/api/problems/{problem_ids[0]}", {**problems[0], "status": 1,
                                                     "description": "Root cause analysis in progress"})
    save_ids("problems", problem_ids)

    # ---- Change Types ----
    log.section("ChangeTypes CRUD")
    ct = {"name": f"EmergencyChange-{ts}", "description": "Emergency change type",
          "approvalRequired": True, "riskLevel": "High"}
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
        api.put(f"/api/changes/{eid}", {**change, "status": 1})
        # Lifecycle: submit, approve
        api.post(f"/api/changes/{eid}/submit")
        api.post(f"/api/changes/{eid}/approve")
        save_ids("changes", [eid])
    api.get("/api/changes")

    # ---- Knowledge Articles ----
    log.section("KnowledgeArticles CRUD")
    articles = [
        {"title": f"How to reset your password {ts}",
         "content": "Step 1: Click Forgot Password... Step 2: Check email... Step 3: Set new password",
         "category": "Account", "articleType": 1, "status": 0, "isPublished": True},
        {"title": f"VPN Setup Guide {ts}",
         "content": "Download the VPN client from... Configure with server address...",
         "category": "Network", "articleType": 1, "status": 0, "isPublished": True},
    ]
    article_ids = []
    for a in articles:
        eid = api.create_and_track("knowledgearticles", "/api/knowledgearticles", a)
        if eid:
            article_ids.append(eid)
    api.get("/api/knowledgearticles")
    if article_ids:
        api.get(f"/api/knowledgearticles/{article_ids[0]}")
    save_ids("knowledgearticles", article_ids)

    # ---- Escalation Rules ----
    log.section("EscalationRules CRUD")
    er = {"name": f"Critical-Escalation-{ts}", "description": "Escalate critical tickets in 1 hour",
          "isActive": True, "priority": 3, "timeThresholdMinutes": 60}
    eid = api.create_and_track("escalationrules", "/api/escalationrules", er)
    if eid:
        api.get(f"/api/escalationrules/{eid}")
        save_ids("escalationrules", [eid])
    api.get("/api/escalationrules")

    # ---- Escalation Policies ----
    log.section("EscalationPolicies CRUD")
    ep = {"name": f"P1-Escalation-Policy-{ts}", "description": "Priority 1 escalation",
          "isActive": True}
    eid = api.create_and_track("escalationpolicies", "/api/escalationpolicies", ep)
    if eid:
        api.get(f"/api/escalationpolicies/{eid}")
        save_ids("escalationpolicies", [eid])
    api.get("/api/escalationpolicies")

    # ---- ITSM Dashboard ----
    log.section("ITSM Dashboard")
    api.get("/api/itsm/dashboard")
    api.get("/api/itsm/dashboard/summary")

    print(f"  Batch 07 done: {log.summary_line()}")

#!/usr/bin/env python3
"""Batch 15: Service Desk Configuration.

Covers full CRUD for entities that Batch 07 only read/listed:
  - ServiceRequestCategories  (POST/PUT/DELETE via /api/service-request-settings/categories)
  - ServiceRequestTypes       (GET/POST/PUT/DELETE via /api/service-request-settings/types)
  - SLA Policies              (/api/slapolicies)
  - Service Queues            (/api/servicequeues)
  - Auto-Assignment Rules     (/api/autoassignment/rules)
  - ITSM Escalation Rules     (/api/itsm/escalationrules)
  - Escalation Analytics      (/api/escalationanalytics — read only)
  - ITSM Webhooks             (/api/itsm/webhooks — read only)
  - Business Hours Config     (/api/system/business-hours)
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 15: Service Desk Configuration")
    ts = int(time.time())

    # ─── Service Request Categories — full CRUD ────────────────────────────
    log.section("ServiceRequestCategories CRUD")
    categories = [
        {"name": f"Hardware Issues {ts}", "description": "Physical hardware problems",
         "color": "#e74c3c", "icon": "hardware", "isActive": True},
        {"name": f"Software Issues {ts}", "description": "Application and OS problems",
         "color": "#3498db", "icon": "software", "isActive": True},
        {"name": f"Network Issues {ts}", "description": "Connectivity and network problems",
         "color": "#2ecc71", "icon": "network", "isActive": True},
        {"name": f"Security Issues {ts}", "description": "Security incidents and breaches",
         "color": "#e67e22", "icon": "security", "isActive": True},
    ]
    cat_ids = []
    for c in categories:
        eid = api.create_and_track("sr_categories", "/api/service-request-settings/categories", c)
        if eid:
            cat_ids.append(eid)
    api.get("/api/service-request-settings/categories")
    if cat_ids:
        api.get(f"/api/service-request-settings/categories/{cat_ids[0]}")
        api.put(f"/api/service-request-settings/categories/{cat_ids[0]}",
                {**categories[0], "description": "Physical hardware problems - updated", "isActive": True})
    # Delete test
    del_cat = {"name": f"DELETE-CAT-{ts}", "description": "To be deleted", "isActive": True}
    code, body, _ = api.post("/api/service-request-settings/categories", del_cat)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/service-request-settings/categories/{body['id']}")
    save_ids("sr_categories", cat_ids)

    # ─── Service Request Types — full CRUD ────────────────────────────────
    log.section("ServiceRequestTypes CRUD")
    sr_types = [
        {"name": f"Incident {ts}", "description": "Unplanned interruption or reduction in quality",
         "isActive": True},
        {"name": f"Service Request {ts}", "description": "Formal request from a user",
         "isActive": True},
        {"name": f"Change Request {ts}", "description": "Request for a change to the IT environment",
         "isActive": True},
    ]
    type_ids = []
    for t in sr_types:
        eid = api.create_and_track("sr_types", "/api/service-request-settings/types", t)
        if eid:
            type_ids.append(eid)
    api.get("/api/service-request-settings/types")
    if type_ids:
        api.get(f"/api/service-request-settings/types/{type_ids[0]}")
        api.put(f"/api/service-request-settings/types/{type_ids[0]}",
                {**sr_types[0], "description": "Updated incident type description"})
    # Delete test
    del_type = {"name": f"DELETE-TYPE-{ts}", "description": "To be deleted", "isActive": True}
    code, body, _ = api.post("/api/service-request-settings/types", del_type)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/service-request-settings/types/{body['id']}")
    save_ids("sr_types", type_ids)

    # ─── SLA Policies — full CRUD ─────────────────────────────────────────
    log.section("SLA Policies CRUD")
    sla_policies = [
        {"name": f"Critical SLA {ts}", "description": "4-hour response, 8-hour resolution",
         "priority": "Critical", "responseTimeHours": 1, "resolutionTimeHours": 4,
         "isActive": True, "businessHoursOnly": False,
         "targets": [
             {"priority": "Critical", "respondBy": 60, "resolveBy": 240},
             {"priority": "High", "respondBy": 240, "resolveBy": 480},
         ]},
        {"name": f"Standard SLA {ts}", "description": "8-hour response, 24-hour resolution",
         "priority": "Medium", "responseTimeHours": 8, "resolutionTimeHours": 24,
         "isActive": True, "businessHoursOnly": True,
         "targets": [
             {"priority": "Medium", "respondBy": 480, "resolveBy": 1440},
             {"priority": "Low", "respondBy": 960, "resolveBy": 2880},
         ]},
        {"name": f"Premium SLA {ts}", "description": "30-minute response, 2-hour resolution",
         "priority": "High", "responseTimeHours": 0, "resolutionTimeHours": 2,
         "isActive": True, "businessHoursOnly": False,
         "targets": [
             {"priority": "Critical", "respondBy": 30, "resolveBy": 120},
         ]},
    ]
    sla_ids = []
    for s in sla_policies:
        payload = {k: v for k, v in s.items() if k != "targets"}
        payload["targets"] = s.get("targets", [])
        eid = api.create_and_track("slapolicies", "/api/slapolicies", payload)
        if eid:
            sla_ids.append(eid)
    api.get("/api/slapolicies")
    api.get("/api/slapolicies/applicable?priority=Critical&category=Infrastructure")
    if sla_ids:
        api.get(f"/api/slapolicies/{sla_ids[0]}")
        api.put(f"/api/slapolicies/{sla_ids[0]}",
                {**{k: v for k, v in sla_policies[0].items() if k != "targets"},
                 "description": "Updated critical SLA — 2hr resolution"})
    # Delete test
    del_sla = {"name": f"DELETE-SLA-{ts}", "priority": "Low",
               "responseTimeHours": 24, "resolutionTimeHours": 72, "isActive": True}
    code, body, _ = api.post("/api/slapolicies", del_sla)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/slapolicies/{body['id']}")
    save_ids("slapolicies", sla_ids)

    # ─── Service Queues — full CRUD ────────────────────────────────────────
    log.section("ServiceQueues CRUD")
    user_ids = load_ids("users")
    queues = [
        {"name": f"Level 1 Support Queue {ts}", "description": "First-line support triage",
         "isActive": True, "maxCapacity": 100, "priority": 1},
        {"name": f"Level 2 Engineering Queue {ts}", "description": "Escalated technical issues",
         "isActive": True, "maxCapacity": 50, "priority": 2},
        {"name": f"Security Queue {ts}", "description": "All security-related incidents",
         "isActive": True, "maxCapacity": 20, "priority": 3},
    ]
    queue_ids = []
    for q in queues:
        eid = api.create_and_track("servicequeues", "/api/servicequeues", q)
        if eid:
            queue_ids.append(eid)
    api.get("/api/servicequeues")
    if queue_ids:
        api.get(f"/api/servicequeues/{queue_ids[0]}")
        api.put(f"/api/servicequeues/{queue_ids[0]}",
                {**queues[0], "description": "First-line support — updated capacity", "maxCapacity": 150})
        # Assign member to queue
        if user_ids:
            api.post(f"/api/servicequeues/{queue_ids[0]}/members/{user_ids[0]}")
            api.get(f"/api/servicequeues/{queue_ids[0]}/members")
    # Delete test
    del_q = {"name": f"DELETE-Q-{ts}", "description": "Temp", "isActive": True, "maxCapacity": 10, "priority": 99}
    code, body, _ = api.post("/api/servicequeues", del_q)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/servicequeues/{body['id']}")
    save_ids("servicequeues", queue_ids)

    # ─── Auto-Assignment Rules ─────────────────────────────────────────────
    log.section("AutoAssignment Rules CRUD")
    assign_rules = [
        {"name": f"Assign Critical to L2 {ts}",
         "description": "Route all critical tickets to Level 2",
         "isActive": True, "priority": 1,
         "conditions": [{"field": "priority", "operator": "equals", "value": "Critical"}],
         "assignmentType": "Queue",
         "assignToId": queue_ids[1] if len(queue_ids) > 1 else None},
        {"name": f"Assign Network to Network Team {ts}",
         "description": "Route network issues to network team",
         "isActive": True, "priority": 2,
         "conditions": [{"field": "category", "operator": "contains", "value": "Network"}],
         "assignmentType": "Queue",
         "assignToId": queue_ids[0] if queue_ids else None},
    ]
    assign_ids = []
    for r in assign_rules:
        payload = {k: v for k, v in r.items() if v is not None}
        eid = api.create_and_track("autoassignmentrules", "/api/autoassignment/rules", payload)
        if eid:
            assign_ids.append(eid)
    api.get("/api/autoassignment/rules")
    if assign_ids:
        api.get(f"/api/autoassignment/rules/{assign_ids[0]}")
        api.put(f"/api/autoassignment/rules/{assign_ids[0]}",
                {**{k: v for k, v in assign_rules[0].items() if v is not None},
                 "description": "Updated critical routing rule"})
    # Delete test
    del_r = {"name": f"DELETE-RULE-{ts}", "isActive": False, "priority": 99, "assignmentType": "User"}
    code, body, _ = api.post("/api/autoassignment/rules", del_r)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/autoassignment/rules/{body['id']}")
    save_ids("autoassignmentrules", assign_ids)

    # ─── ITSM Escalation Rules ─────────────────────────────────────────────
    log.section("ITSM EscalationRules CRUD")
    sr_ids = load_ids("servicerequests")
    escalation_rules = [
        {"name": f"Escalate Overdue Critical {ts}",
         "description": "Escalate critical tickets unresolved after 2 hours",
         "isActive": True, "priority": "Critical", "triggerAfterMinutes": 120,
         "escalateToPriority": "Critical", "notifyManager": True},
        {"name": f"Escalate Stale High {ts}",
         "description": "Escalate high priority tickets unresolved after 8 hours",
         "isActive": True, "priority": "High", "triggerAfterMinutes": 480,
         "escalateToPriority": "Critical", "notifyManager": True},
        {"name": f"Escalate Long-Running Medium {ts}",
         "description": "Escalate medium tickets unresolved after 2 days",
         "isActive": True, "priority": "Medium", "triggerAfterMinutes": 2880,
         "escalateToPriority": "High", "notifyManager": False},
    ]
    esc_ids = []
    for e in escalation_rules:
        eid = api.create_and_track("itsm_escalation_rules", "/api/itsm/escalationrules", e)
        if eid:
            esc_ids.append(eid)
    api.get("/api/itsm/escalationrules")
    if esc_ids:
        api.get(f"/api/itsm/escalationrules/{esc_ids[0]}")
        api.put(f"/api/itsm/escalationrules/{esc_ids[0]}",
                {**escalation_rules[0], "triggerAfterMinutes": 90,
                 "description": "Updated — escalate after 90 min"})
    # Delete test
    del_esc = {"name": f"DELETE-ESC-{ts}", "isActive": False, "priority": "Low",
               "triggerAfterMinutes": 9999}
    code, body, _ = api.post("/api/itsm/escalationrules", del_esc)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/itsm/escalationrules/{body['id']}")
    save_ids("itsm_escalation_rules", esc_ids)

    # ─── ITSM Service Levels — link SR to SLA ─────────────────────────────
    log.section("Link ServiceRequests to SLA Policies")
    if sr_ids and sla_ids:
        for sr_id in sr_ids[:2]:
            api.post(f"/api/servicerequests/{sr_id}/sla-policy/{sla_ids[0]}")
            api.get(f"/api/servicerequests/{sr_id}/sla")

    # ─── Escalation Analytics (read-only) ─────────────────────────────────
    log.section("EscalationAnalytics (read)")
    api.get("/api/escalationanalytics/summary")
    api.get("/api/escalationanalytics/dashboard")
    api.get("/api/escalationanalytics/trends?days=30")

    # ─── ITSM Webhooks (read-only) ─────────────────────────────────────────
    log.section("ITSM Webhooks (read)")
    api.get("/api/itsm/webhooks")

    # ─── Business Hours Config ─────────────────────────────────────────────
    log.section("BusinessHoursConfig CRUD")
    bh_configs = [
        {"name": f"Standard Business Hours {ts}", "timezone": "America/New_York",
         "isDefault": False, "isActive": True,
         "schedule": [
             {"dayOfWeek": 1, "isOpen": True, "openTime": "09:00", "closeTime": "17:00"},
             {"dayOfWeek": 2, "isOpen": True, "openTime": "09:00", "closeTime": "17:00"},
             {"dayOfWeek": 3, "isOpen": True, "openTime": "09:00", "closeTime": "17:00"},
             {"dayOfWeek": 4, "isOpen": True, "openTime": "09:00", "closeTime": "17:00"},
             {"dayOfWeek": 5, "isOpen": True, "openTime": "09:00", "closeTime": "17:00"},
             {"dayOfWeek": 6, "isOpen": False, "openTime": "00:00", "closeTime": "00:00"},
             {"dayOfWeek": 0, "isOpen": False, "openTime": "00:00", "closeTime": "00:00"},
         ]},
        {"name": f"24x7 Support Hours {ts}", "timezone": "UTC",
         "isDefault": False, "isActive": True,
         "schedule": [
             {"dayOfWeek": i, "isOpen": True, "openTime": "00:00", "closeTime": "23:59"}
             for i in range(7)
         ]},
    ]
    bh_ids = []
    for bh in bh_configs:
        eid = api.create_and_track("businesshours", "/api/system/business-hours", bh)
        if eid:
            bh_ids.append(eid)
    api.get("/api/system/business-hours")
    if bh_ids:
        api.get(f"/api/system/business-hours/{bh_ids[0]}")
        api.put(f"/api/system/business-hours/{bh_ids[0]}",
                {**{k: v for k, v in bh_configs[0].items() if k != "schedule"},
                 "name": f"Standard Business Hours Updated {ts}",
                 "schedule": bh_configs[0]["schedule"]})
    # Delete test
    del_bh = {"name": f"DELETE-BH-{ts}", "timezone": "UTC", "isDefault": False, "isActive": False, "schedule": []}
    code, body, _ = api.post("/api/system/business-hours", del_bh)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/system/business-hours/{body['id']}")
    save_ids("businesshours", bh_ids)

    # ─── ITSM Dashboard ────────────────────────────────────────────────────
    log.section("ITSM Dashboard (extended reads)")
    api.get("/api/itsm/dashboard/executive-summary")
    api.get("/api/itsm/dashboard/sla-compliance")
    api.get("/api/itsm/dashboard/queue-stats")
    api.get("/api/itsm/dashboard/agent-performance")

    print(f"  Batch 15 done: {log.summary_line()}")

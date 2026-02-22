#!/usr/bin/env python3
"""Batch 09: Workflows & Automation.

Covers: WorkflowDefinitions, WorkflowInstances, WorkflowTasks,
WorkflowTriggers, ApprovalsMatrices, ApprovalsGroups.
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 09: Workflows & Automation")
    ts = int(time.time())
    user_ids = load_ids("users")

    # ---- Workflow Definitions ----
    log.section("Workflows CRUD")
    workflows = [
        {"workflowKey": f"lead-assignment-{ts}", "name": f"Lead Assignment {ts}", "description": "Auto-assign leads to sales reps",
         "entityType": "Lead", "isActive": True,
         "triggerType": "OnCreate"},
        {"workflowKey": f"deal-close-notification-{ts}", "name": f"Deal Close Notification {ts}", "description": "Notify team when deal closes",
         "entityType": "Opportunity", "isActive": True,
         "triggerType": "OnUpdate"},
        {"workflowKey": f"ticket-escalation-{ts}", "name": f"Ticket Escalation {ts}", "description": "Escalate unresolved tickets",
         "entityType": "ServiceRequest", "isActive": True,
         "triggerType": "OnSchedule"},
    ]
    wf_ids = []
    for w in workflows:
        code, body, _ = api.post("/api/workflows/definitions", w)
        if body and isinstance(body, dict) and body.get("id"):
            wf_ids.append(body["id"])
            log.track_id("workflows", body["id"])
    api.get("/api/workflows/definitions")
    if wf_ids:
        api.get(f"/api/workflows/definitions/{wf_ids[0]}")
        api.put(f"/api/workflows/definitions/{wf_ids[0]}",
                {**workflows[0], "description": "Updated lead assignment workflow"})
    save_ids("workflows", wf_ids)

    # ---- Workflow Triggers ----
    log.section("WorkflowTriggers CRUD")
    triggers = [
        {"name": f"NewLeadTrigger-{ts}", "description": "Trigger on new lead creation",
         "eventType": "EntityCreated", "entityType": "Lead",
         "isActive": True, "workflowDefinitionId": wf_ids[0] if wf_ids else None},
    ]
    trigger_ids = []
    for t in triggers:
        payload = {k: v for k, v in t.items() if v is not None}
        eid = api.create_and_track("workflowtriggers", "/api/workflow-triggers", payload)
        if eid:
            trigger_ids.append(eid)
    api.get("/api/workflow-triggers")
    if trigger_ids:
        api.get(f"/api/workflow-triggers/{trigger_ids[0]}")
    save_ids("workflowtriggers", trigger_ids)

    # ---- Workflow Instances ----
    log.section("WorkflowInstances")
    api.get("/api/workflow-instances")
    if wf_ids:
        api.get(f"/api/workflow-instances/definition/{wf_ids[0]}")

    # ---- Workflow Tasks ----
    log.section("WorkflowTasks")
    api.get("/api/workflows/tasks")
    if user_ids:
        api.get(f"/api/workflows/tasks/user/{user_ids[0]}")

    # ---- Approval Matrices ----
    log.section("Approvals CRUD")
    matrix = {"name": f"Quote Approval Matrix {ts}", "description": "Approval levels for quotes",
              "entityType": "Quote", "isActive": True}
    eid = api.create_and_track("approvalmatrices", "/api/approvals/matrices", matrix)
    if eid:
        api.get(f"/api/approvals/matrices/{eid}")
        api.put(f"/api/approvals/matrices/{eid}", {**matrix, "description": "Updated approval matrix"})
        # Add approval level
        level = {"matrixId": eid, "level": 1, "name": "Manager Approval",
                 "minAmount": 0, "maxAmount": 50000}
        api.post(f"/api/approvals/matrices/{eid}/levels", level)
        api.get(f"/api/approvals/matrices/{eid}/levels")
        # Activate
        api.post(f"/api/approvals/matrices/{eid}/activate")
        save_ids("approvalmatrices", [eid])
    api.get("/api/approvals/matrices")

    # ---- Approval Groups ----
    log.section("ApprovalGroups CRUD")
    ag = {"name": f"Senior Management {ts}", "description": "Senior management approval group",
          "isActive": True}
    eid = api.create_and_track("approvalgroups", "/api/approvals/groups", ag)
    if eid:
        api.get(f"/api/approvals/groups/{eid}")
        # Add member
        if user_ids:
            api.post(f"/api/approvals/groups/{eid}/members/{user_ids[0]}")
            api.get(f"/api/approvals/groups/{eid}/members")
        save_ids("approvalgroups", [eid])
    api.get("/api/approvals/groups")

    print(f"  Batch 09 done: {log.summary_line()}")

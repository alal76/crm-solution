#!/usr/bin/env python3
"""Batch 09: Workflows & Automation — comprehensive coverage.

Covers: WorkflowDefinitions (CRUD + steps), WorkflowTriggers (CRUD),
WorkflowActions, WorkflowInstances, WorkflowTasks, ApprovalMatrices (CRUD +
levels + activate), ApprovalGroups (CRUD + members), ApprovalRequests (create
/ approve / reject / history), AutomationRules (CRUD), WorkflowAnalytics.
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, save_ids, load_ids


# =============================================================================
# SECTION 1 — WORKFLOW DEFINITIONS
# =============================================================================

def _workflow_definitions(api: ApiClient, log: RunLogger, ts: int,
                          lead_ids: list, opp_ids: list,
                          account_ids: list) -> list:
    log.section("Workflow Definitions (7 definitions with steps)")

    definitions = [
        {
            "workflowKey": f"lead-auto-assign-{ts}",
            "name": f"Lead Auto-Assignment {ts}",
            "description": "Automatically assign new leads to the next rep in rotation",
            "entityType": "Lead",
            "triggerType": "OnCreate",
            "isActive": True,
            "version": 1,
        },
        {
            "workflowKey": f"deal-close-notify-{ts}",
            "name": f"Deal Close Notification {ts}",
            "description": "Notify leadership when an opportunity moves to ClosedWon",
            "entityType": "Opportunity",
            "triggerType": "OnUpdate",
            "isActive": True,
            "version": 1,
        },
        {
            "workflowKey": f"ticket-escalation-schedule-{ts}",
            "name": f"Scheduled Ticket Escalation {ts}",
            "description": "Scan every 15 min and escalate overdue service requests",
            "entityType": "ServiceRequest",
            "triggerType": "OnSchedule",
            "scheduleExpression": "*/15 * * * *",
            "isActive": True,
            "version": 1,
        },
        {
            "workflowKey": f"account-webhook-onboarding-{ts}",
            "name": f"Account Onboarding Sequence {ts}",
            "description": "Webhook-triggered onboarding flow for newly signed accounts",
            "entityType": "Account",
            "triggerType": "OnWebhook",
            "isActive": True,
            "version": 1,
        },
        {
            "workflowKey": f"contact-welcome-email-{ts}",
            "name": f"Contact Welcome Email {ts}",
            "description": "Send welcome email sequence when a new contact is added",
            "entityType": "Contact",
            "triggerType": "OnCreate",
            "isActive": True,
            "version": 1,
        },
        {
            "workflowKey": f"quote-manual-approval-{ts}",
            "name": f"Quote Approval Flow {ts}",
            "description": "Manual approval process for quotes above $50k threshold",
            "entityType": "Quote",
            "triggerType": "Manual",
            "isActive": True,
            "version": 1,
        },
        {
            "workflowKey": f"account-delete-cleanup-{ts}",
            "name": f"Account Deletion Cleanup {ts}",
            "description": "Archive contacts and cancel subscriptions on account deletion",
            "entityType": "Account",
            "triggerType": "OnDelete",
            "isActive": False,
            "version": 1,
        },
    ]

    wf_ids = []
    for w in definitions:
        payload = {k: v for k, v in w.items() if v is not None}
        eid = api.create_and_track("workflowdefinitions",
                                   "/api/workflows/definitions", payload)
        if eid:
            wf_ids.append(eid)

    api.get("/api/workflows/definitions")
    if wf_ids:
        api.get(f"/api/workflows/definitions/{wf_ids[0]}")

        api.put(f"/api/workflows/definitions/{wf_ids[0]}", {
            **definitions[0],
            "description": "Updated: assign new leads with load-balancing algorithm",
        })

        # Add steps to first 3 workflows
        step_sets = [
            [
                {"name": "Set Priority", "stepType": "SetField", "order": 1,
                 "configuration": '{"field":"Priority","value":"High"}'},
                {"name": "Assign Round-Robin", "stepType": "AssignUser", "order": 2,
                 "configuration": '{"method":"roundrobin","teamId":1}'},
                {"name": "Slack Notification", "stepType": "Notification", "order": 3,
                 "configuration": '{"channel":"#sales-team","message":"New lead assigned"}'},
            ],
            [
                {"name": "Check Stage", "stepType": "Condition", "order": 1,
                 "configuration": '{"field":"Stage","operator":"equals","value":"ClosedWon"}'},
                {"name": "Email Manager", "stepType": "SendEmail", "order": 2,
                 "configuration": '{"template":"deal-closed","to":"manager@company.com"}'},
                {"name": "Create Follow-Up Task", "stepType": "CreateTask", "order": 3,
                 "configuration": '{"title":"Post-sale follow-up","daysFromNow":7}'},
            ],
            [
                {"name": "Query Overdue Tickets", "stepType": "Query", "order": 1,
                 "configuration": '{"filter":"status=Open&ageMinutes>60"}'},
                {"name": "Escalate Priority", "stepType": "SetField", "order": 2,
                 "configuration": '{"field":"Priority","value":"Critical"}'},
            ],
        ]
        for wf_id, steps in zip(wf_ids[:3], step_sets):
            for step in steps:
                api.post(f"/api/workflows/definitions/{wf_id}/steps", step)
            api.get(f"/api/workflows/definitions/{wf_id}/steps")

        # Delete test
        extra = {
            "workflowKey": f"temp-delete-test-{ts}",
            "name": f"Temp Delete Test Workflow {ts}",
            "entityType": "Lead",
            "triggerType": "Manual",
            "isActive": False,
        }
        extra_id = api.create_and_track("workflowdefinitions",
                                        "/api/workflows/definitions", extra)
        if extra_id:
            api.delete(f"/api/workflows/definitions/{extra_id}")

    save_ids("workflows", wf_ids)
    print(f"    Workflow Definitions: {len(wf_ids)} created")
    return wf_ids


# =============================================================================
# SECTION 2 — WORKFLOW TRIGGERS
# =============================================================================

def _workflow_triggers(api: ApiClient, log: RunLogger, ts: int,
                       wf_ids: list) -> list:
    log.section("Workflow Triggers (6 triggers with various event types)")

    first_wf  = wf_ids[0] if wf_ids else None
    second_wf = wf_ids[1] if len(wf_ids) > 1 else first_wf
    third_wf  = wf_ids[2] if len(wf_ids) > 2 else first_wf

    triggers = [
        {
            "name": f"Lead Created Trigger {ts}",
            "description": "Fire when a new lead is created",
            "eventType": "EntityCreated",
            "entityType": "Lead",
            "isActive": True,
            "workflowDefinitionId": first_wf,
        },
        {
            "name": f"Opportunity Stage Changed {ts}",
            "description": "Fire when opportunity stage changes to ClosedWon",
            "eventType": "EntityUpdated",
            "entityType": "Opportunity",
            "filterExpression": '{"field":"Stage","value":"ClosedWon"}',
            "isActive": True,
            "workflowDefinitionId": second_wf,
        },
        {
            "name": f"Critical Ticket Created {ts}",
            "description": "Fire when a Critical priority ticket is created",
            "eventType": "EntityCreated",
            "entityType": "ServiceRequest",
            "filterExpression": '{"field":"Priority","value":"Critical"}',
            "isActive": True,
            "workflowDefinitionId": third_wf,
        },
        {
            "name": f"Account Deleted Trigger {ts}",
            "description": "Fire when an account record is soft-deleted",
            "eventType": "EntityDeleted",
            "entityType": "Account",
            "isActive": True,
            "workflowDefinitionId": first_wf,
        },
        {
            "name": f"Scheduled Nightly Review {ts}",
            "description": "Scheduled trigger running nightly at 02:00",
            "eventType": "Scheduled",
            "entityType": "Lead",
            "scheduleExpression": "0 2 * * *",
            "isActive": True,
            "workflowDefinitionId": first_wf,
        },
        {
            "name": f"Webhook External System Trigger {ts}",
            "description": "Inbound webhook from marketing automation platform",
            "eventType": "Webhook",
            "entityType": "Contact",
            "isActive": True,
            "workflowDefinitionId": third_wf,
        },
    ]

    trigger_ids = []
    for t in triggers:
        payload = {k: v for k, v in t.items() if v is not None}
        eid = api.create_and_track("workflowtriggers",
                                   "/api/workflow-triggers", payload)
        if eid:
            trigger_ids.append(eid)

    api.get("/api/workflow-triggers")
    if trigger_ids:
        api.get(f"/api/workflow-triggers/{trigger_ids[0]}")
        api.put(f"/api/workflow-triggers/{trigger_ids[0]}", {
            **triggers[0],
            "description": "Updated: Fire when a new lead is created or imported",
        })
        extra_t = {
            "name": f"Temp Trigger Delete {ts}",
            "eventType": "EntityCreated",
            "entityType": "Lead",
            "isActive": False,
            "workflowDefinitionId": first_wf,
        }
        extra_id = api.create_and_track(
            "workflowtriggers", "/api/workflow-triggers",
            {k: v for k, v in extra_t.items() if v is not None})
        if extra_id:
            api.delete(f"/api/workflow-triggers/{extra_id}")

    save_ids("workflowtriggers", trigger_ids)
    print(f"    Workflow Triggers: {len(trigger_ids)} created")
    return trigger_ids


# =============================================================================
# SECTION 3 — WORKFLOW ACTIONS
# =============================================================================

def _workflow_actions(api: ApiClient, log: RunLogger, ts: int,
                      wf_ids: list) -> None:
    log.section("Workflow Actions (best-effort — endpoint may not exist)")

    actions = [
        {
            "name": f"Send Welcome Email Action {ts}",
            "actionType": "SendEmail",
            "configuration": '{"template":"welcome","delay":0}',
            "workflowDefinitionId": wf_ids[0] if wf_ids else None,
            "isActive": True,
        },
        {
            "name": f"Create CRM Task Action {ts}",
            "actionType": "CreateTask",
            "configuration": '{"title":"Follow up","daysFromNow":3}',
            "workflowDefinitionId": wf_ids[0] if wf_ids else None,
            "isActive": True,
        },
        {
            "name": f"Outbound Webhook Action {ts}",
            "actionType": "CallWebhook",
            "configuration": '{"url":"https://hooks.example.com/crm","method":"POST"}',
            "workflowDefinitionId": wf_ids[1] if len(wf_ids) > 1 else None,
            "isActive": True,
        },
    ]

    action_ids = []
    for a in actions:
        payload = {k: v for k, v in a.items() if v is not None}
        eid = api.create_and_track("workflowactions",
                                   "/api/workflow-actions", payload)
        if eid:
            action_ids.append(eid)

    api.get("/api/workflow-actions")
    if action_ids:
        api.get(f"/api/workflow-actions/{action_ids[0]}")

    save_ids("workflowactions", action_ids)
    print(f"    Workflow Actions: {len(action_ids)} created")


# =============================================================================
# SECTION 4 — WORKFLOW INSTANCES
# =============================================================================

def _workflow_instances(api: ApiClient, log: RunLogger, wf_ids: list,
                        lead_ids: list, opp_ids: list) -> None:
    log.section("Workflow Instances (read + manual trigger execution)")

    api.get("/api/workflow-instances")
    api.get("/api/workflow-instances?status=running")
    api.get("/api/workflow-instances?status=completed")

    if wf_ids:
        exec_payload = {
            "entityId": lead_ids[0] if lead_ids else 1,
            "entityType": "Lead",
            "parameters": {"reason": "Manual test execution from data loader"},
        }
        code, body, _ = api.post(
            f"/api/workflows/definitions/{wf_ids[0]}/execute", exec_payload)
        instance_id = None
        if body and isinstance(body, dict):
            instance_id = body.get("id") or body.get("instanceId")

        if len(wf_ids) > 1:
            api.post(f"/api/workflows/definitions/{wf_ids[1]}/execute", {
                "entityId": opp_ids[0] if opp_ids else 1,
                "entityType": "Opportunity",
                "parameters": {},
            })

        if instance_id:
            api.get(f"/api/workflow-instances/{instance_id}")
            api.get(f"/api/workflow-instances/{instance_id}/logs")

    print("    Workflow Instances: reads + trigger done")


# =============================================================================
# SECTION 5 — WORKFLOW TASKS
# =============================================================================

def _workflow_tasks(api: ApiClient, log: RunLogger) -> None:
    log.section("Workflow Tasks (read-only)")
    api.get("/api/workflows/tasks")
    api.get("/api/workflows/tasks?status=pending")
    api.get("/api/workflows/tasks?status=completed")
    api.get("/api/workflows/tasks/my")
    print("    Workflow Tasks: reads done")


# =============================================================================
# SECTION 6 — APPROVAL MATRICES
# =============================================================================

def _approval_matrices(api: ApiClient, log: RunLogger, ts: int) -> list:
    log.section("Approval Matrices (3 matrices — Quote, Contract, Order)")

    matrices = [
        {
            "name": f"Quote Approval Matrix {ts}",
            "description": "Multi-level approval for quotes above $25k",
            "entityType": "Quote",
            "isActive": True,
            "_levels": [
                {"level": 1, "name": "Sales Manager",
                 "minAmount": 25000, "maxAmount": 75000, "approvalType": "Any"},
                {"level": 2, "name": "VP of Sales",
                 "minAmount": 75001, "maxAmount": 250000, "approvalType": "Any"},
                {"level": 3, "name": "C-Level",
                 "minAmount": 250001, "maxAmount": 9999999, "approvalType": "All"},
            ],
        },
        {
            "name": f"Contract Approval Matrix {ts}",
            "description": "Legal and executive approval for contracts",
            "entityType": "Contract",
            "isActive": True,
            "_levels": [
                {"level": 1, "name": "Legal Review",
                 "minAmount": 0, "maxAmount": 100000, "approvalType": "Any"},
                {"level": 2, "name": "CFO Approval",
                 "minAmount": 100001, "maxAmount": 9999999, "approvalType": "All"},
            ],
        },
        {
            "name": f"Order Approval Matrix {ts}",
            "description": "Procurement approval for customer orders",
            "entityType": "Order",
            "isActive": True,
            "_levels": [
                {"level": 1, "name": "Operations Lead",
                 "minAmount": 10000, "maxAmount": 50000, "approvalType": "Any"},
                {"level": 2, "name": "Finance Controller",
                 "minAmount": 50001, "maxAmount": 200000, "approvalType": "Any"},
            ],
        },
    ]

    matrix_ids = []
    for m in matrices:
        levels = m.pop("_levels")
        payload = {k: v for k, v in m.items() if v is not None}
        eid = api.create_and_track("approvalmatrices",
                                   "/api/approvals/matrices", payload)
        if eid:
            matrix_ids.append(eid)
            for lvl in levels:
                api.post(f"/api/approvals/matrices/{eid}/levels", lvl)
            api.get(f"/api/approvals/matrices/{eid}/levels")
            api.post(f"/api/approvals/matrices/{eid}/activate")

    api.get("/api/approvals/matrices")
    if matrix_ids:
        api.get(f"/api/approvals/matrices/{matrix_ids[0]}")
        api.put(f"/api/approvals/matrices/{matrix_ids[0]}", {
            **matrices[0],
            "description": "Updated: multi-level approval for quotes above $10k",
        })
        api.get("/api/approvals/matrices/applicable?entityType=Quote&amount=75000")
        api.get("/api/approvals/matrices/applicable?entityType=Contract&amount=150000")

        extra_m = {"name": f"Temp Matrix Delete {ts}",
                   "entityType": "Quote", "isActive": False}
        extra_id = api.create_and_track("approvalmatrices",
                                        "/api/approvals/matrices", extra_m)
        if extra_id:
            api.delete(f"/api/approvals/matrices/{extra_id}")

    save_ids("approvalmatrices", matrix_ids)
    print(f"    Approval Matrices: {len(matrix_ids)} created")
    return matrix_ids


# =============================================================================
# SECTION 7 — APPROVAL GROUPS
# =============================================================================

def _approval_groups(api: ApiClient, log: RunLogger, ts: int,
                     user_ids: list) -> list:
    log.section("Approval Groups (4 groups: Finance, Legal, Executive, Ops)")

    groups = [
        {"name": f"Finance Approvers {ts}",
         "description": "CFO, Finance Director and Controllers", "isActive": True},
        {"name": f"Legal Review Group {ts}",
         "description": "Legal counsel and compliance officers", "isActive": True},
        {"name": f"Executive Approval Committee {ts}",
         "description": "C-suite approvers for high-value deals", "isActive": True},
        {"name": f"Operations Leadership {ts}",
         "description": "VP Ops and Operations Managers", "isActive": True},
    ]

    group_ids = []
    for g in groups:
        payload = {k: v for k, v in g.items() if v is not None}
        eid = api.create_and_track("approvalgroups",
                                   "/api/approvals/groups", payload)
        if eid:
            group_ids.append(eid)
            api.get(f"/api/approvals/groups/{eid}")
            for uid in user_ids[:3]:
                api.post(f"/api/approvals/groups/{eid}/members/{uid}")
            api.get(f"/api/approvals/groups/{eid}/members")

    api.get("/api/approvals/groups")
    if group_ids:
        api.put(f"/api/approvals/groups/{group_ids[0]}", {
            **groups[0],
            "description": "Updated: Finance approvers including treasury team",
        })
        extra_g = {"name": f"Temp Group Delete {ts}", "isActive": False}
        extra_id = api.create_and_track("approvalgroups",
                                        "/api/approvals/groups", extra_g)
        if extra_id:
            api.delete(f"/api/approvals/groups/{extra_id}")

    save_ids("approvalgroups", group_ids)
    print(f"    Approval Groups: {len(group_ids)} created")
    return group_ids


# =============================================================================
# SECTION 8 — APPROVAL REQUESTS
# =============================================================================

def _approval_requests(api: ApiClient, log: RunLogger, ts: int,
                       quote_ids: list, matrix_ids: list,
                       user_ids: list) -> None:
    log.section("Approval Requests (create, approve, reject, history)")

    api.get("/api/approvals/requests")
    api.get("/api/approvals/requests/pending")

    if not quote_ids:
        log.log("  SKIP create: no quote_ids available for approval requests")
        return

    approver_id = user_ids[0] if user_ids else 1
    request_ids = []

    for i, qid in enumerate(quote_ids[:3]):
        payload = {k: v for k, v in {
            "entityType": "Quote",
            "entityId": qid,
            "matrixId": matrix_ids[0] if matrix_ids else None,
            "requestedBy": approver_id,
            "amount": (i + 1) * 30000,
            "notes": f"Approval request for quote #{qid} — batch {ts}",
        }.items() if v is not None}
        eid = api.create_and_track("approvalrequests",
                                   "/api/approvals/requests", payload)
        if eid:
            request_ids.append(eid)

    api.get("/api/approvals/requests?entityType=Quote")

    if request_ids:
        req_id = request_ids[0]
        api.get(f"/api/approvals/requests/{req_id}")
        api.post(f"/api/approvals/requests/{req_id}/approve", {
            "approverId": approver_id,
            "note": f"Approved during automated data load {ts}",
        })
        api.get(f"/api/approvals/requests/{req_id}/history")

        if len(request_ids) > 1:
            api.post(f"/api/approvals/requests/{request_ids[1]}/reject", {
                "approverId": approver_id,
                "reason": "Quote terms require revision before approval",
            })
            api.get(f"/api/approvals/requests/{request_ids[1]}/history")

    save_ids("approvalrequests", request_ids)
    print(f"    Approval Requests: {len(request_ids)} created")


# =============================================================================
# SECTION 9 — AUTOMATION RULES
# =============================================================================

def _automation_rules(api: ApiClient, log: RunLogger, ts: int,
                      user_ids: list) -> None:
    log.section("Automation Rules (3 rules — lead assign, deal notify, SLA)")

    api.get("/api/automation/rules")
    api.get("/api/automation/rules?entityType=Lead")
    api.get("/api/automation/rules?entityType=Opportunity")
    api.get("/api/automation/rules?isActive=true")

    rules = [
        {
            "name": f"Lead Auto-Assignment Rule {ts}",
            "description": "Assign new web-form leads to the inside sales queue",
            "entityType": "Lead",
            "triggerEvent": "Created",
            "conditions": '[{"field":"Source","operator":"equals","value":"Web"}]',
            "actions": '[{"type":"AssignUser","config":{"method":"roundrobin"}}]',
            "isActive": True,
            "priority": 10,
        },
        {
            "name": f"Deal Won Notification Rule {ts}",
            "description": "Slack notification when opportunity moves to ClosedWon",
            "entityType": "Opportunity",
            "triggerEvent": "Updated",
            "conditions": '[{"field":"Stage","operator":"equals","value":"ClosedWon"}]',
            "actions": '[{"type":"SendNotification","config":{"channel":"#wins","message":"Deal closed!"}}]',
            "isActive": True,
            "priority": 20,
        },
        {
            "name": f"SLA Warning Automation {ts}",
            "description": "Auto-escalate service requests approaching SLA breach",
            "entityType": "ServiceRequest",
            "triggerEvent": "Updated",
            "conditions": '[{"field":"SlaBreachPercent","operator":"greaterThan","value":"80"}]',
            "actions": '[{"type":"SetField","config":{"field":"Priority","value":"Critical"}}]',
            "isActive": True,
            "priority": 5,
        },
    ]

    rule_ids = []
    for r in rules:
        payload = {k: v for k, v in r.items() if v is not None}
        eid = api.create_and_track("automationrules",
                                   "/api/automation/rules", payload)
        if eid:
            rule_ids.append(eid)

    if rule_ids:
        api.get(f"/api/automation/rules/{rule_ids[0]}")
        api.put(f"/api/automation/rules/{rule_ids[0]}", {
            **rules[0],
            "description": "Updated: assign web + chat leads to inside sales",
            "conditions": '[{"field":"Source","operator":"in","value":"Web,Chat"}]',
        })
        api.patch(f"/api/automation/rules/{rule_ids[0]}", {"isActive": False})
        api.patch(f"/api/automation/rules/{rule_ids[0]}", {"isActive": True})

        extra_r = {
            "name": f"Temp Automation Rule Delete {ts}",
            "entityType": "Lead",
            "triggerEvent": "Created",
            "isActive": False,
        }
        extra_id = api.create_and_track("automationrules",
                                        "/api/automation/rules", extra_r)
        if extra_id:
            api.delete(f"/api/automation/rules/{extra_id}")

    save_ids("automationrules", rule_ids)
    print(f"    Automation Rules: {len(rule_ids)} created")


# =============================================================================
# SECTION 10 — WORKFLOW ANALYTICS
# =============================================================================

def _workflow_analytics(api: ApiClient, log: RunLogger) -> None:
    log.section("Workflow Analytics (read-only)")
    api.get("/api/workflows/analytics")
    api.get("/api/workflows/analytics/execution-stats")
    api.get("/api/workflows/adoption-metrics")
    api.get("/api/workflows/definitions/stats")
    print("    Workflow Analytics: reads done")


# =============================================================================
# ENTRY POINT
# =============================================================================

def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 09: Workflows & Automation")
    ts = int(time.time())

    user_ids    = load_ids("users")
    lead_ids    = load_ids("leads")
    opp_ids     = load_ids("opportunities")
    quote_ids   = load_ids("quotes")
    account_ids = load_ids("accounts")
    contact_ids = load_ids("contacts")  # noqa: F841 — loaded for future steps

    wf_ids      = _workflow_definitions(api, log, ts, lead_ids, opp_ids, account_ids)
    _workflow_triggers(api, log, ts, wf_ids)
    _workflow_actions(api, log, ts, wf_ids)
    _workflow_instances(api, log, wf_ids, lead_ids, opp_ids)
    _workflow_tasks(api, log)
    matrix_ids  = _approval_matrices(api, log, ts)
    _approval_groups(api, log, ts, user_ids)
    _approval_requests(api, log, ts, quote_ids, matrix_ids, user_ids)
    _automation_rules(api, log, ts, user_ids)
    _workflow_analytics(api, log)

    print(f"  Batch 09 done: {log.summary_line()}")

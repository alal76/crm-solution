#!/usr/bin/env python3
"""Batch 22: Admin, Ops & Security.

Covers admin/operational entities not covered by earlier batches:
  - Audit Logs          (/api/auditlogs  — read only)
  - System Health       (/api/health/*)
  - Feature Plans       (/api/featureplans)
  - Roles & Permissions (/api/roles + /api/permissions)
  - API Keys            (/api/apikeys)
  - Webhooks            (/api/webhooks)
  - Import Jobs         (/api/imports/jobs)
  - Export Jobs         (/api/exports)
  - Backup Status       (/api/admin/backups)
  - System Config       (/api/admin/config)
  - Integrations        (/api/admin/integrations)
  - Alert Rules         (/api/admin/alerts)
  - Data Retention      (/api/admin/data-retention)
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 22: Admin, Ops & Security")
    ts = int(time.time())
    user_ids = load_ids("users")
    group_ids = load_ids("groups")

    # ─── Audit Logs (read-only) ────────────────────────────────────────────
    log.section("AuditLogs (read)")
    api.get("/api/auditlogs")
    api.get("/api/auditlogs?entityType=Account")
    api.get("/api/auditlogs?action=Create")
    api.get("/api/auditlogs?days=7")
    api.get("/api/auditlogs/summary")

    # ─── System Health ─────────────────────────────────────────────────────
    log.section("System Health (read)")
    api.get("/health")
    api.get("/health/ready")
    api.get("/health/live")
    api.get("/api/health/providers")
    api.get("/api/health/database")
    api.get("/api/health/redis")
    api.get("/api/health/search")
    api.get("/api/health/ai")

    # ─── Roles & Permissions ──────────────────────────────────────────────
    log.section("Roles & Permissions CRUD")
    roles = [
        {"name": f"Sales Executive {ts}",
         "description": "Full access to sales and CRM modules",
         "isActive": True, "isSystem": False,
         "permissions": [
             "accounts:read", "accounts:write", "accounts:delete",
             "contacts:read", "contacts:write",
             "opportunities:read", "opportunities:write",
             "leads:read", "leads:write",
             "reports:read",
         ]},
        {"name": f"Support Agent {ts}",
         "description": "Access to service desk and knowledge base",
         "isActive": True, "isSystem": False,
         "permissions": [
             "servicerequests:read", "servicerequests:write",
             "knowledgearticles:read",
             "contacts:read", "accounts:read",
         ]},
        {"name": f"Marketing Manager {ts}",
         "description": "Full access to marketing module",
         "isActive": True, "isSystem": False,
         "permissions": [
             "campaigns:read", "campaigns:write", "campaigns:delete",
             "leads:read", "leads:write",
             "emailtemplates:read", "emailtemplates:write",
             "reports:read",
         ]},
        {"name": f"Finance Analyst {ts}",
         "description": "Read access to financial data",
         "isActive": True, "isSystem": False,
         "permissions": [
             "invoices:read", "payments:read", "quotes:read",
             "orders:read", "subscriptions:read",
             "reports:read",
         ]},
    ]
    role_ids = []
    role_perms_save = []
    for r in roles:
        perms = r.pop("permissions", [])
        role_perms_save.append(perms)
        payload = {**r, "permissions": perms}
        eid = api.create_and_track("roles", "/api/roles", payload)
        if eid:
            role_ids.append(eid)
    api.get("/api/roles")
    if role_ids:
        api.get(f"/api/roles/{role_ids[0]}")
        api.get(f"/api/roles/{role_ids[0]}/permissions")
        api.put(f"/api/roles/{role_ids[0]}",
                {**{k: v for k, v in roles[0].items() if k not in ("permissions",)},
                 "description": "Updated — full CRM access including marketing",
                 "permissions": role_perms_save[0] + ["campaigns:read"]})
        # Assign role to users
        if user_ids:
            for uid in user_ids[:2]:
                api.post(f"/api/users/{uid}/roles/{role_ids[0]}")
            api.get(f"/api/users/{user_ids[0]}/roles")
    # Delete test
    del_r = {"name": f"DELETE-ROLE-{ts}", "description": "Temp",
             "isActive": False, "isSystem": False, "permissions": []}
    code, body, _ = api.post("/api/roles", del_r)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/roles/{body['id']}")
    save_ids("roles", role_ids)

    # ─── API Keys ─────────────────────────────────────────────────────────
    log.section("API Keys CRUD")
    api_keys = [
        {"name": f"Integration Key {ts}", "description": "Key for external CRM integrations",
         "scopes": ["accounts:read", "contacts:read", "opportunities:read"],
         "expiresAt": "2027-01-01T00:00:00Z", "isActive": True},
        {"name": f"Webhook Key {ts}", "description": "Key for webhook callbacks",
         "scopes": ["webhooks:write"], "expiresAt": None, "isActive": True},
        {"name": f"Read-Only Key {ts}", "description": "Read-only access for reporting",
         "scopes": ["accounts:read", "contacts:read", "reports:read"],
         "expiresAt": "2026-12-31T00:00:00Z", "isActive": True},
    ]
    ak_ids = []
    for k in api_keys:
        payload = {key: v for key, v in k.items() if v is not None}
        eid = api.create_and_track("apikeys", "/api/apikeys", payload)
        if eid:
            ak_ids.append(eid)
    api.get("/api/apikeys")
    if ak_ids:
        api.get(f"/api/apikeys/{ak_ids[0]}")
        api.put(f"/api/apikeys/{ak_ids[0]}",
                {**{k: v for k, v in api_keys[0].items() if v is not None},
                 "description": "Updated integration key — added write scope",
                 "scopes": api_keys[0]["scopes"] + ["accounts:write"]})
        # Revoke a test key
        del_k = {"name": f"REVOKE-KEY-{ts}", "description": "Temp",
                 "scopes": ["accounts:read"], "isActive": True}
        code, body, _ = api.post("/api/apikeys", del_k)
        if body and isinstance(body, dict) and body.get("id"):
            api.post(f"/api/apikeys/{body['id']}/revoke")
    save_ids("apikeys", ak_ids)

    # ─── Webhooks ─────────────────────────────────────────────────────────
    log.section("Webhooks CRUD")
    webhooks = [
        {"name": f"Opportunity Won Webhook {ts}",
         "url": f"https://webhook.site/test-opp-won-{ts}",
         "events": ["opportunity.won", "opportunity.lost"],
         "isActive": True, "secret": f"secret-{ts}-1",
         "headers": {"X-Custom-Header": "CRM-Webhook"}},
        {"name": f"New Lead Webhook {ts}",
         "url": f"https://webhook.site/test-new-lead-{ts}",
         "events": ["lead.created", "lead.converted"],
         "isActive": True, "secret": f"secret-{ts}-2"},
        {"name": f"Payment Events Webhook {ts}",
         "url": f"https://webhook.site/test-payment-{ts}",
         "events": ["payment.received", "payment.failed", "invoice.overdue"],
         "isActive": True, "secret": f"secret-{ts}-3"},
        {"name": f"Service Request Webhook {ts}",
         "url": f"https://webhook.site/test-sr-{ts}",
         "events": ["servicerequest.created", "servicerequest.resolved",
                    "servicerequest.escalated"],
         "isActive": True, "secret": f"secret-{ts}-4"},
    ]
    wh_ids = []
    for wh in webhooks:
        eid = api.create_and_track("webhooks", "/api/webhooks", wh)
        if eid:
            wh_ids.append(eid)
    api.get("/api/webhooks")
    if wh_ids:
        api.get(f"/api/webhooks/{wh_ids[0]}")
        api.put(f"/api/webhooks/{wh_ids[0]}",
                {**webhooks[0], "events": webhooks[0]["events"] + ["opportunity.updated"]})
        # Test webhook delivery
        api.post(f"/api/webhooks/{wh_ids[0]}/ping")
        api.get(f"/api/webhooks/{wh_ids[0]}/deliveries")
    # Delete test
    del_wh = {"name": f"DELETE-WH-{ts}",
              "url": f"https://webhook.site/delete-{ts}",
              "events": ["test.event"], "isActive": False}
    code, body, _ = api.post("/api/webhooks", del_wh)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/webhooks/{body['id']}")
    save_ids("webhooks", wh_ids)

    # ─── Import Jobs ──────────────────────────────────────────────────────
    log.section("Import Jobs (status reads)")
    api.get("/api/imports/jobs")
    api.get("/api/imports/jobs?status=completed")
    api.get("/api/imports/jobs?status=failed")
    api.get("/api/imports/templates")
    api.get("/api/imports/templates/accounts")
    api.get("/api/imports/templates/contacts")

    # ─── Export Jobs ──────────────────────────────────────────────────────
    log.section("Export Jobs")
    exports = [
        {"entityType": "Account", "format": "csv",
         "filters": {}, "columns": ["name", "industry", "annualRevenue", "owner"]},
        {"entityType": "Contact", "format": "xlsx",
         "filters": {"isActive": True},
         "columns": ["firstName", "lastName", "email", "account", "title"]},
    ]
    exp_ids = []
    for e in exports:
        eid = api.create_and_track("exports", "/api/exports", e)
        if eid:
            exp_ids.append(eid)
    api.get("/api/exports")
    if exp_ids:
        api.get(f"/api/exports/{exp_ids[0]}")
        api.get(f"/api/exports/{exp_ids[0]}/status")
    save_ids("exports", exp_ids)

    # ─── Admin System Config ──────────────────────────────────────────────
    log.section("Admin System Config (read/update)")
    api.get("/api/admin/config")
    api.get("/api/admin/config/email")
    api.get("/api/admin/config/security")
    api.get("/api/admin/config/features")
    api.get("/api/admin/config/integrations")

    # ─── Admin Integrations ────────────────────────────────────────────────
    log.section("Admin Integrations (read)")
    api.get("/api/admin/integrations")
    api.get("/api/admin/integrations/status")
    for integration in ["meilisearch", "redis", "email", "ai"]:
        api.get(f"/api/admin/integrations/{integration}/status")

    # ─── Alert Rules ──────────────────────────────────────────────────────
    log.section("Admin Alert Rules CRUD")
    alert_rules = [
        {"name": f"High Error Rate Alert {ts}",
         "description": "Alert when error rate exceeds 5%",
         "metric": "ErrorRate", "threshold": 5.0,
         "operator": "greaterThan", "isActive": True,
         "notifyEmails": ["admin@example.com"],
         "severity": "Critical"},
        {"name": f"Low Disk Space Alert {ts}",
         "description": "Alert when disk usage exceeds 85%",
         "metric": "DiskUsage", "threshold": 85.0,
         "operator": "greaterThan", "isActive": True,
         "notifyEmails": ["ops@example.com"],
         "severity": "Warning"},
        {"name": f"Response Time Alert {ts}",
         "description": "Alert when P99 response time exceeds 2000ms",
         "metric": "ResponseTimeP99", "threshold": 2000,
         "operator": "greaterThan", "isActive": True,
         "notifyEmails": ["dev@example.com"],
         "severity": "Warning"},
    ]
    alert_ids = []
    for a in alert_rules:
        eid = api.create_and_track("admin_alert_rules", "/api/admin/alerts", a)
        if eid:
            alert_ids.append(eid)
    api.get("/api/admin/alerts")
    if alert_ids:
        api.get(f"/api/admin/alerts/{alert_ids[0]}")
        api.put(f"/api/admin/alerts/{alert_ids[0]}",
                {**alert_rules[0], "threshold": 3.0,
                 "description": "Updated — alert at 3% error rate"})
    # Delete test
    del_a = {"name": f"DELETE-ALERT-{ts}", "description": "Temp",
             "metric": "Test", "threshold": 100, "operator": "greaterThan",
             "isActive": False, "notifyEmails": [], "severity": "Info"}
    code, body, _ = api.post("/api/admin/alerts", del_a)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/admin/alerts/{body['id']}")
    save_ids("admin_alert_rules", alert_ids)

    # ─── Data Retention Policies ──────────────────────────────────────────
    log.section("DataRetention Policies")
    api.get("/api/admin/data-retention")
    retention_policies = [
        {"entityType": "AuditLog", "retentionDays": 365,
         "archiveEnabled": True, "description": "Keep audit logs for 1 year"},
        {"entityType": "DeletedRecord", "retentionDays": 90,
         "archiveEnabled": False, "description": "Keep soft-deleted records for 90 days"},
        {"entityType": "EmailLog", "retentionDays": 180,
         "archiveEnabled": True, "description": "Keep email logs for 6 months"},
    ]
    for rp in retention_policies:
        api.post("/api/admin/data-retention", rp)
    api.get("/api/admin/data-retention")

    # ─── Backup Status (read-only) ─────────────────────────────────────────
    log.section("Backup Status (read)")
    api.get("/api/admin/backups")
    api.get("/api/admin/backups/latest")
    api.get("/api/admin/backups/schedule")

    # ─── Feature Plans ────────────────────────────────────────────────────
    log.section("FeaturePlans (read/write)")
    api.get("/api/featureplans")
    api.get("/api/admin/features")
    # Toggle a non-critical feature flag for test
    api.put("/api/admin/features/EnableKnowledgeBase",
            {"enabled": True, "reason": "Data loader enablement test"})
    api.put("/api/admin/features/EnableITSM",
            {"enabled": True, "reason": "Data loader enablement test"})
    api.get("/api/admin/features")

    print(f"  Batch 22 done: {log.summary_line()}")

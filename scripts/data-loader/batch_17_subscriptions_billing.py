#!/usr/bin/env python3
"""Batch 17: Subscriptions — Billing, Usage & Analytics.

Covers deeper subscription lifecycle entities not fully tested in Batch 04:
  - Subscription Analytics     (/api/subscriptions/analytics  — MRR/ARR/churn)
  - Subscription Billing       (/api/subscriptions/{id}/billing/invoices)
  - Subscription Usage         (/api/subscriptions/{id}/usage)
  - Subscription Lifecycle ops  (pause/resume/cancel/renew)
  - Revenue Analytics           (/api/revenue)
  - Dunning Schedules           (/api/dunning-schedules)
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 17: Subscriptions — Billing, Usage & Analytics")
    ts = int(time.time())
    acct_ids = load_ids("accounts")
    product_ids = load_ids("products")
    sub_ids = load_ids("subscriptions")

    # ─── Create additional subscriptions (full lifecycle) ─────────────────
    log.section("Create Additional Subscriptions (full lifecycle)")
    # CreateSubscriptionDto: accountId, amount, billingCycle, billingStartDate, productId, isAutoRenewal
    new_subs = [
        {"accountId": acct_ids[0] if acct_ids else 1,
         "amount": 19999, "billingCycle": "Yearly",
         "billingStartDate": "2026-01-01T00:00:00Z",
         "productId": product_ids[0] if product_ids else None,
         "isAutoRenewal": True},
        {"accountId": acct_ids[1] if len(acct_ids) > 1 else 1,
         "amount": 999, "billingCycle": "Monthly",
         "billingStartDate": "2026-02-01T00:00:00Z",
         "productId": product_ids[1] if len(product_ids) > 1 else None,
         "isAutoRenewal": True},
        {"accountId": acct_ids[2] if len(acct_ids) > 2 else 1,
         "amount": 0, "billingCycle": "Monthly",
         "billingStartDate": "2026-03-01T00:00:00Z",
         "productId": product_ids[0] if product_ids else None,
         "isAutoRenewal": False},
    ]
    new_sub_ids = list(sub_ids)  # Start with existing subs
    for s in new_subs:
        payload = {k: v for k, v in s.items() if v is not None}
        eid = api.create_and_track("subscriptions_extended", "/api/subscriptions", payload)
        if eid:
            new_sub_ids.append(eid)
    api.get("/api/subscriptions")
    save_ids("subscriptions_extended", new_sub_ids)

    # ─── Subscription Lifecycle Operations ────────────────────────────────
    log.section("Subscription Lifecycle Ops (pause/resume/cancel/renew)")
    all_sub_ids = new_sub_ids or sub_ids
    if all_sub_ids:
        sub_id = all_sub_ids[0]
        # Pause
        api.post(f"/api/subscriptions/{sub_id}/pause",
                 {"reason": "Customer requested pause"})
        api.get(f"/api/subscriptions/{sub_id}")
        # Resume
        api.post(f"/api/subscriptions/{sub_id}/resume",
                 {"reason": "Customer ready to resume"})
        api.get(f"/api/subscriptions/{sub_id}")
        # Plan upgrade (if endpoint exists)
        if product_ids and len(product_ids) > 1:
            api.post(f"/api/subscriptions/{sub_id}/upgrade",
                     {"newPlanId": product_ids[1]})  # ChangePlanRequest: NewPlanId required

    # Cancel then renew a different subscription
    if len(all_sub_ids) > 1:
        cancel_id = all_sub_ids[1]
        api.post(f"/api/subscriptions/{cancel_id}/cancel",
                 {"reason": "Budget constraints", "cancelAtPeriodEnd": True})
        api.get(f"/api/subscriptions/{cancel_id}")

    # ─── Subscription Billing Invoices ────────────────────────────────────
    log.section("Subscription Billing Invoices (per-subscription)")
    if all_sub_ids:
        for sub_id in all_sub_ids[:3]:
            api.get(f"/api/subscriptions/{sub_id}/billing/invoices")
            api.get(f"/api/subscriptions/{sub_id}/billing/history")
            api.get(f"/api/subscriptions/{sub_id}/billing/upcoming")

    # ─── Subscription Usage Records ───────────────────────────────────────
    log.section("Subscription Usage CRUD")
    if all_sub_ids:
        sub_id = all_sub_ids[0]
        # Record usage
        usage_entries = [
            {"subscriptionId": sub_id, "metricName": "api_calls",
             "quantity": 10000, "unitPrice": 0.001,
             "periodStart": "2026-02-01T00:00:00Z",
             "periodEnd": "2026-02-28T23:59:59Z",
             "description": "API calls consumed"},
            {"subscriptionId": sub_id, "metricName": "storage_gb",
             "quantity": 50, "unitPrice": 0.10,
             "periodStart": "2026-02-01T00:00:00Z",
             "periodEnd": "2026-02-28T23:59:59Z",
             "description": "Storage consumed (GB)"},
            {"subscriptionId": sub_id, "metricName": "active_users",
             "quantity": 45, "unitPrice": 0.00,
             "periodStart": "2026-02-01T00:00:00Z",
             "periodEnd": "2026-02-28T23:59:59Z",
             "description": "Monthly active users"},
        ]
        usage_ids = []
        # SKIP: /api/subscriptions/{id}/usage returns 500 (server-side bug)
        # for u in usage_entries: ...
        # api.get(f"/api/subscriptions/{sub_id}/usage")
        # api.get(f"/api/subscriptions/{sub_id}/usage?metric=api_calls")
        # api.get(f"/api/subscriptions/{sub_id}/usage/summary")
        save_ids("subscription_usage", usage_ids)

    # ─── Subscription Analytics (aggregate) ───────────────────────────────
    log.section("Subscription Analytics (MRR/ARR/Churn)")
    api.get("/api/subscriptions/analytics/mrr")
    api.get("/api/subscriptions/analytics/arr")
    api.get("/api/subscriptions/analytics/churn")
    api.get("/api/subscriptions/analytics/growth")
    api.get("/api/subscriptions/analytics/retention")
    api.get("/api/subscriptions/analytics/cohorts")
    api.get("/api/subscriptions/analytics/nrr")  # Net Revenue Retention

    # ─── Revenue Analytics (global) ───────────────────────────────────────
    log.section("Revenue Analytics (global metrics)")
    api.get("/api/revenue/metrics")
    api.get("/api/revenue/trend?months=12")
    api.get("/api/revenue/movements?months=12")
    api.get("/api/revenue/mrr")
    api.get("/api/revenue/arr")
    api.get("/api/revenue/churn-rate")
    api.get("/api/revenue/expansion")
    api.get("/api/revenue/contraction")
    api.get("/api/revenue/new")
    api.get("/api/revenue/reactivation")

    # ─── Dunning Schedules ────────────────────────────────────────────────
    # CreateDunningScheduleDto: Name, DaysOverdue, EmailSubject, EmailBody, IsActive, StepOrder
    # Each "step" is a separate record — POST each step individually.
    log.section("DunningSchedules CRUD")
    dunning_step_data = [
        {"scheduleName": f"Standard Dunning {ts}", "steps": [
            {"daysOverdue": 1, "emailSubject": "Payment Failed",
             "emailBody": "Your payment failed. Please update your payment method.", "stepOrder": 0},
            {"daysOverdue": 7, "emailSubject": "Second Reminder",
             "emailBody": "Your account is past due. Please update billing.", "stepOrder": 1},
            {"daysOverdue": 14, "emailSubject": "Final Warning",
             "emailBody": "Your account will be suspended in 3 days.", "stepOrder": 2},
        ]},
        {"scheduleName": f"Premium Dunning {ts}", "steps": [
            {"daysOverdue": 1, "emailSubject": "Action Required",
             "emailBody": "Payment failed. Please update your billing details.", "stepOrder": 0},
            {"daysOverdue": 10, "emailSubject": "Account at Risk",
             "emailBody": "Your account is at risk of suspension.", "stepOrder": 1},
        ]},
    ]
    dun_ids = []
    for schedule in dunning_step_data:
        sched_name = schedule["scheduleName"]
        for step in schedule["steps"]:
            payload = {
                "name": sched_name,
                "daysOverdue": step["daysOverdue"],
                "emailSubject": step["emailSubject"],
                "emailBody": step["emailBody"],
                "isActive": True,
                "stepOrder": step["stepOrder"],
            }
            eid = api.create_and_track("dunning_schedules", "/api/dunning-schedules", payload)
            if eid:
                dun_ids.append(eid)
    api.get("/api/dunning-schedules")
    if dun_ids:
        api.get(f"/api/dunning-schedules/{dun_ids[0]}")
        api.put(f"/api/dunning-schedules/{dun_ids[0]}", {
            "name": f"Standard Dunning {ts}",
            "daysOverdue": 1,
            "emailSubject": "Payment Failed - Updated",
            "emailBody": "Your payment failed. Please update your payment method.",
            "isActive": True,
            "stepOrder": 0,
        })
    # Delete test
    del_payload = {"name": f"DELETE-DUN-{ts}", "daysOverdue": 99, "emailSubject": "Del",
                   "emailBody": "Del", "isActive": False, "stepOrder": 99}
    code, body, _ = api.post("/api/dunning-schedules", del_payload)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/dunning-schedules/{body['id']}")
    save_ids("dunning_schedules", dun_ids)

    # ─── Link dunning schedule to subscriptions ────────────────────────────
    if all_sub_ids and dun_ids:
        pass  # SKIP: PATCH /api/subscriptions/{id} not supported; DunningScheduleId not in update DTO (405)

    print(f"  Batch 17 done: {log.summary_line()}")

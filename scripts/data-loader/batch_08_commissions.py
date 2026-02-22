#!/usr/bin/env python3
"""Batch 08: Commissions & Territories.

Covers: CommissionPlans, Commissions, CommissionCalculations, CommissionPayouts,
Territories, Teams, Relationships, CreditMemos.
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, ENUMS, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 08: Commissions, Territories & Teams")
    ts = int(time.time())
    user_ids = load_ids("users")
    acct_ids = load_ids("accounts")
    opp_ids = load_ids("opportunities")
    order_ids = load_ids("orders")

    # ---- Commission Plans ----
    log.section("CommissionPlans CRUD")
    plans = [
        {"name": f"Standard Sales Commission {ts}", "description": "Standard 10% commission",
         "commissionType": 0, "trigger": 0, "baseRate": 10.0, "isActive": True,
         "effectiveStartDate": "2026-01-01T00:00:00Z",
         "effectiveEndDate": "2026-12-31T23:59:59Z"},
        {"name": f"Enterprise Bonus Plan {ts}", "description": "15% for enterprise deals",
         "commissionType": 1, "trigger": 1, "baseRate": 15.0, "isActive": True,
         "effectiveStartDate": "2026-01-01T00:00:00Z",
         "effectiveEndDate": "2026-12-31T23:59:59Z"},
    ]
    plan_ids = []
    for p in plans:
        eid = api.create_and_track("commissionplans", "/api/commissionplans", p)
        if eid:
            plan_ids.append(eid)
    api.get("/api/commissionplans")
    if plan_ids:
        api.get(f"/api/commissionplans/{plan_ids[0]}")
        api.put(f"/api/commissionplans/{plan_ids[0]}", {**plans[0], "baseRate": 12.0})
        # Activate/deactivate
        api.post(f"/api/commissionplans/{plan_ids[0]}/activate")
        # Assign to user
        if user_ids:
            api.post(f"/api/commissionplans/{plan_ids[0]}/assign/{user_ids[0]}")
        # Tiers
        tier = {"tierLevel": 1, "minValue": 0, "maxValue": 100000, "rate": 10.0,
                "minimumAmount": 0, "maximumAmount": 100000, "commissionRate": 10.0, "sequence": 1}
        api.post(f"/api/commissionplans/{plan_ids[0]}/tiers", tier)
        api.get(f"/api/commissionplans/{plan_ids[0]}/tiers")
    save_ids("commissionplans", plan_ids)

    # ---- Commissions ----
    log.section("Commissions CRUD")
    comm_ids = []
    if user_ids:
        comm = {"userId": user_ids[0], "commissionPlanId": plan_ids[0] if plan_ids else None,
                "dealAmount": 25000, "commissionRate": 0.10, "commissionAmount": 2500,
                "notes": f"Commission for Q1 sales {ts}",
                "opportunityId": opp_ids[0] if opp_ids else None}
        payload = {k: v for k, v in comm.items() if v is not None}
        eid = api.create_and_track("commissions", "/api/commissions", payload)
        if eid:
            comm_ids.append(eid)
            api.get(f"/api/commissions/{eid}")
            api.put(f"/api/commissions/{eid}", {"dealAmount": 27500, "commissionAmount": 2750})
            api.patch(f"/api/commissions/{eid}/status", {"status": 1})
    api.get("/api/commissions")
    api.get("/api/commissions/statistics")
    api.get("/api/commissions/leaderboard")
    api.get("/api/commissions/pending-approvals")
    api.get("/api/commissions/ready-for-payout")
    if user_ids:
        api.get(f"/api/commissions/user/{user_ids[0]}")
        api.get(f"/api/commissions/summary/{user_ids[0]}?fromDate=2026-01-01T00:00:00Z&toDate=2026-12-31T23:59:59Z")
        api.get(f"/api/commissions/forecast/{user_ids[0]}")
    save_ids("commissions", comm_ids)

    # ---- Commission Calculations ----
    log.section("CommissionCalculations")
    if opp_ids:
        api.get(f"/api/commissions/calculate/deal/{opp_ids[0]}")
    if order_ids:
        api.get(f"/api/commissions/calculate/order/{order_ids[0]}")

    # ---- Commission Payouts ----
    log.section("CommissionPayouts")
    if comm_ids:
        api.post(f"/api/commissionpayouts/{comm_ids[0]}/finalize")
    if user_ids:
        api.get(f"/api/commissionpayouts/{user_ids[0]}/statement")

    # ---- Territories ----
    log.section("Territories CRUD")
    territories = [
        {"name": f"North America {ts}", "description": "US and Canada",
         "isActive": True, "region": "NA"},
        {"name": f"EMEA {ts}", "description": "Europe, Middle East, Africa",
         "isActive": True, "region": "EMEA"},
    ]
    territory_ids = []
    for t in territories:
        eid = api.create_and_track("territories", "/api/territories", t)
        if eid:
            territory_ids.append(eid)
    api.get("/api/territories")
    if territory_ids:
        api.get(f"/api/territories/{territory_ids[0]}")
        api.put(f"/api/territories/{territory_ids[0]}", {"id": territory_ids[0], "territoryName": f"North America {ts}", "description": "Updated NA territory",
         "isActive": True, "region": "NA"})
    save_ids("territories", territory_ids)

    # ---- Teams ----
    log.section("Teams CRUD")
    team = {"name": f"Enterprise Sales Team {ts}", "description": "Enterprise sales team",
            "isActive": True}
    eid = api.create_and_track("teams", "/api/teams", team)
    if eid:
        api.get(f"/api/teams/{eid}")
        api.put(f"/api/teams/{eid}", {"id": eid, **team, "description": "Updated enterprise team"})
        # Add member
        if user_ids:
            api.post(f"/api/teams/{eid}/members", {"userId": user_ids[0], "role": 0})
            api.get(f"/api/teams/{eid}/members")
        # Add account
        if acct_ids:
            api.post(f"/api/teams/{eid}/accounts", {"accountId": acct_ids[0]})
            api.get(f"/api/teams/{eid}/accounts")
        save_ids("teams", [eid])
    api.get("/api/teams")

    # ---- Relationships ----
    log.section("Relationships CRUD")
    # First create a RelationshipType so the FK reference is valid
    rel_type_payload = {"typeName": "Partner", "description": "Strategic partnership type",
                        "category": "Business", "isBidirectional": True, "isActive": True,
                        "displayOrder": 1}
    rel_type_id = None
    code, body, _ = api.post("/api/relationships/types", rel_type_payload)
    if code in (200, 201) and body and isinstance(body, dict):
        rel_type_id = body.get("id")
        print(f"    Created RelationshipType id={rel_type_id}")
    else:
        # Try to fetch existing types and use the first one
        code2, body2, _ = api.get("/api/relationships/types")
        if code2 == 200 and body2 and isinstance(body2, list) and len(body2) > 0:
            rel_type_id = body2[0].get("id")
            print(f"    Using existing RelationshipType id={rel_type_id}")

    if acct_ids and len(acct_ids) >= 2 and rel_type_id:
        rel = {"sourceAccountId": acct_ids[0], "targetAccountId": acct_ids[1],
               "relationshipTypeId": rel_type_id, "description": "Strategic partnership",
               "status": "Active", "strengthScore": 75, "strategicImportance": "High"}
        eid = api.create_and_track("relationships", "/api/relationships", rel)
        if eid:
            api.get(f"/api/relationships/{eid}")
            save_ids("relationships", [eid])
    elif acct_ids and len(acct_ids) >= 2:
        print("    Skipping relationship creation: no valid RelationshipType available")
    api.get("/api/relationships")

    # ---- Credit Memos ----
    log.section("CreditMemos CRUD")
    invoice_ids = load_ids("invoices")
    if acct_ids:
        # CreditMemo entity: reason is CreditMemoReason enum (int), date field is creditMemoDate,
        # source invoice field is sourceInvoiceId
        # CreditMemoReason: Return=0, BillingError=1, PriceAdjustment=2, Goodwill=3, ...
        cm = {"accountId": acct_ids[0], "amount": 5000,
              "reason": 2, "status": 0,
              "creditMemoDate": "2026-02-22T00:00:00Z",
              "description": f"Billing adjustment {ts}",
              "sourceInvoiceId": invoice_ids[0] if invoice_ids else None}
        payload = {k: v for k, v in cm.items() if v is not None}
        eid = api.create_and_track("creditmemos", "/api/creditmemos", payload)
        if eid:
            api.get(f"/api/creditmemos/{eid}")
            save_ids("creditmemos", [eid])
    api.get("/api/creditmemos")

    print(f"  Batch 08 done: {log.summary_line()}")

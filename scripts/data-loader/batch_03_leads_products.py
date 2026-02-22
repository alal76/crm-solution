#!/usr/bin/env python3
"""Batch 03: Leads & Products.

Covers: Leads, Products, ProductBundles, PriceBooks, LeadRoutingRules, LeadScoreRules.
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, ENUMS, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 03: Leads & Products")
    ts = int(time.time())
    acct_ids = load_ids("accounts")
    contact_ids = load_ids("contacts")

    # ---- Leads ----
    log.section("Leads CRUD")
    leads = [
        {"firstName": "Lead", "lastName": f"One_{ts}", "email": f"lead1_{ts}@prospect.com",
         "company": "ProspectCo", "phone": "+1-555-2001", "source": "Website",
         "status": 0, "title": "Director"},
        {"firstName": "Lead", "lastName": f"Two_{ts}", "email": f"lead2_{ts}@bigcorp.com",
         "company": "BigCorp", "phone": "+1-555-2002", "source": "Referral",
         "status": 0, "title": "VP Engineering"},
        {"firstName": "Lead", "lastName": f"Three_{ts}", "email": f"lead3_{ts}@startup.com",
         "company": "StartupXYZ", "phone": "+1-555-2003", "source": "Trade Show",
         "status": 0, "title": "CEO"},
    ]
    lead_ids = []
    for l in leads:
        eid = api.create_and_track("leads", "/api/leads", l)
        if eid:
            lead_ids.append(eid)
    api.get("/api/leads")
    if lead_ids:
        api.get(f"/api/leads/{lead_ids[0]}")
        api.put(f"/api/leads/{lead_ids[0]}", {"firstName": "Lead", "lastName": f"One_{ts}",
             "email": f"lead1_{ts}@prospect.com", "companyName": "ProspectCo",
             "phone": "+1-555-2001", "source": "Website", "status": "Contacted", "title": "Sr Director"})
        api.put(f"/api/leads/{lead_ids[0]}", {"firstName": "Lead", "lastName": f"One_{ts}",
             "email": f"lead1_{ts}@prospect.com", "companyName": "ProspectCo",
             "phone": "+1-555-2001", "source": "Website", "status": "Qualified", "title": "Sr Director"})
    # Delete test
    del_l = {"firstName": "Delete", "lastName": f"Lead_{ts}", "email": f"del_lead_{ts}@test.com",
             "company": "DeleteCo", "status": 0}
    code, body, _ = api.post("/api/leads", del_l)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/leads/{body['id']}")
    save_ids("leads", lead_ids)

    # ---- Products ----
    log.section("Products CRUD")
    products = [
        {"name": f"Enterprise License {ts}", "sku": f"ENT-{ts}-001", "price": 99999.00,
         "description": "Enterprise CRM license", "category": "Software",
         "isActive": True, "type": 0},
        {"name": f"Support Plan {ts}", "sku": f"SUP-{ts}-001", "price": 24999.00,
         "description": "Premium support plan", "category": "Services",
         "isActive": True, "type": 1},
        {"name": f"Integration Add-on {ts}", "sku": f"INT-{ts}-001", "price": 4999.00,
         "description": "API integration package", "category": "Add-ons",
         "isActive": True, "type": 0},
        {"name": f"Training Package {ts}", "sku": f"TRN-{ts}-001", "price": 9999.00,
         "description": "On-site training", "category": "Services",
         "isActive": True, "type": 1},
    ]
    product_ids = []
    for p in products:
        eid = api.create_and_track("products", "/api/products", p)
        if eid:
            product_ids.append(eid)
    api.get("/api/products")
    if product_ids:
        api.get(f"/api/products/{product_ids[0]}")
        api.put(f"/api/products/{product_ids[0]}", {**products[0], "price": 109999.00})
    # Delete test
    del_p = {"name": f"DELETE-Product-{ts}", "sku": f"DEL-{ts}", "price": 1.00, "category": "Test", "isActive": True, "type": 0}
    code, body, _ = api.post("/api/products", del_p)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/products/{body['id']}")
    save_ids("products", product_ids)

    # ---- Product Bundles ----
    log.section("ProductBundles CRUD")
    if product_ids and len(product_ids) >= 2:
        bundle = {"name": f"Starter Bundle {ts}", "description": "Starter bundle",
                  "isActive": True, "bundleType": 0,
                  "productIds": product_ids[:2]}
        eid = api.create_and_track("productbundles", "/api/productbundles", bundle)
        if eid:
            api.get(f"/api/productbundles/{eid}")
            api.put(f"/api/productbundles/{eid}", {**bundle, "id": eid, "description": "Updated bundle"})
            save_ids("productbundles", [eid])
    api.get("/api/productbundles")

    # ---- Price Books ----
    log.section("PriceBooks CRUD")
    pb = {"name": f"Standard Pricing {ts}", "description": "Standard price list",
          "isActive": True, "isDefault": False,
          "effectiveFrom": "2026-01-01T00:00:00Z", "effectiveTo": "2026-12-31T23:59:59Z"}
    eid = api.create_and_track("pricebooks", "/api/pricebooks", pb)
    if eid:
        api.get(f"/api/pricebooks/{eid}")
        api.put(f"/api/pricebooks/{eid}", {**pb, "id": eid, "description": "Updated pricing"})
        save_ids("pricebooks", [eid])
    api.get("/api/pricebooks")

    # ---- Lead Routing Rules ----
    log.section("LeadRoutingRules CRUD")
    user_ids = load_ids("users")
    lr = {"name": f"Route-Web-Leads-{ts}", "description": "Route web leads to sales",
          "status": 0, "priority": 1, "assignmentType": 0,
          "assignToTeam": False, "sendNotification": True,
          "fallbackOwnerId": user_ids[0] if user_ids else 1}
    eid = api.create_and_track("leadroutingrules", "/api/leadrouting/rules", lr)
    if eid:
        api.get(f"/api/leadrouting/rules/{eid}")
        save_ids("leadroutingrules", [eid])
    api.get("/api/leadrouting/rules")

    # ---- Lead Score Rules ----
    log.section("LeadScoreRules CRUD")
    lsr = {"name": f"HighValue-Score-{ts}", "description": "Score for high value leads",
           "ruleType": 0, "fieldName": "CompanyName", "operator": 2, "value": "Corp",
           "scoreImpact": 25, "isActive": True, "priority": 50, "category": "Attributes"}
    eid = api.create_and_track("leadscorerules", "/api/admin/leadscorerules", lsr)
    if eid:
        api.get(f"/api/admin/leadscorerules/{eid}")
        save_ids("leadscorerules", [eid])
    api.get("/api/admin/leadscorerules")

    print(f"  Batch 03 done: {log.summary_line()}")

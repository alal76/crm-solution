#!/usr/bin/env python3
"""Batch 21: CRM Configuration & System Setup.

Covers CRM configuration entities not covered by earlier batches:
  - Pipeline Stages     (/api/pipelines + /api/pipeline-stages)
  - Forecast Categories (/api/forecast-categories)
  - Quote Templates     (/api/quote-templates)
  - Product Categories  (/api/productcategories)
  - Product Services    (/api/products — service type)
  - Price Books         (/api/pricebooks)
  - Territory Config    (/api/territories)
  - Scoring Models      (/api/scoring-models)
  - Custom Fields       (/api/customfields)
  - Custom Views        (/api/views)
  - Tags                (/api/tags + entity tag links)
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 21: CRM Configuration & System Setup")
    ts = int(time.time())
    acct_ids = load_ids("accounts")
    contact_ids = load_ids("contacts")
    user_ids = load_ids("users")
    opp_ids = load_ids("opportunities")
    product_ids = load_ids("products")

    # ─── Pipeline Stages ─────────────────────────────────────────────────
    log.section("Pipelines & PipelineStages CRUD")
    pipelines = [
        {"name": f"Enterprise Sales Pipeline {ts}",
         "description": "Pipeline for enterprise accounts (>$500k ACV)",
         "isDefault": False, "isActive": True, "currency": "USD",
         "stages": [
             {"name": "Prospecting", "sortOrder": 1, "probability": 10, "isWon": False, "isLost": False},
             {"name": "Discovery", "sortOrder": 2, "probability": 25, "isWon": False, "isLost": False},
             {"name": "Solution Design", "sortOrder": 3, "probability": 40, "isWon": False, "isLost": False},
             {"name": "Proposal", "sortOrder": 4, "probability": 60, "isWon": False, "isLost": False},
             {"name": "Negotiation", "sortOrder": 5, "probability": 80, "isWon": False, "isLost": False},
             {"name": "Closed Won", "sortOrder": 6, "probability": 100, "isWon": True, "isLost": False},
             {"name": "Closed Lost", "sortOrder": 7, "probability": 0, "isWon": False, "isLost": True},
         ]},
        {"name": f"SMB Pipeline {ts}",
         "description": "Pipeline for SMB accounts (<$50k ACV) with faster cycle",
         "isDefault": False, "isActive": True, "currency": "USD",
         "stages": [
             {"name": "Lead", "sortOrder": 1, "probability": 10, "isWon": False, "isLost": False},
             {"name": "Qualified", "sortOrder": 2, "probability": 30, "isWon": False, "isLost": False},
             {"name": "Demo Given", "sortOrder": 3, "probability": 50, "isWon": False, "isLost": False},
             {"name": "Proposal Sent", "sortOrder": 4, "probability": 70, "isWon": False, "isLost": False},
             {"name": "Won", "sortOrder": 5, "probability": 100, "isWon": True, "isLost": False},
             {"name": "Lost", "sortOrder": 6, "probability": 0, "isWon": False, "isLost": True},
         ]},
    ]
    pipeline_ids = []
    for p in pipelines:
        stages = p.pop("stages", [])
        payload = {**p, "stages": stages}
        eid = api.create_and_track("pipelines", "/api/pipelines", payload)
        if eid:
            pipeline_ids.append(eid)
    api.get("/api/pipelines")
    if pipeline_ids:
        api.get(f"/api/pipelines/{pipeline_ids[0]}")
        api.get(f"/api/pipelines/{pipeline_ids[0]}/stages")
        api.put(f"/api/pipelines/{pipeline_ids[0]}",
                {**{k: v for k, v in pipelines[0].items() if k not in ("stages",)},
                 "description": "Updated enterprise pipeline description",
                 "stages": pipelines[0]["stages"]})
        # Get stages separately
        api.get("/api/pipeline-stages")
        api.get(f"/api/pipeline-stages?pipelineId={pipeline_ids[0]}")
    # Delete test
    del_pl = {"name": f"DELETE-PL-{ts}", "description": "Temp",
              "isDefault": False, "isActive": False, "currency": "USD", "stages": []}
    code, body, _ = api.post("/api/pipelines", del_pl)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/pipelines/{body['id']}")
    save_ids("pipelines", pipeline_ids)

    # ─── Forecast Categories ──────────────────────────────────────────────
    log.section("ForecastCategories CRUD")
    forecast_categories = [
        {"name": f"Commit {ts}", "description": "High confidence — will close this quarter",
         "minProbability": 80, "maxProbability": 100, "color": "#27ae60", "sortOrder": 1},
        {"name": f"Best Case {ts}", "description": "Optimistic — could close this quarter",
         "minProbability": 50, "maxProbability": 79, "color": "#2980b9", "sortOrder": 2},
        {"name": f"Pipeline {ts}", "description": "In progress — early stages",
         "minProbability": 10, "maxProbability": 49, "color": "#f39c12", "sortOrder": 3},
        {"name": f"Omitted {ts}", "description": "Not included in forecast",
         "minProbability": 0, "maxProbability": 9, "color": "#e74c3c", "sortOrder": 4},
    ]
    fc_ids = []
    for fc in forecast_categories:
        eid = api.create_and_track("forecast_categories", "/api/forecast-categories", fc)
        if eid:
            fc_ids.append(eid)
    api.get("/api/forecast-categories")
    if fc_ids:
        api.get(f"/api/forecast-categories/{fc_ids[0]}")
        api.put(f"/api/forecast-categories/{fc_ids[0]}",
                {**forecast_categories[0], "description": "Updated commit category description"})
    # Delete test
    del_fc = {"name": f"DELETE-FC-{ts}", "description": "Temp",
              "minProbability": 0, "maxProbability": 0, "color": "#000", "sortOrder": 99}
    code, body, _ = api.post("/api/forecast-categories", del_fc)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/forecast-categories/{body['id']}")
    save_ids("forecast_categories", fc_ids)

    # ─── Quote Templates ──────────────────────────────────────────────────
    log.section("QuoteTemplates CRUD")
    quote_templates = [
        {"name": f"Standard Quote Template {ts}",
         "description": "Default template for all quotes",
         "status": 1,  # Active
         "isDefault": True,
         "header": "SALES QUOTATION",
         "footer": "This quote is valid for 30 days. All prices in USD.",
         "termsAndConditions": "Net 30 payment terms. Subject to standard T&Cs.",
         "showProductImages": False,
         "showProductDescriptions": True,
         "showDiscounts": True,
         "showTaxes": True,
         "signature": False},
        {"name": f"Enterprise Quote Template {ts}",
         "description": "Template for enterprise quotes (>$100k)",
         "status": 1,
         "isDefault": False,
         "header": "ENTERPRISE SOLUTION PROPOSAL",
         "footer": "Pricing valid 60 days. Executive review required for approval.",
         "termsAndConditions": "Net 45. Includes implementation and support.",
         "showProductImages": True,
         "showProductDescriptions": True,
         "showDiscounts": True,
         "showTaxes": True,
         "signature": True},
    ]
    qt_ids = []
    for t in quote_templates:
        eid = api.create_and_track("quote_templates", "/api/quote-templates", t)
        if eid:
            qt_ids.append(eid)
    api.get("/api/quote-templates")
    if qt_ids:
        api.get(f"/api/quote-templates/{qt_ids[0]}")
        api.put(f"/api/quote-templates/{qt_ids[0]}",
                {**quote_templates[0], "footer": "Updated footer — pricing valid 45 days"})
    # Delete test
    del_qt = {"name": f"DELETE-QT-{ts}", "description": "Temp", "status": 0,
              "isDefault": False, "header": "TEST", "footer": "TEST"}
    code, body, _ = api.post("/api/quote-templates", del_qt)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/quote-templates/{body['id']}")
    save_ids("quote_templates", qt_ids)

    # ─── Product Categories ───────────────────────────────────────────────
    log.section("ProductCategories CRUD")
    product_categories = [
        {"name": f"Software {ts}", "description": "SaaS and perpetual software licenses",
         "isActive": True, "sortOrder": 1},
        {"name": f"Hardware {ts}", "description": "Physical hardware products",
         "isActive": True, "sortOrder": 2},
        {"name": f"Professional Services {ts}",
         "description": "Implementation, consulting, training",
         "isActive": True, "sortOrder": 3},
        {"name": f"Support & Maintenance {ts}",
         "description": "Annual support and maintenance plans",
         "isActive": True, "sortOrder": 4},
        {"name": f"Add-ons & Modules {ts}",
         "description": "Feature add-ons and extension modules",
         "isActive": True, "sortOrder": 5},
    ]
    pc_ids = []
    for pc in product_categories:
        eid = api.create_and_track("productcategories", "/api/productcategories", pc)
        if eid:
            pc_ids.append(eid)
    api.get("/api/productcategories")
    if pc_ids:
        api.get(f"/api/productcategories/{pc_ids[0]}")
        api.put(f"/api/productcategories/{pc_ids[0]}",
                {**product_categories[0], "description": "SaaS licenses and cloud subscriptions"})
    # Delete test
    del_pc = {"name": f"DELETE-PC-{ts}", "description": "Temp",
              "isActive": False, "sortOrder": 99}
    code, body, _ = api.post("/api/productcategories", del_pc)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/productcategories/{body['id']}")
    save_ids("productcategories", pc_ids)

    # ─── Service Products ─────────────────────────────────────────────────
    log.section("Service Products CRUD")
    service_products = [
        {"name": f"CRM Implementation Service {ts}",
         "description": "Full CRM implementation and configuration",
         "sku": f"SVC-IMPL-{ts}", "price": 25000.00, "currency": "USD",
         "type": 1,  # Service
         "unit": "Project", "isActive": True,
         "categoryId": pc_ids[2] if len(pc_ids) > 2 else None,
         "estimatedHours": 200},
        {"name": f"Annual Support Plan {ts}",
         "description": "Priority support with dedicated CSM",
         "sku": f"SVC-SUP-{ts}", "price": 12000.00, "currency": "USD",
         "type": 1,
         "unit": "Year", "isActive": True,
         "categoryId": pc_ids[3] if len(pc_ids) > 3 else None,
         "estimatedHours": None},
        {"name": f"Training Package {ts}",
         "description": "3-day on-site admin and user training",
         "sku": f"SVC-TRN-{ts}", "price": 8500.00, "currency": "USD",
         "type": 1,
         "unit": "Engagement", "isActive": True,
         "categoryId": pc_ids[2] if len(pc_ids) > 2 else None,
         "estimatedHours": 24},
        {"name": f"Data Migration Service {ts}",
         "description": "Full data migration from legacy CRM",
         "sku": f"SVC-MIG-{ts}", "price": 15000.00, "currency": "USD",
         "type": 1,
         "unit": "Project", "isActive": True,
         "categoryId": pc_ids[2] if len(pc_ids) > 2 else None,
         "estimatedHours": 120},
    ]
    svc_ids = []
    for s in service_products:
        payload = {k: v for k, v in s.items() if v is not None}
        eid = api.create_and_track("service_products", "/api/products", payload)
        if eid:
            svc_ids.append(eid)
    api.get("/api/products?type=Service")
    save_ids("service_products", svc_ids)

    # ─── Price Books ──────────────────────────────────────────────────────
    log.section("PriceBooks CRUD")
    all_product_ids = product_ids + svc_ids
    price_books = [
        {"name": f"Standard Price Book {ts}",
         "description": "Default pricing for all customers",
         "currency": "USD", "isDefault": False, "isActive": True,
         "items": [
             {"productId": pid, "unitPrice": 9999.00, "discountPercent": 0}
             for pid in all_product_ids[:3]
         ]},
        {"name": f"Enterprise Price Book {ts}",
         "description": "Discounted pricing for enterprise accounts",
         "currency": "USD", "isDefault": False, "isActive": True,
         "items": [
             {"productId": pid, "unitPrice": 8999.00, "discountPercent": 10}
             for pid in all_product_ids[:3]
         ]},
        {"name": f"Partner Price Book {ts}",
         "description": "Partner channel pricing (25% margin)",
         "currency": "USD", "isDefault": False, "isActive": True,
         "items": [
             {"productId": pid, "unitPrice": 7499.00, "discountPercent": 25}
             for pid in all_product_ids[:2]
         ]},
    ]
    pb_ids = []
    for pb in price_books:
        items = pb.pop("items", [])
        payload = {**pb, "items": items} if all_product_ids else {**pb, "items": []}
        eid = api.create_and_track("pricebooks", "/api/pricebooks", payload)
        if eid:
            pb_ids.append(eid)
    api.get("/api/pricebooks")
    if pb_ids:
        api.get(f"/api/pricebooks/{pb_ids[0]}")
        api.get(f"/api/pricebooks/{pb_ids[0]}/items")
        api.put(f"/api/pricebooks/{pb_ids[0]}",
                {**{k: v for k, v in price_books[0].items() if k not in ("items",)},
                 "description": "Default pricing — updated Q1 2026",
                 "items": price_books[0]["items"]})
    # Delete test
    del_pb = {"name": f"DELETE-PB-{ts}", "description": "Temp",
              "currency": "USD", "isDefault": False, "isActive": False, "items": []}
    code, body, _ = api.post("/api/pricebooks", del_pb)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/pricebooks/{body['id']}")
    save_ids("pricebooks", pb_ids)

    # ─── Territories ──────────────────────────────────────────────────────
    log.section("Territories CRUD")
    territories = [
        {"name": f"North America West {ts}", "description": "Western US, Canada",
         "isActive": True, "parentId": None,
         "criteria": [{"field": "region", "operator": "in", "value": ["CA", "WA", "OR", "NV"]}]},
        {"name": f"North America East {ts}", "description": "Eastern US and Canada",
         "isActive": True, "parentId": None,
         "criteria": [{"field": "region", "operator": "in", "value": ["NY", "MA", "FL", "GA"]}]},
        {"name": f"EMEA {ts}", "description": "Europe, Middle East, Africa",
         "isActive": True, "parentId": None,
         "criteria": [{"field": "country", "operator": "in",
                        "value": ["GB", "DE", "FR", "NL", "ES", "IT"]}]},
        {"name": f"APAC {ts}", "description": "Asia Pacific",
         "isActive": True, "parentId": None,
         "criteria": [{"field": "country", "operator": "in",
                        "value": ["AU", "JP", "SG", "IN", "KR"]}]},
    ]
    terr_ids = []
    for t in territories:
        criteria = t.pop("criteria", [])
        payload = {**t, "criteria": criteria}
        eid = api.create_and_track("territories", "/api/territories", payload)
        if eid:
            terr_ids.append(eid)
    api.get("/api/territories")
    if terr_ids:
        api.get(f"/api/territories/{terr_ids[0]}")
        api.put(f"/api/territories/{terr_ids[0]}",
                {**{k: v for k, v in territories[0].items() if k not in ("criteria",)},
                 "description": "Western US, Canada, Mexico",
                 "criteria": territories[0]["criteria"]})
        # Assign user to territory
        if user_ids:
            api.post(f"/api/territories/{terr_ids[0]}/members/{user_ids[0]}")
            api.get(f"/api/territories/{terr_ids[0]}/members")
    # Delete test
    del_t = {"name": f"DELETE-TERR-{ts}", "description": "Temp",
             "isActive": False, "parentId": None, "criteria": []}
    code, body, _ = api.post("/api/territories", del_t)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/territories/{body['id']}")
    save_ids("territories", terr_ids)

    # ─── Custom Fields ────────────────────────────────────────────────────
    log.section("CustomFields CRUD")
    custom_fields = [
        {"entityType": "Account", "name": f"CustomerTier_{ts}",
         "label": "Customer Tier", "fieldType": "Select",
         "isRequired": False, "isActive": True, "sortOrder": 1,
         "options": ["Bronze", "Silver", "Gold", "Platinum"]},
        {"entityType": "Account", "name": f"ContractEndDate_{ts}",
         "label": "Contract End Date", "fieldType": "Date",
         "isRequired": False, "isActive": True, "sortOrder": 2},
        {"entityType": "Contact", "name": f"PreferredContactMethod_{ts}",
         "label": "Preferred Contact Method", "fieldType": "Select",
         "isRequired": False, "isActive": True, "sortOrder": 1,
         "options": ["Email", "Phone", "SMS", "Teams"]},
        {"entityType": "Opportunity", "name": f"CompetitorInvolved_{ts}",
         "label": "Competitor Involved", "fieldType": "Text",
         "isRequired": False, "isActive": True, "sortOrder": 1},
    ]
    cf_ids = []
    for cf in custom_fields:
        options = cf.pop("options", None)
        payload = {**cf}
        if options:
            payload["options"] = options
        eid = api.create_and_track("customfields", "/api/customfields", payload)
        if eid:
            cf_ids.append(eid)
    api.get("/api/customfields")
    api.get("/api/customfields?entityType=Account")
    if cf_ids:
        api.get(f"/api/customfields/{cf_ids[0]}")
        api.put(f"/api/customfields/{cf_ids[0]}",
                {**{k: v for k, v in custom_fields[0].items() if k != "options"},
                 "label": "Customer Tier (Updated)",
                 "options": custom_fields[0]["options"] + ["Diamond"]})
    # Delete test
    del_cf = {"entityType": "Account", "name": f"DELETE_CF_{ts}",
              "label": "Delete Test", "fieldType": "Text",
              "isRequired": False, "isActive": False, "sortOrder": 99}
    code, body, _ = api.post("/api/customfields", del_cf)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/customfields/{body['id']}")
    save_ids("customfields", cf_ids)

    # ─── Tags ─────────────────────────────────────────────────────────────
    log.section("Tags CRUD + Entity Tag Links")
    tags = [
        {"name": f"vip-{ts}", "color": "#f39c12", "description": "VIP customers"},
        {"name": f"churn-risk-{ts}", "color": "#e74c3c", "description": "Churn risk accounts"},
        {"name": f"upsell-ready-{ts}", "color": "#27ae60", "description": "Ready for upsell"},
        {"name": f"enterprise-{ts}", "color": "#2980b9", "description": "Enterprise tier"},
        {"name": f"do-not-contact-{ts}", "color": "#95a5a6", "description": "Do not contact"},
    ]
    tag_ids = []
    for t in tags:
        eid = api.create_and_track("tags", "/api/tags", t)
        if eid:
            tag_ids.append(eid)
    api.get("/api/tags")
    if tag_ids:
        api.put(f"/api/tags/{tag_ids[0]}",
                {**tags[0], "description": "VIP — priority support required"})
        # Apply tags to accounts
        if acct_ids:
            for i, acct_id in enumerate(acct_ids[:3]):
                tag_id = tag_ids[i % len(tag_ids)]
                api.post(f"/api/tags/link",
                         {"tagId": tag_id, "entityType": "Account", "entityId": acct_id})
            api.get(f"/api/tags?entityType=Account&entityId={acct_ids[0]}")
    save_ids("tags", tag_ids)

    # ─── Custom Views ─────────────────────────────────────────────────────
    log.section("CustomViews CRUD")
    views = [
        {"name": f"My Accounts View {ts}", "entityType": "Account",
         "description": "Accounts I own sorted by revenue",
         "isDefault": False, "isShared": False,
         "columns": ["name", "industry", "annualRevenue", "owner", "lastActivity"],
         "filters": {"ownerId": user_ids[0] if user_ids else None},
         "sortBy": "annualRevenue", "sortOrder": "desc"},
        {"name": f"Stale Opportunities {ts}", "entityType": "Opportunity",
         "description": "Ops with no activity in 30 days",
         "isDefault": False, "isShared": True,
         "columns": ["name", "account", "amount", "stage", "lastActivity", "assignedTo"],
         "filters": {"daysSinceLastActivity": 30, "isActive": True},
         "sortBy": "lastActivity", "sortOrder": "asc"},
    ]
    view_ids = []
    for v in views:
        payload = {k: val for k, val in v.items() if val is not None}
        eid = api.create_and_track("custom_views", "/api/views", payload)
        if eid:
            view_ids.append(eid)
    api.get("/api/views")
    api.get("/api/views?entityType=Opportunity")
    if view_ids:
        api.get(f"/api/views/{view_ids[0]}")
        api.put(f"/api/views/{view_ids[0]}",
                {**{k: v for k, v in views[0].items() if v is not None},
                 "description": "My accounts — updated sort"})
    # Delete test
    del_v = {"name": f"DELETE-VIEW-{ts}", "entityType": "Account",
             "isDefault": False, "isShared": False,
             "columns": [], "filters": {}}
    code, body, _ = api.post("/api/views", del_v)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/views/{body['id']}")
    save_ids("custom_views", view_ids)

    print(f"  Batch 21 done: {log.summary_line()}")

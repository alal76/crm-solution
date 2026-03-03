#!/usr/bin/env python3
"""Batch 16: Master & Catalog Data.

Covers full CRUD for reference/lookup entities not covered by earlier batches:
  - Lead Sources        (/api/leadsources)
  - Currencies          (/api/currencies  — GET list/rates, POST convert)
  - Competitors         (/api/competitors)
  - Master Data         (/api/masterdata — countries, currencies, timezones, industries)
  - ZipCode lookups     (/api/zipcodes/lookup/{zip})
  - Lookups             (/api/lookups — categories, items)
  - Enum Management     (/api/enum-management)
  - Lead Score Config   (/api/leadscoring/config  read-only)
  - Lead Sources (link) (/api/leads/{id}/source)
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 16: Master & Catalog Data")
    ts = int(time.time())
    lead_ids = load_ids("leads")
    acct_ids = load_ids("accounts")

    # ─── Lead Sources ──────────────────────────────────────────────────────
    log.section("LeadSources CRUD")
    lead_sources = [
        {"name": f"Website {ts}", "description": "Organic website traffic",
         "isActive": True, "category": "Inbound"},
        {"name": f"Referral {ts}", "description": "Customer and partner referrals",
         "isActive": True, "category": "Referral"},
        {"name": f"LinkedIn Campaign {ts}", "description": "LinkedIn paid ads",
         "isActive": True, "category": "Outbound"},
        {"name": f"Trade Show {ts}", "description": "In-person events and trade shows",
         "isActive": True, "category": "Events"},
        {"name": f"Cold Outreach {ts}", "description": "SDR cold email/call campaigns",
         "isActive": True, "category": "Outbound"},
        {"name": f"Partner Channel {ts}", "description": "ISV and reseller partners",
         "isActive": True, "category": "Partner"},
    ]
    ls_ids = []
    for s in lead_sources:
        eid = api.create_and_track("leadsources", "/api/leadsources", s)
        if eid:
            ls_ids.append(eid)
    api.get("/api/leadsources")
    if ls_ids:
        api.get(f"/api/leadsources/{ls_ids[0]}")
        api.put(f"/api/leadsources/{ls_ids[0]}",
                {**lead_sources[0], "description": "Updated — organic website incl. SEO"})
    # Delete test
    del_ls = {"name": f"DELETE-SRC-{ts}", "description": "Temp", "isActive": False}
    code, body, _ = api.post("/api/leadsources", del_ls)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/leadsources/{body['id']}")
    save_ids("leadsources", ls_ids)

    # Link source to existing leads
    if lead_ids and ls_ids:
        for i, lid in enumerate(lead_ids[:3]):
            src_id = ls_ids[i % len(ls_ids)]
            api.patch(f"/api/leads/{lid}", {"sourceId": src_id})

    # ─── Competitors ───────────────────────────────────────────────────────
    log.section("Competitors CRUD")
    competitors = [
        {"name": f"Salesforce {ts}", "description": "Leading CRM platform",
         "website": "https://www.salesforce.com", "industry": "Software",
         "isActive": True, "threatLevel": 3,
         "strengths": ["Market leader", "Ecosystem", "Brand recognition"],
         "weaknesses": ["High cost", "Complexity", "Long implementation"]},
        {"name": f"HubSpot {ts}", "description": "Mid-market CRM",
         "website": "https://www.hubspot.com", "industry": "Software",
         "isActive": True, "threatLevel": 2,
         "strengths": ["User friendly", "Free tier", "Marketing tools"],
         "weaknesses": ["Limited enterprise features", "Reporting"]},
        {"name": f"Microsoft Dynamics {ts}", "description": "Enterprise CRM from Microsoft",
         "website": "https://dynamics.microsoft.com", "industry": "Software",
         "isActive": True, "threatLevel": 2,
         "strengths": ["Microsoft integration", "Enterprise features"],
         "weaknesses": ["Complex", "High TCO"]},
        {"name": f"Zoho CRM {ts}", "description": "Budget-friendly CRM",
         "website": "https://www.zoho.com/crm", "industry": "Software",
         "isActive": True, "threatLevel": 1,
         "strengths": ["Affordable", "Feature rich"],
         "weaknesses": ["Support quality", "UI complexity"]},
    ]
    comp_ids = []
    for c in competitors:
        payload = {k: v for k, v in c.items()
                   if k not in ("strengths", "weaknesses")}
        payload["strengths"] = c.get("strengths", [])
        payload["weaknesses"] = c.get("weaknesses", [])
        eid = api.create_and_track("competitors", "/api/competitors", payload)
        if eid:
            comp_ids.append(eid)
    api.get("/api/competitors")
    api.get("/api/competitors?activeOnly=true")
    if comp_ids:
        api.get(f"/api/competitors/{comp_ids[0]}")
        api.put(f"/api/competitors/{comp_ids[0]}",
                {**{k: v for k, v in competitors[0].items()
                    if k not in ("strengths", "weaknesses")},
                 "description": "Updated Salesforce entry — added AI features",
                 "threatLevel": 3,
                 "strengths": competitors[0]["strengths"] + ["AI/Einstein"],
                 "weaknesses": competitors[0]["weaknesses"]})
    # Delete test
    del_c = {"name": f"DELETE-COMP-{ts}", "description": "Temp", "isActive": False, "threatLevel": 0}
    code, body, _ = api.post("/api/competitors", del_c)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/competitors/{body['id']}")
    save_ids("competitors", comp_ids)

    # Link competitors to accounts/opportunities
    opp_ids = load_ids("opportunities")
    if opp_ids and comp_ids:
        for i, opp_id in enumerate(opp_ids[:2]):
            comp_id = comp_ids[i % len(comp_ids)]
            api.post(f"/api/opportunities/{opp_id}/competitors/{comp_id}")
            api.get(f"/api/opportunities/{opp_id}/competitors")

    # ─── Currencies ────────────────────────────────────────────────────────
    log.section("Currencies (reference + conversion)")
    api.get("/api/currencies")
    api.get("/api/currencies/rates?base=USD")
    api.get("/api/currencies/rates?base=EUR")
    # Currency conversion
    api.post("/api/currencies/convert", {"fromCurrency": "USD", "toCurrency": "EUR", "amount": 10000.00})
    api.post("/api/currencies/convert", {"fromCurrency": "GBP", "toCurrency": "USD", "amount": 5000.00})
    api.post("/api/currencies/convert", {"fromCurrency": "EUR", "toCurrency": "JPY", "amount": 1000.00})

    # ─── Master Data Reference ─────────────────────────────────────────────
    log.section("MasterData Reference (countries/currencies/timezones/industries)")
    api.get("/api/masterdata/countries")
    api.get("/api/masterdata/currencies")
    api.get("/api/masterdata/timezones")
    api.get("/api/masterdata/industries")
    # Search master data
    api.get("/api/masterdata/countries?search=United")
    api.get("/api/masterdata/industries?search=Tech")

    # ─── ZipCode Lookups ───────────────────────────────────────────────────
    log.section("ZipCode Lookups")
    for zipcode in ["94105", "10001", "60601", "77001", "30301"]:
        api.get(f"/api/zipcodes/lookup/{zipcode}")

    # ─── Lookups & Categories ─────────────────────────────────────────────
    log.section("Lookups CRUD")
    api.get("/api/lookups/categories")
    # Create a custom lookup category
    lookup_cat = {"name": f"Custom Lookup {ts}", "description": "Custom lookup table",
                  "isSystem": False, "isActive": True}
    cat_code, cat_body, _ = api.post("/api/lookups/categories", lookup_cat)
    if cat_body and isinstance(cat_body, dict) and cat_body.get("id"):
        cat_id = cat_body["id"]
        log.track_id("lookup_categories", cat_id)
        api.get(f"/api/lookups/categories/{cat_id}")
        # Add lookup items to the category
        for i, item_name in enumerate(["Option A", "Option B", "Option C"]):
            item = {"categoryId": cat_id, "name": item_name,
                    "value": item_name.lower().replace(" ", "_"),
                    "sortOrder": i + 1, "isActive": True}
            api.post(f"/api/lookups/categories/{cat_id}/items", item)
        api.get(f"/api/lookups/categories/{cat_id}/items")
        save_ids("lookup_categories", [cat_id])

    # ─── Enum Management (read-only introspection) ─────────────────────────
    log.section("Enum Management (read)")
    api.get("/api/enum-management")
    api.get("/api/enum-management/types")
    # Try reading specific enum types
    for enum_name in ["OpportunityStage", "ServiceRequestStatus", "LeadStatus",
                      "CampaignStatus", "ContractStatus"]:
        api.get(f"/api/enum-management/{enum_name}")

    # ─── Lead Scoring Config (read) ────────────────────────────────────────
    log.section("LeadScoring Config (read)")
    api.get("/api/ai/leads/config")
    api.get("/api/leadscoring/config")
    api.get("/api/leadscoring/rules")

    # ─── Admin Seed Data Status ────────────────────────────────────────────
    log.section("SampleData Status (read)")
    api.get("/api/sampledata/status")

    print(f"  Batch 16 done: {log.summary_line()}")

#!/usr/bin/env python3
"""Batch 02: CRM Core entities.

Covers: Accounts, Contacts, ContactInfo (Addresses, Phones, Emails, SocialMedia),
Account-Contact links, Preferences.
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 02: CRM Core - Accounts & Contacts")
    ts = int(time.time())

    # ---- Accounts ----
    log.section("Accounts CRUD")
    accounts = [
        {"company": f"Acme Corp {ts}", "industry": "Technology", "website": "https://acme.example.com",
         "phone": "+1-555-0101", "email": f"info_{ts}@acme.example.com", "accountType": 1, "category": 1,
         "annualRevenue": 5000000, "numberOfEmployees": 250, "description": "Test account 1"},
        {"company": f"Global Industries {ts}", "industry": "Manufacturing", "website": "https://global.example.com",
         "phone": "+1-555-0102", "email": f"info_{ts}@global.example.com", "accountType": 1, "category": 1,
         "annualRevenue": 10000000, "numberOfEmployees": 500, "description": "Test account 2"},
        {"company": f"TechStart Inc {ts}", "industry": "Software", "website": "https://techstart.example.com",
         "phone": "+1-555-0103", "email": f"info_{ts}@techstart.example.com", "accountType": 1, "category": 1,
         "annualRevenue": 1000000, "numberOfEmployees": 50, "description": "Test account 3"},
        {"company": f"ConsultPro LLC {ts}", "industry": "Consulting", "accountType": 1, "category": 1,
         "phone": "+1-555-0104", "email": f"info_{ts}@consultpro.example.com",
         "annualRevenue": 2000000, "numberOfEmployees": 100},
        {"company": f"RetailMax {ts}", "industry": "Retail", "accountType": 1, "category": 1,
         "phone": "+1-555-0105", "email": f"info_{ts}@retailmax.example.com",
         "annualRevenue": 8000000, "numberOfEmployees": 800},
    ]
    acct_ids = []
    for a in accounts:
        eid = api.create_and_track("accounts", "/api/accounts", a)
        if eid:
            acct_ids.append(eid)
    api.get("/api/accounts")
    if acct_ids:
        api.get(f"/api/accounts/{acct_ids[0]}")
        api.put(f"/api/accounts/{acct_ids[0]}", {**accounts[0], "description": "Updated Acme Corp"})
        api.get("/api/accounts/search/Acme")
    # Account filtering
    api.get("/api/accounts/individuals")
    api.get("/api/accounts/organizations")

    # Delete test
    del_payload = {"company": f"DELETE-Acct-{ts}", "industry": "Test", "accountType": 1, "category": 1,
                   "email": f"del_{ts}@test.com", "phone": "+1-555-0199"}
    code, body, _ = api.post("/api/accounts", del_payload)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/accounts/{body['id']}")
    save_ids("accounts", acct_ids)

    # ---- Contacts ----
    log.section("Contacts CRUD")
    contacts = [
        {"firstName": "John", "lastName": f"Smith_{ts}", "emailPrimary": f"john.smith_{ts}@acme.example.com",
         "phonePrimary": "+1-555-1001", "jobTitle": "VP Sales", "contactType": "Customer"},
        {"firstName": "Jane", "lastName": f"Doe_{ts}", "emailPrimary": f"jane.doe_{ts}@global.example.com",
         "phonePrimary": "+1-555-1002", "jobTitle": "CTO", "contactType": "Customer"},
        {"firstName": "Bob", "lastName": f"Johnson_{ts}", "emailPrimary": f"bob.j_{ts}@techstart.example.com",
         "phonePrimary": "+1-555-1003", "jobTitle": "CEO", "contactType": "Partner"},
        {"firstName": "Alice", "lastName": f"Williams_{ts}", "emailPrimary": f"alice.w_{ts}@example.com",
         "phonePrimary": "+1-555-1004", "jobTitle": "Director", "contactType": "Prospect"},
        {"firstName": "Charlie", "lastName": f"Brown_{ts}", "emailPrimary": f"charlie.b_{ts}@example.com",
         "phonePrimary": "+1-555-1005", "jobTitle": "Manager", "contactType": "Customer"},
    ]
    contact_ids = []
    for c in contacts:
        payload = {k: v for k, v in c.items() if v is not None}
        eid = api.create_and_track("contacts", "/api/contacts", payload)
        if eid:
            contact_ids.append(eid)
    api.get("/api/contacts")
    if contact_ids:
        api.get(f"/api/contacts/{contact_ids[0]}")
        api.put(f"/api/contacts/{contact_ids[0]}", {**contacts[0], "jobTitle": "SVP Sales"})
    # Delete test
    del_c = {"firstName": "Delete", "lastName": f"Test_{ts}", "emailPrimary": f"del_{ts}@test.com", "contactType": "Customer"}
    code, body, _ = api.post("/api/contacts", del_c)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/contacts/{body['id']}")
    save_ids("contacts", contact_ids)

    # ---- Contact Info: Addresses ----
    log.section("ContactInfo - Addresses")
    if acct_ids:
        addr_payload = {
            "street": "123 Main St", "city": "San Francisco", "state": "CA",
            "zipCode": "94105", "country": "US", "addressType": 0,
            "entityType": "Account", "entityId": acct_ids[0]
        }
        code, body, _ = api.post("/api/contactinfo/addresses", addr_payload)
        addr_id = body.get("id") if body and isinstance(body, dict) else None
        if addr_id:
            log.track_id("addresses", addr_id)
            # Link address
            api.post("/api/contactinfo/addresses/link", {
                "addressId": addr_id, "entityType": "Account", "entityId": acct_ids[0]
            })
            api.get(f"/api/contactinfo/Account/{acct_ids[0]}/addresses")
            api.put(f"/api/contactinfo/addresses/{addr_id}", {**addr_payload, "street": "456 Oak Ave"})

    # ---- Contact Info: Phones ----
    log.section("ContactInfo - Phones")
    if acct_ids:
        phone_payload = {
            "number": "+1-555-9999", "phoneType": 0, "isPrimary": True,
            "entityType": "Account", "entityId": acct_ids[0]
        }
        code, body, _ = api.post("/api/contactinfo/phones", phone_payload)
        phone_id = body.get("id") if body and isinstance(body, dict) else None
        if phone_id:
            log.track_id("phones", phone_id)
            api.get(f"/api/contactinfo/Account/{acct_ids[0]}/phones")

    # ---- Contact Info: Emails ----
    log.section("ContactInfo - Emails")
    if acct_ids:
        email_payload = {
            "address": f"billing_{ts}@acme.example.com", "emailType": 0,
            "isPrimary": False, "entityType": "Account", "entityId": acct_ids[0]
        }
        api.post("/api/contactinfo/emails", email_payload)

    # ---- Account-Contact Links ----
    log.section("Account-Contact Links")
    if acct_ids and contact_ids and len(contact_ids) > 3:
        # Link a contact to a different account
        api.post(f"/api/accounts/{acct_ids[0]}/contacts", {"contactId": contact_ids[3]})
        api.get(f"/api/accounts/{acct_ids[0]}/direct-contacts")

    # ---- Preferences ----
    # NOTE: Preferences endpoints moved to batch_13_integration.py
    # (returns 404 when PreferencesService is not configured)

    print(f"  Batch 02 done: {log.summary_line()}")

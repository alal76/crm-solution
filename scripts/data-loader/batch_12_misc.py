#!/usr/bin/env python3
"""Batch 12: File Uploads, Self-Service, Pipelines Detail, DocuSeal Webhooks, Email-to-Ticket.

Covers miscellaneous controllers not covered by batch_01..batch_11.
"""
from __future__ import annotations
import sys, os, time, json
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 12: Misc & File Uploads")
    ts = int(time.time())
    account_ids = load_ids("accounts")
    contact_ids = load_ids("contacts")

    # ---- File Upload (list only, upload requires multipart) ----
    log.section("FileUpload")
    api.get("/api/fileupload/files")

    # ---- Self-Service Chatbot ----
    log.section("SelfServiceChatbot")
    api.get("/api/selfservicechatbot/status")

    # ---- Pipelines Detail ----
    log.section("Pipelines Detail")
    pipe_ids = load_ids("pipelines")
    if pipe_ids:
        api.get(f"/api/pipelines/{pipe_ids[0]}/stages")

    # ---- DocuSeal Webhook ----
    log.section("DocuSealWebhook")
    # Typically POST from DocuSeal to our API - just test the health of the webhook endpoint
    api.get("/api/docuseal/status")

    # ---- Email-to-Ticket ----
    log.section("EmailToTicket")
    api.get("/api/emailtoticket/settings")

    # ---- Custom Fields ----
    log.section("CustomFields CRUD")
    cf = {"entityType": "Account", "fieldName": f"cf_test_{ts}",
          "fieldType": "Text", "label": f"Test Field {ts}",
          "isRequired": False}
    eid = api.create_and_track("customfields", "/api/customfields", cf)
    if eid:
        api.get(f"/api/customfields/{eid}")
        api.put(f"/api/customfields/{eid}", {**cf, "label": f"Updated Field {ts}"})
        save_ids("customfields", [eid])
    api.get("/api/customfields")
    api.get("/api/customfields/entity/Account")

    # ---- Tags ----
    log.section("Tags CRUD")
    tag = {"name": f"tag-{ts}", "color": "#FF5733", "entityType": "Account"}
    eid = api.create_and_track("tags", "/api/tags", tag)
    if eid:
        api.get(f"/api/tags/{eid}")
        api.put(f"/api/tags/{eid}", {**tag, "name": f"tag-updated-{ts}"})
        # Tag an account
        if account_ids:
            api.post(f"/api/tags/{eid}/assign", {"entityType": "Account", "entityId": account_ids[0]})
        save_ids("tags", [eid])
    api.get("/api/tags")
    api.get("/api/tags/entity/Account")

    # ---- Merge Records ----
    log.section("MergeRecords")
    if len(account_ids) >= 2:
        # This is destructive so just test the preview endpoint
        api.post("/api/merge/preview", {
            "entityType": "Account",
            "primaryId": account_ids[0],
            "duplicateIds": [account_ids[-1]]
        })

    # ---- Notification Preferences ----
    log.section("NotificationPreferences CRUD")
    np = {"emailNotifications": True, "pushNotifications": False,
          "inAppNotifications": True, "dailyDigest": False}
    api.put("/api/notificationpreferences", np)
    api.get("/api/notificationpreferences")

    # ---- Currency Exchange Rates ----
    log.section("CurrencyExchangeRates")
    api.get("/api/currencyrates")
    api.get("/api/currencyrates/convert?from=USD&to=EUR&amount=100")

    # ---- Document Management ----
    log.section("DocumentManagement")
    api.get("/api/documents")
    doc = {"name": f"TestDoc_{ts}.pdf", "entityType": "Account",
           "entityId": account_ids[0] if account_ids else 1,
           "description": "Test document for loader"}
    eid = api.create_and_track("documents", "/api/documents", doc)
    if eid:
        api.get(f"/api/documents/{eid}")
        save_ids("documents", [eid])

    # ---- DataExport/DataImport ----
    log.section("DataExport / DataImport")
    api.get("/api/dataexport/formats")
    api.get("/api/dataimport/templates")

    # ---- Recurrence Rules ----
    log.section("RecurrenceRules")
    api.get("/api/recurrencerules")

    print(f"  Batch 12 done: {log.summary_line()}")

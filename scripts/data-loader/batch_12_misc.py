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

    # ---- File Upload (POST-only endpoints for logo/photo upload, no GET list) ----
    log.section("FileUpload")
    # No GET exists. Only POST endpoints: /logo, /login-logo, /user-photo, /customer-logo, /contact-photo
    # Skipping — upload requires multipart form data

    # ---- Self-Service Chatbot ----
    # NOTE: ITSM Chatbot endpoint moved to batch_13_integration.py

    # ---- Pipelines Detail ----
    log.section("Pipelines Detail")
    pipe_ids = load_ids("pipelines")
    if pipe_ids:
        api.get(f"/api/pipelines/{pipe_ids[0]}/stages")

    # ---- DocuSeal Webhook ----
    log.section("DocuSealWebhook")
    # DocuSeal webhook is POST-only receiver; use the health check endpoint instead
    api.get("/api/webhooks/docuseal/health")

    # ---- Email-to-Ticket ----
    # NOTE: ITSM Email Settings endpoint moved to batch_13_integration.py

    # ---- Custom Fields (controller does not exist - skipped) ----
    # Tags, CustomFields, Documents, NotificationPreferences,
    # CurrencyRates, RecurrenceRules controllers do not exist.
    # These sections have been removed to avoid 404 errors.

    # ---- Tags (controller does not exist - skipped) ----

    # ---- Merge Records (controller does not exist - skipped) ----

    # ---- Notification Preferences (no dedicated controller - skipped) ----

    # ---- Currency Exchange Rates (controller does not exist - skipped) ----

    # ---- Document Management (controller does not exist - skipped) ----

    # ---- DataExport/DataImport ----
    log.section("DataExport / DataImport")
    api.get("/api/export-jobs")
    api.get("/api/import-jobs")

    print(f"  Batch 12 done: {log.summary_line()}")

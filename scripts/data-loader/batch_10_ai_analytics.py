#!/usr/bin/env python3
"""Batch 10: AI, Analytics, Audit & Integrations.

Covers: AIAgents, AILeadScoring, AIChatbot, AIEmail, AIAnalytics,
AnalyticsEvents, AuditLogs, Reports, Dashboard, DashboardConfig,
ImportJobs, ExportJobs, WebhookRegistrations, Communications.
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 10: AI, Analytics & Integrations")
    ts = int(time.time())
    lead_ids = load_ids("leads")
    opp_ids = load_ids("opportunities")
    user_ids = load_ids("users")

    # ---- AI Agents ----
    # NOTE: AI Agent endpoints moved to batch_13_integration.py
    # (returns 404 when UseExternalAI feature flag is not enabled)

    # ---- AI Lead Scoring ----
    log.section("AI Lead Scoring")
    if lead_ids:
        api.post(f"/api/ai/leads/{lead_ids[0]}/score")
        api.get(f"/api/ai/leads/{lead_ids[0]}/history")
        api.post(f"/api/ai/leads/{lead_ids[0]}/analyze")
    api.get("/api/ai/leads/top")
    api.get("/api/ai/leads/config")

    # ---- AI Chatbot ----
    log.section("AI Chatbot")
    api.get("/api/ai/chatbot/health")
    api.get("/api/ai/chatbot/suggestions")

    # ---- AI Email ----
    log.section("AI Email")
    # EmailAnalysisRequest DTO uses emailContent (not body)
    api.post("/api/ai/email/analyze", {"subject": "Test email subject",
                                        "emailContent": "This is a test email body for analysis."})

    # ---- AI Analytics/Dashboards ----
    log.section("AI Analytics Dashboards")
    api.get("/api/ai/dashboards")
    api.get("/api/ai/reports")
    ai_dash = {"name": f"AI Sales Dashboard {ts}", "description": "AI-powered sales insights",
               "userId": user_ids[0] if user_ids else 1,
               "widgets": []}
    eid = api.create_and_track("aidashboards", "/api/ai/dashboards", ai_dash)
    if eid:
        api.get(f"/api/ai/dashboards/{eid}")
        save_ids("aidashboards", [eid])

    # ---- Analytics Events ----
    log.section("AnalyticsEvents CRUD")
    events = [
        {"eventName": "PageView", "entityType": "Account", "entityId": 1,
         "eventData": '{"page": "/accounts/1"}', "timestamp": "2026-02-22T10:00:00Z"},
        {"eventName": "ButtonClick", "entityType": "Opportunity", "entityId": 1,
         "eventData": '{"button": "CreateQuote"}', "timestamp": "2026-02-22T11:00:00Z"},
    ]
    event_ids = []
    for e in events:
        eid = api.create_and_track("analyticsevents", "/api/analytics-events", e)
        if eid:
            event_ids.append(eid)
    api.get("/api/analytics-events")
    if event_ids:
        api.get(f"/api/analytics-events/{event_ids[0]}")
        api.delete(f"/api/analytics-events/{event_ids[0]}")
    save_ids("analyticsevents", event_ids)

    # ---- Audit Logs ----
    log.section("AuditLogs")
    audit_entry = {"action": "TestAction", "entityType": "Account", "entityId": 1,
                   "details": f"Test audit entry {ts}", "userId": user_ids[0] if user_ids else 1}
    api.post("/api/audit-logs", audit_entry)
    api.get("/api/audit-logs")
    api.get("/api/audit-logs/entity/Account/1")
    api.get("/api/audit-logs/statistics")
    if user_ids:
        api.get(f"/api/audit-logs/user/{user_ids[0]}")

    # ---- Reports ----
    log.section("Reports")
    api.get("/api/reports")
    api.get("/api/ai/reports")

    # ---- Dashboard ----
    log.section("Dashboard")
    # No base GET /api/dashboard exists; use sub-paths
    api.get("/api/dashboard/stats")
    api.get("/api/dashboard/pipeline")
    api.get("/api/itsm/dashboard/executive-summary")

    # ---- Dashboard Config ----
    # NOTE: Dashboard Config endpoint moved to batch_13_integration.py

    # ---- Import/Export Jobs ----
    log.section("ImportJobs CRUD")
    imp = {"entity": "Account", "source": f"accounts_{ts}.csv",
           "status": "Pending", "submittedByUserId": user_ids[0] if user_ids else 1}
    eid = api.create_and_track("importjobs", "/api/import-jobs", imp)
    if eid:
        api.get(f"/api/import-jobs/{eid}")
        save_ids("importjobs", [eid])
    api.get("/api/import-jobs")

    log.section("ExportJobs CRUD")
    exp = {"entity": "Contact", "destination": "csv",
           "status": "Pending", "requestedByUserId": user_ids[0] if user_ids else 1}
    eid = api.create_and_track("exportjobs", "/api/export-jobs", exp)
    if eid:
        api.get(f"/api/export-jobs/{eid}")
        save_ids("exportjobs", [eid])
    api.get("/api/export-jobs")

    # ---- Webhook Registrations ----
    log.section("WebhookRegistrations CRUD")
    wh = {"name": f"Account Created Webhook {ts}", "url": "https://example.com/webhook/account",
          "eventType": "Account.Created", "isActive": True, "secret": "webhook-secret-123"}
    eid = api.create_and_track("webhookregistrations", "/api/webhook-registrations", wh)
    if eid:
        api.get(f"/api/webhook-registrations/{eid}")
        # No PUT endpoint exists on WebhookRegistrationsController (only POST, GET, DELETE)
        # api.put(f"/api/webhook-registrations/{eid}", {**wh, "url": "https://example.com/webhook/v2/account"})
        save_ids("webhookregistrations", [eid])
    api.get("/api/webhook-registrations")

    # ---- Communications ----
    log.section("Communications CRUD")
    # Channels
    api.get("/api/communications/channels")
    ch = {"name": f"Email Channel {ts}", "channelType": "Email", "isActive": True,
          "configuration": '{"smtp": "mail.example.com"}'}
    eid = api.create_and_track("commchannels", "/api/communications/channels", ch)
    if eid:
        api.get(f"/api/communications/channels/{eid}")
        save_ids("commchannels", [eid])
    # Templates
    api.get("/api/communications/templates")
    ct = {"name": f"Welcome Notification {ts}", "subject": "Welcome!",
          "body": "Welcome to our service.", "type": "Email"}
    eid = api.create_and_track("commtemplates", "/api/communications/templates", ct)
    if eid:
        api.get(f"/api/communications/templates/{eid}")
        save_ids("commtemplates", [eid])
    # Messages
    api.get("/api/communications/messages")
    api.get("/api/communications/conversations")

    # ---- AI Agent Usage ----
    log.section("AIAgentUsage")
    usage = {"agentId": "lead-scoring", "userId": user_ids[0] if user_ids else 1,
             "action": "chat", "tokensUsed": 500,
             "timestamp": "2026-02-22T12:00:00Z", "duration": 2.5}
    api.post("/api/ai-agent-usage", usage)
    api.get("/api/ai-agent-usage")

    print(f"  Batch 10 done: {log.summary_line()}")

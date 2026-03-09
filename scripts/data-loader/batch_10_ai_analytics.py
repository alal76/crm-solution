#!/usr/bin/env python3
"""Batch 10: AI, Analytics & Integrations — comprehensive coverage.

Covers: AI Agents (SK), AI Lead Scoring, AI Opportunity Insights, AI Email,
AI Account Health, AI Chatbot, LLM endpoints, Analytics Events, Reports,
Dashboard (extended), AI Dashboards, Audit Logs, Import/Export Jobs,
Webhook Registrations, AI Agent Usage Tracking.
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, save_ids, load_ids


# =============================================================================
# SECTION 1 — SEMANTIC KERNEL AGENTS
# =============================================================================

def _sk_agents(api: ApiClient, log: RunLogger, ts: int,
               user_ids: list) -> None:
    log.section("Semantic Kernel Agents (check /api/agents, chat, feedback)")

    code, body, _ = api.get("/api/agents")
    if code == 404:
        log.log("  SKIP: /api/agents returned 404 — UseExternalAI flag off")
        return

    # Use integer IDs from the agents list (route uses {agentId:int})
    agent_ids = []
    if code == 200 and isinstance(body, (list, dict)):
        items = body if isinstance(body, list) else body.get("items", body.get("data", []))
        if isinstance(items, list):
            agent_ids = [a["id"] for a in items if "id" in a]
    if not agent_ids:
        log.log("  SKIP: No agents returned from /api/agents — SK agents not seeded or AI disabled")
        return

    for agent_id in agent_ids[:3]:  # Test with first 3 agents
        api.get(f"/api/agents/{agent_id}/conversations")
        api.post(f"/api/agents/{agent_id}/chat", {
            "message": "Give me a brief summary relevant to this agent",
            "userId": user_ids[0] if user_ids else 1,
            "sessionId": f"test-session-{ts}",
        })

    api.get("/api/agents/analytics/usage")
    api.get("/api/agents/analytics/by-agent")
    api.get("/api/agents/analytics/cost-summary")
    print("    SK Agents: chat + feedback done")


# =============================================================================
# SECTION 2 — AI LEAD SCORING
# =============================================================================

def _ai_lead_scoring(api: ApiClient, log: RunLogger, lead_ids: list) -> None:
    log.section("AI Lead Scoring (score, analyze, history, config)")

    for lid in lead_ids[:5]:
        api.post(f"/api/ai/leads/{lid}/score")
        api.get(f"/api/ai/leads/{lid}/history")
        api.post(f"/api/ai/leads/{lid}/analyze")

    api.get("/api/ai/leads/top?limit=10")
    api.get("/api/ai/leads/config")

    if lead_ids:
        ids_param = ",".join(str(i) for i in lead_ids[:5])
        api.get(f"/api/ai/leads/batch-scores?leadIds={ids_param}")

    # SKIP: PUT /api/ai/leads/config not implemented; only GET exists (405)
    print("    AI Lead Scoring: done")


# =============================================================================
# SECTION 3 — AI OPPORTUNITY INSIGHTS
# =============================================================================

def _ai_opportunity_insights(api: ApiClient, log: RunLogger,
                              opp_ids: list) -> None:
    log.section("AI Opportunity Insights (analyze, recommendations, win-prob)")

    for oid in opp_ids[:3]:
        api.post(f"/api/ai/opportunities/{oid}/analyze")
        api.get(f"/api/ai/opportunities/{oid}/recommendations")
        api.post(f"/api/ai/opportunities/{oid}/win-probability")

    api.get("/api/ai/opportunities/risk-report")
    print("    AI Opportunity Insights: done")


# =============================================================================
# SECTION 4 — AI EMAIL ANALYSIS
# =============================================================================

def _ai_email(api: ApiClient, log: RunLogger) -> None:
    log.section("AI Email (analyze multiple samples, generate, summarize)")

    samples = [
        {
            "subject": "Excited about your product — ready to move forward",
            "emailContent": "Hi, I just finished the demo and I am very impressed. We have budget approved and I would like to schedule a call to sign the contract next week.",
            "senderEmail": "prospect@enterprise.com",
        },
        {
            "subject": "Concerns about implementation timeline",
            "emailContent": "We are worried about the 3-month implementation timeline mentioned in the proposal. Our current vendor contract ends in 6 weeks and we need a faster transition or we will have to look elsewhere.",
            "senderEmail": "decision.maker@prospect.com",
        },
        {
            "subject": "Following up on last week's meeting",
            "emailContent": "Just wanted to follow up on the points we discussed. No rush, just checking in to see if there are any questions. Let me know when you want to reconnect.",
            "senderEmail": "rep@example.com",
        },
    ]
    for sample in samples:
        api.post("/api/ai/email/analyze", sample)

    # EmailGenerateRequest: Prompt (required), Tone (optional string)
    api.post("/api/ai/email/generate", {
        "prompt": "Write a professional follow-up email after a product demo. "
                  "The prospect, John Smith at Enterprise Corp, is interested but concerned about price. "
                  "Emphasize ROI and mention flexible pricing options.",
        "tone": "Professional",
    })

    api.post("/api/ai/email/summarize", {
        "emailContent": "Long email thread with multiple exchanges about pricing, implementation, and support SLA requirements...",
        "maxLength": 200,
    })
    print("    AI Email: done")


# =============================================================================
# SECTION 5 — AI ACCOUNT HEALTH
# =============================================================================

def _ai_account_health(api: ApiClient, log: RunLogger,
                       account_ids: list) -> None:
    log.section("AI Account Health (health-score, at-risk, analyze)")

    for aid in account_ids[:3]:
        api.get(f"/api/ai/accounts/{aid}/health-score")
        api.post(f"/api/ai/accounts/{aid}/analyze")

    api.get("/api/ai/accounts/at-risk")
    print("    AI Account Health: done")


# =============================================================================
# SECTION 6 — AI CHATBOT
# =============================================================================

def _ai_chatbot(api: ApiClient, log: RunLogger) -> None:
    log.section("AI Chatbot (health, messages, suggestions, history)")

    api.get("/api/ai/chatbot/health")
    api.get("/api/ai/chatbot/suggestions")

    queries = [
        "How many open opportunities do we have this quarter?",
        "Show me the top 5 leads by score",
        "What is the average deal size for ClosedWon opportunities?",
        "Which accounts have not been contacted in the last 30 days?",
    ]
    for q in queries:
        api.post("/api/ai/chatbot/message", {"message": q, "sessionId": "test-batch-10"})

    api.get("/api/ai/chatbot/history?sessionId=test-batch-10")
    print("    AI Chatbot: done")


# =============================================================================
# SECTION 7 — LLM ENDPOINTS
# =============================================================================

def _llm_endpoints(api: ApiClient, log: RunLogger) -> None:
    log.section("LLM Endpoints (providers, complete, chat, models, embed, health)")

    api.get("/api/llm/health")
    api.get("/api/llm/providers")
    api.get("/api/llm/models")

    api.post("/api/llm/complete", {
        "prompt": "Summarize the key benefits of a CRM system for a mid-size sales team in 3 bullet points.",
        "maxTokens": 200,
        "temperature": 0.7,
    })

    # LlmChatRequest: Message (single string), not messages array
    api.post("/api/llm/chat", {
        "message": "What is the best way to qualify a sales lead?",
        "maxTokens": 300,
        "temperature": 0.5,
    })

    api.post("/api/llm/embed", {
        "text": "Customer relationship management software for enterprise sales teams",
    })
    print("    LLM Endpoints: done")


# =============================================================================
# SECTION 8 — ANALYTICS EVENTS
# =============================================================================

def _analytics_events(api: ApiClient, log: RunLogger, ts: int,
                      account_ids: list, opp_ids: list,
                      lead_ids: list) -> list:
    log.section("Analytics Events (10+ varied events, CRUD)")

    aid1 = account_ids[0] if account_ids else 1
    aid2 = account_ids[1] if len(account_ids) > 1 else aid1
    oid1 = opp_ids[0] if opp_ids else 1
    lid1 = lead_ids[0] if lead_ids else 1

    events = [
        {"eventName": "PageView", "entityType": "Account", "entityId": aid1,
         "eventData": '{"page":"/accounts/detail"}', "timestamp": "2026-03-01T08:00:00Z"},
        {"eventName": "PageView", "entityType": "Opportunity", "entityId": oid1,
         "eventData": '{"page":"/opportunities/detail"}', "timestamp": "2026-03-01T08:05:00Z"},
        {"eventName": "ButtonClick", "entityType": "Account", "entityId": aid1,
         "eventData": '{"button":"CreateContact"}', "timestamp": "2026-03-01T08:10:00Z"},
        {"eventName": "FormSubmit", "entityType": "Lead", "entityId": lid1,
         "eventData": '{"form":"web-to-lead"}', "timestamp": "2026-03-01T08:15:00Z"},
        {"eventName": "FileDownload", "entityType": "Account", "entityId": aid2,
         "eventData": '{"file":"proposal.pdf"}', "timestamp": "2026-03-01T08:20:00Z"},
        {"eventName": "EmailOpened", "entityType": "Lead", "entityId": lid1,
         "eventData": '{"campaign":"spring-promo"}', "timestamp": "2026-03-01T09:00:00Z"},
        {"eventName": "LinkClicked", "entityType": "Lead", "entityId": lid1,
         "eventData": '{"url":"https://crm.example.com/pricing"}', "timestamp": "2026-03-01T09:05:00Z"},
        {"eventName": "DemoRequested", "entityType": "Lead", "entityId": lid1,
         "eventData": '{"source":"website"}', "timestamp": "2026-03-01T09:10:00Z"},
        {"eventName": "LoginEvent", "entityType": "Account", "entityId": aid1,
         "eventData": '{"portal":"customer"}', "timestamp": "2026-03-01T09:30:00Z"},
        {"eventName": "SearchQuery", "entityType": "Account", "entityId": aid2,
         "eventData": '{"query":"integration API"}', "timestamp": "2026-03-01T09:45:00Z"},
        {"eventName": "FeatureUsed", "entityType": "Opportunity", "entityId": oid1,
         "eventData": '{"feature":"ai-scoring"}', "timestamp": "2026-03-01T10:00:00Z"},
    ]

    event_ids = []
    for e in events:
        eid = api.create_and_track("analyticsevents",
                                   "/api/analytics-events", e)
        if eid:
            event_ids.append(eid)

    api.get("/api/analytics-events")
    api.get("/api/analytics-events?entityType=Account")
    api.get("/api/analytics-events?entityType=Lead")
    # SKIP: /api/analytics-events/summary not implemented (404)
    # api.get("/api/analytics-events/summary")

    if event_ids:
        api.get(f"/api/analytics-events/{event_ids[0]}")
        # Delete the last event (tests delete lifecycle)
        api.delete(f"/api/analytics-events/{event_ids[-1]}")
        event_ids.pop()

    save_ids("analyticsevents", event_ids)
    print(f"    Analytics Events: {len(event_ids)} created")
    return event_ids


# =============================================================================
# SECTION 9 — REPORTS
# =============================================================================

def _reports(api: ApiClient, log: RunLogger, ts: int) -> list:
    log.section("Reports (create, get, run, results, scheduled, templates)")

    api.get("/api/reports")
    # SKIP: /api/reports/templates and /api/reports/scheduled not implemented (404)
    # api.get("/api/reports/templates")
    # api.get("/api/reports/scheduled")

    reports_defs = [
        {
            "name": f"Q1 Sales Pipeline Report {ts}",
            "description": "Pipeline breakdown by stage and owner for Q1",
            "reportType": "Pipeline",
            "entityType": "Opportunity",
            "filters": '{"quarter":"Q1","year":2026}',
            "isScheduled": False,
        },
        {
            "name": f"Lead Conversion Rate {ts}",
            "description": "Monthly lead conversion funnel analysis",
            "reportType": "Conversion",
            "entityType": "Lead",
            "filters": '{"dateRange":"last90days"}',
            "isScheduled": False,
        },
        {
            "name": f"Weekly Revenue Forecast {ts}",
            "description": "Automated weekly revenue forecast report",
            "reportType": "Forecast",
            "entityType": "Opportunity",
            "filters": '{"horizon":"90days"}',
            "isScheduled": True,
            "scheduleExpression": "0 7 * * 1",
        },
    ]

    report_ids = []
    for r in reports_defs:
        payload = {k: v for k, v in r.items() if v is not None}
        eid = api.create_and_track("reports", "/api/reports", payload)
        if eid:
            report_ids.append(eid)

    if report_ids:
        api.get(f"/api/reports/{report_ids[0]}")
        # SKIP: /api/reports/{id}/run and /api/reports/{id}/results not implemented (404)
        # api.post(f"/api/reports/{report_ids[0]}/run", {})
        # api.get(f"/api/reports/{report_ids[0]}/results")
        api.get("/api/ai/reports")

        # Delete test
        extra_r = {
            "name": f"Temp Report Delete {ts}",
            "reportType": "Pipeline",
            "entityType": "Opportunity",
        }
        extra_id = api.create_and_track("reports", "/api/reports", extra_r)
        if extra_id:
            api.delete(f"/api/reports/{extra_id}")

    save_ids("reports", report_ids)
    print(f"    Reports: {len(report_ids)} created")
    return report_ids


# =============================================================================
# SECTION 10 — DASHBOARD
# =============================================================================

def _dashboard(api: ApiClient, log: RunLogger) -> None:
    log.section("Dashboard (extended — sales, ITSM, marketing, financial metrics)")

    api.get("/api/dashboard/stats")
    api.get("/api/dashboard/pipeline")
    api.get("/api/dashboard/sales-metrics")
    api.get("/api/dashboard/itsm-metrics")
    api.get("/api/dashboard/marketing-metrics")
    api.get("/api/dashboard/financial-metrics")
    api.get("/api/itsm/dashboard/executive-summary")
    api.get("/api/itsm/dashboard/sla-compliance")
    api.get("/api/itsm/dashboard/queue-stats")
    print("    Dashboard: reads done")


# =============================================================================
# SECTION 11 — AI DASHBOARDS
# =============================================================================

def _ai_dashboards(api: ApiClient, log: RunLogger, ts: int,
                   user_ids: list) -> list:
    log.section("AI Dashboards (3 dashboards with widgets, update, delete test)")

    uid = user_ids[0] if user_ids else 1

    dashboards = [
        {
            "name": f"AI Sales Performance {ts}",
            "description": "AI-powered sales KPIs and pipeline insights",
            "userId": uid,
            "isDefault": False,
            "widgets": [
                {"type": "PipelineChart", "title": "Pipeline by Stage",
                 "config": {"stageFilter": "all"}, "column": 0, "row": 0, "width": 6, "height": 4},
                {"type": "LeadScoreDistribution", "title": "Lead Score Heatmap",
                 "config": {"topN": 50}, "column": 6, "row": 0, "width": 6, "height": 4},
            ],
        },
        {
            "name": f"AI Customer Health Dashboard {ts}",
            "description": "Account health scores and churn risk indicators",
            "userId": uid,
            "isDefault": False,
            "widgets": [
                {"type": "HealthScoreGauge", "title": "Avg Account Health",
                 "config": {"threshold": 70}, "column": 0, "row": 0, "width": 6, "height": 4},
                {"type": "AtRiskAccounts", "title": "Accounts at Risk",
                 "config": {"riskThreshold": 40}, "column": 6, "row": 0, "width": 6, "height": 4},
            ],
        },
        {
            "name": f"AI ITSM Operations {ts}",
            "description": "Intelligent ITSM insights and SLA prediction",
            "userId": uid,
            "isDefault": False,
            "widgets": [
                {"type": "TicketVolumeTrend", "title": "Ticket Volume",
                 "config": {"days": 30}, "column": 0, "row": 0, "width": 6, "height": 4},
                {"type": "SLABreachPredictor", "title": "SLA Risk",
                 "config": {"horizon": "24h"}, "column": 6, "row": 0, "width": 6, "height": 4},
            ],
        },
    ]

    dash_ids = []
    for d in dashboards:
        payload = {k: v for k, v in d.items() if v is not None}
        eid = api.create_and_track("aidashboards", "/api/ai/dashboards", payload)
        if eid:
            dash_ids.append(eid)

    api.get("/api/ai/dashboards")
    if dash_ids:
        api.get(f"/api/ai/dashboards/{dash_ids[0]}")
        api.put(f"/api/ai/dashboards/{dash_ids[0]}", {
            **dashboards[0],
            "description": "Updated: AI sales KPIs with forecasting widget",
        })

        # Delete test
        extra_d = {"name": f"Temp AI Dashboard Delete {ts}", "userId": uid,
                   "isPublic": False}
        extra_id = api.create_and_track("aidashboards",
                                        "/api/ai/dashboards", extra_d)
        if extra_id:
            api.delete(f"/api/ai/dashboards/{extra_id}")

    save_ids("aidashboards", dash_ids)
    print(f"    AI Dashboards: {len(dash_ids)} created")
    return dash_ids


# =============================================================================
# SECTION 12 — AUDIT LOGS
# =============================================================================

def _audit_logs(api: ApiClient, log: RunLogger, ts: int,
                account_ids: list, user_ids: list) -> None:
    log.section("Audit Logs (write + extended reads)")

    entry = {
        "action": "DataLoaderTest",
        "entityType": "Account",
        "entityId": account_ids[0] if account_ids else 1,
        "details": f"Automated audit log entry from batch_10 data loader {ts}",
        "userId": user_ids[0] if user_ids else 1,
    }
    api.post("/api/audit-logs", entry)

    api.get("/api/audit-logs")
    api.get("/api/audit-logs?entityType=Account")
    aid = account_ids[0] if account_ids else 1
    api.get(f"/api/audit-logs/entity/Account/{aid}")
    api.get("/api/audit-logs/statistics")
    if user_ids:
        api.get(f"/api/audit-logs/user/{user_ids[0]}")
    print("    Audit Logs: reads done")


# =============================================================================
# SECTION 13 — IMPORT / EXPORT JOBS
# =============================================================================

def _import_export_jobs(api: ApiClient, log: RunLogger, ts: int,
                        user_ids: list) -> None:
    log.section("Import/Export Jobs (3 import + 3 export)")

    uid = user_ids[0] if user_ids else 1

    import_defs = [
        {"entity": "Account", "source": f"accounts_{ts}.csv",
         "status": "Pending", "submittedByUserId": uid},
        {"entity": "Contact", "source": f"contacts_{ts}.csv",
         "status": "Pending", "submittedByUserId": uid},
        {"entity": "Lead", "source": f"leads_{ts}.xlsx",
         "status": "Pending", "submittedByUserId": uid},
    ]
    import_ids = []
    for imp in import_defs:
        eid = api.create_and_track("importjobs", "/api/import-jobs", imp)
        if eid:
            import_ids.append(eid)
            api.get(f"/api/import-jobs/{eid}")

    api.get("/api/import-jobs")
    if import_ids:
        api.get(f"/api/import-jobs/{import_ids[0]}/status")
    save_ids("importjobs", import_ids)

    export_defs = [
        {"entity": "Account", "destination": "csv",
         "status": "Pending", "requestedByUserId": uid},
        {"entity": "Contact", "destination": "xlsx",
         "status": "Pending", "requestedByUserId": uid},
        {"entity": "Opportunity", "destination": "csv",
         "status": "Pending", "requestedByUserId": uid},
    ]
    export_ids = []
    for exp in export_defs:
        eid = api.create_and_track("exportjobs", "/api/export-jobs", exp)
        if eid:
            export_ids.append(eid)
            api.get(f"/api/export-jobs/{eid}")

    api.get("/api/export-jobs")
    if export_ids:
        # SKIP: /api/export-jobs/{id}/status not implemented (404)
        pass  # api.get(f"/api/export-jobs/{export_ids[0]}/status")
    save_ids("exportjobs", export_ids)

    print(f"    Import/Export Jobs: {len(import_ids)} imports, {len(export_ids)} exports created")


# =============================================================================
# SECTION 14 — WEBHOOK REGISTRATIONS
# =============================================================================

def _webhook_registrations(api: ApiClient, log: RunLogger, ts: int) -> list:
    log.section("Webhook Registrations (8+ event types, CRUD)")

    secret = os.environ.get("WEBHOOK_SECRET", "webhook-secret-batch10")  # NOSONAR

    webhooks = [
        {"name": f"Account Created {ts}", "url": "https://example.com/webhooks/account-created",
         "eventType": "Account.Created", "isActive": True, "secret": secret},
        {"name": f"Opportunity Won {ts}", "url": "https://example.com/webhooks/deal-won",
         "eventType": "Opportunity.Won", "isActive": True, "secret": secret},
        {"name": f"Lead Qualified {ts}", "url": "https://example.com/webhooks/lead-qualified",
         "eventType": "Lead.Qualified", "isActive": True, "secret": secret},
        {"name": f"Ticket Created {ts}", "url": "https://example.com/webhooks/ticket-created",
         "eventType": "ServiceRequest.Created", "isActive": True, "secret": secret},
        {"name": f"Invoice Paid {ts}", "url": "https://example.com/webhooks/invoice-paid",
         "eventType": "Invoice.Paid", "isActive": True, "secret": secret},
        {"name": f"Contact Updated {ts}", "url": "https://example.com/webhooks/contact-updated",
         "eventType": "Contact.Updated", "isActive": True, "secret": secret},
        {"name": f"Campaign Completed {ts}", "url": "https://example.com/webhooks/campaign-done",
         "eventType": "Campaign.Completed", "isActive": True, "secret": secret},
        {"name": f"Approval Requested {ts}", "url": "https://example.com/webhooks/approval-req",
         "eventType": "Approval.Requested", "isActive": True, "secret": secret},
        {"name": f"Contract Signed {ts}", "url": "https://example.com/webhooks/contract-signed",
         "eventType": "Contract.Signed", "isActive": True, "secret": secret},
    ]

    wh_ids = []
    for wh in webhooks:
        payload = {k: v for k, v in wh.items() if v is not None}
        eid = api.create_and_track("webhookregistrations",
                                   "/api/webhook-registrations", payload)
        if eid:
            wh_ids.append(eid)

    api.get("/api/webhook-registrations")
    if wh_ids:
        api.get(f"/api/webhook-registrations/{wh_ids[0]}")
        # Test delivery (best-effort)
        api.post(f"/api/webhook-registrations/{wh_ids[0]}/test", {})
        # Delete the last webhook (tests delete lifecycle)
        api.delete(f"/api/webhook-registrations/{wh_ids[-1]}")
        wh_ids.pop()

    save_ids("webhookregistrations", wh_ids)
    print(f"    Webhook Registrations: {len(wh_ids)} active")
    return wh_ids


# =============================================================================
# SECTION 15 — AI AGENT USAGE TRACKING
# =============================================================================

def _ai_agent_usage(api: ApiClient, log: RunLogger, ts: int,
                    user_ids: list) -> None:
    log.section("AI Agent Usage Tracking (create multiple, summary reads)")

    uid = user_ids[0] if user_ids else 1
    uid2 = user_ids[1] if len(user_ids) > 1 else uid

    usage_records = [
        {"agentId": "lead-scoring", "userId": uid,
         "action": "score", "tokensUsed": 350, "duration": 1.2,
         "timestamp": "2026-03-01T09:00:00Z"},
        {"agentId": "support-triage", "userId": uid,
         "action": "classify", "tokensUsed": 280, "duration": 0.9,
         "timestamp": "2026-03-01T09:15:00Z"},
        {"agentId": "sales-coach", "userId": uid2,
         "action": "chat", "tokensUsed": 650, "duration": 3.1,
         "timestamp": "2026-03-01T10:00:00Z"},
        {"agentId": "sentiment-analysis", "userId": uid,
         "action": "analyze", "tokensUsed": 200, "duration": 0.7,
         "timestamp": "2026-03-01T10:30:00Z"},
        {"agentId": "financial-insights", "userId": uid2,
         "action": "report", "tokensUsed": 900, "duration": 4.2,
         "timestamp": "2026-03-01T11:00:00Z"},
    ]

    for u in usage_records:
        api.post("/api/ai-agent-usage", u)

    api.get("/api/ai-agent-usage")
    # SKIP: /api/ai-agent-usage/summary not implemented (404)
    # SKIP: /api/ai-agent-usage/by-agent not implemented (404)
    print("    AI Agent Usage: records posted + reads done")


# =============================================================================
# ENTRY POINT
# =============================================================================

def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 10: AI, Analytics & Integrations")
    ts = int(time.time())

    user_ids    = load_ids("users")
    lead_ids    = load_ids("leads")
    opp_ids     = load_ids("opportunities")
    account_ids = load_ids("accounts")
    contact_ids = load_ids("contacts")  # noqa: F841

    _sk_agents(api, log, ts, user_ids)
    _ai_lead_scoring(api, log, lead_ids)
    _ai_opportunity_insights(api, log, opp_ids)
    _ai_email(api, log)
    _ai_account_health(api, log, account_ids)
    _ai_chatbot(api, log)
    _llm_endpoints(api, log)
    _analytics_events(api, log, ts, account_ids, opp_ids, lead_ids)
    _reports(api, log, ts)
    _dashboard(api, log)
    _ai_dashboards(api, log, ts, user_ids)
    _audit_logs(api, log, ts, account_ids, user_ids)
    _import_export_jobs(api, log, ts, user_ids)
    _webhook_registrations(api, log, ts)
    _ai_agent_usage(api, log, ts, user_ids)

    print(f"  Batch 10 done: {log.summary_line()}")

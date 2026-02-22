#!/usr/bin/env python3
"""Batch 13: Integration-Dependent Endpoints.

These 32 endpoints return HTTP 404 when their backing services are not
configured (feature flags, external providers, runtime configuration).
Each service group is probed first; if the service is unavailable the
entire group is skipped gracefully rather than counted as failures.

Service groups:
  - AI Agents (UseExternalAI feature flag)
  - Dashboard Config
  - Monitoring
  - Performance Monitoring
  - Master Data
  - Workers / Worker Control
  - Duplicate Detection
  - Normalization
  - Email Integration
  - ITSM Chatbot
  - ITSM Email-to-Ticket
  - News / Social Feed
  - Test Results
  - Auth Diagnostics
  - User Profiles (by ID)
  - Preferences
  - Workflow Instances (by definition)
  - Workflow Tasks (by user)
"""
from __future__ import annotations

import sys
import os
import time

sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import (
    ApiClient, RunLogger, check_service_availability, save_ids, load_ids,
)


# ── Service group definitions ──────────────────────────────────────
# Each tuple: (group_name, probe_endpoint, description, callable)
# The callable receives (api, log, skip_tracker) and returns the number
# of endpoints that would have been called.


def _ai_agents(api: ApiClient, log: RunLogger) -> int:
    """AI Agent endpoints — requires UseExternalAI feature flag."""
    log.section("AI Agents (integration)")
    api.get("/api/agents")
    api.get("/api/agents/admin")
    api.get("/api/agents/analytics/usage")
    api.get("/api/agents/analytics/accuracy")
    api.get("/api/agents/analytics/cost")
    return 5


def _dashboard_config(api: ApiClient, log: RunLogger) -> int:
    """Dashboard configuration service."""
    log.section("Dashboard Config (integration)")
    api.get("/api/dashboard-config")
    return 1


def _monitoring(api: ApiClient, log: RunLogger) -> int:
    """Monitoring service endpoints."""
    log.section("Monitoring (integration)")
    api.get("/api/monitoring/status")
    api.get("/api/monitoring/metrics")
    api.get("/api/monitoring/alerts")
    return 3


def _performance(api: ApiClient, log: RunLogger) -> int:
    """Performance monitoring service."""
    log.section("Performance Monitoring (integration)")
    api.get("/api/performance/metrics")
    api.get("/api/performance/slow-queries")
    return 2


def _master_data(api: ApiClient, log: RunLogger) -> int:
    """Master data reference service."""
    log.section("Master Data (integration)")
    api.get("/api/masterdata/countries")
    api.get("/api/masterdata/currencies")
    api.get("/api/masterdata/timezones")
    api.get("/api/masterdata/industries")
    return 4


def _workers(api: ApiClient, log: RunLogger) -> int:
    """Worker / background-job service."""
    log.section("Workers (integration)")
    api.get("/api/workers")
    api.get("/api/workers/control/status")
    return 2


def _duplicates(api: ApiClient, log: RunLogger) -> int:
    """Duplicate detection service."""
    log.section("Duplicate Detection (integration)")
    api.get("/api/duplicates/accounts")
    api.get("/api/duplicates/contacts")
    return 2


def _normalization(api: ApiClient, log: RunLogger) -> int:
    """Data normalization service."""
    log.section("Normalization (integration)")
    api.post("/api/normalization/phone", {"value": "+1 (555) 123-4567"})
    api.post("/api/normalization/email", {"value": "  Test@Example.COM  "})
    return 2


def _email_integration(api: ApiClient, log: RunLogger) -> int:
    """Email integration service."""
    log.section("Email Integration (integration)")
    api.get("/api/email/status")
    return 1


def _itsm_chatbot(api: ApiClient, log: RunLogger) -> int:
    """ITSM chatbot service."""
    log.section("ITSM Chatbot (integration)")
    api.get("/api/itsm/chatbot/status")
    return 1


def _itsm_email(api: ApiClient, log: RunLogger) -> int:
    """ITSM email-to-ticket integration."""
    log.section("ITSM Email-to-Ticket (integration)")
    api.get("/api/itsm/email/settings")
    return 1


def _news_social(api: ApiClient, log: RunLogger) -> int:
    """News / social feed service."""
    log.section("News/Social Feed (integration)")
    api.get("/api/news-social/feed")
    return 1


def _test_results(api: ApiClient, log: RunLogger) -> int:
    """Test results service."""
    log.section("Test Results (integration)")
    api.get("/api/testresults")
    return 1


def _auth_diagnostics(api: ApiClient, log: RunLogger) -> int:
    """Auth diagnostics service."""
    log.section("Auth Diagnostics (integration)")
    api.get("/api/auth-diagnostics/known-issues")
    return 1


def _user_profiles(api: ApiClient, log: RunLogger) -> int:
    """User profiles service (by ID)."""
    log.section("User Profiles (integration)")
    user_ids = load_ids("users")
    if user_ids:
        api.get(f"/api/userprofiles/{user_ids[0]}")
        return 1
    return 0


def _preferences(api: ApiClient, log: RunLogger) -> int:
    """Preferences service."""
    log.section("Preferences (integration)")
    acct_ids = load_ids("accounts")
    contact_ids = load_ids("contacts")
    count = 0
    if acct_ids:
        api.get(f"/api/preferences/account/{acct_ids[0]}")
        count += 1
    if contact_ids:
        api.get(f"/api/preferences/contact/{contact_ids[0]}")
        count += 1
    return count


def _workflow_instances(api: ApiClient, log: RunLogger) -> int:
    """Workflow instances by definition ID."""
    log.section("Workflow Instances by Def (integration)")
    wf_ids = load_ids("workflows")
    if wf_ids:
        api.get(f"/api/workflow-instances/definition/{wf_ids[0]}")
        return 1
    return 0


def _workflow_tasks_user(api: ApiClient, log: RunLogger) -> int:
    """Workflow tasks by user ID."""
    log.section("Workflow Tasks by User (integration)")
    user_ids = load_ids("users")
    if user_ids:
        api.get(f"/api/workflows/tasks/user/{user_ids[0]}")
        return 1
    return 0


# ── Registry of all service groups ────────────────────────────────
# (group_name, probe_endpoint, feature_flag_or_description, handler_fn, endpoint_count)
SERVICE_GROUPS = [
    ("AI Agents",             "/api/agents",                     "UseExternalAI feature flag",          _ai_agents,          5),
    ("Dashboard Config",      "/api/dashboard-config",           "Dashboard configuration service",     _dashboard_config,   1),
    ("Monitoring",            "/api/monitoring/status",           "Monitoring service",                  _monitoring,         3),
    ("Performance",           "/api/performance/metrics",         "Performance monitoring service",      _performance,        2),
    ("Master Data",           "/api/masterdata/countries",        "Master data service",                 _master_data,        4),
    ("Workers",               "/api/workers",                     "Worker/background-job service",       _workers,            2),
    ("Duplicate Detection",   "/api/duplicates/accounts",         "Duplicate detection service",         _duplicates,         2),
    ("Normalization",         "/api/normalization/email",         "Normalization service",               _normalization,      2),
    ("Email Integration",     "/api/email/status",                "Email integration service",           _email_integration,  1),
    ("ITSM Chatbot",          "/api/itsm/chatbot/status",         "ITSM chatbot service",                _itsm_chatbot,       1),
    ("ITSM Email",            "/api/itsm/email/settings",         "ITSM email-to-ticket integration",   _itsm_email,         1),
    ("News/Social Feed",      "/api/news-social/feed",            "News/Social feed service",            _news_social,        1),
    ("Test Results",          "/api/testresults",                  "Test results service",                _test_results,       1),
    ("Auth Diagnostics",      "/api/auth-diagnostics/known-issues", "Auth diagnostics service",          _auth_diagnostics,   1),
    ("User Profiles",         "/api/userprofiles/1",              "User profiles service",               _user_profiles,      1),
    ("Preferences",           "/api/preferences/account/1",       "Preferences service",                 _preferences,        2),
    ("Workflow Instances",    "/api/workflow-instances/definition/1", "Workflow instances service",       _workflow_instances,  1),
    ("Workflow Tasks/User",   "/api/workflows/tasks/user/1",      "Workflow tasks-by-user service",      _workflow_tasks_user, 1),
]


def run(api: ApiClient, log: RunLogger) -> None:
    """Run all integration-dependent endpoint groups.

    For each group we probe one endpoint first; if the service returns 404
    we skip the whole group and record it as integration-skipped.
    """
    log.section("BATCH 13: Integration-Dependent Endpoints")

    available: list[str] = []
    unavailable: list[str] = []
    total_skipped = 0

    for group_name, probe_ep, description, handler, ep_count in SERVICE_GROUPS:
        svc_ok = check_service_availability(api, probe_ep)

        if svc_ok:
            available.append(group_name)
            try:
                handler(api, log)
            except Exception as exc:
                log.log(f"  ERROR running {group_name}: {exc}")
        else:
            unavailable.append(group_name)
            skip_msg = f"\u26a0 Skipping {group_name} endpoints \u2014 service not configured ({description})"
            print(f"  {skip_msg}")
            log.log(skip_msg)
            # Log each endpoint in the group as skipped
            for _ in range(ep_count):
                log.log_integration_skip(probe_ep, group_name)
                api.stats["skipped_integration"] += 1
            total_skipped += ep_count

    # ── Group summary ──
    log.section("Integration Services Summary")
    if available:
        log.log(f"  Available   ({len(available)}): {', '.join(available)}")
    if unavailable:
        log.log(f"  Unavailable ({len(unavailable)}): {', '.join(unavailable)}")
    log.log(f"  Endpoints skipped (svc not configured): {total_skipped}")

    # Store availability info in state for the coordinator summary
    save_ids("_integration_available", [])   # placeholder — names stored in log
    save_ids("_integration_unavailable", [])

    print(f"  Batch 13 done: {log.summary_line()}  integration_skipped={total_skipped}")

#!/usr/bin/env python3
"""Batch 11: Infrastructure & Config.

Covers: CloudDeployment, CICD, Monitoring, Performance, CalendarIntegration,
EmailIntegration, UIPreferences, UserProfiles, MasterData, FieldMasterData,
ModuleFieldConfigurations, Duplicates, Normalization, Database, ZipCodes.
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 11: Infrastructure & Config")
    ts = int(time.time())
    user_ids = load_ids("users")

    # ---- Cloud Deployment ----
    log.section("CloudDeployment")
    api.get("/api/clouddeployment/providers")
    api.get("/api/clouddeployment/deployments")

    # ---- CICD Integration ----
    log.section("CICDIntegration")
    api.get("/api/itsm/cicd/deployments")
    api.get("/api/itsm/cicd/pipelines")

    # ---- Monitoring ----
    log.section("Monitoring")
    api.get("/api/monitoring/status")
    api.get("/api/monitoring/metrics")
    api.get("/api/monitoring/alerts")

    # ---- Performance Monitoring ----
    log.section("PerformanceMonitoring")
    api.get("/api/performance/metrics")
    api.get("/api/performance/slow-queries")

    # ---- Calendar Integration ----
    log.section("CalendarIntegration")
    api.get("/api/calendar/integrations")

    # ---- Email Integration ----
    log.section("EmailIntegration")
    api.get("/api/email/status")

    # ---- UI Preferences ----
    log.section("UIPreferences CRUD")
    pref = {"theme": "dark", "language": "en", "density": "comfortable",
            "sidebarCollapsed": False, "dashboardLayout": "default"}
    api.post("/api/ui-preferences", pref)
    api.get("/api/ui-preferences")
    # No PUT exists on this controller; use POST to save/update
    api.post("/api/ui-preferences", {**pref, "theme": "light"})

    # ---- User Profiles ----
    log.section("UserProfiles")
    api.get("/api/userprofiles")
    if user_ids:
        api.get(f"/api/userprofiles/{user_ids[0]}")

    # ---- Master Data ----
    log.section("MasterData")
    api.get("/api/masterdata/countries")
    api.get("/api/masterdata/currencies")
    api.get("/api/masterdata/timezones")
    api.get("/api/masterdata/industries")

    # ---- Field Master Data ----
    log.section("FieldMasterData")
    # No base GET exists. Available: GET /field/{fieldConfigurationId}, GET /module/{moduleName}, GET /{id}
    api.get("/api/fieldmasterdata/module/Accounts")

    # ---- Module Field Configurations ----
    log.section("ModuleFieldConfigurations")
    # No base GET exists. Available: GET /{moduleName}, GET /config/{id}
    api.get("/api/modulefieldconfigurations/Accounts")

    # ---- Duplicates ----
    log.section("Duplicates")
    api.get("/api/duplicates/accounts")
    api.get("/api/duplicates/contacts")

    # ---- Normalization ----
    log.section("Normalization")
    api.post("/api/normalization/phone", {"value": "+1 (555) 123-4567"})
    api.post("/api/normalization/email", {"value": "  Test@Example.COM  "})

    # ---- Database ----
    log.section("Database Admin")
    api.get("/api/database/status")

    # ---- ZipCodes ----
    log.section("ZipCodes")
    api.get("/api/zipcodes/lookup/94105")

    # ---- Catalog Categories ----
    log.section("CatalogCategories CRUD")
    cat = {"name": f"Hardware Assets {ts}", "description": "Physical hardware items"}
    eid = api.create_and_track("catalogcategories", "/api/catalog-categories", cat)
    if eid:
        api.get(f"/api/catalog-categories/{eid}")
        save_ids("catalogcategories", [eid])
    api.get("/api/catalog-categories")

    # ---- Admin Settings: Approval Requests ----
    log.section("AdminSettings - ApprovalRequests")
    api.get("/api/adminsettings/approval-requests")

    # ---- Admin Settings: Groups ----
    log.section("AdminSettings - Groups")
    api.get("/api/adminsettings/groups")
    asg = {"name": f"Admin Group {ts}", "description": "Test admin group"}
    eid = api.create_and_track("adminsettingsgroups", "/api/adminsettings/groups", asg)
    if eid:
        api.get(f"/api/adminsettings/groups/{eid}")
        api.put(f"/api/adminsettings/groups/{eid}", {**asg, "description": "Updated admin group"})
        if user_ids:
            api.post(f"/api/adminsettings/groups/{eid}/members/{user_ids[0]}")
            api.get(f"/api/adminsettings/groups/{eid}/members")
        save_ids("adminsettingsgroups", [eid])

    # ---- Admin Settings: Database Backup ----
    log.section("Database Backup")
    api.get("/api/adminsettings/database/backups")

    # ---- News/Social ----
    log.section("NewsSocial")
    api.get("/api/news-social/feed")

    # ---- Worker Health ----
    log.section("WorkerHealth")
    api.get("/api/workers")

    # ---- Worker Control ----
    log.section("WorkerControl")
    api.get("/api/workers/control/status")

    # ---- Provider Health ----
    log.section("ProviderHealth")
    api.get("/api/health/providers")

    # ---- Analytics ----
    log.section("Analytics")
    api.get("/api/analytics/dashboards")
    api.get("/api/analytics/charts")

    # ---- Sample Data ----
    log.section("SampleData")
    api.get("/api/sampledata/status")

    # ---- Test Results ----
    log.section("TestResults")
    api.get("/api/testresults")

    # ---- Auth Diagnostics ----
    log.section("AuthDiagnostics")
    api.get("/api/auth-diagnostics/known-issues")

    print(f"  Batch 11 done: {log.summary_line()}")

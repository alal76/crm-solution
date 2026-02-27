#!/usr/bin/env python3
"""Batch 01: System & Admin entities.

Covers: Users, Roles, Permissions, UserGroups, Departments, SystemSettings,
FeatureFlags, Branding, ColorPalettes, Lookups, ModuleUIConfig.
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 01: System & Admin")

    # ---- Roles ----
    log.section("Roles CRUD")
    roles = [
        {"name": f"TestRole-{int(time.time())}-1", "description": "Sales Manager role"},
        {"name": f"TestRole-{int(time.time())}-2", "description": "Support Agent role"},
    ]
    role_ids = []
    for r in roles:
        eid = api.create_and_track("roles", "/api/roles", r)
        if eid:
            role_ids.append(eid)
    api.get("/api/roles")
    if role_ids:
        api.get(f"/api/roles/{role_ids[0]}")
        api.put(f"/api/roles/{role_ids[0]}", {"name": roles[0]["name"], "description": "Updated Sales Manager"})
    save_ids("roles", role_ids)

    # ---- Permissions ----
    log.section("Permissions CRUD")
    api.get("/api/permissions")

    # ---- Users ----
    log.section("Users CRUD")
    ts = int(time.time())
    test_password = os.environ.get('TEST_PASSWORD', 'Test@12345')  # NOSONAR
    users = [
        {"email": f"test1_{ts}@crm.local", "password": test_password,
         "firstName": "Test", "lastName": "User1", "roleId": 2},
        {"email": f"test2_{ts}@crm.local", "password": test_password,
         "firstName": "Test", "lastName": "User2", "roleId": 2},
        {"email": f"test3_{ts}@crm.local", "password": test_password,
         "firstName": "Test", "lastName": "Manager1", "roleId": 1},
    ]
    user_ids = []
    for u in users:
        code, body, _ = api.post("/api/users", u)
        if body and isinstance(body, dict) and body.get("id"):
            user_ids.append(body["id"])
            log.track_id("users", body["id"])
    api.get("/api/users")
    if user_ids:
        api.get(f"/api/users/{user_ids[0]}")
    save_ids("users", user_ids)

    # ---- User Groups ----
    log.section("UserGroups CRUD")
    groups = [
        {"name": f"TestGroup-{ts}-Sales", "description": "Sales team group"},
        {"name": f"TestGroup-{ts}-Support", "description": "Support team group"},
    ]
    group_ids = []
    for g in groups:
        eid = api.create_and_track("usergroups", "/api/usergroups", g)
        if eid:
            group_ids.append(eid)
    api.get("/api/usergroups")
    # Link: add user to group
    if group_ids and user_ids:
        api.post(f"/api/usergroups/{group_ids[0]}/members/{user_ids[0]}", None)
        api.get(f"/api/usergroups/{group_ids[0]}/members")
        # Unlink
        api.delete(f"/api/usergroups/{group_ids[0]}/members/{user_ids[0]}")
    save_ids("usergroups", group_ids)

    # ---- Departments ----
    log.section("Departments CRUD")
    depts = [
        {"name": f"TestDept-{ts}-Engineering", "description": "Engineering department", "isActive": True},
        {"name": f"TestDept-{ts}-Sales", "description": "Sales department", "isActive": True},
    ]
    dept_ids = []
    for d in depts:
        eid = api.create_and_track("departments", "/api/departments", d)
        if eid:
            dept_ids.append(eid)
    api.get("/api/departments")
    if dept_ids:
        api.get(f"/api/departments/{dept_ids[0]}")
        api.put(f"/api/departments/{dept_ids[0]}", {**depts[0], "description": "Updated Engineering"})
    save_ids("departments", dept_ids)

    # ---- System Settings ----
    log.section("SystemSettings")
    api.get("/api/systemsettings")

    # ---- Feature Flags ----
    log.section("FeatureFlags")
    api.get("/api/admin/features")
    api.get("/api/feature-flags")

    # ---- Branding ----
    log.section("Branding")
    api.get("/api/branding")

    # ---- Color Palettes ----
    log.section("ColorPalettes")
    api.get("/api/colorpalettes")
    api.get("/api/colorpalettes/categories")

    # ---- Lookups ----
    log.section("Lookups")
    api.get("/api/lookups/categories")

    # ---- Module UI Config ----
    log.section("ModuleUIConfig")
    api.get("/api/moduleuiconfig")

    # ---- Navigation ----
    log.section("Navigation")
    api.get("/api/navigation/config")

    # ---- Admin Dashboard ----
    log.section("AdminDashboard")
    api.get("/api/admin/dashboard")
    api.get("/api/admin/statistics")
    api.get("/api/admin/modules/status")
    api.get("/api/admin/health")

    # ---- Health ----
    log.section("Health Endpoints")
    api.get("/health")  # Note: health endpoints do not need auth (no /api prefix)

    print(f"  Batch 01 done: {log.summary_line()}")

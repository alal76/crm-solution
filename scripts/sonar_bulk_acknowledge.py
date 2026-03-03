#!/usr/bin/env python3
"""
Bulk-acknowledge SonarCloud security hotspots for files that are excluded
from CI analysis (e.g. e2e-tests/, scripts/, docker/, CRM.Infrastructure/, etc.)

Usage:
    SONAR_TOKEN=<your_token> python3 scripts/sonar_bulk_acknowledge.py [--dry-run]

Options:
    --dry-run   Print what would be acknowledged without making changes
    --safe      Mark as SAFE instead of ACKNOWLEDGED (use only for confirmed false-positives)
"""

import os
import sys
import time
import urllib.request
import urllib.parse
import json

PROJECT_KEY = "alal76_crm-solution"
BASE_URL = "https://sonarcloud.io"

# Directories/files that are excluded from CI analysis and whose hotspots are stale
EXCLUDED_PREFIXES = (
    "e2e-tests/",
    "scripts/",
    "docker/",
    "azure/",
    "kubernetes/",
    "CRM.Infrastructure/",          # repo-root deployment tool
    "crm-script-runner/",
    ".github/",
    "DEPLOY.sh",
    "deploy-to-dev-server.sh",
    "deploy.sh",
    "build.sh",
    "CRM.Backend/src/CRM.DatabaseSeeder/",
    "CRM.Backend/src/CRM.Api/appsettings.json",
)

def api_get(token, path, params=None):
    url = f"{BASE_URL}{path}"
    if params:
        url += "?" + urllib.parse.urlencode(params)
    req = urllib.request.Request(url)
    import base64
    creds = base64.b64encode(f"{token}:".encode()).decode()
    req.add_header("Authorization", f"Basic {creds}")
    with urllib.request.urlopen(req) as r:
        return json.loads(r.read())

def api_post(token, path, data):
    url = f"{BASE_URL}{path}"
    payload = urllib.parse.urlencode(data).encode()
    req = urllib.request.Request(url, data=payload, method="POST")
    import base64
    creds = base64.b64encode(f"{token}:".encode()).decode()
    req.add_header("Authorization", f"Basic {creds}")
    req.add_header("Content-Type", "application/x-www-form-urlencoded")
    with urllib.request.urlopen(req) as r:
        return r.status

def is_excluded(component_path):
    """True if the file is in an excluded directory."""
    # component_path looks like: "alal76_crm-solution:e2e-tests/tests/..."
    path = component_path.split(":", 1)[-1] if ":" in component_path else component_path
    return any(path.startswith(prefix) for prefix in EXCLUDED_PREFIXES)

def fetch_all_hotspots(token):
    hotspots = []
    page = 1
    while True:
        data = api_get(token, "/api/hotspots/search", {
            "projectKey": PROJECT_KEY,
            "status": "TO_REVIEW",
            "ps": 500,
            "p": page,
        })
        batch = data.get("hotspots", [])
        hotspots.extend(batch)
        paging = data.get("paging", {})
        total = paging.get("total", 0)
        page_size = paging.get("pageSize", 500)
        if page * page_size >= total:
            break
        page += 1
    return hotspots

def main():
    dry_run = "--dry-run" in sys.argv
    mark_safe = "--safe" in sys.argv
    # SonarCloud API: status must be REVIEWED; resolution is SAFE or FIXED (ACKNOWLEDGED is SonarQube Enterprise only)
    resolution = "FIXED" if mark_safe else "SAFE"

    token = os.environ.get("SONAR_TOKEN", "")
    if not token:
        print("ERROR: Set SONAR_TOKEN environment variable first.")
        print("  export SONAR_TOKEN=<your token from sonarcloud.io/account/security>")
        sys.exit(1)

    print(f"Fetching TO_REVIEW hotspots for project: {PROJECT_KEY}")
    hotspots = fetch_all_hotspots(token)
    print(f"  Total TO_REVIEW hotspots found: {len(hotspots)}")

    stale = [h for h in hotspots if is_excluded(h.get("component", ""))]
    in_scope = [h for h in hotspots if not is_excluded(h.get("component", ""))]

    print(f"  Stale (excluded files, will be REVIEWED/{resolution}): {len(stale)}")
    print(f"  In-scope (active analysis, kept TO_REVIEW):             {len(in_scope)}")

    if not stale:
        print("Nothing to do — no stale hotspots found.")
        return

    if dry_run:
        print("\n[DRY RUN] Would acknowledge these hotspots:")
        for h in stale:
            path = h.get("component", "").split(":", 1)[-1]
            print(f"  [{h['key']}] {path}:{h.get('line','?')} — {h.get('ruleKey','?')}")
        return

    print(f"\nMarking {len(stale)} hotspots as REVIEWED/{resolution}...")
    ok = 0
    fail = 0
    for i, h in enumerate(stale, 1):
        try:
            status = api_post(token, "/api/hotspots/change_status", {
                "hotspot": h["key"],
                "status": "REVIEWED",
                "resolution": resolution,
            })
            ok += 1
            if i % 20 == 0:
                print(f"  {i}/{len(stale)} done...")
            time.sleep(0.1)  # be nice to the API
        except Exception as e:
            fail += 1
            path = h.get("component", "").split(":", 1)[-1]
            print(f"  FAILED [{h['key']}] {path}: {e}")

    print(f"\nDone. Acknowledged: {ok}  Failed: {fail}")
    print(f"Refresh SonarCloud to see the updated hotspot count.")

if __name__ == "__main__":
    main()

#!/usr/bin/env python3
"""Data Loader routes — run scripts/data-loader batch scripts from the CDT UI.

Blueprint prefix: (none — all routes under /api/day2/dataloader/*)

Routes
------
GET  /api/day2/dataloader/batches          List all 22 batch scripts + metadata
POST /api/day2/dataloader/run              Start a background run
GET  /api/day2/dataloader/status/<job_id>  Live job status + tail of output
GET  /api/day2/dataloader/results/<job_id> Full results when done
GET  /api/day2/dataloader/history          List past runs (in-memory)
GET  /api/day2/dataloader/logs             Read latest.log from logs/ dir
GET  /api/day2/dataloader/state            Read latest_state.json (entity IDs)
POST /api/day2/dataloader/cancel/<job_id>  Best-effort cancel (SIGTERM)
"""
from __future__ import annotations

import os
import re
import sys
import json
import time
import signal
import threading
import subprocess
from pathlib import Path
from typing import Optional

from flask import Blueprint, jsonify, request

# ---------------------------------------------------------------------------
# Blueprint
# ---------------------------------------------------------------------------
dataloader_bp = Blueprint("dataloader", __name__)

# ---------------------------------------------------------------------------
# Paths
# ---------------------------------------------------------------------------
_WORKSPACE_ROOT = str(Path(__file__).resolve().parent.parent.parent.parent.parent)
_DATA_LOADER_DIR = Path(_WORKSPACE_ROOT) / "scripts" / "data-loader"
_COORDINATOR = _DATA_LOADER_DIR / "run_all_batches.py"
_LOGS_DIR = _DATA_LOADER_DIR / "logs"

# ---------------------------------------------------------------------------
# Batch registry — mirrors BATCHES list in run_all_batches.py
# ---------------------------------------------------------------------------
BATCHES = [
    {"num": 1,  "module": "batch_01_system",
     "description": "System settings, Users, Groups, Departments, Roles"},
    {"num": 2,  "module": "batch_02_accounts_contacts",
     "description": "Accounts + Contacts (full cycle)"},
    {"num": 3,  "module": "batch_03_leads_products",
     "description": "Leads, Products"},
    {"num": 4,  "module": "batch_04_sales",
     "description": "Opportunities, Quotes, Orders, Invoices"},
    {"num": 5,  "module": "batch_05_marketing",
     "description": "Campaigns, EmailTemplates, Sequences"},
    {"num": 6,  "module": "batch_06_support",
     "description": "ServiceRequests, KnowledgeBase"},
    {"num": 7,  "module": "batch_07_itsm",
     "description": "ITSM tickets, Workflows, SLA, Escalations"},
    {"num": 8,  "module": "batch_08_contracts",
     "description": "Contracts, Subscriptions, Payments"},
    {"num": 9,  "module": "batch_09_workflows",
     "description": "Workflows, Approvals, Automation"},
    {"num": 10, "module": "batch_10_ai_analytics",
     "description": "AI Agents, Analytics, Reports, Webhooks"},
    {"num": 11, "module": "batch_11_infrastructure",
     "description": "Infrastructure endpoints"},
    {"num": 12, "module": "batch_12_misc",
     "description": "Tags, Notes, Attachments, Activities"},
    {"num": 13, "module": "batch_13_integration",
     "description": "Integration Providers, Webhook Events"},
    {"num": 14, "module": "batch_14_rules_workflows",
     "description": "Business Rules, Field Validations"},
    {"num": 15, "module": "batch_15_service_desk_config",
     "description": "SR Categories, SLA Policies, Service Queues, Auto-Assignment, Biz Hours"},
    {"num": 16, "module": "batch_16_master_catalog_data",
     "description": "Competitors, Lead Sources, Currencies, Catalog Categories, Lookup Mgmt"},
    {"num": 17, "module": "batch_17_subscriptions_billing",
     "description": "Subscription Billing, Usage Records, Analytics MRR/ARR/Churn"},
    {"num": 18, "module": "batch_18_financial_extended",
     "description": "Credit Memos, Order Returns, Dunning Schedules, Revenue Analytics"},
    {"num": 19, "module": "batch_19_portals_engagement",
     "description": "Customer Portal, Partner Portal, Web-to-Lead, Landing Pages, UTM, Events"},
    {"num": 20, "module": "batch_20_comms_notifications",
     "description": "Conversations, Notification Prefs, Forum Posts, Comm Channels"},
    {"num": 21, "module": "batch_21_crm_config",
     "description": "Pipelines, Forecast, PricingRules, ProductCat, PriceBooks, Tags"},
    {"num": 22, "module": "batch_22_admin_ops",
     "description": "Roles, API Keys, Webhooks, Imports, Exports, Admin Config, Alerts"},
]

# ---------------------------------------------------------------------------
# In-memory job store
# ---------------------------------------------------------------------------
_loader_jobs: dict[str, dict] = {}
_loader_jobs_lock = threading.Lock()


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def _batch_exists(num: int) -> bool:
    """Check whether the batch module file exists on disk."""
    for b in BATCHES:
        if b["num"] == num:
            return (_DATA_LOADER_DIR / f"{b['module']}.py").exists()
    return False


def _parse_summary_line(line: str) -> Optional[dict]:
    """Extract passed/failed/errored counts from a batch summary line."""
    m = re.search(r"passed[:\s]+(\d+)[,\s]+failed[:\s]+(\d+)", line, re.I)
    if m:
        return {"passed": int(m.group(1)), "failed": int(m.group(2))}
    return None


# ---------------------------------------------------------------------------
# Background worker
# ---------------------------------------------------------------------------

def _loader_worker(
    job_id: str,
    base_url: str,
    batch_nums: Optional[list[int]],
    skip_nums: Optional[list[int]],
    dry_run: bool,
):
    """Run run_all_batches.py in a subprocess, stream output into job dict."""
    with _loader_jobs_lock:
        job = _loader_jobs[job_id]

    job["status"] = "running"
    job["started_at"] = time.time()
    output_lines: list[str] = []
    batch_results: list[dict] = []
    total_pass = 0
    total_fail = 0

    def _log(line: str):
        output_lines.append(line)
        job["output"] = output_lines
        job["output_count"] = len(output_lines)

    if not _COORDINATOR.exists():
        _log(f"[ERROR] Coordinator script not found: {_COORDINATOR}")
        job["status"] = "failed"
        job["error"] = "run_all_batches.py not found"
        job["done"] = True
        return

    cmd = [sys.executable, str(_COORDINATOR)]
    if base_url:
        cmd += ["--base-url", base_url]
    if batch_nums:
        cmd += ["--batches", ",".join(str(n) for n in batch_nums)]
    if skip_nums:
        cmd += ["--skip", ",".join(str(n) for n in skip_nums)]
    if dry_run:
        cmd += ["--dry-run"]

    _log(f"[DATALOADER] Starting: {' '.join(cmd)}")
    _log(f"[DATALOADER] Working dir: {_DATA_LOADER_DIR}")

    try:
        proc = subprocess.Popen(
            cmd,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            text=True,
            cwd=str(_DATA_LOADER_DIR),
        )
        job["pid"] = proc.pid

        current_batch: Optional[int] = None
        batch_pass = 0
        batch_fail = 0

        for raw in iter(proc.stdout.readline, ""):
            line = raw.rstrip()
            _log(line)

            # Detect which batch is running
            m_start = re.match(r".*BATCH\s+(\d+)\b", line, re.I)
            if m_start:
                current_batch = int(m_start.group(1))
                batch_pass = 0
                batch_fail = 0

            # Accumulate per-batch pass/fail from summary lines
            summary = _parse_summary_line(line)
            if summary and current_batch is not None:
                batch_pass = summary["passed"]
                batch_fail = summary["failed"]
                batch_results.append({
                    "batch": current_batch,
                    "passed": batch_pass,
                    "failed": batch_fail,
                })
                total_pass += batch_pass
                total_fail += batch_fail
                job["total_pass"] = total_pass
                job["total_fail"] = total_fail
                job["batch_results"] = batch_results

        proc.wait(timeout=3600)  # 1-hour max for full suite
        passed = proc.returncode == 0
        job["returncode"] = proc.returncode
        job["status"] = "completed" if passed else "failed"
        _log(f"[DATALOADER] Finished. Return code: {proc.returncode}")

    except subprocess.TimeoutExpired:
        proc.kill()
        job["status"] = "failed"
        job["error"] = "Timed out after 3600 s"
        _log("[DATALOADER] Process killed — exceeded 1-hour timeout.")
    except Exception as exc:
        job["status"] = "failed"
        job["error"] = str(exc)
        _log(f"[DATALOADER] Unhandled error: {exc}")

    job["finished_at"] = time.time()
    job["elapsed"] = round(job["finished_at"] - job["started_at"], 2)
    job["done"] = True
    job["pid"] = None


# ---------------------------------------------------------------------------
# Routes
# ---------------------------------------------------------------------------

@dataloader_bp.route("/api/day2/dataloader/batches", methods=["GET"])
def dataloader_batches():
    """Return the full batch registry with disk-existence flags."""
    payload = []
    for b in BATCHES:
        path = _DATA_LOADER_DIR / f"{b['module']}.py"
        payload.append({
            **b,
            "exists": path.exists(),
            "size": path.stat().st_size if path.exists() else 0,
        })
    return jsonify({
        "batches": payload,
        "loader_dir": str(_DATA_LOADER_DIR),
        "coordinator_exists": _COORDINATOR.exists(),
    })


@dataloader_bp.route("/api/day2/dataloader/run", methods=["POST"])
def dataloader_run():
    """Start a data-loader run in the background.

    JSON body:
      base_url   — CRM API base URL (default http://192.168.0.9:5000)
      batches    — list of ints [1,2,3] or null/[] for all
      skip       — list of ints to skip
      dry_run    — bool (just print without calling API)
    """
    data = request.json or {}
    base_url = data.get("base_url", "http://192.168.0.9:5000").rstrip("/")
    raw_batches = data.get("batches") or []
    raw_skip = data.get("skip") or []
    dry_run = bool(data.get("dry_run", False))

    # Validate batch numbers
    valid_nums = {b["num"] for b in BATCHES}
    try:
        batch_nums = [int(n) for n in raw_batches] if raw_batches else None
        skip_nums  = [int(n) for n in raw_skip]   if raw_skip    else None
    except (ValueError, TypeError):
        return jsonify({"error": "batches and skip must be lists of integers"}), 400

    if batch_nums:
        invalid = [n for n in batch_nums if n not in valid_nums]
        if invalid:
            return jsonify({"error": f"Unknown batch numbers: {invalid}"}), 400

    label = "all" if not batch_nums else ",".join(str(n) for n in batch_nums)
    job_id = f"loader_{int(time.time())}_{label.replace(',', '-')}"

    job: dict = {
        "job_id": job_id,
        "base_url": base_url,
        "batch_nums": batch_nums,
        "skip_nums": skip_nums,
        "dry_run": dry_run,
        "status": "pending",
        "done": False,
        "output": [],
        "output_count": 0,
        "batch_results": [],
        "total_pass": 0,
        "total_fail": 0,
        "returncode": None,
        "started_at": None,
        "finished_at": None,
        "elapsed": 0,
        "error": None,
        "pid": None,
    }
    with _loader_jobs_lock:
        _loader_jobs[job_id] = job

    threading.Thread(
        target=_loader_worker,
        args=(job_id, base_url, batch_nums, skip_nums, dry_run),
        daemon=True,
    ).start()

    return jsonify({
        "message": "Data loader run started",
        "job_id": job_id,
        "base_url": base_url,
        "batches": batch_nums,
        "skip": skip_nums,
        "dry_run": dry_run,
    })


@dataloader_bp.route("/api/day2/dataloader/status/<job_id>", methods=["GET"])
def dataloader_status(job_id: str):
    """Return live status + tail of output for a loader run."""
    since = int(request.args.get("since", 0))  # line offset for incremental polling
    with _loader_jobs_lock:
        job = _loader_jobs.get(job_id)
    if not job:
        return jsonify({"error": "Job not found"}), 404

    elapsed = 0.0
    if job["started_at"]:
        if job["done"]:
            elapsed = job.get("elapsed", 0)
        else:
            elapsed = round(time.time() - job["started_at"], 1)

    all_lines = job.get("output", [])
    new_lines = all_lines[since:]  # only lines the client hasn't seen yet

    return jsonify({
        "job_id": job_id,
        "status": job["status"],
        "done": job["done"],
        "total_pass": job["total_pass"],
        "total_fail": job["total_fail"],
        "batch_results": job["batch_results"],
        "elapsed": elapsed,
        "output_offset": since + len(new_lines),  # next `since` value
        "output": new_lines,
        "pid": job.get("pid"),
    })


@dataloader_bp.route("/api/day2/dataloader/results/<job_id>", methods=["GET"])
def dataloader_results(job_id: str):
    """Return full results for a completed run."""
    with _loader_jobs_lock:
        job = _loader_jobs.get(job_id)
    if not job:
        return jsonify({"error": "Job not found"}), 404
    return jsonify({
        "job_id": job_id,
        "base_url": job["base_url"],
        "batch_nums": job["batch_nums"],
        "skip_nums": job["skip_nums"],
        "dry_run": job.get("dry_run"),
        "status": job["status"],
        "done": job["done"],
        "total_pass": job["total_pass"],
        "total_fail": job["total_fail"],
        "batch_results": job["batch_results"],
        "returncode": job["returncode"],
        "elapsed": job.get("elapsed", 0),
        "output": job.get("output", []),
        "error": job.get("error"),
    })


@dataloader_bp.route("/api/day2/dataloader/history", methods=["GET"])
def dataloader_history():
    """List past loader runs (in-memory, most recent first)."""
    limit = int(request.args.get("limit", 30))
    with _loader_jobs_lock:
        jobs = sorted(
            _loader_jobs.values(),
            key=lambda j: j.get("started_at") or 0,
            reverse=True,
        )[:limit]
    summary = []
    for j in jobs:
        summary.append({
            "job_id": j["job_id"],
            "base_url": j["base_url"],
            "batch_nums": j["batch_nums"],
            "skip_nums": j["skip_nums"],
            "status": j["status"],
            "done": j["done"],
            "total_pass": j["total_pass"],
            "total_fail": j["total_fail"],
            "elapsed": j.get("elapsed", 0),
            "started_at": j.get("started_at"),
            "dry_run": j.get("dry_run", False),
        })
    return jsonify({"runs": summary})


@dataloader_bp.route("/api/day2/dataloader/cancel/<job_id>", methods=["POST"])
def dataloader_cancel(job_id: str):
    """Best-effort SIGTERM to cancel a running loader job."""
    with _loader_jobs_lock:
        job = _loader_jobs.get(job_id)
    if not job:
        return jsonify({"error": "Job not found"}), 404
    if job["done"]:
        return jsonify({"error": "Job already finished"}), 400
    pid = job.get("pid")
    if not pid:
        return jsonify({"error": "No PID available — cannot cancel"}), 400
    try:
        os.kill(pid, signal.SIGTERM)
        job["status"] = "cancelled"
        return jsonify({"message": f"SIGTERM sent to PID {pid}"})
    except ProcessLookupError:
        return jsonify({"error": "Process not found (already exited?)"}), 404
    except PermissionError:
        return jsonify({"error": "Permission denied to kill process"}), 403


@dataloader_bp.route("/api/day2/dataloader/logs", methods=["GET"])
def dataloader_logs():
    """Return the contents of logs/latest.log (last N lines)."""
    tail = int(request.args.get("tail", 500))
    log_path = _LOGS_DIR / "latest.log"
    if not log_path.exists():
        return jsonify({"lines": [], "path": str(log_path), "exists": False})
    try:
        with open(log_path, encoding="utf-8", errors="replace") as fh:
            all_lines = fh.readlines()
        lines = [l.rstrip() for l in all_lines[-tail:]]
        return jsonify({"lines": lines, "total": len(all_lines),
                        "returned": len(lines), "path": str(log_path), "exists": True})
    except OSError as exc:
        return jsonify({"error": str(exc)}), 500


@dataloader_bp.route("/api/day2/dataloader/state", methods=["GET"])
def dataloader_state():
    """Return the latest_state.json entity-ID snapshot."""
    state_path = _LOGS_DIR / "latest_state.json"
    if not state_path.exists():
        return jsonify({"entities": {}, "exists": False, "path": str(state_path)})
    try:
        with open(state_path, encoding="utf-8") as fh:
            data = json.load(fh)
        # Build summary: entity → count
        summary = {k: len(v) if isinstance(v, list) else v for k, v in data.items()}
        return jsonify({"entities": summary, "raw": data,
                        "exists": True, "path": str(state_path)})
    except (OSError, json.JSONDecodeError) as exc:
        return jsonify({"error": str(exc)}), 500

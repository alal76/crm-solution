#!/usr/bin/env python3
"""
CRM Test Data Loader — Coordinator
===================================
Authenticates once, then calls each batch module in order.
All batches share one ApiClient and one RunLogger so results
are appended to a single log file.

Usage:
  python run_all_batches.py [--base-url URL] [--batches N,N,...] [--skip N,N,...]
                            [--username U] [--password P]

Examples:
  python run_all_batches.py                              # all batches against 192.168.0.9
  python run_all_batches.py --base-url http://localhost:5000
  python run_all_batches.py --batches 1,2,3              # only batches 1-3
  python run_all_batches.py --skip 8,10                  # skip batches 8 and 10
"""
from __future__ import annotations

import argparse
import importlib
import os
import sys
import time
import traceback

# Ensure the data-loader directory is on the import path
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from loader_utils import (
    ApiClient, RunLogger, DockerLogCapture,
    init_state, authenticate, check_service_availability,
)


# ── Batch registry (order matters — later batches depend on entities from earlier batches)
BATCHES = [
    (1,  "batch_01_system",              "System, Users, Roles, Settings"),
    (2,  "batch_02_crm_core",            "Accounts, Contacts, Contact Info"),
    (3,  "batch_03_leads_products",      "Leads, Products, PriceBooks"),
    (4,  "batch_04_sales",               "Opportunities, Quotes, Orders, Invoices"),
    (5,  "batch_05_activities",          "Interactions, Tasks, Notes"),
    (6,  "batch_06_marketing",           "Campaigns, Templates, Sequences"),
    (7,  "batch_07_itsm",                "Service Desk, Incidents, Changes, KB"),
    (8,  "batch_08_commissions",         "Commissions, Territories, Teams"),
    (9,  "batch_09_workflows",           "Workflows, Approvals, Triggers, Rule Engine"),
    (10, "batch_10_ai_analytics",        "AI Agents, SK Plugins, Analytics, Reports"),
    (11, "batch_11_infrastructure",      "Monitoring, Config, MasterData"),
    (12, "batch_12_misc",                "Files, Tags, CustomFields, Misc"),
    (13, "batch_13_integration",         "Integration-Dependent Endpoints (probe & skip)"),
    (14, "batch_14_rules_workflows",     "Rules, Rulesets & Full Workflow E2E"),
    # ── New extended batches ──────────────────────────────────────────────────────
    (15, "batch_15_service_desk_config", "SR Categories, Types, SLAs, Queues, AutoAssign, Escalation"),
    (16, "batch_16_master_catalog_data", "Lead Sources, Currencies, Competitors, Master Data, Lookups"),
    (17, "batch_17_subscriptions_billing","Subscription Lifecycle, Billing, Usage, Revenue, Dunning"),
    (18, "batch_18_financial_extended",  "Credit Memos, Order Returns, Pricing Rules, Tax, Payment Methods"),
    (19, "batch_19_portals_engagement",  "Portal, Partner Portal, Web-to-Lead, Landing Pages, Events, Segments"),
    (20, "batch_20_comms_notifications", "Conversations, Communications, Comments, Notifications, GDPR"),
    (21, "batch_21_crm_config",          "Pipelines, Forecast, Quote Templates, ProductCat, PriceBooks, Tags"),
    (22, "batch_22_admin_ops",           "Roles, API Keys, Webhooks, Imports, Exports, Admin Config, Alerts"),
    (23, "batch_23_fortune100",          "Fortune 100 Companies — Accounts & Linked Contacts"),
]


def parse_int_list(s: str) -> list[int]:
    """Parse a comma-separated list of integers."""
    return [int(x.strip()) for x in s.split(",") if x.strip()]


def main() -> None:
    parser = argparse.ArgumentParser(description="CRM Test Data Loader — Master Coordinator")
    parser.add_argument("--base-url", default="http://192.168.0.9:5000",
                        help="CRM API base URL (default: http://192.168.0.9:5000)")
    parser.add_argument("--username", default="admin@crm.local",
                        help="Admin username (default: admin@crm.local)")
    parser.add_argument("--password", default="Admin@123",
                        help="Admin password (default: Admin@123)")
    parser.add_argument("--batches", default=None,
                        help="Comma-separated list of batch numbers to run (e.g., 1,2,3)")
    parser.add_argument("--skip", default=None,
                        help="Comma-separated list of batch numbers to skip (e.g., 8,10)")
    parser.add_argument("--continue-on-error", action="store_true", default=True,
                        help="Continue to next batch even if the current one fails (default: True)")
    parser.add_argument("--stop-on-error", action="store_true", default=False,
                        help="Stop immediately if any batch throws an exception")
    parser.add_argument("--ssh-host", default=None,
                        help="SSH host for docker log capture (e.g., root@192.168.0.9)")
    parser.add_argument("--no-verify-ssl", action="store_true",
                        help="Disable TLS certificate verification (for self-signed certs)")
    args = parser.parse_args()

    continue_on_error = not args.stop_on_error

    # Determine which batches to run
    if args.batches:
        run_batch_nums = set(parse_int_list(args.batches))
    else:
        run_batch_nums = {b[0] for b in BATCHES}

    if args.skip:
        skip_nums = set(parse_int_list(args.skip))
        run_batch_nums -= skip_nums

    selected = [(num, mod, desc) for (num, mod, desc) in BATCHES if num in run_batch_nums]

    if not selected:
        print("No batches selected to run. Check --batches / --skip flags.")
        sys.exit(1)

    # ── Initialize shared state, logger, and API client
    log_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), "logs")
    run_id = init_state(log_dir)

    docker = None
    if args.ssh_host:
        docker = DockerLogCapture(ssh_host=args.ssh_host)

    log = RunLogger(log_dir, run_id, docker=docker)
    api = ApiClient(args.base_url, logger=log, docker=docker,
                    tls_skip_verify=args.no_verify_ssl)

    print("=" * 72)
    print("  CRM Comprehensive Test Data Loader")
    print(f"  Target : {args.base_url}")
    print(f"  Run ID : {run_id}")
    print(f"  Batches: {', '.join(str(b[0]) for b in selected)}")
    print(f"  Log    : {log.log_path}")
    print("=" * 72)

    log.section("=== CRM Test Data Loader START ===")
    log.log(f"Target: {args.base_url}")
    log.log(f"Run ID: {run_id}")
    log.log(f"Batches: {[b[0] for b in selected]}")

    # ── Authenticate
    log.section("Authentication")
    token = authenticate(api, args.username, args.password, log)
    if not token:
        print("\n  FATAL: Authentication failed. Cannot proceed.")
        log.log("FATAL: Authentication failed")
        log.close()
        sys.exit(1)
    print(f"  Authenticated as {args.username}")

    # ── Run each batch
    batch_results: list[dict] = []
    overall_start = time.time()

    for batch_num, module_name, description in selected:
        print(f"\n{'─' * 60}")
        print(f"  BATCH {batch_num:02d}: {description}")
        print(f"{'─' * 60}")
        log.section(f"BATCH {batch_num:02d}: {description}")

        batch_start = time.time()
        status = "success"
        error_msg = ""

        try:
            mod = importlib.import_module(module_name)
            mod.run(api, log)
        except Exception as exc:
            status = "error"
            error_msg = f"{type(exc).__name__}: {exc}"
            tb = traceback.format_exc()
            log.log(f"BATCH {batch_num} ERROR: {error_msg}")
            log.log(tb)
            print(f"  ERROR in batch {batch_num}: {error_msg}")
            if not continue_on_error:
                print("  Stopping (--stop-on-error is set)")
                batch_results.append({
                    "batch": batch_num,
                    "module": module_name,
                    "status": status,
                    "error": error_msg,
                    "elapsed_s": round(time.time() - batch_start, 1),
                })
                break

        elapsed = round(time.time() - batch_start, 1)
        batch_results.append({
            "batch": batch_num,
            "module": module_name,
            "status": status,
            "error": error_msg,
            "elapsed_s": elapsed,
        })
        log.log(f"Batch {batch_num} finished in {elapsed}s — {status}")

    total_elapsed = round(time.time() - overall_start, 1)

    # ── Summary
    log.section("=== FINAL SUMMARY ===")
    success_count = sum(1 for r in batch_results if r["status"] == "success")
    error_count = sum(1 for r in batch_results if r["status"] == "error")

    summary_lines = [
        f"Total batches run : {len(batch_results)}",
        f"Succeeded         : {success_count}",
        f"Failed            : {error_count}",
        f"Total time        : {total_elapsed}s",
        "",
        f"API calls made    : {api.stats['total']}",
        f"  Success (2xx)   : {api.stats['success']}",
        f"  Exists (dedup)  : {api.stats['exists']}",
        f"  Skipped (svc)   : {api.stats['skipped_integration']}",
        f"  Client err (4xx): {api.stats['client_error']}",
        f"  Server err (5xx): {api.stats['server_error']}",
        f"  Network errors  : {api.stats['network_error']}",
        "",
    ]

    for line in summary_lines:
        log.log(line)

    # Per-batch table
    header = f"  {'Batch':>5}  {'Status':<8}  {'Time':>7}  {'Module':<32}  Error"
    separator = "  " + "-" * 90
    log.log(header)
    log.log(separator)
    for r in batch_results:
        err_short = (r["error"][:50] + "...") if len(r["error"]) > 50 else r["error"]
        line = f"  {r['batch']:>5}  {r['status']:<8}  {r['elapsed_s']:>6.1f}s  {r['module']:<32}  {err_short}"
        log.log(line)

    # Print to stdout
    print("\n" + "=" * 72)
    print("  FINAL SUMMARY")
    print("=" * 72)
    for line in summary_lines:
        print(f"  {line}")
    print(header)
    print(separator)
    for r in batch_results:
        mark = "✓" if r["status"] == "success" else "✗"
        err_short = (r["error"][:50] + "...") if len(r["error"]) > 50 else r["error"]
        print(f"  {mark} {r['batch']:>4}  {r['status']:<8}  {r['elapsed_s']:>6.1f}s  {r['module']:<32}  {err_short}")

    # ── Integration Services availability report ──
    if api.stats["skipped_integration"] > 0:
        print(f"\n  {'─' * 50}")
        print("  INTEGRATION SERVICES")
        print(f"  {'─' * 50}")
        print(f"  Endpoints skipped (service unavailable): {api.stats['skipped_integration']}")
        print("  Run with services configured to exercise these endpoints.")
        log.log(f"Integration endpoints skipped: {api.stats['skipped_integration']}")

    print(f"\n  Log file: {log.log_path}")
    print(f"  JSONL   : {log.jsonl_path}")
    print(f"  State   : {log.state_path}")
    print("=" * 72)

    log.close()

    # Exit code based on success
    if error_count > 0:
        sys.exit(1)


if __name__ == "__main__":
    main()

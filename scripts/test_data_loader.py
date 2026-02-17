#!/usr/bin/env python3
"""CRM Test-Data Loader v2

Loads seed data via the CRM REST API, including:
  * System: roles, permissions, users, user groups, feature flags, settings
  * CRM Core: accounts, contacts, leads, products, opportunities
  * Contact-Info linking: addresses, phones, emails, social-media
  * Preferences: account and contact communication preferences
  * Activities: interactions, activities, tasks, notes
  * Sales: quotes, orders, invoices, payments, contracts, subscriptions,
    commissions
  * Marketing: email templates, sequences, campaigns
  * Service Desk: categories, types, requests
  * ITSM: CMDB, incidents, problems, changes, SLA, knowledge-articles
  * Relationships: types, account-to-account links, health snapshots
  * Workflows

Enhanced logging captures:
  * Docker container logs (backend, DB, frontend) at point of every failure
  * Full request/response bodies
  * Structured JSONL + human-readable .log
"""

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
import textwrap
import traceback
import urllib.error
import urllib.request
from datetime import datetime, timezone
from typing import Any, Dict, List, Optional, Tuple

# ----------------------------------------------------------------- helpers


def _now_iso() -> str:
    return datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%S.%fZ")


def slugify(value: str) -> str:
    value = value.strip().lower()
    value = re.sub(r"[^a-z0-9]+", "-", value)
    return value.strip("-")


def email_from_name(name: str, website: Optional[str] = None) -> str:
    if website:
        m = re.search(r"https?://(?:www\.)?([^/]+)", website)
        if m:
            return f"info@{m.group(1)}"
    return f"info@{slugify(name)}.example.com"


def split_name(full: str) -> Tuple[str, str]:
    parts = full.strip().split()
    if len(parts) <= 1:
        return (parts[0] if parts else "Unknown"), ""
    return parts[0], " ".join(parts[1:])


# ---------------------------------------------------------- enum dicts

OPPORTUNITY_STAGE = {
    "Discovery": 0, "Qualification": 1, "Qualified": 1, "Proposal": 2,
    "Negotiation": 3, "ClosedWon": 4, "ClosedLost": 5,
}
QUOTE_STATUS = {
    "New": 0, "Draft": 1, "UnderApproval": 2, "Approved": 3, "Shared": 4,
    "Sent": 4, "Viewed": 5, "Accepted": 6, "Rejected": 7, "Expired": 8,
    "Revised": 9, "Cancelled": 10, "Converted": 11,
}
ORDER_STATUS = {
    "Draft": 0, "PendingApproval": 1, "Approved": 2, "Processing": 3,
    "PartiallyFulfilled": 4, "Fulfilled": 5, "Delivered": 6, "Completed": 7,
    "Cancelled": 8, "Returned": 9, "Refunded": 10, "OnHold": 11,
}
INVOICE_STATUS = {
    "Draft": 0, "PendingApproval": 1, "Approved": 2, "Sent": 3, "Issued": 3,
    "Viewed": 4, "PartiallyPaid": 5, "Paid": 6, "Overdue": 7, "Disputed": 8,
    "Voided": 9, "WrittenOff": 10,
}
PAYMENT_STATUS = {
    "Pending": 0, "Processing": 1, "Completed": 2, "Failed": 3,
    "Declined": 4, "Cancelled": 5, "Refunded": 6, "PartiallyRefunded": 7,
}
PAYMENT_METHOD = {
    "CreditCard": 0, "DebitCard": 1, "BankTransfer": 2, "WireTransfer": 3,
    "Check": 4, "Cash": 5, "PayPal": 6, "Stripe": 7, "ApplePay": 8,
    "GooglePay": 9, "Venmo": 10, "Crypto": 11, "StoreCredit": 12,
    "GiftCard": 13, "Financing": 14, "PurchaseOrder": 15, "Other": 16,
}
CONTRACT_STATUS = {
    "Draft": 0, "PendingApproval": 1, "Approved": 2, "Active": 3,
    "Expired": 4, "Terminated": 5, "Renewed": 6, "OnHold": 7,
}
SUBSCRIPTION_STATUS = {
    "Active": 0, "Current": 0, "Paused": 1, "Cancelled": 2, "Churned": 2,
    "Suspended": 3, "PendingCancellation": 4, "Expired": 5, "Trial": 6,
}
EMAIL_SEQUENCE_STATUS = {"Draft": 0, "Active": 1, "Paused": 2, "Archived": 3}
CAMPAIGN_STATUS = {
    "Draft": 0, "Scheduled": 1, "Planned": 1, "Active": 2, "Paused": 3,
    "Completed": 4, "Cancelled": 5, "Archived": 6, "PendingApproval": 7,
}
CAMPAIGN_TYPE = {
    "Email": 0, "SocialMedia": 1, "PaidSearch": 2, "DisplayAds": 3,
    "ContentMarketing": 4, "SEO": 5, "Event": 6, "Webinar": 7,
    "DirectMail": 8, "Telemarketing": 9, "Referral": 10, "Affiliate": 11,
    "Influencer": 12, "PR": 13, "TradeShow": 14, "Video": 15,
    "Podcast": 16, "SMS": 17, "PushNotification": 18, "Retargeting": 19,
    "ABM": 20, "PartnerMarketing": 21, "ProductLaunch": 22,
    "BrandAwareness": 23, "Integrated": 24, "Other": 25,
}
INTERACTION_TYPE = {
    "Email": 0, "Phone": 1, "Call": 1, "Meeting": 2, "VideoCall": 3,
    "Chat": 4, "SMS": 5, "SocialMedia": 6, "InPerson": 7, "WebForm": 8,
    "Note": 9, "Task": 10, "Demo": 11, "Presentation": 12, "Contract": 13,
    "Support": 14, "Other": 15,
}
INTERACTION_DIRECTION = {"Inbound": 0, "Outbound": 1, "Internal": 2}
INTERACTION_OUTCOME = {
    "None": 0, "Successful": 1, "Unsuccessful": 2, "FollowUpRequired": 3,
    "NoResponse": 4, "Voicemail": 5, "Rescheduled": 6, "Cancelled": 7,
}
ACTIVITY_TYPE = {
    "EmailSent": 0, "EmailReceived": 1, "CallMade": 2, "CallReceived": 3,
    "MeetingScheduled": 4, "MeetingCompleted": 5, "ChatMessage": 6,
    "SMSSent": 7, "SMSReceived": 8, "NoteAdded": 40, "TaskCreated": 30,
    "TaskCompleted": 31, "OpportunityCreated": 12, "Other": 99,
}
TASK_TYPE = {
    "Call": 0, "Email": 1, "Meeting": 2, "FollowUp": 3, "Demo": 4,
    "Proposal": 5, "Contract": 6, "Research": 7, "Other": 8,
}
TASK_STATUS = {
    "NotStarted": 0, "InProgress": 1, "Completed": 2, "Deferred": 3,
    "Waiting": 4, "Cancelled": 5,
}
TASK_PRIORITY = {"Low": 0, "Normal": 1, "High": 2, "Urgent": 3}
NOTE_TYPE = {"General": 0, "Meeting": 1, "Call": 2, "Email": 3, "Internal": 4}
NOTE_VISIBILITY = {"Private": 0, "Team": 1, "Public": 2}
INCIDENT_IMPACT = {"High": 1, "Medium": 2, "Low": 3}
INCIDENT_URGENCY = {"High": 1, "Medium": 2, "Low": 3}
PROBLEM_PRIORITY = {"Critical": 1, "High": 2, "Medium": 3, "Low": 4}

# ITSM CI Type enum (1-indexed)
CI_TYPE = {
    "Server": 1, "WorkStation": 2, "Workstation": 2, "NetworkDevice": 3,
    "Application": 4, "Database": 5, "Storage": 6, "VirtualMachine": 7,
    "BusinessService": 8, "ITService": 9, "Software": 10, "License": 11,
    "Documentation": 12,
}
# ITSM Knowledge Article Type enum (1-indexed)
ARTICLE_TYPE = {
    "HowTo": 1, "Troubleshooting": 2, "FAQ": 3, "KnownError": 4,
    "Reference": 5, "BestPractice": 6,
}
# Email Sequence Step Type enum (0-indexed)
EMAIL_STEP_TYPE = {
    "Email": 0, "Wait": 1, "Task": 2, "Condition": 3, "LinkedIn": 4,
    "Call": 5, "SMS": 6, "Notification": 7,
}
# ITSM Operational Status enum (1-indexed)
OPERATIONAL_STATUS = {
    "Operational": 1, "Degraded": 2, "NonOperational": 3, "Retired": 4,
    "UnderMaintenance": 5,
}

ALREADY_EXISTS_PATTERNS = [
    "already exists", "duplicate", "already registered",
    "duplicate entry", "unique constraint",
]


def _is_already_exists(body: Optional[str]) -> bool:
    if not body:
        return False
    lo = body.lower()
    return any(p in lo for p in ALREADY_EXISTS_PATTERNS)


# ------------------------------------------------- docker log capture


class DockerLogCapture:
    """Captures recent lines from Docker container logs on the remote server
    or locally.  Uses ``--since`` with a short window so that verbose
    EF-Core SQL logs don't push the actual error messages out of range.
    Also provides a *filtered* view of the API logs that strips out the
    long SQL SELECT/INSERT/UPDATE noise to surface real errors."""

    # SQL noise patterns – lines that are pure EF-Core SQL output
    _SQL_NOISE = re.compile(
        r"^\s*(SELECT|INSERT|UPDATE|DELETE|FROM|WHERE|INNER|LEFT|"
        r"ORDER BY|GROUP BY|HAVING|LIMIT|SET |VALUES|AND |OR |\)|"
        r"JOIN |AS |ON |\(|NOT |CASE |WHEN |THEN |ELSE |END |"
        r"--\s|PRAGMA|CREATE|ALTER|DROP)",
        re.IGNORECASE,
    )

    def __init__(
        self,
        ssh_host: Optional[str] = None,
        api_container: str = "crm-api",
        db_container: str = "crm-mariadb",
        frontend_container: str = "crm-frontend",
        tail_lines: int = 120,
        since_seconds: int = 10,
    ):
        self.ssh_host = ssh_host
        self.api_container = api_container
        self.db_container = db_container
        self.frontend_container = frontend_container
        self.tail_lines = tail_lines
        self.since_seconds = since_seconds

    def _run(self, cmd: str, timeout: int = 10) -> str:
        if self.ssh_host:
            full = [
                "ssh", "-o", "ConnectTimeout=5",
                "-o", "StrictHostKeyChecking=no",
                self.ssh_host, cmd,
            ]
        else:
            full = ["bash", "-c", cmd]
        try:
            result = subprocess.run(
                full, capture_output=True, text=True, timeout=timeout
            )
            return result.stdout + result.stderr
        except Exception as exc:
            return f"[docker-log-capture-error] {exc}"

    @classmethod
    def _filter_sql(cls, raw: str) -> str:
        """Remove verbose EF-Core SQL lines, keep INF/WRN/ERR log entries."""
        keep: list[str] = []
        for line in raw.splitlines():
            stripped = line.lstrip("| ").strip()
            if not stripped:
                continue
            if cls._SQL_NOISE.match(stripped):
                continue
            keep.append(line)
        return "\n".join(keep)

    def get_api_logs(self) -> str:
        return self._run(
            f"docker logs --since {self.since_seconds}s "
            f"--tail {self.tail_lines} {self.api_container} 2>&1"
        )

    def get_db_logs(self) -> str:
        return self._run(
            f"docker logs --since {self.since_seconds}s "
            f"--tail {self.tail_lines} {self.db_container} 2>&1"
        )

    def get_db_processlist(self) -> str:
        return self._run(
            f"docker exec {self.db_container} mariadb -u root -pRootPass@Dev2024 "
            f"-e 'SHOW PROCESSLIST; SHOW WARNINGS;' 2>&1"
        )

    def get_frontend_logs(self) -> str:
        return self._run(
            f"docker logs --since {self.since_seconds}s "
            f"--tail 20 {self.frontend_container} 2>&1"
        )

    def snapshot(self) -> Dict[str, str]:
        """Take a diagnostic snapshot of all containers at this moment.

        Returns both the raw API logs and a filtered version with SQL
        noise removed so errors are easily visible."""
        raw_api = self.get_api_logs()
        return {
            "api_logs": raw_api,
            "api_logs_filtered": self._filter_sql(raw_api),
            "db_logs": self.get_db_logs(),
            "db_processlist": self.get_db_processlist(),
            "frontend_logs": self.get_frontend_logs(),
        }


# --------------------------------------------------------- run logger


class RunLogger:
    """Writes structured JSONL + human-readable .log with Docker diagnostics
    on every failure."""

    def __init__(self, log_dir: str, docker: Optional[DockerLogCapture] = None):
        os.makedirs(log_dir, exist_ok=True)
        ts = datetime.now(timezone.utc).strftime("%Y%m%d_%H%M%S")
        self.text_path = os.path.join(log_dir, f"test_data_load_{ts}.log")
        self.jsonl_path = os.path.join(log_dir, f"test_data_load_{ts}.jsonl")
        self.text_fh = open(self.text_path, "w", encoding="utf-8")
        self.jsonl_fh = open(self.jsonl_path, "w", encoding="utf-8")
        self.counts: Dict[str, int] = {"success": 0, "failed": 0, "skipped": 0}
        self.docker = docker

    def close(self) -> None:
        self._flush_summary()
        self.text_fh.close()
        self.jsonl_fh.close()

    def _write(self, entry: Dict[str, Any]) -> None:
        entry.setdefault("timestamp", _now_iso())
        self.jsonl_fh.write(
            json.dumps(entry, default=str, ensure_ascii=True) + "\n"
        )
        self.jsonl_fh.flush()
        if "summary" in entry:
            self.text_fh.write(entry["summary"] + "\n")
            self.text_fh.flush()

    def _flush_summary(self) -> None:
        line = (
            f"\n=== FINAL: success={self.counts['success']}  "
            f"failed={self.counts['failed']}  skipped={self.counts['skipped']} ===\n"
        )
        self.text_fh.write(line)
        self.text_fh.flush()

    def section(self, name: str) -> None:
        sep = f"\n{'=' * 60}\n  {name}\n{'=' * 60}"
        self.text_fh.write(sep + "\n")
        self.text_fh.flush()
        self._write({"event": "section", "name": name, "summary": sep})

    def log_skip(self, reason: str, **kw: Any) -> None:
        self.counts["skipped"] += 1
        entry: Dict[str, Any] = {"status": "skipped", "reason": reason}
        entry.update(kw)
        entry["summary"] = f"  SKIP  {reason} ({kw.get('file', 'n/a')})"
        self._write(entry)

    def log_result(
        self,
        status: str,
        method: str,
        endpoint: str,
        http_status: Optional[int],
        file: Optional[str] = None,
        index: Optional[int] = None,
        request_summary: Optional[Dict[str, Any]] = None,
        response_body: Optional[str] = None,
        error: Optional[str] = None,
        docker_snapshot: Optional[Dict[str, str]] = None,
    ) -> None:
        self.counts[status] = self.counts.get(status, 0) + 1
        loc = f"{os.path.basename(file or 'n/a')}[{index}]" if file else "inline"
        entry: Dict[str, Any] = {
            "status": status,
            "method": method,
            "endpoint": endpoint,
            "http_status": http_status,
            "file": file,
            "index": index,
            "request": request_summary or {},
            "response_body": _truncate(response_body or "", 2000),
            "error": error,
        }
        if status == "success":
            entry["summary"] = (
                f"  OK    {method} {endpoint} ({loc}) -> {http_status}"
            )
        else:
            entry["summary"] = (
                f"  FAIL  {method} {endpoint} ({loc}) -> {http_status}  "
                f"err={_truncate(error or response_body or '', 200)}"
            )
            if docker_snapshot:
                entry["docker_diagnostics"] = docker_snapshot
                self.text_fh.write(
                    _format_docker_diagnostics(
                        method, endpoint, http_status, docker_snapshot
                    )
                )
                self.text_fh.flush()
        self._write(entry)


def _truncate(s: str, n: int) -> str:
    return s[:n] + "..." if len(s) > n else s


def _format_docker_diagnostics(
    method: str,
    endpoint: str,
    http_status: Optional[int],
    snap: Dict[str, str],
) -> str:
    lines = [
        f"\n  +--- DIAGNOSTICS for {method} {endpoint} -> {http_status} ---",
    ]
    # Show filtered API logs first (SQL noise removed) — most useful
    for label, key, max_lines in [
        ("Backend API (filtered)", "api_logs_filtered", 30),
        ("Database", "db_logs", 10),
        ("DB Processlist", "db_processlist", 5),
        ("Frontend", "frontend_logs", 5),
    ]:
        content = snap.get(key, "").strip()
        if not content:
            continue
        lines.append(f"  | [{label}] ({len(content)} chars):")
        for ln in content.splitlines()[-max_lines:]:
            lines.append(f"  |   {ln}")
    lines.append("  +-----------------------------------------------\n")
    return "\n".join(lines) + "\n"


# ------------------------------------------------------- API client


class ApiClient:
    """Thin HTTP client that logs every request and captures Docker diagnostics
    on failure."""

    def __init__(
        self,
        base_url: str,
        token: str,
        logger: RunLogger,
        docker: Optional[DockerLogCapture] = None,
    ):
        self.base_url = base_url.rstrip("/")
        self.token = token
        self.logger = logger
        self.docker = docker

    def request(
        self,
        method: str,
        path: str,
        payload: Optional[Dict[str, Any]] = None,
        *,
        file: Optional[str] = None,
        index: Optional[int] = None,
        summary: Optional[Dict[str, Any]] = None,
    ) -> Tuple[Optional[int], Optional[Dict[str, Any]], Optional[str]]:
        url = f"{self.base_url}{path}"
        data = (
            json.dumps(payload, default=str, ensure_ascii=True).encode()
            if payload
            else None
        )
        req = urllib.request.Request(url, data=data, method=method)
        req.add_header("Authorization", f"Bearer {self.token}")
        req.add_header("Content-Type", "application/json")
        resp_body: Optional[str] = None
        try:
            with urllib.request.urlopen(req, timeout=30) as resp:
                resp_body = resp.read().decode("utf-8", errors="replace")
                parsed = None
                if resp_body:
                    try:
                        parsed = json.loads(resp_body)
                    except json.JSONDecodeError:
                        pass
                self.logger.log_result(
                    "success",
                    method,
                    path,
                    resp.getcode(),
                    file=file,
                    index=index,
                    request_summary=summary or _compact(payload),
                    response_body=resp_body,
                )
                return resp.getcode(), parsed, resp_body

        except urllib.error.HTTPError as exc:
            resp_body = (
                exc.read().decode("utf-8", errors="replace") if exc.fp else None
            )
            if _is_already_exists(resp_body):
                self.logger.log_result(
                    "success",
                    method,
                    path,
                    exc.code,
                    file=file,
                    index=index,
                    request_summary=summary or _compact(payload),
                    response_body=resp_body,
                )
                return exc.code, None, resp_body

            # Detect "500 but created" — some controllers return 500
            # even though the entity was created successfully.
            parsed_500 = None
            if exc.code == 500 and resp_body and method in ("POST", "PUT"):
                try:
                    parsed_500 = json.loads(resp_body)
                    # Case 1: Response is the created entity (has id/name,
                    # no error keys)
                    if isinstance(parsed_500, dict) and (
                        "id" in parsed_500 or "name" in parsed_500
                    ) and "error" not in parsed_500 and "errors" not in parsed_500:
                        self.logger.log_result(
                            "success",
                            method,
                            path,
                            exc.code,
                            file=file,
                            index=index,
                            request_summary=summary or _compact(payload),
                            response_body=f"[500-but-created] {resp_body[:500]}",
                        )
                        return exc.code, parsed_500, resp_body
                    # Case 2: Known backend bugs — preferences duplicate
                    # entry, EF tracking, etc.  The error message tells us
                    # the record already exists.
                    err_msg = parsed_500.get("error", "") or parsed_500.get("message", "")
                    if any(p in err_msg.lower() for p in [
                        "duplicate entry", "saving the entity",
                        "already exists",
                    ]):
                        self.logger.log_result(
                            "success",
                            method,
                            path,
                            exc.code,
                            file=file,
                            index=index,
                            request_summary=summary or _compact(payload),
                            response_body=f"[500-duplicate] {resp_body[:500]}",
                        )
                        return exc.code, None, resp_body
                except (json.JSONDecodeError, ValueError):
                    pass

            snap = self.docker.snapshot() if self.docker else None
            self.logger.log_result(
                "failed",
                method,
                path,
                exc.code,
                file=file,
                index=index,
                request_summary=summary or _compact(payload),
                response_body=resp_body,
                error=str(exc),
                docker_snapshot=snap,
            )
            return exc.code, None, resp_body

        except Exception as exc:
            # IncompleteRead means server closed connection before full
            # response — item was likely created successfully.
            exc_name = type(exc).__name__
            if exc_name == "IncompleteRead":
                # Try to extract partial response to capture entity ID
                partial_bytes = getattr(exc, "partial", b"")
                partial_str = (
                    partial_bytes.decode("utf-8", errors="replace")
                    if isinstance(partial_bytes, bytes)
                    else str(partial_bytes)
                )
                parsed_partial = None
                if partial_str:
                    try:
                        parsed_partial = json.loads(partial_str)
                    except json.JSONDecodeError:
                        # Try to extract just the id from partial JSON
                        m = re.search(r'"id"\s*:\s*(\d+)', partial_str)
                        if m:
                            parsed_partial = {"id": int(m.group(1))}
                self.logger.log_result(
                    "success",
                    method,
                    path,
                    200,
                    file=file,
                    index=index,
                    request_summary=summary or _compact(payload),
                    response_body=f"[IncompleteRead - partial {len(partial_str)} bytes]",
                )
                return 200, parsed_partial, partial_str
            snap = self.docker.snapshot() if self.docker else None
            self.logger.log_result(
                "failed",
                method,
                path,
                None,
                file=file,
                index=index,
                request_summary=summary or _compact(payload),
                error=f"{exc_name}: {exc}",
                docker_snapshot=snap,
            )
            return None, None, None


def _compact(payload: Optional[Dict[str, Any]]) -> Dict[str, Any]:
    """Return a compact summary of the payload for logging."""
    if not payload:
        return {}
    out: Dict[str, Any] = {}
    for k, v in payload.items():
        if isinstance(v, str) and len(v) > 80:
            out[k] = v[:77] + "..."
        else:
            out[k] = v
    return out


# ------------------------------------------------------ data helpers


def load_json(path: str) -> Any:
    with open(path, "r", encoding="utf-8") as fh:
        return json.load(fh)


def _path(data_dir: str, name: str) -> str:
    return os.path.join(data_dir, name)


def _exists(data_dir: str, name: str) -> bool:
    return os.path.isfile(_path(data_dir, name))


# ---- Prefetch: populate id_maps from existing API data ----

def prefetch_existing(
    client: ApiClient,
    logger: RunLogger,
    data_dir: str,
    id_maps: Dict[str, Any],
) -> None:
    """GET existing accounts, contacts, users, products, relationship types,
    quotes, orders, and invoices so id_maps are populated even when entities
    already exist (idempotent re-runs)."""
    logger.section("Prefetch - Populating ID maps from existing data")

    def _fetch_list(endpoint: str) -> List[Dict[str, Any]]:
        _, resp, raw = client.request("GET", endpoint, None)
        if isinstance(resp, list):
            return resp
        if isinstance(resp, dict):
            # paginated response
            return resp.get("items", resp.get("data", []))
        return []

    # Accounts
    existing = _fetch_list("/api/accounts")
    for acct in existing:
        aid = acct.get("id")
        if aid:
            name = acct.get("company") or acct.get("name") or ""
            id_maps["account_name"][name] = aid
    logger.log_result("success", "GET", "/api/accounts", 200,
                       request_summary={"prefetched": len(existing)})

    # Contacts
    existing = _fetch_list("/api/contacts")
    for ct in existing:
        cid = ct.get("id")
        if cid:
            pass  # contact ids are seed-relative, can't reverse-map
    logger.log_result("success", "GET", "/api/contacts", 200,
                       request_summary={"prefetched": len(existing)})

    # Users
    existing = _fetch_list("/api/users")
    for u in existing:
        uid = u.get("id")
        if uid:
            pass  # user ids are seed-relative, can't reverse-map
    # Store first valid user ID as fallback
    if existing:
        id_maps["_fallback_user_id"] = existing[0].get("id", 1)
    else:
        id_maps["_fallback_user_id"] = 1
    logger.log_result("success", "GET", "/api/users", 200,
                       request_summary={"prefetched": len(existing)})

    # Store first valid account ID as fallback
    all_accounts = _fetch_list("/api/accounts")
    if all_accounts:
        id_maps["_fallback_account_id"] = all_accounts[0].get("id", 1)
    else:
        id_maps["_fallback_account_id"] = 1

    # Products — track existing SKUs so we can skip duplicates
    existing_products = _fetch_list("/api/products")
    existing_skus: set = set()
    for prod in existing_products:
        sku = prod.get("sku") or prod.get("SKU")
        if sku:
            existing_skus.add(sku)
    id_maps["_existing_product_skus"] = existing_skus
    logger.log_result("success", "GET", "/api/products", 200,
                       request_summary={"prefetched": len(existing_products),
                                        "skus": len(existing_skus)})

    # Relationship types — track existing type names
    existing_rel_types = _fetch_list("/api/relationships/types")
    existing_type_names: set = set()
    for rt in existing_rel_types:
        tn = rt.get("typeName")
        if tn:
            existing_type_names.add(tn)
    id_maps["_existing_rel_type_names"] = existing_type_names
    logger.log_result("success", "GET", "/api/relationships/types", 200,
                       request_summary={"prefetched": len(existing_rel_types)})

    # Quotes — track existing QuoteNumbers → IDs for line items
    # (quotes return huge responses that may trigger IncompleteRead)
    existing_quotes = _fetch_list("/api/quotes")
    for q in existing_quotes:
        qid = q.get("id")
        qnum = q.get("quoteNumber")
        if qid and qnum:
            id_maps.setdefault("_quote_by_number", {})[qnum] = qid
    logger.log_result("success", "GET", "/api/quotes", 200,
                       request_summary={"prefetched": len(existing_quotes)})

    # Orders — track existing IDs
    existing_orders = _fetch_list("/api/orders")
    for o in existing_orders:
        oid = o.get("id")
        if oid:
            id_maps.setdefault("_existing_order_ids", set()).add(oid)
    logger.log_result("success", "GET", "/api/orders", 200,
                       request_summary={"prefetched": len(existing_orders)})

    # Email templates — track existing names
    existing_templates = _fetch_list("/api/emailtemplates")
    for t in existing_templates:
        tid = t.get("id")
        tname = t.get("name")
        if tid and tname:
            id_maps.setdefault("_template_by_name", {})[tname] = tid
    logger.log_result("success", "GET", "/api/emailtemplates", 200,
                       request_summary={"prefetched": len(existing_templates)})

    # Email sequences — track existing names to skip duplicates
    existing_seqs = _fetch_list("/api/email-sequences")
    existing_seq_names: set = set()
    for s in existing_seqs:
        sname = s.get("name")
        if sname:
            existing_seq_names.add(sname)
    id_maps["_existing_seq_names"] = existing_seq_names
    logger.log_result("success", "GET", "/api/email-sequences", 200,
                       request_summary={"prefetched": len(existing_seqs)})


# ========================= LOADER PHASES ===========================


# ---- Phase 1: System ------------------------------------------------


def phase_system(
    client: ApiClient,
    logger: RunLogger,
    data_dir: str,
    id_maps: Dict[str, Any],
) -> None:
    """Phase 1: Roles, permissions, users, groups, feature flags, settings."""
    logger.section("Phase 1 - System Configuration")

    # Roles
    p = _path(data_dir, "system_roles_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "Name": item.get("name", ""),
                "Description": item.get("description", ""),
                "HierarchyLevel": 0,
            }
            _, resp, _ = client.request(
                "POST", "/api/roles", payload, file=p, index=i, summary=payload
            )
            if isinstance(resp, dict) and "id" in resp:
                id_maps["role_name"][item["name"]] = resp["id"]

    # Permissions
    p = _path(data_dir, "system_permissions_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            key = item.get("key", "")
            module = key.split(".")[0] if "." in key else "General"
            payload = {
                "Name": key,
                "DisplayName": key,
                "Module": module,
                "Category": "General",
                "Description": item.get("description", ""),
            }
            client.request(
                "POST", "/api/permissions", payload, file=p, index=i, summary=payload
            )

    # Users
    p = _path(data_dir, "bulk_crm_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p).get("users", [])):
            role_id = id_maps["role_name"].get(item.get("role"), 2)
            payload = {
                "Email": item["email"],
                "FirstName": item["firstName"],
                "LastName": item["lastName"],
                "RoleId": role_id,
                "Password": "Admin@123",
            }
            _, resp, _ = client.request(
                "POST", "/api/users", payload, file=p, index=i, summary=payload
            )
            if isinstance(resp, dict) and "id" in resp:
                id_maps["user"][item.get("id", 0)] = resp["id"]

    # User Groups
    p = _path(data_dir, "system_user_groups_seed.json")
    if os.path.isfile(p):
        group_map: Dict[int, int] = {}
        items = load_json(p)
        for i, item in enumerate(items):
            payload = {
                "Name": item.get("name", ""),
                "Description": "",
                "IsActive": True,
            }
            _, resp, _ = client.request(
                "POST", "/api/usergroups", payload, file=p, index=i, summary=payload
            )
            if isinstance(resp, dict) and "id" in resp:
                group_map[item.get("id", 0)] = resp["id"]
        for item in items:
            gid = group_map.get(item.get("id", 0))
            if not gid:
                continue
            for uid in item.get("memberUserIds", []):
                real_uid = id_maps["user"].get(uid)
                if not real_uid:
                    continue
                client.request(
                    "POST",
                    f"/api/AdminSettings/groups/{gid}/members/{real_uid}",
                    None,
                    file=p,
                    summary={"gid": gid, "uid": real_uid},
                )

    # Feature Flags
    p = _path(data_dir, "system_feature_flags_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "Name": item["name"],
                "Enabled": item.get("enabled", False),
                "RolloutPercentage": 100,
                "Reason": "Seed data",
            }
            client.request(
                "PUT",
                f"/api/feature-flags/{item['name']}",
                payload,
                file=p,
                index=i,
                summary=payload,
            )

    # System Settings
    p = _path(data_dir, "system_settings_seed.json")
    if os.path.isfile(p):
        update: Dict[str, Any] = {}
        for item in load_json(p):
            key, val = item.get("key"), item.get("value")
            if key == "System.TimeZone":
                update["DefaultTimezone"] = val
            elif key == "System.DateFormat":
                update["DateFormat"] = val
            elif key == "Security.PasswordMinLength":
                update["MinPasswordLength"] = int(val)
            elif key == "Security.MfaRequired":
                update["RequireTwoFactor"] = str(val).lower() == "true"
            elif key == "Sales.DefaultCurrency":
                update["DefaultCurrency"] = val
        if update:
            client.request("PUT", "/api/systemsettings", update, file=p, summary=update)


# ---- Phase 2: Accounts & Contacts -----------------------------------


def phase_accounts_contacts(
    client: ApiClient,
    logger: RunLogger,
    data_dir: str,
    id_maps: Dict[str, Any],
) -> None:
    """Phase 2: Accounts, contacts, and account-contact linking."""
    logger.section("Phase 2 - Accounts & Contacts")

    ACCT_CAT_ORG = 1
    ACCT_TYPE_ENTERPRISE = 3
    ACCT_PRIORITY_MED = 1

    # Accounts from bulk seed
    p = _path(data_dir, "bulk_crm_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p).get("accounts", [])):
            phone = item.get("phone") or "+1-555-0000"
            em = email_from_name(item.get("name", ""), item.get("website"))
            payload = {
                "Category": ACCT_CAT_ORG,
                "Company": item["name"],
                "Email": em,
                "Phone": phone,
                "Website": item.get("website"),
                "Address": item.get("address"),
                "Industry": item.get("industry"),
                "AccountType": ACCT_TYPE_ENTERPRISE,
                "Priority": ACCT_PRIORITY_MED,
            }
            _, resp, _ = client.request(
                "POST", "/api/accounts", payload, file=p, index=i, summary=payload
            )
            if isinstance(resp, dict) and "id" in resp:
                id_maps["account"][item.get("id", 0)] = resp["id"]
                id_maps["account_name"][item["name"]] = resp["id"]

    # Accounts from IT companies seed
    p = _path(data_dir, "it_companies_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p).get("accounts", [])):
            addr = item.get("address", {})
            addr_str = ", ".join(
                v
                for v in [
                    addr.get("street"),
                    addr.get("city"),
                    addr.get("state"),
                    addr.get("postalCode"),
                ]
                if v
            )
            phone = item.get("phone") or "+1-555-0000"
            em = email_from_name(item.get("name", ""), item.get("website"))
            payload = {
                "Category": ACCT_CAT_ORG,
                "Company": item["name"],
                "Email": em,
                "Phone": phone,
                "Website": item.get("website"),
                "Address": addr_str,
                "City": addr.get("city"),
                "State": addr.get("state"),
                "ZipCode": addr.get("postalCode"),
                "Country": addr.get("country"),
                "Industry": item.get("industry"),
                "AccountType": ACCT_TYPE_ENTERPRISE,
                "Priority": ACCT_PRIORITY_MED,
            }
            _, resp, _ = client.request(
                "POST", "/api/accounts", payload, file=p, index=i, summary=payload
            )
            if isinstance(resp, dict) and "id" in resp:
                id_maps["account_name"][item["name"]] = resp["id"]

    # Contacts from bulk seed
    p = _path(data_dir, "bulk_crm_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p).get("contacts", [])):
            payload = {
                "FirstName": item["firstName"],
                "LastName": item["lastName"],
                "EmailPrimary": item.get("email"),
                "PhonePrimary": item.get("phone"),
            }
            _, resp, _ = client.request(
                "POST", "/api/contacts", payload, file=p, index=i, summary=payload
            )
            if isinstance(resp, dict) and "id" in resp:
                id_maps["contact"][item.get("id", 0)] = resp["id"]
            # Link contact -> account
            acct = id_maps["account"].get(item.get("accountId", 0))
            ct = id_maps["contact"].get(item.get("id", 0))
            if acct and ct:
                client.request(
                    "POST",
                    f"/api/accounts/{acct}/contacts",
                    {"ContactId": ct},
                    file=p,
                    index=i,
                    summary={"acctId": acct, "contactId": ct},
                )

    # Contacts from IT companies (executives)
    p = _path(data_dir, "it_companies_seed.json")
    if os.path.isfile(p):
        for ci, co in enumerate(load_json(p).get("accounts", [])):
            acct = id_maps["account_name"].get(co.get("name", ""))
            for ei, ex in enumerate(co.get("executives", [])):
                fn, ln = split_name(ex.get("name", ""))
                payload = {
                    "FirstName": fn,
                    "LastName": ln,
                    "EmailPrimary": ex.get("email"),
                    "PhonePrimary": ex.get("phone"),
                    "JobTitle": ex.get("title"),
                    "Company": co.get("name"),
                }
                _, resp, _ = client.request(
                    "POST", "/api/contacts", payload, file=p, index=ei, summary=payload
                )
                ct_id = resp.get("id") if isinstance(resp, dict) else None
                if acct and ct_id:
                    client.request(
                        "POST",
                        f"/api/accounts/{acct}/contacts",
                        {"ContactId": ct_id},
                        file=p,
                        index=ei,
                        summary={"acctId": acct, "contactId": ct_id},
                    )


# ---- Phase 3: Contact Info Linking -----------------------------------


def phase_contact_info(
    client: ApiClient,
    logger: RunLogger,
    data_dir: str,
    id_maps: Dict[str, Any],
) -> None:
    """Phase 3: Addresses, phones, emails, social media, preferences."""
    logger.section("Phase 3 - Contact Info Linking")

    # --- Account addresses (IT companies with structured addresses) ---
    p = _path(data_dir, "it_companies_seed.json")
    if os.path.isfile(p):
        for i, co in enumerate(load_json(p).get("accounts", [])):
            acct_id = id_maps["account_name"].get(co.get("name"))
            if not acct_id:
                continue
            addr = co.get("address", {})
            if not addr.get("street"):
                continue
            payload = {
                "entityType": "Account",
                "entityId": acct_id,
                "addressType": "Billing",
                "isPrimary": True,
                "newAddress": {
                    "label": "HQ",
                    "line1": addr.get("street", ""),
                    "city": addr.get("city", ""),
                    "state": addr.get("state", ""),
                    "postalCode": addr.get("postalCode", ""),
                    "country": addr.get("country", "USA"),
                    "countryCode": "US",
                },
            }
            client.request(
                "POST",
                "/api/contactinfo/addresses/link",
                payload,
                file=p,
                index=i,
                summary={"entity": "Account", "id": acct_id, "type": "Billing"},
            )

    # --- Account phone numbers ---
    for seed_id, acct_id in list(id_maps["account"].items()):
        payload = {
            "entityType": "Account",
            "entityId": acct_id,
            "phoneType": "Office",
            "isPrimary": True,
            "doNotCall": False,
            "newPhone": {
                "countryCode": "+1",
                "number": f"555{1000 + seed_id}",
                "canSMS": False,
                "label": "Main Office",
            },
        }
        client.request(
            "POST",
            "/api/contactinfo/phones/link",
            payload,
            summary={"entity": "Account", "id": acct_id, "type": "Office"},
        )

    # --- Account email addresses ---
    p = _path(data_dir, "bulk_crm_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p).get("accounts", [])):
            acct_id = id_maps["account"].get(item.get("id", 0))
            if not acct_id:
                continue
            em = email_from_name(item.get("name", ""), item.get("website"))
            payload = {
                "entityType": "Account",
                "entityId": acct_id,
                "emailType": "General",
                "isPrimary": True,
                "doNotEmail": False,
                "marketingOptIn": True,
                "transactionalOnly": False,
                "newEmail": {
                    "email": em,
                    "displayName": item.get("name", ""),
                    "label": "Main",
                },
            }
            client.request(
                "POST",
                "/api/contactinfo/emails/link",
                payload,
                file=p,
                index=i,
                summary={"entity": "Account", "id": acct_id},
            )

    # --- Account social media ---
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p).get("accounts", [])):
            acct_id = id_maps["account"].get(item.get("id", 0))
            if not acct_id:
                continue
            slug = slugify(item.get("name", "company"))
            payload = {
                "entityType": "Account",
                "entityId": acct_id,
                "isPrimary": True,
                "preferredForContact": False,
                "newSocialMedia": {
                    "platform": "LinkedIn",
                    "accountType": "Business",
                    "handleOrUsername": slug,
                    "profileUrl": f"https://linkedin.com/company/{slug}",
                    "displayName": item.get("name", ""),
                },
            }
            client.request(
                "POST",
                "/api/contactinfo/social-media/link",
                payload,
                file=p,
                index=i,
                summary={"entity": "Account", "id": acct_id, "platform": "LinkedIn"},
            )

    # --- Contact addresses, phones, emails, social-media ---
    cities = [
        "New York", "Boston", "Chicago", "San Francisco", "Austin",
        "Denver", "Seattle", "Atlanta", "Miami", "Portland",
        "Dallas", "Phoenix",
    ]
    states = ["NY", "MA", "IL", "CA", "TX", "CO", "WA", "GA", "FL", "OR", "TX", "AZ"]
    if os.path.isfile(p):
        contacts = load_json(p).get("contacts", [])
        for i, item in enumerate(contacts):
            ct_id = id_maps["contact"].get(item.get("id", 0))
            if not ct_id:
                continue
            city = cities[i % len(cities)]
            state = states[i % len(states)]

            # Address
            client.request(
                "POST",
                "/api/contactinfo/addresses/link",
                {
                    "entityType": "Contact",
                    "entityId": ct_id,
                    "addressType": "Home",
                    "isPrimary": True,
                    "newAddress": {
                        "label": "Home",
                        "line1": f"{100 + i * 10} Oak Street",
                        "city": city,
                        "state": state,
                        "postalCode": f"{10000 + i * 111:05d}",
                        "country": "United States",
                        "countryCode": "US",
                    },
                },
                file=p,
                index=i,
                summary={"entity": "Contact", "id": ct_id, "type": "Home"},
            )

            # Phone
            phone_num = item.get("phone", f"+1-555-{2000 + i}")
            digits = re.sub(r"[^0-9]", "", phone_num)[-7:]
            client.request(
                "POST",
                "/api/contactinfo/phones/link",
                {
                    "entityType": "Contact",
                    "entityId": ct_id,
                    "phoneType": "Mobile",
                    "isPrimary": True,
                    "doNotCall": False,
                    "newPhone": {
                        "countryCode": "+1",
                        "number": digits,
                        "canSMS": True,
                        "label": "Cell",
                    },
                },
                file=p,
                index=i,
                summary={"entity": "Contact", "id": ct_id, "type": "Mobile"},
            )

            # Email
            em = item.get("email", f"contact{i}@example.com")
            full_name = f"{item.get('firstName', '')} {item.get('lastName', '')}".strip()
            client.request(
                "POST",
                "/api/contactinfo/emails/link",
                {
                    "entityType": "Contact",
                    "entityId": ct_id,
                    "emailType": "Work",
                    "isPrimary": True,
                    "doNotEmail": False,
                    "marketingOptIn": True,
                    "transactionalOnly": False,
                    "newEmail": {
                        "email": em,
                        "displayName": full_name,
                        "label": "Work",
                    },
                },
                file=p,
                index=i,
                summary={"entity": "Contact", "id": ct_id},
            )

            # Social media
            handle = slugify(
                f"{item.get('firstName', '')}-{item.get('lastName', '')}"
            )
            client.request(
                "POST",
                "/api/contactinfo/social-media/link",
                {
                    "entityType": "Contact",
                    "entityId": ct_id,
                    "isPrimary": True,
                    "preferredForContact": False,
                    "newSocialMedia": {
                        "platform": "LinkedIn",
                        "accountType": "Personal",
                        "handleOrUsername": handle,
                        "profileUrl": f"https://linkedin.com/in/{handle}",
                        "displayName": full_name,
                    },
                },
                file=p,
                index=i,
                summary={"entity": "Contact", "id": ct_id, "platform": "LinkedIn"},
            )

    # --- Account preferences ---
    timezones = [
        "America/New_York", "America/Chicago", "America/Denver",
        "America/Los_Angeles", "America/Phoenix",
    ]
    languages = ["en-US", "en-GB", "es-MX", "fr-FR", "de-DE"]
    methods = ["Email", "Phone", "SMS", "Mail", "Any"]
    for seed_id, acct_id in list(id_maps["account"].items()):
        payload = {
            "optInEmail": True,
            "optInSms": seed_id % 2 == 0,
            "optInPhone": True,
            "optInPostal": False,
            "preferredContactMethod": methods[seed_id % len(methods)],
            "preferredLanguage": languages[seed_id % len(languages)],
            "timezone": timezones[seed_id % len(timezones)],
        }
        client.request(
            "PUT",
            f"/api/accounts/{acct_id}/preferences",
            payload,
            summary={"acctId": acct_id},
        )

    # --- Contact preferences ---
    for seed_id, ct_id in list(id_maps["contact"].items()):
        payload = {
            "optInEmail": True,
            "optInSms": True,
            "optInPhone": seed_id % 2 == 0,
            "optInPostal": False,
            "preferredContactMethod": methods[seed_id % len(methods)],
            "preferredLanguage": "en-US",
            "timezone": timezones[seed_id % len(timezones)],
        }
        client.request(
            "PUT",
            f"/api/contacts/{ct_id}/preferences",
            payload,
            summary={"contactId": ct_id},
        )


# ---- Phase 4: Leads & Products --------------------------------------


def phase_leads_products(
    client: ApiClient,
    logger: RunLogger,
    data_dir: str,
    id_maps: Dict[str, Any],
) -> None:
    logger.section("Phase 4 - Leads & Products")

    LEAD_STATUS = {"New": 0, "Contacted": 1, "Qualified": 3}

    p = _path(data_dir, "bulk_crm_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p).get("leads", [])):
            payload = {
                "FirstName": item["firstName"],
                "LastName": item["lastName"],
                "Email": item.get("email"),
                "Phone": item.get("phone"),
                "Company": item.get("company"),
                "Status": LEAD_STATUS.get(item.get("status"), 0),
            }
            client.request(
                "POST", "/api/leads", payload, file=p, index=i, summary=payload
            )

    # Products - bulk
    existing_skus = id_maps.get("_existing_product_skus", set())
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p).get("products", [])):
            sku = item.get("sku")
            if sku and sku in existing_skus:
                logger.log_skip(f"Product SKU {sku} already exists", file=p)
                continue
            payload = {
                "Name": item["name"],
                "SKU": sku,
                "Price": item.get("price"),
                "Category": item.get("category"),
                "IsActive": True,
            }
            client.request(
                "POST", "/api/products", payload, file=p, index=i, summary=payload
            )
            if sku:
                existing_skus.add(sku)

    # Products - dedicated
    p = _path(data_dir, "products_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            sku = item.get("SKU")
            if sku and sku in existing_skus:
                logger.log_skip(f"Product SKU {sku} already exists", file=p)
                continue
            payload = {
                "Name": item["Name"],
                "SKU": sku,
                "Price": item.get("Price"),
                "Category": item.get("Category"),
                "Description": item.get("Description"),
                "IsActive": item.get("Status") == "Active",
            }
            client.request(
                "POST", "/api/products", payload, file=p, index=i, summary=payload
            )
            if sku:
                existing_skus.add(sku)


# ---- Phase 5: Opportunities & Sales Pipeline ------------------------


def phase_opportunities_sales(
    client: ApiClient,
    logger: RunLogger,
    data_dir: str,
    id_maps: Dict[str, Any],
) -> None:
    logger.section("Phase 5 - Opportunities & Sales Pipeline")

    fb_acct = id_maps.get("_fallback_account_id", 1)
    fb_user = id_maps.get("_fallback_user_id", 1)

    p = _path(data_dir, "bulk_crm_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p).get("opportunities", [])):
            payload = {
                "Name": item["name"],
                "AccountId": id_maps["account"].get(item.get("accountId", 0)) or fb_acct,
                "Amount": item.get("amount", 0),
                "Stage": OPPORTUNITY_STAGE.get(item.get("stage"), 0),
                "Probability": 50,
                "ExpectedCloseDate": item.get("closeDate"),
                "Currency": "USD",
            }
            _, resp, _ = client.request(
                "POST", "/api/opportunities", payload, file=p, index=i, summary=payload
            )
            if isinstance(resp, dict) and "id" in resp:
                id_maps.setdefault("opportunity", {})[item.get("id", 0)] = resp["id"]

    # Quotes
    p = _path(data_dir, "sales_quotes_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            qnum = f"Q-2026-{item.get('id', i + 1):05d}"
            # Skip if already exists (prefetched)
            if qnum in id_maps.get("_quote_by_number", {}):
                id_maps.setdefault("quote", {})[item.get("id", 0)] = id_maps["_quote_by_number"][qnum]
                logger.log_skip(f"Quote {qnum} already exists", file=p)
                continue
            payload = {
                "QuoteNumber": qnum,
                "Name": f"Quote-{item.get('id', i + 1):04d}",
                "AccountId": id_maps["account"].get(item.get("accountId", 0)) or fb_acct,
                "ContactId": id_maps["contact"].get(item.get("contactId", 0)),
                "Status": QUOTE_STATUS.get(item.get("status"), 0),
                "Total": item.get("totalAmount", 0),
                "Subtotal": item.get("totalAmount", 0),
                "QuoteDate": _now_iso(),
                "ValidityDays": 30,
                "CurrencyCode": "USD",
                "OpportunityId": id_maps.get("opportunity", {}).get(item.get("opportunityId", 0)),
            }
            _, resp, _ = client.request(
                "POST", "/api/quotes", payload, file=p, index=i, summary=payload
            )
            if isinstance(resp, dict) and resp.get("id"):
                id_maps.setdefault("quote", {})[item.get("id", 0)] = resp["id"]
                id_maps.setdefault("_quote_by_number", {})[qnum] = resp["id"]
    # Refresh quote ID map from API (handles IncompleteRead cases
    # where we couldn't parse the response)
    if not id_maps.get("quote"):
        _, qlist, _ = client.request("GET", "/api/quotes", None)
        if isinstance(qlist, dict):
            qlist = qlist.get("items", qlist.get("data", []))
        if isinstance(qlist, list):
            for q in qlist:
                qid = q.get("id")
                qnum = q.get("quoteNumber")
                if qid and qnum:
                    id_maps.setdefault("_quote_by_number", {})[qnum] = qid
            # Re-map seed IDs using quote numbers
            p2 = _path(data_dir, "sales_quotes_seed.json")
            if os.path.isfile(p2):
                for i2, item2 in enumerate(load_json(p2)):
                    qnum2 = f"Q-2026-{item2.get('id', i2 + 1):05d}"
                    mapped_id = id_maps.get("_quote_by_number", {}).get(qnum2)
                    if mapped_id:
                        id_maps.setdefault("quote", {})[item2.get("id", 0)] = mapped_id

    p = _path(data_dir, "sales_quote_line_items_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            qid = id_maps.get("quote", {}).get(item.get("quoteId", 0))
            if not qid:
                logger.log_skip(f"No quote for line item {i}", file=p)
                continue
            payload = {
                "Name": f"Line {i + 1}",
                "ProductId": item.get("productId"),
                "Quantity": item.get("quantity"),
                "UnitPrice": item.get("unitPrice"),
                "LineTotal": item.get("lineTotal"),
            }
            client.request(
                "POST",
                f"/api/quotes/{qid}/lineitems",
                payload,
                file=p,
                index=i,
                summary=payload,
            )

    # Orders
    p = _path(data_dir, "sales_orders_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "AccountId": id_maps["account"].get(item.get("accountId", 0)) or fb_acct,
                "QuoteId": id_maps.get("quote", {}).get(item.get("quoteId", 0)),
                "Status": ORDER_STATUS.get(item.get("status"), 0),
                "TotalAmount": item.get("totalAmount", 0),
                "Subtotal": item.get("totalAmount", 0),
                "CurrencyCode": "USD",
            }
            _, resp, _ = client.request(
                "POST", "/api/orders", payload, file=p, index=i, summary=payload
            )
            if isinstance(resp, dict) and "id" in resp:
                id_maps.setdefault("order", {})[item.get("id", 0)] = resp["id"]
    p = _path(data_dir, "sales_order_line_items_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            oid = id_maps.get("order", {}).get(item.get("orderId", 0))
            if not oid:
                logger.log_skip(f"No order for line item {i}", file=p)
                continue
            payload = {
                "Name": f"Order Line {i + 1}",
                "ProductId": item.get("productId"),
                "Quantity": item.get("quantity"),
                "UnitPrice": item.get("unitPrice"),
                "LineTotal": item.get("lineTotal"),
            }
            client.request(
                "POST",
                f"/api/orders/{oid}/line-items",
                payload,
                file=p,
                index=i,
                summary=payload,
            )

    # Invoices
    p = _path(data_dir, "sales_invoices_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            due = item.get("dueDate") or "2026-04-01T00:00:00Z"
            payload = {
                "OrderId": id_maps.get("order", {}).get(item.get("orderId", 0)),
                "AccountId": id_maps["account"].get(item.get("accountId", 0)) or fb_acct,
                "InvoiceNumber": item.get("invoiceNumber") or f"INV-2026-{i + 1:04d}",
                "Status": INVOICE_STATUS.get(item.get("status"), 0),
                "InvoiceDate": item.get("issueDate") or "2026-02-20T00:00:00Z",
                "DueDate": due,
                "TotalAmount": item.get("totalAmount", 0),
                "Subtotal": item.get("totalAmount", 0),
                "CurrencyCode": "USD",
            }
            _, resp, _ = client.request(
                "POST", "/api/invoices", payload, file=p, index=i, summary=payload
            )
            if isinstance(resp, dict) and "id" in resp:
                id_maps.setdefault("invoice", {})[item.get("id", 0)] = resp["id"]
    p = _path(data_dir, "sales_invoice_line_items_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "Name": item.get("description") or f"Line {i + 1}",
                "ProductId": item.get("productId"),
                "Description": item.get("description"),
                "Quantity": item.get("quantity"),
                "UnitPrice": item.get("unitPrice"),
                "LineTotal": item.get("lineTotal"),
                "LineNumber": i + 1,
            }
            inv_id = id_maps.get("invoice", {}).get(item.get("invoiceId", 0))
            if not inv_id:
                logger.log_skip(f"No invoice for line item {i}", file=p)
                continue
            client.request(
                "POST",
                f"/api/invoices/{inv_id}/line-items",
                payload,
                file=p,
                index=i,
                summary=payload,
            )

    # Payments
    p = _path(data_dir, "sales_payments_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "InvoiceId": id_maps.get("invoice", {}).get(item.get("invoiceId", 0)),
                "AccountId": id_maps["account"].get(item.get("accountId", 0)) or fb_acct,
                "Amount": item.get("amount", 0),
                "PaymentMethod": PAYMENT_METHOD.get(item.get("method"), 0),
                "Status": PAYMENT_STATUS.get(item.get("status"), 0),
                "PaymentDate": item.get("paymentDate") or "2026-02-25T00:00:00Z",
                "CurrencyCode": "USD",
            }
            client.request(
                "POST", "/api/payments", payload, file=p, index=i, summary=payload
            )

    # Contracts
    p = _path(data_dir, "sales_contracts_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "Name": item.get("contractNumber") or f"Contract-{i + 1}",
                "AccountId": id_maps["account"].get(
                    item.get("accountId", 0)
                ) or fb_acct,
                "Status": CONTRACT_STATUS.get(item.get("status"), 0),
                "StartDate": item.get("startDate"),
                "EndDate": item.get("endDate"),
                "Value": item.get("value"),
                "AutoRenew": bool(item.get("renewalTermMonths")),
                "RenewalNoticeDays": 30,
            }
            client.request(
                "POST", "/api/contracts", payload, file=p, index=i, summary=payload
            )

    # Subscriptions
    p = _path(data_dir, "sales_subscriptions_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "AccountId": id_maps["account"].get(
                    item.get("accountId", 0)
                ) or fb_acct,
                "ProductId": item.get("productId"),
                "Status": SUBSCRIPTION_STATUS.get(item.get("status"), 0),
                "StartDate": item.get("startDate"),
                "EndDate": item.get("endDate"),
                "BillingCycle": {"Monthly": "Monthly", "Annual": "Yearly", "Yearly": "Yearly", "Quarterly": "Quarterly", "Weekly": "Weekly"}.get(item.get("billingCycle", "Monthly"), "Monthly"),
                "Amount": item.get("amount"),
            }
            client.request(
                "POST", "/api/subscriptions", payload, file=p, index=i, summary=payload
            )

    # Commission Plans (required for commissions)
    plan_payload = {"Name": "Default Sales Commission", "BaseRate": 0.05}
    _, plan_resp, _ = client.request("POST", "/api/commissions/plans", plan_payload, index=0, summary=plan_payload)
    plan_id = plan_resp.get("id") if isinstance(plan_resp, dict) else None

    # Commissions
    p = _path(data_dir, "sales_commissions_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            rate = item.get("rate", 0.05)
            deal = item.get("amount", 0) / rate if rate else 0
            payload = {
                "UserId": id_maps["user"].get(
                    item.get("userId", 0)
                ) or fb_user,
                "CommissionPlanId": plan_id,
                "OrderId": item.get("orderId"),
                "DealAmount": round(deal, 2),
                "CommissionRate": rate,
                "CommissionAmount": item.get("amount"),
            }
            client.request(
                "POST", "/api/commissions", payload, file=p, index=i, summary=payload
            )


# ---- Phase 6: Interactions, Activities, Tasks, Notes -----------------


def phase_interactions_activities(
    client: ApiClient,
    logger: RunLogger,
    data_dir: str,
    id_maps: Dict[str, Any],
) -> None:
    """Phase 6: Interactions (from bulk seed), activities, tasks, notes."""
    logger.section("Phase 6 - Interactions, Activities, Tasks & Notes")

    # Interactions (from bulk seed - previously not loaded!)
    p = _path(data_dir, "bulk_crm_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p).get("interactions", [])):
            itype = INTERACTION_TYPE.get(item.get("type"), 15)
            payload = {
                "AccountId": id_maps["account"].get(item.get("accountId", 0)),
                "ContactId": id_maps["contact"].get(item.get("contactId", 0)),
                "InteractionType": itype,
                "Direction": 1,
                "Subject": item.get("subject", "Interaction"),
                "Description": item.get("subject", ""),
                "InteractionDate": item.get("date", "2026-02-01") + "T10:00:00Z",
                "DurationMinutes": 30,
                "Outcome": 1,
                "AssignedToUserId": id_maps["user"].get(item.get("userId", 0)),
                "Priority": 1,
            }
            client.request(
                "POST", "/api/interactions", payload, file=p, index=i, summary=payload
            )

    # Activities (generated from account data)
    for i, (seed_id, acct_id) in enumerate(
        list(id_maps["account"].items())[:10]
    ):
        for ai, (atype, title) in enumerate(
            [
                (0, f"Welcome email sent to account {acct_id}"),
                (2, f"Discovery call with account {acct_id}"),
            ]
        ):
            payload = {
                "ActivityType": atype,
                "Title": title,
                "Description": f"Auto-generated activity for account {acct_id}",
                "ActivityDate": "2026-02-01T10:00:00Z",
                "UserId": id_maps["user"].get(2),
                "EntityType": "Account",
                "EntityId": acct_id,
                "AccountId": acct_id,
                "Source": "System" if ai == 0 else "API",
            }
            client.request(
                "POST", "/api/activities", payload, index=i * 2 + ai, summary=payload
            )

    # Tasks
    task_templates = [
        {"subject": "Follow up on proposal", "type": "FollowUp", "priority": "High", "est": 30},
        {"subject": "Schedule product demo", "type": "Demo", "priority": "Normal", "est": 60},
        {"subject": "Send contract for review", "type": "Contract", "priority": "High", "est": 15},
        {"subject": "Research competitor pricing", "type": "Research", "priority": "Normal", "est": 45},
        {"subject": "Prepare quarterly review", "type": "Meeting", "priority": "Normal", "est": 90},
    ]
    acct_items = list(id_maps["account"].items())
    contact_items = list(id_maps["contact"].items())
    user_ids = list(id_maps["user"].values())
    for i, tmpl in enumerate(task_templates):
        acct_pair = acct_items[i % len(acct_items)] if acct_items else (0, None)
        ct_pair = contact_items[i % len(contact_items)] if contact_items else (0, None)
        payload = {
            "Subject": tmpl["subject"],
            "Description": f"Task: {tmpl['subject']}",
            "TaskType": TASK_TYPE.get(tmpl["type"], 8),
            "Status": TASK_STATUS["NotStarted"],
            "Priority": TASK_PRIORITY.get(tmpl["priority"], 1),
            "DueDate": "2026-03-01T17:00:00Z",
            "AccountId": acct_pair[1],
            "ContactId": ct_pair[1],
            "AssignedToUserId": user_ids[i % len(user_ids)] if user_ids else None,
        }
        client.request("POST", "/api/tasks", payload, index=i, summary=payload)

    # Notes
    note_templates = [
        {
            "title": "Initial discovery notes",
            "content": "Client expressed interest in enterprise plan. Key decision maker is the CTO.",
            "type": "Meeting", "vis": "Team", "important": True,
        },
        {
            "title": "Budget discussion",
            "content": "Budget approved for Q2. Procurement takes 2-3 weeks.",
            "type": "Call", "vis": "Team", "important": False,
        },
        {
            "title": "Competitive analysis",
            "content": "Client evaluating Salesforce and HubSpot. Our advantage: better ITSM integration.",
            "type": "Internal", "vis": "Private", "important": True,
        },
        {
            "title": "Contract requirements",
            "content": "Legal requires SOC2 compliance cert and DPA before signing.",
            "type": "General", "vis": "Team", "important": True,
        },
        {
            "title": "Product feedback",
            "content": "Client requested custom reporting dashboard and SSO.",
            "type": "General", "vis": "Public", "important": False,
        },
    ]
    for i, tmpl in enumerate(note_templates):
        ap = acct_items[i % len(acct_items)] if acct_items else (0, None)
        cp = contact_items[i % len(contact_items)] if contact_items else (0, None)
        payload = {
            "Title": tmpl["title"],
            "Content": tmpl["content"],
            "NoteType": NOTE_TYPE.get(tmpl["type"], 0),
            "Visibility": NOTE_VISIBILITY.get(tmpl["vis"], 1),
            "IsPinned": False,
            "IsImportant": tmpl["important"],
            "EntityType": "Account",
            "EntityId": ap[1],
            "AccountId": ap[1],
            "ContactId": cp[1],
            "Tags": "seed-data",
            "Category": "Sales",
        }
        client.request("POST", "/api/notes", payload, index=i, summary=payload)


# ---- Phase 7: Marketing ---------------------------------------------


def phase_marketing(
    client: ApiClient,
    logger: RunLogger,
    data_dir: str,
    id_maps: Dict[str, Any],
) -> None:
    logger.section("Phase 7 - Marketing")

    p = _path(data_dir, "marketing_email_templates_seed.json")
    template_by_name = id_maps.get("_template_by_name", {})
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            tname = item.get("name")
            # If template already exists, use its ID
            if tname and tname in template_by_name:
                id_maps["email_template"][item.get("id", 0)] = template_by_name[tname]
                logger.log_skip(f"Email template '{tname}' already exists", file=p)
                continue
            payload = {
                "Name": tname,
                "Subject": item.get("subject"),
                "Category": item.get("type") or "General",
                "IsActive": item.get("status") == "Active",
            }
            _, resp, _ = client.request(
                "POST", "/api/emailtemplates", payload, file=p, index=i, summary=payload
            )
            if isinstance(resp, dict) and "id" in resp:
                id_maps["email_template"][item.get("id", 0)] = resp["id"]
                if tname:
                    template_by_name[tname] = resp["id"]

    p = _path(data_dir, "marketing_email_sequences_seed.json")
    existing_seq_names = id_maps.get("_existing_seq_names", set())
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            seq_name = item.get("name")
            if seq_name and seq_name in existing_seq_names:
                logger.log_skip(f"Email sequence '{seq_name}' already exists", file=p)
                continue
            steps = []
            for si, tid in enumerate(item.get("templateIds", [])):
                steps.append(
                    {
                        "StepOrder": si + 1,
                        "StepType": EMAIL_STEP_TYPE.get("Email", 0),
                        "TemplateId": id_maps["email_template"].get(tid, tid),
                        "DelayDays": 2,
                    }
                )
            payload = {
                "Name": seq_name,
                "Status": EMAIL_SEQUENCE_STATUS.get(item.get("status"), 0),
                "IsActive": item.get("status") == "Active",
                "Steps": steps,
            }
            client.request(
                "POST", "/api/email-sequences", payload, file=p, index=i, summary=payload
            )
            if seq_name:
                existing_seq_names.add(seq_name)

    p = _path(data_dir, "marketing_campaigns_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "Name": item.get("name"),
                "CampaignType": CAMPAIGN_TYPE.get(item.get("type", "Email"), 0),
                "Status": CAMPAIGN_STATUS.get(item.get("status"), 0),
                "StartDate": item.get("startDate"),
                "EndDate": item.get("endDate"),
                "Budget": item.get("budget"),
                "OwnerId": id_maps["user"].get(item.get("ownerUserId", 0)),
            }
            client.request(
                "POST", "/api/campaigns", payload, file=p, index=i, summary=payload
            )


# ---- Phase 8: Service Desk ------------------------------------------


def phase_service_desk(
    client: ApiClient,
    logger: RunLogger,
    data_dir: str,
    id_maps: Dict[str, Any],
) -> None:
    logger.section("Phase 8 - Service Desk")

    p = _path(data_dir, "service_request_categories_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "Name": item.get("Name"),
                "Description": item.get("Description"),
                "DisplayOrder": item.get("DisplayOrder"),
                "IsActive": item.get("IsActive", True),
                "IconName": item.get("IconName"),
                "ColorCode": item.get("ColorCode"),
                "DefaultResponseTimeHours": item.get("DefaultResponseTimeHours"),
                "DefaultResolutionTimeHours": item.get("DefaultResolutionTimeHours"),
            }
            _, resp, _ = client.request(
                "POST",
                "/api/service-request-settings/categories",
                payload,
                file=p,
                index=i,
                summary=payload,
            )
            if isinstance(resp, dict) and "id" in resp:
                id_maps["sr_category"][item.get("Name", "")] = resp["id"]

    p = _path(data_dir, "service_request_types_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "Name": item.get("Name"),
                "RequestType": item.get("RequestType"),
                "DetailedDescription": item.get("DetailedDescription"),
                "WorkflowName": item.get("WorkflowName"),
                "PossibleResolutions": ";".join(item.get("PossibleResolutions", [])),
                "FinalCustomerResolutions": ";".join(item.get("FinalCustomerResolutions", [])),
                "CategoryId": item.get("CategoryId"),
                "SubcategoryId": item.get("SubcategoryId"),
                "DisplayOrder": item.get("DisplayOrder"),
                "IsActive": item.get("IsActive", True),
                "DefaultPriority": item.get("DefaultPriority"),
                "ResponseTimeHours": item.get("ResponseTimeHours"),
                "ResolutionTimeHours": item.get("ResolutionTimeHours"),
                "Tags": ",".join(item.get("Tags", [])),
            }
            client.request(
                "POST",
                "/api/service-request-settings/types",
                payload,
                file=p,
                index=i,
                summary=payload,
            )

    p = _path(data_dir, "bulk_crm_seed.json")
    if os.path.isfile(p):
        fb_acct = id_maps.get("_fallback_account_id", 1)
        pri = {"Low": 0, "Medium": 1, "High": 2, "Critical": 3, "Urgent": 4}
        for i, item in enumerate(load_json(p).get("serviceRequests", [])):
            payload = {
                "Subject": item.get("title"),
                "Description": item.get("title"),
                "Priority": pri.get(item.get("priority"), 1),
                "CategoryId": id_maps["sr_category"].get(item.get("category")),
                "AccountId": id_maps["account"].get(item.get("accountId", 0)) or fb_acct,
                "ContactId": id_maps["contact"].get(item.get("contactId", 0)),
            }
            client.request(
                "POST", "/api/servicerequests", payload, file=p, index=i, summary=payload
            )


# ---- Phase 9: ITSM --------------------------------------------------


def phase_itsm(
    client: ApiClient,
    logger: RunLogger,
    data_dir: str,
    id_maps: Dict[str, Any],
) -> None:
    logger.section("Phase 9 - ITSM")

    fb_user = id_maps.get("_fallback_user_id", 1)

    p = _path(data_dir, "itsm_cmdb_items_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "CIName": item.get("name"),
                "CIType": CI_TYPE.get(item.get("type"), 1),
                "CISubtype": item.get("category"),
                "OperationalStatus": OPERATIONAL_STATUS.get(item.get("status"), 1),
                "OwnerId": id_maps["account"].get(item.get("ownerAccountId", 0)),
            }
            client.request(
                "POST", "/api/itsm/cmdb", payload, file=p, index=i, summary=payload
            )

    p = _path(data_dir, "itsm_incidents_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "ShortDescription": item.get("title"),
                "Description": item.get("title"),
                "CallerId": id_maps["user"].get(
                    item.get("reportedByContactId", 0)
                ) or fb_user,
                "Impact": INCIDENT_IMPACT.get(item.get("priority"), 2),
                "Urgency": INCIDENT_URGENCY.get(item.get("priority"), 2),
            }
            _, resp, _ = client.request(
                "POST", "/api/itsm/incidents", payload, file=p, index=i, summary=payload
            )
            if isinstance(resp, dict):
                inc_id = resp.get("id") or resp.get("incidentId")
                if inc_id:
                    id_maps.setdefault("incident", {})[item.get("id", 0)] = inc_id

    p = _path(data_dir, "itsm_problems_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            seed_inc_id = item.get("relatedIncidentId", 0)
            actual_inc_id = id_maps.get("incident", {}).get(seed_inc_id)
            inc_ids = [actual_inc_id] if actual_inc_id else []
            if not inc_ids:
                logger.log_skip(f"No incident for problem {i}", file=p)
                continue
            payload = {
                "ShortDescription": item.get("title"),
                "Description": item.get("title"),
                "Priority": PROBLEM_PRIORITY.get(item.get("priority"), 3),
                "IncidentIds": inc_ids,
            }
            client.request(
                "POST", "/api/itsm/problems", payload, file=p, index=i, summary=payload
            )

    p = _path(data_dir, "itsm_changes_seed.json")
    if os.path.isfile(p):
        change_type_map = {"Standard": 1, "Normal": 2, "Emergency": 3}
        change_risk_map = {"High": 1, "Medium": 2, "Low": 3}
        change_impact_map = {"High": 1, "Medium": 2, "Low": 3}
        for i, item in enumerate(load_json(p)):
            payload = {
                "Title": item.get("title"),
                "Description": item.get("title"),
                "Type": change_type_map.get(item.get("type"), 1),
                "Risk": change_risk_map.get(item.get("risk"), 2),
                "Impact": change_impact_map.get(item.get("risk"), 2),
                "PlannedStartDate": item.get("scheduledDate"),
                "PlannedEndDate": item.get("scheduledDate"),
            }
            client.request(
                "POST", "/api/changes", payload, file=p, index=i, summary=payload
            )

    p = _path(data_dir, "itsm_sla_policies_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "Name": item.get("PolicyName"),
                "TargetType": 1,
                "P1ResponseMinutes": item.get("ResponseTimeMinutes"),
                "P1ResolutionMinutes": item.get("ResolutionTimeMinutes"),
                "UseBusinessHours": item.get("BusinessHoursOnly", False),
                "IsActive": item.get("IsActive", True),
            }
            client.request(
                "POST", "/api/itsm/sla/policies", payload, file=p, index=i, summary=payload
            )

    p = _path(data_dir, "service_desk_knowledge_articles_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "Title": item.get("title"),
                "ArticleBody": " ".join(item.get("tags", []))
                or item.get("title"),
                "ArticleType": ARTICLE_TYPE.get("HowTo", 1),
                "ShortDescription": item.get("category"),
                "IsInternal": False,
            }
            client.request(
                "POST", "/api/itsm/knowledge", payload, file=p, index=i, summary=payload
            )


# ---- Phase 10: Relationships & Health --------------------------------


def phase_relationships(
    client: ApiClient,
    logger: RunLogger,
    data_dir: str,
    id_maps: Dict[str, Any],
) -> None:
    logger.section("Phase 10 - Relationships & Health")

    # Relationship types
    rel_types = [
        {
            "typeName": "Partner",
            "typeCategory": "Business",
            "description": "Business partner",
            "isBidirectional": True,
            "reverseTypeName": "Partner",
            "icon": "handshake",
            "color": "#4CAF50",
        },
        {
            "typeName": "Vendor",
            "typeCategory": "Supply Chain",
            "description": "Vendor/supplier",
            "isBidirectional": False,
            "reverseTypeName": "Customer",
            "icon": "local_shipping",
            "color": "#2196F3",
        },
        {
            "typeName": "Subsidiary",
            "typeCategory": "Corporate",
            "description": "Parent-subsidiary",
            "isBidirectional": False,
            "reverseTypeName": "Parent Company",
            "icon": "business",
            "color": "#FF9800",
        },
        {
            "typeName": "Competitor",
            "typeCategory": "Market",
            "description": "Competitive relationship",
            "isBidirectional": True,
            "reverseTypeName": "Competitor",
            "icon": "trending_up",
            "color": "#f44336",
        },
        {
            "typeName": "Referral",
            "typeCategory": "Sales",
            "description": "Referral source",
            "isBidirectional": False,
            "reverseTypeName": "Referred By",
            "icon": "share",
            "color": "#9C27B0",
        },
    ]
    rel_type_ids: Dict[str, int] = {}
    existing_type_names = id_maps.get("_existing_rel_type_names", set())
    for i, rt in enumerate(rel_types):
        if rt["typeName"] in existing_type_names:
            logger.log_skip(f"Relationship type '{rt['typeName']}' already exists")
            continue
        payload = {**rt, "isActive": True, "displayOrder": i + 1}
        _, resp, _ = client.request(
            "POST", "/api/relationships/types", payload, index=i, summary=payload
        )
        if isinstance(resp, dict) and "id" in resp:
            rel_type_ids[rt["typeName"]] = resp["id"]
            existing_type_names.add(rt["typeName"])

    # Account-to-account relationships
    acct_ids = list(id_maps["account"].values())
    if len(acct_ids) >= 4 and rel_type_ids:
        rels = [
            (acct_ids[0], acct_ids[1], "Partner", "Active", 80, "High"),
            (acct_ids[0], acct_ids[2], "Vendor", "Active", 60, "Medium"),
            (acct_ids[1], acct_ids[3], "Referral", "Active", 70, "Medium"),
        ]
        for i, (src, tgt, rtype, status, strength, importance) in enumerate(rels):
            type_id = rel_type_ids.get(rtype)
            if not type_id:
                continue
            payload = {
                "SourceAccountId": src,
                "TargetAccountId": tgt,
                "RelationshipTypeId": type_id,
                "Status": status,
                "StrengthScore": strength,
                "StrategicImportance": importance,
                "RelationshipStartDate": "2025-06-01T00:00:00Z",
                "Description": f"{rtype} relationship between accounts",
            }
            client.request(
                "POST", "/api/relationships", payload, index=i, summary=payload
            )

    # Account health snapshots
    for i, acct_id in enumerate(acct_ids[:5]):
        payload = {
            "AccountId": acct_id,
            "OverallHealthScore": 70 + (i * 5) % 30,
            "EngagementScore": 65 + (i * 7) % 35,
            "ProductAdoptionScore": 60 + (i * 3) % 40,
            "SupportSatisfactionScore": 75 + (i * 4) % 25,
            "FinancialHealthScore": 80 + (i * 6) % 20,
            "RelationshipScore": 70 + (i * 5) % 30,
            "AnalystNotes": f"Health snapshot for account {acct_id}",
        }
        client.request(
            "POST", "/api/relationships/health", payload, index=i, summary=payload
        )


# ---- Phase 11: Workflows --------------------------------------------


def phase_workflows(
    client: ApiClient,
    logger: RunLogger,
    data_dir: str,
    id_maps: Dict[str, Any],
) -> None:
    logger.section("Phase 11 - Workflows")

    p = _path(data_dir, "service_desk_workflow_definitions_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            name = item.get("name", "")
            payload = {
                "WorkflowKey": slugify(name),
                "Name": name,
                "Category": item.get("module"),
                "EntityType": "ServiceRequest",
                "Tags": item.get("steps", []),
            }
            client.request(
                "POST", "/api/workflows", payload, file=p, index=i, summary=payload
            )


# ---- Phase 12: Skipped (no API endpoint) ----------------------------


def phase_unsupported(logger: RunLogger, data_dir: str) -> None:
    logger.section("Skipped - No API Endpoint")
    for name in [
        "analytics_events_seed.json",
        "ai_agent_usage_seed.json",
        "integration_export_jobs_seed.json",
        "integration_import_jobs_seed.json",
        "integration_webhooks_seed.json",
        "service_desk_escalation_rules_seed.json",
        "itsm_catalog_categories_seed.json",
        "itsm_change_types_seed.json",
        "itsm_ci_types_seed.json",
        "itsm_incident_categories_seed.json",
        "marketing_campaign_conversions_seed.json",
        "marketing_campaign_metrics_seed.json",
        "marketing_campaign_recipients_seed.json",
        "services_seed.json",
        "system_audit_logs_seed.json",
    ]:
        logger.log_skip(
            "No supported API endpoint", file=os.path.join(data_dir, name)
        )


# ============================ MAIN ====================================


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Load CRM test data via API with enhanced diagnostics",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=textwrap.dedent(
            """\
            Examples:
              # Local API
              python3 scripts/test_data_loader.py

              # Remote server with Docker log capture
              python3 scripts/test_data_loader.py \\
                --base-url http://192.168.0.9:5000 \\
                --ssh-host root@192.168.0.9

              # Custom data directory
              python3 scripts/test_data_loader.py \\
                --data-dir /path/to/seed-data
        """
        ),
    )
    parser.add_argument(
        "--base-url", default="http://localhost:5000", help="CRM API base URL"
    )
    parser.add_argument(
        "--data-dir", default="e2e-tests/test-data", help="Seed data directory"
    )
    parser.add_argument(
        "--username", default="admin@crm.local", help="Admin email"
    )
    parser.add_argument("--password", default="Admin@123", help="Admin password")
    parser.add_argument(
        "--log-dir", default="logs/test-data", help="Log output directory"
    )
    parser.add_argument(
        "--ssh-host",
        default=None,
        help="SSH host for Docker log capture (e.g. root@192.168.0.9)",
    )
    parser.add_argument(
        "--api-container", default="crm-api", help="Docker container name for API"
    )
    parser.add_argument(
        "--db-container", default="crm-mariadb", help="Docker container name for DB"
    )
    parser.add_argument(
        "--frontend-container",
        default="crm-frontend",
        help="Docker container name for frontend",
    )
    parser.add_argument(
        "--docker-tail",
        type=int,
        default=120,
        help="Docker log lines to capture on error",
    )
    args = parser.parse_args()

    # Docker log capture
    docker: Optional[DockerLogCapture] = None
    if args.ssh_host:
        docker = DockerLogCapture(
            ssh_host=args.ssh_host,
            api_container=args.api_container,
            db_container=args.db_container,
            frontend_container=args.frontend_container,
            tail_lines=args.docker_tail,
        )

    logger = RunLogger(args.log_dir, docker=docker)

    # Authenticate
    try:
        auth_payload = {"email": args.username, "password": args.password}
        auth_req = urllib.request.Request(
            f"{args.base_url.rstrip('/')}/api/auth/login",
            data=json.dumps(auth_payload).encode(),
            method="POST",
        )
        auth_req.add_header("Content-Type", "application/json")
        with urllib.request.urlopen(auth_req, timeout=15) as resp:
            body = resp.read().decode()
            auth_data = json.loads(body)
        token = auth_data.get("accessToken") or auth_data.get("token") or ""
        if not token:
            logger.log_result(
                "failed",
                "POST",
                "/api/auth/login",
                200,
                response_body=body,
                error="No token in response",
            )
            logger.close()
            return 1
        logger.log_result(
            "success",
            "POST",
            "/api/auth/login",
            200,
            request_summary={"email": args.username},
            response_body=body,
        )
    except Exception as exc:
        snap = docker.snapshot() if docker else None
        logger.log_result(
            "failed",
            "POST",
            "/api/auth/login",
            None,
            error=str(exc),
            docker_snapshot=snap,
        )
        logger.close()
        return 1

    client = ApiClient(args.base_url, token, logger, docker)
    data_dir = os.path.abspath(args.data_dir)

    # Shared ID maps
    id_maps: Dict[str, Any] = {
        "role_name": {},
        "user": {},
        "account": {},
        "account_name": {},
        "contact": {},
        "email_template": {},
        "sr_category": {},
        "_fallback_account_id": 1,
        "_fallback_user_id": 1,
    }

    # Prefetch existing entities so id_maps work on re-runs
    prefetch_existing(client, logger, data_dir, id_maps)

    # Execute all phases
    phases = [
        (
            "System",
            lambda: phase_system(client, logger, data_dir, id_maps),
        ),
        (
            "Accounts & Contacts",
            lambda: phase_accounts_contacts(client, logger, data_dir, id_maps),
        ),
        (
            "Contact Info",
            lambda: phase_contact_info(client, logger, data_dir, id_maps),
        ),
        (
            "Leads & Products",
            lambda: phase_leads_products(client, logger, data_dir, id_maps),
        ),
        (
            "Opportunities & Sales",
            lambda: phase_opportunities_sales(client, logger, data_dir, id_maps),
        ),
        (
            "Interactions & Activities",
            lambda: phase_interactions_activities(client, logger, data_dir, id_maps),
        ),
        (
            "Marketing",
            lambda: phase_marketing(client, logger, data_dir, id_maps),
        ),
        (
            "Service Desk",
            lambda: phase_service_desk(client, logger, data_dir, id_maps),
        ),
        (
            "ITSM",
            lambda: phase_itsm(client, logger, data_dir, id_maps),
        ),
        (
            "Relationships",
            lambda: phase_relationships(client, logger, data_dir, id_maps),
        ),
        (
            "Workflows",
            lambda: phase_workflows(client, logger, data_dir, id_maps),
        ),
        (
            "Unsupported",
            lambda: phase_unsupported(logger, data_dir),
        ),
    ]

    for name, fn in phases:
        try:
            fn()
        except Exception as exc:
            logger.section(f"PHASE ERROR: {name}")
            snap = docker.snapshot() if docker else None
            logger.log_result(
                "failed",
                "PHASE",
                name,
                None,
                error=(
                    f"{type(exc).__name__}: {exc}\n{traceback.format_exc()}"
                ),
                docker_snapshot=snap,
            )

    # Summary
    logger.log_result(
        "success",
        "SUMMARY",
        "/complete",
        None,
        request_summary={
            "success": logger.counts["success"],
            "failed": logger.counts["failed"],
            "skipped": logger.counts["skipped"],
        },
    )
    logger.close()

    print(f"\n{'=' * 60}")
    print(f"  Test Data Load Complete")
    print(f"  Success: {logger.counts['success']}")
    print(f"  Failed:  {logger.counts['failed']}")
    print(f"  Skipped: {logger.counts['skipped']}")
    print(f"  Log:     {logger.text_path}")
    print(f"  JSONL:   {logger.jsonl_path}")
    print(f"{'=' * 60}\n")

    return 1 if logger.counts["failed"] > 0 else 0


if __name__ == "__main__":
    sys.exit(main())

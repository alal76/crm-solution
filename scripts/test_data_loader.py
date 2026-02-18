#!/usr/bin/env python3
"""CRM Test-Data Loader v3

Loads, edits, links/unlinks, and deletes test data via the CRM REST API.

Creation Phases (1-11):
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

Mutation & Verification Phases (12-15):
  * Edit (PUT/PATCH): update accounts, contacts, leads, opportunities,
    products, tasks, notes, quotes, orders, service requests, subscriptions
  * Link & Unlink: account-contact, direct-contacts, teams (members,
    accounts, manager), role-permissions, ITSM incident lifecycle
    (assign/escalate/resolve/reopen), contract/subscription/quote lifecycle
  * Delete: create-then-delete for 13 entity types to verify DELETE
    endpoints without breaking cross-references
  * Verification: count checks on all major endpoints, spot-check specific
    IDs, verify edits persisted, verify deletes hidden, health endpoints

Phase 16: Unsupported seed files (no API endpoint)

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
    "duplicate entry", "unique constraint", "already assigned",
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
        exists = self.counts.get("exists", 0)
        line = (
            f"\n=== FINAL: success={self.counts['success']}  "
            f"exists={exists}  "
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

    def log_exists_skip(self, reason: str, **kw: Any) -> None:
        """Log a pre-flight 'already exists' check as 'exists' (not 'skipped')."""
        self.counts["exists"] = self.counts.get("exists", 0) + 1
        entry: Dict[str, Any] = {"status": "exists", "reason": reason}
        entry.update(kw)
        entry["summary"] = f"  EXISTS {reason} ({kw.get('file', 'n/a')})"
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
        elif status == "exists":
            entry["summary"] = (
                f"  EXISTS {method} {endpoint} ({loc}) -> {http_status}"
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
        payload: Any = None,
        *,
        file: Optional[str] = None,
        index: Optional[int] = None,
        summary: Optional[Dict[str, Any]] = None,
    ) -> Tuple[Optional[int], Optional[Dict[str, Any]], Optional[str]]:
        url = f"{self.base_url}{path}"
        data = (
            json.dumps(payload, default=str, ensure_ascii=True).encode()
            if payload is not None
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
                    "exists",
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
                    # entry, EF tracking, permission assignment, workflow
                    # creation.  Treat as non-fatal.
                    err_msg = parsed_500.get("error", "") or parsed_500.get("message", "")
                    if any(p in err_msg.lower() for p in [
                        "duplicate entry", "saving the entity",
                        "already exists", "error assigning permission",
                        "error occurred while creating the workflow",
                    ]):
                        self.logger.log_result(
                            "success",
                            method,
                            path,
                            exc.code,
                            file=file,
                            index=index,
                            request_summary=summary or _compact(payload),
                            response_body=f"[500-known-bug] {resp_body[:500]}",
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
                logger.log_exists_skip(f"Product SKU {sku} already exists", file=p)
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
                logger.log_exists_skip(f"Product SKU {sku} already exists", file=p)
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

    # Services (via Products endpoint with ProductType=Service)
    PRODUCT_TYPE = {"Physical": 0, "Digital": 1, "Service": 2, "Subscription": 3,
                    "Bundle": 4, "Rental": 5, "Consulting": 6, "ManagedService": 7,
                    "SupportContract": 8, "Training": 9, "License": 10}
    SERVICE_TIER = {"Basic": 0, "Standard": 1, "Premium": 2, "Enterprise": 3}
    p = _path(data_dir, "services_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            sku = item.get("SKU")
            if sku and sku in existing_skus:
                logger.log_exists_skip(f"Service SKU {sku} already exists", file=p)
                continue
            # Map ServiceType string to ProductType enum
            svc_type = item.get("ServiceType", "Service")
            if "Consulting" in svc_type:
                ptype = PRODUCT_TYPE["Consulting"]
            elif "Managed" in svc_type:
                ptype = PRODUCT_TYPE["ManagedService"]
            elif "Training" in svc_type:
                ptype = PRODUCT_TYPE["Training"]
            elif "Support" in svc_type:
                ptype = PRODUCT_TYPE["SupportContract"]
            else:
                ptype = PRODUCT_TYPE["Service"]
            payload = {
                "Name": item.get("Name"),
                "SKU": sku,
                "Description": item.get("Description"),
                "ShortDescription": item.get("ShortDescription"),
                "Category": item.get("Category"),
                "SubCategory": item.get("SubCategory"),
                "ProductType": ptype,
                "ServiceTier": SERVICE_TIER.get(item.get("ServiceTier", "Standard"), 1),
                "Price": item.get("Price"),
                "Cost": item.get("Cost"),
                "ListPrice": item.get("ListPrice"),
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
                logger.log_exists_skip(f"Quote {qnum} already exists", file=p)
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
                logger.log_exists_skip(f"Email template '{tname}' already exists", file=p)
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
                logger.log_exists_skip(f"Email sequence '{seq_name}' already exists", file=p)
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
            _, resp, _ = client.request(
                "POST", "/api/campaigns", payload, file=p, index=i, summary=payload
            )
            if isinstance(resp, dict) and "id" in resp:
                id_maps.setdefault("campaign", {})[item.get("id", 0)] = resp["id"]

    # Campaign Recipients
    RECIPIENT_STATUS = {"Pending": 0, "Sent": 1, "Delivered": 2, "Opened": 3,
                        "Clicked": 4, "Bounced": 5, "Unsubscribed": 6}
    p = _path(data_dir, "marketing_campaign_recipients_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "CampaignId": id_maps.get("campaign", {}).get(item.get("campaignId"), item.get("campaignId")),
                "ContactId": id_maps.get("contact", {}).get(item.get("contactId"), item.get("contactId")),
                "LeadId": id_maps.get("lead", {}).get(item.get("leadId")),
                "Email": item.get("email"),
                "Status": RECIPIENT_STATUS.get(item.get("status", "Pending"), 0),
                "SentAt": item.get("sentAt"),
                "DeliveredAt": item.get("deliveredAt"),
                "OpenedAt": item.get("openedAt"),
            }
            client.request(
                "POST", "/api/campaign-recipients", payload, file=p, index=i, summary=payload
            )

    # Campaign Conversions (link campaigns to opportunities)
    p = _path(data_dir, "marketing_campaign_conversions_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "CampaignId": id_maps.get("campaign", {}).get(item.get("campaignId"), item.get("campaignId")),
                "OpportunityId": id_maps.get("opportunity", {}).get(item.get("opportunityId"), item.get("opportunityId")),
                "LeadId": id_maps.get("lead", {}).get(item.get("leadId")),
                "ConvertedAt": item.get("conversionDate"),
                "ConversionValue": item.get("value"),  # seed data uses 'value'
                "ConversionType": item.get("conversionType", "Opportunity"),
            }
            client.request(
                "POST", "/api/campaign-conversions", payload, file=p, index=i, summary=payload
            )

    # Campaign Metrics - map seed data to CampaignMetric entity fields
    p = _path(data_dir, "marketing_campaign_metrics_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            # The seed data has: campaignId, impressions, clicks, conversions, cost
            # Map to CampaignMetric entity fields
            payload = {
                "CampaignId": id_maps.get("campaign", {}).get(item.get("campaignId"), item.get("campaignId")),
                "MetricName": "Campaign Performance",
                "MetricValue": item.get("impressions", 0),
                "RecordedDate": datetime.now().isoformat(),
                "TotalSent": item.get("impressions", 0),  # Use impressions as proxy for sent
                "TotalDelivered": int(item.get("impressions", 0) * 0.95),  # Assume 95% delivery
                "TotalOpened": int(item.get("clicks", 0) * 1.5),  # Estimate opens from clicks
                "TotalClicked": item.get("clicks", 0),
                "TotalConverted": item.get("conversions", 0),
            }
            client.request(
                "POST", "/api/campaign-metrics", payload, file=p, index=i, summary=payload
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
            logger.log_exists_skip(f"Relationship type '{rt['typeName']}' already exists")
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


# ---- Phase 12: Edit (PUT) existing entities --------------------------


def phase_edit(
    client: ApiClient,
    logger: RunLogger,
    data_dir: str,
    id_maps: Dict[str, Any],
) -> None:
    """Phase 12: Update existing entities via PUT/PATCH to verify edit
    operations work end-to-end."""
    logger.section("Phase 12 - Edit (PUT/PATCH) Operations")

    fb_acct = id_maps.get("_fallback_account_id", 1)
    fb_user = id_maps.get("_fallback_user_id", 1)

    # ---- Edit accounts ----
    acct_items = list(id_maps.get("account", {}).items())
    for i, (seed_id, acct_id) in enumerate(acct_items[:3]):
        payload = {
            "Company": f"Updated Company {acct_id}",
            "Industry": "Technology",
            "Phone": f"+1-555-{9000 + i}",
            "Tags": "edited,test-data",
        }
        status, resp, _ = client.request(
            "PUT", f"/api/accounts/{acct_id}", payload,
            index=i, summary={"edit": "account", "id": acct_id},
        )
        if status and 200 <= status < 300:
            # Verify the edit stuck
            _, get_resp, _ = client.request(
                "GET", f"/api/accounts/{acct_id}", None,
                summary={"verify_edit": "account", "id": acct_id},
            )
            if isinstance(get_resp, dict):
                actual = get_resp.get("company") or get_resp.get("name")
                expected = f"Updated Company {acct_id}"
                if actual != expected:
                    logger.log_result(
                        "failed", "VERIFY", f"/api/accounts/{acct_id}", 200,
                        error=f"Edit not persisted: expected '{expected}', got '{actual}'",
                    )

    # ---- Edit contacts ----
    contact_items = list(id_maps.get("contact", {}).items())
    for i, (seed_id, ct_id) in enumerate(contact_items[:3]):
        payload = {
            "JobTitle": f"Updated Title {ct_id}",
            "Department": "Engineering",
            "Notes": "Updated via test data loader edit phase",
        }
        status, _, _ = client.request(
            "PUT", f"/api/contacts/{ct_id}", payload,
            index=i, summary={"edit": "contact", "id": ct_id},
        )
        if status and 200 <= status < 300:
            _, get_resp, _ = client.request(
                "GET", f"/api/contacts/{ct_id}", None,
                summary={"verify_edit": "contact", "id": ct_id},
            )
            if isinstance(get_resp, dict):
                actual = get_resp.get("jobTitle")
                expected = f"Updated Title {ct_id}"
                if actual != expected:
                    logger.log_result(
                        "failed", "VERIFY", f"/api/contacts/{ct_id}", 200,
                        error=f"Edit not persisted: expected '{expected}', got '{actual}'",
                    )

    # ---- Edit leads ----
    # Fetch first 3 leads to get IDs
    _, lead_list, _ = client.request("GET", "/api/leads", None)
    if isinstance(lead_list, dict):
        lead_list = lead_list.get("items", lead_list.get("data", []))
    if isinstance(lead_list, list):
        for i, lead in enumerate(lead_list[:3]):
            lid = lead.get("id")
            if not lid:
                continue
            payload = {
                "CompanyName": f"Updated Lead Co {lid}",
                "Notes": "Updated via edit phase",
                "Score": 75 + i * 5,
            }
            client.request(
                "PUT", f"/api/leads/{lid}", payload,
                index=i, summary={"edit": "lead", "id": lid},
            )

    # ---- Edit opportunities ----
    opp_items = list(id_maps.get("opportunity", {}).items())
    for i, (seed_id, opp_id) in enumerate(opp_items[:3]):
        # Opportunities controller uses full entity body, Id overwritten
        payload = {
            "Id": opp_id,
            "Name": f"Updated Opportunity {opp_id}",
            "AccountId": fb_acct,
            "Amount": 50000 + i * 10000,
            "Stage": OPPORTUNITY_STAGE.get("Negotiation", 3),
            "Probability": 70 + i * 5,
            "Currency": "USD",
        }
        client.request(
            "PUT", f"/api/opportunities/{opp_id}", payload,
            index=i, summary={"edit": "opportunity", "id": opp_id},
        )

    # ---- Edit products ----
    _, prod_list, _ = client.request("GET", "/api/products", None)
    if isinstance(prod_list, dict):
        prod_list = prod_list.get("items", prod_list.get("data", []))
    if isinstance(prod_list, list):
        for i, prod in enumerate(prod_list[:3]):
            pid = prod.get("id")
            if not pid:
                continue
            payload = {
                "Id": pid,
                "Name": prod.get("name", f"Product {pid}") + " (Updated)",
                "SKU": prod.get("sku") or f"UPD-{pid}",
                "Price": (prod.get("price") or 100) + 10,
                "Category": prod.get("category") or "General",
                "IsActive": True,
            }
            client.request(
                "PUT", f"/api/products/{pid}", payload,
                index=i, summary={"edit": "product", "id": pid},
            )

    # ---- Edit tasks (complete one) ----
    _, task_list, _ = client.request("GET", "/api/tasks", None)
    if isinstance(task_list, dict):
        task_list = task_list.get("items", task_list.get("data", []))
    if isinstance(task_list, list):
        for i, task in enumerate(task_list[:2]):
            tid = task.get("id")
            if not tid:
                continue
            payload = {
                "Id": tid,
                "Subject": task.get("subject", "Task") + " (Edited)",
                "Description": "Updated via edit phase",
                "TaskType": task.get("taskType", 8),
                "Status": TASK_STATUS.get("InProgress", 1),
                "Priority": TASK_PRIORITY.get("High", 2),
                "DueDate": "2026-04-01T17:00:00Z",
            }
            client.request(
                "PUT", f"/api/tasks/{tid}", payload,
                index=i, summary={"edit": "task", "id": tid},
            )
        # Complete the last task if available
        if len(task_list) >= 3:
            complete_id = task_list[2].get("id")
            if complete_id:
                client.request(
                    "PUT", f"/api/tasks/{complete_id}/complete", None,
                    summary={"action": "complete_task", "id": complete_id},
                )

    # ---- Edit notes ----
    _, note_list, _ = client.request("GET", "/api/notes", None)
    if isinstance(note_list, dict):
        note_list = note_list.get("items", note_list.get("data", []))
    if isinstance(note_list, list):
        for i, note in enumerate(note_list[:2]):
            nid = note.get("id")
            if not nid:
                continue
            payload = {
                "Id": nid,
                "Title": note.get("title", "Note") + " (Edited)",
                "Content": (note.get("content") or "Content") + "\n\n--- Updated via edit phase ---",
                "NoteType": note.get("noteType", 0),
                "Visibility": note.get("visibility", 1),
                "IsPinned": note.get("isPinned", False),
                "IsImportant": True,
                "EntityType": note.get("entityType"),
                "EntityId": note.get("entityId"),
            }
            client.request(
                "PUT", f"/api/notes/{nid}", payload,
                index=i, summary={"edit": "note", "id": nid},
            )

    # ---- Edit quotes (only Draft ones) ----
    quote_items = list(id_maps.get("quote", {}).items())
    for i, (seed_id, qid) in enumerate(quote_items[:2]):
        # Fetch full quote first to get required fields
        _, q_detail, _ = client.request("GET", f"/api/quotes/{qid}", None)
        if not isinstance(q_detail, dict):
            continue
        q_status = q_detail.get("status", 0)
        # Only edit Draft/New quotes (status 0 or 1)
        if q_status not in (0, 1):
            continue
        payload = {
            "Id": qid,
            "QuoteNumber": q_detail.get("quoteNumber"),
            "Name": q_detail.get("name", f"Quote-{qid}") + " (Revised)",
            "AccountId": q_detail.get("accountId") or fb_acct,
            "Status": q_status,
            "Total": (q_detail.get("total") or 0) + 500,
            "Subtotal": (q_detail.get("subtotal") or 0) + 500,
            "ValidityDays": 60,
            "CurrencyCode": "USD",
        }
        client.request(
            "PUT", f"/api/quotes/{qid}", payload,
            index=i, summary={"edit": "quote", "id": qid},
        )

    # ---- PATCH order status ----
    order_items = list(id_maps.get("order", {}).items())
    for i, (seed_id, oid) in enumerate(order_items[:2]):
        payload = {"Status": ORDER_STATUS.get("Processing", 3)}
        client.request(
            "PATCH", f"/api/orders/{oid}/status", payload,
            index=i, summary={"patch_status": "order", "id": oid},
        )

    # ---- Edit service requests (PATCH status) ----
    # Controller expects a bare enum value, not a JSON object
    _, sr_list, _ = client.request("GET", "/api/servicerequests", None)
    if isinstance(sr_list, dict):
        sr_list = sr_list.get("items", sr_list.get("data", []))
    if isinstance(sr_list, list):
        for i, sr in enumerate(sr_list[:2]):
            srid = sr.get("id")
            if not srid:
                continue
            # ServiceRequestStatus: Open=1 (bare int, not object)
            client.request(
                "PATCH", f"/api/servicerequests/{srid}/status", 1,
                index=i, summary={"patch_status": "service_request", "id": srid, "status": 1},
            )

    # ---- Edit subscriptions ----
    _, sub_list, _ = client.request("GET", "/api/subscriptions", None)
    if isinstance(sub_list, dict):
        sub_list = sub_list.get("items", sub_list.get("data", []))
    if isinstance(sub_list, list):
        for i, sub in enumerate(sub_list[:2]):
            sid = sub.get("id")
            if not sid:
                continue
            payload = {
                "Id": sid,
                "AccountId": sub.get("accountId") or fb_acct,
                "Amount": (sub.get("amount") or 100) + 25,
                "BillingCycle": sub.get("billingCycle") or "Monthly",
                "IsAutoRenew": True,
                "Currency": "USD",
            }
            client.request(
                "PUT", f"/api/subscriptions/{sid}", payload,
                index=i, summary={"edit": "subscription", "id": sid},
            )


# ---- Phase 13: Link & Unlink relationships --------------------------


def phase_link_unlink(
    client: ApiClient,
    logger: RunLogger,
    data_dir: str,
    id_maps: Dict[str, Any],
) -> None:
    """Phase 13: Test linking and unlinking entities through relationship
    endpoints — account-contacts, teams, roles, contact info, etc."""
    logger.section("Phase 13 - Link & Unlink Operations")

    fb_user = id_maps.get("_fallback_user_id", 1)
    acct_items = list(id_maps.get("account", {}).items())
    contact_items = list(id_maps.get("contact", {}).items())
    user_ids = list(id_maps.get("user", {}).values())

    # ---- Account -> Contact: link, verify, unlink, verify ----
    if len(acct_items) >= 1 and len(contact_items) >= 2:
        test_acct_id = acct_items[-1][1]  # Use last account
        test_ct_id = contact_items[-1][1]  # Use last contact

        # Link (Role is AccountContactRole enum: Other=9)
        link_payload = {
            "ContactId": test_ct_id,
            "Role": 9,
            "IsPrimaryContact": False,
        }
        status, _, _ = client.request(
            "POST", f"/api/accounts/{test_acct_id}/contacts",
            link_payload,
            summary={"link": "account-contact", "acct": test_acct_id, "ct": test_ct_id},
        )
        # Verify link exists
        _, acct_contacts, _ = client.request(
            "GET", f"/api/accounts/{test_acct_id}/contacts", None,
            summary={"verify_link": "account-contact", "acct": test_acct_id},
        )
        linked_ids = []
        if isinstance(acct_contacts, list):
            linked_ids = [c.get("contactId") or c.get("id") for c in acct_contacts]
        elif isinstance(acct_contacts, dict):
            items = acct_contacts.get("items", acct_contacts.get("data", []))
            linked_ids = [c.get("contactId") or c.get("id") for c in items]
        if test_ct_id not in linked_ids:
            logger.log_result(
                "failed", "VERIFY", f"/api/accounts/{test_acct_id}/contacts", 200,
                error=f"Contact {test_ct_id} not found in account contacts after link",
            )

        # Unlink
        client.request(
            "DELETE", f"/api/accounts/{test_acct_id}/contacts/{test_ct_id}",
            None,
            summary={"unlink": "account-contact", "acct": test_acct_id, "ct": test_ct_id},
        )
        # Verify unlink
        _, acct_contacts2, _ = client.request(
            "GET", f"/api/accounts/{test_acct_id}/contacts", None,
            summary={"verify_unlink": "account-contact", "acct": test_acct_id},
        )
        linked_ids2 = []
        if isinstance(acct_contacts2, list):
            linked_ids2 = [c.get("contactId") or c.get("id") for c in acct_contacts2]
        elif isinstance(acct_contacts2, dict):
            items2 = acct_contacts2.get("items", acct_contacts2.get("data", []))
            linked_ids2 = [c.get("contactId") or c.get("id") for c in items2]
        if test_ct_id in linked_ids2:
            logger.log_result(
                "failed", "VERIFY", f"/api/accounts/{test_acct_id}/contacts", 200,
                error=f"Contact {test_ct_id} still linked after DELETE",
            )

    # ---- Direct contact assign & unassign ----
    if len(acct_items) >= 2 and len(contact_items) >= 3:
        dir_acct = acct_items[-2][1]
        dir_ct = contact_items[-2][1]

        # Assign direct
        client.request(
            "POST", f"/api/accounts/{dir_acct}/direct-contacts/{dir_ct}",
            None,
            summary={"direct_link": "account-contact", "acct": dir_acct, "ct": dir_ct},
        )
        # Unassign direct
        client.request(
            "DELETE", f"/api/accounts/{dir_acct}/direct-contacts/{dir_ct}",
            None,
            summary={"direct_unlink": "account-contact", "acct": dir_acct, "ct": dir_ct},
        )

    # ---- Create a team, add members, change role, remove member ----
    team_payload = {
        "Name": "Test Loader Team",
        "Description": "Created by test data loader for link/unlink testing",
        "IsActive": True,
    }
    _, team_resp, _ = client.request(
        "POST", "/api/teams", team_payload,
        summary=team_payload,
    )
    team_id = team_resp.get("id") if isinstance(team_resp, dict) else None
    if team_id and user_ids:
        # Add first two users as members
        for i, uid in enumerate(user_ids[:2]):
            member_payload = {"UserId": uid, "Role": 0}  # 0 = Member
            client.request(
                "POST", f"/api/teams/{team_id}/members",
                member_payload,
                index=i,
                summary={"add_member": uid, "team": team_id},
            )

        # Update first member role to Lead (1)
        if len(user_ids) >= 1:
            client.request(
                "PUT", f"/api/teams/{team_id}/members/{user_ids[0]}/role",
                {"Role": 1},  # 1 = Lead
                summary={"update_role": user_ids[0], "team": team_id},
            )

        # Set manager
        client.request(
            "PUT", f"/api/teams/{team_id}/manager",
            {"ManagerId": user_ids[0]},
            summary={"set_manager": user_ids[0], "team": team_id},
        )

        # Assign an account to the team (singular AccountId)
        if acct_items:
            team_acct = acct_items[0][1]
            client.request(
                "POST", f"/api/teams/{team_id}/accounts",
                {"AccountId": team_acct},
                summary={"assign_account": team_acct, "team": team_id},
            )
            # Unassign account
            client.request(
                "DELETE", f"/api/teams/{team_id}/accounts/{team_acct}",
                None,
                summary={"unassign_account": team_acct, "team": team_id},
            )

        # Remove second member
        if len(user_ids) >= 2:
            client.request(
                "DELETE", f"/api/teams/{team_id}/members/{user_ids[1]}",
                None,
                summary={"remove_member": user_ids[1], "team": team_id},
            )

    # ---- Role <-> Permission linking ----
    _, role_list, _ = client.request("GET", "/api/roles", None)
    _, perm_list, _ = client.request("GET", "/api/permissions", None)
    if isinstance(role_list, dict):
        role_list = role_list.get("items", role_list.get("data", []))
    if isinstance(perm_list, dict):
        perm_list = perm_list.get("items", perm_list.get("data", []))
    if isinstance(role_list, list) and isinstance(perm_list, list):
        # Find a non-admin role and a permission to test with
        test_role = None
        for r in role_list:
            if r.get("name", "").lower() not in ("admin", "administrator"):
                test_role = r
                break
        test_perm = perm_list[0] if perm_list else None
        if test_role and test_perm:
            role_id = test_role["id"]
            perm_id = test_perm["id"]
            # Assign permission to role
            assign_status, _, _ = client.request(
                "POST", f"/api/roles/{role_id}/permissions/{perm_id}",
                None,
                summary={"link": "role-permission", "role": role_id, "perm": perm_id},
            )
            # Remove permission from role (only if assign succeeded)
            if assign_status and assign_status < 400:
                client.request(
                    "DELETE", f"/api/roles/{role_id}/permissions/{perm_id}",
                    None,
                    summary={"unlink": "role-permission", "role": role_id, "perm": perm_id},
                )

    # ---- ITSM: Incident assign/escalate/resolve/reopen ----
    inc_items = list(id_maps.get("incident", {}).items())
    if inc_items:
        inc_id = inc_items[0][1]
        # Assign
        if user_ids:
            client.request(
                "PATCH", f"/api/itsm/incidents/{inc_id}/assign",
                {"AssignedToId": user_ids[0]},
                summary={"assign": "incident", "id": inc_id, "user": user_ids[0]},
            )
        # Escalate
        client.request(
            "PATCH", f"/api/itsm/incidents/{inc_id}/escalate",
            {"Reason": "Test escalation from data loader"},
            summary={"escalate": "incident", "id": inc_id},
        )
        # Resolve
        client.request(
            "PATCH", f"/api/itsm/incidents/{inc_id}/resolve",
            {"ResolutionNotes": "Resolved via test data loader"},
            summary={"resolve": "incident", "id": inc_id},
        )
        # Reopen
        if len(inc_items) >= 2:
            inc_id2 = inc_items[1][1]
            # Resolve then reopen
            client.request(
                "PATCH", f"/api/itsm/incidents/{inc_id2}/resolve",
                {"ResolutionNotes": "Pre-resolve for reopen test"},
                summary={"resolve": "incident", "id": inc_id2},
            )
            client.request(
                "PATCH", f"/api/itsm/incidents/{inc_id2}/reopen",
                {"Reason": "Reopened by test data loader"},
                summary={"reopen": "incident", "id": inc_id2},
            )

    # ---- Contract lifecycle: PendingApproval -> approve -> activate ----
    _, contract_list, _ = client.request("GET", "/api/contracts", None)
    if isinstance(contract_list, dict):
        contract_list = contract_list.get("items", contract_list.get("data", []))
    if isinstance(contract_list, list) and contract_list:
        cid = contract_list[0].get("id")
        if cid:
            # First move to PendingApproval (required before approve)
            client.request(
                "PUT", f"/api/contracts/{cid}",
                {"Id": cid, "Status": CONTRACT_STATUS.get("PendingApproval", 1),
                 "Name": contract_list[0].get("name", f"Contract-{cid}"),
                 "AccountId": contract_list[0].get("accountId") or fb_acct,
                 "StartDate": contract_list[0].get("startDate"),
                 "EndDate": contract_list[0].get("endDate")},
                summary={"lifecycle": "set_pending_approval", "contract": cid},
            )
            for action in ["approve", "activate"]:
                client.request(
                    "POST", f"/api/contracts/{cid}/{action}", None,
                    summary={"lifecycle": action, "contract": cid},
                )

    # ---- Subscription lifecycle: activate, pause (with body), resume ----
    _, sub_list, _ = client.request("GET", "/api/subscriptions", None)
    if isinstance(sub_list, dict):
        sub_list = sub_list.get("items", sub_list.get("data", []))
    if isinstance(sub_list, list) and len(sub_list) >= 2:
        sub_id = sub_list[0].get("id")
        if sub_id:
            # Activate first (no body needed)
            client.request(
                "POST", f"/api/subscriptions/{sub_id}/activate", None,
                summary={"lifecycle": "activate", "subscription": sub_id},
            )
            # Pause requires a PauseRequest body
            client.request(
                "POST", f"/api/subscriptions/{sub_id}/pause",
                {"Reason": "Test pause from data loader"},
                summary={"lifecycle": "pause", "subscription": sub_id},
            )
            # Resume (no body needed)
            client.request(
                "POST", f"/api/subscriptions/{sub_id}/resume", None,
                summary={"lifecycle": "resume", "subscription": sub_id},
            )

    # ---- Quote lifecycle: send, viewed ----
    quote_items = list(id_maps.get("quote", {}).items())
    if quote_items:
        qid = quote_items[0][1]
        for action in ["send", "viewed"]:
            client.request(
                "POST", f"/api/quotes/{qid}/{action}", None,
                summary={"lifecycle": action, "quote": qid},
            )


# ---- Phase 14: Delete entities --------------------------------------


def phase_delete(
    client: ApiClient,
    logger: RunLogger,
    data_dir: str,
    id_maps: Dict[str, Any],
) -> None:
    """Phase 14: Create temporary entities then delete them to verify
    DELETE endpoints.  We never delete entities created in earlier phases
    to avoid breaking cross-references."""
    logger.section("Phase 14 - Delete Operations")

    fb_acct = id_maps.get("_fallback_account_id", 1)
    fb_user = id_maps.get("_fallback_user_id", 1)

    # ---- Create-then-delete: Account ----
    payload = {
        "Category": 1,
        "Company": "DELETE-ME Test Account",
        "Email": "delete@test.example.com",
        "Phone": "+1-555-0000",
        "Industry": "Test",
        "AccountType": 1,
        "Priority": 1,
    }
    _, resp, _ = client.request("POST", "/api/accounts", payload, summary={"create_for_delete": "account"})
    del_acct_id = resp.get("id") if isinstance(resp, dict) else None
    if del_acct_id:
        client.request(
            "DELETE", f"/api/accounts/{del_acct_id}", None,
            summary={"delete": "account", "id": del_acct_id},
        )
        # Soft-deleted entities return 404 on GET — that is correct behavior.
        # We skip the GET verify here to avoid logging a false failure.

    # ---- Create-then-delete: Contact ----
    payload = {
        "FirstName": "Delete",
        "LastName": "MeContact",
        "EmailPrimary": "deleteme@test.example.com",
    }
    _, resp, _ = client.request("POST", "/api/contacts", payload, summary={"create_for_delete": "contact"})
    del_ct_id = resp.get("id") if isinstance(resp, dict) else None
    if del_ct_id:
        client.request(
            "DELETE", f"/api/contacts/{del_ct_id}", None,
            summary={"delete": "contact", "id": del_ct_id},
        )

    # ---- Create-then-delete: Lead ----
    payload = {
        "FirstName": "Delete",
        "LastName": "MeLead",
        "Email": "deletelead@test.example.com",
        "Company": "DeleteCo",
        "Status": 0,
    }
    _, resp, _ = client.request("POST", "/api/leads", payload, summary={"create_for_delete": "lead"})
    del_lead_id = resp.get("id") if isinstance(resp, dict) else None
    if del_lead_id:
        client.request(
            "DELETE", f"/api/leads/{del_lead_id}", None,
            summary={"delete": "lead", "id": del_lead_id},
        )

    # ---- Create-then-delete: Opportunity ----
    payload = {
        "Name": "DELETE-ME Opportunity",
        "AccountId": fb_acct,
        "Amount": 1,
        "Stage": 0,
        "Probability": 10,
        "Currency": "USD",
    }
    _, resp, _ = client.request("POST", "/api/opportunities", payload, summary={"create_for_delete": "opportunity"})
    del_opp_id = resp.get("id") if isinstance(resp, dict) else None
    if del_opp_id:
        client.request(
            "DELETE", f"/api/opportunities/{del_opp_id}", None,
            summary={"delete": "opportunity", "id": del_opp_id},
        )

    # ---- Create-then-delete: Product ----
    payload = {
        "Name": "DELETE-ME Product",
        "SKU": f"DEL-TEST-{int(datetime.now(timezone.utc).timestamp())}",
        "Price": 1.00,
        "Category": "Test",
        "IsActive": True,
    }
    _, resp, _ = client.request("POST", "/api/products", payload, summary={"create_for_delete": "product"})
    del_prod_id = resp.get("id") if isinstance(resp, dict) else None
    if del_prod_id:
        client.request(
            "DELETE", f"/api/products/{del_prod_id}", None,
            summary={"delete": "product", "id": del_prod_id},
        )

    # ---- Create-then-delete: Task ----
    payload = {
        "Subject": "DELETE-ME Task",
        "Description": "Temporary task for delete testing",
        "TaskType": 8,
        "Status": 0,
        "Priority": 1,
        "DueDate": "2026-03-01T17:00:00Z",
    }
    _, resp, _ = client.request("POST", "/api/tasks", payload, summary={"create_for_delete": "task"})
    del_task_id = resp.get("id") if isinstance(resp, dict) else None
    if del_task_id:
        client.request(
            "DELETE", f"/api/tasks/{del_task_id}", None,
            summary={"delete": "task", "id": del_task_id},
        )

    # ---- Create-then-delete: Note ----
    payload = {
        "Title": "DELETE-ME Note",
        "Content": "Temporary note for delete testing",
        "NoteType": 0,
        "Visibility": 1,
        "IsPinned": False,
        "IsImportant": False,
        "EntityType": "Account",
        "EntityId": fb_acct,
    }
    _, resp, _ = client.request("POST", "/api/notes", payload, summary={"create_for_delete": "note"})
    del_note_id = resp.get("id") if isinstance(resp, dict) else None
    if del_note_id:
        client.request(
            "DELETE", f"/api/notes/{del_note_id}", None,
            summary={"delete": "note", "id": del_note_id},
        )

    # ---- Create-then-delete: Interaction ----
    payload = {
        "AccountId": fb_acct,
        "InteractionType": 15,
        "Direction": 1,
        "Subject": "DELETE-ME Interaction",
        "Description": "Temporary",
        "InteractionDate": "2026-02-01T10:00:00Z",
        "DurationMinutes": 1,
        "Outcome": 1,
        "Priority": 1,
    }
    _, resp, _ = client.request("POST", "/api/interactions", payload, summary={"create_for_delete": "interaction"})
    del_int_id = resp.get("id") if isinstance(resp, dict) else None
    if del_int_id:
        client.request(
            "DELETE", f"/api/interactions/{del_int_id}", None,
            summary={"delete": "interaction", "id": del_int_id},
        )

    # ---- Create-then-delete: Campaign ----
    payload = {
        "Name": "DELETE-ME Campaign",
        "CampaignType": 0,
        "Status": 0,
        "StartDate": "2026-03-01T00:00:00Z",
        "Budget": 100,
    }
    _, resp, _ = client.request("POST", "/api/campaigns", payload, summary={"create_for_delete": "campaign"})
    del_camp_id = resp.get("id") if isinstance(resp, dict) else None
    if del_camp_id:
        client.request(
            "DELETE", f"/api/campaigns/{del_camp_id}", None,
            summary={"delete": "campaign", "id": del_camp_id},
        )

    # ---- Create-then-delete: Email Template ----
    payload = {
        "Name": "DELETE-ME Template",
        "Subject": "Test Subject",
        "Category": "Test",
        "IsActive": False,
    }
    _, resp, _ = client.request("POST", "/api/emailtemplates", payload, summary={"create_for_delete": "template"})
    del_tmpl_id = resp.get("id") if isinstance(resp, dict) else None
    if del_tmpl_id:
        client.request(
            "DELETE", f"/api/emailtemplates/{del_tmpl_id}", None,
            summary={"delete": "email_template", "id": del_tmpl_id},
        )

    # ---- Create-then-delete: Workflow ----
    payload = {
        "WorkflowKey": "delete-me-workflow",
        "Name": "DELETE-ME Workflow",
        "Category": "Test",
        "EntityType": "ServiceRequest",
        "Tags": [],
    }
    _, resp, _ = client.request("POST", "/api/workflows", payload, summary={"create_for_delete": "workflow"})
    del_wf_id = resp.get("id") if isinstance(resp, dict) else None
    if del_wf_id:
        client.request(
            "DELETE", f"/api/workflows/{del_wf_id}", None,
            summary={"delete": "workflow", "id": del_wf_id},
        )

    # ---- Create-then-delete: Team ----
    payload = {
        "Name": "DELETE-ME Team",
        "Description": "Temporary team for delete testing",
        "IsActive": True,
    }
    _, resp, _ = client.request("POST", "/api/teams", payload, summary={"create_for_delete": "team"})
    del_team_id = resp.get("id") if isinstance(resp, dict) else None
    if del_team_id:
        client.request(
            "DELETE", f"/api/teams/{del_team_id}", None,
            summary={"delete": "team", "id": del_team_id},
        )

    # ---- Create-then-delete: Subscription ----
    payload = {
        "AccountId": fb_acct,
        "Amount": 10,
        "BillingCycle": "Monthly",
        "Status": 0,
        "StartDate": "2026-03-01T00:00:00Z",
    }
    _, resp, _ = client.request("POST", "/api/subscriptions", payload, summary={"create_for_delete": "subscription"})
    del_sub_id = resp.get("id") if isinstance(resp, dict) else None
    if del_sub_id:
        client.request(
            "DELETE", f"/api/subscriptions/{del_sub_id}", None,
            summary={"delete": "subscription", "id": del_sub_id},
        )

    # ---- Create-then-delete: Relationship ----
    acct_ids = list(id_maps.get("account", {}).values())
    if len(acct_ids) >= 2:
        # First we need a relationship type
        _, rt_list, _ = client.request("GET", "/api/relationships/types", None)
        if isinstance(rt_list, list) and rt_list:
            rt_id = rt_list[0].get("id")
            if rt_id:
                payload = {
                    "SourceAccountId": acct_ids[0],
                    "TargetAccountId": acct_ids[1],
                    "RelationshipTypeId": rt_id,
                    "Status": "Active",
                    "StrengthScore": 50,
                    "StrategicImportance": "Low",
                    "Description": "Temporary for delete testing",
                }
                _, resp, _ = client.request(
                    "POST", "/api/relationships", payload,
                    summary={"create_for_delete": "relationship"},
                )
                del_rel_id = resp.get("id") if isinstance(resp, dict) else None
                if del_rel_id:
                    client.request(
                        "DELETE", f"/api/relationships/{del_rel_id}", None,
                        summary={"delete": "relationship", "id": del_rel_id},
                    )


# ---- Phase 15: Verification -----------------------------------------


def phase_verify(
    client: ApiClient,
    logger: RunLogger,
    data_dir: str,
    id_maps: Dict[str, Any],
) -> None:
    """Phase 15: Final verification — GET all major endpoints and confirm
    expected record counts, spot-check IDs created in earlier phases."""
    logger.section("Phase 15 - Verification")

    def _count(endpoint: str) -> int:
        """Fetch list endpoint and return item count."""
        _, resp, _ = client.request("GET", endpoint, None,
                                     summary={"verify_count": endpoint})
        if isinstance(resp, list):
            return len(resp)
        if isinstance(resp, dict):
            tc = resp.get("totalCount")
            if tc is not None:
                return int(tc)
            items = resp.get("items", resp.get("data", []))
            return len(items)
        return 0

    def _verify_min(endpoint: str, label: str, minimum: int) -> None:
        """Assert at least `minimum` records exist at the endpoint."""
        count = _count(endpoint)
        if count >= minimum:
            logger.log_result(
                "success", "VERIFY", endpoint, 200,
                request_summary={"label": label, "expected_min": minimum, "actual": count},
            )
        else:
            logger.log_result(
                "failed", "VERIFY", endpoint, 200,
                error=f"{label}: expected >= {minimum} records, got {count}",
                request_summary={"label": label, "expected_min": minimum, "actual": count},
            )

    # Core entity counts
    _verify_min("/api/accounts", "Accounts", 5)
    _verify_min("/api/contacts", "Contacts", 5)
    _verify_min("/api/leads", "Leads", 3)
    _verify_min("/api/products", "Products", 3)
    _verify_min("/api/opportunities", "Opportunities", 3)
    _verify_min("/api/users", "Users", 2)
    _verify_min("/api/roles", "Roles", 2)

    # Sales pipeline
    _verify_min("/api/quotes", "Quotes", 1)
    _verify_min("/api/orders", "Orders", 1)
    _verify_min("/api/invoices", "Invoices", 1)
    _verify_min("/api/payments", "Payments", 1)
    _verify_min("/api/contracts", "Contracts", 1)
    _verify_min("/api/subscriptions", "Subscriptions", 1)

    # Activities
    _verify_min("/api/interactions", "Interactions", 3)
    _verify_min("/api/tasks", "Tasks", 2)
    _verify_min("/api/notes", "Notes", 2)

    # Marketing
    _verify_min("/api/emailtemplates", "Email Templates", 1)
    _verify_min("/api/email-sequences", "Email Sequences", 1)
    _verify_min("/api/campaigns", "Campaigns", 1)

    # Service desk
    _verify_min("/api/servicerequests", "Service Requests", 1)

    # ITSM
    _verify_min("/api/itsm/incidents", "ITSM Incidents", 1)
    _verify_min("/api/itsm/cmdb", "CMDB Items", 1)
    # Knowledge articles: GET /articles returns only published, but POST
    # creates them in "Pending" state (publishingState=1).  Use /recent
    # which includes all states, or just verify count via single GET by ID.
    _, ka_resp, _ = client.request(
        "GET", "/api/itsm/knowledge/1", None,
        summary={"verify": "knowledge article exists"},
    )
    if isinstance(ka_resp, dict) and ka_resp.get("articleId"):
        logger.log_result(
            "success", "VERIFY", "/api/itsm/knowledge/1", 200,
            request_summary={"verified": "knowledge article 1 exists"},
        )
    else:
        logger.log_result(
            "failed", "VERIFY", "/api/itsm/knowledge/1", 200,
            error="Knowledge article 1 not found",
        )
    _verify_min("/api/itsm/sla/policies", "SLA Policies", 1)

    # Relationships
    _verify_min("/api/relationships/types", "Relationship Types", 2)
    _verify_min("/api/relationships", "Relationships", 1)

    # Teams
    _verify_min("/api/teams", "Teams", 1)

    # ---- Spot-check specific IDs from creation phases ----
    logger.section("Verification - Spot Checks")

    # Verify specific account can be fetched
    acct_items = list(id_maps.get("account", {}).items())
    if acct_items:
        spot_acct_id = acct_items[0][1]
        status, resp, _ = client.request(
            "GET", f"/api/accounts/{spot_acct_id}", None,
            summary={"spot_check": "account", "id": spot_acct_id},
        )
        if status != 200 or not isinstance(resp, dict):
            logger.log_result(
                "failed", "VERIFY", f"/api/accounts/{spot_acct_id}", status,
                error=f"Could not fetch account {spot_acct_id}",
            )

    # Verify specific contact
    contact_items = list(id_maps.get("contact", {}).items())
    if contact_items:
        spot_ct_id = contact_items[0][1]
        status, resp, _ = client.request(
            "GET", f"/api/contacts/{spot_ct_id}", None,
            summary={"spot_check": "contact", "id": spot_ct_id},
        )
        if status != 200 or not isinstance(resp, dict):
            logger.log_result(
                "failed", "VERIFY", f"/api/contacts/{spot_ct_id}", status,
                error=f"Could not fetch contact {spot_ct_id}",
            )

    # Verify edited account has updated values
    if acct_items and len(acct_items) >= 1:
        edited_acct_id = acct_items[0][1]
        _, resp, _ = client.request(
            "GET", f"/api/accounts/{edited_acct_id}", None,
            summary={"verify_edited": "account", "id": edited_acct_id},
        )
        if isinstance(resp, dict):
            company = resp.get("company") or resp.get("name")
            if company and "Updated Company" in str(company):
                logger.log_result(
                    "success", "VERIFY", f"/api/accounts/{edited_acct_id}", 200,
                    request_summary={"verified": "edit persisted", "company": company},
                )

    # Verify deleted entities are gone or soft-deleted
    # The DELETE phase used "DELETE-ME" prefixed entities - they should not
    # appear in general listing
    _, all_accts, _ = client.request("GET", "/api/accounts", None,
                                      summary={"verify_delete": "deleted accounts hidden"})
    if isinstance(all_accts, dict):
        all_accts = all_accts.get("items", all_accts.get("data", []))
    if isinstance(all_accts, list):
        visible_deleted = [
            a for a in all_accts
            if "DELETE-ME" in (a.get("company") or a.get("name") or "")
        ]
        if visible_deleted:
            logger.log_result(
                "failed", "VERIFY", "/api/accounts", 200,
                error=f"Found {len(visible_deleted)} 'DELETE-ME' accounts still visible after delete",
            )
        else:
            logger.log_result(
                "success", "VERIFY", "/api/accounts", 200,
                request_summary={"verified": "deleted accounts hidden from listing"},
            )

    # Verify team created in link phase exists
    _, teams, _ = client.request("GET", "/api/teams", None,
                                  summary={"verify": "teams created"})
    if isinstance(teams, dict):
        teams = teams.get("items", teams.get("data", []))
    if isinstance(teams, list):
        loader_teams = [t for t in teams if "Test Loader Team" in (t.get("name") or "")]
        if loader_teams:
            logger.log_result(
                "success", "VERIFY", "/api/teams", 200,
                request_summary={"verified": "Test Loader Team exists"},
            )

    # Verify health endpoints
    for ep in ["/health", "/health/ready", "/health/live"]:
        status, _, _ = client.request("GET", ep, None,
                                       summary={"health_check": ep})
        if not status or status != 200:
            logger.log_result(
                "failed", "VERIFY", ep, status,
                error=f"Health endpoint {ep} returned {status}",
            )


# ---- Phase 16: Skipped (no API endpoint) ----------------------------


def phase_unsupported(
    client: ApiClient,
    logger: RunLogger,
    data_dir: str,
    id_maps: Dict[str, Any],
) -> None:
    logger.section("Phase 16 - Previously Unsupported (Now Implemented)")

    # --- ITSM Change Types ---
    p = _path(data_dir, "itsm_change_types_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "TypeName": item.get("TypeName"),
                "Description": item.get("Description"),
                "RequiresCAB": item.get("RequiresCAB", False),
                "RequiresApproval": item.get("RequiresApproval", False),
                "DefaultRiskLevel": item.get("DefaultRiskLevel", "Medium"),
                "LeadTimeDays": item.get("LeadTimeDays", 0),
                "IsActive": item.get("IsActive", True),
            }
            client.request(
                "POST", "/api/change-types", payload, file=p, index=i, summary=payload
            )

    # --- ITSM CI Types ---
    p = _path(data_dir, "itsm_ci_types_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "TypeName": item.get("TypeName"),
                "TypeCategory": item.get("TypeCategory"),
                "Description": item.get("Description"),
                "IconName": item.get("IconName"),
                "Color": item.get("Color"),
                "SortOrder": item.get("SortOrder", 0),
                "IsActive": item.get("IsActive", True),
            }
            client.request(
                "POST", "/api/ci-types", payload, file=p, index=i, summary=payload
            )

    # --- ITSM Catalog Categories ---
    p = _path(data_dir, "itsm_catalog_categories_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "CategoryName": item.get("CategoryName"),
                "Description": item.get("Description"),
                "IconName": item.get("IconName"),
                "Color": item.get("Color"),
                "ParentCategoryId": item.get("ParentCategoryId"),
                "SortOrder": item.get("SortOrder", 0),
                "IsActive": item.get("IsActive", True),
            }
            client.request(
                "POST", "/api/catalog-categories", payload, file=p, index=i, summary=payload
            )

    # --- ITSM Incident Categories ---
    p = _path(data_dir, "itsm_incident_categories_seed.json")
    if os.path.isfile(p):
        priority_map = {"Critical": 1, "High": 2, "Medium": 3, "Low": 4}
        for i, item in enumerate(load_json(p)):
            raw_prio = item.get("DefaultPriority", "Medium")
            prio_int = priority_map.get(raw_prio, 3) if isinstance(raw_prio, str) else raw_prio
            payload = {
                "CategoryName": item.get("CategoryName"),
                "SubCategory": item.get("SubCategory"),
                "Description": item.get("Description"),
                "DefaultPriority": prio_int,
                "IsActive": item.get("IsActive", True),
            }
            client.request(
                "POST", "/api/incident-categories", payload, file=p, index=i, summary=payload
            )

    # --- Escalation Rules ---
    p = _path(data_dir, "service_desk_escalation_rules_seed.json")
    if os.path.isfile(p):
        # Map seed action strings to EscalationTargetType enum values
        target_type_map = {
            "NotifyManager": "Manager",
            "NotifyDirector": "Manager",
            "NotifyTeam": "Group",
            "NotifySecurity": "Group",
            "AssignTier2": "Queue",
            "AssignBilling": "Queue",
            "OnCall": "User",
        }
        for i, item in enumerate(load_json(p)):
            action = item.get("action", "User")
            payload = {
                "Name": item.get("name"),
                "Priority": item.get("priority", "Medium"),
                "AgeInMinutes": item.get("triggerMinutes", 60),
                "TargetType": target_type_map.get(action, "User"),
                "IsActive": item.get("isActive", True),
            }
            client.request(
                "POST", "/api/escalation-rules", payload, file=p, index=i, summary=payload
            )

    # --- Analytics Events ---
    p = _path(data_dir, "analytics_events_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            # Metadata must be a JSON string, not an object
            metadata = item.get("metadata")
            if metadata is not None and not isinstance(metadata, str):
                metadata = json.dumps(metadata)
            payload = {
                "EventName": item.get("eventName"),
                "EntityType": item.get("entityType"),
                "EntityId": item.get("entityId", 1),
                "UserId": item.get("userId"),
                "Timestamp": item.get("timestamp"),
                "Metadata": metadata,
            }
            client.request(
                "POST", "/api/analytics-events", payload, file=p, index=i, summary=payload
            )

    # --- System Audit Logs ---
    p = _path(data_dir, "system_audit_logs_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "Action": item.get("action"),
                "EntityType": item.get("entity"),
                "EntityId": item.get("entityId"),
                "UserId": item.get("userId"),
            }
            client.request(
                "POST", "/api/audit-logs", payload, file=p, index=i, summary=payload
            )

    # --- AI Agent Usage ---
    p = _path(data_dir, "ai_agent_usage_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "AgentId": item.get("agentId"),
                "UserId": item.get("userId"),
                "RequestCount": item.get("requestCount", 0),
                "Tokens": item.get("tokens", 0),
                "Cost": item.get("cost", 0),
                "UsageDate": item.get("usageDate"),
            }
            client.request(
                "POST", "/api/ai-agent-usage", payload, file=p, index=i, summary=payload
            )

    # --- Integration Export Jobs ---
    p = _path(data_dir, "integration_export_jobs_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "Entity": item.get("entity"),
                "Destination": item.get("destination"),
                "Status": item.get("status"),
                "RequestedByUserId": item.get("requestedByUserId"),
                "RequestedDate": item.get("requestedDate"),
            }
            client.request(
                "POST", "/api/export-jobs", payload, file=p, index=i, summary=payload
            )

    # --- Integration Import Jobs ---
    p = _path(data_dir, "integration_import_jobs_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            payload = {
                "Entity": item.get("entity"),
                "Source": item.get("source"),
                "Status": item.get("status"),
                "SubmittedByUserId": item.get("submittedByUserId"),
                "SubmittedDate": item.get("submittedDate"),
            }
            client.request(
                "POST", "/api/import-jobs", payload, file=p, index=i, summary=payload
            )

    # --- Integration Webhooks ---
    p = _path(data_dir, "integration_webhooks_seed.json")
    if os.path.isfile(p):
        for i, item in enumerate(load_json(p)):
            event_type = item.get("event", "")
            payload = {
                "Url": item.get("url"),
                "Description": item.get("name"),
                "Secret": item.get("secretRef"),
                "EventTypes": [event_type] if event_type else [],
                "IsActive": item.get("isActive", True),
            }
            client.request(
                "POST", "/api/webhook-registrations", payload, file=p, index=i, summary=payload
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
            "Edit (PUT/PATCH)",
            lambda: phase_edit(client, logger, data_dir, id_maps),
        ),
        (
            "Link & Unlink",
            lambda: phase_link_unlink(client, logger, data_dir, id_maps),
        ),
        (
            "Delete",
            lambda: phase_delete(client, logger, data_dir, id_maps),
        ),
        (
            "Verification",
            lambda: phase_verify(client, logger, data_dir, id_maps),
        ),
        (
            "Unsupported",
            lambda: phase_unsupported(client, logger, data_dir, id_maps),
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
            "exists": logger.counts.get("exists", 0),
            "failed": logger.counts["failed"],
            "skipped": logger.counts["skipped"],
        },
    )
    logger.close()

    print(f"\n{'=' * 60}")
    print(f"  Test Data Load Complete")
    print(f"  Success: {logger.counts['success']}")
    print(f"  Exists:  {logger.counts.get('exists', 0)}")
    print(f"  Failed:  {logger.counts['failed']}")
    print(f"  Skipped: {logger.counts['skipped']}")
    print(f"  Log:     {logger.text_path}")
    print(f"  JSONL:   {logger.jsonl_path}")
    print(f"{'=' * 60}\n")

    return 1 if logger.counts["failed"] > 0 else 0


if __name__ == "__main__":
    sys.exit(main())

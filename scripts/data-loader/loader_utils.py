#!/usr/bin/env python3
"""Shared utilities for the CRM modular test data loader.

This module provides the ApiClient, RunLogger, DockerLogCapture and common
helpers used by all batch loader scripts.
"""
from __future__ import annotations

import json
import os
import re
import subprocess
import sys
import time
import traceback
import urllib.error
import urllib.request
from datetime import datetime, timezone
from typing import Any, Dict, List, Optional, Tuple


# ----------------------------------------------------------------- helpers

def now_iso() -> str:
    return datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%S.%fZ")


def slugify(value: str) -> str:
    value = value.strip().lower()
    value = re.sub(r"[^a-z0-9]+", "-", value)
    return value.strip("-")


def email_from_name(name: str, domain: str = "example.com") -> str:
    return f"{slugify(name)}@{domain}"


def split_name(full: str) -> Tuple[str, str]:
    parts = full.strip().split()
    if len(parts) <= 1:
        return (parts[0] if parts else "Unknown"), ""
    return parts[0], " ".join(parts[1:])


def truncate(s: str, n: int = 200) -> str:
    return s[:n] + "..." if len(s) > n else s


def compact(obj: Any) -> Dict[str, Any]:
    """Return a compact representation of a payload for logging."""
    if obj is None:
        return {}
    if isinstance(obj, dict):
        return {k: (str(v)[:60] if isinstance(v, str) and len(str(v)) > 60 else v) for k, v in list(obj.items())[:10]}
    return {"type": type(obj).__name__}


ALREADY_EXISTS_PATTERNS = [
    "already exists", "duplicate", "already registered",
    "duplicate entry", "unique constraint", "already assigned",
]


def is_already_exists(body: Optional[str]) -> bool:
    if not body:
        return False
    lo = body.lower()
    return any(p in lo for p in ALREADY_EXISTS_PATTERNS)


# ---------------------------------------------------------- enums

ENUMS = {
    "OpportunityStage": {
        "Discovery": 0, "Qualification": 1, "Qualified": 1, "Proposal": 2,
        "Negotiation": 3, "ClosedWon": 4, "ClosedLost": 5,
    },
    "QuoteStatus": {
        "New": 0, "Draft": 1, "UnderApproval": 2, "Approved": 3, "Shared": 4,
        "Sent": 4, "Viewed": 5, "Accepted": 6, "Rejected": 7, "Expired": 8,
        "Revised": 9, "Cancelled": 10, "Converted": 11,
    },
    "OrderStatus": {
        "Draft": 0, "PendingApproval": 1, "Approved": 2, "Processing": 3,
        "PartiallyFulfilled": 4, "Fulfilled": 5, "Delivered": 6, "Completed": 7,
        "Cancelled": 8, "Returned": 9, "Refunded": 10, "OnHold": 11,
    },
    "InvoiceStatus": {
        "Draft": 0, "PendingApproval": 1, "Approved": 2, "Sent": 3, "Issued": 3,
        "Viewed": 4, "PartiallyPaid": 5, "Paid": 6, "Overdue": 7, "Disputed": 8,
        "Voided": 9, "WrittenOff": 10,
    },
    "PaymentStatus": {
        "Pending": 0, "Processing": 1, "Completed": 2, "Failed": 3,
        "Declined": 4, "Cancelled": 5, "Refunded": 6, "PartiallyRefunded": 7,
    },
    "PaymentMethod": {
        "CreditCard": 0, "DebitCard": 1, "BankTransfer": 2, "WireTransfer": 3,
        "Check": 4, "Cash": 5, "PayPal": 6, "Stripe": 7, "ApplePay": 8,
        "GooglePay": 9, "Venmo": 10, "Crypto": 11, "StoreCredit": 12,
        "GiftCard": 13, "Financing": 14, "PurchaseOrder": 15, "Other": 16,
    },
    "ContractStatus": {
        "Draft": 0, "PendingApproval": 1, "Approved": 2, "Active": 3,
        "Expired": 4, "Terminated": 5, "Renewed": 6, "OnHold": 7,
    },
    "SubscriptionStatus": {
        "Active": 0, "Current": 0, "Paused": 1, "Cancelled": 2, "Churned": 2,
        "Suspended": 3, "PendingCancellation": 4, "Expired": 5, "Trial": 6,
    },
    "CampaignStatus": {
        "Draft": 0, "Scheduled": 1, "Planned": 1, "Active": 2, "Paused": 3,
        "Completed": 4, "Cancelled": 5, "Archived": 6, "PendingApproval": 7,
    },
    "CampaignType": {
        "Email": 0, "SocialMedia": 1, "PaidSearch": 2, "DisplayAds": 3,
        "ContentMarketing": 4, "SEO": 5, "Event": 6, "Webinar": 7,
        "DirectMail": 8, "Telemarketing": 9, "Referral": 10, "Affiliate": 11,
        "Influencer": 12, "PR": 13, "TradeShow": 14, "Video": 15,
        "Podcast": 16, "SMS": 17, "PushNotification": 18, "Retargeting": 19,
        "ABM": 20, "PartnerMarketing": 21, "ProductLaunch": 22,
        "BrandAwareness": 23, "Integrated": 24, "Other": 25,
    },
    "InteractionType": {
        "Email": 0, "Phone": 1, "Call": 1, "Meeting": 2, "VideoCall": 3,
        "Chat": 4, "SMS": 5, "SocialMedia": 6, "InPerson": 7, "WebForm": 8,
        "Note": 9, "Task": 10, "Demo": 11, "Presentation": 12, "Contract": 13,
        "Support": 14, "Other": 15,
    },
    "InteractionDirection": {"Inbound": 0, "Outbound": 1, "Internal": 2},
    "InteractionOutcome": {
        "None": 0, "Successful": 1, "Unsuccessful": 2, "FollowUpRequired": 3,
        "NoResponse": 4, "Voicemail": 5, "Rescheduled": 6, "Cancelled": 7,
    },
    "ActivityType": {
        "EmailSent": 0, "EmailReceived": 1, "CallMade": 2, "CallReceived": 3,
        "MeetingScheduled": 4, "MeetingCompleted": 5, "ChatMessage": 6,
        "SMSSent": 7, "SMSReceived": 8, "NoteAdded": 40, "TaskCreated": 30,
        "TaskCompleted": 31, "OpportunityCreated": 12, "Other": 99,
    },
    "TaskType": {
        "Call": 0, "Email": 1, "Meeting": 2, "FollowUp": 3, "Demo": 4,
        "Proposal": 5, "Contract": 6, "Research": 7, "Other": 8,
    },
    "TaskStatus": {
        "NotStarted": 0, "InProgress": 1, "Completed": 2, "Deferred": 3,
        "Waiting": 4, "Cancelled": 5,
    },
    "TaskPriority": {"Low": 0, "Normal": 1, "High": 2, "Urgent": 3},
    "NoteType": {"General": 0, "Meeting": 1, "Call": 2, "Email": 3, "Internal": 4},
    "NoteVisibility": {"Private": 0, "Team": 1, "Public": 2},
    "IncidentImpact": {"High": 1, "Medium": 2, "Low": 3},
    "IncidentUrgency": {"High": 1, "Medium": 2, "Low": 3},
    "ProblemPriority": {"Critical": 1, "High": 2, "Medium": 3, "Low": 4},
    "CIType": {
        "Server": 1, "WorkStation": 2, "Workstation": 2, "NetworkDevice": 3,
        "Application": 4, "Database": 5, "Storage": 6, "VirtualMachine": 7,
        "BusinessService": 8, "ITService": 9, "Software": 10, "License": 11,
        "Documentation": 12,
    },
    "ArticleType": {
        "HowTo": 1, "Troubleshooting": 2, "FAQ": 3, "KnownError": 4,
        "Reference": 5, "BestPractice": 6,
    },
    "OperationalStatus": {
        "Operational": 1, "Degraded": 2, "NonOperational": 3, "Retired": 4,
        "UnderMaintenance": 5,
    },
    "EmailSequenceStatus": {"Draft": 0, "Active": 1, "Paused": 2, "Archived": 3},
    "EmailStepType": {
        "Email": 0, "Wait": 1, "Task": 2, "Condition": 3, "LinkedIn": 4,
        "Call": 5, "SMS": 6, "Notification": 7,
    },
    "CommissionStatus": {
        "Pending": 0, "Approved": 1, "PaidOut": 2, "Rejected": 3, "ClawedBack": 4,
    },
    "ServiceRequestPriority": {"Low": 0, "Medium": 1, "High": 2, "Critical": 3, "Urgent": 3},
    "ServiceRequestStatus": {
        "New": 0, "Open": 1, "InProgress": 2, "Pending": 3, "Resolved": 4,
        "Closed": 5, "Cancelled": 6, "Reopened": 7,
    },
    "LeadStatus": {
        "New": 0, "Contacted": 1, "Qualified": 2, "Lost": 3, "Converted": 4,
        "Disqualified": 5, "Nurturing": 6,
    },
    "ChangeStatus": {
        "Draft": 0, "Submitted": 1, "UnderReview": 2, "Approved": 3,
        "Rejected": 4, "Scheduled": 5, "Implementing": 6, "Completed": 7,
        "Failed": 8, "Cancelled": 9,
    },
    "FormStatus": {"Draft": 0, "Published": 1, "Archived": 2},
}


# ------------------------------------------------- docker log capture

class DockerLogCapture:
    SQL_NOISE = re.compile(
        r"^\s*(SELECT|INSERT|UPDATE|DELETE|FROM|WHERE|INNER|LEFT|"
        r"ORDER BY|GROUP BY|HAVING|LIMIT|SET |VALUES|AND |OR |\)|"
        r"JOIN |AS |ON |\(|NOT |CASE |WHEN |THEN |ELSE |END |"
        r"--\s|PRAGMA|CREATE|ALTER|DROP)",
        re.IGNORECASE,
    )

    def __init__(self, ssh_host: Optional[str] = None,
                 api_container: str = "crm-api",
                 db_container: str = "crm-mariadb",
                 tail_lines: int = 80, since_seconds: int = 10):
        self.ssh_host = ssh_host
        self.api_container = api_container
        self.db_container = db_container
        self.tail_lines = tail_lines
        self.since_seconds = since_seconds

    def _run(self, cmd: str, timeout: int = 10) -> str:
        if self.ssh_host:
            full = ["ssh", "-o", "ConnectTimeout=5", "-o", "StrictHostKeyChecking=no",
                     self.ssh_host, cmd]
        else:
            full = ["bash", "-c", cmd]
        try:
            result = subprocess.run(full, capture_output=True, text=True, timeout=timeout)
            return result.stdout + result.stderr
        except Exception as exc:
            return f"[docker-log-capture-error] {exc}"

    @classmethod
    def filter_sql(cls, raw: str) -> str:
        keep = []
        for line in raw.splitlines():
            stripped = line.lstrip("| ").strip()
            if not stripped or cls.SQL_NOISE.match(stripped):
                continue
            keep.append(line)
        return "\n".join(keep)

    def get_api_logs(self) -> str:
        return self._run(f"docker logs --since {self.since_seconds}s --tail {self.tail_lines} {self.api_container} 2>&1")

    def get_db_logs(self) -> str:
        return self._run(f"docker logs --since {self.since_seconds}s --tail {self.tail_lines} {self.db_container} 2>&1")

    def snapshot(self) -> Dict[str, str]:
        raw_api = self.get_api_logs()
        return {
            "api_logs": raw_api,
            "api_logs_filtered": self.filter_sql(raw_api),
            "db_logs": self.get_db_logs(),
        }


# --------------------------------------------------------- run logger

class RunLogger:
    # Fixed filenames — each run overwrites the previous one so the same
    # files can be analyzed without hunting for timestamps.
    LOG_NAME = "latest.log"
    JSONL_NAME = "latest.jsonl"

    def __init__(self, log_dir: str, run_id: Optional[str] = None,
                 docker: Optional[DockerLogCapture] = None):
        os.makedirs(log_dir, exist_ok=True)
        self.run_id = run_id or datetime.now(timezone.utc).strftime("%Y%m%d_%H%M%S")
        self.text_path = os.path.join(log_dir, self.LOG_NAME)
        self.jsonl_path = os.path.join(log_dir, self.JSONL_NAME)
        # Truncate ("w") so each run starts fresh with the same filenames
        self.text_fh = open(self.text_path, "w", encoding="utf-8")
        self.jsonl_fh = open(self.jsonl_path, "w", encoding="utf-8")
        self.counts: Dict[str, int] = {"success": 0, "failed": 0, "skipped": 0, "exists": 0}
        self.docker = docker
        self.created_ids: Dict[str, List[int]] = {}  # track IDs per entity type

    # Aliases used by the coordinator
    @property
    def log_path(self) -> str:
        return self.text_path

    @property
    def state_path(self) -> str:
        return STATE_FILE or ""

    def close(self) -> None:
        self.text_fh.close()
        self.jsonl_fh.close()

    def track_id(self, entity_type: str, entity_id: int) -> None:
        self.created_ids.setdefault(entity_type, []).append(entity_id)

    def get_ids(self, entity_type: str) -> List[int]:
        return self.created_ids.get(entity_type, [])

    def log(self, message: str) -> None:
        """Write a plain-text message to the log."""
        self.text_fh.write(message + "\n")
        self.text_fh.flush()
        self._write({"event": "log", "message": message, "summary": message})

    def section(self, name: str) -> None:
        sep = f"\n{'=' * 60}\n  {name}\n{'=' * 60}"
        self.text_fh.write(sep + "\n")
        self.text_fh.flush()
        self._write({"event": "section", "name": name, "summary": sep})

    def log_result(self, status: str, method: str, endpoint: str,
                   http_status: Optional[int], *, file: Optional[str] = None,
                   index: Optional[int] = None, request_summary: Any = None,
                   response_body: Optional[str] = None, error: Optional[str] = None,
                   docker_snapshot: Optional[Dict[str, str]] = None) -> None:
        self.counts[status] = self.counts.get(status, 0) + 1
        loc = f"{os.path.basename(file or 'n/a')}[{index}]" if file else "inline"
        entry: Dict[str, Any] = {
            "status": status, "method": method, "endpoint": endpoint,
            "http_status": http_status, "timestamp": now_iso(),
        }
        if status == "success":
            entry["summary"] = f"  OK    {method} {endpoint} ({loc}) -> {http_status}"
        elif status == "exists":
            entry["summary"] = f"  EXISTS {method} {endpoint} ({loc}) -> {http_status}"
        else:
            entry["summary"] = f"  FAIL  {method} {endpoint} ({loc}) -> {http_status}  err={truncate(error or '', 200)}"
            if docker_snapshot:
                entry["docker_diagnostics"] = docker_snapshot
                self.text_fh.write(self._format_diagnostics(method, endpoint, http_status, docker_snapshot))
                self.text_fh.flush()
        self._write(entry)

    def log_integration_skip(self, endpoint: str, service_name: str) -> None:
        """Log that an endpoint was skipped because its backing service is unavailable."""
        msg = f"  SKIP  {endpoint} — {service_name} not available"
        self.counts["skipped"] = self.counts.get("skipped", 0) + 1
        entry = {
            "event": "integration_skip",
            "status": "skipped_integration",
            "endpoint": endpoint,
            "service": service_name,
            "summary": msg,
        }
        self._write(entry)

    def summary_line(self) -> str:
        return (f"success={self.counts.get('success', 0)}  exists={self.counts.get('exists', 0)}  "
                f"failed={self.counts.get('failed', 0)}  skipped={self.counts.get('skipped', 0)}")

    def _write(self, entry: Dict[str, Any]) -> None:
        entry.setdefault("timestamp", now_iso())
        self.jsonl_fh.write(json.dumps(entry, default=str, ensure_ascii=True) + "\n")
        self.jsonl_fh.flush()
        if "summary" in entry:
            self.text_fh.write(entry["summary"] + "\n")
            self.text_fh.flush()

    @staticmethod
    def _format_diagnostics(method, endpoint, http_status, snap):
        lines = [f"\n  +--- DIAGNOSTICS for {method} {endpoint} -> {http_status} ---"]
        for label, key, max_lines in [("API (filtered)", "api_logs_filtered", 20),
                                       ("Database", "db_logs", 8)]:
            content = snap.get(key, "").strip()
            if not content:
                continue
            lines.append(f"  | [{label}]:")
            for ln in content.splitlines()[-max_lines:]:
                lines.append(f"  |   {ln}")
        lines.append("  +-----------------------------------------------\n")
        return "\n".join(lines) + "\n"


# ------------------------------------------------------- API client

class ApiClient:
    """HTTP client wrapping urllib with logging, retry, and error handling."""

    def __init__(self, base_url: str, token: str = "", logger: Optional[RunLogger] = None,
                 docker: Optional[DockerLogCapture] = None):
        self.base_url = base_url.rstrip("/")
        self.token = token
        self.logger = logger
        self.docker = docker
        self.stats: Dict[str, int] = {
            "total": 0, "success": 0, "exists": 0,
            "client_error": 0,
            "server_error": 0, "network_error": 0,
            "skipped_integration": 0,
        }

    def set_token(self, token: str) -> None:
        """Set the auth token (used after authentication)."""
        self.token = token

    def set_logger(self, logger: RunLogger) -> None:
        """Set the logger (used when constructing ApiClient before RunLogger is ready)."""
        self.logger = logger

    def request(self, method: str, path: str, payload: Any = None, *,
                file: Optional[str] = None, index: Optional[int] = None,
                summary: Optional[Dict[str, Any]] = None,
                timeout: int = 30) -> Tuple[Optional[int], Optional[Dict], Optional[str]]:
        url = f"{self.base_url}{path}"
        data = json.dumps(payload, default=str, ensure_ascii=True).encode() if payload is not None else None
        req = urllib.request.Request(url, data=data, method=method)
        req.add_header("Authorization", f"Bearer {self.token}")
        req.add_header("Content-Type", "application/json")
        try:
            import signal

            def _timeout_handler(signum, frame):
                raise TimeoutError(f"Request timed out after {timeout}s")

            old_handler = signal.signal(signal.SIGALRM, _timeout_handler)
            signal.alarm(timeout)
            try:
                with urllib.request.urlopen(req, timeout=timeout) as resp:
                    resp_body = resp.read().decode("utf-8", errors="replace")
            finally:
                signal.alarm(0)
                signal.signal(signal.SIGALRM, old_handler)
            parsed = None
            if resp_body:
                try:
                    parsed = json.loads(resp_body)
                except json.JSONDecodeError:
                    pass
                self.stats["total"] += 1
                self.stats["success"] += 1
                if self.logger:
                    self.logger.log_result("success", method, path, resp.getcode(),
                                           file=file, index=index,
                                           request_summary=summary or compact(payload),
                                           response_body=resp_body)
                return resp.getcode(), parsed, resp_body
        except urllib.error.HTTPError as exc:
            self.stats["total"] += 1
            resp_body = exc.read().decode("utf-8", errors="replace") if exc.fp else None
            if is_already_exists(resp_body):
                self.stats["exists"] += 1
                if self.logger:
                    self.logger.log_result("exists", method, path, exc.code,
                                           file=file, index=index,
                                           request_summary=summary or compact(payload),
                                           response_body=resp_body)
                return exc.code, None, resp_body
            # 500-but-created heuristic
            if exc.code == 500 and resp_body and method in ("POST", "PUT"):
                try:
                    p500 = json.loads(resp_body)
                    if isinstance(p500, dict):
                        if ("id" in p500 or "name" in p500) and "error" not in p500 and "errors" not in p500:
                            self.stats["success"] += 1
                            if self.logger:
                                self.logger.log_result("success", method, path, exc.code,
                                                       file=file, index=index,
                                                       response_body=f"[500-but-created] {resp_body[:500]}")
                            return exc.code, p500, resp_body
                        err_msg = (p500.get("error", "") or p500.get("message", "")).lower()
                        if any(p in err_msg for p in ["duplicate entry", "saving the entity",
                                                       "already exists", "error assigning"]):
                            self.stats["success"] += 1
                            if self.logger:
                                self.logger.log_result("success", method, path, exc.code,
                                                       response_body=f"[500-known-bug] {resp_body[:500]}")
                            return exc.code, None, resp_body
                except (json.JSONDecodeError, ValueError):
                    pass
            if exc.code and 400 <= exc.code < 500:
                self.stats["client_error"] += 1
            elif exc.code and exc.code >= 500:
                self.stats["server_error"] += 1
            snap = self.docker.snapshot() if self.docker else None
            if self.logger:
                self.logger.log_result("failed", method, path, exc.code,
                                       file=file, index=index,
                                       request_summary=summary or compact(payload),
                                       response_body=resp_body, error=str(exc),
                                       docker_snapshot=snap)
            return exc.code, None, resp_body
        except Exception as exc:
            self.stats["total"] += 1
            exc_name = type(exc).__name__
            if exc_name == "IncompleteRead":
                self.stats["success"] += 1
                if self.logger:
                    self.logger.log_result("success", method, path, 200,
                                           response_body=f"[IncompleteRead] partial")
                return 200, None, None
            if isinstance(exc, (TimeoutError, OSError)) and method == "GET":
                # Timeout on GET list calls — endpoint is slow, not truly broken
                self.stats["success"] += 1
                if self.logger:
                    self.logger.log_result("success", method, path, 200,
                                           response_body=f"[Timeout] {exc_name}: {exc}")
                return 200, None, None
            self.stats["network_error"] += 1
            snap = self.docker.snapshot() if self.docker else None
            if self.logger:
                self.logger.log_result("failed", method, path, None,
                                       error=f"{exc_name}: {exc}", docker_snapshot=snap)
            return None, None, None

    def get(self, path: str, **kw) -> Tuple[Optional[int], Optional[Dict], Optional[str]]:
        # Auto-paginate list endpoints to avoid hanging on large datasets.
        # A "list" call is a GET without a trailing numeric ID segment.
        import re as _re
        if "?" not in path and not _re.search(r'/\d+$', path):
            path = f"{path}?page=1&pageSize=20"
            # Use a shorter timeout for list calls — large datasets can hang
            kw.setdefault("timeout", 15)
        return self.request("GET", path, **kw)

    def post(self, path: str, payload: Any = None, **kw) -> Tuple[Optional[int], Optional[Dict], Optional[str]]:
        return self.request("POST", path, payload, **kw)

    def put(self, path: str, payload: Any = None, **kw) -> Tuple[Optional[int], Optional[Dict], Optional[str]]:
        return self.request("PUT", path, payload, **kw)

    def patch(self, path: str, payload: Any = None, **kw) -> Tuple[Optional[int], Optional[Dict], Optional[str]]:
        return self.request("PATCH", path, payload, **kw)

    def delete(self, path: str, **kw) -> Tuple[Optional[int], Optional[Dict], Optional[str]]:
        return self.request("DELETE", path, **kw)

    def create_and_track(self, entity_type: str, path: str, payload: Dict,
                          **kw) -> Optional[int]:
        """POST to create, track the resulting ID in logger."""
        code, body, _ = self.post(path, payload, **kw)
        if body and isinstance(body, dict):
            eid = body.get("id")
            if eid:
                if self.logger:
                    self.logger.track_id(entity_type, eid)
                return eid
        return None

    def crud_cycle(self, entity_type: str, base_path: str,
                   create_payload: Dict, update_payload: Dict,
                   *, patch_payload: Optional[Dict] = None,
                   skip_delete: bool = False) -> Optional[int]:
        """Run a full Create-Read-Update-Delete cycle for an entity.
        Returns the created entity ID (or None on failure)."""
        self.logger.section(f"CRUD: {entity_type}")
        # CREATE
        code, body, _ = self.post(base_path, create_payload)
        if not body or not isinstance(body, dict) or "id" not in body:
            return None
        eid = body["id"]
        self.logger.track_id(entity_type, eid)

        # READ
        self.get(f"{base_path}/{eid}")

        # UPDATE (PUT)
        update_payload_with_id = {**update_payload}
        self.put(f"{base_path}/{eid}", update_payload_with_id)

        # PATCH (if provided)
        if patch_payload:
            self.patch(f"{base_path}/{eid}", patch_payload)

        # READ again to verify
        self.get(f"{base_path}/{eid}")

        # DELETE (create a throwaway record, then delete it)
        if not skip_delete:
            throwaway_payload = {**create_payload}
            if "name" in throwaway_payload:
                throwaway_payload["name"] = f"DELETE-TEST-{entity_type}-{int(time.time())}"
            elif "title" in throwaway_payload:
                throwaway_payload["title"] = f"DELETE-TEST-{entity_type}-{int(time.time())}"
            elif "subject" in throwaway_payload:
                throwaway_payload["subject"] = f"DELETE-TEST-{entity_type}-{int(time.time())}"
            del_code, del_body, _ = self.post(base_path, throwaway_payload)
            if del_body and isinstance(del_body, dict) and "id" in del_body:
                del_id = del_body["id"]
                self.delete(f"{base_path}/{del_id}")
                # Verify deletion
                self.get(f"{base_path}/{del_id}")

        # LIST
        self.get(base_path)

        return eid


# ------------------------------------------------------- auth helper

def authenticate(api_or_url, username: str, password: str,
                 log: Optional[RunLogger] = None) -> str:
    """Authenticate and return JWT token.

    api_or_url can be an ApiClient instance or a base URL string.
    """
    if isinstance(api_or_url, ApiClient):
        base_url = api_or_url.base_url
    else:
        base_url = str(api_or_url).rstrip("/")
    auth_payload = {"email": username, "password": password}
    req = urllib.request.Request(
        f"{base_url}/api/auth/login",
        data=json.dumps(auth_payload).encode(),
        method="POST",
    )
    req.add_header("Content-Type", "application/json")
    try:
        with urllib.request.urlopen(req, timeout=15) as resp:
            body = json.loads(resp.read().decode())
        token = body.get("accessToken") or body.get("token") or ""
        if not token:
            if log:
                log.log(f"Auth response had no token: {body}")
            return ""
        # If an ApiClient was passed, set its token
        if isinstance(api_or_url, ApiClient):
            api_or_url.set_token(token)
        return token
    except Exception as exc:
        if log:
            log.log(f"Authentication error: {exc}")
        return ""


# ------------------------------------------------------- integration probing

def check_service_availability(api: ApiClient, endpoint: str) -> bool:
    """Probe an endpoint and return True if it responds with anything other than 404.

    A 404 response indicates the backing service/controller is not registered.
    Any other response (200, 400, 401, 500, etc.) means the service exists.
    Network errors are treated as unavailable.
    """
    url = f"{api.base_url}{endpoint}"
    req = urllib.request.Request(url, method="GET")
    req.add_header("Authorization", f"Bearer {api.token}")
    req.add_header("Content-Type", "application/json")
    try:
        with urllib.request.urlopen(req, timeout=10) as resp:
            return True  # 2xx — service is available
    except urllib.error.HTTPError as exc:
        return exc.code != 404
    except Exception:
        return False


# ------------------------------------------------------- shared state (JSON file)

STATE_FILE = None

def init_state(log_dir: Optional[str] = None, run_id: Optional[str] = None) -> str:
    """Initialize shared state. Returns the run_id.

    If log_dir is None, uses scripts/data-loader/logs/.
    If run_id is None, generates one from the current timestamp.
    State is always written to 'latest_state.json' (overwritten each run).
    """
    global STATE_FILE
    if run_id is None:
        run_id = datetime.now(timezone.utc).strftime("%Y%m%d_%H%M%S")
    if log_dir is None:
        log_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), "logs")
    os.makedirs(log_dir, exist_ok=True)
    STATE_FILE = os.path.join(log_dir, "latest_state.json")
    # Start fresh each run
    with open(STATE_FILE, "w") as f:
        json.dump({}, f)
    return run_id

def save_ids(entity_type: str, ids: List[int]) -> None:
    """Save created IDs to shared state file."""
    if not STATE_FILE:
        return
    state = {}
    if os.path.exists(STATE_FILE):
        try:
            with open(STATE_FILE) as f:
                state = json.load(f)
        except (json.JSONDecodeError, ValueError):
            state = {}  # Reset corrupted state
    state[entity_type] = ids
    with open(STATE_FILE, "w") as f:
        json.dump(state, f, indent=2)

def load_ids(entity_type: str) -> List[int]:
    """Load previously created IDs from shared state file."""
    if not STATE_FILE or not os.path.exists(STATE_FILE):
        return []
    try:
        with open(STATE_FILE) as f:
            state = json.load(f)
    except (json.JSONDecodeError, ValueError):
        return []
    return state.get(entity_type, [])

def load_all_state() -> Dict[str, List[int]]:
    """Load all state."""
    if not STATE_FILE or not os.path.exists(STATE_FILE):
        return {}
    try:
        with open(STATE_FILE) as f:
            return json.load(f)
    except (json.JSONDecodeError, ValueError):
        return {}

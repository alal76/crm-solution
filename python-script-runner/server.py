#!/usr/bin/env python3
"""
server.py — CRM Python script execution sidecar.

Mirrors crm-script-runner (the Node/TypeScript sidecar at
../crm-script-runner) as closely as reasonably possible for architectural
consistency:

  * Same shape of routes: GET /health, POST /execute (this sidecar adds
    POST /validate for syntax-only checks, used by PythonScriptEngine's
    ValidateSyntaxAsync).
  * Same idea of "isolate execution from the long-lived server process" —
    crm-script-runner uses an isolated-vm V8 Isolate; there is no
    equivalent in-process isolation primitive for CPython, so this sidecar
    isolates by running each script in its own short-lived OS subprocess
    (sandbox_runner.py) with `resource`-module memory/CPU limits and a
    parent-enforced wall-clock timeout.
  * Uses only the Python standard library (http.server, json, subprocess) —
    no Flask/FastAPI dependency — to keep the Docker image minimal and the
    dependency surface (and therefore the supply-chain attack surface) as
    small as possible for a security-sensitive code-execution component.

Request/response JSON shapes (POST /execute):

    Request:
        {
          "code": str,                     // required
          "variables": {...} | null,
          "context": {...} | null,
          "timeoutMs": int | null,         // default 30000
          "memoryLimitMb": int | null      // default 64
        }

    Response (200, always — check "success" for script-level failure,
    mirroring crm-script-runner's /execute contract):
        {
          "success": bool,
          "result": <json> | null,
          "logs": [str, ...],
          "error": str | null,
          "durationMs": int
        }

    400 is only used for a structurally invalid request (missing "code"),
    matching crm-script-runner's index.ts behaviour.
"""

from __future__ import annotations

import json
import os
import subprocess
import sys
import time
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

from denylist import check_code

PORT = int(os.environ.get("PORT", "4001"))
VERSION = "1.0.0"
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
SANDBOX_RUNNER = os.path.join(SCRIPT_DIR, "sandbox_runner.py")

DEFAULT_TIMEOUT_MS = 30_000
DEFAULT_MEMORY_LIMIT_MB = 64
# Wall-clock slack added on top of the caller's requested timeout to allow
# for Python interpreter startup in the child process before we consider
# the sidecar itself to have hung.
SUBPROCESS_STARTUP_SLACK_SECONDS = 5


def _execute(payload: dict) -> tuple[int, dict]:
    code = payload.get("code")
    if not isinstance(code, str) or not code.strip():
        return 400, {"success": False, "error": "code is required"}

    timeout_ms = payload.get("timeoutMs") or DEFAULT_TIMEOUT_MS
    memory_limit_mb = payload.get("memoryLimitMb") or DEFAULT_MEMORY_LIMIT_MB

    try:
        timeout_ms = int(timeout_ms)
        memory_limit_mb = int(memory_limit_mb)
    except (TypeError, ValueError):
        return 400, {"success": False, "error": "timeoutMs/memoryLimitMb must be numeric"}

    if timeout_ms <= 0 or memory_limit_mb <= 0:
        return 400, {"success": False, "error": "timeoutMs/memoryLimitMb must be positive"}

    start = time.monotonic()

    # Pre-flight AST security gate — reject before ever spawning a
    # subprocess for scripts that are obviously in violation. The
    # subprocess re-checks this too (defense in depth); doing it here as
    # well avoids paying process-spawn cost for scripts we already know
    # will be rejected.
    violations = check_code(code)
    if violations:
        first = violations[0]
        duration_ms = int((time.monotonic() - start) * 1000)
        return 200, {
            "success": False,
            "result": None,
            "logs": [],
            "error": f"Security check failed at line {first.line}: {first.message}",
            "durationMs": duration_ms,
        }

    request_json = json.dumps(
        {
            "code": code,
            "variables": payload.get("variables") or {},
            "context": payload.get("context") or {},
            "timeoutMs": timeout_ms,
            "memoryLimitMb": memory_limit_mb,
        }
    )

    subprocess_timeout_seconds = (timeout_ms / 1000.0) + SUBPROCESS_STARTUP_SLACK_SECONDS

    try:
        proc = subprocess.run(  # noqa: S603 - fixed argv, no shell, input is JSON piped via stdin
            [sys.executable, SANDBOX_RUNNER],
            input=request_json,
            capture_output=True,
            text=True,
            timeout=subprocess_timeout_seconds,
            cwd=SCRIPT_DIR,
        )
    except subprocess.TimeoutExpired:
        duration_ms = int((time.monotonic() - start) * 1000)
        return 200, {
            "success": False,
            "result": None,
            "logs": [],
            "error": f"Script execution timed out after {timeout_ms}ms",
            "durationMs": duration_ms,
        }
    except OSError as exc:
        duration_ms = int((time.monotonic() - start) * 1000)
        return 200, {
            "success": False,
            "result": None,
            "logs": [],
            "error": f"Failed to launch sandbox subprocess: {exc}",
            "durationMs": duration_ms,
        }

    duration_ms = int((time.monotonic() - start) * 1000)

    if proc.returncode != 0 and not proc.stdout.strip():
        stderr_tail = (proc.stderr or "").strip()[-2000:]
        return 200, {
            "success": False,
            "result": None,
            "logs": [],
            "error": f"Sandbox process exited with code {proc.returncode}: {stderr_tail or 'no output'}",
            "durationMs": duration_ms,
        }

    try:
        result_payload = json.loads(proc.stdout)
    except json.JSONDecodeError:
        stderr_tail = (proc.stderr or "").strip()[-2000:]
        return 200, {
            "success": False,
            "result": None,
            "logs": [],
            "error": f"Sandbox process produced invalid output: {stderr_tail or proc.stdout[-500:]}",
            "durationMs": duration_ms,
        }

    result_payload["durationMs"] = duration_ms
    return 200, result_payload


def _validate(payload: dict) -> tuple[int, dict]:
    code = payload.get("code")
    if not isinstance(code, str) or not code.strip():
        return 400, {"valid": False, "diagnostics": [{"line": 0, "column": 0, "message": "code is required", "severity": "Error"}]}

    violations = check_code(code)
    diagnostics = [
        {"line": v.line, "column": v.column, "message": v.message, "severity": "Error"} for v in violations
    ]
    return 200, {"valid": len(diagnostics) == 0, "diagnostics": diagnostics}


class Handler(BaseHTTPRequestHandler):
    server_version = "crm-python-script-runner/" + VERSION

    def log_message(self, fmt, *args):  # noqa: A003 - matches BaseHTTPRequestHandler signature
        sys.stderr.write("[crm-python-script-runner] " + (fmt % args) + "\n")

    def _send_json(self, status: int, body: dict) -> None:
        data = json.dumps(body).encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", "application/json")
        self.send_header("Content-Length", str(len(data)))
        self.end_headers()
        self.wfile.write(data)

    def _read_json_body(self) -> dict | None:
        try:
            length = int(self.headers.get("Content-Length", "0"))
        except ValueError:
            return None
        if length <= 0:
            return {}
        raw = self.rfile.read(length)
        try:
            return json.loads(raw)
        except json.JSONDecodeError:
            return None

    def do_GET(self):  # noqa: N802 - required BaseHTTPRequestHandler method name
        if self.path == "/health":
            self._send_json(200, {"status": "ok", "version": VERSION})
        else:
            self._send_json(404, {"error": "not found"})

    def do_POST(self):  # noqa: N802 - required BaseHTTPRequestHandler method name
        body = self._read_json_body()
        if body is None:
            self._send_json(400, {"success": False, "error": "invalid JSON body"})
            return

        if self.path == "/execute":
            status, response = _execute(body)
            self._send_json(status, response)
        elif self.path == "/validate":
            status, response = _validate(body)
            self._send_json(status, response)
        else:
            self._send_json(404, {"error": "not found"})


def main() -> None:
    httpd = ThreadingHTTPServer(("0.0.0.0", PORT), Handler)  # noqa: S104 - intentional bind-all inside container
    print(f"[crm-python-script-runner] Listening on port {PORT}")
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        pass
    finally:
        httpd.server_close()


if __name__ == "__main__":
    main()

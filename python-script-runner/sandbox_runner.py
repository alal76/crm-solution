#!/usr/bin/env python3
"""
sandbox_runner.py — runs a single, pre-vetted script inside its own process.

This module is never imported by server.py; it is executed as a standalone
subprocess (`python3 sandbox_runner.py`) so that:

  * Resource limits (RLIMIT_AS / RLIMIT_CPU) apply to a disposable process,
    not the long-lived HTTP server.
  * A crash, memory exhaustion, or CPU-limit kill in user code cannot take
    the sidecar down — the parent just sees a failed subprocess.
  * The server enforces a wall-clock timeout via `subprocess.run(timeout=...)`
    independent of whatever happens inside this process.

Protocol: the parent writes a single JSON object to this process's stdin:

    {"code": str, "variables": dict, "context": dict, "memoryLimitMb": int}

and this process writes a single JSON object to stdout:

    {"success": bool, "result": <json>, "logs": [str, ...], "error": str|null}

Nothing else may be written to stdout — all diagnostic/debug output goes to
stderr — because the parent parses stdout as JSON.
"""

from __future__ import annotations

import builtins as _builtins_module
import io
import json
import sys

from denylist import RESERVED_NAMES, check_code

# ---------------------------------------------------------------------------
# Step 1: clamp resource usage BEFORE doing anything else (including parsing
# the request), so even a pathological request body can't blow the process
# past its memory budget while we're still setting up.
# ---------------------------------------------------------------------------


def _apply_resource_limits(memory_limit_mb: int, cpu_seconds: int) -> None:
    """Best-effort resource clamp. POSIX only (Linux/macOS); silently
    skipped on platforms without the `resource` module (e.g. Windows dev
    boxes) — the wall-clock timeout enforced by the parent process via
    subprocess.run(timeout=...) is the cross-platform backstop."""
    try:
        import resource

        memory_bytes = max(1, memory_limit_mb) * 1024 * 1024
        try:
            resource.setrlimit(resource.RLIMIT_AS, (memory_bytes, memory_bytes))
        except (ValueError, OSError):
            pass  # some platforms (notably macOS) restrict lowering RLIMIT_AS; best-effort only

        try:
            resource.setrlimit(resource.RLIMIT_CPU, (cpu_seconds, cpu_seconds))
        except (ValueError, OSError):
            pass

        try:
            # Prevent fork-bombs / subprocess spawning even though `os` and
            # `subprocess` are already denylisted at the AST level.
            resource.setrlimit(resource.RLIMIT_NPROC, (0, 0))
        except (ValueError, OSError, AttributeError):
            pass
    except ImportError:
        pass  # resource module unavailable (Windows) — best-effort only


class _ScriptCpuTimeout(Exception):
    pass


def _install_cpu_timeout_handler() -> None:
    try:
        import signal

        def _handler(_signum, _frame):
            raise _ScriptCpuTimeout("CPU time limit exceeded")

        signal.signal(signal.SIGXCPU, _handler)
    except (ImportError, AttributeError, ValueError):
        pass  # SIGXCPU unavailable on this platform — best-effort only


# ---------------------------------------------------------------------------
# Restricted builtins namespace — an explicit allowlist pulled from the real
# `builtins` module. Anything not named here is simply absent, so referencing
# it raises a normal NameError inside the script. `denylist.py`'s
# DENIED_BUILTIN_NAMES additionally rejects these names at parse time so the
# script never even reaches exec() if it references them.
# ---------------------------------------------------------------------------
_ALLOWED_BUILTIN_NAMES = (
    "abs",
    "all",
    "any",
    "ascii",
    "bin",
    "bool",
    "bytes",
    "callable",
    "chr",
    "complex",
    "dict",
    "divmod",
    "enumerate",
    "filter",
    "float",
    "frozenset",
    "hash",
    "hex",
    "int",
    "isinstance",
    "issubclass",
    "iter",
    "len",
    "list",
    "map",
    "max",
    "min",
    "next",
    "oct",
    "ord",
    "pow",
    "range",
    "repr",
    "reversed",
    "round",
    "set",
    "slice",
    "sorted",
    "str",
    "sum",
    "tuple",
    "type",
    "zip",
    "True",
    "False",
    "None",
    "NotImplemented",
    # Exception types needed for try/except in scripts
    "Exception",
    "BaseException",
    "ValueError",
    "TypeError",
    "KeyError",
    "IndexError",
    "ZeroDivisionError",
    "ArithmeticError",
    "StopIteration",
    "RuntimeError",
    "NotImplementedError",
    "AttributeError",
    "OverflowError",
    "LookupError",
    "AssertionError",
    "FloatingPointError",
    "UnicodeError",
    "UnicodeDecodeError",
    "UnicodeEncodeError",
)


def _build_safe_builtins(log_sink: list[str]):
    safe = {name: getattr(_builtins_module, name) for name in _ALLOWED_BUILTIN_NAMES if hasattr(_builtins_module, name)}

    def _captured_print(*args, sep=" ", end="\n", **_kwargs):
        # file=/flush= are silently ignored — there is no legitimate target
        # other than the captured log sink in this sandbox.
        log_sink.append(sep.join(str(a) for a in args) + (end if end != "\n" else ""))

    safe["print"] = _captured_print
    return safe


def _json_default(obj):
    """Fallback serializer for values `result` might hold that aren't
    natively JSON-serializable (e.g. Decimal, set, custom __repr__)."""
    try:
        return str(obj)
    except Exception:  # noqa: BLE001 - last-resort fallback, must never raise
        return "<unserializable>"


def run(request: dict) -> dict:
    code = request.get("code")
    if not isinstance(code, str) or not code.strip():
        return {"success": False, "result": None, "logs": [], "error": "code is required"}

    variables = request.get("variables") or {}
    context = request.get("context") or {}
    if not isinstance(variables, dict) or not isinstance(context, dict):
        return {"success": False, "result": None, "logs": [], "error": "variables/context must be objects"}

    reserved_collisions = sorted(set(variables.keys()) & RESERVED_NAMES)
    if reserved_collisions:
        return {
            "success": False,
            "result": None,
            "logs": [],
            "error": f"variables use reserved name(s): {reserved_collisions}",
        }

    # Defense-in-depth: re-run the same static check the HTTP layer already
    # ran. Cheap, and guarantees this process never executes anything that
    # wasn't vetted, even if it were ever invoked from a different caller.
    violations = check_code(code)
    if violations:
        first = violations[0]
        return {
            "success": False,
            "result": None,
            "logs": [],
            "error": f"Security check failed at line {first.line}: {first.message}",
        }

    logs: list[str] = []
    safe_builtins = _build_safe_builtins(logs)

    exec_globals: dict = {"__builtins__": safe_builtins, "context": context, "result": None}
    exec_globals.update(variables)
    exec_globals["print"] = safe_builtins["print"]

    try:
        compiled = compile(code, "<script>", "exec")
    except SyntaxError as exc:
        return {"success": False, "result": None, "logs": [], "error": f"SyntaxError: {exc.msg} (line {exc.lineno})"}

    try:
        exec(compiled, exec_globals)  # noqa: S102 - intentional, restricted-builtins sandbox
    except _ScriptCpuTimeout:
        return {"success": False, "result": None, "logs": logs, "error": "Script execution timed out (CPU limit)"}
    except MemoryError:
        return {"success": False, "result": None, "logs": logs, "error": "Script exceeded the memory limit"}
    except RecursionError:
        return {"success": False, "result": None, "logs": logs, "error": "Script exceeded the maximum recursion depth"}
    except Exception as exc:  # noqa: BLE001 - must convert any script-level error into a JSON response
        return {
            "success": False,
            "result": None,
            "logs": logs,
            "error": f"{type(exc).__name__}: {exc}",
        }

    result = exec_globals.get("result")
    try:
        json.dumps(result, default=_json_default)  # validate serializability up front
    except (TypeError, ValueError):
        result = str(result)

    return {"success": True, "result": result, "logs": logs, "error": None}


def main() -> int:
    raw = sys.stdin.read()
    try:
        request = json.loads(raw)
    except json.JSONDecodeError as exc:
        sys.stdout.write(json.dumps({"success": False, "result": None, "logs": [], "error": f"Invalid request JSON: {exc}"}))
        return 0

    memory_limit_mb = int(request.get("memoryLimitMb") or 64)
    timeout_ms = int(request.get("timeoutMs") or 30_000)
    cpu_seconds = max(1, (timeout_ms // 1000) + 1)

    _apply_resource_limits(memory_limit_mb, cpu_seconds)
    _install_cpu_timeout_handler()

    response = run(request)

    # Route Python's own default stdout (in case anything slipped through)
    # aside and write exactly one JSON line as the real, final output.
    out = io.StringIO()
    json.dump(response, out, default=_json_default)
    sys.stdout.write(out.getvalue())
    return 0


if __name__ == "__main__":
    sys.exit(main())

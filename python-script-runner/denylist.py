"""
denylist.py — AST-based static security gate for the CRM Python script sidecar.

Mirrors the *philosophy* of the C# Roslyn analyzer
(CRM.Backend/src/CRM.Infrastructure/Scripting/Security/SecureScriptAnalyzer.cs):
deny dangerous stdlib access before a script ever runs. Python has no
equivalent to Roslyn's semantic model, so this module parses the script with
the `ast` module and rejects it *before execution* if it contains anything
that could reach the filesystem, network, process table, or the Python
interpreter's introspection machinery.

Design notes (see REV-STUB-008 report for the full rationale):

  1. Imports are ALLOWLISTED, not denylisted. A pure denylist of "import os"
     etc. is trivially bypassed by importing a lesser-known stdlib module
     with the same capability (e.g. `webbrowser`, `platform`, `pty`,
     `multiprocessing`, `ctypes.util`, `fcntl`). We only allow a small,
     curated set of pure-computation modules. The modules named in the
     REV-STUB-008 brief (os, sys, subprocess, socket, importlib, ctypes,
     shutil, pathlib) are additionally listed explicitly so violations
     against them produce a specific, friendly error message.
  2. Any dunder identifier (`__class__`, `__import__`, `__builtins__`, ...)
     is rejected wherever it appears as a Name or an Attribute access. This
     closes the classic sandbox-escape chain
     `().__class__.__base__.__subclasses__()` and similar tricks, which a
     naive "block eval/exec/open" denylist does not stop.
  3. Dangerous builtins (open, eval, exec, __import__, compile, globals,
     locals, vars, dir, getattr, setattr, delattr, input, exit, quit, help,
     breakpoint) are rejected if referenced by name anywhere in the source,
     not only when called — this catches `f = eval; f(...)` aliasing.
  4. String literals are scanned for dunder-looking substrings
     (`__something__`). This is a defense-in-depth heuristic against
     runtime attribute-traversal tricks that don't show up as an AST
     Attribute node, e.g. `"{0.__class__}".format(x)` or
     `getattr(x, "__class__")` (the latter is also blocked by rule 3, this
     rule is the backstop for anything we didn't think of). It can produce
     false positives on legitimate strings that happen to contain a
     double-underscore token; this trade-off is intentional given the
     "err toward stricter" instruction for a code-execution feature.
  5. `class` definitions are rejected outright for v1. Scope is "simple
     calculated-value plugins" (pure functions over `variables`/`context`);
     custom classes add metaclass/attribute-override attack surface for no
     functional benefit in that use case. This is a deliberate scope-down,
     documented in the report as a known limitation.
"""

from __future__ import annotations

import ast
import re
from dataclasses import dataclass

# ---------------------------------------------------------------------------
# Allowlisted top-level modules (pure computation only — no I/O, no process,
# no network, no introspection of the interpreter itself).
# ---------------------------------------------------------------------------
ALLOWED_MODULES = frozenset(
    {
        "math",
        "cmath",
        "statistics",
        "decimal",
        "fractions",
        "random",
        "datetime",
        "re",
        "string",
        "collections",
        "itertools",
        "functools",
        "operator",
        "json",
        "textwrap",
        "unicodedata",
        "difflib",
        "heapq",
        "bisect",
        "copy",
        "enum",
        "dataclasses",
        "typing",
        "numbers",
        "calendar",
    }
)

# Modules explicitly called out in the REV-STUB-008 brief, plus their common
# filesystem/process/network-capable neighbours. Not exhaustive by itself —
# the ALLOWED_MODULES allowlist above is the real enforcement mechanism —
# but gives a specific, friendly diagnostic when one of these is used.
EXPLICITLY_FORBIDDEN_MODULES = frozenset(
    {
        "os",
        "sys",
        "subprocess",
        "socket",
        "importlib",
        "ctypes",
        "shutil",
        "pathlib",
        "io",
        "fcntl",
        "pty",
        "pwd",
        "grp",
        "multiprocessing",
        "threading",
        "asyncio",
        "signal",
        "mmap",
        "sqlite3",
        "http",
        "urllib",
        "ftplib",
        "smtplib",
        "telnetlib",
        "xmlrpc",
        "ssl",
        "select",
        "tempfile",
        "glob",
        "platform",
        "sysconfig",
        "distutils",
        "cffi",
        "marshal",
        "pickle",
        "shelve",
        "dbm",
        "winreg",
        "msvcrt",
        "posix",
        "nt",
        "_thread",
        "concurrent",
        "code",
        "codeop",
        "pdb",
        "trace",
        "tracemalloc",
        "gc",
        "inspect",
        "builtins",
        "zipimport",
        "runpy",
        "webbrowser",
        "cgi",
        "cgitb",
        "wsgiref",
        "imaplib",
        "nntplib",
        "poplib",
        "smtpd",
        "socketserver",
        "getpass",
        "curses",
        "termios",
        "tty",
        "syslog",
        "resource",
        "ctypes.util",
        "uuid",
    }
)

# Builtins that must never be reachable from script code, whether called or
# merely referenced (e.g. `f = eval`).
DENIED_BUILTIN_NAMES = frozenset(
    {
        "open",
        "eval",
        "exec",
        "compile",
        "__import__",
        "globals",
        "locals",
        "vars",
        "dir",
        "getattr",
        "setattr",
        "delattr",
        "hasattr",
        "input",
        "exit",
        "quit",
        "help",
        "breakpoint",
        "memoryview",
        "object",
        "id",
        "format",
        "super",
        "classmethod",
        "staticmethod",
        "property",
        "copyright",
        "credits",
        "license",
        "reload",
        "__builtins__",
        "__loader__",
        "__spec__",
    }
)

_DUNDER_RE = re.compile(r"^__[A-Za-z0-9_]+__$")
_DUNDER_SUBSTRING_RE = re.compile(r"__[A-Za-z0-9]+__")

# Reserved names in the execution namespace that user-supplied `variables`
# keys must not collide with.
RESERVED_NAMES = frozenset({"__builtins__", "print", "context", "result"})


@dataclass(frozen=True)
class Violation:
    line: int
    column: int
    message: str


def check_code(source: str) -> list[Violation]:
    """Parse `source` and return a list of security violations.

    An empty list means the code passed static analysis; it does NOT mean
    the code is safe to run with unrestricted builtins — the caller must
    still execute it inside the resource-limited subprocess with the
    restricted builtins namespace (see sandbox_runner.py). This function is
    a pre-execution gate, not a full sandbox by itself.
    """
    try:
        tree = ast.parse(source, filename="<script>", mode="exec")
    except SyntaxError as exc:
        return [
            Violation(
                line=exc.lineno or 0,
                column=(exc.offset or 1) - 1,
                message=f"SyntaxError: {exc.msg}",
            )
        ]

    violations: list[Violation] = []

    for node in ast.walk(tree):
        if isinstance(node, ast.ClassDef):
            violations.append(
                _violation(node, "Class definitions are not permitted (v1 scope: pure functions only).")
            )
        elif isinstance(node, (ast.Import, ast.ImportFrom)):
            violations.extend(_check_import(node))
        elif isinstance(node, ast.Name):
            violations.extend(_check_name(node))
        elif isinstance(node, ast.Attribute):
            violations.extend(_check_attribute(node))
        elif isinstance(node, ast.Constant) and isinstance(node.value, str):
            violations.extend(_check_string_constant(node))

    return violations


def is_safe(source: str) -> bool:
    return len(check_code(source)) == 0


def _violation(node: ast.AST, message: str) -> Violation:
    return Violation(
        line=getattr(node, "lineno", 0),
        column=getattr(node, "col_offset", 0),
        message=message,
    )


def _check_import(node: ast.Import | ast.ImportFrom) -> list[Violation]:
    violations: list[Violation] = []
    if isinstance(node, ast.ImportFrom):
        if node.level and node.level > 0:
            violations.append(_violation(node, "Relative imports are not permitted."))
            return violations
        module_names = [node.module or ""]
    else:
        module_names = [alias.name for alias in node.names]

    for full_name in module_names:
        top_level = full_name.split(".")[0]
        if top_level in EXPLICITLY_FORBIDDEN_MODULES or full_name in EXPLICITLY_FORBIDDEN_MODULES:
            violations.append(
                _violation(node, f"Import of '{full_name}' is forbidden (filesystem/process/network-capable module).")
            )
        elif top_level not in ALLOWED_MODULES:
            violations.append(
                _violation(
                    node,
                    f"Import of '{full_name}' is not permitted. Allowed modules: {sorted(ALLOWED_MODULES)}",
                )
            )
    return violations


def _check_name(node: ast.Name) -> list[Violation]:
    if node.id in DENIED_BUILTIN_NAMES:
        return [_violation(node, f"Use of '{node.id}' is not permitted in CRM scripts.")]
    if _DUNDER_RE.match(node.id):
        return [_violation(node, f"Access to dunder name '{node.id}' is not permitted.")]
    return []


def _check_attribute(node: ast.Attribute) -> list[Violation]:
    if _DUNDER_RE.match(node.attr):
        return [_violation(node, f"Access to dunder attribute '.{node.attr}' is not permitted.")]
    return []


def _check_string_constant(node: ast.Constant) -> list[Violation]:
    if _DUNDER_SUBSTRING_RE.search(node.value):
        return [
            _violation(
                node,
                "String literal contains a dunder-like pattern (e.g. '__class__'), "
                "which is disallowed as a defense-in-depth measure against attribute-traversal escapes.",
            )
        ]
    return []

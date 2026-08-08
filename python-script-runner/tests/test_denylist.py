"""
Unit tests for denylist.py — the AST-based static security gate for the
CRM Python script sidecar (REV-STUB-008).

Run with: python3 -m pytest tests/test_denylist.py -v
(from the python-script-runner/ directory, with pytest installed —
see requirements-dev.txt)
"""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from denylist import check_code, is_safe  # noqa: E402


class TestAllowedCode:
    def test_simple_arithmetic_is_safe(self):
        assert is_safe("result = 2 + 3")

    def test_function_definition_is_safe(self):
        assert is_safe(
            """
def calculate(x, y):
    return x + y
result = calculate(5, 3)
"""
        )

    def test_allowed_stdlib_import_is_safe(self):
        assert is_safe("import math\nresult = math.sqrt(16)")

    def test_multiple_allowed_imports_are_safe(self):
        assert is_safe("import json\nimport datetime\nresult = json.dumps({'a': 1})")

    def test_from_import_of_allowed_module_is_safe(self):
        assert is_safe("from collections import Counter\nresult = Counter('aab')")

    def test_loops_and_comprehensions_are_safe(self):
        assert is_safe("result = [x * 2 for x in range(10) if x % 2 == 0]")

    def test_string_formatting_without_dunder_is_safe(self):
        assert is_safe("name = 'world'\nresult = f'hello {name}'")

    def test_try_except_is_safe(self):
        assert is_safe(
            """
try:
    result = 1 / 0
except ZeroDivisionError:
    result = None
"""
        )


class TestForbiddenImports:
    def test_import_os_is_rejected(self):
        assert not is_safe("import os")

    def test_import_sys_is_rejected(self):
        assert not is_safe("import sys")

    def test_import_subprocess_is_rejected(self):
        assert not is_safe("import subprocess")

    def test_import_socket_is_rejected(self):
        assert not is_safe("import socket")

    def test_import_importlib_is_rejected(self):
        assert not is_safe("import importlib")

    def test_import_ctypes_is_rejected(self):
        assert not is_safe("import ctypes")

    def test_import_shutil_is_rejected(self):
        assert not is_safe("import shutil")

    def test_import_pathlib_is_rejected(self):
        assert not is_safe("import pathlib")

    def test_from_os_import_is_rejected(self):
        assert not is_safe("from os import system")

    def test_unlisted_module_is_rejected_by_allowlist(self):
        # Not explicitly forbidden, but also not in ALLOWED_MODULES —
        # the allowlist enforcement should still reject it.
        assert not is_safe("import antigravity")

    def test_relative_import_is_rejected(self):
        assert not is_safe("from . import something")

    def test_forbidden_import_message_mentions_module_name(self):
        violations = check_code("import os")
        assert any("os" in v.message for v in violations)


class TestForbiddenBuiltins:
    def test_open_call_is_rejected(self):
        assert not is_safe("open('/etc/passwd').read()")

    def test_eval_call_is_rejected(self):
        assert not is_safe("eval('1+1')")

    def test_exec_call_is_rejected(self):
        assert not is_safe("exec('x = 1')")

    def test_dunder_import_call_is_rejected(self):
        assert not is_safe("__import__('os')")

    def test_compile_call_is_rejected(self):
        assert not is_safe("compile('1+1', '<s>', 'eval')")

    def test_aliasing_dangerous_builtin_is_rejected(self):
        # `f = eval; f(...)` — referencing the name without calling it must
        # still be rejected, not just direct calls.
        assert not is_safe("f = eval\nresult = f('1+1')")

    def test_getattr_is_rejected(self):
        assert not is_safe("result = getattr([], 'append')")

    def test_globals_call_is_rejected(self):
        assert not is_safe("result = globals()")


class TestSandboxEscapeAttempts:
    def test_class_dunder_traversal_is_rejected(self):
        assert not is_safe("result = ().__class__.__bases__[0].__subclasses__()")

    def test_dunder_attribute_access_is_rejected(self):
        assert not is_safe("result = (1).__class__")

    def test_dunder_class_name_reference_is_rejected(self):
        assert not is_safe("result = __class__")

    def test_class_definition_is_rejected(self):
        assert not is_safe("class Foo:\n    pass")

    def test_string_literal_dunder_pattern_is_rejected(self):
        # Defense-in-depth: format-string attribute traversal tricks like
        # "{0.__class__}".format(x) hide the dunder in a string constant,
        # not an AST Attribute node.
        assert not is_safe("template = '{0.__class__}'\nresult = template.format([])")

    def test_builtins_dunder_reference_is_rejected(self):
        assert not is_safe("result = __builtins__")


class TestSyntaxErrors:
    def test_invalid_syntax_returns_violation_not_exception(self):
        violations = check_code("def broken(x: = 5")
        assert len(violations) == 1
        assert "SyntaxError" in violations[0].message

    def test_invalid_syntax_does_not_raise(self):
        # Must not raise — the sidecar always wants a violation list back.
        check_code("this is not : valid python !!")


class TestViolationLocations:
    def test_violation_has_line_number(self):
        violations = check_code("x = 1\nimport os")
        assert violations[0].line == 2

    def test_empty_code_has_no_violations(self):
        assert check_code("") == []

    def test_check_code_returns_empty_list_for_safe_code(self):
        assert check_code("result = 1 + 1") == []

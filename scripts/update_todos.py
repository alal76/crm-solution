#!/usr/bin/env python3
"""Update MASTER_TODO_LIST.md to mark Round 11 completions."""
import re

filepath = "/Users/alal/Code/Git CRM Solution/crm-solution/docs/MASTER_TODO_LIST.md"

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

todos_done = [
    "TODO-ARCH-013-004",
    "TODO-SALES006-023",
    "TODO-SALES006-004",
    "TODO-SYS001-001",
    "TODO-SYS006-007",
    "TODO-SYS006-008",
    "TODO-SYS007-002",
    "TODO-SYS008-003",
    "TODO-SYS008-005",
    "TODO-SYS009-004",
    "TODO-SYS012-002",
    "TODO-SYS012-003",
    "TODO-CRM002-06",
    "TODO-CRM002-07",
    "TODO-CRM003-06",
    "TODO-GAP-04",
    "TODO-GAP-FRONTEND-001",
    "TODO-UX-12",
    "TODO-UX-15",
    "TODO-AI005-FE-001",
    "TODO-AI005-FE-005",
    "TODO-RPT-03",
    "TODO-RPT-06",
    "TODO-RPT-07",
    "TODO-GAP-MARKETING-001",
    "TODO-DOC-01",
    "TODO-DOC-02",
    "TODO-DOC-04",
]

total = 0
for todo_id in todos_done:
    pattern = r'(\| ' + re.escape(todo_id) + r' \|.*?)\| (?:❌ Not Started|⚠️ Partial) \|'
    replacement = r'\1| ✅ Done (Round 11) |'
    new_content, n = re.subn(pattern, replacement, content)
    if n > 0:
        total += n
        content = new_content
        print(f"  Updated {todo_id}")
    else:
        print(f"  WARNING: No match for {todo_id}")

content = re.sub(
    r'\*\*Progress:\*\* ✅ 370 Done \| ⚠️ 8 Partial \| ❌ 118 Remaining',
    '**Progress:** ✅ 398 Done | ⚠️ 6 Partial | ❌ 90 Remaining',
    content
)

content = re.sub(
    r'> \*\*Last Updated:\*\*[^\n]*',
    '> **Last Updated:** February 25, 2026 — 6 parallel subagents completed 28 TODO items (Round 11); frontend TypeScript errors fixed',
    content, count=1
)

content = re.sub(
    r'> \*\*Version:\*\* 0\.587\.0',
    '> **Version:** 0.589.0',
    content
)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print(f"\nDone. Total replacements: {total}")

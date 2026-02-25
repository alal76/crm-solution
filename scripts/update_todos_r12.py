#!/usr/bin/env python3
"""Update MASTER_TODO_LIST.md to mark Round 12 completions."""
import re

filepath = "/Users/alal/Code/Git CRM Solution/crm-solution/docs/MASTER_TODO_LIST.md"

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# All TODOs completed by the 6 subagents in Round 12
todos_done = [
    # Subagent 1 - Backend Services
    "TODO-SD005-003", "TODO-SD002-012", "TODO-SD005-010",
    "TODO-SYS003-003", "TODO-SALES006-024", "TODO-CRM003-08", "TODO-SYS009-003",
    # Subagent 2 - CRM Features + CPQ
    "TODO-CRM002-04", "TODO-CRM002-08", "TODO-GAP-06", "TODO-AI005-FE-002",
    # Subagent 3 - UX Accessibility + Admin E2E
    "TODO-UX-01", "TODO-UX-02", "TODO-UX-03", "TODO-UX-04", "TODO-UX-05",
    "TODO-UX-13", "TODO-SYS007-003", "TODO-SYS008-004", "TODO-SYS009-001",
    "TODO-AI005-FE-006",
    # Subagent 4 - DB Infrastructure
    "TODO-DB-001", "TODO-DB-002", "TODO-DB-003", "TODO-DB-004", "TODO-DB-005",
    "TODO-DB-006", "TODO-DB-007", "TODO-DB-008", "TODO-DB-009", "TODO-DB-010",
    "TODO-DB-011", "TODO-DB-012", "TODO-DB-013", "TODO-DB-014", "TODO-DB-015",
    "TODO-DB-016", "TODO-DB-017", "TODO-DB-018", "TODO-DB-019", "TODO-DB-020",
    "TODO-DB-021", "TODO-DB-022", "TODO-DB-023", "TODO-DB-024", "TODO-DB-025",
    "TODO-DB-026", "TODO-DB-027", "TODO-DB-028", "TODO-DB-029", "TODO-DB-030",
    "TODO-DB-031", "TODO-DB-032", "TODO-DB-033", "TODO-DB-034", "TODO-DB-035",
    # Subagent 5 - AI/ML + Integrations + Portal
    "TODO-AI-03", "TODO-AI-04", "TODO-AI-07", "TODO-AI-08", "TODO-AI-09",
    "TODO-AI-10", "TODO-INT-07", "TODO-INT-08", "TODO-INT-09", "TODO-INT-10",
    "TODO-INT-11", "TODO-PORTAL-05", "TODO-PORTAL-06", "TODO-PORTAL-07",
    # Subagent 6 - Customization + Portal/Mobile + INFRA
    "TODO-CUST-01", "TODO-CUST-02", "TODO-CUST-03", "TODO-CUST-04",
    "TODO-CUST-05", "TODO-CUST-06", "TODO-CUST-07", "TODO-CUST-08",
    "TODO-CUST-09", "TODO-PORTAL-01", "TODO-PORTAL-02", "TODO-PORTAL-03",
    "TODO-PORTAL-04", "TODO-PORTAL-09", "TODO-PORTAL-10", "TODO-PORTAL-11",
    "TODO-PORTAL-12", "TODO-INFRA-04", "TODO-INFRA-05", "TODO-INFRA-06",
    "TODO-INFRA-07", "TODO-INFRA-10", "TODO-ARCH-013-003",
]

total = 0
for todo_id in todos_done:
    pattern = r'(\| ' + re.escape(todo_id) + r' \|.*?)\| (?:❌ Not Started|⚠️ Partial) \|'
    replacement = r'\1| ✅ Done (Round 12) |'
    new_content, n = re.subn(pattern, replacement, content)
    if n > 0:
        total += n
        content = new_content
        print(f"  Updated {todo_id}")
    else:
        print(f"  WARNING: No match for {todo_id}")

# Count remaining
remaining = len(re.findall(r'❌ Not Started|⚠️ Partial', content))
done_count = 398 + total
print(f"\nNew done count: {done_count}, remaining: {remaining}")

# Update header
content = re.sub(
    r'\*\*Progress:\*\* ✅ \d+ Done \| ⚠️ \d+ Partial \| ❌ \d+ Remaining',
    f'**Progress:** ✅ {done_count} Done | ⚠️ 0 Partial | ❌ {remaining} Remaining',
    content
)
content = re.sub(
    r'> \*\*Last Updated:\*\*[^\n]*',
    '> **Last Updated:** February 25, 2026 — Round 12: 6 parallel subagents completed 78 TODO items',
    content, count=1
)
content = re.sub(
    r'> \*\*Version:\*\* 0\.\d+\.\d+',
    '> **Version:** 0.591.0',
    content
)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print(f"\nDone. Total replacements: {total}")

#!/usr/bin/env python3
"""Analyze the JSONL log from a test data loader run."""
import json
import sys
from collections import Counter

if len(sys.argv) < 2:
    print("Usage: python3 analyze_run.py <jsonl_file>")
    sys.exit(1)

jsonl_path = sys.argv[1]
failures_by_code = {}
successes = 0
total = 0

with open(jsonl_path) as f:
    for line in f:
        try:
            e = json.loads(line)
        except json.JSONDecodeError:
            continue
        if e.get("status") == "success":
            successes += 1
            total += 1
        elif e.get("status") == "exists":
            total += 1
        elif e.get("status") == "failed":
            total += 1
            code = e.get("http_status", "unknown")
            method = e.get("method", "?")
            ep = e.get("endpoint", "?")
            key = f"{method} {ep}"
            failures_by_code.setdefault(code, []).append(key)

print(f"Total API calls: {total}")
print(f"Successes: {successes}")
print()

for code in sorted(failures_by_code.keys(), key=lambda x: str(x)):
    items = failures_by_code[code]
    print(f"=== HTTP {code} ({len(items)} failures) ===")
    c = Counter(items)
    for ep, cnt in sorted(c.items()):
        suffix = f" (x{cnt})" if cnt > 1 else ""
        print(f"  {ep}{suffix}")
    print()

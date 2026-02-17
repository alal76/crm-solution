#!/usr/bin/env python3
import json, collections, sys, glob, os

# Find the latest log
log_dir = os.path.join(os.path.dirname(__file__), '..', 'logs', 'test-data')
logs = sorted(glob.glob(os.path.join(log_dir, '*.jsonl')))
if not logs:
    print("No JSONL logs found"); sys.exit(1)
logfile = logs[-1]
print(f"Analyzing: {logfile}\n")

statuses = collections.Counter()
fail_cats = collections.Counter()
fail_errors = []

with open(logfile) as f:
    for line in f:
        d = json.loads(line)
        s = d.get('status', '')
        statuses[s] += 1
        if s not in ('success', 'skip', 'event', None, ''):
            ep = d.get('endpoint', '?')
            hs = d.get('http_status', '?')
            fail_cats[(ep, hs)] += 1
            err = d.get('error', '')
            if err:
                fail_errors.append((ep, hs, err[:200]))

print("=== Status counts ===")
for k, v in statuses.most_common():
    print(f"  {v:4d}  {k}")

print(f"\n=== Failure categories (total: {sum(fail_cats.values())}) ===")
for (ep, hs), v in fail_cats.most_common():
    print(f"  {v:3d}  {ep}  [{hs}]")

print(f"\n=== Unknown failures (no endpoint) ===")
with open(logfile) as f:
    for line in f:
        d = json.loads(line)
        if d.get('status') == 'failed' and not d.get('endpoint'):
            print(f"  {d.get('summary','')[:150]}")
            if d.get('error'):
                print(f"    ERR: {d.get('error','')[:150]}")

print(f"\n=== Skipped entries ===")
with open(logfile) as f:
    for line in f:
        d = json.loads(line)
        if d.get('status') == 'skipped':
            print(f"  {d.get('summary','')[:150]}")

print(f"\n=== Sample errors (first 20) ===")
for ep, hs, err in fail_errors[:20]:
    print(f"  [{hs}] {ep}")
    print(f"        {err}")
    print()

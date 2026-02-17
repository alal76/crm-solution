#!/usr/bin/env python3
"""Quick diagnostic: pull docker_diagnostics.api_logs for a specific endpoint."""
import json, sys, glob, os

log_dir = os.path.join(os.path.dirname(__file__), '..', 'logs', 'test-data')
logs = sorted(glob.glob(os.path.join(log_dir, '*.jsonl')))
logfile = logs[-1]

endpoint = sys.argv[1] if len(sys.argv) > 1 else '/api/opportunities'
print(f"Checking: {logfile}")
print(f"Endpoint: {endpoint}\n")

with open(logfile) as f:
    for line in f:
        d = json.loads(line)
        if d.get('endpoint') == endpoint and d.get('status') == 'failed':
            snap = d.get('docker_diagnostics', {})
            api = snap.get('api_logs', '')
            print("=== API LOGS (last 30 lines) ===")
            for l in api.split('\n')[-30:]:
                print(l)
            print("\n=== DB LOGS (last 10 lines) ===")
            db = snap.get('db_logs', '')
            for l in db.split('\n')[-10:]:
                print(l)
            break

#!/usr/bin/env python3
"""Detailed analysis of test data loader failures"""

import json
from collections import defaultdict

failures = []
with open("/Users/alal/Code/Git CRM Solution/crm-solution/logs/test-data-load/test_data_load_20260222_180038.jsonl", "r") as f:
    for line in f:
        try:
            record = json.loads(line)
            if record.get("status") == "failed":
                failures.append(record)
        except json.JSONDecodeError:
            pass

# Categorize failures
json_errors = []
http_500_errors = []
http_400_errors = []
validation_errors = []

for record in failures:
    error = record.get("error", "")
    if "JSONDecodeError" in error or "Expecting" in error:
        json_errors.append(record)
    elif "HTTP Error 500" in error:
        http_500_errors.append(record)
    elif "HTTP Error 400" in error:
        http_400_errors.append(record)
    elif "expected >=" in error or "got 0" in error:
        validation_errors.append(record)

print("=" * 100)
print("TEST DATA LOADER FAILURE ANALYSIS")
print("=" * 100)
print(f"\nTotal Failures: {len(failures)}")
print(f"  - JSON Parse Errors: {len(json_errors)}")
print(f"  - HTTP 500 Server Errors: {len(http_500_errors)}")
print(f"  - HTTP 400 Bad Request Errors: {len(http_400_errors)}")
print(f"  - Validation/Count Errors: {len(validation_errors)}")

# Detailed breakdown
print("\n" + "=" * 100)
print("1. JSON PARSE ERRORS (Malformed Test Data)")
print("=" * 100)
for record in json_errors:
    print(f"\nPhase: {record.get('phase', '?')}")
    print(f"Endpoint: {record.get('endpoint', '?')}")
    error = record.get("error", "")
    # Extract the line number
    if "line 129" in error:
        print(f"Issue: Malformed JSON in source data - Extra comma or missing element at line 129")
        print(f"      Character position: 13680")
    print(f"Full error (excerpt): {error[:200]}...")

print("\n" + "=" * 100)
print("2. HTTP 500 SERVER ERRORS (Backend Issues)")
print("=" * 100)

# Group by endpoint
http500_by_endpoint = defaultdict(list)
for record in http_500_errors:
    endpoint = record.get("endpoint", "unknown")
    http500_by_endpoint[endpoint].append(record)

for endpoint in sorted(http500_by_endpoint.keys()):
    records = http500_by_endpoint[endpoint]
    print(f"\n{endpoint}: {len(records)} failures")
    if len(records) > 0:
        print(f"  Source: {records[0].get('data_file', '?')}[{records[0].get('data_index', '?')}]")

print("\n" + "=" * 100)
print("3. VALIDATION/COUNT ERRORS (Data Not Persisted)")
print("=" * 100)

val_by_endpoint = defaultdict(list)
for record in validation_errors:
    endpoint = record.get("endpoint", "unknown")
    val_by_endpoint[endpoint].append(record)

for endpoint in sorted(val_by_endpoint.keys()):
    records = val_by_endpoint[endpoint]
    print(f"\n{endpoint}: {len(records)} failure(s)")
    for r in records:
        print(f"  {r.get('error', '?')}")
        print(f"  Phase: {r.get('phase', '?')}")

print("\n" + "=" * 100)
print("4. HTTP 400 BAD REQUEST ERRORS")
print("=" * 100)

for record in http_400_errors:
    endpoint = record.get("endpoint", "?")
    phase = record.get("phase", "?")
    print(f"\n{endpoint} (Phase: {phase})")
    print(f"  Source: {record.get('data_file', '?')}[{record.get('data_index', '?')}]")

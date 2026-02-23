#!/usr/bin/env python3
"""Analyze test data loader failures from JSONL log"""

import json
from collections import defaultdict

failures = []
empty_records = []

with open("/Users/alal/Code/Git CRM Solution/crm-solution/logs/test-data-load/test_data_load_20260222_180038.jsonl", "r") as f:
    for line_num, line in enumerate(f, 1):
        try:
            record = json.loads(line)
            status = record.get("status", "")
            if status == "failed":
                failures.append((line_num, record))
            elif status == "":
                empty_records.append((line_num, record))
        except json.JSONDecodeError as e:
            pass

print(f"Total failures: {len(failures)}")
print(f"Total empty: {len(empty_records)}\n")

# Group failures by endpoint and status code
error_groups = defaultdict(list)

for line_num, record in failures:
    endpoint = record.get("endpoint", "?")
    status_code = record.get("status_code", "?")
    error = record.get("error", "")
    
    # Use endpoint + code as group key
    key = f"{endpoint}::{status_code}"
    error_groups[key].append((line_num, record))

print("=" * 80)
print("FAILURES GROUPED BY ENDPOINT AND STATUS CODE")
print("=" * 80 + "\n")

for key in sorted(error_groups.keys()):
    group = error_groups[key]
    endpoint, code = key.split("::")
    print(f"{endpoint} -> {code}: {len(group)} failures")
    
    # Show first error
    if group:
        err = group[0][1].get("error", "")
        if err:
            # Handle special errors
            if "JSONDecodeError" in err or "JSON" in err:
                print(f"  Type: JSON Parse Error")
                # Extract the specific error details
                if "Expecting" in err:
                    idx = err.find("Expecting")
                    print(f"  Details: {err[idx:idx+100]}")
            else:
                print(f"  Sample error: {err[:150]}")
    
    # Show data source if available
    if group:
        data_file = group[0][1].get("data_file", "")
        if data_file:
            print(f"  From file: {data_file}")
    print()


# Now show detailed breakdown
print("\n" + "=" * 80)
print("DETAILED FAILURE BREAKDOWN")
print("=" * 80 + "\n")

for key in sorted(error_groups.keys()):
    group = error_groups[key]
    if len(group) > 0:
        endpoint, code = key.split("::")
        print(f"\n--- {endpoint} (Status {code}) - {len(group)} failures ---")
        
        for idx, (line_num, record) in enumerate(group[:3]):  # Show first 3
            print(f"\n  [{idx+1}] Line {line_num}")
            data_file = record.get("data_file", "?")
            data_index = record.get("data_index", "?")
            print(f"      Source: {data_file}[{data_index}]")
            
            error = record.get("error", "")
            if error:
                # Truncate long errors
                if len(error) > 200:
                    print(f"      Error: {error[:200]}...")
                else:
                    print(f"      Error: {error}")

print("\n\n" + "=" * 80)
print(f"SUMMARY: Total failures = {len(failures)}")
print("=" * 80)

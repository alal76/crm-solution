#!/usr/bin/env python3
"""
Fix for bulk_crm_seed.json - Add missing closing brackets

This script repairs the JSON file by appending the missing brackets.
"""

import json
import os

FILE_PATH = "/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/bulk_crm_seed.json"

# Read the current file
with open(FILE_PATH, 'r') as f:
    content = f.read()

# Check if it already ends with closing brackets
if content.rstrip().endswith('}'):
    print("✓ File already has closing braces - no fix needed")
    exit(0)

# The file is missing: closing bracket for notes array + closing brace for root object
fixing_content = content.rstrip() + '\n\t]\n}\n'

# Verify the fix by parsing
try:
    json.loads(fixing_content)
    print("✓ After fix: JSON parses successfully!")
except json.JSONDecodeError as e:
    print(f"✗ After fix: JSON still invalid: {e}")
    exit(1)

# Backup original
backup_path = FILE_PATH + '.backup'
with open(backup_path, 'w') as f:
    f.write(content)
print(f"✓ Backup created: {backup_path}")

# Write the fixed file
with open(FILE_PATH, 'w') as f:
    f.write(fixing_content)

print(f"✓ Fixed file written: {FILE_PATH}")

# Verify by parsing
try:
    data = json.load(open(FILE_PATH))
    print(f"✓ Verification: File now contains valid JSON with {len(data)} top-level keys")
    for key in data.keys():
        count = len(data[key]) if isinstance(data[key], list) else "N/A"
        print(f"  - {key}: {count} items")
except json.JSONDecodeError as e:
    print(f"✗ Verification failed: {e}")
    exit(1)

print("\n✅ File successfully repaired!")

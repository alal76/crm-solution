#!/usr/bin/env python3
"""
AP-029, AP-030, AP-031: Add CancellationToken propagation to SaveChangesAsync() in controllers.
Controllers have HttpContext.RequestAborted available; use it directly.
AP-028 (CommunicationService) deferred - requires interface signature changes.
"""
import re

BASE = "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend/src/CRM.Api/Controllers"
SRC  = "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend/src/CRM.Infrastructure/Services"

FILES = [
    (f"{BASE}/MasterDataController.cs",  "AP-029"),
    (f"{BASE}/UsersController.cs",       "AP-030"),
    (f"{BASE}/ApiUsersController.cs",    "AP-031"),
]

PATTERN = re.compile(r'\bSaveChangesAsync\(\)')
REPLACEMENT = "SaveChangesAsync(HttpContext.RequestAborted)"

for path, ap_id in FILES:
    with open(path, "r") as f:
        content = f.read()
    new_content, count = PATTERN.subn(REPLACEMENT, content)
    if count > 0:
        with open(path, "w") as f:
            f.write(new_content)
        print(f"  {ap_id}: Replaced {count} SaveChangesAsync() calls in {path.split('/')[-1]}")
    else:
        print(f"  {ap_id}: No SaveChangesAsync() found (already fixed?) in {path.split('/')[-1]}")

# AP-028: CommunicationService - add cancellationToken = default to method signatures
# This requires interface updates too. Apply conservative fix only:
# Replace SaveChangesAsync() with SaveChangesAsync(cancellationToken) only where the enclosing
# method already has a CancellationToken parameter. Since none do, skip for now.
print("\nAP-028 (CommunicationService): DEFERRED - requires interface signature changes across ICommunicationService.")
print("Methods have no CancellationToken parameters; adding them requires interface update (separate PR).")

print("\nDone.")

#!/usr/bin/env python3
import subprocess
import sys

commit_message = """fix(ENUM): Fix seed data category mismatch and add migration scripts v0.600.18

FIXES:
- Moved 69 timezone items from LeadStatus (ID=2) to correct Timezones category
  Root cause: Multiple seed files with conflicting hardcoded category IDs
- Re-migrated Leads.StatusId using Key-based mapping (correct values: NEW/NURTURE)
- Fix inline # nosonar comment in deploy-to-dev-server.sh that broke docker run

NEW MIGRATION SCRIPTS (applied to crm_db 2026-02-27):
- database/migrations/SYS-009-DataMigration-Fixed.sql
  Migrates Leads.StatusId (SortOrder offset) and Opportunities.StageId (Key-based)
- database/migrations/SYS-009-ServiceRequest-Fix.sql
  Creates ServiceRequestStatus/Priority categories + items, migrates FKs
- database/migrations/SYS-009-Fix-Seed-Data-Categories.sql
  Fixes timezone items wrongly in LeadStatus, re-runs Lead StatusId migration

VERIFICATION (after all fixes):
- Leads.StatusId: 0 NULL — 221 NEW + 10 NURTURE (correct LeadStatus values)
- Opportunities.StageId: 0 NULL (correct OpportunityStage values)
- ServiceRequests.StatusId: 0 NULL (correct ServiceRequestStatus values)
- ServiceRequests.PriorityId: 0 NULL (correct ServiceRequestPriority values)

DEPLOYMENT STATUS:
- crm-api:      running healthy at http://192.168.0.9:5000/health
- crm-frontend: running healthy at http://192.168.0.9:80
- Database:     all 648 FK values correct, zero NULLs

Bump version: 0.600.17 to 0.600.18"""

result = subprocess.run(
    ['git', '-C', '/Users/alal/Code/Git CRM Solution/crm-solution', 'commit', '-m', commit_message],
    capture_output=True, text=True
)
print("STDOUT:", result.stdout)
print("STDERR:", result.stderr)
print("Return code:", result.returncode)

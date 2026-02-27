#!/usr/bin/env python3
import subprocess
import sys

commit_message = """feat(ENUM): Database-driven enum migration v0.600.17

SCHEMA CHANGES:
- Enhanced LookupCategories: EntityType, PropertyName, IsSystemManaged, AllowCustomValues, ValidationSchema
- Enhanced LookupItems: IsDefault, IsSystemValue, Color, Icon, ValidationRules
- Created EnumTransitions table for state machine rules with role-based access
- Added FK columns: Leads.StatusId, Opportunities.StageId, ServiceRequests.StatusId/PriorityId
- Added FK constraints and performance indexes on all new FK columns

DATA MIGRATION (648 records migrated):
- 231 Leads: Status (enum int) to StatusId (FK to LeadStatus lookup items)
- 230 Opportunities: Stage (enum int) to StageId (FK to OpportunityStage lookup items)
- 187 ServiceRequests: Status/Priority (enum ints) to StatusId/PriorityId (FK to lookup items)

NEW LOOKUP CATEGORIES:
- ServiceRequestStatus: 8 values (NEW, OPEN, IN_PROGRESS, PENDING, ON_HOLD, RESOLVED, CLOSED, CANCELLED)
- ServiceRequestPriority: 4 values (LOW, MEDIUM, HIGH, CRITICAL) with SLA metadata

MIGRATION FILES:
- database/migrations/20260227_enum_schema_enhancements.sql
- database/migrations/20260227_servicerequest_categories.sql
- database/migrations/20260227_entity_fk_migration.sql
- database/migrations/README_ENUM_MIGRATION.md

BACKWARD COMPATIBILITY:
- Old enum columns (Status, Stage, Priority) remain unchanged
- All existing API endpoints continue to work
- No breaking changes

BUILD STATUS:
- Backend: dotnet build SUCCESS (0 errors)
- Frontend: npm run build SUCCESS (0 errors)

Applied to dev server: 192.168.0.9:3306/crm_db
Bump version: 0.600.16 to 0.600.17"""

result = subprocess.run(
    ['git', '-C', '/Users/alal/Code/Git CRM Solution/crm-solution', 'commit', '-m', commit_message],
    capture_output=True, text=True
)
print("STDOUT:", result.stdout)
print("STDERR:", result.stderr)
print("Return code:", result.returncode)

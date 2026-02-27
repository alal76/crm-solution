#!/usr/bin/env python3
import subprocess
import sys

commit_message = """feat(admin): Full Enum Management Admin CRUD UI v0.600.19

BACKEND:
- LookupCategory entity: EntityType, PropertyName, IsSystemManaged, AllowCustomValues, ValidationSchema
- LookupItem entity: IsDefault, IsSystemValue, Color, Icon, ValidationRules
- CrmDbContext ModelBuilder config updated for new columns
- New LookupDtos.cs: 8 DTOs (Category/Item Create/Update/List/Detail + ReorderItemsDto)
- New EnumManagementController with 11 endpoints (Admin role):
  GET/POST/PUT/DELETE /api/enum-management/categories
  GET    /api/enum-management/categories/{id}/items
  POST   /api/enum-management/categories/{id}/items
  GET/PUT/DELETE /api/enum-management/items/{id}
  POST   /api/enum-management/categories/{id}/items/reorder

FRONTEND:
- New enumManagementService.ts: typed API service layer
- New EnumManagementPage.tsx:
  Categories: card grid with search, tabs (All/Active/Inactive), item count
  Items: table with key/value/color/sort/default/system badges
  CRUD for both categories and items with form dialogs
  Reorder items with up/down arrows
  System-managed categories and system values protected (read-only view)
  Color picker preview, single-default enforcement per category
- AdminSettingsMenu: Enum Management nav item added
- App.tsx: lazy import + route at /admin/enum-management
- Breadcrumbs.tsx: added enum-management segment

Bump version: 0.600.18 to 0.600.19"""

result = subprocess.run(
    ['git', '-C', '/Users/alal/Code/Git CRM Solution/crm-solution', 'commit', '-m', commit_message],
    capture_output=True, text=True
)
print("STDOUT:", result.stdout)
print("STDERR:", result.stderr)
print("Return code:", result.returncode)

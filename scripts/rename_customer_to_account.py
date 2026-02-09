#!/usr/bin/env python3
"""Rename Customer → Account terminology across the CRM Frontend."""
import os
import re

base = 'CRM.Frontend/src'
changes = []

def update_file(rel_path, old, new):
    fp = os.path.join(base, rel_path)
    if not os.path.exists(fp):
        return
    with open(fp, 'r') as f:
        content = f.read()
    if old not in content:
        return
    content = content.replace(old, new)
    with open(fp, 'w') as f:
        f.write(content)
    changes.append(f"  {rel_path}: '{old[:60]}' -> '{new[:60]}'")

# ===================================================================
# STEP 1: apiService.ts
# ===================================================================
print("=== Step 1: apiService.ts ===")
f = 'services/apiService.ts'

# Rename interface
update_file(f, 'export interface Customer extends BaseEntity {',
               'export interface Account extends BaseEntity {')

# Rename service object and add compat aliases
# Read file and do a more targeted replacement
fp = os.path.join(base, f)
with open(fp, 'r') as fh:
    content = fh.read()

# Replace customerService declaration
content = content.replace(
    'export const customerService = {',
    '/** @deprecated Use Account instead */\nexport type Customer = Account;\n\nexport const accountService = {')

# Replace generic types in the service block
content = content.replace("<Customer[]>('/customers')", "<Account[]>('/accounts')")
content = content.replace("<Customer>(`/customers/", "<Account>(`/accounts/")
content = content.replace("<Customer[]>(`/customers/", "<Account[]>(`/accounts/")
content = content.replace("<Customer>('/customers',", "<Account>('/accounts',")
content = content.replace("(data: Customer)", "(data: Account)")
content = content.replace("(id: number, data: Customer)", "(id: number, data: Account)")
content = content.replace("apiClient.put(`/customers/", "apiClient.put(`/accounts/")
content = content.replace("apiClient.delete(`/customers/", "apiClient.delete(`/accounts/")

# Find the closing of the service object and add alias
# The service object for customer ends with }; followed by blank + next section
# Add compat alias: look for first }; after accountService start
# We'll use regex: find the }; that closes accountService and add alias
content = re.sub(
    r'(export const accountService = \{[^}]+(?:\{[^}]*\}[^}]*)*\};)',
    r'\1\n\n/** @deprecated Use accountService instead */\nexport const customerService = accountService;',
    content, count=1)

# Rename getByCustomer methods (keep API paths since backend uses /customer/)
content = content.replace('getByCustomer: (accountId: number)', 'getByAccount: (accountId: number)')

with open(fp, 'w') as fh:
    fh.write(content)
changes.append(f"  {f}: full rewrite")

# ===================================================================
# STEP 2: EntityContext.tsx
# ===================================================================
print("=== Step 2: EntityContext.tsx ===")
f = 'contexts/EntityContext.tsx'
update_file(f, "| 'Customer'", "| 'Account'")

fp = os.path.join(base, f)
with open(fp, 'r') as fh:
    content = fh.read()
content = content.replace("/customers/", "/accounts/")
content = content.replace("/customers$", "/accounts$")
content = content.replace("entityType: 'Customer'", "entityType: 'Account'")
with open(fp, 'w') as fh:
    fh.write(content)
changes.append(f"  {f}: route patterns updated")

# ===================================================================
# STEP 3: contactInfoService.ts
# ===================================================================
print("=== Step 3: contactInfoService.ts ===")
update_file('services/contactInfoService.ts',
    "export type EntityType = 'Customer' | 'Contact' | 'Lead' | 'Account';",
    "export type EntityType = 'Account' | 'Contact' | 'Lead';")

# ===================================================================
# STEP 4: EntitySelect.tsx
# ===================================================================
print("=== Step 4: EntitySelect.tsx ===")
f = 'components/EntitySelect.tsx'
fp = os.path.join(base, f)
if os.path.exists(fp):
    with open(fp, 'r') as fh:
        content = fh.read()
    # Remove 'customer' from EntityType, keep 'account'
    content = content.replace(
        "'customer' | 'contact' | 'product' | 'opportunity' | 'user' | 'account'",
        "'account' | 'contact' | 'product' | 'opportunity' | 'user'")
    content = content.replace("endpoint: '/customers'", "endpoint: '/accounts'")
    content = content.replace("case 'customer':", "case 'account':")
    content = content.replace("label: 'Customer'", "label: 'Account'")
    content = content.replace("Customer", "Account")  # Careful - only in config contexts
    with open(fp, 'w') as fh:
        fh.write(content)
    changes.append(f"  {f}: entity types updated")

# ===================================================================
# STEP 5: navigationConfig.ts
# ===================================================================
print("=== Step 5: navigationConfig.ts ===")
f = 'config/navigationConfig.ts'
fp = os.path.join(base, f)
if os.path.exists(fp):
    with open(fp, 'r') as fh:
        content = fh.read()
    content = content.replace("id: 'customers'", "id: 'accounts'")
    content = content.replace("label: 'Customers'", "label: 'Accounts'")
    content = content.replace("path: '/customers'", "path: '/accounts'")
    content = content.replace("description: 'Customer account management'",
                              "description: 'Account management'")
    content = content.replace("id: 'customer-overview'", "id: 'account-overview'")
    content = content.replace("label: 'Customer Overview'", "label: 'Account Overview'")
    content = content.replace("path: '/customer-overview'", "path: '/account-overview'")
    content = content.replace("description: 'Consolidated customer view'",
                              "description: 'Consolidated account view'")
    with open(fp, 'w') as fh:
        fh.write(content)
    changes.append(f"  {f}: nav items renamed")

# ===================================================================
# STEP 6: Breadcrumbs.tsx
# ===================================================================
print("=== Step 6: Breadcrumbs.tsx ===")
f = 'components/Breadcrumbs.tsx'
fp = os.path.join(base, f)
if os.path.exists(fp):
    with open(fp, 'r') as fh:
        content = fh.read()
    content = content.replace("'/customers'", "'/accounts'")
    content = content.replace("label: 'Customers'", "label: 'Accounts'")
    content = content.replace("'/customer-overview'", "'/account-overview'")
    content = content.replace("'Customer Overview'", "'Account Overview'")
    with open(fp, 'w') as fh:
        fh.write(content)
    changes.append(f"  {f}: breadcrumb paths updated")

# ===================================================================
# STEP 7: Navigation.tsx
# ===================================================================
print("=== Step 7: Navigation.tsx ===")
f = 'components/Navigation.tsx'
fp = os.path.join(base, f)
if os.path.exists(fp):
    with open(fp, 'r') as fh:
        content = fh.read()
    content = content.replace("'/customers'", "'/accounts'")
    content = content.replace("'customers':", "'accounts':")
    content = content.replace("menuName: 'Customers'", "menuName: 'Accounts'")
    content = content.replace("path: '/customer-overview'", "path: '/account-overview'")
    content = content.replace("menuName: 'Customer Overview'", "menuName: 'Account Overview'")
    with open(fp, 'w') as fh:
        fh.write(content)
    changes.append(f"  {f}: nav paths updated")

# ===================================================================
# STEP 8: App.tsx
# ===================================================================
print("=== Step 8: App.tsx ===")
f = 'App.tsx'
fp = os.path.join(base, f)
if os.path.exists(fp):
    with open(fp, 'r') as fh:
        content = fh.read()
    content = content.replace('// Customer Module - Lazy Loaded',
                              '// Account Module - Lazy Loaded')
    content = content.replace(
        "const CustomersPage = lazy(() => import('./pages/CustomersPage'));",
        "const AccountsPage = lazy(() => import('./pages/CustomersPage'));")
    content = content.replace(
        "const CustomerOverviewPage = lazy(() => import('./pages/CustomerOverviewPage'));",
        "const AccountOverviewPage = lazy(() => import('./pages/CustomerOverviewPage'));")
    content = content.replace('path="/customers"', 'path="/accounts"')
    content = content.replace('<CustomersPage />', '<AccountsPage />')
    content = content.replace('path="/customer-overview"', 'path="/account-overview"')
    content = content.replace('<CustomerOverviewPage />', '<AccountOverviewPage />')
    with open(fp, 'w') as fh:
        fh.write(content)
    changes.append(f"  {f}: routes updated")

# ===================================================================
# STEP 9: RelatedEntitiesPanel.tsx
# ===================================================================
print("=== Step 9: RelatedEntitiesPanel.tsx ===")
f = 'components/common/RelatedEntitiesPanel.tsx'
fp = os.path.join(base, f)
if os.path.exists(fp):
    with open(fp, 'r') as fh:
        content = fh.read()
    content = content.replace("'/customers'", "'/accounts'")
    content = content.replace("`/customers/", "`/accounts/")
    with open(fp, 'w') as fh:
        fh.write(content)
    changes.append(f"  {f}: paths updated")

# ===================================================================
# STEP 10: DashboardPage.tsx
# ===================================================================
print("=== Step 10: DashboardPage.tsx ===")
f = 'pages/DashboardPage.tsx'
fp = os.path.join(base, f)
if os.path.exists(fp):
    with open(fp, 'r') as fh:
        content = fh.read()
    # Import rename
    content = content.replace(
        "customerService, Customer",
        "accountService, Account")
    # State type - be specific to avoid over-matching
    content = content.replace("customers: Customer[]", "accounts: Account[]")
    content = content.replace("customers: [] as Customer[]", "accounts: [] as Account[]")
    content = content.replace("customerService.getAll()", "accountService.getAll()")
    # Data field references
    content = re.sub(r'\bdata\.customers\b', 'data.accounts', content)
    # Navigation links
    content = content.replace("'/customers'", "'/accounts'")
    content = content.replace("menuKey: 'Customers'", "menuKey: 'Accounts'")
    with open(fp, 'w') as fh:
        fh.write(content)
    changes.append(f"  {f}: imports and references updated")

# ===================================================================
# STEP 11: CustomersPage.tsx → update API paths and imports
# ===================================================================
print("=== Step 11: CustomersPage.tsx ===")
f = 'pages/CustomersPage.tsx'
fp = os.path.join(base, f)
if os.path.exists(fp):
    with open(fp, 'r') as fh:
        content = fh.read()
    # Update API paths from /customers to /accounts
    content = content.replace("'/customers'", "'/accounts'")
    content = content.replace("'/customers/", "'/accounts/")
    content = content.replace("`/customers`", "`/accounts`")
    content = content.replace("`/customers/", "`/accounts/")
    content = content.replace("'/customers?", "'/accounts?")
    # Update constant imports
    content = content.replace("CUSTOMER_TYPE_OPTIONS", "ACCOUNT_TYPE_OPTIONS")
    content = content.replace("CUSTOMER_TYPES", "ACCOUNT_TYPES")
    # Update entity type for SignalR
    content = content.replace("useEntityTypeSubscription('Customer'",
                              "useEntityTypeSubscription('Account'")
    with open(fp, 'w') as fh:
        fh.write(content)
    changes.append(f"  {f}: paths and imports updated")

# ===================================================================
# STEP 12: CustomerOverviewPage.tsx → update API paths and imports
# ===================================================================
print("=== Step 12: CustomerOverviewPage.tsx ===")
f = 'pages/CustomerOverviewPage.tsx'
fp = os.path.join(base, f)
if os.path.exists(fp):
    with open(fp, 'r') as fh:
        content = fh.read()
    content = content.replace("'/customers'", "'/accounts'")
    content = content.replace("'/customers/", "'/accounts/")
    content = content.replace("`/customers`", "`/accounts`")
    content = content.replace("`/customers/", "`/accounts/")
    content = content.replace("CUSTOMER_TYPE_OPTIONS", "ACCOUNT_TYPE_OPTIONS")
    content = content.replace("CUSTOMER_TYPES", "ACCOUNT_TYPES")
    with open(fp, 'w') as fh:
        fh.write(content)
    changes.append(f"  {f}: paths and imports updated")

# ===================================================================
# STEP 13: Other pages with /customers references
# ===================================================================
print("=== Step 13: Other pages ===")
pages = [
    'pages/OpportunitiesPage.tsx',
    'pages/LeadsPage.tsx',
    'pages/HelpPage.tsx',
    'pages/RelationshipsPage.tsx',
    'pages/ContactsPage.tsx',
    'pages/QuotesPage.tsx',
    'pages/InteractionsPage.tsx',
]
for p in pages:
    fp = os.path.join(base, p)
    if not os.path.exists(fp):
        continue
    with open(fp, 'r') as fh:
        content = fh.read()
    orig = content
    content = content.replace("'/customers'", "'/accounts'")
    content = content.replace("'/customers/", "'/accounts/")
    content = content.replace("'/customers?", "'/accounts?")
    content = content.replace("`/customers`", "`/accounts`")
    content = content.replace("`/customers/", "`/accounts/")
    if content != orig:
        with open(fp, 'w') as fh:
            fh.write(content)
        changes.append(f"  {p}: API paths updated")

# ===================================================================
# STEP 14: ActivitiesPage.tsx - activity type names
# ===================================================================
print("=== Step 14: ActivitiesPage.tsx ===")
f = 'pages/ActivitiesPage.tsx'
fp = os.path.join(base, f)
if os.path.exists(fp):
    with open(fp, 'r') as fh:
        content = fh.read()
    content = content.replace("'CustomerCreated'", "'AccountCreated'")
    content = content.replace("'CustomerUpdated'", "'AccountUpdated'")
    content = content.replace("'CustomerDeleted'", "'AccountDeleted'")
    content = content.replace("Customer Created", "Account Created")
    content = content.replace("Customer Updated", "Account Updated")
    content = content.replace("Customer Deleted", "Account Deleted")
    content = content.replace('value="Customer"', 'value="Account"')
    with open(fp, 'w') as fh:
        fh.write(content)
    changes.append(f"  {f}: activity types updated")

# ===================================================================
# STEP 15: ApiDocumentationPage.tsx
# ===================================================================
print("=== Step 15: ApiDocumentationPage.tsx ===")
f = 'pages/admin/ApiDocumentationPage.tsx'
fp = os.path.join(base, f)
if os.path.exists(fp):
    with open(fp, 'r') as fh:
        content = fh.read()
    content = content.replace("/api/customers", "/api/accounts")
    content = content.replace("'Customers'", "'Accounts'")
    content = content.replace("Customer Management", "Account Management")
    with open(fp, 'w') as fh:
        fh.write(content)
    changes.append(f"  {f}: API docs updated")

# ===================================================================
# STEP 16: constants.ts - remove legacy aliases  
# ===================================================================
print("=== Step 16: constants.ts ===")
f = 'utils/constants.ts'
fp = os.path.join(base, f)
if os.path.exists(fp):
    with open(fp, 'r') as fh:
        content = fh.read()
    # Update the CUSTOMER_TYPES alias to be marked deprecated
    content = content.replace(
        "export const CUSTOMER_TYPES = ACCOUNT_TYPES;",
        "/** @deprecated Use ACCOUNT_TYPES instead */\nexport const CUSTOMER_TYPES = ACCOUNT_TYPES;")
    content = content.replace(
        "export const CUSTOMER_TYPE_OPTIONS = ACCOUNT_TYPE_OPTIONS;",
        "/** @deprecated Use ACCOUNT_TYPE_OPTIONS instead */\nexport const CUSTOMER_TYPE_OPTIONS = ACCOUNT_TYPE_OPTIONS;")
    with open(fp, 'w') as fh:
        fh.write(content)
    changes.append(f"  {f}: deprecated annotations added")

# ===================================================================
# STEP 17: Test files
# ===================================================================
print("=== Step 17: Test files ===")
test_files = [
    '__tests__/CustomersPage.test.tsx',
    '__tests__/CustomersPage.comprehensive.test.tsx',
    '__tests__/Navigation.comprehensive.test.tsx',
    '__tests__/DashboardPage.comprehensive.test.tsx',
    '__tests__/SharedComponents.comprehensive.test.tsx',
    '__tests__/OpportunitiesPage.test.tsx',
    '__tests__/apiClient.test.ts',
    '__tests__/mocks/handlers.ts',
]
for tf in test_files:
    fp = os.path.join(base, tf)
    if not os.path.exists(fp):
        continue
    with open(fp, 'r') as fh:
        content = fh.read()
    orig = content
    content = content.replace("'/customers'", "'/accounts'")
    content = content.replace("'/customers/", "'/accounts/")
    content = content.replace("'/customers?", "'/accounts?")
    content = content.replace('"/customers"', '"/accounts"')
    content = content.replace('"/customers/', '"/accounts/')
    content = content.replace("`/customers`", "`/accounts`")
    content = content.replace("`/customers/", "`/accounts/")
    content = content.replace("/api/customers", "/api/accounts")
    content = content.replace("path: '/customers'", "path: '/accounts'")
    content = content.replace("'CUSTOMER_TYPE_OPTIONS'", "'ACCOUNT_TYPE_OPTIONS'")
    content = content.replace("CUSTOMER_TYPES", "ACCOUNT_TYPES")
    if content != orig:
        with open(fp, 'w') as fh:
            fh.write(content)
        changes.append(f"  {tf}: test paths updated")

# ===================================================================
# SUMMARY
# ===================================================================
print(f"\n{'='*60}")
print(f"Total files changed: {len(changes)}")
print(f"{'='*60}")
for c in changes:
    print(c)

#!/usr/bin/env python3
"""Fix remaining Customer → Account references in CRM Frontend."""

import os
import re

BASE = "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Frontend/src"
changes = []

def fix_file(rel_path, replacements):
    """Apply a list of (old, new) string replacements to a file."""
    path = os.path.join(BASE, rel_path)
    if not os.path.exists(path):
        print(f"  SKIP (not found): {rel_path}")
        return
    with open(path, 'r') as f:
        content = f.read()
    original = content
    for old, new in replacements:
        if old in content:
            content = content.replace(old, new, 1)  # Replace first occurrence only
            print(f"  ✅ Replaced in {rel_path}: {repr(old[:60])}...")
        else:
            print(f"  ⚠️  Not found in {rel_path}: {repr(old[:60])}...")
    if content != original:
        with open(path, 'w') as f:
            f.write(content)
        changes.append(rel_path)
    else:
        print(f"  — No changes in {rel_path}")


def fix_entityselect():
    """Fix EntitySelect.tsx - remove duplicate case branches with /customers."""
    path = os.path.join(BASE, "components/EntitySelect.tsx")
    with open(path, 'r') as f:
        content = f.read()
    original = content

    # Problem: The rename script created duplicate 'account' case branches.
    # Pattern 1 (fetchEntities): two case 'account' lines, first with /customers, second with /accounts
    # We need to remove the first one and keep the second
    # Looking for:
    #   case 'account': endpoint = '/customers'; break;
    #   case 'contact': endpoint = '/contacts'; break;
    #   ...
    #   case 'account': endpoint = '/accounts'; break;
    
    # Remove the line: "        case 'account': endpoint = '/customers'; break;\n"
    content = content.replace(
        "        case 'account': endpoint = '/customers'; break;\n",
        ""
    )
    
    # Also fix the second occurrence in the save/create function
    # Same pattern: case 'account' with endpoint = '/customers' followed by payload
    # We need to remove the entire duplicate case block for 'account' that uses '/customers'
    # Looking at the code, the duplicate has:
    #   case 'account':
    #     endpoint = '/customers';
    #     payload = { ...customerForm, customerCategory: 0, lifecycleStage: 0, };
    #     ...
    #     break;
    #   case 'account':
    #     endpoint = '/accounts';
    #     ...
    
    # Find and remove the first 'account' case block that uses '/customers' in the save function
    # This is more complex - need regex
    pattern = r"        case 'account':\n          endpoint = '/customers';\n          payload = \{[^}]*\};\n(?:.*?\n)*?          break;\n        case 'account':\n          endpoint = '/accounts';"
    
    match = re.search(pattern, content, re.DOTALL)
    if match:
        # Keep only the second case block (with /accounts)
        # Replace with just the second case 'account' line
        old_block = match.group(0)
        # Extract everything after the first "break;\n" to get the second case block start
        new_block = "        case 'account':\n          endpoint = '/accounts';"
        content = content.replace(old_block, new_block)
        print("  ✅ Removed duplicate case 'account' block with /customers in save function")
    else:
        # Try simpler approach - just find the block
        # Look for the pattern with customerForm payload
        old = """        case 'account':
          endpoint = '/customers';
          payload = {
            ...customerForm,
            customerCategory: 0, // Individual
            lifecycleStage: 0,
          };
          if (!payload.firstName && !payload.lastName && !payload.company) {
            throw new Error('Please provide at least a name or company');
          }
          break;
        case 'account':
          endpoint = '/accounts';"""
        new = """        case 'account':
          endpoint = '/accounts';"""
        if old in content:
            content = content.replace(old, new)
            print("  ✅ Removed duplicate case 'account' block (simple match)")
        else:
            print("  ⚠️  Could not find duplicate case block pattern - manual fix needed")

    if content != original:
        with open(path, 'w') as f:
            f.write(content)
        changes.append("components/EntitySelect.tsx")
        print("  ✅ EntitySelect.tsx fixed")
    else:
        print("  — No changes in EntitySelect.tsx")


def fix_customers_page():
    """Fix CustomersPage.tsx entityType props."""
    fix_file("pages/CustomersPage.tsx", [
        ('entityType="Customer"\n                    entityId={editingId}\n                    layout="tabs"',
         'entityType="Account"\n                    entityId={editingId}\n                    layout="tabs"'),
        ('entityType="Customer"\n                    entityId={editingId}\n                    entityName=',
         'entityType="Account"\n                    entityId={editingId}\n                    entityName='),
    ])


def fix_quotes_page():
    """Fix QuotesPage.tsx labels and entityType."""
    fix_file("pages/QuotesPage.tsx", [
        # PDF export label
        ("{ label: 'Customer', value: accountName }",
         "{ label: 'Account', value: accountName }"),
        # Table header
        ("<TableCell><strong>Customer</strong></TableCell>",
         "<TableCell><strong>Account</strong></TableCell>"),
        # EntitySelect entityType
        ('entityType="customer"\n                  name="accountId"\n                  value={formData.accountId}\n                  onChange={handleSelectChange}\n                  label="Customer"',
         'entityType="account"\n                  name="accountId"\n                  value={formData.accountId}\n                  onChange={handleSelectChange}\n                  label="Account"'),
    ])


def fix_service_requests_page():
    """Fix ServiceRequestsPage.tsx entityType and label."""
    fix_file("pages/ServiceRequestsPage.tsx", [
        ('entityType="customer"\n                name="accountId"\n                value={formData.accountId || \'\'}\n                onChange={(e: any) => handleFormChange(\'accountId\', e.target.value ? Number(e.target.value) : undefined)}\n                label="Customer"',
         'entityType="account"\n                name="accountId"\n                value={formData.accountId || \'\'}\n                onChange={(e: any) => handleFormChange(\'accountId\', e.target.value ? Number(e.target.value) : undefined)}\n                label="Account"'),
    ])


def fix_contacts_page():
    """Fix ContactsPage.tsx entityType and label."""
    fix_file("pages/ContactsPage.tsx", [
        # First occurrence - in add/edit dialog
        ('entityType="customer"\n                name="accountId"\n                value={formData.accountId || \'\'}\n                onChange={(e) => setFormData({ ...formData, accountId: e.target.value ? Number(e.target.value) : \'\' })}\n                label="Owner Customer"',
         'entityType="account"\n                name="accountId"\n                value={formData.accountId || \'\'}\n                onChange={(e) => setFormData({ ...formData, accountId: e.target.value ? Number(e.target.value) : \'\' })}\n                label="Owner Account"'),
        # Second occurrence - in bulk edit dialog
        ('entityType="customer"\n              name="accountId"\n              value={bulkFormData.accountId}\n              onChange={(e) => setBulkFormData(prev => ({ ...prev, accountId: e.target.value }))}\n              label="Owner Customer"',
         'entityType="account"\n              name="accountId"\n              value={bulkFormData.accountId}\n              onChange={(e) => setBulkFormData(prev => ({ ...prev, accountId: e.target.value }))}\n              label="Owner Account"'),
    ])


def fix_subscriptions_page():
    """Fix SubscriptionsPage.tsx entityType."""
    fix_file("pages/SubscriptionsPage.tsx", [
        ('entityType="customer"\n                    name="accountId"\n                    label="Account *"',
         'entityType="account"\n                    name="accountId"\n                    label="Account *"'),
    ])


def fix_workflow_designer():
    """Fix WorkflowDesignerPage.tsx entityType fallback."""
    fix_file("pages/admin/WorkflowDesignerPage.tsx", [
        ("entityType={workflow?.entityType || 'Customer'}\n                onChange={(property, value) => updateNodeProperty(property as keyof UpdateNodeDto, value)}\n                onDelete={() => deleteNode(selectedNode)}\n                readonly={version?.status === 'Active'}\n              />\n            ) : selectedNode.nodeType === 'Action' ? (\n              // Action node - use specialized Action Properties Panel\n              <ActionPropertiesPanel\n                nodeId={selectedNode.id}\n                nodeKey={selectedNode.nodeKey}\n                nodeName={selectedNode.name}\n                configuration={selectedNode.configuration || '{}'}\n                entityType={workflow?.entityType || 'Customer'}",
         "entityType={workflow?.entityType || 'Account'}\n                onChange={(property, value) => updateNodeProperty(property as keyof UpdateNodeDto, value)}\n                onDelete={() => deleteNode(selectedNode)}\n                readonly={version?.status === 'Active'}\n              />\n            ) : selectedNode.nodeType === 'Action' ? (\n              // Action node - use specialized Action Properties Panel\n              <ActionPropertiesPanel\n                nodeId={selectedNode.id}\n                nodeKey={selectedNode.nodeKey}\n                nodeName={selectedNode.name}\n                configuration={selectedNode.configuration || '{}'}\n                entityType={workflow?.entityType || 'Account'}"),
    ])


def fix_api_documentation():
    """Fix ApiDocumentationPage.tsx webhook events."""
    fix_file("pages/admin/ApiDocumentationPage.tsx", [
        ("{ event: 'customer.created', description: 'Fired when a new customer is created', category: 'Customer'",
         "{ event: 'account.created', description: 'Fired when a new account is created', category: 'Account'"),
        ("{ event: 'customer.updated', description: 'Fired when a customer is updated', category: 'Customer'",
         "{ event: 'account.updated', description: 'Fired when an account is updated', category: 'Account'"),
        ("{ event: 'customer.deleted', description: 'Fired when a customer is deleted', category: 'Customer'",
         "{ event: 'account.deleted', description: 'Fired when an account is deleted', category: 'Account'"),
    ])


def fix_share_contact_modal():
    """Fix ShareContactInfoModal.tsx - remove redundant Customer option."""
    fix_file("components/ContactInfo/ShareContactInfoModal.tsx", [
        ('<MenuItem value="Customer">Customer</MenuItem>\n                    <MenuItem value="Contact">Contact</MenuItem>\n                    <MenuItem value="Lead">Lead</MenuItem>\n                    <MenuItem value="Account">Account</MenuItem>',
         '<MenuItem value="Account">Account</MenuItem>\n                    <MenuItem value="Contact">Contact</MenuItem>\n                    <MenuItem value="Lead">Lead</MenuItem>'),
    ])


def fix_mocks_handlers():
    """Fix mocks/handlers.ts accessiblePages."""
    fix_file("mocks/handlers.ts", [
        ("accessiblePages: ['/dashboard', '/customers', '/contacts', '/leads']",
         "accessiblePages: ['/dashboard', '/accounts', '/contacts', '/leads']"),
    ])


def fix_architecture_diagram():
    """Fix ArchitectureDiagram.tsx module data."""
    fix_file("components/architecture/ArchitectureDiagram.tsx", [
        ("name: 'Customer Management',\n    description: 'Core customer lifecycle and relationship management',\n    icon: '👥',\n    entities: ['Customer', 'CustomerContact', 'Contact', 'SocialMediaLink'],\n    controllers: ['CustomersController', 'ContactsController'],\n    frontendPages: ['CustomersPage', 'ContactsPage', 'CustomerOverviewPage'],",
         "name: 'Account Management',\n    description: 'Core account lifecycle and relationship management',\n    icon: '👥',\n    entities: ['Account', 'AccountContact', 'Contact', 'SocialMediaLink'],\n    controllers: ['AccountsController', 'ContactsController'],\n    frontendPages: ['AccountsPage', 'ContactsPage', 'AccountOverviewPage'],"),
    ])
    # Remove the now-duplicate 'Account Management' module if it exists separately
    path = os.path.join(BASE, "components/architecture/ArchitectureDiagram.tsx")
    if os.path.exists(path):
        with open(path, 'r') as f:
            content = f.read()
        # Check if there are now two 'Account Management' modules
        if content.count("name: 'Account Management'") > 1:
            # Remove the second one (which was the original that only had Account entity)
            old_block = """  {
    name: 'Account Management',
    description: 'Contract and account lifecycle management',
    icon: '📋',
    entities: ['Account'],
    controllers: ['AccountsController'],
    frontendPages: ['AccountPage'],
  },"""
            if old_block in content:
                content = content.replace(old_block, "")
                with open(path, 'w') as f:
                    f.write(content)
                print("  ✅ Removed duplicate Account Management module")


if __name__ == '__main__':
    print("=" * 60)
    print("Fixing remaining Customer → Account references")
    print("=" * 60)
    
    print("\n1. Fixing EntitySelect.tsx (duplicate cases)...")
    fix_entityselect()
    
    print("\n2. Fixing CustomersPage.tsx (entityType props)...")
    fix_customers_page()
    
    print("\n3. Fixing QuotesPage.tsx (labels and entityType)...")
    fix_quotes_page()
    
    print("\n4. Fixing ServiceRequestsPage.tsx...")
    fix_service_requests_page()
    
    print("\n5. Fixing ContactsPage.tsx...")
    fix_contacts_page()
    
    print("\n6. Fixing SubscriptionsPage.tsx...")
    fix_subscriptions_page()
    
    print("\n7. Fixing WorkflowDesignerPage.tsx...")
    fix_workflow_designer()
    
    print("\n8. Fixing ApiDocumentationPage.tsx (webhooks)...")
    fix_api_documentation()
    
    print("\n9. Fixing ShareContactInfoModal.tsx...")
    fix_share_contact_modal()
    
    print("\n10. Fixing mocks/handlers.ts...")
    fix_mocks_handlers()
    
    print("\n11. Fixing ArchitectureDiagram.tsx...")
    fix_architecture_diagram()
    
    print("\n" + "=" * 60)
    print(f"Done! Changed {len(changes)} files:")
    for c in changes:
        print(f"  • {c}")
    print("=" * 60)

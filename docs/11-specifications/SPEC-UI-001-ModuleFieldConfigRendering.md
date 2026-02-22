# SPEC-UI-001: Module Field Configuration & UI Tab Rendering

> **Spec ID:** SPEC-UI-001  
> **Feature:** Module Field Configuration & Dynamic Tab Rendering  
> **Module:** Frontend UI (Cross-Cutting)  
> **Version:** 1.3  
> **Last Updated:** 2026-02-22  
> **Status:** ⚠️ Partial — 5 of 7 seeded modules compliant (Accounts manual, Contacts/Leads/Opportunities/Products via DynamicEntityForm). Campaigns & Quotes deferred (seed data expansion needed). 6 modules not yet seeded.

---

## 1. Business Context

### 1.1 Feature Description

The CRM provides a **Module Field Configuration** system that allows administrators to define which fields appear on entity forms, how they are grouped into tabs, their display order, required status, field types, and conditional visibility. The frontend must consume these configurations via the `useFieldConfig` hook and render forms dynamically using the `FieldRenderer` component and the `TabPanel` component.

### 1.2 Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                     BACKEND (Source of Truth)                        │
│                                                                     │
│  ModuleFieldConfiguration Entity (DB Table)                         │
│  ├── ModuleName: string (e.g., "Customer", "Contact", "Lead")      │
│  ├── FieldName: string (maps to DTO field)                          │
│  ├── FieldLabel: string (display label)                             │
│  ├── FieldType: string (text|select|date|number|email|currency|...) │
│  ├── TabIndex: int (which tab this field appears on)                │
│  ├── TabName: string (display name of the tab)                      │
│  ├── DisplayOrder: int (sort order within tab)                      │
│  ├── IsEnabled: bool (field visibility)                             │
│  ├── IsRequired: bool (validation)                                  │
│  ├── GridSize: int (1-12, MUI Grid)                                 │
│  ├── Options: string? (CSV or "lookup:category")                    │
│  ├── ParentField: string? (conditional visibility)                  │
│  └── ParentFieldValue: string? (conditional value)                  │
│                                                                     │
│  API Endpoint: GET /api/modulefieldconfigurations/{moduleName}      │
│                                                                     │
│  Seeded Modules: Customer, Contact, Lead, Opportunity, Quote,       │
│                  Campaign, Product                                  │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                     FRONTEND (Consumers)                            │
│                                                                     │
│  useFieldConfig(moduleName) Hook                                    │
│  ├── fieldConfigs: ModuleFieldConfiguration[]                       │
│  ├── tabs: TabConfig[] (grouped by tabIndex, sorted)                │
│  ├── loading: boolean                                               │
│  ├── error: string | null                                           │
│  ├── getTabFields(tabIndex, categoryValue?, formData?)              │
│  ├── isFieldVisible(config, formData)                               │
│  └── refresh()                                                      │
│                                                                     │
│  FieldRenderer Component                                            │
│  ├── Renders field based on config.fieldType                        │
│  ├── Supports: text, select, date, number, email, currency,        │
│  │            textarea, checkbox, url, phone                        │
│  ├── Special handling: linkedInUrl, twitterHandle, leadScore        │
│  └── Lookup support: options="lookup:categoryName"                  │
│                                                                     │
│  TabPanel Component                                                 │
│  ├── Props: { value, index, children, padding?, boxProps? }         │
│  ├── Renders children only when value === index                     │
│  └── Supports number and string indices                             │
└─────────────────────────────────────────────────────────────────────┘
```

### 1.3 Sub-Features

| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Dynamic Tab Rendering | Tabs generated from field config + special tabs | ✅ AccountsPage (manual) + ContactsPage, LeadsPage, OpportunitiesPage, ProductsPage (DynamicEntityForm) |
| SF-002 | FieldRenderer | Renders form fields based on config type | ✅ Implemented |
| SF-003 | Conditional Visibility | Fields show/hide based on parentField/parentFieldValue | ✅ Implemented in hook |
| SF-004 | Tab Index Alignment | TabPanel index must match position in visibleTabs array | ✅ Fixed in AccountsPage |
| SF-005 | Field Config Admin | CRUD for module field configurations | ✅ Implemented (backend) |
| SF-006 | Lookup Integration | Select fields with `lookup:` prefix fetch from lookupService | ✅ Implemented in FieldRenderer |

### 1.4 Use Cases

| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Create entity with dynamic form | User | Module has field configs seeded | Form tabs/fields rendered from config | ✅ Accounts, Contacts, Leads, Opportunities, Products |
| UC-002 | Edit entity with special tabs | User | Entity exists | Field config tabs + special tabs (notes, related, contacts) shown | ✅ Accounts, Contacts, Leads, Opportunities, Products |
| UC-003 | Admin changes field config | Admin | Has admin role | Field reorder/hide/require reflected in forms | ✅ Backend API works |
| UC-004 | Conditional field visibility | User | ParentField set on config | Field shows only when parent matches | ✅ Hook supports it |

---

## 2. Standard Tab Rendering Pattern (MANDATORY)

### 2.1 Tab Architecture

Every entity page that has a tabbed dialog/form MUST follow this pattern:

```
┌────────────────────────────────────────────────────────────────┐
│  VISIBLE TABS (ordered array, position = MUI tab index)        │
│                                                                │
│  Position 0: [Field Config Tab 0: "Basic Info"]                │
│  Position 1: [Field Config Tab 1: "Business"]                  │
│  Position 2: [Field Config Tab 2: "Contact Preferences"]       │
│  Position 3: [Field Config Tab 3: "Additional"]                │
│  Position 4: [Special Tab 100: "Contact Info"] (edit only)     │
│  Position 5: [Special Tab 101: "Linked Contacts"] (edit only)  │
│  Position 6: [Special Tab 105: "Addresses"] (edit only)        │
│  Position 7: [Special Tab 106: "Extended Info"] (always)       │
│  Position 8: [Special Tab 103: "Related"] (edit only)          │
│  Position 9: [Special Tab 102: "Notes"] (edit only)            │
│  Position 10: [Special Tab 104: "Preferences"] (edit only)     │
│                                                                │
│  Tab index in <TabPanel> = position in this array              │
│  NOT the tab.index from field config                           │
└────────────────────────────────────────────────────────────────┘
```

### 2.2 Implementation Rules

#### RULE 1: Use `useFieldConfig` for all modules that have seeded configs

**Seeded modules:** `Customer`, `Contact`, `Lead`, `Opportunity`, `Quote`, `Campaign`, `Product`

```tsx
// ✅ CORRECT
const { tabs, fieldConfigs, loading: fieldConfigsLoading, error: fieldConfigError, getTabFields } = useFieldConfig('ModuleName');

// ❌ WRONG - hardcoding tabs
const tabs = [
  { label: 'Basic Info', ... },
  { label: 'Business', ... },
];
```

#### RULE 2: Build visibleTabs array combining field config tabs + special tabs

```tsx
const getVisibleTabs = () => {
  // Start with field config tabs
  const baseTabs = tabs.map(t => ({ index: t.index, name: t.name }));
  
  // Add special tabs conditionally
  if (editingId) {
    baseTabs.push({ index: 100, name: 'Contact Info' });
    baseTabs.push({ index: 101, name: 'Linked Contacts' });
    // ... more special tabs
  }
  
  // Always-visible special tabs
  baseTabs.push({ index: 106, name: 'Extended Info' });
  
  return baseTabs;
};

const visibleTabs = getVisibleTabs();
```

#### RULE 3: Tab MUI element index = position in visibleTabs array

```tsx
// ✅ CORRECT
<Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)} variant="scrollable" scrollButtons="auto">
  {visibleTabs.map((tab, idx) => (
    <Tab key={tab.index} label={tab.name} />
  ))}
</Tabs>
```

The MUI `Tabs` component automatically assigns index 0, 1, 2... to each `Tab` in order. The `dialogTab` state holds this positional index.

#### RULE 4: TabPanel index must use `visibleTabs.findIndex()`, NOT hardcoded numbers

```tsx
// ✅ CORRECT - Field config tabs
{tabs.map((tab) => {
  const visibleTabIndex = visibleTabs.findIndex(t => t.index === tab.index);
  return (
    <TabPanel key={tab.index} value={dialogTab} index={visibleTabIndex}>
      {renderTabFields(tab.index)}
    </TabPanel>
  );
})}

// ✅ CORRECT - Special tabs
<TabPanel value={dialogTab} index={visibleTabs.findIndex(t => t.index === 100)}>
  {/* Contact Info content */}
</TabPanel>

// ❌ WRONG - Hardcoded indices
<TabPanel value={dialogTab} index={0}>  {/* Breaks when tabs are dynamic */}
<TabPanel value={dialogTab} index={1}>
```

#### RULE 5: Render field config tab content using FieldRenderer

```tsx
const renderTabFields = (tabIndex: number) => {
  const fields = getTabFields(tabIndex, undefined, formData);
  return (
    <Grid container spacing={2}>
      {fields.map(config => (
        <Grid item xs={12} sm={config.gridSize || 6} key={config.fieldName}>
          <FieldRenderer
            config={config}
            formData={formData}
            onChange={handleChange}
            onSelectChange={handleSelectChange}
            setFormData={setFormData}
          />
        </Grid>
      ))}
    </Grid>
  );
};
```

#### RULE 6: Handle loading and error states

```tsx
{fieldConfigsLoading ? (
  <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
    <CircularProgress />
  </Box>
) : (
  <>
    {/* Tab panels */}
  </>
)}

{fieldConfigError && (
  <Alert severity="warning" sx={{ mb: 2 }}>
    Field configurations could not be loaded. Using defaults.
  </Alert>
)}
```

#### RULE 7: Special tab index ranges

| Index Range | Purpose | Example |
|-------------|---------|---------|
| 0-99 | Field configuration tabs (from DB) | 0="Basic Info", 1="Business", 2="Contact Preferences", 3="Additional" |
| 100-199 | Special content tabs (hardcoded) | 100="Contact Info", 101="Linked Contacts", 102="Notes", 103="Related", 104="Preferences", 105="Addresses", 106="Extended Info" |

### 2.3 Preferred Implementation: `DynamicEntityForm` Component

> **IMPORTANT:** The `DynamicEntityForm` component (introduced in v0.561.6) encapsulates ALL of Rules 1–7 above into a single reusable component. **All new page migrations and new entity pages MUST use `DynamicEntityForm` instead of manually implementing the pattern.**

**File:** `CRM.Frontend/src/components/DynamicEntityForm.tsx`

#### 2.3.1 Component Props

| Prop | Type | Required | Description |
|------|------|----------|-------------|
| `moduleName` | `string` | ✅ | Backend module name (e.g. `"Contacts"`, `"Leads"`) |
| `formData` | `any` | ✅ | Current form state object |
| `onChange` | `(e: any) => void` | ✅ | Standard text input change handler |
| `onSelectChange` | `(e: any) => void` | ✅ | Select/autocomplete change handler |
| `setFormData` | `(fn: any) => void` | | State setter for slider/checkbox fields |
| `activeTab` | `number` | ✅ | Currently active tab index (controlled by parent) |
| `editingId` | `number \| null` | | Record ID when editing; `null` for create mode |
| `extraTabs` | `ExtraTab[]` | | Special tabs (Notes, Related, etc.) appended after field-config tabs |
| `fieldOverrides` | `Record<string, FieldOverride>` | | Per-field overrides: `{ disabled?, render? }` |
| `excludeFields` | `Set<string> \| string[]` | | Field names to skip rendering |
| `onTabChange` | `(newTab: number) => void` | | Tab change callback |
| `showRefreshButton` | `boolean` | | Show config refresh button (default: `true`) |

#### 2.3.2 `ExtraTab` Interface

```tsx
export interface ExtraTab {
  index: number;     // High index (100+) to avoid collisions with field-config tabs
  name: string;      // Tab label
  icon?: ReactNode;  // Optional icon
  editOnly?: boolean; // Only show when editingId is set
  render: () => ReactNode; // Content renderer
}
```

#### 2.3.3 `FieldOverride` Interface

```tsx
export interface FieldOverride {
  disabled?: boolean;
  render?: (config: ModuleFieldConfiguration, formData: any) => ReactNode;
}
```

#### 2.3.4 Usage Example (Recommended Pattern)

```tsx
import DynamicEntityForm, { ExtraTab } from '../components/DynamicEntityForm';

const ContactsPage: React.FC = () => {
  const [dialogTab, setDialogTab] = useState(0);
  const [formData, setFormData] = useState({});
  const [editingId, setEditingId] = useState<number | null>(null);

  const extraTabs: ExtraTab[] = useMemo(() => [
    { index: 102, name: 'Notes',   editOnly: true, render: () => <NotesPanel contactId={editingId!} /> },
    { index: 103, name: 'Related', editOnly: true, render: () => <RelatedPanel contactId={editingId!} /> },
    { index: 105, name: 'Addresses', editOnly: true, render: () => <AddressesPanel entityId={editingId!} entityType="Contact" /> },
  ], [editingId]);

  return (
    <Dialog open={dialogOpen} maxWidth="md" fullWidth>
      <DialogContent>
        <DynamicEntityForm
          moduleName="Contact"
          formData={formData}
          onChange={handleChange}
          onSelectChange={handleSelectChange}
          setFormData={setFormData}
          activeTab={dialogTab}
          editingId={editingId}
          extraTabs={extraTabs}
          onTabChange={setDialogTab}
        />
      </DialogContent>
    </Dialog>
  );
};
```

#### 2.3.5 What DynamicEntityForm Handles Automatically

| Concern | Manual Pattern (Rules 1-7) | DynamicEntityForm |
|---------|---------------------------|-------------------|
| `useFieldConfig` call | Each page must call | ✅ Internal |
| `visibleTabs` building | Each page must build array | ✅ Internal (`useMemo`) |
| Tab header rendering | Each page renders `<Tabs>` | ✅ Internal |
| TabPanel index alignment | Each page uses `findIndex()` | ✅ Internal (position in `visibleTabs`) |
| FieldRenderer per field | Each page iterates fields | ✅ Internal (`renderTabFields`) |
| Loading spinner | Each page checks `loading` | ✅ Internal |
| Error alert | Each page checks `error` | ✅ Internal |
| Edit-only tab filtering | Each page checks `editingId` | ✅ Internal (`editOnly` flag) |
| Field exclusion | N/A | ✅ `excludeFields` prop |
| Field override/custom render | N/A | ✅ `fieldOverrides` prop |

#### 2.3.6 When NOT to Use DynamicEntityForm

- **AccountsPage**: Already has a fully working manual implementation with complex special tabs. Migration is optional but recommended for consistency.
- **Pages with zero field configs seeded AND no plans to seed**: Can remain hardcoded (PaymentsPage).
- **Highly custom forms** that don't fit the tab pattern (e.g., dashboard widgets).

### 2.4 Standard Special Tabs by Entity

Not all entities need all special tabs. The table below defines which special tabs apply:

| Entity | Contact Info (100) | Linked Contacts (101) | Notes (102) | Related (103) | Preferences (104) | Addresses (105) | Extended Info (106) |
|--------|-------------------|----------------------|-------------|---------------|-------------------|-----------------|-------------------|
| **Accounts** | ✅ Edit | ✅ Edit | ✅ Edit | ✅ Edit | ✅ Edit | ✅ Edit | ✅ Always |
| **Contacts** | ✅ Edit | ❌ | ✅ Edit | ✅ Edit | ✅ Edit | ✅ Edit | ⚠️ Optional |
| **Leads** | ❌ | ❌ | ✅ Edit | ✅ Edit | ❌ | ❌ | ⚠️ Optional |
| **Opportunities** | ❌ | ❌ | ✅ Edit | ✅ Edit (Products) | ❌ | ❌ | ⚠️ Optional |
| **Quotes** | ❌ | ❌ | ✅ Edit | ✅ Edit (Line Items) | ❌ | ❌ | ⚠️ Optional |
| **Campaigns** | ❌ | ❌ | ✅ Edit | ✅ Edit (Recipients/Metrics) | ❌ | ❌ | ⚠️ Optional |
| **Products** | ❌ | ❌ | ❌ | ✅ Edit (Pricing) | ❌ | ❌ | ⚠️ Optional |
| **Orders** | ❌ | ❌ | ✅ Edit | ✅ Edit (Line Items) | ❌ | ❌ | ❌ |
| **Invoices** | ❌ | ❌ | ❌ | ✅ Edit (Line Items) | ❌ | ❌ | ❌ |
| **Contracts** | ❌ | ❌ | ✅ Edit | ✅ Edit | ❌ | ❌ | ❌ |
| **ServiceRequests** | ❌ | ❌ | ✅ Edit | ✅ Edit (SLA) | ❌ | ❌ | ❌ |
| **Payments** | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Interactions** | ❌ | ❌ | ❌ | ✅ Edit | ❌ | ❌ | ❌ |

---

## 3. Frontend Implementation Status

### 3.1 Core Components

| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| **DynamicEntityForm** | `CRM.Frontend/src/components/DynamicEntityForm.tsx` | ✅ Implemented | **Preferred approach** — encapsulates useFieldConfig + Tabs + TabPanel + FieldRenderer in one component. 283 lines. See Section 2.3. |
| TabPanel | `CRM.Frontend/src/components/common/TabPanel.tsx` | ✅ Implemented | Supports number/string index, a11yProps helper |
| FieldRenderer | `CRM.Frontend/src/components/FieldRenderer.tsx` | ✅ Implemented | Supports text, select, date, number, email, currency, textarea, checkbox, url + special fields |
| useFieldConfig | `CRM.Frontend/src/hooks/useFieldConfig.ts` | ✅ Implemented | Fetches from API, groups into TabConfig[], conditional visibility |

### 3.2 Page Compliance Audit

| Page | File | Module Name | Has useFieldConfig | Tab System | Compliance | Gap ID |
|------|------|-------------|-------------------|------------|------------|--------|
| **AccountsPage** | `pages/AccountsPage.tsx` | Customer | ✅ Yes | Dynamic visibleTabs + findIndex | ✅ Compliant | — |
| **ContactsPage** | `pages/ContactsPage.tsx` | Contact | ✅ Yes | ✅ Uses DynamicEntityForm (3 extra tabs: Contact Info, Related, Notes) | ✅ Compliant | ~~GAP-001~~ |
| **LeadsPage** | `pages/LeadsPage.tsx` | Lead | ✅ Yes | ✅ Uses DynamicEntityForm (3 extra tabs: Contact Info, Related, Notes) | ✅ Compliant | ~~GAP-002~~ |
| **OpportunitiesPage** | `pages/OpportunitiesPage.tsx` | Opportunity | ✅ Yes | ✅ Uses DynamicEntityForm (2 extra tabs: Related, Notes) | ✅ Compliant | ~~GAP-003~~ |
| **CampaignsPage** | `pages/CampaignsPage.tsx` | Campaign | ❌ No | `<TabPanel index={0..6}>` hardcoded | ⚠️ Deferred | GAP-004 |
| **QuotesPage** | `pages/QuotesPage.tsx` | Quote | ❌ No | `<TabPanel index={0..6}>` hardcoded | ⚠️ Deferred | GAP-005 |
| **ProductsPage** | `pages/ProductsPage.tsx` | Product | ✅ Yes | ✅ Uses DynamicEntityForm (1 extra tab: Notes) | ✅ Compliant | ~~GAP-006~~ |
| **OrdersPage** | `pages/OrdersPage.tsx` | *(none seeded)* | ❌ No | `<TabPanel index={0..1}>` hardcoded | ⚠️ N/A (no config seeded) | GAP-007 |
| **InvoicesPage** | `pages/InvoicesPage.tsx` | *(none seeded)* | ❌ No | `<TabPanel index={0..1}>` hardcoded | ⚠️ N/A (no config seeded) | GAP-008 |
| **ContractsPage** | `pages/ContractsPage.tsx` | *(none seeded)* | ❌ No | `<TabPanel index={0..4}>` hardcoded | ⚠️ N/A (no config seeded) | GAP-009 |
| **ServiceRequestsPage** | `pages/ServiceRequestsPage.tsx` | *(none seeded)* | ❌ No | Hardcoded `dialogTab === 0/1/2` | ⚠️ N/A (no config seeded) | GAP-010 |
| **PaymentsPage** | `pages/PaymentsPage.tsx` | *(none seeded)* | ❌ No | No tabs at all | ⚠️ N/A (no config, no tabs) | GAP-011 |
| **InteractionsPage** | `pages/InteractionsPage.tsx` | *(none seeded)* | ❌ No | `<TabPanel index={0..1}>` hardcoded | ⚠️ N/A (no config seeded) | GAP-012 |

---

## 4. Gap Details

### ~~GAP-001~~: ContactsPage — ✅ RESOLVED (v0.561.6)

**File:** `CRM.Frontend/src/pages/ContactsPage.tsx`  
**Module Name:** `Contact` (seeded in backend)  
**Status:** ✅ **Migrated to `DynamicEntityForm`** on 2026-02-22.  
**Implementation:** Uses `<DynamicEntityForm moduleName="Contact">` with 3 extra tabs: Contact Info (100), Related (103), Notes (102). All hardcoded form fields removed; fields now rendered dynamically from backend field configurations. Admin can reorder, hide, and require fields.

---

### ~~GAP-002~~: LeadsPage — ✅ RESOLVED (v0.561.6)

**File:** `CRM.Frontend/src/pages/LeadsPage.tsx`  
**Module Name:** `Lead` (seeded in backend)  
**Status:** ✅ **Migrated to `DynamicEntityForm`** on 2026-02-22.  
**Implementation:** Uses `<DynamicEntityForm moduleName="Lead">` with 3 extra tabs: Contact Info (100), Related (103), Notes (102). All hardcoded form fields and create/edit tab conditionals removed; `editOnly` flag on extra tabs handles visibility automatically.

---

### ~~GAP-003~~: OpportunitiesPage — ✅ RESOLVED (v0.561.6)

**File:** `CRM.Frontend/src/pages/OpportunitiesPage.tsx`  
**Module Name:** `Opportunity` (seeded in backend)  
**Status:** ✅ **Migrated to `DynamicEntityForm`** on 2026-02-22.  
**Implementation:** Uses `<DynamicEntityForm moduleName="Opportunity">` with 2 extra tabs: Related/Products (103), Notes (102). All hardcoded form fields removed; edit-mode conditional handled by `editOnly` flag.

---

### GAP-004: CampaignsPage — ⚠️ DEFERRED (backend seed data expansion required)

**File:** `CRM.Frontend/src/pages/CampaignsPage.tsx`  
**Module Name:** `Campaign` (seeded in backend)  
**Current Implementation:** Uses `<TabPanel value={dialogTab} index={0}>` through `index={6}` with hardcoded field sets. Conditional tab visibility for `editingId`.  
**Impact:** Admin changes to Campaign field configurations are ignored.  
**Deferral Reason:** CampaignsPage has ~50+ hardcoded form fields across 7 tabs, but only ~10-11 fields are seeded in the backend `Campaign` module field configuration. Migrating to `DynamicEntityForm` now would result in a severely degraded form with most fields missing. Backend seed data must be expanded to cover all Campaign fields before migration.  
**Remediation (when seed data is expanded):**
1. Expand `Campaign` module field configs in `CoreDataSeederService` to cover all ~50 fields across appropriate tabs
2. Import `DynamicEntityForm` and replace the 7-tab hardcoded layout
3. Pass `moduleName="Campaign"`, `editingId`
4. Define `extraTabs` for: Recipients (100), Metrics (101), Notes (102), Related (103), Conversions (104), Sequences (105) — all with `editOnly: true`
5. Move existing special tab JSX into `render` callbacks

---

### GAP-005: QuotesPage — ⚠️ DEFERRED (backend seed data expansion required)

**File:** `CRM.Frontend/src/pages/QuotesPage.tsx`  
**Module Name:** `Quote` (seeded in backend)  
**Current Implementation:** Uses `<TabPanel value={dialogTab} index={0}>` through `index={6}` with hardcoded fields. Conditional for `editingId`.  
**Impact:** Admin changes to Quote field configurations are ignored.  
**Deferral Reason:** QuotesPage has ~50+ hardcoded form fields across 7 tabs, but only ~10-11 fields are seeded in the backend `Quote` module field configuration. Migrating to `DynamicEntityForm` now would result in a severely degraded form with most fields missing. Backend seed data must be expanded to cover all Quote fields before migration.  
**Remediation (when seed data is expanded):**
1. Expand `Quote` module field configs in `CoreDataSeederService` to cover all ~50 fields across appropriate tabs
2. Import `DynamicEntityForm` and replace the 7-tab hardcoded layout
3. Pass `moduleName="Quote"`, `editingId`
4. Define `extraTabs` for: Line Items (100), Terms (101), Notes (102), Related (103) — with `editOnly: true` as appropriate
5. Move existing line-item table, terms editor, notes panel into `render` callbacks

---

### ~~GAP-006~~: ProductsPage — ✅ RESOLVED (v0.561.6)

**File:** `CRM.Frontend/src/pages/ProductsPage.tsx`  
**Module Name:** `Product` (seeded in backend)  
**Status:** ✅ **Migrated to `DynamicEntityForm`** on 2026-02-22.  
**Implementation:** Uses `<DynamicEntityForm moduleName="Product">` with 1 extra tab: Notes (102). All hardcoded form fields removed; fields now rendered dynamically from backend field configurations.

---

### GAP-007: OrdersPage — No field config seeded, hardcoded tabs

**File:** `CRM.Frontend/src/pages/OrdersPage.tsx`  
**Module Name:** *Not seeded* — needs `Order` module field configs  
**Current Implementation:** Uses `<TabPanel value={dialogTab} index={0}>` and `index={1}`.  
**Impact:** No dynamic field configuration possible.  
**Remediation (use `DynamicEntityForm`):**
1. Seed `Order` module field configurations in `CoreDataSeederService` (define tabs: Order Details, Shipping, etc.)
2. Import `DynamicEntityForm` and replace hardcoded tabs
3. Pass `moduleName="Order"`, define `extraTabs` for Line Items (100), Notes (102)

---

### GAP-008: InvoicesPage — No field config seeded, hardcoded tabs

**File:** `CRM.Frontend/src/pages/InvoicesPage.tsx`  
**Module Name:** *Not seeded* — needs `Invoice` module field configs  
**Current Implementation:** Uses `<TabPanel value={dialogTab} index={0}>` and `index={1}`.  
**Remediation (use `DynamicEntityForm`):**
1. Seed `Invoice` module field configs in `CoreDataSeederService`
2. Import `DynamicEntityForm`, pass `moduleName="Invoice"`
3. Define `extraTabs` for Line Items (100)

---

### GAP-009: ContractsPage — No field config seeded, hardcoded tabs

**File:** `CRM.Frontend/src/pages/ContractsPage.tsx`  
**Module Name:** *Not seeded* — needs `Contract` module field configs  
**Current Implementation:** Uses `<TabPanel value={dialogTab} index={0}>` through `index={4}`.  
**Remediation (use `DynamicEntityForm`):**
1. Seed `Contract` module field configs in `CoreDataSeederService` (tabs: Contract Details, Terms, Parties, etc.)
2. Import `DynamicEntityForm`, pass `moduleName="Contract"`
3. Define `extraTabs` for Notes (102), Related (103)

---

### GAP-010: ServiceRequestsPage — No field config seeded, hardcoded tabs

**File:** `CRM.Frontend/src/pages/ServiceRequestsPage.tsx`  
**Module Name:** *Not seeded* — needs `ServiceRequest` module field configs  
**Current Implementation:** Hardcoded `dialogTab === 0/1/2` with conditional for `selectedRequest`.  
**Remediation (use `DynamicEntityForm`):**
1. Seed `ServiceRequest` module field configs in `CoreDataSeederService`
2. Import `DynamicEntityForm`, pass `moduleName="ServiceRequest"`
3. Define `extraTabs` for Notes (102), SLA/Related (103) with `editOnly: true`

---

### GAP-011: PaymentsPage — No tabs, no field config

**File:** `CRM.Frontend/src/pages/PaymentsPage.tsx`  
**Module Name:** *Not seeded* — needs `Payment` module field configs  
**Current Implementation:** No tab system at all. Simple form.  
**Remediation (use `DynamicEntityForm`):**
1. Low priority — simple form with no tabs currently
2. When needed: Seed `Payment` module field configs and wrap form in `<DynamicEntityForm moduleName="Payment" ... />`
3. No `extraTabs` expected unless form complexity grows

---

### GAP-012: InteractionsPage — No field config seeded, hardcoded tabs

**File:** `CRM.Frontend/src/pages/InteractionsPage.tsx`  
**Module Name:** *Not seeded* — needs `Interaction` module field configs  
**Current Implementation:** Uses `<TabPanel value={tabValue} index={0}>` and `index={1}`.  
**Remediation (use `DynamicEntityForm`):**
1. Seed `Interaction` module field configs in `CoreDataSeederService`
2. Import `DynamicEntityForm`, pass `moduleName="Interaction"`
3. Define `extraTabs` for Related (103) with `editOnly: true`

---

## 5. Backend Implementation

### 5.1 Entities

| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| ModuleFieldConfiguration | `CRM.Core/Entities/ModuleFieldConfiguration.cs` | ✅ Implemented | Inherits BaseEntity |
| FieldMasterDataLink | `CRM.Core/Entities/FieldMasterDataLink.cs` | ✅ Implemented | Links to ModuleFieldConfiguration |

### 5.2 DTOs

| DTO | File Path | Status |
|-----|-----------|--------|
| ModuleFieldConfigurationDto | `CRM.Core/Dtos/ModuleUIConfigDto.cs` | ✅ Implemented |
| CreateModuleFieldConfigurationDto | `CRM.Core/Dtos/ModuleUIConfigDto.cs` | ✅ Implemented |
| UpdateModuleFieldConfigurationDto | `CRM.Core/Dtos/ModuleUIConfigDto.cs` | ✅ Implemented |

### 5.3 Services

| Service | File Path | Status |
|---------|-----------|--------|
| ModuleFieldConfigurationService | `CRM.Infrastructure/Services/ModuleFieldConfigurationService.cs` | ✅ Implemented |
| CoreDataSeederService | `CRM.Infrastructure/Services/CoreDataSeederService.cs` | ✅ Implemented |

### 5.4 Controllers

| Controller | Route | Status |
|------------|-------|--------|
| ModuleFieldConfigurationsController | `/api/modulefieldconfigurations` | ✅ Implemented |
| AdminSeedController | `/api/admin/seed` | ✅ Implemented (reseed endpoint) |

### 5.5 API Endpoints

| Method | Route | Purpose | Status |
|--------|-------|---------|--------|
| GET | `/api/modulefieldconfigurations/{moduleName}` | Get all field configs for a module | ✅ |
| GET | `/api/modulefieldconfigurations/{moduleName}/{id}` | Get specific config | ✅ |
| POST | `/api/modulefieldconfigurations` | Create new config | ✅ |
| PUT | `/api/modulefieldconfigurations/{id}` | Update config | ✅ |
| POST | `/api/admin/seed/module-field-configurations` | Seed/reseed configs | ✅ |

### 5.6 Seeded Module Configurations

| Module Name | Tab Count | Field Count | Tabs | Status |
|-------------|-----------|-------------|------|--------|
| **Customer** | 4 | 44 | Basic Info (0), Business (1), Contact Preferences (2), Additional (3) | ✅ Seeded |
| **Contact** | 4 | 21 | Basic Info (0), Professional (1), Communication (2), Additional (3) | ✅ Seeded |
| **Lead** | 4 | 19 | Basic Info (0), Company (1), Qualification (2), Additional (3) | ✅ Seeded |
| **Opportunity** | 3 | 12 | Basic Info (0), Financial (1), Additional (2) | ✅ Seeded |
| **Quote** | 3 | 10 | Basic Info (0), Financial (1), Additional (2) | ✅ Seeded |
| **Campaign** | 3 | 11 | Basic Info (0), Targeting (1), Additional (2) | ✅ Seeded |
| **Product** | 3 | 10 | Basic Info (0), Pricing (1), Additional (2) | ✅ Seeded |
| **Order** | 0 | 0 | *Not seeded* | ❌ Needs seeding |
| **Invoice** | 0 | 0 | *Not seeded* | ❌ Needs seeding |
| **Contract** | 0 | 0 | *Not seeded* | ❌ Needs seeding |
| **ServiceRequest** | 0 | 0 | *Not seeded* | ❌ Needs seeding |
| **Payment** | 0 | 0 | *Not seeded* | ❌ Needs seeding |
| **Interaction** | 0 | 0 | *Not seeded* | ❌ Needs seeding |

---

## 6. Database

### 6.1 Tables

| Table | Purpose | Status |
|-------|---------|--------|
| ModuleFieldConfigurations | Stores field config for all modules | ✅ Created |
| FieldMasterDataLinks | Links fields to master data lookups | ✅ Created |

---

## 7. Tests

### 7.1 Backend Tests

| Test | File | Status |
|------|------|--------|
| CoreDataSeederServiceTests.SeedModuleFieldConfigurationsAsync | `tests/CRM.Tests/Services/CoreDataSeederServiceTests.cs` | ✅ Implemented |
| CoreDataSeederServiceTests.ForceReseedModuleFieldConfigurationsAsync | `tests/CRM.Tests/Services/CoreDataSeederServiceTests.cs` | ✅ Implemented |

### 7.2 Frontend Tests

| Test | File | Status |
|------|------|--------|
| useFieldConfig hook test | — | ❌ Not Implemented |
| FieldRenderer component test | — | ❌ Not Implemented |
| TabPanel component test | — | ❌ Not Implemented |
| AccountsPage tab rendering test | — | ❌ Not Implemented |

---

## 8. Remediation Priority

### Phase 1: High Priority (Seeded modules with hardcoded tabs)

These modules already have backend field configurations seeded but the frontend doesn't use them:

| Priority | Page | Module | Effort | Impact |
|----------|------|--------|--------|--------|
| ~~P1~~ | ~~ContactsPage~~ | ~~Contact~~ | ~~Medium~~ | ✅ **DONE** — Migrated to DynamicEntityForm (3 extra tabs) |
| ~~P1~~ | ~~LeadsPage~~ | ~~Lead~~ | ~~Medium~~ | ✅ **DONE** — Migrated to DynamicEntityForm (3 extra tabs) |
| ~~P1~~ | ~~OpportunitiesPage~~ | ~~Opportunity~~ | ~~Medium~~ | ✅ **DONE** — Migrated to DynamicEntityForm (2 extra tabs) |
| P2 | CampaignsPage | Campaign | High | ⚠️ **DEFERRED** — needs backend seed data expansion (50+ fields vs 10-11 seeded) |
| P2 | QuotesPage | Quote | High | ⚠️ **DEFERRED** — needs backend seed data expansion (50+ fields vs 10-11 seeded) |
| ~~P2~~ | ~~ProductsPage~~ | ~~Product~~ | ~~Medium~~ | ✅ **DONE** — Migrated to DynamicEntityForm (1 extra tab) |

### Phase 2: Medium Priority (Need backend seeding + frontend update)

| Priority | Page | Module | Effort | Impact |
|----------|------|--------|--------|--------|
| P3 | OrdersPage | Order | High | Medium — needs backend seed + frontend |
| P3 | ContractsPage | Contract | High | Medium |
| P3 | ServiceRequestsPage | ServiceRequest | High | Medium — ITSM module |
| P3 | InvoicesPage | Invoice | High | Low |

### Phase 3: Low Priority

| Priority | Page | Module | Effort | Impact |
|----------|------|--------|--------|--------|
| P4 | InteractionsPage | Interaction | Medium | Low |
| P4 | PaymentsPage | Payment | Low | Low — no tabs currently |

---

## 9. Reference Implementations

### 9.1 DynamicEntityForm (PREFERRED for new work)

The `DynamicEntityForm` component in `CRM.Frontend/src/components/DynamicEntityForm.tsx` (283 lines) is the **recommended approach** for all entity pages. It:

- Calls `useFieldConfig(moduleName)` internally
- Builds `visibleTabs` from field-config tabs + `extraTabs` (filtering `editOnly` based on `editingId`)
- Renders `<Tabs>` header with icons from extra tabs
- Renders `<TabPanel>` for each visible tab using correct positional indices
- Renders `FieldRenderer` for each field in field-config tabs, with `excludeFields` and `fieldOverrides` support
- Handles loading (spinner) and error (alert with refresh button) states

**Key internal implementation details:**

| Feature | How It Works |
|---------|--------------|
| Tab index alignment | `visibleTabs.map((tab, idx) → <TabPanel index={idx}>)` — positional index, not `tab.index` |
| Edit-only filtering | `extraTabs` items with `editOnly: true` are excluded from `visibleTabs` when `!editingId` |
| Field filtering | `getTabFields(tabIndex, formData.category, formData)` → `.filter(cfg => !excludeFields.has(cfg.fieldName))` |
| Tab distinguishing | `visibleTabs` entries have `isFieldTab: boolean` to route to field rendering vs. extra tab render callback |
| Refresh support | Optional refresh button calls `useFieldConfig.refresh()` |

### 9.2 AccountsPage (Manual — legacy reference)

The fully compliant manual implementation is in `CRM.Frontend/src/pages/AccountsPage.tsx`. Key sections:

| Section | Lines (approx) | Description |
|---------|----------------|-------------|
| Imports | 1-52 | useFieldConfig, FieldRenderer, TabPanel, etc. |
| Hook usage | ~65 | `useFieldConfig('Accounts')` |
| renderTabFields | ~910-945 | Dynamic field rendering with Grid + FieldRenderer |
| getVisibleTabs | ~948-988 | Builds visibleTabs from field config tabs + special tabs |
| Tab headers | ~1301-1325 | `<Tabs>` with `visibleTabs.map()` |
| Field config TabPanels | ~1338-1347 | `tabs.map()` → `visibleTabs.findIndex()` → `<TabPanel>` |
| Special TabPanels | ~1348-1670 | Individual `<TabPanel>` with `visibleTabs.findIndex(t => t.index === 100)` etc. |

> **Note:** AccountsPage uses the manual pattern from Rules 1–7. New pages should use `DynamicEntityForm` instead (Section 2.3).

---

## 10. Changelog

| Date | Version | Change | Author |
|------|---------|--------|--------|
| 2026-02-22 | 1.3 | Updated Section 5.6 with verified seed data counts for all 7 modules (127 total fields). Fixed leading typo. Also fixed pre-existing `customFields` duplicate in `sales.ts` and `OrderStatus` enum test alignment during build fixes. | Copilot |
| 2026-02-22 | 1.2 | Marked ContactsPage, LeadsPage, OpportunitiesPage, ProductsPage as ✅ Compliant (migrated to DynamicEntityForm). Deferred CampaignsPage and QuotesPage (⚠️ backend seed data expansion required — 50+ hardcoded fields vs 10-11 seeded). Resolved GAP-001, GAP-002, GAP-003, GAP-006. Updated sub-features, use cases, and remediation priority. | Copilot |
| 2026-02-22 | 1.1 | Added `DynamicEntityForm` as preferred approach (Section 2.3). Updated all GAP remediation entries to recommend `DynamicEntityForm` over manual pattern. Updated reference implementations (Section 9). | Copilot |
| 2026-02-18 | 1.0 | Initial spec created. Documented current state, gaps, and remediation plan. | Copilot |

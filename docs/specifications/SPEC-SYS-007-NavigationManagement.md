# SPEC-SYS-007: Navigation Management

> **Module:** System
> **Feature:** Navigation Management
> **Version:** 1.2
> **Last Updated:** 2026-02-13
> **Status:** ✅ Implemented

---

## 1. Business Context

### 1.1 Feature Description
Provides configurable, role-aware navigation with admin subcategories and provider-aware items. Ensures critical admin items (Workflows, Workflow Monitor, LLM Settings, Integrations) are always present in nav config, even when local or remote configuration is missing.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SYS007-SF01 | Static Navigation | Default nav items and categories | ✅ Implemented |
| SYS007-SF02 | Dynamic Navigation | Backend-provided nav config | ✅ Implemented |
| SYS007-SF03 | Auto-Heal Config | Merge defaults into saved config | ✅ Implemented |
| SYS007-SF04 | Admin Subcategories | Collapsible admin sections | ✅ Implemented |
| SYS007-SF05 | RBAC Visibility | Filter by permissions and module flags | ✅ Implemented |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Admin updates nav order | Admin | Admin authenticated | Nav order saved | ✅ |
| UC-002 | User loads nav on login | User | Authenticated | Visible items rendered | ✅ |
| UC-003 | Missing admin items auto-heal | Admin | Saved config missing items | Required items appear | ✅ |
| UC-004 | Provider-aware nav load | Admin | Provider config available | Items reflect provider status | ✅ |
| UC-005 | Load Sales Module nav | Sales User | Sales module enabled | Quotes, Orders, Invoices visible | ✅ |
| UC-006 | Load Service Desk Module nav | Support User | Service Desk module enabled | Service Requests, Knowledge Base visible | ✅ |
| UC-007 | Filter nav by module flags | Admin | Module flags configured | Only enabled modules show in nav | ✅ |

---

## 1.4 Module Navigation Items (New)

### Sales Module
| Item ID | Label | Route | Icon | Module Flag | Parent Category | Status |
|---------|-------|-------|------|-------------|-----------------|--------|
| SALES-001 | Quotes | /sales/quotes | description | EnableSales | Sales | ✅ |
| SALES-002 | Orders | /sales/orders | shopping_cart | EnableSales | Sales | ✅ |
| SALES-003 | Invoices | /sales/invoices | receipt | EnableSales | Sales | ✅ |
| SALES-004 | Payments | /sales/payments | payment | EnableSales | Sales | ✅ |
| SALES-005 | Contracts | /sales/contracts | description | EnableSales | Sales | ✅ |
| SALES-006 | Subscriptions | /sales/subscriptions | recurring_orders | EnableSales | Sales | ✅ |
| SALES-007 | Commissions | /sales/commissions | trending_up | EnableSales | Sales (Admin) | ✅ |
| SALES-008 | Sales Settings | /admin/settings/sales | settings | EnableSales | Admin › Sales | ⚠️ |

### Service Desk Module
| Item ID | Label | Route | Icon | Module Flag | Parent Category | Status |
|---------|-------|-------|------|-------------|-----------------|--------|
| SDESK-001 | Service Requests | /service-desk/requests | support_agent | EnableServiceDesk | Service Desk | ✅ |
| SDESK-002 | Knowledge Base | /service-desk/knowledge-base | library_books | EnableServiceDesk | Service Desk | ✅ |
| SDESK-003 | SLA Management | /admin/settings/sla | hourglass_bottom | EnableServiceDesk | Admin › Service Desk | ⚠️ |
| SDESK-004 | Escalation Rules | /admin/settings/escalation | call_made | EnableServiceDesk | Admin › Service Desk | ⚠️ |
| SDESK-005 | Workflows | /admin/workflows | schema | EnableWorkflows | Admin › Workflows | ✅ |
| SDESK-006 | Workflow Monitor | /admin/workflow-monitor | monitor | EnableWorkflows | Admin › Workflows | ✅ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| NavigationSettingsPage | CRM.Frontend/src/pages/admin/NavigationSettingsPage.tsx | ✅ | Admin UI wrapper |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| Navigation | CRM.Frontend/src/components/Navigation.tsx | ✅ | Auto-heal + dynamic config + RBAC filtering |
| NavigationSettingsTab | CRM.Frontend/src/components/settings/NavigationSettingsTab.tsx | ✅ | Edit categories + items |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| navigationConfigService | CRM.Frontend/src/services/navigationConfigService.ts | getNavigationConfig, getNavigationItems, getProviderStatus | ✅ |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Nav Item Label | Required, 2–64 chars | Frontend | ✅ |
| Nav Item Path | Required, must be route or URL | Frontend | ✅ |
| Category ID | Required, unique | Frontend | ✅ |
| Admin Subcategory | Required for admin items | Frontend | ✅ |

---

## 3. Backend Implementation

## 3. Backend Implementation

### 3.1 Default Navigation Configuration

**Location:** `CRM.Infrastructure/Services/NavigationConfigService.cs`  
**Build Method:** `BuildDefaultNavItems()`

```json
{
  "categories": [
    {
      "id": "core",
      "label": "CRM",
      "icon": "dashboard",
      "order": 0,
      "items": [
        { "id": "accounts", "label": "Accounts", "route": "/accounts", "icon": "business", "order": 0 },
        { "id": "contacts", "label": "Contacts", "route": "/contacts", "icon": "person", "order": 1 },
        { "id": "leads", "label": "Leads", "route": "/leads", "icon": "trending_up", "order": 2 },
        { "id": "opportunities", "label": "Opportunities", "route": "/opportunities", "icon": "lightbulb", "order": 3 },
        { "id": "activities", "label": "Activities", "route": "/activities", "icon": "event", "order": 4 }
      ]
    },
    {
      "id": "sales",
      "label": "Sales",
      "icon": "shopping_bag",
      "order": 1,
      "moduleFlag": "EnableSales",
      "items": [
        { "id": "quotes", "label": "Quotes", "route": "/sales/quotes", "icon": "description", "order": 0 },
        { "id": "orders", "label": "Orders", "route": "/sales/orders", "icon": "shopping_cart", "order": 1 },
        { "id": "invoices", "label": "Invoices", "route": "/sales/invoices", "icon": "receipt", "order": 2 },
        { "id": "payments", "label": "Payments", "route": "/sales/payments", "icon": "payment", "order": 3 },
        { "id": "contracts", "label": "Contracts", "route": "/sales/contracts", "icon": "description", "order": 4 },
        { "id": "subscriptions", "label": "Subscriptions", "route": "/sales/subscriptions", "icon": "recurring_orders", "order": 5 }
      ]
    },
    {
      "id": "servicedesk",
      "label": "Service Desk",
      "icon": "support_agent",
      "order": 2,
      "moduleFlag": "EnableServiceDesk",
      "items": [
        { "id": "requests", "label": "Service Requests", "route": "/service-desk/requests", "icon": "support_agent", "order": 0 },
        { "id": "knowledge", "label": "Knowledge Base", "route": "/service-desk/knowledge-base", "icon": "library_books", "order": 1 }
      ]
    },
    {
      "id": "admin",
      "label": "Administration",
      "icon": "settings",
      "order": 99,
      "subcategories": [
        {
          "id": "admin-core",
          "label": "Core Settings",
          "items": [
            { "id": "users", "label": "Users", "route": "/admin/users", "icon": "people", "order": 0 },
            { "id": "groups", "label": "Groups", "route": "/admin/groups", "icon": "group_work", "order": 1 },
            { "id": "settings", "label": "System Settings", "route": "/admin/settings", "icon": "tune", "order": 2 }
          ]
        },
        {
          "id": "admin-sales",
          "label": "Sales Configuration",
          "moduleFlag": "EnableSales",
          "items": [
            { "id": "sales-settings", "label": "Sales Settings", "route": "/admin/settings/sales", "icon": "settings", "order": 0 },
            { "id": "commission-rules", "label": "Commission Rules", "route": "/admin/settings/commissions", "icon": "trending_up", "order": 1 },
            { "id": "discount-rules", "label": "Discount Rules", "route": "/admin/settings/discounts", "icon": "percent", "order": 2 }
          ]
        },
        {
          "id": "admin-servicedesk",
          "label": "Service Desk Configuration",
          "moduleFlag": "EnableServiceDesk",
          "items": [
            { "id": "sla-policies", "label": "SLA Policies", "route": "/admin/settings/sla", "icon": "hourglass_bottom", "order": 0 },
            { "id": "escalation-rules", "label": "Escalation Rules", "route": "/admin/settings/escalation", "icon": "call_made", "order": 1 },
            { "id": "queue-config", "label": "Queue Configuration", "route": "/admin/settings/queues", "icon": "queue", "order": 2 }
          ]
        },
        {
          "id": "admin-workflows",
          "label": "Workflow Management",
          "items": [
            { "id": "workflows", "label": "Workflows", "route": "/admin/workflows", "icon": "schema", "order": 0, "required": true },
            { "id": "workflow-monitor", "label": "Workflow Monitor", "route": "/admin/workflow-monitor", "icon": "monitor", "order": 1, "required": true }
          ]
        },
        {
          "id": "admin-integrations",
          "label": "Integrations",
          "items": [
            { "id": "webhooks", "label": "Webhooks", "route": "/admin/webhooks", "icon": "webhook", "order": 0, "required": true },
            { "id": "providers", "label": "Providers", "route": "/admin/providers", "icon": "extension", "order": 1, "required": true },
            { "id": "llm-settings", "label": "LLM Settings", "route": "/admin/settings/llm", "icon": "smart_toy", "order": 2, "required": true }
          ]
        }
      ]
    }
  ]
}
```

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| SystemSettings | CRM.Core/Entities/SystemSettings.cs | ✅ | Stores NavOrderConfig |

### 3.2 DTOs
| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| NavigationConfig | CRM.Core/Dtos/NavigationConfigDto.cs | ✅ | Provider-aware config |
| NavigationItemConfig | CRM.Core/Dtos/NavigationConfigDto.cs | ✅ | Includes menuName, provider flags |

### 3.3 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| INavigationConfigService | CRM.Core/Interfaces/INavigationConfigService.cs | 6 | ✅ |

### 3.4 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| NavigationConfigService | CRM.Infrastructure/Services/NavigationConfigService.cs | 10+ | ✅ |

### 3.5 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| NavigationController | CRM.Api/Controllers/NavigationController.cs | 6 | ✅ |
| SystemSettingsController | CRM.Api/Controllers/SystemSettingsController.cs | 1 (Nav update) | ✅ |

### 3.6 API Endpoints
```
GET  /api/navigation/config
GET  /api/navigation/config/user
GET  /api/navigation/permissions
GET  /api/navigation/items
GET  /api/navigation/items/user
GET  /api/navigation/external-services
GET  /api/navigation/provider-status
POST /api/navigation/cache/invalidate
PUT  /api/systemsettings/navigation/order
```

---

## 4. Database

### 4.1 Tables

#### SystemSettings
| Column | Type | Description |
|--------|------|-------------|
| NavOrderConfig | TEXT | JSON config for categories and items |

---

## 5. Tests

### 5.1 Unit Tests
| Test Class | Method | Description | Status |
|------------|--------|-------------|--------|
| NavigationConfigServiceTests | BuildNavItems_IncludesWorkflowItems | Default items enforced | ✅ |

### 5.2 Integration Tests
| Test Class | Method | Description | Status |
|------------|--------|-------------|--------|
| NavigationControllerTests | GetConfig_ReturnsItems | Config endpoint | ✅ |

---

## 6. Issues & Inconsistencies

| ID | Issue | Severity | Description |
|----|-------|----------|-------------|
| SYS007-ISS01 | Dynamic config not merged | Medium | ✅ Resolved (dynamic config now applied in UI) |
| SYS007-ISS02 | Missing tests | Medium | ✅ Resolved (navigation tests added) |

---

## 7. TODO Items

| ID | Description | Priority | Category |
|----|-------------|----------|----------|
| TODO-SYS007-004 | Add audit logging for nav changes | P3 | Backend | ✅ |

---

## 8. Change History

| Date | Version | Author | Changes |
|------|---------|--------|---------|
| 2026-02-13 | 1.0 | System | Initial specification |
| 2026-02-13 | 1.2 | System | Completed DTOs, interface, service, controller for navigation |
| 2026-02-13 | 1.3 | System | Added navigation tests and audit logging |

---

**END OF SPECIFICATION**

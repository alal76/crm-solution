#!/usr/bin/env python3
"""Insert UX-CONF track into MASTER_TODO_LIST.md."""
import re, os, sys

TODO_PATH = os.path.join(os.path.dirname(__file__), '../docs/MASTER_TODO_LIST.md')

UX_CONF_SECTION = """
### P2 — UX Configuration Consolidation (UX-CONF-001→014)

> Scatter tax: admin config is spread across ~40+ pages and 3 top-level routes outside `/admin/`.
> Goal: consolidate into two coherent hierarchies — **System Settings** and **CRM Config** —
> following the two-section accordion pattern defined in the field-gap policy.
> UX-CONF-001 and UX-CONF-002 are prerequisites. UX-CONF-003→010 can then run in parallel.
> UX-CONF-011→013 require UX-CONF-003→010 to be done. UX-CONF-014 is last.

#### Current Config Scatter Map (as of v0.617.0)

| Current Route | Component | Belongs In |
|--------------|-----------|-----------|
| `/admin/llm` | `LLMSettingsPage` | System Settings > Providers > AI/LLM tab |
| `/admin/social-login` | `SocialLoginSettingsPage` | System Settings > Security > SSO & Social Login tab |
| `/admin/integrations` | `IntegrationsSettingsPage` (only n8n+Zapier links) | System Settings > Integrations (expanded) |
| `/admin/analytics` | `AnalyticsSettingsPage` | System Settings > Providers > Analytics tab or CRM Config |
| `/admin/portal` | `PortalConfigPage` (outside admin layout) | CRM Config > Customer Portal tab |
| `/channel-settings` | `ChannelSettingsPage` (top-level, not under `/admin/`) | System Settings > Communications > Channels |
| `components/settings/EmailIntegrationTab` | SMTP config tab (hosted inside `SystemConfigurationPage`) | System Settings > Communications > Email/SMTP |
| `components/settings/CalendarIntegrationTab` | Calendar config tab (hosted inside `SystemConfigurationPage`) | System Settings > Communications > Calendar |
| `components/settings/SocialLoginSettingsTab` | Duplicate of `SocialLoginSettingsPage` | Consolidate into single sub-tab under Security |
| `components/settings/CompanyBrandingTab` | Appears to duplicate `BrandingSettingsPage` | Remove duplicate — single source of truth |

#### Target Information Architecture

```
System Settings (/admin/config/system — already exists)
  ├── General          (existing tab)
  ├── Security         (/admin/security — expand with SSO & Channels sub-tabs)
  │   ├── Security Policies  (passwords, sessions, 2FA admin policy)
  │   └── SSO & Social Login (absorb /admin/social-login)
  ├── Communications   (/admin/communications — NEW consolidated page)
  │   ├── Email / SMTP       (EmailIntegrationTab)
  │   ├── Channels           (absorb /channel-settings)
  │   ├── Notifications      (NotificationPreferencesPanel)
  │   └── Calendar           (CalendarIntegrationTab)
  ├── Providers        (/admin/providers — expand LLM + analytics into tabs)
  │   ├── AI / LLM           (absorb /admin/llm)
  │   ├── Search, Chat, Notification, Analytics, Signatures, Integrations (existing)
  ├── Integrations     (/admin/integrations — expand to show all external apps)
  │   ├── Automation (n8n, Zapier)
  │   └── External Apps (QuickBooks, Mailchimp, Calendly, LinkedIn stubs)
  └── Features         (/admin/features — no change)

CRM Config (/admin/config/crm — already exists)
  ├── General CRM      (existing)
  ├── Sales            (/admin/settings/sales — add as tab)
  ├── Service Desk     (/admin/settings/service-desk — add as tab)
  ├── Customer Portal  (absorb /admin/portal)
  ├── Branding         (absorb /admin/branding as tab here or keep standalone)
  └── Navigation       (absorb /admin/navigation)
```

#### UX-CONF Items

| ID | Priority | Description | Depends On |
|----|----------|-------------|-----------|
| UX-CONF-001 | P2 | **Audit** — Create a one-page config location map document listing every current admin route, its component, and its target IA destination (use scatter map above as template). Store as `docs/investigations/ux-config-scatter-map.md`. | — |
| UX-CONF-002 | P2 | **Design sign-off** — Review proposed target IA (above) with stakeholders; update `docs/11-specifications/SPEC-SYS-003-AdminSettings.md` (create if missing) with the agreed hierarchy before any code changes. | UX-CONF-001 |
| UX-CONF-003 | P2 | **LLM tab consolidation** — Absorb `LLMSettingsPage` content into `ProvidersPage` as an "AI / LLM" tab. Update route `/admin/llm` to redirect to `/admin/providers#ai`. Remove standalone `LLMSettingsPage` route entry from `App.tsx`. | UX-CONF-002 |
| UX-CONF-004 | P2 | **Social Login consolidation** — Absorb `SocialLoginSettingsPage` into `SecuritySettingsPage` as an "SSO & Social Login" tab (reuse existing `SocialLoginSettingsTab` component). Add redirect from `/admin/social-login` to `/admin/security#sso`. Remove standalone page route. | UX-CONF-002 |
| UX-CONF-005 | P2 | **New Communications page** — Create `/admin/communications` page with 4 tabs: Email/SMTP (`EmailIntegrationTab`), Channels (extract from `ChannelSettingsPage`), Notifications (`NotificationPreferencesPanel`), Calendar (`CalendarIntegrationTab`). Register route in `App.tsx` under `/admin`. | UX-CONF-002 |
| UX-CONF-006 | P2 | **Channel Settings relocation** — Move `ChannelSettingsPage` out of top-level route `/channel-settings` into `/admin/communications/channels`. Add redirect from `/channel-settings` to `/admin/communications#channels` for backward compat. | UX-CONF-005 |
| UX-CONF-007 | P2 | **Expand IntegrationsSettingsPage** — Add cards for all external app integrations: Chatwoot, Novu, Meilisearch, Ollama, DocuSeal, Apache Superset (show connection status via `/api/health/providers`), plus existing QuickBooks, Mailchimp, Calendly, LinkedIn stubs (INT-001→004). Group into two sections: "Automation Platforms" and "Business App Integrations". | UX-CONF-002 |
| UX-CONF-008 | P2 | **Analytics Settings consolidation** — Absorb `AnalyticsSettingsPage` into `ProvidersPage` as an "Analytics" tab (or into `CRMConfigurationPage` under a "Reporting" tab). Remove standalone `/admin/analytics` route. Add redirect. | UX-CONF-002 |
| UX-CONF-009 | P2 | **Portal config relocation** — Move `PortalConfigPage` from standalone `/admin/portal` into `CRMConfigurationPage` as a "Customer Portal" tab. Register `/admin/portal` redirect to `/admin/config/crm#portal`. | UX-CONF-002 |
| UX-CONF-010 | P2 | **Branding deduplication** — Audit `CompanyBrandingTab.tsx` vs `BrandingSettingsPage.tsx`; determine authoritative component. Remove the duplicate. Ensure `BrandingContext` is wired to the surviving component. | UX-CONF-002 |
| UX-CONF-011 | P2 | **Navigation menu update** — Update `AdminSettingsMenu.tsx` and sidebar navigation to reflect the new IA: add "Communications" group, remove direct links for absorbed pages, add sub-navigation or tab links for consolidated pages. | UX-CONF-003, UX-CONF-004, UX-CONF-005, UX-CONF-006, UX-CONF-007, UX-CONF-008, UX-CONF-009 |
| UX-CONF-012 | P2 | **Breadcrumbs** — Add consistent breadcrumb navigation to all admin settings pages using the existing `Breadcrumbs.tsx` component. Admin > System Settings > [Section] > [Tab] hierarchy. | UX-CONF-011 |
| UX-CONF-013 | P2 | **Backend alignment** — Audit `AdminConfigurationController` routes: add `/api/admin/config/communications` and `/api/admin/config/providers/ai` sub-routes if missing; ensure all new consolidated pages have corresponding API endpoints. | UX-CONF-003, UX-CONF-004, UX-CONF-005 |
| UX-CONF-014 | P2 | **Playwright E2E tests** — Add/update tests in `e2e-tests/` to cover: (a) navigation to each consolidated settings page, (b) SMTP config form submit, (c) Social Login SSO provider enable/disable, (d) Provider selection per-category, (e) Portal config tab visibility toggle. | UX-CONF-011, UX-CONF-012, UX-CONF-013 |

"""

P2_BLOCKED_MARKER = '### P2 — Feature Flag and External Integration Enablements (Blocked)'

with open(TODO_PATH, 'r') as f:
    content = f.read()

if 'UX-CONF-001' in content:
    print('UX-CONF section already present — no changes made.')
    sys.exit(0)

# Insert the new section just before the P2 Blocked section
if P2_BLOCKED_MARKER not in content:
    print(f'ERROR: Could not find insertion marker:\n  "{P2_BLOCKED_MARKER}"')
    sys.exit(1)

updated = content.replace(P2_BLOCKED_MARKER, UX_CONF_SECTION.lstrip('\n') + '\n---\n\n' + P2_BLOCKED_MARKER)

# Update the item counts
updated = updated.replace(
    '| P2 Active code | 37 | Post-GA sprint work |',
    '| P2 Active code | 51 | Post-GA sprint work (incl. 14 UX-CONF) |'
)
updated = updated.replace(
    '| **Total** | **82** |',
    '| **Total** | **96** |'
)

# Update the header line
updated = updated.replace(
    '**Active Backlog:** 90 items — 4 P0 critical + 14 P1 pre-GA + 72 P2/P3 post-GA',
    '**Active Backlog:** 104 items — 4 P0 critical + 14 P1 pre-GA + 86 P2/P3 post-GA'
)

# Update Last Updated
updated = re.sub(
    r'\*\*Last Updated:\*\* [^\n]+',
    '**Last Updated:** March 9, 2026 (added UX-CONF track — config UI consolidation)',
    updated, count=1
)
# Update version
updated = updated.replace('**Version:** 0.617.0', '**Version:** 0.617.1')

with open(TODO_PATH, 'w') as f:
    f.write(updated)

lines = updated.count('\n') + 1
print(f'Success: {lines} lines, {len(updated)} bytes')

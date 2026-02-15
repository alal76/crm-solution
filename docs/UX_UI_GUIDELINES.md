# UX/UI Guidelines

> **Last Updated:** February 14, 2026
> **Scope:** Accessibility baseline + shared empty/loading state usage

---

## 1. Accessibility Baseline (WCAG 2.1 AA Target)

### Required Practices
- **Keyboard navigation**: All interactive elements must be reachable via Tab/Shift+Tab.
- **Visible focus**: Focus rings must be clearly visible on buttons, links, and inputs.
- **Skip link**: A “Skip to main content” link must be available for keyboard users.
- **ARIA labels**: Icon-only buttons and links must include `aria-label` text.
- **Dialog semantics**: Dialogs must provide a title and focus trap (MUI handles by default).
- **Color contrast**: Ensure text and UI elements maintain readable contrast (4.5:1 for body text).

### Implementation Notes
- Focus styling is centralized in [CRM.Frontend/src/theme/muiTheme.ts](../CRM.Frontend/src/theme/muiTheme.ts).
- Skip link styling is in [CRM.Frontend/src/App.css](../CRM.Frontend/src/App.css).
- Icon-only buttons should include `aria-label` in their component definition.

---

## 2. Shared Empty & Loading States

### Components
- **EnhancedEmptyState** (empty and no-results states)
  - Location: [CRM.Frontend/src/components/common/EnhancedEmptyState.tsx](../CRM.Frontend/src/components/common/EnhancedEmptyState.tsx)
- **LoadingSpinner** (standard loading experience)
  - Location: [CRM.Frontend/src/components/common/LoadingSpinner.tsx](../CRM.Frontend/src/components/common/LoadingSpinner.tsx)

### Usage Guidelines
- **Lists & tables**: Use `EnhancedEmptyState` for “no data” and “no results” states.
- **Page-level loads**: Use `LoadingSpinner` for initial loading or long-running operations.
- **Inline loads**: Use skeletons for short, localized loads (tables, cards, widgets).

### Examples
- Dashboard empty states now use `EnhancedEmptyState`.
- Settings loading uses `LoadingSpinner`.

---

## 3. Theme Color Validation

- Frontend validates hex colors using [CRM.Frontend/src/utils/colorValidation.ts](../CRM.Frontend/src/utils/colorValidation.ts).
- Backend validates color payloads in `SystemSettingsService`.
- Palette IDs are validated and enforced with a foreign key constraint (`SystemSettings.SelectedPaletteId → ColorPalettes.Id`).

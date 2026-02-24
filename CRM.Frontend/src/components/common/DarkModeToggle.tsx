/**
 * DarkModeToggle - Convenience re-export in common/ directory
 * TODO-UX-11: Dark mode toggle component
 *
 * The canonical implementation lives in components/accessibility/DarkModeToggle.tsx
 * and integrates with ThemeContext. This file makes it accessible from the
 * common barrel export.
 */

export {
  DarkModeToggle,
  type DarkModeToggleProps,
} from '../accessibility/DarkModeToggle';

export default undefined; // barrel‑only module

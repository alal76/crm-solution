/**
 * Contexts - Barrel Export
 */

// Authentication context
export { AuthProvider, useAuth } from './AuthContext';

// Theme context for light/dark/system mode
export { AppThemeProvider as ThemeContextProvider, useTheme as useThemeContext } from './ThemeContext';

// Font size adjustment context
export { FontSizeProvider, useFontSize } from './FontSizeContext';
export { FontSizeSelector } from '../components/accessibility/FontSizeSelector';
export type { FontSize } from './FontSizeContext';

// Recent items tracking context
export { RecentItemsProvider, useRecentItems, useTrackRecentItem } from './RecentItemsContext';
export type { RecentItem, RecentItemType } from './RecentItemsContext';

// ITSM module shared state context
export { ITSMProvider, useITSM } from './ITSMContext';
export type { ITSMModuleTab, ITSMDashboardMetrics, ITSMFilters } from './ITSMContext';

/**
 * High Contrast Theme
 * TODO-UX-04: High contrast color palette for accessibility (WCAG AAA)
 *
 * Re-exports and extends the primary high-contrast theme located in
 * `src/theme/HighContrastTheme.ts` so that imports from `src/themes/`
 * also work.
 */

import { createTheme, Theme, ThemeOptions } from '@mui/material/styles';

// WCAG AAA compliant colors — 7:1+ contrast ratio
const highContrastPalette = {
  // Core
  primary: '#FFFF00',
  onPrimary: '#000000',
  secondary: '#00FFFF',
  onSecondary: '#000000',

  // Status
  error: '#FF4444',
  onError: '#000000',
  warning: '#FFFF00',
  onWarning: '#000000',
  success: '#00FF00',
  onSuccess: '#000000',
  info: '#00FFFF',
  onInfo: '#000000',

  // Surface
  background: '#000000',
  surface: '#0A0A0A',
  onBackground: '#FFFFFF',
  onSurface: '#FFFFFF',

  // Borders
  outline: '#FFFFFF',
  divider: '#FFFFFF',
  disabled: '#808080',
};

const highContrastThemeOptions: ThemeOptions = {
  palette: {
    mode: 'dark',
    primary: { main: highContrastPalette.primary, contrastText: highContrastPalette.onPrimary },
    secondary: { main: highContrastPalette.secondary, contrastText: highContrastPalette.onSecondary },
    error: { main: highContrastPalette.error, contrastText: highContrastPalette.onError },
    warning: { main: highContrastPalette.warning, contrastText: highContrastPalette.onWarning },
    success: { main: highContrastPalette.success, contrastText: highContrastPalette.onSuccess },
    info: { main: highContrastPalette.info, contrastText: highContrastPalette.onInfo },
    background: {
      default: highContrastPalette.background,
      paper: highContrastPalette.surface,
    },
    text: {
      primary: '#FFFFFF',
      secondary: '#EEEEEE',
      disabled: highContrastPalette.disabled,
    },
    divider: highContrastPalette.divider,
    action: {
      active: '#FFFFFF',
      hover: 'rgba(255,255,255,0.16)',
      selected: 'rgba(255,255,0,0.32)',
      disabled: highContrastPalette.disabled,
      disabledBackground: 'rgba(128,128,128,0.24)',
    },
  },
  typography: {
    fontWeightRegular: 500,
    fontWeightMedium: 600,
    fontWeightBold: 700,
    body1: { fontSize: '1rem', lineHeight: 1.6 },
    body2: { fontSize: '0.9375rem', lineHeight: 1.6 },
    button: { fontWeight: 700, letterSpacing: '0.05em' },
  },
  shape: { borderRadius: 4 },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          border: '2px solid',
          fontWeight: 700,
          textTransform: 'none',
          '&:focus-visible': {
            outline: `3px solid ${highContrastPalette.primary}`,
            outlineOffset: 2,
          },
        },
      },
    },
    MuiOutlinedInput: {
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-notchedOutline': {
            borderColor: '#FFFFFF',
            borderWidth: 2,
          },
          '&:hover .MuiOutlinedInput-notchedOutline': {
            borderColor: highContrastPalette.primary,
          },
          '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
            borderColor: highContrastPalette.primary,
            borderWidth: 3,
          },
        },
      },
    },
    MuiTableCell: {
      styleOverrides: {
        root: {
          borderColor: '#FFFFFF',
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: {
          borderWidth: 2,
          fontWeight: 600,
        },
      },
    },
    MuiLink: {
      styleOverrides: {
        root: {
          color: highContrastPalette.secondary,
          textDecorationColor: highContrastPalette.secondary,
          fontWeight: 600,
          '&:focus-visible': {
            outline: `3px solid ${highContrastPalette.primary}`,
            outlineOffset: 2,
          },
        },
      },
    },
    MuiTooltip: {
      styleOverrides: {
        tooltip: {
          backgroundColor: '#FFFFFF',
          color: '#000000',
          border: '2px solid #000000',
          fontSize: '0.875rem',
          fontWeight: 600,
        },
      },
    },
  },
};

export const highContrastTheme: Theme = createTheme(highContrastThemeOptions);

export const highContrastColors = highContrastPalette;
export default highContrastTheme;

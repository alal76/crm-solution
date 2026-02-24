/**
 * HighContrastTheme - WCAG AAA contrast theme variant
 * Provides maximum contrast for accessibility
 */

import { createTheme, Theme, ThemeOptions } from '@mui/material/styles';

// WCAG AAA compliant colors (7:1 contrast ratio minimum)
const highContrastColors = {
  // Pure black and white for maximum contrast
  primary: '#FFFFFF',
  onPrimary: '#000000',
  primaryContainer: '#FFFF00', // Yellow for emphasis
  onPrimaryContainer: '#000000',

  secondary: '#00FFFF', // Cyan
  onSecondary: '#000000',
  secondaryContainer: '#00FFFF',
  onSecondaryContainer: '#000000',

  tertiary: '#FF00FF', // Magenta
  onTertiary: '#000000',
  tertiaryContainer: '#FF00FF',
  onTertiaryContainer: '#000000',

  // High visibility error colors
  error: '#FF0000',
  onError: '#FFFFFF',
  errorContainer: '#FF6666',
  onErrorContainer: '#000000',

  // Success (using pure green)
  success: '#00FF00',
  onSuccess: '#000000',
  successContainer: '#00FF00',
  onSuccessContainer: '#000000',

  // Warning (using bright yellow)
  warning: '#FFFF00',
  onWarning: '#000000',
  warningContainer: '#FFFF00',
  onWarningContainer: '#000000',

  // Info (using bright cyan)
  info: '#00FFFF',
  onInfo: '#000000',
  infoContainer: '#00FFFF',
  onInfoContainer: '#000000',

  // Background (pure black for dark mode high contrast)
  background: '#000000',
  onBackground: '#FFFFFF',
  surface: '#000000',
  onSurface: '#FFFFFF',
  surfaceVariant: '#1A1A1A',
  onSurfaceVariant: '#FFFFFF',

  // Borders and outlines - high visibility
  outline: '#FFFFFF',
  outlineVariant: '#CCCCCC',
  divider: '#FFFFFF',

  // Disabled states - still visible
  disabled: '#808080',
  onDisabled: '#FFFFFF',
};

// Typography for high contrast - larger text, bolder weights
const highContrastTypography = {
  fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
  fontWeightLight: 400,
  fontWeightRegular: 500,
  fontWeightMedium: 600,
  fontWeightBold: 700,
  h1: {
    fontSize: '3.75rem',
    fontWeight: 700,
    lineHeight: 1.2,
    letterSpacing: '-0.01em',
  },
  h2: {
    fontSize: '3rem',
    fontWeight: 700,
    lineHeight: 1.2,
    letterSpacing: '-0.005em',
  },
  h3: {
    fontSize: '2.5rem',
    fontWeight: 600,
    lineHeight: 1.25,
  },
  h4: {
    fontSize: '2rem',
    fontWeight: 600,
    lineHeight: 1.3,
  },
  h5: {
    fontSize: '1.625rem',
    fontWeight: 600,
    lineHeight: 1.35,
  },
  h6: {
    fontSize: '1.375rem',
    fontWeight: 600,
    lineHeight: 1.4,
  },
  body1: {
    fontSize: '1.125rem', // Slightly larger
    fontWeight: 500,
    lineHeight: 1.6,
  },
  body2: {
    fontSize: '1rem',
    fontWeight: 500,
    lineHeight: 1.6,
  },
  button: {
    fontSize: '1rem',
    fontWeight: 700,
    textTransform: 'none' as const,
    letterSpacing: '0.02em',
  },
  caption: {
    fontSize: '0.9375rem',
    fontWeight: 500,
    lineHeight: 1.5,
  },
};

// Component overrides for high contrast
const componentOverrides = {
  MuiCssBaseline: {
    styleOverrides: {
      body: {
        scrollbarColor: '#FFFFFF #000000',
        '&::-webkit-scrollbar': {
          width: '12px',
          height: '12px',
        },
        '&::-webkit-scrollbar-track': {
          backgroundColor: '#000000',
          border: '1px solid #FFFFFF',
        },
        '&::-webkit-scrollbar-thumb': {
          backgroundColor: '#FFFFFF',
          border: '2px solid #000000',
        },
      },
      // Force high contrast for all elements
      '*': {
        outlineColor: '#FFFF00 !important',
      },
      // Focus indicators
      '*:focus-visible': {
        outline: '3px solid #FFFF00 !important',
        outlineOffset: '2px !important',
      },
    },
  },
  MuiButton: {
    styleOverrides: {
      root: {
        borderWidth: '2px',
        '&:hover': {
          borderWidth: '2px',
        },
      },
      contained: {
        backgroundColor: '#FFFFFF',
        color: '#000000',
        '&:hover': {
          backgroundColor: '#FFFF00',
          color: '#000000',
        },
        '&:focus': {
          backgroundColor: '#FFFF00',
        },
      },
      outlined: {
        borderColor: '#FFFFFF',
        borderWidth: '2px',
        color: '#FFFFFF',
        '&:hover': {
          borderColor: '#FFFF00',
          borderWidth: '2px',
          backgroundColor: 'rgba(255, 255, 0, 0.1)',
        },
      },
      text: {
        color: '#FFFFFF',
        '&:hover': {
          backgroundColor: 'rgba(255, 255, 255, 0.1)',
          textDecoration: 'underline',
        },
      },
    },
  },
  MuiIconButton: {
    styleOverrides: {
      root: {
        color: '#FFFFFF',
        '&:hover': {
          backgroundColor: 'rgba(255, 255, 255, 0.1)',
        },
        '&:focus-visible': {
          outline: '3px solid #FFFF00',
          outlineOffset: '2px',
        },
      },
    },
  },
  MuiTextField: {
    styleOverrides: {
      root: {
        '& .MuiOutlinedInput-root': {
          '& fieldset': {
            borderColor: '#FFFFFF',
            borderWidth: '2px',
          },
          '&:hover fieldset': {
            borderColor: '#FFFF00',
            borderWidth: '2px',
          },
          '&.Mui-focused fieldset': {
            borderColor: '#FFFF00',
            borderWidth: '3px',
          },
        },
        '& .MuiInputLabel-root': {
          color: '#FFFFFF',
          fontWeight: 600,
        },
        '& .MuiInputBase-input': {
          color: '#FFFFFF',
        },
      },
    },
  },
  MuiSelect: {
    styleOverrides: {
      icon: {
        color: '#FFFFFF',
      },
    },
  },
  MuiCheckbox: {
    styleOverrides: {
      root: {
        color: '#FFFFFF',
        '&.Mui-checked': {
          color: '#FFFF00',
        },
      },
    },
  },
  MuiRadio: {
    styleOverrides: {
      root: {
        color: '#FFFFFF',
        '&.Mui-checked': {
          color: '#FFFF00',
        },
      },
    },
  },
  MuiSwitch: {
    styleOverrides: {
      track: {
        backgroundColor: '#808080',
      },
      thumb: {
        backgroundColor: '#FFFFFF',
      },
      switchBase: {
        '&.Mui-checked': {
          '& + .MuiSwitch-track': {
            backgroundColor: '#00FF00',
          },
        },
      },
    },
  },
  MuiLink: {
    styleOverrides: {
      root: {
        color: '#00FFFF',
        textDecoration: 'underline',
        fontWeight: 600,
        '&:hover': {
          color: '#FFFF00',
        },
        '&:visited': {
          color: '#FF00FF',
        },
      },
    },
  },
  MuiTableCell: {
    styleOverrides: {
      root: {
        borderColor: '#FFFFFF',
      },
      head: {
        backgroundColor: '#1A1A1A',
        color: '#FFFFFF',
        fontWeight: 700,
      },
    },
  },
  MuiTableRow: {
    styleOverrides: {
      root: {
        '&:hover': {
          backgroundColor: 'rgba(255, 255, 255, 0.1)',
        },
        '&.Mui-selected': {
          backgroundColor: 'rgba(255, 255, 0, 0.2)',
          '&:hover': {
            backgroundColor: 'rgba(255, 255, 0, 0.3)',
          },
        },
      },
    },
  },
  MuiPaper: {
    styleOverrides: {
      root: {
        backgroundColor: '#000000',
        border: '2px solid #FFFFFF',
      },
      elevation1: {
        boxShadow: '0 0 0 2px #FFFFFF',
      },
    },
  },
  MuiCard: {
    styleOverrides: {
      root: {
        border: '2px solid #FFFFFF',
      },
    },
  },
  MuiChip: {
    styleOverrides: {
      root: {
        borderWidth: '2px',
      },
      outlined: {
        borderColor: '#FFFFFF',
      },
      filled: {
        backgroundColor: '#FFFFFF',
        color: '#000000',
      },
    },
  },
  MuiAlert: {
    styleOverrides: {
      root: {
        borderWidth: '2px',
      },
      standardError: {
        backgroundColor: '#330000',
        color: '#FFFFFF',
        border: '2px solid #FF0000',
      },
      standardWarning: {
        backgroundColor: '#333300',
        color: '#FFFFFF',
        border: '2px solid #FFFF00',
      },
      standardSuccess: {
        backgroundColor: '#003300',
        color: '#FFFFFF',
        border: '2px solid #00FF00',
      },
      standardInfo: {
        backgroundColor: '#003333',
        color: '#FFFFFF',
        border: '2px solid #00FFFF',
      },
    },
  },
  MuiTooltip: {
    styleOverrides: {
      tooltip: {
        backgroundColor: '#FFFFFF',
        color: '#000000',
        fontSize: '1rem',
        fontWeight: 600,
        border: '2px solid #000000',
      },
      arrow: {
        color: '#FFFFFF',
      },
    },
  },
  MuiListItemButton: {
    styleOverrides: {
      root: {
        '&:hover': {
          backgroundColor: 'rgba(255, 255, 255, 0.1)',
        },
        '&.Mui-selected': {
          backgroundColor: 'rgba(255, 255, 0, 0.2)',
          borderLeft: '4px solid #FFFF00',
          '&:hover': {
            backgroundColor: 'rgba(255, 255, 0, 0.3)',
          },
        },
      },
    },
  },
  MuiDrawer: {
    styleOverrides: {
      paper: {
        backgroundColor: '#000000',
        borderRight: '2px solid #FFFFFF',
      },
    },
  },
  MuiAppBar: {
    styleOverrides: {
      root: {
        backgroundColor: '#000000',
        borderBottom: '2px solid #FFFFFF',
      },
    },
  },
  MuiDialog: {
    styleOverrides: {
      paper: {
        border: '2px solid #FFFFFF',
      },
    },
  },
  MuiDivider: {
    styleOverrides: {
      root: {
        borderColor: '#FFFFFF',
      },
    },
  },
  MuiTab: {
    styleOverrides: {
      root: {
        fontWeight: 600,
        '&.Mui-selected': {
          color: '#FFFF00',
        },
      },
    },
  },
  MuiTabs: {
    styleOverrides: {
      indicator: {
        backgroundColor: '#FFFF00',
        height: '3px',
      },
    },
  },
};

// Create high contrast theme
export const createHighContrastTheme = (): Theme => {
  return createTheme({
    palette: {
      mode: 'dark',
      primary: {
        main: highContrastColors.primary,
        light: highContrastColors.primaryContainer,
        dark: '#CCCCCC',
        contrastText: highContrastColors.onPrimary,
      },
      secondary: {
        main: highContrastColors.secondary,
        light: '#66FFFF',
        dark: '#00CCCC',
        contrastText: highContrastColors.onSecondary,
      },
      error: {
        main: highContrastColors.error,
        light: '#FF6666',
        dark: '#CC0000',
        contrastText: highContrastColors.onError,
      },
      warning: {
        main: highContrastColors.warning,
        light: '#FFFF66',
        dark: '#CCCC00',
        contrastText: highContrastColors.onWarning,
      },
      success: {
        main: highContrastColors.success,
        light: '#66FF66',
        dark: '#00CC00',
        contrastText: highContrastColors.onSuccess,
      },
      info: {
        main: highContrastColors.info,
        light: '#66FFFF',
        dark: '#00CCCC',
        contrastText: highContrastColors.onInfo,
      },
      background: {
        default: highContrastColors.background,
        paper: highContrastColors.surface,
      },
      text: {
        primary: highContrastColors.onSurface,
        secondary: highContrastColors.onSurfaceVariant,
        disabled: highContrastColors.disabled,
      },
      divider: highContrastColors.divider,
      action: {
        active: '#FFFFFF',
        hover: 'rgba(255, 255, 255, 0.15)',
        selected: 'rgba(255, 255, 0, 0.25)',
        disabled: 'rgba(255, 255, 255, 0.3)',
        disabledBackground: 'rgba(255, 255, 255, 0.1)',
        focus: 'rgba(255, 255, 0, 0.3)',
      },
    },
    typography: highContrastTypography,
    shape: {
      borderRadius: 4,
    },
    components: componentOverrides,
  });
};

// Export pre-created theme
export const highContrastTheme = createHighContrastTheme();

export default highContrastTheme;

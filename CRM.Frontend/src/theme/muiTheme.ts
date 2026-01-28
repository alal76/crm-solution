import { createTheme, Theme, ThemeOptions } from '@mui/material/styles';

// Material Design 3 Light Color System
const md3LightColors = {
  // Primary
  primary: '#6750A4',
  onPrimary: '#FFFFFF',
  primaryContainer: '#EADDFF',
  onPrimaryContainer: '#21005E',

  // Secondary
  secondary: '#625B71',
  onSecondary: '#FFFFFF',
  secondaryContainer: '#E8DEF8',
  onSecondaryContainer: '#1E192B',

  // Tertiary
  tertiary: '#7D5260',
  onTertiary: '#FFFFFF',
  tertiaryContainer: '#FFD8E4',
  onTertiaryContainer: '#31111D',

  // Error
  error: '#B3261E',
  onError: '#FFFFFF',
  errorContainer: '#F9DEDC',
  onErrorContainer: '#410E0B',

  // Neutral
  neutral: '#49454F',
  onNeutral: '#FFFFFF',
  neutralVariant: '#79747E',
  onNeutralVariant: '#FFFFFF',

  // Background/Surface
  background: '#FFFBFE',
  onBackground: '#1C1B1F',
  surface: '#FFFBFE',
  onSurface: '#1C1B1F',
  surfaceVariant: '#F5EFF7',
  onSurfaceVariant: '#49454F',

  // Outline
  outline: '#79747E',
  outlineVariant: '#CAC7D0',

  // Scrim
  scrim: '#000000',
};

// Material Design 3 Dark Color System
const md3DarkColors = {
  // Primary
  primary: '#D0BCFF',
  onPrimary: '#381E72',
  primaryContainer: '#4F378B',
  onPrimaryContainer: '#EADDFF',

  // Secondary
  secondary: '#CCC2DC',
  onSecondary: '#332D41',
  secondaryContainer: '#4A4458',
  onSecondaryContainer: '#E8DEF8',

  // Tertiary
  tertiary: '#EFB8C8',
  onTertiary: '#492532',
  tertiaryContainer: '#633B48',
  onTertiaryContainer: '#FFD8E4',

  // Error
  error: '#F2B8B5',
  onError: '#601410',
  errorContainer: '#8C1D18',
  onErrorContainer: '#F9DEDC',

  // Neutral
  neutral: '#CAC4D0',
  onNeutral: '#313033',
  neutralVariant: '#958E9E',
  onNeutralVariant: '#313033',

  // Background/Surface
  background: '#1C1B1F',
  onBackground: '#E6E1E5',
  surface: '#1C1B1F',
  onSurface: '#E6E1E5',
  surfaceVariant: '#49454F',
  onSurfaceVariant: '#CAC4D0',

  // Outline
  outline: '#938F99',
  outlineVariant: '#49454F',

  // Scrim
  scrim: '#000000',
};

// High Contrast Color System
const md3HighContrastColors = {
  // Primary
  primary: '#FFFFFF',
  onPrimary: '#000000',
  primaryContainer: '#FFFF00',
  onPrimaryContainer: '#000000',

  // Secondary
  secondary: '#FFFFFF',
  onSecondary: '#000000',
  secondaryContainer: '#00FFFF',
  onSecondaryContainer: '#000000',

  // Tertiary
  tertiary: '#FFFFFF',
  onTertiary: '#000000',
  tertiaryContainer: '#FF00FF',
  onTertiaryContainer: '#000000',

  // Error
  error: '#FF0000',
  onError: '#FFFFFF',
  errorContainer: '#FFCCCC',
  onErrorContainer: '#000000',

  // Neutral
  neutral: '#FFFFFF',
  onNeutral: '#000000',
  neutralVariant: '#FFFFFF',
  onNeutralVariant: '#000000',

  // Background/Surface
  background: '#000000',
  onBackground: '#FFFFFF',
  surface: '#000000',
  onSurface: '#FFFFFF',
  surfaceVariant: '#1A1A1A',
  onSurfaceVariant: '#FFFFFF',

  // Outline
  outline: '#FFFFFF',
  outlineVariant: '#CCCCCC',

  // Scrim
  scrim: '#000000',
};

// Alias for backward compatibility
const md3ExpressiveColors = md3LightColors;

// Common typography settings shared across all themes
const commonTypography = {
  fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
  // MD3 Display Styles
  h1: {
    fontSize: '3.5rem',
    fontWeight: 400,
    lineHeight: 1.1,
    letterSpacing: '0em',
    marginBottom: '2rem',
  },
  h2: {
    fontSize: '2.875rem',
    fontWeight: 400,
    lineHeight: 1.15,
    letterSpacing: '0em',
    marginBottom: '1.75rem',
  },
  h3: {
    fontSize: '2.375rem',
    fontWeight: 500,
    lineHeight: 1.2,
    letterSpacing: '0em',
    marginBottom: '1.5rem',
  },
  h4: {
    fontSize: '1.875rem',
    fontWeight: 500,
    lineHeight: 1.25,
    letterSpacing: '0.015625em',
    marginBottom: '1.25rem',
  },
  h5: {
    fontSize: '1.5rem',
    fontWeight: 500,
    lineHeight: 1.33,
    letterSpacing: '0em',
    marginBottom: '1rem',
  },
  h6: {
    fontSize: '1.25rem',
    fontWeight: 500,
    lineHeight: 1.4,
    letterSpacing: '0.0125em',
    marginBottom: '0.75rem',
  },
  // MD3 Body Styles
  body1: {
    fontSize: '1rem',
    fontWeight: 400,
    lineHeight: 1.5,
    letterSpacing: '0.03125em',
  },
  body2: {
    fontSize: '0.875rem',
    fontWeight: 400,
    lineHeight: 1.43,
    letterSpacing: '0.015625em',
  },
  // MD3 Label Styles
  button: {
    textTransform: 'none' as const,
    fontWeight: 500,
    fontSize: '0.875rem',
    letterSpacing: '0.0625em',
    lineHeight: 1.25,
  },
  caption: {
    fontSize: '0.75rem',
    fontWeight: 500,
    lineHeight: 1.33,
    letterSpacing: '0.0333em',
  },
};

// Common shape settings
const commonShape = {
  borderRadius: 12,
};

// Common shadows
const commonShadows = [
  'none',
  '0px 2px 1px -1px rgba(0,0,0,0.2),0px 1px 1px 0px rgba(0,0,0,0.14),0px 1px 3px 0px rgba(0,0,0,0.12)',
  '0px 3px 1px -2px rgba(0,0,0,0.2),0px 2px 2px 0px rgba(0,0,0,0.14),0px 1px 5px 0px rgba(0,0,0,0.12)',
  '0px 3px 3px -2px rgba(0,0,0,0.2),0px 3px 4px 0px rgba(0,0,0,0.14),0px 1px 8px 0px rgba(0,0,0,0.12)',
  '0px 2px 4px -1px rgba(0,0,0,0.2),0px 4px 5px 0px rgba(0,0,0,0.14),0px 1px 10px 0px rgba(0,0,0,0.12)',
  '0px 3px 5px -1px rgba(0,0,0,0.2),0px 5px 8px 0px rgba(0,0,0,0.14),0px 1px 14px 0px rgba(0,0,0,0.12)',
  '0px 3px 5px -1px rgba(0,0,0,0.2),0px 6px 10px 0px rgba(0,0,0,0.14),0px 1px 18px 0px rgba(0,0,0,0.12)',
  '0px 4px 5px -2px rgba(0,0,0,0.2),0px 7px 10px 1px rgba(0,0,0,0.14),0px 2px 16px 1px rgba(0,0,0,0.12)',
  '0px 5px 5px -3px rgba(0,0,0,0.2),0px 8px 10px 1px rgba(0,0,0,0.14),0px 3px 14px 2px rgba(0,0,0,0.12)',
  '0px 5px 6px -3px rgba(0,0,0,0.2),0px 9px 12px 1px rgba(0,0,0,0.14),0px 3px 16px 2px rgba(0,0,0,0.12)',
  '0px 6px 6px -3px rgba(0,0,0,0.2),0px 10px 14px 1px rgba(0,0,0,0.14),0px 4px 18px 3px rgba(0,0,0,0.12)',
  '0px 6px 7px -4px rgba(0,0,0,0.2),0px 11px 15px 1px rgba(0,0,0,0.14),0px 4px 20px 3px rgba(0,0,0,0.12)',
  '0px 7px 8px -4px rgba(0,0,0,0.2),0px 12px 17px 2px rgba(0,0,0,0.14),0px 5px 22px 4px rgba(0,0,0,0.12)',
  '0px 7px 8px -4px rgba(0,0,0,0.2),0px 13px 19px 2px rgba(0,0,0,0.14),0px 5px 24px 4px rgba(0,0,0,0.12)',
  '0px 7px 9px -4px rgba(0,0,0,0.2),0px 14px 21px 2px rgba(0,0,0,0.14),0px 5px 26px 4px rgba(0,0,0,0.12)',
  '0px 8px 9px -5px rgba(0,0,0,0.2),0px 15px 22px 2px rgba(0,0,0,0.14),0px 6px 28px 5px rgba(0,0,0,0.12)',
  '0px 8px 10px -5px rgba(0,0,0,0.2),0px 16px 24px 2px rgba(0,0,0,0.14),0px 6px 30px 5px rgba(0,0,0,0.12)',
  '0px 8px 11px -5px rgba(0,0,0,0.2),0px 17px 26px 2px rgba(0,0,0,0.14),0px 6px 32px 5px rgba(0,0,0,0.12)',
  '0px 9px 11px -5px rgba(0,0,0,0.2),0px 18px 28px 2px rgba(0,0,0,0.14),0px 7px 34px 6px rgba(0,0,0,0.12)',
  '0px 9px 12px -6px rgba(0,0,0,0.2),0px 19px 29px 2px rgba(0,0,0,0.14),0px 7px 36px 6px rgba(0,0,0,0.12)',
  '0px 10px 13px -6px rgba(0,0,0,0.2),0px 20px 31px 3px rgba(0,0,0,0.14),0px 8px 38px 7px rgba(0,0,0,0.12)',
  '0px 10px 13px -6px rgba(0,0,0,0.2),0px 21px 33px 3px rgba(0,0,0,0.14),0px 8px 40px 7px rgba(0,0,0,0.12)',
  '0px 10px 14px -6px rgba(0,0,0,0.2),0px 22px 35px 3px rgba(0,0,0,0.14),0px 8px 42px 7px rgba(0,0,0,0.12)',
  '0px 11px 14px -7px rgba(0,0,0,0.2),0px 23px 36px 3px rgba(0,0,0,0.14),0px 9px 44px 8px rgba(0,0,0,0.12)',
  '0px 11px 15px -7px rgba(0,0,0,0.2),0px 24px 38px 3px rgba(0,0,0,0.14),0px 9px 46px 8px rgba(0,0,0,0.12)',
] as ThemeOptions['shadows'];

// Function to get component overrides for a specific theme mode
const getComponentOverrides = (colors: typeof md3LightColors, isDark: boolean): ThemeOptions['components'] => ({
  MuiButton: {
    styleOverrides: {
      root: {
        textTransform: 'none',
        fontWeight: 500,
        borderRadius: '24px',
        padding: '10px 24px',
        fontSize: '0.875rem',
        letterSpacing: '0.0625em',
        transition: 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)',
        '&:hover': {
          transform: 'translateY(-2px)',
        },
      },
      contained: {
        boxShadow: isDark ? '0px 3px 8px rgba(0, 0, 0, 0.3)' : '0px 3px 8px rgba(0, 0, 0, 0.12)',
        '&:hover': {
          boxShadow: isDark ? '0px 6px 16px rgba(0, 0, 0, 0.4)' : '0px 6px 16px rgba(0, 0, 0, 0.16)',
        },
      },
      outlined: {
        borderWidth: '2px',
        '&:hover': {
          borderWidth: '2px',
          backgroundColor: isDark ? 'rgba(208, 188, 255, 0.08)' : 'rgba(103, 80, 164, 0.04)',
        },
      },
      text: {
        '&:hover': {
          backgroundColor: isDark ? 'rgba(208, 188, 255, 0.12)' : 'rgba(103, 80, 164, 0.08)',
        },
      },
    },
    defaultProps: {
      disableRipple: false,
    },
  },
  MuiTextField: {
    defaultProps: {
      variant: 'outlined',
      fullWidth: true,
    },
    styleOverrides: {
      root: {
        '& .MuiOutlinedInput-root': {
          borderRadius: '12px',
          backgroundColor: isDark ? 'rgba(208, 188, 255, 0.08)' : 'rgba(103, 80, 164, 0.04)',
          transition: 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)',
          '&:hover': {
            backgroundColor: isDark ? 'rgba(208, 188, 255, 0.12)' : 'rgba(103, 80, 164, 0.08)',
          },
          '&.Mui-focused': {
            backgroundColor: 'transparent',
          },
        },
        '& .MuiOutlinedInput-notchedOutline': {
          borderColor: colors.outlineVariant,
        },
      },
    },
  },
  MuiCard: {
    styleOverrides: {
      root: {
        borderRadius: '12px',
        backgroundColor: colors.surface,
        boxShadow: isDark 
          ? '0px 1px 3px rgba(0, 0, 0, 0.3), 0px 1px 2px rgba(0, 0, 0, 0.4)'
          : '0px 1px 3px rgba(0, 0, 0, 0.12), 0px 1px 2px rgba(0, 0, 0, 0.24)',
        transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
        '&:hover': {
          boxShadow: isDark
            ? '0px 6px 12px rgba(0, 0, 0, 0.35), 0px 3px 6px rgba(0, 0, 0, 0.25)'
            : '0px 6px 12px rgba(0, 0, 0, 0.15), 0px 3px 6px rgba(0, 0, 0, 0.1)',
        },
      },
    },
  },
  MuiPaper: {
    styleOverrides: {
      root: {
        backgroundImage: 'none',
        backgroundColor: colors.surface,
      },
      elevation0: {
        backgroundColor: colors.background,
      },
    },
  },
  MuiAppBar: {
    styleOverrides: {
      root: {
        backgroundColor: colors.primary,
        boxShadow: isDark ? '0px 3px 6px rgba(0, 0, 0, 0.3)' : '0px 3px 6px rgba(0, 0, 0, 0.1)',
      },
    },
  },
  MuiTabs: {
    styleOverrides: {
      root: {
        borderBottom: `1px solid ${colors.outlineVariant}`,
      },
      indicator: {
        height: '3px',
        borderRadius: '3px 3px 0 0',
      },
    },
  },
  MuiTab: {
    styleOverrides: {
      root: {
        textTransform: 'none',
        fontWeight: 500,
        fontSize: '0.9375rem',
        letterSpacing: '0.0125em',
        padding: '12px 16px',
        minHeight: '48px',
        color: colors.onSurfaceVariant,
        '&.Mui-selected': {
          color: colors.primary,
        },
      },
    },
  },
  MuiChip: {
    styleOverrides: {
      root: {
        borderRadius: '8px',
        fontWeight: 500,
        fontSize: '0.8125rem',
        letterSpacing: '0.0463em',
      },
      filled: {
        backgroundColor: colors.primaryContainer,
        color: colors.onPrimaryContainer,
      },
    },
  },
  MuiAlert: {
    styleOverrides: {
      root: {
        borderRadius: '12px',
        fontSize: '0.875rem',
        fontWeight: 500,
      },
    },
  },
  MuiDrawer: {
    styleOverrides: {
      paper: {
        backgroundColor: colors.background,
        borderRight: `1px solid ${colors.outlineVariant}`,
      },
    },
  },
  MuiListItem: {
    styleOverrides: {
      root: {
        borderRadius: '8px',
        marginBottom: '4px',
        transition: 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)',
        '&:hover': {
          backgroundColor: isDark ? 'rgba(208, 188, 255, 0.12)' : 'rgba(103, 80, 164, 0.08)',
        },
        '&.Mui-selected': {
          backgroundColor: isDark ? 'rgba(208, 188, 255, 0.16)' : 'rgba(103, 80, 164, 0.12)',
          '&:hover': {
            backgroundColor: isDark ? 'rgba(208, 188, 255, 0.2)' : 'rgba(103, 80, 164, 0.16)',
          },
        },
      },
    },
  },
  MuiCssBaseline: {
    styleOverrides: {
      body: {
        backgroundColor: colors.background,
        color: colors.onBackground,
      },
    },
  },
  MuiDivider: {
    styleOverrides: {
      root: {
        borderColor: colors.outlineVariant,
      },
    },
  },
  MuiDialog: {
    styleOverrides: {
      paper: {
        backgroundColor: colors.surface,
      },
    },
  },
  MuiMenu: {
    styleOverrides: {
      paper: {
        backgroundColor: colors.surface,
      },
    },
  },
  MuiSelect: {
    styleOverrides: {
      root: {
        '& .MuiOutlinedInput-notchedOutline': {
          borderColor: colors.outlineVariant,
        },
      },
    },
  },
  MuiTooltip: {
    styleOverrides: {
      tooltip: {
        backgroundColor: isDark ? colors.surfaceVariant : colors.neutral,
        color: isDark ? colors.onSurfaceVariant : colors.onNeutral,
      },
    },
  },
  MuiTableCell: {
    styleOverrides: {
      root: {
        borderColor: colors.outlineVariant,
      },
      head: {
        backgroundColor: isDark ? colors.surfaceVariant : '#F5F5F5',
        color: colors.onSurface,
        fontWeight: 600,
      },
    },
  },
  MuiTableRow: {
    styleOverrides: {
      root: {
        '&:hover': {
          backgroundColor: isDark ? 'rgba(208, 188, 255, 0.08)' : 'rgba(103, 80, 164, 0.04)',
        },
      },
    },
  },
  MuiInputLabel: {
    styleOverrides: {
      root: {
        color: colors.onSurfaceVariant,
      },
    },
  },
});

// Create a theme based on mode
export const createAppTheme = (mode: 'light' | 'dark' | 'high-contrast'): Theme => {
  const isDark = mode === 'dark' || mode === 'high-contrast';
  const colors = mode === 'dark' ? md3DarkColors : mode === 'high-contrast' ? md3HighContrastColors : md3LightColors;
  
  return createTheme({
    palette: {
      mode: isDark ? 'dark' : 'light',
      primary: {
        main: colors.primary,
        light: colors.primaryContainer,
        dark: isDark ? '#381E72' : '#4F378A',
        contrastText: colors.onPrimary,
      },
      secondary: {
        main: colors.secondary,
        light: colors.secondaryContainer,
        dark: isDark ? '#332D41' : '#4A4458',
        contrastText: colors.onSecondary,
      },
      success: {
        main: isDark ? '#7DD99F' : '#06A77D',
        light: isDark ? '#B3F5D5' : '#B3F5D5',
        dark: isDark ? '#005146' : '#005146',
        contrastText: isDark ? '#003921' : '#FFFFFF',
      },
      error: {
        main: colors.error,
        light: colors.errorContainer,
        dark: isDark ? '#601410' : '#601410',
        contrastText: colors.onError,
      },
      warning: {
        main: isDark ? '#FFD54F' : '#E8A800',
        light: isDark ? '#FFE082' : '#FFE082',
        dark: isDark ? '#6D5500' : '#6D5500',
        contrastText: isDark ? '#3E2D00' : '#FFFFFF',
      },
      info: {
        main: isDark ? '#4FC3F7' : '#0092BC',
        light: isDark ? '#B3E5FC' : '#B3E5FC',
        dark: isDark ? '#003A5C' : '#003A5C',
        contrastText: isDark ? '#003A5C' : '#FFFFFF',
      },
      background: {
        default: colors.background,
        paper: colors.surface,
      },
      text: {
        primary: colors.onSurface,
        secondary: colors.onSurfaceVariant,
      },
      divider: colors.outlineVariant,
      action: {
        active: colors.onSurface,
        hover: isDark ? 'rgba(255, 255, 255, 0.08)' : 'rgba(0, 0, 0, 0.04)',
        selected: isDark ? 'rgba(255, 255, 255, 0.16)' : 'rgba(0, 0, 0, 0.08)',
        disabled: isDark ? 'rgba(255, 255, 255, 0.3)' : 'rgba(0, 0, 0, 0.26)',
        disabledBackground: isDark ? 'rgba(255, 255, 255, 0.12)' : 'rgba(0, 0, 0, 0.12)',
      },
    },
    typography: commonTypography,
    shape: commonShape,
    shadows: commonShadows,
    components: getComponentOverrides(colors, isDark),
  });
};

// Export light theme as default for backward compatibility
export const muiTheme = createAppTheme('light');

// Export individual theme creators for convenience
export const lightTheme = createAppTheme('light');
export const darkTheme = createAppTheme('dark');
export const highContrastTheme = createAppTheme('high-contrast');

// Export type for theme mode
export type ThemeMode = 'system' | 'light' | 'dark' | 'high-contrast';

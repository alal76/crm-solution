import { createTheme } from '@mui/material/styles';

// Material Design 3 Expressive Color System
const md3ExpressiveColors = {
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

export const muiTheme = createTheme({
  palette: {
    primary: {
      main: md3ExpressiveColors.primary,
      light: md3ExpressiveColors.primaryContainer,
      dark: '#4F378A',
      contrastText: md3ExpressiveColors.onPrimary,
    },
    secondary: {
      main: md3ExpressiveColors.secondary,
      light: md3ExpressiveColors.secondaryContainer,
      dark: '#4A4458',
      contrastText: md3ExpressiveColors.onSecondary,
    },
    success: {
      main: '#06A77D',
      light: '#B3F5D5',
      dark: '#005146',
      contrastText: '#FFFFFF',
    },
    error: {
      main: md3ExpressiveColors.error,
      light: md3ExpressiveColors.errorContainer,
      dark: '#601410',
      contrastText: md3ExpressiveColors.onError,
    },
    warning: {
      main: '#E8A800',
      light: '#FFE082',
      dark: '#6D5500',
      contrastText: '#FFFFFF',
    },
    info: {
      main: '#0092BC',
      light: '#B3E5FC',
      dark: '#003A5C',
      contrastText: '#FFFFFF',
    },
    background: {
      default: md3ExpressiveColors.background,
      paper: md3ExpressiveColors.surface,
    },
  },
  typography: {
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
      textTransform: 'none',
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
  },
  shape: {
    borderRadius: 12,
  },
  shadows: [
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
  ],
  components: {
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
          boxShadow: '0px 3px 8px rgba(0, 0, 0, 0.12)',
          '&:hover': {
            boxShadow: '0px 6px 16px rgba(0, 0, 0, 0.16)',
          },
        },
        outlined: {
          borderWidth: '2px',
          '&:hover': {
            borderWidth: '2px',
            backgroundColor: 'rgba(103, 80, 164, 0.04)',
          },
        },
        text: {
          '&:hover': {
            backgroundColor: 'rgba(103, 80, 164, 0.08)',
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
            backgroundColor: 'rgba(103, 80, 164, 0.04)',
            transition: 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)',
            '&:hover': {
              backgroundColor: 'rgba(103, 80, 164, 0.08)',
            },
            '&.Mui-focused': {
              backgroundColor: 'transparent',
            },
          },
          '& .MuiOutlinedInput-notchedOutline': {
            borderColor: '#CAC7D0',
          },
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: '12px',
          backgroundColor: md3ExpressiveColors.surface,
          boxShadow: '0px 1px 3px rgba(0, 0, 0, 0.12), 0px 1px 2px rgba(0, 0, 0, 0.24)',
          transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
          '&:hover': {
            boxShadow: '0px 6px 12px rgba(0, 0, 0, 0.15), 0px 3px 6px rgba(0, 0, 0, 0.1)',
          },
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: 'none',
          backgroundColor: md3ExpressiveColors.surface,
        },
        elevation0: {
          backgroundColor: md3ExpressiveColors.background,
        },
      },
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          backgroundColor: md3ExpressiveColors.primary,
          boxShadow: '0px 3px 6px rgba(0, 0, 0, 0.1)',
        },
      },
    },
    MuiTabs: {
      styleOverrides: {
        root: {
          borderBottom: `1px solid #E0E0E0`,
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
          color: md3ExpressiveColors.onSurfaceVariant,
          '&.Mui-selected': {
            color: md3ExpressiveColors.primary,
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
          backgroundColor: md3ExpressiveColors.primaryContainer,
          color: md3ExpressiveColors.onPrimaryContainer,
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
          backgroundColor: md3ExpressiveColors.background,
          borderRight: `1px solid #E0E0E0`,
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
            backgroundColor: 'rgba(103, 80, 164, 0.08)',
          },
          '&.Mui-selected': {
            backgroundColor: 'rgba(103, 80, 164, 0.12)',
            '&:hover': {
              backgroundColor: 'rgba(103, 80, 164, 0.16)',
            },
          },
        },
      },
    },
  },
});

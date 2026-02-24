/**
 * DarkModeToggle - Toggle component for dark/light mode
 */

import React from 'react';
import {
  IconButton,
  Tooltip,
  ToggleButton,
  ToggleButtonGroup,
  Box,
  Typography,
  Stack,
  useTheme,
} from '@mui/material';
import {
  DarkMode as DarkModeIcon,
  LightMode as LightModeIcon,
  SettingsBrightness as AutoIcon,
  Contrast as ContrastIcon,
} from '@mui/icons-material';
import { useTheme as useAppTheme } from '../../contexts/ThemeContext';
import { ThemeMode } from '../../theme/muiTheme';

export interface DarkModeToggleProps {
  variant?: 'icon' | 'toggle' | 'menu';
  showLabel?: boolean;
  showHighContrast?: boolean;
  size?: 'small' | 'medium';
}

export const DarkModeToggle: React.FC<DarkModeToggleProps> = ({
  variant = 'icon',
  showLabel = false,
  showHighContrast = true,
  size = 'medium',
}) => {
  const theme = useTheme();
  const { themeMode, effectiveTheme, setThemeMode } = useAppTheme();

  // Icon variant - simple toggle button
  if (variant === 'icon') {
    // Cycle through modes: system -> light -> dark -> high-contrast -> system
    const handleClick = () => {
      const modes: ThemeMode[] = showHighContrast
        ? ['system', 'light', 'dark', 'high-contrast']
        : ['system', 'light', 'dark'];
      const currentIndex = modes.indexOf(themeMode);
      const nextIndex = (currentIndex + 1) % modes.length;
      setThemeMode(modes[nextIndex]);
    };

    const getIcon = () => {
      switch (effectiveTheme) {
        case 'dark':
          return <DarkModeIcon />;
        case 'high-contrast':
          return <ContrastIcon />;
        default:
          return <LightModeIcon />;
      }
    };

    const getTooltip = () => {
      switch (themeMode) {
        case 'system':
          return `System theme (${effectiveTheme})`;
        case 'dark':
          return 'Dark mode';
        case 'high-contrast':
          return 'High contrast mode';
        default:
          return 'Light mode';
      }
    };

    return (
      <Tooltip title={getTooltip()}>
        <IconButton
          onClick={handleClick}
          aria-label={`Toggle theme. Current: ${getTooltip()}`}
          size={size}
          sx={{
            color: theme.palette.mode === 'dark' ? 'primary.light' : 'inherit',
          }}
        >
          {getIcon()}
        </IconButton>
      </Tooltip>
    );
  }

  // Toggle variant - ToggleButtonGroup
  if (variant === 'toggle') {
    return (
      <Box>
        {showLabel && (
          <Typography variant="subtitle2" sx={{ mb: 1 }}>
            Theme
          </Typography>
        )}
        <ToggleButtonGroup
          value={themeMode}
          exclusive
          onChange={(_, value) => {
            if (value) setThemeMode(value as ThemeMode);
          }}
          aria-label="Theme selection"
          size={size}
        >
          <ToggleButton value="system" aria-label="System theme">
            <Tooltip title="System default">
              <AutoIcon />
            </Tooltip>
          </ToggleButton>
          <ToggleButton value="light" aria-label="Light theme">
            <Tooltip title="Light mode">
              <LightModeIcon />
            </Tooltip>
          </ToggleButton>
          <ToggleButton value="dark" aria-label="Dark theme">
            <Tooltip title="Dark mode">
              <DarkModeIcon />
            </Tooltip>
          </ToggleButton>
          {showHighContrast && (
            <ToggleButton value="high-contrast" aria-label="High contrast theme">
              <Tooltip title="High contrast">
                <ContrastIcon />
              </Tooltip>
            </ToggleButton>
          )}
        </ToggleButtonGroup>
      </Box>
    );
  }

  // Menu variant - Stack with labels
  return (
    <Box>
      {showLabel && (
        <Typography variant="subtitle2" sx={{ mb: 1 }}>
          Theme
        </Typography>
      )}
      <Stack spacing={0.5}>
        <ToggleButton
          value="system"
          selected={themeMode === 'system'}
          onChange={() => setThemeMode('system')}
          fullWidth
          sx={{ justifyContent: 'flex-start', px: 2 }}
        >
          <AutoIcon sx={{ mr: 1 }} />
          System
        </ToggleButton>
        <ToggleButton
          value="light"
          selected={themeMode === 'light'}
          onChange={() => setThemeMode('light')}
          fullWidth
          sx={{ justifyContent: 'flex-start', px: 2 }}
        >
          <LightModeIcon sx={{ mr: 1 }} />
          Light
        </ToggleButton>
        <ToggleButton
          value="dark"
          selected={themeMode === 'dark'}
          onChange={() => setThemeMode('dark')}
          fullWidth
          sx={{ justifyContent: 'flex-start', px: 2 }}
        >
          <DarkModeIcon sx={{ mr: 1 }} />
          Dark
        </ToggleButton>
        {showHighContrast && (
          <ToggleButton
            value="high-contrast"
            selected={themeMode === 'high-contrast'}
            onChange={() => setThemeMode('high-contrast')}
            fullWidth
            sx={{ justifyContent: 'flex-start', px: 2 }}
          >
            <ContrastIcon sx={{ mr: 1 }} />
            High Contrast
          </ToggleButton>
        )}
      </Stack>
    </Box>
  );
};

export default DarkModeToggle;

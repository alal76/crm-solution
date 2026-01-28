/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  FormControl,
  FormLabel,
  RadioGroup,
  FormControlLabel,
  Radio,
  Box,
  Typography,
  Alert,
  CircularProgress,
  Divider,
  Select,
  MenuItem,
  InputLabel,
  Switch,
  Tabs,
  Tab,
  Grid,
} from '@mui/material';
import {
  LightMode as LightModeIcon,
  DarkMode as DarkModeIcon,
  Contrast as ContrastIcon,
  Settings as SettingsIcon,
  SettingsBrightness as SystemIcon,
  Language as LanguageIcon,
  Schedule as TimezoneIcon,
  TableRows as RowsIcon,
  Notifications as NotificationsIcon,
  Palette as PaletteIcon,
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import { useTheme } from '../contexts/ThemeContext';
import { ThemeMode } from '../theme/muiTheme';

interface UserSettingsDialogProps {
  open: boolean;
  onClose: () => void;
  onThemeChange?: (theme: string) => void;
}

interface UserPreferences {
  themePreference: string;
  headerColor?: string;
  language?: string;
  timezone?: string;
  dateFormat?: string;
  timeFormat?: string;
  rowsPerPage?: number;
  emailNotifications?: boolean;
  desktopNotifications?: boolean;
  compactMode?: boolean;
}

const THEME_OPTIONS = [
  { 
    value: 'system', 
    label: 'System Default', 
    description: 'Follows your operating system preference',
    icon: <SystemIcon sx={{ color: '#9C27B0' }} />
  },
  { 
    value: 'light', 
    label: 'Light Mode', 
    description: 'Standard light theme',
    icon: <LightModeIcon sx={{ color: '#FFA726' }} />
  },
  { 
    value: 'dark', 
    label: 'Dark Mode', 
    description: 'Dark theme for reduced eye strain',
    icon: <DarkModeIcon sx={{ color: '#5C6BC0' }} />
  },
  { 
    value: 'high-contrast', 
    label: 'High Contrast', 
    description: 'Enhanced visibility for accessibility',
    icon: <ContrastIcon sx={{ color: '#424242' }} />
  },
];

const LANGUAGE_OPTIONS = [
  { value: 'en', label: 'English' },
  { value: 'es', label: 'Spanish (Español)' },
  { value: 'fr', label: 'French (Français)' },
  { value: 'de', label: 'German (Deutsch)' },
  { value: 'pt', label: 'Portuguese (Português)' },
  { value: 'zh', label: 'Chinese (中文)' },
  { value: 'ja', label: 'Japanese (日本語)' },
  { value: 'hi', label: 'Hindi (हिन्दी)' },
];

const TIMEZONE_OPTIONS = [
  { value: 'UTC', label: 'UTC' },
  { value: 'America/New_York', label: 'Eastern Time (US)' },
  { value: 'America/Chicago', label: 'Central Time (US)' },
  { value: 'America/Denver', label: 'Mountain Time (US)' },
  { value: 'America/Los_Angeles', label: 'Pacific Time (US)' },
  { value: 'Europe/London', label: 'London (UK)' },
  { value: 'Europe/Paris', label: 'Paris (France)' },
  { value: 'Europe/Berlin', label: 'Berlin (Germany)' },
  { value: 'Asia/Tokyo', label: 'Tokyo (Japan)' },
  { value: 'Asia/Shanghai', label: 'Shanghai (China)' },
  { value: 'Asia/Kolkata', label: 'India Standard Time' },
  { value: 'Australia/Sydney', label: 'Sydney (Australia)' },
];

const DATE_FORMAT_OPTIONS = [
  { value: 'MM/DD/YYYY', label: 'MM/DD/YYYY (US)' },
  { value: 'DD/MM/YYYY', label: 'DD/MM/YYYY (International)' },
  { value: 'YYYY-MM-DD', label: 'YYYY-MM-DD (ISO)' },
  { value: 'DD.MM.YYYY', label: 'DD.MM.YYYY (European)' },
];

const TIME_FORMAT_OPTIONS = [
  { value: '12h', label: '12-hour (AM/PM)' },
  { value: '24h', label: '24-hour' },
];

const ROWS_PER_PAGE_OPTIONS = [10, 15, 20, 25, 50, 100];

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`settings-tabpanel-${index}`}
      aria-labelledby={`settings-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ pt: 2 }}>{children}</Box>}
    </div>
  );
}

const UserSettingsDialog: React.FC<UserSettingsDialogProps> = ({ open, onClose, onThemeChange }) => {
  const { themeMode, setThemeMode } = useTheme();
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [tabValue, setTabValue] = useState(0);
  const [preferences, setPreferences] = useState<UserPreferences>({
    themePreference: themeMode,
    language: 'en',
    timezone: Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC',
    dateFormat: 'MM/DD/YYYY',
    timeFormat: '12h',
    rowsPerPage: 20,
    emailNotifications: true,
    desktopNotifications: false,
    compactMode: false,
  });

  // Detect system theme preference
  const getSystemTheme = () => {
    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
      return 'dark';
    }
    return 'light';
  };

  // Get effective theme (resolve 'system' to actual theme)
  const getEffectiveTheme = (theme: string) => {
    if (theme === 'system') {
      return getSystemTheme();
    }
    return theme;
  };

  // Load user preferences when dialog opens
  useEffect(() => {
    if (open) {
      loadPreferences();
    }
  }, [open]);

  // Listen for system theme changes
  useEffect(() => {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    const handleChange = () => {
      if (preferences.themePreference === 'system' && onThemeChange) {
        onThemeChange(getSystemTheme());
      }
    };
    mediaQuery.addEventListener('change', handleChange);
    return () => mediaQuery.removeEventListener('change', handleChange);
  }, [preferences.themePreference, onThemeChange]);

  const loadPreferences = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await apiClient.get<UserPreferences>('/users/me/preferences');
      setPreferences(prev => ({
        ...prev,
        ...response.data,
        themePreference: response.data.themePreference || 'system',
      }));
    } catch (err: any) {
      console.error('Failed to load preferences:', err);
      // Try to get from localStorage as fallback
      const savedTheme = localStorage.getItem('crm_theme_preference');
      const savedPrefs = localStorage.getItem('crm_user_preferences');
      if (savedPrefs) {
        try {
          const parsed = JSON.parse(savedPrefs);
          setPreferences(prev => ({ ...prev, ...parsed }));
        } catch (e) {
          // Ignore parse errors
        }
      } else if (savedTheme) {
        setPreferences(prev => ({ ...prev, themePreference: savedTheme }));
      }
    } finally {
      setLoading(false);
    }
  };

  const handleThemeChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newTheme = event.target.value as ThemeMode;
    setPreferences(prev => ({ ...prev, themePreference: newTheme }));
    // Apply theme immediately for live preview
    setThemeMode(newTheme);
    setSuccess(false);
  };

  const handlePreferenceChange = (field: keyof UserPreferences, value: any) => {
    setPreferences(prev => ({ ...prev, [field]: value }));
    setSuccess(false);
  };

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const handleSave = async () => {
    setSaving(true);
    setError(null);
    setSuccess(false);
    try {
      await apiClient.put('/users/me/preferences', preferences);
      
      // Save to localStorage for immediate effect
      localStorage.setItem('crm_theme_preference', preferences.themePreference);
      localStorage.setItem('crm_user_preferences', JSON.stringify(preferences));
      
      // Apply theme through context (already applied on change, but ensure it's saved)
      setThemeMode(preferences.themePreference as ThemeMode);
      
      // Notify parent of theme change (legacy callback)
      if (onThemeChange) {
        onThemeChange(getEffectiveTheme(preferences.themePreference));
      }
      
      setSuccess(true);
      
      // Close after short delay to show success
      setTimeout(() => {
        onClose();
      }, 1000);
    } catch (err: any) {
      console.error('Failed to save preferences:', err);
      setError(err.response?.data?.message || 'Failed to save preferences');
    } finally {
      setSaving(false);
    }
  };

  const handleClose = () => {
    setError(null);
    setSuccess(false);
    setTabValue(0);
    onClose();
  };

  return (
    <Dialog 
      open={open} 
      onClose={handleClose}
      maxWidth="md"
      fullWidth
      PaperProps={{
        sx: { borderRadius: 2, minHeight: 500 }
      }}
    >
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1, pb: 1 }}>
        <SettingsIcon color="primary" />
        <Typography variant="h6" component="span">
          User Preferences
        </Typography>
      </DialogTitle>
      
      <Divider />
      
      <DialogContent sx={{ pt: 0, px: 0 }}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <>
            <Box sx={{ borderBottom: 1, borderColor: 'divider', px: 2 }}>
              <Tabs value={tabValue} onChange={handleTabChange} aria-label="settings tabs">
                <Tab icon={<PaletteIcon />} iconPosition="start" label="Appearance" />
                <Tab icon={<LanguageIcon />} iconPosition="start" label="Regional" />
                <Tab icon={<NotificationsIcon />} iconPosition="start" label="Notifications" />
              </Tabs>
            </Box>
            
            <Box sx={{ px: 3 }}>
              {error && (
                <Alert severity="error" sx={{ mt: 2 }}>
                  {error}
                </Alert>
              )}
              
              {success && (
                <Alert severity="success" sx={{ mt: 2 }}>
                  Settings saved successfully!
                </Alert>
              )}

              {/* Appearance Tab */}
              <TabPanel value={tabValue} index={0}>
                <FormControl component="fieldset" fullWidth>
                  <FormLabel component="legend" sx={{ fontWeight: 600, color: 'text.primary', mb: 2 }}>
                    Theme Preference
                  </FormLabel>
                  <RadioGroup
                    value={preferences.themePreference}
                    onChange={handleThemeChange}
                  >
                    {THEME_OPTIONS.map((option) => (
                      <Box
                        key={option.value}
                        sx={{
                          border: '1px solid',
                          borderColor: preferences.themePreference === option.value ? 'primary.main' : 'divider',
                          borderRadius: 1,
                          mb: 1.5,
                          p: 1.5,
                          cursor: 'pointer',
                          transition: 'all 0.2s',
                          backgroundColor: preferences.themePreference === option.value ? 'action.selected' : 'transparent',
                          '&:hover': {
                            borderColor: 'primary.main',
                            backgroundColor: 'action.hover',
                          }
                        }}
                        onClick={() => {
                          setPreferences(prev => ({ ...prev, themePreference: option.value }));
                          setThemeMode(option.value as ThemeMode);
                        }}
                      >
                        <FormControlLabel
                          value={option.value}
                          control={<Radio />}
                          label={
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, ml: 1 }}>
                              {option.icon}
                              <Box>
                                <Typography variant="subtitle1" sx={{ fontWeight: 500 }}>
                                  {option.label}
                                </Typography>
                                <Typography variant="body2" color="text.secondary">
                                  {option.description}
                                </Typography>
                              </Box>
                            </Box>
                          }
                          sx={{ m: 0, width: '100%' }}
                        />
                      </Box>
                    ))}
                  </RadioGroup>
                </FormControl>

                <Divider sx={{ my: 3 }} />

                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <RowsIcon color="action" />
                    <Box>
                      <Typography variant="subtitle2">Compact Mode</Typography>
                      <Typography variant="caption" color="text.secondary">
                        Reduce spacing and padding throughout the interface
                      </Typography>
                    </Box>
                  </Box>
                  <Switch
                    checked={preferences.compactMode || false}
                    onChange={(e) => handlePreferenceChange('compactMode', e.target.checked)}
                  />
                </Box>

                <FormControl fullWidth sx={{ mt: 2 }}>
                  <InputLabel>Rows per Page</InputLabel>
                  <Select
                    value={preferences.rowsPerPage || 20}
                    label="Rows per Page"
                    onChange={(e) => handlePreferenceChange('rowsPerPage', e.target.value)}
                    size="small"
                  >
                    {ROWS_PER_PAGE_OPTIONS.map(num => (
                      <MenuItem key={num} value={num}>{num} rows</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </TabPanel>

              {/* Regional Tab */}
              <TabPanel value={tabValue} index={1}>
                <Grid container spacing={3}>
                  <Grid item xs={12} sm={6}>
                    <FormControl fullWidth>
                      <InputLabel>Language</InputLabel>
                      <Select
                        value={preferences.language || 'en'}
                        label="Language"
                        onChange={(e) => handlePreferenceChange('language', e.target.value)}
                        startAdornment={<LanguageIcon sx={{ mr: 1, color: 'action.active' }} />}
                      >
                        {LANGUAGE_OPTIONS.map(opt => (
                          <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Grid>

                  <Grid item xs={12} sm={6}>
                    <FormControl fullWidth>
                      <InputLabel>Timezone</InputLabel>
                      <Select
                        value={preferences.timezone || 'UTC'}
                        label="Timezone"
                        onChange={(e) => handlePreferenceChange('timezone', e.target.value)}
                        startAdornment={<TimezoneIcon sx={{ mr: 1, color: 'action.active' }} />}
                      >
                        {TIMEZONE_OPTIONS.map(opt => (
                          <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Grid>

                  <Grid item xs={12} sm={6}>
                    <FormControl fullWidth>
                      <InputLabel>Date Format</InputLabel>
                      <Select
                        value={preferences.dateFormat || 'MM/DD/YYYY'}
                        label="Date Format"
                        onChange={(e) => handlePreferenceChange('dateFormat', e.target.value)}
                      >
                        {DATE_FORMAT_OPTIONS.map(opt => (
                          <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Grid>

                  <Grid item xs={12} sm={6}>
                    <FormControl fullWidth>
                      <InputLabel>Time Format</InputLabel>
                      <Select
                        value={preferences.timeFormat || '12h'}
                        label="Time Format"
                        onChange={(e) => handlePreferenceChange('timeFormat', e.target.value)}
                      >
                        {TIME_FORMAT_OPTIONS.map(opt => (
                          <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Grid>
                </Grid>

                <Typography variant="caption" color="text.secondary" sx={{ mt: 3, display: 'block' }}>
                  Regional settings affect how dates, times, and numbers are displayed throughout the application.
                </Typography>
              </TabPanel>

              {/* Notifications Tab */}
              <TabPanel value={tabValue} index={2}>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <NotificationsIcon color="action" />
                    <Box>
                      <Typography variant="subtitle2">Email Notifications</Typography>
                      <Typography variant="caption" color="text.secondary">
                        Receive email notifications for important updates
                      </Typography>
                    </Box>
                  </Box>
                  <Switch
                    checked={preferences.emailNotifications !== false}
                    onChange={(e) => handlePreferenceChange('emailNotifications', e.target.checked)}
                  />
                </Box>

                <Divider />

                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mt: 3 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <NotificationsIcon color="action" />
                    <Box>
                      <Typography variant="subtitle2">Desktop Notifications</Typography>
                      <Typography variant="caption" color="text.secondary">
                        Show browser notifications for real-time alerts
                      </Typography>
                    </Box>
                  </Box>
                  <Switch
                    checked={preferences.desktopNotifications || false}
                    onChange={(e) => handlePreferenceChange('desktopNotifications', e.target.checked)}
                  />
                </Box>

                <Alert severity="info" sx={{ mt: 3 }}>
                  Desktop notifications require browser permission. You may be prompted to allow notifications when enabling this feature.
                </Alert>
              </TabPanel>
            </Box>
          </>
        )}
      </DialogContent>
      
      <Divider />
      
      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={handleClose} disabled={saving}>
          Cancel
        </Button>
        <Button 
          onClick={handleSave} 
          variant="contained" 
          disabled={loading || saving}
          startIcon={saving && <CircularProgress size={16} />}
        >
          {saving ? 'Saving...' : 'Save Preferences'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default UserSettingsDialog;

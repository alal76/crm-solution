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
} from '@mui/material';
import {
  LightMode as LightModeIcon,
  DarkMode as DarkModeIcon,
  Contrast as ContrastIcon,
  Settings as SettingsIcon,
} from '@mui/icons-material';
import apiClient from '../services/apiClient';

interface UserSettingsDialogProps {
  open: boolean;
  onClose: () => void;
  onThemeChange?: (theme: string) => void;
}

interface UserPreferences {
  themePreference: string;
  headerColor?: string;
}

const THEME_OPTIONS = [
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

const UserSettingsDialog: React.FC<UserSettingsDialogProps> = ({ open, onClose, onThemeChange }) => {
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [preferences, setPreferences] = useState<UserPreferences>({
    themePreference: 'light',
  });

  // Load user preferences when dialog opens
  useEffect(() => {
    if (open) {
      loadPreferences();
    }
  }, [open]);

  const loadPreferences = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await apiClient.get<UserPreferences>('/users/me/preferences');
      setPreferences(response.data);
    } catch (err: any) {
      console.error('Failed to load preferences:', err);
      // Try to get from localStorage as fallback
      const savedTheme = localStorage.getItem('crm_theme_preference');
      if (savedTheme) {
        setPreferences(prev => ({ ...prev, themePreference: savedTheme }));
      }
    } finally {
      setLoading(false);
    }
  };

  const handleThemeChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newTheme = event.target.value;
    setPreferences(prev => ({ ...prev, themePreference: newTheme }));
    setSuccess(false);
  };

  const handleSave = async () => {
    setSaving(true);
    setError(null);
    setSuccess(false);
    try {
      await apiClient.put('/users/me/preferences', {
        themePreference: preferences.themePreference,
      });
      
      // Save to localStorage for immediate effect
      localStorage.setItem('crm_theme_preference', preferences.themePreference);
      
      // Notify parent of theme change
      if (onThemeChange) {
        onThemeChange(preferences.themePreference);
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
    onClose();
  };

  return (
    <Dialog 
      open={open} 
      onClose={handleClose}
      maxWidth="sm"
      fullWidth
      PaperProps={{
        sx: { borderRadius: 2 }
      }}
    >
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1, pb: 1 }}>
        <SettingsIcon color="primary" />
        <Typography variant="h6" component="span">
          User Settings
        </Typography>
      </DialogTitle>
      
      <Divider />
      
      <DialogContent sx={{ pt: 3 }}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <>
            {error && (
              <Alert severity="error" sx={{ mb: 2 }}>
                {error}
              </Alert>
            )}
            
            {success && (
              <Alert severity="success" sx={{ mb: 2 }}>
                Settings saved successfully!
              </Alert>
            )}
            
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
                    onClick={() => setPreferences(prev => ({ ...prev, themePreference: option.value }))}
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
            
            <Typography variant="caption" color="text.secondary" sx={{ mt: 2, display: 'block' }}>
              Your theme preference will be saved and applied when you log in on any device.
            </Typography>
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
          {saving ? 'Saving...' : 'Save Settings'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default UserSettingsDialog;

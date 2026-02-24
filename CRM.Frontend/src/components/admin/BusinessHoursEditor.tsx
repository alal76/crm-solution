/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * BusinessHoursEditor.tsx - Admin component for editing business hours
 * TODO-SYS005-004: BusinessHoursEditor.tsx
 */
import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Switch,
  FormControlLabel,
  Button,
  Grid,
  Typography,
  Alert,
  CircularProgress,
  Divider,
  IconButton,
  Tooltip,
} from '@mui/material';
import { TimePicker } from '@mui/x-date-pickers/TimePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import {
  Save as SaveIcon,
  RestartAlt as ResetIcon,
  ContentCopy as CopyIcon,
} from '@mui/icons-material';
import dayjs, { Dayjs } from 'dayjs';
import settingsService from '../../services/settingsService';
import logger from '../../services/logger';

/** Business hours for a single day */
interface DayHours {
  /** Day of week (0=Sunday, 6=Saturday) */
  dayOfWeek: number;
  /** Whether the business is open on this day */
  isOpen: boolean;
  /** Opening time in HH:mm format */
  openTime: string;
  /** Closing time in HH:mm format */
  closeTime: string;
}

/** Full business hours configuration */
interface BusinessHoursConfig {
  /** Whether business hours enforcement is enabled */
  enabled: boolean;
  /** Timezone for the business hours */
  timezone: string;
  /** Hours for each day */
  days: DayHours[];
}

const DAY_NAMES = [
  'Sunday',
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
];

const DEFAULT_HOURS: BusinessHoursConfig = {
  enabled: false,
  timezone: 'UTC',
  days: [
    { dayOfWeek: 0, isOpen: false, openTime: '09:00', closeTime: '17:00' },
    { dayOfWeek: 1, isOpen: true, openTime: '09:00', closeTime: '17:00' },
    { dayOfWeek: 2, isOpen: true, openTime: '09:00', closeTime: '17:00' },
    { dayOfWeek: 3, isOpen: true, openTime: '09:00', closeTime: '17:00' },
    { dayOfWeek: 4, isOpen: true, openTime: '09:00', closeTime: '17:00' },
    { dayOfWeek: 5, isOpen: true, openTime: '09:00', closeTime: '17:00' },
    { dayOfWeek: 6, isOpen: false, openTime: '09:00', closeTime: '17:00' },
  ],
};

/**
 * BusinessHoursEditor - Admin component for configuring business hours
 *
 * Features:
 * - Enable/disable business hours enforcement
 * - Set open/close times per day
 * - Toggle individual days as open/closed
 * - Copy hours from one day to all weekdays
 */
const BusinessHoursEditor: React.FC = () => {
  const [config, setConfig] = useState<BusinessHoursConfig>(DEFAULT_HOURS);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [hasChanges, setHasChanges] = useState(false);
  const [originalConfig, setOriginalConfig] =
    useState<BusinessHoursConfig>(DEFAULT_HOURS);

  // Fetch business hours from API
  const fetchBusinessHours = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await settingsService.getBusinessHours();
      if (response) {
        setConfig(response);
        setOriginalConfig(response);
      }
    } catch (err) {
      logger.error('Failed to fetch business hours:', err);
      setError('Failed to load business hours configuration.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchBusinessHours();
  }, [fetchBusinessHours]);

  // Check if config has changed
  useEffect(() => {
    setHasChanges(JSON.stringify(config) !== JSON.stringify(originalConfig));
  }, [config, originalConfig]);

  // Handle save
  const handleSave = async () => {
    setSaving(true);
    setError(null);
    setSuccess(false);
    try {
      await settingsService.updateBusinessHours(config);
      setOriginalConfig(config);
      setSuccess(true);
      setHasChanges(false);
      logger.info('Business hours saved successfully');
    } catch (err) {
      logger.error('Failed to save business hours:', err);
      setError('Failed to save business hours configuration.');
    } finally {
      setSaving(false);
    }
  };

  // Handle reset
  const handleReset = () => {
    setConfig(originalConfig);
    setHasChanges(false);
    setError(null);
    setSuccess(false);
  };

  // Update a day's hours
  const updateDayHours = (
    dayOfWeek: number,
    field: keyof DayHours,
    value: string | boolean
  ) => {
    setConfig((prev) => ({
      ...prev,
      days: prev.days.map((day) =>
        day.dayOfWeek === dayOfWeek ? { ...day, [field]: value } : day
      ),
    }));
  };

  // Copy hours from a day to all weekdays (Mon-Fri)
  const copyToWeekdays = (sourceDayOfWeek: number) => {
    const sourceDay = config.days.find((d) => d.dayOfWeek === sourceDayOfWeek);
    if (!sourceDay) return;

    setConfig((prev) => ({
      ...prev,
      days: prev.days.map((day) => {
        // Copy to weekdays only (Monday=1 to Friday=5)
        if (day.dayOfWeek >= 1 && day.dayOfWeek <= 5) {
          return {
            ...day,
            isOpen: sourceDay.isOpen,
            openTime: sourceDay.openTime,
            closeTime: sourceDay.closeTime,
          };
        }
        return day;
      }),
    }));
  };

  // Parse time string to Dayjs
  const parseTime = (timeStr: string): Dayjs => {
    const [hours, minutes] = timeStr.split(':').map(Number);
    return dayjs().hour(hours).minute(minutes);
  };

  // Format Dayjs to time string
  const formatTime = (time: Dayjs | null): string => {
    if (!time) return '09:00';
    return time.format('HH:mm');
  };

  if (loading) {
    return (
      <Box
        display="flex"
        justifyContent="center"
        alignItems="center"
        minHeight={200}
      >
        <CircularProgress />
      </Box>
    );
  }

  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
      <Card>
        <CardHeader
          title="Business Hours"
          subheader="Configure when your business is open"
          action={
            <Box sx={{ display: 'flex', gap: 1, mr: 1 }}>
              <Button
                variant="outlined"
                startIcon={<ResetIcon />}
                onClick={handleReset}
                disabled={saving || !hasChanges}
              >
                Reset
              </Button>
              <Button
                variant="contained"
                startIcon={saving ? <CircularProgress size={20} /> : <SaveIcon />}
                onClick={handleSave}
                disabled={saving || !hasChanges}
              >
                Save
              </Button>
            </Box>
          }
        />
        <CardContent>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
              {error}
            </Alert>
          )}
          {success && (
            <Alert
              severity="success"
              sx={{ mb: 2 }}
              onClose={() => setSuccess(false)}
            >
              Business hours saved successfully!
            </Alert>
          )}

          {/* Enable/Disable */}
          <FormControlLabel
            control={
              <Switch
                checked={config.enabled}
                onChange={(e) =>
                  setConfig((prev) => ({ ...prev, enabled: e.target.checked }))
                }
              />
            }
            label="Enable business hours enforcement"
          />
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2, ml: 4 }}>
            When enabled, certain features may be restricted outside business hours.
          </Typography>

          <Divider sx={{ my: 2 }} />

          {/* Day-by-day configuration */}
          <Grid container spacing={2}>
            {config.days.map((day) => (
              <Grid item xs={12} key={day.dayOfWeek}>
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 2,
                    p: 1.5,
                    borderRadius: 1,
                    bgcolor: day.isOpen ? 'background.paper' : 'action.hover',
                    opacity: config.enabled ? 1 : 0.6,
                  }}
                >
                  {/* Day name */}
                  <Typography
                    sx={{ width: 100, fontWeight: day.isOpen ? 600 : 400 }}
                  >
                    {DAY_NAMES[day.dayOfWeek]}
                  </Typography>

                  {/* Open toggle */}
                  <FormControlLabel
                    control={
                      <Switch
                        checked={day.isOpen}
                        onChange={(e) =>
                          updateDayHours(day.dayOfWeek, 'isOpen', e.target.checked)
                        }
                        disabled={!config.enabled}
                        size="small"
                      />
                    }
                    label={day.isOpen ? 'Open' : 'Closed'}
                    sx={{ minWidth: 100 }}
                  />

                  {/* Time pickers */}
                  {day.isOpen && (
                    <>
                      <TimePicker
                        label="Open"
                        value={parseTime(day.openTime)}
                        onChange={(newValue) =>
                          updateDayHours(
                            day.dayOfWeek,
                            'openTime',
                            formatTime(newValue)
                          )
                        }
                        disabled={!config.enabled}
                        slotProps={{
                          textField: { size: 'small', sx: { width: 130 } },
                        }}
                      />
                      <Typography>to</Typography>
                      <TimePicker
                        label="Close"
                        value={parseTime(day.closeTime)}
                        onChange={(newValue) =>
                          updateDayHours(
                            day.dayOfWeek,
                            'closeTime',
                            formatTime(newValue)
                          )
                        }
                        disabled={!config.enabled}
                        slotProps={{
                          textField: { size: 'small', sx: { width: 130 } },
                        }}
                      />

                      {/* Copy to weekdays button */}
                      <Tooltip title="Copy to all weekdays (Mon-Fri)">
                        <IconButton
                          size="small"
                          onClick={() => copyToWeekdays(day.dayOfWeek)}
                          disabled={!config.enabled}
                        >
                          <CopyIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </>
                  )}
                </Box>
              </Grid>
            ))}
          </Grid>
        </CardContent>
      </Card>
    </LocalizationProvider>
  );
};

export default BusinessHoursEditor;

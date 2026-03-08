import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Paper,
  Typography,
  Grid,
  TextField,
  Button,
  Switch,
  FormControlLabel,
  Divider,
  CircularProgress,
  Alert,
  Select,
  MenuItem,
  InputLabel,
  FormControl,
  Snackbar,
} from '@mui/material';
import {
  AccessTime as ClockIcon,
  Save as SaveIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import AdminPageHeader from '../../components/admin/AdminPageHeader';
import apiClient from '../../services/apiClient';

interface BusinessHoursConfig {
  id?: number;
  dayOfWeek: number;
  dayName: string;
  isWorkingDay: boolean;
  startTime: string;
  endTime: string;
  timezone: string;
}

const DAYS_OF_WEEK = [
  'Sunday',
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
];

const DEFAULT_CONFIG: BusinessHoursConfig[] = DAYS_OF_WEEK.map((day, index) => ({
  dayOfWeek: index,
  dayName: day,
  isWorkingDay: index >= 1 && index <= 5,
  startTime: '09:00',
  endTime: '17:00',
  timezone: 'UTC',
}));

const BusinessHoursConfigPage: React.FC = () => {
  const [config, setConfig] = useState<BusinessHoursConfig[]>(DEFAULT_CONFIG);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [timezone, setTimezone] = useState('UTC');

  const commonTimezones = [
    'UTC',
    'America/New_York',
    'America/Chicago',
    'America/Denver',
    'America/Los_Angeles',
    'Europe/London',
    'Europe/Berlin',
    'Europe/Paris',
    'Asia/Tokyo',
    'Asia/Shanghai',
    'Asia/Kolkata',
    'Australia/Sydney',
    'Pacific/Auckland',
  ];

  const fetchConfig = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await apiClient.get('/businesshours');
      if (response.data && Array.isArray(response.data) && response.data.length > 0) {
        setConfig(response.data);
        if (response.data[0]?.timezone) {
          setTimezone(response.data[0].timezone);
        }
      }
    } catch {
      // Config may not exist yet — use defaults
      setConfig(DEFAULT_CONFIG);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchConfig();
  }, [fetchConfig]);

  const handleDayChange = (dayIndex: number, field: keyof BusinessHoursConfig, value: unknown) => {
    setConfig((prev) =>
      prev.map((day) =>
        day.dayOfWeek === dayIndex ? { ...day, [field]: value } : day
      )
    );
  };

  const handleSave = async () => {
    setSaving(true);
    setError(null);
    try {
      const payload = config.map((day) => ({
        ...day,
        timezone,
      }));
      await apiClient.put('/businesshours', payload);
      setSuccessMessage('Business hours configuration saved successfully');
    } catch (err: unknown) {
      const message = err instanceof Error ? (err as Error).message : 'Failed to save configuration';
      setError(message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="Business Hours Configuration"
        subtitle="Configure working days, hours, and timezone for SLA calculations and scheduling"
        icon={ClockIcon}
      />

      <Paper sx={{ p: 3, mb: 2 }}>
        {error && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
            {error}
          </Alert>
        )}

        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <>
            <Box sx={{ mb: 3 }}>
              <FormControl sx={{ minWidth: 250 }}>
                <InputLabel>Timezone</InputLabel>
                <Select
                  value={timezone}
                  label="Timezone"
                  onChange={(e) => setTimezone(e.target.value)}
                  size="small"
                >
                  {commonTimezones.map((tz) => (
                    <MenuItem key={tz} value={tz}>
                      {tz}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Box>

            <Divider sx={{ mb: 2 }} />

            <Grid container spacing={2}>
              {config.map((day) => (
                <Grid item xs={12} key={day.dayOfWeek}>
                  <Box
                    sx={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: 2,
                      py: 1,
                      px: 2,
                      borderRadius: 1,
                      backgroundColor: day.isWorkingDay
                        ? 'action.hover'
                        : 'transparent',
                    }}
                  >
                    <FormControlLabel
                      control={
                        <Switch
                          checked={day.isWorkingDay}
                          onChange={(e) =>
                            handleDayChange(day.dayOfWeek, 'isWorkingDay', e.target.checked)
                          }
                          color="primary"
                        />
                      }
                      label={
                        <Typography
                          sx={{
                            minWidth: 100,
                            fontWeight: day.isWorkingDay ? 600 : 400,
                            color: day.isWorkingDay
                              ? 'text.primary'
                              : 'text.disabled',
                          }}
                        >
                          {day.dayName}
                        </Typography>
                      }
                      sx={{ mr: 2, minWidth: 180 }}
                    />
                    <TextField
                      label="Start Time"
                      type="time"
                      size="small"
                      value={day.startTime}
                      onChange={(e) =>
                        handleDayChange(day.dayOfWeek, 'startTime', e.target.value)
                      }
                      disabled={!day.isWorkingDay}
                      InputLabelProps={{ shrink: true }}
                      sx={{ width: 150 }}
                    />
                    <Typography color="text.secondary">to</Typography>
                    <TextField
                      label="End Time"
                      type="time"
                      size="small"
                      value={day.endTime}
                      onChange={(e) =>
                        handleDayChange(day.dayOfWeek, 'endTime', e.target.value)
                      }
                      disabled={!day.isWorkingDay}
                      InputLabelProps={{ shrink: true }}
                      sx={{ width: 150 }}
                    />
                  </Box>
                </Grid>
              ))}
            </Grid>

            <Divider sx={{ my: 3 }} />

            <Box sx={{ display: 'flex', gap: 2 }}>
              <Button
                variant="contained"
                startIcon={saving ? <CircularProgress size={20} /> : <SaveIcon />}
                onClick={handleSave}
                disabled={saving}
              >
                {saving ? 'Saving...' : 'Save Configuration'}
              </Button>
              <Button
                variant="outlined"
                startIcon={<RefreshIcon />}
                onClick={fetchConfig}
                disabled={loading}
              >
                Reset
              </Button>
            </Box>
          </>
        )}
      </Paper>

      <Snackbar
        open={!!successMessage}
        autoHideDuration={4000}
        onClose={() => setSuccessMessage(null)}
        message={successMessage}
      />
    </Box>
  );
};

export default BusinessHoursConfigPage;

import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Switch,
  FormControlLabel,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Button,
  Stack,
  Chip,
  Alert,
  Divider,
  Grid,
  TextField,
  Checkbox,
  FormGroup,
} from '@mui/material';
import {
  Email as EmailIcon,
  Schedule as ScheduleIcon,
  Save as SaveIcon,
  Preview as PreviewIcon,
} from '@mui/icons-material';
import logger from '../services/logger';

interface EmailDigestConfig {
  enabled: boolean;
  frequency: 'daily' | 'weekly' | 'monthly';
  dayOfWeek?: number; // 0-6 for weekly
  dayOfMonth?: number; // 1-31 for monthly
  timeOfDay: string; // HH:MM format
  timezone: string;
  sections: {
    newLeads: boolean;
    openOpportunities: boolean;
    recentActivities: boolean;
    upcomingTasks: boolean;
    overdueTasks: boolean;
    teamPerformance: boolean;
    kpiSummary: boolean;
  };
}

const defaultConfig: EmailDigestConfig = {
  enabled: false,
  frequency: 'daily',
  timeOfDay: '08:00',
  timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
  sections: {
    newLeads: true,
    openOpportunities: true,
    recentActivities: true,
    upcomingTasks: true,
    overdueTasks: true,
    teamPerformance: false,
    kpiSummary: false,
  },
};

const EmailDigestPage: React.FC = () => {
  const [config, setConfig] = useState<EmailDigestConfig>(defaultConfig);
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    loadConfig();
  }, []);

  const loadConfig = async () => {
    try {
      // PRA-019: TODO — needs backend endpoint before this can be wired.
      // Proposed: GET /api/users/me/email-digest  → returns EmailDigestConfig
      //           PUT /api/users/me/email-digest  → saves EmailDigestConfig
      // Currently no email-digest endpoint exists on UsersController.
      // Replace these TODOs once the endpoint is added:
      // const data = await userService.getEmailDigestConfig();
      // setConfig(data);
      logger.info('Loading email digest configuration');
    } catch (error) {
      logger.error('Failed to load email digest config:', error);
    }
  };

  const handleSave = async () => {
    try {
      setSaving(true);
      setSuccess(false);
      
      // PRA-019: TODO — wire to PUT /api/users/me/email-digest once endpoint is implemented.
      // await userService.updateEmailDigestConfig(config);
      
      logger.info('Email digest configuration saved', config);
      setSuccess(true);
      setTimeout(() => setSuccess(false), 3000);
    } catch (error) {
      logger.error('Failed to save email digest config:', error);
    } finally {
      setSaving(false);
    }
  };

  const handlePreview = () => {
    logger.info('Sending preview email digest');
    // TODO: Implement preview email
  };

  const getDayName = (day: number) => {
    const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    return days[day];
  };

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ mb: 3 }}>
        <Typography variant="h4" fontWeight={700} gutterBottom>
          Email Digest Settings
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Configure personalized email digests with your most important CRM updates
        </Typography>
      </Box>

      {success && (
        <Alert severity="success" sx={{ mb: 3 }} onClose={() => setSuccess(false)}>
          Email digest preferences saved successfully
        </Alert>
      )}

      <Grid container spacing={3}>
        {/* Main Configuration */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Stack spacing={3}>
                <Box>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={config.enabled}
                        onChange={(e) => setConfig({ ...config, enabled: e.target.checked })}
                      />
                    }
                    label={
                      <Box>
                        <Typography variant="body1" fontWeight={500}>
                          Enable Email Digest
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          Receive automatic email summaries of your CRM activity
                        </Typography>
                      </Box>
                    }
                  />
                </Box>

                <Divider />

                <FormControl fullWidth disabled={!config.enabled}>
                  <InputLabel>Frequency</InputLabel>
                  <Select
                    value={config.frequency}
                    label="Frequency"
                    onChange={(e) =>
                      setConfig({
                        ...config,
                        frequency: e.target.value as 'daily' | 'weekly' | 'monthly',
                      })
                    }
                  >
                    <MenuItem value="daily">Daily</MenuItem>
                    <MenuItem value="weekly">Weekly</MenuItem>
                    <MenuItem value="monthly">Monthly</MenuItem>
                  </Select>
                </FormControl>

                {config.frequency === 'weekly' && (
                  <FormControl fullWidth disabled={!config.enabled}>
                    <InputLabel>Day of Week</InputLabel>
                    <Select
                      value={config.dayOfWeek ?? 1}
                      label="Day of Week"
                      onChange={(e) =>
                        setConfig({ ...config, dayOfWeek: Number(e.target.value) })
                      }
                    >
                      {[0, 1, 2, 3, 4, 5, 6].map((day) => (
                        <MenuItem key={day} value={day}>
                          {getDayName(day)}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                )}

                {config.frequency === 'monthly' && (
                  <TextField
                    fullWidth
                    type="number"
                    label="Day of Month"
                    value={config.dayOfMonth ?? 1}
                    onChange={(e) =>
                      setConfig({ ...config, dayOfMonth: Number(e.target.value) })
                    }
                    inputProps={{ min: 1, max: 31 }}
                    disabled={!config.enabled}
                  />
                )}

                <TextField
                  fullWidth
                  type="time"
                  label="Time of Day"
                  value={config.timeOfDay}
                  onChange={(e) => setConfig({ ...config, timeOfDay: e.target.value })}
                  disabled={!config.enabled}
                  InputLabelProps={{ shrink: true }}
                />

                <TextField
                  fullWidth
                  label="Timezone"
                  value={config.timezone}
                  onChange={(e) => setConfig({ ...config, timezone: e.target.value })}
                  disabled={!config.enabled}
                  helperText="Detected from your browser, adjust if needed"
                />

                <Box sx={{ display: 'flex', gap: 2 }}>
                  <Button
                    variant="contained"
                    startIcon={<SaveIcon />}
                    onClick={handleSave}
                    disabled={!config.enabled || saving}
                    fullWidth
                  >
                    {saving ? 'Saving...' : 'Save Settings'}
                  </Button>
                  <Button
                    variant="outlined"
                    startIcon={<PreviewIcon />}
                    onClick={handlePreview}
                    disabled={!config.enabled}
                    fullWidth
                  >
                    Send Preview
                  </Button>
                </Box>
              </Stack>
            </CardContent>
          </Card>
        </Grid>

        {/* Content Sections */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom fontWeight={600}>
                Email Content
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Choose which sections to include in your digest
              </Typography>

              <FormGroup>
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={config.sections.newLeads}
                      onChange={(e) =>
                        setConfig({
                          ...config,
                          sections: { ...config.sections, newLeads: e.target.checked },
                        })
                      }
                      disabled={!config.enabled}
                    />
                  }
                  label={
                    <Box>
                      <Typography variant="body2">New Leads</Typography>
                      <Typography variant="caption" color="text.secondary">
                        Summary of leads created in the period
                      </Typography>
                    </Box>
                  }
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={config.sections.openOpportunities}
                      onChange={(e) =>
                        setConfig({
                          ...config,
                          sections: { ...config.sections, openOpportunities: e.target.checked },
                        })
                      }
                      disabled={!config.enabled}
                    />
                  }
                  label={
                    <Box>
                      <Typography variant="body2">Open Opportunities</Typography>
                      <Typography variant="caption" color="text.secondary">
                        Your active deals and pipeline status
                      </Typography>
                    </Box>
                  }
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={config.sections.recentActivities}
                      onChange={(e) =>
                        setConfig({
                          ...config,
                          sections: { ...config.sections, recentActivities: e.target.checked },
                        })
                      }
                      disabled={!config.enabled}
                    />
                  }
                  label={
                    <Box>
                      <Typography variant="body2">Recent Activities</Typography>
                      <Typography variant="caption" color="text.secondary">
                        Calls, meetings, and interactions
                      </Typography>
                    </Box>
                  }
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={config.sections.upcomingTasks}
                      onChange={(e) =>
                        setConfig({
                          ...config,
                          sections: { ...config.sections, upcomingTasks: e.target.checked },
                        })
                      }
                      disabled={!config.enabled}
                    />
                  }
                  label={
                    <Box>
                      <Typography variant="body2">Upcoming Tasks</Typography>
                      <Typography variant="caption" color="text.secondary">
                        Tasks due in the next 7 days
                      </Typography>
                    </Box>
                  }
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={config.sections.overdueTasks}
                      onChange={(e) =>
                        setConfig({
                          ...config,
                          sections: { ...config.sections, overdueTasks: e.target.checked },
                        })
                      }
                      disabled={!config.enabled}
                    />
                  }
                  label={
                    <Box>
                      <Typography variant="body2">Overdue Tasks</Typography>
                      <Typography variant="caption" color="text.secondary">
                        Tasks that need immediate attention
                      </Typography>
                    </Box>
                  }
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={config.sections.teamPerformance}
                      onChange={(e) =>
                        setConfig({
                          ...config,
                          sections: { ...config.sections, teamPerformance: e.target.checked },
                        })
                      }
                      disabled={!config.enabled}
                    />
                  }
                  label={
                    <Box>
                      <Typography variant="body2">Team Performance</Typography>
                      <Typography variant="caption" color="text.secondary">
                        Team metrics and leaderboard (managers only)
                      </Typography>
                    </Box>
                  }
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={config.sections.kpiSummary}
                      onChange={(e) =>
                        setConfig({
                          ...config,
                          sections: { ...config.sections, kpiSummary: e.target.checked },
                        })
                      }
                      disabled={!config.enabled}
                    />
                  }
                  label={
                    <Box>
                      <Typography variant="body2">KPI Summary</Typography>
                      <Typography variant="caption" color="text.secondary">
                        Key performance indicators at a glance
                      </Typography>
                    </Box>
                  }
                />
              </FormGroup>
            </CardContent>
          </Card>

          {/* Preview Card */}
          <Card sx={{ mt: 2, bgcolor: 'grey.50' }}>
            <CardContent>
              <Typography variant="subtitle2" gutterBottom fontWeight={600}>
                Email Schedule Summary
              </Typography>
              {config.enabled ? (
                <Stack spacing={1}>
                  <Chip
                    icon={<ScheduleIcon />}
                    label={`${config.frequency.charAt(0).toUpperCase() + config.frequency.slice(1)} at ${config.timeOfDay}`}
                    color="primary"
                    size="small"
                  />
                  {config.frequency === 'weekly' && config.dayOfWeek !== undefined && (
                    <Typography variant="caption">
                      Every {getDayName(config.dayOfWeek)}
                    </Typography>
                  )}
                  {config.frequency === 'monthly' && config.dayOfMonth && (
                    <Typography variant="caption">On day {config.dayOfMonth} of each month</Typography>
                  )}
                  <Typography variant="caption" color="text.secondary">
                    {Object.values(config.sections).filter(Boolean).length} sections enabled
                  </Typography>
                </Stack>
              ) : (
                <Typography variant="body2" color="text.secondary">
                  Email digest is currently disabled
                </Typography>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default EmailDigestPage;

import React, { useState, useCallback, useEffect } from 'react';
import {
  Box, Card, CardContent, Grid, TextField, Switch, FormControlLabel,
  Typography, Button, Divider, Stack, Slider, CircularProgress,
  Snackbar, Alert, Accordion, AccordionSummary, AccordionDetails,
} from '@mui/material';
import {
  SupportAgent as ServiceDeskIcon,
  Save as SaveIcon,
  ExpandMore,
} from '@mui/icons-material';
import AdminPageHeader from '../../components/admin/AdminPageHeader';
import apiClient from '../../services/apiClient';

// ── Types ────────────────────────────────────────────────────────────────────

interface ServiceDeskConfig {
  // SLA Settings
  defaultResponseTimeHours: number;
  defaultResolutionTimeHours: number;
  slaBreachNotification: boolean;
  slaWarningThresholdPercent: number;
  businessHoursOnly: boolean;
  businessStartHour: number;
  businessEndHour: number;

  // Ticket Routing
  autoAssignEnabled: boolean;
  roundRobinAssignment: boolean;
  skillBasedRouting: boolean;
  loadBalancingEnabled: boolean;
  maxTicketsPerAgent: number;

  // Escalation Defaults
  autoEscalateEnabled: boolean;
  escalationTimeoutHours: number;
  escalationNotifyManager: boolean;
  maxEscalationLevel: number;

  // Auto-Assignment
  autoCloseResolvedDays: number;
  requireCloseConfirmation: boolean;
  customerSatisfactionSurvey: boolean;
  reopenOnCustomerReply: boolean;
  defaultPriority: string;
  defaultCategory: string;
}

const defaultConfig: ServiceDeskConfig = {
  defaultResponseTimeHours: 4,
  defaultResolutionTimeHours: 24,
  slaBreachNotification: true,
  slaWarningThresholdPercent: 80,
  businessHoursOnly: true,
  businessStartHour: 9,
  businessEndHour: 17,

  autoAssignEnabled: true,
  roundRobinAssignment: true,
  skillBasedRouting: false,
  loadBalancingEnabled: false,
  maxTicketsPerAgent: 20,

  autoEscalateEnabled: true,
  escalationTimeoutHours: 8,
  escalationNotifyManager: true,
  maxEscalationLevel: 3,

  autoCloseResolvedDays: 7,
  requireCloseConfirmation: true,
  customerSatisfactionSurvey: true,
  reopenOnCustomerReply: true,
  defaultPriority: 'Medium',
  defaultCategory: 'General',
};

// ── Page ─────────────────────────────────────────────────────────────────────

const ServiceDeskConfigPage: React.FC = () => {
  const [config, setConfig] = useState<ServiceDeskConfig>(defaultConfig);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [snack, setSnack] = useState<{ open: boolean; msg: string; severity: 'success' | 'error' }>({
    open: false, msg: '', severity: 'success',
  });

  const loadConfig = useCallback(async () => {
    setLoading(true);
    try {
      const { data } = await apiClient.get('/api/admin/config/service-desk');
      setConfig({ ...defaultConfig, ...data });
    } catch {
      // Use defaults if endpoint not available yet
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadConfig(); }, [loadConfig]);

  const update = (field: keyof ServiceDeskConfig, value: any) => {
    setConfig(prev => ({ ...prev, [field]: value }));
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      await apiClient.put('/api/admin/config/service-desk', config);
      setSnack({ open: true, msg: 'Service desk configuration saved successfully', severity: 'success' });
    } catch {
      setSnack({ open: true, msg: 'Failed to save service desk configuration', severity: 'error' });
    } finally {
      setSaving(false);
    }
  };

  const handleCancel = () => {
    loadConfig();
  };

  if (loading) {
    return (
      <Box sx={{ p: 3 }}>
        <AdminPageHeader title="Service Desk Configuration" subtitle="SLA, routing, escalation, and ticket settings" icon={ServiceDeskIcon} />
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      <AdminPageHeader title="Service Desk Configuration" subtitle="SLA defaults, ticket routing, escalation, and auto-assignment settings" icon={ServiceDeskIcon} />

      {/* SLA Settings */}
      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>SLA Settings</Typography>
          <Divider sx={{ mb: 2 }} />
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Default Response Time (Hours)"
                type="number"
                value={config.defaultResponseTimeHours}
                onChange={e => update('defaultResponseTimeHours', Number(e.target.value))}
                fullWidth size="small"
                helperText="Maximum time to first response"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Default Resolution Time (Hours)"
                type="number"
                value={config.defaultResolutionTimeHours}
                onChange={e => update('defaultResolutionTimeHours', Number(e.target.value))}
                fullWidth size="small"
                helperText="Maximum time to resolve ticket"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={config.slaBreachNotification} onChange={e => update('slaBreachNotification', e.target.checked)} />}
                label="Notify on SLA Breach"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Typography gutterBottom>SLA Warning at: {config.slaWarningThresholdPercent}% of time elapsed</Typography>
              <Slider
                value={config.slaWarningThresholdPercent}
                onChange={(_, v) => update('slaWarningThresholdPercent', v as number)}
                min={50} max={95} step={5}
                valueLabelDisplay="auto"
                valueLabelFormat={v => `${v}%`}
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <FormControlLabel
                control={<Switch checked={config.businessHoursOnly} onChange={e => update('businessHoursOnly', e.target.checked)} />}
                label="Business Hours Only"
              />
            </Grid>
            {config.businessHoursOnly && (
              <>
                <Grid item xs={12} sm={4}>
                  <TextField
                    label="Business Start Hour"
                    type="number"
                    value={config.businessStartHour}
                    onChange={e => update('businessStartHour', Number(e.target.value))}
                    fullWidth size="small"
                    inputProps={{ min: 0, max: 23 }}
                  />
                </Grid>
                <Grid item xs={12} sm={4}>
                  <TextField
                    label="Business End Hour"
                    type="number"
                    value={config.businessEndHour}
                    onChange={e => update('businessEndHour', Number(e.target.value))}
                    fullWidth size="small"
                    inputProps={{ min: 0, max: 23 }}
                  />
                </Grid>
              </>
            )}
          </Grid>
        </CardContent>
      </Card>

      {/* Ticket Routing */}
      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>Ticket Routing</Typography>
          <Divider sx={{ mb: 2 }} />
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={config.autoAssignEnabled} onChange={e => update('autoAssignEnabled', e.target.checked)} />}
                label="Auto-Assign Tickets"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={config.roundRobinAssignment} onChange={e => update('roundRobinAssignment', e.target.checked)} />}
                label="Round-Robin Assignment"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={config.skillBasedRouting} onChange={e => update('skillBasedRouting', e.target.checked)} />}
                label="Skill-Based Routing"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={config.loadBalancingEnabled} onChange={e => update('loadBalancingEnabled', e.target.checked)} />}
                label="Load Balancing"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Max Tickets per Agent"
                type="number"
                value={config.maxTicketsPerAgent}
                onChange={e => update('maxTicketsPerAgent', Number(e.target.value))}
                fullWidth size="small"
                helperText="Maximum concurrent tickets assigned to one agent"
              />
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Escalation Defaults */}
      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>Escalation Defaults</Typography>
          <Divider sx={{ mb: 2 }} />
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={config.autoEscalateEnabled} onChange={e => update('autoEscalateEnabled', e.target.checked)} />}
                label="Auto-Escalate on Timeout"
              />
            </Grid>
            {config.autoEscalateEnabled && (
              <>
                <Grid item xs={12} sm={6}>
                  <TextField
                    label="Escalation Timeout (Hours)"
                    type="number"
                    value={config.escalationTimeoutHours}
                    onChange={e => update('escalationTimeoutHours', Number(e.target.value))}
                    fullWidth size="small"
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <FormControlLabel
                    control={<Switch checked={config.escalationNotifyManager} onChange={e => update('escalationNotifyManager', e.target.checked)} />}
                    label="Notify Manager on Escalation"
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    label="Max Escalation Level"
                    type="number"
                    value={config.maxEscalationLevel}
                    onChange={e => update('maxEscalationLevel', Number(e.target.value))}
                    fullWidth size="small"
                    inputProps={{ min: 1, max: 5 }}
                  />
                </Grid>
              </>
            )}
          </Grid>
        </CardContent>
      </Card>

      {/* Auto-Assignment & Closure */}
      <Accordion defaultExpanded>
        <AccordionSummary expandIcon={<ExpandMore />}>
          <Typography variant="h6">Auto-Assignment &amp; Closure</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Auto-Close Resolved Tickets After (Days)"
                type="number"
                value={config.autoCloseResolvedDays}
                onChange={e => update('autoCloseResolvedDays', Number(e.target.value))}
                fullWidth size="small"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={config.requireCloseConfirmation} onChange={e => update('requireCloseConfirmation', e.target.checked)} />}
                label="Require Customer Close Confirmation"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={config.customerSatisfactionSurvey} onChange={e => update('customerSatisfactionSurvey', e.target.checked)} />}
                label="Send Satisfaction Survey on Close"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={config.reopenOnCustomerReply} onChange={e => update('reopenOnCustomerReply', e.target.checked)} />}
                label="Reopen on Customer Reply"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Default Priority"
                value={config.defaultPriority}
                onChange={e => update('defaultPriority', e.target.value)}
                fullWidth size="small"
                select
                SelectProps={{ native: true }}
              >
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
                <option value="Critical">Critical</option>
              </TextField>
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Default Category"
                value={config.defaultCategory}
                onChange={e => update('defaultCategory', e.target.value)}
                fullWidth size="small"
              />
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* Action Buttons */}
      <Stack direction="row" spacing={2} sx={{ mt: 3 }}>
        <Button
          variant="contained"
          startIcon={saving ? <CircularProgress size={18} /> : <SaveIcon />}
          onClick={handleSave}
          disabled={saving}
        >
          {saving ? 'Saving...' : 'Save Configuration'}
        </Button>
        <Button variant="outlined" onClick={handleCancel} disabled={saving}>
          Cancel
        </Button>
      </Stack>

      <Snackbar open={snack.open} autoHideDuration={4000} onClose={() => setSnack(s => ({ ...s, open: false }))}>
        <Alert severity={snack.severity} onClose={() => setSnack(s => ({ ...s, open: false }))}>{snack.msg}</Alert>
      </Snackbar>
    </Box>
  );
};

export default ServiceDeskConfigPage;

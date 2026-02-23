import React, { useState, useCallback, useEffect } from 'react';
import {
  Box, Card, CardContent, Grid, TextField, Switch, FormControlLabel,
  Typography, Button, Divider, Stack, Slider, CircularProgress,
  Snackbar, Alert, Accordion, AccordionSummary, AccordionDetails,
} from '@mui/material';
import {
  PointOfSale as SalesIcon,
  Save as SaveIcon,
  ExpandMore,
} from '@mui/icons-material';
import AdminPageHeader from '../../components/admin/AdminPageHeader';
import apiClient from '../../services/apiClient';

// ── Types ────────────────────────────────────────────────────────────────────

interface SalesConfig {
  // Commission Settings
  defaultCommissionRate: number;
  tieredCommissionsEnabled: boolean;
  commissionPayoutFrequency: string;
  commissionCapEnabled: boolean;
  commissionCapAmount: number;

  // Discount Settings
  maxDiscountPercent: number;
  discountApprovalRequired: boolean;
  discountApprovalThreshold: number;
  volumeDiscountsEnabled: boolean;

  // Quote Defaults
  quoteValidityDays: number;
  quoteAutoNumberPrefix: string;
  quoteRequiresApproval: boolean;
  quoteApprovalThreshold: number;
  defaultPaymentTerms: string;
  defaultCurrency: string;

  // Pipeline Settings
  autoAdvancePipeline: boolean;
  requireStageNotes: boolean;
  staleDealDays: number;
  probabilityByStage: boolean;
  defaultWinProbability: number;
}

const defaultConfig: SalesConfig = {
  defaultCommissionRate: 10,
  tieredCommissionsEnabled: false,
  commissionPayoutFrequency: 'Monthly',
  commissionCapEnabled: false,
  commissionCapAmount: 50000,

  maxDiscountPercent: 25,
  discountApprovalRequired: true,
  discountApprovalThreshold: 15,
  volumeDiscountsEnabled: false,

  quoteValidityDays: 30,
  quoteAutoNumberPrefix: 'QT-',
  quoteRequiresApproval: false,
  quoteApprovalThreshold: 10000,
  defaultPaymentTerms: 'Net 30',
  defaultCurrency: 'USD',

  autoAdvancePipeline: false,
  requireStageNotes: true,
  staleDealDays: 30,
  probabilityByStage: true,
  defaultWinProbability: 50,
};

// ── Page ─────────────────────────────────────────────────────────────────────

const SalesConfigPage: React.FC = () => {
  const [config, setConfig] = useState<SalesConfig>(defaultConfig);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [snack, setSnack] = useState<{ open: boolean; msg: string; severity: 'success' | 'error' }>({
    open: false, msg: '', severity: 'success',
  });

  const loadConfig = useCallback(async () => {
    setLoading(true);
    try {
      const { data } = await apiClient.get('/api/admin/config/sales');
      setConfig({ ...defaultConfig, ...data });
    } catch {
      // Use defaults if endpoint not available yet
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadConfig(); }, [loadConfig]);

  const update = (field: keyof SalesConfig, value: any) => {
    setConfig(prev => ({ ...prev, [field]: value }));
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      await apiClient.put('/api/admin/config/sales', config);
      setSnack({ open: true, msg: 'Sales configuration saved successfully', severity: 'success' });
    } catch {
      setSnack({ open: true, msg: 'Failed to save sales configuration', severity: 'error' });
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
        <AdminPageHeader title="Sales Configuration" subtitle="Commission, discount, quote, and pipeline settings" icon={SalesIcon} />
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      <AdminPageHeader title="Sales Configuration" subtitle="Commission rates, discounts, quotes, and pipeline settings" icon={SalesIcon} />

      {/* Commission Settings */}
      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>Commission Settings</Typography>
          <Divider sx={{ mb: 2 }} />
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Typography gutterBottom>Default Commission Rate: {config.defaultCommissionRate}%</Typography>
              <Slider
                value={config.defaultCommissionRate}
                onChange={(_, v) => update('defaultCommissionRate', v as number)}
                min={0} max={50} step={0.5}
                valueLabelDisplay="auto"
                valueLabelFormat={v => `${v}%`}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Payout Frequency"
                value={config.commissionPayoutFrequency}
                onChange={e => update('commissionPayoutFrequency', e.target.value)}
                fullWidth size="small"
                select
                SelectProps={{ native: true }}
              >
                <option value="Weekly">Weekly</option>
                <option value="Bi-Weekly">Bi-Weekly</option>
                <option value="Monthly">Monthly</option>
                <option value="Quarterly">Quarterly</option>
              </TextField>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={config.tieredCommissionsEnabled} onChange={e => update('tieredCommissionsEnabled', e.target.checked)} />}
                label="Enable Tiered Commissions"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={config.commissionCapEnabled} onChange={e => update('commissionCapEnabled', e.target.checked)} />}
                label="Enable Commission Cap"
              />
            </Grid>
            {config.commissionCapEnabled && (
              <Grid item xs={12} sm={6}>
                <TextField
                  label="Commission Cap Amount"
                  type="number"
                  value={config.commissionCapAmount}
                  onChange={e => update('commissionCapAmount', Number(e.target.value))}
                  fullWidth size="small"
                  InputProps={{ startAdornment: <Typography sx={{ mr: 1 }}>$</Typography> }}
                />
              </Grid>
            )}
          </Grid>
        </CardContent>
      </Card>

      {/* Discount Settings */}
      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>Discount Settings</Typography>
          <Divider sx={{ mb: 2 }} />
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Typography gutterBottom>Maximum Discount: {config.maxDiscountPercent}%</Typography>
              <Slider
                value={config.maxDiscountPercent}
                onChange={(_, v) => update('maxDiscountPercent', v as number)}
                min={0} max={100} step={1}
                valueLabelDisplay="auto"
                valueLabelFormat={v => `${v}%`}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={config.volumeDiscountsEnabled} onChange={e => update('volumeDiscountsEnabled', e.target.checked)} />}
                label="Enable Volume Discounts"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={config.discountApprovalRequired} onChange={e => update('discountApprovalRequired', e.target.checked)} />}
                label="Require Approval for Discounts"
              />
            </Grid>
            {config.discountApprovalRequired && (
              <Grid item xs={12} sm={6}>
                <Typography gutterBottom>Approval Threshold: {config.discountApprovalThreshold}%</Typography>
                <Slider
                  value={config.discountApprovalThreshold}
                  onChange={(_, v) => update('discountApprovalThreshold', v as number)}
                  min={1} max={100} step={1}
                  valueLabelDisplay="auto"
                  valueLabelFormat={v => `${v}%`}
                />
              </Grid>
            )}
          </Grid>
        </CardContent>
      </Card>

      {/* Quote Defaults */}
      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>Quote Defaults</Typography>
          <Divider sx={{ mb: 2 }} />
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Quote Validity (Days)"
                type="number"
                value={config.quoteValidityDays}
                onChange={e => update('quoteValidityDays', Number(e.target.value))}
                fullWidth size="small"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Auto-Number Prefix"
                value={config.quoteAutoNumberPrefix}
                onChange={e => update('quoteAutoNumberPrefix', e.target.value)}
                fullWidth size="small"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Default Payment Terms"
                value={config.defaultPaymentTerms}
                onChange={e => update('defaultPaymentTerms', e.target.value)}
                fullWidth size="small"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Default Currency"
                value={config.defaultCurrency}
                onChange={e => update('defaultCurrency', e.target.value)}
                fullWidth size="small"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={config.quoteRequiresApproval} onChange={e => update('quoteRequiresApproval', e.target.checked)} />}
                label="Require Approval for Quotes"
              />
            </Grid>
            {config.quoteRequiresApproval && (
              <Grid item xs={12} sm={6}>
                <TextField
                  label="Approval Threshold Amount"
                  type="number"
                  value={config.quoteApprovalThreshold}
                  onChange={e => update('quoteApprovalThreshold', Number(e.target.value))}
                  fullWidth size="small"
                  InputProps={{ startAdornment: <Typography sx={{ mr: 1 }}>$</Typography> }}
                />
              </Grid>
            )}
          </Grid>
        </CardContent>
      </Card>

      {/* Pipeline Settings */}
      <Accordion defaultExpanded>
        <AccordionSummary expandIcon={<ExpandMore />}>
          <Typography variant="h6">Pipeline Settings</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={config.autoAdvancePipeline} onChange={e => update('autoAdvancePipeline', e.target.checked)} />}
                label="Auto-Advance Pipeline Stages"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={config.requireStageNotes} onChange={e => update('requireStageNotes', e.target.checked)} />}
                label="Require Notes on Stage Change"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Stale Deal Threshold (Days)"
                type="number"
                value={config.staleDealDays}
                onChange={e => update('staleDealDays', Number(e.target.value))}
                fullWidth size="small"
                helperText="Deals with no activity beyond this are flagged stale"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={config.probabilityByStage} onChange={e => update('probabilityByStage', e.target.checked)} />}
                label="Auto-Set Probability by Stage"
              />
            </Grid>
            {!config.probabilityByStage && (
              <Grid item xs={12} sm={6}>
                <Typography gutterBottom>Default Win Probability: {config.defaultWinProbability}%</Typography>
                <Slider
                  value={config.defaultWinProbability}
                  onChange={(_, v) => update('defaultWinProbability', v as number)}
                  min={0} max={100} step={5}
                  valueLabelDisplay="auto"
                  valueLabelFormat={v => `${v}%`}
                />
              </Grid>
            )}
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

export default SalesConfigPage;

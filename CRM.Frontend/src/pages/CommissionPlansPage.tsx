/**
 * Commission Plans Page
 * TODO-SALES007-004-EXT: Commission plan management for sales managers
 * 
 * This page allows managers to create, edit, and manage commission plans
 * including tiered rates, caps, triggers, and split configurations.
 */
import { useState, useEffect, useMemo } from 'react';
import { useAuth } from '../contexts/AuthContext';
import {
  Box, Card, CardContent, Typography, Button, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Dialog, DialogTitle, DialogContent, DialogActions, Alert, CircularProgress,
  TextField, Container, FormControl, InputLabel, Select, MenuItem, Chip, Grid,
  IconButton, Tooltip, Paper, Divider, Switch, FormControlLabel, Accordion, AccordionSummary,
  AccordionDetails, List, ListItem, ListItemText, ListItemSecondaryAction
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, 
  ExpandMore as ExpandMoreIcon, ContentCopy as CopyIcon,
  ViewList as TiersIcon, Archive as ArchiveIcon, CheckCircle as ActivateIcon,
  Settings as SettingsIcon, TrendingUp as PerformanceIcon
} from '@mui/icons-material';
import commissionService, {
  CommissionPlan, CommissionTier, CommissionPlanStatus, CommissionType, CommissionTrigger,
  CommissionPlanCreateRequest, CommissionPlanUpdateRequest, CommissionTierCreateRequest
} from '../services/commissionService';
import logger from '../services/logger';
import { EnhancedEmptyState } from '../components/common';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs, { Dayjs } from 'dayjs';

// ============================================================================
// Helper Functions
// ============================================================================

const getPlanStatusLabel = (status: CommissionPlanStatus): string => {
  const labels: Record<CommissionPlanStatus, string> = {
    [CommissionPlanStatus.Draft]: 'Draft',
    [CommissionPlanStatus.Active]: 'Active',
    [CommissionPlanStatus.Inactive]: 'Inactive',
    [CommissionPlanStatus.Archived]: 'Archived',
  };
  return labels[status] || 'Unknown';
};

const getPlanStatusColor = (status: CommissionPlanStatus): 'default' | 'warning' | 'success' | 'error' | 'info' | 'primary' | 'secondary' => {
  const colors: Record<CommissionPlanStatus, 'default' | 'warning' | 'success' | 'error' | 'info' | 'primary' | 'secondary'> = {
    [CommissionPlanStatus.Draft]: 'default',
    [CommissionPlanStatus.Active]: 'success',
    [CommissionPlanStatus.Inactive]: 'warning',
    [CommissionPlanStatus.Archived]: 'secondary',
  };
  return colors[status] || 'default';
};

const getCommissionTypeLabel = (type: CommissionType): string => {
  const labels: Record<CommissionType, string> = {
    [CommissionType.FlatPercentage]: 'Flat Percentage',
    [CommissionType.TieredPercentage]: 'Tiered Percentage',
    [CommissionType.FixedAmount]: 'Fixed Amount',
    [CommissionType.TieredAmount]: 'Tiered Amount',
    [CommissionType.MarginBased]: 'Margin Based',
    [CommissionType.Custom]: 'Custom',
  };
  return labels[type] || 'Unknown';
};

const getTriggerLabel = (trigger: CommissionTrigger): string => {
  const labels: Record<CommissionTrigger, string> = {
    [CommissionTrigger.OnClose]: 'On Deal Close',
    [CommissionTrigger.OnOrder]: 'On Order Created',
    [CommissionTrigger.OnInvoice]: 'On Invoice Issued',
    [CommissionTrigger.OnPayment]: 'On Payment Received',
    [CommissionTrigger.OnSubscriptionStart]: 'On Subscription Start',
    [CommissionTrigger.OnSignature]: 'On Contract Signed',
    [CommissionTrigger.Monthly]: 'Monthly',
  };
  return labels[trigger] || 'Unknown';
};

const formatCurrency = (amount: number): string => {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);
};

const formatPercent = (value: number): string => {
  return `${value}%`;
};

// ============================================================================
// Types
// ============================================================================

interface PlanForm {
  name: string;
  code: string;
  description: string;
  commissionType: CommissionType;
  baseRate: number;
  trigger: CommissionTrigger;
  clawbackPeriodDays: number;
  minDealSize: number;
  maxCommissionPerDeal: number;
  maxCommissionPerPeriod: number;
  allowSplits: boolean;
  defaultOverlayPercent: number;
  effectiveStartDate: Dayjs | null;
  effectiveEndDate: Dayjs | null;
  fiscalYear: number;
}

interface TierForm {
  name: string;
  tierOrder: number;
  minValue: number;
  maxValue: number;
  minAttainmentPercent: number;
  maxAttainmentPercent: number;
  commissionRate: number;
  fixedAmount: number;
  multiplier: number;
}

const initialPlanForm: PlanForm = {
  name: '',
  code: '',
  description: '',
  commissionType: CommissionType.FlatPercentage,
  baseRate: 0,
  trigger: CommissionTrigger.OnClose,
  clawbackPeriodDays: 90,
  minDealSize: 0,
  maxCommissionPerDeal: 0,
  maxCommissionPerPeriod: 0,
  allowSplits: true,
  defaultOverlayPercent: 0,
  effectiveStartDate: null,
  effectiveEndDate: null,
  fiscalYear: new Date().getFullYear(),
};

const initialTierForm: TierForm = {
  name: '',
  tierOrder: 1,
  minValue: 0,
  maxValue: 0,
  minAttainmentPercent: 0,
  maxAttainmentPercent: 100,
  commissionRate: 0,
  fixedAmount: 0,
  multiplier: 1,
};

// ============================================================================
// Main Component
// ============================================================================

export default function CommissionPlansPage() {
  const { user } = useAuth();
  
  // Plans state
  const [plans, setPlans] = useState<CommissionPlan[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  
  // Plan dialog state
  const [openPlanDialog, setOpenPlanDialog] = useState(false);
  const [editingPlanId, setEditingPlanId] = useState<number | null>(null);
  const [planForm, setPlanForm] = useState<PlanForm>(initialPlanForm);
  const [saving, setSaving] = useState(false);
  
  // Tiers dialog state
  const [selectedPlan, setSelectedPlan] = useState<CommissionPlan | null>(null);
  const [tiers, setTiers] = useState<CommissionTier[]>([]);
  const [openTiersDialog, setOpenTiersDialog] = useState(false);
  const [loadingTiers, setLoadingTiers] = useState(false);
  
  // Tier edit dialog
  const [openTierDialog, setOpenTierDialog] = useState(false);
  const [editingTierId, setEditingTierId] = useState<number | null>(null);
  const [tierForm, setTierForm] = useState<TierForm>(initialTierForm);
  
  // Filter
  const [statusFilter, setStatusFilter] = useState<CommissionPlanStatus | ''>('');

  // ============================================================================
  // Data Loading
  // ============================================================================

  const loadPlans = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await commissionService.getPlans();
      setPlans(data);
    } catch (err) {
      logger.error('Failed to load commission plans', err);
      setError('Failed to load commission plans');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadPlans();
  }, []);

  const loadTiers = async (planId: number) => {
    try {
      setLoadingTiers(true);
      const data = await commissionService.getTiers(planId);
      setTiers(data);
    } catch (err) {
      logger.error('Failed to load tiers', err);
      setTiers([]);
    } finally {
      setLoadingTiers(false);
    }
  };

  // ============================================================================
  // Filtered Data
  // ============================================================================

  const filteredPlans = useMemo(() => {
    let filtered = [...plans];
    
    if (statusFilter !== '') {
      filtered = filtered.filter(p => p.status === statusFilter);
    }
    
    return filtered.sort((a, b) => a.name.localeCompare(b.name));
  }, [plans, statusFilter]);

  // ============================================================================
  // Plan CRUD
  // ============================================================================

  const handleOpenPlanDialog = (plan?: CommissionPlan) => {
    if (plan) {
      setEditingPlanId(plan.id);
      setPlanForm({
        name: plan.name,
        code: plan.code || '',
        description: plan.description || '',
        commissionType: plan.commissionType,
        baseRate: plan.baseRate,
        trigger: plan.trigger,
        clawbackPeriodDays: plan.clawbackPeriodDays || 90,
        minDealSize: plan.minDealSize || 0,
        maxCommissionPerDeal: plan.maxCommissionPerDeal || 0,
        maxCommissionPerPeriod: plan.maxCommissionPerPeriod || 0,
        allowSplits: plan.allowSplits ?? true,
        defaultOverlayPercent: plan.defaultOverlayPercent || 0,
        effectiveStartDate: plan.effectiveStartDate ? dayjs(plan.effectiveStartDate) : null,
        effectiveEndDate: plan.effectiveEndDate ? dayjs(plan.effectiveEndDate) : null,
        fiscalYear: plan.fiscalYear || new Date().getFullYear(),
      });
    } else {
      setEditingPlanId(null);
      setPlanForm(initialPlanForm);
    }
    setOpenPlanDialog(true);
  };

  const handleSavePlan = async () => {
    if (!planForm.name) {
      setError('Plan name is required');
      return;
    }

    try {
      setSaving(true);
      
      if (editingPlanId) {
        const request: CommissionPlanUpdateRequest = {
          name: planForm.name,
          code: planForm.code || undefined,
          description: planForm.description || undefined,
          commissionType: planForm.commissionType,
          baseRate: planForm.baseRate,
          trigger: planForm.trigger,
          clawbackPeriodDays: planForm.clawbackPeriodDays,
          minDealSize: planForm.minDealSize,
          maxCommissionPerDeal: planForm.maxCommissionPerDeal,
          maxCommissionPerPeriod: planForm.maxCommissionPerPeriod,
          allowSplits: planForm.allowSplits,
          defaultOverlayPercent: planForm.defaultOverlayPercent,
          effectiveStartDate: planForm.effectiveStartDate?.toISOString(),
          effectiveEndDate: planForm.effectiveEndDate?.toISOString(),
          fiscalYear: planForm.fiscalYear,
        };
        await commissionService.updatePlan(editingPlanId, request);
        setSuccessMessage('Plan updated successfully');
      } else {
        const request: CommissionPlanCreateRequest = {
          name: planForm.name,
          code: planForm.code || undefined,
          description: planForm.description || undefined,
          commissionType: planForm.commissionType,
          baseRate: planForm.baseRate,
          trigger: planForm.trigger,
          clawbackPeriodDays: planForm.clawbackPeriodDays,
          minDealSize: planForm.minDealSize,
          maxCommissionPerDeal: planForm.maxCommissionPerDeal,
          maxCommissionPerPeriod: planForm.maxCommissionPerPeriod,
          allowSplits: planForm.allowSplits,
          defaultOverlayPercent: planForm.defaultOverlayPercent,
          effectiveStartDate: planForm.effectiveStartDate?.toISOString(),
          effectiveEndDate: planForm.effectiveEndDate?.toISOString(),
          fiscalYear: planForm.fiscalYear,
        };
        await commissionService.createPlan(request);
        setSuccessMessage('Plan created successfully');
      }
      
      setOpenPlanDialog(false);
      loadPlans();
    } catch (err) {
      logger.error('Failed to save plan', err);
      setError('Failed to save plan');
    } finally {
      setSaving(false);
    }
  };

  const handleDeletePlan = async (planId: number) => {
    if (!window.confirm('Are you sure you want to delete this plan?')) return;
    
    try {
      await commissionService.deletePlan(planId);
      setSuccessMessage('Plan deleted successfully');
      loadPlans();
    } catch (err) {
      logger.error('Failed to delete plan', err);
      setError('Failed to delete plan');
    }
  };

  const handleClonePlan = async (plan: CommissionPlan) => {
    try {
      const newPlan = await commissionService.clonePlan(plan.id);
      setSuccessMessage(`Plan cloned as "${newPlan.name}"`);
      loadPlans();
    } catch (err) {
      logger.error('Failed to clone plan', err);
      setError('Failed to clone plan');
    }
  };

  const handleActivatePlan = async (planId: number) => {
    try {
      await commissionService.updatePlanStatus(planId, CommissionPlanStatus.Active);
      setSuccessMessage('Plan activated');
      loadPlans();
    } catch (err) {
      logger.error('Failed to activate plan', err);
      setError('Failed to activate plan');
    }
  };

  const handleArchivePlan = async (planId: number) => {
    try {
      await commissionService.updatePlanStatus(planId, CommissionPlanStatus.Archived);
      setSuccessMessage('Plan archived');
      loadPlans();
    } catch (err) {
      logger.error('Failed to archive plan', err);
      setError('Failed to archive plan');
    }
  };

  // ============================================================================
  // Tiers Management
  // ============================================================================

  const handleOpenTiersDialog = async (plan: CommissionPlan) => {
    setSelectedPlan(plan);
    setOpenTiersDialog(true);
    await loadTiers(plan.id);
  };

  const handleOpenTierDialog = (tier?: CommissionTier) => {
    if (tier) {
      setEditingTierId(tier.id);
      setTierForm({
        name: tier.name || '',
        tierOrder: tier.tierOrder,
        minValue: tier.minValue ?? 0,
        maxValue: tier.maxValue || 0,
        minAttainmentPercent: tier.minAttainmentPercent || 0,
        maxAttainmentPercent: tier.maxAttainmentPercent || 100,
        commissionRate: tier.commissionRate ?? 0,
        fixedAmount: tier.fixedAmount || 0,
        multiplier: tier.multiplier || 1,
      });
    } else {
      setEditingTierId(null);
      setTierForm({
        ...initialTierForm,
        tierOrder: tiers.length + 1,
      });
    }
    setOpenTierDialog(true);
  };

  const handleSaveTier = async () => {
    if (!selectedPlan) return;
    if (!tierForm.name) {
      setError('Tier name is required');
      return;
    }

    try {
      setSaving(true);
      
      const request: CommissionTierCreateRequest = {
        commissionPlanId: selectedPlan.id,
        name: tierForm.name,
        tierOrder: tierForm.tierOrder,
        minValue: tierForm.minValue,
        maxValue: tierForm.maxValue || undefined,
        minAttainmentPercent: tierForm.minAttainmentPercent,
        maxAttainmentPercent: tierForm.maxAttainmentPercent,
        commissionRate: tierForm.commissionRate,
        fixedAmount: tierForm.fixedAmount || undefined,
        multiplier: tierForm.multiplier || undefined,
      };

      if (editingTierId) {
        await commissionService.updateTier(editingTierId, request);
        setSuccessMessage('Tier updated');
      } else {
        await commissionService.createTier(selectedPlan.id, request);
        setSuccessMessage('Tier created');
      }
      
      setOpenTierDialog(false);
      await loadTiers(selectedPlan.id);
    } catch (err) {
      logger.error('Failed to save tier', err);
      setError('Failed to save tier');
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteTier = async (tierId: number) => {
    if (!selectedPlan) return;
    if (!window.confirm('Are you sure you want to delete this tier?')) return;
    
    try {
      await commissionService.deleteTier(selectedPlan.id, tierId);
      setSuccessMessage('Tier deleted');
      await loadTiers(selectedPlan.id);
    } catch (err) {
      logger.error('Failed to delete tier', err);
      setError('Failed to delete tier');
    }
  };

  // ============================================================================
  // Render
  // ============================================================================

  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
      <Container maxWidth="xl" sx={{ mt: 4, mb: 4 }}>
        {/* Header */}
        <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
          <Typography variant="h4" component="h1">
            Commission Plans
          </Typography>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleOpenPlanDialog()}
          >
            Create Plan
          </Button>
        </Box>

        {/* Alerts */}
        {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccessMessage(null)}>{successMessage}</Alert>}

        {/* Filters */}
        <Paper sx={{ p: 2, mb: 3 }}>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} sm={4} md={3}>
              <FormControl fullWidth size="small">
                <InputLabel>Status Filter</InputLabel>
                <Select
                  value={statusFilter}
                  label="Status Filter"
                  onChange={(e) => setStatusFilter(e.target.value as CommissionPlanStatus | '')}
                >
                  <MenuItem value="">All Statuses</MenuItem>
                  <MenuItem value={CommissionPlanStatus.Draft}>Draft</MenuItem>
                  <MenuItem value={CommissionPlanStatus.Active}>Active</MenuItem>
                  <MenuItem value={CommissionPlanStatus.Inactive}>Inactive</MenuItem>
                  <MenuItem value={CommissionPlanStatus.Archived}>Archived</MenuItem>
                </Select>
              </FormControl>
            </Grid>
          </Grid>
        </Paper>

        {/* Plans Table */}
        {loading ? (
          <Box display="flex" justifyContent="center" p={4}><CircularProgress /></Box>
        ) : filteredPlans.length === 0 ? (
          <EnhancedEmptyState
            illustration="generic"
            title="No Commission Plans"
            description="Create your first commission plan to start managing sales incentives."
            primaryActionLabel="Create Plan"
            onPrimaryAction={() => handleOpenPlanDialog()}
          />
        ) : (
          <TableContainer component={Paper}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Code</TableCell>
                  <TableCell>Type</TableCell>
                  <TableCell align="right">Base Rate</TableCell>
                  <TableCell>Trigger</TableCell>
                  <TableCell align="center">Status</TableCell>
                  <TableCell>Effective Period</TableCell>
                  <TableCell align="center">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredPlans.map((plan) => (
                  <TableRow key={plan.id} hover>
                    <TableCell>
                      <Typography variant="body2" fontWeight="medium">{plan.name}</Typography>
                      {plan.description && (
                        <Typography variant="caption" color="textSecondary">{plan.description}</Typography>
                      )}
                    </TableCell>
                    <TableCell>{plan.code || '-'}</TableCell>
                    <TableCell>{getCommissionTypeLabel(plan.commissionType)}</TableCell>
                    <TableCell align="right">
                      {plan.commissionType === CommissionType.FixedAmount || plan.commissionType === CommissionType.TieredAmount
                        ? formatCurrency(plan.baseRate)
                        : formatPercent(plan.baseRate)}
                    </TableCell>
                    <TableCell>{getTriggerLabel(plan.trigger)}</TableCell>
                    <TableCell align="center">
                      <Chip
                        label={getPlanStatusLabel(plan.status)}
                        color={getPlanStatusColor(plan.status)}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>
                      {plan.effectiveStartDate || plan.effectiveEndDate ? (
                        <>
                          {plan.effectiveStartDate ? new Date(plan.effectiveStartDate).toLocaleDateString() : '...'}
                          {' - '}
                          {plan.effectiveEndDate ? new Date(plan.effectiveEndDate).toLocaleDateString() : '...'}
                        </>
                      ) : 'Always'}
                    </TableCell>
                    <TableCell align="center">
                      <Tooltip title="Edit">
                        <IconButton size="small" onClick={() => handleOpenPlanDialog(plan)}>
                          <EditIcon />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Manage Tiers">
                        <IconButton size="small" onClick={() => handleOpenTiersDialog(plan)}>
                          <TiersIcon />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Clone">
                        <IconButton size="small" onClick={() => handleClonePlan(plan)}>
                          <CopyIcon />
                        </IconButton>
                      </Tooltip>
                      {plan.status === CommissionPlanStatus.Draft && (
                        <Tooltip title="Activate">
                          <IconButton size="small" color="success" onClick={() => handleActivatePlan(plan.id)}>
                            <ActivateIcon />
                          </IconButton>
                        </Tooltip>
                      )}
                      {plan.status !== CommissionPlanStatus.Archived && (
                        <Tooltip title="Archive">
                          <IconButton size="small" onClick={() => handleArchivePlan(plan.id)}>
                            <ArchiveIcon />
                          </IconButton>
                        </Tooltip>
                      )}
                      <Tooltip title="Delete">
                        <IconButton size="small" color="error" onClick={() => handleDeletePlan(plan.id)}>
                          <DeleteIcon />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}

        {/* Plan Dialog */}
        <Dialog open={openPlanDialog} onClose={() => setOpenPlanDialog(false)} maxWidth="md" fullWidth>
          <DialogTitle>{editingPlanId ? 'Edit Commission Plan' : 'Create Commission Plan'}</DialogTitle>
          <DialogContent>
            <Grid container spacing={2} sx={{ mt: 1 }}>
              <Grid item xs={12} sm={8}>
                <TextField
                  fullWidth
                  label="Plan Name"
                  value={planForm.name}
                  onChange={(e) => setPlanForm({ ...planForm, name: e.target.value })}
                  required
                />
              </Grid>
              <Grid item xs={12} sm={4}>
                <TextField
                  fullWidth
                  label="Code"
                  value={planForm.code}
                  onChange={(e) => setPlanForm({ ...planForm, code: e.target.value })}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Description"
                  multiline
                  rows={2}
                  value={planForm.description}
                  onChange={(e) => setPlanForm({ ...planForm, description: e.target.value })}
                />
              </Grid>
              
              <Grid item xs={12}><Divider sx={{ my: 1 }} /></Grid>
              
              <Grid item xs={12} sm={6}>
                <FormControl fullWidth>
                  <InputLabel>Commission Type</InputLabel>
                  <Select
                    value={planForm.commissionType}
                    label="Commission Type"
                    onChange={(e) => setPlanForm({ ...planForm, commissionType: e.target.value as CommissionType })}
                  >
                    <MenuItem value={CommissionType.FlatPercentage}>Flat Percentage</MenuItem>
                    <MenuItem value={CommissionType.TieredPercentage}>Tiered Percentage</MenuItem>
                    <MenuItem value={CommissionType.FixedAmount}>Fixed Amount</MenuItem>
                    <MenuItem value={CommissionType.TieredAmount}>Tiered Amount</MenuItem>
                    <MenuItem value={CommissionType.MarginBased}>Margin Based</MenuItem>
                    <MenuItem value={CommissionType.Custom}>Custom</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label={planForm.commissionType === CommissionType.FixedAmount ? 'Base Amount' : 'Base Rate (%)'}
                  type="number"
                  value={planForm.baseRate}
                  onChange={(e) => setPlanForm({ ...planForm, baseRate: parseFloat(e.target.value) || 0 })}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <FormControl fullWidth>
                  <InputLabel>Trigger Event</InputLabel>
                  <Select
                    value={planForm.trigger}
                    label="Trigger Event"
                    onChange={(e) => setPlanForm({ ...planForm, trigger: e.target.value as CommissionTrigger })}
                  >
                    <MenuItem value={CommissionTrigger.OnClose}>On Deal Close</MenuItem>
                    <MenuItem value={CommissionTrigger.OnOrder}>On Order Created</MenuItem>
                    <MenuItem value={CommissionTrigger.OnInvoice}>On Invoice Issued</MenuItem>
                    <MenuItem value={CommissionTrigger.OnPayment}>On Payment Received</MenuItem>
                    <MenuItem value={CommissionTrigger.OnSubscriptionStart}>On Subscription Start</MenuItem>
                    <MenuItem value={CommissionTrigger.OnSignature}>On Contract Signed</MenuItem>
                    <MenuItem value={CommissionTrigger.Monthly}>Monthly</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Clawback Period (days)"
                  type="number"
                  value={planForm.clawbackPeriodDays}
                  onChange={(e) => setPlanForm({ ...planForm, clawbackPeriodDays: parseInt(e.target.value) || 0 })}
                />
              </Grid>
              
              <Grid item xs={12}><Divider sx={{ my: 1 }} /></Grid>
              
              {/* Caps */}
              <Grid item xs={12}>
                <Typography variant="subtitle2" gutterBottom>Commission Caps</Typography>
              </Grid>
              <Grid item xs={12} sm={4}>
                <TextField
                  fullWidth
                  label="Min Deal Size"
                  type="number"
                  value={planForm.minDealSize}
                  onChange={(e) => setPlanForm({ ...planForm, minDealSize: parseFloat(e.target.value) || 0 })}
                />
              </Grid>
              <Grid item xs={12} sm={4}>
                <TextField
                  fullWidth
                  label="Max Commission Per Deal"
                  type="number"
                  value={planForm.maxCommissionPerDeal}
                  onChange={(e) => setPlanForm({ ...planForm, maxCommissionPerDeal: parseFloat(e.target.value) || 0 })}
                  helperText="0 = no cap"
                />
              </Grid>
              <Grid item xs={12} sm={4}>
                <TextField
                  fullWidth
                  label="Max Commission Per Period"
                  type="number"
                  value={planForm.maxCommissionPerPeriod}
                  onChange={(e) => setPlanForm({ ...planForm, maxCommissionPerPeriod: parseFloat(e.target.value) || 0 })}
                  helperText="0 = no cap"
                />
              </Grid>
              
              <Grid item xs={12}><Divider sx={{ my: 1 }} /></Grid>
              
              {/* Splits & Overlays */}
              <Grid item xs={12}>
                <Typography variant="subtitle2" gutterBottom>Splits & Overlays</Typography>
              </Grid>
              <Grid item xs={12} sm={6}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={planForm.allowSplits}
                      onChange={(e) => setPlanForm({ ...planForm, allowSplits: e.target.checked })}
                    />
                  }
                  label="Allow Commission Splits"
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Default Overlay Percent"
                  type="number"
                  value={planForm.defaultOverlayPercent}
                  onChange={(e) => setPlanForm({ ...planForm, defaultOverlayPercent: parseFloat(e.target.value) || 0 })}
                />
              </Grid>
              
              <Grid item xs={12}><Divider sx={{ my: 1 }} /></Grid>
              
              {/* Effective Dates */}
              <Grid item xs={12}>
                <Typography variant="subtitle2" gutterBottom>Effective Period</Typography>
              </Grid>
              <Grid item xs={12} sm={4}>
                <DatePicker
                  label="Start Date"
                  value={planForm.effectiveStartDate}
                  onChange={(date) => setPlanForm({ ...planForm, effectiveStartDate: date })}
                  slotProps={{ textField: { fullWidth: true } }}
                />
              </Grid>
              <Grid item xs={12} sm={4}>
                <DatePicker
                  label="End Date"
                  value={planForm.effectiveEndDate}
                  onChange={(date) => setPlanForm({ ...planForm, effectiveEndDate: date })}
                  slotProps={{ textField: { fullWidth: true } }}
                />
              </Grid>
              <Grid item xs={12} sm={4}>
                <TextField
                  fullWidth
                  label="Fiscal Year"
                  type="number"
                  value={planForm.fiscalYear}
                  onChange={(e) => setPlanForm({ ...planForm, fiscalYear: parseInt(e.target.value) || new Date().getFullYear() })}
                />
              </Grid>
            </Grid>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenPlanDialog(false)} disabled={saving}>Cancel</Button>
            <Button
              variant="contained"
              onClick={handleSavePlan}
              disabled={saving}
            >
              {saving ? <CircularProgress size={24} /> : (editingPlanId ? 'Update' : 'Create')}
            </Button>
          </DialogActions>
        </Dialog>

        {/* Tiers Management Dialog */}
        <Dialog open={openTiersDialog} onClose={() => setOpenTiersDialog(false)} maxWidth="md" fullWidth>
          <DialogTitle>
            Manage Tiers - {selectedPlan?.name}
            <Typography variant="body2" color="textSecondary">
              Configure commission tiers for tiered rate calculations
            </Typography>
          </DialogTitle>
          <DialogContent>
            {loadingTiers ? (
              <Box display="flex" justifyContent="center" p={4}><CircularProgress /></Box>
            ) : tiers.length === 0 ? (
              <Box textAlign="center" py={4}>
                <Typography color="textSecondary" gutterBottom>No tiers configured</Typography>
                <Button variant="outlined" startIcon={<AddIcon />} onClick={() => handleOpenTierDialog()}>
                  Add First Tier
                </Button>
              </Box>
            ) : (
              <>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Order</TableCell>
                      <TableCell>Name</TableCell>
                      <TableCell align="right">Min Value</TableCell>
                      <TableCell align="right">Max Value</TableCell>
                      <TableCell align="right">Rate</TableCell>
                      <TableCell align="right">Fixed Amount</TableCell>
                      <TableCell align="center">Actions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {tiers.sort((a, b) => a.tierOrder - b.tierOrder).map((tier) => (
                      <TableRow key={tier.id}>
                        <TableCell>{tier.tierOrder}</TableCell>
                        <TableCell>{tier.name}</TableCell>
                        <TableCell align="right">{formatCurrency(tier.minValue ?? 0)}</TableCell>
                        <TableCell align="right">{tier.maxValue ? formatCurrency(tier.maxValue) : 'Unlimited'}</TableCell>
                        <TableCell align="right">{formatPercent(tier.commissionRate ?? 0)}</TableCell>
                        <TableCell align="right">{tier.fixedAmount ? formatCurrency(tier.fixedAmount) : '-'}</TableCell>
                        <TableCell align="center">
                          <IconButton size="small" onClick={() => handleOpenTierDialog(tier)}>
                            <EditIcon />
                          </IconButton>
                          <IconButton size="small" color="error" onClick={() => handleDeleteTier(tier.id)}>
                            <DeleteIcon />
                          </IconButton>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
                <Box mt={2}>
                  <Button startIcon={<AddIcon />} onClick={() => handleOpenTierDialog()}>
                    Add Tier
                  </Button>
                </Box>
              </>
            )}
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenTiersDialog(false)}>Close</Button>
          </DialogActions>
        </Dialog>

        {/* Tier Edit Dialog */}
        <Dialog open={openTierDialog} onClose={() => setOpenTierDialog(false)} maxWidth="sm" fullWidth>
          <DialogTitle>{editingTierId ? 'Edit Tier' : 'Add Tier'}</DialogTitle>
          <DialogContent>
            <Grid container spacing={2} sx={{ mt: 1 }}>
              <Grid item xs={8}>
                <TextField
                  fullWidth
                  label="Tier Name"
                  value={tierForm.name}
                  onChange={(e) => setTierForm({ ...tierForm, name: e.target.value })}
                  required
                />
              </Grid>
              <Grid item xs={4}>
                <TextField
                  fullWidth
                  label="Order"
                  type="number"
                  value={tierForm.tierOrder}
                  onChange={(e) => setTierForm({ ...tierForm, tierOrder: parseInt(e.target.value) || 1 })}
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  fullWidth
                  label="Min Value"
                  type="number"
                  value={tierForm.minValue}
                  onChange={(e) => setTierForm({ ...tierForm, minValue: parseFloat(e.target.value) || 0 })}
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  fullWidth
                  label="Max Value"
                  type="number"
                  value={tierForm.maxValue}
                  onChange={(e) => setTierForm({ ...tierForm, maxValue: parseFloat(e.target.value) || 0 })}
                  helperText="0 = unlimited"
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  fullWidth
                  label="Commission Rate (%)"
                  type="number"
                  value={tierForm.commissionRate}
                  onChange={(e) => setTierForm({ ...tierForm, commissionRate: parseFloat(e.target.value) || 0 })}
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  fullWidth
                  label="Fixed Amount"
                  type="number"
                  value={tierForm.fixedAmount}
                  onChange={(e) => setTierForm({ ...tierForm, fixedAmount: parseFloat(e.target.value) || 0 })}
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  fullWidth
                  label="Min Attainment %"
                  type="number"
                  value={tierForm.minAttainmentPercent}
                  onChange={(e) => setTierForm({ ...tierForm, minAttainmentPercent: parseFloat(e.target.value) || 0 })}
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  fullWidth
                  label="Max Attainment %"
                  type="number"
                  value={tierForm.maxAttainmentPercent}
                  onChange={(e) => setTierForm({ ...tierForm, maxAttainmentPercent: parseFloat(e.target.value) || 0 })}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Multiplier"
                  type="number"
                  value={tierForm.multiplier}
                  onChange={(e) => setTierForm({ ...tierForm, multiplier: parseFloat(e.target.value) || 1 })}
                  helperText="Applied to base rate/amount"
                />
              </Grid>
            </Grid>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenTierDialog(false)} disabled={saving}>Cancel</Button>
            <Button variant="contained" onClick={handleSaveTier} disabled={saving}>
              {saving ? <CircularProgress size={24} /> : (editingTierId ? 'Update' : 'Add')}
            </Button>
          </DialogActions>
        </Dialog>
      </Container>
    </LocalizationProvider>
  );
}

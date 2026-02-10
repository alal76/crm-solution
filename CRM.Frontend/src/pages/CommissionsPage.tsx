import { useState, useEffect, useMemo } from 'react';
import { useAuth } from '../contexts/AuthContext';
import {
  Box, Card, CardContent, Typography, Button, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Dialog, DialogTitle, DialogContent, DialogActions, Alert, CircularProgress,
  TextField, Container, FormControl, InputLabel, Select, MenuItem, Chip, Grid,
  IconButton, Tooltip, Tabs, Tab, SelectChangeEvent, Divider, Paper, LinearProgress
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, 
  CheckCircle as ApproveIcon, Cancel as RejectIcon, Payment as PaidIcon,
  Undo as ClawbackIcon, Calculate as CalculateIcon, Assessment as StatsIcon,
  EmojiEvents as LeaderboardIcon, TrendingUp as ForecastIcon,
  Description as StatementIcon, Settings as PlanIcon
} from '@mui/icons-material';
import commissionService, { 
  Commission, CommissionPlan, CommissionTier, CommissionStatement,
  CommissionStatus, CommissionPlanStatus, CommissionStatementStatus,
  CommissionType, CommissionTrigger, CommissionStatistics, CommissionLeaderboard,
  CommissionCreateRequest, CommissionUpdateRequest,
  CommissionPlanCreateRequest, CommissionPlanUpdateRequest,
  CommissionTierCreateRequest, CommissionStatementGenerateRequest
} from '../services/commissionService';
import logger from '../services/logger';
import { TabPanel, DialogError, DialogSuccess, ActionButton, EnhancedEmptyState } from '../components/common';
import { useApiState } from '../hooks/useApiState';
import logo from '../assets/logo.png';
import EntitySelect from '../components/EntitySelect';
import AdvancedSearch, { SearchField, SearchFilter, filterData } from '../components/AdvancedSearch';

// ============================================================================
// Helper Functions
// ============================================================================

const getStatusLabel = (status: CommissionStatus): string => {
  const labels: Record<CommissionStatus, string> = {
    [CommissionStatus.Pending]: 'Pending',
    [CommissionStatus.Approved]: 'Approved',
    [CommissionStatus.Held]: 'Held',
    [CommissionStatus.Paid]: 'Paid',
    [CommissionStatus.ClawedBack]: 'Clawed Back',
    [CommissionStatus.Adjusted]: 'Adjusted',
    [CommissionStatus.Cancelled]: 'Cancelled',
  };
  return labels[status] || 'Unknown';
};

const getStatusColor = (status: CommissionStatus): 'default' | 'warning' | 'success' | 'error' | 'info' | 'primary' | 'secondary' => {
  const colors: Record<CommissionStatus, 'default' | 'warning' | 'success' | 'error' | 'info' | 'primary' | 'secondary'> = {
    [CommissionStatus.Pending]: 'warning',
    [CommissionStatus.Approved]: 'info',
    [CommissionStatus.Held]: 'secondary',
    [CommissionStatus.Paid]: 'success',
    [CommissionStatus.ClawedBack]: 'error',
    [CommissionStatus.Adjusted]: 'primary',
    [CommissionStatus.Cancelled]: 'default',
  };
  return colors[status] || 'default';
};

const getPlanStatusLabel = (status: CommissionPlanStatus): string => {
  const labels: Record<CommissionPlanStatus, string> = {
    [CommissionPlanStatus.Draft]: 'Draft',
    [CommissionPlanStatus.Active]: 'Active',
    [CommissionPlanStatus.Inactive]: 'Inactive',
    [CommissionPlanStatus.Archived]: 'Archived',
  };
  return labels[status] || 'Unknown';
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
    [CommissionTrigger.OnClose]: 'On Close',
    [CommissionTrigger.OnOrder]: 'On Order',
    [CommissionTrigger.OnInvoice]: 'On Invoice',
    [CommissionTrigger.OnPayment]: 'On Payment',
    [CommissionTrigger.OnSubscriptionStart]: 'On Subscription Start',
    [CommissionTrigger.OnSignature]: 'On Signature',
    [CommissionTrigger.Monthly]: 'Monthly',
  };
  return labels[trigger] || 'Unknown';
};

// Search fields for Advanced Search
const SEARCH_FIELDS: SearchField[] = [
  { name: 'status', label: 'Status', type: 'select', options: [
    { value: 0, label: 'Pending' },
    { value: 1, label: 'Approved' },
    { value: 2, label: 'Held' },
    { value: 3, label: 'Paid' },
    { value: 4, label: 'Clawed Back' },
    { value: 5, label: 'Adjusted' },
    { value: 6, label: 'Cancelled' },
  ]},
  { name: 'finalCommissionAmount', label: 'Amount', type: 'numberRange' },
  { name: 'commissionRate', label: 'Rate (%)', type: 'numberRange' },
];

const SEARCHABLE_FIELDS = ['tierName', 'notes'];

// ============================================================================
// Form Types
// ============================================================================

interface CommissionForm {
  userId: number | '';
  commissionPlanId: number | '';
  opportunityId: number | '';
  orderId: number | '';
  dealAmount: number;
  commissionableAmount: number;
  commissionRate: number;
  commissionAmount: number;
  splitPercent: number;
  finalCommissionAmount: number;
  currencyCode: string;
  notes: string;
}

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
  effectiveStartDate: string;
  effectiveEndDate: string;
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

// ============================================================================
// Main Component
// ============================================================================

function CommissionsPage() {
  const { user } = useAuth();

  // Main tabs: 0 = Commissions, 1 = Plans, 2 = Statistics, 3 = Leaderboard
  const [mainTab, setMainTab] = useState(0);
  
  // Commissions state
  const [commissions, setCommissions] = useState<Commission[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [searchFilters, setSearchFilters] = useState<SearchFilter[]>([]);
  const [searchText, setSearchText] = useState('');
  
  // Commission dialog
  const [openCommissionDialog, setOpenCommissionDialog] = useState(false);
  const [editingCommissionId, setEditingCommissionId] = useState<number | null>(null);
  const [dialogError, setDialogError] = useState<string | null>(null);
  const dialogApi = useApiState();
  
  // Plans state
  const [plans, setPlans] = useState<CommissionPlan[]>([]);
  const [loadingPlans, setLoadingPlans] = useState(false);
  const [openPlanDialog, setOpenPlanDialog] = useState(false);
  const [editingPlanId, setEditingPlanId] = useState<number | null>(null);
  const [selectedPlanForTiers, setSelectedPlanForTiers] = useState<CommissionPlan | null>(null);
  
  // Tiers state
  const [tiers, setTiers] = useState<CommissionTier[]>([]);
  const [openTierDialog, setOpenTierDialog] = useState(false);
  const [editingTierId, setEditingTierId] = useState<number | null>(null);
  
  // Statistics state
  const [statistics, setStatistics] = useState<CommissionStatistics | null>(null);
  const [leaderboard, setLeaderboard] = useState<CommissionLeaderboard[]>([]);
  
  // Action dialogs
  const [rejectDialogOpen, setRejectDialogOpen] = useState(false);
  const [clawbackDialogOpen, setClawbackDialogOpen] = useState(false);
  const [actionReason, setActionReason] = useState('');
  const [selectedCommission, setSelectedCommission] = useState<Commission | null>(null);
  
  // Statement generation
  const [statementDialogOpen, setStatementDialogOpen] = useState(false);
  const [statementUserId, setStatementUserId] = useState<number | ''>('');
  const [statementFromDate, setStatementFromDate] = useState('');
  const [statementToDate, setStatementToDate] = useState('');
  
  // Filtered commissions
  const filteredCommissions = useMemo(
    () => filterData(commissions, searchFilters, searchText, SEARCHABLE_FIELDS),
    [commissions, searchFilters, searchText]
  );

  // Empty forms
  const emptyCommissionForm: CommissionForm = {
    userId: '', commissionPlanId: '', opportunityId: '', orderId: '',
    dealAmount: 0, commissionableAmount: 0, commissionRate: 0, commissionAmount: 0,
    splitPercent: 100, finalCommissionAmount: 0, currencyCode: 'USD', notes: '',
  };
  
  const emptyPlanForm: PlanForm = {
    name: '', code: '', description: '',
    commissionType: CommissionType.FlatPercentage, baseRate: 0,
    trigger: CommissionTrigger.OnClose, clawbackPeriodDays: 0,
    minDealSize: 0, maxCommissionPerDeal: 0, maxCommissionPerPeriod: 0,
    allowSplits: false, defaultOverlayPercent: 0,
    effectiveStartDate: '', effectiveEndDate: '', fiscalYear: new Date().getFullYear(),
  };
  
  const emptyTierForm: TierForm = {
    name: '', tierOrder: 1, minValue: 0, maxValue: 0,
    minAttainmentPercent: 0, maxAttainmentPercent: 100,
    commissionRate: 0, fixedAmount: 0, multiplier: 1,
  };
  
  const [commissionForm, setCommissionForm] = useState<CommissionForm>(emptyCommissionForm);
  const [planForm, setPlanForm] = useState<PlanForm>(emptyPlanForm);
  const [tierForm, setTierForm] = useState<TierForm>(emptyTierForm);

  // ============================================================================
  // Data Fetching
  // ============================================================================

  useEffect(() => {
    fetchCommissions();
    fetchStatistics();
  }, []);

  useEffect(() => {
    if (mainTab === 1) {
      fetchPlans();
    }
    if (mainTab === 3) {
      fetchLeaderboard();
    }
  }, [mainTab]);

  const fetchCommissions = async () => {
    try {
      setLoading(true);
      const data = await commissionService.getCommissions();
      setCommissions(data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch commissions');
      logger.error('Failed to fetch commissions', err);
    } finally {
      setLoading(false);
    }
  };

  const fetchPlans = async () => {
    try {
      setLoadingPlans(true);
      const data = await commissionService.getPlans();
      setPlans(data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch plans');
      logger.error('Failed to fetch plans', err);
    } finally {
      setLoadingPlans(false);
    }
  };

  const fetchStatistics = async () => {
    try {
      const data = await commissionService.getStatistics();
      setStatistics(data);
    } catch (err) {
      logger.error('Failed to fetch statistics', err);
    }
  };

  const fetchLeaderboard = async () => {
    try {
      const data = await commissionService.getLeaderboard(10);
      setLeaderboard(data);
    } catch (err) {
      logger.error('Failed to fetch leaderboard', err);
    }
  };

  const fetchTiers = async (planId: number) => {
    try {
      const data = await commissionService.getTiers(planId);
      setTiers(data);
    } catch (err) {
      logger.error('Failed to fetch tiers', err);
    }
  };

  // ============================================================================
  // Commission Handlers
  // ============================================================================

  const handleSearch = (filters: SearchFilter[], text: string) => {
    setSearchFilters(filters);
    setSearchText(text);
  };

  const handleOpenCommissionDialog = (commission?: Commission) => {
    if (commission) {
      setEditingCommissionId(commission.id);
      setCommissionForm({
        userId: commission.userId,
        commissionPlanId: commission.commissionPlanId || '',
        opportunityId: commission.opportunityId || '',
        orderId: commission.orderId || '',
        dealAmount: commission.dealAmount,
        commissionableAmount: commission.commissionableAmount,
        commissionRate: commission.commissionRate,
        commissionAmount: commission.commissionAmount,
        splitPercent: commission.splitPercent,
        finalCommissionAmount: commission.finalCommissionAmount,
        currencyCode: commission.currencyCode || 'USD',
        notes: commission.notes || '',
      });
    } else {
      setEditingCommissionId(null);
      setCommissionForm(emptyCommissionForm);
    }
    setOpenCommissionDialog(true);
  };

  const handleCloseCommissionDialog = () => {
    setOpenCommissionDialog(false);
    setEditingCommissionId(null);
    setDialogError(null);
    dialogApi.reset();
  };

  const handleCommissionInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    setCommissionForm(prev => ({
      ...prev,
      [name]: type === 'number' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleCommissionSelectChange = (e: SelectChangeEvent<string | number>) => {
    const { name, value } = e.target;
    setCommissionForm(prev => ({ ...prev, [name]: value }));
  };

  const handleSaveCommission = async () => {
    if (!commissionForm.userId) {
      setDialogError('Please select a user');
      return;
    }
    
    await dialogApi.execute(async () => {
      if (editingCommissionId) {
        const payload: CommissionUpdateRequest = {
          dealAmount: commissionForm.dealAmount,
          commissionableAmount: commissionForm.commissionableAmount,
          commissionRate: commissionForm.commissionRate,
          commissionAmount: commissionForm.commissionAmount,
          splitPercent: commissionForm.splitPercent,
          finalCommissionAmount: commissionForm.finalCommissionAmount,
          notes: commissionForm.notes,
        };
        await commissionService.updateCommission(editingCommissionId, payload);
        setSuccessMessage('Commission updated successfully');
      } else {
        const payload: CommissionCreateRequest = {
          userId: commissionForm.userId as number,
          commissionPlanId: commissionForm.commissionPlanId ? commissionForm.commissionPlanId as number : undefined,
          opportunityId: commissionForm.opportunityId ? commissionForm.opportunityId as number : undefined,
          orderId: commissionForm.orderId ? commissionForm.orderId as number : undefined,
          dealAmount: commissionForm.dealAmount,
          commissionableAmount: commissionForm.commissionableAmount,
          commissionRate: commissionForm.commissionRate,
          commissionAmount: commissionForm.commissionAmount,
          splitPercent: commissionForm.splitPercent,
          finalCommissionAmount: commissionForm.finalCommissionAmount,
          currencyCode: commissionForm.currencyCode,
          notes: commissionForm.notes,
        };
        await commissionService.createCommission(payload);
        setSuccessMessage('Commission created successfully');
      }
      handleCloseCommissionDialog();
      fetchCommissions();
      fetchStatistics();
      setTimeout(() => setSuccessMessage(null), 3000);
    });
  };

  const handleDeleteCommission = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this commission?')) return;
    try {
      await commissionService.deleteCommission(id);
      setSuccessMessage('Commission deleted successfully');
      fetchCommissions();
      fetchStatistics();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete commission');
    }
  };

  // ============================================================================
  // Status Actions
  // ============================================================================

  const handleApprove = async (commission: Commission) => {
    try {
      await commissionService.approveCommission(commission.id, user?.id ?? 0);
      setSuccessMessage('Commission approved');
      fetchCommissions();
      fetchStatistics();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to approve commission');
    }
  };

  const openRejectDialog = (commission: Commission) => {
    setSelectedCommission(commission);
    setActionReason('');
    setRejectDialogOpen(true);
  };

  const handleConfirmReject = async () => {
    if (!selectedCommission || !actionReason) return;
    try {
      await commissionService.rejectCommission(selectedCommission.id, actionReason);
      setSuccessMessage('Commission rejected');
      setRejectDialogOpen(false);
      fetchCommissions();
      fetchStatistics();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to reject commission');
      setRejectDialogOpen(false);
    }
  };

  const handleMarkPaid = async (commission: Commission) => {
    try {
      await commissionService.markCommissionPaid(commission.id);
      setSuccessMessage('Commission marked as paid');
      fetchCommissions();
      fetchStatistics();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to mark commission as paid');
    }
  };

  const openClawbackDialog = (commission: Commission) => {
    setSelectedCommission(commission);
    setActionReason('');
    setClawbackDialogOpen(true);
  };

  const handleConfirmClawback = async () => {
    if (!selectedCommission || !actionReason) return;
    try {
      await commissionService.clawbackCommission(selectedCommission.id, actionReason);
      setSuccessMessage('Commission clawed back');
      setClawbackDialogOpen(false);
      fetchCommissions();
      fetchStatistics();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to claw back commission');
      setClawbackDialogOpen(false);
    }
  };

  const handleRecalculate = async (commission: Commission) => {
    try {
      await commissionService.recalculateCommission(commission.id);
      setSuccessMessage('Commission recalculated');
      fetchCommissions();
      fetchStatistics();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to recalculate commission');
    }
  };

  // ============================================================================
  // Plan Handlers
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
        clawbackPeriodDays: plan.clawbackPeriodDays || 0,
        minDealSize: plan.minDealSize || 0,
        maxCommissionPerDeal: plan.maxCommissionPerDeal || 0,
        maxCommissionPerPeriod: plan.maxCommissionPerPeriod || 0,
        allowSplits: plan.allowSplits,
        defaultOverlayPercent: plan.defaultOverlayPercent || 0,
        effectiveStartDate: plan.effectiveStartDate?.split('T')[0] || '',
        effectiveEndDate: plan.effectiveEndDate?.split('T')[0] || '',
        fiscalYear: plan.fiscalYear || new Date().getFullYear(),
      });
    } else {
      setEditingPlanId(null);
      setPlanForm(emptyPlanForm);
    }
    setOpenPlanDialog(true);
  };

  const handleClosePlanDialog = () => {
    setOpenPlanDialog(false);
    setEditingPlanId(null);
    setDialogError(null);
    dialogApi.reset();
  };

  const handlePlanInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    if (type === 'checkbox') {
      setPlanForm(prev => ({ ...prev, [name]: (e.target as HTMLInputElement).checked }));
    } else {
      setPlanForm(prev => ({
        ...prev,
        [name]: type === 'number' ? parseFloat(value) || 0 : value,
      }));
    }
  };

  const handlePlanSelectChange = (e: SelectChangeEvent<number>) => {
    const { name, value } = e.target;
    setPlanForm(prev => ({ ...prev, [name]: value }));
  };

  const handleSavePlan = async () => {
    if (!planForm.name) {
      setDialogError('Please enter a plan name');
      return;
    }
    
    await dialogApi.execute(async () => {
      if (editingPlanId) {
        const payload: CommissionPlanUpdateRequest = {
          name: planForm.name,
          description: planForm.description,
          commissionType: planForm.commissionType,
          baseRate: planForm.baseRate,
          trigger: planForm.trigger,
          clawbackPeriodDays: planForm.clawbackPeriodDays,
          minDealSize: planForm.minDealSize,
          maxCommissionPerDeal: planForm.maxCommissionPerDeal,
          maxCommissionPerPeriod: planForm.maxCommissionPerPeriod,
          allowSplits: planForm.allowSplits,
          defaultOverlayPercent: planForm.defaultOverlayPercent,
          effectiveStartDate: planForm.effectiveStartDate || undefined,
          effectiveEndDate: planForm.effectiveEndDate || undefined,
          fiscalYear: planForm.fiscalYear,
        };
        await commissionService.updatePlan(editingPlanId, payload);
        setSuccessMessage('Plan updated successfully');
      } else {
        const payload: CommissionPlanCreateRequest = {
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
          effectiveStartDate: planForm.effectiveStartDate || undefined,
          effectiveEndDate: planForm.effectiveEndDate || undefined,
          fiscalYear: planForm.fiscalYear,
        };
        await commissionService.createPlan(payload);
        setSuccessMessage('Plan created successfully');
      }
      handleClosePlanDialog();
      fetchPlans();
      setTimeout(() => setSuccessMessage(null), 3000);
    });
  };

  const handleDeletePlan = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this plan?')) return;
    try {
      await commissionService.deletePlan(id);
      setSuccessMessage('Plan deleted successfully');
      fetchPlans();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete plan');
    }
  };

  const handleViewTiers = (plan: CommissionPlan) => {
    setSelectedPlanForTiers(plan);
    fetchTiers(plan.id);
  };

  // ============================================================================
  // Tier Handlers
  // ============================================================================

  const handleOpenTierDialog = (tier?: CommissionTier) => {
    if (tier) {
      setEditingTierId(tier.id);
      setTierForm({
        name: tier.name || '',
        tierOrder: tier.tierOrder,
        minValue: tier.minValue || 0,
        maxValue: tier.maxValue || 0,
        minAttainmentPercent: tier.minAttainmentPercent || 0,
        maxAttainmentPercent: tier.maxAttainmentPercent || 100,
        commissionRate: tier.commissionRate || 0,
        fixedAmount: tier.fixedAmount || 0,
        multiplier: tier.multiplier,
      });
    } else {
      setEditingTierId(null);
      setTierForm({ ...emptyTierForm, tierOrder: tiers.length + 1 });
    }
    setOpenTierDialog(true);
  };

  const handleCloseTierDialog = () => {
    setOpenTierDialog(false);
    setEditingTierId(null);
    setDialogError(null);
    dialogApi.reset();
  };

  const handleTierInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, type } = e.target;
    setTierForm(prev => ({
      ...prev,
      [name]: type === 'number' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleSaveTier = async () => {
    if (!selectedPlanForTiers) return;
    
    await dialogApi.execute(async () => {
      if (editingTierId) {
        await commissionService.updateTier(editingTierId, {
          planId: selectedPlanForTiers.id,
          ...tierForm,
        });
        setSuccessMessage('Tier updated successfully');
      } else {
        await commissionService.addTier(selectedPlanForTiers.id, tierForm);
        setSuccessMessage('Tier added successfully');
      }
      handleCloseTierDialog();
      fetchTiers(selectedPlanForTiers.id);
      setTimeout(() => setSuccessMessage(null), 3000);
    });
  };

  const handleDeleteTier = async (tierId: number) => {
    if (!window.confirm('Are you sure you want to delete this tier?')) return;
    try {
      await commissionService.removeTier(tierId);
      setSuccessMessage('Tier deleted successfully');
      if (selectedPlanForTiers) {
        fetchTiers(selectedPlanForTiers.id);
      }
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete tier');
    }
  };

  // ============================================================================
  // Statement Handlers
  // ============================================================================

  const handleGenerateStatement = async () => {
    if (!statementUserId || !statementFromDate || !statementToDate) {
      setDialogError('Please fill in all fields');
      return;
    }
    
    try {
      const request: CommissionStatementGenerateRequest = {
        userId: statementUserId as number,
        fromDate: statementFromDate,
        toDate: statementToDate,
      };
      await commissionService.generateStatement(request);
      setSuccessMessage('Statement generated successfully');
      setStatementDialogOpen(false);
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to generate statement');
    }
  };

  // ============================================================================
  // Render Functions
  // ============================================================================

  const formatCurrency = (amount: number, currency: string = 'USD') => {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(amount);
  };

  const formatPercent = (value: number) => `${(value * 100).toFixed(1)}%`;

  // ============================================================================
  // Render
  // ============================================================================

  return (
    <Container maxWidth="xl" sx={{ py: 3 }}>
      {/* Header */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" component="h1" fontWeight={600}>
          Commissions
        </Typography>
        <Box display="flex" gap={1}>
          <Button
            variant="outlined"
            startIcon={<StatementIcon />}
            onClick={() => setStatementDialogOpen(true)}
          >
            Generate Statement
          </Button>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleOpenCommissionDialog()}
          >
            New Commission
          </Button>
        </Box>
      </Box>

      {/* Alerts */}
      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
      {successMessage && <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccessMessage(null)}>{successMessage}</Alert>}

      {/* Statistics Cards */}
      {statistics && (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="text.secondary" gutterBottom>Total Commissions</Typography>
                <Typography variant="h5">{formatCurrency(statistics.totalCommissions)}</Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="text.secondary" gutterBottom>Total Paid</Typography>
                <Typography variant="h5" color="success.main">{formatCurrency(statistics.totalPaid)}</Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="text.secondary" gutterBottom>Pending</Typography>
                <Typography variant="h5" color="warning.main">{formatCurrency(statistics.totalPending)}</Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="text.secondary" gutterBottom>Pending Approvals</Typography>
                <Typography variant="h5" color="info.main">{statistics.pendingApprovals}</Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {/* Main Tabs */}
      <Card>
        <Tabs value={mainTab} onChange={(_, v) => setMainTab(v)} sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tab label="Commissions" />
          <Tab label="Plans" />
          <Tab label="Statistics" />
          <Tab label="Leaderboard" />
        </Tabs>

        {/* Commissions Tab */}
        <TabPanel value={mainTab} index={0}>
          <Box sx={{ p: 2 }}>
            <AdvancedSearch
              fields={SEARCH_FIELDS}
              onSearch={handleSearch}
              placeholder="Search commissions..."
            />
          </Box>
          
          {loading ? (
            <Box display="flex" justifyContent="center" p={4}>
              <CircularProgress />
            </Box>
          ) : filteredCommissions.length === 0 ? (
            <EnhancedEmptyState
              icon={<img src={logo} alt="Logo" style={{ width: 64, height: 64, opacity: 0.5 }} />}
              title="No Commissions Found"
              description="Start by creating a new commission or adjust your search filters."
              primaryActionLabel="Add Commission"
              onPrimaryAction={() => handleOpenCommissionDialog()}
            />
          ) : (
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>User</TableCell>
                    <TableCell>Deal</TableCell>
                    <TableCell align="right">Deal Amount</TableCell>
                    <TableCell align="right">Rate</TableCell>
                    <TableCell align="right">Commission</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {filteredCommissions.map((commission) => (
                    <TableRow key={commission.id} hover>
                      <TableCell>
                        {commission.user?.firstName} {commission.user?.lastName}
                      </TableCell>
                      <TableCell>
                        {commission.opportunity?.name || `Order #${commission.orderId}` || '-'}
                      </TableCell>
                      <TableCell align="right">
                        {formatCurrency(commission.dealAmount, commission.currencyCode)}
                      </TableCell>
                      <TableCell align="right">
                        {formatPercent(commission.commissionRate)}
                      </TableCell>
                      <TableCell align="right">
                        {formatCurrency(commission.finalCommissionAmount, commission.currencyCode)}
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={getStatusLabel(commission.status)}
                          color={getStatusColor(commission.status)}
                          size="small"
                        />
                      </TableCell>
                      <TableCell align="right">
                        <Box display="flex" justifyContent="flex-end" gap={0.5}>
                          {commission.status === CommissionStatus.Pending && (
                            <>
                              <Tooltip title="Approve">
                                <IconButton size="small" color="success" onClick={() => handleApprove(commission)}>
                                  <ApproveIcon fontSize="small" />
                                </IconButton>
                              </Tooltip>
                              <Tooltip title="Reject">
                                <IconButton size="small" color="error" onClick={() => openRejectDialog(commission)}>
                                  <RejectIcon fontSize="small" />
                                </IconButton>
                              </Tooltip>
                            </>
                          )}
                          {commission.status === CommissionStatus.Approved && (
                            <Tooltip title="Mark Paid">
                              <IconButton size="small" color="primary" onClick={() => handleMarkPaid(commission)}>
                                <PaidIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                          )}
                          {commission.status === CommissionStatus.Paid && (
                            <Tooltip title="Clawback">
                              <IconButton size="small" color="warning" onClick={() => openClawbackDialog(commission)}>
                                <ClawbackIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                          )}
                          <Tooltip title="Recalculate">
                            <IconButton size="small" onClick={() => handleRecalculate(commission)}>
                              <CalculateIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Edit">
                            <IconButton size="small" onClick={() => handleOpenCommissionDialog(commission)}>
                              <EditIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete">
                            <IconButton size="small" color="error" onClick={() => handleDeleteCommission(commission.id)}>
                              <DeleteIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        </Box>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </TabPanel>

        {/* Plans Tab */}
        <TabPanel value={mainTab} index={1}>
          <Box sx={{ p: 2, display: 'flex', justifyContent: 'flex-end' }}>
            <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenPlanDialog()}>
              New Plan
            </Button>
          </Box>
          
          {loadingPlans ? (
            <Box display="flex" justifyContent="center" p={4}>
              <CircularProgress />
            </Box>
          ) : (
            <Grid container spacing={2} sx={{ p: 2 }}>
              {/* Plans List */}
              <Grid item xs={12} md={selectedPlanForTiers ? 6 : 12}>
                <TableContainer component={Paper}>
                  <Table>
                    <TableHead>
                      <TableRow>
                        <TableCell>Name</TableCell>
                        <TableCell>Type</TableCell>
                        <TableCell>Base Rate</TableCell>
                        <TableCell>Trigger</TableCell>
                        <TableCell>Status</TableCell>
                        <TableCell align="right">Actions</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {plans.map((plan) => (
                        <TableRow 
                          key={plan.id} 
                          hover
                          selected={selectedPlanForTiers?.id === plan.id}
                          onClick={() => handleViewTiers(plan)}
                          sx={{ cursor: 'pointer' }}
                        >
                          <TableCell>{plan.name}</TableCell>
                          <TableCell>{getCommissionTypeLabel(plan.commissionType)}</TableCell>
                          <TableCell>{formatPercent(plan.baseRate)}</TableCell>
                          <TableCell>{getTriggerLabel(plan.trigger)}</TableCell>
                          <TableCell>
                            <Chip
                              label={getPlanStatusLabel(plan.status)}
                              color={plan.status === CommissionPlanStatus.Active ? 'success' : 'default'}
                              size="small"
                            />
                          </TableCell>
                          <TableCell align="right">
                            <IconButton size="small" onClick={(e) => { e.stopPropagation(); handleOpenPlanDialog(plan); }}>
                              <EditIcon fontSize="small" />
                            </IconButton>
                            <IconButton size="small" color="error" onClick={(e) => { e.stopPropagation(); handleDeletePlan(plan.id); }}>
                              <DeleteIcon fontSize="small" />
                            </IconButton>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </Grid>

              {/* Tiers Panel */}
              {selectedPlanForTiers && (
                <Grid item xs={12} md={6}>
                  <Paper sx={{ p: 2 }}>
                    <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
                      <Typography variant="h6">
                        Tiers for: {selectedPlanForTiers.name}
                      </Typography>
                      <Button size="small" startIcon={<AddIcon />} onClick={() => handleOpenTierDialog()}>
                        Add Tier
                      </Button>
                    </Box>
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Order</TableCell>
                          <TableCell>Name</TableCell>
                          <TableCell>Range</TableCell>
                          <TableCell>Rate</TableCell>
                          <TableCell>Multiplier</TableCell>
                          <TableCell align="right">Actions</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {tiers.map((tier) => (
                          <TableRow key={tier.id}>
                            <TableCell>{tier.tierOrder}</TableCell>
                            <TableCell>{tier.name || '-'}</TableCell>
                            <TableCell>
                              {tier.minAttainmentPercent}% - {tier.maxAttainmentPercent}%
                            </TableCell>
                            <TableCell>{tier.commissionRate ? formatPercent(tier.commissionRate) : formatCurrency(tier.fixedAmount || 0)}</TableCell>
                            <TableCell>{tier.multiplier}x</TableCell>
                            <TableCell align="right">
                              <IconButton size="small" onClick={() => handleOpenTierDialog(tier)}>
                                <EditIcon fontSize="small" />
                              </IconButton>
                              <IconButton size="small" color="error" onClick={() => handleDeleteTier(tier.id)}>
                                <DeleteIcon fontSize="small" />
                              </IconButton>
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </Paper>
                </Grid>
              )}
            </Grid>
          )}
        </TabPanel>

        {/* Statistics Tab */}
        <TabPanel value={mainTab} index={2}>
          {statistics && (
            <Box sx={{ p: 3 }}>
              <Grid container spacing={3}>
                <Grid item xs={12} md={6}>
                  <Card>
                    <CardContent>
                      <Typography variant="h6" gutterBottom>Commission Overview</Typography>
                      <Divider sx={{ mb: 2 }} />
                      <Grid container spacing={2}>
                        <Grid item xs={6}>
                          <Typography color="text.secondary">Total Records</Typography>
                          <Typography variant="h5">{statistics.totalRecords}</Typography>
                        </Grid>
                        <Grid item xs={6}>
                          <Typography color="text.secondary">Active Plans</Typography>
                          <Typography variant="h5">{statistics.activePlans}</Typography>
                        </Grid>
                        <Grid item xs={6}>
                          <Typography color="text.secondary">Average Commission</Typography>
                          <Typography variant="h5">{formatCurrency(statistics.averageCommission)}</Typography>
                        </Grid>
                        <Grid item xs={6}>
                          <Typography color="text.secondary">Pending Approvals</Typography>
                          <Typography variant="h5" color="warning.main">{statistics.pendingApprovals}</Typography>
                        </Grid>
                      </Grid>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} md={6}>
                  <Card>
                    <CardContent>
                      <Typography variant="h6" gutterBottom>Commissions by Plan</Typography>
                      <Divider sx={{ mb: 2 }} />
                      {Object.entries(statistics.commissionsByPlan).map(([plan, amount]) => (
                        <Box key={plan} display="flex" justifyContent="space-between" mb={1}>
                          <Typography>{plan}</Typography>
                          <Typography fontWeight={600}>{formatCurrency(amount)}</Typography>
                        </Box>
                      ))}
                    </CardContent>
                  </Card>
                </Grid>
              </Grid>
            </Box>
          )}
        </TabPanel>

        {/* Leaderboard Tab */}
        <TabPanel value={mainTab} index={3}>
          <Box sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              <LeaderboardIcon sx={{ verticalAlign: 'middle', mr: 1 }} />
              Top Performers
            </Typography>
            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Rank</TableCell>
                    <TableCell>Name</TableCell>
                    <TableCell align="right">Total Earned</TableCell>
                    <TableCell align="right">Deals</TableCell>
                    <TableCell align="right">Avg Deal Size</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {leaderboard.map((entry) => (
                    <TableRow key={entry.userId}>
                      <TableCell>
                        {entry.rank <= 3 ? (
                          <Chip 
                            label={`#${entry.rank}`} 
                            color={entry.rank === 1 ? 'warning' : entry.rank === 2 ? 'default' : 'primary'}
                            size="small"
                          />
                        ) : entry.rank}
                      </TableCell>
                      <TableCell>{entry.userName}</TableCell>
                      <TableCell align="right">{formatCurrency(entry.totalEarned)}</TableCell>
                      <TableCell align="right">{entry.dealCount}</TableCell>
                      <TableCell align="right">{formatCurrency(entry.averageDealSize)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </Box>
        </TabPanel>
      </Card>

      {/* Commission Dialog */}
      <Dialog open={openCommissionDialog} onClose={handleCloseCommissionDialog} maxWidth="md" fullWidth>
        <DialogTitle>{editingCommissionId ? 'Edit Commission' : 'New Commission'}</DialogTitle>
        <DialogContent>
          <DialogError error={dialogApi.error || dialogError} />
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12} sm={6}>
              <EntitySelect
                name="userId"
                label="User *"
                entityType="user"
                value={commissionForm.userId}
                onChange={(value) => setCommissionForm(prev => ({ ...prev, userId: value }))}
                fullWidth
              />
            </Grid>
            <Grid item xs={12} sm={6}>
<FormControl fullWidth>
                <InputLabel>Commission Plan</InputLabel>
                <Select
                  name="commissionPlanId"
                  value={commissionForm.commissionPlanId}
                  label="Commission Plan"
                  onChange={(e) => setCommissionForm(prev => ({ ...prev, commissionPlanId: e.target.value as number }))}
                >
                  <MenuItem value="">None</MenuItem>
                  {plans.map((plan) => (
                    <MenuItem key={plan.id} value={plan.id}>{plan.name}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <EntitySelect
                name="opportunityId"
                label="Opportunity"
                entityType="opportunity"
                value={commissionForm.opportunityId}
                onChange={(value) => setCommissionForm(prev => ({ ...prev, opportunityId: value }))}
                fullWidth
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                name="dealAmount"
                label="Deal Amount"
                type="number"
                value={commissionForm.dealAmount}
                onChange={handleCommissionInputChange}
                fullWidth
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                name="commissionRate"
                label="Commission Rate"
                type="number"
                value={commissionForm.commissionRate}
                onChange={handleCommissionInputChange}
                fullWidth
                inputProps={{ step: 0.01 }}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                name="commissionAmount"
                label="Commission Amount"
                type="number"
                value={commissionForm.commissionAmount}
                onChange={handleCommissionInputChange}
                fullWidth
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                name="splitPercent"
                label="Split Percent"
                type="number"
                value={commissionForm.splitPercent}
                onChange={handleCommissionInputChange}
                fullWidth
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                name="finalCommissionAmount"
                label="Final Commission Amount"
                type="number"
                value={commissionForm.finalCommissionAmount}
                onChange={handleCommissionInputChange}
                fullWidth
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                name="notes"
                label="Notes"
                value={commissionForm.notes}
                onChange={handleCommissionInputChange}
                fullWidth
                multiline
                rows={3}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseCommissionDialog}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleSaveCommission}
            disabled={dialogApi.loading}
          >
            {dialogApi.loading ? <CircularProgress size={24} /> : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Plan Dialog */}
      <Dialog open={openPlanDialog} onClose={handleClosePlanDialog} maxWidth="md" fullWidth>
        <DialogTitle>{editingPlanId ? 'Edit Plan' : 'New Commission Plan'}</DialogTitle>
        <DialogContent>
          <DialogError error={dialogApi.error || dialogError} />
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12} sm={6}>
              <TextField
                name="name"
                label="Plan Name *"
                value={planForm.name}
                onChange={handlePlanInputChange}
                fullWidth
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                name="code"
                label="Code"
                value={planForm.code}
                onChange={handlePlanInputChange}
                fullWidth
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                name="description"
                label="Description"
                value={planForm.description}
                onChange={handlePlanInputChange}
                fullWidth
                multiline
                rows={2}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Commission Type</InputLabel>
                <Select
                  name="commissionType"
                  value={planForm.commissionType}
                  label="Commission Type"
                  onChange={handlePlanSelectChange}
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
                name="baseRate"
                label="Base Rate"
                type="number"
                value={planForm.baseRate}
                onChange={handlePlanInputChange}
                fullWidth
                inputProps={{ step: 0.01 }}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Trigger</InputLabel>
                <Select
                  name="trigger"
                  value={planForm.trigger}
                  label="Trigger"
                  onChange={handlePlanSelectChange}
                >
                  <MenuItem value={CommissionTrigger.OnClose}>On Close</MenuItem>
                  <MenuItem value={CommissionTrigger.OnOrder}>On Order</MenuItem>
                  <MenuItem value={CommissionTrigger.OnInvoice}>On Invoice</MenuItem>
                  <MenuItem value={CommissionTrigger.OnPayment}>On Payment</MenuItem>
                  <MenuItem value={CommissionTrigger.OnSubscriptionStart}>On Subscription Start</MenuItem>
                  <MenuItem value={CommissionTrigger.OnSignature}>On Signature</MenuItem>
                  <MenuItem value={CommissionTrigger.Monthly}>Monthly</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                name="clawbackPeriodDays"
                label="Clawback Period (Days)"
                type="number"
                value={planForm.clawbackPeriodDays}
                onChange={handlePlanInputChange}
                fullWidth
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                name="effectiveStartDate"
                label="Effective Start Date"
                type="date"
                value={planForm.effectiveStartDate}
                onChange={handlePlanInputChange}
                fullWidth
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                name="effectiveEndDate"
                label="Effective End Date"
                type="date"
                value={planForm.effectiveEndDate}
                onChange={handlePlanInputChange}
                fullWidth
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClosePlanDialog}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleSavePlan}
            disabled={dialogApi.loading}
          >
            {dialogApi.loading ? <CircularProgress size={24} /> : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Tier Dialog */}
      <Dialog open={openTierDialog} onClose={handleCloseTierDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{editingTierId ? 'Edit Tier' : 'Add Tier'}</DialogTitle>
        <DialogContent>
          <DialogError error={dialogApi.error || dialogError} />
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12} sm={6}>
              <TextField
                name="name"
                label="Tier Name"
                value={tierForm.name}
                onChange={handleTierInputChange}
                fullWidth
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                name="tierOrder"
                label="Order"
                type="number"
                value={tierForm.tierOrder}
                onChange={handleTierInputChange}
                fullWidth
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                name="minAttainmentPercent"
                label="Min Attainment %"
                type="number"
                value={tierForm.minAttainmentPercent}
                onChange={handleTierInputChange}
                fullWidth
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                name="maxAttainmentPercent"
                label="Max Attainment %"
                type="number"
                value={tierForm.maxAttainmentPercent}
                onChange={handleTierInputChange}
                fullWidth
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                name="commissionRate"
                label="Commission Rate"
                type="number"
                value={tierForm.commissionRate}
                onChange={handleTierInputChange}
                fullWidth
                inputProps={{ step: 0.01 }}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                name="multiplier"
                label="Multiplier"
                type="number"
                value={tierForm.multiplier}
                onChange={handleTierInputChange}
                fullWidth
                inputProps={{ step: 0.1 }}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseTierDialog}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleSaveTier}
            disabled={dialogApi.loading}
          >
            {dialogApi.loading ? <CircularProgress size={24} /> : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Reject Dialog */}
      <Dialog open={rejectDialogOpen} onClose={() => setRejectDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Reject Commission</DialogTitle>
        <DialogContent>
          <TextField
            label="Reason for Rejection *"
            value={actionReason}
            onChange={(e) => setActionReason(e.target.value)}
            fullWidth
            multiline
            rows={3}
            sx={{ mt: 2 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRejectDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            color="error"
            onClick={handleConfirmReject}
            disabled={!actionReason}
          >
            Reject
          </Button>
        </DialogActions>
      </Dialog>

      {/* Clawback Dialog */}
      <Dialog open={clawbackDialogOpen} onClose={() => setClawbackDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Clawback Commission</DialogTitle>
        <DialogContent>
          <Alert severity="warning" sx={{ mb: 2 }}>
            This will reverse a paid commission. This action cannot be undone.
          </Alert>
          <TextField
            label="Reason for Clawback *"
            value={actionReason}
            onChange={(e) => setActionReason(e.target.value)}
            fullWidth
            multiline
            rows={3}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setClawbackDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            color="warning"
            onClick={handleConfirmClawback}
            disabled={!actionReason}
          >
            Clawback
          </Button>
        </DialogActions>
      </Dialog>

      {/* Statement Generation Dialog */}
      <Dialog open={statementDialogOpen} onClose={() => setStatementDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Generate Commission Statement</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <EntitySelect
                name="statementUserId"
                label="User *"
                entityType="user"
                value={statementUserId}
                onChange={(value) => setStatementUserId(value)}
                fullWidth
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="From Date *"
                type="date"
                value={statementFromDate}
                onChange={(e) => setStatementFromDate(e.target.value)}
                fullWidth
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="To Date *"
                type="date"
                value={statementToDate}
                onChange={(e) => setStatementToDate(e.target.value)}
                fullWidth
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setStatementDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleGenerateStatement}
            disabled={!statementUserId || !statementFromDate || !statementToDate}
          >
            Generate
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

export default CommissionsPage;

import { useState, useEffect, useMemo } from 'react';
import {
  Box, Card, CardContent, Typography, Button, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, TablePagination, Dialog, DialogTitle, DialogContent, DialogActions, Alert, CircularProgress,
  TextField, Container, FormControl, InputLabel, Select, MenuItem, Chip, Grid,
  IconButton, Tooltip, Tabs, Tab, SelectChangeEvent, Divider, Paper
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, 
  PlayArrow as ActivateIcon, Pause as PauseIcon, PlayCircle as ResumeIcon,
  Cancel as CancelIcon, Block as SuspendIcon, Restore as ReactivateIcon,
  Autorenew as RenewIcon, Receipt as InvoiceIcon, TrendingUp as UpgradeIcon,
  Extension as AddonIcon, Assessment as StatsIcon
} from '@mui/icons-material';
import subscriptionService, { 
  Subscription, SubscriptionStatus, SubscriptionCreateRequest, SubscriptionUpdateRequest,
  SubscriptionStatistics, getStatusLabel, getStatusColor, BILLING_CYCLES
} from '../services/subscriptionService';
import logger from '../services/logger';
import { TabPanel, DialogError, DialogSuccess, ActionButton, DialogHeader, EnhancedEmptyState } from '../components/common';
import { useApiState } from '../hooks/useApiState';
import { usePagination } from '../hooks/usePagination';
import { BaseEntity } from '../types';
import logo from '../assets/logo.png';
import EntitySelect from '../components/EntitySelect';
import AdvancedSearch, { SearchField, SearchFilter, filterData } from '../components/AdvancedSearch';

// Search fields for Advanced Search
const SEARCH_FIELDS: SearchField[] = [
  { name: 'subscriptionNumber', label: 'Subscription #', type: 'text' },
  { name: 'subscriptionStatus', label: 'Status', type: 'select', options: [
    { value: 0, label: 'Active' },
    { value: 1, label: 'Paused' },
    { value: 2, label: 'Cancelled' },
    { value: 3, label: 'Suspended' },
    { value: 4, label: 'Pending Cancellation' },
    { value: 5, label: 'Expired' },
    { value: 6, label: 'Trial' },
  ]},
  { name: 'billingCycle', label: 'Billing Cycle', type: 'select', options: [
    { value: 'Weekly', label: 'Weekly' },
    { value: 'Monthly', label: 'Monthly' },
    { value: 'Quarterly', label: 'Quarterly' },
    { value: 'Yearly', label: 'Yearly' },
  ]},
  { name: 'amount', label: 'Amount', type: 'numberRange' },
];

const SEARCHABLE_FIELDS = ['subscriptionNumber', 'billingContactName', 'billingContactEmail', 'tags'];

interface SubscriptionForm {
  accountId: number | '';
  productId: number | '';
  amount: number;
  billingCycle: string;
  startDate: string;
  endDate: string;
  billingStartDate: string;
  billingEndDate: string;
  isAutoRenew: boolean;
  currency: string;
  billingAddress: string;
  billingCity: string;
  billingState: string;
  billingZip: string;
  billingCountry: string;
  billingContactName: string;
  billingContactEmail: string;
  billingContactPhone: string;
  contractReference: string;
  tags: string;
}

function SubscriptionsPage() {
  const [subscriptions, setSubscriptions] = useState<Subscription[]>([]);
  const [statistics, setStatistics] = useState<SubscriptionStatistics | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [dialogError, setDialogError] = useState<string | null>(null);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  const [searchFilters, setSearchFilters] = useState<SearchFilter[]>([]);
  const [searchText, setSearchText] = useState('');
  const [actionDialogOpen, setActionDialogOpen] = useState(false);
  const [actionType, setActionType] = useState<'pause' | 'cancel' | 'suspend' | null>(null);
  const [actionReason, setActionReason] = useState('');
  const [actionImmediate, setActionImmediate] = useState(false);
  const [selectedSubscription, setSelectedSubscription] = useState<Subscription | null>(null);

  const handleSearch = (filters: SearchFilter[], text: string) => {
    setSearchFilters(filters);
    setSearchText(text);
  };

  const dialogApi = useApiState();

  const filteredSubscriptions = useMemo(
    () => filterData(subscriptions, searchFilters, searchText, SEARCHABLE_FIELDS),
    [subscriptions, searchFilters, searchText]
  );

  const { paginatedData: paginatedSubscriptions, page, pageSize, handlePageChange, handlePageSizeChange, pageSizeOptions } = usePagination(filteredSubscriptions, { defaultPageSize: 25 });

  const emptyForm: SubscriptionForm = {
    accountId: '', productId: '', amount: 0, billingCycle: 'Monthly',
    startDate: '', endDate: '', billingStartDate: '', billingEndDate: '',
    isAutoRenew: true, currency: 'USD', billingAddress: '', billingCity: '',
    billingState: '', billingZip: '', billingCountry: '', billingContactName: '',
    billingContactEmail: '', billingContactPhone: '', contractReference: '', tags: '',
  };
  const [formData, setFormData] = useState<SubscriptionForm>(emptyForm);

  useEffect(() => {
    fetchSubscriptions();
    fetchStatistics();
  }, []);

  const fetchSubscriptions = async () => {
    try {
      setLoading(true);
      const response = await subscriptionService.getAll();
      setSubscriptions(response.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch subscriptions');
      logger.error('Failed to fetch subscriptions', err);
    } finally {
      setLoading(false);
    }
  };

  const fetchStatistics = async () => {
    try {
      const response = await subscriptionService.getStatistics();
      setStatistics(response.data);
    } catch (err) {
      logger.error('Failed to fetch statistics', err);
    }
  };

  const handleOpenDialog = (subscription?: Subscription) => {
    setDialogTab(0);
    if (subscription) {
      setEditingId(subscription.id);
      setFormData({
        accountId: subscription.accountId || '',
        productId: subscription.productId || '',
        amount: subscription.amount,
        billingCycle: subscription.billingCycle || 'Monthly',
        startDate: subscription.startDate?.split('T')[0] || '',
        endDate: subscription.endDate?.split('T')[0] || '',
        billingStartDate: subscription.billingStartDate?.split('T')[0] || '',
        billingEndDate: subscription.billingEndDate?.split('T')[0] || '',
        isAutoRenew: subscription.isAutoRenew,
        currency: subscription.currency || 'USD',
        billingAddress: subscription.billingAddress || '',
        billingCity: subscription.billingCity || '',
        billingState: subscription.billingState || '',
        billingZip: subscription.billingZip || '',
        billingCountry: subscription.billingCountry || '',
        billingContactName: subscription.billingContactName || '',
        billingContactEmail: subscription.billingContactEmail || '',
        billingContactPhone: subscription.billingContactPhone || '',
        contractReference: subscription.contractReference || '',
        tags: subscription.tags || '',
      });
    } else {
      setEditingId(null);
      setFormData(emptyForm);
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
    setDialogError(null);
    dialogApi.reset();
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    if (type === 'checkbox') {
      setFormData(prev => ({ ...prev, [name]: (e.target as HTMLInputElement).checked }));
    } else {
      setFormData(prev => ({
        ...prev,
        [name]: type === 'number' ? parseFloat(value) || 0 : value,
      }));
    }
  };

  const handleSelectChange = (e: SelectChangeEvent<string | number>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSaveSubscription = async () => {
    if (!formData.accountId) {
      setDialogError('Please select an account');
      return;
    }
    if (formData.amount < 0) {
      setDialogError('Amount must be zero or positive');
      return;
    }
    const payload: SubscriptionCreateRequest = {
      ...formData,
      accountId: formData.accountId as number,
      productId: formData.productId ? formData.productId as number : undefined,
      startDate: formData.startDate || undefined,
      endDate: formData.endDate || undefined,
      billingStartDate: formData.billingStartDate || undefined,
      billingEndDate: formData.billingEndDate || undefined,
    };
    await dialogApi.execute(async () => {
      if (editingId) {
        await subscriptionService.update(editingId, payload);
        setSuccessMessage('Subscription updated successfully');
      } else {
        await subscriptionService.create(payload);
        setSuccessMessage('Subscription created successfully');
      }
      handleCloseDialog();
      fetchSubscriptions();
      fetchStatistics();
      setTimeout(() => setSuccessMessage(null), 3000);
    });
  };

  // Lifecycle Actions
  const handleActivate = async (id: number) => {
    try {
      await subscriptionService.activate(id);
      setSuccessMessage('Subscription activated');
      fetchSubscriptions();
      fetchStatistics();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to activate subscription');
    }
  };

  const openActionDialog = (subscription: Subscription, type: 'pause' | 'cancel' | 'suspend') => {
    setSelectedSubscription(subscription);
    setActionType(type);
    setActionReason('');
    setActionImmediate(false);
    setActionDialogOpen(true);
  };

  const handleConfirmAction = async () => {
    if (!selectedSubscription || !actionType) return;
    try {
      switch (actionType) {
        case 'pause':
          await subscriptionService.pause(selectedSubscription.id, actionReason || undefined);
          setSuccessMessage('Subscription paused');
          break;
        case 'cancel':
          await subscriptionService.cancel(selectedSubscription.id, actionReason, actionImmediate);
          setSuccessMessage('Subscription cancelled');
          break;
        case 'suspend':
          await subscriptionService.suspend(selectedSubscription.id, actionReason);
          setSuccessMessage('Subscription suspended');
          break;
      }
      setActionDialogOpen(false);
      fetchSubscriptions();
      fetchStatistics();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || `Failed to ${actionType} subscription`);
      setActionDialogOpen(false);
    }
  };

  const handleResume = async (id: number) => {
    try {
      await subscriptionService.resume(id);
      setSuccessMessage('Subscription resumed');
      fetchSubscriptions();
      fetchStatistics();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to resume subscription');
    }
  };

  const handleReactivate = async (id: number) => {
    try {
      await subscriptionService.reactivate(id);
      setSuccessMessage('Subscription reactivated');
      fetchSubscriptions();
      fetchStatistics();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to reactivate subscription');
    }
  };

  const handleRenew = async (id: number) => {
    try {
      await subscriptionService.renew(id);
      setSuccessMessage('Subscription renewed');
      fetchSubscriptions();
      fetchStatistics();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to renew subscription');
    }
  };

  const handleGenerateInvoice = async (id: number) => {
    try {
      await subscriptionService.generateInvoice(id);
      setSuccessMessage('Invoice generated');
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to generate invoice');
    }
  };

  const handleDeleteSubscription = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this subscription?')) {
      try {
        await subscriptionService.delete(id);
        setSuccessMessage('Subscription deleted successfully');
        fetchSubscriptions();
        fetchStatistics();
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete subscription');
      }
    }
  };

  const formatCurrency = (amount: number, currency?: string | null) => {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: currency || 'USD' }).format(amount);
  };

  const formatDate = (dateStr?: string) => {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString();
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ py: 4 }}>
      <Container maxWidth="xl">
        {/* Header */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
              <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
            </Box>
            <Typography variant="h4">Subscriptions</Typography>
          </Box>
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenDialog()}>
            New Subscription
          </Button>
        </Box>

        {/* Statistics Cards */}
        {statistics && (
          <Grid container spacing={2} sx={{ mb: 3 }}>
            <Grid item xs={12} sm={6} md={3}>
              <Card>
                <CardContent>
                  <Typography color="textSecondary" variant="body2">Active Subscriptions</Typography>
                  <Typography variant="h4">{statistics.activeSubscriptions}</Typography>
                </CardContent>
              </Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card>
                <CardContent>
                  <Typography color="textSecondary" variant="body2">Monthly Recurring Revenue</Typography>
                  <Typography variant="h4">{formatCurrency(statistics.mrr)}</Typography>
                </CardContent>
              </Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card>
                <CardContent>
                  <Typography color="textSecondary" variant="body2">Annual Recurring Revenue</Typography>
                  <Typography variant="h4">{formatCurrency(statistics.arr)}</Typography>
                </CardContent>
              </Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card>
                <CardContent>
                  <Typography color="textSecondary" variant="body2">Churn Rate</Typography>
                  <Typography variant="h4">{(statistics.churnRate * 100).toFixed(1)}%</Typography>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        )}

        {/* Alerts */}
        {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccessMessage(null)}>{successMessage}</Alert>}

        {/* Search */}
        <AdvancedSearch
          fields={SEARCH_FIELDS}
          onSearch={handleSearch}
          placeholder="Search subscriptions..."
        />

        {/* Subscriptions Table */}
        <Card sx={{ mt: 2 }}>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Subscription #</TableCell>
                  <TableCell>Account</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="right">Amount</TableCell>
                  <TableCell>Billing Cycle</TableCell>
                  <TableCell>Next Billing</TableCell>
                  <TableCell>Auto Renew</TableCell>
                  <TableCell align="center">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredSubscriptions.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={8}>
                      <EnhancedEmptyState
                        title="No subscriptions found"
                        description="Create your first subscription to get started"
                        primaryActionLabel="New Subscription"
                        onPrimaryAction={() => handleOpenDialog()}
                      />
                    </TableCell>
                  </TableRow>
                ) : (
                  paginatedSubscriptions.map((sub) => (
                    <TableRow key={sub.id} hover>
                      <TableCell>
                        <Typography fontWeight="medium">{sub.subscriptionNumber}</Typography>
                      </TableCell>
                      <TableCell>
                        {sub.account?.company || sub.account?.email || `Account #${sub.accountId}`}
                      </TableCell>
                      <TableCell>
                        <Chip 
                          label={getStatusLabel(sub.subscriptionStatus)} 
                          color={getStatusColor(sub.subscriptionStatus)}
                          size="small"
                        />
                      </TableCell>
                      <TableCell align="right">
                        {formatCurrency(sub.amount, sub.currency)}
                      </TableCell>
                      <TableCell>{sub.billingCycle || '-'}</TableCell>
                      <TableCell>{formatDate(sub.nextBillingDate)}</TableCell>
                      <TableCell>
                        <Chip 
                          label={sub.isAutoRenew ? 'Yes' : 'No'} 
                          color={sub.isAutoRenew ? 'success' : 'default'}
                          size="small"
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell align="center">
                        <Box sx={{ display: 'flex', justifyContent: 'center', gap: 0.5 }}>
                          <Tooltip title="Edit">
                            <IconButton size="small" onClick={() => handleOpenDialog(sub)}>
                              <EditIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          
                          {/* Lifecycle actions based on status */}
                          {sub.subscriptionStatus === SubscriptionStatus.Trial && (
                            <Tooltip title="Activate">
                              <IconButton size="small" color="success" onClick={() => handleActivate(sub.id)}>
                                <ActivateIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                          )}
                          
                          {sub.subscriptionStatus === SubscriptionStatus.Active && (
                            <>
                              <Tooltip title="Pause">
                                <IconButton size="small" color="warning" onClick={() => openActionDialog(sub, 'pause')}>
                                  <PauseIcon fontSize="small" />
                                </IconButton>
                              </Tooltip>
                              <Tooltip title="Cancel">
                                <IconButton size="small" color="error" onClick={() => openActionDialog(sub, 'cancel')}>
                                  <CancelIcon fontSize="small" />
                                </IconButton>
                              </Tooltip>
                              <Tooltip title="Generate Invoice">
                                <IconButton size="small" onClick={() => handleGenerateInvoice(sub.id)}>
                                  <InvoiceIcon fontSize="small" />
                                </IconButton>
                              </Tooltip>
                            </>
                          )}
                          
                          {sub.subscriptionStatus === SubscriptionStatus.Paused && (
                            <Tooltip title="Resume">
                              <IconButton size="small" color="success" onClick={() => handleResume(sub.id)}>
                                <ResumeIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                          )}
                          
                          {(sub.subscriptionStatus === SubscriptionStatus.Cancelled || 
                            sub.subscriptionStatus === SubscriptionStatus.Suspended) && (
                            <Tooltip title="Reactivate">
                              <IconButton size="small" color="primary" onClick={() => handleReactivate(sub.id)}>
                                <ReactivateIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                          )}
                          
                          {sub.subscriptionStatus === SubscriptionStatus.Expired && (
                            <Tooltip title="Renew">
                              <IconButton size="small" color="success" onClick={() => handleRenew(sub.id)}>
                                <RenewIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                          )}
                          
                          <Tooltip title="Delete">
                            <IconButton size="small" color="error" onClick={() => handleDeleteSubscription(sub.id)}>
                              <DeleteIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        </Box>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
            <TablePagination
              component="div"
              count={filteredSubscriptions.length}
              page={page}
              onPageChange={handlePageChange}
              rowsPerPage={pageSize}
              onRowsPerPageChange={handlePageSizeChange}
              rowsPerPageOptions={pageSizeOptions}
            />
          </TableContainer>
        </Card>

        {/* Create/Edit Dialog */}
        <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
          <DialogHeader 
            mode={editingId ? 'edit' : 'create'} 
            entityType="contract" 
            title={editingId ? 'Edit Subscription' : 'New Subscription'} 
            onClose={handleCloseDialog}
          />
          <DialogContent>
            <DialogError error={dialogApi.error || dialogError} />
            
            <Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)} sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
              <Tab label="Details" />
              <Tab label="Billing" />
              <Tab label="Billing Address" />
            </Tabs>

            <TabPanel value={dialogTab} index={0}>
              <Grid container spacing={2}>
                <Grid item xs={12}>
                  <EntitySelect
                    entityType="account"
                    name="accountId"
                    label="Account *"
                    value={formData.accountId}
                    onChange={(val) => setFormData(prev => ({ ...prev, accountId: val || '' }))}
                    fullWidth
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <EntitySelect
                    entityType="product"
                    name="productId"
                    label="Product"
                    value={formData.productId}
                    onChange={(val) => setFormData(prev => ({ ...prev, productId: val || '' }))}
                    fullWidth
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Amount *"
                    name="amount"
                    type="number"
                    value={formData.amount}
                    onChange={handleInputChange}
                    inputProps={{ min: 0, step: 0.01 }}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <FormControl fullWidth>
                    <InputLabel>Billing Cycle</InputLabel>
                    <Select
                      name="billingCycle"
                      value={formData.billingCycle}
                      label="Billing Cycle"
                      onChange={handleSelectChange}
                    >
                      {BILLING_CYCLES.map(cycle => (
                        <MenuItem key={cycle} value={cycle}>{cycle}</MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Currency"
                    name="currency"
                    value={formData.currency}
                    onChange={handleInputChange}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Start Date"
                    name="startDate"
                    type="date"
                    value={formData.startDate}
                    onChange={handleInputChange}
                    InputLabelProps={{ shrink: true }}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="End Date"
                    name="endDate"
                    type="date"
                    value={formData.endDate}
                    onChange={handleInputChange}
                    InputLabelProps={{ shrink: true }}
                  />
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Contract Reference"
                    name="contractReference"
                    value={formData.contractReference}
                    onChange={handleInputChange}
                  />
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Tags"
                    name="tags"
                    value={formData.tags}
                    onChange={handleInputChange}
                    helperText="Comma-separated tags"
                  />
                </Grid>
              </Grid>
            </TabPanel>

            <TabPanel value={dialogTab} index={1}>
              <Grid container spacing={2}>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Billing Start Date"
                    name="billingStartDate"
                    type="date"
                    value={formData.billingStartDate}
                    onChange={handleInputChange}
                    InputLabelProps={{ shrink: true }}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Billing End Date"
                    name="billingEndDate"
                    type="date"
                    value={formData.billingEndDate}
                    onChange={handleInputChange}
                    InputLabelProps={{ shrink: true }}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Billing Contact Name"
                    name="billingContactName"
                    value={formData.billingContactName}
                    onChange={handleInputChange}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Billing Contact Email"
                    name="billingContactEmail"
                    type="email"
                    value={formData.billingContactEmail}
                    onChange={handleInputChange}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Billing Contact Phone"
                    name="billingContactPhone"
                    value={formData.billingContactPhone}
                    onChange={handleInputChange}
                  />
                </Grid>
              </Grid>
            </TabPanel>

            <TabPanel value={dialogTab} index={2}>
              <Grid container spacing={2}>
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Billing Address"
                    name="billingAddress"
                    value={formData.billingAddress}
                    onChange={handleInputChange}
                    multiline
                    rows={2}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="City"
                    name="billingCity"
                    value={formData.billingCity}
                    onChange={handleInputChange}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="State/Province"
                    name="billingState"
                    value={formData.billingState}
                    onChange={handleInputChange}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="ZIP/Postal Code"
                    name="billingZip"
                    value={formData.billingZip}
                    onChange={handleInputChange}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Country"
                    name="billingCountry"
                    value={formData.billingCountry}
                    onChange={handleInputChange}
                  />
                </Grid>
              </Grid>
            </TabPanel>
          </DialogContent>
          <DialogActions>
            <Button onClick={handleCloseDialog}>Cancel</Button>
            <Button variant="contained" onClick={handleSaveSubscription} disabled={dialogApi.loading}>
              {dialogApi.loading ? <CircularProgress size={24} /> : editingId ? 'Update' : 'Create'}
            </Button>
          </DialogActions>
        </Dialog>

        {/* Action Dialog (Pause/Cancel/Suspend) */}
        <Dialog open={actionDialogOpen} onClose={() => setActionDialogOpen(false)} maxWidth="sm" fullWidth>
          <DialogTitle>
            {actionType === 'pause' && 'Pause Subscription'}
            {actionType === 'cancel' && 'Cancel Subscription'}
            {actionType === 'suspend' && 'Suspend Subscription'}
          </DialogTitle>
          <DialogContent>
            <TextField
              fullWidth
              label="Reason"
              value={actionReason}
              onChange={(e) => setActionReason(e.target.value)}
              multiline
              rows={3}
              sx={{ mt: 2 }}
              required={actionType === 'cancel' || actionType === 'suspend'}
            />
            {actionType === 'cancel' && (
              <FormControl fullWidth sx={{ mt: 2 }}>
                <InputLabel>When to Cancel</InputLabel>
                <Select
                  value={actionImmediate ? 'immediate' : 'end-of-period'}
                  label="When to Cancel"
                  onChange={(e) => setActionImmediate(e.target.value === 'immediate')}
                >
                  <MenuItem value="end-of-period">At End of Billing Period</MenuItem>
                  <MenuItem value="immediate">Immediately</MenuItem>
                </Select>
              </FormControl>
            )}
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setActionDialogOpen(false)}>Cancel</Button>
            <Button 
              variant="contained" 
              color={actionType === 'pause' ? 'warning' : 'error'}
              onClick={handleConfirmAction}
              disabled={(actionType === 'cancel' || actionType === 'suspend') && !actionReason}
            >
              Confirm
            </Button>
          </DialogActions>
        </Dialog>
      </Container>
    </Box>
  );
}

export default SubscriptionsPage;

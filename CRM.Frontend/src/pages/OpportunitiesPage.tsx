import { useState, useEffect, useMemo } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
  CircularProgress,
  TextField,
  Container,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Chip,
  Slider,
  Checkbox,
  Paper,
  Collapse,
  Stack,
  IconButton,
  SelectChangeEvent,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Close as CloseIcon,
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import { getApiErrorMessage } from '../utils/errorHandler';
import logo from '../assets/logo.png';
import LookupSelect from '../components/LookupSelect';
import EntitySelect from '../components/EntitySelect';
import ImportExportButtons from '../components/ImportExportButtons';
import AdvancedSearch, { SearchField, SearchFilter, filterData } from '../components/AdvancedSearch';
import { useAccountContext } from '../contexts/AccountContextProvider';
import { useProfile } from '../contexts/ProfileContext';
import { BaseEntity } from '../types';
import { DialogError, DialogSuccess, ActionButton } from '../components/common';
import { useApiState } from '../hooks/useApiState';

// Search fields for Advanced Search
const SEARCH_FIELDS: SearchField[] = [
  { name: 'name', label: 'Title', type: 'text' },
  { name: 'stage', label: 'Stage', type: 'select', options: [
    { value: 0, label: 'Discovery' },
    { value: 1, label: 'Qualification' },
    { value: 2, label: 'Proposal' },
    { value: 3, label: 'Negotiation' },
    { value: 4, label: 'Closed Won' },
    { value: 5, label: 'Closed Lost' },
  ]},
  { name: 'solutionNotes', label: 'Solution Notes', type: 'text' },
  { name: 'amount', label: 'Amount', type: 'numberRange' },
  { name: 'region', label: 'Region', type: 'text' },
];

const SEARCHABLE_FIELDS = ['name', 'solutionNotes', 'region'];

interface Opportunity extends BaseEntity {
  name: string;
  stage: number;
  probability: number;
  amount: number;
  currency?: string;
  expectedCloseDate?: string;
  pricingModel?: number;
  termLengthMonths?: number;
  solutionNotes?: string;
  qualificationReason?: number;
  qualificationNotes?: string;
  region?: string;
  accountId: number;
  primaryContactId?: number;
  salesOwnerId?: number;
  leadId?: number;
  // Navigation properties from API
  accountName?: string;
  primaryContactName?: string;
  salesOwnerName?: string;
}

interface Account extends BaseEntity {
  firstName?: string;
  lastName?: string;
  company?: string;
  legalName?: string;
}

interface User extends BaseEntity {
  username: string;
  firstName: string;
  lastName: string;
}

interface OpportunityForm {
  name: string;
  stage: number;
  probability: number;
  amount: number;
  currency: string;
  expectedCloseDate: string;
  pricingModel: number;
  termLengthMonths: number;
  solutionNotes: string;
  qualificationReason: number | null;
  qualificationNotes: string;
  region: string;
  accountId: number;
  primaryContactId: number | null;
  salesOwnerId: number | null;
}

const STAGES = [
  { value: 0, label: 'Discovery', color: '#9e9e9e' },
  { value: 1, label: 'Qualification', color: '#2196f3' },
  { value: 2, label: 'Proposal', color: '#ff9800' },
  { value: 3, label: 'Negotiation', color: '#9c27b0' },
  { value: 4, label: 'Closed Won', color: '#4caf50' },
  { value: 5, label: 'Closed Lost', color: '#f44336' },
];

const PRICING_MODELS = [
  { value: 0, label: 'Subscription' },
  { value: 1, label: 'One-Time' },
  { value: 2, label: 'Usage-Based' },
  { value: 3, label: 'Hybrid' },
];

const QUALIFICATION_REASONS = [
  { value: 0, label: 'Budget' },
  { value: 1, label: 'Need' },
  { value: 2, label: 'Timing' },
  { value: 3, label: 'Authority' },
  { value: 4, label: 'Fit' },
];

function OpportunitiesPage() {
  const [opportunities, setOpportunities] = useState<Opportunity[]>([]);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<OpportunityForm>({
    name: '',
    stage: 0,
    probability: 10,
    amount: 0,
    currency: 'USD',
    expectedCloseDate: '',
    pricingModel: 0,
    termLengthMonths: 12,
    solutionNotes: '',
    qualificationReason: null,
    qualificationNotes: '',
    region: '',
    accountId: 0,
    primaryContactId: null,
    salesOwnerId: null,
  });
  const [searchFilters, setSearchFilters] = useState<SearchFilter[]>([]);
  const [searchText, setSearchText] = useState('');
  
  // Multi-select and bulk update state
  const [selectedIds, setSelectedIds] = useState<number[]>([]);
  const [bulkDialogOpen, setBulkDialogOpen] = useState(false);
  const [bulkFormData, setBulkFormData] = useState({
    stage: '' as string | number,
    probability: '' as string | number,
    pricingModel: '' as string | number,
    region: '' as string,
  });
  
  // API state for dialog operations
  const dialogApi = useApiState({ successTimeout: 3000 });
  const bulkApi = useApiState({ successTimeout: 3000 });

  // Get account context for filtering
  const { selectedAccounts, isContextActive, getAccountIds } = useAccountContext();
  const { hasPermission } = useProfile();

  const handleSearch = (filters: SearchFilter[], text: string) => {
    setSearchFilters(filters);
    setSearchText(text);
  };

  // Filter opportunities based on search AND account context
  const filteredOpportunities = useMemo(() => {
    let result = opportunities;
    
    // Apply account context filter first (filter by accountId)
    if (isContextActive) {
      const accountIds = getAccountIds();
      result = result.filter(opp => accountIds.includes(opp.accountId));
    }
    
    // Then apply search filters
    return filterData(result, searchFilters, searchText, SEARCHABLE_FIELDS);
  }, [opportunities, searchFilters, searchText, isContextActive, getAccountIds]);

  useEffect(() => {
    fetchAllData();
  }, []);

  const fetchAllData = async () => {
    try {
      setLoading(true);
      const [oppRes, acctRes, userRes] = await Promise.all([
        apiClient.get('/opportunities'),
        apiClient.get('/accounts'),
        apiClient.get('/users').catch(() => ({ data: [] })),
      ]);
      setOpportunities(oppRes.data);
      setAccounts(acctRes.data);
      setUsers(userRes.data);
      setError(null);
    } catch (err: any) {
      setError(getApiErrorMessage(err, 'Failed to fetch data'));
      console.error('Error fetching data:', err);
    } finally {
      setLoading(false);
    }
  };

  const getAccountName = (accountId: number) => {
    const account = accounts.find(a => a.id === accountId);
    if (!account) return 'Unknown';
    if (account.company) return account.company;
    if (account.firstName || account.lastName) return `${account.firstName || ''} ${account.lastName || ''}`.trim();
    return 'Unnamed Account';
  };

  const getUserName = (userId?: number) => {
    if (!userId) return '-';
    const user = users.find(u => u.id === userId);
    return user ? `${user.firstName} ${user.lastName}` : '-';
  };

  const getStageInfo = (stage: number) => {
    return STAGES.find(s => s.value === stage) || STAGES[0];
  };

  const handleOpenDialog = (opp?: Opportunity) => {
    if (opp) {
      setEditingId(opp.id);
      setFormData({
        name: opp.name,
        stage: opp.stage,
        probability: opp.probability,
        amount: opp.amount,
        currency: opp.currency || 'USD',
        expectedCloseDate: opp.expectedCloseDate?.split('T')[0] || '',
        pricingModel: opp.pricingModel ?? 0,
        termLengthMonths: opp.termLengthMonths ?? 12,
        solutionNotes: opp.solutionNotes || '',
        qualificationReason: opp.qualificationReason ?? null,
        qualificationNotes: opp.qualificationNotes || '',
        region: opp.region || '',
        accountId: opp.accountId,
        primaryContactId: opp.primaryContactId || null,
        salesOwnerId: opp.salesOwnerId || null,
      });
    } else {
      setEditingId(null);
      setFormData({
        name: '',
        stage: 0,
        probability: 10,
        amount: 0,
        currency: 'USD',
        expectedCloseDate: '',
        pricingModel: 0,
        termLengthMonths: 12,
        solutionNotes: '',
        qualificationReason: null,
        qualificationNotes: '',
        region: '',
        accountId: accounts[0]?.id || 0,
        primaryContactId: null,
        salesOwnerId: null,
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
  };

  const handleSelectChange = (e: any) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value === '' ? null : value,
    }));
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'amount' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleProbabilityChange = (_: Event, value: number | number[]) => {
    setFormData(prev => ({
      ...prev,
      probability: value as number,
    }));
  };

  const handleSaveOpportunity = async () => {
    if (!formData.name.trim() || !formData.accountId) {
      dialogApi.setError('Please fill in required fields (Name, Account)');
      return;
    }

    const result = await dialogApi.execute(async () => {
      const payload = {
        ...formData,
        primaryContactId: formData.primaryContactId || undefined,
        salesOwnerId: formData.salesOwnerId || undefined,
        qualificationReason: formData.qualificationReason ?? undefined,
        expectedCloseDate: formData.expectedCloseDate || undefined,
      };

      if (editingId) {
        await apiClient.put(`/opportunities/${editingId}`, payload);
        return 'updated';
      } else {
        await apiClient.post('/opportunities', payload);
        return 'created';
      }
    }, editingId ? 'Opportunity updated successfully' : 'Opportunity created successfully');

    if (result) {
      handleCloseDialog();
      fetchAllData();
      setSuccessMessage(result === 'updated' ? 'Opportunity updated successfully' : 'Opportunity created successfully');
      setTimeout(() => setSuccessMessage(null), 3000);
    }
  };

  const handleDeleteOpportunity = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this opportunity?')) {
      const result = await dialogApi.execute(async () => {
        await apiClient.delete(`/opportunities/${id}`);
        return true;
      }, 'Opportunity deleted successfully');
      
      if (result) {
        setSelectedIds(prev => prev.filter(sid => sid !== id));
        fetchAllData();
        setSuccessMessage('Opportunity deleted successfully');
        setTimeout(() => setSuccessMessage(null), 3000);
      } else {
        setError(dialogApi.error?.message || 'Failed to delete opportunity');
      }
    }
  };

  // Multi-select handlers
  const handleSelectAll = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.checked) {
      setSelectedIds(filteredOpportunities.map(o => o.id));
    } else {
      setSelectedIds([]);
    }
  };

  const handleSelectOne = (id: number) => {
    setSelectedIds(prev => 
      prev.includes(id) 
        ? prev.filter(sid => sid !== id)
        : [...prev, id]
    );
  };

  const handleOpenBulkDialog = () => {
    setBulkFormData({
      stage: '',
      probability: '',
      pricingModel: '',
      region: '',
    });
    bulkApi.clearError();
    setBulkDialogOpen(true);
  };

  const handleBulkUpdate = async () => {
    if (selectedIds.length === 0) {
      bulkApi.setError('No opportunities selected');
      return;
    }

    const updatePayload: Record<string, any> = {};
    if (bulkFormData.stage !== '') updatePayload.stage = Number(bulkFormData.stage);
    if (bulkFormData.probability !== '') updatePayload.probability = Number(bulkFormData.probability);
    if (bulkFormData.pricingModel !== '') updatePayload.pricingModel = Number(bulkFormData.pricingModel);
    if (bulkFormData.region) updatePayload.region = bulkFormData.region;

    if (Object.keys(updatePayload).length === 0) {
      bulkApi.setError('Please select at least one field to update');
      return;
    }

    const result = await bulkApi.execute(async () => {
      const updatePromises = selectedIds.map(id =>
        apiClient.put(`/opportunities/${id}`, updatePayload)
      );
      await Promise.all(updatePromises);
      return selectedIds.length;
    }, `Successfully updated ${selectedIds.length} opportunity(ies)`);

    if (result) {
      fetchAllData();
      setBulkDialogOpen(false);
      setSelectedIds([]);
      setSuccessMessage(`Successfully updated ${result} opportunity(ies)`);
      setTimeout(() => setSuccessMessage(null), 3000);
    }
  };

  const handleBulkDelete = async () => {
    if (selectedIds.length === 0) return;
    
    if (!window.confirm(`Are you sure you want to delete ${selectedIds.length} opportunity(ies)?`)) {
      return;
    }

    const result = await bulkApi.execute(async () => {
      const deletePromises = selectedIds.map(id => apiClient.delete(`/opportunities/${id}`));
      await Promise.all(deletePromises);
      return selectedIds.length;
    }, `Successfully deleted ${selectedIds.length} opportunity(ies)`);

    if (result) {
      fetchAllData();
      setSelectedIds([]);
      setSuccessMessage(`Successfully deleted ${result} opportunity(ies)`);
      setTimeout(() => setSuccessMessage(null), 3000);
    } else {
      setError(bulkApi.error?.message || 'Failed to delete some opportunities');
    }
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
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ width: 40, height: 40, flexShrink: 0 }}><img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} /></Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>Opportunities</Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <ImportExportButtons entityType="opportunities" entityLabel="Opportunities" onImportComplete={fetchAllData} />
            <Button
              variant="contained"
              color="primary"
              startIcon={<AddIcon />}
              onClick={() => handleOpenDialog()}
              sx={{ backgroundColor: '#6750A4' }}
            >
              Add Opportunity
            </Button>
          </Box>
        </Box>

        {error && (
          <Alert 
            severity="error" 
            sx={{ mb: 2, whiteSpace: 'pre-line' }} 
            onClose={() => setError(null)}
          >
            {error}
          </Alert>
        )}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccessMessage(null)}>{successMessage}</Alert>}

        <AdvancedSearch
          fields={SEARCH_FIELDS}
          onSearch={handleSearch}
          placeholder="Search opportunities by title, description..."
        />

        {/* Bulk Actions Toolbar */}
        <Collapse in={selectedIds.length > 0}>
          <Paper sx={{ mb: 2, p: 2, backgroundColor: 'primary.light' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <Typography sx={{ color: 'primary.contrastText' }}>
                {selectedIds.length} opportunity(ies) selected
              </Typography>
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button
                  variant="contained"
                  size="small"
                  onClick={handleOpenBulkDialog}
                  sx={{ backgroundColor: 'white', color: 'primary.main', '&:hover': { backgroundColor: 'grey.100' } }}
                >
                  Bulk Update
                </Button>
                {hasPermission('canDeleteOpportunities') && (
                  <Button
                    variant="contained"
                    size="small"
                    color="error"
                    onClick={handleBulkDelete}
                  >
                    Delete Selected
                  </Button>
                )}
                <IconButton size="small" onClick={() => setSelectedIds([])} sx={{ color: 'white' }}>
                  <CloseIcon />
                </IconButton>
              </Box>
            </Box>
          </Paper>
        </Collapse>

        <Card>
          <CardContent sx={{ p: 0 }}>
            <TableContainer sx={{ overflowX: 'auto' }}>
              <Table sx={{ minWidth: 950 }}>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                  <TableCell padding="checkbox">
                    <Checkbox
                      indeterminate={selectedIds.length > 0 && selectedIds.length < filteredOpportunities.length}
                      checked={filteredOpportunities.length > 0 && selectedIds.length === filteredOpportunities.length}
                      onChange={handleSelectAll}
                    />
                  </TableCell>
                  <TableCell><strong>Name</strong></TableCell>
                  <TableCell><strong>Account</strong></TableCell>
                  <TableCell><strong>Amount</strong></TableCell>
                  <TableCell><strong>Stage</strong></TableCell>
                  <TableCell><strong>Probability</strong></TableCell>
                  <TableCell><strong>Expected Close</strong></TableCell>
                  <TableCell><strong>Pricing Model</strong></TableCell>
                  <TableCell><strong>Sales Owner</strong></TableCell>
                  <TableCell align="center"><strong>Actions</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredOpportunities.map((opp) => {
                  const stageInfo = getStageInfo(opp.stage);
                  const pricingInfo = PRICING_MODELS.find(p => p.value === opp.pricingModel) || PRICING_MODELS[0];
                  return (
                    <TableRow key={opp.id} hover selected={selectedIds.includes(opp.id)}>
                      <TableCell padding="checkbox">
                        <Checkbox
                          checked={selectedIds.includes(opp.id)}
                          onChange={() => handleSelectOne(opp.id)}
                        />
                      </TableCell>
                      <TableCell>{opp.name}</TableCell>
                      <TableCell>{opp.accountName || getAccountName(opp.accountId)}</TableCell>
                      <TableCell>{opp.currency || 'USD'} {opp.amount?.toLocaleString() || 0}</TableCell>
                      <TableCell>
                        <Chip 
                          label={stageInfo.label} 
                          size="small"
                          sx={{ backgroundColor: stageInfo.color, color: 'white' }}
                        />
                      </TableCell>
                      <TableCell>{opp.probability}%</TableCell>
                      <TableCell>{opp.expectedCloseDate ? new Date(opp.expectedCloseDate).toLocaleDateString() : '-'}</TableCell>
                      <TableCell>{pricingInfo.label}</TableCell>
                      <TableCell>{opp.salesOwnerName || getUserName(opp.salesOwnerId)}</TableCell>
                      <TableCell align="center">
                        <Button
                          size="small"
                          color="primary"
                          startIcon={<EditIcon />}
                          onClick={() => handleOpenDialog(opp)}
                          sx={{ mr: 1 }}
                        >
                          Edit
                        </Button>
                        <Button
                          size="small"
                          color="error"
                          startIcon={<DeleteIcon />}
                          onClick={() => handleDeleteOpportunity(opp.id)}
                        >
                          Delete
                        </Button>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
              </Table>
            </TableContainer>
            {filteredOpportunities.length === 0 && (
              <Typography sx={{ textAlign: 'center', py: 2, color: 'textSecondary' }}>
                No opportunities found
              </Typography>
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Add/Edit Opportunity Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle>{editingId ? 'Edit Opportunity' : 'Create New Opportunity'}</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 2 }}>
            <TextField
              autoFocus
              fullWidth
              label="Name"
              name="name"
              value={formData.name}
              onChange={handleInputChange}
              margin="normal"
              required
            />
            
            <Box sx={{ mt: 2 }}>
              <EntitySelect
                entityType="account"
                name="accountId"
                value={formData.accountId}
                onChange={handleSelectChange}
                label="Account"
                required
                showAddNew={true}
              />
            </Box>

            <TextField
              fullWidth
              label="Amount"
              name="amount"
              type="number"
              value={formData.amount}
              onChange={handleInputChange}
              margin="normal"
              inputProps={{ step: "0.01", min: 0 }}
            />

            <FormControl fullWidth margin="normal">
              <InputLabel>Currency</InputLabel>
              <Select
                name="currency"
                value={formData.currency}
                onChange={handleSelectChange}
                label="Currency"
              >
                <MenuItem value="USD">USD</MenuItem>
                <MenuItem value="EUR">EUR</MenuItem>
                <MenuItem value="GBP">GBP</MenuItem>
                <MenuItem value="INR">INR</MenuItem>
                <MenuItem value="CAD">CAD</MenuItem>
                <MenuItem value="AUD">AUD</MenuItem>
              </Select>
            </FormControl>

            <LookupSelect
              category="OpportunityStage"
              name="stage"
              value={formData.stage}
              onChange={handleSelectChange}
              label="Stage"
              fallback={STAGES.map(s => ({ value: s.value, label: s.label }))}
            />

            <FormControl fullWidth margin="normal">
              <InputLabel>Pricing Model</InputLabel>
              <Select
                name="pricingModel"
                value={formData.pricingModel}
                onChange={handleSelectChange}
                label="Pricing Model"
              >
                {PRICING_MODELS.map(pm => (
                  <MenuItem key={pm.value} value={pm.value}>{pm.label}</MenuItem>
                ))}
              </Select>
            </FormControl>

            <Box sx={{ mt: 2, mb: 1, gridColumn: { xs: '1', md: '1 / -1' } }}>
              <Typography gutterBottom>Probability: {formData.probability}%</Typography>
              <Slider
                value={formData.probability}
                onChange={handleProbabilityChange}
                min={0}
                max={100}
                step={5}
                marks={[
                  { value: 0, label: '0%' },
                  { value: 50, label: '50%' },
                  { value: 100, label: '100%' },
                ]}
                valueLabelDisplay="auto"
              />
            </Box>

            <TextField
              fullWidth
              label="Expected Close Date"
              name="expectedCloseDate"
              type="date"
              value={formData.expectedCloseDate}
              onChange={handleInputChange}
              margin="normal"
              InputLabelProps={{ shrink: true }}
            />

            <TextField
              fullWidth
              label="Term Length (Months)"
              name="termLengthMonths"
              type="number"
              value={formData.termLengthMonths}
              onChange={handleInputChange}
              margin="normal"
              inputProps={{ min: 1 }}
            />

            <TextField
              fullWidth
              label="Region"
              name="region"
              value={formData.region}
              onChange={handleInputChange}
              margin="normal"
            />

            <FormControl fullWidth margin="normal">
              <InputLabel>Qualification Reason</InputLabel>
              <Select
                name="qualificationReason"
                value={formData.qualificationReason ?? ''}
                onChange={handleSelectChange}
                label="Qualification Reason"
              >
                <MenuItem value="">None</MenuItem>
                {QUALIFICATION_REASONS.map(qr => (
                  <MenuItem key={qr.value} value={qr.value}>{qr.label}</MenuItem>
                ))}
              </Select>
            </FormControl>

            <Box sx={{ mt: 2 }}>
              <EntitySelect
                entityType="user"
                name="salesOwnerId"
                value={formData.salesOwnerId || ''}
                onChange={handleSelectChange}
                label="Sales Owner (Optional)"
                showAddNew={false}
              />
            </Box>

            <TextField
              fullWidth
              label="Solution Notes"
              name="solutionNotes"
              value={formData.solutionNotes}
              onChange={handleInputChange}
              margin="normal"
              multiline
              rows={2}
              sx={{ gridColumn: { xs: '1', md: '1 / -1' } }}
            />

            <TextField
              fullWidth
              label="Qualification Notes"
              name="qualificationNotes"
              value={formData.qualificationNotes}
              onChange={handleInputChange}
              margin="normal"
              multiline
              rows={2}
              sx={{ gridColumn: { xs: '1', md: '1 / -1' } }}
            />
          </Box>
          <DialogError error={dialogApi.error} onRetry={() => dialogApi.clearError()} />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog} disabled={dialogApi.loading}>Cancel</Button>
          <ActionButton
            onClick={handleSaveOpportunity}
            loading={dialogApi.loading}
            variant="contained"
            color="primary"
          >
            {editingId ? 'Update' : 'Create'}
          </ActionButton>
        </DialogActions>
      </Dialog>

      {/* Bulk Update Dialog */}
      <Dialog open={bulkDialogOpen} onClose={() => !bulkApi.loading && setBulkDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Bulk Update {selectedIds.length} Opportunities</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Only fields with values will be updated. Leave fields empty to keep existing values.
          </Typography>
          
          <FormControl fullWidth margin="normal">
            <InputLabel>Stage</InputLabel>
            <Select
              name="stage"
              value={bulkFormData.stage}
              onChange={(e: SelectChangeEvent) => setBulkFormData(prev => ({ ...prev, stage: e.target.value }))}
              label="Stage"
            >
              <MenuItem value="">-- No Change --</MenuItem>
              {STAGES.map(s => (
                <MenuItem key={s.value} value={s.value}>{s.label}</MenuItem>
              ))}
            </Select>
          </FormControl>
          
          <FormControl fullWidth margin="normal">
            <InputLabel>Pricing Model</InputLabel>
            <Select
              name="pricingModel"
              value={bulkFormData.pricingModel}
              onChange={(e: SelectChangeEvent) => setBulkFormData(prev => ({ ...prev, pricingModel: e.target.value }))}
              label="Pricing Model"
            >
              <MenuItem value="">-- No Change --</MenuItem>
              {PRICING_MODELS.map(pm => (
                <MenuItem key={pm.value} value={pm.value}>{pm.label}</MenuItem>
              ))}
            </Select>
          </FormControl>
          
          <TextField
            fullWidth
            label="Probability (%)"
            name="probability"
            type="number"
            value={bulkFormData.probability}
            onChange={(e) => setBulkFormData(prev => ({ ...prev, probability: e.target.value }))}
            margin="normal"
            inputProps={{ min: 0, max: 100 }}
          />
          
          <TextField
            fullWidth
            label="Region"
            name="region"
            value={bulkFormData.region}
            onChange={(e) => setBulkFormData(prev => ({ ...prev, region: e.target.value }))}
            margin="normal"
          />
          
          <DialogError error={bulkApi.error} onRetry={() => bulkApi.clearError()} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setBulkDialogOpen(false)} disabled={bulkApi.loading}>Cancel</Button>
          <ActionButton
            onClick={handleBulkUpdate}
            loading={bulkApi.loading}
            variant="contained"
            color="primary"
          >
            Update Selected
          </ActionButton>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default OpportunitiesPage;

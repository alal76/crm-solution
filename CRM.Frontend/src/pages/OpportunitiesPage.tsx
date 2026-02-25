import { useState, useEffect, useMemo, useCallback } from 'react';
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
  TablePagination,
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
  Checkbox,
  Paper,
  Collapse,
  Stack,
  IconButton,
  SelectChangeEvent,
  Tabs,
  Tab,
  Grid,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Close as CloseIcon,
  Note as NoteIcon,
  Link as LinkIcon,
  ViewModule as ViewModuleIcon,
  TableChart as TableChartIcon,
} from '@mui/icons-material';
import PipelineKanban from '../components/sales/PipelineKanban';
import apiClient from '../services/apiClient';
import { getApiErrorMessage } from '../utils/errorHandler';
import logo from '../assets/logo.png';

import ImportExportButtons from '../components/ImportExportButtons';
import NotesTab from '../components/NotesTab';
import AdvancedSearch, { SearchField, SearchFilter, filterData } from '../components/AdvancedSearch';
import DynamicEntityForm, { ExtraTab } from '../components/DynamicEntityForm';
import { useAccountContext } from '../contexts/AccountContextProvider';
import { useProfile } from '../contexts/ProfileContext';
import { BaseEntity } from '../types';
import { Opportunity } from '../types/crm';
import {
  DialogError,
  DialogSuccess,
  ActionButton,
  DialogHeader,
  RelatedEntitiesPanel,
  EnhancedEmptyState,
  LoadingSkeleton,
} from '../components/common';
import { useApiState } from '../hooks/useApiState';
import { usePagination } from '../hooks/usePagination';
import { useEntityTypeSubscription } from '../hooks/useSignalR';
import logger from '../services/logger';

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
  const [dialogTab, setDialogTab] = useState(0);
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
  const [viewMode, setViewMode] = useState<'table' | 'kanban'>('table');

  // Dynamic field configuration
  // Field configuration now handled internally by DynamicEntityForm
  
  // Account contacts for the selected account (for Primary Contact dropdown)
  const [accountContacts, setAccountContacts] = useState<Array<{ id: number; firstName: string; lastName: string; role?: string }>>([]);
  const [loadingContacts, setLoadingContacts] = useState(false);
  
  // Multi-select and bulk update state
  const [selectedIds, setSelectedIds] = useState<number[]>([]);
  const [bulkDialogOpen, setBulkDialogOpen] = useState(false);
  const [bulkFormData, setBulkFormData] = useState({
    stage: '',
    probability: '',
    pricingModel: '',
    region: '' as string,
  });
  
  // API state for dialog operations
  const dialogApi = useApiState({ successTimeout: 3000 });
  const bulkApi = useApiState({ successTimeout: 3000 });

  // Get account context for filtering
  const { selectedAccounts, isContextActive, getAccountIds } = useAccountContext();
  const { hasPermission } = useProfile();

  // Fetch function (defined early for SignalR callbacks)
  const fetchAllData = useCallback(async () => {
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
  }, []);

  // SignalR subscription for real-time updates
  useEntityTypeSubscription('Opportunity', {
    onCreated: useCallback(() => {
      logger.debug('[SignalR] Opportunity created - refreshing list');
      fetchAllData();
    }, [fetchAllData]),
    onUpdated: useCallback(() => {
      logger.debug('[SignalR] Opportunity updated - refreshing list');
      fetchAllData();
    }, [fetchAllData]),
    onDeleted: useCallback(() => {
      logger.debug('[SignalR] Opportunity deleted - refreshing list');
      fetchAllData();
    }, [fetchAllData]),
  });

  const handleSearch = (filters: SearchFilter[], text: string) => {
    setSearchFilters(filters);
    setSearchText(text);
  };

  // Fetch contacts when customer is selected
  const fetchAccountContacts = useCallback(async (accountId: number) => {
    if (!accountId) {
      setAccountContacts([]);
      return;
    }
    try {
      setLoadingContacts(true);
      const response = await apiClient.get(`/accounts/${accountId}/contacts`);
      setAccountContacts(response.data.map((c: any) => ({
        id: c.contactId,
        firstName: c.contactName?.split(' ')[0] || 'Contact',
        lastName: c.contactName?.split(' ').slice(1).join(' ') || '',
        role: c.role,
      })));
    } catch (err) {
      console.error('Error fetching account contacts:', err);
      setAccountContacts([]);
    } finally {
      setLoadingContacts(false);
    }
  }, []);

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

  const {
    page,
    pageSize,
    paginatedData: paginatedOpportunities,
    handlePageChange,
    handlePageSizeChange,
    pageSizeOptions,
  } = usePagination(filteredOpportunities, { defaultPageSize: 25 });

  useEffect(() => {
    fetchAllData();
  }, [fetchAllData]);

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
      // Fetch contacts for the opportunity's customer
      if (opp.accountId) {
        fetchAccountContacts(opp.accountId);
      }
    } else {
      setEditingId(null);
      setAccountContacts([]);
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
      // Fetch contacts for default account if any
      if (accounts[0]?.id) {
        fetchAccountContacts(accounts[0].id);
      }
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
    setDialogTab(0);
  };

  const handleSelectChange = (e: any) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value === '' ? null : value,
      // Clear primary contact when customer changes
      ...(name === 'accountId' ? { primaryContactId: null } : {}),
    }));
    
    // Fetch contacts for the selected customer
    if (name === 'accountId' && value) {
      fetchAccountContacts(Number(value));
    }
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
      <Box sx={{ py: 4, px: 2 }}>
        <LoadingSkeleton rows={8} columns={5} ariaLabel="Loading opportunities…" />
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
          <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
            {/* View Mode Toggle */}
            <Box sx={{ display: 'flex', bgcolor: 'grey.100', borderRadius: 1, p: 0.5 }}>
              <IconButton
                size="small"
                onClick={() => setViewMode('table')}
                sx={{
                  bgcolor: viewMode === 'table' ? 'primary.main' : 'transparent',
                  color: viewMode === 'table' ? 'white' : 'text.secondary',
                  '&:hover': { bgcolor: viewMode === 'table' ? 'primary.dark' : 'grey.200' },
                }}
              >
                <TableChartIcon fontSize="small" />
              </IconButton>
              <IconButton
                size="small"
                onClick={() => setViewMode('kanban')}
                sx={{
                  bgcolor: viewMode === 'kanban' ? 'primary.main' : 'transparent',
                  color: viewMode === 'kanban' ? 'white' : 'text.secondary',
                  '&:hover': { bgcolor: viewMode === 'kanban' ? 'primary.dark' : 'grey.200' },
                }}
              >
                <ViewModuleIcon fontSize="small" />
              </IconButton>
            </Box>
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

        {/* Kanban View */}
        {viewMode === 'kanban' && (
          <Box sx={{ mb: 2 }}>
            <PipelineKanban
              opportunities={filteredOpportunities}
              stages={STAGES}
              onStageChange={async (opportunityId, newStage) => {
                await apiClient.put(`/opportunities/${opportunityId}`, { stage: newStage });
                fetchAllData();
              }}
              onEdit={handleOpenDialog}
              loading={loading}
            />
          </Box>
        )}

        {/* Table View */}
        {viewMode === 'table' && (
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
                {paginatedOpportunities.map((opp) => {
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
            <TablePagination
              component="div"
              count={filteredOpportunities.length}
              page={page}
              onPageChange={handlePageChange}
              rowsPerPage={pageSize}
              onRowsPerPageChange={handlePageSizeChange}
              rowsPerPageOptions={pageSizeOptions}
              showFirstButton
              showLastButton
            />
            {filteredOpportunities.length === 0 && (
              <EnhancedEmptyState
                illustration="opportunities"
                variant={searchText || searchFilters.length > 0 ? 'no-results' : 'no-data'}
                title={searchText || searchFilters.length > 0 ? 'No opportunities match your search' : undefined}
                primaryActionLabel={searchText || searchFilters.length > 0 ? 'Clear Filters' : 'Add Opportunity'}
                onPrimaryAction={() => {
                  if (searchText || searchFilters.length > 0) {
                    setSearchText('');
                    setSearchFilters([]);
                  } else {
                    handleOpenDialog();
                  }
                }}
                compact
              />
            )}
          </CardContent>
        </Card>
        )}
      </Container>

      {/* Add/Edit Opportunity Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogHeader
          mode={editingId ? 'edit' : 'create'}
          entityType="opportunity"
          entityName={editingId ? formData.name : undefined}
          entityId={editingId || undefined}
          onClose={handleCloseDialog}
          subtitle={formData.accountId ? `${formData.currency} ${formData.amount?.toLocaleString() || 0}` : undefined}
          status={STAGES.find(s => s.value === formData.stage)?.label}
          statusColor={STAGES.find(s => s.value === formData.stage)?.color}
        />
        <DialogContent sx={{ pt: 0 }}>
          <DialogError error={dialogApi.error} onRetry={() => dialogApi.clearError()} />

          <DynamicEntityForm
            moduleName="Opportunities"
            formData={formData}
            onChange={handleInputChange}
            onSelectChange={(e: any) => setFormData(prev => ({ ...prev, [e.target.name]: e.target.value }))}
            setFormData={setFormData}
            activeTab={dialogTab}
            editingId={editingId}
            onTabChange={setDialogTab}
            excludeFields={['tags', 'customFields']}
            extraTabs={[
              {
                index: 100,
                name: 'Related',
                icon: <LinkIcon fontSize="small" />,
                editOnly: true,
                render: () => (
                  <Box>
                    <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 2 }}>Related Records</Typography>
                    <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
                      View quotes, contacts, and activities linked to this opportunity.
                    </Typography>
                    <RelatedEntitiesPanel
                      entityType="opportunity"
                      entityId={editingId!}
                      showRelated={['contacts', 'quotes', 'activities']}
                      compact
                      showAddButtons
                    />
                  </Box>
                ),
              },
              {
                index: 101,
                name: 'Notes',
                icon: <NoteIcon fontSize="small" />,
                render: () => editingId ? (
                  <NotesTab entityType="Opportunity" entityId={editingId} entityName={formData.name || 'Opportunity'} />
                ) : (
                  <Alert severity="info" sx={{ mt: 2 }}>Please save the opportunity first to add notes.</Alert>
                ),
              },
            ]}
          />
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

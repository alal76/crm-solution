import { useState, useEffect, useCallback, useMemo } from 'react';
import {
  Box, Container, Typography, Card, CardContent, Table, TableBody, TableCell, TableContainer,
  TableHead, TableRow, CircularProgress, Alert, Button, Dialog, DialogTitle,
  DialogContent, DialogActions, Grid, Chip, Tabs, Tab, IconButton, Tooltip, Divider,
  List, ListItem, ListItemText, ListItemSecondaryAction, Autocomplete, TextField,
  FormControl, InputLabel, Select, MenuItem, Checkbox, FormControlLabel, Paper,
  SelectChangeEvent, Collapse, Stack, TablePagination
} from '@mui/material';
import { 
  TabPanel, 
  DialogError, 
  DialogSuccess, 
  ActionButton,
  DialogHeader,
  RelatedEntitiesPanel,
  EnhancedEmptyState,
} from '../components/common';
import {
  LIFECYCLE_STAGE_OPTIONS,
  ACCOUNT_TYPE_OPTIONS,
  PRIORITY_OPTIONS,
  CONTACT_ROLE_OPTIONS
} from '../utils/constants';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon,
  Business as BusinessIcon, Person as PersonIcon, Email as EmailIcon,
  Phone as PhoneIcon, PersonAdd as PersonAddIcon, Group as GroupIcon,
  ContactPhone as ContactPhoneIcon, Refresh as RefreshIcon,
  FilterAlt as FilterIcon, Close as CloseIcon, Note as NoteIcon,
  TrendingUp as TrendingUpIcon,
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import { getApiErrorMessage } from '../utils/errorHandler';
import DuplicateDetectionDialog from '../components/duplicates/DuplicateDetectionDialog';
import MergeDialog from '../components/duplicates/MergeDialog';
import { DuplicateCheckResult, scanForDuplicates, getPendingCandidates } from '../services/duplicateService';
import FieldRenderer from '../components/FieldRenderer';
import ImportExportButtons from '../components/ImportExportButtons';
import AdvancedSearch, { SearchField, SearchFilter, filterData } from '../components/AdvancedSearch';
import { ContactInfoPanel } from '../components/ContactInfo';
import NotesTab from '../components/NotesTab';
import { useFieldConfig, ModuleFieldConfiguration, dispatchFieldConfigUpdate } from '../hooks/useFieldConfig';
import { usePagination } from '../hooks/usePagination';
import { useAccountContext } from '../contexts/AccountContextProvider';
import { useProfile } from '../contexts/ProfileContext';
import { useApiState } from '../hooks/useApiState';
import { useEntityTypeSubscription } from '../hooks/useSignalR';
import logo from '../assets/logo.png';
import { BaseEntity } from '../types';
import logger from '../services/logger';

// Inline type matching MergeDialog's RecordData interface
interface MergeRecordData { id: number; displayName: string; data: Record<string, any>; }

// Search fields for Advanced Search
const SEARCH_FIELDS: SearchField[] = [
  { name: 'customerType', label: 'Account Type', type: 'select', options: [...ACCOUNT_TYPE_OPTIONS] },
  { name: 'firstName', label: 'First Name', type: 'text' },
  { name: 'lastName', label: 'Last Name', type: 'text' },
  { name: 'company', label: 'Business Name', type: 'text' },
  { name: 'email', label: 'Email', type: 'text' },
  { name: 'lifecycleStage', label: 'Status', type: 'select', options: [...LIFECYCLE_STAGE_OPTIONS] },
  { name: 'industry', label: 'Industry', type: 'text' },
  { name: 'city', label: 'City', type: 'text' },
];

const SEARCHABLE_FIELDS = ['firstName', 'lastName', 'company', 'email', 'industry', 'city', 'phone'];

// Use shared constants
const LIFECYCLE_STAGES = LIFECYCLE_STAGE_OPTIONS;
const ACCOUNT_TYPES = ACCOUNT_TYPE_OPTIONS;
const PRIORITIES = PRIORITY_OPTIONS;
const CONTACT_ROLES = CONTACT_ROLE_OPTIONS;

interface AccountContact extends BaseEntity {
  accountId: number;
  contactId: number;
  contactName: string;
  contactEmail?: string;
  contactPhone?: string;
  role: string;
  isPrimaryContact: boolean;
  isDecisionMaker: boolean;
  positionAtCustomer?: string;
}

interface Contact extends BaseEntity {
  firstName: string;
  lastName: string;
  emailPrimary?: string;
  phonePrimary?: string;
  company?: string;
  accountId?: number;
}

interface Account extends BaseEntity {
  category: number | string;
  isOrganization?: boolean;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  company: string;
  legalName?: string;
  jobTitle?: string;
  city: string;
  state: string;
  annualRevenue: number;
  customerType: number;
  priority: number;
  lifecycleStage: number;
  displayName: string;
  contactCount: number;
  [key: string]: any; // Allow dynamic fields
}

interface AccountForm {
  category: number;
  firstName: string;
  lastName: string;
  salutation: string;
  suffix: string;
  dateOfBirth: string;
  gender: string;
  company: string;
  legalName: string;
  dbaName: string;
  taxId: string;
  registrationNumber: string;
  yearFounded: number | null;
  email: string;
  secondaryEmail: string;
  phone: string;
  mobilePhone: string;
  jobTitle: string;
  website: string;
  address: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  industry: string;
  numberOfEmployees: number;
  annualRevenue: number;
  customerType: number;
  priority: number;
  lifecycleStage: number;
  leadSource: string;
  leadScore: number;
  creditLimit: number;
  paymentTerms: string;
  linkedInUrl: string;
  twitterHandle: string;
  optInEmail: boolean;
  optInSms: boolean;
  optInPhone: boolean;
  preferredContactMethod: string;
  timezone: string;
  territory: string;
  tags: string;
  notes: string;
  description: string;
  [key: string]: any; // Allow dynamic fields
}

const INITIAL_FORM_DATA: AccountForm = {
  category: 1,
  firstName: '',
  lastName: '',
  salutation: '',
  suffix: '',
  dateOfBirth: '',
  gender: '',
  company: '',
  legalName: '',
  dbaName: '',
  taxId: '',
  registrationNumber: '',
  yearFounded: null,
  email: '',
  secondaryEmail: '',
  phone: '',
  mobilePhone: '',
  jobTitle: '',
  website: '',
  address: '',
  city: '',
  state: '',
  zipCode: '',
  country: 'USA',
  industry: '',
  numberOfEmployees: 0,
  annualRevenue: 0,
  customerType: 0,
  priority: 1,
  lifecycleStage: 0,
  leadSource: '',
  leadScore: 0,
  creditLimit: 0,
  paymentTerms: 'Net 30',
  linkedInUrl: '',
  twitterHandle: '',
  optInEmail: true,
  optInSms: false,
  optInPhone: true,
  preferredContactMethod: 'Email',
  timezone: '',
  territory: '',
  tags: '',
  notes: '',
  description: '',
};

function AccountsPage() {
  // Data state
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Dialog state
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  const [formData, setFormData] = useState<AccountForm>(INITIAL_FORM_DATA);
  const [dialogError, setDialogError] = useState<string | null>(null);

  // Contact linking state
  const [accountContacts, setAccountContacts] = useState<AccountContact[]>([]);
  const [addContactDialogOpen, setAddContactDialogOpen] = useState(false);
  const [selectedContactId, setSelectedContactId] = useState<number | null>(null);
  const [contactRole, setContactRole] = useState<number>(0);
  const [contactIsPrimary, setContactIsPrimary] = useState(false);
  const [contactIsDecisionMaker, setContactIsDecisionMaker] = useState(false);

  // Search state
  const [searchFilters, setSearchFilters] = useState<SearchFilter[]>([]);
  const [searchText, setSearchText] = useState('');
  
  // Multi-select and bulk update state
  const [selectedIds, setSelectedIds] = useState<number[]>([]);
  const [bulkDialogOpen, setBulkDialogOpen] = useState(false);
  const [bulkFormData, setBulkFormData] = useState({
    customerType: '',
    lifecycleStage: '',
    priority: '',
    industry: '' as string,
    territory: '' as string,
  });
  
  // Duplicate detection & merge state
  const [duplicateDialogOpen, setDuplicateDialogOpen] = useState(false);
  const [duplicateCheckResult, setDuplicateCheckResult] = useState<DuplicateCheckResult | null>(null);
  const [duplicateLoading, setDuplicateLoading] = useState(false);
  const [mergeDialogOpen, setMergeDialogOpen] = useState(false);

  // API state for dialog operations
  const dialogApi = useApiState({ successTimeout: 3000 });
  const bulkApi = useApiState({ successTimeout: 3000 });
  // Use the field configuration hook - this will automatically refresh when configs change
  const { 
    fieldConfigs, 
    tabs, 
    loading: fieldConfigsLoading, 
    error: fieldConfigError,
    refresh: refreshFieldConfigs,
    getTabFields,
    isFieldVisible 
  } = useFieldConfig('Accounts');

  // Get account context for filtering
  const { selectedAccounts, isContextActive, getAccountIds } = useAccountContext();
  const { hasPermission } = useProfile();

  // Fetch functions (defined early for SignalR callbacks)
  const fetchAccounts = useCallback(async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/accounts');
      setAccounts(response.data);
      setError(null);
    } catch (err: any) {
      setError(getApiErrorMessage(err, 'Failed to fetch accounts'));
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchContacts = useCallback(async () => {
    try {
      const response = await apiClient.get('/contacts');
      setContacts(response.data);
    } catch (err) {
      console.error('Error fetching contacts:', err);
    }
  }, []);

  // SignalR subscription for real-time updates
  useEntityTypeSubscription('Account', {
    onCreated: useCallback(() => {
      logger.debug('[SignalR] Account created - refreshing list');
      fetchAccounts();
    }, [fetchAccounts]),
    onUpdated: useCallback(() => {
      logger.debug('[SignalR] Account updated - refreshing list');
      fetchAccounts();
    }, [fetchAccounts]),
    onDeleted: useCallback(() => {
      logger.debug('[SignalR] Account deleted - refreshing list');
      fetchAccounts();
    }, [fetchAccounts]),
  });

  // Filter accounts based on search AND account context
  const filteredAccounts = useMemo(() => {
    let result = accounts;
    
    // Apply account context filter first
    if (isContextActive) {
      const accountIds = getAccountIds();
      result = result.filter(account => accountIds.includes(account.id!));
    }
    
    // Then apply search filters
    return filterData(result, searchFilters, searchText, SEARCHABLE_FIELDS);
  }, [accounts, searchFilters, searchText, isContextActive, getAccountIds]);

  // Pagination - applies to filtered results
  const {
    page,
    pageSize,
    paginatedData: paginatedAccounts,
    handlePageChange,
    handlePageSizeChange,
    pageSizeOptions,
  } = usePagination(filteredAccounts, { defaultPageSize: 25 });

  // Fetch data on mount
  useEffect(() => {
    fetchAccounts();
    fetchContacts();
  }, [fetchAccounts, fetchContacts]);

  const fetchAccountContacts = async (accountId: number) => {
    try {
      const response = await apiClient.get(`/accounts/${accountId}/contacts`);
      setAccountContacts(response.data);
    } catch (err) {
      console.error('Error fetching account contacts:', err);
      setAccountContacts([]);
    }
  };

  // Form handlers
  const handleInputChange = useCallback((e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    const checked = (e.target as HTMLInputElement).checked;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : type === 'number' ? parseFloat(value) || 0 : value,
    }));
  }, []);

  const handleSelectChange = useCallback((e: SelectChangeEvent<string | number>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  }, []);

  // Dialog handlers
  const handleOpenDialog = useCallback((account?: Account) => {
    setDialogTab(0);
    if (account) {
      setEditingId(account.id);
      // Map account data to form
      const formValues: AccountForm = { ...INITIAL_FORM_DATA };
      Object.keys(account).forEach(key => {
        if (key in formValues) {
          (formValues as any)[key] = account[key] ?? (INITIAL_FORM_DATA as any)[key];
        }
      });
      // Normalize category to number for form if it's a string
      if (typeof account.category === 'string') {
        formValues.category = account.category === 'Organization' ? 1 : 0;
      }
      setFormData(formValues);
      // Fetch linked contacts for all accounts
      fetchAccountContacts(account.id);
    } else {
      setEditingId(null);
      setAccountContacts([]);
      setFormData(INITIAL_FORM_DATA);
    }
    setOpenDialog(true);
  }, []);

  const handleCloseDialog = useCallback(() => {
    setOpenDialog(false);
    setEditingId(null);
    setDialogError(null);
  }, []);

  // Validation using field configurations
  const validateRequiredFields = useCallback(() => {
    if (fieldConfigsLoading) return true;
    if (!fieldConfigs.length) return true;

    const visibleRequired = fieldConfigs.filter(cfg => 
      cfg.isRequired && isFieldVisible(cfg, formData)
    );
    
    const missing = visibleRequired.filter(cfg => {
      const value = formData[cfg.fieldName];
      if (typeof value === 'boolean') return false;
      if (value === null || value === undefined) return true;
      if (typeof value === 'string') return value.trim() === '';
      return false;
    });

    if (missing.length) {
      setDialogError(`Please fill in required fields: ${missing.map(m => m.fieldLabel).join(', ')}`);
      return false;
    }

    return true;
  }, [fieldConfigs, fieldConfigsLoading, formData, isFieldVisible]);

  const handleSaveAccount = async () => {
    setDialogError(null);

    if (!validateRequiredFields()) return;

    try {
      // Transform form data before sending - convert empty strings to null for date fields
      const submitData = { ...formData };
      const dateFields = ['dateOfBirth', 'firstContactDate', 'conversionDate', 'lastActivityDate', 'nextFollowUpDate'];
      dateFields.forEach(field => {
        if (submitData[field] === '' || submitData[field] === undefined) {
          submitData[field] = null;
        }
      });

      if (editingId) {
        await apiClient.put(`/accounts/${editingId}`, submitData);
        setSuccessMessage('Account updated successfully');
      } else {
        await apiClient.post('/accounts', submitData);
        setSuccessMessage('Account created successfully');
      }
      handleCloseDialog();
      fetchAccounts();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setDialogError(getApiErrorMessage(err, 'Failed to save account'));
    }
  };

  const handleDeleteAccount = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this account?')) {
      const result = await dialogApi.execute(async () => {
        await apiClient.delete(`/accounts/${id}`);
        return true;
      }, 'Account deleted successfully');
      
      if (result) {
        setSelectedIds(prev => prev.filter(sid => sid !== id));
        fetchAccounts();
        setSuccessMessage('Account deleted successfully');
        setTimeout(() => setSuccessMessage(null), 3000);
      } else {
        setError(getApiErrorMessage(dialogApi.error, 'Failed to delete account'));
      }
    }
  };

  // Multi-select handlers
  const handleSelectAll = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.checked) {
      setSelectedIds(filteredCustomers.map(c => c.id));
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
      customerType: '',
      lifecycleStage: '',
      priority: '',
      industry: '',
      territory: '',
    });
    bulkApi.clearError();
    setBulkDialogOpen(true);
  };

  const handleBulkUpdate = async () => {
    if (selectedIds.length === 0) {
      bulkApi.setError('No accounts selected');
      return;
    }

    // Build update payload only with non-empty fields
    const updatePayload: Record<string, any> = {};
    if (bulkFormData.customerType !== '') updatePayload.customerType = Number(bulkFormData.customerType);
    if (bulkFormData.lifecycleStage !== '') updatePayload.lifecycleStage = Number(bulkFormData.lifecycleStage);
    if (bulkFormData.priority !== '') updatePayload.priority = Number(bulkFormData.priority);
    if (bulkFormData.industry) updatePayload.industry = bulkFormData.industry;
    if (bulkFormData.territory) updatePayload.territory = bulkFormData.territory;

    if (Object.keys(updatePayload).length === 0) {
      bulkApi.setError('Please select at least one field to update');
      return;
    }

    const result = await bulkApi.execute(async () => {
      const updatePromises = selectedIds.map(id =>
        apiClient.put(`/accounts/${id}`, updatePayload)
      );
      await Promise.all(updatePromises);
      return selectedIds.length;
    }, `Successfully updated ${selectedIds.length} account(s)`);

    if (result) {
      fetchAccounts();
      setBulkDialogOpen(false);
      setSelectedIds([]);
      setSuccessMessage(`Successfully updated ${result} account(s)`);
      setTimeout(() => setSuccessMessage(null), 3000);
    }
  };

  const handleBulkDelete = async () => {
    if (selectedIds.length === 0) return;
    
    if (!window.confirm(`Are you sure you want to delete ${selectedIds.length} account(s)?`)) {
      return;
    }

    const result = await bulkApi.execute(async () => {
      const deletePromises = selectedIds.map(id => apiClient.delete(`/accounts/${id}`));
      await Promise.all(deletePromises);
      return selectedIds.length;
    }, `Successfully deleted ${selectedIds.length} account(s)`);

    if (result) {
      fetchAccounts();
      setSelectedIds([]);
      setSuccessMessage(`Successfully deleted ${result} account(s)`);
      setTimeout(() => setSuccessMessage(null), 3000);
    } else {
      setError(bulkApi.error?.message || 'Failed to delete some accounts');
    }
  };

  // Contact linking handlers
  const handleAddContact = async () => {
    if (!editingId || !selectedContactId) return;

    try {
      await apiClient.post(`/accounts/${editingId}/contacts`, {
        contactId: selectedContactId,
        role: contactRole,
        isPrimaryContact: contactIsPrimary,
        isDecisionMaker: contactIsDecisionMaker,
      });
      fetchCustomerContacts(editingId);
      setAddContactDialogOpen(false);
      setSelectedContactId(null);
      setContactRole(0);
      setContactIsPrimary(false);
      setContactIsDecisionMaker(false);
      setSuccessMessage('Contact linked successfully');
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(getApiErrorMessage(err, 'Failed to link contact'));
    }
  };

  const handleRemoveContact = async (contactId: number) => {
    if (!editingId) return;

    if (window.confirm('Are you sure you want to remove this contact from the account?')) {
      try {
        await apiClient.delete(`/accounts/${editingId}/contacts/${contactId}`);
        fetchCustomerContacts(editingId);
        setSuccessMessage('Contact removed successfully');
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(getApiErrorMessage(err, 'Failed to remove contact'));
      }
    }
  };

  // Helper functions
  const getLifecycleStage = (value: number) => LIFECYCLE_STAGES.find(s => s.value === value);
  const getPriority = (value: number) => PRIORITIES.find(p => p.value === value);
  const getCustomerType = (value: number) => ACCOUNT_TYPES.find(t => t.value === value);

  const handleSearch = (filters: SearchFilter[], text: string) => {
    setSearchFilters(filters);
    setSearchText(text);
  };

  // Render fields for a tab based on field configurations
  const renderTabFields = (tabIndex: number) => {
    const tabFields = getTabFields(tabIndex, formData.category, formData);

    if (!tabFields.length) {
      return (
        <Box sx={{ py: 4, textAlign: 'center' }}>
          <Typography color="textSecondary">No fields configured for this tab</Typography>
        </Box>
      );
    }

    return (
      <Grid container spacing={2}>
        {tabFields.map(config => (
          <Grid key={config.id} item xs={12} sm={config.gridSize || 12}>
            <FieldRenderer
              config={config}
              formData={formData}
              onChange={handleInputChange}
              onSelectChange={handleSelectChange}
              setFormData={setFormData}
              disabled={config.fieldName === 'category' && !!editingId}
            />
          </Grid>
        ))}
      </Grid>
    );
  };

  // Calculate which tabs to show
  const getVisibleTabs = () => {
    const baseTabs = tabs.map(t => ({ index: t.index, name: t.name }));
    
    // Add Contact Info tab when editing
    if (editingId) {
      baseTabs.push({ index: 100, name: 'Contact Info' });
    }

    // Add Linked Contacts tab when editing (all accounts can have linked contacts)
    if (editingId) {
      baseTabs.push({ index: 101, name: 'Linked Contacts' });
    }

    // Add Related tab when editing (show related opportunities, service requests, etc.)
    if (editingId) {
      baseTabs.push({ index: 103, name: 'Related' });
    }

    // Add Notes tab when editing
    if (editingId) {
      baseTabs.push({ index: 102, name: 'Notes' });
    }

    return baseTabs;
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
        <CircularProgress />
      </Box>
    );
  }

  const visibleTabs = getVisibleTabs();

  return (
    <Box sx={{ py: 4 }}>
      <Container maxWidth="xl">
        {/* Header */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
              <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
            </Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>Accounts</Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
            <Tooltip title="Refresh field configurations">
              <IconButton 
                onClick={() => refreshFieldConfigs()} 
                size="small"
                sx={{ color: '#6750A4' }}
              >
                <RefreshIcon />
              </IconButton>
            </Tooltip>
            <ImportExportButtons
              entityType="accounts"
              entityLabel="Accounts"
              onImportComplete={fetchAccounts}
            />
            <Button
              variant="outlined"
              size="small"
              disabled={duplicateLoading}
              onClick={async () => {
                setDuplicateLoading(true);
                try {
                  await scanForDuplicates('Account');
                  const candidates = await getPendingCandidates('Account', 1, 50);
                  const hasDuplicates = candidates.length > 0;
                  const highestScore = candidates.reduce((max, c) => Math.max(max, c.matchScore), 0);
                  const checkResult: DuplicateCheckResult = {
                    hasDuplicates,
                    highConfidenceMatch: highestScore >= 80,
                    matches: candidates,
                    totalMatchCount: candidates.length,
                    highestMatchScore: highestScore,
                    recommendation: hasDuplicates ? 'ReviewMatches' : 'CreateNew',
                  };
                  setDuplicateCheckResult(checkResult);
                  setDuplicateDialogOpen(true);
                } catch (err) {
                  setError('Failed to scan for duplicates');
                } finally {
                  setDuplicateLoading(false);
                }
              }}
              sx={{ borderColor: '#6750A4', color: '#6750A4' }}
            >
              {duplicateLoading ? 'Scanning...' : 'Scan Duplicates'}
            </Button>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={() => handleOpenDialog()}
              sx={{ backgroundColor: '#6750A4' }}
            >
              Add Account
            </Button>
          </Box>
        </Box>

        {/* Alerts */}
        {error && (
          <Alert 
            severity="error" 
            sx={{ mb: 2, whiteSpace: 'pre-line' }} 
            onClose={() => setError(null)}
          >
            {error}
          </Alert>
        )}
        {fieldConfigError && <Alert severity="warning" sx={{ mb: 2 }}>Field configurations could not be loaded. Using defaults.</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccessMessage(null)}>{successMessage}</Alert>}

        {/* Search */}
        <AdvancedSearch
          fields={SEARCH_FIELDS}
          onSearch={handleSearch}
          placeholder="Search accounts by name, email, company..."
        />

        {/* Bulk Actions Toolbar */}
        <Collapse in={selectedIds.length > 0}>
          <Paper sx={{ mb: 2, p: 2, backgroundColor: 'primary.light' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <Typography sx={{ color: 'primary.contrastText' }}>
                {selectedIds.length} account(s) selected
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
                {selectedIds.length >= 2 && (
                  <Button
                    variant="contained"
                    size="small"
                    onClick={() => setMergeDialogOpen(true)}
                    sx={{ backgroundColor: 'white', color: 'primary.main', '&:hover': { backgroundColor: 'grey.100' } }}
                  >
                    Merge Selected
                  </Button>
                )}
                {hasPermission('canDeleteCustomers') && (
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

        {/* Account List */}
        <Card>
          <CardContent sx={{ p: 0 }}>
            <TableContainer sx={{ overflowX: 'auto' }}>
              <Table sx={{ minWidth: 950 }}>
                <TableHead>
                  <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                    <TableCell padding="checkbox">
                      <Checkbox
                        indeterminate={selectedIds.length > 0 && selectedIds.length < filteredAccounts.length}
                        checked={filteredAccounts.length > 0 && selectedIds.length === filteredAccounts.length}
                        onChange={handleSelectAll}
                      />
                    </TableCell>
                    <TableCell><strong>Name</strong></TableCell>
                    <TableCell><strong>Contact</strong></TableCell>
                    <TableCell><strong>Type</strong></TableCell>
                    <TableCell><strong>Stage</strong></TableCell>
                    <TableCell><strong>Priority</strong></TableCell>
                    <TableCell><strong>Revenue</strong></TableCell>
                    <TableCell><strong>Contacts</strong></TableCell>
                    <TableCell align="center"><strong>Actions</strong></TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {paginatedAccounts.map((account) => {
                    const stage = getLifecycleStage(account.lifecycleStage);
                    const priority = getPriority(account.priority);
                    const type = getCustomerType(account.customerType);
                    return (
                      <TableRow key={account.id} hover selected={selectedIds.includes(account.id)}>
                        <TableCell padding="checkbox">
                          <Checkbox
                            checked={selectedIds.includes(account.id)}
                            onChange={() => handleSelectOne(account.id)}
                          />
                        </TableCell>
                        <TableCell>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <BusinessIcon fontSize="small" sx={{ color: '#6750A4' }} />
                            <Box>
                              <Typography fontWeight={500}>
                                {account.displayName || account.company || `${account.firstName} ${account.lastName}`}
                              </Typography>
                              {account.legalName && (
                                <Typography variant="caption" color="textSecondary">{account.legalName}</Typography>
                              )}
                            </Box>
                          </Box>
                        </TableCell>
                        <TableCell>
                          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                              <EmailIcon fontSize="small" sx={{ color: '#666', fontSize: 14 }} />
                              <Typography variant="body2">{account.email}</Typography>
                            </Box>
                            {account.phone && (
                              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                                <PhoneIcon fontSize="small" sx={{ color: '#666', fontSize: 14 }} />
                                <Typography variant="body2">{account.phone}</Typography>
                              </Box>
                            )}
                          </Box>
                        </TableCell>
                        <TableCell>
                          <Chip label={type?.label || 'Unknown'} size="small" variant="outlined" />
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={stage?.label || 'Unknown'}
                            size="small"
                            sx={{ backgroundColor: stage?.color, color: 'white' }}
                          />
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={priority?.label || 'Medium'}
                            size="small"
                            sx={{ backgroundColor: priority?.color, color: 'white' }}
                          />
                        </TableCell>
                        <TableCell>
                          ${account.annualRevenue?.toLocaleString() || 0}
                        </TableCell>
                        <TableCell>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <GroupIcon fontSize="small" sx={{ color: '#666' }} />
                            <Typography variant="body2">{account.contactCount || 0}</Typography>
                          </Box>
                        </TableCell>
                        <TableCell align="center">
                          <Tooltip title="Edit">
                            <IconButton size="small" onClick={() => handleOpenDialog(account)} sx={{ color: '#6750A4' }}>
                              <EditIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete">
                            <IconButton size="small" onClick={() => handleDeleteAccount(account.id)} sx={{ color: '#f44336' }}>
                              <DeleteIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </TableContainer>
            <TablePagination
              component="div"
              count={filteredAccounts.length}
              page={page}
              onPageChange={handlePageChange}
              rowsPerPage={pageSize}
              onRowsPerPageChange={handlePageSizeChange}
              rowsPerPageOptions={pageSizeOptions}
              showFirstButton
              showLastButton
            />
            {accounts.length === 0 && (
              <EnhancedEmptyState
                illustration="accounts"
                title={searchFilters.length > 0 || searchText ? "No accounts match your search" : "No accounts yet"}
                description={searchFilters.length > 0 || searchText 
                  ? "Try adjusting your filters or search terms to find what you're looking for"
                  : "Get started by adding your first account to the CRM"
                }
                variant={searchFilters.length > 0 || searchText ? "no-results" : "no-data"}
                primaryActionLabel="Add Account"
                onPrimaryAction={() => handleOpenDialog()}
                secondaryActionLabel={searchFilters.length > 0 ? "Clear Filters" : undefined}
                onSecondaryAction={searchFilters.length > 0 ? () => setSearchFilters([]) : undefined}
              />
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Add/Edit Account Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogHeader
          mode={editingId ? 'edit' : 'create'}
          entityType="account"
          entityName={editingId ? (formData.company || `${formData.firstName || ''} ${formData.lastName || ''}`.trim() || undefined) : undefined}
          entityId={editingId || undefined}
          onClose={handleCloseDialog}
          subtitle={editingId ? formData.emailPrimary || undefined : undefined}
          status={editingId && formData.lifecycleStage ? 
            (LIFECYCLE_STAGE_OPTIONS.find(s => s.value === formData.lifecycleStage)?.label) : undefined}
          statusColor={editingId && formData.lifecycleStage !== undefined ? (
            formData.lifecycleStage === 3 ? 'success' : // Active
            formData.lifecycleStage === 1 ? 'info' : // Lead
            formData.lifecycleStage === 2 ? 'warning' : // Opportunity
            formData.lifecycleStage === 4 ? 'error' : 'default' // At Risk or other
          ) : undefined}
        />
        <Box sx={{ borderBottom: 1, borderColor: 'divider', px: 3 }}>
          <Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)} variant="scrollable" scrollButtons="auto" aria-label="Account dialog tabs">
            {visibleTabs.map((tab, idx) => (
              <Tab 
                key={tab.index} 
                label={tab.name}
                id={`account-tab-${idx}`}
                aria-controls={`account-tabpanel-${idx}`}
                icon={
                  tab.index === 100 ? <ContactPhoneIcon fontSize="small" /> : 
                  tab.index === 101 ? <GroupIcon fontSize="small" /> : 
                  tab.index === 102 ? <NoteIcon fontSize="small" /> : 
                  tab.index === 103 ? <TrendingUpIcon fontSize="small" /> :
                  undefined
                }
                iconPosition="start"
              />
            ))}
          </Tabs>
        </Box>
        <DialogContent sx={{ pt: 2, minHeight: 400 }}>
          <DialogError error={dialogError} onClose={() => setDialogError(null)} />
          {fieldConfigsLoading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
              <CircularProgress />
            </Box>
          ) : (
            <>
              {/* Render field configuration tabs */}
              {tabs.map((tab, idx) => (
                <TabPanel key={tab.index} value={dialogTab} index={idx}>
                  {renderTabFields(tab.index)}
                </TabPanel>
              ))}

              {/* Contact Info Tab */}
              {editingId && (
                <TabPanel value={dialogTab} index={visibleTabs.findIndex(t => t.index === 100)}>
                  <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 2 }}>
                    Manage Contact Information
                  </Typography>
                  <ContactInfoPanel
                    entityType="Account"
                    entityId={editingId}
                    layout="tabs"
                    showCounts={true}
                  />
                </TabPanel>
              )}

              {/* Linked Contacts Tab - available for all accounts */}
              {editingId && (
                <TabPanel value={dialogTab} index={visibleTabs.findIndex(t => t.index === 101)}>
                  <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Typography variant="subtitle1" fontWeight={600}>
                      Linked Contacts ({customerContacts.length})
                    </Typography>
                    <Button
                      variant="outlined"
                      startIcon={<PersonAddIcon />}
                      onClick={() => setAddContactDialogOpen(true)}
                      size="small"
                    >
                      Add Contact
                    </Button>
                  </Box>

                  {customerContacts.length === 0 ? (
                    <Paper elevation={0} sx={{ p: 4, textAlign: 'center', backgroundColor: '#F5EFF7', borderRadius: 2 }}>
                      <GroupIcon sx={{ fontSize: 48, color: '#6750A4', opacity: 0.5, mb: 1 }} />
                      <Typography color="textSecondary">No contacts linked to this account</Typography>
                    </Paper>
                  ) : (
                    <List sx={{ bgcolor: 'background.paper', borderRadius: 1, border: '1px solid #e0e0e0' }}>
                      {customerContacts.map((contact, index) => (
                        <Box key={contact.id}>
                          {index > 0 && <Divider />}
                          <ListItem>
                            <ListItemText
                              primary={
                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                  <Typography fontWeight={500}>{contact.contactName}</Typography>
                                  <Chip
                                    label={CONTACT_ROLES.find(r => r.value === parseInt(contact.role))?.label || contact.role}
                                    size="small"
                                    color="primary"
                                    variant="outlined"
                                  />
                                  {contact.isPrimaryContact && <Chip label="Primary" size="small" color="success" />}
                                  {contact.isDecisionMaker && <Chip label="Decision Maker" size="small" color="warning" />}
                                </Box>
                              }
                              secondary={
                                <Box sx={{ display: 'flex', gap: 2, mt: 0.5 }}>
                                  {contact.contactEmail && (
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                                      <EmailIcon fontSize="small" sx={{ fontSize: 14, color: '#666' }} />
                                      <Typography variant="caption">{contact.contactEmail}</Typography>
                                    </Box>
                                  )}
                                  {contact.contactPhone && (
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                                      <PhoneIcon fontSize="small" sx={{ fontSize: 14, color: '#666' }} />
                                      <Typography variant="caption">{contact.contactPhone}</Typography>
                                    </Box>
                                  )}
                                </Box>
                              }
                            />
                            <ListItemSecondaryAction>
                              <IconButton edge="end" onClick={() => handleRemoveContact(contact.contactId)} size="small" color="error">
                                <DeleteIcon fontSize="small" />
                              </IconButton>
                            </ListItemSecondaryAction>
                          </ListItem>
                        </Box>
                      ))}
                    </List>
                  )}
                </TabPanel>
              )}

              {/* Related Entities Tab */}
              {editingId && (
                <TabPanel value={dialogTab} index={visibleTabs.findIndex(t => t.index === 103)}>
                  <RelatedEntitiesPanel
                    entityType="accounts"
                    entityId={editingId}
                    showRelated={['contacts', 'opportunities', 'serviceRequests', 'quotes', 'contracts']}
                    onEntityClick={(type, id) => {
                      // Close dialog and navigate - in real app would use router
                      handleCloseDialog();
                      logger.debug(`Navigate to ${type} ${id}`);
                    }}
                  />
                </TabPanel>
              )}

              {/* Notes Tab */}
              {editingId && (
                <TabPanel value={dialogTab} index={visibleTabs.findIndex(t => t.index === 102)}>
                  <NotesTab
                    entityType="Account"
                    entityId={editingId}
                    entityName={formData.company || `${formData.firstName} ${formData.lastName}`}
                  />
                </TabPanel>
              )}
            </>
          )}
        </DialogContent>
        <DialogActions>
          <DialogError error={dialogApi.error} />
          <DialogSuccess message={dialogApi.success} />
          <Button onClick={handleCloseDialog} disabled={dialogApi.loading}>Cancel</Button>
          <ActionButton
            label={editingId ? 'Update' : 'Create'}
            loading={dialogApi.loading}
            onClick={handleSaveCustomer}
            color="primary"
          />
        </DialogActions>
      </Dialog>

      {/* Add Contact Dialog */}
      <Dialog open={addContactDialogOpen} onClose={() => setAddContactDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Link Contact to Account</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <Autocomplete
                options={contacts.filter(c => !customerContacts.some(cc => cc.contactId === c.id))}
                getOptionLabel={(option) => `${option.firstName} ${option.lastName}${option.company ? ` (${option.company})` : ''}`}
                value={contacts.find(c => c.id === selectedContactId) || null}
                onChange={(_, newValue) => setSelectedContactId(newValue?.id || null)}
                renderInput={(params) => (
                  <TextField {...params} label="Select Contact" required />
                )}
                renderOption={(props, option) => (
                  <li {...props}>
                    <Box>
                      <Typography>{option.firstName} {option.lastName}</Typography>
                      <Typography variant="caption" color="textSecondary">
                        {option.emailPrimary} {option.company && `• ${option.company}`}
                      </Typography>
                    </Box>
                  </li>
                )}
              />
            </Grid>
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>Role</InputLabel>
                <Select value={contactRole} onChange={(e) => setContactRole(e.target.value as number)} label="Role">
                  {CONTACT_ROLES.map(r => (
                    <MenuItem key={r.value} value={r.value}>{r.label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={6}>
              <FormControlLabel
                control={<Checkbox checked={contactIsPrimary} onChange={(e) => setContactIsPrimary(e.target.checked)} />}
                label="Primary Contact"
              />
            </Grid>
            <Grid item xs={6}>
              <FormControlLabel
                control={<Checkbox checked={contactIsDecisionMaker} onChange={(e) => setContactIsDecisionMaker(e.target.checked)} />}
                label="Decision Maker"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAddContactDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleAddContact} variant="contained" disabled={!selectedContactId} sx={{ backgroundColor: '#6750A4' }}>
            Add Contact
          </Button>
        </DialogActions>
      </Dialog>

      {/* Bulk Update Dialog */}
      <Dialog open={bulkDialogOpen} onClose={() => { bulkApi.clearError(); setBulkDialogOpen(false); }} maxWidth="sm" fullWidth>
        <DialogTitle>
          Bulk Update {selectedIds.length} Account(s)
        </DialogTitle>
        <DialogContent>
          <DialogError 
            error={bulkApi.error} 
            onClose={bulkApi.clearError}
          />
          <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
            Only fields with values will be updated. Leave fields empty to keep current values.
          </Typography>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <FormControl fullWidth size="small">
              <InputLabel>Account Type</InputLabel>
              <Select
                value={bulkFormData.customerType}
                label="Account Type"
                onChange={(e: SelectChangeEvent) => setBulkFormData(prev => ({ ...prev, customerType: e.target.value }))}
              >
                <MenuItem value="">-- No Change --</MenuItem>
                {ACCOUNT_TYPE_OPTIONS.map(type => (
                  <MenuItem key={type.value} value={type.value}>{type.label}</MenuItem>
                ))}
              </Select>
            </FormControl>
            
            <FormControl fullWidth size="small">
              <InputLabel>Lifecycle Stage</InputLabel>
              <Select
                value={bulkFormData.lifecycleStage}
                label="Lifecycle Stage"
                onChange={(e: SelectChangeEvent) => setBulkFormData(prev => ({ ...prev, lifecycleStage: e.target.value }))}
              >
                <MenuItem value="">-- No Change --</MenuItem>
                {LIFECYCLE_STAGE_OPTIONS.map(stage => (
                  <MenuItem key={stage.value} value={stage.value}>{stage.label}</MenuItem>
                ))}
              </Select>
            </FormControl>
            
            <FormControl fullWidth size="small">
              <InputLabel>Priority</InputLabel>
              <Select
                value={bulkFormData.priority}
                label="Priority"
                onChange={(e: SelectChangeEvent) => setBulkFormData(prev => ({ ...prev, priority: e.target.value }))}
              >
                <MenuItem value="">-- No Change --</MenuItem>
                {PRIORITY_OPTIONS.map(priority => (
                  <MenuItem key={priority.value} value={priority.value}>{priority.label}</MenuItem>
                ))}
              </Select>
            </FormControl>
            
            <TextField
              label="Industry"
              size="small"
              value={bulkFormData.industry}
              onChange={(e) => setBulkFormData(prev => ({ ...prev, industry: e.target.value }))}
              placeholder="Leave empty to keep current value"
              fullWidth
            />
            
            <TextField
              label="Territory"
              size="small"
              value={bulkFormData.territory}
              onChange={(e) => setBulkFormData(prev => ({ ...prev, territory: e.target.value }))}
              placeholder="Leave empty to keep current value"
              fullWidth
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => { bulkApi.clearError(); setBulkDialogOpen(false); }}>Cancel</Button>
          <ActionButton
            label="Update All"
            loading={bulkApi.loading}
            onClick={handleBulkUpdate}
            color="primary"
          />
        </DialogActions>
      </Dialog>

      {/* Duplicate Detection Dialog */}
      <DuplicateDetectionDialog
        open={duplicateDialogOpen}
        onClose={() => setDuplicateDialogOpen(false)}
        checkResult={duplicateCheckResult}
        isLoading={duplicateLoading}
        entityType="Account"
        onCreateNew={() => {
          setDuplicateDialogOpen(false);
          handleOpenDialog();
        }}
        onUpdateExisting={(recordId) => {
          setDuplicateDialogOpen(false);
          const customer = customers.find(c => c.id === recordId);
          if (customer) handleOpenDialog(customer);
        }}
        onViewRecord={(recordId) => {
          setDuplicateDialogOpen(false);
          const customer = customers.find(c => c.id === recordId);
          if (customer) handleOpenDialog(customer);
        }}
        onMergeRecords={(masterRecordId, recordsToMerge) => {
          setDuplicateDialogOpen(false);
          const allIds = [masterRecordId, ...recordsToMerge];
          const records: MergeRecordData[] = allIds
            .map(rid => customers.find(c => c.id === rid))
            .filter(Boolean)
            .map(c => ({ id: c!.id, displayName: c!.company || `${c!.firstName} ${c!.lastName}`, data: c as any }));
          if (records.length >= 2) {
            setMergeDialogOpen(true);
          }
        }}
      />

      {/* Merge Dialog */}
      <MergeDialog
        open={mergeDialogOpen}
        onClose={() => setMergeDialogOpen(false)}
        entityType="Account"
        records={selectedIds
          .map(sid => customers.find(c => c.id === sid))
          .filter(Boolean)
          .map(c => ({ id: c!.id, displayName: c!.company || `${c!.firstName} ${c!.lastName}`, data: c as any }))}
        onMergeComplete={(result) => {
          setMergeDialogOpen(false);
          setSelectedIds([]);
          setSuccessMessage(`Records merged successfully into master record #${result.masterRecordId}`);
          fetchAccounts();
        }}
      />
    </Box>
  );
}

export default CustomersPage;

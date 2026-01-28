import { useState, useEffect, useCallback, useMemo } from 'react';
import {
  Box, Container, Typography, Card, CardContent, Table, TableBody, TableCell, TableContainer,
  TableHead, TableRow, CircularProgress, Alert, Button, Dialog, DialogTitle,
  DialogContent, DialogActions, Grid, Chip, Tabs, Tab, IconButton, Tooltip, Divider,
  List, ListItem, ListItemText, ListItemSecondaryAction, Autocomplete, TextField,
  FormControl, InputLabel, Select, MenuItem, Checkbox, FormControlLabel, Paper,
  SelectChangeEvent, Collapse, Stack
} from '@mui/material';
import { TabPanel, DialogError, DialogSuccess, ActionButton } from '../components/common';
import {
  LIFECYCLE_STAGE_OPTIONS,
  CUSTOMER_TYPE_OPTIONS,
  PRIORITY_OPTIONS,
  CONTACT_ROLE_OPTIONS
} from '../utils/constants';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon,
  Business as BusinessIcon, Person as PersonIcon, Email as EmailIcon,
  Phone as PhoneIcon, PersonAdd as PersonAddIcon, Group as GroupIcon,
  ContactPhone as ContactPhoneIcon, Refresh as RefreshIcon,
  FilterAlt as FilterIcon, Close as CloseIcon
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import FieldRenderer from '../components/FieldRenderer';
import ImportExportButtons from '../components/ImportExportButtons';
import AdvancedSearch, { SearchField, SearchFilter, filterData } from '../components/AdvancedSearch';
import { ContactInfoPanel } from '../components/ContactInfo';
import { useFieldConfig, ModuleFieldConfiguration, dispatchFieldConfigUpdate } from '../hooks/useFieldConfig';
import { useAccountContext } from '../contexts/AccountContextProvider';
import { useApiState } from '../hooks/useApiState';
import logo from '../assets/logo.png';
import { BaseEntity } from '../types';

// Search fields for Advanced Search
const SEARCH_FIELDS: SearchField[] = [
  { name: 'customerType', label: 'Account Type', type: 'select', options: [...CUSTOMER_TYPE_OPTIONS] },
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
const CUSTOMER_TYPES = CUSTOMER_TYPE_OPTIONS;
const PRIORITIES = PRIORITY_OPTIONS;
const CONTACT_ROLES = CONTACT_ROLE_OPTIONS;

interface CustomerContact extends BaseEntity {
  customerId: number;
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
  customerId?: number;
}

interface Customer extends BaseEntity {
  category: number;
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

interface CustomerForm {
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

const INITIAL_FORM_DATA: CustomerForm = {
  category: 0,
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

function CustomersPage() {
  // Data state
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Dialog state
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  const [formData, setFormData] = useState<CustomerForm>(INITIAL_FORM_DATA);

  // Contact linking state
  const [customerContacts, setCustomerContacts] = useState<CustomerContact[]>([]);
  const [addContactDialogOpen, setAddContactDialogOpen] = useState(false);
  const [selectedContactId, setSelectedContactId] = useState<number | null>(null);
  const [contactRole, setContactRole] = useState<number>(0);
  const [contactIsPrimary, setContactIsPrimary] = useState(false);
  const [contactIsDecisionMaker, setContactIsDecisionMaker] = useState(false);

  // Direct contacts state (one-to-many relationship)
  const [directContacts, setDirectContacts] = useState<Contact[]>([]);
  const [assignContactDialogOpen, setAssignContactDialogOpen] = useState(false);
  const [selectedDirectContactId, setSelectedDirectContactId] = useState<number | null>(null);

  // Search state
  const [searchFilters, setSearchFilters] = useState<SearchFilter[]>([]);
  const [searchText, setSearchText] = useState('');
  
  // Multi-select and bulk update state
  const [selectedIds, setSelectedIds] = useState<number[]>([]);
  const [bulkDialogOpen, setBulkDialogOpen] = useState(false);
  const [bulkFormData, setBulkFormData] = useState({
    customerType: '' as string | number,
    lifecycleStage: '' as string | number,
    priority: '' as string | number,
    industry: '' as string,
    territory: '' as string,
  });
  
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
  } = useFieldConfig('Customers');

  // Get account context for filtering
  const { selectedAccounts, isContextActive, getAccountIds } = useAccountContext();

  // Filter customers based on search AND account context
  const filteredCustomers = useMemo(() => {
    let result = customers;
    
    // Apply account context filter first
    if (isContextActive) {
      const accountIds = getAccountIds();
      result = result.filter(customer => accountIds.includes(customer.id!));
    }
    
    // Then apply search filters
    return filterData(result, searchFilters, searchText, SEARCHABLE_FIELDS);
  }, [customers, searchFilters, searchText, isContextActive, getAccountIds]);

  // Fetch data on mount
  useEffect(() => {
    fetchCustomers();
    fetchContacts();
  }, []);

  const fetchCustomers = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/customers');
      setCustomers(response.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch accounts');
    } finally {
      setLoading(false);
    }
  };

  const fetchContacts = async () => {
    try {
      const response = await apiClient.get('/contacts');
      setContacts(response.data);
    } catch (err) {
      console.error('Error fetching contacts:', err);
    }
  };

  const fetchCustomerContacts = async (customerId: number) => {
    try {
      const response = await apiClient.get(`/customers/${customerId}/contacts`);
      setCustomerContacts(response.data);
    } catch (err) {
      console.error('Error fetching customer contacts:', err);
      setCustomerContacts([]);
    }
  };

  const fetchDirectContacts = async (customerId: number) => {
    try {
      const response = await apiClient.get(`/customers/${customerId}/direct-contacts`);
      setDirectContacts(response.data);
    } catch (err) {
      console.error('Error fetching direct contacts:', err);
      setDirectContacts([]);
    }
  };

  const handleAssignDirectContact = async () => {
    if (!editingId || !selectedDirectContactId) return;

    try {
      await apiClient.post(`/customers/${editingId}/direct-contacts/${selectedDirectContactId}`);
      fetchDirectContacts(editingId);
      fetchContacts(); // Refresh all contacts to update their customerId
      setAssignContactDialogOpen(false);
      setSelectedDirectContactId(null);
      setSuccessMessage('Contact assigned successfully');
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to assign contact');
    }
  };

  const handleUnassignDirectContact = async (contactId: number) => {
    if (!editingId) return;

    if (window.confirm('Are you sure you want to unassign this contact from the account?')) {
      try {
        await apiClient.delete(`/customers/${editingId}/direct-contacts/${contactId}`);
        fetchDirectContacts(editingId);
        fetchContacts(); // Refresh all contacts to update their customerId
        setSuccessMessage('Contact unassigned successfully');
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to unassign contact');
      }
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
  const handleOpenDialog = useCallback((customer?: Customer) => {
    setDialogTab(0);
    if (customer) {
      setEditingId(customer.id);
      // Map customer data to form
      const formValues: CustomerForm = { ...INITIAL_FORM_DATA };
      Object.keys(customer).forEach(key => {
        if (key in formValues) {
          (formValues as any)[key] = customer[key] ?? (INITIAL_FORM_DATA as any)[key];
        }
      });
      setFormData(formValues);
      // Fetch direct contacts for all customers
      fetchDirectContacts(customer.id);
      // Fetch linked contacts for organizations
      if (customer.category === 1) {
        fetchCustomerContacts(customer.id);
      } else {
        setCustomerContacts([]);
      }
    } else {
      setEditingId(null);
      setCustomerContacts([]);
      setDirectContacts([]);
      setFormData(INITIAL_FORM_DATA);
    }
    setOpenDialog(true);
  }, []);

  const handleCloseDialog = useCallback(() => {
    setOpenDialog(false);
    setEditingId(null);
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
      setError(`Please fill in required fields: ${missing.map(m => m.fieldLabel).join(', ')}`);
      return false;
    }

    return true;
  }, [fieldConfigs, fieldConfigsLoading, formData, isFieldVisible]);

  const handleSaveCustomer = async () => {
    setError(null);

    if (!validateRequiredFields()) return;

    try {
      if (editingId) {
        await apiClient.put(`/customers/${editingId}`, formData);
        setSuccessMessage('Account updated successfully');
      } else {
        await apiClient.post('/customers', formData);
        setSuccessMessage('Account created successfully');
      }
      handleCloseDialog();
      fetchCustomers();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save account');
    }
  };

  const handleDeleteCustomer = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this account?')) {
      const result = await dialogApi.execute(async () => {
        await apiClient.delete(`/customers/${id}`);
        return true;
      }, 'Account deleted successfully');
      
      if (result) {
        setSelectedIds(prev => prev.filter(sid => sid !== id));
        fetchCustomers();
        setSuccessMessage('Account deleted successfully');
        setTimeout(() => setSuccessMessage(null), 3000);
      } else {
        setError(dialogApi.error?.message || 'Failed to delete account');
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
        apiClient.put(`/customers/${id}`, updatePayload)
      );
      await Promise.all(updatePromises);
      return selectedIds.length;
    }, `Successfully updated ${selectedIds.length} account(s)`);

    if (result) {
      fetchCustomers();
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
      const deletePromises = selectedIds.map(id => apiClient.delete(`/customers/${id}`));
      await Promise.all(deletePromises);
      return selectedIds.length;
    }, `Successfully deleted ${selectedIds.length} account(s)`);

    if (result) {
      fetchCustomers();
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
      await apiClient.post(`/customers/${editingId}/contacts`, {
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
      setError(err.response?.data?.message || 'Failed to link contact');
    }
  };

  const handleRemoveContact = async (contactId: number) => {
    if (!editingId) return;

    if (window.confirm('Are you sure you want to remove this contact from the account?')) {
      try {
        await apiClient.delete(`/customers/${editingId}/contacts/${contactId}`);
        fetchCustomerContacts(editingId);
        setSuccessMessage('Contact removed successfully');
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to remove contact');
      }
    }
  };

  // Helper functions
  const getLifecycleStage = (value: number) => LIFECYCLE_STAGES.find(s => s.value === value);
  const getPriority = (value: number) => PRIORITIES.find(p => p.value === value);
  const getCustomerType = (value: number) => CUSTOMER_TYPES.find(t => t.value === value);

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

    // Add Direct Contacts tab when editing (one-to-many relationship)
    if (editingId) {
      baseTabs.push({ index: 102, name: 'Direct Contacts' });
    }
    
    // Add Linked Contacts tab for organizations when editing (many-to-many relationship)
    if (formData.category === 1 && editingId) {
      baseTabs.push({ index: 101, name: 'Linked Contacts' });
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
              entityType="customers"
              entityLabel="Accounts"
              onImportComplete={fetchCustomers}
            />
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
        {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
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
                <Button
                  variant="contained"
                  size="small"
                  color="error"
                  onClick={handleBulkDelete}
                >
                  Delete Selected
                </Button>
                <IconButton size="small" onClick={() => setSelectedIds([])} sx={{ color: 'white' }}>
                  <CloseIcon />
                </IconButton>
              </Box>
            </Box>
          </Paper>
        </Collapse>

        {/* Customer List */}
        <Card>
          <CardContent sx={{ p: 0 }}>
            <TableContainer sx={{ overflowX: 'auto' }}>
              <Table sx={{ minWidth: 950 }}>
                <TableHead>
                  <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                    <TableCell padding="checkbox">
                      <Checkbox
                        indeterminate={selectedIds.length > 0 && selectedIds.length < filteredCustomers.length}
                        checked={filteredCustomers.length > 0 && selectedIds.length === filteredCustomers.length}
                        onChange={handleSelectAll}
                      />
                    </TableCell>
                    <TableCell><strong>Category</strong></TableCell>
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
                  {filteredCustomers.map((customer) => {
                    const stage = getLifecycleStage(customer.lifecycleStage);
                    const priority = getPriority(customer.priority);
                    const type = getCustomerType(customer.customerType);
                    const isOrganization = customer.category === 1;
                    return (
                      <TableRow key={customer.id} hover selected={selectedIds.includes(customer.id)}>
                        <TableCell padding="checkbox">
                          <Checkbox
                            checked={selectedIds.includes(customer.id)}
                            onChange={() => handleSelectOne(customer.id)}
                          />
                        </TableCell>
                        <TableCell>
                          <Chip
                            icon={isOrganization ? <BusinessIcon fontSize="small" /> : <PersonIcon fontSize="small" />}
                            label={isOrganization ? 'Organization' : 'Individual'}
                            size="small"
                            variant="outlined"
                            color={isOrganization ? 'primary' : 'default'}
                          />
                        </TableCell>
                        <TableCell>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            {isOrganization ? (
                              <BusinessIcon fontSize="small" sx={{ color: '#6750A4' }} />
                            ) : (
                              <PersonIcon fontSize="small" sx={{ color: '#6750A4' }} />
                            )}
                            <Box>
                              <Typography fontWeight={500}>
                                {customer.displayName || (isOrganization ? customer.company : `${customer.firstName} ${customer.lastName}`)}
                              </Typography>
                              {isOrganization && customer.legalName && (
                                <Typography variant="caption" color="textSecondary">{customer.legalName}</Typography>
                              )}
                              {!isOrganization && customer.jobTitle && (
                                <Typography variant="caption" color="textSecondary">{customer.jobTitle}</Typography>
                              )}
                            </Box>
                          </Box>
                        </TableCell>
                        <TableCell>
                          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                              <EmailIcon fontSize="small" sx={{ color: '#666', fontSize: 14 }} />
                              <Typography variant="body2">{customer.email}</Typography>
                            </Box>
                            {customer.phone && (
                              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                                <PhoneIcon fontSize="small" sx={{ color: '#666', fontSize: 14 }} />
                                <Typography variant="body2">{customer.phone}</Typography>
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
                          ${customer.annualRevenue?.toLocaleString() || 0}
                        </TableCell>
                        <TableCell>
                          {isOrganization ? (
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                              <GroupIcon fontSize="small" sx={{ color: '#666' }} />
                              <Typography variant="body2">{customer.contactCount || 0}</Typography>
                            </Box>
                          ) : (
                            <Typography variant="body2" color="textSecondary">—</Typography>
                          )}
                        </TableCell>
                        <TableCell align="center">
                          <Tooltip title="Edit">
                            <IconButton size="small" onClick={() => handleOpenDialog(customer)} sx={{ color: '#6750A4' }}>
                              <EditIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete">
                            <IconButton size="small" onClick={() => handleDeleteCustomer(customer.id)} sx={{ color: '#f44336' }}>
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
            {customers.length === 0 && (
              <Typography sx={{ textAlign: 'center', py: 4, color: 'textSecondary' }}>
                No accounts found. Add your first account to get started.
              </Typography>
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Add/Edit Account Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle sx={{ pb: 0 }}>{editingId ? 'Edit Account' : 'Add Account'}</DialogTitle>
        <Box sx={{ borderBottom: 1, borderColor: 'divider', px: 3 }}>
          <Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)} variant="scrollable" scrollButtons="auto">
            {visibleTabs.map((tab, idx) => (
              <Tab 
                key={tab.index} 
                label={tab.name}
                icon={
                  tab.index === 100 ? <ContactPhoneIcon fontSize="small" /> : 
                  tab.index === 101 ? <GroupIcon fontSize="small" /> : 
                  tab.index === 102 ? <PersonIcon fontSize="small" /> : 
                  undefined
                }
                iconPosition="start"
              />
            ))}
          </Tabs>
        </Box>
        <DialogContent sx={{ pt: 2, minHeight: 400 }}>
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
                    entityType="Customer"
                    entityId={editingId}
                    layout="tabs"
                    showCounts={true}
                  />
                </TabPanel>
              )}

              {/* Direct Contacts Tab (one-to-many relationship) */}
              {editingId && (
                <TabPanel value={dialogTab} index={visibleTabs.findIndex(t => t.index === 102)}>
                  <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Typography variant="subtitle1" fontWeight={600}>
                      Direct Contacts ({directContacts.length})
                    </Typography>
                    <Button
                      variant="outlined"
                      startIcon={<PersonAddIcon />}
                      onClick={() => setAssignContactDialogOpen(true)}
                      size="small"
                    >
                      Assign Contact
                    </Button>
                  </Box>

                  <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
                    These contacts are directly owned by this customer. Use this for contacts that belong exclusively to this customer.
                  </Typography>

                  {directContacts.length === 0 ? (
                    <Paper elevation={0} sx={{ p: 4, textAlign: 'center', backgroundColor: '#F5EFF7', borderRadius: 2 }}>
                      <PersonIcon sx={{ fontSize: 48, color: '#6750A4', opacity: 0.5, mb: 1 }} />
                      <Typography color="textSecondary">No contacts directly assigned to this customer</Typography>
                    </Paper>
                  ) : (
                    <List sx={{ bgcolor: 'background.paper', borderRadius: 1, border: '1px solid #e0e0e0' }}>
                      {directContacts.map((contact: any, index: number) => (
                        <Box key={contact.id}>
                          {index > 0 && <Divider />}
                          <ListItem>
                            <ListItemText
                              primary={
                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                  <PersonIcon fontSize="small" sx={{ color: '#6750A4' }} />
                                  <Typography fontWeight={500}>{contact.fullName}</Typography>
                                  {contact.jobTitle && (
                                    <Chip label={contact.jobTitle} size="small" variant="outlined" />
                                  )}
                                </Box>
                              }
                              secondary={
                                <Box sx={{ display: 'flex', gap: 2, mt: 0.5 }}>
                                  {contact.emailPrimary && (
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                                      <EmailIcon fontSize="small" sx={{ fontSize: 14, color: '#666' }} />
                                      <Typography variant="caption">{contact.emailPrimary}</Typography>
                                    </Box>
                                  )}
                                  {contact.phonePrimary && (
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                                      <PhoneIcon fontSize="small" sx={{ fontSize: 14, color: '#666' }} />
                                      <Typography variant="caption">{contact.phonePrimary}</Typography>
                                    </Box>
                                  )}
                                </Box>
                              }
                            />
                            <ListItemSecondaryAction>
                              <Tooltip title="Unassign from customer">
                                <IconButton edge="end" onClick={() => handleUnassignDirectContact(contact.id)} size="small" color="error">
                                  <DeleteIcon fontSize="small" />
                                </IconButton>
                              </Tooltip>
                            </ListItemSecondaryAction>
                          </ListItem>
                        </Box>
                      ))}
                    </List>
                  )}
                </TabPanel>
              )}

              {/* Linked Contacts Tab for Organizations */}
              {formData.category === 1 && editingId && (
                <TabPanel value={dialogTab} index={visibleTabs.findIndex(t => t.index === 101)}>
                  <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Typography variant="subtitle1" fontWeight={600}>
                      Organization Contacts ({customerContacts.length})
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
                      <Typography color="textSecondary">No contacts linked to this organization</Typography>
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
            </>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSaveCustomer} variant="contained" sx={{ backgroundColor: '#6750A4' }}>
            {editingId ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Add Contact Dialog */}
      <Dialog open={addContactDialogOpen} onClose={() => setAddContactDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Link Contact to Organization</DialogTitle>
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

      {/* Assign Direct Contact Dialog */}
      <Dialog open={assignContactDialogOpen} onClose={() => setAssignContactDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Assign Contact to Account</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="textSecondary" sx={{ mb: 2, mt: 1 }}>
            Select a contact to assign directly to this account. This establishes an exclusive ownership relationship.
          </Typography>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <Autocomplete
                options={contacts.filter(c => !c.customerId && !directContacts.some((dc: any) => dc.id === c.id))}
                getOptionLabel={(option) => `${option.firstName} ${option.lastName}${option.company ? ` (${option.company})` : ''}`}
                value={contacts.find(c => c.id === selectedDirectContactId) || null}
                onChange={(_, newValue) => setSelectedDirectContactId(newValue?.id || null)}
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
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAssignContactDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleAssignDirectContact} variant="contained" disabled={!selectedDirectContactId} sx={{ backgroundColor: '#6750A4' }}>
            Assign Contact
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
                {CUSTOMER_TYPE_OPTIONS.map(type => (
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
    </Box>
  );
}

export default CustomersPage;

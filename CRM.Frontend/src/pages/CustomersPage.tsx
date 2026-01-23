import { 
  Box, Container, Typography, Card, CardContent, Table, TableBody, TableCell, 
  TableHead, TableRow, CircularProgress, Alert, Button, Dialog, DialogTitle, 
  DialogContent, DialogActions, TextField, FormControl, InputLabel, Select, 
  MenuItem, Chip, Tabs, Tab, Slider, FormControlLabel, Checkbox, Grid,
  SelectChangeEvent, IconButton, Tooltip, Switch, Autocomplete, Paper, Divider,
  List, ListItem, ListItemText, ListItemSecondaryAction
} from '@mui/material';
import { useState, useEffect } from 'react';
import { 
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, 
  Business as BusinessIcon, Person as PersonIcon, Email as EmailIcon,
  Phone as PhoneIcon, LinkedIn as LinkedInIcon, Twitter as TwitterIcon,
  PersonAdd as PersonAddIcon, Group as GroupIcon
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import lookupService, { LookupItem } from '../services/lookupService';
import FieldRenderer from '../components/FieldRenderer';
import LookupSelect from '../components/LookupSelect';
import ImportExportButtons from '../components/ImportExportButtons';
import AdvancedSearch, { SearchField, SearchFilter, filterData } from '../components/AdvancedSearch';
import logo from '../assets/logo.png';

// Search fields for Advanced Search
const SEARCH_FIELDS: SearchField[] = [
  { name: 'customerType', label: 'Customer Type', type: 'select', options: [
    { value: 0, label: 'Individual' },
    { value: 1, label: 'Small Business' },
    { value: 2, label: 'Mid-Market' },
    { value: 3, label: 'Enterprise' },
    { value: 4, label: 'Government' },
    { value: 5, label: 'Non-Profit' },
  ]},
  { name: 'firstName', label: 'First Name', type: 'text' },
  { name: 'lastName', label: 'Last Name', type: 'text' },
  { name: 'company', label: 'Business Name', type: 'text' },
  { name: 'email', label: 'Email', type: 'text' },
  { name: 'lifecycleStage', label: 'Status', type: 'select', options: [
    { value: 0, label: 'Lead' },
    { value: 1, label: 'Prospect' },
    { value: 2, label: 'Opportunity' },
    { value: 3, label: 'Customer' },
    { value: 4, label: 'Churned' },
    { value: 5, label: 'Reactivated' },
  ]},
  { name: 'industry', label: 'Industry', type: 'text' },
  { name: 'city', label: 'City', type: 'text' },
];

const SEARCHABLE_FIELDS = ['firstName', 'lastName', 'company', 'email', 'industry', 'city', 'phone'];

// Customer Categories
const CUSTOMER_CATEGORIES = [
  { value: 0, label: 'Individual' },
  { value: 1, label: 'Organization' },
];

// Enums matching backend
const LIFECYCLE_STAGES = [
  { value: 0, label: 'Lead', color: '#9e9e9e' },
  { value: 1, label: 'Prospect', color: '#2196f3' },
  { value: 2, label: 'Opportunity', color: '#ff9800' },
  { value: 3, label: 'Customer', color: '#4caf50' },
  { value: 4, label: 'Churned', color: '#f44336' },
  { value: 5, label: 'Reactivated', color: '#9c27b0' },
];

const CUSTOMER_TYPES = [
  { value: 0, label: 'Individual' },
  { value: 1, label: 'Small Business' },
  { value: 2, label: 'Mid-Market' },
  { value: 3, label: 'Enterprise' },
  { value: 4, label: 'Government' },
  { value: 5, label: 'Non-Profit' },
];

const PRIORITIES = [
  { value: 0, label: 'Low', color: '#9e9e9e' },
  { value: 1, label: 'Medium', color: '#2196f3' },
  { value: 2, label: 'High', color: '#ff9800' },
  { value: 3, label: 'Critical', color: '#f44336' },
];

const CONTACT_ROLES = [
  { value: 0, label: 'Primary' },
  { value: 1, label: 'Secondary' },
  { value: 2, label: 'Billing' },
  { value: 3, label: 'Technical' },
  { value: 4, label: 'Decision Maker' },
  { value: 5, label: 'Influencer' },
  { value: 6, label: 'End User' },
  { value: 7, label: 'Executive' },
  { value: 8, label: 'Procurement' },
  { value: 9, label: 'Other' },
];

const INDUSTRIES = [
  'Technology', 'Healthcare', 'Finance', 'Retail', 'Manufacturing', 
  'Education', 'Real Estate', 'Consulting', 'Marketing', 'Legal', 
  'Non-Profit', 'Government', 'Other'
];

const LEAD_SOURCES = [
  'Website', 'Referral', 'Social Media', 'Cold Call', 'Trade Show', 
  'Advertisement', 'Email Campaign', 'Partner', 'Other'
];

interface CustomerContact {
  id: number;
  customerId: number;
  contactId: number;
  contactName: string;
  contactEmail?: string;
  contactPhone?: string;
  role: string;
  isPrimaryContact: boolean;
  isDecisionMaker: boolean;
  receivesBillingNotifications: boolean;
  receivesMarketingEmails: boolean;
  receivesTechnicalUpdates: boolean;
  positionAtCustomer?: string;
  departmentAtCustomer?: string;
  notes?: string;
}

interface Contact {
  id: number;
  firstName: string;
  lastName: string;
  emailPrimary?: string;
  phonePrimary?: string;
  company?: string;
  jobTitle?: string;
}

interface Customer {
  id: number;
  category: number;
  firstName: string;
  lastName: string;
  salutation?: string;
  suffix?: string;
  dateOfBirth?: string;
  gender?: string;
  email: string;
  secondaryEmail?: string;
  phone: string;
  mobilePhone?: string;
  company: string;
  legalName?: string;
  dbaName?: string;
  taxId?: string;
  registrationNumber?: string;
  yearFounded?: number;
  jobTitle?: string;
  website?: string;
  address: string;
  address2?: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  industry?: string;
  numberOfEmployees?: number;
  annualRevenue: number;
  customerType: number;
  priority: number;
  lifecycleStage: number;
  leadSource?: string;
  firstContactDate?: string;
  conversionDate?: string;
  lastActivityDate?: string;
  nextFollowUpDate?: string;
  totalPurchases: number;
  accountBalance: number;
  creditLimit: number;
  paymentTerms?: string;
  leadScore: number;
  customerHealthScore: number;
  npsScore: number;
  satisfactionRating: number;
  linkedInUrl?: string;
  twitterHandle?: string;
  optInEmail: boolean;
  optInSms: boolean;
  optInPhone: boolean;
  preferredContactMethod?: string;
  timezone?: string;
  assignedToUserId?: number;
  territory?: string;
  tags?: string;
  segment?: string;
  notes: string;
  description?: string;
  createdAt: string;
  displayName: string;
  contacts?: CustomerContact[];
  contactCount: number;
}

interface CustomerForm {
  category: number;
  // Individual fields
  firstName: string;
  lastName: string;
  salutation: string;
  suffix: string;
  dateOfBirth: string;
  gender: string;
  // Organization fields
  company: string;
  legalName: string;
  dbaName: string;
  taxId: string;
  registrationNumber: string;
  yearFounded: number | null;
  // Common fields
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
}

interface User {
  id: number;
  firstName: string;
  lastName: string;
}

interface ModuleFieldConfiguration {
  id: number;
  moduleName: string;
  fieldName: string;
  fieldLabel: string;
  fieldType: string;
  tabIndex: number;
  tabName: string;
  displayOrder: number;
  isEnabled: boolean;
  isRequired: boolean;
  gridSize: number;
  placeholder?: string;
  helpText?: string;
  options?: string;
  parentField?: string;
  parentFieldValue?: string;
  isReorderable: boolean;
  isRequiredConfigurable: boolean;
  isHideable: boolean;
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div role="tabpanel" hidden={value !== index} {...other}>
      {value === index && <Box sx={{ pt: 2 }}>{children}</Box>}
    </div>
  );
}

function CustomersPage() {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  const [customerContacts, setCustomerContacts] = useState<CustomerContact[]>([]);
  const [addContactDialogOpen, setAddContactDialogOpen] = useState(false);
  const [selectedContactId, setSelectedContactId] = useState<number | null>(null);
  const [contactRole, setContactRole] = useState<number>(0);
  const [contactIsPrimary, setContactIsPrimary] = useState(false);
  const [contactIsDecisionMaker, setContactIsDecisionMaker] = useState(false);
  const [fieldConfigs, setFieldConfigs] = useState<ModuleFieldConfiguration[]>([]);
  const [fieldConfigsLoading, setFieldConfigsLoading] = useState(false);
  const [lookupItems, setLookupItems] = useState<Record<string, LookupItem[]>>({});
  const [searchFilters, setSearchFilters] = useState<SearchFilter[]>([]);
  const [searchText, setSearchText] = useState('');

  const handleSearch = (filters: SearchFilter[], text: string) => {
    setSearchFilters(filters);
    setSearchText(text);
  };

  const filteredCustomers = filterData(customers, searchFilters, searchText, SEARCHABLE_FIELDS);
  const [formData, setFormData] = useState<CustomerForm>({
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
  });

  useEffect(() => {
    fetchCustomers();
    fetchUsers();
    fetchContacts();
    fetchFieldConfigurations();
    // preload common lookups
    loadLookup('PreferredContactMethod');
  }, []);

  const loadLookup = async (category: string) => {
    try {
      const items = await lookupService.getLookupItems(category);
      setLookupItems(prev => ({ ...prev, [category]: items }));
    } catch (err) {
      console.warn('Failed to load lookup', category, err);
    }
  };

  const fetchCustomers = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/customers');
      setCustomers(response.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch customers');
    } finally {
      setLoading(false);
    }
  };

  const fetchUsers = async () => {
    try {
      const response = await apiClient.get('/users');
      setUsers(response.data);
    } catch (err) {
      console.error('Error fetching users:', err);
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

  const fetchFieldConfigurations = async () => {
    try {
      setFieldConfigsLoading(true);
      const response = await apiClient.get('/modulefieldconfigurations/Customers');
      setFieldConfigs(response.data || []);
    } catch (err) {
      console.error('Error fetching field configurations:', err);
      setFieldConfigs([]);
    } finally {
      setFieldConfigsLoading(false);
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

  const handleOpenDialog = (customer?: Customer) => {
    setDialogTab(0);
    if (customer) {
      setEditingId(customer.id);
      setFormData({
        category: customer.category || 0,
        firstName: customer.firstName || '',
        lastName: customer.lastName || '',
        salutation: customer.salutation || '',
        suffix: customer.suffix || '',
        dateOfBirth: customer.dateOfBirth || '',
        gender: customer.gender || '',
        company: customer.company || '',
        legalName: customer.legalName || '',
        dbaName: customer.dbaName || '',
        taxId: customer.taxId || '',
        registrationNumber: customer.registrationNumber || '',
        yearFounded: customer.yearFounded || null,
        email: customer.email,
        secondaryEmail: customer.secondaryEmail || '',
        phone: customer.phone,
        mobilePhone: customer.mobilePhone || '',
        jobTitle: customer.jobTitle || '',
        website: customer.website || '',
        address: customer.address,
        city: customer.city,
        state: customer.state,
        zipCode: customer.zipCode,
        country: customer.country,
        industry: customer.industry || '',
        numberOfEmployees: customer.numberOfEmployees || 0,
        annualRevenue: customer.annualRevenue,
        customerType: customer.customerType,
        priority: customer.priority,
        lifecycleStage: customer.lifecycleStage,
        leadSource: customer.leadSource || '',
        leadScore: customer.leadScore,
        creditLimit: customer.creditLimit,
        paymentTerms: customer.paymentTerms || 'Net 30',
        linkedInUrl: customer.linkedInUrl || '',
        twitterHandle: customer.twitterHandle || '',
        optInEmail: customer.optInEmail,
        optInSms: customer.optInSms,
        optInPhone: customer.optInPhone,
        preferredContactMethod: customer.preferredContactMethod || 'Email',
        timezone: customer.timezone || '',
        territory: customer.territory || '',
        tags: customer.tags || '',
        notes: customer.notes,
        description: customer.description || '',
      });
      // Fetch contacts for organizations
      if (customer.category === 1) {
        fetchCustomerContacts(customer.id);
      } else {
        setCustomerContacts([]);
      }
    } else {
      setEditingId(null);
      setCustomerContacts([]);
      setFormData({
        category: 0, firstName: '', lastName: '', salutation: '', suffix: '', dateOfBirth: '', gender: '',
        company: '', legalName: '', dbaName: '', taxId: '', registrationNumber: '', yearFounded: null,
        email: '', secondaryEmail: '', phone: '', mobilePhone: '',
        jobTitle: '', website: '', address: '', city: '', state: '', zipCode: '',
        country: 'USA', industry: '', numberOfEmployees: 0, annualRevenue: 0, customerType: 0,
        priority: 1, lifecycleStage: 0, leadSource: '', leadScore: 0, creditLimit: 0,
        paymentTerms: 'Net 30', linkedInUrl: '', twitterHandle: '', optInEmail: true,
        optInSms: false, optInPhone: true, preferredContactMethod: 'Email', timezone: '',
        territory: '', tags: '', notes: '', description: '',
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    const checked = (e.target as HTMLInputElement).checked;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : type === 'number' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleSelectChange = (e: SelectChangeEvent<string | number>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const validateRequiredFields = () => {
    if (fieldConfigsLoading) return true;
    if (!fieldConfigs.length) return true;

    const visibleRequired = fieldConfigs.filter(cfg => cfg.isRequired && isFieldVisible(cfg, formData.category));
    const missing = visibleRequired.filter(cfg => {
      const value = (formData as any)[cfg.fieldName];
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
  };

  const handleSaveCustomer = async () => {
    setError(null);

    if (fieldConfigs.length) {
      const valid = validateRequiredFields();
      if (!valid) return;
    } else {
      // Fallback validation based on category when no configs are present
      if (formData.category === 0) {
        if (!formData.firstName.trim() || !formData.lastName.trim() || !formData.email.trim()) {
          setError('Please fill in required fields (First Name, Last Name, Email)');
          return;
        }
      } else {
        if (!formData.company.trim() || !formData.email.trim()) {
          setError('Please fill in required fields (Company Name, Email)');
          return;
        }
      }
    }

    try {
      if (editingId) {
        await apiClient.put(`/customers/${editingId}`, formData);
        setSuccessMessage('Customer updated successfully');
      } else {
        await apiClient.post('/customers', formData);
        setSuccessMessage('Customer created successfully');
      }
      handleCloseDialog();
      fetchCustomers();
      setError(null);
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save customer');
    }
  };

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
    
    if (window.confirm('Are you sure you want to remove this contact from the customer?')) {
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

  const handleDeleteCustomer = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this customer?')) {
      try {
        await apiClient.delete(`/customers/${id}`);
        setSuccessMessage('Customer deleted successfully');
        fetchCustomers();
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete customer');
      }
    }
  };

  const getLifecycleStage = (value: number) => LIFECYCLE_STAGES.find(s => s.value === value);
  const getPriority = (value: number) => PRIORITIES.find(p => p.value === value);
  const getCustomerType = (value: number) => CUSTOMER_TYPES.find(t => t.value === value);

  const isFieldVisible = (config: ModuleFieldConfiguration, categoryValue: number) => {
    if (!config.isEnabled) return false;
    if (config.parentField === 'category' && config.parentFieldValue !== undefined) {
      return String(categoryValue) === String(config.parentFieldValue);
    }
    if (config.parentField && config.parentFieldValue) {
      const current = (formData as any)[config.parentField];
      return String(current ?? '') === String(config.parentFieldValue);
    }
    return true;
  };

  const getTabFields = (tabIndex: number) => {
    if (!fieldConfigs.length) return [] as ModuleFieldConfiguration[];
    return fieldConfigs
      .filter(f => f.tabIndex === tabIndex)
      .filter(f => isFieldVisible(f, formData.category))
      .sort((a, b) => a.displayOrder - b.displayOrder);
  };

  const getSelectOptions = (config: ModuleFieldConfiguration) => {
    if (!config.options) return [] as string[];
    // support static comma list
    if (!config.options.startsWith('lookup:')) {
      return config.options.split(',').map(o => o.trim()).filter(Boolean);
    }
    // format: lookup:CategoryName
    const parts = config.options.split(':');
    const category = parts[1] || parts.slice(1).join(':');
    const items = lookupItems[category] || [];
    return items.map(i => i.key || i.value);
  };

  const renderField = (config: ModuleFieldConfiguration) => {
    return (
      <FieldRenderer
        config={config}
        formData={formData}
        onChange={handleInputChange}
        onSelectChange={handleSelectChange}
        setFormData={setFormData}
      />
    );
  };

  const renderTabFields = (tabIndex: number) => {
    const configs = getTabFields(tabIndex);
    if (!configs.length) return null;
    return (
      <Grid container spacing={2}>
        {configs.map(cfg => (
          <Grid key={cfg.id} item xs={cfg.gridSize || 12}>
            {renderField(cfg)}
          </Grid>
        ))}
      </Grid>
    );
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
            <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
              <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
            </Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>Customers</Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
            <ImportExportButtons
              entityType="customers"
              entityLabel="Customers"
              onImportComplete={fetchCustomers}
            />
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={() => handleOpenDialog()}
              sx={{ backgroundColor: '#6750A4' }}
            >
              Add Customer
            </Button>
          </Box>
        </Box>
        
        {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}
        
        <AdvancedSearch
          fields={SEARCH_FIELDS}
          onSearch={handleSearch}
          placeholder="Search customers by name, email, company..."
        />

        <Card>
          <CardContent sx={{ p: 0 }}>
            <Table>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
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
                    <TableRow key={customer.id} hover>
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
            {customers.length === 0 && (
              <Typography sx={{ textAlign: 'center', py: 4, color: 'textSecondary' }}>
                No customers found. Add your first customer to get started.
              </Typography>
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Enhanced Add/Edit Customer Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle sx={{ pb: 0 }}>{editingId ? 'Edit Customer' : 'Add Customer'}</DialogTitle>
        <Box sx={{ borderBottom: 1, borderColor: 'divider', px: 3 }}>
          <Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)}>
            <Tab label="Basic Info" />
            <Tab label="Business" />
            <Tab label="Contact Preferences" />
            <Tab label="Additional" />
            {formData.category === 1 && editingId && <Tab label="Linked Contacts" icon={<GroupIcon fontSize="small" />} iconPosition="start" />}
          </Tabs>
        </Box>
        <DialogContent sx={{ pt: 2, minHeight: 400 }}>
          <TabPanel value={dialogTab} index={0}>
            {fieldConfigsLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', py: 3 }}>
                <CircularProgress size={24} />
              </Box>
            ) : fieldConfigs.length ? (
              renderTabFields(0)
            ) : (
              <Grid container spacing={2}>
                {/* Category Toggle */}
                <Grid item xs={12}>
                  <Paper elevation={0} sx={{ p: 2, backgroundColor: '#F5EFF7', borderRadius: 2, mb: 1 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 2 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, opacity: formData.category === 0 ? 1 : 0.5 }}>
                        <PersonIcon color={formData.category === 0 ? 'primary' : 'disabled'} />
                        <Typography fontWeight={formData.category === 0 ? 600 : 400}>Individual</Typography>
                      </Box>
                      <Switch
                        checked={formData.category === 1}
                        onChange={(e) => setFormData(prev => ({ ...prev, category: e.target.checked ? 1 : 0 }))}
                        disabled={!!editingId}
                        color="primary"
                      />
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, opacity: formData.category === 1 ? 1 : 0.5 }}>
                        <BusinessIcon color={formData.category === 1 ? 'primary' : 'disabled'} />
                        <Typography fontWeight={formData.category === 1 ? 600 : 400}>Organization</Typography>
                      </Box>
                    </Box>
                    {editingId && (
                      <Typography variant="caption" color="textSecondary" sx={{ display: 'block', textAlign: 'center', mt: 1 }}>
                        Category cannot be changed after creation
                      </Typography>
                    )}
                  </Paper>
                </Grid>
                
                <Grid item xs={12}>
                  <Divider sx={{ my: 1 }} />
                </Grid>

                {/* Individual Fields */}
                {formData.category === 0 && (
                  <>
                    <Grid item xs={2}>
                      <LookupSelect
                        category="Salutation"
                        name="salutation"
                        value={formData.salutation}
                        onChange={handleSelectChange}
                        label="Salutation"
                        fallback={[{ value: '', label: 'None' }, { value: 'Mr.', label: 'Mr.' }, { value: 'Mrs.', label: 'Mrs.' }, { value: 'Ms.', label: 'Ms.' }, { value: 'Dr.', label: 'Dr.' }, { value: 'Prof.', label: 'Prof.' }]}
                      />
                    </Grid>
                    <Grid item xs={4}>
                      <TextField fullWidth label="First Name" name="firstName" value={formData.firstName} onChange={handleInputChange} required />
                    </Grid>
                    <Grid item xs={4}>
                      <TextField fullWidth label="Last Name" name="lastName" value={formData.lastName} onChange={handleInputChange} required />
                    </Grid>
                    <Grid item xs={2}>
                      <TextField fullWidth label="Suffix" name="suffix" value={formData.suffix} onChange={handleInputChange} placeholder="Jr., III" />
                    </Grid>
                    <Grid item xs={6}>
                      <TextField fullWidth label="Date of Birth" name="dateOfBirth" type="date" value={formData.dateOfBirth} onChange={handleInputChange} InputLabelProps={{ shrink: true }} />
                    </Grid>
                    <Grid item xs={6}>
                      <LookupSelect
                        category="Gender"
                        name="gender"
                        value={formData.gender}
                        onChange={handleSelectChange}
                        label="Gender"
                        fallback={[{ value: '', label: 'Prefer not to say' }, { value: 'Male', label: 'Male' }, { value: 'Female', label: 'Female' }, { value: 'Other', label: 'Other' }]}
                      />
                    </Grid>
                  </>
                )}

                {/* Organization Fields */}
                {formData.category === 1 && (
                  <>
                    <Grid item xs={6}>
                      <TextField fullWidth label="Company Name" name="company" value={formData.company} onChange={handleInputChange} required />
                    </Grid>
                    <Grid item xs={6}>
                      <TextField fullWidth label="Legal Name" name="legalName" value={formData.legalName} onChange={handleInputChange} placeholder="Full legal entity name" />
                    </Grid>
                    <Grid item xs={6}>
                      <TextField fullWidth label="DBA Name" name="dbaName" value={formData.dbaName} onChange={handleInputChange} placeholder="Doing Business As" />
                    </Grid>
                    <Grid item xs={6}>
                      <TextField fullWidth label="Tax ID / EIN" name="taxId" value={formData.taxId} onChange={handleInputChange} />
                    </Grid>
                    <Grid item xs={6}>
                      <TextField fullWidth label="Registration Number" name="registrationNumber" value={formData.registrationNumber} onChange={handleInputChange} />
                    </Grid>
                    <Grid item xs={6}>
                      <TextField fullWidth label="Year Founded" name="yearFounded" type="number" value={formData.yearFounded || ''} onChange={handleInputChange} />
                    </Grid>
                  </>
                )}

                {/* Common Contact Fields */}
                <Grid item xs={12}>
                  <Divider sx={{ my: 1 }}>
                    <Typography variant="caption" color="textSecondary">Contact Information</Typography>
                  </Divider>
                </Grid>
                <Grid item xs={6}>
                  <TextField fullWidth label="Email" name="email" type="email" value={formData.email} onChange={handleInputChange} required />
                </Grid>
                <Grid item xs={6}>
                  <TextField fullWidth label="Secondary Email" name="secondaryEmail" type="email" value={formData.secondaryEmail} onChange={handleInputChange} />
                </Grid>
                <Grid item xs={6}>
                  <TextField fullWidth label="Phone" name="phone" value={formData.phone} onChange={handleInputChange} />
                </Grid>
                <Grid item xs={6}>
                  <TextField fullWidth label="Mobile Phone" name="mobilePhone" value={formData.mobilePhone} onChange={handleInputChange} />
                </Grid>
                {formData.category === 0 && (
                  <Grid item xs={6}>
                    <TextField fullWidth label="Job Title" name="jobTitle" value={formData.jobTitle} onChange={handleInputChange} />
                  </Grid>
                )}
                <Grid item xs={formData.category === 0 ? 6 : 12}>
                  <TextField fullWidth label="Website" name="website" value={formData.website} onChange={handleInputChange} />
                </Grid>

                {/* Address */}
                <Grid item xs={12}>
                  <Divider sx={{ my: 1 }}>
                    <Typography variant="caption" color="textSecondary">Address</Typography>
                  </Divider>
                </Grid>
                <Grid item xs={12}>
                  <TextField fullWidth label="Address" name="address" value={formData.address} onChange={handleInputChange} />
                </Grid>
                <Grid item xs={4}>
                  <TextField fullWidth label="City" name="city" value={formData.city} onChange={handleInputChange} />
                </Grid>
                <Grid item xs={4}>
                  <TextField fullWidth label="State" name="state" value={formData.state} onChange={handleInputChange} />
                </Grid>
                <Grid item xs={4}>
                  <TextField fullWidth label="Zip Code" name="zipCode" value={formData.zipCode} onChange={handleInputChange} />
                </Grid>
              </Grid>
            )}
          </TabPanel>

          <TabPanel value={dialogTab} index={1}>
            {fieldConfigsLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', py: 3 }}>
                <CircularProgress size={24} />
              </Box>
            ) : fieldConfigs.length ? (
              renderTabFields(1)
            ) : (
              <Grid container spacing={2}>
                <Grid item xs={6}>
                  <LookupSelect
                    category="CustomerType"
                    name="customerType"
                    value={formData.customerType}
                    onChange={handleSelectChange}
                    label="Customer Type"
                    fallback={CUSTOMER_TYPES.map(t => ({ value: t.value, label: t.label }))}
                  />
                </Grid>
                <Grid item xs={6}>
                  <LookupSelect
                    category="LifecycleStage"
                    name="lifecycleStage"
                    value={formData.lifecycleStage}
                    onChange={handleSelectChange}
                    label="Lifecycle Stage"
                    fallback={LIFECYCLE_STAGES.map(s => ({ value: s.value, label: s.label }))}
                  />
                </Grid>
                <Grid item xs={6}>
                  <LookupSelect
                    category="Priority"
                    name="priority"
                    value={formData.priority}
                    onChange={handleSelectChange}
                    label="Priority"
                    fallback={PRIORITIES.map(p => ({ value: p.value, label: p.label }))}
                  />
                </Grid>
                <Grid item xs={6}>
                  <LookupSelect
                    category="Industry"
                    name="industry"
                    value={formData.industry}
                    onChange={handleSelectChange}
                    label="Industry"
                    fallback={INDUSTRIES.map(i => ({ value: i, label: i }))}
                  />
                </Grid>
                <Grid item xs={6}>
                  <TextField fullWidth label="Annual Revenue ($)" name="annualRevenue" type="number" value={formData.annualRevenue} onChange={handleInputChange} />
                </Grid>
                <Grid item xs={6}>
                  <TextField fullWidth label="Number of Employees" name="numberOfEmployees" type="number" value={formData.numberOfEmployees} onChange={handleInputChange} />
                </Grid>
                <Grid item xs={6}>
                  <TextField fullWidth label="Credit Limit ($)" name="creditLimit" type="number" value={formData.creditLimit} onChange={handleInputChange} />
                </Grid>
                <Grid item xs={6}>
                  <LookupSelect
                    category="LeadSource"
                    name="leadSource"
                    value={formData.leadSource}
                    onChange={handleSelectChange}
                    label="Lead Source"
                    fallback={LEAD_SOURCES.map(s => ({ value: s, label: s }))}
                  />
                </Grid>
                <Grid item xs={12}>
                  <Typography gutterBottom>Lead Score: {formData.leadScore}</Typography>
                  <Slider
                    value={formData.leadScore}
                    onChange={(_, v) => setFormData(prev => ({ ...prev, leadScore: v as number }))}
                    min={0}
                    max={100}
                    valueLabelDisplay="auto"
                    sx={{ color: formData.leadScore > 70 ? '#4caf50' : formData.leadScore > 40 ? '#ff9800' : '#f44336' }}
                  />
                </Grid>
              </Grid>
            )}
          </TabPanel>

          <TabPanel value={dialogTab} index={2}>
            {fieldConfigsLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', py: 3 }}>
                <CircularProgress size={24} />
              </Box>
            ) : fieldConfigs.length ? (
              renderTabFields(2)
            ) : (
              <Grid container spacing={2}>
                <Grid item xs={6}>
                  <LookupSelect
                    category="PreferredContactMethod"
                    name="preferredContactMethod"
                    value={formData.preferredContactMethod}
                    onChange={handleSelectChange}
                    label="Preferred Contact Method"
                    fallback={[{ value: 'Email', label: 'Email' }, { value: 'Phone', label: 'Phone' }, { value: 'SMS', label: 'SMS' }, { value: 'Mail', label: 'Mail' }]}
                  />
                </Grid>
                <Grid item xs={6}>
                  <TextField fullWidth label="Timezone" name="timezone" value={formData.timezone} onChange={handleInputChange} placeholder="e.g., America/New_York" />
                </Grid>
                <Grid item xs={12}>
                  <Typography variant="subtitle2" gutterBottom>Communication Opt-In</Typography>
                  <FormControlLabel
                    control={<Checkbox name="optInEmail" checked={formData.optInEmail} onChange={handleInputChange} />}
                    label="Email Communications"
                  />
                  <FormControlLabel
                    control={<Checkbox name="optInPhone" checked={formData.optInPhone} onChange={handleInputChange} />}
                    label="Phone Calls"
                  />
                  <FormControlLabel
                    control={<Checkbox name="optInSms" checked={formData.optInSms} onChange={handleInputChange} />}
                    label="SMS Messages"
                  />
                </Grid>
                <Grid item xs={6}>
                  <TextField 
                    fullWidth 
                    label="LinkedIn URL" 
                    name="linkedInUrl" 
                    value={formData.linkedInUrl} 
                    onChange={handleInputChange}
                    InputProps={{ startAdornment: <LinkedInIcon sx={{ mr: 1, color: '#0077b5' }} /> }}
                  />
                </Grid>
                <Grid item xs={6}>
                  <TextField 
                    fullWidth 
                    label="Twitter Handle" 
                    name="twitterHandle" 
                    value={formData.twitterHandle} 
                    onChange={handleInputChange}
                    InputProps={{ startAdornment: <TwitterIcon sx={{ mr: 1, color: '#1da1f2' }} /> }}
                  />
                </Grid>
                <Grid item xs={12}>
                  <TextField fullWidth label="Website" name="website" value={formData.website} onChange={handleInputChange} />
                </Grid>
              </Grid>
            )}
          </TabPanel>

          {/* Linked Contacts Tab - Only for Organizations when editing */}
          {formData.category === 1 && editingId && (
            <TabPanel value={dialogTab} index={4}>
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
                  <Typography variant="caption" color="textSecondary">
                    Add contacts to track key people at this organization
                  </Typography>
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
                              {contact.isPrimaryContact && (
                                <Chip label="Primary" size="small" color="success" />
                              )}
                              {contact.isDecisionMaker && (
                                <Chip label="Decision Maker" size="small" color="warning" />
                              )}
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
                              {contact.positionAtCustomer && (
                                <Typography variant="caption" color="textSecondary">
                                  {contact.positionAtCustomer}
                                </Typography>
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

          <TabPanel value={dialogTab} index={3}>
            {fieldConfigsLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', py: 3 }}>
                <CircularProgress size={24} />
              </Box>
            ) : fieldConfigs.length ? (
              renderTabFields(3)
            ) : (
              <Grid container spacing={2}>
                <Grid item xs={6}>
                  <TextField fullWidth label="Territory" name="territory" value={formData.territory} onChange={handleInputChange} />
                </Grid>
                <Grid item xs={6}>
                  <TextField fullWidth label="Payment Terms" name="paymentTerms" value={formData.paymentTerms} onChange={handleInputChange} placeholder="e.g., Net 30" />
                </Grid>
                <Grid item xs={12}>
                  <TextField fullWidth label="Tags (comma-separated)" name="tags" value={formData.tags} onChange={handleInputChange} placeholder="vip, enterprise, priority" />
                </Grid>
                <Grid item xs={12}>
                  <TextField fullWidth label="Description" name="description" value={formData.description} onChange={handleInputChange} multiline rows={2} />
                </Grid>
                <Grid item xs={12}>
                  <TextField fullWidth label="Notes" name="notes" value={formData.notes} onChange={handleInputChange} multiline rows={3} />
                </Grid>
              </Grid>
            )}
          </TabPanel>
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
    </Box>
  );
}

export default CustomersPage;

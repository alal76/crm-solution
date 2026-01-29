import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  CircularProgress,
  Alert,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  MenuItem,
  Stack,
  Chip,
  Tabs,
  Tab,
  Tooltip,
  IconButton,
  Checkbox,
  Toolbar,
  Paper,
  FormControl,
  InputLabel,
  Select,
  Collapse,
  SelectChangeEvent,
} from '@mui/material';
import { useState, useEffect, useCallback, useMemo } from 'react';
import apiClient from '../services/apiClient';
import { getApiErrorMessage } from '../utils/errorHandler';
import lookupService, { LookupItem } from '../services/lookupService';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import LinkIcon from '@mui/icons-material/Link';
import ContactPhoneIcon from '@mui/icons-material/ContactPhone';
import PhoneIcon from '@mui/icons-material/Phone';
import EmailIcon from '@mui/icons-material/Email';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import BusinessIcon from '@mui/icons-material/Business';
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined';
import FilterListIcon from '@mui/icons-material/FilterList';
import CloseIcon from '@mui/icons-material/Close';
import NoteIcon from '@mui/icons-material/Note';
import logo from '../assets/logo.png';
import LookupSelect from '../components/LookupSelect';
import EntitySelect from '../components/EntitySelect';
import ImportExportButtons from '../components/ImportExportButtons';
import AdvancedSearch, { SearchField, SearchFilter, filterData } from '../components/AdvancedSearch';
import { ContactInfoPanel } from '../components/ContactInfo';
import NotesTab from '../components/NotesTab';
import { contactInfoService, EntityContactInfoDto, LinkedEmailDto, LinkedPhoneDto, LinkedAddressDto, LinkedSocialMediaDto } from '../services/contactInfoService';
import { useAccountContext } from '../contexts/AccountContextProvider';
import { useProfile } from '../contexts/ProfileContext';
import { BaseEntity } from '../types';
import { DialogError, DialogSuccess, ActionButton, StatusSnackbar } from '../components/common';
import { useApiState } from '../hooks/useApiState';
import { usePagination } from '../hooks/usePagination';
import { useEntityTypeSubscription } from '../hooks/useSignalR';

interface SocialMediaLink {
  id: number;
  platform: string;
  url: string;
  handle?: string;
}

// Contact info summary from the one-to-many relationships
interface ContactInfoSummary {
  primaryEmail?: string;
  primaryPhone?: string;
  primaryAddress?: string;
  emailCount: number;
  phoneCount: number;
  addressCount: number;
  socialCount: number;
}

interface Customer extends BaseEntity {
  firstName: string;
  lastName: string;
  company: string;
  displayName?: string;
}

interface Contact extends BaseEntity {
  contactType: string;
  firstName: string;
  lastName: string;
  middleName?: string;
  customerId?: number;  // Link to customer (one-to-many)
  // Legacy single fields (kept for backward compatibility)
  emailPrimary?: string;
  emailSecondary?: string;
  phonePrimary?: string;
  phoneSecondary?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  zipCode?: string;
  jobTitle?: string;
  department?: string;
  company?: string;
  reportsTo?: string;
  notes?: string;
  dateOfBirth?: string;
  dateAdded: string;
  lastModified?: string;
  // New: Contact info summary from one-to-many relationships
  contactInfoSummary?: ContactInfoSummary;
  modifiedBy?: string;
  socialMediaLinks: SocialMediaLink[];
  
  // Normalized Contact Info Collections (source of truth)
  emailAddresses?: LinkedEmailDto[];
  phoneNumbers?: LinkedPhoneDto[];
  addresses?: LinkedAddressDto[];
  socialMediaAccounts?: LinkedSocialMediaDto[];
}

interface CreateContactRequest {
  contactType: string;
  firstName: string;
  lastName: string;
  middleName?: string;
  jobTitle?: string;
  department?: string;
  company?: string;
  reportsTo?: string;
  notes?: string;
  dateOfBirth?: string;
  customerId?: number | '';
}

const CONTACT_TYPES = ['Employee', 'Customer', 'Partner', 'Lead', 'Vendor', 'Other'];

// Search fields configuration for contacts
const CONTACT_SEARCH_FIELDS: SearchField[] = [
  {
    name: 'contactType',
    label: 'Contact Type',
    type: 'select',
    options: CONTACT_TYPES.map((t) => ({ value: t, label: t })),
  },
  { name: 'firstName', label: 'First Name', type: 'text' },
  { name: 'lastName', label: 'Last Name', type: 'text' },
  { name: 'company', label: 'Company', type: 'text' },
  { name: 'jobTitle', label: 'Job Title', type: 'text' },
];

const CONTACT_SEARCHABLE_FIELDS = [
  'firstName', 'lastName', 'company', 'jobTitle'
];

function ContactsPage() {
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [contactInfoCache, setContactInfoCache] = useState<Record<number, ContactInfoSummary>>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filterType, setFilterType] = useState<string>('');
  const [searchFilters, setSearchFilters] = useState<SearchFilter[]>([]);
  const [searchText, setSearchText] = useState('');
  const [openDialog, setOpenDialog] = useState(false);
  const [dialogTab, setDialogTab] = useState(0);
  const [selectedContact, setSelectedContact] = useState<Contact | null>(null);
  
  // Multi-select and bulk update state
  const [selectedIds, setSelectedIds] = useState<number[]>([]);
  const [showFilters, setShowFilters] = useState(false);
  const [bulkDialogOpen, setBulkDialogOpen] = useState(false);
  const [bulkFormData, setBulkFormData] = useState({
    contactType: '' as string,
    company: '' as string,
    department: '' as string,
    customerId: '' as string | number,
  });
  
  // API state for dialog operations
  const dialogApi = useApiState({ successTimeout: 3000 });
  const bulkApi = useApiState({ successTimeout: 3000 });
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [formData, setFormData] = useState<CreateContactRequest>({
    contactType: 'Other',
    firstName: '',
    lastName: '',
  });
  const [lookupItems, setLookupItems] = useState<Record<string, LookupItem[]>>({});

  // Fetch contact info summaries for all contacts
  const fetchContactInfoSummaries = useCallback(async (contactList: Contact[]) => {
    const cache: Record<number, ContactInfoSummary> = {};
    await Promise.all(
      contactList.map(async (contact) => {
        try {
          const response = await contactInfoService.getEntityContactInfo('Contact', contact.id);
          const data = response.data;
          const primaryEmail = data.emailAddresses?.find((e) => e.isPrimary) || data.emailAddresses?.[0];
          const primaryPhone = data.phoneNumbers?.find((p) => p.isPrimary) || data.phoneNumbers?.[0];
          const primaryAddress = data.addresses?.find((a) => a.isPrimary) || data.addresses?.[0];
          cache[contact.id] = {
            primaryEmail: primaryEmail?.email,
            primaryPhone: primaryPhone?.formattedNumber || primaryPhone?.number,
            primaryAddress: primaryAddress?.formattedAddress || 
                           (primaryAddress ? `${primaryAddress.city}, ${primaryAddress.state}` : undefined),
            emailCount: data.emailAddresses?.length || 0,
            phoneCount: data.phoneNumbers?.length || 0,
            addressCount: data.addresses?.length || 0,
            socialCount: data.socialMediaAccounts?.length || 0,
          };
        } catch {
          cache[contact.id] = { emailCount: 0, phoneCount: 0, addressCount: 0, socialCount: 0 };
        }
      })
    );
    setContactInfoCache(cache);
  }, []);

  // Fetch contacts (defined early for SignalR callbacks)
  const fetchContacts = useCallback(async () => {
    try {
      setLoading(true);
      const endpoint = filterType ? `/contacts/type/${filterType}` : '/contacts';
      const response = await apiClient.get(endpoint);
      setContacts(response.data);
      // Fetch contact info summaries
      fetchContactInfoSummaries(response.data);
      setError(null);
    } catch (err: any) {
      setError(getApiErrorMessage(err, 'Failed to fetch contacts'));
      console.error('Error fetching contacts:', err);
    } finally {
      setLoading(false);
    }
  }, [filterType, fetchContactInfoSummaries]);

  // SignalR subscription for real-time updates
  useEntityTypeSubscription('Contact', {
    onCreated: useCallback(() => {
      console.log('[SignalR] Contact created - refreshing list');
      fetchContacts();
    }, [fetchContacts]),
    onUpdated: useCallback(() => {
      console.log('[SignalR] Contact updated - refreshing list');
      fetchContacts();
    }, [fetchContacts]),
    onDeleted: useCallback(() => {
      console.log('[SignalR] Contact deleted - refreshing list');
      fetchContacts();
    }, [fetchContacts]),
  });

  // Fetch contacts
  useEffect(() => {
    fetchContacts();
    fetchCustomers();
  }, [fetchContacts]);

  useEffect(() => {
    // preload contact-related lookups if available
    (async () => {
      try {
        const [types, socials] = await Promise.all([
          lookupService.getLookupItems('ContactType').catch(() => []),
          lookupService.getLookupItems('SocialMediaPlatform').catch(() => []),
        ]);
        setLookupItems({ ContactType: types, SocialMediaPlatform: socials });
      } catch (e) {
        // ignore
      }
    })();
  }, []);

  const fetchCustomers = async () => {
    try {
      const response = await apiClient.get('/customers');
      setCustomers(response.data);
    } catch (err) {
      console.error('Error fetching customers:', err);
    }
  };

  const getCustomerName = (customerId?: number) => {
    if (!customerId) return null;
    const customer = customers.find(c => c.id === customerId);
    return customer ? (customer.displayName || `${customer.firstName} ${customer.lastName}`.trim() || customer.company) : null;
  };

  const handleAddContact = () => {
    setFormData({
      contactType: 'Other',
      firstName: '',
      lastName: '',
    });
    setSelectedContact(null);
    setDialogTab(0);
    setOpenDialog(true);
  };

  const handleEditContact = (contact: Contact) => {
    setFormData({
      contactType: contact.contactType,
      firstName: contact.firstName,
      lastName: contact.lastName,
      middleName: contact.middleName,
      jobTitle: contact.jobTitle,
      department: contact.department,
      company: contact.company,
      reportsTo: contact.reportsTo,
      notes: contact.notes,
      dateOfBirth: contact.dateOfBirth,
    });
    setSelectedContact(contact);
    setDialogTab(0);
    setOpenDialog(true);
  };

  const handleDeleteContact = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this contact?')) {
      const result = await dialogApi.execute(async () => {
        await apiClient.delete(`/contacts/${id}`);
        return true;
      }, 'Contact deleted successfully');
      
      if (result) {
        setContacts(contacts.filter((c) => c.id !== id));
        setSelectedIds(prev => prev.filter(sid => sid !== id));
        setSuccessMessage('Contact deleted successfully');
        setTimeout(() => setSuccessMessage(null), 3000);
      } else {
        setError(getApiErrorMessage(dialogApi.error, 'Failed to delete contact'));
      }
    }
  };

  const handleSaveContact = async () => {
    if (!formData.firstName || !formData.lastName) {
      dialogApi.setError('First name and last name are required');
      return;
    }

    const result = await dialogApi.execute(async () => {
      let savedContactId: number;
      if (selectedContact) {
        // Update
        await apiClient.put(`/contacts/${selectedContact.id}`, formData);
        savedContactId = selectedContact.id;
      } else {
        // Create - then open for editing to add contact info
        const response = await apiClient.post('/contacts', formData);
        savedContactId = response.data.id;
      }
      return savedContactId;
    }, selectedContact ? 'Contact updated successfully' : 'Contact created successfully');

    if (result) {
      await fetchContacts();
      setSuccessMessage(selectedContact ? 'Contact updated successfully' : 'Contact created successfully');
      setTimeout(() => setSuccessMessage(null), 3000);

      // If creating new, switch to edit mode so user can add contact info
      if (!selectedContact && result) {
        const newContact = contacts.find(c => c.id === result) || 
                          { ...formData, id: result, dateAdded: new Date().toISOString(), socialMediaLinks: [] } as Contact;
        setSelectedContact({ ...newContact, id: result } as Contact);
        setDialogTab(1); // Switch to Contact Info tab
      } else {
        setOpenDialog(false);
      }
    }
    // Error is handled by dialogApi.error which is displayed in dialog
  };

  // Bulk update handlers
  const handleSelectAll = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.checked) {
      setSelectedIds(filteredContacts.map(c => c.id));
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
      contactType: '',
      company: '',
      department: '',
      customerId: '',
    });
    bulkApi.clearError();
    setBulkDialogOpen(true);
  };

  const handleBulkUpdate = async () => {
    if (selectedIds.length === 0) {
      bulkApi.setError('No contacts selected');
      return;
    }

    // Build update payload only with non-empty fields
    const updatePayload: Record<string, any> = {};
    if (bulkFormData.contactType) updatePayload.contactType = bulkFormData.contactType;
    if (bulkFormData.company) updatePayload.company = bulkFormData.company;
    if (bulkFormData.department) updatePayload.department = bulkFormData.department;
    if (bulkFormData.customerId) updatePayload.customerId = Number(bulkFormData.customerId);

    if (Object.keys(updatePayload).length === 0) {
      bulkApi.setError('Please select at least one field to update');
      return;
    }

    const result = await bulkApi.execute(async () => {
      // Update each selected contact
      const updatePromises = selectedIds.map(id =>
        apiClient.put(`/contacts/${id}`, updatePayload)
      );
      await Promise.all(updatePromises);
      return selectedIds.length;
    }, `Successfully updated ${selectedIds.length} contact(s)`);

    if (result) {
      await fetchContacts();
      setBulkDialogOpen(false);
      setSelectedIds([]);
      setSuccessMessage(`Successfully updated ${result} contact(s)`);
      setTimeout(() => setSuccessMessage(null), 3000);
    }
    // Error is kept in dialog if update fails
  };

  const handleBulkDelete = async () => {
    if (selectedIds.length === 0) return;
    
    if (!window.confirm(`Are you sure you want to delete ${selectedIds.length} contact(s)?`)) {
      return;
    }

    const result = await bulkApi.execute(async () => {
      const deletePromises = selectedIds.map(id => apiClient.delete(`/contacts/${id}`));
      await Promise.all(deletePromises);
      return selectedIds.length;
    }, `Successfully deleted ${selectedIds.length} contact(s)`);

    if (result) {
      await fetchContacts();
      setSelectedIds([]);
      setSuccessMessage(`Successfully deleted ${result} contact(s)`);
      setTimeout(() => setSuccessMessage(null), 3000);
    } else {
      setError(getApiErrorMessage(bulkApi.error, 'Failed to delete some contacts'));
    }
  };

  const handleFormChange = (e: any) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const getContactTypeColor = (type: string) => {
    const colors: { [key: string]: 'success' | 'primary' | 'warning' | 'error' | 'info' | 'default' } = {
      Employee: 'success',
      Customer: 'primary',
      Partner: 'warning',
      Lead: 'info',
      Vendor: 'error',
      Other: 'default',
    };
    return colors[type] || 'default';
  };

  // Get contact info display with counts
  const getContactInfoDisplay = (contactId: number) => {
    const info = contactInfoCache[contactId];
    if (!info) return null;
    
    return (
      <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
        {info.emailCount > 0 && (
          <Tooltip title={info.primaryEmail || `${info.emailCount} email(s)`}>
            <Chip
              icon={<EmailIcon />}
              label={info.emailCount}
              size="small"
              variant="outlined"
              color="primary"
            />
          </Tooltip>
        )}
        {info.phoneCount > 0 && (
          <Tooltip title={info.primaryPhone || `${info.phoneCount} phone(s)`}>
            <Chip
              icon={<PhoneIcon />}
              label={info.phoneCount}
              size="small"
              variant="outlined"
              color="success"
            />
          </Tooltip>
        )}
        {info.addressCount > 0 && (
          <Tooltip title={info.primaryAddress || `${info.addressCount} address(es)`}>
            <Chip
              icon={<LocationOnIcon />}
              label={info.addressCount}
              size="small"
              variant="outlined"
              color="warning"
            />
          </Tooltip>
        )}
        {info.socialCount > 0 && (
          <Tooltip title={`${info.socialCount} social media account(s)`}>
            <Chip
              icon={<LinkIcon />}
              label={info.socialCount}
              size="small"
              variant="outlined"
              color="info"
            />
          </Tooltip>
        )}
      </Box>
    );
  };

  const handleSearch = (filters: SearchFilter[], text: string) => {
    setSearchFilters(filters);
    setSearchText(text);
  };

  // Get account context for filtering
  const { selectedAccounts, isContextActive, getAccountIds } = useAccountContext();
  const { hasPermission } = useProfile();

  // Apply filters, search text, AND account context to contacts
  const filteredContacts = useMemo(() => {
    let result = contacts;
    
    // Apply account context filter first (filter by customerId)
    if (isContextActive) {
      const accountIds = getAccountIds();
      result = result.filter(contact => contact.customerId && accountIds.includes(contact.customerId));
    }
    
    // Then apply search filters
    return filterData(result, searchFilters, searchText, CONTACT_SEARCHABLE_FIELDS);
  }, [contacts, searchFilters, searchText, isContextActive, getAccountIds]);

  const {
    page,
    pageSize,
    paginatedData: paginatedContacts,
    handlePageChange,
    handlePageSizeChange,
    pageSizeOptions,
  } = usePagination(filteredContacts, { defaultPageSize: 25 });

  return (
    <Box sx={{ py: 4 }}>
      <Container maxWidth="lg">
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ width: 40, height: 40, flexShrink: 0 }}><img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} /></Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>
              Contacts
            </Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
            <ImportExportButtons
              entityType="contacts"
              entityLabel="Contacts"
              onImportComplete={fetchContacts}
            />
            <Button
              variant="contained"
              color="primary"
              startIcon={<AddIcon />}
              onClick={handleAddContact}
            >
              Add Contact
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
          fields={CONTACT_SEARCH_FIELDS}
          onSearch={handleSearch}
          placeholder="Search contacts by name, email, company..."
        />

        {/* Bulk Actions Toolbar */}
        <Collapse in={selectedIds.length > 0}>
          <Paper sx={{ mb: 2, p: 2, backgroundColor: 'primary.light' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <Typography sx={{ color: 'primary.contrastText' }}>
                {selectedIds.length} contact(s) selected
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
                {hasPermission('canDeleteContacts') && (
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

        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Card>
            <CardContent>
              <TableContainer sx={{ overflowX: 'auto' }}>
                <Table sx={{ minWidth: 1000 }}>
                <TableHead>
                  <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                    <TableCell padding="checkbox">
                      <Checkbox
                        indeterminate={selectedIds.length > 0 && selectedIds.length < filteredContacts.length}
                        checked={filteredContacts.length > 0 && selectedIds.length === filteredContacts.length}
                        onChange={handleSelectAll}
                      />
                    </TableCell>
                    <TableCell><strong>Name</strong></TableCell>
                    <TableCell><strong>Type</strong></TableCell>
                    <TableCell><strong>Customer</strong></TableCell>
                    <TableCell><strong>Company</strong></TableCell>
                    <TableCell><strong>Job Title</strong></TableCell>
                    <TableCell><strong>Contact Info</strong></TableCell>
                    <TableCell><strong>Actions</strong></TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {paginatedContacts.map((contact) => (
                    <TableRow 
                      key={contact.id}
                      selected={selectedIds.includes(contact.id)}
                      hover
                    >
                      <TableCell padding="checkbox">
                        <Checkbox
                          checked={selectedIds.includes(contact.id)}
                          onChange={() => handleSelectOne(contact.id)}
                        />
                      </TableCell>
                      <TableCell>
                        {contact.firstName} {contact.lastName}
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={contact.contactType}
                          color={getContactTypeColor(contact.contactType)}
                          size="small"
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell>
                        {contact.customerId ? (
                          <Chip
                            label={getCustomerName(contact.customerId) || 'Unknown'}
                            size="small"
                            color="primary"
                            variant="outlined"
                          />
                        ) : (
                          <Typography sx={{ fontSize: '0.875rem', color: 'textSecondary' }}>-</Typography>
                        )}
                      </TableCell>
                      <TableCell>{contact.company || '-'}</TableCell>
                      <TableCell>{contact.jobTitle || '-'}</TableCell>
                      <TableCell>
                        {getContactInfoDisplay(contact.id) || (
                          <Typography sx={{ fontSize: '0.875rem', color: 'textSecondary' }}>
                            No contact info
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell>
                        <Stack direction="row" spacing={1}>
                          <Button
                            size="small"
                            startIcon={<EditIcon />}
                            onClick={() => handleEditContact(contact)}
                            variant="outlined"
                          >
                            Edit
                          </Button>
                          <Tooltip title="Manage Contact Info">
                            <IconButton
                              size="small"
                              color="primary"
                              onClick={() => {
                                setSelectedContact(contact);
                                setDialogTab(1);
                                setOpenDialog(true);
                              }}
                            >
                              <ContactPhoneIcon />
                            </IconButton>
                          </Tooltip>
                          <Button
                            size="small"
                            startIcon={<DeleteIcon />}
                            color="error"
                            onClick={() => handleDeleteContact(contact.id)}
                            variant="outlined"
                          >
                            Delete
                          </Button>
                        </Stack>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
                </Table>
              </TableContainer>
              <TablePagination
                component="div"
                count={filteredContacts.length}
                page={page}
                onPageChange={handlePageChange}
                rowsPerPage={pageSize}
                onRowsPerPageChange={handlePageSizeChange}
                rowsPerPageOptions={pageSizeOptions}
                showFirstButton
                showLastButton
              />
              {filteredContacts.length === 0 && !loading && (
                <Typography sx={{ textAlign: 'center', py: 2, color: 'textSecondary' }}>
                  No contacts found
                </Typography>
              )}
            </CardContent>
          </Card>
        )}
      </Container>

      {/* Contact Form Dialog */}
      <Dialog open={openDialog} onClose={() => { dialogApi.clearError(); setOpenDialog(false); }} maxWidth="md" fullWidth>
        <DialogTitle>
          {selectedContact ? 'Edit Contact' : 'Add New Contact'}
        </DialogTitle>
        <Box sx={{ borderBottom: 1, borderColor: 'divider', px: 3 }}>
          <Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)}>
            <Tab label="Basic Info" />
            <Tab 
              label="Contact Info" 
              icon={<ContactPhoneIcon fontSize="small" />} 
              iconPosition="start"
              disabled={!selectedContact}
            />
            <Tab 
              label="Notes" 
              icon={<NoteIcon fontSize="small" />} 
              iconPosition="start"
            />
          </Tabs>
        </Box>
        <DialogContent sx={{ pt: 2, minHeight: 400 }}>
          {/* Error Display */}
          <DialogError 
            error={dialogApi.error} 
            onClose={dialogApi.clearError}
          />
          <DialogSuccess 
            message={dialogApi.success} 
            onClose={dialogApi.clearSuccess}
          />
          
          {/* Basic Info Tab */}
          {dialogTab === 0 && (
            <Stack spacing={2}>
              <LookupSelect
                category="ContactType"
                name="contactType"
                value={formData.contactType}
                onChange={handleFormChange}
                label="Contact Type"
                fallback={CONTACT_TYPES.map(t => ({ value: t, label: t }))}
              />

              <TextField
                label="First Name"
                name="firstName"
                value={formData.firstName}
                onChange={handleFormChange}
                fullWidth
                required
              />

              <TextField
                label="Last Name"
                name="lastName"
                value={formData.lastName}
                onChange={handleFormChange}
                fullWidth
                required
              />

              <TextField
                label="Middle Name"
                name="middleName"
                value={formData.middleName || ''}
                onChange={handleFormChange}
                fullWidth
              />

              <TextField
                label="Company"
                name="company"
                value={formData.company || ''}
                onChange={handleFormChange}
                fullWidth
              />

              <TextField
                label="Job Title"
                name="jobTitle"
                value={formData.jobTitle || ''}
                onChange={handleFormChange}
                fullWidth
              />

              <EntitySelect
                entityType="customer"
                name="customerId"
                value={formData.customerId || ''}
                onChange={(e) => setFormData({ ...formData, customerId: e.target.value ? Number(e.target.value) : '' })}
                label="Owner Customer"
                showAddNew={true}
              />

              <TextField
                label="Department"
                name="department"
                value={formData.department || ''}
                onChange={handleFormChange}
                fullWidth
              />

              <TextField
                label="Reports To"
                name="reportsTo"
                value={formData.reportsTo || ''}
                onChange={handleFormChange}
                fullWidth
              />

              <TextField
                label="Date of Birth"
                name="dateOfBirth"
                type="date"
                value={formData.dateOfBirth || ''}
                onChange={handleFormChange}
                fullWidth
                InputLabelProps={{ shrink: true }}
              />

              <TextField
                label="Notes"
                name="notes"
                value={formData.notes || ''}
                onChange={handleFormChange}
                fullWidth
                multiline
                rows={3}
              />

              {!selectedContact && (
                <Alert severity="info" icon={<InfoOutlinedIcon />}>
                  Save the contact first to add addresses, phone numbers, emails, and social media accounts.
                </Alert>
              )}
            </Stack>
          )}

          {/* Contact Info Tab - Only when editing */}
          {dialogTab === 1 && selectedContact && (
            <Box>
              <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 2 }}>
                Manage Contact Information
              </Typography>
              <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
                Add and manage multiple addresses, phone numbers, emails, and social media accounts.
              </Typography>
              <ContactInfoPanel
                entityType="Contact"
                entityId={selectedContact.id}
                layout="tabs"
                showCounts={true}
                onContactInfoChange={() => fetchContactInfoSummaries(contacts)}
              />
            </Box>
          )}

          {/* Notes Tab */}
          {dialogTab === 2 && (
            <Box>
              {selectedContact ? (
                <NotesTab
                  entityType="Contact"
                  entityId={selectedContact.id}
                  entityName={`${selectedContact.firstName} ${selectedContact.lastName}`}
                />
              ) : (
                <Alert severity="info">
                  Save the contact first to add notes.
                </Alert>
              )}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => { dialogApi.clearError(); setOpenDialog(false); }}>Cancel</Button>
          <ActionButton
            label={selectedContact ? 'Update' : 'Create'}
            loading={dialogApi.loading}
            onClick={handleSaveContact}
            color="primary"
          />
        </DialogActions>
      </Dialog>

      {/* Bulk Update Dialog */}
      <Dialog open={bulkDialogOpen} onClose={() => { bulkApi.clearError(); setBulkDialogOpen(false); }} maxWidth="sm" fullWidth>
        <DialogTitle>
          Bulk Update {selectedIds.length} Contact(s)
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
              <InputLabel>Contact Type</InputLabel>
              <Select
                value={bulkFormData.contactType}
                label="Contact Type"
                onChange={(e: SelectChangeEvent) => setBulkFormData(prev => ({ ...prev, contactType: e.target.value }))}
              >
                <MenuItem value="">-- No Change --</MenuItem>
                {CONTACT_TYPES.map(type => (
                  <MenuItem key={type} value={type}>{type}</MenuItem>
                ))}
              </Select>
            </FormControl>
            
            <TextField
              label="Company"
              size="small"
              value={bulkFormData.company}
              onChange={(e) => setBulkFormData(prev => ({ ...prev, company: e.target.value }))}
              placeholder="Leave empty to keep current value"
              fullWidth
            />
            
            <TextField
              label="Department"
              size="small"
              value={bulkFormData.department}
              onChange={(e) => setBulkFormData(prev => ({ ...prev, department: e.target.value }))}
              placeholder="Leave empty to keep current value"
              fullWidth
            />
            
            <EntitySelect
              entityType="customer"
              name="customerId"
              value={bulkFormData.customerId}
              onChange={(e) => setBulkFormData(prev => ({ ...prev, customerId: e.target.value }))}
              label="Owner Customer"
              showAddNew={false}
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

export default ContactsPage;

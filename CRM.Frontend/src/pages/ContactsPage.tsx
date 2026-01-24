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
} from '@mui/material';
import { useState, useEffect, useCallback } from 'react';
import apiClient from '../services/apiClient';
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
import logo from '../assets/logo.png';
import LookupSelect from '../components/LookupSelect';
import EntitySelect from '../components/EntitySelect';
import ImportExportButtons from '../components/ImportExportButtons';
import AdvancedSearch, { SearchField, SearchFilter, filterData } from '../components/AdvancedSearch';
import { ContactInfoPanel } from '../components/ContactInfo';
import { contactInfoService, EntityContactInfoDto, LinkedEmailDto, LinkedPhoneDto, LinkedAddressDto, LinkedSocialMediaDto } from '../services/contactInfoService';

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

interface Customer {
  id: number;
  firstName: string;
  lastName: string;
  company: string;
  displayName?: string;
}

interface Contact {
  id: number;
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

  // Fetch contacts
  useEffect(() => {
    fetchContacts();
    fetchCustomers();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filterType]);

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

  const fetchContacts = async () => {
    try {
      setLoading(true);
      const endpoint = filterType ? `/contacts/type/${filterType}` : '/contacts';
      const response = await apiClient.get(endpoint);
      setContacts(response.data);
      // Fetch contact info summaries
      fetchContactInfoSummaries(response.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch contacts');
      console.error('Error fetching contacts:', err);
    } finally {
      setLoading(false);
    }
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
      try {
        await apiClient.delete(`/contacts/${id}`);
        setContacts(contacts.filter((c) => c.id !== id));
        setError(null);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete contact');
        console.error('Error deleting contact:', err);
      }
    }
  };

  const handleSaveContact = async () => {
    try {
      if (!formData.firstName || !formData.lastName) {
        setError('First name and last name are required');
        return;
      }

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

      await fetchContacts();
      setError(null);

      // If creating new, switch to edit mode so user can add contact info
      if (!selectedContact && savedContactId) {
        const newContact = contacts.find(c => c.id === savedContactId) || 
                          { ...formData, id: savedContactId, dateAdded: new Date().toISOString(), socialMediaLinks: [] } as Contact;
        setSelectedContact({ ...newContact, id: savedContactId } as Contact);
        setDialogTab(1); // Switch to Contact Info tab
      } else {
        setOpenDialog(false);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save contact');
      console.error('Error saving contact:', err);
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

  // Apply filters and search text to contacts
  const filteredContacts = filterData(contacts, searchFilters, searchText, CONTACT_SEARCHABLE_FIELDS);

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

        {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}

        <AdvancedSearch
          fields={CONTACT_SEARCH_FIELDS}
          onSearch={handleSearch}
          placeholder="Search contacts by name, email, company..."
        />

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
                  {filteredContacts.map((contact) => (
                    <TableRow key={contact.id}>
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
      <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="md" fullWidth>
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
          </Tabs>
        </Box>
        <DialogContent sx={{ pt: 2, minHeight: 400 }}>
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
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDialog(false)}>Cancel</Button>
          <Button onClick={handleSaveContact} variant="contained" color="primary">
            {selectedContact ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default ContactsPage;

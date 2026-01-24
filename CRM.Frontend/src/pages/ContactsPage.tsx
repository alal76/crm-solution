import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  Table,
  TableBody,
  TableCell,
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
} from '@mui/material';
import { useState, useEffect } from 'react';
import apiClient from '../services/apiClient';
import lookupService, { LookupItem } from '../services/lookupService';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import LinkIcon from '@mui/icons-material/Link';
import ContactPhoneIcon from '@mui/icons-material/ContactPhone';
import logo from '../assets/logo.png';
import LookupSelect from '../components/LookupSelect';
import ImportExportButtons from '../components/ImportExportButtons';
import AdvancedSearch, { SearchField, SearchFilter, filterData } from '../components/AdvancedSearch';
import { ContactInfoPanel } from '../components/ContactInfo';

interface SocialMediaLink {
  id: number;
  platform: string;
  url: string;
  handle?: string;
}

interface Contact {
  id: number;
  contactType: string;
  firstName: string;
  lastName: string;
  middleName?: string;
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
  modifiedBy?: string;
  socialMediaLinks: SocialMediaLink[];
}

interface CreateContactRequest {
  contactType: string;
  firstName: string;
  lastName: string;
  middleName?: string;
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
}

const CONTACT_TYPES = ['Employee', 'Customer', 'Partner', 'Lead', 'Vendor', 'Other'];
const SOCIAL_MEDIA_PLATFORMS = ['LinkedIn', 'Twitter', 'Facebook', 'Instagram', 'GitHub', 'Website', 'Other'];

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
  { name: 'emailPrimary', label: 'Email', type: 'text' },
  { name: 'company', label: 'Company', type: 'text' },
  { name: 'jobTitle', label: 'Job Title', type: 'text' },
  { name: 'city', label: 'City', type: 'text' },
  { name: 'state', label: 'State', type: 'text' },
  { name: 'country', label: 'Country', type: 'text' },
];

const CONTACT_SEARCHABLE_FIELDS = [
  'firstName', 'lastName', 'emailPrimary', 'phonePrimary', 'company', 'jobTitle', 'city', 'state', 'country'
];

function ContactsPage() {
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filterType, setFilterType] = useState<string>('');
  const [searchFilters, setSearchFilters] = useState<SearchFilter[]>([]);
  const [searchText, setSearchText] = useState('');
  const [openDialog, setOpenDialog] = useState(false);
  const [dialogTab, setDialogTab] = useState(0);
  const [selectedContact, setSelectedContact] = useState<Contact | null>(null);
  const [socialMediaDialog, setSocialMediaDialog] = useState(false);
  const [selectedContactForSocial, setSelectedContactForSocial] = useState<Contact | null>(null);
  const [formData, setFormData] = useState<CreateContactRequest>({
    contactType: 'Other',
    firstName: '',
    lastName: '',
  });
  const [lookupItems, setLookupItems] = useState<Record<string, LookupItem[]>>({});
  const [socialMediaData, setSocialMediaData] = useState({
    platform: 'LinkedIn',
    url: '',
    handle: '',
  });

  // Fetch contacts
  useEffect(() => {
    fetchContacts();
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

  const fetchContacts = async () => {
    try {
      setLoading(true);
      const endpoint = filterType ? `/contacts/type/${filterType}` : '/contacts';
      const response = await apiClient.get(endpoint);
      setContacts(response.data);
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
      emailPrimary: contact.emailPrimary,
      emailSecondary: contact.emailSecondary,
      phonePrimary: contact.phonePrimary,
      phoneSecondary: contact.phoneSecondary,
      address: contact.address,
      city: contact.city,
      state: contact.state,
      country: contact.country,
      zipCode: contact.zipCode,
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

      if (selectedContact) {
        // Update
        await apiClient.put(`/contacts/${selectedContact.id}`, formData);
      } else {
        // Create
        await apiClient.post('/contacts', formData);
      }

      setOpenDialog(false);
      await fetchContacts();
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save contact');
      console.error('Error saving contact:', err);
    }
  };

  const handleAddSocialMedia = async () => {
    try {
      if (!selectedContactForSocial || !socialMediaData.url) {
        setError('Contact and URL are required');
        return;
      }

      await apiClient.post(`/contacts/${selectedContactForSocial.id}/social-media`, socialMediaData);

      setSocialMediaDialog(false);
      setSocialMediaData({ platform: 'LinkedIn', url: '', handle: '' });
      await fetchContacts();
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to add social media link');
      console.error('Error adding social media:', err);
    }
  };

  const handleRemoveSocialMedia = async (linkId: number) => {
    try {
      await apiClient.delete(`/contacts/social-media/${linkId}`);
      await fetchContacts();
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to remove social media link');
      console.error('Error removing social media:', err);
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

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

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
              <Table>
                <TableHead>
                  <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                    <TableCell><strong>Name</strong></TableCell>
                    <TableCell><strong>Type</strong></TableCell>
                    <TableCell><strong>Email</strong></TableCell>
                    <TableCell><strong>Phone</strong></TableCell>
                    <TableCell><strong>Company</strong></TableCell>
                    <TableCell><strong>Job Title</strong></TableCell>
                    <TableCell><strong>Social Media</strong></TableCell>
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
                      <TableCell>{contact.emailPrimary || '-'}</TableCell>
                      <TableCell>{contact.phonePrimary || '-'}</TableCell>
                      <TableCell>{contact.company || '-'}</TableCell>
                      <TableCell>{contact.jobTitle || '-'}</TableCell>
                      <TableCell>
                        {contact.socialMediaLinks && contact.socialMediaLinks.length > 0 ? (
                          <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                            {contact.socialMediaLinks.map((link) => (
                              <Chip
                                key={link.id}
                                icon={<LinkIcon />}
                                label={link.platform}
                                size="small"
                                variant="outlined"
                                onDelete={() => handleRemoveSocialMedia(link.id)}
                                onClick={() => window.open(link.url, '_blank')}
                                sx={{ cursor: 'pointer' }}
                              />
                            ))}
                          </Box>
                        ) : (
                          <Typography sx={{ fontSize: '0.875rem', color: 'textSecondary' }}>-</Typography>
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
                          <Button
                            size="small"
                            startIcon={<LinkIcon />}
                            onClick={() => {
                              setSelectedContactForSocial(contact);
                              setSocialMediaDialog(true);
                            }}
                            variant="outlined"
                          >
                            Add Social
                          </Button>
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
            <Tab label="Work Info" />
            <Tab label="Address" />
            {selectedContact && <Tab label="Contact Info" icon={<ContactPhoneIcon fontSize="small" />} iconPosition="start" />}
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
                label="Email (Primary)"
                name="emailPrimary"
                type="email"
                value={formData.emailPrimary || ''}
                onChange={handleFormChange}
                fullWidth
              />

              <TextField
                label="Phone (Primary)"
                name="phonePrimary"
                value={formData.phonePrimary || ''}
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
            </Stack>
          )}

          {/* Work Info Tab */}
          {dialogTab === 1 && (
            <Stack spacing={2}>
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
            </Stack>
          )}

          {/* Address Tab */}
          {dialogTab === 2 && (
            <Stack spacing={2}>
              <TextField
                label="Address"
                name="address"
                value={formData.address || ''}
                onChange={handleFormChange}
                fullWidth
              />

              <TextField
                label="City"
                name="city"
                value={formData.city || ''}
                onChange={handleFormChange}
                fullWidth
              />

              <TextField
                label="State"
                name="state"
                value={formData.state || ''}
                onChange={handleFormChange}
                fullWidth
              />

              <TextField
                label="Country"
                name="country"
                value={formData.country || ''}
                onChange={handleFormChange}
                fullWidth
              />

              <TextField
                label="Zip Code"
                name="zipCode"
                value={formData.zipCode || ''}
                onChange={handleFormChange}
                fullWidth
              />
            </Stack>
          )}

          {/* Contact Info Tab - Only when editing */}
          {dialogTab === 3 && selectedContact && (
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

      {/* Social Media Dialog */}
      <Dialog open={socialMediaDialog} onClose={() => setSocialMediaDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Add Social Media Link</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <Stack spacing={2}>
            <LookupSelect
              category="SocialMediaPlatform"
              name="platform"
              value={socialMediaData.platform}
              onChange={(e:any) => setSocialMediaData({ ...socialMediaData, platform: e.target.value })}
              label="Platform"
              fallback={SOCIAL_MEDIA_PLATFORMS.map(p => ({ value: p, label: p }))}
            />

            <TextField
              label="URL"
              value={socialMediaData.url}
              onChange={(e) => setSocialMediaData({ ...socialMediaData, url: e.target.value })}
              fullWidth
              required
            />

            <TextField
              label="Handle (Username)"
              value={socialMediaData.handle}
              onChange={(e) => setSocialMediaData({ ...socialMediaData, handle: e.target.value })}
              fullWidth
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSocialMediaDialog(false)}>Cancel</Button>
          <Button onClick={handleAddSocialMedia} variant="contained" color="primary">
            Add
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default ContactsPage;

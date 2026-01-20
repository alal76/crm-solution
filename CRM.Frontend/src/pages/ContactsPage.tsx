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
} from '@mui/material';
import { useState, useEffect } from 'react';
import apiClient from '../services/apiClient';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import LinkIcon from '@mui/icons-material/Link';
import logo from '../assets/logo.png';

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

function ContactsPage() {
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filterType, setFilterType] = useState<string>('');
  const [openDialog, setOpenDialog] = useState(false);
  const [selectedContact, setSelectedContact] = useState<Contact | null>(null);
  const [socialMediaDialog, setSocialMediaDialog] = useState(false);
  const [selectedContactForSocial, setSelectedContactForSocial] = useState<Contact | null>(null);
  const [formData, setFormData] = useState<CreateContactRequest>({
    contactType: 'Other',
    firstName: '',
    lastName: '',
  });
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

  const filteredContacts = filterType
    ? contacts
    : contacts;

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
          <Button
            variant="contained"
            color="primary"
            startIcon={<AddIcon />}
            onClick={handleAddContact}
          >
            Add Contact
          </Button>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        <Card sx={{ mb: 3 }}>
          <CardContent>
            <TextField
              select
              label="Filter by Type"
              value={filterType}
              onChange={(e) => setFilterType(e.target.value)}
              size="small"
              sx={{ minWidth: 200 }}
            >
              <MenuItem value="">All Types</MenuItem>
              {CONTACT_TYPES.map((type) => (
                <MenuItem key={type} value={type}>
                  {type}
                </MenuItem>
              ))}
            </TextField>
          </CardContent>
        </Card>

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
      <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          {selectedContact ? 'Edit Contact' : 'Add New Contact'}
        </DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <Stack spacing={2}>
            <TextField
              select
              label="Contact Type"
              name="contactType"
              value={formData.contactType}
              onChange={handleFormChange}
              fullWidth
            >
              {CONTACT_TYPES.map((type) => (
                <MenuItem key={type} value={type}>
                  {type}
                </MenuItem>
              ))}
            </TextField>

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
              label="Email (Secondary)"
              name="emailSecondary"
              type="email"
              value={formData.emailSecondary || ''}
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
              label="Phone (Secondary)"
              name="phoneSecondary"
              value={formData.phoneSecondary || ''}
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
            <TextField
              select
              label="Platform"
              value={socialMediaData.platform}
              onChange={(e) => setSocialMediaData({ ...socialMediaData, platform: e.target.value })}
              fullWidth
            >
              {SOCIAL_MEDIA_PLATFORMS.map((platform) => (
                <MenuItem key={platform} value={platform}>
                  {platform}
                </MenuItem>
              ))}
            </TextField>

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

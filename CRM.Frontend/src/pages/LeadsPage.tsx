import { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Alert,
  IconButton,
  Chip,
  CircularProgress,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  SelectChangeEvent,
} from '@mui/material';
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  Add as AddIcon,
  PersonAdd as PersonAddIcon,
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';
import LookupSelect from '../components/LookupSelect';

// Lead sources for the dropdown
const LEAD_SOURCES = [
  { value: 'website', label: 'Website', bg: '#E3F2FD', text: '#1565C0' },
  { value: 'referral', label: 'Referral', bg: '#F3E5F5', text: '#6A1B9A' },
  { value: 'event', label: 'Event', bg: '#E0F2F1', text: '#00695C' },
  { value: 'cold_call', label: 'Cold Call', bg: '#FFF3E0', text: '#E65100' },
  { value: 'social', label: 'Social Media', bg: '#FCE4EC', text: '#C2185B' },
  { value: 'other', label: 'Other', bg: '#F0F4C3', text: '#558B2F' },
];

// Lead status options (stored in Notes field since Contact model doesn't have status)
const LEAD_STATUSES = [
  { value: 'new', label: 'New', bg: '#E8DEF8', text: '#6750A4' },
  { value: 'contacted', label: 'Contacted', bg: '#E1F5FE', text: '#0277BD' },
  { value: 'qualified', label: 'Qualified', bg: '#E8F5E9', text: '#06A77D' },
  { value: 'converted', label: 'Converted', bg: '#F1F8E9', text: '#558B2F' },
  { value: 'lost', label: 'Lost', bg: '#FFEBEE', text: '#B3261E' },
];

interface Lead {
  id: number;
  firstName: string;
  lastName: string;
  emailPrimary: string;
  phonePrimary: string;
  company: string;
  jobTitle: string;
  source: string;
  status: string;
  notes: string;
  dateAdded: string;
  contactType: number;
}

interface LeadFormData {
  firstName: string;
  lastName: string;
  emailPrimary: string;
  phonePrimary: string;
  company: string;
  jobTitle: string;
  source: string;
  status: string;
  notes: string;
}

function LeadsPage() {
  const [leads, setLeads] = useState<Lead[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<LeadFormData>({
    firstName: '',
    lastName: '',
    emailPrimary: '',
    phonePrimary: '',
    company: '',
    jobTitle: '',
    source: 'website',
    status: 'new',
    notes: '',
  });

  useEffect(() => {
    fetchLeads();
  }, []);

  const fetchLeads = async () => {
    try {
      setLoading(true);
      // Leads are Contacts with ContactType = Lead (value 2)
      const response = await apiClient.get('/contacts/type/Lead');
      // Parse source and status from notes field (stored as JSON)
      const leadsWithMeta = response.data.map((contact: any) => {
        let source = 'other';
        let status = 'new';
        let notes = contact.notes || '';
        
        // Try to parse metadata from notes
        try {
          if (notes.startsWith('{')) {
            const meta = JSON.parse(notes);
            source = meta.source || 'other';
            status = meta.status || 'new';
            notes = meta.notes || '';
          }
        } catch {
          // Notes is plain text, keep defaults
        }
        
        return {
          ...contact,
          source,
          status,
          notes,
        };
      });
      setLeads(leadsWithMeta);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch leads');
      console.error('Error fetching leads:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = (lead?: Lead) => {
    if (lead) {
      setEditingId(lead.id);
      setFormData({
        firstName: lead.firstName,
        lastName: lead.lastName,
        emailPrimary: lead.emailPrimary || '',
        phonePrimary: lead.phonePrimary || '',
        company: lead.company || '',
        jobTitle: lead.jobTitle || '',
        source: lead.source || 'website',
        status: lead.status || 'new',
        notes: lead.notes || '',
      });
    } else {
      setEditingId(null);
      setFormData({
        firstName: '',
        lastName: '',
        emailPrimary: '',
        phonePrimary: '',
        company: '',
        jobTitle: '',
        source: 'website',
        status: 'new',
        notes: '',
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
    setError(null);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSelectChange = (e: SelectChangeEvent<string>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSave = async () => {
    if (!formData.firstName.trim() || !formData.lastName.trim() || !formData.emailPrimary.trim()) {
      setError('Please fill in required fields (First Name, Last Name, Email)');
      return;
    }

    try {
      // Store source, status, and notes as JSON in notes field
      const notesWithMeta = JSON.stringify({
        source: formData.source,
        status: formData.status,
        notes: formData.notes,
      });

      const contactData = {
        firstName: formData.firstName,
        lastName: formData.lastName,
        emailPrimary: formData.emailPrimary,
        phonePrimary: formData.phonePrimary,
        company: formData.company,
        jobTitle: formData.jobTitle,
        contactType: 2, // Lead
        notes: notesWithMeta,
      };

      if (editingId) {
        await apiClient.put(`/contacts/${editingId}`, contactData);
        setSuccessMessage('Lead updated successfully');
      } else {
        await apiClient.post('/contacts', contactData);
        setSuccessMessage('Lead created successfully');
      }
      
      handleCloseDialog();
      fetchLeads();
      
      // Clear success message after 3 seconds
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save lead');
      console.error('Error saving lead:', err);
    }
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this lead?')) {
      try {
        await apiClient.delete(`/contacts/${id}`);
        setSuccessMessage('Lead deleted successfully');
        fetchLeads();
        
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete lead');
        console.error('Error deleting lead:', err);
      }
    }
  };

  const handleConvertToCustomer = async (lead: Lead) => {
    if (window.confirm(`Convert ${lead.firstName} ${lead.lastName} to a Customer?`)) {
      try {
        // Create customer from lead data
        await apiClient.post('/customers', {
          name: `${lead.firstName} ${lead.lastName}`,
          company: lead.company || `${lead.firstName}'s Company`,
          email: lead.emailPrimary,
          phone: lead.phonePrimary,
          industry: 'Other',
          lifecycleStage: 1, // Customer
          notes: `Converted from lead. ${lead.notes}`,
        });

        // Update lead status to converted
        const notesWithMeta = JSON.stringify({
          source: lead.source,
          status: 'converted',
          notes: lead.notes,
        });

        await apiClient.put(`/contacts/${lead.id}`, {
          firstName: lead.firstName,
          lastName: lead.lastName,
          emailPrimary: lead.emailPrimary,
          phonePrimary: lead.phonePrimary,
          company: lead.company,
          jobTitle: lead.jobTitle,
          contactType: 2,
          notes: notesWithMeta,
        });

        setSuccessMessage('Lead converted to customer successfully!');
        fetchLeads();
        
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to convert lead');
        console.error('Error converting lead:', err);
      }
    }
  };

  const getSourceStyle = (source: string) => {
    const sourceInfo = LEAD_SOURCES.find(s => s.value === source);
    return sourceInfo ? { bg: sourceInfo.bg, text: sourceInfo.text } : { bg: '#F0F4C3', text: '#558B2F' };
  };

  const getStatusStyle = (status: string) => {
    const statusInfo = LEAD_STATUSES.find(s => s.value === status);
    return statusInfo ? { bg: statusInfo.bg, text: statusInfo.text } : { bg: '#E8DEF8', text: '#6750A4' };
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ py: 2 }}>
      <Box sx={{ mb: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
            <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
          </Box>
          <Box>
            <Typography variant="h3" sx={{ fontWeight: 700, mb: 0.5 }}>
              Leads
            </Typography>
            <Typography color="textSecondary" variant="body2">
              Manage and track your sales leads
            </Typography>
          </Box>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenDialog()}
          sx={{ backgroundColor: '#6750A4', textTransform: 'none', borderRadius: 2 }}
        >
          Add Lead
        </Button>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
      {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

      <Card sx={{ borderRadius: 3, boxShadow: 1 }}>
        <CardContent sx={{ p: 0 }}>
          <Table>
            <TableHead>
              <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Name</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Email</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Company</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Source</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Status</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Date Added</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }} align="center">
                  Actions
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {leads.map((lead) => {
                const sourceStyle = getSourceStyle(lead.source);
                const statusStyle = getStatusStyle(lead.status);
                return (
                  <TableRow
                    key={lead.id}
                    sx={{
                      '&:hover': { backgroundColor: '#F5EFF7' },
                      borderBottom: '1px solid #E8DEF8',
                    }}
                  >
                    <TableCell sx={{ fontWeight: 500 }}>
                      {lead.firstName} {lead.lastName}
                      {lead.jobTitle && (
                        <Typography variant="caption" display="block" color="textSecondary">
                          {lead.jobTitle}
                        </Typography>
                      )}
                    </TableCell>
                    <TableCell>{lead.emailPrimary}</TableCell>
                    <TableCell>{lead.company || '—'}</TableCell>
                    <TableCell>
                      <Chip
                        label={LEAD_SOURCES.find(s => s.value === lead.source)?.label || lead.source}
                        size="small"
                        sx={{
                          backgroundColor: sourceStyle.bg,
                          color: sourceStyle.text,
                          fontWeight: 600,
                        }}
                      />
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={LEAD_STATUSES.find(s => s.value === lead.status)?.label || lead.status}
                        size="small"
                        sx={{
                          backgroundColor: statusStyle.bg,
                          color: statusStyle.text,
                          fontWeight: 600,
                        }}
                      />
                    </TableCell>
                    <TableCell>
                      {lead.dateAdded ? new Date(lead.dateAdded).toLocaleDateString() : '—'}
                    </TableCell>
                    <TableCell align="center">
                      {lead.status !== 'converted' && (
                        <IconButton
                          size="small"
                          onClick={() => handleConvertToCustomer(lead)}
                          sx={{ color: '#06A77D' }}
                          title="Convert to Customer"
                        >
                          <PersonAddIcon fontSize="small" />
                        </IconButton>
                      )}
                      <IconButton
                        size="small"
                        onClick={() => handleOpenDialog(lead)}
                        sx={{ color: '#6750A4' }}
                        title="Edit Lead"
                      >
                        <EditIcon fontSize="small" />
                      </IconButton>
                      <IconButton
                        size="small"
                        onClick={() => handleDelete(lead.id)}
                        sx={{ color: '#B3261E' }}
                        title="Delete Lead"
                      >
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                );
              })}
              {leads.length === 0 && (
                <TableRow>
                  <TableCell colSpan={7} align="center" sx={{ py: 4 }}>
                    <Typography color="textSecondary">No leads found. Add your first lead to get started.</Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Add/Edit Lead Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 600, color: '#6750A4' }}>
          {editingId ? 'Edit Lead' : 'Add Lead'}
        </DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              fullWidth
              label="First Name"
              name="firstName"
              value={formData.firstName}
              onChange={handleInputChange}
              margin="normal"
              required
            />
            <TextField
              fullWidth
              label="Last Name"
              name="lastName"
              value={formData.lastName}
              onChange={handleInputChange}
              margin="normal"
              required
            />
          </Box>
          <TextField
            fullWidth
            label="Email"
            name="emailPrimary"
            type="email"
            value={formData.emailPrimary}
            onChange={handleInputChange}
            margin="normal"
            required
          />
          <TextField
            fullWidth
            label="Phone"
            name="phonePrimary"
            value={formData.phonePrimary}
            onChange={handleInputChange}
            margin="normal"
          />
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              fullWidth
              label="Company"
              name="company"
              value={formData.company}
              onChange={handleInputChange}
              margin="normal"
            />
            <TextField
              fullWidth
              label="Job Title"
              name="jobTitle"
              value={formData.jobTitle}
              onChange={handleInputChange}
              margin="normal"
            />
          </Box>
          <Box sx={{ display: 'flex', gap: 2, mt: 1 }}>
            <LookupSelect
              category="LeadSource"
              name="source"
              value={formData.source}
              onChange={handleSelectChange}
              label="Lead Source"
              fallback={LEAD_SOURCES.map(s => ({ value: s.value, label: s.label }))}
            />
            <LookupSelect
              category="LeadStatus"
              name="status"
              value={formData.status}
              onChange={handleSelectChange}
              label="Status"
              fallback={LEAD_STATUSES.map(s => ({ value: s.value, label: s.label }))}
            />
          </Box>
          <TextField
            fullWidth
            label="Notes"
            name="notes"
            value={formData.notes}
            onChange={handleInputChange}
            margin="normal"
            multiline
            rows={3}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button
            onClick={handleSave}
            variant="contained"
            sx={{ backgroundColor: '#6750A4', textTransform: 'none' }}
          >
            {editingId ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default LeadsPage;

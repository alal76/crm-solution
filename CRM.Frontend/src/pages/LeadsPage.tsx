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
  Tabs,
  Tab,
  Stack,
  Checkbox,
  Paper,
  Collapse,
  Container,
  TableContainer,
} from '@mui/material';
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  Add as AddIcon,
  PersonAdd as PersonAddIcon,
  ContactPhone as ContactPhoneIcon,
  Close as CloseIcon,
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';
import LookupSelect from '../components/LookupSelect';
import { ContactInfoPanel } from '../components/ContactInfo';
import { BaseEntity } from '../types';
import { useProfile } from '../contexts/ProfileContext';
import { DialogError, DialogSuccess, ActionButton } from '../components/common';
import { useApiState } from '../hooks/useApiState';
import AdvancedSearch, { SearchField, SearchFilter, filterData } from '../components/AdvancedSearch';

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

interface Lead extends BaseEntity {
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

// Search fields for leads
const SEARCH_FIELDS: SearchField[] = [
  { name: 'firstName', label: 'First Name', type: 'text' },
  { name: 'lastName', label: 'Last Name', type: 'text' },
  { name: 'company', label: 'Company', type: 'text' },
  { name: 'source', label: 'Source', type: 'select', options: LEAD_SOURCES.map(s => ({ value: s.value, label: s.label })) },
  { name: 'status', label: 'Status', type: 'select', options: LEAD_STATUSES.map(s => ({ value: s.value, label: s.label })) },
];

const SEARCHABLE_FIELDS = ['firstName', 'lastName', 'company', 'emailPrimary', 'jobTitle'];

function LeadsPage() {
  const [leads, setLeads] = useState<Lead[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [dialogTab, setDialogTab] = useState(0);
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
  
  // Search and filter state
  const [searchFilters, setSearchFilters] = useState<SearchFilter[]>([]);
  const [searchText, setSearchText] = useState('');
  
  // Multi-select and bulk update state
  const [selectedIds, setSelectedIds] = useState<number[]>([]);
  const [bulkDialogOpen, setBulkDialogOpen] = useState(false);
  const [bulkFormData, setBulkFormData] = useState({
    source: '' as string,
    status: '' as string,
    company: '' as string,
  });
  
  // API state for dialog operations
  const dialogApi = useApiState({ successTimeout: 3000 });
  const bulkApi = useApiState({ successTimeout: 3000 });
  const { hasPermission } = useProfile();
  
  // Filter leads based on search
  const filteredLeads = useMemo(() => {
    return filterData(leads, searchFilters, searchText, SEARCHABLE_FIELDS);
  }, [leads, searchFilters, searchText]);

  const handleSearch = (filters: SearchFilter[], text: string) => {
    setSearchFilters(filters);
    setSearchText(text);
  };

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
    setDialogTab(0);
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
    dialogApi.clearError();
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
      dialogApi.setError('Please fill in required fields (First Name, Last Name, Email)');
      return;
    }

    const result = await dialogApi.execute(async () => {
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
        return 'updated';
      } else {
        await apiClient.post('/contacts', contactData);
        return 'created';
      }
    }, editingId ? 'Lead updated successfully' : 'Lead created successfully');

    if (result) {
      handleCloseDialog();
      fetchLeads();
      setSuccessMessage(result === 'updated' ? 'Lead updated successfully' : 'Lead created successfully');
      setTimeout(() => setSuccessMessage(null), 3000);
    }
    // Error stays in dialog
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this lead?')) {
      const result = await dialogApi.execute(async () => {
        await apiClient.delete(`/contacts/${id}`);
        return true;
      }, 'Lead deleted successfully');
      
      if (result) {
        setSelectedIds(prev => prev.filter(sid => sid !== id));
        fetchLeads();
        setSuccessMessage('Lead deleted successfully');
        setTimeout(() => setSuccessMessage(null), 3000);
      } else {
        setError(dialogApi.error?.message || 'Failed to delete lead');
      }
    }
  };

  // Multi-select handlers
  const handleSelectAll = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.checked) {
      setSelectedIds(filteredLeads.map(l => l.id));
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
      source: '',
      status: '',
      company: '',
    });
    bulkApi.clearError();
    setBulkDialogOpen(true);
  };

  const handleBulkUpdate = async () => {
    if (selectedIds.length === 0) {
      bulkApi.setError('No leads selected');
      return;
    }

    const result = await bulkApi.execute(async () => {
      // Get current leads data and update only non-empty fields
      const updatePromises = selectedIds.map(async (id) => {
        const lead = leads.find(l => l.id === id);
        if (!lead) return;

        const currentMeta = { source: lead.source, status: lead.status, notes: lead.notes };
        const newMeta = {
          source: bulkFormData.source || currentMeta.source,
          status: bulkFormData.status || currentMeta.status,
          notes: currentMeta.notes,
        };

        const updatePayload: Record<string, any> = {
          notes: JSON.stringify(newMeta),
        };
        if (bulkFormData.company) updatePayload.company = bulkFormData.company;

        return apiClient.put(`/contacts/${id}`, updatePayload);
      });
      await Promise.all(updatePromises);
      return selectedIds.length;
    }, `Successfully updated ${selectedIds.length} lead(s)`);

    if (result) {
      fetchLeads();
      setBulkDialogOpen(false);
      setSelectedIds([]);
      setSuccessMessage(`Successfully updated ${result} lead(s)`);
      setTimeout(() => setSuccessMessage(null), 3000);
    }
  };

  const handleBulkDelete = async () => {
    if (selectedIds.length === 0) return;
    
    if (!window.confirm(`Are you sure you want to delete ${selectedIds.length} lead(s)?`)) {
      return;
    }

    const result = await bulkApi.execute(async () => {
      const deletePromises = selectedIds.map(id => apiClient.delete(`/contacts/${id}`));
      await Promise.all(deletePromises);
      return selectedIds.length;
    }, `Successfully deleted ${selectedIds.length} lead(s)`);

    if (result) {
      fetchLeads();
      setSelectedIds([]);
      setSuccessMessage(`Successfully deleted ${result} lead(s)`);
      setTimeout(() => setSuccessMessage(null), 3000);
    } else {
      setError(bulkApi.error?.message || 'Failed to delete some leads');
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
    <Container maxWidth="lg" sx={{ py: 2 }}>
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
      {successMessage && <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccessMessage(null)}>{successMessage}</Alert>}

      {/* Search */}
      <AdvancedSearch
        fields={SEARCH_FIELDS}
        onSearch={handleSearch}
        placeholder="Search leads by name, email, company..."
      />

      {/* Bulk Actions Toolbar */}
      <Collapse in={selectedIds.length > 0}>
        <Paper sx={{ mb: 2, p: 2, backgroundColor: 'primary.light' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
            <Typography sx={{ color: 'primary.contrastText' }}>
              {selectedIds.length} lead(s) selected
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
              {hasPermission('canDeleteLeads') && (
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

      <Card sx={{ borderRadius: 3, boxShadow: 1 }}>
        <CardContent sx={{ p: 0 }}>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                  <TableCell padding="checkbox">
                    <Checkbox
                      indeterminate={selectedIds.length > 0 && selectedIds.length < filteredLeads.length}
                      checked={filteredLeads.length > 0 && selectedIds.length === filteredLeads.length}
                      onChange={handleSelectAll}
                    />
                  </TableCell>
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
                {filteredLeads.map((lead) => {
                  const sourceStyle = getSourceStyle(lead.source);
                  const statusStyle = getStatusStyle(lead.status);
                  return (
                    <TableRow
                      key={lead.id}
                      hover
                      selected={selectedIds.includes(lead.id)}
                      sx={{
                        borderBottom: '1px solid #E8DEF8',
                      }}
                    >
                      <TableCell padding="checkbox">
                        <Checkbox
                          checked={selectedIds.includes(lead.id)}
                          onChange={() => handleSelectOne(lead.id)}
                        />
                      </TableCell>
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
              {filteredLeads.length === 0 && (
                <TableRow>
                  <TableCell colSpan={8} align="center" sx={{ py: 4 }}>
                    <Typography color="textSecondary">No leads found. Add your first lead to get started.</Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
          </TableContainer>
        </CardContent>
      </Card>

      {/* Add/Edit Lead Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle sx={{ fontWeight: 600, color: '#6750A4', pb: 0 }}>
          {editingId ? 'Edit Lead' : 'Add Lead'}
        </DialogTitle>
        <Box sx={{ borderBottom: 1, borderColor: 'divider', px: 3 }}>
          <Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)}>
            <Tab label="Lead Info" />
            {editingId && <Tab label="Contact Info" icon={<ContactPhoneIcon fontSize="small" />} iconPosition="start" />}
          </Tabs>
        </Box>
        <DialogContent sx={{ pt: 2, minHeight: 350 }}>
          {/* Error Display */}
          <DialogError 
            error={dialogApi.error} 
            onClose={dialogApi.clearError}
          />

          {/* Lead Info Tab */}
          {dialogTab === 0 && (
            <Stack spacing={2}>
              <Box sx={{ display: 'flex', gap: 2 }}>
                <TextField
                  fullWidth
                  label="First Name"
                  name="firstName"
                  value={formData.firstName}
                  onChange={handleInputChange}
                  required
                />
                <TextField
                  fullWidth
                  label="Last Name"
                  name="lastName"
                  value={formData.lastName}
                  onChange={handleInputChange}
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
                required
              />
              <TextField
                fullWidth
                label="Phone"
                name="phonePrimary"
                value={formData.phonePrimary}
                onChange={handleInputChange}
              />
              <Box sx={{ display: 'flex', gap: 2 }}>
                <TextField
                  fullWidth
                  label="Company"
                  name="company"
                  value={formData.company}
                  onChange={handleInputChange}
                />
                <TextField
                  fullWidth
                  label="Job Title"
                  name="jobTitle"
                  value={formData.jobTitle}
                  onChange={handleInputChange}
                />
              </Box>
              <Box sx={{ display: 'flex', gap: 2 }}>
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
                multiline
                rows={3}
              />
            </Stack>
          )}

          {/* Contact Info Tab - Only when editing */}
          {dialogTab === 1 && editingId && (
            <Box>
              <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 2 }}>
                Manage Contact Information
              </Typography>
              <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
                Add and manage multiple addresses, phone numbers, emails, and social media accounts for this lead.
              </Typography>
              <ContactInfoPanel
                entityType="Lead"
                entityId={editingId}
                layout="tabs"
                showCounts={true}
              />
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <ActionButton
            label={editingId ? 'Update' : 'Create'}
            loading={dialogApi.loading}
            onClick={handleSave}
            color="primary"
          />
        </DialogActions>
      </Dialog>

      {/* Bulk Update Dialog */}
      <Dialog open={bulkDialogOpen} onClose={() => { bulkApi.clearError(); setBulkDialogOpen(false); }} maxWidth="sm" fullWidth>
        <DialogTitle>
          Bulk Update {selectedIds.length} Lead(s)
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
              <InputLabel>Source</InputLabel>
              <Select
                value={bulkFormData.source}
                label="Source"
                onChange={(e: SelectChangeEvent) => setBulkFormData(prev => ({ ...prev, source: e.target.value }))}
              >
                <MenuItem value="">-- No Change --</MenuItem>
                {LEAD_SOURCES.map(source => (
                  <MenuItem key={source.value} value={source.value}>{source.label}</MenuItem>
                ))}
              </Select>
            </FormControl>
            
            <FormControl fullWidth size="small">
              <InputLabel>Status</InputLabel>
              <Select
                value={bulkFormData.status}
                label="Status"
                onChange={(e: SelectChangeEvent) => setBulkFormData(prev => ({ ...prev, status: e.target.value }))}
              >
                <MenuItem value="">-- No Change --</MenuItem>
                {LEAD_STATUSES.map(status => (
                  <MenuItem key={status.value} value={status.value}>{status.label}</MenuItem>
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
    </Container>
  );
}

export default LeadsPage;

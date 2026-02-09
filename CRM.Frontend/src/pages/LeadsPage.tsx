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
  TableHead,
  TableRow,
  TablePagination,
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
  Note as NoteIcon,
  TrendingUp as TrendingUpIcon,
  Psychology as PsychologyIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import logger from '../services/logger';
import logo from '../assets/logo.png';
import LookupSelect from '../components/LookupSelect';
import { ContactInfoPanel } from '../components/ContactInfo';
import NotesTab from '../components/NotesTab';
import { BaseEntity } from '../types';
import { useProfile } from '../contexts/ProfileContext';
import { 
  DialogError, 
  DialogSuccess, 
  ActionButton,
  DialogHeader,
  RelatedEntitiesPanel,
  EnhancedEmptyState,
} from '../components/common';
import { useApiState } from '../hooks/useApiState';
import { usePagination } from '../hooks/usePagination';
import { useEntityTypeSubscription } from '../hooks/useSignalR';
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
  email?: string;
  phonePrimary: string;
  phone?: string;
  company: string;
  companyName?: string;
  jobTitle: string;
  title?: string;
  source: string;
  status: string;
  notes: string;
  dateAdded: string;
  contactType: number;
  // AI scoring fields
  score?: number;
  fitScore?: number;
  engagementScore?: number;
}

// Helper to get score color based on value
const getScoreColor = (score: number | undefined): string => {
  if (!score || score === 0) return '#9E9E9E';
  if (score >= 80) return '#2E7D32';
  if (score >= 60) return '#06A77D';
  if (score >= 40) return '#ED6C02';
  if (score >= 20) return '#EF6C00';
  return '#D32F2F';
};

const getScoreLabel = (score: number | undefined): string => {
  if (!score || score === 0) return 'Not scored';
  if (score >= 80) return 'Hot';
  if (score >= 60) return 'Warm';
  if (score >= 40) return 'Mild';
  if (score >= 20) return 'Cool';
  return 'Cold';
};

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
  const convertApi = useApiState({ successTimeout: 3000 });
  const { hasPermission } = useProfile();
  
  // Lead conversion dialog state
  const [convertDialogOpen, setConvertDialogOpen] = useState(false);
  const [convertingLead, setConvertingLead] = useState<Lead | null>(null);
  const [convertFormData, setConvertFormData] = useState({
    createOpportunity: false,
    opportunityName: '',
    estimatedValue: '',
    expectedCloseDate: '',
  });
  
  // Fetch leads function (defined early for SignalR callbacks)
  const fetchLeads = useCallback(async () => {
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
  }, []);

  // SignalR subscription for real-time updates (Leads are stored as Contacts)
  useEntityTypeSubscription('Contact', {
    onCreated: useCallback(() => {
      logger.debug('[SignalR] Contact/Lead created - refreshing list');
      fetchLeads();
    }, [fetchLeads]),
    onUpdated: useCallback(() => {
      logger.debug('[SignalR] Contact/Lead updated - refreshing list');
      fetchLeads();
    }, [fetchLeads]),
    onDeleted: useCallback(() => {
      logger.debug('[SignalR] Contact/Lead deleted - refreshing list');
      fetchLeads();
    }, [fetchLeads]),
  });
  
  // Filter leads based on search
  const filteredLeads = useMemo(() => {
    return filterData(leads, searchFilters, searchText, SEARCHABLE_FIELDS);
  }, [leads, searchFilters, searchText]);

  const {
    page,
    pageSize,
    paginatedData: paginatedLeads,
    handlePageChange,
    handlePageSizeChange,
    pageSizeOptions,
  } = usePagination(filteredLeads, { defaultPageSize: 25 });

  const handleSearch = (filters: SearchFilter[], text: string) => {
    setSearchFilters(filters);
    setSearchText(text);
  };

  useEffect(() => {
    fetchLeads();
  }, [fetchLeads]);

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

  // Open conversion dialog
  const handleOpenConvertDialog = (lead: Lead) => {
    setConvertingLead(lead);
    setConvertFormData({
      createOpportunity: false,
      opportunityName: `${lead.company || lead.firstName} - Opportunity`,
      estimatedValue: '',
      expectedCloseDate: new Date(Date.now() + 90 * 24 * 60 * 60 * 1000).toISOString().split('T')[0], // 90 days out
    });
    convertApi.clearError();
    setConvertDialogOpen(true);
  };

  const handleCloseConvertDialog = () => {
    setConvertDialogOpen(false);
    setConvertingLead(null);
    convertApi.clearError();
  };

  const handleConvertLead = async () => {
    if (!convertingLead) return;

    const result = await convertApi.execute(async () => {
      // 1. Create account/customer from lead data
      const accountResponse = await apiClient.post('/accounts', {
        firstName: convertingLead.firstName,
        lastName: convertingLead.lastName,
        company: convertingLead.company || `${convertingLead.firstName}'s Company`,
        legalName: convertingLead.company,
        industry: 'Other',
        lifecycleStage: 1, // Customer
        notes: `Converted from lead. ${convertingLead.notes}`,
      });

      const accountId = accountResponse.data.id;
      let opportunityId = null;

      // 2. Optionally create opportunity
      if (convertFormData.createOpportunity) {
        const oppResponse = await apiClient.post('/opportunities', {
          name: convertFormData.opportunityName,
          accountId: accountId,
          stage: 0, // Prospecting
          probability: 20,
          amount: parseFloat(convertFormData.estimatedValue) || 0,
          currency: 'USD',
          expectedCloseDate: convertFormData.expectedCloseDate || null,
          leadId: convertingLead.id,
        });
        opportunityId = oppResponse.data.id;
      }

      // 3. Update lead status to converted
      const notesWithMeta = JSON.stringify({
        source: convertingLead.source,
        status: 'converted',
        notes: convertingLead.notes,
      });

      await apiClient.put(`/contacts/${convertingLead.id}`, {
        firstName: convertingLead.firstName,
        lastName: convertingLead.lastName,
        emailPrimary: convertingLead.emailPrimary,
        phonePrimary: convertingLead.phonePrimary,
        company: convertingLead.company,
        jobTitle: convertingLead.jobTitle,
        contactType: 2,
        notes: notesWithMeta,
      });

      return { accountId, opportunityId };
    }, convertFormData.createOpportunity 
      ? 'Lead converted to account with opportunity!'
      : 'Lead converted to account successfully!');

    if (result) {
      handleCloseConvertDialog();
      fetchLeads();
      setSuccessMessage(
        convertFormData.createOpportunity
          ? `Lead converted! Account #${result.accountId} and Opportunity #${result.opportunityId} created.`
          : `Lead converted! Account #${result.accountId} created.`
      );
      setTimeout(() => setSuccessMessage(null), 5000);
    }
  };

  const handleConvertToCustomer = async (lead: Lead) => {
    if (window.confirm(`Convert ${lead.firstName} ${lead.lastName} to a Customer?`)) {
      try {
        // Create customer from lead data
        await apiClient.post('/accounts', {
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

  // AI Lead Scoring
  const [scoringLeadId, setScoringLeadId] = useState<number | null>(null);

  const handleScoreLead = async (leadId: number) => {
    try {
      setScoringLeadId(leadId);
      const response = await apiClient.post(`/ai/leads/${leadId}/score`);
      if (response.data.success && response.data.score) {
        // Update local lead with new score
        setLeads(prev => prev.map(l => 
          l.id === leadId 
            ? { ...l, score: response.data.score.score }
            : l
        ));
        setSuccessMessage(`Lead scored: ${response.data.score.score}/100`);
        setTimeout(() => setSuccessMessage(null), 3000);
      } else {
        setError('Failed to score lead - AI service may not be configured');
      }
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to score lead');
    } finally {
      setScoringLeadId(null);
    }
  };

  const handleBatchScoreLeads = async () => {
    if (selectedIds.length === 0) return;
    
    try {
      setLoading(true);
      const response = await apiClient.post('/ai/leads/batch-score', { leadIds: selectedIds });
      if (response.data.success) {
        // Update leads with new scores
        const scoresMap = new Map<number, number>(
          response.data.scores?.map((s: { leadId: number; score: number }) => [s.leadId, s.score] as [number, number]) || []
        );
        setLeads(prev => prev.map(l => 
          scoresMap.has(l.id)
            ? { ...l, score: scoresMap.get(l.id) as number }
            : l
        ));
        setSuccessMessage(`Scored ${response.data.scoredCount} leads`);
        setTimeout(() => setSuccessMessage(null), 3000);
      } else {
        setError('Failed to batch score leads');
      }
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to batch score leads');
    } finally {
      setLoading(false);
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
                startIcon={<PsychologyIcon />}
                onClick={handleBatchScoreLeads}
                sx={{ backgroundColor: '#1976D2', color: 'white', '&:hover': { backgroundColor: '#1565C0' } }}
              >
                AI Score
              </Button>
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
                  <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Score</TableCell>
                  <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Date Added</TableCell>
                  <TableCell sx={{ fontWeight: 600, color: '#6750A4' }} align="center">
                    Actions
                  </TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {paginatedLeads.map((lead) => {
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
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                          <Box
                            sx={{
                              width: 28,
                              height: 28,
                              borderRadius: '50%',
                              backgroundColor: getScoreColor(lead.score) + '20',
                              border: `2px solid ${getScoreColor(lead.score)}`,
                              display: 'flex',
                              alignItems: 'center',
                              justifyContent: 'center',
                              fontWeight: 700,
                              fontSize: '0.7rem',
                              color: getScoreColor(lead.score),
                            }}
                          >
                            {lead.score ?? '—'}
                          </Box>
                          <Typography variant="caption" sx={{ color: getScoreColor(lead.score), fontWeight: 500 }}>
                            {getScoreLabel(lead.score)}
                          </Typography>
                        </Box>
                      </TableCell>
                      <TableCell>
                        {lead.dateAdded ? new Date(lead.dateAdded).toLocaleDateString() : '—'}
                      </TableCell>
                      <TableCell align="center">
                        {lead.status !== 'converted' && (
                          <IconButton
                            size="small"
                            onClick={() => handleScoreLead(lead.id)}
                            sx={{ color: '#1976D2' }}
                            title="AI Score Lead"
                            disabled={scoringLeadId === lead.id}
                          >
                            {scoringLeadId === lead.id ? (
                              <CircularProgress size={16} />
                            ) : (
                              <PsychologyIcon fontSize="small" />
                            )}
                          </IconButton>
                        )}
                        {lead.status !== 'converted' && (
                          <IconButton
                            size="small"
                            onClick={() => handleOpenConvertDialog(lead)}
                            sx={{ color: '#06A77D' }}
                            title="Convert to Account"
                          >
                            <TrendingUpIcon fontSize="small" />
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
                        aria-label={`Delete lead ${lead.firstName} ${lead.lastName}`}
                      >
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                );
              })}
              {filteredLeads.length === 0 && (
                <TableRow>
                  <TableCell colSpan={9} align="center" sx={{ py: 0 }}>
                    <EnhancedEmptyState
                      illustration="leads"
                      variant={searchText || searchFilters.length > 0 ? 'no-results' : 'no-data'}
                      title={searchText || searchFilters.length > 0 ? 'No leads match your search' : undefined}
                      primaryActionLabel={searchText || searchFilters.length > 0 ? 'Clear Filters' : 'Add Lead'}
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
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
          </TableContainer>
          <TablePagination
            component="div"
            count={filteredLeads.length}
            page={page}
            onPageChange={handlePageChange}
            rowsPerPage={pageSize}
            onRowsPerPageChange={handlePageSizeChange}
            rowsPerPageOptions={pageSizeOptions}
            showFirstButton
            showLastButton
          />
        </CardContent>
      </Card>

      {/* Add/Edit Lead Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogHeader
          mode={editingId ? 'edit' : 'create'}
          entityType="lead"
          entityName={editingId ? `${formData.firstName} ${formData.lastName}` : undefined}
          entityId={editingId || undefined}
          onClose={handleCloseDialog}
          subtitle={formData.company || formData.jobTitle}
          status={formData.status ? LEAD_STATUSES.find(s => s.value === formData.status)?.label : undefined}
          statusColor={formData.status ? LEAD_STATUSES.find(s => s.value === formData.status)?.text : undefined}
        />
        <Box sx={{ borderBottom: 1, borderColor: 'divider', px: 3 }}>
          <Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)} aria-label="Lead dialog tabs">
            <Tab label="Lead Info" id="lead-tab-0" aria-controls="lead-tabpanel-0" />
            {editingId && <Tab label="Contact Info" icon={<ContactPhoneIcon fontSize="small" />} iconPosition="start" id="lead-tab-1" aria-controls="lead-tabpanel-1" />}
            {editingId && <Tab label="Related" icon={<PersonAddIcon fontSize="small" />} iconPosition="start" id="lead-tab-2" aria-controls="lead-tabpanel-2" />}
            <Tab label="Notes" icon={<NoteIcon fontSize="small" />} iconPosition="start" id={editingId ? "lead-tab-3" : "lead-tab-1"} aria-controls={editingId ? "lead-tabpanel-3" : "lead-tabpanel-1"} />
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
          {editingId && dialogTab === 1 && (
            <Box role="tabpanel" id="lead-tabpanel-1" aria-labelledby="lead-tab-1">
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

          {/* Related Entities Tab - Only when editing */}
          {editingId && dialogTab === 2 && (
            <Box role="tabpanel" id="lead-tabpanel-2" aria-labelledby="lead-tab-2">
              <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 2 }}>
                Related Records
              </Typography>
              <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
                View activities and related records linked to this lead.
              </Typography>
              <RelatedEntitiesPanel
                entityType="lead"
                entityId={editingId}
                showRelated={['activities']}
                compact
                showAddButtons
              />
            </Box>
          )}

          {/* Notes Tab - Index depends on whether we're editing (has Contact Info + Related tabs) or adding */}
          {((editingId && dialogTab === 3) || (!editingId && dialogTab === 1)) && (
            <Box role="tabpanel" id={editingId ? "lead-tabpanel-3" : "lead-tabpanel-1"} aria-labelledby={editingId ? "lead-tab-3" : "lead-tab-1"}>
              {editingId ? (
                <NotesTab
                  entityType="Lead"
                  entityId={editingId}
                  entityName={`${formData.firstName} ${formData.lastName}`.trim() || 'Lead'}
                />
              ) : (
                <TextField
                  fullWidth
                  label="Initial Notes"
                  name="notes"
                  value={formData.notes}
                  onChange={handleInputChange}
                  multiline
                  rows={4}
                  placeholder="Add any initial notes about this lead..."
                  sx={{ mt: 2 }}
                />
              )}
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

      {/* Lead Conversion Dialog */}
      <Dialog open={convertDialogOpen} onClose={handleCloseConvertDialog} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <TrendingUpIcon sx={{ color: '#06A77D' }} />
          Convert Lead to Account
        </DialogTitle>
        <DialogContent>
          <DialogError 
            error={convertApi.error} 
            onClose={convertApi.clearError}
          />
          {convertingLead && (
            <Box sx={{ mb: 3 }}>
              <Alert severity="info" sx={{ mb: 2 }}>
                Converting: <strong>{convertingLead.firstName} {convertingLead.lastName}</strong>
                {convertingLead.company && ` (${convertingLead.company})`}
              </Alert>
              
              <Typography variant="subtitle2" sx={{ mb: 1, fontWeight: 600 }}>
                This will:
              </Typography>
              <Typography variant="body2" component="ul" sx={{ pl: 2, mb: 2 }}>
                <li>Create a new Account from lead information</li>
                <li>Mark the lead as "Converted"</li>
                {convertFormData.createOpportunity && <li>Create a new Opportunity linked to the account</li>}
              </Typography>

              <Box sx={{ 
                p: 2, 
                border: '1px solid #E0E0E0', 
                borderRadius: 2,
                bgcolor: '#FAFAFA',
                mb: 2 
              }}>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <Checkbox
                    checked={convertFormData.createOpportunity}
                    onChange={(e) => setConvertFormData(prev => ({ 
                      ...prev, 
                      createOpportunity: e.target.checked 
                    }))}
                    sx={{ p: 0, mr: 1 }}
                  />
                  <Typography variant="subtitle2">
                    Also create an Opportunity
                  </Typography>
                </Box>

                <Collapse in={convertFormData.createOpportunity}>
                  <Stack spacing={2}>
                    <TextField
                      label="Opportunity Name"
                      size="small"
                      fullWidth
                      value={convertFormData.opportunityName}
                      onChange={(e) => setConvertFormData(prev => ({ 
                        ...prev, 
                        opportunityName: e.target.value 
                      }))}
                    />
                    <TextField
                      label="Estimated Value ($)"
                      size="small"
                      fullWidth
                      type="number"
                      value={convertFormData.estimatedValue}
                      onChange={(e) => setConvertFormData(prev => ({ 
                        ...prev, 
                        estimatedValue: e.target.value 
                      }))}
                      InputProps={{ inputProps: { min: 0 } }}
                    />
                    <TextField
                      label="Expected Close Date"
                      size="small"
                      fullWidth
                      type="date"
                      value={convertFormData.expectedCloseDate}
                      onChange={(e) => setConvertFormData(prev => ({ 
                        ...prev, 
                        expectedCloseDate: e.target.value 
                      }))}
                      InputLabelProps={{ shrink: true }}
                    />
                  </Stack>
                </Collapse>
              </Box>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseConvertDialog}>Cancel</Button>
          <ActionButton
            label="Convert Lead"
            loading={convertApi.loading}
            onClick={handleConvertLead}
            color="success"
            variant="contained"
          />
        </DialogActions>
      </Dialog>
    </Container>
  );
}

export default LeadsPage;

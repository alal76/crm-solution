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
  Grid,
  Divider,
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
  Comment as CommentIcon,
  QueryStats as QueryStatsIcon,
  Insights as InsightsIcon,
} from '@mui/icons-material';
import { RecordComments } from '../components/common/RecordComments';
import apiClient from '../services/apiClient';
import logger from '../services/logger';
import logo from '../assets/logo.png';

import { ContactInfoPanel } from '../components/ContactInfo';
import NotesTab from '../components/NotesTab';
import EntitySelect from '../components/EntitySelect';
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
import DynamicEntityForm, { ExtraTab } from '../components/DynamicEntityForm';
// FEAT-AISCORING: Score analysis drawer
import LeadScoreExplanationDrawer from '../components/leads/LeadScoreExplanationDrawer';
import enumCacheService from '../services/enumCacheService';
import type { EnumValue } from '../types/enums';
import leadService, {
  LeadSummaryDto,
  LeadDto,
  CreateLeadDto,
  UpdateLeadDto,
  ConvertLeadDto,
} from '../services/leadService';

// REM-ORPHAN-003: LeadsPage now talks to the real `/api/leads` Lead API
// (CRM.Api.Controllers.LeadsController) via leadService, instead of the legacy
// "Contacts-as-Leads" flow (`/contacts/type/Lead` with source/status packed into
// the Contact's `notes` field as JSON). See CRM.Backend/src/CRM.Core/Entities/Lead.cs
// for the canonical enum values below.

// Lead status options — value strings MUST match LeadLifecycleStatus enum member
// names exactly (New|Working|Nurturing|Qualified|Disqualified|Converted), since the
// backend parses them case-sensitively via Enum.TryParse<LeadLifecycleStatus>.
const LEAD_STATUSES = [
  { value: 'New', label: 'New', bg: '#E8DEF8', text: '#6750A4' },
  { value: 'Working', label: 'Working', bg: '#E1F5FE', text: '#0277BD' },
  { value: 'Nurturing', label: 'Nurturing', bg: '#FFF3E0', text: '#E65100' },
  { value: 'Qualified', label: 'Qualified', bg: '#E8F5E9', text: '#06A77D' },
  { value: 'Disqualified', label: 'Disqualified', bg: '#FFEBEE', text: '#B3261E' },
  { value: 'Converted', label: 'Converted', bg: '#F1F8E9', text: '#558B2F' },
];

// Lead source options — value strings MUST match LeadSource enum member names
// exactly (Web|Campaign|Referral|Event|Partner|Manual).
const LEAD_SOURCES = [
  { value: 'Web', label: 'Web', bg: '#E3F2FD', text: '#1565C0' },
  { value: 'Campaign', label: 'Campaign', bg: '#FCE4EC', text: '#C2185B' },
  { value: 'Referral', label: 'Referral', bg: '#F3E5F5', text: '#6A1B9A' },
  { value: 'Event', label: 'Event', bg: '#E0F2F1', text: '#00695C' },
  { value: 'Partner', label: 'Partner', bg: '#FFF8E1', text: '#F9A825' },
  { value: 'Manual', label: 'Manual', bg: '#F0F4C3', text: '#558B2F' },
];

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
  email: string;
  phone: string;
  companyName: string;
  title: string;
  source: string;
  status: string;
  notes: string;
  qualificationNotes: string;
  website: string;
  region: string;
  campaignId: string;
}

const EMPTY_FORM_DATA: LeadFormData = {
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  companyName: '',
  title: '',
  source: 'Web',
  status: 'New',
  notes: '',
  qualificationNotes: '',
  website: '',
  region: '',
  campaignId: '',
};

// Search fields for leads
const SEARCH_FIELDS: SearchField[] = [
  { name: 'firstName', label: 'First Name', type: 'text' },
  { name: 'lastName', label: 'Last Name', type: 'text' },
  { name: 'companyName', label: 'Company', type: 'text' },
  { name: 'source', label: 'Source', type: 'select', options: LEAD_SOURCES.map(s => ({ value: s.value, label: s.label })) },
  { name: 'status', label: 'Status', type: 'select', options: LEAD_STATUSES.map(s => ({ value: s.value, label: s.label })) },
];

const SEARCHABLE_FIELDS = ['firstName', 'lastName', 'companyName', 'email', 'title'];

// Small label:value display row for the read-only Qualification & Attribution tab
function InfoRow({ label, value }: { label: string; value: React.ReactNode }) {
  if (value === undefined || value === null || value === '') return null;
  return (
    <Grid item xs={12} sm={6} md={4}>
      <Typography variant="caption" color="textSecondary" display="block">
        {label}
      </Typography>
      <Typography variant="body2" sx={{ fontWeight: 500 }}>
        {value}
      </Typography>
    </Grid>
  );
}

function LeadsPage() {
  const [leads, setLeads] = useState<LeadSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  // ENUM-FE-015: Dynamic lead status options loaded from enum cache (falls back to LEAD_STATUSES)
  const [dynamicLeadStatuses, setDynamicLeadStatuses] = useState<EnumValue[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [dialogTab, setDialogTab] = useState(0);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<LeadFormData>(EMPTY_FORM_DATA);
  // Full LeadDto for the record being edited — holds the read-only fields
  // (BANT/MEDDIC scores, UTM attribution, mqlDate/sqlDate/tags, etc.) that the
  // list endpoint (LeadSummaryDto) doesn't include.
  const [leadDetail, setLeadDetail] = useState<LeadDto | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);

  // Search and filter state
  const [searchFilters, setSearchFilters] = useState<SearchFilter[]>([]);
  const [searchText, setSearchText] = useState('');

  // Multi-select and bulk update state
  const [selectedIds, setSelectedIds] = useState<number[]>([]);
  const [bulkDialogOpen, setBulkDialogOpen] = useState(false);
  const [bulkFormData, setBulkFormData] = useState({
    source: '' as string,
    status: '' as string,
    companyName: '' as string,
  });

  // API state for dialog operations
  const dialogApi = useApiState({ successTimeout: 3000 });
  const bulkApi = useApiState({ successTimeout: 3000 });
  const convertApi = useApiState({ successTimeout: 3000 });
  const { hasPermission } = useProfile();

  // Lead conversion dialog state
  const [convertDialogOpen, setConvertDialogOpen] = useState(false);
  const [convertingLead, setConvertingLead] = useState<LeadSummaryDto | null>(null);
  // FEAT-AISCORING: Score analysis drawer state
  const [scoreDrawerLeadId, setScoreDrawerLeadId] = useState<number | null>(null);
  const [scoreDrawerOpen, setScoreDrawerOpen] = useState(false);
  const [convertFormData, setConvertFormData] = useState({
    accountId: '' as number | string,
    opportunityName: '',
    estimatedValue: '',
    expectedCloseDate: '',
  });

  // Fetch leads function (defined early for SignalR callbacks)
  const fetchLeads = useCallback(async () => {
    try {
      setLoading(true);
      // Real Lead API — GET /api/leads?page=&pageSize=
      // The page still filters/paginates client-side (usePagination below), so we
      // request a large page to approximate "all leads" in one call.
      const response = await leadService.getAll(1, 1000);
      const payload = response?.data ?? response;
      const rows = Array.isArray(payload?.data) ? payload.data : Array.isArray(payload) ? payload : [];
      setLeads(rows);
      setError(null);
    } catch (err: unknown) {
      setError((err as any).response?.data?.message || 'Failed to fetch leads');
      console.error('Error fetching leads:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  // SignalR subscription for real-time updates.
  // NOTE: as of this migration, LeadsController does not yet call
  // ICrmNotificationService.NotifyRecord{Created,Updated,Deleted}Async for the
  // "Lead" entity type the way ContactsController/AccountsController do — so this
  // subscription is wired correctly but the backend won't emit events for it yet.
  // Flagged as a follow-up; the list still refreshes on manual create/update/delete.
  useEntityTypeSubscription('Lead', {
    onCreated: useCallback(() => {
      logger.debug('[SignalR] Lead created - refreshing list');
      fetchLeads();
    }, [fetchLeads]),
    onUpdated: useCallback(() => {
      logger.debug('[SignalR] Lead updated - refreshing list');
      fetchLeads();
    }, [fetchLeads]),
    onDeleted: useCallback(() => {
      logger.debug('[SignalR] Lead deleted - refreshing list');
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

  // ENUM-FE-015: Load dynamic lead status options from enumCacheService
  useEffect(() => {
    enumCacheService.getValues('LeadStatus').then(setDynamicLeadStatuses).catch(() => {/* fallback to static LEAD_STATUSES */});
  }, []);

  useEffect(() => {
    fetchLeads();
  }, [fetchLeads]);

  const handleOpenDialog = async (lead?: LeadSummaryDto) => {
    setDialogTab(0);
    dialogApi.clearError();

    if (!lead) {
      setEditingId(null);
      setLeadDetail(null);
      setFormData(EMPTY_FORM_DATA);
      setOpenDialog(true);
      return;
    }

    setEditingId(lead.id);
    setOpenDialog(true);
    setDetailLoading(true);
    try {
      const response = await leadService.getById(lead.id);
      const detail = response.data;
      setLeadDetail(detail);
      setFormData({
        firstName: detail.firstName || '',
        lastName: detail.lastName || '',
        email: detail.email || '',
        phone: detail.phone || '',
        companyName: detail.companyName || '',
        title: detail.title || '',
        source: detail.source || 'Web',
        status: detail.status || 'New',
        notes: detail.qualificationNotes || '',
        qualificationNotes: detail.qualificationNotes || '',
        website: detail.website || '',
        region: detail.region || '',
        campaignId: detail.campaignId != null ? String(detail.campaignId) : '',
      });
    } catch (err: unknown) {
      // Fall back to the summary row so the dialog is still usable
      setLeadDetail(null);
      setFormData({
        ...EMPTY_FORM_DATA,
        firstName: lead.firstName || '',
        lastName: lead.lastName || '',
        email: lead.email || '',
        phone: lead.phone || '',
        companyName: lead.companyName || '',
        title: lead.title || '',
        source: lead.source || 'Web',
        status: lead.status || 'New',
      });
      dialogApi.setError((err as any).response?.data?.message || 'Failed to load full lead details');
    } finally {
      setDetailLoading(false);
    }
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
    setLeadDetail(null);
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
    if (!formData.firstName.trim() || !formData.lastName.trim() || !formData.email.trim()) {
      dialogApi.setError('Please fill in required fields (First Name, Last Name, Email)');
      return;
    }

    // The Lead entity only has a single `QualificationNotes` field server-side —
    // prefer the dedicated qualification field, fall back to the general notes field.
    const notes = formData.qualificationNotes.trim() || formData.notes.trim() || undefined;
    const campaignId = formData.campaignId.trim() ? Number(formData.campaignId) : undefined;

    const result = await dialogApi.execute(async () => {
      if (editingId) {
        const payload: UpdateLeadDto = {
          firstName: formData.firstName,
          lastName: formData.lastName,
          email: formData.email,
          phone: formData.phone || undefined,
          companyName: formData.companyName || undefined,
          title: formData.title || undefined,
          status: formData.status,
          source: formData.source,
          region: formData.region || undefined,
          website: formData.website || undefined,
          notes,
          campaignId,
        };
        await leadService.update(editingId, payload);
        return 'updated';
      } else {
        const payload: CreateLeadDto = {
          firstName: formData.firstName,
          lastName: formData.lastName,
          email: formData.email,
          phone: formData.phone || undefined,
          companyName: formData.companyName || undefined,
          title: formData.title || undefined,
          source: formData.source,
          region: formData.region || undefined,
          website: formData.website || undefined,
          notes,
          campaignId,
        };
        await leadService.create(payload);
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
        await leadService.delete(id);
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
      companyName: '',
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
      const payload: UpdateLeadDto = {};
      if (bulkFormData.source) payload.source = bulkFormData.source;
      if (bulkFormData.status) payload.status = bulkFormData.status;
      if (bulkFormData.companyName) payload.companyName = bulkFormData.companyName;

      const updatePromises = selectedIds.map(id => leadService.update(id, payload));
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
      const deletePromises = selectedIds.map(id => leadService.delete(id));
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
  const handleOpenConvertDialog = (lead: LeadSummaryDto) => {
    setConvertingLead(lead);
    setConvertFormData({
      accountId: '',
      opportunityName: `${lead.companyName || lead.firstName} - Opportunity`,
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

  // POST /api/leads/{id}/convert — the backend creates the Opportunity and marks
  // the lead Converted server-side. It does NOT create a new Account (unlike the
  // legacy client-side flow this replaces), so the user must pick an existing one.
  const handleConvertLead = async () => {
    if (!convertingLead) return;

    if (!convertFormData.accountId) {
      convertApi.setError('Please select an Account to link the new Opportunity to.');
      return;
    }

    const result = await convertApi.execute(async () => {
      const dto: ConvertLeadDto = {
        opportunityName: convertFormData.opportunityName || undefined,
        accountId: Number(convertFormData.accountId),
        estimatedValue: convertFormData.estimatedValue ? Number(convertFormData.estimatedValue) : undefined,
        expectedCloseDate: convertFormData.expectedCloseDate || undefined,
      };
      const response = await leadService.convert(convertingLead.id, dto);
      return response.data;
    }, 'Lead converted successfully!');

    if (result) {
      handleCloseConvertDialog();
      fetchLeads();
      setSuccessMessage(`Lead converted! Opportunity #${result.opportunityId} created.`);
      setTimeout(() => setSuccessMessage(null), 5000);
    }
  };

  // AI Lead Scoring
  // NOTE: /ai/leads/{id}/score and /ai/leads/batch-score (AILeadScoringController)
  // already operate on the real Lead.Id (they query _context.Leads directly) —
  // not a Contact ID. Previously `lead.id` here was a Contact ID, so these calls
  // were silently mismatched against the Leads table; this migration fixes that
  // as a side effect of leads now carrying their real Lead ID.
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
    } catch (err: unknown) {
      setError((err as any).response?.data?.error || 'Failed to score lead');
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
    } catch (err: unknown) {
      setError((err as any).response?.data?.error || 'Failed to batch score leads');
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
                        {lead.title && (
                          <Typography variant="caption" display="block" color="textSecondary">
                            {lead.title}
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell>{lead.email}</TableCell>
                      <TableCell>{lead.companyName || '—'}</TableCell>
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
                          {/* FEAT-AISCORING: Score trend indicator */}
                          <Typography
                            variant="caption"
                            title="Score trend"
                            sx={{
                              fontSize: '0.65rem',
                              fontWeight: 700,
                              color:
                                (lead.score ?? 0) >= 70 ? '#2e7d32'
                                : (lead.score ?? 0) >= 40 ? '#757575'
                                : '#c62828',
                            }}
                          >
                            {(lead.score ?? 0) >= 70 ? '⬆' : (lead.score ?? 0) >= 40 ? '→' : '⬇'}
                          </Typography>
                        </Box>
                      </TableCell>
                      <TableCell>
                        {lead.createdAt ? new Date(lead.createdAt).toLocaleDateString() : '—'}
                      </TableCell>
                      <TableCell align="center">
                        {lead.status !== 'Converted' && (
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
                        {/* FEAT-AISCORING: View Score Analysis button */}
                        <IconButton
                          size="small"
                          onClick={() => { setScoreDrawerLeadId(lead.id); setScoreDrawerOpen(true); }}
                          sx={{ color: '#7b1fa2' }}
                          title="View Score Analysis"
                        >
                          <QueryStatsIcon fontSize="small" />
                        </IconButton>
                        {lead.status !== 'Converted' && (
                          <IconButton
                            size="small"
                            onClick={() => handleOpenConvertDialog(lead)}
                            sx={{ color: '#06A77D' }}
                            title="Convert to Opportunity"
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
          subtitle={formData.companyName || formData.title}
          status={formData.status ? LEAD_STATUSES.find(s => s.value === formData.status)?.label : undefined}
          statusColor={formData.status ? LEAD_STATUSES.find(s => s.value === formData.status)?.text : undefined}
        />
        <DialogContent sx={{ pt: 0, minHeight: 350 }}>
          {/* Error Display */}
          <DialogError
            error={dialogApi.error}
            onClose={dialogApi.clearError}
          />

          <DynamicEntityForm
            moduleName="Leads"
            formData={formData}
            onChange={handleInputChange}
            onSelectChange={(e: any) => setFormData(prev => ({ ...prev, [e.target.name]: e.target.value }))}
            setFormData={setFormData}
            activeTab={dialogTab}
            editingId={editingId}
            onTabChange={setDialogTab}
            // tags/mqlDate/sqlDate are exposed for READ on LeadDto but the backend's
            // Create/UpdateLeadDto request DTOs (LeadsController.cs) don't accept
            // them yet, so there's no write path — hidden here and surfaced
            // read-only in the "Qualification & Attribution" tab below instead.
            excludeFields={['tags', 'customFields', 'mqlDate', 'sqlDate']}
            extraTabs={[
              {
                index: 100,
                name: 'Contact Info',
                icon: <ContactPhoneIcon fontSize="small" />,
                editOnly: true,
                render: () => (
                  <Box>
                    <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 2 }}>Manage Contact Information</Typography>
                    <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
                      Add and manage multiple addresses, phone numbers, emails, and social media accounts for this lead.
                    </Typography>
                    <ContactInfoPanel entityType="Lead" entityId={editingId!} layout="tabs" showCounts={true} />
                  </Box>
                ),
              },
              {
                index: 101,
                name: 'Related',
                icon: <PersonAddIcon fontSize="small" />,
                editOnly: true,
                render: () => (
                  <Box>
                    <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 2 }}>Related Records</Typography>
                    <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
                      View activities and related records linked to this lead.
                    </Typography>
                    <RelatedEntitiesPanel entityType="lead" entityId={editingId!} showRelated={['activities']} compact showAddButtons />
                  </Box>
                ),
              },
              {
                index: 102,
                name: 'Qualification & Attribution',
                icon: <InsightsIcon fontSize="small" />,
                editOnly: true,
                render: () => (
                  <Box>
                    <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 1 }}>Qualification &amp; Attribution</Typography>
                    <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
                      Read-only — populated from lead scoring, attribution tracking, and the qualification workflow.
                    </Typography>
                    {detailLoading && (
                      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                        <CircularProgress size={28} />
                      </Box>
                    )}
                    {!detailLoading && !leadDetail && (
                      <Typography variant="body2" color="text.secondary">
                        Details unavailable.
                      </Typography>
                    )}
                    {!detailLoading && leadDetail && (
                      <Stack spacing={3}>
                        <Grid container spacing={2}>
                          <InfoRow label="Score" value={leadDetail.score} />
                          <InfoRow label="Fit Score" value={leadDetail.fitScore} />
                          <InfoRow label="Engagement Score" value={leadDetail.engagementScore} />
                          <InfoRow label="Qualification Framework" value={leadDetail.qualificationFrameworkType} />
                          <InfoRow label="Territory ID" value={leadDetail.territoryId} />
                          <InfoRow label="Tags" value={leadDetail.tags} />
                        </Grid>

                        <Divider />
                        <Typography variant="subtitle2" fontWeight={600}>BANT Scoring</Typography>
                        <Grid container spacing={2}>
                          <InfoRow label="Budget" value={leadDetail.budgetScore} />
                          <InfoRow label="Authority" value={leadDetail.authorityScore} />
                          <InfoRow label="Need" value={leadDetail.needScore} />
                          <InfoRow label="Timeline" value={leadDetail.timelineScore} />
                        </Grid>

                        <Divider />
                        <Typography variant="subtitle2" fontWeight={600}>MEDDIC Scoring</Typography>
                        <Grid container spacing={2}>
                          <InfoRow label="Metrics" value={leadDetail.metricsScore} />
                          <InfoRow label="Economic Buyer" value={leadDetail.economicBuyerScore} />
                          <InfoRow label="Decision Criteria" value={leadDetail.decisionCriteriaScore} />
                          <InfoRow label="Decision Process" value={leadDetail.decisionProcessScore} />
                          <InfoRow label="Identify Pain" value={leadDetail.identifyPainScore} />
                          <InfoRow label="Champion" value={leadDetail.championScore} />
                        </Grid>

                        <Divider />
                        <Typography variant="subtitle2" fontWeight={600}>Source Attribution</Typography>
                        <Grid container spacing={2}>
                          <InfoRow label="Original Source" value={leadDetail.originalSource} />
                          <InfoRow label="UTM Source" value={leadDetail.utmSource} />
                          <InfoRow label="UTM Medium" value={leadDetail.utmMedium} />
                          <InfoRow label="UTM Campaign" value={leadDetail.utmCampaign} />
                          <InfoRow label="First Touch Date" value={leadDetail.firstTouchDate ? new Date(leadDetail.firstTouchDate).toLocaleDateString() : undefined} />
                        </Grid>

                        <Divider />
                        <Typography variant="subtitle2" fontWeight={600}>Nurturing &amp; Timeline</Typography>
                        <Grid container spacing={2}>
                          <InfoRow label="Nurture Campaign ID" value={leadDetail.nurtureCampaignId} />
                          <InfoRow label="Enrolled At" value={leadDetail.nurtureCampaignEnrolledAt ? new Date(leadDetail.nurtureCampaignEnrolledAt).toLocaleDateString() : undefined} />
                          <InfoRow label="Last Contacted" value={leadDetail.lastContactedAt ? new Date(leadDetail.lastContactedAt).toLocaleDateString() : undefined} />
                          <InfoRow label="Days Since Last Contact" value={leadDetail.daysSinceLastContact} />
                          <InfoRow label="MQL Date" value={leadDetail.mqlDate ? new Date(leadDetail.mqlDate).toLocaleDateString() : undefined} />
                          <InfoRow label="SQL Date" value={leadDetail.sqlDate ? new Date(leadDetail.sqlDate).toLocaleDateString() : undefined} />
                          <InfoRow label="Last Activity" value={leadDetail.lastActivityDate ? new Date(leadDetail.lastActivityDate).toLocaleDateString() : undefined} />
                        </Grid>
                      </Stack>
                    )}
                  </Box>
                ),
              },
              {
                index: 103,
                name: 'Notes',
                icon: <NoteIcon fontSize="small" />,
                render: () => editingId ? (
                  <NotesTab entityType="Lead" entityId={editingId} entityName={`${formData.firstName} ${formData.lastName}`.trim() || 'Lead'} />
                ) : (
                  <TextField
                    fullWidth
                    label="Qualification Notes"
                    name="qualificationNotes"
                    value={formData.qualificationNotes}
                    onChange={handleInputChange}
                    multiline
                    rows={4}
                    placeholder="Add any initial qualification notes about this lead..."
                    sx={{ mt: 2 }}
                  />
                ),
              },
              {
                index: 104,
                name: 'Comments',
                icon: <CommentIcon fontSize="small" />,
                render: () => editingId ? (
                  <RecordComments entityType="Lead" entityId={editingId} />
                ) : (
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
                    Save the lead first to add comments.
                  </Typography>
                ),
              },
            ]}
          />
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
                {dynamicLeadStatuses.length > 0
                  ? dynamicLeadStatuses.map(opt => (
                    <MenuItem key={opt.key} value={opt.key}>{opt.label}</MenuItem>
                  ))
                  : LEAD_STATUSES.map(status => (
                    <MenuItem key={status.value} value={status.value}>{status.label}</MenuItem>
                  ))
                }
              </Select>
            </FormControl>

            <TextField
              label="Company"
              size="small"
              value={bulkFormData.companyName}
              onChange={(e) => setBulkFormData(prev => ({ ...prev, companyName: e.target.value }))}
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
          Convert Lead to Opportunity
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
                {convertingLead.companyName && ` (${convertingLead.companyName})`}
              </Alert>

              <Typography variant="subtitle2" sx={{ mb: 1, fontWeight: 600 }}>
                This will:
              </Typography>
              <Typography variant="body2" component="ul" sx={{ pl: 2, mb: 2 }}>
                <li>Create a new Opportunity linked to the selected Account</li>
                <li>Mark the lead as &quot;Converted&quot;</li>
              </Typography>

              <Box sx={{
                p: 2,
                border: '1px solid #E0E0E0',
                borderRadius: 2,
                bgcolor: '#FAFAFA',
                mb: 2
              }}>
                <Stack spacing={2}>
                  <EntitySelect
                    entityType="account"
                    name="accountId"
                    label="Account"
                    required
                    value={convertFormData.accountId}
                    onChange={(e: any) => setConvertFormData(prev => ({ ...prev, accountId: e.target.value }))}
                    helperText="The Opportunity created by conversion must be linked to an existing Account."
                  />
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

      {/* FEAT-AISCORING: Lead Score Analysis Side Drawer */}
      <LeadScoreExplanationDrawer
        leadId={scoreDrawerLeadId}
        open={scoreDrawerOpen}
        onClose={() => setScoreDrawerOpen(false)}
      />
    </Container>
  );
}

export default LeadsPage;

/**
 * CRM Solution - Interaction Management Module
 * Multi-channel interaction tracking, entity linking, and service request creation
 */
import React, { useState, useEffect, useCallback } from 'react';
import { TabPanel } from '../components/common';
import { BaseEntity } from '../types';
import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  Grid,
  Tabs,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Button,
  IconButton,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  CircularProgress,
  Alert,
  Badge,
  Avatar,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  ListItemSecondaryAction,
  Divider,
  Tooltip,
  InputAdornment,
  FormControlLabel,
  Switch,
  Autocomplete,
  Collapse,
  Stack,
} from '@mui/material';
import {
  Email as EmailIcon,
  Phone as PhoneIcon,
  VideoCall as VideoIcon,
  Chat as ChatIcon,
  Message as SmsIcon,
  Public as SocialIcon,
  Person as PersonIcon,
  Web as WebFormIcon,
  Note as NoteIcon,
  Task as TaskIcon,
  Slideshow as PresentationIcon,
  Description as ContractIcon,
  Support as SupportIcon,
  MoreHoriz as OtherIcon,
  Search as SearchIcon,
  FilterList as FilterIcon,
  Refresh as RefreshIcon,
  Add as AddIcon,
  Link as LinkIcon,
  LocalOffer as TagIcon,
  NoteAdd as AddNoteIcon,
  PersonAdd as PersonAddIcon,
  PlaylistAdd as CreateRequestIcon,
  Visibility as ViewIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Schedule as ScheduleIcon,
  PriorityHigh as PriorityIcon,
  Warning as WarningIcon,
  CheckCircle as CompletedIcon,
  Cancel as CancelIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  Speed as ExpediteIcon,
  TrendingUp as OpportunityIcon,
  Business as CustomerIcon,
  Contacts as ContactIcon,
  Assignment as ServiceRequestIcon,
  Timeline as TimelineIcon,
  Inbox as InboxIcon,
  Send as SendIcon,
  CallReceived as InboundIcon,
  CallMade as OutboundIcon,
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';

// Types
interface Interaction extends BaseEntity {
  interactionType: number;
  interactionTypeName?: string;
  direction: number;
  subject: string;
  description: string;
  interactionDate: string;
  endTime?: string;
  durationMinutes?: number;
  scheduledDate?: string;
  completedDate?: string;
  outcome: number;
  sentiment: number;
  isCompleted: boolean;
  isPrivate: boolean;
  priority?: number;
  phoneNumber?: string;
  emailAddress?: string;
  location?: string;
  meetingLink?: string;
  followUpDate?: string;
  followUpNotes?: string;
  tags?: string;
  category?: string;
  customerId: number;
  customer?: { id: number; firstName?: string; lastName?: string; companyName?: string };
  contactId?: number;
  contact?: { id: number; firstName?: string; lastName?: string };
  opportunityId?: number;
  opportunity?: { id: number; title?: string };
  assignedToUserId?: number;
  assignedToUser?: { id: number; firstName: string; lastName: string };
}

interface Customer extends BaseEntity {
  firstName?: string;
  lastName?: string;
  companyName?: string;
  email?: string;
}

interface Contact extends BaseEntity {
  firstName: string;
  lastName: string;
  email?: string;
  customerId: number;
}

interface Opportunity extends BaseEntity {
  title: string;
  customerId: number;
}

interface InteractionStats {
  totalInteractions: number;
  byType: { type: string; count: number }[];
  byOutcome: { outcome: string; count: number }[];
  byDirection: { direction: string; count: number }[];
  averageDuration: number;
  completionRate: number;
}

// Interaction type mappings
const INTERACTION_TYPES: Record<number, { label: string; icon: React.ReactElement; color: string }> = {
  0: { label: 'Email', icon: <EmailIcon />, color: '#2196f3' },
  1: { label: 'Phone', icon: <PhoneIcon />, color: '#4caf50' },
  2: { label: 'Meeting', icon: <PersonIcon />, color: '#9c27b0' },
  3: { label: 'Video Call', icon: <VideoIcon />, color: '#00bcd4' },
  4: { label: 'Chat', icon: <ChatIcon />, color: '#ff9800' },
  5: { label: 'SMS', icon: <SmsIcon />, color: '#8bc34a' },
  6: { label: 'Social Media', icon: <SocialIcon />, color: '#e91e63' },
  7: { label: 'In Person', icon: <PersonIcon />, color: '#795548' },
  8: { label: 'Web Form', icon: <WebFormIcon />, color: '#607d8b' },
  9: { label: 'Note', icon: <NoteIcon />, color: '#9e9e9e' },
  10: { label: 'Task', icon: <TaskIcon />, color: '#3f51b5' },
  11: { label: 'Demo', icon: <PresentationIcon />, color: '#ff5722' },
  12: { label: 'Presentation', icon: <PresentationIcon />, color: '#673ab7' },
  13: { label: 'Contract', icon: <ContractIcon />, color: '#009688' },
  14: { label: 'Support', icon: <SupportIcon />, color: '#f44336' },
  15: { label: 'Other', icon: <OtherIcon />, color: '#757575' },
};

const DIRECTION_LABELS: Record<number, { label: string; icon: React.ReactElement }> = {
  0: { label: 'Inbound', icon: <InboundIcon /> },
  1: { label: 'Outbound', icon: <OutboundIcon /> },
  2: { label: 'Internal', icon: <TimelineIcon /> },
};

const OUTCOME_LABELS: Record<number, { label: string; color: 'default' | 'success' | 'error' | 'warning' | 'info' }> = {
  0: { label: 'None', color: 'default' },
  1: { label: 'Successful', color: 'success' },
  2: { label: 'Unsuccessful', color: 'error' },
  3: { label: 'Follow-up Required', color: 'warning' },
  4: { label: 'No Response', color: 'info' },
  5: { label: 'Voicemail', color: 'info' },
  6: { label: 'Rescheduled', color: 'warning' },
  7: { label: 'Cancelled', color: 'error' },
};

const PRIORITY_LABELS: Record<number, { label: string; color: 'default' | 'success' | 'warning' | 'error' }> = {
  1: { label: 'Low', color: 'success' },
  2: { label: 'Normal', color: 'default' },
  3: { label: 'High', color: 'warning' },
  4: { label: 'Urgent', color: 'error' },
};

function InteractionsPage() {
  // State
  const [interactions, setInteractions] = useState<Interaction[]>([]);
  const [needsAttention, setNeedsAttention] = useState<Interaction[]>([]);
  const [stats, setStats] = useState<InteractionStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [tabValue, setTabValue] = useState(0);

  // Filter state
  const [typeFilter, setTypeFilter] = useState<number | ''>('');
  const [directionFilter, setDirectionFilter] = useState<number | ''>('');
  const [outcomeFilter, setOutcomeFilter] = useState<number | ''>('');
  const [searchQuery, setSearchQuery] = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');

  // Dialog state
  const [selectedInteraction, setSelectedInteraction] = useState<Interaction | null>(null);
  const [detailsOpen, setDetailsOpen] = useState(false);
  const [linkDialogOpen, setLinkDialogOpen] = useState(false);
  const [noteDialogOpen, setNoteDialogOpen] = useState(false);
  const [tagDialogOpen, setTagDialogOpen] = useState(false);
  const [createContactDialogOpen, setCreateContactDialogOpen] = useState(false);
  const [createServiceRequestDialogOpen, setCreateServiceRequestDialogOpen] = useState(false);

  // Entity lookup state
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [opportunities, setOpportunities] = useState<Opportunity[]>([]);

  // Form state
  const [linkForm, setLinkForm] = useState({ customerId: '', contactId: '', opportunityId: '', notes: '' });
  const [noteForm, setNoteForm] = useState({ note: '', isInternal: false });
  const [tagForm, setTagForm] = useState<string[]>([]);
  const [contactForm, setContactForm] = useState({
    firstName: '', lastName: '', email: '', phone: '', title: '',
    customerId: '', createCustomerIfNeeded: false,
  });
  const [serviceRequestForm, setServiceRequestForm] = useState({
    requestType: 'Support', priority: 'Normal', description: '',
    copyInteractionDescription: true, expedite: false,
  });

  // Load data on mount
  useEffect(() => {
    fetchInteractions();
    fetchNeedsAttention();
    fetchStats();
    fetchEntities();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const fetchInteractions = useCallback(async () => {
    try {
      setLoading(true);
      const params = new URLSearchParams();
      if (typeFilter !== '') params.append('interactionType', String(typeFilter));
      if (directionFilter !== '') params.append('direction', String(directionFilter));
      if (outcomeFilter !== '') params.append('outcome', String(outcomeFilter));
      if (dateFrom) params.append('fromDate', dateFrom);
      if (dateTo) params.append('toDate', dateTo);

      const response = await apiClient.get(`/interactions?${params.toString()}`);
      setInteractions(response.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch interactions');
    } finally {
      setLoading(false);
    }
  }, [typeFilter, directionFilter, outcomeFilter, dateFrom, dateTo]);

  const fetchNeedsAttention = async () => {
    try {
      const response = await apiClient.get('/interactions/needs-attention?limit=25');
      setNeedsAttention(response.data);
    } catch (err) {
      console.error('Error fetching needs attention:', err);
    }
  };

  const fetchStats = async () => {
    try {
      const response = await apiClient.get('/interactions/stats');
      setStats(response.data);
    } catch (err) {
      console.error('Error fetching stats:', err);
    }
  };

  const fetchEntities = async () => {
    try {
      const [customersRes, contactsRes, opportunitiesRes] = await Promise.all([
        apiClient.get('/customers?limit=100'),
        apiClient.get('/contacts?limit=100'),
        apiClient.get('/opportunities?limit=100'),
      ]);
      setCustomers(customersRes.data);
      setContacts(contactsRes.data);
      setOpportunities(opportunitiesRes.data);
    } catch (err) {
      console.error('Error fetching entities:', err);
    }
  };

  useEffect(() => {
    fetchInteractions();
  }, [fetchInteractions]);

  // Filter interactions by search
  const filteredInteractions = interactions.filter(interaction => {
    if (!searchQuery) return true;
    const query = searchQuery.toLowerCase();
    return (
      interaction.subject?.toLowerCase().includes(query) ||
      interaction.description?.toLowerCase().includes(query) ||
      interaction.customer?.firstName?.toLowerCase().includes(query) ||
      interaction.customer?.lastName?.toLowerCase().includes(query) ||
      interaction.customer?.companyName?.toLowerCase().includes(query) ||
      interaction.emailAddress?.toLowerCase().includes(query) ||
      interaction.phoneNumber?.includes(query)
    );
  });

  // Handlers
  const handleRefresh = () => {
    fetchInteractions();
    fetchNeedsAttention();
    fetchStats();
  };

  const handleViewDetails = (interaction: Interaction) => {
    setSelectedInteraction(interaction);
    setDetailsOpen(true);
  };

  const handleOpenLinkDialog = (interaction: Interaction) => {
    setSelectedInteraction(interaction);
    setLinkForm({
      customerId: interaction.customerId ? String(interaction.customerId) : '',
      contactId: interaction.contactId ? String(interaction.contactId) : '',
      opportunityId: interaction.opportunityId ? String(interaction.opportunityId) : '',
      notes: '',
    });
    setLinkDialogOpen(true);
  };

  const handleOpenNoteDialog = (interaction: Interaction) => {
    setSelectedInteraction(interaction);
    setNoteForm({ note: '', isInternal: false });
    setNoteDialogOpen(true);
  };

  const handleOpenTagDialog = (interaction: Interaction) => {
    setSelectedInteraction(interaction);
    setTagForm(interaction.tags ? interaction.tags.split(',').map(t => t.trim()) : []);
    setTagDialogOpen(true);
  };

  const handleOpenCreateContactDialog = (interaction: Interaction) => {
    setSelectedInteraction(interaction);
    setContactForm({
      firstName: '',
      lastName: '',
      email: interaction.emailAddress || '',
      phone: interaction.phoneNumber || '',
      title: '',
      customerId: interaction.customerId ? String(interaction.customerId) : '',
      createCustomerIfNeeded: !interaction.customerId,
    });
    setCreateContactDialogOpen(true);
  };

  const handleOpenCreateServiceRequestDialog = (interaction: Interaction) => {
    setSelectedInteraction(interaction);
    setServiceRequestForm({
      requestType: 'Support',
      priority: 'Normal',
      description: '',
      copyInteractionDescription: true,
      expedite: false,
    });
    setCreateServiceRequestDialogOpen(true);
  };

  const handleCompleteInteraction = async (interaction: Interaction, outcome?: number) => {
    try {
      await apiClient.post(`/interactions/${interaction.id}/complete`, { outcome });
      setSuccess('Interaction marked as completed');
      fetchInteractions();
      fetchNeedsAttention();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to complete interaction');
    }
  };

  const handleLinkInteraction = async () => {
    if (!selectedInteraction) return;
    try {
      await apiClient.post(`/interactions/${selectedInteraction.id}/link`, {
        customerId: linkForm.customerId ? parseInt(linkForm.customerId) : null,
        contactId: linkForm.contactId ? parseInt(linkForm.contactId) : null,
        opportunityId: linkForm.opportunityId ? parseInt(linkForm.opportunityId) : null,
        notes: linkForm.notes,
      });
      setSuccess('Interaction linked successfully');
      setLinkDialogOpen(false);
      fetchInteractions();
      fetchNeedsAttention();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to link interaction');
    }
  };

  const handleAddNote = async () => {
    if (!selectedInteraction) return;
    try {
      await apiClient.post(`/interactions/${selectedInteraction.id}/notes`, noteForm);
      setSuccess('Note added successfully');
      setNoteDialogOpen(false);
      fetchInteractions();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to add note');
    }
  };

  const handleUpdateTags = async () => {
    if (!selectedInteraction) return;
    try {
      await apiClient.post(`/interactions/${selectedInteraction.id}/tags`, { tags: tagForm });
      setSuccess('Tags updated successfully');
      setTagDialogOpen(false);
      fetchInteractions();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update tags');
    }
  };

  const handleCreateContact = async () => {
    if (!selectedInteraction) return;
    try {
      await apiClient.post(`/interactions/${selectedInteraction.id}/create-contact`, {
        firstName: contactForm.firstName,
        lastName: contactForm.lastName,
        email: contactForm.email,
        phone: contactForm.phone,
        title: contactForm.title,
        customerId: contactForm.customerId ? parseInt(contactForm.customerId) : null,
        createCustomerIfNeeded: contactForm.createCustomerIfNeeded,
      });
      setSuccess('Contact created successfully');
      setCreateContactDialogOpen(false);
      fetchInteractions();
      fetchEntities();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create contact');
    }
  };

  const handleCreateServiceRequest = async () => {
    if (!selectedInteraction) return;
    try {
      await apiClient.post(`/interactions/${selectedInteraction.id}/create-service-request`, {
        requestType: serviceRequestForm.requestType,
        priority: serviceRequestForm.priority,
        description: serviceRequestForm.description,
        copyInteractionDescription: serviceRequestForm.copyInteractionDescription,
        expedite: serviceRequestForm.expedite,
      });
      setSuccess(`Service request created ${serviceRequestForm.expedite ? '(EXPEDITED)' : ''} successfully`);
      setCreateServiceRequestDialogOpen(false);
      fetchInteractions();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create service request');
    }
  };

  const formatDateTime = (dateStr: string) => {
    return new Date(dateStr).toLocaleString();
  };

  const formatTimeAgo = (dateStr: string) => {
    const now = new Date();
    const date = new Date(dateStr);
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;
    return date.toLocaleDateString();
  };

  const getCustomerName = (interaction: Interaction) => {
    if (interaction.customer) {
      return interaction.customer.companyName ||
        `${interaction.customer.firstName || ''} ${interaction.customer.lastName || ''}`.trim();
    }
    return 'Unlinked';
  };

  const getContactName = (interaction: Interaction) => {
    if (interaction.contact) {
      return `${interaction.contact.firstName || ''} ${interaction.contact.lastName || ''}`.trim();
    }
    return '-';
  };

  // Stats cards
  const renderStats = () => {
    if (!stats) return null;

    return (
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" gutterBottom>Total Interactions</Typography>
              <Typography variant="h4">{stats.totalInteractions}</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" gutterBottom>Completion Rate</Typography>
              <Typography variant="h4">{stats.completionRate.toFixed(1)}%</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" gutterBottom>Avg Duration</Typography>
              <Typography variant="h4">{Math.round(stats.averageDuration)} min</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card sx={{ bgcolor: needsAttention.length > 0 ? 'warning.light' : 'success.light' }}>
            <CardContent>
              <Typography color="text.secondary" gutterBottom>Needs Attention</Typography>
              <Typography variant="h4">{needsAttention.length}</Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    );
  };

  // Filters
  const renderFilters = () => (
    <Card sx={{ mb: 2, p: 2 }}>
      <Grid container spacing={2} alignItems="center">
        <Grid item xs={12} md={3}>
          <TextField
            fullWidth
            size="small"
            placeholder="Search interactions..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon />
                </InputAdornment>
              ),
            }}
          />
        </Grid>
        <Grid item xs={6} md={2}>
          <FormControl fullWidth size="small">
            <InputLabel>Type</InputLabel>
            <Select
              value={typeFilter}
              label="Type"
              onChange={(e) => setTypeFilter(e.target.value as number | '')}
            >
              <MenuItem value="">All Types</MenuItem>
              {Object.entries(INTERACTION_TYPES).map(([key, val]) => (
                <MenuItem key={key} value={parseInt(key)}>{val.label}</MenuItem>
              ))}
            </Select>
          </FormControl>
        </Grid>
        <Grid item xs={6} md={2}>
          <FormControl fullWidth size="small">
            <InputLabel>Direction</InputLabel>
            <Select
              value={directionFilter}
              label="Direction"
              onChange={(e) => setDirectionFilter(e.target.value as number | '')}
            >
              <MenuItem value="">All Directions</MenuItem>
              {Object.entries(DIRECTION_LABELS).map(([key, val]) => (
                <MenuItem key={key} value={parseInt(key)}>{val.label}</MenuItem>
              ))}
            </Select>
          </FormControl>
        </Grid>
        <Grid item xs={6} md={2}>
          <FormControl fullWidth size="small">
            <InputLabel>Outcome</InputLabel>
            <Select
              value={outcomeFilter}
              label="Outcome"
              onChange={(e) => setOutcomeFilter(e.target.value as number | '')}
            >
              <MenuItem value="">All Outcomes</MenuItem>
              {Object.entries(OUTCOME_LABELS).map(([key, val]) => (
                <MenuItem key={key} value={parseInt(key)}>{val.label}</MenuItem>
              ))}
            </Select>
          </FormControl>
        </Grid>
        <Grid item xs={6} md={1.5}>
          <TextField
            fullWidth
            size="small"
            type="date"
            label="From"
            value={dateFrom}
            onChange={(e) => setDateFrom(e.target.value)}
            InputLabelProps={{ shrink: true }}
          />
        </Grid>
        <Grid item xs={6} md={1.5}>
          <TextField
            fullWidth
            size="small"
            type="date"
            label="To"
            value={dateTo}
            onChange={(e) => setDateTo(e.target.value)}
            InputLabelProps={{ shrink: true }}
          />
        </Grid>
      </Grid>
    </Card>
  );

  // Interaction row
  const renderInteractionRow = (interaction: Interaction, showActions = true) => {
    const typeInfo = INTERACTION_TYPES[interaction.interactionType] || INTERACTION_TYPES[15];
    const directionInfo = DIRECTION_LABELS[interaction.direction] || DIRECTION_LABELS[0];
    const outcomeInfo = OUTCOME_LABELS[interaction.outcome] || OUTCOME_LABELS[0];

    return (
      <TableRow
        key={interaction.id}
        hover
        sx={{
          cursor: 'pointer',
          bgcolor: interaction.customerId <= 0 ? 'rgba(255, 152, 0, 0.08)' : 'inherit',
        }}
        onClick={() => handleViewDetails(interaction)}
      >
        <TableCell>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Avatar sx={{ bgcolor: typeInfo.color, width: 32, height: 32 }}>
              {typeInfo.icon}
            </Avatar>
            <Box>
              <Typography variant="body2" fontWeight={500}>
                {interaction.subject || 'No Subject'}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {typeInfo.label} • {directionInfo.label}
              </Typography>
            </Box>
          </Box>
        </TableCell>
        <TableCell>
          <Typography variant="body2">{getCustomerName(interaction)}</Typography>
          <Typography variant="caption" color="text.secondary">{getContactName(interaction)}</Typography>
        </TableCell>
        <TableCell>
          <Chip
            size="small"
            label={outcomeInfo.label}
            color={outcomeInfo.color}
            variant="outlined"
          />
          {interaction.isCompleted && (
            <CompletedIcon color="success" sx={{ ml: 1, fontSize: 16 }} />
          )}
        </TableCell>
        <TableCell>
          <Typography variant="body2">{formatTimeAgo(interaction.interactionDate)}</Typography>
          {interaction.followUpDate && (
            <Typography variant="caption" color={new Date(interaction.followUpDate) < new Date() ? 'error' : 'text.secondary'}>
              Follow-up: {formatTimeAgo(interaction.followUpDate)}
            </Typography>
          )}
        </TableCell>
        <TableCell>
          {interaction.tags && interaction.tags.split(',').slice(0, 3).map((tag, i) => (
            <Chip key={i} size="small" label={tag.trim()} sx={{ mr: 0.5, mb: 0.5 }} />
          ))}
        </TableCell>
        {showActions && (
          <TableCell onClick={(e) => e.stopPropagation()}>
            <Stack direction="row" spacing={0.5}>
              <Tooltip title="Link to entities">
                <IconButton size="small" onClick={() => handleOpenLinkDialog(interaction)}>
                  <LinkIcon fontSize="small" />
                </IconButton>
              </Tooltip>
              <Tooltip title="Add note">
                <IconButton size="small" onClick={() => handleOpenNoteDialog(interaction)}>
                  <AddNoteIcon fontSize="small" />
                </IconButton>
              </Tooltip>
              <Tooltip title="Create contact">
                <IconButton size="small" onClick={() => handleOpenCreateContactDialog(interaction)}>
                  <PersonAddIcon fontSize="small" />
                </IconButton>
              </Tooltip>
              <Tooltip title="Create service request">
                <IconButton size="small" onClick={() => handleOpenCreateServiceRequestDialog(interaction)}>
                  <CreateRequestIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            </Stack>
          </TableCell>
        )}
      </TableRow>
    );
  };

  // Interactions table
  const renderInteractionsTable = (interactionList: Interaction[], showActions = true) => (
    <TableContainer component={Paper}>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Interaction</TableCell>
            <TableCell>Customer / Contact</TableCell>
            <TableCell>Outcome</TableCell>
            <TableCell>Date</TableCell>
            <TableCell>Tags</TableCell>
            {showActions && <TableCell>Actions</TableCell>}
          </TableRow>
        </TableHead>
        <TableBody>
          {interactionList.length === 0 ? (
            <TableRow>
              <TableCell colSpan={showActions ? 6 : 5} align="center">
                <Typography color="text.secondary" sx={{ py: 4 }}>
                  No interactions found
                </Typography>
              </TableCell>
            </TableRow>
          ) : (
            interactionList.map(interaction => renderInteractionRow(interaction, showActions))
          )}
        </TableBody>
      </Table>
    </TableContainer>
  );

  // Details dialog
  const renderDetailsDialog = () => {
    if (!selectedInteraction) return null;
    const typeInfo = INTERACTION_TYPES[selectedInteraction.interactionType] || INTERACTION_TYPES[15];
    const directionInfo = DIRECTION_LABELS[selectedInteraction.direction] || DIRECTION_LABELS[0];
    const outcomeInfo = OUTCOME_LABELS[selectedInteraction.outcome] || OUTCOME_LABELS[0];
    const priorityInfo = PRIORITY_LABELS[selectedInteraction.priority || 2] || PRIORITY_LABELS[2];

    return (
      <Dialog open={detailsOpen} onClose={() => setDetailsOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Avatar sx={{ bgcolor: typeInfo.color }}>{typeInfo.icon}</Avatar>
            <Box>
              <Typography variant="h6">{selectedInteraction.subject || 'No Subject'}</Typography>
              <Typography variant="body2" color="text.secondary">
                {typeInfo.label} • {directionInfo.label} • {formatDateTime(selectedInteraction.interactionDate)}
              </Typography>
            </Box>
          </Box>
        </DialogTitle>
        <DialogContent dividers>
          <Grid container spacing={3}>
            <Grid item xs={12} md={8}>
              <Typography variant="subtitle2" gutterBottom>Description</Typography>
              <Paper variant="outlined" sx={{ p: 2, mb: 2, whiteSpace: 'pre-wrap' }}>
                {selectedInteraction.description || 'No description'}
              </Paper>

              {selectedInteraction.followUpDate && (
                <Alert
                  severity={new Date(selectedInteraction.followUpDate) < new Date() ? 'warning' : 'info'}
                  sx={{ mb: 2 }}
                >
                  Follow-up: {formatDateTime(selectedInteraction.followUpDate)}
                  {selectedInteraction.followUpNotes && ` - ${selectedInteraction.followUpNotes}`}
                </Alert>
              )}
            </Grid>
            <Grid item xs={12} md={4}>
              <Paper variant="outlined" sx={{ p: 2 }}>
                <Typography variant="subtitle2" gutterBottom>Details</Typography>
                <Stack spacing={1}>
                  <Box>
                    <Typography variant="caption" color="text.secondary">Status</Typography>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Chip size="small" label={outcomeInfo.label} color={outcomeInfo.color} />
                      {selectedInteraction.isCompleted && (
                        <Chip size="small" label="Completed" color="success" />
                      )}
                    </Box>
                  </Box>
                  <Box>
                    <Typography variant="caption" color="text.secondary">Priority</Typography>
                    <Chip size="small" label={priorityInfo.label} color={priorityInfo.color} />
                  </Box>
                  <Divider />
                  <Box>
                    <Typography variant="caption" color="text.secondary">Customer</Typography>
                    <Typography variant="body2">{getCustomerName(selectedInteraction)}</Typography>
                  </Box>
                  <Box>
                    <Typography variant="caption" color="text.secondary">Contact</Typography>
                    <Typography variant="body2">{getContactName(selectedInteraction)}</Typography>
                  </Box>
                  {selectedInteraction.opportunity && (
                    <Box>
                      <Typography variant="caption" color="text.secondary">Opportunity</Typography>
                      <Typography variant="body2">{selectedInteraction.opportunity.title}</Typography>
                    </Box>
                  )}
                  <Divider />
                  {selectedInteraction.emailAddress && (
                    <Box>
                      <Typography variant="caption" color="text.secondary">Email</Typography>
                      <Typography variant="body2">{selectedInteraction.emailAddress}</Typography>
                    </Box>
                  )}
                  {selectedInteraction.phoneNumber && (
                    <Box>
                      <Typography variant="caption" color="text.secondary">Phone</Typography>
                      <Typography variant="body2">{selectedInteraction.phoneNumber}</Typography>
                    </Box>
                  )}
                  {selectedInteraction.durationMinutes && (
                    <Box>
                      <Typography variant="caption" color="text.secondary">Duration</Typography>
                      <Typography variant="body2">{selectedInteraction.durationMinutes} minutes</Typography>
                    </Box>
                  )}
                  {selectedInteraction.tags && (
                    <Box>
                      <Typography variant="caption" color="text.secondary">Tags</Typography>
                      <Box sx={{ mt: 0.5 }}>
                        {selectedInteraction.tags.split(',').map((tag, i) => (
                          <Chip key={i} size="small" label={tag.trim()} sx={{ mr: 0.5, mb: 0.5 }} />
                        ))}
                      </Box>
                    </Box>
                  )}
                </Stack>
              </Paper>
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button startIcon={<LinkIcon />} onClick={() => { setDetailsOpen(false); handleOpenLinkDialog(selectedInteraction); }}>
            Link
          </Button>
          <Button startIcon={<TagIcon />} onClick={() => { setDetailsOpen(false); handleOpenTagDialog(selectedInteraction); }}>
            Tags
          </Button>
          <Button startIcon={<AddNoteIcon />} onClick={() => { setDetailsOpen(false); handleOpenNoteDialog(selectedInteraction); }}>
            Add Note
          </Button>
          <Button startIcon={<PersonAddIcon />} onClick={() => { setDetailsOpen(false); handleOpenCreateContactDialog(selectedInteraction); }}>
            Create Contact
          </Button>
          <Button
            variant="contained"
            startIcon={<CreateRequestIcon />}
            onClick={() => { setDetailsOpen(false); handleOpenCreateServiceRequestDialog(selectedInteraction); }}
          >
            Create Service Request
          </Button>
          {!selectedInteraction.isCompleted && (
            <Button
              color="success"
              startIcon={<CompletedIcon />}
              onClick={() => { setDetailsOpen(false); handleCompleteInteraction(selectedInteraction, 1); }}
            >
              Complete
            </Button>
          )}
        </DialogActions>
      </Dialog>
    );
  };

  // Link dialog
  const renderLinkDialog = () => (
    <Dialog open={linkDialogOpen} onClose={() => setLinkDialogOpen(false)} maxWidth="sm" fullWidth>
      <DialogTitle>Link Interaction to Entities</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <Autocomplete
            options={customers}
            getOptionLabel={(c) => c.companyName || `${c.firstName || ''} ${c.lastName || ''}`.trim() || `Account #${c.id}`}
            value={customers.find(c => c.id === parseInt(linkForm.customerId)) || null}
            onChange={(_, v) => setLinkForm({ ...linkForm, customerId: v ? String(v.id) : '' })}
            renderInput={(params) => <TextField {...params} label="Account" fullWidth />}
          />
          <Autocomplete
            options={contacts.filter(c => !linkForm.customerId || c.customerId === parseInt(linkForm.customerId))}
            getOptionLabel={(c) => `${c.firstName} ${c.lastName}` + (c.email ? ` (${c.email})` : '')}
            value={contacts.find(c => c.id === parseInt(linkForm.contactId)) || null}
            onChange={(_, v) => setLinkForm({ ...linkForm, contactId: v ? String(v.id) : '' })}
            renderInput={(params) => <TextField {...params} label="Contact" fullWidth />}
          />
          <Autocomplete
            options={opportunities.filter(o => !linkForm.customerId || o.customerId === parseInt(linkForm.customerId))}
            getOptionLabel={(o) => o.title}
            value={opportunities.find(o => o.id === parseInt(linkForm.opportunityId)) || null}
            onChange={(_, v) => setLinkForm({ ...linkForm, opportunityId: v ? String(v.id) : '' })}
            renderInput={(params) => <TextField {...params} label="Opportunity" fullWidth />}
          />
          <TextField
            label="Notes (optional)"
            multiline
            rows={2}
            value={linkForm.notes}
            onChange={(e) => setLinkForm({ ...linkForm, notes: e.target.value })}
            fullWidth
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={() => setLinkDialogOpen(false)}>Cancel</Button>
        <Button variant="contained" onClick={handleLinkInteraction}>Link</Button>
      </DialogActions>
    </Dialog>
  );

  // Note dialog
  const renderNoteDialog = () => (
    <Dialog open={noteDialogOpen} onClose={() => setNoteDialogOpen(false)} maxWidth="sm" fullWidth>
      <DialogTitle>Add Note</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField
            label="Note"
            multiline
            rows={4}
            value={noteForm.note}
            onChange={(e) => setNoteForm({ ...noteForm, note: e.target.value })}
            fullWidth
            required
          />
          <FormControlLabel
            control={
              <Switch
                checked={noteForm.isInternal}
                onChange={(e) => setNoteForm({ ...noteForm, isInternal: e.target.checked })}
              />
            }
            label="Internal note (not visible to customer)"
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={() => setNoteDialogOpen(false)}>Cancel</Button>
        <Button variant="contained" onClick={handleAddNote} disabled={!noteForm.note}>Add Note</Button>
      </DialogActions>
    </Dialog>
  );

  // Tag dialog
  const renderTagDialog = () => (
    <Dialog open={tagDialogOpen} onClose={() => setTagDialogOpen(false)} maxWidth="sm" fullWidth>
      <DialogTitle>Manage Tags</DialogTitle>
      <DialogContent>
        <Autocomplete
          multiple
          freeSolo
          options={['urgent', 'follow-up', 'vip', 'escalation', 'complaint', 'inquiry', 'feedback', 'sales', 'support']}
          value={tagForm}
          onChange={(_, v) => setTagForm(v)}
          renderInput={(params) => (
            <TextField {...params} label="Tags" placeholder="Add tags..." sx={{ mt: 2 }} />
          )}
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={() => setTagDialogOpen(false)}>Cancel</Button>
        <Button variant="contained" onClick={handleUpdateTags}>Save Tags</Button>
      </DialogActions>
    </Dialog>
  );

  // Create contact dialog
  const renderCreateContactDialog = () => (
    <Dialog open={createContactDialogOpen} onClose={() => setCreateContactDialogOpen(false)} maxWidth="sm" fullWidth>
      <DialogTitle>Create Contact from Interaction</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <Grid container spacing={2}>
            <Grid item xs={6}>
              <TextField
                label="First Name"
                value={contactForm.firstName}
                onChange={(e) => setContactForm({ ...contactForm, firstName: e.target.value })}
                fullWidth
                required
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                label="Last Name"
                value={contactForm.lastName}
                onChange={(e) => setContactForm({ ...contactForm, lastName: e.target.value })}
                fullWidth
                required
              />
            </Grid>
          </Grid>
          <TextField
            label="Email"
            type="email"
            value={contactForm.email}
            onChange={(e) => setContactForm({ ...contactForm, email: e.target.value })}
            fullWidth
          />
          <TextField
            label="Phone"
            value={contactForm.phone}
            onChange={(e) => setContactForm({ ...contactForm, phone: e.target.value })}
            fullWidth
          />
          <TextField
            label="Title / Position"
            value={contactForm.title}
            onChange={(e) => setContactForm({ ...contactForm, title: e.target.value })}
            fullWidth
          />
          <Autocomplete
            options={customers}
            getOptionLabel={(c) => c.companyName || `${c.firstName || ''} ${c.lastName || ''}`.trim() || `Account #${c.id}`}
            value={customers.find(c => c.id === parseInt(contactForm.customerId)) || null}
            onChange={(_, v) => setContactForm({ ...contactForm, customerId: v ? String(v.id) : '' })}
            renderInput={(params) => <TextField {...params} label="Account" fullWidth />}
          />
          <FormControlLabel
            control={
              <Switch
                checked={contactForm.createCustomerIfNeeded}
                onChange={(e) => setContactForm({ ...contactForm, createCustomerIfNeeded: e.target.checked })}
              />
            }
            label="Create new account if none selected"
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={() => setCreateContactDialogOpen(false)}>Cancel</Button>
        <Button
          variant="contained"
          onClick={handleCreateContact}
          disabled={!contactForm.firstName || !contactForm.lastName}
        >
          Create Contact
        </Button>
      </DialogActions>
    </Dialog>
  );

  // Create service request dialog
  const renderCreateServiceRequestDialog = () => (
    <Dialog open={createServiceRequestDialogOpen} onClose={() => setCreateServiceRequestDialogOpen(false)} maxWidth="sm" fullWidth>
      <DialogTitle>Create Service Request from Interaction</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          {selectedInteraction && selectedInteraction.customerId <= 0 && (
            <Alert severity="warning">
              This interaction is not linked to a customer. Please link it first before creating a service request.
            </Alert>
          )}
          <FormControl fullWidth>
            <InputLabel>Request Type</InputLabel>
            <Select
              value={serviceRequestForm.requestType}
              label="Request Type"
              onChange={(e) => setServiceRequestForm({ ...serviceRequestForm, requestType: e.target.value })}
            >
              <MenuItem value="Support">Support</MenuItem>
              <MenuItem value="Inquiry">Inquiry</MenuItem>
              <MenuItem value="Complaint">Complaint</MenuItem>
              <MenuItem value="Feature Request">Feature Request</MenuItem>
              <MenuItem value="Bug Report">Bug Report</MenuItem>
              <MenuItem value="Billing">Billing</MenuItem>
              <MenuItem value="Other">Other</MenuItem>
            </Select>
          </FormControl>
          <FormControl fullWidth>
            <InputLabel>Priority</InputLabel>
            <Select
              value={serviceRequestForm.priority}
              label="Priority"
              onChange={(e) => setServiceRequestForm({ ...serviceRequestForm, priority: e.target.value })}
            >
              <MenuItem value="Low">Low</MenuItem>
              <MenuItem value="Normal">Normal</MenuItem>
              <MenuItem value="High">High</MenuItem>
              <MenuItem value="Urgent">Urgent</MenuItem>
            </Select>
          </FormControl>
          <TextField
            label="Additional Description"
            multiline
            rows={3}
            value={serviceRequestForm.description}
            onChange={(e) => setServiceRequestForm({ ...serviceRequestForm, description: e.target.value })}
            fullWidth
          />
          <FormControlLabel
            control={
              <Switch
                checked={serviceRequestForm.copyInteractionDescription}
                onChange={(e) => setServiceRequestForm({ ...serviceRequestForm, copyInteractionDescription: e.target.checked })}
              />
            }
            label="Copy interaction description to service request"
          />
          <Paper
            variant="outlined"
            sx={{
              p: 2,
              bgcolor: serviceRequestForm.expedite ? 'error.light' : 'grey.100',
              borderColor: serviceRequestForm.expedite ? 'error.main' : 'grey.300',
            }}
          >
            <FormControlLabel
              control={
                <Switch
                  checked={serviceRequestForm.expedite}
                  onChange={(e) => setServiceRequestForm({ ...serviceRequestForm, expedite: e.target.checked })}
                  color="error"
                />
              }
              label={
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <ExpediteIcon color={serviceRequestForm.expedite ? 'error' : 'inherit'} />
                  <Typography fontWeight={serviceRequestForm.expedite ? 700 : 400}>
                    EXPEDITE this request
                  </Typography>
                </Box>
              }
            />
            {serviceRequestForm.expedite && (
              <Typography variant="caption" color="error">
                This will increase priority and flag the request for immediate attention.
              </Typography>
            )}
          </Paper>
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={() => setCreateServiceRequestDialogOpen(false)}>Cancel</Button>
        <Button
          variant="contained"
          color={serviceRequestForm.expedite ? 'error' : 'primary'}
          onClick={handleCreateServiceRequest}
          disabled={selectedInteraction?.customerId! <= 0}
          startIcon={serviceRequestForm.expedite ? <ExpediteIcon /> : <CreateRequestIcon />}
        >
          {serviceRequestForm.expedite ? 'Create & Expedite' : 'Create Service Request'}
        </Button>
      </DialogActions>
    </Dialog>
  );

  if (loading && interactions.length === 0) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 400 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ py: 2 }}>
      <Container maxWidth="xl">
        {/* Header */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
              <img src={logo} alt="CRM Logo" style={{ width: '100%', height: '100%', objectFit: 'contain' }} />
            </Box>
            <Typography variant="h4" fontWeight={700}>Interactions</Typography>
          </Box>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={handleRefresh}
          >
            Refresh
          </Button>
        </Box>

        {/* Alerts */}
        {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
        {success && <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess(null)}>{success}</Alert>}

        {/* Stats */}
        {renderStats()}

        {/* Tabs */}
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={tabValue} onChange={(_, v) => setTabValue(v)}>
            <Tab label="All Interactions" icon={<TimelineIcon />} iconPosition="start" />
            <Tab
              label={
                <Badge badgeContent={needsAttention.length} color="warning">
                  Needs Attention
                </Badge>
              }
              icon={<WarningIcon />}
              iconPosition="start"
            />
          </Tabs>
        </Box>

        {/* Tab Panels */}
        <TabPanel value={tabValue} index={0}>
          {renderFilters()}
          {renderInteractionsTable(filteredInteractions)}
        </TabPanel>

        <TabPanel value={tabValue} index={1}>
          <Alert severity="info" sx={{ mb: 2 }}>
            Interactions that need attention: unlinked to customers, overdue follow-ups, or high priority incomplete.
          </Alert>
          {renderInteractionsTable(needsAttention)}
        </TabPanel>

        {/* Dialogs */}
        {renderDetailsDialog()}
        {renderLinkDialog()}
        {renderNoteDialog()}
        {renderTagDialog()}
        {renderCreateContactDialog()}
        {renderCreateServiceRequestDialog()}
      </Container>
    </Box>
  );
}

export default InteractionsPage;

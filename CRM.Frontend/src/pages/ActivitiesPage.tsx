import { useState, useEffect } from 'react';
import {
  Box, Card, CardContent, Typography, Button, Container, Alert, CircularProgress,
  FormControl, Select, MenuItem, Chip, Grid, Paper, Avatar, TextField,
  InputAdornment, Dialog, DialogTitle, DialogContent, DialogActions,
  IconButton, Tooltip, InputLabel, SelectChangeEvent
} from '@mui/material';
import {
  Timeline, TimelineItem, TimelineSeparator, TimelineConnector, TimelineContent,
  TimelineDot, TimelineOppositeContent
} from '@mui/lab';
import {
  Search as SearchIcon, FilterList as FilterIcon,
  Email as EmailIcon, Phone as PhoneIcon, Note as NoteIcon,
  Assignment as TaskIcon, Description as QuoteIcon, Person as PersonIcon,
  TrendingUp as TrendingUpIcon, AttachMoney as DealIcon,
  Login as LoginIcon, Edit as EditIcon, Delete as DeleteIcon,
  Refresh as RefreshIcon, TrendingDown as TrendingDownIcon,
  Add as AddIcon
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import { BaseEntity } from '../types';
import { ActivityType, ActivityTypeEnum, CreateActivityDto } from '../types/crm';
import logo from '../assets/logo.png';
import ImportExportButtons from '../components/ImportExportButtons';
import { DialogError, DialogSuccess, ActionButton } from '../components/common';
import { useApiState } from '../hooks/useApiState';

// Activity type mappings
const ACTIVITY_ICONS: Record<string, React.ReactElement> = {
  'AccountCreated': <PersonIcon />,
  'AccountUpdated': <EditIcon />,
  'AccountDeleted': <DeleteIcon />,
  'ContactCreated': <PersonIcon />,
  'ContactUpdated': <EditIcon />,
  'OpportunityCreated': <TrendingUpIcon />,
  'OpportunityUpdated': <EditIcon />,
  'OpportunityWon': <DealIcon />,
  'OpportunityLost': <TrendingDownIcon />,
  'TaskCreated': <TaskIcon />,
  'TaskCompleted': <TaskIcon />,
  'QuoteCreated': <QuoteIcon />,
  'QuoteSent': <QuoteIcon />,
  'QuoteAccepted': <DealIcon />,
  'EmailSent': <EmailIcon />,
  'EmailReceived': <EmailIcon />,
  'CallMade': <PhoneIcon />,
  'CallReceived': <PhoneIcon />,
  'NoteAdded': <NoteIcon />,
  'UserLogin': <LoginIcon />,
  'UserLogout': <LoginIcon />,
  'default': <RefreshIcon />,
};

const ACTIVITY_COLORS: Record<string, string> = {
  'AccountCreated': '#4caf50',
  'AccountUpdated': '#2196f3',
  'AccountDeleted': '#f44336',
  'ContactCreated': '#4caf50',
  'ContactUpdated': '#2196f3',
  'OpportunityCreated': '#4caf50',
  'OpportunityUpdated': '#2196f3',
  'OpportunityWon': '#4caf50',
  'OpportunityLost': '#f44336',
  'TaskCreated': '#9c27b0',
  'TaskCompleted': '#4caf50',
  'QuoteCreated': '#ff9800',
  'QuoteSent': '#2196f3',
  'QuoteAccepted': '#4caf50',
  'EmailSent': '#2196f3',
  'EmailReceived': '#00bcd4',
  'CallMade': '#ff9800',
  'CallReceived': '#ff9800',
  'NoteAdded': '#607d8b',
  'UserLogin': '#9e9e9e',
  'UserLogout': '#9e9e9e',
  'default': '#9e9e9e',
};

// Options for form dropdowns
const ACTIVITY_TYPE_OPTIONS: { value: ActivityType; label: string; apiValue: number }[] = [
  { value: ActivityType.Call,     label: 'Call',     apiValue: ActivityTypeEnum.Call },
  { value: ActivityType.Email,    label: 'Email',    apiValue: ActivityTypeEnum.Email },
  { value: ActivityType.Meeting,  label: 'Meeting',  apiValue: ActivityTypeEnum.Meeting },
  { value: ActivityType.Task,     label: 'Task',     apiValue: ActivityTypeEnum.Task },
  { value: ActivityType.Note,     label: 'Note',     apiValue: ActivityTypeEnum.Note },
  { value: ActivityType.Social,   label: 'Social',   apiValue: ActivityTypeEnum.Social },
  { value: ActivityType.Campaign, label: 'Campaign', apiValue: ActivityTypeEnum.Campaign },
  { value: ActivityType.Other,    label: 'Other',    apiValue: ActivityTypeEnum.Other },
];

const STATUS_OPTIONS = [
  { value: 'open',      label: 'Open' },
  { value: 'completed', label: 'Completed' },
  { value: 'cancelled', label: 'Cancelled' },
];

const PRIORITY_OPTIONS = [
  { value: 'low',    label: 'Low' },
  { value: 'normal', label: 'Normal' },
  { value: 'high',   label: 'High' },
];

const ENTITY_TYPE_OPTIONS = [
  { value: 'Account',     label: 'Account' },
  { value: 'Contact',     label: 'Contact' },
  { value: 'Lead',        label: 'Lead' },
  { value: 'Opportunity', label: 'Opportunity' },
];

interface ActivityRecord extends BaseEntity {
  activityType: number;
  activityTypeName?: string;
  title: string;
  description?: string;
  entityType?: string;
  entityId?: number;
  entityName?: string;
  userId?: number;
  user?: { firstName: string; lastName: string };
  ipAddress?: string;
  userAgent?: string;
  oldValue?: string;
  newValue?: string;
  isSystemGenerated: boolean;
  // CRM activity fields
  type?: ActivityType;
  subject?: string;
  activityDate?: string;
  dueDate?: string;
  status?: string;
  priority?: string;
  durationMinutes?: number;
  tags?: string;
  details?: string;
}

interface ActivityStats {
  totalActivities: number;
  todayActivities: number;
  weekActivities: number;
  topActivityTypes: { type: string; count: number }[];
}

interface ActivityFormData {
  type: ActivityType;
  title: string;
  description: string;
  activityDate: string;
  dueDate: string;
  durationMinutes: number | '';
  status: string;
  priority: string;
  entityType: string;
  entityId: number | '';
  tags: string;
  details: string;
}

const emptyForm: ActivityFormData = {
  type: ActivityType.Call,
  title: '',
  description: '',
  activityDate: new Date().toISOString().slice(0, 16), // datetime-local format
  dueDate: '',
  durationMinutes: '',
  status: 'open',
  priority: 'normal',
  entityType: '',
  entityId: '',
  tags: '',
  details: '',
};

function ActivitiesPage() {
  const [activities, setActivities] = useState<ActivityRecord[]>([]);
  const [stats, setStats] = useState<ActivityStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [entityFilter, setEntityFilter] = useState<string>('all');
  const [searchQuery, setSearchQuery] = useState('');
  const [limit, setLimit] = useState(50);

  // Dialog state
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<ActivityFormData>(emptyForm);
  const dialogApi = useApiState();

  useEffect(() => {
    fetchActivities();
    fetchStats();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [entityFilter, limit]);

  const fetchActivities = async () => {
    try {
      setLoading(true);
      let endpoint = '/activities/recent?limit=' + limit;
      if (entityFilter !== 'all') {
        endpoint = `/activities/entity/${entityFilter}`;
      }
      const response = await apiClient.get(endpoint);
      setActivities(response.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch activities');
    } finally {
      setLoading(false);
    }
  };

  const fetchStats = async () => {
    try {
      const response = await apiClient.get('/activities/stats');
      setStats(response.data);
    } catch (err) {
      console.error('Error fetching stats:', err);
    }
  };

  const getActivityIcon = (typeName: string) => {
    return ACTIVITY_ICONS[typeName] || ACTIVITY_ICONS['default'];
  };

  const getActivityColor = (typeName: string) => {
    return ACTIVITY_COLORS[typeName] || ACTIVITY_COLORS['default'];
  };

  const formatTimeAgo = (dateString: string) => {
    const now = new Date();
    const date = new Date(dateString);
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} min ago`;
    if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
    if (diffDays < 7) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
    return date.toLocaleDateString();
  };

  const filteredActivities = activities.filter(activity => {
    if (!searchQuery) return true;
    const query = searchQuery.toLowerCase();
    return (
      activity.title?.toLowerCase().includes(query) ||
      activity.description?.toLowerCase().includes(query) ||
      activity.entityName?.toLowerCase().includes(query) ||
      activity.user?.firstName?.toLowerCase().includes(query) ||
      activity.user?.lastName?.toLowerCase().includes(query)
    );
  });

  // ─── Dialog handlers ───────────────────────────────────────────────────────

  const handleOpenCreate = () => {
    setEditingId(null);
    setFormData(emptyForm);
    dialogApi.reset();
    setOpenDialog(true);
  };

  const handleOpenEdit = (activity: ActivityRecord) => {
    setEditingId(activity.id);
    const typeOption = ACTIVITY_TYPE_OPTIONS.find(o => o.apiValue === activity.activityType);
    setFormData({
      type: typeOption?.value ?? ActivityType.Other,
      title: activity.title ?? activity.subject ?? '',
      description: activity.description ?? '',
      activityDate: activity.activityDate
        ? activity.activityDate.slice(0, 16)
        : (activity.createdAt ? activity.createdAt.slice(0, 16) : ''),
      dueDate: activity.dueDate ? activity.dueDate.slice(0, 16) : '',
      durationMinutes: activity.durationMinutes ?? '',
      status: activity.status ?? 'open',
      priority: activity.priority ?? 'normal',
      entityType: activity.entityType ?? '',
      entityId: activity.entityId ?? '',
      tags: activity.tags ?? '',
      details: activity.details ?? '',
    });
    dialogApi.reset();
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
    dialogApi.reset();
  };

  const handleTextChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'number' ? (value === '' ? '' : Number(value)) : value,
    }));
  };

  const handleSelectChange = (e: SelectChangeEvent<string>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSaveActivity = async () => {
    if (!formData.title.trim()) {
      dialogApi.setError('Please enter a title / subject for the activity.');
      return;
    }
    if (!formData.activityDate) {
      dialogApi.setError('Activity date is required.');
      return;
    }

    const typeOption = ACTIVITY_TYPE_OPTIONS.find(o => o.value === formData.type);

    const payload: CreateActivityDto = {
      activityType: typeOption?.apiValue ?? ActivityTypeEnum.Other,
      type: formData.type,
      title: formData.title.trim(),
      subject: formData.title.trim(),
      description: formData.description || undefined,
      details: formData.details || undefined,
      activityDate: new Date(formData.activityDate).toISOString(),
      dueDate: formData.dueDate ? new Date(formData.dueDate).toISOString() : undefined,
      durationMinutes: formData.durationMinutes !== '' ? Number(formData.durationMinutes) : undefined,
      entityType: (formData.entityType as CreateActivityDto['entityType']) || undefined,
      entityId: formData.entityId !== '' ? Number(formData.entityId) : undefined,
      tags: formData.tags || undefined,
    };

    await dialogApi.execute(async () => {
      if (editingId) {
        await apiClient.put(`/activities/${editingId}`, payload);
      } else {
        await apiClient.post('/activities', payload);
      }
      handleCloseDialog();
      fetchActivities();
    }, editingId ? 'Activity updated successfully' : 'Activity created successfully');
  };

  if (loading && activities.length === 0) {
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
            <Typography variant="h4" sx={{ fontWeight: 700 }}>Activity Feed</Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
            <TextField
              size="small"
              placeholder="Search activities..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              InputProps={{
                startAdornment: <InputAdornment position="start"><SearchIcon /></InputAdornment>
              }}
              sx={{ width: 250 }}
            />
            <FormControl size="small" sx={{ minWidth: 150 }}>
              <Select
                value={entityFilter}
                onChange={(e) => setEntityFilter(e.target.value)}
                startAdornment={<FilterIcon sx={{ mr: 1, color: '#666' }} />}
              >
                <MenuItem value="all">All Activities</MenuItem>
                <MenuItem value="Account">Accounts</MenuItem>
                <MenuItem value="Contact">Contacts</MenuItem>
                <MenuItem value="Opportunity">Opportunities</MenuItem>
                <MenuItem value="Task">Tasks</MenuItem>
                <MenuItem value="Quote">Quotes</MenuItem>
                <MenuItem value="Campaign">Campaigns</MenuItem>
              </Select>
            </FormControl>
            <FormControl size="small" sx={{ minWidth: 100 }}>
              <Select value={limit} onChange={(e) => setLimit(Number(e.target.value))}>
                <MenuItem value={25}>25</MenuItem>
                <MenuItem value={50}>50</MenuItem>
                <MenuItem value={100}>100</MenuItem>
                <MenuItem value={200}>200</MenuItem>
              </Select>
            </FormControl>
            <ImportExportButtons entityType="activities" entityLabel="Activities" onImportComplete={fetchActivities} />
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={handleOpenCreate}
              sx={{ backgroundColor: '#6750A4', whiteSpace: 'nowrap' }}
            >
              New Activity
            </Button>
          </Box>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}

        {/* Stats Cards */}
        {stats && (
          <Grid container spacing={3} sx={{ mb: 4 }}>
            <Grid item xs={12} md={3}>
              <Paper sx={{ p: 3, textAlign: 'center', backgroundColor: '#f5f5f5' }}>
                <Typography variant="h3" fontWeight={700} color="#6750A4">{stats.totalActivities.toLocaleString()}</Typography>
                <Typography color="textSecondary">Total Activities</Typography>
              </Paper>
            </Grid>
            <Grid item xs={12} md={3}>
              <Paper sx={{ p: 3, textAlign: 'center', backgroundColor: '#e3f2fd' }}>
                <Typography variant="h3" fontWeight={700} color="#2196f3">{stats.todayActivities}</Typography>
                <Typography color="textSecondary">Today</Typography>
              </Paper>
            </Grid>
            <Grid item xs={12} md={3}>
              <Paper sx={{ p: 3, textAlign: 'center', backgroundColor: '#e8f5e9' }}>
                <Typography variant="h3" fontWeight={700} color="#4caf50">{stats.weekActivities}</Typography>
                <Typography color="textSecondary">This Week</Typography>
              </Paper>
            </Grid>
            <Grid item xs={12} md={3}>
              <Paper sx={{ p: 3, backgroundColor: '#fff3e0' }}>
                <Typography variant="subtitle2" gutterBottom>Top Activity Types</Typography>
                {stats.topActivityTypes?.slice(0, 3).map((t, i) => (
                  <Box key={i} sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                    <Typography variant="body2">{t.type}</Typography>
                    <Chip label={t.count} size="small" />
                  </Box>
                ))}
              </Paper>
            </Grid>
          </Grid>
        )}

        {/* Activity Timeline */}
        <Card>
          <CardContent>
            {filteredActivities.length === 0 ? (
              <Typography sx={{ textAlign: 'center', py: 4, color: 'textSecondary' }}>
                No activities found.
              </Typography>
            ) : (
              <Timeline position="right">
                {filteredActivities.map((activity, index) => {
                  const typeName = activity.activityTypeName || 'default';
                  const icon = getActivityIcon(typeName);
                  const color = getActivityColor(typeName);

                  return (
                    <TimelineItem key={activity.id}>
                      <TimelineOppositeContent sx={{ flex: 0.2, minWidth: 120 }}>
                        <Typography variant="caption" color="textSecondary">
                          {formatTimeAgo(activity.createdAt || '')}
                        </Typography>
                        <Typography variant="caption" color="textSecondary" display="block">
                          {new Date(activity.createdAt || 0).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                        </Typography>
                      </TimelineOppositeContent>
                      <TimelineSeparator>
                        <TimelineDot sx={{ backgroundColor: color }}>
                          {icon}
                        </TimelineDot>
                        {index < filteredActivities.length - 1 && <TimelineConnector />}
                      </TimelineSeparator>
                      <TimelineContent>
                        <Paper elevation={1} sx={{ p: 2, mb: 1 }}>
                          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                            <Box>
                              <Typography fontWeight={500}>{activity.title}</Typography>
                              {activity.description && (
                                <Typography variant="body2" color="textSecondary" sx={{ mt: 0.5 }}>
                                  {activity.description}
                                </Typography>
                              )}
                              {activity.entityName && (
                                <Chip
                                  label={`${activity.entityType}: ${activity.entityName}`}
                                  size="small"
                                  variant="outlined"
                                  sx={{ mt: 1 }}
                                />
                              )}
                            </Box>
                            <Box sx={{ textAlign: 'right', display: 'flex', alignItems: 'flex-start', gap: 1 }}>
                              <Box>
                                {activity.user && (
                                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                                    <Avatar sx={{ width: 24, height: 24, fontSize: 12, backgroundColor: '#6750A4' }}>
                                      {activity.user.firstName?.[0]}{activity.user.lastName?.[0]}
                                    </Avatar>
                                    <Typography variant="caption">
                                      {activity.user.firstName} {activity.user.lastName}
                                    </Typography>
                                  </Box>
                                )}
                                {activity.isSystemGenerated && (
                                  <Chip label="System" size="small" sx={{ backgroundColor: '#e0e0e0' }} />
                                )}
                              </Box>
                              {!activity.isSystemGenerated && (
                                <Tooltip title="Edit Activity">
                                  <IconButton
                                    size="small"
                                    onClick={() => handleOpenEdit(activity)}
                                    sx={{ color: '#6750A4' }}
                                  >
                                    <EditIcon fontSize="small" />
                                  </IconButton>
                                </Tooltip>
                              )}
                            </Box>
                          </Box>
                          {(activity.oldValue || activity.newValue) && (
                            <Box sx={{ mt: 2, p: 1, backgroundColor: '#f5f5f5', borderRadius: 1 }}>
                              {activity.oldValue && (
                                <Typography variant="caption" color="error" display="block">
                                  - {activity.oldValue}
                                </Typography>
                              )}
                              {activity.newValue && (
                                <Typography variant="caption" color="success.main" display="block">
                                  + {activity.newValue}
                                </Typography>
                              )}
                            </Box>
                          )}
                        </Paper>
                      </TimelineContent>
                    </TimelineItem>
                  );
                })}
              </Timeline>
            )}

            {filteredActivities.length > 0 && filteredActivities.length >= limit && (
              <Box sx={{ textAlign: 'center', mt: 2 }}>
                <Button onClick={() => setLimit(prev => prev + 50)}>
                  Load More Activities
                </Button>
              </Box>
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Create / Edit Activity Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle sx={{ backgroundColor: '#6750A4', color: 'white' }}>
          {editingId ? 'Edit Activity' : 'New Activity'}
        </DialogTitle>
        <DialogContent sx={{ pt: 3 }}>
          <DialogError error={dialogApi.error?.message ?? null} onClose={dialogApi.clearError} />
          <Grid container spacing={2}>

            {/* Row 1: Type + Title */}
            <Grid item xs={12} sm={4}>
              <FormControl fullWidth size="small">
                <InputLabel>Type *</InputLabel>
                <Select
                  name="type"
                  value={formData.type}
                  label="Type *"
                  onChange={handleSelectChange}
                >
                  {ACTIVITY_TYPE_OPTIONS.map(opt => (
                    <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={8}>
              <TextField
                fullWidth
                size="small"
                label="Title / Subject *"
                name="title"
                value={formData.title}
                onChange={handleTextChange}
                required
              />
            </Grid>

            {/* Row 2: Description */}
            <Grid item xs={12}>
              <TextField
                fullWidth
                size="small"
                label="Description"
                name="description"
                value={formData.description}
                onChange={handleTextChange}
                multiline
                rows={2}
              />
            </Grid>

            {/* Row 3: Activity Date + Due Date */}
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                size="small"
                label="Activity Date *"
                name="activityDate"
                type="datetime-local"
                value={formData.activityDate}
                onChange={handleTextChange}
                InputLabelProps={{ shrink: true }}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                size="small"
                label="Due Date"
                name="dueDate"
                type="datetime-local"
                value={formData.dueDate}
                onChange={handleTextChange}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>

            {/* Row 4: Duration + Status + Priority */}
            <Grid item xs={12} sm={4}>
              <TextField
                fullWidth
                size="small"
                label="Duration (minutes)"
                name="durationMinutes"
                type="number"
                value={formData.durationMinutes}
                onChange={handleTextChange}
                inputProps={{ min: 0 }}
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <FormControl fullWidth size="small">
                <InputLabel>Status</InputLabel>
                <Select
                  name="status"
                  value={formData.status}
                  label="Status"
                  onChange={handleSelectChange}
                >
                  {STATUS_OPTIONS.map(opt => (
                    <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={4}>
              <FormControl fullWidth size="small">
                <InputLabel>Priority</InputLabel>
                <Select
                  name="priority"
                  value={formData.priority}
                  label="Priority"
                  onChange={handleSelectChange}
                >
                  {PRIORITY_OPTIONS.map(opt => (
                    <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            {/* Row 5: Related Entity Type + Related Entity ID */}
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth size="small">
                <InputLabel>Related Entity Type</InputLabel>
                <Select
                  name="entityType"
                  value={formData.entityType}
                  label="Related Entity Type"
                  onChange={handleSelectChange}
                >
                  <MenuItem value=""><em>None</em></MenuItem>
                  {ENTITY_TYPE_OPTIONS.map(opt => (
                    <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                size="small"
                label="Related Entity ID"
                name="entityId"
                type="number"
                value={formData.entityId}
                onChange={handleTextChange}
                disabled={!formData.entityType}
                inputProps={{ min: 1 }}
                placeholder={formData.entityType ? `Enter ${formData.entityType} ID` : 'Select entity type first'}
              />
            </Grid>

            {/* Row 6: Tags */}
            <Grid item xs={12}>
              <TextField
                fullWidth
                size="small"
                label="Tags (comma-separated)"
                name="tags"
                value={formData.tags}
                onChange={handleTextChange}
                placeholder="e.g. follow-up, demo, urgent"
              />
            </Grid>

            {/* Row 7: Notes / Details */}
            <Grid item xs={12}>
              <TextField
                fullWidth
                size="small"
                label="Notes / Details"
                name="details"
                value={formData.details}
                onChange={handleTextChange}
                multiline
                rows={3}
              />
            </Grid>

          </Grid>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <DialogSuccess message={dialogApi.success} />
          <Button onClick={handleCloseDialog} disabled={dialogApi.loading}>Cancel</Button>
          <ActionButton
            onClick={handleSaveActivity}
            loading={dialogApi.loading}
            variant="contained"
            sx={{ backgroundColor: '#6750A4' }}
          >
            {editingId ? 'Update Activity' : 'Create Activity'}
          </ActionButton>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default ActivitiesPage;

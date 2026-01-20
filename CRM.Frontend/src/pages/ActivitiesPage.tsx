import { useState, useEffect } from 'react';
import {
  Box, Card, CardContent, Typography, Button, Container, Alert, CircularProgress,
  FormControl, Select, MenuItem, Chip, Grid, Paper, Avatar, TextField,
  InputAdornment
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
  Refresh as RefreshIcon, TrendingDown as TrendingDownIcon
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';

// Activity type mappings
const ACTIVITY_ICONS: Record<string, React.ReactElement> = {
  'CustomerCreated': <PersonIcon />,
  'CustomerUpdated': <EditIcon />,
  'CustomerDeleted': <DeleteIcon />,
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
  'CustomerCreated': '#4caf50',
  'CustomerUpdated': '#2196f3',
  'CustomerDeleted': '#f44336',
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

interface Activity {
  id: number;
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
  createdAt: string;
}

interface ActivityStats {
  totalActivities: number;
  todayActivities: number;
  weekActivities: number;
  topActivityTypes: { type: string; count: number }[];
}

function ActivitiesPage() {
  const [activities, setActivities] = useState<Activity[]>([]);
  const [stats, setStats] = useState<ActivityStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [entityFilter, setEntityFilter] = useState<string>('all');
  const [searchQuery, setSearchQuery] = useState('');
  const [limit, setLimit] = useState(50);

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
                <MenuItem value="Customer">Customers</MenuItem>
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
                          {formatTimeAgo(activity.createdAt)}
                        </Typography>
                        <Typography variant="caption" color="textSecondary" display="block">
                          {new Date(activity.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
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
                            <Box sx={{ textAlign: 'right' }}>
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
    </Box>
  );
}

export default ActivitiesPage;

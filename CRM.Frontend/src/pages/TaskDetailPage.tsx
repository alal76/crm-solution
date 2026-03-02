import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box, Card, CardContent, Typography, Button, Chip, Grid, CircularProgress,
  Alert, Container, Divider, LinearProgress, IconButton, Tooltip
} from '@mui/material';
import {
  ArrowBack as ArrowBackIcon,
  Edit as EditIcon,
  Assignment as TaskIcon,
  Warning as WarningIcon,
  CheckCircle as CheckIcon,
  Group as GroupIcon,
  Business as BusinessIcon,
  Person as PersonIcon,
  CalendarToday as CalendarIcon,
  Label as LabelIcon,
  Category as CategoryIcon,
} from '@mui/icons-material';
import apiClient from '../services/apiClient';

const STATUS_COLORS: Record<string, string> = {
  'NotStarted': '#9e9e9e',
  'InProgress': '#2196f3',
  'Completed': '#4caf50',
  'Deferred': '#ff9800',
  'Waiting': '#607d8b',
  'Cancelled': '#f44336',
};

const PRIORITY_COLORS: Record<string, string> = {
  'Low': '#9e9e9e',
  'Normal': '#2196f3',
  'High': '#ff9800',
  'Urgent': '#f44336',
};

interface TaskDetail {
  id: number;
  subject: string;
  description?: string;
  taskType: string;
  status: string;
  priority: string;
  dueDate?: string;
  startDate?: string;
  completedDate?: string;
  percentComplete: number;
  estimatedMinutes?: number;
  actualMinutes?: number;
  accountId?: number;
  accountName?: string;
  opportunityId?: number;
  opportunityName?: string;
  assignedToUserId?: number;
  assignedToUserName?: string;
  assignedToGroupId?: number;
  assignedToGroupName?: string;
  tags?: string;
  category?: string;
  isOverdue: boolean;
  createdAt: string;
  updatedAt: string;
}

function DetailRow({ icon, label, children }: { icon: React.ReactNode; label: string; children: React.ReactNode }) {
  return (
    <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1.5, py: 1.5 }}>
      <Box sx={{ color: 'text.secondary', mt: 0.25, flexShrink: 0 }}>{icon}</Box>
      <Box sx={{ flex: 1 }}>
        <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 0.25 }}>
          {label}
        </Typography>
        {children}
      </Box>
    </Box>
  );
}

function TaskDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [task, setTask] = useState<TaskDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    setLoading(true);
    apiClient.get(`/tasks/${id}`)
      .then(res => {
        const data = res.data;
        // Normalise subject vs title field from API
        setTask({ ...data, subject: data.subject || data.title || '' });
      })
      .catch(err => {
        setError(err.response?.data?.message || 'Failed to load task details');
      })
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error || !task) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="error" sx={{ mb: 2 }}>
          {error || 'Task not found'}
        </Alert>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/my-queue')}>
          Back to My Queue
        </Button>
      </Container>
    );
  }

  const formatDate = (dateStr?: string) =>
    dateStr ? new Date(dateStr).toLocaleString([], { dateStyle: 'medium', timeStyle: 'short' }) : '—';

  const formatMinutes = (mins?: number) => {
    if (!mins) return '—';
    const h = Math.floor(mins / 60);
    const m = mins % 60;
    return h > 0 ? `${h}h ${m}m` : `${m}m`;
  };

  return (
    <Box sx={{ py: 4 }}>
      <Container maxWidth="lg">

        {/* Header */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
            <Tooltip title="Back to My Queue">
              <IconButton onClick={() => navigate('/my-queue')} color="primary">
                <ArrowBackIcon />
              </IconButton>
            </Tooltip>
            <TaskIcon sx={{ color: '#6750A4', fontSize: 32 }} />
            <Box>
              <Typography variant="h5" sx={{ fontWeight: 700 }}>
                {task.subject}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Task #{task.id}
              </Typography>
            </Box>
          </Box>
          <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
            {task.isOverdue && (
              <Chip
                icon={<WarningIcon />}
                label="Overdue"
                color="error"
                size="small"
              />
            )}
            <Button
              variant="contained"
              startIcon={<EditIcon />}
              onClick={() => navigate('/my-queue')}
              sx={{ backgroundColor: '#6750A4' }}
            >
              Edit in Queue
            </Button>
          </Box>
        </Box>

        <Grid container spacing={3}>

          {/* Left: Core Details */}
          <Grid item xs={12} md={8}>
            <Card sx={{ mb: 3 }}>
              <CardContent>
                <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1 }}>
                  Summary
                </Typography>
                <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 2 }}>
                  <Chip
                    label={typeof task.status === 'string' ? task.status.replaceAll(/([A-Z])/g, ' $1').trim() : 'Unknown'}
                    size="small"
                    sx={{ backgroundColor: STATUS_COLORS[task.status as keyof typeof STATUS_COLORS] || '#9e9e9e', color: 'white' }}
                  />
                  <Chip
                    label={task.priority}
                    size="small"
                    sx={{ backgroundColor: PRIORITY_COLORS[task.priority] || '#9e9e9e', color: 'white' }}
                  />
                  <Chip label={task.taskType} size="small" variant="outlined" />
                </Box>

                {task.description && (
                  <>
                    <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 0.5 }}>
                      Description
                    </Typography>
                    <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap', mb: 2 }}>
                      {task.description}
                    </Typography>
                  </>
                )}

                {/* Progress Bar */}
                <Box sx={{ mt: 1 }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                    <Typography variant="caption" color="text.secondary">Progress</Typography>
                    <Typography variant="caption" color="text.secondary">
                      {task.percentComplete}%
                    </Typography>
                  </Box>
                  <LinearProgress
                    variant="determinate"
                    value={task.percentComplete}
                    sx={{
                      height: 8,
                      borderRadius: 4,
                      backgroundColor: '#e0e0e0',
                      '& .MuiLinearProgress-bar': {
                        backgroundColor: task.percentComplete === 100 ? '#4caf50' : '#6750A4',
                        borderRadius: 4,
                      },
                    }}
                  />
                </Box>
              </CardContent>
            </Card>

            {/* Dates Card */}
            <Card>
              <CardContent>
                <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1 }}>
                  Dates &amp; Time
                </Typography>
                <Divider sx={{ mb: 1 }} />
                <Grid container spacing={1}>
                  <Grid item xs={12} sm={6}>
                    <DetailRow icon={<CalendarIcon fontSize="small" />} label="Due Date">
                      <Typography
                        variant="body2"
                        sx={{ color: task.isOverdue ? '#f44336' : 'text.primary', fontWeight: task.isOverdue ? 600 : 400 }}
                      >
                        {task.dueDate ? formatDate(task.dueDate) : '—'}
                      </Typography>
                    </DetailRow>
                  </Grid>
                  <Grid item xs={12} sm={6}>
                    <DetailRow icon={<CalendarIcon fontSize="small" />} label="Start Date">
                      <Typography variant="body2">{formatDate(task.startDate)}</Typography>
                    </DetailRow>
                  </Grid>
                  <Grid item xs={12} sm={6}>
                    <DetailRow icon={<CheckIcon fontSize="small" />} label="Completed Date">
                      <Typography variant="body2">{formatDate(task.completedDate)}</Typography>
                    </DetailRow>
                  </Grid>
                  <Grid item xs={12} sm={6}>
                    <DetailRow icon={<CalendarIcon fontSize="small" />} label="Created">
                      <Typography variant="body2">{formatDate(task.createdAt)}</Typography>
                    </DetailRow>
                  </Grid>
                  <Grid item xs={12} sm={6}>
                    <DetailRow icon={<CalendarIcon fontSize="small" />} label="Est. Duration">
                      <Typography variant="body2">{formatMinutes(task.estimatedMinutes)}</Typography>
                    </DetailRow>
                  </Grid>
                  <Grid item xs={12} sm={6}>
                    <DetailRow icon={<CalendarIcon fontSize="small" />} label="Actual Duration">
                      <Typography variant="body2">{formatMinutes(task.actualMinutes)}</Typography>
                    </DetailRow>
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>

          {/* Right: Assignment & Relations */}
          <Grid item xs={12} md={4}>
            <Card sx={{ mb: 3 }}>
              <CardContent>
                <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1 }}>
                  Assignment
                </Typography>
                <Divider sx={{ mb: 1 }} />
                <DetailRow icon={<PersonIcon fontSize="small" />} label="Assigned To">
                  <Typography variant="body2">{task.assignedToUserName || '—'}</Typography>
                </DetailRow>
                <DetailRow icon={<GroupIcon fontSize="small" />} label="Assigned Group">
                  {task.assignedToGroupName ? (
                    <Chip
                      icon={<GroupIcon />}
                      label={task.assignedToGroupName}
                      size="small"
                      variant="outlined"
                      color="primary"
                    />
                  ) : (
                    <Typography variant="body2">—</Typography>
                  )}
                </DetailRow>
              </CardContent>
            </Card>

            <Card sx={{ mb: 3 }}>
              <CardContent>
                <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1 }}>
                  Related Records
                </Typography>
                <Divider sx={{ mb: 1 }} />
                <DetailRow icon={<BusinessIcon fontSize="small" />} label="Account">
                  {task.accountName ? (
                    <Chip label={task.accountName} size="small" variant="outlined" />
                  ) : (
                    <Typography variant="body2">—</Typography>
                  )}
                </DetailRow>
                <DetailRow icon={<BusinessIcon fontSize="small" />} label="Opportunity">
                  {task.opportunityName ? (
                    <Chip label={task.opportunityName} size="small" variant="outlined" color="secondary" />
                  ) : (
                    <Typography variant="body2">—</Typography>
                  )}
                </DetailRow>
              </CardContent>
            </Card>

            {(task.tags || task.category) && (
              <Card>
                <CardContent>
                  <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1 }}>
                    Classification
                  </Typography>
                  <Divider sx={{ mb: 1 }} />
                  {task.category && (
                    <DetailRow icon={<CategoryIcon fontSize="small" />} label="Category">
                      <Chip label={task.category} size="small" variant="outlined" />
                    </DetailRow>
                  )}
                  {task.tags && (
                    <DetailRow icon={<LabelIcon fontSize="small" />} label="Tags">
                      <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                        {task.tags.split(',').map(tag => tag.trim()).filter(Boolean).map(tag => (
                          <Chip key={tag} label={tag} size="small" variant="outlined" />
                        ))}
                      </Box>
                    </DetailRow>
                  )}
                </CardContent>
              </Card>
            )}
          </Grid>
        </Grid>

        {/* Footer navigation */}
        <Box sx={{ mt: 3 }}>
          <Button
            startIcon={<ArrowBackIcon />}
            onClick={() => navigate('/my-queue')}
            variant="outlined"
          >
            Back to My Queue
          </Button>
        </Box>

      </Container>
    </Box>
  );
}

export default TaskDetailPage;

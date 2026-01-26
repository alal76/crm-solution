/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Workflow Instance Monitor - Monitor and manage workflow executions
 */

import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Button,
  IconButton,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  Chip,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
  CircularProgress,
  Tabs,
  Tab,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Tooltip,
  Card,
  CardContent,
  Grid,
  Divider,
  LinearProgress,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  ListItemSecondaryAction,
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  ArrowBack as BackIcon,
  PlayArrow as PlayIcon,
  Pause as PauseIcon,
  Stop as StopIcon,
  Replay as RetryIcon,
  SkipNext as SkipIcon,
  Visibility as ViewIcon,
  Error as ErrorIcon,
  CheckCircle as SuccessIcon,
  Schedule as PendingIcon,
  HourglassEmpty as WaitingIcon,
  DonutLarge as RunningIcon,
  Cancel as CancelledIcon,
  AccessTime as TimeIcon,
  Person as PersonIcon,
  Assignment as TaskIcon,
  FilterList as FilterIcon,
  Clear as ClearIcon,
  Download as DownloadIcon,
  Timeline as TimelineIcon,
  Assessment as AssessmentIcon,
} from '@mui/icons-material';
import {
  workflowInstanceService,
  WorkflowInstance,
  WorkflowLog,
  HumanTask,
  statusColors,
} from '../../services/workflowService';

// Helper function for date formatting
const formatDate = (date: string | undefined, pattern: string = 'MMM dd, HH:mm') => {
  if (!date) return '';
  const d = new Date(date);
  if (pattern === 'MMM dd, HH:mm') {
    return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }) + ', ' + 
           d.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: false });
  }
  if (pattern === 'PPpp') {
    return d.toLocaleDateString('en-US', { dateStyle: 'medium' }) + ' ' + 
           d.toLocaleTimeString('en-US', { timeStyle: 'short' });
  }
  if (pattern === 'HH:mm:ss.SSS') {
    return d.toLocaleTimeString('en-US', { hour12: false }) + '.' + 
           String(d.getMilliseconds()).padStart(3, '0');
  }
  if (pattern === 'MMM dd, yyyy HH:mm') {
    return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }) + ' ' +
           d.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: false });
  }
  return d.toLocaleString();
};

const formatDistanceToNow = (date: Date) => {
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  if (diffMins < 60) return `${diffMins} minutes`;
  const diffHours = Math.floor(diffMins / 60);
  if (diffHours < 24) return `${diffHours} hours`;
  const diffDays = Math.floor(diffHours / 24);
  return `${diffDays} days`;
};

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`instance-tabpanel-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ py: 2 }}>{children}</Box>}
    </div>
  );
}

const statusIcons: { [key: string]: React.ReactNode } = {
  Pending: <PendingIcon />,
  Running: <RunningIcon />,
  Waiting: <WaitingIcon />,
  Paused: <PauseIcon />,
  Completed: <SuccessIcon />,
  Failed: <ErrorIcon />,
  Cancelled: <CancelledIcon />,
  TimedOut: <TimeIcon />,
};

function WorkflowMonitorPage() {
  const { workflowId } = useParams<{ workflowId?: string }>();
  const navigate = useNavigate();

  // State
  const [instances, setInstances] = useState<WorkflowInstance[]>([]);
  const [selectedInstance, setSelectedInstance] = useState<any | null>(null);
  const [tasks, setTasks] = useState<HumanTask[]>([]);
  const [logs, setLogs] = useState<WorkflowLog[]>([]);
  const [statistics, setStatistics] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Pagination
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(25);
  const [totalCount, setTotalCount] = useState(0);

  // Filters
  const [statusFilter, setStatusFilter] = useState('');
  const [entityTypeFilter, setEntityTypeFilter] = useState('');
  const [searchQuery, setSearchQuery] = useState('');

  // UI State
  const [tabValue, setTabValue] = useState(0);
  const [detailDialogOpen, setDetailDialogOpen] = useState(false);
  const [detailTab, setDetailTab] = useState(0);
  const [autoRefresh, setAutoRefresh] = useState(false);

  // Load instances
  const loadInstances = useCallback(async () => {
    try {
      setLoading(true);
      const params: any = {
        pageNumber: page + 1,
        pageSize: rowsPerPage,
      };
      if (statusFilter) params.status = statusFilter;
      if (entityTypeFilter) params.entityType = entityTypeFilter;
      if (searchQuery) params.search = searchQuery;
      if (workflowId) params.workflowDefinitionId = parseInt(workflowId);

      const result = await workflowInstanceService.getInstances(params);
      setInstances(result.items || []);
      setTotalCount(result.totalCount || 0);
      setError('');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load instances');
    } finally {
      setLoading(false);
    }
  }, [page, rowsPerPage, statusFilter, entityTypeFilter, searchQuery, workflowId]);

  const loadStatistics = useCallback(async () => {
    try {
      const params = workflowId ? { workflowDefinitionId: parseInt(workflowId) } : undefined;
      const stats = await workflowInstanceService.getStatistics(params);
      setStatistics(stats);
    } catch (err) {
      console.error('Failed to load statistics:', err);
    }
  }, [workflowId]);

  useEffect(() => {
    loadInstances();
    loadStatistics();
  }, [loadInstances, loadStatistics]);

  // Auto refresh
  useEffect(() => {
    if (!autoRefresh) return;
    const interval = setInterval(() => {
      loadInstances();
      loadStatistics();
    }, 5000);
    return () => clearInterval(interval);
  }, [autoRefresh, loadInstances, loadStatistics]);

  // View instance details
  const viewInstance = async (instance: WorkflowInstance) => {
    try {
      setLoading(true);
      const details = await workflowInstanceService.getInstance(instance.id);
      setSelectedInstance(details);

      // Load logs
      const logsResult = await workflowInstanceService.getInstanceLogs(instance.id);
      setLogs(logsResult);

      setDetailDialogOpen(true);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load instance details');
    } finally {
      setLoading(false);
    }
  };

  // Instance actions
  const handleCancel = async (instanceId: number) => {
    try {
      await workflowInstanceService.cancelInstance(instanceId, 'Cancelled by user');
      setSuccess('Instance cancelled');
      loadInstances();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to cancel instance');
    }
  };

  const handlePause = async (instanceId: number) => {
    try {
      await workflowInstanceService.pauseInstance(instanceId);
      setSuccess('Instance paused');
      loadInstances();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to pause instance');
    }
  };

  const handleResume = async (instanceId: number) => {
    try {
      await workflowInstanceService.resumeInstance(instanceId);
      setSuccess('Instance resumed');
      loadInstances();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to resume instance');
    }
  };

  const handleRetry = async (instanceId: number) => {
    try {
      await workflowInstanceService.retryInstance(instanceId);
      setSuccess('Instance retry initiated');
      loadInstances();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to retry instance');
    }
  };

  // Load human tasks
  const loadMyTasks = async () => {
    try {
      setLoading(true);
      const result = await workflowInstanceService.getMyTasks();
      setTasks(result);
      setError('');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load tasks');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (tabValue === 1) {
      loadMyTasks();
    }
  }, [tabValue]);

  // Clear filters
  const clearFilters = () => {
    setStatusFilter('');
    setEntityTypeFilter('');
    setSearchQuery('');
  };

  // Render status chip
  const renderStatusChip = (status: string) => (
    <Chip
      icon={statusIcons[status] as React.ReactElement}
      label={status}
      size="small"
      sx={{
        backgroundColor: statusColors[status] || '#757575',
        color: 'white',
        '& .MuiChip-icon': { color: 'white' },
      }}
    />
  );

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3, gap: 2 }}>
        <IconButton onClick={() => navigate('/admin/workflows')}>
          <BackIcon />
        </IconButton>
        <Typography variant="h4" component="h1" sx={{ flex: 1 }}>
          Workflow Monitor
        </Typography>
        <Button
          variant={autoRefresh ? 'contained' : 'outlined'}
          startIcon={<RefreshIcon className={autoRefresh ? 'spin' : ''} />}
          onClick={() => setAutoRefresh(!autoRefresh)}
        >
          {autoRefresh ? 'Stop Auto-Refresh' : 'Auto-Refresh'}
        </Button>
        <Button
          variant="outlined"
          startIcon={<RefreshIcon />}
          onClick={() => { loadInstances(); loadStatistics(); }}
        >
          Refresh
        </Button>
      </Box>

      {/* Alerts */}
      {error && (
        <Alert severity="error" onClose={() => setError('')} sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}
      {success && (
        <Alert severity="success" onClose={() => setSuccess('')} sx={{ mb: 2 }}>
          {success}
        </Alert>
      )}

      {/* Statistics Cards */}
      {statistics && (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="h4" color="primary">
                  {statistics.totalInstances || 0}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Total Instances
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="h4" color="info.main">
                  {statistics.runningInstances || 0}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Running
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="h4" color="success.main">
                  {statistics.completedInstances || 0}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Completed
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="h4" color="error.main">
                  {statistics.failedInstances || 0}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Failed
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {/* Tabs */}
      <Paper sx={{ mb: 3 }}>
        <Tabs value={tabValue} onChange={(_, v) => setTabValue(v)}>
          <Tab icon={<TimelineIcon />} label="Instances" />
          <Tab icon={<TaskIcon />} label="My Tasks" />
          <Tab icon={<AssessmentIcon />} label="Statistics" />
        </Tabs>
      </Paper>

      {/* Instances Tab */}
      <TabPanel value={tabValue} index={0}>
        {/* Filters */}
        <Paper sx={{ p: 2, mb: 2 }}>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} sm={3}>
              <TextField
                fullWidth
                size="small"
                label="Search"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Entity ID, Correlation ID..."
              />
            </Grid>
            <Grid item xs={12} sm={3}>
              <FormControl fullWidth size="small">
                <InputLabel>Status</InputLabel>
                <Select
                  value={statusFilter}
                  label="Status"
                  onChange={(e) => setStatusFilter(e.target.value)}
                >
                  <MenuItem value="">All</MenuItem>
                  <MenuItem value="Pending">Pending</MenuItem>
                  <MenuItem value="Running">Running</MenuItem>
                  <MenuItem value="Waiting">Waiting</MenuItem>
                  <MenuItem value="Paused">Paused</MenuItem>
                  <MenuItem value="Completed">Completed</MenuItem>
                  <MenuItem value="Failed">Failed</MenuItem>
                  <MenuItem value="Cancelled">Cancelled</MenuItem>
                  <MenuItem value="TimedOut">Timed Out</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={3}>
              <FormControl fullWidth size="small">
                <InputLabel>Entity Type</InputLabel>
                <Select
                  value={entityTypeFilter}
                  label="Entity Type"
                  onChange={(e) => setEntityTypeFilter(e.target.value)}
                >
                  <MenuItem value="">All</MenuItem>
                  <MenuItem value="Contact">Contact</MenuItem>
                  <MenuItem value="Lead">Lead</MenuItem>
                  <MenuItem value="Opportunity">Opportunity</MenuItem>
                  <MenuItem value="Campaign">Campaign</MenuItem>
                  <MenuItem value="Order">Order</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={3}>
              <Button
                variant="text"
                startIcon={<ClearIcon />}
                onClick={clearFilters}
              >
                Clear Filters
              </Button>
            </Grid>
          </Grid>
        </Paper>

        {/* Instances Table */}
        <TableContainer component={Paper}>
          {loading && <LinearProgress />}
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>ID</TableCell>
                <TableCell>Workflow</TableCell>
                <TableCell>Entity</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Progress</TableCell>
                <TableCell>Started</TableCell>
                <TableCell>Duration</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {instances.map((instance) => (
                <TableRow key={instance.id} hover>
                  <TableCell>{instance.id}</TableCell>
                  <TableCell>{instance.workflowName || 'Unknown'}</TableCell>
                  <TableCell>
                    <Typography variant="body2">
                      {instance.entityType} #{instance.entityId}
                    </Typography>
                    {instance.correlationId && (
                      <Typography variant="caption" color="text.secondary">
                        {instance.correlationId}
                      </Typography>
                    )}
                  </TableCell>
                  <TableCell>{renderStatusChip(instance.status)}</TableCell>
                  <TableCell>
                    {instance.currentNodeName && (
                      <Typography variant="caption">
                        @ {instance.currentNodeName}
                      </Typography>
                    )}
                  </TableCell>
                  <TableCell>
                    {instance.startedAt
                      ? formatDate(instance.startedAt, 'MMM dd, HH:mm')
                      : '-'}
                  </TableCell>
                  <TableCell>
                    {instance.startedAt && (
                      <Typography variant="caption">
                        {instance.completedAt
                          ? `${Math.round((new Date(instance.completedAt).getTime() - new Date(instance.startedAt).getTime()) / 1000)}s`
                          : formatDistanceToNow(new Date(instance.startedAt))}
                      </Typography>
                    )}
                  </TableCell>
                  <TableCell>
                    <Tooltip title="View Details">
                      <IconButton size="small" onClick={() => viewInstance(instance)}>
                        <ViewIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    {instance.status === 'Running' && (
                      <Tooltip title="Pause">
                        <IconButton size="small" onClick={() => handlePause(instance.id)}>
                          <PauseIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    )}
                    {instance.status === 'Paused' && (
                      <Tooltip title="Resume">
                        <IconButton size="small" onClick={() => handleResume(instance.id)}>
                          <PlayIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    )}
                    {instance.status === 'Failed' && (
                      <Tooltip title="Retry">
                        <IconButton size="small" onClick={() => handleRetry(instance.id)}>
                          <RetryIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    )}
                    {['Running', 'Waiting', 'Paused'].includes(instance.status) && (
                      <Tooltip title="Cancel">
                        <IconButton
                          size="small"
                          color="error"
                          onClick={() => handleCancel(instance.id)}
                        >
                          <StopIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    )}
                  </TableCell>
                </TableRow>
              ))}
              {instances.length === 0 && !loading && (
                <TableRow>
                  <TableCell colSpan={8} align="center">
                    <Typography color="text.secondary" sx={{ py: 4 }}>
                      No instances found
                    </Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
          <TablePagination
            component="div"
            count={totalCount}
            page={page}
            rowsPerPage={rowsPerPage}
            onPageChange={(_, p) => setPage(p)}
            onRowsPerPageChange={(e) => {
              setRowsPerPage(parseInt(e.target.value, 10));
              setPage(0);
            }}
            rowsPerPageOptions={[10, 25, 50, 100]}
          />
        </TableContainer>
      </TabPanel>

      {/* My Tasks Tab */}
      <TabPanel value={tabValue} index={1}>
        <Paper>
          <List>
            {tasks.length === 0 ? (
              <ListItem>
                <ListItemText
                  primary="No pending tasks"
                  secondary="You have no workflow tasks assigned to you"
                />
              </ListItem>
            ) : (
              tasks.map((task) => (
                <ListItem key={task.id} divider>
                  <ListItemIcon>
                    <TaskIcon color={task.priority < 50 ? 'error' : 'primary'} />
                  </ListItemIcon>
                  <ListItemText
                    primary={task.name || `Task #${task.id}`}
                    secondary={
                      <>
                        <Typography component="span" variant="body2" color="text.secondary">
                          {task.nodeName} • Priority: {task.priority}
                        </Typography>
                        <br />
                        {task.dueAt && (
                          <Typography component="span" variant="caption">
                            Due: {formatDate(task.dueAt, 'MMM dd, yyyy HH:mm')}
                          </Typography>
                        )}
                      </>
                    }
                  />
                  <ListItemSecondaryAction>
                    <Chip
                      label="Pending"
                      size="small"
                      color="warning"
                    />
                    <Tooltip title="View Instance">
                      <IconButton
                        onClick={() =>
                          viewInstance({ id: task.workflowInstanceId } as WorkflowInstance)
                        }
                      >
                        <ViewIcon />
                      </IconButton>
                    </Tooltip>
                  </ListItemSecondaryAction>
                </ListItem>
              ))
            )}
          </List>
        </Paper>
      </TabPanel>

      {/* Statistics Tab */}
      <TabPanel value={tabValue} index={2}>
        {statistics && (
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <Paper sx={{ p: 3 }}>
                <Typography variant="h6" gutterBottom>
                  Instance Status Distribution
                </Typography>
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                  {Object.entries(statistics.statusDistribution || {}).map(([status, count]) => (
                    <Box key={status} sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                      <Box sx={{ width: 100 }}>
                        {renderStatusChip(status)}
                      </Box>
                      <Box sx={{ flex: 1 }}>
                        <LinearProgress
                          variant="determinate"
                          value={((count as number) / (statistics.totalInstances || 1)) * 100}
                          sx={{
                            height: 8,
                            borderRadius: 4,
                            backgroundColor: '#e0e0e0',
                            '& .MuiLinearProgress-bar': {
                              backgroundColor: statusColors[status] || '#757575',
                            },
                          }}
                        />
                      </Box>
                      <Typography variant="body2" sx={{ minWidth: 40 }}>
                        {count as number}
                      </Typography>
                    </Box>
                  ))}
                </Box>
              </Paper>
            </Grid>
            <Grid item xs={12} md={6}>
              <Paper sx={{ p: 3 }}>
                <Typography variant="h6" gutterBottom>
                  Performance Metrics
                </Typography>
                <Grid container spacing={2}>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">
                      Avg. Completion Time
                    </Typography>
                    <Typography variant="h5">
                      {statistics.avgCompletionTimeSeconds
                        ? `${Math.round(statistics.avgCompletionTimeSeconds)}s`
                        : 'N/A'}
                    </Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">
                      Success Rate
                    </Typography>
                    <Typography variant="h5" color="success.main">
                      {statistics.successRate
                        ? `${Math.round(statistics.successRate * 100)}%`
                        : 'N/A'}
                    </Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">
                      Active Workers
                    </Typography>
                    <Typography variant="h5">
                      {statistics.activeWorkers || 0}
                    </Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">
                      Pending Tasks
                    </Typography>
                    <Typography variant="h5" color="warning.main">
                      {statistics.pendingTasks || 0}
                    </Typography>
                  </Grid>
                </Grid>
              </Paper>
            </Grid>
          </Grid>
        )}
      </TabPanel>

      {/* Instance Detail Dialog */}
      <Dialog
        open={detailDialogOpen}
        onClose={() => setDetailDialogOpen(false)}
        maxWidth="lg"
        fullWidth
      >
        <DialogTitle>
          Instance Details #{selectedInstance?.id}
          {selectedInstance && (
            <Chip
              label={selectedInstance.status}
              size="small"
              sx={{
                ml: 2,
                backgroundColor: statusColors[selectedInstance.status] || '#757575',
                color: 'white',
              }}
            />
          )}
        </DialogTitle>
        <DialogContent>
          <Tabs value={detailTab} onChange={(_, v) => setDetailTab(v)} sx={{ mb: 2 }}>
            <Tab label="Overview" />
            <Tab label="Node Execution" />
            <Tab label="Logs" />
            <Tab label="Data" />
          </Tabs>

          {/* Overview */}
          {detailTab === 0 && selectedInstance && (
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">
                  Workflow
                </Typography>
                <Typography variant="body1">{selectedInstance.workflowName}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">
                  Version
                </Typography>
                <Typography variant="body1">v{selectedInstance.versionNumber}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">
                  Entity
                </Typography>
                <Typography variant="body1">
                  {selectedInstance.entityType} #{selectedInstance.entityId}
                </Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">
                  Correlation ID
                </Typography>
                <Typography variant="body1">
                  {selectedInstance.correlationId || 'N/A'}
                </Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">
                  Started At
                </Typography>
                <Typography variant="body1">
                  {selectedInstance.startedAt
                    ? formatDate(selectedInstance.startedAt, 'PPpp')
                    : 'Not started'}
                </Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">
                  Completed At
                </Typography>
                <Typography variant="body1">
                  {selectedInstance.completedAt
                    ? formatDate(selectedInstance.completedAt, 'PPpp')
                    : 'In progress'}
                </Typography>
              </Grid>
              {selectedInstance.errorMessage && (
                <Grid item xs={12}>
                  <Alert severity="error">{selectedInstance.errorMessage}</Alert>
                </Grid>
              )}
            </Grid>
          )}

          {/* Node Execution Timeline */}
          {detailTab === 1 && selectedInstance?.nodeInstances && (
            <Box sx={{ maxHeight: 400, overflow: 'auto' }}>
              {selectedInstance.nodeInstances
                .sort((a: any, b: any) => a.executionSequence - b.executionSequence)
                .map((node: any, index: number) => (
                  <Box
                    key={node.id}
                    sx={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: 2,
                      p: 1.5,
                      borderLeft: `3px solid ${statusColors[node.status] || '#757575'}`,
                      backgroundColor: index % 2 === 0 ? 'grey.50' : 'white',
                      mb: 0.5,
                    }}
                  >
                    <Box
                      sx={{
                        width: 32,
                        height: 32,
                        borderRadius: '50%',
                        backgroundColor: statusColors[node.status] || '#757575',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        color: 'white',
                        fontSize: 12,
                      }}
                    >
                      {node.executionSequence}
                    </Box>
                    <Box sx={{ flex: 1 }}>
                      <Typography variant="body2" fontWeight="medium">
                        {node.nodeName || `Node #${node.nodeId}`}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {node.nodeType}
                      </Typography>
                    </Box>
                    <Box sx={{ textAlign: 'right' }}>
                      <Chip label={node.status} size="small" />
                      {node.durationMs && (
                        <Typography variant="caption" display="block">
                          {node.durationMs}ms
                        </Typography>
                      )}
                    </Box>
                  </Box>
                ))}
            </Box>
          )}

          {/* Logs */}
          {detailTab === 2 && (
            <TableContainer sx={{ maxHeight: 400 }}>
              <Table size="small" stickyHeader>
                <TableHead>
                  <TableRow>
                    <TableCell>Time</TableCell>
                    <TableCell>Level</TableCell>
                    <TableCell>Category</TableCell>
                    <TableCell>Message</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {logs.map((log) => (
                    <TableRow
                      key={log.id}
                      sx={{
                        backgroundColor:
                          log.level === 'Error'
                            ? 'error.lighter'
                            : log.level === 'Warning'
                              ? 'warning.lighter'
                              : undefined,
                      }}
                    >
                      <TableCell>
                        <Typography variant="caption">
                          {formatDate(log.timestamp, 'HH:mm:ss.SSS')}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={log.level}
                          size="small"
                          color={
                            log.level === 'Error'
                              ? 'error'
                              : log.level === 'Warning'
                                ? 'warning'
                                : 'default'
                          }
                        />
                      </TableCell>
                      <TableCell>{log.category}</TableCell>
                      <TableCell>{log.message}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}

          {/* Data */}
          {detailTab === 3 && selectedInstance && (
            <Grid container spacing={2}>
              <Grid item xs={12} md={4}>
                <Typography variant="subtitle2" gutterBottom>
                  Input Data
                </Typography>
                <Paper
                  variant="outlined"
                  sx={{ p: 1, maxHeight: 300, overflow: 'auto', fontFamily: 'monospace', fontSize: 12 }}
                >
                  <pre>
                    {selectedInstance.inputData
                      ? JSON.stringify(JSON.parse(selectedInstance.inputData), null, 2)
                      : 'No input data'}
                  </pre>
                </Paper>
              </Grid>
              <Grid item xs={12} md={4}>
                <Typography variant="subtitle2" gutterBottom>
                  State Data
                </Typography>
                <Paper
                  variant="outlined"
                  sx={{ p: 1, maxHeight: 300, overflow: 'auto', fontFamily: 'monospace', fontSize: 12 }}
                >
                  <pre>
                    {selectedInstance.stateData
                      ? JSON.stringify(JSON.parse(selectedInstance.stateData), null, 2)
                      : 'No state data'}
                  </pre>
                </Paper>
              </Grid>
              <Grid item xs={12} md={4}>
                <Typography variant="subtitle2" gutterBottom>
                  Output Data
                </Typography>
                <Paper
                  variant="outlined"
                  sx={{ p: 1, maxHeight: 300, overflow: 'auto', fontFamily: 'monospace', fontSize: 12 }}
                >
                  <pre>
                    {selectedInstance.outputData
                      ? JSON.stringify(JSON.parse(selectedInstance.outputData), null, 2)
                      : 'No output data'}
                  </pre>
                </Paper>
              </Grid>
            </Grid>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDetailDialogOpen(false)}>Close</Button>
          {selectedInstance && ['Running', 'Waiting', 'Paused'].includes(selectedInstance.status) && (
            <Button
              color="error"
              onClick={() => {
                handleCancel(selectedInstance.id);
                setDetailDialogOpen(false);
              }}
            >
              Cancel Instance
            </Button>
          )}
        </DialogActions>
      </Dialog>

      {/* CSS for spin animation */}
      <style>{`
        @keyframes spin {
          from { transform: rotate(0deg); }
          to { transform: rotate(360deg); }
        }
        .spin {
          animation: spin 1s linear infinite;
        }
      `}</style>
    </Box>
  );
}

export default WorkflowMonitorPage;

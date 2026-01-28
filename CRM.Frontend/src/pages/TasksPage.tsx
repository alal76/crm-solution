import { useState, useEffect } from 'react';
import {
  Box, Card, CardContent, Typography, Button, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Dialog, DialogTitle, DialogContent, DialogActions, Alert, CircularProgress,
  TextField, Container, FormControl, InputLabel, Select, MenuItem, Chip, Grid,
  IconButton, Tooltip, Tabs, Tab, SelectChangeEvent, Badge
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, 
  CheckCircle as CheckIcon, Assignment as TaskIcon,
  Warning as WarningIcon, Inbox as InboxIcon, 
  Visibility as VisibilityIcon, Group as GroupIcon,
  Refresh as RefreshIcon
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import { TabPanel, DialogError } from '../components/common';
import { BaseEntity } from '../types';
import {
  TASK_STATUS_OPTIONS,
  TASK_PRIORITY_OPTIONS,
  TASK_TYPE_OPTIONS,
  getLabelByValue,
  getColorByValue
} from '../utils/constants';
import logo from '../assets/logo.png';
import LookupSelect from '../components/LookupSelect';
import EntitySelect from '../components/EntitySelect';
import ImportExportButtons from '../components/ImportExportButtons';
import AdvancedSearch, { SearchField, SearchFilter, filterData } from '../components/AdvancedSearch';

// Search fields for Advanced Search
const SEARCH_FIELDS: SearchField[] = [
  { name: 'subject', label: 'Title', type: 'text' },
  { name: 'status', label: 'Status', type: 'select', options: [
    { value: 'NotStarted', label: 'Not Started' },
    { value: 'InProgress', label: 'In Progress' },
    { value: 'Completed', label: 'Completed' },
    { value: 'Deferred', label: 'Deferred' },
    { value: 'Waiting', label: 'Waiting' },
    { value: 'Cancelled', label: 'Cancelled' },
  ]},
  { name: 'priority', label: 'Priority', type: 'select', options: [
    { value: 'Low', label: 'Low' },
    { value: 'Normal', label: 'Normal' },
    { value: 'High', label: 'High' },
    { value: 'Urgent', label: 'Urgent' },
  ]},
  { name: 'assignedToUserName', label: 'Assigned To', type: 'text' },
  { name: 'dueDate', label: 'Due Date', type: 'dateRange' },
];

const SEARCHABLE_FIELDS = ['subject', 'description', 'assignedToUserName', 'customerName', 'opportunityName', 'tags'];

// Status mapping for display
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

// Use shared constants - aliased for backward compatibility
const TASK_STATUSES = TASK_STATUS_OPTIONS;
const TASK_PRIORITIES = TASK_PRIORITY_OPTIONS;
const TASK_TYPES = TASK_TYPE_OPTIONS;

// Queue item from my-queue endpoint
interface QueueItem extends BaseEntity {
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
  customerId?: number;
  customerName?: string;
  opportunityId?: number;
  opportunityName?: string;
  assignedToUserId?: number;
  assignedToUserName?: string;
  assignedToGroupId?: number;
  assignedToGroupName?: string;
  tags?: string;
  category?: string;
  isOverdue: boolean;
}

interface QueueResponse {
  isWorkflowAdmin: boolean;
  tasks: QueueItem[];
  totalCount: number;
  pendingCount: number;
  overdueCount: number;
}

interface CrmTask extends BaseEntity {
  title: string;
  description?: string;
  taskType: number;
  status: number;
  priority: number;
  dueDate?: string;
  startDate?: string;
  completedDate?: string;
  estimatedHours?: number;
  actualHours?: number;
  percentComplete: number;
  assignedToUserId?: number;
  assignedToUser?: { firstName: string; lastName: string };
  customerId?: number;
  opportunityId?: number;
  location?: string;
  reminderDate?: string;
  tags?: string;
}

interface TaskForm {
  title: string;
  description: string;
  taskType: number;
  status: number;
  priority: number;
  dueDate: string;
  startDate: string;
  estimatedHours: number;
  actualHours: number;
  percentComplete: number;
  assignedToUserId: number | '';
  customerId: number | '';
  opportunityId: number | '';
  location: string;
  reminderDate: string;
  tags: string;
}

interface User extends BaseEntity {
  firstName: string;
  lastName: string;
}

function TasksPage() {
  const [queueData, setQueueData] = useState<QueueResponse | null>(null);
  const [tasks, setTasks] = useState<CrmTask[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [userGroups, setUserGroups] = useState<{ id: number; name: string }[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [dialogError, setDialogError] = useState<string | null>(null);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  const [statusFilter, setStatusFilter] = useState<string>('pending');
  const [viewMode, setViewMode] = useState<'queue' | 'all'>('queue');
  const [searchFilters, setSearchFilters] = useState<SearchFilter[]>([]);
  const [searchText, setSearchText] = useState('');

  const emptyForm: TaskForm = {
    title: '', description: '', taskType: 10, status: 0, priority: 1,
    dueDate: '', startDate: '', estimatedHours: 0, actualHours: 0,
    percentComplete: 0, assignedToUserId: '', customerId: '', opportunityId: '',
    location: '', reminderDate: '', tags: '',
  };
  const [formData, setFormData] = useState<TaskForm>(emptyForm);

  useEffect(() => {
    fetchMyQueue();
    fetchUsers();
    fetchUserGroups();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const fetchMyQueue = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/tasks/my-queue');
      setQueueData(response.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch queue');
    } finally {
      setLoading(false);
    }
  };

  const fetchTasks = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/tasks');
      setTasks(response.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch tasks');
    } finally {
      setLoading(false);
    }
  };

  const fetchUsers = async () => {
    try {
      const response = await apiClient.get('/users');
      setUsers(response.data);
    } catch (err) {
      console.error('Error fetching users:', err);
    }
  };

  const fetchUserGroups = async () => {
    try {
      const response = await apiClient.get('/usergroups');
      setUserGroups(response.data || []);
    } catch (err) {
      console.error('Error fetching user groups:', err);
    }
  };

  const handleOpenDialog = (task?: CrmTask) => {
    setDialogTab(0);
    if (task) {
      setEditingId(task.id);
      setFormData({
        title: task.title, description: task.description || '',
        taskType: task.taskType, status: task.status, priority: task.priority,
        dueDate: task.dueDate?.split('T')[0] || '', startDate: task.startDate?.split('T')[0] || '',
        estimatedHours: task.estimatedHours || 0, actualHours: task.actualHours || 0,
        percentComplete: task.percentComplete, assignedToUserId: task.assignedToUserId || '',
        customerId: task.customerId || '', opportunityId: task.opportunityId || '',
        location: task.location || '', reminderDate: task.reminderDate?.split('T')[0] || '',
        tags: task.tags || '',
      });
    } else {
      setEditingId(null);
      setFormData(emptyForm);
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => { setOpenDialog(false); setEditingId(null); setDialogError(null); };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'number' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleSelectChange = (e: SelectChangeEvent<string | number>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSaveTask = async () => {
    if (!formData.title.trim()) {
      setDialogError('Please enter a task title');
      return;
    }
    try {
      const payload = {
        ...formData,
        assignedToUserId: formData.assignedToUserId || null,
        customerId: formData.customerId || null,
        opportunityId: formData.opportunityId || null,
      };
      if (editingId) {
        await apiClient.put(`/tasks/${editingId}`, payload);
        setSuccessMessage('Task updated successfully');
      } else {
        await apiClient.post('/tasks', payload);
        setSuccessMessage('Task created successfully');
      }
      handleCloseDialog();
      fetchMyQueue();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setDialogError(err.response?.data?.message || 'Failed to save task');
    }
  };

  const handleCompleteTask = async (id: number) => {
    try {
      await apiClient.put(`/tasks/${id}/complete`);
      setSuccessMessage('Task marked as completed');
      fetchMyQueue();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to complete task');
    }
  };

  const handleDeleteTask = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this task?')) {
      try {
        await apiClient.delete(`/tasks/${id}`);
        setSuccessMessage('Task deleted successfully');
        fetchMyQueue();
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete task');
      }
    }
  };

  const getStatus = (value: number) => TASK_STATUSES.find(s => s.value === value);
  const getPriority = (value: number) => TASK_PRIORITIES.find(p => p.value === value);
  const getType = (value: number) => TASK_TYPES.find(t => t.value === value);

  const handleSearch = (filters: SearchFilter[], text: string) => {
    setSearchFilters(filters);
    setSearchText(text);
  };

  // Filter queue items based on status
  const statusFilteredItems = queueData?.tasks?.filter(item => {
    if (statusFilter === 'pending') {
      return item.status !== 'Completed' && item.status !== 'Cancelled';
    } else if (statusFilter === 'completed') {
      return item.status === 'Completed';
    } else if (statusFilter === 'overdue') {
      return item.isOverdue;
    }
    return true;
  }) || [];

  // Apply advanced search filters
  const filteredQueueItems = filterData(statusFilteredItems, searchFilters, searchText, SEARCHABLE_FIELDS);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ py: 4 }}>
      <Container maxWidth="xl">
        {/* Header */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
              <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
            </Box>
            <Box>
              <Typography variant="h4" sx={{ fontWeight: 700, display: 'flex', alignItems: 'center', gap: 1 }}>
                <InboxIcon /> My Queue
              </Typography>
              <Typography variant="body2" color="textSecondary">
                {queueData?.isWorkflowAdmin 
                  ? 'Workflow Admin: Viewing all tasks across all groups' 
                  : 'Tasks pending action for your group(s)'}
              </Typography>
            </Box>
          </Box>
          <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
            {queueData?.isWorkflowAdmin && (
              <Chip 
                icon={<VisibilityIcon />} 
                label="Workflow Admin" 
                color="primary" 
                variant="outlined" 
              />
            )}
            <IconButton onClick={fetchMyQueue} color="primary" title="Refresh">
              <RefreshIcon />
            </IconButton>
            <ImportExportButtons entityType="tasks" entityLabel="Tasks" onImportComplete={fetchMyQueue} />
            <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenDialog()} sx={{ backgroundColor: '#6750A4' }}>
              Add Task
            </Button>
          </Box>
        </Box>

        {/* Stats Cards */}
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={4}>
            <Card sx={{ backgroundColor: '#E8DEF8' }}>
              <CardContent sx={{ py: 2 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <TaskIcon sx={{ fontSize: 40, color: '#6750A4' }} />
                  <Box>
                    <Typography variant="h4" sx={{ fontWeight: 700, color: '#6750A4' }}>
                      {queueData?.totalCount || 0}
                    </Typography>
                    <Typography variant="body2" color="textSecondary">Total Tasks</Typography>
                  </Box>
                </Box>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={4}>
            <Card sx={{ backgroundColor: '#FFF3E0' }}>
              <CardContent sx={{ py: 2 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <Badge badgeContent={queueData?.pendingCount || 0} color="warning" max={999}>
                    <InboxIcon sx={{ fontSize: 40, color: '#E65100' }} />
                  </Badge>
                  <Box>
                    <Typography variant="h4" sx={{ fontWeight: 700, color: '#E65100' }}>
                      {queueData?.pendingCount || 0}
                    </Typography>
                    <Typography variant="body2" color="textSecondary">Pending Actions</Typography>
                  </Box>
                </Box>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={4}>
            <Card sx={{ backgroundColor: '#FFEBEE' }}>
              <CardContent sx={{ py: 2 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <WarningIcon sx={{ fontSize: 40, color: '#C62828' }} />
                  <Box>
                    <Typography variant="h4" sx={{ fontWeight: 700, color: '#C62828' }}>
                      {queueData?.overdueCount || 0}
                    </Typography>
                    <Typography variant="body2" color="textSecondary">Overdue</Typography>
                  </Box>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        </Grid>

        {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

        <AdvancedSearch
          fields={SEARCH_FIELDS}
          onSearch={handleSearch}
          placeholder="Search tasks by title, description, assignee..."
        />

        {/* Filter Controls */}
        <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
          <FormControl size="small" sx={{ minWidth: 180 }}>
            <InputLabel>Status Filter</InputLabel>
            <Select
              value={statusFilter}
              label="Status Filter"
              onChange={(e) => setStatusFilter(e.target.value)}
            >
              <MenuItem value="all">All Tasks</MenuItem>
              <MenuItem value="pending">Pending Only</MenuItem>
              <MenuItem value="completed">Completed</MenuItem>
              <MenuItem value="overdue">Overdue Only</MenuItem>
            </Select>
          </FormControl>
        </Box>

        {/* Queue Table */}
        <Card>
          <CardContent sx={{ p: 0 }}>
            <TableContainer sx={{ overflowX: 'auto' }}>
              <Table sx={{ minWidth: 1000 }}>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                  <TableCell><strong>Task</strong></TableCell>
                  <TableCell><strong>Type</strong></TableCell>
                  <TableCell><strong>Status</strong></TableCell>
                  <TableCell><strong>Priority</strong></TableCell>
                  <TableCell><strong>Assigned To</strong></TableCell>
                  <TableCell><strong>Group</strong></TableCell>
                  <TableCell><strong>Due Date</strong></TableCell>
                  <TableCell><strong>Related To</strong></TableCell>
                  <TableCell align="center"><strong>Actions</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredQueueItems.map((item) => (
                  <TableRow 
                    key={item.id} 
                    hover 
                    sx={{ 
                      backgroundColor: item.isOverdue ? '#fff3e0' : 'inherit',
                      opacity: item.status === 'Completed' || item.status === 'Cancelled' ? 0.6 : 1
                    }}
                  >
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <TaskIcon sx={{ color: item.status === 'Completed' ? '#4caf50' : '#6750A4' }} />
                        <Box>
                          <Typography 
                            fontWeight={500} 
                            sx={{ textDecoration: item.status === 'Completed' ? 'line-through' : 'none' }}
                          >
                            {item.subject}
                          </Typography>
                          {item.description && (
                            <Typography variant="caption" color="textSecondary" sx={{ display: 'block', maxWidth: 200, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                              {item.description}
                            </Typography>
                          )}
                        </Box>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Chip label={item.taskType} size="small" variant="outlined" />
                    </TableCell>
                    <TableCell>
                      <Chip 
                        label={item.status} 
                        size="small" 
                        sx={{ backgroundColor: STATUS_COLORS[item.status] || '#9e9e9e', color: 'white' }} 
                      />
                    </TableCell>
                    <TableCell>
                      <Chip 
                        label={item.priority} 
                        size="small" 
                        sx={{ backgroundColor: PRIORITY_COLORS[item.priority] || '#9e9e9e', color: 'white' }} 
                      />
                    </TableCell>
                    <TableCell>
                      {item.assignedToUserName || '—'}
                    </TableCell>
                    <TableCell>
                      {item.assignedToGroupName ? (
                        <Chip 
                          icon={<GroupIcon />} 
                          label={item.assignedToGroupName} 
                          size="small" 
                          variant="outlined" 
                          color="primary"
                        />
                      ) : '—'}
                    </TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        {item.isOverdue && <WarningIcon sx={{ color: '#f44336', fontSize: 18 }} />}
                        <Box>
                          <Typography variant="body2" sx={{ color: item.isOverdue ? '#f44336' : 'inherit' }}>
                            {item.dueDate ? new Date(item.dueDate).toLocaleDateString() : '—'}
                          </Typography>
                          {item.dueDate && (
                            <Typography variant="caption" color="textSecondary">
                              {new Date(item.dueDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                            </Typography>
                          )}
                        </Box>
                      </Box>
                    </TableCell>
                    <TableCell>
                      {item.customerName && (
                        <Chip label={item.customerName} size="small" variant="outlined" sx={{ mr: 0.5, mb: 0.5 }} />
                      )}
                      {item.opportunityName && (
                        <Chip label={item.opportunityName} size="small" variant="outlined" color="secondary" />
                      )}
                      {!item.customerName && !item.opportunityName && '—'}
                    </TableCell>
                    <TableCell align="center">
                      {item.status !== 'Completed' && (
                        <Tooltip title="Mark Complete">
                          <IconButton size="small" onClick={() => handleCompleteTask(item.id)} sx={{ color: '#4caf50' }}>
                            <CheckIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      )}
                      <Tooltip title="View/Edit">
                        <IconButton size="small" onClick={() => {
                          // Fetch the full task and open dialog
                          apiClient.get(`/tasks/${item.id}`).then(res => {
                            const task = res.data;
                            setEditingId(task.id);
                            setFormData({
                              title: task.subject || task.title || '',
                              description: task.description || '',
                              taskType: typeof task.taskType === 'number' ? task.taskType : 10,
                              status: typeof task.status === 'number' ? task.status : 0,
                              priority: typeof task.priority === 'number' ? task.priority : 1,
                              dueDate: task.dueDate?.split('T')[0] || '',
                              startDate: task.startDate?.split('T')[0] || '',
                              estimatedHours: task.estimatedMinutes ? task.estimatedMinutes / 60 : 0,
                              actualHours: task.actualMinutes ? task.actualMinutes / 60 : 0,
                              percentComplete: task.percentComplete || 0,
                              assignedToUserId: task.assignedToUserId || '',
                              customerId: task.customerId || '',
                              opportunityId: task.opportunityId || '',
                              location: '',
                              reminderDate: task.reminderDate?.split('T')[0] || '',
                              tags: task.tags || '',
                            });
                            setOpenDialog(true);
                          }).catch(() => setError('Failed to load task details'));
                        }} sx={{ color: '#6750A4' }}>
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Delete">
                        <IconButton size="small" onClick={() => handleDeleteTask(item.id)} sx={{ color: '#f44336' }}>
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
              </Table>
            </TableContainer>
            {filteredQueueItems.length === 0 && (
              <Typography sx={{ textAlign: 'center', py: 4, color: 'textSecondary' }}>
                {statusFilter === 'pending' 
                  ? 'No pending tasks in your queue. Great job!' 
                  : 'No tasks found matching the selected filter.'}
              </Typography>
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Add/Edit Task Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle sx={{ pb: 0 }}>{editingId ? 'Edit Task' : 'Add Task'}</DialogTitle>
        <Box sx={{ borderBottom: 1, borderColor: 'divider', px: 3 }}>
          <Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)}>
            <Tab label="Basic Info" />
            <Tab label="Scheduling" />
            <Tab label="Assignment" />
          </Tabs>
        </Box>
        <DialogContent sx={{ pt: 2, minHeight: 350 }}>
          <DialogError error={dialogError} onClose={() => setDialogError(null)} />
          <TabPanel value={dialogTab} index={0}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField fullWidth label="Task Title" name="title" value={formData.title} onChange={handleInputChange} required />
              </Grid>
              <Grid item xs={4}>
                <LookupSelect
                  category="TaskType"
                  name="taskType"
                  value={formData.taskType}
                  onChange={handleSelectChange}
                  label="Task Type"
                  fallback={TASK_TYPES.map(t => ({ value: t.value, label: t.label }))}
                />
              </Grid>
              <Grid item xs={4}>
                <LookupSelect
                  category="TaskStatus"
                  name="status"
                  value={formData.status}
                  onChange={handleSelectChange}
                  label="Status"
                  fallback={TASK_STATUSES.map(s => ({ value: s.value, label: s.label }))}
                />
              </Grid>
              <Grid item xs={4}>
                <LookupSelect
                  category="Priority"
                  name="priority"
                  value={formData.priority}
                  onChange={handleSelectChange}
                  label="Priority"
                  fallback={TASK_PRIORITIES.map(p => ({ value: p.value, label: p.label }))}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Description" name="description" value={formData.description} onChange={handleInputChange} multiline rows={3} />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Tags (comma-separated)" name="tags" value={formData.tags} onChange={handleInputChange} />
              </Grid>
            </Grid>
          </TabPanel>

          <TabPanel value={dialogTab} index={1}>
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <TextField fullWidth label="Start Date" name="startDate" type="date" value={formData.startDate} onChange={handleInputChange} InputLabelProps={{ shrink: true }} />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Due Date" name="dueDate" type="date" value={formData.dueDate} onChange={handleInputChange} InputLabelProps={{ shrink: true }} />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="Estimated Hours" name="estimatedHours" type="number" value={formData.estimatedHours} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="Actual Hours" name="actualHours" type="number" value={formData.actualHours} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="% Complete" name="percentComplete" type="number" value={formData.percentComplete} onChange={handleInputChange} inputProps={{ min: 0, max: 100 }} />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Reminder Date" name="reminderDate" type="date" value={formData.reminderDate} onChange={handleInputChange} InputLabelProps={{ shrink: true }} />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Location" name="location" value={formData.location} onChange={handleInputChange} />
              </Grid>
            </Grid>
          </TabPanel>

          <TabPanel value={dialogTab} index={2}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <EntitySelect
                  entityType="user"
                  name="assignedToUserId"
                  value={formData.assignedToUserId}
                  onChange={handleSelectChange}
                  label="Assigned To"
                  showAddNew={false}
                />
              </Grid>
            </Grid>
          </TabPanel>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSaveTask} variant="contained" sx={{ backgroundColor: '#6750A4' }}>
            {editingId ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default TasksPage;

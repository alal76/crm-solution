import { useState, useEffect } from 'react';
import {
  Box, Card, CardContent, Typography, Button, Table, TableBody, TableCell, TableHead,
  TableRow, Dialog, DialogTitle, DialogContent, DialogActions, Alert, CircularProgress,
  TextField, Container, FormControl, InputLabel, Select, MenuItem, Chip, Grid,
  IconButton, Tooltip, Tabs, Tab, SelectChangeEvent
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, 
  CheckCircle as CheckIcon, Assignment as TaskIcon,
  Warning as WarningIcon
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';
import LookupSelect from '../components/LookupSelect';

// Enums matching backend
const TASK_STATUSES = [
  { value: 0, label: 'Not Started', color: '#9e9e9e' },
  { value: 1, label: 'In Progress', color: '#2196f3' },
  { value: 2, label: 'Completed', color: '#4caf50' },
  { value: 3, label: 'On Hold', color: '#ff9800' },
  { value: 4, label: 'Cancelled', color: '#f44336' },
  { value: 5, label: 'Deferred', color: '#607d8b' },
];

const TASK_PRIORITIES = [
  { value: 0, label: 'Low', color: '#9e9e9e' },
  { value: 1, label: 'Medium', color: '#2196f3' },
  { value: 2, label: 'High', color: '#ff9800' },
  { value: 3, label: 'Critical', color: '#f44336' },
];

const TASK_TYPES = [
  { value: 0, label: 'Call' },
  { value: 1, label: 'Email' },
  { value: 2, label: 'Meeting' },
  { value: 3, label: 'Follow-Up' },
  { value: 4, label: 'Demo' },
  { value: 5, label: 'Proposal' },
  { value: 6, label: 'Research' },
  { value: 7, label: 'Documentation' },
  { value: 8, label: 'Review' },
  { value: 9, label: 'Approval' },
  { value: 10, label: 'Other' },
];

interface CrmTask {
  id: number;
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
  createdAt: string;
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

interface User {
  id: number;
  firstName: string;
  lastName: string;
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div role="tabpanel" hidden={value !== index} {...other}>
      {value === index && <Box sx={{ pt: 2 }}>{children}</Box>}
    </div>
  );
}

function TasksPage() {
  const [tasks, setTasks] = useState<CrmTask[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  const [filter, setFilter] = useState<'all' | 'due-today' | 'overdue'>('all');

  const emptyForm: TaskForm = {
    title: '', description: '', taskType: 10, status: 0, priority: 1,
    dueDate: '', startDate: '', estimatedHours: 0, actualHours: 0,
    percentComplete: 0, assignedToUserId: '', customerId: '', opportunityId: '',
    location: '', reminderDate: '', tags: '',
  };
  const [formData, setFormData] = useState<TaskForm>(emptyForm);

  useEffect(() => {
    fetchTasks();
    fetchUsers();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter]);

  const fetchTasks = async () => {
    try {
      setLoading(true);
      let endpoint = '/tasks';
      if (filter === 'due-today') endpoint = '/tasks/due-today';
      else if (filter === 'overdue') endpoint = '/tasks/overdue';
      const response = await apiClient.get(endpoint);
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

  const handleCloseDialog = () => { setOpenDialog(false); setEditingId(null); };

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
      setError('Please enter a task title');
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
      fetchTasks();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save task');
    }
  };

  const handleCompleteTask = async (id: number) => {
    try {
      await apiClient.put(`/tasks/${id}/complete`);
      setSuccessMessage('Task marked as completed');
      fetchTasks();
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
        fetchTasks();
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete task');
      }
    }
  };

  const getStatus = (value: number) => TASK_STATUSES.find(s => s.value === value);
  const getPriority = (value: number) => TASK_PRIORITIES.find(p => p.value === value);
  const getType = (value: number) => TASK_TYPES.find(t => t.value === value);

  const isOverdue = (task: CrmTask) => {
    if (!task.dueDate || task.status === 2) return false;
    return new Date(task.dueDate) < new Date();
  };

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
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
              <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
            </Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>Tasks</Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 2 }}>
            <FormControl size="small" sx={{ minWidth: 150 }}>
              <Select
                value={filter}
                onChange={(e) => setFilter(e.target.value as any)}
              >
                <MenuItem value="all">All Tasks</MenuItem>
                <MenuItem value="due-today">Due Today</MenuItem>
                <MenuItem value="overdue">Overdue</MenuItem>
              </Select>
            </FormControl>
            <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenDialog()} sx={{ backgroundColor: '#6750A4' }}>
              Add Task
            </Button>
          </Box>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

        <Card>
          <CardContent sx={{ p: 0 }}>
            <Table>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                  <TableCell><strong>Task</strong></TableCell>
                  <TableCell><strong>Type</strong></TableCell>
                  <TableCell><strong>Status</strong></TableCell>
                  <TableCell><strong>Priority</strong></TableCell>
                  <TableCell><strong>Assigned To</strong></TableCell>
                  <TableCell><strong>Due Date</strong></TableCell>
                  <TableCell><strong>Progress</strong></TableCell>
                  <TableCell align="center"><strong>Actions</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {tasks.map((task) => {
                  const status = getStatus(task.status);
                  const priority = getPriority(task.priority);
                  const type = getType(task.taskType);
                  const overdue = isOverdue(task);

                  return (
                    <TableRow key={task.id} hover sx={{ backgroundColor: overdue ? '#fff3e0' : 'inherit' }}>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <TaskIcon sx={{ color: task.status === 2 ? '#4caf50' : '#6750A4' }} />
                          <Box>
                            <Typography fontWeight={500} sx={{ textDecoration: task.status === 2 ? 'line-through' : 'none' }}>
                              {task.title}
                            </Typography>
                            {task.description && (
                              <Typography variant="caption" color="textSecondary" sx={{ display: 'block', maxWidth: 250, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                                {task.description}
                              </Typography>
                            )}
                          </Box>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Chip label={type?.label || 'Other'} size="small" variant="outlined" />
                      </TableCell>
                      <TableCell>
                        <Chip label={status?.label || 'Unknown'} size="small" sx={{ backgroundColor: status?.color, color: 'white' }} />
                      </TableCell>
                      <TableCell>
                        <Chip label={priority?.label || 'Medium'} size="small" sx={{ backgroundColor: priority?.color, color: 'white' }} />
                      </TableCell>
                      <TableCell>
                        {task.assignedToUser ? `${task.assignedToUser.firstName} ${task.assignedToUser.lastName}` : '—'}
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          {overdue && <WarningIcon sx={{ color: '#f44336', fontSize: 18 }} />}
                          <Box>
                            <Typography variant="body2" sx={{ color: overdue ? '#f44336' : 'inherit' }}>
                              {task.dueDate ? new Date(task.dueDate).toLocaleDateString() : '—'}
                            </Typography>
                            {task.dueDate && (
                              <Typography variant="caption" color="textSecondary">
                                {new Date(task.dueDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                              </Typography>
                            )}
                          </Box>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Box sx={{ width: 60, height: 6, backgroundColor: '#e0e0e0', borderRadius: 3, overflow: 'hidden' }}>
                            <Box sx={{ width: `${task.percentComplete}%`, height: '100%', backgroundColor: '#4caf50', borderRadius: 3 }} />
                          </Box>
                          <Typography variant="caption">{task.percentComplete}%</Typography>
                        </Box>
                      </TableCell>
                      <TableCell align="center">
                        {task.status !== 2 && (
                          <Tooltip title="Mark Complete">
                            <IconButton size="small" onClick={() => handleCompleteTask(task.id)} sx={{ color: '#4caf50' }}>
                              <CheckIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        )}
                        <Tooltip title="Edit">
                          <IconButton size="small" onClick={() => handleOpenDialog(task)} sx={{ color: '#6750A4' }}>
                            <EditIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton size="small" onClick={() => handleDeleteTask(task.id)} sx={{ color: '#f44336' }}>
                            <DeleteIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
            {tasks.length === 0 && (
              <Typography sx={{ textAlign: 'center', py: 4, color: 'textSecondary' }}>
                No tasks found. Create your first task to get started.
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
                <FormControl fullWidth>
                  <InputLabel>Assigned To</InputLabel>
                  <Select name="assignedToUserId" value={formData.assignedToUserId} onChange={handleSelectChange} label="Assigned To">
                    <MenuItem value="">Unassigned</MenuItem>
                    {users.map(u => <MenuItem key={u.id} value={u.id}>{u.firstName} {u.lastName}</MenuItem>)}
                  </Select>
                </FormControl>
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

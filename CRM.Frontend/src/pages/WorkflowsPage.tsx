import { useState, useEffect } from 'react';
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
  CircularProgress,
  Tabs,
  Tab,
  Chip,
} from '@mui/material';
import { Add as AddIcon } from '@mui/icons-material';
import apiClient from '../services/apiClient';
import { useNavigate } from 'react-router-dom';
import logo from '../assets/logo.png';

interface Workflow {
  id: number;
  name: string;
  description: string;
  entityType: string;
  isActive: boolean;
  priority: number;
}

interface QueueItem {
  id: number;
  workflowId: number;
  workflowRuleId?: number;
  entityType: string;
  entityId: number;
  sourceUserGroupId: number;
  targetUserGroupId: number;
  status: string;
  errorMessage?: string;
  entitySnapshotJson?: string;
  createdAt: string;
}

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
      id={`tabpanel-${index}`}
      aria-labelledby={`tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  );
}

function WorkflowsPage() {
  const [workflows, setWorkflows] = useState<Workflow[]>([]);
  const [queueItems, setQueueItems] = useState<QueueItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState<string>('All');
  const navigate = useNavigate();
  const [tabValue, setTabValue] = useState(0);
  const [openDialog, setOpenDialog] = useState(false);
  const [selectedWorkflow, setSelectedWorkflow] = useState<Workflow | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    entityType: 'Customer',
    isActive: true,
    priority: 100,
  });

  useEffect(() => {
    fetchQueue();
  }, []);

  const fetchWorkflows = async () => {
    try {
      setLoading(true);
      // Fetch workflows for all entity types
      const response = await apiClient.get('/workflows/Customer');
      setWorkflows(response.data || []);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch workflows');
      console.error('Error fetching workflows:', err);
    } finally {
      setLoading(false);
    }
  };

  const fetchQueue = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/workflows/queue');
      setQueueItems(response.data || []);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch queue');
      console.error('Error fetching queue:', err);
    } finally {
      setLoading(false);
    }
  };

  const getEntityRoute = (entityType: string, id: number) => {
    const t = entityType?.toLowerCase();
    switch (t) {
      case 'customer':
        return `/customers/${id}`;
      case 'account':
        return `/accounts/${id}`;
      case 'contact':
        return `/contacts/${id}`;
      case 'opportunity':
        return `/opportunities/${id}`;
      case 'lead':
        return `/leads/${id}`;
      default:
        return `/${t}/${id}`;
    }
  };

  const filteredQueue = queueItems.filter((q) => {
    if (statusFilter === 'All') return true;
    return (q.status || '').toLowerCase() === statusFilter.toLowerCase();
  });

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const handleOpenDialog = (workflow?: Workflow) => {
    if (workflow) {
      setSelectedWorkflow(workflow);
      setFormData({
        name: workflow.name,
        description: workflow.description,
        entityType: workflow.entityType,
        isActive: workflow.isActive,
        priority: workflow.priority,
      });
    } else {
      setSelectedWorkflow(null);
      setFormData({
        name: '',
        description: '',
        entityType: 'Customer',
        isActive: true,
        priority: 100,
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setSelectedWorkflow(null);
  };

  const handleSave = async () => {
    try {
      if (!formData.name || !formData.entityType) {
        setError('Please fill in all required fields');
        return;
      }

      if (selectedWorkflow) {
        // Update workflow
        await apiClient.put(`/workflows/${selectedWorkflow.id}`, formData);
      } else {
        // Create workflow
        await apiClient.post('/workflows', formData);
      }

      await fetchWorkflows();
      handleCloseDialog();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error saving workflow');
      console.error('Error saving workflow:', err);
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this workflow?')) {
      return;
    }

    try {
      await apiClient.delete(`/workflows/${id}`);
      await fetchWorkflows();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error deleting workflow');
      console.error('Error deleting workflow:', err);
    }
  };

  return (
    <Box sx={{ py: 2 }}>
      <Box sx={{ mb: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Box sx={{ width: 40, height: 40, flexShrink: 0 }}><img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} /></Box>
          <Box>
            <Typography variant="h3" sx={{ fontWeight: 700, mb: 0.5 }}>
              My Queue
            </Typography>
            <Typography color="textSecondary" variant="body2">
              Entities requiring action for your group(s)
            </Typography>
          </Box>
        </Box>
        {/* create workflow button hidden on My Queue view */}
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
        <Tabs value={tabValue} onChange={handleTabChange} aria-label="workflow tabs">
          <Tab label="Workflows" id="tab-0" aria-controls="tabpanel-0" />
          <Tab label="Execution History" id="tab-1" aria-controls="tabpanel-1" />
          <Tab label="Documentation" id="tab-2" aria-controls="tabpanel-2" />
        </Tabs>
      </Box>

      <TabPanel value={tabValue} index={0}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Card sx={{ borderRadius: 3, boxShadow: 1 }}>
            <CardContent sx={{ p: 0 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', p: 2 }}>
                <TextField
                  select
                  variant="outlined"
                  size="small"
                  label="Status"
                  value={statusFilter}
                  onChange={(e) => setStatusFilter(e.target.value)}
                  SelectProps={{ native: true }}
                  sx={{ width: 200 }}
                >
                  <option value="All">All</option>
                  <option value="Pending">Pending</option>
                  <option value="Success">Success</option>
                  <option value="Failed">Failed</option>
                </TextField>
                <Button size="small" onClick={() => fetchQueue()} sx={{ textTransform: 'none' }}>Refresh</Button>
              </Box>
              <Table>
                <TableHead>
                  <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                    <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Entity Type</TableCell>
                    <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Entity ID</TableCell>
                    <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Workflow</TableCell>
                    <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Status</TableCell>
                    <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Created</TableCell>
                    <TableCell sx={{ fontWeight: 600, color: '#6750A4' }} align="center">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {filteredQueue.map((item) => (
                    <TableRow key={item.id} sx={{ '&:hover': { backgroundColor: '#F5EFF7' }, borderBottom: '1px solid #E8DEF8' }}>
                      <TableCell>{item.entityType}</TableCell>
                      <TableCell>{item.entityId}</TableCell>
                      <TableCell>{item.workflowId}</TableCell>
                      <TableCell>
                        <Chip label={item.status} size="small" />
                      </TableCell>
                      <TableCell>{new Date(item.createdAt).toLocaleString()}</TableCell>
                      <TableCell align="center">
                        <Button size="small" sx={{ color: '#6750A4' }} onClick={() => navigate(getEntityRoute(item.entityType, item.entityId))}>Open</Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
              {queueItems.length === 0 && !loading && (
                <Typography sx={{ textAlign: 'center', py: 3, color: 'textSecondary' }}>
                  No items in your queue.
                </Typography>
              )}
            </CardContent>
          </Card>
        )}
      </TabPanel>

      <TabPanel value={tabValue} index={1}>
        <Card sx={{ borderRadius: 3, boxShadow: 1 }}>
          <CardContent>
            <Typography color="textSecondary">
              Coming soon: View execution history of workflows and track entity transfers between user groups.
            </Typography>
          </CardContent>
        </Card>
      </TabPanel>

      <TabPanel value={tabValue} index={2}>
        <Card sx={{ borderRadius: 3, boxShadow: 1 }}>
          <CardContent>
            <Typography variant="h6" sx={{ fontWeight: 600, mb: 2 }}>
              How to Use Workflows
            </Typography>
            <Typography variant="body2" paragraph>
              <strong>Workflows</strong> allow you to automatically transfer entities (Customers, Opportunities, Products, etc.) between user groups based on configured rules.
            </Typography>
            <Typography variant="body2" paragraph>
              <strong>Example:</strong> Create a workflow that automatically assigns all customers with an annual revenue greater than $1,000,000 to the "Enterprise Sales" group.
            </Typography>
            <Typography variant="h6" sx={{ fontWeight: 600, mb: 2, mt: 2 }}>
              Supported Operators
            </Typography>
            <ul>
              <li><strong>Equals / NotEquals:</strong> Exact match comparison</li>
              <li><strong>GreaterThan / LessThan:</strong> Numeric comparison</li>
              <li><strong>Contains:</strong> String contains check</li>
              <li><strong>In:</strong> Check if value exists in comma-separated list</li>
              <li><strong>Between:</strong> Check if value falls within range</li>
            </ul>
          </CardContent>
        </Card>
      </TabPanel>

      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 600, color: '#6750A4' }}>
          {selectedWorkflow ? 'Edit Workflow' : 'Create Workflow'}
        </DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <TextField
            fullWidth
            label="Workflow Name"
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            margin="normal"
            variant="outlined"
            required
          />
          <TextField
            fullWidth
            label="Description"
            value={formData.description}
            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            margin="normal"
            variant="outlined"
            multiline
            rows={3}
          />
          <TextField
            fullWidth
            label="Entity Type"
            select
            value={formData.entityType}
            onChange={(e) => setFormData({ ...formData, entityType: e.target.value })}
            margin="normal"
            variant="outlined"
            SelectProps={{ native: true }}
          >
            <option value="Customer">Customer</option>
            <option value="Opportunity">Opportunity</option>
            <option value="Product">Product</option>
            <option value="MarketingCampaign">Marketing Campaign</option>
          </TextField>
          <TextField
            fullWidth
            label="Priority"
            type="number"
            value={formData.priority}
            onChange={(e) => setFormData({ ...formData, priority: parseInt(e.target.value) })}
            margin="normal"
            variant="outlined"
            inputProps={{ min: 1, max: 1000 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button
            onClick={handleSave}
            variant="contained"
            sx={{ backgroundColor: '#6750A4', textTransform: 'none' }}
          >
            {selectedWorkflow ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default WorkflowsPage;

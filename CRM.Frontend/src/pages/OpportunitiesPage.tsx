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
  Alert,
  CircularProgress,
  TextField,
  Container,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';

interface Opportunity {
  id: number;
  title: string;
  customerId: number;
  expectedValue: number;
  expectedCloseDate: string;
  description?: string;
  createdAt: string;
}

interface Customer {
  id: number;
  name: string;
}

interface OpportunityForm {
  title: string;
  customerId: number;
  expectedValue: number;
  expectedCloseDate: string;
  description: string;
}

function OpportunitiesPage() {
  const [opportunities, setOpportunities] = useState<Opportunity[]>([]);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<OpportunityForm>({
    title: '',
    customerId: 0,
    expectedValue: 0,
    expectedCloseDate: '',
    description: '',
  });

  useEffect(() => {
    fetchAllData();
  }, []);

  const fetchAllData = async () => {
    try {
      setLoading(true);
      const [oppRes, custRes] = await Promise.all([
        apiClient.get('/opportunities'),
        apiClient.get('/customers'),
      ]);
      setOpportunities(oppRes.data);
      setCustomers(custRes.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch data');
      console.error('Error fetching data:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = (opp?: Opportunity) => {
    if (opp) {
      setEditingId(opp.id);
      setFormData({
        title: opp.title,
        customerId: opp.customerId,
        expectedValue: opp.expectedValue,
        expectedCloseDate: opp.expectedCloseDate.split('T')[0],
        description: opp.description || '',
      });
    } else {
      setEditingId(null);
      setFormData({
        title: '',
        customerId: customers[0]?.id || 0,
        expectedValue: 0,
        expectedCloseDate: '',
        description: '',
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
  };

  const handleSelectChange = (e: any) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'expectedValue' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleSaveOpportunity = async () => {
    if (!formData.title.trim() || !formData.customerId || !formData.expectedCloseDate) {
      setError('Please fill in required fields (Title, Customer, Close Date)');
      return;
    }

    try {
      if (editingId) {
        await apiClient.put(`/opportunities/${editingId}`, formData);
        setSuccessMessage('Opportunity updated successfully');
      } else {
        await apiClient.post('/opportunities', formData);
        setSuccessMessage('Opportunity created successfully');
      }
      handleCloseDialog();
      fetchAllData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save opportunity');
      console.error('Error saving opportunity:', err);
    }
  };

  const handleDeleteOpportunity = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this opportunity?')) {
      try {
        await apiClient.delete(`/opportunities/${id}`);
        setSuccessMessage('Opportunity deleted successfully');
        fetchAllData();
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete opportunity');
        console.error('Error deleting opportunity:', err);
      }
    }
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
      <Container maxWidth="lg">
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ width: 40, height: 40, flexShrink: 0 }}><img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} /></Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>Opportunities</Typography>
          </Box>
          <Button
            variant="contained"
            color="primary"
            startIcon={<AddIcon />}
            onClick={() => handleOpenDialog()}
            sx={{ backgroundColor: '#6750A4' }}
          >
            Add Opportunity
          </Button>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

        <Card>
          <CardContent>
            <Table>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                  <TableCell><strong>Title</strong></TableCell>
                  <TableCell><strong>Expected Value</strong></TableCell>
                  <TableCell><strong>Close Date</strong></TableCell>
                  <TableCell><strong>Description</strong></TableCell>
                  <TableCell align="center"><strong>Actions</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {opportunities.map((opp) => (
                  <TableRow key={opp.id}>
                    <TableCell>{opp.title}</TableCell>
                    <TableCell>${opp.expectedValue.toLocaleString()}</TableCell>
                    <TableCell>{new Date(opp.expectedCloseDate).toLocaleDateString()}</TableCell>
                    <TableCell>{opp.description || '-'}</TableCell>
                    <TableCell align="center">
                      <Button
                        size="small"
                        color="primary"
                        startIcon={<EditIcon />}
                        onClick={() => handleOpenDialog(opp)}
                        sx={{ mr: 1 }}
                      >
                        Edit
                      </Button>
                      <Button
                        size="small"
                        color="error"
                        startIcon={<DeleteIcon />}
                        onClick={() => handleDeleteOpportunity(opp.id)}
                      >
                        Delete
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
            {opportunities.length === 0 && (
              <Typography sx={{ textAlign: 'center', py: 2, color: 'textSecondary' }}>
                No opportunities found
              </Typography>
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Add/Edit Opportunity Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{editingId ? 'Edit Opportunity' : 'Create New Opportunity'}</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <TextField
            autoFocus
            fullWidth
            label="Title"
            name="title"
            value={formData.title}
            onChange={handleInputChange}
            margin="normal"
            required
          />
          <FormControl fullWidth margin="normal" required>
            <InputLabel>Customer</InputLabel>
            <Select
              name="customerId"
              value={formData.customerId}
              onChange={handleSelectChange}
              label="Customer"
            >
              {customers.map(cust => (
                <MenuItem key={cust.id} value={cust.id}>{cust.name}</MenuItem>
              ))}
            </Select>
          </FormControl>
          <TextField
            fullWidth
            label="Expected Value"
            name="expectedValue"
            type="number"
            value={formData.expectedValue}
            onChange={handleInputChange}
            margin="normal"
            inputProps={{ step: "0.01" }}
          />
          <TextField
            fullWidth
            label="Expected Close Date"
            name="expectedCloseDate"
            type="date"
            value={formData.expectedCloseDate}
            onChange={handleInputChange}
            margin="normal"
            InputLabelProps={{ shrink: true }}
            required
          />
          <TextField
            fullWidth
            label="Description"
            name="description"
            value={formData.description}
            onChange={handleInputChange}
            margin="normal"
            multiline
            rows={2}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSaveOpportunity} variant="contained" color="primary">
            {editingId ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default OpportunitiesPage;

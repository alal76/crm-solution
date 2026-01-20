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
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';

interface Service {
  id: number;
  name: string;
  description: string;
  category: string;
  hourlyRate: number;
  status: number;
  createdAt: string;
}

interface ServiceForm {
  name: string;
  description: string;
  category: string;
  hourlyRate: number;
  status: number;
}

function ServicesPage() {
  const [services, setServices] = useState<Service[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<ServiceForm>({
    name: '',
    description: '',
    category: '',
    hourlyRate: 0,
    status: 1,
  });

  useEffect(() => {
    fetchServices();
  }, []);

  const fetchServices = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/services');
      setServices(response.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch services');
      console.error('Error fetching services:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = (service?: Service) => {
    if (service) {
      setEditingId(service.id);
      setFormData({
        name: service.name,
        description: service.description,
        category: service.category,
        hourlyRate: service.hourlyRate,
        status: service.status,
      });
    } else {
      setEditingId(null);
      setFormData({
        name: '',
        description: '',
        category: '',
        hourlyRate: 0,
        status: 1,
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'hourlyRate' || name === 'status' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleSaveService = async () => {
    if (!formData.name.trim() || !formData.category.trim()) {
      setError('Please fill in required fields (Name, Category)');
      return;
    }

    try {
      if (editingId) {
        await apiClient.put(`/services/${editingId}`, formData);
        setSuccessMessage('Service updated successfully');
      } else {
        await apiClient.post('/services', formData);
        setSuccessMessage('Service created successfully');
      }
      handleCloseDialog();
      fetchServices();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save service');
      console.error('Error saving service:', err);
    }
  };

  const handleDeleteService = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this service?')) {
      try {
        await apiClient.delete(`/services/${id}`);
        setSuccessMessage('Service deleted successfully');
        fetchServices();
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete service');
        console.error('Error deleting service:', err);
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
            <Typography variant="h4" sx={{ fontWeight: 700 }}>Services</Typography>
          </Box>
          <Button
            variant="contained"
            color="primary"
            startIcon={<AddIcon />}
            onClick={() => handleOpenDialog()}
            sx={{ backgroundColor: '#6750A4' }}
          >
            Add Service
          </Button>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

        <Card>
          <CardContent>
            <Table>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                  <TableCell><strong>Name</strong></TableCell>
                  <TableCell><strong>Category</strong></TableCell>
                  <TableCell><strong>Description</strong></TableCell>
                  <TableCell><strong>Hourly Rate</strong></TableCell>
                  <TableCell align="center"><strong>Actions</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {services.map((service) => (
                  <TableRow key={service.id}>
                    <TableCell>{service.name}</TableCell>
                    <TableCell>{service.category}</TableCell>
                    <TableCell>{service.description}</TableCell>
                    <TableCell>${service.hourlyRate}/hr</TableCell>
                    <TableCell align="center">
                      <Button
                        size="small"
                        color="primary"
                        startIcon={<EditIcon />}
                        onClick={() => handleOpenDialog(service)}
                        sx={{ mr: 1 }}
                      >
                        Edit
                      </Button>
                      <Button
                        size="small"
                        color="error"
                        startIcon={<DeleteIcon />}
                        onClick={() => handleDeleteService(service.id)}
                      >
                        Delete
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
            {services.length === 0 && (
              <Typography sx={{ textAlign: 'center', py: 2, color: 'textSecondary' }}>
                No services found
              </Typography>
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Add/Edit Service Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{editingId ? 'Edit Service' : 'Create New Service'}</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <TextField
            autoFocus
            fullWidth
            label="Service Name"
            name="name"
            value={formData.name}
            onChange={handleInputChange}
            margin="normal"
            required
          />
          <TextField
            fullWidth
            label="Category"
            name="category"
            value={formData.category}
            onChange={handleInputChange}
            margin="normal"
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
          <TextField
            fullWidth
            label="Hourly Rate"
            name="hourlyRate"
            type="number"
            value={formData.hourlyRate}
            onChange={handleInputChange}
            margin="normal"
            inputProps={{ step: "0.01" }}
          />
          <TextField
            fullWidth
            label="Status"
            name="status"
            type="number"
            value={formData.status}
            onChange={handleInputChange}
            margin="normal"
            inputProps={{ step: "1" }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSaveService} variant="contained" color="primary">
            {editingId ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default ServicesPage;
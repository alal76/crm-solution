/**
 * CRM Solution - Services Page
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This page displays service-type products from the Products catalog.
 * Services are filtered from Products based on their type (Service, Consulting, 
 * ManagedService, ProfessionalServices, Training, SupportContract).
 */
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
  Chip,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  IconButton,
  Tooltip,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import { DialogError } from '../components/common';
import logo from '../assets/logo.png';

// Service types that are considered "Services" (subset of ProductType enum)
enum ServiceType {
  Service = 2,
  Consulting = 6,
  ManagedService = 7,
  SupportContract = 8,
  Training = 9,
  ProfessionalServices = 11,
}

// Product status enum
enum ProductStatus {
  Draft = 0,
  Active = 1,
  Discontinued = 2,
  OutOfStock = 3,
  ComingSoon = 4,
  Archived = 5,
  Limited = 6,
  Beta = 7,
  EndOfLife = 8,
}

const SERVICE_TYPES = [
  { value: ServiceType.Service, label: 'Service' },
  { value: ServiceType.Consulting, label: 'Consulting' },
  { value: ServiceType.ManagedService, label: 'Managed Service' },
  { value: ServiceType.SupportContract, label: 'Support Contract' },
  { value: ServiceType.Training, label: 'Training' },
  { value: ServiceType.ProfessionalServices, label: 'Professional Services' },
];

const PRODUCT_STATUSES = [
  { value: ProductStatus.Draft, label: 'Draft', color: 'default' as const },
  { value: ProductStatus.Active, label: 'Active', color: 'success' as const },
  { value: ProductStatus.Discontinued, label: 'Discontinued', color: 'error' as const },
  { value: ProductStatus.Beta, label: 'Beta', color: 'info' as const },
];

interface ServiceProduct {
  id: number;
  name: string;
  sku: string;
  description: string;
  shortDescription?: string;
  category: string;
  type: number;
  price: number;
  cost?: number;
  status: number;
  isActive: boolean;
  tags?: string;
  createdAt: string;
  updatedAt?: string;
}

interface ServiceForm {
  name: string;
  sku: string;
  description: string;
  shortDescription: string;
  category: string;
  type: number;
  price: number;
  cost: number;
  status: number;
  isActive: boolean;
  tags: string;
}

const initialFormData: ServiceForm = {
  name: '',
  sku: '',
  description: '',
  shortDescription: '',
  category: '',
  type: ServiceType.Service,
  price: 0,
  cost: 0,
  status: ProductStatus.Active,
  isActive: true,
  tags: '',
};

function ServicesPage() {
  const [services, setServices] = useState<ServiceProduct[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [dialogError, setDialogError] = useState<string | null>(null);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<ServiceForm>(initialFormData);

  useEffect(() => {
    fetchServices();
  }, []);

  const fetchServices = async () => {
    try {
      setLoading(true);
      setError(null);
      // Use the dedicated /products/services endpoint that filters service-type products
      const response = await apiClient.get<ServiceProduct[]>('/products/services');
      setServices(response.data);
    } catch (err: any) {
      console.error('Error fetching services:', err);
      setError(err.response?.data?.message || 'Failed to fetch services');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = (service?: ServiceProduct) => {
    if (service) {
      setEditingId(service.id);
      setFormData({
        name: service.name,
        sku: service.sku || '',
        description: service.description || '',
        shortDescription: service.shortDescription || '',
        category: service.category || '',
        type: service.type,
        price: service.price || 0,
        cost: service.cost || 0,
        status: service.status,
        isActive: service.isActive,
        tags: service.tags || '',
      });
    } else {
      setEditingId(null);
      setFormData(initialFormData);
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
    setDialogError(null);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'price' || name === 'cost' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleSelectChange = (name: string, value: any) => {
    setFormData(prev => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSaveService = async () => {
    if (!formData.name.trim()) {
      setDialogError('Please enter a service name');
      return;
    }

    if (!formData.sku.trim()) {
      setDialogError('Please enter a SKU');
      return;
    }

    try {
      const payload = {
        ...formData,
        // Ensure the type is a service type
        type: formData.type >= 2 ? formData.type : ServiceType.Service,
      };

      if (editingId) {
        await apiClient.put(`/products/${editingId}`, payload);
        setSuccessMessage('Service updated successfully');
      } else {
        await apiClient.post('/products', payload);
        setSuccessMessage('Service created successfully');
      }
      handleCloseDialog();
      fetchServices();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setDialogError(err.response?.data?.message || 'Failed to save service');
      console.error('Error saving service:', err);
    }
  };

  const handleDeleteService = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this service?')) {
      try {
        await apiClient.delete(`/products/${id}`);
        setSuccessMessage('Service deleted successfully');
        fetchServices();
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete service');
        console.error('Error deleting service:', err);
      }
    }
  };

  const getServiceTypeName = (type: number) => {
    return SERVICE_TYPES.find(t => t.value === type)?.label || 'Service';
  };

  const getStatusInfo = (status: number) => {
    return PRODUCT_STATUSES.find(s => s.value === status) || PRODUCT_STATUSES[0];
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
            <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
              <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
            </Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>Services</Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Tooltip title="Refresh">
              <IconButton onClick={fetchServices} color="primary">
                <RefreshIcon />
              </IconButton>
            </Tooltip>
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
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

        <Card>
          <CardContent>
            <Table>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                  <TableCell><strong>Name</strong></TableCell>
                  <TableCell><strong>SKU</strong></TableCell>
                  <TableCell><strong>Type</strong></TableCell>
                  <TableCell><strong>Category</strong></TableCell>
                  <TableCell align="right"><strong>Price</strong></TableCell>
                  <TableCell><strong>Status</strong></TableCell>
                  <TableCell align="center"><strong>Actions</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {services.map((service) => {
                  const statusInfo = getStatusInfo(service.status);
                  return (
                    <TableRow key={service.id} hover>
                      <TableCell>
                        <Typography fontWeight={500}>{service.name}</Typography>
                        {service.shortDescription && (
                          <Typography variant="body2" color="text.secondary">
                            {service.shortDescription}
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell>{service.sku}</TableCell>
                      <TableCell>
                        <Chip 
                          label={getServiceTypeName(service.type)} 
                          size="small" 
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell>{service.category || '-'}</TableCell>
                      <TableCell align="right">
                        ${service.price.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                      </TableCell>
                      <TableCell>
                        <Chip 
                          label={statusInfo.label} 
                          size="small" 
                          color={statusInfo.color}
                        />
                      </TableCell>
                      <TableCell align="center">
                        <Tooltip title="Edit">
                          <IconButton
                            size="small"
                            color="primary"
                            onClick={() => handleOpenDialog(service)}
                          >
                            <EditIcon />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton
                            size="small"
                            color="error"
                            onClick={() => handleDeleteService(service.id)}
                          >
                            <DeleteIcon />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
            {services.length === 0 && (
              <Typography sx={{ textAlign: 'center', py: 4, color: 'text.secondary' }}>
                No services found. Click "Add Service" to create your first service.
              </Typography>
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Add/Edit Service Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle sx={{ pb: 1 }}>
          {editingId ? 'Edit Service' : 'Create New Service'}
        </DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <DialogError error={dialogError} onClose={() => setDialogError(null)} />
          
          <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2 }}>
            <TextField
              fullWidth
              label="Service Name"
              name="name"
              value={formData.name}
              onChange={handleInputChange}
              required
            />
            <TextField
              fullWidth
              label="SKU"
              name="sku"
              value={formData.sku}
              onChange={handleInputChange}
              required
              placeholder="SVC-001"
            />
          </Box>

          <TextField
            fullWidth
            label="Short Description"
            name="shortDescription"
            value={formData.shortDescription}
            onChange={handleInputChange}
            margin="normal"
            placeholder="Brief summary of the service"
          />

          <TextField
            fullWidth
            label="Full Description"
            name="description"
            value={formData.description}
            onChange={handleInputChange}
            margin="normal"
            multiline
            rows={3}
          />

          <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2, mt: 2 }}>
            <FormControl fullWidth>
              <InputLabel>Service Type</InputLabel>
              <Select
                value={formData.type}
                label="Service Type"
                onChange={(e) => handleSelectChange('type', e.target.value)}
              >
                {SERVICE_TYPES.map(type => (
                  <MenuItem key={type.value} value={type.value}>{type.label}</MenuItem>
                ))}
              </Select>
            </FormControl>

            <TextField
              fullWidth
              label="Category"
              name="category"
              value={formData.category}
              onChange={handleInputChange}
              placeholder="e.g., IT Support, Consulting"
            />
          </Box>

          <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 2, mt: 2 }}>
            <TextField
              fullWidth
              label="Price"
              name="price"
              type="number"
              value={formData.price}
              onChange={handleInputChange}
              InputProps={{ startAdornment: <span>$</span> }}
              inputProps={{ step: "0.01", min: 0 }}
            />
            <TextField
              fullWidth
              label="Cost"
              name="cost"
              type="number"
              value={formData.cost}
              onChange={handleInputChange}
              InputProps={{ startAdornment: <span>$</span> }}
              inputProps={{ step: "0.01", min: 0 }}
            />
            <FormControl fullWidth>
              <InputLabel>Status</InputLabel>
              <Select
                value={formData.status}
                label="Status"
                onChange={(e) => handleSelectChange('status', e.target.value)}
              >
                {PRODUCT_STATUSES.map(status => (
                  <MenuItem key={status.value} value={status.value}>{status.label}</MenuItem>
                ))}
              </Select>
            </FormControl>
          </Box>

          <TextField
            fullWidth
            label="Tags"
            name="tags"
            value={formData.tags}
            onChange={handleInputChange}
            margin="normal"
            placeholder="Comma-separated tags"
            helperText="Enter tags separated by commas (e.g., remote, enterprise, 24x7)"
          />
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
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

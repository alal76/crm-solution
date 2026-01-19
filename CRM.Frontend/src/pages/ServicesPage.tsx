import { useState } from 'react';
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
  IconButton,
  Chip,
} from '@mui/material';
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  Add as AddIcon,
} from '@mui/icons-material';

interface Service {
  id: string;
  name: string;
  description: string;
  category: string;
  hourlyRate: number;
  status: 'available' | 'unavailable';
  capacity: number;
  allocated: number;
}

function ServicesPage() {
  const [services, setServices] = useState<Service[]>([
    {
      id: '1',
      name: 'Implementation Consulting',
      description: 'Expert guidance for system setup and configuration',
      category: 'Consulting',
      hourlyRate: 150,
      status: 'available',
      capacity: 40,
      allocated: 20,
    },
    {
      id: '2',
      name: 'Technical Support',
      description: '24/7 technical support and maintenance',
      category: 'Support',
      hourlyRate: 100,
      status: 'available',
      capacity: 80,
      allocated: 45,
    },
  ]);

  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    category: '',
    hourlyRate: '',
    status: 'available',
    capacity: '',
    allocated: '',
  });
  const [error, setError] = useState('');

  const handleOpenDialog = (service?: Service) => {
    if (service) {
      setEditingId(service.id);
      setFormData({
        name: service.name,
        description: service.description,
        category: service.category,
        hourlyRate: service.hourlyRate.toString(),
        status: service.status,
        capacity: service.capacity.toString(),
        allocated: service.allocated.toString(),
      });
    } else {
      setEditingId(null);
      setFormData({
        name: '',
        description: '',
        category: '',
        hourlyRate: '',
        status: 'available',
        capacity: '',
        allocated: '',
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setError('');
  };

  const handleSave = () => {
    if (!formData.name || !formData.category || !formData.hourlyRate || !formData.capacity) {
      setError('Please fill in all required fields');
      return;
    }

    if (editingId) {
      setServices(
        services.map((s) =>
          s.id === editingId
            ? {
                ...s,
                name: formData.name,
                description: formData.description,
                category: formData.category,
                hourlyRate: parseFloat(formData.hourlyRate),
                status: formData.status as any,
                capacity: parseInt(formData.capacity),
                allocated: parseInt(formData.allocated),
              }
            : s
        )
      );
    } else {
      const newService: Service = {
        id: Date.now().toString(),
        name: formData.name,
        description: formData.description,
        category: formData.category,
        hourlyRate: parseFloat(formData.hourlyRate),
        status: formData.status as any,
        capacity: parseInt(formData.capacity),
        allocated: parseInt(formData.allocated),
      };
      setServices([...services, newService]);
    }
    handleCloseDialog();
  };

  const handleDelete = (id: string) => {
    if (window.confirm('Are you sure you want to delete this service?')) {
      setServices(services.filter((s) => s.id !== id));
    }
  };

  return (
    <Box sx={{ py: 2 }}>
      <Box sx={{ mb: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Typography variant="h3" sx={{ fontWeight: 700, mb: 0.5 }}>
            Services
          </Typography>
          <Typography color="textSecondary" variant="body2">
            Manage your services and capacity allocation
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenDialog()}
          sx={{ backgroundColor: '#6750A4', textTransform: 'none', borderRadius: 2 }}
        >
          Add Service
        </Button>
      </Box>

      <Card sx={{ borderRadius: 3, boxShadow: 1 }}>
        <CardContent sx={{ p: 0 }}>
          <Table>
            <TableHead>
              <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Service Name</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Category</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }} align="right">
                  Hourly Rate
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Status</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }} align="center">
                  Capacity
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }} align="center">
                  Allocated
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }} align="center">
                  Actions
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {services.map((service) => {
                const utilizationPercent = (service.allocated / service.capacity) * 100;
                return (
                  <TableRow
                    key={service.id}
                    sx={{
                      '&:hover': { backgroundColor: '#F5EFF7' },
                      borderBottom: '1px solid #E8DEF8',
                    }}
                  >
                    <TableCell sx={{ fontWeight: 500 }}>{service.name}</TableCell>
                    <TableCell>{service.category}</TableCell>
                    <TableCell align="right" sx={{ fontWeight: 600, color: '#6750A4' }}>
                      ${service.hourlyRate}/hr
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={service.status === 'available' ? 'Available' : 'Unavailable'}
                        size="small"
                        sx={{
                          backgroundColor: service.status === 'available' ? '#E8F5E9' : '#FFEBEE',
                          color: service.status === 'available' ? '#06A77D' : '#B3261E',
                          fontWeight: 600,
                        }}
                      />
                    </TableCell>
                    <TableCell align="center">
                      <Typography variant="body2" sx={{ fontWeight: 500 }}>
                        {service.allocated}/{service.capacity} hrs
                      </Typography>
                      <Typography
                        variant="caption"
                        sx={{ color: utilizationPercent > 80 ? '#B3261E' : '#06A77D', fontWeight: 600 }}
                      >
                        {utilizationPercent.toFixed(0)}%
                      </Typography>
                    </TableCell>
                    <TableCell align="center">
                      <Typography variant="body2">{service.allocated} hrs</Typography>
                    </TableCell>
                    <TableCell align="center">
                      <IconButton
                        size="small"
                        onClick={() => handleOpenDialog(service)}
                        sx={{ color: '#6750A4' }}
                      >
                        <EditIcon fontSize="small" />
                      </IconButton>
                      <IconButton
                        size="small"
                        onClick={() => handleDelete(service.id)}
                        sx={{ color: '#B3261E' }}
                      >
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 600, color: '#6750A4' }}>
          {editingId ? 'Edit Service' : 'Add Service'}
        </DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}
          <TextField
            fullWidth
            label="Service Name"
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
            rows={2}
          />
          <TextField
            fullWidth
            label="Category"
            value={formData.category}
            onChange={(e) => setFormData({ ...formData, category: e.target.value })}
            margin="normal"
            variant="outlined"
            required
          />
          <TextField
            fullWidth
            label="Hourly Rate"
            type="number"
            value={formData.hourlyRate}
            onChange={(e) => setFormData({ ...formData, hourlyRate: e.target.value })}
            margin="normal"
            variant="outlined"
            required
            inputProps={{ step: '0.01' }}
          />
          <TextField
            fullWidth
            label="Total Capacity (hours)"
            type="number"
            value={formData.capacity}
            onChange={(e) => setFormData({ ...formData, capacity: e.target.value })}
            margin="normal"
            variant="outlined"
            required
          />
          <TextField
            fullWidth
            label="Currently Allocated (hours)"
            type="number"
            value={formData.allocated}
            onChange={(e) => setFormData({ ...formData, allocated: e.target.value })}
            margin="normal"
            variant="outlined"
          />
          <TextField
            fullWidth
            label="Status"
            select
            value={formData.status}
            onChange={(e) => setFormData({ ...formData, status: e.target.value })}
            margin="normal"
            variant="outlined"
            SelectProps={{ native: true }}
          >
            <option value="available">Available</option>
            <option value="unavailable">Unavailable</option>
          </TextField>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button
            onClick={handleSave}
            variant="contained"
            sx={{ backgroundColor: '#6750A4', textTransform: 'none' }}
          >
            {editingId ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default ServicesPage;

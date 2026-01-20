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
import logo from '../assets/logo.png';

interface Lead {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  company: string;
  source: 'website' | 'referral' | 'event' | 'cold_call' | 'social' | 'other';
  status: 'new' | 'contacted' | 'qualified' | 'converted' | 'lost';
  dateAdded: string;
}

function LeadsPage() {
  const [leads, setLeads] = useState<Lead[]>([
    {
      id: '1',
      firstName: 'John',
      lastName: 'Smith',
      email: 'john.smith@example.com',
      phone: '(555) 123-4567',
      company: 'TechStart Inc',
      source: 'website',
      status: 'new',
      dateAdded: '2026-01-15',
    },
    {
      id: '2',
      firstName: 'Sarah',
      lastName: 'Johnson',
      email: 'sarah.j@business.com',
      phone: '(555) 987-6543',
      company: 'Enterprise Solutions',
      source: 'referral',
      status: 'contacted',
      dateAdded: '2026-01-10',
    },
  ]);

  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    company: '',
    source: 'website',
    status: 'new',
  });
  const [error, setError] = useState('');

  const sourceColors: Record<string, { bg: string; text: string }> = {
    website: { bg: '#E3F2FD', text: '#1565C0' },
    referral: { bg: '#F3E5F5', text: '#6A1B9A' },
    event: { bg: '#E0F2F1', text: '#00695C' },
    cold_call: { bg: '#FFF3E0', text: '#E65100' },
    social: { bg: '#FCE4EC', text: '#C2185B' },
    other: { bg: '#F0F4C3', text: '#558B2F' },
  };

  const statusColors: Record<string, { bg: string; text: string }> = {
    new: { bg: '#E8DEF8', text: '#6750A4' },
    contacted: { bg: '#E1F5FE', text: '#0277BD' },
    qualified: { bg: '#E8F5E9', text: '#06A77D' },
    converted: { bg: '#F1F8E9', text: '#558B2F' },
    lost: { bg: '#FFEBEE', text: '#B3261E' },
  };

  const handleOpenDialog = (lead?: Lead) => {
    if (lead) {
      setEditingId(lead.id);
      setFormData({
        firstName: lead.firstName,
        lastName: lead.lastName,
        email: lead.email,
        phone: lead.phone,
        company: lead.company,
        source: lead.source,
        status: lead.status,
      });
    } else {
      setEditingId(null);
      setFormData({
        firstName: '',
        lastName: '',
        email: '',
        phone: '',
        company: '',
        source: 'website',
        status: 'new',
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setError('');
  };

  const handleSave = () => {
    if (!formData.firstName || !formData.lastName || !formData.email || !formData.company) {
      setError('Please fill in all required fields');
      return;
    }

    if (editingId) {
      setLeads(
        leads.map((l) =>
          l.id === editingId
            ? {
                ...l,
                firstName: formData.firstName,
                lastName: formData.lastName,
                email: formData.email,
                phone: formData.phone,
                company: formData.company,
                source: formData.source as any,
                status: formData.status as any,
              }
            : l
        )
      );
    } else {
      const newLead: Lead = {
        id: Date.now().toString(),
        firstName: formData.firstName,
        lastName: formData.lastName,
        email: formData.email,
        phone: formData.phone,
        company: formData.company,
        source: formData.source as any,
        status: formData.status as any,
        dateAdded: new Date().toISOString().split('T')[0],
      };
      setLeads([...leads, newLead]);
    }
    handleCloseDialog();
  };

  const handleDelete = (id: string) => {
    if (window.confirm('Are you sure you want to delete this lead?')) {
      setLeads(leads.filter((l) => l.id !== id));
    }
  };

  return (
    <Box sx={{ py: 2 }}>
      <Box sx={{ mb: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Box sx={{ width: 40, height: 40, flexShrink: 0 }}><img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} /></Box>
          <Box>
            <Typography variant="h3" sx={{ fontWeight: 700, mb: 0.5 }}>
              Leads
            </Typography>
            <Typography color="textSecondary" variant="body2">
              Manage and track your sales leads
            </Typography>
          </Box>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenDialog()}
          sx={{ backgroundColor: '#6750A4', textTransform: 'none', borderRadius: 2 }}
        >
          Add Lead
        </Button>
      </Box>

      <Card sx={{ borderRadius: 3, boxShadow: 1 }}>
        <CardContent sx={{ p: 0 }}>
          <Table>
            <TableHead>
              <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Name</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Email</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Company</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Source</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Status</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Date Added</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }} align="center">
                  Actions
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {leads.map((lead) => (
                <TableRow
                  key={lead.id}
                  sx={{
                    '&:hover': { backgroundColor: '#F5EFF7' },
                    borderBottom: '1px solid #E8DEF8',
                  }}
                >
                  <TableCell sx={{ fontWeight: 500 }}>
                    {lead.firstName} {lead.lastName}
                  </TableCell>
                  <TableCell>{lead.email}</TableCell>
                  <TableCell>{lead.company}</TableCell>
                  <TableCell>
                    <Chip
                      label={lead.source.replace('_', ' ').toUpperCase()}
                      size="small"
                      sx={{
                        backgroundColor: sourceColors[lead.source]?.bg,
                        color: sourceColors[lead.source]?.text,
                        fontWeight: 600,
                      }}
                    />
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={lead.status.charAt(0).toUpperCase() + lead.status.slice(1)}
                      size="small"
                      sx={{
                        backgroundColor: statusColors[lead.status]?.bg,
                        color: statusColors[lead.status]?.text,
                        fontWeight: 600,
                      }}
                    />
                  </TableCell>
                  <TableCell>{lead.dateAdded}</TableCell>
                  <TableCell align="center">
                    <IconButton
                      size="small"
                      onClick={() => handleOpenDialog(lead)}
                      sx={{ color: '#6750A4' }}
                    >
                      <EditIcon fontSize="small" />
                    </IconButton>
                    <IconButton
                      size="small"
                      onClick={() => handleDelete(lead.id)}
                      sx={{ color: '#B3261E' }}
                    >
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 600, color: '#6750A4' }}>
          {editingId ? 'Edit Lead' : 'Add Lead'}
        </DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}
          <TextField
            fullWidth
            label="First Name"
            value={formData.firstName}
            onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
            margin="normal"
            variant="outlined"
            required
          />
          <TextField
            fullWidth
            label="Last Name"
            value={formData.lastName}
            onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
            margin="normal"
            variant="outlined"
            required
          />
          <TextField
            fullWidth
            label="Email"
            type="email"
            value={formData.email}
            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            margin="normal"
            variant="outlined"
            required
          />
          <TextField
            fullWidth
            label="Phone"
            value={formData.phone}
            onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
            margin="normal"
            variant="outlined"
          />
          <TextField
            fullWidth
            label="Company"
            value={formData.company}
            onChange={(e) => setFormData({ ...formData, company: e.target.value })}
            margin="normal"
            variant="outlined"
            required
          />
          <TextField
            fullWidth
            label="Lead Source"
            select
            value={formData.source}
            onChange={(e) => setFormData({ ...formData, source: e.target.value })}
            margin="normal"
            variant="outlined"
            SelectProps={{ native: true }}
          >
            <option value="website">Website</option>
            <option value="referral">Referral</option>
            <option value="event">Event</option>
            <option value="cold_call">Cold Call</option>
            <option value="social">Social Media</option>
            <option value="other">Other</option>
          </TextField>
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
            <option value="new">New</option>
            <option value="contacted">Contacted</option>
            <option value="qualified">Qualified</option>
            <option value="converted">Converted</option>
            <option value="lost">Lost</option>
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

export default LeadsPage;

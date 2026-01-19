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
  LinearProgress,
} from '@mui/material';
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  Add as AddIcon,
} from '@mui/icons-material';

interface Campaign {
  id: string;
  name: string;
  type: 'email' | 'social' | 'webinar' | 'content' | 'event';
  status: 'planning' | 'active' | 'paused' | 'completed';
  startDate: string;
  endDate: string;
  budget: number;
  spent: number;
}

function CampaignsPage() {
  const [campaigns, setCampaigns] = useState<Campaign[]>([
    {
      id: '1',
      name: 'Q1 Email Campaign',
      type: 'email',
      status: 'active',
      startDate: '2026-01-01',
      endDate: '2026-03-31',
      budget: 5000,
      spent: 2500,
    },
    {
      id: '2',
      name: 'Spring Webinar Series',
      type: 'webinar',
      status: 'planning',
      startDate: '2026-04-01',
      endDate: '2026-06-30',
      budget: 10000,
      spent: 0,
    },
  ]);

  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    type: 'email',
    status: 'planning',
    startDate: '',
    endDate: '',
    budget: '',
    spent: '',
  });
  const [error, setError] = useState('');

  const typeColors: Record<string, { bg: string; text: string }> = {
    email: { bg: '#E3F2FD', text: '#1565C0' },
    social: { bg: '#F3E5F5', text: '#6A1B9A' },
    webinar: { bg: '#E0F2F1', text: '#00695C' },
    content: { bg: '#FFF3E0', text: '#E65100' },
    event: { bg: '#FCE4EC', text: '#C2185B' },
  };

  const statusColors: Record<string, { bg: string; text: string }> = {
    planning: { bg: '#E8DEF8', text: '#6750A4' },
    active: { bg: '#E8F5E9', text: '#06A77D' },
    paused: { bg: '#FFF3E0', text: '#F57C00' },
    completed: { bg: '#E1F5FE', text: '#0092BC' },
  };

  const handleOpenDialog = (campaign?: Campaign) => {
    if (campaign) {
      setEditingId(campaign.id);
      setFormData({
        name: campaign.name,
        type: campaign.type,
        status: campaign.status,
        startDate: campaign.startDate,
        endDate: campaign.endDate,
        budget: campaign.budget.toString(),
        spent: campaign.spent.toString(),
      });
    } else {
      setEditingId(null);
      setFormData({
        name: '',
        type: 'email',
        status: 'planning',
        startDate: '',
        endDate: '',
        budget: '',
        spent: '',
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setError('');
  };

  const handleSave = () => {
    if (!formData.name || !formData.startDate || !formData.endDate || !formData.budget) {
      setError('Please fill in all required fields');
      return;
    }

    if (editingId) {
      setCampaigns(
        campaigns.map((c) =>
          c.id === editingId
            ? {
                ...c,
                name: formData.name,
                type: formData.type as any,
                status: formData.status as any,
                startDate: formData.startDate,
                endDate: formData.endDate,
                budget: parseFloat(formData.budget),
                spent: parseFloat(formData.spent),
              }
            : c
        )
      );
    } else {
      const newCampaign: Campaign = {
        id: Date.now().toString(),
        name: formData.name,
        type: formData.type as any,
        status: formData.status as any,
        startDate: formData.startDate,
        endDate: formData.endDate,
        budget: parseFloat(formData.budget),
        spent: parseFloat(formData.spent),
      };
      setCampaigns([...campaigns, newCampaign]);
    }
    handleCloseDialog();
  };

  const handleDelete = (id: string) => {
    if (window.confirm('Are you sure you want to delete this campaign?')) {
      setCampaigns(campaigns.filter((c) => c.id !== id));
    }
  };

  return (
    <Box sx={{ py: 2 }}>
      <Box sx={{ mb: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Typography variant="h3" sx={{ fontWeight: 700, mb: 0.5 }}>
            Marketing Campaigns
          </Typography>
          <Typography color="textSecondary" variant="body2">
            Plan and track your marketing campaigns
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenDialog()}
          sx={{ backgroundColor: '#6750A4', textTransform: 'none', borderRadius: 2 }}
        >
          New Campaign
        </Button>
      </Box>

      <Card sx={{ borderRadius: 3, boxShadow: 1 }}>
        <CardContent sx={{ p: 0 }}>
          <Table>
            <TableHead>
              <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Campaign Name</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Type</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Status</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Budget</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }} align="center">
                  Progress
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }} align="center">
                  Actions
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {campaigns.map((campaign) => {
                const progress = campaign.budget > 0 ? (campaign.spent / campaign.budget) * 100 : 0;
                return (
                  <TableRow
                    key={campaign.id}
                    sx={{
                      '&:hover': { backgroundColor: '#F5EFF7' },
                      borderBottom: '1px solid #E8DEF8',
                    }}
                  >
                    <TableCell sx={{ fontWeight: 500 }}>{campaign.name}</TableCell>
                    <TableCell>
                      <Chip
                        label={campaign.type.charAt(0).toUpperCase() + campaign.type.slice(1)}
                        size="small"
                        sx={{
                          backgroundColor: typeColors[campaign.type]?.bg,
                          color: typeColors[campaign.type]?.text,
                          fontWeight: 600,
                        }}
                      />
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={campaign.status.charAt(0).toUpperCase() + campaign.status.slice(1)}
                        size="small"
                        sx={{
                          backgroundColor: statusColors[campaign.status]?.bg,
                          color: statusColors[campaign.status]?.text,
                          fontWeight: 600,
                        }}
                      />
                    </TableCell>
                    <TableCell>${campaign.budget.toLocaleString()}</TableCell>
                    <TableCell align="center" sx={{ minWidth: 150 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <LinearProgress
                          variant="determinate"
                          value={progress}
                          sx={{ flex: 1, height: 8, borderRadius: 2 }}
                        />
                        <Typography variant="caption" sx={{ minWidth: 35 }}>
                          {progress.toFixed(0)}%
                        </Typography>
                      </Box>
                    </TableCell>
                    <TableCell align="center">
                      <IconButton
                        size="small"
                        onClick={() => handleOpenDialog(campaign)}
                        sx={{ color: '#6750A4' }}
                      >
                        <EditIcon fontSize="small" />
                      </IconButton>
                      <IconButton
                        size="small"
                        onClick={() => handleDelete(campaign.id)}
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
          {editingId ? 'Edit Campaign' : 'Create Campaign'}
        </DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}
          <TextField
            fullWidth
            label="Campaign Name"
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            margin="normal"
            variant="outlined"
            required
          />
          <TextField
            fullWidth
            label="Campaign Type"
            select
            value={formData.type}
            onChange={(e) => setFormData({ ...formData, type: e.target.value })}
            margin="normal"
            variant="outlined"
            SelectProps={{ native: true }}
          >
            <option value="email">Email</option>
            <option value="social">Social Media</option>
            <option value="webinar">Webinar</option>
            <option value="content">Content Marketing</option>
            <option value="event">Event</option>
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
            <option value="planning">Planning</option>
            <option value="active">Active</option>
            <option value="paused">Paused</option>
            <option value="completed">Completed</option>
          </TextField>
          <TextField
            fullWidth
            label="Start Date"
            type="date"
            value={formData.startDate}
            onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
            margin="normal"
            variant="outlined"
            required
            InputLabelProps={{ shrink: true }}
          />
          <TextField
            fullWidth
            label="End Date"
            type="date"
            value={formData.endDate}
            onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
            margin="normal"
            variant="outlined"
            required
            InputLabelProps={{ shrink: true }}
          />
          <TextField
            fullWidth
            label="Budget"
            type="number"
            value={formData.budget}
            onChange={(e) => setFormData({ ...formData, budget: e.target.value })}
            margin="normal"
            variant="outlined"
            required
          />
          <TextField
            fullWidth
            label="Amount Spent"
            type="number"
            value={formData.spent}
            onChange={(e) => setFormData({ ...formData, spent: e.target.value })}
            margin="normal"
            variant="outlined"
          />
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

export default CampaignsPage;

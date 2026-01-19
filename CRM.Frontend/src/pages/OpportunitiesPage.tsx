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

interface Opportunity {
  id: string;
  name: string;
  company: string;
  value: number;
  stage: 'lead' | 'qualified' | 'proposal' | 'negotiation' | 'won' | 'lost';
  probability: number;
  closeDate: string;
}

function OpportunitiesPage() {
  const [opportunities, setOpportunities] = useState<Opportunity[]>([
    {
      id: '1',
      name: 'Enterprise Software Deal',
      company: 'Tech Corp',
      value: 50000,
      stage: 'proposal',
      probability: 70,
      closeDate: '2026-02-15',
    },
    {
      id: '2',
      name: 'Implementation Services',
      company: 'Global Industries',
      value: 25000,
      stage: 'negotiation',
      probability: 50,
      closeDate: '2026-03-01',
    },
  ]);

  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    company: '',
    value: '',
    stage: 'lead',
    probability: '',
    closeDate: '',
  });
  const [error, setError] = useState('');

  const stageColors: Record<string, { bg: string; text: string }> = {
    lead: { bg: '#E0E7FF', text: '#3949AB' },
    qualified: { bg: '#E1F5FE', text: '#0277BD' },
    proposal: { bg: '#F3E5F5', text: '#6A1B9A' },
    negotiation: { bg: '#FFF3E0', text: '#E65100' },
    won: { bg: '#E8F5E9', text: '#06A77D' },
    lost: { bg: '#FFEBEE', text: '#B3261E' },
  };

  const handleOpenDialog = (opportunity?: Opportunity) => {
    if (opportunity) {
      setEditingId(opportunity.id);
      setFormData({
        name: opportunity.name,
        company: opportunity.company,
        value: opportunity.value.toString(),
        stage: opportunity.stage,
        probability: opportunity.probability.toString(),
        closeDate: opportunity.closeDate,
      });
    } else {
      setEditingId(null);
      setFormData({
        name: '',
        company: '',
        value: '',
        stage: 'lead',
        probability: '',
        closeDate: '',
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setError('');
  };

  const handleSave = () => {
    if (!formData.name || !formData.company || !formData.value || !formData.closeDate) {
      setError('Please fill in all required fields');
      return;
    }

    if (editingId) {
      setOpportunities(
        opportunities.map((o) =>
          o.id === editingId
            ? {
                ...o,
                name: formData.name,
                company: formData.company,
                value: parseFloat(formData.value),
                stage: formData.stage as any,
                probability: parseInt(formData.probability),
                closeDate: formData.closeDate,
              }
            : o
        )
      );
    } else {
      const newOpp: Opportunity = {
        id: Date.now().toString(),
        name: formData.name,
        company: formData.company,
        value: parseFloat(formData.value),
        stage: formData.stage as any,
        probability: parseInt(formData.probability),
        closeDate: formData.closeDate,
      };
      setOpportunities([...opportunities, newOpp]);
    }
    handleCloseDialog();
  };

  const handleDelete = (id: string) => {
    if (window.confirm('Are you sure you want to delete this opportunity?')) {
      setOpportunities(opportunities.filter((o) => o.id !== id));
    }
  };

  return (
    <Box sx={{ py: 2 }}>
      <Box sx={{ mb: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Typography variant="h3" sx={{ fontWeight: 700, mb: 0.5 }}>
            Opportunities
          </Typography>
          <Typography color="textSecondary" variant="body2">
            Manage sales pipeline and opportunities
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenDialog()}
          sx={{ backgroundColor: '#6750A4', textTransform: 'none', borderRadius: 2 }}
        >
          Add Opportunity
        </Button>
      </Box>

      <Card sx={{ borderRadius: 3, boxShadow: 1 }}>
        <CardContent sx={{ p: 0 }}>
          <Table>
            <TableHead>
              <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Opportunity</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Company</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }} align="right">
                  Value
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Stage</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }} align="center">
                  Probability
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Close Date</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }} align="center">
                  Actions
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {opportunities.map((opp) => (
                <TableRow
                  key={opp.id}
                  sx={{
                    '&:hover': { backgroundColor: '#F5EFF7' },
                    borderBottom: '1px solid #E8DEF8',
                  }}
                >
                  <TableCell sx={{ fontWeight: 500 }}>{opp.name}</TableCell>
                  <TableCell>{opp.company}</TableCell>
                  <TableCell align="right" sx={{ fontWeight: 600, color: '#6750A4' }}>
                    ${opp.value.toLocaleString()}
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={opp.stage.charAt(0).toUpperCase() + opp.stage.slice(1)}
                      size="small"
                      sx={{
                        backgroundColor: stageColors[opp.stage]?.bg,
                        color: stageColors[opp.stage]?.text,
                        fontWeight: 600,
                      }}
                    />
                  </TableCell>
                  <TableCell align="center">{opp.probability}%</TableCell>
                  <TableCell>{opp.closeDate}</TableCell>
                  <TableCell align="center">
                    <IconButton
                      size="small"
                      onClick={() => handleOpenDialog(opp)}
                      sx={{ color: '#6750A4' }}
                    >
                      <EditIcon fontSize="small" />
                    </IconButton>
                    <IconButton
                      size="small"
                      onClick={() => handleDelete(opp.id)}
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
          {editingId ? 'Edit Opportunity' : 'Add Opportunity'}
        </DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}
          <TextField
            fullWidth
            label="Opportunity Name"
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            margin="normal"
            variant="outlined"
            required
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
            label="Opportunity Value"
            type="number"
            value={formData.value}
            onChange={(e) => setFormData({ ...formData, value: e.target.value })}
            margin="normal"
            variant="outlined"
            required
          />
          <TextField
            fullWidth
            label="Stage"
            select
            value={formData.stage}
            onChange={(e) => setFormData({ ...formData, stage: e.target.value })}
            margin="normal"
            variant="outlined"
            SelectProps={{ native: true }}
          >
            <option value="lead">Lead</option>
            <option value="qualified">Qualified</option>
            <option value="proposal">Proposal</option>
            <option value="negotiation">Negotiation</option>
            <option value="won">Won</option>
            <option value="lost">Lost</option>
          </TextField>
          <TextField
            fullWidth
            label="Win Probability (%)"
            type="number"
            value={formData.probability}
            onChange={(e) => setFormData({ ...formData, probability: e.target.value })}
            margin="normal"
            variant="outlined"
            inputProps={{ min: 0, max: 100 }}
          />
          <TextField
            fullWidth
            label="Expected Close Date"
            type="date"
            value={formData.closeDate}
            onChange={(e) => setFormData({ ...formData, closeDate: e.target.value })}
            margin="normal"
            variant="outlined"
            required
            InputLabelProps={{ shrink: true }}
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

export default OpportunitiesPage;

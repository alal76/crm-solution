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
  TableContainer,
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
  Chip,
  Slider,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';
import LookupSelect from '../components/LookupSelect';
import EntitySelect from '../components/EntitySelect';
import ImportExportButtons from '../components/ImportExportButtons';
import AdvancedSearch, { SearchField, SearchFilter, filterData } from '../components/AdvancedSearch';

// Search fields for Advanced Search
const SEARCH_FIELDS: SearchField[] = [
  { name: 'name', label: 'Title', type: 'text' },
  { name: 'stage', label: 'Stage', type: 'select', options: [
    { value: 0, label: 'Discovery' },
    { value: 1, label: 'Qualification' },
    { value: 2, label: 'Proposal' },
    { value: 3, label: 'Negotiation' },
    { value: 4, label: 'Closed Won' },
    { value: 5, label: 'Closed Lost' },
  ]},
  { name: 'solutionNotes', label: 'Solution Notes', type: 'text' },
  { name: 'amount', label: 'Amount', type: 'numberRange' },
  { name: 'region', label: 'Region', type: 'text' },
];

const SEARCHABLE_FIELDS = ['name', 'solutionNotes', 'region'];

interface Opportunity {
  id: number;
  name: string;
  stage: number;
  probability: number;
  amount: number;
  currency?: string;
  expectedCloseDate?: string;
  pricingModel?: number;
  termLengthMonths?: number;
  solutionNotes?: string;
  qualificationReason?: number;
  qualificationNotes?: string;
  region?: string;
  accountId: number;
  primaryContactId?: number;
  salesOwnerId?: number;
  leadId?: number;
  createdAt?: string;
  // Navigation properties from API
  accountName?: string;
  primaryContactName?: string;
  salesOwnerName?: string;
}

interface Account {
  id: number;
  firstName?: string;
  lastName?: string;
  company?: string;
  legalName?: string;
}

interface User {
  id: number;
  username: string;
  firstName: string;
  lastName: string;
}

interface OpportunityForm {
  name: string;
  stage: number;
  probability: number;
  amount: number;
  currency: string;
  expectedCloseDate: string;
  pricingModel: number;
  termLengthMonths: number;
  solutionNotes: string;
  qualificationReason: number | null;
  qualificationNotes: string;
  region: string;
  accountId: number;
  primaryContactId: number | null;
  salesOwnerId: number | null;
}

const STAGES = [
  { value: 0, label: 'Discovery', color: '#9e9e9e' },
  { value: 1, label: 'Qualification', color: '#2196f3' },
  { value: 2, label: 'Proposal', color: '#ff9800' },
  { value: 3, label: 'Negotiation', color: '#9c27b0' },
  { value: 4, label: 'Closed Won', color: '#4caf50' },
  { value: 5, label: 'Closed Lost', color: '#f44336' },
];

const PRICING_MODELS = [
  { value: 0, label: 'Subscription' },
  { value: 1, label: 'One-Time' },
  { value: 2, label: 'Usage-Based' },
  { value: 3, label: 'Hybrid' },
];

const QUALIFICATION_REASONS = [
  { value: 0, label: 'Budget' },
  { value: 1, label: 'Need' },
  { value: 2, label: 'Timing' },
  { value: 3, label: 'Authority' },
  { value: 4, label: 'Fit' },
];

function OpportunitiesPage() {
  const [opportunities, setOpportunities] = useState<Opportunity[]>([]);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<OpportunityForm>({
    name: '',
    stage: 0,
    probability: 10,
    amount: 0,
    currency: 'USD',
    expectedCloseDate: '',
    pricingModel: 0,
    termLengthMonths: 12,
    solutionNotes: '',
    qualificationReason: null,
    qualificationNotes: '',
    region: '',
    accountId: 0,
    primaryContactId: null,
    salesOwnerId: null,
  });
  const [searchFilters, setSearchFilters] = useState<SearchFilter[]>([]);
  const [searchText, setSearchText] = useState('');

  const handleSearch = (filters: SearchFilter[], text: string) => {
    setSearchFilters(filters);
    setSearchText(text);
  };

  const filteredOpportunities = filterData(opportunities, searchFilters, searchText, SEARCHABLE_FIELDS);

  useEffect(() => {
    fetchAllData();
  }, []);

  const fetchAllData = async () => {
    try {
      setLoading(true);
      const [oppRes, acctRes, userRes] = await Promise.all([
        apiClient.get('/opportunities'),
        apiClient.get('/accounts'),
        apiClient.get('/users').catch(() => ({ data: [] })),
      ]);
      setOpportunities(oppRes.data);
      setAccounts(acctRes.data);
      setUsers(userRes.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch data');
      console.error('Error fetching data:', err);
    } finally {
      setLoading(false);
    }
  };

  const getAccountName = (accountId: number) => {
    const account = accounts.find(a => a.id === accountId);
    if (!account) return 'Unknown';
    if (account.company) return account.company;
    if (account.firstName || account.lastName) return `${account.firstName || ''} ${account.lastName || ''}`.trim();
    return 'Unnamed Account';
  };

  const getUserName = (userId?: number) => {
    if (!userId) return '-';
    const user = users.find(u => u.id === userId);
    return user ? `${user.firstName} ${user.lastName}` : '-';
  };

  const getStageInfo = (stage: number) => {
    return STAGES.find(s => s.value === stage) || STAGES[0];
  };

  const handleOpenDialog = (opp?: Opportunity) => {
    if (opp) {
      setEditingId(opp.id);
      setFormData({
        name: opp.name,
        stage: opp.stage,
        probability: opp.probability,
        amount: opp.amount,
        currency: opp.currency || 'USD',
        expectedCloseDate: opp.expectedCloseDate?.split('T')[0] || '',
        pricingModel: opp.pricingModel ?? 0,
        termLengthMonths: opp.termLengthMonths ?? 12,
        solutionNotes: opp.solutionNotes || '',
        qualificationReason: opp.qualificationReason ?? null,
        qualificationNotes: opp.qualificationNotes || '',
        region: opp.region || '',
        accountId: opp.accountId,
        primaryContactId: opp.primaryContactId || null,
        salesOwnerId: opp.salesOwnerId || null,
      });
    } else {
      setEditingId(null);
      setFormData({
        name: '',
        stage: 0,
        probability: 10,
        amount: 0,
        currency: 'USD',
        expectedCloseDate: '',
        pricingModel: 0,
        termLengthMonths: 12,
        solutionNotes: '',
        qualificationReason: null,
        qualificationNotes: '',
        region: '',
        accountId: accounts[0]?.id || 0,
        primaryContactId: null,
        salesOwnerId: null,
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
      [name]: value === '' ? null : value,
    }));
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'amount' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleProbabilityChange = (_: Event, value: number | number[]) => {
    setFormData(prev => ({
      ...prev,
      probability: value as number,
    }));
  };

  const handleSaveOpportunity = async () => {
    if (!formData.name.trim() || !formData.accountId) {
      setError('Please fill in required fields (Name, Account)');
      return;
    }

    try {
      const payload = {
        ...formData,
        primaryContactId: formData.primaryContactId || undefined,
        salesOwnerId: formData.salesOwnerId || undefined,
        qualificationReason: formData.qualificationReason ?? undefined,
        expectedCloseDate: formData.expectedCloseDate || undefined,
      };

      if (editingId) {
        await apiClient.put(`/opportunities/${editingId}`, payload);
        setSuccessMessage('Opportunity updated successfully');
      } else {
        await apiClient.post('/opportunities', payload);
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
      <Container maxWidth="xl">
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ width: 40, height: 40, flexShrink: 0 }}><img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} /></Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>Opportunities</Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <ImportExportButtons entityType="opportunities" entityLabel="Opportunities" onImportComplete={fetchAllData} />
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
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccessMessage(null)}>{successMessage}</Alert>}

        <AdvancedSearch
          fields={SEARCH_FIELDS}
          onSearch={handleSearch}
          placeholder="Search opportunities by title, description..."
        />

        <Card>
          <CardContent sx={{ p: 0 }}>
            <TableContainer sx={{ overflowX: 'auto' }}>
              <Table sx={{ minWidth: 950 }}>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                  <TableCell><strong>Name</strong></TableCell>
                  <TableCell><strong>Account</strong></TableCell>
                  <TableCell><strong>Amount</strong></TableCell>
                  <TableCell><strong>Stage</strong></TableCell>
                  <TableCell><strong>Probability</strong></TableCell>
                  <TableCell><strong>Expected Close</strong></TableCell>
                  <TableCell><strong>Pricing Model</strong></TableCell>
                  <TableCell><strong>Sales Owner</strong></TableCell>
                  <TableCell align="center"><strong>Actions</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredOpportunities.map((opp) => {
                  const stageInfo = getStageInfo(opp.stage);
                  const pricingInfo = PRICING_MODELS.find(p => p.value === opp.pricingModel) || PRICING_MODELS[0];
                  return (
                    <TableRow key={opp.id}>
                      <TableCell>{opp.name}</TableCell>
                      <TableCell>{opp.accountName || getAccountName(opp.accountId)}</TableCell>
                      <TableCell>{opp.currency || 'USD'} {opp.amount?.toLocaleString() || 0}</TableCell>
                      <TableCell>
                        <Chip 
                          label={stageInfo.label} 
                          size="small"
                          sx={{ backgroundColor: stageInfo.color, color: 'white' }}
                        />
                      </TableCell>
                      <TableCell>{opp.probability}%</TableCell>
                      <TableCell>{opp.expectedCloseDate ? new Date(opp.expectedCloseDate).toLocaleDateString() : '-'}</TableCell>
                      <TableCell>{pricingInfo.label}</TableCell>
                      <TableCell>{opp.salesOwnerName || getUserName(opp.salesOwnerId)}</TableCell>
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
                  );
                })}
              </TableBody>
              </Table>
            </TableContainer>
            {filteredOpportunities.length === 0 && (
              <Typography sx={{ textAlign: 'center', py: 2, color: 'textSecondary' }}>
                No opportunities found
              </Typography>
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Add/Edit Opportunity Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle>{editingId ? 'Edit Opportunity' : 'Create New Opportunity'}</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 2 }}>
            <TextField
              autoFocus
              fullWidth
              label="Name"
              name="name"
              value={formData.name}
              onChange={handleInputChange}
              margin="normal"
              required
            />
            
            <Box sx={{ mt: 2 }}>
              <EntitySelect
                entityType="account"
                name="accountId"
                value={formData.accountId}
                onChange={handleSelectChange}
                label="Account"
                required
                showAddNew={true}
              />
            </Box>

            <TextField
              fullWidth
              label="Amount"
              name="amount"
              type="number"
              value={formData.amount}
              onChange={handleInputChange}
              margin="normal"
              inputProps={{ step: "0.01", min: 0 }}
            />

            <FormControl fullWidth margin="normal">
              <InputLabel>Currency</InputLabel>
              <Select
                name="currency"
                value={formData.currency}
                onChange={handleSelectChange}
                label="Currency"
              >
                <MenuItem value="USD">USD</MenuItem>
                <MenuItem value="EUR">EUR</MenuItem>
                <MenuItem value="GBP">GBP</MenuItem>
                <MenuItem value="INR">INR</MenuItem>
                <MenuItem value="CAD">CAD</MenuItem>
                <MenuItem value="AUD">AUD</MenuItem>
              </Select>
            </FormControl>

            <LookupSelect
              category="OpportunityStage"
              name="stage"
              value={formData.stage}
              onChange={handleSelectChange}
              label="Stage"
              fallback={STAGES.map(s => ({ value: s.value, label: s.label }))}
            />

            <FormControl fullWidth margin="normal">
              <InputLabel>Pricing Model</InputLabel>
              <Select
                name="pricingModel"
                value={formData.pricingModel}
                onChange={handleSelectChange}
                label="Pricing Model"
              >
                {PRICING_MODELS.map(pm => (
                  <MenuItem key={pm.value} value={pm.value}>{pm.label}</MenuItem>
                ))}
              </Select>
            </FormControl>

            <Box sx={{ mt: 2, mb: 1, gridColumn: { xs: '1', md: '1 / -1' } }}>
              <Typography gutterBottom>Probability: {formData.probability}%</Typography>
              <Slider
                value={formData.probability}
                onChange={handleProbabilityChange}
                min={0}
                max={100}
                step={5}
                marks={[
                  { value: 0, label: '0%' },
                  { value: 50, label: '50%' },
                  { value: 100, label: '100%' },
                ]}
                valueLabelDisplay="auto"
              />
            </Box>

            <TextField
              fullWidth
              label="Expected Close Date"
              name="expectedCloseDate"
              type="date"
              value={formData.expectedCloseDate}
              onChange={handleInputChange}
              margin="normal"
              InputLabelProps={{ shrink: true }}
            />

            <TextField
              fullWidth
              label="Term Length (Months)"
              name="termLengthMonths"
              type="number"
              value={formData.termLengthMonths}
              onChange={handleInputChange}
              margin="normal"
              inputProps={{ min: 1 }}
            />

            <TextField
              fullWidth
              label="Region"
              name="region"
              value={formData.region}
              onChange={handleInputChange}
              margin="normal"
            />

            <FormControl fullWidth margin="normal">
              <InputLabel>Qualification Reason</InputLabel>
              <Select
                name="qualificationReason"
                value={formData.qualificationReason ?? ''}
                onChange={handleSelectChange}
                label="Qualification Reason"
              >
                <MenuItem value="">None</MenuItem>
                {QUALIFICATION_REASONS.map(qr => (
                  <MenuItem key={qr.value} value={qr.value}>{qr.label}</MenuItem>
                ))}
              </Select>
            </FormControl>

            <Box sx={{ mt: 2 }}>
              <EntitySelect
                entityType="user"
                name="salesOwnerId"
                value={formData.salesOwnerId || ''}
                onChange={handleSelectChange}
                label="Sales Owner (Optional)"
                showAddNew={false}
              />
            </Box>

            <TextField
              fullWidth
              label="Solution Notes"
              name="solutionNotes"
              value={formData.solutionNotes}
              onChange={handleInputChange}
              margin="normal"
              multiline
              rows={2}
              sx={{ gridColumn: { xs: '1', md: '1 / -1' } }}
            />

            <TextField
              fullWidth
              label="Qualification Notes"
              name="qualificationNotes"
              value={formData.qualificationNotes}
              onChange={handleInputChange}
              margin="normal"
              multiline
              rows={2}
              sx={{ gridColumn: { xs: '1', md: '1 / -1' } }}
            />
          </Box>
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

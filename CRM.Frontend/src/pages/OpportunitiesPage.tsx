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
import ImportExportButtons from '../components/ImportExportButtons';
import AdvancedSearch, { SearchField, SearchFilter, filterData } from '../components/AdvancedSearch';

// Search fields for Advanced Search
const SEARCH_FIELDS: SearchField[] = [
  { name: 'name', label: 'Title', type: 'text' },
  { name: 'stage', label: 'Stage', type: 'select', options: [
    { value: 0, label: 'Prospecting' },
    { value: 1, label: 'Qualification' },
    { value: 2, label: 'Proposal' },
    { value: 3, label: 'Negotiation' },
    { value: 4, label: 'Closed Won' },
    { value: 5, label: 'Closed Lost' },
  ]},
  { name: 'description', label: 'Description', type: 'text' },
  { name: 'amount', label: 'Amount', type: 'numberRange' },
];

const SEARCHABLE_FIELDS = ['name', 'description'];

interface Opportunity {
  id: number;
  name: string;
  description: string;
  amount: number;
  stage: number;
  closeDate: string;
  probability: number;
  customerId: number;
  assignedToUserId?: number;
  productId?: number;
  customer?: { id: number; firstName: string; lastName: string };
  product?: { id: number; name: string };
  createdAt: string;
}

interface Customer {
  id: number;
  firstName: string;
  lastName: string;
  company: string;
}

interface Product {
  id: number;
  name: string;
  price: number;
}

interface User {
  id: number;
  username: string;
  firstName: string;
  lastName: string;
}

interface OpportunityForm {
  name: string;
  description: string;
  amount: number;
  stage: number;
  closeDate: string;
  probability: number;
  customerId: number;
  assignedToUserId: number | null;
  productId: number | null;
}

const STAGES = [
  { value: 0, label: 'Prospecting', color: '#9e9e9e' },
  { value: 1, label: 'Qualification', color: '#2196f3' },
  { value: 2, label: 'Proposal', color: '#ff9800' },
  { value: 3, label: 'Negotiation', color: '#9c27b0' },
  { value: 4, label: 'Closed Won', color: '#4caf50' },
  { value: 5, label: 'Closed Lost', color: '#f44336' },
];

function OpportunitiesPage() {
  const [opportunities, setOpportunities] = useState<Opportunity[]>([]);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<OpportunityForm>({
    name: '',
    description: '',
    amount: 0,
    stage: 0,
    closeDate: '',
    probability: 10,
    customerId: 0,
    assignedToUserId: null,
    productId: null,
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
      const [oppRes, custRes, prodRes, userRes] = await Promise.all([
        apiClient.get('/opportunities'),
        apiClient.get('/customers'),
        apiClient.get('/products'),
        apiClient.get('/users').catch(() => ({ data: [] })),
      ]);
      setOpportunities(oppRes.data);
      setCustomers(custRes.data);
      setProducts(prodRes.data);
      setUsers(userRes.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch data');
      console.error('Error fetching data:', err);
    } finally {
      setLoading(false);
    }
  };

  const getCustomerName = (customerId: number) => {
    const customer = customers.find(c => c.id === customerId);
    return customer ? `${customer.firstName} ${customer.lastName}` : 'Unknown';
  };

  const getProductName = (productId?: number) => {
    if (!productId) return '-';
    const product = products.find(p => p.id === productId);
    return product ? product.name : '-';
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
        description: opp.description || '',
        amount: opp.amount,
        stage: opp.stage,
        closeDate: opp.closeDate?.split('T')[0] || '',
        probability: opp.probability,
        customerId: opp.customerId,
        assignedToUserId: opp.assignedToUserId || null,
        productId: opp.productId || null,
      });
    } else {
      setEditingId(null);
      setFormData({
        name: '',
        description: '',
        amount: 0,
        stage: 0,
        closeDate: '',
        probability: 10,
        customerId: customers[0]?.id || 0,
        assignedToUserId: null,
        productId: null,
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
    if (!formData.name.trim() || !formData.customerId || !formData.closeDate) {
      setError('Please fill in required fields (Name, Customer, Close Date)');
      return;
    }

    try {
      const payload = {
        ...formData,
        assignedToUserId: formData.assignedToUserId || undefined,
        productId: formData.productId || undefined,
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
          <CardContent sx={{ overflowX: 'auto' }}>
            <Table>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                  <TableCell><strong>Name</strong></TableCell>
                  <TableCell><strong>Customer</strong></TableCell>
                  <TableCell><strong>Amount</strong></TableCell>
                  <TableCell><strong>Stage</strong></TableCell>
                  <TableCell><strong>Probability</strong></TableCell>
                  <TableCell><strong>Close Date</strong></TableCell>
                  <TableCell><strong>Product</strong></TableCell>
                  <TableCell><strong>Assigned To</strong></TableCell>
                  <TableCell align="center"><strong>Actions</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredOpportunities.map((opp) => {
                  const stageInfo = getStageInfo(opp.stage);
                  return (
                    <TableRow key={opp.id}>
                      <TableCell>{opp.name}</TableCell>
                      <TableCell>{getCustomerName(opp.customerId)}</TableCell>
                      <TableCell>${opp.amount?.toLocaleString() || 0}</TableCell>
                      <TableCell>
                        <Chip 
                          label={stageInfo.label} 
                          size="small"
                          sx={{ backgroundColor: stageInfo.color, color: 'white' }}
                        />
                      </TableCell>
                      <TableCell>{opp.probability}%</TableCell>
                      <TableCell>{opp.closeDate ? new Date(opp.closeDate).toLocaleDateString() : '-'}</TableCell>
                      <TableCell>{getProductName(opp.productId)}</TableCell>
                      <TableCell>{getUserName(opp.assignedToUserId)}</TableCell>
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
            {filteredOpportunities.length === 0 && (
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
            label="Name"
            name="name"
            value={formData.name}
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
                <MenuItem key={cust.id} value={cust.id}>
                  {cust.firstName} {cust.lastName} {cust.company ? `(${cust.company})` : ''}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

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

          <LookupSelect
            category="OpportunityStage"
            name="stage"
            value={formData.stage}
            onChange={handleSelectChange}
            label="Stage"
            fallback={STAGES.map(s => ({ value: s.value, label: s.label }))}
          />

          <Box sx={{ mt: 2, mb: 1 }}>
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
            label="Close Date"
            name="closeDate"
            type="date"
            value={formData.closeDate}
            onChange={handleInputChange}
            margin="normal"
            InputLabelProps={{ shrink: true }}
            required
          />

          <FormControl fullWidth margin="normal">
            <InputLabel>Product (Optional)</InputLabel>
            <Select
              name="productId"
              value={formData.productId || ''}
              onChange={handleSelectChange}
              label="Product (Optional)"
            >
              <MenuItem value="">
                <em>None</em>
              </MenuItem>
              {products.map(prod => (
                <MenuItem key={prod.id} value={prod.id}>
                  {prod.name} (${prod.price?.toLocaleString()})
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <FormControl fullWidth margin="normal">
            <InputLabel>Assigned To (Optional)</InputLabel>
            <Select
              name="assignedToUserId"
              value={formData.assignedToUserId || ''}
              onChange={handleSelectChange}
              label="Assigned To (Optional)"
            >
              <MenuItem value="">
                <em>Unassigned</em>
              </MenuItem>
              {users.map(user => (
                <MenuItem key={user.id} value={user.id}>
                  {user.firstName} {user.lastName} ({user.username})
                </MenuItem>
              ))}
            </Select>
          </FormControl>

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

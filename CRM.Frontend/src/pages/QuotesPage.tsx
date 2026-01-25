import { useState, useEffect } from 'react';
import {
  Box, Card, CardContent, Typography, Button, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Dialog, DialogTitle, DialogContent, DialogActions, Alert, CircularProgress,
  TextField, Container, FormControl, InputLabel, Select, MenuItem, Chip, Grid,
  IconButton, Tooltip, Tabs, Tab, SelectChangeEvent, Divider
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, 
  Description as QuoteIcon, Send as SendIcon, CheckCircle as AcceptIcon,
  Cancel as RejectIcon, Refresh as ReviseIcon
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import { TabPanel } from '../components/common';
import logo from '../assets/logo.png';
import LookupSelect from '../components/LookupSelect';
import EntitySelect from '../components/EntitySelect';
import ImportExportButtons from '../components/ImportExportButtons';
import AdvancedSearch, { SearchField, SearchFilter, filterData } from '../components/AdvancedSearch';

// Search fields for Advanced Search
const SEARCH_FIELDS: SearchField[] = [
  { name: 'quoteNumber', label: 'Quote Number', type: 'text' },
  { name: 'title', label: 'Title', type: 'text' },
  { name: 'status', label: 'Status', type: 'select', options: [
    { value: 0, label: 'Draft' },
    { value: 1, label: 'Pending' },
    { value: 2, label: 'Sent' },
    { value: 3, label: 'Viewed' },
    { value: 4, label: 'Accepted' },
    { value: 5, label: 'Rejected' },
    { value: 6, label: 'Expired' },
    { value: 7, label: 'Revised' },
  ]},
  { name: 'totalAmount', label: 'Total Amount', type: 'numberRange' },
];

const SEARCHABLE_FIELDS = ['quoteNumber', 'title', 'description', 'notes'];

// Enums matching backend
const QUOTE_STATUSES = [
  { value: 0, label: 'Draft', color: '#9e9e9e' },
  { value: 1, label: 'Pending', color: '#2196f3' },
  { value: 2, label: 'Sent', color: '#9c27b0' },
  { value: 3, label: 'Viewed', color: '#ff9800' },
  { value: 4, label: 'Accepted', color: '#4caf50' },
  { value: 5, label: 'Rejected', color: '#f44336' },
  { value: 6, label: 'Expired', color: '#607d8b' },
  { value: 7, label: 'Revised', color: '#00bcd4' },
];

interface Quote {
  id: number;
  quoteNumber: string;
  title: string;
  description?: string;
  customerId?: number;
  customer?: { firstName: string; lastName: string; company?: string };
  opportunityId?: number;
  status: number;
  subtotal: number;
  taxAmount: number;
  discountAmount: number;
  shippingAmount: number;
  totalAmount: number;
  discountPercent: number;
  taxPercent: number;
  validUntil?: string;
  sentDate?: string;
  viewedDate?: string;
  acceptedDate?: string;
  rejectedDate?: string;
  revisionNumber: number;
  termsAndConditions?: string;
  notes?: string;
  billingAddress?: string;
  shippingAddress?: string;
  createdAt: string;
}

interface QuoteForm {
  title: string;
  description: string;
  customerId: number | '';
  opportunityId: number | '';
  status: number;
  subtotal: number;
  taxPercent: number;
  discountPercent: number;
  shippingAmount: number;
  validUntil: string;
  termsAndConditions: string;
  notes: string;
  billingAddress: string;
  shippingAddress: string;
}

interface Customer {
  id: number;
  firstName: string;
  lastName: string;
  company?: string;
}

function QuotesPage() {
  const [quotes, setQuotes] = useState<Quote[]>([]);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  const [searchFilters, setSearchFilters] = useState<SearchFilter[]>([]);
  const [searchText, setSearchText] = useState('');

  const handleSearch = (filters: SearchFilter[], text: string) => {
    setSearchFilters(filters);
    setSearchText(text);
  };

  const filteredQuotes = filterData(quotes, searchFilters, searchText, SEARCHABLE_FIELDS);

  const emptyForm: QuoteForm = {
    title: '', description: '', customerId: '', opportunityId: '', status: 0,
    subtotal: 0, taxPercent: 0, discountPercent: 0, shippingAmount: 0,
    validUntil: '', termsAndConditions: '', notes: '', billingAddress: '', shippingAddress: '',
  };
  const [formData, setFormData] = useState<QuoteForm>(emptyForm);

  useEffect(() => {
    fetchQuotes();
    fetchCustomers();
  }, []);

  const fetchQuotes = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/quotes');
      setQuotes(response.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch quotes');
    } finally {
      setLoading(false);
    }
  };

  const fetchCustomers = async () => {
    try {
      const response = await apiClient.get('/customers');
      setCustomers(response.data);
    } catch (err) {
      console.error('Error fetching customers:', err);
    }
  };

  const handleOpenDialog = (quote?: Quote) => {
    setDialogTab(0);
    if (quote) {
      setEditingId(quote.id);
      setFormData({
        title: quote.title, description: quote.description || '',
        customerId: quote.customerId || '', opportunityId: quote.opportunityId || '',
        status: quote.status, subtotal: quote.subtotal, taxPercent: quote.taxPercent,
        discountPercent: quote.discountPercent, shippingAmount: quote.shippingAmount,
        validUntil: quote.validUntil?.split('T')[0] || '',
        termsAndConditions: quote.termsAndConditions || '', notes: quote.notes || '',
        billingAddress: quote.billingAddress || '', shippingAddress: quote.shippingAddress || '',
      });
    } else {
      setEditingId(null);
      setFormData(emptyForm);
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => { setOpenDialog(false); setEditingId(null); };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'number' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleSelectChange = (e: SelectChangeEvent<string | number>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const calculateTotals = () => {
    const discount = formData.subtotal * (formData.discountPercent / 100);
    const taxable = formData.subtotal - discount;
    const tax = taxable * (formData.taxPercent / 100);
    const total = taxable + tax + formData.shippingAmount;
    return { discount, tax, total };
  };

  const handleSaveQuote = async () => {
    if (!formData.title.trim()) {
      setError('Please enter a quote title');
      return;
    }
    try {
      const totals = calculateTotals();
      const payload = {
        ...formData,
        customerId: formData.customerId || null,
        opportunityId: formData.opportunityId || null,
        discountAmount: totals.discount,
        taxAmount: totals.tax,
        totalAmount: totals.total,
      };
      if (editingId) {
        await apiClient.put(`/quotes/${editingId}`, payload);
        setSuccessMessage('Quote updated successfully');
      } else {
        await apiClient.post('/quotes', payload);
        setSuccessMessage('Quote created successfully');
      }
      handleCloseDialog();
      fetchQuotes();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save quote');
    }
  };

  const handleSendQuote = async (id: number) => {
    try {
      await apiClient.post(`/quotes/${id}/send`);
      setSuccessMessage('Quote sent successfully');
      fetchQuotes();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to send quote');
    }
  };

  const handleAcceptQuote = async (id: number) => {
    try {
      await apiClient.post(`/quotes/${id}/accept`);
      setSuccessMessage('Quote accepted');
      fetchQuotes();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to accept quote');
    }
  };

  const handleRejectQuote = async (id: number) => {
    try {
      await apiClient.post(`/quotes/${id}/reject`);
      setSuccessMessage('Quote rejected');
      fetchQuotes();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to reject quote');
    }
  };

  const handleReviseQuote = async (id: number) => {
    try {
      await apiClient.post(`/quotes/${id}/revise`);
      setSuccessMessage('Quote revised - new revision created');
      fetchQuotes();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to revise quote');
    }
  };

  const handleDeleteQuote = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this quote?')) {
      try {
        await apiClient.delete(`/quotes/${id}`);
        setSuccessMessage('Quote deleted successfully');
        fetchQuotes();
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete quote');
      }
    }
  };

  const getStatus = (value: number) => QUOTE_STATUSES.find(s => s.value === value);

  const isExpired = (quote: Quote) => {
    if (!quote.validUntil || quote.status === 4 || quote.status === 5) return false;
    return new Date(quote.validUntil) < new Date();
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
        <CircularProgress />
      </Box>
    );
  }

  const totals = calculateTotals();

  return (
    <Box sx={{ py: 4 }}>
      <Container maxWidth="xl">
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
              <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
            </Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>Quotes</Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <ImportExportButtons entityType="quotes" entityLabel="Quotes" onImportComplete={fetchQuotes} />
            <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenDialog()} sx={{ backgroundColor: '#6750A4' }}>
              Create Quote
            </Button>
          </Box>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

        <AdvancedSearch
          fields={SEARCH_FIELDS}
          onSearch={handleSearch}
          placeholder="Search quotes by number, title..."
        />

        <Card>
          <CardContent sx={{ p: 0 }}>
            <TableContainer sx={{ overflowX: 'auto' }}>
              <Table sx={{ minWidth: 850 }}>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                  <TableCell><strong>Quote #</strong></TableCell>
                  <TableCell><strong>Title</strong></TableCell>
                  <TableCell><strong>Customer</strong></TableCell>
                  <TableCell><strong>Status</strong></TableCell>
                  <TableCell><strong>Amount</strong></TableCell>
                  <TableCell><strong>Valid Until</strong></TableCell>
                  <TableCell><strong>Revision</strong></TableCell>
                  <TableCell align="center"><strong>Actions</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredQuotes.map((quote) => {
                  const status = getStatus(quote.status);
                  const expired = isExpired(quote);

                  return (
                    <TableRow key={quote.id} hover sx={{ backgroundColor: expired ? '#fff3e0' : 'inherit' }}>
                      <TableCell>
                        <Typography fontFamily="monospace" fontWeight={500}>{quote.quoteNumber}</Typography>
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <QuoteIcon sx={{ color: '#6750A4' }} />
                          <Box>
                            <Typography fontWeight={500}>{quote.title}</Typography>
                            {quote.description && (
                              <Typography variant="caption" color="textSecondary" sx={{ display: 'block', maxWidth: 200, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                                {quote.description}
                              </Typography>
                            )}
                          </Box>
                        </Box>
                      </TableCell>
                      <TableCell>
                        {quote.customer ? (
                          <Box>
                            <Typography variant="body2">{quote.customer.firstName} {quote.customer.lastName}</Typography>
                            {quote.customer.company && (
                              <Typography variant="caption" color="textSecondary">{quote.customer.company}</Typography>
                            )}
                          </Box>
                        ) : '—'}
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
                          <Chip label={status?.label || 'Unknown'} size="small" sx={{ backgroundColor: status?.color, color: 'white' }} />
                          {expired && quote.status !== 6 && (
                            <Chip label="Expired" size="small" sx={{ backgroundColor: '#607d8b', color: 'white' }} />
                          )}
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Box>
                          <Typography fontWeight={500}>${quote.totalAmount?.toLocaleString(undefined, { minimumFractionDigits: 2 })}</Typography>
                          {quote.discountAmount > 0 && (
                            <Typography variant="caption" color="success.main">-${quote.discountAmount.toFixed(2)} discount</Typography>
                          )}
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" sx={{ color: expired ? '#f44336' : 'inherit' }}>
                          {quote.validUntil ? new Date(quote.validUntil).toLocaleDateString() : '—'}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip label={`v${quote.revisionNumber}`} size="small" variant="outlined" />
                      </TableCell>
                      <TableCell align="center">
                        {quote.status === 0 && (
                          <Tooltip title="Send Quote">
                            <IconButton size="small" onClick={() => handleSendQuote(quote.id)} sx={{ color: '#2196f3' }}>
                              <SendIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        )}
                        {(quote.status === 2 || quote.status === 3) && (
                          <>
                            <Tooltip title="Accept">
                              <IconButton size="small" onClick={() => handleAcceptQuote(quote.id)} sx={{ color: '#4caf50' }}>
                                <AcceptIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Reject">
                              <IconButton size="small" onClick={() => handleRejectQuote(quote.id)} sx={{ color: '#f44336' }}>
                                <RejectIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                          </>
                        )}
                        {quote.status !== 4 && quote.status !== 0 && (
                          <Tooltip title="Create Revision">
                            <IconButton size="small" onClick={() => handleReviseQuote(quote.id)} sx={{ color: '#00bcd4' }}>
                              <ReviseIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        )}
                        <Tooltip title="Edit">
                          <IconButton size="small" onClick={() => handleOpenDialog(quote)} sx={{ color: '#6750A4' }}>
                            <EditIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton size="small" onClick={() => handleDeleteQuote(quote.id)} sx={{ color: '#f44336' }}>
                            <DeleteIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
              </Table>
            </TableContainer>
            {quotes.length === 0 && (
              <Typography sx={{ textAlign: 'center', py: 4, color: 'textSecondary' }}>
                No quotes found. Create your first quote to get started.
              </Typography>
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Add/Edit Quote Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle sx={{ pb: 0 }}>{editingId ? 'Edit Quote' : 'Create Quote'}</DialogTitle>
        <Box sx={{ borderBottom: 1, borderColor: 'divider', px: 3 }}>
          <Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)}>
            <Tab label="Details" />
            <Tab label="Pricing" />
            <Tab label="Addresses" />
            <Tab label="Terms" />
          </Tabs>
        </Box>
        <DialogContent sx={{ pt: 2, minHeight: 400 }}>
          <TabPanel value={dialogTab} index={0}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField fullWidth label="Quote Title" name="title" value={formData.title} onChange={handleInputChange} required />
              </Grid>
              <Grid item xs={6}>
                <EntitySelect
                  entityType="customer"
                  name="customerId"
                  value={formData.customerId}
                  onChange={handleSelectChange}
                  label="Customer"
                  showAddNew={true}
                />
              </Grid>
              <Grid item xs={6}>
                <LookupSelect
                  category="QuoteStatus"
                  name="status"
                  value={formData.status}
                  onChange={handleSelectChange}
                  label="Status"
                  fallback={QUOTE_STATUSES.map(s => ({ value: s.value, label: s.label }))}
                />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Valid Until" name="validUntil" type="date" value={formData.validUntil} onChange={handleInputChange} InputLabelProps={{ shrink: true }} />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Description" name="description" value={formData.description} onChange={handleInputChange} multiline rows={2} />
              </Grid>
            </Grid>
          </TabPanel>

          <TabPanel value={dialogTab} index={1}>
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <TextField fullWidth label="Subtotal ($)" name="subtotal" type="number" value={formData.subtotal} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Discount (%)" name="discountPercent" type="number" value={formData.discountPercent} onChange={handleInputChange} inputProps={{ min: 0, max: 100 }} />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Tax (%)" name="taxPercent" type="number" value={formData.taxPercent} onChange={handleInputChange} inputProps={{ min: 0 }} />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Shipping ($)" name="shippingAmount" type="number" value={formData.shippingAmount} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={12}>
                <Divider sx={{ my: 2 }} />
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                  <Typography>Subtotal:</Typography>
                  <Typography>${formData.subtotal.toFixed(2)}</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1, color: 'success.main' }}>
                  <Typography>Discount ({formData.discountPercent}%):</Typography>
                  <Typography>-${totals.discount.toFixed(2)}</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                  <Typography>Tax ({formData.taxPercent}%):</Typography>
                  <Typography>${totals.tax.toFixed(2)}</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                  <Typography>Shipping:</Typography>
                  <Typography>${formData.shippingAmount.toFixed(2)}</Typography>
                </Box>
                <Divider sx={{ my: 1 }} />
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="h6">Total:</Typography>
                  <Typography variant="h6" fontWeight={700}>${totals.total.toFixed(2)}</Typography>
                </Box>
              </Grid>
            </Grid>
          </TabPanel>

          <TabPanel value={dialogTab} index={2}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField fullWidth label="Billing Address" name="billingAddress" value={formData.billingAddress} onChange={handleInputChange} multiline rows={3} />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Shipping Address" name="shippingAddress" value={formData.shippingAddress} onChange={handleInputChange} multiline rows={3} />
              </Grid>
            </Grid>
          </TabPanel>

          <TabPanel value={dialogTab} index={3}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField fullWidth label="Terms and Conditions" name="termsAndConditions" value={formData.termsAndConditions} onChange={handleInputChange} multiline rows={6} />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Notes" name="notes" value={formData.notes} onChange={handleInputChange} multiline rows={3} />
              </Grid>
            </Grid>
          </TabPanel>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSaveQuote} variant="contained" sx={{ backgroundColor: '#6750A4' }}>
            {editingId ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default QuotesPage;

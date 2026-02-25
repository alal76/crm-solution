import { useState, useEffect, useMemo } from 'react';
import {
  Box, Card, CardContent, Typography, Button, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, TablePagination, Dialog, DialogContent, DialogActions, Alert, CircularProgress,
  Container, Chip,
  IconButton, Tooltip, Divider
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, 
  Description as QuoteIcon, Send as SendIcon, CheckCircle as AcceptIcon,
  Cancel as RejectIcon, Refresh as ReviseIcon, Note as NoteIcon,
  Print as PrintIcon, Link as LinkIcon
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import logger from '../services/logger';
import { DialogError, DialogSuccess, ActionButton, DialogHeader, RelatedEntitiesPanel, EnhancedEmptyState } from '../components/common';
import { useApiState } from '../hooks/useApiState';
import { usePagination } from '../hooks/usePagination';
import { BaseEntity } from '../types';
import logo from '../assets/logo.png';
import ImportExportButtons from '../components/ImportExportButtons';
import NotesTab from '../components/NotesTab';
import QuoteLineItemsEditor from '../components/QuoteLineItemsEditor';
import DynamicEntityForm, { ExtraTab } from '../components/DynamicEntityForm';
import AdvancedSearch, { SearchField, SearchFilter, filterData } from '../components/AdvancedSearch';
import { generatePDF, formatCurrency, formatDate } from '../services/pdfExportService';

// Search fields for Advanced Search
const SEARCH_FIELDS: SearchField[] = [
  { name: 'quoteNumber', label: 'Quote Number', type: 'text' },
  { name: 'title', label: 'Title', type: 'text' },
  { name: 'status', label: 'Status', type: 'select', options: [
    { value: 0, label: 'New' },
    { value: 1, label: 'Draft' },
    { value: 2, label: 'Under Approval' },
    { value: 3, label: 'Approved' },
    { value: 4, label: 'Shared' },
    { value: 5, label: 'Viewed' },
    { value: 6, label: 'Accepted' },
    { value: 7, label: 'Rejected' },
    { value: 8, label: 'Expired' },
    { value: 9, label: 'Revised' },
    { value: 10, label: 'Cancelled' },
    { value: 11, label: 'Converted' },
  ]},
  { name: 'totalAmount', label: 'Total Amount', type: 'numberRange' },
];

const SEARCHABLE_FIELDS = ['quoteNumber', 'title', 'description', 'notes'];

// Enums matching backend QuoteStatus
const QUOTE_STATUSES = [
  { value: 0, label: 'New', color: '#e0e0e0' },
  { value: 1, label: 'Draft', color: '#9e9e9e' },
  { value: 2, label: 'Under Approval', color: '#ff9800' },
  { value: 3, label: 'Approved', color: '#8bc34a' },
  { value: 4, label: 'Shared', color: '#2196f3' },
  { value: 5, label: 'Viewed', color: '#9c27b0' },
  { value: 6, label: 'Accepted', color: '#4caf50' },
  { value: 7, label: 'Rejected', color: '#f44336' },
  { value: 8, label: 'Expired', color: '#607d8b' },
  { value: 9, label: 'Revised', color: '#00bcd4' },
  { value: 10, label: 'Cancelled', color: '#795548' },
  { value: 11, label: 'Converted', color: '#009688' },
  { value: 12, label: 'End of Life', color: '#424242' },
];

interface Quote extends BaseEntity {
  quoteNumber: string;
  name: string;
  description?: string;
  accountId?: number;
  account?: { firstName: string; lastName: string; company?: string };
  opportunityId?: number;
  status: number;
  subtotal: number;
  taxTotal: number;
  discountTotal: number;
  shippingCost: number;
  grandTotal: number;
  discountPercent: number;
  taxRate: number;
  expirationDate?: string;
  sentDate?: string;
  viewedDate?: string;
  acceptedDate?: string;
  rejectedDate?: string;
  version: number;
  termsAndConditions?: string;
  notes?: string;
  billingAddress?: string;
  shippingAddress?: string;
}

interface QuoteForm {
  name: string;
  description: string;
  accountId: number | '';
  opportunityId: number | '';
  status: number;
  subtotal: number;
  taxRate: number;
  discountPercent: number;
  shippingCost: number;
  expirationDate: string;
  termsAndConditions: string;
  notes: string;
  billingAddress: string;
  shippingAddress: string;
  // Billing address group
  billingName: string;
  billingCity: string;
  billingState: string;
  billingZipCode: string;
  billingCountry: string;
  // Shipping address group
  shippingName: string;
  shippingCity: string;
  shippingState: string;
  shippingZipCode: string;
  shippingCountry: string;
  // Approval group
  requiresApproval: boolean;
  isApproved: boolean;
  approvedByUserId: number | '';
  approvalDate: string;
  approvalNotes: string;
  submittedForApprovalDate: string;
  // Signature group
  isSigned: boolean;
  signedDate: string;
  signedBy: string;
  signatureUrl: string;
  // Contact/terms group
  contactEmail: string;
  contactPhone: string;
  paymentTerms: string;
  deliveryTerms: string;
  warrantyMonths: number | '';
  internalNotes: string;
}

interface Customer {
  id: number;
  firstName: string;
  lastName: string;
  company?: string;
}

function QuotesPage() {
  const [quotes, setQuotes] = useState<Quote[]>([]);
  const [accounts, setAccounts] = useState<Customer[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [dialogError, setDialogError] = useState<string | null>(null);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  const [searchFilters, setSearchFilters] = useState<SearchFilter[]>([]);
  const [searchText, setSearchText] = useState('');

  const handleSearch = (filters: SearchFilter[], text: string) => {
    setSearchFilters(filters);
    setSearchText(text);
  };

  const dialogApi = useApiState();

  const filteredQuotes = useMemo(
    () => filterData(quotes, searchFilters, searchText, SEARCHABLE_FIELDS),
    [quotes, searchFilters, searchText]
  );

  const { paginatedData: paginatedQuotes, page, pageSize, handlePageChange, handlePageSizeChange, pageSizeOptions } = usePagination(filteredQuotes, { defaultPageSize: 25 });

  const emptyForm: QuoteForm = {
    name: '', description: '', accountId: '', opportunityId: '', status: 0,
    subtotal: 0, taxRate: 0, discountPercent: 0, shippingCost: 0,
    expirationDate: '', termsAndConditions: '', notes: '', billingAddress: '', shippingAddress: '',
    // Billing address group
    billingName: '', billingCity: '', billingState: '', billingZipCode: '', billingCountry: '',
    // Shipping address group
    shippingName: '', shippingCity: '', shippingState: '', shippingZipCode: '', shippingCountry: '',
    // Approval group
    requiresApproval: false, isApproved: false, approvedByUserId: '', approvalDate: '',
    approvalNotes: '', submittedForApprovalDate: '',
    // Signature group
    isSigned: false, signedDate: '', signedBy: '', signatureUrl: '',
    // Contact/terms group
    contactEmail: '', contactPhone: '', paymentTerms: '', deliveryTerms: '',
    warrantyMonths: '', internalNotes: '',
  };
  const [formData, setFormData] = useState<QuoteForm>(emptyForm);

  useEffect(() => {
    fetchQuotes();
    fetchAccounts();
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

  const fetchAccounts = async () => {
    try {
      const response = await apiClient.get('/accounts');
      setAccounts(response.data);
    } catch (err) {
      console.error('Error fetching accounts:', err);
    }
  };

  const handleOpenDialog = (quote?: Quote) => {
    setDialogTab(0);
    if (quote) {
      setEditingId(quote.id);
      setFormData({
        name: quote.name || '', description: quote.description || '',
        accountId: quote.accountId || '', opportunityId: quote.opportunityId || '',
        status: quote.status, subtotal: quote.subtotal || 0, taxRate: quote.taxRate || 0,
        discountPercent: quote.discountPercent || 0, shippingCost: quote.shippingCost || 0,
        expirationDate: quote.expirationDate?.split('T')[0] || '',
        termsAndConditions: quote.termsAndConditions || '', notes: quote.notes || '',
        billingAddress: quote.billingAddress || '', shippingAddress: quote.shippingAddress || '',
        // Billing address group
        billingName: '', billingCity: '', billingState: '', billingZipCode: '', billingCountry: '',
        // Shipping address group
        shippingName: '', shippingCity: '', shippingState: '', shippingZipCode: '', shippingCountry: '',
        // Approval group
        requiresApproval: false, isApproved: false,
        approvedByUserId: '', approvalDate: '', approvalNotes: '', submittedForApprovalDate: '',
        // Signature group
        isSigned: false, signedDate: '', signedBy: '', signatureUrl: '',
        // Contact/terms group
        contactEmail: '', contactPhone: '', paymentTerms: '', deliveryTerms: '',
        warrantyMonths: '', internalNotes: '',
      });
    } else {
      setEditingId(null);
      setFormData(emptyForm);
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => { setOpenDialog(false); setEditingId(null); setDialogError(null); dialogApi.reset(); };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'number' ? parseFloat(value) || 0 : value,
    }));
  };

  const calculateTotals = () => {
    const discount = (formData.subtotal || 0) * ((formData.discountPercent || 0) / 100);
    const taxable = (formData.subtotal || 0) - discount;
    const tax = taxable * ((formData.taxRate || 0) / 100);
    const total = taxable + tax + (formData.shippingCost || 0);
    return { discount, tax, total };
  };

  const handleSaveQuote = async () => {
    if (!formData.name.trim()) {
      setDialogError('Please enter a quote title');
      return;
    }
    const totals = calculateTotals();
    const payload = {
      ...formData,
      accountId: formData.accountId || null,
      opportunityId: formData.opportunityId || null,
      discountTotal: totals.discount,
      taxTotal: totals.tax,
      grandTotal: totals.total,
    };
    await dialogApi.execute(async () => {
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
    });
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

  const handlePrintQuote = async (quote: Quote) => {
    // Fetch line items for the quote
    let lineItems: any[] = [];
    try {
      const response = await apiClient.get(`/quotes/${quote.id}/lineitems`);
      lineItems = response.data;
    } catch {
      // Continue without line items
    }

    const status = getStatus(quote.status);
    const accountName = quote.account 
      ? `${quote.account.firstName} ${quote.account.lastName}${quote.account.company ? ` (${quote.account.company})` : ''}`
      : 'N/A';

    generatePDF(
      {
        title: 'Quote',
        subtitle: `${quote.quoteNumber} - ${quote.name}`,
        headerColor: '#6750A4',
        includeDate: true,
      },
      [
        {
          title: 'Quote Details',
          fields: [
            { label: 'Quote Number', value: quote.quoteNumber },
            { label: 'Title', value: quote.name },
            { label: 'Status', value: status?.label || 'Unknown' },
            { label: 'Account', value: accountName },
            { label: 'Valid Until', value: formatDate(quote.expirationDate) },
            { label: 'Revision', value: `v${quote.version}` },
          ],
        },
        lineItems.length > 0 ? {
          title: 'Line Items',
          table: {
            columns: [
              { header: 'Product', field: 'productName' },
              { header: 'Description', field: 'description' },
              { header: 'Qty', field: 'quantity', align: 'right' as const },
              { header: 'Unit Price', field: 'unitPrice', align: 'right' as const, format: formatCurrency },
              { header: 'Discount', field: 'discountPercent', align: 'right' as const, format: (v: number) => `${v || 0}%` },
              { header: 'Total', field: 'lineTotal', align: 'right' as const, format: formatCurrency },
            ],
            data: lineItems,
          },
        } : { content: '' },
        {
          title: 'Totals',
          fields: [
            { label: 'Subtotal', value: formatCurrency(quote.subtotal) },
            { label: 'Discount', value: `${formatCurrency(quote.discountTotal)} (${quote.discountPercent || 0}%)` },
            { label: 'Tax', value: `${formatCurrency(quote.taxTotal)} (${quote.taxRate || 0}%)` },
            { label: 'Shipping', value: formatCurrency(quote.shippingCost) },
            { label: 'Total', value: formatCurrency(quote.grandTotal) },
          ],
        },
        quote.termsAndConditions ? {
          title: 'Terms & Conditions',
          content: quote.termsAndConditions,
        } : { content: '' },
        quote.notes ? {
          title: 'Notes',
          content: quote.notes,
        } : { content: '' },
      ].filter(s => s.title || s.content || s.fields || s.table)
    );
  };

  const getStatus = (value: number) => QUOTE_STATUSES.find(s => s.value === value);

  const isExpired = (quote: Quote) => {
    if (!quote.expirationDate || quote.status === 4 || quote.status === 5) return false;
    return new Date(quote.expirationDate) < new Date();
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
                  <TableCell><strong>Account</strong></TableCell>
                  <TableCell><strong>Status</strong></TableCell>
                  <TableCell><strong>Amount</strong></TableCell>
                  <TableCell><strong>Valid Until</strong></TableCell>
                  <TableCell><strong>Revision</strong></TableCell>
                  <TableCell align="center"><strong>Actions</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {paginatedQuotes.map((quote) => {
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
                            <Typography fontWeight={500}>{quote.name}</Typography>
                            {quote.description && (
                              <Typography variant="caption" color="textSecondary" sx={{ display: 'block', maxWidth: 200, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                                {quote.description}
                              </Typography>
                            )}
                          </Box>
                        </Box>
                      </TableCell>
                      <TableCell>
                        {quote.account ? (
                          <Box>
                            <Typography variant="body2">{quote.account.firstName} {quote.account.lastName}</Typography>
                            {quote.account.company && (
                              <Typography variant="caption" color="textSecondary">{quote.account.company}</Typography>
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
                          <Typography fontWeight={500}>${(quote.grandTotal || 0).toLocaleString(undefined, { minimumFractionDigits: 2 })}</Typography>
                          {(quote.discountTotal || 0) > 0 && (
                            <Typography variant="caption" color="success.main">-${(quote.discountTotal || 0).toFixed(2)} discount</Typography>
                          )}
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" sx={{ color: expired ? '#f44336' : 'inherit' }}>
                          {quote.expirationDate ? new Date(quote.expirationDate).toLocaleDateString() : '—'}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip label={`v${quote.version || 1}`} size="small" variant="outlined" />
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
                        <Tooltip title="Print / PDF">
                          <IconButton size="small" onClick={() => handlePrintQuote(quote)} sx={{ color: '#795548' }}>
                            <PrintIcon fontSize="small" />
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
              <TablePagination
                component="div"
                count={filteredQuotes.length}
                page={page}
                onPageChange={handlePageChange}
                rowsPerPage={pageSize}
                onRowsPerPageChange={handlePageSizeChange}
                rowsPerPageOptions={pageSizeOptions}
              />
            </TableContainer>
            {quotes.length === 0 && (
              <EnhancedEmptyState
                illustration="quotes"
                title={searchFilters.length > 0 ? "No quotes match your filters" : "No quotes yet"}
                description={searchFilters.length > 0 
                  ? "Try adjusting your filters to find what you're looking for"
                  : "Create your first quote to start generating proposals for accounts"
                }
                variant={searchFilters.length > 0 ? "no-results" : "no-data"}
                primaryActionLabel="Create Quote"
                onPrimaryAction={() => handleOpenDialog()}
                secondaryActionLabel={searchFilters.length > 0 ? "Clear Filters" : undefined}
                onSecondaryAction={searchFilters.length > 0 ? () => setSearchFilters([]) : undefined}
              />
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Add/Edit Quote Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogHeader
          mode={editingId ? 'edit' : 'create'}
          entityType="quote"
          entityName={editingId ? formData.name || undefined : undefined}
          entityId={editingId || undefined}
          onClose={handleCloseDialog}
          status={editingId && formData.status !== undefined ? 
            (QUOTE_STATUSES.find(s => s.value === formData.status)?.label || undefined) : undefined}
          statusColor={editingId && formData.status !== undefined ? (
            formData.status === 6 ? 'success' :
            formData.status === 3 ? 'info' :
            formData.status === 2 ? 'warning' :
            formData.status === 7 ? 'error' :
            formData.status === 8 ? 'error' : 'default'
          ) : undefined}
        />
        <DialogContent dividers>
          <DialogError error={dialogError} onClose={() => setDialogError(null)} />

          <DynamicEntityForm
            moduleName="Quotes"
            formData={formData}
            onChange={handleInputChange}
            onSelectChange={(e: any) => setFormData((prev: any) => ({ ...prev, [e.target.name]: e.target.value }))}
            setFormData={setFormData}
            activeTab={dialogTab}
            editingId={editingId}
            onTabChange={setDialogTab}
            excludeFields={['tags', 'customFields']}
            extraTabs={[
              {
                index: 100,
                name: 'Line Items',
                icon: <QuoteIcon fontSize="small" />,
                editOnly: true,
                render: () => editingId ? (
                  <QuoteLineItemsEditor
                    quoteId={editingId}
                    onTotalsChange={(t) => {
                      setFormData(prev => ({
                        ...prev,
                        subtotal: t.subtotal,
                        discountPercent: t.discount > 0 && t.subtotal > 0 ? (t.discount / t.subtotal) * 100 : 0,
                        taxRate: t.tax > 0 && (t.subtotal - t.discount) > 0 ? (t.tax / (t.subtotal - t.discount)) * 100 : 0,
                      }));
                    }}
                  />
                ) : (
                  <Alert severity="info">
                    Please save the quote first to add line items.
                  </Alert>
                ),
              },
              {
                index: 101,
                name: 'Pricing',
                render: () => {
                  const t = calculateTotals();
                  return (
                    <Box>
                      <Divider sx={{ my: 2 }} />
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                        <Typography>Subtotal:</Typography>
                        <Typography>${(formData.subtotal || 0).toFixed(2)}</Typography>
                      </Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1, color: 'success.main' }}>
                        <Typography>Discount ({formData.discountPercent || 0}%):</Typography>
                        <Typography>-${(t.discount || 0).toFixed(2)}</Typography>
                      </Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                        <Typography>Tax ({formData.taxRate || 0}%):</Typography>
                        <Typography>${(t.tax || 0).toFixed(2)}</Typography>
                      </Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                        <Typography>Shipping:</Typography>
                        <Typography>${(formData.shippingCost || 0).toFixed(2)}</Typography>
                      </Box>
                      <Divider sx={{ my: 1 }} />
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant="h6">Total:</Typography>
                        <Typography variant="h6" fontWeight={700}>${(t.total || 0).toFixed(2)}</Typography>
                      </Box>
                    </Box>
                  );
                },
              },
              {
                index: 102,
                name: 'Related',
                icon: <LinkIcon fontSize="small" />,
                editOnly: true,
                render: () => (
                  <RelatedEntitiesPanel
                    entityType="quotes"
                    entityId={editingId!}
                    showRelated={['accounts', 'contacts', 'opportunities']}
                    onEntityClick={(type, id) => {
                      handleCloseDialog();
                      logger.debug(`Navigate to ${type} ${id}`);
                    }}
                  />
                ),
              },
              {
                index: 103,
                name: 'Notes',
                icon: <NoteIcon fontSize="small" />,
                render: () => editingId ? (
                  <NotesTab entityType="Quote" entityId={editingId} entityName={formData.name || 'Quote'} />
                ) : (
                  <Alert severity="info" sx={{ mt: 2 }}>Please save the quote first to add notes.</Alert>
                ),
              },
            ]}
          />
        </DialogContent>
        <DialogActions>
          <DialogError error={dialogApi.error} />
          <DialogSuccess message={dialogApi.success} />
          <Button onClick={handleCloseDialog} disabled={dialogApi.loading}>Cancel</Button>
          <ActionButton onClick={handleSaveQuote} loading={dialogApi.loading} variant="contained" sx={{ backgroundColor: '#6750A4' }}>
            {editingId ? 'Update' : 'Create'}
          </ActionButton>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default QuotesPage;

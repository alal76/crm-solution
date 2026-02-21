import React, { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Typography,
  Button,
  Card,
  CardContent,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  Chip,
  IconButton,
  Tooltip,
  Dialog,
  DialogContent,
  DialogActions,
  TextField,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Stack,
  Alert,
  CircularProgress,
  Tabs,
  Tab,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Switch,
  FormControlLabel,
  Divider,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import type { SelectChangeEvent } from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Refresh as RefreshIcon,
  Send as SendIcon,
  CheckCircle as CheckCircleIcon,
  Warning as WarningIcon,
} from '@mui/icons-material';
import { DialogError } from '../components/common/DialogError';
import ActionButton from '../components/common/ActionButton';
import { DialogHeader } from '../components/common/DialogHeader';
import TabPanel from '../components/common/TabPanel';
import { EnhancedEmptyState } from '../components/common/EnhancedEmptyState';
import EntitySelect from '../components/EntitySelect';
import { useApiState } from '../hooks/useApiState';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';

// ==================== ENUMS ====================

enum InvoiceStatus {
  Draft = 0, PendingApproval = 1, Approved = 2, Sent = 3, Viewed = 4,
  PartiallyPaid = 5, Paid = 6, Overdue = 7, Disputed = 8, Voided = 9,
  WrittenOff = 10, Collections = 11, Refunded = 12,
}

enum PaymentMethod {
  CreditCard = 0, DebitCard = 1, BankTransfer = 2, ACH = 3, Wire = 4, Check = 5,
  Cash = 6, PayPal = 7, Stripe = 8, ApplePay = 9, GooglePay = 10, Crypto = 11,
  StoreCredit = 12, GiftCard = 13, FinancingPlan = 14, PurchaseOrder = 15, Other = 16,
}

// ==================== INTERFACES ====================

interface Invoice {
  id: number;
  invoiceNumber: string;
  accountId: number;
  accountName?: string;
  orderId?: number;
  quoteId?: number;
  status: InvoiceStatus;
  invoiceDate: string;
  dueDate: string;
  subtotal: number;
  taxAmount: number;
  discountAmount: number;
  totalAmount: number;
  amountPaid: number;
  balanceDue: number;
  notes?: string;
  createdAt: string;
  updatedAt?: string;
}

interface InvoiceLineItem {
  id: number;
  invoiceId: number;
  productId?: number;
  productName?: string;
  description: string;
  quantity: number;
  unitPrice: number;
  discount: number;
  taxRate: number;
  totalPrice: number;
}

interface InvoiceForm {
  customerId: number | null;
  status: InvoiceStatus;
  issueDate: string;
  dueDate: string;
  notes: string;
  orderId: number | null;
  quoteId: number | null;
  paymentTerms: string;
  discountPercent: number;
  taxRate: number;
  internalNotes: string;
  billingName: string;
  billingCompany: string;
  billingStreet: string;
  billingCity: string;
  billingState: string;
  billingPostalCode: string;
  billingCountry: string;
  billingEmail: string;
  billingPhone: string;
  earlyPaymentDiscountPercent: number;
  earlyPaymentDiscountDays: number;
  lateFeePercent: number;
  // Classification
  description: string;
  invoiceType: string;
  // Service Period
  servicePeriodStart: string;
  servicePeriodEnd: string;
  // Financials
  subtotal: number;
  discountAmount: number;
  taxAmount: number;
  shippingAmount: number;
  currencyCode: string;
  earlyPaymentDiscountAmount: number;
  lateFeeAmount: number;
  // Collections
  inCollections: boolean;
  collectionReference: string;
  // Relations
  contactId: number | null;
  originalInvoiceId: number | null;
  // Admin
  footer: string;
  termsAndConditions: string;
  voidReason: string;
}

// ==================== CONSTANTS ====================

type ChipColor = 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning';

const INVOICE_STATUS_OPTIONS: Array<{ value: InvoiceStatus; label: string; color: ChipColor }> = [
  { value: InvoiceStatus.Draft, label: 'Draft', color: 'default' },
  { value: InvoiceStatus.PendingApproval, label: 'Pending Approval', color: 'warning' },
  { value: InvoiceStatus.Approved, label: 'Approved', color: 'info' },
  { value: InvoiceStatus.Sent, label: 'Sent', color: 'info' },
  { value: InvoiceStatus.Viewed, label: 'Viewed', color: 'info' },
  { value: InvoiceStatus.PartiallyPaid, label: 'Partially Paid', color: 'warning' },
  { value: InvoiceStatus.Paid, label: 'Paid', color: 'success' },
  { value: InvoiceStatus.Overdue, label: 'Overdue', color: 'error' },
  { value: InvoiceStatus.Disputed, label: 'Disputed', color: 'error' },
  { value: InvoiceStatus.Voided, label: 'Voided', color: 'default' },
  { value: InvoiceStatus.WrittenOff, label: 'Written Off', color: 'default' },
  { value: InvoiceStatus.Collections, label: 'Collections', color: 'warning' },
  { value: InvoiceStatus.Refunded, label: 'Refunded', color: 'secondary' },
];

// ==================== HELPER FUNCTIONS ====================

const getStatusInfo = (status: InvoiceStatus): { label: string; color: ChipColor } =>
  INVOICE_STATUS_OPTIONS.find(s => s.value === status) || { label: 'Unknown', color: 'default' };

const formatCurrency = (value: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);

const formatDate = (dateString: string) =>
  dateString ? new Date(dateString).toLocaleDateString() : '-';

const isOverdue = (dueDate: string, status: InvoiceStatus): boolean => {
  if (status === InvoiceStatus.Paid || status === InvoiceStatus.Voided || status === InvoiceStatus.Refunded) return false;
  return new Date(dueDate) < new Date();
};

// ==================== MAIN COMPONENT ====================

function InvoicesPage() {
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  const [filterStatus, setFilterStatus] = useState<InvoiceStatus | 'all'>('all');
  const [lineItems, setLineItems] = useState<InvoiceLineItem[]>([]);

  const emptyForm: InvoiceForm = {
    customerId: null,
    status: InvoiceStatus.Draft,
    issueDate: new Date().toISOString().split('T')[0],
    dueDate: '',
    notes: '',
    orderId: null,
    quoteId: null,
    paymentTerms: '',
    discountPercent: 0,
    taxRate: 0,
    internalNotes: '',
    billingName: '',
    billingCompany: '',
    billingStreet: '',
    billingCity: '',
    billingState: '',
    billingPostalCode: '',
    billingCountry: '',
    billingEmail: '',
    billingPhone: '',
    earlyPaymentDiscountPercent: 0,
    earlyPaymentDiscountDays: 0,
    lateFeePercent: 0,
    // Classification
    description: '',
    invoiceType: '',
    // Service Period
    servicePeriodStart: '',
    servicePeriodEnd: '',
    // Financials
    subtotal: 0,
    discountAmount: 0,
    taxAmount: 0,
    shippingAmount: 0,
    currencyCode: 'USD',
    earlyPaymentDiscountAmount: 0,
    lateFeeAmount: 0,
    // Collections
    inCollections: false,
    collectionReference: '',
    // Relations
    contactId: null,
    originalInvoiceId: null,
    // Admin
    footer: '',
    termsAndConditions: '',
    voidReason: '',
  };
  const [formData, setFormData] = useState<InvoiceForm>(emptyForm);

  const dialogApi = useApiState();

  // ==================== DATA FETCHING ====================

  useEffect(() => {
    fetchInvoices();
  }, []);

  const fetchInvoices = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/invoices');
      setInvoices(response.data);
      setError(null);
    } catch (err: any) {
      if (err.response?.status === 404) {
        setInvoices([]);
        setError(null);
      } else {
        setError(err.response?.data?.message || 'Failed to fetch invoices');
      }
    } finally {
      setLoading(false);
    }
  };

  const fetchLineItems = async (invoiceId: number) => {
    try {
      const response = await apiClient.get(`/invoices/${invoiceId}/line-items`);
      setLineItems(response.data);
    } catch {
      setLineItems([]);
    }
  };

  // ==================== DIALOG HANDLERS ====================

  const handleOpenDialog = (invoice?: Invoice) => {
    setDialogTab(0);
    if (invoice) {
      setEditingId(invoice.id);
      setFormData({
        customerId: invoice.accountId,
        status: invoice.status,
        issueDate: invoice.invoiceDate?.split('T')[0] || '',
        dueDate: invoice.dueDate?.split('T')[0] || '',
        notes: invoice.notes || '',
        orderId: invoice.orderId || null,
        quoteId: invoice.quoteId || null,
        paymentTerms: (invoice as any).paymentTerms || '',
        discountPercent: (invoice as any).discountPercent || 0,
        taxRate: (invoice as any).taxRate || 0,
        internalNotes: (invoice as any).internalNotes || '',
        billingName: (invoice as any).billingName || '',
        billingCompany: (invoice as any).billingCompany || '',
        billingStreet: (invoice as any).billingStreet || '',
        billingCity: (invoice as any).billingCity || '',
        billingState: (invoice as any).billingState || '',
        billingPostalCode: (invoice as any).billingPostalCode || '',
        billingCountry: (invoice as any).billingCountry || '',
        billingEmail: (invoice as any).billingEmail || '',
        billingPhone: (invoice as any).billingPhone || '',
        earlyPaymentDiscountPercent: (invoice as any).earlyPaymentDiscountPercent || 0,
        earlyPaymentDiscountDays: (invoice as any).earlyPaymentDiscountDays || 0,
        lateFeePercent: (invoice as any).lateFeePercent || 0,
        // Classification
        description: (invoice as any).description || '',
        invoiceType: (invoice as any).invoiceType || '',
        // Service Period
        servicePeriodStart: (invoice as any).servicePeriodStart?.split('T')[0] || '',
        servicePeriodEnd: (invoice as any).servicePeriodEnd?.split('T')[0] || '',
        // Financials
        subtotal: invoice.subtotal || 0,
        discountAmount: invoice.discountAmount || 0,
        taxAmount: invoice.taxAmount || 0,
        shippingAmount: (invoice as any).shippingAmount || 0,
        currencyCode: (invoice as any).currencyCode || 'USD',
        earlyPaymentDiscountAmount: (invoice as any).earlyPaymentDiscountAmount || 0,
        lateFeeAmount: (invoice as any).lateFeeAmount || 0,
        // Collections
        inCollections: (invoice as any).inCollections || false,
        collectionReference: (invoice as any).collectionReference || '',
        // Relations
        contactId: (invoice as any).contactId || null,
        originalInvoiceId: (invoice as any).originalInvoiceId || null,
        // Admin
        footer: (invoice as any).footer || '',
        termsAndConditions: (invoice as any).termsAndConditions || '',
        voidReason: (invoice as any).voidReason || '',
      });
      fetchLineItems(invoice.id);
    } else {
      setEditingId(null);
      setFormData(emptyForm);
      setLineItems([]);
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
    dialogApi.clearError();
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    const checked = (e.target as HTMLInputElement).checked;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : type === 'number' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleSelectChange = (e: SelectChangeEvent<number | string>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name as string]: value }));
  };

  // ==================== SAVE OPERATIONS ====================

  const handleSaveInvoice = async () => {
    if (!formData.customerId) {
      dialogApi.setError('Account is required');
      return;
    }

    await dialogApi.execute(async () => {
      if (editingId) {
        await apiClient.put(`/invoices/${editingId}`, formData);
        setSuccessMessage('Invoice updated successfully');
      } else {
        await apiClient.post('/invoices', formData);
        setSuccessMessage('Invoice created successfully');
      }
      handleCloseDialog();
      fetchInvoices();
      setTimeout(() => setSuccessMessage(null), 3000);
    });
  };

  const handleDeleteInvoice = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this invoice?')) {
      try {
        await apiClient.delete(`/invoices/${id}`);
        setSuccessMessage('Invoice deleted successfully');
        fetchInvoices();
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete invoice');
      }
    }
  };

  // ==================== INVOICE ACTIONS ====================

  const handleSendInvoice = async (id: number) => {
    try {
      await apiClient.post(`/invoices/${id}/send`);
      setSuccessMessage('Invoice sent successfully');
      fetchInvoices();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to send invoice');
    }
  };

  const handleMarkAsPaid = async (id: number) => {
    try {
      await apiClient.post(`/invoices/${id}/mark-paid`);
      setSuccessMessage('Invoice marked as paid');
      fetchInvoices();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to mark invoice as paid');
    }
  };

  const handleApproveInvoice = async (id: number) => {
    try {
      await apiClient.post(`/invoices/${id}/approve`);
      setSuccessMessage('Invoice approved');
      fetchInvoices();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to approve invoice');
    }
  };

  // ==================== FILTERING ====================

  const filteredInvoices = filterStatus === 'all'
    ? invoices
    : invoices.filter(i => i.status === filterStatus);

  // ==================== RENDER ====================

  if (loading) {
    return (
      <Container maxWidth="lg">
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="50vh">
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg">
      <Box mb={4}>
        {/* Header */}
        <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
          <Box display="flex" alignItems="center" gap={2}>
            <img src={logo} alt="CRM Logo" style={{ height: 40, borderRadius: 8 }} />
            <Typography variant="h4">Invoices</Typography>
          </Box>
          <Stack direction="row" spacing={2}>
            <FormControl size="small" sx={{ minWidth: 150 }}>
              <InputLabel>Status Filter</InputLabel>
              <Select
                value={filterStatus}
                onChange={(e) => setFilterStatus(e.target.value as InvoiceStatus | 'all')}
                label="Status Filter"
              >
                <MenuItem value="all">All</MenuItem>
                {INVOICE_STATUS_OPTIONS.map(opt => (
                  <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                ))}
              </Select>
            </FormControl>
            <Button variant="outlined" startIcon={<RefreshIcon />} onClick={fetchInvoices}>
              Refresh
            </Button>
            <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenDialog()}>
              New Invoice
            </Button>
          </Stack>
        </Box>

        {/* Alerts */}
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

        {/* Overdue Alert */}
        {invoices.filter(i => isOverdue(i.dueDate, i.status)).length > 0 && (
          <Alert severity="warning" icon={<WarningIcon />} sx={{ mb: 2 }}>
            {invoices.filter(i => isOverdue(i.dueDate, i.status)).length} invoice(s) are overdue
          </Alert>
        )}

        {/* Invoices Table */}
        <Card>
          <CardContent>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Invoice #</TableCell>
                  <TableCell>Account</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Issue Date</TableCell>
                  <TableCell>Due Date</TableCell>
                  <TableCell align="right">Total</TableCell>
                  <TableCell align="right">Balance Due</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredInvoices.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={8} sx={{ border: 0 }}>
                      <EnhancedEmptyState
                        illustration="generic"
                        title="No invoices yet"
                        description="Create your first invoice to start billing customers"
                        variant="no-data"
                        primaryActionLabel="Create Invoice"
                        onPrimaryAction={() => handleOpenDialog()}
                      />
                    </TableCell>
                  </TableRow>
                ) : (
                  filteredInvoices.map((invoice) => {
                    const statusInfo = getStatusInfo(invoice.status);
                    const overdue = isOverdue(invoice.dueDate, invoice.status);

                    return (
                      <TableRow key={invoice.id} hover>
                        <TableCell>
                          <Typography fontFamily="monospace">{invoice.invoiceNumber}</Typography>
                        </TableCell>
                        <TableCell>{invoice.accountName || '-'}</TableCell>
                        <TableCell>
                          <Chip label={statusInfo.label} size="small" color={statusInfo.color} />
                          {overdue && invoice.status !== InvoiceStatus.Overdue && (
                            <Chip label="Overdue" size="small" color="error" sx={{ ml: 0.5 }} />
                          )}
                        </TableCell>
                        <TableCell>{formatDate(invoice.invoiceDate)}</TableCell>
                        <TableCell>
                          <Typography color={overdue ? 'error.main' : 'text.primary'}>
                            {formatDate(invoice.dueDate)}
                          </Typography>
                        </TableCell>
                        <TableCell align="right">{formatCurrency(invoice.totalAmount)}</TableCell>
                        <TableCell align="right">
                          <Typography color={invoice.balanceDue > 0 ? 'error.main' : 'success.main'} fontWeight="medium">
                            {formatCurrency(invoice.balanceDue)}
                          </Typography>
                        </TableCell>
                        <TableCell align="right">
                          <Tooltip title="Edit">
                            <IconButton size="small" onClick={() => handleOpenDialog(invoice)}>
                              <EditIcon />
                            </IconButton>
                          </Tooltip>
                          {invoice.status === InvoiceStatus.Draft && (
                            <Tooltip title="Send">
                              <IconButton size="small" color="primary" onClick={() => handleSendInvoice(invoice.id)}>
                                <SendIcon />
                              </IconButton>
                            </Tooltip>
                          )}
                          {invoice.status !== InvoiceStatus.Paid && invoice.status !== InvoiceStatus.Voided && (
                            <Tooltip title="Mark as Paid">
                              <IconButton size="small" color="success" onClick={() => handleMarkAsPaid(invoice.id)}>
                                <CheckCircleIcon />
                              </IconButton>
                            </Tooltip>
                          )}
                          <Tooltip title="Delete">
                            <IconButton size="small" color="error" onClick={() => handleDeleteInvoice(invoice.id)}>
                              <DeleteIcon />
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    );
                  })
                )}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </Box>

      {/* Invoice Editor Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="lg" fullWidth>
        <DialogHeader
          mode={editingId ? 'edit' : 'create'}
          entityType="invoice"
          entityName={editingId ? `Invoice` : undefined}
          entityId={editingId || undefined}
          onClose={handleCloseDialog}
          status={editingId ? getStatusInfo(formData.status).label : undefined}
          statusColor={editingId ? (
            formData.status === InvoiceStatus.Paid ? 'success' :
            formData.status === InvoiceStatus.Overdue ? 'error' :
            formData.status === InvoiceStatus.Sent ? 'info' :
            formData.status === InvoiceStatus.Draft ? 'default' : 'warning'
          ) : undefined}
        />
        <DialogContent dividers>
          <Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)} sx={{ mb: 2 }}>
            <Tab label="Invoice Details" />
            {editingId && <Tab label="Line Items" />}
          </Tabs>

          <DialogError error={dialogApi.error} />

          {/* Tab 0: Invoice Details */}
          <TabPanel value={dialogTab} index={0}>
            <Grid container spacing={3}>
              <Grid item xs={12} md={6}>
                <EntitySelect
                  name="customerId"
                  label="Account *"
                  entityType="account"
                  value={formData.customerId ?? ''}
                  onChange={(val) => setFormData(prev => ({ ...prev, customerId: val as number | null }))}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth>
                  <InputLabel>Status</InputLabel>
                  <Select
                    name="status"
                    value={formData.status}
                    onChange={handleSelectChange}
                    label="Status"
                  >
                    {INVOICE_STATUS_OPTIONS.map(opt => (
                      <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  type="date"
                  label="Issue Date"
                  name="issueDate"
                  value={formData.issueDate}
                  onChange={handleInputChange}
                  InputLabelProps={{ shrink: true }}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  type="date"
                  label="Due Date"
                  name="dueDate"
                  value={formData.dueDate}
                  onChange={handleInputChange}
                  InputLabelProps={{ shrink: true }}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  multiline
                  rows={3}
                  label="Notes"
                  name="notes"
                  value={formData.notes}
                  onChange={handleInputChange}
                />
              </Grid>
            </Grid>
            {/* Accordion for Additional Information */}
            <Accordion sx={{ mt: 3 }}>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography variant="subtitle1" fontWeight={600}>Additional Information</Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Grid container spacing={2}>
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      type="number"
                      label="Discount %"
                      name="discountPercent"
                      value={formData.discountPercent}
                      onChange={handleInputChange}
                      inputProps={{ min: 0, max: 100, step: 0.01 }}
                    />
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      type="number"
                      label="Tax Rate %"
                      name="taxRate"
                      value={formData.taxRate}
                      onChange={handleInputChange}
                      inputProps={{ min: 0, step: 0.01 }}
                    />
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      label="Payment Terms"
                      name="paymentTerms"
                      value={formData.paymentTerms}
                      onChange={handleInputChange}
                      placeholder="e.g. Net 30"
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      multiline
                      rows={3}
                      label="Internal Notes"
                      name="internalNotes"
                      value={formData.internalNotes}
                      onChange={handleInputChange}
                      placeholder="Internal notes (not visible to customer)"
                    />
                  </Grid>
                </Grid>
              </AccordionDetails>
            </Accordion>

            {/* Accordion for Classification & Service Period */}
            <Accordion sx={{ mt: 2 }}>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography variant="subtitle1" fontWeight={600}>Classification &amp; Service Period</Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <Typography variant="subtitle2" color="text.secondary" gutterBottom>Classification</Typography>
                    <Divider sx={{ mb: 2 }} />
                  </Grid>
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      multiline
                      rows={3}
                      label="Description"
                      name="description"
                      value={formData.description}
                      onChange={handleInputChange}
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <FormControl fullWidth>
                      <InputLabel>Invoice Type</InputLabel>
                      <Select
                        name="invoiceType"
                        value={formData.invoiceType}
                        onChange={handleSelectChange}
                        label="Invoice Type"
                      >
                        <MenuItem value="">None</MenuItem>
                        <MenuItem value="standard">Standard</MenuItem>
                        <MenuItem value="proforma">Proforma</MenuItem>
                        <MenuItem value="credit">Credit Note</MenuItem>
                        <MenuItem value="debit">Debit Note</MenuItem>
                        <MenuItem value="recurring">Recurring</MenuItem>
                      </Select>
                    </FormControl>
                  </Grid>
                  <Grid item xs={12}>
                    <Typography variant="subtitle2" color="text.secondary" sx={{ mt: 1 }} gutterBottom>Service Period</Typography>
                    <Divider sx={{ mb: 2 }} />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      type="date"
                      label="Service Period Start"
                      name="servicePeriodStart"
                      value={formData.servicePeriodStart}
                      onChange={handleInputChange}
                      InputLabelProps={{ shrink: true }}
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      type="date"
                      label="Service Period End"
                      name="servicePeriodEnd"
                      value={formData.servicePeriodEnd}
                      onChange={handleInputChange}
                      InputLabelProps={{ shrink: true }}
                    />
                  </Grid>
                </Grid>
              </AccordionDetails>
            </Accordion>

            {/* Accordion for Financial Details */}
            <Accordion sx={{ mt: 2 }}>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography variant="subtitle1" fontWeight={600}>Financial Details</Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <Typography variant="subtitle2" color="text.secondary" gutterBottom>Financials</Typography>
                    <Divider sx={{ mb: 2 }} />
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      type="number"
                      label="Subtotal"
                      name="subtotal"
                      value={formData.subtotal}
                      onChange={handleInputChange}
                      InputProps={{ readOnly: true }}
                      helperText="Calculated automatically"
                    />
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      type="number"
                      label="Discount Amount"
                      name="discountAmount"
                      value={formData.discountAmount}
                      onChange={handleInputChange}
                      inputProps={{ min: 0, step: 0.01 }}
                    />
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      type="number"
                      label="Tax Amount"
                      name="taxAmount"
                      value={formData.taxAmount}
                      onChange={handleInputChange}
                      inputProps={{ min: 0, step: 0.01 }}
                    />
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      type="number"
                      label="Tax Rate %"
                      name="taxRate"
                      value={formData.taxRate}
                      onChange={handleInputChange}
                      inputProps={{ min: 0, step: 0.01 }}
                    />
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      type="number"
                      label="Shipping Amount"
                      name="shippingAmount"
                      value={formData.shippingAmount}
                      onChange={handleInputChange}
                      inputProps={{ min: 0, step: 0.01 }}
                    />
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      label="Currency Code"
                      name="currencyCode"
                      value={formData.currencyCode}
                      onChange={handleInputChange}
                      inputProps={{ maxLength: 3 }}
                      placeholder="USD"
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      type="number"
                      label="Early Payment Discount Amount"
                      name="earlyPaymentDiscountAmount"
                      value={formData.earlyPaymentDiscountAmount}
                      onChange={handleInputChange}
                      InputProps={{ readOnly: true }}
                      helperText="Calculated automatically"
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      type="number"
                      label="Late Fee Amount"
                      name="lateFeeAmount"
                      value={formData.lateFeeAmount}
                      onChange={handleInputChange}
                      InputProps={{ readOnly: true }}
                      helperText="Calculated automatically"
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <Typography variant="subtitle2" color="text.secondary" sx={{ mt: 1 }} gutterBottom>Collections</Typography>
                    <Divider sx={{ mb: 2 }} />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <FormControlLabel
                      control={
                        <Switch
                          name="inCollections"
                          checked={formData.inCollections}
                          onChange={handleInputChange}
                        />
                      }
                      label="In Collections"
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Collection Reference"
                      name="collectionReference"
                      value={formData.collectionReference}
                      onChange={handleInputChange}
                      disabled={!formData.inCollections}
                    />
                  </Grid>
                </Grid>
              </AccordionDetails>
            </Accordion>

            {/* Accordion for Relations & Admin */}
            <Accordion sx={{ mt: 2 }}>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography variant="subtitle1" fontWeight={600}>Relations &amp; Admin</Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <Typography variant="subtitle2" color="text.secondary" gutterBottom>Relations</Typography>
                    <Divider sx={{ mb: 2 }} />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      type="number"
                      label="Contact ID"
                      value={formData.contactId || ''}
                      onChange={(e) => setFormData(prev => ({ ...prev, contactId: e.target.value ? parseInt(e.target.value) : null }))}
                      helperText="ID of the associated contact"
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      type="number"
                      label="Original Invoice ID"
                      value={formData.originalInvoiceId || ''}
                      onChange={(e) => setFormData(prev => ({ ...prev, originalInvoiceId: e.target.value ? parseInt(e.target.value) : null }))}
                      helperText="For credit notes or replacements"
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <Typography variant="subtitle2" color="text.secondary" sx={{ mt: 1 }} gutterBottom>Admin</Typography>
                    <Divider sx={{ mb: 2 }} />
                  </Grid>
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      multiline
                      rows={2}
                      label="Footer"
                      name="footer"
                      value={formData.footer}
                      onChange={handleInputChange}
                      placeholder="Invoice footer text"
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      multiline
                      rows={4}
                      label="Terms and Conditions"
                      name="termsAndConditions"
                      value={formData.termsAndConditions}
                      onChange={handleInputChange}
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Void Reason"
                      name="voidReason"
                      value={formData.voidReason}
                      onChange={handleInputChange}
                      InputProps={{ readOnly: true }}
                      helperText="Set by the system when voided"
                    />
                  </Grid>
                </Grid>
              </AccordionDetails>
            </Accordion>

            {/* Accordion for Billing Information */}
            <Accordion sx={{ mt: 2 }}>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography variant="subtitle1" fontWeight={600}>Billing Information</Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Grid container spacing={2}>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Billing Name"
                      name="billingName"
                      value={formData.billingName}
                      onChange={handleInputChange}
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Billing Company"
                      name="billingCompany"
                      value={formData.billingCompany}
                      onChange={handleInputChange}
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      label="Street"
                      name="billingStreet"
                      value={formData.billingStreet}
                      onChange={handleInputChange}
                    />
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      label="City"
                      name="billingCity"
                      value={formData.billingCity}
                      onChange={handleInputChange}
                    />
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      label="State"
                      name="billingState"
                      value={formData.billingState}
                      onChange={handleInputChange}
                    />
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      label="Postal Code"
                      name="billingPostalCode"
                      value={formData.billingPostalCode}
                      onChange={handleInputChange}
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Country"
                      name="billingCountry"
                      value={formData.billingCountry}
                      onChange={handleInputChange}
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      type="email"
                      label="Billing Email"
                      name="billingEmail"
                      value={formData.billingEmail}
                      onChange={handleInputChange}
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Billing Phone"
                      name="billingPhone"
                      value={formData.billingPhone}
                      onChange={handleInputChange}
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      type="number"
                      label="Early Payment Discount %"
                      name="earlyPaymentDiscountPercent"
                      value={formData.earlyPaymentDiscountPercent}
                      onChange={handleInputChange}
                      inputProps={{ min: 0, max: 100, step: 0.01 }}
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      type="number"
                      label="Early Payment Discount Days"
                      name="earlyPaymentDiscountDays"
                      value={formData.earlyPaymentDiscountDays}
                      onChange={handleInputChange}
                      inputProps={{ min: 0, step: 1 }}
                      helperText="Number of days within which early discount applies"
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      type="number"
                      label="Late Fee %"
                      name="lateFeePercent"
                      value={formData.lateFeePercent}
                      onChange={handleInputChange}
                      inputProps={{ min: 0, max: 100, step: 0.01 }}
                      helperText="Percentage charged on overdue balance"
                    />
                  </Grid>
                </Grid>
              </AccordionDetails>
            </Accordion>
          </TabPanel>

          {/* Tab 1: Line Items (read-only when editing) */}
          {editingId && (
            <TabPanel value={dialogTab} index={1}>
              {lineItems.length === 0 ? (
                <Typography color="text.secondary" textAlign="center" py={4}>
                  No line items yet. Add line items through the API.
                </Typography>
              ) : (
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Description</TableCell>
                      <TableCell align="right">Qty</TableCell>
                      <TableCell align="right">Unit Price</TableCell>
                      <TableCell align="right">Discount</TableCell>
                      <TableCell align="right">Total</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {lineItems.map(item => (
                      <TableRow key={item.id}>
                        <TableCell>{item.description}</TableCell>
                        <TableCell align="right">{item.quantity}</TableCell>
                        <TableCell align="right">{formatCurrency(item.unitPrice)}</TableCell>
                        <TableCell align="right">{formatCurrency(item.discount)}</TableCell>
                        <TableCell align="right">{formatCurrency(item.totalPrice)}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
            </TabPanel>
          )}

        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <ActionButton
            onClick={handleSaveInvoice}
            loading={dialogApi.loading}
            variant="contained"
          >
            {editingId ? 'Update Invoice' : 'Create Invoice'}
          </ActionButton>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

export default InvoicesPage;

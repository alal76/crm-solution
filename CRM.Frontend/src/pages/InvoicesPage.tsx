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
  TablePagination,
  Chip,
  IconButton,
  Tooltip,
  Dialog,
  DialogContent,
  DialogActions,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Stack,
  Alert,
  CircularProgress,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Refresh as RefreshIcon,
  Send as SendIcon,
  CheckCircle as CheckCircleIcon,
  Warning as WarningIcon,
  Receipt as ReceiptIcon,
} from '@mui/icons-material';
import { DialogError } from '../components/common/DialogError';
import ActionButton from '../components/common/ActionButton';
import { DialogHeader } from '../components/common/DialogHeader';
import { EnhancedEmptyState } from '../components/common/EnhancedEmptyState';
import DynamicEntityForm, { ExtraTab } from '../components/DynamicEntityForm';
import { useApiState } from '../hooks/useApiState';
import { usePagination } from '../hooks/usePagination';
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

  const { paginatedData: paginatedInvoices, page, pageSize, handlePageChange, handlePageSizeChange, pageSizeOptions } = usePagination(filteredInvoices, { defaultPageSize: 25 });

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
                  paginatedInvoices.map((invoice) => {
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
            <TablePagination
              component="div"
              count={filteredInvoices.length}
              page={page}
              onPageChange={handlePageChange}
              rowsPerPage={pageSize}
              onRowsPerPageChange={handlePageSizeChange}
              rowsPerPageOptions={pageSizeOptions}
            />
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
          <DialogError error={dialogApi.error} />

          <DynamicEntityForm
            moduleName="Invoices"
            formData={formData}
            onChange={handleInputChange}
            onSelectChange={(e: any) => setFormData(prev => ({ ...prev, [e.target.name]: e.target.value }))}
            setFormData={setFormData}
            activeTab={dialogTab}
            editingId={editingId}
            onTabChange={setDialogTab}
            excludeFields={['tags', 'customFields']}
            extraTabs={[
              {
                index: 100,
                name: 'Line Items',
                icon: <ReceiptIcon fontSize="small" />,
                editOnly: true,
                render: () => (
                  lineItems.length === 0 ? (
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
                  )
                ),
              },
            ]}
          />

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

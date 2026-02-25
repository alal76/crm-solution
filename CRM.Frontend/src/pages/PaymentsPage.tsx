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
import type { SelectChangeEvent } from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Refresh as RefreshIcon,
  Undo as UndoIcon,
  Warning as WarningIcon,
} from '@mui/icons-material';
import { DialogError } from '../components/common/DialogError';
import DynamicEntityForm from '../components/DynamicEntityForm';
import ActionButton from '../components/common/ActionButton';
import { DialogHeader } from '../components/common/DialogHeader';
import { EnhancedEmptyState } from '../components/common/EnhancedEmptyState';
import { useApiState } from '../hooks/useApiState';
import { usePagination } from '../hooks/usePagination';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';

// ==================== ENUMS ====================

enum PaymentStatus {
  Pending = 0, Processing = 1, Completed = 2, Failed = 3, Declined = 4,
  Cancelled = 5, Refunded = 6, PartiallyRefunded = 7, Disputed = 8,
  Voided = 9, OnHold = 10, Expired = 11,
}

enum PaymentMethod {
  CreditCard = 0, DebitCard = 1, BankTransfer = 2, WireTransfer = 3, Check = 4,
  Cash = 5, PayPal = 6, Stripe = 7, ApplePay = 8, GooglePay = 9, Venmo = 10,
  Crypto = 11, StoreCredit = 12, GiftCard = 13, Financing = 14, PurchaseOrder = 15, Other = 16,
}

// ==================== INTERFACES ====================

interface Payment {
  id: number;
  transactionId?: string;
  invoiceId?: number;
  invoiceNumber?: string;
  customerId: number;
  customerName?: string;
  amount: number;
  refundedAmount: number;
  status: PaymentStatus;
  paymentMethod: PaymentMethod;
  paymentDate: string;
  reference?: string;
  failureReason?: string;
  notes?: string;
  createdAt: string;
}

interface PaymentForm {
  invoiceId: number | null;
  amount: number;
  paymentMethod: PaymentMethod;
  reference: string;
  notes: string;
  cardBrand: string;
  cardLast4: string;
  cardExpMonth: number;
  cardExpYear: number;
  cardholderName: string;
  bankName: string;
  accountLast4: string;
  accountType: string;
  gateway: string;
  gatewayResponseCode: string;
  internalNotes: string;
  // Identifiers
  paymentNumber: string;
  externalPaymentId: string;
  gatewayTransactionId: string;
  gatewayReference: string;
  checkNumber: string;
  // Financials
  amountApplied: number;
  processingFee: number;
  exchangeRate: number;
  // Dates
  processedDate: string;
  settledDate: string;
  depositDate: string;
  scheduledDate: string;
  // Relations
  accountId: number | null;
  originalPaymentId: number | null;
}

// ==================== CONSTANTS ====================

type ChipColor = 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning';

const PAYMENT_STATUS_OPTIONS: Array<{ value: PaymentStatus; label: string; color: ChipColor }> = [
  { value: PaymentStatus.Pending, label: 'Pending', color: 'warning' },
  { value: PaymentStatus.Processing, label: 'Processing', color: 'info' },
  { value: PaymentStatus.Completed, label: 'Completed', color: 'success' },
  { value: PaymentStatus.Failed, label: 'Failed', color: 'error' },
  { value: PaymentStatus.Declined, label: 'Declined', color: 'error' },
  { value: PaymentStatus.Cancelled, label: 'Cancelled', color: 'default' },
  { value: PaymentStatus.Refunded, label: 'Refunded', color: 'secondary' },
  { value: PaymentStatus.PartiallyRefunded, label: 'Partially Refunded', color: 'warning' },
  { value: PaymentStatus.Disputed, label: 'Disputed', color: 'error' },
  { value: PaymentStatus.Voided, label: 'Voided', color: 'default' },
  { value: PaymentStatus.OnHold, label: 'On Hold', color: 'warning' },
  { value: PaymentStatus.Expired, label: 'Expired', color: 'default' },
];

const PAYMENT_METHOD_OPTIONS: Array<{ value: PaymentMethod; label: string }> = [
  { value: PaymentMethod.CreditCard, label: 'Credit Card' },
  { value: PaymentMethod.DebitCard, label: 'Debit Card' },
  { value: PaymentMethod.BankTransfer, label: 'Bank Transfer' },
  { value: PaymentMethod.WireTransfer, label: 'Wire Transfer' },
  { value: PaymentMethod.Check, label: 'Check' },
  { value: PaymentMethod.Cash, label: 'Cash' },
  { value: PaymentMethod.PayPal, label: 'PayPal' },
  { value: PaymentMethod.Stripe, label: 'Stripe' },
  { value: PaymentMethod.ApplePay, label: 'Apple Pay' },
  { value: PaymentMethod.GooglePay, label: 'Google Pay' },
  { value: PaymentMethod.Venmo, label: 'Venmo' },
  { value: PaymentMethod.Crypto, label: 'Crypto' },
  { value: PaymentMethod.StoreCredit, label: 'Store Credit' },
  { value: PaymentMethod.GiftCard, label: 'Gift Card' },
  { value: PaymentMethod.Financing, label: 'Financing' },
  { value: PaymentMethod.PurchaseOrder, label: 'Purchase Order' },
  { value: PaymentMethod.Other, label: 'Other' },
];

// ==================== HELPER FUNCTIONS ====================

const getStatusInfo = (status: PaymentStatus): { label: string; color: ChipColor } =>
  PAYMENT_STATUS_OPTIONS.find(s => s.value === status) || { label: 'Unknown', color: 'default' };

const getMethodLabel = (method: PaymentMethod): string =>
  PAYMENT_METHOD_OPTIONS.find(m => m.value === method)?.label || 'Unknown';

const formatCurrency = (value: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);

const formatDate = (dateString: string) =>
  dateString ? new Date(dateString).toLocaleDateString() : '-';

// ==================== MAIN COMPONENT ====================

function PaymentsPage() {
  const [payments, setPayments] = useState<Payment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [dialogTab, setDialogTab] = useState(0);
  const [filterStatus, setFilterStatus] = useState<PaymentStatus | 'all'>('all');

  const emptyForm: PaymentForm = {
    invoiceId: null,
    amount: 0,
    paymentMethod: PaymentMethod.CreditCard,
    reference: '',
    notes: '',
    cardBrand: '',
    cardLast4: '',
    cardExpMonth: 0,
    cardExpYear: 0,
    cardholderName: '',
    bankName: '',
    accountLast4: '',
    accountType: '',
    gateway: '',
    gatewayResponseCode: '',
    internalNotes: '',
    // Identifiers
    paymentNumber: '',
    externalPaymentId: '',
    gatewayTransactionId: '',
    gatewayReference: '',
    checkNumber: '',
    // Financials
    amountApplied: 0,
    processingFee: 0,
    exchangeRate: 1,
    // Dates
    processedDate: '',
    settledDate: '',
    depositDate: '',
    scheduledDate: '',
    // Relations
    accountId: null,
    originalPaymentId: null,
  };
  const [formData, setFormData] = useState<PaymentForm>(emptyForm);

  const dialogApi = useApiState();

  // ==================== DATA FETCHING ====================

  useEffect(() => {
    fetchPayments();
  }, []);

  const fetchPayments = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/payments');
      setPayments(response.data);
      setError(null);
    } catch (err: any) {
      if (err.response?.status === 404) {
        setPayments([]);
        setError(null);
      } else {
        setError(err.response?.data?.message || 'Failed to fetch payments');
      }
    } finally {
      setLoading(false);
    }
  };

  // ==================== DIALOG HANDLERS ====================

  const handleOpenDialog = () => {
    setFormData(emptyForm);
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    dialogApi.clearError();
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'number' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleSelectChange = (e: SelectChangeEvent<number | string>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name as string]: value }));
  };

  // ==================== SAVE / ACTION OPERATIONS ====================

  const handleProcessPayment = async () => {
    if (!formData.invoiceId || formData.amount <= 0) {
      dialogApi.setError('Invoice and a positive amount are required');
      return;
    }

    await dialogApi.execute(async () => {
      await apiClient.post('/payments/process', {
        invoiceId: formData.invoiceId,
        amount: formData.amount,
        method: formData.paymentMethod,
        details: { externalReference: formData.reference },
        // Identifiers
        paymentNumber: formData.paymentNumber,
        externalPaymentId: formData.externalPaymentId,
        gatewayTransactionId: formData.gatewayTransactionId,
        gatewayReference: formData.gatewayReference,
        checkNumber: formData.checkNumber,
        // Financials
        amountApplied: formData.amountApplied,
        processingFee: formData.processingFee,
        exchangeRate: formData.exchangeRate,
        // Dates
        processedDate: formData.processedDate || null,
        settledDate: formData.settledDate || null,
        depositDate: formData.depositDate || null,
        scheduledDate: formData.scheduledDate || null,
        // Relations
        accountId: formData.accountId,
        originalPaymentId: formData.originalPaymentId,
      });
      setSuccessMessage('Payment processed successfully');
      handleCloseDialog();
      fetchPayments();
      setTimeout(() => setSuccessMessage(null), 3000);
    });
  };

  const handleRefundPayment = async (id: number) => {
    const reason = window.prompt('Enter refund reason:');
    if (!reason) return;

    try {
      await apiClient.post(`/payments/${id}/refund`, { amount: 0, reason }); // Full refund
      setSuccessMessage('Payment refunded');
      fetchPayments();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to refund payment');
    }
  };

  const handleVoidPayment = async (id: number) => {
    const reason = window.prompt('Enter void reason:');
    if (!reason) return;

    try {
      await apiClient.post(`/payments/${id}/void`, { reason });
      setSuccessMessage('Payment voided');
      fetchPayments();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to void payment');
    }
  };

  // ==================== FILTERING ====================

  const filteredPayments = filterStatus === 'all'
    ? payments
    : payments.filter(p => p.status === filterStatus);

  const { paginatedData: paginatedPayments, page, pageSize, handlePageChange, handlePageSizeChange, pageSizeOptions } = usePagination(filteredPayments, { defaultPageSize: 25 });

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
            <Typography variant="h4">Payments</Typography>
          </Box>
          <Stack direction="row" spacing={2}>
            <FormControl size="small" sx={{ minWidth: 150 }}>
              <InputLabel>Status Filter</InputLabel>
              <Select
                value={filterStatus}
                onChange={(e) => setFilterStatus(e.target.value as PaymentStatus | 'all')}
                label="Status Filter"
              >
                <MenuItem value="all">All</MenuItem>
                {PAYMENT_STATUS_OPTIONS.map(opt => (
                  <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                ))}
              </Select>
            </FormControl>
            <Button variant="outlined" startIcon={<RefreshIcon />} onClick={fetchPayments}>
              Refresh
            </Button>
            <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenDialog}>
              Record Payment
            </Button>
          </Stack>
        </Box>

        {/* Alerts */}
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

        {/* Failed Payments Alert */}
        {payments.filter(p => p.status === PaymentStatus.Failed).length > 0 && (
          <Alert severity="warning" icon={<WarningIcon />} sx={{ mb: 2 }}>
            {payments.filter(p => p.status === PaymentStatus.Failed).length} payment(s) have failed
          </Alert>
        )}

        {/* Payments Table */}
        <Card>
          <CardContent>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Transaction ID</TableCell>
                  <TableCell>Account</TableCell>
                  <TableCell>Invoice</TableCell>
                  <TableCell>Method</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Date</TableCell>
                  <TableCell align="right">Amount</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredPayments.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={8} sx={{ border: 0 }}>
                      <EnhancedEmptyState
                        illustration="generic"
                        title="No payments yet"
                        description="Record your first payment to start tracking transactions"
                        variant="no-data"
                        primaryActionLabel="Record Payment"
                        onPrimaryAction={handleOpenDialog}
                      />
                    </TableCell>
                  </TableRow>
                ) : (
                  paginatedPayments.map((payment) => {
                    const statusInfo = getStatusInfo(payment.status);

                    return (
                      <TableRow key={payment.id} hover>
                        <TableCell>
                          <Typography fontFamily="monospace" variant="body2">
                            {payment.transactionId || '-'}
                          </Typography>
                        </TableCell>
                        <TableCell>{payment.customerName || '-'}</TableCell>
                        <TableCell>{payment.invoiceNumber || '-'}</TableCell>
                        <TableCell>{getMethodLabel(payment.paymentMethod)}</TableCell>
                        <TableCell>
                          <Chip label={statusInfo.label} size="small" color={statusInfo.color} />
                        </TableCell>
                        <TableCell>{formatDate(payment.paymentDate)}</TableCell>
                        <TableCell align="right">
                          <Typography fontWeight="medium">{formatCurrency(payment.amount)}</Typography>
                          {payment.refundedAmount > 0 && (
                            <Typography variant="caption" color="error.main" display="block">
                              -{formatCurrency(payment.refundedAmount)} refunded
                            </Typography>
                          )}
                        </TableCell>
                        <TableCell align="right">
                          {payment.status === PaymentStatus.Completed && (
                            <Tooltip title="Refund">
                              <IconButton size="small" color="warning" onClick={() => handleRefundPayment(payment.id)}>
                                <UndoIcon />
                              </IconButton>
                            </Tooltip>
                          )}
                          {payment.status === PaymentStatus.Pending && (
                            <Tooltip title="Void">
                              <IconButton size="small" color="error" onClick={() => handleVoidPayment(payment.id)}>
                                <DeleteIcon />
                              </IconButton>
                            </Tooltip>
                          )}
                        </TableCell>
                      </TableRow>
                    );
                  })
                )}
              </TableBody>
            </Table>
            <TablePagination
              component="div"
              count={filteredPayments.length}
              page={page}
              onPageChange={handlePageChange}
              rowsPerPage={pageSize}
              onRowsPerPageChange={handlePageSizeChange}
              rowsPerPageOptions={pageSizeOptions}
            />
          </CardContent>
        </Card>
      </Box>

      {/* Process Payment Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogHeader
          mode="create"
          entityType="payment"
          onClose={handleCloseDialog}
        />
        <DialogContent dividers>
          <DialogError error={dialogApi.error} />

          <DynamicEntityForm
            moduleName="Payments"
            formData={formData}
            onChange={handleInputChange}
            onSelectChange={(e: any) => setFormData((prev: any) => ({ ...prev, [e.target.name]: e.target.value }))}
            setFormData={setFormData}
            activeTab={dialogTab}
            editingId={null}
            onTabChange={setDialogTab}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <ActionButton
            onClick={handleProcessPayment}
            loading={dialogApi.loading}
            variant="contained"
          >
            Process Payment
          </ActionButton>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

export default PaymentsPage;

import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Grid,
  MenuItem,
  Autocomplete,
  CircularProgress,
  Typography,
} from '@mui/material';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { DialogHeader } from '../common/DialogHeader';
import { DialogError } from '../common/DialogError';
import ActionButton from '../common/ActionButton';
import apiClient from '../../services/apiClient';

/**
 * Payment Method enum matching backend
 */
export enum PaymentMethod {
  CreditCard = 0,
  DebitCard = 1,
  BankTransfer = 2,
  WireTransfer = 3,
  Check = 4,
  Cash = 5,
  PayPal = 6,
  Stripe = 7,
  ApplePay = 8,
  GooglePay = 9,
  Venmo = 10,
  Crypto = 11,
  StoreCredit = 12,
  GiftCard = 13,
  Financing = 14,
  PurchaseOrder = 15,
  Other = 16,
}

const PAYMENT_METHOD_OPTIONS = [
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

interface Invoice {
  id: number;
  invoiceNumber: string;
  accountName?: string;
  balanceDue: number;
  totalAmount: number;
}

export interface CreatePaymentDto {
  invoiceId: number;
  amount: number;
  paymentMethod: PaymentMethod;
  paymentDate?: string;
  reference?: string;
  notes?: string;
}

export interface PaymentFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreatePaymentDto) => Promise<void>;
  invoiceId?: number; // Pre-fill if paying specific invoice
}

/**
 * PaymentForm - Standalone payment form component
 * 
 * Features:
 * - Invoice selector (autocomplete)
 * - Amount input
 * - Payment method dropdown
 * - Payment date picker
 * - Reference number input
 * - Notes textarea
 * - Form validation
 */
export const PaymentForm: React.FC<PaymentFormProps> = ({
  open,
  onClose,
  onSubmit,
  invoiceId,
}) => {
  const [formData, setFormData] = useState<CreatePaymentDto>({
    invoiceId: 0,
    amount: 0,
    paymentMethod: PaymentMethod.CreditCard,
    paymentDate: new Date().toISOString().split('T')[0],
    reference: '',
    notes: '',
  });

  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [selectedInvoice, setSelectedInvoice] = useState<Invoice | null>(null);
  const [loadingInvoices, setLoadingInvoices] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Load invoices for dropdown
  useEffect(() => {
    if (open) {
      fetchInvoices();
    }
  }, [open]);

  // Pre-fill invoice if provided
  useEffect(() => {
    if (invoiceId && invoices.length > 0) {
      const invoice = invoices.find((inv) => inv.id === invoiceId);
      if (invoice) {
        setSelectedInvoice(invoice);
        setFormData((prev) => ({
          ...prev,
          invoiceId: invoice.id,
          amount: invoice.balanceDue || invoice.totalAmount,
        }));
      }
    }
  }, [invoiceId, invoices]);

  // Reset form when closed
  useEffect(() => {
    if (!open) {
      setFormData({
        invoiceId: 0,
        amount: 0,
        paymentMethod: PaymentMethod.CreditCard,
        paymentDate: new Date().toISOString().split('T')[0],
        reference: '',
        notes: '',
      });
      setSelectedInvoice(null);
      setError(null);
    }
  }, [open]);

  const fetchInvoices = async () => {
    try {
      setLoadingInvoices(true);
      const response = await apiClient.get('/invoices');
      // Filter to unpaid/partially paid invoices
      const unpaidInvoices = (response.data || []).filter(
        (inv: Invoice) => inv.balanceDue && inv.balanceDue > 0
      );
      setInvoices(unpaidInvoices);
    } catch (err) {
      console.error('Failed to fetch invoices:', err);
      setInvoices([]);
    } finally {
      setLoadingInvoices(false);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: name === 'amount' ? Number.parseFloat(value) || 0 : value,
    }));
  };

  const handleInvoiceChange = (_event: any, value: Invoice | null) => {
    setSelectedInvoice(value);
    setFormData((prev) => ({
      ...prev,
      invoiceId: value?.id || 0,
      amount: value?.balanceDue || value?.totalAmount || 0,
    }));
  };

  const handlePaymentMethodChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prev) => ({
      ...prev,
      paymentMethod: Number.parseInt(e.target.value, 10) as PaymentMethod,
    }));
  };

  const validate = (): boolean => {
    if (!formData.invoiceId || formData.invoiceId === 0) {
      setError('Invoice is required');
      return false;
    }
    if (formData.amount <= 0) {
      setError('Amount must be greater than zero');
      return false;
    }
    if (selectedInvoice && formData.amount > selectedInvoice.balanceDue) {
      setError('Amount cannot exceed invoice balance due');
      return false;
    }
    if (!formData.paymentDate) {
      setError('Payment date is required');
      return false;
    }
    return true;
  };

  const handleSubmit = async () => {
    setError(null);

    if (!validate()) {
      return;
    }

    try {
      setLoading(true);
      await onSubmit(formData);
      onClose();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to process payment');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogHeader mode="create" entityType="payment" onClose={onClose} />
      <DialogContent dividers>
        <DialogError error={error} onClose={() => setError(null)} />

        <LocalizationProvider dateAdapter={AdapterDateFns}>
          <Grid container spacing={2}>
            {/* Invoice Selector */}
            <Grid item xs={12}>
              <Autocomplete
                options={invoices}
                getOptionLabel={(option) =>
                  `${option.invoiceNumber} - ${option.accountName || 'Unknown'} (Due: $${option.balanceDue?.toFixed(2) || option.totalAmount?.toFixed(2)})`
                }
                value={selectedInvoice}
                onChange={handleInvoiceChange}
                loading={loadingInvoices}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Invoice *"
                    placeholder="Select invoice to pay"
                    InputProps={{
                      ...params.InputProps,
                      endAdornment: (
                        <>
                          {loadingInvoices ? <CircularProgress size={20} /> : null}
                          {params.InputProps.endAdornment}
                        </>
                      ),
                    }}
                  />
                )}
              />
            </Grid>

            {/* Amount */}
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Amount *"
                name="amount"
                type="number"
                value={formData.amount}
                onChange={handleInputChange}
                InputProps={{ startAdornment: '$' }}
                helperText={
                  selectedInvoice
                    ? `Balance Due: $${selectedInvoice.balanceDue?.toFixed(2) || selectedInvoice.totalAmount?.toFixed(2)}`
                    : ''
                }
              />
            </Grid>

            {/* Payment Date */}
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Payment Date *"
                name="paymentDate"
                type="date"
                value={formData.paymentDate}
                onChange={handleInputChange}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>

            {/* Payment Method */}
            <Grid item xs={12}>
              <TextField
                fullWidth
                select
                label="Payment Method *"
                name="paymentMethod"
                value={formData.paymentMethod}
                onChange={handlePaymentMethodChange}
              >
                {PAYMENT_METHOD_OPTIONS.map((option) => (
                  <MenuItem key={option.value} value={option.value}>
                    {option.label}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>

            {/* Reference Number */}
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Reference Number"
                name="reference"
                value={formData.reference}
                onChange={handleInputChange}
                placeholder="Transaction ID, check number, etc."
              />
            </Grid>

            {/* Notes */}
            <Grid item xs={12}>
              <TextField
                fullWidth
                multiline
                rows={3}
                label="Notes"
                name="notes"
                value={formData.notes}
                onChange={handleInputChange}
                placeholder="Additional payment details"
              />
            </Grid>
          </Grid>
        </LocalizationProvider>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={loading}>
          Cancel
        </Button>
        <ActionButton onClick={handleSubmit} loading={loading} variant="contained">
          Process Payment
        </ActionButton>
      </DialogActions>
    </Dialog>
  );
};

export default PaymentForm;

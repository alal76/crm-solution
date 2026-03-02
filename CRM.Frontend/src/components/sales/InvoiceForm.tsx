import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Grid,
  MenuItem,
  Box,
  Typography,
  Autocomplete,
  CircularProgress,
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { DialogHeader } from '../common/DialogHeader';
import { DialogError } from '../common/DialogError';
import ActionButton from '../common/ActionButton';
import apiClient from '../../services/apiClient';

/**
 * Invoice Status enum matching backend
 */
export enum InvoiceStatus {
  Draft = 0,
  PendingApproval = 1,
  Approved = 2,
  Sent = 3,
  Viewed = 4,
  PartiallyPaid = 5,
  Paid = 6,
  Overdue = 7,
  Disputed = 8,
  Voided = 9,
  WrittenOff = 10,
  Collections = 11,
  Refunded = 12,
}

interface Account {
  id: number;
  name: string;
}

export interface InvoiceDto {
  id: number;
  invoiceNumber: string;
  accountId: number;
  accountName?: string;
  status: InvoiceStatus;
  invoiceDate: string;
  dueDate: string;
  subtotal: number;
  taxAmount: number;
  discountAmount: number;
  totalAmount: number;
  notes?: string;
  paymentTerms?: string;
  currencyCode?: string;
}

export interface CreateInvoiceDto {
  accountId: number;
  status?: InvoiceStatus;
  invoiceDate: string;
  dueDate: string;
  subtotal: number;
  taxAmount: number;
  discountAmount: number;
  notes?: string;
  paymentTerms?: string;
  currencyCode?: string;
}

export interface InvoiceFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateInvoiceDto) => Promise<void>;
  initialData?: InvoiceDto;
  mode: 'create' | 'edit';
}

/**
 * InvoiceForm - Standalone invoice form component
 * 
 * Features:
 * - Customer/Account selector dropdown
 * - Invoice date picker
 * - Due date picker
 * - Amount inputs (subtotal, tax, discount)
 * - Currency selector
 * - Notes textarea
 * - Submit and Cancel buttons
 * - Form validation
 */
export const InvoiceForm: React.FC<InvoiceFormProps> = ({
  open,
  onClose,
  onSubmit,
  initialData,
  mode,
}) => {
  const [formData, setFormData] = useState<CreateInvoiceDto>({
    accountId: 0,
    status: InvoiceStatus.Draft,
    invoiceDate: new Date().toISOString().split('T')[0],
    dueDate: '',
    subtotal: 0,
    taxAmount: 0,
    discountAmount: 0,
    notes: '',
    paymentTerms: '',
    currencyCode: 'USD',
  });

  const [accounts, setAccounts] = useState<Account[]>([]);
  const [selectedAccount, setSelectedAccount] = useState<Account | null>(null);
  const [loadingAccounts, setLoadingAccounts] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Load accounts for dropdown
  useEffect(() => {
    if (open) {
      fetchAccounts();
    }
  }, [open]);

  // Initialize form with initial data
  useEffect(() => {
    if (initialData && mode === 'edit') {
      setFormData({
        accountId: initialData.accountId,
        status: initialData.status,
        invoiceDate: initialData.invoiceDate?.split('T')[0] || '',
        dueDate: initialData.dueDate?.split('T')[0] || '',
        subtotal: initialData.subtotal || 0,
        taxAmount: initialData.taxAmount || 0,
        discountAmount: initialData.discountAmount || 0,
        notes: initialData.notes || '',
        paymentTerms: initialData.paymentTerms || '',
        currencyCode: initialData.currencyCode || 'USD',
      });
      
      if (initialData.accountId && initialData.accountName) {
        setSelectedAccount({
          id: initialData.accountId,
          name: initialData.accountName,
        });
      }
    } else {
      // Reset form for create mode
      setFormData({
        accountId: 0,
        status: InvoiceStatus.Draft,
        invoiceDate: new Date().toISOString().split('T')[0],
        dueDate: '',
        subtotal: 0,
        taxAmount: 0,
        discountAmount: 0,
        notes: '',
        paymentTerms: '',
        currencyCode: 'USD',
      });
      setSelectedAccount(null);
    }
  }, [initialData, mode, open]);

  const fetchAccounts = async () => {
    try {
      setLoadingAccounts(true);
      const response = await apiClient.get('/accounts');
      setAccounts(response.data || []);
    } catch (err) {
      console.error('Failed to fetch accounts:', err);
      setAccounts([]);
    } finally {
      setLoadingAccounts(false);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: name.includes('Amount') || name === 'subtotal' ? Number.parseFloat(value) || 0 : value,
    }));
  };

  const handleAccountChange = (_event: any, value: Account | null) => {
    setSelectedAccount(value);
    setFormData((prev) => ({
      ...prev,
      accountId: value?.id || 0,
    }));
  };

  const validate = (): boolean => {
    if (!formData.accountId || formData.accountId === 0) {
      setError('Account is required');
      return false;
    }
    if (!formData.invoiceDate) {
      setError('Invoice date is required');
      return false;
    }
    if (!formData.dueDate) {
      setError('Due date is required');
      return false;
    }
    if (formData.subtotal < 0) {
      setError('Subtotal cannot be negative');
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
      setError(err.response?.data?.message || 'Failed to save invoice');
    } finally {
      setLoading(false);
    }
  };

  const totalAmount = formData.subtotal + formData.taxAmount - formData.discountAmount;

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogHeader
        mode={mode}
        entityType="invoice"
        entityName={initialData?.invoiceNumber}
        entityId={initialData?.id}
        onClose={onClose}
      />
      <DialogContent dividers>
        <DialogError error={error} onClose={() => setError(null)} />

        <LocalizationProvider dateAdapter={AdapterDateFns}>
          <Grid container spacing={2}>
            {/* Account Selector */}
            <Grid item xs={12}>
              <Autocomplete
                options={accounts}
                getOptionLabel={(option) => option.name}
                value={selectedAccount}
                onChange={handleAccountChange}
                loading={loadingAccounts}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Account *"
                    placeholder="Select account"
                    InputProps={{
                      ...params.InputProps,
                      endAdornment: (
                        <>
                          {loadingAccounts ? <CircularProgress size={20} /> : null}
                          {params.InputProps.endAdornment}
                        </>
                      ),
                    }}
                  />
                )}
              />
            </Grid>

            {/* Invoice Date */}
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Invoice Date *"
                name="invoiceDate"
                type="date"
                value={formData.invoiceDate}
                onChange={handleInputChange}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>

            {/* Due Date */}
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Due Date *"
                name="dueDate"
                type="date"
                value={formData.dueDate}
                onChange={handleInputChange}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>

            {/* Subtotal */}
            <Grid item xs={12} sm={4}>
              <TextField
                fullWidth
                label="Subtotal *"
                name="subtotal"
                type="number"
                value={formData.subtotal}
                onChange={handleInputChange}
                InputProps={{ startAdornment: '$' }}
              />
            </Grid>

            {/* Tax Amount */}
            <Grid item xs={12} sm={4}>
              <TextField
                fullWidth
                label="Tax Amount"
                name="taxAmount"
                type="number"
                value={formData.taxAmount}
                onChange={handleInputChange}
                InputProps={{ startAdornment: '$' }}
              />
            </Grid>

            {/* Discount Amount */}
            <Grid item xs={12} sm={4}>
              <TextField
                fullWidth
                label="Discount Amount"
                name="discountAmount"
                type="number"
                value={formData.discountAmount}
                onChange={handleInputChange}
                InputProps={{ startAdornment: '$' }}
              />
            </Grid>

            {/* Total Amount (calculated) */}
            <Grid item xs={12}>
              <Box sx={{ p: 2, bgcolor: 'action.hover', borderRadius: 1 }}>
                <Typography variant="h6" textAlign="right">
                  Total Amount: ${totalAmount.toFixed(2)}
                </Typography>
              </Box>
            </Grid>

            {/* Currency */}
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                select
                label="Currency"
                name="currencyCode"
                value={formData.currencyCode}
                onChange={handleInputChange}
              >
                <MenuItem value="USD">USD - US Dollar</MenuItem>
                <MenuItem value="EUR">EUR - Euro</MenuItem>
                <MenuItem value="GBP">GBP - British Pound</MenuItem>
                <MenuItem value="CAD">CAD - Canadian Dollar</MenuItem>
              </TextField>
            </Grid>

            {/* Payment Terms */}
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Payment Terms"
                name="paymentTerms"
                value={formData.paymentTerms}
                onChange={handleInputChange}
                placeholder="e.g., Net 30"
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
                placeholder="Additional notes or instructions"
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
          {mode === 'create' ? 'Create Invoice' : 'Update Invoice'}
        </ActionButton>
      </DialogActions>
    </Dialog>
  );
};

export default InvoiceForm;

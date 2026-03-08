import React, { useState, useEffect } from 'react';
import {
  Box,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  Typography,
  CircularProgress,
  Alert,
} from '@mui/material';
import apiClient from '../../services/apiClient';

/**
 * Payment interface matching backend
 */
interface Payment {
  id: number;
  transactionId?: string;
  amount: number;
  paymentMethod: number; // PaymentMethod enum
  paymentDate: string;
  reference?: string;
  notes?: string;
  recordedBy?: string;
  createdAt: string;
}

const PAYMENT_METHOD_LABELS: Record<number, string> = {
  0: 'Credit Card',
  1: 'Debit Card',
  2: 'Bank Transfer',
  3: 'Wire Transfer',
  4: 'Check',
  5: 'Cash',
  6: 'PayPal',
  7: 'Stripe',
  8: 'Apple Pay',
  9: 'Google Pay',
  10: 'Venmo',
  11: 'Crypto',
  12: 'Store Credit',
  13: 'Gift Card',
  14: 'Financing',
  15: 'Purchase Order',
  16: 'Other',
};

const formatCurrency = (value: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);

const formatDate = (dateString: string) =>
  dateString ? new Date(dateString).toLocaleDateString() : '-';

export interface PaymentHistoryProps {
  invoiceId: number;
}

/**
 * PaymentHistory - Reusable payment history table
 * 
 * Features:
 * - Fetch payments for invoice
 * - Display table: Date, Amount, Method, Reference, Recorded By
 * - Loading state
 * - Empty state message
 */
export const PaymentHistory: React.FC<PaymentHistoryProps> = ({ invoiceId }) => {
  const [payments, setPayments] = useState<Payment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchPayments();
  }, [invoiceId]);

  const fetchPayments = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await apiClient.get(`/invoices/${invoiceId}/payments`);
      setPayments(response.data || []);
    } catch (err: unknown) {
      if ((err as any).response?.status === 404) {
        setPayments([]);
      } else {
        setError((err as any).response?.data?.message || 'Failed to fetch payment history');
      }
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" py={4}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ mb: 2 }}>
        {error}
      </Alert>
    );
  }

  if (payments.length === 0) {
    return (
      <Typography color="text.secondary" textAlign="center" py={4}>
        No payments recorded for this invoice yet.
      </Typography>
    );
  }

  return (
    <Box>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Date</TableCell>
            <TableCell>Transaction ID</TableCell>
            <TableCell align="right">Amount</TableCell>
            <TableCell>Method</TableCell>
            <TableCell>Reference</TableCell>
            <TableCell>Recorded By</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {payments.map((payment) => (
            <TableRow key={payment.id} hover>
              <TableCell>{formatDate(payment.paymentDate)}</TableCell>
              <TableCell>
                <Typography variant="body2" fontFamily="monospace">
                  {payment.transactionId || '-'}
                </Typography>
              </TableCell>
              <TableCell align="right">
                <Typography variant="body2" fontWeight="medium" color="success.main">
                  {formatCurrency(payment.amount)}
                </Typography>
              </TableCell>
              <TableCell>
                {PAYMENT_METHOD_LABELS[payment.paymentMethod] || 'Unknown'}
              </TableCell>
              <TableCell>
                <Typography variant="body2" color="text.secondary">
                  {payment.reference || '-'}
                </Typography>
              </TableCell>
              <TableCell>{payment.recordedBy || '-'}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <Box sx={{ mt: 2, textAlign: 'right' }}>
        <Typography variant="body2" fontWeight={600}>
          Total Paid: {formatCurrency(payments.reduce((sum, p) => sum + p.amount, 0))}
        </Typography>
      </Box>
    </Box>
  );
};

export default PaymentHistory;

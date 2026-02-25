/**
 * RefundDialog - Dialog for processing partial or full refunds.
 */

import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  MenuItem,
  Box,
  Typography,
  ToggleButtonGroup,
  ToggleButton,
  Card,
  CardContent,
  CircularProgress,
} from '@mui/material';

export interface RefundData {
  amount: number;
  reason: string;
  type: 'Full' | 'Partial';
  notes?: string;
}

interface RefundDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: RefundData) => void;
  paymentAmount: number;
  paymentId: number;
  currency?: string;
  loading?: boolean;
}

const REFUND_REASONS = [
  'Customer Request',
  'Product Defect',
  'Billing Error',
  'Duplicate',
  'Other',
];

const formatCurrency = (value: number, currency?: string | null) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: currency || 'USD' }).format(value);

const RefundDialog: React.FC<RefundDialogProps> = ({
  open,
  onClose,
  onSubmit,
  paymentAmount,
  paymentId,
  currency = 'USD',
  loading = false,
}) => {
  const [refundType, setRefundType] = useState<'Full' | 'Partial'>('Full');
  const [amount, setAmount] = useState<number>(paymentAmount);
  const [reason, setReason] = useState<string>('');
  const [notes, setNotes] = useState<string>('');
  const [showConfirm, setShowConfirm] = useState(false);

  useEffect(() => {
    if (open) {
      setRefundType('Full');
      setAmount(paymentAmount);
      setReason('');
      setNotes('');
      setShowConfirm(false);
    }
  }, [open, paymentAmount]);

  useEffect(() => {
    if (refundType === 'Full') {
      setAmount(paymentAmount);
    }
  }, [refundType, paymentAmount]);

  const isValid = amount > 0 && amount <= paymentAmount && reason !== '';

  const handleSubmit = () => {
    if (!showConfirm) {
      setShowConfirm(true);
      return;
    }
    onSubmit({ amount, reason, type: refundType, notes: notes || undefined });
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Process Refund — Payment #{paymentId}</DialogTitle>
      <DialogContent dividers>
        <Box display="flex" flexDirection="column" gap={2} mt={1}>
          <Typography variant="body2" color="text.secondary">
            Original Payment: <strong>{formatCurrency(paymentAmount, currency)}</strong>
          </Typography>

          <ToggleButtonGroup
            value={refundType}
            exclusive
            onChange={(_, val) => { if (val) setRefundType(val as 'Full' | 'Partial'); }}
            size="small"
            fullWidth
          >
            <ToggleButton value="Full">Full Refund</ToggleButton>
            <ToggleButton value="Partial">Partial Refund</ToggleButton>
          </ToggleButtonGroup>

          <TextField
            label="Refund Amount"
            type="number"
            value={amount}
            onChange={(e) => setAmount(Number(e.target.value))}
            disabled={refundType === 'Full'}
            inputProps={{ min: 0.01, max: paymentAmount, step: 0.01 }}
            helperText={refundType === 'Partial' ? `Max: ${formatCurrency(paymentAmount, currency)}` : undefined}
            error={amount > paymentAmount || amount <= 0}
            fullWidth
          />

          <TextField
            label="Reason"
            select
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            fullWidth
            required
          >
            {REFUND_REASONS.map((r) => (
              <MenuItem key={r} value={r}>{r}</MenuItem>
            ))}
          </TextField>

          <TextField
            label="Additional Notes"
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            multiline
            minRows={2}
            fullWidth
          />

          {showConfirm && (
            <Card variant="outlined" sx={{ bgcolor: 'warning.50' }}>
              <CardContent>
                <Typography variant="subtitle2" gutterBottom color="warning.main">
                  Confirm Refund
                </Typography>
                <Typography variant="body2">
                  Type: <strong>{refundType}</strong>
                </Typography>
                <Typography variant="body2">
                  Amount: <strong>{formatCurrency(amount, currency)}</strong>
                </Typography>
                <Typography variant="body2">
                  Reason: <strong>{reason}</strong>
                </Typography>
                {notes && (
                  <Typography variant="body2">Notes: {notes}</Typography>
                )}
              </CardContent>
            </Card>
          )}
        </Box>
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose} disabled={loading}>Cancel</Button>
        <Button
          variant="contained"
          color={showConfirm ? 'warning' : 'primary'}
          onClick={handleSubmit}
          disabled={!isValid || loading}
          startIcon={loading ? <CircularProgress size={18} /> : undefined}
        >
          {showConfirm ? 'Confirm Refund' : 'Review Refund'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default RefundDialog;

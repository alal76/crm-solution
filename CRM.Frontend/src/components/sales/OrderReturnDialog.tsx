/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * OrderReturnDialog — 4-step dialog for initiating an order return / RMA.
 *
 * Steps:
 *   0. Select Items — choose which line items to return with quantities
 *   1. Return Reason — select reason + optional description
 *   2. Refund Method — choose refund type + notes
 *   3. Confirm — review summary and submit
 */

import React, { useState, useMemo } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Stepper,
  Step,
  StepLabel,
  Box,
  Typography,
  Checkbox,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  RadioGroup,
  FormControlLabel,
  Radio,
  Divider,
  Stack,
  Alert,
  CircularProgress,
  Chip,
} from '@mui/material';
import {
  NavigateNext as NextIcon,
  NavigateBefore as BackIcon,
  AssignmentReturn as ReturnIcon,
} from '@mui/icons-material';
import {
  createReturn,
  OrderReturnReason,
  CreateReturnLineItemDto,
} from '../../services/orderReturnsService';

// ─── Types ──────────────────────────────────────────────────────────────────

export interface OrderLineItem {
  id: number;
  productId?: number;
  productName?: string;
  sku?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  discount?: number;
  orderId?: number;
  description?: string;
  fulfilledQuantity?: number;
}

interface SelectedItem {
  selected: boolean;
  returnQuantity: number;
}

type SelectedItemsMap = Record<number, SelectedItem>;

interface OrderReturnDialogProps {
  open: boolean;
  onClose: () => void;
  orderId: number;
  orderNumber?: string;
  lineItems: OrderLineItem[];
  onSuccess?: (message: string) => void;
}

// ─── Constants ───────────────────────────────────────────────────────────────

const STEPS = ['Select Items', 'Return Reason', 'Refund Method', 'Confirm'];

const REASON_OPTIONS = [
  { value: OrderReturnReason.Defective, label: 'Defective Product' },
  { value: OrderReturnReason.WrongItem, label: 'Wrong Item Received' },
  { value: OrderReturnReason.NotAsDescribed, label: 'Not as Described' },
  { value: OrderReturnReason.ChangedMind, label: 'Changed Mind' },
  { value: OrderReturnReason.DamagedInShipping, label: 'Damaged in Shipping' },
  { value: OrderReturnReason.Other, label: 'Other' },
];

const REFUND_METHODS = [
  { value: 'OriginalPayment', label: 'Original Payment Method' },
  { value: 'StoreCredit', label: 'Store Credit' },
  { value: 'Check', label: 'Check / Bank Transfer' },
];

const fmt = (v: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(v);

// ─── Component ──────────────────────────────────────────────────────────────

const OrderReturnDialog: React.FC<OrderReturnDialogProps> = ({
  open,
  onClose,
  orderId,
  orderNumber,
  lineItems,
  onSuccess,
}) => {
  const [activeStep, setActiveStep] = useState(0);

  // Step 0
  const [selectedItems, setSelectedItems] = useState<SelectedItemsMap>({});

  // Step 1
  const [reason, setReason] = useState<number>(OrderReturnReason.Defective);
  const [reasonDescription, setReasonDescription] = useState('');

  // Step 2
  const [refundMethod, setRefundMethod] = useState('OriginalPayment');
  const [notes, setNotes] = useState('');

  // Submit state
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState('');

  // ── Derived ────────────────────────────────────────────────────────────────
  const returnItems = useMemo(
    () => lineItems.filter(item => selectedItems[item.id]?.selected),
    [lineItems, selectedItems]
  );

  const estimatedRefund = useMemo(
    () =>
      returnItems.reduce((total, item) => {
        const qty = selectedItems[item.id]?.returnQuantity ?? 1;
        return total + item.unitPrice * qty;
      }, 0),
    [returnItems, selectedItems]
  );

  const canGoNext = () => {
    switch (activeStep) {
      case 0: return returnItems.length > 0;
      case 1: return true;
      case 2: return true;
      case 3: return false;
      default: return false;
    }
  };

  // ── Handlers ───────────────────────────────────────────────────────────────
  const handleItemCheck = (itemId: number, checked: boolean, lineItem: OrderLineItem) => {
    setSelectedItems(prev => ({
      ...prev,
      [itemId]: {
        selected: checked,
        returnQuantity: prev[itemId]?.returnQuantity ?? lineItem.quantity,
      },
    }));
  };

  const handleQtyChange = (itemId: number, qty: number, max: number) => {
    const clamped = Math.max(1, Math.min(max, qty));
    setSelectedItems(prev => ({
      ...prev,
      [itemId]: { ...prev[itemId], returnQuantity: clamped },
    }));
  };

  const handleNext = () => setActiveStep(s => s + 1);
  const handleBack = () => setActiveStep(s => s - 1);

  const handleClose = () => {
    // Reset state on close
    setActiveStep(0);
    setSelectedItems({});
    setReason(OrderReturnReason.Defective);
    setReasonDescription('');
    setRefundMethod('OriginalPayment');
    setNotes('');
    setSubmitError('');
    onClose();
  };

  const handleSubmit = async () => {
    setSubmitting(true);
    setSubmitError('');
    try {
      const lineItemDtos: CreateReturnLineItemDto[] = returnItems.map(item => ({
        orderLineItemId: item.id,
        productId: item.productId ?? 0,
        quantity: selectedItems[item.id]?.returnQuantity ?? 1,
        reason: reasonDescription || undefined,
      }));

      await createReturn({
        orderId,
        reason,
        reasonDescription: reasonDescription || undefined,
        notes: notes || undefined,
        refundAmount: estimatedRefund,
        restockingFee: 0,
        shippingRefund: 0,
        lineItems: lineItemDtos,
      });

      handleClose();
      onSuccess?.('Return request submitted successfully.');
    } catch {
      setSubmitError('Failed to submit return request. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };

  // ── Step renders ───────────────────────────────────────────────────────────

  const renderStep0 = () => (
    <Box>
      <Typography variant="body2" color="text.secondary" mb={2}>
        Select the items you want to return and specify the quantity.
      </Typography>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell padding="checkbox" />
            <TableCell>Item</TableCell>
            <TableCell align="right">Ordered</TableCell>
            <TableCell align="right">Return Qty</TableCell>
            <TableCell align="right">Unit Price</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {lineItems.map(item => {
            const sel = selectedItems[item.id];
            const isSelected = sel?.selected ?? false;
            return (
              <TableRow key={item.id} sx={{ opacity: isSelected ? 1 : 0.6 }}>
                <TableCell padding="checkbox">
                  <Checkbox
                    checked={isSelected}
                    onChange={e => handleItemCheck(item.id, e.target.checked, item)}
                  />
                </TableCell>
                <TableCell>
                  <Typography variant="body2">
                    {item.productName ?? `Product #${item.productId}`}
                  </Typography>
                  {item.sku && (
                    <Typography variant="caption" color="text.secondary">
                      SKU: {item.sku}
                    </Typography>
                  )}
                </TableCell>
                <TableCell align="right">{item.quantity}</TableCell>
                <TableCell align="right">
                  <TextField
                    type="number"
                    size="small"
                    value={sel?.returnQuantity ?? item.quantity}
                    onChange={e =>
                      handleQtyChange(item.id, Number(e.target.value), item.quantity)
                    }
                    disabled={!isSelected}
                    inputProps={{ min: 1, max: item.quantity }}
                    sx={{ width: 70 }}
                  />
                </TableCell>
                <TableCell align="right">{fmt(item.unitPrice)}</TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </Box>
  );

  const renderStep1 = () => (
    <Box>
      <Typography variant="body2" color="text.secondary" mb={2}>
        Please select the reason for this return.
      </Typography>
      <FormControl fullWidth sx={{ mb: 2 }}>
        <InputLabel>Return Reason</InputLabel>
        <Select
          value={reason}
          label="Return Reason"
          onChange={e => setReason(Number(e.target.value))}
        >
          {REASON_OPTIONS.map(opt => (
            <MenuItem key={opt.value} value={opt.value}>
              {opt.label}
            </MenuItem>
          ))}
        </Select>
      </FormControl>
      <TextField
        fullWidth
        multiline
        rows={3}
        label="Additional Description (optional)"
        value={reasonDescription}
        onChange={e => setReasonDescription(e.target.value)}
        placeholder="Provide any additional details about the return reason…"
      />
    </Box>
  );

  const renderStep2 = () => (
    <Box>
      <Typography variant="body2" color="text.secondary" mb={2}>
        Choose how you would like to receive your refund.
      </Typography>
      <RadioGroup
        value={refundMethod}
        onChange={e => setRefundMethod(e.target.value)}
        sx={{ mb: 2 }}
      >
        {REFUND_METHODS.map(m => (
          <FormControlLabel key={m.value} value={m.value} control={<Radio />} label={m.label} />
        ))}
      </RadioGroup>
      <TextField
        fullWidth
        multiline
        rows={2}
        label="Notes (optional)"
        value={notes}
        onChange={e => setNotes(e.target.value)}
        placeholder="Any additional notes for the returns team…"
      />
    </Box>
  );

  const renderStep3 = () => (
    <Box>
      <Typography variant="body2" color="text.secondary" mb={2}>
        Please review your return request before submitting.
      </Typography>

      <Stack spacing={1} mb={2}>
        <Stack direction="row" justifyContent="space-between">
          <Typography variant="body2" color="text.secondary">Order</Typography>
          <Typography variant="body2">{orderNumber ?? `#${orderId}`}</Typography>
        </Stack>
        <Stack direction="row" justifyContent="space-between">
          <Typography variant="body2" color="text.secondary">Reason</Typography>
          <Typography variant="body2">
            {REASON_OPTIONS.find(o => o.value === reason)?.label ?? 'Unknown'}
          </Typography>
        </Stack>
        <Stack direction="row" justifyContent="space-between">
          <Typography variant="body2" color="text.secondary">Refund Method</Typography>
          <Typography variant="body2">
            {REFUND_METHODS.find(m => m.value === refundMethod)?.label ?? refundMethod}
          </Typography>
        </Stack>
      </Stack>

      <Divider sx={{ mb: 2 }} />

      <Typography variant="subtitle2" gutterBottom>Items to Return:</Typography>
      {returnItems.map(item => (
        <Stack key={item.id} direction="row" justifyContent="space-between" py={0.5}>
          <Typography variant="body2">
            {item.productName ?? `Product #${item.productId}`}
            <Chip
              label={`×${selectedItems[item.id]?.returnQuantity ?? 1}`}
              size="small"
              sx={{ ml: 0.5 }}
            />
          </Typography>
          <Typography variant="body2">
            {fmt(item.unitPrice * (selectedItems[item.id]?.returnQuantity ?? 1))}
          </Typography>
        </Stack>
      ))}

      <Divider sx={{ my: 1 }} />
      <Stack direction="row" justifyContent="space-between">
        <Typography variant="subtitle2">Estimated Refund</Typography>
        <Typography variant="subtitle2" color="success.main" fontWeight="bold">
          {fmt(estimatedRefund)}
        </Typography>
      </Stack>

      {submitError && (
        <Alert severity="error" sx={{ mt: 2 }}>
          {submitError}
        </Alert>
      )}
    </Box>
  );

  const renderStepContent = () => {
    switch (activeStep) {
      case 0: return renderStep0();
      case 1: return renderStep1();
      case 2: return renderStep2();
      case 3: return renderStep3();
      default: return null;
    }
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="md" fullWidth>
      <DialogTitle>
        <Stack direction="row" alignItems="center" gap={1}>
          <ReturnIcon color="secondary" />
          <Typography variant="h6">
            Return Items — {orderNumber ?? `Order #${orderId}`}
          </Typography>
        </Stack>
      </DialogTitle>

      <DialogContent dividers>
        <Stepper activeStep={activeStep} sx={{ mb: 3 }}>
          {STEPS.map(label => (
            <Step key={label}>
              <StepLabel>{label}</StepLabel>
            </Step>
          ))}
        </Stepper>

        {renderStepContent()}
      </DialogContent>

      <DialogActions>
        <Button onClick={handleClose} disabled={submitting}>
          Cancel
        </Button>
        <Box flex={1} />
        <Button
          startIcon={<BackIcon />}
          onClick={handleBack}
          disabled={activeStep === 0 || submitting}
        >
          Back
        </Button>
        {activeStep < STEPS.length - 1 ? (
          <Button
            variant="contained"
            endIcon={<NextIcon />}
            onClick={handleNext}
            disabled={!canGoNext()}
          >
            Next
          </Button>
        ) : (
          <Button
            variant="contained"
            color="secondary"
            onClick={handleSubmit}
            disabled={submitting}
            startIcon={
              submitting ? <CircularProgress size={18} color="inherit" /> : <ReturnIcon />
            }
          >
            Submit Return
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
};

export default OrderReturnDialog;

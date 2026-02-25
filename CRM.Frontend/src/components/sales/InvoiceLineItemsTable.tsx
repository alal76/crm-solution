/**
 * InvoiceLineItemsTable - Editable table for invoice line items
 * with auto-calculation and grand total.
 */

import React, { useCallback } from 'react';
import {
  Box,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  TableContainer,
  Paper,
  TextField,
  IconButton,
  Button,
  Typography,
} from '@mui/material';
import { Add as AddIcon, Delete as DeleteIcon } from '@mui/icons-material';

export interface InvoiceLineItem {
  id?: number;
  description: string;
  quantity: number;
  unitPrice: number;
  discount?: number;
  taxRate?: number;
  total: number;
}

interface InvoiceLineItemsTableProps {
  items: InvoiceLineItem[];
  onChange: (items: InvoiceLineItem[]) => void;
  readOnly?: boolean;
  currency?: string;
}

const calcLineTotal = (item: InvoiceLineItem): number => {
  const subtotal = item.quantity * item.unitPrice;
  const discountAmt = subtotal * ((item.discount ?? 0) / 100);
  const afterDiscount = subtotal - discountAmt;
  const taxAmt = afterDiscount * ((item.taxRate ?? 0) / 100);
  return Math.round((afterDiscount + taxAmt) * 100) / 100;
};

const emptyItem = (): InvoiceLineItem => ({
  description: '',
  quantity: 1,
  unitPrice: 0,
  discount: 0,
  taxRate: 0,
  total: 0,
});

const formatCurrency = (value: number, currency?: string | null) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: currency || 'USD' }).format(value);

const InvoiceLineItemsTable: React.FC<InvoiceLineItemsTableProps> = ({
  items,
  onChange,
  readOnly = false,
  currency = 'USD',
}) => {
  const updateItem = useCallback(
    (index: number, field: keyof InvoiceLineItem, value: string | number) => {
      const updated = items.map((item, i) => {
        if (i !== index) return item;
        const patched = { ...item, [field]: value };
        return { ...patched, total: calcLineTotal(patched) };
      });
      onChange(updated);
    },
    [items, onChange],
  );

  const addRow = useCallback(() => {
    onChange([...items, emptyItem()]);
  }, [items, onChange]);

  const removeRow = useCallback(
    (index: number) => {
      onChange(items.filter((_, i) => i !== index));
    },
    [items, onChange],
  );

  const grandTotal = items.reduce((sum, item) => sum + item.total, 0);

  return (
    <Box>
      <TableContainer component={Paper} variant="outlined">
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell sx={{ fontWeight: 600 }}>Description</TableCell>
              <TableCell sx={{ fontWeight: 600, width: 80 }} align="right">Qty</TableCell>
              <TableCell sx={{ fontWeight: 600, width: 110 }} align="right">Unit Price</TableCell>
              <TableCell sx={{ fontWeight: 600, width: 90 }} align="right">Discount%</TableCell>
              <TableCell sx={{ fontWeight: 600, width: 90 }} align="right">Tax%</TableCell>
              <TableCell sx={{ fontWeight: 600, width: 120 }} align="right">Total</TableCell>
              {!readOnly && <TableCell sx={{ width: 50 }} />}
            </TableRow>
          </TableHead>
          <TableBody>
            {items.map((item, idx) => (
              <TableRow key={item.id ?? idx}>
                <TableCell>
                  {readOnly ? (
                    item.description
                  ) : (
                    <TextField
                      size="small"
                      fullWidth
                      value={item.description}
                      onChange={(e) => updateItem(idx, 'description', e.target.value)}
                      placeholder="Item description"
                    />
                  )}
                </TableCell>
                <TableCell align="right">
                  {readOnly ? item.quantity : (
                    <TextField size="small" type="number" value={item.quantity}
                      inputProps={{ min: 0, step: 1 }} sx={{ width: 70 }}
                      onChange={(e) => updateItem(idx, 'quantity', Number(e.target.value))} />
                  )}
                </TableCell>
                <TableCell align="right">
                  {readOnly ? formatCurrency(item.unitPrice, currency) : (
                    <TextField size="small" type="number" value={item.unitPrice}
                      inputProps={{ min: 0, step: 0.01 }} sx={{ width: 100 }}
                      onChange={(e) => updateItem(idx, 'unitPrice', Number(e.target.value))} />
                  )}
                </TableCell>
                <TableCell align="right">
                  {readOnly ? `${item.discount ?? 0}%` : (
                    <TextField size="small" type="number" value={item.discount ?? 0}
                      inputProps={{ min: 0, max: 100, step: 0.5 }} sx={{ width: 80 }}
                      onChange={(e) => updateItem(idx, 'discount', Number(e.target.value))} />
                  )}
                </TableCell>
                <TableCell align="right">
                  {readOnly ? `${item.taxRate ?? 0}%` : (
                    <TextField size="small" type="number" value={item.taxRate ?? 0}
                      inputProps={{ min: 0, max: 100, step: 0.5 }} sx={{ width: 80 }}
                      onChange={(e) => updateItem(idx, 'taxRate', Number(e.target.value))} />
                  )}
                </TableCell>
                <TableCell align="right">
                  <Typography variant="body2" fontWeight={500}>
                    {formatCurrency(item.total, currency)}
                  </Typography>
                </TableCell>
                {!readOnly && (
                  <TableCell>
                    <IconButton size="small" color="error" onClick={() => removeRow(idx)}>
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </TableCell>
                )}
              </TableRow>
            ))}

            {/* Grand Total Row */}
            <TableRow>
              <TableCell colSpan={readOnly ? 5 : 5} align="right">
                <Typography variant="subtitle1" fontWeight={700}>Grand Total</Typography>
              </TableCell>
              <TableCell align="right">
                <Typography variant="subtitle1" fontWeight={700}>
                  {formatCurrency(grandTotal, currency)}
                </Typography>
              </TableCell>
              {!readOnly && <TableCell />}
            </TableRow>
          </TableBody>
        </Table>
      </TableContainer>

      {!readOnly && (
        <Box mt={1}>
          <Button startIcon={<AddIcon />} size="small" onClick={addRow}>
            Add Line Item
          </Button>
        </Box>
      )}
    </Box>
  );
};

export default InvoiceLineItemsTable;

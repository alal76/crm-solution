/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * BundleItemSelector — searchable, filterable list of bundle items with
 * checkbox selection and quantity controls.
 *
 * Usage:
 *   <BundleItemSelector items={items} selected={selected} onChange={setSelected} />
 */

import React, { useState, useMemo } from 'react';
import {
  Box,
  Typography,
  Checkbox,
  TextField,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  Chip,
  IconButton,
  Tooltip,
  InputAdornment,
  CircularProgress,
  Alert,
} from '@mui/material';
import {
  Search as SearchIcon,
  Add as AddIcon,
  Remove as RemoveIcon,
  Lock as LockIcon,
} from '@mui/icons-material';

// ─── Types ──────────────────────────────────────────────────────────────────

export interface BundleItem {
  id: number;
  productId: number;
  productName?: string;
  itemType: number; // BundleItemType: 0=Required, 1=Optional, 2=Default, 3=Exclusive
  defaultQuantity: number;
  minQuantity: number;
  maxQuantity?: number;
  overridePrice?: number;
  discountPercent?: number;
  isFree: boolean;
  isDefaultSelected: boolean;
  allowQuantityChange: boolean;
  allowRemoval: boolean;
  exclusiveGroup?: string;
  unitPrice?: number; // from joined product
}

export interface BundleItemSelection {
  selected: boolean;
  quantity: number;
}

export type BundleSelectionMap = Record<number, BundleItemSelection>;

interface BundleItemSelectorProps {
  items: BundleItem[];
  selection: BundleSelectionMap;
  onChange: (updated: BundleSelectionMap) => void;
  loading?: boolean;
  error?: string;
}

// ─── Helpers ────────────────────────────────────────────────────────────────

const ITEM_TYPE_LABELS: Record<number, string> = {
  0: 'Required',
  1: 'Optional',
  2: 'Default',
  3: 'Exclusive',
};

const ITEM_TYPE_COLORS: Record<number, 'error' | 'primary' | 'default' | 'warning'> = {
  0: 'error',
  1: 'primary',
  2: 'default',
  3: 'warning',
};

// ─── Component ──────────────────────────────────────────────────────────────

const BundleItemSelector: React.FC<BundleItemSelectorProps> = ({
  items,
  selection,
  onChange,
  loading = false,
  error,
}) => {
  const [search, setSearch] = useState('');

  const filtered = useMemo(
    () =>
      items.filter(item =>
        (item.productName ?? `Product #${item.productId}`)
          .toLowerCase()
          .includes(search.toLowerCase())
      ),
    [items, search]
  );

  const handleCheckbox = (item: BundleItem, checked: boolean) => {
    if (item.itemType === 0) return; // Required items are always selected
    onChange({
      ...selection,
      [item.id]: {
        selected: checked,
        quantity: checked
          ? (selection[item.id]?.quantity ?? item.defaultQuantity)
          : 0,
      },
    });
  };

  const handleQuantityChange = (itemId: number, delta: number, item: BundleItem) => {
    const current = selection[itemId]?.quantity ?? item.defaultQuantity;
    const next = Math.max(
      item.minQuantity,
      Math.min(item.maxQuantity ?? 999, current + delta)
    );
    onChange({
      ...selection,
      [itemId]: { ...selection[itemId], quantity: next, selected: true },
    });
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" py={4}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error">{error}</Alert>;
  }

  if (items.length === 0) {
    return (
      <Typography color="text.secondary" textAlign="center" py={4}>
        No items in this bundle.
      </Typography>
    );
  }

  return (
    <Box>
      <TextField
        size="small"
        placeholder="Search items…"
        value={search}
        onChange={e => setSearch(e.target.value)}
        fullWidth
        sx={{ mb: 2 }}
        InputProps={{
          startAdornment: (
            <InputAdornment position="start">
              <SearchIcon fontSize="small" />
            </InputAdornment>
          ),
        }}
      />

      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell padding="checkbox" />
            <TableCell>Product</TableCell>
            <TableCell>Type</TableCell>
            <TableCell align="right">Unit Price</TableCell>
            <TableCell align="center">Quantity</TableCell>
            <TableCell align="right">Line Total</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {filtered.map(item => {
            const isRequired = item.itemType === 0;
            const sel = selection[item.id];
            const isSelected = isRequired || (sel?.selected ?? item.isDefaultSelected);
            const qty = sel?.quantity ?? item.defaultQuantity;
            const unitPrice = item.isFree ? 0 : (item.overridePrice ?? item.unitPrice ?? 0);
            const lineTotal = unitPrice * qty;

            return (
              <TableRow
                key={item.id}
                sx={{ opacity: isSelected ? 1 : 0.5 }}
              >
                <TableCell padding="checkbox">
                  {isRequired ? (
                    <Tooltip title="Required — cannot be removed">
                      <span>
                        <Checkbox checked disabled />
                      </span>
                    </Tooltip>
                  ) : (
                    <Checkbox
                      checked={isSelected}
                      onChange={e => handleCheckbox(item, e.target.checked)}
                      disabled={!item.allowRemoval && isSelected}
                    />
                  )}
                </TableCell>

                <TableCell>
                  <Box display="flex" alignItems="center" gap={0.5}>
                    {isRequired && (
                      <Tooltip title="Required item">
                        <LockIcon fontSize="inherit" color="action" />
                      </Tooltip>
                    )}
                    <Typography variant="body2">
                      {item.productName ?? `Product #${item.productId}`}
                    </Typography>
                    {item.isFree && (
                      <Chip label="FREE" size="small" color="success" />
                    )}
                    {item.discountPercent && item.discountPercent > 0 && (
                      <Chip
                        label={`-${item.discountPercent}%`}
                        size="small"
                        color="warning"
                      />
                    )}
                  </Box>
                </TableCell>

                <TableCell>
                  <Chip
                    label={ITEM_TYPE_LABELS[item.itemType] ?? 'Unknown'}
                    size="small"
                    color={ITEM_TYPE_COLORS[item.itemType] ?? 'default'}
                  />
                </TableCell>

                <TableCell align="right">
                  {item.isFree
                    ? <Typography variant="body2" color="success.main">Free</Typography>
                    : <Typography variant="body2">
                        {new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })
                          .format(unitPrice)}
                      </Typography>
                  }
                </TableCell>

                <TableCell align="center">
                  {item.allowQuantityChange ? (
                    <Box display="flex" alignItems="center" justifyContent="center" gap={0.5}>
                      <IconButton
                        size="small"
                        onClick={() => handleQuantityChange(item.id, -1, item)}
                        disabled={!isSelected || qty <= item.minQuantity}
                      >
                        <RemoveIcon fontSize="small" />
                      </IconButton>
                      <Typography variant="body2" minWidth={24} textAlign="center">
                        {qty}
                      </Typography>
                      <IconButton
                        size="small"
                        onClick={() => handleQuantityChange(item.id, 1, item)}
                        disabled={!isSelected || (!!item.maxQuantity && qty >= item.maxQuantity)}
                      >
                        <AddIcon fontSize="small" />
                      </IconButton>
                    </Box>
                  ) : (
                    <Typography variant="body2">{qty}</Typography>
                  )}
                </TableCell>

                <TableCell align="right">
                  <Typography variant="body2" fontWeight="medium">
                    {new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })
                      .format(lineTotal)}
                  </Typography>
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </Box>
  );
};

export default BundleItemSelector;

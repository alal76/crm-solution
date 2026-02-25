/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * PriceSummaryPanel — displays the price breakdown returned by
 * POST /api/pricingrules/calculate including base price, discounts
 * and totals.
 */

import React from 'react';
import {
  Box,
  Typography,
  Divider,
  Chip,
  Stack,
  CircularProgress,
  Paper,
  List,
  ListItem,
  ListItemText,
} from '@mui/material';
import {
  CheckCircle as CheckIcon,
  LocalOffer as TagIcon,
} from '@mui/icons-material';

// ─── Types ──────────────────────────────────────────────────────────────────

export interface AppliedRuleSummary {
  ruleId: number;
  ruleName: string;
  ruleType: string;
  discountAmount: number;
  description: string;
}

export interface PriceBreakdown {
  productId: number;
  quantity: number;
  basePrice: number;
  unitPrice: number;
  discountAmount: number;
  discountPercent: number;
  discountType: string;
  finalPrice: number;
  extendedPrice: number;
  currency: string;
  promoCodeApplied?: string;
  appliedRules: AppliedRuleSummary[];
}

interface PriceSummaryPanelProps {
  breakdown: PriceBreakdown | null;
  quantity?: number;
  /** Estimated tax rate 0–1 (e.g. 0.08 for 8%). Defaults to 0 (no estimate). */
  taxRate?: number;
  loading?: boolean;
}

// ─── Helpers ────────────────────────────────────────────────────────────────

const fmt = (value: number, currency = 'USD') =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(value);

// ─── Component ──────────────────────────────────────────────────────────────

const PriceSummaryPanel: React.FC<PriceSummaryPanelProps> = ({
  breakdown,
  quantity,
  taxRate = 0,
  loading = false,
}) => {
  if (loading) {
    return (
      <Box display="flex" justifyContent="center" py={3}>
        <CircularProgress size={28} />
      </Box>
    );
  }

  if (!breakdown) {
    return (
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography color="text.secondary" variant="body2" textAlign="center">
          Select a product and quantity to see the price summary.
        </Typography>
      </Paper>
    );
  }

  const qty = quantity ?? breakdown.quantity;
  const subtotal = breakdown.finalPrice * qty;
  const taxAmount = subtotal * taxRate;
  const total = subtotal + taxAmount;
  const currency = breakdown.currency ?? 'USD';

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Typography variant="subtitle1" fontWeight="bold" gutterBottom>
        Price Summary
      </Typography>

      {/* Base price row */}
      <Stack direction="row" justifyContent="space-between" mb={0.5}>
        <Typography variant="body2" color="text.secondary">
          Base price (per unit)
        </Typography>
        <Typography variant="body2">{fmt(breakdown.basePrice, currency)}</Typography>
      </Stack>

      {/* Discounts */}
      {breakdown.discountAmount > 0 && (
        <Stack direction="row" justifyContent="space-between" mb={0.5}>
          <Typography variant="body2" color="success.main">
            Discount ({breakdown.discountPercent.toFixed(1)}%)
          </Typography>
          <Typography variant="body2" color="success.main">
            -{fmt(breakdown.discountAmount, currency)}
          </Typography>
        </Stack>
      )}

      {/* Unit price after discount */}
      <Stack direction="row" justifyContent="space-between" mb={0.5}>
        <Typography variant="body2">Unit price</Typography>
        <Typography variant="body2" fontWeight="medium">
          {fmt(breakdown.finalPrice, currency)}
        </Typography>
      </Stack>

      {/* Quantity × price */}
      {qty > 1 && (
        <Stack direction="row" justifyContent="space-between" mb={0.5}>
          <Typography variant="body2" color="text.secondary">
            × {qty} units
          </Typography>
          <Typography variant="body2">{fmt(subtotal, currency)}</Typography>
        </Stack>
      )}

      <Divider sx={{ my: 1 }} />

      {/* Subtotal */}
      <Stack direction="row" justifyContent="space-between" mb={0.5}>
        <Typography variant="body2">Subtotal</Typography>
        <Typography variant="body2">{fmt(subtotal, currency)}</Typography>
      </Stack>

      {/* Tax estimate */}
      {taxRate > 0 && (
        <Stack direction="row" justifyContent="space-between" mb={0.5}>
          <Typography variant="body2" color="text.secondary">
            Tax estimate ({(taxRate * 100).toFixed(0)}%)
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {fmt(taxAmount, currency)}
          </Typography>
        </Stack>
      )}

      <Divider sx={{ my: 1 }} />

      {/* Total */}
      <Stack direction="row" justifyContent="space-between" mb={1}>
        <Typography variant="subtitle2" fontWeight="bold">Total</Typography>
        <Typography variant="subtitle2" fontWeight="bold" color="primary.main">
          {fmt(total, currency)}
        </Typography>
      </Stack>

      {/* Promo code badge */}
      {breakdown.promoCodeApplied && (
        <Box display="flex" alignItems="center" gap={0.5} mb={1}>
          <CheckIcon fontSize="small" color="success" />
          <Chip
            icon={<TagIcon />}
            label={`Promo: ${breakdown.promoCodeApplied}`}
            size="small"
            color="success"
            variant="outlined"
          />
        </Box>
      )}

      {/* Applied rules detail */}
      {breakdown.appliedRules.length > 0 && (
        <Box mt={1}>
          <Typography variant="caption" color="text.secondary" display="block" gutterBottom>
            Applied pricing rules:
          </Typography>
          <List dense disablePadding>
            {breakdown.appliedRules.map(rule => (
              <ListItem key={rule.ruleId} disablePadding sx={{ pl: 0 }}>
                <ListItemText
                  primary={
                    <Typography variant="caption">
                      {rule.ruleName}
                      <Typography component="span" variant="caption" color="success.main" ml={0.5}>
                        (-{fmt(rule.discountAmount, currency)})
                      </Typography>
                    </Typography>
                  }
                />
              </ListItem>
            ))}
          </List>
        </Box>
      )}
    </Paper>
  );
};

export default PriceSummaryPanel;

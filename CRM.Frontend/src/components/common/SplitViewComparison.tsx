/**
 * SplitViewComparison - Side-by-side record comparison with diff highlighting
 * TODO-UX-13: Compare two records field-by-field
 */

import React, { useMemo } from 'react';
import {
  Box,
  Paper,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Stack,
  Divider,
  IconButton,
  Tooltip,
  useTheme,
  alpha,
} from '@mui/material';
import {
  SwapHoriz as SwapIcon,
  CompareArrows as CompareIcon,
  Close as CloseIcon,
} from '@mui/icons-material';

// --------------------------------------------------------------------------
// Types
// --------------------------------------------------------------------------

export interface FieldDefinition {
  key: string;
  label: string;
  format?: (value: unknown) => string;
}

export interface SplitViewComparisonProps<T extends Record<string, unknown>> {
  /** Left record */
  leftRecord: T;
  /** Right record */
  rightRecord: T;
  /** Fields to compare */
  fields: FieldDefinition[];
  /** Labels */
  leftLabel?: string;
  rightLabel?: string;
  /** Only show fields that differ */
  showDifferencesOnly?: boolean;
  /** Allow swapping left/right */
  swappable?: boolean;
  /** Callbacks */
  onSwap?: () => void;
  onClose?: () => void;
  /** ARIA */
  ariaLabel?: string;
}

// --------------------------------------------------------------------------
// Helpers
// --------------------------------------------------------------------------

function formatValue(value: unknown, format?: (v: unknown) => string): string {
  if (format) return format(value);
  if (value === null || value === undefined) return '—';
  if (value instanceof Date) return value.toLocaleDateString();
  if (typeof value === 'boolean') return value ? 'Yes' : 'No';
  if (typeof value === 'object') return JSON.stringify(value);
  return String(value);
}

function valuesEqual(a: unknown, b: unknown): boolean {
  if (a === b) return true;
  if (a == null && b == null) return true;
  if (typeof a === 'object' && typeof b === 'object') {
    return JSON.stringify(a) === JSON.stringify(b);
  }
  return false;
}

// --------------------------------------------------------------------------
// Component
// --------------------------------------------------------------------------

export function SplitViewComparison<T extends Record<string, unknown>>({
  leftRecord,
  rightRecord,
  fields,
  leftLabel = 'Record A',
  rightLabel = 'Record B',
  showDifferencesOnly = false,
  swappable = true,
  onSwap,
  onClose,
  ariaLabel = 'Side-by-side record comparison',
}: SplitViewComparisonProps<T>): React.ReactElement {
  const theme = useTheme();

  // Compute diffs
  const fieldRows = useMemo(() => {
    return fields
      .map((f) => {
        const leftVal = leftRecord[f.key];
        const rightVal = rightRecord[f.key];
        const isDifferent = !valuesEqual(leftVal, rightVal);
        return { ...f, leftVal, rightVal, isDifferent };
      })
      .filter((f) => (showDifferencesOnly ? f.isDifferent : true));
  }, [fields, leftRecord, rightRecord, showDifferencesOnly]);

  const diffCount = fieldRows.filter((f) => f.isDifferent).length;

  return (
    <Paper
      sx={{ p: 2 }}
      role="region"
      aria-label={ariaLabel}
    >
      {/* Header */}
      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
        <Stack direction="row" spacing={1} alignItems="center">
          <CompareIcon color="primary" />
          <Typography variant="h6">Comparison</Typography>
          <Chip
            label={`${diffCount} difference${diffCount !== 1 ? 's' : ''}`}
            size="small"
            color={diffCount > 0 ? 'warning' : 'success'}
          />
        </Stack>

        <Stack direction="row" spacing={0.5}>
          {swappable && onSwap && (
            <Tooltip title="Swap left / right">
              <IconButton size="small" onClick={onSwap} aria-label="Swap records">
                <SwapIcon />
              </IconButton>
            </Tooltip>
          )}
          {onClose && (
            <Tooltip title="Close comparison">
              <IconButton size="small" onClick={onClose} aria-label="Close comparison">
                <CloseIcon />
              </IconButton>
            </Tooltip>
          )}
        </Stack>
      </Stack>

      <Divider sx={{ mb: 2 }} />

      {/* Comparison table */}
      <TableContainer>
        <Table size="small" aria-label="Field comparison table">
          <TableHead>
            <TableRow>
              <TableCell sx={{ fontWeight: 600, width: '25%' }}>Field</TableCell>
              <TableCell sx={{ fontWeight: 600, width: '35%' }}>{leftLabel}</TableCell>
              <TableCell sx={{ fontWeight: 600, width: '35%' }}>{rightLabel}</TableCell>
              <TableCell sx={{ fontWeight: 600, width: '5%', textAlign: 'center' }}>Match</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {fieldRows.length === 0 ? (
              <TableRow>
                <TableCell colSpan={4} align="center">
                  <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
                    {showDifferencesOnly
                      ? 'All fields match — no differences found.'
                      : 'No fields to compare.'}
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              fieldRows.map((f) => (
                <TableRow
                  key={f.key}
                  sx={{
                    backgroundColor: f.isDifferent
                      ? alpha(theme.palette.warning.main, 0.08)
                      : undefined,
                  }}
                >
                  <TableCell sx={{ fontWeight: 500 }}>{f.label}</TableCell>
                  <TableCell
                    sx={{
                      color: f.isDifferent ? theme.palette.warning.dark : undefined,
                      fontWeight: f.isDifferent ? 600 : 400,
                    }}
                  >
                    {formatValue(f.leftVal, f.format)}
                  </TableCell>
                  <TableCell
                    sx={{
                      color: f.isDifferent ? theme.palette.warning.dark : undefined,
                      fontWeight: f.isDifferent ? 600 : 400,
                    }}
                  >
                    {formatValue(f.rightVal, f.format)}
                  </TableCell>
                  <TableCell align="center">
                    <Chip
                      label={f.isDifferent ? '✗' : '✓'}
                      size="small"
                      color={f.isDifferent ? 'warning' : 'success'}
                      variant="outlined"
                      sx={{ minWidth: 32 }}
                    />
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Paper>
  );
}

export default SplitViewComparison;

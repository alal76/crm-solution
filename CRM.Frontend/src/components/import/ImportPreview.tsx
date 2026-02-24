/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This software is source-available. Non-commercial use is permitted under
 * the terms of the LICENSE file. Commercial use requires a separate license.
 * See the LICENSE file in the root directory for full terms.
 */

import { useState, useMemo } from 'react';
import {
  Box,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Alert,
  Chip,
  Switch,
  FormControlLabel,
  Tooltip,
  Stack,
} from '@mui/material';
import {
  Warning as WarningIcon,
  ErrorOutline as ErrorIcon,
} from '@mui/icons-material';
import type { TargetField } from './ColumnMapper';

// ============================================================================
// Types
// ============================================================================

export interface ValidationError {
  row: number;
  field: string;
  message: string;
}

export interface ImportPreviewProps {
  data: Record<string, unknown>[];
  mapping: Record<string, string>;
  targetFields: TargetField[];
  maxRows?: number;
  validationErrors?: ValidationError[];
}

// ============================================================================
// Component
// ============================================================================

export default function ImportPreview({
  data,
  mapping,
  targetFields,
  maxRows = 10,
  validationErrors = [],
}: ImportPreviewProps) {
  const [showErrorsOnly, setShowErrorsOnly] = useState(false);

  // Derive the columns to display (mapped columns only, in target field order)
  const displayColumns = useMemo(() => {
    const mappedTargetKeys = new Set(Object.values(mapping).filter(Boolean));
    return targetFields.filter((f) => mappedTargetKeys.has(f.key));
  }, [targetFields, mapping]);

  // Build a reverse map: targetKey → sourceColumn
  const reverseMapping = useMemo(() => {
    const rev: Record<string, string> = {};
    for (const [src, tgt] of Object.entries(mapping)) {
      if (tgt) rev[tgt] = src;
    }
    return rev;
  }, [mapping]);

  // Index validation errors by row number for fast lookup
  const errorsByRow = useMemo(() => {
    const index: Record<number, ValidationError[]> = {};
    for (const err of validationErrors) {
      if (!index[err.row]) index[err.row] = [];
      index[err.row].push(err);
    }
    return index;
  }, [validationErrors]);

  const rowsWithErrors = new Set(validationErrors.map((e) => e.row));

  // Apply maxRows and optional filter
  const displayRows = useMemo(() => {
    const sliced = data.slice(0, maxRows);
    if (showErrorsOnly) {
      return sliced.filter((_, idx) => rowsWithErrors.has(idx + 1));
    }
    return sliced;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [data, maxRows, showErrorsOnly, rowsWithErrors]);

  const totalErrorRows = useMemo(
    () => new Set(validationErrors.map((e) => e.row)).size,
    [validationErrors],
  );

  if (displayColumns.length === 0) {
    return (
      <Alert severity="info">
        No columns are mapped yet. Go back to the mapping step to continue.
      </Alert>
    );
  }

  return (
    <Box>
      {/* Header */}
      <Stack
        direction="row"
        alignItems="center"
        justifyContent="space-between"
        sx={{ mb: 2 }}
      >
        <Box>
          <Typography variant="subtitle1" fontWeight={600}>
            Data Preview
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Showing first {Math.min(data.length, maxRows)} of {data.length} rows
          </Typography>
        </Box>
        {validationErrors.length > 0 && (
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={showErrorsOnly}
                onChange={(_, checked) => setShowErrorsOnly(checked)}
              />
            }
            label={
              <Typography variant="body2">Show errors only</Typography>
            }
          />
        )}
      </Stack>

      {/* Error summary bar */}
      {totalErrorRows > 0 && (
        <Alert
          severity="warning"
          icon={<WarningIcon />}
          sx={{ mb: 2 }}
        >
          <Typography variant="body2">
            <strong>{totalErrorRows}</strong> row
            {totalErrorRows !== 1 ? 's have' : ' has'} validation errors
            ({validationErrors.length} issue
            {validationErrors.length !== 1 ? 's' : ''} total)
          </Typography>
        </Alert>
      )}

      {/* Preview table */}
      <TableContainer component={Paper} variant="outlined" sx={{ maxHeight: 480, overflow: 'auto' }}>
        <Table size="small" stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell
                sx={{ fontWeight: 600, bgcolor: 'grey.50', width: 48, color: 'text.secondary' }}
              >
                #
              </TableCell>
              {displayColumns.map((field) => (
                <TableCell
                  key={field.key}
                  sx={{ fontWeight: 600, bgcolor: 'grey.50' }}
                >
                  <Stack direction="row" alignItems="center" spacing={0.5}>
                    <span>{field.label}</span>
                    {field.required && (
                      <Chip
                        label="req"
                        size="small"
                        color="error"
                        variant="outlined"
                        sx={{ height: 16, fontSize: 10 }}
                      />
                    )}
                  </Stack>
                </TableCell>
              ))}
              <TableCell sx={{ fontWeight: 600, bgcolor: 'grey.50', width: 80 }}>
                Status
              </TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {displayRows.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={displayColumns.length + 2}
                  align="center"
                  sx={{ py: 4, color: 'text.secondary' }}
                >
                  {showErrorsOnly ? 'No rows with errors in preview' : 'No data to preview'}
                </TableCell>
              </TableRow>
            ) : (
              displayRows.map((row, idx) => {
                // Calculate original row number (1-based)
                const rowNum = showErrorsOnly
                  ? ([] as number[]).concat(...Object.keys(errorsByRow).map(Number))[idx] ?? idx + 1
                  : idx + 1;
                const rowErrors = errorsByRow[rowNum] ?? [];
                const hasError = rowErrors.length > 0;

                return (
                  <TableRow
                    key={rowNum}
                    sx={{
                      ...(hasError
                        ? { bgcolor: 'error.50', '&:hover': { bgcolor: 'error.100' } }
                        : { '&:hover': { bgcolor: 'action.hover' } }),
                    }}
                  >
                    {/* Row number */}
                    <TableCell sx={{ color: 'text.secondary', fontVariantNumeric: 'tabular-nums' }}>
                      {rowNum}
                    </TableCell>

                    {/* Data cells */}
                    {displayColumns.map((field) => {
                      const sourceCol = reverseMapping[field.key];
                      const cellValue =
                        sourceCol !== undefined ? String(row[sourceCol] ?? '') : '';
                      const fieldErrors = rowErrors.filter((e) => e.field === field.key);

                      return (
                        <TableCell key={field.key}>
                          {fieldErrors.length > 0 ? (
                            <Tooltip
                              title={
                                <Box>
                                  {fieldErrors.map((e, i) => (
                                    <Typography key={i} variant="caption" display="block">
                                      {e.message}
                                    </Typography>
                                  ))}
                                </Box>
                              }
                            >
                              <Stack direction="row" alignItems="center" spacing={0.5}>
                                <ErrorIcon
                                  fontSize="small"
                                  color="error"
                                  sx={{ flexShrink: 0 }}
                                />
                                <Typography
                                  variant="body2"
                                  color="error.main"
                                  sx={{
                                    maxWidth: 160,
                                    overflow: 'hidden',
                                    textOverflow: 'ellipsis',
                                    whiteSpace: 'nowrap',
                                  }}
                                >
                                  {cellValue || <em>empty</em>}
                                </Typography>
                              </Stack>
                            </Tooltip>
                          ) : (
                            <Typography
                              variant="body2"
                              sx={{
                                maxWidth: 200,
                                overflow: 'hidden',
                                textOverflow: 'ellipsis',
                                whiteSpace: 'nowrap',
                              }}
                            >
                              {cellValue || (
                                <Typography
                                  component="span"
                                  variant="body2"
                                  color="text.disabled"
                                >
                                  —
                                </Typography>
                              )}
                            </Typography>
                          )}
                        </TableCell>
                      );
                    })}

                    {/* Status cell */}
                    <TableCell>
                      {hasError ? (
                        <Tooltip title={`${rowErrors.length} error(s)`}>
                          <Chip
                            label={`${rowErrors.length} error${rowErrors.length !== 1 ? 's' : ''}`}
                            size="small"
                            color="error"
                            variant="outlined"
                          />
                        </Tooltip>
                      ) : (
                        <Chip label="OK" size="small" color="success" variant="outlined" />
                      )}
                    </TableCell>
                  </TableRow>
                );
              })
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {data.length > maxRows && (
        <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
          Showing {maxRows} of {data.length} total rows. All rows will be imported.
        </Typography>
      )}
    </Box>
  );
}

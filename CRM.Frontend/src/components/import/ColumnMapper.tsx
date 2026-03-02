/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This software is source-available. Non-commercial use is permitted under
 * the terms of the LICENSE file. Commercial use requires a separate license.
 * See the LICENSE file in the root directory for full terms.
 */

import { useMemo } from 'react';
import {
  Box,
  Typography,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Select,
  MenuItem,
  FormControl,
  Chip,
  Tooltip,
  Alert,
  Stack,
} from '@mui/material';
import {
  AutoFixHigh as AutoMapIcon,
  ClearAll as ClearAllIcon,
} from '@mui/icons-material';

// ============================================================================
// Types
// ============================================================================

export interface TargetField {
  key: string;
  label: string;
  required: boolean;
  type: 'string' | 'number' | 'date' | 'boolean' | 'email' | 'phone';
}

export interface ColumnMapperProps {
  sourceColumns: string[];
  targetFields: TargetField[];
  mapping: Record<string, string>;
  onMappingChange: (mapping: Record<string, string>) => void;
  entityType: string;
  sampleRows?: Record<string, string>[];
}

// ============================================================================
// Helpers
// ============================================================================

/**
 * Normalises a column/field label for fuzzy comparison.
 * E.g. "First Name" → "firstname", "email_address" → "emailaddress"
 */
function normalise(value: string): string {
  return value.toLowerCase().replaceAll(/[\s_\-\.]/g, '');
}

function autoMap(
  sourceColumns: string[],
  targetFields: TargetField[],
): Record<string, string> {
  const result: Record<string, string> = {};
  const usedTargets = new Set<string>();

  for (const col of sourceColumns) {
    const normCol = normalise(col);
    const match = targetFields.find(
      (f) => !usedTargets.has(f.key) && normalise(f.label) === normCol,
    );
    if (match) {
      result[col] = match.key;
      usedTargets.add(match.key);
    } else {
      // Second pass: partial match (normCol contains or is contained by the field key)
      const partial = targetFields.find(
        (f) =>
          !usedTargets.has(f.key) &&
          (normalise(f.key) === normCol ||
            normCol.includes(normalise(f.key)) ||
            normalise(f.key).includes(normCol)),
      );
      if (partial) {
        result[col] = partial.key;
        usedTargets.add(partial.key);
      } else {
        result[col] = '';
      }
    }
  }

  return result;
}

// ============================================================================
// Component
// ============================================================================

export default function ColumnMapper({
  sourceColumns,
  targetFields,
  mapping,
  onMappingChange,
  sampleRows = [],
}: ColumnMapperProps) {
  // Completeness stats
  const requiredFields = useMemo(
    () => targetFields.filter((f) => f.required),
    [targetFields],
  );
  const mappedRequired = useMemo(
    () =>
      requiredFields.filter((f) =>
        Object.values(mapping).includes(f.key),
      ),
    [requiredFields, mapping],
  );

  const handleAutoMap = () => {
    onMappingChange(autoMap(sourceColumns, targetFields));
  };

  const handleClearAll = () => {
    const cleared: Record<string, string> = {};
    sourceColumns.forEach((col) => {
      cleared[col] = '';
    });
    onMappingChange(cleared);
  };

  const handleSelectChange = (sourceColumn: string, targetKey: string) => {
    onMappingChange({ ...mapping, [sourceColumn]: targetKey });
  };

  const mappingComplete =
    mappedRequired.length === requiredFields.length && requiredFields.length > 0;

  return (
    <Box>
      {/* Header toolbar */}
      <Stack
        direction="row"
        alignItems="center"
        justifyContent="space-between"
        sx={{ mb: 2 }}
      >
        <Box>
          <Typography variant="subtitle1" fontWeight={600}>
            Map Columns
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Match each CSV column to the corresponding CRM field.
          </Typography>
        </Box>
        <Stack direction="row" spacing={1}>
          <Button
            size="small"
            variant="outlined"
            startIcon={<AutoMapIcon />}
            onClick={handleAutoMap}
          >
            Auto-Map
          </Button>
          <Button
            size="small"
            variant="outlined"
            color="inherit"
            startIcon={<ClearAllIcon />}
            onClick={handleClearAll}
          >
            Clear All
          </Button>
        </Stack>
      </Stack>

      {/* Completeness indicator */}
      <Alert
        severity={mappingComplete ? 'success' : 'warning'}
        sx={{ mb: 2 }}
        icon={false}
      >
        <Typography variant="body2">
          <strong>
            {mappedRequired.length} of {requiredFields.length}
          </strong>{' '}
          required fields mapped
          {!mappingComplete && ' — all required fields must be mapped to continue'}
        </Typography>
      </Alert>

      {/* Mapping table */}
      <TableContainer component={Paper} variant="outlined">
        <Table size="small">
          <TableHead>
            <TableRow sx={{ bgcolor: 'grey.50' }}>
              <TableCell sx={{ fontWeight: 600, width: '35%' }}>
                Source Column
              </TableCell>
              <TableCell sx={{ fontWeight: 600, width: '20%' }}>
                Sample Value
              </TableCell>
              <TableCell sx={{ fontWeight: 600, width: '45%' }}>
                Target CRM Field
              </TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {sourceColumns.map((col) => {
              const currentTarget = mapping[col] ?? '';
              const targetField = targetFields.find(
                (f) => f.key === currentTarget,
              );
              const sampleValue =
                sampleRows.length > 0 ? sampleRows[0][col] ?? '' : '';
              const isUnmappedRequired = false; // source cols are not required themselves

              return (
                <TableRow
                  key={col}
                  sx={{
                    '&:hover': { bgcolor: 'action.hover' },
                    ...(isUnmappedRequired
                      ? { bgcolor: 'error.50' }
                      : undefined),
                  }}
                >
                  {/* Source column */}
                  <TableCell>
                    <Typography variant="body2" fontWeight={500}>
                      {col}
                    </Typography>
                  </TableCell>

                  {/* Sample value */}
                  <TableCell>
                    <Typography
                      variant="body2"
                      color="text.secondary"
                      sx={{
                        maxWidth: 160,
                        overflow: 'hidden',
                        textOverflow: 'ellipsis',
                        whiteSpace: 'nowrap',
                      }}
                    >
                      {sampleValue || <em>—</em>}
                    </Typography>
                  </TableCell>

                  {/* Target field selector */}
                  <TableCell>
                    <FormControl fullWidth size="small">
                      <Select
                        displayEmpty
                        value={currentTarget}
                        onChange={(e) =>
                          handleSelectChange(col, e.target.value)
                        }
                        renderValue={(value) => {
                          if (!value) {
                            return (
                              <Typography
                                variant="body2"
                                color="text.disabled"
                              >
                                — Skip column —
                              </Typography>
                            );
                          }
                          const field = targetFields.find(
                            (f) => f.key === value,
                          );
                          return (
                            <Stack direction="row" alignItems="center" spacing={0.5}>
                              <Typography variant="body2">
                                {field?.label ?? value}
                              </Typography>
                              {field?.required && (
                                <Chip
                                  label="required"
                                  size="small"
                                  color="error"
                                  variant="outlined"
                                  sx={{ height: 16, fontSize: 10 }}
                                />
                              )}
                            </Stack>
                          );
                        }}
                      >
                        <MenuItem value="">
                          <em>— Skip column —</em>
                        </MenuItem>
                        {targetFields.map((field) => {
                          const alreadyMapped =
                            field.key !== currentTarget &&
                            Object.values(mapping).includes(field.key);
                          return (
                            <MenuItem
                              key={field.key}
                              value={field.key}
                              disabled={alreadyMapped}
                            >
                              <Stack
                                direction="row"
                                alignItems="center"
                                spacing={0.5}
                                width="100%"
                              >
                                <Typography variant="body2" flexGrow={1}>
                                  {field.label}
                                </Typography>
                                {field.required && (
                                  <Chip
                                    label="req"
                                    size="small"
                                    color="error"
                                    variant="outlined"
                                    sx={{ height: 16, fontSize: 10 }}
                                  />
                                )}
                                {alreadyMapped && (
                                  <Tooltip title="Already mapped to another column">
                                    <Chip
                                      label="in use"
                                      size="small"
                                      variant="outlined"
                                      sx={{ height: 16, fontSize: 10 }}
                                    />
                                  </Tooltip>
                                )}
                              </Stack>
                            </MenuItem>
                          );
                        })}
                      </Select>
                    </FormControl>
                    {/* Flag if required target field but no mapping */}
                    {targetField?.required === false && !currentTarget && (
                      <Typography
                        variant="caption"
                        color="error.main"
                        display="block"
                        mt={0.25}
                      >
                        Required field not mapped
                      </Typography>
                    )}
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
}

/**
 * InlineEditableGrid - DataGrid with click-to-edit, auto-save on blur, revert on Escape
 * TODO-UX-07: Inline editing for data grid cells
 */

import React, { useState, useCallback, useRef, useEffect, KeyboardEvent } from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  Paper,
  TextField,
  Select,
  MenuItem,
  Checkbox,
  Box,
  Typography,
  Skeleton,
  useTheme,
  alpha,
  Tooltip,
  Snackbar,
  Alert,
} from '@mui/material';
import { Edit as EditIcon, Check as CheckIcon, Close as CloseIcon } from '@mui/icons-material';

// --------------------------------------------------------------------------
// Types
// --------------------------------------------------------------------------

export type CellFieldType = 'string' | 'number' | 'boolean' | 'select' | 'date';

export interface EditableColumn<T> {
  field: keyof T & string;
  headerName: string;
  width?: number | string;
  flex?: number;
  editable?: boolean;
  type?: CellFieldType;
  options?: { value: string | number; label: string }[];
  validate?: (value: unknown, row: T) => string | null;
  renderCell?: (row: T) => React.ReactNode;
}

export interface InlineEditableGridProps<T extends { id: number | string }> {
  rows: T[];
  columns: EditableColumn<T>[];
  loading?: boolean;
  /** Save callback — receives the row id, field name, and new value */
  onSave: (rowId: number | string, field: string, value: unknown) => Promise<void> | void;
  /** Called when save fails */
  onSaveError?: (error: unknown, rowId: number | string, field: string) => void;
  /** Pagination */
  page?: number;
  pageSize?: number;
  totalCount?: number;
  onPageChange?: (page: number) => void;
  onPageSizeChange?: (size: number) => void;
  /** Row click */
  onRowClick?: (row: T) => void;
  /** ARIA */
  ariaLabel?: string;
  /** Show success feedback */
  showSaveToast?: boolean;
  /** Dense mode */
  dense?: boolean;
}

interface EditState {
  rowId: number | string;
  field: string;
  originalValue: unknown;
  currentValue: unknown;
  error: string | null;
}

// --------------------------------------------------------------------------
// Component
// --------------------------------------------------------------------------

export function InlineEditableGrid<T extends { id: number | string }>({
  rows,
  columns,
  loading = false,
  onSave,
  onSaveError,
  page = 0,
  pageSize = 10,
  totalCount,
  onPageChange,
  onPageSizeChange,
  onRowClick,
  ariaLabel = 'Editable data grid',
  showSaveToast = true,
  dense = false,
}: InlineEditableGridProps<T>): React.ReactElement {
  const theme = useTheme();
  const [editState, setEditState] = useState<EditState | null>(null);
  const [saving, setSaving] = useState(false);
  const [toast, setToast] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: false,
    message: '',
    severity: 'success',
  });
  const inputRef = useRef<HTMLInputElement>(null);

  // Focus input when entering edit mode
  useEffect(() => {
    if (editState && inputRef.current) {
      inputRef.current.focus();
      if (inputRef.current.select) {
        inputRef.current.select();
      }
    }
  }, [editState?.rowId, editState?.field]);

  // Start editing a cell
  const startEdit = useCallback(
    (row: T, col: EditableColumn<T>) => {
      if (!col.editable) return;
      const value = row[col.field];
      setEditState({
        rowId: row.id,
        field: col.field,
        originalValue: value,
        currentValue: value,
        error: null,
      });
    },
    [],
  );

  // Commit the edit
  const commitEdit = useCallback(async () => {
    if (!editState) return;
    if (editState.currentValue === editState.originalValue) {
      setEditState(null);
      return;
    }

    // Validate
    const col = columns.find((c) => c.field === editState.field);
    if (col?.validate) {
      const row = rows.find((r) => r.id === editState.rowId);
      if (row) {
        const err = col.validate(editState.currentValue, row);
        if (err) {
          setEditState((s) => (s ? { ...s, error: err } : null));
          return;
        }
      }
    }

    setSaving(true);
    try {
      await onSave(editState.rowId, editState.field, editState.currentValue);
      if (showSaveToast) {
        setToast({ open: true, message: 'Saved', severity: 'success' });
      }
      setEditState(null);
    } catch (error) {
      onSaveError?.(error, editState.rowId, editState.field);
      setToast({ open: true, message: 'Save failed', severity: 'error' });
    } finally {
      setSaving(false);
    }
  }, [editState, columns, rows, onSave, onSaveError, showSaveToast]);

  // Cancel edit and revert
  const cancelEdit = useCallback(() => {
    setEditState(null);
  }, []);

  // Key handler for edit cell
  const handleEditKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (e.key === 'Enter') {
        e.preventDefault();
        commitEdit();
      } else if (e.key === 'Escape') {
        e.preventDefault();
        cancelEdit();
      } else if (e.key === 'Tab') {
        e.preventDefault();
        commitEdit();
      }
    },
    [commitEdit, cancelEdit],
  );

  // Render an edit cell
  const renderEditInput = (col: EditableColumn<T>) => {
    if (!editState) return null;

    const commonProps = {
      size: 'small' as const,
      fullWidth: true,
      onKeyDown: handleEditKeyDown,
      onBlur: commitEdit,
      error: !!editState.error,
      helperText: editState.error,
      disabled: saving,
    };

    switch (col.type) {
      case 'number':
        return (
          <TextField
            {...commonProps}
            inputRef={inputRef}
            type="number"
            value={editState.currentValue ?? ''}
            onChange={(e) =>
              setEditState((s) =>
                s ? { ...s, currentValue: e.target.value ? Number(e.target.value) : null, error: null } : null,
              )
            }
          />
        );

      case 'boolean':
        return (
          <Checkbox
            checked={!!editState.currentValue}
            onChange={(e) =>
              setEditState((s) => (s ? { ...s, currentValue: e.target.checked, error: null } : null))
            }
            onKeyDown={handleEditKeyDown}
            inputRef={inputRef}
          />
        );

      case 'select':
        return (
          <Select
            {...commonProps}
            value={editState.currentValue ?? ''}
            onChange={(e) =>
              setEditState((s) => (s ? { ...s, currentValue: e.target.value, error: null } : null))
            }
            inputRef={inputRef}
          >
            {(col.options ?? []).map((opt) => (
              <MenuItem key={opt.value} value={opt.value}>
                {opt.label}
              </MenuItem>
            ))}
          </Select>
        );

      case 'date':
        return (
          <TextField
            {...commonProps}
            inputRef={inputRef}
            type="date"
            value={editState.currentValue ?? ''}
            onChange={(e) =>
              setEditState((s) => (s ? { ...s, currentValue: e.target.value, error: null } : null))
            }
            InputLabelProps={{ shrink: true }}
          />
        );

      default:
        return (
          <TextField
            {...commonProps}
            inputRef={inputRef}
            value={editState.currentValue ?? ''}
            onChange={(e) =>
              setEditState((s) => (s ? { ...s, currentValue: e.target.value, error: null } : null))
            }
          />
        );
    }
  };

  // Render cell content
  const renderCell = (row: T, col: EditableColumn<T>) => {
    const isEditing = editState?.rowId === row.id && editState?.field === col.field;

    if (isEditing) {
      return (
        <Box sx={{ minWidth: 80 }} onClick={(e) => e.stopPropagation()}>
          {renderEditInput(col)}
        </Box>
      );
    }

    if (col.renderCell) return col.renderCell(row);

    const value = row[col.field];
    if (value === null || value === undefined) return '-';
    if (typeof value === 'boolean') return value ? 'Yes' : 'No';
    return String(value);
  };

  return (
    <Paper>
      <TableContainer>
        <Table
          size={dense ? 'small' : 'medium'}
          aria-label={ariaLabel}
          role="grid"
        >
          <TableHead>
            <TableRow>
              {columns.map((col) => (
                <TableCell
                  key={col.field}
                  sx={{ fontWeight: 600, width: col.width }}
                  role="columnheader"
                >
                  {col.headerName}
                  {col.editable && (
                    <EditIcon fontSize="inherit" sx={{ ml: 0.5, opacity: 0.4, fontSize: '0.85rem' }} />
                  )}
                </TableCell>
              ))}
            </TableRow>
          </TableHead>

          <TableBody>
            {loading ? (
              Array.from({ length: pageSize }).map((_, i) => (
                <TableRow key={`skeleton-${i}`}>
                  {columns.map((col) => (
                    <TableCell key={col.field}>
                      <Skeleton variant="text" />
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : rows.length === 0 ? (
              <TableRow>
                <TableCell colSpan={columns.length} align="center">
                  <Typography variant="body2" color="text.secondary" sx={{ py: 4 }}>
                    No data available
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              rows.map((row) => (
                <TableRow
                  key={row.id}
                  hover
                  onClick={() => onRowClick?.(row)}
                  sx={{
                    cursor: onRowClick ? 'pointer' : 'default',
                    '&:hover .edit-indicator': { opacity: 1 },
                  }}
                  role="row"
                >
                  {columns.map((col) => (
                    <TableCell
                      key={col.field}
                      role="gridcell"
                      onDoubleClick={() => col.editable && startEdit(row, col)}
                      sx={{
                        cursor: col.editable ? 'text' : 'default',
                        '&:hover': col.editable
                          ? { backgroundColor: alpha(theme.palette.primary.main, 0.04) }
                          : undefined,
                      }}
                      aria-label={`${col.headerName}: ${row[col.field]}`}
                    >
                      {renderCell(row, col)}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Pagination */}
      {totalCount !== undefined && onPageChange && (
        <TablePagination
          component="div"
          count={totalCount}
          page={page}
          onPageChange={(_, newPage) => onPageChange(newPage)}
          rowsPerPage={pageSize}
          onRowsPerPageChange={(e) => onPageSizeChange?.(parseInt(e.target.value, 10))}
          rowsPerPageOptions={[10, 25, 50, 100]}
        />
      )}

      {/* Toast feedback */}
      <Snackbar
        open={toast.open}
        autoHideDuration={2000}
        onClose={() => setToast((t) => ({ ...t, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert severity={toast.severity} variant="filled" onClose={() => setToast((t) => ({ ...t, open: false }))}>
          {toast.message}
        </Alert>
      </Snackbar>
    </Paper>
  );
}

export default InlineEditableGrid;

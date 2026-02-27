/**
 * DataGrid - Accessible, keyboard-navigable data grid component
 * Implements WCAG 2.1 AA accessibility requirements
 */

import React, { useState, useRef, useCallback, useEffect, useMemo, KeyboardEvent } from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TableSortLabel,
  TablePagination,
  Paper,
  Checkbox,
  Box,
  Typography,
  IconButton,
  Skeleton,
  useTheme,
} from '@mui/material';
import { visuallyHidden } from '@mui/utils';
import {
  KeyboardArrowUp as ArrowUpIcon,
  KeyboardArrowDown as ArrowDownIcon,
} from '@mui/icons-material';

// Column definition
export interface DataGridColumn<T> {
  field: keyof T | string;
  headerName: string;
  width?: number | string;
  minWidth?: number;
  flex?: number;
  align?: 'left' | 'center' | 'right';
  sortable?: boolean;
  editable?: boolean;
  renderCell?: (row: T, index: number) => React.ReactNode;
  renderEditCell?: (row: T, value: unknown, onChange: (value: unknown) => void) => React.ReactNode;
  valueGetter?: (row: T) => unknown;
  type?: 'string' | 'number' | 'date' | 'boolean' | 'select';
}

// Sort direction
export type SortDirection = 'asc' | 'desc' | undefined;
export type SortOrder = SortDirection;
export interface PaginationInfo {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

// Props
export interface DataGridProps<T extends { id: number | string }> {
  rows: T[];
  columns: DataGridColumn<T>[];
  loading?: boolean;
  // Pagination
  page?: number;
  pageSize?: number;
  totalCount?: number;
  onPageChange?: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
  pageSizeOptions?: number[];
  // Sorting
  sortField?: string;
  sortDirection?: SortDirection;
  onSortChange?: (field: string, direction: SortDirection) => void;
  // Selection
  selectable?: boolean;
  selectedIds?: (number | string)[];
  onSelectionChange?: (ids: (number | string)[]) => void;
  // Editing
  editMode?: 'cell' | 'row' | 'none';
  onCellEdit?: (rowId: number | string, field: string, value: unknown) => void;
  onRowEdit?: (row: T) => void;
  // Row actions
  onRowClick?: (row: T) => void;
  onRowDoubleClick?: (row: T) => void;
  // Accessibility
  ariaLabel?: string;
  ariaDescribedBy?: string;
  // Styling
  stickyHeader?: boolean;
  maxHeight?: number | string;
  rowHeight?: number;
  dense?: boolean;
  striped?: boolean;
  hover?: boolean;
  emptyMessage?: string;
  // Custom rendering
  renderEmptyState?: () => React.ReactNode;
  getRowId?: (row: T) => number | string;
}

// Cell focus state
interface CellFocus {
  rowIndex: number;
  colIndex: number;
}

export function DataGrid<T extends { id: number | string }>({
  rows,
  columns,
  loading = false,
  page = 0,
  pageSize = 10,
  totalCount,
  onPageChange,
  onPageSizeChange,
  pageSizeOptions = [10, 25, 50, 100],
  sortField,
  sortDirection,
  onSortChange,
  selectable = false,
  selectedIds = [],
  onSelectionChange,
  editMode = 'none',
  onCellEdit,
  onRowEdit,
  onRowClick,
  onRowDoubleClick,
  ariaLabel = 'Data grid',
  ariaDescribedBy,
  stickyHeader = false,
  maxHeight,
  rowHeight = 52,
  dense = false,
  striped = false,
  hover = true,
  emptyMessage = 'No data available',
  renderEmptyState,
  getRowId = (row) => row.id,
}: DataGridProps<T>): React.ReactElement {
  const theme = useTheme();
  const tableRef = useRef<HTMLTableElement>(null);
  const [focusedCell, setFocusedCell] = useState<CellFocus | null>(null);
  const [editingCell, setEditingCell] = useState<CellFocus | null>(null);
  const [editValue, setEditValue] = useState<unknown>(null);
  const cellRefs = useRef<Map<string, HTMLTableCellElement>>(new Map());

  // Calculate visible columns with selection column
  const visibleColumns = useMemo(() => {
    if (selectable) {
      return [{ field: '__selection__', headerName: '', width: 50 } as DataGridColumn<T>, ...columns];
    }
    return columns;
  }, [columns, selectable]);

  // Get cell key for ref map
  const getCellKey = (rowIndex: number, colIndex: number) => `${rowIndex}-${colIndex}`;

  // Focus specific cell
  const focusCell = useCallback((rowIndex: number, colIndex: number) => {
    const key = getCellKey(rowIndex, colIndex);
    const cell = cellRefs.current.get(key);
    if (cell) {
      cell.focus();
      setFocusedCell({ rowIndex, colIndex });
    }
  }, []);

  // Handle keyboard navigation
  const handleKeyDown = useCallback((e: KeyboardEvent<HTMLTableElement>) => {
    if (!focusedCell) return;

    const { rowIndex, colIndex } = focusedCell;
    const maxRow = rows.length - 1;
    const maxCol = visibleColumns.length - 1;

    // If editing, handle differently
    if (editingCell) {
      if (e.key === 'Escape') {
        e.preventDefault();
        setEditingCell(null);
        setEditValue(null);
        focusCell(editingCell.rowIndex, editingCell.colIndex);
      } else if (e.key === 'Enter') {
        e.preventDefault();
        // Save edit
        const row = rows[editingCell.rowIndex];
        const col = visibleColumns[editingCell.colIndex];
        if (row && col && col.field !== '__selection__' && onCellEdit) {
          onCellEdit(getRowId(row), String(col.field), editValue);
        }
        setEditingCell(null);
        setEditValue(null);
        focusCell(editingCell.rowIndex, editingCell.colIndex);
      } else if (e.key === 'Tab') {
        e.preventDefault();
        // Save and move to next editable cell
        const row = rows[editingCell.rowIndex];
        const col = visibleColumns[editingCell.colIndex];
        if (row && col && col.field !== '__selection__' && onCellEdit) {
          onCellEdit(getRowId(row), String(col.field), editValue);
        }
        setEditingCell(null);
        setEditValue(null);
        // Move to next cell
        if (e.shiftKey) {
          if (colIndex > 0) focusCell(rowIndex, colIndex - 1);
          else if (rowIndex > 0) focusCell(rowIndex - 1, maxCol);
        } else {
          if (colIndex < maxCol) focusCell(rowIndex, colIndex + 1);
          else if (rowIndex < maxRow) focusCell(rowIndex + 1, 0);
        }
      }
      return;
    }

    switch (e.key) {
      case 'ArrowUp':
        e.preventDefault();
        if (rowIndex > 0) focusCell(rowIndex - 1, colIndex);
        break;
      case 'ArrowDown':
        e.preventDefault();
        if (rowIndex < maxRow) focusCell(rowIndex + 1, colIndex);
        break;
      case 'ArrowLeft':
        e.preventDefault();
        if (colIndex > 0) focusCell(rowIndex, colIndex - 1);
        break;
      case 'ArrowRight':
        e.preventDefault();
        if (colIndex < maxCol) focusCell(rowIndex, colIndex + 1);
        break;
      case 'Home':
        e.preventDefault();
        if (e.ctrlKey) focusCell(0, 0);
        else focusCell(rowIndex, 0);
        break;
      case 'End':
        e.preventDefault();
        if (e.ctrlKey) focusCell(maxRow, maxCol);
        else focusCell(rowIndex, maxCol);
        break;
      case 'PageUp':
        e.preventDefault();
        focusCell(Math.max(0, rowIndex - pageSize), colIndex);
        break;
      case 'PageDown':
        e.preventDefault();
        focusCell(Math.min(maxRow, rowIndex + pageSize), colIndex);
        break;
      case 'Enter':
        e.preventDefault();
        const row = rows[rowIndex];
        const col = visibleColumns[colIndex];
        if (row && col) {
          if (editMode !== 'none' && col.editable) {
            const value = col.valueGetter ? col.valueGetter(row) : (row as Record<string, unknown>)[String(col.field)];
            setEditValue(value);
            setEditingCell({ rowIndex, colIndex });
          } else if (onRowClick) {
            onRowClick(row);
          }
        }
        break;
      case ' ':
        e.preventDefault();
        if (selectable && colIndex === 0) {
          const row = rows[rowIndex];
          if (row) {
            const id = getRowId(row);
            const newIds = selectedIds.includes(id)
              ? selectedIds.filter((i) => i !== id)
              : [...selectedIds, id];
            onSelectionChange?.(newIds);
          }
        }
        break;
      case 'Tab':
        // Allow default tab behavior to move focus out of grid
        break;
      default:
        // Start editing if editable and printable character
        if (editMode !== 'none') {
          const col = visibleColumns[colIndex];
          if (col?.editable && e.key.length === 1 && !e.ctrlKey && !e.altKey && !e.metaKey) {
            e.preventDefault();
            setEditValue(e.key);
            setEditingCell({ rowIndex, colIndex });
          }
        }
        break;
    }
  }, [focusedCell, editingCell, rows, visibleColumns, pageSize, editMode, onCellEdit, onRowClick, selectable, selectedIds, onSelectionChange, focusCell, getRowId, editValue]);

  // Handle sort click
  const handleSortClick = (field: string) => {
    if (!onSortChange) return;
    let newDirection: SortDirection;
    if (sortField === field) {
      newDirection = sortDirection === 'asc' ? 'desc' : sortDirection === 'desc' ? undefined : 'asc';
    } else {
      newDirection = 'asc';
    }
    onSortChange(field, newDirection);
  };

  // Handle select all
  const handleSelectAll = (checked: boolean) => {
    if (!onSelectionChange) return;
    if (checked) {
      onSelectionChange(rows.map(getRowId));
    } else {
      onSelectionChange([]);
    }
  };

  // Handle row selection
  const handleRowSelect = (rowId: number | string) => {
    if (!onSelectionChange) return;
    const newIds = selectedIds.includes(rowId)
      ? selectedIds.filter((id) => id !== rowId)
      : [...selectedIds, rowId];
    onSelectionChange(newIds);
  };

  // Get cell value
  const getCellValue = (row: T, column: DataGridColumn<T>): unknown => {
    if (column.valueGetter) {
      return column.valueGetter(row);
    }
    return (row as Record<string, unknown>)[String(column.field)];
  };

  // Render loading skeleton
  const renderSkeleton = () => (
    <>
      {Array.from({ length: pageSize }).map((_, rowIndex) => (
        <TableRow key={rowIndex}>
          {visibleColumns.map((col, colIndex) => (
            <TableCell key={colIndex}>
              <Skeleton variant="text" width="80%" />
            </TableCell>
          ))}
        </TableRow>
      ))}
    </>
  );

  // Render empty state
  const renderEmpty = () => {
    if (renderEmptyState) {
      return (
        <TableRow>
          <TableCell colSpan={visibleColumns.length} align="center">
            {renderEmptyState()}
          </TableCell>
        </TableRow>
      );
    }
    return (
      <TableRow>
        <TableCell colSpan={visibleColumns.length} align="center" sx={{ py: 4 }}>
          <Typography variant="body1" color="text.secondary">
            {emptyMessage}
          </Typography>
        </TableCell>
      </TableRow>
    );
  };

  // Calculate selection state
  const isAllSelected = rows.length > 0 && selectedIds.length === rows.length;
  const isIndeterminate = selectedIds.length > 0 && selectedIds.length < rows.length;

  // Aria live region for screen readers
  const [announcement, setAnnouncement] = useState('');
  useEffect(() => {
    if (loading) {
      setAnnouncement('Loading data');
    } else if (rows.length === 0) {
      setAnnouncement(emptyMessage);
    } else {
      setAnnouncement(`Showing ${rows.length} rows`);
    }
  }, [loading, rows.length, emptyMessage]);

  return (
    <Paper sx={{ width: '100%', overflow: 'hidden' }}>
      {/* Screen reader announcements */}
      <Box
        role="status"
        aria-live="polite"
        aria-atomic="true"
        sx={visuallyHidden}
      >
        {announcement}
      </Box>

      <TableContainer sx={{ maxHeight }}>
        <Table
          ref={tableRef}
          stickyHeader={stickyHeader}
          size={dense ? 'small' : 'medium'}
          aria-label={ariaLabel}
          aria-describedby={ariaDescribedBy}
          aria-rowcount={totalCount ?? rows.length}
          aria-colcount={visibleColumns.length}
          onKeyDown={handleKeyDown}
        >
          <TableHead>
            <TableRow>
              {visibleColumns.map((column, colIndex) => {
                const isSelectionColumn = column.field === '__selection__';
                const isSortable = !isSelectionColumn && column.sortable !== false && onSortChange;
                const isSorted = sortField === column.field;

                return (
                  <TableCell
                    key={String(column.field)}
                    align={column.align || 'left'}
                    sortDirection={isSorted ? sortDirection : false}
                    aria-sort={
                      isSorted
                        ? sortDirection === 'asc'
                          ? 'ascending'
                          : sortDirection === 'desc'
                          ? 'descending'
                          : 'none'
                        : undefined
                    }
                    aria-label={isSortable ? `Sort by ${column.headerName}` : undefined}
                    sx={{
                      width: column.width,
                      minWidth: column.minWidth,
                      flex: column.flex,
                      fontWeight: 600,
                    }}
                  >
                    {isSelectionColumn ? (
                      <Checkbox
                        checked={isAllSelected}
                        indeterminate={isIndeterminate}
                        onChange={(e) => handleSelectAll(e.target.checked)}
                        inputProps={{
                          'aria-label': 'Select all rows',
                        }}
                      />
                    ) : isSortable ? (
                      <TableSortLabel
                        active={isSorted && sortDirection !== undefined}
                        direction={sortDirection || 'asc'}
                        onClick={() => handleSortClick(String(column.field))}
                      >
                        {column.headerName}
                        {isSorted && (
                          <Box component="span" sx={visuallyHidden}>
                            {sortDirection === 'desc' ? 'sorted descending' : 'sorted ascending'}
                          </Box>
                        )}
                      </TableSortLabel>
                    ) : (
                      column.headerName
                    )}
                  </TableCell>
                );
              })}
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              renderSkeleton()
            ) : rows.length === 0 ? (
              renderEmpty()
            ) : (
              rows.map((row, rowIndex) => {
                const rowId = getRowId(row);
                const isSelected = selectedIds.includes(rowId);

                return (
                  <TableRow
                    key={rowId}
                    hover={hover}
                    selected={isSelected}
                    onClick={() => onRowClick?.(row)}
                    onDoubleClick={() => onRowDoubleClick?.(row)}
                    aria-selected={isSelected || undefined}
                    sx={{
                      cursor: onRowClick ? 'pointer' : 'default',
                      backgroundColor: striped && rowIndex % 2 === 1 ? theme.palette.action.hover : undefined,
                      '&:focus-within': {
                        outline: `2px solid ${theme.palette.primary.main}`,
                        outlineOffset: -2,
                      },
                    }}
                  >
                    {visibleColumns.map((column, colIndex) => {
                      const isSelectionColumn = column.field === '__selection__';
                      const cellKey = getCellKey(rowIndex, colIndex);
                      const isFocused = focusedCell?.rowIndex === rowIndex && focusedCell?.colIndex === colIndex;
                      const isEditing = editingCell?.rowIndex === rowIndex && editingCell?.colIndex === colIndex;

                      return (
                        <TableCell
                          key={cellKey}
                          ref={(el: HTMLTableCellElement | null) => {
                            if (el) cellRefs.current.set(cellKey, el);
                          }}
                          align={column.align || 'left'}
                          tabIndex={isFocused ? 0 : -1}
                          onFocus={() => setFocusedCell({ rowIndex, colIndex })}
                          sx={{
                            outline: isFocused ? `2px solid ${theme.palette.primary.main}` : 'none',
                            outlineOffset: -2,
                          }}
                          aria-readonly={!column.editable}
                        >
                          {isSelectionColumn ? (
                            <Checkbox
                              checked={isSelected}
                              onChange={() => handleRowSelect(rowId)}
                              onClick={(e) => e.stopPropagation()}
                              inputProps={{
                                'aria-label': `Select row ${rowIndex + 1}`,
                              }}
                            />
                          ) : isEditing && column.renderEditCell ? (
                            column.renderEditCell(row, editValue, setEditValue)
                          ) : isEditing ? (
                            <input
                              autoFocus
                              value={String(editValue ?? '')}
                              onChange={(e) => setEditValue(e.target.value)}
                              style={{
                                width: '100%',
                                padding: '4px 8px',
                                border: '1px solid',
                                borderColor: theme.palette.primary.main,
                                borderRadius: 4,
                                fontSize: 'inherit',
                              }}
                              aria-label={`Edit ${column.headerName}`}
                            />
                          ) : column.renderCell ? (
                            column.renderCell(row, rowIndex)
                          ) : (
                            String(getCellValue(row, column) ?? '')
                          )}
                        </TableCell>
                      );
                    })}
                  </TableRow>
                );
              })
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {(onPageChange || onPageSizeChange) && (
        <TablePagination
          component="div"
          count={totalCount ?? rows.length}
          page={page}
          rowsPerPage={pageSize}
          rowsPerPageOptions={pageSizeOptions}
          onPageChange={(_, newPage) => onPageChange?.(newPage)}
          onRowsPerPageChange={(e) => onPageSizeChange?.(parseInt(e.target.value, 10))}
          aria-label="Table pagination"
        />
      )}
    </Paper>
  );
}

export default DataGrid;

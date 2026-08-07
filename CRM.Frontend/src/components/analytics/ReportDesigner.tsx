// TODO: Integration target — reports page
// This component is currently orphaned (not imported by any page).

/**
 * ReportDesigner Component
 * 
 * A visual report designer that allows users to create custom reports
 * by selecting data sources, columns, filters, grouping, and formatting.
 */

import React, { useState, useCallback, useMemo } from 'react';
import {
  Box,
  Paper,
  Typography,
  Button,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Grid,
  Chip,
  Divider,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  Checkbox,
  FormControlLabel,
  Tabs,
  Tab,
  Alert,
  Snackbar,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Tooltip,
  Switch,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  Save as SaveIcon,
  PlayArrow as RunIcon,
  Assessment as ReportIcon,
  FilterList as FilterIcon,
  Sort as SortIcon,
  GroupWork as GroupIcon,
  TableChart as ColumnsIcon,
  Download as ExportIcon,
  Schedule as ScheduleIcon,
  ExpandMore as ExpandIcon,
  DragIndicator as DragIcon,
  Visibility as PreviewIcon,
  ArrowUpward as AscIcon,
  ArrowDownward as DescIcon,
} from '@mui/icons-material';

// Report data source types
export type ReportDataSource = 
  | 'accounts'
  | 'contacts'
  | 'leads'
  | 'opportunities'
  | 'activities'
  | 'tasks'
  | 'quotes'
  | 'orders'
  | 'invoices'
  | 'subscriptions';

// Column data types
export type ColumnType = 'string' | 'number' | 'date' | 'boolean' | 'currency' | 'percent';

// Filter operators
export type FilterOperator = 
  | 'equals'
  | 'not_equals'
  | 'contains'
  | 'starts_with'
  | 'ends_with'
  | 'greater_than'
  | 'less_than'
  | 'between'
  | 'in'
  | 'is_null'
  | 'is_not_null';

// Sort direction
export type SortDirection = 'asc' | 'desc';

// Aggregation functions
export type AggregationFunction = 'sum' | 'avg' | 'min' | 'max' | 'count' | 'count_distinct';

// Column definition
export interface ReportColumn {
  id: string;
  field: string;
  label: string;
  type: ColumnType;
  width?: number;
  visible: boolean;
  aggregation?: AggregationFunction;
  format?: string;
  order: number;
}

// Filter definition
export interface ReportFilter {
  id: string;
  field: string;
  operator: FilterOperator;
  value: string | string[] | number | number[];
  isActive: boolean;
}

// Sort definition
export interface ReportSort {
  field: string;
  direction: SortDirection;
}

// Grouping definition
export interface ReportGrouping {
  field: string;
  label: string;
  showSubtotals: boolean;
}

// Schedule definition
export interface ReportSchedule {
  enabled: boolean;
  frequency: 'daily' | 'weekly' | 'monthly';
  time: string;
  recipients: string[];
  format: 'pdf' | 'excel' | 'csv';
}

// Report configuration
export interface ReportConfig {
  id?: number;
  name: string;
  description?: string;
  dataSource: ReportDataSource;
  columns: ReportColumn[];
  filters: ReportFilter[];
  sorts: ReportSort[];
  groupings: ReportGrouping[];
  schedule?: ReportSchedule;
  limitRows?: number;
  createdAt?: string;
  updatedAt?: string;
}

// Props
interface ReportDesignerProps {
  report?: ReportConfig;
  onSave?: (report: ReportConfig) => Promise<void>;
  onRun?: (report: ReportConfig) => Promise<void>;
  onCancel?: () => void;
  existingReportNames?: string[];
}

// Available columns by data source
const DATA_SOURCE_COLUMNS: Record<ReportDataSource, Array<{ field: string; label: string; type: ColumnType }>> = {
  accounts: [
    { field: 'name', label: 'Account Name', type: 'string' },
    { field: 'industry', label: 'Industry', type: 'string' },
    { field: 'type', label: 'Account Type', type: 'string' },
    { field: 'status', label: 'Status', type: 'string' },
    { field: 'revenue', label: 'Annual Revenue', type: 'currency' },
    { field: 'employees', label: 'Employees', type: 'number' },
    { field: 'createdAt', label: 'Created Date', type: 'date' },
    { field: 'ownerName', label: 'Owner', type: 'string' },
  ],
  contacts: [
    { field: 'firstName', label: 'First Name', type: 'string' },
    { field: 'lastName', label: 'Last Name', type: 'string' },
    { field: 'email', label: 'Email', type: 'string' },
    { field: 'phone', label: 'Phone', type: 'string' },
    { field: 'title', label: 'Title', type: 'string' },
    { field: 'accountName', label: 'Account', type: 'string' },
    { field: 'createdAt', label: 'Created Date', type: 'date' },
  ],
  leads: [
    { field: 'name', label: 'Lead Name', type: 'string' },
    { field: 'company', label: 'Company', type: 'string' },
    { field: 'email', label: 'Email', type: 'string' },
    { field: 'status', label: 'Status', type: 'string' },
    { field: 'source', label: 'Source', type: 'string' },
    { field: 'score', label: 'Lead Score', type: 'number' },
    { field: 'createdAt', label: 'Created Date', type: 'date' },
    { field: 'ownerName', label: 'Owner', type: 'string' },
  ],
  opportunities: [
    { field: 'name', label: 'Opportunity Name', type: 'string' },
    { field: 'accountName', label: 'Account', type: 'string' },
    { field: 'stage', label: 'Stage', type: 'string' },
    { field: 'amount', label: 'Amount', type: 'currency' },
    { field: 'probability', label: 'Probability', type: 'percent' },
    { field: 'closeDate', label: 'Close Date', type: 'date' },
    { field: 'createdAt', label: 'Created Date', type: 'date' },
    { field: 'ownerName', label: 'Owner', type: 'string' },
  ],
  activities: [
    { field: 'type', label: 'Activity Type', type: 'string' },
    { field: 'subject', label: 'Subject', type: 'string' },
    { field: 'relatedTo', label: 'Related To', type: 'string' },
    { field: 'dueDate', label: 'Due Date', type: 'date' },
    { field: 'status', label: 'Status', type: 'string' },
    { field: 'assignedTo', label: 'Assigned To', type: 'string' },
  ],
  tasks: [
    { field: 'subject', label: 'Subject', type: 'string' },
    { field: 'status', label: 'Status', type: 'string' },
    { field: 'priority', label: 'Priority', type: 'string' },
    { field: 'dueDate', label: 'Due Date', type: 'date' },
    { field: 'assignedTo', label: 'Assigned To', type: 'string' },
  ],
  quotes: [
    { field: 'quoteNumber', label: 'Quote Number', type: 'string' },
    { field: 'accountName', label: 'Account', type: 'string' },
    { field: 'status', label: 'Status', type: 'string' },
    { field: 'totalAmount', label: 'Total Amount', type: 'currency' },
    { field: 'validUntil', label: 'Valid Until', type: 'date' },
    { field: 'createdAt', label: 'Created Date', type: 'date' },
  ],
  orders: [
    { field: 'orderNumber', label: 'Order Number', type: 'string' },
    { field: 'accountName', label: 'Account', type: 'string' },
    { field: 'status', label: 'Status', type: 'string' },
    { field: 'totalAmount', label: 'Total Amount', type: 'currency' },
    { field: 'orderDate', label: 'Order Date', type: 'date' },
  ],
  invoices: [
    { field: 'invoiceNumber', label: 'Invoice Number', type: 'string' },
    { field: 'accountName', label: 'Account', type: 'string' },
    { field: 'status', label: 'Status', type: 'string' },
    { field: 'amount', label: 'Amount', type: 'currency' },
    { field: 'dueDate', label: 'Due Date', type: 'date' },
    { field: 'paidDate', label: 'Paid Date', type: 'date' },
  ],
  subscriptions: [
    { field: 'name', label: 'Subscription Name', type: 'string' },
    { field: 'accountName', label: 'Account', type: 'string' },
    { field: 'status', label: 'Status', type: 'string' },
    { field: 'mrr', label: 'MRR', type: 'currency' },
    { field: 'startDate', label: 'Start Date', type: 'date' },
    { field: 'endDate', label: 'End Date', type: 'date' },
  ],
};

// Filter operators by type
const FILTER_OPERATORS: Record<ColumnType, Array<{ operator: FilterOperator; label: string }>> = {
  string: [
    { operator: 'equals', label: 'Equals' },
    { operator: 'not_equals', label: 'Not Equals' },
    { operator: 'contains', label: 'Contains' },
    { operator: 'starts_with', label: 'Starts With' },
    { operator: 'ends_with', label: 'Ends With' },
    { operator: 'is_null', label: 'Is Empty' },
    { operator: 'is_not_null', label: 'Is Not Empty' },
  ],
  number: [
    { operator: 'equals', label: 'Equals' },
    { operator: 'not_equals', label: 'Not Equals' },
    { operator: 'greater_than', label: 'Greater Than' },
    { operator: 'less_than', label: 'Less Than' },
    { operator: 'between', label: 'Between' },
    { operator: 'is_null', label: 'Is Empty' },
  ],
  currency: [
    { operator: 'equals', label: 'Equals' },
    { operator: 'greater_than', label: 'Greater Than' },
    { operator: 'less_than', label: 'Less Than' },
    { operator: 'between', label: 'Between' },
  ],
  percent: [
    { operator: 'equals', label: 'Equals' },
    { operator: 'greater_than', label: 'Greater Than' },
    { operator: 'less_than', label: 'Less Than' },
    { operator: 'between', label: 'Between' },
  ],
  date: [
    { operator: 'equals', label: 'Equals' },
    { operator: 'greater_than', label: 'After' },
    { operator: 'less_than', label: 'Before' },
    { operator: 'between', label: 'Between' },
    { operator: 'is_null', label: 'Is Empty' },
  ],
  boolean: [
    { operator: 'equals', label: 'Is True' },
    { operator: 'not_equals', label: 'Is False' },
  ],
};

// Generate unique ID
const generateId = () => `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`; // NOSONAR - non-security use: UI element ID generation

// Default report config
const getDefaultReport = (): ReportConfig => ({
  name: 'New Report',
  dataSource: 'opportunities',
  columns: [],
  filters: [],
  sorts: [],
  groupings: [],
  limitRows: 1000,
});

/**
 * ReportDesigner Component
 */
export const ReportDesigner: React.FC<ReportDesignerProps> = ({
  report,
  onSave,
  onRun,
  onCancel,
  existingReportNames,
}) => {
  // State
  const [config, setConfig] = useState<ReportConfig>(report || getDefaultReport());
  const [activeTab, setActiveTab] = useState(0);
  const [saving, setSaving] = useState(false);
  const [running, setRunning] = useState(false);
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: false,
    message: '',
    severity: 'success',
  });
  // TODO-AI005-FE-006: per-filter inline validation errors
  const [filterErrors, setFilterErrors] = useState<Record<string, string>>({});

  // REV-FE-007: block Save/Run while any *active* filter has a validation error.
  // Inactive filters (isActive === false) are excluded because their value is not
  // applied to the report — the summary footer and run logic already ignore them
  // (see "Filters:" count below, which is filtered on f.isActive), so a stale/invalid
  // value in a disabled filter should not block submission.
  const hasFilterErrors = useMemo(
    () => config.filters.some((f) => f.isActive && !!filterErrors[f.id]),
    [config.filters, filterErrors],
  );

  // Get available columns for current data source
  const availableColumns = DATA_SOURCE_COLUMNS[config.dataSource] || [];

  /**
   * TODO-AI005-FE-006: Validate a raw string value against a column type.
   * Returns an error message string, or empty string if valid.
   */
  const validateFilterValue = useCallback(
    (value: string, columnType: string): string => {
      if (!value || !value.toString().trim()) return '';

      if (columnType === 'date') {
        // Accept ISO-8601 date strings: YYYY-MM-DD or full ISO datetime
        const iso8601 = /^\d{4}-\d{2}-\d{2}(T\d{2}:\d{2}(:\d{2}(\.\d+)?)?(Z|[+-]\d{2}:\d{2})?)?$/;
        if (!iso8601.test(value.trim())) {
          return 'Please enter a valid date (YYYY-MM-DD or ISO 8601)';
        }
        const parsed = new Date(value.trim());
        if (Number.isNaN(parsed.getTime())) {
          return 'Invalid date — check day, month and year values';
        }
      }

      if (columnType === 'number' || columnType === 'currency' || columnType === 'percent') {
        if (!/^-?\d+(\.\d+)?$/.test(value.trim())) {
          return 'Please enter a numeric value';
        }
      }

      return '';
    },
    [],
  );

  /** Update a filter field and re-validate the value if the value changed */
  const handleUpdateFilterWithValidation = useCallback(
    (filterId: string, patch: Partial<ReportFilter>) => {
      setConfig((prev) => ({
        ...prev,
        filters: prev.filters.map((f) =>
          f.id === filterId ? { ...f, ...patch } : f,
        ),
      }));

      // Validate on value change
      if ('value' in patch && typeof patch.value === 'string') {
        const filter = config.filters.find((f) => f.id === filterId);
        const columnDef = availableColumns.find((c) => c.field === (patch.field ?? filter?.field));
        if (columnDef) {
          const errorMsg = validateFilterValue(patch.value as string, columnDef.type);
          setFilterErrors((prev) => ({ ...prev, [filterId]: errorMsg }));
        }
      }

      // Clear error when operator changes to null-check variants
      if ('operator' in patch && ['is_null', 'is_not_null'].includes(patch.operator as string)) {
        setFilterErrors((prev) => ({ ...prev, [filterId]: '' }));
      }

      // Clear error when field changes (new column, reset validation)
      if ('field' in patch) {
        setFilterErrors((prev) => ({ ...prev, [filterId]: '' }));
      }
    },
    [config.filters, availableColumns, validateFilterValue],
  );

  // Handle data source change
  const handleDataSourceChange = useCallback((newSource: ReportDataSource) => {
    setConfig(prev => ({
      ...prev,
      dataSource: newSource,
      columns: [],
      filters: [],
      sorts: [],
      groupings: [],
    }));
  }, []);

  // Add column
  const handleAddColumn = useCallback((field: string) => {
    const columnDef = availableColumns.find(c => c.field === field);
    if (!columnDef) return;

    const newColumn: ReportColumn = {
      id: generateId(),
      field: columnDef.field,
      label: columnDef.label,
      type: columnDef.type,
      visible: true,
      order: config.columns.length,
    };

    setConfig(prev => ({
      ...prev,
      columns: [...prev.columns, newColumn],
    }));
  }, [availableColumns, config.columns.length]);

  // Remove column
  const handleRemoveColumn = useCallback((columnId: string) => {
    setConfig(prev => ({
      ...prev,
      columns: prev.columns.filter(c => c.id !== columnId),
    }));
  }, []);

  // Add filter
  const handleAddFilter = useCallback(() => {
    if (availableColumns.length === 0) return;

    const firstColumn = availableColumns[0];
    const operators = FILTER_OPERATORS[firstColumn.type] || FILTER_OPERATORS.string;

    const newFilter: ReportFilter = {
      id: generateId(),
      field: firstColumn.field,
      operator: operators[0].operator,
      value: '',
      isActive: true,
    };

    setConfig(prev => ({
      ...prev,
      filters: [...prev.filters, newFilter],
    }));
  }, [availableColumns]);

  // Update filter
  const handleUpdateFilter = useCallback((filterId: string, updates: Partial<ReportFilter>) => {
    setConfig(prev => ({
      ...prev,
      filters: prev.filters.map(f => 
        f.id === filterId ? { ...f, ...updates } : f
      ),
    }));
  }, []);

  // Remove filter
  const handleRemoveFilter = useCallback((filterId: string) => {
    setConfig(prev => ({
      ...prev,
      filters: prev.filters.filter(f => f.id !== filterId),
    }));
  }, []);

  // Add sort
  const handleAddSort = useCallback((field: string) => {
    // Check if already sorting by this field
    if (config.sorts.some(s => s.field === field)) return;

    setConfig(prev => ({
      ...prev,
      sorts: [...prev.sorts, { field, direction: 'asc' }],
    }));
  }, [config.sorts]);

  // Toggle sort direction
  const handleToggleSortDirection = useCallback((field: string) => {
    setConfig(prev => ({
      ...prev,
      sorts: prev.sorts.map(s => 
        s.field === field 
          ? { ...s, direction: s.direction === 'asc' ? 'desc' : 'asc' }
          : s
      ),
    }));
  }, []);

  // Remove sort
  const handleRemoveSort = useCallback((field: string) => {
    setConfig(prev => ({
      ...prev,
      sorts: prev.sorts.filter(s => s.field !== field),
    }));
  }, []);

  // Add grouping
  const handleAddGrouping = useCallback((field: string) => {
    const columnDef = availableColumns.find(c => c.field === field);
    if (!columnDef || config.groupings.some(g => g.field === field)) return;

    setConfig(prev => ({
      ...prev,
      groupings: [...prev.groupings, { field, label: columnDef.label, showSubtotals: true }],
    }));
  }, [availableColumns, config.groupings]);

  // Remove grouping
  const handleRemoveGrouping = useCallback((field: string) => {
    setConfig(prev => ({
      ...prev,
      groupings: prev.groupings.filter(g => g.field !== field),
    }));
  }, []);

  // Save report
  const handleSave = useCallback(async () => {
    if (hasFilterErrors) {
      setSnackbar({ open: true, message: 'Fix invalid filter values before saving', severity: 'error' });
      return;
    }
    const normalizedName = config.name.trim();
    if (!normalizedName) {
      setSnackbar({ open: true, message: 'Report name is required', severity: 'error' });
      return;
    }
    const normalizedExistingNames = (existingReportNames || [])
      .map(name => name?.trim().toLowerCase())
      .filter(Boolean);
    if (normalizedExistingNames.includes(normalizedName.toLowerCase())) {
      setSnackbar({ open: true, message: 'Report name must be unique', severity: 'error' });
      return;
    }
    if (config.columns.length === 0) {
      setSnackbar({ open: true, message: 'Report query is required', severity: 'error' });
      return;
    }

    setSaving(true);
    try {
      if (onSave) {
        await onSave(config);
      }
      setSnackbar({ open: true, message: 'Report saved successfully', severity: 'success' });
    } catch (error) {
      setSnackbar({ open: true, message: 'Failed to save report', severity: 'error' });
    } finally {
      setSaving(false);
    }
  }, [config, existingReportNames, onSave, hasFilterErrors]);

  // Run report
  const handleRun = useCallback(async () => {
    if (hasFilterErrors) {
      setSnackbar({ open: true, message: 'Fix invalid filter values before running the report', severity: 'error' });
      return;
    }
    if (config.columns.length === 0) {
      setSnackbar({ open: true, message: 'At least one column is required to run the report', severity: 'error' });
      return;
    }

    setRunning(true);
    try {
      if (onRun) {
        await onRun(config);
      }
    } catch (error) {
      setSnackbar({ open: true, message: 'Failed to run report', severity: 'error' });
    } finally {
      setRunning(false);
    }
  }, [config, onRun, hasFilterErrors]);

  // Render columns tab
  const renderColumnsTab = () => (
    <Box>
      <Typography variant="subtitle2" gutterBottom>
        Available Columns
      </Typography>
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 3 }}>
        {availableColumns
          .filter(col => !config.columns.some(c => c.field === col.field))
          .map(col => (
            <Chip
              key={col.field}
              label={col.label}
              onClick={() => handleAddColumn(col.field)}
              icon={<AddIcon />}
              variant="outlined"
              size="small"
            />
          ))}
      </Box>

      <Typography variant="subtitle2" gutterBottom>
        Selected Columns ({config.columns.length})
      </Typography>
      {config.columns.length === 0 ? (
        <Alert severity="info">Click on available columns above to add them to your report</Alert>
      ) : (
        <List dense>
          {config.columns.map((column, index) => (
            <ListItem key={column.id} divider>
              <DragIcon sx={{ mr: 1, color: 'text.disabled', cursor: 'move' }} />
              <ListItemText
                primary={column.label}
                secondary={`Type: ${column.type}`}
              />
              <ListItemSecondaryAction>
                <IconButton size="small" onClick={() => handleRemoveColumn(column.id)}>
                  <DeleteIcon fontSize="small" />
                </IconButton>
              </ListItemSecondaryAction>
            </ListItem>
          ))}
        </List>
      )}
    </Box>
  );

  // Render filters tab
  const renderFiltersTab = () => (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
        <Typography variant="subtitle2">Filters ({config.filters.length})</Typography>
        <Button size="small" startIcon={<AddIcon />} onClick={handleAddFilter}>
          Add Filter
        </Button>
      </Box>

      {config.filters.length === 0 ? (
        <Alert severity="info">No filters applied. Click "Add Filter" to filter your report data.</Alert>
      ) : (
        <List>
          {config.filters.map((filter) => {
            const columnDef = availableColumns.find(c => c.field === filter.field);
            const operators = columnDef ? FILTER_OPERATORS[columnDef.type] : FILTER_OPERATORS.string;

            return (
              <ListItem key={filter.id} divider sx={{ flexWrap: 'wrap', gap: 1 }}>
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={filter.isActive}
                      onChange={(e) => handleUpdateFilterWithValidation(filter.id, { isActive: e.target.checked })}
                      size="small"
                    />
                  }
                  label=""
                />
                <FormControl size="small" sx={{ minWidth: 120 }}>
                  <Select
                    value={filter.field}
                    onChange={(e) => handleUpdateFilterWithValidation(filter.id, { field: e.target.value })}
                  >
                    {availableColumns.map(col => (
                      <MenuItem key={col.field} value={col.field}>{col.label}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
                <FormControl size="small" sx={{ minWidth: 120 }}>
                  <Select
                    value={filter.operator}
                    onChange={(e) => handleUpdateFilterWithValidation(filter.id, { operator: e.target.value as FilterOperator })}
                  >
                    {operators.map(op => (
                      <MenuItem key={op.operator} value={op.operator}>{op.label}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
                {/* TODO-AI005-FE-006: type-aware validated filter value input */}
                {!['is_null', 'is_not_null'].includes(filter.operator) && (() => {
                  const colType = columnDef?.type ?? 'string';
                  const errorMsg = filterErrors[filter.id] ?? '';

                  // Date column — use HTML5 date input + ISO validation
                  if (colType === 'date') {
                    return (
                      <TextField
                        size="small"
                        type="date"
                        placeholder="YYYY-MM-DD"
                        value={filter.value}
                        onChange={(e) =>
                          handleUpdateFilterWithValidation(filter.id, { value: e.target.value })
                        }
                        onBlur={(e) => {
                          const msg = validateFilterValue(e.target.value, 'date');
                          setFilterErrors((prev) => ({ ...prev, [filter.id]: msg }));
                        }}
                        error={!!errorMsg}
                        helperText={errorMsg || undefined}
                        sx={{ minWidth: 165 }}
                        inputProps={{ 'aria-label': 'Filter date value', 'aria-invalid': !!errorMsg }}
                        InputLabelProps={{ shrink: true }}
                      />
                    );
                  }

                  // Numeric column — use number input + numeric validation
                  if (colType === 'number' || colType === 'currency' || colType === 'percent') {
                    return (
                      <TextField
                        size="small"
                        type="number"
                        placeholder="Numeric value"
                        value={filter.value}
                        onChange={(e) =>
                          handleUpdateFilterWithValidation(filter.id, { value: e.target.value })
                        }
                        onBlur={(e) => {
                          const msg = validateFilterValue(e.target.value, colType);
                          setFilterErrors((prev) => ({ ...prev, [filter.id]: msg }));
                        }}
                        error={!!errorMsg}
                        helperText={errorMsg || undefined}
                        sx={{ minWidth: 150 }}
                        inputProps={{ 'aria-label': 'Filter numeric value', 'aria-invalid': !!errorMsg }}
                      />
                    );
                  }

                  // Boolean — not applicable for non-null operators; show nothing for custom operators
                  if (colType === 'boolean') {
                    return (
                      <FormControl size="small" sx={{ minWidth: 120 }}>
                        <Select
                          value={filter.value?.toString() ?? 'true'}
                          onChange={(e) =>
                            handleUpdateFilterWithValidation(filter.id, { value: e.target.value })
                          }
                          inputProps={{ 'aria-label': 'Filter boolean value' }}
                        >
                          <MenuItem value="true">True</MenuItem>
                          <MenuItem value="false">False</MenuItem>
                        </Select>
                      </FormControl>
                    );
                  }

                  // Default — plain text input (string / enum)
                  return (
                    <TextField
                      size="small"
                      placeholder="Value"
                      value={filter.value}
                      onChange={(e) =>
                        handleUpdateFilterWithValidation(filter.id, { value: e.target.value })
                      }
                      sx={{ minWidth: 150 }}
                      inputProps={{ 'aria-label': 'Filter text value' }}
                    />
                  );
                })()}
                <IconButton size="small" onClick={() => handleRemoveFilter(filter.id)}>
                  <DeleteIcon fontSize="small" />
                </IconButton>
              </ListItem>
            );
          })}
        </List>
      )}
    </Box>
  );

  // Render sorting tab
  const renderSortingTab = () => (
    <Box>
      <Typography variant="subtitle2" gutterBottom>
        Sort By
      </Typography>
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
        {config.columns
          .filter(col => !config.sorts.some(s => s.field === col.field))
          .map(col => (
            <Chip
              key={col.field}
              label={col.label}
              onClick={() => handleAddSort(col.field)}
              icon={<AddIcon />}
              variant="outlined"
              size="small"
            />
          ))}
      </Box>

      {config.sorts.length === 0 ? (
        <Alert severity="info">No sorting applied. Click on columns above to add sorting.</Alert>
      ) : (
        <List dense>
          {config.sorts.map((sort, index) => {
            const column = config.columns.find(c => c.field === sort.field);
            return (
              <ListItem key={sort.field} divider>
                <Chip label={index + 1} size="small" sx={{ mr: 1 }} />
                <ListItemText primary={column?.label || sort.field} />
                <IconButton size="small" onClick={() => handleToggleSortDirection(sort.field)}>
                  {sort.direction === 'asc' ? <AscIcon /> : <DescIcon />}
                </IconButton>
                <IconButton size="small" onClick={() => handleRemoveSort(sort.field)}>
                  <DeleteIcon fontSize="small" />
                </IconButton>
              </ListItem>
            );
          })}
        </List>
      )}
    </Box>
  );

  // Render grouping tab
  const renderGroupingTab = () => (
    <Box>
      <Typography variant="subtitle2" gutterBottom>
        Group By
      </Typography>
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
        {config.columns
          .filter(col => !config.groupings.some(g => g.field === col.field))
          .map(col => (
            <Chip
              key={col.field}
              label={col.label}
              onClick={() => handleAddGrouping(col.field)}
              icon={<AddIcon />}
              variant="outlined"
              size="small"
            />
          ))}
      </Box>

      {config.groupings.length === 0 ? (
        <Alert severity="info">No grouping applied. Click on columns above to group your data.</Alert>
      ) : (
        <List dense>
          {config.groupings.map((grouping, index) => (
            <ListItem key={grouping.field} divider>
              <Chip label={index + 1} size="small" sx={{ mr: 1 }} />
              <ListItemText primary={grouping.label} />
              <FormControlLabel
                control={
                  <Switch
                    checked={grouping.showSubtotals}
                    onChange={(e) => {
                      setConfig(prev => ({
                        ...prev,
                        groupings: prev.groupings.map(g =>
                          g.field === grouping.field ? { ...g, showSubtotals: e.target.checked } : g
                        ),
                      }));
                    }}
                    size="small"
                  />
                }
                label="Subtotals"
              />
              <IconButton size="small" onClick={() => handleRemoveGrouping(grouping.field)}>
                <DeleteIcon fontSize="small" />
              </IconButton>
            </ListItem>
          ))}
        </List>
      )}
    </Box>
  );

  return (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      {/* Header */}
      <Paper sx={{ p: 2, mb: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <ReportIcon color="primary" />
            <TextField
              size="small"
              label="Report Name"
              value={config.name}
              onChange={(e) => setConfig(prev => ({ ...prev, name: e.target.value }))}
              sx={{ minWidth: 250 }}
            />
            <FormControl size="small" sx={{ minWidth: 150 }}>
              <InputLabel>Data Source</InputLabel>
              <Select
                value={config.dataSource}
                label="Data Source"
                onChange={(e) => handleDataSourceChange(e.target.value as ReportDataSource)}
              >
                {Object.keys(DATA_SOURCE_COLUMNS).map((source) => (
                  <MenuItem key={source} value={source}>
                    {source.charAt(0).toUpperCase() + source.slice(1)}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Box>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button
              variant="outlined"
              startIcon={<RunIcon />}
              onClick={handleRun}
              disabled={running || config.columns.length === 0 || hasFilterErrors}
            >
              {running ? 'Running...' : 'Run Report'}
            </Button>
            <Button
              variant="contained"
              startIcon={<SaveIcon />}
              onClick={handleSave}
              disabled={saving || hasFilterErrors}
            >
              {saving ? 'Saving...' : 'Save Report'}
            </Button>
            {onCancel && (
              <Button onClick={onCancel}>Cancel</Button>
            )}
          </Box>
        </Box>
      </Paper>

      {/* Main Content */}
      <Paper sx={{ flexGrow: 1, display: 'flex', flexDirection: 'column' }}>
        <Tabs
          value={activeTab}
          onChange={(_, newValue) => setActiveTab(newValue)}
          sx={{ borderBottom: 1, borderColor: 'divider', px: 2 }}
        >
          <Tab icon={<ColumnsIcon />} label="Columns" iconPosition="start" />
          <Tab icon={<FilterIcon />} label="Filters" iconPosition="start" />
          <Tab icon={<SortIcon />} label="Sorting" iconPosition="start" />
          <Tab icon={<GroupIcon />} label="Grouping" iconPosition="start" />
        </Tabs>

        <Box sx={{ flexGrow: 1, p: 3, overflow: 'auto' }}>
          {activeTab === 0 && renderColumnsTab()}
          {activeTab === 1 && renderFiltersTab()}
          {activeTab === 2 && renderSortingTab()}
          {activeTab === 3 && renderGroupingTab()}
        </Box>
      </Paper>

      {/* Summary Footer */}
      <Paper sx={{ p: 2, mt: 2 }}>
        <Box sx={{ display: 'flex', gap: 3, alignItems: 'center' }}>
          <Typography variant="body2" color="text.secondary">
            <strong>Data Source:</strong> {config.dataSource}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            <strong>Columns:</strong> {config.columns.length}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            <strong>Filters:</strong> {config.filters.filter(f => f.isActive).length}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            <strong>Sort By:</strong> {config.sorts.length}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            <strong>Group By:</strong> {config.groupings.length}
          </Typography>
          <Box sx={{ flexGrow: 1 }} />
          <TextField
            size="small"
            type="number"
            label="Row Limit"
            value={config.limitRows || 1000}
            onChange={(e) => setConfig(prev => ({ ...prev, limitRows: Number.parseInt(e.target.value) || 1000 }))}
            sx={{ width: 120 }}
          />
        </Box>
      </Paper>

      {/* Snackbar */}
      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar(prev => ({ ...prev, open: false }))}
      >
        <Alert severity={snackbar.severity} onClose={() => setSnackbar(prev => ({ ...prev, open: false }))}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default ReportDesigner;

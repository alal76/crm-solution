/**
 * AdvancedFilterBuilder - Component for building complex filter conditions
 * Supports AND/OR grouping, multiple operators, and field-type-specific inputs
 */

import React, { useState, useCallback, useMemo } from 'react';
import {
  Box,
  Paper,
  Stack,
  Button,
  IconButton,
  Select,
  MenuItem,
  TextField,
  FormControl,
  InputLabel,
  Typography,
  Chip,
  Tooltip,
  Divider,
  Collapse,
  Switch,
  FormControlLabel,
  useTheme,
  alpha,
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  FilterList as FilterIcon,
  Close as CloseIcon,
  Save as SaveIcon,
  RestartAlt as ResetIcon,
} from '@mui/icons-material';

// Field definition
export interface FilterField {
  name: string;
  label: string;
  type: 'string' | 'number' | 'date' | 'boolean' | 'select' | 'enum';
  options?: { value: string | number; label: string }[];
  operators?: FilterOperator[];
}

// Operators
export type FilterOperator =
  | 'equals'
  | 'notEquals'
  | 'contains'
  | 'notContains'
  | 'startsWith'
  | 'endsWith'
  | 'greaterThan'
  | 'lessThan'
  | 'greaterThanOrEquals'
  | 'lessThanOrEquals'
  | 'between'
  | 'in'
  | 'notIn'
  | 'isEmpty'
  | 'isNotEmpty';

// Operator labels
export const operatorLabels: Record<FilterOperator, string> = {
  equals: 'Equals',
  notEquals: 'Not equals',
  contains: 'Contains',
  notContains: 'Does not contain',
  startsWith: 'Starts with',
  endsWith: 'Ends with',
  greaterThan: 'Greater than',
  lessThan: 'Less than',
  greaterThanOrEquals: 'Greater than or equals',
  lessThanOrEquals: 'Less than or equals',
  between: 'Between',
  in: 'In',
  notIn: 'Not in',
  isEmpty: 'Is empty',
  isNotEmpty: 'Is not empty',
};

// Default operators by field type
const defaultOperators: Record<FilterField['type'], FilterOperator[]> = {
  string: ['equals', 'notEquals', 'contains', 'notContains', 'startsWith', 'endsWith', 'isEmpty', 'isNotEmpty'],
  number: ['equals', 'notEquals', 'greaterThan', 'lessThan', 'greaterThanOrEquals', 'lessThanOrEquals', 'between', 'isEmpty', 'isNotEmpty'],
  date: ['equals', 'notEquals', 'greaterThan', 'lessThan', 'greaterThanOrEquals', 'lessThanOrEquals', 'between', 'isEmpty', 'isNotEmpty'],
  boolean: ['equals'],
  select: ['equals', 'notEquals', 'in', 'notIn', 'isEmpty', 'isNotEmpty'],
  enum: ['equals', 'notEquals', 'in', 'notIn', 'isEmpty', 'isNotEmpty'],
};

// Filter condition
export interface FilterCondition {
  id: string;
  field: string;
  operator: FilterOperator;
  value: unknown;
  value2?: unknown; // For 'between' operator
}

// Filter group
export interface FilterGroup {
  id: string;
  logic: 'AND' | 'OR';
  conditions: FilterCondition[];
  groups?: FilterGroup[];
}

// Props
export interface AdvancedFilterBuilderProps {
  fields: FilterField[];
  value?: FilterGroup;
  onChange: (filter: FilterGroup | null) => void;
  // Saved filters
  savedFilters?: { name: string; filter: FilterGroup }[];
  onSaveFilter?: (name: string, filter: FilterGroup) => void;
  onDeleteSavedFilter?: (name: string) => void;
  // Options
  maxDepth?: number;
  showSavedFilters?: boolean;
  collapsible?: boolean;
  defaultExpanded?: boolean;
}

// Generate unique ID
const generateId = () => Math.random().toString(36).substring(2, 11);

// Create empty condition
const createEmptyCondition = (): FilterCondition => ({
  id: generateId(),
  field: '',
  operator: 'equals',
  value: '',
});

// Create empty group
const createEmptyGroup = (): FilterGroup => ({
  id: generateId(),
  logic: 'AND',
  conditions: [createEmptyCondition()],
});

export const AdvancedFilterBuilder: React.FC<AdvancedFilterBuilderProps> = ({
  fields,
  value,
  onChange,
  savedFilters = [],
  onSaveFilter,
  onDeleteSavedFilter,
  maxDepth = 2,
  showSavedFilters = true,
  collapsible = false,
  defaultExpanded = true,
}) => {
  const theme = useTheme();
  const [expanded, setExpanded] = useState(defaultExpanded);
  const [saveDialogOpen, setSaveDialogOpen] = useState(false);
  const [filterName, setFilterName] = useState('');

  // Initialize with empty group if no value
  const filter = value || createEmptyGroup();

  // Update filter
  const updateFilter = useCallback((newFilter: FilterGroup) => {
    onChange(newFilter);
  }, [onChange]);

  // Add condition to group
  const addCondition = useCallback((groupId: string) => {
    const updateGroup = (group: FilterGroup): FilterGroup => {
      if (group.id === groupId) {
        return {
          ...group,
          conditions: [...group.conditions, createEmptyCondition()],
        };
      }
      if (group.groups) {
        return {
          ...group,
          groups: group.groups.map(updateGroup),
        };
      }
      return group;
    };
    updateFilter(updateGroup(filter));
  }, [filter, updateFilter]);

  // Remove condition from group
  const removeCondition = useCallback((groupId: string, conditionId: string) => {
    const updateGroup = (group: FilterGroup): FilterGroup => {
      if (group.id === groupId) {
        return {
          ...group,
          conditions: group.conditions.filter((c) => c.id !== conditionId),
        };
      }
      if (group.groups) {
        return {
          ...group,
          groups: group.groups.map(updateGroup),
        };
      }
      return group;
    };
    updateFilter(updateGroup(filter));
  }, [filter, updateFilter]);

  // Update condition
  const updateCondition = useCallback((groupId: string, conditionId: string, updates: Partial<FilterCondition>) => {
    const updateGroup = (group: FilterGroup): FilterGroup => {
      if (group.id === groupId) {
        return {
          ...group,
          conditions: group.conditions.map((c) =>
            c.id === conditionId ? { ...c, ...updates } : c
          ),
        };
      }
      if (group.groups) {
        return {
          ...group,
          groups: group.groups.map(updateGroup),
        };
      }
      return group;
    };
    updateFilter(updateGroup(filter));
  }, [filter, updateFilter]);

  // Toggle logic
  const toggleLogic = useCallback((groupId: string) => {
    const updateGroup = (group: FilterGroup): FilterGroup => {
      if (group.id === groupId) {
        return {
          ...group,
          logic: group.logic === 'AND' ? 'OR' : 'AND',
        };
      }
      if (group.groups) {
        return {
          ...group,
          groups: group.groups.map(updateGroup),
        };
      }
      return group;
    };
    updateFilter(updateGroup(filter));
  }, [filter, updateFilter]);

  // Reset filter
  const handleReset = useCallback(() => {
    onChange(null);
  }, [onChange]);

  // Get operators for field
  const getOperators = (fieldName: string): FilterOperator[] => {
    const field = fields.find((f) => f.name === fieldName);
    if (!field) return defaultOperators.string;
    return field.operators || defaultOperators[field.type];
  };

  // Render value input
  const renderValueInput = (
    condition: FilterCondition,
    groupId: string,
    field: FilterField | undefined
  ) => {
    // No value needed for isEmpty/isNotEmpty
    if (condition.operator === 'isEmpty' || condition.operator === 'isNotEmpty') {
      return null;
    }

    const fieldType = field?.type || 'string';

    // Boolean
    if (fieldType === 'boolean') {
      return (
        <FormControl size="small" sx={{ minWidth: 100 }}>
          <Select
            value={condition.value ?? ''}
            onChange={(e) => updateCondition(groupId, condition.id, { value: e.target.value === 'true' })}
          >
            <MenuItem value="true">Yes</MenuItem>
            <MenuItem value="false">No</MenuItem>
          </Select>
        </FormControl>
      );
    }

    // Select/Enum
    if ((fieldType === 'select' || fieldType === 'enum') && field?.options) {
      return (
        <FormControl size="small" sx={{ minWidth: 150 }}>
          <Select
            value={condition.value ?? ''}
            onChange={(e) => updateCondition(groupId, condition.id, { value: e.target.value })}
            multiple={condition.operator === 'in' || condition.operator === 'notIn'}
          >
            {field.options.map((opt) => (
              <MenuItem key={opt.value} value={opt.value}>
                {opt.label}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      );
    }

    // Date
    if (fieldType === 'date') {
      return (
        <LocalizationProvider dateAdapter={AdapterDateFns}>
          <Stack direction="row" spacing={1}>
            <DatePicker
              value={condition.value ? new Date(condition.value as string) : null}
              onChange={(newValue) => updateCondition(groupId, condition.id, { value: newValue?.toISOString() })}
              slotProps={{
                textField: { size: 'small', sx: { width: 150 } },
              }}
            />
            {condition.operator === 'between' && (
              <>
                <Typography sx={{ alignSelf: 'center' }}>and</Typography>
                <DatePicker
                  value={condition.value2 ? new Date(condition.value2 as string) : null}
                  onChange={(newValue) => updateCondition(groupId, condition.id, { value2: newValue?.toISOString() })}
                  slotProps={{
                    textField: { size: 'small', sx: { width: 150 } },
                  }}
                />
              </>
            )}
          </Stack>
        </LocalizationProvider>
      );
    }

    // Number
    if (fieldType === 'number') {
      return (
        <Stack direction="row" spacing={1}>
          <TextField
            size="small"
            type="number"
            value={condition.value ?? ''}
            onChange={(e) => updateCondition(groupId, condition.id, { value: e.target.value === '' ? null : Number(e.target.value) })}
            sx={{ width: 120 }}
          />
          {condition.operator === 'between' && (
            <>
              <Typography sx={{ alignSelf: 'center' }}>and</Typography>
              <TextField
                size="small"
                type="number"
                value={condition.value2 ?? ''}
                onChange={(e) => updateCondition(groupId, condition.id, { value2: e.target.value === '' ? null : Number(e.target.value) })}
                sx={{ width: 120 }}
              />
            </>
          )}
        </Stack>
      );
    }

    // Default: String
    return (
      <TextField
        size="small"
        value={condition.value ?? ''}
        onChange={(e) => updateCondition(groupId, condition.id, { value: e.target.value })}
        placeholder="Enter value..."
        sx={{ minWidth: 200 }}
      />
    );
  };

  // Render condition row
  const renderCondition = (condition: FilterCondition, groupId: string, isLast: boolean) => {
    const field = fields.find((f) => f.name === condition.field);
    const operators = getOperators(condition.field);

    return (
      <Stack
        key={condition.id}
        direction="row"
        spacing={1}
        alignItems="center"
        sx={{ mb: isLast ? 0 : 1 }}
      >
        {/* Field selector */}
        <FormControl size="small" sx={{ minWidth: 150 }}>
          <InputLabel>Field</InputLabel>
          <Select
            value={condition.field}
            onChange={(e) => updateCondition(groupId, condition.id, { 
              field: e.target.value,
              operator: 'equals',
              value: '',
            })}
            label="Field"
          >
            {fields.map((f) => (
              <MenuItem key={f.name} value={f.name}>
                {f.label}
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        {/* Operator selector */}
        <FormControl size="small" sx={{ minWidth: 150 }}>
          <InputLabel>Operator</InputLabel>
          <Select
            value={condition.operator}
            onChange={(e) => updateCondition(groupId, condition.id, { operator: e.target.value as FilterOperator })}
            label="Operator"
            disabled={!condition.field}
          >
            {operators.map((op) => (
              <MenuItem key={op} value={op}>
                {operatorLabels[op]}
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        {/* Value input */}
        {renderValueInput(condition, groupId, field)}

        {/* Remove button */}
        <Tooltip title="Remove condition">
          <IconButton
            size="small"
            onClick={() => removeCondition(groupId, condition.id)}
            disabled={filter.conditions.length === 1 && filter.id === groupId}
          >
            <DeleteIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      </Stack>
    );
  };

  // Render filter group
  const renderGroup = (group: FilterGroup, depth: number = 0) => {
    return (
      <Paper
        variant="outlined"
        sx={{
          p: 2,
          bgcolor: depth > 0 ? alpha(theme.palette.primary.main, 0.02) : 'transparent',
        }}
      >
        <Stack spacing={2}>
          {/* Group header */}
          <Stack direction="row" justifyContent="space-between" alignItems="center">
            <Chip
              label={group.logic}
              color={group.logic === 'AND' ? 'primary' : 'secondary'}
              size="small"
              onClick={() => toggleLogic(group.id)}
              sx={{ cursor: 'pointer' }}
            />
            <Stack direction="row" spacing={1}>
              <Button
                size="small"
                startIcon={<AddIcon />}
                onClick={() => addCondition(group.id)}
              >
                Add Condition
              </Button>
            </Stack>
          </Stack>

          {/* Conditions */}
          <Box>
            {group.conditions.map((condition, index) => (
              <Box key={condition.id}>
                {index > 0 && (
                  <Typography
                    variant="caption"
                    color="text.secondary"
                    sx={{ display: 'block', my: 1, textAlign: 'center' }}
                  >
                    {group.logic}
                  </Typography>
                )}
                {renderCondition(condition, group.id, index === group.conditions.length - 1)}
              </Box>
            ))}
          </Box>

          {/* Nested groups */}
          {group.groups?.map((nestedGroup) => (
            <Box key={nestedGroup.id} sx={{ pl: 2 }}>
              {renderGroup(nestedGroup, depth + 1)}
            </Box>
          ))}
        </Stack>
      </Paper>
    );
  };

  // Count active filters
  const activeFilterCount = useMemo(() => {
    const countConditions = (group: FilterGroup): number => {
      let count = group.conditions.filter((c) => c.field && c.value !== '').length;
      if (group.groups) {
        count += group.groups.reduce((acc, g) => acc + countConditions(g), 0);
      }
      return count;
    };
    return countConditions(filter);
  }, [filter]);

  // Content
  const content = (
    <Box>
      {/* Saved filters */}
      {showSavedFilters && savedFilters.length > 0 && (
        <Box sx={{ mb: 2 }}>
          <Typography variant="subtitle2" sx={{ mb: 1 }}>
            Saved Filters
          </Typography>
          <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
            {savedFilters.map((sf) => (
              <Chip
                key={sf.name}
                label={sf.name}
                onClick={() => onChange(sf.filter)}
                onDelete={onDeleteSavedFilter ? () => onDeleteSavedFilter(sf.name) : undefined}
                variant="outlined"
                size="small"
              />
            ))}
          </Stack>
        </Box>
      )}

      {/* Filter builder */}
      {renderGroup(filter)}

      {/* Actions */}
      <Stack direction="row" spacing={1} sx={{ mt: 2 }} justifyContent="flex-end">
        <Button
          size="small"
          startIcon={<ResetIcon />}
          onClick={handleReset}
          disabled={activeFilterCount === 0}
        >
          Reset
        </Button>
        {onSaveFilter && (
          <Button
            size="small"
            startIcon={<SaveIcon />}
            onClick={() => setSaveDialogOpen(true)}
            disabled={activeFilterCount === 0}
          >
            Save Filter
          </Button>
        )}
      </Stack>
    </Box>
  );

  // Collapsible wrapper
  if (collapsible) {
    return (
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Stack
          direction="row"
          justifyContent="space-between"
          alignItems="center"
          onClick={() => setExpanded(!expanded)}
          sx={{ cursor: 'pointer' }}
        >
          <Stack direction="row" spacing={1} alignItems="center">
            <FilterIcon />
            <Typography variant="subtitle1">Filters</Typography>
            {activeFilterCount > 0 && (
              <Chip label={activeFilterCount} size="small" color="primary" />
            )}
          </Stack>
          <IconButton size="small">
            {expanded ? <CloseIcon /> : <AddIcon />}
          </IconButton>
        </Stack>
        <Collapse in={expanded}>
          <Box sx={{ mt: 2 }}>{content}</Box>
        </Collapse>
      </Paper>
    );
  }

  return content;
};

export default AdvancedFilterBuilder;

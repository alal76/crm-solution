/**
 * CustomFieldRenderer - Renders dynamic custom fields in forms/views
 * TODO-SD001-003 (P2)
 *
 * Supports: text, number, date, select, multiselect, boolean, textarea
 * In readOnly mode renders values as plain text.
 */

import React from 'react';
import {
  TextField,
  FormControl,
  FormControlLabel,
  InputLabel,
  Select,
  MenuItem,
  Switch,
  Box,
  Typography,
  Autocomplete,
  Checkbox,
  Chip,
  SelectChangeEvent,
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { parseISO } from 'date-fns';

// ─── Types ───────────────────────────────────────────────────────────────────

export interface CustomFieldOption {
  value: string;
  label: string;
}

export type CustomFieldType =
  | 'text'
  | 'number'
  | 'date'
  | 'select'
  | 'multiselect'
  | 'boolean'
  | 'textarea';

export interface CustomFieldDefinition {
  id: number;
  name: string;
  label: string;
  type: CustomFieldType;
  options?: CustomFieldOption[];
  required?: boolean;
  placeholder?: string;
}

export interface CustomFieldRendererProps {
  field: CustomFieldDefinition;
  value: unknown;
  onChange?: (value: unknown) => void;
  readOnly?: boolean;
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

function displayValue(field: CustomFieldDefinition, value: unknown): string {
  if (value === null || value === undefined || value === '') return '—';

  switch (field.type) {
    case 'boolean':
      return value ? 'Yes' : 'No';
    case 'select': {
      const opt = field.options?.find((o) => o.value === String(value));
      return opt ? opt.label : String(value);
    }
    case 'multiselect': {
      const vals = Array.isArray(value) ? (value as string[]) : [String(value)];
      return vals
        .map((v) => field.options?.find((o) => o.value === v)?.label ?? v)
        .join(', ');
    }
    case 'date': {
      try {
        return new Date(String(value)).toLocaleDateString();
      } catch {
        return String(value);
      }
    }
    default:
      return String(value);
  }
}

// ─── Component ────────────────────────────────────────────────────────────────

const CustomFieldRenderer: React.FC<CustomFieldRendererProps> = ({
  field,
  value,
  onChange,
  readOnly = false,
}) => {
  // ── Read-only mode ────────────────────────────────────────────────────────
  if (readOnly) {
    return (
      <Box sx={{ mb: 1 }}>
        <Typography variant="caption" color="text.secondary" display="block">
          {field.label}
          {field.required && (
            <Typography component="span" color="error" ml={0.25}>
              *
            </Typography>
          )}
        </Typography>
        <Typography variant="body2">{displayValue(field, value)}</Typography>
      </Box>
    );
  }

  const commonTextProps = {
    label: field.label,
    fullWidth: true,
    required: field.required,
    placeholder: field.placeholder,
    size: 'small' as const,
  };

  // ── Editable mode by field type ───────────────────────────────────────────

  switch (field.type) {
    case 'text':
      return (
        <TextField
          {...commonTextProps}
          value={typeof value === 'string' ? value : ''}
          onChange={(e) => onChange?.(e.target.value)}
        />
      );

    case 'textarea':
      return (
        <TextField
          {...commonTextProps}
          multiline
          minRows={3}
          value={typeof value === 'string' ? value : ''}
          onChange={(e) => onChange?.(e.target.value)}
        />
      );

    case 'number':
      return (
        <TextField
          {...commonTextProps}
          type="number"
          value={value ?? ''}
          onChange={(e) => {
            const parsed = parseFloat(e.target.value);
            onChange?.(isNaN(parsed) ? '' : parsed);
          }}
          inputProps={{ step: 'any' }}
        />
      );

    case 'date':
      return (
        <LocalizationProvider dateAdapter={AdapterDateFns}>
          <DatePicker
            label={field.label}
            value={value ? parseISO(String(value)) : null}
            onChange={(date: Date | null) =>
              onChange?.(date ? date.toISOString().split('T')[0] : null)
            }
            slotProps={{
              textField: {
                size: 'small',
                fullWidth: true,
                required: field.required,
                placeholder: field.placeholder,
              },
            }}
          />
        </LocalizationProvider>
      );

    case 'select': {
      const selectValue = value !== null && value !== undefined ? String(value) : '';
      return (
        <FormControl fullWidth size="small" required={field.required}>
          <InputLabel>{field.label}</InputLabel>
          <Select
            label={field.label}
            value={selectValue}
            onChange={(e: SelectChangeEvent<string>) => onChange?.(e.target.value)}
          >
            {!field.required && (
              <MenuItem value="">
                <em>None</em>
              </MenuItem>
            )}
            {(field.options ?? []).map((opt) => (
              <MenuItem key={opt.value} value={opt.value}>
                {opt.label}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      );
    }

    case 'multiselect': {
      const selectedValues: string[] = Array.isArray(value)
        ? (value as string[])
        : value
        ? [String(value)]
        : [];

      return (
        <Autocomplete
          multiple
          options={field.options ?? []}
          getOptionLabel={(opt) => opt.label}
          value={(field.options ?? []).filter((o) => selectedValues.includes(o.value))}
          onChange={(_, newVal) => onChange?.(newVal.map((v) => v.value))}
          isOptionEqualToValue={(opt, val) => opt.value === val.value}
          renderInput={(params) => (
            <TextField
              {...params}
              label={field.label}
              size="small"
              required={field.required}
              placeholder={field.placeholder}
            />
          )}
          renderOption={(props, option, { selected }) => (
            <li {...props}>
              <Checkbox size="small" checked={selected} sx={{ mr: 1, p: 0 }} />
              {option.label}
            </li>
          )}
          renderTags={(tagValues, getTagProps) =>
            tagValues.map((option, index) => (
              <Chip
                label={option.label}
                size="small"
                {...getTagProps({ index })}
                key={option.value}
              />
            ))
          }
        />
      );
    }

    case 'boolean':
      return (
        <FormControlLabel
          control={
            <Switch
              checked={Boolean(value)}
              onChange={(e) => onChange?.(e.target.checked)}
              size="small"
            />
          }
          label={
            <Typography variant="body2">
              {field.label}
              {field.required && (
                <Typography component="span" color="error" ml={0.25}>
                  *
                </Typography>
              )}
            </Typography>
          }
        />
      );

    default:
      return (
        <TextField
          {...commonTextProps}
          value={value !== null && value !== undefined ? String(value) : ''}
          onChange={(e) => onChange?.(e.target.value)}
        />
      );
  }
};

export default CustomFieldRenderer;

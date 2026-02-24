/**/**




















































































































































































































































































































































































export default EditableCell;};  );    </Box>      )}        </IconButton>          <EditIcon fontSize="small" />        >          aria-label="Edit"          }}            ml: 0.5,            transition: 'opacity 0.2s',            opacity: 0,          sx={{          }}            setIsEditing(true);            e.stopPropagation();          onClick={(e) => {          className="edit-icon"          size="small"        <IconButton      {showEditIcon && !disabled && (            </Typography>        {renderDisplay()}      <Typography variant="body2" noWrap>    >      aria-label={`${renderDisplay()}. ${!disabled ? (editOnDoubleClick ? 'Double-click to edit' : 'Click to edit') : ''}`}      }}        }          setIsEditing(true);          e.preventDefault();        if ((e.key === 'Enter' || e.key === ' ') && (editOnClick || editOnDoubleClick)) {      onKeyDown={(e) => {      tabIndex={editOnClick || editOnDoubleClick ? 0 : undefined}      role={editOnClick || editOnDoubleClick ? 'button' : undefined}      }}        } : {},          },            opacity: 1,          '& .edit-icon': {          borderRadius: 1,          backgroundColor: theme.palette.action.hover,        '&:hover': !disabled ? {        minHeight: 32,        cursor: disabled ? 'default' : editOnClick ? 'pointer' : editOnDoubleClick ? 'pointer' : 'default',        justifyContent: 'space-between',        alignItems: 'center',        display: 'flex',      sx={{      onDoubleClick={handleDoubleClick}      onClick={handleClick}    <Box  return (  // Render display mode  }    );      </ClickAwayListener>        </Box>          </Stack>            </Tooltip>              </IconButton>                <CloseIcon fontSize="small" />              >                aria-label="Cancel"                disabled={isSaving}                onClick={handleCancel}                size="small"              <IconButton            <Tooltip title="Cancel (Escape)">            </Tooltip>              </IconButton>                <CheckIcon fontSize="small" />              >                aria-label="Save"                color="primary"                disabled={isSaving}                onClick={handleSave}                size="small"              <IconButton            <Tooltip title="Save (Enter)">          <Stack direction="row" spacing={0.5}>          )}            />              }}                },                  maxLength,                  minLength,                  max,                  min,                inputProps: {              InputProps={{              fullWidth={fullWidth}              size={size}              helperText={error}              error={!!error}              disabled={isSaving}              placeholder={placeholder}              type={type === 'number' ? 'number' : 'text'}              }}                setValue(newValue);                  : e.target.value;                  ? e.target.value === '' ? null : Number(e.target.value)                const newValue = type === 'number'               onChange={(e) => {              value={value ?? ''}              inputRef={inputRef}            <TextField          ) : (            </LocalizationProvider>              />                }}                  },                    helperText: error,                    error: !!error,                    fullWidth,                    size,                  textField: {                slotProps={{                disabled={isSaving}                onChange={(newValue) => setValue(newValue)}                value={value ? new Date(value as string | number | Date) : null}              <DatePicker            <LocalizationProvider dateAdapter={AdapterDateFns}>          ) : type === 'date' ? (            </FormControl>              </Select>                ))}                  </MenuItem>                    {option.label}                  <MenuItem key={option.value} value={option.value}>                {options.map((option) => (              >                autoFocus                disabled={isSaving}                onChange={(e) => setValue(e.target.value)}                value={value ?? ''}              <Select            <FormControl size={size} fullWidth={fullWidth} error={!!error}>          {type === 'select' ? (        >          onKeyDown={handleKeyDown}          }}            gap: 0.5,            alignItems: 'center',            display: 'flex',          sx={{        <Box      <ClickAwayListener onClickAway={handleCancel}>    return (  if (isEditing) {  // Render edit mode  };    return String(initialValue ?? '');    }      return initialValue ? 'Yes' : 'No';    if (type === 'boolean') {    }      return new Date(initialValue as string | number | Date).toLocaleDateString();    if (type === 'date' && initialValue) {    }      return option?.label ?? String(initialValue ?? '');      const option = options.find((o) => o.value === initialValue);    if (type === 'select' && options.length > 0) {    }      return displayFormat(initialValue);    if (displayFormat) {  const renderDisplay = (): React.ReactNode => {  // Render display value  };    }      setIsEditing(true);    if (!disabled && editOnDoubleClick) {  const handleDoubleClick = () => {  };    }      setIsEditing(true);    if (!disabled && editOnClick) {  const handleClick = () => {  // Handle click/double-click to edit  }, [handleSave, handleCancel]);    }      handleCancel();      e.preventDefault();    } else if (e.key === 'Escape') {      handleSave();      e.preventDefault();    if (e.key === 'Enter' && !e.shiftKey) {  const handleKeyDown = useCallback((e: KeyboardEvent<HTMLDivElement>) => {  // Handle keyboard  }, [initialValue, onCancel]);    onCancel?.();    setError(null);    setIsEditing(false);    setValue(initialValue);  const handleCancel = useCallback(() => {  // Handle cancel  }, [value, validate, onSave]);    }      setIsSaving(false);    } finally {      setError(err instanceof Error ? err.message : 'Failed to save');    } catch (err) {      setError(null);      setIsEditing(false);      await onSave(value);    try {    setIsSaving(true);    }      return;      setError(validationError);    if (validationError) {    const validationError = validate(value);  const handleSave = useCallback(async () => {  // Handle save  }, [required, type, minLength, maxLength, min, max, pattern, customValidation]);    return null;    }      return customValidation(val);    if (customValidation) {    // Custom validation    }      }        return `Maximum value is ${max}`;      if (max !== undefined && val > max) {      }        return `Minimum value is ${min}`;      if (min !== undefined && val < min) {    if (type === 'number' && typeof val === 'number') {    }      }        return 'Invalid format';      if (pattern && !pattern.test(val)) {      }        return `Maximum length is ${maxLength} characters`;      if (maxLength && val.length > maxLength) {      }        return `Minimum length is ${minLength} characters`;      if (minLength && val.length < minLength) {    if (type === 'text' && typeof val === 'string') {    // Type-specific validation    }      return 'This field is required';    if (required && (val === null || val === undefined || val === '')) {    // Required check  const validate = useCallback((val: unknown): string | null => {  // Validate value  }, [isEditing]);    }      inputRef.current.select?.();      inputRef.current.focus();    if (isEditing && inputRef.current) {  useEffect(() => {  // Focus input when editing starts  }, [initialValue]);    setValue(initialValue);  useEffect(() => {  // Sync with initial value  const inputRef = useRef<HTMLInputElement>(null);  const [isSaving, setIsSaving] = useState(false);  const [error, setError] = useState<string | null>(null);  const [value, setValue] = useState(initialValue);  const [isEditing, setIsEditing] = useState(false);  const theme = useTheme();}) => {  size = 'small',  fullWidth = true,  showEditIcon = true,  editOnDoubleClick = true,  editOnClick = false,  displayFormat,  placeholder,  customValidation,  pattern,  max,  min,  maxLength,  minLength,  required = false,  options = [],  disabled = false,  onCancel,  onSave,  type = 'text',  value: initialValue,export const EditableCell: React.FC<EditableCellProps> = ({}  size?: 'small' | 'medium';  fullWidth?: boolean;  // Styling  showEditIcon?: boolean;  editOnDoubleClick?: boolean;  editOnClick?: boolean;  displayFormat?: (value: unknown) => React.ReactNode;  placeholder?: string;  // Display  customValidation?: (value: unknown) => string | null;  pattern?: RegExp;  max?: number;  min?: number;  maxLength?: number;  minLength?: number;  required?: boolean;  // Validation  options?: SelectOption[];  // Options for select type  disabled?: boolean;  onCancel?: () => void;  onSave: (value: unknown) => void | Promise<void>;  type?: EditableCellType;  value: unknown;export interface EditableCellProps {}  label: string;  value: string | number;export interface SelectOption {// Select optionexport type EditableCellType = 'text' | 'number' | 'date' | 'select' | 'boolean';// Field type} from '@mui/icons-material';  Edit as EditIcon,  Close as CloseIcon,  Check as CheckIcon,import {import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';import { DatePicker } from '@mui/x-date-pickers/DatePicker';} from '@mui/material';  Tooltip,  useTheme,  Typography,  Stack,  ClickAwayListener,  IconButton,  FormControl,  MenuItem,  Select,  TextField,  Box,import {import React, { useState, useRef, useEffect, useCallback, KeyboardEvent } from 'react'; */ * Supports text, number, date, and select field types * EditableCell - Inline editing component for data grids * EditableCell - Inline editing component for DataGrid cells
 * Supports text, number, date, and select field types
 */

import React, { useState, useRef, useEffect, useCallback, KeyboardEvent } from 'react';
import {
  Box,
  TextField,
  Select,
  MenuItem,
  Checkbox,
  IconButton,
  Stack,
  ClickAwayListener,
  useTheme,
  FormControl,
  InputLabel,
  SelectChangeEvent,
} from '@mui/material';
import {
  Check as CheckIcon,
  Close as CloseIcon,
} from '@mui/icons-material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';

// Editable field types
export type EditableFieldType = 'text' | 'number' | 'date' | 'select' | 'boolean' | 'email' | 'url';

// Select option
export interface SelectOption {
  value: string | number;
  label: string;
  disabled?: boolean;
}

export interface EditableCellProps {
  value: unknown;
  type: EditableFieldType;
  // Editing state
  editing?: boolean;
  onStartEdit?: () => void;
  onEndEdit?: () => void;
  // Save/Cancel
  onSave: (value: unknown) => void;
  onCancel: () => void;
  // Validation
  required?: boolean;
  minLength?: number;
  maxLength?: number;
  min?: number;
  max?: number;
  pattern?: RegExp;
  validate?: (value: unknown) => string | undefined;
  // Select options
  options?: SelectOption[];
  // Number formatting
  decimalPlaces?: number;
  // Accessibility
  ariaLabel?: string;
  // Behavior
  saveOnBlur?: boolean;
  saveOnEnter?: boolean;
  showButtons?: boolean;
  autoFocus?: boolean;
  // Styling
  fullWidth?: boolean;
  size?: 'small' | 'medium';
}

export const EditableCell: React.FC<EditableCellProps> = ({
  value: initialValue,
  type,
  editing: controlledEditing,
  onStartEdit,
  onEndEdit,
  onSave,
  onCancel,
  required = false,
  minLength,
  maxLength,
  min,
  max,
  pattern,
  validate,
  options = [],
  decimalPlaces,
  ariaLabel,
  saveOnBlur = true,
  saveOnEnter = true,
  showButtons = true,
  autoFocus = true,
  fullWidth = true,
  size = 'small',
}) => {
  const theme = useTheme();
  const inputRef = useRef<HTMLInputElement>(null);
  const [value, setValue] = useState<unknown>(initialValue);
  const [error, setError] = useState<string | undefined>();
  const [isEditing, setIsEditing] = useState(controlledEditing ?? false);

  // Sync with controlled editing prop
  useEffect(() => {
    if (controlledEditing !== undefined) {
      setIsEditing(controlledEditing);
    }
  }, [controlledEditing]);

  // Focus input when editing starts
  useEffect(() => {
    if (isEditing && autoFocus && inputRef.current) {
      inputRef.current.focus();
      inputRef.current.select?.();
    }
  }, [isEditing, autoFocus]);

  // Reset value when editing starts
  useEffect(() => {
    if (isEditing) {
      setValue(initialValue);
      setError(undefined);
    }
  }, [isEditing, initialValue]);

  // Validate value
  const validateValue = useCallback((val: unknown): string | undefined => {
    // Custom validation
    if (validate) {
      const customError = validate(val);
      if (customError) return customError;
    }

    // Required validation
    if (required && (val === null || val === undefined || val === '')) {
      return 'This field is required';
    }

    // String validations
    if (type === 'text' || type === 'email' || type === 'url') {
      const strVal = String(val ?? '');
      if (minLength && strVal.length < minLength) {
        return `Minimum ${minLength} characters`;
      }
      if (maxLength && strVal.length > maxLength) {
        return `Maximum ${maxLength} characters`;
      }
      if (pattern && !pattern.test(strVal)) {
        return 'Invalid format';
      }
      if (type === 'email' && strVal && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(strVal)) {
        return 'Invalid email address';
      }
      if (type === 'url' && strVal && !/^https?:\/\/.+/.test(strVal)) {
        return 'Invalid URL';
      }
    }

    // Number validations
    if (type === 'number') {
      const numVal = Number(val);
      if (isNaN(numVal)) {
        return 'Invalid number';
      }
      if (min !== undefined && numVal < min) {
        return `Minimum value is ${min}`;
      }
      if (max !== undefined && numVal > max) {
        return `Maximum value is ${max}`;
      }
    }

    return undefined;
  }, [required, minLength, maxLength, min, max, pattern, validate, type]);

  // Handle save
  const handleSave = useCallback(() => {
    const validationError = validateValue(value);
    if (validationError) {
      setError(validationError);
      return;
    }

    // Format value based on type
    let formattedValue = value;
    if (type === 'number' && decimalPlaces !== undefined) {
      formattedValue = Number(Number(value).toFixed(decimalPlaces));
    }

    onSave(formattedValue);
    setIsEditing(false);
    onEndEdit?.();
  }, [value, validateValue, type, decimalPlaces, onSave, onEndEdit]);

  // Handle cancel
  const handleCancel = useCallback(() => {
    setValue(initialValue);
    setError(undefined);
    setIsEditing(false);
    onCancel();
    onEndEdit?.();
  }, [initialValue, onCancel, onEndEdit]);

  // Handle keyboard
  const handleKeyDown = useCallback((e: KeyboardEvent) => {
    if (e.key === 'Enter' && saveOnEnter) {
      e.preventDefault();
      handleSave();
    } else if (e.key === 'Escape') {
      e.preventDefault();
      handleCancel();
    }
  }, [saveOnEnter, handleSave, handleCancel]);

  // Handle click away
  const handleClickAway = useCallback(() => {
    if (saveOnBlur) {
      handleSave();
    } else {
      handleCancel();
    }
  }, [saveOnBlur, handleSave, handleCancel]);

  // Start editing on double click
  const handleDoubleClick = useCallback(() => {
    if (!isEditing) {
      setIsEditing(true);
      onStartEdit?.();
    }
  }, [isEditing, onStartEdit]);

  // Render display value
  if (!isEditing) {
    return (
      <Box
        onDoubleClick={handleDoubleClick}
        sx={{
          cursor: 'pointer',
          minHeight: 24,
          display: 'flex',
          alignItems: 'center',
          '&:hover': {
            backgroundColor: theme.palette.action.hover,
          },
        }}
        role="button"
        tabIndex={0}
        onKeyDown={(e) => {
          if (e.key === 'Enter' || e.key === ' ') {
            handleDoubleClick();
          }
        }}
        aria-label={ariaLabel || 'Double-click to edit'}
      >
        {type === 'boolean' ? (
          <Checkbox checked={Boolean(initialValue)} disabled size={size} />
        ) : type === 'select' ? (
          options.find((o) => o.value === initialValue)?.label ?? String(initialValue ?? '')
        ) : type === 'date' ? (
          initialValue ? new Date(initialValue as string).toLocaleDateString() : ''
        ) : (
          String(initialValue ?? '')
        )}
      </Box>
    );
  }

  // Render editor
  const renderEditor = () => {
    switch (type) {
      case 'boolean':
        return (
          <Checkbox
            checked={Boolean(value)}
            onChange={(e) => setValue(e.target.checked)}
            inputProps={{ 'aria-label': ariaLabel || 'Edit boolean value' }}
            size={size}
          />
        );

      case 'select':
        return (
          <FormControl fullWidth={fullWidth} size={size} error={!!error}>
            <Select
              value={value as string | number}
              onChange={(e: SelectChangeEvent<string | number>) => setValue(e.target.value)}
              inputRef={inputRef}
              onKeyDown={handleKeyDown}
              aria-label={ariaLabel}
            >
              {options.map((option) => (
                <MenuItem
                  key={option.value}
                  value={option.value}
                  disabled={option.disabled}
                >
                  {option.label}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        );

      case 'date':
        return (
          <LocalizationProvider dateAdapter={AdapterDateFns}>
            <DatePicker
              value={value ? new Date(value as string) : null}
              onChange={(newValue) => setValue(newValue?.toISOString())}
              slotProps={{
                textField: {
                  size,
                  fullWidth,
                  error: !!error,
                  helperText: error,
                  onKeyDown: handleKeyDown,
                  inputRef,
                },
              }}
            />
          </LocalizationProvider>
        );

      case 'number':
        return (
          <TextField
            inputRef={inputRef}
            type="number"
            value={value ?? ''}
            onChange={(e) => setValue(e.target.value ? Number(e.target.value) : null)}
            onKeyDown={handleKeyDown}
            size={size}
            fullWidth={fullWidth}
            error={!!error}
            helperText={error}
            inputProps={{
              min,
              max,
              step: decimalPlaces ? Math.pow(10, -decimalPlaces) : 1,
              'aria-label': ariaLabel,
            }}
          />
        );

      default:
        return (
          <TextField
            inputRef={inputRef}
            type={type === 'email' ? 'email' : type === 'url' ? 'url' : 'text'}
            value={value ?? ''}
            onChange={(e) => setValue(e.target.value)}
            onKeyDown={handleKeyDown}
            size={size}
            fullWidth={fullWidth}
            error={!!error}
            helperText={error}
            inputProps={{
              minLength,
              maxLength,
              'aria-label': ariaLabel,
            }}
          />
        );
    }
  };

  return (
    <ClickAwayListener onClickAway={handleClickAway}>
      <Stack direction="row" spacing={0.5} alignItems="flex-start">
        {renderEditor()}
        {showButtons && (
          <Stack direction="row" spacing={0.5}>
            <IconButton
              size="small"
              onClick={handleSave}
              color="primary"
              aria-label="Save"
              sx={{ p: 0.5 }}
            >
              <CheckIcon fontSize="small" />
            </IconButton>
            <IconButton
              size="small"
              onClick={handleCancel}
              color="default"
              aria-label="Cancel"
              sx={{ p: 0.5 }}
            >
              <CloseIcon fontSize="small" />
            </IconButton>
          </Stack>
        )}
      </Stack>
    </ClickAwayListener>
  );
};

export default EditableCell;

// Catalog Request Form - Dynamic form builder for catalog items
// Part of ITSM Enhancement Plan - Phase 4.2

import React, { useState, useCallback, useMemo } from 'react';
import {
  Box,
  Paper,
  Typography,
  TextField,
  FormControl,
  FormLabel,
  FormControlLabel,
  FormHelperText,
  RadioGroup,
  Radio,
  Checkbox,
  Select,
  MenuItem,
  InputLabel,
  Stack,
  Button,
  Divider,
  Alert,
  Card,
  CardContent,
  Chip,
  IconButton,
  Tooltip,
  Stepper,
  Step,
  StepLabel,
  InputAdornment,
  Autocomplete,
} from '@mui/material';
import { DatePicker, DateTimePicker } from '@mui/x-date-pickers';
import {
  Add as AddIcon,
  Remove as RemoveIcon,
  AttachFile as AttachIcon,
  Help as HelpIcon,
  AccessTime as TimeIcon,
  ShoppingCart as CartIcon,
  ArrowBack as BackIcon,
  ArrowForward as NextIcon,
  Send as SubmitIcon,
} from '@mui/icons-material';

export type FieldType =
  | 'text'
  | 'textarea'
  | 'number'
  | 'email'
  | 'phone'
  | 'date'
  | 'datetime'
  | 'select'
  | 'multiselect'
  | 'radio'
  | 'checkbox'
  | 'checkboxGroup'
  | 'file'
  | 'user'
  | 'group';

export interface FieldOption {
  value: string;
  label: string;
  description?: string;
  additionalCost?: number;
}

export interface FormField {
  id: string;
  name: string;
  type: FieldType;
  label: string;
  placeholder?: string;
  description?: string;
  required?: boolean;
  defaultValue?: unknown;
  options?: FieldOption[];
  validation?: {
    min?: number;
    max?: number;
    minLength?: number;
    maxLength?: number;
    pattern?: string;
    patternMessage?: string;
  };
  conditionalOn?: {
    field: string;
    value: unknown;
    operator?: 'equals' | 'notEquals' | 'contains' | 'greaterThan' | 'lessThan';
  };
  section?: string;
  gridSize?: 6 | 12;
}

export interface CatalogItemDetails {
  id: number;
  name: string;
  description: string;
  categoryName: string;
  estimatedDelivery?: string;
  price?: number;
  icon?: string;
  approvalRequired?: boolean;
  fields: FormField[];
  sections?: string[];
}

export interface CatalogRequestFormProps {
  catalogItem: CatalogItemDetails;
  onSubmit?: (data: Record<string, unknown>) => void;
  onSaveDraft?: (data: Record<string, unknown>) => void;
  onCancel?: () => void;
  initialData?: Record<string, unknown>;
  showSummary?: boolean;
  variant?: 'single' | 'wizard';
  users?: { id: string; name: string; email: string }[];
  groups?: { id: string; name: string }[];
}

interface FormErrors {
  [key: string]: string;
}

const validateField = (
  field: FormField,
  value: unknown
): string | null => {
  if (field.required) {
    if (value === undefined || value === null || value === '') {
      return `${field.label} is required`;
    }
    if (Array.isArray(value) && value.length === 0) {
      return `At least one ${field.label} must be selected`;
    }
  }

  if (!value) return null;

  const { validation } = field;
  if (!validation) return null;

  if (field.type === 'number' || field.type === 'text' || field.type === 'textarea') {
    const strValue = String(value);
    if (validation.minLength && strValue.length < validation.minLength) {
      return `${field.label} must be at least ${validation.minLength} characters`;
    }
    if (validation.maxLength && strValue.length > validation.maxLength) {
      return `${field.label} must be at most ${validation.maxLength} characters`;
    }
  }

  if (field.type === 'number') {
    const numValue = Number(value);
    if (validation.min !== undefined && numValue < validation.min) {
      return `${field.label} must be at least ${validation.min}`;
    }
    if (validation.max !== undefined && numValue > validation.max) {
      return `${field.label} must be at most ${validation.max}`;
    }
  }

  if (validation.pattern) {
    const regex = new RegExp(validation.pattern);
    if (!regex.test(String(value))) {
      return validation.patternMessage || `${field.label} format is invalid`;
    }
  }

  if (field.type === 'email' && value) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/; // NOSONAR - safe regex, character class [^\s@]+ prevents catastrophic backtracking
    if (!emailRegex.test(String(value))) {
      return 'Please enter a valid email address';
    }
  }

  return null;
};

// Field renderer component
const FormFieldRenderer: React.FC<{
  field: FormField;
  value: unknown;
  error?: string;
  onChange: (value: unknown) => void;
  users?: { id: string; name: string; email: string }[];
  groups?: { id: string; name: string }[];
}> = ({ field, value, error, onChange, users = [], groups = [] }) => {
  const commonProps = {
    fullWidth: true,
    error: !!error,
    helperText: error || field.description,
    required: field.required,
    placeholder: field.placeholder,
  };

  switch (field.type) {
    case 'text':
    case 'email':
    case 'phone':
      return (
        <TextField
          {...commonProps}
          type={field.type === 'email' ? 'email' : 'text'}
          label={field.label}
          value={value || ''}
          onChange={(e) => onChange(e.target.value)}
          inputProps={{
            maxLength: field.validation?.maxLength,
          }}
          InputProps={{
            endAdornment: field.description && (
              <Tooltip title={field.description}>
                <HelpIcon color="action" fontSize="small" />
              </Tooltip>
            ),
          }}
        />
      );

    case 'textarea':
      return (
        <TextField
          {...commonProps}
          label={field.label}
          value={value || ''}
          onChange={(e) => onChange(e.target.value)}
          multiline
          rows={4}
          inputProps={{
            maxLength: field.validation?.maxLength,
          }}
        />
      );

    case 'number':
      return (
        <TextField
          {...commonProps}
          type="number"
          label={field.label}
          value={value || ''}
          onChange={(e) => onChange(Number(e.target.value))}
          inputProps={{
            min: field.validation?.min,
            max: field.validation?.max,
          }}
        />
      );

    case 'date':
      return (
        <DatePicker
          label={field.label}
          value={value as Date | null}
          onChange={(date) => onChange(date)}
          slotProps={{
            textField: {
              ...commonProps,
            },
          }}
        />
      );

    case 'datetime':
      return (
        <DateTimePicker
          label={field.label}
          value={value as Date | null}
          onChange={(date) => onChange(date)}
          slotProps={{
            textField: {
              ...commonProps,
            },
          }}
        />
      );

    case 'select':
      return (
        <FormControl fullWidth error={!!error} required={field.required}>
          <InputLabel>{field.label}</InputLabel>
          <Select
            value={value || ''}
            label={field.label}
            onChange={(e) => onChange(e.target.value)}
          >
            {field.options?.map((option) => (
              <MenuItem key={option.value} value={option.value}>
                <Stack direction="row" justifyContent="space-between" width="100%">
                  <span>{option.label}</span>
                  {option.additionalCost && (
                    <Chip
                      label={`+$${option.additionalCost}`}
                      size="small"
                      color="secondary"
                    />
                  )}
                </Stack>
              </MenuItem>
            ))}
          </Select>
          {(error || field.description) && (
            <FormHelperText>{error || field.description}</FormHelperText>
          )}
        </FormControl>
      );

    case 'multiselect':
      return (
        <FormControl fullWidth error={!!error} required={field.required}>
          <InputLabel>{field.label}</InputLabel>
          <Select
            multiple
            value={(value as string[]) || []}
            label={field.label}
            onChange={(e) => onChange(e.target.value)}
            renderValue={(selected) => (
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                {(selected as string[]).map((val) => {
                  const option = field.options?.find((o) => o.value === val);
                  return <Chip key={val} label={option?.label || val} size="small" />;
                })}
              </Box>
            )}
          >
            {field.options?.map((option) => (
              <MenuItem key={option.value} value={option.value}>
                {option.label}
              </MenuItem>
            ))}
          </Select>
          {(error || field.description) && (
            <FormHelperText>{error || field.description}</FormHelperText>
          )}
        </FormControl>
      );

    case 'radio':
      return (
        <FormControl error={!!error} required={field.required}>
          <FormLabel>{field.label}</FormLabel>
          <RadioGroup
            value={value || ''}
            onChange={(e) => onChange(e.target.value)}
          >
            {field.options?.map((option) => (
              <FormControlLabel
                key={option.value}
                value={option.value}
                control={<Radio />}
                label={
                  <Stack>
                    <Stack direction="row" spacing={1} alignItems="center">
                      <Typography>{option.label}</Typography>
                      {option.additionalCost && (
                        <Chip
                          label={`+$${option.additionalCost}`}
                          size="small"
                          color="secondary"
                        />
                      )}
                    </Stack>
                    {option.description && (
                      <Typography variant="caption" color="text.secondary">
                        {option.description}
                      </Typography>
                    )}
                  </Stack>
                }
              />
            ))}
          </RadioGroup>
          {(error || field.description) && (
            <FormHelperText>{error || field.description}</FormHelperText>
          )}
        </FormControl>
      );

    case 'checkbox':
      return (
        <FormControlLabel
          control={
            <Checkbox
              checked={!!value}
              onChange={(e) => onChange(e.target.checked)}
            />
          }
          label={field.label}
        />
      );

    case 'checkboxGroup':
      return (
        <FormControl error={!!error} required={field.required}>
          <FormLabel>{field.label}</FormLabel>
          <Stack>
            {field.options?.map((option) => (
              <FormControlLabel
                key={option.value}
                control={
                  <Checkbox
                    checked={((value as string[]) || []).includes(option.value)}
                    onChange={(e) => {
                      const current = (value as string[]) || [];
                      if (e.target.checked) {
                        onChange([...current, option.value]);
                      } else {
                        onChange(current.filter((v) => v !== option.value));
                      }
                    }}
                  />
                }
                label={option.label}
              />
            ))}
          </Stack>
          {(error || field.description) && (
            <FormHelperText>{error || field.description}</FormHelperText>
          )}
        </FormControl>
      );

    case 'file':
      return (
        <FormControl fullWidth error={!!error}>
          <>
            <Button
              variant="outlined"
              component="label"
              startIcon={<AttachIcon />}
            >
              {field.label}
              <input
                type="file"
                hidden
                onChange={(e) => {
                  const files = e.target.files;
                  if (files && files.length > 0) {
                    onChange(files[0]);
                  }
                }}
              />
            </Button>
            {value && (
              <Typography variant="caption" sx={{ mt: 1 }}>
                Selected: {(value as File).name}
              </Typography>
            )}
            {(error || field.description) && (
              <FormHelperText>{error || field.description}</FormHelperText>
            )}
          </>
        </FormControl>
      );

    case 'user':
      return (
        <Autocomplete
          options={users}
          getOptionLabel={(option) => `${option.name} (${option.email})`}
          value={users.find((u) => u.id === value) || null}
          onChange={(_, newValue) => onChange(newValue?.id || null)}
          renderInput={(params) => (
            <TextField
              {...params}
              {...commonProps}
              label={field.label}
            />
          )}
        />
      );

    case 'group':
      return (
        <Autocomplete
          options={groups}
          getOptionLabel={(option) => option.name}
          value={groups.find((g) => g.id === value) || null}
          onChange={(_, newValue) => onChange(newValue?.id || null)}
          renderInput={(params) => (
            <TextField
              {...params}
              {...commonProps}
              label={field.label}
            />
          )}
        />
      );

    default:
      return null;
  }
};

export const CatalogRequestForm: React.FC<CatalogRequestFormProps> = ({
  catalogItem,
  onSubmit,
  onSaveDraft,
  onCancel,
  initialData = {},
  showSummary = true,
  variant = 'single',
  users = [],
  groups = [],
}) => {
  const [formData, setFormData] = useState<Record<string, unknown>>(initialData);
  const [errors, setErrors] = useState<FormErrors>({});
  const [activeStep, setActiveStep] = useState(0);
  const [touched, setTouched] = useState<Set<string>>(new Set());

  const sections = catalogItem.sections || ['General'];

  // Check if field is visible based on conditions
  const isFieldVisible = useCallback(
    (field: FormField): boolean => {
      if (!field.conditionalOn) return true;

      const { field: condField, value: condValue, operator = 'equals' } = field.conditionalOn;
      const currentValue = formData[condField];

      switch (operator) {
        case 'equals':
          return currentValue === condValue;
        case 'notEquals':
          return currentValue !== condValue;
        case 'contains':
          return Array.isArray(currentValue) && currentValue.includes(condValue);
        case 'greaterThan':
          return Number(currentValue) > Number(condValue);
        case 'lessThan':
          return Number(currentValue) < Number(condValue);
        default:
          return true;
      }
    },
    [formData]
  );

  // Get visible fields
  const visibleFields = useMemo(
    () => catalogItem.fields.filter(isFieldVisible),
    [catalogItem.fields, isFieldVisible]
  );

  // Get fields by section for wizard mode
  const fieldsBySection = useMemo(() => {
    const grouped: Record<string, FormField[]> = {};
    sections.forEach((section) => {
      grouped[section] = visibleFields.filter(
        (f) => (f.section || 'General') === section
      );
    });
    return grouped;
  }, [visibleFields, sections]);

  // Calculate total cost including options
  const totalCost = useMemo(() => {
    let cost = catalogItem.price || 0;
    visibleFields.forEach((field) => {
      if (field.options) {
        const value = formData[field.name];
        if (Array.isArray(value)) {
          value.forEach((v) => {
            const option = field.options?.find((o) => o.value === v);
            if (option?.additionalCost) cost += option.additionalCost;
          });
        } else if (value) {
          const option = field.options.find((o) => o.value === value);
          if (option?.additionalCost) cost += option.additionalCost;
        }
      }
    });
    return cost;
  }, [catalogItem.price, visibleFields, formData]);

  const handleChange = useCallback((fieldName: string, value: unknown) => {
    setFormData((prev) => ({ ...prev, [fieldName]: value }));
    setTouched((prev) => new Set(prev).add(fieldName));
  }, []);

  const validateAll = useCallback((): boolean => {
    const newErrors: FormErrors = {};
    let isValid = true;

    visibleFields.forEach((field) => {
      const error = validateField(field, formData[field.name]);
      if (error) {
        newErrors[field.name] = error;
        isValid = false;
      }
    });

    setErrors(newErrors);
    return isValid;
  }, [visibleFields, formData]);

  const validateSection = useCallback(
    (sectionIndex: number): boolean => {
      const sectionName = sections[sectionIndex];
      const sectionFields = fieldsBySection[sectionName] || [];
      const newErrors: FormErrors = { ...errors };
      let isValid = true;

      sectionFields.forEach((field) => {
        const error = validateField(field, formData[field.name]);
        if (error) {
          newErrors[field.name] = error;
          isValid = false;
        } else {
          delete newErrors[field.name];
        }
      });

      setErrors(newErrors);
      return isValid;
    },
    [sections, fieldsBySection, errors, formData]
  );

  const handleSubmit = () => {
    if (validateAll()) {
      onSubmit?.(formData);
    }
  };

  const handleNext = () => {
    if (validateSection(activeStep)) {
      setActiveStep((prev) => prev + 1);
    }
  };

  const handleBack = () => {
    setActiveStep((prev) => prev - 1);
  };

  const renderFields = (fields: FormField[]) => (
    <Stack spacing={3}>
      {fields.map((field) => (
        <Box key={field.id} sx={{ gridColumn: `span ${field.gridSize || 12}` }}>
          <FormFieldRenderer
            field={field}
            value={formData[field.name]}
            error={touched.has(field.name) ? errors[field.name] : undefined}
            onChange={(value) => handleChange(field.name, value)}
            users={users}
            groups={groups}
          />
        </Box>
      ))}
    </Stack>
  );

  const renderSummary = () => (
    <Card variant="outlined" sx={{ mt: 3 }}>
      <CardContent>
        <Typography variant="subtitle1" fontWeight={600} gutterBottom>
          Request Summary
        </Typography>
        <Divider sx={{ my: 1 }} />
        <Stack spacing={1}>
          <Stack direction="row" justifyContent="space-between">
            <Typography variant="body2" color="text.secondary">
              Catalog Item
            </Typography>
            <Typography variant="body2">{catalogItem.name}</Typography>
          </Stack>
          {catalogItem.estimatedDelivery && (
            <Stack direction="row" justifyContent="space-between">
              <Typography variant="body2" color="text.secondary">
                <TimeIcon sx={{ fontSize: 14, mr: 0.5, verticalAlign: 'middle' }} />
                Estimated Delivery
              </Typography>
              <Typography variant="body2">{catalogItem.estimatedDelivery}</Typography>
            </Stack>
          )}
          {totalCost > 0 && (
            <Stack direction="row" justifyContent="space-between">
              <Typography variant="body2" color="text.secondary">
                Total Cost
              </Typography>
              <Typography variant="body2" fontWeight={600}>
                ${totalCost.toFixed(2)}
              </Typography>
            </Stack>
          )}
          {catalogItem.approvalRequired && (
            <Alert severity="info" sx={{ mt: 1 }}>
              This request requires manager approval
            </Alert>
          )}
        </Stack>
      </CardContent>
    </Card>
  );

  return (
    <Paper sx={{ p: 3 }}>
      {/* Header */}
      <Stack direction="row" alignItems="flex-start" spacing={2} sx={{ mb: 3 }}>
        <CartIcon color="primary" sx={{ fontSize: 40 }} />
        <Box>
          <Typography variant="h6">{catalogItem.name}</Typography>
          <Typography variant="body2" color="text.secondary">
            {catalogItem.categoryName}
          </Typography>
          <Typography variant="body2" sx={{ mt: 1 }}>
            {catalogItem.description}
          </Typography>
        </Box>
      </Stack>

      <Divider sx={{ mb: 3 }} />

      {/* Form content */}
      {variant === 'wizard' && sections.length > 1 ? (
        <>
          <Stepper activeStep={activeStep} sx={{ mb: 3 }}>
            {sections.map((section) => (
              <Step key={section}>
                <StepLabel>{section}</StepLabel>
              </Step>
            ))}
          </Stepper>

          {renderFields(fieldsBySection[sections[activeStep]] || [])}

          <Stack direction="row" justifyContent="space-between" sx={{ mt: 3 }}>
            <Button
              disabled={activeStep === 0}
              onClick={handleBack}
              startIcon={<BackIcon />}
            >
              Back
            </Button>
            {activeStep < sections.length - 1 ? (
              <Button
                variant="contained"
                onClick={handleNext}
                endIcon={<NextIcon />}
              >
                Next
              </Button>
            ) : (
              <Button
                variant="contained"
                onClick={handleSubmit}
                startIcon={<SubmitIcon />}
              >
                Submit Request
              </Button>
            )}
          </Stack>
        </>
      ) : (
        <>
          {sections.map((section, index) => (
            <Box key={section} sx={{ mb: 3 }}>
              {sections.length > 1 && (
                <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 2 }}>
                  {section}
                </Typography>
              )}
              {renderFields(fieldsBySection[section] || [])}
              {index < sections.length - 1 && <Divider sx={{ mt: 3 }} />}
            </Box>
          ))}

          {showSummary && renderSummary()}

          <Stack direction="row" justifyContent="flex-end" spacing={2} sx={{ mt: 3 }}>
            {onCancel && (
              <Button onClick={onCancel}>Cancel</Button>
            )}
            {onSaveDraft && (
              <Button variant="outlined" onClick={() => onSaveDraft(formData)}>
                Save Draft
              </Button>
            )}
            <Button
              variant="contained"
              onClick={handleSubmit}
              startIcon={<SubmitIcon />}
            >
              Submit Request
            </Button>
          </Stack>
        </>
      )}
    </Paper>
  );
};

export default CatalogRequestForm;

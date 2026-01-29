import React, { useEffect, useState, useMemo } from 'react';
import { 
  FormControl, InputLabel, Select, MenuItem, TextField, FormControlLabel, Checkbox,
  Box, Typography, InputAdornment, Slider, FormHelperText,
  Autocomplete
} from '@mui/material';
import {
  LinkedIn as LinkedInIcon,
  Twitter as TwitterIcon,
} from '@mui/icons-material';
import lookupService, { LookupItem } from '../services/lookupService';

interface SelectOption {
  value: string;
  label: string;
}

interface ModuleFieldConfiguration {
  id: number;
  moduleName?: string;
  fieldName: string;
  fieldLabel: string;
  fieldType: string;
  tabIndex?: number;
  displayOrder?: number;
  isEnabled?: boolean;
  isRequired?: boolean;
  gridSize?: number;
  options?: string | null;
  placeholder?: string;
  helpText?: string;
  parentField?: string | null;
  parentFieldValue?: string | null;
}

interface Props {
  config: ModuleFieldConfiguration;
  formData: any;
  onChange: (e: any) => void;
  onSelectChange: (e: any) => void;
  setFormData?: (fn: any) => void;
  disabled?: boolean;
}

const FieldRenderer: React.FC<Props> = ({ config, formData, onChange, onSelectChange, setFormData, disabled }) => {
  const [items, setItems] = useState<LookupItem[] | null>(null);

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      try {
        if (config.options && config.options.startsWith('lookup:')) {
          const parts = config.options.split(':');
          const category = parts[1] || parts.slice(1).join(':');
          const res = await lookupService.getLookupItems(category);
          if (!cancelled) setItems(res);
        }
      } catch (err) {
        if (!cancelled) setItems([]);
      }
    };
    load();
    return () => { cancelled = true; };
  }, [config.options]);

  const fieldValue = (formData as any)[config.fieldName];
  const commonProps = {
    fullWidth: true,
    label: config.fieldLabel,
    name: config.fieldName,
    value: fieldValue ?? '',
    onChange: onChange,
    required: config.isRequired,
    placeholder: config.placeholder || undefined,
    disabled: disabled,
    helperText: config.helpText,
    size: 'small' as const,
  } as any;

  // Hide category field completely - Individual/Organization toggle is no longer needed
  // All accounts are treated as organizations by default
  if (config.fieldName === 'category') {
    return null;
  }

  // Special handling for social media fields
  if (config.fieldName === 'linkedInUrl') {
    return (
      <TextField 
        {...commonProps}
        InputProps={{ 
          startAdornment: (
            <InputAdornment position="start">
              <LinkedInIcon sx={{ color: '#0077b5' }} />
            </InputAdornment>
          )
        }}
      />
    );
  }

  if (config.fieldName === 'twitterHandle') {
    return (
      <TextField 
        {...commonProps}
        InputProps={{ 
          startAdornment: (
            <InputAdornment position="start">
              <TwitterIcon sx={{ color: '#1da1f2' }} />
            </InputAdornment>
          )
        }}
      />
    );
  }

  // Handle lead score with slider
  if (config.fieldName === 'leadScore' && config.fieldType === 'number') {
    return (
      <Box>
        <Typography gutterBottom>{config.fieldLabel}: {fieldValue ?? 0}</Typography>
        <Slider
          value={fieldValue ?? 0}
          onChange={(_, v) => setFormData && setFormData((prev: any) => ({ ...prev, [config.fieldName]: v as number }))}
          min={0}
          max={100}
          valueLabelDisplay="auto"
          disabled={disabled}
          sx={{ color: (fieldValue ?? 0) > 70 ? '#4caf50' : (fieldValue ?? 0) > 40 ? '#ff9800' : '#f44336' }}
        />
        {config.helpText && <FormHelperText>{config.helpText}</FormHelperText>}
      </Box>
    );
  }

  // Select field handling - using Autocomplete for better UX
  if (config.fieldType === 'select') {
    const options: SelectOption[] = config.options && config.options.startsWith('lookup:') 
      ? (items || []).map(i => ({ value: i.key || i.value, label: i.value }))
      : (config.options || '').split(',').map(o => o.trim()).filter(Boolean).map(opt => ({ value: opt, label: opt }));

    const selectedOption = options.find(opt => opt.value === fieldValue) || null;

    return (
      <Autocomplete
        value={selectedOption}
        onChange={(_, newValue) => {
          // Create a synthetic event for compatibility with existing handlers
          const syntheticEvent = {
            target: {
              name: config.fieldName,
              value: newValue?.value ?? ''
            }
          };
          onSelectChange(syntheticEvent);
        }}
        options={options}
        getOptionLabel={(option) => option.label}
        isOptionEqualToValue={(option, value) => option.value === value.value}
        disabled={disabled}
        size="small"
        fullWidth
        renderInput={(params) => (
          <TextField
            {...params}
            label={config.fieldLabel}
            required={config.isRequired}
            helperText={config.helpText}
            placeholder={config.placeholder}
          />
        )}
        renderOption={(props, option) => (
          <li {...props} key={option.value}>
            {option.label}
          </li>
        )}
      />
    );
  }

  // Textarea handling
  if (config.fieldType === 'textarea') {
    return <TextField {...commonProps} multiline rows={3} />;
  }

  // Checkbox handling
  if (config.fieldType === 'checkbox') {
    return (
      <FormControlLabel
        control={
          <Checkbox 
            name={config.fieldName} 
            checked={Boolean(fieldValue)} 
            onChange={onChange}
            disabled={disabled}
          />
        }
        label={config.fieldLabel}
      />
    );
  }

  // Date handling
  if (config.fieldType === 'date') {
    return (
      <TextField 
        {...commonProps} 
        type="date"
        InputLabelProps={{ shrink: true }}
      />
    );
  }

  // Currency handling
  if (config.fieldType === 'currency') {
    return (
      <TextField 
        {...commonProps}
        type="number"
        InputProps={{
          startAdornment: <InputAdornment position="start">$</InputAdornment>
        }}
      />
    );
  }

  // Number handling
  if (config.fieldType === 'number') {
    return <TextField {...commonProps} type="number" />;
  }

  // Email handling
  if (config.fieldType === 'email') {
    return <TextField {...commonProps} type="email" />;
  }

  // Default text field
  return <TextField {...commonProps} type="text" />;
};

export default FieldRenderer;

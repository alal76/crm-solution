import React, { useEffect, useState } from 'react';
import { 
  FormControl, InputLabel, Select, MenuItem, TextField, FormControlLabel, Checkbox,
  Switch, Box, Typography, Paper, InputAdornment, Slider, FormHelperText
} from '@mui/material';
import {
  LinkedIn as LinkedInIcon,
  Twitter as TwitterIcon,
  Person as PersonIcon,
  Business as BusinessIcon
} from '@mui/icons-material';
import lookupService, { LookupItem } from '../services/lookupService';

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

  // Special handling for category field with switch
  if (config.fieldName === 'category') {
    return (
      <Paper elevation={0} sx={{ p: 2, backgroundColor: '#F5EFF7', borderRadius: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, opacity: formData.category === 0 ? 1 : 0.5 }}>
            <PersonIcon color={formData.category === 0 ? 'primary' : 'disabled'} />
            <Typography fontWeight={formData.category === 0 ? 600 : 400}>Individual</Typography>
          </Box>
          <Switch
            checked={formData.category === 1}
            onChange={(e) => setFormData && setFormData((prev: any) => ({ ...prev, category: e.target.checked ? 1 : 0 }))}
            disabled={disabled}
            color="primary"
          />
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, opacity: formData.category === 1 ? 1 : 0.5 }}>
            <BusinessIcon color={formData.category === 1 ? 'primary' : 'disabled'} />
            <Typography fontWeight={formData.category === 1 ? 600 : 400}>Organization</Typography>
          </Box>
        </Box>
        {disabled && (
          <Typography variant="caption" color="textSecondary" sx={{ display: 'block', textAlign: 'center', mt: 1 }}>
            Category cannot be changed after creation
          </Typography>
        )}
      </Paper>
    );
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

  // Select field handling
  if (config.fieldType === 'select') {
    const options = config.options && config.options.startsWith('lookup:') 
      ? (items || []).map(i => ({ value: i.key || i.value, label: i.value }))
      : (config.options || '').split(',').map(o => o.trim()).filter(Boolean).map(opt => ({ value: opt, label: opt }));

    return (
      <FormControl fullWidth size="small" required={config.isRequired} disabled={disabled}>
        <InputLabel>{config.fieldLabel}</InputLabel>
        <Select 
          name={config.fieldName} 
          value={fieldValue ?? ''} 
          onChange={onSelectChange} 
          label={config.fieldLabel}
        >
          {options.map(opt => (
            <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
          ))}
        </Select>
        {config.helpText && <FormHelperText>{config.helpText}</FormHelperText>}
      </FormControl>
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

import React, { useEffect, useState } from 'react';
import { FormControl, InputLabel, Select, MenuItem, TextField, FormControlLabel, Checkbox } from '@mui/material';
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
}

const FieldRenderer: React.FC<Props> = ({ config, formData, onChange, onSelectChange, setFormData }) => {
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

  const commonProps = {
    fullWidth: true,
    label: config.fieldLabel,
    name: config.fieldName,
    value: (formData as any)[config.fieldName] ?? '',
    onChange: onChange,
    required: config.isRequired,
    placeholder: config.placeholder || undefined,
  } as any;

  switch (config.fieldName) {
    case 'category':
      return (
        <FormControl fullWidth>
          <InputLabel>{config.fieldLabel}</InputLabel>
          <Select
            name="category"
            value={formData.category}
            onChange={(e) => setFormData && setFormData((prev: any) => ({ ...prev, category: Number(e.target.value) }))}
            label={config.fieldLabel}
          >
            <MenuItem value={0}>Individual</MenuItem>
            <MenuItem value={1}>Organization</MenuItem>
          </Select>
        </FormControl>
      );
    case 'preferredContactMethod':
      return (
        <FormControl fullWidth>
          <InputLabel>{config.fieldLabel}</InputLabel>
          <Select name="preferredContactMethod" value={formData.preferredContactMethod} onChange={onSelectChange} label={config.fieldLabel}>
            {(items && items.length ? items.map(i => <MenuItem key={i.key || i.id} value={i.key || i.value}>{i.value}</MenuItem>) : (config.options ? config.options.split(',').map(o => o.trim()).filter(Boolean).map(opt => <MenuItem key={opt} value={opt}>{opt}</MenuItem>) : null))}
          </Select>
        </FormControl>
      );
    default:
      if (config.fieldType === 'select') {
        if (config.options && config.options.startsWith('lookup:')) {
          return (
            <FormControl fullWidth>
              <InputLabel>{config.fieldLabel}</InputLabel>
              <Select name={config.fieldName} value={(formData as any)[config.fieldName] ?? ''} onChange={onSelectChange} label={config.fieldLabel}>
                {(items && items.length ? items.map(i => <MenuItem key={i.key || i.id} value={i.key || i.value}>{i.value}</MenuItem>) : null)}
              </Select>
            </FormControl>
          );
        }
        return (
          <FormControl fullWidth>
            <InputLabel>{config.fieldLabel}</InputLabel>
            <Select name={config.fieldName} value={(formData as any)[config.fieldName] ?? ''} onChange={onSelectChange} label={config.fieldLabel}>
              {config.options ? config.options.split(',').map(o => o.trim()).filter(Boolean).map(opt => <MenuItem key={opt} value={opt}>{opt}</MenuItem>) : null}
            </Select>
          </FormControl>
        );
      }
      if (config.fieldType === 'textarea') {
        return <TextField {...commonProps} multiline rows={3} />;
      }
      if (config.fieldType === 'checkbox') {
        return (
          <FormControlLabel
            control={<Checkbox name={config.fieldName} checked={(formData as any)[config.fieldName]} onChange={onChange} />}
            label={config.fieldLabel}
          />
        );
      }
      return <TextField {...commonProps} type={config.fieldType === 'currency' ? 'number' : config.fieldType} />;
  }
};

export default FieldRenderer;

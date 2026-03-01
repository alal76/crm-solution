/**
 * ENUM-FE-008: EnumValueForm.tsx
 * Dialog form for creating or editing an enum value.
 * Accepts initial values and fires onSave with the completed DTO.
 */
import React, { useEffect, useState } from 'react';
import {
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  Grid,
  Switch,
  TextField,
  Typography,
} from '@mui/material';
import type { CreateEnumValueDto, UpdateEnumValueDto } from '../../../types/enums';

export interface EnumValueFormProps {
  open: boolean;
  /** Prefilled values when editing an existing item */
  initial?: Partial<CreateEnumValueDto & Pick<UpdateEnumValueDto, 'isActive' | 'isDefault' | 'sortOrder'>>;
  isEdit?: boolean;
  isReadOnly?: boolean;
  title?: string;
  onSave: (dto: CreateEnumValueDto | UpdateEnumValueDto) => Promise<void>;
  onClose: () => void;
}

const defaults: CreateEnumValueDto = {
  key: '',
  label: '',
  description: '',
  color: '',
  icon: '',
  metadata: '',
  isDefault: false,
};

const EnumValueForm: React.FC<EnumValueFormProps> = ({
  open,
  initial,
  isEdit = false,
  isReadOnly = false,
  title,
  onSave,
  onClose,
}) => {
  const [form, setForm] = useState<CreateEnumValueDto & { isActive: boolean; isDefault: boolean; sortOrder: number }>({
    ...defaults,
    isActive: true,
    isDefault: false,
    sortOrder: 0,
    ...initial,
  } as CreateEnumValueDto & { isActive: boolean; isDefault: boolean; sortOrder: number });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (open) {
      setForm({ ...defaults, isActive: true, sortOrder: 0, ...initial } as CreateEnumValueDto & { isActive: boolean; isDefault: boolean; sortOrder: number });
      setErrors({});
    }
  }, [open, initial]);

  const set = (field: string) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm(prev => ({ ...prev, [field]: e.target.value }));

  const setToggle = (field: string) => (_: unknown, checked: boolean) =>
    setForm(prev => ({ ...prev, [field]: checked }));

  const validate = () => {
    const e: Record<string, string> = {};
    if (!isEdit && !form.key?.trim()) e.key = 'Key is required';
    if (!form.label?.trim()) e.label = 'Label is required';
    return e;
  };

  const handleSave = async () => {
    const e = validate();
    if (Object.keys(e).length) { setErrors(e); return; }
    setSaving(true);
    try {
      if (isEdit) {
        await onSave({
          key: form.key,
          label: form.label,
          description: form.description,
          color: form.color,
          icon: form.icon,
          metadata: form.metadata,
          isActive: form.isActive ?? true,
          isDefault: form.isDefault ?? false,
          sortOrder: form.sortOrder ?? 0,
        } as UpdateEnumValueDto);
      } else {
        await onSave({ ...form } as CreateEnumValueDto);
      }
      onClose();
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{title ?? (isEdit ? 'Edit Enum Value' : 'Add Enum Value')}</DialogTitle>
      <DialogContent dividers>
        {isReadOnly && (
          <Typography variant="caption" color="warning.main" sx={{ display: 'block', mb: 1 }}>
            This is a system value and cannot be modified.
          </Typography>
        )}
        <Grid container spacing={2} sx={{ mt: 0 }}>
          {!isEdit && (
            <Grid item xs={12}>
              <TextField
                label="Key"
                value={form.key}
                onChange={set('key')}
                fullWidth
                required
                disabled={isReadOnly}
                error={!!errors.key}
                helperText={errors.key || 'Unique identifier (e.g. NEW, IN_PROGRESS). Cannot be changed after creation.'}
              />
            </Grid>
          )}
          <Grid item xs={12}>
            <TextField
              label="Label"
              value={form.label}
              onChange={set('label')}
              fullWidth
              required
              disabled={isReadOnly}
              error={!!errors.label}
              helperText={errors.label}
            />
          </Grid>
          <Grid item xs={12}>
            <TextField
              label="Description"
              value={form.description ?? ''}
              onChange={set('description')}
              fullWidth
              multiline
              rows={2}
              disabled={isReadOnly}
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              label="Color (CSS / hex)"
              value={form.color ?? ''}
              onChange={set('color')}
              fullWidth
              disabled={isReadOnly}
              placeholder="#4CAF50 or green"
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              label="Icon"
              value={form.icon ?? ''}
              onChange={set('icon')}
              fullWidth
              disabled={isReadOnly}
              placeholder="check_circle"
            />
          </Grid>
          {isEdit && (
            <>
              <Grid item xs={6}>
                <FormControlLabel
                  control={<Switch checked={form.isActive ?? true} onChange={setToggle('isActive')} disabled={isReadOnly} />}
                  label="Active"
                />
              </Grid>
              <Grid item xs={6}>
                <FormControlLabel
                  control={<Switch checked={form.isDefault ?? false} onChange={setToggle('isDefault')} disabled={isReadOnly} />}
                  label="Set as Default"
                />
              </Grid>
            </>
          )}
        </Grid>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={saving}>Cancel</Button>
        {!isReadOnly && (
          <Button onClick={handleSave} variant="contained" disabled={saving}
            startIcon={saving ? <CircularProgress size={16} /> : undefined}>
            {isEdit ? 'Save Changes' : 'Add Value'}
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
};

export default EnumValueForm;

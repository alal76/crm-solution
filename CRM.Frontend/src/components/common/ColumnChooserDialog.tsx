import React, { useState } from 'react';
import {
  Box, Button, Checkbox, Dialog, DialogActions, DialogContent, DialogTitle,
  FormControlLabel, Stack, Typography
} from '@mui/material';
import apiClient from '../../services/apiClient';

export interface ColumnDef {
  fieldKey: string;
  label: string;
  visible: boolean;
}

interface Props {
  open: boolean;
  onClose: () => void;
  entityType: string;
  columns: ColumnDef[];
  onChange: (columns: ColumnDef[]) => void;
}

const ColumnChooserDialog: React.FC<Props> = ({ open, onClose, entityType, columns, onChange }) => {
  const [local, setLocal] = useState<ColumnDef[]>([...columns]);
  const [saving, setSaving] = useState(false);

  const handleToggle = (fieldKey: string) => {
    setLocal(prev => prev.map(c => c.fieldKey === fieldKey ? { ...c, visible: !c.visible } : c));
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      await apiClient.put('/api/page-layouts/user-preferences', { entityType, columns: local });
      onChange(local);
      onClose();
    } catch {
      // surface error via onClose — parent handles
    } finally {
      setSaving(false);
    }
  };

  const handleReset = () => setLocal(columns.map(c => ({ ...c, visible: true })));

  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle>Choose Columns</DialogTitle>
      <DialogContent dividers>
        {local.length === 0 ? (
          <Typography color="text.secondary">No columns available.</Typography>
        ) : (
          <Stack spacing={0.5}>
            {local.map(col => (
              <FormControlLabel
                key={col.fieldKey}
                label={<Box><Typography variant="body2">{col.label}</Typography>
                  <Typography variant="caption" color="text.secondary">{col.fieldKey}</Typography></Box>}
                control={
                  <Checkbox checked={col.visible} onChange={() => handleToggle(col.fieldKey)} size="small" />
                }
              />
            ))}
          </Stack>
        )}
      </DialogContent>
      <DialogActions>
        <Button size="small" onClick={handleReset}>Show All</Button>
        <Box flex={1} />
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" onClick={handleSave} disabled={saving}>
          {saving ? 'Saving…' : 'Apply'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default ColumnChooserDialog;

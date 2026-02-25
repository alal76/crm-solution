import React, { useState, useEffect, useCallback } from 'react';
import {
  Box, Button, Dialog, DialogActions, DialogContent, DialogTitle,
  FormControl, FormControlLabel, Grid, InputLabel, MenuItem,
  Paper, Select, Stack, Switch, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow, TextField, Typography,
  Chip, IconButton, Tooltip, Alert, CircularProgress
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import apiClient from '../../services/apiClient';

interface CustomFieldDefinition {
  id: number;
  entityType: string;
  fieldKey: string;
  label: string;
  fieldType: string;
  isRequired: boolean;
  isActive: boolean;
  displayOrder: number;
  defaultValue?: string;
  optionsJson?: string;
  groupName?: string;
}

const ENTITY_TYPES = ['Account', 'Contact', 'Lead', 'Opportunity', 'ServiceRequest', 'Product'];
const FIELD_TYPES = ['Text', 'Number', 'Date', 'Dropdown', 'Checkbox', 'MultiSelect', 'Email', 'Url', 'TextArea', 'Currency'];

const defaultForm: Omit<CustomFieldDefinition, 'id'> = {
  entityType: 'Account',
  fieldKey: '',
  label: '',
  fieldType: 'Text',
  isRequired: false,
  isActive: true,
  displayOrder: 0,
  defaultValue: '',
  optionsJson: '',
  groupName: '',
};

const CustomFieldsAdminPage: React.FC = () => {
  const [fields, setFields] = useState<CustomFieldDefinition[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedEntity, setSelectedEntity] = useState('Account');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingField, setEditingField] = useState<CustomFieldDefinition | null>(null);
  const [form, setForm] = useState(defaultForm);

  const loadFields = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await apiClient.get<CustomFieldDefinition[]>(
        `/api/custom-fields?entityType=${selectedEntity}`
      );
      setFields(res.data);
    } catch {
      setError('Failed to load custom fields.');
    } finally {
      setLoading(false);
    }
  }, [selectedEntity]);

  useEffect(() => { loadFields(); }, [loadFields]);

  const openCreate = () => {
    setEditingField(null);
    setForm({ ...defaultForm, entityType: selectedEntity });
    setDialogOpen(true);
  };

  const openEdit = (field: CustomFieldDefinition) => {
    setEditingField(field);
    setForm({ ...field });
    setDialogOpen(true);
  };

  const handleSave = async () => {
    try {
      if (editingField) {
        await apiClient.put(`/api/custom-fields/${editingField.id}`, form);
      } else {
        await apiClient.post('/api/custom-fields', form);
      }
      setDialogOpen(false);
      loadFields();
    } catch {
      setError('Failed to save field definition.');
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Delete this custom field?')) return;
    try {
      await apiClient.delete(`/api/custom-fields/${id}`);
      loadFields();
    } catch {
      setError('Failed to delete field.');
    }
  };

  return (
    <Box p={3}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" mb={2}>
        <Typography variant="h5" fontWeight="bold">Custom Fields</Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={openCreate}>
          New Field
        </Button>
      </Stack>

      <FormControl size="small" sx={{ minWidth: 200, mb: 2 }}>
        <InputLabel>Entity Type</InputLabel>
        <Select
          value={selectedEntity}
          label="Entity Type"
          onChange={e => setSelectedEntity(e.target.value)}
        >
          {ENTITY_TYPES.map(et => (
            <MenuItem key={et} value={et}>{et}</MenuItem>
          ))}
        </Select>
      </FormControl>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {loading ? (
        <Box display="flex" justifyContent="center" mt={4}><CircularProgress /></Box>
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Key</TableCell>
                <TableCell>Label</TableCell>
                <TableCell>Type</TableCell>
                <TableCell>Required</TableCell>
                <TableCell>Active</TableCell>
                <TableCell>Group</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {fields.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} align="center">No custom fields defined for {selectedEntity}.</TableCell>
                </TableRow>
              ) : fields.map(f => (
                <TableRow key={f.id} hover>
                  <TableCell><code>{f.fieldKey}</code></TableCell>
                  <TableCell>{f.label}</TableCell>
                  <TableCell><Chip size="small" label={f.fieldType} /></TableCell>
                  <TableCell>{f.isRequired ? 'Yes' : 'No'}</TableCell>
                  <TableCell>
                    <Chip size="small" color={f.isActive ? 'success' : 'default'} label={f.isActive ? 'Active' : 'Inactive'} />
                  </TableCell>
                  <TableCell>{f.groupName || '—'}</TableCell>
                  <TableCell align="right">
                    <Tooltip title="Edit"><IconButton size="small" onClick={() => openEdit(f)}><EditIcon fontSize="small" /></IconButton></Tooltip>
                    <Tooltip title="Delete"><IconButton size="small" color="error" onClick={() => handleDelete(f.id)}><DeleteIcon fontSize="small" /></IconButton></Tooltip>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {/* Create / Edit dialog */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editingField ? 'Edit Custom Field' : 'New Custom Field'}</DialogTitle>
        <DialogContent dividers>
          <Grid container spacing={2} mt={0.5}>
            <Grid item xs={6}>
              <FormControl fullWidth size="small">
                <InputLabel>Entity Type</InputLabel>
                <Select value={form.entityType} label="Entity Type"
                  onChange={e => setForm(f => ({ ...f, entityType: e.target.value }))}>
                  {ENTITY_TYPES.map(et => <MenuItem key={et} value={et}>{et}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={6}>
              <FormControl fullWidth size="small">
                <InputLabel>Field Type</InputLabel>
                <Select value={form.fieldType} label="Field Type"
                  onChange={e => setForm(f => ({ ...f, fieldType: e.target.value }))}>
                  {FIELD_TYPES.map(ft => <MenuItem key={ft} value={ft}>{ft}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={6}>
              <TextField size="small" fullWidth label="Field Key" value={form.fieldKey}
                onChange={e => setForm(f => ({ ...f, fieldKey: e.target.value }))} />
            </Grid>
            <Grid item xs={6}>
              <TextField size="small" fullWidth label="Label" value={form.label}
                onChange={e => setForm(f => ({ ...f, label: e.target.value }))} />
            </Grid>
            <Grid item xs={6}>
              <TextField size="small" fullWidth label="Group Name" value={form.groupName}
                onChange={e => setForm(f => ({ ...f, groupName: e.target.value }))} />
            </Grid>
            <Grid item xs={6}>
              <TextField size="small" fullWidth type="number" label="Display Order"
                value={form.displayOrder}
                onChange={e => setForm(f => ({ ...f, displayOrder: Number(e.target.value) }))} />
            </Grid>
            <Grid item xs={12}>
              <TextField size="small" fullWidth label="Default Value" value={form.defaultValue}
                onChange={e => setForm(f => ({ ...f, defaultValue: e.target.value }))} />
            </Grid>
            <Grid item xs={12}>
              <TextField size="small" fullWidth label="Options (JSON array)" value={form.optionsJson}
                helperText='E.g. ["Small","Medium","Large"] — for Dropdown/MultiSelect'
                onChange={e => setForm(f => ({ ...f, optionsJson: e.target.value }))} />
            </Grid>
            <Grid item xs={6}>
              <FormControlLabel control={
                <Switch checked={form.isRequired}
                  onChange={e => setForm(f => ({ ...f, isRequired: e.target.checked }))} />
              } label="Required" />
            </Grid>
            <Grid item xs={6}>
              <FormControlLabel control={
                <Switch checked={form.isActive}
                  onChange={e => setForm(f => ({ ...f, isActive: e.target.checked }))} />
              } label="Active" />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSave} disabled={!form.fieldKey || !form.label}>
            {editingField ? 'Save Changes' : 'Create Field'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default CustomFieldsAdminPage;

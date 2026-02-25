import React, { useState, useEffect, useCallback } from 'react';
import {
  Alert, Box, Button, Card, CardContent, Chip, CircularProgress,
  Dialog, DialogActions, DialogContent, DialogTitle, FormControl,
  Grid, IconButton, InputLabel, MenuItem, Paper, Select, Stack,
  TextField, Tooltip, Typography
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import SaveIcon from '@mui/icons-material/Save';
import DeleteIcon from '@mui/icons-material/Delete';
import DragIndicatorIcon from '@mui/icons-material/DragIndicator';
import apiClient from '../../services/apiClient';

interface PageLayout {
  id: number;
  entityType: string;
  name: string;
  layoutJson: string;
  isDefault: boolean;
  userGroupId?: number;
}

interface LayoutColumn {
  fieldKey: string;
  label: string;
  width: number;
  visible: boolean;
  order: number;
}

const ENTITY_TYPES = ['Account', 'Contact', 'Lead', 'Opportunity', 'ServiceRequest'];

const PageLayoutEditorPage: React.FC = () => {
  const [layouts, setLayouts] = useState<PageLayout[]>([]);
  const [selectedLayout, setSelectedLayout] = useState<PageLayout | null>(null);
  const [columns, setColumns] = useState<LayoutColumn[]>([]);
  const [selectedEntity, setSelectedEntity] = useState('Account');
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [newLayoutDialog, setNewLayoutDialog] = useState(false);
  const [newLayoutName, setNewLayoutName] = useState('');

  const loadLayouts = useCallback(async () => {
    setLoading(true);
    try {
      const res = await apiClient.get<PageLayout[]>(`/api/page-layouts?entityType=${selectedEntity}`);
      setLayouts(res.data);
      if (res.data.length > 0) {
        const layout = res.data.find(l => l.isDefault) ?? res.data[0];
        setSelectedLayout(layout);
        tryParseColumns(layout.layoutJson);
      } else {
        setSelectedLayout(null);
        setColumns([]);
      }
    } catch {
      setError('Failed to load layouts.');
    } finally {
      setLoading(false);
    }
  }, [selectedEntity]);

  useEffect(() => { loadLayouts(); }, [loadLayouts]);

  const tryParseColumns = (json: string) => {
    try {
      const parsed: LayoutColumn[] = JSON.parse(json);
      setColumns(parsed.sort((a, b) => a.order - b.order));
    } catch {
      setColumns([]);
    }
  };

  const selectLayout = (layout: PageLayout) => {
    setSelectedLayout(layout);
    tryParseColumns(layout.layoutJson);
  };

  const moveUp = (index: number) => {
    if (index === 0) return;
    const updated = [...columns];
    [updated[index - 1], updated[index]] = [updated[index], updated[index - 1]];
    setColumns(updated.map((c, i) => ({ ...c, order: i })));
  };

  const moveDown = (index: number) => {
    if (index === columns.length - 1) return;
    const updated = [...columns];
    [updated[index], updated[index + 1]] = [updated[index + 1], updated[index]];
    setColumns(updated.map((c, i) => ({ ...c, order: i })));
  };

  const toggleVisible = (index: number) => {
    setColumns(prev => prev.map((c, i) => i === index ? { ...c, visible: !c.visible } : c));
  };

  const handleSave = async () => {
    if (!selectedLayout) return;
    setSaving(true);
    setError(null);
    try {
      const updated = { ...selectedLayout, layoutJson: JSON.stringify(columns) };
      await apiClient.put(`/api/page-layouts/${selectedLayout.id}`, updated);
      setSuccess('Layout saved successfully.');
      setTimeout(() => setSuccess(null), 3000);
    } catch {
      setError('Failed to save layout.');
    } finally {
      setSaving(false);
    }
  };

  const handleCreateLayout = async () => {
    if (!newLayoutName.trim()) return;
    try {
      await apiClient.post('/api/page-layouts', {
        entityType: selectedEntity,
        name: newLayoutName,
        layoutJson: '[]',
        isDefault: false,
      });
      setNewLayoutDialog(false);
      setNewLayoutName('');
      loadLayouts();
    } catch {
      setError('Failed to create layout.');
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Delete this layout?')) return;
    try {
      await apiClient.delete(`/api/page-layouts/${id}`);
      loadLayouts();
    } catch {
      setError('Failed to delete layout.');
    }
  };

  return (
    <Box p={3}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" mb={2}>
        <Typography variant="h5" fontWeight="bold">Page Layout Editor</Typography>
        <Stack direction="row" spacing={1}>
          <Button variant="outlined" startIcon={<AddIcon />} onClick={() => setNewLayoutDialog(true)}>
            New Layout
          </Button>
          <Button variant="contained" startIcon={<SaveIcon />} onClick={handleSave}
            disabled={!selectedLayout || saving}>
            {saving ? 'Saving…' : 'Save Layout'}
          </Button>
        </Stack>
      </Stack>

      <FormControl size="small" sx={{ minWidth: 200, mb: 2 }}>
        <InputLabel>Entity Type</InputLabel>
        <Select value={selectedEntity} label="Entity Type"
          onChange={e => setSelectedEntity(e.target.value)}>
          {ENTITY_TYPES.map(et => <MenuItem key={et} value={et}>{et}</MenuItem>)}
        </Select>
      </FormControl>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}

      {loading ? (
        <Box display="flex" justifyContent="center" mt={4}><CircularProgress /></Box>
      ) : (
        <Grid container spacing={2}>
          {/* Layout selector */}
          <Grid item xs={3}>
            <Typography variant="subtitle2" gutterBottom>Layouts</Typography>
            <Stack spacing={1}>
              {layouts.map(l => (
                <Card key={l.id} variant={selectedLayout?.id === l.id ? 'elevation' : 'outlined'}
                  sx={{ cursor: 'pointer', bgcolor: selectedLayout?.id === l.id ? 'action.selected' : undefined }}
                  onClick={() => selectLayout(l)}>
                  <CardContent sx={{ py: 1, '&:last-child': { pb: 1 } }}>
                    <Stack direction="row" justifyContent="space-between" alignItems="center">
                      <Box>
                        <Typography variant="body2" fontWeight="bold">{l.name}</Typography>
                        {l.isDefault && <Chip size="small" label="Default" color="primary" />}
                      </Box>
                      <Tooltip title="Delete">
                        <IconButton size="small" color="error"
                          onClick={e => { e.stopPropagation(); handleDelete(l.id); }}>
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </Stack>
                  </CardContent>
                </Card>
              ))}
              {layouts.length === 0 && (
                <Typography variant="body2" color="text.secondary">No layouts. Create one.</Typography>
              )}
            </Stack>
          </Grid>

          {/* Column editor */}
          <Grid item xs={9}>
            <Typography variant="subtitle2" gutterBottom>
              Columns {selectedLayout ? `— ${selectedLayout.name}` : ''}
            </Typography>
            {!selectedLayout ? (
              <Paper variant="outlined" sx={{ p: 3, textAlign: 'center' }}>
                <Typography color="text.secondary">Select a layout to edit its columns.</Typography>
              </Paper>
            ) : columns.length === 0 ? (
              <Paper variant="outlined" sx={{ p: 3, textAlign: 'center' }}>
                <Typography color="text.secondary">No columns configured in this layout.</Typography>
              </Paper>
            ) : (
              <Paper variant="outlined">
                {columns.map((col, i) => (
                  <Box key={col.fieldKey} sx={{ display: 'flex', alignItems: 'center', p: 1.5,
                    borderBottom: i < columns.length - 1 ? '1px solid' : undefined, borderColor: 'divider' }}>
                    <DragIndicatorIcon color="action" sx={{ mr: 1 }} />
                    <Box flex={1}>
                      <Typography variant="body2" fontWeight="bold">{col.label}</Typography>
                      <Typography variant="caption" color="text.secondary">{col.fieldKey}</Typography>
                    </Box>
                    <Chip size="small" label={`${col.width}px`} sx={{ mr: 1 }} />
                    <Chip size="small" color={col.visible ? 'success' : 'default'}
                      label={col.visible ? 'Visible' : 'Hidden'}
                      onClick={() => toggleVisible(i)} sx={{ mr: 1, cursor: 'pointer' }} />
                    <Stack direction="row" spacing={0.5}>
                      <Button size="small" onClick={() => moveUp(i)} disabled={i === 0}>↑</Button>
                      <Button size="small" onClick={() => moveDown(i)} disabled={i === columns.length - 1}>↓</Button>
                    </Stack>
                  </Box>
                ))}
              </Paper>
            )}
          </Grid>
        </Grid>
      )}

      <Dialog open={newLayoutDialog} onClose={() => setNewLayoutDialog(false)} maxWidth="xs" fullWidth>
        <DialogTitle>New Layout</DialogTitle>
        <DialogContent>
          <TextField autoFocus fullWidth size="small" label="Layout Name" sx={{ mt: 1 }}
            value={newLayoutName} onChange={e => setNewLayoutName(e.target.value)} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setNewLayoutDialog(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleCreateLayout} disabled={!newLayoutName.trim()}>
            Create
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default PageLayoutEditorPage;

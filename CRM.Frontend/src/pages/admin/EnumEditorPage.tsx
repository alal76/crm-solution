/**
 * ENUM-FE-005: EnumEditorPage.tsx
 * Displays and edits values for a specific enum category, identified by
 * `:categoryName` in the URL. Functions as the dedicated editor route for
 * /admin/enum-management/:categoryName and /admin/master-data/enums/:categoryName
 */
import React, { useCallback, useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  FormControlLabel,
  Grid,
  IconButton,
  InputAdornment,
  Paper,
  Snackbar,
  Stack,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import {
  Add as AddIcon,
  ArrowBack as BackIcon,
  ArrowDownward as MoveDownIcon,
  ArrowUpward as MoveUpIcon,
  Delete as DeleteIcon,
  Edit as EditIcon,
  Lock as SystemIcon,
  Visibility as ViewIcon,
} from '@mui/icons-material';
import {
  createItem,
  deleteItem,
  getCategories,
  getItems,
  reorderItems,
  updateItem,
  type CreateLookupItemDto,
  type LookupCategoryDto,
  type LookupItemDto,
  type UpdateLookupItemDto,
} from '../../services/enumManagementService';
import enumCacheService from '../../services/enumCacheService';

// ─── Item Form Dialog ─────────────────────────────────────────────────────────

interface ItemFormProps {
  open: boolean;
  initial: Partial<CreateLookupItemDto & { id?: number }>;
  isEdit: boolean;
  isSystemValue: boolean;
  onSave: (dto: CreateLookupItemDto | UpdateLookupItemDto) => Promise<void>;
  onClose: () => void;
}

const emptyItem: CreateLookupItemDto = {
  key: '', value: '', meta: '', sortOrder: 0,
  isActive: true, isDefault: false, color: '', icon: '', validationRules: '',
};

function ItemFormDialog({ open, initial, isEdit, isSystemValue, onSave, onClose }: ItemFormProps) {
  const [form, setForm] = useState<CreateLookupItemDto>({ ...emptyItem, ...initial });
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (open) { setForm({ ...emptyItem, ...initial }); setErrors({}); }
  }, [open, initial]);

  const validate = () => {
    const e: Record<string, string> = {};
    if (!form.key.trim()) e.key = 'Key is required';
    if (!form.value.trim()) e.value = 'Display value is required';
    return e;
  };

  const handleSave = async () => {
    const e = validate();
    if (Object.keys(e).length) { setErrors(e); return; }
    setSaving(true);
    try { await onSave(form); onClose(); } finally { setSaving(false); }
  };

  const set = (field: keyof CreateLookupItemDto) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm(prev => ({ ...prev, [field]: field === 'sortOrder' ? Number.parseInt(e.target.value) || 0 : e.target.value }));

  const setSwitch = (field: keyof CreateLookupItemDto) => (_: unknown, checked: boolean) =>
    setForm(prev => ({ ...prev, [field]: checked }));

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{isEdit ? (isSystemValue ? 'View Item (System)' : 'Edit Item') : 'Add Item'}</DialogTitle>
      <DialogContent dividers>
        <Grid container spacing={2} sx={{ mt: 0 }}>
          <Grid item xs={6}>
            <TextField label="Key" value={form.key} onChange={set('key')} fullWidth required
              disabled={isEdit || isSystemValue} error={!!errors.key}
              helperText={errors.key || 'Unique key, no spaces (e.g. NEW, IN_PROGRESS)'} />
          </Grid>
          <Grid item xs={6}>
            <TextField label="Display Value" value={form.value} onChange={set('value')} fullWidth required
              disabled={isSystemValue} error={!!errors.value} helperText={errors.value} />
          </Grid>
          <Grid item xs={4}>
            <TextField label="Sort Order" type="number" value={form.sortOrder}
              onChange={set('sortOrder')} fullWidth disabled={isSystemValue} />
          </Grid>
          <Grid item xs={4}>
            <TextField label="Color (hex)" value={form.color ?? ''} onChange={set('color')}
              fullWidth disabled={isSystemValue} placeholder="#4CAF50"
              InputProps={{
                startAdornment: form.color ? (
                  <InputAdornment position="start">
                    <Box sx={{ width: 16, height: 16, borderRadius: '50%', bgcolor: form.color, border: '1px solid #ccc' }} />
                  </InputAdornment>
                ) : undefined,
              }} />
          </Grid>
          <Grid item xs={4}>
            <TextField label="Icon" value={form.icon ?? ''} onChange={set('icon')}
              fullWidth disabled={isSystemValue} placeholder="check_circle" />
          </Grid>
          <Grid item xs={12}>
            <TextField label="Description / Meta" value={form.meta ?? ''} onChange={set('meta')}
              fullWidth multiline rows={2} disabled={isSystemValue} />
          </Grid>
          <Grid item xs={6}>
            <FormControlLabel
              control={<Switch checked={form.isActive} onChange={setSwitch('isActive')} disabled={isSystemValue} />}
              label="Active" />
          </Grid>
          <Grid item xs={6}>
            <FormControlLabel
              control={<Switch checked={form.isDefault} onChange={setSwitch('isDefault')} disabled={isSystemValue} />}
              label="Default Value" />
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={saving}>Cancel</Button>
        {!isSystemValue && (
          <Button onClick={handleSave} variant="contained" disabled={saving}
            startIcon={saving ? <CircularProgress size={16} /> : undefined}>
            {isEdit ? 'Save' : 'Add'}
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
}

// ─── Confirm Delete ───────────────────────────────────────────────────────────

function ConfirmDeleteDialog({ open, message, onConfirm, onClose }: {
  open: boolean; message: string; onConfirm: () => Promise<void>; onClose: () => void;
}) {
  const [deleting, setDeleting] = useState(false);
  const handleConfirm = async () => {
    setDeleting(true);
    try { await onConfirm(); onClose(); } finally { setDeleting(false); }
  };
  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle>Delete Item</DialogTitle>
      <DialogContent><Typography>{message}</Typography></DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={deleting}>Cancel</Button>
        <Button onClick={handleConfirm} color="error" variant="contained" disabled={deleting}
          startIcon={deleting ? <CircularProgress size={16} /> : undefined}>Delete</Button>
      </DialogActions>
    </Dialog>
  );
}

// ─── Main Page ────────────────────────────────────────────────────────────────

export default function EnumEditorPage() {
  const { categoryName } = useParams<{ categoryName: string }>();
  const navigate = useNavigate();

  const [category, setCategory] = useState<LookupCategoryDto | null>(null);
  const [items, setItems] = useState<LookupItemDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [showInactive, setShowInactive] = useState(false);

  const [itemFormOpen, setItemFormOpen] = useState(false);
  const [editItem, setEditItem] = useState<LookupItemDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<LookupItemDto | null>(null);

  const [toast, setToast] = useState<{ message: string; severity: 'success' | 'error' } | null>(null);
  const notify = useCallback((message: string, severity: 'success' | 'error' = 'success') => {
    setToast({ message, severity });
  }, []);

  const load = useCallback(async () => {
    if (!categoryName) return;
    setLoading(true);
    try {
      const cats = await getCategories({ includeInactive: true });
      const found = cats.find(c => c.name.toLowerCase() === categoryName.toLowerCase());
      if (!found) { notify(`Category '${categoryName}' not found`, 'error'); setLoading(false); return; }
      setCategory(found);
      const data = await getItems(found.id, { includeInactive: showInactive });
      setItems(data);
    } catch {
      notify('Failed to load category data', 'error');
    } finally {
      setLoading(false);
    }
  }, [categoryName, showInactive, notify]);

  useEffect(() => { load(); }, [load]);

  const handleAdd = async (dto: CreateLookupItemDto | UpdateLookupItemDto) => {
    if (!category) return;
    await createItem(category.id, dto as CreateLookupItemDto);
    enumCacheService.invalidate(categoryName);
    notify(`Item '${(dto as CreateLookupItemDto).value}' created`);
    load();
  };

  const handleEdit = async (dto: CreateLookupItemDto | UpdateLookupItemDto) => {
    if (!editItem) return;
    await updateItem(editItem.id, dto as UpdateLookupItemDto);
    enumCacheService.invalidate(categoryName);
    notify(`Item '${(dto as UpdateLookupItemDto).value}' updated`);
    load();
  };

  const handleDelete = async () => {
    if (!deleteTarget || !category) return;
    await deleteItem(deleteTarget.id);
    enumCacheService.invalidate(categoryName);
    notify(`Item '${deleteTarget.value}' deleted`);
    load();
  };

  const moveItem = async (index: number, direction: 'up' | 'down') => {
    if (!category) return;
    const arr = [...items];
    const swap = direction === 'up' ? index - 1 : index + 1;
    if (swap < 0 || swap >= arr.length) return;
    [arr[index], arr[swap]] = [arr[swap], arr[index]];
    setItems(arr);
    await reorderItems(category.id, arr.map(i => i.id));
    enumCacheService.invalidate(categoryName);
  };

  const handleBack = () => {
    // Go back to parent enum list (try to detect which admin section we came from)
    const from = document.referrer;
    if (from.includes('master-data/enums')) {
      navigate('/admin/master-data/enums');
    } else {
      navigate('/admin/enum-management');
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!category) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="error">Category '{categoryName}' not found.</Alert>
        <Button sx={{ mt: 2 }} onClick={handleBack} startIcon={<BackIcon />}>Back</Button>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
        <IconButton onClick={handleBack} size="small"><BackIcon /></IconButton>
        <Box sx={{ flex: 1 }}>
          <Stack direction="row" spacing={1} alignItems="center">
            <Typography variant="h5" sx={{ fontWeight: 700 }}>{category.name}</Typography>
            {category.isSystemManaged && <Chip label="System" size="small" color="warning" icon={<SystemIcon />} />}
          </Stack>
          {category.entityType && (
            <Typography variant="caption" color="text.secondary">
              {category.entityType} › {category.propertyName}
            </Typography>
          )}
        </Box>
        <FormControlLabel
          control={<Switch size="small" checked={showInactive} onChange={e => setShowInactive(e.target.checked)} />}
          label="Show inactive" sx={{ mr: 1 }} />
        {category.allowCustomValues && (
          <Button variant="contained" size="small" startIcon={<AddIcon />}
            onClick={() => { setEditItem(null); setItemFormOpen(true); }}>
            Add Value
          </Button>
        )}
      </Stack>

      <Divider sx={{ mb: 3 }} />

      {items.length === 0 ? (
        <Alert severity="info">
          No values found. {category.allowCustomValues ? 'Click "Add Value" to create the first one.' : ''}
        </Alert>
      ) : (
        <TableContainer component={Paper} variant="outlined">
          <Table size="small">
            <TableHead>
              <TableRow sx={{ '& th': { fontWeight: 600 } }}>
                <TableCell width={50}>#</TableCell>
                <TableCell>Key</TableCell>
                <TableCell>Display Value</TableCell>
                <TableCell width={80}>Color</TableCell>
                <TableCell width={80}>Status</TableCell>
                <TableCell width={80}>Default</TableCell>
                <TableCell width={80}>System</TableCell>
                <TableCell width={120}>Sort</TableCell>
                <TableCell width={100} align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {items.map((item, idx) => (
                <TableRow key={item.id} hover sx={{ opacity: item.isActive ? 1 : 0.5 }}>
                  <TableCell>
                    <Typography variant="body2" color="text.secondary">{item.sortOrder}</Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" sx={{ fontFamily: 'monospace', fontSize: '0.75rem', bgcolor: 'action.hover', px: 0.5, borderRadius: 0.5, display: 'inline-block' }}>
                      {item.key}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" sx={{ fontWeight: 500 }}>{item.value}</Typography>
                    {item.meta && <Typography variant="caption" color="text.secondary">{item.meta}</Typography>}
                  </TableCell>
                  <TableCell>
                    {item.color ? (
                      <Tooltip title={item.color}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                          <Box sx={{ width: 16, height: 16, borderRadius: '50%', bgcolor: item.color, border: '1px solid', borderColor: 'divider' }} />
                          <Typography variant="caption" sx={{ fontFamily: 'monospace' }}>{item.color}</Typography>
                        </Box>
                      </Tooltip>
                    ) : '—'}
                  </TableCell>
                  <TableCell>
                    <Chip label={item.isActive ? 'Active' : 'Inactive'} size="small"
                      color={item.isActive ? 'success' : 'default'} variant="outlined" />
                  </TableCell>
                  <TableCell>
                    {item.isDefault && <Chip label="Default" size="small" color="primary" />}
                  </TableCell>
                  <TableCell>
                    {item.isSystemValue && <Chip label="System" size="small" color="warning" icon={<SystemIcon />} />}
                  </TableCell>
                  <TableCell>
                    <Stack direction="row" spacing={0.5}>
                      <IconButton size="small" onClick={() => moveItem(idx, 'up')} disabled={idx === 0}><MoveUpIcon fontSize="small" /></IconButton>
                      <IconButton size="small" onClick={() => moveItem(idx, 'down')} disabled={idx === items.length - 1}><MoveDownIcon fontSize="small" /></IconButton>
                    </Stack>
                  </TableCell>
                  <TableCell align="right">
                    <IconButton size="small" onClick={() => { setEditItem(item); setItemFormOpen(true); }}>
                      {item.isSystemValue ? <ViewIcon fontSize="small" /> : <EditIcon fontSize="small" />}
                    </IconButton>
                    {!item.isSystemValue && (
                      <IconButton size="small" color="error" onClick={() => setDeleteTarget(item)}>
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {/* Item Form */}
      <ItemFormDialog
        open={itemFormOpen}
        initial={editItem ? {
          key: editItem.key, value: editItem.value, meta: editItem.meta ?? '',
          sortOrder: editItem.sortOrder, isActive: editItem.isActive,
          isDefault: editItem.isDefault, color: editItem.color ?? '', icon: editItem.icon ?? '',
          validationRules: editItem.validationRules ?? '',
        } : {}}
        isEdit={!!editItem}
        isSystemValue={editItem?.isSystemValue ?? false}
        onSave={editItem ? handleEdit : handleAdd}
        onClose={() => setItemFormOpen(false)}
      />

      {/* Delete Confirm */}
      <ConfirmDeleteDialog
        open={!!deleteTarget}
        message={`Delete item "${deleteTarget?.value}" (${deleteTarget?.key})? This cannot be undone.`}
        onConfirm={handleDelete}
        onClose={() => setDeleteTarget(null)}
      />

      {/* Toast */}
      <Snackbar open={!!toast} autoHideDuration={3500} onClose={() => setToast(null)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}>
        {toast ? (
          <Alert onClose={() => setToast(null)} severity={toast.severity} variant="filled">
            {toast.message}
          </Alert>
        ) : undefined}
      </Snackbar>
    </Box>
  );
}

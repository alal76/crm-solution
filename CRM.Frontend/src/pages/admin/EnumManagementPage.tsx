import React, { useState, useCallback, useEffect } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
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
  Alert,
  Snackbar,
  Stack,
  Tabs,
  Tab,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  ArrowBack as BackIcon,
  Search as SearchIcon,
  Category as CategoryIcon,
  FormatListNumbered as ItemsIcon,
  Circle as ColorDotIcon,
  Visibility as ViewIcon,
  VisibilityOff as HideIcon,
  Lock as SystemIcon,
  LockOpen as UserIcon,
  DragIndicator as DragIcon,
  ArrowUpward as MoveUpIcon,
  ArrowDownward as MoveDownIcon,
} from '@mui/icons-material';
import {
  getCategories,
  createCategory,
  updateCategory,
  deleteCategory,
  getItems,
  createItem,
  updateItem,
  deleteItem,
  reorderItems,
  type LookupCategoryDto,
  type LookupItemDto,
  type CreateLookupCategoryDto,
  type UpdateLookupCategoryDto,
  type CreateLookupItemDto,
  type UpdateLookupItemDto,
} from '../../services/enumManagementService';

// ─────────────────────────────────────────────────────────────────
// Helpers
// ─────────────────────────────────────────────────────────────────

const emptyCategory: CreateLookupCategoryDto = {
  name: '',
  description: '',
  entityType: '',
  propertyName: '',
  isActive: true,
  allowCustomValues: true,
  validationSchema: '',
};

const emptyItem: CreateLookupItemDto = {
  key: '',
  value: '',
  meta: '',
  sortOrder: 0,
  isActive: true,
  isDefault: false,
  color: '',
  icon: '',
  validationRules: '',
};

// ─────────────────────────────────────────────────────────────────
// Category Form Dialog
// ─────────────────────────────────────────────────────────────────

interface CategoryFormProps {
  open: boolean;
  initial: Partial<CreateLookupCategoryDto & { id?: number }>;
  isEdit: boolean;
  onSave: (dto: CreateLookupCategoryDto | UpdateLookupCategoryDto) => Promise<void>;
  onClose: () => void;
}

function CategoryFormDialog({ open, initial, isEdit, onSave, onClose }: CategoryFormProps) {
  const [form, setForm] = useState<CreateLookupCategoryDto>({ ...emptyCategory, ...initial });
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (open) {
      setForm({ ...emptyCategory, ...initial });
      setErrors({});
    }
  }, [open, initial]);

  const validate = () => {
    const e: Record<string, string> = {};
    if (!form.name.trim()) e.name = 'Name is required';
    return e;
  };

  const handleSave = async () => {
    const e = validate();
    if (Object.keys(e).length) { setErrors(e); return; }
    setSaving(true);
    try {
      await onSave(form);
      onClose();
    } finally {
      setSaving(false);
    }
  };

  const set = (field: keyof CreateLookupCategoryDto) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm(prev => ({ ...prev, [field]: e.target.value }));

  const setSwitch = (field: keyof CreateLookupCategoryDto) => (_: unknown, checked: boolean) =>
    setForm(prev => ({ ...prev, [field]: checked }));

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{isEdit ? 'Edit Category' : 'Add Category'}</DialogTitle>
      <DialogContent dividers>
        <Grid container spacing={2} sx={{ mt: 0 }}>
          <Grid item xs={12}>
            <TextField
              label="Name"
              value={form.name}
              onChange={set('name')}
              fullWidth required
              error={!!errors.name}
              helperText={errors.name}
            />
          </Grid>
          <Grid item xs={12}>
            <TextField
              label="Description"
              value={form.description ?? ''}
              onChange={set('description')}
              fullWidth multiline rows={2}
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              label="Entity Type"
              value={form.entityType ?? ''}
              onChange={set('entityType')}
              fullWidth
              helperText="e.g. Lead, ServiceRequest"
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              label="Property Name"
              value={form.propertyName ?? ''}
              onChange={set('propertyName')}
              fullWidth
              helperText="e.g. Status, Priority"
            />
          </Grid>
          <Grid item xs={6}>
            <FormControlLabel
              control={<Switch checked={form.isActive} onChange={setSwitch('isActive')} />}
              label="Active"
            />
          </Grid>
          <Grid item xs={6}>
            <FormControlLabel
              control={<Switch checked={form.allowCustomValues} onChange={setSwitch('allowCustomValues')} />}
              label="Allow Custom Values"
            />
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={saving}>Cancel</Button>
        <Button onClick={handleSave} variant="contained" disabled={saving} startIcon={saving ? <CircularProgress size={16} /> : undefined}>
          {isEdit ? 'Save' : 'Create'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}

// ─────────────────────────────────────────────────────────────────
// Item Form Dialog
// ─────────────────────────────────────────────────────────────────

interface ItemFormProps {
  open: boolean;
  initial: Partial<CreateLookupItemDto & { id?: number }>;
  isEdit: boolean;
  isSystemValue: boolean;
  onSave: (dto: CreateLookupItemDto | UpdateLookupItemDto) => Promise<void>;
  onClose: () => void;
}

function ItemFormDialog({ open, initial, isEdit, isSystemValue, onSave, onClose }: ItemFormProps) {
  const [form, setForm] = useState<CreateLookupItemDto>({ ...emptyItem, ...initial });
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (open) {
      setForm({ ...emptyItem, ...initial });
      setErrors({});
    }
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
    try {
      await onSave(form);
      onClose();
    } finally {
      setSaving(false);
    }
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
            <TextField
              label="Key (machine key)"
              value={form.key}
              onChange={set('key')}
              fullWidth required
              disabled={isSystemValue}
              error={!!errors.key}
              helperText={errors.key || 'Unique identifier, no spaces (e.g. NEW, IN_PROGRESS)'}
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              label="Display Value"
              value={form.value}
              onChange={set('value')}
              fullWidth required
              disabled={isSystemValue}
              error={!!errors.value}
              helperText={errors.value}
            />
          </Grid>
          <Grid item xs={4}>
            <TextField
              label="Sort Order"
              type="number"
              value={form.sortOrder}
              onChange={set('sortOrder')}
              fullWidth
              disabled={isSystemValue}
            />
          </Grid>
          <Grid item xs={4}>
            <TextField
              label="Color (hex)"
              value={form.color ?? ''}
              onChange={set('color')}
              fullWidth
              disabled={isSystemValue}
              placeholder="#4CAF50"
              InputProps={{
                startAdornment: form.color ? (
                  <InputAdornment position="start">
                    <Box sx={{ width: 16, height: 16, borderRadius: '50%', bgcolor: form.color, border: '1px solid #ccc' }} />
                  </InputAdornment>
                ) : undefined,
              }}
            />
          </Grid>
          <Grid item xs={4}>
            <TextField
              label="Icon"
              value={form.icon ?? ''}
              onChange={set('icon')}
              fullWidth
              disabled={isSystemValue}
              placeholder="check_circle"
            />
          </Grid>
          <Grid item xs={12}>
            <TextField
              label="Description / Meta"
              value={form.meta ?? ''}
              onChange={set('meta')}
              fullWidth multiline rows={2}
              disabled={isSystemValue}
            />
          </Grid>
          <Grid item xs={6}>
            <FormControlLabel
              control={<Switch checked={form.isActive} onChange={setSwitch('isActive')} disabled={isSystemValue} />}
              label="Active"
            />
          </Grid>
          <Grid item xs={6}>
            <FormControlLabel
              control={<Switch checked={form.isDefault} onChange={setSwitch('isDefault')} disabled={isSystemValue} />}
              label="Default Value"
            />
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={saving}>Cancel</Button>
        {!isSystemValue && (
          <Button onClick={handleSave} variant="contained" disabled={saving} startIcon={saving ? <CircularProgress size={16} /> : undefined}>
            {isEdit ? 'Save' : 'Add'}
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
}

// ─────────────────────────────────────────────────────────────────
// Confirm Delete Dialog
// ─────────────────────────────────────────────────────────────────

function ConfirmDeleteDialog({ open, title, message, onConfirm, onClose }: {
  open: boolean; title: string; message: string; onConfirm: () => Promise<void>; onClose: () => void;
}) {
  const [deleting, setDeleting] = useState(false);
  const handleConfirm = async () => {
    setDeleting(true);
    try { await onConfirm(); onClose(); }
    finally { setDeleting(false); }
  };
  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent><Typography>{message}</Typography></DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={deleting}>Cancel</Button>
        <Button onClick={handleConfirm} color="error" variant="contained" disabled={deleting}
          startIcon={deleting ? <CircularProgress size={16} /> : undefined}>
          Delete
        </Button>
      </DialogActions>
    </Dialog>
  );
}

// ─────────────────────────────────────────────────────────────────
// Items Panel (shown when a category is selected)
// ─────────────────────────────────────────────────────────────────

interface ItemsPanelProps {
  category: LookupCategoryDto;
  onBack: () => void;
  onNotify: (msg: string, severity?: 'success' | 'error') => void;
}

function ItemsPanel({ category, onBack, onNotify }: ItemsPanelProps) {
  const [items, setItems] = useState<LookupItemDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [showInactive, setShowInactive] = useState(false);
  const [itemFormOpen, setItemFormOpen] = useState(false);
  const [editItem, setEditItem] = useState<LookupItemDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<LookupItemDto | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const data = await getItems(category.id, { includeInactive: showInactive });
      setItems(data);
    } catch {
      onNotify('Failed to load items', 'error');
    } finally {
      setLoading(false);
    }
  }, [category.id, showInactive, onNotify]);

  useEffect(() => { load(); }, [load]);

  const handleAdd = async (dto: CreateLookupItemDto | UpdateLookupItemDto) => {
    await createItem(category.id, dto as CreateLookupItemDto);
    onNotify(`Item '${dto.value}' created`);
    load();
  };

  const handleEdit = async (dto: CreateLookupItemDto | UpdateLookupItemDto) => {
    if (!editItem) return;
    await updateItem(editItem.id, dto as UpdateLookupItemDto);
    onNotify(`Item '${dto.value}' updated`);
    load();
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    await deleteItem(deleteTarget.id);
    onNotify(`Item '${deleteTarget.value}' deleted`);
    load();
  };

  const moveItem = async (index: number, direction: 'up' | 'down') => {
    const arr = [...items];
    const swap = direction === 'up' ? index - 1 : index + 1;
    if (swap < 0 || swap >= arr.length) return;
    [arr[index], arr[swap]] = [arr[swap], arr[index]];
    setItems(arr);
    await reorderItems(category.id, arr.map(i => i.id));
  };

  return (
    <Box>
      {/* Header */}
      <Stack direction="row" alignItems="center" spacing={2} sx={{ mb: 2 }}>
        <IconButton onClick={onBack} size="small">
          <BackIcon />
        </IconButton>
        <Box sx={{ flex: 1 }}>
          <Typography variant="h6" sx={{ fontWeight: 600 }}>
            {category.name}
            {category.isSystemManaged && (
              <Chip label="System" size="small" color="warning" sx={{ ml: 1 }} icon={<SystemIcon />} />
            )}
          </Typography>
          {category.entityType && (
            <Typography variant="caption" color="text.secondary">
              {category.entityType} › {category.propertyName}
            </Typography>
          )}
        </Box>
        <FormControlLabel
          control={<Switch size="small" checked={showInactive} onChange={e => setShowInactive(e.target.checked)} />}
          label="Show inactive"
          sx={{ mr: 1 }}
        />
        {category.allowCustomValues && (
          <Button variant="contained" size="small" startIcon={<AddIcon />}
            onClick={() => { setEditItem(null); setItemFormOpen(true); }}>
            Add Item
          </Button>
        )}
      </Stack>

      <Divider sx={{ mb: 2 }} />

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : items.length === 0 ? (
        <Alert severity="info">No items. {category.allowCustomValues ? 'Click "Add Item" to create the first one.' : ''}</Alert>
      ) : (
        <TableContainer component={Paper} variant="outlined">
          <Table size="small">
            <TableHead>
              <TableRow sx={{ '& th': { fontWeight: 600 } }}>
                <TableCell width={40}>#</TableCell>
                <TableCell>Key</TableCell>
                <TableCell>Display Value</TableCell>
                <TableCell width={80}>Color</TableCell>
                <TableCell width={80}>Status</TableCell>
                <TableCell width={80}>Default</TableCell>
                <TableCell width={80}>System</TableCell>
                <TableCell width={130}>Sort</TableCell>
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
                    <Typography variant="body2" sx={{ fontFamily: 'monospace', fontSize: '0.75rem', bgcolor: 'action.hover', px: 0.5, borderRadius: 0.5 }}>
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
                    <Chip
                      label={item.isActive ? 'Active' : 'Inactive'}
                      size="small"
                      color={item.isActive ? 'success' : 'default'}
                      variant="outlined"
                    />
                  </TableCell>
                  <TableCell>
                    {item.isDefault && <Chip label="Default" size="small" color="primary" />}
                  </TableCell>
                  <TableCell>
                    {item.isSystemValue && <Chip label="System" size="small" color="warning" icon={<SystemIcon />} />}
                  </TableCell>
                  <TableCell>
                    <Stack direction="row" spacing={0.5}>
                      <IconButton size="small" onClick={() => moveItem(idx, 'up')} disabled={idx === 0} title="Move up">
                        <MoveUpIcon fontSize="small" />
                      </IconButton>
                      <IconButton size="small" onClick={() => moveItem(idx, 'down')} disabled={idx === items.length - 1} title="Move down">
                        <MoveDownIcon fontSize="small" />
                      </IconButton>
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
          sortOrder: editItem.sortOrder, isActive: editItem.isActive, isDefault: editItem.isDefault,
          color: editItem.color ?? '', icon: editItem.icon ?? '', validationRules: editItem.validationRules ?? '',
        } : {}}
        isEdit={!!editItem}
        isSystemValue={editItem?.isSystemValue ?? false}
        onSave={editItem ? handleEdit : handleAdd}
        onClose={() => setItemFormOpen(false)}
      />

      {/* Delete Confirm */}
      <ConfirmDeleteDialog
        open={!!deleteTarget}
        title="Delete Item"
        message={`Delete item "${deleteTarget?.value}" (${deleteTarget?.key})? This cannot be undone.`}
        onConfirm={handleDelete}
        onClose={() => setDeleteTarget(null)}
      />
    </Box>
  );
}

// ─────────────────────────────────────────────────────────────────
// Categories List Panel
// ─────────────────────────────────────────────────────────────────

interface CategoriesListProps {
  onSelect: (cat: LookupCategoryDto) => void;
  onNotify: (msg: string, severity?: 'success' | 'error') => void;
}

function CategoriesList({ onSelect, onNotify }: CategoriesListProps) {
  const [categories, setCategories] = useState<LookupCategoryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [filterTab, setFilterTab] = useState(0); // 0=All,1=Active,2=Inactive
  const [showInactive, setShowInactive] = useState(false);
  const [catFormOpen, setCatFormOpen] = useState(false);
  const [editCat, setEditCat] = useState<LookupCategoryDto | null>(null);
  const [deleteCat, setDeleteCat] = useState<LookupCategoryDto | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const data = await getCategories({ includeInactive: showInactive });
      setCategories(data);
    } catch {
      onNotify('Failed to load categories', 'error');
    } finally {
      setLoading(false);
    }
  }, [showInactive, onNotify]);

  useEffect(() => { load(); }, [load]);

  const filtered = categories.filter(c => {
    const matchSearch = !search || c.name.toLowerCase().includes(search.toLowerCase())
      || (c.entityType ?? '').toLowerCase().includes(search.toLowerCase())
      || (c.description ?? '').toLowerCase().includes(search.toLowerCase());
    const matchTab = filterTab === 0 ? true : filterTab === 1 ? c.isActive : !c.isActive;
    return matchSearch && matchTab;
  });

  const handleAddCat = async (dto: CreateLookupCategoryDto | UpdateLookupCategoryDto) => {
    await createCategory(dto as CreateLookupCategoryDto);
    onNotify(`Category '${dto.name}' created`);
    load();
  };

  const handleEditCat = async (dto: CreateLookupCategoryDto | UpdateLookupCategoryDto) => {
    if (!editCat) return;
    await updateCategory(editCat.id, dto as UpdateLookupCategoryDto);
    onNotify(`Category '${dto.name}' updated`);
    load();
  };

  const handleDeleteCat = async () => {
    if (!deleteCat) return;
    await deleteCategory(deleteCat.id);
    onNotify(`Category '${deleteCat.name}' deleted`);
    load();
  };

  return (
    <Box>
      {/* Toolbar */}
      <Stack direction="row" spacing={2} alignItems="center" sx={{ mb: 2 }}>
        <TextField
          placeholder="Search categories…"
          value={search}
          onChange={e => setSearch(e.target.value)}
          size="small"
          sx={{ width: 260 }}
          InputProps={{ startAdornment: <InputAdornment position="start"><SearchIcon fontSize="small" /></InputAdornment> }}
        />
        <FormControlLabel
          control={<Switch size="small" checked={showInactive} onChange={e => setShowInactive(e.target.checked)} />}
          label="Show inactive"
        />
        <Box sx={{ flex: 1 }} />
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => { setEditCat(null); setCatFormOpen(true); }}>
          Add Category
        </Button>
      </Stack>

      <Tabs value={filterTab} onChange={(_, v) => setFilterTab(v)} sx={{ mb: 2 }}>
        <Tab label={`All (${categories.length})`} />
        <Tab label={`Active (${categories.filter(c => c.isActive).length})`} />
        <Tab label={`Inactive (${categories.filter(c => !c.isActive).length})`} />
      </Tabs>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : filtered.length === 0 ? (
        <Alert severity="info">No categories found. {!search && 'Click "Add Category" to create one.'}</Alert>
      ) : (
        <Grid container spacing={2}>
          {filtered.map(cat => (
            <Grid item xs={12} sm={6} md={4} key={cat.id}>
              <Card
                variant="outlined"
                sx={{
                  cursor: 'pointer',
                  transition: 'box-shadow 0.15s',
                  '&:hover': { boxShadow: 3 },
                  opacity: cat.isActive ? 1 : 0.65,
                  position: 'relative',
                }}
                onClick={() => onSelect(cat)}
              >
                <CardHeader
                  avatar={<CategoryIcon color={cat.isSystemManaged ? 'warning' : 'primary'} />}
                  title={
                    <Stack direction="row" spacing={1} alignItems="center">
                      <Typography variant="subtitle1" sx={{ fontWeight: 600, fontSize: '0.9rem' }}>{cat.name}</Typography>
                      {cat.isSystemManaged && <Chip label="System" size="small" color="warning" />}
                      {!cat.isActive && <Chip label="Inactive" size="small" />}
                    </Stack>
                  }
                  subheader={
                    cat.entityType ? (
                      <Typography variant="caption" color="text.secondary">
                        {cat.entityType} › {cat.propertyName}
                      </Typography>
                    ) : (
                      <Typography variant="caption" color="text.secondary">{cat.description ?? ''}</Typography>
                    )
                  }
                  action={
                    <Box onClick={e => e.stopPropagation()}>
                      <Tooltip title="Edit">
                        <IconButton size="small" onClick={() => { setEditCat(cat); setCatFormOpen(true); }}>
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      {!cat.isSystemManaged && (
                        <Tooltip title="Delete">
                          <IconButton size="small" color="error" onClick={() => setDeleteCat(cat)}>
                            <DeleteIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      )}
                    </Box>
                  }
                />
                <CardContent sx={{ pt: 0 }}>
                  <Stack direction="row" spacing={1} alignItems="center">
                    <ItemsIcon fontSize="small" color="action" />
                    <Typography variant="body2" color="text.secondary">
                      {cat.itemCount} {cat.itemCount === 1 ? 'item' : 'items'}
                    </Typography>
                    {cat.allowCustomValues ? (
                      <Chip label="Custom allowed" size="small" color="info" variant="outlined" />
                    ) : (
                      <Chip label="Fixed" size="small" variant="outlined" />
                    )}
                  </Stack>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}

      {/* Category Form */}
      <CategoryFormDialog
        open={catFormOpen}
        initial={editCat ? {
          name: editCat.name, description: editCat.description ?? '',
          entityType: editCat.entityType ?? '', propertyName: editCat.propertyName ?? '',
          isActive: editCat.isActive, allowCustomValues: editCat.allowCustomValues,
          validationSchema: editCat.validationSchema ?? '',
        } : {}}
        isEdit={!!editCat}
        onSave={editCat ? handleEditCat : handleAddCat}
        onClose={() => setCatFormOpen(false)}
      />

      {/* Delete Confirm */}
      <ConfirmDeleteDialog
        open={!!deleteCat}
        title="Delete Category"
        message={`Delete category "${deleteCat?.name}" and all its items? This cannot be undone.`}
        onConfirm={handleDeleteCat}
        onClose={() => setDeleteCat(null)}
      />
    </Box>
  );
}

// ─────────────────────────────────────────────────────────────────
// Main Page
// ─────────────────────────────────────────────────────────────────

export default function EnumManagementPage() {
  const [selectedCategory, setSelectedCategory] = useState<LookupCategoryDto | null>(null);
  const [toast, setToast] = useState<{ message: string; severity: 'success' | 'error' } | null>(null);

  const notify = useCallback((message: string, severity: 'success' | 'error' = 'success') => {
    setToast({ message, severity });
  }, []);

  return (
    <Box sx={{ p: 3 }}>
      {/* Page Header */}
      <Stack direction="row" alignItems="center" spacing={2} sx={{ mb: 3 }}>
        <CategoryIcon color="primary" sx={{ fontSize: 32 }} />
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 700 }}>Enum Management</Typography>
          <Typography variant="body2" color="text.secondary">
            Manage dropdown values, statuses, priorities and other configurable enum categories
          </Typography>
        </Box>
      </Stack>

      <Divider sx={{ mb: 3 }} />

      {selectedCategory ? (
        <ItemsPanel
          category={selectedCategory}
          onBack={() => setSelectedCategory(null)}
          onNotify={notify}
        />
      ) : (
        <CategoriesList
          onSelect={setSelectedCategory}
          onNotify={notify}
        />
      )}

      {/* Toast */}
      <Snackbar
        open={!!toast}
        autoHideDuration={3500}
        onClose={() => setToast(null)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        {toast ? (
          <Alert onClose={() => setToast(null)} severity={toast.severity} variant="filled">
            {toast.message}
          </Alert>
        ) : undefined}
      </Snackbar>
    </Box>
  );
}

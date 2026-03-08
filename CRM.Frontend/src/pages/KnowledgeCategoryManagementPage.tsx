/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 * Licensed under the Source-Available License (see LICENSE) v3.0
 */
// KB-017: Knowledge Category Management Page — hierarchical CRUD UI

import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Paper,
  Typography,
  TextField,
  Button,
  CircularProgress,
  Alert,
  Stack,
  Divider,
  Tooltip,
  IconButton,
  MenuItem,
  FormControlLabel,
  Switch,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  Save as SaveIcon,
  FolderOpen as FolderOpenIcon,
  Category as CategoryIcon,
} from '@mui/icons-material';
import { SimpleTreeView, TreeItem } from '@mui/x-tree-view'; // KB-017: v8 API
import { useFormik } from 'formik';
import * as Yup from 'yup';
import {
  knowledgeBaseService,
  type KnowledgeCategoryDto,
  type KnowledgeCategoryTreeDto, // KB-017
} from '../services/knowledgeBaseService';

// ============================
// Helpers
// ============================

/** KB-017: Build a hierarchy tree from a flat category list. */
function buildTree(flat: KnowledgeCategoryDto[]): KnowledgeCategoryTreeDto[] {
  const map: Record<number, KnowledgeCategoryTreeDto> = {};
  const roots: KnowledgeCategoryTreeDto[] = [];

  for (const cat of flat) {
    map[cat.id] = { ...cat, children: [] };
  }
  for (const node of Object.values(map)) {
    // KB-017: != null safely checks both undefined and null
    if (node.parentId != null && map[node.parentId]) {
      map[node.parentId].children.push(node);
    } else {
      roots.push(node);
    }
  }

  const sortChildren = (nodes: KnowledgeCategoryTreeDto[]) => {
    nodes.sort((a, b) => a.displayOrder - b.displayOrder);
    for (const n of nodes) sortChildren(n.children);
  };
  sortChildren(roots);
  return roots;
}

function generateSlug(name: string): string { // KB-017
  return name.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '');
}

// ============================
// Validation schema
// ============================

const categorySchema = Yup.object({
  name: Yup.string().required('Name is required').max(200, 'Max 200 characters'),
  description: Yup.string().max(500, 'Max 500 characters'),
  slug: Yup.string().max(200),
  icon: Yup.string().max(100),
  displayOrder: Yup.number().integer().min(0),
  isActive: Yup.boolean(),
});

interface CategoryFormValues { // KB-017
  name: string;
  description: string;
  slug: string;
  icon: string;
  displayOrder: number;
  isActive: boolean;
  parentId: number | null;
}

const EMPTY_FORM: CategoryFormValues = { // KB-017
  name: '',
  description: '',
  slug: '',
  icon: '',
  displayOrder: 0,
  isActive: true,
  parentId: null,
};

// ============================
// Tree rendering component
// ============================

function CategoryTreeNodes({ nodes }: { nodes: KnowledgeCategoryTreeDto[] }) { // KB-017
  return (
    <>
      {nodes.map((node) => (
        <TreeItem
          key={String(node.id)}
          itemId={String(node.id)}
          label={
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, py: 0.25 }}>
              {node.icon ? (
                <span style={{ fontSize: 16 }}>{node.icon}</span>
              ) : (
                <CategoryIcon fontSize="small" sx={{ opacity: 0.5 }} />
              )}
              <Typography variant="body2" noWrap sx={{ flex: 1 }}>
                {node.name}
              </Typography>
              {node.articleCount > 0 && (
                <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                  ({node.articleCount})
                </Typography>
              )}
            </Box>
          }
        >
          {node.children.length > 0 && (
            <CategoryTreeNodes nodes={node.children} />
          )}
        </TreeItem>
      ))}
    </>
  );
}

// ============================
// Main page
// ============================

export default function KnowledgeCategoryManagementPage() { // KB-017
  const [flatCategories, setFlatCategories] = useState<KnowledgeCategoryDto[]>([]);
  const [tree, setTree] = useState<KnowledgeCategoryTreeDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [isNew, setIsNew] = useState(false);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(false);

  // ---- Load categories ----

  const loadCategories = useCallback(async () => {
    try {
      setLoading(true);
      const data = await knowledgeBaseService.getCategories();
      setFlatCategories(data);
      setTree(buildTree(data));
      setError(null);
    } catch (err: unknown) {
      const apiErr = err as { response?: { data?: { message?: string } } };
      setError(apiErr?.response?.data?.message ?? 'Failed to load categories');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadCategories();
  }, [loadCategories]);

  // ---- Formik ----

  const formik = useFormik<CategoryFormValues>({
    initialValues: EMPTY_FORM,
    validationSchema: categorySchema,
    enableReinitialize: true,
    onSubmit: async (values) => {
      setSaving(true);
      setSuccessMessage(null);
      setError(null);
      try {
        if (isNew) {
          const created = await knowledgeBaseService.createCategory({
            name: values.name,
            description: values.description || undefined,
            slug: values.slug || generateSlug(values.name),
            icon: values.icon || undefined,
            displayOrder: values.displayOrder,
            isActive: values.isActive,
            parentId: values.parentId,
          });
          setSuccessMessage(`Category "${created.name}" created.`);
          setIsNew(false);
          setSelectedId(created.id);
        } else if (selectedId !== null) {
          const updated = await knowledgeBaseService.updateCategory(selectedId, {
            name: values.name,
            description: values.description || undefined,
            slug: values.slug || generateSlug(values.name),
            icon: values.icon || undefined,
            displayOrder: values.displayOrder,
            isActive: values.isActive,
            parentId: values.parentId,
          });
          setSuccessMessage(`Category "${updated.name}" updated.`);
        }
        await loadCategories();
      } catch (err: unknown) {
        const apiErr = err as { response?: { data?: { message?: string } } };
        setError(apiErr?.response?.data?.message ?? 'Failed to save category');
      } finally {
        setSaving(false);
      }
    },
  });

  // ---- Select a category ----

  function handleSelect(id: number) { // KB-017
    const cat = flatCategories.find((c) => c.id === id);
    if (!cat) return;
    setSelectedId(id);
    setIsNew(false);
    formik.setValues({
      name: cat.name,
      description: cat.description ?? '',
      slug: cat.slug,
      icon: cat.icon ?? '',
      displayOrder: cat.displayOrder,
      isActive: cat.isActive,
      parentId: cat.parentId ?? null,
    });
    setSuccessMessage(null);
  }

  // ---- Tree selection event (SimpleTreeView v8 onSelectedItemsChange) ----

  function handleTreeSelect(
    _: React.SyntheticEvent | null,
    itemId: string | null,
  ) { // KB-017
    if (itemId === null) return;
    const id = parseInt(itemId, 10);
    if (!isNaN(id)) handleSelect(id);
  }

  // ---- Add new ----

  function handleAddNew(parentId: number | null = null) { // KB-017
    setSelectedId(null);
    setIsNew(true);
    formik.resetForm({ values: { ...EMPTY_FORM, parentId } });
    setSuccessMessage(null);
    setError(null);
  }

  // ---- Delete ----

  async function handleDelete() { // KB-017
    if (selectedId === null) return;
    const cat = flatCategories.find((c) => c.id === selectedId);
    if (!cat) return;
    if (
      !window.confirm(
        `Delete category "${cat.name}"? Articles in this category will lose their category assignment.`,
      )
    )
      return;

    setDeleting(true);
    try {
      await knowledgeBaseService.deleteCategory(selectedId);
      setSuccessMessage(`Category "${cat.name}" deleted.`);
      setSelectedId(null);
      formik.resetForm();
      setIsNew(false);
      await loadCategories();
    } catch (err: unknown) {
      const apiErr = err as { response?: { data?: { message?: string } } };
      setError(apiErr?.response?.data?.message ?? 'Failed to delete category');
    } finally {
      setDeleting(false);
    }
  }

  // Parent category options (exclude self when editing)
  const parentOptions = flatCategories.filter((c) => c.id !== selectedId);

  return (
    <Box sx={{ p: 2 }}>
      {/* Page header */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
        <Box display="flex" alignItems="center" gap={1}>
          <FolderOpenIcon color="primary" />
          <Typography variant="h5">Knowledge Base Categories</Typography>
        </Box>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleAddNew()}>
          New Category
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}
      {successMessage && (
        <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccessMessage(null)}>
          {successMessage}
        </Alert>
      )}

      <Box display="flex" gap={2} sx={{ minHeight: 500 }}>
        {/* ── Left panel: Category tree ── */}
        <Paper sx={{ width: 280, p: 2, flexShrink: 0, overflow: 'auto' }}>
          <Box display="flex" justifyContent="space-between" alignItems="center" mb={1}>
            <Typography variant="subtitle1" fontWeight="medium">
              Categories
            </Typography>
            <Tooltip title="Add root category">
              <IconButton size="small" onClick={() => handleAddNew(null)}>
                <AddIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          </Box>
          <Divider sx={{ mb: 1 }} />

          {loading ? (
            <Box display="flex" justifyContent="center" pt={4}>
              <CircularProgress size={24} />
            </Box>
          ) : tree.length === 0 ? (
            <Typography
              color="text.secondary"
              variant="body2"
              sx={{ pt: 2, textAlign: 'center' }}
            >
              No categories yet. Click + to add one.
            </Typography>
          ) : (
            <SimpleTreeView // KB-017: @mui/x-tree-view v8
              selectedItems={selectedId !== null ? String(selectedId) : null}
              onSelectedItemsChange={handleTreeSelect}
            >
              <CategoryTreeNodes nodes={tree} />
            </SimpleTreeView>
          )}
        </Paper>

        {/* ── Right panel: Edit form ── */}
        <Paper sx={{ flex: 1, p: 3 }}>
          {!isNew && selectedId === null ? (
            <Box
              display="flex"
              alignItems="center"
              justifyContent="center"
              sx={{ height: '100%', minHeight: 200 }}
            >
              <Typography color="text.secondary">
                Select a category to edit, or click "New Category".
              </Typography>
            </Box>
          ) : (
            <form onSubmit={formik.handleSubmit}>
              <Typography variant="h6" mb={2}>
                {isNew ? 'New Category' : 'Edit Category'}
              </Typography>

              <Stack spacing={2}>
                {/* Name — auto-generates slug when creating */}
                <TextField
                  label="Name *"
                  name="name"
                  value={formik.values.name}
                  onChange={(e) => {
                    formik.handleChange(e);
                    if (isNew) {
                      formik.setFieldValue('slug', generateSlug(e.target.value)); // KB-017
                    }
                  }}
                  onBlur={formik.handleBlur}
                  error={formik.touched.name && Boolean(formik.errors.name)}
                  helperText={formik.touched.name && formik.errors.name}
                  fullWidth
                />

                <TextField
                  label="Description"
                  name="description"
                  value={formik.values.description}
                  onChange={formik.handleChange}
                  onBlur={formik.handleBlur}
                  error={formik.touched.description && Boolean(formik.errors.description)}
                  helperText={formik.touched.description && formik.errors.description}
                  multiline
                  rows={2}
                  fullWidth
                />

                <TextField
                  label="Slug"
                  name="slug"
                  value={formik.values.slug}
                  onChange={formik.handleChange}
                  onBlur={formik.handleBlur}
                  error={formik.touched.slug && Boolean(formik.errors.slug)}
                  helperText={
                    (formik.touched.slug && formik.errors.slug) ||
                    'URL-friendly identifier (auto-generated from name)'
                  }
                  fullWidth
                />

                <TextField
                  label="Icon"
                  name="icon"
                  value={formik.values.icon}
                  onChange={formik.handleChange}
                  onBlur={formik.handleBlur}
                  error={formik.touched.icon && Boolean(formik.errors.icon)}
                  helperText={
                    (formik.touched.icon && formik.errors.icon) ||
                    'Icon name or emoji (e.g. "folder", 📁)'
                  }
                  fullWidth
                />

                {/* Additional Information section */}
                <Divider textAlign="left">
                  <Typography variant="caption" color="text.secondary">
                    Additional Information
                  </Typography>
                </Divider>

                <TextField
                  select
                  label="Parent Category"
                  name="parentId"
                  value={formik.values.parentId ?? ''}
                  onChange={(e) => {
                    const val = e.target.value;
                    formik.setFieldValue('parentId', val === '' ? null : Number(val)); // KB-017
                  }}
                  fullWidth
                  helperText="Leave empty for a top-level category"
                >
                  <MenuItem value="">— None (top level) —</MenuItem>
                  {parentOptions.map((cat) => (
                    <MenuItem key={cat.id} value={cat.id}>
                      {cat.name}
                    </MenuItem>
                  ))}
                </TextField>

                <TextField
                  label="Display Order"
                  name="displayOrder"
                  type="number"
                  value={formik.values.displayOrder}
                  onChange={formik.handleChange}
                  onBlur={formik.handleBlur}
                  error={formik.touched.displayOrder && Boolean(formik.errors.displayOrder)}
                  helperText={formik.touched.displayOrder && formik.errors.displayOrder}
                  inputProps={{ min: 0 }}
                  sx={{ width: 160 }}
                />

                <FormControlLabel
                  control={
                    <Switch
                      name="isActive"
                      checked={formik.values.isActive}
                      onChange={formik.handleChange}
                    />
                  }
                  label="Active"
                />

                <Divider />

                <Box display="flex" gap={2} justifyContent="space-between">
                  <Button
                    type="submit"
                    variant="contained"
                    startIcon={
                      saving ? (
                        <CircularProgress size={16} color="inherit" />
                      ) : (
                        <SaveIcon />
                      )
                    }
                    disabled={saving || !formik.isValid}
                  >
                    {saving ? 'Saving…' : isNew ? 'Create' : 'Save Changes'}
                  </Button>

                  {!isNew && selectedId !== null && (
                    <Button
                      variant="outlined"
                      color="error"
                      startIcon={
                        deleting ? (
                          <CircularProgress size={16} color="inherit" />
                        ) : (
                          <DeleteIcon />
                        )
                      }
                      onClick={handleDelete}
                      disabled={deleting || saving}
                    >
                      Delete
                    </Button>
                  )}
                </Box>
              </Stack>
            </form>
          )}
        </Paper>
      </Box>
    </Box>
  );
}

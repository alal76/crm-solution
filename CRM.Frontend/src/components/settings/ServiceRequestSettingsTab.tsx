/**
 * CRM Solution - Service Request Settings Tab
 * Wrapper component for embedding service request settings in admin settings page
 */

import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  CircularProgress,
  Alert,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  MenuItem,
  Stack,
  Chip,
  IconButton,
  Tooltip,
  FormControl,
  InputLabel,
  Select,
  Grid,
  Tabs,
  Tab,
  Switch,
  FormControlLabel,
  Paper,
  Divider,
  SelectChangeEvent,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import ArrowUpwardIcon from '@mui/icons-material/ArrowUpward';
import ArrowDownwardIcon from '@mui/icons-material/ArrowDownward';
import CategoryIcon from '@mui/icons-material/Category';
import SubdirectoryArrowRightIcon from '@mui/icons-material/SubdirectoryArrowRight';
import InputIcon from '@mui/icons-material/Input';
import {
  ServiceRequestCategory,
  CreateServiceRequestCategory,
  ServiceRequestSubcategory,
  CreateServiceRequestSubcategory,
  ServiceRequestCustomFieldDefinition,
  CreateServiceRequestCustomFieldDefinition,
  ServiceRequestPriority,
  CustomFieldType,
  serviceRequestCategoryService,
  serviceRequestSubcategoryService,
  serviceRequestCustomFieldService,
} from '../../services/apiService';
import apiClient from '../../services/apiClient';

interface UserGroup {
  id: number;
  name: string;
}

interface Workflow {
  id: number;
  name: string;
}

const PRIORITY_LABELS: { [key: number]: string } = {
  [ServiceRequestPriority.Low]: 'Low',
  [ServiceRequestPriority.Medium]: 'Medium',
  [ServiceRequestPriority.High]: 'High',
  [ServiceRequestPriority.Critical]: 'Critical',
  [ServiceRequestPriority.Urgent]: 'Urgent',
};

const FIELD_TYPE_LABELS: { [key: number]: string } = {
  [CustomFieldType.Text]: 'Text',
  [CustomFieldType.TextArea]: 'Text Area',
  [CustomFieldType.Number]: 'Number',
  [CustomFieldType.Decimal]: 'Decimal',
  [CustomFieldType.Date]: 'Date',
  [CustomFieldType.DateTime]: 'Date & Time',
  [CustomFieldType.Dropdown]: 'Dropdown',
  [CustomFieldType.MultiSelect]: 'Multi-Select',
  [CustomFieldType.Boolean]: 'Yes/No',
  [CustomFieldType.Email]: 'Email',
  [CustomFieldType.Phone]: 'Phone',
  [CustomFieldType.Url]: 'URL',
};

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`sr-settings-tabpanel-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ pt: 2 }}>{children}</Box>}
    </div>
  );
}

function ServiceRequestSettingsTab() {
  const [tabValue, setTabValue] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Categories
  const [categories, setCategories] = useState<ServiceRequestCategory[]>([]);
  const [categoryDialogOpen, setCategoryDialogOpen] = useState(false);
  const [selectedCategory, setSelectedCategory] = useState<ServiceRequestCategory | null>(null);
  const [categoryForm, setCategoryForm] = useState<CreateServiceRequestCategory>({
    name: '',
    description: '',
    isActive: true,
    defaultPriority: ServiceRequestPriority.Medium,
  });

  // Subcategories
  const [subcategories, setSubcategories] = useState<ServiceRequestSubcategory[]>([]);
  const [subcategoryDialogOpen, setSubcategoryDialogOpen] = useState(false);
  const [selectedSubcategory, setSelectedSubcategory] = useState<ServiceRequestSubcategory | null>(null);
  const [subcategoryForm, setSubcategoryForm] = useState<CreateServiceRequestSubcategory>({
    categoryId: 0,
    name: '',
    description: '',
    isActive: true,
  });

  // Custom Fields
  const [customFields, setCustomFields] = useState<ServiceRequestCustomFieldDefinition[]>([]);
  const [fieldDialogOpen, setFieldDialogOpen] = useState(false);
  const [selectedField, setSelectedField] = useState<ServiceRequestCustomFieldDefinition | null>(null);
  const [fieldForm, setFieldForm] = useState<CreateServiceRequestCustomFieldDefinition>({
    name: '',
    label: '',
    description: '',
    fieldType: CustomFieldType.Text,
    isRequired: false,
    isActive: true,
  });
  const [activeFieldCount, setActiveFieldCount] = useState(0);

  // Reference data
  const [groups, setGroups] = useState<UserGroup[]>([]);
  const [workflows, setWorkflows] = useState<Workflow[]>([]);

  // Fetch all data
  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const [catRes, subRes, fieldRes, groupRes, workflowRes, countRes] = await Promise.all([
          serviceRequestCategoryService.getAll(true),
          serviceRequestSubcategoryService.getAll(true),
          serviceRequestCustomFieldService.getAll(true),
          apiClient.get('/user-groups'),
          apiClient.get('/workflows'),
          serviceRequestCustomFieldService.getCount(),
        ]);
        setCategories(catRes.data);
        setSubcategories(subRes.data);
        setCustomFields(fieldRes.data);
        setGroups(groupRes.data);
        setWorkflows(workflowRes.data);
        setActiveFieldCount(countRes.data.activeCount);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to load settings');
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  // Category handlers
  const handleAddCategory = () => {
    setCategoryForm({
      name: '',
      description: '',
      isActive: true,
      defaultPriority: ServiceRequestPriority.Medium,
    });
    setSelectedCategory(null);
    setCategoryDialogOpen(true);
  };

  const handleEditCategory = (category: ServiceRequestCategory) => {
    setCategoryForm({
      name: category.name,
      description: category.description,
      icon: category.icon,
      color: category.color,
      isActive: category.isActive,
      defaultPriority: category.defaultPriority,
      slaResponseHours: category.slaResponseHours,
      slaResolutionHours: category.slaResolutionHours,
    });
    setSelectedCategory(category);
    setCategoryDialogOpen(true);
  };

  const handleDeleteCategory = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this category?')) {
      try {
        await serviceRequestCategoryService.delete(id);
        setCategories(categories.filter((c) => c.id !== id));
        setSuccessMessage('Category deleted successfully');
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete category');
      }
    }
  };

  const handleSaveCategory = async () => {
    try {
      if (!categoryForm.name) {
        setError('Category name is required');
        return;
      }

      if (selectedCategory) {
        const response = await serviceRequestCategoryService.update(selectedCategory.id!, categoryForm);
        setCategories(categories.map((c) => (c.id === selectedCategory.id ? response.data : c)));
        setSuccessMessage('Category updated successfully');
      } else {
        const response = await serviceRequestCategoryService.create(categoryForm);
        setCategories([...categories, response.data]);
        setSuccessMessage('Category created successfully');
      }

      setCategoryDialogOpen(false);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save category');
    }
  };

  const handleMoveCategory = async (category: ServiceRequestCategory, direction: 'up' | 'down') => {
    const index = categories.findIndex((c) => c.id === category.id);
    if ((direction === 'up' && index === 0) || (direction === 'down' && index === categories.length - 1)) {
      return;
    }

    const newCategories = [...categories];
    const swapIndex = direction === 'up' ? index - 1 : index + 1;
    [newCategories[index], newCategories[swapIndex]] = [newCategories[swapIndex], newCategories[index]];
    
    try {
      await serviceRequestCategoryService.reorder(newCategories.map((c) => c.id!));
      setCategories(newCategories);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to reorder categories');
    }
  };

  // Subcategory handlers
  const handleAddSubcategory = () => {
    setSubcategoryForm({
      categoryId: categories.length > 0 ? categories[0].id! : 0,
      name: '',
      description: '',
      isActive: true,
    });
    setSelectedSubcategory(null);
    setSubcategoryDialogOpen(true);
  };

  const handleEditSubcategory = (subcategory: ServiceRequestSubcategory) => {
    setSubcategoryForm({
      categoryId: subcategory.categoryId,
      name: subcategory.name,
      description: subcategory.description,
      isActive: subcategory.isActive,
      defaultWorkflowId: subcategory.defaultWorkflowId,
      defaultAssigneeGroupId: subcategory.defaultAssigneeGroupId,
      slaResponseHours: subcategory.slaResponseHours,
      slaResolutionHours: subcategory.slaResolutionHours,
    });
    setSelectedSubcategory(subcategory);
    setSubcategoryDialogOpen(true);
  };

  const handleDeleteSubcategory = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this subcategory?')) {
      try {
        await serviceRequestSubcategoryService.delete(id);
        setSubcategories(subcategories.filter((s) => s.id !== id));
        setSuccessMessage('Subcategory deleted successfully');
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete subcategory');
      }
    }
  };

  const handleSaveSubcategory = async () => {
    try {
      if (!subcategoryForm.name || !subcategoryForm.categoryId) {
        setError('Name and category are required');
        return;
      }

      if (selectedSubcategory) {
        const response = await serviceRequestSubcategoryService.update(selectedSubcategory.id!, subcategoryForm);
        setSubcategories(subcategories.map((s) => (s.id === selectedSubcategory.id ? response.data : s)));
        setSuccessMessage('Subcategory updated successfully');
      } else {
        const response = await serviceRequestSubcategoryService.create(subcategoryForm);
        setSubcategories([...subcategories, response.data]);
        setSuccessMessage('Subcategory created successfully');
      }

      setSubcategoryDialogOpen(false);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save subcategory');
    }
  };

  // Custom field handlers
  const handleAddField = () => {
    setFieldForm({
      name: '',
      label: '',
      description: '',
      fieldType: CustomFieldType.Text,
      isRequired: false,
      isActive: true,
    });
    setSelectedField(null);
    setFieldDialogOpen(true);
  };

  const handleEditField = (field: ServiceRequestCustomFieldDefinition) => {
    setFieldForm({
      name: field.name,
      label: field.label,
      description: field.description,
      fieldType: field.fieldType,
      isRequired: field.isRequired,
      isActive: field.isActive,
      options: field.options,
      defaultValue: field.defaultValue,
      appliesToCategoryId: field.appliesToCategoryId,
      appliesToSubcategoryId: field.appliesToSubcategoryId,
    });
    setSelectedField(field);
    setFieldDialogOpen(true);
  };

  const handleDeleteField = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this custom field?')) {
      try {
        await serviceRequestCustomFieldService.delete(id);
        setCustomFields(customFields.filter((f) => f.id !== id));
        setActiveFieldCount((prev) => prev - 1);
        setSuccessMessage('Custom field deleted successfully');
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete custom field');
      }
    }
  };

  const handleSaveField = async () => {
    try {
      if (!fieldForm.name || !fieldForm.label) {
        setError('Name and label are required');
        return;
      }

      if (selectedField) {
        const response = await serviceRequestCustomFieldService.update(selectedField.id!, fieldForm);
        setCustomFields(customFields.map((f) => (f.id === selectedField.id ? response.data : f)));
        setSuccessMessage('Custom field updated successfully');
      } else {
        const response = await serviceRequestCustomFieldService.create(fieldForm);
        setCustomFields([...customFields, response.data]);
        setActiveFieldCount((prev) => prev + 1);
        setSuccessMessage('Custom field created successfully');
      }

      setFieldDialogOpen(false);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save custom field');
    }
  };

  const handleMoveField = async (field: ServiceRequestCustomFieldDefinition, direction: 'up' | 'down') => {
    const index = customFields.findIndex((f) => f.id === field.id);
    if ((direction === 'up' && index === 0) || (direction === 'down' && index === customFields.length - 1)) {
      return;
    }

    const newFields = [...customFields];
    const swapIndex = direction === 'up' ? index - 1 : index + 1;
    [newFields[index], newFields[swapIndex]] = [newFields[swapIndex], newFields[index]];
    
    try {
      await serviceRequestCustomFieldService.reorder(newFields.map((f) => f.id!));
      setCustomFields(newFields);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to reorder fields');
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h6" sx={{ fontWeight: 600, mb: 1 }}>
        Service Request Settings
      </Typography>
      <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
        Configure categories, subcategories, and custom fields for service requests.
      </Typography>

      {successMessage && (
        <Alert severity="success" sx={{ mb: 2, borderRadius: 2 }} onClose={() => setSuccessMessage(null)}>
          {successMessage}
        </Alert>
      )}

      {error && (
        <Alert severity="error" sx={{ mb: 2, borderRadius: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Quick Stats */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={4}>
          <Card sx={{ borderRadius: 2 }}>
            <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <CategoryIcon color="primary" sx={{ fontSize: 32 }} />
              <Box>
                <Typography variant="h4" sx={{ fontWeight: 700 }}>{categories.length}</Typography>
                <Typography variant="body2" color="textSecondary">Categories</Typography>
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={4}>
          <Card sx={{ borderRadius: 2 }}>
            <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <SubdirectoryArrowRightIcon color="secondary" sx={{ fontSize: 32 }} />
              <Box>
                <Typography variant="h4" sx={{ fontWeight: 700 }}>{subcategories.length}</Typography>
                <Typography variant="body2" color="textSecondary">Subcategories</Typography>
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={4}>
          <Card sx={{ borderRadius: 2 }}>
            <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <InputIcon color="success" sx={{ fontSize: 32 }} />
              <Box>
                <Typography variant="h4" sx={{ fontWeight: 700 }}>{activeFieldCount}/15</Typography>
                <Typography variant="body2" color="textSecondary">Custom Fields</Typography>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Inner Tabs */}
      <Paper sx={{ borderRadius: 3 }}>
        <Tabs
          value={tabValue}
          onChange={(e, newValue) => setTabValue(newValue)}
          variant="fullWidth"
          sx={{ borderBottom: 1, borderColor: 'divider' }}
        >
          <Tab label="Categories" icon={<CategoryIcon />} iconPosition="start" sx={{ textTransform: 'none' }} />
          <Tab label="Subcategories" icon={<SubdirectoryArrowRightIcon />} iconPosition="start" sx={{ textTransform: 'none' }} />
          <Tab label="Custom Fields" icon={<InputIcon />} iconPosition="start" sx={{ textTransform: 'none' }} />
        </Tabs>

        <Box sx={{ p: 2 }}>
          {/* Categories Tab */}
          <TabPanel value={tabValue} index={0}>
            <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
              <Button variant="contained" startIcon={<AddIcon />} onClick={handleAddCategory}>
                Add Category
              </Button>
            </Box>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Order</TableCell>
                  <TableCell>Name</TableCell>
                  <TableCell>Description</TableCell>
                  <TableCell>Priority</TableCell>
                  <TableCell>SLA</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {categories.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={7} align="center">
                      No categories found. Add one to get started.
                    </TableCell>
                  </TableRow>
                ) : (
                  categories.map((cat, index) => (
                    <TableRow key={cat.id}>
                      <TableCell>
                        <Stack direction="row" spacing={0.5}>
                          <IconButton size="small" onClick={() => handleMoveCategory(cat, 'up')} disabled={index === 0}>
                            <ArrowUpwardIcon fontSize="small" />
                          </IconButton>
                          <IconButton size="small" onClick={() => handleMoveCategory(cat, 'down')} disabled={index === categories.length - 1}>
                            <ArrowDownwardIcon fontSize="small" />
                          </IconButton>
                        </Stack>
                      </TableCell>
                      <TableCell>
                        <Typography fontWeight="medium">{cat.name}</Typography>
                      </TableCell>
                      <TableCell>{cat.description || '-'}</TableCell>
                      <TableCell>
                        <Chip label={PRIORITY_LABELS[cat.defaultPriority]} size="small" />
                      </TableCell>
                      <TableCell>
                        {cat.slaResponseHours || cat.slaResolutionHours
                          ? `${cat.slaResponseHours || '-'}h / ${cat.slaResolutionHours || '-'}h`
                          : '-'}
                      </TableCell>
                      <TableCell>
                        <Chip label={cat.isActive ? 'Active' : 'Inactive'} color={cat.isActive ? 'success' : 'default'} size="small" />
                      </TableCell>
                      <TableCell align="right">
                        <Tooltip title="Edit">
                          <IconButton size="small" onClick={() => handleEditCategory(cat)}>
                            <EditIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton size="small" color="error" onClick={() => handleDeleteCategory(cat.id!)}>
                            <DeleteIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </TabPanel>

          {/* Subcategories Tab */}
          <TabPanel value={tabValue} index={1}>
            <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
              <Button variant="contained" startIcon={<AddIcon />} onClick={handleAddSubcategory} disabled={categories.length === 0}>
                Add Subcategory
              </Button>
            </Box>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Category</TableCell>
                  <TableCell>Name</TableCell>
                  <TableCell>Description</TableCell>
                  <TableCell>Workflow</TableCell>
                  <TableCell>Assignee Group</TableCell>
                  <TableCell>SLA</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {subcategories.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={8} align="center">
                      No subcategories found. Add one to get started.
                    </TableCell>
                  </TableRow>
                ) : (
                  subcategories.map((sub) => (
                    <TableRow key={sub.id}>
                      <TableCell>{sub.categoryName || '-'}</TableCell>
                      <TableCell>
                        <Typography fontWeight="medium">{sub.name}</Typography>
                      </TableCell>
                      <TableCell>{sub.description || '-'}</TableCell>
                      <TableCell>{sub.defaultWorkflowName || '-'}</TableCell>
                      <TableCell>{sub.defaultAssigneeGroupName || '-'}</TableCell>
                      <TableCell>
                        {sub.slaResponseHours || sub.slaResolutionHours
                          ? `${sub.slaResponseHours || '-'}h / ${sub.slaResolutionHours || '-'}h`
                          : '-'}
                      </TableCell>
                      <TableCell>
                        <Chip label={sub.isActive ? 'Active' : 'Inactive'} color={sub.isActive ? 'success' : 'default'} size="small" />
                      </TableCell>
                      <TableCell align="right">
                        <Tooltip title="Edit">
                          <IconButton size="small" onClick={() => handleEditSubcategory(sub)}>
                            <EditIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton size="small" color="error" onClick={() => handleDeleteSubcategory(sub.id!)}>
                            <DeleteIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </TabPanel>

          {/* Custom Fields Tab */}
          <TabPanel value={tabValue} index={2}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="body2" color="textSecondary">
                Active custom fields: {activeFieldCount} / 15
              </Typography>
              <Button variant="contained" startIcon={<AddIcon />} onClick={handleAddField} disabled={activeFieldCount >= 15}>
                Add Custom Field
              </Button>
            </Box>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Order</TableCell>
                  <TableCell>Name</TableCell>
                  <TableCell>Label</TableCell>
                  <TableCell>Type</TableCell>
                  <TableCell>Required</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {customFields.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={7} align="center">
                      No custom fields found. Add one to get started.
                    </TableCell>
                  </TableRow>
                ) : (
                  customFields.map((field, index) => (
                    <TableRow key={field.id}>
                      <TableCell>
                        <Stack direction="row" spacing={0.5}>
                          <IconButton size="small" onClick={() => handleMoveField(field, 'up')} disabled={index === 0}>
                            <ArrowUpwardIcon fontSize="small" />
                          </IconButton>
                          <IconButton size="small" onClick={() => handleMoveField(field, 'down')} disabled={index === customFields.length - 1}>
                            <ArrowDownwardIcon fontSize="small" />
                          </IconButton>
                        </Stack>
                      </TableCell>
                      <TableCell>{field.name}</TableCell>
                      <TableCell>{field.label}</TableCell>
                      <TableCell>{FIELD_TYPE_LABELS[field.fieldType]}</TableCell>
                      <TableCell>
                        <Chip label={field.isRequired ? 'Yes' : 'No'} size="small" color={field.isRequired ? 'warning' : 'default'} />
                      </TableCell>
                      <TableCell>
                        <Chip label={field.isActive ? 'Active' : 'Inactive'} color={field.isActive ? 'success' : 'default'} size="small" />
                      </TableCell>
                      <TableCell align="right">
                        <Tooltip title="Edit">
                          <IconButton size="small" onClick={() => handleEditField(field)}>
                            <EditIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton size="small" color="error" onClick={() => handleDeleteField(field.id!)}>
                            <DeleteIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </TabPanel>
        </Box>
      </Paper>

      {/* Category Dialog */}
      <Dialog open={categoryDialogOpen} onClose={() => setCategoryDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{selectedCategory ? 'Edit Category' : 'Add Category'}</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Name"
                value={categoryForm.name}
                onChange={(e) => setCategoryForm({ ...categoryForm, name: e.target.value })}
                required
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Description"
                value={categoryForm.description}
                onChange={(e) => setCategoryForm({ ...categoryForm, description: e.target.value })}
                multiline
                rows={2}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Default Priority</InputLabel>
                <Select
                  value={categoryForm.defaultPriority}
                  onChange={(e: SelectChangeEvent<number>) => setCategoryForm({ ...categoryForm, defaultPriority: Number(e.target.value) })}
                  label="Default Priority"
                >
                  {Object.entries(PRIORITY_LABELS).map(([value, label]) => (
                    <MenuItem key={value} value={Number(value)}>{label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={categoryForm.isActive} onChange={(e) => setCategoryForm({ ...categoryForm, isActive: e.target.checked })} />}
                label="Active"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                type="number"
                label="SLA Response (hours)"
                value={categoryForm.slaResponseHours || ''}
                onChange={(e) => setCategoryForm({ ...categoryForm, slaResponseHours: e.target.value ? Number(e.target.value) : undefined })}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                type="number"
                label="SLA Resolution (hours)"
                value={categoryForm.slaResolutionHours || ''}
                onChange={(e) => setCategoryForm({ ...categoryForm, slaResolutionHours: e.target.value ? Number(e.target.value) : undefined })}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCategoryDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSaveCategory}>{selectedCategory ? 'Update' : 'Create'}</Button>
        </DialogActions>
      </Dialog>

      {/* Subcategory Dialog */}
      <Dialog open={subcategoryDialogOpen} onClose={() => setSubcategoryDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{selectedSubcategory ? 'Edit Subcategory' : 'Add Subcategory'}</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <FormControl fullWidth required>
                <InputLabel>Category</InputLabel>
                <Select
                  value={subcategoryForm.categoryId}
                  onChange={(e: SelectChangeEvent<number>) => setSubcategoryForm({ ...subcategoryForm, categoryId: Number(e.target.value) })}
                  label="Category"
                >
                  {categories.map((cat) => (
                    <MenuItem key={cat.id} value={cat.id}>{cat.name}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Name"
                value={subcategoryForm.name}
                onChange={(e) => setSubcategoryForm({ ...subcategoryForm, name: e.target.value })}
                required
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Description"
                value={subcategoryForm.description}
                onChange={(e) => setSubcategoryForm({ ...subcategoryForm, description: e.target.value })}
                multiline
                rows={2}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Default Workflow</InputLabel>
                <Select
                  value={subcategoryForm.defaultWorkflowId || ''}
                  onChange={(e: SelectChangeEvent<string | number>) => setSubcategoryForm({ ...subcategoryForm, defaultWorkflowId: e.target.value ? Number(e.target.value) : undefined })}
                  label="Default Workflow"
                >
                  <MenuItem value="">None</MenuItem>
                  {workflows.map((wf) => (
                    <MenuItem key={wf.id} value={wf.id}>{wf.name}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Default Assignee Group</InputLabel>
                <Select
                  value={subcategoryForm.defaultAssigneeGroupId || ''}
                  onChange={(e: SelectChangeEvent<string | number>) => setSubcategoryForm({ ...subcategoryForm, defaultAssigneeGroupId: e.target.value ? Number(e.target.value) : undefined })}
                  label="Default Assignee Group"
                >
                  <MenuItem value="">None</MenuItem>
                  {groups.map((g) => (
                    <MenuItem key={g.id} value={g.id}>{g.name}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={subcategoryForm.isActive} onChange={(e) => setSubcategoryForm({ ...subcategoryForm, isActive: e.target.checked })} />}
                label="Active"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSubcategoryDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSaveSubcategory}>{selectedSubcategory ? 'Update' : 'Create'}</Button>
        </DialogActions>
      </Dialog>

      {/* Custom Field Dialog */}
      <Dialog open={fieldDialogOpen} onClose={() => setFieldDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{selectedField ? 'Edit Custom Field' : 'Add Custom Field'}</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Field Name (internal)"
                value={fieldForm.name}
                onChange={(e) => setFieldForm({ ...fieldForm, name: e.target.value.replace(/\s/g, '_').toLowerCase() })}
                required
                helperText="No spaces, lowercase"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Display Label"
                value={fieldForm.label}
                onChange={(e) => setFieldForm({ ...fieldForm, label: e.target.value })}
                required
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Description"
                value={fieldForm.description}
                onChange={(e) => setFieldForm({ ...fieldForm, description: e.target.value })}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Field Type</InputLabel>
                <Select
                  value={fieldForm.fieldType}
                  onChange={(e: SelectChangeEvent<number>) => setFieldForm({ ...fieldForm, fieldType: Number(e.target.value) })}
                  label="Field Type"
                >
                  {Object.entries(FIELD_TYPE_LABELS).map(([value, label]) => (
                    <MenuItem key={value} value={Number(value)}>{label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            {(fieldForm.fieldType === CustomFieldType.Dropdown || fieldForm.fieldType === CustomFieldType.MultiSelect) && (
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Options (comma-separated)"
                  value={fieldForm.options || ''}
                  onChange={(e) => setFieldForm({ ...fieldForm, options: e.target.value })}
                  helperText="Enter options separated by commas"
                />
              </Grid>
            )}
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={fieldForm.isRequired} onChange={(e) => setFieldForm({ ...fieldForm, isRequired: e.target.checked })} />}
                label="Required"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={fieldForm.isActive} onChange={(e) => setFieldForm({ ...fieldForm, isActive: e.target.checked })} />}
                label="Active"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setFieldDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSaveField}>{selectedField ? 'Update' : 'Create'}</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default ServiceRequestSettingsTab;

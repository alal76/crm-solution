/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 * Licensed under the GNU Affero General Public License v3.0
 */

import {
  Box,
  Container,
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
  SelectChangeEvent,
} from '@mui/material';
import { useState, useEffect } from 'react';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import ArrowUpwardIcon from '@mui/icons-material/ArrowUpward';
import ArrowDownwardIcon from '@mui/icons-material/ArrowDownward';
import logo from '../assets/logo.png';
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
} from '../services/apiService';
import apiClient from '../services/apiClient';

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
      id={`settings-tabpanel-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ py: 3 }}>{children}</Box>}
    </div>
  );
}

function ServiceRequestSettingsPage() {
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
        setError('Category and name are required');
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
    if (activeFieldCount >= 15) {
      setError('Maximum of 15 active custom fields allowed');
      return;
    }

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
      defaultValue: field.defaultValue,
      options: field.options,
      validationPattern: field.validationPattern,
      validationMessage: field.validationMessage,
      minValue: field.minValue,
      maxValue: field.maxValue,
      appliesToCategoryId: field.appliesToCategoryId,
      appliesToSubcategoryId: field.appliesToSubcategoryId,
    });
    setSelectedField(field);
    setFieldDialogOpen(true);
  };

  const handleDeleteField = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this custom field?')) {
      try {
        const field = customFields.find((f) => f.id === id);
        await serviceRequestCustomFieldService.delete(id);
        setCustomFields(customFields.filter((f) => f.id !== id));
        if (field?.isActive) {
          setActiveFieldCount((prev) => prev - 1);
        }
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

      // Check active count when activating a field
      if (fieldForm.isActive && !selectedField?.isActive && activeFieldCount >= 15) {
        setError('Maximum of 15 active custom fields allowed');
        return;
      }

      if (selectedField) {
        const response = await serviceRequestCustomFieldService.update(selectedField.id!, fieldForm);
        setCustomFields(customFields.map((f) => (f.id === selectedField.id ? response.data : f)));
        
        // Update active count if status changed
        if (selectedField.isActive !== fieldForm.isActive) {
          setActiveFieldCount((prev) => fieldForm.isActive ? prev + 1 : prev - 1);
        }
        
        setSuccessMessage('Custom field updated successfully');
      } else {
        const response = await serviceRequestCustomFieldService.create(fieldForm);
        setCustomFields([...customFields, response.data]);
        if (fieldForm.isActive) {
          setActiveFieldCount((prev) => prev + 1);
        }
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
      setError(err.response?.data?.message || 'Failed to reorder custom fields');
    }
  };

  if (loading) {
    return (
      <Container maxWidth="lg">
        <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg">
      <Box sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
        <img src={logo} alt="CRM Logo" style={{ height: 40 }} />
        <Typography variant="h4" component="h1">
          Service Request Settings
        </Typography>
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

      <Card>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={tabValue} onChange={(_, v) => setTabValue(v)}>
            <Tab label="Categories" />
            <Tab label="Subcategories" />
            <Tab label="Custom Fields" />
          </Tabs>
        </Box>

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
                <TableCell>Default Priority</TableCell>
                <TableCell>SLA Response</TableCell>
                <TableCell>SLA Resolution</TableCell>
                <TableCell>Status</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {categories.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={8} align="center">
                    No categories found. Add one to get started.
                  </TableCell>
                </TableRow>
              ) : (
                categories.map((category, index) => (
                  <TableRow key={category.id}>
                    <TableCell>
                      <Stack direction="row" spacing={0.5}>
                        <IconButton
                          size="small"
                          onClick={() => handleMoveCategory(category, 'up')}
                          disabled={index === 0}
                        >
                          <ArrowUpwardIcon fontSize="small" />
                        </IconButton>
                        <IconButton
                          size="small"
                          onClick={() => handleMoveCategory(category, 'down')}
                          disabled={index === categories.length - 1}
                        >
                          <ArrowDownwardIcon fontSize="small" />
                        </IconButton>
                      </Stack>
                    </TableCell>
                    <TableCell>
                      <Typography fontWeight="medium">{category.name}</Typography>
                    </TableCell>
                    <TableCell>{category.description || '-'}</TableCell>
                    <TableCell>{PRIORITY_LABELS[category.defaultPriority]}</TableCell>
                    <TableCell>{category.slaResponseHours ? `${category.slaResponseHours}h` : '-'}</TableCell>
                    <TableCell>{category.slaResolutionHours ? `${category.slaResolutionHours}h` : '-'}</TableCell>
                    <TableCell>
                      <Chip
                        label={category.isActive ? 'Active' : 'Inactive'}
                        color={category.isActive ? 'success' : 'default'}
                        size="small"
                      />
                    </TableCell>
                    <TableCell align="right">
                      <Tooltip title="Edit">
                        <IconButton size="small" onClick={() => handleEditCategory(category)}>
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Delete">
                        <IconButton size="small" color="error" onClick={() => handleDeleteCategory(category.id!)}>
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
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={handleAddSubcategory}
              disabled={categories.length === 0}
            >
              Add Subcategory
            </Button>
          </Box>
          {categories.length === 0 ? (
            <Paper sx={{ p: 3, textAlign: 'center', bgcolor: 'grey.50' }}>
              <Typography color="text.secondary">
                Please create a category first before adding subcategories.
              </Typography>
            </Paper>
          ) : (
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Category</TableCell>
                  <TableCell>Name</TableCell>
                  <TableCell>Description</TableCell>
                  <TableCell>Default Workflow</TableCell>
                  <TableCell>Default Group</TableCell>
                  <TableCell>SLA Override</TableCell>
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
                        <Chip
                          label={sub.isActive ? 'Active' : 'Inactive'}
                          color={sub.isActive ? 'success' : 'default'}
                          size="small"
                        />
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
          )}
        </TabPanel>

        {/* Custom Fields Tab */}
        <TabPanel value={tabValue} index={2}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="body2" color="text.secondary">
              Active custom fields: {activeFieldCount} / 15
            </Typography>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={handleAddField}
              disabled={activeFieldCount >= 15}
            >
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
                <TableCell>Applies To</TableCell>
                <TableCell>Status</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {customFields.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={8} align="center">
                    No custom fields found. Add one to get started.
                  </TableCell>
                </TableRow>
              ) : (
                customFields.map((field, index) => (
                  <TableRow key={field.id}>
                    <TableCell>
                      <Stack direction="row" spacing={0.5}>
                        <IconButton
                          size="small"
                          onClick={() => handleMoveField(field, 'up')}
                          disabled={index === 0}
                        >
                          <ArrowUpwardIcon fontSize="small" />
                        </IconButton>
                        <IconButton
                          size="small"
                          onClick={() => handleMoveField(field, 'down')}
                          disabled={index === customFields.length - 1}
                        >
                          <ArrowDownwardIcon fontSize="small" />
                        </IconButton>
                      </Stack>
                    </TableCell>
                    <TableCell>
                      <Typography fontWeight="medium">{field.name}</Typography>
                    </TableCell>
                    <TableCell>{field.label}</TableCell>
                    <TableCell>{FIELD_TYPE_LABELS[field.fieldType]}</TableCell>
                    <TableCell>
                      <Chip
                        label={field.isRequired ? 'Yes' : 'No'}
                        color={field.isRequired ? 'warning' : 'default'}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>
                      {field.appliesToCategoryName
                        ? field.appliesToSubcategoryName
                          ? `${field.appliesToCategoryName} > ${field.appliesToSubcategoryName}`
                          : field.appliesToCategoryName
                        : 'All'}
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={field.isActive ? 'Active' : 'Inactive'}
                        color={field.isActive ? 'success' : 'default'}
                        size="small"
                      />
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
      </Card>

      {/* Category Dialog */}
      <Dialog open={categoryDialogOpen} onClose={() => setCategoryDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{selectedCategory ? 'Edit Category' : 'Add Category'}</DialogTitle>
        <DialogContent dividers>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <TextField
                label="Name"
                value={categoryForm.name}
                onChange={(e) => setCategoryForm({ ...categoryForm, name: e.target.value })}
                fullWidth
                required
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                label="Description"
                value={categoryForm.description || ''}
                onChange={(e) => setCategoryForm({ ...categoryForm, description: e.target.value })}
                multiline
                rows={2}
                fullWidth
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Icon"
                value={categoryForm.icon || ''}
                onChange={(e) => setCategoryForm({ ...categoryForm, icon: e.target.value })}
                fullWidth
                helperText="Material icon name"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Color"
                value={categoryForm.color || ''}
                onChange={(e) => setCategoryForm({ ...categoryForm, color: e.target.value })}
                fullWidth
                helperText="Hex color code"
              />
            </Grid>
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>Default Priority</InputLabel>
                <Select
                  value={categoryForm.defaultPriority}
                  onChange={(e: SelectChangeEvent<number>) =>
                    setCategoryForm({ ...categoryForm, defaultPriority: Number(e.target.value) })
                  }
                  label="Default Priority"
                >
                  {Object.entries(PRIORITY_LABELS).map(([key, label]) => (
                    <MenuItem key={key} value={parseInt(key)}>
                      {label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="SLA Response (Hours)"
                type="number"
                value={categoryForm.slaResponseHours || ''}
                onChange={(e) =>
                  setCategoryForm({ ...categoryForm, slaResponseHours: e.target.value ? parseInt(e.target.value) : undefined })
                }
                fullWidth
                inputProps={{ min: 0 }}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="SLA Resolution (Hours)"
                type="number"
                value={categoryForm.slaResolutionHours || ''}
                onChange={(e) =>
                  setCategoryForm({ ...categoryForm, slaResolutionHours: e.target.value ? parseInt(e.target.value) : undefined })
                }
                fullWidth
                inputProps={{ min: 0 }}
              />
            </Grid>
            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Switch
                    checked={categoryForm.isActive}
                    onChange={(e) => setCategoryForm({ ...categoryForm, isActive: e.target.checked })}
                  />
                }
                label="Active"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCategoryDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSaveCategory}>
            {selectedCategory ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Subcategory Dialog */}
      <Dialog open={subcategoryDialogOpen} onClose={() => setSubcategoryDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{selectedSubcategory ? 'Edit Subcategory' : 'Add Subcategory'}</DialogTitle>
        <DialogContent dividers>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <FormControl fullWidth required>
                <InputLabel>Category</InputLabel>
                <Select
                  value={subcategoryForm.categoryId}
                  onChange={(e: SelectChangeEvent<number>) =>
                    setSubcategoryForm({ ...subcategoryForm, categoryId: Number(e.target.value) })
                  }
                  label="Category"
                >
                  {categories.map((cat) => (
                    <MenuItem key={cat.id} value={cat.id}>
                      {cat.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <TextField
                label="Name"
                value={subcategoryForm.name}
                onChange={(e) => setSubcategoryForm({ ...subcategoryForm, name: e.target.value })}
                fullWidth
                required
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                label="Description"
                value={subcategoryForm.description || ''}
                onChange={(e) => setSubcategoryForm({ ...subcategoryForm, description: e.target.value })}
                multiline
                rows={2}
                fullWidth
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Default Workflow</InputLabel>
                <Select
                  value={subcategoryForm.defaultWorkflowId || ''}
                  onChange={(e: SelectChangeEvent<string | number>) =>
                    setSubcategoryForm({
                      ...subcategoryForm,
                      defaultWorkflowId: e.target.value ? Number(e.target.value) : undefined,
                    })
                  }
                  label="Default Workflow"
                >
                  <MenuItem value="">None</MenuItem>
                  {workflows.map((wf) => (
                    <MenuItem key={wf.id} value={wf.id}>
                      {wf.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Default Assignee Group</InputLabel>
                <Select
                  value={subcategoryForm.defaultAssigneeGroupId || ''}
                  onChange={(e: SelectChangeEvent<string | number>) =>
                    setSubcategoryForm({
                      ...subcategoryForm,
                      defaultAssigneeGroupId: e.target.value ? Number(e.target.value) : undefined,
                    })
                  }
                  label="Default Assignee Group"
                >
                  <MenuItem value="">None</MenuItem>
                  {groups.map((group) => (
                    <MenuItem key={group.id} value={group.id}>
                      {group.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="SLA Response Override (Hours)"
                type="number"
                value={subcategoryForm.slaResponseHours || ''}
                onChange={(e) =>
                  setSubcategoryForm({
                    ...subcategoryForm,
                    slaResponseHours: e.target.value ? parseInt(e.target.value) : undefined,
                  })
                }
                fullWidth
                inputProps={{ min: 0 }}
                helperText="Overrides category SLA"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="SLA Resolution Override (Hours)"
                type="number"
                value={subcategoryForm.slaResolutionHours || ''}
                onChange={(e) =>
                  setSubcategoryForm({
                    ...subcategoryForm,
                    slaResolutionHours: e.target.value ? parseInt(e.target.value) : undefined,
                  })
                }
                fullWidth
                inputProps={{ min: 0 }}
                helperText="Overrides category SLA"
              />
            </Grid>
            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Switch
                    checked={subcategoryForm.isActive}
                    onChange={(e) => setSubcategoryForm({ ...subcategoryForm, isActive: e.target.checked })}
                  />
                }
                label="Active"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSubcategoryDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSaveSubcategory}>
            {selectedSubcategory ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Custom Field Dialog */}
      <Dialog open={fieldDialogOpen} onClose={() => setFieldDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>{selectedField ? 'Edit Custom Field' : 'Add Custom Field'}</DialogTitle>
        <DialogContent dividers>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Field Name"
                value={fieldForm.name}
                onChange={(e) => setFieldForm({ ...fieldForm, name: e.target.value })}
                fullWidth
                required
                helperText="Internal name (no spaces)"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Display Label"
                value={fieldForm.label}
                onChange={(e) => setFieldForm({ ...fieldForm, label: e.target.value })}
                fullWidth
                required
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                label="Description"
                value={fieldForm.description || ''}
                onChange={(e) => setFieldForm({ ...fieldForm, description: e.target.value })}
                multiline
                rows={2}
                fullWidth
                helperText="Help text shown to users"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth required>
                <InputLabel>Field Type</InputLabel>
                <Select
                  value={fieldForm.fieldType}
                  onChange={(e: SelectChangeEvent<number>) =>
                    setFieldForm({ ...fieldForm, fieldType: Number(e.target.value) })
                  }
                  label="Field Type"
                >
                  {Object.entries(FIELD_TYPE_LABELS).map(([key, label]) => (
                    <MenuItem key={key} value={parseInt(key)}>
                      {label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Default Value"
                value={fieldForm.defaultValue || ''}
                onChange={(e) => setFieldForm({ ...fieldForm, defaultValue: e.target.value })}
                fullWidth
              />
            </Grid>
            
            {(fieldForm.fieldType === CustomFieldType.Dropdown ||
              fieldForm.fieldType === CustomFieldType.MultiSelect) && (
              <Grid item xs={12}>
                <TextField
                  label="Options"
                  value={fieldForm.options || ''}
                  onChange={(e) => setFieldForm({ ...fieldForm, options: e.target.value })}
                  fullWidth
                  required
                  helperText="Comma-separated list of options"
                />
              </Grid>
            )}

            {(fieldForm.fieldType === CustomFieldType.Number ||
              fieldForm.fieldType === CustomFieldType.Decimal) && (
              <>
                <Grid item xs={12} sm={6}>
                  <TextField
                    label="Min Value"
                    type="number"
                    value={fieldForm.minValue ?? ''}
                    onChange={(e) =>
                      setFieldForm({ ...fieldForm, minValue: e.target.value ? parseFloat(e.target.value) : undefined })
                    }
                    fullWidth
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    label="Max Value"
                    type="number"
                    value={fieldForm.maxValue ?? ''}
                    onChange={(e) =>
                      setFieldForm({ ...fieldForm, maxValue: e.target.value ? parseFloat(e.target.value) : undefined })
                    }
                    fullWidth
                  />
                </Grid>
              </>
            )}

            {fieldForm.fieldType === CustomFieldType.Text && (
              <>
                <Grid item xs={12} sm={6}>
                  <TextField
                    label="Validation Pattern"
                    value={fieldForm.validationPattern || ''}
                    onChange={(e) => setFieldForm({ ...fieldForm, validationPattern: e.target.value })}
                    fullWidth
                    helperText="Regular expression"
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    label="Validation Message"
                    value={fieldForm.validationMessage || ''}
                    onChange={(e) => setFieldForm({ ...fieldForm, validationMessage: e.target.value })}
                    fullWidth
                    helperText="Error message for invalid input"
                  />
                </Grid>
              </>
            )}

            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Applies to Category</InputLabel>
                <Select
                  value={fieldForm.appliesToCategoryId || ''}
                  onChange={(e: SelectChangeEvent<string | number>) => {
                    setFieldForm({
                      ...fieldForm,
                      appliesToCategoryId: e.target.value ? Number(e.target.value) : undefined,
                      appliesToSubcategoryId: undefined,
                    });
                  }}
                  label="Applies to Category"
                >
                  <MenuItem value="">All Categories</MenuItem>
                  {categories.map((cat) => (
                    <MenuItem key={cat.id} value={cat.id}>
                      {cat.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth disabled={!fieldForm.appliesToCategoryId}>
                <InputLabel>Applies to Subcategory</InputLabel>
                <Select
                  value={fieldForm.appliesToSubcategoryId || ''}
                  onChange={(e: SelectChangeEvent<string | number>) =>
                    setFieldForm({
                      ...fieldForm,
                      appliesToSubcategoryId: e.target.value ? Number(e.target.value) : undefined,
                    })
                  }
                  label="Applies to Subcategory"
                >
                  <MenuItem value="">All Subcategories</MenuItem>
                  {subcategories
                    .filter((s) => s.categoryId === fieldForm.appliesToCategoryId)
                    .map((sub) => (
                      <MenuItem key={sub.id} value={sub.id}>
                        {sub.name}
                      </MenuItem>
                    ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={fieldForm.isRequired}
                    onChange={(e) => setFieldForm({ ...fieldForm, isRequired: e.target.checked })}
                  />
                }
                label="Required"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={fieldForm.isActive}
                    onChange={(e) => setFieldForm({ ...fieldForm, isActive: e.target.checked })}
                  />
                }
                label="Active"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setFieldDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSaveField}>
            {selectedField ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

export default ServiceRequestSettingsPage;

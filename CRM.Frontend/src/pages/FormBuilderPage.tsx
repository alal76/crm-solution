/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 * Licensed under the Source-Available License (see LICENSE) v3.0
 */

import { useState, useEffect } from 'react';
import {
  Box, Container, Typography, Card, CardContent, Table, TableBody, TableCell,
  TableHead, TableRow, Button, Dialog, DialogTitle, DialogContent, DialogActions,
  TextField, MenuItem, Stack, Chip, IconButton, Tooltip, CircularProgress,
  Alert, Grid, Tabs, Tab, FormControl, InputLabel, Select, FormControlLabel,
  Checkbox, Divider, Paper, SelectChangeEvent, Collapse,
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon,
  Visibility as PreviewIcon, ContentCopy as CopyIcon,
  Code as EmbedIcon, Close as CloseIcon, Refresh as RefreshIcon,
  DragIndicator as DragIcon, ArrowUpward, ArrowDownward,
} from '@mui/icons-material';
import { DialogError, ActionButton, TabPanel } from '../components/common';
import { useApiState } from '../hooks/useApiState';
import { useProfile } from '../contexts/ProfileContext';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';

// ==================== TYPES ====================

enum FormFieldType {
  Text = 0,
  TextArea = 1,
  Email = 2,
  Phone = 3,
  Number = 4,
  Date = 5,
  DateTime = 6,
  Dropdown = 7,
  MultiSelect = 8,
  Radio = 9,
  Checkbox = 10,
  FileUpload = 11,
  Hidden = 12,
  Country = 13,
  State = 14,
  Url = 15,
  Rating = 16,
  Range = 17,
  Consent = 18,
  Captcha = 19,
  Heading = 20,
  Paragraph = 21,
  Divider = 22,
}

enum FormStatus {
  Draft = 0,
  Published = 1,
  Paused = 2,
  Archived = 3,
}

enum FormSubmitAction {
  ShowMessage = 0,
  Redirect = 1,
  ShowForm = 2,
  StayOnPage = 3,
}

enum SubmissionStatus {
  New = 0,
  Processing = 1,
  LeadCreated = 2,
  ContactCreated = 3,
  SubmittedExternal = 4,
  Failed = 5,
  Spam = 6,
  Duplicate = 7,
}

interface FormField {
  id?: number;
  fieldName: string;
  label: string;
  fieldType: FormFieldType;
  order: number;
  isRequired: boolean;
  requiredMessage?: string;
  minLength?: number;
  maxLength?: number;
  minValue?: number;
  maxValue?: number;
  validationPattern?: string;
  validationMessage?: string;
  placeholder?: string;
  helpText?: string;
  defaultValue?: string;
  width?: string;
  cssClasses?: string;
  isHidden?: boolean;
  isReadOnly?: boolean;
  options?: string;
  optionValueField?: string;
  optionLabelField?: string;
  allowOther?: boolean;
  crmFieldMapping?: string;
  crmEntityMapping?: string;
  hasConditionalLogic?: boolean;
  conditionalLogic?: string;
  formDefinitionId?: number;
}

interface FormDefinition {
  id: number;
  name: string;
  formKey: string;
  description?: string;
  status: FormStatus;
  title?: string;
  subtitle?: string;
  submitButtonText: string;
  width?: string;
  cssClasses?: string;
  customCss?: string;
  customJs?: string;
  theme?: string;
  submitAction: FormSubmitAction;
  thankYouMessage?: string;
  redirectUrl?: string;
  doubleOptIn: boolean;
  doubleOptInTemplateId?: number;
  spamProtection: boolean;
  captchaType?: string;
  honeypotFieldName?: string;
  createLead: boolean;
  leadSource?: string;
  defaultLeadOwnerId?: number;
  leadRoutingRuleId?: number;
  updateExistingLead: boolean;
  existingLeadMatchField?: string;
  campaignId?: number;
  campaignMemberStatus?: string;
  notifyOwner: boolean;
  notificationRecipients?: string;
  notificationTemplateId?: number;
  sendAutoresponder: boolean;
  autoresponderTemplateId?: number;
  embedCode?: string;
  directUrl?: string;
  allowedDomains?: string;
  totalViews: number;
  totalSubmissions: number;
  conversionRate: number;
  ownerId?: number;
  fields: FormField[];
  createdAt?: string;
  updatedAt?: string;
}

interface FormSubmission {
  id: number;
  submissionNumber: string;
  submittedAt: string;
  status: SubmissionStatus;
  errorMessage?: string;
  formData: string;
  ipAddress?: string;
  userAgent?: string;
  referrer?: string;
  pageUrl?: string;
  utmSource?: string;
  utmMedium?: string;
  utmCampaign?: string;
  processedAt?: string;
  optInConfirmed: boolean;
  isSpam: boolean;
  spamScore?: number;
  formDefinitionId: number;
  leadId?: number;
  contactId?: number;
}

// ==================== CONSTANTS ====================

const FIELD_TYPE_OPTIONS = [
  { value: FormFieldType.Text, label: 'Text' },
  { value: FormFieldType.TextArea, label: 'Text Area' },
  { value: FormFieldType.Email, label: 'Email' },
  { value: FormFieldType.Phone, label: 'Phone' },
  { value: FormFieldType.Number, label: 'Number' },
  { value: FormFieldType.Date, label: 'Date' },
  { value: FormFieldType.DateTime, label: 'Date & Time' },
  { value: FormFieldType.Dropdown, label: 'Dropdown' },
  { value: FormFieldType.MultiSelect, label: 'Multi-Select' },
  { value: FormFieldType.Radio, label: 'Radio Buttons' },
  { value: FormFieldType.Checkbox, label: 'Checkbox' },
  { value: FormFieldType.FileUpload, label: 'File Upload' },
  { value: FormFieldType.Hidden, label: 'Hidden' },
  { value: FormFieldType.Country, label: 'Country' },
  { value: FormFieldType.State, label: 'State/Region' },
  { value: FormFieldType.Url, label: 'URL' },
  { value: FormFieldType.Rating, label: 'Rating' },
  { value: FormFieldType.Range, label: 'Range Slider' },
  { value: FormFieldType.Consent, label: 'Consent' },
  { value: FormFieldType.Captcha, label: 'CAPTCHA' },
  { value: FormFieldType.Heading, label: 'Heading' },
  { value: FormFieldType.Paragraph, label: 'Paragraph' },
  { value: FormFieldType.Divider, label: 'Divider' },
];

type ChipColor = 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning';

const FORM_STATUS_OPTIONS: Array<{ value: FormStatus; label: string; color: ChipColor }> = [
  { value: FormStatus.Draft, label: 'Draft', color: 'default' },
  { value: FormStatus.Published, label: 'Published', color: 'success' },
  { value: FormStatus.Paused, label: 'Paused', color: 'warning' },
  { value: FormStatus.Archived, label: 'Archived', color: 'error' },
];

const SUBMIT_ACTION_OPTIONS = [
  { value: FormSubmitAction.ShowMessage, label: 'Show Thank You Message' },
  { value: FormSubmitAction.Redirect, label: 'Redirect to URL' },
  { value: FormSubmitAction.ShowForm, label: 'Show Another Form' },
  { value: FormSubmitAction.StayOnPage, label: 'Stay on Page' },
];

const SUBMISSION_STATUS_OPTIONS: Array<{ value: SubmissionStatus; label: string; color: ChipColor }> = [
  { value: SubmissionStatus.New, label: 'New', color: 'info' },
  { value: SubmissionStatus.Processing, label: 'Processing', color: 'warning' },
  { value: SubmissionStatus.LeadCreated, label: 'Lead Created', color: 'success' },
  { value: SubmissionStatus.ContactCreated, label: 'Contact Created', color: 'success' },
  { value: SubmissionStatus.SubmittedExternal, label: 'Submitted External', color: 'primary' },
  { value: SubmissionStatus.Failed, label: 'Failed', color: 'error' },
  { value: SubmissionStatus.Spam, label: 'Spam', color: 'error' },
  { value: SubmissionStatus.Duplicate, label: 'Duplicate', color: 'warning' },
];

const CRM_FIELD_MAPPINGS = [
  { entity: 'Lead', fields: ['FirstName', 'LastName', 'Email', 'Phone', 'Company', 'Title', 'Website', 'Description'] },
  { entity: 'Contact', fields: ['FirstName', 'LastName', 'EmailPrimary', 'PhonePrimary', 'Title', 'Department'] },
];

const WIDTH_OPTIONS = [
  { value: 'full', label: 'Full Width' },
  { value: 'half', label: 'Half Width' },
  { value: 'third', label: 'One Third' },
  { value: 'two-thirds', label: 'Two Thirds' },
];

// ==================== HELPER FUNCTIONS ====================

const getStatusInfo = (status: FormStatus): { label: string; color: ChipColor } => 
  FORM_STATUS_OPTIONS.find(s => s.value === status) || { label: 'Unknown', color: 'default' };

const getSubmissionStatusInfo = (status: SubmissionStatus): { label: string; color: ChipColor } =>
  SUBMISSION_STATUS_OPTIONS.find(s => s.value === status) || { label: 'Unknown', color: 'default' };

const getFieldTypeLabel = (type: FormFieldType) =>
  FIELD_TYPE_OPTIONS.find(f => f.value === type)?.label || 'Unknown';

const generateFormKey = (name: string): string => {
  return name.toLowerCase().replaceAll(/[^a-z0-9]+/g, '-').replaceAll(/^-|-$/g, '');
};

// ==================== MAIN COMPONENT ====================

function FormBuilderPage() {
  // State
  const [forms, setForms] = useState<FormDefinition[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingForm, setEditingForm] = useState<FormDefinition | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  const [selectedFormId, setSelectedFormId] = useState<number | null>(null);
  const [submissions, setSubmissions] = useState<FormSubmission[]>([]);
  const [submissionsDialogOpen, setSubmissionsDialogOpen] = useState(false);
  const [embedDialogOpen, setEmbedDialogOpen] = useState(false);
  const [embedCode, setEmbedCode] = useState('');

  // Form state
  const [formData, setFormData] = useState<Partial<FormDefinition>>({
    name: '',
    formKey: '',
    description: '',
    status: FormStatus.Draft,
    title: '',
    subtitle: '',
    submitButtonText: 'Submit',
    submitAction: FormSubmitAction.ShowMessage,
    thankYouMessage: 'Thank you for your submission!',
    spamProtection: true,
    createLead: true,
    updateExistingLead: true,
    existingLeadMatchField: 'Email',
    notifyOwner: true,
    fields: [],
  });

  // Field editor state
  const [editingField, setEditingField] = useState<FormField | null>(null);
  const [fieldDialogOpen, setFieldDialogOpen] = useState(false);

  const dialogApi = useApiState();
  const { hasPermission } = useProfile();

  // ==================== DATA FETCHING ====================

  useEffect(() => {
    fetchForms();
  }, []);

  const fetchForms = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/forms');
      setForms(response.data);
      setError(null);
    } catch (err: unknown) {
      // If the endpoint doesn't exist yet, show empty state
      if ((err as any).response?.status === 404) {
        setForms([]);
        setError(null);
      } else {
        setError((err as any).response?.data?.message || 'Failed to fetch forms');
      }
    } finally {
      setLoading(false);
    }
  };

  const fetchSubmissions = async (formId: number) => {
    try {
      const response = await apiClient.get(`/forms/${formId}/submissions`);
      setSubmissions(response.data);
    } catch (err: unknown) {
      setSubmissions([]);
    }
  };

  // ==================== DIALOG HANDLERS ====================

  const handleOpenDialog = (form?: FormDefinition) => {
    setDialogTab(0);
    if (form) {
      setEditingForm(form);
      setFormData({ ...form });
    } else {
      setEditingForm(null);
      setFormData({
        name: '',
        formKey: '',
        description: '',
        status: FormStatus.Draft,
        title: '',
        subtitle: '',
        submitButtonText: 'Submit',
        submitAction: FormSubmitAction.ShowMessage,
        thankYouMessage: 'Thank you for your submission!',
        spamProtection: true,
        createLead: true,
        updateExistingLead: true,
        existingLeadMatchField: 'Email',
        notifyOwner: true,
        fields: [],
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingForm(null);
    dialogApi.clearError();
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    const checked = (e.target as HTMLInputElement).checked;
    
    let newValue: any = type === 'checkbox' ? checked : value;
    
    // Auto-generate formKey from name
    if (name === 'name' && !editingForm) {
      setFormData(prev => ({
        ...prev,
        [name]: newValue,
        formKey: generateFormKey(value),
      }));
      return;
    }
    
    setFormData(prev => ({ ...prev, [name]: newValue }));
  };

  const handleSelectChange = (e: SelectChangeEvent<number | string>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name as string]: value }));
  };

  // ==================== SAVE OPERATIONS ====================

  const handleSaveForm = async () => {
    if (!formData.name?.trim()) {
      dialogApi.setError('Form name is required');
      return;
    }

    await dialogApi.execute(async () => {
      if (editingForm) {
        await apiClient.put(`/forms/${editingForm.id}`, formData);
        setSuccessMessage('Form updated successfully');
      } else {
        await apiClient.post('/forms', formData);
        setSuccessMessage('Form created successfully');
      }
      handleCloseDialog();
      fetchForms();
      setTimeout(() => setSuccessMessage(null), 3000);
    });
  };

  const handleDeleteForm = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this form? All submissions will be lost.')) {
      try {
        await apiClient.delete(`/forms/${id}`);
        setSuccessMessage('Form deleted successfully');
        fetchForms();
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: unknown) {
        setError((err as any).response?.data?.message || 'Failed to delete form');
      }
    }
  };

  const handleDuplicateForm = async (form: FormDefinition) => {
    try {
      const newForm = {
        ...form,
        id: undefined,
        name: `${form.name} (Copy)`,
        formKey: `${form.formKey}-copy`,
        status: FormStatus.Draft,
        totalViews: 0,
        totalSubmissions: 0,
      };
      await apiClient.post('/forms', newForm);
      setSuccessMessage('Form duplicated successfully');
      fetchForms();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: unknown) {
      setError((err as any).response?.data?.message || 'Failed to duplicate form');
    }
  };

  // ==================== FIELD MANAGEMENT ====================

  const handleAddField = () => {
    const newField: FormField = {
      fieldName: '',
      label: '',
      fieldType: FormFieldType.Text,
      order: (formData.fields?.length || 0) + 1,
      isRequired: false,
      width: 'full',
    };
    setEditingField(newField);
    setFieldDialogOpen(true);
  };

  const handleEditField = (field: FormField, index: number) => {
    setEditingField({ ...field, order: index });
    setFieldDialogOpen(true);
  };

  const handleSaveField = () => {
    if (!editingField?.fieldName || !editingField?.label) {
      return;
    }

    const fields = [...(formData.fields || [])];
    const existingIndex = fields.findIndex(f => f.order === editingField.order && f.id === editingField.id);
    
    if (existingIndex >= 0) {
      fields[existingIndex] = editingField;
    } else {
      fields.push(editingField);
    }

    // Re-order fields
    fields.forEach((f, i) => f.order = i + 1);
    
    setFormData(prev => ({ ...prev, fields }));
    setFieldDialogOpen(false);
    setEditingField(null);
  };

  const handleDeleteField = (index: number) => {
    const fields = [...(formData.fields || [])];
    fields.splice(index, 1);
    fields.forEach((f, i) => f.order = i + 1);
    setFormData(prev => ({ ...prev, fields }));
  };

  const handleMoveField = (index: number, direction: 'up' | 'down') => {
    const fields = [...(formData.fields || [])];
    const newIndex = direction === 'up' ? index - 1 : index + 1;
    if (newIndex < 0 || newIndex >= fields.length) return;
    
    [fields[index], fields[newIndex]] = [fields[newIndex], fields[index]];
    fields.forEach((f, i) => f.order = i + 1);
    setFormData(prev => ({ ...prev, fields }));
  };

  // ==================== EMBED CODE ====================

  const handleShowEmbedCode = (form: FormDefinition) => {
    const baseUrl = window.location.origin;
    const code = `<!-- CRM Form Embed Code -->
<div id="crm-form-${form.formKey}"></div>
<script src="${baseUrl}/api/forms/${form.id}/embed.js"></script>
<script>
  CRMForm.init({
    formId: ${form.id},
    container: '#crm-form-${form.formKey}'
  });
</script>

<!-- Alternative: Direct iFrame -->
<iframe 
  src="${baseUrl}/forms/${form.formKey}" 
  width="100%" 
  height="600" 
  frameborder="0"
  style="border: none;">
</iframe>`;
    setEmbedCode(code);
    setEmbedDialogOpen(true);
  };

  const handleCopyEmbedCode = () => {
    navigator.clipboard.writeText(embedCode);
    setSuccessMessage('Embed code copied to clipboard');
    setTimeout(() => setSuccessMessage(null), 3000);
  };

  // ==================== SUBMISSIONS ====================

  const handleViewSubmissions = (formId: number) => {
    setSelectedFormId(formId);
    fetchSubmissions(formId);
    setSubmissionsDialogOpen(true);
  };

  // ==================== RENDER ====================

  if (loading) {
    return (
      <Container maxWidth="lg">
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="50vh">
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg">
      <Box mb={4}>
        {/* Header */}
        <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
          <Box display="flex" alignItems="center" gap={2}>
            <img src={logo} alt="CRM Logo" style={{ height: 40, borderRadius: 8 }} />
            <Typography variant="h4">Form Builder</Typography>
          </Box>
          <Stack direction="row" spacing={2}>
            <Button variant="outlined" startIcon={<RefreshIcon />} onClick={fetchForms}>
              Refresh
            </Button>
            <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenDialog()}>
              New Form
            </Button>
          </Stack>
        </Box>

        {/* Alerts */}
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

        {/* Forms Table */}
        <Card>
          <CardContent>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Form Key</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="center">Fields</TableCell>
                  <TableCell align="center">Views</TableCell>
                  <TableCell align="center">Submissions</TableCell>
                  <TableCell align="center">Conversion</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {forms.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={8} align="center">
                      <Typography color="text.secondary" py={4}>
                        No forms found. Create your first form to start capturing leads!
                      </Typography>
                    </TableCell>
                  </TableRow>
                ) : (
                  forms.map((form) => {
                    const statusInfo = getStatusInfo(form.status);
                    return (
                      <TableRow key={form.id} hover>
                        <TableCell>
                          <Typography fontWeight="medium">{form.name}</Typography>
                          {form.description && (
                            <Typography variant="caption" color="text.secondary">
                              {form.description}
                            </Typography>
                          )}
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2" fontFamily="monospace">
                            {form.formKey}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Chip 
                            label={statusInfo.label} 
                            size="small" 
                            color={statusInfo.color}
                          />
                        </TableCell>
                        <TableCell align="center">{form.fields?.length || 0}</TableCell>
                        <TableCell align="center">{form.totalViews}</TableCell>
                        <TableCell align="center">{form.totalSubmissions}</TableCell>
                        <TableCell align="center">
                          {form.conversionRate.toFixed(1)}%
                        </TableCell>
                        <TableCell align="right">
                          <Tooltip title="Edit">
                            <IconButton size="small" onClick={() => handleOpenDialog(form)}>
                              <EditIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="View Submissions">
                            <IconButton size="small" onClick={() => handleViewSubmissions(form.id)}>
                              <PreviewIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Get Embed Code">
                            <IconButton size="small" onClick={() => handleShowEmbedCode(form)}>
                              <EmbedIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Duplicate">
                            <IconButton size="small" onClick={() => handleDuplicateForm(form)}>
                              <CopyIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete">
                            <IconButton size="small" color="error" onClick={() => handleDeleteForm(form.id)}>
                              <DeleteIcon />
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    );
                  })
                )}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </Box>

      {/* Form Editor Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="lg" fullWidth>
        <DialogTitle>
          <Box display="flex" justifyContent="space-between" alignItems="center">
            {editingForm ? 'Edit Form' : 'Create New Form'}
            <IconButton onClick={handleCloseDialog}><CloseIcon /></IconButton>
          </Box>
        </DialogTitle>
        <DialogContent dividers>
          <Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)} sx={{ mb: 2 }}>
            <Tab label="Basic Info" />
            <Tab label="Fields" />
            <Tab label="Submission Settings" />
            <Tab label="Lead Settings" />
            <Tab label="Notifications" />
            <Tab label="Styling" />
          </Tabs>

          <DialogError error={dialogApi.error} />

          {/* Tab 0: Basic Info */}
          <TabPanel value={dialogTab} index={0}>
            <Grid container spacing={3}>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  required
                  label="Form Name"
                  name="name"
                  value={formData.name || ''}
                  onChange={handleInputChange}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Form Key"
                  name="formKey"
                  value={formData.formKey || ''}
                  onChange={handleInputChange}
                  helperText="URL-friendly identifier"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  multiline
                  rows={2}
                  label="Description"
                  name="description"
                  value={formData.description || ''}
                  onChange={handleInputChange}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth>
                  <InputLabel>Status</InputLabel>
                  <Select
                    name="status"
                    value={formData.status ?? FormStatus.Draft}
                    onChange={handleSelectChange}
                    label="Status"
                  >
                    {FORM_STATUS_OPTIONS.map(opt => (
                      <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Display Title"
                  name="title"
                  value={formData.title || ''}
                  onChange={handleInputChange}
                  helperText="Title shown on the form"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Subtitle"
                  name="subtitle"
                  value={formData.subtitle || ''}
                  onChange={handleInputChange}
                />
              </Grid>
            </Grid>
          </TabPanel>

          {/* Tab 1: Fields */}
          <TabPanel value={dialogTab} index={1}>
            <Box mb={2}>
              <Button variant="contained" startIcon={<AddIcon />} onClick={handleAddField}>
                Add Field
              </Button>
            </Box>
            
            {(formData.fields?.length || 0) === 0 ? (
              <Paper sx={{ p: 4, textAlign: 'center', bgcolor: 'grey.50' }}>
                <Typography color="text.secondary">
                  No fields added yet. Click "Add Field" to start building your form.
                </Typography>
              </Paper>
            ) : (
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell width={40}></TableCell>
                    <TableCell>Label</TableCell>
                    <TableCell>Field Name</TableCell>
                    <TableCell>Type</TableCell>
                    <TableCell>Required</TableCell>
                    <TableCell>Width</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {formData.fields?.map((field, index) => (
                    <TableRow key={index}>
                      <TableCell>
                        <Stack direction="column" spacing={0}>
                          <IconButton 
                            size="small" 
                            disabled={index === 0}
                            onClick={() => handleMoveField(index, 'up')}
                          >
                            <ArrowUpward fontSize="small" />
                          </IconButton>
                          <IconButton 
                            size="small" 
                            disabled={index === (formData.fields?.length || 0) - 1}
                            onClick={() => handleMoveField(index, 'down')}
                          >
                            <ArrowDownward fontSize="small" />
                          </IconButton>
                        </Stack>
                      </TableCell>
                      <TableCell>{field.label}</TableCell>
                      <TableCell><code>{field.fieldName}</code></TableCell>
                      <TableCell>{getFieldTypeLabel(field.fieldType)}</TableCell>
                      <TableCell>
                        {field.isRequired && <Chip label="Required" size="small" color="error" />}
                      </TableCell>
                      <TableCell>{field.width || 'full'}</TableCell>
                      <TableCell align="right">
                        <IconButton size="small" onClick={() => handleEditField(field, index)}>
                          <EditIcon />
                        </IconButton>
                        <IconButton size="small" color="error" onClick={() => handleDeleteField(index)}>
                          <DeleteIcon />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </TabPanel>

          {/* Tab 2: Submission Settings */}
          <TabPanel value={dialogTab} index={2}>
            <Grid container spacing={3}>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Submit Button Text"
                  name="submitButtonText"
                  value={formData.submitButtonText || 'Submit'}
                  onChange={handleInputChange}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth>
                  <InputLabel>After Submission</InputLabel>
                  <Select
                    name="submitAction"
                    value={formData.submitAction ?? FormSubmitAction.ShowMessage}
                    onChange={handleSelectChange}
                    label="After Submission"
                  >
                    {SUBMIT_ACTION_OPTIONS.map(opt => (
                      <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              {formData.submitAction === FormSubmitAction.ShowMessage && (
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    multiline
                    rows={3}
                    label="Thank You Message"
                    name="thankYouMessage"
                    value={formData.thankYouMessage || ''}
                    onChange={handleInputChange}
                  />
                </Grid>
              )}
              {formData.submitAction === FormSubmitAction.Redirect && (
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Redirect URL"
                    name="redirectUrl"
                    value={formData.redirectUrl || ''}
                    onChange={handleInputChange}
                    placeholder="https://example.com/thank-you"
                  />
                </Grid>
              )}
              <Grid item xs={12}>
                <Divider sx={{ my: 1 }} />
                <Typography variant="subtitle2" gutterBottom>Spam Protection</Typography>
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControlLabel
                  control={
                    <Checkbox 
                      checked={formData.spamProtection ?? true}
                      onChange={handleInputChange}
                      name="spamProtection"
                    />
                  }
                  label="Enable Spam Protection"
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth size="small">
                  <InputLabel>CAPTCHA Type</InputLabel>
                  <Select
                    name="captchaType"
                    value={formData.captchaType || 'none'}
                    onChange={handleSelectChange}
                    label="CAPTCHA Type"
                  >
                    <MenuItem value="none">None</MenuItem>
                    <MenuItem value="recaptcha">reCAPTCHA v2</MenuItem>
                    <MenuItem value="recaptcha-v3">reCAPTCHA v3</MenuItem>
                    <MenuItem value="hcaptcha">hCaptcha</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12}>
                <FormControlLabel
                  control={
                    <Checkbox 
                      checked={formData.doubleOptIn ?? false}
                      onChange={handleInputChange}
                      name="doubleOptIn"
                    />
                  }
                  label="Enable Double Opt-In (email confirmation required)"
                />
              </Grid>
            </Grid>
          </TabPanel>

          {/* Tab 3: Lead Settings */}
          <TabPanel value={dialogTab} index={3}>
            <Grid container spacing={3}>
              <Grid item xs={12}>
                <FormControlLabel
                  control={
                    <Checkbox 
                      checked={formData.createLead ?? true}
                      onChange={handleInputChange}
                      name="createLead"
                    />
                  }
                  label="Create Lead on Submission"
                />
              </Grid>
              {formData.createLead && (
                <>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Lead Source"
                      name="leadSource"
                      value={formData.leadSource || ''}
                      onChange={handleInputChange}
                      placeholder="Web Form"
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <FormControlLabel
                      control={
                        <Checkbox 
                          checked={formData.updateExistingLead ?? true}
                          onChange={handleInputChange}
                          name="updateExistingLead"
                        />
                      }
                      label="Update Existing Lead if Found"
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <FormControl fullWidth>
                      <InputLabel>Match Existing Lead By</InputLabel>
                      <Select
                        name="existingLeadMatchField"
                        value={formData.existingLeadMatchField || 'Email'}
                        onChange={handleSelectChange}
                        label="Match Existing Lead By"
                      >
                        <MenuItem value="Email">Email</MenuItem>
                        <MenuItem value="Phone">Phone</MenuItem>
                        <MenuItem value="Both">Email or Phone</MenuItem>
                      </Select>
                    </FormControl>
                  </Grid>
                </>
              )}
            </Grid>
          </TabPanel>

          {/* Tab 4: Notifications */}
          <TabPanel value={dialogTab} index={4}>
            <Grid container spacing={3}>
              <Grid item xs={12}>
                <FormControlLabel
                  control={
                    <Checkbox 
                      checked={formData.notifyOwner ?? true}
                      onChange={handleInputChange}
                      name="notifyOwner"
                    />
                  }
                  label="Notify Lead Owner on Submission"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Additional Notification Recipients"
                  name="notificationRecipients"
                  value={formData.notificationRecipients || ''}
                  onChange={handleInputChange}
                  helperText="Comma-separated email addresses"
                  placeholder="sales@company.com, marketing@company.com"
                />
              </Grid>
              <Grid item xs={12}>
                <Divider sx={{ my: 1 }} />
              </Grid>
              <Grid item xs={12}>
                <FormControlLabel
                  control={
                    <Checkbox 
                      checked={formData.sendAutoresponder ?? false}
                      onChange={handleInputChange}
                      name="sendAutoresponder"
                    />
                  }
                  label="Send Autoresponder Email to Submitter"
                />
              </Grid>
            </Grid>
          </TabPanel>

          {/* Tab 5: Styling */}
          <TabPanel value={dialogTab} index={5}>
            <Grid container spacing={3}>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Form Width"
                  name="width"
                  value={formData.width || ''}
                  onChange={handleInputChange}
                  placeholder="100% or 600px"
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Theme"
                  name="theme"
                  value={formData.theme || ''}
                  onChange={handleInputChange}
                  placeholder="default"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="CSS Classes"
                  name="cssClasses"
                  value={formData.cssClasses || ''}
                  onChange={handleInputChange}
                  placeholder="my-custom-form rounded"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  multiline
                  rows={4}
                  label="Custom CSS"
                  name="customCss"
                  value={formData.customCss || ''}
                  onChange={handleInputChange}
                  placeholder=".my-form { background: #f5f5f5; }"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Allowed Domains (for embedding)"
                  name="allowedDomains"
                  value={formData.allowedDomains || ''}
                  onChange={handleInputChange}
                  helperText="Comma-separated domains that can embed this form"
                  placeholder="example.com, another-site.com"
                />
              </Grid>
            </Grid>
          </TabPanel>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <ActionButton
            onClick={handleSaveForm}
            loading={dialogApi.loading}
            variant="contained"
          >
            {editingForm ? 'Update Form' : 'Create Form'}
          </ActionButton>
        </DialogActions>
      </Dialog>

      {/* Field Editor Dialog */}
      <Dialog open={fieldDialogOpen} onClose={() => setFieldDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          {editingField?.id ? 'Edit Field' : 'Add Field'}
        </DialogTitle>
        <DialogContent dividers>
          <Grid container spacing={2}>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                required
                label="Label"
                value={editingField?.label || ''}
                onChange={(e) => setEditingField(prev => prev ? { ...prev, label: e.target.value } : null)}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                required
                label="Field Name"
                value={editingField?.fieldName || ''}
                onChange={(e) => setEditingField(prev => prev ? { ...prev, fieldName: e.target.value.replaceAll(/\s/g, '_') } : null)}
                helperText="Internal name (no spaces)"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Field Type</InputLabel>
                <Select
                  value={editingField?.fieldType ?? FormFieldType.Text}
                  onChange={(e) => setEditingField(prev => prev ? { ...prev, fieldType: e.target.value as FormFieldType } : null)}
                  label="Field Type"
                >
                  {FIELD_TYPE_OPTIONS.map(opt => (
                    <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Width</InputLabel>
                <Select
                  value={editingField?.width || 'full'}
                  onChange={(e) => setEditingField(prev => prev ? { ...prev, width: e.target.value } : null)}
                  label="Width"
                >
                  {WIDTH_OPTIONS.map(opt => (
                    <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Checkbox 
                    checked={editingField?.isRequired ?? false}
                    onChange={(e) => setEditingField(prev => prev ? { ...prev, isRequired: e.target.checked } : null)}
                  />
                }
                label="Required"
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Placeholder"
                value={editingField?.placeholder || ''}
                onChange={(e) => setEditingField(prev => prev ? { ...prev, placeholder: e.target.value } : null)}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Help Text"
                value={editingField?.helpText || ''}
                onChange={(e) => setEditingField(prev => prev ? { ...prev, helpText: e.target.value } : null)}
              />
            </Grid>
            {(editingField?.fieldType === FormFieldType.Dropdown || 
              editingField?.fieldType === FormFieldType.MultiSelect ||
              editingField?.fieldType === FormFieldType.Radio) && (
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  multiline
                  rows={4}
                  label="Options"
                  value={editingField?.options || ''}
                  onChange={(e) => setEditingField(prev => prev ? { ...prev, options: e.target.value } : null)}
                  helperText="One option per line, or JSON array"
                  placeholder="Option 1&#10;Option 2&#10;Option 3"
                />
              </Grid>
            )}
            <Grid item xs={12}>
              <Divider sx={{ my: 1 }} />
              <Typography variant="subtitle2" gutterBottom>CRM Mapping</Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Map to Entity</InputLabel>
                <Select
                  value={editingField?.crmEntityMapping || ''}
                  onChange={(e) => setEditingField(prev => prev ? { ...prev, crmEntityMapping: e.target.value } : null)}
                  label="Map to Entity"
                >
                  <MenuItem value="">None</MenuItem>
                  <MenuItem value="Lead">Lead</MenuItem>
                  <MenuItem value="Contact">Contact</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Map to Field</InputLabel>
                <Select
                  value={editingField?.crmFieldMapping || ''}
                  onChange={(e) => setEditingField(prev => prev ? { ...prev, crmFieldMapping: e.target.value } : null)}
                  label="Map to Field"
                >
                  <MenuItem value="">None</MenuItem>
                  {CRM_FIELD_MAPPINGS.find(m => m.entity === editingField?.crmEntityMapping)?.fields.map(field => (
                    <MenuItem key={field} value={field}>{field}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setFieldDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSaveField}>
            {editingField?.id ? 'Update Field' : 'Add Field'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Embed Code Dialog */}
      <Dialog open={embedDialogOpen} onClose={() => setEmbedDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          <Box display="flex" justifyContent="space-between" alignItems="center">
            Embed Code
            <IconButton onClick={() => setEmbedDialogOpen(false)}><CloseIcon /></IconButton>
          </Box>
        </DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" paragraph>
            Copy and paste this code into your website to display the form.
          </Typography>
          <TextField
            fullWidth
            multiline
            rows={12}
            value={embedCode}
            InputProps={{ readOnly: true, sx: { fontFamily: 'monospace', fontSize: 12 } }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEmbedDialogOpen(false)}>Close</Button>
          <Button variant="contained" startIcon={<CopyIcon />} onClick={handleCopyEmbedCode}>
            Copy Code
          </Button>
        </DialogActions>
      </Dialog>

      {/* Submissions Dialog */}
      <Dialog open={submissionsDialogOpen} onClose={() => setSubmissionsDialogOpen(false)} maxWidth="lg" fullWidth>
        <DialogTitle>
          <Box display="flex" justifyContent="space-between" alignItems="center">
            Form Submissions
            <IconButton onClick={() => setSubmissionsDialogOpen(false)}><CloseIcon /></IconButton>
          </Box>
        </DialogTitle>
        <DialogContent>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Submission #</TableCell>
                <TableCell>Submitted At</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>IP Address</TableCell>
                <TableCell>Lead ID</TableCell>
                <TableCell>Spam</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {submissions.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} align="center">
                    <Typography color="text.secondary" py={2}>No submissions yet</Typography>
                  </TableCell>
                </TableRow>
              ) : (
                submissions.map((sub) => {
                  const statusInfo = getSubmissionStatusInfo(sub.status);
                  return (
                    <TableRow key={sub.id}>
                      <TableCell>{sub.submissionNumber}</TableCell>
                      <TableCell>{new Date(sub.submittedAt).toLocaleString()}</TableCell>
                      <TableCell>
                        <Chip label={statusInfo.label} size="small" color={statusInfo.color} />
                      </TableCell>
                      <TableCell>{sub.ipAddress || '-'}</TableCell>
                      <TableCell>{sub.leadId || '-'}</TableCell>
                      <TableCell>
                        {sub.isSpam && <Chip label="Spam" size="small" color="error" />}
                      </TableCell>
                    </TableRow>
                  );
                })
              )}
            </TableBody>
          </Table>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSubmissionsDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

export default FormBuilderPage;

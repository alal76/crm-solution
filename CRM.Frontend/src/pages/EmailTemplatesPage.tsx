import { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Alert,
  IconButton,
  Chip,
  CircularProgress,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormControlLabel,
  Switch,
  Tab,
  Tabs,
  Container,
  TableContainer,
  Stack,
  Tooltip,
  Paper,
  SelectChangeEvent,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  ContentCopy as DuplicateIcon,
  Visibility as PreviewIcon,
  Email as EmailIcon,
  CheckCircle as ActiveIcon,
  Cancel as InactiveIcon,
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import { BaseEntity } from '../types';
import logo from '../assets/logo.png';
import { DialogError, ActionButton, DialogHeader, EnhancedEmptyState } from '../components/common';
import { useApiState } from '../hooks/useApiState';
import { useProfile } from '../contexts/ProfileContext';
import { TabPanel } from '../components/common';

// Template categories
const TEMPLATE_CATEGORIES = [
  { value: 'General', label: 'General', color: '#6750A4' },
  { value: 'Sales', label: 'Sales', color: '#06A77D' },
  { value: 'Marketing', label: 'Marketing', color: '#1976D2' },
  { value: 'Support', label: 'Support', color: '#ED6C02' },
  { value: 'Welcome', label: 'Welcome', color: '#2E7D32' },
  { value: 'FollowUp', label: 'Follow Up', color: '#9C27B0' },
  { value: 'Newsletter', label: 'Newsletter', color: '#E91E63' },
  { value: 'Notification', label: 'Notification', color: '#00BCD4' },
  { value: 'Transactional', label: 'Transactional', color: '#795548' },
  { value: 'Custom', label: 'Custom', color: '#607D8B' },
];

interface EmailTemplate extends BaseEntity {
  name: string;
  description?: string;
  category: string;
  subject: string;
  plainTextBody?: string;
  htmlBody?: string;
  isActive: boolean;
  isSystem: boolean;
  fromEmail?: string;
  fromName?: string;
  replyToEmail?: string;
  usageCount: number;
  lastUsedAt?: string;
}

interface TemplateForm {
  name: string;
  description: string;
  category: string;
  subject: string;
  plainTextBody: string;
  htmlBody: string;
  isActive: boolean;
  fromEmail: string;
  fromName: string;
  replyToEmail: string;
}

const emptyForm: TemplateForm = {
  name: '',
  description: '',
  category: 'General',
  subject: '',
  plainTextBody: '',
  htmlBody: '',
  isActive: true,
  fromEmail: '',
  fromName: '',
  replyToEmail: '',
};

function EmailTemplatesPage() {
  const [templates, setTemplates] = useState<EmailTemplate[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  const [formData, setFormData] = useState<TemplateForm>(emptyForm);
  const [categoryFilter, setCategoryFilter] = useState<string>('');
  
  // Preview dialog
  const [previewOpen, setPreviewOpen] = useState(false);
  const [previewContent, setPreviewContent] = useState<{ subject: string; body: string } | null>(null);
  
  const dialogApi = useApiState({ successTimeout: 3000 });
  const { hasPermission } = useProfile();

  const fetchTemplates = useCallback(async () => {
    try {
      setLoading(true);
      const params = categoryFilter ? `?category=${categoryFilter}` : '';
      const response = await apiClient.get(`/emailtemplates${params}`);
      setTemplates(response.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch email templates');
    } finally {
      setLoading(false);
    }
  }, [categoryFilter]);

  useEffect(() => {
    fetchTemplates();
  }, [fetchTemplates]);

  const handleOpenDialog = (template?: EmailTemplate) => {
    setDialogTab(0);
    dialogApi.clearError();
    if (template) {
      setEditingId(template.id);
      setFormData({
        name: template.name,
        description: template.description || '',
        category: template.category,
        subject: template.subject,
        plainTextBody: template.plainTextBody || '',
        htmlBody: template.htmlBody || '',
        isActive: template?.isActive !== false,
        fromEmail: template.fromEmail || '',
        fromName: template.fromName || '',
        replyToEmail: template.replyToEmail || '',
      });
    } else {
      setEditingId(null);
      setFormData(emptyForm);
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
    dialogApi.clearError();
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSelectChange = (e: SelectChangeEvent<string>) => {
    setFormData(prev => ({ ...prev, category: e.target.value }));
  };

  const handleSave = async () => {
    if (!formData.name.trim() || !formData.subject.trim()) {
      dialogApi.setError('Name and Subject are required');
      return;
    }

    const result = await dialogApi.execute(async () => {
      if (editingId) {
        await apiClient.put(`/emailtemplates/${editingId}`, formData);
        return 'updated';
      } else {
        await apiClient.post('/emailtemplates', formData);
        return 'created';
      }
    }, editingId ? 'Template updated successfully' : 'Template created successfully');

    if (result) {
      handleCloseDialog();
      fetchTemplates();
      setSuccessMessage(result === 'updated' ? 'Template updated' : 'Template created');
      setTimeout(() => setSuccessMessage(null), 3000);
    }
  };

  const handleDelete = async (id: number, isSystem: boolean) => {
    if (isSystem) {
      setError('Cannot delete system templates');
      return;
    }
    if (!window.confirm('Are you sure you want to delete this template?')) return;

    try {
      await apiClient.delete(`/emailtemplates/${id}`);
      fetchTemplates();
      setSuccessMessage('Template deleted');
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete template');
    }
  };

  const handleDuplicate = async (id: number) => {
    try {
      await apiClient.post(`/emailtemplates/${id}/duplicate`);
      fetchTemplates();
      setSuccessMessage('Template duplicated');
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to duplicate template');
    }
  };

  const handlePreview = async (id: number) => {
    try {
      const response = await apiClient.post(`/emailtemplates/${id}/preview`, {
        FirstName: 'John',
        LastName: 'Doe',
        Company: 'Acme Corp',
        Email: 'john.doe@example.com',
      });
      setPreviewContent({
        subject: response.data.subject,
        body: response.data.body,
      });
      setPreviewOpen(true);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to preview template');
    }
  };

  const getCategoryStyle = (category: string) => {
    const cat = TEMPLATE_CATEGORIES.find(c => c.value === category);
    return cat ? { color: cat.color } : { color: '#6750A4' };
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ py: 2 }}>
      <Box sx={{ mb: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
            <img src={logo} alt="CRM Logo" style={{ width: '100%', height: '100%', objectFit: 'contain' }} />
          </Box>
          <Box>
            <Typography variant="h3" sx={{ fontWeight: 700, mb: 0.5 }}>
              Email Templates
            </Typography>
            <Typography color="textSecondary" variant="body2">
              Create and manage reusable email templates for campaigns
            </Typography>
          </Box>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenDialog()}
          sx={{ backgroundColor: '#6750A4', textTransform: 'none', borderRadius: 2 }}
        >
          New Template
        </Button>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
      {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

      {/* Category Filter */}
      <Box sx={{ mb: 3 }}>
        <FormControl size="small" sx={{ minWidth: 200 }}>
          <InputLabel>Filter by Category</InputLabel>
          <Select
            value={categoryFilter}
            label="Filter by Category"
            onChange={(e) => setCategoryFilter(e.target.value)}
          >
            <MenuItem value="">All Categories</MenuItem>
            {TEMPLATE_CATEGORIES.map(cat => (
              <MenuItem key={cat.value} value={cat.value}>{cat.label}</MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>

      <Card sx={{ borderRadius: 3, boxShadow: 1 }}>
        <CardContent sx={{ p: 0 }}>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                  <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Name</TableCell>
                  <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Category</TableCell>
                  <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Subject</TableCell>
                  <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Status</TableCell>
                  <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Usage</TableCell>
                  <TableCell sx={{ fontWeight: 600, color: '#6750A4' }} align="center">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {templates.map(template => {
                  const catStyle = getCategoryStyle(template.category);
                  return (
                    <TableRow key={template.id} hover>
                      <TableCell>
                        <Box>
                          <Typography fontWeight={600}>{template.name}</Typography>
                          {template.description && (
                            <Typography variant="caption" color="textSecondary">
                              {template.description}
                            </Typography>
                          )}
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Chip 
                          label={TEMPLATE_CATEGORIES.find(c => c.value === template.category)?.label || template.category}
                          size="small"
                          sx={{ 
                            backgroundColor: catStyle.color + '20',
                            color: catStyle.color,
                            fontWeight: 600,
                          }}
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" noWrap sx={{ maxWidth: 250 }}>
                          {template.subject}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip
                          icon={template?.isActive !== false ? <ActiveIcon /> : <InactiveIcon />}
                          label={template?.isActive !== false ? 'Active' : 'Inactive'}
                          variant="filled"
                          sx={{
                            backgroundColor: template?.isActive !== false ? '#E8F5E9' : '#FFEBEE',
                            color: template?.isActive !== false ? '#2E7D32' : '#C62828',
                          }}
                        />
                        {template.isSystem && (
                          <Chip
                            label="System"
                            size="small"
                            sx={{ ml: 0.5, backgroundColor: '#E3F2FD', color: '#1565C0' }}
                          />
                        )}
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {template.usageCount} uses
                        </Typography>
                        {template.lastUsedAt && (
                          <Typography variant="caption" color="textSecondary">
                            Last: {new Date(template.lastUsedAt).toLocaleDateString()}
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell align="center">
                        <Tooltip title="Preview">
                          <IconButton size="small" onClick={() => handlePreview(template.id)}>
                            <PreviewIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Duplicate">
                          <IconButton size="small" onClick={() => handleDuplicate(template.id)}>
                            <DuplicateIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        {!template.isSystem && (
                          <>
                            <Tooltip title="Edit">
                              <IconButton 
                                size="small" 
                                onClick={() => handleOpenDialog(template)}
                                sx={{ color: '#6750A4' }}
                              >
                                <EditIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Delete">
                              <IconButton
                                size="small"
                                onClick={() => handleDelete(template.id, template.isSystem)}
                                sx={{ color: '#B3261E' }}
                              >
                                <DeleteIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                          </>
                        )}
                      </TableCell>
                    </TableRow>
                  );
                })}
                {templates.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={6} align="center" sx={{ py: 0 }}>
                      <EnhancedEmptyState
                        illustration="campaigns"
                        variant={categoryFilter ? 'no-results' : 'no-data'}
                        title={categoryFilter ? 'No templates in this category' : 'No email templates yet'}
                        primaryActionLabel="Create Template"
                        onPrimaryAction={() => handleOpenDialog()}
                      />
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </CardContent>
      </Card>

      {/* Create/Edit Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogHeader 
          mode={editingId ? 'edit' : 'create'}
          entityType="campaign"
          title={editingId ? 'Edit Email Template' : 'Create Email Template'}
          onClose={handleCloseDialog}
        />
        <DialogContent dividers>
          <DialogError error={dialogApi.error} onClose={dialogApi.clearError} />
          
          <Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)} sx={{ mb: 2 }}>
            <Tab label="Details" icon={<EmailIcon fontSize="small" />} iconPosition="start" />
            <Tab label="Content" icon={<EditIcon fontSize="small" />} iconPosition="start" />
          </Tabs>

          <TabPanel value={dialogTab} index={0}>
            <Stack spacing={2}>
              <TextField
                label="Template Name"
                name="name"
                value={formData.name}
                onChange={handleInputChange}
                fullWidth
                required
              />
              <TextField
                label="Description"
                name="description"
                value={formData.description}
                onChange={handleInputChange}
                fullWidth
                multiline
                rows={2}
              />
              <FormControl fullWidth>
                <InputLabel>Category</InputLabel>
                <Select
                  value={formData.category}
                  label="Category"
                  onChange={handleSelectChange}
                >
                  {TEMPLATE_CATEGORIES.map(cat => (
                    <MenuItem key={cat.value} value={cat.value}>{cat.label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
              <TextField
                label="Email Subject"
                name="subject"
                value={formData.subject}
                onChange={handleInputChange}
                fullWidth
                required
                helperText="Use {{FirstName}}, {{Company}}, etc. for merge fields"
              />
              <Box sx={{ display: 'flex', gap: 2 }}>
                <TextField
                  label="From Email"
                  name="fromEmail"
                  value={formData.fromEmail}
                  onChange={handleInputChange}
                  fullWidth
                  placeholder="noreply@company.com"
                />
                <TextField
                  label="From Name"
                  name="fromName"
                  value={formData.fromName}
                  onChange={handleInputChange}
                  fullWidth
                  placeholder="Sales Team"
                />
              </Box>
              <TextField
                label="Reply-To Email"
                name="replyToEmail"
                value={formData.replyToEmail}
                onChange={handleInputChange}
                fullWidth
              />
              <FormControlLabel
                control={
                  <Switch
                    checked={formData?.isActive !== false}
                    onChange={(e) => setFormData(prev => ({ ...prev, isActive: e.target.checked }))}
                  />
                }
                label="Active"
              />
            </Stack>
          </TabPanel>

          <TabPanel value={dialogTab} index={1}>
            <Stack spacing={2}>
              <Alert severity="info" sx={{ mb: 1 }}>
                Available merge fields: {'{{FirstName}}'}, {'{{LastName}}'}, {'{{Company}}'}, {'{{Email}}'}, {'{{Phone}}'}
              </Alert>
              <TextField
                label="Plain Text Body"
                name="plainTextBody"
                value={formData.plainTextBody}
                onChange={handleInputChange}
                fullWidth
                multiline
                rows={6}
                placeholder="Enter the plain text version of your email..."
              />
              <TextField
                label="HTML Body"
                name="htmlBody"
                value={formData.htmlBody}
                onChange={handleInputChange}
                fullWidth
                multiline
                rows={10}
                placeholder="<html><body>Your HTML email content...</body></html>"
              />
            </Stack>
          </TabPanel>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <ActionButton
            label={editingId ? 'Update' : 'Create'}
            loading={dialogApi.loading}
            onClick={handleSave}
          />
        </DialogActions>
      </Dialog>

      {/* Preview Dialog */}
      <Dialog open={previewOpen} onClose={() => setPreviewOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>Template Preview</DialogTitle>
        <DialogContent>
          {previewContent && (
            <Box>
              <Typography variant="subtitle2" color="textSecondary">Subject:</Typography>
              <Typography variant="h6" sx={{ mb: 2 }}>{previewContent.subject}</Typography>
              <Typography variant="subtitle2" color="textSecondary">Body:</Typography>
              <Paper 
                variant="outlined" 
                sx={{ p: 2, mt: 1, maxHeight: 400, overflow: 'auto' }}
                dangerouslySetInnerHTML={{ __html: previewContent.body }}
              />
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setPreviewOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

export default EmailTemplatesPage;

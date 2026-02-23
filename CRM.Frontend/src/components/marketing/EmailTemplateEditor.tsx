/**
 * EmailTemplateEditor - Editor for creating and editing email templates
 * Features: subject line, HTML body, preview pane, variable picker
 */

import React, { useState, useCallback, useMemo } from 'react';
import {
  Box,
  Grid,
  TextField,
  Button,
  Typography,
  Paper,
  Chip,
  Divider,
  IconButton,
  Tooltip,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Tab,
  Tabs,
  Alert,
  SelectChangeEvent,
} from '@mui/material';
import {
  Save as SaveIcon,
  Close as CancelIcon,
  ContentCopy as CopyIcon,
  Visibility as PreviewIcon,
  Code as CodeIcon,
} from '@mui/icons-material';
import { EmailTemplate } from '../../types/marketing';

// ============================================================================
// Types
// ============================================================================

export interface EmailTemplateData {
  name: string;
  subject: string;
  htmlBody: string;
  textBody?: string;
  variables: string[];
  category: string;
}

export interface EmailTemplateEditorProps {
  template?: EmailTemplate;
  onSave: (template: EmailTemplateData) => void;
  onCancel: () => void;
}

// ============================================================================
// Constants
// ============================================================================

const AVAILABLE_VARIABLES = [
  { key: 'firstName', label: 'First Name', example: 'John' },
  { key: 'lastName', label: 'Last Name', example: 'Doe' },
  { key: 'companyName', label: 'Company Name', example: 'Acme Inc.' },
  { key: 'email', label: 'Email', example: 'john@acme.com' },
  { key: 'title', label: 'Job Title', example: 'VP of Sales' },
  { key: 'phone', label: 'Phone', example: '+1-555-0100' },
  { key: 'city', label: 'City', example: 'San Francisco' },
  { key: 'country', label: 'Country', example: 'United States' },
  { key: 'customField1', label: 'Custom Field 1', example: 'Custom Value 1' },
  { key: 'customField2', label: 'Custom Field 2', example: 'Custom Value 2' },
  { key: 'customField3', label: 'Custom Field 3', example: 'Custom Value 3' },
  { key: 'customField4', label: 'Custom Field 4', example: 'Custom Value 4' },
  { key: 'customField5', label: 'Custom Field 5', example: 'Custom Value 5' },
  { key: 'unsubscribeLink', label: 'Unsubscribe Link', example: '#' },
  { key: 'currentDate', label: 'Current Date', example: new Date().toLocaleDateString() },
];

const TEMPLATE_CATEGORIES = [
  'Welcome',
  'Newsletter',
  'Promotional',
  'Transactional',
  'Follow-Up',
  'Event Invitation',
  'Product Update',
  'Survey',
  'Re-engagement',
  'Other',
];

// ============================================================================
// Component
// ============================================================================

const EmailTemplateEditor: React.FC<EmailTemplateEditorProps> = ({
  template,
  onSave,
  onCancel,
}) => {
  const [formData, setFormData] = useState<EmailTemplateData>({
    name: template?.name || '',
    subject: template?.subject || '',
    htmlBody: template?.htmlContent || '',
    textBody: template?.textContent || '',
    variables: template?.variables || [],
    category: template?.category || 'Newsletter',
  });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [previewTab, setPreviewTab] = useState<number>(0); // 0=edit, 1=preview
  const [copiedVariable, setCopiedVariable] = useState<string | null>(null);

  const handleFieldChange = useCallback(
    (field: keyof EmailTemplateData, value: unknown) => {
      setFormData((prev) => ({ ...prev, [field]: value }));
      setErrors((prev) => {
        const next = { ...prev };
        delete next[field];
        return next;
      });
    },
    []
  );

  const insertVariable = useCallback(
    (variableKey: string) => {
      const tag = `{{${variableKey}}}`;

      // Add to subject (append at cursor or end)
      // For simplicity we just copy to clipboard
      navigator.clipboard?.writeText(tag).catch(() => {
        // Clipboard not available; fallback
      });

      // Track used variables
      setFormData((prev) => ({
        ...prev,
        variables: prev.variables.includes(tag) ? prev.variables : [...prev.variables, tag],
      }));

      setCopiedVariable(variableKey);
      setTimeout(() => setCopiedVariable(null), 1500);
    },
    []
  );

  const validate = useCallback((): boolean => {
    const newErrors: Record<string, string> = {};
    if (!formData.name.trim()) newErrors.name = 'Template name is required';
    if (!formData.subject.trim()) newErrors.subject = 'Subject line is required';
    if (!formData.htmlBody.trim()) newErrors.htmlBody = 'HTML body is required';
    if (!formData.category) newErrors.category = 'Category is required';
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  }, [formData]);

  const handleSave = useCallback(() => {
    if (validate()) {
      onSave(formData);
    }
  }, [formData, onSave, validate]);

  // Render preview with sample data
  const renderedPreview = useMemo(() => {
    let html = formData.htmlBody;
    AVAILABLE_VARIABLES.forEach((v) => {
      const regex = new RegExp(`\\{\\{${v.key}\\}\\}`, 'g');
      html = html.replace(regex, v.example);
    });
    return html;
  }, [formData.htmlBody]);

  return (
    <Box sx={{ height: '100%' }}>
      {/* Header */}
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          mb: 2,
        }}
      >
        <Typography variant="h6">
          {template ? 'Edit Email Template' : 'Create Email Template'}
        </Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button onClick={onCancel} startIcon={<CancelIcon />} color="inherit">
            Cancel
          </Button>
          <Button variant="contained" onClick={handleSave} startIcon={<SaveIcon />}>
            Save Template
          </Button>
        </Box>
      </Box>

      <Divider sx={{ mb: 2 }} />

      <Grid container spacing={2}>
        {/* Left Column — Form */}
        <Grid item xs={12} md={9}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Template Name"
                value={formData.name}
                onChange={(e) => handleFieldChange('name', e.target.value)}
                error={!!errors.name}
                helperText={errors.name}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth error={!!errors.category}>
                <InputLabel>Category</InputLabel>
                <Select
                  value={formData.category}
                  label="Category"
                  onChange={(e: SelectChangeEvent) =>
                    handleFieldChange('category', e.target.value)
                  }
                >
                  {TEMPLATE_CATEGORIES.map((cat) => (
                    <MenuItem key={cat} value={cat}>
                      {cat}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Subject Line"
                value={formData.subject}
                onChange={(e) => handleFieldChange('subject', e.target.value)}
                error={!!errors.subject}
                helperText={
                  errors.subject ||
                  `${formData.subject.length} characters — use variables like {{firstName}}`
                }
                required
                placeholder="e.g., Hey {{firstName}}, check out our latest update!"
              />
            </Grid>

            {/* Editor / Preview Tabs */}
            <Grid item xs={12}>
              <Tabs
                value={previewTab}
                onChange={(_, val) => setPreviewTab(val)}
                sx={{ mb: 1 }}
              >
                <Tab icon={<CodeIcon fontSize="small" />} iconPosition="start" label="HTML Editor" />
                <Tab icon={<PreviewIcon fontSize="small" />} iconPosition="start" label="Preview" />
              </Tabs>

              {previewTab === 0 ? (
                <Box>
                  <TextField
                    fullWidth
                    multiline
                    rows={14}
                    label="HTML Body"
                    value={formData.htmlBody}
                    onChange={(e) => handleFieldChange('htmlBody', e.target.value)}
                    error={!!errors.htmlBody}
                    helperText={errors.htmlBody}
                    required
                    InputProps={{
                      sx: { fontFamily: 'monospace', fontSize: '0.85rem' },
                    }}
                    placeholder={`<html>\n<body>\n  <h1>Hello {{firstName}},</h1>\n  <p>Welcome to {{companyName}}!</p>\n</body>\n</html>`}
                  />
                  <TextField
                    fullWidth
                    multiline
                    rows={4}
                    label="Plain Text Body (Optional)"
                    value={formData.textBody || ''}
                    onChange={(e) => handleFieldChange('textBody', e.target.value)}
                    sx={{ mt: 2 }}
                    placeholder="Fallback text for email clients that don't support HTML"
                  />
                </Box>
              ) : (
                <Paper
                  variant="outlined"
                  sx={{
                    p: 2,
                    minHeight: 300,
                    bgcolor: '#fafafa',
                    overflow: 'auto',
                  }}
                >
                  <Typography variant="caption" color="text.secondary" sx={{ mb: 1, display: 'block' }}>
                    Subject: {formData.subject || '(no subject)'}
                  </Typography>
                  <Divider sx={{ mb: 2 }} />
                  {renderedPreview ? (
                    <div
                      dangerouslySetInnerHTML={{ __html: renderedPreview }}
                      style={{ maxWidth: '100%', wordBreak: 'break-word' }}
                    />
                  ) : (
                    <Typography variant="body2" color="text.secondary" sx={{ fontStyle: 'italic' }}>
                      Enter HTML content in the editor to see a preview
                    </Typography>
                  )}
                </Paper>
              )}
            </Grid>
          </Grid>
        </Grid>

        {/* Right Column — Variable Picker */}
        <Grid item xs={12} md={3}>
          <Paper variant="outlined" sx={{ p: 2, position: 'sticky', top: 16 }}>
            <Typography variant="subtitle2" gutterBottom>
              Template Variables
            </Typography>
            <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1.5 }}>
              Click a variable to copy its tag to your clipboard, then paste it into the subject or body.
            </Typography>
            <Divider sx={{ mb: 1.5 }} />
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
              {AVAILABLE_VARIABLES.map((v) => (
                <Tooltip
                  key={v.key}
                  title={`Example: ${v.example}`}
                  placement="left"
                  arrow
                >
                  <Chip
                    label={
                      copiedVariable === v.key
                        ? 'Copied!'
                        : `{{${v.key}}}`
                    }
                    size="small"
                    variant={copiedVariable === v.key ? 'filled' : 'outlined'}
                    color={copiedVariable === v.key ? 'success' : 'default'}
                    onClick={() => insertVariable(v.key)}
                    icon={<CopyIcon fontSize="small" />}
                    sx={{
                      justifyContent: 'flex-start',
                      fontFamily: 'monospace',
                      fontSize: '0.75rem',
                      cursor: 'pointer',
                    }}
                  />
                </Tooltip>
              ))}
            </Box>

            {formData.variables.length > 0 && (
              <>
                <Divider sx={{ my: 1.5 }} />
                <Typography variant="subtitle2" gutterBottom>
                  Used Variables
                </Typography>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {formData.variables.map((tag) => (
                    <Chip
                      key={tag}
                      label={tag}
                      size="small"
                      color="primary"
                      variant="outlined"
                      onDelete={() =>
                        handleFieldChange(
                          'variables',
                          formData.variables.filter((v) => v !== tag)
                        )
                      }
                    />
                  ))}
                </Box>
              </>
            )}
          </Paper>
        </Grid>
      </Grid>

      {/* Validation Alerts */}
      {Object.keys(errors).length > 0 && (
        <Alert severity="error" sx={{ mt: 2 }}>
          Please fix the errors above before saving.
        </Alert>
      )}
    </Box>
  );
};

export default EmailTemplateEditor;

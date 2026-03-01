// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
import React, { useState, useEffect, useRef } from 'react';
import {
  AppBar,
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  FormControl,
  FormHelperText,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Toolbar,
  Typography,
  Alert,
  List,
  ListItem,
  ListItemText,
  IconButton as MuiIconButton,
} from '@mui/material';
import { ArrowBack, ExitToApp, SupportAgent, AttachFile, Close } from '@mui/icons-material';
import { useNavigate, Link } from 'react-router-dom';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import {
  portalAuthService,
  portalService,
  type PortalConfigDto,
} from '../../services/portalService';

const CATEGORIES = ['General', 'Technical', 'Billing', 'Feature Request', 'Bug Report'];

const validationSchema = Yup.object({
  title: Yup.string().required('Subject is required').max(200, 'Max 200 characters'),
  category: Yup.string().required('Category is required'),
  priority: Yup.string().required('Priority is required'),
  description: Yup.string()
    .required('Description is required')
    .min(20, 'Min 20 characters')
    .max(2000, 'Max 2000 characters'),
});

const PortalNewTicketPage: React.FC = () => {
  const navigate = useNavigate();
  const [config, setConfig] = useState<PortalConfigDto | null>(null);
  const [files, setFiles] = useState<File[]>([]);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const user = portalAuthService.getCurrentUser();

  useEffect(() => {
    if (!portalAuthService.isAuthenticated()) {
      navigate('/portal/login', { replace: true });
      return;
    }
    portalService.getConfig().then(setConfig).catch(() => {});
  }, [navigate]);

  const handleLogout = () => {
    portalAuthService.logout();
    navigate('/portal/login', { replace: true });
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      const newFiles = Array.from(e.target.files).filter((f) => f.size <= 10 * 1024 * 1024);
      setFiles((prev) => [...prev, ...newFiles]);
    }
    if (fileInputRef.current) fileInputRef.current.value = '';
  };

  const removeFile = (idx: number) => {
    setFiles((prev) => prev.filter((_, i) => i !== idx));
  };

  const formik = useFormik({
    initialValues: {
      title: '',
      category: '',
      priority: 'Medium',
      description: '',
    },
    validationSchema,
    onSubmit: async (values) => {
      setSubmitError(null);
      try {
        const ticket = await portalService.createTicket({
          title: values.title,
          description: values.description,
          priority: values.priority,
        });
        // Upload attachments if any
        for (const file of files) {
          try {
            await portalService.uploadAttachment(ticket.id, file);
          } catch {
            // Non-fatal
          }
        }
        navigate(`/portal/tickets/${ticket.id}`);
      } catch (err: any) {
        setSubmitError(err?.response?.data?.message ?? 'Failed to create ticket. Please try again.');
      }
    },
  });

  const brandColor = config?.primaryColor ?? '#1976d2';

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <AppBar position="static" sx={{ bgcolor: brandColor }}>
        <Toolbar>
          <IconButton color="inherit" component={Link} to="/portal/tickets" sx={{ mr: 1 }}>
            <ArrowBack />
          </IconButton>
          <SupportAgent sx={{ mr: 1 }} />
          <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 700 }}>
            New Support Ticket
          </Typography>
          <Typography variant="body2" sx={{ mr: 2 }}>{user?.displayName ?? user?.email}</Typography>
          <IconButton color="inherit" onClick={handleLogout} title="Sign out">
            <ExitToApp />
          </IconButton>
        </Toolbar>
      </AppBar>

      <Box sx={{ p: 3, maxWidth: 720, mx: 'auto' }}>
        <Typography variant="h5" fontWeight={700} mb={3}>
          Submit a Support Request
        </Typography>

        <Card>
          <CardContent sx={{ p: 3 }}>
            {submitError && <Alert severity="error" sx={{ mb: 2 }}>{submitError}</Alert>}

            <Box component="form" onSubmit={formik.handleSubmit}>
              <TextField
                fullWidth
                label="Subject *"
                name="title"
                value={formik.values.title}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                error={formik.touched.title && !!formik.errors.title}
                helperText={formik.touched.title && formik.errors.title}
                sx={{ mb: 2 }}
                autoFocus
              />

              <FormControl fullWidth sx={{ mb: 2 }} error={formik.touched.category && !!formik.errors.category}>
                <InputLabel>Category *</InputLabel>
                <Select
                  name="category"
                  value={formik.values.category}
                  label="Category *"
                  onChange={formik.handleChange}
                  onBlur={formik.handleBlur}
                >
                  {CATEGORIES.map((cat) => (
                    <MenuItem key={cat} value={cat}>{cat}</MenuItem>
                  ))}
                </Select>
                {formik.touched.category && formik.errors.category && (
                  <FormHelperText>{formik.errors.category}</FormHelperText>
                )}
              </FormControl>

              <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel>Priority</InputLabel>
                <Select
                  name="priority"
                  value={formik.values.priority}
                  label="Priority"
                  onChange={formik.handleChange}
                >
                  <MenuItem value="Low">Low</MenuItem>
                  <MenuItem value="Medium">Medium</MenuItem>
                  <MenuItem value="High">High</MenuItem>
                </Select>
              </FormControl>

              <TextField
                fullWidth
                label="Description *"
                name="description"
                multiline
                rows={6}
                value={formik.values.description}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                error={formik.touched.description && !!formik.errors.description}
                helperText={
                  (formik.touched.description && formik.errors.description) ||
                  `${formik.values.description.length}/2000 characters (min 20)`
                }
                sx={{ mb: 2 }}
              />

              {/* File Attachments */}
              <Box sx={{ mb: 3 }}>
                <Typography variant="body2" color="text.secondary" mb={1}>
                  Attachments (optional — max 10 MB per file)
                </Typography>
                <Button
                  variant="outlined"
                  startIcon={<AttachFile />}
                  size="small"
                  onClick={() => fileInputRef.current?.click()}
                >
                  Add Files
                </Button>
                <input
                  ref={fileInputRef}
                  type="file"
                  hidden
                  multiple
                  accept=".pdf,.png,.jpg,.jpeg,.doc,.docx"
                  onChange={handleFileChange}
                />
                {files.length > 0 && (
                  <List dense sx={{ mt: 1 }}>
                    {files.map((file, idx) => (
                      <ListItem
                        key={idx}
                        secondaryAction={
                          <MuiIconButton edge="end" size="small" onClick={() => removeFile(idx)}>
                            <Close fontSize="small" />
                          </MuiIconButton>
                        }
                        sx={{ bgcolor: 'grey.100', borderRadius: 1, mb: 0.5 }}
                      >
                        <ListItemText
                          primary={file.name}
                          secondary={`${(file.size / 1024).toFixed(1)} KB`}
                        />
                      </ListItem>
                    ))}
                  </List>
                )}
              </Box>

              <Box sx={{ display: 'flex', gap: 2 }}>
                <Button
                  type="submit"
                  variant="contained"
                  disabled={formik.isSubmitting}
                  sx={{ bgcolor: brandColor }}
                >
                  {formik.isSubmitting ? <CircularProgress size={20} color="inherit" /> : 'Submit Ticket'}
                </Button>
                <Button variant="outlined" component={Link} to="/portal/tickets">
                  Cancel
                </Button>
              </Box>
            </Box>
          </CardContent>
        </Card>
      </Box>
    </Box>
  );
};

export default PortalNewTicketPage;

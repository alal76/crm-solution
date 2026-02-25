// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
import React, { useCallback, useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  IconButton,
  Paper,
  Snackbar,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Toolbar,
  Tooltip,
  Typography,
} from '@mui/material';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import RefreshIcon from '@mui/icons-material/Refresh';
import apiClient from '../services/apiClient';

// ─── Types ────────────────────────────────────────────────────────────────────

interface WebToLeadForm {
  id: number;
  name: string;
  isActive: boolean;
  embedKey: string;
  submissionCount?: number;
  createdAt: string;
}

// ─── Component ────────────────────────────────────────────────────────────────

/**
 * WebToLeadFormsPage — lists web-to-lead forms and lets users copy embed codes.
 * Route: /leads/web-forms  (TODO-CRM002-04)
 */
const WebToLeadFormsPage: React.FC = () => {
  const [forms, setForms] = useState<WebToLeadForm[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string }>({
    open: false,
    message: '',
  });

  // ─── Data fetching ────────────────────────────────────────────────────────

  const fetchForms = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await apiClient.get<WebToLeadForm[]>('/web-to-lead-forms');
      setForms(response.data);
    } catch {
      setError('Failed to load web-to-lead forms. Please try again.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void fetchForms();
  }, [fetchForms]);

  // ─── Embed code helper ────────────────────────────────────────────────────

  const buildEmbedCode = (form: WebToLeadForm): string => {
    const baseUrl = window.location.origin;
    return `<iframe src="${baseUrl}/forms/${form.embedKey}" width="100%" height="600" frameborder="0" title="${form.name}" allow="clipboard-write"></iframe>`;
  };

  const handleCopyEmbed = async (form: WebToLeadForm) => {
    try {
      await navigator.clipboard.writeText(buildEmbedCode(form));
      setSnackbar({ open: true, message: `Embed code copied for "${form.name}"!` });
    } catch {
      setSnackbar({ open: true, message: 'Failed to copy — please copy manually.' });
    }
  };

  // ─── Render ───────────────────────────────────────────────────────────────

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Toolbar disableGutters sx={{ mb: 2, gap: 2 }}>
        <Typography variant="h5" component="h1" sx={{ flexGrow: 1, fontWeight: 600 }}>
          Web-to-Lead Forms
        </Typography>
        <Tooltip title="Refresh">
          <IconButton onClick={() => void fetchForms()} size="small">
            <RefreshIcon />
          </IconButton>
        </Tooltip>
        <Button variant="contained" size="small" disabled>
          + New Form
        </Button>
      </Toolbar>

      {/* Error banner */}
      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Table */}
      <Paper variant="outlined">
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
            <CircularProgress />
          </Box>
        ) : forms.length === 0 ? (
          <Box sx={{ py: 6, textAlign: 'center' }}>
            <Typography color="text.secondary">
              No web-to-lead forms found. Create your first form to embed lead capture on your website.
            </Typography>
          </Box>
        ) : (
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Submissions</TableCell>
                <TableCell>Embed Key</TableCell>
                <TableCell>Created</TableCell>
                <TableCell align="center">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {forms.map((form) => (
                <TableRow key={form.id} hover>
                  <TableCell>
                    <Typography variant="body2" fontWeight={500}>
                      {form.name}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={form.isActive ? 'Active' : 'Inactive'}
                      color={form.isActive ? 'success' : 'default'}
                      size="small"
                    />
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" color="text.secondary">
                      {form.submissionCount ?? 0}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography
                      variant="caption"
                      fontFamily="monospace"
                      color="text.secondary"
                      sx={{ userSelect: 'all' }}
                    >
                      {form.embedKey}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" color="text.secondary">
                      {new Date(form.createdAt).toLocaleDateString()}
                    </Typography>
                  </TableCell>
                  <TableCell align="center">
                    <Tooltip title="Copy embed code">
                      <IconButton
                        size="small"
                        onClick={() => void handleCopyEmbed(form)}
                        aria-label={`Copy embed code for ${form.name}`}
                      >
                        <ContentCopyIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </Paper>

      {/* Snackbar feedback */}
      <Snackbar
        open={snackbar.open}
        autoHideDuration={3500}
        onClose={() => setSnackbar((s) => ({ ...s, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert severity="success" onClose={() => setSnackbar((s) => ({ ...s, open: false }))}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default WebToLeadFormsPage;

/**
 * MKT-002: Email Sequences Page
 * Lists all email sequences with CRUD. Opens EmailTemplateBuilder in a dialog
 * to edit the HTML body for each sequence's email step.
 */

import { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Alert,
  CircularProgress,
  Stack,
  Tooltip,
  Container,
  Divider,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Email as EmailIcon,
  PlayArrow as ActiveIcon,
  Pause as InactiveIcon,
  Close as CloseIcon,
} from '@mui/icons-material';
import EmailTemplateBuilder from '../../components/marketing/EmailTemplateBuilder';
import marketingService from '../../services/marketingService';
import { EmailSequenceDto } from '../../types/marketing';

// ─── Local types ─────────────────────────────────────────────────────────────

/** Extended form data for edit/create dialog */
interface SequenceFormData {
  name: string;
  description: string;
  isActive: boolean;
  /** Step 1 email subject (stored locally, not in DTO) */
  subject: string;
  /** Step 1 HTML body (stored locally, not in DTO) */
  bodyHtml: string;
}

const emptyForm = (): SequenceFormData => ({
  name: '',
  description: '',
  isActive: true,
  subject: '',
  bodyHtml: '',
});

// ─── Component ───────────────────────────────────────────────────────────────

export default function EmailSequencesPage() {
  const [sequences, setSequences] = useState<EmailSequenceDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  // ── Dialog state ────────────────────────────────────────────────────────────
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editId, setEditId] = useState<number | null>(null);
  const [formData, setFormData] = useState<SequenceFormData>(emptyForm());
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  // ── Delete confirm ──────────────────────────────────────────────────────────
  const [deleteTarget, setDeleteTarget] = useState<EmailSequenceDto | null>(null);
  const [deleting, setDeleting] = useState(false);

  // ── Load sequences ──────────────────────────────────────────────────────────
  const loadSequences = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await marketingService.getEmailSequenceDtos();
      setSequences(Array.isArray(data) ? data : []);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load email sequences.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadSequences();
  }, [loadSequences]);

  // ── Open create dialog ──────────────────────────────────────────────────────
  const openCreate = () => {
    setEditId(null);
    setFormData(emptyForm());
    setFormError(null);
    setDialogOpen(true);
  };

  // ── Open edit dialog ────────────────────────────────────────────────────────
  const openEdit = (seq: EmailSequenceDto) => {
    setEditId(seq.id);
    setFormData({
      name: seq.name,
      description: seq.description ?? '',
      isActive: seq.isActive,
      subject: '',
      bodyHtml: '',
    });
    setFormError(null);
    setDialogOpen(true);
  };

  // ── Save ────────────────────────────────────────────────────────────────────
  const handleSave = async () => {
    if (!formData.name.trim()) {
      setFormError('Name is required.');
      return;
    }
    setSaving(true);
    setFormError(null);
    try {
      if (editId !== null) {
        await marketingService.updateEmailSequenceSimp(editId, {
          name: formData.name,
          description: formData.description,
          isActive: formData.isActive,
        });
        setSuccessMsg('Sequence updated.');
      } else {
        await marketingService.createEmailSequenceSimp({
          name: formData.name,
          description: formData.description,
        });
        setSuccessMsg('Sequence created.');
      }
      setDialogOpen(false);
      void loadSequences();
    } catch (err: unknown) {
      setFormError(err instanceof Error ? err.message : 'Failed to save sequence.');
    } finally {
      setSaving(false);
    }
  };

  // ── Delete ──────────────────────────────────────────────────────────────────
  const handleDelete = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await marketingService.deleteEmailSequenceById(deleteTarget.id);
      setSuccessMsg('Sequence deleted.');
      setDeleteTarget(null);
      void loadSequences();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to delete sequence.');
    } finally {
      setDeleting(false);
    }
  };

  // ─────────────────────────────────────────────────────────────────────────────

  return (
    <Container maxWidth="xl" sx={{ py: 3 }}>
      {/* Header */}
      <Stack direction="row" alignItems="center" justifyContent="space-between" mb={3}>
        <Box>
          <Typography variant="h5" fontWeight={600} gutterBottom>
            Email Sequences
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Manage drip-email sequences and edit step content.
          </Typography>
        </Box>
        <Button variant="contained" startIcon={<AddIcon />} onClick={openCreate}>
          New Sequence
        </Button>
      </Stack>

      {/* Alerts */}
      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
      {successMsg && (
        <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccessMsg(null)}>
          {successMsg}
        </Alert>
      )}

      {/* Table */}
      <Card>
        <CardContent sx={{ p: 0, '&:last-child': { pb: 0 } }}>
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
              <CircularProgress />
            </Box>
          ) : sequences.length === 0 ? (
            <Box sx={{ py: 8, textAlign: 'center' }}>
              <EmailIcon sx={{ fontSize: 48, color: 'text.disabled', mb: 1 }} />
              <Typography color="text.secondary">No email sequences found.</Typography>
              <Button variant="outlined" startIcon={<AddIcon />} sx={{ mt: 2 }} onClick={openCreate}>
                Create your first sequence
              </Button>
            </Box>
          ) : (
            <TableContainer component={Paper} elevation={0}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Name</TableCell>
                    <TableCell>Description</TableCell>
                    <TableCell align="center">Steps</TableCell>
                    <TableCell align="center">Active Enrollments</TableCell>
                    <TableCell align="center">Status</TableCell>
                    <TableCell>Created</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {sequences.map((seq) => (
                    <TableRow key={seq.id} hover>
                      <TableCell>
                        <Typography variant="body2" fontWeight={500}>
                          {seq.name}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" color="text.secondary" noWrap sx={{ maxWidth: 240 }}>
                          {seq.description || '—'}
                        </Typography>
                      </TableCell>
                      <TableCell align="center">
                        <Chip label={seq.stepCount} size="small" variant="outlined" />
                      </TableCell>
                      <TableCell align="center">
                        <Chip
                          label={seq.activeEnrollmentCount}
                          size="small"
                          color={seq.activeEnrollmentCount > 0 ? 'primary' : 'default'}
                          variant={seq.activeEnrollmentCount > 0 ? 'filled' : 'outlined'}
                        />
                      </TableCell>
                      <TableCell align="center">
                        <Chip
                          icon={seq.isActive ? <ActiveIcon fontSize="small" /> : <InactiveIcon fontSize="small" />}
                          label={seq.isActive ? 'Active' : 'Inactive'}
                          size="small"
                          color={seq.isActive ? 'success' : 'default'}
                          variant="filled"
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant="caption" color="text.secondary">
                          {seq.createdAt ? new Date(seq.createdAt).toLocaleDateString() : '—'}
                        </Typography>
                      </TableCell>
                      <TableCell align="right">
                        <Stack direction="row" spacing={0.5} justifyContent="flex-end">
                          <Tooltip title="Edit sequence">
                            <IconButton size="small" onClick={() => openEdit(seq)}>
                              <EditIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete sequence">
                            <IconButton
                              size="small"
                              color="error"
                              onClick={() => setDeleteTarget(seq)}
                            >
                              <DeleteIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        </Stack>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </CardContent>
      </Card>

      {/* ── Create / Edit dialog ───────────────────────────────────────────── */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          <Stack direction="row" alignItems="center" justifyContent="space-between">
            <Typography variant="h6">{editId ? 'Edit Sequence' : 'New Email Sequence'}</Typography>
            <IconButton size="small" onClick={() => setDialogOpen(false)}>
              <CloseIcon />
            </IconButton>
          </Stack>
        </DialogTitle>

        <DialogContent dividers>
          <Stack spacing={2}>
            {formError && <Alert severity="error" onClose={() => setFormError(null)}>{formError}</Alert>}

            <TextField
              label="Sequence Name *"
              value={formData.name}
              onChange={(e) => setFormData((prev) => ({ ...prev, name: e.target.value }))}
              fullWidth
              size="small"
              placeholder="e.g. Welcome Series"
            />

            <TextField
              label="Description"
              value={formData.description}
              onChange={(e) => setFormData((prev) => ({ ...prev, description: e.target.value }))}
              fullWidth
              size="small"
              multiline
              rows={2}
              placeholder="Optional description…"
            />

            <Divider sx={{ my: 1 }}>
              <Typography variant="caption" color="text.secondary">
                Step 1 — Email Content (optional)
              </Typography>
            </Divider>

            <EmailTemplateBuilder
              subject={formData.subject}
              onSubjectChange={(s) => setFormData((prev) => ({ ...prev, subject: s }))}
              value={formData.bodyHtml}
              onChange={(html) => setFormData((prev) => ({ ...prev, bodyHtml: html }))}
            />
          </Stack>
        </DialogContent>

        <DialogActions sx={{ px: 3, py: 2 }}>
          <Button onClick={() => setDialogOpen(false)} variant="outlined" disabled={saving}>
            Cancel
          </Button>
          <Button
            onClick={handleSave}
            variant="contained"
            disabled={saving}
            startIcon={saving ? <CircularProgress size={16} /> : undefined}
          >
            {editId ? 'Save Changes' : 'Create Sequence'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* ── Delete confirm dialog ──────────────────────────────────────────── */}
      <Dialog open={!!deleteTarget} onClose={() => setDeleteTarget(null)} maxWidth="xs" fullWidth>
        <DialogTitle>Delete Sequence?</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete{' '}
            <strong>&ldquo;{deleteTarget?.name}&rdquo;</strong>? This cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setDeleteTarget(null)} disabled={deleting}>
            Cancel
          </Button>
          <Button
            color="error"
            variant="contained"
            onClick={handleDelete}
            disabled={deleting}
            startIcon={deleting ? <CircularProgress size={16} /> : <DeleteIcon />}
          >
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

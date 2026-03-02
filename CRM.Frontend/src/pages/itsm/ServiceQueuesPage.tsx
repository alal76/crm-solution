import React, { useState, useEffect, useCallback } from 'react';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Drawer,
  FormControlLabel,
  Grid,
  IconButton,
  List,
  ListItem,
  ListItemText,
  Paper,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import VisibilityIcon from '@mui/icons-material/Visibility';
import CloseIcon from '@mui/icons-material/Close';
import serviceQueueService, {
  ServiceQueueDto,
  CreateServiceQueueDto,
  ServiceRequestQueueItemDto,
} from '../../services/serviceQueueService';
import { usePagination } from '../../hooks/usePagination';

const DEFAULT_FORM: CreateServiceQueueDto = {
  name: '',
  description: '',
  priority: 5,
  isActive: true,
  assignmentGroup: '',
  maxQueueDepth: undefined,
};

const ServiceQueuesPage: React.FC = () => {
  const [queues, setQueues] = useState<ServiceQueueDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Dialog state
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingQueue, setEditingQueue] = useState<ServiceQueueDto | null>(null);
  const [formData, setFormData] = useState<CreateServiceQueueDto>(DEFAULT_FORM);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  // Delete state
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deletingQueue, setDeletingQueue] = useState<ServiceQueueDto | null>(null);
  const [deleting, setDeleting] = useState(false);

  // Queue detail drawer
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [selectedQueue, setSelectedQueue] = useState<ServiceQueueDto | null>(null);
  const [queueItems, setQueueItems] = useState<ServiceRequestQueueItemDto[]>([]);
  const [loadingItems, setLoadingItems] = useState(false);

  const loadQueues = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await serviceQueueService.getAll();
      setQueues(data.items ?? []);
    } catch (err) {
      console.error('Failed to load service queues', err);
      setError('Failed to load service queues. Please try again.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadQueues();
  }, [loadQueues]);

  const handleOpenCreate = () => {
    setEditingQueue(null);
    setFormData(DEFAULT_FORM);
    setFormError(null);
    setDialogOpen(true);
  };

  const handleOpenEdit = (queue: ServiceQueueDto) => {
    setEditingQueue(queue);
    setFormData({
      name: queue.name,
      description: queue.description ?? '',
      priority: queue.priority,
      isActive: queue.isActive,
      assignmentGroup: queue.assignmentGroup ?? '',
      maxQueueDepth: queue.maxQueueDepth,
    });
    setFormError(null);
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setEditingQueue(null);
    setFormData(DEFAULT_FORM);
    setFormError(null);
  };

  const handleSave = async () => {
    if (!formData.name.trim()) {
      setFormError('Name is required.');
      return;
    }
    if ((formData.priority ?? 5) < 1 || (formData.priority ?? 5) > 10) {
      setFormError('Priority must be between 1 and 10.');
      return;
    }
    setSaving(true);
    setFormError(null);
    try {
      if (editingQueue) {
        await serviceQueueService.update(editingQueue.id, formData);
      } else {
        await serviceQueueService.create(formData);
      }
      handleCloseDialog();
      await loadQueues();
    } catch (err) {
      console.error('Failed to save service queue', err);
      setFormError('Failed to save. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  const handleOpenDelete = (queue: ServiceQueueDto) => {
    setDeletingQueue(queue);
    setDeleteDialogOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!deletingQueue) return;
    setDeleting(true);
    try {
      await serviceQueueService.delete(deletingQueue.id);
      setDeleteDialogOpen(false);
      setDeletingQueue(null);
      await loadQueues();
    } catch (err) {
      console.error('Failed to delete service queue', err);
    } finally {
      setDeleting(false);
    }
  };

  const handleViewQueue = async (queue: ServiceQueueDto) => {
    setSelectedQueue(queue);
    setDrawerOpen(true);
    setLoadingItems(true);
    try {
      const items = await serviceQueueService.getQueueItems(queue.id);
      setQueueItems(items);
    } catch (err) {
      console.error('Failed to load queue items', err);
      setQueueItems([]);
    } finally {
      setLoadingItems(false);
    }
  };

  const handleCloseDrawer = () => {
    setDrawerOpen(false);
    setSelectedQueue(null);
    setQueueItems([]);
  };

  const activeCount = queues.filter(q => q.isActive).length;

  const stats = [
    { label: 'Total Queues', value: queues.length, color: 'primary.main' },
    { label: 'Active Queues', value: activeCount, color: 'success.main' },
    { label: 'Total Items', value: 0, color: 'info.main' },
  ];

  const priorityColor = (priority: number): 'error' | 'warning' | 'info' | 'default' => {
    if (priority <= 2) return 'error';
    if (priority <= 4) return 'warning';
    if (priority <= 7) return 'info';
    return 'default';
  };

  const { paginatedData: paginatedQueues, page, pageSize, handlePageChange, handlePageSizeChange, pageSizeOptions } =
    usePagination(queues, { defaultPageSize: 25 });

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h4" component="h1" fontWeight="bold">
            Service Queues
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
            Manage service request queues and assignment groups
          </Typography>
        </Box>
        <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenCreate}>
          New Queue
        </Button>
      </Box>

      {/* Stats Cards */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        {stats.map((stat) => (
          <Grid item xs={12} sm={4} key={stat.label}>
            <Card variant="outlined">
              <CardContent sx={{ textAlign: 'center', py: 2 }}>
                <Typography variant="h4" fontWeight="bold" sx={{ color: stat.color }}>
                  {stat.value}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {stat.label}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      {/* Error */}
      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Table */}
      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
          <CircularProgress />
        </Box>
      ) : (
        <>
        <TableContainer component={Paper} variant="outlined">
          <Table>
            <TableHead>
              <TableRow sx={{ bgcolor: 'grey.50' }}>
                <TableCell><strong>Name</strong></TableCell>
                <TableCell><strong>Priority</strong></TableCell>
                <TableCell><strong>Active</strong></TableCell>
                <TableCell><strong>Assignment Group</strong></TableCell>
                <TableCell><strong>Default SLA</strong></TableCell>
                <TableCell><strong>Max Depth</strong></TableCell>
                <TableCell align="right"><strong>Actions</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {queues.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} align="center" sx={{ py: 4 }}>
                    <Typography color="text.secondary">
                      No service queues found. Create one to get started.
                    </Typography>
                  </TableCell>
                </TableRow>
              ) : (
                paginatedQueues.map((queue) => (
                  <TableRow key={queue.id} hover>
                    <TableCell>
                      <Typography fontWeight={500} color="primary.main">
                        {queue.name}
                      </Typography>
                      {queue.description && (
                        <Typography variant="caption" color="text.secondary" display="block">
                          {queue.description}
                        </Typography>
                      )}
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={`P${queue.priority}`}
                        color={priorityColor(queue.priority)}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={queue.isActive ? 'Active' : 'Inactive'}
                        color={queue.isActive ? 'success' : 'default'}
                        size="small"
                        variant="outlined"
                      />
                    </TableCell>
                    <TableCell>{queue.assignmentGroup ?? '—'}</TableCell>
                    <TableCell>{queue.defaultSLAPolicyId ? `Policy #${queue.defaultSLAPolicyId}` : '—'}</TableCell>
                    <TableCell>{queue.maxQueueDepth ?? '—'}</TableCell>
                    <TableCell align="right">
                      <Tooltip title="View Items">
                        <IconButton size="small" onClick={() => handleViewQueue(queue)} color="info">
                          <VisibilityIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Edit">
                        <IconButton size="small" onClick={() => handleOpenEdit(queue)}>
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Delete">
                        <IconButton size="small" onClick={() => handleOpenDelete(queue)} color="error">
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>
        <TablePagination
          component="div"
          count={queues.length}
          page={page}
          onPageChange={handlePageChange}
          rowsPerPage={pageSize}
          onRowsPerPageChange={handlePageSizeChange}
          rowsPerPageOptions={pageSizeOptions}
        />
        </>
      )}

      {/* Create / Edit Dialog */}
      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{editingQueue ? 'Edit Service Queue' : 'Create Service Queue'}</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          {formError && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {formError}
            </Alert>
          )}

          <TextField
            label="Name"
            value={formData.name}
            onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
            fullWidth
            required
            sx={{ mb: 2, mt: 1 }}
          />

          <TextField
            label="Description"
            value={formData.description ?? ''}
            onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
            fullWidth
            multiline
            rows={2}
            sx={{ mb: 2 }}
          />

          <Grid container spacing={2} sx={{ mb: 2 }}>
            <Grid item xs={6}>
              <TextField
                label="Priority (1-10)"
                type="number"
                value={formData.priority ?? 5}
                onChange={(e) => setFormData(prev => ({ ...prev, priority: Number.parseInt(e.target.value, 10) || 5 }))}
                fullWidth
                inputProps={{ min: 1, max: 10 }}
                helperText="1 = highest, 10 = lowest"
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                label="Max Queue Depth"
                type="number"
                value={formData.maxQueueDepth ?? ''}
                onChange={(e) => {
                  const val = e.target.value ? Number.parseInt(e.target.value, 10) : undefined;
                  setFormData(prev => ({ ...prev, maxQueueDepth: val }));
                }}
                fullWidth
                inputProps={{ min: 1 }}
                helperText="Optional maximum items"
              />
            </Grid>
          </Grid>

          <TextField
            label="Assignment Group"
            value={formData.assignmentGroup ?? ''}
            onChange={(e) => setFormData(prev => ({ ...prev, assignmentGroup: e.target.value }))}
            fullWidth
            sx={{ mb: 2 }}
            helperText="Team or group responsible for this queue"
          />

          <FormControlLabel
            control={
              <Switch
                checked={formData.isActive ?? true}
                onChange={(e) => setFormData(prev => ({ ...prev, isActive: e.target.checked }))}
              />
            }
            label="Active"
          />
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={handleCloseDialog} disabled={saving}>Cancel</Button>
          <Button variant="contained" onClick={handleSave} disabled={saving}>
            {saving ? <CircularProgress size={20} /> : (editingQueue ? 'Save Changes' : 'Create Queue')}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Delete Service Queue</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete <strong>{deletingQueue?.name}</strong>? This action cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setDeleteDialogOpen(false)} disabled={deleting}>Cancel</Button>
          <Button variant="contained" color="error" onClick={handleConfirmDelete} disabled={deleting}>
            {deleting ? <CircularProgress size={20} /> : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Queue Items Drawer */}
      <Drawer
        anchor="right"
        open={drawerOpen}
        onClose={handleCloseDrawer}
        PaperProps={{ sx: { width: { xs: '100%', sm: 450 } } }}
      >
        <Box sx={{ p: 3 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Box>
              <Typography variant="h6" fontWeight="bold">
                {selectedQueue?.name}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Queue Items
              </Typography>
            </Box>
            <IconButton onClick={handleCloseDrawer}>
              <CloseIcon />
            </IconButton>
          </Box>

          {selectedQueue?.description && (
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              {selectedQueue.description}
            </Typography>
          )}

          <Box sx={{ display: 'flex', gap: 1, mb: 2 }}>
            <Chip label={`Priority: P${selectedQueue?.priority ?? 0}`} size="small" />
            <Chip
              label={selectedQueue?.isActive ? 'Active' : 'Inactive'}
              color={selectedQueue?.isActive ? 'success' : 'default'}
              size="small"
              variant="outlined"
            />
            {selectedQueue?.assignmentGroup && (
              <Chip label={selectedQueue.assignmentGroup} size="small" variant="outlined" />
            )}
          </Box>

          {loadingItems ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
              <CircularProgress />
            </Box>
          ) : queueItems.length === 0 ? (
            <Typography color="text.secondary" sx={{ py: 4, textAlign: 'center' }}>
              No items in this queue.
            </Typography>
          ) : (
            <List disablePadding>
              {queueItems.map((item) => (
                <ListItem key={item.id} divider sx={{ px: 0 }}>
                  <ListItemText
                    primary={
                      <Typography fontWeight={500}>
                        #{item.id} — {item.title}
                      </Typography>
                    }
                    secondary={
                      <Box sx={{ display: 'flex', gap: 1, mt: 0.5 }}>
                        <Chip label={item.priority} size="small" />
                        <Chip label={item.status} size="small" variant="outlined" />
                        {item.assignedTo && (
                          <Typography variant="caption" color="text.secondary" sx={{ alignSelf: 'center' }}>
                            → {item.assignedTo}
                          </Typography>
                        )}
                      </Box>
                    }
                  />
                </ListItem>
              ))}
            </List>
          )}
        </Box>
      </Drawer>
    </Box>
  );
};

export default ServiceQueuesPage;

import { useState, useEffect, useMemo } from 'react';
import {
  Box,
  Typography,
  Button,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TablePagination,
  Chip,
  CircularProgress,
  Alert,
  IconButton,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Paper,
  Stack,
  Switch,
  FormControlLabel,
  SelectChangeEvent,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Code as CodeIcon,
  ToggleOn as ToggleOnIcon,
  ToggleOff as ToggleOffIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import {
  getScriptPlugins,
  deleteScriptPlugin,
  updateScriptPlugin,
  ScriptPluginDto,
  ScriptLanguage,
} from '../services/scriptPluginService';
import logger from '../services/logger';

// ─── Language badge config ────────────────────────────────────────────────

const LANGUAGE_LABELS: Record<number, { label: string; color: 'primary' | 'success' | 'secondary' }> = {
  0: { label: 'JavaScript', color: 'primary' },
  1: { label: 'Python', color: 'success' },
  2: { label: 'C#', color: 'secondary' },
};

const LANGUAGE_OPTIONS: { value: number; label: string }[] = [
  { value: -1, label: 'All Languages' },
  { value: 0, label: 'JavaScript' },
  { value: 1, label: 'Python' },
  { value: 2, label: 'C#' },
];

// ─── Component ────────────────────────────────────────────────────────────

const ScriptPluginLibraryPage = () => {
  const navigate = useNavigate();

  // Data
  const [plugins, setPlugins] = useState<ScriptPluginDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Filters
  const [searchText, setSearchText] = useState('');
  const [languageFilter, setLanguageFilter] = useState<number>(-1);
  const [showInactive, setShowInactive] = useState(false);

  // Pagination
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);

  // Delete dialog
  const [deleteTarget, setDeleteTarget] = useState<ScriptPluginDto | null>(null);
  const [deleting, setDeleting] = useState(false);

  // Toggle active in-progress ids
  const [togglingIds, setTogglingIds] = useState<Set<number>>(new Set());

  // ── Load ────────────────────────────────────────────────────────────────

  const loadPlugins = async (includeInactive: boolean) => {
    setLoading(true);
    setError(null);
    try {
      const data = await getScriptPlugins(includeInactive);
      setPlugins(data);
    } catch (err) {
      logger.error('ScriptPluginLibraryPage: failed to load plugins', err);
      setError('Failed to load script plugins. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadPlugins(false);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Reload when showInactive changes
  useEffect(() => {
    loadPlugins(showInactive);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [showInactive]);

  // ── Filtered data ───────────────────────────────────────────────────────

  const filtered = useMemo(() => {
    return plugins.filter((p) => {
      const matchesSearch =
        searchText.trim() === '' ||
        p.name.toLowerCase().includes(searchText.toLowerCase()) ||
        (p.description ?? '').toLowerCase().includes(searchText.toLowerCase());
      const matchesLang = languageFilter === -1 || p.language === languageFilter;
      return matchesSearch && matchesLang;
    });
  }, [plugins, searchText, languageFilter]);

  const paginated = useMemo(
    () => filtered.slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage),
    [filtered, page, rowsPerPage],
  );

  // ── Delete ──────────────────────────────────────────────────────────────

  const handleDeleteConfirm = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await deleteScriptPlugin(deleteTarget.id);
      setPlugins((prev) => prev.filter((p) => p.id !== deleteTarget.id));
      setDeleteTarget(null);
    } catch (err) {
      logger.error('ScriptPluginLibraryPage: delete failed', err);
      setError('Failed to delete plugin.');
    } finally {
      setDeleting(false);
    }
  };

  // ── Toggle active ────────────────────────────────────────────────────────

  const handleToggleActive = async (plugin: ScriptPluginDto) => {
    setTogglingIds((prev) => new Set(prev).add(plugin.id));
    try {
      const updated = await updateScriptPlugin(plugin.id, {
        name: plugin.name,
        description: plugin.description ?? undefined,
        code: plugin.code,
        parameterSchema: plugin.parameterSchema ?? undefined,
        returnValueDescription: plugin.returnValueDescription ?? undefined,
        isActive: !plugin.isActive,
      });
      setPlugins((prev) => prev.map((p) => (p.id === plugin.id ? updated : p)));
    } catch (err) {
      logger.error('ScriptPluginLibraryPage: toggle active failed', err);
      setError('Failed to update plugin status.');
    } finally {
      setTogglingIds((prev) => {
        const next = new Set(prev);
        next.delete(plugin.id);
        return next;
      });
    }
  };

  // ── Handlers ─────────────────────────────────────────────────────────────

  const handleLanguageFilterChange = (e: SelectChangeEvent<number>) => {
    setLanguageFilter(e.target.value as number);
    setPage(0);
  };

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchText(e.target.value);
    setPage(0);
  };

  // ── Render ────────────────────────────────────────────────────────────────

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Stack direction="row" justifyContent="space-between" alignItems="flex-start" mb={3}>
        <Box>
          <Stack direction="row" alignItems="center" spacing={1} mb={0.5}>
            <CodeIcon color="primary" />
            <Typography variant="h5" fontWeight={600}>
              Script Plugin Library
            </Typography>
          </Stack>
          <Typography variant="body2" color="text.secondary">
            Manage reusable AI agent script plugins
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => navigate('/scripting/plugins/new')}
        >
          New Plugin
        </Button>
      </Stack>

      {/* Error */}
      {error && (
        <Alert severity="error" onClose={() => setError(null)} sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {/* Filter toolbar */}
      <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems="center">
          <TextField
            size="small"
            label="Search by name"
            value={searchText}
            onChange={handleSearchChange}
            sx={{ minWidth: 220 }}
          />
          <FormControl size="small" sx={{ minWidth: 160 }}>
            <InputLabel id="lang-filter-label">Language</InputLabel>
            <Select<number>
              labelId="lang-filter-label"
              label="Language"
              value={languageFilter}
              onChange={handleLanguageFilterChange}
            >
              {LANGUAGE_OPTIONS.map((opt) => (
                <MenuItem key={opt.value} value={opt.value}>
                  {opt.label}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          <FormControlLabel
            control={
              <Switch
                checked={showInactive}
                onChange={(e) => setShowInactive(e.target.checked)}
                size="small"
              />
            }
            label="Show inactive"
          />
          <Typography variant="body2" color="text.secondary" sx={{ ml: 'auto' }}>
            {filtered.length} plugin{filtered.length !== 1 ? 's' : ''}
          </Typography>
        </Stack>
      </Paper>

      {/* Table */}
      {loading ? (
        <Box display="flex" justifyContent="center" py={6}>
          <CircularProgress />
        </Box>
      ) : (
        <Paper variant="outlined">
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600 }}>Name</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Description</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Language</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Version</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Active</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Created</TableCell>
                <TableCell sx={{ fontWeight: 600 }} align="right">
                  Actions
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {paginated.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} align="center" sx={{ py: 4, color: 'text.secondary' }}>
                    {plugins.length === 0 ? 'No plugins found. Create your first plugin.' : 'No plugins match the current filters.'}
                  </TableCell>
                </TableRow>
              ) : (
                paginated.map((plugin) => {
                  const lang = LANGUAGE_LABELS[plugin.language] ?? { label: 'Unknown', color: 'default' as const };
                  const isToggling = togglingIds.has(plugin.id);
                  return (
                    <TableRow key={plugin.id} hover>
                      <TableCell>
                        <Typography variant="body2" fontWeight={500}>
                          {plugin.name}
                        </Typography>
                      </TableCell>
                      <TableCell sx={{ maxWidth: 260 }}>
                        <Tooltip title={plugin.description ?? ''} placement="top">
                          <Typography
                            variant="body2"
                            color="text.secondary"
                            sx={{
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                              whiteSpace: 'nowrap',
                              maxWidth: 260,
                            }}
                          >
                            {plugin.description ?? '—'}
                          </Typography>
                        </Tooltip>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={lang.label}
                          color={lang.color as 'primary' | 'success' | 'secondary'}
                          size="small"
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" color="text.secondary">
                          v{plugin.version}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={plugin.isActive ? 'Active' : 'Inactive'}
                          color={plugin.isActive ? 'success' : 'default'}
                          size="small"
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" color="text.secondary">
                          {plugin.createdAt
                            ? new Date(plugin.createdAt).toLocaleDateString()
                            : '—'}
                        </Typography>
                      </TableCell>
                      <TableCell align="right">
                        <Stack direction="row" spacing={0.5} justifyContent="flex-end">
                          <Tooltip title={plugin.isActive ? 'Deactivate' : 'Activate'}>
                            <span>
                              <IconButton
                                size="small"
                                disabled={isToggling}
                                onClick={() => handleToggleActive(plugin)}
                                color={plugin.isActive ? 'success' : 'default'}
                              >
                                {isToggling ? (
                                  <CircularProgress size={16} />
                                ) : plugin.isActive ? (
                                  <ToggleOnIcon fontSize="small" />
                                ) : (
                                  <ToggleOffIcon fontSize="small" />
                                )}
                              </IconButton>
                            </span>
                          </Tooltip>
                          <Tooltip title="Edit plugin">
                            <IconButton
                              size="small"
                              color="primary"
                              onClick={() => navigate(`/scripting/plugins/${plugin.id}/edit`)}
                            >
                              <EditIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete plugin">
                            <IconButton
                              size="small"
                              color="error"
                              onClick={() => setDeleteTarget(plugin)}
                            >
                              <DeleteIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        </Stack>
                      </TableCell>
                    </TableRow>
                  );
                })
              )}
            </TableBody>
          </Table>
          <TablePagination
            component="div"
            count={filtered.length}
            page={page}
            rowsPerPage={rowsPerPage}
            onPageChange={(_, newPage) => setPage(newPage)}
            onRowsPerPageChange={(e) => {
              setRowsPerPage(Number.parseInt(e.target.value, 10));
              setPage(0);
            }}
            rowsPerPageOptions={[5, 10, 25, 50]}
          />
        </Paper>
      )}

      {/* Delete Confirm Dialog */}
      <Dialog open={!!deleteTarget} onClose={() => setDeleteTarget(null)} maxWidth="xs" fullWidth>
        <DialogTitle>Delete Plugin?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete <strong>{deleteTarget?.name}</strong>? This action cannot
            be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteTarget(null)} disabled={deleting}>
            Cancel
          </Button>
          <Button
            onClick={handleDeleteConfirm}
            color="error"
            variant="contained"
            disabled={deleting}
            startIcon={deleting ? <CircularProgress size={16} color="inherit" /> : <DeleteIcon />}
          >
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ScriptPluginLibraryPage;

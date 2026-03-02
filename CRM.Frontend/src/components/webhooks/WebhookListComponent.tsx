/**
 * WebhookListComponent - Standalone reusable webhook list
 * TODO-INT001-21: Extract webhook list from WebhooksManagementPage
 */

import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  TablePagination,
  TableContainer,
  Paper,
  IconButton,
  Tooltip,
  Chip,
  Typography,
  CircularProgress,
  Alert,
  Stack,
  Switch,
  FormControlLabel,
  TextField,
  InputAdornment,
} from '@mui/material';
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  Visibility as ViewIcon,
  PlayArrow as TestIcon,
  Refresh as RefreshIcon,
  Search as SearchIcon,
} from '@mui/icons-material';
import webhookService, {
  Webhook,
  WebhookStatus,
} from '../../services/webhookService';

// --------------------------------------------------------------------------
// Types
// --------------------------------------------------------------------------

export interface WebhookListAction {
  type: 'view' | 'edit' | 'delete' | 'test';
  webhook: Webhook;
}

export interface WebhookListComponentProps {
  /** Pre-loaded webhooks — if provided the component won't fetch its own */
  webhooks?: Webhook[];
  /** Loading state (when webhooks are managed externally) */
  loading?: boolean;
  /** Error (when webhooks are managed externally) */
  error?: string | null;
  /** Called when an action button is clicked */
  onAction?: (action: WebhookListAction) => void;
  /** Called when toggle status is changed */
  onStatusToggle?: (webhook: Webhook, newActive: boolean) => void;
  /** Show the search bar */
  showSearch?: boolean;
  /** Show refresh button */
  showRefresh?: boolean;
  /** Page size options */
  pageSizeOptions?: number[];
  /** Dense display */
  dense?: boolean;
}

// --------------------------------------------------------------------------
// Helpers
// --------------------------------------------------------------------------

const statusLabel = (status: WebhookStatus): string => {
  return ['Active', 'Inactive', 'Paused', 'Disabled'][status] ?? 'Unknown';
};

const statusColor = (
  status: WebhookStatus,
): 'success' | 'default' | 'warning' | 'error' => {
  const map: Record<WebhookStatus, 'success' | 'default' | 'warning' | 'error'> = {
    [WebhookStatus.Active]: 'success',
    [WebhookStatus.Inactive]: 'default',
    [WebhookStatus.Paused]: 'warning',
    [WebhookStatus.Disabled]: 'error',
  };
  return map[status] ?? 'default';
};

// --------------------------------------------------------------------------
// Component
// --------------------------------------------------------------------------

export const WebhookListComponent: React.FC<WebhookListComponentProps> = ({
  webhooks: externalWebhooks,
  loading: externalLoading,
  error: externalError,
  onAction,
  onStatusToggle,
  showSearch = true,
  showRefresh = true,
  pageSizeOptions = [10, 25, 50],
  dense = false,
}) => {
  // Internal state for self-fetching mode
  const [internalWebhooks, setInternalWebhooks] = useState<Webhook[]>([]);
  const [internalLoading, setInternalLoading] = useState(false);
  const [internalError, setInternalError] = useState<string | null>(null);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(pageSizeOptions[0]);
  const [totalCount, setTotalCount] = useState(0);
  const [search, setSearch] = useState('');

  const isExternalMode = externalWebhooks !== undefined;
  const webhooks = isExternalMode ? externalWebhooks : internalWebhooks;
  const loading = isExternalMode ? (externalLoading ?? false) : internalLoading;
  const error = isExternalMode ? (externalError ?? null) : internalError;

  // Fetch webhooks when in self-managed mode
  const fetchWebhooks = useCallback(async () => {
    if (isExternalMode) return;
    setInternalLoading(true);
    setInternalError(null);
    try {
      const result = await webhookService.getWebhooks(page + 1, pageSize, {
        search: search || undefined,
      });
      setInternalWebhooks(result.items);
      setTotalCount(result.totalCount);
    } catch (err) {
      setInternalError('Failed to load webhooks');
    } finally {
      setInternalLoading(false);
    }
  }, [isExternalMode, page, pageSize, search]);

  useEffect(() => {
    fetchWebhooks();
  }, [fetchWebhooks]);

  return (
    <Paper>
      {/* Toolbar */}
      {(showSearch || showRefresh) && (
        <Stack
          direction="row"
          alignItems="center"
          spacing={1}
          sx={{ p: 1.5, borderBottom: 1, borderColor: 'divider' }}
        >
          {showSearch && (
            <TextField
              size="small"
              placeholder="Search webhooks…"
              value={search}
              onChange={(e) => {
                setSearch(e.target.value);
                setPage(0);
              }}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon fontSize="small" />
                  </InputAdornment>
                ),
              }}
              sx={{ minWidth: 240 }}
            />
          )}
          <Box flex={1} />
          {showRefresh && !isExternalMode && (
            <Tooltip title="Refresh">
              <IconButton size="small" onClick={fetchWebhooks} disabled={loading}>
                <RefreshIcon />
              </IconButton>
            </Tooltip>
          )}
        </Stack>
      )}

      {error && (
        <Alert severity="error" sx={{ m: 1 }}>
          {error}
        </Alert>
      )}

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : (
        <TableContainer>
          <Table size={dense ? 'small' : 'medium'} aria-label="Webhook list">
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>URL</TableCell>
                <TableCell>Events</TableCell>
                <TableCell>Status</TableCell>
                <TableCell align="right">Deliveries</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {webhooks.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} align="center">
                    <Typography variant="body2" color="text.secondary" sx={{ py: 3 }}>
                      No webhooks found
                    </Typography>
                  </TableCell>
                </TableRow>
              ) : (
                webhooks.map((wh) => (
                  <TableRow key={wh.id} hover>
                    <TableCell>
                      <Typography variant="body2" fontWeight={500}>
                        {wh.name}
                      </Typography>
                      {wh.description && (
                        <Typography variant="caption" color="text.secondary">
                          {wh.description}
                        </Typography>
                      )}
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" noWrap sx={{ maxWidth: 200 }}>
                        {wh.url}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Stack direction="row" spacing={0.5} flexWrap="wrap" useFlexGap>
                        {wh.events.slice(0, 3).map((ev) => (
                          <Chip key={ev} label={ev} size="small" variant="outlined" />
                        ))}
                        {wh.events.length > 3 && (
                          <Chip label={`+${wh.events.length - 3}`} size="small" />
                        )}
                      </Stack>
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={statusLabel(wh.status)}
                        size="small"
                        color={statusColor(wh.status)}
                      />
                    </TableCell>
                    <TableCell align="right">
                      <Typography variant="body2">
                        {wh.successfulDeliveries}/{wh.totalDeliveries}
                      </Typography>
                    </TableCell>
                    <TableCell align="right">
                      <Stack direction="row" spacing={0.5} justifyContent="flex-end">
                        <Tooltip title="View">
                          <IconButton
                            size="small"
                            onClick={() => onAction?.({ type: 'view', webhook: wh })}
                          >
                            <ViewIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Edit">
                          <IconButton
                            size="small"
                            onClick={() => onAction?.({ type: 'edit', webhook: wh })}
                          >
                            <EditIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Test">
                          <IconButton
                            size="small"
                            onClick={() => onAction?.({ type: 'test', webhook: wh })}
                          >
                            <TestIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton
                            size="small"
                            color="error"
                            onClick={() => onAction?.({ type: 'delete', webhook: wh })}
                          >
                            <DeleteIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </Stack>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {/* Pagination */}
      {!isExternalMode && (
        <TablePagination
          component="div"
          count={totalCount}
          page={page}
          onPageChange={(_, p) => setPage(p)}
          rowsPerPage={pageSize}
          onRowsPerPageChange={(e) => {
            setPageSize(Number.parseInt(e.target.value, 10));
            setPage(0);
          }}
          rowsPerPageOptions={pageSizeOptions}
        />
      )}
    </Paper>
  );
};

export default WebhookListComponent;

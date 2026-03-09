import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Paper,
  Typography,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  Chip,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
  CircularProgress,
  Switch,
  FormControlLabel,
  InputAdornment,
  Stack,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  Refresh as RefreshIcon,
  ContentCopy as CopyIcon,
  VpnKey as KeyIcon,
  Block as RevokeIcon,
  Visibility as ViewIcon,
  VisibilityOff as ViewOffIcon,
} from '@mui/icons-material';
import AdminPageHeader from '../../components/admin/AdminPageHeader';
import apiUserService, {
  ApiUserDto,
  CreateApiUserRequest,
  ApiKeyResponse,
} from '../../services/apiUserService';
import apiClient from '../../services/apiClient';

interface UserGroupOption {
  id: number;
  name: string;
  isApiGroup: boolean;
}

const ROLE_OPTIONS = [
  { value: 0, label: 'Admin' },
  { value: 1, label: 'Manager' },
  { value: 2, label: 'Sales' },
  { value: 3, label: 'Support' },
  { value: 4, label: 'Guest' },
];

const emptyForm: CreateApiUserRequest = {
  name: '',
  email: '',
  description: '',
  roleId: 4,
  primaryGroupId: null,
  expiresAt: null,
};

const ApiUsersPage: React.FC = () => {
  // Data state
  const [apiUsers, setApiUsers] = useState<ApiUserDto[]>([]);
  const [apiGroups, setApiGroups] = useState<UserGroupOption[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Dialog state
  const [createOpen, setCreateOpen] = useState(false);
  const [editUser, setEditUser] = useState<ApiUserDto | null>(null);
  const [formData, setFormData] = useState<CreateApiUserRequest>({ ...emptyForm });

  // API key display state
  const [keyResponse, setKeyResponse] = useState<ApiKeyResponse | null>(null);
  const [showKey, setShowKey] = useState(false);
  const [keyCopied, setKeyCopied] = useState(false);

  // Confirm dialog
  const [confirmAction, setConfirmAction] = useState<{
    title: string;
    message: string;
    onConfirm: () => Promise<void>;
  } | null>(null);

  const fetchApiUsers = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await apiUserService.getAll();
      setApiUsers(data);
    } catch (err: unknown) {
      const message = err instanceof Error ? (err as Error).message : 'Failed to load API users';
      setError(message);
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchApiGroups = useCallback(async () => {
    try {
      const response = await apiClient.get<{ items?: UserGroupOption[]; data?: UserGroupOption[] }>('/usergroups');
      const groups: UserGroupOption[] = response.data?.items || response.data?.data || (Array.isArray(response.data) ? response.data as unknown as UserGroupOption[] : []);
      setApiGroups(groups.filter((g) => g.isApiGroup));
    } catch {
      // Non-critical — just means no group selection available
      setApiGroups([]);
    }
  }, []);

  useEffect(() => {
    fetchApiUsers();
    fetchApiGroups();
  }, [fetchApiUsers, fetchApiGroups]);

  // ── Handlers ──

  const handleCreate = async () => {
    try {
      setError(null);
      const response = await apiUserService.create(formData);
      setKeyResponse(response);
      setShowKey(true);
      setCreateOpen(false);
      setFormData({ ...emptyForm });
      setSuccess('API user created successfully. Copy the API key — it will not be shown again.');
      fetchApiUsers();
    } catch (err: unknown) {
      const message = err instanceof Error ? (err as Error).message : 'Failed to create API user';
      setError(message);
    }
  };

  const handleUpdate = async () => {
    if (!editUser) return;
    try {
      setError(null);
      await apiUserService.update(editUser.id, formData);
      setEditUser(null);
      setFormData({ ...emptyForm });
      setSuccess('API user updated successfully');
      fetchApiUsers();
    } catch (err: unknown) {
      const message = err instanceof Error ? (err as Error).message : 'Failed to update API user';
      setError(message);
    }
  };

  const handleRegenerateKey = (user: ApiUserDto) => {
    setConfirmAction({
      title: 'Regenerate API Key',
      message: `This will invalidate the current API key for "${user.username}". The user will need to update their key. Continue?`,
      onConfirm: async () => {
        try {
          const response = await apiUserService.regenerateKey(user.id);
          setKeyResponse(response);
          setShowKey(true);
          setSuccess('API key regenerated. Copy the new key — it will not be shown again.');
          fetchApiUsers();
        } catch (err: unknown) {
          const message = err instanceof Error ? (err as Error).message : 'Failed to regenerate key';
          setError(message);
        }
      },
    });
  };

  const handleRevoke = (user: ApiUserDto) => {
    setConfirmAction({
      title: 'Revoke API Key',
      message: `This will revoke the API key and deactivate "${user.username}". The user will lose all API access. Continue?`,
      onConfirm: async () => {
        try {
          await apiUserService.revoke(user.id);
          setSuccess('API key revoked and user deactivated');
          fetchApiUsers();
        } catch (err: unknown) {
          const message = err instanceof Error ? (err as Error).message : 'Failed to revoke key';
          setError(message);
        }
      },
    });
  };

  const handleDelete = (user: ApiUserDto) => {
    setConfirmAction({
      title: 'Delete API User',
      message: `Permanently delete API user "${user.username}"? This cannot be undone.`,
      onConfirm: async () => {
        try {
          await apiUserService.delete(user.id);
          setSuccess('API user deleted');
          fetchApiUsers();
        } catch (err: unknown) {
          const message = err instanceof Error ? (err as Error).message : 'Failed to delete API user';
          setError(message);
        }
      },
    });
  };

  const handleToggleStatus = async (user: ApiUserDto) => {
    try {
      await apiUserService.toggleStatus(user.id);
      fetchApiUsers();
    } catch (err: unknown) {
      const message = err instanceof Error ? (err as Error).message : 'Failed to toggle status';
      setError(message);
    }
  };

  const handleCopyKey = async () => {
    if (keyResponse?.apiKey) {
      await navigator.clipboard.writeText(keyResponse.apiKey);
      setKeyCopied(true);
      setTimeout(() => setKeyCopied(false), 3000);
    }
  };

  const openEdit = (user: ApiUserDto) => {
    setFormData({
      name: `${user.firstName} ${user.lastName}`,
      email: user.email,
      description: user.apiUserDescription || '',
      roleId: ROLE_OPTIONS.find((r) => r.label === user.role)?.value ?? 4,
      primaryGroupId: user.primaryGroupId,
      expiresAt: user.apiKeyExpiresAt,
    });
    setEditUser(user);
  };

  const formatDate = (dateStr: string | null) => {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const isKeyExpired = (expiresAt: string | null) => {
    if (!expiresAt) return false;
    return new Date(expiresAt) < new Date();
  };

  // ── Form Dialog ──

  const renderFormDialog = (
    open: boolean,
    title: string,
    onSubmit: () => Promise<void>,
    onClose: () => void,
  ) => (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField
            label="Name"
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            fullWidth
            required
            placeholder="e.g. CI/CD Pipeline"
          />
          <TextField
            label="Email"
            type="email"
            value={formData.email}
            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            fullWidth
            required
            placeholder="e.g. api-pipeline@crm.local"
          />
          <TextField
            label="Description"
            value={formData.description}
            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            fullWidth
            multiline
            rows={2}
            placeholder="What is this API user for?"
          />
          <FormControl fullWidth>
            <InputLabel>Role</InputLabel>
            <Select
              value={formData.roleId}
              label="Role"
              onChange={(e) => setFormData({ ...formData, roleId: Number(e.target.value) })}
            >
              {ROLE_OPTIONS.map((r) => (
                <MenuItem key={r.value} value={r.value}>
                  {r.label}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          {apiGroups.length > 0 && (
            <FormControl fullWidth>
              <InputLabel>API Group (RBAC)</InputLabel>
              <Select
                value={formData.primaryGroupId ?? ''}
                label="API Group (RBAC)"
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    primaryGroupId: e.target.value ? Number(e.target.value) : null,
                  })
                }
              >
                <MenuItem value="">
                  <em>None</em>
                </MenuItem>
                {apiGroups.map((g) => (
                  <MenuItem key={g.id} value={g.id}>
                    {g.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          )}
          <TextField
            label="Key Expiration"
            type="datetime-local"
            value={formData.expiresAt ? formData.expiresAt.slice(0, 16) : ''}
            onChange={(e) =>
              setFormData({
                ...formData,
                expiresAt: e.target.value ? new Date(e.target.value).toISOString() : null,
              })
            }
            fullWidth
            InputLabelProps={{ shrink: true }}
            helperText="Leave empty for no expiration"
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          variant="contained"
          onClick={onSubmit}
          disabled={!formData.name || !formData.email}
        >
          {title.startsWith('Edit') ? 'Save' : 'Create'}
        </Button>
      </DialogActions>
    </Dialog>
  );

  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader title="API Users" subtitle="Manage API keys and service accounts for programmatic access" icon={KeyIcon} />

      {/* Alerts */}
      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}
      {success && (
        <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess(null)}>
          {success}
        </Alert>
      )}

      {/* API Key Display Banner */}
      {keyResponse && (
        <Alert
          severity="warning"
          sx={{ mb: 2 }}
          action={
            <Button color="inherit" size="small" onClick={() => setKeyResponse(null)}>
              Dismiss
            </Button>
          }
        >
          <Typography variant="subtitle2" gutterBottom>
            API Key for {keyResponse.username} — Copy now, it will not be shown again!
          </Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 1 }}>
            <TextField
              size="small"
              value={showKey ? keyResponse.apiKey : '••••••••••••••••••••••••••••••••'}
              InputProps={{
                readOnly: true,
                sx: { fontFamily: 'monospace', fontSize: '0.85rem' },
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton size="small" onClick={() => setShowKey(!showKey)}>
                      {showKey ? <ViewOffIcon /> : <ViewIcon />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
              sx={{ flexGrow: 1 }}
            />
            <Tooltip title={keyCopied ? 'Copied!' : 'Copy to clipboard'}>
              <IconButton onClick={handleCopyKey} color={keyCopied ? 'success' : 'default'}>
                <CopyIcon />
              </IconButton>
            </Tooltip>
          </Box>
        </Alert>
      )}

      {/* Actions */}
      <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2, gap: 1 }}>
        <Button variant="outlined" startIcon={<RefreshIcon />} onClick={fetchApiUsers}>
          Refresh
        </Button>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => {
            setFormData({ ...emptyForm });
            setCreateOpen(true);
          }}
        >
          Create API User
        </Button>
      </Box>

      {/* Table */}
      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : apiUsers.length === 0 ? (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <KeyIcon sx={{ fontSize: 48, color: 'text.disabled', mb: 2 }} />
          <Typography variant="h6" color="text.secondary">
            No API Users
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Create an API user to enable programmatic access to the CRM API.
          </Typography>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => {
              setFormData({ ...emptyForm });
              setCreateOpen(true);
            }}
          >
            Create First API User
          </Button>
        </Paper>
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Email</TableCell>
                <TableCell>Role</TableCell>
                <TableCell>API Group</TableCell>
                <TableCell>Key Prefix</TableCell>
                <TableCell>Last Used</TableCell>
                <TableCell>Expires</TableCell>
                <TableCell>Active</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {apiUsers.map((user) => (
                <TableRow key={user.id} hover>
                  <TableCell>
                    <Typography variant="body2" fontWeight={500}>
                      {user.firstName} {user.lastName}
                    </Typography>
                    {user.apiUserDescription && (
                      <Typography variant="caption" color="text.secondary" display="block">
                        {user.apiUserDescription}
                      </Typography>
                    )}
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" sx={{ fontFamily: 'monospace', fontSize: '0.8rem' }}>
                      {user.email}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Chip label={user.role} size="small" variant="outlined" />
                  </TableCell>
                  <TableCell>
                    {user.primaryGroupName ? (
                      <Chip label={user.primaryGroupName} size="small" color="info" variant="outlined" />
                    ) : (
                      <Typography variant="body2" color="text.disabled">—</Typography>
                    )}
                  </TableCell>
                  <TableCell>
                    {user.apiKeyPrefix ? (
                      <Typography variant="body2" sx={{ fontFamily: 'monospace', fontSize: '0.8rem' }}>
                        {user.apiKeyPrefix}…
                      </Typography>
                    ) : (
                      <Chip label="No Key" size="small" color="default" />
                    )}
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" fontSize="0.8rem">
                      {formatDate(user.apiKeyLastUsedAt)}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    {user.apiKeyExpiresAt ? (
                      <Chip
                        label={formatDate(user.apiKeyExpiresAt)}
                        size="small"
                        color={isKeyExpired(user.apiKeyExpiresAt) ? 'error' : 'default'}
                        variant="outlined"
                      />
                    ) : (
                      <Chip label="Never" size="small" color="success" variant="outlined" />
                    )}
                  </TableCell>
                  <TableCell>
                    <FormControlLabel
                      control={
                        <Switch
                          size="small"
                          checked={user.isActive}
                          onChange={() => handleToggleStatus(user)}
                        />
                      }
                      label=""
                    />
                  </TableCell>
                  <TableCell align="right">
                    <Tooltip title="Edit">
                      <IconButton size="small" onClick={() => openEdit(user)}>
                        <ViewIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Regenerate Key">
                      <IconButton size="small" color="warning" onClick={() => handleRegenerateKey(user)}>
                        <RefreshIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Revoke Key">
                      <IconButton size="small" color="error" onClick={() => handleRevoke(user)}>
                        <RevokeIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Delete">
                      <IconButton size="small" color="error" onClick={() => handleDelete(user)}>
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {/* Create Dialog */}
      {renderFormDialog(createOpen, 'Create API User', handleCreate, () => {
        setCreateOpen(false);
        setFormData({ ...emptyForm });
      })}

      {/* Edit Dialog */}
      {renderFormDialog(!!editUser, 'Edit API User', handleUpdate, () => {
        setEditUser(null);
        setFormData({ ...emptyForm });
      })}

      {/* Confirm Dialog */}
      <Dialog open={!!confirmAction} onClose={() => setConfirmAction(null)} maxWidth="xs">
        <DialogTitle>{confirmAction?.title}</DialogTitle>
        <DialogContent>
          <Typography>{confirmAction?.message}</Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmAction(null)}>Cancel</Button>
          <Button
            variant="contained"
            color="error"
            onClick={async () => {
              if (confirmAction) {
                await confirmAction.onConfirm();
                setConfirmAction(null);
              }
            }}
          >
            Confirm
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ApiUsersPage;

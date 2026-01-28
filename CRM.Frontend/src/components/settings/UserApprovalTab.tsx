import { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Button,
  Checkbox,
  Chip,
  CircularProgress,
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
  IconButton,
  Tooltip,
  Tabs,
  Tab,
} from '@mui/material';
import {
  CheckCircle as ApproveIcon,
  Cancel as RejectIcon,
  Refresh as RefreshIcon,
  Person as PersonIcon,
} from '@mui/icons-material';
import apiClient from '../../services/apiClient';
import { useProfile } from '../../contexts/ProfileContext';

interface UserApprovalRequest {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  company?: string;
  phone?: string;
  status: number;
  requestedAt: string;
  reviewedAt?: string;
  reviewedByUserName?: string;
  rejectionReason?: string;
}

interface UserGroup {
  id: number;
  name: string;
}

interface Department {
  id: number;
  name: string;
}

// Status enum to match backend
enum ApprovalStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
}

function UserApprovalTab() {
  const { hasPermission } = useProfile();
  const [requests, setRequests] = useState<UserApprovalRequest[]>([]);
  const [selectedIds, setSelectedIds] = useState<number[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [tabValue, setTabValue] = useState(0);

  // Approve dialog state
  const [approveDialogOpen, setApproveDialogOpen] = useState(false);
  const [approveTarget, setApproveTarget] = useState<number[]>([]);
  const [approveRole, setApproveRole] = useState('Sales');
  const [approveDepartmentId, setApproveDepartmentId] = useState<number | ''>('');
  const [approveGroupId, setApproveGroupId] = useState<number | ''>('');

  // Reject dialog state
  const [rejectDialogOpen, setRejectDialogOpen] = useState(false);
  const [rejectTarget, setRejectTarget] = useState<number[]>([]);
  const [rejectionReason, setRejectionReason] = useState('');

  // Lookup data
  const [groups, setGroups] = useState<UserGroup[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [processing, setProcessing] = useState(false);

  const fetchRequests = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const statusFilter = tabValue === 0 ? 0 : tabValue === 1 ? 1 : 2;
      const response = await apiClient.get(`/AdminSettings/approval-requests?status=${statusFilter}`);
      setRequests(response.data || []);
    } catch (err: any) {
      console.error('Error fetching approval requests:', err);
      setError(err.response?.data?.error || 'Failed to load approval requests');
    } finally {
      setLoading(false);
    }
  }, [tabValue]);

  const fetchLookups = async () => {
    try {
      const [groupsRes, deptsRes] = await Promise.all([
        apiClient.get('/AdminSettings/groups'),
        apiClient.get('/departments'),
      ]);
      setGroups(groupsRes.data || []);
      setDepartments(deptsRes.data || []);
    } catch (err) {
      console.error('Error fetching lookups:', err);
    }
  };

  useEffect(() => {
    fetchRequests();
    fetchLookups();
  }, [fetchRequests]);

  const handleSelectAll = (checked: boolean) => {
    if (checked) {
      setSelectedIds(requests.filter(r => r.status === ApprovalStatus.Pending).map(r => r.id));
    } else {
      setSelectedIds([]);
    }
  };

  const handleSelectOne = (id: number, checked: boolean) => {
    if (checked) {
      setSelectedIds([...selectedIds, id]);
    } else {
      setSelectedIds(selectedIds.filter(sid => sid !== id));
    }
  };

  const openApproveDialog = (ids: number[]) => {
    setApproveTarget(ids);
    setApproveRole('Sales');
    setApproveDepartmentId('');
    setApproveGroupId('');
    setApproveDialogOpen(true);
  };

  const openRejectDialog = (ids: number[]) => {
    setRejectTarget(ids);
    setRejectionReason('');
    setRejectDialogOpen(true);
  };

  const handleApprove = async () => {
    try {
      setProcessing(true);
      setError(null);

      for (const id of approveTarget) {
        await apiClient.post(`/AdminSettings/approval-requests/${id}/approve`, {
          assignedRole: approveRole,
          departmentId: approveDepartmentId || null,
          userProfileId: approveGroupId || null,
        });
      }

      setSuccess(`Successfully approved ${approveTarget.length} user(s)`);
      setApproveDialogOpen(false);
      setSelectedIds([]);
      fetchRequests();
    } catch (err: any) {
      console.error('Error approving users:', err);
      setError(err.response?.data?.error || 'Failed to approve user(s)');
    } finally {
      setProcessing(false);
    }
  };

  const handleReject = async () => {
    if (!rejectionReason.trim()) {
      setError('Please provide a rejection reason');
      return;
    }

    try {
      setProcessing(true);
      setError(null);

      for (const id of rejectTarget) {
        await apiClient.post(`/AdminSettings/approval-requests/${id}/reject`, {
          rejectionReason: rejectionReason.trim(),
        });
      }

      setSuccess(`Successfully rejected ${rejectTarget.length} user(s)`);
      setRejectDialogOpen(false);
      setSelectedIds([]);
      fetchRequests();
    } catch (err: any) {
      console.error('Error rejecting users:', err);
      setError(err.response?.data?.error || 'Failed to reject user(s)');
    } finally {
      setProcessing(false);
    }
  };

  const getStatusChip = (status: number) => {
    switch (status) {
      case ApprovalStatus.Pending:
        return <Chip label="Pending" color="warning" size="small" />;
      case ApprovalStatus.Approved:
        return <Chip label="Approved" color="success" size="small" />;
      case ApprovalStatus.Rejected:
        return <Chip label="Rejected" color="error" size="small" />;
      default:
        return <Chip label="Unknown" size="small" />;
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  const pendingRequests = requests.filter(r => r.status === ApprovalStatus.Pending);
  const canManage = hasPermission('canManageUsers');

  return (
    <Box>
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

      <Card>
        <CardContent>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h6" sx={{ fontWeight: 600, display: 'flex', alignItems: 'center', gap: 1 }}>
              <PersonIcon color="primary" />
              User Approvals
            </Typography>
            <Tooltip title="Refresh">
              <IconButton onClick={fetchRequests} disabled={loading}>
                <RefreshIcon />
              </IconButton>
            </Tooltip>
          </Box>

          <Tabs value={tabValue} onChange={(_, v) => setTabValue(v)} sx={{ mb: 2 }}>
            <Tab label="Pending" />
            <Tab label="Approved" />
            <Tab label="Rejected" />
          </Tabs>

          {/* Bulk Actions Bar */}
          {selectedIds.length > 0 && tabValue === 0 && canManage && (
            <Box sx={{ 
              mb: 2, 
              p: 1.5, 
              backgroundColor: '#E8DEF8', 
              borderRadius: 1, 
              display: 'flex', 
              alignItems: 'center', 
              gap: 2 
            }}>
              <Typography variant="body2" sx={{ fontWeight: 500 }}>
                {selectedIds.length} selected
              </Typography>
              <Button
                variant="contained"
                size="small"
                color="success"
                startIcon={<ApproveIcon />}
                onClick={() => openApproveDialog(selectedIds)}
              >
                Approve Selected
              </Button>
              <Button
                variant="contained"
                size="small"
                color="error"
                startIcon={<RejectIcon />}
                onClick={() => openRejectDialog(selectedIds)}
              >
                Reject Selected
              </Button>
            </Box>
          )}

          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
              <CircularProgress />
            </Box>
          ) : (
            <Table size="small">
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                  {tabValue === 0 && canManage && (
                    <TableCell padding="checkbox">
                      <Checkbox
                        checked={pendingRequests.length > 0 && selectedIds.length === pendingRequests.length}
                        indeterminate={selectedIds.length > 0 && selectedIds.length < pendingRequests.length}
                        onChange={(e) => handleSelectAll(e.target.checked)}
                      />
                    </TableCell>
                  )}
                  <TableCell><strong>Name</strong></TableCell>
                  <TableCell><strong>Email</strong></TableCell>
                  <TableCell><strong>Company</strong></TableCell>
                  <TableCell><strong>Requested</strong></TableCell>
                  <TableCell><strong>Status</strong></TableCell>
                  {tabValue !== 0 && <TableCell><strong>Reviewed By</strong></TableCell>}
                  {tabValue === 2 && <TableCell><strong>Reason</strong></TableCell>}
                  {tabValue === 0 && canManage && <TableCell align="center"><strong>Actions</strong></TableCell>}
                </TableRow>
              </TableHead>
              <TableBody>
                {requests.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={tabValue === 0 ? 8 : 7} sx={{ textAlign: 'center', py: 4 }}>
                      <Typography color="textSecondary">
                        No {tabValue === 0 ? 'pending' : tabValue === 1 ? 'approved' : 'rejected'} approvals
                      </Typography>
                    </TableCell>
                  </TableRow>
                ) : (
                  requests.map((request) => (
                    <TableRow key={request.id} hover>
                      {tabValue === 0 && canManage && (
                        <TableCell padding="checkbox">
                          <Checkbox
                            checked={selectedIds.includes(request.id)}
                            onChange={(e) => handleSelectOne(request.id, e.target.checked)}
                          />
                        </TableCell>
                      )}
                      <TableCell>{request.firstName} {request.lastName}</TableCell>
                      <TableCell>{request.email}</TableCell>
                      <TableCell>{request.company || '-'}</TableCell>
                      <TableCell>{formatDate(request.requestedAt)}</TableCell>
                      <TableCell>{getStatusChip(request.status)}</TableCell>
                      {tabValue !== 0 && (
                        <TableCell>
                          {request.reviewedByUserName || '-'}
                          {request.reviewedAt && (
                            <Typography variant="caption" display="block" color="textSecondary">
                              {formatDate(request.reviewedAt)}
                            </Typography>
                          )}
                        </TableCell>
                      )}
                      {tabValue === 2 && (
                        <TableCell>
                          <Tooltip title={request.rejectionReason || ''}>
                            <Typography noWrap sx={{ maxWidth: 150 }}>
                              {request.rejectionReason || '-'}
                            </Typography>
                          </Tooltip>
                        </TableCell>
                      )}
                      {tabValue === 0 && canManage && (
                        <TableCell align="center">
                          <Tooltip title="Approve">
                            <IconButton
                              color="success"
                              size="small"
                              onClick={() => openApproveDialog([request.id])}
                            >
                              <ApproveIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Reject">
                            <IconButton
                              color="error"
                              size="small"
                              onClick={() => openRejectDialog([request.id])}
                            >
                              <RejectIcon />
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      )}
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Approve Dialog */}
      <Dialog open={approveDialogOpen} onClose={() => setApproveDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          Approve {approveTarget.length > 1 ? `${approveTarget.length} Users` : 'User'}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
            <FormControl fullWidth>
              <InputLabel>Role</InputLabel>
              <Select
                value={approveRole}
                label="Role"
                onChange={(e) => setApproveRole(e.target.value)}
              >
                <MenuItem value="Sales">Sales</MenuItem>
                <MenuItem value="Support">Support</MenuItem>
                <MenuItem value="Manager">Manager</MenuItem>
                <MenuItem value="Admin">Admin</MenuItem>
              </Select>
            </FormControl>
            <FormControl fullWidth>
              <InputLabel>Department</InputLabel>
              <Select
                value={approveDepartmentId}
                label="Department"
                onChange={(e) => setApproveDepartmentId(e.target.value as number)}
              >
                <MenuItem value="">None</MenuItem>
                {departments.map((dept) => (
                  <MenuItem key={dept.id} value={dept.id}>{dept.name}</MenuItem>
                ))}
              </Select>
            </FormControl>
            <FormControl fullWidth>
              <InputLabel>User Group</InputLabel>
              <Select
                value={approveGroupId}
                label="User Group"
                onChange={(e) => setApproveGroupId(e.target.value as number)}
              >
                <MenuItem value="">None</MenuItem>
                {groups.map((group) => (
                  <MenuItem key={group.id} value={group.id}>{group.name}</MenuItem>
                ))}
              </Select>
            </FormControl>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setApproveDialogOpen(false)} disabled={processing}>
            Cancel
          </Button>
          <Button
            variant="contained"
            color="success"
            onClick={handleApprove}
            disabled={processing}
            startIcon={processing ? <CircularProgress size={16} /> : <ApproveIcon />}
          >
            Approve
          </Button>
        </DialogActions>
      </Dialog>

      {/* Reject Dialog */}
      <Dialog open={rejectDialogOpen} onClose={() => setRejectDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          Reject {rejectTarget.length > 1 ? `${rejectTarget.length} Users` : 'User'}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 2 }}>
            <TextField
              fullWidth
              multiline
              rows={3}
              label="Rejection Reason"
              value={rejectionReason}
              onChange={(e) => setRejectionReason(e.target.value)}
              placeholder="Please provide a reason for rejection..."
              required
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRejectDialogOpen(false)} disabled={processing}>
            Cancel
          </Button>
          <Button
            variant="contained"
            color="error"
            onClick={handleReject}
            disabled={processing || !rejectionReason.trim()}
            startIcon={processing ? <CircularProgress size={16} /> : <RejectIcon />}
          >
            Reject
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default UserApprovalTab;

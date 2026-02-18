/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 * Licensed under the Source-Available License (see LICENSE) v3.0
 */

import { useState, useEffect, useCallback } from 'react';
import {
  Box, Container, Typography, Card, CardContent, Table, TableBody, TableCell,
  TableHead, TableRow, Button, Dialog, DialogTitle, DialogContent, DialogActions,
  TextField, Stack, Chip, IconButton, Tooltip, CircularProgress,
  Alert, Grid, Tabs, Tab, FormControl, InputLabel, Select, MenuItem,
  Paper, LinearProgress, Badge, List, ListItem, ListItemText, ListItemIcon,
  ListItemSecondaryAction, Avatar, Divider, Checkbox, SelectChangeEvent,
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon,
  CheckCircle as ApproveIcon, Cancel as RejectIcon, Forward as DelegateIcon,
  History as HistoryIcon, Refresh as RefreshIcon, Settings as SettingsIcon,
  Person as PersonIcon, Group as GroupIcon, AccessTime as TimeIcon,
  ThumbUp, ThumbDown, Send as ReminderIcon, AttachFile as AttachIcon,
  Assessment as StatsIcon, Warning as UrgentIcon, Schedule as PendingIcon,
} from '@mui/icons-material';
import { DialogError, ActionButton, TabPanel } from '../components/common';
import { useProfile } from '../contexts/ProfileContext';
import approvalService, {
  ApprovalRequest, ApprovalMatrix, ApprovalStatistics,
  ApprovalEntityType, ApprovalStatus, ApprovalUrgency, ApproverStatus
} from '../services/approvalService';
import logo from '../assets/logo.png';

// ==================== HELPER FUNCTIONS ====================

const getEntityTypeLabel = (type: ApprovalEntityType): string => {
  const labels: Record<ApprovalEntityType, string> = {
    [ApprovalEntityType.Quote]: 'Quote',
    [ApprovalEntityType.Discount]: 'Discount',
    [ApprovalEntityType.Contract]: 'Contract',
    [ApprovalEntityType.Order]: 'Order',
    [ApprovalEntityType.Expense]: 'Expense',
    [ApprovalEntityType.PurchaseOrder]: 'Purchase Order',
    [ApprovalEntityType.Leave]: 'Leave Request',
    [ApprovalEntityType.Custom]: 'Custom',
  };
  return labels[type] || type;
};

const getStatusColor = (status: ApprovalStatus): 'warning' | 'success' | 'error' | 'info' | 'default' => {
  switch (status) {
    case ApprovalStatus.Pending: return 'warning';
    case ApprovalStatus.Approved: return 'success';
    case ApprovalStatus.Rejected: return 'error';
    case ApprovalStatus.Delegated: return 'info';
    case ApprovalStatus.Cancelled: return 'default';
    case ApprovalStatus.Expired: return 'default';
    default: return 'default';
  }
};

const getUrgencyColor = (urgency: ApprovalUrgency): 'error' | 'warning' | 'info' | 'success' => {
  switch (urgency) {
    case ApprovalUrgency.Critical: return 'error';
    case ApprovalUrgency.High: return 'warning';
    case ApprovalUrgency.Normal: return 'info';
    case ApprovalUrgency.Low: return 'success';
    default: return 'info';
  }
};

// ==================== MAIN COMPONENT ====================

const ApprovalsPage = () => {
  const { profile } = useProfile();
  const [activeTab, setActiveTab] = useState(0);
  const [pendingApprovals, setPendingApprovals] = useState<ApprovalRequest[]>([]);
  const [allRequests, setAllRequests] = useState<ApprovalRequest[]>([]);
  const [matrices, setMatrices] = useState<ApprovalMatrix[]>([]);
  const [statistics, setStatistics] = useState<ApprovalStatistics | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Dialog states
  const [approveDialog, setApproveDialog] = useState(false);
  const [rejectDialog, setRejectDialog] = useState(false);
  const [delegateDialog, setDelegateDialog] = useState(false);
  const [matrixDialog, setMatrixDialog] = useState(false);
  const [selectedRequest, setSelectedRequest] = useState<ApprovalRequest | null>(null);
  const [selectedRequests, setSelectedRequests] = useState<number[]>([]);
  const [comments, setComments] = useState('');
  const [delegateToUserId, setDelegateToUserId] = useState<number | null>(null);
  const [saving, setSaving] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [matrixFormData, setMatrixFormData] = useState<Partial<ApprovalMatrix>>({});

  // Filters
  const [filterStatus, setFilterStatus] = useState<string>('all');
  const [filterEntityType, setFilterEntityType] = useState<string>('all');

  // Load data
  const loadPendingApprovals = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await approvalService.getMyPendingApprovals();
      setPendingApprovals(res.data?.items ?? []);
    } catch (err: any) {
      setError(err.message || 'Failed to load pending approvals');
      setPendingApprovals([]);
    } finally {
      setLoading(false);
    }
  }, []);

  const loadAllRequests = useCallback(async () => {
    try {
      const status = filterStatus !== 'all' ? filterStatus as ApprovalStatus : undefined;
      const entityType = filterEntityType !== 'all' ? filterEntityType as ApprovalEntityType : undefined;
      const res = await approvalService.getAllRequests(status, entityType, 1, 100);
      setAllRequests(res.data?.items ?? []);
    } catch (err: any) {
      console.error('Failed to load requests:', err);
      setAllRequests([]);
    }
  }, [filterStatus, filterEntityType]);

  const loadMatrices = useCallback(async () => {
    try {
      const res = await approvalService.getMatrices();
      setMatrices(res.data ?? []);
    } catch (err: any) {
      console.error('Failed to load matrices:', err);
      setMatrices([]);
    }
  }, []);

  const loadStatistics = useCallback(async () => {
    try {
      const res = await approvalService.getStatistics();
      setStatistics(res.data ?? null);
    } catch (err: any) {
      console.error('Failed to load statistics:', err);
      setStatistics(null);
    }
  }, []);

  useEffect(() => {
    loadPendingApprovals();
    loadStatistics();
  }, [loadPendingApprovals, loadStatistics]);

  useEffect(() => {
    if (activeTab === 1) loadAllRequests();
    if (activeTab === 2) loadMatrices();
  }, [activeTab, loadAllRequests, loadMatrices]);

  // Handlers
  const handleApprove = (request: ApprovalRequest) => {
    setSelectedRequest(request);
    setComments('');
    setApproveDialog(true);
  };

  const handleReject = (request: ApprovalRequest) => {
    setSelectedRequest(request);
    setComments('');
    setRejectDialog(true);
  };

  const handleDelegate = (request: ApprovalRequest) => {
    setSelectedRequest(request);
    setDelegateToUserId(null);
    setDelegateDialog(true);
  };

  const handleConfirmApprove = async () => {
    if (!selectedRequest?.id) return;
    setSaving(true);
    try {
      await approvalService.approve(selectedRequest.id, { comments });
      setSuccessMessage('Request approved successfully');
      setApproveDialog(false);
      loadPendingApprovals();
      loadStatistics();
    } catch (err: any) {
      setError(err.message || 'Failed to approve request');
    } finally {
      setSaving(false);
    }
  };

  const handleConfirmReject = async () => {
    if (!selectedRequest?.id || !comments.trim()) return;
    setSaving(true);
    try {
      await approvalService.reject(selectedRequest.id, { comments });
      setSuccessMessage('Request rejected');
      setRejectDialog(false);
      loadPendingApprovals();
      loadStatistics();
    } catch (err: any) {
      setError(err.message || 'Failed to reject request');
    } finally {
      setSaving(false);
    }
  };

  const handleConfirmDelegate = async () => {
    if (!selectedRequest?.id || !delegateToUserId) return;
    setSaving(true);
    try {
      await approvalService.delegate(selectedRequest.id, { delegateToUserId, comments });
      setSuccessMessage('Request delegated successfully');
      setDelegateDialog(false);
      loadPendingApprovals();
    } catch (err: any) {
      setError(err.message || 'Failed to delegate request');
    } finally {
      setSaving(false);
    }
  };

  const handleBulkApprove = async () => {
    if (selectedRequests.length === 0) return;
    setSaving(true);
    try {
      await approvalService.bulkApprove(selectedRequests, 'Bulk approved');
      setSuccessMessage(`${selectedRequests.length} requests approved`);
      setSelectedRequests([]);
      loadPendingApprovals();
      loadStatistics();
    } catch (err: any) {
      setError(err.message || 'Failed to bulk approve');
    } finally {
      setSaving(false);
    }
  };

  const handleSendReminder = async (request: ApprovalRequest) => {
    try {
      await approvalService.sendReminder(request.id!);
      setSuccessMessage('Reminder sent');
    } catch (err: any) {
      setError(err.message || 'Failed to send reminder');
    }
  };

  const handleSelectRequest = (id: number) => {
    setSelectedRequests(prev =>
      prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]
    );
  };

  const handleSelectAll = () => {
    if (selectedRequests.length === pendingApprovals.length) {
      setSelectedRequests([]);
    } else {
      setSelectedRequests(pendingApprovals.map(r => r.id!));
    }
  };

  const handleAddMatrix = () => {
    setMatrixFormData({
      name: '',
      description: '',
      entityType: ApprovalEntityType.Quote,
      isActive: true,
      levels: [],
      conditions: [],
      settings: {
        allowParallelApprovals: false,
        requireAllLevels: true,
        allowSelfApproval: false,
        notifyOnSubmit: true,
        notifyOnApproval: true,
        notifyOnRejection: true,
      },
    });
    setMatrixDialog(true);
  };

  const handleSaveMatrix = async () => {
    if (!matrixFormData.name) return;
    setSaving(true);
    try {
      await approvalService.createMatrix(matrixFormData as any);
      setSuccessMessage('Approval matrix created');
      setMatrixDialog(false);
      loadMatrices();
    } catch (err: any) {
      setError(err.message || 'Failed to save matrix');
    } finally {
      setSaving(false);
    }
  };

  // Statistics cards
  const StatsCard = ({ title, value, icon, color, subtitle }: { title: string; value: string | number; icon: React.ReactNode; color: string; subtitle?: string }) => (
    <Card>
      <CardContent>
        <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
          <Box>
            <Typography variant="caption" color="text.secondary">{title}</Typography>
            <Typography variant="h4">{value}</Typography>
            {subtitle && <Typography variant="caption" color="text.secondary">{subtitle}</Typography>}
          </Box>
          <Box sx={{ color, opacity: 0.8 }}>{icon}</Box>
        </Stack>
      </CardContent>
    </Card>
  );

  return (
    <Box sx={{ minHeight: '100vh', backgroundColor: '#f5f5f5' }}>
      {/* Header */}
      <Box sx={{ backgroundColor: '#1976d2', color: 'white', py: 3 }}>
        <Container maxWidth="xl">
          <Stack direction="row" justifyContent="space-between" alignItems="center">
            <Stack direction="row" spacing={2} alignItems="center">
              <img src={logo} alt="CRM" style={{ height: 40 }} />
              <Box>
                <Typography variant="h5">
                  Approvals
                  {pendingApprovals.length > 0 && (
                    <Badge
                      badgeContent={pendingApprovals.length}
                      color="error"
                      sx={{ ml: 2 }}
                    />
                  )}
                </Typography>
                <Typography variant="body2" sx={{ opacity: 0.8 }}>
                  Review and manage approval requests
                </Typography>
              </Box>
            </Stack>
            <Stack direction="row" spacing={1}>
              {selectedRequests.length > 0 && (
                <Button
                  variant="contained"
                  startIcon={<ThumbUp />}
                  onClick={handleBulkApprove}
                  disabled={saving}
                  sx={{ backgroundColor: 'white', color: 'green' }}
                >
                  Approve Selected ({selectedRequests.length})
                </Button>
              )}
              <IconButton onClick={loadPendingApprovals} sx={{ color: 'white' }}>
                <RefreshIcon />
              </IconButton>
            </Stack>
          </Stack>
        </Container>
      </Box>

      <Container maxWidth="xl" sx={{ py: 3 }}>
        {/* Messages */}
        {error && <Alert severity="error" onClose={() => setError(null)} sx={{ mb: 2 }}>{error}</Alert>}
        {successMessage && <Alert severity="success" onClose={() => setSuccessMessage(null)} sx={{ mb: 2 }}>{successMessage}</Alert>}

        {/* Stats */}
        {statistics && (
          <Grid container spacing={2} sx={{ mb: 3 }}>
            <Grid item xs={12} sm={6} md={3}>
              <StatsCard
                title="Pending Approvals"
                value={statistics?.pendingRequests ?? 0}
                subtitle={`${statistics?.totalRequests ?? 0} total`}
                icon={<PendingIcon sx={{ fontSize: 40 }} />}
                color="#ed6c02"
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <StatsCard
                title="Approved"
                value={statistics?.approvedRequests ?? 0}
                subtitle="Total approved"
                icon={<ThumbUp sx={{ fontSize: 40 }} />}
                color="#2e7d32"
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <StatsCard
                title="Rejected"
                value={statistics?.rejectedRequests ?? 0}
                subtitle="Total rejected"
                icon={<ThumbDown sx={{ fontSize: 40 }} />}
                color="#d32f2f"
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <StatsCard
                title="Avg Approval Time"
                value={`${statistics.avgApprovalTimeHours?.toFixed(1) || 0}h`}
                icon={<TimeIcon sx={{ fontSize: 40 }} />}
                color="#1976d2"
              />
            </Grid>
          </Grid>
        )}

        {/* Tabs */}
        <Paper>
          <Tabs value={activeTab} onChange={(_, v) => setActiveTab(v)}>
            <Tab
              icon={<Badge badgeContent={pendingApprovals.length} color="error"><PendingIcon /></Badge>}
              label="Pending"
            />
            <Tab icon={<HistoryIcon />} label="All Requests" />
            <Tab icon={<SettingsIcon />} label="Approval Matrices" />
            <Tab icon={<StatsIcon />} label="Analytics" />
          </Tabs>

          {loading && <LinearProgress />}

          {/* Pending Tab */}
          <TabPanel value={activeTab} index={0}>
            <Box sx={{ p: 2 }}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell padding="checkbox">
                      <Checkbox
                        checked={selectedRequests.length === pendingApprovals.length && pendingApprovals.length > 0}
                        indeterminate={selectedRequests.length > 0 && selectedRequests.length < pendingApprovals.length}
                        onChange={handleSelectAll}
                      />
                    </TableCell>
                    <TableCell>Request</TableCell>
                    <TableCell>Type</TableCell>
                    <TableCell>Requester</TableCell>
                    <TableCell align="right">Amount</TableCell>
                    <TableCell>Urgency</TableCell>
                    <TableCell>Submitted</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {pendingApprovals.map(request => (
                    <TableRow key={request.id}>
                      <TableCell padding="checkbox">
                        <Checkbox
                          checked={selectedRequests.includes(request.id!)}
                          onChange={() => handleSelectRequest(request.id!)}
                        />
                      </TableCell>
                      <TableCell>
                        <Typography fontWeight={500}>{request.entityName || `${request.entityType} #${request.entityId}`}</Typography>
                        {request.entityDescription && (
                          <Typography variant="caption" color="text.secondary" display="block">
                            {request.entityDescription.substring(0, 100)}
                            {request.entityDescription.length > 100 && '...'}
                          </Typography>
                        )}
                        {request.reason && (
                          <Chip label={request.reason} size="small" sx={{ mt: 0.5 }} />
                        )}
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={getEntityTypeLabel(request.entityType)}
                          size="small"
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell>
                        <Stack direction="row" alignItems="center" spacing={1}>
                          <Avatar sx={{ width: 24, height: 24, fontSize: 12 }}>
                            {request.requestedByName?.charAt(0) || '?'}
                          </Avatar>
                          <Typography variant="body2">{request.requestedByName}</Typography>
                        </Stack>
                      </TableCell>
                      <TableCell align="right">
                        {request.totalAmount != null ? (
                          <Typography fontWeight={500}>
                            ${request.totalAmount.toLocaleString()}
                          </Typography>
                        ) : '-'}
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={request.urgency}
                          size="small"
                          color={getUrgencyColor(request.urgency)}
                          icon={request.urgency === ApprovalUrgency.Critical ? <UrgentIcon /> : undefined}
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {new Date(request.requestedAt).toLocaleDateString()}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          {new Date(request.requestedAt).toLocaleTimeString()}
                        </Typography>
                      </TableCell>
                      <TableCell align="right">
                        <Stack direction="row" spacing={0.5} justifyContent="flex-end">
                          <Tooltip title="Approve">
                            <IconButton
                              size="small"
                              color="success"
                              onClick={() => handleApprove(request)}
                            >
                              <ApproveIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Reject">
                            <IconButton
                              size="small"
                              color="error"
                              onClick={() => handleReject(request)}
                            >
                              <RejectIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delegate">
                            <IconButton
                              size="small"
                              onClick={() => handleDelegate(request)}
                            >
                              <DelegateIcon />
                            </IconButton>
                          </Tooltip>
                        </Stack>
                      </TableCell>
                    </TableRow>
                  ))}
                  {pendingApprovals.length === 0 && !loading && (
                    <TableRow>
                      <TableCell colSpan={8} align="center">
                        <Typography color="text.secondary" sx={{ py: 4 }}>
                          No pending approvals
                        </Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </Box>
          </TabPanel>

          {/* All Requests Tab */}
          <TabPanel value={activeTab} index={1}>
            <Box sx={{ p: 2 }}>
              <Stack direction="row" spacing={2} sx={{ mb: 2 }}>
                <FormControl size="small" sx={{ minWidth: 150 }}>
                  <InputLabel>Status</InputLabel>
                  <Select
                    value={filterStatus}
                    label="Status"
                    onChange={(e) => setFilterStatus(e.target.value)}
                  >
                    <MenuItem value="all">All</MenuItem>
                    {Object.values(ApprovalStatus).map(status => (
                      <MenuItem key={status} value={status}>{status}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
                <FormControl size="small" sx={{ minWidth: 150 }}>
                  <InputLabel>Type</InputLabel>
                  <Select
                    value={filterEntityType}
                    label="Type"
                    onChange={(e) => setFilterEntityType(e.target.value)}
                  >
                    <MenuItem value="all">All</MenuItem>
                    {Object.values(ApprovalEntityType).map(type => (
                      <MenuItem key={type} value={type}>{getEntityTypeLabel(type)}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Stack>

              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Request</TableCell>
                    <TableCell>Type</TableCell>
                    <TableCell>Requester</TableCell>
                    <TableCell align="right">Amount</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Date</TableCell>
                    <TableCell>Processed By</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {allRequests.map(request => (
                    <TableRow key={request.id}>
                      <TableCell>
                        <Typography fontWeight={500}>{request.entityName || `${request.entityType} #${request.entityId}`}</Typography>
                        {request.reason && (
                          <Chip label={request.reason} size="small" sx={{ mt: 0.5 }} />
                        )}
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={getEntityTypeLabel(request.entityType)}
                          size="small"
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell>{request.requestedByName}</TableCell>
                      <TableCell align="right">
                        {request.totalAmount != null ? `$${request.totalAmount.toLocaleString()}` : '-'}
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={request.status}
                          size="small"
                          color={getStatusColor(request.status)}
                        />
                      </TableCell>
                      <TableCell>
                        {new Date(request.requestedAt).toLocaleDateString()}
                      </TableCell>
                      <TableCell>
                        {request.approvers?.find(a => a.status === ApproverStatus.Approved)?.userName || '-'}
                      </TableCell>
                      <TableCell align="right">
                        {request.status === ApprovalStatus.Pending && (
                          <Tooltip title="Send Reminder">
                            <IconButton size="small" onClick={() => handleSendReminder(request)}>
                              <ReminderIcon />
                            </IconButton>
                          </Tooltip>
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                  {allRequests.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={8} align="center">
                        <Typography color="text.secondary" sx={{ py: 4 }}>No requests found</Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </Box>
          </TabPanel>

          {/* Approval Matrices Tab */}
          <TabPanel value={activeTab} index={2}>
            <Box sx={{ p: 2 }}>
              <Stack direction="row" justifyContent="flex-end" sx={{ mb: 2 }}>
                <Button variant="contained" startIcon={<AddIcon />} onClick={handleAddMatrix}>
                  Add Matrix
                </Button>
              </Stack>

              <Grid container spacing={2}>
                {matrices.map(matrix => (
                  <Grid item xs={12} md={6} key={matrix.id}>
                    <Card>
                      <CardContent>
                        <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
                          <Box>
                            <Typography variant="h6">{matrix.name}</Typography>
                            <Chip
                              label={getEntityTypeLabel(matrix.entityType)}
                              size="small"
                              variant="outlined"
                              sx={{ mt: 1 }}
                            />
                          </Box>
                          <Chip
                            label={matrix?.isActive !== false ? 'Active' : 'Inactive'}
                            color={matrix?.isActive !== false ? 'success' : 'default'}
                            size="small"
                          />
                        </Stack>
                        <Divider sx={{ my: 2 }} />
                        <Typography variant="body2" color="text.secondary" gutterBottom>
                          Conditions
                        </Typography>
                        <Stack direction="row" spacing={1} flexWrap="wrap" sx={{ mb: 1 }}>
                          {matrix.conditions && matrix.conditions.length > 0 ? (
                            matrix.conditions.slice(0, 3).map((condition, idx) => (
                              <Chip
                                key={idx}
                                label={`${condition.field} ${condition.operator} ${condition.value}`}
                                size="small"
                              />
                            ))
                          ) : (
                            <Typography variant="caption" color="text.secondary">No conditions</Typography>
                          )}
                        </Stack>
                        <Typography variant="body2" color="text.secondary" gutterBottom sx={{ mt: 2 }}>
                          Approval Levels ({matrix.levels?.length || 0})
                        </Typography>
                        <List dense>
                          {matrix.levels?.slice(0, 3).map((level, idx) => (
                            <ListItem key={idx} disablePadding>
                              <ListItemIcon sx={{ minWidth: 32 }}>
                                {level.approverType === 'user' ? <PersonIcon fontSize="small" /> : <GroupIcon fontSize="small" />}
                              </ListItemIcon>
                              <ListItemText
                                primary={level.name || `Level ${level.level}`}
                                secondary={`${level.requiredApprovers} approver(s) required`}
                              />
                            </ListItem>
                          ))}
                          {(matrix.levels?.length || 0) > 3 && (
                            <Typography variant="caption" color="text.secondary">
                              +{(matrix.levels?.length || 0) - 3} more levels
                            </Typography>
                          )}
                        </List>
                      </CardContent>
                    </Card>
                  </Grid>
                ))}
                {matrices.length === 0 && (
                  <Grid item xs={12}>
                    <Typography color="text.secondary" align="center" sx={{ py: 4 }}>
                      No approval matrices configured
                    </Typography>
                  </Grid>
                )}
              </Grid>
            </Box>
          </TabPanel>

          {/* Analytics Tab */}
          <TabPanel value={activeTab} index={3}>
            <Box sx={{ p: 2 }}>
              {statistics && (
                <Grid container spacing={3}>
                  <Grid item xs={12} md={6}>
                    <Card>
                      <CardContent>
                        <Typography variant="h6" gutterBottom>Requests by Type</Typography>
                        <List>
                          {statistics.requestsByType?.map(item => {
                            const total = statistics.totalRequests || 1;
                            const percentage = Math.round((item.count / total) * 100);
                            return (
                              <ListItem key={item.type}>
                                <ListItemText
                                  primary={getEntityTypeLabel(item.type)}
                                  secondary={`${item.count} requests`}
                                />
                                <LinearProgress
                                  variant="determinate"
                                  value={percentage}
                                  sx={{ width: 100, height: 8, borderRadius: 4 }}
                                />
                              </ListItem>
                            );
                          })}
                        </List>
                      </CardContent>
                    </Card>
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <Card>
                      <CardContent>
                        <Typography variant="h6" gutterBottom>Top Approvers</Typography>
                        <List>
                          {statistics.topApprovers?.slice(0, 5).map(approver => (
                            <ListItem key={approver.userId}>
                              <ListItemIcon>
                                <Avatar sx={{ width: 32, height: 32, fontSize: 14 }}>
                                  {approver.userName?.charAt(0)}
                                </Avatar>
                              </ListItemIcon>
                              <ListItemText
                                primary={approver.userName}
                                secondary={`${approver.count} approvals`}
                              />
                              <Typography variant="body2" color="text.secondary">
                                Avg: {approver.avgTimeHours?.toFixed(1)}h
                              </Typography>
                            </ListItem>
                          ))}
                        </List>
                      </CardContent>
                    </Card>
                  </Grid>
                </Grid>
              )}
            </Box>
          </TabPanel>
        </Paper>
      </Container>

      {/* Approve Dialog */}
      <Dialog open={approveDialog} onClose={() => setApproveDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Approve Request</DialogTitle>
        <DialogContent>
          <Typography gutterBottom>
            Are you sure you want to approve "{selectedRequest?.entityName}"?
          </Typography>
          <TextField
            label="Comments (optional)"
            value={comments}
            onChange={(e) => setComments(e.target.value)}
            fullWidth
            multiline
            rows={3}
            sx={{ mt: 2 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setApproveDialog(false)}>Cancel</Button>
          <Button
            variant="contained"
            color="success"
            onClick={handleConfirmApprove}
            disabled={saving}
          >
            {saving ? <CircularProgress size={20} /> : 'Approve'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Reject Dialog */}
      <Dialog open={rejectDialog} onClose={() => setRejectDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Reject Request</DialogTitle>
        <DialogContent>
          <Typography gutterBottom color="error">
            Are you sure you want to reject "{selectedRequest?.entityName}"?
          </Typography>
          <TextField
            label="Reason (required)"
            value={comments}
            onChange={(e) => setComments(e.target.value)}
            fullWidth
            multiline
            rows={3}
            required
            sx={{ mt: 2 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRejectDialog(false)}>Cancel</Button>
          <Button
            variant="contained"
            color="error"
            onClick={handleConfirmReject}
            disabled={saving || !comments.trim()}
          >
            {saving ? <CircularProgress size={20} /> : 'Reject'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delegate Dialog */}
      <Dialog open={delegateDialog} onClose={() => setDelegateDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Delegate Request</DialogTitle>
        <DialogContent>
          <Typography gutterBottom>
            Delegate approval request "{selectedRequest?.entityName}" to another user.
          </Typography>
          <TextField
            label="Delegate to User ID"
            type="number"
            value={delegateToUserId || ''}
            onChange={(e) => setDelegateToUserId(parseInt(e.target.value))}
            fullWidth
            required
            sx={{ mt: 2 }}
          />
          <TextField
            label="Reason (optional)"
            value={comments}
            onChange={(e) => setComments(e.target.value)}
            fullWidth
            multiline
            rows={2}
            sx={{ mt: 2 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDelegateDialog(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleConfirmDelegate}
            disabled={saving || !delegateToUserId}
          >
            {saving ? <CircularProgress size={20} /> : 'Delegate'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Matrix Dialog */}
      <Dialog open={matrixDialog} onClose={() => setMatrixDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Create Approval Matrix</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 0.5 }}>
            <Grid item xs={12}>
              <TextField
                label="Name"
                value={matrixFormData.name || ''}
                onChange={(e) => setMatrixFormData({ ...matrixFormData, name: e.target.value })}
                fullWidth
                required
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                label="Description"
                value={matrixFormData.description || ''}
                onChange={(e) => setMatrixFormData({ ...matrixFormData, description: e.target.value })}
                fullWidth
                multiline
                rows={2}
              />
            </Grid>
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>Entity Type</InputLabel>
                <Select
                  value={matrixFormData.entityType || ApprovalEntityType.Quote}
                  label="Entity Type"
                  onChange={(e: SelectChangeEvent) => setMatrixFormData({ ...matrixFormData, entityType: e.target.value as ApprovalEntityType })}
                >
                  {Object.values(ApprovalEntityType).map(type => (
                    <MenuItem key={type} value={type}>{getEntityTypeLabel(type)}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <Typography variant="subtitle2" color="text.secondary">
                Approval levels and conditions can be configured after creation.
              </Typography>
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setMatrixDialog(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleSaveMatrix}
            disabled={saving || !matrixFormData.name}
          >
            {saving ? <CircularProgress size={20} /> : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ApprovalsPage;

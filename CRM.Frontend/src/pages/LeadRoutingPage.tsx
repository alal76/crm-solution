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
  Paper, LinearProgress, List, ListItem, ListItemText, ListItemIcon,
  FormControlLabel, Switch, Divider, Accordion, AccordionSummary, AccordionDetails,
  SelectChangeEvent, TablePagination,
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon,
  PlayArrow as TestIcon, ContentCopy as CopyIcon, History as HistoryIcon,
  TrendingUp as StatsIcon, ExpandMore, Refresh as RefreshIcon,
  Route as RouteIcon, Rule as RuleIcon, People as PeopleIcon,
  Speed as SpeedIcon, CheckCircle as SuccessIcon, Error as ErrorIcon,
  Schedule as ScheduleIcon, Equalizer as BalanceIcon,
} from '@mui/icons-material';
import { DialogError, ActionButton, TabPanel } from '../components/common';
import { useApiState } from '../hooks/useApiState';
import { useProfile } from '../contexts/ProfileContext';
import leadRoutingService, {
  LeadRoutingRule, RoutingType, TargetType, CriteriaOperator,
  RoutingLog, RoutingStatistics, RoutingCriteria
} from '../services/leadRoutingService';
import logo from '../assets/logo.png';
import { usePagination } from '../hooks/usePagination';

// ==================== HELPER FUNCTIONS ====================

const getRoutingTypeLabel = (type: RoutingType): string => {
  const labels: Record<RoutingType, string> = {
    [RoutingType.DirectAssignment]: 'Direct Assignment',
    [RoutingType.RoundRobin]: 'Round Robin',
    [RoutingType.WeightedDistribution]: 'Weighted',
    [RoutingType.LeastLoaded]: 'Least Loaded',
    [RoutingType.Geography]: 'Geography',
    [RoutingType.Skill]: 'Skill-Based',
    [RoutingType.Queue]: 'Queue',
  };
  return labels[type] || type;
};

const getTargetTypeLabel = (type: TargetType): string => {
  const labels: Record<TargetType, string> = {
    [TargetType.User]: 'User',
    [TargetType.Team]: 'Team',
    [TargetType.Queue]: 'Queue',
    [TargetType.Territory]: 'Territory',
  };
  return labels[type] || type;
};

const getOperatorLabel = (op: CriteriaOperator): string => {
  const labels: Record<CriteriaOperator, string> = {
    [CriteriaOperator.Equals]: 'Equals',
    [CriteriaOperator.NotEquals]: 'Not Equals',
    [CriteriaOperator.Contains]: 'Contains',
    [CriteriaOperator.NotContains]: 'Not Contains',
    [CriteriaOperator.StartsWith]: 'Starts With',
    [CriteriaOperator.EndsWith]: 'Ends With',
    [CriteriaOperator.GreaterThan]: 'Greater Than',
    [CriteriaOperator.LessThan]: 'Less Than',
    [CriteriaOperator.GreaterOrEqual]: 'Greater Or Equal',
    [CriteriaOperator.LessOrEqual]: 'Less Or Equal',
    [CriteriaOperator.In]: 'In List',
    [CriteriaOperator.NotIn]: 'Not In List',
    [CriteriaOperator.IsEmpty]: 'Is Empty',
    [CriteriaOperator.IsNotEmpty]: 'Is Not Empty',
    [CriteriaOperator.Between]: 'Between',
  };
  return labels[op] || op;
};

// ==================== MAIN COMPONENT ====================

const LeadRoutingPage = () => {
  const { profile } = useProfile();
  const [activeTab, setActiveTab] = useState(0);
  const [rules, setRules] = useState<LeadRoutingRule[]>([]);
  const [history, setHistory] = useState<RoutingLog[]>([]);
  const [statistics, setStatistics] = useState<RoutingStatistics | null>(null);
  const [workload, setWorkload] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Dialog states
  const [editDialog, setEditDialog] = useState(false);
  const [deleteDialog, setDeleteDialog] = useState(false);
  const [historyDialog, setHistoryDialog] = useState(false);
  const [selectedRule, setSelectedRule] = useState<LeadRoutingRule | null>(null);
  const [formData, setFormData] = useState<Partial<LeadRoutingRule>>({});
  const [saving, setSaving] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [historyTotal, setHistoryTotal] = useState(0);
  const [historyPage, setHistoryPage] = useState(1);

  // Load data
  const loadRules = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await leadRoutingService.getAllRules(true);
      setRules(res.data);
    } catch (err: any) {
      setError(err.message || 'Failed to load routing rules');
    } finally {
      setLoading(false);
    }
  }, []);

  const loadStatistics = useCallback(async () => {
    try {
      const res = await leadRoutingService.getStatistics();
      setStatistics(res.data);
    } catch (err: any) {
      console.error('Failed to load statistics:', err);
    }
  }, []);

  const loadHistory = useCallback(async (page: number = 1) => {
    try {
      const res = await leadRoutingService.getAllHistory(page, 20);
      setHistory(res.data.items);
      setHistoryTotal(res.data.totalCount);
      setHistoryPage(page);
    } catch (err: any) {
      console.error('Failed to load history:', err);
    }
  }, []);

  const loadWorkload = useCallback(async () => {
    try {
      const res = await leadRoutingService.getAllUsersWorkload();
      setWorkload(res.data);
    } catch (err: any) {
      console.error('Failed to load workload:', err);
    }
  }, []);

  useEffect(() => {
    loadRules();
    loadStatistics();
  }, [loadRules, loadStatistics]);

  useEffect(() => {
    if (activeTab === 1) loadHistory();
    if (activeTab === 2) loadWorkload();
  }, [activeTab, loadHistory, loadWorkload]);

  // Handlers
  const handleAddRule = () => {
    setSelectedRule(null);
    setFormData({
      name: '',
      description: '',
      isActive: true,
      priority: rules.length + 1,
      routingType: RoutingType.RoundRobin,
      targetType: TargetType.User,
      criteria: [],
      workingHoursOnly: false,
    });
    setEditDialog(true);
  };

  const handleEditRule = (rule: LeadRoutingRule) => {
    setSelectedRule(rule);
    setFormData(rule);
    setEditDialog(true);
  };

  const handleDeleteRule = (rule: LeadRoutingRule) => {
    setSelectedRule(rule);
    setDeleteDialog(true);
  };

  const handleToggleRule = async (rule: LeadRoutingRule) => {
    try {
      if (rule?.isActive !== false) {
        await leadRoutingService.disableRule(rule.id!);
      } else {
        await leadRoutingService.enableRule(rule.id!);
      }
      loadRules();
      setSuccessMessage(`Rule ${rule?.isActive !== false ? 'disabled' : 'enabled'} successfully`);
    } catch (err: any) {
      setError(err.message || 'Failed to update rule');
    }
  };

  const handleSaveRule = async () => {
    if (!formData.name) return;
    setSaving(true);
    try {
      if (selectedRule?.id) {
        await leadRoutingService.updateRule(selectedRule.id, formData);
        setSuccessMessage('Rule updated successfully');
      } else {
        await leadRoutingService.createRule(formData as any);
        setSuccessMessage('Rule created successfully');
      }
      setEditDialog(false);
      loadRules();
    } catch (err: any) {
      setError(err.message || 'Failed to save rule');
    } finally {
      setSaving(false);
    }
  };

  const handleConfirmDelete = async () => {
    if (!selectedRule?.id) return;
    try {
      await leadRoutingService.deleteRule(selectedRule.id);
      setSuccessMessage('Rule deleted successfully');
      setDeleteDialog(false);
      loadRules();
    } catch (err: any) {
      setError(err.message || 'Failed to delete rule');
    }
  };

  const handleCloneRule = async (rule: LeadRoutingRule) => {
    try {
      await leadRoutingService.cloneRule(rule.id!, `${rule.name} (Copy)`);
      setSuccessMessage('Rule cloned successfully');
      loadRules();
    } catch (err: any) {
      setError(err.message || 'Failed to clone rule');
    }
  };

  const handleRouteUnassigned = async () => {
    setLoading(true);
    try {
      const res = await leadRoutingService.routeUnassignedLeads();
      setSuccessMessage(`Routed ${res.data.successCount} leads successfully`);
      loadStatistics();
      loadHistory();
    } catch (err: any) {
      setError(err.message || 'Failed to route leads');
    } finally {
      setLoading(false);
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

  const { paginatedData: paginatedRules, page, pageSize, handlePageChange, handlePageSizeChange, pageSizeOptions } =
    usePagination(rules, { defaultPageSize: 25 });

  return (
    <Box sx={{ minHeight: '100vh', backgroundColor: '#f5f5f5' }}>
      {/* Header */}
      <Box sx={{ backgroundColor: '#1976d2', color: 'white', py: 3 }}>
        <Container maxWidth="xl">
          <Stack direction="row" justifyContent="space-between" alignItems="center">
            <Stack direction="row" spacing={2} alignItems="center">
              <img src={logo} alt="CRM" style={{ height: 40 }} />
              <Box>
                <Typography variant="h5">Lead Routing</Typography>
                <Typography variant="body2" sx={{ opacity: 0.8 }}>
                  Configure automatic lead assignment rules and track routing history
                </Typography>
              </Box>
            </Stack>
            <Stack direction="row" spacing={1}>
              <Tooltip title="Route all unassigned leads">
                <Button
                  variant="outlined"
                  startIcon={<RouteIcon />}
                  onClick={handleRouteUnassigned}
                  disabled={loading}
                  sx={{ color: 'white', borderColor: 'white' }}
                >
                  Route Unassigned
                </Button>
              </Tooltip>
              <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={handleAddRule}
                sx={{ backgroundColor: 'white', color: '#1976d2' }}
              >
                New Rule
              </Button>
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
                title="Active Rules"
                value={statistics.activeRules}
                subtitle={`of ${statistics.totalRules} total`}
                icon={<RuleIcon sx={{ fontSize: 40 }} />}
                color="#1976d2"
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <StatsCard
                title="Leads Routed (24h)"
                value={statistics.routedLast24Hours}
                subtitle={`${statistics.routedLast7Days} this week`}
                icon={<RouteIcon sx={{ fontSize: 40 }} />}
                color="#2e7d32"
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <StatsCard
                title="Success Rate"
                value={`${((statistics?.successRate ?? 0) * 100).toFixed(1)}%`}
                subtitle={`${statistics?.totalRoutedLeads ?? 0} total routed`}
                icon={<SuccessIcon sx={{ fontSize: 40 }} />}
                color="#ed6c02"
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <StatsCard
                title="Avg Processing"
                value={`${statistics.avgProcessingTimeMs?.toFixed(0) || 0}ms`}
                icon={<SpeedIcon sx={{ fontSize: 40 }} />}
                color="#9c27b0"
              />
            </Grid>
          </Grid>
        )}

        {/* Tabs */}
        <Paper>
          <Tabs value={activeTab} onChange={(_, v) => setActiveTab(v)}>
            <Tab icon={<RuleIcon />} label="Rules" />
            <Tab icon={<HistoryIcon />} label="History" />
            <Tab icon={<BalanceIcon />} label="Workload" />
            <Tab icon={<StatsIcon />} label="Analytics" />
          </Tabs>

          {loading && <LinearProgress />}

          {/* Rules Tab */}
          <TabPanel value={activeTab} index={0}>
            <Box sx={{ p: 2 }}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Priority</TableCell>
                    <TableCell>Name</TableCell>
                    <TableCell>Type</TableCell>
                    <TableCell>Target</TableCell>
                    <TableCell>Criteria</TableCell>
                    <TableCell align="center">Triggered</TableCell>
                    <TableCell align="center">Status</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {paginatedRules.map(rule => (
                    <TableRow key={rule.id}>
                      <TableCell>
                        <Chip label={rule.priority} size="small" color="primary" variant="outlined" />
                      </TableCell>
                      <TableCell>
                        <Typography fontWeight={500}>{rule.name}</Typography>
                        {rule.description && (
                          <Typography variant="caption" color="text.secondary">
                            {rule.description}
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell>
                        <Chip label={getRoutingTypeLabel(rule.routingType)} size="small" />
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {getTargetTypeLabel(rule.targetType)}
                          {rule.targetName && `: ${rule.targetName}`}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        {rule.criteria?.length ? (
                          <Stack spacing={0.5}>
                            {rule.criteria.slice(0, 2).map((c, i) => (
                              <Typography key={i} variant="caption">
                                {c.fieldLabel || c.field} {getOperatorLabel(c.operator)} {c.value}
                              </Typography>
                            ))}
                            {rule.criteria.length > 2 && (
                              <Typography variant="caption" color="text.secondary">
                                +{rule.criteria.length - 2} more
                              </Typography>
                            )}
                          </Stack>
                        ) : (
                          <Typography variant="caption" color="text.secondary">All leads</Typography>
                        )}
                      </TableCell>
                      <TableCell align="center">{rule.triggerCount || 0}</TableCell>
                      <TableCell align="center">
                        <Chip
                          label={rule?.isActive !== false ? 'Active' : 'Inactive'}
                          color={rule?.isActive !== false ? 'success' : 'default'}
                          size="small"
                          onClick={() => handleToggleRule(rule)}
                          sx={{ cursor: 'pointer' }}
                        />
                      </TableCell>
                      <TableCell align="right">
                        <IconButton size="small" onClick={() => handleEditRule(rule)}>
                          <EditIcon fontSize="small" />
                        </IconButton>
                        <IconButton size="small" onClick={() => handleCloneRule(rule)}>
                          <CopyIcon fontSize="small" />
                        </IconButton>
                        <IconButton size="small" onClick={() => handleDeleteRule(rule)}>
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))}
                  {rules.length === 0 && !loading && (
                    <TableRow>
                      <TableCell colSpan={8} align="center">
                        <Typography color="text.secondary" sx={{ py: 4 }}>
                          No routing rules found. Click "New Rule" to create one.
                        </Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
              <TablePagination
                component="div"
                count={rules.length}
                page={page}
                onPageChange={handlePageChange}
                rowsPerPage={pageSize}
                onRowsPerPageChange={handlePageSizeChange}
                rowsPerPageOptions={pageSizeOptions}
              />
            </Box>
          </TabPanel>

          {/* History Tab */}
          <TabPanel value={activeTab} index={1}>
            <Box sx={{ p: 2 }}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Time</TableCell>
                    <TableCell>Lead</TableCell>
                    <TableCell>Rule</TableCell>
                    <TableCell>Assigned To</TableCell>
                    <TableCell>Reason</TableCell>
                    <TableCell align="center">Status</TableCell>
                    <TableCell align="right">Processing</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {history.map(log => (
                    <TableRow key={log.id}>
                      <TableCell>
                        <Typography variant="body2">
                          {new Date(log.routedAt).toLocaleString()}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography fontWeight={500}>{log.leadName || `Lead #${log.leadId}`}</Typography>
                        {log.leadEmail && (
                          <Typography variant="caption" color="text.secondary">
                            {log.leadEmail}
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell>{log.ruleName || 'Manual'}</TableCell>
                      <TableCell>{log.newOwnerName || `User #${log.newOwnerId}`}</TableCell>
                      <TableCell>
                        <Typography variant="body2" sx={{ maxWidth: 200 }} noWrap>
                          {log.reason}
                        </Typography>
                      </TableCell>
                      <TableCell align="center">
                        {log.success ? (
                          <SuccessIcon color="success" fontSize="small" />
                        ) : (
                          <Tooltip title={log.errorMessage}>
                            <ErrorIcon color="error" fontSize="small" />
                          </Tooltip>
                        )}
                      </TableCell>
                      <TableCell align="right">{log.processingTimeMs ?? 0}ms</TableCell>
                    </TableRow>
                  ))}
                  {history.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={7} align="center">
                        <Typography color="text.secondary" sx={{ py: 4 }}>No routing history</Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </Box>
          </TabPanel>

          {/* Workload Tab */}
          <TabPanel value={activeTab} index={2}>
            <Box sx={{ p: 2 }}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>User</TableCell>
                    <TableCell align="center">Pending Leads</TableCell>
                    <TableCell align="center">Today</TableCell>
                    <TableCell align="center">Capacity</TableCell>
                    <TableCell>Utilization</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {workload.map(user => (
                    <TableRow key={user.userId}>
                      <TableCell>
                        <Stack direction="row" spacing={1} alignItems="center">
                          <PeopleIcon color="action" />
                          <Typography>{user.userName || `User #${user.userId}`}</Typography>
                        </Stack>
                      </TableCell>
                      <TableCell align="center">{user.pendingLeads ?? 0}</TableCell>
                      <TableCell align="center">{user.leadsToday ?? 0}</TableCell>
                      <TableCell align="center">{user.capacity ?? 0}</TableCell>
                      <TableCell>
                        <Stack direction="row" alignItems="center" spacing={1}>
                          <LinearProgress
                            variant="determinate"
                            value={Math.min(user.utilizationPercentage ?? 0, 100)}
                            sx={{ flexGrow: 1, height: 8, borderRadius: 4 }}
                            color={
                              (user.utilizationPercentage ?? 0) >= 100 ? 'error' :
                              (user.utilizationPercentage ?? 0) >= 80 ? 'warning' : 'primary'
                            }
                          />
                          <Typography variant="body2" sx={{ minWidth: 40 }}>
                            {(user.utilizationPercentage ?? 0).toFixed(0)}%
                          </Typography>
                        </Stack>
                      </TableCell>
                    </TableRow>
                  ))}
                  {workload.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={5} align="center">
                        <Typography color="text.secondary" sx={{ py: 4 }}>No workload data</Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
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
                        <Typography variant="h6" gutterBottom>Routing by Type</Typography>
                        <List>
                          {(statistics.routingByType || []).map(item => (
                            <ListItem key={item.type}>
                              <ListItemText
                                primary={getRoutingTypeLabel(item.type)}
                                secondary={`${item.count ?? 0} leads (${(item.percentage ?? 0).toFixed(1)}%)`}
                              />
                              <LinearProgress
                                variant="determinate"
                                value={item.percentage ?? 0}
                                sx={{ width: 100, height: 8, borderRadius: 4 }}
                              />
                            </ListItem>
                          ))}
                          {(statistics.routingByType || []).length === 0 && (
                            <Typography color="text.secondary" sx={{ py: 2, textAlign: 'center' }}>
                              No routing data available
                            </Typography>
                          )}
                        </List>
                      </CardContent>
                    </Card>
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <Card>
                      <CardContent>
                        <Typography variant="h6" gutterBottom>Top Users by Leads Received</Typography>
                        <List>
                          {(statistics.routingByUser || []).slice(0, 5).map(user => (
                            <ListItem key={user.userId}>
                              <ListItemIcon><PeopleIcon /></ListItemIcon>
                              <ListItemText
                                primary={user.userName || `User #${user.userId}`}
                                secondary={`${user.leadsReceived ?? 0} leads (${(user.percentage ?? 0).toFixed(1)}%)`}
                              />
                            </ListItem>
                          ))}
                          {(statistics.routingByUser || []).length === 0 && (
                            <Typography color="text.secondary" sx={{ py: 2, textAlign: 'center' }}>
                              No user routing data available
                            </Typography>
                          )}
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

      {/* Edit/Create Dialog */}
      <Dialog open={editDialog} onClose={() => setEditDialog(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          {selectedRule ? 'Edit Routing Rule' : 'Create Routing Rule'}
        </DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 0.5 }}>
            <Grid item xs={12} md={8}>
              <TextField
                label="Name"
                value={formData.name || ''}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                fullWidth
                required
              />
            </Grid>
            <Grid item xs={12} md={4}>
              <TextField
                label="Priority"
                type="number"
                value={formData.priority || 1}
                onChange={(e) => setFormData({ ...formData, priority: parseInt(e.target.value) })}
                fullWidth
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                label="Description"
                value={formData.description || ''}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                fullWidth
                multiline
                rows={2}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Routing Type</InputLabel>
                <Select
                  value={formData.routingType || RoutingType.RoundRobin}
                  label="Routing Type"
                  onChange={(e: SelectChangeEvent) => setFormData({ ...formData, routingType: e.target.value as RoutingType })}
                >
                  {Object.values(RoutingType).map(type => (
                    <MenuItem key={type} value={type}>{getRoutingTypeLabel(type)}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Target Type</InputLabel>
                <Select
                  value={formData.targetType || TargetType.User}
                  label="Target Type"
                  onChange={(e: SelectChangeEvent) => setFormData({ ...formData, targetType: e.target.value as TargetType })}
                >
                  {Object.values(TargetType).map(type => (
                    <MenuItem key={type} value={type}>{getTargetTypeLabel(type)}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <Divider sx={{ my: 1 }} />
              <FormControlLabel
                control={
                  <Switch
                    checked={formData.workingHoursOnly ?? false}
                    onChange={(e) => setFormData({ ...formData, workingHoursOnly: e.target.checked })}
                  />
                }
                label="Apply during working hours only"
              />
            </Grid>
            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Switch
                    checked={formData?.isActive !== false}
                    onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                  />
                }
                label="Active"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditDialog(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSaveRule} disabled={saving || !formData.name}>
            {saving ? <CircularProgress size={20} /> : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Dialog */}
      <Dialog open={deleteDialog} onClose={() => setDeleteDialog(false)}>
        <DialogTitle>Delete Routing Rule</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete the rule "{selectedRule?.name}"?
            This action cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialog(false)}>Cancel</Button>
          <Button variant="contained" color="error" onClick={handleConfirmDelete}>
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default LeadRoutingPage;

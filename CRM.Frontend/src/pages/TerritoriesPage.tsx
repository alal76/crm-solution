/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 * Licensed under the Source-Available License (see LICENSE) v3.0
 */

import { useState, useEffect, useCallback } from 'react';
import {
  Box, Container, Typography, Card, CardContent, Table, TableBody, TableCell,
  TableHead, TableRow, Button, Dialog, DialogTitle, DialogContent, DialogActions,
  TextField, Stack, Chip, IconButton, CircularProgress,
  Alert, Grid, Tabs, Tab, FormControl, InputLabel, Select, MenuItem,
  Paper, LinearProgress, Accordion, AccordionSummary, AccordionDetails,
  List, ListItem, ListItemText, ListItemIcon, FormControlLabel, Switch,
  SelectChangeEvent, TablePagination,
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon,
  Map as MapIcon, Business as BusinessIcon, People as PeopleIcon,
  TrendingUp as TrendingUpIcon, ExpandMore, Refresh as RefreshIcon,
  AccountTree as HierarchyIcon, Rule as RuleIcon, Flag as QuotaIcon,
  LocationOn as LocationIcon,
} from '@mui/icons-material';
import { DialogError, ActionButton, TabPanel } from '../components/common';
import { useApiState } from '../hooks/useApiState';
import { useProfile } from '../contexts/ProfileContext';
import territoryService, {
  Territory, TerritoryStatistics, TerritoryHierarchy, TerritoryRule, TerritoryQuota
} from '../services/territoryService';
import logo from '../assets/logo.png';
import { usePagination } from '../hooks/usePagination';

// ==================== DATA NORMALIZATION ====================
// Backend returns AccountTerritory entity with different property names.
// These helpers map backend responses to the frontend interfaces.

const normalizeTerritory = (t: any): Territory => ({
  ...t,
  id: t.id,
  name: t.name || t.territoryName || '',
  code: t.code || t.territoryCode || '',
  description: t.description || '',
  region: t.region || t.regions || '',
  country: t.country || t.countries || '',
  isActive: t.isActive ?? true,
  managerId: t.managerId || t.primaryOwnerId,
  accountCount: t.accountCount ?? 0,
  opportunityCount: t.opportunityCount ?? 0,
});

const normalizeHierarchy = (h: any): TerritoryHierarchy => ({
  territory: h.territory ? normalizeTerritory(h.territory) : normalizeTerritory(h),
  children: (h.children || []).map((c: any) => normalizeHierarchy(c)),
  level: h.level ?? 0,
  totalAccounts: h.totalAccounts ?? 0,
  totalPipelineValue: h.totalPipelineValue ?? 0,
});

const normalizeStatistics = (s: any): TerritoryStatistics | null => {
  if (!s) return null;
  return {
    ...s,
    territoryId: s.territoryId,
    territoryName: s.territoryName || '',
    totalAccounts: s.totalAccounts ?? 0,
    activeOpportunities: s.activeOpportunities ?? s.totalOpportunities ?? 0,
    totalPipelineValue: s.totalPipelineValue ?? s.pipelineValue ?? 0,
    closedWonValue: s.closedWonValue ?? s.totalRevenue ?? 0,
    closedLostValue: s.closedLostValue ?? 0,
    avgDealSize: s.avgDealSize ?? s.averageAccountValue ?? 0,
    winRate: s.winRate ?? 0,
    quotaAttainment: s.quotaAttainment ?? 0,
    teamMemberCount: s.teamMemberCount ?? 0,
    accountsBySegment: s.accountsBySegment ?? [],
    monthlyTrend: s.monthlyTrend ?? [],
  };
};

const toBackendTerritory = (t: Partial<Territory>): any => ({
  ...t,
  territoryName: t.name,
  territoryCode: t.code,
  regions: t.region,
  countries: t.country,
  primaryOwnerId: t.managerId,
});

// ==================== MAIN COMPONENT ====================

const TerritoriesPage = () => {
  const { profile } = useProfile();
  const [activeTab, setActiveTab] = useState(0);
  const [territories, setTerritories] = useState<Territory[]>([]);
  const [hierarchy, setHierarchy] = useState<TerritoryHierarchy[]>([]);
  const [statistics, setStatistics] = useState<TerritoryStatistics | null>(null);
  const [rules, setRules] = useState<TerritoryRule[]>([]);
  const [quotas, setQuotas] = useState<TerritoryQuota[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Dialog states
  const [editDialog, setEditDialog] = useState(false);
  const [deleteDialog, setDeleteDialog] = useState(false);
  const [selectedTerritory, setSelectedTerritory] = useState<Territory | null>(null);
  const [formData, setFormData] = useState<Partial<Territory>>({});
  const [saving, setSaving] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Load data
  const loadData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [territoriesRes, hierarchyRes, statsRes] = await Promise.all([
        territoryService.getAll(),
        territoryService.getHierarchy(),
        territoryService.getAllStatistics(),
      ]);
      setTerritories((territoriesRes.data || []).map(normalizeTerritory));
      setHierarchy((hierarchyRes.data || []).map(normalizeHierarchy));
      // Use first territory's statistics or null if no territories
      const statsArr = statsRes.data || [];
      setStatistics(statsArr.length > 0 ? normalizeStatistics(statsArr[0]) : null);
    } catch (err: unknown) {
      setError((err as Error).message || 'Failed to load territories');
    } finally {
      setLoading(false);
    }
  }, []);

  const loadRules = useCallback(async () => {
    // Rules are loaded per territory when selected
    if (!selectedTerritory?.id) {
      setRules([]);
      return;
    }
    try {
      const res = await territoryService.getRules(selectedTerritory.id);
      setRules(res.data);
    } catch (err: unknown) {
      console.error('Failed to load rules:', err);
    }
  }, [selectedTerritory?.id]);

  const loadQuotas = useCallback(async () => {
    // Quotas are loaded per territory when selected
    if (!selectedTerritory?.id) {
      setQuotas([]);
      return;
    }
    try {
      const res = await territoryService.getQuotas(selectedTerritory.id);
      setQuotas(res.data);
    } catch (err: unknown) {
      console.error('Failed to load quotas:', err);
    }
  }, [selectedTerritory?.id]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  useEffect(() => {
    if (activeTab === 2) loadRules();
    if (activeTab === 3) loadQuotas();
  }, [activeTab, loadRules, loadQuotas]);

  // Handlers
  const handleAddTerritory = () => {
    setSelectedTerritory(null);
    setFormData({
      name: '',
      description: '',
      region: '',
      country: '',
      isActive: true,
    });
    setEditDialog(true);
  };

  const handleEditTerritory = (territory: Territory) => {
    setSelectedTerritory(territory);
    setFormData(territory);
    setEditDialog(true);
  };

  const handleDeleteTerritory = (territory: Territory) => {
    setSelectedTerritory(territory);
    setDeleteDialog(true);
  };

  const handleSaveTerritory = async () => {
    if (!formData.name) return;
    setSaving(true);
    try {
      const backendData = toBackendTerritory(formData);
      if (selectedTerritory?.id) {
        backendData.id = selectedTerritory.id;
        await territoryService.update(selectedTerritory.id, backendData);
        setSuccessMessage('Territory updated successfully');
      } else {
        await territoryService.create(backendData);
        setSuccessMessage('Territory created successfully');
      }
      setEditDialog(false);
      loadData();
    } catch (err: unknown) {
      setError((err as Error).message || 'Failed to save territory');
    } finally {
      setSaving(false);
    }
  };

  const handleConfirmDelete = async () => {
    if (!selectedTerritory?.id) return;
    try {
      await territoryService.delete(selectedTerritory.id);
      setSuccessMessage('Territory deleted successfully');
      setDeleteDialog(false);
      loadData();
    } catch (err: unknown) {
      setError((err as Error).message || 'Failed to delete territory');
    }
  };

  // Render hierarchy tree
  const renderHierarchyNode = (node: TerritoryHierarchy, depth: number = 0) => (
    <Box key={node.territory.id} sx={{ ml: depth * 3 }}>
      <Accordion defaultExpanded={depth < 2}>
        <AccordionSummary expandIcon={node.children?.length ? <ExpandMore /> : null}>
          <Stack direction="row" spacing={2} alignItems="center" sx={{ width: '100%' }}>
            <LocationIcon color={node.territory?.isActive !== false ? 'primary' : 'disabled'} />
            <Typography fontWeight={depth === 0 ? 600 : 400}>{node.territory.name}</Typography>
            <Chip label={`${node.totalAccounts} accounts`} size="small" />
            <Chip label={node.territory.region || 'No region'} size="small" variant="outlined" />
          </Stack>
        </AccordionSummary>
        {node.children?.length > 0 && (
          <AccordionDetails>
            {node.children.map(child => renderHierarchyNode(child, depth + 1))}
          </AccordionDetails>
        )}
      </Accordion>
    </Box>
  );

  // Statistics cards
  const StatsCard = ({ title, value, icon, color }: { title: string; value: string | number; icon: React.ReactNode; color: string }) => (
    <Card>
      <CardContent>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Box>
            <Typography variant="caption" color="text.secondary">{title}</Typography>
            <Typography variant="h4">{value}</Typography>
          </Box>
          <Box sx={{ color, opacity: 0.8 }}>{icon}</Box>
        </Stack>
      </CardContent>
    </Card>
  );

  const { paginatedData: paginatedTerritories, page, pageSize, handlePageChange, handlePageSizeChange, pageSizeOptions } =
    usePagination(territories, { defaultPageSize: 25 });

  return (
    <Box sx={{ minHeight: '100vh', backgroundColor: '#f5f5f5' }}>
      {/* Header */}
      <Box sx={{ backgroundColor: '#1976d2', color: 'white', py: 3 }}>
        <Container maxWidth="xl">
          <Stack direction="row" justifyContent="space-between" alignItems="center">
            <Stack direction="row" spacing={2} alignItems="center">
              <img src={logo} alt="CRM" style={{ height: 40 }} />
              <Box>
                <Typography variant="h5">Territory Management</Typography>
                <Typography variant="body2" sx={{ opacity: 0.8 }}>
                  Manage sales territories, regions, and account assignments
                </Typography>
              </Box>
            </Stack>
            <Stack direction="row" spacing={1}>
              <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={handleAddTerritory}
                sx={{ backgroundColor: 'white', color: '#1976d2' }}
              >
                New Territory
              </Button>
            </Stack>
          </Stack>
        </Container>
      </Box>

      <Container maxWidth="xl" sx={{ py: 3 }}>
        {/* Messages */}
        {error && <Alert severity="error" onClose={() => setError(null)} sx={{ mb: 2 }}>{error}</Alert>}
        {successMessage && <Alert severity="success" onClose={() => setSuccessMessage(null)} sx={{ mb: 2 }}>{successMessage}</Alert>}

        {/* Stats - calculated from territories list */}
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={6} md={3}>
            <StatsCard
              title="Total Territories"
              value={territories.length}
              icon={<MapIcon sx={{ fontSize: 40 }} />}
              color="#1976d2"
            />
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <StatsCard
              title="Active Territories"
              value={territories.filter(t => t?.isActive !== false).length}
              icon={<BusinessIcon sx={{ fontSize: 40 }} />}
              color="#2e7d32"
            />
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <StatsCard
              title="Total Accounts"
              value={statistics?.totalAccounts || 0}
              icon={<PeopleIcon sx={{ fontSize: 40 }} />}
              color="#ed6c02"
            />
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <StatsCard
              title="Pipeline Value"
              value={`$${(statistics?.totalPipelineValue || 0).toLocaleString()}`}
              icon={<TrendingUpIcon sx={{ fontSize: 40 }} />}
              color="#9c27b0"
            />
          </Grid>
        </Grid>

        {/* Tabs */}
        <Paper>
          <Tabs value={activeTab} onChange={(_, v) => setActiveTab(v)}>
            <Tab icon={<MapIcon />} label="Territories" />
            <Tab icon={<HierarchyIcon />} label="Hierarchy" />
            <Tab icon={<RuleIcon />} label="Rules" />
            <Tab icon={<QuotaIcon />} label="Quotas" />
          </Tabs>

          {loading && <LinearProgress />}

          {/* Territories Tab */}
          <TabPanel value={activeTab} index={0}>
            <Box sx={{ p: 2 }}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Name</TableCell>
                    <TableCell>Region</TableCell>
                    <TableCell>Country</TableCell>
                    <TableCell align="center">Accounts</TableCell>
                    <TableCell align="center">Opportunities</TableCell>
                    <TableCell align="center">Status</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {paginatedTerritories.map(territory => (
                    <TableRow key={territory.id}>
                      <TableCell>
                        <Typography fontWeight={500}>{territory.name}</Typography>
                        {territory.description && (
                          <Typography variant="caption" color="text.secondary">
                            {territory.description}
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell>{territory.region || '-'}</TableCell>
                      <TableCell>{territory.country || '-'}</TableCell>
                      <TableCell align="center">{territory.accountCount || 0}</TableCell>
                      <TableCell align="center">{territory.opportunityCount || 0}</TableCell>
                      <TableCell align="center">
                        <Chip
                          label={territory?.isActive !== false ? 'Active' : 'Inactive'}
                          color={territory?.isActive !== false ? 'primary' : 'default'}
                          size="small"
                        />
                      </TableCell>
                      <TableCell align="right">
                        <IconButton size="small" onClick={() => handleEditTerritory(territory)}>
                          <EditIcon fontSize="small" />
                        </IconButton>
                        <IconButton size="small" onClick={() => handleDeleteTerritory(territory)}>
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))}
                  {territories.length === 0 && !loading && (
                    <TableRow>
                      <TableCell colSpan={7} align="center">
                        <Typography color="text.secondary" sx={{ py: 4 }}>
                          No territories found. Click "New Territory" to create one.
                        </Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
              <TablePagination
                component="div"
                count={territories.length}
                page={page}
                onPageChange={handlePageChange}
                rowsPerPage={pageSize}
                onRowsPerPageChange={handlePageSizeChange}
                rowsPerPageOptions={pageSizeOptions}
              />
            </Box>
          </TabPanel>

          {/* Hierarchy Tab */}
          <TabPanel value={activeTab} index={1}>
            <Box sx={{ p: 2 }}>
              {hierarchy.length > 0 ? (
                hierarchy.map(node => renderHierarchyNode(node))
              ) : (
                <Typography color="text.secondary" align="center" sx={{ py: 4 }}>
                  No hierarchy data available
                </Typography>
              )}
            </Box>
          </TabPanel>

          {/* Rules Tab */}
          <TabPanel value={activeTab} index={2}>
            <Box sx={{ p: 2 }}>
              <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
                <Typography variant="h6">Assignment Rules</Typography>
                <Button variant="outlined" startIcon={<AddIcon />}>Add Rule</Button>
              </Stack>
              {rules.length > 0 ? (
                <List>
                  {rules.map(rule => (
                    <ListItem key={rule.id}>
                      <ListItemIcon><RuleIcon /></ListItemIcon>
                      <ListItemText
                        primary={`${rule.field} ${rule.operator} ${rule.value}`}
                        secondary={`Priority: ${rule.priority} | Type: ${rule.ruleType}`}
                      />
                      <Chip
                        label={rule?.isActive !== false ? 'Active' : 'Inactive'}
                        color={rule?.isActive !== false ? 'success' : 'default'}
                        size="small"
                      />
                    </ListItem>
                  ))}
                </List>
              ) : (
                <Typography color="text.secondary" align="center" sx={{ py: 4 }}>
                  No rules configured
                </Typography>
              )}
            </Box>
          </TabPanel>

          {/* Quotas Tab */}
          <TabPanel value={activeTab} index={3}>
            <Box sx={{ p: 2 }}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Territory</TableCell>
                    <TableCell>Period</TableCell>
                    <TableCell align="right">Target</TableCell>
                    <TableCell align="right">Achieved</TableCell>
                    <TableCell align="center">Progress</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {quotas.map(quota => {
                    const progress = quota.revenueTarget ? ((quota.actualRevenue || 0) / quota.revenueTarget) * 100 : 0;
                    const period = quota.month 
                      ? `${quota.year}-${String(quota.month).padStart(2, '0')}` 
                      : quota.quarter 
                        ? `${quota.year} Q${quota.quarter}` 
                        : `${quota.year}`;
                    return (
                      <TableRow key={quota.id}>
                        <TableCell>{quota.territoryName}</TableCell>
                        <TableCell>{period}</TableCell>
                        <TableCell align="right">${quota.revenueTarget?.toLocaleString()}</TableCell>
                        <TableCell align="right">${(quota.actualRevenue || 0)?.toLocaleString()}</TableCell>
                        <TableCell>
                          <Stack direction="row" alignItems="center" spacing={1}>
                            <LinearProgress
                              variant="determinate"
                              value={Math.min(progress, 100)}
                              sx={{ flexGrow: 1, height: 8, borderRadius: 4 }}
                              color={progress >= 100 ? 'success' : progress >= 75 ? 'primary' : 'warning'}
                            />
                            <Typography variant="body2">{progress.toFixed(0)}%</Typography>
                          </Stack>
                        </TableCell>
                      </TableRow>
                    );
                  })}
                  {quotas.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={5} align="center">
                        <Typography color="text.secondary" sx={{ py: 4 }}>No quotas configured</Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </Box>
          </TabPanel>
        </Paper>
      </Container>

      {/* Edit/Create Dialog */}
      <Dialog open={editDialog} onClose={() => setEditDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          {selectedTerritory ? 'Edit Territory' : 'Create Territory'}
        </DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label="Name"
              value={formData.name || ''}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              fullWidth
              required
            />
            <TextField
              label="Description"
              value={formData.description || ''}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              fullWidth
              multiline
              rows={2}
            />
            <TextField
              label="Region"
              value={formData.region || ''}
              onChange={(e) => setFormData({ ...formData, region: e.target.value })}
              fullWidth
            />
            <TextField
              label="Country"
              value={formData.country || ''}
              onChange={(e) => setFormData({ ...formData, country: e.target.value })}
              fullWidth
            />
            <FormControlLabel
              control={
                <Switch
                  checked={formData?.isActive !== false}
                  onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                />
              }
              label="Active"
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditDialog(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSaveTerritory} disabled={saving || !formData.name}>
            {saving ? <CircularProgress size={20} /> : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Dialog */}
      <Dialog open={deleteDialog} onClose={() => setDeleteDialog(false)}>
        <DialogTitle>Delete Territory</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete the territory "{selectedTerritory?.name}"?
            This will also remove all account assignments.
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

export default TerritoriesPage;

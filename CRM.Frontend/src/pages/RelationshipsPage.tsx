/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Relationships Page - B2B/B2C Relationship Management
 */

import { useState, useEffect, useCallback, useMemo } from 'react';
import {
  Box, Card, CardContent, Typography, Button, Table, TableBody, TableCell, TableHead,
  TableRow, Dialog, DialogTitle, DialogContent, DialogActions, Alert, CircularProgress,
  TextField, Container, FormControl, InputLabel, Select, MenuItem, Chip, Tabs, Tab,
  Grid, IconButton, Tooltip, LinearProgress, Paper, Stack, Autocomplete, TablePagination
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon,
  Handshake as RelationshipIcon, TrendingUp as TrendingUpIcon,
  AccountTree as MapIcon, HealthAndSafety as HealthIcon,
  Visibility as ViewIcon, Close as CloseIcon, Refresh as RefreshIcon
} from '@mui/icons-material';
import { useApiState } from '../hooks/useApiState';
import { useProfile } from '../contexts/ProfileContext';
import apiClient from '../services/apiClient';
import { usePagination } from '../hooks/usePagination';
import relationshipService, {
  RelationshipType, AccountRelationship, RelationshipInteraction,
  AccountHealthSnapshot, RelationshipMapVisualization,
  RelationshipCategory, RelationshipStatus, StrategicImportance,
  InteractionType, InteractionOutcome, HealthImpact, HealthTrend,
  CreateRelationshipTypeRequest, CreateAccountRelationshipRequest, CreateInteractionRequest
} from '../services/relationshipService';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div role="tabpanel" hidden={value !== index} {...other}>
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  );
}

interface Customer {
  id: number;
  firstName?: string;
  lastName?: string;
  company?: string;
}

function RelationshipGraph({ nodes, edges }: {
  nodes: RelationshipMapVisualization['nodes'];
  edges: RelationshipMapVisualization['edges'];
}) {
  const [hoveredId, setHoveredId] = useState<number | null>(null);
  const SVG_W = 700;
  const SVG_H = 460;
  const CX = SVG_W / 2;
  const CY = SVG_H / 2;
  const NODE_R = 30;

  // Position nodes: center node at middle, depth-1 in inner ring, depth-2+ in outer ring
  const positioned = useMemo(() => {
    const center = nodes.find(n => n.isCenter);
    const inner = nodes.filter(n => !n.isCenter && n.depth <= 1);
    const outer = nodes.filter(n => !n.isCenter && n.depth > 1);

    const result: Record<number, { x: number; y: number }> = {};
    if (center) {
      result[center.accountId] = { x: CX, y: CY };
    }

    const placeRing = (items: typeof nodes, radius: number) => {
      const count = items.length;
      if (count === 0) return;
      const step = (2 * Math.PI) / count;
      const startAngle = -Math.PI / 2;
      items.forEach((n, i) => {
        const angle = startAngle + i * step;
        result[n.accountId] = {
          x: CX + radius * Math.cos(angle),
          y: CY + radius * Math.sin(angle),
        };
      });
    };

    placeRing(inner, 140);
    placeRing(outer, 200);

    return result;
  }, [nodes, CX, CY]);

  const healthColor = (score: number | null | undefined) => {
    if (!score) return '#9e9e9e';
    if (score >= 75) return '#4caf50';
    if (score >= 50) return '#ff9800';
    return '#f44336';
  };

  const strengthColor = (score: number) => {
    if (score >= 75) return '#4caf50';
    if (score >= 50) return '#2196f3';
    if (score >= 25) return '#ff9800';
    return '#f44336';
  };

  const truncate = (s: string, max: number) => s.length > max ? s.slice(0, max - 1) + '…' : s;

  return (
    <svg width="100%" height={SVG_H} viewBox={`0 0 ${SVG_W} ${SVG_H}`} style={{ userSelect: 'none' }}>
      <defs>
        <filter id="rel-shadow" x="-20%" y="-20%" width="140%" height="140%">
          <feDropShadow dx="0" dy="1" stdDeviation="2" floodOpacity="0.12" />
        </filter>
      </defs>

      {/* Edges */}
      {edges.map((edge) => {
        const from = positioned[edge.sourceId];
        const to = positioned[edge.targetId];
        if (!from || !to) return null;
        const dx = to.x - from.x;
        const dy = to.y - from.y;
        const dist = Math.sqrt(dx * dx + dy * dy) || 1;
        const ux = dx / dist;
        const uy = dy / dist;
        const x1 = from.x + ux * (NODE_R + 2);
        const y1 = from.y + uy * (NODE_R + 2);
        const x2 = to.x - ux * (NODE_R + 2);
        const y2 = to.y - uy * (NODE_R + 2);
        const midX = (x1 + x2) / 2;
        const midY = (y1 + y2) / 2;
        const highlighted = hoveredId === edge.sourceId || hoveredId === edge.targetId;

        return (
          <g key={`edge-${edge.id}`}>
            <line
              x1={x1} y1={y1} x2={x2} y2={y2}
              stroke={highlighted ? strengthColor(edge.strengthScore) : '#bdbdbd'}
              strokeWidth={highlighted ? 2.5 : 1.5}
              strokeDasharray={edge.strengthScore < 25 ? '4 3' : undefined}
              style={{ transition: 'stroke 0.2s, stroke-width 0.2s' }}
            />
            {/* Edge label */}
            <rect
              x={midX - 36} y={midY - 8} width={72} height={16} rx={4}
              fill="white" stroke="#e0e0e0" strokeWidth={0.5}
              opacity={highlighted ? 1 : 0.85}
            />
            <text
              x={midX} y={midY + 3} textAnchor="middle"
              fontSize={8} fill="#616161" fontWeight={highlighted ? 600 : 400}
            >
              {truncate(edge.relationshipTypeName, 14)}
            </text>
          </g>
        );
      })}

      {/* Nodes */}
      {nodes.map((node) => {
        const pos = positioned[node.accountId];
        if (!pos) return null;
        const isHovered = hoveredId === node.accountId;
        const r = node.isCenter ? NODE_R + 6 : NODE_R;
        const fill = node.isCenter ? '#1565c0' : '#42a5f5';

        return (
          <g
            key={`node-${node.accountId}`}
            transform={`translate(${pos.x}, ${pos.y})`}
            onMouseEnter={() => setHoveredId(node.accountId)}
            onMouseLeave={() => setHoveredId(null)}
            style={{ cursor: 'pointer' }}
          >
            {/* Health ring */}
            <circle
              r={r + 3} fill="none"
              stroke={healthColor(node.healthScore)}
              strokeWidth={2.5}
              opacity={isHovered ? 1 : 0.6}
              style={{ transition: 'opacity 0.2s' }}
            />
            {/* Main circle */}
            <circle
              r={r} fill={fill}
              opacity={isHovered ? 1 : 0.85}
              filter="url(#rel-shadow)"
              style={{ transition: 'opacity 0.2s' }}
            />
            {/* Name */}
            <text textAnchor="middle" dy={node.company ? '-0.3em' : '0.35em'}
              fill="white" fontSize={node.isCenter ? 11 : 10} fontWeight={node.isCenter ? 700 : 600}>
              {truncate(node.name, 12)}
            </text>
            {node.company && (
              <text textAnchor="middle" dy="1.0em" fill="rgba(255,255,255,0.8)" fontSize={8}>
                {truncate(node.company, 14)}
              </text>
            )}
            {/* Tooltip */}
            {isHovered && (
              <g>
                <rect x={-72} y={r + 8} width={144} height={40} rx={6}
                  fill="white" stroke="#e0e0e0" strokeWidth={1} filter="url(#rel-shadow)" />
                <text x={0} y={r + 22} textAnchor="middle" fontSize={10} fill="#212121" fontWeight={600}>
                  {truncate(node.name, 24)}
                </text>
                <text x={0} y={r + 36} textAnchor="middle" fontSize={9} fill="#757575">
                  Health: {node.healthScore ?? 'N/A'} • Depth: {node.depth}
                </text>
              </g>
            )}
          </g>
        );
      })}
    </svg>
  );
}

function RelationshipsPage() {
  const [tabValue, setTabValue] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  
  // Data states
  const [relationshipTypes, setRelationshipTypes] = useState<RelationshipType[]>([]);
  const [relationships, setRelationships] = useState<AccountRelationship[]>([]);
  const [accounts, setAccounts] = useState<Customer[]>([]);
  
  // Dialog states
  const [typeDialogOpen, setTypeDialogOpen] = useState(false);
  const [relationshipDialogOpen, setRelationshipDialogOpen] = useState(false);
  const [interactionDialogOpen, setInteractionDialogOpen] = useState(false);
  const [mapDialogOpen, setMapDialogOpen] = useState(false);
  const [healthDialogOpen, setHealthDialogOpen] = useState(false);
  
  // Edit states
  const [editingTypeId, setEditingTypeId] = useState<number | null>(null);
  const [editingRelationshipId, setEditingRelationshipId] = useState<number | null>(null);
  const [selectedRelationship, setSelectedRelationship] = useState<AccountRelationship | null>(null);
  const [selectedCustomerForMap, setSelectedCustomerForMap] = useState<number | null>(null);
  const [selectedCustomerForHealth, setSelectedCustomerForHealth] = useState<number | null>(null);
  
  // Map and health data
  const [mapData, setMapData] = useState<RelationshipMapVisualization | null>(null);
  const [healthSnapshots, setHealthSnapshots] = useState<AccountHealthSnapshot[]>([]);
  const [interactions, setInteractions] = useState<RelationshipInteraction[]>([]);
  
  const { hasPermission } = useProfile();
  const dialogApi = useApiState();

  // Form states
  const emptyTypeForm: CreateRelationshipTypeRequest = {
    typeName: '',
    typeCategory: RelationshipCategory.B2B,
    description: '',
    isBidirectional: true,
    reverseTypeName: '',
    color: '#3f51b5',
    icon: 'handshake',
    sortOrder: 0
  };
  
  const emptyRelationshipForm: CreateAccountRelationshipRequest = {
    sourceAccountId: 0,
    targetAccountId: 0,
    relationshipTypeId: 0,
    status: RelationshipStatus.Active,
    strategicImportance: StrategicImportance.Medium,
    startDate: new Date().toISOString().split('T')[0],
    notes: '',
    contractValue: undefined,
    annualRevenue: undefined
  };
  
  const emptyInteractionForm: CreateInteractionRequest = {
    accountRelationshipId: 0,
    interactionType: InteractionType.Meeting,
    subject: '',
    description: '',
    interactionDate: new Date().toISOString().split('T')[0],
    outcome: InteractionOutcome.Neutral,
    healthImpact: HealthImpact.Minor,
    strengthChange: 0,
    followUpRequired: false,
    followUpDate: undefined,
    followUpNotes: ''
  };

  const [typeForm, setTypeForm] = useState(emptyTypeForm);
  const [relationshipForm, setRelationshipForm] = useState(emptyRelationshipForm);
  const [interactionForm, setInteractionForm] = useState(emptyInteractionForm);

  // Fetch data
  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      const [typesRes, relationshipsRes, accountsRes] = await Promise.all([
        relationshipService.getRelationshipTypes(),
        relationshipService.getAccountRelationships(),
        apiClient.get('/accounts')
      ]);
      setRelationshipTypes(Array.isArray(typesRes) ? typesRes : []);
      setRelationships(Array.isArray(relationshipsRes) ? relationshipsRes : []);
      const acctData = accountsRes.data;
      setAccounts(Array.isArray(acctData) ? acctData : (acctData?.items ?? []));
      setError(null);
    } catch (err) {
      setError('Failed to load relationship data');
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  // Get customer display name
  const getAccountName = (accountId: number) => {
    const customer = accounts.find(c => c.id === accountId);
    if (!customer) return `Customer #${accountId}`;
    if (customer.company) return customer.company;
    return `${customer.firstName || ''} ${customer.lastName || ''}`.trim();
  };

  // Handle Type Dialog
  const handleTypeDialogOpen = (type?: RelationshipType) => {
    if (type) {
      setEditingTypeId(type.id);
      setTypeForm({
        typeName: type.typeName,
        typeCategory: type.typeCategory,
        description: type.description || '',
        isBidirectional: type.isBidirectional,
        reverseTypeName: type.reverseTypeName || '',
        color: type.color || '#3f51b5',
        icon: type.icon || 'handshake',
        sortOrder: type.sortOrder
      });
    } else {
      setEditingTypeId(null);
      setTypeForm(emptyTypeForm);
    }
    setTypeDialogOpen(true);
  };

  const handleTypeSave = async () => {
    try {
      dialogApi.setLoading(true);
      if (editingTypeId) {
        await relationshipService.updateRelationshipType(editingTypeId, typeForm);
        setSuccessMessage('Relationship type updated successfully');
      } else {
        await relationshipService.createRelationshipType(typeForm);
        setSuccessMessage('Relationship type created successfully');
      }
      setTypeDialogOpen(false);
      fetchData();
    } catch (err: unknown) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to save relationship type';
      dialogApi.setError(errorMessage);
    } finally {
      dialogApi.setLoading(false);
    }
  };

  const handleTypeDelete = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this relationship type?')) return;
    try {
      await relationshipService.deleteRelationshipType(id);
      setSuccessMessage('Relationship type deleted successfully');
      fetchData();
    } catch (err) {
      setError('Failed to delete relationship type');
    }
  };

  // Handle Relationship Dialog
  const handleRelationshipDialogOpen = (relationship?: AccountRelationship) => {
    if (relationship) {
      setEditingRelationshipId(relationship.id);
      setRelationshipForm({
        sourceAccountId: relationship.sourceAccountId,
        targetAccountId: relationship.targetAccountId,
        relationshipTypeId: relationship.relationshipTypeId,
        status: relationship.status,
        strategicImportance: relationship.strategicImportance,
        startDate: relationship.startDate?.split('T')[0] || '',
        notes: relationship.notes || '',
        contractValue: relationship.contractValue,
        annualRevenue: relationship.annualRevenue
      });
    } else {
      setEditingRelationshipId(null);
      setRelationshipForm(emptyRelationshipForm);
    }
    setRelationshipDialogOpen(true);
  };

  const handleRelationshipSave = async () => {
    try {
      dialogApi.setLoading(true);
      if (editingRelationshipId) {
        await relationshipService.updateAccountRelationship(editingRelationshipId, relationshipForm);
        setSuccessMessage('Relationship updated successfully');
      } else {
        await relationshipService.createAccountRelationship(relationshipForm);
        setSuccessMessage('Relationship created successfully');
      }
      setRelationshipDialogOpen(false);
      fetchData();
    } catch (err: unknown) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to save relationship';
      dialogApi.setError(errorMessage);
    } finally {
      dialogApi.setLoading(false);
    }
  };

  const handleRelationshipDelete = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this relationship?')) return;
    try {
      await relationshipService.deleteAccountRelationship(id);
      setSuccessMessage('Relationship deleted successfully');
      fetchData();
    } catch (err) {
      setError('Failed to delete relationship');
    }
  };

  // Handle Interaction Dialog
  const handleInteractionDialogOpen = (relationship: AccountRelationship) => {
    setSelectedRelationship(relationship);
    setInteractionForm({
      ...emptyInteractionForm,
      accountRelationshipId: relationship.id
    });
    fetchInteractions(relationship.id);
    setInteractionDialogOpen(true);
  };

  const fetchInteractions = async (relationshipId: number) => {
    try {
      const data = await relationshipService.getRelationshipInteractions(relationshipId);
      setInteractions(data);
    } catch (err) {
      console.error('Failed to load interactions', err);
    }
  };

  const handleInteractionSave = async () => {
    try {
      dialogApi.setLoading(true);
      await relationshipService.createRelationshipInteraction(interactionForm);
      setSuccessMessage('Interaction logged successfully');
      if (selectedRelationship) {
        fetchInteractions(selectedRelationship.id);
      }
      setInteractionForm({
        ...emptyInteractionForm,
        accountRelationshipId: interactionForm.accountRelationshipId
      });
      fetchData(); // Refresh relationships to update last interaction date
    } catch (err: unknown) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to log interaction';
      dialogApi.setError(errorMessage);
    } finally {
      dialogApi.setLoading(false);
    }
  };

  // Handle Map Dialog
  const handleMapDialogOpen = (accountId: number) => {
    setSelectedCustomerForMap(accountId);
    fetchMapData(accountId);
    setMapDialogOpen(true);
  };

  const fetchMapData = async (accountId: number) => {
    try {
      const data = await relationshipService.getRelationshipMap(accountId, 2, false);
      setMapData(data);
    } catch (err) {
      console.error('Failed to load map data', err);
    }
  };

  // Handle Health Dialog
  const handleHealthDialogOpen = (accountId: number) => {
    setSelectedCustomerForHealth(accountId);
    fetchHealthData(accountId);
    setHealthDialogOpen(true);
  };

  const fetchHealthData = async (accountId: number) => {
    try {
      const data = await relationshipService.getHealthSnapshots(accountId);
      setHealthSnapshots(data);
    } catch (err) {
      console.error('Failed to load health data', err);
    }
  };

  const handleCreateHealthSnapshot = async () => {
    if (!selectedCustomerForHealth) return;
    try {
      await relationshipService.createHealthSnapshot(selectedCustomerForHealth);
      setSuccessMessage('Health snapshot created successfully');
      fetchHealthData(selectedCustomerForHealth);
    } catch (err) {
      setError('Failed to create health snapshot');
    }
  };

  // Status and importance colors
  const getStatusColor = (status: RelationshipStatus) => {
    const colors: Record<RelationshipStatus, 'success' | 'warning' | 'info' | 'error'> = {
      [RelationshipStatus.Active]: 'success',
      [RelationshipStatus.Inactive]: 'warning',
      [RelationshipStatus.Pending]: 'info',
      [RelationshipStatus.Terminated]: 'error'
    };
    return colors[status] || 'default';
  };

  const getImportanceColor = (importance: StrategicImportance) => {
    const colors: Record<StrategicImportance, 'error' | 'warning' | 'info' | 'default'> = {
      [StrategicImportance.Critical]: 'error',
      [StrategicImportance.High]: 'warning',
      [StrategicImportance.Medium]: 'info',
      [StrategicImportance.Low]: 'default'
    };
    return colors[importance] || 'default';
  };

  const getTrendIcon = (trend: HealthTrend) => {
    if (trend === HealthTrend.Improving) return '📈';
    if (trend === HealthTrend.Declining) return '📉';
    return '➡️';
  };

  const { paginatedData: paginatedRelationships, page, pageSize, handlePageChange, handlePageSizeChange, pageSizeOptions } =
    usePagination(relationships, { defaultPageSize: 25 });

  if (loading && relationships.length === 0) {
    return (
      <Container maxWidth="xl">
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="xl">
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          <RelationshipIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
          Relationship Management
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Track B2B and B2C relationships, monitor health, and visualize relationship networks
        </Typography>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}
      
      {successMessage && (
        <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccessMessage(null)}>
          {successMessage}
        </Alert>
      )}

      <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
        <Tabs value={tabValue} onChange={(_, v) => setTabValue(v)}>
          <Tab label="Relationships" icon={<RelationshipIcon />} iconPosition="start" />
          <Tab label="Relationship Types" icon={<MapIcon />} iconPosition="start" />
        </Tabs>
      </Box>

      {/* Relationships Tab */}
      <TabPanel value={tabValue} index={0}>
        <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between' }}>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleRelationshipDialogOpen()}
          >
            Add Relationship
          </Button>
          <Button startIcon={<RefreshIcon />} onClick={fetchData}>
            Refresh
          </Button>
        </Box>

        <Card>
          <CardContent>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Source Account</TableCell>
                  <TableCell>Type</TableCell>
                  <TableCell>Target Account</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Importance</TableCell>
                  <TableCell>Strength</TableCell>
                  <TableCell>Last Interaction</TableCell>
                  <TableCell>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {paginatedRelationships.map((rel) => (
                  <TableRow key={rel.id}>
                    <TableCell>
                      {rel.sourceAccountName || getAccountName(rel.sourceAccountId)}
                    </TableCell>
                    <TableCell>
                      <Chip 
                        label={rel.relationshipTypeName || relationshipTypes.find(t => t.id === rel.relationshipTypeId)?.typeName || 'Unknown'}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>
                      {rel.targetAccountName || getAccountName(rel.targetAccountId)}
                    </TableCell>
                    <TableCell>
                      <Chip label={rel.status} color={getStatusColor(rel.status)} size="small" />
                    </TableCell>
                    <TableCell>
                      <Chip 
                        label={rel.strategicImportance} 
                        color={getImportanceColor(rel.strategicImportance)} 
                        size="small" 
                      />
                    </TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        <LinearProgress 
                          variant="determinate" 
                          value={rel.strengthScore} 
                          sx={{ width: 60, mr: 1 }}
                        />
                        {rel.strengthScore}%
                      </Box>
                    </TableCell>
                    <TableCell>
                      {rel.lastInteractionDate 
                        ? new Date(rel.lastInteractionDate).toLocaleDateString()
                        : 'Never'}
                    </TableCell>
                    <TableCell>
                      <Tooltip title="Log Interaction">
                        <IconButton size="small" onClick={() => handleInteractionDialogOpen(rel)}>
                          <TrendingUpIcon />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="View Map">
                        <IconButton size="small" onClick={() => handleMapDialogOpen(rel.sourceAccountId)}>
                          <MapIcon />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Health Snapshots">
                        <IconButton size="small" onClick={() => handleHealthDialogOpen(rel.sourceAccountId)}>
                          <HealthIcon />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Edit">
                        <IconButton size="small" onClick={() => handleRelationshipDialogOpen(rel)}>
                          <EditIcon />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Delete">
                        <IconButton size="small" color="error" onClick={() => handleRelationshipDelete(rel.id)}>
                          <DeleteIcon />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))}
                {relationships.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={8} align="center">
                      <Typography color="text.secondary">
                        No relationships found. Create your first relationship to get started.
                      </Typography>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
            <TablePagination
              component="div"
              count={relationships.length}
              page={page}
              onPageChange={handlePageChange}
              rowsPerPage={pageSize}
              onRowsPerPageChange={handlePageSizeChange}
              rowsPerPageOptions={pageSizeOptions}
            />
          </CardContent>
        </Card>
      </TabPanel>

      {/* Relationship Types Tab */}
      <TabPanel value={tabValue} index={1}>
        <Box sx={{ mb: 2 }}>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleTypeDialogOpen()}
          >
            Add Relationship Type
          </Button>
        </Box>

        <Grid container spacing={2}>
          {relationshipTypes.map((type) => (
            <Grid item xs={12} md={6} lg={4} key={type.id}>
              <Card>
                <CardContent>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                    <Box>
                      <Typography variant="h6" gutterBottom>
                        {type.typeName}
                      </Typography>
                      <Stack direction="row" spacing={1} sx={{ mb: 1 }}>
                        <Chip label={type.typeCategory} size="small" />
                        {type.isBidirectional && (
                          <Chip label="Bidirectional" size="small" color="info" />
                        )}
                        {type.isSystem && (
                          <Chip label="System" size="small" color="warning" />
                        )}
                      </Stack>
                      {type.description && (
                        <Typography variant="body2" color="text.secondary">
                          {type.description}
                        </Typography>
                      )}
                      {type.reverseTypeName && (
                        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                          Reverse: {type.reverseTypeName}
                        </Typography>
                      )}
                    </Box>
                    {!type.isSystem && (
                      <Box>
                        <IconButton size="small" onClick={() => handleTypeDialogOpen(type)}>
                          <EditIcon />
                        </IconButton>
                        <IconButton size="small" color="error" onClick={() => handleTypeDelete(type.id)}>
                          <DeleteIcon />
                        </IconButton>
                      </Box>
                    )}
                  </Box>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      </TabPanel>

      {/* Type Dialog */}
      <Dialog open={typeDialogOpen} onClose={() => setTypeDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          {editingTypeId ? 'Edit Relationship Type' : 'Create Relationship Type'}
        </DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Type Name"
                value={typeForm.typeName}
                onChange={(e) => setTypeForm({ ...typeForm, typeName: e.target.value })}
                required
              />
            </Grid>
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>Category</InputLabel>
                <Select
                  value={typeForm.typeCategory}
                  label="Category"
                  onChange={(e) => setTypeForm({ ...typeForm, typeCategory: e.target.value as RelationshipCategory })}
                >
                  {Object.values(RelationshipCategory).map((cat) => (
                    <MenuItem key={cat} value={cat}>{cat}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Description"
                value={typeForm.description}
                onChange={(e) => setTypeForm({ ...typeForm, description: e.target.value })}
                multiline
                rows={2}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Reverse Type Name"
                value={typeForm.reverseTypeName}
                onChange={(e) => setTypeForm({ ...typeForm, reverseTypeName: e.target.value })}
                helperText="For non-bidirectional relationships (e.g., 'Parent' → 'Subsidiary')"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setTypeDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleTypeSave} disabled={dialogApi.loading}>
            {dialogApi.loading ? <CircularProgress size={24} /> : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Relationship Dialog */}
      <Dialog open={relationshipDialogOpen} onClose={() => setRelationshipDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          {editingRelationshipId ? 'Edit Relationship' : 'Create Relationship'}
        </DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12} md={6}>
              <Autocomplete
                options={accounts}
                getOptionLabel={(c) => c.company || `${c.firstName || ''} ${c.lastName || ''}`.trim()}
                value={accounts.find(c => c.id === relationshipForm.sourceAccountId) || null}
                onChange={(_, v) => setRelationshipForm({ ...relationshipForm, sourceAccountId: v?.id || 0 })}
                renderInput={(params) => <TextField {...params} label="Source Account" required />}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <Autocomplete
                options={accounts}
                getOptionLabel={(c) => c.company || `${c.firstName || ''} ${c.lastName || ''}`.trim()}
                value={accounts.find(c => c.id === relationshipForm.targetAccountId) || null}
                onChange={(_, v) => setRelationshipForm({ ...relationshipForm, targetAccountId: v?.id || 0 })}
                renderInput={(params) => <TextField {...params} label="Target Account" required />}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth required>
                <InputLabel>Relationship Type</InputLabel>
                <Select
                  value={relationshipForm.relationshipTypeId}
                  label="Relationship Type"
                  onChange={(e) => setRelationshipForm({ ...relationshipForm, relationshipTypeId: Number(e.target.value) })}
                >
                  {relationshipTypes.filter(t => t?.isActive !== false).map((type) => (
                    <MenuItem key={type.id} value={type.id}>{type.typeName}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Status</InputLabel>
                <Select
                  value={relationshipForm.status}
                  label="Status"
                  onChange={(e) => setRelationshipForm({ ...relationshipForm, status: e.target.value as RelationshipStatus })}
                >
                  {Object.values(RelationshipStatus).map((status) => (
                    <MenuItem key={status} value={status}>{status}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Strategic Importance</InputLabel>
                <Select
                  value={relationshipForm.strategicImportance}
                  label="Strategic Importance"
                  onChange={(e) => setRelationshipForm({ ...relationshipForm, strategicImportance: e.target.value as StrategicImportance })}
                >
                  {Object.values(StrategicImportance).map((imp) => (
                    <MenuItem key={imp} value={imp}>{imp}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Start Date"
                type="date"
                value={relationshipForm.startDate}
                onChange={(e) => setRelationshipForm({ ...relationshipForm, startDate: e.target.value })}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Contract Value"
                type="number"
                value={relationshipForm.contractValue || ''}
                onChange={(e) => setRelationshipForm({ ...relationshipForm, contractValue: Number(e.target.value) || undefined })}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Annual Revenue"
                type="number"
                value={relationshipForm.annualRevenue || ''}
                onChange={(e) => setRelationshipForm({ ...relationshipForm, annualRevenue: Number(e.target.value) || undefined })}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Notes"
                value={relationshipForm.notes}
                onChange={(e) => setRelationshipForm({ ...relationshipForm, notes: e.target.value })}
                multiline
                rows={3}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRelationshipDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleRelationshipSave} disabled={dialogApi.loading}>
            {dialogApi.loading ? <CircularProgress size={24} /> : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Interaction Dialog */}
      <Dialog open={interactionDialogOpen} onClose={() => setInteractionDialogOpen(false)} maxWidth="lg" fullWidth>
        <DialogTitle>
          Log Interaction - {selectedRelationship?.sourceAccountName} ↔ {selectedRelationship?.targetAccountName}
          <IconButton sx={{ position: 'absolute', right: 8, top: 8 }} onClick={() => setInteractionDialogOpen(false)}>
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <DialogContent>
          <Grid container spacing={3}>
            {/* New Interaction Form */}
            <Grid item xs={12} md={5}>
              <Typography variant="h6" gutterBottom>New Interaction</Typography>
              <Grid container spacing={2}>
                <Grid item xs={12}>
                  <FormControl fullWidth>
                    <InputLabel>Type</InputLabel>
                    <Select
                      value={interactionForm.interactionType}
                      label="Type"
                      onChange={(e) => setInteractionForm({ ...interactionForm, interactionType: e.target.value as InteractionType })}
                    >
                      {Object.values(InteractionType).map((type) => (
                        <MenuItem key={type} value={type}>{type}</MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Subject"
                    value={interactionForm.subject}
                    onChange={(e) => setInteractionForm({ ...interactionForm, subject: e.target.value })}
                    required
                  />
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Description"
                    value={interactionForm.description}
                    onChange={(e) => setInteractionForm({ ...interactionForm, description: e.target.value })}
                    multiline
                    rows={3}
                  />
                </Grid>
                <Grid item xs={12} md={6}>
                  <FormControl fullWidth>
                    <InputLabel>Outcome</InputLabel>
                    <Select
                      value={interactionForm.outcome}
                      label="Outcome"
                      onChange={(e) => setInteractionForm({ ...interactionForm, outcome: e.target.value as InteractionOutcome })}
                    >
                      {Object.values(InteractionOutcome).map((outcome) => (
                        <MenuItem key={outcome} value={outcome}>{outcome}</MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={12} md={6}>
                  <FormControl fullWidth>
                    <InputLabel>Health Impact</InputLabel>
                    <Select
                      value={interactionForm.healthImpact}
                      label="Health Impact"
                      onChange={(e) => setInteractionForm({ ...interactionForm, healthImpact: e.target.value as HealthImpact })}
                    >
                      {Object.values(HealthImpact).map((impact) => (
                        <MenuItem key={impact} value={impact}>{impact}</MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={12}>
                  <Button variant="contained" onClick={handleInteractionSave} disabled={dialogApi.loading} fullWidth>
                    {dialogApi.loading ? <CircularProgress size={24} /> : 'Log Interaction'}
                  </Button>
                </Grid>
              </Grid>
            </Grid>
            
            {/* Interaction History */}
            <Grid item xs={12} md={7}>
              <Typography variant="h6" gutterBottom>Interaction History</Typography>
              <Paper sx={{ maxHeight: 400, overflow: 'auto' }}>
                {interactions.length === 0 ? (
                  <Box p={3} textAlign="center">
                    <Typography color="text.secondary">No interactions recorded yet</Typography>
                  </Box>
                ) : (
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Date</TableCell>
                        <TableCell>Type</TableCell>
                        <TableCell>Subject</TableCell>
                        <TableCell>Outcome</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {interactions.map((int) => (
                        <TableRow key={int.id}>
                          <TableCell>{new Date(int.interactionDate).toLocaleDateString()}</TableCell>
                          <TableCell>{int.interactionType}</TableCell>
                          <TableCell>{int.subject}</TableCell>
                          <TableCell>
                            <Chip 
                              label={int.outcome} 
                              size="small" 
                              color={int.outcome === InteractionOutcome.Positive ? 'success' : int.outcome === InteractionOutcome.Negative ? 'error' : 'default'}
                            />
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                )}
              </Paper>
            </Grid>
          </Grid>
        </DialogContent>
      </Dialog>

      {/* Map Dialog */}
      <Dialog open={mapDialogOpen} onClose={() => setMapDialogOpen(false)} maxWidth="lg" fullWidth>
        <DialogTitle>
          Relationship Map - {getAccountName(selectedCustomerForMap || 0)}
          <IconButton sx={{ position: 'absolute', right: 8, top: 8 }} onClick={() => setMapDialogOpen(false)}>
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <DialogContent>
          {mapData ? (
            <Box>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Generated: {new Date(mapData.generatedAt).toLocaleString()}
              </Typography>
              <Grid container spacing={2}>
                <Grid item xs={12} md={6}>
                  <Typography variant="h6">Nodes ({mapData.nodes.length})</Typography>
                  <Paper sx={{ maxHeight: 300, overflow: 'auto' }}>
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Name</TableCell>
                          <TableCell>Company</TableCell>
                          <TableCell>Health</TableCell>
                          <TableCell>Depth</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {mapData.nodes.map((node) => (
                          <TableRow key={node.id} sx={{ bgcolor: node.isCenter ? 'action.selected' : 'inherit' }}>
                            <TableCell>{node.name}</TableCell>
                            <TableCell>{node.company || '-'}</TableCell>
                            <TableCell>{node.healthScore || '-'}</TableCell>
                            <TableCell>{node.depth}</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </Paper>
                </Grid>
                <Grid item xs={12} md={6}>
                  <Typography variant="h6">Edges ({mapData.edges.length})</Typography>
                  <Paper sx={{ maxHeight: 300, overflow: 'auto' }}>
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>From → To</TableCell>
                          <TableCell>Type</TableCell>
                          <TableCell>Strength</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {mapData.edges.map((edge) => (
                          <TableRow key={edge.id}>
                            <TableCell>
                              {mapData.nodes.find(n => n.accountId === edge.sourceId)?.name} → 
                              {mapData.nodes.find(n => n.accountId === edge.targetId)?.name}
                            </TableCell>
                            <TableCell>{edge.relationshipTypeName}</TableCell>
                            <TableCell>{edge.strengthScore}%</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </Paper>
                </Grid>
              </Grid>
              {/* Interactive relationship graph */}
              <Box sx={{ mt: 2, border: 1, borderColor: 'divider', borderRadius: 2, p: 1, bgcolor: 'background.default' }}>
                <RelationshipGraph nodes={mapData.nodes} edges={mapData.edges} />
              </Box>
            </Box>
          ) : (
            <Box display="flex" justifyContent="center" p={4}>
              <CircularProgress />
            </Box>
          )}
        </DialogContent>
      </Dialog>

      {/* Health Dialog */}
      <Dialog open={healthDialogOpen} onClose={() => setHealthDialogOpen(false)} maxWidth="lg" fullWidth>
        <DialogTitle>
          Health Snapshots - {getAccountName(selectedCustomerForHealth || 0)}
          <IconButton sx={{ position: 'absolute', right: 8, top: 8 }} onClick={() => setHealthDialogOpen(false)}>
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <DialogContent>
          <Box sx={{ mb: 2 }}>
            <Button variant="contained" onClick={handleCreateHealthSnapshot}>
              Create New Snapshot
            </Button>
          </Box>
          {healthSnapshots.length === 0 ? (
            <Alert severity="info">
              No health snapshots yet. Create one to track account health over time.
            </Alert>
          ) : (
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Date</TableCell>
                  <TableCell>Overall</TableCell>
                  <TableCell>Engagement</TableCell>
                  <TableCell>Satisfaction</TableCell>
                  <TableCell>Risk</TableCell>
                  <TableCell>Growth</TableCell>
                  <TableCell>Trend</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {healthSnapshots.map((snapshot) => (
                  <TableRow key={snapshot.id}>
                    <TableCell>{new Date(snapshot.snapshotDate).toLocaleDateString()}</TableCell>
                    <TableCell>
                      <Chip label={snapshot.overallHealthScore} color={snapshot.overallHealthScore >= 70 ? 'success' : snapshot.overallHealthScore >= 40 ? 'warning' : 'error'} />
                    </TableCell>
                    <TableCell>{snapshot.engagementScore}</TableCell>
                    <TableCell>{snapshot.satisfactionScore}</TableCell>
                    <TableCell>{snapshot.riskScore}</TableCell>
                    <TableCell>{snapshot.growthScore}</TableCell>
                    <TableCell>
                      {getTrendIcon(snapshot.trend)} {snapshot.trend} ({snapshot.trendPercentage > 0 ? '+' : ''}{snapshot.trendPercentage}%)
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </DialogContent>
      </Dialog>
    </Container>
  );
}

export default RelationshipsPage;

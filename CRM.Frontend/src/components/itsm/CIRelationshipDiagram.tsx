// CI Relationship Diagram - Visual dependency map using React Flow
// Part of ITSM Enhancement Plan - Phase 5.3

import React, { useState, useCallback, useMemo, useEffect } from 'react';
import {
  Box,
  Paper,
  Typography,
  Stack,
  Chip,
  IconButton,
  Tooltip,
  Button,
  Menu,
  MenuItem,
  Divider,
  ToggleButton,
  ToggleButtonGroup,
  FormControlLabel,
  Switch,
  Badge,
  Card,
  CardContent,
} from '@mui/material';
import {
  Computer as ServerIcon,
  Storage as DatabaseIcon,
  Router as NetworkIcon,
  Cloud as CloudIcon,
  DesktopWindows as DesktopIcon,
  Dns as DnsIcon,
  Security as SecurityIcon,
  Memory as MemoryIcon,
  Warning as WarningIcon,
  CheckCircle as HealthyIcon,
  Error as ErrorIcon,
  Help as UnknownIcon,
  ZoomIn as ZoomInIcon,
  ZoomOut as ZoomOutIcon,
  CenterFocusStrong as FitIcon,
  Fullscreen as FullscreenIcon,
  Download as ExportIcon,
  FilterList as FilterIcon,
  Refresh as RefreshIcon,
  ArrowForward as ArrowIcon,
} from '@mui/icons-material';

// Types for Configuration Items
export type CIType =
  | 'server'
  | 'database'
  | 'network'
  | 'cloud'
  | 'desktop'
  | 'dns'
  | 'security'
  | 'application';

export type CIStatus = 'operational' | 'degraded' | 'outage' | 'maintenance' | 'unknown';

export type RelationshipType =
  | 'depends_on'
  | 'runs_on'
  | 'connects_to'
  | 'hosts'
  | 'uses'
  | 'backup_of'
  | 'cluster_member';

export interface ConfigurationItem {
  id: string;
  name: string;
  type: CIType;
  status: CIStatus;
  environment?: 'production' | 'staging' | 'development';
  criticality: 'low' | 'medium' | 'high' | 'critical';
  owner?: string;
  location?: string;
  ipAddress?: string;
  metadata?: Record<string, string>;
  activeIncidents?: number;
  lastUpdated?: Date | string;
}

export interface CIRelationship {
  id: string;
  sourceId: string;
  targetId: string;
  type: RelationshipType;
  description?: string;
  bidirectional?: boolean;
}

export interface CIRelationshipDiagramProps {
  configItems: ConfigurationItem[];
  relationships: CIRelationship[];
  selectedCIId?: string;
  onCISelect?: (ciId: string | null) => void;
  onCIDoubleClick?: (ciId: string) => void;
  highlightImpact?: boolean;
  showOrphans?: boolean;
  filterEnvironment?: 'production' | 'staging' | 'development';
  readOnly?: boolean;
}

// Helper functions
const getCITypeIcon = (type: CIType) => {
  const iconProps = { sx: { fontSize: 20 } };
  switch (type) {
    case 'server':
      return <ServerIcon {...iconProps} />;
    case 'database':
      return <DatabaseIcon {...iconProps} />;
    case 'network':
      return <NetworkIcon {...iconProps} />;
    case 'cloud':
      return <CloudIcon {...iconProps} />;
    case 'desktop':
      return <DesktopIcon {...iconProps} />;
    case 'dns':
      return <DnsIcon {...iconProps} />;
    case 'security':
      return <SecurityIcon {...iconProps} />;
    case 'application':
      return <MemoryIcon {...iconProps} />;
    default:
      return <ServerIcon {...iconProps} />;
  }
};

const getStatusColor = (status: CIStatus): string => {
  switch (status) {
    case 'operational':
      return '#4caf50';
    case 'degraded':
      return '#ff9800';
    case 'outage':
      return '#f44336';
    case 'maintenance':
      return '#2196f3';
    default:
      return '#9e9e9e';
  }
};

const getStatusIcon = (status: CIStatus) => {
  switch (status) {
    case 'operational':
      return <HealthyIcon sx={{ fontSize: 14, color: '#4caf50' }} />;
    case 'degraded':
      return <WarningIcon sx={{ fontSize: 14, color: '#ff9800' }} />;
    case 'outage':
      return <ErrorIcon sx={{ fontSize: 14, color: '#f44336' }} />;
    default:
      return <UnknownIcon sx={{ fontSize: 14, color: '#9e9e9e' }} />;
  }
};

const getRelationshipLabel = (type: RelationshipType): string => {
  switch (type) {
    case 'depends_on':
      return 'Depends On';
    case 'runs_on':
      return 'Runs On';
    case 'connects_to':
      return 'Connects To';
    case 'hosts':
      return 'Hosts';
    case 'uses':
      return 'Uses';
    case 'backup_of':
      return 'Backup Of';
    case 'cluster_member':
      return 'Cluster Member';
    default:
      return type;
  }
};

const getRelationshipColor = (type: RelationshipType): string => {
  switch (type) {
    case 'depends_on':
      return '#2196f3';
    case 'runs_on':
      return '#9c27b0';
    case 'connects_to':
      return '#4caf50';
    case 'hosts':
      return '#ff9800';
    case 'uses':
      return '#607d8b';
    case 'backup_of':
      return '#795548';
    case 'cluster_member':
      return '#00bcd4';
    default:
      return '#9e9e9e';
  }
};

// CI Node component
const CINode: React.FC<{
  ci: ConfigurationItem;
  isSelected: boolean;
  isHighlighted: boolean;
  isImpacted: boolean;
  position: { x: number; y: number };
  onSelect: () => void;
  onDoubleClick?: () => void;
}> = ({ ci, isSelected, isHighlighted, isImpacted, position, onSelect, onDoubleClick }) => (
  <Card
    variant="outlined"
    onClick={onSelect}
    onDoubleClick={onDoubleClick}
    sx={{
      position: 'absolute',
      left: position.x,
      top: position.y,
      width: 160,
      cursor: 'pointer',
      borderWidth: isSelected ? 2 : 1,
      borderColor: isSelected
        ? 'primary.main'
        : isImpacted
        ? '#f44336'
        : isHighlighted
        ? getStatusColor(ci.status)
        : 'divider',
      backgroundColor: isImpacted
        ? '#ffebee'
        : isSelected
        ? 'action.selected'
        : 'background.paper',
      opacity: isHighlighted || isSelected ? 1 : 0.6,
      transition: 'all 0.2s',
      '&:hover': {
        borderColor: 'primary.main',
        boxShadow: 2,
        zIndex: 10,
      },
      zIndex: isSelected ? 5 : 1,
    }}
  >
    <CardContent sx={{ p: 1.5, '&:last-child': { pb: 1.5 } }}>
      <Stack direction="row" alignItems="center" spacing={1}>
        <Badge
          overlap="circular"
          anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
          badgeContent={getStatusIcon(ci.status)}
        >
          <Box
            sx={{
              p: 0.5,
              borderRadius: 1,
              backgroundColor: `${getStatusColor(ci.status)}20`,
              display: 'flex',
            }}
          >
            {React.cloneElement(getCITypeIcon(ci.type), {
              sx: { color: getStatusColor(ci.status), fontSize: 20 },
            })}
          </Box>
        </Badge>
        <Box sx={{ flex: 1, minWidth: 0 }}>
          <Typography variant="caption" fontWeight={600} noWrap display="block">
            {ci.name}
          </Typography>
          <Stack direction="row" spacing={0.5} alignItems="center">
            <Chip
              label={ci.type}
              size="small"
              sx={{ height: 16, fontSize: '0.6rem' }}
            />
            {ci.activeIncidents && ci.activeIncidents > 0 && (
              <Chip
                label={ci.activeIncidents}
                size="small"
                color="error"
                sx={{ height: 16, fontSize: '0.6rem', minWidth: 20 }}
              />
            )}
          </Stack>
        </Box>
      </Stack>
    </CardContent>
  </Card>
);

// Relationship line component (SVG)
const RelationshipLine: React.FC<{
  relationship: CIRelationship;
  sourcePos: { x: number; y: number };
  targetPos: { x: number; y: number };
  isHighlighted: boolean;
  isImpactPath: boolean;
}> = ({ relationship, sourcePos, targetPos, isHighlighted, isImpactPath }) => {
  const nodeWidth = 160;
  const nodeHeight = 60;

  // Calculate edge points
  const sx = sourcePos.x + nodeWidth;
  const sy = sourcePos.y + nodeHeight / 2;
  const tx = targetPos.x;
  const ty = targetPos.y + nodeHeight / 2;

  // Create curved path
  const midX = (sx + tx) / 2;
  const path = `M ${sx} ${sy} C ${midX} ${sy}, ${midX} ${ty}, ${tx} ${ty}`;

  const color = isImpactPath ? '#f44336' : getRelationshipColor(relationship.type);
  const opacity = isHighlighted || isImpactPath ? 1 : 0.3;

  return (
    <g>
      <path
        d={path}
        fill="none"
        stroke={color}
        strokeWidth={isImpactPath ? 3 : 2}
        strokeOpacity={opacity}
        strokeDasharray={relationship.bidirectional ? 'none' : '5,3'}
        markerEnd="url(#arrowhead)"
      />
      {/* Relationship label */}
      <text
        x={midX}
        y={(sy + ty) / 2 - 5}
        textAnchor="middle"
        fontSize="10"
        fill={color}
        opacity={opacity}
      >
        {getRelationshipLabel(relationship.type)}
      </text>
    </g>
  );
};

// CI Detail panel
const CIDetailPanel: React.FC<{
  ci: ConfigurationItem;
  relationships: CIRelationship[];
  allCIs: ConfigurationItem[];
  onNavigate: (ciId: string) => void;
}> = ({ ci, relationships, allCIs, onNavigate }) => {
  const upstreamRels = relationships.filter((r) => r.targetId === ci.id);
  const downstreamRels = relationships.filter((r) => r.sourceId === ci.id);

  return (
    <Paper sx={{ p: 2, width: 280 }}>
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
        <Box
          sx={{
            p: 1,
            borderRadius: 1,
            backgroundColor: `${getStatusColor(ci.status)}20`,
          }}
        >
          {React.cloneElement(getCITypeIcon(ci.type), {
            sx: { color: getStatusColor(ci.status) },
          })}
        </Box>
        <Box>
          <Typography variant="subtitle1" fontWeight={600}>
            {ci.name}
          </Typography>
          <Stack direction="row" spacing={1} alignItems="center">
            {getStatusIcon(ci.status)}
            <Typography variant="caption" color="text.secondary">
              {ci.status.charAt(0).toUpperCase() + ci.status.slice(1)}
            </Typography>
          </Stack>
        </Box>
      </Stack>

      <Divider sx={{ my: 1 }} />

      {/* Properties */}
      <Stack spacing={1} sx={{ mb: 2 }}>
        <Stack direction="row" justifyContent="space-between">
          <Typography variant="caption" color="text.secondary">
            Type
          </Typography>
          <Chip label={ci.type} size="small" sx={{ height: 20 }} />
        </Stack>
        <Stack direction="row" justifyContent="space-between">
          <Typography variant="caption" color="text.secondary">
            Criticality
          </Typography>
          <Chip
            label={ci.criticality}
            size="small"
            color={ci.criticality === 'critical' ? 'error' : 'default'}
            sx={{ height: 20 }}
          />
        </Stack>
        {ci.environment && (
          <Stack direction="row" justifyContent="space-between">
            <Typography variant="caption" color="text.secondary">
              Environment
            </Typography>
            <Typography variant="caption">{ci.environment}</Typography>
          </Stack>
        )}
        {ci.ipAddress && (
          <Stack direction="row" justifyContent="space-between">
            <Typography variant="caption" color="text.secondary">
              IP Address
            </Typography>
            <Typography variant="caption" fontFamily="monospace">
              {ci.ipAddress}
            </Typography>
          </Stack>
        )}
        {ci.owner && (
          <Stack direction="row" justifyContent="space-between">
            <Typography variant="caption" color="text.secondary">
              Owner
            </Typography>
            <Typography variant="caption">{ci.owner}</Typography>
          </Stack>
        )}
      </Stack>

      <Divider sx={{ my: 1 }} />

      {/* Upstream (depends on) */}
      <Typography variant="subtitle2" gutterBottom>
        Depends On ({upstreamRels.length})
      </Typography>
      {upstreamRels.length > 0 ? (
        <Stack spacing={0.5} sx={{ mb: 2 }}>
          {upstreamRels.map((rel) => {
            const source = allCIs.find((c) => c.id === rel.sourceId);
            if (!source) return null;
            return (
              <Chip
                key={rel.id}
                icon={getStatusIcon(source.status)}
                label={source.name}
                size="small"
                variant="outlined"
                onClick={() => onNavigate(source.id)}
                sx={{ justifyContent: 'flex-start' }}
              />
            );
          })}
        </Stack>
      ) : (
        <Typography variant="caption" color="text.secondary" sx={{ mb: 2, display: 'block' }}>
          No upstream dependencies
        </Typography>
      )}

      {/* Downstream (depended on by) */}
      <Typography variant="subtitle2" gutterBottom>
        Used By ({downstreamRels.length})
      </Typography>
      {downstreamRels.length > 0 ? (
        <Stack spacing={0.5}>
          {downstreamRels.map((rel) => {
            const target = allCIs.find((c) => c.id === rel.targetId);
            if (!target) return null;
            return (
              <Chip
                key={rel.id}
                icon={getStatusIcon(target.status)}
                label={target.name}
                size="small"
                variant="outlined"
                onClick={() => onNavigate(target.id)}
                sx={{ justifyContent: 'flex-start' }}
              />
            );
          })}
        </Stack>
      ) : (
        <Typography variant="caption" color="text.secondary">
          No downstream consumers
        </Typography>
      )}
    </Paper>
  );
};

export const CIRelationshipDiagram: React.FC<CIRelationshipDiagramProps> = ({
  configItems,
  relationships,
  selectedCIId,
  onCISelect,
  onCIDoubleClick,
  highlightImpact = true,
  showOrphans = false,
  filterEnvironment,
  readOnly = false,
}) => {
  const [zoom, setZoom] = useState(1);
  const [filterAnchor, setFilterAnchor] = useState<null | HTMLElement>(null);
  const [showLabels, setShowLabels] = useState(true);
  const [selectedTypes, setSelectedTypes] = useState<Set<CIType>>(new Set());

  // Filter CIs by environment and type
  const filteredCIs = useMemo(() => {
    let result = configItems;

    if (filterEnvironment) {
      result = result.filter((ci) => ci.environment === filterEnvironment);
    }

    if (selectedTypes.size > 0) {
      result = result.filter((ci) => selectedTypes.has(ci.type));
    }

    if (!showOrphans) {
      const connectedIds = new Set<string>();
      relationships.forEach((r) => {
        connectedIds.add(r.sourceId);
        connectedIds.add(r.targetId);
      });
      result = result.filter((ci) => connectedIds.has(ci.id));
    }

    return result;
  }, [configItems, filterEnvironment, selectedTypes, showOrphans, relationships]);

  // Filter relationships based on visible CIs
  const filteredRelationships = useMemo(() => {
    const ciIds = new Set(filteredCIs.map((ci) => ci.id));
    return relationships.filter(
      (r) => ciIds.has(r.sourceId) && ciIds.has(r.targetId)
    );
  }, [filteredCIs, relationships]);

  // Calculate CI positions (simple grid layout)
  const ciPositions = useMemo(() => {
    const positions: Record<string, { x: number; y: number }> = {};
    const cols = Math.ceil(Math.sqrt(filteredCIs.length));
    const nodeWidth = 180;
    const nodeHeight = 80;
    const padding = 40;

    filteredCIs.forEach((ci, index) => {
      const col = index % cols;
      const row = Math.floor(index / cols);
      positions[ci.id] = {
        x: padding + col * nodeWidth,
        y: padding + row * nodeHeight,
      };
    });

    return positions;
  }, [filteredCIs]);

  // Calculate impact path (CIs affected by selected CI's outage)
  const impactedCIIds = useMemo(() => {
    if (!selectedCIId || !highlightImpact) return new Set<string>();

    const impacted = new Set<string>();
    impacted.add(selectedCIId);

    // Find all CIs that depend on the selected CI (downstream impact)
    const findDependents = (ciId: string) => {
      relationships.forEach((rel) => {
        if (rel.sourceId === ciId && !impacted.has(rel.targetId)) {
          impacted.add(rel.targetId);
          findDependents(rel.targetId);
        }
      });
    };

    findDependents(selectedCIId);
    return impacted;
  }, [selectedCIId, relationships, highlightImpact]);

  // Highlighted CIs (selected + related)
  const highlightedCIIds = useMemo(() => {
    if (!selectedCIId) return new Set<string>();

    const highlighted = new Set<string>();
    highlighted.add(selectedCIId);

    relationships.forEach((rel) => {
      if (rel.sourceId === selectedCIId) highlighted.add(rel.targetId);
      if (rel.targetId === selectedCIId) highlighted.add(rel.sourceId);
    });

    return highlighted;
  }, [selectedCIId, relationships]);

  const selectedCI = useMemo(
    () => configItems.find((ci) => ci.id === selectedCIId),
    [configItems, selectedCIId]
  );

  const canvasWidth = 800;
  const canvasHeight = 600;

  return (
    <Box sx={{ display: 'flex', gap: 2, height: '100%' }}>
      <Paper sx={{ flex: 1, p: 2, overflow: 'hidden' }}>
        {/* Toolbar */}
        <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 2 }}>
          <Stack direction="row" spacing={1}>
            <Button
              size="small"
              startIcon={<FilterIcon />}
              onClick={(e) => setFilterAnchor(e.currentTarget)}
            >
              Filter
            </Button>
            <FormControlLabel
              control={
                <Switch
                  size="small"
                  checked={showLabels}
                  onChange={(e) => setShowLabels(e.target.checked)}
                />
              }
              label={<Typography variant="caption">Labels</Typography>}
            />
          </Stack>

          <Stack direction="row" spacing={0.5}>
            <IconButton size="small" onClick={() => setZoom((z) => Math.min(z + 0.1, 2))}>
              <ZoomInIcon />
            </IconButton>
            <IconButton size="small" onClick={() => setZoom((z) => Math.max(z - 0.1, 0.5))}>
              <ZoomOutIcon />
            </IconButton>
            <IconButton size="small" onClick={() => setZoom(1)}>
              <FitIcon />
            </IconButton>
            <Tooltip title="Refresh Layout">
              <IconButton size="small">
                <RefreshIcon />
              </IconButton>
            </Tooltip>
          </Stack>
        </Stack>

        {/* Filter menu */}
        <Menu
          anchorEl={filterAnchor}
          open={Boolean(filterAnchor)}
          onClose={() => setFilterAnchor(null)}
        >
          <MenuItem disabled>
            <Typography variant="caption">Filter by Type</Typography>
          </MenuItem>
          {(['server', 'database', 'network', 'cloud', 'application'] as CIType[]).map((type) => (
            <MenuItem
              key={type}
              onClick={() => {
                setSelectedTypes((prev) => {
                  const next = new Set(prev);
                  if (next.has(type)) {
                    next.delete(type);
                  } else {
                    next.add(type);
                  }
                  return next;
                });
              }}
            >
              <Stack direction="row" alignItems="center" spacing={1}>
                {getCITypeIcon(type)}
                <Typography variant="body2">{type}</Typography>
                {selectedTypes.has(type) && (
                  <Chip label="Active" size="small" color="primary" sx={{ height: 18 }} />
                )}
              </Stack>
            </MenuItem>
          ))}
          <Divider />
          <MenuItem onClick={() => setSelectedTypes(new Set())}>
            Clear Filters
          </MenuItem>
        </Menu>

        {/* Diagram canvas */}
        <Box
          sx={{
            position: 'relative',
            width: canvasWidth,
            height: canvasHeight,
            border: '1px solid',
            borderColor: 'divider',
            borderRadius: 1,
            overflow: 'auto',
            backgroundColor: 'grey.50',
            transform: `scale(${zoom})`,
            transformOrigin: 'top left',
          }}
        >
          {/* SVG for relationship lines */}
          <svg
            width={canvasWidth}
            height={canvasHeight}
            style={{ position: 'absolute', top: 0, left: 0, pointerEvents: 'none' }}
          >
            <defs>
              <marker
                id="arrowhead"
                markerWidth="10"
                markerHeight="7"
                refX="9"
                refY="3.5"
                orient="auto"
              >
                <polygon points="0 0, 10 3.5, 0 7" fill="#9e9e9e" />
              </marker>
            </defs>
            {filteredRelationships.map((rel) => {
              const sourcePos = ciPositions[rel.sourceId];
              const targetPos = ciPositions[rel.targetId];
              if (!sourcePos || !targetPos) return null;

              const isHighlighted =
                highlightedCIIds.has(rel.sourceId) || highlightedCIIds.has(rel.targetId);
              const isImpactPath =
                impactedCIIds.has(rel.sourceId) && impactedCIIds.has(rel.targetId);

              return (
                <RelationshipLine
                  key={rel.id}
                  relationship={rel}
                  sourcePos={sourcePos}
                  targetPos={targetPos}
                  isHighlighted={isHighlighted}
                  isImpactPath={isImpactPath}
                />
              );
            })}
          </svg>

          {/* CI Nodes */}
          {filteredCIs.map((ci) => (
            <CINode
              key={ci.id}
              ci={ci}
              isSelected={ci.id === selectedCIId}
              isHighlighted={highlightedCIIds.size === 0 || highlightedCIIds.has(ci.id)}
              isImpacted={impactedCIIds.has(ci.id) && ci.id !== selectedCIId}
              position={ciPositions[ci.id] || { x: 0, y: 0 }}
              onSelect={() => onCISelect?.(ci.id === selectedCIId ? null : ci.id)}
              onDoubleClick={() => onCIDoubleClick?.(ci.id)}
            />
          ))}
        </Box>

        {/* Legend */}
        <Box sx={{ mt: 2, pt: 2, borderTop: '1px solid', borderColor: 'divider' }}>
          <Stack direction="row" spacing={3} flexWrap="wrap" useFlexGap>
            <Typography variant="caption" color="text.secondary">
              Status:
            </Typography>
            <Stack direction="row" spacing={0.5} alignItems="center">
              <HealthyIcon sx={{ fontSize: 14, color: '#4caf50' }} />
              <Typography variant="caption">Operational</Typography>
            </Stack>
            <Stack direction="row" spacing={0.5} alignItems="center">
              <WarningIcon sx={{ fontSize: 14, color: '#ff9800' }} />
              <Typography variant="caption">Degraded</Typography>
            </Stack>
            <Stack direction="row" spacing={0.5} alignItems="center">
              <ErrorIcon sx={{ fontSize: 14, color: '#f44336' }} />
              <Typography variant="caption">Outage</Typography>
            </Stack>
            {highlightImpact && selectedCIId && (
              <>
                <Divider orientation="vertical" flexItem />
                <Stack direction="row" spacing={0.5} alignItems="center">
                  <Box sx={{ width: 12, height: 12, backgroundColor: '#ffebee', border: '1px solid #f44336' }} />
                  <Typography variant="caption">Impact Zone</Typography>
                </Stack>
              </>
            )}
          </Stack>
        </Box>
      </Paper>

      {/* Detail panel */}
      {selectedCI && (
        <CIDetailPanel
          ci={selectedCI}
          relationships={relationships}
          allCIs={configItems}
          onNavigate={(id) => onCISelect?.(id)}
        />
      )}
    </Box>
  );
};

export default CIRelationshipDiagram;

// Relationship Diagram - CI relationship visualization component
// Part of ITSM Enhancement Plan - Phase 2.1

import React, { useMemo } from 'react';
import {
  Box,
  Typography,
  Paper,
  Stack,
  Chip,
  Avatar,
  Tooltip,
  IconButton,
  Divider,
} from '@mui/material';
import {
  Computer as ServerIcon,
  Apps as AppIcon,
  Storage as DatabaseIcon,
  Router as NetworkIcon,
  Cloud as CloudIcon,
  Person as PersonIcon,
  Link as LinkIcon,
  ArrowForward as ArrowIcon,
  ZoomIn as ZoomInIcon,
  ZoomOut as ZoomOutIcon,
  CenterFocusStrong as CenterIcon,
} from '@mui/icons-material';

export type CIType = 'server' | 'application' | 'database' | 'network' | 'cloud' | 'service' | 'other';
export type RelationshipType = 'runs_on' | 'depends_on' | 'connected_to' | 'installed_on' | 'uses';

export interface ConfigurationItemNode {
  id: number;
  name: string;
  ciNumber: string;
  ciType: CIType;
  status: 'operational' | 'degraded' | 'down' | 'maintenance';
  criticality: 'critical' | 'high' | 'medium' | 'low';
  owner?: string;
}

export interface CIRelationship {
  id: number;
  parentId: number;
  childId: number;
  type: RelationshipType;
  description?: string;
}

export interface RelationshipDiagramProps {
  centerCI: ConfigurationItemNode;
  relatedCIs: ConfigurationItemNode[];
  relationships: CIRelationship[];
  onCIClick?: (ci: ConfigurationItemNode) => void;
  onRelationshipClick?: (relationship: CIRelationship) => void;
  maxDepth?: number;
}

const getCITypeIcon = (type: CIType) => {
  switch (type) {
    case 'server':
      return <ServerIcon />;
    case 'application':
      return <AppIcon />;
    case 'database':
      return <DatabaseIcon />;
    case 'network':
      return <NetworkIcon />;
    case 'cloud':
      return <CloudIcon />;
    case 'service':
      return <PersonIcon />;
    default:
      return <ServerIcon />;
  }
};

const getCITypeColor = (type: CIType): string => {
  switch (type) {
    case 'server':
      return '#1976d2';
    case 'application':
      return '#9c27b0';
    case 'database':
      return '#ff9800';
    case 'network':
      return '#4caf50';
    case 'cloud':
      return '#00bcd4';
    case 'service':
      return '#e91e63';
    default:
      return '#757575';
  }
};

const getStatusColor = (status: ConfigurationItemNode['status']): string => {
  switch (status) {
    case 'operational':
      return '#4caf50';
    case 'degraded':
      return '#ff9800';
    case 'down':
      return '#f44336';
    case 'maintenance':
      return '#9e9e9e';
    default:
      return '#9e9e9e';
  }
};

const getRelationshipLabel = (type: RelationshipType): string => {
  switch (type) {
    case 'runs_on':
      return 'Runs On';
    case 'depends_on':
      return 'Depends On';
    case 'connected_to':
      return 'Connected To';
    case 'installed_on':
      return 'Installed On';
    case 'uses':
      return 'Uses';
    default:
      return type;
  }
};

interface CINodeProps {
  ci: ConfigurationItemNode;
  isCenter?: boolean;
  onClick?: (ci: ConfigurationItemNode) => void;
}

const CINode: React.FC<CINodeProps> = ({ ci, isCenter = false, onClick }) => {
  const typeColor = getCITypeColor(ci.ciType);
  const statusColor = getStatusColor(ci.status);

  return (
    <Tooltip
      title={
        <Box>
          <Typography variant="body2" fontWeight={600}>{ci.name}</Typography>
          <Typography variant="caption">{ci.ciNumber}</Typography>
          <Divider sx={{ my: 0.5, borderColor: 'rgba(255,255,255,0.3)' }} />
          <Typography variant="caption">Type: {ci.ciType}</Typography>
          <br />
          <Typography variant="caption">Status: {ci.status}</Typography>
          <br />
          <Typography variant="caption">Criticality: {ci.criticality}</Typography>
          {ci.owner && (
            <>
              <br />
              <Typography variant="caption">Owner: {ci.owner}</Typography>
            </>
          )}
        </Box>
      }
    >
      <Paper
        elevation={isCenter ? 4 : 1}
        onClick={() => onClick?.(ci)}
        sx={{
          p: 1.5,
          cursor: 'pointer',
          border: isCenter ? `3px solid ${typeColor}` : `1px solid #e0e0e0`,
          borderRadius: 2,
          backgroundColor: isCenter ? `${typeColor}10` : 'white',
          transition: 'all 0.2s ease',
          minWidth: 120,
          '&:hover': {
            transform: 'scale(1.05)',
            boxShadow: 3,
          },
        }}
      >
        <Stack alignItems="center" spacing={0.5}>
          <Avatar
            sx={{
              backgroundColor: typeColor,
              width: isCenter ? 48 : 36,
              height: isCenter ? 48 : 36,
            }}
          >
            {getCITypeIcon(ci.ciType)}
          </Avatar>
          <Typography
            variant={isCenter ? 'body1' : 'body2'}
            fontWeight={isCenter ? 600 : 400}
            noWrap
            sx={{ maxWidth: 100 }}
          >
            {ci.name}
          </Typography>
          <Chip
            size="small"
            label={ci.status}
            sx={{
              height: 18,
              fontSize: '0.65rem',
              backgroundColor: `${statusColor}20`,
              color: statusColor,
            }}
          />
        </Stack>
      </Paper>
    </Tooltip>
  );
};

interface RelationshipLineProps {
  relationship: CIRelationship;
  direction: 'incoming' | 'outgoing';
  onClick?: (relationship: CIRelationship) => void;
}

const RelationshipLine: React.FC<RelationshipLineProps> = ({
  relationship,
  direction,
  onClick,
}) => {
  return (
    <Stack
      direction="row"
      alignItems="center"
      spacing={0.5}
      sx={{ px: 1 }}
      onClick={() => onClick?.(relationship)}
    >
      {direction === 'incoming' && <ArrowIcon fontSize="small" color="action" />}
      <Chip
        size="small"
        icon={<LinkIcon fontSize="small" />}
        label={getRelationshipLabel(relationship.type)}
        variant="outlined"
        sx={{ cursor: 'pointer', fontSize: '0.7rem' }}
      />
      {direction === 'outgoing' && <ArrowIcon fontSize="small" color="action" />}
    </Stack>
  );
};

export const RelationshipDiagram: React.FC<RelationshipDiagramProps> = ({
  centerCI,
  relatedCIs,
  relationships,
  onCIClick,
  onRelationshipClick,
}) => {
  // Group relationships by direction
  const { upstream, downstream } = useMemo(() => {
    const upstream: Array<{ ci: ConfigurationItemNode; rel: CIRelationship }> = [];
    const downstream: Array<{ ci: ConfigurationItemNode; rel: CIRelationship }> = [];

    relationships.forEach((rel) => {
      if (rel.childId === centerCI.id) {
        // This CI depends on parent (upstream)
        const parentCI = relatedCIs.find((c) => c.id === rel.parentId);
        if (parentCI) {
          upstream.push({ ci: parentCI, rel });
        }
      } else if (rel.parentId === centerCI.id) {
        // Other CIs depend on this one (downstream)
        const childCI = relatedCIs.find((c) => c.id === rel.childId);
        if (childCI) {
          downstream.push({ ci: childCI, rel });
        }
      }
    });

    return { upstream, downstream };
  }, [centerCI, relatedCIs, relationships]);

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 2 }}>
        <Typography variant="subtitle1" fontWeight={600}>
          CI Relationships
        </Typography>
        <Stack direction="row" spacing={0.5}>
          <Tooltip title="Zoom In">
            <IconButton size="small">
              <ZoomInIcon fontSize="small" />
            </IconButton>
          </Tooltip>
          <Tooltip title="Zoom Out">
            <IconButton size="small">
              <ZoomOutIcon fontSize="small" />
            </IconButton>
          </Tooltip>
          <Tooltip title="Center">
            <IconButton size="small">
              <CenterIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        </Stack>
      </Stack>

      {/* Legend */}
      <Stack direction="row" spacing={2} sx={{ mb: 2 }} flexWrap="wrap">
        {(['server', 'application', 'database', 'network', 'cloud'] as CIType[]).map((type) => (
          <Stack key={type} direction="row" alignItems="center" spacing={0.5}>
            <Avatar sx={{ width: 20, height: 20, backgroundColor: getCITypeColor(type) }}>
              {React.cloneElement(getCITypeIcon(type), { sx: { fontSize: 12 } })}
            </Avatar>
            <Typography variant="caption" sx={{ textTransform: 'capitalize' }}>
              {type}
            </Typography>
          </Stack>
        ))}
      </Stack>

      <Divider sx={{ mb: 2 }} />

      {/* Diagram layout */}
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 4, py: 2 }}>
        {/* Upstream (dependencies) */}
        <Box>
          {upstream.length > 0 ? (
            <Stack spacing={2}>
              <Typography variant="caption" color="text.secondary" textAlign="center">
                Depends On ({upstream.length})
              </Typography>
              {upstream.map(({ ci, rel }) => (
                <Stack key={ci.id} direction="row" alignItems="center" spacing={1}>
                  <CINode ci={ci} onClick={onCIClick} />
                  <RelationshipLine
                    relationship={rel}
                    direction="outgoing"
                    onClick={onRelationshipClick}
                  />
                </Stack>
              ))}
            </Stack>
          ) : (
            <Typography variant="caption" color="text.secondary">
              No dependencies
            </Typography>
          )}
        </Box>

        {/* Center CI */}
        <Box sx={{ mx: 4 }}>
          <CINode ci={centerCI} isCenter onClick={onCIClick} />
        </Box>

        {/* Downstream (dependents) */}
        <Box>
          {downstream.length > 0 ? (
            <Stack spacing={2}>
              <Typography variant="caption" color="text.secondary" textAlign="center">
                Dependents ({downstream.length})
              </Typography>
              {downstream.map(({ ci, rel }) => (
                <Stack key={ci.id} direction="row" alignItems="center" spacing={1}>
                  <RelationshipLine
                    relationship={rel}
                    direction="incoming"
                    onClick={onRelationshipClick}
                  />
                  <CINode ci={ci} onClick={onCIClick} />
                </Stack>
              ))}
            </Stack>
          ) : (
            <Typography variant="caption" color="text.secondary">
              No dependents
            </Typography>
          )}
        </Box>
      </Box>

      {/* Summary */}
      <Divider sx={{ mt: 2, mb: 1 }} />
      <Stack direction="row" spacing={2} justifyContent="center">
        <Chip
          size="small"
          label={`${upstream.length} Dependencies`}
          color="primary"
          variant="outlined"
        />
        <Chip
          size="small"
          label={`${downstream.length} Dependents`}
          color="secondary"
          variant="outlined"
        />
        <Chip
          size="small"
          label={`${relationships.length} Total Relationships`}
          variant="outlined"
        />
      </Stack>
    </Paper>
  );
};

export default RelationshipDiagram;

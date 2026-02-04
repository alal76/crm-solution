// Service Map - Service dependency visualization
// Part of ITSM Enhancement Plan - Phase 5.2

import React, { useState, useMemo, useCallback } from 'react';
import {
  Box,
  Paper,
  Typography,
  Stack,
  Chip,
  IconButton,
  Tooltip,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Divider,
  Badge,
  Card,
  CardContent,
  ToggleButton,
  ToggleButtonGroup,
  TextField,
  InputAdornment,
  Collapse,
} from '@mui/material';
import {
  Cloud as ServiceIcon,
  Storage as DatabaseIcon,
  Api as ApiIcon,
  Web as WebIcon,
  Security as SecurityIcon,
  Memory as InfraIcon,
  Warning as WarningIcon,
  Error as ErrorIcon,
  CheckCircle as HealthyIcon,
  Help as UnknownIcon,
  ZoomIn as ZoomInIcon,
  ZoomOut as ZoomOutIcon,
  CenterFocusStrong as FitIcon,
  Info as InfoIcon,
  ExpandMore as ExpandIcon,
  ExpandLess as CollapseIcon,
  Search as SearchIcon,
  Layers as LayersIcon,
  AccountTree as TreeIcon,
} from '@mui/icons-material';

export type ServiceStatus = 'healthy' | 'degraded' | 'outage' | 'unknown';
export type ServiceType = 'application' | 'database' | 'api' | 'web' | 'security' | 'infrastructure';

export interface ServiceNode {
  id: string;
  name: string;
  type: ServiceType;
  status: ServiceStatus;
  description?: string;
  owner?: string;
  criticality: 'low' | 'medium' | 'high' | 'critical';
  tier?: number; // For vertical layout positioning
  dependencies: string[]; // IDs of services this depends on
  consumers?: string[]; // IDs of services that depend on this
  metrics?: {
    availability?: number;
    responseTime?: number;
    errorRate?: number;
    activeIncidents?: number;
  };
}

export interface ServiceMapProps {
  services: ServiceNode[];
  selectedServiceId?: string;
  onServiceSelect?: (serviceId: string | null) => void;
  onServiceClick?: (serviceId: string) => void;
  showLegend?: boolean;
  showMiniMap?: boolean;
  viewMode?: 'layered' | 'force' | 'hierarchy';
  highlightPath?: boolean;
}

const getServiceTypeIcon = (type: ServiceType) => {
  switch (type) {
    case 'application':
      return <ServiceIcon />;
    case 'database':
      return <DatabaseIcon />;
    case 'api':
      return <ApiIcon />;
    case 'web':
      return <WebIcon />;
    case 'security':
      return <SecurityIcon />;
    case 'infrastructure':
      return <InfraIcon />;
    default:
      return <ServiceIcon />;
  }
};

const getStatusIcon = (status: ServiceStatus) => {
  switch (status) {
    case 'healthy':
      return <HealthyIcon fontSize="small" sx={{ color: '#4caf50' }} />;
    case 'degraded':
      return <WarningIcon fontSize="small" sx={{ color: '#ff9800' }} />;
    case 'outage':
      return <ErrorIcon fontSize="small" sx={{ color: '#f44336' }} />;
    default:
      return <UnknownIcon fontSize="small" sx={{ color: '#9e9e9e' }} />;
  }
};

const getStatusColor = (status: ServiceStatus): string => {
  switch (status) {
    case 'healthy':
      return '#4caf50';
    case 'degraded':
      return '#ff9800';
    case 'outage':
      return '#f44336';
    default:
      return '#9e9e9e';
  }
};

const getCriticalityColor = (criticality: string): string => {
  switch (criticality) {
    case 'critical':
      return '#9c27b0';
    case 'high':
      return '#f44336';
    case 'medium':
      return '#ff9800';
    default:
      return '#4caf50';
  }
};

// Service node card component
const ServiceCard: React.FC<{
  service: ServiceNode;
  isSelected: boolean;
  isHighlighted: boolean;
  onSelect: () => void;
  compact?: boolean;
}> = ({ service, isSelected, isHighlighted, onSelect, compact = false }) => (
  <Card
    variant="outlined"
    sx={{
      cursor: 'pointer',
      borderWidth: isSelected ? 2 : 1,
      borderColor: isSelected
        ? 'primary.main'
        : isHighlighted
        ? getStatusColor(service.status)
        : 'divider',
      backgroundColor: isHighlighted ? 'action.hover' : 'background.paper',
      opacity: isHighlighted || isSelected ? 1 : 0.7,
      transition: 'all 0.2s',
      minWidth: compact ? 120 : 180,
      '&:hover': {
        borderColor: 'primary.main',
        boxShadow: 1,
      },
    }}
    onClick={onSelect}
  >
    <CardContent sx={{ p: compact ? 1 : 2, '&:last-child': { pb: compact ? 1 : 2 } }}>
      <Stack direction="row" alignItems="center" spacing={1}>
        <Badge
          overlap="circular"
          anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
          badgeContent={getStatusIcon(service.status)}
        >
          <Box
            sx={{
              p: 0.5,
              borderRadius: 1,
              backgroundColor: `${getStatusColor(service.status)}20`,
              display: 'flex',
            }}
          >
            {React.cloneElement(getServiceTypeIcon(service.type), {
              sx: { color: getStatusColor(service.status) },
            })}
          </Box>
        </Badge>
        <Box sx={{ flex: 1, minWidth: 0 }}>
          <Typography
            variant={compact ? 'caption' : 'body2'}
            fontWeight={600}
            noWrap
          >
            {service.name}
          </Typography>
          {!compact && (
            <Stack direction="row" spacing={0.5} alignItems="center">
              <Chip
                label={service.criticality}
                size="small"
                sx={{
                  height: 16,
                  fontSize: '0.65rem',
                  backgroundColor: `${getCriticalityColor(service.criticality)}20`,
                  color: getCriticalityColor(service.criticality),
                }}
              />
              {service.metrics?.activeIncidents && service.metrics.activeIncidents > 0 && (
                <Chip
                  label={`${service.metrics.activeIncidents} incidents`}
                  size="small"
                  color="error"
                  sx={{ height: 16, fontSize: '0.65rem' }}
                />
              )}
            </Stack>
          )}
        </Box>
      </Stack>
    </CardContent>
  </Card>
);

// Service detail panel
const ServiceDetailPanel: React.FC<{
  service: ServiceNode;
  allServices: ServiceNode[];
  onClose: () => void;
  onNavigate: (serviceId: string) => void;
}> = ({ service, allServices, onClose, onNavigate }) => {
  const dependencies = useMemo(
    () => allServices.filter((s) => service.dependencies.includes(s.id)),
    [service.dependencies, allServices]
  );

  const consumers = useMemo(
    () => allServices.filter((s) => s.dependencies.includes(service.id)),
    [service.id, allServices]
  );

  return (
    <Paper sx={{ p: 2, width: 320 }}>
      <Stack direction="row" alignItems="flex-start" justifyContent="space-between" sx={{ mb: 2 }}>
        <Stack direction="row" spacing={1} alignItems="center">
          {React.cloneElement(getServiceTypeIcon(service.type), {
            sx: { color: getStatusColor(service.status), fontSize: 28 },
          })}
          <Box>
            <Typography variant="subtitle1" fontWeight={600}>
              {service.name}
            </Typography>
            <Stack direction="row" spacing={1} alignItems="center">
              {getStatusIcon(service.status)}
              <Typography variant="caption" color="text.secondary">
                {service.status.charAt(0).toUpperCase() + service.status.slice(1)}
              </Typography>
            </Stack>
          </Box>
        </Stack>
        <IconButton size="small" onClick={onClose}>
          <CollapseIcon />
        </IconButton>
      </Stack>

      {service.description && (
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {service.description}
        </Typography>
      )}

      <Divider sx={{ my: 2 }} />

      {/* Metrics */}
      {service.metrics && (
        <Box sx={{ mb: 2 }}>
          <Typography variant="subtitle2" gutterBottom>
            Metrics
          </Typography>
          <Stack spacing={1}>
            {service.metrics.availability !== undefined && (
              <Stack direction="row" justifyContent="space-between">
                <Typography variant="caption">Availability</Typography>
                <Chip
                  label={`${service.metrics.availability}%`}
                  size="small"
                  color={service.metrics.availability >= 99.9 ? 'success' : 'warning'}
                  sx={{ height: 20 }}
                />
              </Stack>
            )}
            {service.metrics.responseTime !== undefined && (
              <Stack direction="row" justifyContent="space-between">
                <Typography variant="caption">Response Time</Typography>
                <Typography variant="caption">{service.metrics.responseTime}ms</Typography>
              </Stack>
            )}
            {service.metrics.errorRate !== undefined && (
              <Stack direction="row" justifyContent="space-between">
                <Typography variant="caption">Error Rate</Typography>
                <Chip
                  label={`${service.metrics.errorRate}%`}
                  size="small"
                  color={service.metrics.errorRate < 1 ? 'success' : 'error'}
                  sx={{ height: 20 }}
                />
              </Stack>
            )}
          </Stack>
        </Box>
      )}

      <Divider sx={{ my: 2 }} />

      {/* Dependencies */}
      <Box sx={{ mb: 2 }}>
        <Typography variant="subtitle2" gutterBottom>
          Depends On ({dependencies.length})
        </Typography>
        {dependencies.length > 0 ? (
          <List dense disablePadding>
            {dependencies.map((dep) => (
              <ListItem
                key={dep.id}
                disablePadding
                sx={{ cursor: 'pointer' }}
                onClick={() => onNavigate(dep.id)}
              >
                <ListItemIcon sx={{ minWidth: 32 }}>
                  {getStatusIcon(dep.status)}
                </ListItemIcon>
                <ListItemText
                  primary={dep.name}
                  primaryTypographyProps={{ variant: 'body2' }}
                />
              </ListItem>
            ))}
          </List>
        ) : (
          <Typography variant="caption" color="text.secondary">
            No dependencies
          </Typography>
        )}
      </Box>

      {/* Consumers */}
      <Box>
        <Typography variant="subtitle2" gutterBottom>
          Used By ({consumers.length})
        </Typography>
        {consumers.length > 0 ? (
          <List dense disablePadding>
            {consumers.map((cons) => (
              <ListItem
                key={cons.id}
                disablePadding
                sx={{ cursor: 'pointer' }}
                onClick={() => onNavigate(cons.id)}
              >
                <ListItemIcon sx={{ minWidth: 32 }}>
                  {getStatusIcon(cons.status)}
                </ListItemIcon>
                <ListItemText
                  primary={cons.name}
                  primaryTypographyProps={{ variant: 'body2' }}
                />
              </ListItem>
            ))}
          </List>
        ) : (
          <Typography variant="caption" color="text.secondary">
            No consumers
          </Typography>
        )}
      </Box>

      {service.owner && (
        <>
          <Divider sx={{ my: 2 }} />
          <Typography variant="caption" color="text.secondary">
            Owner: {service.owner}
          </Typography>
        </>
      )}
    </Paper>
  );
};

export const ServiceMap: React.FC<ServiceMapProps> = ({
  services,
  selectedServiceId,
  onServiceSelect,
  onServiceClick,
  showLegend = true,
  viewMode = 'layered',
  highlightPath = true,
}) => {
  const [zoom, setZoom] = useState(1);
  const [searchQuery, setSearchQuery] = useState('');
  const [view, setView] = useState(viewMode);
  const [showDetail, setShowDetail] = useState(false);

  // Group services by tier for layered view
  const servicesByTier = useMemo(() => {
    const tiers: Record<number, ServiceNode[]> = {};
    services.forEach((service) => {
      const tier = service.tier || 0;
      if (!tiers[tier]) tiers[tier] = [];
      tiers[tier].push(service);
    });
    return tiers;
  }, [services]);

  const sortedTiers = useMemo(
    () => Object.keys(servicesByTier).map(Number).sort((a, b) => a - b),
    [servicesByTier]
  );

  // Get highlighted services (selected + path)
  const highlightedServiceIds = useMemo(() => {
    if (!selectedServiceId || !highlightPath) return new Set<string>();

    const highlighted = new Set<string>();
    const selected = services.find((s) => s.id === selectedServiceId);
    if (!selected) return highlighted;

    highlighted.add(selectedServiceId);

    // Add dependencies
    const addDependencies = (serviceId: string) => {
      const svc = services.find((s) => s.id === serviceId);
      svc?.dependencies.forEach((depId) => {
        if (!highlighted.has(depId)) {
          highlighted.add(depId);
          addDependencies(depId);
        }
      });
    };

    // Add consumers
    const addConsumers = (serviceId: string) => {
      services.forEach((svc) => {
        if (svc.dependencies.includes(serviceId) && !highlighted.has(svc.id)) {
          highlighted.add(svc.id);
          addConsumers(svc.id);
        }
      });
    };

    addDependencies(selectedServiceId);
    addConsumers(selectedServiceId);

    return highlighted;
  }, [selectedServiceId, services, highlightPath]);

  // Filter services by search
  const filteredServices = useMemo(() => {
    if (!searchQuery) return services;
    const query = searchQuery.toLowerCase();
    return services.filter(
      (s) =>
        s.name.toLowerCase().includes(query) ||
        s.type.toLowerCase().includes(query) ||
        s.owner?.toLowerCase().includes(query)
    );
  }, [services, searchQuery]);

  const selectedService = useMemo(
    () => services.find((s) => s.id === selectedServiceId),
    [services, selectedServiceId]
  );

  const handleServiceClick = (serviceId: string) => {
    onServiceSelect?.(serviceId === selectedServiceId ? null : serviceId);
    onServiceClick?.(serviceId);
    setShowDetail(true);
  };

  const tierLabels: Record<number, string> = {
    0: 'Presentation',
    1: 'Application',
    2: 'Service',
    3: 'Data',
    4: 'Infrastructure',
  };

  return (
    <Box sx={{ display: 'flex', gap: 2, height: '100%' }}>
      {/* Main map area */}
      <Paper sx={{ flex: 1, p: 2, overflow: 'hidden' }}>
        {/* Toolbar */}
        <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 2 }}>
          <Stack direction="row" spacing={1} alignItems="center">
            <TextField
              size="small"
              placeholder="Search services..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon fontSize="small" />
                  </InputAdornment>
                ),
              }}
              sx={{ width: 200 }}
            />
            <ToggleButtonGroup
              value={view}
              exclusive
              onChange={(_, v) => v && setView(v)}
              size="small"
            >
              <ToggleButton value="layered">
                <Tooltip title="Layered View">
                  <LayersIcon fontSize="small" />
                </Tooltip>
              </ToggleButton>
              <ToggleButton value="hierarchy">
                <Tooltip title="Hierarchy View">
                  <TreeIcon fontSize="small" />
                </Tooltip>
              </ToggleButton>
            </ToggleButtonGroup>
          </Stack>

          <Stack direction="row" spacing={1}>
            <IconButton size="small" onClick={() => setZoom((z) => Math.min(z + 0.1, 2))}>
              <ZoomInIcon />
            </IconButton>
            <IconButton size="small" onClick={() => setZoom((z) => Math.max(z - 0.1, 0.5))}>
              <ZoomOutIcon />
            </IconButton>
            <IconButton size="small" onClick={() => setZoom(1)}>
              <FitIcon />
            </IconButton>
          </Stack>
        </Stack>

        {/* Service map visualization */}
        <Box
          sx={{
            transform: `scale(${zoom})`,
            transformOrigin: 'top left',
            transition: 'transform 0.2s',
          }}
        >
          {view === 'layered' && (
            <Stack spacing={3}>
              {sortedTiers.map((tier) => (
                <Box key={tier}>
                  <Typography
                    variant="caption"
                    color="text.secondary"
                    sx={{ mb: 1, display: 'block' }}
                  >
                    {tierLabels[tier] || `Tier ${tier}`}
                  </Typography>
                  <Stack
                    direction="row"
                    spacing={2}
                    flexWrap="wrap"
                    useFlexGap
                    sx={{
                      p: 2,
                      backgroundColor: 'grey.50',
                      borderRadius: 1,
                      minHeight: 80,
                    }}
                  >
                    {servicesByTier[tier]
                      .filter((s) => filteredServices.includes(s))
                      .map((service) => (
                        <ServiceCard
                          key={service.id}
                          service={service}
                          isSelected={service.id === selectedServiceId}
                          isHighlighted={
                            highlightedServiceIds.size === 0 ||
                            highlightedServiceIds.has(service.id)
                          }
                          onSelect={() => handleServiceClick(service.id)}
                        />
                      ))}
                  </Stack>
                </Box>
              ))}
            </Stack>
          )}

          {view === 'hierarchy' && (
            <Box sx={{ p: 2 }}>
              <Stack
                direction="row"
                spacing={4}
                flexWrap="wrap"
                useFlexGap
                justifyContent="center"
              >
                {filteredServices.map((service) => (
                  <ServiceCard
                    key={service.id}
                    service={service}
                    isSelected={service.id === selectedServiceId}
                    isHighlighted={
                      highlightedServiceIds.size === 0 ||
                      highlightedServiceIds.has(service.id)
                    }
                    onSelect={() => handleServiceClick(service.id)}
                    compact
                  />
                ))}
              </Stack>
            </Box>
          )}
        </Box>

        {/* Legend */}
        {showLegend && (
          <Box sx={{ mt: 3, pt: 2, borderTop: '1px solid', borderColor: 'divider' }}>
            <Stack direction="row" spacing={3} flexWrap="wrap" useFlexGap>
              <Stack direction="row" spacing={1} alignItems="center">
                <HealthyIcon sx={{ color: '#4caf50', fontSize: 16 }} />
                <Typography variant="caption">Healthy</Typography>
              </Stack>
              <Stack direction="row" spacing={1} alignItems="center">
                <WarningIcon sx={{ color: '#ff9800', fontSize: 16 }} />
                <Typography variant="caption">Degraded</Typography>
              </Stack>
              <Stack direction="row" spacing={1} alignItems="center">
                <ErrorIcon sx={{ color: '#f44336', fontSize: 16 }} />
                <Typography variant="caption">Outage</Typography>
              </Stack>
              <Divider orientation="vertical" flexItem />
              <Stack direction="row" spacing={1} alignItems="center">
                <ServiceIcon sx={{ fontSize: 16 }} />
                <Typography variant="caption">Application</Typography>
              </Stack>
              <Stack direction="row" spacing={1} alignItems="center">
                <DatabaseIcon sx={{ fontSize: 16 }} />
                <Typography variant="caption">Database</Typography>
              </Stack>
              <Stack direction="row" spacing={1} alignItems="center">
                <ApiIcon sx={{ fontSize: 16 }} />
                <Typography variant="caption">API</Typography>
              </Stack>
            </Stack>
          </Box>
        )}
      </Paper>

      {/* Detail panel */}
      {showDetail && selectedService && (
        <ServiceDetailPanel
          service={selectedService}
          allServices={services}
          onClose={() => {
            setShowDetail(false);
            onServiceSelect?.(null);
          }}
          onNavigate={(id) => {
            onServiceSelect?.(id);
          }}
        />
      )}
    </Box>
  );
};

export default ServiceMap;

/**
 * CRM Solution - Related Entities Panel Component
 * Copyright (C) 2024-2026 Abhishek Lal
 * 
 * Displays related/linked entities for any CRM entity with consistent styling.
 * Shows counts, lists, and quick actions for related records.
 */

import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Typography,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  ListItemSecondaryAction,
  Avatar,
  Chip,
  IconButton,
  Tooltip,
  CircularProgress,
  Collapse,
  Divider,
  Button,
  Badge,
  Paper,
  Stack,
  Alert,
} from '@mui/material';
import {
  Person as PersonIcon,
  Business as BusinessIcon,
  TrendingUp as OpportunityIcon,
  Assignment as ServiceIcon,
  Campaign as CampaignIcon,
  ContactPhone as ContactIcon,
  Email as EmailIcon,
  Phone as PhoneIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  OpenInNew as OpenInNewIcon,
  Visibility as ViewIcon,
  Add as AddIcon,
  Info as InfoIcon,
  AttachMoney as MoneyIcon,
  LocationOn as LocationIcon,
  Description as QuoteIcon,
  Gavel as ContractIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import apiClient from '../../services/apiClient';

export type RelatedEntityType = 
  | 'contacts' 
  | 'opportunities' 
  | 'serviceRequests' 
  | 'campaigns' 
  | 'accounts' 
  | 'activities'
  | 'quotes'
  | 'contracts';

interface RelatedEntity {
  id: number;
  name: string;
  subtitle?: string;
  status?: string;
  statusColor?: string;
  amount?: number;
  icon?: React.ReactNode;
  metadata?: Record<string, any>;
}

export interface RelatedEntitiesPanelProps {
  /** Type of parent entity */
  entityType: 'contact' | 'account' | 'lead' | 'opportunity' | 'serviceRequest' | 'campaign' | 'quote' | 'contract' | 
              'contacts' | 'accounts' | 'leads' | 'opportunities' | 'serviceRequests' | 'campaigns' | 'quotes' | 'contracts';
  /** ID of the parent entity */
  entityId: number;
  /** Which related entities to show */
  showRelated?: RelatedEntityType[];
  /** Whether to show collapsed sections by default */
  defaultCollapsed?: boolean;
  /** Maximum items to show per section before "Show more" */
  maxItemsPerSection?: number;
  /** Callback when a related entity is clicked */
  onEntityClick?: (type: RelatedEntityType, id: number) => void;
  /** Show add buttons for each section */
  showAddButtons?: boolean;
  /** Callback for add action */
  onAdd?: (type: RelatedEntityType) => void;
  /** Compact mode for smaller dialogs */
  compact?: boolean;
}

interface SectionState {
  expanded: boolean;
  loading: boolean;
  error: string | null;
  data: RelatedEntity[];
  total: number;
}

const entityTypeConfig: Record<RelatedEntityType, { 
  icon: React.ReactNode; 
  label: string; 
  singularLabel: string;
  path: string;
  color: string;
}> = {
  contacts: { 
    icon: <ContactIcon />, 
    label: 'Contacts', 
    singularLabel: 'Contact',
    path: '/contacts',
    color: '#6750A4'
  },
  opportunities: { 
    icon: <OpportunityIcon />, 
    label: 'Opportunities', 
    singularLabel: 'Opportunity',
    path: '/sales/opportunities',
    color: '#06A77D'
  },
  serviceRequests: { 
    icon: <ServiceIcon />, 
    label: 'Service Requests', 
    singularLabel: 'Service Request',
    path: '/service/requests',
    color: '#FF9800'
  },
  campaigns: { 
    icon: <CampaignIcon />, 
    label: 'Campaigns', 
    singularLabel: 'Campaign',
    path: '/marketing/campaigns',
    color: '#E91E63'
  },
  accounts: { 
    icon: <BusinessIcon />, 
    label: 'Accounts', 
    singularLabel: 'Account',
    path: '/accounts',
    color: '#2196F3'
  },
  activities: { 
    icon: <ServiceIcon />, 
    label: 'Activities', 
    singularLabel: 'Activity',
    path: '/activities',
    color: '#9C27B0'
  },
  quotes: { 
    icon: <QuoteIcon />, 
    label: 'Quotes', 
    singularLabel: 'Quote',
    path: '/sales/quotes',
    color: '#4CAF50'
  },
  contracts: { 
    icon: <ContractIcon />, 
    label: 'Contracts', 
    singularLabel: 'Contract',
    path: '/sales/contracts',
    color: '#795548'
  },
};

/**
 * RelatedEntitiesPanel - Displays related entities for any CRM record
 */
export const RelatedEntitiesPanel: React.FC<RelatedEntitiesPanelProps> = ({
  entityType,
  entityId,
  showRelated = ['contacts', 'opportunities', 'serviceRequests'],
  defaultCollapsed = false,
  maxItemsPerSection = 5,
  onEntityClick,
  showAddButtons = false,
  onAdd,
  compact = false,
}) => {
  const navigate = useNavigate();
  const [sections, setSections] = useState<Record<RelatedEntityType, SectionState>>(() => {
    const initialState: Record<string, SectionState> = {};
    showRelated.forEach(type => {
      initialState[type] = {
        expanded: !defaultCollapsed,
        loading: true,
        error: null,
        data: [],
        total: 0,
      };
    });
    return initialState as Record<RelatedEntityType, SectionState>;
  });

  // Fetch related entities based on entity type
  const fetchRelatedEntities = useCallback(async (relatedType: RelatedEntityType) => {
    setSections(prev => ({
      ...prev,
      [relatedType]: { ...prev[relatedType], loading: true, error: null }
    }));

    try {
      let endpoint = '';
      let data: RelatedEntity[] = [];
      let total = 0;

      // Determine endpoint based on parent entity type and related type
      switch (entityType) {
        case 'account':
          switch (relatedType) {
            case 'contacts':
              endpoint = `/accounts/${entityId}/contacts`;
              break;
            case 'opportunities':
              endpoint = `/opportunities?accountId=${entityId}`;
              break;
            case 'serviceRequests':
              endpoint = `/servicerequests?customerId=${entityId}`;
              break;
            case 'quotes':
              endpoint = `/quotes?accountId=${entityId}`;
              break;
            case 'contracts':
              endpoint = `/contracts?customerId=${entityId}`;
              break;
          }
          break;
        case 'contact':
          switch (relatedType) {
            case 'accounts':
              endpoint = `/contacts/${entityId}/accounts`;
              break;
            case 'opportunities':
              endpoint = `/opportunities?primaryContactId=${entityId}`;
              break;
            case 'activities':
              endpoint = `/activities?contactId=${entityId}`;
              break;
          }
          break;
        case 'opportunity':
          switch (relatedType) {
            case 'contacts':
              endpoint = `/opportunities/${entityId}/contacts`;
              break;
            case 'quotes':
              endpoint = `/quotes?opportunityId=${entityId}`;
              break;
            case 'activities':
              endpoint = `/activities?opportunityId=${entityId}`;
              break;
          }
          break;
        case 'lead':
          switch (relatedType) {
            case 'activities':
              endpoint = `/activities?leadId=${entityId}`;
              break;
          }
          break;
        case 'serviceRequest':
          switch (relatedType) {
            case 'contacts':
              endpoint = `/servicerequests/${entityId}/contacts`;
              break;
            case 'activities':
              endpoint = `/activities?serviceRequestId=${entityId}`;
              break;
          }
          break;
      }

      if (endpoint) {
        const response = await apiClient.get(endpoint);
        const items = Array.isArray(response.data) ? response.data : 
                     response.data?.items || response.data?.data || [];
        
        // Transform based on related type
        data = items.map((item: any) => transformToRelatedEntity(item, relatedType));
        total = response.data?.total || items.length;
      }

      setSections(prev => ({
        ...prev,
        [relatedType]: { 
          ...prev[relatedType], 
          loading: false, 
          data: data.slice(0, maxItemsPerSection * 2), // Keep some extra for "show more"
          total 
        }
      }));
    } catch (err: any) {
      setSections(prev => ({
        ...prev,
        [relatedType]: { 
          ...prev[relatedType], 
          loading: false, 
          error: err.response?.data?.message || 'Failed to load related data',
          data: [],
          total: 0
        }
      }));
    }
  }, [entityType, entityId, maxItemsPerSection]);

  // Transform API response to RelatedEntity format
  const transformToRelatedEntity = (item: any, type: RelatedEntityType): RelatedEntity => {
    switch (type) {
      case 'contacts':
        return {
          id: item.contactId || item.id,
          name: item.contactName || `${item.firstName || ''} ${item.lastName || ''}`.trim() || 'Unknown',
          subtitle: item.role || item.jobTitle || item.positionAtCustomer,
          status: item.isPrimaryContact ? 'Primary' : undefined,
          statusColor: item.isPrimaryContact ? '#06A77D' : undefined,
          metadata: { email: item.contactEmail || item.emailPrimary, phone: item.contactPhone || item.phonePrimary }
        };
      case 'opportunities':
        return {
          id: item.id,
          name: item.name,
          subtitle: getStageLabel(item.stage),
          status: getStageLabel(item.stage),
          statusColor: getStageColor(item.stage),
          amount: item.amount,
          metadata: { probability: item.probability, expectedCloseDate: item.expectedCloseDate }
        };
      case 'serviceRequests':
        return {
          id: item.id,
          name: item.title || item.subject,
          subtitle: item.description?.substring(0, 50),
          status: item.status,
          statusColor: getServiceStatusColor(item.status),
          metadata: { priority: item.priority, createdAt: item.createdAt }
        };
      case 'accounts':
        return {
          id: item.id,
          name: item.company || `${item.firstName || ''} ${item.lastName || ''}`.trim(),
          subtitle: item.industry,
          status: getLifecycleLabel(item.lifecycleStage),
          statusColor: getLifecycleColor(item.lifecycleStage),
          metadata: { email: item.email, phone: item.phone }
        };
      case 'quotes':
        return {
          id: item.id,
          name: item.quoteNumber || `Quote #${item.id}`,
          subtitle: item.subject,
          status: item.status,
          statusColor: getQuoteStatusColor(item.status),
          amount: item.totalAmount,
          metadata: { validUntil: item.validUntil }
        };
      case 'activities':
        return {
          id: item.id,
          name: item.subject || item.title,
          subtitle: item.activityType,
          status: item.status,
          statusColor: item.status === 'Completed' ? '#06A77D' : '#FF9800',
          metadata: { dueDate: item.dueDate, assignedTo: item.assignedToName }
        };
      default:
        return {
          id: item.id,
          name: item.name || item.title || `Item #${item.id}`,
          subtitle: item.description,
        };
    }
  };

  // Fetch all related data on mount
  useEffect(() => {
    if (entityId) {
      showRelated.forEach(type => fetchRelatedEntities(type));
    }
  }, [entityId, showRelated, fetchRelatedEntities]);

  const toggleSection = (type: RelatedEntityType) => {
    setSections(prev => ({
      ...prev,
      [type]: { ...prev[type], expanded: !prev[type].expanded }
    }));
  };

  const handleEntityClick = (type: RelatedEntityType, id: number) => {
    if (onEntityClick) {
      onEntityClick(type, id);
    } else {
      // Default navigation
      const config = entityTypeConfig[type];
      navigate(`${config.path}?id=${id}`);
    }
  };

  if (!entityId) {
    return (
      <Alert severity="info" icon={<InfoIcon />}>
        Save the record first to see related entities.
      </Alert>
    );
  }

  return (
    <Box sx={{ mt: compact ? 1 : 2 }}>
      {showRelated.map(type => {
        const section = sections[type];
        const config = entityTypeConfig[type];
        const displayData = section.expanded 
          ? section.data 
          : section.data.slice(0, maxItemsPerSection);

        return (
          <Paper 
            key={type} 
            elevation={0}
            sx={{ 
              mb: compact ? 1 : 2, 
              border: '1px solid',
              borderColor: 'divider',
              borderRadius: 2,
              overflow: 'hidden'
            }}
          >
            {/* Section Header */}
            <Box
              onClick={() => toggleSection(type)}
              sx={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                p: compact ? 1.5 : 2,
                cursor: 'pointer',
                backgroundColor: 'grey.50',
                '&:hover': { backgroundColor: 'grey.100' },
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                <Avatar sx={{ 
                  width: compact ? 28 : 36, 
                  height: compact ? 28 : 36, 
                  bgcolor: config.color,
                }}>
                  {React.cloneElement(config.icon as React.ReactElement, { 
                    sx: { fontSize: compact ? 16 : 20 } 
                  })}
                </Avatar>
                <Typography variant={compact ? 'body2' : 'subtitle1'} fontWeight={600}>
                  {config.label}
                </Typography>
                <Badge 
                  badgeContent={section.total} 
                  color="primary" 
                  max={99}
                  sx={{ ml: 1 }}
                />
              </Box>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                {showAddButtons && onAdd && (
                  <Tooltip title={`Add ${config.singularLabel}`}>
                    <IconButton 
                      size="small" 
                      onClick={(e) => { e.stopPropagation(); onAdd(type); }}
                      sx={{ color: config.color }}
                    >
                      <AddIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                )}
                <IconButton size="small">
                  {section.expanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                </IconButton>
              </Box>
            </Box>

            {/* Section Content */}
            <Collapse in={section.expanded}>
              <Divider />
              <Box sx={{ p: compact ? 1 : 2 }}>
                {section.loading ? (
                  <Box sx={{ display: 'flex', justifyContent: 'center', py: 3 }}>
                    <CircularProgress size={24} />
                  </Box>
                ) : section.error ? (
                  <Alert severity="error" sx={{ m: 1 }}>
                    {section.error}
                  </Alert>
                ) : section.data.length === 0 ? (
                  <Box sx={{ textAlign: 'center', py: 3 }}>
                    <Typography variant="body2" color="text.secondary">
                      No {config.label.toLowerCase()} linked yet
                    </Typography>
                    {showAddButtons && onAdd && (
                      <Button
                        size="small"
                        startIcon={<AddIcon />}
                        onClick={() => onAdd(type)}
                        sx={{ mt: 1 }}
                      >
                        Add {config.singularLabel}
                      </Button>
                    )}
                  </Box>
                ) : (
                  <>
                    <List dense disablePadding>
                      {displayData.map((entity, index) => (
                        <React.Fragment key={entity.id}>
                          {index > 0 && <Divider variant="inset" component="li" />}
                          <ListItem
                            sx={{ 
                              py: compact ? 0.5 : 1,
                              cursor: 'pointer',
                              '&:hover': { backgroundColor: 'action.hover' },
                              borderRadius: 1,
                            }}
                            onClick={() => handleEntityClick(type, entity.id)}
                          >
                            <ListItemAvatar sx={{ minWidth: compact ? 36 : 48 }}>
                              <Avatar sx={{ 
                                width: compact ? 28 : 36, 
                                height: compact ? 28 : 36,
                                bgcolor: entity.statusColor || 'grey.300',
                                fontSize: compact ? 12 : 14,
                              }}>
                                {entity.name.charAt(0).toUpperCase()}
                              </Avatar>
                            </ListItemAvatar>
                            <ListItemText
                              primary={
                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                  <Typography 
                                    variant={compact ? 'body2' : 'body1'} 
                                    fontWeight={500}
                                    noWrap
                                    sx={{ maxWidth: 200 }}
                                  >
                                    {entity.name}
                                  </Typography>
                                  {entity.status && (
                                    <Chip
                                      label={entity.status}
                                      size="small"
                                      sx={{
                                        height: 20,
                                        fontSize: '0.7rem',
                                        backgroundColor: entity.statusColor ? `${entity.statusColor}20` : undefined,
                                        color: entity.statusColor,
                                        fontWeight: 600,
                                      }}
                                    />
                                  )}
                                </Box>
                              }
                              secondary={
                                <Stack direction="row" spacing={1} alignItems="center">
                                  {entity.subtitle && (
                                    <Typography variant="caption" color="text.secondary" noWrap>
                                      {entity.subtitle}
                                    </Typography>
                                  )}
                                  {entity.amount !== undefined && entity.amount > 0 && (
                                    <Typography variant="caption" fontWeight={600} color="success.main">
                                      ${entity.amount.toLocaleString()}
                                    </Typography>
                                  )}
                                  {entity.metadata?.email && (
                                    <Tooltip title={entity.metadata.email}>
                                      <EmailIcon sx={{ fontSize: 14, color: 'text.disabled' }} />
                                    </Tooltip>
                                  )}
                                  {entity.metadata?.phone && (
                                    <Tooltip title={entity.metadata.phone}>
                                      <PhoneIcon sx={{ fontSize: 14, color: 'text.disabled' }} />
                                    </Tooltip>
                                  )}
                                </Stack>
                              }
                            />
                            <ListItemSecondaryAction>
                              <Tooltip title={`View ${config.singularLabel}`}>
                                <IconButton 
                                  size="small"
                                  onClick={(e) => { 
                                    e.stopPropagation(); 
                                    handleEntityClick(type, entity.id); 
                                  }}
                                >
                                  <ViewIcon fontSize="small" />
                                </IconButton>
                              </Tooltip>
                            </ListItemSecondaryAction>
                          </ListItem>
                        </React.Fragment>
                      ))}
                    </List>
                    {section.total > maxItemsPerSection && (
                      <Box sx={{ textAlign: 'center', mt: 1 }}>
                        <Button
                          size="small"
                          onClick={() => navigate(`${config.path}?${entityType}Id=${entityId}`)}
                          endIcon={<OpenInNewIcon />}
                        >
                          View all {section.total} {config.label.toLowerCase()}
                        </Button>
                      </Box>
                    )}
                  </>
                )}
              </Box>
            </Collapse>
          </Paper>
        );
      })}
    </Box>
  );
};

// Helper functions for status colors and labels
function getStageLabel(stage: number): string {
  const stages = ['Discovery', 'Qualification', 'Proposal', 'Negotiation', 'Closed Won', 'Closed Lost'];
  return stages[stage] || 'Unknown';
}

function getStageColor(stage: number): string {
  const colors = ['#9e9e9e', '#2196f3', '#ff9800', '#9c27b0', '#4caf50', '#f44336'];
  return colors[stage] || '#9e9e9e';
}

function getServiceStatusColor(status: string): string {
  const colors: Record<string, string> = {
    'Open': '#2196f3',
    'In Progress': '#ff9800',
    'Resolved': '#4caf50',
    'Closed': '#9e9e9e',
    'Escalated': '#f44336',
  };
  return colors[status] || '#9e9e9e';
}

function getLifecycleLabel(stage: number): string {
  const stages = ['Lead', 'Prospect', 'Customer', 'Churned', 'Inactive'];
  return stages[stage] || 'Unknown';
}

function getLifecycleColor(stage: number): string {
  const colors = ['#9e9e9e', '#2196f3', '#4caf50', '#f44336', '#795548'];
  return colors[stage] || '#9e9e9e';
}

function getQuoteStatusColor(status: string): string {
  const colors: Record<string, string> = {
    'Draft': '#9e9e9e',
    'Sent': '#2196f3',
    'Accepted': '#4caf50',
    'Rejected': '#f44336',
    'Expired': '#795548',
  };
  return colors[status] || '#9e9e9e';
}

export default RelatedEntitiesPanel;

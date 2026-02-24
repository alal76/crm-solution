/**
 * CampaignDashboardWidgets - Marketing campaign dashboard widget components
 * 
 * Provides reusable widgets for displaying campaign performance metrics,
 * engagement data, and real-time campaign status.
 * 
 * TODO-GAP-MARKETING-001
 */

import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Grid,
  Typography,
  Chip,
  LinearProgress,
  CircularProgress,
  IconButton,
  Tooltip,
  Skeleton,
  Paper,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Divider,
} from '@mui/material';
import {
  TrendingUp,
  TrendingDown,
  TrendingFlat,
  Refresh,
  Email,
  Mouse,
  People,
  MonetizationOn,
  Campaign,
  Schedule,
  CheckCircle,
  Error,
  Warning,
} from '@mui/icons-material';
import { campaignService, CampaignSummary } from '../../services/campaignService';

// ============================================================================
// Types
// ============================================================================

export interface CampaignMetric {
  label: string;
  value: number | string;
  previousValue?: number;
  format?: 'number' | 'percent' | 'currency';
  trend?: 'up' | 'down' | 'flat';
  icon?: React.ReactNode;
}

export interface CampaignDashboardWidgetsProps {
  refreshInterval?: number;  // in milliseconds
  onCampaignSelect?: (campaignId: number) => void;
}

// ============================================================================
// Helper Functions
// ============================================================================

const formatValue = (value: number | string, format?: string): string => {
  if (typeof value === 'string') return value;
  
  switch (format) {
    case 'percent':
      return `${value.toFixed(1)}%`;
    case 'currency':
      return `$${value.toLocaleString('en-US', { minimumFractionDigits: 2 })}`;
    case 'number':
    default:
      return value.toLocaleString();
  }
};

const getTrendIcon = (trend?: string) => {
  switch (trend) {
    case 'up':
      return <TrendingUp color="success" fontSize="small" />;
    case 'down':
      return <TrendingDown color="error" fontSize="small" />;
    default:
      return <TrendingFlat color="disabled" fontSize="small" />;
  }
};

const getStatusColor = (status: string): 'success' | 'warning' | 'error' | 'default' => {
  switch (status.toLowerCase()) {
    case 'active':
    case 'running':
      return 'success';
    case 'scheduled':
    case 'pending':
      return 'warning';
    case 'paused':
    case 'stopped':
      return 'error';
    default:
      return 'default';
  }
};

// ============================================================================
// Sub-Components
// ============================================================================

interface MetricCardProps {
  metric: CampaignMetric;
  loading?: boolean;
}

const MetricCard: React.FC<MetricCardProps> = ({ metric, loading }) => {
  if (loading) {
    return (
      <Card variant="outlined">
        <CardContent>
          <Skeleton variant="text" width="60%" height={24} />
          <Skeleton variant="text" width="40%" height={40} />
          <Skeleton variant="rectangular" height={4} sx={{ mt: 1 }} />
        </CardContent>
      </Card>
    );
  }

  return (
    <Card variant="outlined">
      <CardContent>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Typography variant="body2" color="text.secondary">
            {metric.label}
          </Typography>
          {metric.icon}
        </Box>
        <Box display="flex" alignItems="baseline" gap={1} mt={1}>
          <Typography variant="h4" fontWeight="bold">
            {formatValue(metric.value, metric.format)}
          </Typography>
          {metric.trend && getTrendIcon(metric.trend)}
        </Box>
        {metric.previousValue !== undefined && (
          <Typography variant="caption" color="text.secondary">
            Previous: {formatValue(metric.previousValue, metric.format)}
          </Typography>
        )}
      </CardContent>
    </Card>
  );
};

interface ActiveCampaignListProps {
  campaigns: CampaignSummary[];
  loading?: boolean;
  onSelect?: (campaignId: number) => void;
}

const ActiveCampaignList: React.FC<ActiveCampaignListProps> = ({ 
  campaigns, 
  loading, 
  onSelect 
}) => {
  if (loading) {
    return (
      <List>
        {[1, 2, 3].map((i) => (
          <ListItem key={i}>
            <ListItemIcon>
              <Skeleton variant="circular" width={24} height={24} />
            </ListItemIcon>
            <ListItemText
              primary={<Skeleton variant="text" width="60%" />}
              secondary={<Skeleton variant="text" width="40%" />}
            />
          </ListItem>
        ))}
      </List>
    );
  }

  return (
    <List>
      {campaigns.slice(0, 5).map((campaign, index) => (
        <React.Fragment key={campaign.id}>
          {index > 0 && <Divider />}
          <ListItem
            button
            onClick={() => onSelect?.(campaign.id)}
            sx={{ cursor: 'pointer' }}
          >
            <ListItemIcon>
              <Campaign color="primary" />
            </ListItemIcon>
            <ListItemText
              primary={campaign.name}
              secondary={
                <Box display="flex" alignItems="center" gap={1}>
                  <Chip
                    size="small"
                    label={campaign.status}
                    color={getStatusColor(campaign.status)}
                    sx={{ height: 20 }}
                  />
                  <Typography variant="caption">
                    {campaign.type || 'Email'}
                  </Typography>
                </Box>
              }
            />
          </ListItem>
        </React.Fragment>
      ))}
    </List>
  );
};

interface CampaignProgressProps {
  campaign: CampaignSummary;
}

const CampaignProgress: React.FC<CampaignProgressProps> = ({ campaign }) => {
  // Calculate progress based on sent/total
  const total = campaign.totalRecipients || 1;
  const sent = campaign.sentCount || 0;
  const progress = Math.min((sent / total) * 100, 100);

  return (
    <Box mb={2}>
      <Box display="flex" justifyContent="space-between" mb={0.5}>
        <Typography variant="body2" noWrap sx={{ maxWidth: '60%' }}>
          {campaign.name}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {sent.toLocaleString()} / {total.toLocaleString()}
        </Typography>
      </Box>
      <LinearProgress
        variant="determinate"
        value={progress}
        color={progress >= 100 ? 'success' : 'primary'}
        sx={{ height: 6, borderRadius: 3 }}
      />
    </Box>
  );
};

// ============================================================================
// Main Component
// ============================================================================

export const CampaignDashboardWidgets: React.FC<CampaignDashboardWidgetsProps> = ({
  refreshInterval = 60000,
  onCampaignSelect,
}) => {
  const [loading, setLoading] = useState(true);
  const [metrics, setMetrics] = useState<CampaignMetric[]>([]);
  const [activeCampaigns, setActiveCampaigns] = useState<CampaignSummary[]>([]);
  const [lastUpdated, setLastUpdated] = useState<Date>(new Date());

  const fetchData = async () => {
    try {
      setLoading(true);
      
      // Fetch campaign metrics
      const response = await campaignService.getAll();
      const campaigns = response.items || [];
      
      const active = campaigns.filter(c => 
        c.status?.toLowerCase() === 'active' || c.status?.toLowerCase() === 'running'
      );
      
      setActiveCampaigns(active);
      
      // Calculate aggregate metrics
      const totalSent = campaigns.reduce((sum, c) => sum + (c.sentCount || 0), 0);
      const totalDelivered = campaigns.reduce((sum, c) => sum + (c.deliveredCount || 0), 0);
      const totalOpened = campaigns.reduce((sum, c) => sum + (c.openedCount || 0), 0);
      const totalClicked = campaigns.reduce((sum, c) => sum + (c.clickedCount || 0), 0);
      
      const openRate = totalDelivered > 0 ? (totalOpened / totalDelivered) * 100 : 0;
      const clickRate = totalOpened > 0 ? (totalClicked / totalOpened) * 100 : 0;
      
      setMetrics([
        {
          label: 'Active Campaigns',
          value: active.length,
          icon: <Campaign color="primary" />,
          format: 'number',
        },
        {
          label: 'Total Emails Sent',
          value: totalSent,
          icon: <Email color="action" />,
          format: 'number',
        },
        {
          label: 'Open Rate',
          value: openRate,
          icon: <People color="success" />,
          format: 'percent',
          trend: openRate > 20 ? 'up' : openRate < 15 ? 'down' : 'flat',
        },
        {
          label: 'Click Rate',
          value: clickRate,
          icon: <Mouse color="info" />,
          format: 'percent',
          trend: clickRate > 5 ? 'up' : clickRate < 2 ? 'down' : 'flat',
        },
      ]);
      
      setLastUpdated(new Date());
    } catch (error) {
      console.error('Error fetching campaign data:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
    
    const interval = setInterval(fetchData, refreshInterval);
    return () => clearInterval(interval);
  }, [refreshInterval]);

  return (
    <Box>
      {/* Header */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
        <Typography variant="h6">Campaign Overview</Typography>
        <Box display="flex" alignItems="center" gap={1}>
          <Typography variant="caption" color="text.secondary">
            Last updated: {lastUpdated.toLocaleTimeString()}
          </Typography>
          <Tooltip title="Refresh">
            <IconButton size="small" onClick={fetchData} disabled={loading}>
              {loading ? <CircularProgress size={18} /> : <Refresh />}
            </IconButton>
          </Tooltip>
        </Box>
      </Box>

      {/* Metrics Grid */}
      <Grid container spacing={2} mb={3}>
        {metrics.map((metric, index) => (
          <Grid item xs={12} sm={6} md={3} key={index}>
            <MetricCard metric={metric} loading={loading} />
          </Grid>
        ))}
      </Grid>

      {/* Active Campaigns and Progress */}
      <Grid container spacing={2}>
        <Grid item xs={12} md={6}>
          <Paper variant="outlined" sx={{ p: 2 }}>
            <Typography variant="subtitle1" fontWeight="medium" mb={1}>
              Active Campaigns
            </Typography>
            <ActiveCampaignList
              campaigns={activeCampaigns}
              loading={loading}
              onSelect={onCampaignSelect}
            />
          </Paper>
        </Grid>
        
        <Grid item xs={12} md={6}>
          <Paper variant="outlined" sx={{ p: 2 }}>
            <Typography variant="subtitle1" fontWeight="medium" mb={2}>
              Campaign Progress
            </Typography>
            {loading ? (
              <Box>
                <Skeleton variant="rectangular" height={24} sx={{ mb: 2 }} />
                <Skeleton variant="rectangular" height={24} sx={{ mb: 2 }} />
                <Skeleton variant="rectangular" height={24} />
              </Box>
            ) : activeCampaigns.length > 0 ? (
              activeCampaigns.slice(0, 5).map((campaign) => (
                <CampaignProgress key={campaign.id} campaign={campaign} />
              ))
            ) : (
              <Typography color="text.secondary" textAlign="center" py={4}>
                No active campaigns
              </Typography>
            )}
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default CampaignDashboardWidgets;

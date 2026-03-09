/**
 * STATUS: Implemented - Available for integration
 * PURPOSE: Embeds analytics dashboards from external providers (Apache Superset, Power BI)
 *   into the CRM UI via authenticated iframes. Supports dashboard selection, token-based auth,
 *   fullscreen mode, refresh, and entity-scoped filters (e.g., filter by account_id).
 * INTEGRATION: Can be used in DashboardPage, AccountDetailsPage, ReportsPage, or any page
 *   needing embedded analytics. Requires an active analytics provider configured via
 *   FeatureManagement.UseExternalAnalytics and Providers.Analytics settings.
 * CREATED: Phase 0 Session 23 (Pluggable Architecture - OpenRouter + Frontend Components)
 * DEPENDS ON: @mui/material, @mui/icons-material, apiClient (services/apiClient),
 *   Backend endpoints: GET /api/analytics/dashboards, GET /api/analytics/dashboards/{id}/embed,
 *   IAnalyticsPort (SupersetProvider or PowerBIProvider)
 */
import React, { useState, useEffect, useCallback } from 'react';
import {
  Box, Card, CardContent, CardHeader, Typography, CircularProgress,
  Alert, IconButton, Tooltip, Skeleton, Select, MenuItem, FormControl,
  InputLabel
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  OpenInNew as OpenInNewIcon,
  Fullscreen as FullscreenIcon,
  FullscreenExit as FullscreenExitIcon
} from '@mui/icons-material';
import apiClient from '../../services/apiClient';

/**
 * Analytics dashboard configuration from backend
 */
interface AnalyticsDashboard {
  id: string;
  name: string;
  description?: string;
  embedUrl?: string;
  thumbnailUrl?: string;
  tags?: string[];
}

/**
 * Embed response from the analytics provider
 */
interface AnalyticsEmbedResponse {
  embedUrl: string;
  token?: string;
  expiresAt?: string;
  dashboardId: string;
  provider: string;
}

interface AnalyticsEmbedProps {
  /**
   * Specific dashboard ID to embed. If not provided, shows dashboard selector.
   */
  dashboardId?: string;
  
  /**
   * Height of the embedded dashboard
   */
  height?: number | string;
  
  /**
   * Whether to show the card wrapper
   */
  showCard?: boolean;
  
  /**
   * Title override for the card
   */
  title?: string;
  
  /**
   * Entity filters to pass to the dashboard (e.g., { account_id: 123 })
   */
  filters?: Record<string, string | number>;
  
  /**
   * Callback when dashboard is loaded
   */
  onLoad?: () => void;
  
  /**
   * Callback on error
   */
  onError?: (error: string) => void;
}

/**
 * AnalyticsEmbed component for embedding analytics dashboards from Superset or Power BI.
 * Handles authentication, token refresh, and responsive iframe embedding.
 */
const AnalyticsEmbed: React.FC<AnalyticsEmbedProps> = ({
  dashboardId,
  height = 600,
  showCard = true,
  title,
  filters,
  onLoad,
  onError
}) => {
  const [dashboards, setDashboards] = useState<AnalyticsDashboard[]>([]);
  const [selectedDashboard, setSelectedDashboard] = useState<string>(dashboardId || '');
  const [embedData, setEmbedData] = useState<AnalyticsEmbedResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isFullscreen, setIsFullscreen] = useState(false);
  const iframeRef = React.useRef<HTMLIFrameElement>(null);

  // Fetch available dashboards
  const fetchDashboards = useCallback(async () => {
    try {
      const response = await apiClient.get('/analytics/dashboards');
      setDashboards(response.data || []);
      
      // Auto-select first dashboard if none specified
      if (!dashboardId && response.data?.length > 0) {
        setSelectedDashboard(response.data[0].id);
      }
    } catch (err: unknown) {
      console.error('Failed to fetch dashboards:', err);
      // Don't set error - dashboards endpoint might not exist
    }
  }, [dashboardId]);

  // Fetch embed URL for selected dashboard
  const fetchEmbedUrl = useCallback(async () => {
    if (!selectedDashboard) {
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      setError(null);

      // Build query params for filters
      const params = new URLSearchParams();
      if (filters) {
        Object.entries(filters).forEach(([key, value]) => {
          params.append(`filter_${key}`, String(value));
        });
      }

      const response = await apiClient.get(
        `/analytics/dashboards/${selectedDashboard}/embed?${params.toString()}`
      );
      
      setEmbedData(response.data);
      onLoad?.();
    } catch (err: unknown) {
      const errorMessage = (err as any).response?.data?.message || 'Failed to load analytics dashboard';
      setError(errorMessage);
      onError?.(errorMessage);
    } finally {
      setLoading(false);
    }
  }, [selectedDashboard, filters, onLoad, onError]);

  // Initial load
  useEffect(() => {
    if (!dashboardId) {
      fetchDashboards();
    }
  }, [dashboardId, fetchDashboards]);

  // Fetch embed when dashboard changes
  useEffect(() => {
    if (selectedDashboard) {
      fetchEmbedUrl();
    }
  }, [selectedDashboard, fetchEmbedUrl]);

  // Handle fullscreen toggle
  const toggleFullscreen = () => {
    if (!iframeRef.current) return;

    if (!isFullscreen) {
      if (iframeRef.current.requestFullscreen) {
        iframeRef.current.requestFullscreen();
      }
    } else {
      if (document.exitFullscreen) {
        document.exitFullscreen();
      }
    }
    setIsFullscreen(!isFullscreen);
  };

  // Handle refresh
  const handleRefresh = () => {
    fetchEmbedUrl();
  };

  // Open in new tab
  const openInNewTab = () => {
    if (embedData?.embedUrl) {
      window.open(embedData.embedUrl, '_blank');
    }
  };

  // Render content
  const renderContent = () => {
    if (loading) {
      return (
        <Box sx={{ width: '100%', height: typeof height === 'number' ? height : 400 }}>
          <Skeleton variant="rectangular" width="100%" height="100%" />
          <Box sx={{ 
            position: 'absolute', 
            top: '50%', 
            left: '50%', 
            transform: 'translate(-50%, -50%)',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            gap: 2
          }}>
            <CircularProgress />
            <Typography color="text.secondary">Loading dashboard...</Typography>
          </Box>
        </Box>
      );
    }

    if (error) {
      return (
        <Alert 
          severity="error" 
          action={
            <IconButton size="small" onClick={handleRefresh}>
              <RefreshIcon />
            </IconButton>
          }
        >
          {error}
        </Alert>
      );
    }

    if (!embedData?.embedUrl) {
      return (
        <Alert severity="info">
          No analytics dashboard available. Configure an analytics provider in settings.
        </Alert>
      );
    }

    return (
      <Box sx={{ position: 'relative', width: '100%', height }}>
        <iframe
          ref={iframeRef}
          src={embedData.embedUrl}
          width="100%"
          height="100%"
          frameBorder="0"
          style={{ border: 'none', borderRadius: 4 }}
          title={title || 'Analytics Dashboard'}
          allow="fullscreen"
          sandbox="allow-same-origin allow-scripts allow-popups allow-forms"
        />
      </Box>
    );
  };

  // Render with or without card wrapper
  if (!showCard) {
    return renderContent();
  }

  return (
    <Card>
      <CardHeader
        title={title || 'Analytics Dashboard'}
        subheader={embedData?.provider ? `Powered by ${embedData.provider}` : undefined}
        action={
          <Box sx={{ display: 'flex', gap: 0.5 }}>
            {/* Dashboard selector if multiple available */}
            {!dashboardId && dashboards.length > 1 && (
              <FormControl size="small" sx={{ minWidth: 150, mr: 1 }}>
                <InputLabel>Dashboard</InputLabel>
                <Select
                  value={selectedDashboard}
                  label="Dashboard"
                  onChange={(e) => setSelectedDashboard(e.target.value)}
                >
                  {dashboards.map((d) => (
                    <MenuItem key={d.id} value={d.id}>{d.name}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            )}
            
            <Tooltip title="Refresh">
              <IconButton onClick={handleRefresh} disabled={loading}>
                <RefreshIcon />
              </IconButton>
            </Tooltip>
            
            <Tooltip title="Open in new tab">
              <IconButton onClick={openInNewTab} disabled={!embedData?.embedUrl}>
                <OpenInNewIcon />
              </IconButton>
            </Tooltip>
            
            <Tooltip title={isFullscreen ? 'Exit fullscreen' : 'Fullscreen'}>
              <IconButton onClick={toggleFullscreen} disabled={!embedData?.embedUrl}>
                {isFullscreen ? <FullscreenExitIcon /> : <FullscreenIcon />}
              </IconButton>
            </Tooltip>
          </Box>
        }
      />
      <CardContent sx={{ p: 0, '&:last-child': { pb: 0 } }}>
        {renderContent()}
      </CardContent>
    </Card>
  );
};

export default AnalyticsEmbed;

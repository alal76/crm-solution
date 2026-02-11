import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  Button,
  Chip,
  Tabs,
  Tab,
  CircularProgress,
  Alert,
} from '@mui/material';
import {
  Analytics as AnalyticsIcon,
  TrendingUp as TrendingUpIcon,
  People as PeopleIcon,
  AttachMoney as MoneyIcon,
  Assessment as ReportIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';

// Import the AnalyticsEmbed component if available
// import AnalyticsEmbed from '../components/common/AnalyticsEmbed';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`analytics-tabpanel-${index}`}
      aria-labelledby={`analytics-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ py: 3 }}>{children}</Box>}
    </div>
  );
}

interface MetricCardProps {
  title: string;
  value: string | number;
  change?: string;
  icon: React.ReactNode;
  color: string;
}

const MetricCard: React.FC<MetricCardProps> = ({ title, value, change, icon, color }) => (
  <Card>
    <CardContent>
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
        <Box sx={{ color, mr: 1 }}>{icon}</Box>
        <Typography variant="subtitle2" color="text.secondary">
          {title}
        </Typography>
      </Box>
      <Typography variant="h4" component="div">
        {value}
      </Typography>
      {change && (
        <Chip
          label={change}
          size="small"
          color={change.startsWith('+') ? 'success' : 'error'}
          sx={{ mt: 1 }}
        />
      )}
    </CardContent>
  </Card>
);

/**
 * AnalyticsPage - Main analytics dashboard page
 * 
 * Provides access to CRM analytics including:
 * - AI-powered lead scoring
 * - Opportunity insights
 * - Sales performance metrics
 * - Pipeline analytics
 */
const AnalyticsPage: React.FC = () => {
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState(0);
  const [loading, setLoading] = useState(true);
  const [metrics, setMetrics] = useState<any>(null);

  useEffect(() => {
    // Simulate loading analytics data
    const loadMetrics = async () => {
      setLoading(true);
      try {
        // In production, this would fetch from /api/ai-analytics/dashboard
        // const response = await fetch('/api/ai-analytics/dashboard');
        // const data = await response.json();
        
        // Simulated data for now
        setMetrics({
          totalLeads: 1247,
          leadChange: '+12%',
          avgLeadScore: 72,
          scoreChange: '+5%',
          pipelineValue: '$2.4M',
          pipelineChange: '+18%',
          winRate: '34%',
          winChange: '+3%',
        });
      } catch (error) {
        console.error('Failed to load analytics:', error);
      } finally {
        setLoading(false);
      }
    };

    loadMetrics();
  }, []);

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue);
  };

  const handleRefresh = () => {
    setLoading(true);
    // Re-fetch data
    setTimeout(() => setLoading(false), 1000);
  };

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <AnalyticsIcon sx={{ fontSize: 32, mr: 2, color: 'primary.main' }} />
          <Typography variant="h4" component="h1">
            Analytics
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={handleRefresh}
            disabled={loading}
          >
            Refresh
          </Button>
          <Button
            variant="contained"
            startIcon={<ReportIcon />}
            onClick={() => navigate('/reports')}
          >
            View Reports
          </Button>
        </Box>
      </Box>

      {/* Loading State */}
      {loading && (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      )}

      {/* Metrics Grid */}
      {!loading && metrics && (
        <>
          <Grid container spacing={3} sx={{ mb: 3 }}>
            <Grid item xs={12} sm={6} md={3}>
              <MetricCard
                title="Total Leads"
                value={metrics.totalLeads.toLocaleString()}
                change={metrics.leadChange}
                icon={<PeopleIcon />}
                color="#1976d2"
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <MetricCard
                title="Avg Lead Score"
                value={metrics.avgLeadScore}
                change={metrics.scoreChange}
                icon={<TrendingUpIcon />}
                color="#2e7d32"
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <MetricCard
                title="Pipeline Value"
                value={metrics.pipelineValue}
                change={metrics.pipelineChange}
                icon={<MoneyIcon />}
                color="#ed6c02"
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <MetricCard
                title="Win Rate"
                value={metrics.winRate}
                change={metrics.winChange}
                icon={<AnalyticsIcon />}
                color="#9c27b0"
              />
            </Grid>
          </Grid>

          {/* Tabs for different analytics views */}
          <Card>
            <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
              <Tabs value={activeTab} onChange={handleTabChange} aria-label="analytics tabs">
                <Tab label="Overview" id="analytics-tab-0" aria-controls="analytics-tabpanel-0" />
                <Tab label="Lead Scoring" id="analytics-tab-1" aria-controls="analytics-tabpanel-1" />
                <Tab label="Pipeline" id="analytics-tab-2" aria-controls="analytics-tabpanel-2" />
                <Tab label="AI Insights" id="analytics-tab-3" aria-controls="analytics-tabpanel-3" />
              </Tabs>
            </Box>

            <TabPanel value={activeTab} index={0}>
              <CardContent>
                <Alert severity="info" sx={{ mb: 2 }}>
                  The Analytics Overview provides a high-level summary of CRM performance. 
                  Use the tabs above to drill into specific analytics areas.
                </Alert>
                <Typography variant="h6" gutterBottom>
                  Quick Stats
                </Typography>
                <Grid container spacing={2}>
                  <Grid item xs={12} md={4}>
                    <Typography variant="body2" color="text.secondary">
                      Active Opportunities: <strong>156</strong>
                    </Typography>
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <Typography variant="body2" color="text.secondary">
                      Deals Won This Month: <strong>23</strong>
                    </Typography>
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <Typography variant="body2" color="text.secondary">
                      Average Deal Size: <strong>$45,000</strong>
                    </Typography>
                  </Grid>
                </Grid>
              </CardContent>
            </TabPanel>

            <TabPanel value={activeTab} index={1}>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  AI-Powered Lead Scoring
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                  Leads are automatically scored based on engagement, demographics, 
                  behavior patterns, and historical conversion data.
                </Typography>
                <Grid container spacing={2}>
                  <Grid item xs={12} md={4}>
                    <Card variant="outlined">
                      <CardContent>
                        <Typography variant="subtitle1" color="success.main">Hot Leads (80+)</Typography>
                        <Typography variant="h4">47</Typography>
                      </CardContent>
                    </Card>
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <Card variant="outlined">
                      <CardContent>
                        <Typography variant="subtitle1" color="warning.main">Warm Leads (50-79)</Typography>
                        <Typography variant="h4">312</Typography>
                      </CardContent>
                    </Card>
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <Card variant="outlined">
                      <CardContent>
                        <Typography variant="subtitle1" color="text.secondary">Cold Leads (&lt;50)</Typography>
                        <Typography variant="h4">888</Typography>
                      </CardContent>
                    </Card>
                  </Grid>
                </Grid>
              </CardContent>
            </TabPanel>

            <TabPanel value={activeTab} index={2}>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Pipeline Analytics
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                  Track deal progression through your sales pipeline stages.
                </Typography>
                <Alert severity="info">
                  Pipeline visualization coming soon. View detailed reports in the 
                  <Button size="small" onClick={() => navigate('/reports')}>Reports</Button> section.
                </Alert>
              </CardContent>
            </TabPanel>

            <TabPanel value={activeTab} index={3}>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  AI Insights
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                  Machine learning-powered insights and recommendations.
                </Typography>
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <Alert severity="success" sx={{ mb: 1 }}>
                      <strong>Opportunity Alert:</strong> 3 high-value opportunities are approaching their expected close date.
                    </Alert>
                  </Grid>
                  <Grid item xs={12}>
                    <Alert severity="warning" sx={{ mb: 1 }}>
                      <strong>Churn Risk:</strong> 5 accounts show declining engagement patterns.
                    </Alert>
                  </Grid>
                  <Grid item xs={12}>
                    <Alert severity="info">
                      <strong>Recommendation:</strong> Focus on leads from the Technology sector - they show 2x higher conversion rates.
                    </Alert>
                  </Grid>
                </Grid>
              </CardContent>
            </TabPanel>
          </Card>
        </>
      )}
    </Box>
  );
};

export default AnalyticsPage;

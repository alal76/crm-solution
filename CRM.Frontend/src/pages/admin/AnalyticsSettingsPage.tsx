import React from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  Button,
  Chip,
  Link,
  Alert,
} from '@mui/material';
import {
  Analytics as AnalyticsIcon,
  OpenInNew as ExternalLinkIcon,
  CheckCircle as ActiveIcon,
  Settings as ConfigIcon,
  Dashboard as DashboardIcon,
} from '@mui/icons-material';

/**
 * AnalyticsSettingsPage - Manage analytics platforms like Superset, Power BI, Metabase
 * 
 * This page provides access to external analytics/BI platforms configured
 * for the CRM solution.
 */
const AnalyticsSettingsPage: React.FC = () => {
  // Use dynamic hostname so links work on any deployment
  const hostname = window.location.hostname;
  const supersetUrl = process.env.REACT_APP_SUPERSET_URL || `http://${hostname}:8088`; // NOSONAR - S5332 - http:// URL constructed from runtime hostname for local dev analytics access
  const supersetCrmDashboard = `${supersetUrl}/superset/dashboard/crm-overview/`;
  const supersetSqlLab = `${supersetUrl}/sqllab/`;
  const metabaseUrl = process.env.REACT_APP_METABASE_URL || `http://${hostname}:3000`; // NOSONAR - S5332 - http:// URL constructed from runtime hostname for local dev analytics access
  
  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
        <AnalyticsIcon sx={{ fontSize: 32, mr: 2, color: 'primary.main' }} />
        <Typography variant="h4" component="h1">
          Analytics & BI Platforms
        </Typography>
      </Box>
      
      <Alert severity="info" sx={{ mb: 3 }}>
        Configure and access external Business Intelligence (BI) and analytics platforms. 
        These tools provide advanced reporting, dashboards, and data visualization capabilities.
      </Alert>

      <Grid container spacing={3}>
        {/* Apache Superset */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <DashboardIcon sx={{ fontSize: 32, mr: 2, color: '#20a7c9' }} />
                <Typography variant="h6" sx={{ flexGrow: 1 }}>
                  Apache Superset
                </Typography>
                <Chip
                  icon={<ActiveIcon />}
                  label="Recommended"
                  color="success"
                  size="small"
                />
              </Box>
              
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Apache Superset is an open-source data visualization and business 
                intelligence platform. Create interactive dashboards, explore data, 
                and share insights. A CRM Overview dashboard with 6 charts and 10 datasets 
                is pre-configured.
              </Typography>

              <Alert severity="success" sx={{ mb: 2 }}>
                <Typography variant="body2">
                  <strong>CRM Dashboard:</strong> Pre-configured with Accounts, Leads, Opportunities, and Service Request charts.
                  <br />
                  <strong>Credentials:</strong> See your deployment environment configuration for Superset credentials.
                </Typography>
              </Alert>
              
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                <Button
                  variant="contained"
                  startIcon={<DashboardIcon />}
                  component={Link}
                  href={supersetCrmDashboard}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  CRM Dashboard
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<ExternalLinkIcon />}
                  component={Link}
                  href={supersetSqlLab}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  SQL Lab
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<ExternalLinkIcon />}
                  component={Link}
                  href={supersetUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  Open Superset
                </Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Metabase */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <Box
                  component="img"
                  src="https://www.metabase.com/images/favicon.ico"
                  alt="Metabase"
                  sx={{ width: 32, height: 32, mr: 2 }}
                  onError={(e: React.SyntheticEvent<HTMLImageElement>) => {
                    e.currentTarget.style.display = 'none';
                  }}
                />
                <Typography variant="h6" sx={{ flexGrow: 1 }}>
                  Metabase
                </Typography>
                <Chip
                  label="Alternative"
                  color="default"
                  size="small"
                />
              </Box>
              
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Metabase is an open-source business intelligence tool that makes 
                it easy for anyone to ask questions and learn from data.
              </Typography>
              
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                <Button
                  variant="outlined"
                  startIcon={<ExternalLinkIcon />}
                  component={Link}
                  href={metabaseUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  Open Metabase
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<ConfigIcon />}
                  disabled
                >
                  Configure
                </Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Power BI */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <Box
                  component="img"
                  src="https://app.powerbi.com/favicon.ico"
                  alt="Power BI"
                  sx={{ width: 32, height: 32, mr: 2 }}
                  onError={(e: React.SyntheticEvent<HTMLImageElement>) => {
                    e.currentTarget.style.display = 'none';
                  }}
                />
                <Typography variant="h6" sx={{ flexGrow: 1 }}>
                  Microsoft Power BI
                </Typography>
                <Chip
                  label="Enterprise"
                  color="primary"
                  size="small"
                />
              </Box>
              
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Power BI is Microsoft's enterprise business analytics service. 
                Requires Azure AD integration for embedded dashboards.
              </Typography>
              
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                <Button
                  variant="outlined"
                  startIcon={<ExternalLinkIcon />}
                  component={Link}
                  href="https://app.powerbi.com"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  Open Power BI
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<ConfigIcon />}
                  disabled
                >
                  Configure Azure AD
                </Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Built-in Analytics */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <AnalyticsIcon sx={{ fontSize: 32, mr: 2, color: 'primary.main' }} />
                <Typography variant="h6" sx={{ flexGrow: 1 }}>
                  Built-in AI Analytics
                </Typography>
                <Chip
                  icon={<ActiveIcon />}
                  label="Active"
                  color="success"
                  size="small"
                />
              </Box>
              
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                The CRM includes built-in AI-powered analytics for lead scoring, 
                opportunity insights, and knowledge base search.
              </Typography>
              
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                <Button
                  variant="contained"
                  startIcon={<DashboardIcon />}
                  component={Link}
                  href="/reports"
                >
                  View Reports
                </Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Available Data Sources */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2 }}>
                Available CRM Data Sources
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                The following CRM data is available for analytics platforms:
              </Typography>
              
              <Grid container spacing={1}>
                {[
                  'Accounts', 'Contacts', 'Leads', 'Opportunities',
                  'Products', 'Quotes', 'Orders', 'Invoices', 'Payments',
                  'Service Requests', 'Activities', 'Campaigns',
                  'Users', 'Teams', 'Commissions'
                ].map((source) => (
                  <Grid item key={source}>
                    <Chip label={source} size="small" variant="outlined" />
                  </Grid>
                ))}
              </Grid>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default AnalyticsSettingsPage;

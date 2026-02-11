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
  Hub as IntegrationsIcon,
  OpenInNew as ExternalLinkIcon,
  CheckCircle as ActiveIcon,
  Settings as ConfigIcon,
} from '@mui/icons-material';

/**
 * IntegrationsSettingsPage - Manage integration platforms like n8n, Zapier, etc.
 * 
 * This page provides access to external integration platforms that connect
 * the CRM with other business applications.
 */
const IntegrationsSettingsPage: React.FC = () => {
  // In production, these URLs would come from backend configuration
  const n8nUrl = process.env.REACT_APP_N8N_URL || 'http://localhost:5678';
  const zapierUrl = 'https://zapier.com/app/dashboard';
  
  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
        <IntegrationsIcon sx={{ fontSize: 32, mr: 2, color: 'primary.main' }} />
        <Typography variant="h4" component="h1">
          Integrations
        </Typography>
      </Box>
      
      <Alert severity="info" sx={{ mb: 3 }}>
        Manage connections to external automation and integration platforms. 
        These integrations allow the CRM to communicate with other business applications.
      </Alert>

      <Grid container spacing={3}>
        {/* n8n Integration */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <Box
                  component="img"
                  src="https://n8n.io/favicon.ico"
                  alt="n8n"
                  sx={{ width: 32, height: 32, mr: 2 }}
                  onError={(e: React.SyntheticEvent<HTMLImageElement>) => {
                    e.currentTarget.style.display = 'none';
                  }}
                />
                <Typography variant="h6" sx={{ flexGrow: 1 }}>
                  n8n (Self-Hosted)
                </Typography>
                <Chip
                  icon={<ActiveIcon />}
                  label="Available"
                  color="success"
                  size="small"
                />
              </Box>
              
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                n8n is an open-source workflow automation platform. Create workflows 
                to automate tasks between the CRM and hundreds of other applications.
              </Typography>
              
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button
                  variant="contained"
                  startIcon={<ExternalLinkIcon />}
                  component={Link}
                  href={n8nUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  Open n8n
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

        {/* Zapier Integration */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <Box
                  component="img"
                  src="https://zapier.com/favicon.ico"
                  alt="Zapier"
                  sx={{ width: 32, height: 32, mr: 2 }}
                  onError={(e: React.SyntheticEvent<HTMLImageElement>) => {
                    e.currentTarget.style.display = 'none';
                  }}
                />
                <Typography variant="h6" sx={{ flexGrow: 1 }}>
                  Zapier (Cloud)
                </Typography>
                <Chip
                  label="Optional"
                  color="default"
                  size="small"
                />
              </Box>
              
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Zapier is a cloud-based automation platform. Connect the CRM 
                via webhooks to thousands of apps without writing code.
              </Typography>
              
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button
                  variant="outlined"
                  startIcon={<ExternalLinkIcon />}
                  component={Link}
                  href={zapierUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  Open Zapier
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<ConfigIcon />}
                  disabled
                >
                  Configure Webhooks
                </Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* CRM Webhooks */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2 }}>
                CRM Webhook Events
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                The CRM can send webhook notifications to n8n, Zapier, or any 
                other system when certain events occur:
              </Typography>
              
              <Grid container spacing={1}>
                {[
                  'account.created', 'account.updated', 'account.deleted',
                  'contact.created', 'contact.updated', 
                  'opportunity.created', 'opportunity.stage_changed', 'opportunity.won', 'opportunity.lost',
                  'lead.created', 'lead.converted',
                  'invoice.created', 'invoice.paid',
                  'service_request.created', 'service_request.resolved'
                ].map((event) => (
                  <Grid item key={event}>
                    <Chip label={event} size="small" variant="outlined" />
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

export default IntegrationsSettingsPage;

import React, { useEffect, useState } from 'react';
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
  Divider,
} from '@mui/material';
import {
  Hub as IntegrationsIcon,
  OpenInNew as ExternalLinkIcon,
  Settings as ConfigIcon,
} from '@mui/icons-material';
import { providerHealthService, ProviderHealthReport, HealthProviderCategory } from '../../services/providerHealthService';

/**
 * IntegrationsSettingsPage - Manage integration platforms and connected services.
 *
 * UX-CONF-007: Expanded with:
 *   - Business Apps: Meilisearch, Ollama, Chatwoot, Novu, DocuSeal, Apache Superset
 *   - Stubbed cards: QuickBooks, Mailchimp, Calendly, LinkedIn
 *
 * REV-FE-006: Status chips reflect real provider health from GET /api/health/providers
 * instead of static placeholder labels.
 * Provider connection status is managed in Admin > Providers (ProvidersPage).
 * This page provides quick-access launch links and status summaries.
 */
const IntegrationsSettingsPage: React.FC = () => {
  const [healthReport, setHealthReport] = useState<ProviderHealthReport | null>(null);
  const [healthLoading, setHealthLoading] = useState(true);
  const [healthError, setHealthError] = useState(false);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const report = await providerHealthService.getProviderHealth();
        if (!cancelled) {
          setHealthReport(report);
        }
      } catch {
        if (!cancelled) {
          setHealthError(true);
        }
      } finally {
        if (!cancelled) {
          setHealthLoading(false);
        }
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const providerStatusLabel = (category: HealthProviderCategory, providerName: string): string => {
    if (healthLoading) return 'Checking…';
    if (healthError || !healthReport) return 'Status unavailable';
    const status = healthReport.providers[category];
    if (!status) return 'Status unavailable';
    if (status.activeProvider?.toLowerCase() !== providerName.toLowerCase()) return 'Not active';
    return status.isHealthy ? 'Healthy' : 'Unreachable';
  };

  const ProviderStatusChip: React.FC<{ category: HealthProviderCategory; providerName: string }> = ({ category, providerName }) => {
    const label = providerStatusLabel(category, providerName);
    const color: 'default' | 'success' | 'error' | 'warning' =
      label === 'Healthy' ? 'success' : label === 'Unreachable' ? 'error' : label === 'Checking…' ? 'default' : 'warning';
    return <Chip label={label} color={color} size="small" />;
  };

  const hostname = window.location.hostname;
  // NOSONAR - S5332 - http:// URLs constructed from runtime hostname for local dev integration access
  const n8nUrl = process.env.REACT_APP_N8N_URL || `http://${hostname}:5678`;
  const meilisearchUrl = process.env.REACT_APP_MEILISEARCH_URL || `http://${hostname}:7700`;
  const ollamaUrl = process.env.REACT_APP_OLLAMA_URL || `http://${hostname}:11434`;
  const chatwootUrl = process.env.REACT_APP_CHATWOOT_URL || `http://${hostname}:3000`;
  const novuUrl = process.env.REACT_APP_NOVU_URL || `http://${hostname}:3001`;
  const docusealUrl = process.env.REACT_APP_DOCUSEAL_URL || `http://${hostname}:3002`;
  const supersetUrl = process.env.REACT_APP_SUPERSET_URL || `http://${hostname}:8088`;
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
        Manage connections to external platforms. Provider credentials and feature flags are
        configured in{' '}
        <Link href="/admin/providers" underline="hover">Admin › Providers</Link>.
      </Alert>

      {/* ── Automation ─────────────────────────────────────────────────────── */}
      <Typography variant="h6" sx={{ mb: 2, mt: 1 }}>Automation</Typography>
      <Grid container spacing={3} sx={{ mb: 4 }}>
        {/* n8n */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <Box
                  component="img"
                  src="https://n8n.io/favicon.ico"
                  alt="n8n"
                  sx={{ width: 32, height: 32, mr: 2 }}
                  onError={(e: React.SyntheticEvent<HTMLImageElement>) => { e.currentTarget.style.display = 'none'; }}
                />
                <Typography variant="h6" sx={{ flexGrow: 1 }}>n8n (Self-Hosted)</Typography>
                <ProviderStatusChip category="Integrations" providerName="n8n" />
              </Box>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Open-source workflow automation. Connect the CRM to hundreds of apps with visual workflows.
              </Typography>
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button variant="contained" startIcon={<ExternalLinkIcon />} component={Link} href={n8nUrl} target="_blank" rel="noopener noreferrer">Open n8n</Button>
                <Button variant="outlined" startIcon={<ConfigIcon />} href="/admin/providers" component={Link}>Configure</Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Zapier */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <Box
                  component="img"
                  src="https://zapier.com/favicon.ico"
                  alt="Zapier"
                  sx={{ width: 32, height: 32, mr: 2 }}
                  onError={(e: React.SyntheticEvent<HTMLImageElement>) => { e.currentTarget.style.display = 'none'; }}
                />
                <Typography variant="h6" sx={{ flexGrow: 1 }}>Zapier (Cloud)</Typography>
                <ProviderStatusChip category="Integrations" providerName="Zapier" />
              </Box>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Cloud automation platform. Connect the CRM via webhooks to thousands of apps.
              </Typography>
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button variant="outlined" startIcon={<ExternalLinkIcon />} component={Link} href={zapierUrl} target="_blank" rel="noopener noreferrer">Open Zapier</Button>
                <Button variant="outlined" startIcon={<ConfigIcon />} disabled>Configure Webhooks</Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <Divider sx={{ mb: 3 }} />

      {/* ── Business Apps (Self-Hosted Providers) ──────────────────────────── */}
      {/* UX-CONF-007: Added provider quick-links section */}
      <Typography variant="h6" sx={{ mb: 2 }}>Business Apps</Typography>
      <Grid container spacing={3} sx={{ mb: 4 }}>
        {/* Meilisearch */}
        <Grid item xs={12} sm={6} md={4}>
          <Card>
            <CardContent>
              <Typography variant="subtitle1" fontWeight={600} gutterBottom>🔍 Meilisearch</Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Full-text search engine. Provides instant, typo-tolerant search across all CRM records.
              </Typography>
              <Box sx={{ mb: 2 }}>
                <Chip label="Search Provider" size="small" color="primary" variant="outlined" sx={{ mr: 1 }} />
                <ProviderStatusChip category="Search" providerName="Meilisearch" />
              </Box>
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button variant="outlined" size="small" startIcon={<ExternalLinkIcon />} component={Link} href={meilisearchUrl} target="_blank" rel="noopener noreferrer">Open</Button>
                <Button variant="outlined" size="small" href="/admin/providers" component={Link}>Configure</Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Ollama */}
        <Grid item xs={12} sm={6} md={4}>
          <Card>
            <CardContent>
              <Typography variant="subtitle1" fontWeight={600} gutterBottom>🤖 Ollama</Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Local LLM inference engine. Run AI models (Llama, Mistral, CodeLlama) privately on your infrastructure.
              </Typography>
              <Box sx={{ mb: 2 }}>
                <Chip label="AI Provider" size="small" color="secondary" variant="outlined" sx={{ mr: 1 }} />
                <ProviderStatusChip category="AI" providerName="Ollama" />
              </Box>
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button variant="outlined" size="small" startIcon={<ExternalLinkIcon />} component={Link} href={ollamaUrl} target="_blank" rel="noopener noreferrer">Open</Button>
                <Button variant="outlined" size="small" href="/admin/providers" component={Link}>Configure</Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Chatwoot */}
        <Grid item xs={12} sm={6} md={4}>
          <Card>
            <CardContent>
              <Typography variant="subtitle1" fontWeight={600} gutterBottom>💬 Chatwoot</Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Customer messaging platform. Live chat, email, social inbox in one place.
              </Typography>
              <Box sx={{ mb: 2 }}>
                <Chip label="Chat Provider" size="small" color="info" variant="outlined" sx={{ mr: 1 }} />
                <ProviderStatusChip category="Chat" providerName="Chatwoot" />
              </Box>
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button variant="outlined" size="small" startIcon={<ExternalLinkIcon />} component={Link} href={chatwootUrl} target="_blank" rel="noopener noreferrer">Open</Button>
                <Button variant="outlined" size="small" href="/admin/providers" component={Link}>Configure</Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Novu */}
        <Grid item xs={12} sm={6} md={4}>
          <Card>
            <CardContent>
              <Typography variant="subtitle1" fontWeight={600} gutterBottom>🔔 Novu</Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Multi-channel notification infrastructure. Send in-app, email, SMS, and push notifications.
              </Typography>
              <Box sx={{ mb: 2 }}>
                <Chip label="Notifications Provider" size="small" color="warning" variant="outlined" sx={{ mr: 1 }} />
                <ProviderStatusChip category="Notifications" providerName="Novu" />
              </Box>
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button variant="outlined" size="small" startIcon={<ExternalLinkIcon />} component={Link} href={novuUrl} target="_blank" rel="noopener noreferrer">Open</Button>
                <Button variant="outlined" size="small" href="/admin/providers" component={Link}>Configure</Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* DocuSeal */}
        <Grid item xs={12} sm={6} md={4}>
          <Card>
            <CardContent>
              <Typography variant="subtitle1" fontWeight={600} gutterBottom>✍️ DocuSeal</Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                E-signature workflows. Send contracts and documents for digital signatures.
              </Typography>
              <Box sx={{ mb: 2 }}>
                <Chip label="Signatures Provider" size="small" color="success" variant="outlined" sx={{ mr: 1 }} />
                <ProviderStatusChip category="Signatures" providerName="DocuSeal" />
              </Box>
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button variant="outlined" size="small" startIcon={<ExternalLinkIcon />} component={Link} href={docusealUrl} target="_blank" rel="noopener noreferrer">Open</Button>
                <Button variant="outlined" size="small" href="/admin/providers" component={Link}>Configure</Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Apache Superset */}
        <Grid item xs={12} sm={6} md={4}>
          <Card>
            <CardContent>
              <Typography variant="subtitle1" fontWeight={600} gutterBottom>📊 Apache Superset</Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Business intelligence & data visualization. Interactive dashboards and SQL Lab.
              </Typography>
              <Box sx={{ mb: 2 }}>
                <Chip label="Analytics Provider" size="small" color="primary" variant="outlined" sx={{ mr: 1 }} />
                <ProviderStatusChip category="Analytics" providerName="Superset" />
              </Box>
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button variant="outlined" size="small" startIcon={<ExternalLinkIcon />} component={Link} href={supersetUrl} target="_blank" rel="noopener noreferrer">Open</Button>
                <Button variant="outlined" size="small" href="/admin/analytics" component={Link}>Analytics Settings</Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <Divider sx={{ mb: 3 }} />

      {/* ── Stubbed: Coming Soon ────────────────────────────────────────────── */}
      {/* UX-CONF-007: Stubbed cards — TODO-UX-CONF-014: implement when connectors are built */}
      <Typography variant="h6" sx={{ mb: 2 }}>Coming Soon</Typography>
      <Grid container spacing={3} sx={{ mb: 4 }}>
        {[
          { name: 'QuickBooks', icon: '💼', desc: 'Sync invoices and payments with QuickBooks accounting.' },
          { name: 'Mailchimp', icon: '📧', desc: 'Sync contacts and campaign lists with Mailchimp.' },
          { name: 'Calendly', icon: '📅', desc: 'Embed scheduling links and sync meetings from Calendly.' },
          { name: 'LinkedIn', icon: '🔗', desc: 'Enrich contact profiles with LinkedIn data.' },
        ].map(({ name, icon, desc }) => (
          <Grid item xs={12} sm={6} md={3} key={name}>
            <Card sx={{ opacity: 0.7, border: '1px dashed', borderColor: 'divider' }}>
              <CardContent>
                <Typography variant="subtitle1" fontWeight={600} gutterBottom>{icon} {name}</Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>{desc}</Typography>
                <Chip label="Coming Soon" size="small" variant="outlined" />
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      {/* ── Webhook events reference ────────────────────────────────────────── */}
      <Divider sx={{ mb: 3 }} />
      <Card>
        <CardContent>
          <Typography variant="h6" sx={{ mb: 2 }}>CRM Webhook Events</Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            The CRM can send webhook notifications to n8n, Zapier, or any other system when these events occur:
          </Typography>
          <Grid container spacing={1}>
            {[
              'account.created', 'account.updated', 'account.deleted',
              'contact.created', 'contact.updated',
              'opportunity.created', 'opportunity.stage_changed', 'opportunity.won', 'opportunity.lost',
              'lead.created', 'lead.converted',
              'invoice.created', 'invoice.paid',
              'service_request.created', 'service_request.resolved',
            ].map((event) => (
              <Grid item key={event}>
                <Chip label={event} size="small" variant="outlined" />
              </Grid>
            ))}
          </Grid>
        </CardContent>
      </Card>
    </Box>
  );
};

export default IntegrationsSettingsPage;

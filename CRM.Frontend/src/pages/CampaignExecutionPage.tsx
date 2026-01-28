/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Campaign Execution Page - Workflow Integration, A/B Testing, Analytics
 */

import { useState, useEffect, useCallback } from 'react';
import {
  Box, Card, CardContent, Typography, Button, Table, TableBody, TableCell, TableHead,
  TableRow, Dialog, DialogTitle, DialogContent, DialogActions, Alert, CircularProgress,
  TextField, Container, FormControl, InputLabel, Select, MenuItem, Chip, Tabs, Tab,
  Grid, IconButton, Tooltip, LinearProgress, Paper, Stack, Divider
} from '@mui/material';
import {
  Add as AddIcon, PlayArrow as PlayIcon, Pause as PauseIcon,
  Analytics as AnalyticsIcon, Science as ABTestIcon,
  People as RecipientsIcon, Link as LinkIcon,
  CheckCircle as ConversionIcon, Close as CloseIcon,
  Refresh as RefreshIcon, Email as EmailIcon
} from '@mui/icons-material';
import { useParams, useNavigate } from 'react-router-dom';
import apiClient from '../services/apiClient';
import campaignExecutionService, {
  CampaignWorkflow, CampaignRecipient, CampaignABTest, CampaignConversion,
  CampaignAnalytics, CampaignWorkflowType, RecipientStatus, ABTestType, ABTestStatus,
  ConversionType, AttributionModel, LinkCampaignWorkflowRequest, CreateABTestRequest
} from '../services/campaignExecutionService';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div role="tabpanel" hidden={value !== index} {...other}>
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  );
}

interface Campaign {
  id: number;
  name: string;
  campaignType: number;
  status: number;
  startDate: string;
  endDate: string;
  budget: number;
}

interface WorkflowDefinition {
  id: number;
  name: string;
  description?: string;
  status: string;
}

function CampaignExecutionPage() {
  const { campaignId: campaignIdParam } = useParams<{ campaignId: string }>();
  const navigate = useNavigate();
  const campaignId = campaignIdParam ? parseInt(campaignIdParam) : 0;

  const [tabValue, setTabValue] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Data states
  const [campaign, setCampaign] = useState<Campaign | null>(null);
  const [workflows, setWorkflows] = useState<CampaignWorkflow[]>([]);
  const [recipients, setRecipients] = useState<CampaignRecipient[]>([]);
  const [abTests, setAbTests] = useState<CampaignABTest[]>([]);
  const [conversions, setConversions] = useState<CampaignConversion[]>([]);
  const [analytics, setAnalytics] = useState<CampaignAnalytics | null>(null);
  const [availableWorkflows, setAvailableWorkflows] = useState<WorkflowDefinition[]>([]);

  // Dialog states
  const [workflowDialogOpen, setWorkflowDialogOpen] = useState(false);
  const [abTestDialogOpen, setAbTestDialogOpen] = useState(false);
  const [addRecipientsDialogOpen, setAddRecipientsDialogOpen] = useState(false);

  // Form states
  const emptyWorkflowForm: LinkCampaignWorkflowRequest = {
    campaignId: campaignId,
    workflowDefinitionId: 0,
    workflowType: CampaignWorkflowType.Execution,
    triggerEvent: 'CampaignStarted',
    priority: 1
  };

  const emptyABTestForm: CreateABTestRequest = {
    campaignId: campaignId,
    testName: '',
    testType: ABTestType.SubjectLine,
    variantA: '',
    variantB: '',
    variantC: '',
    trafficSplit: { variantA: 50, variantB: 50 },
    testMetric: 'OpenRate',
    sampleSize: 1000,
    autoWinnerAfterHours: 24
  };

  const [workflowForm, setWorkflowForm] = useState(emptyWorkflowForm);
  const [abTestForm, setAbTestForm] = useState(emptyABTestForm);
  const [recipientEmails, setRecipientEmails] = useState('');

  // Fetch data
  const fetchData = useCallback(async () => {
    if (!campaignId) return;
    
    try {
      setLoading(true);
      const [
        campaignRes,
        workflowsRes,
        recipientsRes,
        abTestsRes,
        conversionsRes,
        analyticsRes,
        availableWorkflowsRes
      ] = await Promise.all([
        apiClient.get(`/campaigns/${campaignId}`),
        campaignExecutionService.getCampaignWorkflows(campaignId),
        campaignExecutionService.getCampaignRecipients(campaignId),
        campaignExecutionService.getCampaignABTests(campaignId),
        campaignExecutionService.getCampaignConversions(campaignId),
        campaignExecutionService.getCampaignAnalytics(campaignId).catch(() => null),
        apiClient.get('/workflows/definitions')
      ]);

      setCampaign(campaignRes.data);
      setWorkflows(workflowsRes);
      setRecipients(recipientsRes);
      setAbTests(abTestsRes);
      setConversions(conversionsRes);
      setAnalytics(analyticsRes);
      setAvailableWorkflows(availableWorkflowsRes.data);
      setError(null);
    } catch (err) {
      setError('Failed to load campaign execution data');
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, [campaignId]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  // Handle workflow operations
  const handleLinkWorkflow = async () => {
    try {
      await campaignExecutionService.linkCampaignWorkflow({
        ...workflowForm,
        campaignId
      });
      setSuccessMessage('Workflow linked successfully');
      setWorkflowDialogOpen(false);
      fetchData();
    } catch (err) {
      setError('Failed to link workflow');
    }
  };

  const handleExecuteWorkflow = async (workflowId: number) => {
    try {
      const result = await campaignExecutionService.executeCampaignWorkflow(campaignId, workflowId);
      setSuccessMessage(`Workflow instance started: ${result.workflowInstanceId}`);
      fetchData();
    } catch (err) {
      setError('Failed to execute workflow');
    }
  };

  const handleToggleWorkflow = async (workflowId: number, isActive: boolean) => {
    try {
      await campaignExecutionService.toggleWorkflowActive(campaignId, workflowId, isActive);
      setSuccessMessage(`Workflow ${isActive ? 'activated' : 'deactivated'}`);
      fetchData();
    } catch (err) {
      setError('Failed to toggle workflow');
    }
  };

  // Handle recipient operations
  const handleAddRecipients = async () => {
    try {
      const emails = recipientEmails.split('\n').map(e => e.trim()).filter(e => e);
      const result = await campaignExecutionService.addCampaignRecipients(campaignId, emails);
      setSuccessMessage(`Added ${result.added} recipients (${result.duplicates} duplicates skipped)`);
      setAddRecipientsDialogOpen(false);
      setRecipientEmails('');
      fetchData();
    } catch (err) {
      setError('Failed to add recipients');
    }
  };

  // Handle A/B test operations
  const handleCreateABTest = async () => {
    try {
      await campaignExecutionService.createABTest({
        ...abTestForm,
        campaignId
      });
      setSuccessMessage('A/B test created successfully');
      setAbTestDialogOpen(false);
      fetchData();
    } catch (err) {
      setError('Failed to create A/B test');
    }
  };

  const handleStartABTest = async (testId: number) => {
    try {
      await campaignExecutionService.startABTest(campaignId, testId);
      setSuccessMessage('A/B test started');
      fetchData();
    } catch (err) {
      setError('Failed to start A/B test');
    }
  };

  const handleCompleteABTest = async (testId: number, winningVariant: string) => {
    try {
      await campaignExecutionService.completeABTest(campaignId, testId, winningVariant);
      setSuccessMessage(`A/B test completed. Winner: ${winningVariant}`);
      fetchData();
    } catch (err) {
      setError('Failed to complete A/B test');
    }
  };

  // Status colors
  const getRecipientStatusColor = (status: RecipientStatus) => {
    const colors: Record<RecipientStatus, 'default' | 'info' | 'success' | 'warning' | 'error'> = {
      [RecipientStatus.Pending]: 'default',
      [RecipientStatus.Sent]: 'info',
      [RecipientStatus.Delivered]: 'info',
      [RecipientStatus.Opened]: 'success',
      [RecipientStatus.Clicked]: 'success',
      [RecipientStatus.Converted]: 'success',
      [RecipientStatus.Bounced]: 'error',
      [RecipientStatus.Unsubscribed]: 'warning',
      [RecipientStatus.Failed]: 'error'
    };
    return colors[status] || 'default';
  };

  const getABTestStatusColor = (status: ABTestStatus) => {
    const colors: Record<ABTestStatus, 'default' | 'info' | 'success' | 'warning'> = {
      [ABTestStatus.Draft]: 'default',
      [ABTestStatus.Running]: 'info',
      [ABTestStatus.Completed]: 'success',
      [ABTestStatus.Cancelled]: 'warning'
    };
    return colors[status] || 'default';
  };

  if (loading) {
    return (
      <Container maxWidth="xl">
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (!campaign) {
    return (
      <Container maxWidth="xl">
        <Alert severity="error">Campaign not found</Alert>
        <Button onClick={() => navigate('/campaigns')} sx={{ mt: 2 }}>
          Back to Campaigns
        </Button>
      </Container>
    );
  }

  return (
    <Container maxWidth="xl">
      <Box sx={{ mb: 4 }}>
        <Button onClick={() => navigate('/campaigns')} sx={{ mb: 2 }}>
          ← Back to Campaigns
        </Button>
        <Typography variant="h4" component="h1" gutterBottom>
          <EmailIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
          {campaign.name} - Execution Dashboard
        </Typography>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {successMessage && (
        <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccessMessage(null)}>
          {successMessage}
        </Alert>
      )}

      {/* Analytics Summary Cards */}
      {analytics && (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={6} md={2}>
            <Card>
              <CardContent>
                <Typography color="text.secondary" variant="body2">Recipients</Typography>
                <Typography variant="h5">{analytics.totalRecipients}</Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={6} md={2}>
            <Card>
              <CardContent>
                <Typography color="text.secondary" variant="body2">Delivered</Typography>
                <Typography variant="h5">{analytics.deliveryRate.toFixed(1)}%</Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={6} md={2}>
            <Card>
              <CardContent>
                <Typography color="text.secondary" variant="body2">Open Rate</Typography>
                <Typography variant="h5">{analytics.openRate.toFixed(1)}%</Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={6} md={2}>
            <Card>
              <CardContent>
                <Typography color="text.secondary" variant="body2">Click Rate</Typography>
                <Typography variant="h5">{analytics.clickRate.toFixed(1)}%</Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={6} md={2}>
            <Card>
              <CardContent>
                <Typography color="text.secondary" variant="body2">Conversion</Typography>
                <Typography variant="h5">{analytics.conversionRate.toFixed(1)}%</Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={6} md={2}>
            <Card>
              <CardContent>
                <Typography color="text.secondary" variant="body2">ROI</Typography>
                <Typography variant="h5" color={analytics.roi >= 0 ? 'success.main' : 'error.main'}>
                  {analytics.roi.toFixed(0)}%
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
        <Tabs value={tabValue} onChange={(_, v) => setTabValue(v)}>
          <Tab label="Workflows" icon={<PlayIcon />} iconPosition="start" />
          <Tab label="Recipients" icon={<RecipientsIcon />} iconPosition="start" />
          <Tab label="A/B Tests" icon={<ABTestIcon />} iconPosition="start" />
          <Tab label="Conversions" icon={<ConversionIcon />} iconPosition="start" />
          <Tab label="Analytics" icon={<AnalyticsIcon />} iconPosition="start" />
        </Tabs>
      </Box>

      {/* Workflows Tab */}
      <TabPanel value={tabValue} index={0}>
        <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between' }}>
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setWorkflowDialogOpen(true)}>
            Link Workflow
          </Button>
          <Button startIcon={<RefreshIcon />} onClick={fetchData}>
            Refresh
          </Button>
        </Box>

        <Card>
          <CardContent>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Workflow</TableCell>
                  <TableCell>Type</TableCell>
                  <TableCell>Trigger Event</TableCell>
                  <TableCell>Priority</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {workflows.map((wf) => (
                  <TableRow key={wf.id}>
                    <TableCell>{wf.workflowDefinitionName || `Workflow #${wf.workflowDefinitionId}`}</TableCell>
                    <TableCell><Chip label={wf.workflowType} size="small" /></TableCell>
                    <TableCell>{wf.triggerEvent}</TableCell>
                    <TableCell>{wf.priority}</TableCell>
                    <TableCell>
                      <Chip 
                        label={wf.isActive ? 'Active' : 'Inactive'} 
                        color={wf.isActive ? 'success' : 'default'} 
                        size="small" 
                      />
                    </TableCell>
                    <TableCell>
                      <Tooltip title="Execute">
                        <IconButton size="small" onClick={() => handleExecuteWorkflow(wf.id)}>
                          <PlayIcon />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title={wf.isActive ? 'Deactivate' : 'Activate'}>
                        <IconButton size="small" onClick={() => handleToggleWorkflow(wf.id, !wf.isActive)}>
                          {wf.isActive ? <PauseIcon /> : <PlayIcon />}
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))}
                {workflows.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={6} align="center">
                      <Typography color="text.secondary">
                        No workflows linked. Link a workflow to automate campaign execution.
                      </Typography>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </TabPanel>

      {/* Recipients Tab */}
      <TabPanel value={tabValue} index={1}>
        <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between' }}>
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setAddRecipientsDialogOpen(true)}>
            Add Recipients
          </Button>
          <Typography variant="body2" color="text.secondary">
            Total: {recipients.length} recipients
          </Typography>
        </Box>

        <Card>
          <CardContent>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Email</TableCell>
                  <TableCell>Contact</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Opens</TableCell>
                  <TableCell>Clicks</TableCell>
                  <TableCell>A/B Variant</TableCell>
                  <TableCell>Last Activity</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {recipients.slice(0, 100).map((recipient) => (
                  <TableRow key={recipient.id}>
                    <TableCell>{recipient.email}</TableCell>
                    <TableCell>{recipient.contactName || '-'}</TableCell>
                    <TableCell>
                      <Chip label={recipient.status} color={getRecipientStatusColor(recipient.status)} size="small" />
                    </TableCell>
                    <TableCell>{recipient.openCount}</TableCell>
                    <TableCell>{recipient.clickCount}</TableCell>
                    <TableCell>{recipient.abTestVariant || '-'}</TableCell>
                    <TableCell>
                      {recipient.lastClickedAt 
                        ? new Date(recipient.lastClickedAt).toLocaleString()
                        : recipient.lastOpenedAt 
                          ? new Date(recipient.lastOpenedAt).toLocaleString()
                          : recipient.deliveredAt
                            ? new Date(recipient.deliveredAt).toLocaleString()
                            : '-'}
                    </TableCell>
                  </TableRow>
                ))}
                {recipients.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={7} align="center">
                      <Typography color="text.secondary">
                        No recipients added yet.
                      </Typography>
                    </TableCell>
                  </TableRow>
                )}
                {recipients.length > 100 && (
                  <TableRow>
                    <TableCell colSpan={7} align="center">
                      <Typography variant="body2" color="text.secondary">
                        Showing first 100 of {recipients.length} recipients
                      </Typography>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </TabPanel>

      {/* A/B Tests Tab */}
      <TabPanel value={tabValue} index={2}>
        <Box sx={{ mb: 2 }}>
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setAbTestDialogOpen(true)}>
            Create A/B Test
          </Button>
        </Box>

        <Grid container spacing={2}>
          {abTests.map((test) => (
            <Grid item xs={12} md={6} key={test.id}>
              <Card>
                <CardContent>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
                    <Typography variant="h6">{test.testName}</Typography>
                    <Chip label={test.status} color={getABTestStatusColor(test.status)} size="small" />
                  </Box>
                  <Stack spacing={1}>
                    <Typography variant="body2">Type: {test.testType}</Typography>
                    <Typography variant="body2">Metric: {test.testMetric}</Typography>
                    <Divider />
                    <Typography variant="body2">Variant A: {test.variantA}</Typography>
                    <Typography variant="body2">Variant B: {test.variantB}</Typography>
                    {test.variantC && <Typography variant="body2">Variant C: {test.variantC}</Typography>}
                    <Divider />
                    {test.winningVariant && (
                      <Alert severity="success" sx={{ py: 0 }}>
                        Winner: Variant {test.winningVariant}
                      </Alert>
                    )}
                    <Box>
                      <Typography variant="body2" color="text.secondary">Results:</Typography>
                      <Grid container spacing={1}>
                        <Grid item xs={4}>
                          <Paper sx={{ p: 1, textAlign: 'center' }}>
                            <Typography variant="body2">A</Typography>
                            <Typography variant="h6">{test.variantAMetric?.toFixed(1) || '-'}%</Typography>
                          </Paper>
                        </Grid>
                        <Grid item xs={4}>
                          <Paper sx={{ p: 1, textAlign: 'center' }}>
                            <Typography variant="body2">B</Typography>
                            <Typography variant="h6">{test.variantBMetric?.toFixed(1) || '-'}%</Typography>
                          </Paper>
                        </Grid>
                        {test.variantC && (
                          <Grid item xs={4}>
                            <Paper sx={{ p: 1, textAlign: 'center' }}>
                              <Typography variant="body2">C</Typography>
                              <Typography variant="h6">{test.variantCMetric?.toFixed(1) || '-'}%</Typography>
                            </Paper>
                          </Grid>
                        )}
                      </Grid>
                    </Box>
                  </Stack>
                  <Box sx={{ mt: 2 }}>
                    {test.status === ABTestStatus.Draft && (
                      <Button size="small" variant="contained" onClick={() => handleStartABTest(test.id)}>
                        Start Test
                      </Button>
                    )}
                    {test.status === ABTestStatus.Running && (
                      <Stack direction="row" spacing={1}>
                        <Button size="small" variant="outlined" onClick={() => handleCompleteABTest(test.id, 'A')}>
                          Pick A
                        </Button>
                        <Button size="small" variant="outlined" onClick={() => handleCompleteABTest(test.id, 'B')}>
                          Pick B
                        </Button>
                        {test.variantC && (
                          <Button size="small" variant="outlined" onClick={() => handleCompleteABTest(test.id, 'C')}>
                            Pick C
                          </Button>
                        )}
                      </Stack>
                    )}
                  </Box>
                </CardContent>
              </Card>
            </Grid>
          ))}
          {abTests.length === 0 && (
            <Grid item xs={12}>
              <Alert severity="info">
                No A/B tests created. Create one to optimize your campaign performance.
              </Alert>
            </Grid>
          )}
        </Grid>
      </TabPanel>

      {/* Conversions Tab */}
      <TabPanel value={tabValue} index={3}>
        <Card>
          <CardContent>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Contact</TableCell>
                  <TableCell>Type</TableCell>
                  <TableCell>Value</TableCell>
                  <TableCell>Attribution</TableCell>
                  <TableCell>Source</TableCell>
                  <TableCell>Date</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {conversions.map((conv) => (
                  <TableRow key={conv.id}>
                    <TableCell>{conv.contactName || `Contact #${conv.contactId}`}</TableCell>
                    <TableCell><Chip label={conv.conversionType} size="small" /></TableCell>
                    <TableCell>
                      {conv.conversionValue ? `${conv.currency || '$'}${conv.conversionValue.toFixed(2)}` : '-'}
                    </TableCell>
                    <TableCell>{conv.attributionModel} ({(conv.attributionWeight * 100).toFixed(0)}%)</TableCell>
                    <TableCell>{conv.sourceChannel || '-'}</TableCell>
                    <TableCell>{new Date(conv.convertedAt).toLocaleString()}</TableCell>
                  </TableRow>
                ))}
                {conversions.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={6} align="center">
                      <Typography color="text.secondary">
                        No conversions recorded yet.
                      </Typography>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </TabPanel>

      {/* Analytics Tab */}
      <TabPanel value={tabValue} index={4}>
        {analytics ? (
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>Engagement Funnel</Typography>
                  <Stack spacing={2}>
                    <Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography>Delivered</Typography>
                        <Typography>{analytics.delivered} ({analytics.deliveryRate.toFixed(1)}%)</Typography>
                      </Box>
                      <LinearProgress variant="determinate" value={analytics.deliveryRate} />
                    </Box>
                    <Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography>Opened</Typography>
                        <Typography>{analytics.opened} ({analytics.openRate.toFixed(1)}%)</Typography>
                      </Box>
                      <LinearProgress variant="determinate" value={analytics.openRate} color="info" />
                    </Box>
                    <Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography>Clicked</Typography>
                        <Typography>{analytics.clicked} ({analytics.clickRate.toFixed(1)}%)</Typography>
                      </Box>
                      <LinearProgress variant="determinate" value={analytics.clickRate} color="primary" />
                    </Box>
                    <Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography>Converted</Typography>
                        <Typography>{analytics.converted} ({analytics.conversionRate.toFixed(1)}%)</Typography>
                      </Box>
                      <LinearProgress variant="determinate" value={analytics.conversionRate} color="success" />
                    </Box>
                  </Stack>
                </CardContent>
              </Card>
            </Grid>
            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>Revenue & ROI</Typography>
                  <Stack spacing={2}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography>Total Revenue</Typography>
                      <Typography variant="h6">${analytics.totalRevenue.toFixed(2)}</Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography>Average Order Value</Typography>
                      <Typography variant="h6">${analytics.averageOrderValue.toFixed(2)}</Typography>
                    </Box>
                    <Divider />
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography>Campaign ROI</Typography>
                      <Typography 
                        variant="h5" 
                        color={analytics.roi >= 0 ? 'success.main' : 'error.main'}
                      >
                        {analytics.roi >= 0 ? '+' : ''}{analytics.roi.toFixed(0)}%
                      </Typography>
                    </Box>
                  </Stack>
                </CardContent>
              </Card>
            </Grid>
            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>Negative Metrics</Typography>
                  <Stack spacing={2}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography>Bounced</Typography>
                      <Typography color="error">{analytics.bounced} ({analytics.bounceRate.toFixed(2)}%)</Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography>Unsubscribed</Typography>
                      <Typography color="warning.main">{analytics.unsubscribed} ({analytics.unsubscribeRate.toFixed(2)}%)</Typography>
                    </Box>
                  </Stack>
                </CardContent>
              </Card>
            </Grid>
            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>Top Links</Typography>
                  {analytics.topLinks && analytics.topLinks.length > 0 ? (
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Link</TableCell>
                          <TableCell align="right">Clicks</TableCell>
                          <TableCell align="right">Unique</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {analytics.topLinks.map((link, idx) => (
                          <TableRow key={idx}>
                            <TableCell>
                              <Tooltip title={link.linkUrl}>
                                <Typography variant="body2" noWrap sx={{ maxWidth: 200 }}>
                                  {link.linkName || link.linkUrl}
                                </Typography>
                              </Tooltip>
                            </TableCell>
                            <TableCell align="right">{link.clicks}</TableCell>
                            <TableCell align="right">{link.uniqueClicks}</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  ) : (
                    <Typography color="text.secondary">No link clicks recorded</Typography>
                  )}
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        ) : (
          <Alert severity="info">
            Analytics will be available once the campaign has recipients and engagement data.
          </Alert>
        )}
      </TabPanel>

      {/* Link Workflow Dialog */}
      <Dialog open={workflowDialogOpen} onClose={() => setWorkflowDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Link Workflow to Campaign</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <FormControl fullWidth required>
                <InputLabel>Workflow</InputLabel>
                <Select
                  value={workflowForm.workflowDefinitionId}
                  label="Workflow"
                  onChange={(e) => setWorkflowForm({ ...workflowForm, workflowDefinitionId: Number(e.target.value) })}
                >
                  {availableWorkflows.map((wf) => (
                    <MenuItem key={wf.id} value={wf.id}>{wf.name}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>Workflow Type</InputLabel>
                <Select
                  value={workflowForm.workflowType}
                  label="Workflow Type"
                  onChange={(e) => setWorkflowForm({ ...workflowForm, workflowType: e.target.value as CampaignWorkflowType })}
                >
                  {Object.values(CampaignWorkflowType).map((type) => (
                    <MenuItem key={type} value={type}>{type}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Trigger Event"
                value={workflowForm.triggerEvent}
                onChange={(e) => setWorkflowForm({ ...workflowForm, triggerEvent: e.target.value })}
                helperText="e.g., CampaignStarted, RecipientClicked, ConversionRecorded"
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Priority"
                type="number"
                value={workflowForm.priority}
                onChange={(e) => setWorkflowForm({ ...workflowForm, priority: Number(e.target.value) })}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setWorkflowDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleLinkWorkflow}>Link Workflow</Button>
        </DialogActions>
      </Dialog>

      {/* Add Recipients Dialog */}
      <Dialog open={addRecipientsDialogOpen} onClose={() => setAddRecipientsDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Add Recipients</DialogTitle>
        <DialogContent>
          <TextField
            fullWidth
            multiline
            rows={10}
            label="Email Addresses (one per line)"
            value={recipientEmails}
            onChange={(e) => setRecipientEmails(e.target.value)}
            placeholder="john@example.com&#10;jane@example.com&#10;..."
            sx={{ mt: 2 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAddRecipientsDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleAddRecipients}>Add Recipients</Button>
        </DialogActions>
      </Dialog>

      {/* Create A/B Test Dialog */}
      <Dialog open={abTestDialogOpen} onClose={() => setAbTestDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>Create A/B Test</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Test Name"
                value={abTestForm.testName}
                onChange={(e) => setAbTestForm({ ...abTestForm, testName: e.target.value })}
                required
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Test Type</InputLabel>
                <Select
                  value={abTestForm.testType}
                  label="Test Type"
                  onChange={(e) => setAbTestForm({ ...abTestForm, testType: e.target.value as ABTestType })}
                >
                  {Object.values(ABTestType).map((type) => (
                    <MenuItem key={type} value={type}>{type}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Test Metric"
                value={abTestForm.testMetric}
                onChange={(e) => setAbTestForm({ ...abTestForm, testMetric: e.target.value })}
                helperText="e.g., OpenRate, ClickRate, ConversionRate"
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Variant A"
                value={abTestForm.variantA}
                onChange={(e) => setAbTestForm({ ...abTestForm, variantA: e.target.value })}
                multiline
                rows={2}
                required
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Variant B"
                value={abTestForm.variantB}
                onChange={(e) => setAbTestForm({ ...abTestForm, variantB: e.target.value })}
                multiline
                rows={2}
                required
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Variant C (Optional)"
                value={abTestForm.variantC}
                onChange={(e) => setAbTestForm({ ...abTestForm, variantC: e.target.value })}
                multiline
                rows={2}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Sample Size"
                type="number"
                value={abTestForm.sampleSize}
                onChange={(e) => setAbTestForm({ ...abTestForm, sampleSize: Number(e.target.value) })}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Auto-select Winner After (hours)"
                type="number"
                value={abTestForm.autoWinnerAfterHours}
                onChange={(e) => setAbTestForm({ ...abTestForm, autoWinnerAfterHours: Number(e.target.value) })}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAbTestDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleCreateABTest}>Create Test</Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

export default CampaignExecutionPage;

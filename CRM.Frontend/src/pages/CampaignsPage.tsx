import { useState, useEffect } from 'react';
import { TabPanel } from '../components/common';
import {
  Box, Card, CardContent, Typography, Button, Table, TableBody, TableCell, TableHead,
  TableRow, Dialog, DialogTitle, DialogContent, DialogActions, Alert, CircularProgress,
  TextField, Container, FormControl, InputLabel, Select, MenuItem, Chip, Tabs, Tab,
  Grid, IconButton, Tooltip, FormControlLabel, Checkbox, LinearProgress,
  SelectChangeEvent, Paper, Collapse, Stack
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, 
  Campaign as CampaignIcon, TrendingUp as TrendingUpIcon,
  Email as EmailIcon, Share as ShareIcon, Close as CloseIcon,
  Note as NoteIcon
} from '@mui/icons-material';
import { DialogError, ActionButton } from '../components/common';
import { useApiState } from '../hooks/useApiState';
import { useProfile } from '../contexts/ProfileContext';
import apiClient from '../services/apiClient';
import { BaseEntity } from '../types';
import logo from '../assets/logo.png';
import LookupSelect from '../components/LookupSelect';
import ImportExportButtons from '../components/ImportExportButtons';
import NotesTab from '../components/NotesTab';
import {
  CAMPAIGN_STATUS_OPTIONS,
  CAMPAIGN_TYPE_OPTIONS,
  PRIORITY_OPTIONS,
  getLabelByValue,
  getColorByValue
} from '../utils/constants';

// Use shared constants - aliased for backward compatibility
const CAMPAIGN_TYPES = CAMPAIGN_TYPE_OPTIONS;
const CAMPAIGN_STATUSES = CAMPAIGN_STATUS_OPTIONS;
const CAMPAIGN_PRIORITIES = PRIORITY_OPTIONS;

interface Campaign extends BaseEntity {
  name: string;
  description: string;
  campaignType: number;
  status: number;
  priority: number;
  startDate: string;
  endDate: string;
  budget: number;
  actualSpend: number;
  targetAudience: number;
  // Performance metrics
  impressions: number;
  clicks: number;
  ctr: number;
  conversions: number;
  conversionRate: number;
  leadsGenerated: number;
  revenue: number;
  roi: number;
  // Email metrics
  emailsSent: number;
  emailsOpened: number;
  openRate: number;
  unsubscribes: number;
  bounces: number;
  // Social metrics
  socialReach: number;
  socialEngagement: number;
  socialShares: number;
  // A/B Testing
  isABTest: boolean;
  abTestVariants?: string;
  winningVariant?: string;
  // UTM
  utmSource?: string;
  utmMedium?: string;
  utmCampaign?: string;
  tags?: string;
}

interface CampaignForm {
  name: string;
  description: string;
  campaignType: number;
  status: number;
  priority: number;
  startDate: string;
  endDate: string;
  budget: number;
  actualSpend: number;
  targetAudience: number;
  impressions: number;
  clicks: number;
  conversions: number;
  leadsGenerated: number;
  revenue: number;
  emailsSent: number;
  emailsOpened: number;
  unsubscribes: number;
  bounces: number;
  socialReach: number;
  socialEngagement: number;
  socialShares: number;
  isABTest: boolean;
  abTestVariants: string;
  winningVariant: string;
  utmSource: string;
  utmMedium: string;
  utmCampaign: string;
  tags: string;
}

function CampaignsPage() {
  const [campaigns, setCampaigns] = useState<Campaign[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  
  // Multi-select and bulk operations
  const [selectedIds, setSelectedIds] = useState<number[]>([]);
  const [bulkDialogOpen, setBulkDialogOpen] = useState(false);
  const [bulkFormData, setBulkFormData] = useState<{ status: string; priority: string; campaignType: string }>({
    status: '',
    priority: '',
    campaignType: '',
  });
  
  // API state hooks
  const dialogApi = useApiState();
  const bulkApi = useApiState();
  const { hasPermission } = useProfile();
  
  const emptyForm: CampaignForm = {
    name: '', description: '', campaignType: 0, status: 0, priority: 1,
    startDate: '', endDate: '', budget: 0, actualSpend: 0, targetAudience: 0,
    impressions: 0, clicks: 0, conversions: 0, leadsGenerated: 0, revenue: 0,
    emailsSent: 0, emailsOpened: 0, unsubscribes: 0, bounces: 0,
    socialReach: 0, socialEngagement: 0, socialShares: 0,
    isABTest: false, abTestVariants: '', winningVariant: '',
    utmSource: '', utmMedium: '', utmCampaign: '', tags: '',
  };
  const [formData, setFormData] = useState<CampaignForm>(emptyForm);

  useEffect(() => { fetchCampaigns(); }, []);

  const fetchCampaigns = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/campaigns');
      setCampaigns(response.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch campaigns');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = (campaign?: Campaign) => {
    setDialogTab(0);
    if (campaign) {
      setEditingId(campaign.id);
      setFormData({
        name: campaign.name, description: campaign.description || '',
        campaignType: campaign.campaignType, status: campaign.status, priority: campaign.priority,
        startDate: campaign.startDate?.split('T')[0] || '', endDate: campaign.endDate?.split('T')[0] || '',
        budget: campaign.budget, actualSpend: campaign.actualSpend, targetAudience: campaign.targetAudience,
        impressions: campaign.impressions, clicks: campaign.clicks, conversions: campaign.conversions,
        leadsGenerated: campaign.leadsGenerated, revenue: campaign.revenue,
        emailsSent: campaign.emailsSent, emailsOpened: campaign.emailsOpened,
        unsubscribes: campaign.unsubscribes, bounces: campaign.bounces,
        socialReach: campaign.socialReach, socialEngagement: campaign.socialEngagement,
        socialShares: campaign.socialShares, isABTest: campaign.isABTest,
        abTestVariants: campaign.abTestVariants || '', winningVariant: campaign.winningVariant || '',
        utmSource: campaign.utmSource || '', utmMedium: campaign.utmMedium || '',
        utmCampaign: campaign.utmCampaign || '', tags: campaign.tags || '',
      });
    } else {
      setEditingId(null);
      setFormData(emptyForm);
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => { setOpenDialog(false); setEditingId(null); };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    const checked = (e.target as HTMLInputElement).checked;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : type === 'number' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleSelectChange = (e: SelectChangeEvent<string | number>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSaveCampaign = async () => {
    if (!formData.name.trim() || !formData.startDate) {
      dialogApi.setError('Please fill in required fields (Name, Start Date)');
      return;
    }
    await dialogApi.execute(async () => {
      if (editingId) {
        await apiClient.put(`/campaigns/${editingId}`, formData);
        setSuccessMessage('Campaign updated successfully');
      } else {
        await apiClient.post('/campaigns', formData);
        setSuccessMessage('Campaign created successfully');
      }
      handleCloseDialog();
      fetchCampaigns();
      setTimeout(() => setSuccessMessage(null), 3000);
    });
  };

  const handleDeleteCampaign = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this campaign?')) {
      await dialogApi.execute(async () => {
        await apiClient.delete(`/campaigns/${id}`);
        setSuccessMessage('Campaign deleted successfully');
        fetchCampaigns();
        setTimeout(() => setSuccessMessage(null), 3000);
      });
    }
  };
  
  // Multi-select handlers
  const handleSelectAll = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.checked) {
      setSelectedIds(campaigns.map(c => c.id));
    } else {
      setSelectedIds([]);
    }
  };
  
  const handleSelectOne = (id: number) => {
    setSelectedIds(prev => 
      prev.includes(id) ? prev.filter(i => i !== id) : [...prev, id]
    );
  };
  
  const handleOpenBulkDialog = () => {
    setBulkFormData({ status: '', priority: '', campaignType: '' });
    bulkApi.clearError();
    setBulkDialogOpen(true);
  };
  
  const handleBulkUpdate = async () => {
    await bulkApi.execute(async () => {
      const updates = selectedIds.map(id => {
        const updatePayload: any = {};
        if (bulkFormData.status) updatePayload.status = parseInt(bulkFormData.status);
        if (bulkFormData.priority) updatePayload.priority = parseInt(bulkFormData.priority);
        if (bulkFormData.campaignType) updatePayload.campaignType = parseInt(bulkFormData.campaignType);
        return apiClient.put(`/campaigns/${id}`, updatePayload);
      });
      await Promise.all(updates);
      setSuccessMessage(`Updated ${selectedIds.length} campaigns`);
      setSelectedIds([]);
      setBulkDialogOpen(false);
      fetchCampaigns();
    });
  };
  
  const handleBulkDelete = async () => {
    if (!window.confirm(`Are you sure you want to delete ${selectedIds.length} campaigns?`)) return;
    await bulkApi.execute(async () => {
      await Promise.all(selectedIds.map(id => apiClient.delete(`/campaigns/${id}`)));
      setSuccessMessage(`Deleted ${selectedIds.length} campaigns`);
      setSelectedIds([]);
      fetchCampaigns();
    });
  };

  const getStatus = (value: number) => CAMPAIGN_STATUSES.find(s => s.value === value);
  const getType = (value: number) => CAMPAIGN_TYPES.find(t => t.value === value);
  const getPriority = (value: number) => CAMPAIGN_PRIORITIES.find(p => p.value === value);

  const calculateROI = (campaign: Campaign) => {
    if (campaign.actualSpend > 0) {
      return ((campaign.revenue - campaign.actualSpend) / campaign.actualSpend * 100).toFixed(1);
    }
    return '0';
  };

  const calculateCTR = (campaign: Campaign) => {
    if (campaign.impressions > 0) {
      return ((campaign.clicks / campaign.impressions) * 100).toFixed(2);
    }
    return '0';
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ py: 4 }}>
      <Container maxWidth="xl">
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
              <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
            </Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>Campaigns</Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <ImportExportButtons entityType="campaigns" entityLabel="Campaigns" onImportComplete={fetchCampaigns} />
            <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenDialog()} sx={{ backgroundColor: '#6750A4' }}>
              Add Campaign
            </Button>
          </Box>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

        {/* Bulk Actions Toolbar */}
        <Collapse in={selectedIds.length > 0}>
          <Paper sx={{ p: 2, mb: 2, backgroundColor: '#e3f2fd' }}>
            <Stack direction="row" spacing={2} alignItems="center">
              <Typography variant="body1">
                {selectedIds.length} item(s) selected
              </Typography>
              <Button
                variant="contained"
                size="small"
                onClick={handleOpenBulkDialog}
              >
                Bulk Update
              </Button>
              {hasPermission('canDeleteCampaigns') && (
                <Button
                  variant="outlined"
                  color="error"
                  size="small"
                  onClick={handleBulkDelete}
                >
                  Delete Selected
                </Button>
              )}
              <IconButton size="small" onClick={() => setSelectedIds([])}>
                <CloseIcon />
              </IconButton>
            </Stack>
          </Paper>
        </Collapse>

        <Card>
          <CardContent sx={{ p: 0, overflowX: 'auto' }}>
            <Table>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                  <TableCell padding="checkbox">
                    <Checkbox
                      indeterminate={selectedIds.length > 0 && selectedIds.length < campaigns.length}
                      checked={campaigns.length > 0 && selectedIds.length === campaigns.length}
                      onChange={handleSelectAll}
                    />
                  </TableCell>
                  <TableCell><strong>Campaign</strong></TableCell>
                  <TableCell><strong>Type</strong></TableCell>
                  <TableCell><strong>Status</strong></TableCell>
                  <TableCell><strong>Budget</strong></TableCell>
                  <TableCell><strong>Performance</strong></TableCell>
                  <TableCell><strong>ROI</strong></TableCell>
                  <TableCell><strong>Dates</strong></TableCell>
                  <TableCell align="center"><strong>Actions</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {campaigns.map((campaign) => {
                  const status = getStatus(campaign.status);
                  const type = getType(campaign.campaignType);
                  const priority = getPriority(campaign.priority);
                  const roi = parseFloat(calculateROI(campaign));
                  const budgetUsed = campaign.budget > 0 ? (campaign.actualSpend / campaign.budget) * 100 : 0;
                  
                  return (
                    <TableRow key={campaign.id} hover selected={selectedIds.includes(campaign.id)}>
                      <TableCell padding="checkbox">
                        <Checkbox
                          checked={selectedIds.includes(campaign.id)}
                          onChange={() => handleSelectOne(campaign.id)}
                        />
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <CampaignIcon sx={{ color: '#6750A4' }} />
                          <Box>
                            <Typography fontWeight={500}>{campaign.name}</Typography>
                            {campaign.description && (
                              <Typography variant="caption" color="textSecondary" sx={{ display: 'block', maxWidth: 200, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                                {campaign.description}
                              </Typography>
                            )}
                          </Box>
                          {campaign.isABTest && <Chip label="A/B" size="small" color="secondary" sx={{ ml: 1 }} />}
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Chip label={`${type?.icon || ''} ${type?.label || 'Unknown'}`} size="small" variant="outlined" />
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
                          <Chip label={status?.label || 'Unknown'} size="small" sx={{ backgroundColor: status?.color, color: 'white' }} />
                          <Chip label={priority?.label || 'Medium'} size="small" sx={{ backgroundColor: priority?.color, color: 'white', fontSize: 10 }} />
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Box>
                          <Typography fontWeight={500}>${campaign.actualSpend?.toLocaleString() || 0}</Typography>
                          <Typography variant="caption" color="textSecondary">of ${campaign.budget?.toLocaleString() || 0}</Typography>
                          <LinearProgress 
                            variant="determinate" 
                            value={Math.min(budgetUsed, 100)} 
                            sx={{ mt: 0.5, height: 4, borderRadius: 2 }}
                            color={budgetUsed > 100 ? 'error' : budgetUsed > 80 ? 'warning' : 'primary'}
                          />
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5, fontSize: 12 }}>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Typography variant="caption"><strong>{campaign.impressions?.toLocaleString() || 0}</strong> impressions</Typography>
                          </Box>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Typography variant="caption"><strong>{campaign.clicks?.toLocaleString() || 0}</strong> clicks ({calculateCTR(campaign)}%)</Typography>
                          </Box>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Typography variant="caption"><strong>{campaign.leadsGenerated || 0}</strong> leads</Typography>
                          </Box>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <TrendingUpIcon sx={{ color: roi >= 0 ? '#4caf50' : '#f44336', fontSize: 18 }} />
                          <Typography sx={{ color: roi >= 0 ? '#4caf50' : '#f44336', fontWeight: 500 }}>
                            {roi >= 0 ? '+' : ''}{roi}%
                          </Typography>
                        </Box>
                        <Typography variant="caption" color="textSecondary">
                          ${campaign.revenue?.toLocaleString() || 0} revenue
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">{new Date(campaign.startDate).toLocaleDateString()}</Typography>
                        <Typography variant="caption" color="textSecondary">
                          to {campaign.endDate ? new Date(campaign.endDate).toLocaleDateString() : 'Ongoing'}
                        </Typography>
                      </TableCell>
                      <TableCell align="center">
                        <Tooltip title="Edit">
                          <IconButton size="small" onClick={() => handleOpenDialog(campaign)} sx={{ color: '#6750A4' }}>
                            <EditIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton size="small" onClick={() => handleDeleteCampaign(campaign.id)} sx={{ color: '#f44336' }}>
                            <DeleteIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
            {campaigns.length === 0 && (
              <Typography sx={{ textAlign: 'center', py: 4, color: 'textSecondary' }}>
                No campaigns found. Create your first campaign to get started.
              </Typography>
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Enhanced Add/Edit Campaign Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle sx={{ pb: 0 }}>{editingId ? 'Edit Campaign' : 'Add Campaign'}</DialogTitle>
        <Box sx={{ borderBottom: 1, borderColor: 'divider', px: 3 }}>
          <Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)}>
            <Tab label="Basic Info" />
            <Tab label="Performance" />
            <Tab label="Email Metrics" />
            <Tab label="Social & A/B" />
            <Tab label="Tracking" />
            <Tab label="Notes" icon={<NoteIcon fontSize="small" />} iconPosition="start" />
          </Tabs>
        </Box>
        <DialogContent sx={{ pt: 2, minHeight: 400 }}>
          <TabPanel value={dialogTab} index={0}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField fullWidth label="Campaign Name" name="name" value={formData.name} onChange={handleInputChange} required />
              </Grid>
              <Grid item xs={4}>
                <LookupSelect
                  category="CampaignType"
                  name="campaignType"
                  value={formData.campaignType}
                  onChange={handleSelectChange}
                  label="Campaign Type"
                  fallback={CAMPAIGN_TYPES.map(t => ({ value: t.value, label: t.label }))}
                />
              </Grid>
              <Grid item xs={4}>
                <LookupSelect
                  category="CampaignStatus"
                  name="status"
                  value={formData.status}
                  onChange={handleSelectChange}
                  label="Status"
                  fallback={CAMPAIGN_STATUSES.map(s => ({ value: s.value, label: s.label }))}
                />
              </Grid>
              <Grid item xs={4}>
                <LookupSelect
                  category="Priority"
                  name="priority"
                  value={formData.priority}
                  onChange={handleSelectChange}
                  label="Priority"
                  fallback={CAMPAIGN_PRIORITIES.map(p => ({ value: p.value, label: p.label }))}
                />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Start Date" name="startDate" type="date" value={formData.startDate} onChange={handleInputChange} InputLabelProps={{ shrink: true }} required />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="End Date" name="endDate" type="date" value={formData.endDate} onChange={handleInputChange} InputLabelProps={{ shrink: true }} />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="Budget ($)" name="budget" type="number" value={formData.budget} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="Actual Spend ($)" name="actualSpend" type="number" value={formData.actualSpend} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="Target Audience" name="targetAudience" type="number" value={formData.targetAudience} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Description" name="description" value={formData.description} onChange={handleInputChange} multiline rows={3} />
              </Grid>
            </Grid>
          </TabPanel>

          <TabPanel value={dialogTab} index={1}>
            <Grid container spacing={2}>
              <Grid item xs={4}>
                <TextField fullWidth label="Impressions" name="impressions" type="number" value={formData.impressions} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="Clicks" name="clicks" type="number" value={formData.clicks} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={4}>
                <TextField 
                  fullWidth 
                  label="CTR (%)" 
                  value={formData.impressions > 0 ? ((formData.clicks / formData.impressions) * 100).toFixed(2) : '0'}
                  InputProps={{ readOnly: true }}
                />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="Conversions" name="conversions" type="number" value={formData.conversions} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="Leads Generated" name="leadsGenerated" type="number" value={formData.leadsGenerated} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="Revenue ($)" name="revenue" type="number" value={formData.revenue} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={12}>
                <Typography variant="h6" sx={{ mt: 2 }}>
                  Calculated ROI: {formData.actualSpend > 0 ? ((formData.revenue - formData.actualSpend) / formData.actualSpend * 100).toFixed(1) : 0}%
                </Typography>
              </Grid>
            </Grid>
          </TabPanel>

          <TabPanel value={dialogTab} index={2}>
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <TextField fullWidth label="Emails Sent" name="emailsSent" type="number" value={formData.emailsSent} onChange={handleInputChange} InputProps={{ startAdornment: <EmailIcon sx={{ mr: 1, color: '#666' }} /> }} />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Emails Opened" name="emailsOpened" type="number" value={formData.emailsOpened} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={4}>
                <TextField 
                  fullWidth 
                  label="Open Rate (%)" 
                  value={formData.emailsSent > 0 ? ((formData.emailsOpened / formData.emailsSent) * 100).toFixed(2) : '0'}
                  InputProps={{ readOnly: true }}
                />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="Unsubscribes" name="unsubscribes" type="number" value={formData.unsubscribes} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="Bounces" name="bounces" type="number" value={formData.bounces} onChange={handleInputChange} />
              </Grid>
            </Grid>
          </TabPanel>

          <TabPanel value={dialogTab} index={3}>
            <Grid container spacing={2}>
              <Grid item xs={4}>
                <TextField fullWidth label="Social Reach" name="socialReach" type="number" value={formData.socialReach} onChange={handleInputChange} InputProps={{ startAdornment: <ShareIcon sx={{ mr: 1, color: '#666' }} /> }} />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="Social Engagement" name="socialEngagement" type="number" value={formData.socialEngagement} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="Social Shares" name="socialShares" type="number" value={formData.socialShares} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={12}>
                <FormControlLabel
                  control={<Checkbox name="isABTest" checked={formData.isABTest} onChange={handleInputChange} />}
                  label="This is an A/B Test Campaign"
                />
              </Grid>
              {formData.isABTest && (
                <>
                  <Grid item xs={6}>
                    <TextField fullWidth label="A/B Test Variants" name="abTestVariants" value={formData.abTestVariants} onChange={handleInputChange} placeholder="Variant A, Variant B" />
                  </Grid>
                  <Grid item xs={6}>
                    <TextField fullWidth label="Winning Variant" name="winningVariant" value={formData.winningVariant} onChange={handleInputChange} />
                  </Grid>
                </>
              )}
            </Grid>
          </TabPanel>

          <TabPanel value={dialogTab} index={4}>
            <Grid container spacing={2}>
              <Grid item xs={4}>
                <TextField fullWidth label="UTM Source" name="utmSource" value={formData.utmSource} onChange={handleInputChange} placeholder="google" />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="UTM Medium" name="utmMedium" value={formData.utmMedium} onChange={handleInputChange} placeholder="cpc" />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="UTM Campaign" name="utmCampaign" value={formData.utmCampaign} onChange={handleInputChange} placeholder="spring_sale" />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Tags (comma-separated)" name="tags" value={formData.tags} onChange={handleInputChange} placeholder="seasonal, promotion, q1" />
              </Grid>
            </Grid>
          </TabPanel>

          <TabPanel value={dialogTab} index={5}>
            {editingId ? (
              <NotesTab
                entityType="Campaign"
                entityId={editingId}
                entityName={formData.name || 'Campaign'}
              />
            ) : (
              <Alert severity="info" sx={{ mt: 2 }}>
                Please save the campaign first to add notes.
              </Alert>
            )}
          </TabPanel>
          <DialogError error={dialogApi.error} onRetry={() => dialogApi.clearError()} />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog} disabled={dialogApi.loading}>Cancel</Button>
          <ActionButton onClick={handleSaveCampaign} variant="contained" loading={dialogApi.loading} sx={{ backgroundColor: '#6750A4' }}>
            {editingId ? 'Update' : 'Create'}
          </ActionButton>
        </DialogActions>
      </Dialog>

      {/* Bulk Update Dialog */}
      <Dialog open={bulkDialogOpen} onClose={() => !bulkApi.loading && setBulkDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Bulk Update {selectedIds.length} Campaigns</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Only fields with values will be updated. Leave fields empty to keep existing values.
          </Typography>
          
          <FormControl fullWidth margin="normal">
            <InputLabel>Status</InputLabel>
            <Select
              value={bulkFormData.status}
              onChange={(e: SelectChangeEvent) => setBulkFormData(prev => ({ ...prev, status: e.target.value }))}
              label="Status"
            >
              <MenuItem value="">-- No Change --</MenuItem>
              {CAMPAIGN_STATUSES.map(s => (
                <MenuItem key={s.value} value={s.value.toString()}>{s.label}</MenuItem>
              ))}
            </Select>
          </FormControl>
          
          <FormControl fullWidth margin="normal">
            <InputLabel>Priority</InputLabel>
            <Select
              value={bulkFormData.priority}
              onChange={(e: SelectChangeEvent) => setBulkFormData(prev => ({ ...prev, priority: e.target.value }))}
              label="Priority"
            >
              <MenuItem value="">-- No Change --</MenuItem>
              {CAMPAIGN_PRIORITIES.map(p => (
                <MenuItem key={p.value} value={p.value.toString()}>{p.label}</MenuItem>
              ))}
            </Select>
          </FormControl>
          
          <FormControl fullWidth margin="normal">
            <InputLabel>Campaign Type</InputLabel>
            <Select
              value={bulkFormData.campaignType}
              onChange={(e: SelectChangeEvent) => setBulkFormData(prev => ({ ...prev, campaignType: e.target.value }))}
              label="Campaign Type"
            >
              <MenuItem value="">-- No Change --</MenuItem>
              {CAMPAIGN_TYPES.map(t => (
                <MenuItem key={t.value} value={t.value.toString()}>{t.label}</MenuItem>
              ))}
            </Select>
          </FormControl>
          
          <DialogError error={bulkApi.error} onRetry={() => bulkApi.clearError()} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setBulkDialogOpen(false)} disabled={bulkApi.loading}>Cancel</Button>
          <ActionButton
            onClick={handleBulkUpdate}
            loading={bulkApi.loading}
            variant="contained"
            color="primary"
          >
            Update Selected
          </ActionButton>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default CampaignsPage;

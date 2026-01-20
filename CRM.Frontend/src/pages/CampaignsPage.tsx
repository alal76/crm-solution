import { useState, useEffect } from 'react';
import {
  Box, Card, CardContent, Typography, Button, Table, TableBody, TableCell, TableHead,
  TableRow, Dialog, DialogTitle, DialogContent, DialogActions, Alert, CircularProgress,
  TextField, Container, FormControl, InputLabel, Select, MenuItem, Chip, Tabs, Tab,
  Grid, IconButton, Tooltip, FormControlLabel, Checkbox, LinearProgress,
  SelectChangeEvent
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, 
  Campaign as CampaignIcon, TrendingUp as TrendingUpIcon,
  Email as EmailIcon, Share as ShareIcon
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';

// Enums matching backend
const CAMPAIGN_TYPES = [
  { value: 0, label: 'Email', icon: '📧' },
  { value: 1, label: 'Social Media', icon: '📱' },
  { value: 2, label: 'Paid Search', icon: '🔍' },
  { value: 3, label: 'Display Ads', icon: '🖼️' },
  { value: 4, label: 'Content Marketing', icon: '📝' },
  { value: 5, label: 'SEO', icon: '🔎' },
  { value: 6, label: 'Events', icon: '🎪' },
  { value: 7, label: 'Webinar', icon: '💻' },
  { value: 8, label: 'Trade Show', icon: '🏢' },
  { value: 9, label: 'Direct Mail', icon: '✉️' },
  { value: 10, label: 'Referral', icon: '👥' },
  { value: 11, label: 'Partner', icon: '🤝' },
  { value: 12, label: 'PR', icon: '📰' },
  { value: 13, label: 'Video', icon: '🎬' },
  { value: 14, label: 'Podcast', icon: '🎙️' },
  { value: 15, label: 'Other', icon: '📋' },
];

const CAMPAIGN_STATUSES = [
  { value: 0, label: 'Draft', color: '#9e9e9e' },
  { value: 1, label: 'Scheduled', color: '#2196f3' },
  { value: 2, label: 'Active', color: '#4caf50' },
  { value: 3, label: 'Paused', color: '#ff9800' },
  { value: 4, label: 'Completed', color: '#9c27b0' },
  { value: 5, label: 'Cancelled', color: '#f44336' },
  { value: 6, label: 'Archived', color: '#607d8b' },
];

const CAMPAIGN_PRIORITIES = [
  { value: 0, label: 'Low', color: '#9e9e9e' },
  { value: 1, label: 'Medium', color: '#2196f3' },
  { value: 2, label: 'High', color: '#ff9800' },
  { value: 3, label: 'Critical', color: '#f44336' },
];

interface Campaign {
  id: number;
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
  createdAt: string;
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

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div role="tabpanel" hidden={value !== index} {...other}>
      {value === index && <Box sx={{ pt: 2 }}>{children}</Box>}
    </div>
  );
}

function CampaignsPage() {
  const [campaigns, setCampaigns] = useState<Campaign[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  
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
      setError('Please fill in required fields (Name, Start Date)');
      return;
    }
    try {
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
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save campaign');
    }
  };

  const handleDeleteCampaign = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this campaign?')) {
      try {
        await apiClient.delete(`/campaigns/${id}`);
        setSuccessMessage('Campaign deleted successfully');
        fetchCampaigns();
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete campaign');
      }
    }
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
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenDialog()} sx={{ backgroundColor: '#6750A4' }}>
            Add Campaign
          </Button>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

        <Card>
          <CardContent sx={{ p: 0, overflowX: 'auto' }}>
            <Table>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
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
                    <TableRow key={campaign.id} hover>
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
          </Tabs>
        </Box>
        <DialogContent sx={{ pt: 2, minHeight: 400 }}>
          <TabPanel value={dialogTab} index={0}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField fullWidth label="Campaign Name" name="name" value={formData.name} onChange={handleInputChange} required />
              </Grid>
              <Grid item xs={4}>
                <FormControl fullWidth>
                  <InputLabel>Campaign Type</InputLabel>
                  <Select name="campaignType" value={formData.campaignType} onChange={handleSelectChange} label="Campaign Type">
                    {CAMPAIGN_TYPES.map(t => <MenuItem key={t.value} value={t.value}>{t.icon} {t.label}</MenuItem>)}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={4}>
                <FormControl fullWidth>
                  <InputLabel>Status</InputLabel>
                  <Select name="status" value={formData.status} onChange={handleSelectChange} label="Status">
                    {CAMPAIGN_STATUSES.map(s => (
                      <MenuItem key={s.value} value={s.value}>
                        <Chip label={s.label} size="small" sx={{ backgroundColor: s.color, color: 'white' }} />
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={4}>
                <FormControl fullWidth>
                  <InputLabel>Priority</InputLabel>
                  <Select name="priority" value={formData.priority} onChange={handleSelectChange} label="Priority">
                    {CAMPAIGN_PRIORITIES.map(p => (
                      <MenuItem key={p.value} value={p.value}>
                        <Chip label={p.label} size="small" sx={{ backgroundColor: p.color, color: 'white' }} />
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
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
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSaveCampaign} variant="contained" sx={{ backgroundColor: '#6750A4' }}>
            {editingId ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default CampaignsPage;

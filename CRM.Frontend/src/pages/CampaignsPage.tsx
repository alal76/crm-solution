import { useState, useEffect } from 'react';
import { DialogHeader, RelatedEntitiesPanel, EnhancedEmptyState } from '../components/common';
import SegmentBuilder from '../components/marketing/SegmentBuilder';
import AbTestConfigPanel, { defaultAbTestConfig } from '../components/marketing/AbTestConfig';
import { SegmentConfig, AbTestConfig as AbTestConfigType } from '../types/marketing';
import FilterAltIcon from '@mui/icons-material/FilterAlt';
import ScienceIcon from '@mui/icons-material/Science';
import {
  Box, Card, CardContent, Typography, Button, Table, TableBody, TableCell, TableHead,
  TableRow, TablePagination, Dialog, DialogTitle, DialogContent, DialogActions, Alert, CircularProgress,
  Container, FormControl, InputLabel, Select, MenuItem, Chip,
  IconButton, Tooltip, Checkbox, LinearProgress,
  SelectChangeEvent, Paper, Collapse, Stack
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, 
  Campaign as CampaignIcon, TrendingUp as TrendingUpIcon,
  Close as CloseIcon,
  Note as NoteIcon, Link as LinkIcon
} from '@mui/icons-material';
import { DialogError, ActionButton } from '../components/common';
import { useApiState } from '../hooks/useApiState';
import { useProfile } from '../contexts/ProfileContext';
import apiClient from '../services/apiClient';
import logger from '../services/logger';
import { BaseEntity } from '../types';
import logo from '../assets/logo.png';
import ImportExportButtons from '../components/ImportExportButtons';
import NotesTab from '../components/NotesTab';
import DynamicEntityForm, { ExtraTab } from '../components/DynamicEntityForm';
import {
  CAMPAIGN_STATUS_OPTIONS,
  CAMPAIGN_TYPE_OPTIONS,
  PRIORITY_OPTIONS,
  getLabelByValue,
  getColorByValue
} from '../utils/constants';
import { usePagination } from '../hooks/usePagination';

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
  // Budget & Performance Metrics
  dailyBudget: number;
  monthlyBudget: number;
  expectedRevenue: number;
  costPerLead: number;
  costPerAcquisition: number;
  mqlsGenerated: number;
  sqlsGenerated: number;
  opportunitiesCreated: number;
  dealsWon: number;
  utmContent: string;
  utmTerm: string;
  // Scheduling
  actualStartDate: string;
  actualEndDate: string;
  objectiveType: number;
  // Audience
  audienceType: number;
  // Email metrics
  emailsDelivered: number;
  deliveryRate: number;
  emailClicks: number;
  bounceRate: number;
  // Digital
  reach: number;
  clickThroughRate: number;
  landingPageVisits: number;
  // Event
  attendance: number;
  noShows: number;
  eventCapacity: number;
  eventLocation: string;
  eventDateTime: string;
  // Admin
  costCenter: string;
  parentCampaignId: number;
  externalId: string;
  abTestMetric: string;
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
  
  // MKT-007: Segment config
  const [segmentConfig, setSegmentConfig] = useState<SegmentConfig | undefined>(undefined);
  // MKT-008: A/B test config
  const [abTestConfig, setAbTestConfig] = useState<AbTestConfigType>(defaultAbTestConfig());

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
    dailyBudget: 0, monthlyBudget: 0, expectedRevenue: 0, costPerLead: 0, costPerAcquisition: 0,
    mqlsGenerated: 0, sqlsGenerated: 0, opportunitiesCreated: 0, dealsWon: 0,
    utmContent: '', utmTerm: '',
    actualStartDate: '', actualEndDate: '', objectiveType: 0,
    audienceType: 0,
    emailsDelivered: 0, deliveryRate: 0, emailClicks: 0, bounceRate: 0,
    reach: 0, clickThroughRate: 0, landingPageVisits: 0,
    attendance: 0, noShows: 0, eventCapacity: 0, eventLocation: '', eventDateTime: '',
    costCenter: '', parentCampaignId: 0, externalId: '', abTestMetric: '',
  };
  const [formData, setFormData] = useState<CampaignForm>(emptyForm);

  const { paginatedData: paginatedCampaigns, page, pageSize, handlePageChange, handlePageSizeChange, pageSizeOptions } = usePagination(campaigns, { defaultPageSize: 25 });

  useEffect(() => { fetchCampaigns(); }, []);

  const fetchCampaigns = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/campaigns');
      setCampaigns(response.data);
      setError(null);
    } catch (err: unknown) {
      setError((err as any).response?.data?.message || 'Failed to fetch campaigns');
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
        dailyBudget: (campaign as any).dailyBudget || 0, monthlyBudget: (campaign as any).monthlyBudget || 0, expectedRevenue: (campaign as any).expectedRevenue || 0, costPerLead: (campaign as any).costPerLead || 0, costPerAcquisition: (campaign as any).costPerAcquisition || 0,
        mqlsGenerated: (campaign as any).mqlsGenerated || 0, sqlsGenerated: (campaign as any).sqlsGenerated || 0, opportunitiesCreated: (campaign as any).opportunitiesCreated || 0, dealsWon: (campaign as any).dealsWon || 0,
        utmContent: (campaign as any).utmContent || '', utmTerm: (campaign as any).utmTerm || '',
        actualStartDate: (campaign as any).actualStartDate?.split('T')[0] || '',
        actualEndDate: (campaign as any).actualEndDate?.split('T')[0] || '',
        objectiveType: (campaign as any).objectiveType || 0,
        audienceType: (campaign as any).audienceType || 0,
        emailsDelivered: (campaign as any).emailsDelivered || 0,
        deliveryRate: (campaign as any).deliveryRate || 0,
        emailClicks: (campaign as any).emailClicks || 0,
        bounceRate: (campaign as any).bounceRate || 0,
        reach: (campaign as any).reach || 0,
        clickThroughRate: (campaign as any).clickThroughRate || 0,
        landingPageVisits: (campaign as any).landingPageVisits || 0,
        attendance: (campaign as any).attendance || 0,
        noShows: (campaign as any).noShows || 0,
        eventCapacity: (campaign as any).eventCapacity || 0,
        eventLocation: (campaign as any).eventLocation || '',
        eventDateTime: (campaign as any).eventDateTime || '',
        costCenter: (campaign as any).costCenter || '',
        parentCampaignId: (campaign as any).parentCampaignId || 0,
        externalId: (campaign as any).externalId || '',
        abTestMetric: (campaign as any).abTestMetric || '',
      });
    } else {
      setEditingId(null);
      setFormData(emptyForm);
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => { 
    setOpenDialog(false); 
    setEditingId(null); 
    dialogApi.clearError();
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    const checked = (e.target as HTMLInputElement).checked;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : type === 'number' ? Number.parseFloat(value) || 0 : value,
    }));
  };

  const handleSelectChange = (e: SelectChangeEvent<string | number>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSaveCampaign = async () => {
    // Required field validation
    if (!formData.name.trim()) {
      dialogApi.setError('Campaign name is required');
      return;
    }
    if (!formData.startDate) {
      dialogApi.setError('Start date is required');
      return;
    }
    
    // Date range validation
    if (formData.endDate && formData.startDate && formData.endDate < formData.startDate) {
      dialogApi.setError('End date cannot be before start date');
      return;
    }
    
    // Budget validation
    if (formData.budget < 0) {
      dialogApi.setError('Budget cannot be negative');
      return;
    }
    if (formData.actualSpend < 0) {
      dialogApi.setError('Actual spend cannot be negative');
      return;
    }
    
    // Metrics validation (non-negative)
    if (formData.impressions < 0 || formData.clicks < 0 || formData.conversions < 0 ||
        formData.leadsGenerated < 0 || formData.revenue < 0 || formData.emailsSent < 0 ||
        formData.emailsOpened < 0 || formData.socialReach < 0) {
      dialogApi.setError('Metrics cannot be negative');
      return;
    }
    
    // Logical validation: clicks <= impressions, emailsOpened <= emailsSent
    if (formData.clicks > formData.impressions && formData.impressions > 0) {
      dialogApi.setError('Clicks cannot exceed impressions');
      return;
    }
    if (formData.emailsOpened > formData.emailsSent && formData.emailsSent > 0) {
      dialogApi.setError('Emails opened cannot exceed emails sent');
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
    // Validate at least one field is selected for update
    if (!bulkFormData.status && !bulkFormData.priority && !bulkFormData.campaignType) {
      bulkApi.setError('Please select at least one field to update');
      return;
    }
    
    await bulkApi.execute(async () => {
      const updatePayload: any = {};
      if (bulkFormData.status) updatePayload.status = Number.parseInt(bulkFormData.status);
      if (bulkFormData.priority) updatePayload.priority = Number.parseInt(bulkFormData.priority);
      if (bulkFormData.campaignType) updatePayload.campaignType = Number.parseInt(bulkFormData.campaignType);
      
      // Send same payload to all selected campaigns
      await Promise.all(selectedIds.map(id => 
        apiClient.put(`/campaigns/${id}`, updatePayload)
      ));
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
                {paginatedCampaigns.map((campaign) => {
                  const status = getStatus(campaign.status);
                  const type = getType(campaign.campaignType);
                  const priority = getPriority(campaign.priority);
                  const roi = Number.parseFloat(calculateROI(campaign));
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
            <TablePagination
              component="div"
              count={campaigns.length}
              page={page}
              onPageChange={handlePageChange}
              rowsPerPage={pageSize}
              onRowsPerPageChange={handlePageSizeChange}
              rowsPerPageOptions={pageSizeOptions}
            />
            {campaigns.length === 0 && (
              <EnhancedEmptyState
                illustration="campaigns"
                title="No campaigns yet"
                description="Create your first marketing campaign to start tracking performance"
                variant="no-data"
                primaryActionLabel="Create Campaign"
                onPrimaryAction={() => handleOpenDialog()}
              />
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Enhanced Add/Edit Campaign Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogHeader
          mode={editingId ? 'edit' : 'create'}
          entityType="campaign"
          entityName={editingId ? formData.name : undefined}
          entityId={editingId || undefined}
          onClose={handleCloseDialog}
          subtitle={editingId && formData.startDate ? `${formData.startDate} - ${formData.endDate || 'Ongoing'}` : undefined}
          status={editingId && formData.status ? (CAMPAIGN_STATUSES.find(s => s.value === formData.status)?.label || undefined) : undefined}
          statusColor={editingId && formData.status ? (
            formData.status === 2 ? 'success' :
            formData.status === 1 ? 'info' :
            formData.status === 3 ? 'warning' :
            'default'
          ) : undefined}
        />
        <DialogContent sx={{ pt: 0, minHeight: 400 }}>
          <DialogError error={dialogApi.error} onRetry={() => dialogApi.clearError()} />

          <DynamicEntityForm
            moduleName="Campaigns"
            formData={formData}
            onChange={handleInputChange}
            onSelectChange={(e: any) => setFormData(prev => ({ ...prev, [e.target.name]: e.target.value }))}
            setFormData={setFormData}
            activeTab={dialogTab}
            editingId={editingId}
            onTabChange={setDialogTab}
            excludeFields={['tags', 'customFields', 'recipients']}
            extraTabs={[
              {
                index: 100,
                name: 'Related',
                icon: <LinkIcon fontSize="small" />,
                editOnly: true,
                render: () => (
                  <RelatedEntitiesPanel
                    entityType="campaigns"
                    entityId={editingId!}
                    showRelated={['contacts', 'opportunities', 'activities']}
                    onEntityClick={(type, id) => {
                      handleCloseDialog();
                      logger.debug(`Navigate to ${type} ${id}`);
                    }}
                  />
                ),
              },
              {
                index: 101,
                name: 'Segment',
                icon: <FilterAltIcon fontSize="small" />,
                editOnly: true,
                render: () => editingId ? (
                  <SegmentBuilder
                    campaignId={editingId}
                    initialConfig={segmentConfig}
                    onSaved={setSegmentConfig}
                  />
                ) : (
                  <Alert severity="info" sx={{ mt: 2 }}>Save the campaign first to configure recipient segmentation.</Alert>
                ),
              },
              {
                index: 102,
                name: 'A/B Test',
                icon: <ScienceIcon fontSize="small" />,
                render: () => (
                  <AbTestConfigPanel
                    value={abTestConfig}
                    onChange={setAbTestConfig}
                  />
                ),
              },
              {
                index: 103,
                name: 'Notes',
                icon: <NoteIcon fontSize="small" />,
                render: () => editingId ? (
                  <NotesTab
                    entityType="Campaign"
                    entityId={editingId}
                    entityName={formData.name || 'Campaign'}
                  />
                ) : (
                  <Alert severity="info" sx={{ mt: 2 }}>
                    Please save the campaign first to add notes.
                  </Alert>
                ),
              },
            ]}
          />
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

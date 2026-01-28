import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControlLabel,
  Switch,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Grid,
  Chip,
  IconButton,
  Alert,
  CircularProgress,
  Tabs,
  Tab,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  ExpandMore as ExpandMoreIcon,
  Security as SecurityIcon,
  AdminPanelSettings as AdminIcon,
} from '@mui/icons-material';
import { getApiEndpoint } from '../../config/ports';
import { DialogError, ActionButton } from '../common';
import { useApiState } from '../../hooks/useApiState';

interface GroupPermissions {
  isSystemAdmin: boolean;
  canAccessDashboard: boolean;
  canAccessCustomers: boolean;
  canAccessContacts: boolean;
  canAccessLeads: boolean;
  canAccessOpportunities: boolean;
  canAccessProducts: boolean;
  canAccessServices: boolean;
  canAccessCampaigns: boolean;
  canAccessQuotes: boolean;
  canAccessTasks: boolean;
  canAccessActivities: boolean;
  canAccessNotes: boolean;
  canAccessWorkflows: boolean;
  canAccessServiceRequests: boolean;
  canAccessReports: boolean;
  canAccessSettings: boolean;
  canAccessUserManagement: boolean;
  canCreateCustomers: boolean;
  canEditCustomers: boolean;
  canDeleteCustomers: boolean;
  canViewAllCustomers: boolean;
  canCreateContacts: boolean;
  canEditContacts: boolean;
  canDeleteContacts: boolean;
  canCreateLeads: boolean;
  canEditLeads: boolean;
  canDeleteLeads: boolean;
  canConvertLeads: boolean;
  canCreateOpportunities: boolean;
  canEditOpportunities: boolean;
  canDeleteOpportunities: boolean;
  canCloseOpportunities: boolean;
  canCreateProducts: boolean;
  canEditProducts: boolean;
  canDeleteProducts: boolean;
  canManagePricing: boolean;
  canCreateCampaigns: boolean;
  canEditCampaigns: boolean;
  canDeleteCampaigns: boolean;
  canLaunchCampaigns: boolean;
  canCreateQuotes: boolean;
  canEditQuotes: boolean;
  canDeleteQuotes: boolean;
  canApproveQuotes: boolean;
  canCreateTasks: boolean;
  canEditTasks: boolean;
  canDeleteTasks: boolean;
  canAssignTasks: boolean;
  canCreateWorkflows: boolean;
  canEditWorkflows: boolean;
  canDeleteWorkflows: boolean;
  canActivateWorkflows: boolean;
  dataAccessScope: string;
  canExportData: boolean;
  canImportData: boolean;
  canBulkEdit: boolean;
  canBulkDelete: boolean;
}

interface UserGroup extends GroupPermissions {
  id: number;
  name: string;
  description: string;
  isActive: boolean;
  isDefault: boolean;
  displayOrder: number;
  headerColor: string;
  memberCount: number;
  createdAt: string;
}

const defaultPermissions: GroupPermissions = {
  isSystemAdmin: false,
  canAccessDashboard: true,
  canAccessCustomers: false,
  canAccessContacts: false,
  canAccessLeads: false,
  canAccessOpportunities: false,
  canAccessProducts: false,
  canAccessServices: false,
  canAccessCampaigns: false,
  canAccessQuotes: false,
  canAccessTasks: false,
  canAccessActivities: false,
  canAccessNotes: false,
  canAccessWorkflows: false,
  canAccessServiceRequests: false,
  canAccessReports: false,
  canAccessSettings: false,
  canAccessUserManagement: false,
  canCreateCustomers: false,
  canEditCustomers: false,
  canDeleteCustomers: false,
  canViewAllCustomers: false,
  canCreateContacts: false,
  canEditContacts: false,
  canDeleteContacts: false,
  canCreateLeads: false,
  canEditLeads: false,
  canDeleteLeads: false,
  canConvertLeads: false,
  canCreateOpportunities: false,
  canEditOpportunities: false,
  canDeleteOpportunities: false,
  canCloseOpportunities: false,
  canCreateProducts: false,
  canEditProducts: false,
  canDeleteProducts: false,
  canManagePricing: false,
  canCreateCampaigns: false,
  canEditCampaigns: false,
  canDeleteCampaigns: false,
  canLaunchCampaigns: false,
  canCreateQuotes: false,
  canEditQuotes: false,
  canDeleteQuotes: false,
  canApproveQuotes: false,
  canCreateTasks: false,
  canEditTasks: false,
  canDeleteTasks: false,
  canAssignTasks: false,
  canCreateWorkflows: false,
  canEditWorkflows: false,
  canDeleteWorkflows: false,
  canActivateWorkflows: false,
  dataAccessScope: 'own',
  canExportData: false,
  canImportData: false,
  canBulkEdit: false,
  canBulkDelete: false,
};

function GroupManagementTab() {
  const [groups, setGroups] = useState<UserGroup[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  
  // API state for dialog error handling
  const dialogApi = useApiState();
  const [dialogTab, setDialogTab] = useState(0);
  const [editingGroup, setEditingGroup] = useState<UserGroup | null>(null);
  const [formData, setFormData] = useState<Partial<UserGroup>>({
    name: '',
    description: '',
    isActive: true,
    isDefault: false,
    displayOrder: 0,
    headerColor: '#6750A4',
    ...defaultPermissions,
  });
  
  const getToken = () => localStorage.getItem('accessToken');

  const fetchGroups = async () => {
    const token = getToken();
    try {
      setLoading(true);
      const response = await fetch(getApiEndpoint('/usergroups'), {
        headers: { 'Authorization': `Bearer ${token}` },
      });
      
      if (!response.ok) throw new Error('Failed to fetch groups');
      
      const data = await response.json();
      setGroups(data);
      setError(null);
    } catch (err) {
      setError('Failed to load groups');
      console.error('Error fetching groups:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchGroups();
  }, []);

  const handleOpenDialog = (group?: UserGroup) => {
    if (group) {
      setEditingGroup(group);
      setFormData(group);
    } else {
      setEditingGroup(null);
      setFormData({
        name: '',
        description: '',
        isActive: true,
        isDefault: false,
        displayOrder: 0,
        headerColor: '#6750A4',
        ...defaultPermissions,
      });
    }
    setDialogTab(0);
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setEditingGroup(null);
    dialogApi.clearError();
  };

  const handleSaveGroup = async () => {
    await dialogApi.execute(async () => {
      const token = getToken();
      const url = editingGroup ? getApiEndpoint(`/usergroups/${editingGroup.id}`) : getApiEndpoint('/usergroups');
      const method = editingGroup ? 'PUT' : 'POST';
      
      const response = await fetch(url, {
        method,
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(formData),
      });
      
      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to save group');
      }
      
      setSuccess(editingGroup ? 'Group updated successfully' : 'Group created successfully');
      handleCloseDialog();
      fetchGroups();
      setTimeout(() => setSuccess(null), 3000);
    });
  };

  const handleDeleteGroup = async (groupId: number) => {
    if (!window.confirm('Are you sure you want to delete this group?')) return;
    
    await dialogApi.execute(async () => {
      const token = getToken();
      const response = await fetch(getApiEndpoint(`/usergroups/${groupId}`), {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` },
      });
      
      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to delete group');
      }
      
      setSuccess('Group deleted successfully');
      fetchGroups();
      setTimeout(() => setSuccess(null), 3000);
    });
  };

  const handlePermissionChange = (key: keyof GroupPermissions, value: boolean | string) => {
    setFormData(prev => ({ ...prev, [key]: value }));
  };

  const setAllMenuPermissions = (value: boolean) => {
    const menuPerms: (keyof GroupPermissions)[] = [
      'canAccessDashboard', 'canAccessCustomers', 'canAccessContacts', 'canAccessLeads',
      'canAccessOpportunities', 'canAccessProducts', 'canAccessServices', 'canAccessCampaigns',
      'canAccessQuotes', 'canAccessTasks', 'canAccessActivities', 'canAccessNotes',
      'canAccessWorkflows', 'canAccessServiceRequests', 'canAccessReports', 'canAccessSettings', 'canAccessUserManagement'
    ];
    const updates: Record<string, boolean> = {};
    menuPerms.forEach(p => updates[p] = value);
    setFormData(prev => ({ ...prev, ...updates }));
  };

  const renderPermissionSwitch = (key: keyof GroupPermissions, label: string) => (
    <FormControlLabel
      control={
        <Switch
          checked={!!formData[key]}
          onChange={(e) => handlePermissionChange(key, e.target.checked)}
          disabled={formData.isSystemAdmin}
        />
      }
      label={label}
      sx={{ width: '100%', ml: 0 }}
    />
  );

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}

      <Card>
        <CardContent>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h6" sx={{ fontWeight: 600 }}>Group Management</Typography>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={() => handleOpenDialog()}
              sx={{ backgroundColor: '#6750A4', textTransform: 'none' }}
            >
              Create Group
            </Button>
          </Box>
          
          <Table>
            <TableHead>
              <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                <TableCell><strong>Group Name</strong></TableCell>
                <TableCell><strong>Description</strong></TableCell>
                <TableCell><strong>Header Color</strong></TableCell>
                <TableCell><strong>Type</strong></TableCell>
                <TableCell><strong>Members</strong></TableCell>
                <TableCell><strong>Status</strong></TableCell>
                <TableCell><strong>Actions</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {groups.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} sx={{ textAlign: 'center', py: 4 }}>
                    <Typography color="textSecondary">No groups created yet</Typography>
                  </TableCell>
                </TableRow>
              ) : (
                groups.map((group) => (
                  <TableRow key={group.id}>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        {group.isSystemAdmin && <AdminIcon color="primary" fontSize="small" />}
                        {group.name}
                      </Box>
                    </TableCell>
                    <TableCell>{group.description || '-'}</TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Box
                          sx={{
                            width: 24,
                            height: 24,
                            backgroundColor: group.headerColor || '#6750A4',
                            borderRadius: 0.5,
                            border: '1px solid #ddd',
                          }}
                        />
                        <Typography variant="caption">{group.headerColor || '#6750A4'}</Typography>
                      </Box>
                    </TableCell>
                    <TableCell>
                      {group.isSystemAdmin ? (
                        <Chip label="System Admin" color="primary" size="small" />
                      ) : (
                        <Chip label="Standard" variant="outlined" size="small" />
                      )}
                    </TableCell>
                    <TableCell>{group.memberCount}</TableCell>
                    <TableCell>
                      <Chip
                        label={group.isActive ? 'Active' : 'Inactive'}
                        color={group.isActive ? 'success' : 'default'}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>
                      <IconButton size="small" onClick={() => handleOpenDialog(group)}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                      <IconButton size="small" onClick={() => handleDeleteGroup(group.id)} color="error">
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Group Edit Dialog */}
      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle>
          {editingGroup ? 'Edit Group' : 'Create New Group'}
        </DialogTitle>
        <DialogContent>
          <Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)} sx={{ mb: 2 }}>
            <Tab label="Basic Info" />
            <Tab label="Menu Access" />
            <Tab label="Entity Permissions" />
            <Tab label="Data Access" />
          </Tabs>

          {dialogTab === 0 && (
            <Box sx={{ pt: 2 }}>
              <Grid container spacing={2}>
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Group Name"
                    value={formData.name || ''}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    required
                  />
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Description"
                    value={formData.description || ''}
                    onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    multiline
                    rows={2}
                  />
                </Grid>
                <Grid item xs={6}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={formData.isActive}
                        onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                      />
                    }
                    label="Active"
                  />
                </Grid>
                <Grid item xs={6}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={formData.isDefault}
                        onChange={(e) => setFormData({ ...formData, isDefault: e.target.checked })}
                      />
                    }
                    label="Default Group (for new users)"
                  />
                </Grid>
                <Grid item xs={12}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, p: 2, backgroundColor: '#f5f5f5', borderRadius: 1 }}>
                    <Box>
                      <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
                        Header Color
                      </Typography>
                      <Typography variant="caption" sx={{ color: '#79747E' }}>
                        Navigation bar color for users in this group (when Group Header Color is enabled)
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, ml: 'auto' }}>
                      <input
                        type="color"
                        value={formData.headerColor || '#6750A4'}
                        onChange={(e) => setFormData({ ...formData, headerColor: e.target.value })}
                        style={{ 
                          width: 50, 
                          height: 35, 
                          border: 'none', 
                          borderRadius: 4,
                          cursor: 'pointer'
                        }}
                      />
                      <TextField
                        size="small"
                        value={formData.headerColor || '#6750A4'}
                        onChange={(e) => setFormData({ ...formData, headerColor: e.target.value })}
                        sx={{ width: 100 }}
                      />
                    </Box>
                  </Box>
                </Grid>
                <Grid item xs={12}>
                  <Alert severity="info" icon={<SecurityIcon />}>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={formData.isSystemAdmin}
                          onChange={(e) => setFormData({ ...formData, isSystemAdmin: e.target.checked })}
                          color="primary"
                        />
                      }
                      label={
                        <Box>
                          <Typography variant="body2" fontWeight={600}>System Administrator</Typography>
                          <Typography variant="caption" color="textSecondary">
                            Members have full access to all features and bypass all permission checks
                          </Typography>
                        </Box>
                      }
                    />
                  </Alert>
                </Grid>
              </Grid>
            </Box>
          )}

          {dialogTab === 1 && (
            <Box sx={{ pt: 2 }}>
              {formData.isSystemAdmin && (
                <Alert severity="info" sx={{ mb: 2 }}>
                  System Admin groups have access to all menus by default.
                </Alert>
              )}
              <Box sx={{ mb: 2, display: 'flex', gap: 1 }}>
                <Button size="small" onClick={() => setAllMenuPermissions(true)} disabled={formData.isSystemAdmin}>
                  Select All
                </Button>
                <Button size="small" onClick={() => setAllMenuPermissions(false)} disabled={formData.isSystemAdmin}>
                  Clear All
                </Button>
              </Box>
              <Grid container spacing={1}>
                <Grid item xs={6} sm={4}>{renderPermissionSwitch('canAccessDashboard', 'Dashboard')}</Grid>
                <Grid item xs={6} sm={4}>{renderPermissionSwitch('canAccessCustomers', 'Customers')}</Grid>
                <Grid item xs={6} sm={4}>{renderPermissionSwitch('canAccessContacts', 'Contacts')}</Grid>
                <Grid item xs={6} sm={4}>{renderPermissionSwitch('canAccessLeads', 'Leads')}</Grid>
                <Grid item xs={6} sm={4}>{renderPermissionSwitch('canAccessOpportunities', 'Opportunities')}</Grid>
                <Grid item xs={6} sm={4}>{renderPermissionSwitch('canAccessProducts', 'Products')}</Grid>
                <Grid item xs={6} sm={4}>{renderPermissionSwitch('canAccessServices', 'Services')}</Grid>
                <Grid item xs={6} sm={4}>{renderPermissionSwitch('canAccessCampaigns', 'Campaigns')}</Grid>
                <Grid item xs={6} sm={4}>{renderPermissionSwitch('canAccessQuotes', 'Quotes')}</Grid>
                <Grid item xs={6} sm={4}>{renderPermissionSwitch('canAccessTasks', 'Tasks')}</Grid>
                <Grid item xs={6} sm={4}>{renderPermissionSwitch('canAccessActivities', 'Activities')}</Grid>
                <Grid item xs={6} sm={4}>{renderPermissionSwitch('canAccessNotes', 'Notes')}</Grid>
                <Grid item xs={6} sm={4}>{renderPermissionSwitch('canAccessWorkflows', 'Workflows')}</Grid>
                <Grid item xs={6} sm={4}>{renderPermissionSwitch('canAccessServiceRequests', 'Service Requests')}</Grid>
                <Grid item xs={6} sm={4}>{renderPermissionSwitch('canAccessReports', 'Reports')}</Grid>
                <Grid item xs={6} sm={4}>{renderPermissionSwitch('canAccessSettings', 'Settings')}</Grid>
                <Grid item xs={6} sm={4}>{renderPermissionSwitch('canAccessUserManagement', 'User Management')}</Grid>
              </Grid>
            </Box>
          )}

          {dialogTab === 2 && (
            <Box sx={{ pt: 2 }}>
              {formData.isSystemAdmin && (
                <Alert severity="info" sx={{ mb: 2 }}>
                  System Admin groups have all entity permissions by default.
                </Alert>
              )}
              <Accordion defaultExpanded>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Typography fontWeight={600}>Customer Permissions</Typography>
                </AccordionSummary>
                <AccordionDetails>
                  <Grid container spacing={1}>
                    <Grid item xs={6}>{renderPermissionSwitch('canCreateCustomers', 'Create')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canEditCustomers', 'Edit')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canDeleteCustomers', 'Delete')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canViewAllCustomers', 'View All')}</Grid>
                  </Grid>
                </AccordionDetails>
              </Accordion>
              <Accordion>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Typography fontWeight={600}>Contact Permissions</Typography>
                </AccordionSummary>
                <AccordionDetails>
                  <Grid container spacing={1}>
                    <Grid item xs={4}>{renderPermissionSwitch('canCreateContacts', 'Create')}</Grid>
                    <Grid item xs={4}>{renderPermissionSwitch('canEditContacts', 'Edit')}</Grid>
                    <Grid item xs={4}>{renderPermissionSwitch('canDeleteContacts', 'Delete')}</Grid>
                  </Grid>
                </AccordionDetails>
              </Accordion>
              <Accordion>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Typography fontWeight={600}>Lead Permissions</Typography>
                </AccordionSummary>
                <AccordionDetails>
                  <Grid container spacing={1}>
                    <Grid item xs={6}>{renderPermissionSwitch('canCreateLeads', 'Create')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canEditLeads', 'Edit')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canDeleteLeads', 'Delete')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canConvertLeads', 'Convert')}</Grid>
                  </Grid>
                </AccordionDetails>
              </Accordion>
              <Accordion>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Typography fontWeight={600}>Opportunity Permissions</Typography>
                </AccordionSummary>
                <AccordionDetails>
                  <Grid container spacing={1}>
                    <Grid item xs={6}>{renderPermissionSwitch('canCreateOpportunities', 'Create')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canEditOpportunities', 'Edit')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canDeleteOpportunities', 'Delete')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canCloseOpportunities', 'Close')}</Grid>
                  </Grid>
                </AccordionDetails>
              </Accordion>
              <Accordion>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Typography fontWeight={600}>Product Permissions</Typography>
                </AccordionSummary>
                <AccordionDetails>
                  <Grid container spacing={1}>
                    <Grid item xs={6}>{renderPermissionSwitch('canCreateProducts', 'Create')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canEditProducts', 'Edit')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canDeleteProducts', 'Delete')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canManagePricing', 'Manage Pricing')}</Grid>
                  </Grid>
                </AccordionDetails>
              </Accordion>
              <Accordion>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Typography fontWeight={600}>Campaign Permissions</Typography>
                </AccordionSummary>
                <AccordionDetails>
                  <Grid container spacing={1}>
                    <Grid item xs={6}>{renderPermissionSwitch('canCreateCampaigns', 'Create')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canEditCampaigns', 'Edit')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canDeleteCampaigns', 'Delete')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canLaunchCampaigns', 'Launch')}</Grid>
                  </Grid>
                </AccordionDetails>
              </Accordion>
              <Accordion>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Typography fontWeight={600}>Quote Permissions</Typography>
                </AccordionSummary>
                <AccordionDetails>
                  <Grid container spacing={1}>
                    <Grid item xs={6}>{renderPermissionSwitch('canCreateQuotes', 'Create')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canEditQuotes', 'Edit')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canDeleteQuotes', 'Delete')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canApproveQuotes', 'Approve')}</Grid>
                  </Grid>
                </AccordionDetails>
              </Accordion>
              <Accordion>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Typography fontWeight={600}>Task Permissions</Typography>
                </AccordionSummary>
                <AccordionDetails>
                  <Grid container spacing={1}>
                    <Grid item xs={6}>{renderPermissionSwitch('canCreateTasks', 'Create')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canEditTasks', 'Edit')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canDeleteTasks', 'Delete')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canAssignTasks', 'Assign')}</Grid>
                  </Grid>
                </AccordionDetails>
              </Accordion>
              <Accordion>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Typography fontWeight={600}>Workflow Permissions</Typography>
                </AccordionSummary>
                <AccordionDetails>
                  <Grid container spacing={1}>
                    <Grid item xs={6}>{renderPermissionSwitch('canCreateWorkflows', 'Create')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canEditWorkflows', 'Edit')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canDeleteWorkflows', 'Delete')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canActivateWorkflows', 'Activate')}</Grid>
                  </Grid>
                </AccordionDetails>
              </Accordion>
            </Box>
          )}

          {dialogTab === 3 && (
            <Box sx={{ pt: 2 }}>
              {formData.isSystemAdmin && (
                <Alert severity="info" sx={{ mb: 2 }}>
                  System Admin groups have full data access by default.
                </Alert>
              )}
              <Grid container spacing={2}>
                <Grid item xs={12}>
                  <Typography variant="subtitle2" sx={{ mb: 1, fontWeight: 600 }}>Data Scope</Typography>
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    {['own', 'team', 'all'].map((scope) => (
                      <Button
                        key={scope}
                        variant={formData.dataAccessScope === scope ? 'contained' : 'outlined'}
                        onClick={() => handlePermissionChange('dataAccessScope', scope)}
                        disabled={formData.isSystemAdmin}
                        sx={{ textTransform: 'capitalize' }}
                      >
                        {scope === 'own' && 'Own Records'}
                        {scope === 'team' && 'Team Records'}
                        {scope === 'all' && 'All Records'}
                      </Button>
                    ))}
                  </Box>
                </Grid>
                <Grid item xs={12}>
                  <Typography variant="subtitle2" sx={{ mb: 1, fontWeight: 600 }}>Bulk Operations</Typography>
                  <Grid container spacing={1}>
                    <Grid item xs={6}>{renderPermissionSwitch('canExportData', 'Export Data')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canImportData', 'Import Data')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canBulkEdit', 'Bulk Edit')}</Grid>
                    <Grid item xs={6}>{renderPermissionSwitch('canBulkDelete', 'Bulk Delete')}</Grid>
                  </Grid>
                </Grid>
              </Grid>
            </Box>
          )}
          <DialogError error={dialogApi.error} onRetry={() => dialogApi.clearError()} />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog} disabled={dialogApi.loading}>Cancel</Button>
          <ActionButton onClick={handleSaveGroup} variant="contained" color="primary" loading={dialogApi.loading}>
            {editingGroup ? 'Update' : 'Create'}
          </ActionButton>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default GroupManagementTab;

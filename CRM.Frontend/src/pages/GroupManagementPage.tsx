import React, { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  CircularProgress,
  Alert,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControlLabel,
  Checkbox,
  IconButton,
  Tooltip,
  Chip,
  TablePagination,
  TableSortLabel,
  TextField as MuiTextField,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  People as PeopleIcon,
  Security as SecurityIcon,
  Close as CloseIcon,
} from '@mui/icons-material';
import apiClient from '../services/apiClient';

interface UserGroup {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
  isDefault: boolean;
  memberCount: number;
  displayOrder?: number;
  headerColor?: string;
  isSystemAdmin: boolean;
  createdAt: string;
  // Permission flags
  canAccessDashboard: boolean;
  canAccessAccounts: boolean;
  canAccessContacts: boolean;
  canAccessLeads: boolean;
  canAccessOpportunities: boolean;
  canAccessQuotes: boolean;
  canAccessOrders: boolean;
  canAccessInvoices: boolean;
  canAccessProducts: boolean;
  canAccessReports: boolean;
  canAccessSettings: boolean;
  canAccessServiceDesk: boolean;
  canAccessCampaigns: boolean;
  canAccessTeams: boolean;
  canAccessCustomPortal: boolean;
  canAccessPartnerPortal: boolean;
  canAccessKnowledgeBase: boolean;
  canManageUsers: boolean;
  canManageGroups: boolean;
  canManageRoles: boolean;
}

interface GroupFormData {
  name: string;
  description: string;
  isActive: boolean;
  isDefault: boolean;
  displayOrder: number;
  headerColor: string;
  isSystemAdmin: boolean;
  // Permissions
  canAccessDashboard: boolean;
  canAccessAccounts: boolean;
  canAccessContacts: boolean;
  canAccessLeads: boolean;
  canAccessOpportunities: boolean;
  canAccessQuotes: boolean;
  canAccessOrders: boolean;
  canAccessInvoices: boolean;
  canAccessProducts: boolean;
  canAccessReports: boolean;
  canAccessSettings: boolean;
  canAccessServiceDesk: boolean;
  canAccessCampaigns: boolean;
  canAccessTeams: boolean;
  canAccessCustomPortal: boolean;
  canAccessPartnerPortal: boolean;
  canAccessKnowledgeBase: boolean;
  canManageUsers: boolean;
  canManageGroups: boolean;
  canManageRoles: boolean;
}

function GroupManagementPage() {
  const [groups, setGroups] = useState<UserGroup[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [openMembersDialog, setOpenMembersDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [selectedGroupId, setSelectedGroupId] = useState<number | null>(null);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [sortBy, setSortBy] = useState<'name' | 'memberCount' | 'createdAt'>('name');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');

  const [formData, setFormData] = useState<GroupFormData>({
    name: '',
    description: '',
    isActive: true,
    isDefault: false,
    displayOrder: 0,
    headerColor: '#1976d2',
    isSystemAdmin: false,
    canAccessDashboard: true,
    canAccessAccounts: true,
    canAccessContacts: true,
    canAccessLeads: true,
    canAccessOpportunities: true,
    canAccessQuotes: true,
    canAccessOrders: false,
    canAccessInvoices: false,
    canAccessProducts: true,
    canAccessReports: true,
    canAccessSettings: true,
    canAccessServiceDesk: false,
    canAccessCampaigns: false,
    canAccessTeams: true,
    canAccessCustomPortal: false,
    canAccessPartnerPortal: false,
    canAccessKnowledgeBase: true,
    canManageUsers: false,
    canManageGroups: false,
    canManageRoles: false,
  });

  useEffect(() => {
    fetchGroups();
  }, []);

  const fetchGroups = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/usergroups');
      setGroups(response.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch groups');
      console.error('Error fetching groups:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = (group?: UserGroup) => {
    if (group) {
      setEditingId(group.id);
      setFormData({
        name: group.name,
        description: group.description || '',
        isActive: group.isActive,
        isDefault: group.isDefault,
        displayOrder: group.displayOrder || 0,
        headerColor: group.headerColor || '#1976d2',
        isSystemAdmin: group.isSystemAdmin,
        canAccessDashboard: group.canAccessDashboard,
        canAccessAccounts: group.canAccessAccounts,
        canAccessContacts: group.canAccessContacts,
        canAccessLeads: group.canAccessLeads,
        canAccessOpportunities: group.canAccessOpportunities,
        canAccessQuotes: group.canAccessQuotes,
        canAccessOrders: group.canAccessOrders,
        canAccessInvoices: group.canAccessInvoices,
        canAccessProducts: group.canAccessProducts,
        canAccessReports: group.canAccessReports,
        canAccessSettings: group.canAccessSettings,
        canAccessServiceDesk: group.canAccessServiceDesk,
        canAccessCampaigns: group.canAccessCampaigns,
        canAccessTeams: group.canAccessTeams,
        canAccessCustomPortal: group.canAccessCustomPortal,
        canAccessPartnerPortal: group.canAccessPartnerPortal,
        canAccessKnowledgeBase: group.canAccessKnowledgeBase,
        canManageUsers: group.canManageUsers,
        canManageGroups: group.canManageGroups,
        canManageRoles: group.canManageRoles,
      });
    } else {
      setEditingId(null);
      setFormData({
        name: '',
        description: '',
        isActive: true,
        isDefault: false,
        displayOrder: 0,
        headerColor: '#1976d2',
        isSystemAdmin: false,
        canAccessDashboard: true,
        canAccessAccounts: true,
        canAccessContacts: true,
        canAccessLeads: true,
        canAccessOpportunities: true,
        canAccessQuotes: true,
        canAccessOrders: false,
        canAccessInvoices: false,
        canAccessProducts: true,
        canAccessReports: true,
        canAccessSettings: true,
        canAccessServiceDesk: false,
        canAccessCampaigns: false,
        canAccessTeams: true,
        canAccessCustomPortal: false,
        canAccessPartnerPortal: false,
        canAccessKnowledgeBase: true,
        canManageUsers: false,
        canManageGroups: false,
        canManageRoles: false,
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
  };

  const handleOpenMembersDialog = (groupId: number) => {
    setSelectedGroupId(groupId);
    setOpenMembersDialog(true);
  };

  const handleCloseMembersDialog = () => {
    setOpenMembersDialog(false);
    setSelectedGroupId(null);
  };

  const handleFormChange = (field: keyof GroupFormData, value: any) => {
    setFormData(prev => ({
      ...prev,
      [field]: value,
    }));
  };

  const handleSaveGroup = async () => {
    try {
      if (!formData.name.trim()) {
        setError('Group name is required');
        return;
      }

      if (editingId) {
        // Update existing group
        await apiClient.put(`/usergroups/${editingId}`, formData);
        setSuccessMessage('Group updated successfully');
      } else {
        // Create new group
        await apiClient.post('/usergroups', formData);
        setSuccessMessage('Group created successfully');
      }

      handleCloseDialog();
      fetchGroups();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save group');
      console.error('Error saving group:', err);
    }
  };

  const handleDeleteGroup = async (groupId: number) => {
    if (!window.confirm('Are you sure you want to delete this group?')) return;

    try {
      await apiClient.delete(`/usergroups/${groupId}`);
      setSuccessMessage('Group deleted successfully');
      fetchGroups();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete group');
      console.error('Error deleting group:', err);
    }
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(Number.parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleChangePage = (event: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleSort = (column: 'name' | 'memberCount' | 'createdAt') => {
    if (sortBy === column) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setSortBy(column);
      setSortOrder('asc');
    }
  };

  const filteredGroups = groups.filter(g =>
    g.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    g.description?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const sortedGroups = [...filteredGroups].sort((a, b) => {
    let compareValue = 0;
    if (sortBy === 'name') {
      compareValue = a.name.localeCompare(b.name);
    } else if (sortBy === 'memberCount') {
      compareValue = a.memberCount - b.memberCount;
    } else if (sortBy === 'createdAt') {
      compareValue = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
    }
    return sortOrder === 'asc' ? compareValue : -compareValue;
  });

  const paginatedGroups = sortedGroups.slice(
    page * rowsPerPage,
    page * rowsPerPage + rowsPerPage
  );

  if (loading) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <CircularProgress />
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
        <Typography variant="h4" component="h1">
          Group Management
        </Typography>
        <Button
          variant="contained"
          color="primary"
          startIcon={<AddIcon />}
          onClick={() => handleOpenDialog()}
        >
          Create Group
        </Button>
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

      <Box sx={{ mb: 2 }}>
        <TextField
          placeholder="Search groups..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          fullWidth
          size="small"
        />
      </Box>

      <Card>
        <CardContent>
          <Table>
            <TableHead>
              <TableRow sx={{ backgroundColor: '#f5f5f5' }}>
                <TableCell>
                  <TableSortLabel
                    active={sortBy === 'name'}
                    direction={sortOrder}
                    onClick={() => handleSort('name')}
                  >
                    Group Name
                  </TableSortLabel>
                </TableCell>
                <TableCell>Description</TableCell>
                <TableCell align="center">
                  <TableSortLabel
                    active={sortBy === 'memberCount'}
                    direction={sortOrder}
                    onClick={() => handleSort('memberCount')}
                  >
                    Members
                  </TableSortLabel>
                </TableCell>
                <TableCell align="center">Status</TableCell>
                <TableCell align="center">Default</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {paginatedGroups.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} align="center">
                    No groups found
                  </TableCell>
                </TableRow>
              ) : (
                paginatedGroups.map((group) => (
                  <TableRow key={group.id} hover>
                    <TableCell sx={{ fontWeight: 600 }}>{group.name}</TableCell>
                    <TableCell>{group.description || '-'}</TableCell>
                    <TableCell align="center">
                      <Chip
                        icon={<PeopleIcon />}
                        label={group.memberCount}
                        variant="outlined"
                        size="small"
                      />
                    </TableCell>
                    <TableCell align="center">
                      <Chip
                        label={group.isActive ? 'Active' : 'Inactive'}
                        color={group.isActive ? 'success' : 'default'}
                        size="small"
                      />
                    </TableCell>
                    <TableCell align="center">
                      {group.isDefault && (
                        <Chip
                          icon={<SecurityIcon />}
                          label="Default"
                          color="primary"
                          size="small"
                          variant="outlined"
                        />
                      )}
                    </TableCell>
                    <TableCell align="right">
                      <Tooltip title="View Members">
                        <IconButton
                          size="small"
                          onClick={() => handleOpenMembersDialog(group.id)}
                          color="info"
                        >
                          <PeopleIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Edit">
                        <IconButton
                          size="small"
                          onClick={() => handleOpenDialog(group)}
                          color="primary"
                        >
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Delete">
                        <IconButton
                          size="small"
                          onClick={() => handleDeleteGroup(group.id)}
                          color="error"
                        >
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
          <TablePagination
            rowsPerPageOptions={[5, 10, 25, 50]}
            component="div"
            count={filteredGroups.length}
            rowsPerPage={rowsPerPage}
            page={page}
            onPageChange={handleChangePage}
            onRowsPerPageChange={handleChangeRowsPerPage}
          />
        </CardContent>
      </Card>

      {/* Create/Edit Group Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle>
          {editingId ? 'Edit Group' : 'Create New Group'}
        </DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <TextField
            label="Group Name"
            value={formData.name}
            onChange={(e) => handleFormChange('name', e.target.value)}
            fullWidth
            margin="normal"
            required
          />
          <TextField
            label="Description"
            value={formData.description}
            onChange={(e) => handleFormChange('description', e.target.value)}
            fullWidth
            margin="normal"
            multiline
            rows={3}
          />
          <Box sx={{ mt: 3, mb: 2 }}>
            <Typography variant="h6" sx={{ mb: 2 }}>
              Permissions
            </Typography>
            <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 1 }}>
              {Object.entries(formData).map(([key, value]) => {
                if (key.startsWith('can') || key.startsWith('canManage')) {
                  return (
                    <FormControlLabel
                      key={key}
                      control={
                        <Checkbox
                          checked={typeof value === 'boolean' ? value : false}
                          onChange={(e) =>
                            handleFormChange(key as keyof GroupFormData, e.target.checked)
                          }
                        />
                      }
                      label={key.replaceAll(/([A-Z])/g, ' $1').trim()}
                    />
                  );
                }
                return null;
              })}
            </Box>
          </Box>
          <FormControlLabel
            control={
              <Checkbox
                checked={formData.isActive}
                onChange={(e) => handleFormChange('isActive', e.target.checked)}
              />
            }
            label="Active"
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={formData.isDefault}
                onChange={(e) => handleFormChange('isDefault', e.target.checked)}
              />
            }
            label="Set as Default"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSaveGroup} variant="contained" color="primary">
            {editingId ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Members Dialog */}
      <Dialog
        open={openMembersDialog}
        onClose={handleCloseMembersDialog}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>
          Group Members
          <IconButton
            aria-label="close"
            onClick={handleCloseMembersDialog}
            sx={{
              position: 'absolute',
              right: 8,
              top: 8,
              color: (theme) => theme.palette.grey[500],
            }}
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="textSecondary">
            Members management coming soon...
          </Typography>
        </DialogContent>
      </Dialog>
    </Container>
  );
}

export default GroupManagementPage;

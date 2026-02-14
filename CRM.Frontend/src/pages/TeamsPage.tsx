import React, { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Typography,
  Button,
  Card,
  CardContent,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  Chip,
  IconButton,
  Tooltip,
  Dialog,
  DialogContent,
  DialogActions,
  TextField,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Stack,
  Alert,
  CircularProgress,
  Tabs,
  Tab,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  Avatar,
} from '@mui/material';
import type { SelectChangeEvent } from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Refresh as RefreshIcon,
  PersonAdd as PersonAddIcon,
  PersonRemove as PersonRemoveIcon,
  Groups as GroupsIcon,
} from '@mui/icons-material';
import { DialogError } from '../components/common/DialogError';
import ActionButton from '../components/common/ActionButton';
import { DialogHeader } from '../components/common/DialogHeader';
import TabPanel from '../components/common/TabPanel';
import { EnhancedEmptyState } from '../components/common/EnhancedEmptyState';
import { useApiState } from '../hooks/useApiState';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';

// ==================== ENUMS ====================

enum TeamRole {
  Member = 0,
  Lead = 1,
  Manager = 2,
  Admin = 3,
}

// ==================== INTERFACES ====================

interface Team {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
  managerId?: number;
  managerName?: string;
  parentTeamId?: number;
  parentTeamName?: string;
  memberCount: number;
  createdAt: string;
  updatedAt?: string;
}

interface TeamMember {
  id: number;
  teamId: number;
  userId: number;
  userName?: string;
  userEmail?: string;
  role: TeamRole;
  joinedAt?: string;
}

interface TeamForm {
  name: string;
  description: string;
  isActive: boolean;
  managerId: number | null;
  parentTeamId: number | null;
}

interface AddMemberForm {
  userId: number | null;
  role: TeamRole;
}

// ==================== CONSTANTS ====================

type ChipColor = 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning';

const TEAM_ROLE_OPTIONS: Array<{ value: TeamRole; label: string; color: ChipColor }> = [
  { value: TeamRole.Member, label: 'Member', color: 'default' },
  { value: TeamRole.Lead, label: 'Lead', color: 'info' },
  { value: TeamRole.Manager, label: 'Manager', color: 'primary' },
  { value: TeamRole.Admin, label: 'Admin', color: 'warning' },
];

// ==================== HELPER FUNCTIONS ====================

const getRoleInfo = (role: TeamRole): { label: string; color: ChipColor } =>
  TEAM_ROLE_OPTIONS.find(r => r.value === role) || { label: 'Unknown', color: 'default' };

// ==================== MAIN COMPONENT ====================

function TeamsPage() {
  const [teams, setTeams] = useState<Team[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  const [members, setMembers] = useState<TeamMember[]>([]);
  const [addMemberDialogOpen, setAddMemberDialogOpen] = useState(false);
  const [addMemberTeamId, setAddMemberTeamId] = useState<number | null>(null);

  const emptyForm: TeamForm = {
    name: '',
    description: '',
    isActive: true,
    managerId: null,
    parentTeamId: null,
  };
  const [formData, setFormData] = useState<TeamForm>(emptyForm);

  const emptyMemberForm: AddMemberForm = {
    userId: null,
    role: TeamRole.Member,
  };
  const [memberFormData, setMemberFormData] = useState<AddMemberForm>(emptyMemberForm);

  const dialogApi = useApiState();
  const memberDialogApi = useApiState();

  // ==================== DATA FETCHING ====================

  useEffect(() => {
    fetchTeams();
  }, []);

  const fetchTeams = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/teams');
      setTeams(response.data);
      setError(null);
    } catch (err: any) {
      if (err.response?.status === 404) {
        setTeams([]);
        setError(null);
      } else {
        setError(err.response?.data?.message || 'Failed to fetch teams');
      }
    } finally {
      setLoading(false);
    }
  };

  const fetchMembers = async (teamId: number) => {
    try {
      const response = await apiClient.get(`/teams/${teamId}/members`);
      setMembers(response.data);
    } catch {
      setMembers([]);
    }
  };

  // ==================== DIALOG HANDLERS ====================

  const handleOpenDialog = (team?: Team) => {
    setDialogTab(0);
    if (team) {
      setEditingId(team.id);
      setFormData({
        name: team.name,
        description: team.description || '',
        isActive: team?.isActive !== false,
        managerId: team.managerId || null,
        parentTeamId: team.parentTeamId || null,
      });
      fetchMembers(team.id);
    } else {
      setEditingId(null);
      setFormData(emptyForm);
      setMembers([]);
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
      [name]: type === 'checkbox' ? checked : value,
    }));
  };

  // ==================== SAVE OPERATIONS ====================

  const handleSaveTeam = async () => {
    if (!formData.name?.trim()) {
      dialogApi.setError('Team name is required');
      return;
    }

    await dialogApi.execute(async () => {
      if (editingId) {
        await apiClient.put(`/teams/${editingId}`, formData);
        setSuccessMessage('Team updated successfully');
      } else {
        await apiClient.post('/teams', formData);
        setSuccessMessage('Team created successfully');
      }
      handleCloseDialog();
      fetchTeams();
      setTimeout(() => setSuccessMessage(null), 3000);
    });
  };

  const handleDeleteTeam = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this team?')) {
      try {
        await apiClient.delete(`/teams/${id}`);
        setSuccessMessage('Team deleted successfully');
        fetchTeams();
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete team');
      }
    }
  };

  // ==================== MEMBER OPERATIONS ====================

  const handleOpenAddMember = (teamId: number) => {
    setAddMemberTeamId(teamId);
    setMemberFormData(emptyMemberForm);
    setAddMemberDialogOpen(true);
  };

  const handleAddMember = async () => {
    if (!addMemberTeamId || !memberFormData.userId) {
      memberDialogApi.setError('Please enter a user ID');
      return;
    }

    await memberDialogApi.execute(async () => {
      await apiClient.post(`/teams/${addMemberTeamId}/members`, {
        userId: memberFormData.userId,
        role: memberFormData.role,
      });
      setSuccessMessage('Member added successfully');
      setAddMemberDialogOpen(false);
      if (editingId === addMemberTeamId) {
        fetchMembers(addMemberTeamId);
      }
      fetchTeams();
      setTimeout(() => setSuccessMessage(null), 3000);
    });
  };

  const handleRemoveMember = async (teamId: number, userId: number) => {
    if (window.confirm('Remove this member from the team?')) {
      try {
        await apiClient.delete(`/teams/${teamId}/members/${userId}`);
        setSuccessMessage('Member removed');
        if (editingId === teamId) {
          fetchMembers(teamId);
        }
        fetchTeams();
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to remove member');
      }
    }
  };

  // ==================== RENDER ====================

  if (loading) {
    return (
      <Container maxWidth="lg">
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="50vh">
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg">
      <Box mb={4}>
        {/* Header */}
        <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
          <Box display="flex" alignItems="center" gap={2}>
            <img src={logo} alt="CRM Logo" style={{ height: 40, borderRadius: 8 }} />
            <Typography variant="h4">Teams</Typography>
          </Box>
          <Stack direction="row" spacing={2}>
            <Button variant="outlined" startIcon={<RefreshIcon />} onClick={fetchTeams}>
              Refresh
            </Button>
            <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenDialog()}>
              New Team
            </Button>
          </Stack>
        </Box>

        {/* Alerts */}
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

        {/* Teams Table */}
        <Card>
          <CardContent>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Description</TableCell>
                  <TableCell>Manager</TableCell>
                  <TableCell align="center">Members</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {teams.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={6} sx={{ border: 0 }}>
                      <EnhancedEmptyState
                        illustration="generic"
                        title="No teams yet"
                        description="Create your first team to organize your sales force"
                        variant="no-data"
                        primaryActionLabel="Create Team"
                        onPrimaryAction={() => handleOpenDialog()}
                      />
                    </TableCell>
                  </TableRow>
                ) : (
                  teams.map((team) => (
                    <TableRow key={team.id} hover>
                      <TableCell>
                        <Box display="flex" alignItems="center" gap={1}>
                          <GroupsIcon fontSize="small" color="action" />
                          <Typography fontWeight="medium">{team.name}</Typography>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" color="text.secondary" noWrap sx={{ maxWidth: 250 }}>
                          {team.description || '-'}
                        </Typography>
                      </TableCell>
                      <TableCell>{team.managerName || '-'}</TableCell>
                      <TableCell align="center">
                        <Chip label={team.memberCount} size="small" variant="outlined" />
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={team?.isActive !== false ? 'Active' : 'Inactive'}
                          size="small"
                          color={team?.isActive !== false ? 'success' : 'default'}
                        />
                      </TableCell>
                      <TableCell align="right">
                        <Tooltip title="Edit">
                          <IconButton size="small" onClick={() => handleOpenDialog(team)}>
                            <EditIcon />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Add Member">
                          <IconButton size="small" color="primary" onClick={() => handleOpenAddMember(team.id)}>
                            <PersonAddIcon />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton size="small" color="error" onClick={() => handleDeleteTeam(team.id)}>
                            <DeleteIcon />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </Box>

      {/* Team Editor Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogHeader
          mode={editingId ? 'edit' : 'create'}
          entityType="team"
          entityName={editingId ? formData.name || undefined : undefined}
          entityId={editingId || undefined}
          onClose={handleCloseDialog}
        />
        <DialogContent dividers>
          <Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)} sx={{ mb: 2 }}>
            <Tab label="Team Details" />
            {editingId && <Tab label="Members" />}
          </Tabs>

          <DialogError error={dialogApi.error} />

          {/* Tab 0: Team Details */}
          <TabPanel value={dialogTab} index={0}>
            <Grid container spacing={3}>
              <Grid item xs={12} md={8}>
                <TextField
                  fullWidth
                  required
                  label="Team Name"
                  name="name"
                  value={formData.name}
                  onChange={handleInputChange}
                />
              </Grid>
              <Grid item xs={12} md={4}>
                <Box display="flex" alignItems="center" height="100%">
                  <label>
                    <input
                      type="checkbox"
                      name="isActive"
                      checked={formData?.isActive !== false}
                      onChange={handleInputChange}
                    />
                    {' '}Active
                  </label>
                </Box>
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  multiline
                  rows={3}
                  label="Description"
                  name="description"
                  value={formData.description}
                  onChange={handleInputChange}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  type="number"
                  label="Manager User ID"
                  value={formData.managerId || ''}
                  onChange={(e) => setFormData(prev => ({ ...prev, managerId: e.target.value ? parseInt(e.target.value) : null }))}
                  helperText="Enter the user ID of the team manager"
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  type="number"
                  label="Parent Team ID"
                  value={formData.parentTeamId || ''}
                  onChange={(e) => setFormData(prev => ({ ...prev, parentTeamId: e.target.value ? parseInt(e.target.value) : null }))}
                  helperText="Enter the parent team ID for hierarchy"
                />
              </Grid>
            </Grid>
          </TabPanel>

          {/* Tab 1: Members */}
          {editingId && (
            <TabPanel value={dialogTab} index={1}>
              <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
                <Typography variant="subtitle1">
                  Team Members ({members.length})
                </Typography>
                <Button size="small" startIcon={<PersonAddIcon />} onClick={() => handleOpenAddMember(editingId)}>
                  Add Member
                </Button>
              </Box>
              {members.length === 0 ? (
                <Typography color="text.secondary" textAlign="center" py={4}>
                  No members yet. Add members to this team.
                </Typography>
              ) : (
                <List>
                  {members.map(member => {
                    const roleInfo = getRoleInfo(member.role);
                    return (
                      <ListItem
                        key={member.id}
                        secondaryAction={
                          <IconButton edge="end" size="small" color="error"
                            onClick={() => handleRemoveMember(editingId, member.userId)}>
                            <PersonRemoveIcon />
                          </IconButton>
                        }
                      >
                        <ListItemAvatar>
                          <Avatar sx={{ width: 32, height: 32, fontSize: 14 }}>
                            {(member.userName || 'U').charAt(0).toUpperCase()}
                          </Avatar>
                        </ListItemAvatar>
                        <ListItemText
                          primary={member.userName || `User #${member.userId}`}
                          secondary={member.userEmail || ''}
                        />
                        <Chip label={roleInfo.label} size="small" color={roleInfo.color} sx={{ mr: 2 }} />
                      </ListItem>
                    );
                  })}
                </List>
              )}
            </TabPanel>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <ActionButton
            onClick={handleSaveTeam}
            loading={dialogApi.loading}
            variant="contained"
          >
            {editingId ? 'Update Team' : 'Create Team'}
          </ActionButton>
        </DialogActions>
      </Dialog>

      {/* Add Member Dialog */}
      <Dialog open={addMemberDialogOpen} onClose={() => setAddMemberDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogHeader
          mode="create"
          entityType="user"
          onClose={() => setAddMemberDialogOpen(false)}
        />
        <DialogContent dividers>
          <DialogError error={memberDialogApi.error} />
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                required
                type="number"
                label="User ID"
                value={memberFormData.userId || ''}
                onChange={(e) => setMemberFormData(prev => ({ ...prev, userId: e.target.value ? parseInt(e.target.value) : null }))}
                helperText="Enter the user ID to add"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Role</InputLabel>
                <Select
                  value={memberFormData.role}
                  onChange={(e: SelectChangeEvent<number>) =>
                    setMemberFormData(prev => ({ ...prev, role: e.target.value as TeamRole }))
                  }
                  label="Role"
                >
                  {TEAM_ROLE_OPTIONS.map(opt => (
                    <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAddMemberDialogOpen(false)}>Cancel</Button>
          <ActionButton
            onClick={handleAddMember}
            loading={memberDialogApi.loading}
            variant="contained"
          >
            Add Member
          </ActionButton>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

export default TeamsPage;

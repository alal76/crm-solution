import { Box, Container, Typography, Card, CardContent, Table, TableBody, TableCell, TableHead, TableRow, CircularProgress, Alert, Button, Dialog, DialogTitle, DialogContent, DialogActions, TextField, FormControl, InputLabel, Select, MenuItem, FormControlLabel, Checkbox, Grid } from '@mui/material';
import { useState, useEffect } from 'react';
import { Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon } from '@mui/icons-material';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';
import LookupSelect from '../components/LookupSelect';

interface UserProfile {
  id: number;
  name: string;
  description: string;
  departmentId: number;
  departmentName?: string;
  isActive: boolean;
  canCreateCustomers: boolean;
  canEditCustomers: boolean;
  canDeleteCustomers: boolean;
  canCreateOpportunities: boolean;
  canEditOpportunities: boolean;
  canDeleteOpportunities: boolean;
  canCreateProducts: boolean;
  canEditProducts: boolean;
  canDeleteProducts: boolean;
  canManageCampaigns: boolean;
  canViewReports: boolean;
  canManageUsers: boolean;
  createdAt: string;
}

interface Department {
  id: number;
  name: string;
}

interface ProfileForm {
  name: string;
  description: string;
  departmentId: number;
  isActive: boolean;
  canCreateCustomers: boolean;
  canEditCustomers: boolean;
  canDeleteCustomers: boolean;
  canCreateOpportunities: boolean;
  canEditOpportunities: boolean;
  canDeleteOpportunities: boolean;
  canCreateProducts: boolean;
  canEditProducts: boolean;
  canDeleteProducts: boolean;
  canManageCampaigns: boolean;
  canViewReports: boolean;
  canManageUsers: boolean;
}

function ProfileManagementPage() {
  const [profiles, setProfiles] = useState<UserProfile[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<ProfileForm>({
    name: '',
    description: '',
    departmentId: 0,
    isActive: true,
    canCreateCustomers: false,
    canEditCustomers: false,
    canDeleteCustomers: false,
    canCreateOpportunities: false,
    canEditOpportunities: false,
    canDeleteOpportunities: false,
    canCreateProducts: false,
    canEditProducts: false,
    canDeleteProducts: false,
    canManageCampaigns: false,
    canViewReports: false,
    canManageUsers: false,
  });

  useEffect(() => {
    fetchAllData();
  }, []);

  const fetchAllData = async () => {
    try {
      setLoading(true);
      const [profilesRes, deptsRes] = await Promise.all([
        apiClient.get('/user-profiles'),
        apiClient.get('/departments'),
      ]);
      setProfiles(profilesRes.data);
      setDepartments(deptsRes.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch data');
      console.error('Error fetching data:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = (profile?: UserProfile) => {
    if (profile) {
      setEditingId(profile.id);
      setFormData({
        name: profile.name,
        description: profile.description,
        departmentId: profile.departmentId,
        isActive: profile.isActive,
        canCreateCustomers: profile.canCreateCustomers,
        canEditCustomers: profile.canEditCustomers,
        canDeleteCustomers: profile.canDeleteCustomers,
        canCreateOpportunities: profile.canCreateOpportunities,
        canEditOpportunities: profile.canEditOpportunities,
        canDeleteOpportunities: profile.canDeleteOpportunities,
        canCreateProducts: profile.canCreateProducts,
        canEditProducts: profile.canEditProducts,
        canDeleteProducts: profile.canDeleteProducts,
        canManageCampaigns: profile.canManageCampaigns,
        canViewReports: profile.canViewReports,
        canManageUsers: profile.canManageUsers,
      });
    } else {
      setEditingId(null);
      setFormData({
        name: '',
        description: '',
        departmentId: departments[0]?.id || 0,
        isActive: true,
        canCreateCustomers: false,
        canEditCustomers: false,
        canDeleteCustomers: false,
        canCreateOpportunities: false,
        canEditOpportunities: false,
        canDeleteOpportunities: false,
        canCreateProducts: false,
        canEditProducts: false,
        canDeleteProducts: false,
        canManageCampaigns: false,
        canViewReports: false,
        canManageUsers: false,
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
  };

  const handleSelectChange = (e: any) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, checked, type } = e.target as any;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }));
  };

  const handleSaveProfile = async () => {
    if (!formData.name.trim() || !formData.departmentId) {
      setError('Please fill in required fields (Name, Department)');
      return;
    }

    try {
      if (editingId) {
        await apiClient.put(`/user-profiles/${editingId}`, formData);
        setSuccessMessage('Profile updated successfully');
      } else {
        await apiClient.post('/user-profiles', formData);
        setSuccessMessage('Profile created successfully');
      }
      handleCloseDialog();
      fetchAllData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save profile');
      console.error('Error saving profile:', err);
    }
  };

  const handleDeleteProfile = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this profile?')) {
      try {
        await apiClient.delete(`/user-profiles/${id}`);
        setSuccessMessage('Profile deleted successfully');
        fetchAllData();
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete profile');
        console.error('Error deleting profile:', err);
      }
    }
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
      <Container maxWidth="lg">
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ width: 40, height: 40, flexShrink: 0 }}><img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} /></Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>Profile Management</Typography>
          </Box>
          <Button
            variant="contained"
            color="primary"
            startIcon={<AddIcon />}
            onClick={() => handleOpenDialog()}
            sx={{ backgroundColor: '#6750A4' }}
          >
            Add Profile
          </Button>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

        <Card>
          <CardContent>
            <Table>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                  <TableCell><strong>Name</strong></TableCell>
                  <TableCell><strong>Department</strong></TableCell>
                  <TableCell><strong>Description</strong></TableCell>
                  <TableCell><strong>Status</strong></TableCell>
                  <TableCell align="center"><strong>Actions</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {profiles.map((profile) => (
                  <TableRow key={profile.id}>
                    <TableCell>{profile.name}</TableCell>
                    <TableCell>{profile.departmentName || '-'}</TableCell>
                    <TableCell>{profile.description}</TableCell>
                    <TableCell>{profile.isActive ? 'Active' : 'Inactive'}</TableCell>
                    <TableCell align="center">
                      <Button
                        size="small"
                        color="primary"
                        startIcon={<EditIcon />}
                        onClick={() => handleOpenDialog(profile)}
                        sx={{ mr: 1 }}
                      >
                        Edit
                      </Button>
                      <Button
                        size="small"
                        color="error"
                        startIcon={<DeleteIcon />}
                        onClick={() => handleDeleteProfile(profile.id)}
                      >
                        Delete
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
            {profiles.length === 0 && (
              <Typography sx={{ textAlign: 'center', py: 2, color: 'textSecondary' }}>
                No profiles found
              </Typography>
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Add/Edit Profile Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{editingId ? 'Edit Profile' : 'Create New Profile'}</DialogTitle>
        <DialogContent sx={{ pt: 2, maxHeight: '70vh', overflow: 'auto' }}>
          <TextField
            autoFocus
            fullWidth
            label="Profile Name"
            name="name"
            value={formData.name}
            onChange={handleInputChange}
            margin="normal"
            required
          />
          <FormControl fullWidth margin="normal" required>
            <LookupSelect
              category="Department"
              name="departmentId"
              value={formData.departmentId}
              onChange={handleSelectChange}
              label="Department"
              fallback={departments.map(d => ({ value: d.id, label: d.name }))}
            />
          </FormControl>
          <TextField
            fullWidth
            label="Description"
            name="description"
            value={formData.description}
            onChange={handleInputChange}
            margin="normal"
            multiline
            rows={2}
          />

          <Typography variant="subtitle2" sx={{ mt: 3, mb: 2, fontWeight: 600 }}>
            Permissions
          </Typography>

          <Typography variant="subtitle2" sx={{ mt: 2, mb: 1, color: 'primary.main' }}>
            Customer Management
          </Typography>
          <Grid container spacing={1}>
            <Grid item xs={12}>
              <FormControlLabel
                control={<Checkbox name="canCreateCustomers" checked={formData.canCreateCustomers} onChange={handleInputChange} />}
                label="Create Customers"
              />
            </Grid>
            <Grid item xs={12}>
              <FormControlLabel
                control={<Checkbox name="canEditCustomers" checked={formData.canEditCustomers} onChange={handleInputChange} />}
                label="Edit Customers"
              />
            </Grid>
            <Grid item xs={12}>
              <FormControlLabel
                control={<Checkbox name="canDeleteCustomers" checked={formData.canDeleteCustomers} onChange={handleInputChange} />}
                label="Delete Customers"
              />
            </Grid>
          </Grid>

          <Typography variant="subtitle2" sx={{ mt: 2, mb: 1, color: 'primary.main' }}>
            Opportunity Management
          </Typography>
          <Grid container spacing={1}>
            <Grid item xs={12}>
              <FormControlLabel
                control={<Checkbox name="canCreateOpportunities" checked={formData.canCreateOpportunities} onChange={handleInputChange} />}
                label="Create Opportunities"
              />
            </Grid>
            <Grid item xs={12}>
              <FormControlLabel
                control={<Checkbox name="canEditOpportunities" checked={formData.canEditOpportunities} onChange={handleInputChange} />}
                label="Edit Opportunities"
              />
            </Grid>
            <Grid item xs={12}>
              <FormControlLabel
                control={<Checkbox name="canDeleteOpportunities" checked={formData.canDeleteOpportunities} onChange={handleInputChange} />}
                label="Delete Opportunities"
              />
            </Grid>
          </Grid>

          <Typography variant="subtitle2" sx={{ mt: 2, mb: 1, color: 'primary.main' }}>
            Product Management
          </Typography>
          <Grid container spacing={1}>
            <Grid item xs={12}>
              <FormControlLabel
                control={<Checkbox name="canCreateProducts" checked={formData.canCreateProducts} onChange={handleInputChange} />}
                label="Create Products"
              />
            </Grid>
            <Grid item xs={12}>
              <FormControlLabel
                control={<Checkbox name="canEditProducts" checked={formData.canEditProducts} onChange={handleInputChange} />}
                label="Edit Products"
              />
            </Grid>
            <Grid item xs={12}>
              <FormControlLabel
                control={<Checkbox name="canDeleteProducts" checked={formData.canDeleteProducts} onChange={handleInputChange} />}
                label="Delete Products"
              />
            </Grid>
          </Grid>

          <Typography variant="subtitle2" sx={{ mt: 2, mb: 1, color: 'primary.main' }}>
            System Features
          </Typography>
          <Grid container spacing={1}>
            <Grid item xs={12}>
              <FormControlLabel
                control={<Checkbox name="canManageCampaigns" checked={formData.canManageCampaigns} onChange={handleInputChange} />}
                label="Manage Campaigns"
              />
            </Grid>
            <Grid item xs={12}>
              <FormControlLabel
                control={<Checkbox name="canViewReports" checked={formData.canViewReports} onChange={handleInputChange} />}
                label="View Reports"
              />
            </Grid>
            <Grid item xs={12}>
              <FormControlLabel
                control={<Checkbox name="canManageUsers" checked={formData.canManageUsers} onChange={handleInputChange} />}
                label="Manage Users"
              />
            </Grid>
            <Grid item xs={12}>
              <FormControlLabel
                control={<Checkbox name="isActive" checked={formData.isActive} onChange={handleInputChange} />}
                label="Active"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSaveProfile} variant="contained" color="primary">
            {editingId ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default ProfileManagementPage;

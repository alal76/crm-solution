import { Box, Container, Typography, Card, CardContent, Table, TableBody, TableCell, TableHead, TableRow, CircularProgress, Alert, Button, Dialog, DialogTitle, DialogContent, DialogActions, TextField, Select, MenuItem, FormControl, InputLabel, Chip } from '@mui/material';
import { useState, useEffect } from 'react';
import { Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, Lock as LockIcon } from '@mui/icons-material';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';

interface User {
  id: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  role: number;
  isActive: boolean;
  lastLoginDate?: string;
  departmentId?: number;
  userProfileId?: number;
}

interface Contact {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
}

interface Department {
  id: number;
  name: string;
}

interface UserProfile {
  id: number;
  name: string;
  departmentId: number;
}

interface UserFormData {
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  password: string;
  role: number;
  departmentId?: number;
  userProfileId?: number;
  contactId?: number;
}

const roleOptions = [
  { value: 0, label: 'Admin' },
  { value: 1, label: 'Manager' },
  { value: 2, label: 'Sales' },
  { value: 3, label: 'Support' },
  { value: 4, label: 'Guest' },
];

const getRoleLabel = (role: number) => roleOptions.find(r => r.value === role)?.label || 'Unknown';

function UserManagementPage() {
  const [users, setUsers] = useState<User[]>([]);
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [profiles, setProfiles] = useState<UserProfile[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [openPasswordDialog, setOpenPasswordDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [selectedUserId, setSelectedUserId] = useState<number | null>(null);
  const [formData, setFormData] = useState<UserFormData>({
    username: '',
    email: '',
    firstName: '',
    lastName: '',
    password: '',
    role: 2, // Sales by default
  });
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  useEffect(() => {
    fetchAllData();
  }, []);

  const fetchAllData = async () => {
    try {
      setLoading(true);
      const [usersRes, contactsRes, deptRes, profileRes] = await Promise.all([
        apiClient.get('/users'),
        apiClient.get('/contacts'),
        apiClient.get('/departments'),
        apiClient.get('/user-profiles'),
      ]);
      setUsers(usersRes.data);
      setContacts(contactsRes.data);
      setDepartments(deptRes.data);
      setProfiles(profileRes.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch data');
      console.error('Error fetching data:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = (user?: User) => {
    if (user) {
      setEditingId(user.id);
      setFormData({
        username: user.username,
        email: user.email,
        firstName: user.firstName,
        lastName: user.lastName,
        password: '',
        role: user.role,
        departmentId: user.departmentId,
        userProfileId: user.userProfileId,
      });
    } else {
      setEditingId(null);
      setFormData({
        username: '',
        email: '',
        firstName: '',
        lastName: '',
        password: '',
        role: 2,
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
  };

  const handleOpenPasswordDialog = (userId: number) => {
    setSelectedUserId(userId);
    setNewPassword('');
    setConfirmPassword('');
    setOpenPasswordDialog(true);
  };

  const handleClosePasswordDialog = () => {
    setOpenPasswordDialog(false);
    setSelectedUserId(null);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | { name?: string; value: unknown }>) => {
    const { name, value } = e.target as HTMLInputElement;
    setFormData(prev => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSelectChange = (name: string, value: unknown) => {
    setFormData(prev => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSaveUser = async () => {
    if (!formData.username.trim() || !formData.email.trim() || !formData.firstName.trim() || !formData.lastName.trim()) {
      setError('Please fill in all required fields');
      return;
    }

    if (!editingId && !formData.password.trim()) {
      setError('Password is required for new users');
      return;
    }

    try {
      const payload = editingId
        ? { ...formData, password: formData.password || undefined }
        : formData;

      if (editingId) {
        await apiClient.put(`/users/${editingId}`, payload);
        setSuccessMessage('User updated successfully');
      } else {
        await apiClient.post('/users', payload);
        setSuccessMessage('User created successfully');
      }
      handleCloseDialog();
      fetchAllData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save user');
      console.error('Error saving user:', err);
    }
  };

  const handleResetPassword = async () => {
    if (!newPassword.trim() || !confirmPassword.trim()) {
      setError('Please fill in password fields');
      return;
    }

    if (newPassword !== confirmPassword) {
      setError('Passwords do not match');
      return;
    }

    if (newPassword.length < 6) {
      setError('Password must be at least 6 characters');
      return;
    }

    try {
      await apiClient.post(`/auth/reset-password/${selectedUserId}`, {
        newPassword,
      });
      setSuccessMessage('Password reset successfully');
      handleClosePasswordDialog();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to reset password');
      console.error('Error resetting password:', err);
    }
  };

  const handleDeleteUser = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this user?')) {
      try {
        await apiClient.delete(`/users/${id}`);
        setSuccessMessage('User deleted successfully');
        fetchAllData();
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete user');
        console.error('Error deleting user:', err);
      }
    }
  };

  const handleToggleActive = async (user: User) => {
    try {
      await apiClient.put(`/users/${user.id}`, { ...user, isActive: !user.isActive });
      setSuccessMessage(`User ${!user.isActive ? 'activated' : 'deactivated'} successfully`);
      fetchAllData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update user status');
      console.error('Error updating user:', err);
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
            <Typography variant="h4" sx={{ fontWeight: 700 }}>User Management</Typography>
          </Box>
          <Button
            variant="contained"
            color="primary"
            startIcon={<AddIcon />}
            onClick={() => handleOpenDialog()}
            sx={{ backgroundColor: '#6750A4' }}
          >
            Add User
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
                  <TableCell><strong>Email</strong></TableCell>
                  <TableCell><strong>Role</strong></TableCell>
                  <TableCell><strong>Department</strong></TableCell>
                  <TableCell><strong>Status</strong></TableCell>
                  <TableCell align="center"><strong>Actions</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {users.map((user) => (
                  <TableRow key={user.id}>
                    <TableCell>{user.firstName} {user.lastName}</TableCell>
                    <TableCell>{user.email}</TableCell>
                    <TableCell>{getRoleLabel(user.role)}</TableCell>
                    <TableCell>{departments.find(d => d.id === user.departmentId)?.name || '-'}</TableCell>
                    <TableCell>
                      <Chip
                        label={user.isActive ? 'Active' : 'Inactive'}
                        color={user.isActive ? 'success' : 'error'}
                        variant="outlined"
                        size="small"
                      />
                    </TableCell>
                    <TableCell align="center">
                      <Button
                        size="small"
                        color="primary"
                        startIcon={<EditIcon />}
                        onClick={() => handleOpenDialog(user)}
                        sx={{ mr: 1 }}
                      >
                        Edit
                      </Button>
                      <Button
                        size="small"
                        color="info"
                        startIcon={<LockIcon />}
                        onClick={() => handleOpenPasswordDialog(user.id)}
                        sx={{ mr: 1 }}
                      >
                        Reset
                      </Button>
                      <Button
                        size="small"
                        color={user.isActive ? 'warning' : 'success'}
                        onClick={() => handleToggleActive(user)}
                        sx={{ mr: 1 }}
                      >
                        {user.isActive ? 'Deactivate' : 'Activate'}
                      </Button>
                      <Button
                        size="small"
                        color="error"
                        startIcon={<DeleteIcon />}
                        onClick={() => handleDeleteUser(user.id)}
                      >
                        Delete
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
            {users.length === 0 && (
              <Typography sx={{ textAlign: 'center', py: 2, color: 'textSecondary' }}>
                No users found
              </Typography>
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Add/Edit User Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{editingId ? 'Edit User' : 'Create New User'}</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <TextField
            autoFocus
            fullWidth
            label="First Name"
            name="firstName"
            value={formData.firstName}
            onChange={handleInputChange}
            margin="normal"
            required
          />
          <TextField
            fullWidth
            label="Last Name"
            name="lastName"
            value={formData.lastName}
            onChange={handleInputChange}
            margin="normal"
            required
          />
          <TextField
            fullWidth
            label="Username"
            name="username"
            value={formData.username}
            onChange={handleInputChange}
            margin="normal"
            required
            disabled={!!editingId}
          />
          <TextField
            fullWidth
            label="Email"
            name="email"
            type="email"
            value={formData.email}
            onChange={handleInputChange}
            margin="normal"
            required
          />
          {!editingId && (
            <TextField
              fullWidth
              label="Password"
              name="password"
              type="password"
              value={formData.password}
              onChange={handleInputChange}
              margin="normal"
              required
            />
          )}
          <FormControl fullWidth margin="normal">
            <InputLabel>Role</InputLabel>
            <Select
              name="role"
              value={formData.role}
              onChange={(e) => handleSelectChange('role', e.target.value)}
              label="Role"
            >
              {roleOptions.map(option => (
                <MenuItem key={option.value} value={option.value}>{option.label}</MenuItem>
              ))}
            </Select>
          </FormControl>
          <FormControl fullWidth margin="normal">
            <InputLabel>Department</InputLabel>
            <Select
              name="departmentId"
              value={formData.departmentId || ''}
              onChange={(e) => handleSelectChange('departmentId', e.target.value || undefined)}
              label="Department"
            >
              <MenuItem value="">None</MenuItem>
              {departments.map(dept => (
                <MenuItem key={dept.id} value={dept.id}>{dept.name}</MenuItem>
              ))}
            </Select>
          </FormControl>
          <FormControl fullWidth margin="normal">
            <InputLabel>User Profile</InputLabel>
            <Select
              name="userProfileId"
              value={formData.userProfileId || ''}
              onChange={(e) => handleSelectChange('userProfileId', e.target.value || undefined)}
              label="User Profile"
            >
              <MenuItem value="">None</MenuItem>
              {profiles.map(profile => (
                <MenuItem key={profile.id} value={profile.id}>{profile.name}</MenuItem>
              ))}
            </Select>
          </FormControl>
          <Typography variant="caption" sx={{ mt: 2, display: 'block', color: 'textSecondary' }}>
            Note: User details can be linked to a contact through the contacts page.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSaveUser} variant="contained" color="primary">
            {editingId ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Password Reset Dialog */}
      <Dialog open={openPasswordDialog} onClose={handleClosePasswordDialog} maxWidth="sm" fullWidth>
        <DialogTitle>Reset User Password</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <TextField
            autoFocus
            fullWidth
            label="New Password"
            type="password"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            margin="normal"
            required
          />
          <TextField
            fullWidth
            label="Confirm Password"
            type="password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            margin="normal"
            required
          />
          <Typography variant="caption" sx={{ mt: 2, display: 'block', color: 'textSecondary' }}>
            Password must be at least 6 characters long.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClosePasswordDialog}>Cancel</Button>
          <Button onClick={handleResetPassword} variant="contained" color="primary">
            Reset Password
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default UserManagementPage;

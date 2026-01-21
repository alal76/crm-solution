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
  Grid,
  Chip,
  IconButton,
  Alert,
  CircularProgress,
  Avatar,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  TableContainer,
  Paper,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Person as PersonIcon,
  AdminPanelSettings as AdminIcon,
  Visibility as ViewIcon,
  VisibilityOff as HideIcon,
} from '@mui/icons-material';
import { getApiEndpoint } from '../../config/ports';

interface User {
  id: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  isActive: boolean;
  emailVerified: boolean;
  departmentId: number | null;
  departmentName: string | null;
  userProfileId: number | null;
  userProfileName: string | null;
  primaryGroupId: number | null;
  lastLoginDate: string | null;
  createdAt: string;
}

interface UserGroup {
  id: number;
  name: string;
}

interface Department {
  id: number;
  name: string;
}

interface UserFormData {
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  password: string;
  role: string;
  isActive: boolean;
  departmentId: number | null;
  primaryGroupId: number | null;
}

const defaultFormData: UserFormData = {
  username: '',
  email: '',
  firstName: '',
  lastName: '',
  password: '',
  role: 'User',
  isActive: true,
  departmentId: null,
  primaryGroupId: null,
};

const roleToInt = (role: string): number => {
  switch (role) {
    case 'Admin': return 0;
    case 'Manager': return 1;
    case 'Sales': return 2;
    case 'Support': return 3;
    case 'Guest': return 4;
    default: return 2; // Default to Sales
  }
};

function UserManagementTab() {
  const [users, setUsers] = useState<User[]>([]);
  const [groups, setGroups] = useState<UserGroup[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [formData, setFormData] = useState<UserFormData>(defaultFormData);
  const [showPassword, setShowPassword] = useState(false);
  
  const getToken = () => localStorage.getItem('accessToken');

  const fetchUsers = async () => {
    const token = getToken();
    try {
      setLoading(true);
      const response = await fetch(getApiEndpoint('/users'), {
        headers: { 'Authorization': `Bearer ${token}` },
      });
      
      if (!response.ok) throw new Error('Failed to fetch users');
      
      const data = await response.json();
      setUsers(data);
      setError(null);
    } catch (err) {
      setError('Failed to load users');
      console.error('Error fetching users:', err);
    } finally {
      setLoading(false);
    }
  };

  const fetchGroups = async () => {
    const token = getToken();
    try {
      const response = await fetch(getApiEndpoint('/usergroups'), {
        headers: { 'Authorization': `Bearer ${token}` },
      });
      if (response.ok) {
        const data = await response.json();
        setGroups(data);
      }
    } catch (err) {
      console.error('Error fetching groups:', err);
    }
  };

  const fetchDepartments = async () => {
    const token = getToken();
    try {
      const response = await fetch(getApiEndpoint('/departments'), {
        headers: { 'Authorization': `Bearer ${token}` },
      });
      if (response.ok) {
        const data = await response.json();
        setDepartments(data);
      }
    } catch (err) {
      console.error('Error fetching departments:', err);
    }
  };

  useEffect(() => {
    fetchUsers();
    fetchGroups();
    fetchDepartments();
  }, []);

  const handleOpenDialog = (user?: User) => {
    if (user) {
      setEditingUser(user);
      setFormData({
        username: user.username,
        email: user.email,
        firstName: user.firstName,
        lastName: user.lastName,
        password: '',
        role: user.role,
        isActive: user.isActive,
        departmentId: user.departmentId,
        primaryGroupId: user.primaryGroupId,
      });
    } else {
      setEditingUser(null);
      setFormData(defaultFormData);
    }
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setEditingUser(null);
    setShowPassword(false);
  };

  const handleSaveUser = async () => {
    const token = getToken();
    try {
      const url = editingUser 
        ? getApiEndpoint(`/users/${editingUser.id}`) 
        : getApiEndpoint('/auth/register');
      const method = editingUser ? 'PUT' : 'POST';
      
      const payload = editingUser 
        ? {
            firstName: formData.firstName,
            lastName: formData.lastName,
            role: roleToInt(formData.role),
            isActive: formData.isActive,
            departmentId: formData.departmentId,
            primaryGroupId: formData.primaryGroupId,
            ...(formData.password ? { password: formData.password } : {}),
          }
        : {
            username: formData.username,
            email: formData.email,
            password: formData.password,
            firstName: formData.firstName,
            lastName: formData.lastName,
          };
      
      const response = await fetch(url, {
        method,
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      });
      
      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to save user');
      }
      
      setSuccess(editingUser ? 'User updated successfully' : 'User created successfully');
      handleCloseDialog();
      fetchUsers();
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(err.message || 'Failed to save user');
      setTimeout(() => setError(null), 5000);
    }
  };

  const handleDeleteUser = async (userId: number) => {
    if (!window.confirm('Are you sure you want to delete this user?')) return;
    
    const token = getToken();
    try {
      const response = await fetch(getApiEndpoint(`/users/${userId}`), {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` },
      });
      
      if (!response.ok) throw new Error('Failed to delete user');
      
      setSuccess('User deleted successfully');
      fetchUsers();
      setTimeout(() => setSuccess(null), 3000);
    } catch (err) {
      setError('Failed to delete user');
      setTimeout(() => setError(null), 5000);
    }
  };

  const handleToggleActive = async (user: User) => {
    const token = getToken();
    try {
      const response = await fetch(getApiEndpoint(`/users/${user.id}`), {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          ...user,
          isActive: !user.isActive,
        }),
      });
      
      if (!response.ok) throw new Error('Failed to update user');
      
      fetchUsers();
    } catch (err) {
      setError('Failed to update user status');
      setTimeout(() => setError(null), 5000);
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h6" sx={{ fontWeight: 600 }}>
          User Management
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenDialog()}
          sx={{ textTransform: 'none' }}
        >
          Add User
        </Button>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}

      <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
        <Table>
          <TableHead>
            <TableRow sx={{ backgroundColor: '#f5f5f5' }}>
              <TableCell>User</TableCell>
              <TableCell>Email</TableCell>
              <TableCell>Role</TableCell>
              <TableCell>Department</TableCell>
              <TableCell>Group</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Last Login</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {users.map((user) => (
              <TableRow key={user.id} hover>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                    <Avatar sx={{ width: 36, height: 36, bgcolor: user.role === 'Admin' ? '#6750A4' : '#9e9e9e' }}>
                      {user.role === 'Admin' ? <AdminIcon fontSize="small" /> : <PersonIcon fontSize="small" />}
                    </Avatar>
                    <Box>
                      <Typography variant="body2" sx={{ fontWeight: 500 }}>
                        {user.firstName} {user.lastName}
                      </Typography>
                      <Typography variant="caption" color="textSecondary">
                        @{user.username}
                      </Typography>
                    </Box>
                  </Box>
                </TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                    {user.email}
                    {user.emailVerified && (
                      <Chip label="Verified" size="small" color="success" sx={{ ml: 1, height: 20 }} />
                    )}
                  </Box>
                </TableCell>
                <TableCell>
                  <Chip 
                    label={user.role} 
                    size="small" 
                    color={user.role === 'Admin' ? 'primary' : 'default'}
                    sx={{ fontWeight: 500 }}
                  />
                </TableCell>
                <TableCell>{user.departmentName || '-'}</TableCell>
                <TableCell>
                  {groups.find(g => g.id === user.primaryGroupId)?.name || '-'}
                </TableCell>
                <TableCell>
                  <Chip 
                    label={user.isActive ? 'Active' : 'Inactive'} 
                    size="small" 
                    color={user.isActive ? 'success' : 'default'}
                    onClick={() => handleToggleActive(user)}
                    sx={{ cursor: 'pointer' }}
                  />
                </TableCell>
                <TableCell>
                  {user.lastLoginDate 
                    ? new Date(user.lastLoginDate).toLocaleDateString()
                    : 'Never'
                  }
                </TableCell>
                <TableCell align="right">
                  <IconButton size="small" onClick={() => handleOpenDialog(user)} title="Edit">
                    <EditIcon fontSize="small" />
                  </IconButton>
                  <IconButton 
                    size="small" 
                    onClick={() => handleDeleteUser(user.id)} 
                    title="Delete"
                    color="error"
                  >
                    <DeleteIcon fontSize="small" />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
            {users.length === 0 && (
              <TableRow>
                <TableCell colSpan={8} align="center" sx={{ py: 4 }}>
                  <Typography color="textSecondary">No users found</Typography>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* User Dialog */}
      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>
          {editingUser ? 'Edit User' : 'Create New User'}
        </DialogTitle>
        <DialogContent dividers>
          <Grid container spacing={2} sx={{ pt: 1 }}>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Username"
                value={formData.username}
                onChange={(e) => setFormData({ ...formData, username: e.target.value })}
                disabled={!!editingUser}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Email"
                type="email"
                value={formData.email}
                onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                disabled={!!editingUser}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="First Name"
                value={formData.firstName}
                onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Last Name"
                value={formData.lastName}
                onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label={editingUser ? 'New Password (leave blank to keep current)' : 'Password'}
                type={showPassword ? 'text' : 'password'}
                value={formData.password}
                onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                required={!editingUser}
                InputProps={{
                  endAdornment: (
                    <IconButton onClick={() => setShowPassword(!showPassword)} edge="end">
                      {showPassword ? <HideIcon /> : <ViewIcon />}
                    </IconButton>
                  ),
                }}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Role</InputLabel>
                <Select
                  value={formData.role}
                  label="Role"
                  onChange={(e) => setFormData({ ...formData, role: e.target.value })}
                >
                  <MenuItem value="User">User</MenuItem>
                  <MenuItem value="Manager">Manager</MenuItem>
                  <MenuItem value="Admin">Admin</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Department</InputLabel>
                <Select
                  value={formData.departmentId || ''}
                  label="Department"
                  onChange={(e) => setFormData({ ...formData, departmentId: e.target.value as number || null })}
                >
                  <MenuItem value="">None</MenuItem>
                  {departments.map((dept) => (
                    <MenuItem key={dept.id} value={dept.id}>{dept.name}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Primary Group</InputLabel>
                <Select
                  value={formData.primaryGroupId || ''}
                  label="Primary Group"
                  onChange={(e) => setFormData({ ...formData, primaryGroupId: e.target.value as number || null })}
                >
                  <MenuItem value="">None</MenuItem>
                  {groups.map((group) => (
                    <MenuItem key={group.id} value={group.id}>{group.name}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
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
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSaveUser} variant="contained" color="primary">
            {editingUser ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default UserManagementTab;

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
  Divider,
  Autocomplete,
  Collapse,
  Stack,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Person as PersonIcon,
  AdminPanelSettings as AdminIcon,
  Visibility as ViewIcon,
  VisibilityOff as HideIcon,
  Link as LinkIcon,
  LinkOff as UnlinkIcon,
  PersonAdd as PersonAddIcon,
  ContactMail as ContactMailIcon,
  FilterList as FilterIcon,
  Close as CloseIcon,
  CheckBox as CheckBoxIcon,
  CheckBoxOutlineBlank as CheckBoxOutlineBlankIcon,
} from '@mui/icons-material';
import Checkbox from '@mui/material/Checkbox';
import Toolbar from '@mui/material/Toolbar';
import { getApiEndpoint } from '../../config/ports';
import { DialogError, ActionButton } from '../common';
import { useApiState } from '../../hooks/useApiState';

interface Contact {
  id: number;
  firstName: string;
  middleName?: string;
  lastName: string;
  emailPrimary?: string;
  phonePrimary?: string;
  jobTitle?: string;
  company?: string;
}

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
  primaryGroupName: string | null;
  contactId: number | null;
  contact?: Contact;
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
  password: string;
  role: string;
  isActive: boolean;
  departmentId: number | null;
  primaryGroupId: number | null;
  contactId: number | null;
}

const defaultFormData: UserFormData = {
  username: '',
  password: '',
  role: 'User',
  isActive: true,
  departmentId: null,
  primaryGroupId: null,
  contactId: null,
};

interface ContactFormData {
  firstName: string;
  middleName: string;
  lastName: string;
  email: string;
  phone: string;
  title: string;
}

const defaultContactFormData: ContactFormData = {
  firstName: '',
  middleName: '',
  lastName: '',
  email: '',
  phone: '',
  title: '',
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
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [formData, setFormData] = useState<UserFormData>(defaultFormData);
  const [showPassword, setShowPassword] = useState(false);
  
  // Contact linking state
  const [selectedContact, setSelectedContact] = useState<Contact | null>(null);
  const [showContactDialog, setShowContactDialog] = useState(false);
  const [contactFormData, setContactFormData] = useState<ContactFormData>(defaultContactFormData);
  const [contactSearchQuery, setContactSearchQuery] = useState('');
  const [creatingContact, setCreatingContact] = useState(false);
  
  // Multi-select and filtering state
  const [selectedUserIds, setSelectedUserIds] = useState<number[]>([]);
  const [showFilters, setShowFilters] = useState(false);
  const [filters, setFilters] = useState({
    role: '' as string,
    departmentId: '' as string | number,
    groupId: '' as string | number,
    status: '' as string,
    search: '' as string,
  });
  const [bulkDialogOpen, setBulkDialogOpen] = useState(false);
  const [bulkFormData, setBulkFormData] = useState({
    role: '' as string,
    departmentId: '' as string | number,
    groupId: '' as string | number,
    isActive: '' as string,
  });
  
  // API state hooks for dialog error handling
  const dialogApi = useApiState();
  const bulkApi = useApiState();
  const contactApi = useApiState();
  
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

  const fetchContacts = async () => {
    const token = getToken();
    try {
      const response = await fetch(getApiEndpoint('/contacts'), {
        headers: { 'Authorization': `Bearer ${token}` },
      });
      if (response.ok) {
        const data = await response.json();
        setContacts(data);
      }
    } catch (err) {
      console.error('Error fetching contacts:', err);
    }
  };

  useEffect(() => {
    fetchUsers();
    fetchGroups();
    fetchDepartments();
    fetchContacts();
  }, []);

  const handleOpenDialog = (user?: User) => {
    if (user) {
      setEditingUser(user);
      setFormData({
        username: user.username,
        password: '',
        role: user.role,
        isActive: user.isActive,
        departmentId: user.departmentId,
        primaryGroupId: user.primaryGroupId,
        contactId: user.contactId,
      });
      // Find linked contact
      if (user.contactId) {
        const linkedContact = contacts.find(c => c.id === user.contactId);
        setSelectedContact(linkedContact || null);
      } else {
        setSelectedContact(null);
      }
    } else {
      setEditingUser(null);
      setFormData(defaultFormData);
      setSelectedContact(null);
    }
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setEditingUser(null);
    setShowPassword(false);
    setSelectedContact(null);
    dialogApi.clearError();
  };

  const handleSaveUser = async () => {
    await dialogApi.execute(async () => {
      const token = getToken();
      const url = editingUser 
        ? getApiEndpoint(`/users/${editingUser.id}`) 
        : getApiEndpoint('/auth/register');
      const method = editingUser ? 'PUT' : 'POST';
      
      // Get name/email from linked contact or use defaults
      const firstName = selectedContact?.firstName || 'New';
      const lastName = selectedContact?.lastName || 'User';
      const email = selectedContact?.emailPrimary || `${formData.username}@system.local`;
      
      const payload = editingUser 
        ? {
            role: roleToInt(formData.role),
            isActive: formData.isActive,
            departmentId: formData.departmentId,
            primaryGroupId: formData.primaryGroupId,
            contactId: selectedContact?.id || null,
            ...(formData.password ? { password: formData.password } : {}),
          }
        : {
            username: formData.username,
            email: email,
            password: formData.password,
            confirmPassword: formData.password,
            firstName: firstName,
            lastName: lastName,
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
      
      // If creating new user and contact is selected, link them
      if (!editingUser && selectedContact) {
        // Get the created user and link contact
        const result = await response.json();
        if (result.user?.id) {
          await linkContactToUser(result.user.id, selectedContact.id);
        }
      }
      
      setSuccess(editingUser ? 'User updated successfully' : 'User created successfully');
      handleCloseDialog();
      fetchUsers();
      setTimeout(() => setSuccess(null), 3000);
    });
  };

  const linkContactToUser = async (userId: number, contactId: number) => {
    const token = getToken();
    try {
      const response = await fetch(getApiEndpoint(`/users/${userId}/link-contact`), {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ contactId }),
      });
      
      if (!response.ok) {
        console.error('Failed to link contact');
      }
    } catch (err) {
      console.error('Error linking contact:', err);
    }
  };

  const handleUnlinkContact = () => {
    setSelectedContact(null);
    setFormData({ ...formData, contactId: null });
  };

  const handleOpenContactDialog = () => {
    setContactFormData(defaultContactFormData);
    setShowContactDialog(true);
  };

  const handleCloseContactDialog = () => {
    setShowContactDialog(false);
  };

  const handleCreateAndLinkContact = async () => {
    const token = getToken();
    setCreatingContact(true);
    try {
      // Create new contact
      const response = await fetch(getApiEndpoint('/contacts'), {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          firstName: contactFormData.firstName,
          middleName: contactFormData.middleName || null,
          lastName: contactFormData.lastName,
          emailPrimary: contactFormData.email || null,
          phonePrimary: contactFormData.phone || null,
          jobTitle: contactFormData.title || null,
          contactType: 'Employee',
        }),
      });
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('Create contact error:', errorText);
        throw new Error('Failed to create contact');
      }
      
      const newContact = await response.json();
      console.log('Created contact:', newContact);
      
      // Close the contact dialog first
      setShowContactDialog(false);
      
      // Then update the state
      setSelectedContact(newContact);
      setFormData(prev => ({ ...prev, contactId: newContact.id }));
      
      // Refresh contacts list
      fetchContacts();
      setSuccess('Contact created and linked');
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      console.error('Error in handleCreateAndLinkContact:', err);
      setError(err.message || 'Failed to create contact');
      setTimeout(() => setError(null), 5000);
    } finally {
      setCreatingContact(false);
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

  // Filter users based on filter criteria
  const filteredUsers = users.filter(user => {
    if (filters.role && user.role !== filters.role) return false;
    if (filters.departmentId && user.departmentId !== Number(filters.departmentId)) return false;
    if (filters.groupId && user.primaryGroupId !== Number(filters.groupId)) return false;
    if (filters.status === 'active' && !user.isActive) return false;
    if (filters.status === 'inactive' && user.isActive) return false;
    if (filters.search) {
      const searchLower = filters.search.toLowerCase();
      const linkedContact = contacts.find(c => c.id === user.contactId);
      const contactName = linkedContact 
        ? `${linkedContact.firstName} ${linkedContact.middleName || ''} ${linkedContact.lastName}`.toLowerCase()
        : '';
      const matches = 
        user.username.toLowerCase().includes(searchLower) ||
        user.email?.toLowerCase().includes(searchLower) ||
        contactName.includes(searchLower);
      if (!matches) return false;
    }
    return true;
  });

  // Selection handlers
  const handleSelectAll = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.checked) {
      setSelectedUserIds(filteredUsers.map(u => u.id));
    } else {
      setSelectedUserIds([]);
    }
  };

  const handleSelectUser = (userId: number) => {
    setSelectedUserIds(prev => 
      prev.includes(userId) 
        ? prev.filter(id => id !== userId)
        : [...prev, userId]
    );
  };

  const isAllSelected = filteredUsers.length > 0 && selectedUserIds.length === filteredUsers.length;
  const isSomeSelected = selectedUserIds.length > 0 && selectedUserIds.length < filteredUsers.length;

  // Bulk update handler
  const handleBulkUpdate = async () => {
    await bulkApi.execute(async () => {
      const token = getToken();
      const updatePromises = selectedUserIds.map(async (userId) => {
        const user = users.find(u => u.id === userId);
        if (!user) return;
        
        const payload: any = {};
        if (bulkFormData.role) payload.role = roleToInt(bulkFormData.role);
        if (bulkFormData.departmentId) payload.departmentId = Number(bulkFormData.departmentId);
        if (bulkFormData.groupId) payload.primaryGroupId = Number(bulkFormData.groupId);
        if (bulkFormData.isActive !== '') payload.isActive = bulkFormData.isActive === 'true';
        
        if (Object.keys(payload).length === 0) return;
        
        const response = await fetch(getApiEndpoint(`/users/${userId}`), {
          method: 'PUT',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(payload),
        });
        if (!response.ok) throw new Error(`Failed to update user ${userId}`);
        return response.ok;
      });

      await Promise.all(updatePromises);
      setSuccess(`Successfully updated ${selectedUserIds.length} users`);
      setBulkDialogOpen(false);
      setBulkFormData({ role: '', departmentId: '', groupId: '', isActive: '' });
      setSelectedUserIds([]);
      fetchUsers();
      setTimeout(() => setSuccess(null), 3000);
    });
  };

  const handleClearFilters = () => {
    setFilters({ role: '', departmentId: '', groupId: '', status: '', search: '' });
  };

  const hasActiveFilters = filters.role || filters.departmentId || filters.groupId || filters.status || filters.search;

  // Get available contacts (not already linked to other users)
  const getAvailableContacts = () => {
    const linkedContactIds = users
      .filter(u => u.contactId && (!editingUser || u.id !== editingUser.id))
      .map(u => u.contactId);
    return contacts.filter(c => !linkedContactIds.includes(c.id));
  };

  // Format contact display name
  const getContactDisplayName = (contact: Contact) => {
    const middleName = contact.middleName ? ` ${contact.middleName}` : '';
    return `${contact.firstName}${middleName} ${contact.lastName}${contact.emailPrimary ? ` (${contact.emailPrimary})` : ''}`;
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
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="h6" sx={{ fontWeight: 600 }}>
          User Management
        </Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            variant={showFilters ? 'contained' : 'outlined'}
            startIcon={<FilterIcon />}
            onClick={() => setShowFilters(!showFilters)}
            sx={{ textTransform: 'none' }}
            color={hasActiveFilters ? 'primary' : 'inherit'}
          >
            Filters {hasActiveFilters && `(${[filters.role, filters.departmentId, filters.groupId, filters.status, filters.search].filter(Boolean).length})`}
          </Button>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleOpenDialog()}
            sx={{ textTransform: 'none' }}
          >
            Add User
          </Button>
        </Box>
      </Box>

      {/* Filter Panel */}
      {showFilters && (
        <Paper sx={{ p: 2, mb: 2, borderRadius: 2 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
              Filter Users
            </Typography>
            {hasActiveFilters && (
              <Button size="small" onClick={handleClearFilters} startIcon={<CloseIcon />}>
                Clear Filters
              </Button>
            )}
          </Box>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6} md={2.4}>
              <TextField
                fullWidth
                size="small"
                label="Search"
                placeholder="Username, name, email..."
                value={filters.search}
                onChange={(e) => setFilters({ ...filters, search: e.target.value })}
              />
            </Grid>
            <Grid item xs={12} sm={6} md={2.4}>
              <FormControl fullWidth size="small">
                <InputLabel>Role</InputLabel>
                <Select
                  value={filters.role}
                  label="Role"
                  onChange={(e) => setFilters({ ...filters, role: e.target.value })}
                >
                  <MenuItem value="">All Roles</MenuItem>
                  <MenuItem value="Admin">Admin</MenuItem>
                  <MenuItem value="Manager">Manager</MenuItem>
                  <MenuItem value="User">User</MenuItem>
                  <MenuItem value="Sales">Sales</MenuItem>
                  <MenuItem value="Support">Support</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6} md={2.4}>
              <FormControl fullWidth size="small">
                <InputLabel>Department</InputLabel>
                <Select
                  value={filters.departmentId}
                  label="Department"
                  onChange={(e) => setFilters({ ...filters, departmentId: e.target.value })}
                >
                  <MenuItem value="">All Departments</MenuItem>
                  {departments.map((dept) => (
                    <MenuItem key={dept.id} value={dept.id}>{dept.name}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6} md={2.4}>
              <FormControl fullWidth size="small">
                <InputLabel>Group</InputLabel>
                <Select
                  value={filters.groupId}
                  label="Group"
                  onChange={(e) => setFilters({ ...filters, groupId: e.target.value })}
                >
                  <MenuItem value="">All Groups</MenuItem>
                  {groups.map((group) => (
                    <MenuItem key={group.id} value={group.id}>{group.name}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6} md={2.4}>
              <FormControl fullWidth size="small">
                <InputLabel>Status</InputLabel>
                <Select
                  value={filters.status}
                  label="Status"
                  onChange={(e) => setFilters({ ...filters, status: e.target.value })}
                >
                  <MenuItem value="">All Status</MenuItem>
                  <MenuItem value="active">Active</MenuItem>
                  <MenuItem value="inactive">Inactive</MenuItem>
                </Select>
              </FormControl>
            </Grid>
          </Grid>
        </Paper>
      )}

      {/* Bulk Actions Toolbar */}
      {selectedUserIds.length > 0 && (
        <Toolbar
          sx={{
            pl: 2,
            pr: 1,
            mb: 2,
            borderRadius: 2,
            backgroundColor: '#e3f2fd',
            display: 'flex',
            justifyContent: 'space-between',
          }}
        >
          <Typography color="primary" variant="subtitle1" sx={{ fontWeight: 600 }}>
            {selectedUserIds.length} user{selectedUserIds.length > 1 ? 's' : ''} selected
          </Typography>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button
              size="small"
              variant="contained"
              onClick={() => setBulkDialogOpen(true)}
              sx={{ textTransform: 'none' }}
            >
              Bulk Update
            </Button>
            <Button
              size="small"
              variant="outlined"
              onClick={() => setSelectedUserIds([])}
              sx={{ textTransform: 'none' }}
            >
              Clear Selection
            </Button>
          </Box>
        </Toolbar>
      )}

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}

      <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
        <Table>
          <TableHead>
            <TableRow sx={{ backgroundColor: '#f5f5f5' }}>
              <TableCell padding="checkbox">
                <Checkbox
                  indeterminate={isSomeSelected}
                  checked={isAllSelected}
                  onChange={handleSelectAll}
                />
              </TableCell>
              <TableCell>User</TableCell>
              <TableCell>Contact</TableCell>
              <TableCell>Role</TableCell>
              <TableCell>Department</TableCell>
              <TableCell>Group</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Last Login</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredUsers.map((user) => {
              const linkedContact = contacts.find(c => c.id === user.contactId);
              const displayName = linkedContact 
                ? `${linkedContact.firstName}${linkedContact.middleName ? ' ' + linkedContact.middleName : ''} ${linkedContact.lastName}`
                : user.firstName && user.lastName 
                  ? `${user.firstName} ${user.lastName}`
                  : user.username;
              const isSelected = selectedUserIds.includes(user.id);
              
              return (
                <TableRow 
                  key={user.id} 
                  hover
                  selected={isSelected}
                  sx={{ '&.Mui-selected': { backgroundColor: '#e3f2fd' } }}
                >
                  <TableCell padding="checkbox">
                    <Checkbox
                      checked={isSelected}
                      onChange={() => handleSelectUser(user.id)}
                    />
                  </TableCell>
                  <TableCell>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                      <Avatar sx={{ width: 36, height: 36, bgcolor: user.role === 'Admin' ? '#6750A4' : '#9e9e9e' }}>
                        {user.role === 'Admin' ? <AdminIcon fontSize="small" /> : <PersonIcon fontSize="small" />}
                      </Avatar>
                      <Box>
                        <Typography variant="body2" sx={{ fontWeight: 500 }}>
                          @{user.username}
                        </Typography>
                      </Box>
                    </Box>
                  </TableCell>
                  <TableCell>
                    {linkedContact ? (
                      <Box>
                        <Typography variant="body2" sx={{ fontWeight: 500 }}>
                          {displayName}
                        </Typography>
                        <Typography variant="caption" color="textSecondary">
                          {linkedContact.emailPrimary || 'No email'}
                        </Typography>
                      </Box>
                    ) : (
                      <Chip label="Not linked" size="small" variant="outlined" />
                    )}
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
                  <TableCell>{user.primaryGroupName || '-'}</TableCell>
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
              );
            })}
            {filteredUsers.length === 0 && (
              <TableRow>
                <TableCell colSpan={9} align="center" sx={{ py: 4 }}>
                  <Typography color="textSecondary">
                    {users.length === 0 ? 'No users found' : 'No users match the current filters'}
                  </Typography>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Bulk Update Dialog */}
      <Dialog open={bulkDialogOpen} onClose={() => setBulkDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          Bulk Update {selectedUserIds.length} User{selectedUserIds.length > 1 ? 's' : ''}
        </DialogTitle>
        <DialogContent dividers>
          <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
            Select the attributes you want to change. Only fields with a value selected will be updated.
          </Typography>
          <Grid container spacing={2} sx={{ pt: 1 }}>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Role</InputLabel>
                <Select
                  value={bulkFormData.role}
                  label="Role"
                  onChange={(e) => setBulkFormData({ ...bulkFormData, role: e.target.value })}
                >
                  <MenuItem value="">-- No Change --</MenuItem>
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
                  value={bulkFormData.departmentId}
                  label="Department"
                  onChange={(e) => setBulkFormData({ ...bulkFormData, departmentId: e.target.value })}
                >
                  <MenuItem value="">-- No Change --</MenuItem>
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
                  value={bulkFormData.groupId}
                  label="Primary Group"
                  onChange={(e) => setBulkFormData({ ...bulkFormData, groupId: e.target.value })}
                >
                  <MenuItem value="">-- No Change --</MenuItem>
                  {groups.map((group) => (
                    <MenuItem key={group.id} value={group.id}>{group.name}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Status</InputLabel>
                <Select
                  value={bulkFormData.isActive}
                  label="Status"
                  onChange={(e) => setBulkFormData({ ...bulkFormData, isActive: e.target.value })}
                >
                  <MenuItem value="">-- No Change --</MenuItem>
                  <MenuItem value="true">Active</MenuItem>
                  <MenuItem value="false">Inactive</MenuItem>
                </Select>
              </FormControl>
            </Grid>
          </Grid>
          <DialogError error={bulkApi.error} onRetry={() => bulkApi.clearError()} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setBulkDialogOpen(false)} disabled={bulkApi.loading}>Cancel</Button>
          <ActionButton 
            onClick={handleBulkUpdate} 
            variant="contained" 
            color="primary"
            loading={bulkApi.loading}
            disabled={!bulkFormData.role && !bulkFormData.departmentId && !bulkFormData.groupId && bulkFormData.isActive === ''}
          >
            Update {selectedUserIds.length} User{selectedUserIds.length > 1 ? 's' : ''}
          </ActionButton>
        </DialogActions>
      </Dialog>

      {/* User Dialog */}
      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>
          {editingUser ? 'Edit User' : 'Create New User'}
        </DialogTitle>
        <DialogContent dividers>
          <Grid container spacing={2} sx={{ pt: 1 }}>
            {/* Login Credentials Section */}
            <Grid item xs={12}>
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
                Login Credentials
              </Typography>
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Username"
                value={formData.username}
                onChange={(e) => setFormData({ ...formData, username: e.target.value })}
                disabled={!!editingUser}
                required
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

            {/* Role & Organization Section */}
            <Grid item xs={12}>
              <Divider sx={{ my: 1 }} />
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1, mt: 1 }}>
                Role & Organization
              </Typography>
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

            {/* Contact Section */}
            <Grid item xs={12}>
              <Divider sx={{ my: 1 }} />
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1, mt: 1, display: 'flex', alignItems: 'center', gap: 1 }}>
                <ContactMailIcon fontSize="small" />
                Linked Contact
              </Typography>
              <Typography variant="caption" color="textSecondary" sx={{ display: 'block', mb: 2 }}>
                Link this user account to a contact record to manage personal details, email, and other information.
              </Typography>
            </Grid>
            
            {selectedContact ? (
              // Show linked contact details
              <Grid item xs={12}>
                <Card variant="outlined" sx={{ p: 2, backgroundColor: '#f5f5f5' }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                    <Box>
                      <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                        {selectedContact.firstName} {selectedContact.middleName || ''} {selectedContact.lastName}
                      </Typography>
                      {selectedContact.jobTitle && (
                        <Typography variant="body2" color="textSecondary">
                          {selectedContact.jobTitle}
                        </Typography>
                      )}
                      {selectedContact.emailPrimary && (
                        <Typography variant="body2" color="textSecondary">
                          {selectedContact.emailPrimary}
                        </Typography>
                      )}
                      {selectedContact.phonePrimary && (
                        <Typography variant="body2" color="textSecondary">
                          {selectedContact.phonePrimary}
                        </Typography>
                      )}
                    </Box>
                    <Button
                      size="small"
                      color="error"
                      startIcon={<UnlinkIcon />}
                      onClick={handleUnlinkContact}
                    >
                      Unlink
                    </Button>
                  </Box>
                </Card>
              </Grid>
            ) : (
              // Show link/add contact options
              <Grid item xs={12}>
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                  <Autocomplete
                    options={getAvailableContacts()}
                    getOptionLabel={(option) => getContactDisplayName(option)}
                    value={selectedContact}
                    onChange={(_, newValue) => {
                      setSelectedContact(newValue);
                      setFormData({ ...formData, contactId: newValue?.id || null });
                    }}
                    renderInput={(params) => (
                      <TextField
                        {...params}
                        label="Search and Link Existing Contact"
                        placeholder="Type to search contacts..."
                        InputProps={{
                          ...params.InputProps,
                          startAdornment: (
                            <>
                              <LinkIcon sx={{ color: 'action.active', mr: 1 }} />
                              {params.InputProps.startAdornment}
                            </>
                          ),
                        }}
                      />
                    )}
                    renderOption={(props, option) => (
                      <li {...props}>
                        <Box>
                          <Typography variant="body2">
                            {option.firstName} {option.middleName || ''} {option.lastName}
                          </Typography>
                          <Typography variant="caption" color="textSecondary">
                            {option.emailPrimary || 'No email'} {option.jobTitle ? `• ${option.jobTitle}` : ''}
                          </Typography>
                        </Box>
                      </li>
                    )}
                  />
                  
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Divider sx={{ flexGrow: 1 }} />
                    <Typography variant="caption" color="textSecondary">OR</Typography>
                    <Divider sx={{ flexGrow: 1 }} />
                  </Box>
                  
                  <Button
                    variant="outlined"
                    startIcon={<PersonAddIcon />}
                    onClick={handleOpenContactDialog}
                    sx={{ textTransform: 'none' }}
                  >
                    Create New Contact
                  </Button>
                </Box>
              </Grid>
            )}
          </Grid>
          <DialogError error={dialogApi.error} onRetry={() => dialogApi.clearError()} />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog} disabled={dialogApi.loading}>Cancel</Button>
          <ActionButton onClick={handleSaveUser} variant="contained" color="primary" loading={dialogApi.loading}>
            {editingUser ? 'Update' : 'Create'}
          </ActionButton>
        </DialogActions>
      </Dialog>

      {/* Create Contact Dialog */}
      <Dialog open={showContactDialog} onClose={handleCloseContactDialog} maxWidth="sm" fullWidth>
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <PersonAddIcon />
            Create New Contact
          </Box>
        </DialogTitle>
        <DialogContent dividers>
          <Grid container spacing={2} sx={{ pt: 1 }}>
            <Grid item xs={12} sm={4}>
              <TextField
                fullWidth
                label="First Name"
                value={contactFormData.firstName}
                onChange={(e) => setContactFormData({ ...contactFormData, firstName: e.target.value })}
                required
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField
                fullWidth
                label="Middle Name"
                value={contactFormData.middleName}
                onChange={(e) => setContactFormData({ ...contactFormData, middleName: e.target.value })}
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField
                fullWidth
                label="Last Name"
                value={contactFormData.lastName}
                onChange={(e) => setContactFormData({ ...contactFormData, lastName: e.target.value })}
                required
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Email"
                type="email"
                value={contactFormData.email}
                onChange={(e) => setContactFormData({ ...contactFormData, email: e.target.value })}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Phone"
                value={contactFormData.phone}
                onChange={(e) => setContactFormData({ ...contactFormData, phone: e.target.value })}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Title / Position"
                value={contactFormData.title}
                onChange={(e) => setContactFormData({ ...contactFormData, title: e.target.value })}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseContactDialog} disabled={creatingContact}>Cancel</Button>
          <Button 
            onClick={handleCreateAndLinkContact} 
            variant="contained" 
            color="primary"
            disabled={!contactFormData.firstName || !contactFormData.lastName || creatingContact}
          >
            {creatingContact ? 'Creating...' : 'Create & Link'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default UserManagementTab;

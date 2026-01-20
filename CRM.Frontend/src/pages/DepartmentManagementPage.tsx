import { Box, Container, Typography, Card, CardContent, Table, TableBody, TableCell, TableHead, TableRow, CircularProgress, Alert, Button, Dialog, DialogTitle, DialogContent, DialogActions, TextField, FormControlLabel, Checkbox, FormControl, InputLabel, Select, MenuItem, SelectChangeEvent, Chip } from '@mui/material';
import { useState, useEffect } from 'react';
import { Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, AccountTree as TreeIcon } from '@mui/icons-material';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';

interface Department {
  id: number;
  name: string;
  description: string;
  departmentCode: string;
  isActive: boolean;
  parentDepartmentId?: number;
  createdAt: string;
  userCount: number;
}

interface DepartmentForm {
  name: string;
  description: string;
  departmentCode: string;
  isActive: boolean;
  parentDepartmentId?: number;
}

function DepartmentManagementPage() {
  const [departments, setDepartments] = useState<Department[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<DepartmentForm>({
    name: '',
    description: '',
    departmentCode: '',
    isActive: true,
  });

  useEffect(() => {
    fetchDepartments();
  }, []);

  const fetchDepartments = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/departments');
      setDepartments(response.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch departments');
      console.error('Error fetching departments:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = (department?: Department) => {
    if (department) {
      setEditingId(department.id);
      setFormData({
        name: department.name,
        description: department.description,
        departmentCode: department.departmentCode,
        isActive: department.isActive,
        parentDepartmentId: department.parentDepartmentId,
      });
    } else {
      setEditingId(null);
      setFormData({
        name: '',
        description: '',
        departmentCode: '',
        isActive: true,
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, checked, type } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }));
  };

  const handleSelectChange = (e: SelectChangeEvent<number | ''>) => {
    const value = e.target.value;
    setFormData(prev => ({
      ...prev,
      parentDepartmentId: value === '' ? undefined : value as number,
    }));
  };

  const getParentDepartmentName = (parentId?: number): string | null => {
    if (!parentId) return null;
    const parent = departments.find(d => d.id === parentId);
    return parent ? parent.name : null;
  };

  const handleSaveDepartment = async () => {
    if (!formData.name.trim() || !formData.departmentCode.trim()) {
      setError('Please fill in required fields (Name, Department Code)');
      return;
    }

    try {
      if (editingId) {
        await apiClient.put(`/departments/${editingId}`, formData);
        setSuccessMessage('Department updated successfully');
      } else {
        await apiClient.post('/departments', formData);
        setSuccessMessage('Department created successfully');
      }
      handleCloseDialog();
      fetchDepartments();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save department');
      console.error('Error saving department:', err);
    }
  };

  const handleDeleteDepartment = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this department?')) {
      try {
        await apiClient.delete(`/departments/${id}`);
        setSuccessMessage('Department deleted successfully');
        fetchDepartments();
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete department');
        console.error('Error deleting department:', err);
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
            <Typography variant="h4" sx={{ fontWeight: 700 }}>Department Management</Typography>
          </Box>
          <Button
            variant="contained"
            color="primary"
            startIcon={<AddIcon />}
            onClick={() => handleOpenDialog()}
            sx={{ backgroundColor: '#6750A4' }}
          >
            Add Department
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
                  <TableCell><strong>Code</strong></TableCell>
                  <TableCell><strong>Description</strong></TableCell>
                  <TableCell><strong>Parent Department</strong></TableCell>
                  <TableCell><strong>Users</strong></TableCell>
                  <TableCell><strong>Status</strong></TableCell>
                  <TableCell align="center"><strong>Actions</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {departments.map((dept) => {
                  const parentName = getParentDepartmentName(dept.parentDepartmentId);
                  return (
                    <TableRow key={dept.id}>
                      <TableCell>{dept.name}</TableCell>
                      <TableCell>{dept.departmentCode}</TableCell>
                      <TableCell>{dept.description}</TableCell>
                      <TableCell>
                        {parentName ? (
                          <Chip
                            icon={<TreeIcon />}
                            label={parentName}
                            size="small"
                            variant="outlined"
                            color="primary"
                          />
                        ) : (
                          <Typography variant="body2" color="textSecondary">—</Typography>
                        )}
                      </TableCell>
                      <TableCell>{dept.userCount}</TableCell>
                      <TableCell>
                        <Chip
                          label={dept.isActive ? 'Active' : 'Inactive'}
                          size="small"
                          color={dept.isActive ? 'success' : 'default'}
                        />
                      </TableCell>
                      <TableCell align="center">
                        <Button
                          size="small"
                          color="primary"
                          startIcon={<EditIcon />}
                          onClick={() => handleOpenDialog(dept)}
                          sx={{ mr: 1 }}
                        >
                          Edit
                        </Button>
                        <Button
                          size="small"
                          color="error"
                          startIcon={<DeleteIcon />}
                          onClick={() => handleDeleteDepartment(dept.id)}
                        >
                          Delete
                        </Button>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
            {departments.length === 0 && (
              <Typography sx={{ textAlign: 'center', py: 2, color: 'textSecondary' }}>
                No departments found
              </Typography>
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Add/Edit Department Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{editingId ? 'Edit Department' : 'Create New Department'}</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <TextField
            autoFocus
            fullWidth
            label="Department Name"
            name="name"
            value={formData.name}
            onChange={handleInputChange}
            margin="normal"
            required
          />
          <TextField
            fullWidth
            label="Department Code"
            name="departmentCode"
            value={formData.departmentCode}
            onChange={handleInputChange}
            margin="normal"
            required
          />
          <TextField
            fullWidth
            label="Description"
            name="description"
            value={formData.description}
            onChange={handleInputChange}
            margin="normal"
            multiline
            rows={3}
          />
          <FormControl fullWidth margin="normal">
            <InputLabel>Parent Department</InputLabel>
            <Select
              value={formData.parentDepartmentId ?? ''}
              onChange={handleSelectChange}
              label="Parent Department"
            >
              <MenuItem value="">
                <em>None (Top Level)</em>
              </MenuItem>
              {departments
                .filter(d => d.id !== editingId) // Prevent selecting self as parent
                .map(dept => (
                  <MenuItem key={dept.id} value={dept.id}>
                    {dept.name} ({dept.departmentCode})
                  </MenuItem>
                ))}
            </Select>
          </FormControl>
          <FormControlLabel
            control={
              <Checkbox
                name="isActive"
                checked={formData.isActive}
                onChange={handleInputChange}
              />
            }
            label="Active"
            sx={{ mt: 2 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSaveDepartment} variant="contained" color="primary">
            {editingId ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default DepartmentManagementPage;

/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 * Licensed under the GNU Affero General Public License v3.0
 */

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
  TablePagination,
  CircularProgress,
  Alert,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  MenuItem,
  Stack,
  Chip,
  IconButton,
  Tooltip,
  FormControl,
  InputLabel,
  Select,
  Grid,
  Paper,
  Divider,
  Collapse,
  SelectChangeEvent,
} from '@mui/material';
import LookupSelect from '../components/LookupSelect';
import { useState, useEffect, useCallback } from 'react';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import VisibilityIcon from '@mui/icons-material/Visibility';
import RefreshIcon from '@mui/icons-material/Refresh';
import FilterListIcon from '@mui/icons-material/FilterList';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import CancelIcon from '@mui/icons-material/Cancel';
import EscalateIcon from '@mui/icons-material/TrendingUp';
import AssignmentIcon from '@mui/icons-material/Assignment';
import logo from '../assets/logo.png';
import {
  ServiceRequest,
  CreateServiceRequest,
  UpdateServiceRequest,
  ServiceRequestCategory,
  ServiceRequestSubcategory,
  ServiceRequestCustomFieldDefinition,
  ServiceRequestCustomFieldValue,
  ServiceRequestFilter,
  PagedServiceRequestResult,
  ServiceRequestChannel,
  ServiceRequestStatus,
  ServiceRequestPriority,
  CustomFieldType,
  serviceRequestService,
  serviceRequestCategoryService,
  serviceRequestSubcategoryService,
  serviceRequestCustomFieldService,
  customerService,
  Customer,
} from '../services/apiService';
import apiClient from '../services/apiClient';

interface Contact {
  id: number;
  firstName: string;
  lastName: string;
  emailPrimary?: string;
}

interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
}

interface UserGroup {
  id: number;
  name: string;
}

interface Workflow {
  id: number;
  name: string;
}

const CHANNEL_LABELS: { [key: number]: string } = {
  [ServiceRequestChannel.WhatsApp]: 'WhatsApp',
  [ServiceRequestChannel.Email]: 'Email',
  [ServiceRequestChannel.Phone]: 'Phone',
  [ServiceRequestChannel.InPerson]: 'In Person',
  [ServiceRequestChannel.SelfServicePortal]: 'Self Service Portal',
  [ServiceRequestChannel.SocialMedia]: 'Social Media',
  [ServiceRequestChannel.LiveChat]: 'Live Chat',
  [ServiceRequestChannel.API]: 'API',
};

const STATUS_LABELS: { [key: number]: string } = {
  [ServiceRequestStatus.New]: 'New',
  [ServiceRequestStatus.Open]: 'Open',
  [ServiceRequestStatus.InProgress]: 'In Progress',
  [ServiceRequestStatus.PendingCustomer]: 'Pending Customer',
  [ServiceRequestStatus.PendingInternal]: 'Pending Internal',
  [ServiceRequestStatus.Escalated]: 'Escalated',
  [ServiceRequestStatus.Resolved]: 'Resolved',
  [ServiceRequestStatus.Closed]: 'Closed',
  [ServiceRequestStatus.Cancelled]: 'Cancelled',
  [ServiceRequestStatus.OnHold]: 'On Hold',
  [ServiceRequestStatus.Reopened]: 'Reopened',
};

const STATUS_COLORS: { [key: number]: 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' } = {
  [ServiceRequestStatus.New]: 'info',
  [ServiceRequestStatus.Open]: 'primary',
  [ServiceRequestStatus.InProgress]: 'primary',
  [ServiceRequestStatus.PendingCustomer]: 'warning',
  [ServiceRequestStatus.PendingInternal]: 'warning',
  [ServiceRequestStatus.Escalated]: 'error',
  [ServiceRequestStatus.Resolved]: 'success',
  [ServiceRequestStatus.Closed]: 'default',
  [ServiceRequestStatus.Cancelled]: 'default',
  [ServiceRequestStatus.OnHold]: 'warning',
  [ServiceRequestStatus.Reopened]: 'info',
};

const PRIORITY_LABELS: { [key: number]: string } = {
  [ServiceRequestPriority.Low]: 'Low',
  [ServiceRequestPriority.Medium]: 'Medium',
  [ServiceRequestPriority.High]: 'High',
  [ServiceRequestPriority.Critical]: 'Critical',
  [ServiceRequestPriority.Urgent]: 'Urgent',
};

const PRIORITY_COLORS: { [key: number]: 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' } = {
  [ServiceRequestPriority.Low]: 'default',
  [ServiceRequestPriority.Medium]: 'info',
  [ServiceRequestPriority.High]: 'warning',
  [ServiceRequestPriority.Critical]: 'error',
  [ServiceRequestPriority.Urgent]: 'error',
};

function ServiceRequestsPage() {
  const [requests, setRequests] = useState<ServiceRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [showFilters, setShowFilters] = useState(false);
  
  // Filter state
  const [filterStatus, setFilterStatus] = useState<string>('');
  const [filterPriority, setFilterPriority] = useState<string>('');
  const [filterChannel, setFilterChannel] = useState<string>('');
  const [filterCategory, setFilterCategory] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState('');
  
  // Dialog state
  const [openDialog, setOpenDialog] = useState(false);
  const [selectedRequest, setSelectedRequest] = useState<ServiceRequest | null>(null);
  const [viewMode, setViewMode] = useState(false);
  
  // Reference data
  const [categories, setCategories] = useState<ServiceRequestCategory[]>([]);
  const [subcategories, setSubcategories] = useState<ServiceRequestSubcategory[]>([]);
  const [filteredSubcategories, setFilteredSubcategories] = useState<ServiceRequestSubcategory[]>([]);
  const [customFields, setCustomFields] = useState<ServiceRequestCustomFieldDefinition[]>([]);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [groups, setGroups] = useState<UserGroup[]>([]);
  const [workflows, setWorkflows] = useState<Workflow[]>([]);
  
  // Form state
  const [formData, setFormData] = useState<CreateServiceRequest>({
    subject: '',
    description: '',
    channel: ServiceRequestChannel.Email,
    priority: ServiceRequestPriority.Medium,
    customFieldValues: [],
  });
  const [customFieldValues, setCustomFieldValues] = useState<{ [key: number]: string }>({});

  // Action dialogs
  const [resolveDialogOpen, setResolveDialogOpen] = useState(false);
  const [escalateDialogOpen, setEscalateDialogOpen] = useState(false);
  const [assignDialogOpen, setAssignDialogOpen] = useState(false);
  const [actionNotes, setActionNotes] = useState('');
  const [assignToUserId, setAssignToUserId] = useState<number | null>(null);
  const [assignToGroupId, setAssignToGroupId] = useState<number | null>(null);

  // Fetch reference data
  useEffect(() => {
    const fetchReferenceData = async () => {
      try {
        const [catRes, subRes, fieldRes, custRes, contactRes, userRes, groupRes, workflowRes] = await Promise.all([
          serviceRequestCategoryService.getAll(),
          serviceRequestSubcategoryService.getAll(),
          serviceRequestCustomFieldService.getAll(),
          customerService.getAll(),
          apiClient.get('/contacts'),
          apiClient.get('/users'),
          apiClient.get('/user-groups'),
          apiClient.get('/workflows'),
        ]);
        setCategories(catRes.data);
        setSubcategories(subRes.data);
        setCustomFields(fieldRes.data);
        setCustomers(custRes.data);
        setContacts(contactRes.data);
        setUsers(userRes.data);
        setGroups(groupRes.data);
        setWorkflows(workflowRes.data);
      } catch (err) {
        console.error('Error fetching reference data:', err);
      }
    };
    fetchReferenceData();
  }, []);

  // Fetch service requests
  const fetchRequests = useCallback(async () => {
    try {
      setLoading(true);
      const filter: ServiceRequestFilter = {
        page: page + 1,
        pageSize,
        searchTerm: searchTerm || undefined,
        status: filterStatus ? [parseInt(filterStatus)] : undefined,
        priority: filterPriority ? [parseInt(filterPriority)] : undefined,
        channel: filterChannel ? [parseInt(filterChannel)] : undefined,
        categoryId: filterCategory ? parseInt(filterCategory) : undefined,
      };
      const response = await serviceRequestService.getAll(filter);
      setRequests(response.data.items);
      setTotalCount(response.data.totalCount);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch service requests');
      console.error('Error fetching service requests:', err);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, searchTerm, filterStatus, filterPriority, filterChannel, filterCategory]);

  useEffect(() => {
    fetchRequests();
  }, [fetchRequests]);

  // Filter subcategories based on selected category
  useEffect(() => {
    if (formData.categoryId) {
      setFilteredSubcategories(
        subcategories.filter((s) => s.categoryId === formData.categoryId)
      );
    } else {
      setFilteredSubcategories([]);
    }
  }, [formData.categoryId, subcategories]);

  // Load custom fields based on category/subcategory
  useEffect(() => {
    const loadApplicableFields = async () => {
      if (formData.categoryId || formData.subcategoryId) {
        try {
          const response = await serviceRequestCustomFieldService.getApplicable(
            formData.categoryId,
            formData.subcategoryId
          );
          setCustomFields(response.data);
        } catch (err) {
          console.error('Error loading custom fields:', err);
        }
      }
    };
    loadApplicableFields();
  }, [formData.categoryId, formData.subcategoryId]);

  const handleAddRequest = () => {
    setFormData({
      subject: '',
      description: '',
      channel: ServiceRequestChannel.Email,
      priority: ServiceRequestPriority.Medium,
      customFieldValues: [],
    });
    setCustomFieldValues({});
    setSelectedRequest(null);
    setViewMode(false);
    setOpenDialog(true);
  };

  const handleViewRequest = (request: ServiceRequest) => {
    setSelectedRequest(request);
    setViewMode(true);
    // Populate custom field values
    const values: { [key: number]: string } = {};
    request.customFieldValues?.forEach((v) => {
      values[v.fieldDefinitionId] = v.value || '';
    });
    setCustomFieldValues(values);
    setOpenDialog(true);
  };

  const handleEditRequest = (request: ServiceRequest) => {
    setFormData({
      subject: request.subject,
      description: request.description,
      channel: request.channel,
      priority: request.priority,
      categoryId: request.categoryId,
      subcategoryId: request.subcategoryId,
      customerId: request.customerId,
      contactId: request.contactId,
      assignedToUserId: request.assignedToUserId,
      assignedToGroupId: request.assignedToGroupId,
      workflowId: request.workflowId,
    });
    const values: { [key: number]: string } = {};
    request.customFieldValues?.forEach((v) => {
      values[v.fieldDefinitionId] = v.value || '';
    });
    setCustomFieldValues(values);
    setSelectedRequest(request);
    setViewMode(false);
    setOpenDialog(true);
  };

  const handleDeleteRequest = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this service request?')) {
      try {
        await serviceRequestService.delete(id);
        setSuccessMessage('Service request deleted successfully');
        await fetchRequests();
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete service request');
      }
    }
  };

  const handleSaveRequest = async () => {
    try {
      if (!formData.subject) {
        setError('Subject is required');
        return;
      }

      const customFieldValuesArray = Object.entries(customFieldValues)
        .filter(([, value]) => value)
        .map(([fieldId, value]) => ({
          fieldDefinitionId: parseInt(fieldId),
          value,
        }));

      if (selectedRequest) {
        const updateData: UpdateServiceRequest = {
          ...formData,
          customFieldValues: customFieldValuesArray,
        };
        await serviceRequestService.update(selectedRequest.id!, updateData);
        setSuccessMessage('Service request updated successfully');
      } else {
        const createData: CreateServiceRequest = {
          ...formData,
          customFieldValues: customFieldValuesArray,
        };
        await serviceRequestService.create(createData);
        setSuccessMessage('Service request created successfully');
      }

      setOpenDialog(false);
      await fetchRequests();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save service request');
    }
  };

  const handleResolve = async () => {
    if (!selectedRequest || !actionNotes) {
      setError('Resolution notes are required');
      return;
    }
    try {
      await serviceRequestService.resolve(selectedRequest.id!, actionNotes);
      setSuccessMessage('Service request resolved');
      setResolveDialogOpen(false);
      setActionNotes('');
      await fetchRequests();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to resolve service request');
    }
  };

  const handleClose = async (request: ServiceRequest) => {
    try {
      await serviceRequestService.close(request.id!);
      setSuccessMessage('Service request closed');
      await fetchRequests();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to close service request');
    }
  };

  const handleEscalate = async () => {
    if (!selectedRequest || !actionNotes) {
      setError('Escalation reason is required');
      return;
    }
    try {
      await serviceRequestService.escalate(selectedRequest.id!, actionNotes, assignToGroupId || undefined);
      setSuccessMessage('Service request escalated');
      setEscalateDialogOpen(false);
      setActionNotes('');
      setAssignToGroupId(null);
      await fetchRequests();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to escalate service request');
    }
  };

  const handleAssign = async () => {
    if (!selectedRequest) return;
    try {
      if (assignToUserId) {
        await serviceRequestService.assignToUser(selectedRequest.id!, assignToUserId, actionNotes || undefined);
      } else if (assignToGroupId) {
        await serviceRequestService.assignToGroup(selectedRequest.id!, assignToGroupId, actionNotes || undefined);
      }
      setSuccessMessage('Service request assigned');
      setAssignDialogOpen(false);
      setActionNotes('');
      setAssignToUserId(null);
      setAssignToGroupId(null);
      await fetchRequests();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to assign service request');
    }
  };

  const handleFormChange = (field: keyof CreateServiceRequest, value: any) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleCustomFieldChange = (fieldId: number, value: string) => {
    setCustomFieldValues((prev) => ({ ...prev, [fieldId]: value }));
  };

  const renderCustomField = (field: ServiceRequestCustomFieldDefinition) => {
    const value = customFieldValues[field.id!] || '';
    const isDisabled = viewMode;

    switch (field.fieldType) {
      case CustomFieldType.TextArea:
        return (
          <TextField
            key={field.id}
            label={field.label}
            value={value}
            onChange={(e) => handleCustomFieldChange(field.id!, e.target.value)}
            multiline
            rows={3}
            fullWidth
            required={field.isRequired}
            disabled={isDisabled}
            helperText={field.description}
          />
        );
      case CustomFieldType.Number:
      case CustomFieldType.Decimal:
        return (
          <TextField
            key={field.id}
            label={field.label}
            value={value}
            onChange={(e) => handleCustomFieldChange(field.id!, e.target.value)}
            type="number"
            fullWidth
            required={field.isRequired}
            disabled={isDisabled}
            helperText={field.description}
            inputProps={{
              min: field.minValue,
              max: field.maxValue,
              step: field.fieldType === CustomFieldType.Decimal ? 0.01 : 1,
            }}
          />
        );
      case CustomFieldType.Date:
        return (
          <TextField
            key={field.id}
            label={field.label}
            value={value}
            onChange={(e) => handleCustomFieldChange(field.id!, e.target.value)}
            type="date"
            fullWidth
            required={field.isRequired}
            disabled={isDisabled}
            helperText={field.description}
            InputLabelProps={{ shrink: true }}
          />
        );
      case CustomFieldType.DateTime:
        return (
          <TextField
            key={field.id}
            label={field.label}
            value={value}
            onChange={(e) => handleCustomFieldChange(field.id!, e.target.value)}
            type="datetime-local"
            fullWidth
            required={field.isRequired}
            disabled={isDisabled}
            helperText={field.description}
            InputLabelProps={{ shrink: true }}
          />
        );
      case CustomFieldType.Dropdown:
      case CustomFieldType.MultiSelect:
        const options = field.options ? field.options.split(',').map((o) => o.trim()) : [];
        return (
          <FormControl key={field.id} fullWidth required={field.isRequired}>
            <InputLabel>{field.label}</InputLabel>
            <Select
              value={value}
              onChange={(e: SelectChangeEvent<string>) => handleCustomFieldChange(field.id!, e.target.value)}
              label={field.label}
              disabled={isDisabled}
              multiple={field.fieldType === CustomFieldType.MultiSelect}
            >
              {options.map((option) => (
                <MenuItem key={option} value={option}>
                  {option}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        );
      case CustomFieldType.Boolean:
        return (
          <FormControl key={field.id} fullWidth required={field.isRequired}>
            <InputLabel>{field.label}</InputLabel>
            <Select
              value={value}
              onChange={(e: SelectChangeEvent<string>) => handleCustomFieldChange(field.id!, e.target.value)}
              label={field.label}
              disabled={isDisabled}
            >
              <MenuItem value="true">Yes</MenuItem>
              <MenuItem value="false">No</MenuItem>
            </Select>
          </FormControl>
        );
      default:
        return (
          <TextField
            key={field.id}
            label={field.label}
            value={value}
            onChange={(e) => handleCustomFieldChange(field.id!, e.target.value)}
            fullWidth
            required={field.isRequired}
            disabled={isDisabled}
            helperText={field.description}
            type={
              field.fieldType === CustomFieldType.Email
                ? 'email'
                : field.fieldType === CustomFieldType.Phone
                ? 'tel'
                : field.fieldType === CustomFieldType.Url
                ? 'url'
                : 'text'
            }
          />
        );
    }
  };

  const clearFilters = () => {
    setFilterStatus('');
    setFilterPriority('');
    setFilterChannel('');
    setFilterCategory('');
    setSearchTerm('');
  };

  return (
    <Container maxWidth="xl">
      <Box sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
        <img src={logo} alt="CRM Logo" style={{ height: 40 }} />
        <Typography variant="h4" component="h1">
          Service Requests
        </Typography>
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

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Stack direction="row" spacing={2}>
              <TextField
                placeholder="Search..."
                size="small"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                sx={{ minWidth: 250 }}
              />
              <Button
                variant="outlined"
                startIcon={<FilterListIcon />}
                onClick={() => setShowFilters(!showFilters)}
              >
                Filters
              </Button>
              <IconButton onClick={fetchRequests} title="Refresh">
                <RefreshIcon />
              </IconButton>
            </Stack>
            <Button
              variant="contained"
              color="primary"
              startIcon={<AddIcon />}
              onClick={handleAddRequest}
            >
              New Request
            </Button>
          </Box>

          <Collapse in={showFilters}>
            <Paper sx={{ p: 2, mb: 2, bgcolor: 'grey.50' }}>
              <Grid container spacing={2}>
                <Grid item xs={12} sm={6} md={3}>
                  <LookupSelect
                    category="ServiceStatus"
                    name="filterStatus"
                    value={filterStatus}
                    onChange={(e:any) => setFilterStatus(e.target.value)}
                    label="Status"
                    fallback={[{ value: '', label: 'All' }, ...Object.entries(STATUS_LABELS).map(([k, v]) => ({ value: k, label: v }))]}
                  />
                </Grid>
                <Grid item xs={12} sm={6} md={3}>
                  <LookupSelect
                    category="Priority"
                    name="filterPriority"
                    value={filterPriority}
                    onChange={(e:any) => setFilterPriority(e.target.value)}
                    label="Priority"
                    fallback={[{ value: '', label: 'All' }, ...Object.entries(PRIORITY_LABELS).map(([k, v]) => ({ value: k, label: v }))]}
                  />
                </Grid>
                <Grid item xs={12} sm={6} md={3}>
                  <LookupSelect
                    category="ServiceChannel"
                    name="filterChannel"
                    value={filterChannel}
                    onChange={(e:any) => setFilterChannel(e.target.value)}
                    label="Channel"
                    fallback={[{ value: '', label: 'All' }, ...Object.entries(CHANNEL_LABELS).map(([k, v]) => ({ value: k, label: v }))]}
                  />
                </Grid>
                <Grid item xs={12} sm={6} md={3}>
                  <FormControl fullWidth size="small">
                    <InputLabel>Category</InputLabel>
                    <Select
                      value={filterCategory}
                      onChange={(e: SelectChangeEvent<string>) => setFilterCategory(e.target.value)}
                      label="Category"
                    >
                      <MenuItem value="">All</MenuItem>
                      {categories.map((cat) => (
                        <MenuItem key={cat.id} value={cat.id?.toString()}>
                          {cat.name}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Grid>
              </Grid>
              <Box sx={{ mt: 2, textAlign: 'right' }}>
                <Button onClick={clearFilters} size="small">
                  Clear Filters
                </Button>
              </Box>
            </Paper>
          </Collapse>

          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
              <CircularProgress />
            </Box>
          ) : (
            <>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Ticket #</TableCell>
                    <TableCell>Subject</TableCell>
                    <TableCell>Channel</TableCell>
                    <TableCell>Category</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Priority</TableCell>
                    <TableCell>Customer</TableCell>
                    <TableCell>Assigned To</TableCell>
                    <TableCell>Created</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {requests.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={10} align="center">
                        No service requests found
                      </TableCell>
                    </TableRow>
                  ) : (
                    requests.map((request) => (
                      <TableRow key={request.id} hover>
                        <TableCell>
                          <Typography variant="body2" fontWeight="medium">
                            {request.ticketNumber}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2" noWrap sx={{ maxWidth: 200 }}>
                            {request.subject}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={CHANNEL_LABELS[request.channel]}
                            size="small"
                            variant="outlined"
                          />
                        </TableCell>
                        <TableCell>{request.categoryName || '-'}</TableCell>
                        <TableCell>
                          <Chip
                            label={STATUS_LABELS[request.status]}
                            size="small"
                            color={STATUS_COLORS[request.status]}
                          />
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={PRIORITY_LABELS[request.priority]}
                            size="small"
                            color={PRIORITY_COLORS[request.priority]}
                          />
                        </TableCell>
                        <TableCell>{request.customerName || '-'}</TableCell>
                        <TableCell>
                          {request.assignedToUserName || request.assignedToGroupName || '-'}
                        </TableCell>
                        <TableCell>
                          {request.createdAt
                            ? new Date(request.createdAt).toLocaleDateString()
                            : '-'}
                        </TableCell>
                        <TableCell align="right">
                          <Stack direction="row" spacing={0.5} justifyContent="flex-end">
                            <Tooltip title="View">
                              <IconButton
                                size="small"
                                onClick={() => handleViewRequest(request)}
                              >
                                <VisibilityIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Edit">
                              <IconButton
                                size="small"
                                onClick={() => handleEditRequest(request)}
                              >
                                <EditIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Assign">
                              <IconButton
                                size="small"
                                onClick={() => {
                                  setSelectedRequest(request);
                                  setAssignDialogOpen(true);
                                }}
                              >
                                <AssignmentIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                            {request.status !== ServiceRequestStatus.Resolved &&
                              request.status !== ServiceRequestStatus.Closed && (
                                <>
                                  <Tooltip title="Resolve">
                                    <IconButton
                                      size="small"
                                      color="success"
                                      onClick={() => {
                                        setSelectedRequest(request);
                                        setResolveDialogOpen(true);
                                      }}
                                    >
                                      <CheckCircleIcon fontSize="small" />
                                    </IconButton>
                                  </Tooltip>
                                  <Tooltip title="Escalate">
                                    <IconButton
                                      size="small"
                                      color="warning"
                                      onClick={() => {
                                        setSelectedRequest(request);
                                        setEscalateDialogOpen(true);
                                      }}
                                    >
                                      <EscalateIcon fontSize="small" />
                                    </IconButton>
                                  </Tooltip>
                                </>
                              )}
                            {request.status === ServiceRequestStatus.Resolved && (
                              <Tooltip title="Close">
                                <IconButton
                                  size="small"
                                  onClick={() => handleClose(request)}
                                >
                                  <CancelIcon fontSize="small" />
                                </IconButton>
                              </Tooltip>
                            )}
                            <Tooltip title="Delete">
                              <IconButton
                                size="small"
                                color="error"
                                onClick={() => handleDeleteRequest(request.id!)}
                              >
                                <DeleteIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                          </Stack>
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
              <TablePagination
                component="div"
                count={totalCount}
                page={page}
                onPageChange={(_, newPage) => setPage(newPage)}
                rowsPerPage={pageSize}
                onRowsPerPageChange={(e) => {
                  setPageSize(parseInt(e.target.value, 10));
                  setPage(0);
                }}
                rowsPerPageOptions={[10, 25, 50, 100]}
              />
            </>
          )}
        </CardContent>
      </Card>

      {/* Create/Edit/View Dialog */}
      <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          {viewMode
            ? `View Service Request - ${selectedRequest?.ticketNumber}`
            : selectedRequest
            ? 'Edit Service Request'
            : 'New Service Request'}
        </DialogTitle>
        <DialogContent dividers>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <TextField
                label="Subject"
                value={formData.subject}
                onChange={(e) => handleFormChange('subject', e.target.value)}
                fullWidth
                required
                disabled={viewMode}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                label="Description"
                value={formData.description || ''}
                onChange={(e) => handleFormChange('description', e.target.value)}
                multiline
                rows={4}
                fullWidth
                disabled={viewMode}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth required>
                <InputLabel>Channel</InputLabel>
                <Select
                  value={formData.channel}
                  onChange={(e: SelectChangeEvent<number>) =>
                    handleFormChange('channel', e.target.value)
                  }
                  label="Channel"
                  disabled={viewMode}
                >
                  {Object.entries(CHANNEL_LABELS).map(([key, label]) => (
                    <MenuItem key={key} value={parseInt(key)}>
                      {label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Priority</InputLabel>
                <Select
                  value={formData.priority}
                  onChange={(e: SelectChangeEvent<number>) =>
                    handleFormChange('priority', e.target.value)
                  }
                  label="Priority"
                  disabled={viewMode}
                >
                  {Object.entries(PRIORITY_LABELS).map(([key, label]) => (
                    <MenuItem key={key} value={parseInt(key)}>
                      {label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Category</InputLabel>
                <Select
                  value={formData.categoryId || ''}
                  onChange={(e: SelectChangeEvent<string | number>) => {
                    const val = e.target.value ? Number(e.target.value) : undefined;
                    handleFormChange('categoryId', val);
                    handleFormChange('subcategoryId', undefined);
                  }}
                  label="Category"
                  disabled={viewMode}
                >
                  <MenuItem value="">None</MenuItem>
                  {categories.map((cat) => (
                    <MenuItem key={cat.id} value={cat.id}>
                      {cat.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth disabled={!formData.categoryId || viewMode}>
                <InputLabel>Subcategory</InputLabel>
                <Select
                  value={formData.subcategoryId || ''}
                  onChange={(e: SelectChangeEvent<string | number>) =>
                    handleFormChange('subcategoryId', e.target.value ? Number(e.target.value) : undefined)
                  }
                  label="Subcategory"
                >
                  <MenuItem value="">None</MenuItem>
                  {filteredSubcategories.map((sub) => (
                    <MenuItem key={sub.id} value={sub.id}>
                      {sub.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Customer</InputLabel>
                <Select
                  value={formData.customerId || ''}
                  onChange={(e: SelectChangeEvent<string | number>) =>
                    handleFormChange('customerId', e.target.value ? Number(e.target.value) : undefined)
                  }
                  label="Customer"
                  disabled={viewMode}
                >
                  <MenuItem value="">None</MenuItem>
                  {customers.map((cust) => (
                    <MenuItem key={cust.id} value={cust.id}>
                      {cust.firstName} {cust.lastName} - {cust.company}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Contact</InputLabel>
                <Select
                  value={formData.contactId || ''}
                  onChange={(e: SelectChangeEvent<string | number>) =>
                    handleFormChange('contactId', e.target.value ? Number(e.target.value) : undefined)
                  }
                  label="Contact"
                  disabled={viewMode}
                >
                  <MenuItem value="">None</MenuItem>
                  {contacts.map((contact) => (
                    <MenuItem key={contact.id} value={contact.id}>
                      {contact.firstName} {contact.lastName}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Assign to User</InputLabel>
                <Select
                  value={formData.assignedToUserId || ''}
                  onChange={(e: SelectChangeEvent<string | number>) =>
                    handleFormChange('assignedToUserId', e.target.value ? Number(e.target.value) : undefined)
                  }
                  label="Assign to User"
                  disabled={viewMode}
                >
                  <MenuItem value="">None</MenuItem>
                  {users.map((user) => (
                    <MenuItem key={user.id} value={user.id}>
                      {user.firstName} {user.lastName}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Assign to Group</InputLabel>
                <Select
                  value={formData.assignedToGroupId || ''}
                  onChange={(e: SelectChangeEvent<string | number>) =>
                    handleFormChange('assignedToGroupId', e.target.value ? Number(e.target.value) : undefined)
                  }
                  label="Assign to Group"
                  disabled={viewMode}
                >
                  <MenuItem value="">None</MenuItem>
                  {groups.map((group) => (
                    <MenuItem key={group.id} value={group.id}>
                      {group.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>Workflow</InputLabel>
                <Select
                  value={formData.workflowId || ''}
                  onChange={(e: SelectChangeEvent<string | number>) =>
                    handleFormChange('workflowId', e.target.value ? Number(e.target.value) : undefined)
                  }
                  label="Workflow"
                  disabled={viewMode}
                >
                  <MenuItem value="">None</MenuItem>
                  {workflows.map((wf) => (
                    <MenuItem key={wf.id} value={wf.id}>
                      {wf.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            {/* Custom Fields */}
            {customFields.length > 0 && (
              <>
                <Grid item xs={12}>
                  <Divider sx={{ my: 1 }}>
                    <Typography variant="subtitle2" color="text.secondary">
                      Custom Fields
                    </Typography>
                  </Divider>
                </Grid>
                {customFields.map((field) => (
                  <Grid item xs={12} sm={6} key={field.id}>
                    {renderCustomField(field)}
                  </Grid>
                ))}
              </>
            )}

            {/* View mode additional info */}
            {viewMode && selectedRequest && (
              <>
                <Grid item xs={12}>
                  <Divider sx={{ my: 1 }}>
                    <Typography variant="subtitle2" color="text.secondary">
                      Status Information
                    </Typography>
                  </Divider>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="body2" color="text.secondary">
                    Status
                  </Typography>
                  <Chip
                    label={STATUS_LABELS[selectedRequest.status]}
                    color={STATUS_COLORS[selectedRequest.status]}
                    size="small"
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="body2" color="text.secondary">
                    SLA Breached
                  </Typography>
                  <Chip
                    label={selectedRequest.isSlaBreached ? 'Yes' : 'No'}
                    color={selectedRequest.isSlaBreached ? 'error' : 'success'}
                    size="small"
                  />
                </Grid>
                {selectedRequest.slaResponseDueDate && (
                  <Grid item xs={12} sm={6}>
                    <Typography variant="body2" color="text.secondary">
                      Response Due
                    </Typography>
                    <Typography>
                      {new Date(selectedRequest.slaResponseDueDate).toLocaleString()}
                    </Typography>
                  </Grid>
                )}
                {selectedRequest.slaResolutionDueDate && (
                  <Grid item xs={12} sm={6}>
                    <Typography variant="body2" color="text.secondary">
                      Resolution Due
                    </Typography>
                    <Typography>
                      {new Date(selectedRequest.slaResolutionDueDate).toLocaleString()}
                    </Typography>
                  </Grid>
                )}
                {selectedRequest.resolvedDate && (
                  <Grid item xs={12} sm={6}>
                    <Typography variant="body2" color="text.secondary">
                      Resolved Date
                    </Typography>
                    <Typography>
                      {new Date(selectedRequest.resolvedDate).toLocaleString()}
                    </Typography>
                  </Grid>
                )}
                {selectedRequest.closedDate && (
                  <Grid item xs={12} sm={6}>
                    <Typography variant="body2" color="text.secondary">
                      Closed Date
                    </Typography>
                    <Typography>
                      {new Date(selectedRequest.closedDate).toLocaleString()}
                    </Typography>
                  </Grid>
                )}
              </>
            )}
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDialog(false)}>
            {viewMode ? 'Close' : 'Cancel'}
          </Button>
          {!viewMode && (
            <Button variant="contained" onClick={handleSaveRequest}>
              {selectedRequest ? 'Update' : 'Create'}
            </Button>
          )}
        </DialogActions>
      </Dialog>

      {/* Resolve Dialog */}
      <Dialog open={resolveDialogOpen} onClose={() => setResolveDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Resolve Service Request</DialogTitle>
        <DialogContent>
          <TextField
            label="Resolution Notes"
            value={actionNotes}
            onChange={(e) => setActionNotes(e.target.value)}
            multiline
            rows={4}
            fullWidth
            required
            sx={{ mt: 1 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setResolveDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" color="success" onClick={handleResolve}>
            Resolve
          </Button>
        </DialogActions>
      </Dialog>

      {/* Escalate Dialog */}
      <Dialog open={escalateDialogOpen} onClose={() => setEscalateDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Escalate Service Request</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label="Escalation Reason"
              value={actionNotes}
              onChange={(e) => setActionNotes(e.target.value)}
              multiline
              rows={3}
              fullWidth
              required
            />
            <FormControl fullWidth>
              <InputLabel>Escalate to Group</InputLabel>
              <Select
                value={assignToGroupId || ''}
                onChange={(e: SelectChangeEvent<string | number>) =>
                  setAssignToGroupId(e.target.value ? Number(e.target.value) : null)
                }
                label="Escalate to Group"
              >
                <MenuItem value="">None</MenuItem>
                {groups.map((group) => (
                  <MenuItem key={group.id} value={group.id}>
                    {group.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEscalateDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" color="warning" onClick={handleEscalate}>
            Escalate
          </Button>
        </DialogActions>
      </Dialog>

      {/* Assign Dialog */}
      <Dialog open={assignDialogOpen} onClose={() => setAssignDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Assign Service Request</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <FormControl fullWidth>
              <InputLabel>Assign to User</InputLabel>
              <Select
                value={assignToUserId || ''}
                onChange={(e: SelectChangeEvent<string | number>) => {
                  setAssignToUserId(e.target.value ? Number(e.target.value) : null);
                  setAssignToGroupId(null);
                }}
                label="Assign to User"
              >
                <MenuItem value="">None</MenuItem>
                {users.map((user) => (
                  <MenuItem key={user.id} value={user.id}>
                    {user.firstName} {user.lastName}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <Typography variant="body2" color="text.secondary" align="center">
              OR
            </Typography>
            <FormControl fullWidth>
              <InputLabel>Assign to Group</InputLabel>
              <Select
                value={assignToGroupId || ''}
                onChange={(e: SelectChangeEvent<string | number>) => {
                  setAssignToGroupId(e.target.value ? Number(e.target.value) : null);
                  setAssignToUserId(null);
                }}
                label="Assign to Group"
              >
                <MenuItem value="">None</MenuItem>
                {groups.map((group) => (
                  <MenuItem key={group.id} value={group.id}>
                    {group.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <TextField
              label="Notes (Optional)"
              value={actionNotes}
              onChange={(e) => setActionNotes(e.target.value)}
              multiline
              rows={2}
              fullWidth
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAssignDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleAssign}
            disabled={!assignToUserId && !assignToGroupId}
          >
            Assign
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

export default ServiceRequestsPage;

/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 * Licensed under the Source-Available License (see LICENSE) v3.0
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
  Checkbox,
  Tabs,
  Tab,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Switch,
  FormControlLabel,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import CloseIcon from '@mui/icons-material/Close';
import { 
  DialogError, 
  DialogSuccess, 
  ActionButton,
  DialogHeader,
  RelatedEntitiesPanel,
  EnhancedEmptyState,
} from '../components/common';
import { useApiState } from '../hooks/useApiState';
import { useProfile } from '../contexts/ProfileContext';
import LookupSelect from '../components/LookupSelect';
import EntitySelect from '../components/EntitySelect';
import { useState, useEffect, useCallback } from 'react';
import AdvancedSearch, { SearchField, SearchFilter, filterData } from '../components/AdvancedSearch';
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
import NoteIcon from '@mui/icons-material/Note';
import SupportAgentIcon from '@mui/icons-material/SupportAgent';
import LinkIcon from '@mui/icons-material/Link';
import logo from '../assets/logo.png';
import ImportExportButtons from '../components/ImportExportButtons';
import NotesTab from '../components/NotesTab';
import DynamicEntityForm, { ExtraTab } from '../components/DynamicEntityForm';
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
import logger from '../services/logger';
import enumCacheService from '../services/enumCacheService';
import type { EnumValue } from '../types/enums';

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

// Search fields for Advanced Search
const SEARCH_FIELDS: SearchField[] = [
  { name: 'subject', label: 'Title', type: 'text' },
  { name: 'status', label: 'Status', type: 'select', options: [
    { value: 0, label: 'New' },
    { value: 1, label: 'Open' },
    { value: 2, label: 'In Progress' },
    { value: 3, label: 'Pending Customer' },
    { value: 4, label: 'Pending Internal' },
    { value: 5, label: 'Escalated' },
    { value: 6, label: 'Resolved' },
    { value: 7, label: 'Closed' },
    { value: 8, label: 'Cancelled' },
    { value: 9, label: 'On Hold' },
    { value: 10, label: 'Reopened' },
  ]},
  { name: 'priority', label: 'Priority', type: 'select', options: [
    { value: 0, label: 'Low' },
    { value: 1, label: 'Medium' },
    { value: 2, label: 'High' },
    { value: 3, label: 'Critical' },
    { value: 4, label: 'Urgent' },
  ]},
  { name: 'channel', label: 'Channel', type: 'select', options: [
    { value: 0, label: 'WhatsApp' },
    { value: 1, label: 'Email' },
    { value: 2, label: 'Phone' },
    { value: 3, label: 'In Person' },
    { value: 4, label: 'Self Service Portal' },
    { value: 5, label: 'Social Media' },
    { value: 6, label: 'Live Chat' },
    { value: 7, label: 'API' },
  ]},
];

const SEARCHABLE_FIELDS = ['subject', 'description', 'ticketNumber'];

interface ResolutionSlaFields {
  resolutionSummary: string;
  resolutionCode: string;
  rootCause: string;
  slaStatus: string;
  isVipAccount: boolean;
  estimatedEffortHours: number | '';
  actualEffortHours: number | '';
  internalNotes: string;
  // Expedite
  isExpedited: boolean;
  expediteReason: string;
  // Feedback
  satisfactionRating: number | '';
  customerFeedback: string;
  // Reference
  externalReferenceId: string;
}

const DEFAULT_RESOLUTION_SLA: ResolutionSlaFields = {
  resolutionSummary: '',
  resolutionCode: '',
  rootCause: '',
  slaStatus: '',
  isVipAccount: false,
  estimatedEffortHours: '',
  actualEffortHours: '',
  internalNotes: '',
  isExpedited: false,
  expediteReason: '',
  satisfactionRating: '',
  customerFeedback: '',
  externalReferenceId: '',
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
  const [dialogTab, setDialogTab] = useState(0);
  const [selectedRequest, setSelectedRequest] = useState<ServiceRequest | null>(null);
  const [viewMode, setViewMode] = useState(false);
  
  // Reference data
  const [categories, setCategories] = useState<ServiceRequestCategory[]>([]);
  const [subcategories, setSubcategories] = useState<ServiceRequestSubcategory[]>([]);
  const [filteredSubcategories, setFilteredSubcategories] = useState<ServiceRequestSubcategory[]>([]);
  const [customFields, setCustomFields] = useState<ServiceRequestCustomFieldDefinition[]>([]);
  const [accounts, setAccounts] = useState<Customer[]>([]);
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
  const [resolutionSlaData, setResolutionSlaData] = useState<ResolutionSlaFields>(DEFAULT_RESOLUTION_SLA);

  // Action dialogs
  const [resolveDialogOpen, setResolveDialogOpen] = useState(false);
  const [escalateDialogOpen, setEscalateDialogOpen] = useState(false);
  const [assignDialogOpen, setAssignDialogOpen] = useState(false);
  const [actionNotes, setActionNotes] = useState('');
  const [assignToUserId, setAssignToUserId] = useState<number | null>(null);
  const [assignToGroupId, setAssignToGroupId] = useState<number | null>(null);
  const [advancedSearchFilters, setAdvancedSearchFilters] = useState<SearchFilter[]>([]);
  const [advancedSearchText, setAdvancedSearchText] = useState('');
  
  // Multi-select and bulk operations
  const [selectedIds, setSelectedIds] = useState<number[]>([]);
  const [bulkDialogOpen, setBulkDialogOpen] = useState(false);
  const [bulkFormData, setBulkFormData] = useState<{ status: string; priority: string; assignedToUserId: string }>({
    status: '',
    priority: '',
    assignedToUserId: '',
  });
  // ENUM-FE-017: Dynamic priorities loaded from enumCacheService
  const [dynamicPriorities, setDynamicPriorities] = useState<EnumValue[]>([]);
  
  // API state hooks
  const dialogApi = useApiState();
  const bulkApi = useApiState();
  const resolveApi = useApiState();
  const escalateApi = useApiState();
  const assignApi = useApiState();
  const { hasPermission } = useProfile();
  useEffect(() => {
    const fetchReferenceData = async () => {
      try {
        // Core reference data
        const [catRes, subRes, fieldRes, custRes, contactRes, userRes] = await Promise.all([
          serviceRequestCategoryService.getAll(),
          serviceRequestSubcategoryService.getAll(),
          serviceRequestCustomFieldService.getAll(),
          customerService.getAll(),
          apiClient.get('/contacts'),
          apiClient.get('/users'),
        ]);
        setCategories(catRes.data);
        setSubcategories(subRes.data);
        setCustomFields(fieldRes.data);
        setAccounts(custRes.data);
        setContacts(contactRes.data);
        setUsers(userRes.data);
        
        // Optional reference data
        try {
          const groupRes = await apiClient.get('/usergroups');
          setGroups(groupRes.data || []);
        } catch (e) {
          console.warn('Could not load user groups:', e);
        }
        
        try {
          const workflowRes = await apiClient.get('/workflowdefinitions');
          setWorkflows(workflowRes.data || []);
        } catch (e) {
          console.warn('Could not load workflows:', e);
        }
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
        status: filterStatus ? [Number.parseInt(filterStatus)] : undefined,
        priority: filterPriority ? [Number.parseInt(filterPriority)] : undefined,
        channel: filterChannel ? [Number.parseInt(filterChannel)] : undefined,
        categoryId: filterCategory ? Number.parseInt(filterCategory) : undefined,
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

  // ENUM-FE-017: Load dynamic service request priority options
  useEffect(() => {
    enumCacheService.getValues('ServiceRequestPriority').then(setDynamicPriorities).catch(() => {/* fallback to static PRIORITY_LABELS */});
  }, []);

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
    setResolutionSlaData(DEFAULT_RESOLUTION_SLA);
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
      accountId: request.accountId,
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
    setResolutionSlaData({
      resolutionSummary: (request as any).resolutionSummary || '',
      resolutionCode: (request as any).resolutionCode || '',
      rootCause: (request as any).rootCause || '',
      slaStatus: (request as any).slaStatus || '',
      isVipAccount: (request as any).isVipAccount ?? false,
      estimatedEffortHours: (request as any).estimatedEffortHours ?? '',
      actualEffortHours: (request as any).actualEffortHours ?? '',
      internalNotes: (request as any).internalNotes || '',
      isExpedited: (request as any).isExpedited ?? false,
      expediteReason: (request as any).expediteReason || '',
      satisfactionRating: (request as any).satisfactionRating ?? '',
      customerFeedback: (request as any).customerFeedback || '',
      externalReferenceId: (request as any).externalReferenceId || '',
    });
    setSelectedRequest(request);
    setViewMode(false);
    setOpenDialog(true);
  };

  const handleDeleteRequest = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this service request?')) {
      await dialogApi.execute(async () => {
        await serviceRequestService.delete(id);
        setSuccessMessage('Service request deleted successfully');
        await fetchRequests();
      });
    }
  };

  const handleSaveRequest = async () => {
    await dialogApi.execute(async () => {
      if (!formData.subject) {
        throw new Error('Subject is required');
      }

      const customFieldValuesArray = Object.entries(customFieldValues)
        .filter(([, value]) => value)
        .map(([fieldId, value]) => ({
          fieldDefinitionId: Number.parseInt(fieldId),
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
    });
  };

  const handleResolve = async () => {
    if (!selectedRequest || !actionNotes) {
      resolveApi.setError('Resolution notes are required');
      return;
    }
    await resolveApi.execute(async () => {
      await serviceRequestService.resolve(selectedRequest.id!, actionNotes);
      setSuccessMessage('Service request resolved');
      setResolveDialogOpen(false);
      setActionNotes('');
      await fetchRequests();
    });
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
      escalateApi.setError('Escalation reason is required');
      return;
    }
    await escalateApi.execute(async () => {
      await serviceRequestService.escalate(selectedRequest.id!, actionNotes, assignToGroupId || undefined);
      setSuccessMessage('Service request escalated');
      setEscalateDialogOpen(false);
      setActionNotes('');
      setAssignToGroupId(null);
      await fetchRequests();
    });
  };

  const handleAssign = async () => {
    if (!selectedRequest) return;
    await assignApi.execute(async () => {
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
    });
  };
  
  // Multi-select handlers
  const handleSelectAll = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.checked) {
      setSelectedIds(filteredRequests.map(r => r.id!));
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
    setBulkFormData({ status: '', priority: '', assignedToUserId: '' });
    bulkApi.clearError();
    setBulkDialogOpen(true);
  };
  
  const handleBulkUpdate = async () => {
    await bulkApi.execute(async () => {
      const updates = selectedIds.map(id => {
        const updatePayload: any = {};
        if (bulkFormData.status) updatePayload.status = Number.parseInt(bulkFormData.status);
        if (bulkFormData.priority) updatePayload.priority = Number.parseInt(bulkFormData.priority);
        if (bulkFormData.assignedToUserId) updatePayload.assignedToUserId = Number.parseInt(bulkFormData.assignedToUserId);
        return serviceRequestService.update(id, updatePayload);
      });
      await Promise.all(updates);
      setSuccessMessage(`Updated ${selectedIds.length} service requests`);
      setSelectedIds([]);
      setBulkDialogOpen(false);
      await fetchRequests();
    });
  };
  
  const handleBulkDelete = async () => {
    if (!window.confirm(`Are you sure you want to delete ${selectedIds.length} service requests?`)) return;
    await bulkApi.execute(async () => {
      await Promise.all(selectedIds.map(id => serviceRequestService.delete(id)));
      setSuccessMessage(`Deleted ${selectedIds.length} service requests`);
      setSelectedIds([]);
      await fetchRequests();
    });
  };

  const handleFormChange = (field: keyof CreateServiceRequest, value: any) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData((prev: any) => ({ ...prev, [name]: value }));
  };

  const handleSelectChange = (e: any) => {
    const { name, value } = e.target;
    setFormData((prev: any) => ({ ...prev, [name]: value }));
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

  const handleAdvancedSearch = (filters: SearchFilter[], text: string) => {
    setAdvancedSearchFilters(filters);
    setAdvancedSearchText(text);
  };

  const filteredRequests = filterData(requests, advancedSearchFilters, advancedSearchText, SEARCHABLE_FIELDS);

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

      <AdvancedSearch
        fields={SEARCH_FIELDS}
        onSearch={handleAdvancedSearch}
        placeholder="Search service requests by title, description..."
      />

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
            <Box sx={{ display: 'flex', gap: 1 }}>
              <ImportExportButtons entityType="service-requests" entityLabel="Service Requests" onImportComplete={fetchRequests} />
              <Button
                variant="contained"
                color="primary"
                startIcon={<AddIcon />}
                onClick={handleAddRequest}
              >
                New Request
              </Button>
            </Box>
          </Box>
          <Collapse in={showFilters}>
            <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
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
                {hasPermission('canBulkDelete') && (
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

          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
              <CircularProgress />
            </Box>
          ) : (
            <>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell padding="checkbox">
                      <Checkbox
                        indeterminate={selectedIds.length > 0 && selectedIds.length < filteredRequests.length}
                        checked={filteredRequests.length > 0 && selectedIds.length === filteredRequests.length}
                        onChange={handleSelectAll}
                      />
                    </TableCell>
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
                  {filteredRequests.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={11} sx={{ border: 0 }}>
                        <EnhancedEmptyState
                          illustration="serviceRequests"
                          title={advancedSearchFilters.length > 0 ? "No service requests match your filters" : "No service requests yet"}
                          description={advancedSearchFilters.length > 0 
                            ? "Try adjusting your filters to find what you're looking for"
                            : "Create your first service request to start tracking customer issues"
                          }
                          variant={advancedSearchFilters.length > 0 ? "no-results" : "no-data"}
                          primaryActionLabel="New Service Request"
                          onPrimaryAction={() => handleAddRequest()}
                          secondaryActionLabel={advancedSearchFilters.length > 0 ? "Clear Filters" : undefined}
                          onSecondaryAction={advancedSearchFilters.length > 0 ? () => setAdvancedSearchFilters([]) : undefined}
                        />
                      </TableCell>
                    </TableRow>
                  ) : (
                    filteredRequests.map((request) => (
                      <TableRow key={request.id} hover selected={selectedIds.includes(request.id!)}>
                        <TableCell padding="checkbox">
                          <Checkbox
                            checked={selectedIds.includes(request.id!)}
                            onChange={() => handleSelectOne(request.id!)}
                          />
                        </TableCell>
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
                        <TableCell>{request.accountName || '-'}</TableCell>
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
                  setPageSize(Number.parseInt(e.target.value, 10));
                  setPage(0);
                }}
                rowsPerPageOptions={[10, 25, 50, 100]}
              />
            </>
          )}
        </CardContent>
      </Card>

      {/* Create/Edit/View Dialog */}
      <Dialog open={openDialog} onClose={() => { setOpenDialog(false); setDialogTab(0); }} maxWidth="md" fullWidth>
        <DialogHeader
          mode={viewMode ? 'view' : selectedRequest ? 'edit' : 'create'}
          entityType="serviceRequest"
          entityName={selectedRequest?.ticketNumber || undefined}
          entityId={selectedRequest?.id || undefined}
          onClose={() => { setOpenDialog(false); setDialogTab(0); }}
          subtitle={selectedRequest?.subject || undefined}
          status={selectedRequest ? (STATUS_LABELS[selectedRequest.status] || undefined) : undefined}
          statusColor={selectedRequest ? STATUS_COLORS[selectedRequest.status] : undefined}
        />
        <DialogContent dividers>
          <DialogError error={dialogApi.error} onRetry={() => dialogApi.clearError()} />

          <DynamicEntityForm
            moduleName="ServiceRequests"
            formData={formData}
            onChange={handleInputChange}
            onSelectChange={handleSelectChange}
            setFormData={setFormData}
            activeTab={dialogTab}
            editingId={selectedRequest?.id || undefined}
            onTabChange={setDialogTab}
            excludeFields={['customFieldValues']}
            extraTabs={[
              {
                index: 100,
                name: 'Related',
                icon: <LinkIcon fontSize="small" />,
                editOnly: true,
                render: () => (
                  <RelatedEntitiesPanel
                    entityType="serviceRequests"
                    entityId={selectedRequest!.id!}
                    showRelated={['accounts', 'contacts', 'activities']}
                    onEntityClick={(type, id) => {
                      setOpenDialog(false);
                      setDialogTab(0);
                      logger.debug(`Navigate to ${type} ${id}`);
                    }}
                  />
                ),
              },
              {
                index: 101,
                name: 'Notes',
                icon: <NoteIcon fontSize="small" />,
                render: () => selectedRequest?.id ? (
                  <NotesTab
                    entityType="ServiceRequest"
                    entityId={selectedRequest.id}
                    entityName={selectedRequest.ticketNumber || 'Service Request'}
                  />
                ) : (
                  <Alert severity="info" sx={{ mt: 2 }}>
                    Please save the service request first to add notes.
                  </Alert>
                ),
              },
            ]}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDialog(false)} disabled={dialogApi.loading}>
            {viewMode ? 'Close' : 'Cancel'}
          </Button>
          {!viewMode && (
            <ActionButton
              variant="contained"
              onClick={handleSaveRequest}
              loading={dialogApi.loading}
            >
              {selectedRequest ? 'Update' : 'Create'}
            </ActionButton>
          )}
        </DialogActions>
      </Dialog>

      {/* Resolve Dialog */}
      <Dialog open={resolveDialogOpen} onClose={() => !resolveApi.loading && setResolveDialogOpen(false)} maxWidth="sm" fullWidth>
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
          <DialogError error={resolveApi.error} onRetry={() => resolveApi.clearError()} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setResolveDialogOpen(false)} disabled={resolveApi.loading}>Cancel</Button>
          <ActionButton variant="contained" color="success" onClick={handleResolve} loading={resolveApi.loading}>
            Resolve
          </ActionButton>
        </DialogActions>
      </Dialog>

      {/* Escalate Dialog */}
      <Dialog open={escalateDialogOpen} onClose={() => !escalateApi.loading && setEscalateDialogOpen(false)} maxWidth="sm" fullWidth>
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
          <DialogError error={escalateApi.error} onRetry={() => escalateApi.clearError()} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEscalateDialogOpen(false)} disabled={escalateApi.loading}>Cancel</Button>
          <ActionButton variant="contained" color="warning" onClick={handleEscalate} loading={escalateApi.loading}>
            Escalate
          </ActionButton>
        </DialogActions>
      </Dialog>

      {/* Assign Dialog */}
      <Dialog open={assignDialogOpen} onClose={() => !assignApi.loading && setAssignDialogOpen(false)} maxWidth="sm" fullWidth>
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
          <DialogError error={assignApi.error} onRetry={() => assignApi.clearError()} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAssignDialogOpen(false)} disabled={assignApi.loading}>Cancel</Button>
          <ActionButton
            variant="contained"
            onClick={handleAssign}
            disabled={!assignToUserId && !assignToGroupId}
            loading={assignApi.loading}
          >
            Assign
          </ActionButton>
        </DialogActions>
      </Dialog>

      {/* Bulk Update Dialog */}
      <Dialog open={bulkDialogOpen} onClose={() => !bulkApi.loading && setBulkDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Bulk Update {selectedIds.length} Service Requests</DialogTitle>
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
              {Object.entries(STATUS_LABELS).map(([value, label]) => (
                <MenuItem key={value} value={value}>{label}</MenuItem>
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
              {dynamicPriorities.length > 0
                ? dynamicPriorities.map(opt => (
                  <MenuItem key={opt.key} value={opt.key}>{opt.label}</MenuItem>
                ))
                : Object.entries(PRIORITY_LABELS).map(([value, label]) => (
                  <MenuItem key={value} value={value}>{label}</MenuItem>
                ))
              }
            </Select>
          </FormControl>
          
          <FormControl fullWidth margin="normal">
            <InputLabel>Assign to User</InputLabel>
            <Select
              value={bulkFormData.assignedToUserId}
              onChange={(e: SelectChangeEvent) => setBulkFormData(prev => ({ ...prev, assignedToUserId: e.target.value }))}
              label="Assign to User"
            >
              <MenuItem value="">-- No Change --</MenuItem>
              {users.map((user) => (
                <MenuItem key={user.id} value={user.id.toString()}>
                  {user.firstName} {user.lastName}
                </MenuItem>
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
    </Container>
  );
}

export default ServiceRequestsPage;

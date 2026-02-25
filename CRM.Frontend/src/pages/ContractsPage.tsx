/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 * Licensed under the Source-Available License (see LICENSE) v3.0
 */

import { useState, useEffect } from 'react';
import {
  Box, Container, Typography, Card, CardContent, Table, TableBody, TableCell,
  TableHead, TableRow, TablePagination, Button, Dialog, DialogTitle, DialogContent, DialogActions,
  MenuItem, Stack, Chip, IconButton, Tooltip, CircularProgress,
  Alert, FormControl, InputLabel, Select,
  SelectChangeEvent,
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon,
  Download as DownloadIcon, Upload as UploadIcon,
  Refresh as RefreshIcon,
  Warning as WarningIcon,
  Print as PrintIcon, Link as LinkIcon, Note as NoteIcon,
} from '@mui/icons-material';
import { DialogError, ActionButton, TabPanel, DialogHeader, RelatedEntitiesPanel, EnhancedEmptyState } from '../components/common';
import { useApiState } from '../hooks/useApiState';
import { usePagination } from '../hooks/usePagination';
import { useProfile } from '../contexts/ProfileContext';
import NotesTab from '../components/NotesTab';
import DynamicEntityForm, { ExtraTab } from '../components/DynamicEntityForm';
import apiClient from '../services/apiClient';
import logger from '../services/logger';
import logo from '../assets/logo.png';

// ==================== TYPES ====================

enum ContractStatus {
  Draft = 0,
  PendingApproval = 1,
  Approved = 2,
  Active = 3,
  Expired = 4,
  Terminated = 5,
  Renewed = 6,
  OnHold = 7,
}

enum ContractType {
  Service = 0,
  License = 1,
  Subscription = 2,
  Support = 3,
  Maintenance = 4,
  NDA = 5,
  Master = 6,
  Amendment = 7,
  Other = 8,
}

interface Contract {
  id: number;
  contractNumber: string;
  name: string;
  description?: string;
  status: ContractStatus;
  contractType: ContractType;
  accountId: number;
  accountName?: string;
  contactId?: number;
  contactName?: string;
  ownerId?: number;
  ownerName?: string;
  startDate: string;
  endDate: string;
  signedDate?: string;
  value: number;
  billingFrequency?: string;
  autoRenew: boolean;
  renewalNoticeDays: number;
  terms?: string;
  specialConditions?: string;
  contractFileUrl?: string;
  contractFileName?: string;
  parentContractId?: number;
  opportunityId?: number;
  quoteId?: number;
  createdAt?: string;
  updatedAt?: string;
}

interface ContractForm {
  name: string;
  description: string;
  status: ContractStatus;
  contractType: ContractType;
  accountId: number | null;
  contactId: number | null;
  startDate: string;
  endDate: string;
  signedDate: string;
  value: number;
  billingFrequency: string;
  autoRenew: boolean;
  renewalNoticeDays: number;
  terms: string;
  specialConditions: string;
  parentContractId: number | null;
  opportunityId: number | null;
  quoteId: number | null;
  contractFileUrl: string;
  signedContractFileUrl: string;
  approvedByUserId: number | null;
  approvedDate: string;
  rejectionReason: string;
  suspensionReason: string;
  suspendedDate: string;
  terminationClause: string;
  // Currency
  currencyCode: string;
  // Renewal Tracking
  renewalNoticeSent: boolean;
  renewalNoticeSentDate: string;
  renewalInitiatedAt: string;
  renewalCompletedAt: string;
}

// ==================== TYPES ====================

type ChipColor = 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning';

// ==================== CONSTANTS ====================

const CONTRACT_STATUS_OPTIONS: Array<{ value: ContractStatus; label: string; color: ChipColor }> = [
  { value: ContractStatus.Draft, label: 'Draft', color: 'default' },
  { value: ContractStatus.PendingApproval, label: 'Pending Approval', color: 'warning' },
  { value: ContractStatus.Approved, label: 'Approved', color: 'info' },
  { value: ContractStatus.Active, label: 'Active', color: 'success' },
  { value: ContractStatus.Expired, label: 'Expired', color: 'error' },
  { value: ContractStatus.Terminated, label: 'Terminated', color: 'error' },
  { value: ContractStatus.Renewed, label: 'Renewed', color: 'success' },
  { value: ContractStatus.OnHold, label: 'On Hold', color: 'warning' },
];

const CONTRACT_TYPE_OPTIONS = [
  { value: ContractType.Service, label: 'Service Agreement' },
  { value: ContractType.License, label: 'License Agreement' },
  { value: ContractType.Subscription, label: 'Subscription' },
  { value: ContractType.Support, label: 'Support Contract' },
  { value: ContractType.Maintenance, label: 'Maintenance' },
  { value: ContractType.NDA, label: 'Non-Disclosure Agreement' },
  { value: ContractType.Master, label: 'Master Agreement' },
  { value: ContractType.Amendment, label: 'Amendment' },
  { value: ContractType.Other, label: 'Other' },
];

const BILLING_FREQUENCY_OPTIONS = [
  { value: 'monthly', label: 'Monthly' },
  { value: 'quarterly', label: 'Quarterly' },
  { value: 'semi-annual', label: 'Semi-Annual' },
  { value: 'annual', label: 'Annual' },
  { value: 'one-time', label: 'One-Time' },
];

// ==================== HELPER FUNCTIONS ====================

const getStatusInfo = (status: ContractStatus): { label: string; color: ChipColor } =>
  CONTRACT_STATUS_OPTIONS.find(s => s.value === status) || { label: 'Unknown', color: 'default' };

const getTypeLabel = (type: ContractType) =>
  CONTRACT_TYPE_OPTIONS.find(t => t.value === type)?.label || 'Unknown';

const formatCurrency = (value: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);

const formatDate = (dateString: string) =>
  dateString ? new Date(dateString).toLocaleDateString() : '-';

const getDaysUntilExpiry = (endDate: string): number => {
  const end = new Date(endDate);
  const now = new Date();
  return Math.ceil((end.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
};

const isExpiringSoon = (endDate: string, days: number = 30): boolean => {
  const daysLeft = getDaysUntilExpiry(endDate);
  return daysLeft > 0 && daysLeft <= days;
};

// ==================== MAIN COMPONENT ====================

function ContractsPage() {
  // State
  const [contracts, setContracts] = useState<Contract[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  const [filterStatus, setFilterStatus] = useState<ContractStatus | 'all'>('all');
  const [uploadDialogOpen, setUploadDialogOpen] = useState(false);
  const [uploadContractId, setUploadContractId] = useState<number | null>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);

  const emptyForm: ContractForm = {
    name: '',
    description: '',
    status: ContractStatus.Draft,
    contractType: ContractType.Service,
    accountId: null,
    contactId: null,
    startDate: '',
    endDate: '',
    signedDate: '',
    value: 0,
    billingFrequency: 'annual',
    autoRenew: false,
    renewalNoticeDays: 30,
    terms: '',
    specialConditions: '',
    parentContractId: null,
    opportunityId: null,
    quoteId: null,
    contractFileUrl: '',
    signedContractFileUrl: '',
    approvedByUserId: null,
    approvedDate: '',
    rejectionReason: '',
    suspensionReason: '',
    suspendedDate: '',
    terminationClause: '',
    // Currency
    currencyCode: 'USD',
    // Renewal Tracking
    renewalNoticeSent: false,
    renewalNoticeSentDate: '',
    renewalInitiatedAt: '',
    renewalCompletedAt: '',
  };
  const [formData, setFormData] = useState<ContractForm>(emptyForm);

  const dialogApi = useApiState();
  const { hasPermission } = useProfile();

  // ==================== DATA FETCHING ====================

  useEffect(() => {
    fetchContracts();
  }, []);

  const fetchContracts = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/contracts');
      const data = response.data;
      setContracts(Array.isArray(data) ? data : (data?.items ?? []));
      setError(null);
    } catch (err: any) {
      // If endpoint doesn't exist, show empty state
      if (err.response?.status === 404) {
        setContracts([]);
        setError(null);
      } else {
        setError(err.response?.data?.message || 'Failed to fetch contracts');
      }
    } finally {
      setLoading(false);
    }
  };

  // ==================== DIALOG HANDLERS ====================

  const handleOpenDialog = (contract?: Contract) => {
    setDialogTab(0);
    if (contract) {
      setEditingId(contract.id);
      setFormData({
        name: contract.name,
        description: contract.description || '',
        status: contract.status,
        contractType: contract.contractType,
        accountId: contract.accountId,
        contactId: contract.contactId || null,
        startDate: contract.startDate?.split('T')[0] || '',
        endDate: contract.endDate?.split('T')[0] || '',
        signedDate: contract.signedDate?.split('T')[0] || '',
        value: contract.value,
        billingFrequency: contract.billingFrequency || 'annual',
        autoRenew: contract.autoRenew,
        renewalNoticeDays: contract.renewalNoticeDays,
        terms: contract.terms || '',
        specialConditions: contract.specialConditions || '',
        parentContractId: contract.parentContractId || null,
        opportunityId: contract.opportunityId || null,
        quoteId: contract.quoteId || null,
        contractFileUrl: (contract as any).contractFileUrl || '',
        signedContractFileUrl: (contract as any).signedContractFileUrl || '',
        approvedByUserId: (contract as any).approvedByUserId || null,
        approvedDate: (contract as any).approvedDate?.split('T')[0] || '',
        rejectionReason: (contract as any).rejectionReason || '',
        suspensionReason: (contract as any).suspensionReason || '',
        suspendedDate: (contract as any).suspendedDate?.split('T')[0] || '',
        terminationClause: (contract as any).terminationClause || '',
        // Currency
        currencyCode: (contract as any).currencyCode || 'USD',
        // Renewal Tracking
        renewalNoticeSent: (contract as any).renewalNoticeSent || false,
        renewalNoticeSentDate: (contract as any).renewalNoticeSentDate?.split('T')[0] || '',
        renewalInitiatedAt: (contract as any).renewalInitiatedAt?.split('T')[0] || '',
        renewalCompletedAt: (contract as any).renewalCompletedAt?.split('T')[0] || '',
      });
    } else {
      setEditingId(null);
      setFormData(emptyForm);
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
      [name]: type === 'checkbox' ? checked : type === 'number' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleSelectChange = (e: SelectChangeEvent<number | string>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name as string]: value }));
  };

  // ==================== SAVE OPERATIONS ====================

  const handleSaveContract = async () => {
    if (!formData.name?.trim() || !formData.accountId) {
      dialogApi.setError('Contract name and account are required');
      return;
    }

    await dialogApi.execute(async () => {
      if (editingId) {
        await apiClient.put(`/contracts/${editingId}`, formData);
        setSuccessMessage('Contract updated successfully');
      } else {
        await apiClient.post('/contracts', formData);
        setSuccessMessage('Contract created successfully');
      }
      handleCloseDialog();
      fetchContracts();
      setTimeout(() => setSuccessMessage(null), 3000);
    });
  };

  const handleDeleteContract = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this contract?')) {
      try {
        await apiClient.delete(`/contracts/${id}`);
        setSuccessMessage('Contract deleted successfully');
        fetchContracts();
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete contract');
      }
    }
  };

  // ==================== FILE OPERATIONS ====================

  const handleOpenUploadDialog = (contractId: number) => {
    setUploadContractId(contractId);
    setSelectedFile(null);
    setUploadDialogOpen(true);
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      setSelectedFile(e.target.files[0]);
    }
  };

  const handleUploadFile = async () => {
    if (!selectedFile || !uploadContractId) return;

    try {
      const formDataUpload = new FormData();
      formDataUpload.append('file', selectedFile);
      await apiClient.post(`/contracts/${uploadContractId}/upload`, formDataUpload, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      setSuccessMessage('Contract file uploaded successfully');
      setUploadDialogOpen(false);
      fetchContracts();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to upload file');
    }
  };

  const handleDownloadFile = async (contract: Contract) => {
    if (!contract.contractFileUrl) return;
    try {
      const response = await apiClient.get(`/contracts/${contract.id}/download`, {
        responseType: 'blob',
      });
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.download = contract.contractFileName || `contract-${contract.contractNumber}.pdf`;
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (err: any) {
      setError('Failed to download file');
    }
  };

  // ==================== PDF PRINT ====================

  const handlePrintContract = (contract: Contract) => {
    const printWindow = window.open('', '_blank');
    if (!printWindow) return;

    const statusInfo = getStatusInfo(contract.status);
    
    printWindow.document.write(`
      <!DOCTYPE html>
      <html>
      <head>
        <title>Contract - ${contract.contractNumber}</title>
        <style>
          body { font-family: Arial, sans-serif; padding: 40px; max-width: 800px; margin: 0 auto; }
          h1 { color: #333; border-bottom: 2px solid #1976d2; padding-bottom: 10px; }
          .header { display: flex; justify-content: space-between; margin-bottom: 30px; }
          .section { margin-bottom: 20px; }
          .section-title { font-weight: bold; color: #1976d2; margin-bottom: 10px; }
          .field { display: flex; margin-bottom: 8px; }
          .label { font-weight: bold; width: 180px; color: #666; }
          .value { flex: 1; }
          .status { 
            display: inline-block; 
            padding: 4px 12px; 
            border-radius: 4px; 
            background: #4caf50; 
            color: white; 
            font-weight: bold;
          }
          .terms { background: #f5f5f5; padding: 15px; border-radius: 4px; white-space: pre-wrap; }
          @media print {
            body { padding: 20px; }
            button { display: none; }
          }
        </style>
      </head>
      <body>
        <div class="header">
          <div>
            <h1>${contract.name}</h1>
            <p>Contract #: <strong>${contract.contractNumber}</strong></p>
          </div>
          <div>
            <span class="status">${statusInfo.label}</span>
          </div>
        </div>
        
        <div class="section">
          <div class="section-title">Contract Details</div>
          <div class="field"><span class="label">Type:</span><span class="value">${getTypeLabel(contract.contractType)}</span></div>
          <div class="field"><span class="label">Account:</span><span class="value">${contract.accountName || '-'}</span></div>
          <div class="field"><span class="label">Contact:</span><span class="value">${contract.contactName || '-'}</span></div>
          <div class="field"><span class="label">Value:</span><span class="value">${formatCurrency(contract.value)}</span></div>
          <div class="field"><span class="label">Billing:</span><span class="value">${contract.billingFrequency || '-'}</span></div>
        </div>
        
        <div class="section">
          <div class="section-title">Dates</div>
          <div class="field"><span class="label">Start Date:</span><span class="value">${formatDate(contract.startDate)}</span></div>
          <div class="field"><span class="label">End Date:</span><span class="value">${formatDate(contract.endDate)}</span></div>
          <div class="field"><span class="label">Signed Date:</span><span class="value">${formatDate(contract.signedDate || '')}</span></div>
          <div class="field"><span class="label">Auto Renew:</span><span class="value">${contract.autoRenew ? 'Yes' : 'No'}</span></div>
        </div>
        
        ${contract.terms ? `
        <div class="section">
          <div class="section-title">Terms & Conditions</div>
          <div class="terms">${contract.terms}</div>
        </div>
        ` : ''}
        
        ${contract.specialConditions ? `
        <div class="section">
          <div class="section-title">Special Conditions</div>
          <div class="terms">${contract.specialConditions}</div>
        </div>
        ` : ''}
        
        <button onclick="window.print()" style="margin-top: 30px; padding: 10px 20px; cursor: pointer;">Print Contract</button>
      </body>
      </html>
    `);
    printWindow.document.close();
  };

  // ==================== FILTERING ====================

  const filteredContracts = filterStatus === 'all' 
    ? contracts 
    : contracts.filter(c => c.status === filterStatus);

  const { paginatedData: paginatedContracts, page, pageSize, handlePageChange, handlePageSizeChange, pageSizeOptions } = usePagination(filteredContracts, { defaultPageSize: 25 });

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
            <Typography variant="h4">Contracts</Typography>
          </Box>
          <Stack direction="row" spacing={2}>
            <FormControl size="small" sx={{ minWidth: 150 }}>
              <InputLabel>Status Filter</InputLabel>
              <Select
                value={filterStatus}
                onChange={(e) => setFilterStatus(e.target.value as ContractStatus | 'all')}
                label="Status Filter"
              >
                <MenuItem value="all">All</MenuItem>
                {CONTRACT_STATUS_OPTIONS.map(opt => (
                  <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                ))}
              </Select>
            </FormControl>
            <Button variant="outlined" startIcon={<RefreshIcon />} onClick={fetchContracts}>
              Refresh
            </Button>
            <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenDialog()}>
              New Contract
            </Button>
          </Stack>
        </Box>

        {/* Alerts */}
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

        {/* Expiring Soon Alert */}
        {contracts.filter(c => c.status === ContractStatus.Active && isExpiringSoon(c.endDate)).length > 0 && (
          <Alert severity="warning" icon={<WarningIcon />} sx={{ mb: 2 }}>
            {contracts.filter(c => c.status === ContractStatus.Active && isExpiringSoon(c.endDate)).length} contract(s) expiring within 30 days
          </Alert>
        )}

        {/* Contracts Table */}
        <Card>
          <CardContent>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Contract #</TableCell>
                  <TableCell>Name</TableCell>
                  <TableCell>Account</TableCell>
                  <TableCell>Type</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="right">Value</TableCell>
                  <TableCell>End Date</TableCell>
                  <TableCell align="center">File</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredContracts.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={9} sx={{ border: 0 }}>
                      <EnhancedEmptyState
                        illustration="contracts"
                        title="No contracts yet"
                        description="Create your first contract to start managing customer agreements"
                        variant="no-data"
                        primaryActionLabel="Create Contract"
                        onPrimaryAction={() => handleOpenDialog()}
                      />
                    </TableCell>
                  </TableRow>
                ) : (
                  paginatedContracts.map((contract) => {
                    const statusInfo = getStatusInfo(contract.status);
                    const expiringSoon = contract.status === ContractStatus.Active && isExpiringSoon(contract.endDate);
                    const daysLeft = getDaysUntilExpiry(contract.endDate);
                    
                    return (
                      <TableRow key={contract.id} hover>
                        <TableCell>
                          <Typography fontFamily="monospace">{contract.contractNumber}</Typography>
                        </TableCell>
                        <TableCell>
                          <Typography fontWeight="medium">{contract.name}</Typography>
                        </TableCell>
                        <TableCell>{contract.accountName || '-'}</TableCell>
                        <TableCell>{getTypeLabel(contract.contractType)}</TableCell>
                        <TableCell>
                          <Chip
                            label={statusInfo.label}
                            size="small"
                            color={statusInfo.color}
                          />
                        </TableCell>
                        <TableCell align="right">{formatCurrency(contract.value)}</TableCell>
                        <TableCell>
                          <Box>
                            {formatDate(contract.endDate)}
                            {expiringSoon && (
                              <Typography variant="caption" color="warning.main" display="block">
                                {daysLeft} days left
                              </Typography>
                            )}
                          </Box>
                        </TableCell>
                        <TableCell align="center">
                          {contract.contractFileUrl ? (
                            <Tooltip title="Download">
                              <IconButton size="small" onClick={() => handleDownloadFile(contract)}>
                                <DownloadIcon />
                              </IconButton>
                            </Tooltip>
                          ) : (
                            <Tooltip title="Upload File">
                              <IconButton size="small" onClick={() => handleOpenUploadDialog(contract.id)}>
                                <UploadIcon />
                              </IconButton>
                            </Tooltip>
                          )}
                        </TableCell>
                        <TableCell align="right">
                          <Tooltip title="Edit">
                            <IconButton size="small" onClick={() => handleOpenDialog(contract)}>
                              <EditIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Print / PDF">
                            <IconButton size="small" onClick={() => handlePrintContract(contract)}>
                              <PrintIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete">
                            <IconButton size="small" color="error" onClick={() => handleDeleteContract(contract.id)}>
                              <DeleteIcon />
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    );
                  })
                )}
              </TableBody>
            </Table>
            <TablePagination
              component="div"
              count={filteredContracts.length}
              page={page}
              onPageChange={handlePageChange}
              rowsPerPage={pageSize}
              onRowsPerPageChange={handlePageSizeChange}
              rowsPerPageOptions={pageSizeOptions}
            />
          </CardContent>
        </Card>
      </Box>

      {/* Contract Editor Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="lg" fullWidth>
        <DialogHeader
          mode={editingId ? 'edit' : 'create'}
          entityType="contract"
          entityName={editingId ? formData.name || undefined : undefined}
          entityId={editingId || undefined}
          onClose={handleCloseDialog}
          status={editingId && formData.status !== undefined ? 
            (CONTRACT_STATUS_OPTIONS.find(s => s.value === formData.status)?.label || undefined) : undefined}
          statusColor={editingId && formData.status !== undefined ? (
            formData.status === ContractStatus.Active ? 'success' :
            formData.status === ContractStatus.Approved ? 'info' :
            formData.status === ContractStatus.PendingApproval ? 'warning' :
            formData.status === ContractStatus.Expired ? 'error' :
            formData.status === ContractStatus.Terminated ? 'error' : 'default'
          ) : undefined}
        />
        <DialogContent dividers>
          <DialogError error={dialogApi.error} />

          <DynamicEntityForm
            moduleName="Contracts"
            formData={formData}
            onChange={handleInputChange}
            onSelectChange={(e: any) => setFormData(prev => ({ ...prev, [e.target.name]: e.target.value }))}
            setFormData={setFormData}
            activeTab={dialogTab}
            editingId={editingId}
            onTabChange={setDialogTab}
            excludeFields={['tags', 'customFields']}
            extraTabs={[
              {
                index: 100,
                name: 'Related',
                icon: <LinkIcon fontSize="small" />,
                editOnly: true,
                render: () => (
                  <RelatedEntitiesPanel
                    entityType="contracts"
                    entityId={editingId!}
                    showRelated={['accounts', 'contacts', 'quotes']}
                    onEntityClick={(type, id) => {
                      handleCloseDialog();
                      logger.debug(`Navigate to ${type} ${id}`);
                    }}
                  />
                ),
              },
              {
                index: 101,
                name: 'Notes',
                icon: <NoteIcon fontSize="small" />,
                render: () => editingId ? (
                  <NotesTab entityType="Contract" entityId={editingId} entityName={formData.name || 'Contract'} />
                ) : (
                  <Alert severity="info" sx={{ mt: 2 }}>Please save the contract first to add notes.</Alert>
                ),
              },
            ]}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <ActionButton
            onClick={handleSaveContract}
            loading={dialogApi.loading}
            variant="contained"
          >
            {editingId ? 'Update Contract' : 'Create Contract'}
          </ActionButton>
        </DialogActions>
      </Dialog>

      {/* Upload Dialog */}
      <Dialog open={uploadDialogOpen} onClose={() => setUploadDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Upload Contract File</DialogTitle>
        <DialogContent>
          <Box py={2}>
            <input
              type="file"
              accept=".pdf,.doc,.docx"
              onChange={handleFileSelect}
              style={{ marginBottom: 16 }}
            />
            {selectedFile && (
              <Typography variant="body2" color="text.secondary">
                Selected: {selectedFile.name} ({(selectedFile.size / 1024).toFixed(1)} KB)
              </Typography>
            )}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setUploadDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleUploadFile} disabled={!selectedFile}>
            Upload
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

export default ContractsPage;

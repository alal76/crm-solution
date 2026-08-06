/**
 * ContractDetailsPage
 * Displays detailed view of a contract with metadata, documents, and lifecycle actions
 * Priority: P1
 * 
 * Implementation based on SPEC-SALES-005
 */

import React, { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  CircularProgress,
  Alert,
  Chip,
  Grid,
  Divider,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  IconButton,
  Tooltip,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
} from '@mui/material';
import {
  ArrowBack as ArrowBackIcon,
  Download as DownloadIcon,
  Edit as EditIcon,
  Cancel as CancelIcon,
  Refresh as RefreshIcon,
  CheckCircle as CheckCircleIcon,
  AttachFile as AttachFileIcon,
  Autorenew as AutorenewIcon,
  Warning as WarningIcon,
  Print as PrintIcon,
  Link as LinkIcon,
} from '@mui/icons-material';
import { useParams, useNavigate } from 'react-router-dom';
import contractService, {
  Contract,
  ContractStatus,
  ContractType,
  ContractDocument,
  getStatusLabel,
  getStatusColor,
  getTypeLabel,
} from '../services/contractService';

/**
 * Contract Details Page Component
 */
export const ContractDetailsPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const [contract, setContract] = useState<Contract | null>(null);
  const [documents, setDocuments] = useState<ContractDocument[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [renewDialogOpen, setRenewDialogOpen] = useState(false);
  const [terminateDialogOpen, setTerminateDialogOpen] = useState(false);
  const [terminateReason, setTerminateReason] = useState('');
  const [terminateDate, setTerminateDate] = useState('');
  const [renewalMonths, setRenewalMonths] = useState<number>(12);
  const [renewalValue, setRenewalValue] = useState<number>(0);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    const loadContract = async () => {
      if (!id) return;
      
      try {
        setLoading(true);
        const response = await contractService.getById(Number.parseInt(id, 10));
        setContract(response.data);
        setRenewalValue(response.data.totalValue || 0);
        
        // Load documents
        try {
          const docsResponse = await contractService.getDocuments(Number.parseInt(id, 10));
          setDocuments(docsResponse.data || []);
        } catch (err) {
          console.error('Error loading documents:', err);
        }
        
        setError(null);
      } catch (err) {
        setError('Failed to load contract');
        console.error('Error loading contract:', err);
      } finally {
        setLoading(false);
      }
    };

    loadContract();
  }, [id]);

  const handleRenewContract = async () => {
    if (!contract) return;

    try {
      setSubmitting(true);
      const newStartDate = contract.endDate;
      const newEndDate = new Date(contract.endDate);
      newEndDate.setMonth(newEndDate.getMonth() + renewalMonths);
      await contractService.renew(contract.id, newStartDate, newEndDate.toISOString(), renewalValue);
      setRenewDialogOpen(false);
      // Reload contract
      const response = await contractService.getById(contract.id);
      setContract(response.data);
      setError(null);
    } catch (err) {
      setError('Failed to renew contract');
      console.error('Error renewing contract:', err);
    } finally {
      setSubmitting(false);
    }
  };

  const handleTerminateContract = async () => {
    if (!contract) return;
    
    try {
      setSubmitting(true);
      await contractService.terminate(contract.id, terminateReason, terminateDate);
      setTerminateDialogOpen(false);
      // Reload contract
      const response = await contractService.getById(contract.id);
      setContract(response.data);
      setError(null);
    } catch (err) {
      setError('Failed to terminate contract');
      console.error('Error terminating contract:', err);
    } finally {
      setSubmitting(false);
    }
  };

  const handleApproveContract = async () => {
    if (!contract) return;
    
    try {
      setSubmitting(true);
      await contractService.approve(contract.id);
      // Reload contract
      const response = await contractService.getById(contract.id);
      setContract(response.data);
      setError(null);
    } catch (err) {
      setError('Failed to approve contract');
      console.error('Error approving contract:', err);
    } finally {
      setSubmitting(false);
    }
  };

  const handleActivateContract = async () => {
    if (!contract) return;
    
    try {
      setSubmitting(true);
      await contractService.activate(contract.id);
      // Reload contract
      const response = await contractService.getById(contract.id);
      setContract(response.data);
      setError(null);
    } catch (err) {
      setError('Failed to activate contract');
      console.error('Error activating contract:', err);
    } finally {
      setSubmitting(false);
    }
  };

  const handleDownloadPDF = async () => {
    if (!contract) return;
    
    try {
      const response = await contractService.generatePdf(contract.id);
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `Contract_${contract.contractNumber}.pdf`);
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (err) {
      setError('Failed to download PDF');
      console.error('Error downloading PDF:', err);
    }
  };

  const formatCurrency = (amount?: number): string => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount || 0);
  };

  const formatDate = (dateString?: string): string => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  const calculateDaysRemaining = (endDate?: string): number => {
    if (!endDate) return 0;
    const end = new Date(endDate);
    const today = new Date();
    const diffTime = end.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
  };

  if (loading) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 400 }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (!contract) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Alert severity="error">Contract not found</Alert>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/contracts')} sx={{ mt: 2 }}>
          Back to Contracts
        </Button>
      </Container>
    );
  }

  const daysRemaining = calculateDaysRemaining(contract.endDate);
  const isExpiringSoon = daysRemaining > 0 && daysRemaining <= 30;
  const isExpired = daysRemaining < 0;

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}

      {/* Header */}
      <Box sx={{ mb: 4 }}>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/contracts')} sx={{ mb: 2 }}>
          Back to Contracts
        </Button>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
          <Box>
            <Typography variant="h4" gutterBottom>
              {contract.name || `Contract ${contract.contractNumber}`}
            </Typography>
            <Typography color="textSecondary" gutterBottom>
              Account: {contract.accountName || contract.accountId}
            </Typography>
          </Box>
          <Stack direction="row" spacing={1} alignItems="center">
            <Chip
              label={getStatusLabel(contract.status)}
              color={getStatusColor(contract.status)}
              variant="outlined"
            />
            <Chip
              label={getTypeLabel(contract.contractType)}
              variant="outlined"
            />
            {isExpiringSoon && (
              <Chip
                icon={<WarningIcon />}
                label={`Expires in ${daysRemaining} days`}
                color="warning"
                variant="outlined"
              />
            )}
            {isExpired && (
              <Chip
                icon={<WarningIcon />}
                label="Expired"
                color="error"
                variant="outlined"
              />
            )}
          </Stack>
        </Box>
      </Box>

      <Grid container spacing={3}>
        {/* Main Content */}
        <Grid item xs={12} md={8}>
          {/* Contract Details */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>Contract Details</Typography>
              <Divider sx={{ mb: 2 }} />
              <Grid container spacing={2}>
                <Grid item xs={6}>
                  <Typography variant="body2" color="textSecondary">Contract Number</Typography>
                  <Typography variant="body1">{contract.contractNumber}</Typography>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="body2" color="textSecondary">Contract Type</Typography>
                  <Typography variant="body1">{getTypeLabel(contract.contractType)}</Typography>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="body2" color="textSecondary">Start Date</Typography>
                  <Typography variant="body1">{formatDate(contract.startDate)}</Typography>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="body2" color="textSecondary">End Date</Typography>
                  <Typography variant="body1">{formatDate(contract.endDate)}</Typography>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="body2" color="textSecondary">Signed Date</Typography>
                  <Typography variant="body1">{formatDate(contract.signedDate)}</Typography>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="body2" color="textSecondary">Signed By</Typography>
                  <Typography variant="body1">{contract.signedBy || 'N/A'}</Typography>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="body2" color="textSecondary">Value</Typography>
                  <Typography variant="h6" color="primary">{formatCurrency(contract.totalValue)}</Typography>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="body2" color="textSecondary">Billing Frequency</Typography>
                  <Typography variant="body1">{contract.billingFrequency || 'N/A'}</Typography>
                </Grid>
                <Grid item xs={12}>
                  <Typography variant="body2" color="textSecondary">Description</Typography>
                  <Typography variant="body1">{contract.description || 'No description'}</Typography>
                </Grid>
              </Grid>
            </CardContent>
          </Card>

          {/* Terms & Conditions */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>Terms & Conditions</Typography>
              <Divider sx={{ mb: 2 }} />
              <Grid container spacing={2}>
                <Grid item xs={6}>
                  <Typography variant="body2" color="textSecondary">Payment Terms</Typography>
                  <Typography variant="body1">{contract.paymentTerms || 'Standard terms'}</Typography>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="body2" color="textSecondary">Auto-Renew</Typography>
                  <Typography variant="body1">{contract.autoRenew ? 'Yes' : 'No'}</Typography>
                </Grid>
                {contract.autoRenew && (
                  <>
                    <Grid item xs={6}>
                      <Typography variant="body2" color="textSecondary">Renewal Term (Months)</Typography>
                      <Typography variant="body1">{contract.renewalTermMonths || 'N/A'}</Typography>
                    </Grid>
                    <Grid item xs={6}>
                      <Typography variant="body2" color="textSecondary">Renewal Notice (Days)</Typography>
                      <Typography variant="body1">{contract.renewalNoticeDays || 'N/A'}</Typography>
                    </Grid>
                  </>
                )}
                <Grid item xs={12}>
                  <Typography variant="body2" color="textSecondary">Contract Terms</Typography>
                  <Typography variant="body1" sx={{ whiteSpace: 'pre-wrap' }}>
                    {contract.termsAndConditions || 'Standard terms and conditions apply'}
                  </Typography>
                </Grid>
                {contract.specialConditions && (
                  <Grid item xs={12}>
                    <Typography variant="body2" color="textSecondary">Special Conditions</Typography>
                    <Typography variant="body1">{contract.specialConditions}</Typography>
                  </Grid>
                )}
              </Grid>
            </CardContent>
          </Card>

          {/* Documents */}
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>Contract Documents</Typography>
              <Divider sx={{ mb: 2 }} />
              {documents.length > 0 ? (
                <List>
                  {documents.map((doc) => (
                    <ListItem key={doc.id}>
                      <ListItemIcon>
                        <AttachFileIcon />
                      </ListItemIcon>
                      <ListItemText
                        primary={doc.fileName}
                        secondary={`Uploaded: ${formatDate(doc.uploadedAt)} • ${(doc.fileSize / 1024).toFixed(2)} KB`}
                      />
                      <IconButton edge="end" onClick={() => window.open(doc.filePath, '_blank')}>
                        <DownloadIcon />
                      </IconButton>
                    </ListItem>
                  ))}
                </List>
              ) : (
                <Typography color="textSecondary">No documents attached</Typography>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Sidebar */}
        <Grid item xs={12} md={4}>
          {/* Status & Expiry */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>Status</Typography>
              <Divider sx={{ mb: 2 }} />
              <Stack spacing={2}>
                <Box>
                  <Typography variant="body2" color="textSecondary">Current Status</Typography>
                  <Chip
                    label={getStatusLabel(contract.status)}
                    color={getStatusColor(contract.status)}
                    sx={{ mt: 1 }}
                  />
                </Box>
                {daysRemaining > 0 && (
                  <Box>
                    <Typography variant="body2" color="textSecondary">Days Remaining</Typography>
                    <Typography variant="h5" color={isExpiringSoon ? 'warning.main' : 'success.main'}>
                      {daysRemaining}
                    </Typography>
                  </Box>
                )}
                {isExpired && (
                  <Alert severity="error">
                    This contract has expired
                  </Alert>
                )}
                {isExpiringSoon && (
                  <Alert severity="warning">
                    This contract expires soon
                  </Alert>
                )}
              </Stack>
            </CardContent>
          </Card>

          {/* Actions */}
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>Actions</Typography>
              <Divider sx={{ mb: 2 }} />
              <Stack spacing={2}>
                <Button
                  variant="outlined"
                  startIcon={<DownloadIcon />}
                  fullWidth
                  onClick={handleDownloadPDF}
                >
                  Download PDF
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<PrintIcon />}
                  fullWidth
                  onClick={() => window.print()}
                >
                  Print
                </Button>
                {contract.status === ContractStatus.PendingApproval && (
                  <Button
                    variant="contained"
                    color="success"
                    startIcon={<CheckCircleIcon />}
                    fullWidth
                    onClick={handleApproveContract}
                    disabled={submitting}
                  >
                    Approve Contract
                  </Button>
                )}
                {contract.status === ContractStatus.Approved && (
                  <Button
                    variant="contained"
                    color="primary"
                    startIcon={<CheckCircleIcon />}
                    fullWidth
                    onClick={handleActivateContract}
                    disabled={submitting}
                  >
                    Activate Contract
                  </Button>
                )}
                {(contract.status === ContractStatus.Active || 
                  contract.status === ContractStatus.Expired) && (
                  <Button
                    variant="contained"
                    color="primary"
                    startIcon={<AutorenewIcon />}
                    fullWidth
                    onClick={() => setRenewDialogOpen(true)}
                  >
                    Renew Contract
                  </Button>
                )}
                {contract.status === ContractStatus.Active && (
                  <Button
                    variant="outlined"
                    color="error"
                    startIcon={<CancelIcon />}
                    fullWidth
                    onClick={() => setTerminateDialogOpen(true)}
                  >
                    Terminate Contract
                  </Button>
                )}
                <Button
                  variant="text"
                  startIcon={<RefreshIcon />}
                  fullWidth
                  onClick={() => window.location.reload()}
                >
                  Refresh
                </Button>
              </Stack>
            </CardContent>
          </Card>

          {/* Related Information */}
          <Card sx={{ mt: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>Related Information</Typography>
              <Divider sx={{ mb: 2 }} />
              <Stack spacing={2}>
                <Box>
                  <Typography variant="body2" color="textSecondary">Account</Typography>
                  <Typography variant="body1">{contract.accountName || 'N/A'}</Typography>
                </Box>
                {contract.contactId && (
                  <Box>
                    <Typography variant="body2" color="textSecondary">Contact</Typography>
                    <Typography variant="body1">{contract.contactName || contract.contactId}</Typography>
                  </Box>
                )}
                {contract.parentContractId && (
                  <Box>
                    <Typography variant="body2" color="textSecondary">Parent Contract</Typography>
                    <Button
                      size="small"
                      startIcon={<LinkIcon />}
                      onClick={() => navigate(`/contracts/${contract.parentContractId}`)}
                    >
                      View Parent
                    </Button>
                  </Box>
                )}
                <Box>
                  <Typography variant="body2" color="textSecondary">Created</Typography>
                  <Typography variant="body1">{formatDate(contract.createdAt)}</Typography>
                </Box>
                {contract.updatedAt && (
                  <Box>
                    <Typography variant="body2" color="textSecondary">Last Updated</Typography>
                    <Typography variant="body1">{formatDate(contract.updatedAt)}</Typography>
                  </Box>
                )}
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Renew Dialog */}
      <Dialog open={renewDialogOpen} onClose={() => setRenewDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Renew Contract</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <TextField
              fullWidth
              label="Renewal Term (Months)"
              type="number"
              value={renewalMonths}
              onChange={(e) => setRenewalMonths(Number.parseInt(e.target.value))}
              sx={{ mb: 2 }}
              inputProps={{ min: 1, max: 120 }}
            />
            <TextField
              fullWidth
              label="New Contract Value"
              type="number"
              value={renewalValue}
              onChange={(e) => setRenewalValue(Number.parseFloat(e.target.value))}
              inputProps={{ min: 0, step: 0.01 }}
              helperText="Leave at current value or enter new amount"
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRenewDialogOpen(false)} disabled={submitting}>
            Cancel
          </Button>
          <Button 
            onClick={handleRenewContract} 
            variant="contained" 
            disabled={submitting || renewalMonths < 1}
          >
            {submitting ? 'Renewing...' : 'Renew Contract'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Terminate Dialog */}
      <Dialog open={terminateDialogOpen} onClose={() => setTerminateDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Terminate Contract</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Alert severity="warning" sx={{ mb: 2 }}>
              Terminating a contract will end the agreement. This action may have legal and financial implications.
            </Alert>
            <TextField
              fullWidth
              multiline
              rows={4}
              label="Termination Reason"
              value={terminateReason}
              onChange={(e) => setTerminateReason(e.target.value)}
              placeholder="Explain the reason for terminating this contract..."
              variant="outlined"
              sx={{ mb: 2 }}
            />
            <TextField
              fullWidth
              label="Termination Date"
              type="date"
              value={terminateDate}
              onChange={(e) => setTerminateDate(e.target.value)}
              InputLabelProps={{ shrink: true }}
              helperText="Leave blank to terminate immediately"
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setTerminateDialogOpen(false)} disabled={submitting}>
            Cancel
          </Button>
          <Button 
            onClick={handleTerminateContract} 
            variant="contained" 
            color="error"
            disabled={submitting || !terminateReason.trim()}
          >
            {submitting ? 'Terminating...' : 'Terminate Contract'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default ContractDetailsPage;

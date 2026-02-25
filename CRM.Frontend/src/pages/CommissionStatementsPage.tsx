/**
 * Commission Statements Page
 * TODO-SALES007-004-EXT: Commission statements for sales reps
 * 
 * This page provides detailed commission statements for each sales rep,
 * including period summaries, deal breakdowns, and payment history.
 */
import { useState, useEffect, useMemo } from 'react';
import { useAuth } from '../contexts/AuthContext';
import {
  Box, Card, CardContent, Typography, Button, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, TablePagination, Dialog, DialogTitle, DialogContent, DialogActions, Alert, CircularProgress,
  TextField, Container, FormControl, InputLabel, Select, MenuItem, Chip, Grid,
  IconButton, Tooltip, Paper, Divider, LinearProgress, Accordion, AccordionSummary, AccordionDetails
} from '@mui/material';
import {
  Download as DownloadIcon, Print as PrintIcon, Email as EmailIcon,
  ExpandMore as ExpandMoreIcon, Visibility as ViewIcon, Calculate as CalculateIcon,
  DateRange as DateRangeIcon, Person as PersonIcon, AttachMoney as AmountIcon,
  CheckCircle as ApprovedIcon, Schedule as PendingIcon, AccountBalance as PaidIcon
} from '@mui/icons-material';
import commissionService, {
  CommissionStatement, CommissionStatementStatus, CommissionStatementGenerateRequest,
  Commission, CommissionStatus
} from '../services/commissionService';
import logger from '../services/logger';
import { EnhancedEmptyState } from '../components/common';
import { usePagination } from '../hooks/usePagination';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs, { Dayjs } from 'dayjs';

// ============================================================================
// Helper Functions
// ============================================================================

const getStatementStatusLabel = (status: CommissionStatementStatus): string => {
  const labels: Record<CommissionStatementStatus, string> = {
    [CommissionStatementStatus.Draft]: 'Draft',
    [CommissionStatementStatus.PendingApproval]: 'Pending Approval',
    [CommissionStatementStatus.Approved]: 'Approved',
    [CommissionStatementStatus.Paid]: 'Paid',
    [CommissionStatementStatus.Disputed]: 'Disputed',
    [CommissionStatementStatus.Voided]: 'Voided',
  };
  return labels[status] || 'Unknown';
};

const getStatementStatusColor = (status: CommissionStatementStatus): 'default' | 'warning' | 'success' | 'error' | 'info' | 'primary' | 'secondary' => {
  const colors: Record<CommissionStatementStatus, 'default' | 'warning' | 'success' | 'error' | 'info' | 'primary' | 'secondary'> = {
    [CommissionStatementStatus.Draft]: 'default',
    [CommissionStatementStatus.PendingApproval]: 'warning',
    [CommissionStatementStatus.Approved]: 'info',
    [CommissionStatementStatus.Paid]: 'success',
    [CommissionStatementStatus.Disputed]: 'error',
    [CommissionStatementStatus.Voided]: 'secondary',
  };
  return colors[status] || 'default';
};

const formatCurrency = (amount: number, currencyCode?: string | null): string => {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: currencyCode || 'USD' }).format(amount);
};

const formatDate = (date: string | Date | null | undefined): string => {
  if (!date) return '-';
  return new Date(date).toLocaleDateString();
};

// ============================================================================
// Main Component
// ============================================================================

export default function CommissionStatementsPage() {
  const { user } = useAuth();
  
  // Statements state
  const [statements, setStatements] = useState<CommissionStatement[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  
  // View statement dialog
  const [selectedStatement, setSelectedStatement] = useState<CommissionStatement | null>(null);
  const [statementCommissions, setStatementCommissions] = useState<Commission[]>([]);
  const [viewDialogOpen, setViewDialogOpen] = useState(false);
  
  // Generate statement dialog
  const [generateDialogOpen, setGenerateDialogOpen] = useState(false);
  const [generating, setGenerating] = useState(false);
  const [generateForm, setGenerateForm] = useState({
    userId: '',
    periodStart: null as Dayjs | null,
    periodEnd: null as Dayjs | null,
  });
  
  // Filter state
  const [statusFilter, setStatusFilter] = useState<CommissionStatementStatus | ''>('');
  const [periodFilter, setPeriodFilter] = useState<'current' | 'previous' | 'ytd' | 'all'>('current');

  // ============================================================================
  // Data Loading
  // ============================================================================

  const loadStatements = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await commissionService.getStatements();
      setStatements(data);
    } catch (err) {
      logger.error('Failed to load commission statements', err);
      setError('Failed to load commission statements');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadStatements();
  }, []);

  // ============================================================================
  // Filtered Data
  // ============================================================================

  const filteredStatements = useMemo(() => {
    let filtered = [...statements];
    
    if (statusFilter !== '') {
      filtered = filtered.filter(s => s.status === statusFilter);
    }
    
    const now = new Date();
    const currentMonth = now.getMonth();
    const currentYear = now.getFullYear();
    
    switch (periodFilter) {
      case 'current':
        filtered = filtered.filter(s => {
          const startDate = new Date(s.periodStart || s.periodStartDate);
          return startDate.getMonth() === currentMonth && startDate.getFullYear() === currentYear;
        });
        break;
      case 'previous':
        filtered = filtered.filter(s => {
          const startDate = new Date(s.periodStart || s.periodStartDate);
          const prevMonth = currentMonth === 0 ? 11 : currentMonth - 1;
          const prevYear = currentMonth === 0 ? currentYear - 1 : currentYear;
          return startDate.getMonth() === prevMonth && startDate.getFullYear() === prevYear;
        });
        break;
      case 'ytd':
        filtered = filtered.filter(s => {
          const startDate = new Date(s.periodStart || s.periodStartDate);
          return startDate.getFullYear() === currentYear;
        });
        break;
    }
    
    return filtered.sort((a, b) => new Date(b.periodStart || b.periodStartDate).getTime() - new Date(a.periodStart || a.periodStartDate).getTime());
  }, [statements, statusFilter, periodFilter]);

  // ============================================================================
  // Calculations
  // ============================================================================

  const summaryStats = useMemo(() => {
    const totalEarnings = filteredStatements.reduce((sum, s) => sum + (s.totalAmount ?? s.totalEarned ?? 0), 0);
    const pendingAmount = filteredStatements
      .filter(s => s.status === CommissionStatementStatus.PendingApproval)
      .reduce((sum, s) => sum + (s.totalAmount ?? s.totalEarned ?? 0), 0);
    const paidAmount = filteredStatements
      .filter(s => s.status === CommissionStatementStatus.Paid)
      .reduce((sum, s) => sum + (s.totalAmount ?? s.totalEarned ?? 0), 0);
    const approvedAmount = filteredStatements
      .filter(s => s.status === CommissionStatementStatus.Approved)
      .reduce((sum, s) => sum + (s.totalAmount ?? s.totalEarned ?? 0), 0);
      
    return { totalEarnings, pendingAmount, paidAmount, approvedAmount };
  }, [filteredStatements]);
  const { paginatedData: paginatedStatements, page, pageSize, handlePageChange, handlePageSizeChange, pageSizeOptions } = usePagination(filteredStatements, { defaultPageSize: 25 });

  // ============================================================================
  // Actions
  // ============================================================================

  const handleViewStatement = async (statement: CommissionStatement) => {
    setSelectedStatement(statement);
    setViewDialogOpen(true);
    
    try {
      const commissions = await commissionService.getCommissionsForStatement(statement.id);
      setStatementCommissions(commissions);
    } catch (err) {
      logger.error('Failed to load statement commissions', err);
      setStatementCommissions([]);
    }
  };

  const handleGenerateStatement = async () => {
    if (!generateForm.userId || !generateForm.periodStart || !generateForm.periodEnd) {
      setError('Please fill in all fields');
      return;
    }
    
    try {
      setGenerating(true);
      const request: CommissionStatementGenerateRequest = {
        userId: parseInt(generateForm.userId),
        periodStart: generateForm.periodStart.toISOString(),
        periodEnd: generateForm.periodEnd.toISOString(),
      };
      
      await commissionService.generateStatement(request);
      setSuccessMessage('Statement generated successfully');
      setGenerateDialogOpen(false);
      loadStatements();
    } catch (err) {
      logger.error('Failed to generate statement', err);
      setError('Failed to generate statement');
    } finally {
      setGenerating(false);
    }
  };

  const handleApproveStatement = async (statementId: number) => {
    try {
      await commissionService.updateStatementStatus(statementId, CommissionStatementStatus.Approved);
      setSuccessMessage('Statement approved');
      loadStatements();
    } catch (err) {
      logger.error('Failed to approve statement', err);
      setError('Failed to approve statement');
    }
  };

  const handleMarkPaid = async (statementId: number) => {
    try {
      await commissionService.updateStatementStatus(statementId, CommissionStatementStatus.Paid);
      setSuccessMessage('Statement marked as paid');
      loadStatements();
    } catch (err) {
      logger.error('Failed to mark statement as paid', err);
      setError('Failed to mark statement as paid');
    }
  };

  const handleDownloadPdf = async (statementId: number) => {
    try {
      const blob = await commissionService.downloadStatementPdf(statementId);
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `commission-statement-${statementId}.pdf`;
      link.click();
      window.URL.revokeObjectURL(url);
    } catch (err) {
      logger.error('Failed to download statement PDF', err);
      setError('Failed to download statement PDF');
    }
  };

  // ============================================================================
  // Render
  // ============================================================================

  return (
    <Container maxWidth="xl" sx={{ mt: 4, mb: 4 }}>
      {/* Header */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" component="h1">
          Commission Statements
        </Typography>
        <Button
          variant="contained"
          startIcon={<CalculateIcon />}
          onClick={() => setGenerateDialogOpen(true)}
        >
          Generate Statement
        </Button>
      </Box>

      {/* Alerts */}
      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
      {successMessage && <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccessMessage(null)}>{successMessage}</Alert>}

      {/* Summary Cards */}
      <Grid container spacing={3} mb={3}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="textSecondary" variant="caption">Total Earnings</Typography>
                  <Typography variant="h5">{formatCurrency(summaryStats.totalEarnings)}</Typography>
                </Box>
                <AmountIcon color="primary" fontSize="large" />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="textSecondary" variant="caption">Pending Approval</Typography>
                  <Typography variant="h5">{formatCurrency(summaryStats.pendingAmount)}</Typography>
                </Box>
                <PendingIcon color="warning" fontSize="large" />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="textSecondary" variant="caption">Approved</Typography>
                  <Typography variant="h5">{formatCurrency(summaryStats.approvedAmount)}</Typography>
                </Box>
                <ApprovedIcon color="info" fontSize="large" />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="textSecondary" variant="caption">Paid</Typography>
                  <Typography variant="h5">{formatCurrency(summaryStats.paidAmount)}</Typography>
                </Box>
                <PaidIcon color="success" fontSize="large" />
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Filters */}
      <Paper sx={{ p: 2, mb: 3 }}>
        <Grid container spacing={2} alignItems="center">
          <Grid item xs={12} sm={4} md={3}>
            <FormControl fullWidth size="small">
              <InputLabel>Period</InputLabel>
              <Select
                value={periodFilter}
                label="Period"
                onChange={(e) => setPeriodFilter(e.target.value as typeof periodFilter)}
              >
                <MenuItem value="current">Current Month</MenuItem>
                <MenuItem value="previous">Previous Month</MenuItem>
                <MenuItem value="ytd">Year to Date</MenuItem>
                <MenuItem value="all">All Time</MenuItem>
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} sm={4} md={3}>
            <FormControl fullWidth size="small">
              <InputLabel>Status</InputLabel>
              <Select
                value={statusFilter}
                label="Status"
                onChange={(e) => setStatusFilter(e.target.value as CommissionStatementStatus | '')}
              >
                <MenuItem value="">All Statuses</MenuItem>
                <MenuItem value={CommissionStatementStatus.Draft}>Draft</MenuItem>
                <MenuItem value={CommissionStatementStatus.PendingApproval}>Pending Approval</MenuItem>
                <MenuItem value={CommissionStatementStatus.Approved}>Approved</MenuItem>
                <MenuItem value={CommissionStatementStatus.Paid}>Paid</MenuItem>
                <MenuItem value={CommissionStatementStatus.Disputed}>Disputed</MenuItem>
              </Select>
            </FormControl>
          </Grid>
        </Grid>
      </Paper>

      {/* Statements Table */}
      {loading ? (
        <Box display="flex" justifyContent="center" p={4}><CircularProgress /></Box>
      ) : filteredStatements.length === 0 ? (
        <EnhancedEmptyState
          illustration="generic"
          title="No Commission Statements"
          description="No commission statements found for the selected filters."
          primaryActionLabel="Generate Statement"
          onPrimaryAction={() => setGenerateDialogOpen(true)}
        />
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Period</TableCell>
                <TableCell>User</TableCell>
                <TableCell align="right">Total Amount</TableCell>
                <TableCell align="right">Adjustments</TableCell>
                <TableCell align="right">Net Amount</TableCell>
                <TableCell align="center">Status</TableCell>
                <TableCell align="center">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {paginatedStatements.map((statement) => (
                <TableRow key={statement.id} hover>
                  <TableCell>
                    {formatDate(statement.periodStart || statement.periodStartDate)} - {formatDate(statement.periodEnd || statement.periodEndDate)}
                  </TableCell>
                  <TableCell>{statement.userName || statement.user?.firstName ? `${statement.user?.firstName || ''} ${statement.user?.lastName || ''}`.trim() : `User #${statement.userId}`}</TableCell>
                  <TableCell align="right">{formatCurrency(statement.totalAmount ?? statement.totalEarned ?? 0)}</TableCell>
                  <TableCell align="right">{formatCurrency(statement.adjustments ?? statement.totalAdjustments ?? 0)}</TableCell>
                  <TableCell align="right">{formatCurrency(statement.netAmount ?? statement.netPayout ?? statement.totalAmount ?? statement.totalEarned ?? 0)}</TableCell>
                  <TableCell align="center">
                    <Chip
                      label={getStatementStatusLabel(statement.status)}
                      color={getStatementStatusColor(statement.status)}
                      size="small"
                    />
                  </TableCell>
                  <TableCell align="center">
                    <Tooltip title="View Details">
                      <IconButton size="small" onClick={() => handleViewStatement(statement)}>
                        <ViewIcon />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Download PDF">
                      <IconButton size="small" onClick={() => handleDownloadPdf(statement.id)}>
                        <DownloadIcon />
                      </IconButton>
                    </Tooltip>
                    {statement.status === CommissionStatementStatus.PendingApproval && (
                      <Tooltip title="Approve">
                        <IconButton size="small" color="success" onClick={() => handleApproveStatement(statement.id)}>
                          <ApprovedIcon />
                        </IconButton>
                      </Tooltip>
                    )}
                    {statement.status === CommissionStatementStatus.Approved && (
                      <Tooltip title="Mark as Paid">
                        <IconButton size="small" color="primary" onClick={() => handleMarkPaid(statement.id)}>
                          <PaidIcon />
                        </IconButton>
                      </Tooltip>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
          <TablePagination
            component="div"
            count={filteredStatements.length}
            page={page}
            onPageChange={handlePageChange}
            rowsPerPage={pageSize}
            onRowsPerPageChange={handlePageSizeChange}
            rowsPerPageOptions={pageSizeOptions}
          />
        </TableContainer>
      )}

      {/* View Statement Dialog */}
      <Dialog open={viewDialogOpen} onClose={() => setViewDialogOpen(false)} maxWidth="lg" fullWidth>
        <DialogTitle>
          Commission Statement
          {selectedStatement && (
            <Typography variant="subtitle1" color="textSecondary">
              {formatDate(selectedStatement.periodStart || selectedStatement.periodStartDate)} - {formatDate(selectedStatement.periodEnd || selectedStatement.periodEndDate)}
            </Typography>
          )}
        </DialogTitle>
        <DialogContent dividers>
          {selectedStatement && (
            <>
              <Grid container spacing={2} mb={3}>
                <Grid item xs={4}>
                  <Typography variant="caption" color="textSecondary">Total Amount</Typography>
                  <Typography variant="h6">{formatCurrency(selectedStatement.totalAmount ?? selectedStatement.totalEarned ?? 0)}</Typography>
                </Grid>
                <Grid item xs={4}>
                  <Typography variant="caption" color="textSecondary">Adjustments</Typography>
                  <Typography variant="h6">{formatCurrency(selectedStatement.adjustments ?? selectedStatement.totalAdjustments ?? 0)}</Typography>
                </Grid>
                <Grid item xs={4}>
                  <Typography variant="caption" color="textSecondary">Net Amount</Typography>
                  <Typography variant="h6">{formatCurrency(selectedStatement.netAmount ?? selectedStatement.netPayout ?? selectedStatement.totalAmount ?? selectedStatement.totalEarned ?? 0)}</Typography>
                </Grid>
              </Grid>
              
              <Divider sx={{ my: 2 }} />
              
              <Typography variant="h6" gutterBottom>Commission Details</Typography>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Deal</TableCell>
                      <TableCell align="right">Deal Amount</TableCell>
                      <TableCell align="right">Rate</TableCell>
                      <TableCell align="right">Commission</TableCell>
                      <TableCell align="center">Status</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {statementCommissions.map((comm) => (
                      <TableRow key={comm.id}>
                        <TableCell>{comm.opportunity?.name || `Opportunity #${comm.opportunityId}`}</TableCell>
                        <TableCell align="right">{formatCurrency(comm.dealAmount)}</TableCell>
                        <TableCell align="right">{comm.commissionRate}%</TableCell>
                        <TableCell align="right">{formatCurrency(comm.finalCommissionAmount)}</TableCell>
                        <TableCell align="center">
                          <Chip
                            label={comm.status === CommissionStatus.Paid ? 'Paid' : 'Pending'}
                            color={comm.status === CommissionStatus.Paid ? 'success' : 'warning'}
                            size="small"
                          />
                        </TableCell>
                      </TableRow>
                    ))}
                    {statementCommissions.length === 0 && (
                      <TableRow>
                        <TableCell colSpan={5} align="center">No commission details available</TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </TableContainer>
            </>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setViewDialogOpen(false)}>Close</Button>
          {selectedStatement && (
            <Button
              variant="contained"
              startIcon={<DownloadIcon />}
              onClick={() => handleDownloadPdf(selectedStatement.id)}
            >
              Download PDF
            </Button>
          )}
        </DialogActions>
      </Dialog>

      {/* Generate Statement Dialog */}
      <Dialog open={generateDialogOpen} onClose={() => setGenerateDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Generate Commission Statement</DialogTitle>
        <DialogContent>
          <LocalizationProvider dateAdapter={AdapterDayjs}>
            <Grid container spacing={2} sx={{ mt: 1 }}>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="User ID"
                  type="number"
                  value={generateForm.userId}
                  onChange={(e) => setGenerateForm({ ...generateForm, userId: e.target.value })}
                />
              </Grid>
              <Grid item xs={6}>
                <DatePicker
                  label="Period Start"
                  value={generateForm.periodStart}
                  onChange={(date) => setGenerateForm({ ...generateForm, periodStart: date })}
                  slotProps={{ textField: { fullWidth: true } }}
                />
              </Grid>
              <Grid item xs={6}>
                <DatePicker
                  label="Period End"
                  value={generateForm.periodEnd}
                  onChange={(date) => setGenerateForm({ ...generateForm, periodEnd: date })}
                  slotProps={{ textField: { fullWidth: true } }}
                />
              </Grid>
            </Grid>
          </LocalizationProvider>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setGenerateDialogOpen(false)} disabled={generating}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleGenerateStatement}
            disabled={generating}
            startIcon={generating ? <CircularProgress size={20} /> : <CalculateIcon />}
          >
            Generate
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

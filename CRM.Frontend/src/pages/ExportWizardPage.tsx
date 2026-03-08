/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This software is source-available. Non-commercial use is permitted under
 * the terms of the LICENSE file. Commercial use requires a separate license.
 * See the LICENSE file in the root directory for full terms.
 */

import { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Typography,
  Button,
  Paper,
  Stepper,
  Step,
  StepLabel,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  FormControlLabel,
  Radio,
  RadioGroup,
  Checkbox,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  LinearProgress,
  Alert,
  Chip,
  IconButton,
  Tooltip,
  Card,
  CardContent,
  Breadcrumbs,
  Link,
  Divider,
  TextField,
  SelectChangeEvent,
} from '@mui/material';
import {
  Download as DownloadIcon,
  NavigateNext as NextIcon,
  NavigateBefore as BackIcon,
  PlayArrow as StartIcon,
  CheckCircle as SuccessIcon,
  SelectAll as SelectAllIcon,
  DeselectOutlined as DeselectIcon,
  Refresh as RefreshIcon,
  Home as HomeIcon,
} from '@mui/icons-material';
import { Link as RouterLink } from 'react-router-dom';
import importExportService, { ExportJobDto } from '../services/importExportService';

// ============================================================================
// Constants
// ============================================================================

const ENTITY_TYPES = [
  { value: 'Accounts', label: 'Accounts' },
  { value: 'Contacts', label: 'Contacts' },
  { value: 'Leads', label: 'Leads' },
  { value: 'Opportunities', label: 'Opportunities' },
  { value: 'Products', label: 'Products' },
] as const;

const EXPORT_FORMATS = [
  { value: 'csv', label: 'CSV', description: 'Comma-separated values, compatible with Excel and most tools' },
  { value: 'xlsx', label: 'Excel (.xlsx)', description: 'Native Excel format with formatting support' },
  { value: 'json', label: 'JSON', description: 'Structured data format for developer integrations' },
] as const;

const ENTITY_COLUMNS: Record<string, { field: string; label: string }[]> = {
  Accounts: [
    { field: 'Id', label: 'ID' },
    { field: 'Name', label: 'Name' },
    { field: 'Industry', label: 'Industry' },
    { field: 'Website', label: 'Website' },
    { field: 'Phone', label: 'Phone' },
    { field: 'Email', label: 'Email' },
    { field: 'Address', label: 'Address' },
    { field: 'City', label: 'City' },
    { field: 'State', label: 'State' },
    { field: 'Country', label: 'Country' },
    { field: 'PostalCode', label: 'Postal Code' },
    { field: 'Revenue', label: 'Revenue' },
    { field: 'Employees', label: 'Employees' },
    { field: 'CreatedAt', label: 'Created Date' },
    { field: 'UpdatedAt', label: 'Updated Date' },
  ],
  Contacts: [
    { field: 'Id', label: 'ID' },
    { field: 'FirstName', label: 'First Name' },
    { field: 'LastName', label: 'Last Name' },
    { field: 'Email', label: 'Email' },
    { field: 'Phone', label: 'Phone' },
    { field: 'Title', label: 'Title' },
    { field: 'Department', label: 'Department' },
    { field: 'AccountName', label: 'Account' },
    { field: 'Address', label: 'Address' },
    { field: 'City', label: 'City' },
    { field: 'State', label: 'State' },
    { field: 'Country', label: 'Country' },
    { field: 'CreatedAt', label: 'Created Date' },
  ],
  Leads: [
    { field: 'Id', label: 'ID' },
    { field: 'FirstName', label: 'First Name' },
    { field: 'LastName', label: 'Last Name' },
    { field: 'Email', label: 'Email' },
    { field: 'Phone', label: 'Phone' },
    { field: 'Company', label: 'Company' },
    { field: 'Title', label: 'Title' },
    { field: 'Source', label: 'Source' },
    { field: 'Status', label: 'Status' },
    { field: 'Industry', label: 'Industry' },
    { field: 'CreatedAt', label: 'Created Date' },
  ],
  Opportunities: [
    { field: 'Id', label: 'ID' },
    { field: 'Name', label: 'Name' },
    { field: 'AccountName', label: 'Account' },
    { field: 'Stage', label: 'Stage' },
    { field: 'Amount', label: 'Amount' },
    { field: 'CloseDate', label: 'Close Date' },
    { field: 'Probability', label: 'Probability' },
    { field: 'Description', label: 'Description' },
    { field: 'Type', label: 'Type' },
    { field: 'CreatedAt', label: 'Created Date' },
  ],
  Products: [
    { field: 'Id', label: 'ID' },
    { field: 'Name', label: 'Name' },
    { field: 'SKU', label: 'SKU' },
    { field: 'Category', label: 'Category' },
    { field: 'Price', label: 'Price' },
    { field: 'Currency', label: 'Currency' },
    { field: 'Description', label: 'Description' },
    { field: 'IsActive', label: 'Active' },
    { field: 'CreatedAt', label: 'Created Date' },
  ],
};

const steps = ['Select Entity & Format', 'Configure Columns', 'Export & Download'];

function getStatusColor(status: string): 'default' | 'primary' | 'success' | 'error' | 'warning' {
  switch (status) {
    case 'Completed': return 'success';
    case 'Failed': return 'error';
    case 'Processing': return 'primary';
    case 'Pending': return 'warning';
    default: return 'default';
  }
}

// ============================================================================
// ExportWizardPage Component
// ============================================================================

function ExportWizardPage() {
  // Wizard state
  const [activeStep, setActiveStep] = useState(0);
  const [entityType, setEntityType] = useState('');
  const [exportFormat, setExportFormat] = useState('csv');

  // Filter state
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  // Column selection state
  const [selectedColumns, setSelectedColumns] = useState<Set<string>>(new Set());

  // Export state
  const [exportJob, setExportJob] = useState<ExportJobDto | null>(null);
  const [isExporting, setIsExporting] = useState(false);
  const [exportError, setExportError] = useState<string | null>(null);

  // Previous jobs
  const [previousJobs, setPreviousJobs] = useState<ExportJobDto[]>([]);
  const [loadingJobs, setLoadingJobs] = useState(false);

  // Initialize columns when entity changes
  useEffect(() => {
    if (entityType) {
      const columns = ENTITY_COLUMNS[entityType] || [];
      setSelectedColumns(new Set(columns.map(c => c.field)));
    } else {
      setSelectedColumns(new Set());
    }
  }, [entityType]);

  // Load previous export jobs
  const loadPreviousJobs = useCallback(async () => {
    setLoadingJobs(true);
    try {
      const jobs = await importExportService.getExportJobs();
      setPreviousJobs(jobs);
    } catch {
      setPreviousJobs([]);
    } finally {
      setLoadingJobs(false);
    }
  }, []);

  useEffect(() => {
    loadPreviousJobs();
  }, [loadPreviousJobs]);

  // ---- Column selection ----

  const handleToggleColumn = (field: string) => {
    setSelectedColumns(prev => {
      const next = new Set(prev);
      if (next.has(field)) {
        next.delete(field);
      } else {
        next.add(field);
      }
      return next;
    });
  };

  const handleSelectAll = () => {
    const columns = ENTITY_COLUMNS[entityType] || [];
    setSelectedColumns(new Set(columns.map(c => c.field)));
  };

  const handleDeselectAll = () => {
    setSelectedColumns(new Set());
  };

  // ---- Export ----

  const handleStartExport = async () => {
    if (!entityType || !exportFormat) return;
    setIsExporting(true);
    setExportError(null);
    try {
      const filters: Record<string, string> = {};
      if (dateFrom) filters.dateFrom = dateFrom;
      if (dateTo) filters.dateTo = dateTo;
      if (statusFilter) filters.status = statusFilter;
      const columns = Array.from(selectedColumns);
      if (columns.length > 0) filters.columns = columns.join(',');

      const job = await importExportService.startExport(entityType, exportFormat, filters);
      setExportJob(job);
      pollExportJob(job.id);
    } catch (err: unknown) {
      const message = err instanceof Error ? (err as Error).message : 'Export failed. Please try again.';
      setExportError(message);
      setIsExporting(false);
    }
  };

  const pollExportJob = (jobId: number) => {
    const interval = setInterval(async () => {
      try {
        const job = await importExportService.getExportJob(jobId);
        setExportJob(job);
        if (job.status === 'Completed' || job.status === 'Failed') {
          clearInterval(interval);
          setIsExporting(false);
          loadPreviousJobs();
        }
      } catch {
        clearInterval(interval);
        setIsExporting(false);
      }
    }, 2000);
  };

  const handleDownload = async (jobId: number) => {
    try {
      const blob = await importExportService.downloadExport(jobId);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${entityType}_export.${exportFormat}`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch {
      setExportError('Download failed. Please try again.');
    }
  };

  // ---- Navigation ----

  const canGoNext = (): boolean => {
    switch (activeStep) {
      case 0: return Boolean(entityType && exportFormat);
      case 1: return selectedColumns.size > 0;
      case 2: return false;
      default: return false;
    }
  };

  const handleNext = () => {
    setActiveStep(prev => Math.min(prev + 1, steps.length - 1));
  };

  const handleBack = () => {
    setActiveStep(prev => Math.max(prev - 1, 0));
  };

  const handleReset = () => {
    setActiveStep(0);
    setEntityType('');
    setExportFormat('csv');
    setDateFrom('');
    setDateTo('');
    setStatusFilter('');
    setSelectedColumns(new Set());
    setExportJob(null);
    setIsExporting(false);
    setExportError(null);
  };

  // ---- Render Steps ----

  const renderStep0 = () => (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <FormControl fullWidth>
        <InputLabel id="export-entity-label">Entity Type</InputLabel>
        <Select
          labelId="export-entity-label"
          value={entityType}
          label="Entity Type"
          onChange={(e: SelectChangeEvent) => setEntityType(e.target.value)}
        >
          {ENTITY_TYPES.map(et => (
            <MenuItem key={et.value} value={et.value}>{et.label}</MenuItem>
          ))}
        </Select>
      </FormControl>

      <Box>
        <Typography variant="subtitle1" gutterBottom fontWeight="bold">
          Export Format
        </Typography>
        <RadioGroup
          value={exportFormat}
          onChange={(e) => setExportFormat(e.target.value)}
        >
          {EXPORT_FORMATS.map(fmt => (
            <FormControlLabel
              key={fmt.value}
              value={fmt.value}
              control={<Radio />}
              label={
                <Box>
                  <Typography variant="body1">{fmt.label}</Typography>
                  <Typography variant="caption" color="text.secondary">{fmt.description}</Typography>
                </Box>
              }
              sx={{ mb: 1, alignItems: 'flex-start', '& .MuiRadio-root': { mt: -0.5 } }}
            />
          ))}
        </RadioGroup>
      </Box>

      <Divider />

      <Typography variant="subtitle1" fontWeight="bold">
        Filters (Optional)
      </Typography>
      <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
        <TextField
          label="Date From"
          type="date"
          value={dateFrom}
          onChange={(e) => setDateFrom(e.target.value)}
          InputLabelProps={{ shrink: true }}
          size="small"
          sx={{ minWidth: 180 }}
        />
        <TextField
          label="Date To"
          type="date"
          value={dateTo}
          onChange={(e) => setDateTo(e.target.value)}
          InputLabelProps={{ shrink: true }}
          size="small"
          sx={{ minWidth: 180 }}
        />
        <TextField
          label="Status"
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          size="small"
          placeholder="e.g., Active"
          sx={{ minWidth: 180 }}
        />
      </Box>
    </Box>
  );

  const renderStep1 = () => {
    const columns = ENTITY_COLUMNS[entityType] || [];
    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Typography variant="subtitle1" fontWeight="bold" sx={{ flexGrow: 1 }}>
            Select Columns to Export ({selectedColumns.size} of {columns.length} selected)
          </Typography>
          <Button size="small" startIcon={<SelectAllIcon />} onClick={handleSelectAll}>
            Select All
          </Button>
          <Button size="small" startIcon={<DeselectIcon />} onClick={handleDeselectAll}>
            Deselect All
          </Button>
        </Box>
        <Paper variant="outlined" sx={{ maxHeight: 400, overflow: 'auto' }}>
          {columns.map(col => (
            <Box
              key={col.field}
              sx={{
                display: 'flex',
                alignItems: 'center',
                px: 2,
                py: 0.5,
                '&:hover': { bgcolor: 'action.hover' },
                borderBottom: '1px solid',
                borderColor: 'divider',
              }}
            >
              <Checkbox
                checked={selectedColumns.has(col.field)}
                onChange={() => handleToggleColumn(col.field)}
                size="small"
              />
              <Box sx={{ flexGrow: 1 }}>
                <Typography variant="body2">{col.label}</Typography>
                <Typography variant="caption" color="text.secondary">{col.field}</Typography>
              </Box>
            </Box>
          ))}
        </Paper>
      </Box>
    );
  };

  const renderStep2 = () => {
    const columns = ENTITY_COLUMNS[entityType] || [];
    const selectedColumnLabels = columns.filter(c => selectedColumns.has(c.field));
    const formatLabel = EXPORT_FORMATS.find(f => f.value === exportFormat)?.label || exportFormat;

    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, alignItems: 'center' }}>
        {!exportJob && !isExporting && !exportError && (
          <Card variant="outlined" sx={{ width: '100%' }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>Export Summary</Typography>
              <Divider sx={{ mb: 2 }} />
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
                <Box sx={{ display: 'flex', gap: 1 }}>
                  <Typography variant="body2" color="text.secondary" sx={{ minWidth: 120 }}>Entity:</Typography>
                  <Typography variant="body2" fontWeight="medium">{entityType}</Typography>
                </Box>
                <Box sx={{ display: 'flex', gap: 1 }}>
                  <Typography variant="body2" color="text.secondary" sx={{ minWidth: 120 }}>Format:</Typography>
                  <Typography variant="body2" fontWeight="medium">{formatLabel}</Typography>
                </Box>
                <Box sx={{ display: 'flex', gap: 1 }}>
                  <Typography variant="body2" color="text.secondary" sx={{ minWidth: 120 }}>Columns:</Typography>
                  <Typography variant="body2" fontWeight="medium">{selectedColumns.size} selected</Typography>
                </Box>
                {(dateFrom || dateTo || statusFilter) && (
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    <Typography variant="body2" color="text.secondary" sx={{ minWidth: 120 }}>Filters:</Typography>
                    <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                      {dateFrom && <Chip label={`From: ${dateFrom}`} size="small" variant="outlined" />}
                      {dateTo && <Chip label={`To: ${dateTo}`} size="small" variant="outlined" />}
                      {statusFilter && <Chip label={`Status: ${statusFilter}`} size="small" variant="outlined" />}
                    </Box>
                  </Box>
                )}
                <Box sx={{ display: 'flex', gap: 1, alignItems: 'flex-start' }}>
                  <Typography variant="body2" color="text.secondary" sx={{ minWidth: 120 }}>Fields:</Typography>
                  <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                    {selectedColumnLabels.map(col => (
                      <Chip key={col.field} label={col.label} size="small" color="primary" variant="outlined" />
                    ))}
                  </Box>
                </Box>
              </Box>
              <Button
                variant="contained"
                size="large"
                startIcon={<StartIcon />}
                onClick={handleStartExport}
                sx={{ mt: 3 }}
                fullWidth
              >
                Start Export
              </Button>
            </CardContent>
          </Card>
        )}

        {isExporting && (
          <Box sx={{ width: '100%', textAlign: 'center' }}>
            <Typography variant="h6" gutterBottom>Exporting...</Typography>
            <LinearProgress sx={{ height: 10, borderRadius: 5, mb: 2 }} />
            {exportJob && (
              <Typography variant="body2" color="text.secondary">
                Status: {exportJob.status} — {exportJob.totalRecords} records
              </Typography>
            )}
          </Box>
        )}

        {exportError && (
          <Alert severity="error" sx={{ width: '100%' }}>
            {exportError}
            <Button size="small" onClick={handleStartExport} sx={{ mt: 1 }}>
              Retry
            </Button>
          </Alert>
        )}

        {exportJob && !isExporting && (
          <Card variant="outlined" sx={{ width: '100%' }}>
            <CardContent sx={{ textAlign: 'center' }}>
              <SuccessIcon color="success" sx={{ fontSize: 48, mb: 1 }} />
              <Typography variant="h6" gutterBottom>
                Export {exportJob.status}
              </Typography>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                {exportJob.totalRecords} records exported as {exportJob.format?.toUpperCase()}
              </Typography>
              {exportJob.status === 'Completed' && (
                <Button
                  variant="contained"
                  startIcon={<DownloadIcon />}
                  onClick={() => handleDownload(exportJob.id)}
                  sx={{ mt: 2, mr: 1 }}
                >
                  Download File
                </Button>
              )}
              <Button variant="outlined" onClick={handleReset} sx={{ mt: 2 }}>
                Start New Export
              </Button>
            </CardContent>
          </Card>
        )}
      </Box>
    );
  };

  // ============================================================================
  // Main Render
  // ============================================================================

  return (
    <Box sx={{ p: 3 }}>
      {/* Breadcrumbs */}
      <Breadcrumbs separator={<NextIcon fontSize="small" />} sx={{ mb: 2 }}>
        <Link component={RouterLink} to="/" color="inherit" sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
          <HomeIcon fontSize="small" /> Home
        </Link>
        <Typography color="text.primary">Export Data</Typography>
      </Breadcrumbs>

      <Typography variant="h4" gutterBottom>Export Wizard</Typography>
      <Typography variant="body2" color="text.secondary" gutterBottom>
        Export your CRM data to CSV, Excel, or JSON format.
      </Typography>

      {/* Stepper */}
      <Stepper activeStep={activeStep} sx={{ my: 3 }}>
        {steps.map(label => (
          <Step key={label}>
            <StepLabel>{label}</StepLabel>
          </Step>
        ))}
      </Stepper>

      {/* Step Content */}
      <Paper sx={{ p: 3, mb: 3 }}>
        {activeStep === 0 && renderStep0()}
        {activeStep === 1 && renderStep1()}
        {activeStep === 2 && renderStep2()}
      </Paper>

      {/* Navigation Buttons */}
      {activeStep < 2 && (
        <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
          <Button
            variant="outlined"
            component={RouterLink}
            to="/"
          >
            Cancel
          </Button>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button
              disabled={activeStep === 0}
              onClick={handleBack}
              startIcon={<BackIcon />}
            >
              Back
            </Button>
            <Button
              variant="contained"
              disabled={!canGoNext()}
              onClick={handleNext}
              endIcon={<NextIcon />}
            >
              Next
            </Button>
          </Box>
        </Box>
      )}

      {/* Previous Export Jobs */}
      <Divider sx={{ my: 4 }} />
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
        <Typography variant="h6">Previous Exports</Typography>
        <Tooltip title="Refresh">
          <IconButton size="small" onClick={loadPreviousJobs} disabled={loadingJobs}>
            <RefreshIcon />
          </IconButton>
        </Tooltip>
      </Box>
      {loadingJobs && <LinearProgress sx={{ mb: 2 }} />}
      {!loadingJobs && previousJobs.length === 0 && (
        <Typography variant="body2" color="text.secondary">No previous export jobs found.</Typography>
      )}
      {previousJobs.length > 0 && (
        <TableContainer component={Paper} variant="outlined">
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 'bold' }}>ID</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Entity</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Format</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Status</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Records</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Date</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {previousJobs.map(job => (
                <TableRow key={job.id}>
                  <TableCell>{job.id}</TableCell>
                  <TableCell>{job.entityType}</TableCell>
                  <TableCell>{job.format?.toUpperCase()}</TableCell>
                  <TableCell>
                    <Chip label={job.status} size="small" color={getStatusColor(job.status)} />
                  </TableCell>
                  <TableCell>{job.totalRecords}</TableCell>
                  <TableCell>{new Date(job.createdAt).toLocaleDateString()}</TableCell>
                  <TableCell>
                    {job.status === 'Completed' && (
                      <Tooltip title="Download">
                        <IconButton size="small" onClick={() => handleDownload(job.id)}>
                          <DownloadIcon />
                        </IconButton>
                      </Tooltip>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
}

export default ExportWizardPage;

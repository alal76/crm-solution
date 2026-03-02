/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This software is source-available. Non-commercial use is permitted under
 * the terms of the LICENSE file. Commercial use requires a separate license.
 * See the LICENSE file in the root directory for full terms.
 */

import { useState, useEffect, useCallback, useRef, DragEvent, ChangeEvent } from 'react';
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
  SelectChangeEvent,
} from '@mui/material';
import {
  CloudUpload as UploadIcon,
  NavigateNext as NextIcon,
  NavigateBefore as BackIcon,
  PlayArrow as StartIcon,
  CheckCircle as SuccessIcon,
  Error as ErrorIcon,
  Warning as WarningIcon,
  InsertDriveFile as FileIcon,
  Delete as DeleteIcon,
  Refresh as RefreshIcon,
  Home as HomeIcon,
} from '@mui/icons-material';
import { Link as RouterLink } from 'react-router-dom';
import importExportService, {
  ImportJobDto,
  ImportErrorDto,
  ColumnMappingDto,
} from '../services/importExportService';

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

const ENTITY_FIELDS: Record<string, string[]> = {
  Accounts: ['Name', 'Industry', 'Website', 'Phone', 'Email', 'Address', 'City', 'State', 'Country', 'PostalCode', 'Revenue', 'Employees'],
  Contacts: ['FirstName', 'LastName', 'Email', 'Phone', 'Title', 'Department', 'AccountName', 'Address', 'City', 'State', 'Country'],
  Leads: ['FirstName', 'LastName', 'Email', 'Phone', 'Company', 'Title', 'Source', 'Status', 'Industry'],
  Opportunities: ['Name', 'AccountName', 'Stage', 'Amount', 'CloseDate', 'Probability', 'Description', 'Type'],
  Products: ['Name', 'SKU', 'Category', 'Price', 'Currency', 'Description', 'IsActive'],
};

const ACCEPTED_FILE_TYPES = '.csv,.xlsx,.json';

const steps = ['Select Entity & Upload', 'Column Mapping', 'Preview & Validate', 'Import'];

// ============================================================================
// Helper: parse CSV header from file
// ============================================================================

function parseFileHeaders(file: File): Promise<{ headers: string[]; previewRows: Record<string, string>[] }> {
  return new Promise((resolve) => {
    const reader = new FileReader();
    reader.onload = (e) => {
      const text = e.target?.result as string;
      if (!text) {
        resolve({ headers: [], previewRows: [] });
        return;
      }
      const lines = text.split('\n').filter(l => l.trim());
      if (lines.length === 0) {
        resolve({ headers: [], previewRows: [] });
        return;
      }
      const headers = lines[0].split(',').map(h => h.trim().replaceAll(/^"|"$/g, ''));
      const previewRows: Record<string, string>[] = [];
      for (let i = 1; i < Math.min(lines.length, 4); i++) {
        const values = lines[i].split(',').map(v => v.trim().replaceAll(/^"|"$/g, ''));
        const row: Record<string, string> = {};
        headers.forEach((h, idx) => {
          row[h] = values[idx] || '';
        });
        previewRows.push(row);
      }
      resolve({ headers, previewRows });
    };
    reader.onerror = () => resolve({ headers: [], previewRows: [] });
    reader.readAsText(file.slice(0, 50000)); // Read first 50KB
  });
}

function autoDetectMappings(sourceColumns: string[], targetFields: string[]): ColumnMappingDto[] {
  return sourceColumns.map(source => {
    const normalizedSource = source.toLowerCase().replaceAll(/[_\s-]/g, '');
    const match = targetFields.find(
      f => f.toLowerCase().replaceAll(/[_\s-]/g, '') === normalizedSource
    );
    return { sourceColumn: source, targetField: match || '' };
  });
}

function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

function getStatusColor(status: string): 'default' | 'primary' | 'success' | 'error' | 'warning' {
  switch (status) {
    case 'Completed': return 'success';
    case 'Failed': return 'error';
    case 'Processing':
    case 'Validating': return 'primary';
    case 'Pending': return 'warning';
    default: return 'default';
  }
}

// ============================================================================
// ImportWizardPage Component
// ============================================================================

function ImportWizardPage() {
  // Wizard state
  const [activeStep, setActiveStep] = useState(0);
  const [entityType, setEntityType] = useState('');
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [isDragOver, setIsDragOver] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Parsing state
  const [fileHeaders, setFileHeaders] = useState<string[]>([]);
  const [previewRows, setPreviewRows] = useState<Record<string, string>[]>([]);
  const [mappings, setMappings] = useState<ColumnMappingDto[]>([]);

  // Validation state
  const [validationErrors, setValidationErrors] = useState<ImportErrorDto[]>([]);
  const [isValidating, setIsValidating] = useState(false);
  const [validationDone, setValidationDone] = useState(false);

  // Import state
  const [importJob, setImportJob] = useState<ImportJobDto | null>(null);
  const [isImporting, setIsImporting] = useState(false);
  const [importError, setImportError] = useState<string | null>(null);

  // Previous jobs
  const [previousJobs, setPreviousJobs] = useState<ImportJobDto[]>([]);
  const [loadingJobs, setLoadingJobs] = useState(false);

  // Load previous import jobs
  const loadPreviousJobs = useCallback(async () => {
    setLoadingJobs(true);
    try {
      const jobs = await importExportService.getImportJobs();
      setPreviousJobs(jobs);
    } catch {
      // Silently handle — API may not exist yet
      setPreviousJobs([]);
    } finally {
      setLoadingJobs(false);
    }
  }, []);

  useEffect(() => {
    loadPreviousJobs();
  }, [loadPreviousJobs]);

  // Parse file when selected
  useEffect(() => {
    if (!selectedFile) {
      setFileHeaders([]);
      setPreviewRows([]);
      setMappings([]);
      return;
    }
    const parse = async () => {
      const { headers, previewRows: rows } = await parseFileHeaders(selectedFile);
      setFileHeaders(headers);
      setPreviewRows(rows);
      if (entityType && headers.length > 0) {
        const targetFields = ENTITY_FIELDS[entityType] || [];
        setMappings(autoDetectMappings(headers, targetFields));
      }
    };
    parse();
  }, [selectedFile, entityType]);

  // ---- File handling ----

  const handleFileSelect = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) setSelectedFile(file);
  };

  const handleDragOver = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragOver(true);
  };

  const handleDragLeave = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragOver(false);
  };

  const handleDrop = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragOver(false);
    const file = e.dataTransfer.files?.[0];
    if (file) setSelectedFile(file);
  };

  const handleRemoveFile = () => {
    setSelectedFile(null);
    if (fileInputRef.current) fileInputRef.current.value = '';
  };

  // ---- Mapping ----

  const handleMappingChange = (sourceColumn: string, targetField: string) => {
    setMappings(prev =>
      prev.map(m =>
        m.sourceColumn === sourceColumn ? { ...m, targetField } : m
      )
    );
  };

  // ---- Validation ----

  const handleValidate = async () => {
    if (!selectedFile || !entityType) return;
    setIsValidating(true);
    setValidationErrors([]);
    try {
      const result = await importExportService.validateImport(entityType, selectedFile);
      setValidationErrors(result.errors);
      setValidationDone(true);
      if (result.previewRows.length > 0) {
        setPreviewRows(result.previewRows);
      }
    } catch {
      setValidationErrors([{ rowNumber: 0, field: '', message: 'Validation request failed. The API endpoint may not be available yet.' }]);
      setValidationDone(true);
    } finally {
      setIsValidating(false);
    }
  };

  // ---- Import ----

  const handleStartImport = async () => {
    if (!selectedFile || !entityType) return;
    setIsImporting(true);
    setImportError(null);
    try {
      const activeMappings = mappings.filter(m => m.targetField !== '');
      const job = await importExportService.startImport(entityType, selectedFile, activeMappings);
      setImportJob(job);
      // Poll for updates
      pollImportJob(job.id);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Import failed. Please try again.';
      setImportError(message);
      setIsImporting(false);
    }
  };

  const pollImportJob = (jobId: number) => {
    const interval = setInterval(async () => {
      try {
        const job = await importExportService.getImportJob(jobId);
        setImportJob(job);
        if (job.status === 'Completed' || job.status === 'Failed') {
          clearInterval(interval);
          setIsImporting(false);
          loadPreviousJobs();
        }
      } catch {
        clearInterval(interval);
        setIsImporting(false);
      }
    }, 2000);
  };

  // ---- Navigation ----

  const canGoNext = (): boolean => {
    switch (activeStep) {
      case 0: return Boolean(entityType && selectedFile);
      case 1: return mappings.some(m => m.targetField !== '');
      case 2: return validationDone;
      case 3: return false;
      default: return false;
    }
  };

  const handleNext = () => {
    if (activeStep === 2 && !validationDone) {
      handleValidate();
      return;
    }
    setActiveStep(prev => Math.min(prev + 1, steps.length - 1));
  };

  const handleBack = () => {
    setActiveStep(prev => Math.max(prev - 1, 0));
  };

  const handleReset = () => {
    setActiveStep(0);
    setEntityType('');
    setSelectedFile(null);
    setFileHeaders([]);
    setPreviewRows([]);
    setMappings([]);
    setValidationErrors([]);
    setIsValidating(false);
    setValidationDone(false);
    setImportJob(null);
    setIsImporting(false);
    setImportError(null);
  };

  // ---- Render Steps ----

  const renderStep0 = () => (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <FormControl fullWidth>
        <InputLabel id="entity-type-label">Entity Type</InputLabel>
        <Select
          labelId="entity-type-label"
          value={entityType}
          label="Entity Type"
          onChange={(e: SelectChangeEvent) => setEntityType(e.target.value)}
        >
          {ENTITY_TYPES.map(et => (
            <MenuItem key={et.value} value={et.value}>{et.label}</MenuItem>
          ))}
        </Select>
      </FormControl>

      <Paper
        variant="outlined"
        sx={{
          p: 4,
          textAlign: 'center',
          cursor: 'pointer',
          bgcolor: isDragOver ? 'action.hover' : 'background.paper',
          borderStyle: 'dashed',
          borderColor: isDragOver ? 'primary.main' : 'divider',
          transition: 'all 0.2s',
          '&:hover': { borderColor: 'primary.main', bgcolor: 'action.hover' },
        }}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
        onClick={() => fileInputRef.current?.click()}
      >
        <input
          ref={fileInputRef}
          type="file"
          accept={ACCEPTED_FILE_TYPES}
          onChange={handleFileSelect}
          style={{ display: 'none' }}
        />
        <UploadIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 1 }} />
        <Typography variant="h6" gutterBottom>
          Drag & drop a file here, or click to browse
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Supported formats: CSV, Excel (.xlsx), JSON
        </Typography>
      </Paper>

      {selectedFile && (
        <Paper variant="outlined" sx={{ p: 2, display: 'flex', alignItems: 'center', gap: 2 }}>
          <FileIcon color="primary" />
          <Box sx={{ flexGrow: 1 }}>
            <Typography variant="body1" fontWeight="medium">{selectedFile.name}</Typography>
            <Typography variant="body2" color="text.secondary">
              {formatFileSize(selectedFile.size)} &bull; {selectedFile.type || 'unknown type'}
            </Typography>
          </Box>
          <Tooltip title="Remove file">
            <IconButton onClick={handleRemoveFile} size="small">
              <DeleteIcon />
            </IconButton>
          </Tooltip>
        </Paper>
      )}
    </Box>
  );

  const renderStep1 = () => {
    const targetFields = ENTITY_FIELDS[entityType] || [];
    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        <Alert severity="info" sx={{ mb: 1 }}>
          Map source columns from your file to the target entity fields. Unmapped columns will be skipped.
        </Alert>
        <TableContainer component={Paper} variant="outlined">
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 'bold' }}>Source Column</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Sample Data</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Target Field</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {mappings.map(mapping => (
                <TableRow key={mapping.sourceColumn}>
                  <TableCell>
                    <Typography variant="body2" fontWeight="medium">{mapping.sourceColumn}</Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" color="text.secondary">
                      {previewRows[0]?.[mapping.sourceColumn] || '—'}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <FormControl fullWidth size="small">
                      <Select
                        value={mapping.targetField}
                        displayEmpty
                        onChange={(e: SelectChangeEvent) =>
                          handleMappingChange(mapping.sourceColumn, e.target.value)
                        }
                      >
                        <MenuItem value="">
                          <em>Skip (unmapped)</em>
                        </MenuItem>
                        {targetFields.map(field => (
                          <MenuItem key={field} value={field}>{field}</MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>

        {previewRows.length > 0 && (
          <Box>
            <Typography variant="subtitle2" gutterBottom sx={{ mt: 2 }}>
              Preview (first {previewRows.length} rows)
            </Typography>
            <TableContainer component={Paper} variant="outlined" sx={{ maxHeight: 200, overflow: 'auto' }}>
              <Table size="small" stickyHeader>
                <TableHead>
                  <TableRow>
                    {fileHeaders.map(h => (
                      <TableCell key={h} sx={{ fontWeight: 'bold', fontSize: '0.75rem' }}>{h}</TableCell>
                    ))}
                  </TableRow>
                </TableHead>
                <TableBody>
                  {previewRows.map((row, idx) => (
                    <TableRow key={idx}>
                      {fileHeaders.map(h => (
                        <TableCell key={h} sx={{ fontSize: '0.75rem' }}>{row[h] || ''}</TableCell>
                      ))}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </Box>
        )}
      </Box>
    );
  };

  const renderStep2 = () => {
    const activeMappings = mappings.filter(m => m.targetField !== '');
    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        <Typography variant="subtitle1" fontWeight="bold">
          Mapped Columns Summary
        </Typography>
        <Paper variant="outlined" sx={{ p: 2 }}>
          {activeMappings.length === 0 ? (
            <Alert severity="warning">No columns mapped. Go back and map at least one column.</Alert>
          ) : (
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
              {activeMappings.map(m => (
                <Chip
                  key={m.sourceColumn}
                  label={`${m.sourceColumn} → ${m.targetField}`}
                  color="primary"
                  variant="outlined"
                  size="small"
                />
              ))}
            </Box>
          )}
        </Paper>

        {!validationDone && (
          <Button
            variant="contained"
            onClick={handleValidate}
            disabled={isValidating}
            startIcon={isValidating ? undefined : <StartIcon />}
          >
            {isValidating ? 'Validating...' : 'Validate Data'}
          </Button>
        )}

        {isValidating && <LinearProgress sx={{ mt: 1 }} />}

        {validationDone && validationErrors.length === 0 && (
          <Alert severity="success" icon={<SuccessIcon />}>
            Validation passed! No errors found. You can proceed with the import.
          </Alert>
        )}

        {validationDone && validationErrors.length > 0 && (
          <Box>
            <Alert severity="warning" icon={<WarningIcon />} sx={{ mb: 2 }}>
              {validationErrors.length} validation issue(s) found. Review below and fix if needed.
            </Alert>
            <TableContainer component={Paper} variant="outlined" sx={{ maxHeight: 250 }}>
              <Table size="small" stickyHeader>
                <TableHead>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 'bold' }}>Row</TableCell>
                    <TableCell sx={{ fontWeight: 'bold' }}>Field</TableCell>
                    <TableCell sx={{ fontWeight: 'bold' }}>Message</TableCell>
                    <TableCell sx={{ fontWeight: 'bold' }}>Value</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {validationErrors.slice(0, 50).map((err, idx) => (
                    <TableRow key={idx}>
                      <TableCell>{err.rowNumber || '—'}</TableCell>
                      <TableCell>{err.field || '—'}</TableCell>
                      <TableCell>{err.message}</TableCell>
                      <TableCell>{err.value || '—'}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
            {validationErrors.length > 50 && (
              <Typography variant="caption" color="text.secondary" sx={{ mt: 1 }}>
                Showing first 50 of {validationErrors.length} errors.
              </Typography>
            )}
          </Box>
        )}

        {validationDone && previewRows.length > 0 && (
          <Box>
            <Typography variant="subtitle2" gutterBottom sx={{ mt: 2 }}>
              Data Preview (first {Math.min(previewRows.length, 10)} rows)
            </Typography>
            <TableContainer component={Paper} variant="outlined" sx={{ maxHeight: 300 }}>
              <Table size="small" stickyHeader>
                <TableHead>
                  <TableRow>
                    {fileHeaders.map(h => (
                      <TableCell key={h} sx={{ fontWeight: 'bold', fontSize: '0.75rem' }}>{h}</TableCell>
                    ))}
                  </TableRow>
                </TableHead>
                <TableBody>
                  {previewRows.slice(0, 10).map((row, idx) => (
                    <TableRow key={idx}>
                      {fileHeaders.map(h => (
                        <TableCell key={h} sx={{ fontSize: '0.75rem' }}>{row[h] || ''}</TableCell>
                      ))}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </Box>
        )}
      </Box>
    );
  };

  const renderStep3 = () => {
    const progressPercent = importJob && importJob.totalRecords > 0
      ? Math.round((importJob.processedRecords / importJob.totalRecords) * 100)
      : 0;

    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, alignItems: 'center' }}>
        {!importJob && !isImporting && !importError && (
          <Box sx={{ textAlign: 'center' }}>
            <Typography variant="h6" gutterBottom>Ready to Import</Typography>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              {entityType} — {selectedFile?.name} — {mappings.filter(m => m.targetField).length} mapped columns
            </Typography>
            <Button
              variant="contained"
              size="large"
              startIcon={<StartIcon />}
              onClick={handleStartImport}
              sx={{ mt: 2 }}
            >
              Start Import
            </Button>
          </Box>
        )}

        {isImporting && (
          <Box sx={{ width: '100%', textAlign: 'center' }}>
            <Typography variant="h6" gutterBottom>Importing...</Typography>
            <LinearProgress
              variant={importJob ? 'determinate' : 'indeterminate'}
              value={progressPercent}
              sx={{ height: 10, borderRadius: 5, mb: 2 }}
            />
            {importJob && (
              <Typography variant="body2" color="text.secondary">
                {importJob.processedRecords} / {importJob.totalRecords} records ({progressPercent}%)
              </Typography>
            )}
          </Box>
        )}

        {importError && (
          <Alert severity="error" sx={{ width: '100%' }}>
            {importError}
            <Button size="small" onClick={handleStartImport} sx={{ mt: 1 }}>
              Retry
            </Button>
          </Alert>
        )}

        {importJob && !isImporting && (
          <Card variant="outlined" sx={{ width: '100%' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                {importJob.status === 'Completed' ? (
                  <SuccessIcon color="success" sx={{ fontSize: 32 }} />
                ) : (
                  <ErrorIcon color="error" sx={{ fontSize: 32 }} />
                )}
                <Typography variant="h6">
                  Import {importJob.status}
                </Typography>
              </Box>
              <Divider sx={{ mb: 2 }} />
              <Box sx={{ display: 'flex', gap: 4, flexWrap: 'wrap' }}>
                <Box>
                  <Typography variant="caption" color="text.secondary">Total Records</Typography>
                  <Typography variant="h5">{importJob.totalRecords}</Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">Processed</Typography>
                  <Typography variant="h5" color="success.main">{importJob.processedRecords}</Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">Failed</Typography>
                  <Typography variant="h5" color="error.main">{importJob.failedRecords}</Typography>
                </Box>
              </Box>

              {importJob.errors && importJob.errors.length > 0 && (
                <Box sx={{ mt: 3 }}>
                  <Typography variant="subtitle2" gutterBottom>Errors</Typography>
                  <TableContainer component={Paper} variant="outlined" sx={{ maxHeight: 200 }}>
                    <Table size="small" stickyHeader>
                      <TableHead>
                        <TableRow>
                          <TableCell sx={{ fontWeight: 'bold' }}>Row</TableCell>
                          <TableCell sx={{ fontWeight: 'bold' }}>Field</TableCell>
                          <TableCell sx={{ fontWeight: 'bold' }}>Message</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {importJob.errors.map((err, idx) => (
                          <TableRow key={idx}>
                            <TableCell>{err.rowNumber}</TableCell>
                            <TableCell>{err.field}</TableCell>
                            <TableCell>{err.message}</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                </Box>
              )}

              <Button variant="outlined" onClick={handleReset} sx={{ mt: 3 }}>
                Start New Import
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
        <Typography color="text.primary">Import Data</Typography>
      </Breadcrumbs>

      <Typography variant="h4" gutterBottom>Import Wizard</Typography>
      <Typography variant="body2" color="text.secondary" gutterBottom>
        Import data from CSV, Excel, or JSON files into your CRM entities.
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
        {activeStep === 3 && renderStep3()}
      </Paper>

      {/* Navigation Buttons */}
      {activeStep < 3 && (
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
              {activeStep === 2 && !validationDone ? 'Validate' : 'Next'}
            </Button>
          </Box>
        </Box>
      )}

      {/* Previous Import Jobs */}
      <Divider sx={{ my: 4 }} />
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
        <Typography variant="h6">Previous Imports</Typography>
        <Tooltip title="Refresh">
          <IconButton size="small" onClick={loadPreviousJobs} disabled={loadingJobs}>
            <RefreshIcon />
          </IconButton>
        </Tooltip>
      </Box>
      {loadingJobs && <LinearProgress sx={{ mb: 2 }} />}
      {!loadingJobs && previousJobs.length === 0 && (
        <Typography variant="body2" color="text.secondary">No previous import jobs found.</Typography>
      )}
      {previousJobs.length > 0 && (
        <TableContainer component={Paper} variant="outlined">
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 'bold' }}>ID</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Entity</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>File</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Status</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Records</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Failed</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Date</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {previousJobs.map(job => (
                <TableRow key={job.id}>
                  <TableCell>{job.id}</TableCell>
                  <TableCell>{job.entityType}</TableCell>
                  <TableCell>{job.fileName}</TableCell>
                  <TableCell>
                    <Chip label={job.status} size="small" color={getStatusColor(job.status)} />
                  </TableCell>
                  <TableCell>{job.totalRecords}</TableCell>
                  <TableCell>{job.failedRecords}</TableCell>
                  <TableCell>{new Date(job.createdAt).toLocaleDateString()}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
}

export default ImportWizardPage;

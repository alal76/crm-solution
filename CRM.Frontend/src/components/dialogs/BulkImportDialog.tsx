/**
 * BulkImportDialog.tsx
 * CSV file upload and import for accounts with validation
 * Displays preview of rows with error indicators before importing
 */
import React, { useState, useRef } from 'react';
import {
  Dialog, DialogTitle, DialogContent, DialogActions, Button, Box, Alert,
  CircularProgress, Stepper, Step, StepLabel, Typography, Paper,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Chip, Card, CardContent
} from '@mui/material';
import { 
  Upload as UploadIcon, 
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Close as CloseIcon 
} from '@mui/icons-material';
import apiClient from '../../services/apiClient';
import { createBulkAccountValidationSchema, BulkImportRow } from '../../validation/accountSchema';
import logger from '../../services/logger';

interface BulkImportDialogProps {
  open: boolean;
  onClose: () => void;
  onImportComplete: (count: number) => void;
}

/**
 * Parse CSV content into rows
 */
const parseCSV = (content: string): string[][] => {
  const lines = content.trim().split('\n');
  return lines.map(line => {
    // Simple CSV parsing: handle quoted fields with commas
    const result: string[] = [];
    let current = '';
    let inQuotes = false;

    for (let i = 0; i < line.length; i++) {
      const char = line[i];
      if (char === '"') {
        inQuotes = !inQuotes;
      } else if (char === ',' && !inQuotes) {
        result.push(current.trim().replace(/^"|"$/g, ''));
        current = '';
      } else {
        current += char;
      }
    }
    result.push(current.trim().replace(/^"|"$/g, ''));
    return result;
  });
};

/**
 * Map CSV row to account object
 */
const mapRowToAccount = (headers: string[], row: string[]): Record<string, string> => {
  const account: Record<string, string> = {};
  headers.forEach((header, idx) => {
    if (row[idx] !== undefined) {
      account[header] = row[idx];
    }
  });
  return account;
};

const BulkImportDialog: React.FC<BulkImportDialogProps> = ({
  open,
  onClose,
  onImportComplete,
}) => {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [step, setStep] = useState(0); // 0: Upload, 1: Preview, 2: Importing, 3: Complete
  const [file, setFile] = useState<File | null>(null);
  const [fileError, setFileError] = useState<string | null>(null);
  const [previewRows, setPreviewRows] = useState<BulkImportRow[]>([]);
  const [importError, setImportError] = useState<string | null>(null);
  const [importSuccess, setImportSuccess] = useState<string | null>(null);
  const [importedCount, setImportedCount] = useState(0);

  const validationSchema = createBulkAccountValidationSchema();

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = event.target.files?.[0];
    setFileError(null);

    if (!selectedFile) return;

    // Validate file type
    if (!selectedFile.name.endsWith('.csv')) {
      setFileError('Please select a CSV file');
      return;
    }

    if (selectedFile.size > 10 * 1024 * 1024) { // 10MB max
      setFileError('File size exceeds 10MB limit');
      return;
    }

    setFile(selectedFile);
  };

  const handleParse = async () => {
    if (!file) return;

    try {
      setFileError(null);
      const content = await file.text();
      const rows = parseCSV(content);

      if (rows.length < 2) {
        setFileError('CSV must contain header row and at least one data row');
        return;
      }

      const headers = rows[0];
      const expectedHeaders = ['FirstName', 'LastName', 'Email', 'Phone', 'Company', 'Category', 'Industry'];
      const hasRequiredHeaders = ['FirstName', 'LastName', 'Email'].every(h => 
        headers.some(hdr => hdr.toLowerCase() === h.toLowerCase())
      );

      if (!hasRequiredHeaders) {
        setFileError(`CSV must contain required headers: FirstName, LastName, Email`);
        return;
      }

      // Validate each row
      const validationPromises = rows.slice(1).map(async (row, idx) => {
        const account = mapRowToAccount(headers, row);
        const errors: string[] = [];

        // Normalize header names for validation (FirstName → firstName)
        const normalizedAccount: Record<string, string> = {};
        Object.entries(account).forEach(([key, value]) => {
          const normalizedKey = key.charAt(0).toLowerCase() + key.slice(1);
          normalizedAccount[normalizedKey] = value;
        });

        try {
          await validationSchema.validate(normalizedAccount, { abortEarly: false });
        } catch (validationError: any) {
          if (validationError.inner) {
            validationError.inner.forEach((error: any) => {
              errors.push(error.message);
            });
          }
        }

        return {
          rowNumber: idx + 2, // +2 because row 1 is header, +1 for 1-based indexing
          data: normalizedAccount,
          errors,
          isValid: errors.length === 0,
        };
      });

      const validated = await Promise.all(validationPromises);
      setPreviewRows(validated);
      setStep(1); // Move to preview step
    } catch (err: any) {
      setFileError(`Failed to parse CSV: ${err.message}`);
    }
  };

  const handleImport = async () => {
    const validRows = previewRows.filter(r => r.isValid);

    if (validRows.length === 0) {
      setImportError('No valid rows to import. Please fix errors and try again.');
      return;
    }

    setStep(2); // Move to importing step
    setImportError(null);
    setImportSuccess(null);

    try {
      // Transform data for API
      const accountsToCreate = validRows.map(row => ({
        firstName: row.data.firstName || '',
        lastName: row.data.lastName || '',
        email: row.data.email || '',
        phone: row.data.phone || null,
        company: row.data.company || null,
        category: row.data.category ? parseInt(row.data.category, 10) : 1,
        industry: row.data.industry || null,
      }));

      // Batch import API call
      const response = await apiClient.post('/accounts/batch', {
        accounts: accountsToCreate,
      });

      const successCount = response.data.successCount || accountsToCreate.length;
      setImportedCount(successCount);
      setImportSuccess(`✓ ${successCount} of ${validRows.length} accounts imported successfully`);
      setStep(3); // Move to complete step

      // Notify parent of completion after delay
      setTimeout(() => {
        onImportComplete(successCount);
      }, 2000);
    } catch (err: any) {
      setImportError(err.response?.data?.message || 'Failed to import accounts');
      setStep(1); // Back to preview
    }
  };

  const handleClose = () => {
    setStep(0);
    setFile(null);
    setFileError(null);
    setPreviewRows([]);
    setImportError(null);
    setImportSuccess(null);
    setImportedCount(0);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
    onClose();
  };

  const validRowCount = previewRows.filter(r => r.isValid).length;
  const invalidRowCount = previewRows.filter(r => !r.isValid).length;

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="md" fullWidth>
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <UploadIcon />
        Bulk Import Accounts
      </DialogTitle>

      <DialogContent sx={{ py: 2 }}>
        <Stepper activeStep={step} sx={{ mb: 3 }}>
          <Step completed={step > 0}>
            <StepLabel>Upload CSV</StepLabel>
          </Step>
          <Step completed={step > 1}>
            <StepLabel>Preview</StepLabel>
          </Step>
          <Step completed={step > 2}>
            <StepLabel>Import</StepLabel>
          </Step>
        </Stepper>

        {/* Step 0: File Upload */}
        {step === 0 && (
          <Box>
            <Alert severity="info" sx={{ mb: 2 }}>
              Expected CSV headers: <strong>FirstName, LastName, Email, Phone, Company, Category, Industry</strong>
            </Alert>
            <Paper
              sx={{
                p: 3,
                textAlign: 'center',
                border: '2px dashed #6750A4',
                borderRadius: 2,
                cursor: 'pointer',
                backgroundColor: '#F5EFF7',
                transition: 'all 0.3s ease',
                '&:hover': { backgroundColor: '#E8DAEF', borderColor: '#4A235A' }
              }}
              onClick={() => fileInputRef.current?.click()}
            >
              <input
                ref={fileInputRef}
                type="file"
                accept=".csv"
                onChange={handleFileSelect}
                style={{ display: 'none' }}
              />
              <UploadIcon sx={{ fontSize: 48, color: '#6750A4', mb: 1 }} />
              <Typography variant="h6">
                {file ? file.name : 'Click to select or drag CSV file here'}
              </Typography>
              <Typography variant="caption" color="textSecondary">
                Maximum file size: 10MB
              </Typography>
            </Paper>
            {fileError && (
              <Alert severity="error" sx={{ mt: 2 }}>
                {fileError}
              </Alert>
            )}
          </Box>
        )}

        {/* Step 1: Preview */}
        {step === 1 && (
          <Box>
            <Card sx={{ mb: 2 }}>
              <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Box>
                    <Typography variant="body2" color="textSecondary">Total Rows</Typography>
                    <Typography variant="h6">{previewRows.length}</Typography>
                  </Box>
                  <Box sx={{ color: 'green' }}>
                    <CheckCircleIcon sx={{ fontSize: 24 }} />
                    <Typography variant="body2">Valid: {validRowCount}</Typography>
                  </Box>
                  {invalidRowCount > 0 && (
                    <Box sx={{ color: 'red' }}>
                      <ErrorIcon sx={{ fontSize: 24 }} />
                      <Typography variant="body2">Invalid: {invalidRowCount}</Typography>
                    </Box>
                  )}
                </Box>
              </CardContent>
            </Card>

            <TableContainer sx={{ maxHeight: 400 }}>
              <Table size="small">
                <TableHead>
                  <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                    <TableCell width={60}>Row</TableCell>
                    <TableCell>First Name</TableCell>
                    <TableCell>Last Name</TableCell>
                    <TableCell>Email</TableCell>
                    <TableCell width={80}>Status</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {previewRows.slice(0, 20).map((row) => (
                    <TableRow key={row.rowNumber} sx={{ backgroundColor: row.isValid ? 'inherit' : '#FFEBEE' }}>
                      <TableCell>{row.rowNumber}</TableCell>
                      <TableCell>{row.data.firstName}</TableCell>
                      <TableCell>{row.data.lastName}</TableCell>
                      <TableCell>{row.data.email}</TableCell>
                      <TableCell>
                        {row.isValid ? (
                          <Chip label="✓ Valid" size="small" color="success" variant="filled" />
                        ) : (
                          <Chip
                            label="✗ Invalid"
                            size="small"
                            color="error"
                            variant="filled"
                            title={row.errors.join('; ')}
                          />
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>

            {previewRows.length > 20 && (
              <Typography variant="caption" color="textSecondary" sx={{ display: 'block', mt: 1 }}>
                Showing 20 of {previewRows.length} rows
              </Typography>
            )}

            {invalidRowCount > 0 && (
              <Alert severity="warning" sx={{ mt: 2 }}>
                {invalidRowCount} row(s) have validation errors and will be skipped during import
              </Alert>
            )}
          </Box>
        )}

        {/* Step 2: Importing */}
        {step === 2 && (
          <Box sx={{ textAlign: 'center', py: 4 }}>
            <CircularProgress sx={{ mb: 2 }} />
            <Typography>Importing {validRowCount} account(s)...</Typography>
          </Box>
        )}

        {/* Step 3: Complete */}
        {step === 3 && (
          <Box>
            <Alert severity="success" icon={<CheckCircleIcon />}>
              {importSuccess}
            </Alert>
          </Box>
        )}

        {importError && (
          <Alert severity="error" sx={{ mt: 2 }}>
            {importError}
          </Alert>
        )}
      </DialogContent>

      <DialogActions>
        <Button onClick={handleClose} color="inherit">
          Close
        </Button>
        {step === 0 && (
          <Button
            onClick={handleParse}
            variant="contained"
            disabled={!file}
            sx={{ backgroundColor: '#6750A4' }}
          >
            Next
          </Button>
        )}
        {step === 1 && (
          <>
            <Button onClick={() => { setStep(0); setFile(null); setPreviewRows([]); }} color="inherit">
              Back
            </Button>
            <Button
              onClick={handleImport}
              variant="contained"
              disabled={validRowCount === 0}
              sx={{ backgroundColor: '#6750A4' }}
            >
              Import ({validRowCount} valid)
            </Button>
          </>
        )}
      </DialogActions>
    </Dialog>
  );
};

export default BulkImportDialog;

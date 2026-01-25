import React, { useState, useRef } from 'react';
import {
  Box,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Typography,
  CircularProgress,
  Alert,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormControlLabel,
  Checkbox,
  Stack,
  Chip,
  Divider,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  IconButton,
  Tooltip,
} from '@mui/material';
import FileDownloadIcon from '@mui/icons-material/FileDownload';
import FileUploadIcon from '@mui/icons-material/FileUpload';
import DescriptionIcon from '@mui/icons-material/Description';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import ErrorIcon from '@mui/icons-material/Error';
import WarningIcon from '@mui/icons-material/Warning';
import CloseIcon from '@mui/icons-material/Close';
import apiClient from '../services/apiClient';

interface ImportExportButtonsProps {
  entityType: string;
  entityLabel: string;
  onImportComplete?: () => void;
  variant?: 'buttons' | 'icons' | 'menu';
}

interface ImportResult {
  entityType: string;
  total: number;
  imported: number;
  skipped: number;
  failed: number;
  errors: string[];
}

export const ImportExportButtons: React.FC<ImportExportButtonsProps> = ({
  entityType,
  entityLabel,
  onImportComplete,
  variant = 'buttons',
}) => {
  const [importDialogOpen, setImportDialogOpen] = useState(false);
  const [exportDialogOpen, setExportDialogOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [importResult, setImportResult] = useState<ImportResult | null>(null);
  const [exportFormat, setExportFormat] = useState<'json' | 'csv'>('json');
  const [skipDuplicates, setSkipDuplicates] = useState(true);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleExport = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await apiClient.get(`/import-export/export/${entityType}?format=${exportFormat}`, {
        responseType: 'blob',
      });

      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `${entityType}_${new Date().toISOString().split('T')[0]}.${exportFormat}`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);

      setExportDialogOpen(false);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to export data');
    } finally {
      setLoading(false);
    }
  };

  const handleDownloadTemplate = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await apiClient.get(`/import-export/template/${entityType}?format=${exportFormat}`, {
        responseType: 'blob',
      });

      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `${entityType}_template.${exportFormat}`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to download template');
    } finally {
      setLoading(false);
    }
  };

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      setSelectedFile(file);
      setError(null);
      setImportResult(null);
    }
  };

  const handleImport = async () => {
    if (!selectedFile) {
      setError('Please select a file to import');
      return;
    }

    try {
      setLoading(true);
      setError(null);
      setImportResult(null);

      const formData = new FormData();
      formData.append('file', selectedFile);

      const response = await apiClient.post(
        `/import-export/import/${entityType}?skipDuplicates=${skipDuplicates}`,
        formData,
        {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
        }
      );

      setImportResult(response.data);
      
      if (response.data.imported > 0 && onImportComplete) {
        onImportComplete();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to import data');
    } finally {
      setLoading(false);
    }
  };

  const resetImportDialog = () => {
    setSelectedFile(null);
    setImportResult(null);
    setError(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const renderButtons = () => {
    if (variant === 'icons') {
      return (
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Tooltip title={`Export ${entityLabel}`}>
            <IconButton
              size="small"
              onClick={() => setExportDialogOpen(true)}
              color="primary"
            >
              <FileDownloadIcon />
            </IconButton>
          </Tooltip>
          <Tooltip title={`Import ${entityLabel}`}>
            <IconButton
              size="small"
              onClick={() => {
                resetImportDialog();
                setImportDialogOpen(true);
              }}
              color="primary"
            >
              <FileUploadIcon />
            </IconButton>
          </Tooltip>
        </Box>
      );
    }

    return (
      <Box sx={{ display: 'flex', gap: 1 }}>
        <Button
          variant="outlined"
          size="small"
          startIcon={<FileDownloadIcon />}
          onClick={() => setExportDialogOpen(true)}
        >
          Export
        </Button>
        <Button
          variant="outlined"
          size="small"
          startIcon={<FileUploadIcon />}
          onClick={() => {
            resetImportDialog();
            setImportDialogOpen(true);
          }}
        >
          Import
        </Button>
      </Box>
    );
  };

  return (
    <>
      {renderButtons()}

      {/* Export Dialog */}
      <Dialog
        open={exportDialogOpen}
        onClose={() => setExportDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h6">Export {entityLabel}</Typography>
            <IconButton onClick={() => setExportDialogOpen(false)} size="small">
              <CloseIcon />
            </IconButton>
          </Box>
        </DialogTitle>
        <DialogContent dividers>
          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
          
          <FormControl fullWidth sx={{ mb: 2 }}>
            <InputLabel>Export Format</InputLabel>
            <Select
              value={exportFormat}
              onChange={(e) => setExportFormat(e.target.value as 'json' | 'csv')}
              label="Export Format"
            >
              <MenuItem value="json">JSON</MenuItem>
              <MenuItem value="csv">CSV</MenuItem>
            </Select>
          </FormControl>

          <Typography variant="body2" color="text.secondary">
            This will export all {entityLabel.toLowerCase()} data in the selected format.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setExportDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleExport}
            disabled={loading}
            startIcon={loading ? <CircularProgress size={20} /> : <FileDownloadIcon />}
          >
            Export
          </Button>
        </DialogActions>
      </Dialog>

      {/* Import Dialog */}
      <Dialog
        open={importDialogOpen}
        onClose={() => setImportDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h6">Import {entityLabel}</Typography>
            <IconButton onClick={() => setImportDialogOpen(false)} size="small">
              <CloseIcon />
            </IconButton>
          </Box>
        </DialogTitle>
        <DialogContent dividers>
          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

          {!importResult && (
            <>
              <Box sx={{ mb: 3 }}>
                <Typography variant="subtitle2" gutterBottom>
                  Download Template
                </Typography>
                <Stack direction="row" spacing={1}>
                  <Button
                    variant="outlined"
                    size="small"
                    startIcon={<DescriptionIcon />}
                    onClick={() => {
                      setExportFormat('json');
                      handleDownloadTemplate();
                    }}
                  >
                    JSON Template
                  </Button>
                  <Button
                    variant="outlined"
                    size="small"
                    startIcon={<DescriptionIcon />}
                    onClick={() => {
                      setExportFormat('csv');
                      handleDownloadTemplate();
                    }}
                  >
                    CSV Template
                  </Button>
                </Stack>
              </Box>

              <Divider sx={{ my: 2 }} />

              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle2" gutterBottom>
                  Select File to Import
                </Typography>
                <input
                  type="file"
                  accept=".json,.csv"
                  onChange={handleFileSelect}
                  ref={fileInputRef}
                  style={{ marginBottom: 16 }}
                />
                
                {selectedFile && (
                  <Chip
                    label={selectedFile.name}
                    onDelete={() => {
                      setSelectedFile(null);
                      if (fileInputRef.current) {
                        fileInputRef.current.value = '';
                      }
                    }}
                    sx={{ mt: 1 }}
                  />
                )}
              </Box>

              <FormControlLabel
                control={
                  <Checkbox
                    checked={skipDuplicates}
                    onChange={(e) => setSkipDuplicates(e.target.checked)}
                  />
                }
                label="Skip duplicate records (based on email or unique identifier)"
              />
            </>
          )}

          {importResult && (
            <Box>
              <Alert
                severity={importResult.failed > 0 ? 'warning' : importResult.imported > 0 ? 'success' : 'info'}
                sx={{ mb: 2 }}
              >
                Import completed for {entityLabel}
              </Alert>

              <List dense>
                <ListItem>
                  <ListItemIcon>
                    <CheckCircleIcon color="success" />
                  </ListItemIcon>
                  <ListItemText
                    primary={`${importResult.imported} records imported successfully`}
                  />
                </ListItem>
                {importResult.skipped > 0 && (
                  <ListItem>
                    <ListItemIcon>
                      <WarningIcon color="warning" />
                    </ListItemIcon>
                    <ListItemText
                      primary={`${importResult.skipped} records skipped (duplicates)`}
                    />
                  </ListItem>
                )}
                {importResult.failed > 0 && (
                  <ListItem>
                    <ListItemIcon>
                      <ErrorIcon color="error" />
                    </ListItemIcon>
                    <ListItemText
                      primary={`${importResult.failed} records failed`}
                    />
                  </ListItem>
                )}
              </List>

              {importResult.errors.length > 0 && (
                <Box sx={{ mt: 2 }}>
                  <Typography variant="subtitle2" color="error" gutterBottom>
                    Errors:
                  </Typography>
                  <Box
                    sx={{
                      maxHeight: 150,
                      overflow: 'auto',
                      bgcolor: 'grey.100',
                      p: 1,
                      borderRadius: 1,
                      fontSize: '0.75rem',
                    }}
                  >
                    {importResult.errors.map((err, idx) => (
                      <Typography key={idx} variant="caption" display="block" color="error">
                        {err}
                      </Typography>
                    ))}
                  </Box>
                </Box>
              )}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          {importResult ? (
            <Button
              variant="contained"
              onClick={() => setImportDialogOpen(false)}
            >
              Done
            </Button>
          ) : (
            <>
              <Button onClick={() => setImportDialogOpen(false)}>Cancel</Button>
              <Button
                variant="contained"
                onClick={handleImport}
                disabled={loading || !selectedFile}
                startIcon={loading ? <CircularProgress size={20} /> : <FileUploadIcon />}
              >
                Import
              </Button>
            </>
          )}
        </DialogActions>
      </Dialog>
    </>
  );
};

export default ImportExportButtons;

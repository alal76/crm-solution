/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

import { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Typography,
  Button,
  Card,
  CardContent,
  CardActions,
  Grid,
  Chip,
  IconButton,
  Tooltip,
  Dialog,
  DialogContent,
  Alert,
  CircularProgress,
  TextField,
  InputAdornment,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  PlayArrow as RunIcon,
  Search as SearchIcon,
  Assessment as ReportIcon,
  Schedule as ScheduleIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import { ReportDesigner, ReportConfig } from '../components/analytics';
import apiClient from '../services/apiClient';

interface ReportSummary {
  id: number;
  name: string;
  description?: string;
  dataSource?: string;
  category?: string;
  lastRunAt?: string;
  createdAt: string;
  updatedAt?: string;
}

function ReportsPage() {
  const [reports, setReports] = useState<ReportSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [designerOpen, setDesignerOpen] = useState(false);
  const [editingReport, setEditingReport] = useState<ReportConfig | undefined>(undefined);

  const fetchReports = useCallback(async () => {
    setLoading(true);
    try {
      const response = await apiClient.get('/reports');
      setReports(response.data?.items || response.data || []);
      setError(null);
    } catch (err: any) {
      console.error('Failed to load reports:', err);
      setError('Failed to load reports. The reports module may not be configured yet.');
      setReports([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchReports();
  }, [fetchReports]);

  const handleCreateNew = () => {
    setEditingReport(undefined);
    setDesignerOpen(true);
  };

  const handleSave = async (report: ReportConfig) => {
    try {
      await apiClient.post('/reports', report);
      setDesignerOpen(false);
      setEditingReport(undefined);
      await fetchReports();
    } catch (err: any) {
      console.error('Failed to save report:', err);
    }
  };

  const handleRun = async (report: ReportConfig) => {
    try {
      await apiClient.post('/reports/execute', report);
    } catch (err: any) {
      console.error('Failed to run report:', err);
    }
  };

  const handleDelete = async (id: number) => {
    try {
      await apiClient.delete(`/reports/${id}`);
      await fetchReports();
    } catch (err: any) {
      console.error('Failed to delete report:', err);
    }
  };

  const filteredReports = reports.filter(
    (r) =>
      r.name?.toLowerCase().includes(searchQuery.toLowerCase()) ||
      r.description?.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <Box sx={{ py: 2 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
        <Box>
          <Typography variant="h3" sx={{ fontWeight: 700, mb: 0.5 }}>
            Reports
          </Typography>
          <Typography color="textSecondary" variant="body2">
            Create, manage, and run custom reports across your CRM data.
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Tooltip title="Refresh">
            <IconButton onClick={fetchReports} disabled={loading}>
              <RefreshIcon />
            </IconButton>
          </Tooltip>
          <Button variant="contained" startIcon={<AddIcon />} onClick={handleCreateNew}>
            Create Report
          </Button>
        </Box>
      </Box>

      {/* Search */}
      <TextField
        fullWidth
        placeholder="Search reports..."
        value={searchQuery}
        onChange={(e) => setSearchQuery(e.target.value)}
        sx={{ mb: 3 }}
        InputProps={{
          startAdornment: (
            <InputAdornment position="start">
              <SearchIcon />
            </InputAdornment>
          ),
        }}
      />

      {/* Error Alert */}
      {error && (
        <Alert severity="warning" sx={{ mb: 3, borderRadius: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Loading */}
      {loading && (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      )}

      {/* Reports Grid */}
      {!loading && filteredReports.length > 0 && (
        <Grid container spacing={3}>
          {filteredReports.map((report) => (
            <Grid item xs={12} sm={6} md={4} key={report.id}>
              <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
                <CardContent sx={{ flexGrow: 1 }}>
                  <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1, mb: 1 }}>
                    <ReportIcon color="primary" />
                    <Typography variant="h6" sx={{ fontWeight: 600, lineHeight: 1.3 }}>
                      {report.name}
                    </Typography>
                  </Box>
                  {report.description && (
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 1.5 }}>
                      {report.description}
                    </Typography>
                  )}
                  <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                    {report.dataSource && (
                      <Chip label={report.dataSource} size="small" variant="outlined" />
                    )}
                    {report.category && (
                      <Chip label={report.category} size="small" color="primary" variant="outlined" />
                    )}
                  </Box>
                  {report.lastRunAt && (
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 1.5 }}>
                      <ScheduleIcon sx={{ fontSize: 14, color: 'text.secondary' }} />
                      <Typography variant="caption" color="text.secondary">
                        Last run: {new Date(report.lastRunAt).toLocaleDateString()}
                      </Typography>
                    </Box>
                  )}
                </CardContent>
                <CardActions sx={{ justifyContent: 'flex-end', px: 2, pb: 1.5 }}>
                  <Tooltip title="Run Report">
                    <IconButton size="small" color="primary" onClick={() => handleRun({ name: report.name } as ReportConfig)}>
                      <RunIcon />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title="Edit">
                    <IconButton size="small" onClick={() => { setEditingReport({ name: report.name } as ReportConfig); setDesignerOpen(true); }}>
                      <EditIcon />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title="Delete">
                    <IconButton size="small" color="error" onClick={() => handleDelete(report.id)}>
                      <DeleteIcon />
                    </IconButton>
                  </Tooltip>
                </CardActions>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}

      {/* Empty State */}
      {!loading && filteredReports.length === 0 && !error && (
        <Box sx={{ textAlign: 'center', py: 8 }}>
          <ReportIcon sx={{ fontSize: 64, color: 'text.disabled', mb: 2 }} />
          <Typography variant="h6" color="text.secondary" gutterBottom>
            No reports yet
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
            Create your first custom report to analyze your CRM data.
          </Typography>
          <Button variant="contained" startIcon={<AddIcon />} onClick={handleCreateNew}>
            Create Report
          </Button>
        </Box>
      )}

      {/* Report Designer Dialog */}
      <Dialog
        open={designerOpen}
        onClose={() => setDesignerOpen(false)}
        maxWidth="lg"
        fullWidth
        PaperProps={{ sx: { minHeight: '80vh' } }}
      >
        <DialogContent sx={{ p: 0 }}>
          <ReportDesigner
            report={editingReport}
            onSave={handleSave}
            onRun={handleRun}
            onCancel={() => { setDesignerOpen(false); setEditingReport(undefined); }}
          />
        </DialogContent>
      </Dialog>
    </Box>
  );
}

export default ReportsPage;

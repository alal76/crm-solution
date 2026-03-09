/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This software is source-available. Non-commercial use is permitted under
 * the terms of the LICENSE file. Commercial use requires a separate license.
 * See the LICENSE file in the root directory for full terms.
 */

import { useEffect, useRef, useCallback, useState } from 'react';
import {
  Box,
  Typography,
  LinearProgress,
  Paper,
  Grid,
  Button,
  Alert,
  Chip,
  Stack,
  Divider,
  CircularProgress,
} from '@mui/material';
import {
  CheckCircle as SuccessIcon,
  Error as ErrorIcon,
  Warning as WarningIcon,
  Download as DownloadIcon,
} from '@mui/icons-material';
import importExportService from '../../services/importExportService';

// ============================================================================
// Types
// ============================================================================

export interface ImportResult {
  total: number;
  imported: number;
  skipped: number;
  errors: number;
  duration: number; // ms
}

export interface ImportProgressProps {
  jobId: string;
  onComplete?: (result: ImportResult) => void;
  onError?: (error: string) => void;
}

interface JobStatus {
  status: string;
  totalRecords: number;
  processedRecords: number;
  failedRecords: number;
  skippedRecords?: number;
  completedAt?: string;
  startedAt?: string;
  errorMessage?: string;
}

// ============================================================================
// Helpers
// ============================================================================

function calcPercent(processed: number, total: number): number {
  if (!total || total <= 0) return 0;
  return Math.min(100, Math.round((processed / total) * 100));
}

function formatDuration(ms: number): string {
  if (ms < 1000) return `${ms}ms`;
  if (ms < 60000) return `${(ms / 1000).toFixed(1)}s`;
  return `${Math.floor(ms / 60000)}m ${Math.round((ms % 60000) / 1000)}s`;
}

function getOutcome(status: JobStatus): 'success' | 'partial' | 'failed' | 'running' {
  if (!['Completed', 'Failed'].includes(status.status)) return 'running';
  if (status.status === 'Failed') return 'failed';
  if (status.failedRecords > 0) return 'partial';
  return 'success';
}

// ============================================================================
// Stat card sub-component
// ============================================================================

function StatCard({
  label,
  value,
  color,
}: {
  label: string;
  value: number;
  color?: string;
}) {
  return (
    <Paper variant="outlined" sx={{ p: 1.5, textAlign: 'center' }}>
      <Typography
        variant="h5"
        fontWeight={700}
        color={color ?? 'text.primary'}
        lineHeight={1.2}
      >
        {value.toLocaleString()}
      </Typography>
      <Typography variant="caption" color="text.secondary">
        {label}
      </Typography>
    </Paper>
  );
}

// ============================================================================
// Component
// ============================================================================

const POLL_INTERVAL_MS = 2000;

export default function ImportProgress({
  jobId,
  onComplete,
  onError,
}: ImportProgressProps) {
  const [jobStatus, setJobStatus] = useState<JobStatus | null>(null);
  const [fetchError, setFetchError] = useState<string | null>(null);
  const [downloadingErrors, setDownloadingErrors] = useState(false);

  // Track poll interval so we can clear it
  const pollRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const startedAtRef = useRef<number>(Date.now());

  const stopPolling = useCallback(() => {
    if (pollRef.current !== null) {
      clearInterval(pollRef.current);
      pollRef.current = null;
    }
  }, []);

  const handleStatus = useCallback(
    (status: JobStatus) => {
      setJobStatus(status);
      const isDone = status.status === 'Completed' || status.status === 'Failed';
      if (isDone) {
        stopPolling();
        if (status.status === 'Failed' && onError) {
          onError(status.errorMessage ?? 'Import failed');
        }
        if (status.status === 'Completed' && onComplete) {
          const durationMs = Date.now() - startedAtRef.current;
          onComplete({
            total: status.totalRecords,
            imported: status.processedRecords - status.failedRecords,
            skipped: status.skippedRecords ?? 0,
            errors: status.failedRecords,
            duration: durationMs,
          });
        }
      }
    },
    [onComplete, onError, stopPolling],
  );

  const fetchStatus = useCallback(async () => {
    try {
      // The existing service uses numeric IDs; if jobId is a string representation
      // of a number, cast it. Otherwise treat as string (future UUID support).
      const numericId = Number.parseInt(jobId, 10);
      const job = await importExportService.getImportJob(
        Number.isNaN(numericId) ? (jobId as unknown as number) : numericId,
      );
      // Map ImportJobDto → JobStatus
      const status: JobStatus = {
        status: job.status,
        totalRecords: job.totalRecords,
        processedRecords: job.processedRecords,
        failedRecords: job.failedRecords,
        completedAt: job.completedAt,
        errorMessage: undefined,
      };
      handleStatus(status);
      setFetchError(null);
    } catch (err: unknown) {
      const msg = err instanceof Error ? (err as Error).message : 'Failed to fetch job status';
      setFetchError(msg);
    }
  }, [jobId, handleStatus]);

  useEffect(() => {
    startedAtRef.current = Date.now();

    // Initial fetch
    fetchStatus();

    // Start polling (SignalR not yet wired for import hub; fallback to polling)
    pollRef.current = setInterval(fetchStatus, POLL_INTERVAL_MS);

    return () => {
      stopPolling();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [jobId]);

  // ---- Download error report ----
  const handleDownloadErrors = async () => {
    setDownloadingErrors(true);
    try {
      const numericId = Number.parseInt(jobId, 10);
      const id = Number.isNaN(numericId) ? (jobId as unknown as number) : numericId;
      const job = await importExportService.getImportJob(id);
      if (job.errors && job.errors.length > 0) {
        const csv = [
          'Row,Field,Message',
          ...job.errors.map(
            (e) =>
              `${e.rowNumber},${JSON.stringify(e.field)},${JSON.stringify(e.message)}`,
          ),
        ].join('\n');
        const blob = new Blob([csv], { type: 'text/csv' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `import_errors_${jobId}.csv`;
        link.click();
        URL.revokeObjectURL(url);
      }
    } catch (err) {
      // Silently ignore download errors
    } finally {
      setDownloadingErrors(false);
    }
  };

  // ---- Render states ----

  if (fetchError && !jobStatus) {
    return (
      <Alert severity="error">
        <Typography variant="body2">
          Unable to retrieve import status: {fetchError}
        </Typography>
      </Alert>
    );
  }

  if (!jobStatus) {
    return (
      <Box display="flex" alignItems="center" gap={2} py={4} justifyContent="center">
        <CircularProgress size={24} />
        <Typography variant="body2" color="text.secondary">
          Connecting to import job…
        </Typography>
      </Box>
    );
  }

  const percent = calcPercent(jobStatus.processedRecords, jobStatus.totalRecords);
  const outcome = getOutcome(jobStatus);
  const isDone = outcome !== 'running';
  const imported =
    jobStatus.processedRecords - jobStatus.failedRecords;
  const skipped = jobStatus.skippedRecords ?? 0;

  return (
    <Box>
      {/* Status chip */}
      <Stack direction="row" alignItems="center" spacing={1} mb={2}>
        {outcome === 'running' && (
          <CircularProgress size={16} thickness={5} />
        )}
        {outcome === 'success' && <SuccessIcon color="success" />}
        {outcome === 'partial' && <WarningIcon color="warning" />}
        {outcome === 'failed' && <ErrorIcon color="error" />}
        <Chip
          label={jobStatus.status}
          size="small"
          color={
            outcome === 'success'
              ? 'success'
              : outcome === 'partial'
              ? 'warning'
              : outcome === 'failed'
              ? 'error'
              : 'default'
          }
          variant="outlined"
        />
        <Typography variant="body2" color="text.secondary">
          {outcome === 'running' ? 'Processing…' : isDone ? 'Finished' : ''}
        </Typography>
      </Stack>

      {/* Progress bar */}
      <Box mb={1}>
        <Stack direction="row" justifyContent="space-between" mb={0.5}>
          <Typography variant="body2" color="text.secondary">
            Progress
          </Typography>
          <Typography variant="body2" fontWeight={600}>
            {percent}%
          </Typography>
        </Stack>
        <LinearProgress
          variant={
            jobStatus.totalRecords > 0 ? 'determinate' : 'indeterminate'
          }
          value={percent}
          color={
            outcome === 'success'
              ? 'success'
              : outcome === 'partial'
              ? 'warning'
              : outcome === 'failed'
              ? 'error'
              : 'primary'
          }
          sx={{ height: 10, borderRadius: 5 }}
        />
      </Box>

      {/* Stats grid */}
      <Grid container spacing={1.5} sx={{ mt: 1, mb: 2 }}>
        <Grid item xs={6} sm={3}>
          <StatCard label="Total Records" value={jobStatus.totalRecords} />
        </Grid>
        <Grid item xs={6} sm={3}>
          <StatCard
            label="Processed"
            value={jobStatus.processedRecords}
            color="text.primary"
          />
        </Grid>
        <Grid item xs={6} sm={3}>
          <StatCard
            label="Imported"
            value={imported >= 0 ? imported : 0}
            color="success.main"
          />
        </Grid>
        <Grid item xs={6} sm={3}>
          <StatCard label="Skipped" value={skipped} color="text.secondary" />
        </Grid>
        <Grid item xs={6} sm={3}>
          <StatCard
            label="Errors"
            value={jobStatus.failedRecords}
            color={jobStatus.failedRecords > 0 ? 'error.main' : 'text.secondary'}
          />
        </Grid>
      </Grid>

      {/* Completion summary */}
      {isDone && (
        <>
          <Divider sx={{ my: 2 }} />
          <Alert
            severity={
              outcome === 'success'
                ? 'success'
                : outcome === 'partial'
                ? 'warning'
                : 'error'
            }
            icon={
              outcome === 'success' ? (
                <SuccessIcon />
              ) : outcome === 'partial' ? (
                <WarningIcon />
              ) : (
                <ErrorIcon />
              )
            }
          >
            <Typography variant="body2">
              {outcome === 'success' &&
                `All ${jobStatus.totalRecords.toLocaleString()} records imported successfully.`}
              {outcome === 'partial' &&
                `Import completed with issues: ${imported.toLocaleString()} imported, 
                 ${jobStatus.failedRecords.toLocaleString()} failed.`}
              {outcome === 'failed' &&
                `Import failed: ${jobStatus.errorMessage ?? 'Unknown error'}`}
            </Typography>
            {jobStatus.completedAt && (
              <Typography variant="caption" color="text.secondary" display="block" mt={0.5}>
                Completed at{' '}
                {new Date(jobStatus.completedAt).toLocaleString()}
                {' — '}
                Duration:{' '}
                {formatDuration(Date.now() - startedAtRef.current)}
              </Typography>
            )}
          </Alert>

          {jobStatus.failedRecords > 0 && (
            <Box mt={2}>
              <Button
                variant="outlined"
                color="error"
                size="small"
                startIcon={
                  downloadingErrors ? (
                    <CircularProgress size={14} />
                  ) : (
                    <DownloadIcon />
                  )
                }
                onClick={handleDownloadErrors}
                disabled={downloadingErrors}
              >
                Download Error Report
              </Button>
            </Box>
          )}
        </>
      )}

      {/* Live polling error note */}
      {fetchError && (
        <Alert severity="warning" sx={{ mt: 2 }}>
          <Typography variant="caption">
            Status update failed: {fetchError} — retrying…
          </Typography>
        </Alert>
      )}
    </Box>
  );
}

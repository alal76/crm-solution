/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Audit Log Viewer - View workflow change history and events
 * Event type configuration is loaded from backend API
 */

import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Typography,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TablePagination,
  Chip,
  IconButton,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Collapse,
  Paper,
  CircularProgress,
  Tooltip,
} from '@mui/material';
import {
  History as HistoryIcon,
  Close as CloseIcon,
  ExpandMore as ExpandIcon,
  ExpandLess as CollapseIcon,
  FilterList as FilterIcon,
  Download as DownloadIcon,
  Person as PersonIcon,
  Schedule as TimeIcon,
  Code as CodeIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import { workflowService, EventTypeOption, WorkflowConfig } from '../../services/workflowService';

// ============================================================================
// Types
// ============================================================================

export interface AuditLogEntry {
  id: number;
  timestamp: string;
  eventType: string;
  entityType: string;
  entityId: number;
  actorType: 'User' | 'System' | 'Worker';
  actorId?: number;
  actorName?: string;
  description: string;
  previousValue?: string;
  newValue?: string;
  ipAddress?: string;
  userAgent?: string;
  correlationId?: string;
  metadata?: Record<string, any>;
}

interface AuditLogViewerProps {
  open: boolean;
  onClose: () => void;
  workflowId?: number;
  instanceId?: number;
  loadAuditLogs: (params: {
    workflowId?: number;
    instanceId?: number;
    eventType?: string;
    startDate?: string;
    endDate?: string;
    page: number;
    pageSize: number;
  }) => Promise<{ items: AuditLogEntry[]; totalCount: number }>;
  title?: string;
}

// ============================================================================
// Event Type Configuration - Default fallback values (overridden by backend)
// ============================================================================

const defaultEventTypeConfig: Record<string, { color: 'default' | 'primary' | 'secondary' | 'success' | 'error' | 'warning' | 'info'; label: string }> = {
  WorkflowCreated: { color: 'success', label: 'Created' },
  WorkflowUpdated: { color: 'info', label: 'Updated' },
  WorkflowDeleted: { color: 'error', label: 'Deleted' },
  WorkflowActivated: { color: 'success', label: 'Activated' },
  WorkflowPaused: { color: 'warning', label: 'Paused' },
  WorkflowArchived: { color: 'default', label: 'Archived' },
  VersionCreated: { color: 'info', label: 'Version Created' },
  VersionPublished: { color: 'success', label: 'Version Published' },
  NodeAdded: { color: 'primary', label: 'Node Added' },
  NodeUpdated: { color: 'info', label: 'Node Updated' },
  NodeDeleted: { color: 'error', label: 'Node Deleted' },
  TransitionAdded: { color: 'primary', label: 'Transition Added' },
  TransitionUpdated: { color: 'info', label: 'Transition Updated' },
  TransitionDeleted: { color: 'error', label: 'Transition Deleted' },
  InstanceStarted: { color: 'success', label: 'Instance Started' },
  InstanceCompleted: { color: 'success', label: 'Instance Completed' },
  InstanceFailed: { color: 'error', label: 'Instance Failed' },
  InstanceCancelled: { color: 'warning', label: 'Instance Cancelled' },
  InstancePaused: { color: 'warning', label: 'Instance Paused' },
  InstanceResumed: { color: 'info', label: 'Instance Resumed' },
  TaskStarted: { color: 'primary', label: 'Task Started' },
  TaskCompleted: { color: 'success', label: 'Task Completed' },
  TaskFailed: { color: 'error', label: 'Task Failed' },
  TaskRetried: { color: 'warning', label: 'Task Retried' },
  TaskSkipped: { color: 'default', label: 'Task Skipped' },
  HumanTaskAssigned: { color: 'info', label: 'Task Assigned' },
  HumanTaskClaimed: { color: 'primary', label: 'Task Claimed' },
  HumanTaskApproved: { color: 'success', label: 'Task Approved' },
  HumanTaskRejected: { color: 'error', label: 'Task Rejected' },
};

// Helper to get event config from backend or fallback
const getEventConfig = (
  eventType: string, 
  backendConfig?: EventTypeOption[]
): { color: 'default' | 'primary' | 'secondary' | 'success' | 'error' | 'warning' | 'info'; label: string } => {
  if (backendConfig) {
    const found = backendConfig.find(e => e.value === eventType);
    if (found) {
      // Map backend color strings to MUI chip colors
      const colorMap: Record<string, 'default' | 'primary' | 'secondary' | 'success' | 'error' | 'warning' | 'info'> = {
        success: 'success',
        error: 'error',
        warning: 'warning',
        info: 'info',
        primary: 'primary',
        secondary: 'secondary',
        default: 'default',
      };
      return { color: colorMap[found.color] || 'default', label: found.label };
    }
  }
  return defaultEventTypeConfig[eventType] || { color: 'default', label: eventType };
};

// ============================================================================
// Helper Components
// ============================================================================

const ValueDiff: React.FC<{ previous?: string; current?: string }> = ({ previous, current }) => {
  const [expanded, setExpanded] = useState(false);

  const parseValue = (val?: string) => {
    if (!val) return null;
    try {
      return JSON.parse(val);
    } catch {
      return val;
    }
  };

  const prevParsed = parseValue(previous);
  const currParsed = parseValue(current);

  if (!previous && !current) return <Typography variant="caption" color="text.secondary">No data</Typography>;

  return (
    <Box>
      <Button
        size="small"
        startIcon={expanded ? <CollapseIcon /> : <ExpandIcon />}
        onClick={() => setExpanded(!expanded)}
        sx={{ py: 0 }}
      >
        {expanded ? 'Hide' : 'Show'} Details
      </Button>
      <Collapse in={expanded}>
        <Box sx={{ display: 'flex', gap: 2, mt: 1 }}>
          {previous && (
            <Paper variant="outlined" sx={{ p: 1, flex: 1, backgroundColor: 'error.lighter' }}>
              <Typography variant="caption" color="error.main" fontWeight="medium">Previous</Typography>
              <pre style={{ fontSize: 11, margin: 0, overflow: 'auto', maxHeight: 150 }}>
                {typeof prevParsed === 'object' ? JSON.stringify(prevParsed, null, 2) : previous}
              </pre>
            </Paper>
          )}
          {current && (
            <Paper variant="outlined" sx={{ p: 1, flex: 1, backgroundColor: 'success.lighter' }}>
              <Typography variant="caption" color="success.main" fontWeight="medium">New</Typography>
              <pre style={{ fontSize: 11, margin: 0, overflow: 'auto', maxHeight: 150 }}>
                {typeof currParsed === 'object' ? JSON.stringify(currParsed, null, 2) : current}
              </pre>
            </Paper>
          )}
        </Box>
      </Collapse>
    </Box>
  );
};

// ============================================================================
// Main Component
// ============================================================================

export const AuditLogViewer: React.FC<AuditLogViewerProps> = ({
  open,
  onClose,
  workflowId,
  instanceId,
  loadAuditLogs,
  title = 'Audit Log',
}) => {
  const [logs, setLogs] = useState<AuditLogEntry[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(25);
  
  // Backend configuration
  const [eventTypes, setEventTypes] = useState<EventTypeOption[]>([]);
  
  // Filters
  const [showFilters, setShowFilters] = useState(false);
  const [eventTypeFilter, setEventTypeFilter] = useState<string>('');
  const [startDate, setStartDate] = useState<string>('');
  const [endDate, setEndDate] = useState<string>('');

  // Expanded rows
  const [expandedRows, setExpandedRows] = useState<Set<number>>(new Set());

  // Load event types from backend
  useEffect(() => {
    workflowService.getEventTypes().then(setEventTypes).catch(() => {});
  }, []);

  const fetchLogs = useCallback(async () => {
    setLoading(true);
    try {
      const result = await loadAuditLogs({
        workflowId,
        instanceId,
        eventType: eventTypeFilter || undefined,
        startDate: startDate || undefined,
        endDate: endDate || undefined,
        page: page + 1,
        pageSize,
      });
      setLogs(result.items);
      setTotalCount(result.totalCount);
    } catch (error) {
      console.error('Failed to load audit logs:', error);
    } finally {
      setLoading(false);
    }
  }, [workflowId, instanceId, eventTypeFilter, startDate, endDate, page, pageSize, loadAuditLogs]);

  useEffect(() => {
    if (open) {
      fetchLogs();
    }
  }, [open, fetchLogs]);

  const toggleRow = (id: number) => {
    setExpandedRows(prev => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  };

  const handleExport = () => {
    const csv = [
      ['Timestamp', 'Event Type', 'Actor', 'Description', 'IP Address'].join(','),
      ...logs.map(log => [
        log.timestamp,
        log.eventType,
        log.actorName || log.actorType,
        `"${log.description.replace(/"/g, '""')}"`,
        log.ipAddress || '',
      ].join(',')),
    ].join('\n');

    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `audit-log-${workflowId || instanceId || 'all'}-${new Date().toISOString().slice(0, 10)}.csv`;
    a.click();
  };

  const getEventTypeChip = (eventType: string) => {
    const config = getEventConfig(eventType, eventTypes);
    return (
      <Chip 
        label={config.label} 
        color={config.color} 
        size="small" 
        sx={{ fontSize: 11, height: 22 }} 
      />
    );
  };

  // Get event type options for the dropdown - prefer backend, fallback to defaults
  const eventTypeOptions = eventTypes.length > 0 
    ? eventTypes 
    : Object.entries(defaultEventTypeConfig).map(([key, value]) => ({ 
        value: key, 
        label: value.label, 
        color: value.color,
        category: 'General' 
      }));

  return (
    <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth>
      <DialogTitle>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <HistoryIcon />
          <Typography variant="h6">{title}</Typography>
          <Box sx={{ flex: 1 }} />
          <Tooltip title="Refresh">
            <IconButton onClick={fetchLogs} disabled={loading}>
              <RefreshIcon className={loading ? 'spin' : ''} />
            </IconButton>
          </Tooltip>
          <Tooltip title="Toggle Filters">
            <IconButton onClick={() => setShowFilters(!showFilters)}>
              <FilterIcon color={showFilters ? 'primary' : 'inherit'} />
            </IconButton>
          </Tooltip>
          <Tooltip title="Export CSV">
            <IconButton onClick={handleExport}>
              <DownloadIcon />
            </IconButton>
          </Tooltip>
          <IconButton onClick={onClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent dividers>
        {/* Filters */}
        <Collapse in={showFilters}>
          <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
            <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
              <FormControl size="small" sx={{ minWidth: 180 }}>
                <InputLabel>Event Type</InputLabel>
                <Select
                  value={eventTypeFilter}
                  label="Event Type"
                  onChange={(e) => setEventTypeFilter(e.target.value)}
                >
                  <MenuItem value="">All Events</MenuItem>
                  {eventTypeOptions.map((event) => (
                    <MenuItem key={event.value} value={event.value}>{event.label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
              <TextField
                size="small"
                type="date"
                label="Start Date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                InputLabelProps={{ shrink: true }}
              />
              <TextField
                size="small"
                type="date"
                label="End Date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                InputLabelProps={{ shrink: true }}
              />
              <Button
                size="small"
                onClick={() => {
                  setEventTypeFilter('');
                  setStartDate('');
                  setEndDate('');
                }}
              >
                Clear Filters
              </Button>
            </Box>
          </Paper>
        </Collapse>

        {/* Table */}
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell sx={{ width: 40 }} />
                <TableCell sx={{ width: 160 }}>Timestamp</TableCell>
                <TableCell sx={{ width: 140 }}>Event</TableCell>
                <TableCell sx={{ width: 140 }}>Actor</TableCell>
                <TableCell>Description</TableCell>
                <TableCell sx={{ width: 100 }}>IP Address</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {logs.map((log) => (
                <React.Fragment key={log.id}>
                  <TableRow 
                    hover 
                    onClick={() => toggleRow(log.id)}
                    sx={{ cursor: 'pointer' }}
                  >
                    <TableCell>
                      <IconButton size="small">
                        {expandedRows.has(log.id) ? <CollapseIcon fontSize="small" /> : <ExpandIcon fontSize="small" />}
                      </IconButton>
                    </TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                        <TimeIcon fontSize="small" color="action" />
                        <Typography variant="caption">
                          {new Date(log.timestamp).toLocaleString()}
                        </Typography>
                      </Box>
                    </TableCell>
                    <TableCell>{getEventTypeChip(log.eventType)}</TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                        <PersonIcon fontSize="small" color="action" />
                        <Typography variant="body2">
                          {log.actorName || log.actorType}
                        </Typography>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">{log.description}</Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="caption" color="text.secondary">
                        {log.ipAddress || '-'}
                      </Typography>
                    </TableCell>
                  </TableRow>
                  <TableRow>
                    <TableCell colSpan={6} sx={{ py: 0, borderBottom: expandedRows.has(log.id) ? undefined : 'none' }}>
                      <Collapse in={expandedRows.has(log.id)}>
                        <Box sx={{ p: 2, backgroundColor: 'grey.50' }}>
                          <Box sx={{ display: 'flex', gap: 4, mb: 2 }}>
                            <Box>
                              <Typography variant="caption" color="text.secondary">Entity</Typography>
                              <Typography variant="body2">
                                {log.entityType} #{log.entityId}
                              </Typography>
                            </Box>
                            {log.correlationId && (
                              <Box>
                                <Typography variant="caption" color="text.secondary">Correlation ID</Typography>
                                <Typography variant="body2" fontFamily="monospace" fontSize={11}>
                                  {log.correlationId}
                                </Typography>
                              </Box>
                            )}
                            {log.userAgent && (
                              <Box>
                                <Typography variant="caption" color="text.secondary">User Agent</Typography>
                                <Typography variant="body2" fontSize={11} noWrap sx={{ maxWidth: 300 }}>
                                  {log.userAgent}
                                </Typography>
                              </Box>
                            )}
                          </Box>
                          
                          {(log.previousValue || log.newValue) && (
                            <ValueDiff previous={log.previousValue} current={log.newValue} />
                          )}

                          {log.metadata && Object.keys(log.metadata).length > 0 && (
                            <Box sx={{ mt: 2 }}>
                              <Typography variant="caption" color="text.secondary">Additional Metadata</Typography>
                              <Paper variant="outlined" sx={{ p: 1, mt: 0.5 }}>
                                <pre style={{ fontSize: 11, margin: 0 }}>
                                  {JSON.stringify(log.metadata, null, 2)}
                                </pre>
                              </Paper>
                            </Box>
                          )}
                        </Box>
                      </Collapse>
                    </TableCell>
                  </TableRow>
                </React.Fragment>
              ))}
              {logs.length === 0 && (
                <TableRow>
                  <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                    <Typography color="text.secondary">No audit log entries found</Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        )}

        <TablePagination
          component="div"
          count={totalCount}
          page={page}
          onPageChange={(_, newPage) => setPage(newPage)}
          rowsPerPage={pageSize}
          onRowsPerPageChange={(e) => {
            setPageSize(parseInt(e.target.value));
            setPage(0);
          }}
          rowsPerPageOptions={[10, 25, 50, 100]}
        />
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
};

export default AuditLogViewer;

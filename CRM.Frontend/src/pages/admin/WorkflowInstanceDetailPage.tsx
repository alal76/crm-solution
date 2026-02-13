/**
 * CRM Solution - Workflow Instance Detail Page
 */

import React, { useEffect, useMemo, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Paper,
  Button,
  CircularProgress,
  Alert,
  Divider,
  Chip,
  Stack,
  List,
  ListItem,
  ListItemText,
} from '@mui/material';
import { ArrowBack as BackIcon } from '@mui/icons-material';
import { workflowInstanceService, type WorkflowInstanceDetail, type WorkflowLog } from '../../services/workflowService';
import { InstanceTimeline, type TimelineStep } from '../../components/workflow';

const toTimelineStatus = (status?: string): TimelineStep['status'] => {
  switch ((status || '').toLowerCase()) {
    case 'running':
      return 'running';
    case 'waiting':
      return 'waiting';
    case 'completed':
      return 'completed';
    case 'failed':
      return 'failed';
    case 'skipped':
      return 'skipped';
    default:
      return 'pending';
  }
};

const WorkflowInstanceDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [instance, setInstance] = useState<WorkflowInstanceDetail | null>(null);
  const [logs, setLogs] = useState<WorkflowLog[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const load = async () => {
      if (!id) return;
      try {
        setLoading(true);
        const [detail, logEntries] = await Promise.all([
          workflowInstanceService.getInstance(parseInt(id)),
          workflowInstanceService.getInstanceLogs(parseInt(id))
        ]);
        setInstance(detail);
        setLogs(logEntries);
        setError('');
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to load workflow instance');
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [id]);

  const timelineSteps: TimelineStep[] = useMemo(() => {
    if (!instance?.nodeInstances) return [];
    return instance.nodeInstances.map(ni => ({
      id: ni.id,
      nodeKey: ni.nodeId.toString(),
      nodeName: ni.nodeName,
      nodeType: instance.nodes.find(n => n.id === ni.nodeId)?.nodeType || 'Unknown',
      status: toTimelineStatus(ni.status),
      startedAt: ni.startedAt,
      completedAt: ni.completedAt,
      durationMs: ni.durationMs,
      errorMessage: ni.errorMessage,
      executionSequence: ni.executionSequence,
    }));
  }, [instance]);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="error">{error}</Alert>
      </Box>
    );
  }

  if (!instance) return null;

  return (
    <Box sx={{ p: 3, display: 'grid', gap: 3 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
        <Button startIcon={<BackIcon />} onClick={() => navigate('/admin/workflows/instances')}>
          Back to Instances
        </Button>
        <Typography variant="h5" fontWeight="bold">
          {instance.workflowName}
        </Typography>
        <Chip label={instance.status} color={instance.status === 'Completed' ? 'success' : 'default'} />
      </Box>

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle1" fontWeight="medium" gutterBottom>Instance Summary</Typography>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <Box>
            <Typography variant="caption" color="text.secondary">Entity</Typography>
            <Typography variant="body2">{instance.entityType} #{instance.entityId}</Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">Started</Typography>
            <Typography variant="body2">{instance.startedAt ? new Date(instance.startedAt).toLocaleString() : '—'}</Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">Completed</Typography>
            <Typography variant="body2">{instance.completedAt ? new Date(instance.completedAt).toLocaleString() : '—'}</Typography>
          </Box>
        </Stack>
      </Paper>

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle1" fontWeight="medium" gutterBottom>Execution Timeline</Typography>
        <InstanceTimeline
          steps={timelineSteps}
          workflowStartedAt={instance.startedAt}
          workflowCompletedAt={instance.completedAt}
          showDurations
        />
      </Paper>

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle1" fontWeight="medium" gutterBottom>Pending Tasks</Typography>
        <List dense>
          {instance.tasks.map(task => (
            <ListItem key={task.id} divider>
              <ListItemText
                primary={task.name}
                secondary={`Node: ${task.nodeName} • Status: ${task.status}`}
              />
            </ListItem>
          ))}
          {!instance.tasks.length && (
            <ListItem>
              <ListItemText primary="No pending tasks." />
            </ListItem>
          )}
        </List>
      </Paper>

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle1" fontWeight="medium" gutterBottom>Recent Logs</Typography>
        <Divider sx={{ mb: 2 }} />
        <List dense>
          {logs.map(log => (
            <ListItem key={log.id} divider>
              <ListItemText
                primary={`${log.level} • ${log.category}`}
                secondary={`${log.message} — ${new Date(log.timestamp).toLocaleString()}`}
              />
            </ListItem>
          ))}
          {!logs.length && (
            <ListItem>
              <ListItemText primary="No logs available." />
            </ListItem>
          )}
        </List>
      </Paper>
    </Box>
  );
};

export default WorkflowInstanceDetailPage;

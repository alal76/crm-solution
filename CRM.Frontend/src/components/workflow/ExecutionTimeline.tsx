/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Execution Timeline - Gantt-style visualization of workflow execution
 * Status/node colors can be configured via backend API (see workflowService.statusColors)
 */

import React, { useMemo, useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  Tooltip,
  Chip,
} from '@mui/material';
import {
  CheckCircle as CompletedIcon,
  Error as FailedIcon,
  HourglassEmpty as WaitingIcon,
  PlayCircle as RunningIcon,
  Schedule as PendingIcon,
  SkipNext as SkippedIcon,
} from '@mui/icons-material';
import { workflowService, nodeTypeInfo, StatusOption } from '../../services/workflowService';

// ============================================================================
// Types
// ============================================================================

export interface TimelineStep {
  id: number;
  nodeKey: string;
  nodeName: string;
  nodeType: string;
  status: 'pending' | 'running' | 'waiting' | 'completed' | 'failed' | 'skipped';
  startedAt?: string;
  completedAt?: string;
  durationMs?: number;
  errorMessage?: string;
  executionSequence: number;
}

interface ExecutionTimelineProps {
  steps: TimelineStep[];
  workflowStartedAt?: string;
  workflowCompletedAt?: string;
  showDurations?: boolean;
}

// ============================================================================
// Constants - Default values (can be overridden by backend config)
// ============================================================================

const defaultStatusConfig: Record<string, {
  color: string;
  bgColor: string;
  icon: React.ReactNode;
  label: string;
}> = {
  pending: { color: '#2196F3', bgColor: '#E3F2FD', icon: <PendingIcon fontSize="small" />, label: 'Pending' },
  running: { color: '#4CAF50', bgColor: '#E8F5E9', icon: <RunningIcon fontSize="small" />, label: 'Running' },
  waiting: { color: '#FF9800', bgColor: '#FFF3E0', icon: <WaitingIcon fontSize="small" />, label: 'Waiting' },
  completed: { color: '#4CAF50', bgColor: '#E8F5E9', icon: <CompletedIcon fontSize="small" />, label: 'Completed' },
  failed: { color: '#F44336', bgColor: '#FFEBEE', icon: <FailedIcon fontSize="small" />, label: 'Failed' },
  skipped: { color: '#9E9E9E', bgColor: '#F5F5F5', icon: <SkippedIcon fontSize="small" />, label: 'Skipped' },
};

// Use nodeTypeInfo from service (which loads from backend)
const getNodeTypeColor = (nodeType: string): string => {
  const info = nodeTypeInfo[nodeType];
  return info?.color || '#6750A4';
};

// ============================================================================
// Helper Functions
// ============================================================================

const formatDuration = (ms: number): string => {
  if (ms < 1000) return `${ms}ms`;
  if (ms < 60000) return `${(ms / 1000).toFixed(1)}s`;
  if (ms < 3600000) return `${(ms / 60000).toFixed(1)}m`;
  return `${(ms / 3600000).toFixed(1)}h`;
};

const parseDate = (dateStr: string): Date => new Date(dateStr);

// ============================================================================
// Timeline Bar Component
// ============================================================================

interface TimelineBarProps {
  step: TimelineStep;
  timelineStart: Date;
  timelineDuration: number;
  showDuration: boolean;
}

const TimelineBar: React.FC<TimelineBarProps> = ({
  step,
  timelineStart,
  timelineDuration,
  showDuration,
}) => {
  const status = defaultStatusConfig[step.status] || defaultStatusConfig.pending;
  const nodeColor = getNodeTypeColor(step.nodeType);

  const barStyle = useMemo(() => {
    if (!step.startedAt || timelineDuration === 0) {
      return { left: '0%', width: '100%' };
    }

    const startMs = parseDate(step.startedAt).getTime() - timelineStart.getTime();
    const endMs = step.completedAt
      ? parseDate(step.completedAt).getTime() - timelineStart.getTime()
      : step.durationMs
      ? startMs + step.durationMs
      : startMs + 1000; // Default 1s for running steps

    const left = Math.max(0, (startMs / timelineDuration) * 100);
    const width = Math.max(2, ((endMs - startMs) / timelineDuration) * 100);

    return { left: `${left}%`, width: `${Math.min(100 - left, width)}%` };
  }, [step, timelineStart, timelineDuration]);

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', py: 0.75, minHeight: 40 }}>
      {/* Step Info */}
      <Box sx={{ width: 200, flexShrink: 0, pr: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Box
            sx={{
              width: 8,
              height: 8,
              borderRadius: '50%',
              backgroundColor: nodeColor,
            }}
          />
          <Typography variant="body2" noWrap title={step.nodeName}>
            {step.nodeName}
          </Typography>
        </Box>
        <Typography variant="caption" color="text.secondary">
          {step.nodeType}
        </Typography>
      </Box>

      {/* Timeline Bar */}
      <Box sx={{ flex: 1, position: 'relative', height: 24 }}>
        <Box
          sx={{
            position: 'absolute',
            top: 0,
            bottom: 0,
            backgroundColor: 'grey.100',
            borderRadius: 1,
            width: '100%',
          }}
        />
        <Tooltip
          title={
            <Box>
              <Typography variant="body2" fontWeight="medium">{step.nodeName}</Typography>
              <Typography variant="caption">Status: {status.label}</Typography>
              {step.startedAt && (
                <Typography variant="caption" display="block">
                  Started: {new Date(step.startedAt).toLocaleTimeString()}
                </Typography>
              )}
              {step.completedAt && (
                <Typography variant="caption" display="block">
                  Completed: {new Date(step.completedAt).toLocaleTimeString()}
                </Typography>
              )}
              {step.durationMs !== undefined && (
                <Typography variant="caption" display="block">
                  Duration: {formatDuration(step.durationMs)}
                </Typography>
              )}
              {step.errorMessage && (
                <Typography variant="caption" display="block" color="error.main">
                  Error: {step.errorMessage}
                </Typography>
              )}
            </Box>
          }
          arrow
        >
          <Box
            sx={{
              position: 'absolute',
              top: 2,
              bottom: 2,
              ...barStyle,
              backgroundColor: status.bgColor,
              border: `2px solid ${status.color}`,
              borderRadius: 1,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              overflow: 'hidden',
              cursor: 'pointer',
              transition: 'transform 0.1s',
              '&:hover': {
                transform: 'scaleY(1.1)',
                zIndex: 1,
              },
            }}
          >
            {showDuration && step.durationMs !== undefined && (
              <Typography
                variant="caption"
                sx={{
                  color: status.color,
                  fontWeight: 'medium',
                  whiteSpace: 'nowrap',
                  px: 0.5,
                }}
              >
                {formatDuration(step.durationMs)}
              </Typography>
            )}
          </Box>
        </Tooltip>
      </Box>

      {/* Status */}
      <Box sx={{ width: 100, flexShrink: 0, pl: 2, display: 'flex', justifyContent: 'flex-end' }}>
        <Chip
          icon={status.icon as React.ReactElement}
          label={status.label}
          size="small"
          sx={{
            backgroundColor: status.bgColor,
            color: status.color,
            '& .MuiChip-icon': { color: status.color },
            fontSize: 11,
            height: 22,
          }}
        />
      </Box>
    </Box>
  );
};

// ============================================================================
// Time Scale Component
// ============================================================================

interface TimeScaleProps {
  startTime: Date;
  duration: number;
}

const TimeScale: React.FC<TimeScaleProps> = ({ startTime, duration }) => {
  const intervals = useMemo(() => {
    const count = 5;
    const intervalMs = duration / count;
    return Array.from({ length: count + 1 }, (_, i) => ({
      position: (i / count) * 100,
      time: new Date(startTime.getTime() + i * intervalMs),
    }));
  }, [startTime, duration]);

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', py: 0.5, borderBottom: 1, borderColor: 'divider', mb: 1 }}>
      <Box sx={{ width: 200, flexShrink: 0 }} />
      <Box sx={{ flex: 1, position: 'relative', height: 20 }}>
        {intervals.map((interval, i) => (
          <Box
            key={i}
            sx={{
              position: 'absolute',
              left: `${interval.position}%`,
              transform: 'translateX(-50%)',
            }}
          >
            <Typography variant="caption" color="text.secondary">
              {interval.time.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' })}
            </Typography>
          </Box>
        ))}
      </Box>
      <Box sx={{ width: 100, flexShrink: 0 }} />
    </Box>
  );
};

// ============================================================================
// Main Component
// ============================================================================

export const ExecutionTimeline: React.FC<ExecutionTimelineProps> = ({
  steps,
  workflowStartedAt,
  workflowCompletedAt,
  showDurations = true,
}) => {
  // Sort steps by execution sequence
  const sortedSteps = useMemo(() => 
    [...steps].sort((a, b) => a.executionSequence - b.executionSequence),
    [steps]
  );

  // Calculate timeline bounds
  const { timelineStart, timelineDuration } = useMemo(() => {
    const startTime = workflowStartedAt
      ? parseDate(workflowStartedAt)
      : steps.length > 0 && steps[0].startedAt
      ? parseDate(steps[0].startedAt)
      : new Date();

    const completedSteps = steps.filter(s => s.completedAt);
    const endTime = workflowCompletedAt
      ? parseDate(workflowCompletedAt)
      : completedSteps.length > 0
      ? new Date(Math.max(...completedSteps.map(s => parseDate(s.completedAt!).getTime())))
      : new Date();

    const duration = Math.max(endTime.getTime() - startTime.getTime(), 1000);

    return { timelineStart: startTime, timelineDuration: duration };
  }, [steps, workflowStartedAt, workflowCompletedAt]);

  // Summary stats
  const summary = useMemo(() => {
    const completed = steps.filter(s => s.status === 'completed');
    const totalDuration = steps.reduce((sum, s) => sum + (s.durationMs || 0), 0);
    const avgDuration = completed.length > 0 ? totalDuration / completed.length : 0;

    return {
      total: steps.length,
      completed: completed.length,
      failed: steps.filter(s => s.status === 'failed').length,
      pending: steps.filter(s => ['pending', 'running', 'waiting'].includes(s.status)).length,
      totalDuration,
      avgDuration,
    };
  }, [steps]);

  if (steps.length === 0) {
    return (
      <Paper variant="outlined" sx={{ p: 3, textAlign: 'center' }}>
        <Typography color="text.secondary">No execution data available</Typography>
      </Paper>
    );
  }

  return (
    <Box>
      {/* Summary */}
      <Box sx={{ display: 'flex', gap: 2, mb: 2, flexWrap: 'wrap' }}>
        <Chip label={`${summary.total} steps`} size="small" variant="outlined" />
        <Chip 
          label={`${summary.completed} completed`} 
          size="small" 
          color="success" 
          variant="outlined" 
        />
        {summary.failed > 0 && (
          <Chip 
            label={`${summary.failed} failed`} 
            size="small" 
            color="error" 
            variant="outlined" 
          />
        )}
        {summary.pending > 0 && (
          <Chip 
            label={`${summary.pending} in progress`} 
            size="small" 
            color="info" 
            variant="outlined" 
          />
        )}
        <Chip 
          label={`Total: ${formatDuration(summary.totalDuration)}`} 
          size="small" 
          variant="outlined" 
        />
        <Chip 
          label={`Avg: ${formatDuration(summary.avgDuration)}`} 
          size="small" 
          variant="outlined" 
        />
      </Box>

      {/* Timeline */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <TimeScale startTime={timelineStart} duration={timelineDuration} />
        
        {sortedSteps.map((step) => (
          <TimelineBar
            key={step.id}
            step={step}
            timelineStart={timelineStart}
            timelineDuration={timelineDuration}
            showDuration={showDurations}
          />
        ))}
      </Paper>

      {/* Legend */}
      <Box sx={{ display: 'flex', gap: 2, mt: 2, flexWrap: 'wrap' }}>
        <Typography variant="caption" color="text.secondary" sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
          Legend:
        </Typography>
        {Object.entries(defaultStatusConfig).map(([key, config]) => (
          <Box key={key} sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <Box
              sx={{
                width: 16,
                height: 8,
                backgroundColor: config.bgColor,
                border: `1px solid ${config.color}`,
                borderRadius: 0.5,
              }}
            />
            <Typography variant="caption" color="text.secondary">
              {config.label}
            </Typography>
          </Box>
        ))}
      </Box>
    </Box>
  );
};

export default ExecutionTimeline;

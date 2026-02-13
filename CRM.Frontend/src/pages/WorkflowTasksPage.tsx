/**
 * CRM Solution - Workflow Tasks Page
 */

import React, { useCallback, useEffect, useState } from 'react';
import { Box, Typography, Alert, CircularProgress } from '@mui/material';
import { workflowTaskService } from '../services/workflowTaskService';
import type { HumanTask } from '../services/workflowService';
import { TaskList, TaskApprovalDialog } from '../components/workflow';

const WorkflowTasksPage: React.FC = () => {
  const [tasks, setTasks] = useState<HumanTask[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedTask, setSelectedTask] = useState<HumanTask | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);

  const loadTasks = useCallback(async () => {
    try {
      setLoading(true);
      const result = await workflowTaskService.getMyTasks();
      setTasks(result);
      setError('');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load workflow tasks');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadTasks();
  }, [loadTasks]);

  const handleComplete = (task: HumanTask) => {
    setSelectedTask(task);
    setDialogOpen(true);
  };

  const handleSubmit = async (taskId: number, formData?: string, outputData?: string) => {
    await workflowTaskService.completeTask(taskId, formData, outputData);
    setDialogOpen(false);
    setSelectedTask(null);
    loadTasks();
  };

  return (
    <Box sx={{ p: 3, display: 'grid', gap: 3 }}>
      <Typography variant="h5" fontWeight="bold">My Workflow Tasks</Typography>

      {error && <Alert severity="error">{error}</Alert>}
      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
          <CircularProgress />
        </Box>
      ) : (
        <TaskList tasks={tasks} onComplete={handleComplete} />
      )}

      <TaskApprovalDialog
        open={dialogOpen}
        task={selectedTask}
        onClose={() => {
          setDialogOpen(false);
          setSelectedTask(null);
        }}
        onSubmit={handleSubmit}
      />
    </Box>
  );
};

export default WorkflowTasksPage;

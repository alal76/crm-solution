/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Task Approval Dialog - complete or reject a workflow task
 */

import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Typography,
  Box,
} from '@mui/material';
import type { HumanTask } from '../../services/workflowService';

interface TaskApprovalDialogProps {
  open: boolean;
  task?: HumanTask | null;
  onClose: () => void;
  onSubmit: (taskId: number, formData?: string, outputData?: string) => Promise<void> | void;
}

const TaskApprovalDialog: React.FC<TaskApprovalDialogProps> = ({
  open,
  task,
  onClose,
  onSubmit,
}) => {
  const [comments, setComments] = useState('');
  const [outputData, setOutputData] = useState('');

  useEffect(() => {
    if (!open) return;
    setComments('');
    setOutputData('');
  }, [open]);

  const handleSubmit = async () => {
    if (!task) return;
    const output = outputData || (comments ? JSON.stringify({ comments }) : undefined);
    await onSubmit(task.id, comments || undefined, output);
  };

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>Complete Task</DialogTitle>
      <DialogContent>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
          <Box>
            <Typography variant="subtitle2" color="text.secondary">Task</Typography>
            <Typography variant="body1" fontWeight="medium">
              {task?.name || 'Task'}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {task?.workflowName}
            </Typography>
          </Box>
          <TextField
            label="Comments"
            multiline
            minRows={3}
            value={comments}
            onChange={(e) => setComments(e.target.value)}
            placeholder="Add a note or decision rationale"
          />
          <TextField
            label="Output Data (JSON)"
            multiline
            minRows={3}
            value={outputData}
            onChange={(e) => setOutputData(e.target.value)}
            placeholder='{"approved": true, "notes": "..."}'
          />
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" onClick={handleSubmit} disabled={!task}>
          Complete
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default TaskApprovalDialog;

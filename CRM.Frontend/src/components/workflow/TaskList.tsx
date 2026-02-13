/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Task List - grid list of human workflow tasks
 */

import React from 'react';
import { Grid, Typography, Box } from '@mui/material';
import type { HumanTask } from '../../services/workflowService';
import TaskCard from './TaskCard';

interface TaskListProps {
  tasks: HumanTask[];
  onComplete: (task: HumanTask) => void;
}

const TaskList: React.FC<TaskListProps> = ({ tasks, onComplete }) => {
  if (!tasks.length) {
    return (
      <Box sx={{ py: 4, textAlign: 'center' }}>
        <Typography variant="body2" color="text.secondary">
          No pending workflow tasks.
        </Typography>
      </Box>
    );
  }

  return (
    <Grid container spacing={2}>
      {tasks.map(task => (
        <Grid item xs={12} md={6} lg={4} key={task.id}>
          <TaskCard task={task} onComplete={onComplete} />
        </Grid>
      ))}
    </Grid>
  );
};

export default TaskList;

/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Task Card - display a human workflow task
 */

import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
  Chip,
  Button,
  Stack,
} from '@mui/material';
import type { HumanTask } from '../../services/workflowService';

interface TaskCardProps {
  task: HumanTask;
  onComplete: (task: HumanTask) => void;
}

const TaskCard: React.FC<TaskCardProps> = ({ task, onComplete }) => {
  return (
    <Card variant="outlined" sx={{ height: '100%' }}>
      <CardContent>
        <Stack spacing={1.5}>
          <Box>
            <Typography variant="subtitle1" fontWeight="medium">
              {task.name}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {task.workflowName} • {task.nodeName}
            </Typography>
          </Box>
          {task.description && (
            <Typography variant="body2" color="text.secondary">
              {task.description}
            </Typography>
          )}
          <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
            <Chip size="small" label={`Priority ${task.priority}`} />
            {task.dueAt && (
              <Chip
                size="small"
                color="warning"
                label={`Due ${new Date(task.dueAt).toLocaleDateString()}`}
              />
            )}
          </Box>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
            <Button variant="contained" size="small" onClick={() => onComplete(task)}>
              Complete
            </Button>
          </Box>
        </Stack>
      </CardContent>
    </Card>
  );
};

export default TaskCard;

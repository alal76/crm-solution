/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Workflow Viewer - read-only summary of nodes and transitions
 */

import React from 'react';
import { Box, Typography, Paper, List, ListItem, ListItemText, Divider } from '@mui/material';
import type { WorkflowNode, WorkflowTransition } from '../../services/workflowService';

interface WorkflowViewerProps {
  nodes: WorkflowNode[];
  transitions: WorkflowTransition[];
}

const WorkflowViewer: React.FC<WorkflowViewerProps> = ({ nodes, transitions }) => {
  return (
    <Box sx={{ display: 'grid', gap: 2 }}>
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>Nodes</Typography>
        <List dense>
          {nodes.map((node) => (
            <ListItem key={node.id}>
              <ListItemText
                primary={`${node.name} (${node.nodeType})`}
                secondary={`Key: ${node.nodeKey}${node.isStartNode ? ' • Start' : ''}${node.isEndNode ? ' • End' : ''}`}
              />
            </ListItem>
          ))}
          {!nodes.length && (
            <ListItem>
              <ListItemText primary="No nodes defined" />
            </ListItem>
          )}
        </List>
      </Paper>

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>Transitions</Typography>
        <List dense>
          {transitions.map((transition) => (
            <React.Fragment key={transition.id}>
              <ListItem>
                <ListItemText
                  primary={transition.label || 'Transition'}
                  secondary={`From ${transition.sourceNodeId} → ${transition.targetNodeId}`}
                />
              </ListItem>
              <Divider component="li" />
            </React.Fragment>
          ))}
          {!transitions.length && (
            <ListItem>
              <ListItemText primary="No transitions defined" />
            </ListItem>
          )}
        </List>
      </Paper>
    </Box>
  );
};

export default WorkflowViewer;

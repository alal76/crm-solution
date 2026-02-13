/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Node Palette - draggable node list for workflow designer
 */

import React from 'react';
import {
  Box,
  Collapse,
  Divider,
  FormControlLabel,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Switch,
} from '@mui/material';
import {
  Add as AddIcon,
  ExpandLess as ExpandLessIcon,
  ExpandMore as ExpandMoreIcon,
} from '@mui/icons-material';
import { nodeTypeInfo } from '../../services/workflowService';

export interface NodePaletteItem {
  type: string;
  label: string;
  description?: string;
  icon: React.ComponentType<{ fontSize?: 'small' | 'medium' | 'large' | 'inherit' }>;
}

interface NodePaletteProps {
  nodeTypes: NodePaletteItem[];
  expanded: boolean;
  onToggle: () => void;
  onAddNode: (nodeType: string) => void;
  showGrid: boolean;
  onToggleGrid: (value: boolean) => void;
}

const NodePalette: React.FC<NodePaletteProps> = ({
  nodeTypes,
  expanded,
  onToggle,
  onAddNode,
  showGrid,
  onToggleGrid,
}) => {
  return (
    <Box>
      <ListItem button onClick={onToggle}>
        <ListItemIcon>
          <AddIcon />
        </ListItemIcon>
        <ListItemText primary="Nodes" />
        {expanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
      </ListItem>
      <Collapse in={expanded}>
        <List dense>
          {nodeTypes.map(nt => (
            <ListItem key={nt.type} button onClick={() => onAddNode(nt.type)}>
              <ListItemIcon sx={{ minWidth: 36, color: nodeTypeInfo[nt.type]?.color }}>
                <nt.icon fontSize="small" />
              </ListItemIcon>
              <ListItemText
                primary={nt.label}
                secondary={nt.description}
                primaryTypographyProps={{ variant: 'body2' }}
                secondaryTypographyProps={{ variant: 'caption' }}
              />
            </ListItem>
          ))}
        </List>
      </Collapse>
      <Divider />
      <Box sx={{ p: 2 }}>
        <FormControlLabel
          control={
            <Switch
              checked={showGrid}
              onChange={(e) => onToggleGrid(e.target.checked)}
              size="small"
            />
          }
          label="Show Grid"
        />
      </Box>
    </Box>
  );
};

export default NodePalette;

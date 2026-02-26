/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Workflow Canvas - container for design surface
 */

import React from 'react';
import { Box } from '@mui/material';

interface WorkflowCanvasProps {
  canvasRef?: React.RefObject<HTMLDivElement>;
  showGrid: boolean;
  zoom: number;
  pan: { x: number; y: number };
  isPanning: boolean;
  onMouseDown?: React.MouseEventHandler<HTMLDivElement>;
  onMouseMove?: React.MouseEventHandler<HTMLDivElement>;
  onMouseUp?: React.MouseEventHandler<HTMLDivElement>;
  onMouseLeave?: React.MouseEventHandler<HTMLDivElement>;
  gridSize?: number;
  children: React.ReactNode;
}

const WorkflowCanvas: React.FC<WorkflowCanvasProps> = ({
  canvasRef,
  showGrid,
  zoom,
  pan,
  isPanning,
  onMouseDown,
  onMouseMove,
  onMouseUp,
  onMouseLeave,
  gridSize = 20,
  children,
}) => {
  return (
    <Box
      ref={canvasRef}
      sx={{
        flexGrow: 1,
        flexShrink: 1,
        flexBasis: 0,
        minWidth: 0,        // prevents canvas from blocking shrink when script panel opens
        overflow: 'hidden',
        position: 'relative',
        backgroundColor: '#f5f5f5',
        cursor: isPanning ? 'grabbing' : 'default',
      }}
      onMouseDown={onMouseDown}
      onMouseMove={onMouseMove}
      onMouseUp={onMouseUp}
      onMouseLeave={onMouseLeave}
    >
      {showGrid && (
        <Box
          className="canvas-grid"
          sx={{
            position: 'absolute',
            inset: 0,
            backgroundImage: `
              linear-gradient(rgba(0,0,0,0.05) 1px, transparent 1px),
              linear-gradient(90deg, rgba(0,0,0,0.05) 1px, transparent 1px)
            `,
            backgroundSize: `${gridSize * zoom}px ${gridSize * zoom}px`,
            backgroundPosition: `${pan.x}px ${pan.y}px`,
          }}
        />
      )}
      {children}
    </Box>
  );
};

export default WorkflowCanvas;

/**
 * SplitView - Component for comparing two records side-by-side
 */

import React, { useState, useCallback } from 'react';
import {
  Box,
  Paper,
  Grid,
  IconButton,
  Typography,
  Divider,
  Tooltip,
  Stack,
  Slider,
  useTheme,
} from '@mui/material';
import {
  SwapHoriz as SwapIcon,
  Close as CloseIcon,
  VerticalSplit as SplitVerticalIcon,
  HorizontalSplit as SplitHorizontalIcon,
  Fullscreen as FullscreenIcon,
  FullscreenExit as FullscreenExitIcon,
  SyncAlt as SyncIcon,
} from '@mui/icons-material';

export type SplitOrientation = 'horizontal' | 'vertical';

export interface SplitViewProps<T> {
  // Data
  leftItem: T;
  rightItem: T;
  // Rendering
  renderItem: (item: T, side: 'left' | 'right') => React.ReactNode;
  renderHeader?: (item: T, side: 'left' | 'right') => React.ReactNode;
  // Labels
  leftLabel?: string;
  rightLabel?: string;
  // Options
  orientation?: SplitOrientation;
  defaultRatio?: number; // 0-100, percentage for left panel
  resizable?: boolean;
  swappable?: boolean;
  closable?: boolean;
  // Callbacks
  onSwap?: () => void;
  onClose?: () => void;
  onOrientationChange?: (orientation: SplitOrientation) => void;
  // Accessibility
  ariaLabel?: string;
  // Styling
  minPanelWidth?: number;
  fullscreen?: boolean;
  onFullscreenToggle?: () => void;
}

export function SplitView<T>({
  leftItem,
  rightItem,
  renderItem,
  renderHeader,
  leftLabel = 'Left',
  rightLabel = 'Right',
  orientation = 'horizontal',
  defaultRatio = 50,
  resizable = true,
  swappable = true,
  closable = true,
  onSwap,
  onClose,
  onOrientationChange,
  ariaLabel = 'Split view comparison',
  minPanelWidth = 200,
  fullscreen = false,
  onFullscreenToggle,
}: SplitViewProps<T>): React.ReactElement {
  const theme = useTheme();
  const [ratio, setRatio] = useState(defaultRatio);
  const [localOrientation, setLocalOrientation] = useState(orientation);
  const [isFullscreen, setIsFullscreen] = useState(fullscreen);

  // Handle orientation toggle
  const handleOrientationToggle = useCallback(() => {
    const newOrientation = localOrientation === 'horizontal' ? 'vertical' : 'horizontal';
    setLocalOrientation(newOrientation);
    onOrientationChange?.(newOrientation);
  }, [localOrientation, onOrientationChange]);

  // Handle fullscreen toggle
  const handleFullscreenToggle = useCallback(() => {
    setIsFullscreen(!isFullscreen);
    onFullscreenToggle?.();
  }, [isFullscreen, onFullscreenToggle]);

  // Handle ratio change
  const handleRatioChange = useCallback((_: Event, value: number | number[]) => {
    setRatio(value as number);
  }, []);

  // Calculate panel sizes
  const leftSize = ratio;
  const rightSize = 100 - ratio;

  // Render panel
  const renderPanel = (item: T, side: 'left' | 'right', label: string) => (
    <Paper
      variant="outlined"
      sx={{
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        overflow: 'hidden',
      }}
    >
      {/* Panel header */}
      <Box
        sx={{
          p: 1.5,
          borderBottom: 1,
          borderColor: 'divider',
          bgcolor: theme.palette.action.hover,
        }}
      >
        {renderHeader ? (
          renderHeader(item, side)
        ) : (
          <Typography variant="subtitle2" fontWeight={600}>
            {label}
          </Typography>
        )}
      </Box>

      {/* Panel content */}
      <Box
        sx={{
          flex: 1,
          overflow: 'auto',
          p: 2,
        }}
        role="region"
        aria-label={`${side} panel: ${label}`}
      >
        {renderItem(item, side)}
      </Box>
    </Paper>
  );

  return (
    <Box
      sx={{
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        ...(isFullscreen && {
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          zIndex: theme.zIndex.modal,
          bgcolor: 'background.default',
        }),
      }}
      role="region"
      aria-label={ariaLabel}
    >
      {/* Toolbar */}
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          p: 1,
          borderBottom: 1,
          borderColor: 'divider',
          bgcolor: 'background.paper',
        }}
      >
        <Stack direction="row" spacing={1} alignItems="center">
          <Typography variant="subtitle2" color="text.secondary">
            Comparing: {leftLabel} vs {rightLabel}
          </Typography>
          
          {swappable && (
            <Tooltip title="Swap sides">
              <IconButton size="small" onClick={onSwap} aria-label="Swap left and right panels">
                <SwapIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </Stack>

        <Stack direction="row" spacing={1} alignItems="center">
          {/* Resize slider */}
          {resizable && localOrientation === 'horizontal' && (
            <Box sx={{ width: 120, mx: 2 }}>
              <Tooltip title={`Split ratio: ${leftSize}% / ${rightSize}%`}>
                <Slider
                  value={ratio}
                  onChange={handleRatioChange}
                  min={20}
                  max={80}
                  size="small"
                  aria-label="Adjust split ratio"
                />
              </Tooltip>
            </Box>
          )}

          {/* Orientation toggle */}
          <Tooltip title={`Switch to ${localOrientation === 'horizontal' ? 'vertical' : 'horizontal'} split`}>
            <IconButton size="small" onClick={handleOrientationToggle} aria-label="Toggle split orientation">
              {localOrientation === 'horizontal' ? (
                <SplitHorizontalIcon fontSize="small" />
              ) : (
                <SplitVerticalIcon fontSize="small" />
              )}
            </IconButton>
          </Tooltip>

          {/* Fullscreen toggle */}
          <Tooltip title={isFullscreen ? 'Exit fullscreen' : 'Fullscreen'}>
            <IconButton size="small" onClick={handleFullscreenToggle} aria-label="Toggle fullscreen">
              {isFullscreen ? (
                <FullscreenExitIcon fontSize="small" />
              ) : (
                <FullscreenIcon fontSize="small" />
              )}
            </IconButton>
          </Tooltip>

          {/* Close button */}
          {closable && (
            <Tooltip title="Close split view">
              <IconButton size="small" onClick={onClose} aria-label="Close split view">
                <CloseIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </Stack>
      </Box>

      {/* Split content */}
      <Box
        sx={{
          flex: 1,
          overflow: 'hidden',
          display: 'flex',
          flexDirection: localOrientation === 'horizontal' ? 'row' : 'column',
          gap: 1,
          p: 1,
        }}
      >
        {/* Left panel */}
        <Box
          sx={{
            ...(localOrientation === 'horizontal'
              ? { width: `${leftSize}%`, minWidth: minPanelWidth }
              : { height: `${leftSize}%`, minHeight: minPanelWidth }),
            overflow: 'hidden',
          }}
        >
          {renderPanel(leftItem, 'left', leftLabel)}
        </Box>

        {/* Divider */}
        <Divider
          orientation={localOrientation === 'horizontal' ? 'vertical' : 'horizontal'}
          flexItem
          sx={{
            borderColor: theme.palette.primary.main,
            borderWidth: 2,
            opacity: 0.5,
          }}
        />

        {/* Right panel */}
        <Box
          sx={{
            ...(localOrientation === 'horizontal'
              ? { width: `${rightSize}%`, minWidth: minPanelWidth }
              : { height: `${rightSize}%`, minHeight: minPanelWidth }),
            overflow: 'hidden',
          }}
        >
          {renderPanel(rightItem, 'right', rightLabel)}
        </Box>
      </Box>
    </Box>
  );
}

export default SplitView;

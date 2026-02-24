/**
 * FontSizeControl - Accessible +/- font size adjustment buttons
 * TODO-UX-05: Adjusts root font size via the FontSizeContext
 */

import React from 'react';
import {
  Box,
  IconButton,
  Typography,
  Tooltip,
  Stack,
  useTheme,
} from '@mui/material';
import {
  TextDecrease as DecreaseIcon,
  TextIncrease as IncreaseIcon,
  RestartAlt as ResetIcon,
} from '@mui/icons-material';
import { useFontSize, fontSizeLabels } from '../../contexts/FontSizeContext';

// --------------------------------------------------------------------------
// Types
// --------------------------------------------------------------------------

export interface FontSizeControlProps {
  /** Show the current size label between buttons */
  showLabel?: boolean;
  /** Show the reset button */
  showReset?: boolean;
  /** Visual size of buttons */
  size?: 'small' | 'medium';
  /** Orientation of the control */
  orientation?: 'horizontal' | 'vertical';
}

// --------------------------------------------------------------------------
// Component
// --------------------------------------------------------------------------

export const FontSizeControl: React.FC<FontSizeControlProps> = ({
  showLabel = true,
  showReset = true,
  size = 'medium',
  orientation = 'horizontal',
}) => {
  const theme = useTheme();
  const { fontSize, setFontSize, increaseFontSize, decreaseFontSize } = useFontSize();

  const isSmallest = fontSize === 'small';
  const isLargest = fontSize === 'large';

  return (
    <Stack
      direction={orientation === 'horizontal' ? 'row' : 'column'}
      alignItems="center"
      spacing={0.5}
      role="group"
      aria-label="Font size controls"
    >
      <Tooltip title="Decrease font size">
        <span>
          <IconButton
            onClick={decreaseFontSize}
            disabled={isSmallest}
            size={size}
            aria-label="Decrease font size"
          >
            <DecreaseIcon fontSize={size} />
          </IconButton>
        </span>
      </Tooltip>

      {showLabel && (
        <Typography
          variant="body2"
          sx={{
            minWidth: 56,
            textAlign: 'center',
            fontWeight: 500,
            userSelect: 'none',
            color: theme.palette.text.secondary,
          }}
          aria-live="polite"
        >
          {fontSizeLabels[fontSize]}
        </Typography>
      )}

      <Tooltip title="Increase font size">
        <span>
          <IconButton
            onClick={increaseFontSize}
            disabled={isLargest}
            size={size}
            aria-label="Increase font size"
          >
            <IncreaseIcon fontSize={size} />
          </IconButton>
        </span>
      </Tooltip>

      {showReset && fontSize !== 'medium' && (
        <Tooltip title="Reset to default size">
          <IconButton
            onClick={() => setFontSize('medium')}
            size={size}
            aria-label="Reset font size to medium"
          >
            <ResetIcon fontSize={size} />
          </IconButton>
        </Tooltip>
      )}
    </Stack>
  );
};

export default FontSizeControl;

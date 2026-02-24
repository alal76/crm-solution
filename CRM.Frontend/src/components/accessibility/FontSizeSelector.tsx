/**
 * FontSizeSelector - UI component for selecting font size
 */

import React from 'react';
import {
  Box,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
  IconButton,
  Stack,
  Tooltip,
  Paper,
  useTheme,
} from '@mui/material';
import {
  TextDecrease as TextDecreaseIcon,
  TextIncrease as TextIncreaseIcon,
  TextFields as TextFieldsIcon,
} from '@mui/icons-material';
import { useFontSize, FontSize, fontSizeLabels } from '../../contexts/FontSizeContext';

export interface FontSizeSelectorProps {
  variant?: 'toggle' | 'buttons' | 'compact';
  showLabel?: boolean;
  label?: string;
}

export const FontSizeSelector: React.FC<FontSizeSelectorProps> = ({
  variant = 'toggle',
  showLabel = true,
  label = 'Font Size',
}) => {
  const theme = useTheme();
  const { fontSize, setFontSize, increaseFontSize, decreaseFontSize } = useFontSize();

  // Toggle variant - ToggleButtonGroup
  if (variant === 'toggle') {
    return (
      <Box>
        {showLabel && (
          <Typography variant="subtitle2" sx={{ mb: 1 }}>
            {label}
          </Typography>
        )}
        <ToggleButtonGroup
          value={fontSize}
          exclusive
          onChange={(_, value) => {
            if (value) setFontSize(value as FontSize);
          }}
          aria-label="Font size selection"
          size="small"
        >
          <ToggleButton
            value="small"
            aria-label="Small font size"
            sx={{ px: 2 }}
          >
            <Typography variant="body2" sx={{ fontSize: '0.75rem' }}>
              A
            </Typography>
          </ToggleButton>
          <ToggleButton
            value="medium"
            aria-label="Medium font size"
            sx={{ px: 2 }}
          >
            <Typography variant="body2" sx={{ fontSize: '1rem' }}>
              A
            </Typography>
          </ToggleButton>
          <ToggleButton
            value="large"
            aria-label="Large font size"
            sx={{ px: 2 }}
          >
            <Typography variant="body2" sx={{ fontSize: '1.25rem' }}>
              A
            </Typography>
          </ToggleButton>
        </ToggleButtonGroup>
      </Box>
    );
  }

  // Buttons variant - Increase/Decrease buttons
  if (variant === 'buttons') {
    return (
      <Box>
        {showLabel && (
          <Typography variant="subtitle2" sx={{ mb: 1 }}>
            {label}
          </Typography>
        )}
        <Stack direction="row" spacing={1} alignItems="center">
          <Tooltip title="Decrease font size">
            <span>
              <IconButton
                onClick={decreaseFontSize}
                disabled={fontSize === 'small'}
                aria-label="Decrease font size"
                size="small"
              >
                <TextDecreaseIcon />
              </IconButton>
            </span>
          </Tooltip>
          
          <Paper
            variant="outlined"
            sx={{
              px: 2,
              py: 0.5,
              minWidth: 80,
              textAlign: 'center',
            }}
          >
            <Typography variant="body2">
              {fontSizeLabels[fontSize]}
            </Typography>
          </Paper>
          
          <Tooltip title="Increase font size">
            <span>
              <IconButton
                onClick={increaseFontSize}
                disabled={fontSize === 'large'}
                aria-label="Increase font size"
                size="small"
              >
                <TextIncreaseIcon />
              </IconButton>
            </span>
          </Tooltip>
        </Stack>
      </Box>
    );
  }

  // Compact variant - Icon only
  return (
    <Stack direction="row" spacing={0.5} alignItems="center">
      <Tooltip title="Decrease font size">
        <span>
          <IconButton
            onClick={decreaseFontSize}
            disabled={fontSize === 'small'}
            aria-label="Decrease font size"
            size="small"
          >
            <TextDecreaseIcon fontSize="small" />
          </IconButton>
        </span>
      </Tooltip>
      
      <Tooltip title={`Font size: ${fontSizeLabels[fontSize]}`}>
        <IconButton
          size="small"
          aria-label={`Current font size: ${fontSizeLabels[fontSize]}`}
          sx={{ cursor: 'default' }}
        >
          <TextFieldsIcon fontSize="small" />
        </IconButton>
      </Tooltip>
      
      <Tooltip title="Increase font size">
        <span>
          <IconButton
            onClick={increaseFontSize}
            disabled={fontSize === 'large'}
            aria-label="Increase font size"
            size="small"
          >
            <TextIncreaseIcon fontSize="small" />
          </IconButton>
        </span>
      </Tooltip>
    </Stack>
  );
};

export default FontSizeSelector;

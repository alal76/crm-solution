/**
 * ENUM-FE-010: EnumColorPicker.tsx
 * Color picker for enum values. Shows a pallete of predefined Material Design
 * colors as clickable swatches plus a text field for custom hex input.
 */
import React, { useState } from 'react';
import {
  Box,
  Button,
  InputAdornment,
  Popover,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import { Palette as PaletteIcon } from '@mui/icons-material';

const PRESET_COLORS: { label: string; value: string }[] = [
  { label: 'Red', value: '#F44336' },
  { label: 'Pink', value: '#E91E63' },
  { label: 'Purple', value: '#9C27B0' },
  { label: 'Deep Purple', value: '#673AB7' },
  { label: 'Indigo', value: '#3F51B5' },
  { label: 'Blue', value: '#2196F3' },
  { label: 'Light Blue', value: '#03A9F4' },
  { label: 'Cyan', value: '#00BCD4' },
  { label: 'Teal', value: '#009688' },
  { label: 'Green', value: '#4CAF50' },
  { label: 'Light Green', value: '#8BC34A' },
  { label: 'Lime', value: '#CDDC39' },
  { label: 'Yellow', value: '#FFEB3B' },
  { label: 'Amber', value: '#FFC107' },
  { label: 'Orange', value: '#FF9800' },
  { label: 'Deep Orange', value: '#FF5722' },
  { label: 'Brown', value: '#795548' },
  { label: 'Grey', value: '#9E9E9E' },
  { label: 'Blue Grey', value: '#607D8B' },
  { label: 'None', value: '' },
];

export interface EnumColorPickerProps {
  value: string;
  onChange: (color: string) => void;
  disabled?: boolean;
  label?: string;
}

const EnumColorPicker: React.FC<EnumColorPickerProps> = ({
  value,
  onChange,
  disabled = false,
  label = 'Color',
}) => {
  const [anchorEl, setAnchorEl] = useState<HTMLButtonElement | null>(null);

  const open = Boolean(anchorEl);

  return (
    <>
      <TextField
        label={label}
        value={value}
        onChange={e => onChange(e.target.value)}
        fullWidth
        disabled={disabled}
        placeholder="#4CAF50 or rgba(76,175,80,0.9)"
        size="small"
        InputProps={{
          startAdornment: value ? (
            <InputAdornment position="start">
              <Box
                sx={{
                  width: 18,
                  height: 18,
                  borderRadius: '3px',
                  bgcolor: value,
                  border: '1px solid',
                  borderColor: 'divider',
                }}
              />
            </InputAdornment>
          ) : undefined,
          endAdornment: (
            <InputAdornment position="end">
              <Button
                size="small"
                sx={{ minWidth: 28, p: 0.5 }}
                onClick={e => setAnchorEl(e.currentTarget as HTMLButtonElement)}
                disabled={disabled}
              >
                <PaletteIcon fontSize="small" />
              </Button>
            </InputAdornment>
          ),
        }}
      />

      <Popover
        open={open}
        anchorEl={anchorEl}
        onClose={() => setAnchorEl(null)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'left' }}
        PaperProps={{ sx: { p: 1.5, width: 240 } }}
      >
        <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1 }}>
          Select a colour
        </Typography>
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.75 }}>
          {PRESET_COLORS.map(({ label: colorLabel, value: colorVal }) => (
            <Tooltip key={colorLabel} title={colorLabel}>
              <Box
                onClick={() => { onChange(colorVal); setAnchorEl(null); }}
                sx={{
                  width: 26,
                  height: 26,
                  borderRadius: '4px',
                  bgcolor: colorVal || 'transparent',
                  border: '2px solid',
                  borderColor: value === colorVal ? 'primary.main' : 'divider',
                  cursor: 'pointer',
                  '&:hover': { opacity: 0.8 },
                  ...(colorVal === '' && {
                    background: 'repeating-linear-gradient(45deg, #ccc 0, #ccc 4px, #fff 4px, #fff 10px)',
                  }),
                }}
              />
            </Tooltip>
          ))}
        </Box>
      </Popover>
    </>
  );
};

export default EnumColorPicker;

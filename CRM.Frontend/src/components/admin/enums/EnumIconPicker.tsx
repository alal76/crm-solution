/**
 * ENUM-FE-011: EnumIconPicker.tsx
 * A dialog that lets the user type a Material-UI icon name and previews it
 * dynamically using a lazy SvgIcon resolver.
 * Falls back gracefully when icon is not found.
 */
import React, { Suspense, lazy, useCallback, useEffect, useState } from 'react';
import {
  Box,
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  InputAdornment,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import { Search as SearchIcon } from '@mui/icons-material';

/** Common icon picks to display as a quick-select grid */
const COMMON_ICONS = [
  'CheckCircle', 'Cancel', 'Warning', 'Info', 'Star', 'StarBorder',
  'Pending', 'HourglassEmpty', 'Done', 'DoneAll', 'Close',
  'ArrowUpward', 'ArrowDownward', 'Flag', 'FlagOutlined',
  'Error', 'ErrorOutline', 'Block', 'NotInterested',
  'TrendingUp', 'TrendingDown', 'ShowChart', 'BarChart',
  'Person', 'Group', 'Business', 'Support',
];

export interface EnumIconPickerProps {
  value: string;
  onChange: (iconName: string) => void;
  disabled?: boolean;
  label?: string;
}

/** Dynamically resolve a MUI icon component by name */
function useDynamicIcon(name: string) {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const [IconComponent, setIconComponent] = useState<React.ComponentType<any> | null>(null);
  const [error, setError] = useState(false);

  useEffect(() => {
    if (!name.trim()) { setIconComponent(null); setError(false); return; }
    setError(false);
    import('@mui/icons-material')
      .then(icons => {
        const comp = (icons as Record<string, unknown>)[name] as React.ComponentType | undefined;
        if (comp) { setIconComponent(() => comp); }
        else { setIconComponent(null); setError(true); }
      })
      .catch(() => { setIconComponent(null); setError(true); });
  }, [name]);

  return { IconComponent, error };
}

/** Small preview tile for a single icon */
function IconTile({ name, selected, onClick }: { name: string; selected: boolean; onClick: () => void }) {
  const { IconComponent, error } = useDynamicIcon(name);
  if (error) return null;
  return (
    <Tooltip title={name}>
      <Box
        onClick={onClick}
        sx={{
          width: 48,
          height: 48,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          borderRadius: 1,
          border: '2px solid',
          borderColor: selected ? 'primary.main' : 'divider',
          cursor: 'pointer',
          bgcolor: selected ? 'primary.50' : 'transparent',
          '&:hover': { bgcolor: 'action.hover' },
          gap: 0.25,
        }}
      >
        {IconComponent ? <IconComponent sx={{ fontSize: 22 }} /> : <CircularProgress size={16} />}
        <Typography variant="caption" sx={{ fontSize: '0.55rem', textAlign: 'center', lineHeight: 1.1 }}>
          {name.replaceAll(/([A-Z])/g, ' $1').trim().slice(0, 10)}
        </Typography>
      </Box>
    </Tooltip>
  );
}

const EnumIconPicker: React.FC<EnumIconPickerProps> = ({
  value,
  onChange,
  disabled = false,
  label = 'Icon',
}) => {
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState('');
  const [pending, setPending] = useState(value);
  const { IconComponent, error: iconError } = useDynamicIcon(value);

  const handleOpen = useCallback(() => { setPending(value); setSearch(''); setOpen(true); }, [value]);
  const handleSave = useCallback(() => { onChange(pending); setOpen(false); }, [pending, onChange]);

  const filteredIcons = search.trim()
    ? COMMON_ICONS.filter(n => n.toLowerCase().includes(search.toLowerCase()))
    : COMMON_ICONS;

  return (
    <>
      <TextField
        label={label}
        value={value}
        onChange={e => onChange(e.target.value)}
        fullWidth
        disabled={disabled}
        placeholder="CheckCircle"
        size="small"
        InputProps={{
          startAdornment: IconComponent && !iconError ? (
            <InputAdornment position="start">
              <Suspense fallback={<CircularProgress size={14} />}>
                <IconComponent sx={{ fontSize: 18 }} />
              </Suspense>
            </InputAdornment>
          ) : undefined,
          endAdornment: (
            <InputAdornment position="end">
              <Button size="small" sx={{ minWidth: 28, p: 0.5 }} onClick={handleOpen} disabled={disabled}>
                <SearchIcon fontSize="small" />
              </Button>
            </InputAdornment>
          ),
        }}
      />

      <Dialog open={open} onClose={() => setOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Select Icon</DialogTitle>
        <DialogContent dividers>
          <TextField
            fullWidth
            size="small"
            placeholder="Search icons..."
            value={search}
            onChange={e => setSearch(e.target.value)}
            sx={{ mb: 2 }}
          />
          <TextField
            fullWidth
            size="small"
            label="Custom icon name"
            value={pending}
            onChange={e => setPending(e.target.value)}
            helperText="Type any @mui/icons-material icon name (e.g. 'Insights', 'CloudDone')"
            sx={{ mb: 2 }}
          />
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
            {filteredIcons.map(name => (
              <IconTile
                key={name}
                name={name}
                selected={pending === name}
                onClick={() => setPending(name)}
              />
            ))}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Cancel</Button>
          <Button onClick={handleSave} variant="contained">Select</Button>
        </DialogActions>
      </Dialog>
    </>
  );
};

export default EnumIconPicker;

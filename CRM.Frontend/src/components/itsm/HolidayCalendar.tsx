// Holiday Calendar Component - Visual calendar with holiday management
// Part of ITSM Enhancement Plan - SLA Business Hours (TODO-SD003-002)

import React, { useState, useMemo, useCallback } from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  IconButton,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  FormControlLabel,
  Switch,
  Chip,
  Tooltip,
  Stack,
  Divider,
} from '@mui/material';
import {
  ChevronLeft as PrevIcon,
  ChevronRight as NextIcon,
  Add as AddIcon,
  Delete as DeleteIcon,
  Event as EventIcon,
  Circle as CircleIcon,
} from '@mui/icons-material';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface Holiday {
  id: number;
  name: string;
  date: string; // ISO date string (YYYY-MM-DD)
  recurring: boolean;
  country?: string;
  type: 'national' | 'company' | 'regional';
}

export interface HolidayCalendarProps {
  holidays: Holiday[];
  onAdd?: (holiday: Omit<Holiday, 'id'>) => void;
  onDelete?: (id: number) => void;
  year?: number;
  readOnly?: boolean;
}

// ─── Constants ────────────────────────────────────────────────────────────────

const MONTH_NAMES = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
];

const DAY_LABELS = ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'];

const TYPE_COLORS: Record<Holiday['type'], string> = {
  national: '#e53935',  // red
  company: '#1e88e5',   // blue
  regional: '#43a047',  // green
};

const TYPE_LABELS: Record<Holiday['type'], string> = {
  national: 'National',
  company: 'Company',
  regional: 'Regional',
};

// ─── Helper Functions ─────────────────────────────────────────────────────────

function getDaysInMonth(year: number, month: number): number {
  return new Date(year, month + 1, 0).getDate();
}

function getFirstDayOfMonth(year: number, month: number): number {
  return new Date(year, month, 1).getDay();
}

function formatDate(year: number, month: number, day: number): string {
  return `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
}

function parseDate(dateStr: string): { year: number; month: number; day: number } {
  const parts = dateStr.split('-');
  return {
    year: Number.parseInt(parts[0], 10),
    month: Number.parseInt(parts[1], 10) - 1,
    day: Number.parseInt(parts[2], 10),
  };
}

// ─── Mini Month Grid ──────────────────────────────────────────────────────────

interface MiniMonthProps {
  year: number;
  month: number;
  holidays: Holiday[];
  onDayClick?: (date: string) => void;
}

const MiniMonth: React.FC<MiniMonthProps> = ({ year, month, holidays, onDayClick }) => {
  const daysInMonth = getDaysInMonth(year, month);
  const firstDay = getFirstDayOfMonth(year, month);
  const today = new Date();
  const isCurrentMonth = today.getFullYear() === year && today.getMonth() === month;
  const currentDay = today.getDate();

  // Build map of day -> holidays for this month
  const holidaysByDay = useMemo(() => {
    const map: Record<number, Holiday[]> = {};
    holidays.forEach((h) => {
      const parsed = parseDate(h.date);
      const matchesYear = parsed.year === year || h.recurring;
      if (matchesYear && parsed.month === month) {
        if (!map[parsed.day]) map[parsed.day] = [];
        map[parsed.day].push(h);
      }
    });
    return map;
  }, [holidays, year, month]);

  const cells: React.ReactNode[] = [];

  // Empty cells before first day
  for (let i = 0; i < firstDay; i++) {
    cells.push(<Box key={`empty-${i}`} sx={{ width: 24, height: 28 }} />);
  }

  // Day cells
  for (let day = 1; day <= daysInMonth; day++) {
    const dayHolidays = holidaysByDay[day] || [];
    const isToday = isCurrentMonth && day === currentDay;
    const dateStr = formatDate(year, month, day);

    cells.push(
      <Tooltip
        key={day}
        title={
          dayHolidays.length > 0
            ? dayHolidays.map((h) => `${h.name} (${TYPE_LABELS[h.type]})`).join(', ')
            : ''
        }
        arrow
        disableHoverListener={dayHolidays.length === 0}
      >
        <Box
          onClick={() => onDayClick?.(dateStr)}
          sx={{
            width: 24,
            height: 28,
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            borderRadius: '4px',
            cursor: onDayClick ? 'pointer' : 'default',
            position: 'relative',
            backgroundColor: isToday ? 'primary.light' : 'transparent',
            color: isToday ? 'primary.contrastText' : 'text.primary',
            '&:hover': onDayClick
              ? { backgroundColor: isToday ? 'primary.main' : 'action.hover' }
              : {},
          }}
        >
          <Typography variant="caption" sx={{ fontSize: '0.65rem', lineHeight: 1.2 }}>
            {day}
          </Typography>
          {dayHolidays.length > 0 && (
            <Box sx={{ display: 'flex', gap: '1px', position: 'absolute', bottom: 1 }}>
              {dayHolidays.slice(0, 3).map((h, idx) => (
                <CircleIcon
                  key={idx}
                  sx={{ fontSize: 4, color: TYPE_COLORS[h.type] }}
                />
              ))}
            </Box>
          )}
        </Box>
      </Tooltip>
    );
  }

  return (
    <Paper variant="outlined" sx={{ p: 1, minHeight: 180 }}>
      <Typography
        variant="subtitle2"
        align="center"
        sx={{ mb: 0.5, fontWeight: 600, fontSize: '0.75rem' }}
      >
        {MONTH_NAMES[month]}
      </Typography>
      <Box sx={{ display: 'flex', gap: '2px', mb: 0.5 }}>
        {DAY_LABELS.map((d) => (
          <Box key={d} sx={{ width: 24, textAlign: 'center' }}>
            <Typography variant="caption" sx={{ fontSize: '0.6rem', color: 'text.secondary' }}>
              {d}
            </Typography>
          </Box>
        ))}
      </Box>
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: '2px' }}>{cells}</Box>
    </Paper>
  );
};

// ─── Add Holiday Dialog ───────────────────────────────────────────────────────

interface AddHolidayDialogProps {
  open: boolean;
  onClose: () => void;
  onAdd: (holiday: Omit<Holiday, 'id'>) => void;
  initialDate?: string;
}

const AddHolidayDialog: React.FC<AddHolidayDialogProps> = ({
  open,
  onClose,
  onAdd,
  initialDate,
}) => {
  const [name, setName] = useState('');
  const [date, setDate] = useState(initialDate || new Date().toISOString().split('T')[0]);
  const [type, setType] = useState<Holiday['type']>('company');
  const [recurring, setRecurring] = useState(false);
  const [country, setCountry] = useState('');

  const handleSubmit = () => {
    if (!name.trim() || !date) return;
    onAdd({
      name: name.trim(),
      date,
      type,
      recurring,
      country: country.trim() || undefined,
    });
    // Reset form
    setName('');
    setDate(new Date().toISOString().split('T')[0]);
    setType('company');
    setRecurring(false);
    setCountry('');
    onClose();
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle>Add Holiday</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField
            label="Holiday Name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            fullWidth
            required
            autoFocus
          />
          <TextField
            label="Date"
            type="date"
            value={date}
            onChange={(e) => setDate(e.target.value)}
            fullWidth
            required
            InputLabelProps={{ shrink: true }}
          />
          <FormControl fullWidth>
            <InputLabel>Type</InputLabel>
            <Select
              value={type}
              label="Type"
              onChange={(e) => setType(e.target.value as Holiday['type'])}
            >
              <MenuItem value="national">National</MenuItem>
              <MenuItem value="company">Company</MenuItem>
              <MenuItem value="regional">Regional</MenuItem>
            </Select>
          </FormControl>
          <TextField
            label="Country (optional)"
            value={country}
            onChange={(e) => setCountry(e.target.value)}
            fullWidth
            placeholder="e.g., US, UK, IN"
          />
          <FormControlLabel
            control={
              <Switch checked={recurring} onChange={(e) => setRecurring(e.target.checked)} />
            }
            label="Recurring annually"
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={!name.trim() || !date}
        >
          Add Holiday
        </Button>
      </DialogActions>
    </Dialog>
  );
};

// ─── Main Component ───────────────────────────────────────────────────────────

/**
 * Holiday Calendar component for ITSM SLA business hours management.
 *
 * Displays a 12-month compact grid view (4 columns × 3 rows) with holidays
 * highlighted as colored dots. Supports adding and deleting holidays.
 *
 * @example
 * ```tsx
 * <HolidayCalendar
 *   holidays={holidays}
 *   onAdd={(h) => createHoliday(h)}
 *   onDelete={(id) => deleteHoliday(id)}
 *   year={2026}
 * />
 * ```
 */
const HolidayCalendar: React.FC<HolidayCalendarProps> = ({
  holidays,
  onAdd,
  onDelete,
  year: initialYear,
  readOnly = false,
}) => {
  const [currentYear, setCurrentYear] = useState(initialYear || new Date().getFullYear());
  const [addDialogOpen, setAddDialogOpen] = useState(false);
  const [selectedDate, setSelectedDate] = useState<string | undefined>(undefined);

  const handlePrevYear = useCallback(() => setCurrentYear((y) => y - 1), []);
  const handleNextYear = useCallback(() => setCurrentYear((y) => y + 1), []);

  const handleDayClick = useCallback(
    (date: string) => {
      if (!readOnly && onAdd) {
        setSelectedDate(date);
        setAddDialogOpen(true);
      }
    },
    [readOnly, onAdd]
  );

  const handleAddHoliday = useCallback(
    (holiday: Omit<Holiday, 'id'>) => {
      onAdd?.(holiday);
    },
    [onAdd]
  );

  // Group holidays by month for the holiday list sidebar
  const holidayList = useMemo(() => {
    return holidays
      .filter((h) => {
        const parsed = parseDate(h.date);
        return parsed.year === currentYear || h.recurring;
      })
      .sort((a, b) => a.date.localeCompare(b.date));
  }, [holidays, currentYear]);

  return (
    <Box>
      {/* Header: Year selector */}
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          mb: 2,
        }}
      >
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <EventIcon color="primary" />
          <Typography variant="h6">Holiday Calendar</Typography>
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <IconButton onClick={handlePrevYear} size="small">
            <PrevIcon />
          </IconButton>
          <Typography variant="h6" sx={{ minWidth: 60, textAlign: 'center' }}>
            {currentYear}
          </Typography>
          <IconButton onClick={handleNextYear} size="small">
            <NextIcon />
          </IconButton>
        </Box>
        {!readOnly && onAdd && (
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            size="small"
            onClick={() => {
              setSelectedDate(undefined);
              setAddDialogOpen(true);
            }}
          >
            Add Holiday
          </Button>
        )}
      </Box>

      {/* Legend */}
      <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
        {(Object.keys(TYPE_COLORS) as Holiday['type'][]).map((t) => (
          <Box key={t} sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <CircleIcon sx={{ fontSize: 10, color: TYPE_COLORS[t] }} />
            <Typography variant="caption">{TYPE_LABELS[t]}</Typography>
          </Box>
        ))}
      </Box>

      {/* 12-month grid: 4 columns × 3 rows */}
      <Grid container spacing={1}>
        {Array.from({ length: 12 }, (_, month) => (
          <Grid item xs={6} sm={3} key={month}>
            <MiniMonth
              year={currentYear}
              month={month}
              holidays={holidays}
              onDayClick={!readOnly && onAdd ? handleDayClick : undefined}
            />
          </Grid>
        ))}
      </Grid>

      {/* Holiday list below the calendar */}
      {holidayList.length > 0 && (
        <Box sx={{ mt: 3 }}>
          <Divider sx={{ mb: 2 }} />
          <Typography variant="subtitle1" sx={{ mb: 1, fontWeight: 600 }}>
            Holidays in {currentYear} ({holidayList.length})
          </Typography>
          <Stack spacing={0.5}>
            {holidayList.map((h) => {
              const parsed = parseDate(h.date);
              const displayDate = `${MONTH_NAMES[parsed.month]} ${parsed.day}`;
              return (
                <Box
                  key={h.id}
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    py: 0.5,
                    px: 1,
                    borderRadius: 1,
                    '&:hover': { backgroundColor: 'action.hover' },
                  }}
                >
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <CircleIcon sx={{ fontSize: 10, color: TYPE_COLORS[h.type] }} />
                    <Typography variant="body2">{h.name}</Typography>
                    <Typography variant="caption" color="text.secondary">
                      {displayDate}
                    </Typography>
                    {h.recurring && (
                      <Chip label="Recurring" size="small" variant="outlined" sx={{ height: 20 }} />
                    )}
                    {h.country && (
                      <Chip label={h.country} size="small" variant="outlined" sx={{ height: 20 }} />
                    )}
                  </Box>
                  {!readOnly && onDelete && (
                    <IconButton
                      size="small"
                      onClick={() => onDelete(h.id)}
                      color="error"
                      sx={{ opacity: 0.6, '&:hover': { opacity: 1 } }}
                    >
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  )}
                </Box>
              );
            })}
          </Stack>
        </Box>
      )}

      {/* Add Holiday Dialog */}
      {onAdd && (
        <AddHolidayDialog
          open={addDialogOpen}
          onClose={() => setAddDialogOpen(false)}
          onAdd={handleAddHoliday}
          initialDate={selectedDate}
        />
      )}
    </Box>
  );
};

export default HolidayCalendar;

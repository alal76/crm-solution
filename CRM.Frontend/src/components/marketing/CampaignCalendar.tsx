/**
 * CampaignCalendar - Monthly calendar view showing scheduled campaigns
 * Simple grid calendar with campaign pills color-coded by type
 */

import React, { useState, useMemo, useCallback } from 'react';
import {
  Box,
  Typography,
  IconButton,
  Paper,
  Chip,
  Tooltip,
  Grid,
} from '@mui/material';
import {
  ChevronLeft as PrevIcon,
  ChevronRight as NextIcon,
  Today as TodayIcon,
} from '@mui/icons-material';

// ============================================================================
// Types
// ============================================================================

export interface CampaignEvent {
  id: number;
  name: string;
  type: string;
  startDate: string;
  endDate?: string;
  status: string;
}

export interface CampaignCalendarProps {
  campaigns: CampaignEvent[];
  onDateClick?: (date: Date) => void;
  onCampaignClick?: (campaignId: number) => void;
}

// ============================================================================
// Helpers
// ============================================================================

const TYPE_COLORS: Record<string, string> = {
  email: '#1976d2',
  sms: '#9c27b0',
  social: '#e91e63',
  event: '#ff9800',
  webinar: '#4caf50',
  direct: '#795548',
  phone: '#607d8b',
};

function getTypeColor(type: string): string {
  return TYPE_COLORS[type.toLowerCase()] || '#757575';
}

function getDaysInMonth(year: number, month: number): number {
  return new Date(year, month + 1, 0).getDate();
}

function getFirstDayOfMonth(year: number, month: number): number {
  return new Date(year, month, 1).getDay(); // 0=Sun
}

const MONTH_NAMES = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
];

const DAY_HEADERS = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

function isSameDate(d1: Date, d2: Date): boolean {
  return (
    d1.getFullYear() === d2.getFullYear() &&
    d1.getMonth() === d2.getMonth() &&
    d1.getDate() === d2.getDate()
  );
}

function isDateInRange(date: Date, start: string, end?: string): boolean {
  const d = new Date(date.getFullYear(), date.getMonth(), date.getDate());
  const s = new Date(start);
  s.setHours(0, 0, 0, 0);
  if (end) {
    const e = new Date(end);
    e.setHours(23, 59, 59, 999);
    return d >= s && d <= e;
  }
  return isSameDate(d, s);
}

// ============================================================================
// Component
// ============================================================================

const CampaignCalendar: React.FC<CampaignCalendarProps> = ({
  campaigns,
  onDateClick,
  onCampaignClick,
}) => {
  const today = new Date();
  const [year, setYear] = useState(today.getFullYear());
  const [month, setMonth] = useState(today.getMonth());

  const handlePrev = useCallback(() => {
    setMonth((prev) => {
      if (prev === 0) {
        setYear((y) => y - 1);
        return 11;
      }
      return prev - 1;
    });
  }, []);

  const handleNext = useCallback(() => {
    setMonth((prev) => {
      if (prev === 11) {
        setYear((y) => y + 1);
        return 0;
      }
      return prev + 1;
    });
  }, []);

  const handleToday = useCallback(() => {
    setYear(today.getFullYear());
    setMonth(today.getMonth());
  }, [today]);

  // Build the calendar grid
  const calendarDays = useMemo(() => {
    const daysInMonth = getDaysInMonth(year, month);
    const firstDay = getFirstDayOfMonth(year, month);
    const cells: (number | null)[] = [];

    // Leading empty cells
    for (let i = 0; i < firstDay; i++) {
      cells.push(null);
    }
    // Day cells
    for (let d = 1; d <= daysInMonth; d++) {
      cells.push(d);
    }
    // Pad to complete weeks
    while (cells.length % 7 !== 0) {
      cells.push(null);
    }

    return cells;
  }, [year, month]);

  // Map each day to its campaigns
  const campaignsByDay = useMemo(() => {
    const map: Record<number, CampaignEvent[]> = {};
    calendarDays.forEach((day) => {
      if (day === null) return;
      const date = new Date(year, month, day);
      const matching = campaigns.filter((c) => isDateInRange(date, c.startDate, c.endDate));
      if (matching.length > 0) {
        map[day] = matching;
      }
    });
    return map;
  }, [calendarDays, campaigns, year, month]);

  const isToday = (day: number) =>
    day === today.getDate() && month === today.getMonth() && year === today.getFullYear();

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      {/* Header */}
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          mb: 2,
        }}
      >
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <IconButton onClick={handlePrev} size="small">
            <PrevIcon />
          </IconButton>
          <Typography variant="h6" sx={{ minWidth: 180, textAlign: 'center' }}>
            {MONTH_NAMES[month]} {year}
          </Typography>
          <IconButton onClick={handleNext} size="small">
            <NextIcon />
          </IconButton>
        </Box>
        <Tooltip title="Go to today">
          <IconButton onClick={handleToday} size="small">
            <TodayIcon />
          </IconButton>
        </Tooltip>
      </Box>

      {/* Day headers */}
      <Grid container>
        {DAY_HEADERS.map((d) => (
          <Grid
            item
            key={d}
            sx={{
              width: `${100 / 7}%`,
              textAlign: 'center',
              py: 0.5,
            }}
          >
            <Typography variant="caption" fontWeight={600} color="text.secondary">
              {d}
            </Typography>
          </Grid>
        ))}
      </Grid>

      {/* Calendar grid */}
      <Grid container>
        {calendarDays.map((day, idx) => {
          const dayCampaigns = day !== null ? campaignsByDay[day] || [] : [];
          const todayHighlight = day !== null && isToday(day);

          return (
            <Grid
              item
              key={idx}
              sx={{
                width: `${100 / 7}%`,
                minHeight: 80,
                borderTop: '1px solid',
                borderColor: 'divider',
                p: 0.5,
                cursor: day !== null && onDateClick ? 'pointer' : 'default',
                bgcolor: todayHighlight ? 'action.hover' : 'transparent',
                '&:hover': day !== null ? { bgcolor: 'action.selected' } : {},
              }}
              onClick={() => {
                if (day !== null && onDateClick) {
                  onDateClick(new Date(year, month, day));
                }
              }}
            >
              {day !== null && (
                <Box>
                  <Typography
                    variant="body2"
                    sx={{
                      fontWeight: todayHighlight ? 700 : 400,
                      color: todayHighlight ? 'primary.main' : 'text.primary',
                      mb: 0.5,
                    }}
                  >
                    {day}
                  </Typography>
                  {dayCampaigns.slice(0, 2).map((c) => (
                    <Tooltip key={c.id} title={`${c.name} (${c.type})`} arrow>
                      <Chip
                        label={c.name.length > 12 ? `${c.name.slice(0, 12)}…` : c.name}
                        size="small"
                        sx={{
                          mb: 0.25,
                          maxWidth: '100%',
                          height: 20,
                          fontSize: '0.65rem',
                          bgcolor: getTypeColor(c.type),
                          color: '#fff',
                          cursor: 'pointer',
                          '& .MuiChip-label': { px: 0.75 },
                        }}
                        onClick={(e) => {
                          e.stopPropagation();
                          onCampaignClick?.(c.id);
                        }}
                      />
                    </Tooltip>
                  ))}
                  {dayCampaigns.length > 2 && (
                    <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
                      +{dayCampaigns.length - 2} more
                    </Typography>
                  )}
                </Box>
              )}
            </Grid>
          );
        })}
      </Grid>

      {/* Legend */}
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mt: 2, pt: 1, borderTop: '1px solid', borderColor: 'divider' }}>
        {Object.entries(TYPE_COLORS).map(([type, color]) => (
          <Chip
            key={type}
            label={type.charAt(0).toUpperCase() + type.slice(1)}
            size="small"
            sx={{ bgcolor: color, color: '#fff', height: 20, fontSize: '0.7rem' }}
          />
        ))}
      </Box>
    </Paper>
  );
};

export default CampaignCalendar;

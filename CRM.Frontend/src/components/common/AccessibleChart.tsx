/**
 * AccessibleChart - Screen reader support for chart/dashboard visualizations
 * TODO-UX-03: Wraps chart components with aria-label descriptions
 * and provides a togglable data table alternative view.
 */

import React, { useState, useMemo } from 'react';
import {
  Box,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
  IconButton,
  Tooltip,
  Collapse,
  Stack,
  Divider,
} from '@mui/material';
import {
  TableChart as TableIcon,
  ShowChart as ChartIcon,
  Info as InfoIcon,
  BarChart as BarChartIcon,
  PieChart as PieChartIcon,
  BubbleChart as ScatterIcon,
  StackedLineChart as AreaIcon,
  DonutSmall as DonutIcon,
} from '@mui/icons-material';
import { visuallyHidden } from '@mui/utils';

// --------------------------------------------------------------------------
// Types
// --------------------------------------------------------------------------

export type ChartType = 'bar' | 'line' | 'pie' | 'area' | 'scatter' | 'donut';

export interface DataPoint {
  label: string;
  value: number;
  secondaryValue?: number;
  color?: string;
  metadata?: Record<string, string | number>;
}

export interface ChartSeries {
  name: string;
  data: DataPoint[];
  unit?: string;
  color?: string;
}

export interface ChartDescription {
  summary: string;
  trend?: string;
  insights?: string[];
}

export interface AccessibleChartProps {
  /** Chart title */
  title: string;
  /** Optional human‑readable description */
  description?: string;
  /** Chart visualisation type */
  chartType: ChartType;
  /** One or more data series */
  series: ChartSeries[];
  /** Pre‑computed summary for screen readers */
  summaryText?: string;
  /** Structured description object */
  chartDescription?: ChartDescription;
  /** Show the data‑table by default instead of chart */
  showTableByDefault?: boolean;
  /** Allow toggling between chart / table view */
  allowToggle?: boolean;
  /** Show auto‑generated summary */
  showSummary?: boolean;
  /** The actual chart component rendered as children */
  children: React.ReactNode;
  /** Callback when view switches */
  onViewChange?: (view: 'chart' | 'table') => void;
}

// --------------------------------------------------------------------------
// Helpers
// --------------------------------------------------------------------------

const chartTypeLabels: Record<ChartType, string> = {
  bar: 'Bar chart',
  line: 'Line chart',
  pie: 'Pie chart',
  area: 'Area chart',
  scatter: 'Scatter chart',
  donut: 'Donut chart',
};

const chartTypeIcons: Record<ChartType, React.ReactElement> = {
  bar: <BarChartIcon />,
  line: <ChartIcon />,
  pie: <PieChartIcon />,
  area: <AreaIcon />,
  scatter: <ScatterIcon />,
  donut: <DonutIcon />,
};

function generateAutoSummary(series: ChartSeries[], chartType: ChartType): string {
  if (series.length === 0) return 'No data available.';

  const parts: string[] = [`${chartTypeLabels[chartType]} with ${series.length} series.`];

  for (const s of series) {
    if (s.data.length === 0) continue;
    const values = s.data.map((d) => d.value);
    const min = Math.min(...values);
    const max = Math.max(...values);
    const avg = values.reduce((a, b) => a + b, 0) / values.length;
    parts.push(
      `${s.name}: ${s.data.length} data points, range ${min.toLocaleString()}${s.unit ? ` ${s.unit}` : ''} – ${max.toLocaleString()}${s.unit ? ` ${s.unit}` : ''}, average ${avg.toLocaleString(undefined, { maximumFractionDigits: 1 })}${s.unit ? ` ${s.unit}` : ''}.`,
    );
  }

  return parts.join(' ');
}

// --------------------------------------------------------------------------
// Component
// --------------------------------------------------------------------------

export const AccessibleChart: React.FC<AccessibleChartProps> = ({
  title,
  description,
  chartType,
  series,
  summaryText,
  chartDescription,
  showTableByDefault = false,
  allowToggle = true,
  showSummary = true,
  children,
  onViewChange,
}) => {
  const [showTable, setShowTable] = useState(showTableByDefault);
  const descriptionId = React.useId();
  const summaryId = React.useId();

  // Build automatic summary
  const autoSummary = useMemo(
    () => summaryText ?? chartDescription?.summary ?? generateAutoSummary(series, chartType),
    [summaryText, chartDescription, series, chartType],
  );

  // Toggle handler
  const handleToggle = () => {
    const next = !showTable;
    setShowTable(next);
    onViewChange?.(next ? 'table' : 'chart');
  };

  // Build flat data for the table
  const hasMultipleSeries = series.length > 1;

  return (
    <Paper
      role="figure"
      aria-label={`${title} — ${chartTypeLabels[chartType]}`}
      aria-describedby={descriptionId}
      sx={{ p: 2 }}
    >
      {/* Header row */}
      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 1 }}>
        <Stack direction="row" spacing={1} alignItems="center">
          {chartTypeIcons[chartType]}
          <Typography variant="subtitle1" fontWeight={600}>
            {title}
          </Typography>
        </Stack>

        {allowToggle && (
          <Tooltip title={showTable ? 'Show chart' : 'Show data table'}>
            <IconButton
              onClick={handleToggle}
              size="small"
              aria-label={showTable ? 'Switch to chart view' : 'Switch to data table view'}
            >
              {showTable ? <ChartIcon /> : <TableIcon />}
            </IconButton>
          </Tooltip>
        )}
      </Stack>

      {/* Visually‑hidden description for screen readers */}
      <Typography id={descriptionId} sx={visuallyHidden}>
        {autoSummary}
      </Typography>

      {/* Optional visible description */}
      {description && (
        <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
          {description}
        </Typography>
      )}

      {/* Summary bar */}
      {showSummary && chartDescription?.insights && chartDescription.insights.length > 0 && (
        <Box sx={{ mb: 1 }}>
          {chartDescription.insights.map((insight, i) => (
            <Typography key={i} variant="caption" color="text.secondary" display="block">
              • {insight}
            </Typography>
          ))}
        </Box>
      )}

      <Divider sx={{ mb: 1 }} />

      {/* Chart or Table */}
      {showTable ? (
        <TableContainer aria-label={`${title} data table`}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Label</TableCell>
                {hasMultipleSeries && <TableCell>Series</TableCell>}
                <TableCell align="right">Value</TableCell>
                {series.some((s) => s.data.some((d) => d.secondaryValue !== undefined)) && (
                  <TableCell align="right">Secondary</TableCell>
                )}
              </TableRow>
            </TableHead>
            <TableBody>
              {series.flatMap((s) =>
                s.data.map((d, idx) => (
                  <TableRow key={`${s.name}-${idx}`}>
                    <TableCell>{d.label}</TableCell>
                    {hasMultipleSeries && <TableCell>{s.name}</TableCell>}
                    <TableCell align="right">
                      {d.value.toLocaleString()}
                      {s.unit ? ` ${s.unit}` : ''}
                    </TableCell>
                    {d.secondaryValue !== undefined && (
                      <TableCell align="right">{d.secondaryValue.toLocaleString()}</TableCell>
                    )}
                  </TableRow>
                )),
              )}
            </TableBody>
          </Table>
        </TableContainer>
      ) : (
        <Box role="img" aria-label={autoSummary}>
          {children}
        </Box>
      )}
    </Paper>
  );
};

export default AccessibleChart;

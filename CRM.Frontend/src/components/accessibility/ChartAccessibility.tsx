/**
 * ChartAccessibility - Screen reader support for charts
 * Provides tabular alternative view and aria-describedby summaries
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
} from '@mui/material';
import {
  TableChart as TableIcon,
  ShowChart as ChartIcon,
  Info as InfoIcon,
} from '@mui/icons-material';
import { visuallyHidden } from '@mui/utils';

// Data point interface
export interface ChartDataPoint {
  label: string;
  value: number;
  secondaryValue?: number;
  color?: string;
  metadata?: Record<string, string | number>;
}

// Chart series interface
export interface ChartSeries {
  name: string;
  data: ChartDataPoint[];
  unit?: string;
  color?: string;
}

export interface ChartAccessibilityProps {
  // Chart info
  title: string;
  description?: string;
  chartType: 'bar' | 'line' | 'pie' | 'area' | 'scatter' | 'donut';
  // Data
  series: ChartSeries[];
  // Summary
  summaryText?: string;
  // Options
  showTableByDefault?: boolean;
  allowToggle?: boolean;
  showSummary?: boolean;
  // Rendering
  children: React.ReactNode; // The actual chart component
  // Callbacks
  onViewChange?: (view: 'chart' | 'table') => void;
}

export const ChartAccessibility: React.FC<ChartAccessibilityProps> = ({
  title,
  description,
  chartType,
  series,
  summaryText,
  showTableByDefault = false,
  allowToggle = true,
  showSummary = true,
  children,
  onViewChange,
}) => {
  const [showTable, setShowTable] = useState(showTableByDefault);
  const descriptionId = React.useId();
  const summaryId = React.useId();

  // Generate automatic summary text
  const autoSummary = useMemo(() => {
    if (summaryText) return summaryText;

    const summaries: string[] = [];
    
    series.forEach((s) => {
      const values = s.data.map((d) => d.value);
      const total = values.reduce((a, b) => a + b, 0);
      const min = Math.min(...values);
      const max = Math.max(...values);
      const avg = total / values.length;
      
      const minPoint = s.data.find((d) => d.value === min);
      const maxPoint = s.data.find((d) => d.value === max);
      
      let summary = `${s.name}:`;
      
      switch (chartType) {
        case 'pie':
        case 'donut':
          summary += ` Total ${total.toLocaleString()}${s.unit ? ` ${s.unit}` : ''}.`;
          summary += ` Largest segment: ${maxPoint?.label} (${max.toLocaleString()}).`;
          summary += ` Smallest segment: ${minPoint?.label} (${min.toLocaleString()}).`;
          break;
        case 'bar':
        case 'line':
        case 'area':
          summary += ` ${values.length} data points.`;
          summary += ` Range: ${min.toLocaleString()} to ${max.toLocaleString()}${s.unit ? ` ${s.unit}` : ''}.`;
          summary += ` Average: ${avg.toFixed(1)}${s.unit ? ` ${s.unit}` : ''}.`;
          if (maxPoint) summary += ` Peak at ${maxPoint.label}.`;
          break;
        case 'scatter':
          summary += ` ${values.length} data points.`;
          break;
      }
      
      summaries.push(summary);
    });

    return summaries.join(' ');
  }, [series, chartType, summaryText]);

  // Handle view toggle
  const handleToggle = () => {
    const newView = !showTable;
    setShowTable(newView);
    onViewChange?.(newView ? 'table' : 'chart');
  };

  // Combine all data for table view
  const tableData = useMemo(() => {
    if (series.length === 1) {
      return series[0].data.map((point) => ({
        label: point.label,
        values: { [series[0].name]: point.value },
        metadata: point.metadata,
      }));
    }

    // Multiple series - combine by label
    const labelMap = new Map<string, Record<string, number>>();
    
    series.forEach((s) => {
      s.data.forEach((point) => {
        const existing = labelMap.get(point.label) || {};
        existing[s.name] = point.value;
        labelMap.set(point.label, existing);
      });
    });

    return Array.from(labelMap.entries()).map(([label, values]) => ({
      label,
      values,
    }));
  }, [series]);

  return (
    <Box role="figure" aria-labelledby={descriptionId}>
      {/* Header with title and controls */}
      <Stack
        direction="row"
        justifyContent="space-between"
        alignItems="center"
        sx={{ mb: 1 }}
      >
        <Typography variant="h6" id={descriptionId}>
          {title}
        </Typography>
        
        {allowToggle && (
          <Stack direction="row" spacing={1}>
            {showSummary && (
              <Tooltip title="View chart summary">
                <IconButton
                  size="small"
                  aria-label="Show chart summary"
                  aria-describedby={summaryId}
                >
                  <InfoIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
            <Tooltip title={showTable ? 'Show chart' : 'Show data table'}>
              <IconButton
                size="small"
                onClick={handleToggle}
                aria-label={showTable ? 'Switch to chart view' : 'Switch to table view'}
                aria-pressed={showTable}
              >
                {showTable ? <ChartIcon fontSize="small" /> : <TableIcon fontSize="small" />}
              </IconButton>
            </Tooltip>
          </Stack>
        )}
      </Stack>

      {/* Optional description */}
      {description && (
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {description}
        </Typography>
      )}

      {/* Screen reader summary (always available but visually hidden) */}
      <Box id={summaryId} sx={visuallyHidden}>
        {chartType} chart. {autoSummary}
      </Box>

      {/* Chart or Table view */}
      {showTable ? (
        <TableContainer component={Paper} variant="outlined">
          <Table size="small" aria-label={`${title} data table`}>
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600 }}>
                  {chartType === 'scatter' ? 'Point' : 'Category'}
                </TableCell>
                {series.map((s) => (
                  <TableCell key={s.name} align="right" sx={{ fontWeight: 600 }}>
                    {s.name}
                    {s.unit && (
                      <Typography variant="caption" sx={{ ml: 0.5 }}>
                        ({s.unit})
                      </Typography>
                    )}
                  </TableCell>
                ))}
              </TableRow>
            </TableHead>
            <TableBody>
              {tableData.map((row, index) => (
                <TableRow key={index}>
                  <TableCell component="th" scope="row">
                    {row.label}
                  </TableCell>
                  {series.map((s) => (
                    <TableCell key={s.name} align="right">
                      {row.values[s.name]?.toLocaleString() ?? '-'}
                    </TableCell>
                  ))}
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      ) : (
        <Box
          aria-describedby={summaryId}
          aria-label={`${chartType} chart: ${title}`}
          role="img"
        >
          {children}
        </Box>
      )}

      {/* Summary shown below chart */}
      {showSummary && !showTable && (
        <Collapse in={true}>
          <Paper
            variant="outlined"
            sx={{
              mt: 2,
              p: 2,
              bgcolor: 'action.hover',
            }}
          >
            <Typography variant="body2" color="text.secondary">
              <strong>Summary:</strong> {autoSummary}
            </Typography>
          </Paper>
        </Collapse>
      )}
    </Box>
  );
};

// Helper: Wrap any chart with accessibility support
export interface AccessibleChartWrapperProps {
  title: string;
  description?: string;
  chartType: 'bar' | 'line' | 'pie' | 'area' | 'scatter' | 'donut';
  data: ChartDataPoint[];
  seriesName?: string;
  unit?: string;
  children: React.ReactNode;
}

export const AccessibleChartWrapper: React.FC<AccessibleChartWrapperProps> = ({
  title,
  description,
  chartType,
  data,
  seriesName = 'Values',
  unit,
  children,
}) => {
  const series: ChartSeries[] = [
    {
      name: seriesName,
      data,
      unit,
    },
  ];

  return (
    <ChartAccessibility
      title={title}
      description={description}
      chartType={chartType}
      series={series}
    >
      {children}
    </ChartAccessibility>
  );
};

// Type aliases for barrel exports
export type ChartType = ChartAccessibilityProps['chartType'];
export type DataPoint = ChartDataPoint;
export type ChartDescription = {
  title: string;
  description?: string;
  chartType: ChartType;
  summaryText?: string;
};

export default ChartAccessibility;

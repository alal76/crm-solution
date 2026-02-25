import React from 'react';
import { Box, Typography, Paper, Stack, Tooltip as MuiTooltip } from '@mui/material';
import { Funnel, FunnelChart, LabelList, ResponsiveContainer, Cell, Tooltip as RechartsTooltip } from 'recharts';

interface FunnelStage {
  name: string;
  value: number;
  conversionRate?: number;
  dropoff?: number;
}

interface FunnelVisualizationProps {
  data: FunnelStage[];
  title?: string;
  colors?: string[];
  showConversionRates?: boolean;
  height?: number;
}

const defaultColors = ['#1976d2', '#42a5f5', '#64b5f6', '#90caf9', '#bbdefb'];

const FunnelVisualization: React.FC<FunnelVisualizationProps> = ({
  data,
  title = 'Sales Funnel',
  colors = defaultColors,
  showConversionRates = true,
  height = 400,
}) => {
  // Calculate conversion rates if not provided
  const enrichedData = data.map((stage, index) => {
    const previousValue = index > 0 ? data[index - 1].value : stage.value;
    const conversionRate = previousValue > 0 ? (stage.value / previousValue) * 100 : 100;
    const dropoff = previousValue - stage.value;
    return {
      ...stage,
      conversionRate: stage.conversionRate ?? conversionRate,
      dropoff: stage.dropoff ?? dropoff,
    };
  });

  const formatValue = (value: number) => {
    if (value >= 1000000) return `${(value / 1000000).toFixed(1)}M`;
    if (value >= 1000) return `${(value / 1000).toFixed(1)}K`;
    return value.toString();
  };

  const CustomLabel = (props: any) => {
    const { x, y, width, height, name, value, conversionRate } = props;
    return (
      <g>
        <text
          x={x + width / 2}
          y={y + height / 2 - 10}
          fill="#fff"
          textAnchor="middle"
          dominantBaseline="middle"
          fontWeight="bold"
          fontSize={14}
        >
          {name}
        </text>
        <text
          x={x + width / 2}
          y={y + height / 2 + 10}
          fill="#fff"
          textAnchor="middle"
          dominantBaseline="middle"
          fontSize={12}
        >
          {formatValue(value)}
        </text>
        {showConversionRates && conversionRate !== undefined && (
          <text
            x={x + width / 2}
            y={y + height / 2 + 25}
            fill="#fff"
            textAnchor="middle"
            dominantBaseline="middle"
            fontSize={10}
            opacity={0.9}
          >
            {conversionRate.toFixed(1)}% conversion
          </text>
        )}
      </g>
    );
  };

  return (
    <Paper sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom fontWeight={600}>
        {title}
      </Typography>
      <ResponsiveContainer width="100%" height={height}>
        <FunnelChart>
          <RechartsTooltip />
          <Funnel dataKey="value" isAnimationActive={true} data={enrichedData}>
            {enrichedData.map((entry, index) => (
              <Cell key={`cell-${index}`} fill={colors[index % colors.length]} />
            ))}
            <LabelList content={<CustomLabel />} />
          </Funnel>
        </FunnelChart>
      </ResponsiveContainer>

      {/* Stage Conversion Table */}
      {showConversionRates && (
        <Box sx={{ mt: 3 }}>
          <Typography variant="subtitle2" gutterBottom>
            Stage-by-Stage Breakdown
          </Typography>
          <Stack spacing={1}>
            {enrichedData.map((stage, index) => (
              <Box
                key={stage.name}
                sx={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  p: 1,
                  bgcolor: 'grey.50',
                  borderRadius: 1,
                  borderLeft: `4px solid ${colors[index % colors.length]}`,
                }}
              >
                <Typography variant="body2" fontWeight={500}>
                  {stage.name}
                </Typography>
                <Stack direction="row" spacing={2} alignItems="center">
                  <Typography variant="body2">{formatValue(stage.value)}</Typography>
                  <Typography variant="caption" color="text.secondary">
                    {stage.conversionRate.toFixed(1)}% conversion
                  </Typography>
                  {index > 0 && stage.dropoff > 0 && (
                    <Typography variant="caption" color="error.main">
                      -{formatValue(stage.dropoff)} dropped
                    </Typography>
                  )}
                </Stack>
              </Box>
            ))}
          </Stack>
        </Box>
      )}
    </Paper>
  );
};

export default FunnelVisualization;

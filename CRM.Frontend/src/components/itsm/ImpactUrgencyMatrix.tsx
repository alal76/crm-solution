// Impact/Urgency Matrix - Priority calculation component for incidents
// Part of ITSM Enhancement Plan - Phase 1.1

import React, { useState, useCallback, useMemo } from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Tooltip,
  Chip,
  Stack,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  SelectChangeEvent,
} from '@mui/material';
import {
  KeyboardDoubleArrowUp as CriticalIcon,
  KeyboardArrowUp as HighIcon,
  Remove as MediumIcon,
  KeyboardArrowDown as LowIcon,
} from '@mui/icons-material';

export type ImpactLevel = 1 | 2 | 3; // 1=High, 2=Medium, 3=Low
export type UrgencyLevel = 1 | 2 | 3; // 1=High, 2=Medium, 3=Low
export type PriorityLevel = 1 | 2 | 3 | 4 | 5; // 1=Critical, 2=High, 3=Medium, 4=Low, 5=Planning

export interface ImpactUrgencyMatrixProps {
  impact?: ImpactLevel;
  urgency?: UrgencyLevel;
  onChange?: (impact: ImpactLevel, urgency: UrgencyLevel, priority: PriorityLevel) => void;
  readOnly?: boolean;
  showMatrix?: boolean;
  compact?: boolean;
}

// Priority calculation matrix (ITIL standard)
const PRIORITY_MATRIX: Record<ImpactLevel, Record<UrgencyLevel, PriorityLevel>> = {
  1: { 1: 1, 2: 2, 3: 3 }, // High impact
  2: { 1: 2, 2: 3, 3: 4 }, // Medium impact
  3: { 1: 3, 2: 4, 3: 5 }, // Low impact
};

const IMPACT_LABELS: Record<ImpactLevel, string> = {
  1: 'High - Affects many users or critical business function',
  2: 'Medium - Affects a group of users or important function',
  3: 'Low - Affects single user or non-critical function',
};

const URGENCY_LABELS: Record<UrgencyLevel, string> = {
  1: 'High - Work cannot continue, immediate action needed',
  2: 'Medium - Work is degraded but can continue',
  3: 'Low - Inconvenience, workaround available',
};

const PRIORITY_CONFIG: Record<
  PriorityLevel,
  { label: string; color: string; icon: React.ReactNode; sla: string }
> = {
  1: {
    label: 'Critical',
    color: '#d32f2f',
    icon: <CriticalIcon />,
    sla: '1 hour response, 4 hours resolution',
  },
  2: {
    label: 'High',
    color: '#f57c00',
    icon: <HighIcon />,
    sla: '4 hours response, 8 hours resolution',
  },
  3: {
    label: 'Medium',
    color: '#fbc02d',
    icon: <MediumIcon />,
    sla: '8 hours response, 24 hours resolution',
  },
  4: {
    label: 'Low',
    color: '#388e3c',
    icon: <LowIcon />,
    sla: '24 hours response, 72 hours resolution',
  },
  5: {
    label: 'Planning',
    color: '#1976d2',
    icon: <LowIcon />,
    sla: '48 hours response, 1 week resolution',
  },
};

const calculatePriority = (impact: ImpactLevel, urgency: UrgencyLevel): PriorityLevel => {
  return PRIORITY_MATRIX[impact][urgency];
};

interface MatrixCellProps {
  impact: ImpactLevel;
  urgency: UrgencyLevel;
  isSelected: boolean;
  onClick?: () => void;
  readOnly?: boolean;
}

const MatrixCell: React.FC<MatrixCellProps> = ({
  impact,
  urgency,
  isSelected,
  onClick,
  readOnly,
}) => {
  const priority = calculatePriority(impact, urgency);
  const config = PRIORITY_CONFIG[priority];

  return (
    <Tooltip title={`Priority: ${config.label} - ${config.sla}`}>
      <Box
        onClick={readOnly ? undefined : onClick}
        sx={{
          width: 60,
          height: 60,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          backgroundColor: isSelected ? config.color : `${config.color}40`,
          border: isSelected ? `3px solid ${config.color}` : '1px solid #e0e0e0',
          borderRadius: 1,
          cursor: readOnly ? 'default' : 'pointer',
          transition: 'all 0.2s ease',
          '&:hover': readOnly
            ? {}
            : {
                transform: 'scale(1.05)',
                boxShadow: 2,
              },
        }}
      >
        <Typography
          variant="body2"
          fontWeight={isSelected ? 700 : 500}
          sx={{ color: isSelected ? 'white' : config.color }}
        >
          P{priority}
        </Typography>
      </Box>
    </Tooltip>
  );
};

export const ImpactUrgencyMatrix: React.FC<ImpactUrgencyMatrixProps> = ({
  impact: initialImpact = 2,
  urgency: initialUrgency = 2,
  onChange,
  readOnly = false,
  showMatrix = true,
  compact = false,
}) => {
  const [impact, setImpact] = useState<ImpactLevel>(initialImpact);
  const [urgency, setUrgency] = useState<UrgencyLevel>(initialUrgency);

  const priority = useMemo(() => calculatePriority(impact, urgency), [impact, urgency]);
  const priorityConfig = PRIORITY_CONFIG[priority];

  const handleCellClick = useCallback(
    (newImpact: ImpactLevel, newUrgency: UrgencyLevel) => {
      setImpact(newImpact);
      setUrgency(newUrgency);
      const newPriority = calculatePriority(newImpact, newUrgency);
      onChange?.(newImpact, newUrgency, newPriority);
    },
    [onChange]
  );

  const handleImpactChange = (event: SelectChangeEvent<number>) => {
    const newImpact = event.target.value as ImpactLevel;
    setImpact(newImpact);
    const newPriority = calculatePriority(newImpact, urgency);
    onChange?.(newImpact, urgency, newPriority);
  };

  const handleUrgencyChange = (event: SelectChangeEvent<number>) => {
    const newUrgency = event.target.value as UrgencyLevel;
    setUrgency(newUrgency);
    const newPriority = calculatePriority(impact, newUrgency);
    onChange?.(impact, newUrgency, newPriority);
  };

  if (compact) {
    return (
      <Stack direction="row" spacing={2} alignItems="center">
        <Chip
          icon={priorityConfig.icon as React.ReactElement}
          label={`P${priority} - ${priorityConfig.label}`}
          sx={{
            backgroundColor: `${priorityConfig.color}20`,
            color: priorityConfig.color,
            fontWeight: 600,
            '& .MuiChip-icon': {
              color: priorityConfig.color,
            },
          }}
        />
        <Typography variant="caption" color="text.secondary">
          Impact: {impact === 1 ? 'High' : impact === 2 ? 'Medium' : 'Low'} |
          Urgency: {urgency === 1 ? 'High' : urgency === 2 ? 'Medium' : 'Low'}
        </Typography>
      </Stack>
    );
  }

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Typography variant="subtitle1" fontWeight={600} gutterBottom>
        Priority Calculation
      </Typography>

      {/* Dropdowns for selection */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6}>
          <FormControl fullWidth size="small" disabled={readOnly}>
            <InputLabel>Impact</InputLabel>
            <Select
              value={impact}
              label="Impact"
              onChange={handleImpactChange}
            >
              <MenuItem value={1}>
                <Stack>
                  <Typography variant="body2" fontWeight={600}>High</Typography>
                  <Typography variant="caption" color="text.secondary">
                    Affects many users or critical function
                  </Typography>
                </Stack>
              </MenuItem>
              <MenuItem value={2}>
                <Stack>
                  <Typography variant="body2" fontWeight={600}>Medium</Typography>
                  <Typography variant="caption" color="text.secondary">
                    Affects a group or important function
                  </Typography>
                </Stack>
              </MenuItem>
              <MenuItem value={3}>
                <Stack>
                  <Typography variant="body2" fontWeight={600}>Low</Typography>
                  <Typography variant="caption" color="text.secondary">
                    Single user or non-critical function
                  </Typography>
                </Stack>
              </MenuItem>
            </Select>
          </FormControl>
        </Grid>
        <Grid item xs={12} sm={6}>
          <FormControl fullWidth size="small" disabled={readOnly}>
            <InputLabel>Urgency</InputLabel>
            <Select
              value={urgency}
              label="Urgency"
              onChange={handleUrgencyChange}
            >
              <MenuItem value={1}>
                <Stack>
                  <Typography variant="body2" fontWeight={600}>High</Typography>
                  <Typography variant="caption" color="text.secondary">
                    Work cannot continue
                  </Typography>
                </Stack>
              </MenuItem>
              <MenuItem value={2}>
                <Stack>
                  <Typography variant="body2" fontWeight={600}>Medium</Typography>
                  <Typography variant="caption" color="text.secondary">
                    Work is degraded
                  </Typography>
                </Stack>
              </MenuItem>
              <MenuItem value={3}>
                <Stack>
                  <Typography variant="body2" fontWeight={600}>Low</Typography>
                  <Typography variant="caption" color="text.secondary">
                    Workaround available
                  </Typography>
                </Stack>
              </MenuItem>
            </Select>
          </FormControl>
        </Grid>
      </Grid>

      {/* Calculated Priority */}
      <Box
        sx={{
          p: 2,
          mb: 3,
          borderRadius: 1,
          backgroundColor: `${priorityConfig.color}15`,
          border: `2px solid ${priorityConfig.color}`,
        }}
      >
        <Stack direction="row" alignItems="center" spacing={2}>
          <Box sx={{ color: priorityConfig.color }}>{priorityConfig.icon}</Box>
          <Box>
            <Typography variant="h6" sx={{ color: priorityConfig.color }}>
              Priority {priority}: {priorityConfig.label}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              SLA Target: {priorityConfig.sla}
            </Typography>
          </Box>
        </Stack>
      </Box>

      {/* Visual Matrix */}
      {showMatrix && (
        <Box>
          <Typography variant="body2" color="text.secondary" gutterBottom>
            Click on the matrix to select Impact/Urgency:
          </Typography>
          <Box sx={{ display: 'flex', gap: 1 }}>
            {/* Y-axis label */}
            <Box
              sx={{
                display: 'flex',
                flexDirection: 'column',
                justifyContent: 'space-around',
                pr: 1,
              }}
            >
              <Typography variant="caption" sx={{ writingMode: 'vertical-rl', transform: 'rotate(180deg)' }}>
                Impact
              </Typography>
            </Box>

            <Box>
              {/* Matrix grid */}
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
                {/* Header row */}
                <Box sx={{ display: 'flex', gap: 0.5, ml: 8 }}>
                  <Typography variant="caption" sx={{ width: 60, textAlign: 'center' }}>
                    High
                  </Typography>
                  <Typography variant="caption" sx={{ width: 60, textAlign: 'center' }}>
                    Medium
                  </Typography>
                  <Typography variant="caption" sx={{ width: 60, textAlign: 'center' }}>
                    Low
                  </Typography>
                </Box>

                {/* Impact rows */}
                {([1, 2, 3] as ImpactLevel[]).map((i) => (
                  <Box key={i} sx={{ display: 'flex', gap: 0.5, alignItems: 'center' }}>
                    <Typography
                      variant="caption"
                      sx={{ width: 60, textAlign: 'right', pr: 1 }}
                    >
                      {i === 1 ? 'High' : i === 2 ? 'Medium' : 'Low'}
                    </Typography>
                    {([1, 2, 3] as UrgencyLevel[]).map((u) => (
                      <MatrixCell
                        key={`${i}-${u}`}
                        impact={i}
                        urgency={u}
                        isSelected={impact === i && urgency === u}
                        onClick={() => handleCellClick(i, u)}
                        readOnly={readOnly}
                      />
                    ))}
                  </Box>
                ))}

                {/* X-axis label */}
                <Typography
                  variant="caption"
                  sx={{ textAlign: 'center', mt: 1, ml: 8 }}
                >
                  Urgency
                </Typography>
              </Box>
            </Box>
          </Box>
        </Box>
      )}
    </Paper>
  );
};

export default ImpactUrgencyMatrix;

/**
 * Escalation Hierarchy Viewer
 *
 * Visualizes escalation rules as a stepped hierarchy showing the escalation
 * chain: trigger conditions, delay times, and target assignments at each level.
 */
import React from 'react';
import {
  Box,
  Paper,
  Typography,
  Chip,
  Stepper,
  Step,
  StepLabel,
  StepContent,
  Avatar,
} from '@mui/material';
import PersonIcon from '@mui/icons-material/Person';
import GroupIcon from '@mui/icons-material/Group';
import TimerIcon from '@mui/icons-material/Timer';
import type { EscalationRuleDto } from '../../services/escalationService';

interface EscalationHierarchyViewerProps {
  rules: EscalationRuleDto[];
  title?: string;
}

const priorityColor = (priority: string): 'error' | 'warning' | 'info' | 'default' => {
  switch (priority?.toLowerCase()) {
    case 'critical': return 'error';
    case 'high': return 'warning';
    case 'medium': return 'info';
    default: return 'default';
  }
};

const targetIcon = (targetType: string) => {
  return targetType?.toLowerCase() === 'group' ? <GroupIcon /> : <PersonIcon />;
};

export const EscalationHierarchyViewer: React.FC<EscalationHierarchyViewerProps> = ({
  rules,
  title = 'Escalation Hierarchy',
}) => {
  // Sort by delay time to show escalation order
  const sorted = [...rules]
    .filter(r => r.isActive)
    .sort((a, b) => a.escalationDelayMinutes - b.escalationDelayMinutes);

  if (sorted.length === 0) {
    return (
      <Paper variant="outlined" sx={{ p: 3 }}>
        <Typography variant="h6" fontWeight={600} gutterBottom>{title}</Typography>
        <Typography color="text.secondary">No active escalation rules configured.</Typography>
      </Paper>
    );
  }

  return (
    <Paper variant="outlined" sx={{ p: 3 }}>
      <Typography variant="h6" fontWeight={600} gutterBottom>{title}</Typography>

      <Stepper orientation="vertical" activeStep={-1}>
        {sorted.map((rule, index) => (
          <Step key={rule.id} active expanded>
            <StepLabel
              icon={
                <Avatar
                  sx={{
                    width: 32,
                    height: 32,
                    fontSize: 14,
                    bgcolor: index === 0
                      ? 'primary.main'
                      : index === sorted.length - 1
                        ? 'error.main'
                        : 'warning.main',
                  }}
                >
                  L{index + 1}
                </Avatar>
              }
            >
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
                <Typography fontWeight={600}>{rule.name}</Typography>
                <Chip label={rule.priority} size="small" color={priorityColor(rule.priority)} />
              </Box>
            </StepLabel>
            <StepContent>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5, pl: 1 }}>
                {rule.description && (
                  <Typography variant="body2" color="text.secondary">
                    {rule.description}
                  </Typography>
                )}
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 0.5 }}>
                  <TimerIcon fontSize="small" color="action" />
                  <Typography variant="body2">
                    Escalates after <strong>{rule.escalationDelayMinutes} min</strong>
                  </Typography>
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  {targetIcon(rule.targetType)}
                  <Typography variant="body2">
                    Target: <strong>{rule.targetName || `${rule.targetType} #${rule.targetId}`}</strong>
                  </Typography>
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <Typography variant="caption" color="text.secondary">
                    Condition: {rule.conditionType} = {rule.conditionValue}
                  </Typography>
                </Box>
              </Box>
            </StepContent>
          </Step>
        ))}
      </Stepper>
    </Paper>
  );
};

export default EscalationHierarchyViewer;

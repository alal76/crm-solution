import React, { useState, useMemo } from 'react';
import {
  Box,
  Button,
  Chip,
  CircularProgress,
  Divider,
  FormControlLabel,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Paper,
  Switch,
  Typography,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import WarningIcon from '@mui/icons-material/Warning';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import CancelIcon from '@mui/icons-material/Cancel';
import type { EscalationRuleDto } from '../../services/escalationService';

// ---------------------------------------------------------------------------
// Props
// ---------------------------------------------------------------------------

export interface EscalationRulesPanelProps {
  rules: EscalationRuleDto[];
  onRuleClick?: (ruleId: number) => void;
  onCreateRule?: () => void;
  compact?: boolean;
  maxDisplay?: number;
  loading?: boolean;
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

const priorityColor = (priority: string): 'error' | 'warning' | 'info' | 'default' => {
  switch (priority) {
    case 'Critical':
      return 'error';
    case 'High':
      return 'warning';
    case 'Medium':
      return 'info';
    default:
      return 'default';
  }
};

// ===========================================================================
// Component
// ===========================================================================

const EscalationRulesPanel: React.FC<EscalationRulesPanelProps> = ({
  rules,
  onRuleClick,
  onCreateRule,
  compact = false,
  maxDisplay,
  loading = false,
}) => {
  const [showActiveOnly, setShowActiveOnly] = useState(false);

  const filteredRules = useMemo(() => {
    let result = showActiveOnly ? rules.filter(r => r.isActive) : rules;
    if (maxDisplay !== undefined && maxDisplay > 0) {
      result = result.slice(0, maxDisplay);
    }
    return result;
  }, [rules, showActiveOnly, maxDisplay]);

  // ---- Loading state ----
  if (loading) {
    return (
      <Paper variant="outlined" sx={{ p: 3, textAlign: 'center' }}>
        <CircularProgress size={28} />
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
          Loading escalation rules…
        </Typography>
      </Paper>
    );
  }

  // ---- Empty state ----
  if (rules.length === 0) {
    return (
      <Paper variant="outlined" sx={{ p: 3, textAlign: 'center' }}>
        <WarningIcon color="disabled" sx={{ fontSize: 40, mb: 1 }} />
        <Typography color="text.secondary" sx={{ mb: 1 }}>
          No escalation rules available.
        </Typography>
        {onCreateRule && (
          <Button size="small" startIcon={<AddIcon />} onClick={onCreateRule}>
            Create Rule
          </Button>
        )}
      </Paper>
    );
  }

  return (
    <Paper variant="outlined" sx={{ overflow: 'hidden' }}>
      {/* Header */}
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          px: 2,
          py: 1.5,
          bgcolor: 'grey.50',
        }}
      >
        <Typography variant="subtitle2" fontWeight="bold">
          Escalation Rules ({rules.length})
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={showActiveOnly}
                onChange={e => setShowActiveOnly(e.target.checked)}
              />
            }
            label={<Typography variant="caption">Active only</Typography>}
            sx={{ mr: 0 }}
          />
          {onCreateRule && (
            <Button size="small" startIcon={<AddIcon />} onClick={onCreateRule}>
              Create
            </Button>
          )}
        </Box>
      </Box>

      <Divider />

      {/* Rule List */}
      <List dense disablePadding>
        {filteredRules.length === 0 ? (
          <ListItem>
            <ListItemText
              primary={
                <Typography variant="body2" color="text.secondary" align="center">
                  No matching rules.
                </Typography>
              }
            />
          </ListItem>
        ) : (
          filteredRules.map(rule => {
            const content = (
              <>
                <ListItemIcon sx={{ minWidth: 32 }}>
                  {rule.isActive ? (
                    <CheckCircleIcon fontSize="small" color="success" />
                  ) : (
                    <CancelIcon fontSize="small" color="disabled" />
                  )}
                </ListItemIcon>
                <ListItemText
                  primary={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Typography variant="body2" fontWeight={500} noWrap>
                        {rule.name}
                      </Typography>
                      <Chip
                        label={rule.priority}
                        color={priorityColor(rule.priority)}
                        size="small"
                        sx={{ height: 20, fontSize: '0.7rem' }}
                      />
                    </Box>
                  }
                  secondary={
                    compact ? undefined : (
                      <Box component="span" sx={{ display: 'block', mt: 0.5 }}>
                        <Typography variant="caption" color="text.secondary" component="span">
                          {rule.conditionType}: {rule.conditionValue}
                        </Typography>
                        <Typography variant="caption" color="text.secondary" component="span" sx={{ ml: 1 }}>
                          → {rule.targetType}{rule.targetName ? `: ${rule.targetName}` : ''}
                        </Typography>
                        <Typography variant="caption" color="text.secondary" component="span" sx={{ ml: 1 }}>
                          (delay: {rule.escalationDelayMinutes}m)
                        </Typography>
                      </Box>
                    )
                  }
                />
              </>
            );

            if (onRuleClick) {
              return (
                <ListItemButton
                  key={rule.id}
                  onClick={() => onRuleClick(rule.id)}
                  sx={{ py: compact ? 0.5 : 1 }}
                >
                  {content}
                </ListItemButton>
              );
            }
            return (
              <ListItem key={rule.id} sx={{ py: compact ? 0.5 : 1 }}>
                {content}
              </ListItem>
            );
          })
        )}
      </List>

      {/* Truncation notice */}
      {maxDisplay !== undefined && maxDisplay > 0 && rules.length > maxDisplay && (
        <>
          <Divider />
          <Box sx={{ px: 2, py: 1, textAlign: 'center' }}>
            <Typography variant="caption" color="text.secondary">
              Showing {maxDisplay} of {rules.length} rules
            </Typography>
          </Box>
        </>
      )}
    </Paper>
  );
};

export default EscalationRulesPanel;

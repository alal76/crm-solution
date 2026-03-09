// CRM Solution - Customer Relationship Management System
// FEAT-AISCORING: AI Lead Scoring Real-time Triggers — Score explanation side-drawer
import React, { useEffect, useState } from 'react';
import {
  Drawer,
  Box,
  Typography,
  IconButton,
  Divider,
  LinearProgress,
  Table,
  TableBody,
  TableRow,
  TableCell,
  Chip,
  CircularProgress,
  Alert,
  List,
  ListItem,
  ListItemText,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';
import TrendingFlatIcon from '@mui/icons-material/TrendingFlat';
import {
  getScoreExplanation,
  LeadScoreExplanation,
  ScoreComponents,
} from '../../services/leadScoreService';
import LeadScoreHistoryChart from './LeadScoreHistoryChart';

interface Props {
  leadId: number | null;
  open: boolean;
  onClose: () => void;
}

const getBarColor = (score: number | undefined): 'success' | 'warning' | 'error' => {
  if (score == null) return 'warning';
  if (score >= 70) return 'success';
  if (score >= 40) return 'warning';
  return 'error';
};

const COMPONENT_LABELS: [keyof ScoreComponents, string][] = [
  ['fit', 'Overall Fit'],
  ['engagement', 'Engagement'],
  ['budget', 'Budget (B)'],
  ['authority', 'Authority (A)'],
  ['need', 'Need (N)'],
  ['timeline', 'Timeline (T)'],
  ['metrics', 'Metrics (M)'],
  ['economicBuyer', 'Economic Buyer (E)'],
  ['decisionCriteria', 'Decision Criteria (D)'],
  ['decisionProcess', 'Decision Process (D)'],
  ['identifyPain', 'Identify Pain (I)'],
  ['champion', 'Champion (C)'],
];

const trendConfig = {
  improving: {
    label: 'Improving',
    color: '#2e7d32',
    bgcolor: '#e8f5e9',
    icon: <TrendingUpIcon fontSize="small" />,
  },
  declining: {
    label: 'Declining',
    color: '#c62828',
    bgcolor: '#ffebee',
    icon: <TrendingDownIcon fontSize="small" />,
  },
  stable: {
    label: 'Stable',
    color: '#616161',
    bgcolor: '#f5f5f5',
    icon: <TrendingFlatIcon fontSize="small" />,
  },
};

const formatDelta = (delta: number): string => (delta > 0 ? `+${delta}` : `${delta}`);
const formatDate = (iso: string): string =>
  new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });

const LeadScoreExplanationDrawer: React.FC<Props> = ({ leadId, open, onClose }) => {
  const [data, setData] = useState<LeadScoreExplanation | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!open || !leadId) {
      setData(null);
      return;
    }
    setLoading(true);
    setError(null);
    getScoreExplanation(leadId)
      .then(d => {
        setData(d);
        setLoading(false);
      })
      .catch(err => {
        setError((err as Error)?.message ?? 'Failed to load score explanation');
        setLoading(false);
      });
  }, [open, leadId]);

  const trend = data?.trend ?? 'stable';
  const tc = trendConfig[trend];

  return (
    <Drawer
      anchor="right"
      open={open}
      onClose={onClose}
      PaperProps={{ sx: { width: 400, p: 0 } }}
    >
      {/* Header */}
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          px: 2,
          py: 1.5,
          bgcolor: '#1976d2',
          color: 'white',
        }}
      >
        <Typography variant="h6" fontWeight={700} fontSize="0.95rem">
          Score Analysis
        </Typography>
        <IconButton size="small" onClick={onClose} sx={{ color: 'white' }}>
          <CloseIcon fontSize="small" />
        </IconButton>
      </Box>

      <Box sx={{ overflowY: 'auto', px: 2, pt: 2, pb: 4 }}>
        {loading && (
          <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
            <CircularProgress />
          </Box>
        )}

        {error && <Alert severity="error">{error}</Alert>}

        {data && !loading && (
          <>
            {/* Current score + trend */}
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
              <Box
                sx={{
                  width: 56,
                  height: 56,
                  borderRadius: '50%',
                  bgcolor: '#e3f2fd',
                  border: '3px solid #1976d2',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontWeight: 800,
                  fontSize: '1.1rem',
                  color: '#1976d2',
                }}
              >
                {data.currentScore}
              </Box>
              <Box>
                <Typography variant="body2" color="text.secondary">
                  Qualification: <strong>{data.qualificationFramework}</strong>
                </Typography>
                <Chip
                  icon={tc.icon}
                  label={tc.label}
                  size="small"
                  sx={{ bgcolor: tc.bgcolor, color: tc.color, fontWeight: 600, mt: 0.5 }}
                />
              </Box>
            </Box>

            <Divider sx={{ mb: 1.5 }} />

            {/* Score history chart */}
            {leadId && (
              <>
                <Typography variant="subtitle2" fontWeight={700} gutterBottom>
                  Score History
                </Typography>
                <LeadScoreHistoryChart leadId={leadId} />
                <Divider sx={{ my: 1.5 }} />
              </>
            )}

            {/* Component breakdown */}
            <Typography variant="subtitle2" fontWeight={700} gutterBottom>
              Score Components
            </Typography>
            <Table size="small" sx={{ mb: 1 }}>
              <TableBody>
                {COMPONENT_LABELS.map(([key, label]) => {
                  const val = data.components[key];
                  if (val == null) return null;
                  return (
                    <TableRow key={key} sx={{ '&:last-child td': { border: 0 } }}>
                      <TableCell sx={{ py: 0.5, pr: 1, width: '45%', fontSize: '0.75rem' }}>
                        {label}
                      </TableCell>
                      <TableCell sx={{ py: 0.5, width: '15%', fontSize: '0.75rem', fontWeight: 700 }}>
                        {val}
                      </TableCell>
                      <TableCell sx={{ py: 0.5 }}>
                        <LinearProgress
                          variant="determinate"
                          value={Math.min(100, val)}
                          color={getBarColor(val)}
                          sx={{ height: 6, borderRadius: 3 }}
                        />
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>

            <Divider sx={{ my: 1.5 }} />

            {/* Recent history list */}
            {data.recentHistory.length > 0 && (
              <>
                <Typography variant="subtitle2" fontWeight={700} gutterBottom>
                  Recent Changes
                </Typography>
                <List dense disablePadding>
                  {data.recentHistory.map(h => (
                    <ListItem
                      key={h.id}
                      disableGutters
                      sx={{
                        py: 0.25,
                        borderBottom: '1px solid #f0f0f0',
                      }}
                    >
                      <ListItemText
                        primary={
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Typography
                              variant="caption"
                              fontWeight={700}
                              sx={{
                                color:
                                  h.delta > 0 ? '#2e7d32' : h.delta < 0 ? '#c62828' : '#616161',
                              }}
                            >
                              {h.delta > 0 ? '↑' : h.delta < 0 ? '↓' : '→'}
                              {formatDelta(h.delta)}
                            </Typography>
                            <Typography variant="caption" sx={{ fontWeight: 600 }}>
                              {h.score}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              {h.reason}
                            </Typography>
                          </Box>
                        }
                        secondary={
                          <Typography variant="caption" color="text.secondary" sx={{ fontSize: '0.68rem' }}>
                            {formatDate(h.scoredAt)} · by {h.scoredBy}
                          </Typography>
                        }
                      />
                    </ListItem>
                  ))}
                </List>
              </>
            )}
          </>
        )}
      </Box>
    </Drawer>
  );
};

export default LeadScoreExplanationDrawer;

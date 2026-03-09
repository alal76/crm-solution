import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Grid,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import ListIcon from '@mui/icons-material/List';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import escalationService, { EscalationRuleDto } from '../../services/escalationService';
import { EscalationHierarchyViewer } from '../../components/itsm/EscalationHierarchyViewer';

interface PriorityRow {
  priority: string;
  total: number;
  active: number;
}

const EscalationDashboardPage: React.FC = () => {
  const navigate = useNavigate();
  const [rules, setRules] = useState<EscalationRuleDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadRules = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await escalationService.getAll();
      setRules(data.items ?? []);
    } catch (err) {
      console.error('Failed to load escalation rules', err);
      setError('Failed to load escalation data. The escalation rules API may not be available yet.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadRules();
  }, [loadRules]);

  const activeCount = rules.filter(r => r.isActive).length;
  const inactiveCount = rules.length - activeCount;

  const priorityRows: PriorityRow[] = ['Critical', 'High', 'Medium', 'Low'].map((priority) => ({
    priority,
    total: rules.filter(r => r.priority === priority).length,
    active: rules.filter(r => r.priority === priority && r.isActive).length,
  }));

  const summaryCards = [
    { label: 'Total Rules', value: rules.length, color: 'primary.main', subtitle: 'configured' },
    { label: 'Active Rules', value: activeCount, color: 'success.main', subtitle: 'currently enabled' },
    { label: 'Inactive Rules', value: inactiveCount, color: 'text.secondary', subtitle: 'disabled' },
    { label: 'Active Ratio', value: `${rules.length > 0 ? Math.round((activeCount / rules.length) * 100) : 0}%`, color: 'warning.main', subtitle: 'of rules are active' },
  ];

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h4" component="h1" fontWeight="bold">
            Escalation Dashboard
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
            Monitor active escalations and rule performance
          </Typography>
        </Box>
        <TrendingUpIcon sx={{ fontSize: 40, color: 'primary.light' }} />
      </Box>

      {error && (
        <Alert severity="warning" sx={{ mb: 3 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Summary Cards */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        {summaryCards.map((card) => (
          <Grid item xs={12} sm={6} md={3} key={card.label}>
            <Card variant="outlined" sx={{ height: '100%' }}>
              <CardContent sx={{ textAlign: 'center', py: 2.5 }}>
                <Typography variant="h3" fontWeight="bold" sx={{ color: card.color }}>
                  {card.value}
                </Typography>
                <Typography variant="body1" fontWeight={500} sx={{ mt: 0.5 }}>
                  {card.label}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {card.subtitle}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Grid container spacing={3}>
        {/* Priority Distribution */}
        <Grid item xs={12} md={4}>
          <Paper variant="outlined" sx={{ p: 2, height: '100%' }}>
            <Typography variant="h6" fontWeight={600} sx={{ mb: 2 }}>
              Rules by Priority
            </Typography>
            {loading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', py: 3 }}>
                <CircularProgress size={24} />
              </Box>
            ) : (
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell><strong>Priority</strong></TableCell>
                      <TableCell align="center"><strong>Total</strong></TableCell>
                      <TableCell align="center"><strong>Active</strong></TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {priorityRows.map((row) => (
                      <TableRow key={row.priority} hover>
                        <TableCell>
                          <Chip
                            label={row.priority}
                            size="small"
                            color={
                              row.priority === 'Critical' ? 'error' :
                              row.priority === 'High' ? 'warning' :
                              row.priority === 'Medium' ? 'info' : 'default'
                            }
                          />
                        </TableCell>
                        <TableCell align="center">{row.total}</TableCell>
                        <TableCell align="center">
                          <Typography color="success.main" fontWeight={500}>
                            {row.active}
                          </Typography>
                        </TableCell>
                      </TableRow>
                    ))}
                    {rules.length === 0 && !loading && (
                      <TableRow>
                        <TableCell colSpan={3} align="center">
                          <Typography variant="caption" color="text.secondary">
                            No rules configured yet
                          </Typography>
                        </TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </Paper>
        </Grid>

        {/* Recent Events */}
        <Grid item xs={12} md={8}>
          <Paper variant="outlined" sx={{ p: 2 }}>
            <Typography variant="h6" fontWeight={600} sx={{ mb: 2 }}>
              Recent Escalation Events
            </Typography>
            <Alert severity="info">
              Real-time escalation event tracking is coming soon. Once the escalation events API is available,
              triggered escalations — including which rule fired, the target, and current status — will appear here.
            </Alert>
          </Paper>
        </Grid>
      </Grid>

      {/* Escalation Hierarchy Visualization */}
      {!loading && rules.length > 0 && (
        <Box sx={{ mt: 3 }}>
          <EscalationHierarchyViewer rules={rules} title="Escalation Chain" />
        </Box>
      )}

      {/* Quick Actions */}
      <Paper variant="outlined" sx={{ p: 2, mt: 3 }}>
        <Typography variant="h6" fontWeight={600} sx={{ mb: 2 }}>
          Quick Actions
        </Typography>
        <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
          <Button
            variant="outlined"
            startIcon={<ListIcon />}
            onClick={() => navigate('/itsm/escalation/rules')}
          >
            View All Rules
          </Button>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => navigate('/itsm/escalation/rules')}
          >
            Create New Rule
          </Button>
        </Box>
      </Paper>
    </Box>
  );
};

export default EscalationDashboardPage;

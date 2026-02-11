import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Button from '@mui/material/Button';
import Grid from '@mui/material/Grid';
import CircularProgress from '@mui/material/CircularProgress';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import apiClient from '../../services/apiClient';
import { SLACountdownWidget, SLABreachBanner } from '../../components/itsm';
import type { SLAInstanceData, SLABreachInfo } from '../../components/itsm';

interface SLAPolicySummary {
  slaPolicyId: number;
}

interface SLAInstanceSummary {
  slaInstanceId: number;
  targetId?: number;
  targetType?: number;
  responseBreached: boolean;
  resolutionBreached: boolean;
  responseDueAt?: string;
  resolutionDueAt?: string;
}

const SLADashboardPage: React.FC = () => {
  const navigate = useNavigate();
  const [policyCount, setPolicyCount] = useState(0);
  const [breachedCount, setBreachedCount] = useState(0);
  const [breaches, setBreaches] = useState<SLAInstanceSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [slaInstances, setSlaInstances] = useState<SLAInstanceData[]>([]);
  const [slaBreachInfos, setSlaBreachInfos] = useState<SLABreachInfo[]>([]);

  useEffect(() => {
    const load = async () => {
      try {
        const [policiesResp, breachedResp, instancesResp, breachInfoResp] = await Promise.allSettled([
          apiClient.get('/api/sla/policies'),
          apiClient.get('/api/sla/breached'),
          apiClient.get('/api/sla/instances/active'),
          apiClient.get('/api/sla/breach-alerts'),
        ]);
        if (policiesResp.status === 'fulfilled') {
          const policies: SLAPolicySummary[] = policiesResp.value.data ?? [];
          setPolicyCount(policies.length);
        }
        if (breachedResp.status === 'fulfilled') {
          const breachData: SLAInstanceSummary[] = breachedResp.value.data ?? [];
          setBreachedCount(breachData.length);
          setBreaches(breachData);
        }
        if (instancesResp.status === 'fulfilled') setSlaInstances(instancesResp.value.data ?? []);
        if (breachInfoResp.status === 'fulfilled') setSlaBreachInfos(breachInfoResp.value.data ?? []);
      } catch (error) {
        console.error('Failed to load SLA dashboard', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1" fontWeight="bold">SLA Dashboard</Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button variant="outlined" onClick={() => navigate('/itsm/sla/policies')}>Policies</Button>
          <Button variant="contained" onClick={() => navigate('/itsm/sla/instances')}>Instances</Button>
        </Box>
      </Box>

      <Grid container spacing={3}>
        <Grid item xs={12} lg={4}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" fontWeight="bold" gutterBottom>Active SLAs</Typography>
            <Typography variant="h4" fontWeight="bold" color="primary.main">{loading ? '—' : policyCount}</Typography>
          </Paper>
        </Grid>
        <Grid item xs={12} lg={4}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" fontWeight="bold" gutterBottom>Breached</Typography>
            <Typography variant="h4" fontWeight="bold" color="error.main">{loading ? '—' : breachedCount}</Typography>
          </Paper>
        </Grid>
        <Grid item xs={12} lg={4}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" fontWeight="bold" gutterBottom>On Track</Typography>
            <Typography variant="h4" fontWeight="bold" color="success.main">{loading ? '—' : Math.max(policyCount - breachedCount, 0)}</Typography>
          </Paper>
        </Grid>
      </Grid>

      {/* SLA Breach Banner */}
      {slaBreachInfos.length > 0 && (
        <Box sx={{ mt: 3 }}>
          <SLABreachBanner breaches={slaBreachInfos} maxDisplay={5} />
        </Box>
      )}

      {/* SLA Countdown Timers */}
      {slaInstances.length > 0 && (
        <Box sx={{ mt: 3 }}>
          <SLACountdownWidget slaInstances={slaInstances} showDetails />
        </Box>
      )}

      <Paper sx={{ mt: 3, p: 3 }}>
        <Typography variant="h6" fontWeight="bold" sx={{ mb: 2 }}>Breached SLA Instances</Typography>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>
        ) : breaches.length === 0 ? (
          <Typography color="text.secondary">No breached SLA instances.</Typography>
        ) : (
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell><strong>Target</strong></TableCell>
                  <TableCell><strong>Response Due</strong></TableCell>
                  <TableCell><strong>Resolution Due</strong></TableCell>
                  <TableCell><strong>Breaches</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {breaches.map((item) => (
                  <TableRow key={item.slaInstanceId} hover>
                    <TableCell>{item.targetType ?? '—'} / {item.targetId ?? '—'}</TableCell>
                    <TableCell>{item.responseDueAt ? new Date(item.responseDueAt).toLocaleString() : '—'}</TableCell>
                    <TableCell>{item.resolutionDueAt ? new Date(item.resolutionDueAt).toLocaleString() : '—'}</TableCell>
                    <TableCell>
                      {item.responseBreached ? 'Response' : ''}
                      {item.responseBreached && item.resolutionBreached ? ' & ' : ''}
                      {item.resolutionBreached ? 'Resolution' : ''}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </Paper>
    </Box>
  );
};

export default SLADashboardPage;

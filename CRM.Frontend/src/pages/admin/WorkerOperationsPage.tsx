import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  Grid,
  LinearProgress,
  Paper,
  TextField,
  Typography,
} from '@mui/material';
import {
  PowerSettingsNew as PowerIcon,
  RestartAlt as RestartIcon,
  PlayArrow as StartIcon,
  Tune as TuneIcon,
  Speed as SpeedIcon,
  CloudQueue as QueueIcon,
  ErrorOutline as ErrorIcon,
  Verified as VerifiedIcon,
  AccessTime as TimeIcon,
} from '@mui/icons-material';
import workerAdminService, {
  WorkerControlStatus,
  WorkerHealthStatus,
  WorkerQueueStats,
} from '../../services/workerAdminService';

const formatAge = (seconds?: number | null) => {
  if (!seconds && seconds !== 0) return 'N/A';
  if (seconds < 60) return `${Math.round(seconds)}s`;
  if (seconds < 3600) return `${Math.round(seconds / 60)}m`;
  return `${Math.round(seconds / 3600)}h`;
};

const WorkerOperationsPage: React.FC = () => {
  const [health, setHealth] = useState<WorkerHealthStatus | null>(null);
  const [stats, setStats] = useState<WorkerQueueStats | null>(null);
  const [control, setControl] = useState<WorkerControlStatus | null>(null);
  const [maxWorkers, setMaxWorkers] = useState(1);
  const [loading, setLoading] = useState(false);
  const [action, setAction] = useState<'start' | 'stop' | 'restart' | null>(null);
  const [error, setError] = useState<string | null>(null);

  const loadAll = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const [healthResponse, statsResponse, controlResponse] = await Promise.all([
        workerAdminService.getHealth(),
        workerAdminService.getStats(),
        workerAdminService.getControlStatus(),
      ]);

      setHealth(healthResponse);
      setStats(statsResponse);
      setControl(controlResponse);
      setMaxWorkers(controlResponse.maxWorkers);
    } catch (err) {
      setError('Failed to load worker status. Please retry.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadAll();
  }, [loadAll]);

  const statusTone = useMemo(() => {
    if (!health?.status) return 'default';
    return health.status === 'healthy' ? 'success' : 'warning';
  }, [health?.status]);

  const handleMaxWorkersSave = async () => {
    const value = Math.max(1, Number(maxWorkers) || 1);
    setMaxWorkers(value);
    setLoading(true);
    setError(null);

    try {
      const updated = await workerAdminService.updateMaxWorkers(value);
      setControl(updated);
    } catch (err) {
      setError('Unable to update max workers.');
    } finally {
      setLoading(false);
    }
  };

  const handleControlAction = async () => {
    if (!action) return;
    setLoading(true);
    setError(null);

    try {
      if (action === 'start') {
        setControl(await workerAdminService.startWorkers());
      }
      if (action === 'stop') {
        setControl(await workerAdminService.stopWorkers());
      }
      if (action === 'restart') {
        setControl(await workerAdminService.restartWorkers());
      }
      await loadAll();
    } catch (err) {
      setError('Worker control request failed.');
    } finally {
      setLoading(false);
      setAction(null);
    }
  };

  return (
    <Box
      sx={{
        fontFamily: '"Space Grotesk", "IBM Plex Sans", sans-serif',
        position: 'relative',
        minHeight: 'calc(100vh - 120px)',
        px: { xs: 2, md: 4 },
        py: { xs: 3, md: 4 },
        overflow: 'hidden',
        '--worker-accent': '#ff6a00',
        '--worker-ink': '#1a1a1a',
        '--worker-panel': '#ffffff',
        '--worker-glow': 'rgba(255, 106, 0, 0.2)',
        background: 'linear-gradient(135deg, #fef2e4 0%, #f6f6ff 45%, #eef7ff 100%)',
        '&::before': {
          content: '""',
          position: 'absolute',
          top: '-120px',
          right: '-140px',
          width: '320px',
          height: '320px',
          background: 'radial-gradient(circle, rgba(255,106,0,0.35) 0%, rgba(255,106,0,0) 70%)',
          zIndex: 0,
        },
        '&::after': {
          content: '""',
          position: 'absolute',
          bottom: '-160px',
          left: '-120px',
          width: '360px',
          height: '360px',
          background: 'radial-gradient(circle, rgba(88,132,255,0.25) 0%, rgba(88,132,255,0) 70%)',
          zIndex: 0,
        },
      }}
    >
      <Box sx={{ position: 'relative', zIndex: 1 }}>
        <Box
          sx={{
            display: 'flex',
            flexDirection: { xs: 'column', md: 'row' },
            alignItems: { xs: 'flex-start', md: 'center' },
            justifyContent: 'space-between',
            gap: 2,
            mb: 3,
          }}
        >
          <Box>
            <Typography
              variant="h3"
              sx={{
                fontFamily: '"Fraunces", "Space Grotesk", serif',
                fontWeight: 700,
                color: 'var(--worker-ink)',
                letterSpacing: '-0.02em',
              }}
            >
              Worker Operations
            </Typography>
            <Typography variant="body1" sx={{ mt: 1, color: 'rgba(26,26,26,0.7)', maxWidth: 560 }}>
              Monitor queue health, manage worker lifecycle, and tune scale limits from a single command surface.
            </Typography>
          </Box>
          <Button
            variant="contained"
            onClick={loadAll}
            sx={{
              background: 'var(--worker-accent)',
              color: '#fff',
              boxShadow: '0 10px 30px rgba(255,106,0,0.25)',
              textTransform: 'none',
              fontWeight: 600,
              px: 3,
              '&:hover': { background: '#e85f00' },
            }}
          >
            Refresh Status
          </Button>
        </Box>

        {loading && <LinearProgress sx={{ mb: 3 }} />}
        {error && (
          <Paper
            sx={{
              mb: 3,
              p: 2,
              borderRadius: 2,
              border: '1px solid rgba(255, 106, 0, 0.3)',
              background: 'rgba(255, 248, 240, 0.9)',
            }}
          >
            <Typography sx={{ color: '#b34700', fontWeight: 600 }}>{error}</Typography>
          </Paper>
        )}

        <Grid container spacing={3}>
          <Grid item xs={12} md={5}>
            <Paper
              sx={{
                p: 3,
                borderRadius: 3,
                background: 'var(--worker-panel)',
                boxShadow: '0 16px 45px rgba(16, 24, 40, 0.12)',
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                <SpeedIcon sx={{ color: 'var(--worker-accent)' }} />
                <Typography variant="h6" sx={{ fontWeight: 700 }}>
                  Control Room
                </Typography>
              </Box>
              <Divider sx={{ mb: 2 }} />

              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                  <Typography variant="body2" sx={{ color: 'text.secondary' }}>
                    Health
                  </Typography>
                  <Chip
                    icon={health?.status === 'healthy' ? <VerifiedIcon /> : <ErrorIcon />}
                    label={health?.status ?? 'unknown'}
                    color={statusTone}
                    size="small"
                  />
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                  <Typography variant="body2" sx={{ color: 'text.secondary' }}>
                    Control State
                  </Typography>
                  <Chip label={control?.controlState ?? 'unknown'} size="small" />
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                  <Typography variant="body2" sx={{ color: 'text.secondary' }}>
                    Max Workers
                  </Typography>
                  <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                    {control?.maxWorkers ?? 1}
                  </Typography>
                </Box>
              </Box>

              <Box sx={{ display: 'flex', gap: 1.5, mt: 3, flexWrap: 'wrap' }}>
                <Button
                  variant="outlined"
                  startIcon={<StartIcon />}
                  onClick={() => setAction('start')}
                  sx={{ textTransform: 'none', borderColor: 'rgba(16, 185, 129, 0.6)', color: '#0f766e' }}
                >
                  Start
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<PowerIcon />}
                  onClick={() => setAction('stop')}
                  sx={{ textTransform: 'none', borderColor: 'rgba(239, 68, 68, 0.6)', color: '#b91c1c' }}
                >
                  Stop
                </Button>
                <Button
                  variant="contained"
                  startIcon={<RestartIcon />}
                  onClick={() => setAction('restart')}
                  sx={{
                    textTransform: 'none',
                    background: '#111827',
                    '&:hover': { background: '#0b1220' },
                  }}
                >
                  Restart
                </Button>
              </Box>
            </Paper>
          </Grid>

          <Grid item xs={12} md={7}>
            <Paper
              sx={{
                p: 3,
                borderRadius: 3,
                background: 'linear-gradient(145deg, rgba(255,255,255,0.95) 0%, rgba(246,248,255,0.92) 100%)',
                boxShadow: '0 18px 48px rgba(31, 41, 55, 0.15)',
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                <TuneIcon sx={{ color: 'var(--worker-accent)' }} />
                <Typography variant="h6" sx={{ fontWeight: 700 }}>
                  Scale Limits
                </Typography>
              </Box>
              <Divider sx={{ mb: 2 }} />
              <Typography variant="body2" sx={{ color: 'text.secondary', mb: 2 }}>
                Limit the number of worker instances that can be scheduled. Minimum value is 1.
              </Typography>
              <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', flexWrap: 'wrap' }}>
                <TextField
                  type="number"
                  value={maxWorkers}
                  onChange={(event) => setMaxWorkers(Number(event.target.value))}
                  inputProps={{ min: 1 }}
                  size="small"
                  sx={{ width: 140 }}
                />
                <Button
                  variant="contained"
                  onClick={handleMaxWorkersSave}
                  sx={{
                    background: 'var(--worker-accent)',
                    color: '#fff',
                    textTransform: 'none',
                    fontWeight: 600,
                    '&:hover': { background: '#e85f00' },
                  }}
                >
                  Save Limit
                </Button>
              </Box>
            </Paper>
          </Grid>

          <Grid item xs={12}>
            <Paper
              sx={{
                p: 3,
                borderRadius: 3,
                background: 'var(--worker-panel)',
                boxShadow: '0 18px 48px rgba(31, 41, 55, 0.1)',
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                <QueueIcon sx={{ color: 'var(--worker-accent)' }} />
                <Typography variant="h6" sx={{ fontWeight: 700 }}>
                  Queue Snapshot
                </Typography>
              </Box>
              <Divider sx={{ mb: 2 }} />

              <Grid container spacing={2}>
                {[
                  { label: 'Queued', value: stats?.jobs.queued ?? 0 },
                  { label: 'In Progress', value: stats?.jobs.inProgress ?? 0 },
                  { label: 'Completed', value: stats?.jobs.completed ?? 0 },
                  { label: 'Failed', value: stats?.jobs.failed ?? 0 },
                  { label: 'Dead Lettered', value: stats?.jobs.deadLettered ?? 0 },
                  { label: 'Total Jobs', value: stats?.jobs.total ?? 0 },
                ].map((item) => (
                  <Grid item xs={6} md={2} key={item.label}>
                    <Paper
                      sx={{
                        p: 2,
                        borderRadius: 2,
                        background: 'linear-gradient(140deg, #fff 0%, #f6f6ff 100%)',
                        border: '1px solid rgba(15, 23, 42, 0.05)',
                      }}
                    >
                      <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                        {item.label}
                      </Typography>
                      <Typography variant="h6" sx={{ fontWeight: 700, mt: 0.5 }}>
                        {item.value}
                      </Typography>
                    </Paper>
                  </Grid>
                ))}
              </Grid>

              <Divider sx={{ my: 3 }} />

              <Grid container spacing={2}>
                {[
                  { label: 'Outbox Pending', value: stats?.outbox.pending ?? 0 },
                  { label: 'Outbox Processing', value: stats?.outbox.processing ?? 0 },
                  { label: 'Outbox Completed', value: stats?.outbox.completed ?? 0 },
                  { label: 'Outbox Failed', value: stats?.outbox.failed ?? 0 },
                  { label: 'Outbox Total', value: stats?.outbox.total ?? 0 },
                ].map((item) => (
                  <Grid item xs={6} md={2.4} key={item.label}>
                    <Paper
                      sx={{
                        p: 2,
                        borderRadius: 2,
                        background: 'linear-gradient(140deg, #fff 0%, #f2f8ff 100%)',
                        border: '1px solid rgba(15, 23, 42, 0.05)',
                      }}
                    >
                      <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                        {item.label}
                      </Typography>
                      <Typography variant="h6" sx={{ fontWeight: 700, mt: 0.5 }}>
                        {item.value}
                      </Typography>
                    </Paper>
                  </Grid>
                ))}
              </Grid>

              <Divider sx={{ my: 3 }} />

              <Grid container spacing={2}>
                {[
                  { label: 'Oldest Queued', value: formatAge(stats?.metrics.oldestQueuedAgeSeconds) },
                  { label: 'Oldest Outbox Pending', value: formatAge(stats?.metrics.oldestPendingOutboxAgeSeconds) },
                ].map((item) => (
                  <Grid item xs={12} md={6} key={item.label}>
                    <Paper
                      sx={{
                        p: 2,
                        borderRadius: 2,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between',
                        border: '1px solid rgba(15, 23, 42, 0.08)',
                      }}
                    >
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <TimeIcon sx={{ color: 'var(--worker-accent)' }} />
                        <Typography variant="body2" sx={{ color: 'text.secondary' }}>
                          {item.label}
                        </Typography>
                      </Box>
                      <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
                        {item.value}
                      </Typography>
                    </Paper>
                  </Grid>
                ))}
              </Grid>
            </Paper>
          </Grid>
        </Grid>
      </Box>

      <Dialog open={!!action} onClose={() => setAction(null)}>
        <DialogTitle>Confirm action</DialogTitle>
        <DialogContent>
          <Typography>
            {action === 'stop'
              ? 'This will stop all worker processes. Continue?'
              : action === 'restart'
              ? 'This will restart worker processes. Continue?'
              : 'This will start worker processes. Continue?'}
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAction(null)}>Cancel</Button>
          <Button variant="contained" onClick={handleControlAction}>
            Confirm
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default WorkerOperationsPage;

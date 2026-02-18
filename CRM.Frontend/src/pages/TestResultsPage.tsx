import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Chip,
  Container,
  Grid,
  LinearProgress,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
  Collapse,
  IconButton,
} from '@mui/material';
import {
  KeyboardArrowDown as KeyboardArrowDownIcon,
  KeyboardArrowUp as KeyboardArrowUpIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Warning as WarningIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import { format, formatDistanceToNow } from 'date-fns';

interface TestResult {
  sessionId: string;
  testName: string;
  className: string;
  status: 'Passed' | 'Failed' | 'Skipped';
  duration: string;
  message?: string;
  exceptionType?: string;
  stackTrace?: string;
  timestamp: string;
}

interface TestRunSummary {
  sessionId: string;
  startTime: string;
  endTime: string;
  totalTests: number;
  passedTests: number;
  failedTests: number;
  skippedTests: number;
  totalDuration: string;
  passRate: number;
  results: TestResult[];
}

interface TestResultRowProps {
  result: TestResult;
}

const TestResultRow: React.FC<TestResultRowProps> = ({ result }) => {
  const [open, setOpen] = useState(false);

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Passed':
        return <CheckCircleIcon sx={{ color: '#4caf50', mr: 1 }} />;
      case 'Failed':
        return <ErrorIcon sx={{ color: '#f44336', mr: 1 }} />;
      case 'Skipped':
        return <WarningIcon sx={{ color: '#ff9800', mr: 1 }} />;
      default:
        return null;
    }
  };

  const getStatusColor = (status: string): 'success' | 'error' | 'warning' | 'default' => {
    switch (status) {
      case 'Passed':
        return 'success';
      case 'Failed':
        return 'error';
      case 'Skipped':
        return 'warning';
      default:
        return 'default';
    }
  };

  const parseDuration = (duration: string): number => {
    return parseFloat(duration.replace('PT', '').replace('S', '')) * 1000;
  };

  return (
    <>
      <TableRow sx={{
        backgroundColor: result.status === 'Failed' ? '#ffebee' : result.status === 'Skipped' ? '#fff3e0' : 'inherit',
      }}>
        <TableCell>
          <IconButton
            size="small"
            onClick={() => setOpen(!open)}
            disabled={!result.message && !result.stackTrace}
          >
            {open ? <KeyboardArrowUpIcon /> : <KeyboardArrowDownIcon />}
          </IconButton>
        </TableCell>
        <TableCell>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            {getStatusIcon(result.status)}
            <Typography variant="body2" sx={{ fontFamily: 'monospace', fontSize: '0.85rem' }}>
              {result.testName.split('.').pop()}
            </Typography>
          </Box>
        </TableCell>
        <TableCell sx={{ fontSize: '0.85rem', color: '#666' }}>
          {result.className}
        </TableCell>
        <TableCell>
          <Chip
            label={result.status}
            size="small"
            color={getStatusColor(result.status)}
            variant="outlined"
          />
        </TableCell>
        <TableCell align="right" sx={{ fontSize: '0.85rem' }}>
          {parseDuration(result.duration).toFixed(0)}ms
        </TableCell>
        <TableCell sx={{ fontSize: '0.75rem', color: '#999' }}>
          {format(new Date(result.timestamp), 'HH:mm:ss')}
        </TableCell>
      </TableRow>
      {(result.message || result.stackTrace) && (
        <TableRow>
          <TableCell colSpan={6}>
            <Collapse in={open} timeout="auto" unmountOnExit>
              <Box sx={{ p: 2, backgroundColor: '#f5f5f5', fontFamily: 'monospace', fontSize: '0.75rem' }}>
                {result.exceptionType && (
                  <Typography variant="caption" display="block" sx={{ color: '#d32f2f', fontWeight: 'bold', mb: 1 }}>
                    {result.exceptionType}
                  </Typography>
                )}
                {result.message && (
                  <Typography variant="caption" display="block" sx={{ mb: 1, whiteSpace: 'pre-wrap' }}>
                    {result.message}
                  </Typography>
                )}
                {result.stackTrace && (
                  <Box sx={{ mt: 1, pt: 1, borderTop: '1px solid #ddd', color: '#666' }}>
                    <Typography variant="caption" display="block" sx={{ whiteSpace: 'pre-wrap', overflow: 'auto', maxHeight: '300px' }}>
                      {result.stackTrace}
                    </Typography>
                  </Box>
                )}
              </Box>
            </Collapse>
          </TableCell>
        </TableRow>
      )}
    </>
  );
};

export const TestResultsPage: React.FC = () => {
  const [summary, setSummary] = useState<TestRunSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [filterStatus, setFilterStatus] = useState<string | null>(null);

  useEffect(() => {
    loadTestResults();
    // Auto-refresh every 30 seconds
    const interval = setInterval(loadTestResults, 30000);
    return () => clearInterval(interval);
  }, []);

  const loadTestResults = async () => {
    try {
      // Fetch the latest test results JSON from the API
      const response = await fetch('/api/test-results/latest');
      if (response.ok) {
        const data = await response.json();
        setSummary(data);
      }
    } catch (error) {
      console.error('Failed to load test results:', error);
    } finally {
      setLoading(false);
    }
  };

  const filteredResults = summary?.results.filter(r =>
    !filterStatus || r.status === filterStatus
  ) || [];

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
        <Typography variant="h4" gutterBottom>
          Test Results Dashboard
        </Typography>
        <IconButton onClick={loadTestResults} disabled={loading}>
          <RefreshIcon />
        </IconButton>
      </Box>

      {loading && <LinearProgress />}

      {summary && (
        <>
          {/* Summary Cards */}
          <Grid container spacing={2} sx={{ mb: 4 }}>
            <Grid item xs={12} sm={6} md={3}>
              <Card>
                <CardContent>
                  <Typography color="textSecondary" gutterBottom>
                    Total Tests
                  </Typography>
                  <Typography variant="h5">
                    {summary.totalTests}
                  </Typography>
                  <Typography variant="caption" color="textSecondary">
                    {format(new Date(summary.startTime), 'yyyy-MM-dd HH:mm:ss')}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} sm={6} md={3}>
              <Card>
                <CardContent>
                  <Typography color="textSecondary" gutterBottom>
                    Passed
                  </Typography>
                  <Typography variant="h5" sx={{ color: '#4caf50' }}>
                    {summary.passedTests}
                  </Typography>
                  <Box sx={{ mt: 1 }}>
                    <LinearProgress variant="determinate" value={summary.passRate} />
                  </Box>
                  <Typography variant="caption" color="textSecondary">
                    {summary.passRate.toFixed(1)}% pass rate
                  </Typography>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} sm={6} md={3}>
              <Card>
                <CardContent>
                  <Typography color="textSecondary" gutterBottom>
                    Failed
                  </Typography>
                  <Typography variant="h5" sx={{ color: '#f44336' }}>
                    {summary.failedTests}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} sm={6} md={3}>
              <Card>
                <CardContent>
                  <Typography color="textSecondary" gutterBottom>
                    Skipped
                  </Typography>
                  <Typography variant="h5" sx={{ color: '#ff9800' }}>
                    {summary.skippedTests}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
          </Grid>

          {/* Filter Controls */}
          <Paper sx={{ p: 2, mb: 2 }}>
            <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
              <Chip
                label={`All (${summary.totalTests})`}
                onClick={() => setFilterStatus(null)}
                variant={filterStatus === null ? 'filled' : 'outlined'}
                color={filterStatus === null ? 'primary' : 'default'}
              />
              <Chip
                label={`Passed (${summary.passedTests})`}
                onClick={() => setFilterStatus('Passed')}
                variant={filterStatus === 'Passed' ? 'filled' : 'outlined'}
                color={filterStatus === 'Passed' ? 'success' : 'default'}
              />
              <Chip
                label={`Failed (${summary.failedTests})`}
                onClick={() => setFilterStatus('Failed')}
                variant={filterStatus === 'Failed' ? 'filled' : 'outlined'}
                color={filterStatus === 'Failed' ? 'error' : 'default'}
              />
              <Chip
                label={`Skipped (${summary.skippedTests})`}
                onClick={() => setFilterStatus('Skipped')}
                variant={filterStatus === 'Skipped' ? 'filled' : 'outlined'}
                color={filterStatus === 'Skipped' ? 'warning' : 'default'}
              />
            </Box>
          </Paper>

          {/* Results Table */}
          <TableContainer component={Paper}>
            <Table size="small">
              <TableHead sx={{ backgroundColor: '#f5f5f5' }}>
                <TableRow>
                  <TableCell width="40"></TableCell>
                  <TableCell>Test Name</TableCell>
                  <TableCell>Class</TableCell>
                  <TableCell width="100">Status</TableCell>
                  <TableCell align="right" width="80">Duration</TableCell>
                  <TableCell width="80">Time</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredResults.length > 0 ? (
                  filteredResults.map((result, idx) => (
                    <TestResultRow key={idx} result={result} />
                  ))
                ) : (
                  <TableRow>
                    <TableCell colSpan={6} align="center" sx={{ py: 4, color: '#999' }}>
                      No test results found.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>

          {/* Footer */}
          <Box sx={{ mt: 4, textAlign: 'center', color: '#999', fontSize: '0.85rem' }}>
            <Typography variant="caption">
              Session ID: {summary.sessionId}
            </Typography>
            <Typography variant="caption" display="block">
              Duration: {formatDistanceToNow(new Date(summary.startTime), { addSuffix: false })}
            </Typography>
          </Box>
        </>
      )}
    </Container>
  );
};

export default TestResultsPage;

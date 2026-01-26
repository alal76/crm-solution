import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Card,
  CardContent,
  Chip,
  LinearProgress,
  Divider,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Alert,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Button,
  CircularProgress,
} from '@mui/material';
import {
  CheckCircle as PassIcon,
  Cancel as FailIcon,
  SkipNext as SkipIcon,
  ExpandMore as ExpandMoreIcon,
  Refresh as RefreshIcon,
  PlayArrow as RunIcon,
} from '@mui/icons-material';
import apiClient from '../../services/apiClient';

interface TestResult {
  name: string;
  status: 'passed' | 'failed' | 'skipped';
  duration: string;
  category: string;
  errorMessage?: string;
  stackTrace?: string;
}

interface TestSummary {
  total: number;
  passed: number;
  failed: number;
  skipped: number;
  duration: string;
  timestamp: string;
  testCategories: {
    [key: string]: {
      total: number;
      passed: number;
      failed: number;
      skipped: number;
    };
  };
}

interface TestRun {
  id: string;
  type: 'backend' | 'frontend';
  summary: TestSummary;
  results: TestResult[];
  status: 'running' | 'completed' | 'failed';
}

// Static test results data (would normally come from API)
const staticTestResults: TestRun[] = [
  {
    id: 'backend-latest',
    type: 'backend',
    status: 'completed',
    summary: {
      total: 708,
      passed: 692,
      failed: 8,
      skipped: 8,
      duration: '35.95s',
      timestamp: new Date().toISOString(),
      testCategories: {
        'Unit': { total: 450, passed: 446, failed: 4, skipped: 0 },
        'BVT': { total: 50, passed: 50, failed: 0, skipped: 0 },
        'Functional': { total: 150, passed: 142, failed: 4, skipped: 4 },
        'Performance': { total: 8, passed: 4, failed: 0, skipped: 4 },
        'Services': { total: 50, passed: 50, failed: 0, skipped: 0 },
      },
    },
    results: [
      { name: 'Lead_FullName_WithEmptyFirstName', status: 'failed', duration: '2ms', category: 'Unit', errorMessage: 'Expected lead.FullName to be " Doe" but got "Doe"' },
      { name: 'Department_Description_IsOptional', status: 'failed', duration: '1ms', category: 'Unit', errorMessage: 'Expected department.Description to be null, but found ""' },
      { name: 'SystemSettings_FeatureFlags_DefaultValues', status: 'failed', duration: '55ms', category: 'Unit', errorMessage: 'Expected settings.CustomersEnabled to be false, but found True' },
      { name: 'Product_CanBeCreated_WithDefaults', status: 'failed', duration: '40ms', category: 'Unit', errorMessage: 'Expected product.Name not to be empty' },
      { name: 'FT041_Get_Workflow_Definitions_Should_Return_List', status: 'failed', duration: '591ms', category: 'Functional', errorMessage: 'Expected success but got NotFound' },
      { name: 'FT042_Get_Workflow_Instances_Should_Return_List', status: 'failed', duration: '604ms', category: 'Functional', errorMessage: 'Expected success but got NotFound' },
      { name: 'FT043_Get_Workflow_Tasks_Should_Return_List', status: 'failed', duration: '567ms', category: 'Functional', errorMessage: 'Expected success but got NotFound' },
      { name: 'FT062_Get_System_Settings_Should_Return_Settings', status: 'failed', duration: '883ms', category: 'Functional', errorMessage: 'Expected success but got InternalServerError' },
    ],
  },
  {
    id: 'frontend-latest',
    type: 'frontend',
    status: 'completed',
    summary: {
      total: 16,
      passed: 16,
      failed: 0,
      skipped: 0,
      duration: '45.2s',
      timestamp: new Date().toISOString(),
      testCategories: {
        'Components': { total: 6, passed: 6, failed: 0, skipped: 0 },
        'Pages': { total: 8, passed: 8, failed: 0, skipped: 0 },
        'Services': { total: 2, passed: 2, failed: 0, skipped: 0 },
      },
    },
    results: [],
  },
];

const TestResultsPage: React.FC = () => {
  const [testRuns, setTestRuns] = useState<TestRun[]>(staticTestResults);
  const [loading, setLoading] = useState(false);
  const [runningTests, setRunningTests] = useState<string | null>(null);

  const getPassRate = (summary: TestSummary) => {
    return summary.total > 0 ? ((summary.passed / summary.total) * 100).toFixed(1) : '0';
  };

  const getStatusColor = (status: 'passed' | 'failed' | 'skipped') => {
    switch (status) {
      case 'passed':
        return 'success';
      case 'failed':
        return 'error';
      case 'skipped':
        return 'warning';
      default:
        return 'default';
    }
  };

  const getStatusIcon = (status: 'passed' | 'failed' | 'skipped') => {
    switch (status) {
      case 'passed':
        return <PassIcon color="success" />;
      case 'failed':
        return <FailIcon color="error" />;
      case 'skipped':
        return <SkipIcon color="warning" />;
    }
  };

  const handleRefresh = async () => {
    setLoading(true);
    // In a real implementation, this would fetch from an API
    setTimeout(() => {
      setTestRuns(staticTestResults);
      setLoading(false);
    }, 1000);
  };

  const handleRunTests = async (type: string) => {
    setRunningTests(type);
    // In a real implementation, this would trigger test execution via API
    setTimeout(() => {
      setRunningTests(null);
    }, 5000);
  };

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1">
          Test Results Dashboard
        </Typography>
        <Box>
          <Button
            startIcon={<RefreshIcon />}
            onClick={handleRefresh}
            disabled={loading}
            sx={{ mr: 1 }}
          >
            Refresh
          </Button>
          <Button
            variant="contained"
            color="primary"
            startIcon={runningTests ? <CircularProgress size={20} color="inherit" /> : <RunIcon />}
            onClick={() => handleRunTests('all')}
            disabled={!!runningTests}
          >
            {runningTests ? 'Running...' : 'Run All Tests'}
          </Button>
        </Box>
      </Box>

      {/* Summary Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        {testRuns.map((run) => (
          <Grid item xs={12} md={6} key={run.id}>
            <Card elevation={3}>
              <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                  <Typography variant="h6" component="h2">
                    {run.type === 'backend' ? '🔧 Backend Tests' : '🎨 Frontend Tests'}
                  </Typography>
                  <Chip
                    label={run.status === 'completed' ? 'Completed' : 'Running'}
                    color={run.status === 'completed' ? 'success' : 'info'}
                    size="small"
                  />
                </Box>

                <Box sx={{ mb: 2 }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                    <Typography variant="body2" color="text.secondary">
                      Pass Rate: {getPassRate(run.summary)}%
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Duration: {run.summary.duration}
                    </Typography>
                  </Box>
                  <LinearProgress
                    variant="determinate"
                    value={Number(getPassRate(run.summary))}
                    color={run.summary.failed > 0 ? 'error' : 'success'}
                    sx={{ height: 10, borderRadius: 5 }}
                  />
                </Box>

                <Grid container spacing={2}>
                  <Grid item xs={3}>
                    <Box sx={{ textAlign: 'center' }}>
                      <Typography variant="h5" color="text.primary">
                        {run.summary.total}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        Total
                      </Typography>
                    </Box>
                  </Grid>
                  <Grid item xs={3}>
                    <Box sx={{ textAlign: 'center' }}>
                      <Typography variant="h5" color="success.main">
                        {run.summary.passed}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        Passed
                      </Typography>
                    </Box>
                  </Grid>
                  <Grid item xs={3}>
                    <Box sx={{ textAlign: 'center' }}>
                      <Typography variant="h5" color="error.main">
                        {run.summary.failed}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        Failed
                      </Typography>
                    </Box>
                  </Grid>
                  <Grid item xs={3}>
                    <Box sx={{ textAlign: 'center' }}>
                      <Typography variant="h5" color="warning.main">
                        {run.summary.skipped}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        Skipped
                      </Typography>
                    </Box>
                  </Grid>
                </Grid>

                <Divider sx={{ my: 2 }} />

                <Typography variant="subtitle2" gutterBottom>
                  Test Categories
                </Typography>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                  {Object.entries(run.summary.testCategories).map(([category, stats]) => (
                    <Chip
                      key={category}
                      label={`${category}: ${stats.passed}/${stats.total}`}
                      size="small"
                      color={stats.failed > 0 ? 'error' : 'success'}
                      variant="outlined"
                    />
                  ))}
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      {/* Failed Tests Details */}
      <Paper elevation={2} sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Failed Tests Details
        </Typography>
        
        {testRuns.some(run => run.results.filter(r => r.status === 'failed').length > 0) ? (
          testRuns.map((run) => (
            run.results.filter(r => r.status === 'failed').length > 0 && (
              <Box key={run.id} sx={{ mb: 3 }}>
                <Typography variant="subtitle1" color="error" gutterBottom>
                  {run.type === 'backend' ? 'Backend' : 'Frontend'} - {run.results.filter(r => r.status === 'failed').length} Failed
                </Typography>
                
                {run.results
                  .filter(r => r.status === 'failed')
                  .map((result, index) => (
                    <Accordion key={index}>
                      <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, width: '100%' }}>
                          <FailIcon color="error" />
                          <Typography sx={{ flexGrow: 1 }}>{result.name}</Typography>
                          <Chip label={result.category} size="small" variant="outlined" />
                          <Typography variant="caption" color="text.secondary">
                            {result.duration}
                          </Typography>
                        </Box>
                      </AccordionSummary>
                      <AccordionDetails>
                        <Alert severity="error" sx={{ mb: 2 }}>
                          {result.errorMessage}
                        </Alert>
                        {result.stackTrace && (
                          <Box sx={{ bgcolor: 'grey.100', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: '12px', overflow: 'auto' }}>
                            <pre style={{ margin: 0, whiteSpace: 'pre-wrap' }}>{result.stackTrace}</pre>
                          </Box>
                        )}
                      </AccordionDetails>
                    </Accordion>
                  ))}
              </Box>
            )
          ))
        ) : (
          <Alert severity="success">All tests passed! 🎉</Alert>
        )}
      </Paper>

      {/* Test Execution Summary */}
      <Paper elevation={2} sx={{ p: 3, mt: 3 }}>
        <Typography variant="h6" gutterBottom>
          Test Execution Summary
        </Typography>
        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Test Suite</TableCell>
                <TableCell align="right">Total</TableCell>
                <TableCell align="right">Passed</TableCell>
                <TableCell align="right">Failed</TableCell>
                <TableCell align="right">Skipped</TableCell>
                <TableCell align="right">Pass Rate</TableCell>
                <TableCell align="right">Duration</TableCell>
                <TableCell>Status</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {testRuns.map((run) => (
                <TableRow key={run.id}>
                  <TableCell>
                    {run.type === 'backend' ? 'Backend (.NET)' : 'Frontend (Jest)'}
                  </TableCell>
                  <TableCell align="right">{run.summary.total}</TableCell>
                  <TableCell align="right" sx={{ color: 'success.main' }}>
                    {run.summary.passed}
                  </TableCell>
                  <TableCell align="right" sx={{ color: run.summary.failed > 0 ? 'error.main' : 'inherit' }}>
                    {run.summary.failed}
                  </TableCell>
                  <TableCell align="right" sx={{ color: run.summary.skipped > 0 ? 'warning.main' : 'inherit' }}>
                    {run.summary.skipped}
                  </TableCell>
                  <TableCell align="right">{getPassRate(run.summary)}%</TableCell>
                  <TableCell align="right">{run.summary.duration}</TableCell>
                  <TableCell>
                    <Chip
                      icon={run.summary.failed === 0 ? <PassIcon /> : <FailIcon />}
                      label={run.summary.failed === 0 ? 'Pass' : 'Fail'}
                      color={run.summary.failed === 0 ? 'success' : 'error'}
                      size="small"
                    />
                  </TableCell>
                </TableRow>
              ))}
              <TableRow sx={{ bgcolor: 'grey.100' }}>
                <TableCell><strong>Total</strong></TableCell>
                <TableCell align="right">
                  <strong>{testRuns.reduce((sum, run) => sum + run.summary.total, 0)}</strong>
                </TableCell>
                <TableCell align="right" sx={{ color: 'success.main' }}>
                  <strong>{testRuns.reduce((sum, run) => sum + run.summary.passed, 0)}</strong>
                </TableCell>
                <TableCell align="right" sx={{ color: 'error.main' }}>
                  <strong>{testRuns.reduce((sum, run) => sum + run.summary.failed, 0)}</strong>
                </TableCell>
                <TableCell align="right" sx={{ color: 'warning.main' }}>
                  <strong>{testRuns.reduce((sum, run) => sum + run.summary.skipped, 0)}</strong>
                </TableCell>
                <TableCell align="right">
                  <strong>
                    {(
                      (testRuns.reduce((sum, run) => sum + run.summary.passed, 0) /
                        testRuns.reduce((sum, run) => sum + run.summary.total, 0)) *
                      100
                    ).toFixed(1)}%
                  </strong>
                </TableCell>
                <TableCell align="right">-</TableCell>
                <TableCell></TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>

      {/* Deployment Status */}
      <Paper elevation={2} sx={{ p: 3, mt: 3 }}>
        <Typography variant="h6" gutterBottom>
          Deployment Status
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12} md={6}>
            <Alert severity="success" icon={<PassIcon />}>
              <strong>Local Kubernetes:</strong> Deployed successfully
              <br />
              <Typography variant="caption">
                crm-backend:v34, crm-frontend:v42 running in crm-app namespace
              </Typography>
            </Alert>
          </Grid>
          <Grid item xs={12} md={6}>
            <Alert severity="info" icon={<RefreshIcon />}>
              <strong>Build Server (192.168.0.9):</strong> Build in progress
              <br />
              <Typography variant="caption">
                Building crm-backend:v35, crm-frontend:v43
              </Typography>
            </Alert>
          </Grid>
        </Grid>
      </Paper>
    </Box>
  );
};

export default TestResultsPage;

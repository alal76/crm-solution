/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * AI Analytics Dashboard - Cost tracking and performance analytics for AI workflow nodes
 */

import React, { useState, useEffect, useMemo } from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Card,
  CardContent,
  CardHeader,
  Divider,
  Chip,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  CircularProgress,
  Alert,
  Tooltip,
  IconButton,
  Button,
  LinearProgress,
  Avatar,
  List,
  ListItem,
  ListItemText,
  ListItemAvatar,
  ListItemSecondaryAction,
} from '@mui/material';
import {
  AttachMoney as CostIcon,
  Speed as SpeedIcon,
  Token as TokenIcon,
  Psychology as AIIcon,
  TrendingUp as TrendingUpIcon,
  TrendingDown as TrendingDownIcon,
  Warning as WarningIcon,
  CheckCircle as SuccessIcon,
  Error as ErrorIcon,
  Refresh as RefreshIcon,
  Download as DownloadIcon,
  SmartToy as AgentIcon,
  AutoAwesome as ContentIcon,
  DataObject as ExtractorIcon,
  Category as ClassifierIcon,
  SentimentSatisfied as SentimentIcon,
  Route as DecisionIcon,
  RateReview as ReviewIcon,
  Schedule as ScheduleIcon,
} from '@mui/icons-material';
import { AINodeExecution, AIAnalyticsSummary } from '../../services/workflowService';

// ============================================================================
// Types
// ============================================================================

interface AIAnalyticsDashboardProps {
  workflowId?: number;
  dateRange?: 'today' | 'week' | 'month' | 'quarter' | 'year';
  onExport?: () => void;
}

interface ModelStats {
  model: string;
  cost: number;
  tokens: number;
  executions: number;
  successRate: number;
  avgLatency: number;
}

interface NodeTypeStats {
  nodeType: string;
  cost: number;
  tokens: number;
  executions: number;
  successRate: number;
}

// ============================================================================
// Mock Data (Replace with actual API calls)
// ============================================================================

const mockExecutions: AINodeExecution[] = [
  { nodeId: 1, nodeType: 'AIDecision', model: 'gpt-4o', inputTokens: 500, outputTokens: 100, totalTokens: 600, cost: 0.0054, latencyMs: 1200, success: true, timestamp: new Date().toISOString() },
  { nodeId: 2, nodeType: 'AIContentGenerator', model: 'gpt-4o', inputTokens: 800, outputTokens: 1500, totalTokens: 2300, cost: 0.0265, latencyMs: 3500, success: true, timestamp: new Date().toISOString() },
  { nodeId: 3, nodeType: 'AIAgent', model: 'gpt-4o', inputTokens: 2000, outputTokens: 800, totalTokens: 2800, cost: 0.022, latencyMs: 8500, success: true, timestamp: new Date().toISOString() },
  { nodeId: 4, nodeType: 'AIClassifier', model: 'gpt-4o-mini', inputTokens: 200, outputTokens: 50, totalTokens: 250, cost: 0.00025, latencyMs: 400, success: true, timestamp: new Date().toISOString() },
  { nodeId: 5, nodeType: 'AISentimentAnalyzer', model: 'gpt-4o-mini', inputTokens: 150, outputTokens: 80, totalTokens: 230, cost: 0.00023, latencyMs: 350, success: true, timestamp: new Date().toISOString() },
  { nodeId: 6, nodeType: 'AIDataExtractor', model: 'gpt-4o', inputTokens: 1200, outputTokens: 300, totalTokens: 1500, cost: 0.0105, latencyMs: 2100, success: false, errorMessage: 'Invalid JSON response', timestamp: new Date().toISOString() },
];

const mockSummary: AIAnalyticsSummary = {
  period: 'month',
  totalCost: 45.67,
  totalTokens: 1250000,
  totalExecutions: 3456,
  successRate: 97.8,
  averageLatencyMs: 1850,
  byModel: [
    { model: 'gpt-4o', cost: 38.50, tokens: 850000, executions: 2100 },
    { model: 'gpt-4o-mini', cost: 5.20, tokens: 320000, executions: 1100 },
    { model: 'claude-3-sonnet', cost: 1.97, tokens: 80000, executions: 256 },
  ],
  byNodeType: [
    { nodeType: 'AIContentGenerator', cost: 18.45, tokens: 520000, executions: 890 },
    { nodeType: 'AIAgent', cost: 12.30, tokens: 380000, executions: 345 },
    { nodeType: 'AIDecision', cost: 8.90, tokens: 200000, executions: 1200 },
    { nodeType: 'AIClassifier', cost: 3.20, tokens: 90000, executions: 780 },
    { nodeType: 'AISentimentAnalyzer', cost: 1.82, tokens: 40000, executions: 180 },
    { nodeType: 'AIDataExtractor', cost: 1.00, tokens: 20000, executions: 61 },
  ],
};

// ============================================================================
// Helper Components
// ============================================================================

interface StatCardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  icon: React.ReactNode;
  trend?: number;
  trendLabel?: string;
  color?: string;
}

const StatCard: React.FC<StatCardProps> = ({
  title,
  value,
  subtitle,
  icon,
  trend,
  trendLabel,
  color = 'primary.main',
}) => (
  <Card elevation={0} sx={{ border: 1, borderColor: 'divider', height: '100%' }}>
    <CardContent>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <Box>
          <Typography variant="body2" color="text.secondary" gutterBottom>
            {title}
          </Typography>
          <Typography variant="h4" fontWeight="bold" sx={{ color }}>
            {value}
          </Typography>
          {subtitle && (
            <Typography variant="caption" color="text.secondary">
              {subtitle}
            </Typography>
          )}
        </Box>
        <Avatar sx={{ bgcolor: `${color}15`, color }}>
          {icon}
        </Avatar>
      </Box>
      {trend !== undefined && (
        <Box sx={{ display: 'flex', alignItems: 'center', mt: 1, gap: 0.5 }}>
          {trend >= 0 ? (
            <TrendingUpIcon fontSize="small" color="success" />
          ) : (
            <TrendingDownIcon fontSize="small" color="error" />
          )}
          <Typography
            variant="caption"
            color={trend >= 0 ? 'success.main' : 'error.main'}
            fontWeight="medium"
          >
            {Math.abs(trend)}% {trendLabel || 'vs last period'}
          </Typography>
        </Box>
      )}
    </CardContent>
  </Card>
);

const getNodeTypeIcon = (nodeType: string): React.ReactNode => {
  switch (nodeType) {
    case 'AIDecision': return <DecisionIcon fontSize="small" />;
    case 'AIAgent': return <AgentIcon fontSize="small" />;
    case 'AIContentGenerator': return <ContentIcon fontSize="small" />;
    case 'AIDataExtractor': return <ExtractorIcon fontSize="small" />;
    case 'AIClassifier': return <ClassifierIcon fontSize="small" />;
    case 'AISentimentAnalyzer': return <SentimentIcon fontSize="small" />;
    case 'HumanReview': return <ReviewIcon fontSize="small" />;
    default: return <AIIcon fontSize="small" />;
  }
};

const getNodeTypeColor = (nodeType: string): string => {
  switch (nodeType) {
    case 'AIDecision': return '#00BCD4';
    case 'AIAgent': return '#673AB7';
    case 'AIContentGenerator': return '#3F51B5';
    case 'AIDataExtractor': return '#009688';
    case 'AIClassifier': return '#8BC34A';
    case 'AISentimentAnalyzer': return '#FFEB3B';
    case 'HumanReview': return '#FF5722';
    default: return '#9E9E9E';
  }
};

const formatCost = (cost: number): string => {
  if (cost >= 1000) {
    return `$${(cost / 1000).toFixed(1)}k`;
  }
  if (cost >= 1) {
    return `$${cost.toFixed(2)}`;
  }
  return `$${cost.toFixed(4)}`;
};

const formatTokens = (tokens: number): string => {
  if (tokens >= 1000000) {
    return `${(tokens / 1000000).toFixed(1)}M`;
  }
  if (tokens >= 1000) {
    return `${(tokens / 1000).toFixed(1)}k`;
  }
  return tokens.toString();
};

const formatLatency = (ms: number): string => {
  if (ms >= 1000) {
    return `${(ms / 1000).toFixed(1)}s`;
  }
  return `${ms}ms`;
};

// ============================================================================
// Main Component
// ============================================================================

export const AIAnalyticsDashboard: React.FC<AIAnalyticsDashboardProps> = ({
  workflowId,
  dateRange: initialDateRange = 'month',
  onExport,
}) => {
  const [dateRange, setDateRange] = useState(initialDateRange);
  const [loading, setLoading] = useState(false);
  const [summary, setSummary] = useState<AIAnalyticsSummary | null>(mockSummary);
  const [recentExecutions, setRecentExecutions] = useState<AINodeExecution[]>(mockExecutions);

  // In a real implementation, this would fetch data from the API
  useEffect(() => {
    setLoading(true);
    // Simulated API call
    setTimeout(() => {
      setSummary(mockSummary);
      setRecentExecutions(mockExecutions);
      setLoading(false);
    }, 500);
  }, [dateRange, workflowId]);

  const handleRefresh = () => {
    setLoading(true);
    setTimeout(() => {
      setLoading(false);
    }, 500);
  };

  if (loading && !summary) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', p: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!summary) {
    return (
      <Alert severity="info">No AI analytics data available for the selected period.</Alert>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h5" fontWeight="bold" gutterBottom>
            AI Analytics Dashboard
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Monitor AI node performance, costs, and usage across workflows
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel>Period</InputLabel>
            <Select
              value={dateRange}
              label="Period"
              onChange={(e) => setDateRange(e.target.value as any)}
            >
              <MenuItem value="today">Today</MenuItem>
              <MenuItem value="week">This Week</MenuItem>
              <MenuItem value="month">This Month</MenuItem>
              <MenuItem value="quarter">This Quarter</MenuItem>
              <MenuItem value="year">This Year</MenuItem>
            </Select>
          </FormControl>
          <Tooltip title="Refresh">
            <IconButton onClick={handleRefresh} disabled={loading}>
              <RefreshIcon />
            </IconButton>
          </Tooltip>
          {onExport && (
            <Button
              variant="outlined"
              startIcon={<DownloadIcon />}
              onClick={onExport}
            >
              Export
            </Button>
          )}
        </Box>
      </Box>

      {loading && <LinearProgress sx={{ mb: 2 }} />}

      {/* Key Metrics */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Total Cost"
            value={formatCost(summary.totalCost)}
            subtitle={`${summary.totalExecutions.toLocaleString()} executions`}
            icon={<CostIcon />}
            trend={-12}
            trendLabel="vs last month"
            color="#4CAF50"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Total Tokens"
            value={formatTokens(summary.totalTokens)}
            subtitle="Input + Output"
            icon={<TokenIcon />}
            trend={8}
            color="#2196F3"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Success Rate"
            value={`${summary.successRate}%`}
            subtitle="Successful executions"
            icon={<SuccessIcon />}
            color={summary.successRate >= 95 ? '#4CAF50' : summary.successRate >= 90 ? '#FF9800' : '#F44336'}
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Avg Latency"
            value={formatLatency(summary.averageLatencyMs)}
            subtitle="Average response time"
            icon={<SpeedIcon />}
            trend={-5}
            color="#9C27B0"
          />
        </Grid>
      </Grid>

      {/* Charts Row */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        {/* Cost by Model */}
        <Grid item xs={12} md={6}>
          <Card elevation={0} sx={{ border: 1, borderColor: 'divider', height: '100%' }}>
            <CardHeader
              title="Cost by Model"
              titleTypographyProps={{ variant: 'subtitle1', fontWeight: 'bold' }}
              action={
                <Chip
                  label={`${summary.byModel.length} models`}
                  size="small"
                  variant="outlined"
                />
              }
            />
            <CardContent>
              <List dense disablePadding>
                {summary.byModel.map((model, index) => {
                  const percentage = (model.cost / summary.totalCost) * 100;
                  return (
                    <ListItem key={model.model} disableGutters sx={{ mb: 1 }}>
                      <ListItemAvatar>
                        <Avatar
                          sx={{
                            bgcolor: index === 0 ? 'primary.light' : index === 1 ? 'secondary.light' : 'grey.300',
                            width: 32,
                            height: 32,
                          }}
                        >
                          <AIIcon fontSize="small" />
                        </Avatar>
                      </ListItemAvatar>
                      <ListItemText
                        primary={model.model}
                        secondary={
                          <Box sx={{ mt: 0.5 }}>
                            <LinearProgress
                              variant="determinate"
                              value={percentage}
                              sx={{ height: 6, borderRadius: 3, mb: 0.5 }}
                            />
                            <Typography variant="caption" color="text.secondary">
                              {formatTokens(model.tokens)} tokens • {model.executions.toLocaleString()} runs
                            </Typography>
                          </Box>
                        }
                      />
                      <ListItemSecondaryAction>
                        <Typography variant="body2" fontWeight="medium">
                          {formatCost(model.cost)}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          {percentage.toFixed(1)}%
                        </Typography>
                      </ListItemSecondaryAction>
                    </ListItem>
                  );
                })}
              </List>
            </CardContent>
          </Card>
        </Grid>

        {/* Cost by Node Type */}
        <Grid item xs={12} md={6}>
          <Card elevation={0} sx={{ border: 1, borderColor: 'divider', height: '100%' }}>
            <CardHeader
              title="Cost by Node Type"
              titleTypographyProps={{ variant: 'subtitle1', fontWeight: 'bold' }}
              action={
                <Chip
                  label={`${summary.byNodeType.length} types`}
                  size="small"
                  variant="outlined"
                />
              }
            />
            <CardContent>
              <List dense disablePadding>
                {summary.byNodeType.map((nodeType) => {
                  const percentage = (nodeType.cost / summary.totalCost) * 100;
                  const color = getNodeTypeColor(nodeType.nodeType);
                  return (
                    <ListItem key={nodeType.nodeType} disableGutters sx={{ mb: 1 }}>
                      <ListItemAvatar>
                        <Avatar sx={{ bgcolor: `${color}20`, color, width: 32, height: 32 }}>
                          {getNodeTypeIcon(nodeType.nodeType)}
                        </Avatar>
                      </ListItemAvatar>
                      <ListItemText
                        primary={nodeType.nodeType.replace('AI', 'AI ')}
                        secondary={
                          <Box sx={{ mt: 0.5 }}>
                            <LinearProgress
                              variant="determinate"
                              value={percentage}
                              sx={{
                                height: 6,
                                borderRadius: 3,
                                mb: 0.5,
                                '& .MuiLinearProgress-bar': { bgcolor: color },
                              }}
                            />
                            <Typography variant="caption" color="text.secondary">
                              {formatTokens(nodeType.tokens)} tokens • {nodeType.executions.toLocaleString()} runs
                            </Typography>
                          </Box>
                        }
                      />
                      <ListItemSecondaryAction>
                        <Typography variant="body2" fontWeight="medium">
                          {formatCost(nodeType.cost)}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          {percentage.toFixed(1)}%
                        </Typography>
                      </ListItemSecondaryAction>
                    </ListItem>
                  );
                })}
              </List>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Recent Executions */}
      <Card elevation={0} sx={{ border: 1, borderColor: 'divider' }}>
        <CardHeader
          title="Recent AI Executions"
          titleTypographyProps={{ variant: 'subtitle1', fontWeight: 'bold' }}
          action={
            <Button size="small">View All</Button>
          }
        />
        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Node Type</TableCell>
                <TableCell>Model</TableCell>
                <TableCell align="right">Tokens</TableCell>
                <TableCell align="right">Cost</TableCell>
                <TableCell align="right">Latency</TableCell>
                <TableCell align="center">Status</TableCell>
                <TableCell>Time</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {recentExecutions.map((execution, index) => (
                <TableRow key={index} hover>
                  <TableCell>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Avatar
                        sx={{
                          width: 28,
                          height: 28,
                          bgcolor: `${getNodeTypeColor(execution.nodeType)}20`,
                          color: getNodeTypeColor(execution.nodeType),
                        }}
                      >
                        {getNodeTypeIcon(execution.nodeType)}
                      </Avatar>
                      <Typography variant="body2">
                        {execution.nodeType.replace('AI', 'AI ')}
                      </Typography>
                    </Box>
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={execution.model}
                      size="small"
                      variant="outlined"
                    />
                  </TableCell>
                  <TableCell align="right">
                    <Tooltip title={`Input: ${execution.inputTokens} / Output: ${execution.outputTokens}`}>
                      <Typography variant="body2">
                        {formatTokens(execution.totalTokens)}
                      </Typography>
                    </Tooltip>
                  </TableCell>
                  <TableCell align="right">
                    <Typography variant="body2" fontWeight="medium">
                      {formatCost(execution.cost)}
                    </Typography>
                  </TableCell>
                  <TableCell align="right">
                    <Typography
                      variant="body2"
                      color={execution.latencyMs > 5000 ? 'warning.main' : 'text.primary'}
                    >
                      {formatLatency(execution.latencyMs)}
                    </Typography>
                  </TableCell>
                  <TableCell align="center">
                    {execution.success ? (
                      <Tooltip title="Success">
                        <SuccessIcon color="success" fontSize="small" />
                      </Tooltip>
                    ) : (
                      <Tooltip title={execution.errorMessage || 'Failed'}>
                        <ErrorIcon color="error" fontSize="small" />
                      </Tooltip>
                    )}
                  </TableCell>
                  <TableCell>
                    <Typography variant="caption" color="text.secondary">
                      {new Date(execution.timestamp).toLocaleTimeString()}
                    </Typography>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Card>

      {/* Cost Alerts */}
      {summary.totalCost > 40 && (
        <Alert
          severity="warning"
          icon={<WarningIcon />}
          sx={{ mt: 3 }}
          action={
            <Button color="inherit" size="small">
              Set Budget
            </Button>
          }
        >
          AI costs are approaching your monthly budget. Consider optimizing expensive workflows or using more cost-effective models.
        </Alert>
      )}
    </Box>
  );
};

export default AIAnalyticsDashboard;

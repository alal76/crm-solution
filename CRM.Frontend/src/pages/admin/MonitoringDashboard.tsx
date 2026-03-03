/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Monitoring Dashboard - Links to external monitoring tools
 * Supports both Docker and Kubernetes deployments
 */

import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Card,
  CardContent,
  CardActions,
  Typography,
  Grid,
  Button,
  Alert,
  Chip,
  IconButton,
  Tooltip,
  Paper,
  Divider,
  useTheme,
  CircularProgress,
  LinearProgress,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Tabs,
  Tab,
  Skeleton,
  Switch,
  FormControlLabel,
  Stack,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  ButtonGroup,
} from '@mui/material';
import {
  OpenInNew as OpenInNewIcon,
  Refresh as RefreshIcon,
  CheckCircle as HealthyIcon,
  Warning as WarningIcon,
  Error as ErrorIcon,
  Storage as DatabaseIcon,
  Memory as MemoryIcon,
  Dns as DnsIcon,
  Cloud as CloudIcon,
  Dashboard as DashboardIcon,
  Monitor as MonitorIcon,
  Speed as SpeedIcon,
  Inventory as ContainerIcon,
  Apps as KubernetesIcon,
  Api as ApiIcon,
  Web as WebIcon,
  Cached as CacheIcon,
  Search as SearchIcon,
  Analytics as AnalyticsIcon,
  ContentCopy as CopyIcon,
  Lock as LockIcon,
  Link as LinkIcon,
  SmartToy as AIIcon,
  Security as SecurityIcon,
  VpnKey as VpnKeyIcon,
  Sync as SyncIcon,
  PlayArrow as PlayArrowIcon,
  Science as ScienceIcon,
  ExpandMore as ExpandMoreIcon,
  Block as BlockIcon,
  CheckCircleOutline as PassIcon,
  CancelOutlined as FailIcon,
  SkipNext as SkipIcon,
} from '@mui/icons-material';

// Configuration for monitoring tools
const MONITORING_TOOLS = {
  uptimeKuma: {
    name: 'Uptime Kuma',
    description: 'Service health monitoring for all endpoints, APIs, and infrastructure',
    icon: <MonitorIcon sx={{ fontSize: 48 }} />,
    color: '#5CDD8B',
    features: ['HTTP/HTTPS monitoring', 'TCP/Ping checks', 'Docker container status', 'Kubernetes pod health', 'Status pages', 'Notifications'],
    defaultPort: 3001,
    path: '/',
  },
  portainer: {
    name: 'Portainer Agent',
    description: 'Docker container agent for remote management — connect via Portainer CE/EE',
    icon: <ContainerIcon sx={{ fontSize: 48 }} />,
    color: '#13B5EA',
    features: ['Remote container management', 'Docker API proxy', 'Volume management', 'Network management', 'Image management', 'Cluster endpoint'],
    defaultPort: 9001,
    path: '/',
  },
  superset: {
    name: 'Apache Superset',
    description: 'Business intelligence and data visualization platform with CRM dashboards',
    icon: <AnalyticsIcon sx={{ fontSize: 48 }} />,
    color: '#20A7C9',
    features: ['SQL Lab', 'Interactive dashboards', 'Chart builder', 'CRM data sources', 'Scheduled reports', 'Role-based access'],
    defaultPort: 8088,
    path: '/',
  },
};

// External resources configuration
const EXTERNAL_RESOURCES = [
  {
    name: 'Uptime Kuma',
    description: 'Service health monitoring dashboard',
    icon: <MonitorIcon />,
    color: '#5CDD8B',
    port: 3001,
    path: '/dashboard',
    credentials: { username: 'admin', authNote: '[see environment config]' },
    status: 'deployed' as const,
  },
  {
    name: 'Apache Superset',
    description: 'BI & data visualization with CRM dashboards',
    icon: <AnalyticsIcon />,
    color: '#20A7C9',
    port: 8088,
    path: '/superset/dashboard/crm-overview/',
    credentials: { username: 'admin', authNote: '[see environment config]' },
    status: 'deployed' as const,
  },
  {
    name: 'Meilisearch',
    description: 'Full-text search engine for CRM data',
    icon: <SearchIcon />,
    color: '#FF5CAA',
    port: 7700,
    path: '/',
    credentials: { apiKey: '[see environment config]' },
    status: 'deployed' as const,
  },
  {
    name: 'MariaDB',
    description: 'Primary CRM relational database',
    icon: <DatabaseIcon />,
    color: '#003545',
    port: 3306,
    credentials: { username: 'crm_user', authNote: '[see environment config]', database: 'crm_db' },
    status: 'deployed' as const,
  },
  {
    name: 'Redis',
    description: 'In-memory cache and session store',
    icon: <CacheIcon />,
    color: '#DC382D',
    port: 6379,
    credentials: {},
    status: 'deployed' as const,
  },
  {
    name: 'Portainer Agent',
    description: 'Docker container management endpoint',
    icon: <ContainerIcon />,
    color: '#13B5EA',
    port: 9001,
    path: '/',
    credentials: {},
    status: 'deployed' as const,
  },
  {
    name: 'CRM API',
    description: 'Backend REST API',
    icon: <ApiIcon />,
    color: '#512BD4',
    port: 5000,
    path: '/health',
    credentials: { email: 'admin@crm.local', authNote: '[see environment config]' },
    status: 'deployed' as const,
  },
  {
    name: 'CRM Frontend',
    description: 'React web application',
    icon: <WebIcon />,
    color: '#61DAFB',
    port: 80,
    path: '/',
    credentials: {},
    status: 'deployed' as const,
  },
];

// CRM Service endpoints to monitor
const CRM_SERVICES = [
  { name: 'CRM API', endpoint: '/api/monitoring/health', icon: <ApiIcon />, description: 'Main API service' },
  { name: 'Database', endpoint: '/api/monitoring/health/ready', icon: <DatabaseIcon />, description: 'MariaDB database' },
  { name: 'Frontend', endpoint: '/', icon: <WebIcon />, description: 'React web application' },
];

interface ServiceStatus {
  name: string;
  status: 'healthy' | 'degraded' | 'error' | 'unknown' | 'loading';
  responseTime?: number;
  message?: string;
  details?: Record<string, unknown>;
}

interface EnvironmentInfo {
  deploymentType: string;
  isDocker: boolean;
  isKubernetes: boolean;
  databaseProvider: string;
  databaseConnected: boolean;
  hostname: string;
  version: string;
  dotNetVersion?: string;
  enabledMonitors?: string[];
}

interface ExternalToolStatus {
  status: 'online' | 'offline' | 'degraded' | 'error';
  version?: string;
  url?: string;
  port?: number;
  message?: string;
}

interface MonitoringToolsData {
  uptimeKuma: ExternalToolStatus;
  portainer: ExternalToolStatus;
  timestamp: string;
}

interface UptimeKumaMonitor {
  id: string;
  status: number; // 0 = down, 1 = up, 2 = pending
  ping: number;
  time: string;
  msg?: string;
}

interface UptimeKumaMonitorsData {
  connected: boolean;
  monitors: UptimeKumaMonitor[];
  uptimeList?: Record<string, number>;
  monitorCount: number;
  message?: string;
}

interface PortainerData {
  connected: boolean;
  version?: string;
  instanceId?: string;
  message?: string;
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div role="tabpanel" hidden={value !== index} {...other}>
      {value === index && <Box sx={{ py: 3 }}>{children}</Box>}
    </div>
  );
}

// ── CDT batch descriptors ─────────────────────────────────────────────────
interface CdtCheck { label: string; method: string; path: string; body?: object; expectedStatus?: number; }
interface CdtBatch { id: string; label: string; description: string; checks: CdtCheck[]; }
interface CdtCheckResult { label: string; path: string; status: 'pass' | 'fail' | 'skip'; httpStatus?: number; durationMs?: number; error?: string; }
interface CdtBatchResult { id: string; label: string; passed: number; failed: number; skipped: number; durationMs: number; results: CdtCheckResult[]; }

const CDT_BATCHES: CdtBatch[] = [
  { id: 'b01', label: 'Batch 01 — System & Users',          description: 'Core system health, users, departments',                  checks: [{label:'Health liveness',method:'GET',path:'/health/live'},{label:'List users',method:'GET',path:'/api/users'},{label:'List departments',method:'GET',path:'/api/departments'},{label:'System settings',method:'GET',path:'/api/settings/system'}] },
  { id: 'b02', label: 'Batch 02 — Accounts & Contacts',     description: 'Account (customer) and contact listings',                 checks: [{label:'List accounts',method:'GET',path:'/api/accounts'},{label:'List contacts',method:'GET',path:'/api/contacts'},{label:'Account search',method:'GET',path:'/api/accounts?page=1&pageSize=5'}] },
  { id: 'b03', label: 'Batch 03 — Leads & Products',        description: 'Sales leads and product catalog',                         checks: [{label:'List leads',method:'GET',path:'/api/leads'},{label:'List products',method:'GET',path:'/api/products'},{label:'Active products',method:'GET',path:'/api/products?isActive=true'}] },
  { id: 'b04', label: 'Batch 04 — Opportunities & Orders',  description: 'Pipeline, quotes, orders, invoices',                     checks: [{label:'List opportunities',method:'GET',path:'/api/opportunities'},{label:'List quotes',method:'GET',path:'/api/quotes'},{label:'List orders',method:'GET',path:'/api/orders'},{label:'List invoices',method:'GET',path:'/api/invoices'}] },
  { id: 'b05', label: 'Batch 05 — Interactions & Tasks',    description: 'Activity tracking, tasks, notes',                        checks: [{label:'List interactions',method:'GET',path:'/api/interactions'},{label:'List tasks',method:'GET',path:'/api/tasks'},{label:'List notes',method:'GET',path:'/api/notes'}] },
  { id: 'b06', label: 'Batch 06 — Campaigns & Email',       description: 'Marketing campaigns and email templates',                 checks: [{label:'List campaigns',method:'GET',path:'/api/campaigns'},{label:'Email templates',method:'GET',path:'/api/emailtemplates'},{label:'Email sequences',method:'GET',path:'/api/emailsequences'}] },
  { id: 'b07', label: 'Batch 07 — Service Desk (ITSM)',     description: 'Tickets, knowledge base, SLA policies',                  checks: [{label:'List service requests',method:'GET',path:'/api/servicerequests'},{label:'Knowledge articles',method:'GET',path:'/api/knowledgearticles'},{label:'SLA policies',method:'GET',path:'/api/slapolicies'}] },
  { id: 'b08', label: 'Batch 08 — Commissions & Territories', description: 'Commission plans and territory management',             checks: [{label:'Commission plans',method:'GET',path:'/api/commissionplans'},{label:'Territories',method:'GET',path:'/api/territories'},{label:'Commission records',method:'GET',path:'/api/commissions'}] },
  { id: 'b09', label: 'Batch 09 — Workflows & Escalations', description: 'Workflow definitions and escalation rules',               checks: [{label:'Workflow definitions',method:'GET',path:'/api/workflowdefinitions'},{label:'Escalation rules',method:'GET',path:'/api/escalationrules'},{label:'Workflow categories',method:'GET',path:'/api/servicerequestcategories'}] },
  { id: 'b10', label: 'Batch 10 — AI Agents & Webhooks',   description: 'AI agent registry and webhook endpoints',                 checks: [{label:'List agents',method:'GET',path:'/api/agents'},{label:'Webhooks',method:'GET',path:'/api/webhooks'},{label:'Agent analytics',method:'GET',path:'/api/agents/analytics/usage'}] },
  { id: 'b11', label: 'Batch 11 — Monitoring & Features',  description: 'Provider health and feature flags',                       checks: [{label:'Provider health',method:'GET',path:'/api/health/providers'},{label:'Feature flags',method:'GET',path:'/api/admin/features'},{label:'System controls',method:'GET',path:'/api/system-controls/rate-limiting'}] },
  { id: 'b12', label: 'Batch 12 — Files & Tags',           description: 'Document storage and tagging system',                     checks: [{label:'Files list',method:'GET',path:'/api/files'},{label:'Tags list',method:'GET',path:'/api/tags'},{label:'Audit logs',method:'GET',path:'/api/audit-logs'}] },
  { id: 'b13', label: 'Batch 13 — Subscriptions & Payments', description: 'Subscription and payment records',                      checks: [{label:'Subscriptions',method:'GET',path:'/api/subscriptions'},{label:'Payments',method:'GET',path:'/api/payments'},{label:'Contracts',method:'GET',path:'/api/contracts'}] },
  { id: 'b14', label: 'Batch 14 — Rules & Routing',        description: 'Assignment rules, pricing rules, lead routing',            checks: [{label:'Assignment rules',method:'GET',path:'/api/assignmentrules'},{label:'Pricing rules',method:'GET',path:'/api/pricingrules'},{label:'Lead routing rules',method:'GET',path:'/api/leadrouting/rules'},{label:'Rule sets',method:'GET',path:'/api/rulesets'}] },
];

const MonitoringDashboard: React.FC = () => {
  const theme = useTheme();
  const [loading, setLoading] = useState(true);
  const [envInfo, setEnvInfo] = useState<EnvironmentInfo | null>(null);
  const [services, setServices] = useState<ServiceStatus[]>([]);
  const [monitoringTools, setMonitoringTools] = useState<ServiceStatus[]>([]);
  const [externalTools, setExternalTools] = useState<MonitoringToolsData | null>(null);
  const [uptimeKumaMonitors, setUptimeKumaMonitors] = useState<UptimeKumaMonitorsData | null>(null);
  const [portainerData, setPortainerData] = useState<PortainerData | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [lastRefresh, setLastRefresh] = useState(new Date());
  const [tabValue, setTabValue] = useState(0);
  const [showEmbeddedView, setShowEmbeddedView] = useState<'none' | 'uptimeKuma' | 'portainer' | 'superset'>('none');

  // ── Dev Tools & Controls state ───────────────────────────────────────────
  const [rateLimitEnabled, setRateLimitEnabled] = useState<boolean | null>(null);
  const [rateLimitLoading, setRateLimitLoading] = useState(false);
  const [rateLimitLastChanged, setRateLimitLastChanged] = useState<string | null>(null);
  const [jwtInfo, setJwtInfo] = useState<{fingerprint?: string; lastRotatedAt?: string | null; lastRotatedBy?: string | null} | null>(null);
  const [jwtRotating, setJwtRotating] = useState(false);
  const [jwtRotateResult, setJwtRotateResult] = useState<{success: boolean; message: string} | null>(null);
  const [jwtConfirmOpen, setJwtConfirmOpen] = useState(false);
  const [cdtRunning, setCdtRunning] = useState(false);
  const [cdtResults, setCdtResults] = useState<CdtBatchResult[]>([]);
  const [cdtProgress, setCdtProgress] = useState(0);
  const [cdtExpanded, setCdtExpanded] = useState<string | false>(false);

  // Determine base URL for monitoring tools
  const getBaseUrl = () => {
    const hostname = window.location.hostname;
    return hostname === 'localhost' ? 'localhost' : hostname;
  };

  const baseUrl = getBaseUrl();

  // Check CRM service health with actual API calls
  const checkCrmServiceHealth = useCallback(async (service: typeof CRM_SERVICES[0]): Promise<ServiceStatus> => {
    const startTime = Date.now();
    try {
      const controller = new AbortController();
      const timeoutId = setTimeout(() => controller.abort(), 10000);
      
      const response = await fetch(service.endpoint, { 
        method: 'GET',
        signal: controller.signal,
        headers: { 'Accept': 'application/json' }
      });
      
      clearTimeout(timeoutId);
      const responseTime = Date.now() - startTime;
      
      if (response.ok) {
        let details = {};
        const contentType = response.headers.get('content-type');
        if (contentType?.includes('application/json')) {
          try {
            details = await response.json();
          } catch {
            // Not JSON response
          }
        }
        
        return {
          name: service.name,
          status: 'healthy',
          responseTime,
          message: 'Service is running',
          details,
        };
      }
      
      return {
        name: service.name,
        status: response.status >= 500 ? 'error' : 'degraded',
        responseTime,
        message: `HTTP ${response.status}`,
      };
    } catch (err) {
      const responseTime = Date.now() - startTime;
      const message = err instanceof Error ? err.message : 'Connection failed';
      return {
        name: service.name,
        status: 'error',
        responseTime,
        message: message.includes('abort') ? 'Timeout' : message,
      };
    }
  }, []);

  // Check monitoring tool availability
  const checkMonitoringTool = useCallback(async (name: string, port: number): Promise<ServiceStatus> => {
    const startTime = Date.now();
    try {
      const controller = new AbortController();
      const timeoutId = setTimeout(() => controller.abort(), 5000);
      
      // Try to fetch the tool's main page
      const response = await fetch(`http://${baseUrl}:${port}/`, {  // NOSONAR - S5332 - http:// URL built from runtime baseUrl variable for local monitoring access
        method: 'HEAD',
        mode: 'no-cors',
        signal: controller.signal 
      });
      
      clearTimeout(timeoutId);
      const responseTime = Date.now() - startTime;
      
      // no-cors mode doesn't give us status, but if we get here, the service responded
      return {
        name,
        status: 'healthy',
        responseTime,
        message: 'Service is accessible',
      };
    } catch (err) {
      return {
        name,
        status: 'unknown',
        responseTime: Date.now() - startTime,
        message: 'Unable to verify (may require manual check)',
      };
    }
  }, [baseUrl]);

  const refreshData = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      // Fetch environment info from API
      const envResponse = await fetch('/api/monitoring/environment');
      if (envResponse.ok) {
        const data = await envResponse.json();
        setEnvInfo(data);
      } else {
        console.warn('Environment info not available:', envResponse.status);
      }
    } catch (err) {
      console.warn('Environment info fetch failed:', err);
    }

    // Fetch external tools status from backend API (avoids CORS issues)
    try {
      const toolsResponse = await fetch('/api/monitoring/tools/status');
      if (toolsResponse.ok) {
        const toolsData: MonitoringToolsData = await toolsResponse.json();
        setExternalTools(toolsData);
        
        // Also update the legacy monitoringTools state for backwards compatibility
        setMonitoringTools([
          {
            name: 'Uptime Kuma',
            status: toolsData.uptimeKuma.status === 'online' ? 'healthy' : 
                   toolsData.uptimeKuma.status === 'degraded' ? 'degraded' : 
                   toolsData.uptimeKuma.status === 'offline' ? 'error' : 'unknown',
            message: toolsData.uptimeKuma.status === 'online' ? 'Service is running' : 
                     toolsData.uptimeKuma.message || 'Service unavailable',
          },
          {
            name: 'Portainer',
            status: toolsData.portainer.status === 'online' ? 'healthy' : 
                   toolsData.portainer.status === 'degraded' ? 'degraded' : 
                   toolsData.portainer.status === 'offline' ? 'error' : 'unknown',
            message: toolsData.portainer.version ? `v${toolsData.portainer.version}` : 
                     toolsData.portainer.message || 'Service unavailable',
          },
        ]);
      }
    } catch (err) {
      console.warn('External tools status fetch failed:', err);
      // Fallback to direct checks if API fails
      const toolChecks = await Promise.all([
        checkMonitoringTool('Uptime Kuma', MONITORING_TOOLS.uptimeKuma.defaultPort),
        checkMonitoringTool('Portainer', MONITORING_TOOLS.portainer.defaultPort),
      ]);
      setMonitoringTools(toolChecks);
    }

    // Fetch detailed Uptime Kuma monitor data
    try {
      const monitorsResponse = await fetch('/api/monitoring/uptime-kuma/monitors');
      if (monitorsResponse.ok) {
        const monitorsData: UptimeKumaMonitorsData = await monitorsResponse.json();
        setUptimeKumaMonitors(monitorsData);
      }
    } catch (err) {
      console.warn('Uptime Kuma monitors fetch failed:', err);
    }

    // Fetch Portainer data
    try {
      const portainerResponse = await fetch('/api/monitoring/portainer/containers');
      if (portainerResponse.ok) {
        const pData: PortainerData = await portainerResponse.json();
        setPortainerData(pData);
      }
    } catch (err) {
      console.warn('Portainer data fetch failed:', err);
    }

    // Check CRM services health
    const serviceChecks = await Promise.all(
      CRM_SERVICES.map(service => checkCrmServiceHealth(service))
    );
    setServices(serviceChecks);

    setLastRefresh(new Date());
    setLoading(false);
  }, [checkCrmServiceHealth, checkMonitoringTool]);

  useEffect(() => {
    refreshData();
    const interval = setInterval(refreshData, 30000); // Refresh every 30 seconds
    return () => clearInterval(interval);
  }, [refreshData]);

  const getStatusIcon = (status: string, size: 'small' | 'medium' = 'medium') => {
    const fontSize = size === 'small' ? 20 : 24;
    switch (status) {
      case 'healthy':
        return <HealthyIcon sx={{ color: '#4caf50', fontSize }} />;
      case 'degraded':
        return <WarningIcon sx={{ color: '#ff9800', fontSize }} />;
      case 'error':
        return <ErrorIcon sx={{ color: '#f44336', fontSize }} />;
      case 'loading':
        return <CircularProgress size={fontSize - 4} />;
      default:
        return <WarningIcon sx={{ color: '#9e9e9e', fontSize }} />;
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'healthy': return '#4caf50';
      case 'degraded': return '#ff9800';
      case 'error': return '#f44336';
      default: return '#9e9e9e';
    }
  };

  const getServiceStatus = (name: string): ServiceStatus | undefined => {
    return monitoringTools.find(s => s.name === name);
  };

  const openTool = (port: number, path: string = '/') => {
    window.open(`http://${baseUrl}:${port}${path}`, '_blank'); // NOSONAR - S5332 - http:// URL built from runtime baseUrl variable for local monitoring access
  };

  const handleTabChange = (_: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
    if (newValue === 4) {
      // Lazy-load controls status when tab is first opened
      if (rateLimitEnabled === null) fetchRateLimitStatus();
      if (!jwtInfo) fetchJwtInfo();
    }
  };

  // ── Dev Tools helpers ────────────────────────────────────────────────────
  const authHeader = () => ({ 'Authorization': `Bearer ${localStorage.getItem('accessToken') ?? ''}`, 'Content-Type': 'application/json' });

  const fetchRateLimitStatus = async () => {
    try {
      const res = await fetch('/api/system-controls/rate-limiting', { headers: authHeader() });
      if (res.ok) {
        const data = await res.json();
        setRateLimitEnabled(data.isEnabled ?? data.enabled ?? false);
        setRateLimitLastChanged(data.lastChangedAt ?? null);
      }
    } catch { /* network error – leave null */ }
  };

  const toggleRateLimit = async (enable: boolean) => {
    setRateLimitLoading(true);
    try {
      const action = enable ? 'enable' : 'disable';
      const res = await fetch(`/api/system-controls/rate-limiting/${action}`, { method: 'POST', headers: authHeader() });
      if (res.ok) {
        const data = await res.json();
        setRateLimitEnabled(data.isEnabled ?? enable);
        setRateLimitLastChanged(data.changedAt ?? new Date().toISOString());
      }
    } finally {
      setRateLimitLoading(false);
    }
  };

  const fetchJwtInfo = async () => {
    try {
      const res = await fetch('/api/system-controls/jwt-rotation', { headers: authHeader() });
      if (res.ok) { const d = await res.json(); setJwtInfo(d); }
    } catch { /* ignore */ }
  };

  const rotateJwtSecret = async () => {
    setJwtRotating(true);
    setJwtRotateResult(null);
    try {
      const res = await fetch('/api/system-controls/jwt-rotation/rotate', { method: 'POST', headers: authHeader() });
      const d = await res.json();
      if (res.ok) {
        setJwtRotateResult({ success: true, message: d.message ?? 'Secret rotated successfully. All existing tokens are now invalid.' });
        setJwtInfo(prev => ({ ...prev, fingerprint: d.newFingerprint, lastRotatedAt: new Date().toISOString(), lastRotatedBy: 'admin' }));
      } else {
        setJwtRotateResult({ success: false, message: d.error ?? d.message ?? 'Rotation failed.' });
      }
    } catch (e: any) {
      setJwtRotateResult({ success: false, message: e.message });
    } finally {
      setJwtRotating(false);
    }
  };

  const runCdt = async () => {
    setCdtRunning(true);
    setCdtResults([]);
    setCdtProgress(0);
    const batchResults: CdtBatchResult[] = [];
    for (let bi = 0; bi < CDT_BATCHES.length; bi++) {
      const batch = CDT_BATCHES[bi];
      const bStart = Date.now();
      const checkResults: CdtCheckResult[] = [];
      for (const check of batch.checks) {
        const cStart = Date.now();
        try {
          const res = await fetch(check.path, { method: check.method, headers: authHeader(), ...(check.body ? { body: JSON.stringify(check.body) } : {}) });
          const expected = check.expectedStatus ?? 200;
          checkResults.push({ label: check.label, path: check.path, status: res.status === expected || res.status < 400 ? 'pass' : 'fail', httpStatus: res.status, durationMs: Date.now() - cStart });
        } catch (e: any) {
          checkResults.push({ label: check.label, path: check.path, status: 'fail', error: e.message, durationMs: Date.now() - cStart });
        }
      }
      batchResults.push({ id: batch.id, label: batch.label, passed: checkResults.filter(r => r.status === 'pass').length, failed: checkResults.filter(r => r.status === 'fail').length, skipped: checkResults.filter(r => r.status === 'skip').length, durationMs: Date.now() - bStart, results: checkResults });
      setCdtResults([...batchResults]);
      setCdtProgress(Math.round(((bi + 1) / CDT_BATCHES.length) * 100));
    }
    setCdtRunning(false);
  };

  const downloadCdtReport = () => {
    const totalPass = cdtResults.reduce((s, b) => s + b.passed, 0);
    const totalFail = cdtResults.reduce((s, b) => s + b.failed, 0);
    const totalChecks = totalPass + totalFail;
    const scoreColor = totalFail === 0 ? '#22c55e' : totalFail < 3 ? '#f59e0b' : '#ef4444';
    const html = `<!DOCTYPE html><html lang="en"><head><meta charset="UTF-8"><title>CRM CDT Report</title>
<style>*{box-sizing:border-box;margin:0;padding:0}body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;background:#0f172a;color:#e2e8f0;padding:32px}
h1{font-size:28px;font-weight:700;margin-bottom:4px}.subtitle{color:#94a3b8;margin-bottom:24px;font-size:14px}
.summary{display:flex;gap:16px;margin-bottom:32px}.stat{background:#1e293b;border-radius:12px;padding:20px 28px;flex:1;text-align:center}
.stat .val{font-size:36px;font-weight:800;color:${scoreColor}}.stat .lbl{color:#64748b;font-size:12px;text-transform:uppercase;letter-spacing:1px;margin-top:4px}
.batch{background:#1e293b;border-radius:12px;margin-bottom:12px;overflow:hidden}.bh{display:flex;align-items:center;padding:16px 20px;gap:12px;cursor:default}
.bh .name{flex:1;font-weight:600;font-size:15px}.badges{display:flex;gap:8px;font-size:12px;font-weight:600}
.badge{padding:4px 10px;border-radius:20px}.pass{background:#14532d;color:#86efac}.fail{background:#450a0a;color:#fca5a5}.skip{background:#1e3a5f;color:#93c5fd}
.checks{padding:0 20px 16px}.check{display:flex;align-items:center;gap:10px;padding:8px 0;border-bottom:1px solid #1e293b;font-size:13px}
.check:last-child{border:none}.dot{width:8px;height:8px;border-radius:50%;flex-shrink:0}
.dot.pass{background:#22c55e}.dot.fail{background:#ef4444}.dot.skip{background:#3b82f6}
.check .path{color:#64748b;font-size:11px;font-family:monospace;margin-left:auto}.status-badge{font-size:11px;padding:2px 8px;border-radius:10px;font-weight:600}
footer{text-align:center;color:#334155;margin-top:32px;font-size:12px}
</style></head><body>
<h1>🧪 Comprehensive CRUD Tests Report</h1>
<p class="subtitle">Generated ${new Date().toLocaleString()} &nbsp;·&nbsp; CRM Solution CDT</p>
<div class="summary">
  <div class="stat"><div class="val">${totalPass}/${totalChecks}</div><div class="lbl">Checks Passed</div></div>
  <div class="stat"><div class="val" style="color:${totalFail>0?'#ef4444':'#22c55e'}">${totalFail}</div><div class="lbl">Failures</div></div>
  <div class="stat"><div class="val">${cdtResults.length}</div><div class="lbl">Batches Run</div></div>
  <div class="stat"><div class="val">${Math.round(cdtResults.reduce((s,b)=>s+b.durationMs,0)/1000*10)/10}s</div><div class="lbl">Total Time</div></div>
</div>
${cdtResults.map(b => `<div class="batch">
<div class="bh"><span class="name">${b.label}</span>
<div class="badges"><span class="badge pass">✓ ${b.passed}</span>${b.failed>0?`<span class="badge fail">✗ ${b.failed}</span>`:''}<span style="color:#64748b;font-size:12px">${(b.durationMs/1000).toFixed(2)}s</span></div></div>
<div class="checks">${b.results.map(r=>`<div class="check"><div class="dot ${r.status}"></div><span>${r.label}</span>${r.httpStatus?`<span class="status-badge ${r.status}">${r.httpStatus}</span>`:''}${r.error?`<span style="color:#f87171;font-size:11px">${r.error}</span>`:''}<span class="path">${r.path}</span></div>`).join('')}</div>
</div>`).join('')}
<footer>CRM Solution · Comprehensive CRUD Tests · ${new Date().toISOString()}</footer>
</body></html>`;
    const blob = new Blob([html], { type: 'text/html' });
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = `cdt-report-${new Date().toISOString().replace(/[:.]/g, '-')}.html`;
    a.click();
  };


  const healthySvc = services.filter(s => s.status === 'healthy').length;
  const totalSvc = services.length;

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
        <Box>
          <Typography variant="h4" sx={{ fontWeight: 700, display: 'flex', alignItems: 'center', gap: 2 }}>
            <DashboardIcon sx={{ fontSize: 40 }} />
            System Monitoring
          </Typography>
          <Typography color="textSecondary" sx={{ mt: 1 }}>
            Infrastructure and service monitoring dashboard
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
          <Typography variant="body2" color="textSecondary">
            Last updated: {lastRefresh.toLocaleTimeString()}
          </Typography>
          <Tooltip title="Refresh">
            <IconButton onClick={refreshData} disabled={loading} color="primary">
              {loading ? <CircularProgress size={24} /> : <RefreshIcon />}
            </IconButton>
          </Tooltip>
        </Box>
      </Box>

      {/* Environment Info Banner */}
      {envInfo && (
        <Alert severity="info" sx={{ mb: 4, borderRadius: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
            <Chip 
              icon={envInfo.isKubernetes ? <KubernetesIcon /> : <ContainerIcon />}
              label={envInfo.deploymentType?.toUpperCase() || 'DOCKER'} 
              color="primary" 
            />
            <Chip 
              icon={<DatabaseIcon />}
              label={`${envInfo.databaseProvider} ${envInfo.databaseConnected ? '✓' : '✗'}`} 
              color={envInfo.databaseConnected ? 'success' : 'error'}
              variant="outlined"
            />
            <Chip 
              icon={<DnsIcon />}
              label={envInfo.hostname || 'Unknown Host'} 
              variant="outlined"
            />
            <Chip 
              label={`v${envInfo.version || '1.0.0'}`} 
              size="small"
              variant="outlined"
            />
            {envInfo.dotNetVersion && (
              <Chip 
                label={envInfo.dotNetVersion} 
                size="small"
                variant="outlined"
              />
            )}
          </Box>
        </Alert>
      )}

      {/* Loading indicator */}
      {loading && <LinearProgress sx={{ mb: 2 }} />}

      {/* Services Status Summary */}
      <Paper elevation={0} sx={{ p: 3, mb: 4, borderRadius: 2, bgcolor: 'background.default' }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="h5" sx={{ fontWeight: 600 }}>
            CRM Services Status
          </Typography>
          <Chip 
            icon={healthySvc === totalSvc ? <HealthyIcon /> : <WarningIcon />}
            label={`${healthySvc}/${totalSvc} Healthy`}
            color={healthySvc === totalSvc ? 'success' : 'warning'}
          />
        </Box>
        
        <Grid container spacing={2}>
          {services.map((service, index) => {
            const svcDef = CRM_SERVICES[index];
            return (
              <Grid item xs={12} sm={6} md={4} key={service.name}>
                <Paper 
                  variant="outlined" 
                  sx={{ 
                    p: 2, 
                    borderRadius: 2,
                    borderColor: getStatusColor(service.status),
                    borderWidth: 2,
                  }}
                >
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 1 }}>
                    <Box sx={{ color: getStatusColor(service.status) }}>
                      {svcDef?.icon || <ApiIcon />}
                    </Box>
                    <Box sx={{ flex: 1 }}>
                      <Typography variant="subtitle1" fontWeight={600}>
                        {service.name}
                      </Typography>
                      <Typography variant="caption" color="textSecondary">
                        {svcDef?.description}
                      </Typography>
                    </Box>
                    {getStatusIcon(service.status, 'small')}
                  </Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 1 }}>
                    <Typography variant="caption" color="textSecondary">
                      {service.message}
                    </Typography>
                    {service.responseTime && (
                      <Typography variant="caption" color="textSecondary">
                        {service.responseTime}ms
                      </Typography>
                    )}
                  </Box>
                </Paper>
              </Grid>
            );
          })}
        </Grid>
      </Paper>

      {/* Tabs for different views */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 0 }}>
        <Tabs value={tabValue} onChange={handleTabChange}>
          <Tab label="Monitoring Tools" />
          <Tab label="External Resources" />
          <Tab label="Quick Links" />
          <Tab label="Embedded View" />
          <Tab label="Dev Tools & Controls" icon={<SecurityIcon />} iconPosition="start" />
        </Tabs>
      </Box>

      {/* Tab 0: Monitoring Tools Cards */}
      <TabPanel value={tabValue} index={0}>
        <Grid container spacing={3}>
          {/* Uptime Kuma Card */}
          <Grid item xs={12} md={6}>
            <Card 
              sx={{ 
                borderRadius: 3, 
                height: '100%',
                border: `2px solid ${MONITORING_TOOLS.uptimeKuma.color}40`,
                transition: 'all 0.3s ease',
                '&:hover': {
                  boxShadow: `0 8px 24px ${MONITORING_TOOLS.uptimeKuma.color}30`,
                  transform: 'translateY(-4px)',
                }
              }}
            >
              <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                  <Box sx={{ color: MONITORING_TOOLS.uptimeKuma.color }}>
                    {MONITORING_TOOLS.uptimeKuma.icon}
                  </Box>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    {getStatusIcon(getServiceStatus('Uptime Kuma')?.status || 'unknown')}
                    <Chip 
                      label={`Port ${MONITORING_TOOLS.uptimeKuma.defaultPort}`} 
                      size="small" 
                      variant="outlined" 
                    />
                    {externalTools?.uptimeKuma?.status === 'online' && (
                      <Chip 
                        label="Online" 
                        size="small" 
                        color="success"
                      />
                    )}
                  </Box>
                </Box>
                
                <Typography variant="h5" sx={{ fontWeight: 700, mb: 1 }}>
                  {MONITORING_TOOLS.uptimeKuma.name}
                </Typography>
                <Typography color="textSecondary" sx={{ mb: 2 }}>
                  {MONITORING_TOOLS.uptimeKuma.description}
                </Typography>
                
                <Divider sx={{ my: 2 }} />
                
                {/* Live Monitor Status from Uptime Kuma */}
                {uptimeKumaMonitors?.connected && uptimeKumaMonitors.monitors.length > 0 ? (
                  <>
                    <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
                      Monitor Status ({uptimeKumaMonitors.monitorCount} monitors):
                    </Typography>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
                      {uptimeKumaMonitors.monitors.slice(0, 8).map((monitor) => (
                        <Chip 
                          key={monitor.id}
                          label={monitor.status === 1 ? '✓' : monitor.status === 0 ? '✗' : '?'}
                          size="small"
                          color={monitor.status === 1 ? 'success' : monitor.status === 0 ? 'error' : 'default'}
                          title={`ID: ${monitor.id}, Ping: ${monitor.ping}ms`}
                        />
                      ))}
                      {uptimeKumaMonitors.monitors.length > 8 && (
                        <Chip label={`+${uptimeKumaMonitors.monitors.length - 8} more`} size="small" variant="outlined" />
                      )}
                    </Box>
                    <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
                      <Typography variant="body2" color="textSecondary">
                        Up: {uptimeKumaMonitors.monitors.filter(m => m.status === 1).length}
                      </Typography>
                      <Typography variant="body2" color="error">
                        Down: {uptimeKumaMonitors.monitors.filter(m => m.status === 0).length}
                      </Typography>
                    </Box>
                  </>
                ) : externalTools?.uptimeKuma?.status === 'online' ? (
                  <>
                    <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
                      Status:
                    </Typography>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
                      <Chip 
                        icon={<HealthyIcon />}
                        label="Service Running"
                        size="small"
                        color="success"
                      />
                      <Chip 
                        label="8 monitors configured"
                        size="small"
                        variant="outlined"
                      />
                    </Box>
                    <Typography variant="body2" color="textSecondary" sx={{ mb: 1 }}>
                      Click to view monitor dashboard
                    </Typography>
                  </>
                ) : (
                  <>
                    <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
                      Features:
                    </Typography>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                      {MONITORING_TOOLS.uptimeKuma.features.map((feature) => (
                        <Chip key={feature} label={feature} size="small" variant="outlined" />
                      ))}
                    </Box>
                  </>
                )}
              </CardContent>
              <CardActions sx={{ p: 2, pt: 0 }}>
                <Button 
                  variant="contained" 
                  fullWidth
                  onClick={() => openTool(MONITORING_TOOLS.uptimeKuma.defaultPort)}
                  endIcon={<OpenInNewIcon />}
                sx={{ 
                  bgcolor: MONITORING_TOOLS.uptimeKuma.color,
                  '&:hover': { bgcolor: MONITORING_TOOLS.uptimeKuma.color, filter: 'brightness(0.9)' }
                }}
              >
                Open Uptime Kuma
              </Button>
            </CardActions>
          </Card>
        </Grid>

        {/* Portainer Card */}
        <Grid item xs={12} md={6}>
          <Card 
            sx={{ 
              borderRadius: 3, 
              height: '100%',
              border: `2px solid ${MONITORING_TOOLS.portainer.color}40`,
              transition: 'all 0.3s ease',
              '&:hover': {
                boxShadow: `0 8px 24px ${MONITORING_TOOLS.portainer.color}30`,
                transform: 'translateY(-4px)',
              }
            }}
          >
            <CardContent>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                <Box sx={{ color: MONITORING_TOOLS.portainer.color }}>
                  {MONITORING_TOOLS.portainer.icon}
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  {getStatusIcon(getServiceStatus('Portainer')?.status || 'unknown')}
                  <Chip 
                    label={`Port ${MONITORING_TOOLS.portainer.defaultPort}`} 
                    size="small" 
                    variant="outlined" 
                  />
                  <Chip 
                    label="Agent Only"
                    size="small"
                    color="info"
                    variant="outlined"
                  />
                </Box>
              </Box>
              
              <Typography variant="h5" sx={{ fontWeight: 700, mb: 1 }}>
                {MONITORING_TOOLS.portainer.name}
              </Typography>
              <Typography color="textSecondary" sx={{ mb: 2 }}>
                {MONITORING_TOOLS.portainer.description}
              </Typography>
              
              <Divider sx={{ my: 2 }} />
              
              <Alert severity="info" sx={{ mb: 2 }}>
                <Typography variant="body2">
                  The Portainer Agent is running on port 9001. Connect to it from a Portainer CE/EE instance to manage Docker containers remotely.
                </Typography>
              </Alert>
              
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
                Features:
              </Typography>
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                {MONITORING_TOOLS.portainer.features.map((feature) => (
                  <Chip key={feature} label={feature} size="small" variant="outlined" />
                ))}
              </Box>
            </CardContent>
            <CardActions sx={{ p: 2, pt: 0 }}>
              <Button 
                variant="outlined" 
                fullWidth
                disabled
                sx={{ 
                  borderColor: MONITORING_TOOLS.portainer.color,
                  color: MONITORING_TOOLS.portainer.color,
                }}
              >
                Agent Only — No Web UI
              </Button>
            </CardActions>
          </Card>
        </Grid>

        {/* Apache Superset Card */}
        <Grid item xs={12} md={6}>
          <Card 
            sx={{ 
              borderRadius: 3, 
              height: '100%',
              border: `2px solid ${MONITORING_TOOLS.superset.color}40`,
              transition: 'all 0.3s ease',
              '&:hover': {
                boxShadow: `0 8px 24px ${MONITORING_TOOLS.superset.color}30`,
                transform: 'translateY(-4px)',
              }
            }}
          >
            <CardContent>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                <Box sx={{ color: MONITORING_TOOLS.superset.color }}>
                  {MONITORING_TOOLS.superset.icon}
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <Chip 
                    label={`Port ${MONITORING_TOOLS.superset.defaultPort}`} 
                    size="small" 
                    variant="outlined" 
                  />
                  <Chip 
                    label="Online" 
                    size="small" 
                    color="success"
                  />
                </Box>
              </Box>
              
              <Typography variant="h5" sx={{ fontWeight: 700, mb: 1 }}>
                {MONITORING_TOOLS.superset.name}
              </Typography>
              <Typography color="textSecondary" sx={{ mb: 2 }}>
                {MONITORING_TOOLS.superset.description}
              </Typography>
              
              <Divider sx={{ my: 2 }} />
              
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
                CRM Dashboards:
              </Typography>
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
                <Chip label="CRM Overview" size="small" color="primary" />
                <Chip label="10 CRM Datasets" size="small" variant="outlined" />
                <Chip label="6 Charts" size="small" variant="outlined" />
                <Chip label="SQL Lab" size="small" variant="outlined" />
              </Box>
              
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
                Features:
              </Typography>
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                {MONITORING_TOOLS.superset.features.map((feature) => (
                  <Chip key={feature} label={feature} size="small" variant="outlined" />
                ))}
              </Box>
            </CardContent>
            <CardActions sx={{ p: 2, pt: 0 }}>
              <Button 
                variant="contained" 
                fullWidth
                onClick={() => openTool(MONITORING_TOOLS.superset.defaultPort, '/superset/dashboard/crm-overview/')}
                endIcon={<OpenInNewIcon />}
                sx={{ 
                  bgcolor: MONITORING_TOOLS.superset.color,
                  '&:hover': { bgcolor: MONITORING_TOOLS.superset.color, filter: 'brightness(0.9)' }
                }}
              >
                Open Superset
              </Button>
            </CardActions>
          </Card>
        </Grid>
      </Grid>
      </TabPanel>

      {/* Tab 1: External Resources */}
      <TabPanel value={tabValue} index={1}>
        <Alert severity="info" sx={{ mb: 3, borderRadius: 2 }}>
          <Typography variant="body2">
            All external resources deployed on the CRM infrastructure. Click to open in a new tab. Credentials are for the development environment only.
          </Typography>
        </Alert>

        <Grid container spacing={2}>
          {EXTERNAL_RESOURCES.map((resource) => (
            <Grid item xs={12} sm={6} md={4} key={resource.name}>
              <Card
                sx={{
                  borderRadius: 2,
                  height: '100%',
                  border: `1px solid ${resource.color}30`,
                  transition: 'all 0.2s ease',
                  '&:hover': {
                    boxShadow: `0 4px 16px ${resource.color}20`,
                    transform: 'translateY(-2px)',
                  }
                }}
              >
                <CardContent sx={{ pb: 1 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 1.5 }}>
                    <Box sx={{ color: resource.color }}>{resource.icon}</Box>
                    <Box sx={{ flex: 1 }}>
                      <Typography variant="subtitle1" fontWeight={700}>
                        {resource.name}
                      </Typography>
                    </Box>
                    <Chip
                      icon={<HealthyIcon />}
                      label="Deployed"
                      size="small"
                      color="success"
                      variant="outlined"
                    />
                  </Box>
                  <Typography variant="body2" color="textSecondary" sx={{ mb: 1.5 }}>
                    {resource.description}
                  </Typography>
                  <Box sx={{ bgcolor: 'action.hover', borderRadius: 1, p: 1.5, mb: 1 }}>
                    <Typography variant="caption" sx={{ fontFamily: 'monospace', display: 'block', mb: 0.5 }}>
                      <LinkIcon sx={{ fontSize: 12, mr: 0.5, verticalAlign: 'middle' }} />
                      {`http://${baseUrl}:${resource.port}${resource.path || ''}`} // NOSONAR - S5332 - http:// URL built from runtime baseUrl variable for local monitoring access
                    </Typography>
                    {resource.credentials && Object.keys(resource.credentials).length > 0 && (
                      <>
                        <Divider sx={{ my: 0.5 }} />
                        {Object.entries(resource.credentials).map(([key, value]) => (
                          <Typography key={key} variant="caption" sx={{ fontFamily: 'monospace', display: 'block' }}>
                            <LockIcon sx={{ fontSize: 10, mr: 0.5, verticalAlign: 'middle' }} />
                            {key}: <strong>{value}</strong>
                          </Typography>
                        ))}
                      </>
                    )}
                  </Box>
                </CardContent>
                <CardActions sx={{ px: 2, pb: 2, pt: 0 }}>
                  {resource.port !== 3306 && resource.port !== 6379 ? (
                    <Button
                      variant="outlined"
                      size="small"
                      fullWidth
                      onClick={() => openTool(resource.port, resource.path)}
                      endIcon={<OpenInNewIcon />}
                      sx={{ borderColor: resource.color, color: resource.color }}
                    >
                      Open
                    </Button>
                  ) : (
                    <Button
                      variant="outlined"
                      size="small"
                      fullWidth
                      disabled
                      sx={{ borderColor: resource.color }}
                    >
                      TCP Service — No Web UI
                    </Button>
                  )}
                </CardActions>
              </Card>
            </Grid>
          ))}
        </Grid>

        {/* Superset Quick Access */}
        <Paper variant="outlined" sx={{ borderRadius: 2, p: 3, mt: 3 }}>
          <Typography variant="h6" sx={{ fontWeight: 600, mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
            <AnalyticsIcon sx={{ color: '#20A7C9' }} />
            Apache Superset — Quick Access
          </Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6} md={3}>
              <Button
                variant="contained"
                fullWidth
                startIcon={<DashboardIcon />}
                onClick={() => openTool(8088, '/superset/dashboard/crm-overview/')}
                sx={{ py: 1.5, bgcolor: '#20A7C9', '&:hover': { bgcolor: '#1890b0' } }}
              >
                CRM Dashboard
              </Button>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Button
                variant="outlined"
                fullWidth
                startIcon={<DatabaseIcon />}
                onClick={() => openTool(8088, '/sqllab/')}
                sx={{ py: 1.5 }}
              >
                SQL Lab
              </Button>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Button
                variant="outlined"
                fullWidth
                startIcon={<AnalyticsIcon />}
                onClick={() => openTool(8088, '/chart/list/')}
                sx={{ py: 1.5 }}
              >
                All Charts
              </Button>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Button
                variant="outlined"
                fullWidth
                startIcon={<DashboardIcon />}
                onClick={() => openTool(8088, '/dashboard/list/')}
                sx={{ py: 1.5 }}
              >
                All Dashboards
              </Button>
            </Grid>
          </Grid>
        </Paper>
      </TabPanel>

      {/* Tab 2: Quick Links */}
      <TabPanel value={tabValue} index={2}>
        <Paper variant="outlined" sx={{ borderRadius: 2, p: 3, mb: 3 }}>
          <Typography variant="h6" sx={{ fontWeight: 600, mb: 2 }}>
            Uptime Kuma
          </Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6} md={3}>
              <Button 
                variant="outlined" 
                fullWidth 
                startIcon={<MonitorIcon />}
                onClick={() => openTool(MONITORING_TOOLS.uptimeKuma.defaultPort, '/dashboard')}
                sx={{ py: 1.5 }}
              >
                Dashboard
              </Button>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Button 
                variant="outlined" 
                fullWidth 
                startIcon={<SpeedIcon />}
                onClick={() => openTool(MONITORING_TOOLS.uptimeKuma.defaultPort, '/status')}
                sx={{ py: 1.5 }}
              >
                Status Page
              </Button>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Button 
                variant="outlined" 
                fullWidth 
                startIcon={<ApiIcon />}
                onClick={() => openTool(MONITORING_TOOLS.uptimeKuma.defaultPort, '/add')}
                sx={{ py: 1.5 }}
              >
                Add Monitor
              </Button>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Button 
                variant="outlined" 
                fullWidth 
                startIcon={<DnsIcon />}
                onClick={() => openTool(MONITORING_TOOLS.uptimeKuma.defaultPort, '/settings')}
                sx={{ py: 1.5 }}
              >
                Settings
              </Button>
            </Grid>
          </Grid>
        </Paper>

        <Paper variant="outlined" sx={{ borderRadius: 2, p: 3 }}>
          <Typography variant="h6" sx={{ fontWeight: 600, mb: 2 }}>
            Portainer Agent
          </Typography>
          <Alert severity="info" sx={{ mb: 2 }}>
            Portainer Agent (port 9001) provides a Docker API endpoint. Connect via a Portainer CE/EE instance for full container management.
          </Alert>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6} md={3}>
              <Button 
                variant="outlined" 
                fullWidth 
                startIcon={<ContainerIcon />}
                disabled
                sx={{ py: 1.5 }}
              >
                Agent on :9001
              </Button>
            </Grid>
          </Grid>
        </Paper>

        <Paper variant="outlined" sx={{ borderRadius: 2, p: 3, mt: 3 }}>
          <Typography variant="h6" sx={{ fontWeight: 600, mb: 2 }}>
            Apache Superset
          </Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6} md={3}>
              <Button 
                variant="outlined" 
                fullWidth 
                startIcon={<DashboardIcon />}
                onClick={() => openTool(MONITORING_TOOLS.superset.defaultPort, '/superset/dashboard/crm-overview/')}
                sx={{ py: 1.5 }}
              >
                CRM Dashboard
              </Button>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Button 
                variant="outlined" 
                fullWidth 
                startIcon={<DatabaseIcon />}
                onClick={() => openTool(MONITORING_TOOLS.superset.defaultPort, '/sqllab/')}
                sx={{ py: 1.5 }}
              >
                SQL Lab
              </Button>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Button 
                variant="outlined" 
                fullWidth 
                startIcon={<AnalyticsIcon />}
                onClick={() => openTool(MONITORING_TOOLS.superset.defaultPort, '/chart/list/')}
                sx={{ py: 1.5 }}
              >
                Charts
              </Button>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Button 
                variant="outlined" 
                fullWidth 
                startIcon={<DashboardIcon />}
                onClick={() => openTool(MONITORING_TOOLS.superset.defaultPort, '/dashboard/list/')}
                sx={{ py: 1.5 }}
              >
                All Dashboards
              </Button>
            </Grid>
          </Grid>
        </Paper>
      </TabPanel>

      {/* Tab 3: Embedded View */}
      <TabPanel value={tabValue} index={3}>
        <Box sx={{ mb: 2 }}>
          <Button 
            variant={showEmbeddedView === 'uptimeKuma' ? 'contained' : 'outlined'}
            onClick={() => setShowEmbeddedView(showEmbeddedView === 'uptimeKuma' ? 'none' : 'uptimeKuma')}
            sx={{ mr: 2 }}
            startIcon={<MonitorIcon />}
          >
            Uptime Kuma
          </Button>
          <Button 
            variant={showEmbeddedView === 'superset' ? 'contained' : 'outlined'}
            onClick={() => setShowEmbeddedView(showEmbeddedView === 'superset' ? 'none' : 'superset')}
            sx={{ mr: 2 }}
            startIcon={<AnalyticsIcon />}
          >
            Superset
          </Button>
          <Button 
            variant={showEmbeddedView === 'portainer' ? 'contained' : 'outlined'}
            onClick={() => setShowEmbeddedView(showEmbeddedView === 'portainer' ? 'none' : 'portainer')}
            startIcon={<ContainerIcon />}
            disabled
          >
            Portainer (Agent Only)
          </Button>
        </Box>

        {showEmbeddedView === 'none' && (
          <Alert severity="info">
            Click a button above to embed the monitoring tool dashboard. 
            Note: Some browsers may block embedded content due to security policies.
          </Alert>
        )}

        {showEmbeddedView === 'uptimeKuma' && (
          <Paper variant="outlined" sx={{ borderRadius: 2, overflow: 'hidden' }}>
            <Box sx={{ p: 2, bgcolor: 'background.default', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Typography variant="subtitle1" fontWeight={600}>Uptime Kuma Dashboard</Typography>
              <Button 
                size="small" 
                onClick={() => openTool(MONITORING_TOOLS.uptimeKuma.defaultPort)}
                endIcon={<OpenInNewIcon />}
              >
                Open in New Tab
              </Button>
            </Box>
            <iframe
              src={`http://${baseUrl}:${MONITORING_TOOLS.uptimeKuma.defaultPort}/dashboard`} // NOSONAR - S5332 - http:// URL built from runtime baseUrl variable for local monitoring access
              style={{ width: '100%', height: '600px', border: 'none' }}
              title="Uptime Kuma"
            />
          </Paper>
        )}

        {showEmbeddedView === 'superset' && (
          <Paper variant="outlined" sx={{ borderRadius: 2, overflow: 'hidden' }}>
            <Box sx={{ p: 2, bgcolor: 'background.default', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Typography variant="subtitle1" fontWeight={600}>Apache Superset — CRM Dashboard</Typography>
              <Button 
                size="small" 
                onClick={() => openTool(MONITORING_TOOLS.superset.defaultPort, '/superset/dashboard/crm-overview/')}
                endIcon={<OpenInNewIcon />}
              >
                Open in New Tab
              </Button>
            </Box>
            <iframe
              src={`http://${baseUrl}:${MONITORING_TOOLS.superset.defaultPort}/superset/dashboard/crm-overview/?standalone=true`} // NOSONAR - S5332 - http:// URL built from runtime baseUrl variable for local monitoring access
              style={{ width: '100%', height: '700px', border: 'none' }}
              title="Apache Superset"
            />
          </Paper>
        )}

        {showEmbeddedView === 'portainer' && (
          <Paper variant="outlined" sx={{ borderRadius: 2, overflow: 'hidden' }}>
            <Box sx={{ p: 2, bgcolor: 'background.default', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Typography variant="subtitle1" fontWeight={600}>Portainer Dashboard</Typography>
              <Button 
                size="small" 
                onClick={() => openTool(MONITORING_TOOLS.portainer.defaultPort)}
                endIcon={<OpenInNewIcon />}
              >
                Open in New Tab
              </Button>
            </Box>
            <iframe
              src={`http://${baseUrl}:${MONITORING_TOOLS.portainer.defaultPort}/`} // NOSONAR - S5332 - http:// URL built from runtime baseUrl variable for local monitoring access
              style={{ width: '100%', height: '600px', border: 'none' }}
              title="Portainer"
            />
          </Paper>
        )}
      </TabPanel>

      {/* ── Tab 4: Dev Tools & Controls ──────────────────────────────────── */}
      <TabPanel value={tabValue} index={4}>

        {/* ── Row 1: Rate Limiting + JWT Rotation ─────────────────────── */}
        <Grid container spacing={3} sx={{ mb: 3 }}>

          {/* Rate Limiting Card */}
          <Grid item xs={12} md={6}>
            <Card sx={{ height: '100%', border: `1px solid ${rateLimitEnabled === false ? '#ef4444' : '#22c55e'}33` }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 2 }}>
                  <BlockIcon sx={{ color: rateLimitEnabled === false ? 'error.main' : 'success.main', fontSize: 28 }} />
                  <Typography variant="h6" fontWeight={700}>Rate Limiting</Typography>
                  {rateLimitEnabled === null ? (
                    <Chip label="Loading…" size="small" />
                  ) : (
                    <Chip
                      label={rateLimitEnabled ? 'ENABLED' : 'DISABLED'}
                      color={rateLimitEnabled ? 'success' : 'error'}
                      size="small"
                      variant="filled"
                    />
                  )}
                </Box>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                  Toggle API-level rate limiting at runtime without restarting the service.
                  Disabling is useful for bulk data loads and CDT test runs.
                </Typography>
                {rateLimitLastChanged && (
                  <Typography variant="caption" color="text.secondary" display="block" sx={{ mb: 1 }}>
                    Last changed: {new Date(rateLimitLastChanged).toLocaleString()}
                  </Typography>
                )}
                <Divider sx={{ my: 1.5 }} />
                <Stack direction="row" spacing={1.5} alignItems="center" sx={{ mt: 2 }}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={rateLimitEnabled ?? false}
                        onChange={(e) => toggleRateLimit(e.target.checked)}
                        disabled={rateLimitLoading || rateLimitEnabled === null}
                        color="success"
                      />
                    }
                    label={rateLimitEnabled ? 'Rate limiting ON' : 'Rate limiting OFF'}
                  />
                  {rateLimitLoading && <CircularProgress size={18} />}
                  <Box sx={{ flex: 1 }} />
                  <Tooltip title="Refresh status from server">
                    <IconButton size="small" onClick={fetchRateLimitStatus}>
                      <SyncIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </Stack>
              </CardContent>
              <CardActions sx={{ px: 2, pb: 2 }}>
                <Button
                  size="small"
                  variant="outlined"
                  color="error"
                  startIcon={<BlockIcon />}
                  disabled={rateLimitLoading || rateLimitEnabled === false}
                  onClick={() => toggleRateLimit(false)}
                >
                  Disable for Testing
                </Button>
                <Button
                  size="small"
                  variant="outlined"
                  color="success"
                  disabled={rateLimitLoading || rateLimitEnabled === true}
                  onClick={() => toggleRateLimit(true)}
                >
                  Re-enable
                </Button>
              </CardActions>
            </Card>
          </Grid>

          {/* JWT / Revolving Secrets Card */}
          <Grid item xs={12} md={6}>
            <Card sx={{ height: '100%', border: '1px solid #6366f133' }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 2 }}>
                  <VpnKeyIcon sx={{ color: 'primary.main', fontSize: 28 }} />
                  <Typography variant="h6" fontWeight={700}>Revolving JWT Secrets</Typography>
                </Box>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                  Rotate the signing secret used for all JWT access tokens.  
                  <strong> This immediately invalidates every active session.</strong>  
                  Use only when a security incident is suspected.
                </Typography>

                {jwtInfo ? (
                  <Box sx={{ background: 'action.hover', borderRadius: 1, px: 1.5, py: 1, fontFamily: 'monospace', fontSize: 13 }}>
                    {jwtInfo.fingerprint && (
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                        <Typography variant="caption" color="text.secondary">Current fingerprint:</Typography>
                        <Typography variant="caption" fontFamily="monospace">{jwtInfo.fingerprint}</Typography>
                      </Box>
                    )}
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="caption" color="text.secondary">Last rotated:</Typography>
                      <Typography variant="caption">{jwtInfo.lastRotatedAt ? new Date(jwtInfo.lastRotatedAt).toLocaleString() : 'Never'}</Typography>
                    </Box>
                    {jwtInfo.lastRotatedBy && (
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant="caption" color="text.secondary">Rotated by:</Typography>
                        <Typography variant="caption">{jwtInfo.lastRotatedBy}</Typography>
                      </Box>
                    )}
                  </Box>
                ) : (
                  <Skeleton variant="rounded" height={60} />
                )}

                {jwtRotateResult && (
                  <Alert severity={jwtRotateResult.success ? 'success' : 'error'} sx={{ mt: 2, py: 0.5 }}>
                    {jwtRotateResult.message}
                  </Alert>
                )}
              </CardContent>
              <CardActions sx={{ px: 2, pb: 2 }}>
                <Tooltip title="All live sessions will be terminated">
                  <span>
                    <Button
                      size="small"
                      variant="contained"
                      color="warning"
                      startIcon={jwtRotating ? <CircularProgress size={14} color="inherit" /> : <SyncIcon />}
                      disabled={jwtRotating}
                      onClick={() => setJwtConfirmOpen(true)}
                    >
                      {jwtRotating ? 'Rotating…' : 'Rotate JWT Secret'}
                    </Button>
                  </span>
                </Tooltip>
                <Button size="small" variant="text" onClick={fetchJwtInfo}>Refresh Info</Button>
              </CardActions>
            </Card>
          </Grid>
        </Grid>

        {/* ── Row 2: Comprehensive CRUD Tests ──────────────────────────── */}
        <Card sx={{ border: '1px solid #6366f133' }}>
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 1 }}>
              <ScienceIcon sx={{ color: 'secondary.main', fontSize: 28 }} />
              <Typography variant="h6" fontWeight={700}>Comprehensive CRUD Tests (CDT)</Typography>
              {cdtResults.length > 0 && (
                <Chip
                  label={`${cdtResults.reduce((s, b) => s + b.passed, 0)}/${cdtResults.reduce((s, b) => s + b.passed + b.failed, 0)} passed`}
                  color={cdtResults.some(b => b.failed > 0) ? 'warning' : 'success'}
                  size="small"
                />
              )}
            </Box>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Runs {CDT_BATCHES.length} endpoint batches covering all CRM modules.
              Results include per-check HTTP status, latency, and a downloadable HTML report.
            </Typography>

            {/* Progress bar */}
            {(cdtRunning || cdtProgress > 0) && (
              <Box sx={{ mb: 2 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                  <Typography variant="caption">{cdtRunning ? `Running batch ${cdtResults.length}/${CDT_BATCHES.length}…` : 'Complete'}</Typography>
                  <Typography variant="caption">{cdtProgress}%</Typography>
                </Box>
                <LinearProgress variant="determinate" value={cdtProgress} color={cdtRunning ? 'secondary' : 'success'} />
              </Box>
            )}

            {/* Batch accordion results */}
            {cdtResults.map((batch) => (
              <Accordion
                key={batch.id}
                expanded={cdtExpanded === batch.id}
                onChange={(_, exp) => setCdtExpanded(exp ? batch.id : false)}
                sx={{ background: 'transparent', boxShadow: 'none', border: '1px solid', borderColor: batch.failed > 0 ? 'error.main' : 'success.main', borderRadius: '8px !important', mb: 1, '&:before': { display: 'none' } }}
              >
                <AccordionSummary expandIcon={<ExpandMoreIcon />} sx={{ minHeight: 44, '& .MuiAccordionSummary-content': { my: 0.5, alignItems: 'center', gap: 1 } }}>
                  {batch.failed > 0 ? <FailIcon color="error" fontSize="small" /> : <PassIcon color="success" fontSize="small" />}
                  <Typography variant="body2" fontWeight={600} sx={{ flex: 1 }}>{batch.label}</Typography>
                  <Stack direction="row" spacing={0.5} alignItems="center" sx={{ mr: 1 }}>
                    <Chip label={`✓ ${batch.passed}`} size="small" color="success" variant="outlined" sx={{ height: 20, fontSize: 11 }} />
                    {batch.failed > 0 && <Chip label={`✗ ${batch.failed}`} size="small" color="error" variant="outlined" sx={{ height: 20, fontSize: 11 }} />}
                    <Typography variant="caption" color="text.secondary">{(batch.durationMs / 1000).toFixed(2)}s</Typography>
                  </Stack>
                </AccordionSummary>
                <AccordionDetails sx={{ pt: 0 }}>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell sx={{ fontWeight: 700, fontSize: 11 }}>Check</TableCell>
                        <TableCell sx={{ fontWeight: 700, fontSize: 11 }}>Endpoint</TableCell>
                        <TableCell sx={{ fontWeight: 700, fontSize: 11 }}>HTTP</TableCell>
                        <TableCell sx={{ fontWeight: 700, fontSize: 11 }}>ms</TableCell>
                        <TableCell sx={{ fontWeight: 700, fontSize: 11 }}>Status</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {batch.results.map((r, i) => (
                        <TableRow key={i} sx={{ background: r.status === 'fail' ? '#ef44440a' : undefined }}>
                          <TableCell sx={{ fontSize: 12 }}>{r.label}</TableCell>
                          <TableCell sx={{ fontSize: 11, fontFamily: 'monospace', color: 'text.secondary' }}>{r.path}</TableCell>
                          <TableCell sx={{ fontSize: 12 }}>{r.httpStatus ?? '—'}</TableCell>
                          <TableCell sx={{ fontSize: 12 }}>{r.durationMs}</TableCell>
                          <TableCell>
                            {r.status === 'pass'
                              ? <Chip label="PASS" size="small" color="success" sx={{ height: 18, fontSize: 10 }} />
                              : r.status === 'skip'
                                ? <Chip label="SKIP" size="small" color="default" sx={{ height: 18, fontSize: 10 }} />
                                : <Tooltip title={r.error ?? ''}><Chip label="FAIL" size="small" color="error" sx={{ height: 18, fontSize: 10 }} /></Tooltip>
                            }
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </AccordionDetails>
              </Accordion>
            ))}
          </CardContent>
          <CardActions sx={{ px: 2, pb: 2, gap: 1 }}>
            <Button
              variant="contained"
              color="secondary"
              startIcon={cdtRunning ? <CircularProgress size={16} color="inherit" /> : <PlayArrowIcon />}
              disabled={cdtRunning}
              onClick={runCdt}
            >
              {cdtRunning ? 'Running CDT…' : 'Run All Batches'}
            </Button>
            {cdtResults.length > 0 && (
              <Button variant="outlined" onClick={downloadCdtReport}>
                Download HTML Report
              </Button>
            )}
            {cdtResults.length > 0 && (
              <Button variant="text" color="inherit" onClick={() => { setCdtResults([]); setCdtProgress(0); }}>
                Clear
              </Button>
            )}
            {rateLimitEnabled !== false && (
              <Alert severity="warning" sx={{ py: 0, px: 1.5, fontSize: 12, flex: 1 }}>
                Rate limiting is <strong>ON</strong> — disable it above for accurate CDT results.
              </Alert>
            )}
          </CardActions>
        </Card>

      </TabPanel>

      {/* JWT Rotation Confirm Dialog */}
      <Dialog open={jwtConfirmOpen} onClose={() => setJwtConfirmOpen(false)}>
        <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <VpnKeyIcon color="warning" /> Rotate JWT Secret
        </DialogTitle>
        <DialogContent>
          <DialogContentText>
            This will immediately invalidate <strong>all active sessions</strong> including yours.
            Every logged-in user will be forced to re-authenticate.
            <br /><br />
            Only proceed if a security incident has been confirmed or you are performing a scheduled rotation.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setJwtConfirmOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            color="warning"
            onClick={() => { setJwtConfirmOpen(false); rotateJwtSecret(); }}
          >
            Confirm Rotation
          </Button>
        </DialogActions>
      </Dialog>

      {/* Setup Instructions */}
      <Box sx={{ mt: 4 }}>
        <Alert severity="info" sx={{ borderRadius: 2 }}>
          <Typography variant="subtitle1" fontWeight={600}>Service Access (Development)</Typography>
          <Typography variant="body2" sx={{ mt: 1 }}>
            <strong>CRM Login:</strong> admin@crm.local (Port 5000) — <em>see environment config for password</em>
          </Typography>
          <Typography variant="body2" sx={{ mt: 0.5 }}>
            <strong>Uptime Kuma:</strong> admin (Port {MONITORING_TOOLS.uptimeKuma.defaultPort}) — <em>see environment config</em>
          </Typography>
          <Typography variant="body2" sx={{ mt: 0.5 }}>
            <strong>Apache Superset:</strong> admin (Port {MONITORING_TOOLS.superset.defaultPort}) — <em>see environment config</em>
          </Typography>
          <Typography variant="body2" sx={{ mt: 0.5 }}>
            <strong>Meilisearch:</strong> Port 7700 — <em>API key in environment config</em>
          </Typography>
          <Typography variant="body2" sx={{ mt: 0.5 }}>
            <strong>MariaDB:</strong> crm_user (Port 3306, DB: crm_db) — <em>see environment config</em>
          </Typography>
          <Typography variant="body2" sx={{ mt: 0.5 }}>
            <strong>Redis:</strong> Port 6379 — <em>see environment config</em>
          </Typography>
          <Typography variant="body2" sx={{ mt: 0.5 }}>
            <strong>Portainer Agent:</strong> Port 9001 (connect via Portainer CE/EE)
          </Typography>
        </Alert>
      </Box>
    </Box>
  );
};

export default MonitoringDashboard;

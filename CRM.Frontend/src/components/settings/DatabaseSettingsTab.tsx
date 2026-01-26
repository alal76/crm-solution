import React, { useState, useEffect } from 'react';
import { TabPanel } from '../common';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Grid,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormControlLabel,
  Checkbox,
  Alert,
  CircularProgress,
  Divider,
  IconButton,
  Tooltip,
  Tabs,
  Tab,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Switch,
} from '@mui/material';
import {
  Backup as BackupIcon,
  Restore as RestoreIcon,
  Speed as OptimizeIcon,
  Storage as StorageIcon,
  Code as CodeIcon,
  Delete as DeleteIcon,
  Download as DownloadIcon,
  Refresh as RefreshIcon,
  ExpandMore as ExpandMoreIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  CloudSync as MigrateIcon,
  Visibility as VisibilityIcon,
  VisibilityOff as VisibilityOffIcon,
  ArrowForward as ArrowForwardIcon,
  ArrowBack as ArrowBackIcon,
  Check as CheckIcon,
  Close as CloseIcon,
  PlayArrow as PlayArrowIcon,
  TableChart as TableChartIcon,
  ViewList as ViewListIcon,
  List as ListIcon,
  Upload as UploadIcon,
  Schedule as ScheduleIcon,
  Add as AddIcon,
  Edit as EditIcon,
  FolderOpen as FolderIcon,
} from '@mui/icons-material';
import { getApiEndpoint } from '../../config/ports';

interface DatabaseStatus {
  currentProvider: string;
  databaseEngine: string;
  databaseVersion: string;
  serverHost: string;
  serverPort: number;
  databaseName: string;
  userId: string;
  password: string;
  connectionStatus: string;
  databaseSize: string;
  tableCount: number;
  viewCount: number;
  indexCount: number;
  lastBackupDate: string | null;
  tables: TableStat[];
  views: ViewStat[];
  indexes: IndexStat[];
}

interface TableStat {
  name: string;
  recordCount: number;
  rowSize?: string;
  totalSize?: string;
}

interface ViewStat {
  name: string;
  definition?: string;
}

interface IndexStat {
  name: string;
  tableName: string;
  columns?: string;
  isUnique: boolean;
  isPrimaryKey: boolean;
}

interface Backup {
  id: number;
  backupName: string;
  filePath: string;
  fileSizeBytes: number;
  sourceDatabase: string;
  backupType: string;
  createdAt: string;
  description: string;
  isCompressed: boolean;
}

interface DatabaseProvider {
  name: string;
  code: string;
  description: string;
  isCurrentProvider: boolean;
}

interface ConnectionTestResult {
  success: boolean;
  message: string;
  details?: string;
  serverVersion?: string;
}

interface MigrationStep {
  name: string;
  status: string;
  details?: string;
  error?: string;
}

interface MigrationResult {
  success: boolean;
  message?: string;
  errorMessage?: string;
  startTime: string;
  endTime?: string;
  steps: MigrationStep[];
  newConnectionString?: string;
  newProvider?: string;
  configurationInstructions?: string;
}

interface BackupSchedule {
  id: number;
  name: string;
  isEnabled: boolean;
  backupType: string;
  cronExpression: string;
  cronDescription: string;
  backupPath: string;
  retentionDays: number;
  maxBackupsToKeep: number;
  compressBackups: boolean;
  lastBackupAt: string | null;
  nextBackupAt: string | null;
  lastError: string | null;
  successfulBackups: number;
  failedBackups: number;
}

interface StatisticsSchedule {
  isEnabled: boolean;
  intervalMinutes: number;
  lastRefreshed: string | null;
  nextScheduledRefresh: string | null;
  databaseProvider: string;
}

interface StatisticsRefreshResult {
  startTime: string;
  endTime: string;
  success: boolean;
  databaseProvider: string;
  command: string;
  message: string;
  tablesAnalyzed: string[];
}

function DatabaseSettingsTab() {
  const [tabValue, setTabValue] = useState(0);
  const [status, setStatus] = useState<DatabaseStatus | null>(null);
  const [backups, setBackups] = useState<Backup[]>([]);
  const [providers, setProviders] = useState<DatabaseProvider[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [showPassword, setShowPassword] = useState(false);
  
  // Dialog states
  const [backupDialogOpen, setBackupDialogOpen] = useState(false);
  const [migrationDialogOpen, setMigrationDialogOpen] = useState(false);
  const [clearDataDialogOpen, setClearDataDialogOpen] = useState(false);
  const [scriptDialogOpen, setScriptDialogOpen] = useState(false);
  const [generatedScript, setGeneratedScript] = useState('');
  const [scriptTitle, setScriptTitle] = useState('');
  
  // Backup schedule states
  const [schedules, setSchedules] = useState<BackupSchedule[]>([]);
  const [scheduleDialogOpen, setScheduleDialogOpen] = useState(false);
  const [uploadDialogOpen, setUploadDialogOpen] = useState(false);
  const [editingSchedule, setEditingSchedule] = useState<BackupSchedule | null>(null);
  const [backupPath, setBackupPath] = useState('DatabaseBackups');
  const [scheduleForm, setScheduleForm] = useState({
    name: '',
    isEnabled: true,
    backupType: 'Full',
    cronExpression: '0 2 * * *',
    backupPath: 'DatabaseBackups',
    retentionDays: 30,
    maxBackupsToKeep: 10,
    compressBackups: true,
  });
  
  // Statistics schedule states
  const [statisticsSchedule, setStatisticsSchedule] = useState<StatisticsSchedule | null>(null);
  const [statisticsRefreshing, setStatisticsRefreshing] = useState(false);
  const [statisticsScheduleForm, setStatisticsScheduleForm] = useState({
    isEnabled: false,
    intervalMinutes: 60,
  });
  
  // Migration Wizard states
  const [wizardStep, setWizardStep] = useState(0);
  const [wizardOpen, setWizardOpen] = useState(false);
  const [connectionTest, setConnectionTest] = useState<ConnectionTestResult | null>(null);
  const [migrationResult, setMigrationResult] = useState<MigrationResult | null>(null);
  const [migrating, setMigrating] = useState(false);
  const [wizardForm, setWizardForm] = useState({
    targetProvider: 'mariadb',
    host: 'localhost',
    port: 3306,
    database: 'crm_db',
    userId: '',
    password: '',
    migrateData: true,
    createBackup: true,
  });
  
  // Form states
  const [backupForm, setBackupForm] = useState({ backupName: '', backupType: 'Full', description: '' });
  const [migrationForm, setMigrationForm] = useState({
    targetDatabase: 'postgresql',
    includeData: true,
    includeIndexes: true,
    includeSeedData: false,
  });
  const [clearDataConfirmation, setClearDataConfirmation] = useState('');

  // Note: localStorage is used for token storage. For enhanced security in production,
  // consider using httpOnly cookies managed by the backend.
  const getToken = () => localStorage.getItem('accessToken');

  const databaseOptions = [
    { code: 'mariadb', name: 'MariaDB', port: 3306, description: 'Open-source MySQL fork' },
    { code: 'mysql', name: 'MySQL', port: 3306, description: 'Popular open-source database' },
    { code: 'postgresql', name: 'PostgreSQL', port: 5432, description: 'Advanced open-source database' },
    { code: 'sqlserver', name: 'SQL Server', port: 1433, description: 'Microsoft enterprise database' },
    { code: 'oracle', name: 'Oracle', port: 1521, description: 'Enterprise-grade database' },
    { code: 'mongodb', name: 'MongoDB', port: 27017, description: 'Document-based NoSQL database' },
  ];

  useEffect(() => {
    fetchDatabaseStatus();
    fetchBackups();
    fetchProviders();
    fetchSchedules();
    fetchStatisticsSchedule();
  }, []);

  const fetchSchedules = async () => {
    try {
      const response = await fetch(getApiEndpoint('/adminsettings/database/schedules'), {
        headers: { 'Authorization': `Bearer ${getToken()}` }
      });
      if (response.ok) {
        setSchedules(await response.json());
      }
    } catch (err) {
      console.error('Failed to fetch backup schedules', err);
    }
  };

  const fetchStatisticsSchedule = async () => {
    try {
      const response = await fetch(getApiEndpoint('/database/statistics-schedule'), {
        headers: { 'Authorization': `Bearer ${getToken()}` }
      });
      if (response.ok) {
        const schedule = await response.json();
        setStatisticsSchedule(schedule);
        setStatisticsScheduleForm({
          isEnabled: schedule.isEnabled,
          intervalMinutes: schedule.intervalMinutes,
        });
      }
    } catch (err) {
      console.error('Failed to fetch statistics schedule', err);
    }
  };

  const handleRefreshStatistics = async () => {
    setStatisticsRefreshing(true);
    setError(null);
    try {
      const response = await fetch(getApiEndpoint('/database/refresh-statistics'), {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${getToken()}` }
      });
      if (response.ok) {
        const result: StatisticsRefreshResult = await response.json();
        setSuccess(`Statistics refreshed: ${result.tablesAnalyzed.length} table(s) analyzed using ${result.command}`);
        // Refresh status to get updated table stats
        fetchDatabaseStatus();
        fetchStatisticsSchedule();
      } else {
        const err = await response.json();
        setError(err.message || 'Failed to refresh statistics');
      }
    } catch (err) {
      setError('Failed to refresh statistics');
    } finally {
      setStatisticsRefreshing(false);
    }
  };

  const handleUpdateStatisticsSchedule = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetch(getApiEndpoint('/database/statistics-schedule'), {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${getToken()}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(statisticsScheduleForm)
      });
      if (response.ok) {
        const schedule = await response.json();
        setStatisticsSchedule(schedule);
        setSuccess('Statistics schedule updated successfully');
      } else {
        const err = await response.json();
        setError(err.message || 'Failed to update statistics schedule');
      }
    } catch (err) {
      setError('Failed to update statistics schedule');
    } finally {
      setLoading(false);
    }
  };

  const fetchBackupSettings = async () => {
    try {
      const response = await fetch(getApiEndpoint('/adminsettings/database/backup-settings'), {
        headers: { 'Authorization': `Bearer ${getToken()}` }
      });
      if (response.ok) {
        const settings = await response.json();
        setBackupPath(settings.defaultBackupPath);
        setSchedules(settings.schedules);
      }
    } catch (err) {
      console.error('Failed to fetch backup settings', err);
    }
  };

  const fetchDatabaseStatus = async () => {
    try {
      const response = await fetch(getApiEndpoint('/database/status'), {
        headers: { 'Authorization': `Bearer ${getToken()}` }
      });
      if (response.ok) {
        setStatus(await response.json());
      } else if (response.status === 401) {
        setError('Session expired. Please log in again.');
      } else {
        setError('Failed to fetch database status. Please try again.');
      }
    } catch (err) {
      console.error('Failed to fetch database status', err);
      setError('Unable to connect to server. Please check your connection.');
    }
  };

  const fetchBackups = async () => {
    try {
      const response = await fetch(getApiEndpoint('/database/backups'), {
        headers: { 'Authorization': `Bearer ${getToken()}` }
      });
      if (response.ok) {
        setBackups(await response.json());
      } else if (response.status !== 401) {
        // Don't show error for 401 - already handled by fetchDatabaseStatus
        console.error('Failed to fetch backups:', response.status);
      }
    } catch (err) {
      console.error('Failed to fetch backups', err);
    }
  };

  const fetchProviders = async () => {
    try {
      const response = await fetch(getApiEndpoint('/database/providers'), {
        headers: { 'Authorization': `Bearer ${getToken()}` }
      });
      if (response.ok) {
        setProviders(await response.json());
      }
    } catch (err) {
      console.error('Failed to fetch providers', err);
    }
  };

  // Migration Wizard Functions
  const handleProviderChange = (provider: string) => {
    const selected = databaseOptions.find(p => p.code === provider);
    setWizardForm({
      ...wizardForm,
      targetProvider: provider,
      port: selected?.port || 3306,
    });
    setConnectionTest(null);
  };

  const handleTestConnection = async () => {
    setLoading(true);
    setConnectionTest(null);
    try {
      const response = await fetch(getApiEndpoint('/database/test-connection'), {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${getToken()}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          provider: wizardForm.targetProvider,
          host: wizardForm.host,
          port: wizardForm.port,
          database: wizardForm.database,
          userId: wizardForm.userId,
          password: wizardForm.password,
        })
      });
      const result = await response.json();
      setConnectionTest(result);
    } catch (err: any) {
      setConnectionTest({ success: false, message: err.message });
    } finally {
      setLoading(false);
    }
  };

  const handleStartMigration = async () => {
    setMigrating(true);
    setMigrationResult(null);
    try {
      const response = await fetch(getApiEndpoint('/database/migrate'), {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${getToken()}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          targetProvider: wizardForm.targetProvider,
          host: wizardForm.host,
          port: wizardForm.port,
          database: wizardForm.database,
          userId: wizardForm.userId,
          password: wizardForm.password,
          migrateData: wizardForm.migrateData,
          createBackup: wizardForm.createBackup,
        })
      });
      const result = await response.json();
      setMigrationResult(result);
      if (result.success) {
        setWizardStep(3); // Go to completion step
      }
    } catch (err: any) {
      setMigrationResult({ 
        success: false, 
        errorMessage: err.message,
        steps: [],
        startTime: new Date().toISOString()
      });
    } finally {
      setMigrating(false);
    }
  };

  const getStepIcon = (stepStatus: string) => {
    switch (stepStatus) {
      case 'completed': return <CheckCircleIcon color="success" />;
      case 'failed': return <ErrorIcon color="error" />;
      case 'in-progress': return <CircularProgress size={20} />;
      default: return null;
    }
  };

  const handleCreateBackup = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetch(getApiEndpoint('/database/backup'), {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${getToken()}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(backupForm)
      });
      if (response.ok) {
        setSuccess('Backup created successfully');
        setBackupDialogOpen(false);
        fetchBackups();
        fetchDatabaseStatus();
      } else {
        throw new Error('Failed to create backup');
      }
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleRestoreBackup = async (backupId: number) => {
    if (!window.confirm('Are you sure you want to restore this backup? This will overwrite current data.')) return;
    
    setLoading(true);
    try {
      const response = await fetch(getApiEndpoint(`/database/restore/${backupId}`), {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${getToken()}` }
      });
      if (response.ok) {
        setSuccess('Restore initiated successfully');
      }
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleDownloadBackup = async (backupId: number, backupName: string) => {
    try {
      const response = await fetch(getApiEndpoint(`/adminsettings/database/backups/${backupId}/download`), {
        headers: { 'Authorization': `Bearer ${getToken()}` }
      });
      if (response.ok) {
        const blob = await response.blob();
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${backupName}.sql`;
        a.click();
        URL.revokeObjectURL(url);
        setSuccess('Backup downloaded successfully');
      } else {
        throw new Error('Failed to download backup');
      }
    } catch (err: any) {
      setError(err.message);
    }
  };

  const handleUploadBackup = async (file: File) => {
    setLoading(true);
    try {
      const formData = new FormData();
      formData.append('file', file);
      formData.append('description', `Uploaded: ${file.name}`);
      formData.append('sourceDatabase', 'Unknown');

      const response = await fetch(getApiEndpoint('/adminsettings/database/backups/upload'), {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${getToken()}` },
        body: formData
      });

      if (response.ok) {
        setSuccess('Backup uploaded successfully');
        setUploadDialogOpen(false);
        fetchBackups();
      } else {
        throw new Error('Failed to upload backup');
      }
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleRestoreFromUpload = async (file: File) => {
    if (!window.confirm('Are you sure you want to restore from this file? This will overwrite current data.')) return;
    
    setLoading(true);
    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await fetch(getApiEndpoint('/adminsettings/database/restore/upload'), {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${getToken()}` },
        body: formData
      });

      if (response.ok) {
        setSuccess('Database restore from uploaded file initiated');
        setUploadDialogOpen(false);
      } else {
        throw new Error('Failed to restore from uploaded file');
      }
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  // Schedule handlers
  const handleCreateSchedule = async () => {
    setLoading(true);
    try {
      const response = await fetch(getApiEndpoint('/adminsettings/database/schedules'), {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${getToken()}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(scheduleForm)
      });
      if (response.ok) {
        setSuccess('Backup schedule created successfully');
        setScheduleDialogOpen(false);
        fetchSchedules();
        resetScheduleForm();
      } else {
        throw new Error('Failed to create schedule');
      }
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateSchedule = async () => {
    if (!editingSchedule) return;
    setLoading(true);
    try {
      const response = await fetch(getApiEndpoint(`/adminsettings/database/schedules/${editingSchedule.id}`), {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${getToken()}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(scheduleForm)
      });
      if (response.ok) {
        setSuccess('Backup schedule updated successfully');
        setScheduleDialogOpen(false);
        setEditingSchedule(null);
        fetchSchedules();
        resetScheduleForm();
      } else {
        throw new Error('Failed to update schedule');
      }
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteSchedule = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this schedule?')) return;
    try {
      const response = await fetch(getApiEndpoint(`/adminsettings/database/schedules/${id}`), {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${getToken()}` }
      });
      if (response.ok) {
        setSuccess('Schedule deleted successfully');
        fetchSchedules();
      }
    } catch (err: any) {
      setError(err.message);
    }
  };

  const handleToggleSchedule = async (id: number, enabled: boolean) => {
    try {
      const response = await fetch(getApiEndpoint(`/adminsettings/database/schedules/${id}/toggle`), {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${getToken()}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ enabled })
      });
      if (response.ok) {
        fetchSchedules();
      }
    } catch (err: any) {
      setError(err.message);
    }
  };

  const handleRunScheduleNow = async (id: number) => {
    try {
      setLoading(true);
      const response = await fetch(getApiEndpoint(`/adminsettings/database/schedules/${id}/run`), {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${getToken()}` }
      });
      if (response.ok) {
        setSuccess('Backup job triggered successfully');
        fetchSchedules();
        fetchBackups();
      }
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateBackupPath = async () => {
    try {
      const response = await fetch(getApiEndpoint('/adminsettings/database/backup-path'), {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${getToken()}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ path: backupPath })
      });
      if (response.ok) {
        setSuccess('Backup path updated successfully');
      }
    } catch (err: any) {
      setError(err.message);
    }
  };

  const openEditSchedule = (schedule: BackupSchedule) => {
    setEditingSchedule(schedule);
    setScheduleForm({
      name: schedule.name,
      isEnabled: schedule.isEnabled,
      backupType: schedule.backupType,
      cronExpression: schedule.cronExpression,
      backupPath: schedule.backupPath,
      retentionDays: schedule.retentionDays,
      maxBackupsToKeep: schedule.maxBackupsToKeep,
      compressBackups: schedule.compressBackups,
    });
    setScheduleDialogOpen(true);
  };

  const resetScheduleForm = () => {
    setScheduleForm({
      name: '',
      isEnabled: true,
      backupType: 'Full',
      cronExpression: '0 2 * * *',
      backupPath: 'DatabaseBackups',
      retentionDays: 30,
      maxBackupsToKeep: 10,
      compressBackups: true,
    });
    setEditingSchedule(null);
  };

  const cronPresets = [
    { label: 'Daily at 2 AM', value: '0 2 * * *' },
    { label: 'Daily at midnight', value: '0 0 * * *' },
    { label: 'Every 6 hours', value: '0 */6 * * *' },
    { label: 'Every 12 hours', value: '0 */12 * * *' },
    { label: 'Weekly (Sunday 2 AM)', value: '0 2 * * 0' },
    { label: 'Monthly (1st at 2 AM)', value: '0 2 1 * *' },
  ];

  const handleOptimize = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetch(getApiEndpoint('/database/optimize'), {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${getToken()}` }
      });
      if (response.ok) {
        const result = await response.json();
        setSuccess(`Database optimized: ${result.operations.join(', ')}`);
      }
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleRebuildIndexes = async () => {
    setLoading(true);
    try {
      const response = await fetch(getApiEndpoint('/database/rebuild-indexes'), {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${getToken()}` }
      });
      if (response.ok) {
        setSuccess('Indexes rebuilt successfully');
      }
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleGenerateSeedScript = async () => {
    setLoading(true);
    try {
      const response = await fetch(getApiEndpoint('/database/generate-seed-script'), {
        headers: { 'Authorization': `Bearer ${getToken()}` }
      });
      if (response.ok) {
        const result = await response.json();
        setGeneratedScript(result.script);
        setScriptTitle('Seed Script');
        setScriptDialogOpen(true);
      }
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleGenerateMigrationScript = async () => {
    setLoading(true);
    try {
      const response = await fetch(getApiEndpoint('/database/generate-migration-script'), {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${getToken()}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(migrationForm)
      });
      if (response.ok) {
        const result = await response.json();
        setGeneratedScript(result.script);
        setScriptTitle(`Migration Script for ${result.targetDatabase}`);
        setScriptDialogOpen(true);
        setMigrationDialogOpen(false);
      }
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleClearData = async () => {
    if (clearDataConfirmation !== 'DELETE_ALL_DATA') {
      setError('Please type DELETE_ALL_DATA to confirm');
      return;
    }
    
    setLoading(true);
    try {
      const response = await fetch(getApiEndpoint('/database/clear-data'), {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${getToken()}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ confirmationCode: clearDataConfirmation })
      });
      if (response.ok) {
        setSuccess('All data cleared successfully');
        setClearDataDialogOpen(false);
        setClearDataConfirmation('');
        fetchDatabaseStatus();
      }
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleReseed = async () => {
    if (!window.confirm('Reseed the database with initial data?')) return;
    
    setLoading(true);
    try {
      const response = await fetch(getApiEndpoint('/database/reseed'), {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${getToken()}` }
      });
      if (response.ok) {
        setSuccess('Database reseeded successfully');
        fetchDatabaseStatus();
      }
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const downloadScript = () => {
    const blob = new Blob([generatedScript], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${scriptTitle.toLowerCase().replace(/\s+/g, '_')}_${new Date().toISOString().split('T')[0]}.sql`;
    a.click();
    URL.revokeObjectURL(url);
  };

  const formatBytes = (bytes: number) => {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  return (
    <Box>
      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess(null)}>{success}</Alert>}
      
      <Tabs value={tabValue} onChange={(_, v) => setTabValue(v)} sx={{ mb: 2 }}>
        <Tab label="Overview" icon={<StorageIcon />} iconPosition="start" />
        <Tab label="Backup & Restore" icon={<BackupIcon />} iconPosition="start" />
        <Tab label="Maintenance" icon={<OptimizeIcon />} iconPosition="start" />
        <Tab label="Migration" icon={<MigrateIcon />} iconPosition="start" />
      </Tabs>

      {/* Overview Tab */}
      <TabPanel value={tabValue} index={0}>
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                  <Typography variant="h6">Database Connection</Typography>
                  <IconButton onClick={fetchDatabaseStatus} size="small">
                    <RefreshIcon />
                  </IconButton>
                </Box>
                
                {status ? (
                  <Box>
                    <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                      <Chip
                        icon={status.connectionStatus === 'Connected' ? <CheckCircleIcon /> : <ErrorIcon />}
                        label={status.connectionStatus}
                        color={status.connectionStatus === 'Connected' ? 'success' : 'error'}
                        sx={{ mr: 1 }}
                      />
                      <Chip label={status.currentProvider} color="primary" />
                    </Box>
                    
                    <TableContainer>
                      <Table size="small">
                        <TableBody>
                          <TableRow>
                            <TableCell sx={{ fontWeight: 'bold', width: '40%' }}>Database Engine</TableCell>
                            <TableCell>{status.databaseEngine || status.currentProvider}</TableCell>
                          </TableRow>
                          <TableRow>
                            <TableCell sx={{ fontWeight: 'bold' }}>Version</TableCell>
                            <TableCell>{status.databaseVersion || 'Unknown'}</TableCell>
                          </TableRow>
                          <TableRow>
                            <TableCell sx={{ fontWeight: 'bold' }}>Server Host</TableCell>
                            <TableCell>{status.serverHost || 'localhost'}</TableCell>
                          </TableRow>
                          <TableRow>
                            <TableCell sx={{ fontWeight: 'bold' }}>Port</TableCell>
                            <TableCell>{status.serverPort || 'N/A'}</TableCell>
                          </TableRow>
                          <TableRow>
                            <TableCell sx={{ fontWeight: 'bold' }}>Database Name</TableCell>
                            <TableCell>{status.databaseName || 'crm_db'}</TableCell>
                          </TableRow>
                          <TableRow>
                            <TableCell sx={{ fontWeight: 'bold' }}>User ID</TableCell>
                            <TableCell>{status.userId || 'N/A'}</TableCell>
                          </TableRow>
                          <TableRow>
                            <TableCell sx={{ fontWeight: 'bold' }}>Password</TableCell>
                            <TableCell>
                              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                                {showPassword ? status.password : '••••••••••••'}
                                <IconButton size="small" onClick={() => setShowPassword(!showPassword)}>
                                  {showPassword ? <VisibilityOffIcon fontSize="small" /> : <VisibilityIcon fontSize="small" />}
                                </IconButton>
                              </Box>
                            </TableCell>
                          </TableRow>
                        </TableBody>
                      </Table>
                    </TableContainer>
                  </Box>
                ) : (
                  <CircularProgress />
                )}
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} md={6}>
            <Card sx={{ mb: 2 }}>
              <CardContent>
                <Typography variant="h6" sx={{ mb: 2 }}>Database Statistics</Typography>
                {status ? (
                  <Grid container spacing={2}>
                    <Grid item xs={4}>
                      <Typography variant="body2" color="text.secondary">Database Size</Typography>
                      <Typography variant="h5">{status.databaseSize}</Typography>
                    </Grid>
                    <Grid item xs={4}>
                      <Typography variant="body2" color="text.secondary">Tables</Typography>
                      <Typography variant="h5">{status.tableCount}</Typography>
                    </Grid>
                    <Grid item xs={4}>
                      <Typography variant="body2" color="text.secondary">Views</Typography>
                      <Typography variant="h5">{status.viewCount || 0}</Typography>
                    </Grid>
                    <Grid item xs={4}>
                      <Typography variant="body2" color="text.secondary">Indexes</Typography>
                      <Typography variant="h5">{status.indexCount || 0}</Typography>
                    </Grid>
                    <Grid item xs={8}>
                      <Typography variant="body2" color="text.secondary">Last Backup</Typography>
                      <Typography variant="body1">
                        {status.lastBackupDate 
                          ? new Date(status.lastBackupDate).toLocaleString() 
                          : 'Never'}
                      </Typography>
                    </Grid>
                  </Grid>
                ) : (
                  <CircularProgress />
                )}
              </CardContent>
            </Card>
            
            <Card>
              <CardContent>
                <Typography variant="h6" sx={{ mb: 2 }}>Quick Actions</Typography>
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                  <Button 
                    variant="outlined" 
                    startIcon={<MigrateIcon />}
                    onClick={() => { setWizardStep(0); setWizardOpen(true); setConnectionTest(null); setMigrationResult(null); }}
                    fullWidth
                  >
                    Switch Database
                  </Button>
                  <Button 
                    variant="outlined" 
                    startIcon={<BackupIcon />}
                    onClick={() => setBackupDialogOpen(true)}
                    fullWidth
                  >
                    Create Backup
                  </Button>
                </Box>
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center' }}>
                    <TableChartIcon sx={{ mr: 1 }} />
                    <Typography variant="h6">Table Statistics</Typography>
                  </Box>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    {statisticsSchedule?.lastRefreshed && (
                      <Typography variant="caption" color="text.secondary" sx={{ mr: 1 }}>
                        Last refreshed: {new Date(statisticsSchedule.lastRefreshed).toLocaleString()}
                      </Typography>
                    )}
                    <Tooltip title="Refresh statistics now">
                      <Button
                        variant="outlined"
                        size="small"
                        startIcon={statisticsRefreshing ? <CircularProgress size={16} /> : <RefreshIcon />}
                        onClick={handleRefreshStatistics}
                        disabled={statisticsRefreshing}
                      >
                        Refresh
                      </Button>
                    </Tooltip>
                  </Box>
                </Box>
                
                {/* Statistics Refresh Schedule */}
                <Box sx={{ mb: 2, p: 2, bgcolor: 'background.default', borderRadius: 1 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 2 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                      <ScheduleIcon color="action" />
                      <Typography variant="subtitle2">Auto-Refresh Schedule</Typography>
                      <FormControlLabel
                        control={
                          <Switch
                            size="small"
                            checked={statisticsScheduleForm.isEnabled}
                            onChange={(e) => setStatisticsScheduleForm(prev => ({
                              ...prev,
                              isEnabled: e.target.checked
                            }))}
                          />
                        }
                        label={statisticsScheduleForm.isEnabled ? 'Enabled' : 'Disabled'}
                      />
                    </Box>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                      <FormControl size="small" sx={{ minWidth: 150 }}>
                        <InputLabel>Refresh Interval</InputLabel>
                        <Select
                          value={statisticsScheduleForm.intervalMinutes}
                          label="Refresh Interval"
                          onChange={(e) => setStatisticsScheduleForm(prev => ({
                            ...prev,
                            intervalMinutes: Number(e.target.value)
                          }))}
                        >
                          <MenuItem value={15}>Every 15 mins</MenuItem>
                          <MenuItem value={30}>Every 30 mins</MenuItem>
                          <MenuItem value={60}>Every hour</MenuItem>
                          <MenuItem value={120}>Every 2 hours</MenuItem>
                          <MenuItem value={360}>Every 6 hours</MenuItem>
                          <MenuItem value={720}>Every 12 hours</MenuItem>
                          <MenuItem value={1440}>Once a day</MenuItem>
                        </Select>
                      </FormControl>
                      <Button
                        variant="contained"
                        size="small"
                        onClick={handleUpdateStatisticsSchedule}
                        disabled={loading || (
                          statisticsSchedule?.isEnabled === statisticsScheduleForm.isEnabled &&
                          statisticsSchedule?.intervalMinutes === statisticsScheduleForm.intervalMinutes
                        )}
                      >
                        Save Schedule
                      </Button>
                    </Box>
                  </Box>
                  {statisticsSchedule?.nextScheduledRefresh && statisticsSchedule.isEnabled && (
                    <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
                      Next scheduled refresh: {new Date(statisticsSchedule.nextScheduledRefresh).toLocaleString()}
                    </Typography>
                  )}
                  <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 0.5 }}>
                    Database: {statisticsSchedule?.databaseProvider?.toUpperCase() || status?.currentProvider?.toUpperCase() || 'Unknown'}
                    {' | Command: '}
                    {(() => {
                      const provider = statisticsSchedule?.databaseProvider || status?.currentProvider?.toLowerCase();
                      switch (provider) {
                        case 'mysql':
                        case 'mariadb': return 'ANALYZE TABLE';
                        case 'postgresql': return 'ANALYZE';
                        case 'sqlserver': return 'sp_updatestats';
                        case 'oracle': return 'GATHER_SCHEMA_STATS';
                        default: return 'ANALYZE';
                      }
                    })()}
                  </Typography>
                </Box>
                
                {status?.tables && status.tables.length > 0 ? (
                  <TableContainer sx={{ maxHeight: 300 }}>
                    <Table size="small" stickyHeader>
                      <TableHead>
                        <TableRow>
                          <TableCell>Table Name</TableCell>
                          <TableCell align="right">Records</TableCell>
                          <TableCell align="right">Row Size</TableCell>
                          <TableCell align="right">Total Size</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {status.tables.map((table) => (
                          <TableRow key={table.name}>
                            <TableCell>{table.name}</TableCell>
                            <TableCell align="right">{table.recordCount.toLocaleString()}</TableCell>
                            <TableCell align="right">{table.rowSize || '-'}</TableCell>
                            <TableCell align="right">{table.totalSize || '-'}</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                ) : (
                  <Typography variant="body2" color="text.secondary">No tables found</Typography>
                )}
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <ViewListIcon sx={{ mr: 1 }} />
                  <Typography variant="h6">Views ({status?.viewCount || 0})</Typography>
                </Box>
                {status?.views && status.views.length > 0 ? (
                  <TableContainer sx={{ maxHeight: 250 }}>
                    <Table size="small" stickyHeader>
                      <TableHead>
                        <TableRow>
                          <TableCell>View Name</TableCell>
                          <TableCell>Definition</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {status.views.map((view) => (
                          <TableRow key={view.name}>
                            <TableCell>{view.name}</TableCell>
                            <TableCell sx={{ maxWidth: 300, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                              <Typography variant="caption" title={view.definition || ''}>
                                {view.definition ? (view.definition.length > 50 ? view.definition.substring(0, 50) + '...' : view.definition) : '-'}
                              </Typography>
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                ) : (
                  <Typography variant="body2" color="text.secondary">No views found</Typography>
                )}
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <ListIcon sx={{ mr: 1 }} />
                  <Typography variant="h6">Indexes ({status?.indexCount || 0})</Typography>
                </Box>
                {status?.indexes && status.indexes.length > 0 ? (
                  <TableContainer sx={{ maxHeight: 250 }}>
                    <Table size="small" stickyHeader>
                      <TableHead>
                        <TableRow>
                          <TableCell>Index Name</TableCell>
                          <TableCell>Table</TableCell>
                          <TableCell>Columns</TableCell>
                          <TableCell align="center">Unique</TableCell>
                          <TableCell align="center">PK</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {status.indexes.map((index, idx) => (
                          <TableRow key={`${index.name}-${idx}`}>
                            <TableCell>{index.name}</TableCell>
                            <TableCell>{index.tableName}</TableCell>
                            <TableCell sx={{ maxWidth: 150, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                              <Typography variant="caption" title={index.columns || ''}>
                                {index.columns || '-'}
                              </Typography>
                            </TableCell>
                            <TableCell align="center">{index.isUnique ? '✓' : ''}</TableCell>
                            <TableCell align="center">{index.isPrimaryKey ? '✓' : ''}</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                ) : (
                  <Typography variant="body2" color="text.secondary">No indexes found</Typography>
                )}
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      </TabPanel>

      {/* Backup & Restore Tab */}
      <TabPanel value={tabValue} index={1}>
        {/* Backup Path Configuration */}
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>Backup Storage Location</Typography>
            <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
              <TextField
                label="Backup Path"
                value={backupPath}
                onChange={(e) => setBackupPath(e.target.value)}
                fullWidth
                size="small"
                helperText="Directory where backups will be stored"
                InputProps={{
                  startAdornment: <FolderIcon sx={{ mr: 1, color: 'text.secondary' }} />
                }}
              />
              <Button
                variant="outlined"
                onClick={handleUpdateBackupPath}
                sx={{ minWidth: 100 }}
              >
                Update
              </Button>
            </Box>
          </CardContent>
        </Card>

        {/* Manual Backup Actions */}
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>Manual Backup</Typography>
            <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
              <Button
                variant="contained"
                startIcon={<BackupIcon />}
                onClick={() => setBackupDialogOpen(true)}
                sx={{ backgroundColor: '#06A77D' }}
              >
                Create Full Backup
              </Button>
              <Button
                variant="outlined"
                startIcon={<UploadIcon />}
                onClick={() => setUploadDialogOpen(true)}
              >
                Upload Backup
              </Button>
              <Button
                variant="outlined"
                startIcon={<RestoreIcon />}
                onClick={handleReseed}
              >
                Reseed Database
              </Button>
            </Box>
          </CardContent>
        </Card>

        {/* Backup Schedules */}
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h6">
                <ScheduleIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                Backup Schedules
              </Typography>
              <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={() => { resetScheduleForm(); setScheduleDialogOpen(true); }}
                size="small"
              >
                Add Schedule
              </Button>
            </Box>
            
            {schedules.length === 0 ? (
              <Alert severity="info">
                No backup schedules configured. Create a schedule to automate your backups.
              </Alert>
            ) : (
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Name</TableCell>
                      <TableCell>Type</TableCell>
                      <TableCell>Schedule</TableCell>
                      <TableCell>Path</TableCell>
                      <TableCell>Last Backup</TableCell>
                      <TableCell>Next Backup</TableCell>
                      <TableCell>Status</TableCell>
                      <TableCell>Actions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {schedules.map((schedule) => (
                      <TableRow key={schedule.id}>
                        <TableCell>{schedule.name}</TableCell>
                        <TableCell>
                          <Chip label={schedule.backupType} size="small" color={schedule.backupType === 'Full' ? 'primary' : 'default'} />
                        </TableCell>
                        <TableCell>
                          <Typography variant="caption">{schedule.cronDescription || schedule.cronExpression}</Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="caption" sx={{ maxWidth: 150, display: 'block', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                            {schedule.backupPath}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          {schedule.lastBackupAt ? new Date(schedule.lastBackupAt).toLocaleString() : '-'}
                        </TableCell>
                        <TableCell>
                          {schedule.nextBackupAt ? new Date(schedule.nextBackupAt).toLocaleString() : '-'}
                        </TableCell>
                        <TableCell>
                          <Switch
                            checked={schedule.isEnabled}
                            onChange={(e) => handleToggleSchedule(schedule.id, e.target.checked)}
                            size="small"
                          />
                          {schedule.lastError && (
                            <Tooltip title={schedule.lastError}>
                              <ErrorIcon color="error" sx={{ fontSize: 16, ml: 1 }} />
                            </Tooltip>
                          )}
                        </TableCell>
                        <TableCell>
                          <Tooltip title="Run Now">
                            <IconButton size="small" onClick={() => handleRunScheduleNow(schedule.id)}>
                              <PlayArrowIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Edit">
                            <IconButton size="small" onClick={() => openEditSchedule(schedule)}>
                              <EditIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete">
                            <IconButton size="small" onClick={() => handleDeleteSchedule(schedule.id)}>
                              <DeleteIcon />
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </CardContent>
        </Card>

        {/* Backup History */}
        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h6">Backup History</Typography>
              <IconButton onClick={fetchBackups} size="small">
                <RefreshIcon />
              </IconButton>
            </Box>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Name</TableCell>
                    <TableCell>Type</TableCell>
                    <TableCell>Size</TableCell>
                    <TableCell>Created</TableCell>
                    <TableCell>Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {backups.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={5} align="center">No backups found</TableCell>
                    </TableRow>
                  ) : (
                    backups.map((backup) => (
                      <TableRow key={backup.id}>
                        <TableCell>{backup.backupName}</TableCell>
                        <TableCell>
                          <Chip 
                            label={backup.backupType} 
                            size="small" 
                            color={backup.backupType === 'Full' ? 'primary' : backup.backupType === 'Uploaded' ? 'secondary' : 'default'}
                          />
                        </TableCell>
                        <TableCell>{formatBytes(backup.fileSizeBytes)}</TableCell>
                        <TableCell>{new Date(backup.createdAt).toLocaleString()}</TableCell>
                        <TableCell>
                          <Tooltip title="Restore">
                            <IconButton size="small" onClick={() => handleRestoreBackup(backup.id)}>
                              <RestoreIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Download">
                            <IconButton size="small" onClick={() => handleDownloadBackup(backup.id, backup.backupName)}>
                              <DownloadIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete">
                            <IconButton size="small" onClick={async () => {
                              if (!window.confirm('Delete this backup?')) return;
                              try {
                                await fetch(getApiEndpoint(`/adminsettings/database/backups/${backup.id}`), {
                                  method: 'DELETE',
                                  headers: { 'Authorization': `Bearer ${getToken()}` }
                                });
                                fetchBackups();
                              } catch (err) {
                                setError('Failed to delete backup');
                              }
                            }}>
                              <DeleteIcon />
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>
      </TabPanel>

      {/* Maintenance Tab */}
      <TabPanel value={tabValue} index={2}>
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" sx={{ mb: 2 }}>Database Optimization</Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                  Optimize database performance by running VACUUM and ANALYZE commands.
                  This reclaims space and updates query statistics.
                </Typography>
                <Button
                  variant="contained"
                  startIcon={loading ? <CircularProgress size={20} /> : <OptimizeIcon />}
                  onClick={handleOptimize}
                  disabled={loading}
                  sx={{ mr: 2 }}
                >
                  Optimize Database
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<RefreshIcon />}
                  onClick={handleRebuildIndexes}
                  disabled={loading}
                >
                  Rebuild Indexes
                </Button>
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" sx={{ mb: 2 }}>Data Management</Typography>
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                  <Button
                    variant="outlined"
                    startIcon={<CodeIcon />}
                    onClick={handleGenerateSeedScript}
                  >
                    Generate Seed Script
                  </Button>
                  <Button
                    variant="outlined"
                    color="error"
                    startIcon={<DeleteIcon />}
                    onClick={() => setClearDataDialogOpen(true)}
                  >
                    Clear All Data
                  </Button>
                </Box>
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12}>
            <Card>
              <CardContent>
                <Typography variant="h6" sx={{ mb: 2 }}>3NF Database Normalization</Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                  The CRM database schema is designed following Third Normal Form (3NF) principles:
                </Typography>
                <Accordion>
                  <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                    <Typography fontWeight={600}>1NF - First Normal Form</Typography>
                  </AccordionSummary>
                  <AccordionDetails>
                    <Typography variant="body2">
                      • Each table has a primary key (Id column)<br />
                      • All columns contain atomic (indivisible) values<br />
                      • No repeating groups or arrays in columns<br />
                      • Example: Contact addresses are stored in separate columns (Street, City, State, Zip)
                    </Typography>
                  </AccordionDetails>
                </Accordion>
                <Accordion>
                  <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                    <Typography fontWeight={600}>2NF - Second Normal Form</Typography>
                  </AccordionSummary>
                  <AccordionDetails>
                    <Typography variant="body2">
                      • All non-key attributes depend on the entire primary key<br />
                      • Junction tables used for many-to-many relationships<br />
                      • Example: OpportunityProducts links Opportunities and Products<br />
                      • Example: UserGroupMembers links Users and UserGroups
                    </Typography>
                  </AccordionDetails>
                </Accordion>
                <Accordion>
                  <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                    <Typography fontWeight={600}>3NF - Third Normal Form</Typography>
                  </AccordionSummary>
                  <AccordionDetails>
                    <Typography variant="body2">
                      • No transitive dependencies between non-key attributes<br />
                      • Departments are separate from Users (not denormalized)<br />
                      • ProductCategories are separate from Products<br />
                      • Customer status/industry normalized to lookup tables<br />
                      • All derived data calculated at query time
                    </Typography>
                  </AccordionDetails>
                </Accordion>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      </TabPanel>

      {/* Migration Tab */}
      <TabPanel value={tabValue} index={3}>
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>Database Migration</Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
              Generate migration scripts to move your CRM database to a different database provider.
              Scripts include schema creation, data migration, and index creation.
            </Typography>
            
            <Button
              variant="contained"
              startIcon={<MigrateIcon />}
              onClick={() => setMigrationDialogOpen(true)}
              sx={{ backgroundColor: '#6750A4' }}
            >
              Generate Migration Script
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>Supported Database Providers</Typography>
            <Grid container spacing={2}>
              {providers.map((provider) => (
                <Grid item xs={12} sm={6} md={4} key={provider.code}>
                  <Paper
                    elevation={provider.isCurrentProvider ? 3 : 1}
                    sx={{
                      p: 2,
                      border: provider.isCurrentProvider ? '2px solid #06A77D' : '1px solid #e0e0e0',
                      borderRadius: 2,
                    }}
                  >
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                      <Typography variant="subtitle1" fontWeight={600}>
                        {provider.name}
                      </Typography>
                      {provider.isCurrentProvider && (
                        <Chip label="Current" size="small" color="success" />
                      )}
                    </Box>
                    <Typography variant="body2" color="text.secondary">
                      {provider.description}
                    </Typography>
                  </Paper>
                </Grid>
              ))}
            </Grid>
          </CardContent>
        </Card>
      </TabPanel>

      {/* Backup Dialog */}
      <Dialog open={backupDialogOpen} onClose={() => setBackupDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Create Database Backup</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
            <TextField
              label="Backup Name"
              value={backupForm.backupName}
              onChange={(e) => setBackupForm({ ...backupForm, backupName: e.target.value })}
              placeholder="Leave empty for auto-generated name"
              fullWidth
            />
            <FormControl fullWidth>
              <InputLabel>Backup Type</InputLabel>
              <Select
                value={backupForm.backupType}
                label="Backup Type"
                onChange={(e) => setBackupForm({ ...backupForm, backupType: e.target.value })}
              >
                <MenuItem value="Full">Full Backup</MenuItem>
                <MenuItem value="Incremental">Incremental</MenuItem>
                <MenuItem value="Differential">Differential</MenuItem>
              </Select>
            </FormControl>
            <TextField
              label="Description"
              value={backupForm.description}
              onChange={(e) => setBackupForm({ ...backupForm, description: e.target.value })}
              multiline
              rows={2}
              fullWidth
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setBackupDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleCreateBackup} disabled={loading}>
            {loading ? <CircularProgress size={24} /> : 'Create Backup'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Migration Dialog */}
      <Dialog open={migrationDialogOpen} onClose={() => setMigrationDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Generate Migration Script</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
            <FormControl fullWidth>
              <InputLabel>Target Database</InputLabel>
              <Select
                value={migrationForm.targetDatabase}
                label="Target Database"
                onChange={(e) => setMigrationForm({ ...migrationForm, targetDatabase: e.target.value })}
              >
                <MenuItem value="mariadb">MariaDB</MenuItem>
                <MenuItem value="mysql">MySQL</MenuItem>
                <MenuItem value="postgresql">PostgreSQL</MenuItem>
                <MenuItem value="sqlserver">SQL Server</MenuItem>
                <MenuItem value="oracle">Oracle</MenuItem>
                <MenuItem value="mongodb">MongoDB (Document Schema)</MenuItem>
              </Select>
            </FormControl>
            
            <Divider />
            
            <FormControlLabel
              control={
                <Checkbox
                  checked={migrationForm.includeData}
                  onChange={(e) => setMigrationForm({ ...migrationForm, includeData: e.target.checked })}
                />
              }
              label="Include data migration (INSERT statements)"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={migrationForm.includeIndexes}
                  onChange={(e) => setMigrationForm({ ...migrationForm, includeIndexes: e.target.checked })}
                />
              }
              label="Include index creation"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={migrationForm.includeSeedData}
                  onChange={(e) => setMigrationForm({ ...migrationForm, includeSeedData: e.target.checked })}
                />
              }
              label="Include initial seed data only (excludes user data)"
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setMigrationDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleGenerateMigrationScript} disabled={loading}>
            {loading ? <CircularProgress size={24} /> : 'Generate Script'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Clear Data Confirmation Dialog */}
      <Dialog open={clearDataDialogOpen} onClose={() => setClearDataDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ color: 'error.main' }}>⚠️ Clear All Data</DialogTitle>
        <DialogContent>
          <Alert severity="error" sx={{ mb: 2 }}>
            This action will permanently delete ALL data from the database. This cannot be undone!
          </Alert>
          <Typography variant="body2" sx={{ mb: 2 }}>
            Type <strong>DELETE_ALL_DATA</strong> to confirm:
          </Typography>
          <TextField
            value={clearDataConfirmation}
            onChange={(e) => setClearDataConfirmation(e.target.value)}
            fullWidth
            placeholder="Type confirmation code"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => { setClearDataDialogOpen(false); setClearDataConfirmation(''); }}>
            Cancel
          </Button>
          <Button
            variant="contained"
            color="error"
            onClick={handleClearData}
            disabled={loading || clearDataConfirmation !== 'DELETE_ALL_DATA'}
          >
            Delete All Data
          </Button>
        </DialogActions>
      </Dialog>

      {/* Script Viewer Dialog */}
      <Dialog open={scriptDialogOpen} onClose={() => setScriptDialogOpen(false)} maxWidth="lg" fullWidth>
        <DialogTitle>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h6">{scriptTitle}</Typography>
            <Button variant="outlined" startIcon={<DownloadIcon />} onClick={downloadScript}>
              Download Script
            </Button>
          </Box>
        </DialogTitle>
        <DialogContent>
          <Paper
            sx={{
              p: 2,
              backgroundColor: '#1e1e1e',
              color: '#d4d4d4',
              fontFamily: 'monospace',
              fontSize: '12px',
              overflow: 'auto',
              maxHeight: '60vh',
              whiteSpace: 'pre-wrap',
            }}
          >
            {generatedScript}
          </Paper>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setScriptDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>

      {/* Migration Wizard Dialog */}
      <Dialog open={wizardOpen} onClose={() => !migrating && setWizardOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <MigrateIcon />
            Database Migration Wizard
          </Box>
        </DialogTitle>
        <DialogContent>
          {/* Stepper */}
          <Box sx={{ mb: 3 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
              {['Select Database', 'Connection Details', 'Test & Migrate', 'Complete'].map((label, index) => (
                <Box
                  key={label}
                  sx={{
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    flex: 1,
                    position: 'relative',
                    '&::before': index > 0 ? {
                      content: '""',
                      position: 'absolute',
                      left: 0,
                      top: '15px',
                      width: '50%',
                      height: '2px',
                      backgroundColor: index <= wizardStep ? 'primary.main' : 'grey.300',
                    } : {},
                    '&::after': index < 3 ? {
                      content: '""',
                      position: 'absolute',
                      right: 0,
                      top: '15px',
                      width: '50%',
                      height: '2px',
                      backgroundColor: index < wizardStep ? 'primary.main' : 'grey.300',
                    } : {},
                  }}
                >
                  <Box
                    sx={{
                      width: 30,
                      height: 30,
                      borderRadius: '50%',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      backgroundColor: index <= wizardStep ? 'primary.main' : 'grey.300',
                      color: 'white',
                      fontWeight: 'bold',
                      zIndex: 1,
                    }}
                  >
                    {index < wizardStep ? <CheckIcon fontSize="small" /> : index + 1}
                  </Box>
                  <Typography variant="caption" sx={{ mt: 1, textAlign: 'center' }}>{label}</Typography>
                </Box>
              ))}
            </Box>
          </Box>

          {/* Step 0: Select Database */}
          {wizardStep === 0 && (
            <Box>
              <Typography variant="subtitle1" sx={{ mb: 2 }}>Select the target database system:</Typography>
              <Grid container spacing={2}>
                {databaseOptions.map((db) => (
                  <Grid item xs={6} key={db.code}>
                    <Card
                      sx={{
                        cursor: 'pointer',
                        border: wizardForm.targetProvider === db.code ? '2px solid' : '1px solid',
                        borderColor: wizardForm.targetProvider === db.code ? 'primary.main' : 'grey.300',
                        '&:hover': { borderColor: 'primary.main' },
                      }}
                      onClick={() => handleProviderChange(db.code)}
                    >
                      <CardContent>
                        <Typography variant="h6">{db.name}</Typography>
                        <Typography variant="body2" color="text.secondary">{db.description}</Typography>
                        <Chip label={`Port ${db.port}`} size="small" sx={{ mt: 1 }} />
                      </CardContent>
                    </Card>
                  </Grid>
                ))}
              </Grid>
            </Box>
          )}

          {/* Step 1: Connection Details */}
          {wizardStep === 1 && (
            <Box>
              <Typography variant="subtitle1" sx={{ mb: 2 }}>
                Enter connection details for {databaseOptions.find(d => d.code === wizardForm.targetProvider)?.name}:
              </Typography>
              <Grid container spacing={2}>
                <Grid item xs={8}>
                  <TextField
                    label="Host"
                    value={wizardForm.host}
                    onChange={(e) => setWizardForm({ ...wizardForm, host: e.target.value })}
                    fullWidth
                    placeholder="localhost or IP address"
                  />
                </Grid>
                <Grid item xs={4}>
                  <TextField
                    label="Port"
                    type="number"
                    value={wizardForm.port}
                    onChange={(e) => setWizardForm({ ...wizardForm, port: parseInt(e.target.value) || 0 })}
                    fullWidth
                  />
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    label="Database Name"
                    value={wizardForm.database}
                    onChange={(e) => setWizardForm({ ...wizardForm, database: e.target.value })}
                    fullWidth
                    placeholder="crm_db"
                  />
                </Grid>
                <Grid item xs={6}>
                  <TextField
                    label="User ID"
                    value={wizardForm.userId}
                    onChange={(e) => setWizardForm({ ...wizardForm, userId: e.target.value })}
                    fullWidth
                  />
                </Grid>
                <Grid item xs={6}>
                  <TextField
                    label="Password"
                    type={showPassword ? 'text' : 'password'}
                    value={wizardForm.password}
                    onChange={(e) => setWizardForm({ ...wizardForm, password: e.target.value })}
                    fullWidth
                    InputProps={{
                      endAdornment: (
                        <IconButton onClick={() => setShowPassword(!showPassword)} edge="end">
                          {showPassword ? <VisibilityOffIcon /> : <VisibilityIcon />}
                        </IconButton>
                      ),
                    }}
                  />
                </Grid>
                <Grid item xs={12}>
                  <Divider sx={{ my: 1 }} />
                </Grid>
                <Grid item xs={12}>
                  <FormControlLabel
                    control={
                      <Checkbox
                        checked={wizardForm.migrateData}
                        onChange={(e) => setWizardForm({ ...wizardForm, migrateData: e.target.checked })}
                      />
                    }
                    label="Migrate existing data to new database"
                  />
                </Grid>
                <Grid item xs={12}>
                  <FormControlLabel
                    control={
                      <Checkbox
                        checked={wizardForm.createBackup}
                        onChange={(e) => setWizardForm({ ...wizardForm, createBackup: e.target.checked })}
                      />
                    }
                    label="Create backup before migration"
                  />
                </Grid>
              </Grid>
            </Box>
          )}

          {/* Step 2: Test & Migrate */}
          {wizardStep === 2 && (
            <Box>
              <Typography variant="subtitle1" sx={{ mb: 2 }}>Test connection and start migration:</Typography>
              
              {/* Connection Test Section */}
              <Card sx={{ mb: 3, p: 2 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                  <Typography variant="h6">1. Test Connection</Typography>
                  <Button
                    variant="outlined"
                    onClick={handleTestConnection}
                    disabled={loading}
                    startIcon={loading ? <CircularProgress size={20} /> : <PlayArrowIcon />}
                  >
                    Test Connection
                  </Button>
                </Box>
                
                {connectionTest && (
                  <Alert severity={connectionTest.success ? 'success' : 'error'} sx={{ mt: 2 }}>
                    <Typography variant="subtitle2">{connectionTest.message}</Typography>
                    {connectionTest.serverVersion && (
                      <Typography variant="body2">Server version: {connectionTest.serverVersion}</Typography>
                    )}
                    {connectionTest.details && (
                      <Typography variant="body2">{connectionTest.details}</Typography>
                    )}
                  </Alert>
                )}
              </Card>

              {/* Migration Section */}
              <Card sx={{ p: 2 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                  <Typography variant="h6">2. Start Migration</Typography>
                  <Button
                    variant="contained"
                    color="primary"
                    onClick={handleStartMigration}
                    disabled={!connectionTest?.success || migrating}
                    startIcon={migrating ? <CircularProgress size={20} /> : <MigrateIcon />}
                  >
                    {migrating ? 'Migrating...' : 'Start Migration'}
                  </Button>
                </Box>
                
                {!connectionTest?.success && (
                  <Alert severity="info">Please test the connection first before starting migration.</Alert>
                )}

                {migrationResult && !migrationResult.success && (
                  <Alert severity="error" sx={{ mt: 2 }}>
                    <Typography variant="subtitle2">Migration Failed</Typography>
                    <Typography variant="body2">{migrationResult.errorMessage}</Typography>
                  </Alert>
                )}

                {migrationResult?.steps && migrationResult.steps.length > 0 && (
                  <Box sx={{ mt: 2 }}>
                    <Typography variant="subtitle2" sx={{ mb: 1 }}>Migration Progress:</Typography>
                    {migrationResult.steps.map((step, index) => (
                      <Box key={index} sx={{ display: 'flex', alignItems: 'center', gap: 1, py: 0.5 }}>
                        {getStepIcon(step.status)}
                        <Typography>{step.name}</Typography>
                        {step.details && <Typography variant="body2" color="text.secondary">- {step.details}</Typography>}
                        {step.error && <Typography variant="body2" color="error">- {step.error}</Typography>}
                      </Box>
                    ))}
                  </Box>
                )}
              </Card>
            </Box>
          )}

          {/* Step 3: Complete */}
          {wizardStep === 3 && (
            <Box>
              <Alert severity="success" sx={{ mb: 3 }}>
                <Typography variant="h6">Migration Completed Successfully!</Typography>
              </Alert>
              
              <Typography variant="subtitle1" sx={{ mb: 2 }}>
                The database structure has been created. To complete the migration, you need to update your configuration:
              </Typography>
              
              {migrationResult?.configurationInstructions && (
                <Paper
                  sx={{
                    p: 2,
                    backgroundColor: '#1e1e1e',
                    color: '#d4d4d4',
                    fontFamily: 'monospace',
                    fontSize: '12px',
                    whiteSpace: 'pre-wrap',
                    mb: 3,
                  }}
                >
                  {migrationResult.configurationInstructions}
                </Paper>
              )}

              <Alert severity="info">
                After updating the configuration and restarting the services, refresh this page to see the new database connection.
              </Alert>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setWizardOpen(false)} disabled={migrating}>
            {wizardStep === 3 ? 'Close' : 'Cancel'}
          </Button>
          {wizardStep > 0 && wizardStep < 3 && (
            <Button onClick={() => setWizardStep(wizardStep - 1)} startIcon={<ArrowBackIcon />} disabled={migrating}>
              Back
            </Button>
          )}
          {wizardStep < 2 && (
            <Button
              variant="contained"
              onClick={() => setWizardStep(wizardStep + 1)}
              endIcon={<ArrowForwardIcon />}
              disabled={wizardStep === 1 && (!wizardForm.host || !wizardForm.database || !wizardForm.userId)}
            >
              Next
            </Button>
          )}
        </DialogActions>
      </Dialog>

      {/* Upload Backup Dialog */}
      <Dialog open={uploadDialogOpen} onClose={() => setUploadDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Upload Backup</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Alert severity="info" sx={{ mb: 2 }}>
              Upload a backup file (.sql, .sql.gz) to add it to the backup history or restore from it directly.
            </Alert>
            <input
              type="file"
              accept=".sql,.sql.gz,.gz"
              id="backup-file-upload"
              style={{ display: 'none' }}
              onChange={(e) => {
                const file = e.target.files?.[0];
                if (file) handleUploadBackup(file);
              }}
            />
            <label htmlFor="backup-file-upload">
              <Button
                variant="contained"
                component="span"
                startIcon={<UploadIcon />}
                fullWidth
                disabled={loading}
              >
                {loading ? <CircularProgress size={24} /> : 'Select & Upload Backup File'}
              </Button>
            </label>
            <Divider sx={{ my: 3 }}>OR</Divider>
            <Typography variant="subtitle2" gutterBottom>Restore Directly from File</Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Upload and restore immediately without saving the backup file.
            </Typography>
            <input
              type="file"
              accept=".sql,.sql.gz,.gz"
              id="restore-file-upload"
              style={{ display: 'none' }}
              onChange={(e) => {
                const file = e.target.files?.[0];
                if (file) handleRestoreFromUpload(file);
              }}
            />
            <label htmlFor="restore-file-upload">
              <Button
                variant="outlined"
                component="span"
                startIcon={<RestoreIcon />}
                fullWidth
                color="warning"
                disabled={loading}
              >
                Select & Restore from File
              </Button>
            </label>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setUploadDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>

      {/* Backup Schedule Dialog */}
      <Dialog open={scheduleDialogOpen} onClose={() => { setScheduleDialogOpen(false); resetScheduleForm(); }} maxWidth="md" fullWidth>
        <DialogTitle>{editingSchedule ? 'Edit Backup Schedule' : 'Create Backup Schedule'}</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ pt: 2 }}>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Schedule Name"
                value={scheduleForm.name}
                onChange={(e) => setScheduleForm({ ...scheduleForm, name: e.target.value })}
                fullWidth
                required
                placeholder="e.g., Daily Full Backup"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Backup Type</InputLabel>
                <Select
                  value={scheduleForm.backupType}
                  label="Backup Type"
                  onChange={(e) => setScheduleForm({ ...scheduleForm, backupType: e.target.value })}
                >
                  <MenuItem value="Full">Full Backup</MenuItem>
                  <MenuItem value="Incremental">Incremental</MenuItem>
                  <MenuItem value="Differential">Differential</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={8}>
              <TextField
                label="Backup Storage Path"
                value={scheduleForm.backupPath}
                onChange={(e) => setScheduleForm({ ...scheduleForm, backupPath: e.target.value })}
                fullWidth
                helperText="Directory where scheduled backups will be stored"
                InputProps={{
                  startAdornment: <FolderIcon sx={{ mr: 1, color: 'text.secondary' }} />
                }}
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <FormControlLabel
                control={
                  <Switch
                    checked={scheduleForm.compressBackups}
                    onChange={(e) => setScheduleForm({ ...scheduleForm, compressBackups: e.target.checked })}
                  />
                }
                label="Compress"
              />
            </Grid>
            <Grid item xs={12}>
              <Typography variant="subtitle2" gutterBottom>Schedule Frequency</Typography>
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 2 }}>
                {cronPresets.map((preset) => (
                  <Chip
                    key={preset.value}
                    label={preset.label}
                    onClick={() => setScheduleForm({ ...scheduleForm, cronExpression: preset.value })}
                    color={scheduleForm.cronExpression === preset.value ? 'primary' : 'default'}
                    variant={scheduleForm.cronExpression === preset.value ? 'filled' : 'outlined'}
                    size="small"
                  />
                ))}
              </Box>
              <TextField
                label="Cron Expression"
                value={scheduleForm.cronExpression}
                onChange={(e) => setScheduleForm({ ...scheduleForm, cronExpression: e.target.value })}
                fullWidth
                size="small"
                helperText="Format: minute hour day-of-month month day-of-week (e.g., '0 2 * * *' = daily at 2 AM)"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Retention Days"
                type="number"
                value={scheduleForm.retentionDays}
                onChange={(e) => setScheduleForm({ ...scheduleForm, retentionDays: parseInt(e.target.value) || 0 })}
                fullWidth
                helperText="Delete backups older than X days (0 = keep forever)"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Max Backups to Keep"
                type="number"
                value={scheduleForm.maxBackupsToKeep}
                onChange={(e) => setScheduleForm({ ...scheduleForm, maxBackupsToKeep: parseInt(e.target.value) || 0 })}
                fullWidth
                helperText="Keep only the last X backups (0 = unlimited)"
              />
            </Grid>
            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Switch
                    checked={scheduleForm.isEnabled}
                    onChange={(e) => setScheduleForm({ ...scheduleForm, isEnabled: e.target.checked })}
                  />
                }
                label="Enable Schedule"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => { setScheduleDialogOpen(false); resetScheduleForm(); }}>Cancel</Button>
          <Button
            variant="contained"
            onClick={editingSchedule ? handleUpdateSchedule : handleCreateSchedule}
            disabled={loading || !scheduleForm.name}
          >
            {loading ? <CircularProgress size={24} /> : editingSchedule ? 'Update Schedule' : 'Create Schedule'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default DatabaseSettingsTab;

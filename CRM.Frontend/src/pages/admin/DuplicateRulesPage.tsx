// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Duplicate Detection Rules Management Page

import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  Button,
  Chip,
  Tooltip,
  TextField,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Slider,
  Switch,
  FormControlLabel,
  Grid,
  Card,
  CardContent,
  Alert,
  Snackbar,
  CircularProgress,
  Tabs,
  Tab,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  ContentCopy as DuplicateIcon,
  PlayArrow as RunIcon,
  Refresh as RefreshIcon,
  Settings as SettingsIcon,
  Rule as RuleIcon,
} from '@mui/icons-material';
import { getActiveRules, scanForDuplicates, DuplicateRule, DuplicateMatchField } from '../../services/duplicateService';
import api from '../../services/apiClient';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`duplicate-rules-tabpanel-${index}`}
      aria-labelledby={`duplicate-rules-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  );
}

type EntityType = 'Lead' | 'Contact' | 'Account';

const entityFields: Record<EntityType, string[]> = {
  Lead: ['FirstName', 'LastName', 'Email', 'Phone', 'Company', 'Title', 'Website', 'Address'],
  Contact: ['FirstName', 'LastName', 'Email', 'Phone', 'Mobile', 'JobTitle', 'Department'],
  Account: ['Name', 'Email', 'Phone', 'Website', 'Industry', 'Address', 'City', 'Country'],
};

const matchTypes = [
  { value: 'Exact', label: 'Exact Match' },
  { value: 'CaseInsensitive', label: 'Case Insensitive' },
  { value: 'Fuzzy', label: 'Fuzzy (Levenshtein)' },
  { value: 'Phonetic', label: 'Phonetic (Soundex)' },
  { value: 'Contains', label: 'Contains' },
  { value: 'StartsWith', label: 'Starts With' },
];

const DuplicateRulesPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState(0);
  const [rules, setRules] = useState<DuplicateRule[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [selectedEntityType, setSelectedEntityType] = useState<EntityType>('Lead');
  
  // Dialog state
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingRule, setEditingRule] = useState<DuplicateRule | null>(null);
  
  // Form state
  const [formData, setFormData] = useState({
    name: '',
    entityType: 'Lead' as EntityType,
    isActive: true,
    matchThreshold: 80,
    description: '',
    matchFields: [] as Omit<DuplicateMatchField, 'id'>[],
  });

  // Scan state
  const [scanning, setScanning] = useState(false);
  const [scanResults, setScanResults] = useState<{
    totalRecordsScanned: number;
    duplicateCandidatesFound: number;
  } | null>(null);

  useEffect(() => {
    loadRules();
  }, [selectedEntityType]);

  const loadRules = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await getActiveRules(selectedEntityType);
      setRules(data);
    } catch (err: any) {
      console.error('Failed to load rules:', err);
      setError(err?.response?.data?.message || err?.message || 'Failed to load duplicate rules. Please try again.');
      setRules([]);
    } finally {
      setLoading(false);
    }
  };

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue);
  };

  const handleEntityTypeChange = (entityType: EntityType) => {
    setSelectedEntityType(entityType);
  };

  const handleAddRule = () => {
    setEditingRule(null);
    setFormData({
      name: '',
      entityType: selectedEntityType,
      isActive: true,
      matchThreshold: 80,
      description: '',
      matchFields: [],
    });
    setDialogOpen(true);
  };

  const handleEditRule = (rule: DuplicateRule) => {
    setEditingRule(rule);
    setFormData({
      name: rule.name,
      entityType: rule.entityType as EntityType,
      isActive: rule.isActive,
      matchThreshold: rule.matchThreshold,
      description: rule.description || '',
      matchFields: rule.matchFields.map(f => ({
        fieldName: f.fieldName,
        matchType: f.matchType,
        weight: f.weight,
        transformations: f.transformations,
      })),
    });
    setDialogOpen(true);
  };

  const handleDeleteRule = async (ruleId: number) => {
    if (!window.confirm('Are you sure you want to delete this rule?')) return;
    
    try {
      await api.delete(`/api/duplicates/rules/${ruleId}`);
      setSuccessMessage('Rule deleted successfully');
      loadRules();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete rule');
    }
  };

  const handleSaveRule = async () => {
    if (!formData.name || formData.matchFields.length === 0) {
      setError('Rule name and at least one match field are required');
      return;
    }

    try {
      if (editingRule) {
        await api.put(`/api/duplicates/rules/${editingRule.id}`, formData);
        setSuccessMessage('Rule updated successfully');
      } else {
        await api.post('/api/duplicates/rules', formData);
        setSuccessMessage('Rule created successfully');
      }
      setDialogOpen(false);
      loadRules();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save rule');
    }
  };

  const handleAddField = () => {
    setFormData({
      ...formData,
      matchFields: [
        ...formData.matchFields,
        { fieldName: '', matchType: 'Exact', weight: 50 },
      ],
    });
  };

  const handleRemoveField = (index: number) => {
    setFormData({
      ...formData,
      matchFields: formData.matchFields.filter((_, i) => i !== index),
    });
  };

  const handleFieldChange = (index: number, field: string, value: any) => {
    const updatedFields = [...formData.matchFields];
    updatedFields[index] = { ...updatedFields[index], [field]: value };
    setFormData({ ...formData, matchFields: updatedFields });
  };

  const handleRunScan = async (ruleId?: number) => {
    setScanning(true);
    setScanResults(null);
    try {
      const result = await scanForDuplicates(selectedEntityType, ruleId);
      setScanResults({
        totalRecordsScanned: result.totalRecordsScanned,
        duplicateCandidatesFound: result.duplicateCandidatesFound,
      });
      setSuccessMessage(`Scan complete: Found ${result.duplicateCandidatesFound} potential duplicates`);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to run scan');
    } finally {
      setScanning(false);
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h4" component="h1" gutterBottom>
            <RuleIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
            Duplicate Detection Rules
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Configure rules for detecting and managing duplicate records
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            variant="outlined"
            startIcon={scanning ? <CircularProgress size={20} /> : <RunIcon />}
            onClick={() => handleRunScan()}
            disabled={scanning}
          >
            Run Full Scan
          </Button>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={handleAddRule}
          >
            Add Rule
          </Button>
        </Box>
      </Box>

      {/* Entity Type Selector */}
      <Paper sx={{ mb: 3 }}>
        <Tabs value={activeTab} onChange={handleTabChange} sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tab label="Lead Rules" onClick={() => handleEntityTypeChange('Lead')} />
          <Tab label="Contact Rules" onClick={() => handleEntityTypeChange('Contact')} />
          <Tab label="Account Rules" onClick={() => handleEntityTypeChange('Account')} />
        </Tabs>

        <TabPanel value={activeTab} index={activeTab}>
          {/* Scan Results */}
          {scanResults && (
            <Alert severity="info" sx={{ mb: 2 }} onClose={() => setScanResults(null)}>
              Scanned {scanResults.totalRecordsScanned} records and found {scanResults.duplicateCandidatesFound} potential duplicate pairs.
            </Alert>
          )}

          {/* Rules Table */}
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
              <CircularProgress />
            </Box>
          ) : rules.length === 0 ? (
            <Box sx={{ textAlign: 'center', p: 4 }}>
              <Typography variant="body1" color="text.secondary">
                No duplicate detection rules configured for {selectedEntityType}s.
              </Typography>
              <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={handleAddRule}
                sx={{ mt: 2 }}
              >
                Create First Rule
              </Button>
            </Box>
          ) : (
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Rule Name</TableCell>
                    <TableCell>Description</TableCell>
                    <TableCell align="center">Match Fields</TableCell>
                    <TableCell align="center">Threshold</TableCell>
                    <TableCell align="center">Status</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {rules.map((rule) => (
                    <TableRow key={rule.id} hover>
                      <TableCell>
                        <Typography variant="body2" fontWeight="medium">
                          {rule.name}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" color="text.secondary">
                          {rule.description || '-'}
                        </Typography>
                      </TableCell>
                      <TableCell align="center">
                        <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap', justifyContent: 'center' }}>
                          {rule.matchFields.map((field, idx) => (
                            <Chip
                              key={idx}
                              size="small"
                              label={`${field.fieldName} (${field.matchType})`}
                              variant="outlined"
                            />
                          ))}
                        </Box>
                      </TableCell>
                      <TableCell align="center">
                        <Chip
                          size="small"
                          label={`${rule.matchThreshold}%`}
                          color={rule.matchThreshold >= 90 ? 'success' : rule.matchThreshold >= 70 ? 'warning' : 'default'}
                        />
                      </TableCell>
                      <TableCell align="center">
                        <Chip
                          size="small"
                          label={rule.isActive ? 'Active' : 'Inactive'}
                          color={rule.isActive ? 'success' : 'default'}
                        />
                      </TableCell>
                      <TableCell align="right">
                        <Tooltip title="Run Scan with this Rule">
                          <IconButton size="small" onClick={() => handleRunScan(rule.id)} disabled={scanning}>
                            <RunIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Edit">
                          <IconButton size="small" onClick={() => handleEditRule(rule)}>
                            <EditIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton size="small" onClick={() => handleDeleteRule(rule.id)} color="error">
                            <DeleteIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </TabPanel>
      </Paper>

      {/* Rule Dialog */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          {editingRule ? 'Edit Duplicate Detection Rule' : 'Create Duplicate Detection Rule'}
        </DialogTitle>
        <DialogContent>
          <Grid container spacing={3} sx={{ mt: 0 }}>
            <Grid item xs={12} md={6}>
              <TextField
                label="Rule Name"
                fullWidth
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                required
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Entity Type</InputLabel>
                <Select
                  value={formData.entityType}
                  label="Entity Type"
                  onChange={(e) => setFormData({ ...formData, entityType: e.target.value as EntityType, matchFields: [] })}
                  disabled={!!editingRule}
                >
                  <MenuItem value="Lead">Lead</MenuItem>
                  <MenuItem value="Contact">Contact</MenuItem>
                  <MenuItem value="Account">Account</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <TextField
                label="Description"
                fullWidth
                multiline
                rows={2}
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography gutterBottom>Match Threshold: {formData.matchThreshold}%</Typography>
              <Slider
                value={formData.matchThreshold}
                onChange={(_e, value) => setFormData({ ...formData, matchThreshold: value as number })}
                min={50}
                max={100}
                marks={[
                  { value: 50, label: '50%' },
                  { value: 75, label: '75%' },
                  { value: 100, label: '100%' },
                ]}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={formData.isActive}
                    onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                  />
                }
                label="Rule is Active"
              />
            </Grid>
            
            {/* Match Fields */}
            <Grid item xs={12}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="h6">Match Fields</Typography>
                <Button startIcon={<AddIcon />} onClick={handleAddField} size="small">
                  Add Field
                </Button>
              </Box>
              
              {formData.matchFields.length === 0 ? (
                <Alert severity="info">
                  Add at least one field to match on. Fields with higher weights contribute more to the match score.
                </Alert>
              ) : (
                formData.matchFields.map((field, index) => (
                  <Card key={index} sx={{ mb: 2 }}>
                    <CardContent>
                      <Grid container spacing={2} alignItems="center">
                        <Grid item xs={12} md={4}>
                          <FormControl fullWidth size="small">
                            <InputLabel>Field</InputLabel>
                            <Select
                              value={field.fieldName}
                              label="Field"
                              onChange={(e) => handleFieldChange(index, 'fieldName', e.target.value)}
                            >
                              {entityFields[formData.entityType].map((f) => (
                                <MenuItem key={f} value={f}>{f}</MenuItem>
                              ))}
                            </Select>
                          </FormControl>
                        </Grid>
                        <Grid item xs={12} md={3}>
                          <FormControl fullWidth size="small">
                            <InputLabel>Match Type</InputLabel>
                            <Select
                              value={field.matchType}
                              label="Match Type"
                              onChange={(e) => handleFieldChange(index, 'matchType', e.target.value)}
                            >
                              {matchTypes.map((mt) => (
                                <MenuItem key={mt.value} value={mt.value}>{mt.label}</MenuItem>
                              ))}
                            </Select>
                          </FormControl>
                        </Grid>
                        <Grid item xs={12} md={4}>
                          <Typography variant="caption" gutterBottom>Weight: {field.weight}%</Typography>
                          <Slider
                            value={field.weight}
                            onChange={(_e, value) => handleFieldChange(index, 'weight', value)}
                            min={10}
                            max={100}
                            size="small"
                          />
                        </Grid>
                        <Grid item xs={12} md={1}>
                          <IconButton onClick={() => handleRemoveField(index)} color="error" size="small">
                            <DeleteIcon />
                          </IconButton>
                        </Grid>
                      </Grid>
                    </CardContent>
                  </Card>
                ))
              )}
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleSaveRule}
            disabled={!formData.name || formData.matchFields.length === 0}
          >
            {editingRule ? 'Update Rule' : 'Create Rule'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Snackbars */}
      <Snackbar
        open={!!successMessage}
        autoHideDuration={4000}
        onClose={() => setSuccessMessage(null)}
      >
        <Alert severity="success" onClose={() => setSuccessMessage(null)}>
          {successMessage}
        </Alert>
      </Snackbar>
      <Snackbar
        open={!!error}
        autoHideDuration={6000}
        onClose={() => setError(null)}
      >
        <Alert severity="error" onClose={() => setError(null)}>
          {error}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default DuplicateRulesPage;

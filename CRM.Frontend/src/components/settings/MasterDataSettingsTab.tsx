/**
 * CRM Solution - Master Data Management Settings Tab
 * Provides UI for managing lookup categories, lookup items, color palettes, ZIP codes, etc.
 */

import React, { useState, useEffect, useCallback } from 'react';
import { TabPanel } from '../common';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Button,
  Alert,
  CircularProgress,
  Grid,
  Paper,
  Chip,
  Tabs,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TablePagination,
  IconButton,
  TextField,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Switch,
  Tooltip,
  InputAdornment,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  Divider,
} from '@mui/material';
import {
  Search as SearchIcon,
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Refresh as RefreshIcon,
  Download as DownloadIcon,
  Upload as UploadIcon,
  Palette as PaletteIcon,
  LocationOn as LocationIcon,
  Category as CategoryIcon,
  List as ListIcon,
  ExpandMore as ExpandMoreIcon,
  Settings as SettingsIcon,
  Storage as StorageIcon,
  CloudDownload as CloudDownloadIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
} from '@mui/icons-material';
import { LinearProgress } from '@mui/material';
import apiClient from '../../services/apiClient';

interface MasterDataOverview {
  lookupCategoriesCount: number;
  lookupItemsCount: number;
  colorPalettesCount: number;
  zipCodesCount: number;
  serviceRequestCategoriesCount: number;
  serviceRequestTypesCount: number;
  dataTypes: MasterDataTypeInfo[];
}

interface MasterDataTypeInfo {
  name: string;
  tableName: string;
  count: number;
  canImportExport: boolean;
}

interface LookupCategory {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
  itemCount: number;
  items: LookupItem[];
}

interface LookupItem {
  id: number;
  key: string;
  value: string;
  meta?: string;
  sortOrder: number;
  isActive: boolean;
  parentItemId?: number;
}

interface ColorPalette {
  id: number;
  name: string;
  category?: string;
  color1?: string;
  color2?: string;
  color3?: string;
  color4?: string;
  color5?: string;
  isUserDefined: boolean;
}

// Helper function to get colors array from palette
const getPaletteColors = (palette: ColorPalette): string[] => {
  return [palette.color1, palette.color2, palette.color3, palette.color4, palette.color5]
    .filter((c): c is string => !!c);
};

interface ZipCode {
  id: number;
  postalCode: string;
  city: string;
  state?: string;
  stateCode?: string;
  county?: string;
  countryCode: string;
  latitude?: number;
  longitude?: number;
}

interface QuickSearchResult {
  id: number;
  postalCode: string;
  city: string;
  state?: string;
  stateCode?: string;
  countryCode: string;
  display: string;
}

interface ZipImportResult {
  success: boolean;
  recordsImported: number;
  recordsSkipped: number;
  recordsUpdated: number;
  recordsFailed: number;
  errorMessage?: string;
  duration: string;
  completedAt: string;
  source?: string;
}

interface ZipImportStatus {
  isRunning: boolean;
  totalRecords: number;
  processedRecords: number;
  progressPercent: number;
  lastImportAt?: string;
  lastResult?: ZipImportResult;
  currentSource?: string;
  currentCountry?: string;
}

function MasterDataSettingsTab() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [selectedTab, setSelectedTab] = useState(0);
  
  // Overview data
  const [overview, setOverview] = useState<MasterDataOverview | null>(null);
  
  // Lookup data
  const [categories, setCategories] = useState<LookupCategory[]>([]);
  const [categoriesLoading, setCategoriesLoading] = useState(false);
  const [categorySearch, setCategorySearch] = useState('');
  const [expandedCategory, setExpandedCategory] = useState<number | null>(null);
  
  // Color palettes
  const [palettes, setPalettes] = useState<ColorPalette[]>([]);
  const [palettesLoading, setPalettesLoading] = useState(false);
  const [paletteSearch, setPaletteSearch] = useState('');
  
  // ZIP codes
  const [zipCodes, setZipCodes] = useState<ZipCode[]>([]);
  const [zipCodesLoading, setZipCodesLoading] = useState(false);
  const [zipSearch, setZipSearch] = useState('');
  const [zipCountryFilter, setZipCountryFilter] = useState('');
  const [zipCountries, setZipCountries] = useState<string[]>([]);
  const [zipPage, setZipPage] = useState(0);
  const [zipRowsPerPage, setZipRowsPerPage] = useState(25);
  const [zipTotalCount, setZipTotalCount] = useState(0);
  
  // ZIP import state
  const [importStatus, setImportStatus] = useState<ZipImportStatus | null>(null);
  const [importLoading, setImportLoading] = useState(false);
  const [importMessage, setImportMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [importCountryCode, setImportCountryCode] = useState('');
  const [csvFile, setCsvFile] = useState<File | null>(null);
  const csvInputRef = React.useRef<HTMLInputElement>(null);
  const importPollRef = React.useRef<NodeJS.Timeout | null>(null);

  // Quick search for global ZIP lookup
  const [quickSearch, setQuickSearch] = useState('');
  const [quickSearchResults, setQuickSearchResults] = useState<QuickSearchResult[]>([]);
  const [quickSearchLoading, setQuickSearchLoading] = useState(false);
  const [showQuickSearch, setShowQuickSearch] = useState(true);
  const quickSearchDebounceRef = React.useRef<NodeJS.Timeout | null>(null);
  
  // Dialogs
  const [categoryDialogOpen, setCategoryDialogOpen] = useState(false);
  const [itemDialogOpen, setItemDialogOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<LookupCategory | null>(null);
  const [editingItem, setEditingItem] = useState<LookupItem | null>(null);
  const [newCategoryName, setNewCategoryName] = useState('');
  const [newCategoryDescription, setNewCategoryDescription] = useState('');
  const [newItemKey, setNewItemKey] = useState('');
  const [newItemValue, setNewItemValue] = useState('');
  const [newItemCategoryId, setNewItemCategoryId] = useState<number>(0);

  // Load overview
  const loadOverview = useCallback(async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/masterdata/overview');
      setOverview(response.data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load master data overview');
    } finally {
      setLoading(false);
    }
  }, []);

  // Load lookup categories
  const loadCategories = useCallback(async () => {
    try {
      setCategoriesLoading(true);
      const params = categorySearch ? { search: categorySearch } : {};
      const response = await apiClient.get('/masterdata/lookup-categories', { params });
      setCategories(response.data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load lookup categories');
    } finally {
      setCategoriesLoading(false);
    }
  }, [categorySearch]);

  // Load color palettes
  const loadPalettes = useCallback(async () => {
    try {
      setPalettesLoading(true);
      const params = paletteSearch ? { search: paletteSearch } : {};
      const response = await apiClient.get('/masterdata/color-palettes', { params });
      setPalettes(response.data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load color palettes');
    } finally {
      setPalettesLoading(false);
    }
  }, [paletteSearch]);

  // Load ZIP codes
  const loadZipCodes = useCallback(async () => {
    try {
      setZipCodesLoading(true);
      setError(null); // Clear previous errors
      const params: any = {
        page: zipPage + 1,
        pageSize: zipRowsPerPage,
      };
      if (zipSearch) params.search = zipSearch;
      if (zipCountryFilter) params.country = zipCountryFilter;
      
      const response = await apiClient.get('/masterdata/zipcodes', { params });
      setZipCodes(response.data.items || []);
      setZipTotalCount(response.data.totalCount || 0);
    } catch (err: any) {
      console.error('Failed to load ZIP codes:', err);
      setError(err.response?.data?.message || 'Failed to load ZIP codes. Please try again.');
      setZipCodes([]);
      setZipTotalCount(0);
    } finally {
      setZipCodesLoading(false);
    }
  }, [zipSearch, zipCountryFilter, zipPage, zipRowsPerPage]);

  // Load ZIP countries
  const loadZipCountries = useCallback(async () => {
    try {
      const response = await apiClient.get('/masterdata/zipcodes/countries');
      setZipCountries(response.data);
    } catch (err) {
      console.error('Failed to load ZIP countries', err);
    }
  }, []);

  // ---- ZIP Import helpers ----
  const loadImportStatus = useCallback(async () => {
    try {
      const res = await apiClient.get('/api/zipcodes/import/status');
      setImportStatus(res.data);
      return res.data as ZipImportStatus;
    } catch {
      return null;
    }
  }, []);

  const startImportPoll = useCallback(() => {
    if (importPollRef.current) clearInterval(importPollRef.current);
    importPollRef.current = setInterval(async () => {
      const status = await loadImportStatus();
      if (status && !status.isRunning) {
        clearInterval(importPollRef.current!);
        importPollRef.current = null;
        setImportLoading(false);
        if (status.lastResult?.success) {
          setImportMessage({ type: 'success', text: `Import complete: ${status.lastResult.recordsImported.toLocaleString()} imported, ${status.lastResult.recordsSkipped.toLocaleString()} skipped.` });
          loadOverview();
        } else if (status.lastResult) {
          setImportMessage({ type: 'error', text: status.lastResult.errorMessage || 'Import failed.' });
        }
      }
    }, 2000);
  }, [loadImportStatus, loadOverview]);

  const triggerGeoNamesAll = useCallback(async () => {
    setImportLoading(true);
    setImportMessage(null);
    try {
      await apiClient.post('/api/zipcodes/import/geonames');
    } catch (e: any) {
      if (e.response?.status !== 409) {
        setImportMessage({ type: 'error', text: e.response?.data?.message || 'Failed to start import.' });
        setImportLoading(false);
        return;
      }
    }
    await loadImportStatus();
    startImportPoll();
  }, [loadImportStatus, startImportPoll]);

  const triggerGeoNamesCountry = useCallback(async () => {
    const cc = importCountryCode.trim().toUpperCase();
    if (!cc || cc.length !== 2) {
      setImportMessage({ type: 'error', text: 'Enter a valid 2-letter ISO country code (e.g. US, GB, DE).' });
      return;
    }
    setImportLoading(true);
    setImportMessage(null);
    try {
      await apiClient.post(`/api/zipcodes/import/geonames/${cc}`);
    } catch (e: any) {
      if (e.response?.status !== 409) {
        setImportMessage({ type: 'error', text: e.response?.data?.message || 'Failed to start import.' });
        setImportLoading(false);
        return;
      }
    }
    await loadImportStatus();
    startImportPoll();
  }, [importCountryCode, loadImportStatus, startImportPoll]);

  const triggerCsvUpload = useCallback(async () => {
    if (!csvFile) {
      setImportMessage({ type: 'error', text: 'Please select a CSV file first.' });
      return;
    }
    setImportLoading(true);
    setImportMessage(null);
    const formData = new FormData();
    formData.append('file', csvFile);
    try {
      const res = await apiClient.post('/api/zipcodes/import/csv-upload', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      setImportLoading(false);
      const result: ZipImportResult = res.data;
      if (result.success) {
        setImportMessage({ type: 'success', text: `Import complete: ${result.recordsImported.toLocaleString()} imported, ${result.recordsSkipped.toLocaleString()} skipped.` });
        loadOverview();
      } else {
        setImportMessage({ type: 'error', text: result.errorMessage || 'Import failed.' });
      }
      setCsvFile(null);
      if (csvInputRef.current) csvInputRef.current.value = '';
      await loadImportStatus();
    } catch (e: any) {
      setImportLoading(false);
      setImportMessage({ type: 'error', text: e.response?.data?.message || 'Failed to upload file.' });
    }
  }, [csvFile, loadImportStatus, loadOverview]);

  // Load import status when ZIP tab is active
  useEffect(() => {
    if (selectedTab === 3) loadImportStatus();
  }, [selectedTab, loadImportStatus]);

  // Cleanup polling on unmount
  useEffect(() => () => {
    if (importPollRef.current) clearInterval(importPollRef.current);
  }, []);

  // Quick search for global ZIP lookup with debounce
  const performQuickSearch = useCallback(async (query: string) => {
    if (!query || query.length < 2) {
      setQuickSearchResults([]);
      return;
    }
    
    try {
      setQuickSearchLoading(true);
      const response = await apiClient.get('/masterdata/zipcodes/search', { 
        params: { q: query, limit: 20 } 
      });
      setQuickSearchResults(response.data.items || []);
    } catch (err) {
      console.error('Quick search failed:', err);
      setQuickSearchResults([]);
    } finally {
      setQuickSearchLoading(false);
    }
  }, []);

  // Debounced quick search handler
  const handleQuickSearchChange = useCallback((value: string) => {
    setQuickSearch(value);
    
    // Clear previous debounce timer
    if (quickSearchDebounceRef.current) {
      clearTimeout(quickSearchDebounceRef.current);
    }
    
    // Set new debounce timer (300ms)
    quickSearchDebounceRef.current = setTimeout(() => {
      performQuickSearch(value);
    }, 300);
  }, [performQuickSearch]);

  useEffect(() => {
    loadOverview();
    loadZipCountries();
  }, [loadOverview, loadZipCountries]);

  useEffect(() => {
    if (selectedTab === 1) loadCategories();
    if (selectedTab === 2) loadPalettes();
    if (selectedTab === 3) loadZipCodes();
  }, [selectedTab, loadCategories, loadPalettes, loadZipCodes]);

  // Reload ZIP codes when pagination changes
  useEffect(() => {
    if (selectedTab === 3) {
      loadZipCodes();
    }
  }, [zipPage, zipRowsPerPage]);

  // CRUD handlers
  const handleCreateCategory = async () => {
    try {
      await apiClient.post('/masterdata/lookup-categories', {
        name: newCategoryName,
        description: newCategoryDescription
      });
      setSuccess('Category created successfully');
      setCategoryDialogOpen(false);
      setNewCategoryName('');
      setNewCategoryDescription('');
      loadCategories();
      loadOverview();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create category');
    }
  };

  const handleDeleteCategory = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this category? All items will be deleted.')) return;
    try {
      await apiClient.delete(`/masterdata/lookup-categories/${id}`);
      setSuccess('Category deleted successfully');
      loadCategories();
      loadOverview();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete category');
    }
  };

  const handleCreateItem = async () => {
    try {
      await apiClient.post('/masterdata/lookup-items', {
        categoryId: newItemCategoryId,
        key: newItemKey,
        value: newItemValue,
        sortOrder: 0
      });
      setSuccess('Item created successfully');
      setItemDialogOpen(false);
      setNewItemKey('');
      setNewItemValue('');
      setNewItemCategoryId(0);
      loadCategories();
      loadOverview();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create item');
    }
  };

  const handleDeleteItem = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this item?')) return;
    try {
      await apiClient.delete(`/masterdata/lookup-items/${id}`);
      setSuccess('Item deleted successfully');
      loadCategories();
      loadOverview();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete item');
    }
  };

  const handleExport = async (dataType: string) => {
    try {
      const response = await apiClient.get(`/masterdata/export/${dataType}`, {
        responseType: 'blob'
      });
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `${dataType}-export.json`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      setSuccess('Export completed successfully');
    } catch (err: any) {
      setError('Failed to export data');
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h6" sx={{ fontWeight: 600, mb: 1 }}>
        Master Data Management
      </Typography>
      <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
        Manage lookup values, color palettes, ZIP codes, and other system reference data.
      </Typography>

      {success && (
        <Alert severity="success" sx={{ mb: 2, borderRadius: 2 }} onClose={() => setSuccess(null)}>
          {success}
        </Alert>
      )}

      {error && (
        <Alert severity="error" sx={{ mb: 2, borderRadius: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Tabs */}
      <Paper sx={{ mb: 3, borderRadius: 2 }}>
        <Tabs 
          value={selectedTab} 
          onChange={(_, v) => setSelectedTab(v)}
          sx={{ borderBottom: 1, borderColor: 'divider', px: 2 }}
        >
          <Tab icon={<StorageIcon />} label="Overview" iconPosition="start" />
          <Tab icon={<CategoryIcon />} label="Lookups" iconPosition="start" />
          <Tab icon={<PaletteIcon />} label="Color Palettes" iconPosition="start" />
          <Tab icon={<LocationIcon />} label="ZIP Codes" iconPosition="start" />
        </Tabs>
      </Paper>

      {/* Overview Tab */}
      <TabPanel value={selectedTab} index={0}>
        <Grid container spacing={3}>
          {overview?.dataTypes.map((dt) => (
            <Grid item xs={12} sm={6} md={4} key={dt.tableName}>
              <Card sx={{ borderRadius: 2 }}>
                <CardContent>
                  <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                    <Box>
                      <Typography variant="h4" color="primary">{dt.count.toLocaleString()}</Typography>
                      <Typography variant="subtitle2" color="textSecondary">{dt.name}</Typography>
                    </Box>
                    {dt.canImportExport && (
                      <Tooltip title="Export">
                        <IconButton onClick={() => handleExport(dt.tableName)} size="small">
                          <DownloadIcon />
                        </IconButton>
                      </Tooltip>
                    )}
                  </Box>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      </TabPanel>

      {/* Lookups Tab */}
      <TabPanel value={selectedTab} index={1}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <TextField
            placeholder="Search categories..."
            size="small"
            value={categorySearch}
            onChange={(e) => setCategorySearch(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && loadCategories()}
            InputProps={{
              startAdornment: <InputAdornment position="start"><SearchIcon /></InputAdornment>
            }}
            sx={{ width: 300 }}
          />
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button startIcon={<RefreshIcon />} onClick={loadCategories} disabled={categoriesLoading}>
              Refresh
            </Button>
            <Button 
              variant="contained" 
              startIcon={<AddIcon />}
              onClick={() => setCategoryDialogOpen(true)}
            >
              Add Category
            </Button>
          </Box>
        </Box>

        {categoriesLoading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Box>
            {categories.map((category) => (
              <Accordion 
                key={category.id}
                expanded={expandedCategory === category.id}
                onChange={() => setExpandedCategory(expandedCategory === category.id ? null : category.id)}
                sx={{ mb: 1, borderRadius: 2, '&:before': { display: 'none' } }}
              >
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, width: '100%' }}>
                    <CategoryIcon color="primary" />
                    <Typography sx={{ fontWeight: 600 }}>{category.name}</Typography>
                    <Chip label={`${category.itemCount} items`} size="small" />
                    {!category.isActive && <Chip label="Inactive" size="small" color="warning" />}
                    <Box sx={{ ml: 'auto', mr: 2 }}>
                      <IconButton 
                        size="small" 
                        onClick={(e) => { e.stopPropagation(); handleDeleteCategory(category.id); }}
                        color="error"
                      >
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </Box>
                  </Box>
                </AccordionSummary>
                <AccordionDetails>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                    <Typography variant="body2" color="textSecondary">
                      {category.description || 'No description'}
                    </Typography>
                    <Button 
                      size="small" 
                      startIcon={<AddIcon />}
                      onClick={() => {
                        setNewItemCategoryId(category.id);
                        setItemDialogOpen(true);
                      }}
                    >
                      Add Item
                    </Button>
                  </Box>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Key</TableCell>
                        <TableCell>Value</TableCell>
                        <TableCell>Order</TableCell>
                        <TableCell>Status</TableCell>
                        <TableCell>Actions</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {category.items.map((item) => (
                        <TableRow key={item.id}>
                          <TableCell><strong>{item.key}</strong></TableCell>
                          <TableCell>{item.value}</TableCell>
                          <TableCell>{item.sortOrder}</TableCell>
                          <TableCell>
                            <Chip 
                              label={item.isActive ? 'Active' : 'Inactive'} 
                              size="small" 
                              color={item.isActive ? 'success' : 'default'}
                            />
                          </TableCell>
                          <TableCell>
                            <IconButton 
                              size="small" 
                              onClick={() => handleDeleteItem(item.id)}
                              color="error"
                            >
                              <DeleteIcon fontSize="small" />
                            </IconButton>
                          </TableCell>
                        </TableRow>
                      ))}
                      {category.items.length === 0 && (
                        <TableRow>
                          <TableCell colSpan={5} sx={{ textAlign: 'center', py: 2 }}>
                            No items in this category
                          </TableCell>
                        </TableRow>
                      )}
                    </TableBody>
                  </Table>
                </AccordionDetails>
              </Accordion>
            ))}
            {categories.length === 0 && (
              <Paper sx={{ p: 4, textAlign: 'center', borderRadius: 2 }}>
                <CategoryIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
                <Typography color="textSecondary">No lookup categories found</Typography>
              </Paper>
            )}
          </Box>
        )}
      </TabPanel>

      {/* Color Palettes Tab */}
      <TabPanel value={selectedTab} index={2}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <TextField
            placeholder="Search palettes..."
            size="small"
            value={paletteSearch}
            onChange={(e) => setPaletteSearch(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && loadPalettes()}
            InputProps={{
              startAdornment: <InputAdornment position="start"><SearchIcon /></InputAdornment>
            }}
            sx={{ width: 300 }}
          />
          <Button startIcon={<RefreshIcon />} onClick={loadPalettes} disabled={palettesLoading}>
            Refresh
          </Button>
        </Box>

        {palettesLoading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Grid container spacing={2}>
            {palettes.map((palette) => (
              <Grid item xs={12} sm={6} md={4} key={palette.id}>
                <Card sx={{ borderRadius: 2 }}>
                  <CardContent>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                      <Box>
                        <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>{palette.name}</Typography>
                        {palette.category && (
                          <Chip label={palette.category} size="small" sx={{ mt: 0.5 }} />
                        )}
                      </Box>
                      {palette.isUserDefined && (
                        <Chip label="Custom" size="small" color="primary" variant="outlined" />
                      )}
                    </Box>
                    <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                      {getPaletteColors(palette).map((color, idx) => (
                        <Tooltip key={idx} title={color}>
                          <Box
                            sx={{
                              width: 32,
                              height: 32,
                              bgcolor: color,
                              borderRadius: 1,
                              border: '1px solid rgba(0,0,0,0.1)'
                            }}
                          />
                        </Tooltip>
                      ))}
                    </Box>
                  </CardContent>
                </Card>
              </Grid>
            ))}
            {palettes.length === 0 && (
              <Grid item xs={12}>
                <Paper sx={{ p: 4, textAlign: 'center', borderRadius: 2 }}>
                  <PaletteIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
                  <Typography color="textSecondary">No color palettes found</Typography>
                </Paper>
              </Grid>
            )}
          </Grid>
        )}
      </TabPanel>

      {/* ZIP Codes Tab */}
      <TabPanel value={selectedTab} index={3}>
        {/* ---- Import / Update ZIP Data ---- */}
        <Paper sx={{ p: 2, mb: 3, borderRadius: 2, border: '1px solid', borderColor: 'divider' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <CloudDownloadIcon color="primary" />
              <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>Import / Update ZIP Data</Typography>
            </Box>
            <Chip
              size="small"
              label={importStatus?.isRunning ? 'Importing…' : (importStatus?.lastResult ? 'Last import done' : 'No imports yet')}
              color={importStatus?.isRunning ? 'warning' : (importStatus?.lastResult?.success ? 'success' : 'default')}
            />
          </Box>

          {importStatus?.isRunning && (
            <Box sx={{ mb: 2 }}>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 0.5 }}>
                {importStatus.currentSource} — {importStatus.processedRecords.toLocaleString()} / {importStatus.totalRecords.toLocaleString() || '?'} records
                {importStatus.currentCountry && ` (${importStatus.currentCountry})`}
              </Typography>
              <LinearProgress variant={importStatus.totalRecords > 0 ? 'determinate' : 'indeterminate'} value={importStatus.progressPercent} />
            </Box>
          )}

          {importMessage && (
            <Alert
              severity={importMessage.type}
              sx={{ mb: 2, borderRadius: 1 }}
              icon={importMessage.type === 'success' ? <CheckCircleIcon /> : <ErrorIcon />}
              onClose={() => setImportMessage(null)}
            >
              {importMessage.text}
            </Alert>
          )}

          {importStatus?.lastResult && !importStatus.isRunning && (
            <Box sx={{ mb: 2, p: 1.5, bgcolor: 'grey.50', borderRadius: 1 }}>
              <Typography variant="caption" color="text.secondary">
                Last import: <strong>{importStatus.lastResult.source || 'Unknown'}</strong>
                {importStatus.lastImportAt && ` · ${new Date(importStatus.lastImportAt).toLocaleString()}`}
                {' · '}{importStatus.lastResult.recordsImported.toLocaleString()} imported
                {' · '}{importStatus.lastResult.recordsSkipped.toLocaleString()} skipped
              </Typography>
            </Box>
          )}

          <Grid container spacing={2}>
            {/* GeoNames — All Countries */}
            <Grid item xs={12} md={4}>
              <Paper variant="outlined" sx={{ p: 2, borderRadius: 2, height: '100%' }}>
                <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 0.5 }}>🌍 GeoNames — All Countries</Typography>
                <Typography variant="caption" color="text.secondary" display="block" sx={{ mb: 1.5 }}>
                  Downloads the complete world postal code dataset (~11M records) from
                  {' '}<a href="https://www.geonames.org/" target="_blank" rel="noreferrer">geonames.org</a>.
                  License: CC Attribution 4.0. This may take 15–30 minutes.
                </Typography>
                <Button
                  fullWidth
                  variant="contained"
                  size="small"
                  startIcon={<CloudDownloadIcon />}
                  onClick={triggerGeoNamesAll}
                  disabled={importLoading || importStatus?.isRunning}
                >
                  Start Full Import
                </Button>
              </Paper>
            </Grid>

            {/* GeoNames — Single Country */}
            <Grid item xs={12} md={4}>
              <Paper variant="outlined" sx={{ p: 2, borderRadius: 2, height: '100%' }}>
                <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 0.5 }}>🏳 GeoNames — Single Country</Typography>
                <Typography variant="caption" color="text.secondary" display="block" sx={{ mb: 1 }}>
                  Import postal codes for one country only. Enter the ISO 2-letter code (e.g. US, GB, DE, AU).
                </Typography>
                <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
                  <TextField
                    size="small"
                    placeholder="US"
                    value={importCountryCode}
                    onChange={(e) => setImportCountryCode(e.target.value.slice(0, 2).toUpperCase())}
                    inputProps={{ maxLength: 2, style: { textTransform: 'uppercase', width: 32, textAlign: 'center' } }}
                    sx={{ width: 72 }}
                  />
                  <Button
                    variant="contained"
                    size="small"
                    startIcon={<CloudDownloadIcon />}
                    onClick={triggerGeoNamesCountry}
                    disabled={importLoading || importStatus?.isRunning || importCountryCode.length !== 2}
                    sx={{ flex: 1 }}
                  >
                    Import Country
                  </Button>
                </Box>
              </Paper>
            </Grid>

            {/* Zeeshanahmad4 CSV Upload */}
            <Grid item xs={12} md={4}>
              <Paper variant="outlined" sx={{ p: 2, borderRadius: 2, height: '100%' }}>
                <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 0.5 }}>📁 CSV Upload (Zeeshanahmad4 format)</Typography>
                <Typography variant="caption" color="text.secondary" display="block" sx={{ mb: 1 }}>
                  Download the dataset from{' '}
                  <a href="https://github.com/Zeeshanahmad4/Zip-code-of-all-countries-cities-in-the-world-CSV-TXT-SQL-DATABASE" target="_blank" rel="noreferrer">this GitHub repo</a>
                  {' '}(Google Drive link in README), then upload the CSV here.
                  Columns: COUNTRY, POSTAL_CODE, CITY, STATE, SHORT_STATE, COUNTY, SHORT_COUNTY, COMMUNITY, SHORT_COMMUNITY, LATITUDE, LONGITUDE, ACCURACY.
                </Typography>
                <input
                  type="file"
                  accept=".csv,.txt,.tsv"
                  ref={csvInputRef}
                  style={{ display: 'none' }}
                  onChange={(e) => setCsvFile(e.target.files?.[0] || null)}
                />
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                  <Button
                    variant="outlined"
                    size="small"
                    startIcon={<UploadIcon />}
                    onClick={() => csvInputRef.current?.click()}
                    disabled={importLoading || importStatus?.isRunning}
                  >
                    {csvFile ? csvFile.name : 'Choose CSV File'}
                  </Button>
                  <Button
                    variant="contained"
                    size="small"
                    startIcon={importLoading ? <CircularProgress size={14} color="inherit" /> : <CloudDownloadIcon />}
                    onClick={triggerCsvUpload}
                    disabled={!csvFile || importLoading || importStatus?.isRunning}
                  >
                    Upload & Import
                  </Button>
                </Box>
              </Paper>
            </Grid>
          </Grid>
        </Paper>

        {/* Quick Global Search */}
        <Paper sx={{ p: 2, mb: 3, bgcolor: 'primary.50', borderRadius: 2 }}>
          <Typography variant="subtitle2" sx={{ mb: 1, fontWeight: 600, color: 'primary.main' }}>
            🔍 Quick Global Search ({overview?.zipCodesCount?.toLocaleString() || 0}+ locations)
          </Typography>
          <TextField
            fullWidth
            placeholder="Type ZIP code or city name (e.g., 90210, London, Paris)..."
            size="small"
            value={quickSearch}
            onChange={(e) => handleQuickSearchChange(e.target.value)}
            InputProps={{
              startAdornment: <InputAdornment position="start"><SearchIcon /></InputAdornment>,
              endAdornment: quickSearchLoading ? <CircularProgress size={20} /> : null
            }}
            sx={{ 
              bgcolor: 'white', 
              borderRadius: 1,
              '& .MuiOutlinedInput-root': { borderRadius: 2 }
            }}
          />
          {quickSearchResults.length > 0 && (
            <Box sx={{ mt: 1, maxHeight: 300, overflowY: 'auto' }}>
              <Table size="small" sx={{ bgcolor: 'white', borderRadius: 1 }}>
                <TableHead>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 600 }}>ZIP/Postal</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>City</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>State</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Country</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {quickSearchResults.map((result) => (
                    <TableRow key={result.id} hover>
                      <TableCell><strong>{result.postalCode}</strong></TableCell>
                      <TableCell>{result.city}</TableCell>
                      <TableCell>{result.state} {result.stateCode && `(${result.stateCode})`}</TableCell>
                      <TableCell><Chip label={result.countryCode} size="small" variant="outlined" /></TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </Box>
          )}
          {quickSearch.length >= 2 && quickSearchResults.length === 0 && !quickSearchLoading && (
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              No results found for "{quickSearch}"
            </Typography>
          )}
          {quickSearch.length > 0 && quickSearch.length < 2 && (
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              Type at least 2 characters to search
            </Typography>
          )}
        </Paper>

        {/* Detailed Browse Section */}
        <Typography variant="subtitle2" sx={{ mb: 1, fontWeight: 600, color: 'text.secondary' }}>
          Browse & Filter
        </Typography>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2, flexWrap: 'wrap', gap: 1 }}>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <TextField
              placeholder="Filter by ZIP/City/State..."
              size="small"
              value={zipSearch}
              onChange={(e) => setZipSearch(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && loadZipCodes()}
              InputProps={{
                startAdornment: <InputAdornment position="start"><SearchIcon /></InputAdornment>
              }}
              sx={{ width: 250 }}
            />
            <FormControl size="small" sx={{ minWidth: 120 }}>
              <InputLabel>Country</InputLabel>
              <Select
                value={zipCountryFilter}
                label="Country"
                onChange={(e) => {
                  setZipCountryFilter(e.target.value);
                  setZipPage(0);
                }}
              >
                <MenuItem value="">All Countries</MenuItem>
                {zipCountries.map((c) => (
                  <MenuItem key={c} value={c}>{c}</MenuItem>
                ))}
              </Select>
            </FormControl>
          </Box>
          <Button startIcon={<RefreshIcon />} onClick={loadZipCodes} disabled={zipCodesLoading}>
            Search
          </Button>
        </Box>

        {zipCodesLoading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Paper sx={{ borderRadius: 2 }}>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Postal Code</TableCell>
                  <TableCell>City</TableCell>
                  <TableCell>State</TableCell>
                  <TableCell>County</TableCell>
                  <TableCell>Country</TableCell>
                  <TableCell>Coordinates</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {zipCodes.map((zip) => (
                  <TableRow key={zip.id}>
                    <TableCell><strong>{zip.postalCode}</strong></TableCell>
                    <TableCell>{zip.city}</TableCell>
                    <TableCell>{zip.state} {zip.stateCode && `(${zip.stateCode})`}</TableCell>
                    <TableCell>{zip.county || '-'}</TableCell>
                    <TableCell><Chip label={zip.countryCode} size="small" /></TableCell>
                    <TableCell>
                      {zip.latitude && zip.longitude 
                        ? `${zip.latitude.toFixed(4)}, ${zip.longitude.toFixed(4)}`
                        : '-'
                      }
                    </TableCell>
                  </TableRow>
                ))}
                {zipCodes.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={6} sx={{ textAlign: 'center', py: 4 }}>
                      <LocationIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 1 }} />
                      <Typography color="textSecondary">No ZIP codes found</Typography>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
            <TablePagination
              component="div"
              count={zipTotalCount}
              page={zipPage}
              onPageChange={(_, newPage) => setZipPage(newPage)}
              rowsPerPage={zipRowsPerPage}
              onRowsPerPageChange={(e) => {
                setZipRowsPerPage(parseInt(e.target.value, 10));
                setZipPage(0);
              }}
              rowsPerPageOptions={[10, 25, 50, 100]}
            />
          </Paper>
        )}
      </TabPanel>

      {/* Add Category Dialog */}
      <Dialog open={categoryDialogOpen} onClose={() => setCategoryDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Add Lookup Category</DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            label="Category Name"
            fullWidth
            value={newCategoryName}
            onChange={(e) => setNewCategoryName(e.target.value)}
            sx={{ mb: 2 }}
          />
          <TextField
            margin="dense"
            label="Description"
            fullWidth
            multiline
            rows={2}
            value={newCategoryDescription}
            onChange={(e) => setNewCategoryDescription(e.target.value)}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCategoryDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleCreateCategory} variant="contained" disabled={!newCategoryName}>
            Create
          </Button>
        </DialogActions>
      </Dialog>

      {/* Add Item Dialog */}
      <Dialog open={itemDialogOpen} onClose={() => setItemDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Add Lookup Item</DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            label="Key"
            fullWidth
            value={newItemKey}
            onChange={(e) => setNewItemKey(e.target.value)}
            sx={{ mb: 2 }}
          />
          <TextField
            margin="dense"
            label="Value"
            fullWidth
            value={newItemValue}
            onChange={(e) => setNewItemValue(e.target.value)}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setItemDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleCreateItem} variant="contained" disabled={!newItemKey || !newItemValue}>
            Create
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default MasterDataSettingsTab;

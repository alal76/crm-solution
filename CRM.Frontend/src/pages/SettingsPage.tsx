import React, { useState, useEffect, useCallback } from 'react';
import { TabPanel } from '../components/common';
import {
  Container,
  Box,
  Tabs,
  Tab,
  Alert,
  CircularProgress,
  Typography,
  Paper,
  Card,
  CardContent,
  TextField,
  Button,
  Grid,
  Avatar,
  Autocomplete,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  IconButton,
  Tooltip,
  Chip,
  Divider,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  DialogContentText,
  Slider,
  Snackbar,
  Switch,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
} from '@mui/material';
import {
  PersonAdd as PersonAddIcon,
  Groups as GroupsIcon,
  Storage as StorageIcon,
  Business as BusinessIcon,
  ToggleOn as ModuleIcon,
  People as PeopleIcon,
  Login as LoginIcon,
  Security as SecurityIcon,
  Refresh as RefreshIcon,
  Delete as DeleteIcon,
  RestartAlt as ResetIcon,
  Palette as PaletteIcon,
  Add as AddIcon,
  Save as SaveIcon,
  Preview as PreviewIcon,
  Settings as SettingsIcon,
  Menu as MenuIcon,
  SupportAgent as SupportAgentIcon,
  ExpandMore as ExpandMoreIcon,
  AdminPanelSettings as AdminPanelSettingsIcon,
  ManageAccounts as ManageAccountsIcon,
  Hub as HubIcon,
} from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';
import UserApprovalTab from '../components/settings/UserApprovalTab';
import GroupManagementTab from '../components/settings/GroupManagementTab';
import DatabaseSettingsTab from '../components/settings/DatabaseSettingsTab';
import ModuleFieldSettingsTab from '../components/settings/ModuleFieldSettingsTabNew';
import UserManagementTab from '../components/settings/UserManagementTab';
import SocialLoginSettingsTab from '../components/settings/SocialLoginSettingsTab';
import SecuritySettingsTab from '../components/settings/SecuritySettingsTab';
import NavigationSettingsTab from '../components/settings/NavigationSettingsTab';
import ServiceRequestSettingsTab from '../components/settings/ServiceRequestSettingsTab';
import DeploymentSettingsTab from '../components/settings/DeploymentSettingsTab';
import MonitoringSettingsTab from '../components/settings/MonitoringSettingsTab';
import MasterDataSettingsTab from '../components/settings/MasterDataSettingsTab';
import FeatureManagementTab from '../components/settings/FeatureManagementTab';
import logo from '../assets/logo.png';
import {
  Cloud as CloudIcon,
  Monitor as MonitorIcon,
  Storage as MasterDataIcon,
  ToggleOn as FeatureToggleIcon,
} from '@mui/icons-material';

interface ColorPalette {
  id: number;
  name: string;
  category: string;
  colors: string[];
  isUserDefined?: boolean;
}

// Color descriptions for UI elements
const COLOR_DESCRIPTIONS = [
  { key: 'primaryColor', label: 'Primary', description: 'Main buttons, headers, navigation bar, links' },
  { key: 'secondaryColor', label: 'Secondary', description: 'Secondary buttons, accents, icons, hover states' },
  { key: 'tertiaryColor', label: 'Tertiary', description: 'Highlights, badges, notifications, tags' },
  { key: 'surfaceColor', label: 'Surface', description: 'Card backgrounds, panels, modals, dropdowns' },
  { key: 'backgroundColor', label: 'Background', description: 'Page background, main content area' },
];

// Helper function to convert hex to RGB
const hexToRgb = (hex: string) => {
  const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
  return result ? {
    r: parseInt(result[1], 16),
    g: parseInt(result[2], 16),
    b: parseInt(result[3], 16)
  } : { r: 0, g: 0, b: 0 };
};

// Helper function to convert RGB to hex
const rgbToHex = (r: number, g: number, b: number) => {
  return '#' + [r, g, b].map(x => {
    const hex = Math.max(0, Math.min(255, x)).toString(16);
    return hex.length === 1 ? '0' + hex : hex;
  }).join('');
};

interface SettingsTab {
  id: string;
  label: string;
  icon: React.ReactNode;
  component: React.ReactNode;
}

function CompanyBrandingTab() {
  const [formData, setFormData] = useState({
    companyName: '',
    companyLogoUrl: '',
    companyLoginLogoUrl: '',
    primaryColor: '#6750A4',
    secondaryColor: '#625B71',
    tertiaryColor: '#7D5260',
    surfaceColor: '#FFFBFE',
    backgroundColor: '#FFFBFE',
    companyWebsite: '',
    companyEmail: '',
    companyPhone: '',
    selectedPaletteId: null as number | null,
    selectedPaletteName: '' as string,
  });
  const [logoPreview, setLogoPreview] = useState<string | null>(null);
  const [loginLogoPreview, setLoginLogoPreview] = useState<string | null>(null);
  const [saved, setSaved] = useState(false);
  const [applied, setApplied] = useState(false);
  const [loading, setLoading] = useState(true);
  const [uploading, setUploading] = useState(false);
  const [uploadingLoginLogo, setUploadingLoginLogo] = useState(false);
  
  // Palette state
  const [palettes, setPalettes] = useState<ColorPalette[]>([]);
  const [userDefinedPalettes, setUserDefinedPalettes] = useState<ColorPalette[]>([]);
  const [categories, setCategories] = useState<string[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<string>('');
  const [paletteSearch, setPaletteSearch] = useState('');
  const [refreshingPalettes, setRefreshingPalettes] = useState(false);
  const [paletteCount, setPaletteCount] = useState(0);
  const [palettesLastRefreshed, setPalettesLastRefreshed] = useState<string | null>(null);
  const [selectedPalette, setSelectedPalette] = useState<ColorPalette | null>(null);
  const [resetDialogOpen, setResetDialogOpen] = useState(false);
  
  // Custom palette creation
  const [customPaletteDialogOpen, setCustomPaletteDialogOpen] = useState(false);
  const [customPaletteName, setCustomPaletteName] = useState('');
  const [customColors, setCustomColors] = useState(['#6750A4', '#625B71', '#7D5260', '#FFFBFE', '#FFFBFE']);
  
  // RGB editing state
  const [editingColorIndex, setEditingColorIndex] = useState<number | null>(null);
  const [editingColorKey, setEditingColorKey] = useState<string | null>(null);
  const [originalColorBeforeEdit, setOriginalColorBeforeEdit] = useState<string>('');
  const [rgbValues, setRgbValues] = useState({ r: 0, g: 0, b: 0 });
  
  // Header color source toggle
  const [useGroupHeaderColor, setUseGroupHeaderColor] = useState(false);
  
  // Snackbar for notifications
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  const getApiUrl = () => {
    return window.location.hostname === 'localhost' 
      ? 'http://localhost:5000/api'
      : `http://${window.location.hostname}:5000/api`;
  };

  // Load user-defined palettes
  const loadUserDefinedPalettes = async () => {
    try {
      const token = localStorage.getItem('accessToken');
      const apiUrl = getApiUrl();
      
      const response = await fetch(`${apiUrl}/colorpalettes/user-defined`, {
        headers: { 'Authorization': `Bearer ${token}` }
      });
      
      if (response.ok) {
        const data = await response.json();
        setUserDefinedPalettes(data);
      }
    } catch (err) {
      console.error('Error loading user-defined palettes:', err);
    }
  };

  // Load palettes
  const loadPalettes = useCallback(async () => {
    try {
      const token = localStorage.getItem('accessToken');
      const apiUrl = getApiUrl();
      
      let url = `${apiUrl}/colorpalettes`;
      if (selectedCategory) {
        url = `${apiUrl}/colorpalettes/category/${encodeURIComponent(selectedCategory)}`;
      } else if (paletteSearch) {
        url = `${apiUrl}/colorpalettes/search?q=${encodeURIComponent(paletteSearch)}`;
      }
      
      const response = await fetch(url, {
        headers: { 'Authorization': `Bearer ${token}` }
      });
      
      if (response.ok) {
        const data = await response.json();
        setPalettes(data.slice(0, 100)); // Limit to 100 for performance
      }
    } catch (err) {
      console.error('Error loading palettes:', err);
    }
  }, [selectedCategory, paletteSearch]);

  // Load categories and palette count
  const loadPaletteMetadata = async () => {
    try {
      const token = localStorage.getItem('accessToken');
      const apiUrl = getApiUrl();
      
      const [categoriesRes, countRes] = await Promise.all([
        fetch(`${apiUrl}/colorpalettes/categories`, {
          headers: { 'Authorization': `Bearer ${token}` }
        }),
        fetch(`${apiUrl}/colorpalettes/count`, {
          headers: { 'Authorization': `Bearer ${token}` }
        })
      ]);
      
      if (categoriesRes.ok) {
        const cats = await categoriesRes.json();
        setCategories(cats);
      }
      if (countRes.ok) {
        const count = await countRes.json();
        setPaletteCount(count);
      }
    } catch (err) {
      console.error('Error loading palette metadata:', err);
    }
  };

  // Load settings on mount
  useEffect(() => {
    const loadSettings = async () => {
      try {
        const token = localStorage.getItem('accessToken');
        const apiUrl = getApiUrl();
        
        const response = await fetch(`${apiUrl}/systemsettings`, {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        
        if (response.ok) {
          const data = await response.json();
          setFormData({
            companyName: data.companyName || 'CRM System',
            companyLogoUrl: data.companyLogoUrl || '',
            companyLoginLogoUrl: data.companyLoginLogoUrl || '',
            primaryColor: data.primaryColor || '#6750A4',
            secondaryColor: data.secondaryColor || '#625B71',
            tertiaryColor: data.tertiaryColor || '#7D5260',
            surfaceColor: data.surfaceColor || '#FFFBFE',
            backgroundColor: data.backgroundColor || '#FFFBFE',
            companyWebsite: data.companyWebsite || '',
            companyEmail: data.companyEmail || '',
            companyPhone: data.companyPhone || '',
            selectedPaletteId: data.selectedPaletteId || null,
            selectedPaletteName: data.selectedPaletteName || '',
          });
          setUseGroupHeaderColor(data.useGroupHeaderColor || false);
          if (data.companyLogoUrl) {
            setLogoPreview(data.companyLogoUrl);
          }
          if (data.companyLoginLogoUrl) {
            setLoginLogoPreview(data.companyLoginLogoUrl);
          }
          if (data.palettesLastRefreshed) {
            setPalettesLastRefreshed(data.palettesLastRefreshed);
          }
        }
        
        await loadPaletteMetadata();
        await loadUserDefinedPalettes();
      } catch (err) {
        console.error('Error loading settings:', err);
      } finally {
        setLoading(false);
      }
    };
    loadSettings();
  }, []);

  // Load palettes when search or category changes
  useEffect(() => {
    loadPalettes();
  }, [loadPalettes]);

  // Find selected palette when palettes load
  useEffect(() => {
    if (formData.selectedPaletteId && palettes.length > 0) {
      const found = palettes.find(p => p.id === formData.selectedPaletteId);
      if (found) {
        setSelectedPalette(found);
      }
    }
  }, [formData.selectedPaletteId, palettes]);

  const handleRefreshPalettes = async () => {
    setRefreshingPalettes(true);
    try {
      const token = localStorage.getItem('accessToken');
      const apiUrl = getApiUrl();
      
      const response = await fetch(`${apiUrl}/colorpalettes/refresh`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      
      if (response.ok) {
        await loadPalettes();
        await loadPaletteMetadata();
        setPalettesLastRefreshed(new Date().toISOString());
      }
    } catch (err) {
      console.error('Error refreshing palettes:', err);
    } finally {
      setRefreshingPalettes(false);
    }
  };

  const handleFileChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      setUploading(true);
      try {
        const token = localStorage.getItem('accessToken');
        const apiUrl = getApiUrl();
        
        const formDataUpload = new FormData();
        formDataUpload.append('file', file);
        
        const response = await fetch(`${apiUrl}/fileupload/logo`, {
          method: 'POST',
          headers: { 'Authorization': `Bearer ${token}` },
          body: formDataUpload,
        });
        
        if (response.ok) {
          const result = await response.json();
          setLogoPreview(result.url);
          setFormData(prev => ({ ...prev, companyLogoUrl: result.url }));
        }
      } catch (err) {
        console.error('Error uploading logo:', err);
      } finally {
        setUploading(false);
      }
    }
  };

  const handleRemoveLogo = async () => {
    try {
      const token = localStorage.getItem('accessToken');
      const apiUrl = getApiUrl();
      
      await fetch(`${apiUrl}/systemsettings/logo`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      
      setLogoPreview(null);
      setFormData(prev => ({ ...prev, companyLogoUrl: '' }));
    } catch (err) {
      console.error('Error removing logo:', err);
    }
  };

  const handleLoginLogoFileChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      setUploadingLoginLogo(true);
      try {
        const token = localStorage.getItem('accessToken');
        const apiUrl = getApiUrl();
        
        const formDataUpload = new FormData();
        formDataUpload.append('file', file);
        
        const response = await fetch(`${apiUrl}/fileupload/login-logo`, {
          method: 'POST',
          headers: { 'Authorization': `Bearer ${token}` },
          body: formDataUpload,
        });
        
        if (response.ok) {
          const result = await response.json();
          setLoginLogoPreview(result.url);
          setFormData(prev => ({ ...prev, companyLoginLogoUrl: result.url }));
        }
      } catch (err) {
        console.error('Error uploading login logo:', err);
      } finally {
        setUploadingLoginLogo(false);
      }
    }
  };

  const handleRemoveLoginLogo = async () => {
    try {
      const token = localStorage.getItem('accessToken');
      const apiUrl = getApiUrl();
      
      await fetch(`${apiUrl}/systemsettings/login-logo`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      
      setLoginLogoPreview(null);
      setFormData(prev => ({ ...prev, companyLoginLogoUrl: '' }));
    } catch (err) {
      console.error('Error removing login logo:', err);
    }
  };

  const handleResetBranding = async () => {
    try {
      const token = localStorage.getItem('accessToken');
      const apiUrl = getApiUrl();
      
      const response = await fetch(`${apiUrl}/systemsettings/reset-branding`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      
      if (response.ok) {
        // Reset to defaults
        setFormData(prev => ({
          ...prev,
          primaryColor: '#6750A4',
          secondaryColor: '#625B71',
          tertiaryColor: '#7D5260',
          surfaceColor: '#FFFBFE',
          backgroundColor: '#FFFBFE',
          companyLogoUrl: '',
          companyLoginLogoUrl: '',
          selectedPaletteId: null,
          selectedPaletteName: '',
        }));
        setLogoPreview(null);
        setLoginLogoPreview(null);
        setSelectedPalette(null);
        setResetDialogOpen(false);
        setSnackbar({ open: true, message: 'Branding reset to defaults', severity: 'success' });
      }
    } catch (err) {
      console.error('Error resetting branding:', err);
      setSnackbar({ open: true, message: 'Error resetting branding', severity: 'error' });
    }
  };

  const handlePaletteSelect = (palette: ColorPalette | null) => {
    setSelectedPalette(palette);
    if (palette) {
      setFormData(prev => ({
        ...prev,
        selectedPaletteId: palette.id,
        selectedPaletteName: palette.name,
        primaryColor: palette.colors[0] || '#6750A4',
        secondaryColor: palette.colors[1] || '#625B71',
        tertiaryColor: palette.colors[2] || '#7D5260',
        surfaceColor: palette.colors[3] || '#FFFBFE',
        backgroundColor: palette.colors[4] || '#FFFBFE',
      }));
    } else {
      setFormData(prev => ({
        ...prev,
        selectedPaletteId: null,
        selectedPaletteName: '',
      }));
    }
  };

  const handleInputChange = (field: string, value: string) => {
    setFormData({ ...formData, [field]: value });
  };

  // Apply changes to preview only (doesn't save to server)
  const handleApply = () => {
    setApplied(true);
    setSnackbar({ open: true, message: 'Preview updated! Click Save to persist changes.', severity: 'success' });
    setTimeout(() => setApplied(false), 2000);
  };

  const handleSave = async () => {
    try {
      const token = localStorage.getItem('accessToken');
      const apiUrl = getApiUrl();
      
      const response = await fetch(`${apiUrl}/systemsettings`, {
        method: 'PUT',
        headers: { 
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          ...formData,
          useGroupHeaderColor,
        }),
      });
      
      if (response.ok) {
        setSaved(true);
        setSnackbar({ open: true, message: 'Branding settings saved successfully!', severity: 'success' });
        setTimeout(() => setSaved(false), 3000);
        // Dispatch event so other components (like navigation) know branding has changed
        window.dispatchEvent(new CustomEvent('brandingUpdated', { detail: formData }));
      }
    } catch (err) {
      console.error('Error saving settings:', err);
      setSnackbar({ open: true, message: 'Error saving settings', severity: 'error' });
    }
  };

  // Create custom palette
  const handleCreateCustomPalette = async () => {
    if (!customPaletteName.trim()) {
      setSnackbar({ open: true, message: 'Please enter a palette name', severity: 'error' });
      return;
    }
    
    try {
      const token = localStorage.getItem('accessToken');
      const apiUrl = getApiUrl();
      
      const response = await fetch(`${apiUrl}/colorpalettes/custom`, {
        method: 'POST',
        headers: { 
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          name: customPaletteName,
          colors: customColors,
        }),
      });
      
      if (response.ok) {
        const newPalette = await response.json();
        setUserDefinedPalettes(prev => [...prev, newPalette]);
        setCustomPaletteDialogOpen(false);
        setCustomPaletteName('');
        setCustomColors(['#6750A4', '#625B71', '#7D5260', '#FFFBFE', '#FFFBFE']);
        setSnackbar({ open: true, message: 'Custom palette created!', severity: 'success' });
        await loadPaletteMetadata();
      }
    } catch (err) {
      console.error('Error creating custom palette:', err);
      setSnackbar({ open: true, message: 'Error creating palette', severity: 'error' });
    }
  };

  // Delete custom palette
  const handleDeleteCustomPalette = async (paletteId: number) => {
    try {
      const token = localStorage.getItem('accessToken');
      const apiUrl = getApiUrl();
      
      const response = await fetch(`${apiUrl}/colorpalettes/custom/${paletteId}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      
      if (response.ok) {
        setUserDefinedPalettes(prev => prev.filter(p => p.id !== paletteId));
        if (selectedPalette?.id === paletteId) {
          setSelectedPalette(null);
        }
        setSnackbar({ open: true, message: 'Palette deleted', severity: 'success' });
      }
    } catch (err) {
      console.error('Error deleting palette:', err);
      setSnackbar({ open: true, message: 'Error deleting palette', severity: 'error' });
    }
  };

  // Handle RGB color change
  const handleColorChange = (colorKey: string, hex: string) => {
    setFormData(prev => ({ ...prev, [colorKey]: hex }));
  };

  // Open RGB editor for a color
  const openRgbEditor = (index: number, colorKey: string) => {
    const currentColor = formData[colorKey as keyof typeof formData] as string;
    const rgb = hexToRgb(currentColor);
    setRgbValues(rgb);
    setEditingColorIndex(index);
    setEditingColorKey(colorKey);
    setOriginalColorBeforeEdit(currentColor);
  };

  // Cancel RGB editing and revert to original color
  const cancelRgbEditor = () => {
    if (editingColorKey && originalColorBeforeEdit) {
      setFormData(prev => ({ ...prev, [editingColorKey]: originalColorBeforeEdit }));
    }
    setEditingColorIndex(null);
    setEditingColorKey(null);
    setOriginalColorBeforeEdit('');
  };

  // Confirm RGB editing
  const confirmRgbEditor = () => {
    setEditingColorIndex(null);
    setEditingColorKey(null);
    setOriginalColorBeforeEdit('');
  };

  // Update color from RGB sliders
  const handleRgbChange = (colorKey: string) => {
    const hex = rgbToHex(rgbValues.r, rgbValues.g, rgbValues.b);
    setFormData(prev => ({ ...prev, [colorKey]: hex }));
  };

  if (loading) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>;
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h6" sx={{ fontWeight: 600 }}>
          Company Branding & Settings
        </Typography>
        <Button
          variant="outlined"
          color="error"
          startIcon={<ResetIcon />}
          onClick={() => setResetDialogOpen(true)}
          sx={{ textTransform: 'none' }}
        >
          Reset Branding
        </Button>
      </Box>

      {saved && (
        <Alert severity="success" sx={{ mb: 2, borderRadius: 2 }}>
          Company settings saved successfully!
        </Alert>
      )}

      <Grid container spacing={3}>
        {/* Logo Section */}
        <Grid item xs={12} md={4}>
          <Card sx={{ borderRadius: 3, boxShadow: 1, textAlign: 'center' }}>
            <CardContent>
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 2, color: '#625B71' }}>
                Navigation Logo
              </Typography>
              <Typography variant="caption" sx={{ color: '#79747E', display: 'block', mb: 2 }}>
                Used in the app header/navigation (150×150px)
              </Typography>
              {logoPreview ? (
                <Box sx={{ position: 'relative', display: 'inline-block' }}>
                  <Avatar
                    src={logoPreview}
                    sx={{
                      width: 120,
                      height: 120,
                      margin: '0 auto',
                      mb: 2,
                      backgroundColor: '#E8DEF8',
                    }}
                  />
                  <Tooltip title="Remove Logo">
                    <IconButton
                      size="small"
                      onClick={handleRemoveLogo}
                      sx={{
                        position: 'absolute',
                        top: -8,
                        right: -8,
                        backgroundColor: '#f44336',
                        color: 'white',
                        '&:hover': { backgroundColor: '#d32f2f' },
                      }}
                    >
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </Box>
              ) : (
                <Avatar
                  src={logo}
                  sx={{
                    width: 120,
                    height: 120,
                    margin: '0 auto',
                    mb: 2,
                    backgroundColor: '#E8DEF8',
                    fontSize: '2rem',
                  }}
                >
                  {formData.companyName?.charAt(0)?.toUpperCase()}
                </Avatar>
              )}
              <Typography variant="caption" sx={{ color: '#79747E', display: 'block', mb: 2 }}>
                {logoPreview ? 'Custom logo uploaded' : 'Using default logo'}
              </Typography>
              <Button
                variant="outlined"
                component="label"
                disabled={uploading}
                sx={{
                  textTransform: 'none',
                  color: '#6750A4',
                  borderColor: '#6750A4',
                  width: '100%',
                }}
              >
                {uploading ? 'Uploading...' : 'Upload Logo'}
                <input
                  hidden
                  accept="image/*"
                  type="file"
                  onChange={handleFileChange}
                />
              </Button>
              <Typography variant="caption" sx={{ color: '#79747E', display: 'block', mt: 1 }}>
                Auto-resized to 150×150px (max 5MB)
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        {/* Login Page Logo Section */}
        <Grid item xs={12} md={4}>
          <Card sx={{ borderRadius: 3, boxShadow: 1, textAlign: 'center' }}>
            <CardContent>
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 2, color: '#625B71' }}>
                Login Page Logo
              </Typography>
              <Typography variant="caption" sx={{ color: '#79747E', display: 'block', mb: 2 }}>
                Displayed on the login page (400px width)
              </Typography>
              {loginLogoPreview ? (
                <Box sx={{ position: 'relative', display: 'inline-block' }}>
                  <Box
                    component="img"
                    src={loginLogoPreview}
                    alt="Login Logo"
                    sx={{
                      maxWidth: 180,
                      maxHeight: 120,
                      objectFit: 'contain',
                      margin: '0 auto',
                      mb: 2,
                      borderRadius: 2,
                      border: '1px solid #E8DEF8',
                    }}
                  />
                  <Tooltip title="Remove Login Logo">
                    <IconButton
                      size="small"
                      onClick={handleRemoveLoginLogo}
                      sx={{
                        position: 'absolute',
                        top: -8,
                        right: -8,
                        backgroundColor: '#f44336',
                        color: 'white',
                        '&:hover': { backgroundColor: '#d32f2f' },
                      }}
                    >
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </Box>
              ) : (
                <Box
                  sx={{
                    width: 180,
                    height: 120,
                    margin: '0 auto',
                    mb: 2,
                    backgroundColor: '#E8DEF8',
                    borderRadius: 2,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                  }}
                >
                  <LoginIcon sx={{ fontSize: 48, color: '#79747E' }} />
                </Box>
              )}
              <Typography variant="caption" sx={{ color: '#79747E', display: 'block', mb: 2 }}>
                {loginLogoPreview ? 'Custom login logo uploaded' : 'No login logo set'}
              </Typography>
              <Button
                variant="outlined"
                component="label"
                disabled={uploadingLoginLogo}
                sx={{
                  textTransform: 'none',
                  color: '#6750A4',
                  borderColor: '#6750A4',
                  width: '100%',
                }}
              >
                {uploadingLoginLogo ? 'Uploading...' : 'Upload Login Logo'}
                <input
                  hidden
                  accept="image/*"
                  type="file"
                  onChange={handleLoginLogoFileChange}
                />
              </Button>
              <Typography variant="caption" sx={{ color: '#79747E', display: 'block', mt: 1 }}>
                Auto-resized to 400px width (max 5MB)
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={4}>
          <Paper sx={{ borderRadius: 3, p: 3 }}>
            <Grid container spacing={2}>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Company Name"
                  value={formData.companyName}
                  onChange={(e) => handleInputChange('companyName', e.target.value)}
                  variant="outlined"
                  sx={{ borderRadius: 2 }}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Company Website"
                  value={formData.companyWebsite}
                  onChange={(e) => handleInputChange('companyWebsite', e.target.value)}
                  variant="outlined"
                  type="url"
                />
              </Grid>

              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Support Email"
                  value={formData.companyEmail}
                  onChange={(e) => handleInputChange('companyEmail', e.target.value)}
                  variant="outlined"
                  type="email"
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Support Phone"
                  value={formData.companyPhone}
                  onChange={(e) => handleInputChange('companyPhone', e.target.value)}
                  variant="outlined"
                />
              </Grid>

              {/* Color Palette Section */}
              <Grid item xs={12}>
                <Divider sx={{ my: 2 }} />
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <PaletteIcon color="primary" />
                    <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                      Color Palette
                    </Typography>
                    <Chip 
                      label={`${paletteCount.toLocaleString()} palettes available`} 
                      size="small" 
                      color="primary" 
                      variant="outlined"
                    />
                  </Box>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    {palettesLastRefreshed && (
                      <Typography variant="caption" sx={{ color: '#79747E' }}>
                        Last updated: {new Date(palettesLastRefreshed).toLocaleDateString()}
                      </Typography>
                    )}
                    <Tooltip title="Refresh palettes from GitHub">
                      <IconButton 
                        onClick={handleRefreshPalettes}
                        disabled={refreshingPalettes}
                        color="primary"
                      >
                        {refreshingPalettes ? <CircularProgress size={20} /> : <RefreshIcon />}
                      </IconButton>
                    </Tooltip>
                  </Box>
                </Box>
              </Grid>

              <Grid item xs={12} sm={4}>
                <FormControl fullWidth>
                  <InputLabel>Category</InputLabel>
                  <Select
                    value={selectedCategory}
                    label="Category"
                    onChange={(e) => {
                      setSelectedCategory(e.target.value);
                      setPaletteSearch('');
                    }}
                  >
                    <MenuItem value="">All Categories</MenuItem>
                    {categories.map((cat) => (
                      <MenuItem key={cat} value={cat}>{cat}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>

              <Grid item xs={12} sm={8}>
                <Box sx={{ display: 'flex', gap: 1 }}>
                  <Autocomplete
                    options={palettes}
                    getOptionLabel={(option) => option.name}
                    value={selectedPalette}
                    onChange={(_, newValue) => handlePaletteSelect(newValue)}
                    onInputChange={(_, newInputValue) => {
                      if (!selectedCategory) {
                        setPaletteSearch(newInputValue);
                      }
                    }}
                    groupBy={(option) => option.isUserDefined ? '⭐ User Defined' : option.category}
                    renderOption={(props, option) => (
                      <Box component="li" {...props} sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Box sx={{ display: 'flex', gap: 0.5 }}>
                          {(option.colors || []).map((color, idx) => (
                            <Box
                              key={idx}
                              sx={{
                                width: 20,
                                height: 20,
                                backgroundColor: color,
                                borderRadius: 0.5,
                                border: '1px solid #ddd',
                              }}
                            />
                          ))}
                        </Box>
                        <Typography variant="body2">{option.name}</Typography>
                        {option.isUserDefined && (
                          <Chip label="Custom" size="small" color="secondary" sx={{ ml: 'auto' }} />
                        )}
                      </Box>
                    )}
                    renderInput={(params) => (
                      <TextField 
                        {...params} 
                        label="Search Palettes" 
                        placeholder="Type to search palettes..."
                      />
                    )}
                    sx={{ flex: 1 }}
                  />
                  <Tooltip title="Create Custom Palette">
                    <Button
                      variant="outlined"
                      onClick={() => {
                        setCustomColors([
                          formData.primaryColor,
                          formData.secondaryColor,
                          formData.tertiaryColor,
                          formData.surfaceColor,
                          formData.backgroundColor,
                        ]);
                        setCustomPaletteDialogOpen(true);
                      }}
                      sx={{ minWidth: 'auto', px: 2 }}
                    >
                      <AddIcon />
                    </Button>
                  </Tooltip>
                </Box>
              </Grid>

              {/* Selected Palette Preview */}
              {selectedPalette && (
                <Grid item xs={12}>
                  <Paper 
                    variant="outlined" 
                    sx={{ 
                      p: 2, 
                      display: 'flex', 
                      alignItems: 'center', 
                      gap: 2,
                      backgroundColor: '#f5f5f5',
                    }}
                  >
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      Selected: {selectedPalette.name}
                    </Typography>
                    <Box sx={{ display: 'flex', gap: 0.5 }}>
                      {(selectedPalette.colors || []).map((color, idx) => (
                        <Tooltip key={idx} title={`${COLOR_DESCRIPTIONS[idx]?.label || `Color ${idx+1}`}: ${color}`}>
                          <Box
                            sx={{
                              width: 40,
                              height: 40,
                              backgroundColor: color,
                              borderRadius: 1,
                              border: '2px solid #ddd',
                              cursor: 'pointer',
                            }}
                          />
                        </Tooltip>
                      ))}
                    </Box>
                    {selectedPalette.isUserDefined && (
                      <IconButton 
                        size="small" 
                        color="error"
                        onClick={() => selectedPalette.id && handleDeleteCustomPalette(selectedPalette.id)}
                      >
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    )}
                  </Paper>
                </Grid>
              )}

              {/* Header Color Source Toggle */}
              <Grid item xs={12}>
                <Paper variant="outlined" sx={{ p: 2, backgroundColor: '#f9f9ff' }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                    <Box>
                      <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
                        Header Color Source
                      </Typography>
                      <Typography variant="caption" sx={{ color: '#79747E' }}>
                        {useGroupHeaderColor 
                          ? 'Each user group can have its own header color (set in Group Management)'
                          : 'Header color is determined by the palette primary color'}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Typography variant="body2" sx={{ color: useGroupHeaderColor ? '#79747E' : '#6750A4' }}>
                        Palette
                      </Typography>
                      <Switch
                        checked={useGroupHeaderColor}
                        onChange={(e) => setUseGroupHeaderColor(e.target.checked)}
                        color="primary"
                      />
                      <Typography variant="body2" sx={{ color: useGroupHeaderColor ? '#6750A4' : '#79747E' }}>
                        Group
                      </Typography>
                    </Box>
                  </Box>
                </Paper>
              </Grid>

              <Grid item xs={12}>
                <Divider sx={{ my: 2 }} />
                <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 2 }}>
                  Brand Colors
                </Typography>
              </Grid>

              {/* All 5 Brand Colors with Descriptions and RGB Controls */}
              {COLOR_DESCRIPTIONS.map((colorDef, idx) => {
                const colorValue = formData[colorDef.key as keyof typeof formData] as string;
                const isEditing = editingColorIndex === idx;
                
                return (
                  <Grid item xs={12} key={colorDef.key}>
                    <Paper variant="outlined" sx={{ p: 2 }}>
                      <Grid container spacing={2} alignItems="center">
                        <Grid item xs={12} sm={3}>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Box
                              sx={{
                                width: 40,
                                height: 40,
                                backgroundColor: colorValue,
                                borderRadius: 1,
                                border: '2px solid #ddd',
                              }}
                            />
                            <Box>
                              <Typography variant="body2" sx={{ fontWeight: 600 }}>
                                {colorDef.label}
                              </Typography>
                              <Typography variant="caption" sx={{ color: '#79747E' }}>
                                {colorDef.description}
                              </Typography>
                            </Box>
                          </Box>
                        </Grid>
                        <Grid item xs={12} sm={3}>
                          <TextField
                            fullWidth
                            size="small"
                            value={colorValue}
                            onChange={(e) => handleColorChange(colorDef.key, e.target.value)}
                            type="color"
                            InputProps={{
                              style: { height: 40 },
                            }}
                          />
                        </Grid>
                        <Grid item xs={12} sm={2}>
                          <TextField
                            fullWidth
                            size="small"
                            label="Hex"
                            value={colorValue}
                            onChange={(e) => handleColorChange(colorDef.key, e.target.value)}
                          />
                        </Grid>
                        <Grid item xs={12} sm={4}>
                          {isEditing ? (
                            <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
                              <Box sx={{ flex: 1 }}>
                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                  <Typography variant="caption" sx={{ width: 15 }}>R</Typography>
                                  <Slider
                                    size="small"
                                    value={rgbValues.r}
                                    min={0}
                                    max={255}
                                    onChange={(_, v) => {
                                      setRgbValues(prev => ({ ...prev, r: v as number }));
                                      handleRgbChange(colorDef.key);
                                    }}
                                    sx={{ color: '#f44336' }}
                                  />
                                  <Typography variant="caption" sx={{ width: 25 }}>{rgbValues.r}</Typography>
                                </Box>
                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                  <Typography variant="caption" sx={{ width: 15 }}>G</Typography>
                                  <Slider
                                    size="small"
                                    value={rgbValues.g}
                                    min={0}
                                    max={255}
                                    onChange={(_, v) => {
                                      setRgbValues(prev => ({ ...prev, g: v as number }));
                                      handleRgbChange(colorDef.key);
                                    }}
                                    sx={{ color: '#4caf50' }}
                                  />
                                  <Typography variant="caption" sx={{ width: 25 }}>{rgbValues.g}</Typography>
                                </Box>
                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                  <Typography variant="caption" sx={{ width: 15 }}>B</Typography>
                                  <Slider
                                    size="small"
                                    value={rgbValues.b}
                                    min={0}
                                    max={255}
                                    onChange={(_, v) => {
                                      setRgbValues(prev => ({ ...prev, b: v as number }));
                                      handleRgbChange(colorDef.key);
                                    }}
                                    sx={{ color: '#2196f3' }}
                                  />
                                  <Typography variant="caption" sx={{ width: 25 }}>{rgbValues.b}</Typography>
                                </Box>
                              </Box>
                              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
                                <IconButton size="small" onClick={confirmRgbEditor} color="success" title="Confirm">
                                  ✓
                                </IconButton>
                                <IconButton size="small" onClick={cancelRgbEditor} color="error" title="Cancel">
                                  ✕
                                </IconButton>
                              </Box>
                            </Box>
                          ) : (
                            <Button
                              size="small"
                              variant="outlined"
                              onClick={() => openRgbEditor(idx, colorDef.key)}
                            >
                              RGB Editor
                            </Button>
                          )}
                        </Grid>
                      </Grid>
                    </Paper>
                  </Grid>
                );
              })}

              {/* Preview Window */}
              <Grid item xs={12}>
                <Divider sx={{ my: 2 }} />
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                  <PreviewIcon color="primary" />
                  <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                    Live Preview
                  </Typography>
                </Box>
                <Paper 
                  variant="outlined" 
                  sx={{ 
                    p: 0, 
                    overflow: 'hidden',
                    borderRadius: 2,
                    border: '2px solid #ddd',
                  }}
                >
                  {/* Mock Header */}
                  <Box 
                    sx={{ 
                      backgroundColor: formData.primaryColor, 
                      p: 2, 
                      display: 'flex', 
                      alignItems: 'center', 
                      gap: 2 
                    }}
                  >
                    <Typography sx={{ color: '#fff', fontWeight: 600 }}>
                      {formData.companyName || 'Company Name'}
                    </Typography>
                    <Box sx={{ ml: 'auto', display: 'flex', gap: 1 }}>
                      <Box sx={{ 
                        backgroundColor: 'rgba(255,255,255,0.2)', 
                        px: 2, 
                        py: 0.5, 
                        borderRadius: 1,
                        color: '#fff',
                        fontSize: '0.875rem'
                      }}>
                        Dashboard
                      </Box>
                      <Box sx={{ 
                        backgroundColor: formData.secondaryColor, 
                        px: 2, 
                        py: 0.5, 
                        borderRadius: 1,
                        color: '#fff',
                        fontSize: '0.875rem'
                      }}>
                        Contacts
                      </Box>
                    </Box>
                  </Box>
                  
                  {/* Mock Content */}
                  <Box sx={{ backgroundColor: formData.backgroundColor, p: 3 }}>
                    <Grid container spacing={2}>
                      {/* Mock Cards */}
                      <Grid item xs={4}>
                        <Paper 
                          elevation={0} 
                          sx={{ 
                            backgroundColor: formData.surfaceColor, 
                            p: 2, 
                            borderRadius: 2,
                            border: '1px solid #eee'
                          }}
                        >
                          <Typography variant="subtitle2" sx={{ mb: 1 }}>Sales</Typography>
                          <Typography variant="h5" sx={{ fontWeight: 600 }}>$12,450</Typography>
                          <Chip 
                            label="+12%" 
                            size="small" 
                            sx={{ 
                              mt: 1, 
                              backgroundColor: formData.tertiaryColor,
                              color: '#fff'
                            }} 
                          />
                        </Paper>
                      </Grid>
                      <Grid item xs={4}>
                        <Paper 
                          elevation={0} 
                          sx={{ 
                            backgroundColor: formData.surfaceColor, 
                            p: 2, 
                            borderRadius: 2,
                            border: '1px solid #eee'
                          }}
                        >
                          <Typography variant="subtitle2" sx={{ mb: 1 }}>Contacts</Typography>
                          <Typography variant="h5" sx={{ fontWeight: 600 }}>1,284</Typography>
                          <Typography 
                            variant="caption" 
                            sx={{ color: formData.primaryColor, cursor: 'pointer' }}
                          >
                            View all →
                          </Typography>
                        </Paper>
                      </Grid>
                      <Grid item xs={4}>
                        <Paper 
                          elevation={0} 
                          sx={{ 
                            backgroundColor: formData.surfaceColor, 
                            p: 2, 
                            borderRadius: 2,
                            border: '1px solid #eee'
                          }}
                        >
                          <Typography variant="subtitle2" sx={{ mb: 1 }}>Tasks</Typography>
                          <Typography variant="h5" sx={{ fontWeight: 600 }}>23</Typography>
                          <Button 
                            size="small" 
                            variant="contained"
                            sx={{ 
                              mt: 1, 
                              backgroundColor: formData.secondaryColor,
                              textTransform: 'none',
                              fontSize: '0.75rem'
                            }}
                          >
                            Add Task
                          </Button>
                        </Paper>
                      </Grid>
                    </Grid>
                  </Box>
                </Paper>
              </Grid>

              <Grid item xs={12}>
                <Box sx={{ display: 'flex', gap: 2, mt: 2 }}>
                  <Button
                    variant="outlined"
                    onClick={handleApply}
                    startIcon={<PreviewIcon />}
                    sx={{
                      textTransform: 'none',
                      borderRadius: 2,
                      px: 4,
                    }}
                  >
                    Apply Preview
                  </Button>
                  <Button
                    variant="contained"
                    onClick={handleSave}
                    startIcon={<SaveIcon />}
                    sx={{
                      backgroundColor: '#6750A4',
                      textTransform: 'none',
                      borderRadius: 2,
                      px: 4,
                    }}
                  >
                    Save Branding Settings
                  </Button>
                </Box>
              </Grid>
            </Grid>
          </Paper>
        </Grid>
      </Grid>

      {/* Reset Confirmation Dialog */}
      <Dialog open={resetDialogOpen} onClose={() => setResetDialogOpen(false)}>
        <DialogTitle>Reset Branding?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            This will reset the logo and all brand colors to their default values. 
            This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setResetDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleResetBranding} color="error" variant="contained">
            Reset Branding
          </Button>
        </DialogActions>
      </Dialog>

      {/* Create Custom Palette Dialog */}
      <Dialog 
        open={customPaletteDialogOpen} 
        onClose={() => setCustomPaletteDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Create Custom Palette</DialogTitle>
        <DialogContent>
          <DialogContentText sx={{ mb: 2 }}>
            Save your current colors as a custom palette. Custom palettes are saved under "User Defined" category and won't be deleted when refreshing from GitHub.
          </DialogContentText>
          <TextField
            autoFocus
            fullWidth
            label="Palette Name"
            value={customPaletteName}
            onChange={(e) => setCustomPaletteName(e.target.value)}
            sx={{ mb: 3 }}
          />
          <Typography variant="subtitle2" sx={{ mb: 2 }}>Colors in this palette:</Typography>
          <Box sx={{ display: 'flex', gap: 1, mb: 2 }}>
            {customColors.map((color, idx) => (
              <Box key={idx}>
                <input
                  type="color"
                  value={color}
                  onChange={(e) => {
                    const newColors = [...customColors];
                    newColors[idx] = e.target.value;
                    setCustomColors(newColors);
                  }}
                  style={{ 
                    width: 50, 
                    height: 50, 
                    border: 'none', 
                    borderRadius: 4,
                    cursor: 'pointer'
                  }}
                />
                <Typography variant="caption" display="block" sx={{ textAlign: 'center' }}>
                  {COLOR_DESCRIPTIONS[idx]?.label || `Color ${idx+1}`}
                </Typography>
              </Box>
            ))}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCustomPaletteDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleCreateCustomPalette} variant="contained" startIcon={<SaveIcon />}>
            Save Palette
          </Button>
        </DialogActions>
      </Dialog>

      {/* Snackbar for notifications */}
      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar({ ...snackbar, open: false })}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert 
          onClose={() => setSnackbar({ ...snackbar, open: false })} 
          severity={snackbar.severity}
          sx={{ width: '100%' }}
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}

// Settings section configuration
interface SettingsSection {
  id: string;
  title: string;
  description: string;
  icon: React.ReactNode;
  color: string;
  items: SettingsTab[];
}

function SettingsPage() {
  useAuth();
  const [expandedSection, setExpandedSection] = useState<string | false>('crmadmin');
  const [activeTab, setActiveTab] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error] = useState('');

  useEffect(() => {
    setLoading(false);
  }, []);

  // Define the three main sections
  const sections: SettingsSection[] = [
    {
      id: 'systemadmin',
      title: 'System Administration',
      description: 'Core technical settings, database, deployment, monitoring, and security',
      icon: <AdminPanelSettingsIcon />,
      color: '#1976d2',
      items: [
        {
          id: 'database',
          label: 'Database Settings',
          icon: <StorageIcon sx={{ fontSize: 20 }} />,
          component: <DatabaseSettingsTab />,
        },
        {
          id: 'deployment',
          label: 'Deployment & Hosting',
          icon: <CloudIcon sx={{ fontSize: 20 }} />,
          component: <DeploymentSettingsTab />,
        },
        {
          id: 'monitoring',
          label: 'Monitoring',
          icon: <MonitorIcon sx={{ fontSize: 20 }} />,
          component: <MonitoringSettingsTab />,
        },
        {
          id: 'security',
          label: 'Security',
          icon: <SecurityIcon sx={{ fontSize: 20 }} />,
          component: <SecuritySettingsTab />,
        },
        {
          id: 'features',
          label: 'Features & Modules',
          icon: <FeatureToggleIcon sx={{ fontSize: 20 }} />,
          component: <FeatureManagementTab />,
        },
      ],
    },
    {
      id: 'useradmin',
      title: 'User Administration',
      description: 'User management, approvals, groups, and authentication settings',
      icon: <ManageAccountsIcon />,
      color: '#388e3c',
      items: [
        {
          id: 'users',
          label: 'User Management',
          icon: <PeopleIcon sx={{ fontSize: 20 }} />,
          component: <UserManagementTab />,
        },
        {
          id: 'approvals',
          label: 'User Approvals',
          icon: <PersonAddIcon sx={{ fontSize: 20 }} />,
          component: <UserApprovalTab />,
        },
        {
          id: 'groups',
          label: 'Group Management',
          icon: <GroupsIcon sx={{ fontSize: 20 }} />,
          component: <GroupManagementTab />,
        },
        {
          id: 'sociallogin',
          label: 'Social Login',
          icon: <LoginIcon sx={{ fontSize: 20 }} />,
          component: <SocialLoginSettingsTab />,
        },
      ],
    },
    {
      id: 'crmadmin',
      title: 'CRM Administration',
      description: 'Branding, navigation, modules, service requests, and master data',
      icon: <HubIcon />,
      color: '#7b1fa2',
      items: [
        {
          id: 'branding',
          label: 'Company Branding',
          icon: <BusinessIcon sx={{ fontSize: 20 }} />,
          component: <CompanyBrandingTab />,
        },
        {
          id: 'navigation',
          label: 'Navigation',
          icon: <MenuIcon sx={{ fontSize: 20 }} />,
          component: <NavigationSettingsTab />,
        },
        {
          id: 'modules',
          label: 'Modules & Fields',
          icon: <ModuleIcon sx={{ fontSize: 20 }} />,
          component: <ModuleFieldSettingsTab />,
        },
        {
          id: 'servicerequests',
          label: 'Service Request Definitions',
          icon: <SupportAgentIcon sx={{ fontSize: 20 }} />,
          component: <ServiceRequestSettingsTab />,
        },
        {
          id: 'masterdata',
          label: 'Master Data',
          icon: <MasterDataIcon sx={{ fontSize: 20 }} />,
          component: <MasterDataSettingsTab />,
        },
      ],
    },
  ];

  const handleAccordionChange = (sectionId: string) => (_event: React.SyntheticEvent, isExpanded: boolean) => {
    setExpandedSection(isExpanded ? sectionId : false);
    if (!isExpanded) {
      setActiveTab(null);
    }
  };

  const handleTabSelect = (tabId: string) => {
    setActiveTab(activeTab === tabId ? null : tabId);
  };

  // Find the active tab component
  const getActiveComponent = () => {
    for (const section of sections) {
      const tab = section.items.find(item => item.id === activeTab);
      if (tab) return tab.component;
    }
    return null;
  };

  if (error) {
    return (
      <Alert severity="error" sx={{ mt: 2 }}>
        {error}
      </Alert>
    );
  }

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ py: 2 }}>
      {/* Header */}
      <Box sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
        <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
          <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
        </Box>
        <Box>
          <Typography variant="h3" sx={{ fontWeight: 700, mb: 0.5 }}>
            Settings
          </Typography>
          <Typography color="textSecondary" variant="body2">
            Manage your CRM configuration and preferences
          </Typography>
        </Box>
      </Box>

      <Grid container spacing={3}>
        {/* Left sidebar with sections */}
        <Grid item xs={12} md={4} lg={3}>
          <Box sx={{ position: 'sticky', top: 16 }}>
            {sections.map((section) => (
              <Accordion
                key={section.id}
                expanded={expandedSection === section.id}
                onChange={handleAccordionChange(section.id)}
                sx={{
                  mb: 1,
                  borderRadius: '12px !important',
                  overflow: 'hidden',
                  '&:before': { display: 'none' },
                  boxShadow: expandedSection === section.id ? 3 : 1,
                  border: expandedSection === section.id ? `2px solid ${section.color}` : '1px solid #e0e0e0',
                }}
              >
                <AccordionSummary
                  expandIcon={<ExpandMoreIcon />}
                  sx={{
                    bgcolor: expandedSection === section.id ? `${section.color}10` : 'transparent',
                    '& .MuiAccordionSummary-content': { my: 1.5 },
                  }}
                >
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                    <Avatar
                      sx={{
                        bgcolor: section.color,
                        width: 36,
                        height: 36,
                      }}
                    >
                      {section.icon}
                    </Avatar>
                    <Box>
                      <Typography variant="subtitle1" sx={{ fontWeight: 600, lineHeight: 1.2 }}>
                        {section.title}
                      </Typography>
                      <Typography variant="caption" color="textSecondary" sx={{ lineHeight: 1.2 }}>
                        {section.items.length} settings
                      </Typography>
                    </Box>
                  </Box>
                </AccordionSummary>
                <AccordionDetails sx={{ p: 0 }}>
                  <List dense sx={{ py: 0 }}>
                    {section.items.map((item) => (
                      <ListItem key={item.id} disablePadding>
                        <ListItemButton
                          selected={activeTab === item.id}
                          onClick={() => handleTabSelect(item.id)}
                          sx={{
                            py: 1.5,
                            pl: 3,
                            '&.Mui-selected': {
                              bgcolor: `${section.color}15`,
                              borderLeft: `3px solid ${section.color}`,
                              '&:hover': {
                                bgcolor: `${section.color}20`,
                              },
                            },
                          }}
                        >
                          <ListItemIcon sx={{ minWidth: 36, color: activeTab === item.id ? section.color : 'inherit' }}>
                            {item.icon}
                          </ListItemIcon>
                          <ListItemText
                            primary={item.label}
                            primaryTypographyProps={{
                              fontSize: '0.875rem',
                              fontWeight: activeTab === item.id ? 600 : 400,
                              color: activeTab === item.id ? section.color : 'inherit',
                            }}
                          />
                        </ListItemButton>
                      </ListItem>
                    ))}
                  </List>
                </AccordionDetails>
              </Accordion>
            ))}
          </Box>
        </Grid>

        {/* Right content area */}
        <Grid item xs={12} md={8} lg={9}>
          {activeTab ? (
            <Paper sx={{ borderRadius: 3, boxShadow: 1, p: 3, minHeight: 400 }}>
              {getActiveComponent()}
            </Paper>
          ) : (
            <Paper sx={{ borderRadius: 3, boxShadow: 1, p: 4, minHeight: 400, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center' }}>
              <SettingsIcon sx={{ fontSize: 80, color: 'action.disabled', mb: 2 }} />
              <Typography variant="h5" color="textSecondary" sx={{ mb: 1 }}>
                Select a Setting
              </Typography>
              <Typography variant="body2" color="textSecondary" textAlign="center" sx={{ maxWidth: 400 }}>
                Choose a section from the left panel and click on a setting to configure it.
              </Typography>
              
              {/* Quick access cards */}
              <Box sx={{ mt: 4, display: 'flex', gap: 2, flexWrap: 'wrap', justifyContent: 'center' }}>
                {sections.map((section) => (
                  <Card
                    key={section.id}
                    sx={{
                      width: 180,
                      cursor: 'pointer',
                      transition: 'all 0.2s',
                      '&:hover': {
                        transform: 'translateY(-4px)',
                        boxShadow: 4,
                      },
                    }}
                    onClick={() => {
                      setExpandedSection(section.id);
                      setActiveTab(section.items[0]?.id || null);
                    }}
                  >
                    <CardContent sx={{ textAlign: 'center', py: 3 }}>
                      <Avatar
                        sx={{
                          bgcolor: section.color,
                          width: 48,
                          height: 48,
                          mx: 'auto',
                          mb: 1.5,
                        }}
                      >
                        {section.icon}
                      </Avatar>
                      <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
                        {section.title.replace(' Administration', '')}
                      </Typography>
                      <Typography variant="caption" color="textSecondary">
                        {section.items.length} options
                      </Typography>
                    </CardContent>
                  </Card>
                ))}
              </Box>
            </Paper>
          )}
        </Grid>
      </Grid>
    </Box>
  );
}

export default SettingsPage;


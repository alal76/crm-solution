// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// Unified UI Settings Page with tabs for Theme & Layout, Logo & Identity, and Colors & Palette

import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Container,
  FormControl,
  FormControlLabel,
  FormLabel,
  Grid,
  MenuItem,
  Radio,
  RadioGroup,
  Select,
  SelectChangeEvent,
  Stack,
  Switch,
  TextField,
  Button,
  Alert,
  CircularProgress,
  Typography,
  Tabs,
  Tab,
} from '@mui/material';
import RestartAltIcon from '@mui/icons-material/RestartAlt';
import PaletteIcon from '@mui/icons-material/Palette';
import BrandingWatermarkIcon from '@mui/icons-material/BrandingWatermark';
import TuneIcon from '@mui/icons-material/Tune';
import { useUIPreferences } from '../../contexts/UIPreferencesContext';
import BrandingSettings from '../../components/admin/BrandingSettings';
import CompanyBrandingTab from '../../components/settings/CompanyBrandingTab';
import logo from '../../assets/logo.png';

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
      id={`ui-tabpanel-${index}`}
      aria-labelledby={`ui-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ py: 3 }}>{children}</Box>}
    </div>
  );
}

// Theme & Layout Tab (original UICustomizationPage content)
const ThemeLayoutTab: React.FC = () => {
  const { preferences, loading, error, savePreferences, resetPreferences } = useUIPreferences();
  const [isSaving, setIsSaving] = React.useState(false);
  const [saveMessage, setSaveMessage] = React.useState<string | null>(null);

  const handleThemeChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const theme = event.target.value as 'light' | 'dark' | 'auto';
    setIsSaving(true);
    try {
      await savePreferences({ theme });
      setSaveMessage('Theme updated successfully');
      setTimeout(() => setSaveMessage(null), 3000);
    } catch (err) {
      setSaveMessage('Failed to save theme');
    } finally {
      setIsSaving(false);
    }
  };

  const handleSidebarPositionChange = async (event: SelectChangeEvent) => {
    setIsSaving(true);
    try {
      await savePreferences({ sidebarPosition: event.target.value as any });
      setSaveMessage('Sidebar position updated');
      setTimeout(() => setSaveMessage(null), 3000);
    } finally {
      setIsSaving(false);
    }
  };

  const handleFontSizeChange = async (event: SelectChangeEvent) => {
    setIsSaving(true);
    try {
      await savePreferences({ fontSize: event.target.value as any });
      setSaveMessage('Font size updated');
      setTimeout(() => setSaveMessage(null), 3000);
    } finally {
      setIsSaving(false);
    }
  };

  const handleBreadcrumbsToggle = async (event: React.ChangeEvent<HTMLInputElement>) => {
    setIsSaving(true);
    try {
      await savePreferences({ showBreadcrumbs: event.target.checked });
    } finally {
      setIsSaving(false);
    }
  };

  const handleStatusBarToggle = async (event: React.ChangeEvent<HTMLInputElement>) => {
    setIsSaving(true);
    try {
      await savePreferences({ showStatusBar: event.target.checked });
    } finally {
      setIsSaving(false);
    }
  };

  const handleResetClick = async () => {
    if (window.confirm('Reset all UI preferences to defaults?')) {
      setIsSaving(true);
      try {
        await resetPreferences();
        setSaveMessage('UI preferences reset to defaults');
        setTimeout(() => setSaveMessage(null), 3000);
      } finally {
        setIsSaving(false);
      }
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '300px' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!preferences) {
    return <Alert severity="error">Failed to load UI preferences</Alert>;
  }

  return (
    <Box>
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {saveMessage && <Alert severity="success" sx={{ mb: 2 }}>{saveMessage}</Alert>}

      <Grid container spacing={3}>
        {/* Theme Settings */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader title="Theme" />
            <CardContent>
              <FormControl fullWidth disabled={isSaving}>
                <FormLabel>Color Scheme</FormLabel>
                <RadioGroup value={preferences.theme} onChange={handleThemeChange} sx={{ mt: 2 }}>
                  <FormControlLabel value="light" control={<Radio />} label="Light" />
                  <FormControlLabel value="dark" control={<Radio />} label="Dark" />
                  <FormControlLabel value="auto" control={<Radio />} label="Auto (System)" />
                </RadioGroup>
              </FormControl>
            </CardContent>
          </Card>
        </Grid>

        {/* Layout Settings */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader title="Layout" />
            <CardContent>
              <FormControl fullWidth sx={{ mb: 2 }}>
                <FormLabel>Sidebar Position</FormLabel>
                <Select
                  value={preferences.sidebarPosition}
                  onChange={handleSidebarPositionChange}
                  disabled={isSaving}
                  size="small"
                >
                  <MenuItem value="left">Left</MenuItem>
                  <MenuItem value="right">Right</MenuItem>
                  <MenuItem value="hidden">Hidden</MenuItem>
                </Select>
              </FormControl>
              <FormControl fullWidth>
                <FormLabel>Font Size</FormLabel>
                <Select
                  value={preferences.fontSize}
                  onChange={handleFontSizeChange}
                  disabled={isSaving}
                  size="small"
                >
                  <MenuItem value="small">Small</MenuItem>
                  <MenuItem value="normal">Normal</MenuItem>
                  <MenuItem value="large">Large</MenuItem>
                </Select>
              </FormControl>
            </CardContent>
          </Card>
        </Grid>

        {/* Display Options */}
        <Grid item xs={12}>
          <Card>
            <CardHeader title="Display Options" />
            <CardContent>
              <Stack spacing={2}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={preferences.showBreadcrumbs}
                      onChange={handleBreadcrumbsToggle}
                      disabled={isSaving}
                    />
                  }
                  label="Show Breadcrumbs Navigation"
                />
                <FormControlLabel
                  control={
                    <Switch
                      checked={preferences.showStatusBar}
                      onChange={handleStatusBarToggle}
                      disabled={isSaving}
                    />
                  }
                  label="Show Status Bar"
                />
                <FormControlLabel
                  control={
                    <Switch
                      checked={preferences.showTopNavigation}
                      onChange={(e) => savePreferences({ showTopNavigation: e.target.checked })}
                      disabled={isSaving}
                    />
                  }
                  label="Show Top Navigation"
                />
              </Stack>
            </CardContent>
          </Card>
        </Grid>

        {/* Date & Time Format */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader title="Date & Time Format" />
            <CardContent>
              <TextField
                fullWidth
                label="Date Format"
                value={preferences.dateFormat}
                onChange={(e) => savePreferences({ dateFormat: e.target.value })}
                disabled={isSaving}
                size="small"
                sx={{ mb: 2 }}
              />
              <TextField
                fullWidth
                label="Time Format"
                value={preferences.timeFormat}
                onChange={(e) => savePreferences({ timeFormat: e.target.value })}
                disabled={isSaving}
                size="small"
              />
            </CardContent>
          </Card>
        </Grid>

        {/* List Settings */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader title="List Settings" />
            <CardContent>
              <TextField
                fullWidth
                label="Default Page Size"
                type="number"
                value={preferences.defaultPageSize}
                onChange={(e) => savePreferences({ defaultPageSize: Number.parseInt(e.target.value) })}
                disabled={isSaving}
                size="small"
              />
            </CardContent>
          </Card>
        </Grid>

        {/* Actions */}
        <Grid item xs={12}>
          <Stack direction="row" spacing={2} sx={{ justifyContent: 'flex-end' }}>
            <Button
              variant="outlined"
              startIcon={<RestartAltIcon />}
              onClick={handleResetClick}
              disabled={isSaving}
            >
              Reset to Defaults
            </Button>
          </Stack>
        </Grid>
      </Grid>
    </Box>
  );
};

export const UICustomizationPage: React.FC = () => {
  const [tabValue, setTabValue] = useState(0);

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
          <img src={logo} alt="CRM Logo" style={{ width: '100%', height: '100%', objectFit: 'contain' }} />
        </Box>
        <Box>
          <Typography variant="h4" fontWeight={700}>UI Settings</Typography>
          <Typography variant="body2" color="text.secondary">
            Customize your interface preferences, branding, logos, and color themes
          </Typography>
        </Box>
      </Box>

      {/* Tabs */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 0 }}>
        <Tabs
          value={tabValue}
          onChange={handleTabChange}
          aria-label="UI Settings tabs"
          sx={{
            '& .MuiTab-root': { textTransform: 'none', fontWeight: 500, fontSize: '0.95rem' },
          }}
        >
          <Tab icon={<TuneIcon />} iconPosition="start" label="Theme & Layout" />
          <Tab icon={<BrandingWatermarkIcon />} iconPosition="start" label="Logo & Identity" />
          <Tab icon={<PaletteIcon />} iconPosition="start" label="Colors & Palette" />
        </Tabs>
      </Box>

      {/* Tab Panels */}
      <TabPanel value={tabValue} index={0}>
        <ThemeLayoutTab />
      </TabPanel>

      <TabPanel value={tabValue} index={1}>
        <BrandingSettings />
      </TabPanel>

      <TabPanel value={tabValue} index={2}>
        <CompanyBrandingTab />
      </TabPanel>
    </Container>
  );
};

export default UICustomizationPage;

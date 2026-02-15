// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

import React from 'react';
import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Container,
  Divider,
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
  Typography
} from '@mui/material';
import RestartAltIcon from '@mui/icons-material/RestartAlt';
import SaveIcon from '@mui/icons-material/Save';
import { useUIPreferences } from '../contexts/UIPreferencesContext';

export const UICustomizationPage: React.FC = () => {
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
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (!preferences) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="error">Failed to load UI preferences</Alert>
      </Container>
    );
  }

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" gutterBottom>UI Customization</Typography>
        <Typography variant="body2" color="textSecondary">
          Customize your interface preferences
        </Typography>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {saveMessage && <Alert severity="success" sx={{ mb: 2 }}>{saveMessage}</Alert>}

      <Grid container spacing={3}>
        {/* Theme Settings */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader title="Theme" />
            <CardContent>
              <FormControl fullWidth>
                <FormLabel>Color Scheme</FormLabel>
                <RadioGroup
                  value={preferences.theme}
                  onChange={handleThemeChange}
                  disabled={isSaving}
                  sx={{ mt: 2 }}
                >
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
                onChange={(e) => savePreferences({ defaultPageSize: parseInt(e.target.value) })}
                disabled={isSaving}
                size="small"
              />
            </CardContent>
          </Card>
        </Grid>

        {/* Actions */}
        <Grid item xs={12}>
          <Stack direction="row" spacing={2} sx={{justifyContent: 'flex-end' }}>
            <Button
              variant="outlined"
              startIcon={<RestartAltIcon />}
              onClick={handleResetClick}
              disabled={isSaving}
            >
              Reset to Defaults
            </Button>
            <Button
              variant="contained"
              startIcon={<SaveIcon />}
              disabled={isSaving}
              onClick={() => setSaveMessage('Preferences updated')}
            >
              {isSaving ? 'Saving...' : 'Save All'}
            </Button>
          </Stack>
        </Grid>
      </Grid>
    </Container>
  );
};

export default UICustomizationPage;

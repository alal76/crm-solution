import React, { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Divider,
  FormControlLabel,
  Switch,
  TextField,
  Typography,
  Snackbar,
  Alert,
  CircularProgress,
} from '@mui/material';
import brandingConfigService, { BrandingConfigDto } from '../../services/brandingConfigService';
import { getApiBaseUrl } from '../../config/ports';
import CompanyBrandingTab from '../settings/CompanyBrandingTab';

const MAX_LOGO_SIZE_BYTES = 2 * 1024 * 1024;
const MAX_FAVICON_SIZE_BYTES = 500 * 1024;
const LOGO_MIN_DIMENSION = 200;
const LOGO_MAX_DIMENSION = 500;
const FAVICON_DIMENSIONS = [32, 64];

const getAssetUrl = (path?: string | null) => {
  if (!path) return null;
  if (path.startsWith('data:')) return path;
  if (path.startsWith('/uploads')) return `${getApiBaseUrl()}${path}`;
  return path;
};

const fileToBase64 = (file: File): Promise<string> =>
  new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => {
      const result = reader.result as string;
      const base64 = result.includes(',') ? result.split(',')[1] : result;
      resolve(base64);
    };
    reader.onerror = () => reject(reader.error);
    reader.readAsDataURL(file);
  });

const loadImageDimensions = (file: File): Promise<{ width: number; height: number }> =>
  new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => {
      const img = new Image();
      img.onload = () => resolve({ width: img.width, height: img.height });
      img.onerror = () => reject(new Error('Unable to read image dimensions'));
      img.src = reader.result as string;
    };
    reader.onerror = () => reject(reader.error);
    reader.readAsDataURL(file);
  });

const BrandingSettings: React.FC = () => {
  const [config, setConfig] = useState<BrandingConfigDto | null>(null);
  const [solutionName, setSolutionName] = useState('');
  const [customBrandingEnabled, setCustomBrandingEnabled] = useState(true);
  const [logoPreview, setLogoPreview] = useState<string | null>(null);
  const [faviconPreview, setFaviconPreview] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>(
    { open: false, message: '', severity: 'success' }
  );

  const showSnackbar = useCallback((message: string, severity: 'success' | 'error') => {
    setSnackbar({ open: true, message, severity });
  }, []);

  const loadConfig = useCallback(async () => {
    try {
      setLoading(true);
      const data = await brandingConfigService.getCurrent();
      setConfig(data);
      setSolutionName(data.solutionName || 'CRM Solution');
      setCustomBrandingEnabled(data.isCustomBrandingEnabled);
      setLogoPreview(getAssetUrl(data.customLogoPath) || getAssetUrl(data.softwareLogoPath));
      setFaviconPreview(getAssetUrl(data.faviconPath) || null);
    } catch (error) {
      console.error('Failed to load branding configuration', error);
      showSnackbar('Failed to load branding configuration', 'error');
    } finally {
      setLoading(false);
    }
  }, [showSnackbar]);

  useEffect(() => {
    loadConfig();
  }, [loadConfig]);

  const handleSaveSolutionName = async () => {
    if (!solutionName.trim()) {
      showSnackbar('Solution name is required', 'error');
      return;
    }

    if (!/^[A-Za-z0-9 ]+$/.test(solutionName.trim())) {
      showSnackbar('Solution name can only contain letters, numbers, and spaces', 'error');
      return;
    }

    setSaving(true);
    try {
      const updated = await brandingConfigService.updateSolutionName(solutionName.trim());
      setConfig(updated);
      showSnackbar('Solution name updated', 'success');
    } catch (error) {
      console.error('Failed to update solution name', error);
      showSnackbar('Failed to update solution name', 'error');
    } finally {
      setSaving(false);
    }
  };

  const handleToggleCustomBranding = async (enabled: boolean) => {
    setSaving(true);
    try {
      const updated = await brandingConfigService.toggleCustomBranding(enabled);
      setConfig(updated);
      setCustomBrandingEnabled(updated.isCustomBrandingEnabled);
      setLogoPreview(getAssetUrl(updated.customLogoPath) || getAssetUrl(updated.softwareLogoPath));
      showSnackbar('Branding setting updated', 'success');
    } catch (error) {
      console.error('Failed to toggle custom branding', error);
      showSnackbar('Failed to update branding setting', 'error');
    } finally {
      setSaving(false);
    }
  };

  const validateLogoFile = async (file: File) => {
    const allowedTypes = ['image/png', 'image/jpeg'];
    if (!allowedTypes.includes(file.type)) {
      return 'Logo must be PNG or JPEG';
    }
    if (file.size > MAX_LOGO_SIZE_BYTES) {
      return 'Logo must be 2MB or smaller';
    }

    const { width, height } = await loadImageDimensions(file);
    if (width !== height) {
      return 'Logo must be a square image';
    }
    if (width < LOGO_MIN_DIMENSION || width > LOGO_MAX_DIMENSION) {
      return `Logo dimensions must be between ${LOGO_MIN_DIMENSION}x${LOGO_MIN_DIMENSION} and ${LOGO_MAX_DIMENSION}x${LOGO_MAX_DIMENSION}px`;
    }

    return null;
  };

  const validateFaviconFile = async (file: File) => {
    const allowedTypes = ['image/png', 'image/x-icon', 'image/vnd.microsoft.icon'];
    if (!allowedTypes.includes(file.type)) {
      return 'Favicon must be PNG or ICO';
    }
    if (file.size > MAX_FAVICON_SIZE_BYTES) {
      return 'Favicon must be 500KB or smaller';
    }

    const { width, height } = await loadImageDimensions(file);
    if (width !== height) {
      return 'Favicon must be a square image';
    }
    if (!FAVICON_DIMENSIONS.includes(width)) {
      return 'Favicon dimensions must be 32x32 or 64x64px';
    }

    return null;
  };

  const handleUploadLogo = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    const error = await validateLogoFile(file);
    if (error) {
      showSnackbar(error, 'error');
      return;
    }

    setSaving(true);
    try {
      const base64 = await fileToBase64(file);
      const response = await brandingConfigService.uploadLogo({
        fileContent: base64,
        fileName: file.name,
        mimeType: file.type,
        fileSizeBytes: file.size,
      });

      if (!response.success) {
        showSnackbar(response.message || 'Logo upload failed', 'error');
        return;
      }

      await loadConfig();
      showSnackbar('Logo uploaded successfully', 'success');
    } catch (error) {
      console.error('Failed to upload logo', error);
      showSnackbar('Failed to upload logo', 'error');
    } finally {
      setSaving(false);
    }
  };

  const handleUploadFavicon = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    const error = await validateFaviconFile(file);
    if (error) {
      showSnackbar(error, 'error');
      return;
    }

    setSaving(true);
    try {
      const base64 = await fileToBase64(file);
      const response = await brandingConfigService.uploadFavicon({
        fileContent: base64,
        fileName: file.name,
        mimeType: file.type,
        fileSizeBytes: file.size,
      });

      if (!response.success) {
        showSnackbar(response.message || 'Favicon upload failed', 'error');
        return;
      }

      await loadConfig();
      showSnackbar('Favicon uploaded successfully', 'success');
    } catch (error) {
      console.error('Failed to upload favicon', error);
      showSnackbar('Failed to upload favicon', 'error');
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteLogo = async () => {
    setSaving(true);
    try {
      const updated = await brandingConfigService.deleteCustomLogo();
      setConfig(updated);
      setLogoPreview(getAssetUrl(updated.softwareLogoPath));
      showSnackbar('Custom logo removed', 'success');
    } catch (error) {
      console.error('Failed to delete logo', error);
      showSnackbar('Failed to delete logo', 'error');
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteFavicon = async () => {
    setSaving(true);
    try {
      const updated = await brandingConfigService.deleteFavicon();
      setConfig(updated);
      setFaviconPreview(null);
      showSnackbar('Favicon removed', 'success');
    } catch (error) {
      console.error('Failed to delete favicon', error);
      showSnackbar('Failed to delete favicon', 'error');
    } finally {
      setSaving(false);
    }
  };

  const disabled = saving || loading;
  const logoLabel = useMemo(() => (customBrandingEnabled ? 'Custom Logo' : 'Software Logo'), [customBrandingEnabled]);

  return (
    <Box display="flex" flexDirection="column" gap={3}>
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            White-label Branding
          </Typography>
          {loading ? (
            <Box display="flex" justifyContent="center" py={4}>
              <CircularProgress size={32} />
            </Box>
          ) : (
            <Box display="flex" flexDirection="column" gap={2}>
              <FormControlLabel
                control={
                  <Switch
                    checked={customBrandingEnabled}
                    onChange={(event) => handleToggleCustomBranding(event.target.checked)}
                    disabled={disabled}
                  />
                }
                label="Enable custom branding"
              />

              <Box display="flex" flexDirection={{ xs: 'column', md: 'row' }} gap={2}>
                <TextField
                  label="Solution name"
                  value={solutionName}
                  onChange={(event) => setSolutionName(event.target.value)}
                  fullWidth
                  disabled={disabled}
                />
                <Button
                  variant="contained"
                  onClick={handleSaveSolutionName}
                  disabled={disabled}
                  sx={{ whiteSpace: 'nowrap' }}
                >
                  Save name
                </Button>
              </Box>

              <Divider />

              <Box display="flex" flexDirection={{ xs: 'column', md: 'row' }} gap={3}>
                <Box flex={1}>
                  <Typography variant="subtitle1" gutterBottom>
                    {logoLabel}
                  </Typography>
                  <Box
                    sx={{
                      width: 140,
                      height: 140,
                      borderRadius: 2,
                      border: '1px solid',
                      borderColor: 'divider',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      mb: 1.5,
                      overflow: 'hidden',
                      bgcolor: 'background.paper',
                    }}
                  >
                    {logoPreview ? (
                      <img src={logoPreview} alt="Logo preview" style={{ maxWidth: '100%', maxHeight: '100%' }} />
                    ) : (
                      <Typography variant="caption" color="text.secondary">
                        No logo uploaded
                      </Typography>
                    )}
                  </Box>
                  <Box display="flex" gap={1} flexWrap="wrap">
                    <Button variant="outlined" component="label" disabled={disabled}>
                      Upload logo
                      <input hidden type="file" accept="image/png,image/jpeg" onChange={handleUploadLogo} />
                    </Button>
                    <Button variant="text" color="error" onClick={handleDeleteLogo} disabled={disabled || !config?.customLogoPath}>
                      Remove custom logo
                    </Button>
                  </Box>
                </Box>

                <Box flex={1}>
                  <Typography variant="subtitle1" gutterBottom>
                    Browser tab icon
                  </Typography>
                  <Box
                    sx={{
                      width: 64,
                      height: 64,
                      borderRadius: 2,
                      border: '1px solid',
                      borderColor: 'divider',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      mb: 1.5,
                      overflow: 'hidden',
                      bgcolor: 'background.paper',
                    }}
                  >
                    {faviconPreview ? (
                      <img src={faviconPreview} alt="Favicon preview" style={{ maxWidth: '100%', maxHeight: '100%' }} />
                    ) : (
                      <Typography variant="caption" color="text.secondary">
                        No favicon
                      </Typography>
                    )}
                  </Box>
                  <Box display="flex" gap={1} flexWrap="wrap">
                    <Button variant="outlined" component="label" disabled={disabled}>
                      Upload favicon
                      <input hidden type="file" accept="image/png,image/x-icon,image/vnd.microsoft.icon" onChange={handleUploadFavicon} />
                    </Button>
                    <Button variant="text" color="error" onClick={handleDeleteFavicon} disabled={disabled || !config?.faviconPath}>
                      Remove favicon
                    </Button>
                  </Box>
                </Box>
              </Box>
            </Box>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Theme & Company Settings
          </Typography>
          <CompanyBrandingTab />
        </CardContent>
      </Card>

      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar(prev => ({ ...prev, open: false }))}
      >
        <Alert severity={snackbar.severity} variant="filled" sx={{ width: '100%' }}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default BrandingSettings;

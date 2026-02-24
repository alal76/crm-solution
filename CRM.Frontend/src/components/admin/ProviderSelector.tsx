import React, { useState } from 'react';
import {
  Box,
  FormControl,
  FormControlLabel,
  Radio,
  RadioGroup,
  Typography,
  Chip,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Button,
  CircularProgress,
} from '@mui/material';
import {
  CheckCircle as HealthyIcon,
  Cancel as UnhealthyIcon,
  HelpOutline as UnknownIcon,
  Settings as ConfiguredIcon,
} from '@mui/icons-material';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ProviderOption {
  value: string;
  label: string;
  description: string;
  isConfigured: boolean;
  isBuiltIn?: boolean;
  isSaaS?: boolean;
  isHealthy?: boolean; // undefined = not checked
}

interface ProviderSelectorProps {
  /** Provider category label (e.g. "Search", "AI") */
  category: string;
  /** Currently active provider type */
  currentProvider: string;
  /** List of available providers for this category */
  providers: ProviderOption[];
  /** Callback when the user confirms a provider switch */
  onProviderChange: (providerType: string) => void;
  /** Loading state while a switch is in progress */
  loading?: boolean;
  /** Disable all interactions */
  disabled?: boolean;
}

// ─── Helper components ────────────────────────────────────────────────────────

function HealthIndicator({ isHealthy, isConfigured }: { isHealthy?: boolean; isConfigured: boolean }) {
  if (!isConfigured) {
    return (
      <Tooltip title="Not configured">
        <UnknownIcon sx={{ fontSize: 16, color: 'text.disabled' }} />
      </Tooltip>
    );
  }
  if (isHealthy === undefined) {
    return (
      <Tooltip title="Health not checked">
        <UnknownIcon sx={{ fontSize: 16, color: 'text.secondary' }} />
      </Tooltip>
    );
  }
  return isHealthy ? (
    <Tooltip title="Healthy">
      <HealthyIcon sx={{ fontSize: 16, color: 'success.main' }} />
    </Tooltip>
  ) : (
    <Tooltip title="Unhealthy or unreachable">
      <UnhealthyIcon sx={{ fontSize: 16, color: 'error.main' }} />
    </Tooltip>
  );
}

// ─── Main component ───────────────────────────────────────────────────────────

/**
 * ProviderSelector — renders a RadioGroup to switch providers within a category.
 *
 * Shows a confirmation dialog before switching to prevent accidental changes.
 * Displays health / configured status alongside each option.
 */
const ProviderSelector: React.FC<ProviderSelectorProps> = ({
  category,
  currentProvider,
  providers,
  onProviderChange,
  loading = false,
  disabled = false,
}) => {
  const [pendingProvider, setPendingProvider] = useState<string | null>(null);

  const handleSelectionChange = (_: React.ChangeEvent<HTMLInputElement>, value: string) => {
    if (value === currentProvider) return;
    setPendingProvider(value);
  };

  const handleConfirm = () => {
    if (pendingProvider) {
      onProviderChange(pendingProvider);
    }
    setPendingProvider(null);
  };

  const handleCancel = () => {
    setPendingProvider(null);
  };

  const pendingLabel = providers.find((p) => p.value === pendingProvider)?.label ?? pendingProvider;

  return (
    <Box>
      {loading && (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
          <CircularProgress size={16} />
          <Typography variant="body2" color="text.secondary">
            Switching provider…
          </Typography>
        </Box>
      )}

      <FormControl component="fieldset" disabled={disabled || loading} fullWidth>
        <RadioGroup value={currentProvider} onChange={handleSelectionChange}>
          {providers.map((provider) => (
            <Box
              key={provider.value}
              sx={{
                display: 'flex',
                alignItems: 'flex-start',
                justifyContent: 'space-between',
                py: 0.75,
                px: 1,
                borderRadius: 1,
                bgcolor: provider.value === currentProvider ? 'action.selected' : 'transparent',
                '&:hover': { bgcolor: 'action.hover' },
                transition: 'background-color 0.15s',
              }}
            >
              <FormControlLabel
                value={provider.value}
                control={<Radio size="small" />}
                label={
                  <Box>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Typography variant="body2" fontWeight={provider.value === currentProvider ? 600 : 400}>
                        {provider.label}
                      </Typography>
                      {provider.value === currentProvider && (
                        <Chip label="Active" size="small" color="primary" sx={{ height: 18, fontSize: 10 }} />
                      )}
                      {provider.isBuiltIn && (
                        <Chip label="Built-in" size="small" variant="outlined" sx={{ height: 18, fontSize: 10 }} />
                      )}
                      {provider.isSaaS && (
                        <Chip label="SaaS" size="small" color="info" variant="outlined" sx={{ height: 18, fontSize: 10 }} />
                      )}
                    </Box>
                    <Typography variant="caption" color="text.secondary">
                      {provider.description}
                    </Typography>
                  </Box>
                }
                sx={{ flex: 1, m: 0, alignItems: 'flex-start', '& .MuiFormControlLabel-label': { pt: 0.25 } }}
              />

              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75, mt: 0.5 }}>
                {provider.isConfigured && (
                  <Tooltip title="Configured">
                    <ConfiguredIcon sx={{ fontSize: 16, color: 'success.main' }} />
                  </Tooltip>
                )}
                <HealthIndicator isHealthy={provider.isHealthy} isConfigured={provider.isConfigured} />
              </Box>
            </Box>
          ))}
        </RadioGroup>
      </FormControl>

      {/* Confirmation dialog */}
      <Dialog
        open={Boolean(pendingProvider)}
        onClose={handleCancel}
        maxWidth="xs"
        fullWidth
      >
        <DialogTitle>Switch {category} Provider?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            You are switching the <strong>{category}</strong> provider to{' '}
            <strong>{pendingLabel}</strong>. This may affect existing functionality
            and require a service restart to take full effect. Continue?
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCancel} color="inherit">
            Cancel
          </Button>
          <Button onClick={handleConfirm} variant="contained" color="warning">
            Switch Provider
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ProviderSelector;

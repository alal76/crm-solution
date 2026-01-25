import React from 'react';
import { Box, Typography, Paper } from '@mui/material';
import { Business as BusinessIcon } from '@mui/icons-material';
import logo from '../../assets/logo.png';

// Note: The Company Branding Tab is complex and currently embedded in SettingsPage.tsx
// For now, this page redirects to the main Settings page with the branding tab active
// TODO: Extract CompanyBrandingTab to its own component file

const BrandingSettingsPage: React.FC = () => {
  React.useEffect(() => {
    // Navigate to settings page with branding tab
    window.location.href = '/settings?tab=branding';
  }, []);

  return (
    <Box sx={{ py: 2 }}>
      {/* Header */}
      <Box sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
        <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
          <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <BusinessIcon sx={{ color: '#7b1fa2', fontSize: 28 }} />
          <Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>
              Company Branding
            </Typography>
            <Typography color="textSecondary" variant="body2">
              Customize logo, colors, and company information
            </Typography>
          </Box>
        </Box>
      </Box>

      <Paper sx={{ p: 3, textAlign: 'center' }}>
        <Typography>Redirecting to branding settings...</Typography>
      </Paper>
    </Box>
  );
};

export default BrandingSettingsPage;

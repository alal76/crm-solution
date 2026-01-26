import React from 'react';
import { Box, Typography } from '@mui/material';
import { SvgIconComponent } from '@mui/icons-material';
import { useBranding } from '../../contexts/BrandingContext';
import logo from '../../assets/logo.png';

interface AdminPageHeaderProps {
  title: string;
  subtitle: string;
  icon: SvgIconComponent;
}

/**
 * Standardized header component for admin settings pages
 * Uses the primary branding color for the icon
 */
const AdminPageHeader: React.FC<AdminPageHeaderProps> = ({ title, subtitle, icon: Icon }) => {
  const { branding } = useBranding();
  
  return (
    <Box sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
      <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
        {branding.companyLogoUrl ? (
          <img 
            src={branding.companyLogoUrl} 
            alt={branding.companyName || 'Company Logo'} 
            style={{ width: "100%", height: "100%", objectFit: "contain" }} 
          />
        ) : (
          <img 
            src={logo} 
            alt="CRM Logo" 
            style={{ width: "100%", height: "100%", objectFit: "contain" }} 
          />
        )}
      </Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <Icon sx={{ color: branding.primaryColor || '#6750A4', fontSize: 28 }} />
        <Box>
          <Typography variant="h4" sx={{ fontWeight: 700, color: 'text.primary' }}>
            {title}
          </Typography>
          <Typography color="textSecondary" variant="body2">
            {subtitle}
          </Typography>
        </Box>
      </Box>
    </Box>
  );
};

export default AdminPageHeader;

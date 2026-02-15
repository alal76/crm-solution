import React from 'react';
import { Avatar, Box, Typography } from '@mui/material';
import { useBranding } from '../../contexts/BrandingContext';
import { getApiBaseUrl } from '../../config/ports';
import defaultLogo from '../../assets/logo.png';

interface LogoDisplayProps {
  size?: number;
  showText?: boolean;
  textVariant?: 'body1' | 'body2' | 'subtitle1' | 'subtitle2' | 'h6';
  textColor?: string;
  align?: 'row' | 'column';
}

const LogoDisplay: React.FC<LogoDisplayProps> = ({
  size = 48,
  showText = false,
  textVariant = 'subtitle1',
  textColor,
  align = 'row',
}) => {
  const { branding } = useBranding();

  const getLogoUrl = () => {
    const logoPath = branding.brandingLogoUrl || branding.companyLogoUrl;
    if (!logoPath) {
      return defaultLogo;
    }

    if (logoPath.startsWith('data:')) {
      return logoPath;
    }

    if (logoPath.startsWith('/uploads')) {
      return `${getApiBaseUrl()}${logoPath}`;
    }

    return logoPath;
  };

  const displayName = branding.solutionName || branding.companyName || 'CRM System';

  return (
    <Box display="flex" alignItems="center" flexDirection={align} gap={align === 'row' ? 1.5 : 0.75}>
      <Avatar
        src={getLogoUrl()}
        alt={displayName}
        sx={{ width: size, height: size, bgcolor: 'transparent' }}
        variant="square"
      />
      {showText && (
        <Typography variant={textVariant} color={textColor} noWrap>
          {displayName}
        </Typography>
      )}
    </Box>
  );
};

export default LogoDisplay;

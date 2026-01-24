import React from 'react';
import { Box, CircularProgress, Typography } from '@mui/material';

export interface LoadingSpinnerProps {
  /** Message to display below the spinner */
  message?: string;
  /** Size of the spinner */
  size?: number;
  /** Full page loading overlay */
  fullPage?: boolean;
  /** Padding for non-full-page mode */
  py?: number;
}

/**
 * Reusable loading spinner component
 * Can be used as a full-page overlay or inline loading indicator
 */
export const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({
  message,
  size = 40,
  fullPage = false,
  py = 4,
}) => {
  if (fullPage) {
    return (
      <Box
        sx={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          backgroundColor: 'rgba(255, 255, 255, 0.8)',
          zIndex: 9999,
        }}
      >
        <CircularProgress size={size} />
        {message && (
          <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
            {message}
          </Typography>
        )}
      </Box>
    );
  }

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        py,
      }}
    >
      <CircularProgress size={size} />
      {message && (
        <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
          {message}
        </Typography>
      )}
    </Box>
  );
};

export default LoadingSpinner;

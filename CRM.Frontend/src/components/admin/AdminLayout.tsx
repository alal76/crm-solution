import React from 'react';
import { Box } from '@mui/material';
import { Outlet } from 'react-router-dom';
import { AdminSettingsMenu } from './AdminSettingsMenu';

/**
 * Admin layout wrapper that renders the AdminSettingsMenu sidebar
 * alongside the admin page content via React Router <Outlet />.
 *
 * Used as a layout route for all /admin/* paths in App.tsx.
 */
const AdminLayout: React.FC = () => {
  return (
    <Box
      sx={{
        display: 'flex',
        minHeight: 'calc(100vh - 200px)',
        mx: -2,          // offset parent Container padding
        width: 'calc(100% + 32px)',
      }}
    >
      {/* Sidebar */}
      <Box
        sx={{
          flexShrink: 0,
          display: { xs: 'none', md: 'block' },
        }}
      >
        <AdminSettingsMenu />
      </Box>

      {/* Content area */}
      <Box
        sx={{
          flexGrow: 1,
          minWidth: 0,
          px: 3,
          py: 1,
          overflow: 'auto',
        }}
      >
        <Outlet />
      </Box>
    </Box>
  );
};

export default AdminLayout;

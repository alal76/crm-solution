import React from 'react';
import { Container } from '@mui/material';
import { Outlet } from 'react-router-dom';

/**
 * Admin layout wrapper for all /admin/* paths.
 * Navigation is handled by the main Navigation drawer (hamburger menu).
 * This layout simply provides consistent spacing for admin page content.
 */
const AdminLayout: React.FC = () => {
  return (
    <Container maxWidth="lg" sx={{ py: 2 }}>
      <Outlet />
    </Container>
  );
};

export default AdminLayout;

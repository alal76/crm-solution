import React from 'react';
import BrandingSettings from '../../components/admin/BrandingSettings';

/**
 * Admin page for Company Branding — logo, favicon, solution name, and colour palette selection.
 * Route: /admin/branding
 */
const BrandingSettingsPage: React.FC = () => {
  return <BrandingSettings />;
};

export default BrandingSettingsPage;

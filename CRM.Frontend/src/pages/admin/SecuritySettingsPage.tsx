/**
 * SecuritySettingsPage — Password policies, session configuration, and SSO/Social Login.
 *
 * UX-CONF-004: Added "SSO & Social Login" tab so SocialLoginSettingsPage can be absorbed.
 *              Redirect /admin/social-login → /admin/security added in App.tsx.
 */
import React, { useState } from 'react';
import { Box, Tabs, Tab, Paper } from '@mui/material';
import { Security as SecurityIcon, Login as SSOIcon } from '@mui/icons-material';
import SecuritySettingsTab from '../../components/settings/SecuritySettingsTab';
import SocialLoginSettingsTab from '../../components/settings/SocialLoginSettingsTab';
import AdminPageHeader from '../../components/admin/AdminPageHeader';

// ─── Tab panel helper ─────────────────────────────────────────────────────────
interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}
function TabPanel({ children, value, index }: TabPanelProps) {
  return (
    <div role="tabpanel" hidden={value !== index}>
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  );
}

const SecuritySettingsPage: React.FC = () => {
  const [tab, setTab] = useState(0);

  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="Security Settings"
        subtitle="Password policies, authentication, session configuration, and SSO providers"
        icon={SecurityIcon}
      />

      <Paper>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs
            value={tab}
            onChange={(_, v: number) => setTab(v)}
            sx={{ '& .MuiTab-root': { textTransform: 'none', fontWeight: 500 } }}
          >
            <Tab icon={<SecurityIcon />} iconPosition="start" label="Security" />
            {/* UX-CONF-004: SSO & Social Login tab — absorbs /admin/social-login */}
            <Tab icon={<SSOIcon />} iconPosition="start" label="SSO & Social Login" />
          </Tabs>
        </Box>

        <Box sx={{ px: 3, pb: 3 }}>
          <TabPanel value={tab} index={0}>
            <SecuritySettingsTab />
          </TabPanel>
          {/* UX-CONF-004: SocialLoginSettingsTab rendered here; standalone /admin/social-login redirects here */}
          <TabPanel value={tab} index={1}>
            <SocialLoginSettingsTab />
          </TabPanel>
        </Box>
      </Paper>
    </Box>
  );
};

export default SecuritySettingsPage;

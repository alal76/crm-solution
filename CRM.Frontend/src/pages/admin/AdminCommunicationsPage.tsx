/**
 * AdminCommunicationsPage — Unified communications administration hub.
 *
 * Route: /admin/communications
 * Consolidates: Email/SMTP, Channel Settings, Notification Preferences, and Calendar.
 *
 * UX-CONF-005: Created to consolidate scattered communications admin UIs.
 * UX-CONF-006: ChannelSettingsPage absorbed here as the "Channels" tab.
 *              Redirect /channel-settings → /admin/communications added in App.tsx.
 * UX-CONF-011: Original stub replaced with real component composition (2026-03-08).
 *
 * TODO-UX-CONF-014: Extract each tab into a dedicated settings tab component so
 *                   they can be composed cleanly without rendering full page wrappers.
 */
import React, { useState } from 'react';
import { Box, Tab, Tabs, Paper } from '@mui/material';
import {
  Email as EmailIcon,
  Hub as ChannelsIcon,
  Notifications as NotificationsIcon,
  CalendarMonth as CalendarIcon,
} from '@mui/icons-material';
import AdminPageHeader from '../../components/admin/AdminPageHeader';
import EmailIntegrationTab from '../../components/settings/EmailIntegrationTab';
import NotificationPreferencesPanel from '../../components/settings/NotificationPreferencesPanel';
import CalendarIntegrationTab from '../../components/settings/CalendarIntegrationTab';
// UX-CONF-006: ChannelSettingsPage embedded as the "Channels" tab.
// The standalone /channel-settings route now redirects here.
import ChannelSettingsPage from '../ChannelSettingsPage';

// ─── Tab panel helper ─────────────────────────────────────────────────────────

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel({ children, value, index }: TabPanelProps) {
  return (
    <div role="tabpanel" hidden={value !== index} id={`comm-tab-${index}`}>
      {value === index && <Box>{children}</Box>}
    </div>
  );
}

// ─── Main Page ────────────────────────────────────────────────────────────────

const AdminCommunicationsPage: React.FC = () => {
  const [tab, setTab] = useState(0);

  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="Communications"
        subtitle="Configure email servers, communication channels, notifications, and calendar integrations"
        icon={EmailIcon}
      />

      <Paper>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs
            value={tab}
            onChange={(_, v: number) => setTab(v)}
            variant="scrollable"
            scrollButtons="auto"
            sx={{ '& .MuiTab-root': { textTransform: 'none', fontWeight: 500 } }}
          >
            <Tab icon={<EmailIcon />} iconPosition="start" label="Email / SMTP" />
            {/* UX-CONF-006: Channels tab hosts ChannelSettingsPage content */}
            <Tab icon={<ChannelsIcon />} iconPosition="start" label="Channels" />
            <Tab icon={<NotificationsIcon />} iconPosition="start" label="Notifications" />
            <Tab icon={<CalendarIcon />} iconPosition="start" label="Calendar" />
          </Tabs>
        </Box>

        {/* Tab 0 — Email / SMTP */}
        <TabPanel value={tab} index={0}>
          <Box sx={{ p: 3 }}>
            <EmailIntegrationTab />
          </Box>
        </TabPanel>

        {/* Tab 1 — Channels (UX-CONF-006: previously /channel-settings standalone page) */}
        <TabPanel value={tab} index={1}>
          <ChannelSettingsPage />
        </TabPanel>

        {/* Tab 2 — Notifications */}
        <TabPanel value={tab} index={2}>
          <Box sx={{ p: 3 }}>
            <NotificationPreferencesPanel />
          </Box>
        </TabPanel>

        {/* Tab 3 — Calendar */}
        <TabPanel value={tab} index={3}>
          <Box sx={{ p: 3 }}>
            <CalendarIntegrationTab />
          </Box>
        </TabPanel>
      </Paper>
    </Box>
  );
};

export default AdminCommunicationsPage;

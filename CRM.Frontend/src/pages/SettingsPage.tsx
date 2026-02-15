import React, { useEffect, useState } from 'react';
import { Box, Typography, Grid, Card, CardContent, CardActions, Button } from '@mui/material';
import { AdminPanelSettings as AdminPanelSettingsIcon, Settings as SettingsIcon, Person as PersonIcon } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import UserSettingsDialog from '../components/UserSettingsDialog';
import LoadingSpinner from '../components/common/LoadingSpinner';
import logo from '../assets/logo.png';

function SettingsPage() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [userSettingsOpen, setUserSettingsOpen] = useState(false);

  useEffect(() => {
    setLoading(false);
  }, []);

  const isAdmin = user?.role === 'Admin' || String(user?.role) === '0';

  if (loading) {
    return <LoadingSpinner message="Loading settings..." />;
  }

  return (
    <Box sx={{ py: 2 }}>
      {/* Header */}
      <Box sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
        <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
          <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
        </Box>
        <Box>
          <Typography variant="h3" sx={{ fontWeight: 700, mb: 0.5 }}>
            Settings
          </Typography>
          <Typography color="textSecondary" variant="body2">
            Manage your personal preferences. Administration settings live under the Administration menu.
          </Typography>
        </Box>
      </Box>

      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Card sx={{ borderRadius: 3, boxShadow: 1, height: '100%' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 1 }}>
                <PersonIcon color="primary" />
                <Typography variant="h6" sx={{ fontWeight: 600 }}>
                  User Preferences
                </Typography>
              </Box>
              <Typography variant="body2" color="text.secondary">
                Update theme, language, notifications, and display options.
              </Typography>
            </CardContent>
            <CardActions sx={{ px: 2, pb: 2 }}>
              <Button variant="contained" startIcon={<SettingsIcon />} onClick={() => setUserSettingsOpen(true)}>
                Open Preferences
              </Button>
            </CardActions>
          </Card>
        </Grid>

        {isAdmin && (
          <Grid item xs={12} md={6}>
            <Card sx={{ borderRadius: 3, boxShadow: 1, height: '100%' }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 1 }}>
                  <AdminPanelSettingsIcon color="primary" />
                  <Typography variant="h6" sx={{ fontWeight: 600 }}>
                    Administration
                  </Typography>
                </Box>
                <Typography variant="body2" color="text.secondary">
                  Configure system, CRM, and workflow settings from the Administration menu.
                </Typography>
              </CardContent>
              <CardActions sx={{ px: 2, pb: 2 }}>
                <Button variant="outlined" onClick={() => navigate('/admin/monitoring')}>
                  Go to Administration
                </Button>
              </CardActions>
            </Card>
          </Grid>
        )}
      </Grid>

      <UserSettingsDialog open={userSettingsOpen} onClose={() => setUserSettingsOpen(false)} />
    </Box>
  );
}

export default SettingsPage;


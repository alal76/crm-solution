import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Button,
  TextField,
  Switch,
  FormControlLabel,
  Grid,
  Divider,
  Alert,
} from '@mui/material';
import { Save as SaveIcon } from '@mui/icons-material';
import logger from '../../services/logger';

/**
 * User Settings Panel - User profile and preferences
 */
const UserSettingsPanel: React.FC = () => {
  const [saving, setSaving] = useState(false);

  const handleSave = async () => {
    try {
      setSaving(true);
      logger.info('User settings saved');
    } catch (err) {
      logger.error('Failed to save user settings', err);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Box>
      <Card sx={{ mb: 3 }}>
        <CardHeader title="User Preferences" subtitle="Manage your personal settings" />
        <Divider />
        <CardContent>
          <Alert severity="info" sx={{ mb: 2 }}>
            These are personal user settings that apply only to your account.
          </Alert>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Theme"
                value="Light"
                disabled
                variant="outlined"
                size="small"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Language"
                value="English"
                disabled
                variant="outlined"
                size="small"
              />
            </Grid>
            <Grid item xs={12}>
              <FormControlLabel
                control={<Switch defaultChecked />}
                label="Enable email notifications"
              />
            </Grid>
            <Grid item xs={12}>
              <FormControlLabel
                control={<Switch defaultChecked />}
                label="Show completion summary on logout"
              />
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
        <Button
          variant="contained"
          startIcon={<SaveIcon />}
          onClick={handleSave}
          disabled={saving}
        >
          Save Preferences
        </Button>
      </Box>
    </Box>
  );
};

export default UserSettingsPanel;
export { UserSettingsPanel };

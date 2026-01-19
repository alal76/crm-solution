import { Box, Card, CardContent, Typography, Button } from '@mui/material';

function DatabaseSettingsTab() {
  return (
    <Box>
      <Card>
        <CardContent>
          <Typography variant="h6" sx={{ fontWeight: 600, mb: 3 }}>Database Management</Typography>
          
          <Box sx={{ mb: 3 }}>
            <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1.5 }}>Backup & Restore</Typography>
            <Box sx={{ display: 'flex', gap: 2 }}>
              <Button variant="contained" sx={{ backgroundColor: '#06A77D', textTransform: 'none' }}>
                Create Backup
              </Button>
              <Button variant="outlined" sx={{ color: '#6750A4', borderColor: '#6750A4', textTransform: 'none' }}>
                Restore Backup
              </Button>
            </Box>
          </Box>

          <Box sx={{ mb: 3 }}>
            <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1.5 }}>Database Maintenance</Typography>
            <Box sx={{ display: 'flex', gap: 2 }}>
              <Button variant="outlined" sx={{ color: '#6750A4', borderColor: '#6750A4', textTransform: 'none' }}>
                Optimize Database
              </Button>
              <Button variant="outlined" sx={{ color: '#6750A4', borderColor: '#6750A4', textTransform: 'none' }}>
                Rebuild Indexes
              </Button>
            </Box>
          </Box>

          <Box>
            <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1.5 }}>Data Management</Typography>
            <Box sx={{ display: 'flex', gap: 2 }}>
              <Button variant="outlined" sx={{ color: '#6750A4', borderColor: '#6750A4', textTransform: 'none' }}>
                Generate Seed Script
              </Button>
              <Button variant="outlined" sx={{ color: '#B3261E', borderColor: '#B3261E', textTransform: 'none' }}>
                Clear All Data
              </Button>
            </Box>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
}

export default DatabaseSettingsTab;

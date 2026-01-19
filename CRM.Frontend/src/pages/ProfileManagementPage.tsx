import { Box, Container, Typography, Card, CardContent } from '@mui/material';

function ProfileManagementPage() {
  return (
    <Box sx={{ py: 4 }}>
      <Container maxWidth="lg">
        <Typography variant="h4" sx={{ fontWeight: 700, mb: 3 }}>Profile Management</Typography>
        <Card>
          <CardContent>
            <Typography>Profile management interface</Typography>
          </CardContent>
        </Card>
      </Container>
    </Box>
  );
}

export default ProfileManagementPage;

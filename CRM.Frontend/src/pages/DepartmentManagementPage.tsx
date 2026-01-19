import { Box, Container, Typography, Card, CardContent } from '@mui/material';

function DepartmentManagementPage() {
  return (
    <Box sx={{ py: 4 }}>
      <Container maxWidth="lg">
        <Typography variant="h4" sx={{ fontWeight: 700, mb: 3 }}>Department Management</Typography>
        <Card>
          <CardContent>
            <Typography>Department management interface</Typography>
          </CardContent>
        </Card>
      </Container>
    </Box>
  );
}

export default DepartmentManagementPage;

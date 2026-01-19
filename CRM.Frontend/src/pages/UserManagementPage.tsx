import { Box, Container, Typography, Card, CardContent, Table, TableBody, TableCell, TableHead, TableRow, Button } from '@mui/material';

function UserManagementPage() {
  return (
    <Box sx={{ py: 4 }}>
      <Container maxWidth="lg">
        <Typography variant="h4" sx={{ fontWeight: 700, mb: 3 }}>User Management</Typography>
        <Card>
          <CardContent>
            <Typography variant="body1">User management interface</Typography>
          </CardContent>
        </Card>
      </Container>
    </Box>
  );
}

export default UserManagementPage;

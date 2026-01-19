import { Box, Container, Typography, Card, CardContent, Table, TableBody, TableCell, TableHead, TableRow } from '@mui/material';

function CustomersPage() {
  const customers = [
    { id: 1, name: 'Acme Corp', email: 'contact@acme.com', status: 'Active' },
    { id: 2, name: 'Tech Solutions', email: 'info@techsol.com', status: 'Active' },
    { id: 3, name: 'Global Industries', email: 'sales@global.com', status: 'Inactive' },
  ];

  return (
    <Box sx={{ py: 4 }}>
      <Container maxWidth="lg">
        <Typography variant="h4" sx={{ fontWeight: 700, mb: 3 }}>Customers</Typography>
        <Card>
          <CardContent>
            <Table>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                  <TableCell><strong>Name</strong></TableCell>
                  <TableCell><strong>Email</strong></TableCell>
                  <TableCell><strong>Status</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {customers.map((customer) => (
                  <TableRow key={customer.id}>
                    <TableCell>{customer.name}</TableCell>
                    <TableCell>{customer.email}</TableCell>
                    <TableCell>{customer.status}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </Container>
    </Box>
  );
}

export default CustomersPage;

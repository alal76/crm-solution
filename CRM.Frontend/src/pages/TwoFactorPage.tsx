import { Box, Container, Typography, Card, CardContent, TextField, Button, Alert, Table, TableBody, TableCell, TableHead, TableRow } from '@mui/material';
import { useState } from 'react';

function TwoFactorPage() {
  return (
    <Box sx={{ py: 4 }}>
      <Container maxWidth="md">
        <Typography variant="h4" sx={{ fontWeight: 700, mb: 3 }}>Two-Factor Authentication</Typography>
        <Card>
          <CardContent>
            <Typography variant="body1">Two-Factor Authentication Setup</Typography>
          </CardContent>
        </Card>
      </Container>
    </Box>
  );
}

export default TwoFactorPage;

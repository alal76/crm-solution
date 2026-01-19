import { Box, Card, CardContent, Typography, Button, Table, TableBody, TableCell, TableHead, TableRow } from '@mui/material';

function GroupManagementTab() {
  return (
    <Box>
      <Card>
        <CardContent>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h6" sx={{ fontWeight: 600 }}>Group Management</Typography>
            <Button variant="contained" sx={{ backgroundColor: '#6750A4', textTransform: 'none' }}>
              Create Group
            </Button>
          </Box>
          <Table>
            <TableHead>
              <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                <TableCell><strong>Group Name</strong></TableCell>
                <TableCell><strong>Members</strong></TableCell>
                <TableCell><strong>Actions</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              <TableRow>
                <TableCell colSpan={3} sx={{ textAlign: 'center', py: 4 }}>
                  <Typography color="textSecondary">No groups created yet</Typography>
                </TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </Box>
  );
}

export default GroupManagementTab;

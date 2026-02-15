import React, { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Tabs,
  Tab,
  Typography,
  Card,
  CardContent,
  CardHeader,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Button,
  TextField,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Grid,
  Divider,
  Alert,
  CircularProgress,
} from '@mui/material';
import { Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon } from '@mui/icons-material';
import logger from '../../services/logger';

/**
 * Admin Configuration Page - Sales and Service Desk settings
 */
const AdminConfigurationPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('sales');
  const [loading, setLoading] = useState(false);
  const [openDialog, setOpenDialog] = useState(false);
  const [dialogType, setDialogType] = useState<'commission' | 'discount' | 'sla' | 'escalation'>('commission');

  return (
    <Container maxWidth="lg" sx={{ py: 3 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h3" sx={{ fontWeight: 700, mb: 1 }}>
          Admin Configuration
        </Typography>
        <Typography color="text.secondary">
          Manage sales commissions, discounts, SLA policies, and escalation rules
        </Typography>
      </Box>

      <Box sx={{ borderBottom: 'solid 1px', borderColor: 'divider', mb: 3 }}>
        <Tabs
          value={activeTab}
          onChange={(e, v) => setActiveTab(v)}
          aria-label="Admin configuration tabs"
        >
          <Tab label="Sales Configuration" value="sales" />
          <Tab label="Service Desk Configuration" value="service-desk" />
        </Tabs>
      </Box>

      {/* Sales Configuration */}
      {activeTab === 'sales' && (
        <Box>
          {/* Commission Rules */}
          <Card sx={{ mb: 3 }}>
            <CardHeader
              title="Commission Rules"
              subtitle="Configure sales commission structure"
              action={
                <Button
                  variant="contained"
                  startIcon={<AddIcon />}
                  onClick={() => {
                    setDialogType('commission');
                    setOpenDialog(true);
                  }}
                >
                  Add Rule
                </Button>
              }
            />
            <Divider />
            <CardContent>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow sx={{ bgcolor: 'grey.100' }}>
                      <TableCell>Name</TableCell>
                      <TableCell>Type</TableCell>
                      <TableCell>Base Rate</TableCell>
                      <TableCell>Min Amount</TableCell>
                      <TableCell>Max Amount</TableCell>
                      <TableCell>Status</TableCell>
                      <TableCell align="center">Actions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    <TableRow>
                      <TableCell>Standard Commission</TableCell>
                      <TableCell>Percentage</TableCell>
                      <TableCell>5%</TableCell>
                      <TableCell>$0</TableCell>
                      <TableCell>Unlimited</TableCell>
                      <TableCell>
                        <Box sx={{ display: 'inline-block', px: 1, py: 0.5, bgcolor: 'success.light', color: 'success.dark', borderRadius: 1, fontSize: '0.75rem', fontWeight: 600 }}>
                          Active
                        </Box>
                      </TableCell>
                      <TableCell align="center">
                        <Button size="small" startIcon={<EditIcon />} />
                        <Button size="small" startIcon={<DeleteIcon />} color="error" />
                      </TableCell>
                    </TableRow>
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>

          {/* Discount Rules */}
          <Card sx={{ mb: 3 }}>
            <CardHeader
              title="Discount Rules"
              subtitle="Configure discount structures and promotional discounts"
              action={
                <Button
                  variant="contained"
                  startIcon={<AddIcon />}
                  onClick={() => {
                    setDialogType('discount');
                    setOpenDialog(true);
                  }}
                >
                  Add Rule
                </Button>
              }
            />
            <Divider />
            <CardContent>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow sx={{ bgcolor: 'grey.100' }}>
                      <TableCell>Name</TableCell>
                      <TableCell>Type</TableCell>
                      <TableCell>Value</TableCell>
                      <TableCell>Code</TableCell>
                      <TableCell>Valid From</TableCell>
                      <TableCell>Valid Until</TableCell>
                      <TableCell>Status</TableCell>
                      <TableCell align="center">Actions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    <TableRow>
                      <TableCell>Summer Sale</TableCell>
                      <TableCell>Percentage</TableCell>
                      <TableCell>10%</TableCell>
                      <TableCell>SUMMER2024</TableCell>
                      <TableCell>06/01/2024</TableCell>
                      <TableCell>08/31/2024</TableCell>
                      <TableCell>
                        <Box sx={{ display: 'inline-block', px: 1, py: 0.5, bgcolor: 'success.light', color: 'success.dark', borderRadius: 1, fontSize: '0.75rem', fontWeight: 600 }}>
                          Active
                        </Box>
                      </TableCell>
                      <TableCell align="center">
                        <Button size="small" startIcon={<EditIcon />} />
                        <Button size="small" startIcon={<DeleteIcon />} color="error" />
                      </TableCell>
                    </TableRow>
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </Box>
      )}

      {/* Service Desk Configuration */}
      {activeTab === 'service-desk' && (
        <Box>
          {/* SLA Policies */}
          <Card sx={{ mb: 3 }}>
            <CardHeader
              title="SLA Policies"
              subtitle="Define service level agreements and response times"
              action={
                <Button
                  variant="contained"
                  startIcon={<AddIcon />}
                  onClick={() => {
                    setDialogType('sla');
                    setOpenDialog(true);
                  }}
                >
                  Add Policy
                </Button>
              }
            />
            <Divider />
            <CardContent>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow sx={{ bgcolor: 'grey.100' }}>
                      <TableCell>Name</TableCell>
                      <TableCell>Priority</TableCell>
                      <TableCell>Response Time</TableCell>
                      <TableCell>Resolution Time</TableCell>
                      <TableCell>Working Hours</TableCell>
                      <TableCell>Status</TableCell>
                      <TableCell align="center">Actions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    <TableRow>
                      <TableCell>Critical Support</TableCell>
                      <TableCell>
                        <Box sx={{ display: 'inline-block', px: 1, py: 0.5, bgcolor: 'error.light', color: 'error.dark', borderRadius: 1, fontSize: '0.75rem', fontWeight: 600 }}>
                          Critical
                        </Box>
                      </TableCell>
                      <TableCell>15 minutes</TableCell>
                      <TableCell>4 hours</TableCell>
                      <TableCell>24/7</TableCell>
                      <TableCell>
                        <Box sx={{ display: 'inline-block', px: 1, py: 0.5, bgcolor: 'success.light', color: 'success.dark', borderRadius: 1, fontSize: '0.75rem', fontWeight: 600 }}>
                          Active
                        </Box>
                      </TableCell>
                      <TableCell align="center">
                        <Button size="small" startIcon={<EditIcon />} />
                        <Button size="small" startIcon={<DeleteIcon />} color="error" />
                      </TableCell>
                    </TableRow>
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>

          {/* Escalation Rules */}
          <Card sx={{ mb: 3 }}>
            <CardHeader
              title="Escalation Rules"
              subtitle="Configure automatic escalation triggers"
              action={
                <Button
                  variant="contained"
                  startIcon={<AddIcon />}
                  onClick={() => {
                    setDialogType('escalation');
                    setOpenDialog(true);
                  }}
                >
                  Add Rule
                </Button>
              }
            />
            <Divider />
            <CardContent>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow sx={{ bgcolor: 'grey.100' }}>
                      <TableCell>Name</TableCell>
                      <TableCell>Condition</TableCell>
                      <TableCell>Metric</TableCell>
                      <TableCell>Escalate To</TableCell>
                      <TableCell>Notify</TableCell>
                      <TableCell>Status</TableCell>
                      <TableCell align="center">Actions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    <TableRow>
                      <TableCell>Age-Based Escalation</TableCell>
                      <TableCell>&gt; 2 hours old</TableCell>
                      <TableCell>AgeMinutes</TableCell>
                      <TableCell>Manager Group</TableCell>
                      <TableCell>Yes</TableCell>
                      <TableCell>
                        <Box sx={{ display: 'inline-block', px: 1, py: 0.5, bgcolor: 'success.light', color: 'success.dark', borderRadius: 1, fontSize: '0.75rem', fontWeight: 600 }}>
                          Active
                        </Box>
                      </TableCell>
                      <TableCell align="center">
                        <Button size="small" startIcon={<EditIcon />} />
                        <Button size="small" startIcon={<DeleteIcon />} color="error" />
                      </TableCell>
                    </TableRow>
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </Box>
      )}

      {/* Dialog for adding/editing rules */}
      <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          {dialogType === 'commission' && 'Add Commission Rule'}
          {dialogType === 'discount' && 'Add Discount Rule'}
          {dialogType === 'sla' && 'Add SLA Policy'}
          {dialogType === 'escalation' && 'Add Escalation Rule'}
        </DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <TextField
            fullWidth
            label="Name"
            variant="outlined"
            size="small"
            sx={{ mb: 2 }}
          />
          <TextField
            fullWidth
            label="Description"
            variant="outlined"
            size="small"
            multiline
            rows={3}
            sx={{ mb: 2 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDialog(false)}>Cancel</Button>
          <Button variant="contained" onClick={() => setOpenDialog(false)}>
            Create
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default AdminConfigurationPage;

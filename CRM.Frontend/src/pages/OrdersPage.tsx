import React, { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Typography,
  Button,
  Card,
  CardContent,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  Chip,
  IconButton,
  Tooltip,
  Dialog,
  DialogContent,
  DialogActions,
  TextField,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Stack,
  Alert,
  CircularProgress,
  Tabs,
  Tab,
} from '@mui/material';
import type { SelectChangeEvent } from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Refresh as RefreshIcon,
  CheckCircle as CheckCircleIcon,
  Cancel as CancelIcon,
  LocalShipping as ShippingIcon,
  Warning as WarningIcon,
} from '@mui/icons-material';
import { DialogError } from '../components/common/DialogError';
import ActionButton from '../components/common/ActionButton';
import { DialogHeader } from '../components/common/DialogHeader';
import TabPanel from '../components/common/TabPanel';
import { EnhancedEmptyState } from '../components/common/EnhancedEmptyState';
import { EntitySelect } from '../components/common/EntitySelect';
import { useApiState } from '../hooks/useApiState';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';

// ==================== ENUMS ====================

enum OrderStatus {
  Draft = 0, Submitted = 1, PendingApproval = 2, Approved = 3, Rejected = 4,
  Processing = 5, PartiallyFulfilled = 6, Fulfilled = 7, Shipped = 8,
  Delivered = 9, Cancelled = 10, OnHold = 11, ActionRequired = 12,
}

// ==================== INTERFACES ====================

interface Order {
  id: number;
  orderNumber: string;
  customerId: number;
  customerName?: string;
  opportunityId?: number;
  quoteId?: number;
  status: OrderStatus;
  orderDate: string;
  requestedDate?: string;
  subtotal: number;
  taxAmount: number;
  discountAmount: number;
  totalAmount: number;
  shippingAddress?: string;
  billingAddress?: string;
  notes?: string;
  createdAt: string;
  updatedAt?: string;
}

interface OrderLineItem {
  id: number;
  orderId: number;
  productId?: number;
  productName?: string;
  description: string;
  quantity: number;
  unitPrice: number;
  discount: number;
  totalPrice: number;
  fulfilledQuantity: number;
}

interface OrderForm {
  customerId: number | null;
  status: OrderStatus;
  orderDate: string;
  requestedDate: string;
  shippingAddress: string;
  billingAddress: string;
  notes: string;
  opportunityId: number | null;
  quoteId: number | null;
}

// ==================== CONSTANTS ====================

type ChipColor = 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning';

const ORDER_STATUS_OPTIONS: Array<{ value: OrderStatus; label: string; color: ChipColor }> = [
  { value: OrderStatus.Draft, label: 'Draft', color: 'default' },
  { value: OrderStatus.Submitted, label: 'Submitted', color: 'info' },
  { value: OrderStatus.PendingApproval, label: 'Pending Approval', color: 'warning' },
  { value: OrderStatus.Approved, label: 'Approved', color: 'info' },
  { value: OrderStatus.Rejected, label: 'Rejected', color: 'error' },
  { value: OrderStatus.Processing, label: 'Processing', color: 'primary' },
  { value: OrderStatus.PartiallyFulfilled, label: 'Partially Fulfilled', color: 'warning' },
  { value: OrderStatus.Fulfilled, label: 'Fulfilled', color: 'success' },
  { value: OrderStatus.Shipped, label: 'Shipped', color: 'info' },
  { value: OrderStatus.Delivered, label: 'Delivered', color: 'success' },
  { value: OrderStatus.Cancelled, label: 'Cancelled', color: 'default' },
  { value: OrderStatus.OnHold, label: 'On Hold', color: 'warning' },
  { value: OrderStatus.ActionRequired, label: 'Action Required', color: 'error' },
];

// ==================== HELPER FUNCTIONS ====================

const getStatusInfo = (status: OrderStatus): { label: string; color: ChipColor } =>
  ORDER_STATUS_OPTIONS.find(s => s.value === status) || { label: 'Unknown', color: 'default' };

const formatCurrency = (value: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);

const formatDate = (dateString: string) =>
  dateString ? new Date(dateString).toLocaleDateString() : '-';

// ==================== MAIN COMPONENT ====================

function OrdersPage() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  const [filterStatus, setFilterStatus] = useState<OrderStatus | 'all'>('all');
  const [lineItems, setLineItems] = useState<OrderLineItem[]>([]);

  const emptyForm: OrderForm = {
    customerId: null,
    status: OrderStatus.Draft,
    orderDate: new Date().toISOString().split('T')[0],
    requestedDate: '',
    shippingAddress: '',
    billingAddress: '',
    notes: '',
    opportunityId: null,
    quoteId: null,
  };
  const [formData, setFormData] = useState<OrderForm>(emptyForm);

  const dialogApi = useApiState();

  // ==================== DATA FETCHING ====================

  useEffect(() => {
    fetchOrders();
  }, []);

  const fetchOrders = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/orders');
      setOrders(response.data);
      setError(null);
    } catch (err: any) {
      if (err.response?.status === 404) {
        setOrders([]);
        setError(null);
      } else {
        setError(err.response?.data?.message || 'Failed to fetch orders');
      }
    } finally {
      setLoading(false);
    }
  };

  const fetchLineItems = async (orderId: number) => {
    try {
      const response = await apiClient.get(`/orders/${orderId}/line-items`);
      setLineItems(response.data);
    } catch {
      setLineItems([]);
    }
  };

  // ==================== DIALOG HANDLERS ====================

  const handleOpenDialog = (order?: Order) => {
    setDialogTab(0);
    if (order) {
      setEditingId(order.id);
      setFormData({
        customerId: order.customerId,
        status: order.status,
        orderDate: order.orderDate?.split('T')[0] || '',
        requestedDate: order.requestedDate?.split('T')[0] || '',
        shippingAddress: order.shippingAddress || '',
        billingAddress: order.billingAddress || '',
        notes: order.notes || '',
        opportunityId: order.opportunityId || null,
        quoteId: order.quoteId || null,
      });
      fetchLineItems(order.id);
    } else {
      setEditingId(null);
      setFormData(emptyForm);
      setLineItems([]);
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
    dialogApi.clearError();
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'number' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleSelectChange = (e: SelectChangeEvent<number | string>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name as string]: value }));
  };

  // ==================== SAVE OPERATIONS ====================

  const handleSaveOrder = async () => {
    if (!formData.customerId) {
      dialogApi.setError('Account is required');
      return;
    }

    await dialogApi.execute(async () => {
      if (editingId) {
        await apiClient.put(`/orders/${editingId}`, formData);
        setSuccessMessage('Order updated successfully');
      } else {
        await apiClient.post('/orders', formData);
        setSuccessMessage('Order created successfully');
      }
      handleCloseDialog();
      fetchOrders();
      setTimeout(() => setSuccessMessage(null), 3000);
    });
  };

  const handleDeleteOrder = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this order?')) {
      try {
        await apiClient.delete(`/orders/${id}`);
        setSuccessMessage('Order deleted successfully');
        fetchOrders();
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete order');
      }
    }
  };

  // ==================== ORDER ACTIONS ====================

  const handleApproveOrder = async (id: number) => {
    try {
      await apiClient.post(`/orders/${id}/approve`);
      setSuccessMessage('Order approved');
      fetchOrders();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to approve order');
    }
  };

  const handleCancelOrder = async (id: number) => {
    const reason = window.prompt('Enter cancellation reason:');
    if (!reason) return;

    try {
      await apiClient.post(`/orders/${id}/cancel`, { reason });
      setSuccessMessage('Order cancelled');
      fetchOrders();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to cancel order');
    }
  };

  const handleFulfillOrder = async (id: number) => {
    try {
      await apiClient.post(`/orders/${id}/fulfill`);
      setSuccessMessage('Order marked as fulfilled');
      fetchOrders();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fulfill order');
    }
  };

  // ==================== FILTERING ====================

  const filteredOrders = filterStatus === 'all'
    ? orders
    : orders.filter(o => o.status === filterStatus);

  // ==================== RENDER ====================

  if (loading) {
    return (
      <Container maxWidth="lg">
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="50vh">
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg">
      <Box mb={4}>
        {/* Header */}
        <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
          <Box display="flex" alignItems="center" gap={2}>
            <img src={logo} alt="CRM Logo" style={{ height: 40, borderRadius: 8 }} />
            <Typography variant="h4">Orders</Typography>
          </Box>
          <Stack direction="row" spacing={2}>
            <FormControl size="small" sx={{ minWidth: 150 }}>
              <InputLabel>Status Filter</InputLabel>
              <Select
                value={filterStatus}
                onChange={(e) => setFilterStatus(e.target.value as OrderStatus | 'all')}
                label="Status Filter"
              >
                <MenuItem value="all">All</MenuItem>
                {ORDER_STATUS_OPTIONS.map(opt => (
                  <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                ))}
              </Select>
            </FormControl>
            <Button variant="outlined" startIcon={<RefreshIcon />} onClick={fetchOrders}>
              Refresh
            </Button>
            <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenDialog()}>
              New Order
            </Button>
          </Stack>
        </Box>

        {/* Alerts */}
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

        {/* Action Required Alert */}
        {orders.filter(o => o.status === OrderStatus.ActionRequired || o.status === OrderStatus.PendingApproval).length > 0 && (
          <Alert severity="warning" icon={<WarningIcon />} sx={{ mb: 2 }}>
            {orders.filter(o => o.status === OrderStatus.ActionRequired || o.status === OrderStatus.PendingApproval).length} order(s) require attention
          </Alert>
        )}

        {/* Orders Table */}
        <Card>
          <CardContent>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Order #</TableCell>
                  <TableCell>Account</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Order Date</TableCell>
                  <TableCell>Requested Date</TableCell>
                  <TableCell align="right">Total</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredOrders.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={7} sx={{ border: 0 }}>
                      <EnhancedEmptyState
                        illustration="generic"
                        title="No orders yet"
                        description="Create your first order to start tracking sales"
                        variant="no-data"
                        primaryActionLabel="Create Order"
                        onPrimaryAction={() => handleOpenDialog()}
                      />
                    </TableCell>
                  </TableRow>
                ) : (
                  filteredOrders.map((order) => {
                    const statusInfo = getStatusInfo(order.status);

                    return (
                      <TableRow key={order.id} hover>
                        <TableCell>
                          <Typography fontFamily="monospace">{order.orderNumber}</Typography>
                        </TableCell>
                        <TableCell>{order.customerName || '-'}</TableCell>
                        <TableCell>
                          <Chip label={statusInfo.label} size="small" color={statusInfo.color} />
                        </TableCell>
                        <TableCell>{formatDate(order.orderDate)}</TableCell>
                        <TableCell>{formatDate(order.requestedDate || '')}</TableCell>
                        <TableCell align="right">
                          <Typography fontWeight="medium">{formatCurrency(order.totalAmount)}</Typography>
                        </TableCell>
                        <TableCell align="right">
                          <Tooltip title="Edit">
                            <IconButton size="small" onClick={() => handleOpenDialog(order)}>
                              <EditIcon />
                            </IconButton>
                          </Tooltip>
                          {order.status === OrderStatus.PendingApproval && (
                            <Tooltip title="Approve">
                              <IconButton size="small" color="success" onClick={() => handleApproveOrder(order.id)}>
                                <CheckCircleIcon />
                              </IconButton>
                            </Tooltip>
                          )}
                          {(order.status === OrderStatus.Approved || order.status === OrderStatus.Processing) && (
                            <Tooltip title="Mark Fulfilled">
                              <IconButton size="small" color="primary" onClick={() => handleFulfillOrder(order.id)}>
                                <ShippingIcon />
                              </IconButton>
                            </Tooltip>
                          )}
                          {order.status !== OrderStatus.Cancelled && order.status !== OrderStatus.Delivered && (
                            <Tooltip title="Cancel">
                              <IconButton size="small" color="warning" onClick={() => handleCancelOrder(order.id)}>
                                <CancelIcon />
                              </IconButton>
                            </Tooltip>
                          )}
                          <Tooltip title="Delete">
                            <IconButton size="small" color="error" onClick={() => handleDeleteOrder(order.id)}>
                              <DeleteIcon />
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    );
                  })
                )}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </Box>

      {/* Order Editor Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="lg" fullWidth>
        <DialogHeader
          mode={editingId ? 'edit' : 'create'}
          entityType="order"
          entityName={editingId ? `Order` : undefined}
          entityId={editingId || undefined}
          onClose={handleCloseDialog}
          status={editingId ? getStatusInfo(formData.status).label : undefined}
          statusColor={editingId ? (
            formData.status === OrderStatus.Delivered ? 'success' :
            formData.status === OrderStatus.Fulfilled ? 'success' :
            formData.status === OrderStatus.Cancelled ? 'default' :
            formData.status === OrderStatus.Rejected ? 'error' :
            formData.status === OrderStatus.PendingApproval ? 'warning' : 'info'
          ) : undefined}
        />
        <DialogContent dividers>
          <Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)} sx={{ mb: 2 }}>
            <Tab label="Order Details" />
            <Tab label="Addresses" />
            {editingId && <Tab label="Line Items" />}
          </Tabs>

          <DialogError error={dialogApi.error} />

          {/* Tab 0: Order Details */}
          <TabPanel value={dialogTab} index={0}>
            <Grid container spacing={3}>
              <Grid item xs={12} md={6}>
                <EntitySelect
                  name="customerId"
                  label="Account *"
                  entityType="account"
                  value={formData.customerId ?? ''}
                  onChange={(val) => setFormData(prev => ({ ...prev, customerId: val as number | null }))}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth>
                  <InputLabel>Status</InputLabel>
                  <Select
                    name="status"
                    value={formData.status}
                    onChange={handleSelectChange}
                    label="Status"
                  >
                    {ORDER_STATUS_OPTIONS.map(opt => (
                      <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  type="date"
                  label="Order Date"
                  name="orderDate"
                  value={formData.orderDate}
                  onChange={handleInputChange}
                  InputLabelProps={{ shrink: true }}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  type="date"
                  label="Requested Date"
                  name="requestedDate"
                  value={formData.requestedDate}
                  onChange={handleInputChange}
                  InputLabelProps={{ shrink: true }}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <EntitySelect
                  name="opportunityId"
                  label="Related Opportunity"
                  entityType="opportunity"
                  value={formData.opportunityId ?? ''}
                  onChange={(val) => setFormData(prev => ({ ...prev, opportunityId: val as number | null }))}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  type="number"
                  label="Related Quote ID"
                  value={formData.quoteId || ''}
                  onChange={(e) => setFormData(prev => ({ ...prev, quoteId: e.target.value ? parseInt(e.target.value) : null }))}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  multiline
                  rows={3}
                  label="Notes"
                  name="notes"
                  value={formData.notes}
                  onChange={handleInputChange}
                />
              </Grid>
            </Grid>
          </TabPanel>

          {/* Tab 1: Addresses */}
          <TabPanel value={dialogTab} index={1}>
            <Grid container spacing={3}>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  multiline
                  rows={3}
                  label="Shipping Address"
                  name="shippingAddress"
                  value={formData.shippingAddress}
                  onChange={handleInputChange}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  multiline
                  rows={3}
                  label="Billing Address"
                  name="billingAddress"
                  value={formData.billingAddress}
                  onChange={handleInputChange}
                />
              </Grid>
            </Grid>
          </TabPanel>

          {/* Tab 2: Line Items (read-only when editing) */}
          {editingId && (
            <TabPanel value={dialogTab} index={2}>
              {lineItems.length === 0 ? (
                <Typography color="text.secondary" textAlign="center" py={4}>
                  No line items yet. Add line items through the API.
                </Typography>
              ) : (
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Product</TableCell>
                      <TableCell>Description</TableCell>
                      <TableCell align="right">Qty</TableCell>
                      <TableCell align="right">Unit Price</TableCell>
                      <TableCell align="right">Fulfilled</TableCell>
                      <TableCell align="right">Total</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {lineItems.map(item => (
                      <TableRow key={item.id}>
                        <TableCell>{item.productName || '-'}</TableCell>
                        <TableCell>{item.description}</TableCell>
                        <TableCell align="right">{item.quantity}</TableCell>
                        <TableCell align="right">{formatCurrency(item.unitPrice)}</TableCell>
                        <TableCell align="right">
                          <Chip
                            size="small"
                            label={`${item.fulfilledQuantity}/${item.quantity}`}
                            color={item.fulfilledQuantity >= item.quantity ? 'success' : item.fulfilledQuantity > 0 ? 'warning' : 'default'}
                          />
                        </TableCell>
                        <TableCell align="right">{formatCurrency(item.totalPrice)}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
            </TabPanel>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <ActionButton
            onClick={handleSaveOrder}
            loading={dialogApi.loading}
            variant="contained"
          >
            {editingId ? 'Update Order' : 'Create Order'}
          </ActionButton>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

export default OrdersPage;

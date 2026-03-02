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
  TablePagination,
  Chip,
  IconButton,
  Tooltip,
  Dialog,
  DialogContent,
  DialogActions,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Stack,
  Alert,
  CircularProgress,
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
  AssignmentReturn as ReturnIcon,
} from '@mui/icons-material';
import OrderReturnDialog from '../components/sales/OrderReturnDialog';
import { DialogError } from '../components/common/DialogError';
import ActionButton from '../components/common/ActionButton';
import { DialogHeader } from '../components/common/DialogHeader';
import DynamicEntityForm, { ExtraTab } from '../components/DynamicEntityForm';
import { EnhancedEmptyState } from '../components/common/EnhancedEmptyState';
import { useApiState } from '../hooks/useApiState';
import { usePagination } from '../hooks/usePagination';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';

// ==================== ENUMS ====================

// Numeric values match backend CRM.Core.Entities.OrderStatus
enum OrderStatus {
  Draft = 0,
  PendingApproval = 1,
  Approved = 2,
  Processing = 3,
  PartiallyFulfilled = 4,
  Fulfilled = 5,
  Delivered = 6,
  Completed = 7,
  Cancelled = 8,
  Returned = 9,
  Refunded = 10,
  OnHold = 11,
  ActionRequired = 12,
}

// ==================== INTERFACES ====================

interface Order {
  id: number;
  orderNumber: string;
  accountId: number;   // primary account reference (API uses accountId)
  accountName?: string;
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
  accountId: number | null;
  status: OrderStatus;
  orderDate: string;
  requestedDate: string;
  shippingAddress: string;
  billingAddress: string;
  notes: string;
  opportunityId: number | null;
  quoteId: number | null;
  // Shipping & Fulfillment
  shippingName: string;
  shippingCity: string;
  shippingState: string;
  shippingZipCode: string;
  shippingCountry: string;
  shippingMethod: string;
  shippingCarrier: string;
  trackingNumber: string;
  trackingUrl: string;
  shippedDate: string;
  deliveredDate: string;
  estimatedDeliveryDate: string;
  // Billing
  billingName: string;
  billingCity: string;
  billingState: string;
  billingZipCode: string;
  billingCountry: string;
  // Payment
  paymentMethod: string;
  paymentTerms: string;
  paymentReference: string;
  paymentDate: string;
  // Revenue Recognition
  revenueRecognitionMethod: string;
  revenueStartDate: string;
  revenueEndDate: string;
}

// ==================== CONSTANTS ====================

type ChipColor = 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning';

const ORDER_STATUS_OPTIONS: Array<{ value: OrderStatus; label: string; color: ChipColor }> = [
  { value: OrderStatus.Draft, label: 'Draft', color: 'default' },
  { value: OrderStatus.PendingApproval, label: 'Pending Approval', color: 'warning' },
  { value: OrderStatus.Approved, label: 'Approved', color: 'info' },
  { value: OrderStatus.Processing, label: 'Processing', color: 'primary' },
  { value: OrderStatus.PartiallyFulfilled, label: 'Partially Fulfilled', color: 'warning' },
  { value: OrderStatus.Fulfilled, label: 'Fulfilled', color: 'success' },
  { value: OrderStatus.Delivered, label: 'Delivered', color: 'success' },
  { value: OrderStatus.Completed, label: 'Completed', color: 'success' },
  { value: OrderStatus.Cancelled, label: 'Cancelled', color: 'default' },
  { value: OrderStatus.Returned, label: 'Returned', color: 'error' },
  { value: OrderStatus.Refunded, label: 'Refunded', color: 'secondary' },
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
  const [returnDialogOpen, setReturnDialogOpen] = useState(false);
  const [returnOrderId, setReturnOrderId] = useState<number | null>(null);

  const emptyForm: OrderForm = {
    accountId: null,
    status: OrderStatus.Draft,
    orderDate: new Date().toISOString().split('T')[0],
    requestedDate: '',
    shippingAddress: '',
    billingAddress: '',
    notes: '',
    opportunityId: null,
    quoteId: null,
    // Shipping & Fulfillment
    shippingName: '',
    shippingCity: '',
    shippingState: '',
    shippingZipCode: '',
    shippingCountry: '',
    shippingMethod: '',
    shippingCarrier: '',
    trackingNumber: '',
    trackingUrl: '',
    shippedDate: '',
    deliveredDate: '',
    estimatedDeliveryDate: '',
    // Billing
    billingName: '',
    billingCity: '',
    billingState: '',
    billingZipCode: '',
    billingCountry: '',
    // Payment
    paymentMethod: '',
    paymentTerms: '',
    paymentReference: '',
    paymentDate: '',
    // Revenue Recognition
    revenueRecognitionMethod: '',
    revenueStartDate: '',
    revenueEndDate: '',
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
        accountId: order.accountId,
        status: order.status,
        orderDate: order.orderDate?.split('T')[0] || '',
        requestedDate: order.requestedDate?.split('T')[0] || '',
        shippingAddress: order.shippingAddress || '',
        billingAddress: order.billingAddress || '',
        notes: order.notes || '',
        opportunityId: order.opportunityId || null,
        quoteId: order.quoteId || null,
        // Shipping & Fulfillment
        shippingName: '',
        shippingCity: '',
        shippingState: '',
        shippingZipCode: '',
        shippingCountry: '',
        shippingMethod: '',
        shippingCarrier: '',
        trackingNumber: '',
        trackingUrl: '',
        shippedDate: '',
        deliveredDate: '',
        estimatedDeliveryDate: '',
        // Billing
        billingName: '',
        billingCity: '',
        billingState: '',
        billingZipCode: '',
        billingCountry: '',
        // Payment
        paymentMethod: '',
        paymentTerms: '',
        paymentReference: '',
        paymentDate: '',
        // Revenue Recognition
        revenueRecognitionMethod: '',
        revenueStartDate: '',
        revenueEndDate: '',
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
      [name]: type === 'number' ? Number.parseFloat(value) || 0 : value,
    }));
  };

  const handleSelectChange = (e: SelectChangeEvent<number | string>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name as string]: value }));
  };

  // ==================== SAVE OPERATIONS ====================

  const handleSaveOrder = async () => {
    if (!formData.accountId) {
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

  const { paginatedData: paginatedOrders, page, pageSize, handlePageChange, handlePageSizeChange, pageSizeOptions } = usePagination(filteredOrders, { defaultPageSize: 25 });

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
                  paginatedOrders.map((order) => {
                    const statusInfo = getStatusInfo(order.status);

                    return (
                      <TableRow key={order.id} hover>
                        <TableCell>
                          <Typography fontFamily="monospace">{order.orderNumber}</Typography>
                        </TableCell>
                        <TableCell>{order.accountName || '-'}</TableCell>
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
                          {(order.status === OrderStatus.Fulfilled ||
                            order.status === OrderStatus.Delivered ||
                            order.status === OrderStatus.Completed) && (
                            <Tooltip title="Return Items">
                              <IconButton
                                size="small"
                                color="secondary"
                                onClick={() => {
                                  setReturnOrderId(order.id);
                                  setReturnDialogOpen(true);
                                }}
                              >
                                <ReturnIcon />
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
            <TablePagination
              component="div"
              count={filteredOrders.length}
              page={page}
              onPageChange={handlePageChange}
              rowsPerPage={pageSize}
              onRowsPerPageChange={handlePageSizeChange}
              rowsPerPageOptions={pageSizeOptions}
            />
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
            formData.status === OrderStatus.ActionRequired ? 'error' :
            formData.status === OrderStatus.PendingApproval ? 'warning' : 'info'
          ) : undefined}
        />
        <DialogContent dividers>
          <DialogError error={dialogApi.error} />

          <DynamicEntityForm
            moduleName="Orders"
            formData={formData}
            onChange={handleInputChange}
            onSelectChange={(e: any) => setFormData((prev: any) => ({ ...prev, [e.target.name]: e.target.value }))}
            setFormData={setFormData}
            activeTab={dialogTab}
            editingId={editingId}
            onTabChange={setDialogTab}
            excludeFields={['tags', 'customFields']}
            extraTabs={[
              {
                index: 100,
                name: 'Line Items',
                icon: <ShippingIcon fontSize="small" />,
                editOnly: true,
                render: () => (
                  lineItems.length === 0 ? (
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
                  )
                ),
              },
            ]}
          />
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

      {/* Order Return Dialog */}
      {returnOrderId !== null && (
        <OrderReturnDialog
          open={returnDialogOpen}
          onClose={() => {
            setReturnDialogOpen(false);
            setReturnOrderId(null);
          }}
          orderId={returnOrderId}
          orderNumber={
            orders.find(o => o.id === returnOrderId)?.orderNumber
          }
          lineItems={lineItems}
          onSuccess={(msg) => {
            setSuccessMessage(msg);
            setReturnDialogOpen(false);
            setReturnOrderId(null);
            fetchOrders();
          }}
        />
      )}
    </Container>
  );
}

export default OrdersPage;

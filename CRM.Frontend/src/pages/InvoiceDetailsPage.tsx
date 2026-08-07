/**
 * InvoiceDetailsPage
 * Displays detailed view of an invoice with line items, payment history, and actions
 * Priority: P1
 * 
 * Implementation based on SPEC-SALES-003
 */

import React, { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  CircularProgress,
  Alert,
  Chip,
  Grid,
  Paper,
  Divider,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  IconButton,
  Tooltip,
} from '@mui/material';
import {
  ArrowBack as ArrowBackIcon,
  Download as DownloadIcon,
  Send as SendIcon,
  Edit as EditIcon,
  Payment as PaymentIcon,
  Block as BlockIcon,
  Print as PrintIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import { useParams, useNavigate } from 'react-router-dom';
import invoiceService, { 
  Invoice, 
  InvoiceLineItem, 
  InvoiceStatus, 
  PaymentMethod,
  getInvoiceStatusLabel,
  getInvoiceStatusColor,
} from '../services/invoiceService';

/**
 * Invoice Details Page Component
 */
export const InvoiceDetailsPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const [invoice, setInvoice] = useState<Invoice | null>(null);
  const [lineItems, setLineItems] = useState<InvoiceLineItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [paymentDialogOpen, setPaymentDialogOpen] = useState(false);
  const [voidDialogOpen, setVoidDialogOpen] = useState(false);
  const [sendDialogOpen, setSendDialogOpen] = useState(false);
  const [paymentAmount, setPaymentAmount] = useState<number>(0);
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>(PaymentMethod.CreditCard);
  const [voidReason, setVoidReason] = useState('');
  const [recipientEmail, setRecipientEmail] = useState('');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    const loadInvoice = async () => {
      if (!id) return;
      
      try {
        setLoading(true);
        const response = await invoiceService.getById(Number.parseInt(id, 10));
        setInvoice(response.data);
        
        // Load line items
        try {
          const lineItemsResponse = await invoiceService.getLineItems(Number.parseInt(id, 10));
          setLineItems(lineItemsResponse.data || []);
        } catch (err) {
          console.error('Error loading line items:', err);
        }
        
        setError(null);
      } catch (err) {
        setError('Failed to load invoice');
        console.error('Error loading invoice:', err);
      } finally {
        setLoading(false);
      }
    };

    loadInvoice();
  }, [id]);

  const handleRecordPayment = async () => {
    if (!invoice) return;
    
    try {
      setSubmitting(true);
      await invoiceService.recordPayment(invoice.id, paymentAmount, paymentMethod);
      setPaymentDialogOpen(false);
      // Reload invoice
      const response = await invoiceService.getById(invoice.id);
      setInvoice(response.data);
      setError(null);
    } catch (err) {
      setError('Failed to record payment');
      console.error('Error recording payment:', err);
    } finally {
      setSubmitting(false);
    }
  };

  const handleVoidInvoice = async () => {
    if (!invoice) return;
    
    try {
      setSubmitting(true);
      await invoiceService.void(invoice.id, voidReason);
      setVoidDialogOpen(false);
      // Reload invoice
      const response = await invoiceService.getById(invoice.id);
      setInvoice(response.data);
      setError(null);
    } catch (err) {
      setError('Failed to void invoice');
      console.error('Error voiding invoice:', err);
    } finally {
      setSubmitting(false);
    }
  };

  const handleSendInvoice = async () => {
    if (!invoice) return;
    
    try {
      setSubmitting(true);
      await invoiceService.send(invoice.id, recipientEmail);
      setSendDialogOpen(false);
      // Reload invoice
      const response = await invoiceService.getById(invoice.id);
      setInvoice(response.data);
      setError(null);
    } catch (err) {
      setError('Failed to send invoice');
      console.error('Error sending invoice:', err);
    } finally {
      setSubmitting(false);
    }
  };

  const handleDownloadPDF = async () => {
    if (!invoice) return;

    try {
      const response = await invoiceService.generatePdf(invoice.id);
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `Invoice_${invoice.invoiceNumber}.pdf`);
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (err) {
      setError('Failed to download PDF');
      console.error('Error downloading PDF:', err);
    }
  };

  const formatCurrency = (amount?: number): string => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount || 0);
  };

  const formatDate = (dateString?: string): string => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  if (loading) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 400 }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (!invoice) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Alert severity="error">Invoice not found</Alert>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/invoices')} sx={{ mt: 2 }}>
          Back to Invoices
        </Button>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}

      {/* Header */}
      <Box sx={{ mb: 4 }}>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/invoices')} sx={{ mb: 2 }}>
          Back to Invoices
        </Button>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
          <Box>
            <Typography variant="h4" gutterBottom>
              Invoice {invoice.invoiceNumber}
            </Typography>
            <Typography color="textSecondary" gutterBottom>
              Customer: {invoice.customerId || 'N/A'}
            </Typography>
          </Box>
          <Stack direction="row" spacing={1} alignItems="center">
            <Chip
              label={getInvoiceStatusLabel(invoice.status)}
              color={getInvoiceStatusColor(invoice.status) as any}
              variant="outlined"
            />
          </Stack>
        </Box>
      </Box>

      <Grid container spacing={3}>
        {/* Main Content */}
        <Grid item xs={12} md={8}>
          {/* Invoice Details */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>Invoice Details</Typography>
              <Divider sx={{ mb: 2 }} />
              <Grid container spacing={2}>
                <Grid item xs={6}>
                  <Typography variant="body2" color="textSecondary">Issue Date</Typography>
                  <Typography variant="body1">{formatDate(invoice.issueDate)}</Typography>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="body2" color="textSecondary">Due Date</Typography>
                  <Typography variant="body1">{formatDate(invoice.dueDate)}</Typography>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="body2" color="textSecondary">Order ID</Typography>
                  <Typography variant="body1">{invoice.orderId || 'N/A'}</Typography>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="body2" color="textSecondary">Quote ID</Typography>
                  <Typography variant="body1">{invoice.quoteId || 'N/A'}</Typography>
                </Grid>
                <Grid item xs={12}>
                  <Typography variant="body2" color="textSecondary">Notes</Typography>
                  <Typography variant="body1">{invoice.notes || 'No notes'}</Typography>
                </Grid>
                <Grid item xs={12}>
                  <Typography variant="body2" color="textSecondary">Terms</Typography>
                  <Typography variant="body1">{invoice.terms || 'Standard terms apply'}</Typography>
                </Grid>
              </Grid>
            </CardContent>
          </Card>

          {/* Line Items */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>Line Items</Typography>
              <Divider sx={{ mb: 2 }} />
              {lineItems.length > 0 ? (
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Description</TableCell>
                      <TableCell align="right">Quantity</TableCell>
                      <TableCell align="right">Unit Price</TableCell>
                      <TableCell align="right">Discount</TableCell>
                      <TableCell align="right">Tax Rate</TableCell>
                      <TableCell align="right">Total</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {lineItems.map((item) => (
                      <TableRow key={item.id}>
                        <TableCell>{item.description || `Product ${item.productId}`}</TableCell>
                        <TableCell align="right">{item.quantity}</TableCell>
                        <TableCell align="right">{formatCurrency(item.unitPrice)}</TableCell>
                        <TableCell align="right">{formatCurrency(item.discount)}</TableCell>
                        <TableCell align="right">{item.taxRate ? `${item.taxRate}%` : '0%'}</TableCell>
                        <TableCell align="right">{formatCurrency(item.totalPrice)}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              ) : (
                <Typography color="textSecondary">No line items found</Typography>
              )}
            </CardContent>
          </Card>

          {/* Payment History */}
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>Payment History</Typography>
              <Divider sx={{ mb: 2 }} />
              <Typography color="textSecondary">Payment history coming soon</Typography>
            </CardContent>
          </Card>
        </Grid>

        {/* Sidebar */}
        <Grid item xs={12} md={4}>
          {/* Amount Summary */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>Amount Summary</Typography>
              <Divider sx={{ mb: 2 }} />
              <Stack spacing={1}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2">Subtotal:</Typography>
                  <Typography variant="body2">{formatCurrency(invoice.subtotal)}</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2">Tax:</Typography>
                  <Typography variant="body2">{formatCurrency(invoice.taxAmount)}</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2">Discount:</Typography>
                  <Typography variant="body2" color="error">
                    -{formatCurrency(invoice.discountAmount)}
                  </Typography>
                </Box>
                <Divider />
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="h6">Total:</Typography>
                  <Typography variant="h6">{formatCurrency(invoice.totalAmount)}</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2">Paid:</Typography>
                  <Typography variant="body2" color="success.main">
                    {formatCurrency(invoice.amountPaid)}
                  </Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body1" fontWeight="bold">Balance Due:</Typography>
                  <Typography variant="body1" fontWeight="bold" color="primary">
                    {formatCurrency(invoice.amountDue)}
                  </Typography>
                </Box>
              </Stack>
            </CardContent>
          </Card>

          {/* Actions */}
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>Actions</Typography>
              <Divider sx={{ mb: 2 }} />
              <Stack spacing={2}>
                <Button
                  variant="outlined"
                  startIcon={<DownloadIcon />}
                  fullWidth
                  onClick={handleDownloadPDF}
                >
                  Download PDF
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<PrintIcon />}
                  fullWidth
                  onClick={() => window.print()}
                >
                  Print
                </Button>
                {invoice.status === InvoiceStatus.Draft && (
                  <Button
                    variant="contained"
                    startIcon={<SendIcon />}
                    fullWidth
                    onClick={() => setSendDialogOpen(true)}
                  >
                    Send Invoice
                  </Button>
                )}
                {(invoice.status === InvoiceStatus.Sent || 
                  invoice.status === InvoiceStatus.PartiallyPaid ||
                  invoice.status === InvoiceStatus.Overdue) && (
                  <Button
                    variant="contained"
                    color="success"
                    startIcon={<PaymentIcon />}
                    fullWidth
                    onClick={() => {
                      setPaymentAmount(invoice.amountDue || 0);
                      setPaymentDialogOpen(true);
                    }}
                  >
                    Record Payment
                  </Button>
                )}
                {invoice.status !== InvoiceStatus.Voided && 
                 invoice.status !== InvoiceStatus.Paid && (
                  <Button
                    variant="outlined"
                    color="error"
                    startIcon={<BlockIcon />}
                    fullWidth
                    onClick={() => setVoidDialogOpen(true)}
                  >
                    Void Invoice
                  </Button>
                )}
                <Button
                  variant="text"
                  startIcon={<RefreshIcon />}
                  fullWidth
                  onClick={() => window.location.reload()}
                >
                  Refresh
                </Button>
              </Stack>
            </CardContent>
          </Card>

          {/* Timeline */}
          <Card sx={{ mt: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>Timeline</Typography>
              <Divider sx={{ mb: 2 }} />
              <Stack spacing={2}>
                <Box>
                  <Typography variant="body2" color="textSecondary">Created</Typography>
                  <Typography variant="body1">{formatDate(invoice.createdAt)}</Typography>
                </Box>
                {invoice.updatedAt && (
                  <Box>
                    <Typography variant="body2" color="textSecondary">Last Updated</Typography>
                    <Typography variant="body1">{formatDate(invoice.updatedAt)}</Typography>
                  </Box>
                )}
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Payment Dialog */}
      <Dialog open={paymentDialogOpen} onClose={() => setPaymentDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Record Payment</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <TextField
              fullWidth
              label="Payment Amount"
              type="number"
              value={paymentAmount}
              onChange={(e) => setPaymentAmount(Number.parseFloat(e.target.value))}
              sx={{ mb: 2 }}
              inputProps={{ min: 0, step: 0.01 }}
            />
            <FormControl fullWidth>
              <InputLabel>Payment Method</InputLabel>
              <Select
                value={paymentMethod}
                onChange={(e) => setPaymentMethod(e.target.value as PaymentMethod)}
                label="Payment Method"
              >
                <MenuItem value={PaymentMethod.CreditCard}>Credit Card</MenuItem>
                <MenuItem value={PaymentMethod.DebitCard}>Debit Card</MenuItem>
                <MenuItem value={PaymentMethod.BankTransfer}>Bank Transfer</MenuItem>
                <MenuItem value={PaymentMethod.Check}>Check</MenuItem>
                <MenuItem value={PaymentMethod.Cash}>Cash</MenuItem>
                <MenuItem value={PaymentMethod.PayPal}>PayPal</MenuItem>
                <MenuItem value={PaymentMethod.Other}>Other</MenuItem>
              </Select>
            </FormControl>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setPaymentDialogOpen(false)} disabled={submitting}>
            Cancel
          </Button>
          <Button 
            onClick={handleRecordPayment} 
            variant="contained" 
            disabled={submitting || paymentAmount <= 0}
          >
            {submitting ? 'Recording...' : 'Record Payment'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Void Dialog */}
      <Dialog open={voidDialogOpen} onClose={() => setVoidDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Void Invoice</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Alert severity="warning" sx={{ mb: 2 }}>
              Voiding an invoice cannot be undone. The invoice will be marked as void and removed from financial reports.
            </Alert>
            <TextField
              fullWidth
              multiline
              rows={4}
              label="Reason for Voiding"
              value={voidReason}
              onChange={(e) => setVoidReason(e.target.value)}
              placeholder="Explain why this invoice is being voided..."
              variant="outlined"
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setVoidDialogOpen(false)} disabled={submitting}>
            Cancel
          </Button>
          <Button 
            onClick={handleVoidInvoice} 
            variant="contained" 
            color="error"
            disabled={submitting || !voidReason.trim()}
          >
            {submitting ? 'Voiding...' : 'Void Invoice'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Send Dialog */}
      <Dialog open={sendDialogOpen} onClose={() => setSendDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Send Invoice</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <TextField
              fullWidth
              label="Recipient Email"
              type="email"
              value={recipientEmail}
              onChange={(e) => setRecipientEmail(e.target.value)}
              placeholder="customer@example.com"
              variant="outlined"
              helperText="Leave blank to use customer's default email"
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSendDialogOpen(false)} disabled={submitting}>
            Cancel
          </Button>
          <Button 
            onClick={handleSendInvoice} 
            variant="contained" 
            disabled={submitting}
          >
            {submitting ? 'Sending...' : 'Send Invoice'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default InvoiceDetailsPage;

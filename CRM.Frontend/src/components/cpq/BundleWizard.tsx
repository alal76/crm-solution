// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
import React, { useCallback, useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Checkbox,
  CircularProgress,
  Divider,
  Grid,
  Step,
  StepLabel,
  Stepper,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import apiClient from '../../services/apiClient';

// ─── Types ────────────────────────────────────────────────────────────────────

interface ProductBundle {
  id: number;
  name: string;
  description?: string;
  basePrice: number;
  currency?: string;
}

interface BundleComponent {
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  included: boolean;
}

interface LineItem {
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  discount: number;
}

interface BundleWizardProps {
  /** Called when the user clicks "Add to Quote" on the final step. */
  onAddToQuote?: (bundle: ProductBundle, items: LineItem[], total: number) => void;
}

// ─── Constants ────────────────────────────────────────────────────────────────

const STEPS = ['Select Bundle', 'Choose Add-ons', 'Configure Pricing', 'Review & Add'];

// ─── Component ────────────────────────────────────────────────────────────────

/**
 * BundleWizard — embedded 4-step MUI Stepper for selecting a product bundle,
 * toggling optional components, applying line-level discounts, and adding to quote.
 * Route: /quotes/bundles  (TODO-GAP-06)
 *
 * Can be used as a standalone page or embedded inside a Dialog.
 */
const BundleWizard: React.FC<BundleWizardProps> = ({ onAddToQuote }) => {
  const [activeStep, setActiveStep] = useState(0);
  const [error, setError] = useState<string | null>(null);

  // Step 1 state
  const [bundles, setBundles] = useState<ProductBundle[]>([]);
  const [loadingBundles, setLoadingBundles] = useState(false);
  const [selectedBundle, setSelectedBundle] = useState<ProductBundle | null>(null);

  // Step 2 state
  const [components, setComponents] = useState<BundleComponent[]>([]);
  const [loadingComponents, setLoadingComponents] = useState(false);

  // Step 3 state — derived from components with discount fields
  const [lineItems, setLineItems] = useState<LineItem[]>([]);

  // ─── Data loading ─────────────────────────────────────────────────────────

  const loadBundles = useCallback(async () => {
    setLoadingBundles(true);
    setError(null);
    try {
      const response = await apiClient.get<ProductBundle[]>('/products/bundles');
      setBundles(response.data);
    } catch {
      setError('Failed to load bundles.');
    } finally {
      setLoadingBundles(false);
    }
  }, []);

  useEffect(() => {
    void loadBundles();
  }, [loadBundles]);

  const loadComponents = useCallback(async (bundleId: number) => {
    setLoadingComponents(true);
    setError(null);
    try {
      const response = await apiClient.get<BundleComponent[]>(
        `/products/bundles/${bundleId}/components`,
      );
      setComponents(response.data.map((c) => ({ ...c, included: true })));
    } catch {
      // Fallback: show an empty component list — the bundle itself still has value
      setComponents([]);
    } finally {
      setLoadingComponents(false);
    }
  }, []);

  // ─── Navigation ───────────────────────────────────────────────────────────

  const handleSelectBundle = (bundle: ProductBundle) => {
    setSelectedBundle(bundle);
    void loadComponents(bundle.id);
  };

  const handleNext = () => {
    if (activeStep === 1) {
      // Derive line items from selected components
      const items: LineItem[] = components
        .filter((c) => c.included)
        .map((c) => ({
          productId: c.productId,
          productName: c.productName,
          quantity: c.quantity,
          unitPrice: c.unitPrice,
          discount: 0,
        }));
      setLineItems(items);
    }
    setActiveStep((s) => s + 1);
  };

  const handleBack = () => setActiveStep((s) => s - 1);

  const handleToggleComponent = (productId: number) => {
    setComponents((prev) =>
      prev.map((c) => (c.productId === productId ? { ...c, included: !c.included } : c)),
    );
  };

  const handleDiscountChange = (productId: number, value: string) => {
    const discount = Math.min(100, Math.max(0, parseFloat(value) || 0));
    setLineItems((prev) =>
      prev.map((li) => (li.productId === productId ? { ...li, discount } : li)),
    );
  };

  const handleQtyChange = (productId: number, value: string) => {
    const qty = Math.max(1, parseInt(value) || 1);
    setLineItems((prev) =>
      prev.map((li) => (li.productId === productId ? { ...li, quantity: qty } : li)),
    );
  };

  const lineTotal = (li: LineItem) =>
    li.unitPrice * li.quantity * (1 - li.discount / 100);

  const grandTotal = lineItems.reduce((sum, li) => sum + lineTotal(li), 0);

  const handleAddToQuote = () => {
    if (selectedBundle) {
      onAddToQuote?.(selectedBundle, lineItems, grandTotal);
    }
  };

  // ─── Step content ─────────────────────────────────────────────────────────

  const step0 = (
    <Box sx={{ mt: 2 }}>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        Choose a product bundle to start configuring your quote.
      </Typography>
      {loadingBundles ? (
        <Box sx={{ textAlign: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : (
        <Grid container spacing={2}>
          {bundles.map((b) => (
            <Grid item xs={12} sm={6} md={4} key={b.id}>
              <Card
                variant="outlined"
                sx={{
                  cursor: 'pointer',
                  border: selectedBundle?.id === b.id ? '2px solid' : undefined,
                  borderColor: selectedBundle?.id === b.id ? 'primary.main' : undefined,
                }}
                onClick={() => handleSelectBundle(b)}
              >
                <CardHeader title={b.name} subheader={b.description} />
                <CardContent>
                  <Typography variant="h6" color="primary">
                    {new Intl.NumberFormat('en-US', {
                      style: 'currency',
                      currency: b.currency ?? 'USD',
                    }).format(b.basePrice)}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}
    </Box>
  );

  const step1 = (
    <Box sx={{ mt: 2 }}>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        Select which components to include in your bundle.
      </Typography>
      {loadingComponents ? (
        <Box sx={{ textAlign: 'center', py: 3 }}>
          <CircularProgress />
        </Box>
      ) : components.length === 0 ? (
        <Alert severity="info">This bundle has no optional components.</Alert>
      ) : (
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell padding="checkbox">Include</TableCell>
              <TableCell>Product</TableCell>
              <TableCell align="right">Qty</TableCell>
              <TableCell align="right">Unit Price</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {components.map((c) => (
              <TableRow key={c.productId}>
                <TableCell padding="checkbox">
                  <Checkbox
                    checked={c.included}
                    onChange={() => handleToggleComponent(c.productId)}
                    size="small"
                  />
                </TableCell>
                <TableCell>{c.productName}</TableCell>
                <TableCell align="right">{c.quantity}</TableCell>
                <TableCell align="right">
                  {new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(
                    c.unitPrice,
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </Box>
  );

  const step2 = (
    <Box sx={{ mt: 2 }}>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        Adjust quantities and apply line-level discounts (%).
      </Typography>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Product</TableCell>
            <TableCell align="right">Qty</TableCell>
            <TableCell align="right">Unit Price</TableCell>
            <TableCell align="right">Discount %</TableCell>
            <TableCell align="right">Line Total</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {lineItems.map((li) => (
            <TableRow key={li.productId}>
              <TableCell>{li.productName}</TableCell>
              <TableCell align="right">
                <TextField
                  type="number"
                  size="small"
                  value={li.quantity}
                  onChange={(e) => handleQtyChange(li.productId, e.target.value)}
                  inputProps={{ min: 1, style: { textAlign: 'right', width: 60 } }}
                  variant="standard"
                />
              </TableCell>
              <TableCell align="right">
                {new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(
                  li.unitPrice,
                )}
              </TableCell>
              <TableCell align="right">
                <TextField
                  type="number"
                  size="small"
                  value={li.discount}
                  onChange={(e) => handleDiscountChange(li.productId, e.target.value)}
                  inputProps={{ min: 0, max: 100, step: 1, style: { textAlign: 'right', width: 60 } }}
                  variant="standard"
                />
              </TableCell>
              <TableCell align="right">
                {new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(
                  lineTotal(li),
                )}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Box>
  );

  const step3 = selectedBundle && (
    <Box sx={{ mt: 2 }}>
      <Typography variant="subtitle1" fontWeight={600} gutterBottom>
        {selectedBundle.name}
      </Typography>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Product</TableCell>
            <TableCell align="right">Qty</TableCell>
            <TableCell align="right">Discount</TableCell>
            <TableCell align="right">Total</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {lineItems.map((li) => (
            <TableRow key={li.productId}>
              <TableCell>{li.productName}</TableCell>
              <TableCell align="right">{li.quantity}</TableCell>
              <TableCell align="right">{li.discount > 0 ? `${li.discount}%` : '—'}</TableCell>
              <TableCell align="right">
                {new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(
                  lineTotal(li),
                )}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <Divider sx={{ my: 2 }} />
      <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2 }}>
        <Typography variant="subtitle1" fontWeight={700}>
          Grand Total:
        </Typography>
        <Typography variant="subtitle1" fontWeight={700} color="primary">
          {new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(
            grandTotal,
          )}
        </Typography>
      </Box>
    </Box>
  );

  // ─── Render ───────────────────────────────────────────────────────────────

  return (
    <Box>
      <Stepper activeStep={activeStep} sx={{ mb: 3 }}>
        {STEPS.map((label) => (
          <Step key={label}>
            <StepLabel>{label}</StepLabel>
          </Step>
        ))}
      </Stepper>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {activeStep === 0 && step0}
      {activeStep === 1 && step1}
      {activeStep === 2 && step2}
      {activeStep === 3 && step3}

      <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 4 }}>
        <Button onClick={handleBack} disabled={activeStep === 0}>
          Back
        </Button>
        {activeStep < 3 ? (
          <Button
            variant="contained"
            onClick={handleNext}
            disabled={activeStep === 0 && !selectedBundle}
          >
            Next
          </Button>
        ) : (
          <Button variant="contained" color="success" onClick={handleAddToQuote}>
            Add to Quote
          </Button>
        )}
      </Box>
    </Box>
  );
};

export default BundleWizard;

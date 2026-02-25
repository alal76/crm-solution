/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * CPQBundleWizardPage — 5-step Configure, Price, Quote wizard
 * for creating bundle-based quotes.
 *
 * Steps:
 *   0. Select Bundle
 *   1. Choose Add-ons (optional items)
 *   2. Configure Quantities
 *   3. Apply Discounts / Promo Code
 *   4. Review & Add to Quote
 */

import React, { useState, useCallback, useEffect } from 'react';
import {
  Box,
  Container,
  Typography,
  Stepper,
  Step,
  StepLabel,
  Button,
  Grid,
  Card,
  CardContent,
  CardActionArea,
  TextField,
  CircularProgress,
  Alert,
  Divider,
  Stack,
  Chip,
  Paper,
  Snackbar,
} from '@mui/material';
import {
  NavigateNext as NextIcon,
  NavigateBefore as BackIcon,
  ShoppingCart as CartIcon,
  CheckCircle as DoneIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import apiClient from '../services/apiClient';
import BundleItemSelector, {
  BundleItem,
  BundleSelectionMap,
} from '../components/sales/BundleItemSelector';
import PriceSummaryPanel, {
  PriceBreakdown,
} from '../components/sales/PriceSummaryPanel';

// ─── Types ──────────────────────────────────────────────────────────────────

interface ProductBundle {
  id: number;
  name: string;
  description?: string;
  bundleCode?: string;
  basePrice: number;
  currency: string;
  pricingType?: number; // BundlePricingType
  discountPercent?: number;
  isActive: boolean;
}

const STEPS = [
  'Select Bundle',
  'Choose Add-ons',
  'Configure Quantities',
  'Apply Discounts',
  'Review & Quote',
];

// ─── Component ──────────────────────────────────────────────────────────────

const CPQBundleWizardPage: React.FC = () => {
  const navigate = useNavigate();

  // ── Stepper ──────────────────────────────────────────────────────────────
  const [activeStep, setActiveStep] = useState(0);

  // ── Step 0: bundles ───────────────────────────────────────────────────────
  const [bundles, setBundles] = useState<ProductBundle[]>([]);
  const [bundlesLoading, setBundlesLoading] = useState(false);
  const [bundlesError, setBundlesError] = useState('');
  const [selectedBundleId, setSelectedBundleId] = useState<number | null>(null);

  // ── Step 1 & 2: bundle items + selection ─────────────────────────────────
  const [bundleItems, setBundleItems] = useState<BundleItem[]>([]);
  const [itemsLoading, setItemsLoading] = useState(false);
  const [itemsError, setItemsError] = useState('');
  const [selection, setSelection] = useState<BundleSelectionMap>({});

  // ── Step 3: promo code ────────────────────────────────────────────────────
  const [promoCode, setPromoCode] = useState('');
  const [breakdown, setBreakdown] = useState<PriceBreakdown | null>(null);
  const [pricingLoading, setPricingLoading] = useState(false);
  const [pricingError, setPricingError] = useState('');

  // ── Step 4: final / submit ────────────────────────────────────────────────
  const [submitting, setSubmitting] = useState(false);
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({ open: false, message: '', severity: 'success' });

  // ── Derived ───────────────────────────────────────────────────────────────
  const selectedBundle = bundles.find(b => b.id === selectedBundleId) ?? null;

  const selectedItems = bundleItems.filter(
    item =>
      item.itemType === 0 || // required
      selection[item.id]?.selected
  );

  const totalSelectedQty = Object.values(selection).reduce(
    (sum, s) => sum + (s.selected ? s.quantity : 0),
    0
  );

  // ── Load bundles on mount ─────────────────────────────────────────────────
  useEffect(() => {
    setBundlesLoading(true);
    apiClient
      .get<ProductBundle[]>('/bundles')
      .then(r => setBundles(r.data))
      .catch(() => setBundlesError('Failed to load bundles.'))
      .finally(() => setBundlesLoading(false));
  }, []);

  // ── Load bundle items when bundle selected ────────────────────────────────
  useEffect(() => {
    if (!selectedBundleId) return;
    setItemsLoading(true);
    setItemsError('');
    apiClient
      .get<BundleItem[]>(`/bundles/${selectedBundleId}/items`)
      .then(r => {
        const items = r.data;
        setBundleItems(items);
        // Pre-populate selection with defaults
        const defaults: BundleSelectionMap = {};
        items.forEach(item => {
          defaults[item.id] = {
            selected: item.itemType === 0 || item.isDefaultSelected,
            quantity: item.defaultQuantity,
          };
        });
        setSelection(defaults);
      })
      .catch(() => setItemsError('Failed to load bundle items.'))
      .finally(() => setItemsLoading(false));
  }, [selectedBundleId]);

  // ── Calculate price (step 3 & 4) ─────────────────────────────────────────
  const calculatePrice = useCallback(async () => {
    if (!selectedBundle) return;
    setPricingLoading(true);
    setPricingError('');
    try {
      const res = await apiClient.post<PriceBreakdown>('/pricingrules/calculate', {
        productId: selectedBundle.id, // bundle treated as product
        quantity: Math.max(1, totalSelectedQty),
        promoCode: promoCode.trim() || undefined,
      });
      setBreakdown(res.data);
    } catch (e: unknown) {
      const msg = (e as { response?: { data?: { message?: string } } })?.response?.data?.message;
      setPricingError(msg ?? 'Failed to calculate price. Please try again.');
    } finally {
      setPricingLoading(false);
    }
  }, [selectedBundle, totalSelectedQty, promoCode]);

  // ── Navigation ────────────────────────────────────────────────────────────
  const handleNext = async () => {
    if (activeStep === 3) {
      await calculatePrice();
    }
    setActiveStep(s => s + 1);
  };

  const handleBack = () => setActiveStep(s => s - 1);

  const canGoNext = () => {
    switch (activeStep) {
      case 0: return selectedBundleId !== null;
      case 1: return true; // optional add-ons; always can proceed
      case 2: return Object.values(selection).some(s => s.selected && s.quantity > 0);
      case 3: return true;
      case 4: return false;
      default: return false;
    }
  };

  // ── Submit ────────────────────────────────────────────────────────────────
  const handleAddToQuote = async () => {
    setSubmitting(true);
    try {
      // Build line items for the quote
      const lineItems = selectedItems.map(item => ({
        productId: item.productId,
        quantity: selection[item.id]?.quantity ?? item.defaultQuantity,
        unitPrice: item.overridePrice ?? item.unitPrice ?? 0,
        description: item.productName,
        bundleItemId: item.id,
      }));

      await apiClient.post('/quotes', {
        bundleId: selectedBundleId,
        promoCode: promoCode.trim() || undefined,
        totalAmount: breakdown?.extendedPrice ?? selectedBundle?.basePrice ?? 0,
        lineItems,
      });

      setSnackbar({ open: true, message: 'Bundle added to quote successfully!', severity: 'success' });
      setTimeout(() => navigate('/quotes'), 1500);
    } catch {
      setSnackbar({ open: true, message: 'Failed to create quote. Please try again.', severity: 'error' });
    } finally {
      setSubmitting(false);
    }
  };

  // ─── Render steps ──────────────────────────────────────────────────────────

  const renderStep0 = () => (
    <Box>
      <Typography variant="h6" gutterBottom>Select a Bundle</Typography>
      {bundlesLoading && <CircularProgress />}
      {bundlesError && <Alert severity="error">{bundlesError}</Alert>}
      <Grid container spacing={2}>
        {bundles.map(bundle => (
          <Grid item xs={12} sm={6} md={4} key={bundle.id}>
            <Card
              variant={selectedBundleId === bundle.id ? 'elevation' : 'outlined'}
              sx={{
                borderColor: selectedBundleId === bundle.id ? 'primary.main' : undefined,
                borderWidth: selectedBundleId === bundle.id ? 2 : 1,
              }}
            >
              <CardActionArea onClick={() => setSelectedBundleId(bundle.id)} sx={{ p: 1 }}>
                <CardContent>
                  <Typography variant="subtitle1" fontWeight="bold" gutterBottom>
                    {bundle.name}
                  </Typography>
                  {bundle.description && (
                    <Typography variant="body2" color="text.secondary" mb={1}>
                      {bundle.description}
                    </Typography>
                  )}
                  <Stack direction="row" alignItems="center" justifyContent="space-between">
                    <Typography variant="h6" color="primary.main">
                      {new Intl.NumberFormat('en-US', {
                        style: 'currency',
                        currency: bundle.currency ?? 'USD',
                      }).format(bundle.basePrice)}
                    </Typography>
                    {bundle.discountPercent && bundle.discountPercent > 0 && (
                      <Chip label={`${bundle.discountPercent}% off`} size="small" color="warning" />
                    )}
                  </Stack>
                  {bundle.bundleCode && (
                    <Typography variant="caption" color="text.secondary">
                      Code: {bundle.bundleCode}
                    </Typography>
                  )}
                </CardContent>
              </CardActionArea>
            </Card>
          </Grid>
        ))}
        {!bundlesLoading && bundles.length === 0 && (
          <Grid item xs={12}>
            <Alert severity="info">No bundles available.</Alert>
          </Grid>
        )}
      </Grid>
    </Box>
  );

  const renderStep1 = () => (
    <Box>
      <Typography variant="h6" gutterBottom>Choose Add-ons</Typography>
      <Typography variant="body2" color="text.secondary" mb={2}>
        Required items are pre-selected. Add optional enhancements to your bundle.
      </Typography>
      <BundleItemSelector
        items={bundleItems.filter(i => i.itemType !== 2)} // non-default (optional/required/exclusive)
        selection={selection}
        onChange={setSelection}
        loading={itemsLoading}
        error={itemsError}
      />
    </Box>
  );

  const renderStep2 = () => (
    <Box>
      <Typography variant="h6" gutterBottom>Configure Quantities</Typography>
      <Typography variant="body2" color="text.secondary" mb={2}>
        Adjust quantities for each selected item.
      </Typography>
      <BundleItemSelector
        items={bundleItems.filter(i => selection[i.id]?.selected || i.itemType === 0)}
        selection={selection}
        onChange={setSelection}
        loading={itemsLoading}
        error={itemsError}
      />
    </Box>
  );

  const renderStep3 = () => (
    <Box>
      <Typography variant="h6" gutterBottom>Apply Discounts</Typography>
      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Typography variant="body2" color="text.secondary" mb={2}>
            Enter a promo code if you have one. Leave blank to continue.
          </Typography>
          <Stack direction="row" spacing={1} alignItems="flex-start">
            <TextField
              label="Promo Code"
              value={promoCode}
              onChange={e => setPromoCode(e.target.value.toUpperCase())}
              size="small"
              helperText="Optional"
              sx={{ flex: 1 }}
            />
            <Button
              variant="outlined"
              onClick={calculatePrice}
              disabled={pricingLoading}
              sx={{ mt: 0.5 }}
            >
              Apply
            </Button>
          </Stack>
          {pricingError && (
            <Alert severity="warning" sx={{ mt: 1 }}>{pricingError}</Alert>
          )}
        </Grid>
        <Grid item xs={12} md={6}>
          <PriceSummaryPanel
            breakdown={breakdown}
            quantity={Math.max(1, totalSelectedQty)}
            loading={pricingLoading}
          />
        </Grid>
      </Grid>
    </Box>
  );

  const renderStep4 = () => (
    <Box>
      <Typography variant="h6" gutterBottom>Review & Add to Quote</Typography>
      <Grid container spacing={3}>
        <Grid item xs={12} md={7}>
          <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
            <Typography variant="subtitle2" fontWeight="bold" gutterBottom>
              Bundle: {selectedBundle?.name}
            </Typography>
            <Divider sx={{ my: 1 }} />
            {selectedItems.map(item => (
              <Stack
                key={item.id}
                direction="row"
                justifyContent="space-between"
                py={0.5}
              >
                <Typography variant="body2">
                  {item.productName ?? `Product #${item.productId}`}
                  {item.itemType === 0 && (
                    <Chip label="Required" size="small" sx={{ ml: 0.5 }} />
                  )}
                </Typography>
                <Typography variant="body2">
                  × {selection[item.id]?.quantity ?? item.defaultQuantity}
                </Typography>
              </Stack>
            ))}
          </Paper>
        </Grid>
        <Grid item xs={12} md={5}>
          <PriceSummaryPanel
            breakdown={breakdown}
            quantity={Math.max(1, totalSelectedQty)}
            loading={pricingLoading}
          />
          {promoCode && breakdown?.promoCodeApplied && (
            <Alert severity="success" sx={{ mt: 1 }}>
              Promo code <strong>{promoCode}</strong> applied!
            </Alert>
          )}
        </Grid>
      </Grid>
    </Box>
  );

  const renderStepContent = () => {
    switch (activeStep) {
      case 0: return renderStep0();
      case 1: return renderStep1();
      case 2: return renderStep2();
      case 3: return renderStep3();
      case 4: return renderStep4();
      default: return null;
    }
  };

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Typography variant="h4" fontWeight="bold" gutterBottom>
        CPQ Bundle Wizard
      </Typography>
      <Typography variant="body1" color="text.secondary" mb={4}>
        Configure, Price, and Quote product bundles step-by-step.
      </Typography>

      <Stepper activeStep={activeStep} alternativeLabel sx={{ mb: 4 }}>
        {STEPS.map(label => (
          <Step key={label}>
            <StepLabel>{label}</StepLabel>
          </Step>
        ))}
      </Stepper>

      <Box mb={4}>{renderStepContent()}</Box>

      <Divider sx={{ mb: 2 }} />

      <Stack direction="row" justifyContent="space-between">
        <Button
          startIcon={<BackIcon />}
          onClick={handleBack}
          disabled={activeStep === 0}
        >
          Back
        </Button>

        {activeStep < STEPS.length - 1 ? (
          <Button
            variant="contained"
            endIcon={<NextIcon />}
            onClick={handleNext}
            disabled={!canGoNext()}
          >
            Next
          </Button>
        ) : (
          <Button
            variant="contained"
            color="success"
            startIcon={submitting ? <CircularProgress size={18} color="inherit" /> : <CartIcon />}
            onClick={handleAddToQuote}
            disabled={submitting}
          >
            Add to Quote
          </Button>
        )}
      </Stack>

      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar(s => ({ ...s, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert
          severity={snackbar.severity}
          icon={snackbar.severity === 'success' ? <DoneIcon /> : undefined}
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Container>
  );
};

export default CPQBundleWizardPage;

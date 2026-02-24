import React, { useState, useCallback, useMemo } from 'react';
import {
  Box,
  Stepper,
  Step,
  StepLabel,
  Button,
  Typography,
  Card,
  CardContent,
  TextField,
  Table,
  TableHead,
  TableBody,
  TableRow,
  TableCell,
  IconButton,
  Chip,
  Paper,
  Grid,
  InputAdornment,
  Alert,
  Divider,
  Autocomplete,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/Delete';
import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import { Product } from '../../types/crm';

// ---------- Types ----------

export interface BundleLineItem {
  product: Product;
  quantity: number;
  unitPrice: number;
  discountPercent: number;
  lineTotal: number;
}

export interface BundleConfig {
  name: string;
  description: string;
  items: BundleLineItem[];
  bundleDiscount: number;
}

export interface ProductBundleWizardProps {
  /** Available products to choose from */
  products: Product[];
  /** Callback when the bundle is confirmed */
  onComplete: (bundle: BundleConfig) => void;
  /** Callback when wizard is cancelled */
  onCancel?: () => void;
  /** Initial bundle to edit (optional) */
  initialBundle?: Partial<BundleConfig>;
}

// ---------- Steps ----------
const steps = ['Bundle Details', 'Select Products', 'Configure Pricing', 'Review & Confirm'];

// ---------- Component ----------
const ProductBundleWizard: React.FC<ProductBundleWizardProps> = ({
  products,
  onComplete,
  onCancel,
  initialBundle,
}) => {
  const [activeStep, setActiveStep] = useState(0);
  const [bundleName, setBundleName] = useState(initialBundle?.name ?? '');
  const [bundleDescription, setBundleDescription] = useState(initialBundle?.description ?? '');
  const [items, setItems] = useState<BundleLineItem[]>(initialBundle?.items ?? []);
  const [bundleDiscount, setBundleDiscount] = useState(initialBundle?.bundleDiscount ?? 0);
  const [errors, setErrors] = useState<string[]>([]);

  // Filtered products that haven't been added yet
  const availableProducts = useMemo(
    () => products.filter((p) => !items.some((item) => item.product.id === p.id)),
    [products, items],
  );

  // Totals
  const subtotal = useMemo(() => items.reduce((sum, item) => sum + item.lineTotal, 0), [items]);
  const bundleDiscountAmount = useMemo(
    () => subtotal * (bundleDiscount / 100),
    [subtotal, bundleDiscount],
  );
  const grandTotal = useMemo(() => subtotal - bundleDiscountAmount, [subtotal, bundleDiscountAmount]);

  // --- Add Product ---
  const handleAddProduct = useCallback(
    (product: Product | null) => {
      if (!product) return;
      const unitPrice = product.unitPrice ?? 0;
      setItems((prev) => [
        ...prev,
        {
          product,
          quantity: 1,
          unitPrice,
          discountPercent: 0,
          lineTotal: unitPrice,
        },
      ]);
    },
    [],
  );

  // --- Remove Line ---
  const handleRemoveItem = useCallback((productId: number) => {
    setItems((prev) => prev.filter((item) => item.product.id !== productId));
  }, []);

  // --- Update Quantity ---
  const handleQuantityChange = useCallback((productId: number, qty: number) => {
    setItems((prev) =>
      prev.map((item) => {
        if (item.product.id !== productId) return item;
        const q = Math.max(1, qty);
        const lineTotal = q * item.unitPrice * (1 - item.discountPercent / 100);
        return { ...item, quantity: q, lineTotal };
      }),
    );
  }, []);

  // --- Update Discount ---
  const handleDiscountChange = useCallback((productId: number, discount: number) => {
    setItems((prev) =>
      prev.map((item) => {
        if (item.product.id !== productId) return item;
        const d = Math.max(0, Math.min(100, discount));
        const lineTotal = item.quantity * item.unitPrice * (1 - d / 100);
        return { ...item, discountPercent: d, lineTotal };
      }),
    );
  }, []);

  // --- Validation ---
  const validate = useCallback((): boolean => {
    const errs: string[] = [];
    if (activeStep === 0) {
      if (!bundleName.trim()) errs.push('Bundle name is required.');
    }
    if (activeStep === 1) {
      if (items.length === 0) errs.push('Add at least one product to the bundle.');
    }
    if (activeStep === 2) {
      if (bundleDiscount < 0 || bundleDiscount > 100) errs.push('Bundle discount must be 0-100%.');
      if (items.some((i) => i.discountPercent < 0 || i.discountPercent > 100))
        errs.push('Line item discount must be 0-100%.');
    }
    setErrors(errs);
    return errs.length === 0;
  }, [activeStep, bundleName, items, bundleDiscount]);

  const handleNext = useCallback(() => {
    if (!validate()) return;
    if (activeStep === steps.length - 1) {
      onComplete({
        name: bundleName.trim(),
        description: bundleDescription.trim(),
        items,
        bundleDiscount,
      });
    } else {
      setActiveStep((s) => s + 1);
      setErrors([]);
    }
  }, [activeStep, validate, onComplete, bundleName, bundleDescription, items, bundleDiscount]);

  const handleBack = useCallback(() => {
    setActiveStep((s) => s - 1);
    setErrors([]);
  }, []);

  // ---------- Step Content ----------
  const renderStepContent = () => {
    switch (activeStep) {
      // ----- Step 0: Bundle Details -----
      case 0:
        return (
          <Box sx={{ mt: 3 }}>
            <TextField
              label="Bundle Name"
              value={bundleName}
              onChange={(e) => setBundleName(e.target.value)}
              fullWidth
              required
              sx={{ mb: 3 }}
              placeholder="e.g. Enterprise Security Suite"
            />
            <TextField
              label="Description"
              value={bundleDescription}
              onChange={(e) => setBundleDescription(e.target.value)}
              fullWidth
              multiline
              rows={3}
              placeholder="Describe the bundle for customers..."
            />
          </Box>
        );

      // ----- Step 1: Select Products -----
      case 1:
        return (
          <Box sx={{ mt: 3 }}>
            <Autocomplete
              options={availableProducts}
              getOptionLabel={(p) => `${p.name}${p.sku ? ` (${p.sku})` : ''}`}
              onChange={(_, val) => handleAddProduct(val)}
              renderInput={(params) => (
                <TextField {...params} label="Search & Add Product" placeholder="Type to search..." />
              )}
              value={null}
              blurOnSelect
              sx={{ mb: 3 }}
            />

            {items.length === 0 ? (
              <Paper sx={{ p: 4, textAlign: 'center' }}>
                <ShoppingCartIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 1 }} />
                <Typography color="text.secondary">
                  No products added yet. Search above to add products to the bundle.
                </Typography>
              </Paper>
            ) : (
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Product</TableCell>
                    <TableCell>Category</TableCell>
                    <TableCell align="right">Unit Price</TableCell>
                    <TableCell align="center">Qty</TableCell>
                    <TableCell align="center" />
                  </TableRow>
                </TableHead>
                <TableBody>
                  {items.map((item) => (
                    <TableRow key={item.product.id}>
                      <TableCell>
                        <Typography variant="body2" fontWeight={500}>
                          {item.product.name}
                        </Typography>
                        {item.product.sku && (
                          <Typography variant="caption" color="text.secondary">
                            SKU: {item.product.sku}
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell>
                        <Chip label={item.product.category ?? 'General'} size="small" />
                      </TableCell>
                      <TableCell align="right">
                        ${item.unitPrice.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                      </TableCell>
                      <TableCell align="center" sx={{ width: 100 }}>
                        <TextField
                          type="number"
                          size="small"
                          value={item.quantity}
                          onChange={(e) =>
                            handleQuantityChange(item.product.id, parseInt(e.target.value, 10) || 1)
                          }
                          inputProps={{ min: 1, style: { textAlign: 'center' } }}
                          sx={{ width: 80 }}
                        />
                      </TableCell>
                      <TableCell align="center">
                        <IconButton
                          color="error"
                          size="small"
                          onClick={() => handleRemoveItem(item.product.id)}
                        >
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </Box>
        );

      // ----- Step 2: Configure Pricing -----
      case 2:
        return (
          <Box sx={{ mt: 3 }}>
            <Typography variant="subtitle1" gutterBottom fontWeight={600}>
              Line Item Pricing
            </Typography>
            <Table size="small" sx={{ mb: 3 }}>
              <TableHead>
                <TableRow>
                  <TableCell>Product</TableCell>
                  <TableCell align="right">Unit Price</TableCell>
                  <TableCell align="center">Qty</TableCell>
                  <TableCell align="center">Discount %</TableCell>
                  <TableCell align="right">Line Total</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {items.map((item) => (
                  <TableRow key={item.product.id}>
                    <TableCell>{item.product.name}</TableCell>
                    <TableCell align="right">
                      ${item.unitPrice.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                    </TableCell>
                    <TableCell align="center">{item.quantity}</TableCell>
                    <TableCell align="center" sx={{ width: 120 }}>
                      <TextField
                        type="number"
                        size="small"
                        value={item.discountPercent}
                        onChange={(e) =>
                          handleDiscountChange(
                            item.product.id,
                            parseFloat(e.target.value) || 0,
                          )
                        }
                        InputProps={{
                          endAdornment: <InputAdornment position="end">%</InputAdornment>,
                        }}
                        inputProps={{ min: 0, max: 100, step: 1 }}
                        sx={{ width: 100 }}
                      />
                    </TableCell>
                    <TableCell align="right" sx={{ fontWeight: 600 }}>
                      ${item.lineTotal.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>

            <Divider sx={{ mb: 3 }} />

            <Grid container spacing={2} alignItems="center">
              <Grid item xs={12} sm={6}>
                <TextField
                  label="Bundle-Level Discount"
                  type="number"
                  size="small"
                  value={bundleDiscount}
                  onChange={(e) => setBundleDiscount(Math.max(0, Math.min(100, parseFloat(e.target.value) || 0)))}
                  InputProps={{
                    endAdornment: <InputAdornment position="end">%</InputAdornment>,
                  }}
                  inputProps={{ min: 0, max: 100, step: 1 }}
                  helperText="Applied on top of line-item discounts"
                  fullWidth
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <Paper sx={{ p: 2, bgcolor: 'primary.50' }}>
                  <Typography variant="body2" color="text.secondary">
                    Subtotal: ${subtotal.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                  </Typography>
                  {bundleDiscount > 0 && (
                    <Typography variant="body2" color="error">
                      Bundle Discount: -$
                      {bundleDiscountAmount.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                    </Typography>
                  )}
                  <Typography variant="h6" fontWeight={700} sx={{ mt: 0.5 }}>
                    Total: ${grandTotal.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                  </Typography>
                </Paper>
              </Grid>
            </Grid>
          </Box>
        );

      // ----- Step 3: Review & Confirm -----
      case 3:
        return (
          <Box sx={{ mt: 3 }}>
            <Card variant="outlined" sx={{ mb: 3 }}>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  {bundleName}
                </Typography>
                {bundleDescription && (
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                    {bundleDescription}
                  </Typography>
                )}
                <Divider sx={{ mb: 2 }} />
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Product</TableCell>
                      <TableCell align="center">Qty</TableCell>
                      <TableCell align="right">Unit Price</TableCell>
                      <TableCell align="center">Discount</TableCell>
                      <TableCell align="right">Line Total</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {items.map((item) => (
                      <TableRow key={item.product.id}>
                        <TableCell>{item.product.name}</TableCell>
                        <TableCell align="center">{item.quantity}</TableCell>
                        <TableCell align="right">
                          ${item.unitPrice.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                        </TableCell>
                        <TableCell align="center">
                          {item.discountPercent > 0 ? `${item.discountPercent}%` : '—'}
                        </TableCell>
                        <TableCell align="right">
                          ${item.lineTotal.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
                <Divider sx={{ my: 2 }} />
                <Box sx={{ display: 'flex', justifyContent: 'flex-end', flexDirection: 'column', alignItems: 'flex-end' }}>
                  <Typography variant="body2">
                    Subtotal: ${subtotal.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                  </Typography>
                  {bundleDiscount > 0 && (
                    <Typography variant="body2" color="error">
                      Bundle Discount ({bundleDiscount}%): -$
                      {bundleDiscountAmount.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                    </Typography>
                  )}
                  <Typography variant="h6" fontWeight={700} sx={{ mt: 1 }}>
                    Grand Total: ${grandTotal.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                  </Typography>
                </Box>
              </CardContent>
            </Card>

            <Alert severity="info" icon={<CheckCircleIcon />}>
              Review the bundle configuration above. Click <strong>Confirm Bundle</strong> to finalize.
            </Alert>
          </Box>
        );

      default:
        return null;
    }
  };

  return (
    <Box sx={{ width: '100%' }}>
      <Stepper activeStep={activeStep} sx={{ mb: 4 }}>
        {steps.map((label) => (
          <Step key={label}>
            <StepLabel>{label}</StepLabel>
          </Step>
        ))}
      </Stepper>

      {errors.length > 0 && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {errors.map((e, i) => (
            <div key={i}>{e}</div>
          ))}
        </Alert>
      )}

      {renderStepContent()}

      <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 4 }}>
        <Box>
          {onCancel && (
            <Button variant="text" onClick={onCancel}>
              Cancel
            </Button>
          )}
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          {activeStep > 0 && (
            <Button variant="outlined" onClick={handleBack}>
              Back
            </Button>
          )}
          <Button
            variant="contained"
            onClick={handleNext}
            startIcon={activeStep === steps.length - 1 ? <CheckCircleIcon /> : <AddIcon />}
          >
            {activeStep === steps.length - 1 ? 'Confirm Bundle' : 'Next'}
          </Button>
        </Box>
      </Box>
    </Box>
  );
};

export default ProductBundleWizard;

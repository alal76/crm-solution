/**
 * Quote Form Validation Schema
 * Validates quote creation and updates
 */
import * as Yup from 'yup';
import { QuoteLineItem } from '../types/sales';

/**
 * Quote line item validation
 */
const quoteLineItemSchema = Yup.object().shape({
  productId: Yup.number()
    .required('Product is required')
    .positive('Product ID must be positive'),
  quantity: Yup.number()
    .required('Quantity is required')
    .positive('Quantity must be greater than 0')
    .integer('Quantity must be a whole number'),
  unitPrice: Yup.number()
    .required('Unit price is required')
    .min(0, 'Unit price cannot be negative'),
  discount: Yup.number()
    .min(0, 'Discount cannot be negative')
    .nullable(),
  tax: Yup.number()
    .min(0, 'Tax cannot be negative')
    .nullable()
});

/**
 * Quote validation schema
 */
export const quoteValidationSchema = Yup.object().shape({
  accountId: Yup.number()
    .required('Account is required')
    .positive('Account must be selected'),
  contactId: Yup.number()
    .typeError('Contact must be a number')
    .nullable()
    .positive('Contact ID must be positive'),
  expiryDate: Yup.date()
    .required('Expiry date is required')
    .typeError('Expiry date must be a valid date')
    .min(new Date(), 'Expiry date must be in the future'),
  lineItems: Yup.array()
    .of(quoteLineItemSchema)
    .required('At least one line item is required')
    .min(1, 'At least one line item is required'),
  notes: Yup.string()
    .max(2000, 'Notes cannot exceed 2000 characters'),
  terms: Yup.string()
    .max(2000, 'Terms cannot exceed 2000 characters'),
  discount: Yup.number()
    .min(0, 'Discount cannot be negative')
    .nullable(),
  currency: Yup.string()
    .length(3, 'Currency code must be 3 characters')
    .uppercase()
});

/**
 * Calculate quote tax based on line items
 */
export const calculateQuoteTax = (
  lineItems: QuoteLineItem[],
  taxRate: number = 0
): number => {
  const subtotal = lineItems.reduce((sum, item) => {
    return sum + (item.quantity * item.unitPrice - (item.discount || 0));
  }, 0);
  return Math.round((subtotal * taxRate) * 100) / 100;
};

/**
 * Calculate quote total
 */
export const calculateQuoteTotal = (
  lineItems: QuoteLineItem[],
  taxRate: number = 0,
  discount: number = 0,
  shippingCost: number = 0
): number => {
  const subtotal = lineItems.reduce((sum, item) => {
    return sum + (item.quantity * item.unitPrice - (item.discount || 0));
  }, 0);
  const tax = calculateQuoteTax(lineItems, taxRate);
  return Math.round((subtotal + tax + shippingCost - discount) * 100) / 100;
};

/**
 * Validate line item pricing
 */
export const validateLineItemPrice = (
  quantity: number,
  unitPrice: number,
  expected: number,
  tolerance: number = 0.01
): boolean => {
  const calculated = quantity * unitPrice;
  return Math.abs(calculated - expected) <= tolerance;
};

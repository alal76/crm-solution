/**
 * Order Form Validation Schema
 * Validates order creation and updates matching backend rules
 */
import * as Yup from 'yup';
import { OrderLineItem } from '../types/sales';

/**
 * Line item validation - validates each product in the order
 */
const lineItemSchema = Yup.object().shape({
  productId: Yup.number()
    .required('Product is required')
    .positive('Product ID must be positive'),
  quantity: Yup.number()
    .required('Quantity is required')
    .positive('Quantity must be greater than 0')
    .integer('Quantity must be a whole number')
    .max(999999, 'Quantity cannot exceed 999,999'),
  unitPrice: Yup.number()
    .required('Unit price is required')
    .min(0, 'Unit price cannot be negative')
});

/**
 * Order form validation schema
 */
export const orderValidationSchema = Yup.object().shape({
  accountId: Yup.number()
    .required('Account is required')
    .positive('Account must be selected'),
  contactId: Yup.number()
    .typeError('Contact must be a number')
    .nullable()
    .positive('Contact ID must be positive'),
  orderDate: Yup.date()
    .required('Order date is required')
    .typeError('Order date must be a valid date'),
  requiredDate: Yup.date()
    .typeError('Required date must be a valid date')
    .nullable()
    .min(
      Yup.ref('orderDate'),
      'Required date cannot be before order date'
    ),
  lineItems: Yup.array()
    .of(lineItemSchema)
    .required('At least one line item is required')
    .min(1, 'At least one line item is required'),
  shippingAddress: Yup.string()
    .max(500, 'Shipping address cannot exceed 500 characters'),
  billingAddress: Yup.string()
    .max(500, 'Billing address cannot exceed 500 characters'),
  paymentTerms: Yup.string()
    .max(100, 'Payment terms cannot exceed 100 characters'),
  dueDate: Yup.date()
    .typeError('Due date must be a valid date')
    .nullable(),
  notes: Yup.string()
    .max(2000, 'Notes cannot exceed 2000 characters'),
  currency: Yup.string()
    .length(3, 'Currency code must be 3 characters')
    .uppercase()
    .matches(/^[A-Z]{3}$/, 'Currency must be a valid ISO 4217 code')
});

/**
 * Calculate tax on line items
 */
export const calculateOrderTax = (
  lineItems: OrderLineItem[],
  taxRate: number = 0
): number => {
  const subtotal = lineItems.reduce((sum, item) => {
    return sum + (item.quantity * item.unitPrice - (item.discount || 0));
  }, 0);
  return Math.round((subtotal * taxRate) * 100) / 100;
};

/**
 * Calculate order total
 */
export const calculateOrderTotal = (
  lineItems: OrderLineItem[],
  shippingCost: number = 0,
  taxRate: number = 0,
  discount: number = 0
): number => {
  const subtotal = lineItems.reduce((sum, item) => {
    return sum + (item.quantity * item.unitPrice - (item.discount || 0));
  }, 0);
  const tax = calculateOrderTax(lineItems, taxRate);
  return Math.round((subtotal + tax + shippingCost - discount) * 100) / 100;
};

/**
 * Validate order totals match calculated values
 */
export const validateOrderTotals = (
  subtotal: number,
  calculated: number,
  tolerance: number = 0.01
): boolean => {
  return Math.abs(subtotal - calculated) <= tolerance;
};

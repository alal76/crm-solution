/**
 * Invoice Form Validation Schema
 * Validates invoice creation and updates with payment terms and due dates
 */
import * as Yup from 'yup';
import { InvoiceLineItem } from '../types/sales';

/**
 * Invoice line item validation
 */
const invoiceLineItemSchema = Yup.object().shape({
  description: Yup.string()
    .required('Description is required')
    .max(500, 'Description cannot exceed 500 characters'),
  quantity: Yup.number()
    .required('Quantity is required')
    .positive('Quantity must be greater than 0')
    .integer('Quantity must be a whole number'),
  unitPrice: Yup.number()
    .required('Unit price is required')
    .min(0, 'Unit price cannot be negative'),
  tax: Yup.number()
    .min(0, 'Tax cannot be negative')
    .nullable()
});

/**
 * Payment terms enum for validation
 */
export const PAYMENT_TERMS = {
  NET_15: 'net_15',
  NET_30: 'net_30',
  NET_45: 'net_45',
  NET_60: 'net_60',
  DUE_ON_RECEIPT: 'due_on_receipt',
  CUSTOM: 'custom'
} as const;

/**
 * Calculate due date based on payment terms
 */
export const calculateDueDate = (
  invoiceDate: Date,
  paymentTerms: string
): Date => {
  const dueDate = new Date(invoiceDate);
  
  switch (paymentTerms) {
    case PAYMENT_TERMS.NET_15:
      dueDate.setDate(dueDate.getDate() + 15);
      break;
    case PAYMENT_TERMS.NET_30:
      dueDate.setDate(dueDate.getDate() + 30);
      break;
    case PAYMENT_TERMS.NET_45:
      dueDate.setDate(dueDate.getDate() + 45);
      break;
    case PAYMENT_TERMS.NET_60:
      dueDate.setDate(dueDate.getDate() + 60);
      break;
    case PAYMENT_TERMS.DUE_ON_RECEIPT:
      return invoiceDate;
    default:
      dueDate.setDate(dueDate.getDate() + 30); // Default to 30 days
  }
  
  return dueDate;
};

/**
 * Invoice validation schema
 */
export const invoiceValidationSchema = Yup.object().shape({
  accountId: Yup.number()
    .required('Account is required')
    .positive('Account must be selected'),
  invoiceDate: Yup.date()
    .required('Invoice date is required')
    .typeError('Invoice date must be a valid date'),
  dueDate: Yup.date()
    .required('Due date is required')
    .typeError('Due date must be a valid date')
    .min(
      Yup.ref('invoiceDate'),
      'Due date cannot be before invoice date'
    ),
  lineItems: Yup.array()
    .of(invoiceLineItemSchema)
    .required('At least one line item is required')
    .min(1, 'At least one line item is required'),
  paymentTerms: Yup.string()
    .oneOf(
      Object.values(PAYMENT_TERMS),
      'Invalid payment terms'
    ),
  notes: Yup.string()
    .max(2000, 'Notes cannot exceed 2000 characters'),
  currency: Yup.string()
    .length(3, 'Currency code must be 3 characters')
    .uppercase()
    .matches(/^[A-Z]{3}$/, 'Currency must be a valid ISO 4217 code')
});

/**
 * Calculate invoice total
 */
export const calculateInvoiceTotal = (
  lineItems: InvoiceLineItem[],
  taxRate: number = 0
): number => {
  const subtotal = lineItems.reduce((sum, item) => {
    return sum + (item.quantity * item.unitPrice - (item.tax || 0));
  }, 0);
  const tax = Math.round((subtotal * taxRate) * 100) / 100;
  return Math.round((subtotal + tax) * 100) / 100;
};

/**
 * Check if invoice is overdue
 */
export const isInvoiceOverdue = (dueDate: string): boolean => {
  const due = new Date(dueDate);
  return due < new Date();
};

/**
 * Calculate days until due
 */
export const daysUntilDue = (dueDate: string): number => {
  const due = new Date(dueDate);
  const today = new Date();
  const diffTime = due.getTime() - today.getTime();
  return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
};

/**
 * Calculate remaining balance (unpaid amount)
 */
export const calculateRemainingBalance = (
  totalAmount: number,
  paidAmount: number
): number => {
  return Math.round((totalAmount - paidAmount) * 100) / 100;
};

/**
 * Validate payment does not exceed invoice amount
 */
export const validatePaymentAmount = (
  payment: number,
  invoiceTotal: number,
  paidAmount: number = 0
): boolean => {
  const remaining = invoiceTotal - paidAmount;
  return payment >= 0 && payment <= remaining;
};

/**
 * ITSM Form Validation Schemas
 * Yup schemas for ITSM module forms matching backend validation rules
 */
import * as Yup from 'yup';

// ============================================================================
// Incident Form
// ============================================================================

export const incidentFormSchema = Yup.object().shape({
  shortDescription: Yup.string()
    .required('Short description is required')
    .min(5, 'Must be at least 5 characters')
    .max(200, 'Cannot exceed 200 characters'),
  description: Yup.string()
    .max(4000, 'Cannot exceed 4000 characters'),
  callerId: Yup.number()
    .min(0, 'Caller is required'),
  impact: Yup.number()
    .required('Impact is required')
    .min(1, 'Impact must be selected')
    .max(3, 'Invalid impact value'),
  urgency: Yup.number()
    .required('Urgency is required')
    .min(1, 'Urgency must be selected')
    .max(3, 'Invalid urgency value'),
  categoryId: Yup.number()
    .min(0, 'Invalid category'),
});

// ============================================================================
// Change Form
// ============================================================================

export const changeFormSchema = Yup.object().shape({
  shortDescription: Yup.string()
    .required('Short description is required')
    .min(5, 'Must be at least 5 characters')
    .max(200, 'Cannot exceed 200 characters'),
  description: Yup.string()
    .max(4000, 'Cannot exceed 4000 characters'),
  type: Yup.number()
    .required('Change type is required')
    .min(1)
    .max(3),
  risk: Yup.number()
    .required('Risk level is required')
    .min(1)
    .max(3),
  impact: Yup.number()
    .required('Impact level is required')
    .min(1)
    .max(3),
  plannedStartDate: Yup.string()
    .required('Planned start date is required'),
  plannedEndDate: Yup.string()
    .required('Planned end date is required')
    .test('after-start', 'End date must be after start date', function (value) {
      const { plannedStartDate } = this.parent;
      if (!plannedStartDate || !value) return true;
      return new Date(value) > new Date(plannedStartDate);
    }),
  implementationPlan: Yup.string()
    .required('Implementation plan is required')
    .min(10, 'Implementation plan must be at least 10 characters')
    .max(4000, 'Cannot exceed 4000 characters'),
  backoutPlan: Yup.string()
    .required('Backout plan is required')
    .min(10, 'Backout plan must be at least 10 characters')
    .max(4000, 'Cannot exceed 4000 characters'),
});

// ============================================================================
// Problem Form
// ============================================================================

export const problemFormSchema = Yup.object().shape({
  title: Yup.string()
    .required('Title is required')
    .min(5, 'Must be at least 5 characters')
    .max(200, 'Cannot exceed 200 characters'),
  description: Yup.string()
    .required('Description is required')
    .min(10, 'Must be at least 10 characters')
    .max(4000, 'Cannot exceed 4000 characters'),
  priority: Yup.number()
    .required('Priority is required')
    .min(0)
    .max(4),
  category: Yup.number()
    .required('Category is required')
    .min(0)
    .max(6),
});

// ============================================================================
// Service Request Form
// ============================================================================

export const serviceRequestFormSchema = Yup.object().shape({
  title: Yup.string()
    .required('Title is required')
    .min(3, 'Must be at least 3 characters')
    .max(200, 'Cannot exceed 200 characters'),
  description: Yup.string()
    .max(4000, 'Cannot exceed 4000 characters'),
  priority: Yup.number()
    .required('Priority is required')
    .min(0)
    .max(4),
  categoryId: Yup.number()
    .min(0, 'Invalid category'),
});

// ============================================================================
// Rollback Reason
// ============================================================================

export const rollbackReasonSchema = Yup.object().shape({
  reason: Yup.string()
    .required('Rollback reason is required')
    .min(10, 'Reason must be at least 10 characters')
    .max(2000, 'Cannot exceed 2000 characters'),
});

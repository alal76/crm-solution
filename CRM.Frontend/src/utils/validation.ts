/**
 * Zod schemas for API response validation.
 * Use these schemas to validate API responses at runtime for type safety.
 */

import { z } from 'zod';

// =============================================================================
// Base Schemas
// =============================================================================

export const paginatedResponseSchema = <T extends z.ZodTypeAny>(itemSchema: T) =>
  z.object({
    items: z.array(itemSchema),
    totalCount: z.number().int().nonnegative(),
    page: z.number().int().positive().optional(),
    pageSize: z.number().int().positive().optional(),
    hasMore: z.boolean().optional(),
  });

export const apiErrorSchema = z.object({
  message: z.string(),
  code: z.string().optional(),
  details: z.record(z.string(), z.unknown()).optional(),
});

export const timestampSchema = z.object({
  createdAt: z.string().datetime().optional(),
  updatedAt: z.string().datetime().optional(),
});

// =============================================================================
// User & Auth Schemas
// =============================================================================

export const userSchema = z.object({
  id: z.number().int().positive(),
  email: z.string().email(),
  firstName: z.string().min(1).max(100),
  lastName: z.string().min(1).max(100),
  displayName: z.string().optional(),
  role: z.string(),
  isActive: z.boolean(),
  profilePictureUrl: z.string().url().nullable().optional(),
  ...timestampSchema.shape,
});

export const authResponseSchema = z.object({
  token: z.string().min(1),
  refreshToken: z.string().optional(),
  user: userSchema,
  expiresAt: z.string().datetime().optional(),
});

// =============================================================================
// Customer/Account Schemas
// =============================================================================

export const customerSchema = z.object({
  id: z.number().int().positive(),
  company: z.string().min(1).max(200),
  email: z.string().email().nullable().optional(),
  phone: z.string().max(50).nullable().optional(),
  website: z.string().url().nullable().optional(),
  industry: z.string().nullable().optional(),
  lifecycleStage: z.number().int().nonnegative().optional(),
  status: z.string().optional(),
  addressLine1: z.string().nullable().optional(),
  addressLine2: z.string().nullable().optional(),
  city: z.string().nullable().optional(),
  state: z.string().nullable().optional(),
  postalCode: z.string().nullable().optional(),
  country: z.string().nullable().optional(),
  notes: z.string().nullable().optional(),
  assignedToId: z.number().int().positive().nullable().optional(),
  ...timestampSchema.shape,
});

export const customersResponseSchema = paginatedResponseSchema(customerSchema);

// =============================================================================
// Contact Schemas
// =============================================================================

export const contactSchema = z.object({
  id: z.number().int().positive(),
  firstName: z.string().min(1).max(100),
  lastName: z.string().min(1).max(100),
  email: z.string().email().nullable().optional(),
  phone: z.string().max(50).nullable().optional(),
  mobile: z.string().max(50).nullable().optional(),
  jobTitle: z.string().nullable().optional(),
  department: z.string().nullable().optional(),
  accountId: z.number().int().positive().nullable().optional(),
  isPrimary: z.boolean().optional(),
  status: z.string().optional(),
  notes: z.string().nullable().optional(),
  ...timestampSchema.shape,
});

export const contactsResponseSchema = paginatedResponseSchema(contactSchema);

// =============================================================================
// Lead Schemas
// =============================================================================

export const leadSchema = z.object({
  id: z.number().int().positive(),
  firstName: z.string().min(1).max(100),
  lastName: z.string().min(1).max(100),
  email: z.string().email().nullable().optional(),
  phone: z.string().max(50).nullable().optional(),
  company: z.string().nullable().optional(),
  jobTitle: z.string().nullable().optional(),
  source: z.string().nullable().optional(),
  status: z.string().optional(),
  score: z.number().int().nonnegative().optional(),
  isQualified: z.boolean().optional(),
  assignedToId: z.number().int().positive().nullable().optional(),
  notes: z.string().nullable().optional(),
  ...timestampSchema.shape,
});

export const leadsResponseSchema = paginatedResponseSchema(leadSchema);

// =============================================================================
// Opportunity Schemas
// =============================================================================

export const opportunitySchema = z.object({
  id: z.number().int().positive(),
  name: z.string().min(1).max(200),
  accountId: z.number().int().positive().nullable().optional(),
  contactId: z.number().int().positive().nullable().optional(),
  value: z.number().nonnegative().optional(),
  currency: z.string().length(3).optional(),
  stage: z.string().optional(),
  probability: z.number().min(0).max(100).optional(),
  expectedCloseDate: z.string().datetime().nullable().optional(),
  status: z.string().optional(),
  source: z.string().nullable().optional(),
  assignedToId: z.number().int().positive().nullable().optional(),
  notes: z.string().nullable().optional(),
  ...timestampSchema.shape,
});

export const opportunitiesResponseSchema = paginatedResponseSchema(opportunitySchema);

// =============================================================================
// Service Request Schemas
// =============================================================================

export const serviceRequestSchema = z.object({
  id: z.number().int().positive(),
  subject: z.string().min(1).max(200),
  description: z.string().nullable().optional(),
  accountId: z.number().int().positive().nullable().optional(),
  contactId: z.number().int().positive().nullable().optional(),
  status: z.string().optional(),
  priority: z.string().optional(),
  category: z.string().nullable().optional(),
  assignedToId: z.number().int().positive().nullable().optional(),
  resolvedAt: z.string().datetime().nullable().optional(),
  ...timestampSchema.shape,
});

export const serviceRequestsResponseSchema = paginatedResponseSchema(serviceRequestSchema);

// =============================================================================
// Campaign Schemas
// =============================================================================

export const campaignSchema = z.object({
  id: z.number().int().positive(),
  name: z.string().min(1).max(200),
  description: z.string().nullable().optional(),
  type: z.string().optional(),
  status: z.string().optional(),
  startDate: z.string().datetime().nullable().optional(),
  endDate: z.string().datetime().nullable().optional(),
  budget: z.number().nonnegative().optional(),
  actualCost: z.number().nonnegative().optional(),
  expectedRevenue: z.number().nonnegative().optional(),
  actualRevenue: z.number().nonnegative().optional(),
  ownerId: z.number().int().positive().nullable().optional(),
  ...timestampSchema.shape,
});

export const campaignsResponseSchema = paginatedResponseSchema(campaignSchema);

// =============================================================================
// Validation Helper Functions
// =============================================================================

/**
 * Validate API response data against a schema
 * @param schema - Zod schema to validate against
 * @param data - Data to validate
 * @returns Validated and typed data
 * @throws ZodError if validation fails
 */
export function validate<T extends z.ZodTypeAny>(
  schema: T,
  data: unknown
): z.infer<T> {
  return schema.parse(data);
}

/**
 * Safely validate API response data, returning null on failure
 * @param schema - Zod schema to validate against
 * @param data - Data to validate
 * @returns Validated data or null if validation fails
 */
export function validateSafe<T extends z.ZodTypeAny>(
  schema: T,
  data: unknown
): z.infer<T> | null {
  const result = schema.safeParse(data);
  return result.success ? result.data : null;
}

/**
 * Validate with detailed error information
 * @param schema - Zod schema to validate against
 * @param data - Data to validate
 * @returns Object with success status, data, and errors
 */
export function validateWithErrors<T extends z.ZodTypeAny>(
  schema: T,
  data: unknown
): { success: boolean; data: z.infer<T> | null; errors: z.ZodError | null } {
  const result = schema.safeParse(data);
  if (result.success) {
    return { success: true, data: result.data, errors: null };
  }
  return { success: false, data: null, errors: result.error };
}

// =============================================================================
// Type Exports
// =============================================================================

export type User = z.infer<typeof userSchema>;
export type AuthResponse = z.infer<typeof authResponseSchema>;
export type Customer = z.infer<typeof customerSchema>;
export type Contact = z.infer<typeof contactSchema>;
export type Lead = z.infer<typeof leadSchema>;
export type Opportunity = z.infer<typeof opportunitySchema>;
export type ServiceRequest = z.infer<typeof serviceRequestSchema>;
export type Campaign = z.infer<typeof campaignSchema>;
export type ApiError = z.infer<typeof apiErrorSchema>;

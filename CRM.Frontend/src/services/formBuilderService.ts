/**
 * Form Builder Service
 * 
 * Provides API operations for form builder functionality including:
 * - Form definitions CRUD
 * - Form fields management
 * - Form submissions
 * - Lead conversion from form submissions
 */
import apiClient from './apiClient';

// ============================================================================
// Types and Interfaces
// ============================================================================

export interface FormField {
  id?: number;
  formId: number;
  name: string;
  label: string;
  fieldType: FieldType;
  placeholder?: string;
  defaultValue?: string;
  isRequired: boolean;
  validationRules?: string;
  options?: FieldOption[];
  sortOrder: number;
  cssClass?: string;
  helpText?: string;
  conditionalLogic?: ConditionalLogic;
}

export interface FieldOption {
  value: string;
  label: string;
  isDefault?: boolean;
}

export interface ConditionalLogic {
  action: 'show' | 'hide';
  conditions: Condition[];
  logicalOperator: 'and' | 'or';
}

export interface Condition {
  fieldName: string;
  operator: 'equals' | 'notEquals' | 'contains' | 'notContains' | 'greaterThan' | 'lessThan';
  value: string;
}

export enum FieldType {
  Text = 'text',
  Email = 'email',
  Phone = 'phone',
  Number = 'number',
  TextArea = 'textarea',
  Select = 'select',
  MultiSelect = 'multiselect',
  Checkbox = 'checkbox',
  Radio = 'radio',
  Date = 'date',
  DateTime = 'datetime',
  File = 'file',
  Hidden = 'hidden',
  Rating = 'rating',
  Signature = 'signature',
}

export interface FormDefinition {
  id?: number;
  name: string;
  slug: string;
  description?: string;
  headerHtml?: string;
  footerHtml?: string;
  successMessage?: string;
  redirectUrl?: string;
  isPublished: boolean;
  isEmbeddable: boolean;
  submitButtonText: string;
  notifyEmails?: string;
  confirmationEmailTemplateId?: number;
  leadSourceId?: number;
  campaignId?: number;
  fields: FormField[];
  createdAt?: string;
  updatedAt?: string;
  submissionCount?: number;
  conversionCount?: number;
}

export interface FormSubmission {
  id?: number;
  formId: number;
  formName?: string;
  data: Record<string, any>;
  ipAddress?: string;
  userAgent?: string;
  referrer?: string;
  utmSource?: string;
  utmMedium?: string;
  utmCampaign?: string;
  submittedAt?: string;
  isConverted?: boolean;
  convertedLeadId?: number;
  convertedAt?: string;
}

export interface FormStatistics {
  formId: number;
  formName: string;
  totalSubmissions: number;
  totalConversions: number;
  conversionRate: number;
  submissionsLast7Days: number;
  submissionsLast30Days: number;
  submissionsByDay: DailySubmissions[];
  topReferrers: ReferrerStats[];
}

export interface DailySubmissions {
  date: string;
  count: number;
}

export interface ReferrerStats {
  referrer: string;
  count: number;
  percentage: number;
}

export interface CreateFormDto {
  name: string;
  slug?: string;
  description?: string;
  submitButtonText?: string;
  successMessage?: string;
  redirectUrl?: string;
  isEmbeddable?: boolean;
  notifyEmails?: string;
  leadSourceId?: number;
  campaignId?: number;
}

export interface UpdateFormDto extends Partial<CreateFormDto> {
  headerHtml?: string;
  footerHtml?: string;
  confirmationEmailTemplateId?: number;
}

// ============================================================================
// Form Builder Service
// ============================================================================

const formBuilderService = {
  // === Form Definition CRUD ===
  
  /**
   * Get all form definitions
   */
  getAllForms: (includeUnpublished: boolean = true) =>
    apiClient.get<FormDefinition[]>(`/forms?includeUnpublished=${includeUnpublished}`),

  /**
   * Get published forms only
   */
  getPublishedForms: () =>
    apiClient.get<FormDefinition[]>('/forms/published'),

  /**
   * Get form by ID
   */
  getFormById: (id: number) =>
    apiClient.get<FormDefinition>(`/forms/${id}`),

  /**
   * Get form by slug (for public embedding)
   */
  getFormBySlug: (slug: string) =>
    apiClient.get<FormDefinition>(`/forms/slug/${slug}`),

  /**
   * Create a new form
   */
  createForm: (data: CreateFormDto) =>
    apiClient.post<FormDefinition>('/forms', data),

  /**
   * Update an existing form
   */
  updateForm: (id: number, data: UpdateFormDto) =>
    apiClient.put<FormDefinition>(`/forms/${id}`, data),

  /**
   * Delete a form
   */
  deleteForm: (id: number) =>
    apiClient.delete(`/forms/${id}`),

  /**
   * Publish a form
   */
  publishForm: (id: number) =>
    apiClient.post<FormDefinition>(`/forms/${id}/publish`),

  /**
   * Unpublish a form
   */
  unpublishForm: (id: number) =>
    apiClient.post<FormDefinition>(`/forms/${id}/unpublish`),

  /**
   * Clone a form
   */
  cloneForm: (id: number, newName: string) =>
    apiClient.post<FormDefinition>(`/forms/${id}/clone`, { newName }),

  // === Form Fields ===

  /**
   * Get all fields for a form
   */
  getFormFields: (formId: number) =>
    apiClient.get<FormField[]>(`/forms/${formId}/fields`),

  /**
   * Add a field to a form
   */
  addField: (formId: number, field: Omit<FormField, 'id' | 'formId'>) =>
    apiClient.post<FormField>(`/forms/${formId}/fields`, { ...field, formId }),

  /**
   * Update a form field
   */
  updateField: (formId: number, fieldId: number, field: Partial<FormField>) =>
    apiClient.put<FormField>(`/forms/${formId}/fields/${fieldId}`, field),

  /**
   * Remove a field from a form
   */
  removeField: (formId: number, fieldId: number) =>
    apiClient.delete(`/forms/${formId}/fields/${fieldId}`),

  /**
   * Reorder form fields
   */
  reorderFields: (formId: number, fieldIds: number[]) =>
    apiClient.post(`/forms/${formId}/fields/reorder`, { fieldIds }),

  // === Form Submissions ===

  /**
   * Get all submissions for a form
   */
  getSubmissions: (formId: number, page: number = 1, pageSize: number = 50) =>
    apiClient.get<{ items: FormSubmission[]; totalCount: number; page: number; pageSize: number }>(
      `/forms/${formId}/submissions?page=${page}&pageSize=${pageSize}`
    ),

  /**
   * Get a specific submission
   */
  getSubmissionById: (formId: number, submissionId: number) =>
    apiClient.get<FormSubmission>(`/forms/${formId}/submissions/${submissionId}`),

  /**
   * Submit a form (public endpoint)
   */
  submitForm: (formId: number, data: Record<string, any>) =>
    apiClient.post<FormSubmission>(`/forms/${formId}/submit`, data),

  /**
   * Submit a form by slug (public endpoint)
   */
  submitFormBySlug: (slug: string, data: Record<string, any>) =>
    apiClient.post<FormSubmission>(`/forms/slug/${slug}/submit`, data),

  /**
   * Delete a submission
   */
  deleteSubmission: (formId: number, submissionId: number) =>
    apiClient.delete(`/forms/${formId}/submissions/${submissionId}`),

  /**
   * Export submissions to CSV
   */
  exportSubmissions: (formId: number, fromDate?: string, toDate?: string) => {
    let url = `/forms/${formId}/submissions/export`;
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    if (params.toString()) url += `?${params.toString()}`;
    return apiClient.get(url, { responseType: 'blob' });
  },

  // === Lead Conversion ===

  /**
   * Convert a submission to a lead
   */
  convertToLead: (formId: number, submissionId: number, additionalData?: Record<string, any>) =>
    apiClient.post<{ leadId: number }>(`/forms/${formId}/submissions/${submissionId}/convert`, additionalData),

  /**
   * Bulk convert submissions to leads
   */
  bulkConvertToLeads: (formId: number, submissionIds: number[]) =>
    apiClient.post<{ convertedCount: number; failedCount: number }>(
      `/forms/${formId}/submissions/bulk-convert`,
      { submissionIds }
    ),

  // === Form Statistics ===

  /**
   * Get form statistics
   */
  getFormStatistics: (formId: number) =>
    apiClient.get<FormStatistics>(`/forms/${formId}/statistics`),

  /**
   * Get all forms statistics summary
   */
  getAllFormsStatistics: () =>
    apiClient.get<FormStatistics[]>('/forms/statistics'),

  // === Embed Code ===

  /**
   * Get embed code for a form
   */
  getEmbedCode: (formId: number, style: 'inline' | 'modal' | 'popup' = 'inline') =>
    apiClient.get<{ html: string; javascript: string }>(`/forms/${formId}/embed?style=${style}`),

  // === Form Preview ===

  /**
   * Preview a form (returns rendered HTML)
   */
  previewForm: (id: number) =>
    apiClient.get<{ html: string }>(`/forms/${id}/preview`),

  // === Validation ===

  /**
   * Validate form field value
   */
  validateField: (formId: number, fieldName: string, value: any) =>
    apiClient.post<{ isValid: boolean; errors: string[] }>(
      `/forms/${formId}/validate-field`,
      { fieldName, value }
    ),
};

export default formBuilderService;

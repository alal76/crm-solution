import apiClient from './apiClient';
import { LinkedEmailDto, LinkedPhoneDto, LinkedAddressDto, LinkedSocialMediaDto } from './contactInfoService';

export interface Customer {
  id?: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  company: string;
  lifecycleStage: number;
  
  // Normalized Contact Info Collections
  emailAddresses?: LinkedEmailDto[];
  phoneNumbers?: LinkedPhoneDto[];
  addresses?: LinkedAddressDto[];
  socialMediaAccounts?: LinkedSocialMediaDto[];
}

export const customerService = {
  getAll: () => apiClient.get<Customer[]>('/customers'),
  getById: (id: number) => apiClient.get<Customer>(`/customers/${id}`),
  search: (term: string) => apiClient.get<Customer[]>(`/customers/search/${term}`),
  create: (data: Customer) => apiClient.post<Customer>('/customers', data),
  update: (id: number, data: Customer) => apiClient.put(`/customers/${id}`, data),
  delete: (id: number) => apiClient.delete(`/customers/${id}`),
};

export interface Opportunity {
  id?: number;
  name: string;
  description?: string;
  amount: number;
  stage: number;
  customerId: number;
  probability: number;
  expectedCloseDate?: string;
  closeDate?: string;
  type?: number;
  priority?: number;
  forecastCategory?: number;
  createdAt?: string;
  lastActivityDate?: string;
  customerName?: string;
  ownerName?: string;
  source?: string;
  weightedAmount?: number;
}

export const opportunityService = {
  getAll: () => apiClient.get<Opportunity[]>('/opportunities'),
  getById: (id: number) => apiClient.get<Opportunity>(`/opportunities/${id}`),
  getByCustomer: (customerId: number) => 
    apiClient.get<Opportunity[]>(`/opportunities/customer/${customerId}`),
  getTotalPipeline: () => apiClient.get(`/opportunities/pipeline/total`),
  create: (data: Opportunity) => apiClient.post<Opportunity>('/opportunities', data),
  update: (id: number, data: Opportunity) => apiClient.put(`/opportunities/${id}`, data),
  delete: (id: number) => apiClient.delete(`/opportunities/${id}`),
};

export interface Product {
  id?: number;
  name: string;
  sku: string;
  price: number;
  cost: number;
  category: string;
  isActive: boolean;
}

export const productService = {
  getAll: () => apiClient.get<Product[]>('/products'),
  getById: (id: number) => apiClient.get<Product>(`/products/${id}`),
  getByCategory: (category: string) => 
    apiClient.get<Product[]>(`/products/category/${category}`),
  create: (data: Product) => apiClient.post<Product>('/products', data),
  update: (id: number, data: Product) => apiClient.put(`/products/${id}`, data),
  delete: (id: number) => apiClient.delete(`/products/${id}`),
};

export interface MarketingCampaign {
  id?: number;
  name: string;
  type: string;
  budget: number;
  startDate: string;
  endDate?: string;
  status: number;
}

export const campaignService = {
  getAll: () => apiClient.get<MarketingCampaign[]>('/campaigns'),
  getActive: () => apiClient.get<MarketingCampaign[]>('/campaigns/active'),
  getById: (id: number) => apiClient.get<MarketingCampaign>(`/campaigns/${id}`),
  create: (data: MarketingCampaign) => apiClient.post<MarketingCampaign>('/campaigns', data),
  update: (id: number, data: MarketingCampaign) => 
    apiClient.put(`/campaigns/${id}`, data),
  delete: (id: number) => apiClient.delete(`/campaigns/${id}`),
  addMetric: (id: number, metric: any) => 
    apiClient.post(`/campaigns/${id}/metrics`, metric),
};

// Service Request Types and Enums
export enum ServiceRequestChannel {
  WhatsApp = 0,
  Email = 1,
  Phone = 2,
  InPerson = 3,
  SelfServicePortal = 4,
  SocialMedia = 5,
  LiveChat = 6,
  API = 7
}

export enum ServiceRequestStatus {
  New = 0,
  Open = 1,
  InProgress = 2,
  PendingCustomer = 3,
  PendingInternal = 4,
  Escalated = 5,
  Resolved = 6,
  Closed = 7,
  Cancelled = 8,
  OnHold = 9,
  Reopened = 10
}

export enum ServiceRequestPriority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3,
  Urgent = 4
}

export enum CustomFieldType {
  Text = 0,
  TextArea = 1,
  Number = 2,
  Decimal = 3,
  Date = 4,
  DateTime = 5,
  Dropdown = 6,
  MultiSelect = 7,
  Boolean = 8,
  Email = 9,
  Phone = 10,
  Url = 11
}

export interface ServiceRequestCategory {
  id?: number;
  name: string;
  description?: string;
  icon?: string;
  color?: string;
  isActive: boolean;
  sortOrder: number;
  defaultPriority: ServiceRequestPriority;
  slaResponseHours?: number;
  slaResolutionHours?: number;
  subcategories?: ServiceRequestSubcategory[];
}

export interface CreateServiceRequestCategory {
  name: string;
  description?: string;
  icon?: string;
  color?: string;
  isActive?: boolean;
  sortOrder?: number;
  defaultPriority?: ServiceRequestPriority;
  slaResponseHours?: number;
  slaResolutionHours?: number;
}

export interface ServiceRequestSubcategory {
  id?: number;
  categoryId: number;
  categoryName?: string;
  name: string;
  description?: string;
  isActive: boolean;
  sortOrder: number;
  defaultWorkflowId?: number;
  defaultWorkflowName?: string;
  defaultAssigneeGroupId?: number;
  defaultAssigneeGroupName?: string;
  slaResponseHours?: number;
  slaResolutionHours?: number;
}

export interface CreateServiceRequestSubcategory {
  categoryId: number;
  name: string;
  description?: string;
  isActive?: boolean;
  sortOrder?: number;
  defaultWorkflowId?: number;
  defaultAssigneeGroupId?: number;
  slaResponseHours?: number;
  slaResolutionHours?: number;
}

// Service Request Type interfaces
export interface ServiceRequestType {
  id: number;
  name: string;
  requestType: string; // 'Complaint' or 'Request'
  detailedDescription?: string;
  workflowName?: string;
  possibleResolutions?: string;
  finalCustomerResolutions?: string;
  possibleResolutionsList: string[];
  finalCustomerResolutionsList: string[];
  categoryId: number;
  categoryName?: string;
  subcategoryId: number;
  subcategoryName?: string;
  displayOrder: number;
  isActive: boolean;
  defaultPriority?: string;
  responseTimeHours?: number;
  resolutionTimeHours?: number;
  tags?: string;
  tagsList: string[];
}

export interface ServiceRequestTypeGrouped {
  categoryId: number;
  categoryName: string;
  subcategories: SubcategoryWithTypes[];
}

export interface SubcategoryWithTypes {
  subcategoryId: number;
  subcategoryName: string;
  types: ServiceRequestType[];
}

export interface CreateServiceRequestType {
  name: string;
  requestType: string;
  detailedDescription?: string;
  workflowName?: string;
  possibleResolutions?: string;
  finalCustomerResolutions?: string;
  categoryId: number;
  subcategoryId: number;
  displayOrder?: number;
  isActive?: boolean;
  defaultPriority?: string;
  responseTimeHours?: number;
  resolutionTimeHours?: number;
  tags?: string;
}

export interface UpdateServiceRequestType extends CreateServiceRequestType {
  id: number;
}

export interface ServiceRequestCustomFieldDefinition {
  id?: number;
  name: string;
  label: string;
  description?: string;
  fieldType: CustomFieldType;
  isRequired: boolean;
  isActive: boolean;
  sortOrder: number;
  defaultValue?: string;
  options?: string;
  validationPattern?: string;
  validationMessage?: string;
  minValue?: number;
  maxValue?: number;
  appliesToCategoryId?: number;
  appliesToCategoryName?: string;
  appliesToSubcategoryId?: number;
  appliesToSubcategoryName?: string;
}

export interface CreateServiceRequestCustomFieldDefinition {
  name: string;
  label: string;
  description?: string;
  fieldType: CustomFieldType;
  isRequired?: boolean;
  isActive?: boolean;
  sortOrder?: number;
  defaultValue?: string;
  options?: string;
  validationPattern?: string;
  validationMessage?: string;
  minValue?: number;
  maxValue?: number;
  appliesToCategoryId?: number;
  appliesToSubcategoryId?: number;
}

export interface ServiceRequestCustomFieldValue {
  id?: number;
  fieldDefinitionId: number;
  fieldName: string;
  fieldLabel: string;
  fieldType: CustomFieldType;
  value?: string;
}

export interface ServiceRequest {
  id?: number;
  ticketNumber?: string;
  subject: string;
  description?: string;
  channel: ServiceRequestChannel;
  channelName?: string;
  status: ServiceRequestStatus;
  statusName?: string;
  priority: ServiceRequestPriority;
  priorityName?: string;
  categoryId?: number;
  categoryName?: string;
  subcategoryId?: number;
  subcategoryName?: string;
  customerId?: number;
  customerName?: string;
  contactId?: number;
  contactName?: string;
  assignedToUserId?: number;
  assignedToUserName?: string;
  assignedToGroupId?: number;
  assignedToGroupName?: string;
  workflowId?: number;
  workflowName?: string;
  currentWorkflowStepId?: number;
  currentWorkflowStepName?: string;
  customFieldValues?: ServiceRequestCustomFieldValue[];
  slaResponseDueDate?: string;
  slaResolutionDueDate?: string;
  firstResponseDate?: string;
  resolvedDate?: string;
  closedDate?: string;
  isSlaBreached: boolean;
  customerSatisfactionRating?: number;
  customerFeedback?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateServiceRequest {
  subject: string;
  description?: string;
  channel: ServiceRequestChannel;
  priority?: ServiceRequestPriority;
  categoryId?: number;
  subcategoryId?: number;
  customerId?: number;
  contactId?: number;
  assignedToUserId?: number;
  assignedToGroupId?: number;
  workflowId?: number;
  customFieldValues?: { fieldDefinitionId: number; value: string }[];
}

export interface UpdateServiceRequest {
  subject?: string;
  description?: string;
  priority?: ServiceRequestPriority;
  categoryId?: number;
  subcategoryId?: number;
  customerId?: number;
  contactId?: number;
  assignedToUserId?: number;
  assignedToGroupId?: number;
  workflowId?: number;
  customFieldValues?: { fieldDefinitionId: number; value: string }[];
}

export interface ServiceRequestFilter {
  searchTerm?: string;
  status?: ServiceRequestStatus[];
  priority?: ServiceRequestPriority[];
  channel?: ServiceRequestChannel[];
  categoryId?: number;
  subcategoryId?: number;
  customerId?: number;
  contactId?: number;
  assignedToUserId?: number;
  assignedToGroupId?: number;
  createdFrom?: string;
  createdTo?: string;
  slaBreached?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface PagedServiceRequestResult {
  items: ServiceRequest[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface ServiceRequestStatistics {
  totalRequests: number;
  openRequests: number;
  closedRequests: number;
  resolvedRequests: number;
  slaBreachedRequests: number;
  averageResolutionTimeHours: number;
  averageResponseTimeHours: number;
  requestsByChannel: { [key: string]: number };
  requestsByPriority: { [key: string]: number };
  requestsByStatus: { [key: string]: number };
  requestsByCategory: { [key: string]: number };
}

export const serviceRequestService = {
  getAll: (filter?: ServiceRequestFilter) => 
    apiClient.get<PagedServiceRequestResult>('/servicerequests', { params: filter }),
  getById: (id: number) => apiClient.get<ServiceRequest>(`/servicerequests/${id}`),
  create: (data: CreateServiceRequest) => 
    apiClient.post<ServiceRequest>('/servicerequests', data),
  update: (id: number, data: UpdateServiceRequest) => 
    apiClient.put<ServiceRequest>(`/servicerequests/${id}`, data),
  delete: (id: number) => apiClient.delete(`/servicerequests/${id}`),
  
  // Status operations
  updateStatus: (id: number, status: ServiceRequestStatus, notes?: string) =>
    apiClient.patch<ServiceRequest>(`/servicerequests/${id}/status`, { status, notes }),
  resolve: (id: number, resolutionNotes: string) =>
    apiClient.post<ServiceRequest>(`/servicerequests/${id}/resolve`, { resolutionNotes }),
  close: (id: number, notes?: string) =>
    apiClient.post<ServiceRequest>(`/servicerequests/${id}/close`, { notes }),
  reopen: (id: number, reason: string) =>
    apiClient.post<ServiceRequest>(`/servicerequests/${id}/reopen`, { reason }),
  escalate: (id: number, reason: string, escalateToGroupId?: number) =>
    apiClient.post<ServiceRequest>(`/servicerequests/${id}/escalate`, { reason, escalateToGroupId }),
  
  // Assignment
  assignToUser: (id: number, userId: number, notes?: string) =>
    apiClient.post<ServiceRequest>(`/servicerequests/${id}/assign/user/${userId}`, { notes }),
  assignToGroup: (id: number, groupId: number, notes?: string) =>
    apiClient.post<ServiceRequest>(`/servicerequests/${id}/assign/group/${groupId}`, { notes }),
  
  // Custom fields
  updateCustomFields: (id: number, values: { customFieldDefinitionId: number; textValue?: string; numericValue?: number; dateValue?: string; booleanValue?: boolean }[]) =>
    apiClient.put(`/servicerequests/${id}/custom-fields`, values),
  
  // Feedback
  addFeedback: (id: number, rating: number, feedback?: string) =>
    apiClient.post(`/servicerequests/${id}/feedback`, { rating, feedback }),
  
  // Queries
  getByCustomer: (customerId: number) => 
    apiClient.get<ServiceRequest[]>(`/servicerequests/customer/${customerId}`),
  getByContact: (contactId: number) => 
    apiClient.get<ServiceRequest[]>(`/servicerequests/contact/${contactId}`),
  getByAssignee: (userId: number) => 
    apiClient.get<ServiceRequest[]>(`/servicerequests/assignee/${userId}`),
  getMyRequests: () => apiClient.get<ServiceRequest[]>('/servicerequests/my-requests'),
  
  // Statistics
  getStatistics: (startDate?: string, endDate?: string) =>
    apiClient.get<ServiceRequestStatistics>('/servicerequests/statistics', { 
      params: { startDate, endDate } 
    }),
  getOpenCount: () => apiClient.get<number>('/servicerequests/count/open'),
  getSlaBreachedCount: () => apiClient.get<number>('/servicerequests/count/sla-breached'),
};

export const serviceRequestCategoryService = {
  getAll: (includeInactive = false) => 
    apiClient.get<ServiceRequestCategory[]>('/service-request-settings/categories', { 
      params: { includeInactive } 
    }),
  getById: (id: number) => 
    apiClient.get<ServiceRequestCategory>(`/service-request-settings/categories/${id}`),
  create: (data: CreateServiceRequestCategory) => 
    apiClient.post<ServiceRequestCategory>('/service-request-settings/categories', data),
  update: (id: number, data: Partial<CreateServiceRequestCategory>) => 
    apiClient.put<ServiceRequestCategory>(`/service-request-settings/categories/${id}`, data),
  delete: (id: number) => apiClient.delete(`/service-request-settings/categories/${id}`),
  reorder: (categoryIds: number[]) => 
    apiClient.post('/service-request-settings/categories/reorder', categoryIds),
};

export const serviceRequestSubcategoryService = {
  getAll: (includeInactive = false) => 
    apiClient.get<ServiceRequestSubcategory[]>('/service-request-settings/subcategories', { 
      params: { includeInactive } 
    }),
  getByCategory: (categoryId: number, includeInactive = false) => 
    apiClient.get<ServiceRequestSubcategory[]>(
      `/service-request-settings/categories/${categoryId}/subcategories`, 
      { params: { includeInactive } }
    ),
  getById: (id: number) => 
    apiClient.get<ServiceRequestSubcategory>(`/service-request-settings/subcategories/${id}`),
  create: (data: CreateServiceRequestSubcategory) => 
    apiClient.post<ServiceRequestSubcategory>('/service-request-settings/subcategories', data),
  update: (id: number, data: Partial<CreateServiceRequestSubcategory>) => 
    apiClient.put<ServiceRequestSubcategory>(`/service-request-settings/subcategories/${id}`, data),
  delete: (id: number) => apiClient.delete(`/service-request-settings/subcategories/${id}`),
  reorder: (categoryId: number, subcategoryIds: number[]) => 
    apiClient.post(`/service-request-settings/categories/${categoryId}/subcategories/reorder`, subcategoryIds),
};

export const serviceRequestCustomFieldService = {
  getAll: (includeInactive = false) => 
    apiClient.get<ServiceRequestCustomFieldDefinition[]>('/service-request-settings/custom-fields', { 
      params: { includeInactive } 
    }),
  getApplicable: (categoryId?: number, subcategoryId?: number) => 
    apiClient.get<ServiceRequestCustomFieldDefinition[]>('/service-request-settings/custom-fields/applicable', { 
      params: { categoryId, subcategoryId } 
    }),
  getById: (id: number) => 
    apiClient.get<ServiceRequestCustomFieldDefinition>(`/service-request-settings/custom-fields/${id}`),
  create: (data: CreateServiceRequestCustomFieldDefinition) => 
    apiClient.post<ServiceRequestCustomFieldDefinition>('/service-request-settings/custom-fields', data),
  update: (id: number, data: Partial<CreateServiceRequestCustomFieldDefinition>) => 
    apiClient.put<ServiceRequestCustomFieldDefinition>(`/service-request-settings/custom-fields/${id}`, data),
  delete: (id: number) => apiClient.delete(`/service-request-settings/custom-fields/${id}`),
  reorder: (fieldIds: number[]) => 
    apiClient.post('/service-request-settings/custom-fields/reorder', fieldIds),
  getCount: () => apiClient.get<{ activeCount: number; maxAllowed: number }>(
    '/service-request-settings/custom-fields/count'
  ),
};

export const serviceRequestTypeService = {
  getAll: (includeInactive = false) => 
    apiClient.get<ServiceRequestType[]>('/service-request-settings/types', { 
      params: { includeInactive } 
    }),
  getGrouped: (includeInactive = false) => 
    apiClient.get<ServiceRequestTypeGrouped[]>('/service-request-settings/types/grouped', { 
      params: { includeInactive } 
    }),
  getByCategory: (categoryId: number, includeInactive = false) => 
    apiClient.get<ServiceRequestType[]>(`/service-request-settings/types/by-category/${categoryId}`, { 
      params: { includeInactive } 
    }),
  getBySubcategory: (subcategoryId: number, includeInactive = false) => 
    apiClient.get<ServiceRequestType[]>(`/service-request-settings/types/by-subcategory/${subcategoryId}`, { 
      params: { includeInactive } 
    }),
  getById: (id: number) => 
    apiClient.get<ServiceRequestType>(`/service-request-settings/types/${id}`),
  create: (data: CreateServiceRequestType) => 
    apiClient.post<ServiceRequestType>('/service-request-settings/types', data),
  update: (id: number, data: Partial<CreateServiceRequestType>) => 
    apiClient.put<ServiceRequestType>(`/service-request-settings/types/${id}`, data),
  delete: (id: number) => apiClient.delete(`/service-request-settings/types/${id}`),
  reorder: (subcategoryId: number, typeIds: number[]) => 
    apiClient.post(`/service-request-settings/types/reorder/${subcategoryId}`, typeIds),
};

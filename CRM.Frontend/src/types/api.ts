export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiError {
  message: string;
  error?: string;
  statusCode?: number;
}

export interface Account {
  id: number;
  category?: string;
  firstName?: string;
  lastName?: string;
  company?: string;
  email?: string;
  phone?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface Contact {
  id: number;
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  accountId?: number;
}

export interface Lead {
  id: number;
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  companyName?: string;
  status?: string;
  ownerId?: number;
}

export interface Opportunity {
  id: number;
  name?: string;
  accountId?: number;
  amount?: number;
  stage?: string;
  probability?: number;
}

export interface Quote {
  id: number;
  quoteNumber?: string;
  accountId?: number;
  status?: string;
  totalAmount?: number;
}

export interface QuoteLineItem {
  id: number;
  quoteId: number;
  productId?: number;
  description?: string;
  quantity?: number;
  unitPrice?: number;
  totalAmount?: number;
}

export interface Order {
  id: number;
  orderNumber?: string;
  accountId?: number;
  status?: string;
  totalAmount?: number;
}

export interface OrderLineItem {
  id: number;
  orderId: number;
  productId?: number;
  description?: string;
  quantity?: number;
  unitPrice?: number;
  totalAmount?: number;
}

export interface Invoice {
  id: number;
  invoiceNumber?: string;
  accountId?: number;
  status?: string;
  totalAmount?: number;
}

export interface Payment {
  id: number;
  invoiceId?: number;
  accountId?: number;
  amount?: number;
  status?: string;
  method?: string;
}

export interface CreditMemo {
  id: number;
  creditMemoNumber?: string;
  accountId?: number;
  amount?: number;
  status?: string;
}

export interface Subscription {
  id: number;
  subscriptionNumber?: string;
  accountId?: number;
  status?: string;
  amount?: number;
}

export interface Commission {
  id: number;
  userId?: number;
  status?: string;
  amount?: number;
}

export interface CommissionPlan {
  id: number;
  name?: string;
  isActive?: boolean;
}

export interface Campaign {
  id: number;
  name?: string;
  status?: string;
}

export interface EmailTemplate {
  id: number;
  name?: string;
  subject?: string;
  category?: string;
}

export interface EmailSequence {
  id: number;
  name?: string;
  status?: string;
}

export interface ServiceRequest {
  id: number;
  title?: string;
  status?: string;
  priority?: string;
}

export interface KnowledgeArticle {
  articleId: number;
  title?: string;
  status?: string;
}

import apiClient from './apiClient';

export interface Customer {
  id?: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  company: string;
  lifecycleStage: number;
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
  description: string;
  amount: number;
  stage: number;
  customerId: number;
  probability: number;
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

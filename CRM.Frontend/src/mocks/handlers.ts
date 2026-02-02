/**
 * MSW (Mock Service Worker) handlers for API mocking in tests.
 * These handlers intercept API calls and return mock responses.
 */

import { rest } from 'msw';

// Mock data factories
import { 
  createMockCustomer, 
  createMockContact, 
  createMockLead,
  createMockOpportunity,
  createMockServiceRequest,
  createMockCampaign,
  createMockUser,
} from './factories';

// Base URL for API calls (relative to avoid CORS issues)
const API_BASE = '/api';

// Default mock data
const mockCustomers = Array.from({ length: 10 }, (_, i) => createMockCustomer({ id: i + 1 }));
const mockContacts = Array.from({ length: 10 }, (_, i) => createMockContact({ id: i + 1 }));
const mockLeads = Array.from({ length: 10 }, (_, i) => createMockLead({ id: i + 1 }));
const mockOpportunities = Array.from({ length: 10 }, (_, i) => createMockOpportunity({ id: i + 1 }));
const mockServiceRequests = Array.from({ length: 10 }, (_, i) => createMockServiceRequest({ id: i + 1 }));
const mockCampaigns = Array.from({ length: 5 }, (_, i) => createMockCampaign({ id: i + 1 }));
const mockUsers = Array.from({ length: 5 }, (_, i) => createMockUser({ id: i + 1 }));

export const handlers = [
  // =============================================================================
  // Authentication
  // =============================================================================
  
  rest.post(`${API_BASE}/auth/login`, async (req, res, ctx) => {
    const body = await req.json() as { email: string; password: string };
    
    if (body.email === 'admin@crm.com' && body.password === 'password') {
      return res(
        ctx.delay(100),
        ctx.json({
          token: 'mock-jwt-token',
          refreshToken: 'mock-refresh-token',
          user: createMockUser({ id: 1, email: 'admin@crm.com', role: 'Admin' }),
        })
      );
    }
    
    return res(
      ctx.status(401),
      ctx.json({ message: 'Invalid email or password' })
    );
  }),
  
  rest.post(`${API_BASE}/auth/logout`, (_req, res, ctx) => {
    return res(ctx.json({ success: true }));
  }),
  
  rest.get(`${API_BASE}/auth/me`, (_req, res, ctx) => {
    return res(ctx.json(createMockUser({ id: 1 })));
  }),

  // =============================================================================
  // Customers (Accounts)
  // =============================================================================
  
  rest.get(`${API_BASE}/accounts`, (req, res, ctx) => {
    const url = new URL(req.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '10');
    
    const start = (page - 1) * pageSize;
    const items = mockCustomers.slice(start, start + pageSize);
    
    return res(ctx.json({
      items,
      totalCount: mockCustomers.length,
      page,
      pageSize,
    }));
  }),
  
  rest.get(`${API_BASE}/accounts/:id`, (req, res, ctx) => {
    const id = parseInt(req.params.id as string);
    const customer = mockCustomers.find(c => c.id === id);
    
    if (!customer) {
      return res(ctx.status(404), ctx.json({ message: 'Customer not found' }));
    }
    
    return res(ctx.json(customer));
  }),
  
  rest.post(`${API_BASE}/accounts`, async (req, res, ctx) => {
    const body = await req.json() as Record<string, unknown>;
    const newCustomer = createMockCustomer({ 
      id: mockCustomers.length + 1,
      ...body,
    });
    mockCustomers.push(newCustomer);
    return res(ctx.status(201), ctx.json(newCustomer));
  }),
  
  rest.put(`${API_BASE}/accounts/:id`, async (req, res, ctx) => {
    const id = parseInt(req.params.id as string);
    const body = await req.json() as Record<string, unknown>;
    const index = mockCustomers.findIndex(c => c.id === id);
    
    if (index === -1) {
      return res(ctx.status(404), ctx.json({ message: 'Customer not found' }));
    }
    
    mockCustomers[index] = { ...mockCustomers[index], ...body };
    return res(ctx.json(mockCustomers[index]));
  }),
  
  rest.delete(`${API_BASE}/accounts/:id`, (req, res, ctx) => {
    const id = parseInt(req.params.id as string);
    const index = mockCustomers.findIndex(c => c.id === id);
    
    if (index === -1) {
      return res(ctx.status(404), ctx.json({ message: 'Customer not found' }));
    }
    
    mockCustomers.splice(index, 1);
    return res(ctx.json({ success: true }));
  }),

  // =============================================================================
  // Contacts
  // =============================================================================
  
  rest.get(`${API_BASE}/contacts`, (req, res, ctx) => {
    const url = new URL(req.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '10');
    
    const start = (page - 1) * pageSize;
    const items = mockContacts.slice(start, start + pageSize);
    
    return res(ctx.json({
      items,
      totalCount: mockContacts.length,
      page,
      pageSize,
    }));
  }),
  
  rest.get(`${API_BASE}/contacts/:id`, (req, res, ctx) => {
    const id = parseInt(req.params.id as string);
    const contact = mockContacts.find(c => c.id === id);
    
    if (!contact) {
      return res(ctx.status(404), ctx.json({ message: 'Contact not found' }));
    }
    
    return res(ctx.json(contact));
  }),

  // =============================================================================
  // Leads
  // =============================================================================
  
  rest.get(`${API_BASE}/leads`, (req, res, ctx) => {
    const url = new URL(req.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '10');
    
    const start = (page - 1) * pageSize;
    const items = mockLeads.slice(start, start + pageSize);
    
    return res(ctx.json({
      items,
      totalCount: mockLeads.length,
      page,
      pageSize,
    }));
  }),

  // =============================================================================
  // Opportunities
  // =============================================================================
  
  rest.get(`${API_BASE}/opportunities`, (req, res, ctx) => {
    const url = new URL(req.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '10');
    
    const start = (page - 1) * pageSize;
    const items = mockOpportunities.slice(start, start + pageSize);
    
    return res(ctx.json({
      items,
      totalCount: mockOpportunities.length,
      page,
      pageSize,
    }));
  }),

  // =============================================================================
  // Service Requests
  // =============================================================================
  
  rest.get(`${API_BASE}/servicerequests`, (req, res, ctx) => {
    const url = new URL(req.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '10');
    
    const start = (page - 1) * pageSize;
    const items = mockServiceRequests.slice(start, start + pageSize);
    
    return res(ctx.json({
      items,
      totalCount: mockServiceRequests.length,
      page,
      pageSize,
    }));
  }),

  // =============================================================================
  // Campaigns
  // =============================================================================
  
  rest.get(`${API_BASE}/campaigns`, (req, res, ctx) => {
    const url = new URL(req.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '10');
    
    const start = (page - 1) * pageSize;
    const items = mockCampaigns.slice(start, start + pageSize);
    
    return res(ctx.json({
      items,
      totalCount: mockCampaigns.length,
      page,
      pageSize,
    }));
  }),

  // =============================================================================
  // Users
  // =============================================================================
  
  rest.get(`${API_BASE}/users`, (_req, res, ctx) => {
    return res(ctx.json({
      items: mockUsers,
      totalCount: mockUsers.length,
    }));
  }),
  
  rest.get(`${API_BASE}/users/:id`, (req, res, ctx) => {
    const id = parseInt(req.params.id as string);
    const user = mockUsers.find(u => u.id === id);
    
    if (!user) {
      return res(ctx.status(404), ctx.json({ message: 'User not found' }));
    }
    
    return res(ctx.json(user));
  }),

  // =============================================================================
  // Master Data
  // =============================================================================
  
  rest.get(`${API_BASE}/masterdata/industries`, (_req, res, ctx) => {
    return res(ctx.json([
      { id: 1, code: 'TECH', name: 'Technology' },
      { id: 2, code: 'FIN', name: 'Finance' },
      { id: 3, code: 'HEALTH', name: 'Healthcare' },
      { id: 4, code: 'RETAIL', name: 'Retail' },
      { id: 5, code: 'MFG', name: 'Manufacturing' },
    ]));
  }),
  
  rest.get(`${API_BASE}/masterdata/leadsources`, (_req, res, ctx) => {
    return res(ctx.json([
      { id: 1, code: 'WEB', name: 'Website' },
      { id: 2, code: 'REF', name: 'Referral' },
      { id: 3, code: 'TRADE', name: 'Trade Show' },
      { id: 4, code: 'COLD', name: 'Cold Call' },
      { id: 5, code: 'ADV', name: 'Advertisement' },
    ]));
  }),

  // =============================================================================
  // Field Configuration
  // =============================================================================
  
  rest.get(`${API_BASE}/fieldconfig/:module`, (_req, res, ctx) => {
    return res(ctx.json({
      moduleKey: 'accounts',
      fields: [],
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    }));
  }),

  // =============================================================================
  // User Profile
  // =============================================================================
  
  rest.get(`${API_BASE}/profile/current`, (_req, res, ctx) => {
    return res(ctx.json({
      id: 1,
      userId: 1,
      name: 'Default Profile',
      permissions: {
        canManageUsers: true,
        canViewReports: true,
        canEditSettings: true,
      },
      groupPermissions: {},
      accessiblePages: ['/dashboard', '/customers', '/contacts', '/leads'],
    }));
  }),

  // =============================================================================
  // Branding
  // =============================================================================
  
  rest.get(`${API_BASE}/branding/current`, (_req, res, ctx) => {
    return res(ctx.json({
      companyName: 'Test CRM',
      primaryColor: '#6750A4',
      logoUrl: null,
    }));
  }),
];

export default handlers;

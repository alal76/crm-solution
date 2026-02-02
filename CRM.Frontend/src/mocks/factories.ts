/**
 * Test data factories for creating mock entities.
 * Use these factories to generate consistent test data across all tests.
 * 
 * Usage:
 *   const customer = createMockCustomer({ company: 'Acme Corp' });
 *   const customers = Array.from({ length: 10 }, (_, i) => createMockCustomer({ id: i + 1 }));
 */

// =============================================================================
// Base Types
// =============================================================================

interface BaseEntity {
  id: number;
  createdAt: string;
  updatedAt: string;
}

// =============================================================================
// Customer Factory
// =============================================================================

export interface MockCustomer extends BaseEntity {
  company: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  website: string;
  industry: string;
  lifecycleStage: number;
  customerType: number;
  priority: number;
  address: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  notes: string;
  assignedToId: number | null;
  isActive: boolean;
}

let customerCounter = 0;

export function createMockCustomer(overrides: Partial<MockCustomer> = {}): MockCustomer {
  customerCounter++;
  const id = overrides.id ?? customerCounter;
  
  return {
    id,
    company: `Test Company ${id}`,
    firstName: `John`,
    lastName: `Doe${id}`,
    email: `customer${id}@test.com`,
    phone: `(555) 100-${String(id).padStart(4, '0')}`,
    website: `https://company${id}.com`,
    industry: 'Technology',
    lifecycleStage: 1,
    customerType: 1,
    priority: 2,
    address: `${id} Main Street`,
    city: 'San Francisco',
    state: 'CA',
    zipCode: '94102',
    country: 'USA',
    notes: '',
    assignedToId: 1,
    isActive: true,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    ...overrides,
  };
}

// =============================================================================
// Contact Factory
// =============================================================================

export interface MockContact extends BaseEntity {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  mobile: string;
  jobTitle: string;
  department: string;
  accountId: number | null;
  accountName: string | null;
  isPrimary: boolean;
  status: string;
  notes: string;
}

let contactCounter = 0;

export function createMockContact(overrides: Partial<MockContact> = {}): MockContact {
  contactCounter++;
  const id = overrides.id ?? contactCounter;
  
  return {
    id,
    firstName: `Jane`,
    lastName: `Smith${id}`,
    email: `contact${id}@test.com`,
    phone: `(555) 200-${String(id).padStart(4, '0')}`,
    mobile: `(555) 300-${String(id).padStart(4, '0')}`,
    jobTitle: 'Manager',
    department: 'Sales',
    accountId: id <= 5 ? id : null,
    accountName: id <= 5 ? `Test Company ${id}` : null,
    isPrimary: id % 2 === 1,
    status: 'Active',
    notes: '',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    ...overrides,
  };
}

// =============================================================================
// Lead Factory
// =============================================================================

export interface MockLead extends BaseEntity {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  company: string;
  jobTitle: string;
  source: string;
  status: string;
  score: number;
  isQualified: boolean;
  assignedToId: number | null;
  notes: string;
}

let leadCounter = 0;

export function createMockLead(overrides: Partial<MockLead> = {}): MockLead {
  leadCounter++;
  const id = overrides.id ?? leadCounter;
  
  return {
    id,
    firstName: `Lead`,
    lastName: `Person${id}`,
    email: `lead${id}@test.com`,
    phone: `(555) 400-${String(id).padStart(4, '0')}`,
    company: `Lead Company ${id}`,
    jobTitle: 'Director',
    source: 'Website',
    status: id % 3 === 0 ? 'Qualified' : id % 3 === 1 ? 'New' : 'Working',
    score: Math.min(100, id * 10),
    isQualified: id % 3 === 0,
    assignedToId: 1,
    notes: '',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    ...overrides,
  };
}

// =============================================================================
// Opportunity Factory
// =============================================================================

export interface MockOpportunity extends BaseEntity {
  name: string;
  accountId: number | null;
  accountName: string | null;
  contactId: number | null;
  contactName: string | null;
  value: number;
  currency: string;
  stage: string;
  probability: number;
  expectedCloseDate: string;
  status: string;
  source: string;
  assignedToId: number | null;
  notes: string;
}

let opportunityCounter = 0;

export function createMockOpportunity(overrides: Partial<MockOpportunity> = {}): MockOpportunity {
  opportunityCounter++;
  const id = overrides.id ?? opportunityCounter;
  
  const stages = ['Prospecting', 'Qualification', 'Proposal', 'Negotiation', 'Closed Won', 'Closed Lost'];
  const stageIndex = id % stages.length;
  
  return {
    id,
    name: `Opportunity ${id}`,
    accountId: id <= 5 ? id : null,
    accountName: id <= 5 ? `Test Company ${id}` : null,
    contactId: id <= 5 ? id : null,
    contactName: id <= 5 ? `Jane Smith${id}` : null,
    value: id * 10000,
    currency: 'USD',
    stage: stages[stageIndex],
    probability: Math.min(100, stageIndex * 20),
    expectedCloseDate: new Date(Date.now() + id * 7 * 24 * 60 * 60 * 1000).toISOString(),
    status: stageIndex < 4 ? 'Open' : stageIndex === 4 ? 'Won' : 'Lost',
    source: 'Website',
    assignedToId: 1,
    notes: '',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    ...overrides,
  };
}

// =============================================================================
// Service Request Factory
// =============================================================================

export interface MockServiceRequest extends BaseEntity {
  subject: string;
  description: string;
  accountId: number | null;
  accountName: string | null;
  contactId: number | null;
  contactName: string | null;
  status: string;
  priority: string;
  category: string;
  assignedToId: number | null;
  resolvedAt: string | null;
}

let serviceRequestCounter = 0;

export function createMockServiceRequest(overrides: Partial<MockServiceRequest> = {}): MockServiceRequest {
  serviceRequestCounter++;
  const id = overrides.id ?? serviceRequestCounter;
  
  const statuses = ['Open', 'In Progress', 'Pending', 'Resolved', 'Closed'];
  const priorities = ['Low', 'Medium', 'High', 'Urgent'];
  
  return {
    id,
    subject: `Service Request ${id}`,
    description: `Description for service request ${id}`,
    accountId: id <= 5 ? id : null,
    accountName: id <= 5 ? `Test Company ${id}` : null,
    contactId: id <= 5 ? id : null,
    contactName: id <= 5 ? `Jane Smith${id}` : null,
    status: statuses[id % statuses.length],
    priority: priorities[id % priorities.length],
    category: 'Technical Support',
    assignedToId: 1,
    resolvedAt: id % 2 === 0 ? new Date().toISOString() : null,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    ...overrides,
  };
}

// =============================================================================
// Campaign Factory
// =============================================================================

export interface MockCampaign extends BaseEntity {
  name: string;
  description: string;
  type: string;
  status: string;
  startDate: string;
  endDate: string;
  budget: number;
  actualCost: number;
  expectedRevenue: number;
  actualRevenue: number;
  ownerId: number | null;
}

let campaignCounter = 0;

export function createMockCampaign(overrides: Partial<MockCampaign> = {}): MockCampaign {
  campaignCounter++;
  const id = overrides.id ?? campaignCounter;
  
  const types = ['Email', 'Social Media', 'Webinar', 'Trade Show', 'Content'];
  const statuses = ['Draft', 'Scheduled', 'Active', 'Completed', 'Cancelled'];
  
  return {
    id,
    name: `Campaign ${id}`,
    description: `Description for campaign ${id}`,
    type: types[id % types.length],
    status: statuses[id % statuses.length],
    startDate: new Date().toISOString(),
    endDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
    budget: id * 5000,
    actualCost: id * 4000,
    expectedRevenue: id * 20000,
    actualRevenue: id * 15000,
    ownerId: 1,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    ...overrides,
  };
}

// =============================================================================
// User Factory
// =============================================================================

export interface MockUser extends BaseEntity {
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  role: string;
  isActive: boolean;
  departmentId: number | null;
  departmentName: string | null;
  photoUrl: string | null;
}

let userCounter = 0;

export function createMockUser(overrides: Partial<MockUser> = {}): MockUser {
  userCounter++;
  const id = overrides.id ?? userCounter;
  
  const roles = ['Admin', 'Manager', 'Sales Rep', 'Support Agent', 'User'];
  
  return {
    id,
    email: overrides.email ?? `user${id}@test.com`,
    firstName: `User`,
    lastName: `${id}`,
    fullName: `User ${id}`,
    role: overrides.role ?? roles[id % roles.length],
    isActive: true,
    departmentId: 1,
    departmentName: 'Sales',
    photoUrl: null,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    ...overrides,
  };
}

// =============================================================================
// Reset Functions (for test isolation)
// =============================================================================

export function resetAllFactories(): void {
  customerCounter = 0;
  contactCounter = 0;
  leadCounter = 0;
  opportunityCounter = 0;
  serviceRequestCounter = 0;
  campaignCounter = 0;
  userCounter = 0;
}

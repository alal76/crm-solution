/**
 * CRM Solution - E2E Test Data
 * 
 * All test data is clearly marked with TEST_ prefix or _TEST suffix
 * to distinguish from production data.
 */

// ============================================================================
// Test User Credentials
// ============================================================================

export const TEST_USERS = {
  admin: {
    email: 'test_admin@crm-test.local',
    password: 'TestAdmin123!@#',
    firstName: 'Test',
    lastName: 'Administrator',
    role: 'Administrator',
  },
  salesRep: {
    email: 'test_sales@crm-test.local',
    password: 'TestSales123!@#',
    firstName: 'Test',
    lastName: 'SalesRep',
    role: 'Sales Representative',
  },
  supportAgent: {
    email: 'test_support@crm-test.local',
    password: 'TestSupport123!@#',
    firstName: 'Test',
    lastName: 'SupportAgent',
    role: 'Support Agent',
  },
  viewer: {
    email: 'test_viewer@crm-test.local',
    password: 'TestViewer123!@#',
    firstName: 'Test',
    lastName: 'Viewer',
    role: 'Viewer',
  },
  // Use existing admin for quick login (from QuickLogin button)
  existingAdmin: {
    email: 'abhi.lal@gmail.com',
    username: 'abhi.lal',
    password: 'Admin@123',
    firstName: 'Abhishek',
    lastName: 'Lal',
    role: 'Administrator',
  },
};

// ============================================================================
// Test Customer Data
// ============================================================================

export const TEST_CUSTOMERS = {
  corporate: {
    name: 'TEST_Acme Corporation',
    type: 'Corporate',
    industry: 'Technology',
    email: 'test_contact@acme-test.com',
    phone: '+1-555-TEST-001',
    website: 'https://test-acme.example.com',
    address: {
      street: '123 Test Street',
      city: 'Test City',
      state: 'TC',
      postalCode: '12345',
      country: 'Test Country',
    },
    notes: 'TEST: This is a test customer for automated E2E testing',
  },
  individual: {
    name: 'TEST_John Smith',
    type: 'Individual',
    email: 'test_john.smith@test.local',
    phone: '+1-555-TEST-002',
    address: {
      street: '456 Test Avenue',
      city: 'Test Town',
      state: 'TT',
      postalCode: '67890',
      country: 'Test Country',
    },
    notes: 'TEST: Individual customer for testing',
  },
  enterprise: {
    name: 'TEST_Global Enterprises Inc',
    type: 'Enterprise',
    industry: 'Finance',
    email: 'test_enterprise@global-test.com',
    phone: '+1-555-TEST-003',
    website: 'https://test-global.example.com',
    notes: 'TEST: Enterprise customer for large account testing',
  },
};

// ============================================================================
// Test Contact Data
// ============================================================================

export const TEST_CONTACTS = {
  primary: {
    firstName: 'TEST_Jane',
    lastName: 'Doe',
    email: 'test_jane.doe@test.local',
    phone: '+1-555-TEST-100',
    mobile: '+1-555-TEST-101',
    title: 'Test Director',
    department: 'Test Department',
    notes: 'TEST: Primary contact for automated testing',
  },
  secondary: {
    firstName: 'TEST_Bob',
    lastName: 'Wilson',
    email: 'test_bob.wilson@test.local',
    phone: '+1-555-TEST-102',
    title: 'Test Manager',
    department: 'Test Operations',
    notes: 'TEST: Secondary contact for testing',
  },
  technical: {
    firstName: 'TEST_Alice',
    lastName: 'Tech',
    email: 'test_alice.tech@test.local',
    phone: '+1-555-TEST-103',
    title: 'Technical Lead',
    department: 'IT',
    notes: 'TEST: Technical contact',
  },
};

// ============================================================================
// Test Opportunity Data
// ============================================================================

export const TEST_OPPORTUNITIES = {
  newDeal: {
    name: 'TEST_New Software License Deal',
    value: 50000,
    stage: 'Prospecting',
    probability: 20,
    closeDate: '2026-06-30',
    description: 'TEST: New deal for software licensing',
    source: 'Website',
  },
  qualifiedDeal: {
    name: 'TEST_Qualified Enterprise Solution',
    value: 150000,
    stage: 'Qualification',
    probability: 40,
    closeDate: '2026-04-15',
    description: 'TEST: Qualified enterprise opportunity',
    source: 'Referral',
  },
  negotiation: {
    name: 'TEST_Contract Negotiation Deal',
    value: 75000,
    stage: 'Negotiation',
    probability: 70,
    closeDate: '2026-02-28',
    description: 'TEST: Deal in negotiation phase',
    source: 'Trade Show',
  },
  closedWon: {
    name: 'TEST_Closed Won Deal',
    value: 100000,
    stage: 'Closed Won',
    probability: 100,
    closeDate: '2026-01-15',
    description: 'TEST: Successfully closed deal',
    source: 'Direct',
  },
};

// ============================================================================
// Test Lead Data
// ============================================================================

export const TEST_LEADS = {
  hot: {
    firstName: 'TEST_Hot',
    lastName: 'Lead',
    email: 'test_hot.lead@test.local',
    phone: '+1-555-TEST-200',
    company: 'TEST_Hot Lead Company',
    title: 'CEO',
    status: 'Hot',
    source: 'Website',
    notes: 'TEST: Hot lead ready for conversion',
  },
  warm: {
    firstName: 'TEST_Warm',
    lastName: 'Prospect',
    email: 'test_warm.prospect@test.local',
    phone: '+1-555-TEST-201',
    company: 'TEST_Warm Prospect LLC',
    title: 'Director',
    status: 'Warm',
    source: 'Email Campaign',
    notes: 'TEST: Warm lead for testing',
  },
  cold: {
    firstName: 'TEST_Cold',
    lastName: 'Contact',
    email: 'test_cold.contact@test.local',
    phone: '+1-555-TEST-202',
    company: 'TEST_Cold Contact Inc',
    title: 'Manager',
    status: 'Cold',
    source: 'List Import',
    notes: 'TEST: Cold lead for nurturing tests',
  },
};

// ============================================================================
// Test Service Request Data
// ============================================================================

export const TEST_SERVICE_REQUESTS = {
  bug: {
    title: 'TEST_Bug Report - Login Issue',
    description: 'TEST: User unable to login with valid credentials',
    priority: 'High',
    category: 'Bug',
    status: 'New',
  },
  feature: {
    title: 'TEST_Feature Request - Dark Mode',
    description: 'TEST: Request for dark mode theme option',
    priority: 'Medium',
    category: 'Feature Request',
    status: 'New',
  },
  support: {
    title: 'TEST_Support Request - Password Reset',
    description: 'TEST: User needs password reset assistance',
    priority: 'Low',
    category: 'Support',
    status: 'New',
  },
  urgent: {
    title: 'TEST_Urgent - System Outage',
    description: 'TEST: Critical system outage affecting all users',
    priority: 'Critical',
    category: 'Incident',
    status: 'New',
  },
};

// ============================================================================
// Test Campaign Data
// ============================================================================

export const TEST_CAMPAIGNS = {
  emailCampaign: {
    name: 'TEST_Summer Email Campaign',
    type: 'Email',
    status: 'Draft',
    startDate: '2026-06-01',
    endDate: '2026-08-31',
    budget: 5000,
    description: 'TEST: Summer promotional email campaign',
    targetAudience: 'All Customers',
  },
  socialMedia: {
    name: 'TEST_Social Media Blitz',
    type: 'Social Media',
    status: 'Draft',
    startDate: '2026-03-01',
    endDate: '2026-03-31',
    budget: 3000,
    description: 'TEST: Social media marketing campaign',
    targetAudience: 'Prospects',
  },
  webinar: {
    name: 'TEST_Product Webinar Series',
    type: 'Webinar',
    status: 'Draft',
    startDate: '2026-04-15',
    endDate: '2026-04-15',
    budget: 1000,
    description: 'TEST: Product demonstration webinar',
    targetAudience: 'Qualified Leads',
  },
};

// ============================================================================
// Test Product Data
// ============================================================================

export const TEST_PRODUCTS = {
  software: {
    name: 'TEST_Enterprise Software License',
    sku: 'TEST-SW-001',
    price: 999.99,
    category: 'Software',
    description: 'TEST: Enterprise software license for testing',
    active: true,
  },
  service: {
    name: 'TEST_Premium Support Package',
    sku: 'TEST-SVC-001',
    price: 299.99,
    category: 'Service',
    description: 'TEST: Premium support service package',
    active: true,
  },
  hardware: {
    name: 'TEST_Hardware Device',
    sku: 'TEST-HW-001',
    price: 499.99,
    category: 'Hardware',
    description: 'TEST: Hardware device for testing',
    active: true,
  },
};

// ============================================================================
// Test Workflow Data
// ============================================================================

export const TEST_WORKFLOWS = {
  leadQualification: {
    name: 'TEST_Lead Qualification Workflow',
    description: 'TEST: Automated lead qualification process',
    triggerType: 'Lead Created',
    active: false,
  },
  customerOnboarding: {
    name: 'TEST_Customer Onboarding Workflow',
    description: 'TEST: New customer onboarding automation',
    triggerType: 'Customer Created',
    active: false,
  },
  escalation: {
    name: 'TEST_Ticket Escalation Workflow',
    description: 'TEST: Automatic ticket escalation based on SLA',
    triggerType: 'Service Request Updated',
    active: false,
  },
};

// ============================================================================
// Test Quote Data
// ============================================================================

export const TEST_QUOTES = {
  standard: {
    name: 'TEST_Standard Quote',
    status: 'Draft',
    validUntil: '2026-03-31',
    discount: 10,
    notes: 'TEST: Standard quote for testing',
  },
  enterprise: {
    name: 'TEST_Enterprise Quote',
    status: 'Draft',
    validUntil: '2026-04-30',
    discount: 20,
    notes: 'TEST: Enterprise quote with volume discount',
  },
};

// ============================================================================
// Utility Functions
// ============================================================================

/**
 * Generate unique test data with timestamp suffix
 */
export function uniqueTestData<T extends Record<string, any>>(data: T): T {
  const timestamp = Date.now();
  const result = { ...data };
  
  // Add timestamp to string fields that should be unique
  for (const key of Object.keys(result)) {
    if (typeof result[key] === 'string' && key !== 'password') {
      if (key === 'email') {
        const [localPart, domain] = result[key].split('@');
        result[key] = `${localPart}_${timestamp}@${domain}`;
      } else if (key === 'name' || key === 'firstName' || key === 'title') {
        result[key] = `${result[key]}_${timestamp}`;
      }
    }
  }
  
  return result;
}

/**
 * Check if data is test data (contains TEST_ prefix)
 */
export function isTestData(value: string): boolean {
  return value.includes('TEST_') || value.includes('_TEST') || value.includes('test_');
}

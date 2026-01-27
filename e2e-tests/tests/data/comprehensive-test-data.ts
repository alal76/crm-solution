/**
 * CRM Solution - Comprehensive Test Data
 * 
 * Extensive test users, user groups, and related business entities
 * All data is prefixed with TEST_ for easy identification and cleanup
 */

import { randomUUID } from 'crypto';

// ============================================================================
// Helper Functions
// ============================================================================

export function generateId(): string {
  return randomUUID().substring(0, 8);
}

export function randomString(length: number = 8): string {
  return Math.random().toString(36).substring(2, 2 + length);
}

export function randomEmail(prefix: string): string {
  return `${prefix}_${randomString(6)}@test.crm.local`;
}

export function randomPhone(): string {
  return `+1${Math.floor(Math.random() * 9000000000 + 1000000000)}`;
}

export function randomAmount(min: number = 1000, max: number = 100000): number {
  return Math.floor(Math.random() * (max - min) + min);
}

export function futureDate(daysFromNow: number): string {
  const date = new Date();
  date.setDate(date.getDate() + daysFromNow);
  return date.toISOString().split('T')[0];
}

export function pastDate(daysAgo: number): string {
  const date = new Date();
  date.setDate(date.getDate() - daysAgo);
  return date.toISOString().split('T')[0];
}

// ============================================================================
// TEST USERS - Comprehensive User Set
// ============================================================================

export const COMPREHENSIVE_TEST_USERS = {
  // Admin Users
  testAdmin1: {
    id: 'TEST_ADMIN_001',
    username: 'TEST_admin_primary',
    email: 'test_admin_primary@test.crm.local',
    password: 'TestAdmin123!@#',
    firstName: 'TEST_Primary',
    lastName: 'Administrator',
    role: 'Administrator',
    department: 'IT',
    isActive: true,
  },
  testAdmin2: {
    id: 'TEST_ADMIN_002',
    username: 'TEST_admin_secondary',
    email: 'test_admin_secondary@test.crm.local',
    password: 'TestAdmin123!@#',
    firstName: 'TEST_Secondary',
    lastName: 'Administrator',
    role: 'Administrator',
    department: 'IT',
    isActive: true,
  },
  
  // Sales Users
  salesManager: {
    id: 'TEST_SALES_MGR_001',
    username: 'TEST_sales_manager',
    email: 'test_sales_manager@test.crm.local',
    password: 'TestSales123!@#',
    firstName: 'TEST_Sales',
    lastName: 'Manager',
    role: 'Sales Manager',
    department: 'Sales',
    isActive: true,
  },
  salesRep1: {
    id: 'TEST_SALES_REP_001',
    username: 'TEST_sales_rep_1',
    email: 'test_sales_rep_1@test.crm.local',
    password: 'TestSales123!@#',
    firstName: 'TEST_Sales_Rep',
    lastName: 'One',
    role: 'Sales Representative',
    department: 'Sales',
    isActive: true,
  },
  salesRep2: {
    id: 'TEST_SALES_REP_002',
    username: 'TEST_sales_rep_2',
    email: 'test_sales_rep_2@test.crm.local',
    password: 'TestSales123!@#',
    firstName: 'TEST_Sales_Rep',
    lastName: 'Two',
    role: 'Sales Representative',
    department: 'Sales',
    isActive: true,
  },
  salesRep3: {
    id: 'TEST_SALES_REP_003',
    username: 'TEST_sales_rep_3',
    email: 'test_sales_rep_3@test.crm.local',
    password: 'TestSales123!@#',
    firstName: 'TEST_Sales_Rep',
    lastName: 'Three',
    role: 'Sales Representative',
    department: 'Sales',
    isActive: false, // Inactive user for testing
  },

  // Support Users
  supportManager: {
    id: 'TEST_SUPPORT_MGR_001',
    username: 'TEST_support_manager',
    email: 'test_support_manager@test.crm.local',
    password: 'TestSupport123!@#',
    firstName: 'TEST_Support',
    lastName: 'Manager',
    role: 'Support Manager',
    department: 'Support',
    isActive: true,
  },
  supportAgent1: {
    id: 'TEST_SUPPORT_001',
    username: 'TEST_support_agent_1',
    email: 'test_support_agent_1@test.crm.local',
    password: 'TestSupport123!@#',
    firstName: 'TEST_Support_Agent',
    lastName: 'One',
    role: 'Support Agent',
    department: 'Support',
    isActive: true,
  },
  supportAgent2: {
    id: 'TEST_SUPPORT_002',
    username: 'TEST_support_agent_2',
    email: 'test_support_agent_2@test.crm.local',
    password: 'TestSupport123!@#',
    firstName: 'TEST_Support_Agent',
    lastName: 'Two',
    role: 'Support Agent',
    department: 'Support',
    isActive: true,
  },

  // Marketing Users
  marketingManager: {
    id: 'TEST_MKTG_MGR_001',
    username: 'TEST_marketing_manager',
    email: 'test_marketing_manager@test.crm.local',
    password: 'TestMarketing123!@#',
    firstName: 'TEST_Marketing',
    lastName: 'Manager',
    role: 'Marketing Manager',
    department: 'Marketing',
    isActive: true,
  },
  marketingSpec: {
    id: 'TEST_MKTG_001',
    username: 'TEST_marketing_spec',
    email: 'test_marketing_spec@test.crm.local',
    password: 'TestMarketing123!@#',
    firstName: 'TEST_Marketing',
    lastName: 'Specialist',
    role: 'Marketing Specialist',
    department: 'Marketing',
    isActive: true,
  },

  // Viewer/Read-only Users
  viewer1: {
    id: 'TEST_VIEWER_001',
    username: 'TEST_viewer_1',
    email: 'test_viewer_1@test.crm.local',
    password: 'TestViewer123!@#',
    firstName: 'TEST_Viewer',
    lastName: 'One',
    role: 'Viewer',
    department: 'General',
    isActive: true,
  },
  viewer2: {
    id: 'TEST_VIEWER_002',
    username: 'TEST_viewer_2',
    email: 'test_viewer_2@test.crm.local',
    password: 'TestViewer123!@#',
    firstName: 'TEST_Viewer',
    lastName: 'Two',
    role: 'Viewer',
    department: 'General',
    isActive: true,
  },
};

// ============================================================================
// TEST USER GROUPS
// ============================================================================

export const COMPREHENSIVE_TEST_USER_GROUPS = {
  salesTeam: {
    id: 'TEST_GROUP_SALES',
    name: 'TEST_Sales_Team',
    description: 'Test Sales Team Group',
    members: ['TEST_SALES_MGR_001', 'TEST_SALES_REP_001', 'TEST_SALES_REP_002', 'TEST_SALES_REP_003'],
    permissions: ['customers.read', 'customers.write', 'opportunities.read', 'opportunities.write', 'leads.read', 'leads.write'],
  },
  supportTeam: {
    id: 'TEST_GROUP_SUPPORT',
    name: 'TEST_Support_Team',
    description: 'Test Support Team Group',
    members: ['TEST_SUPPORT_MGR_001', 'TEST_SUPPORT_001', 'TEST_SUPPORT_002'],
    permissions: ['customers.read', 'service-requests.read', 'service-requests.write'],
  },
  marketingTeam: {
    id: 'TEST_GROUP_MARKETING',
    name: 'TEST_Marketing_Team',
    description: 'Test Marketing Team Group',
    members: ['TEST_MKTG_MGR_001', 'TEST_MKTG_001'],
    permissions: ['campaigns.read', 'campaigns.write', 'leads.read', 'contacts.read'],
  },
  executives: {
    id: 'TEST_GROUP_EXEC',
    name: 'TEST_Executives',
    description: 'Test Executive Team Group',
    members: ['TEST_ADMIN_001'],
    permissions: ['all'],
  },
  allViewers: {
    id: 'TEST_GROUP_VIEWERS',
    name: 'TEST_All_Viewers',
    description: 'Test Read-only Users Group',
    members: ['TEST_VIEWER_001', 'TEST_VIEWER_002'],
    permissions: ['customers.read', 'contacts.read', 'opportunities.read', 'service-requests.read'],
  },
};

// ============================================================================
// TEST CUSTOMERS - Comprehensive Set
// ============================================================================

export const COMPREHENSIVE_TEST_CUSTOMERS = {
  // Enterprise Customers
  enterprise1: {
    id: 'TEST_CUST_ENT_001',
    name: 'TEST_GlobalTech_Enterprise',
    company: 'TEST GlobalTech Enterprise Inc',
    customerType: 'Corporate',
    lifecycleStage: 'Customer',
    priority: 'High',
    industry: 'Technology',
    annualRevenue: 5000000,
    employeeCount: 500,
    email: 'contact@test-globaltech.local',
    phone: '+1-555-100-0001',
    website: 'https://test-globaltech.local',
    address: {
      street: '100 TEST Enterprise Way',
      city: 'San Francisco',
      state: 'CA',
      zip: '94102',
      country: 'USA',
    },
    assignedTo: 'TEST_SALES_REP_001',
    tags: ['enterprise', 'technology', 'key-account'],
  },
  enterprise2: {
    id: 'TEST_CUST_ENT_002',
    name: 'TEST_MegaCorp_Industries',
    company: 'TEST MegaCorp Industries LLC',
    customerType: 'Corporate',
    lifecycleStage: 'Customer',
    priority: 'High',
    industry: 'Manufacturing',
    annualRevenue: 10000000,
    employeeCount: 1000,
    email: 'sales@test-megacorp.local',
    phone: '+1-555-100-0002',
    website: 'https://test-megacorp.local',
    address: {
      street: '200 TEST Industrial Blvd',
      city: 'Detroit',
      state: 'MI',
      zip: '48201',
      country: 'USA',
    },
    assignedTo: 'TEST_SALES_MGR_001',
    tags: ['enterprise', 'manufacturing', 'strategic'],
  },

  // Mid-Market Customers
  midMarket1: {
    id: 'TEST_CUST_MID_001',
    name: 'TEST_TechStart_Solutions',
    company: 'TEST TechStart Solutions Inc',
    customerType: 'Corporate',
    lifecycleStage: 'Customer',
    priority: 'Medium',
    industry: 'Software',
    annualRevenue: 1000000,
    employeeCount: 50,
    email: 'info@test-techstart.local',
    phone: '+1-555-200-0001',
    website: 'https://test-techstart.local',
    address: {
      street: '50 TEST Startup Lane',
      city: 'Austin',
      state: 'TX',
      zip: '78701',
      country: 'USA',
    },
    assignedTo: 'TEST_SALES_REP_001',
    tags: ['mid-market', 'software', 'growing'],
  },
  midMarket2: {
    id: 'TEST_CUST_MID_002',
    name: 'TEST_HealthCare_Partners',
    company: 'TEST HealthCare Partners Group',
    customerType: 'Corporate',
    lifecycleStage: 'Customer',
    priority: 'Medium',
    industry: 'Healthcare',
    annualRevenue: 2000000,
    employeeCount: 100,
    email: 'admin@test-healthcare.local',
    phone: '+1-555-200-0002',
    website: 'https://test-healthcare.local',
    address: {
      street: '75 TEST Medical Center Dr',
      city: 'Boston',
      state: 'MA',
      zip: '02108',
      country: 'USA',
    },
    assignedTo: 'TEST_SALES_REP_002',
    tags: ['mid-market', 'healthcare', 'regulated'],
  },

  // Small Business Customers
  smallBiz1: {
    id: 'TEST_CUST_SMB_001',
    name: 'TEST_LocalShop_Retail',
    company: 'TEST LocalShop Retail',
    customerType: 'Corporate',
    lifecycleStage: 'Customer',
    priority: 'Low',
    industry: 'Retail',
    annualRevenue: 250000,
    employeeCount: 10,
    email: 'owner@test-localshop.local',
    phone: '+1-555-300-0001',
    website: 'https://test-localshop.local',
    address: {
      street: '10 TEST Main Street',
      city: 'Portland',
      state: 'OR',
      zip: '97201',
      country: 'USA',
    },
    assignedTo: 'TEST_SALES_REP_002',
    tags: ['smb', 'retail', 'local'],
  },
  smallBiz2: {
    id: 'TEST_CUST_SMB_002',
    name: 'TEST_QuickService_Consulting',
    company: 'TEST QuickService Consulting',
    customerType: 'Corporate',
    lifecycleStage: 'Lead',
    priority: 'Medium',
    industry: 'Consulting',
    annualRevenue: 500000,
    employeeCount: 15,
    email: 'hello@test-quickservice.local',
    phone: '+1-555-300-0002',
    website: 'https://test-quickservice.local',
    address: {
      street: '25 TEST Business Park',
      city: 'Denver',
      state: 'CO',
      zip: '80202',
      country: 'USA',
    },
    assignedTo: 'TEST_SALES_REP_001',
    tags: ['smb', 'consulting', 'prospect'],
  },

  // Individual Customers
  individual1: {
    id: 'TEST_CUST_IND_001',
    firstName: 'TEST_John',
    lastName: 'Doe_Individual',
    name: 'TEST_John_Doe_Individual',
    customerType: 'Individual',
    lifecycleStage: 'Customer',
    priority: 'Low',
    email: 'test_john.doe@personal.local',
    phone: '+1-555-400-0001',
    address: {
      street: '123 TEST Home Street',
      city: 'Seattle',
      state: 'WA',
      zip: '98101',
      country: 'USA',
    },
    assignedTo: 'TEST_SALES_REP_002',
    tags: ['individual', 'residential'],
  },
  individual2: {
    id: 'TEST_CUST_IND_002',
    firstName: 'TEST_Jane',
    lastName: 'Smith_Individual',
    name: 'TEST_Jane_Smith_Individual',
    customerType: 'Individual',
    lifecycleStage: 'Lead',
    priority: 'Medium',
    email: 'test_jane.smith@personal.local',
    phone: '+1-555-400-0002',
    address: {
      street: '456 TEST Apartment Blvd',
      city: 'Chicago',
      state: 'IL',
      zip: '60601',
      country: 'USA',
    },
    assignedTo: 'TEST_SALES_REP_001',
    tags: ['individual', 'prospect'],
  },

  // Churned/Inactive Customers
  churned1: {
    id: 'TEST_CUST_CHURNED_001',
    name: 'TEST_OldClient_Corp',
    company: 'TEST OldClient Corporation',
    customerType: 'Corporate',
    lifecycleStage: 'Churned',
    priority: 'Low',
    industry: 'Finance',
    annualRevenue: 750000,
    email: 'former@test-oldclient.local',
    phone: '+1-555-500-0001',
    address: {
      street: '999 TEST Former Client Rd',
      city: 'New York',
      state: 'NY',
      zip: '10001',
      country: 'USA',
    },
    assignedTo: 'TEST_SALES_MGR_001',
    tags: ['churned', 'win-back-target'],
    churnReason: 'Pricing concerns',
    churnDate: pastDate(90),
  },
};

// ============================================================================
// TEST CONTACTS - Related to Customers
// ============================================================================

export const COMPREHENSIVE_TEST_CONTACTS = {
  // GlobalTech Enterprise Contacts
  globalTechCEO: {
    id: 'TEST_CONT_001',
    firstName: 'TEST_Michael',
    lastName: 'GlobalTech_CEO',
    email: 'test_michael.ceo@test-globaltech.local',
    phone: '+1-555-101-0001',
    title: 'Chief Executive Officer',
    customerId: 'TEST_CUST_ENT_001',
    isPrimary: true,
    isDecisionMaker: true,
    department: 'Executive',
  },
  globalTechCTO: {
    id: 'TEST_CONT_002',
    firstName: 'TEST_Sarah',
    lastName: 'GlobalTech_CTO',
    email: 'test_sarah.cto@test-globaltech.local',
    phone: '+1-555-101-0002',
    title: 'Chief Technology Officer',
    customerId: 'TEST_CUST_ENT_001',
    isPrimary: false,
    isDecisionMaker: true,
    department: 'Technology',
  },
  globalTechProcurement: {
    id: 'TEST_CONT_003',
    firstName: 'TEST_David',
    lastName: 'GlobalTech_Procurement',
    email: 'test_david.proc@test-globaltech.local',
    phone: '+1-555-101-0003',
    title: 'Procurement Manager',
    customerId: 'TEST_CUST_ENT_001',
    isPrimary: false,
    isDecisionMaker: false,
    department: 'Procurement',
  },

  // MegaCorp Contacts
  megaCorpCFO: {
    id: 'TEST_CONT_004',
    firstName: 'TEST_Robert',
    lastName: 'MegaCorp_CFO',
    email: 'test_robert.cfo@test-megacorp.local',
    phone: '+1-555-102-0001',
    title: 'Chief Financial Officer',
    customerId: 'TEST_CUST_ENT_002',
    isPrimary: true,
    isDecisionMaker: true,
    department: 'Finance',
  },
  megaCorpOps: {
    id: 'TEST_CONT_005',
    firstName: 'TEST_Lisa',
    lastName: 'MegaCorp_Operations',
    email: 'test_lisa.ops@test-megacorp.local',
    phone: '+1-555-102-0002',
    title: 'VP of Operations',
    customerId: 'TEST_CUST_ENT_002',
    isPrimary: false,
    isDecisionMaker: true,
    department: 'Operations',
  },

  // TechStart Solutions Contacts
  techStartFounder: {
    id: 'TEST_CONT_006',
    firstName: 'TEST_Alex',
    lastName: 'TechStart_Founder',
    email: 'test_alex.founder@test-techstart.local',
    phone: '+1-555-201-0001',
    title: 'Founder & CEO',
    customerId: 'TEST_CUST_MID_001',
    isPrimary: true,
    isDecisionMaker: true,
    department: 'Executive',
  },

  // HealthCare Partners Contacts
  healthCareCMO: {
    id: 'TEST_CONT_007',
    firstName: 'TEST_Dr_Emily',
    lastName: 'HealthCare_CMO',
    email: 'test_emily.cmo@test-healthcare.local',
    phone: '+1-555-202-0001',
    title: 'Chief Medical Officer',
    customerId: 'TEST_CUST_MID_002',
    isPrimary: true,
    isDecisionMaker: true,
    department: 'Medical',
  },
  healthCareIT: {
    id: 'TEST_CONT_008',
    firstName: 'TEST_James',
    lastName: 'HealthCare_IT',
    email: 'test_james.it@test-healthcare.local',
    phone: '+1-555-202-0002',
    title: 'IT Director',
    customerId: 'TEST_CUST_MID_002',
    isPrimary: false,
    isDecisionMaker: false,
    department: 'IT',
  },

  // Standalone Contacts (no customer yet)
  standaloneProspect1: {
    id: 'TEST_CONT_009',
    firstName: 'TEST_Standalone',
    lastName: 'Prospect_One',
    email: 'test_standalone1@prospect.local',
    phone: '+1-555-600-0001',
    title: 'Business Owner',
    customerId: null,
    isPrimary: false,
    isDecisionMaker: true,
    company: 'TEST Unknown Prospect Co',
  },
  standaloneProspect2: {
    id: 'TEST_CONT_010',
    firstName: 'TEST_Standalone',
    lastName: 'Prospect_Two',
    email: 'test_standalone2@prospect.local',
    phone: '+1-555-600-0002',
    title: 'Director',
    customerId: null,
    isPrimary: false,
    isDecisionMaker: true,
    company: 'TEST Another Prospect Inc',
  },
};

// ============================================================================
// TEST OPPORTUNITIES - Sales Pipeline
// ============================================================================

export const COMPREHENSIVE_TEST_OPPORTUNITIES = {
  // High Value Opportunities
  enterpriseDeal1: {
    id: 'TEST_OPP_001',
    name: 'TEST_GlobalTech_Enterprise_License',
    customerId: 'TEST_CUST_ENT_001',
    contactId: 'TEST_CONT_001',
    value: 500000,
    stage: 'Negotiation',
    probability: 75,
    expectedCloseDate: futureDate(30),
    description: 'TEST: Enterprise license deal for GlobalTech',
    assignedTo: 'TEST_SALES_REP_001',
    source: 'Referral',
    nextStep: 'Contract review meeting',
    competitors: ['Competitor A', 'Competitor B'],
  },
  enterpriseDeal2: {
    id: 'TEST_OPP_002',
    name: 'TEST_MegaCorp_Implementation',
    customerId: 'TEST_CUST_ENT_002',
    contactId: 'TEST_CONT_004',
    value: 750000,
    stage: 'Proposal',
    probability: 50,
    expectedCloseDate: futureDate(60),
    description: 'TEST: Full implementation project for MegaCorp',
    assignedTo: 'TEST_SALES_MGR_001',
    source: 'Trade Show',
    nextStep: 'Technical demo',
    competitors: ['Competitor C'],
  },

  // Mid-Value Opportunities
  midMarketDeal1: {
    id: 'TEST_OPP_003',
    name: 'TEST_TechStart_Starter_Package',
    customerId: 'TEST_CUST_MID_001',
    contactId: 'TEST_CONT_006',
    value: 75000,
    stage: 'Qualification',
    probability: 30,
    expectedCloseDate: futureDate(45),
    description: 'TEST: Starter package for TechStart',
    assignedTo: 'TEST_SALES_REP_001',
    source: 'Website',
    nextStep: 'Discovery call',
    competitors: [],
  },
  midMarketDeal2: {
    id: 'TEST_OPP_004',
    name: 'TEST_HealthCare_Compliance_Module',
    customerId: 'TEST_CUST_MID_002',
    contactId: 'TEST_CONT_007',
    value: 150000,
    stage: 'Demo',
    probability: 60,
    expectedCloseDate: futureDate(30),
    description: 'TEST: Healthcare compliance module',
    assignedTo: 'TEST_SALES_REP_002',
    source: 'Partner Referral',
    nextStep: 'Security assessment',
    competitors: ['HealthTech Inc'],
  },

  // Small Opportunities
  smallDeal1: {
    id: 'TEST_OPP_005',
    name: 'TEST_LocalShop_Basic_Plan',
    customerId: 'TEST_CUST_SMB_001',
    contactId: null,
    value: 5000,
    stage: 'Closed Won',
    probability: 100,
    expectedCloseDate: pastDate(7),
    closedDate: pastDate(7),
    description: 'TEST: Basic plan for LocalShop - CLOSED WON',
    assignedTo: 'TEST_SALES_REP_002',
    source: 'Cold Call',
    wonReason: 'Price and ease of use',
  },
  smallDeal2: {
    id: 'TEST_OPP_006',
    name: 'TEST_QuickService_Consulting_Package',
    customerId: 'TEST_CUST_SMB_002',
    contactId: null,
    value: 25000,
    stage: 'Prospecting',
    probability: 10,
    expectedCloseDate: futureDate(90),
    description: 'TEST: Consulting package opportunity',
    assignedTo: 'TEST_SALES_REP_001',
    source: 'LinkedIn',
    nextStep: 'Initial outreach',
  },

  // Lost/Stalled Opportunities
  lostDeal: {
    id: 'TEST_OPP_007',
    name: 'TEST_OldClient_Renewal_LOST',
    customerId: 'TEST_CUST_CHURNED_001',
    contactId: null,
    value: 100000,
    stage: 'Closed Lost',
    probability: 0,
    expectedCloseDate: pastDate(30),
    closedDate: pastDate(30),
    description: 'TEST: Renewal that was lost to competitor',
    assignedTo: 'TEST_SALES_MGR_001',
    source: 'Renewal',
    lostReason: 'Competitor offered lower price',
    competitor: 'Budget Solutions Inc',
  },
  stalledDeal: {
    id: 'TEST_OPP_008',
    name: 'TEST_Stalled_Enterprise_Deal',
    customerId: 'TEST_CUST_ENT_001',
    contactId: 'TEST_CONT_002',
    value: 200000,
    stage: 'On Hold',
    probability: 25,
    expectedCloseDate: futureDate(120),
    description: 'TEST: Deal stalled due to budget freeze',
    assignedTo: 'TEST_SALES_REP_001',
    source: 'Referral',
    nextStep: 'Follow up Q2',
    stallReason: 'Customer budget freeze',
  },
};

// ============================================================================
// TEST LEADS - Pre-qualification
// ============================================================================

export const COMPREHENSIVE_TEST_LEADS = {
  hotLead1: {
    id: 'TEST_LEAD_001',
    firstName: 'TEST_Hot',
    lastName: 'Lead_One',
    email: 'test_hot_lead1@prospect.local',
    phone: '+1-555-700-0001',
    company: 'TEST Hot Lead Company',
    title: 'VP of Engineering',
    source: 'Website Demo Request',
    status: 'Hot',
    score: 85,
    assignedTo: 'TEST_SALES_REP_001',
    notes: 'TEST: Requested urgent demo, high budget',
    industry: 'Technology',
    estimatedValue: 100000,
  },
  hotLead2: {
    id: 'TEST_LEAD_002',
    firstName: 'TEST_Hot',
    lastName: 'Lead_Two',
    email: 'test_hot_lead2@prospect.local',
    phone: '+1-555-700-0002',
    company: 'TEST Enterprise Prospect',
    title: 'CIO',
    source: 'Trade Show',
    status: 'Hot',
    score: 90,
    assignedTo: 'TEST_SALES_MGR_001',
    notes: 'TEST: Met at conference, actively evaluating',
    industry: 'Finance',
    estimatedValue: 250000,
  },
  warmLead1: {
    id: 'TEST_LEAD_003',
    firstName: 'TEST_Warm',
    lastName: 'Lead_One',
    email: 'test_warm_lead1@prospect.local',
    phone: '+1-555-700-0003',
    company: 'TEST Warm Lead Inc',
    title: 'IT Manager',
    source: 'Webinar',
    status: 'Warm',
    score: 60,
    assignedTo: 'TEST_SALES_REP_002',
    notes: 'TEST: Downloaded whitepaper, engaged on webinar',
    industry: 'Healthcare',
    estimatedValue: 50000,
  },
  warmLead2: {
    id: 'TEST_LEAD_004',
    firstName: 'TEST_Warm',
    lastName: 'Lead_Two',
    email: 'test_warm_lead2@prospect.local',
    phone: '+1-555-700-0004',
    company: 'TEST Growing Startup',
    title: 'Founder',
    source: 'Referral',
    status: 'Warm',
    score: 55,
    assignedTo: 'TEST_SALES_REP_001',
    notes: 'TEST: Referred by existing customer',
    industry: 'Software',
    estimatedValue: 30000,
  },
  coldLead1: {
    id: 'TEST_LEAD_005',
    firstName: 'TEST_Cold',
    lastName: 'Lead_One',
    email: 'test_cold_lead1@prospect.local',
    phone: '+1-555-700-0005',
    company: 'TEST Cold Lead Corp',
    title: 'Business Analyst',
    source: 'Cold Email',
    status: 'Cold',
    score: 20,
    assignedTo: 'TEST_SALES_REP_002',
    notes: 'TEST: Initial outreach, no response yet',
    industry: 'Consulting',
    estimatedValue: 15000,
  },
  qualifiedLead: {
    id: 'TEST_LEAD_006',
    firstName: 'TEST_Qualified',
    lastName: 'Lead',
    email: 'test_qualified_lead@prospect.local',
    phone: '+1-555-700-0006',
    company: 'TEST Qualified Prospect LLC',
    title: 'Director of Operations',
    source: 'Partner',
    status: 'Qualified',
    score: 75,
    assignedTo: 'TEST_SALES_REP_001',
    notes: 'TEST: Qualified, ready for conversion to opportunity',
    industry: 'Manufacturing',
    estimatedValue: 80000,
  },
  disqualifiedLead: {
    id: 'TEST_LEAD_007',
    firstName: 'TEST_Disqualified',
    lastName: 'Lead',
    email: 'test_disq_lead@prospect.local',
    phone: '+1-555-700-0007',
    company: 'TEST Too Small Co',
    title: 'Owner',
    source: 'Website',
    status: 'Disqualified',
    score: 10,
    assignedTo: 'TEST_SALES_REP_002',
    notes: 'TEST: Disqualified - below minimum company size',
    industry: 'Retail',
    estimatedValue: 1000,
    disqualifyReason: 'Company too small',
  },
  convertedLead: {
    id: 'TEST_LEAD_008',
    firstName: 'TEST_Converted',
    lastName: 'Lead',
    email: 'test_converted_lead@prospect.local',
    phone: '+1-555-700-0008',
    company: 'TEST Converted Prospect Inc',
    title: 'CEO',
    source: 'Event',
    status: 'Converted',
    score: 95,
    assignedTo: 'TEST_SALES_MGR_001',
    notes: 'TEST: Converted to opportunity and customer',
    industry: 'Technology',
    estimatedValue: 120000,
    convertedToCustomerId: 'TEST_CUST_SMB_002',
    convertedToOpportunityId: 'TEST_OPP_006',
    convertedDate: pastDate(14),
  },
};

// ============================================================================
// TEST SERVICE REQUESTS
// ============================================================================

export const COMPREHENSIVE_TEST_SERVICE_REQUESTS = {
  criticalTicket: {
    id: 'TEST_SR_001',
    title: 'TEST_Critical_System_Outage',
    description: 'TEST: Critical system outage affecting all users',
    customerId: 'TEST_CUST_ENT_001',
    contactId: 'TEST_CONT_002',
    priority: 'Critical',
    status: 'In Progress',
    category: 'Technical',
    subcategory: 'System Outage',
    assignedTo: 'TEST_SUPPORT_MGR_001',
    slaTarget: new Date(Date.now() + 4 * 60 * 60 * 1000).toISOString(), // 4 hours
    createdDate: new Date().toISOString(),
  },
  highPriorityTicket: {
    id: 'TEST_SR_002',
    title: 'TEST_Performance_Degradation',
    description: 'TEST: System running slowly, affecting productivity',
    customerId: 'TEST_CUST_ENT_002',
    contactId: 'TEST_CONT_005',
    priority: 'High',
    status: 'Open',
    category: 'Technical',
    subcategory: 'Performance',
    assignedTo: 'TEST_SUPPORT_001',
    slaTarget: new Date(Date.now() + 8 * 60 * 60 * 1000).toISOString(), // 8 hours
    createdDate: new Date().toISOString(),
  },
  mediumTicket1: {
    id: 'TEST_SR_003',
    title: 'TEST_Feature_Request',
    description: 'TEST: Request for new reporting feature',
    customerId: 'TEST_CUST_MID_001',
    contactId: 'TEST_CONT_006',
    priority: 'Medium',
    status: 'Open',
    category: 'Feature Request',
    subcategory: 'Reporting',
    assignedTo: 'TEST_SUPPORT_002',
    slaTarget: new Date(Date.now() + 72 * 60 * 60 * 1000).toISOString(), // 72 hours
    createdDate: pastDate(2),
  },
  mediumTicket2: {
    id: 'TEST_SR_004',
    title: 'TEST_Integration_Help',
    description: 'TEST: Need help setting up API integration',
    customerId: 'TEST_CUST_MID_002',
    contactId: 'TEST_CONT_008',
    priority: 'Medium',
    status: 'Waiting on Customer',
    category: 'Technical',
    subcategory: 'Integration',
    assignedTo: 'TEST_SUPPORT_001',
    slaTarget: new Date(Date.now() + 48 * 60 * 60 * 1000).toISOString(),
    createdDate: pastDate(3),
    lastResponseDate: pastDate(1),
  },
  lowTicket1: {
    id: 'TEST_SR_005',
    title: 'TEST_Training_Request',
    description: 'TEST: Request for user training session',
    customerId: 'TEST_CUST_SMB_001',
    contactId: null,
    priority: 'Low',
    status: 'Open',
    category: 'Training',
    subcategory: 'User Training',
    assignedTo: 'TEST_SUPPORT_002',
    createdDate: pastDate(5),
  },
  resolvedTicket: {
    id: 'TEST_SR_006',
    title: 'TEST_Resolved_Login_Issue',
    description: 'TEST: User unable to login - RESOLVED',
    customerId: 'TEST_CUST_ENT_001',
    contactId: 'TEST_CONT_003',
    priority: 'High',
    status: 'Resolved',
    category: 'Technical',
    subcategory: 'Authentication',
    assignedTo: 'TEST_SUPPORT_001',
    createdDate: pastDate(7),
    resolvedDate: pastDate(6),
    resolution: 'Password reset performed successfully',
    satisfactionRating: 5,
  },
  closedTicket: {
    id: 'TEST_SR_007',
    title: 'TEST_Closed_Billing_Inquiry',
    description: 'TEST: Billing question about invoice - CLOSED',
    customerId: 'TEST_CUST_MID_001',
    contactId: 'TEST_CONT_006',
    priority: 'Low',
    status: 'Closed',
    category: 'Billing',
    subcategory: 'Invoice',
    assignedTo: 'TEST_SUPPORT_002',
    createdDate: pastDate(14),
    resolvedDate: pastDate(12),
    closedDate: pastDate(10),
    resolution: 'Invoice clarification provided',
    satisfactionRating: 4,
  },
  escalatedTicket: {
    id: 'TEST_SR_008',
    title: 'TEST_Escalated_Data_Issue',
    description: 'TEST: Data corruption issue requiring escalation',
    customerId: 'TEST_CUST_ENT_002',
    contactId: 'TEST_CONT_004',
    priority: 'Critical',
    status: 'Escalated',
    category: 'Technical',
    subcategory: 'Data',
    assignedTo: 'TEST_SUPPORT_MGR_001',
    escalatedFrom: 'TEST_SUPPORT_001',
    escalationReason: 'Requires database admin access',
    createdDate: pastDate(1),
    escalatedDate: new Date().toISOString(),
  },
};

// ============================================================================
// TEST CAMPAIGNS
// ============================================================================

export const COMPREHENSIVE_TEST_CAMPAIGNS = {
  emailCampaignActive: {
    id: 'TEST_CAMP_001',
    name: 'TEST_Q1_Product_Launch_Email',
    type: 'Email',
    status: 'Active',
    startDate: pastDate(7),
    endDate: futureDate(23),
    budget: 5000,
    actualCost: 2500,
    targetAudience: 'Enterprise Customers',
    description: 'TEST: Q1 product launch email campaign',
    goals: ['Generate 100 leads', 'Increase awareness'],
    metrics: {
      sent: 5000,
      delivered: 4800,
      opened: 1200,
      clicked: 300,
      converted: 25,
    },
    assignedTo: 'TEST_MKTG_MGR_001',
  },
  emailCampaignScheduled: {
    id: 'TEST_CAMP_002',
    name: 'TEST_Spring_Newsletter',
    type: 'Email',
    status: 'Scheduled',
    startDate: futureDate(14),
    endDate: futureDate(14),
    budget: 1000,
    targetAudience: 'All Customers',
    description: 'TEST: Spring newsletter campaign',
    goals: ['Customer engagement', 'Feature announcements'],
    assignedTo: 'TEST_MKTG_001',
  },
  socialCampaignActive: {
    id: 'TEST_CAMP_003',
    name: 'TEST_LinkedIn_Thought_Leadership',
    type: 'Social Media',
    status: 'Active',
    startDate: pastDate(30),
    endDate: futureDate(60),
    budget: 10000,
    actualCost: 4000,
    targetAudience: 'B2B Decision Makers',
    description: 'TEST: LinkedIn thought leadership campaign',
    goals: ['Brand awareness', '500 new followers'],
    metrics: {
      impressions: 50000,
      engagements: 2500,
      followers: 350,
      clicks: 800,
    },
    assignedTo: 'TEST_MKTG_MGR_001',
  },
  eventCampaign: {
    id: 'TEST_CAMP_004',
    name: 'TEST_Annual_Conference_2026',
    type: 'Event',
    status: 'Planning',
    startDate: futureDate(90),
    endDate: futureDate(92),
    budget: 50000,
    targetAudience: 'Customers and Prospects',
    description: 'TEST: Annual user conference',
    goals: ['300 attendees', 'Customer retention', 'Upsell opportunities'],
    assignedTo: 'TEST_MKTG_MGR_001',
    venue: 'TEST Convention Center',
    registrations: 45,
  },
  webinarCampaign: {
    id: 'TEST_CAMP_005',
    name: 'TEST_Product_Demo_Webinar_Series',
    type: 'Webinar',
    status: 'Active',
    startDate: pastDate(14),
    endDate: futureDate(45),
    budget: 3000,
    actualCost: 1500,
    targetAudience: 'Leads and Prospects',
    description: 'TEST: Weekly product demo webinar series',
    goals: ['Lead generation', 'Product education'],
    metrics: {
      registrations: 250,
      attendees: 150,
      qualified: 30,
    },
    assignedTo: 'TEST_MKTG_001',
  },
  completedCampaign: {
    id: 'TEST_CAMP_006',
    name: 'TEST_Holiday_Promotion_COMPLETED',
    type: 'Email',
    status: 'Completed',
    startDate: pastDate(60),
    endDate: pastDate(30),
    budget: 8000,
    actualCost: 7500,
    targetAudience: 'All Customers',
    description: 'TEST: Holiday season promotion - COMPLETED',
    goals: ['Year-end sales boost', '50 new deals'],
    metrics: {
      sent: 10000,
      delivered: 9500,
      opened: 3800,
      clicked: 950,
      converted: 65,
      revenue: 125000,
    },
    assignedTo: 'TEST_MKTG_MGR_001',
    results: 'Exceeded goal by 30%',
  },
  pausedCampaign: {
    id: 'TEST_CAMP_007',
    name: 'TEST_Paused_Outreach_Campaign',
    type: 'Email',
    status: 'Paused',
    startDate: pastDate(21),
    endDate: futureDate(30),
    budget: 2000,
    actualCost: 800,
    targetAudience: 'Cold Leads',
    description: 'TEST: Paused outreach campaign',
    pauseReason: 'Messaging review in progress',
    assignedTo: 'TEST_MKTG_001',
  },
};

// ============================================================================
// TEST WORKFLOWS - For Automation Testing
// ============================================================================

export const COMPREHENSIVE_TEST_WORKFLOWS = {
  leadScoringWorkflow: {
    id: 'TEST_WF_001',
    name: 'TEST_Lead_Scoring_Automation',
    description: 'TEST: Automatically score leads based on engagement',
    status: 'Active',
    trigger: {
      type: 'EntityCreated',
      entity: 'Lead',
    },
    nodes: [
      { type: 'Trigger', config: { entity: 'Lead', event: 'Created' } },
      { type: 'Condition', config: { field: 'source', operator: 'equals', value: 'Website' } },
      { type: 'Action', config: { action: 'UpdateField', field: 'score', value: '+10' } },
      { type: 'AIDecision', config: { prompt: 'Evaluate lead quality', outcomes: ['High', 'Medium', 'Low'] } },
      { type: 'Action', config: { action: 'AssignTo', assignmentRule: 'RoundRobin' } },
    ],
    executionCount: 150,
    lastExecuted: pastDate(1),
  },
  opportunityStageWorkflow: {
    id: 'TEST_WF_002',
    name: 'TEST_Opportunity_Stage_Notifications',
    description: 'TEST: Notify on opportunity stage changes',
    status: 'Active',
    trigger: {
      type: 'FieldChanged',
      entity: 'Opportunity',
      field: 'stage',
    },
    nodes: [
      { type: 'Trigger', config: { entity: 'Opportunity', event: 'StageChanged' } },
      { type: 'Condition', config: { field: 'value', operator: 'greaterThan', value: 100000 } },
      { type: 'Action', config: { action: 'SendEmail', template: 'HighValueStageChange' } },
      { type: 'Action', config: { action: 'CreateTask', title: 'Follow up on stage change' } },
    ],
    executionCount: 75,
    lastExecuted: pastDate(2),
  },
  serviceEscalationWorkflow: {
    id: 'TEST_WF_003',
    name: 'TEST_Service_Request_Escalation',
    description: 'TEST: Auto-escalate overdue service requests',
    status: 'Active',
    trigger: {
      type: 'Scheduled',
      schedule: '0 * * * *', // Every hour
    },
    nodes: [
      { type: 'Trigger', config: { schedule: 'Hourly' } },
      { type: 'Query', config: { entity: 'ServiceRequest', filter: 'status=Open AND slaBreached=true' } },
      { type: 'Loop', config: { variable: 'request' } },
      { type: 'Action', config: { action: 'UpdateField', field: 'status', value: 'Escalated' } },
      { type: 'Action', config: { action: 'SendEmail', template: 'Escalation' } },
    ],
    executionCount: 200,
    lastExecuted: new Date().toISOString(),
  },
  customerOnboardingWorkflow: {
    id: 'TEST_WF_004',
    name: 'TEST_Customer_Onboarding',
    description: 'TEST: Automated customer onboarding sequence',
    status: 'Active',
    trigger: {
      type: 'EntityCreated',
      entity: 'Customer',
      condition: 'lifecycleStage = Customer',
    },
    nodes: [
      { type: 'Trigger', config: { entity: 'Customer', event: 'Created' } },
      { type: 'Action', config: { action: 'SendEmail', template: 'Welcome' } },
      { type: 'Delay', config: { days: 1 } },
      { type: 'Action', config: { action: 'CreateTask', title: 'Schedule kickoff call' } },
      { type: 'Delay', config: { days: 3 } },
      { type: 'AIAgent', config: { task: 'Generate personalized onboarding checklist' } },
      { type: 'Action', config: { action: 'SendEmail', template: 'OnboardingChecklist' } },
    ],
    executionCount: 30,
    lastExecuted: pastDate(3),
  },
  aiContentWorkflow: {
    id: 'TEST_WF_005',
    name: 'TEST_AI_Content_Generation',
    description: 'TEST: AI-powered content generation workflow',
    status: 'Draft',
    trigger: {
      type: 'Manual',
    },
    nodes: [
      { type: 'Trigger', config: { type: 'Manual' } },
      { type: 'AIContentGenerator', config: { contentType: 'Email', tone: 'Professional' } },
      { type: 'Approval', config: { approvers: ['Marketing Manager'] } },
      { type: 'Action', config: { action: 'PublishContent' } },
    ],
    executionCount: 5,
    lastExecuted: pastDate(7),
  },
  inactiveWorkflow: {
    id: 'TEST_WF_006',
    name: 'TEST_Inactive_Old_Workflow',
    description: 'TEST: Inactive workflow for testing',
    status: 'Inactive',
    trigger: {
      type: 'EntityUpdated',
      entity: 'Contact',
    },
    nodes: [
      { type: 'Trigger', config: { entity: 'Contact', event: 'Updated' } },
      { type: 'Action', config: { action: 'Log', message: 'Contact updated' } },
    ],
    executionCount: 500,
    lastExecuted: pastDate(90),
    deactivatedDate: pastDate(60),
    deactivatedReason: 'No longer needed',
  },
};

// ============================================================================
// TEST DATA RELATIONSHIPS - For Impact Testing
// ============================================================================

export const TEST_DATA_RELATIONSHIPS = {
  // Customer -> Contacts mapping
  customerContacts: {
    'TEST_CUST_ENT_001': ['TEST_CONT_001', 'TEST_CONT_002', 'TEST_CONT_003'],
    'TEST_CUST_ENT_002': ['TEST_CONT_004', 'TEST_CONT_005'],
    'TEST_CUST_MID_001': ['TEST_CONT_006'],
    'TEST_CUST_MID_002': ['TEST_CONT_007', 'TEST_CONT_008'],
  },
  
  // Customer -> Opportunities mapping
  customerOpportunities: {
    'TEST_CUST_ENT_001': ['TEST_OPP_001', 'TEST_OPP_008'],
    'TEST_CUST_ENT_002': ['TEST_OPP_002'],
    'TEST_CUST_MID_001': ['TEST_OPP_003'],
    'TEST_CUST_MID_002': ['TEST_OPP_004'],
    'TEST_CUST_SMB_001': ['TEST_OPP_005'],
    'TEST_CUST_SMB_002': ['TEST_OPP_006'],
    'TEST_CUST_CHURNED_001': ['TEST_OPP_007'],
  },
  
  // Customer -> Service Requests mapping
  customerServiceRequests: {
    'TEST_CUST_ENT_001': ['TEST_SR_001', 'TEST_SR_006'],
    'TEST_CUST_ENT_002': ['TEST_SR_002', 'TEST_SR_008'],
    'TEST_CUST_MID_001': ['TEST_SR_003', 'TEST_SR_007'],
    'TEST_CUST_MID_002': ['TEST_SR_004'],
    'TEST_CUST_SMB_001': ['TEST_SR_005'],
  },
  
  // User -> Assigned Items mapping
  userAssignments: {
    'TEST_SALES_REP_001': {
      customers: ['TEST_CUST_ENT_001', 'TEST_CUST_MID_001', 'TEST_CUST_SMB_002', 'TEST_CUST_IND_002'],
      opportunities: ['TEST_OPP_001', 'TEST_OPP_003', 'TEST_OPP_006', 'TEST_OPP_008'],
      leads: ['TEST_LEAD_001', 'TEST_LEAD_004', 'TEST_LEAD_006'],
    },
    'TEST_SALES_REP_002': {
      customers: ['TEST_CUST_MID_002', 'TEST_CUST_SMB_001', 'TEST_CUST_IND_001'],
      opportunities: ['TEST_OPP_004', 'TEST_OPP_005'],
      leads: ['TEST_LEAD_003', 'TEST_LEAD_005', 'TEST_LEAD_007'],
    },
    'TEST_SALES_MGR_001': {
      customers: ['TEST_CUST_ENT_002', 'TEST_CUST_CHURNED_001'],
      opportunities: ['TEST_OPP_002', 'TEST_OPP_007'],
      leads: ['TEST_LEAD_002', 'TEST_LEAD_008'],
    },
    'TEST_SUPPORT_001': {
      serviceRequests: ['TEST_SR_002', 'TEST_SR_004', 'TEST_SR_006'],
    },
    'TEST_SUPPORT_002': {
      serviceRequests: ['TEST_SR_003', 'TEST_SR_005', 'TEST_SR_007'],
    },
    'TEST_SUPPORT_MGR_001': {
      serviceRequests: ['TEST_SR_001', 'TEST_SR_008'],
    },
    'TEST_MKTG_MGR_001': {
      campaigns: ['TEST_CAMP_001', 'TEST_CAMP_003', 'TEST_CAMP_004', 'TEST_CAMP_006'],
    },
    'TEST_MKTG_001': {
      campaigns: ['TEST_CAMP_002', 'TEST_CAMP_005', 'TEST_CAMP_007'],
    },
  },
};

// ============================================================================
// EXPORT ALL TEST DATA
// ============================================================================

export const ALL_TEST_DATA = {
  users: COMPREHENSIVE_TEST_USERS,
  userGroups: COMPREHENSIVE_TEST_USER_GROUPS,
  customers: COMPREHENSIVE_TEST_CUSTOMERS,
  contacts: COMPREHENSIVE_TEST_CONTACTS,
  opportunities: COMPREHENSIVE_TEST_OPPORTUNITIES,
  leads: COMPREHENSIVE_TEST_LEADS,
  serviceRequests: COMPREHENSIVE_TEST_SERVICE_REQUESTS,
  campaigns: COMPREHENSIVE_TEST_CAMPAIGNS,
  workflows: COMPREHENSIVE_TEST_WORKFLOWS,
  relationships: TEST_DATA_RELATIONSHIPS,
};

export default ALL_TEST_DATA;

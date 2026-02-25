/**
 * CRM Solution - Comprehensive E2E Test Data
 *
 * All test data is clearly marked with TEST_ prefix to distinguish from
 * production data. This file covers every major CRM entity for full
 * end-to-end test coverage.
 *
 * Base URLs:
 *   Frontend : http://192.168.0.9
 *   API      : http://192.168.0.9:5000
 *
 * Admin credentials: admin@crm.local / Admin@123
 */

// ============================================================================
// Base URLs
// ============================================================================

export const TEST_BASE_URL = 'http://192.168.0.9';
export const TEST_API_URL  = 'http://192.168.0.9:5000';

// ============================================================================
// Helper Functions
// ============================================================================

/** Returns a 6-digit numeric suffix based on the current timestamp. */
export function uniqueTestId(): string {
  return Date.now().toString().slice(-6);
}

/** Generates a unique e-mail address safe for test use. */
export function generateTestEmail(prefix = 'test'): string {
  return `${prefix}_${uniqueTestId()}@crm-test.local`;
}

// ============================================================================
// TypeScript Interfaces
// ============================================================================

export interface TestUser {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: string;
}

export interface TestAddress {
  street: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
}

export interface TestAccount {
  name: string;
  type: string;
  industry?: string;
  email: string;
  phone: string;
  website?: string;
  address: TestAddress;
  description: string;
  annualRevenue?: number;
  numberOfEmployees?: number;
  lifecycleStage?: string;
}

export interface TestContact {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  jobTitle: string;
  department: string;
  accountName?: string;
}

export interface TestOpportunity {
  name: string;
  accountName: string;
  stage: string;
  amount: number;
  closeDate: string;
  probability: number;
  description: string;
}

export interface TestLead {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  company: string;
  title: string;
  source: string;
  status: string;
  rating: string;
  description: string;
}

export interface TestProduct {
  name: string;
  sku: string;
  price: number;
  description: string;
  category: string;
  unit: string;
}

export interface TestQuote {
  name: string;
  accountName: string;
  validUntil: string;
  discountPercent: number;
  terms: string;
  notes: string;
}

export interface TestOrder {
  name: string;
  accountName: string;
  orderDate: string;
  status: string;
  shippingAddress: string;
}

export interface TestInvoice {
  invoiceNumber: string;
  accountName: string;
  dueDate: string;
  notes: string;
}

export interface TestContract {
  name: string;
  accountName: string;
  startDate: string;
  endDate: string;
  value: number;
  type: string;
  status: string;
  terms: string;
}

export interface TestSubscription {
  name: string;
  accountName: string;
  planName: string;
  billingCycle: string;
  amount: number;
  startDate: string;
}

export interface TestPayment {
  amount: number;
  method: string;
  reference: string;
  notes: string;
}

export interface TestCampaign {
  name: string;
  type: string;
  status: string;
  subject?: string;
  startDate: string;
  endDate: string;
  description: string;
}

export interface TestEmailTemplate {
  name: string;
  subject: string;
  body: string;
  category: string;
}

export interface TestServiceRequest {
  title: string;
  description: string;
  priority: string;
  category: string;
  status: string;
}

export interface TestIncident {
  title: string;
  description: string;
  priority: string;
  category: string;
  affectedService: string;
  impactLevel: string;
}

export interface TestProblem {
  title: string;
  description: string;
  priority: string;
  category: string;
  rootCause: string;
}

export interface TestChange {
  title: string;
  description: string;
  type: string;
  priority: string;
  risk: string;
  scheduledStart: string;
  scheduledEnd: string;
  justification: string;
}

export interface TestSlaPolicy {
  name: string;
  description: string;
  responseTimeHours: number;
  resolutionTimeHours: number;
  priority: string;
  escalationEnabled: boolean;
}

export interface TestCmdbItem {
  name: string;
  type: string;
  category: string;
  status: string;
  environment: string;
  ipAddress?: string;
  description: string;
}

export interface TestKnowledgeArticle {
  title: string;
  content: string;
  category: string;
  tags: string[];
  status: string;
}

export interface TestEscalationRule {
  name: string;
  condition: string;
  escalateTo: string;
  delayMinutes: number;
  active: boolean;
}

export interface TestWorkflowStep {
  action: string;
  target: string;
  condition?: string;
}

export interface TestWorkflow {
  name: string;
  description: string;
  triggerType: string;
  status: string;
  steps: TestWorkflowStep[];
}

export interface TestTask {
  title: string;
  description: string;
  priority: string;
  dueDate: string;
  assignedTo: string;
}

export interface TestNote {
  title: string;
  content: string;
  entityType: string;
  tags: string[];
}

export interface TestActivity {
  type: string;
  subject: string;
  description: string;
  duration: number;
  outcome: string;
  date: string;
}

export interface TestTeam {
  name: string;
  description: string;
  leadEmail: string;
  region: string;
}

export interface TestDepartment {
  name: string;
  description: string;
  managerEmail: string;
  parentDept?: string;
}

export interface TestCommissionPlan {
  name: string;
  type: string;
  rate: number;
  currency: string;
  quota: number;
  period: string;
}

export interface TestLandingPage {
  name: string;
  title: string;
  description: string;
  status: string;
  url: string;
}

export interface TestWebFormField {
  label: string;
  type: string;
  required: boolean;
}

export interface TestWebForm {
  name: string;
  title: string;
  description: string;
  entityType: string;
  fields: TestWebFormField[];
}

export interface TestWebhook {
  name: string;
  url: string;
  events: string[];
  secret: string;
  active: boolean;
}

export interface TestAgent {
  name: string;
  description: string;
  type: string;
  model: string;
  systemPrompt: string;
  tools: string[];
  active: boolean;
}

export interface TestDuplicateRule {
  name: string;
  entityType: string;
  field: string;
  matchType: string;
  active: boolean;
}

export interface TestLeadScoreRule {
  name: string;
  entityType: string;
  field: string;
  value: string;
  score: number;
  active: boolean;
}

export interface TestEscalationLevel {
  level: number;
  notifyEmail: string;
  delayMinutes: number;
}

export interface TestEscalationPolicy {
  name: string;
  description: string;
  levels: TestEscalationLevel[];
  active: boolean;
}

export interface TestServiceQueue {
  name: string;
  description: string;
  priority: number;
  assignmentRule: string;
  active: boolean;
}

export interface TestGroup {
  name: string;
  description: string;
  permissions: string[];
  members: string[];
}

export interface AdminEmailSettings {
  senderEmail: string;
  senderName: string;
  smtpHost: string;
  smtpPort: number;
}

export interface AdminBrandingSettings {
  companyName: string;
  logoUrl: string;
  primaryColor: string;
}

export interface AdminSessionSettings {
  sessionTimeout: number;
  maxConcurrent: number;
}

export interface AdminSecuritySettings {
  mfaRequired: boolean;
  passwordMinLength: number;
}

export interface AdminSettings {
  email: AdminEmailSettings;
  branding: AdminBrandingSettings;
  session: AdminSessionSettings;
  security: AdminSecuritySettings;
}

// ============================================================================
// 1. TEST_USERS
// ============================================================================

export const TEST_USERS: Record<string, TestUser> = {
  admin: {
    email: 'admin@crm.local',
    password: 'Admin@123',
    firstName: 'System',
    lastName: 'Administrator',
    role: 'Administrator',
  },
  salesRep: {
    email: 'test_salesrep@crm-test.local',
    password: 'TestSales@123',
    firstName: 'TEST_Sales',
    lastName: 'Representative',
    role: 'Sales Representative',
  },
  supportAgent: {
    email: 'test_supportagent@crm-test.local',
    password: 'TestSupport@123',
    firstName: 'TEST_Support',
    lastName: 'Agent',
    role: 'Support Agent',
  },
  viewer: {
    email: 'test_viewer@crm-test.local',
    password: 'TestViewer@123',
    firstName: 'TEST_Read',
    lastName: 'Viewer',
    role: 'Viewer',
  },
  itsmAgent: {
    email: 'test_itsmagent@crm-test.local',
    password: 'TestITSM@123',
    firstName: 'TEST_ITSM',
    lastName: 'Agent',
    role: 'ITSM Agent',
  },
};

// ============================================================================
// 2. TEST_ACCOUNTS
// ============================================================================

export const TEST_ACCOUNTS: Record<string, TestAccount> = {
  corporate: {
    name: 'TEST_Acme Corporation',
    type: 'Corporate',
    industry: 'Technology',
    email: 'test_contact@acme-test.local',
    phone: '+1-555-0100',
    website: 'https://test-acme.crm-test.local',
    address: {
      street: '100 TEST Corporate Drive',
      city: 'TEST_San Francisco',
      state: 'CA',
      postalCode: '94105',
      country: 'US',
    },
    description: 'TEST: Large corporate technology account for automated E2E testing',
    annualRevenue: 10000000,
    numberOfEmployees: 500,
    lifecycleStage: 'Customer',
  },
  individual: {
    name: 'TEST_Freelance Consulting',
    type: 'Individual',
    email: 'test_freelance@crm-test.local',
    phone: '+1-555-0101',
    address: {
      street: '200 TEST Maker Lane',
      city: 'TEST_Austin',
      state: 'TX',
      postalCode: '78701',
      country: 'US',
    },
    description: 'TEST: Individual/sole-trader account for testing',
    lifecycleStage: 'Prospect',
  },
  enterprise: {
    name: 'TEST_Global Enterprises Inc',
    type: 'Enterprise',
    industry: 'Finance',
    email: 'test_info@global-test.local',
    phone: '+1-555-0102',
    website: 'https://test-globalent.crm-test.local',
    address: {
      street: '300 TEST Finance Blvd',
      city: 'TEST_New York',
      state: 'NY',
      postalCode: '10001',
      country: 'US',
    },
    description: 'TEST: Large enterprise finance account for testing',
    annualRevenue: 500000000,
    numberOfEmployees: 5000,
    lifecycleStage: 'Customer',
  },
  startup: {
    name: 'TEST_StartupCo Ltd',
    type: 'Small Business',
    industry: 'SaaS',
    email: 'test_hello@startupco-test.local',
    phone: '+1-555-0103',
    website: 'https://test-startupco.crm-test.local',
    address: {
      street: '400 TEST Innovation Way',
      city: 'TEST_Seattle',
      state: 'WA',
      postalCode: '98101',
      country: 'US',
    },
    description: 'TEST: Startup account to test small-business workflows',
    annualRevenue: 250000,
    numberOfEmployees: 12,
    lifecycleStage: 'Lead',
  },
};

// ============================================================================
// 3. TEST_CONTACTS
// ============================================================================

export const TEST_CONTACTS: Record<string, TestContact> = {
  primary: {
    firstName: 'TEST_Jane',
    lastName: 'Doe',
    email: 'test_jane.doe@crm-test.local',
    phone: '+1-555-0200',
    jobTitle: 'VP of Engineering',
    department: 'Engineering',
    accountName: 'TEST_Acme Corporation',
  },
  secondary: {
    firstName: 'TEST_Bob',
    lastName: 'Wilson',
    email: 'test_bob.wilson@crm-test.local',
    phone: '+1-555-0201',
    jobTitle: 'Operations Manager',
    department: 'Operations',
    accountName: 'TEST_Acme Corporation',
  },
  vip: {
    firstName: 'TEST_Victoria',
    lastName: 'Chen',
    email: 'test_victoria.chen@crm-test.local',
    phone: '+1-555-0202',
    jobTitle: 'Chief Executive Officer',
    department: 'Executive',
    accountName: 'TEST_Global Enterprises Inc',
  },
};

// ============================================================================
// 4. TEST_OPPORTUNITIES
// ============================================================================

export const TEST_OPPORTUNITIES: Record<string, TestOpportunity> = {
  high_value: {
    name: 'TEST_High Value Enterprise Deal',
    accountName: 'TEST_Global Enterprises Inc',
    stage: 'Proposal',
    amount: 250000,
    closeDate: '2026-06-30',
    probability: 65,
    description: 'TEST: High-value enterprise opportunity for automation testing',
  },
  medium: {
    name: 'TEST_Mid-Market Software Renewal',
    accountName: 'TEST_Acme Corporation',
    stage: 'Qualification',
    amount: 50000,
    closeDate: '2026-04-15',
    probability: 40,
    description: 'TEST: Mid-market software renewal opportunity',
  },
  small: {
    name: 'TEST_Startup Starter Package',
    accountName: 'TEST_StartupCo Ltd',
    stage: 'Prospecting',
    amount: 5000,
    closeDate: '2026-03-31',
    probability: 20,
    description: 'TEST: Small opportunity for startup package',
  },
  enterprise_deal: {
    name: 'TEST_Enterprise Platform License',
    accountName: 'TEST_Global Enterprises Inc',
    stage: 'Negotiation',
    amount: 800000,
    closeDate: '2026-09-30',
    probability: 80,
    description: 'TEST: Large-scale enterprise platform licensing deal',
  },
};

// ============================================================================
// 5. TEST_LEADS
// ============================================================================

export const TEST_LEADS: Record<string, TestLead> = {
  hot: {
    firstName: 'TEST_Henry',
    lastName: 'HotLead',
    email: 'test_henry.hot@crm-test.local',
    phone: '+1-555-0300',
    company: 'TEST_HotCo Industries',
    title: 'CTO',
    source: 'Website',
    status: 'Hot',
    rating: 'Hot',
    description: 'TEST: Hot lead — requested demo, responded to all outreach',
  },
  warm: {
    firstName: 'TEST_Wendy',
    lastName: 'WarmProspect',
    email: 'test_wendy.warm@crm-test.local',
    phone: '+1-555-0301',
    company: 'TEST_WarmB2B Corp',
    title: 'VP Sales',
    source: 'Email Campaign',
    status: 'Warm',
    rating: 'Warm',
    description: 'TEST: Warm lead — opened multiple emails, visited pricing page',
  },
  cold: {
    firstName: 'TEST_Carl',
    lastName: 'ColdContact',
    email: 'test_carl.cold@crm-test.local',
    phone: '+1-555-0302',
    company: 'TEST_ColdReach LLC',
    title: 'Marketing Manager',
    source: 'List Import',
    status: 'Cold',
    rating: 'Cold',
    description: 'TEST: Cold lead from imported list — no prior engagement',
  },
  web_form_lead: {
    firstName: 'TEST_Fiona',
    lastName: 'WebForm',
    email: 'test_fiona.web@crm-test.local',
    phone: '+1-555-0303',
    company: 'TEST_WebForm Startup',
    title: 'Founder',
    source: 'Web Form',
    status: 'New',
    rating: 'Warm',
    description: 'TEST: Lead captured via website contact form',
  },
};

// ============================================================================
// 6. TEST_PRODUCTS
// ============================================================================

export const TEST_PRODUCTS: Record<string, TestProduct> = {
  software: {
    name: 'TEST_Enterprise CRM License',
    sku: 'TEST-SW-ENT-001',
    price: 1499.99,
    description: 'TEST: Annual per-seat enterprise CRM license for testing',
    category: 'Software',
    unit: 'Seat/Year',
  },
  hardware: {
    name: 'TEST_Smart Badge Scanner',
    sku: 'TEST-HW-SCN-001',
    price: 349.00,
    description: 'TEST: RFID badge scanner hardware unit for testing',
    category: 'Hardware',
    unit: 'Unit',
  },
  service: {
    name: 'TEST_Professional Services Block',
    sku: 'TEST-SVC-PS-001',
    price: 175.00,
    description: 'TEST: 1-hour professional services engagement for implementation',
    category: 'Professional Services',
    unit: 'Hour',
  },
  bundle: {
    name: 'TEST_Starter Bundle',
    sku: 'TEST-BND-STR-001',
    price: 2499.00,
    description: 'TEST: Starter bundle combining software license + onboarding hours',
    category: 'Bundle',
    unit: 'Bundle',
  },
};

// ============================================================================
// 7. TEST_QUOTES
// ============================================================================

export const TEST_QUOTES: Record<string, TestQuote> = {
  standard: {
    name: 'TEST_Standard Quote Q1-2026',
    accountName: 'TEST_Acme Corporation',
    validUntil: '2026-03-31',
    discountPercent: 10,
    terms: 'Net 30. TEST: Standard payment terms apply.',
    notes: 'TEST: Standard quote generated for Q1 pipeline review',
  },
  enterprise: {
    name: 'TEST_Enterprise Quote Q2-2026',
    accountName: 'TEST_Global Enterprises Inc',
    validUntil: '2026-06-30',
    discountPercent: 20,
    terms: 'Net 60. TEST: Enterprise terms with volume discount.',
    notes: 'TEST: Enterprise quote for large platform deployment',
  },
  renewal: {
    name: 'TEST_Annual Renewal Quote 2026',
    accountName: 'TEST_Acme Corporation',
    validUntil: '2026-12-31',
    discountPercent: 5,
    terms: 'Net 30. TEST: Renewal terms, loyalty discount applied.',
    notes: 'TEST: Annual renewal quote — existing customer',
  },
};

// ============================================================================
// 8. TEST_ORDERS
// ============================================================================

export const TEST_ORDERS: Record<string, TestOrder> = {
  standard: {
    name: 'TEST_Order-STD-2026-001',
    accountName: 'TEST_Acme Corporation',
    orderDate: '2026-02-01',
    status: 'Pending',
    shippingAddress: '100 TEST Corporate Drive, TEST_San Francisco, CA 94105, US',
  },
  urgent: {
    name: 'TEST_Order-URG-2026-002',
    accountName: 'TEST_Global Enterprises Inc',
    orderDate: '2026-02-15',
    status: 'Processing',
    shippingAddress: '300 TEST Finance Blvd, TEST_New York, NY 10001, US',
  },
  recurring: {
    name: 'TEST_Order-REC-2026-003',
    accountName: 'TEST_Acme Corporation',
    orderDate: '2026-03-01',
    status: 'Recurring',
    shippingAddress: '100 TEST Corporate Drive, TEST_San Francisco, CA 94105, US',
  },
};

// ============================================================================
// 9. TEST_INVOICES
// ============================================================================

export const TEST_INVOICES: Record<string, TestInvoice> = {
  standard: {
    invoiceNumber: 'TEST-INV-2026-001',
    accountName: 'TEST_Acme Corporation',
    dueDate: '2026-03-15',
    notes: 'TEST: Standard invoice for Q1 services',
  },
  overdue: {
    invoiceNumber: 'TEST-INV-2025-099',
    accountName: 'TEST_StartupCo Ltd',
    dueDate: '2026-01-01',
    notes: 'TEST: Overdue invoice for testing collection workflows',
  },
  paid: {
    invoiceNumber: 'TEST-INV-2026-002',
    accountName: 'TEST_Global Enterprises Inc',
    dueDate: '2026-02-28',
    notes: 'TEST: Paid invoice for reconciliation testing',
  },
};

// ============================================================================
// 10. TEST_CONTRACTS
// ============================================================================

export const TEST_CONTRACTS: Record<string, TestContract> = {
  standard: {
    name: 'TEST_Standard Service Agreement 2026',
    accountName: 'TEST_Acme Corporation',
    startDate: '2026-01-01',
    endDate: '2026-12-31',
    value: 120000,
    type: 'Service Agreement',
    status: 'Active',
    terms: 'TEST: Annual service agreement with standard SLA terms. Renewable.',
  },
  enterprise: {
    name: 'TEST_Enterprise Master Agreement 2026',
    accountName: 'TEST_Global Enterprises Inc',
    startDate: '2026-01-01',
    endDate: '2027-12-31',
    value: 2500000,
    type: 'Master Service Agreement',
    status: 'Active',
    terms: 'TEST: Multi-year MSA with custom SLA, dedicated support and priority escalation.',
  },
  renewal: {
    name: 'TEST_Renewal Contract 2026-2027',
    accountName: 'TEST_Acme Corporation',
    startDate: '2026-07-01',
    endDate: '2027-06-30',
    value: 130000,
    type: 'Renewal',
    status: 'Draft',
    terms: 'TEST: Contract renewal draft with 8% uplift from previous term.',
  },
};

// ============================================================================
// 11. TEST_SUBSCRIPTIONS
// ============================================================================

export const TEST_SUBSCRIPTIONS: Record<string, TestSubscription> = {
  monthly: {
    name: 'TEST_Monthly Pro Subscription',
    accountName: 'TEST_StartupCo Ltd',
    planName: 'Pro',
    billingCycle: 'Monthly',
    amount: 299.00,
    startDate: '2026-02-01',
  },
  annual: {
    name: 'TEST_Annual Enterprise Subscription',
    accountName: 'TEST_Acme Corporation',
    planName: 'Enterprise',
    billingCycle: 'Annual',
    amount: 14400.00,
    startDate: '2026-01-01',
  },
  trial: {
    name: 'TEST_14-Day Trial Subscription',
    accountName: 'TEST_StartupCo Ltd',
    planName: 'Trial',
    billingCycle: 'Trial',
    amount: 0,
    startDate: '2026-02-25',
  },
};

// ============================================================================
// 12. TEST_PAYMENTS
// ============================================================================

export const TEST_PAYMENTS: Record<string, TestPayment> = {
  standard: {
    amount: 1499.99,
    method: 'Credit Card',
    reference: 'TEST-PAY-CC-2026-001',
    notes: 'TEST: Standard credit card payment for Feb 2026 subscription',
  },
  refund: {
    amount: -349.00,
    method: 'Refund',
    reference: 'TEST-PAY-REF-2026-001',
    notes: 'TEST: Refund issued for returned hardware unit',
  },
  partial: {
    amount: 500.00,
    method: 'Bank Transfer',
    reference: 'TEST-PAY-BT-2026-001',
    notes: 'TEST: Partial payment — first instalment of invoice TEST-INV-2026-001',
  },
};

// ============================================================================
// 13. TEST_CAMPAIGNS
// ============================================================================

export const TEST_CAMPAIGNS: Record<string, TestCampaign> = {
  email: {
    name: 'TEST_Spring Email Campaign 2026',
    type: 'Email',
    status: 'Draft',
    subject: 'Exclusive Spring Offer for TEST Customers',
    startDate: '2026-03-01',
    endDate: '2026-03-31',
    description: 'TEST: Spring promotional email campaign targeting existing customers',
  },
  social: {
    name: 'TEST_Social Media Q2 Push 2026',
    type: 'Social Media',
    status: 'Draft',
    startDate: '2026-04-01',
    endDate: '2026-06-30',
    description: 'TEST: Quarterly social media awareness campaign',
  },
  sms: {
    name: 'TEST_SMS Flash Sale 2026',
    type: 'SMS',
    status: 'Draft',
    subject: 'Flash Sale — 24h only for TEST subscribers',
    startDate: '2026-05-15',
    endDate: '2026-05-16',
    description: 'TEST: 24-hour flash sale SMS blast',
  },
  drip_campaign: {
    name: 'TEST_Lead Nurture Drip 2026',
    type: 'Drip',
    status: 'Draft',
    subject: 'Welcome to CRM TEST — Email 1 of 5',
    startDate: '2026-03-01',
    endDate: '2026-06-30',
    description: 'TEST: 5-email drip sequence for new lead nurturing',
  },
};

// ============================================================================
// 14. TEST_EMAIL_TEMPLATES
// ============================================================================

export const TEST_EMAIL_TEMPLATES: Record<string, TestEmailTemplate> = {
  welcome: {
    name: 'TEST_Welcome Email Template',
    subject: 'Welcome to TEST CRM Platform',
    body: `<p>Dear {{firstName}},</p>
<p>Welcome to the TEST CRM Platform. Your account is now active.</p>
<p>If you have any questions, contact support at test-support@crm-test.local</p>
<p>TEST: This is an automated test email template.</p>`,
    category: 'Onboarding',
  },
  follow_up: {
    name: 'TEST_Follow-Up Email Template',
    subject: 'Following Up on Your TEST Enquiry',
    body: `<p>Hi {{firstName}},</p>
<p>I wanted to follow up on your recent enquiry about {{product}}.</p>
<p>Would you be available for a brief call this week?</p>
<p>TEST: This is an automated test follow-up template.</p>`,
    category: 'Sales',
  },
  invoice_notification: {
    name: 'TEST_Invoice Notification Template',
    subject: 'Invoice {{invoiceNumber}} — Due {{dueDate}}',
    body: `<p>Dear {{contactName}},</p>
<p>Please find attached invoice {{invoiceNumber}} for the amount of {{amount}}.</p>
<p>Payment is due by {{dueDate}}. TEST: Auto-generated invoice notification.</p>`,
    category: 'Finance',
  },
};

// ============================================================================
// 15. TEST_SERVICE_REQUESTS
// ============================================================================

export const TEST_SERVICE_REQUESTS: Record<string, TestServiceRequest> = {
  high_priority: {
    title: 'TEST_High Priority — Application Cannot Start',
    description: 'TEST: Production application fails to start after latest deployment. Environment: production. All users affected.',
    priority: 'High',
    category: 'Incident',
    status: 'New',
  },
  medium: {
    title: 'TEST_Medium — Slow Report Generation',
    description: 'TEST: Monthly reports taking >10 minutes to generate. Degraded performance since last DB upgrade.',
    priority: 'Medium',
    category: 'Performance',
    status: 'New',
  },
  low: {
    title: 'TEST_Low — Update User Profile Picture',
    description: 'TEST: User requests to update profile picture but UI button is not visible in Firefox.',
    priority: 'Low',
    category: 'UI Bug',
    status: 'New',
  },
  bulk: {
    title: 'TEST_BULK — Batch Data Import Failure',
    description: 'TEST: Bulk CSV import for 10000 contact records fails at row 5000 with a timeout error.',
    priority: 'High',
    category: 'Data',
    status: 'New',
  },
};

// ============================================================================
// 16. TEST_INCIDENTS
// ============================================================================

export const TEST_INCIDENTS: Record<string, TestIncident> = {
  critical: {
    title: 'TEST_CRITICAL — Full Database Outage',
    description: 'TEST: Primary database cluster is unresponsive. All read/write operations failing. Business impact: severe.',
    priority: 'Critical',
    category: 'Infrastructure',
    affectedService: 'crm-mariadb',
    impactLevel: 'Full Outage',
  },
  high: {
    title: 'TEST_HIGH — API Gateway Returning 503',
    description: 'TEST: API gateway intermittently returning HTTP 503 to 30% of requests. Partial service degradation.',
    priority: 'High',
    category: 'Networking',
    affectedService: 'crm-gateway',
    impactLevel: 'Partial Outage',
  },
  medium: {
    title: 'TEST_MEDIUM — Email Notifications Delayed',
    description: 'TEST: Transactional emails being delivered 2–4 hours late. SMTP queue queue backup detected.',
    priority: 'Medium',
    category: 'Integration',
    affectedService: 'Notification Service',
    impactLevel: 'Degraded',
  },
  low: {
    title: 'TEST_LOW — Dashboard Widget Misaligned',
    description: 'TEST: On 1280px viewport the revenue widget overlaps the pipeline chart. Visual-only defect.',
    priority: 'Low',
    category: 'UI',
    affectedService: 'crm-frontend',
    impactLevel: 'Cosmetic',
  },
};

// ============================================================================
// 17. TEST_PROBLEMS
// ============================================================================

export const TEST_PROBLEMS: Record<string, TestProblem> = {
  infrastructure: {
    title: 'TEST_Recurring Database Connection Pool Exhaustion',
    description: 'TEST: Recurring incident where DB connection pool reaches max under peak load. Needs root cause analysis and permanent fix.',
    priority: 'High',
    category: 'Infrastructure',
    rootCause: 'TEST: Identified — long-running queries not releasing connections. Requires query optimisation and pool tuning.',
  },
  application: {
    title: 'TEST_Memory Leak in Background Job Processor',
    description: 'TEST: Background job service consumes 8GB RAM after 72h uptime requiring a restart. Memory leak suspected in report aggregation job.',
    priority: 'Medium',
    category: 'Application',
    rootCause: 'TEST: Under investigation — likely unbounded collection in AggregationService.ProcessBatch()',
  },
};

// ============================================================================
// 18. TEST_CHANGES
// ============================================================================

export const TEST_CHANGES: Record<string, TestChange> = {
  standard: {
    title: 'TEST_Standard Change — MariaDB Version Upgrade 10.11 → 11.2',
    description: 'TEST: Planned minor version upgrade of MariaDB. All migrations validated in staging.',
    type: 'Standard',
    priority: 'Medium',
    risk: 'Low',
    scheduledStart: '2026-03-01T02:00:00Z',
    scheduledEnd:   '2026-03-01T04:00:00Z',
    justification: 'TEST: Security patches and performance improvements included in 11.2 release.',
  },
  emergency: {
    title: 'TEST_Emergency Change — CVE-2026-XXXX Critical Patch',
    description: 'TEST: Emergency security patch for critical CVE. Must be applied within 4 hours of approval.',
    type: 'Emergency',
    priority: 'Critical',
    risk: 'Medium',
    scheduledStart: '2026-02-25T18:00:00Z',
    scheduledEnd:   '2026-02-25T20:00:00Z',
    justification: 'TEST: Active exploit in the wild. CISO mandated immediate deployment.',
  },
  maintenance: {
    title: 'TEST_Maintenance Window — Redis Cache Clear & Restart',
    description: 'TEST: Monthly maintenance window to flush Redis cache and perform rolling restart to apply config changes.',
    type: 'Maintenance',
    priority: 'Low',
    risk: 'Low',
    scheduledStart: '2026-03-15T01:00:00Z',
    scheduledEnd:   '2026-03-15T02:30:00Z',
    justification: 'TEST: Routine maintenance per operational runbook. No anticipated user impact.',
  },
};

// ============================================================================
// 19. TEST_SLA_POLICIES
// ============================================================================

export const TEST_SLA_POLICIES: Record<string, TestSlaPolicy> = {
  gold: {
    name: 'TEST_Gold SLA Policy',
    description: 'TEST: Premium SLA for enterprise customers — fastest response and resolution targets',
    responseTimeHours: 1,
    resolutionTimeHours: 4,
    priority: 'Critical',
    escalationEnabled: true,
  },
  silver: {
    name: 'TEST_Silver SLA Policy',
    description: 'TEST: Standard SLA for business customers — balanced response targets',
    responseTimeHours: 4,
    resolutionTimeHours: 24,
    priority: 'High',
    escalationEnabled: true,
  },
  bronze: {
    name: 'TEST_Bronze SLA Policy',
    description: 'TEST: Basic SLA for standard customers — best-effort response targets',
    responseTimeHours: 8,
    resolutionTimeHours: 72,
    priority: 'Medium',
    escalationEnabled: false,
  },
};

// ============================================================================
// 20. TEST_CMDB_ITEMS
// ============================================================================

export const TEST_CMDB_ITEMS: Record<string, TestCmdbItem> = {
  server: {
    name: 'TEST_CRM-APP-SERVER-01',
    type: 'Server',
    category: 'Compute',
    status: 'Active',
    environment: 'Production',
    ipAddress: '192.168.0.9',
    description: 'TEST: Primary application server running crm-api and crm-frontend containers',
  },
  database: {
    name: 'TEST_CRM-MARIADB-01',
    type: 'Database Server',
    category: 'Database',
    status: 'Active',
    environment: 'Production',
    ipAddress: '172.20.0.10',
    description: 'TEST: MariaDB 11.2 primary database server — crm_db schema',
  },
  application: {
    name: 'TEST_CRM-API-SERVICE',
    type: 'Application',
    category: 'Software',
    status: 'Active',
    environment: 'Production',
    description: 'TEST: ASP.NET Core 10 CRM API service — monolith deployment',
  },
  network_device: {
    name: 'TEST_CRM-SWITCH-CORE-01',
    type: 'Network Switch',
    category: 'Networking',
    status: 'Active',
    environment: 'Production',
    ipAddress: '192.168.0.1',
    description: 'TEST: Core layer-3 switch connecting CRM server VLANs',
  },
};

// ============================================================================
// 21. TEST_KNOWLEDGE_ARTICLES
// ============================================================================

export const TEST_KNOWLEDGE_ARTICLES: Record<string, TestKnowledgeArticle> = {
  how_to: {
    title: 'TEST_How To: Reset Your CRM Password',
    content: `## Overview\nTEST article describing the password reset flow.\n\n## Steps\n1. Navigate to the login page.\n2. Click "Forgot Password".\n3. Enter your registered email address.\n4. Follow the link in the reset email.\n5. Set a new password that meets complexity requirements.\n\n_TEST: This is a test knowledge article for E2E validation._`,
    category: 'How-To',
    tags: ['password', 'reset', 'login', 'test'],
    status: 'Published',
  },
  troubleshooting: {
    title: 'TEST_Troubleshooting: Cannot Log In to CRM',
    content: `## Symptoms\nUser receives an error when attempting to log in.\n\n## Possible Causes\n- Incorrect password\n- Account locked after 5 failed attempts\n- SSO misconfiguration\n\n## Resolution\nReset password via the forgot-password flow or contact admin.\n\n_TEST: Troubleshooting article for E2E testing._`,
    category: 'Troubleshooting',
    tags: ['login', 'error', 'troubleshooting', 'test'],
    status: 'Published',
  },
  faq: {
    title: 'TEST_FAQ: CRM Billing & Subscription Questions',
    content: `## FAQ\n\n**Q: When am I billed?**\nA: You are billed on the 1st of each calendar month.\n\n**Q: Can I change my plan?**\nA: Yes, upgrades apply immediately; downgrades apply at the next billing cycle.\n\n_TEST: FAQ article for billing workflows._`,
    category: 'FAQ',
    tags: ['billing', 'subscription', 'faq', 'test'],
    status: 'Draft',
  },
  policy: {
    title: 'TEST_Policy: Data Retention & Privacy',
    content: `## Policy Statement\nAll customer data is retained for 7 years in compliance with GDPR Article 17 obligations.\n\n## Scope\nApplies to all CRM entities: Accounts, Contacts, Leads, Opportunities.\n\n_TEST: Policy article for compliance workflow testing._`,
    category: 'Policy',
    tags: ['gdpr', 'data', 'retention', 'policy', 'test'],
    status: 'Published',
  },
};

// ============================================================================
// 22. TEST_ESCALATION_RULES
// ============================================================================

export const TEST_ESCALATION_RULES: Record<string, TestEscalationRule> = {
  high_priority: {
    name: 'TEST_Escalate High Priority After 4h',
    condition: 'priority = High AND status = Open AND age > 4h',
    escalateTo: 'test_supportagent@crm-test.local',
    delayMinutes: 240,
    active: true,
  },
  critical: {
    name: 'TEST_Escalate Critical Immediately',
    condition: 'priority = Critical AND status = New',
    escalateTo: 'admin@crm.local',
    delayMinutes: 15,
    active: true,
  },
};

// ============================================================================
// 23. TEST_WORKFLOWS
// ============================================================================

export const TEST_WORKFLOWS: Record<string, TestWorkflow> = {
  lead_assignment: {
    name: 'TEST_Lead Auto-Assignment Workflow',
    description: 'TEST: Assigns incoming leads to the next available sales rep using round-robin logic',
    triggerType: 'Lead Created',
    status: 'Inactive',
    steps: [
      { action: 'AssignOwner', target: 'RoundRobin:SalesTeam', condition: 'source = Website' },
      { action: 'SendEmail',   target: 'Owner',                condition: 'assigned = true'  },
      { action: 'CreateTask',  target: 'Owner',                condition: 'none'              },
    ],
  },
  opportunity_stage: {
    name: 'TEST_Opportunity Stage Progression Workflow',
    description: 'TEST: Sends notifications and creates tasks when opportunity stage changes',
    triggerType: 'Opportunity Updated',
    status: 'Inactive',
    steps: [
      { action: 'SendNotification', target: 'AccountOwner',  condition: 'stage changed'    },
      { action: 'CreateTask',       target: 'Owner',         condition: 'stage = Proposal' },
    ],
  },
  customer_onboarding: {
    name: 'TEST_New Customer Onboarding Workflow',
    description: 'TEST: Triggers a sequence of tasks and emails when a new customer account is created',
    triggerType: 'Account Created',
    status: 'Inactive',
    steps: [
      { action: 'SendEmail',  target: 'PrimaryContact', condition: 'none'               },
      { action: 'CreateTask', target: 'AccountOwner',   condition: 'none'               },
      { action: 'SendEmail',  target: 'PrimaryContact', condition: 'delay = 3d'         },
    ],
  },
  service_request_routing: {
    name: 'TEST_Service Request Auto-Routing Workflow',
    description: 'TEST: Routes incoming service requests to the correct support queue based on category',
    triggerType: 'Service Request Created',
    status: 'Inactive',
    steps: [
      { action: 'AssignQueue', target: 'TEST_Tier1Queue', condition: 'priority = Low OR priority = Medium' },
      { action: 'AssignQueue', target: 'TEST_Tier2Queue', condition: 'priority = High OR priority = Critical' },
      { action: 'SendEmail',   target: 'Requester',       condition: 'none' },
    ],
  },
};

// ============================================================================
// 24. TEST_TASKS
// ============================================================================

export const TEST_TASKS: Record<string, TestTask> = {
  standard: {
    title: 'TEST_Follow up with primary contact',
    description: 'TEST: Schedule a follow-up call to discuss proposal feedback from TEST_Acme Corporation',
    priority: 'Medium',
    dueDate: '2026-03-15',
    assignedTo: 'test_salesrep@crm-test.local',
  },
  urgent: {
    title: 'TEST_URGENT — Send renewal quote before deadline',
    description: 'TEST: Renewal quote must be sent by EOD. Contract expires 2026-06-30. High-priority action required.',
    priority: 'High',
    dueDate: '2026-03-01',
    assignedTo: 'test_salesrep@crm-test.local',
  },
  recurring_task: {
    title: 'TEST_Weekly pipeline review preparation',
    description: 'TEST: Prepare opportunity pipeline slides for Monday management review (recurring weekly task)',
    priority: 'Low',
    dueDate: '2026-03-03',
    assignedTo: 'test_salesrep@crm-test.local',
  },
};

// ============================================================================
// 25. TEST_NOTES
// ============================================================================

export const TEST_NOTES: Record<string, TestNote> = {
  account_note: {
    title: 'TEST_Account Review Notes - Q1 2026',
    content: 'TEST: Spoke with Victoria Chen on 2026-02-20. Confirmed budget approved for platform expansion. Decision expected by 2026-03-15. Stakeholders: IT, Finance, Procurement.',
    entityType: 'Account',
    tags: ['review', 'q1-2026', 'budget', 'test'],
  },
  contact_note: {
    title: 'TEST_Contact Preferences Note',
    content: 'TEST: Jane Doe prefers communication via email only. No calls before 10am. Best time to reach: Tuesday/Thursday afternoons.',
    entityType: 'Contact',
    tags: ['preferences', 'communication', 'test'],
  },
  opportunity_note: {
    title: 'TEST_Opportunity Negotiation Note',
    content: 'TEST: Client requested 25% discount but 20% is maximum approved threshold. Legal review of contract terms in progress. Expected sign-off 2026-03-20.',
    entityType: 'Opportunity',
    tags: ['negotiation', 'discount', 'legal', 'test'],
  },
};

// ============================================================================
// 26. TEST_ACTIVITIES
// ============================================================================

export const TEST_ACTIVITIES: Record<string, TestActivity> = {
  call: {
    type: 'Call',
    subject: 'TEST_Discovery Call — TEST_Acme Corporation',
    description: 'TEST: Initial discovery call to understand requirements and pain points. Discussed current CRM limitations.',
    duration: 45,
    outcome: 'Positive — follow-up demo scheduled',
    date: '2026-02-20',
  },
  email_activity: {
    type: 'Email',
    subject: 'TEST_Proposal Follow-Up Email',
    description: 'TEST: Sent proposal summary email with pricing deck attached. Awaiting reply.',
    duration: 15,
    outcome: 'Email sent — no reply yet',
    date: '2026-02-22',
  },
  meeting: {
    type: 'Meeting',
    subject: 'TEST_Stakeholder Presentation — TEST_Global Enterprises Inc',
    description: 'TEST: Presented platform capabilities to 5 stakeholders. Technical Q&A followed. Strong interest from CTO.',
    duration: 90,
    outcome: 'Excellent — pilot project proposed',
    date: '2026-02-24',
  },
};

// ============================================================================
// 27. TEST_TEAMS
// ============================================================================

export const TEST_TEAMS: Record<string, TestTeam> = {
  sales_team: {
    name: 'TEST_Enterprise Sales Team',
    description: 'TEST: Sales team focused on enterprise accounts (500+ employees)',
    leadEmail: 'test_salesrep@crm-test.local',
    region: 'North America',
  },
  support_team: {
    name: 'TEST_Customer Support Team',
    description: 'TEST: Tier-1 and Tier-2 customer support agents',
    leadEmail: 'test_supportagent@crm-test.local',
    region: 'Global',
  },
  marketing_team: {
    name: 'TEST_Digital Marketing Team',
    description: 'TEST: Demand generation and campaign management team',
    leadEmail: 'admin@crm.local',
    region: 'EMEA',
  },
};

// ============================================================================
// 28. TEST_DEPARTMENTS
// ============================================================================

export const TEST_DEPARTMENTS: Record<string, TestDepartment> = {
  sales: {
    name: 'TEST_Sales Department',
    description: 'TEST: Revenue-generating sales function responsible for new business and renewals',
    managerEmail: 'test_salesrep@crm-test.local',
  },
  engineering: {
    name: 'TEST_Engineering Department',
    description: 'TEST: Product and platform engineering team',
    managerEmail: 'admin@crm.local',
    parentDept: 'TEST_Technology Division',
  },
  support: {
    name: 'TEST_Customer Support Department',
    description: 'TEST: Customer-facing support and ITSM operations',
    managerEmail: 'test_supportagent@crm-test.local',
  },
  marketing: {
    name: 'TEST_Marketing Department',
    description: 'TEST: Brand, demand generation and customer marketing function',
    managerEmail: 'admin@crm.local',
  },
};

// ============================================================================
// 29. TEST_COMMISSION_PLANS
// ============================================================================

export const TEST_COMMISSION_PLANS: Record<string, TestCommissionPlan> = {
  standard: {
    name: 'TEST_Standard Sales Commission Plan',
    type: 'Percentage of Revenue',
    rate: 5.0,
    currency: 'USD',
    quota: 500000,
    period: 'Annual',
  },
  accelerated: {
    name: 'TEST_Accelerated Commission Plan (>120% Quota)',
    type: 'Accelerated Percentage',
    rate: 8.5,
    currency: 'USD',
    quota: 600000,
    period: 'Annual',
  },
  tiered: {
    name: 'TEST_Tiered Commission Plan',
    type: 'Tiered',
    rate: 3.0,
    currency: 'USD',
    quota: 400000,
    period: 'Quarterly',
  },
};

// ============================================================================
// 30. TEST_LANDING_PAGES
// ============================================================================

export const TEST_LANDING_PAGES: Record<string, TestLandingPage> = {
  demo_request: {
    name: 'TEST_Request a Demo Landing Page',
    title: 'See TEST CRM in Action — Book Your Free Demo',
    description: 'TEST: Landing page capturing demo request leads from paid search campaigns',
    status: 'Draft',
    url: '/lp/test-demo-request',
  },
  newsletter: {
    name: 'TEST_Newsletter Sign-Up Page',
    title: 'Stay Ahead — Subscribe to the TEST CRM Newsletter',
    description: 'TEST: Newsletter subscription landing page linked from blog posts',
    status: 'Draft',
    url: '/lp/test-newsletter',
  },
  webinar: {
    name: 'TEST_Webinar Registration Page',
    title: 'Exclusive Webinar: CRM Best Practices 2026',
    description: 'TEST: Webinar registration page for the Q2 2026 virtual event series',
    status: 'Draft',
    url: '/lp/test-webinar-q2-2026',
  },
};

// ============================================================================
// 31. TEST_WEB_FORMS
// ============================================================================

export const TEST_WEB_FORMS: Record<string, TestWebForm> = {
  contact_us: {
    name: 'TEST_Contact Us Form',
    title: 'Get in Touch with the TEST Team',
    description: 'TEST: General contact enquiry form embedded on the marketing website',
    entityType: 'Lead',
    fields: [
      { label: 'First Name',    type: 'text',   required: true  },
      { label: 'Last Name',     type: 'text',   required: true  },
      { label: 'Work Email',    type: 'email',  required: true  },
      { label: 'Company',       type: 'text',   required: false },
      { label: 'Message',       type: 'textarea',required: true },
    ],
  },
  lead_capture: {
    name: 'TEST_Lead Capture Form',
    title: 'Download Our Free TEST CRM Guide',
    description: 'TEST: Lead magnet capture form for ebook download gating',
    entityType: 'Lead',
    fields: [
      { label: 'Full Name',     type: 'text',      required: true  },
      { label: 'Business Email', type: 'email',    required: true  },
      { label: 'Job Title',     type: 'text',      required: false },
      { label: 'Company Size',  type: 'select',    required: false },
    ],
  },
  event_registration: {
    name: 'TEST_Event Registration Form',
    title: 'Register for TEST CRM Summit 2026',
    description: 'TEST: Event registration capturing attendee details for the annual summit',
    entityType: 'Contact',
    fields: [
      { label: 'First Name',    type: 'text',      required: true  },
      { label: 'Last Name',     type: 'text',      required: true  },
      { label: 'Email',         type: 'email',     required: true  },
      { label: 'Phone',         type: 'tel',       required: false },
      { label: 'Dietary Requirements', type: 'text', required: false },
    ],
  },
};

// ============================================================================
// 32. TEST_WEBHOOKS
// ============================================================================

export const TEST_WEBHOOKS: Record<string, TestWebhook> = {
  crm_events: {
    name: 'TEST_CRM Events Webhook',
    url: 'https://webhook-test.crm-test.local/crm-events',
    events: ['account.created', 'account.updated', 'contact.created', 'opportunity.closed'],
    secret: 'TEST_WEBHOOK_SECRET_CRM_EVENTS_2026',
    active: true,
  },
  payment_webhook: {
    name: 'TEST_Payment Events Webhook',
    url: 'https://webhook-test.crm-test.local/payments',
    events: ['payment.received', 'invoice.paid', 'subscription.cancelled'],
    secret: 'TEST_WEBHOOK_SECRET_PAYMENTS_2026',
    active: true,
  },
};

// ============================================================================
// 33. TEST_AGENTS
// ============================================================================

export const TEST_AGENTS: Record<string, TestAgent> = {
  lead_scorer: {
    name: 'TEST_Lead Scoring Agent',
    description: 'TEST: AI agent that analyses lead behaviour and assigns a quality score 0–100',
    type: 'Scoring',
    model: 'gpt-4o',
    systemPrompt: 'You are a CRM lead scoring assistant. Analyse the provided lead data and engagement signals to produce a numeric quality score from 0 to 100 with a brief justification. TEST context only.',
    tools: ['LeadPlugin', 'ActivityPlugin', 'ContactPlugin'],
    active: false,
  },
  support_triage: {
    name: 'TEST_Support Triage Agent',
    description: 'TEST: AI agent that auto-classifies incoming support tickets by priority, category, and suggested KB articles',
    type: 'Triage',
    model: 'gpt-4o',
    systemPrompt: 'You are a CRM support triage assistant. Classify incoming service requests by priority and category, and suggest relevant knowledge base articles. TEST context only.',
    tools: ['ServiceRequestPlugin', 'KnowledgeBasePlugin'],
    active: false,
  },
  sales_assistant: {
    name: 'TEST_Sales Assistant Agent',
    description: 'TEST: AI agent that helps sales reps craft personalised outreach emails and proposal summaries',
    type: 'Assistant',
    model: 'gpt-4o',
    systemPrompt: 'You are a CRM sales assistant. Help sales representatives write personalised outreach emails and summarise opportunity data. TEST context only.',
    tools: ['OpportunityPlugin', 'AccountPlugin', 'ContactPlugin'],
    active: false,
  },
  itsm_agent: {
    name: 'TEST_ITSM Ops Agent',
    description: 'TEST: AI agent for ITSM operations — root cause analysis, change risk assessment, and runbook suggestions',
    type: 'ITSM',
    model: 'gpt-4o',
    systemPrompt: 'You are an ITSM operations AI agent. Assist with incident root cause analysis, change risk scoring and runbook retrieval. TEST context only.',
    tools: ['IncidentPlugin', 'ProblemPlugin', 'ChangePlugin', 'CmdbPlugin', 'KnowledgeBasePlugin'],
    active: false,
  },
};

// ============================================================================
// 34. TEST_DUPLICATE_RULES
// ============================================================================

export const TEST_DUPLICATE_RULES: Record<string, TestDuplicateRule> = {
  account_email: {
    name: 'TEST_Account Duplicate by Email',
    entityType: 'Account',
    field: 'email',
    matchType: 'Exact',
    active: true,
  },
  contact_email: {
    name: 'TEST_Contact Duplicate by Email',
    entityType: 'Contact',
    field: 'email',
    matchType: 'Exact',
    active: true,
  },
};

// ============================================================================
// 35. TEST_LEAD_SCORE_RULES
// ============================================================================

export const TEST_LEAD_SCORE_RULES: Record<string, TestLeadScoreRule> = {
  email_engagement: {
    name: 'TEST_Score: Lead Opened Email',
    entityType: 'Lead',
    field: 'emailOpened',
    value: 'true',
    score: 10,
    active: true,
  },
  website_visit: {
    name: 'TEST_Score: Lead Visited Pricing Page',
    entityType: 'Lead',
    field: 'lastPageVisited',
    value: '/pricing',
    score: 20,
    active: true,
  },
  demo_request: {
    name: 'TEST_Score: Lead Requested Demo',
    entityType: 'Lead',
    field: 'source',
    value: 'Demo Request',
    score: 50,
    active: true,
  },
};

// ============================================================================
// 36. TEST_ESCALATION_POLICIES
// ============================================================================

export const TEST_ESCALATION_POLICIES: Record<string, TestEscalationPolicy> = {
  standard: {
    name: 'TEST_Standard Escalation Policy',
    description: 'TEST: Three-level escalation policy for standard service requests',
    levels: [
      { level: 1, notifyEmail: 'test_supportagent@crm-test.local', delayMinutes: 60  },
      { level: 2, notifyEmail: 'test_supportagent@crm-test.local', delayMinutes: 240 },
      { level: 3, notifyEmail: 'admin@crm.local',                  delayMinutes: 480 },
    ],
    active: true,
  },
  critical_response: {
    name: 'TEST_Critical Response Escalation Policy',
    description: 'TEST: Aggressive escalation policy for P1/Critical incidents',
    levels: [
      { level: 1, notifyEmail: 'test_supportagent@crm-test.local', delayMinutes: 10 },
      { level: 2, notifyEmail: 'admin@crm.local',                  delayMinutes: 30 },
    ],
    active: true,
  },
};

// ============================================================================
// 37. TEST_SERVICE_QUEUES
// ============================================================================

export const TEST_SERVICE_QUEUES: Record<string, TestServiceQueue> = {
  tier1: {
    name: 'TEST_Tier 1 Support Queue',
    description: 'TEST: First-line support queue for low and medium priority incidents',
    priority: 1,
    assignmentRule: 'RoundRobin',
    active: true,
  },
  tier2: {
    name: 'TEST_Tier 2 Support Queue',
    description: 'TEST: Second-line support queue for high priority and escalated incidents',
    priority: 2,
    assignmentRule: 'SkillBased',
    active: true,
  },
  escalation: {
    name: 'TEST_Escalation Queue',
    description: 'TEST: Critical incidents and unresolved escalations requiring senior engineer review',
    priority: 3,
    assignmentRule: 'Manual',
    active: true,
  },
};

// ============================================================================
// 38. TEST_GROUPS
// ============================================================================

export const TEST_GROUPS: Record<string, TestGroup> = {
  sales_managers: {
    name: 'TEST_Sales Managers Group',
    description: 'TEST: Group for sales managers — pipeline visibility and reporting permissions',
    permissions: ['read:accounts', 'write:accounts', 'read:opportunities', 'write:opportunities', 'read:reports'],
    members: ['test_salesrep@crm-test.local'],
  },
  support_agents: {
    name: 'TEST_Support Agents Group',
    description: 'TEST: Group for support agents — ticket management and ITSM access',
    permissions: ['read:service_requests', 'write:service_requests', 'read:knowledge_articles'],
    members: ['test_supportagent@crm-test.local', 'test_itsmagent@crm-test.local'],
  },
};

// ============================================================================
// 39. ADMIN_SETTINGS
// ============================================================================

export const ADMIN_SETTINGS: AdminSettings = {
  email: {
    senderEmail: 'test-noreply@crm-test.local',
    senderName:  'TEST CRM Platform',
    smtpHost:    'test-smtp.crm-test.local',
    smtpPort:    587,
  },
  branding: {
    companyName:  'TEST CRM Solutions Ltd',
    logoUrl:      'https://test-assets.crm-test.local/logo.png',
    primaryColor: '#1976D2',
  },
  session: {
    sessionTimeout:  60,
    maxConcurrent:   3,
  },
  security: {
    mfaRequired:        false,
    passwordMinLength:  8,
  },
};

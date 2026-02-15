// CRM Solution - E2E Tests using Playwright
// Comprehensive end-to-end workflow tests

import { test, expect } from '@playwright/test';

const BASE_URL = 'http://localhost:3000';

test.describe('ITSM Workflows', () => {
  
  test.beforeEach(async ({ page }) => {
    // Login before each test
    await page.goto(`${BASE_URL}/login`);
    await page.fill('input[type="email"]', 'admin@crm.local');
    await page.fill('input[type="password"]', 'Admin@123');
    await page.click('button:has-text("Login")');
    await page.waitForNavigation();
  });

  test('Complete Incident Workflow: Create → Investigate → Resolve', async ({ page }) => {
    // Navigate to incidents
    await page.goto(`${BASE_URL}/itsm/incidents`);

    // Create new incident
    await page.click('button:has-text("New Incident")');
    await page.fill('input[name="title"]', 'Database Connection Timeout');
    await page.fill('textarea[name="description"]', 'Database connection pool exhausted');
    await page.selectOption('select[name="priority"]', 'High');
    await page.click('button:has-text("Create")');

    // Verify incident created
    await expect(page.locator('text=Database Connection Timeout')).toBeVisible();

    // Assign incident
    await page.click('button:has-text("Assign")');
    await page.selectOption('select[name="assignee"]', '1'); // Select first technician
    await page.click('button:has-text("Confirm")');

    // Update status to In Progress
    await page.selectOption('select[name="status"]', 'In Progress');
    await page.click('button:has-text("Update")');

    // Add comment
    await page.fill('textarea[name="comment"]', 'Investigating connection pool settings');
    await page.click('button:has-text("Add Comment")');

    // Verify comment added
    await expect(page.locator('text=Investigating connection pool')).toBeVisible();

    // Resolve incident
    await page.selectOption('select[name="status"]', 'Resolved');
    await page.fill('textarea[name="resolution"]', 'Increased connection pool size');
    await page.click('button:has-text("Resolve")');

    // Verify resolved
    await expect(page.locator('text=Resolved')).toBeVisible({ timeout: 10000 });
  });

  test('Problem Management: Create → Analyze → Link Incidents', async ({ page }) => {
    // Navigate to problems
    await page.goto(`${BASE_URL}/itsm/problems`);

    // Create new problem
    await page.click('button:has-text("New Problem")');
    await page.fill('input[name="title"]', 'Database Connection Pool Issue');
    await page.fill('textarea[name="description"]', 'Insufficient connection pool configuration');
    await page.selectOption('select[name="priority"]', 'High');
    await page.click('button:has-text("Create")');

    // Verify problem created
    await expect(page.locator('text=Database Connection Pool Issue')).toBeVisible();

    // Add Root Cause Analysis
    await page.click('button:has-text("Add RCA")');
    await page.fill('textarea[name="rootCause"]', 'Configuration not optimized for load');
    await page.fill('textarea[name="preventionPlan"]', 'Implement proper connection pooling');
    await page.click('button:has-text("Save RCA")');

    // Link to incidents
    await page.click('button:has-text("Link Incidents")');
    await page.click('text=Incident 1'); // Select incidents to link
    await page.click('button:has-text("Link")');

    // Verify links created
    await expect(page.locator('text=Linked Incidents')).toBeVisible();
  });

  test('Change Management: Create → Approval → Implementation', async ({ page }) => {
    // Navigate to changes
    await page.goto(`${BASE_URL}/itsm/changes`);

    // Create new change
    await page.click('button:has-text("New Change")');
    await page.fill('input[name="title"]', 'Database Schema Update');
    await page.fill('textarea[name="description"]', 'Add new columns to users table');
    await page.selectOption('select[name="type"]', 'Normal');
    await page.selectOption('select[name="priority"]', 'Medium');
    await page.fill('input[name="scheduledDate"]', '2024-01-15');
    await page.click('button:has-text("Create")');

    // Verify change created
    await expect(page.locator('text=Database Schema Update')).toBeVisible();

    // Submit for approval
    await page.click('button:has-text("Submit for Approval")');
    await expect(page.locator('text=Pending Approval')).toBeVisible();

    // Add impact analysis
    await page.click('button:has-text("Add Impact")');
    await page.fill('input[name="component"]', 'User Service');
    await page.selectOption('select[name="impactLevel"]', 'High');
    await page.click('button:has-text("Add")');

    // Wait for approval (simulated)
    // In real scenario, another user would approve
    await page.click('button:has-text("Approve Change")');
    await expect(page.locator('text=Approved')).toBeVisible();

    // Schedule implementation
    await page.click('button:has-text("Schedule")');
    await expect(page.locator('text=Scheduled')).toBeVisible();
  });

  test('Incident Escalation → Manager Review → Resolution', async ({ page }) => {
    // Navigate to incidents
    await page.goto(`${BASE_URL}/itsm/incidents`);

    // Find high severity incident
    await page.selectOption('select[name="priority"]', 'High');
    await page.click('text=First incident in list').first();

    // Escalate incident
    await page.click('button:has-text("Escalate")');
    await page.selectOption('select[name="escalateTo"]', '2'); // Escalate to manager
    await page.fill('textarea[name="reason"]', 'Cannot resolve at level 1');
    await page.click('button:has-text("Escalate")');

    // Verify escalation
    await expect(page.locator('text=Escalated')).toBeVisible();

    // Add manager notes
    await page.fill('textarea[name="comment"]', 'Requires database admin access');
    await page.click('button:has-text("Add Comment")');

    // Resolve
    await page.selectOption('select[name="status"]', 'Resolved');
    await page.click('button:has-text("Resolve")');

    // Close
    await page.selectOption('select[name="status"]', 'Closed');
    await page.click('button:has-text("Close")');

    await expect(page.locator('text=Closed')).toBeVisible();
  });
});

test.describe('Sales Workflows', () => {
  
  test.beforeEach(async ({ page }) => {
    await page.goto(`${BASE_URL}/login`);
    await page.fill('input[type="email"]', 'admin@crm.local');
    await page.fill('input[type="password"]', 'Admin@123');
    await page.click('button:has-text("Login")');
    await page.waitForNavigation();
  });

  test('Commission Workflow: Plan → Calculation → Approval → Payout', async ({ page }) => {
    // Navigate to commissions
    await page.goto(`${BASE_URL}/sales/commissions`);

    // Create commission plan
    await page.click('button:has-text("New Plan")');
    await page.fill('input[name="planName"]', 'Q1 Sales Commission');
    await page.selectOption('select[name="type"]', 'FlatPercentage');
    await page.fill('input[name="rate"]', '0.05');
    await page.click('button:has-text("Create")');

    // Verify plan created
    await expect(page.locator('text=Q1 Sales Commission')).toBeVisible();

    // Assign plan to user
    await page.click('button:has-text("Assign")');
    await page.selectOption('select[name="user"]', '1');
    await page.click('button:has-text("Confirm")');

    // Calculate commission for deal
    await page.navigate(`${BASE_URL}/sales/deals`);
    await page.click('text=First deal').first();
    
    // Trigger commission calculation
    await page.click('button:has-text("Calculate Commission")');
    await expect(page.locator('text=Commission calculated')).toBeVisible();

    // Go back to commissions
    await page.goto(`${BASE_URL}/sales/commissions`);
    
    // Approve commission
    await page.click('button:has-text("Approve")');
    await expect(page.locator('text=Approved')).toBeVisible();

    // Mark as paid
    await page.click('button:has-text("Mark as Paid")');
    await expect(page.locator('text=Paid')).toBeVisible();
  });

  test('Order Fulfillment: Create → Track → Complete', async ({ page }) => {
    // Navigate to orders
    await page.goto(`${BASE_URL}/sales/orders`);

    // Create new order
    await page.click('button:has-text("New Order")');
    await page.selectOption('select[name="account"]', '1');
    await page.fill('input[name="amount"]', '1000');
    await page.fill('input[name="description"]', 'Software license renewal');
    await page.click('button:has-text("Create")');

    // Verify order created
    await expect(page.locator('text=Order created successfully')).toBeVisible();

    // Update order status
    await page.selectOption('select[name="status"]', 'Processing');
    await page.click('button:has-text("Update")');

    // Add shipment info
    await page.click('button:has-text("Add Shipment")');
    await page.fill('input[name="carrier"]', 'FedEx');
    await page.fill('input[name="trackingNumber"]', 'ABC123456');
    await page.click('button:has-text("Add")');

    // Verify shipment added
    await expect(page.locator('text=ABC123456')).toBeVisible();

    // Mark as delivered
    await page.selectOption('select[name="status"]', 'Delivered');
    await page.click('button:has-text("Update")');

    // Complete order
    await page.selectOption('select[name="status"]', 'Completed');
    await page.click('button:has-text("Update")');

    await expect(page.locator('text=Completed')).toBeVisible();
  });
});

test.describe('Integration Workflows', () => {
  
  test.beforeEach(async ({ page }) => {
    await page.goto(`${BASE_URL}/login`);
    await page.fill('input[type="email"]', 'admin@crm.local');
    await page.fill('input[type="password"]', 'Admin@123');
    await page.click('button:has-text("Login")');
    await page.waitForNavigation();
  });

  test('Webhook Configuration and Testing', async ({ page }) => {
    // Navigate to webhooks
    await page.goto(`${BASE_URL}/integration/webhooks`);

    // Create webhook
    await page.click('button:has-text("New Webhook")');
    await page.fill('input[name="url"]', 'https://webhook.example.com/events');
    await page.click('input[value="order.created"]');
    await page.click('input[value="contact.updated"]');
    await page.click('button:has-text("Create")');

    // Verify webhook created
    await expect(page.locator('text=https://webhook.example.com/events')).toBeVisible();

    // Test webhook delivery
    await page.click('button:has-text("Test")');
    await page.selectOption('select[name="eventType"]', 'order.created');
    await page.click('button:has-text("Send Test")');

    // Verify test result
    await expect(page.locator('text=Test sent successfully')).toBeVisible({ timeout: 10000 });

    // View delivery history
    await page.click('button:has-text("View History")');
    await expect(page.locator('text=Delivery History')).toBeVisible();

    // Verify delivery in history
    await expect(page.locator('text=Success')).toBeVisible();
  });

  test('Email Sequence Execution and Tracking', async ({ page }) => {
    // Navigate to email sequences
    await page.goto(`${BASE_URL}/marketing/sequences`);

    // Create sequence
    await page.click('button:has-text("New Sequence")');
    await page.fill('input[name="name"]', 'Welcome Email Series');
    await page.fill('input[name="steps"]', '3');
    await page.click('button:has-text("Create")');

    // Add sequence steps
    for (let i = 1; i <= 3; i++) {
      await page.click(`button:has-text("Add Step ${i}")`);
      await page.fill(`input[name="template_${i}"]`, `Email Template ${i}`);
      await page.fill(`input[name="delay_${i}"]`, `${i * 2}`);
      await page.click(`button:has-text("Add Step")`);
    }

    // Enroll contact
    await page.click('button:has-text("Enroll Contact")');
    await page.selectOption('select[name="contact"]', '1');
    await page.click('button:has-text("Enroll")');

    // Verify enrollment
    await expect(page.locator('text=Contact enrolled')).toBeVisible();

    // Start sequence
    await page.click('button:has-text("Start")');
    await expect(page.locator('text=Active')).toBeVisible();

    // View sequence status
    await page.click('button:has-text("View Status")');
    await expect(page.locator('text=Enrolled')).toBeVisible();

    // Track engagement
    await page.click('button:has-text("Track Engagement")');
    await expect(page.locator('text=Opens')).toBeVisible();
    await expect(page.locator('text=Clicks')).toBeVisible();
  });
});

test.describe('UI/UX Tests', () => {
  
  test('Responsive Layout on Mobile Devices', async ({ page }) => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });

    // Navigate to incidents
    await page.goto(`${BASE_URL}/login`);
    await page.fill('input[type="email"]', 'admin@crm.local');
    await page.fill('input[type="password"]', 'Admin@123');
    await page.click('button:has-text("Login")');
    await page.waitForNavigation();

    await page.goto(`${BASE_URL}/itsm/incidents`);

    // Verify mobile navigation
    await expect(page.locator('button[aria-label="Menu"]')).toBeVisible();

    // Verify content is readable
    await expect(page.locator('text=Incidents')).toBeVisible();
  });

  test('Accessibility Compliance', async ({ page }) => {
    // Navigate to page
    await page.goto(`${BASE_URL}/login`);

    // Check for proper heading hierarchy
    const headings = await page.locator('h1, h2, h3').count();
    expect(headings).toBeGreaterThan(0);

    // Check for aria labels
    const ariaLabels = await page.locator('[aria-label]').count();
    expect(ariaLabels).toBeGreaterThan(0);

    // Check for form labels
    const labels = await page.locator('label').count();
    expect(labels).toBeGreaterThan(0);
  });
});

test.describe('Performance Tests', () => {
  
  test('Page Load Performance', async ({ page }) => {
    // Measure page load time
    const startTime = Date.now();

    await page.goto(`${BASE_URL}/login`);

    const loadTime = Date.now() - startTime;

    // Page should load in less than 3 seconds
    expect(loadTime).toBeLessThan(3000);
  });

  test('Large List Rendering', async ({ page }) => {
    // Login
    await page.goto(`${BASE_URL}/login`);
    await page.fill('input[type="email"]', 'admin@crm.local');
    await page.fill('input[type="password"]', 'Admin@123');
    await page.click('button:has-text("Login")');
    await page.waitForNavigation();

    // Navigate to list with many items
    await page.goto(`${BASE_URL}/itsm/incidents`);

    // Measure render time
    const startTime = Date.now();

    // Scroll down to load more items
    await page.evaluate(() => window.scrollBy(0, window.innerHeight));

    const renderTime = Date.now() - startTime;

    // Should remain responsive
    expect(renderTime).toBeLessThan(1000);
  });
});

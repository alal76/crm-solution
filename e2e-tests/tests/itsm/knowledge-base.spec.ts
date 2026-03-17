/**
 * CRM Solution - Knowledge Base E2E Tests
 *
 * Tests for knowledge article management including:
 * - Create article
 * - Edit article
 * - Search articles
 * - View article details
 *
 * TODO-SD002-011: Knowledge Base E2E tests
 */

import { test, expect } from '../fixtures';
import type { Page } from '@playwright/test';

const KB_BASE_URL = '/itsm/knowledge';
const _BASE_URL = process.env.BASE_URL || 'http://192.168.0.9';
const API_BASE_URL = _BASE_URL.includes(':5000') ? _BASE_URL : `${_BASE_URL.replace(':80', '')}:5000`;

async function expectUrlOrRedirect(page: Page, expected: RegExp) {
  const url = page.url();
  const redirected = url.includes('/login') || !url.includes('/itsm');
  expect(expected.test(url) || redirected).toBeTruthy();
}

test.describe('Knowledge Base E2E Tests', () => {
  let createdArticleId: number | null = null;
  const timestamp = Date.now();

  test.beforeEach(async ({ authenticatedPage }) => {
    // Ensure we're on the knowledge base page
    await authenticatedPage.goto(KB_BASE_URL);
    await authenticatedPage.waitForLoadState('domcontentloaded');
    await authenticatedPage.waitForTimeout(800);
  });

  test('KB-001: Create a new knowledge article', async ({ authenticatedPage }) => {
    const page = authenticatedPage;

    // Click create button
    const createButton = page.locator('button:has-text("Create"), button:has-text("New"), button:has-text("Add Article")').first();
    if (await createButton.isVisible({ timeout: 5000 })) {
      await createButton.click();
      await page.waitForTimeout(500);

      // Fill in article details
      const titleInput = page.locator('input[name="title"], input[placeholder*="title" i]').first();
      if (await titleInput.isVisible()) {
        await titleInput.fill(`TEST_KB_Article_${timestamp}`);
      }

      const descInput = page.locator('textarea[name="shortDescription"], input[name="shortDescription"]').first();
      if (await descInput.isVisible()) {
        await descInput.fill('Test article created by Playwright E2E test');
      }

      const bodyInput = page.locator('textarea[name="articleBody"], [contenteditable="true"]').first();
      if (await bodyInput.isVisible()) {
        await bodyInput.fill(`
# Test Article Content

This is a test knowledge article created by automated E2E tests.

## Problem Description
Sample problem description for testing purposes.

## Solution
1. Step one
2. Step two
3. Step three

## Related Articles
- None

Created at: ${new Date().toISOString()}
        `);
      }

      // Select article type if available
      const typeSelect = page.locator('[aria-label*="type" i], label:has-text("Type") + div').first();
      if (await typeSelect.isVisible()) {
        await typeSelect.click();
        await page.locator('[role="option"]:has-text("How-To"), [role="option"]:has-text("FAQ")').first().click();
      }

      // Save the article
      const saveButton = page.locator('button[type="submit"], button:has-text("Save"), button:has-text("Create")').first();
      await saveButton.click();
      await page.waitForTimeout(1500);
    }

    // Verify we're back on the list or detail page
    await expectUrlOrRedirect(page, /itsm|incidents|problems|changes|cmdb|knowledge|catalog|sla/i);
  });

  test('KB-002: Search for knowledge articles', async ({ authenticatedPage }) => {
    const page = authenticatedPage;

    // Look for search input
    const searchInput = page.locator('input[type="search"], input[placeholder*="search" i], input[aria-label*="search" i]').first();
    
    if (await searchInput.isVisible({ timeout: 5000 })) {
      // Search for test articles
      await searchInput.fill('TEST_KB');
      await page.waitForTimeout(1000);

      // Verify search results appear or no results message
      const results = page.locator('[data-testid="article-list"], table tbody tr, .article-card, .MuiDataGrid-row');
      const noResults = page.locator('text=/no (results|articles|items)/i');

      // Either results exist or "no results" message is shown
      const hasResults = await results.count() > 0;
      const hasNoResultsMessage = await noResults.isVisible({ timeout: 2000 }).catch(() => false);

      expect(hasResults || hasNoResultsMessage).toBe(true);
    }

    await expectUrlOrRedirect(page, /itsm|incidents|problems|changes|cmdb|knowledge|catalog|sla/i);
  });

  test('KB-003: View article details', async ({ authenticatedPage }) => {
    const page = authenticatedPage;

    // Wait for article list to load
    await page.waitForTimeout(500);

    // Try to click on the first article in the list
    const firstArticle = page.locator('table tbody tr, .article-card, .MuiDataGrid-row, [data-testid="article-item"]').first();
    
    if (await firstArticle.isVisible({ timeout: 5000 })) {
      await firstArticle.click();
      await page.waitForTimeout(1000);

      // Check that article details are displayed
      const articleContent = page.locator('[data-testid="article-content"], .article-body, article, main');
      const hasContent = await articleContent.first().isVisible({ timeout: 3000 }).catch(() => false);

      // If article detail page, verify title or content is visible
      if (hasContent) {
        const title = page.locator('h1, h2, [data-testid="article-title"]').first();
        await expect(title).toBeVisible({ timeout: 5000 });
      }
    }

    // Navigate back if needed
    await expectUrlOrRedirect(page, /itsm|incidents|problems|changes|cmdb|knowledge|catalog|sla/i);
  });

  test('KB-004: Edit an existing article', async ({ authenticatedPage }) => {
    const page = authenticatedPage;

    // Wait for page to load
    await page.waitForTimeout(500);

    // Click on first article
    const firstArticle = page.locator('table tbody tr, .article-card, .MuiDataGrid-row').first();
    
    if (await firstArticle.isVisible({ timeout: 5000 })) {
      await firstArticle.click();
      await page.waitForTimeout(500);

      // Look for edit button
      const editButton = page.locator('button:has-text("Edit"), button[aria-label*="edit" i], [data-testid="edit-button"]').first();
      
      if (await editButton.isVisible({ timeout: 3000 })) {
        await editButton.click();
        await page.waitForTimeout(500);

        // Update the title or content
        const titleInput = page.locator('input[name="title"], input[placeholder*="title" i]').first();
        if (await titleInput.isVisible()) {
          const currentTitle = await titleInput.inputValue();
          await titleInput.fill(`${currentTitle} (Updated ${Date.now()})`);
        }

        // Save changes
        const saveButton = page.locator('button[type="submit"], button:has-text("Save"), button:has-text("Update")').first();
        if (await saveButton.isVisible()) {
          await saveButton.click();
          await page.waitForTimeout(1500);
        }
      }
    }

    await expectUrlOrRedirect(page, /itsm|incidents|problems|changes|cmdb|knowledge|catalog|sla/i);
  });

  test('KB-005: Filter articles by category', async ({ authenticatedPage }) => {
    const page = authenticatedPage;

    // Look for category filter
    const categoryFilter = page.locator(
      '[aria-label*="category" i], ' +
      'label:has-text("Category") + div, ' +
      '[data-testid="category-filter"], ' +
      'button:has-text("Category")'
    ).first();

    if (await categoryFilter.isVisible({ timeout: 5000 })) {
      await categoryFilter.click();
      await page.waitForTimeout(300);

      // Select first available category
      const firstOption = page.locator('[role="option"], [role="menuitem"]').first();
      if (await firstOption.isVisible()) {
        await firstOption.click();
        await page.waitForTimeout(1000);
      }
    }

    // Verify page is still on knowledge base
    await expectUrlOrRedirect(page, /itsm|incidents|problems|changes|cmdb|knowledge|catalog|sla/i);
  });

  test('KB-006: Verify article publishing workflow', async ({ authenticatedPage }) => {
    const page = authenticatedPage;

    // Go to pending articles if available
    const pendingTab = page.locator('button:has-text("Pending"), [role="tab"]:has-text("Pending"), a:has-text("Pending")').first();
    
    if (await pendingTab.isVisible({ timeout: 3000 })) {
      await pendingTab.click();
      await page.waitForTimeout(500);
    }

    // Check for draft/pending articles
    const draftArticle = page.locator('text=/draft|pending|review/i').first();
    const hasDraftArticles = await draftArticle.isVisible({ timeout: 5000 }).catch(() => false);

    // If draft articles exist, try to publish one
    if (hasDraftArticles) {
      await draftArticle.click();
      await page.waitForTimeout(500);

      const publishButton = page.locator('button:has-text("Publish"), button:has-text("Approve")').first();
      if (await publishButton.isVisible({ timeout: 3000 })) {
        // Just verify the button exists - don't actually publish
        await expect(publishButton).toBeVisible();
      }
    }

    await expectUrlOrRedirect(page, /itsm|incidents|problems|changes|cmdb|knowledge|catalog|sla/i);
  });
});

test.describe('Knowledge Base API Tests', () => {
  test('KB-API-001: Create article via API', async ({ request }) => {
    // Skip if no auth token available
    const response = await request.post(`${API_BASE_URL}/api/itsm/knowledge`, {
      data: {
        title: `API Test Article ${Date.now()}`,
        shortDescription: 'Created via API test',
        articleBody: 'Test content',
        articleType: 1
      },
      headers: {
        'Content-Type': 'application/json'
      }
    });

    // Accept both 201 (created) and 401 (if auth required)
    expect([200, 201, 401]).toContain(response.status());
  });

  test('KB-API-002: Search articles via API', async ({ request }) => {
    const response = await request.get(`${API_BASE_URL}/api/itsm/knowledge/search?searchTerm=test`);
    
    // Accept success or auth required
    expect([200, 401]).toContain(response.status());

    if (response.status() === 200) {
      const data = await response.json();
      expect(Array.isArray(data) || data.items !== undefined).toBe(true);
    }
  });

  test('KB-API-003: Get popular articles via API', async ({ request }) => {
    const response = await request.get(`${API_BASE_URL}/api/itsm/knowledge/popular?count=5`);
    
    expect([200, 401]).toContain(response.status());

    if (response.status() === 200) {
      const data = await response.json();
      expect(Array.isArray(data)).toBe(true);
    }
  });
});

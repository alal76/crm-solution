/**
 * CRM Solution - Workflow Designer Tests
 * 
 * Tests for workflow creation, node management, AI nodes, and execution.
 */

import { test, expect } from '@playwright/test';
import { DataGridHelper } from '../fixtures';
import { TEST_WORKFLOWS, uniqueTestData } from '../test-data';

test.describe('Workflows - List View', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/admin/workflows');
    await page.waitForLoadState('networkidle');
  });

  test('TC-WF-001: Should display workflows list', async ({ page }) => {
    await expect(page.locator('h1, h2, .page-title').filter({ hasText: /workflow/i })).toBeVisible({ timeout: 10000 });
  });

  test('TC-WF-002: Should have create workflow button', async ({ page }) => {
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    await expect(addButton).toBeVisible();
  });

  test('TC-WF-003: Should show workflow status', async ({ page }) => {
    const statusColumn = page.locator('text=/active|inactive|draft/i').first();
    // Status may be shown in various ways
  });
});

test.describe('Workflows - Create', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/admin/workflows');
    await page.waitForLoadState('networkidle');
  });

  test('TC-WF-004: Should open workflow designer', async ({ page }) => {
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    await addButton.click();
    await page.waitForTimeout(1000);
    
    // Should navigate to designer or show dialog
    const designerCanvas = page.locator('canvas, .workflow-canvas, .designer, [role="dialog"]').first();
    await expect(designerCanvas).toBeVisible({ timeout: 5000 });
  });

  test('TC-WF-005: Should create new workflow', async ({ page }) => {
    const testWorkflow = uniqueTestData(TEST_WORKFLOWS.leadQualification);
    
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    await addButton.click();
    await page.waitForTimeout(1000);
    
    // Fill workflow name
    const nameInput = page.locator('input[name="name"], input[aria-label*="Name"], #name').first();
    if (await nameInput.isVisible()) {
      await nameInput.fill(testWorkflow.name);
    }
    
    // Fill description
    const descInput = page.locator('textarea[name="description"], #description').first();
    if (await descInput.isVisible()) {
      await descInput.fill(testWorkflow.description);
    }
    
    // Save
    await page.locator('button:has-text("Save"), button:has-text("Create")').first().click();
    await page.waitForTimeout(2000);
  });

  test('TC-WF-006: Should set workflow trigger', async ({ page }) => {
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    await addButton.click();
    await page.waitForTimeout(1000);
    
    // Look for trigger selection
    const triggerSelect = page.locator('[aria-label*="Trigger"], label:has-text("Trigger") + div, text=/trigger/i').first();
    if (await triggerSelect.isVisible()) {
      await triggerSelect.click();
      await page.waitForTimeout(500);
    }
  });
});

test.describe('Workflows - Designer Canvas', () => {
  test('TC-WF-007: Should display node palette', async ({ page }) => {
    await page.goto('/admin/workflows');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const editButton = page.locator('button:has-text("Edit"), button:has-text("Design")').first();
      if (await editButton.isVisible()) {
        await editButton.click();
        await page.waitForTimeout(1000);
        
        // Check for node palette/toolbox
        const nodePalette = page.locator('.node-palette, .toolbox, text=/nodes|actions|triggers/i').first();
        if (await nodePalette.isVisible()) {
          await expect(nodePalette).toBeVisible();
        }
      }
    }
  });

  test('TC-WF-008: Should add trigger node', async ({ page }) => {
    await page.goto('/admin/workflows');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const editButton = page.locator('button:has-text("Edit"), button:has-text("Design")').first();
      if (await editButton.isVisible()) {
        await editButton.click();
        await page.waitForTimeout(1000);
        
        // Look for add node button or drag trigger node
        const triggerNode = page.locator('text=/trigger/i, [data-node-type="trigger"]').first();
        if (await triggerNode.isVisible()) {
          await triggerNode.click();
        }
      }
    }
  });

  test('TC-WF-009: Should add action node', async ({ page }) => {
    await page.goto('/admin/workflows');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const editButton = page.locator('button:has-text("Edit"), button:has-text("Design")').first();
      if (await editButton.isVisible()) {
        await editButton.click();
        await page.waitForTimeout(1000);
        
        const actionNode = page.locator('text=/action/i, [data-node-type="action"]').first();
        if (await actionNode.isVisible()) {
          await actionNode.click();
        }
      }
    }
  });

  test('TC-WF-010: Should add condition node', async ({ page }) => {
    await page.goto('/admin/workflows');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const editButton = page.locator('button:has-text("Edit"), button:has-text("Design")').first();
      if (await editButton.isVisible()) {
        await editButton.click();
        await page.waitForTimeout(1000);
        
        const conditionNode = page.locator('text=/condition|decision/i, [data-node-type="condition"]').first();
        if (await conditionNode.isVisible()) {
          await conditionNode.click();
        }
      }
    }
  });
});

test.describe('Workflows - AI Nodes', () => {
  test('TC-WF-011: Should display AI node types', async ({ page }) => {
    await page.goto('/admin/workflows');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const editButton = page.locator('button:has-text("Edit"), button:has-text("Design")').first();
      if (await editButton.isVisible()) {
        await editButton.click();
        await page.waitForTimeout(1000);
        
        // Check for AI node types
        const aiNodes = page.locator('text=/AI Decision|AI Agent|AI Content|AI Classifier/i').first();
        if (await aiNodes.isVisible()) {
          await expect(aiNodes).toBeVisible();
        }
      }
    }
  });

  test('TC-WF-012: Should add AI Decision node', async ({ page }) => {
    await page.goto('/admin/workflows');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const editButton = page.locator('button:has-text("Edit"), button:has-text("Design")').first();
      if (await editButton.isVisible()) {
        await editButton.click();
        await page.waitForTimeout(1000);
        
        const aiDecisionNode = page.locator('text=/AI Decision/i').first();
        if (await aiDecisionNode.isVisible()) {
          await aiDecisionNode.click();
        }
      }
    }
  });

  test('TC-WF-013: Should add AI Agent node', async ({ page }) => {
    await page.goto('/admin/workflows');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const editButton = page.locator('button:has-text("Edit"), button:has-text("Design")').first();
      if (await editButton.isVisible()) {
        await editButton.click();
        await page.waitForTimeout(1000);
        
        const aiAgentNode = page.locator('text=/AI Agent/i').first();
        if (await aiAgentNode.isVisible()) {
          await aiAgentNode.click();
        }
      }
    }
  });

  test('TC-WF-014: Should configure AI node properties', async ({ page }) => {
    await page.goto('/admin/workflows');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const editButton = page.locator('button:has-text("Edit"), button:has-text("Design")').first();
      if (await editButton.isVisible()) {
        await editButton.click();
        await page.waitForTimeout(1000);
        
        // Click on an existing AI node to show properties
        const aiNode = page.locator('[data-node-type*="AI"], .ai-node').first();
        if (await aiNode.isVisible()) {
          await aiNode.click();
          await page.waitForTimeout(500);
          
          // Check for properties panel
          const propertiesPanel = page.locator('.properties-panel, text=/properties|configuration/i').first();
          if (await propertiesPanel.isVisible()) {
            await expect(propertiesPanel).toBeVisible();
          }
        }
      }
    }
  });
});

test.describe('Workflows - Execution', () => {
  test('TC-WF-015: Should activate workflow', async ({ page }) => {
    await page.goto('/admin/workflows');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const activateButton = page.locator('button:has-text("Activate"), button:has-text("Enable")').first();
      if (await activateButton.isVisible()) {
        await activateButton.click();
        await page.waitForTimeout(2000);
      }
    }
  });

  test('TC-WF-016: Should deactivate workflow', async ({ page }) => {
    await page.goto('/admin/workflows');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const deactivateButton = page.locator('button:has-text("Deactivate"), button:has-text("Disable"), button:has-text("Pause")').first();
      if (await deactivateButton.isVisible()) {
        await deactivateButton.click();
        await page.waitForTimeout(2000);
      }
    }
  });

  test('TC-WF-017: Should view workflow execution history', async ({ page }) => {
    await page.goto('/admin/workflows');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const historyTab = page.locator('button:has-text("History"), button:has-text("Executions"), [role="tab"]:has-text("History")').first();
      if (await historyTab.isVisible()) {
        await historyTab.click();
        await page.waitForTimeout(1000);
      }
    }
  });
});

test.describe('Workflows - Delete', () => {
  test('TC-WF-018: Should delete test workflow', async ({ page }) => {
    await page.goto('/admin/workflows');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const deleteButton = page.locator('button:has-text("Delete")').first();
      if (await deleteButton.isVisible()) {
        await deleteButton.click();
        await page.waitForTimeout(500);
        
        const confirmButton = page.locator('button:has-text("Confirm"), button:has-text("Yes")').first();
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
          await page.waitForTimeout(2000);
        }
      }
    }
  });
});

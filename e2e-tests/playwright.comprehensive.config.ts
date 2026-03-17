/// <reference types="node" />
import { defineConfig, devices } from '@playwright/test';
import { WEB_BASE_URL } from './testConfig';

/**
 * CRM Solution - Playwright Comprehensive Test Configuration
 *
 * Runs comprehensive (non-BVT) e2e tests against Chromium and Firefox only.
 * Mobile Safari is excluded because macOS WebKit cannot reach private-network
 * IPs (192.168.0.x) due to OS-level network sandbox restrictions.
 * Mobile Chrome (Pixel 5 emulation) is excluded for the same reason.
 * Desktop Safari (webkit) is excluded as it is flaky on private-IP targets.
 *
 * Usage:  npm run test:comprehensive
 */
export default defineConfig({
  testDir: './tests',

  /* Run tests in files serially by default for deterministic ordering */
  fullyParallel: false,

  /* Fail the build on CI if test.only is accidentally committed */
  forbidOnly: !!process.env.CI,

  /* Retry on CI only */
  retries: process.env.CI ? 1 : 0,

  /* Single worker keeps serial-mode suites well-behaved */
  workers: 1,

  /* Use same reporters as base config */
  reporter: [
    ['./tests/utils/crm-reporter.ts', { outputDir: 'test-logs' }],
    ['html', { outputFolder: 'test-results/html-report-comprehensive' }],
    ['json', { outputFile: 'test-results/results-comprehensive.json' }],
    ['junit', { outputFile: 'test-results/junit-comprehensive.xml' }],
    ['list'],
  ],

  use: {
    baseURL: WEB_BASE_URL,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'on-first-retry',
    actionTimeout: 10000,
    /* Keep 30s navigation timeout so auth/login redirect flows work.
     * Individual tests that call waitForLoadState('networkidle') must pass
     * an explicit short timeout, e.g. { timeout: 5000 }, to avoid burning
     * the 60s test budget while waiting for SignalR to settle.              */
    navigationTimeout: 30000,
  },

  /* Per-test timeout — CRUD tests need more time (form fill + network ops)  */
  timeout: 120000,

  expect: {
    timeout: 10000,
  },

  /* Output folder for test artifacts */
  outputDir: 'test-results/artifacts',

  projects: [
    /* Setup project - runs authentication and stores state */
    {
      name: 'setup',
      testMatch: /.*\.setup\.ts/,
    },

    /* Desktop Chrome */
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
        storageState: 'test-results/.auth/user.json',
      },
      dependencies: ['setup'],
    },

    /* Desktop Firefox */
    {
      name: 'firefox',
      use: {
        ...devices['Desktop Firefox'],
        storageState: 'test-results/.auth/user.json',
      },
      dependencies: ['setup'],
    },
  ],
});

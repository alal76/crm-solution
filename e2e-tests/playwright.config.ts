import { defineConfig, devices } from '@playwright/test';

/**
 * CRM Solution - Playwright E2E Test Configuration
 * 
 * This configuration supports running tests against a live CRM instance.
 * Set BASE_URL environment variable to target different environments.
 */
export default defineConfig({
  testDir: './tests',
  
  /* Run tests in parallel */
  fullyParallel: true,
  
  /* Fail the build on CI if you accidentally left test.only in the source code */
  forbidOnly: !!process.env.CI,
  
  /* Retry on CI only */
  retries: process.env.CI ? 2 : 0,
  
  /* Opt out of parallel tests on CI */
  workers: process.env.CI ? 1 : undefined,
  
  /* Reporter to use - Custom CRM reporter plus standard reporters */
  reporter: [
    ['./tests/utils/crm-reporter.ts', { outputDir: 'test-logs' }],
    ['html', { outputFolder: 'test-results/html-report' }],
    ['json', { outputFile: 'test-results/results.json' }],
    ['junit', { outputFile: 'test-results/junit.xml' }],
    ['list']
  ],
  
  /* Shared settings for all the projects below */
  use: {
    /* Base URL to use in actions like `await page.goto('/')` */
    baseURL: process.env.BASE_URL || 'http://192.168.0.9',
    
    /* Collect trace when retrying the failed test */
    trace: 'on-first-retry',
    
    /* Take screenshot on failure */
    screenshot: 'only-on-failure',
    
    /* Video recording on failure */
    video: 'on-first-retry',
    
    /* Default timeout for actions */
    actionTimeout: 10000,
    
    /* Default navigation timeout */
    navigationTimeout: 30000,
  },
  
  /* Global timeout for each test */
  timeout: 60000,
  
  /* Expect timeout */
  expect: {
    timeout: 10000,
  },

  /* Configure projects for major browsers */
  projects: [
    /* Setup project - runs authentication and stores state */
    {
      name: 'setup',
      testMatch: /.*\.setup\.ts/,
    },
    
    /* Desktop Chrome tests */
    {
      name: 'chromium',
      use: { 
        ...devices['Desktop Chrome'],
        storageState: 'test-results/.auth/user.json',
      },
      dependencies: ['setup'],
    },

    /* Desktop Firefox tests */
    {
      name: 'firefox',
      use: { 
        ...devices['Desktop Firefox'],
        storageState: 'test-results/.auth/user.json',
      },
      dependencies: ['setup'],
    },

    /* Desktop Safari tests */
    {
      name: 'webkit',
      use: { 
        ...devices['Desktop Safari'],
        storageState: 'test-results/.auth/user.json',
      },
      dependencies: ['setup'],
    },

    /* Mobile Chrome tests */
    {
      name: 'Mobile Chrome',
      use: { 
        ...devices['Pixel 5'],
        storageState: 'test-results/.auth/user.json',
      },
      dependencies: ['setup'],
    },

    /* Mobile Safari tests */
    {
      name: 'Mobile Safari',
      use: { 
        ...devices['iPhone 12'],
        storageState: 'test-results/.auth/user.json',
      },
      dependencies: ['setup'],
    },
  ],

  /* Output folder for test artifacts */
  outputDir: 'test-results/artifacts',
});

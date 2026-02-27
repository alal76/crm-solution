/// <reference types="node" />
import { defineConfig } from '@playwright/test';

/**
 * BVT-only Playwright config — no browser auth setup needed.
 * BVT tests use Playwright's request API context (not browser).
 */
export default defineConfig({
  testDir: './tests/bvt',
  fullyParallel: false,
  retries: 1,
  workers: 1,
  reporter: [['list']],
  use: {
    baseURL: process.env.BASE_URL || 'http://192.168.0.9:5000',
  },
  timeout: 60000,
  expect: { timeout: 10000 },
  projects: [
    {
      name: 'bvt',
      testMatch: /.*\.spec\.ts/,
    },
  ],
  outputDir: 'test-results/bvt-artifacts',
});

/// <reference types="node" />
import { defineConfig } from '@playwright/test';
import { API_BASE_URL } from './testConfig';

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
    baseURL: API_BASE_URL,
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

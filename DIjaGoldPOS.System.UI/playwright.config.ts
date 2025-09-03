/// <reference types="node" />
import { defineConfig, devices } from '@playwright/test';

// UI base URL where the React app runs (e.g., http://localhost:3000)
const UI_BASE_URL = process.env.E2E_UI_URL || 'http://localhost:3000';
// API base URL for direct API checks (e.g., https://localhost:50866/api)
const API_BASE_URL = process.env.E2E_API_URL || 'https://localhost:50866/api';

export default defineConfig({
  testDir: 'tests/e2e',
  timeout: 60_000,
  expect: { timeout: 10_000 },
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 2 : undefined,
  reporter: [['html', { open: 'never' }], ['list']],
  use: {
    baseURL: UI_BASE_URL,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    extraHTTPHeaders: {
      // Allow self-signed certs in dev by not sending special headers
    },
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
  ],
  metadata: {
    uiBaseUrl: UI_BASE_URL,
    apiBaseUrl: API_BASE_URL,
  },
});

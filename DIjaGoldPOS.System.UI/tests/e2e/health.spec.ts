import { test, expect, request } from '@playwright/test';

// Uses API base URL from config metadata
const apiBaseUrl = (test.info().config.metadata as any).apiBaseUrl as string;

// Basic API health check so CI can verify connectivity without UI selectors
// This should pass as soon as API is running and CORS allows GET to /api/health/simple
// If your API uses a different port, set E2E_API_URL env: e.g. https://localhost:50866/api

test('API health simple is OK', async ({}) => {
  const api = await request.newContext({ ignoreHTTPSErrors: true });
  const res = await api.get(`${apiBaseUrl}/health/simple`);
  expect(res.ok()).toBeTruthy();
});

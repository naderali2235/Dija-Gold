/// <reference types="node" />
import { test, expect } from '@playwright/test';

// Uses baseURL from playwright.config (E2E_UI_URL)
// Provide E2E_USER and E2E_PASS in env for real credentials

test.describe('Authentication flow', () => {
  test('User can login successfully', async ({ page }) => {
    await page.goto('/login');

    // Fill credentials using stable data-testids
    await page.fill('[data-testid="username"]', process.env.E2E_USER || 'admin');
    await page.fill('[data-testid="password"]', process.env.E2E_PASS || 'password');
    await page.click('[data-testid="login-submit"]');

    // Post-login: ensure we left the login screen and no error is shown
    await expect(page).not.toHaveURL(/\/login(\b|$)/);
    await expect(page.locator('[data-testid="login-submit"]')).toHaveCount(0);
    await expect(page.locator('[data-testid="login-error"]')).toHaveCount(0);
  });
});

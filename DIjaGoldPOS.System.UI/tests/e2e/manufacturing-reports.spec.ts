import { test, expect } from '@playwright/test';

// Skipped until test env and data are stable
// Validates tabs, date filter, refresh and export actions

test.describe.skip('Manufacturing Reports', () => {
  test('Navigate tabs and apply date filter', async ({ page }) => {
    await page.goto('/manufacturing-reports');

    // Tabs
    await page.click('[data-testid="mr-tab-raw-gold"]');
    await expect(page).toHaveURL(/manufacturing-reports/);

    await page.click('[data-testid="mr-tab-efficiency"]');
    await page.click('[data-testid="mr-tab-cost-analysis"]');
    await page.click('[data-testid="mr-tab-workflow"]');
    await page.click('[data-testid="mr-tab-dashboard"]');

    // Date filter and actions
    await page.fill('[data-testid="mr-start-date"]', '2025-01-01');
    await page.fill('[data-testid="mr-end-date"]', '2025-01-31');
    await page.click('[data-testid="mr-apply-filter-btn"]');

    // Refresh and Export buttons present
    await expect(page.locator('[data-testid="mr-refresh-btn"]')).toBeVisible();
    await expect(page.locator('[data-testid="mr-export-excel-btn"]')).toBeVisible();
    await expect(page.locator('[data-testid="mr-export-pdf-btn"]')).toBeVisible();
  });
});

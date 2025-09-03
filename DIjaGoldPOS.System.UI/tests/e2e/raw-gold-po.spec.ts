import { test, expect } from '@playwright/test';

// Skipped until real selectors/routes are confirmed
// Covers creating a raw gold purchase order flow

test.describe.skip('Raw Gold Purchase Orders', () => {
  test('Open raw gold PO dialog and add item', async ({ page }) => {
    // Assumes user is already authenticated or auth is disabled in test env
    await page.goto('/purchase-orders');

    // Open create dialog for raw gold
    await page.click('[data-testid="new-raw-gold-po-btn"]');
    await expect(page.locator('[data-testid="raw-gold-po-dialog"]')).toBeVisible();

    // Add an item row
    await page.click('[data-testid="raw-po-add-item-btn"]');

    // For now, we stop before submitting since field-level testids are not yet wired
    // and creation requires valid selections and values.
  });
});

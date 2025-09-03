import { test, expect } from '@playwright/test';

// Skipped until real selectors/routes are confirmed
// Covers navigating to products, searching/filtering, opening a product, and pricing call

test.describe.skip('Products management', () => {
  test('View list, filter, and verify pricing display', async ({ page }) => {
    await page.goto('/products');

    // Use stable data-testid selectors
    await page.fill('[data-testid="product-search"]', 'ring');
    await page.click('[data-testid="product-search-btn"]');

    await page.waitForSelector('[data-testid="product-row"]');
    const first = page.locator('[data-testid="product-row"]').first();
    await first.click();

    await expect(page).toHaveURL(/\/products\//);

    // Validate pricing display is visible for the selected product
    await expect(page.locator('[data-testid="pricing-total"]').first()).toBeVisible();
  });
});

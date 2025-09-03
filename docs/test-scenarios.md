# DijaGoldPOS Test Scenarios

This document provides step-by-step test scenarios for validating the DijaGold POS system end-to-end. Use it for manual QA runs or to derive automated E2E tests.

- App: DijaGoldPOS.System.UI (React/TS)
- API: DijaGoldPOS.API (.NET)
- Base API URL: configurable via `REACT_APP_API_URL` (commonly `https://localhost:50866`)

Note: Many API controllers use PascalCase routes (e.g., `/api/ProductManufacture`, `/api/RawGoldPurchaseOrders`, `/api/ManufacturingReports`). Ensure UI/API integration respects these paths.

---

## 0) Prerequisites & Environment
1. Ensure API is running and reachable at configured base URL.
2. TLS: Accept dev certificate if using HTTPS locally.
3. Seed/admin user exists, or create via API/DB.
4. CORS allows UI origin.
5. Confirm health checks:
   - GET `/api/health/simple` returns 200
   - GET `/api/health` returns 200 and services healthy
6. UI loads without console errors on `/` and `/login`.

---

## 1) Smoke Test (Happy Path)
1. Navigate to `/login`.
2. Login with valid admin credentials.
3. Land on dashboard without errors.
4. Open Suppliers and Products pages.
5. Create a regular Purchase Order (PO) and a Raw Gold PO (see sections 6 and 7).
6. Receive the POs.
7. Create a basic Manufacturing record consuming raw gold from inventory.
8. View Manufacturing Reports dashboard.
9. Log out.

Expected: No errors, visible confirmations/toasts, data persists across reloads.

---

## 2) Authentication
1. Invalid login: wrong password
   - Expect inline error and no token stored.
2. Valid login
   - Expect JWT stored (localStorage) and redirect to dashboard.
3. Token persistence
   - Refresh page; user remains authenticated.
4. Logout
   - Token cleared; redirected to `/login`.

---

## 3) Lookups & Settings
1. Categories & Subcategories
   - Pick a Category; Subcategory dropdown populates via `getSubCategories(categoryId)`.
   - Select "None" option for optional subcategory.
2. Pricing settings
   - Update making charges by category/subcategory.
   - Save and reload; values persist and display correctly.

---

## 4) Suppliers
1. Create supplier with required fields.
2. Edit supplier details.
3. Search/filter suppliers by name or phone.
4. Delete/soft-delete supplier (if supported) and verify status.

Expected: UI feedback, duplicate validation, required field errors.

---

## 5) Products
1. Create product with category + subcategory (using dropdowns).
2. Edit product attributes (price, karat, etc.).
3. Validate required fields and formatting.
4. Search and filter by category/subcategory.

---

## 6) Regular Purchase Orders
1. Create PO
   - Choose supplier, branch, notes.
   - Add line items for products with qty/price.
   - Submit; expect success toast and PO appears in list.
2. View PO details
   - Verify totals, statuses, and items table.
3. Receive PO
   - Partially receive items; check status transitions and remaining quantities.
   - Fully receive; PO status becomes Completed.
4. Negative
   - Over-receive prevention.
   - Invalid values (negative qty/price) show validation errors.

---

## 7) Raw Gold Purchase Orders
1. From Suppliers or Purchase Orders page, click "Raw Gold" creation (gold Zap icon).
2. Create Raw Gold PO
   - Choose supplier, branch, karat type per item, ordered weight.
   - Submit; expect success and Raw Gold badge in list.
3. Filter order list by type = Raw Gold; ensure visibility.
4. Receive Raw Gold PO
   - Provide weights received per item; cannot exceed ordered.
   - Inventory movement created; weighted average cost updated.
   - Status transitions to Completed when fully received.
5. Regression (routing)
   - Endpoints used must be PascalCase: `/api/RawGoldPurchaseOrders` (Create/Get/ById/Status/Receive).

---

## 8) Manufacturing (Product Manufacture)
1. Create manufacturing record
   - Select product to manufacture.
   - Add raw material sources:
     - Link to raw gold purchase order items (multiple sources allowed) with contribution percentages/weights.
   - Submit and verify creation.
2. Workflow transitions
   - Perform transitions: start -> in_progress -> quality_check -> final_approval (as configured).
   - Check available transitions and workflow history endpoints.
3. Validation
   - Cannot consume more weight than available from any raw gold source.
   - Proper error messages on exceeding limits.
4. Regression (routing)
   - Use `/api/ProductManufacture` PascalCase endpoints for CRUD and workflow.

---

## 9) Product Ownership, Consolidation & Costing
1. Ownership view
   - Open Product Ownership page; verify records list and actions.
2. Consolidation
   - Trigger "Consolidate Ownership"; consolidate records for same supplier/product.
   - Verify consolidation result summary and post-state records.
3. Cost analysis
   - Run weighted average costing for a product or record.
   - Verify cost breakdown and ability to update product cost using the result.
4. Alternative costing comparisons
   - View FIFO/LIFO results (if exposed) and compare with weighted average.

---

## 10) Reports (Manufacturing Reports)
1. Dashboard
   - Open Manufacturing Reports dashboard; widgets load without errors.
   - Endpoint: `/api/ManufacturingReports/dashboard`.
2. Raw Gold Utilization
   - Validate utilization over period; matches received and consumed weights.
3. Efficiency & Workflow Performance
   - Validate metrics reflect manufacturing records and transitions.
4. Cost Analysis
   - Validate cost analysis summary aligns with ownership/costing results.
5. Regression (routing)
   - All analytics endpoints under `/api/ManufacturingReports/*`.

---

## 11) Negative & Error Handling
1. Network/API down
   - UI displays friendly error banners; no unhandled exceptions.
2. Auth errors (401/403)
   - Redirect to login or show permission error without data leakage.
3. Validation errors from API
   - Inline form messages; backend messages shown meaningfully.
4. Transactions
   - Manufacturing over-consumption rejected; no partial writes.

---

## 12) Data Integrity
1. Soft delete behavior (where applicable) keeps history but hides from active lists.
2. Audit trail exists and timestamps/user info populated on changes.
3. Inventory movements match receive/manufacture operations.
4. Idempotency: repeated submit prevention and duplicate guards.

---

## 13) Performance & UX
1. Large lists paginate or virtualize; acceptable load times.
2. Loading states shown on API calls; buttons disabled to avoid double-submit.
3. Accessibility basics: keyboard nav, labels, contrast for key flows.

---

## 14) API-only Verification (Optional)
Use Postman/REST client to directly test controllers:
- AuthController: login -> token
- RawGoldPurchaseOrdersController: CRUD + Receive (PascalCase)
- ProductManufactureController: CRUD + workflow + weight checks
- ManufacturingReportsController: dashboard + analytics
- Health endpoints

---

## 15) Regression Matrix (Known Past Issues)
- Routing case sensitivity:
  - Raw gold POs -> `/api/RawGoldPurchaseOrders`
  - Product manufacture -> `/api/ProductManufacture`
  - Manufacturing reports -> `/api/ManufacturingReports/*`
- Subcategory dropdowns correctly load from `getSubCategories(categoryId)` and save `subCategoryId`.
- Purchase orders fully separated from raw gold (no raw-gold fields on regular PO items).

---

## Appendix: Suggested Test Data
- Supplier A/B with distinct branches.
- Categories: Rings, Chains; Subcategories per category.
- Products: 18k Ring, 21k Chain.
- Raw gold karat types: 18k, 21k.
- POs: one regular (2 items), one raw gold (2 items with different karats).
- Manufacturing: produce 18k Ring using mixed raw gold sources.

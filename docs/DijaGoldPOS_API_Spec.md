# Dija Gold POS API Specification

Version: 1.0
Generated: UTC

- Base URL: `/api`
- Authentication: JWT Bearer via header `Authorization: Bearer <token>`
- Content Types:
  - Consumes: `application/json` for POST/PUT/PATCH; query string for GET
  - Produces: `application/json` unless stated otherwise (exports produce files)
- Standard Response Wrapper: `ApiResponse` or `ApiResponse<T>`
- Pagination Wrapper: `PagedResult<T>` with `items`, `totalCount`, `pageNumber`, `pageSize`, `totalPages`
- Authorization Policies:
  - `ManagerOnly`: role `Manager`
  - `CashierOrManager`: roles `Cashier` or `Manager`

## Authentication Flow
1) POST `api/auth/login` with credentials → receive JWT and user info
2) Include `Authorization: Bearer <token>` for all protected endpoints
3) Optional: GET `api/auth/me` for current user details
4) Optional: POST `api/auth/refresh-token` to renew token
5) POST `api/auth/logout` to record logout audit

---

## AuthController (`api/auth`)
- POST `login`
  - Auth: Anonymous
  - Request: `LoginRequestDto { username, password }`
  - Response: `ApiResponse<LoginResponseDto>` 200; 400; 401

- POST `logout`
  - Auth: Any
  - Response: `ApiResponse` 200; 401

- POST `change-password`
  - Auth: Any
  - Request: `ChangePasswordRequestDto { currentPassword, newPassword }`
  - Response: `ApiResponse` 200; 400; 401

- GET `me`
  - Auth: Any
  - Response: `ApiResponse<UserInfoDto>` 200; 401

- POST `refresh-token`
  - Auth: Any
  - Response: `ApiResponse<LoginResponseDto>` 200; 401

---

## UsersController (`api/users`) [ManagerOnly]
- GET ``
  - Auth: `ManagerOnly`
  - Query: `UserSearchRequestDto { searchTerm?, branchId?, isActive?, isLocked?, role?, pageNumber, pageSize }`
  - Response: `ApiResponse<PagedResult<UserDto>>` 200

- GET `{id}`
  - Auth: `ManagerOnly`
  - Response: `ApiResponse<UserDto>` 200; 404

- POST `` (create)
  - Auth: `ManagerOnly`
  - Request: `CreateUserRequestDto`
  - Response: `ApiResponse<UserDto>` 201; 400

- PUT `{id}` (update)
  - Auth: `ManagerOnly`
  - Request: `UpdateUserRequestDto`
  - Response: `ApiResponse<UserDto>` 200; 400; 404

- PUT `{id}/roles`
  - Auth: `ManagerOnly`
  - Request: `UpdateUserRoleRequestDto { roles: string[] }`
  - Response: `ApiResponse` 200; 400; 404

- PUT `{id}/status`
  - Auth: `ManagerOnly`
  - Request: `UpdateUserStatusRequestDto { isActive: bool, reason? }`
  - Response: `ApiResponse` 200; 400; 404

- GET `{id}/activity`
  - Auth: `ManagerOnly`
  - Query: `fromDate?`, `toDate?`, `pageNumber=1`, `pageSize=50`
  - Response: `ApiResponse<UserActivityDto>` 200; 404

- POST `{id}/reset-password`
  - Auth: `ManagerOnly`
  - Request: `ResetPasswordRequestDto { newPassword, forcePasswordChange? }`
  - Response: `ApiResponse` 200; 400; 404

- GET `{id}/permissions`
  - Auth: `ManagerOnly`
  - Response: `ApiResponse<UserPermissionsDto>` 200; 404

- PUT `{id}/permissions`
  - Auth: `ManagerOnly`
  - Request: `UpdateUserPermissionsRequestDto { permissions: string[], featureAccess: Record<string,bool> }`
  - Response: `ApiResponse` 200; 400; 404

---

## ProductsController (`api/products`)
- GET ``
  - Auth: `CashierOrManager`
  - Query: `ProductSearchRequestDto { searchTerm?, categoryType?, karatType?, brand?, subCategory?, supplierId?, isActive?, pageNumber, pageSize }`
  - Response: `ApiResponse<PagedResult<ProductDto>>` 200

- GET `{id}`
  - Auth: `CashierOrManager`
  - Response: `ApiResponse<ProductDto>` 200; 404

- GET `{id}/inventory`
  - Auth: `CashierOrManager`
  - Response: `ApiResponse<ProductWithInventoryDto>` 200; 404

- POST `` (create)
  - Auth: `ManagerOnly`
  - Request: `CreateProductRequestDto`
  - Response: `ApiResponse<ProductDto>` 201; 400

- PUT `{id}` (update)
  - Auth: `ManagerOnly`
  - Request: `UpdateProductRequestDto`
  - Response: `ApiResponse<ProductDto>` 200; 400; 404

- DELETE `{id}` (soft delete)
  - Auth: `ManagerOnly`
  - Response: `ApiResponse` 200; 404

- GET `{id}/pricing`
  - Auth: `CashierOrManager`
  - Query: `quantity` (decimal, default 1), `customerId?`
  - Response: `ApiResponse<ProductPricingDto>` 200; 404

---

## CustomersController (`api/customers`)
- GET ``
  - Auth: `CashierOrManager`
  - Query: `CustomerSearchRequestDto { searchTerm?, nationalId?, mobileNumber?, email?, loyaltyTier?, isActive?, pageNumber, pageSize }`
  - Response: `ApiResponse<PagedResult<CustomerDto>>` 200

- GET `{id}`
  - Auth: `CashierOrManager`
  - Response: `ApiResponse<CustomerDto>` 200; 404

- POST `` (create)
  - Auth: `CashierOrManager`
  - Request: `CreateCustomerRequestDto`
  - Response: `ApiResponse<CustomerDto>` 201; 400

- PUT `{id}` (update)
  - Auth: `CashierOrManager`
  - Request: `UpdateCustomerRequestDto`
  - Response: `ApiResponse<CustomerDto>` 200; 400; 404

- DELETE `{id}` (soft delete)
  - Auth: `ManagerOnly`
  - Response: `ApiResponse` 200; 404

- GET `{id}/transactions`
  - Auth: `CashierOrManager`
  - Query: `fromDate?`, `toDate?`
  - Response: `ApiResponse<CustomerTransactionHistoryDto>` 200; 404

- GET `{id}/loyalty`
  - Auth: `CashierOrManager`
  - Response: `ApiResponse<CustomerLoyaltyDto>` 200; 404

- PUT `{id}/loyalty`
  - Auth: `ManagerOnly`
  - Request: `UpdateCustomerLoyaltyRequestDto`
  - Response: `ApiResponse<CustomerLoyaltyDto>` 200; 400; 404

- GET `search`
  - Auth: `CashierOrManager`
  - Query: `searchTerm`, `limit=10`
  - Response: `ApiResponse<List<CustomerDto>>` 200

---

## BranchesController (`api/branches`)
- GET ``
  - Auth: `CashierOrManager`
  - Query: `BranchSearchRequestDto { searchTerm?, code?, isHeadquarters?, isActive?, pageNumber, pageSize }`
  - Response: `ApiResponse<PagedResult<BranchDto>>` 200

- GET `{id}`
  - Auth: `CashierOrManager`
  - Response: `ApiResponse<BranchDto>` 200; 404

- POST `` (create)
  - Auth: `ManagerOnly`
  - Request: `CreateBranchRequestDto`
  - Response: `ApiResponse<BranchDto>` 201; 400

- PUT `{id}` (update)
  - Auth: `ManagerOnly`
  - Request: `UpdateBranchRequestDto`
  - Response: `ApiResponse<BranchDto>` 200; 400; 404

- DELETE `{id}` (soft delete)
  - Auth: `ManagerOnly`
  - Response: `ApiResponse` 200; 404

- GET `{id}/inventory`
  - Auth: `CashierOrManager`
  - Response: `ApiResponse<BranchInventorySummaryDto>` 200; 404

- GET `{id}/staff`
  - Auth: `ManagerOnly`
  - Response: `ApiResponse<BranchStaffDto>` 200; 404

- GET `{id}/performance`
  - Auth: `ManagerOnly`
  - Query: `date?`
  - Response: `ApiResponse<BranchPerformanceDto>` 200; 404

- GET `{id}/transactions`
  - Auth: `CashierOrManager`
  - Query: `fromDate?`, `toDate?`, `pageNumber`, `pageSize`
  - Response: `ApiResponse<PagedResult<BranchTransactionDto>>` 200; 404

---

## SuppliersController (`api/suppliers`)
- GET ``
  - Auth: `CashierOrManager`
  - Query: `SupplierSearchRequestDto { searchTerm?, taxRegistrationNumber?, commercialRegistrationNumber?, creditLimitEnforced?, isActive?, pageNumber, pageSize }`
  - Response: `ApiResponse<PagedResult<SupplierDto>>` 200

- GET `{id}`
  - Auth: `CashierOrManager`
  - Response: `ApiResponse<SupplierDto>` 200; 404

- POST `` (create)
  - Auth: `ManagerOnly`
  - Request: `CreateSupplierRequestDto`
  - Response: `ApiResponse<SupplierDto>` 201; 400

- PUT `{id}` (update)
  - Auth: `ManagerOnly`
  - Request: `UpdateSupplierRequestDto`
  - Response: `ApiResponse<SupplierDto>` 200; 400; 404

- DELETE `{id}` (soft delete)
  - Auth: `ManagerOnly`
  - Response: `ApiResponse` 200; 404

- GET `{id}/products`
  - Auth: `CashierOrManager`
  - Response: `ApiResponse<SupplierProductsDto>` 200; 404

- GET `{id}/balance`
  - Auth: `ManagerOnly`
  - Response: `ApiResponse<SupplierBalanceDto>` 200; 404

- PUT `{id}/balance`
  - Auth: `ManagerOnly`
  - Request: `UpdateSupplierBalanceRequestDto { transactionType: payment|credit|adjustment, amount }`
  - Response: `ApiResponse<SupplierBalanceDto>` 200; 400; 404

- GET `{id}/transactions`
  - Auth: `ManagerOnly`
  - Query: `fromDate?`, `toDate?`
  - Response: `ApiResponse<List<SupplierTransactionDto>>` 200; 404

---

## InventoryController (`api/inventory`)
- GET `product/{productId}/branch/{branchId}`
  - Auth: `CashierOrManager`
  - Response: `ApiResponse<InventoryDto>` 200; 404

- GET `branch/{branchId}`
  - Auth: `CashierOrManager`
  - Query: `includeZeroStock` (bool)
  - Response: `ApiResponse<List<InventoryDto>>` 200

- GET `branch/{branchId}/low-stock`
  - Auth: `CashierOrManager`
  - Response: `ApiResponse<List<InventoryDto>>` 200

- GET `check-availability`
  - Auth: `CashierOrManager`
  - Query: `productId`, `branchId`, `requestedQuantity`
  - Response: `ApiResponse<StockAvailabilityDto>` 200

- POST `add`
  - Auth: `ManagerOnly`
  - Request: `AddInventoryRequestDto`
  - Response: `ApiResponse` 200; 400

- POST `adjust`
  - Auth: `ManagerOnly`
  - Request: `AdjustInventoryRequestDto`
  - Response: `ApiResponse` 200; 400

- POST `transfer`
  - Auth: `ManagerOnly`
  - Request: `TransferInventoryRequestDto`
  - Response: `ApiResponse` 200; 400

- GET `movements`
  - Auth: `CashierOrManager`
  - Query: `InventoryMovementSearchRequestDto { productId?, branchId?, fromDate?, toDate?, movementType?, pageNumber, pageSize }`
  - Response: `ApiResponse<PagedResult<InventoryMovementDto>>` 200

---

## TransactionsController (`api/transactions`)
- POST `sale`
  - Auth: `CashierOrManager`
  - Request: `SaleTransactionRequestDto { branchId, customerId?, amountPaid, paymentMethod, items[] }`
  - Response: `ApiResponse<TransactionDto>` 201; 400

- POST `return`
  - Auth: `ManagerOnly`
  - Request: `ReturnTransactionRequestDto { originalTransactionId, returnReason, returnAmount, items[] }`
  - Response: `ApiResponse<TransactionDto>` 201; 400

- POST `repair`
  - Auth: `CashierOrManager`
  - Request: `RepairTransactionRequestDto`
  - Response: `ApiResponse<TransactionDto>` 201; 400

- GET `{id}`
  - Auth: `CashierOrManager`
  - Response: `ApiResponse<TransactionDto>` 200; 404

- GET `by-number/{transactionNumber}`
  - Auth: `CashierOrManager`
  - Query: `branchId`
  - Response: `ApiResponse<TransactionDto>` 200; 404

- GET `search`
  - Auth: `CashierOrManager`
  - Query: `TransactionSearchRequestDto { branchId?, transactionNumber?, transactionType?, status?, customerId?, cashierId?, fromDate?, toDate?, minAmount?, maxAmount?, pageNumber, pageSize }`
  - Response: `ApiResponse<PagedResult<TransactionDto>>` 200

- POST `cancel`
  - Auth: `ManagerOnly`
  - Request: `CancelTransactionRequestDto { transactionId, reason }`
  - Response: `ApiResponse` 200; 400

- POST `reprint-receipt`
  - Auth: `CashierOrManager`
  - Request: `ReprintReceiptRequestDto { transactionId, copies }`
  - Response: `ApiResponse<TransactionReceiptDto>` 200; 400; 404

- GET `summary`
  - Auth: `CashierOrManager`
  - Query: `branchId?`, `fromDate?`, `toDate?`
  - Response: `ApiResponse<TransactionSummaryDto>` 200

---

## PurchaseOrdersController (`api/purchaseorders`) [ManagerOnly]
- POST `` (create)
  - Auth: `ManagerOnly`
  - Request: `CreatePurchaseOrderRequestDto`
  - Response: `ApiResponse<PurchaseOrderDto>` 200

- GET `{id:int}`
  - Auth: `ManagerOnly`
  - Response: `ApiResponse<PurchaseOrderDto>` 200; 404

- POST `search`
  - Auth: `ManagerOnly`
  - Request: `PurchaseOrderSearchRequestDto`
  - Response: `ApiResponse<List<PurchaseOrderDto>>` 200 (total count in header `X-Total-Count`)

- POST `receive`
  - Auth: `ManagerOnly`
  - Request: `ReceivePurchaseOrderRequestDto`
  - Response: `ApiResponse` 200; 400

---

## PricingController (`api/pricing`)
- GET `gold-rates`
  - Auth: `CashierOrManager`
  - Response: `ApiResponse<List<GoldRateDto>>` 200

- POST `gold-rates`
  - Auth: `ManagerOnly`
  - Request: `UpdateGoldRatesRequestDto { goldRates: [ { karatType, ratePerGram, effectiveFrom } ] }`
  - Response: `ApiResponse` 200; 400

- GET `making-charges`
  - Auth: `CashierOrManager`
  - Response: `ApiResponse<List<MakingChargesDto>>` 200

- POST `making-charges`
  - Auth: `ManagerOnly`
  - Request: `UpdateMakingChargesRequestDto`
  - Response: `ApiResponse` 200; 400

- GET `taxes`
  - Auth: `CashierOrManager`
  - Response: `ApiResponse<List<TaxConfigurationDto>>` 200

- POST `calculate`
  - Auth: `CashierOrManager`
  - Request: `PriceCalculationRequestDto { productId, quantity, customerId? }`
  - Response: `ApiResponse<PriceCalculationResultDto>` 200; 400

---

## ReportsController (`api/reports`)
- GET `daily-sales-summary`
  - Auth: `CashierOrManager`
  - Query: `branchId`, `date`
  - Response: `ApiResponse<DailySalesSummaryReport>` 200

- GET `cash-reconciliation`
  - Auth: `CashierOrManager`
  - Query: `branchId`, `date`
  - Response: `ApiResponse<CashReconciliationReport>` 200

- GET `inventory-movement`
  - Auth: `CashierOrManager`
  - Query: `branchId`, `fromDate`, `toDate`
  - Response: `ApiResponse<InventoryMovementReport>` 200

- GET `profit-analysis`
  - Auth: `ManagerOnly`
  - Query: `fromDate`, `toDate`, `branchId?`, `categoryType?`
  - Response: `ApiResponse<ProfitAnalysisReport>` 200

- GET `customer-analysis`
  - Auth: `ManagerOnly`
  - Query: `fromDate`, `toDate`, `branchId?`, `topCustomersCount=20`
  - Response: `ApiResponse<CustomerAnalysisReport>` 200

- GET `supplier-balance`
  - Auth: `ManagerOnly`
  - Query: `asOfDate?`
  - Response: `ApiResponse<SupplierBalanceReport>` 200

- GET `inventory-valuation`
  - Auth: `ManagerOnly`
  - Query: `branchId?`, `asOfDate?`
  - Response: `ApiResponse<InventoryValuationReport>` 200

- GET `tax-report`
  - Auth: `ManagerOnly`
  - Query: `fromDate`, `toDate`, `branchId?`
  - Response: `ApiResponse<TaxReport>` 200

- GET `transaction-log`
  - Auth: `CashierOrManager`
  - Query: `branchId`, `date`
  - Response: `ApiResponse<TransactionLogReport>` 200

- POST `export/excel`
  - Auth: `CashierOrManager`
  - Request: `ExportReportRequestDto { reportType, reportName, reportDataJson }`
  - Response: `FileResult` (Excel `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`)

- POST `export/pdf`
  - Auth: `CashierOrManager`
  - Request: `ExportReportRequestDto { reportType, reportName, reportDataJson }`
  - Response: `FileResult` (PDF `application/pdf`)

---

## LabelsController (`api/labels`)
- GET `{productId}/zpl`
  - Auth: `ManagerOnly`
  - Query: `copies=1`
  - Response: `ApiResponse<string>` (ZPL) 200; 404

- POST `{productId}/print`
  - Auth: `ManagerOnly`
  - Query: `copies=1`
  - Response: `ApiResponse` 200; 500

- POST `decode-qr`
  - Auth: AllowAnonymous
  - Request: `DecodeQrRequestDto { payload }`
  - Response: `ApiResponse<ProductDto>` 200; 404

---

## Response Wrapper Contracts
- `ApiResponse<T>`
  - `success: bool`
  - `message?: string`
  - `data?: T`
  - `errors?: object | string[]`

- `PagedResult<T>`
  - `items: T[]`
  - `totalCount: number`
  - `pageNumber: number`
  - `pageSize: number`
  - `totalPages: number`

---

## Core Workflows
- Sales workflow
  1) Login → obtain JWT
  2) Browse products `GET /api/products` and/or fetch details `GET /api/products/{id}`
  3) Optionally price quote `GET /api/products/{id}/pricing` and stock check `GET /api/inventory/check-availability`
  4) Process sale `POST /api/transactions/sale`
  5) Reprint receipt if needed `POST /api/transactions/reprint-receipt`

- Returns workflow
  1) Manager authenticates
  2) Locate transaction `GET /api/transactions/by-number/{transactionNumber}?branchId=...`
  3) Submit return `POST /api/transactions/return`

- Repairs workflow
  1) Authenticate
  2) Submit repair `POST /api/transactions/repair`

- Procurement workflow
  1) Maintain suppliers (CRUD under `/api/suppliers`)
  2) Create PO `POST /api/purchaseorders`
  3) Receive PO `POST /api/purchaseorders/receive` → inventory updated

- Inventory admin
  - Add/adjust/transfer via `/api/inventory/{add|adjust|transfer}` (ManagerOnly)
  - Monitor branch inventory and movements

- Pricing admin
  - Update gold rates and making charges (ManagerOnly)
  - Retrieve current pricing configs; calculate quotes

- Reporting
  - Generate operational reports; export to Excel/PDF

- User administration (ManagerOnly)
  - Manage users, roles, status, permissions; view activity logs

---

## Security & Auditing
- Protected endpoints require JWT and appropriate policy (`ManagerOnly`, `CashierOrManager`)
- Sensitive mutations restricted to `ManagerOnly`
- Audit logs recorded for logins, CRUD operations, inventory movements, pricing updates, report exports, receipt reprints
- Correlation ID propagated via `X-Correlation-Id` header for tracing

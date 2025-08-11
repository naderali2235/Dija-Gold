# Dija Gold POS API Controllers Documentation

Version: 1.0
Generated: UTC

This document describes all API controllers, endpoints, authentication, request/response contracts, and the consume/produce workflow for Dija Gold POS.

- Base URL: `/api`
- Authentication: JWT Bearer in header `Authorization: Bearer <token>` (unless endpoint is explicitly anonymous)
- Content Types:
  - Consumes: `application/json` for POST/PUT; query string for GET
  - Produces: `application/json` unless stated otherwise (exports produce files)
- Standard Response Wrapper: `ApiResponse` or `ApiResponse<T>`
- Pagination Wrapper: `PagedResult<T>` with `items`, `totalCount`, `pageNumber`, `pageSize`, `totalPages`

## Authentication Flow
1. POST `api/auth/login` with credentials to receive a JWT.
2. Pass `Authorization: Bearer <token>` to all protected endpoints.
3. Optional: GET `api/auth/me` to retrieve the current user.
4. Optional: POST `api/auth/refresh-token` to refresh the JWT.

---

## AuthController (`api/auth`)

- POST `login`
  - Auth: Anonymous
  - Consumes: `LoginRequestDto { username, password }`
  - Produces: `ApiResponse<LoginResponseDto>` 200; 400; 401
  - Notes: Locks out on repeated failures; logs audit events.

- POST `logout`
  - Auth: Any
  - Produces: `ApiResponse` 200; 401

- POST `change-password`
  - Auth: Any
  - Consumes: `ChangePasswordRequestDto { currentPassword, newPassword }`
  - Produces: `ApiResponse` 200; 400; 401

- GET `me`
  - Auth: Any
  - Produces: `ApiResponse<UserInfoDto>` 200; 401

- POST `refresh-token`
  - Auth: Any
  - Produces: `ApiResponse<LoginResponseDto>` 200; 401

Consume steps:
- Include `Authorization: Bearer <token>` for protected calls.
- Send JSON body for POST/PUT.

---

## ProductsController (`api/products`)

- GET ``
  - Auth: Policy `CashierOrManager`
  - Query: `ProductSearchRequestDto { searchTerm?, categoryType?, karatType?, brand?, subCategory?, supplierId?, isActive?, pageNumber, pageSize }`
  - Produces: `ApiResponse<PagedResult<ProductDto>>` 200

- GET `{id}`
  - Auth: `CashierOrManager`
  - Produces: `ApiResponse<ProductDto>` 200; 404

- GET `{id}/inventory`
  - Auth: `CashierOrManager`
  - Produces: `ApiResponse<ProductWithInventoryDto>` 200; 404

- POST `` (create)
  - Auth: `ManagerOnly`
  - Consumes: `CreateProductRequestDto`
  - Produces: `ApiResponse<ProductDto>` 201; 400
  - Validation: unique `ProductCode`, existing `SupplierId` if set.

- PUT `{id}` (update)
  - Auth: `ManagerOnly`
  - Consumes: `UpdateProductRequestDto`
  - Produces: `ApiResponse<ProductDto>` 200; 400; 404
  - Validation: `ProductCode` uniqueness on change; supplier existence.

- DELETE `{id}` (soft delete)
  - Auth: `ManagerOnly`
  - Produces: `ApiResponse` 200; 404

- GET `{id}/pricing`
  - Auth: `CashierOrManager`
  - Query: `quantity` (decimal, default 1), `customerId?`
  - Produces: `ApiResponse<ProductPricingDto>` 200; 404

Consume steps:
- For create/update, send JSON body; for list/detail, use query/path params.
- All calls audit; failures produce `ApiResponse` with message.

---

## CustomersController (`api/customers`)

- GET ``
  - Auth: `CashierOrManager`
  - Query: `CustomerSearchRequestDto { searchTerm?, nationalId?, mobileNumber?, email?, loyaltyTier?, isActive?, pageNumber, pageSize }`
  - Produces: `ApiResponse<PagedResult<CustomerDto>>` 200

- GET `{id}`
  - Auth: `CashierOrManager`
  - Produces: `ApiResponse<CustomerDto>` 200; 404

- POST `` (create)
  - Auth: `CashierOrManager`
  - Consumes: `CreateCustomerRequestDto`
  - Produces: `ApiResponse<CustomerDto>` 201; 400
  - Validation: duplicate checks for NationalId, MobileNumber, Email.

- PUT `{id}` (update)
  - Auth: `CashierOrManager`
  - Consumes: `UpdateCustomerRequestDto`
  - Produces: `ApiResponse<CustomerDto>` 200; 400; 404
  - Validation: duplicate checks (excluding current).

- DELETE `{id}` (soft delete)
  - Auth: `ManagerOnly`
  - Produces: `ApiResponse` 200; 404
  - Constraint: cannot delete if active transactions exist.

- GET `{id}/transactions`
  - Auth: `CashierOrManager`
  - Query: `fromDate?`, `toDate?`
  - Produces: `ApiResponse<CustomerTransactionHistoryDto>` 200; 404

- GET `{id}/loyalty`
  - Auth: `CashierOrManager`
  - Produces: `ApiResponse<CustomerLoyaltyDto>` 200; 404

- PUT `{id}/loyalty`
  - Auth: `ManagerOnly`
  - Consumes: `UpdateCustomerLoyaltyRequestDto`
  - Produces: `ApiResponse<CustomerLoyaltyDto>` 200; 400; 404

- GET `search`
  - Auth: `CashierOrManager`
  - Query: `searchTerm`, `limit=10`
  - Produces: `ApiResponse<List<CustomerDto>>` 200

Consume steps:
- Pass query filters for list/search.
- Body payloads for create/update.

---

## TransactionsController (`api/transactions`)

- POST `sale`
  - Auth: `CashierOrManager`
  - Consumes: `SaleTransactionRequestDto { branchId, customerId?, amountPaid, paymentMethod, items[] }`
  - Produces: `ApiResponse<TransactionDto>` 201; 400

- POST `return`
  - Auth: `ManagerOnly`
  - Consumes: `ReturnTransactionRequestDto { originalTransactionId, returnReason, returnAmount, items[] }`
  - Produces: `ApiResponse<TransactionDto>` 201; 400

- POST `repair`
  - Auth: `CashierOrManager`
  - Consumes: `RepairTransactionRequestDto`
  - Produces: `ApiResponse<TransactionDto>` 201; 400

- GET `{id}`
  - Auth: `CashierOrManager`
  - Produces: `ApiResponse<TransactionDto>` 200; 404

- GET `by-number/{transactionNumber}`
  - Auth: `CashierOrManager`
  - Query: `branchId`
  - Produces: `ApiResponse<TransactionDto>` 200; 404

- GET `search`
  - Auth: `CashierOrManager`
  - Query: `TransactionSearchRequestDto { branchId?, transactionNumber?, transactionType?, status?, customerId?, cashierId?, fromDate?, toDate?, minAmount?, maxAmount?, pageNumber, pageSize }`
  - Produces: `ApiResponse<PagedResult<TransactionDto>>` 200

- POST `cancel`
  - Auth: `ManagerOnly`
  - Consumes: `CancelTransactionRequestDto { transactionId, reason }`
  - Produces: `ApiResponse` 200; 400

- POST `reprint-receipt`
  - Auth: `CashierOrManager`
  - Consumes: `ReprintReceiptRequestDto { transactionId, copies }`
  - Produces: `ApiResponse<TransactionReceiptDto>` 200; 400; 404

- GET `summary`
  - Auth: `CashierOrManager`
  - Query: `branchId?`, `fromDate?`, `toDate?`
  - Produces: `ApiResponse<TransactionSummaryDto>` 200

Consume steps:
- POST endpoints require JSON bodies; ID lookups via route or query.
- All operations audit; errors yield descriptive `ApiResponse` messages.

---

## InventoryController (`api/inventory`)

- GET `product/{productId}/branch/{branchId}`
  - Auth: `CashierOrManager`
  - Produces: `ApiResponse<InventoryDto>` 200; 404

- GET `branch/{branchId}`
  - Auth: `CashierOrManager`
  - Query: `includeZeroStock` (bool)
  - Produces: `ApiResponse<List<InventoryDto>>` 200

- GET `branch/{branchId}/low-stock`
  - Auth: `CashierOrManager`
  - Produces: `ApiResponse<List<InventoryDto>>` 200

- GET `check-availability`
  - Auth: `CashierOrManager`
  - Query: `productId`, `branchId`, `requestedQuantity`
  - Produces: `ApiResponse<StockAvailabilityDto>` 200

- POST `add`
  - Auth: `ManagerOnly`
  - Consumes: `AddInventoryRequestDto`
  - Produces: `ApiResponse` 200; 400

- POST `adjust`
  - Auth: `ManagerOnly`
  - Consumes: `AdjustInventoryRequestDto`
  - Produces: `ApiResponse` 200; 400

- POST `transfer`
  - Auth: `ManagerOnly`
  - Consumes: `TransferInventoryRequestDto`
  - Produces: `ApiResponse` 200; 400

- GET `movements`
  - Auth: `CashierOrManager`
  - Query: `InventoryMovementSearchRequestDto { productId?, branchId?, fromDate?, toDate?, movementType?, pageNumber, pageSize }`
  - Produces: `ApiResponse<PagedResult<InventoryMovementDto>>` 200

Consume steps:
- Use route params for specific inventory records; body for updates/transfers.

---

## BranchesController (`api/branches`)

- GET ``
  - Auth: `CashierOrManager`
  - Query: `BranchSearchRequestDto { searchTerm?, code?, isHeadquarters?, isActive?, pageNumber, pageSize }`
  - Produces: `ApiResponse<PagedResult<BranchDto>>` 200

- GET `{id}`
  - Auth: `CashierOrManager`
  - Produces: `ApiResponse<BranchDto>` 200; 404

- POST `` (create)
  - Auth: `ManagerOnly`
  - Consumes: `CreateBranchRequestDto`
  - Produces: `ApiResponse<BranchDto>` 201; 400
  - Constraints: unique `code`; only one headquarters active at a time.

- PUT `{id}` (update)
  - Auth: `ManagerOnly`
  - Consumes: `UpdateBranchRequestDto`
  - Produces: `ApiResponse<BranchDto>` 200; 400; 404
  - Constraints: `code` uniqueness; HQ exclusivity.

- DELETE `{id}` (soft delete)
  - Auth: `ManagerOnly`
  - Produces: `ApiResponse` 200; 404
  - Constraints: cannot delete HQ; cannot delete if inventory or users exist.

- GET `{id}/inventory`
  - Auth: `CashierOrManager`
  - Produces: `ApiResponse<BranchInventorySummaryDto>` 200; 404

- GET `{id}/staff`
  - Auth: `ManagerOnly`
  - Produces: `ApiResponse<BranchStaffDto>` 200; 404

- GET `{id}/performance`
  - Auth: `ManagerOnly`
  - Query: `date?`
  - Produces: `ApiResponse<BranchPerformanceDto>` 200; 404

- GET `{id}/transactions`
  - Auth: `CashierOrManager`
  - Query: `fromDate?`, `toDate?`, `pageNumber=1`, `pageSize=50`
  - Produces: `ApiResponse<PagedResult<BranchTransactionDto>>` 200; 404

Consume steps:
- Administrative changes require manager policy; reading lists/details typically cashier or manager.

---

## SuppliersController (`api/suppliers`)

- GET ``
  - Auth: `CashierOrManager`
  - Query: `SupplierSearchRequestDto { searchTerm?, taxRegistrationNumber?, commercialRegistrationNumber?, creditLimitEnforced?, isActive?, pageNumber, pageSize }`
  - Produces: `ApiResponse<PagedResult<SupplierDto>>` 200

- GET `{id}`
  - Auth: `CashierOrManager`
  - Produces: `ApiResponse<SupplierDto>` 200; 404

- POST `` (create)
  - Auth: `ManagerOnly`
  - Consumes: `CreateSupplierRequestDto`
  - Produces: `ApiResponse<SupplierDto>` 201; 400
  - Validation: TRN/CRN uniqueness if provided.

- PUT `{id}` (update)
  - Auth: `ManagerOnly`
  - Consumes: `UpdateSupplierRequestDto`
  - Produces: `ApiResponse<SupplierDto>` 200; 400; 404

- DELETE `{id}` (soft delete)
  - Auth: `ManagerOnly`
  - Produces: `ApiResponse` 200; 404
  - Constraint: cannot delete if products exist for supplier.

- GET `{id}/products`
  - Auth: `CashierOrManager`
  - Produces: `ApiResponse<SupplierProductsDto>` 200; 404

- GET `{id}/balance`
  - Auth: `ManagerOnly`
  - Produces: `ApiResponse<SupplierBalanceDto>` 200; 404

- PUT `{id}/balance`
  - Auth: `ManagerOnly`
  - Consumes: `UpdateSupplierBalanceRequestDto { transactionType: payment|credit|adjustment, amount }`
  - Produces: `ApiResponse<SupplierBalanceDto>` 200; 400; 404

- GET `{id}/transactions`
  - Auth: `ManagerOnly`
  - Query: `fromDate?`, `toDate?`
  - Produces: `ApiResponse<List<SupplierTransactionDto>>` 200; 404

Consume steps:
- Use manager policy for supplier mutations and sensitive balance endpoints.

---

## PricingController (`api/pricing`)

- GET `gold-rates`
  - Auth: `CashierOrManager`
  - Produces: `ApiResponse<List<GoldRateDto>>` 200

- POST `gold-rates`
  - Auth: `ManagerOnly`
  - Consumes: `UpdateGoldRatesRequestDto { goldRates: [ { karatType, ratePerGram, effectiveFrom } ] }`
  - Produces: `ApiResponse` 200; 400

- GET `making-charges`
  - Auth: `CashierOrManager`
  - Produces: `ApiResponse<List<MakingChargesDto>>` 200

- POST `making-charges`
  - Auth: `ManagerOnly`
  - Consumes: `UpdateMakingChargesRequestDto`
  - Produces: `ApiResponse` 200; 400

- GET `taxes`
  - Auth: `CashierOrManager`
  - Produces: `ApiResponse<List<TaxConfigurationDto>>` 200

- POST `calculate`
  - Auth: `CashierOrManager`
  - Consumes: `PriceCalculationRequestDto { productId, quantity, customerId? }`
  - Produces: `ApiResponse<PriceCalculationResultDto>` 200; 400

Consume steps:
- Manager role updates master data (rates/charges). Cashiers/managers read current configs and calculate prices.

---

## LabelsController (`api/labels`)

- GET `{productId}/zpl`
  - Auth: `ManagerOnly`
  - Query: `copies=1`
  - Produces: `ApiResponse<string>` (ZPL content) 200; 404

- POST `{productId}/print`
  - Auth: `ManagerOnly`
  - Query: `copies=1`
  - Produces: `ApiResponse` 200; 500

- POST `decode-qr`
  - Auth: AllowAnonymous
  - Consumes: `DecodeQrRequestDto { payload }`
  - Produces: `ApiResponse<ProductDto>` 200; 404

Consume steps:
- For printing, service sends label to configured printer; ZPL payload can be previewed via GET.

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

## Common Consume/Produce Workflow

- Consume:
  1) Authenticate (login) and store JWT.
  2) For reads, supply query parameters; for writes, send JSON bodies.
  3) Include `Authorization` header; ensure proper roles/policies.

- Produce:
  - Returns `ApiResponse` wrapper with success flag and data or error message.
  - File exports (Reports) return `FileResult` with appropriate content type.

---

## Notes on Security & Auditing
- All sensitive mutations require `ManagerOnly` or appropriate policy.
- Audit logs are recorded for key actions (create/update/delete, logins, pricing updates, inventory movements, receipt reprints).


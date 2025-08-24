# API Endpoint Fixes Required

Based on the analysis of the API service file and controller endpoints, here are the remaining fixes needed:

## Completed Fixes ✅
- **Pricing API**: Fixed `/Pricing/` → `/api/pricing/`
- **Cash Drawer API**: Fixed `/CashDrawer/` → `/api/cashdrawer/`
- **Products API**: Added `/api` prefix to all endpoints
- **Users API**: Added `/api` prefix to all endpoints  
- **Branches API**: Added `/api` prefix to all endpoints
- **Suppliers API**: Partially fixed (in progress)

## Remaining Fixes Needed ⚠️

### 1. Complete Suppliers API
```typescript
// Fix these endpoints in suppliersApi:
'/suppliers/${id}' → '/api/suppliers/${id}'
'/suppliers/${id}/products' → '/api/suppliers/${id}/products'
'/suppliers/${id}/balance' → '/api/suppliers/${id}/balance'
'/suppliers/${id}/transactions' → '/api/suppliers/${id}/transactions'
```

### 2. Inventory API
```typescript
// Fix these endpoints in inventoryApi:
'/inventory/product/${productId}/branch/${branchId}' → '/api/inventory/product/${productId}/branch/${branchId}'
'/inventory/branch/${branchId}' → '/api/inventory/branch/${branchId}'
'/inventory/branch/${branchId}/low-stock' → '/api/inventory/branch/${branchId}/low-stock'
'/inventory/check-availability' → '/api/inventory/check-availability'
'/inventory/add' → '/api/inventory/add'
'/inventory/adjust' → '/api/inventory/adjust'
'/inventory/transfer' → '/api/inventory/transfer'
'/inventory/movements' → '/api/inventory/movements'
```

### 3. Reports API
```typescript
// Fix these endpoints in reportsApi:
'/reports/daily-sales-summary' → '/api/reports/daily-sales-summary'
'/reports/cash-reconciliation' → '/api/reports/cash-reconciliation'
'/reports/inventory-movement' → '/api/reports/inventory-movement'
'/reports/profit-analysis' → '/api/reports/profit-analysis'
'/reports/customer-analysis' → '/api/reports/customer-analysis'
'/reports/supplier-balance' → '/api/reports/supplier-balance'
'/reports/inventory-valuation' → '/api/reports/inventory-valuation'
'/reports/tax-report' → '/api/reports/tax-report'
'/reports/transaction-log' → '/api/reports/transaction-log'
'/reports/types' → '/api/reports/types'
```

### 4. Purchase Orders API
```typescript
// Fix these endpoints in purchaseOrdersApi:
'/purchaseorders' → '/api/purchaseorders'
'/purchaseorders/${id}' → '/api/purchaseorders/${id}'
'/purchaseorders/search' → '/api/purchaseorders/search'
'/purchaseorders/receive' → '/api/purchaseorders/receive'
```

### 5. Labels API
```typescript
// Fix these endpoints in labelsApi:
'/labels/${productId}/zpl' → '/api/labels/${productId}/zpl'
'/labels/${productId}/print' → '/api/labels/${productId}/print'
'/labels/decode-qr' → '/api/labels/decode-qr'
```

### 6. Customers API
```typescript
// Fix these endpoints in customersApi:
'/customers' → '/api/customers'
'/customers/${id}' → '/api/customers/${id}'
```

### 7. Lookups API
```typescript
// Fix these endpoints in lookupsApi:
'/lookups' → '/api/lookups'
'/lookups/transaction-types' → '/api/lookups/transaction-types'
'/lookups/payment-methods' → '/api/lookups/payment-methods'
'/lookups/transaction-statuses' → '/api/lookups/transaction-statuses'
'/lookups/karat-types' → '/api/lookups/karat-types'
'/lookups/product-category-types' → '/api/lookups/product-category-types'
'/lookups/charge-types' → '/api/lookups/charge-types'
'/lookups/repair-statuses' → '/api/lookups/repair-statuses'
'/lookups/repair-priorities' → '/api/lookups/repair-priorities'
```

### 8. Repair Jobs API
```typescript
// Fix these endpoints in repairJobsApi:
'/repairjobs' → '/api/repairjobs'
'/repairjobs/${id}' → '/api/repairjobs/${id}'
'/repairjobs/by-financial-transaction/${financialTransactionId}' → '/api/repairjobs/by-financial-transaction/${financialTransactionId}'
'/repairjobs/${id}/status' → '/api/repairjobs/${id}/status'
'/repairjobs/${id}/assign' → '/api/repairjobs/${id}/assign'
'/repairjobs/${id}/complete' → '/api/repairjobs/${id}/complete'
'/repairjobs/${id}/ready-for-pickup' → '/api/repairjobs/${id}/ready-for-pickup'
'/repairjobs/${id}/deliver' → '/api/repairjobs/${id}/deliver'
'/repairjobs/${id}/cancel' → '/api/repairjobs/${id}/cancel'
'/repairjobs/search' → '/api/repairjobs/search'
'/repairjobs/statistics' → '/api/repairjobs/statistics'
'/repairjobs/by-status/${status}' → '/api/repairjobs/by-status/${status}'
'/repairjobs/by-technician/${technicianId}' → '/api/repairjobs/by-technician/${technicianId}'
'/repairjobs/overdue' → '/api/repairjobs/overdue'
'/repairjobs/due-today' → '/api/repairjobs/due-today'
```

### 9. Technicians API
```typescript
// Fix these endpoints in techniciansApi:
'/technicians' → '/api/technicians'
'/technicians/${id}' → '/api/technicians/${id}'
'/technicians/search' → '/api/technicians/search'
'/technicians/active' → '/api/technicians/active'
'/technicians/branch/${branchId}' → '/api/technicians/branch/${branchId}'
```

### 10. Orders API
```typescript
// Fix these endpoints in ordersApi:
'/orders/sale' → '/api/orders/sale'
'/orders/repair' → '/api/orders/repair'
'/orders/${id}' → '/api/orders/${id}'
'/orders/search' → '/api/orders/search'
'/orders/summary' → '/api/orders/summary'
'/orders/customer/${customerId}' → '/api/orders/customer/${customerId}'
'/orders/cashier/${cashierId}' → '/api/orders/cashier/${cashierId}'
```

### 11. Financial Transactions API
```typescript
// Fix these endpoints in financialTransactionsApi:
'/financialtransactions/${id}' → '/api/financialtransactions/${id}'
'/financialtransactions/search' → '/api/financialtransactions/search'
'/financialtransactions/${id}/void' → '/api/financialtransactions/${id}/void'
// Note: generate-browser-receipt is already fixed
```

### 12. Product Ownership API
```typescript
// Fix these endpoints in productOwnershipApi:
'/productownership' → '/api/productownership'
'/productownership/validate' → '/api/productownership/validate'
'/productownership/payment' → '/api/productownership/payment'
'/productownership/sale' → '/api/productownership/sale'
'/productownership/convert-raw-gold' → '/api/productownership/convert-raw-gold'
'/productownership/alerts' → '/api/productownership/alerts'
'/productownership/product/${productId}/branch/${branchId}' → '/api/productownership/product/${productId}/branch/${branchId}'
'/productownership/movements/${productOwnershipId}' → '/api/productownership/movements/${productOwnershipId}'
'/productownership/low-ownership' → '/api/productownership/low-ownership'
'/productownership/outstanding-payments' → '/api/productownership/outstanding-payments'
```

## Additional Missing Endpoints to Add

Based on the controller analysis, these endpoints are missing and should be added:

### Financial Transactions Controller
```typescript
// Already added: generate-browser-receipt
// Missing: mark-gl-posted
async markGLPosted(id: number): Promise<boolean> {
  const response = await apiRequest<boolean>(`/api/financialtransactions/${id}/mark-gl-posted`, {
    method: 'POST',
  });
  
  if (response.success) {
    return true;
  }
  
  throw new Error(response.message || 'Failed to mark GL as posted');
}
```

## Search and Replace Commands

You can use these search and replace patterns to fix the remaining endpoints systematically:

1. **Global patterns:**
   - `'/suppliers/` → `'/api/suppliers/`
   - `'/inventory/` → `'/api/inventory/`
   - `'/reports/` → `'/api/reports/`
   - `'/purchaseorders` → `'/api/purchaseorders`
   - `'/labels/` → `'/api/labels/`
   - `'/customers` → `'/api/customers`
   - `'/lookups` → `'/api/lookups`
   - `'/repairjobs` → `'/api/repairjobs`
   - `'/technicians` → `'/api/technicians`
   - `'/orders/` → `'/api/orders/`
   - `'/financialtransactions/` → `'/api/financialtransactions/`
   - `'/productownership` → `'/api/productownership`

## Verification Steps

After making these changes:

1. **Test API connectivity** with `testApiConnection()`
2. **Verify each API section** works correctly
3. **Check console for any 404 errors** indicating missed endpoints
4. **Test authentication flow** to ensure `/api/auth/*` endpoints work
5. **Test a few key operations** like product search, user management, etc.

## Notes

- All controllers use `[Route("api/[controller]")]` pattern
- Controller names are case-sensitive (e.g., `CashDrawer` → `cashdrawer`)
- The API base URL already includes the protocol and host
- Some endpoints may have different HTTP methods than expected - verify against controllers

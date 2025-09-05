# Supplier-Centralized Gold Balance Management System

## Overview

This document describes the new supplier-centralized gold balance management system that replaces the complex product ownership tracking with a simpler, more intuitive approach focused on supplier relationships and gold balance management.

## Business Requirements

### Core Concept
- **Supplier-Focused**: Track gold ownership by supplier, not individual products
- **Weight-Based**: Track gold by weight and karat type, not individual items
- **Balance Management**: Allow waiving customer-purchased gold to suppliers to reduce debt
- **Karat Conversion**: Support automatic conversion between different karat types based on current gold rates

### Example Scenario
1. Merchant receives 100g of 21K gold from Supplier A, pays 70% (owes 30%)
2. Customer sells 10g of 18K gold to merchant
3. Merchant can waive the 10g of 18K gold to Supplier A
4. System automatically converts 18K to 21K based on current rates (e.g., 10g 18K = ~7g 21K)
5. Supplier A's debt is reduced by the equivalent value

## System Architecture

### New Models

#### 1. RawGoldTransfer
Tracks all gold transfers between merchant and suppliers:
```csharp
public class RawGoldTransfer : BaseEntity
{
    public string TransferNumber { get; set; }
    public int BranchId { get; set; }
    public int? FromSupplierId { get; set; }    // null = from merchant
    public int? ToSupplierId { get; set; }      // null = to merchant
    public int FromKaratTypeId { get; set; }
    public int ToKaratTypeId { get; set; }
    public decimal FromWeight { get; set; }
    public decimal ToWeight { get; set; }       // After conversion
    public decimal FromGoldRate { get; set; }
    public decimal ToGoldRate { get; set; }
    public decimal ConversionFactor { get; set; }
    public decimal TransferValue { get; set; }
    public string TransferType { get; set; }    // Waive, Credit, Convert
    public int? CustomerPurchaseId { get; set; }
    // ... navigation properties
}
```

#### 2. SupplierGoldBalance
Aggregated view of supplier balances by karat type:
```csharp
public class SupplierGoldBalance : BaseEntity
{
    public int SupplierId { get; set; }
    public int BranchId { get; set; }
    public int KaratTypeId { get; set; }
    public decimal TotalWeightReceived { get; set; }
    public decimal TotalWeightPaidFor { get; set; }
    public decimal OutstandingWeightDebt => TotalWeightReceived - TotalWeightPaidFor;
    public decimal MerchantGoldBalance { get; set; }    // Positive = credit to merchant
    public decimal OutstandingMonetaryValue { get; set; }
    public decimal AverageCostPerGram { get; set; }
    // ... navigation properties
}
```

### New Services

#### IRawGoldBalanceService
Core service for gold balance management:

**Key Methods:**
- `GetSupplierBalancesAsync()` - Get supplier gold balances
- `WaiveGoldToSupplierAsync()` - Waive customer gold to supplier
- `ConvertGoldKaratAsync()` - Convert gold between karat types
- `GetMerchantRawGoldBalanceAsync()` - Get merchant's available gold
- `GetTransferHistoryAsync()` - Get transfer history with filtering
- `CalculateKaratConversionAsync()` - Preview conversion calculations

### New Controllers

#### RawGoldBalanceController
RESTful API endpoints:

- `GET /api/rawgoldbalance/supplier-balances/{branchId}` - Get supplier balances
- `GET /api/rawgoldbalance/merchant-balance/{branchId}` - Get merchant balance
- `GET /api/rawgoldbalance/summary/{branchId}` - Get comprehensive summary
- `POST /api/rawgoldbalance/waive-to-supplier` - Waive gold to supplier
- `POST /api/rawgoldbalance/convert-karat` - Convert gold karat
- `GET /api/rawgoldbalance/transfers` - Get transfer history
- `GET /api/rawgoldbalance/calculate-conversion` - Preview conversion
- `GET /api/rawgoldbalance/available-for-waiving/{branchId}` - Get available gold

## Key Features

### 1. Gold Waiving System
**Purpose**: Reduce supplier debt by transferring customer-purchased gold

**Process**:
1. Validate merchant has sufficient gold of requested karat type
2. Get current gold rates for both karat types
3. Calculate conversion (value-based): `toWeight = (fromWeight × fromRate) / toRate`
4. Create transfer record
5. Update merchant inventory (reduce)
6. Update supplier balance (increase credit)
7. Update supplier financial balance (reduce debt)

### 2. Karat Conversion System
**Purpose**: Convert gold between different karat types

**Conversion Logic**:
- Based on current gold rates from `GoldRates` table
- Value-based conversion: maintains monetary value
- Example: 10g × 1800 EGP/g (18K) = 18,000 EGP value
- Convert to 21K: 18,000 EGP ÷ 2400 EGP/g (21K) = 7.5g

### 3. Balance Tracking
**Supplier Balance Components**:
- **Debt**: What merchant owes supplier (positive outstanding weight/value)
- **Credit**: What supplier owes merchant (positive merchant gold balance)
- **Net Position**: Overall balance between merchant and supplier

### 4. Comprehensive Reporting
**Available Reports**:
- Supplier balances by branch and karat type
- Merchant available gold for waiving
- Transfer history with filtering
- Balance summaries with debt/credit analysis

## Database Schema

### New Tables
1. **RawGoldTransfers** - All gold transfer records
2. **SupplierGoldBalances** - Aggregated supplier balance views

### Key Relationships
- RawGoldTransfer → Branch (required)
- RawGoldTransfer → FromSupplier (optional, null = from merchant)
- RawGoldTransfer → ToSupplier (optional, null = to merchant)
- RawGoldTransfer → FromKaratType (required)
- RawGoldTransfer → ToKaratType (required)
- RawGoldTransfer → CustomerPurchase (optional)
- SupplierGoldBalance → Supplier (required)
- SupplierGoldBalance → Branch (required)
- SupplierGoldBalance → KaratType (required)

### Indexes for Performance
- RawGoldTransfer: TransferNumber (unique), TransferDate, TransferType, (BranchId, TransferDate)
- SupplierGoldBalance: (SupplierId, BranchId, KaratTypeId) (unique), SupplierId, BranchId

## API Examples

### 1. Waive Gold to Supplier
```http
POST /api/rawgoldbalance/waive-to-supplier
Content-Type: application/json

{
    "branchId": 1,
    "toSupplierId": 5,
    "fromKaratTypeId": 2,  // 18K
    "toKaratTypeId": 3,    // 21K
    "fromWeight": 10.000,
    "customerPurchaseId": 123,
    "notes": "Waiving customer gold to reduce debt"
}
```

### 2. Get Supplier Balances
```http
GET /api/rawgoldbalance/supplier-balances/1?supplierId=5
```

### 3. Calculate Conversion Preview
```http
GET /api/rawgoldbalance/calculate-conversion?fromKaratTypeId=2&toKaratTypeId=3&fromWeight=10.000
```

## Migration Strategy

### Phase 1: Parallel Implementation ✅ COMPLETED
- [x] New models created
- [x] New services implemented
- [x] New controllers added
- [x] AutoMapper profiles configured
- [x] Validators implemented
- [x] Dependency injection configured

### Phase 2: Data Migration (Your Responsibility)
- [ ] Create database migration
- [ ] Migrate existing ProductOwnership data to SupplierGoldBalance
- [ ] Migrate existing RawGoldOwnership data
- [ ] Validate data integrity

### Phase 3: Integration
- [ ] Update CustomerPurchaseService to use new system
- [ ] Modify existing gold receiving processes
- [ ] Update reporting systems

### Phase 4: Deprecation
- [ ] Mark old ProductOwnershipService as obsolete
- [ ] Remove OwnershipConsolidationService
- [ ] Simplify PurchaseOrderService (remove complex ownership logic)

## Benefits of New System

### 1. Simplified Business Logic
- ❌ **Old**: Complex ownership percentages, partial payments, consolidation
- ✅ **New**: Simple weight-based tracking, clear supplier balances

### 2. Intuitive Supplier Management
- ❌ **Old**: Track individual products with multiple ownership records
- ✅ **New**: Track gold by supplier and karat type

### 3. Flexible Gold Management
- ❌ **Old**: Fixed product ownership, complex conversions
- ✅ **New**: Easy waiving, automatic karat conversions

### 4. Better Financial Tracking
- ❌ **Old**: Ownership percentages don't clearly show debts
- ✅ **New**: Clear debt/credit balances with suppliers

### 5. Performance Improvements
- ❌ **Old**: Complex queries across multiple ownership tables
- ✅ **New**: Simple aggregated balance queries

## Security & Authorization

### Role-Based Access:
- **ManagerOrAbove**: Can waive gold, convert karat, view all balances
- **CashierOrManager**: Can view balances, transfer history, calculate conversions
- **ReadOnly**: Can view reports only

### Audit Trail:
- All transfers logged with user ID and timestamp
- Full history maintained in RawGoldTransfers table
- Integration with existing AuditService

## Testing Considerations

### Unit Tests Needed:
- [ ] RawGoldBalanceService business logic
- [ ] Karat conversion calculations
- [ ] Balance update logic
- [ ] Validation rules

### Integration Tests Needed:
- [ ] Controller endpoints
- [ ] Database transactions
- [ ] AutoMapper mappings
- [ ] Authorization policies

### Business Logic Tests:
- [ ] Waiving scenarios (various karat combinations)
- [ ] Conversion accuracy
- [ ] Balance calculations
- [ ] Edge cases (insufficient gold, invalid rates)

## Future Enhancements

### Potential Improvements:
1. **Batch Operations**: Waive multiple gold types in one transaction
2. **Automated Waiving**: Auto-waive based on predefined rules
3. **Advanced Reporting**: Trend analysis, profitability reports
4. **Mobile API**: Simplified endpoints for mobile apps
5. **Integration**: Connect with external gold rate services
6. **Notifications**: Alerts for low balances, debt thresholds

## Conclusion

The new supplier-centralized gold balance management system provides a much simpler, more intuitive approach to managing gold inventory and supplier relationships. It eliminates the complexity of the previous product ownership system while providing more flexibility and better financial tracking.

The system is now ready for database migration and integration with your existing processes.

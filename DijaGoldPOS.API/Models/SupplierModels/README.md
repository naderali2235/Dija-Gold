# Supplier Module Models

This module contains all supplier-related entities for managing vendor relationships, procurement, and supplier gold balance tracking.

## Models Overview

### Supplier
- **Purpose**: Supplier information and relationship management
- **Key Features**:
  - Company and contact information
  - Credit limit and payment terms
  - Performance tracking
  - Multi-branch relationships
- **Schema**: `Supplier`
- **Relationships**: One-to-many with Products, PurchaseOrders, RawGoldPurchaseOrders

### SupplierTransaction
- **Purpose**: Financial transactions with suppliers
- **Key Features**:
  - Payment tracking
  - Credit/debit management
  - Transaction categorization
- **Schema**: `Supplier`
- **Relationships**: Many-to-one with Supplier, PurchaseOrder

### SupplierGoldBalance
- **Purpose**: Centralized gold balance management per supplier
- **Key Features**:
  - Karat-specific balance tracking
  - Branch-specific balances
  - Real-time balance updates
- **Schema**: `Supplier`
- **Relationships**: Many-to-one with Supplier, Branch, KaratType

### RawGoldTransfer
- **Purpose**: Gold transfer operations between suppliers and internal inventory
- **Key Features**:
  - Transfer type tracking (Purchase, Sale, Transfer)
  - Karat conversion support
  - Customer purchase integration
- **Schema**: `Supplier`
- **Relationships**: Many-to-one with Supplier, Branch, KaratType

## Database Schema Design

All supplier models use the `Supplier` schema for logical separation.

## Business Rules

1. **Supplier Codes**: Must be unique among active suppliers
2. **Credit Limits**: Enforced during purchase order creation
3. **Balance Tracking**: Real-time updates for all gold transactions
4. **Transfer Validation**: Weight and karat consistency checks

## Unique Constraints

- Supplier codes: Filtered unique constraints excluding inactive records
- Gold balance: Composite unique on (SupplierId, BranchId, KaratTypeId)

## API Endpoints

- `/api/supplier/suppliers` - Supplier management
- `/api/supplier/transactions` - Financial transaction tracking
- `/api/supplier/gold-balance` - Gold balance management
- `/api/supplier/transfers` - Gold transfer operations

## Service Dependencies

- `ISupplierService` - Supplier operations and validation
- `ISupplierTransactionService` - Financial transaction management
- `IRawGoldBalanceService` - Gold balance calculations
- `ITransferService` - Gold transfer operations

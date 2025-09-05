# Lookup Module Models

This module contains all lookup tables that replace enums and provide reference data for the system.

## Design Principles

1. **Consistency**: All lookup tables follow the same structure
2. **Extensibility**: New lookup values can be added without code changes
3. **Soft Delete**: Inactive lookups are retained for historical data integrity
4. **Unique Constraints**: Names are unique only among active records

## Models Overview

### ILookupEntity
- **Purpose**: Interface defining common lookup table structure
- **Properties**: Id, Name, Description, DisplayOrder, IsActive
- **Schema**: `Lookup`

### Business Entity Lookups
- **BusinessEntityTypeLookup**: Customer, Supplier, Branch types
- **PaymentMethodLookup**: Cash, Card, Bank Transfer, etc.

### Product Lookups
- **KaratTypeLookup**: 18K, 21K, 22K, 24K gold types
- **ProductCategoryTypeLookup**: Jewelry, Bullion, Coins
- **SubCategoryLookup**: Rings, Necklaces, Bracelets, etc.
- **ChargeTypeLookup**: Fixed, Percentage, Per Gram

### Order & Transaction Lookups
- **OrderTypeLookup**: Sale, Return, Exchange
- **OrderStatusLookup**: Pending, Completed, Cancelled
- **FinancialTransactionTypeLookup**: Sale, Purchase, Payment
- **FinancialTransactionStatusLookup**: Pending, Completed, Failed
- **TransactionTypeLookup**: General transaction types
- **TransactionStatusLookup**: General transaction statuses

### Repair & Service Lookups
- **RepairStatusLookup**: Received, In Progress, Completed
- **RepairPriorityLookup**: Low, Medium, High, Urgent

## Database Schema Design

All lookup models use the `Lookup` schema with consistent naming:
- Table names end with "Lookup"
- Unique constraints on Name field (filtered for IsActive = 1)
- Standard audit fields from BaseEntity

## Business Rules

1. **Unique Names**: Names must be unique among active records only
2. **Soft Delete**: Lookups are never hard deleted to preserve referential integrity
3. **Display Order**: Optional ordering for UI display
4. **System Lookups**: Some lookups are system-managed and cannot be deleted

## API Endpoints

- `/api/lookup/karat-types` - Karat type management
- `/api/lookup/product-categories` - Product category management
- `/api/lookup/order-statuses` - Order status management
- etc.

## Service Dependencies

- `ILookupService<T>` - Generic lookup operations
- Specific services for each lookup type
- Caching layer for performance optimization

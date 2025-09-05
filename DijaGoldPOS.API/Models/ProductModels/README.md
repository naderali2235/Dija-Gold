# Product Module Models

This module contains all product-related entities including catalog management, pricing, and configuration.

## Models Overview

### Product
- **Purpose**: Core product catalog entity
- **Key Features**:
  - Unique product codes with soft delete support
  - Multi-category support (Jewelry, Bullion, Coins)
  - Weight-based pricing
  - Brand and design tracking
- **Schema**: `Product`
- **Relationships**: Many-to-one with Supplier, Lookup tables

### GoldRate
- **Purpose**: Daily gold rate management with versioning
- **Key Features**:
  - Karat-specific rates
  - Time-based versioning
  - Current rate tracking
- **Schema**: `Product`
- **Relationships**: Many-to-one with KaratTypeLookup

### MakingCharges
- **Purpose**: Manufacturing cost configuration
- **Key Features**:
  - Category and sub-category specific
  - Multiple charge types (Fixed, Percentage, Per Gram)
  - Time-based versioning
- **Schema**: `Product`
- **Relationships**: Many-to-one with ProductCategoryTypeLookup, ChargeTypeLookup

### TaxConfiguration
- **Purpose**: Tax rate management
- **Key Features**:
  - Multiple tax types support
  - Rate versioning
  - Category-specific configuration
- **Schema**: `Product`

## Database Schema Design

All product models use the `Product` schema for logical separation.

## Business Rules

1. **Product Codes**: Must be unique among active products only
2. **Gold Rates**: Only one current rate per karat type at any time
3. **Making Charges**: Effective date ranges cannot overlap for same category
4. **Soft Delete**: Products maintain referential integrity through soft delete

## Unique Constraints

- Product codes: Unique constraint excludes inactive records
- Gold rates: Composite unique on (KaratTypeId, EffectiveFrom)
- Tax codes: Unique constraint excludes inactive records

## API Endpoints

- `/api/product/products` - Product catalog management
- `/api/product/gold-rates` - Gold rate management
- `/api/product/making-charges` - Making charges configuration
- `/api/product/tax-configuration` - Tax setup

## Service Dependencies

- `IProductService` - Product catalog operations
- `IGoldRateService` - Rate management
- `IPricingService` - Price calculations
- `ITaxService` - Tax calculations

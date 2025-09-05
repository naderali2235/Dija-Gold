# Customer Module Models

This module contains all customer-related entities and business logic for customer management and relationship tracking.

## Models Overview

### Customer
- **Purpose**: Customer information and relationship management
- **Key Features**:
  - Unique customer identification (NationalId, Mobile, Email)
  - Loyalty program integration
  - Purchase history tracking
  - Discount and preference management
- **Schema**: `Customer`
- **Relationships**: One-to-many with Orders, CustomerPurchases

### CustomerPurchase
- **Purpose**: Direct gold purchase transactions from customers
- **Key Features**:
  - Gold weight and karat tracking
  - Purchase value calculation
  - Payment method tracking
- **Schema**: `Customer`
- **Relationships**: Many-to-one with Customer, One-to-many with CustomerPurchaseItems

### CustomerPurchaseItem
- **Purpose**: Individual items in customer purchase transactions
- **Key Features**:
  - Weight and karat specification
  - Unit price and total calculation
  - Item-specific notes
- **Schema**: `Customer`
- **Relationships**: Many-to-one with CustomerPurchase, KaratType

## Database Schema Design

All customer models use the `Customer` schema for logical separation.

## Business Rules

1. **Unique Identifiers**: NationalId, Mobile, and Email must be unique among active customers
2. **Loyalty Tiers**: Automatic tier calculation based on purchase history
3. **Purchase Validation**: Weight and karat combinations must be valid
4. **Discount Application**: Tier-based and customer-specific discounts

## Unique Constraints

- Customer identifiers: Filtered unique constraints excluding inactive records
- Purchase numbers: Unique constraint with branch scoping

## API Endpoints

- `/api/customer/customers` - Customer management
- `/api/customer/purchases` - Customer purchase tracking
- `/api/customer/loyalty` - Loyalty program management

## Service Dependencies

- `ICustomerService` - Customer operations and validation
- `ICustomerPurchaseService` - Purchase transaction management
- `ILoyaltyService` - Loyalty program calculations
- `INotificationService` - Customer communications

# Core Module Models

This module contains the fundamental business entities that form the backbone of the Dija Gold POS system.

## Models Overview

### Branch
- **Purpose**: Represents physical store locations
- **Key Features**: 
  - Unique branch codes
  - Manager assignment
  - Location details
- **Schema**: `Core`
- **Relationships**: One-to-many with Users, Orders, FinancialTransactions

### ApplicationUser
- **Purpose**: System users extending ASP.NET Identity
- **Key Features**:
  - Role-based access control
  - Branch assignment
  - Audit trail integration
- **Schema**: `Identity` (ASP.NET Identity default)
- **Relationships**: Many-to-one with Branch, One-to-many with Orders/Transactions

### AuditLog
- **Purpose**: Comprehensive audit trail for all system operations
- **Key Features**:
  - User action tracking
  - Entity change history
  - IP and session tracking
- **Schema**: `Audit`
- **Relationships**: References to all major entities

## Database Schema Design

All core models use the `Core` schema to maintain separation from other business modules.

## Business Rules

1. **Branch Codes**: Must be unique and cannot be reused even for inactive branches
2. **User Assignment**: Users must be assigned to at least one branch
3. **Audit Trail**: All operations are automatically logged with user context

## API Endpoints

- `/api/core/branches` - Branch management
- `/api/core/users` - User management
- `/api/core/audit` - Audit log queries

## Service Dependencies

- `IBranchService` - Branch operations
- `IUserService` - User management
- `IAuditService` - Audit logging

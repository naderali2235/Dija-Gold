# Dija Gold POS System - ASP.NET Core Web API

## Overview

A comprehensive Point of Sale (POS) system specifically designed for the Egyptian gold retail industry. Built with ASP.NET Core 8.0, Entity Framework Core, and following enterprise-grade architecture patterns.

## Features

### ✅ **Egyptian Gold Market Compliance**
- Multi-karat support (18K, 21K, 22K, 24K)
- Weight-based inventory (grams with 3 decimal precision)
- Egyptian Pound (EGP) currency formatting
- VAT and local tax compliance (14% Egyptian standard)
- Making charges (percentage & fixed amount)

### ✅ **Complete POS Functionality**
- **Sales Transactions**: Full transaction processing with inventory management
- **Return Transactions**: Manager-approved returns with inventory restoration
- **Repair Transactions**: Service transaction management
- **Receipt Generation**: Professional receipt printing with Egyptian market formatting
- **Multi-branch Support**: Centralized management with branch-specific operations

### ✅ **Advanced Business Logic**
- **Dynamic Pricing**: `(Gold Weight × Daily Rate × Karat Factor) + Making Charges + Taxes - Discounts`
- **Customer Loyalty**: Tier-based discounts and making charge waivers
- **Inventory Management**: Real-time tracking with movement history
- **Audit Trail**: Complete user activity and data change logging

### ✅ **Enterprise Architecture**
- **Clean Architecture**: Proper separation of concerns
- **Role-Based Security**: Manager/Cashier permissions with JWT authentication
- **RESTful APIs**: Standard HTTP methods and status codes
- **Comprehensive Logging**: Structured logging with Serilog
- **Error Handling**: Consistent error responses

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login with JWT token generation
- `POST /api/auth/logout` - User logout
- `POST /api/auth/change-password` - Change user password
- `GET /api/auth/me` - Get current user information
- `POST /api/auth/refresh-token` - Refresh JWT token

### Products
- `GET /api/products` - Get products with filtering and pagination
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/{id}/inventory` - Get product with inventory information
- `POST /api/products` - Create new product (Manager only)
- `PUT /api/products/{id}` - Update product (Manager only)
- `DELETE /api/products/{id}` - Deactivate product (Manager only)
- `GET /api/products/{id}/pricing` - Get product pricing information

### Transactions
- `POST /api/transactions/sale` - Process sale transaction
- `POST /api/transactions/return` - Process return transaction (Manager only)
- `POST /api/transactions/repair` - Process repair transaction
- `GET /api/transactions/{id}` - Get transaction by ID
- `GET /api/transactions/by-number/{transactionNumber}` - Get transaction by number
- `GET /api/transactions/search` - Search transactions with filtering
- `POST /api/transactions/cancel` - Cancel transaction (Manager only)
- `POST /api/transactions/reprint-receipt` - Reprint receipt
- `GET /api/transactions/summary` - Get transaction summary

### Pricing Management
- `GET /api/pricing/gold-rates` - Get current gold rates
- `POST /api/pricing/gold-rates` - Update gold rates (Manager only)
- `GET /api/pricing/making-charges` - Get current making charges
- `POST /api/pricing/making-charges` - Update making charges (Manager only)
- `GET /api/pricing/taxes` - Get tax configurations
- `POST /api/pricing/calculate` - Calculate product pricing

### Inventory Management
- `GET /api/inventory/product/{productId}/branch/{branchId}` - Get specific inventory
- `GET /api/inventory/branch/{branchId}` - Get branch inventory
- `GET /api/inventory/branch/{branchId}/low-stock` - Get low stock items
- `GET /api/inventory/check-availability` - Check stock availability
- `POST /api/inventory/add` - Add inventory (Manager only)
- `POST /api/inventory/adjust` - Adjust inventory (Manager only)
- `POST /api/inventory/transfer` - Transfer inventory between branches (Manager only)
- `GET /api/inventory/movements` - Get inventory movement history

### Reports
- `GET /api/reports/daily-sales-summary` - Daily sales summary
- `GET /api/reports/cash-reconciliation` - Cash reconciliation report
- `GET /api/reports/inventory-movement` - Inventory movement report
- `GET /api/reports/profit-analysis` - Profit analysis (Manager only)
- `GET /api/reports/customer-analysis` - Customer analysis (Manager only)
- `GET /api/reports/supplier-balance` - Supplier balance (Manager only)
- `GET /api/reports/inventory-valuation` - Inventory valuation (Manager only)
- `GET /api/reports/tax-report` - Tax report (Manager only)
- `GET /api/reports/transaction-log` - Transaction log
- `POST /api/reports/export/excel` - Export to Excel
- `POST /api/reports/export/pdf` - Export to PDF
- `GET /api/reports/types` - Get available report types

## Project Structure

```
DijaGoldPOS.API/
├── Controllers/           # API Controllers
│   ├── AuthController.cs
│   ├── ProductsController.cs
│   ├── TransactionsController.cs
│   ├── PricingController.cs
│   ├── InventoryController.cs
│   └── ReportsController.cs
├── Data/                  # Database Context and Initialization
│   ├── ApplicationDbContext.cs
│   └── DbInitializer.cs
├── DTOs/                  # Data Transfer Objects
│   ├── AuthenticationDtos.cs
│   ├── ProductDtos.cs
│   └── TransactionDtos.cs
├── Models/                # Entity Models
│   ├── Enums/
│   ├── ApplicationUser.cs
│   ├── Product.cs
│   ├── Transaction.cs
│   ├── Customer.cs
│   ├── Supplier.cs
│   ├── Branch.cs
│   ├── GoldRate.cs
│   ├── MakingCharges.cs
│   ├── TaxConfiguration.cs
│   ├── Inventory.cs
│   └── AuditLog.cs
├── Services/              # Business Logic Services
│   ├── ITokenService.cs & TokenService.cs
│   ├── IPricingService.cs & PricingService.cs
│   ├── IInventoryService.cs & InventoryService.cs
│   ├── ITransactionService.cs & TransactionService.cs
│   ├── IReportService.cs & ReportService.cs
│   ├── IReceiptService.cs & ReceiptService.cs
│   └── IAuditService.cs & AuditService.cs
├── Program.cs             # Application startup
├── appsettings.json       # Configuration
└── DijaGoldPOS.API.csproj # Project file
```

## Configuration

### Database Connection
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DijaGoldPOS;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

### JWT Configuration
```json
{
  "Jwt": {
    "Key": "YourSecretKeyHere",
    "Issuer": "DijaGoldPOS",
    "Audience": "DijaGoldPOSUsers",
    "ExpiryInHours": 8
  }
}
```

### Company Information
```json
{
  "CompanyInfo": {
    "Name": "Dija Gold",
    "Address": "123 Gold Street, Cairo, Egypt",
    "Phone": "+20 2 1234567",
    "TaxRegistrationNumber": "123-456-789"
  }
}
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or Full)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone [repository-url]
   cd DijaGoldPOS.API
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Update database connection string** in `appsettings.json`

4. **Create and seed the database**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access Swagger UI**
   Navigate to `https://localhost:5001/swagger` for API documentation

### Default Users

The system creates default users during initialization:

**Manager Account:**
- Email: `manager@dijagold.com`
- Password: `Manager123!`
- Role: Manager

**Cashier Account:**
- Email: `cashier@dijagold.com`
- Password: `Cashier123!`
- Role: Cashier

## Database Schema

### Key Entities
- **Products**: Gold jewelry, bullion, and coins with karat types
- **Transactions**: Sales, returns, and repairs with complete audit trail
- **Inventory**: Real-time stock tracking with movement history
- **Pricing**: Versioned gold rates, making charges, and tax configurations
- **Users**: Role-based access with branch assignments
- **Customers**: Loyalty program with tier-based benefits
- **Suppliers**: Credit management and purchase tracking

### Business Rules
- All products must have karat classification (18K, 21K, 22K, 24K)
- Inventory tracked to 3 decimal places (0.001g precision)
- Manager approval required for returns and high-value operations
- Complete audit trail for all business operations
- Automatic GL posting for financial transactions

## Security Features

### Authentication & Authorization
- JWT token-based authentication
- Role-based access control (Manager/Cashier)
- Session management with automatic logout
- Password complexity requirements
- Account lockout protection

### Audit & Compliance
- Complete user activity logging
- Data change tracking with old/new values
- Immutable transaction records
- IP address and user agent tracking
- Financial transaction audit trail

## API Response Format

All API endpoints return responses in a consistent format:

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { /* response data */ },
  "errors": null
}
```

Error responses include detailed information:

```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": {
    "field1": ["Error message 1"],
    "field2": ["Error message 2"]
  }
}
```

## Development Guidelines

### Code Standards
- Follow Microsoft C# coding conventions
- Use async/await for I/O operations
- Implement proper error handling
- Add comprehensive logging
- Include XML documentation comments

### Testing
- Unit tests for business logic
- Integration tests for API endpoints
- Mock external dependencies
- Test error scenarios

### Deployment
- Use environment-specific configurations
- Enable HTTPS in production
- Configure proper logging levels
- Set up monitoring and health checks

## Future Enhancements

- Real-time notifications
- Advanced reporting dashboard
- Mobile application support
- Integration with external systems
- Multi-language support (Arabic)
- Barcode scanning integration

## Support

For technical support or questions about the Dija Gold POS system, please contact the development team.

---

**Built with ❤️ for the Egyptian Gold Retail Industry**
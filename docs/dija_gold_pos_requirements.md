# Dija Gold POS System - Business Requirements Document

## Project Overview

**System Name:** Dija Gold Point of Sale (POS) System  
**Target Market:** Egypt Gold Retail Industry  
**Technology Stack:**
- Backend: ASP.NET Core Web API
- Database: SQL Server with Entity Framework Core (EF Core)
- Frontend: Angular
- Architecture: Client-Server Model

## 1. Product Catalog Management

### 1.1 Product Categories

#### Gold Jewelry
- **Attributes:**
  - Weight (grams) - decimal precision to 3 places
  - Karat type (18K, 21K, 22K, 24K) - enum
  - Brand (branded or custom-made by merchant)
  - Design/Style name
  - Category (rings, necklaces, bracelets, earrings, etc.)
  - Making charges applicable
  - Product code/SKU

#### Bullion
- **Attributes:**
  - Weight range: 0.25g to 100g
  - Karat type (18K, 21K, 22K, 24K)
  - Brand (branded or custom-made by merchant)
  - Shape/Form (bars, ingots, etc.)
  - Purity certificate number (if applicable)
  - Product code/SKU

#### Coins
- **Attributes:**
  - Weight range: 0.25g to 1g
  - Karat type
  - Denomination/Face value (if applicable)
  - Country of origin
  - Year of minting
  - Numismatic vs. bullion value
  - Product code/SKU

### 1.2 Product Master Data
- Each product must have a unique identifier
- Creation and modification timestamps
- Active/Inactive status
- Branch-specific availability
- Supplier information linkage

## 2. Pricing Management System

### 2.1 Daily Gold Rate Management
- **Manual Entry System:** Daily gold rate input by authorized personnel
- **Versioning Required:** 
  - Effective DateTime (start)
  - End DateTime
  - Rate per gram by karat type
  - Entry user and timestamp
- **Rate Updates:** Can change multiple times per day
- **Historical Tracking:** Complete audit trail of all rate changes

### 2.2 Making Charges
- **Versioned System:** Track changes over time
- **Charge Types:**
  - Percentage-based (% of gold value)
  - Fixed amount (EGP)
- **Product-Specific:** Different charges per product category
- **Effective Dating:** Start and end dates for each version

### 2.3 Tax Management
- **Custom Tax Configuration:** Flexible tax setup
- **Versioned System:** Track tax changes over time
- **Tax Types:**
  - Percentage-based (VAT, etc.)
  - Fixed amount taxes
- **Multiple Taxes:** Support for multiple concurrent taxes
- **Egyptian Tax Compliance:** Support for local tax requirements

### 2.4 Price Calculation Formula
```
Final Price = (Gold Weight × Daily Rate × Karat Factor) + Making Charges + Taxes
```

## 3. Transaction Management

### 3.1 Transaction Types

#### Sales Transactions
- **Process Flow:**
  1. Product selection and quantity
  2. Price calculation with current rates
  3. Customer information capture
  4. Payment processing
  5. Receipt generation
  6. Inventory deduction
  7. General ledger posting

#### Return Transactions
- **Requirements:**
  - Manager approval mandatory
  - Manual return amount entry
  - Original receipt reference
  - Reason code selection
  - Inventory adjustment
  - Reverse general ledger entries

#### Repair Transactions
- **Process:**
  - Manual amount entry for repair charges
  - Service description
  - Estimated completion date
  - Customer notification system
  - Payment handling

### 3.2 General Ledger Integration
- **Automatic Posting:** All transactions auto-post to GL
- **Daily Settlement:** End-of-day settlement process
- **Reverse Entries:** Returns create offsetting GL entries
- **Account Mapping:** Predefined account codes for different transaction types

## 4. Inventory Management

### 4.1 Weight-Based Tracking
- **Precision:** Track to 3 decimal places (0.001g)
- **Real-time Updates:** Immediate inventory adjustments
- **Stock Levels:** Current weight on hand by product
- **Movement History:** Complete transaction trail

### 4.2 Karat Type Classification
- **Mandatory Field:** All products must have karat classification
- **Standard Types:** 18K, 21K, 22K, 24K (Egyptian market standard)
- **Separate Tracking:** Inventory tracked separately by karat

### 4.3 Multi-Branch Support
- **Branch Hierarchy:** Support for multiple stores/branches
- **Inter-branch Transfers:** Stock movement between locations
- **Branch-Specific Reporting:** Inventory reports by location
- **Centralized Management:** Headquarters visibility of all branches

## 5. Payment Processing

### 5.1 Payment Methods
- **Cash Only:** Currently limited to cash transactions
- **Currency:** Egyptian Pounds (EGP)
- **Change Calculation:** Automatic change computation
- **Payment Validation:** Ensure sufficient payment received

### 5.2 Cash Management
- **Cash Drawer Integration:** Opening/closing procedures
- **Daily Cash Reconciliation:** End-of-day cash counting
- **Cash-Over/Short Tracking:** Variance reporting

## 6. User Management & Security

### 6.1 User Roles

#### Cashier Role
- **Permissions:**
  - Process sales transactions
  - View current pricing
  - Generate receipts
  - Basic customer lookup
  - Cash drawer operations

#### Manager Role
- **Additional Permissions:**
  - Update daily gold rates
  - Modify making charges and taxes
  - Approve returns
  - Access all reports
  - User management
  - Supplier management
  - End-of-day procedures

### 6.2 Security Features
- **User Authentication:** Secure login system
- **Role-Based Access:** Permissions by user role
- **Audit Trail:** Complete user activity logging
- **Session Management:** Automatic logout after inactivity

## 7. Customer Management

### 7.1 Customer Data Structure
- **Name:** Full customer name (required)
- **ID Number:** National ID (optional, unique if provided)
- **Mobile Number:** Egyptian mobile format (optional, unique if provided)
- **Email:** Valid email address (optional, unique if provided)
- **Registration Date:** Account creation timestamp

### 7.2 Loyalty Program
- **Discount Management:** 
  - Percentage-based discounts
  - Fixed amount discounts
  - Product-specific discounts
- **Charge Waivers:** Option to waive making charges
- **Tax Exemptions:** Loyalty-based tax reductions
- **Tier System:** Different loyalty levels with varying benefits

## 8. Supplier Management

### 8.1 Supplier Information
- **Basic Data:**
  - Company name and contact information
  - Payment terms and conditions
  - Credit limits
  - Tax registration details

### 8.2 Purchase Management
- **Purchase Orders:** Generate and track purchase orders
- **Goods Receipt:** Record incoming inventory
- **Settlement Tracking:** Monitor paid vs. unpaid amounts
- **Supplier Balance:** Real-time balance calculations

### 8.3 Material Linkage
- **Product Traceability:** Link purchased materials to final products
- **Manufacturing Records:** Track custom-made items
- **Direct Sales:** Items sold without modification

### 8.4 Credit Management
- **Credit Limit Enforcement:** Warning system for exceeded limits
- **Settlement Alarms:** Notifications for unsettled amounts
- **Payment Scheduling:** Track payment due dates
- **Manager Oversight:** All supplier operations require manager approval

## 9. Accounting System

### 9.1 Chart of Accounts Structure

#### Revenue Accounts
- `4000` - Gold Jewelry Sales
- `4100` - Bullion Sales  
- `4200` - Coin Sales
- `4300` - Making Charges Revenue
- `4400` - Repair Services Revenue

#### Inventory Accounts
- `1300` - Gold Jewelry Inventory
- `1310` - Bullion Inventory
- `1320` - Coin Inventory

#### Cost of Goods Sold
- `5000` - Cost of Gold Jewelry Sold
- `5100` - Cost of Bullion Sold
- `5200` - Cost of Coins Sold

#### Tax Accounts
- `2100` - VAT Payable
- `2110` - Other Taxes Payable

#### Cash and Bank Accounts
- `1000` - Main Treasury
- `1001` - Branch 1 Cash
- `1002` - Branch 2 Cash
- `1010` - Petty Cash

#### Supplier Accounts
- `2000` - Accounts Payable - Suppliers
- `2001` - Supplier Advances

### 9.2 Treasury Management
- **Main Treasury:** Centralized cash management
- **Branch Treasuries:** Individual branch cash accounts
- **Daily Reconciliation:** Balance verification procedures
- **Cash Transfer Tracking:** Inter-branch cash movements

## 10. Receipt and Invoice Generation

### 10.1 Receipt Requirements
- **Company Information:** Dija Gold branding and details
- **Transaction Details:**
  - Date and time
  - Receipt number (sequential)
  - Cashier identification
  - Branch location
- **Item Details:**
  - Product description
  - Weight and karat
  - Unit price and total
  - Making charges
  - Taxes applied
- **Payment Information:**
  - Total amount
  - Payment method
  - Change given
- **Customer Information:** If provided
- **Return Policy:** Printed terms and conditions

### 10.2 Document Management
- **Digital Storage:** Electronic receipt archival
- **Reprint Capability:** Ability to reprint receipts
- **Legal Compliance:** Egyptian receipt requirements
- **Sequential Numbering:** Unique receipt numbering system

## 11. Reporting Requirements

### 11.1 Daily Reports
- **Daily Sales Summary:** Revenue by product category
- **Cash Reconciliation:** Expected vs. actual cash
- **Inventory Movement:** Stock changes during the day
- **Transaction Log:** Complete transaction listing

### 11.2 Management Reports
- **Profit Analysis:** Margin analysis by product
- **Supplier Balances:** Outstanding amounts owed
- **Customer Analysis:** Top customers and loyalty metrics
- **Inventory Valuation:** Current stock value at market rates

### 11.3 Regulatory Reports
- **Tax Reports:** VAT and other tax summaries
- **Financial Statements:** Integration with accounting system
- **Audit Trail:** Complete transaction history

## 12. System Integration Requirements

### 12.1 Database Design Considerations
- **Entity Framework Core:** Code-first approach
- **Data Integrity:** Proper foreign key relationships
- **Audit Fields:** Created/Modified timestamps and user tracking
- **Soft Deletes:** Logical deletion for critical records
- **Performance:** Proper indexing strategy

### 12.2 API Design
- **RESTful Services:** Standard HTTP methods and status codes
- **Authentication:** JWT token-based security
- **Error Handling:** Consistent error response format
- **Logging:** Comprehensive application logging
- **Documentation:** Swagger/OpenAPI documentation

### 12.3 Angular Frontend
- **Responsive Design:** Mobile and tablet compatibility
- **Real-time Updates:** Live pricing and inventory updates
- **Offline Capability:** Basic functionality during connectivity issues
- **Print Integration:** Direct printer support for receipts

## 13. Egyptian Market Considerations

### 13.1 Local Compliance
- **Currency:** Egyptian Pound (EGP) formatting
- **Tax Regulations:** VAT and local tax compliance
- **Business Hours:** Local business hour considerations
- **Arabic Language:** UI support for Arabic (future enhancement)

### 13.2 Gold Market Practices
- **Karat Standards:** Egyptian gold market preferences (21K, 18K popular)
- **Weight Units:** Gram-based measurements (standard in Egypt)
- **Pricing Models:** Local pricing conventions and practices
- **Documentation:** Receipt formats common in Egyptian gold trade

## 14. Performance and Scalability

### 14.1 Performance Requirements
- **Response Time:** Sub-second response for normal operations
- **Concurrent Users:** Support for 10+ simultaneous users per branch
- **Data Volume:** Handle thousands of transactions per day
- **Backup Strategy:** Regular automated backups

### 14.2 Scalability Considerations
- **Horizontal Scaling:** Support for additional branches
- **Data Archival:** Historical data management strategy
- **Load Balancing:** Future load balancing capabilities
- **Cloud Readiness:** Design for potential cloud deployment

## 15. Security and Data Protection

### 15.1 Data Security
- **Encryption:** Sensitive data encryption at rest and in transit
- **Access Control:** Role-based permissions throughout system
- **Session Security:** Secure session management
- **Data Backup:** Encrypted backup procedures

### 15.2 Audit Requirements
- **User Activity:** Complete user action logging
- **Data Changes:** Track all data modifications
- **Financial Transactions:** Immutable transaction records
- **System Access:** Login/logout audit trail

---

**Document Version:** 1.0  
**Date:** August 2025  
**Prepared for:** Dija Gold POS Implementation  
**Technology Team:** ASP.NET Core + Angular Development Team
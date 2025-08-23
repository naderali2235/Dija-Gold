# Dija Gold POS - Comprehensive Test Scenarios

## Overview
This document provides comprehensive test scenarios for the Dija Gold POS application, covering all major features including authentication, sales, inventory, repairs, cash management, and reporting.

## Test Environment Setup

### Prerequisites
- Backend API running on localhost:5000
- Frontend React app running on localhost:3000
- SQL Server database with test data
- Test users with different roles (Cashier, Manager, Admin)
- Test products, customers, suppliers, and branches

### Test Data Requirements
- At least 3 branches
- 10+ products across different categories (Rings, Necklaces, Bracelets, etc.)
- 5+ customers with different profiles
- 3+ suppliers
- 2+ technicians
- Gold rates for different karats (18k, 21k, 22k, 24k)

---

## 1. Authentication & User Management

### 1.1 User Login Scenarios

#### Scenario 1.1.1: Successful Login
**Objective**: Verify successful user authentication
**Steps**:
1. Navigate to login page
2. Enter valid username/email and password
3. Click "Login" button
**Expected Result**: 
- User is redirected to dashboard
- JWT token is stored
- User information is displayed correctly
- Audit log entry is created

#### Scenario 1.1.2: Invalid Credentials
**Objective**: Verify login validation
**Steps**:
1. Enter invalid username/password
2. Click "Login" button
**Expected Result**: 
- Error message displayed
- User remains on login page
- Failed login attempt logged

#### Scenario 1.1.3: Account Lockout
**Objective**: Verify account security
**Steps**:
1. Attempt login with wrong password 5 times
2. Try to login again
**Expected Result**: 
- Account locked message
- Cannot login until lockout period expires
- Security audit logged

#### Scenario 1.1.4: Inactive User Login
**Objective**: Verify inactive user handling
**Steps**:
1. Try to login with inactive user account
**Expected Result**: 
- "User not found or inactive" error
- Login attempt logged

### 1.2 User Management Scenarios

#### Scenario 1.2.1: Create New User
**Objective**: Verify user creation workflow
**Steps**:
1. Login as Manager/Admin
2. Navigate to Users section
3. Click "Add New User"
4. Fill required fields (name, email, role, branch)
5. Submit form
**Expected Result**: 
- User created successfully
- Email sent to new user
- Audit log entry created

#### Scenario 1.2.2: Update User Profile
**Objective**: Verify profile update functionality
**Steps**:
1. Login as any user
2. Navigate to profile settings
3. Update personal information
4. Save changes
**Expected Result**: 
- Profile updated successfully
- Changes reflected immediately
- Audit log entry created

---

## 2. Product Management

### 2.1 Product Creation & Management

#### Scenario 2.1.1: Add New Product
**Objective**: Verify product creation workflow
**Steps**:
1. Login as Manager
2. Navigate to Products section
3. Click "Add New Product"
4. Fill product details:
   - Name: "Gold Ring 18k"
   - Category: Rings
   - Karat: 18k
   - Weight: 5.5g
   - Supplier: Select existing supplier
   - Cost Price: $500
   - Selling Price: $750
5. Save product
**Expected Result**: 
- Product created successfully
- Product code generated automatically
- Product appears in inventory
- Audit log entry created

#### Scenario 2.1.2: Update Product Information
**Objective**: Verify product update functionality
**Steps**:
1. Search for existing product
2. Click "Edit"
3. Update price or other details
4. Save changes
**Expected Result**: 
- Product updated successfully
- Changes reflected in all related screens
- Audit trail maintained

#### Scenario 2.1.3: Product Search & Filtering
**Objective**: Verify search functionality
**Steps**:
1. Navigate to Products section
2. Use search bar to find products by:
   - Product name
   - Product code
   - Brand
3. Apply filters:
   - Category
   - Karat type
   - Supplier
   - Active/Inactive status
**Expected Result**: 
- Search results displayed correctly
- Filters work as expected
- Pagination works properly

### 2.2 Product Ownership Management

#### Scenario 2.2.1: Create Product Ownership
**Objective**: Verify ownership creation
**Steps**:
1. Navigate to Product Ownership section
2. Click "Add New Ownership"
3. Select product and customer
4. Set ownership details:
   - Purchase Date
   - Purchase Amount
   - Payment Terms
   - Notes
5. Save ownership
**Expected Result**: 
- Ownership created successfully
- Customer linked to product
- Payment tracking initiated
- Audit log entry created

#### Scenario 2.2.2: Update Ownership Status
**Objective**: Verify ownership workflow
**Steps**:
1. Find existing ownership record
2. Update payment status
3. Add payment records
4. Mark as fully paid
**Expected Result**: 
- Status updated correctly
- Payment history maintained
- Notifications sent if applicable

---

## 3. Sales & Order Management

### 3.1 Sales Transaction Scenarios

#### Scenario 3.1.1: Complete Sale Transaction
**Objective**: Verify end-to-end sale process
**Steps**:
1. Login as Cashier
2. Navigate to Sales section
3. Create new sale:
   - Select customer
   - Add products to cart
   - Apply any discounts
   - Select payment method
   - Process payment
4. Complete transaction
**Expected Result**: 
- Sale completed successfully
- Receipt generated
- Inventory updated
- Financial transaction recorded
- Customer history updated

#### Scenario 3.1.2: Sale with Multiple Payment Methods
**Objective**: Verify split payment functionality
**Steps**:
1. Create sale with multiple items
2. Use different payment methods:
   - Cash: $300
   - Card: $200
   - Bank Transfer: $100
3. Complete transaction
**Expected Result**: 
- All payments recorded correctly
- Receipt shows payment breakdown
- Cash drawer updated
- Audit trail maintained

#### Scenario 3.1.3: Sale with Discounts
**Objective**: Verify discount application
**Steps**:
1. Create sale with multiple items
2. Apply different discount types:
   - Percentage discount on specific items
   - Fixed amount discount
   - Customer loyalty discount
3. Complete transaction
**Expected Result**: 
- Discounts applied correctly
- Final amount calculated properly
- Discount reasons logged
- Receipt shows discount breakdown

### 3.2 Return & Refund Scenarios

#### Scenario 3.2.1: Process Return
**Objective**: Verify return workflow
**Steps**:
1. Navigate to Returns section
2. Search for original sale
3. Select items to return
4. Specify return reason
5. Process refund
**Expected Result**: 
- Return processed successfully
- Refund issued
- Inventory updated
- Customer account credited
- Return receipt generated

#### Scenario 3.2.2: Partial Return
**Objective**: Verify partial return functionality
**Steps**:
1. Find multi-item sale
2. Return only some items
3. Process partial refund
**Expected Result**: 
- Partial return processed
- Only returned items refunded
- Original receipt updated
- Inventory adjusted correctly

---

## 4. Inventory Management

### 4.1 Inventory Operations

#### Scenario 4.1.1: Stock Count
**Objective**: Verify inventory counting
**Steps**:
1. Navigate to Inventory section
2. Select branch
3. Perform stock count for products
4. Enter actual quantities
5. Submit count
**Expected Result**: 
- Count recorded successfully
- Discrepancies highlighted
- Inventory updated
- Count report generated

#### Scenario 4.1.2: Stock Transfer Between Branches
**Objective**: Verify inter-branch transfers
**Steps**:
1. Navigate to Inventory section
2. Select "Transfer Stock"
3. Choose source and destination branches
4. Select products and quantities
5. Process transfer
**Expected Result**: 
- Transfer completed successfully
- Both branches' inventory updated
- Transfer receipt generated
- Audit trail maintained

#### Scenario 4.1.3: Low Stock Alerts
**Objective**: Verify stock monitoring
**Steps**:
1. Set low stock thresholds for products
2. Perform sales that reduce stock below threshold
3. Check dashboard for alerts
**Expected Result**: 
- Low stock alerts displayed
- Email notifications sent (if configured)
- Reorder suggestions provided

### 4.2 Purchase Order Management

#### Scenario 4.2.1: Create Purchase Order
**Objective**: Verify PO creation workflow
**Steps**:
1. Navigate to Purchase Orders section
2. Click "Create New PO"
3. Select supplier
4. Add products with quantities and prices
5. Set delivery date
6. Submit PO
**Expected Result**: 
- PO created successfully
- PO number generated
- Supplier notified (if configured)
- PO status tracked

#### Scenario 4.2.2: Receive Purchase Order
**Objective**: Verify PO receiving process
**Steps**:
1. Find existing PO
2. Click "Receive Items"
3. Enter received quantities
4. Update inventory
5. Mark PO as received
**Expected Result**: 
- Items received successfully
- Inventory updated
- PO status updated
- Receipt generated

---

## 5. Repair Job Management

### 5.1 Repair Workflow Scenarios

#### Scenario 5.1.1: Create Repair Job
**Objective**: Verify repair job creation
**Steps**:
1. Navigate to Repairs section
2. Click "New Repair Job"
3. Fill repair details:
   - Customer information
   - Item description
   - Repair type
   - Priority level
   - Estimated cost
4. Assign technician
5. Save repair job
**Expected Result**: 
- Repair job created successfully
- Job number generated
- Technician assigned
- Customer notified
- Status tracking initiated

#### Scenario 5.1.2: Update Repair Status
**Objective**: Verify repair workflow
**Steps**:
1. Find existing repair job
2. Update status through workflow:
   - In Progress
   - Awaiting Parts
   - Ready for Pickup
   - Completed
3. Add notes at each stage
**Expected Result**: 
- Status updated correctly
- Timeline maintained
- Customer notifications sent
- Audit trail preserved

#### Scenario 5.1.3: Complete Repair with Payment
**Objective**: Verify repair completion
**Steps**:
1. Mark repair as "Ready for Pickup"
2. Process customer payment
3. Complete repair job
**Expected Result**: 
- Payment processed successfully
- Repair marked as completed
- Receipt generated
- Customer history updated

---

## 6. Cash Management

### 6.1 Cash Drawer Operations

#### Scenario 6.1.1: Open Cash Drawer
**Objective**: Verify drawer opening process
**Steps**:
1. Login as Cashier
2. Navigate to Cash Drawer section
3. Click "Open Drawer"
4. Enter opening balance
5. Add notes
6. Submit
**Expected Result**: 
- Drawer opened successfully
- Opening balance recorded
- User assigned to drawer
- Audit log entry created

#### Scenario 6.1.2: Close Cash Drawer
**Objective**: Verify drawer closing process
**Steps**:
1. Navigate to Cash Drawer section
2. Click "Close Drawer"
3. Enter actual closing balance
4. Add notes
5. Submit
**Expected Result**: 
- Drawer closed successfully
- Closing balance recorded
- Variance calculated
- Settlement report generated

#### Scenario 6.1.3: Cash Reconciliation
**Objective**: Verify cash reconciliation
**Steps**:
1. After closing drawer
2. Review cash reconciliation report
3. Verify all transactions
4. Investigate any variances
**Expected Result**: 
- Report shows all cash transactions
- Variances highlighted
- Audit trail maintained
- Manager approval recorded

### 6.2 Financial Transactions

#### Scenario 6.2.1: Record Cash Transaction
**Objective**: Verify cash transaction recording
**Steps**:
1. Process cash sale
2. Verify cash drawer balance updated
3. Check transaction log
**Expected Result**: 
- Cash amount recorded correctly
- Drawer balance updated
- Transaction logged
- Receipt generated

#### Scenario 6.2.2: Record Non-Cash Transaction
**Objective**: Verify non-cash transaction handling
**Steps**:
1. Process card/bank transfer sale
2. Verify transaction recording
3. Check financial reports
**Expected Result**: 
- Transaction recorded correctly
- Payment method tracked
- Financial reports updated
- Audit trail maintained

---

## 7. Customer Management

### 7.1 Customer Operations

#### Scenario 7.1.1: Add New Customer
**Objective**: Verify customer creation
**Steps**:
1. Navigate to Customers section
2. Click "Add New Customer"
3. Fill customer details:
   - Name, phone, email
   - Address information
   - ID document details
4. Save customer
**Expected Result**: 
- Customer created successfully
- Customer ID generated
- Profile saved
- Audit log entry created

#### Scenario 7.1.2: Update Customer Information
**Objective**: Verify customer update functionality
**Steps**:
1. Search for existing customer
2. Edit customer information
3. Update contact details
4. Save changes
**Expected Result**: 
- Information updated successfully
- Changes reflected immediately
- History maintained
- Audit trail preserved

#### Scenario 7.1.3: Customer Purchase History
**Objective**: Verify customer history tracking
**Steps**:
1. Find customer profile
2. View purchase history
3. Check ownership records
4. Review payment history
**Expected Result**: 
- Complete history displayed
- All transactions listed
- Ownership records shown
- Payment status tracked

---

## 8. Reporting & Analytics

### 8.1 Sales Reports

#### Scenario 8.1.1: Daily Sales Summary
**Objective**: Verify daily sales reporting
**Steps**:
1. Navigate to Reports section
2. Select "Daily Sales Summary"
3. Choose branch and date
4. Generate report
**Expected Result**: 
- Report generated successfully
- Sales totals calculated correctly
- Payment method breakdown shown
- Export functionality works

#### Scenario 8.1.2: Sales by Product Category
**Objective**: Verify category-based reporting
**Steps**:
1. Generate sales report by category
2. Filter by date range
3. Export report
**Expected Result**: 
- Category totals calculated
- Date filtering works
- Export successful
- Data accuracy verified

### 8.2 Financial Reports

#### Scenario 8.2.1: Cash Reconciliation Report
**Objective**: Verify cash reconciliation reporting
**Steps**:
1. Generate cash reconciliation report
2. Review cash vs. system totals
3. Investigate variances
**Expected Result**: 
- Report shows all cash transactions
- Variances highlighted
- Audit trail maintained
- Manager approval workflow

#### Scenario 8.2.2: Profit & Loss Report
**Objective**: Verify financial reporting
**Steps**:
1. Generate P&L report
2. Review revenue and expenses
3. Check profit calculations
**Expected Result**: 
- Revenue calculated correctly
- Expenses included
- Profit margins accurate
- Export functionality works

### 8.3 Inventory Reports

#### Scenario 8.3.1: Low Stock Report
**Objective**: Verify inventory reporting
**Steps**:
1. Generate low stock report
2. Review items below threshold
3. Create reorder suggestions
**Expected Result**: 
- Low stock items identified
- Quantities accurate
- Reorder suggestions provided
- Report exportable

#### Scenario 8.3.2: Stock Movement Report
**Objective**: Verify stock movement tracking
**Steps**:
1. Generate stock movement report
2. Review in/out movements
3. Check balance calculations
**Expected Result**: 
- All movements recorded
- Balances calculated correctly
- Audit trail maintained
- Date filtering works

---

## 9. System Administration

### 9.1 Branch Management

#### Scenario 9.1.1: Add New Branch
**Objective**: Verify branch creation
**Steps**:
1. Login as Admin
2. Navigate to Branches section
3. Add new branch details
4. Configure branch settings
**Expected Result**: 
- Branch created successfully
- Settings configured
- Users can be assigned
- Audit log entry created

### 9.2 Lookup Table Management

#### Scenario 9.2.1: Update Gold Rates
**Objective**: Verify rate management
**Steps**:
1. Navigate to Settings
2. Update gold rates for different karats
3. Set effective date
4. Save changes
**Expected Result**: 
- Rates updated successfully
- Effective date applied
- All calculations use new rates
- History maintained

#### Scenario 9.2.2: Manage Product Categories
**Objective**: Verify category management
**Steps**:
1. Add new product category
2. Update existing categories
3. Set category hierarchy
**Expected Result**: 
- Categories managed correctly
- Hierarchy maintained
- Products can be assigned
- Audit trail preserved

---

## 10. Integration Testing

### 10.1 End-to-End Workflows

#### Scenario 10.1.1: Complete Customer Journey
**Objective**: Verify full customer experience
**Steps**:
1. Create new customer
2. Add products to inventory
3. Process sale transaction
4. Create repair job
5. Complete repair
6. Generate customer report
**Expected Result**: 
- All systems work together
- Data consistency maintained
- Customer experience smooth
- Audit trail complete

#### Scenario 10.1.2: Daily Operations Workflow
**Objective**: Verify daily business operations
**Steps**:
1. Open cash drawer
2. Process multiple sales
3. Handle returns
4. Update inventory
5. Close cash drawer
6. Generate daily reports
**Expected Result**: 
- All operations successful
- Data integrity maintained
- Reports accurate
- System performance good

---

## 11. Performance & Stress Testing

### 11.1 Load Testing

#### Scenario 11.1.1: Concurrent Users
**Objective**: Verify system performance under load
**Steps**:
1. Simulate 10+ concurrent users
2. Perform typical operations
3. Monitor system response
**Expected Result**: 
- System responds within 3 seconds
- No data corruption
- Database performance maintained
- Error rates below 1%

### 11.2 Data Volume Testing

#### Scenario 11.2.1: Large Dataset Handling
**Objective**: Verify system with large data volumes
**Steps**:
1. Load 10,000+ products
2. 1,000+ customers
3. 1,000+ transactions
4. Generate reports
**Expected Result**: 
- System handles large datasets
- Reports generated in reasonable time
- Search functionality works
- Pagination works correctly

---

## 12. Security Testing

### 12.1 Authentication & Authorization

#### Scenario 12.1.1: Role-Based Access Control
**Objective**: Verify user permissions
**Steps**:
1. Test each user role
2. Verify access to different sections
3. Check permission restrictions
**Expected Result**: 
- Users can only access authorized sections
- Unauthorized access blocked
- Audit logs maintained
- Security policies enforced

#### Scenario 12.1.2: Session Management
**Objective**: Verify session security
**Steps**:
1. Test session timeout
2. Verify logout functionality
3. Check token expiration
**Expected Result**: 
- Sessions timeout correctly
- Logout works properly
- Tokens expire as expected
- Security maintained

---

## Test Execution Guidelines

### Test Environment
- Use dedicated test database
- Reset data between test runs
- Use test users with appropriate permissions
- Document any test data requirements

### Test Documentation
- Record test results
- Document any bugs found
- Track test coverage
- Maintain test data sets

### Regression Testing
- Run critical path tests after each deployment
- Verify core functionality
- Check data integrity
- Validate user workflows

### Performance Monitoring
- Monitor response times
- Check database performance
- Verify memory usage
- Track error rates

---

## Success Criteria

### Functional Requirements
- All features work as specified
- Data integrity maintained
- User workflows complete successfully
- Error handling works properly

### Non-Functional Requirements
- Response time < 3 seconds for most operations
- System available 99.9% of the time
- Data backup and recovery working
- Security requirements met

### User Experience
- Intuitive user interface
- Clear error messages
- Helpful validation feedback
- Consistent behavior across modules

---

## Bug Reporting Template

### Bug Report Format
```
Bug ID: [Auto-generated]
Title: [Brief description]
Severity: [Critical/High/Medium/Low]
Priority: [High/Medium/Low]
Module: [Affected module]
Steps to Reproduce:
1. [Step 1]
2. [Step 2]
3. [Step 3]

Expected Result: [What should happen]
Actual Result: [What actually happened]
Environment: [OS, Browser, etc.]
Screenshots: [If applicable]
Additional Notes: [Any other relevant information]
```

This comprehensive test scenario document covers all major aspects of the Dija Gold POS application and should be used as a guide for thorough testing of the system.

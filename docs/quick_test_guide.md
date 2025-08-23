# Dija Gold POS - Quick Test Guide

## 🚀 Quick Start Testing

This guide provides essential test scenarios to quickly validate the core functionality of the Dija Gold POS application.

---

## Prerequisites

### Environment Setup
1. **Backend API**: Running on `localhost:5000`
2. **Frontend App**: Running on `localhost:3000`
3. **Database**: SQL Server with test data
4. **Test Users**: At least one user per role (Cashier, Manager, Admin)

### Quick Data Setup
```sql
-- Ensure you have at least:
-- 1. 2-3 branches
-- 2. 5-10 products (different categories)
-- 3. 3-5 customers
-- 4. 2-3 suppliers
-- 5. Gold rates for different karats
```

---

## 🎯 Critical Test Paths (Must Test)

### 1. Authentication Flow
**Time**: 5 minutes
**Priority**: Critical

**Steps**:
1. Navigate to login page
2. Login with valid credentials
3. Verify dashboard loads
4. Test logout functionality

**Expected**: User can login/logout successfully

---

### 2. Basic Sales Transaction
**Time**: 10 minutes
**Priority**: Critical

**Steps**:
1. Login as Cashier
2. Navigate to Sales section
3. Create new sale:
   - Select customer
   - Add 1-2 products
   - Apply payment (cash/card)
   - Complete transaction
4. Verify receipt generation
5. Check inventory updated

**Expected**: Sale completes successfully, inventory reduces, receipt generated

---

### 3. Product Management
**Time**: 8 minutes
**Priority**: High

**Steps**:
1. Login as Manager
2. Navigate to Products section
3. Add new product with required fields
4. Search for existing products
5. Update product information
6. Verify changes saved

**Expected**: Product CRUD operations work correctly

---

### 4. Inventory Check
**Time**: 5 minutes
**Priority**: High

**Steps**:
1. Navigate to Inventory section
2. View inventory for specific product
3. Check low stock alerts
4. Verify inventory quantities

**Expected**: Inventory data displays correctly, alerts work

---

### 5. Basic Reporting
**Time**: 5 minutes
**Priority**: Medium

**Steps**:
1. Navigate to Reports section
2. Generate daily sales summary
3. Export report (if available)
4. Check data accuracy

**Expected**: Reports generate successfully with correct data

---

## 🔄 End-to-End Test Scenario

### Complete Business Day Simulation
**Time**: 20 minutes
**Priority**: Critical

**Scenario**: Simulate a typical business day

**Steps**:
1. **Morning Setup** (5 min)
   - Login as Cashier
   - Open cash drawer
   - Check inventory levels

2. **Sales Operations** (10 min)
   - Process 2-3 sales transactions
   - Use different payment methods
   - Handle one return/refund

3. **End of Day** (5 min)
   - Close cash drawer
   - Generate daily reports
   - Verify reconciliation

**Expected**: All operations complete successfully, data consistency maintained

---

## 🚨 Quick Smoke Tests

### Critical Functionality Checks
- [ ] **Login/Logout**: Works without errors
- [ ] **Navigation**: All main sections accessible
- [ ] **Database Connection**: No connection errors
- [ ] **Search Functionality**: Basic search works
- [ ] **Form Submission**: Forms save data correctly
- [ ] **Data Display**: Tables and lists load properly

### Performance Checks
- [ ] **Page Load Time**: < 3 seconds for main pages
- [ ] **Search Response**: < 2 seconds for simple searches
- [ ] **Form Submission**: < 5 seconds for data saves
- [ ] **Report Generation**: < 10 seconds for basic reports

---

## 🐛 Common Issues to Watch For

### Authentication Issues
- **Problem**: Login fails with valid credentials
- **Check**: Database connection, user account status
- **Solution**: Verify user exists and is active

### Data Not Saving
- **Problem**: Forms submit but data doesn't persist
- **Check**: Database permissions, validation errors
- **Solution**: Check browser console for errors

### Slow Performance
- **Problem**: Pages load slowly
- **Check**: Database queries, network connectivity
- **Solution**: Monitor database performance

### UI Issues
- **Problem**: Buttons not working, forms not responsive
- **Check**: JavaScript errors, CSS loading
- **Solution**: Clear browser cache, check console

---

## 📊 Test Data Requirements

### Minimum Test Data
```json
{
  "branches": 2,
  "products": 10,
  "customers": 5,
  "suppliers": 3,
  "users": 3,
  "goldRates": 4
}
```

### Sample Test Data
```sql
-- Sample Product
INSERT INTO Products (Name, ProductCode, CategoryTypeId, KaratTypeId, Weight, CostPrice, SellingPrice)
VALUES ('Test Gold Ring', 'RING001', 1, 1, 5.5, 500.00, 750.00);

-- Sample Customer
INSERT INTO Customers (Name, Phone, Email, IsActive)
VALUES ('Test Customer', '+1234567890', 'test@example.com', 1);

-- Sample User
INSERT INTO Users (UserName, Email, FullName, Role, IsActive)
VALUES ('testcashier', 'cashier@test.com', 'Test Cashier', 'Cashier', 1);
```

---

## 🎯 Focus Areas by Role

### Cashier Testing Focus
- [ ] Sales transactions
- [ ] Returns and refunds
- [ ] Cash drawer operations
- [ ] Customer search
- [ ] Receipt printing

### Manager Testing Focus
- [ ] Product management
- [ ] Inventory operations
- [ ] Purchase orders
- [ ] Reports generation
- [ ] User management

### Admin Testing Focus
- [ ] System configuration
- [ ] Branch management
- [ ] User administration
- [ ] Lookup table management
- [ ] System reports

---

## ⚡ Quick Validation Checklist

### Before Starting Tests
- [ ] Environment is running
- [ ] Test data is loaded
- [ ] Test users are available
- [ ] Browser is compatible
- [ ] Network is stable

### After Each Test Session
- [ ] All critical paths tested
- [ ] No critical errors found
- [ ] Performance is acceptable
- [ ] Data integrity maintained
- [ ] Issues documented

---

## 🆘 Troubleshooting

### Common Setup Issues

**Backend Not Starting**
```bash
# Check if port 5000 is available
netstat -an | findstr :5000

# Restart the API
dotnet run --project DijaGoldPOS.API
```

**Frontend Not Loading**
```bash
# Check if port 3000 is available
netstat -an | findstr :3000

# Restart the React app
npm start
```

**Database Connection Issues**
```bash
# Check connection string in appsettings.json
# Verify SQL Server is running
# Check firewall settings
```

### Quick Fixes

**Clear Browser Cache**
- Press `Ctrl+Shift+R` (hard refresh)
- Clear browser cache and cookies

**Reset Test Data**
```sql
-- Reset to known good state
-- Run database initialization scripts
```

**Check Logs**
- Backend logs: Check console output
- Frontend logs: Check browser console (F12)
- Database logs: Check SQL Server logs

---

## 📝 Test Reporting

### Quick Test Report Template
```
Test Session: [Date/Time]
Tester: [Name]
Environment: [Dev/Test/Staging]

Critical Paths Tested:
- [ ] Authentication
- [ ] Sales Transaction
- [ ] Product Management
- [ ] Inventory Check
- [ ] Basic Reporting

Issues Found:
- [ ] Critical: [Description]
- [ ] High: [Description]
- [ ] Medium: [Description]

Performance Notes:
- [ ] Response times acceptable
- [ ] Any performance issues

Overall Status:
- [ ] Ready for detailed testing
- [ ] Issues need resolution
- [ ] Not ready for testing
```

---

## 🎯 Success Criteria

### Minimum Viable Testing
- [ ] All critical paths pass
- [ ] No critical errors
- [ ] Performance is acceptable
- [ ] Data integrity maintained
- [ ] Core functionality works

### Ready for Production
- [ ] All test scenarios pass
- [ ] Performance criteria met
- [ ] Security requirements satisfied
- [ ] User acceptance criteria met
- [ ] Documentation complete

---

*Use this quick guide to rapidly validate the core functionality of the Dija Gold POS application before proceeding with comprehensive testing.*

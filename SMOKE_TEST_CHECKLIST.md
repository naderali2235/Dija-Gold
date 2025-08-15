# DijaGold POS - Smoke Test Checklist

## 🚀 Setup Instructions

### 1. Backend API Setup
```bash
cd DijaGoldPOS.API
dotnet restore
dotnet run
```
**Expected**: API should start on `https://localhost:7000` or `http://localhost:5000`

### 2. Frontend Setup
```bash
cd "DijaGold POS System"
npm install
npm start
```
**Expected**: React app should start on `http://localhost:3000`

### 3. Environment Configuration
Create `.env.local` in the frontend root:
```
REACT_APP_API_URL=https://localhost:7000/api
REACT_APP_MOCK_API=false
```

## ✅ **Test Scenarios**

### **Authentication Flow**
- [ ] 1. Login page appears on first load
- [ ] 2. Can login with valid credentials (test user from API)
- [ ] 3. Shows loading spinner during authentication
- [ ] 4. Redirects to dashboard after successful login
- [ ] 5. User info displays correctly in the UI

**Test Credentials**: Check API documentation or database for valid test users

### **Dashboard Integration**
- [ ] 6. Dashboard loads without errors
- [ ] 7. Gold rates section shows loading state initially
- [ ] 8. Gold rates load from API (or show cached data if API fails)
- [ ] 9. Business metrics cards display properly

### **Products Component (Step 2 - Just Completed)**
#### **Product Listing**
- [ ] 10. Products table shows loading skeletons initially
- [ ] 11. Products load from API and display in table
- [ ] 12. Search functionality works (type in search box)
- [ ] 13. Category and Karat filters work properly
- [ ] 14. Product pricing calculations display correctly

#### **Product Management (Manager Only)**
- [ ] 15. "Add Product" button visible for managers
- [ ] 16. Create product dialog opens properly
- [ ] 17. All form fields are present and functional
- [ ] 18. Can successfully create a new product
- [ ] 19. Product list refreshes after creation
- [ ] 20. Edit product functionality works
- [ ] 21. Delete product works with confirmation

#### **Product Management (Cashier)**
- [ ] 22. "Add Product" button hidden for cashiers
- [ ] 23. Edit/Delete options hidden in dropdown for cashiers

### **Sales Component (Step 1 - Previously Completed)**
#### **Product Selection**
- [ ] 24. Product search loads real products from API
- [ ] 25. Can add products to cart
- [ ] 26. Cart calculations work correctly (gold rate + making charges)
- [ ] 27. Discount functionality works

#### **Customer Management**
- [ ] 28. Customer search loads real customers from API
- [ ] 29. Can select customers for sales
- [ ] 30. Customer information displays correctly

#### **Transaction Processing**
- [ ] 31. Payment method selection works
- [ ] 32. Amount paid validation works
- [ ] 33. Can successfully process a sale transaction
- [ ] 34. Success message shows with transaction number
- [ ] 35. Cart clears after successful sale

## 🚨 **Common Issues & Solutions**

### **API Connection Issues**
- **CORS Errors**: Check API CORS configuration
- **SSL Certificate**: Try HTTP instead of HTTPS in .env.local
- **Port Conflicts**: Verify API is running on expected port

### **Authentication Issues**
- **401 Errors**: Check if test users exist in database
- **Token Issues**: Check browser Network tab for token in headers

### **Data Loading Issues**
- **Empty Lists**: Verify database has test data
- **API Errors**: Check browser Console and Network tabs
- **Loading States**: Should show skeletons, not blank screens

### **Form Issues**
- **Validation Errors**: Check required fields are filled
- **Save Failures**: Check API logs for validation errors
- **Field Mapping**: Verify form data matches API expectations

## 🎯 **Success Criteria**

### **Must Work** (Blocking Issues)
- ✅ User can login successfully
- ✅ Products load and display from API
- ✅ Can create new products (managers)
- ✅ Sales transaction processing works
- ✅ Customer selection works

### **Should Work** (Minor Issues)
- ✅ Search and filtering functions properly
- ✅ Loading states display correctly  
- ✅ Error handling shows appropriate messages
- ✅ Role-based permissions work

### **Nice to Have** (Enhancement Opportunities)
- ✅ Smooth animations and transitions
- ✅ Responsive design works on different screen sizes
- ✅ All form validations provide helpful feedback

## 📋 **Test Results**
Document any issues found:

**Authentication**: ✅ Pass / ❌ Fail - Notes: ___________

**Products Listing**: ✅ Pass / ❌ Fail - Notes: ___________

**Product Creation**: ✅ Pass / ❌ Fail - Notes: ___________

**Sales Processing**: ✅ Pass / ❌ Fail - Notes: ___________

**Customer Selection**: ✅ Pass / ❌ Fail - Notes: ___________

## 🔧 **Debug Tools**

### **Browser DevTools**
- **Console**: Check for JavaScript errors
- **Network**: Verify API calls and responses
- **Application**: Check localStorage for auth tokens

### **API Debugging**
- **Swagger UI**: Usually at `https://localhost:7000/swagger`
- **API Logs**: Check Visual Studio or console output
- **Database**: Verify data exists in tables

---

## Next Steps After Testing
1. ✅ If all critical tests pass → Continue to Step 3 (Inventory)
2. ⚠️ If minor issues → Document and continue
3. ❌ If blocking issues → Fix before continuing

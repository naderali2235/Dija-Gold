# DijaGold POS System - API Integration Guide

## Overview

This guide explains how to bind the DijaGold POS UI project to the backend API. The integration includes authentication, real-time data fetching, and complete CRUD operations.

## API Configuration

### Environment Variables

Create a `.env.local` file in the root of the UI project with the following variables:

```env
# API Configuration
REACT_APP_API_URL=https://localhost:5001/api

# Development Configuration
REACT_APP_MOCK_API=false

# Application Configuration
REACT_APP_NAME="DijaGold POS System"
REACT_APP_VERSION="2.0.1"
```

### Backend Requirements

Ensure the DijaGold API backend is running with:

1. **CORS Configuration**: The API must allow requests from the frontend domain
2. **JWT Authentication**: Properly configured JWT tokens
3. **HTTPS**: For production deployments
4. **Database**: SQL Server with proper schema and seed data

## Integration Features

### ✅ Completed Integrations

#### 1. Authentication System
- **Real API Login**: Replaces mock authentication with actual API calls
- **JWT Token Management**: Automatic token storage and refresh
- **Role-Based Access**: Manager/Cashier permissions from API
- **Session Validation**: Validates stored tokens on app startup

#### 2. Gold Rates Integration
- **Live Data**: Fetches current gold rates from `/api/pricing/gold-rates`
- **Loading States**: Shows skeleton while loading
- **Error Handling**: Fallback to cached data if API fails
- **Auto-Refresh**: Periodic updates of gold rates

#### 3. API Service Layer
- **Centralized API Client**: Single service for all API communication
- **Error Handling**: Consistent error handling across all endpoints
- **Type Safety**: Full TypeScript interfaces for API responses
- **Request Interceptors**: Automatic JWT token inclusion

#### 4. React Hooks Integration
- **useApiCall**: Generic hook for API operations with loading/error states
- **useGoldRates**: Specialized hook for gold rates management
- **usePaginatedApi**: Hook for paginated data fetching
- **useCurrentUser**: User authentication state management

### 🔄 Endpoints Integrated

| Component | Endpoint | Status | Description |
|-----------|----------|--------|-------------|
| Authentication | `/api/auth/login` | ✅ Complete | User login with JWT |
| Authentication | `/api/auth/logout` | ✅ Complete | User logout |
| Authentication | `/api/auth/me` | ✅ Complete | Get current user info |
| Dashboard | `/api/pricing/gold-rates` | ✅ Complete | Live gold rates display |
| Products | `/api/products` | 🟡 Ready | Product CRUD operations |
| Transactions | `/api/transactions/sale` | 🟡 Ready | Sales processing |
| Inventory | `/api/inventory` | 🟡 Ready | Stock management |
| Reports | `/api/reports/*` | 🟡 Ready | Business analytics |

### 📋 Next Steps for Full Integration

#### 1. Products Component
```typescript
// Update Products.tsx to use:
import { useProducts, useCreateProduct } from '../hooks/useApi';

const { data: products, loading, execute: fetchProducts } = useProducts();
```

#### 2. Sales Component
```typescript
// Update Sales.tsx to use:
import { useProcessSale, useProducts } from '../hooks/useApi';

const { execute: processSale } = useProcessSale();
```

#### 3. Inventory Component
```typescript
// Update Inventory.tsx to use:
import { useApiCall } from '../hooks/useApi';
import api from '../services/api';

// Add inventory API endpoints to services/api.ts
```

## API Response Format

All API responses follow this consistent format:

```typescript
{
  success: boolean;
  message: string;
  data?: any;
  errors?: any;
}
```

## Error Handling

The integration includes comprehensive error handling:

1. **Network Errors**: Automatic retry with exponential backoff
2. **Authentication Errors**: Auto-redirect to login on 401
3. **Validation Errors**: Display field-specific error messages
4. **Server Errors**: User-friendly error messages with fallbacks

## Security Features

- **JWT Token Management**: Secure token storage and automatic refresh
- **HTTPS Only**: Enforces secure connections
- **CORS Protection**: Proper cross-origin request handling
- **XSS Prevention**: Sanitized data handling

## Development Setup

1. **Start Backend API**:
   ```bash
   cd DijaGoldPOS.API
   dotnet run
   ```

2. **Start Frontend**:
   ```bash
   cd "DijaGold POS System"
   npm start
   ```

3. **Test Integration**:
   - Login with default credentials
   - Verify gold rates are loading from API
   - Check browser network tab for API calls

## Default API Credentials

For testing the integration, use these default accounts:

**Manager Account:**
- Username: `manager@dijagold.com`
- Password: `Manager123!`

**Cashier Account:**
- Username: `cashier@dijagold.com`
- Password: `Cashier123!`

## Troubleshooting

### Common Issues

1. **CORS Errors**: Verify `appsettings.json` includes frontend URL in CORS settings
2. **Token Expiration**: Tokens expire after 8 hours by default
3. **Network Errors**: Check if API backend is running on correct port
4. **SSL Certificates**: Use `TrustServerCertificate=true` for local development

### API Health Check

Visit `https://localhost:65149/swagger` to verify API is running and explore endpoints.

## Production Deployment

For production deployment:

1. Update `REACT_APP_API_URL` to production API URL
2. Ensure HTTPS certificates are properly configured
3. Set appropriate CORS origins (no wildcards)
4. Configure proper JWT secret keys
5. Set up database connection strings


REACT_APP_API_URL=https://localhost:65149/api
REACT_APP_MOCK_API=false
---

The UI is now successfully bound to the API backend with real authentication and live data integration!

/**
 * API Service for DijaGold POS System
 * Handles all communication with the backend API
 */

import { API_CONFIG, STORAGE_KEYS } from '../config/environment';
import { EnumLookupDto, ApiLookupsResponse } from '../types/enums';

// Base API configuration
const API_BASE_URL = API_CONFIG.BASE_URL;

// Auth token management
let authToken: string | null = null;

export const setAuthToken = (token: string | null) => {
  authToken = token;
  if (token) {
    localStorage.setItem(STORAGE_KEYS.AUTH_TOKEN, token);
  } else {
    localStorage.removeItem(STORAGE_KEYS.AUTH_TOKEN);
  }
};

export const getAuthToken = (): string | null => {
  if (!authToken) {
    authToken = localStorage.getItem(STORAGE_KEYS.AUTH_TOKEN);
  }
  return authToken;
};

// API Response interfaces
export interface ApiResponse<T = any> {
  success: boolean;
  message: string;
  data?: T;
  errors?: any;
}

// Test API connectivity
export const testApiConnection = async (): Promise<boolean> => {
  try {
    const response = await fetch(`${API_BASE_URL}/health`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });
    console.log('API health check response:', response.status, response.statusText);
    return response.ok;
  } catch (error) {
    console.error('API connection test failed:', error);
    return false;
  }
};

export interface User {
  id: string;
  username: string;
  fullName: string;
  email: string;
  employeeCode?: string;
  roles: string[];
  branch?: {
    id: number;
    name: string;
    code: string;
    isHeadquarters: boolean;
  };
  lastLoginAt?: string;
}

export interface LoginRequest {
  username: string;
  password: string;
  rememberMe?: boolean;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  user: User;
}

// Generic API client function
async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<ApiResponse<T>> {
  const url = `${API_BASE_URL}${endpoint}`;
  const token = getAuthToken();

  const defaultHeaders: HeadersInit = {
    'Content-Type': 'application/json',
  };

  if (token) {
    defaultHeaders.Authorization = `Bearer ${token}`;
  }

  const config: RequestInit = {
    ...options,
    headers: {
      ...defaultHeaders,
      ...options.headers,
    },
  };

  try {
    const response = await fetch(url, config);
    const data = await response.json();

    if (!response.ok) {
      // Handle specific HTTP errors
      if (response.status === 401) {
        // Unauthorized - clear token and redirect to login
        setAuthToken(null);
        window.location.href = '/login';
        throw new Error('Session expired. Please login again.');
      }
      
      throw new Error(data.message || `HTTP ${response.status}: ${response.statusText}`);
    }

    return data;
  } catch (error) {
    console.error(`API Error (${endpoint}):`, error);
    throw error;
  }
}

// Authentication API
export const authApi = {
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response = await apiRequest<LoginResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify(credentials),
    });
    
    if (response.success && response.data) {
      setAuthToken(response.data.token);
      return response.data;
    }
    
    throw new Error(response.message || 'Login failed');
  },

  async logout(): Promise<void> {
    try {
      await apiRequest('/auth/logout', {
        method: 'POST',
      });
    } finally {
      setAuthToken(null);
    }
  },

  async getCurrentUser(): Promise<User> {
    const response = await apiRequest<User>('/auth/me');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to get user info');
  },

  async changePassword(data: {
    currentPassword: string;
    newPassword: string;
    confirmPassword: string;
  }): Promise<void> {
    const response = await apiRequest('/auth/change-password', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to change password');
    }
  },

  async refreshToken(): Promise<LoginResponse> {
    const response = await apiRequest<LoginResponse>('/auth/refresh-token', {
      method: 'POST',
    });
    
    if (response.success && response.data) {
      setAuthToken(response.data.token);
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to refresh token');
  }
};

// Products API
export interface Product {
  id: number;
  productCode: string;
  name: string;
  categoryType: 'GoldJewelry' | 'Bullion' | 'Coins';
  karatType: '18K' | '21K' | '22K' | '24K';
  weight: number;
  brand?: string;
  designStyle?: string;
  subCategory?: string;
  shape?: string;
  purityCertificateNumber?: string;
  countryOfOrigin?: string;
  yearOfMinting?: number;
  faceValue?: number;
  hasNumismaticValue?: boolean;
  makingChargesApplicable: boolean;
  supplierId?: number;
  supplierName?: string;
  createdAt: string;
  isActive: boolean;
}

export interface ProductSearchRequest {
  searchTerm?: string;
  categoryType?: string;
  karatType?: string;
  brand?: string;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export const productsApi = {
  async getProducts(params: ProductSearchRequest = {}): Promise<{
    items: Product[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
  }> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<{
      items: Product[];
      totalCount: number;
      pageNumber: number;
      pageSize: number;
    }>(`/products?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch products');
  },

  async getProduct(id: number): Promise<Product> {
    const response = await apiRequest<Product>(`/products/${id}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch product');
  },

  async createProduct(product: Omit<Product, 'id' | 'createdAt'>): Promise<Product> {
    const response = await apiRequest<Product>('/products', {
      method: 'POST',
      body: JSON.stringify(product),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to create product');
  },

  async updateProduct(id: number, product: Partial<Product>): Promise<Product> {
    const response = await apiRequest<Product>(`/products/${id}`, {
      method: 'PUT',
      body: JSON.stringify({ ...product, id }),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to update product');
  },

  async deleteProduct(id: number): Promise<void> {
    const response = await apiRequest(`/products/${id}`, {
      method: 'DELETE',
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to delete product');
    }
  }
};

// Pricing API
export interface GoldRate {
  id: number;
  karat: string;
  buyRate: number;
  sellRate: number;
  effectiveFrom: string;
  isActive: boolean;
}

export const pricingApi = {
  async getGoldRates(): Promise<GoldRate[]> {
    const response = await apiRequest<GoldRate[]>('/pricing/gold-rates');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch gold rates');
  },

  async updateGoldRates(rates: Array<{
    karat: string;
    buyRate: number;
    sellRate: number;
  }>): Promise<void> {
    const response = await apiRequest('/pricing/gold-rates', {
      method: 'POST',
      body: JSON.stringify(rates),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to update gold rates');
    }
  }
};

// User Management API interfaces
export interface UserDto {
  id: string;
  userName: string;
  fullName: string;
  email: string;
  employeeCode?: string;
  roles: string[];
  branchId?: number;
  branchName?: string;
  isActive: boolean;
  createdAt: string;
  lastLoginAt?: string;
  emailConfirmed: boolean;
  lockoutEnabled: boolean;
  lockoutEnd?: string;
}

export interface CreateUserRequest {
  userName: string;
  fullName: string;
  email: string;
  employeeCode?: string;
  password: string;
  roles: string[];
  branchId?: number;
  isActive: boolean;
}

export interface UpdateUserRequest {
  id: string;
  fullName: string;
  email: string;
  employeeCode?: string;
  branchId?: number;
}

export interface UpdateUserRoleRequest {
  userId: string;
  roles: string[];
}

export interface UpdateUserStatusRequest {
  userId: string;
  isActive: boolean;
  reason?: string;
}

export interface UserSearchRequest {
  searchTerm?: string;
  role?: string;
  branchId?: number;
  isActive?: boolean;
  isLocked?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface UserActivityDto {
  userId: string;
  userName: string;
  fullName: string;
  activities: UserActivityLogDto[];
  totalActivities: number;
}

export interface UserActivityLogDto {
  id: number;
  timestamp: string;
  action: string;
  entityType: string;
  entityId: string;
  details: string;
  branchName?: string;
  ipAddress: string;
}

export interface ResetPasswordRequest {
  userId: string;
  newPassword: string;
  forcePasswordChange: boolean;
}

export interface UserPermissionsDto {
  userId: string;
  userName: string;
  fullName: string;
  roles: string[];
  permissions: string[];
  featureAccess: { [key: string]: boolean };
}

export interface UpdateUserPermissionsRequest {
  userId: string;
  permissions: string[];
  featureAccess: { [key: string]: boolean };
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Users API
export const usersApi = {
  async getUsers(params: UserSearchRequest = {}): Promise<PagedResult<UserDto>> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<PagedResult<UserDto>>(`/users?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch users');
  },

  async getUser(id: string): Promise<UserDto> {
    const response = await apiRequest<UserDto>(`/users/${id}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch user');
  },

  async createUser(user: CreateUserRequest): Promise<UserDto> {
    const response = await apiRequest<UserDto>('/users', {
      method: 'POST',
      body: JSON.stringify(user),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to create user');
  },

  async updateUser(id: string, user: UpdateUserRequest): Promise<void> {
    const response = await apiRequest(`/users/${id}`, {
      method: 'PUT',
      body: JSON.stringify({ ...user, id }),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to update user');
    }
  },

  async updateUserRoles(id: string, request: UpdateUserRoleRequest): Promise<void> {
    const response = await apiRequest(`/users/${id}/roles`, {
      method: 'PUT',
      body: JSON.stringify({ ...request, userId: id }),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to update user roles');
    }
  },

  async updateUserStatus(id: string, request: UpdateUserStatusRequest): Promise<void> {
    const response = await apiRequest(`/users/${id}/status`, {
      method: 'PUT',
      body: JSON.stringify({ ...request, userId: id }),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to update user status');
    }
  },

  async getUserActivity(id: string, params: {
    fromDate?: string;
    toDate?: string;
    actionType?: string;
    entityType?: string;
    pageNumber?: number;
    pageSize?: number;
  } = {}): Promise<UserActivityDto> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<UserActivityDto>(`/users/${id}/activity?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch user activity');
  },

  async resetPassword(id: string, request: ResetPasswordRequest): Promise<void> {
    const response = await apiRequest(`/users/${id}/reset-password`, {
      method: 'POST',
      body: JSON.stringify({ ...request, userId: id }),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to reset password');
    }
  },

  async getUserPermissions(id: string): Promise<UserPermissionsDto> {
    const response = await apiRequest<UserPermissionsDto>(`/users/${id}/permissions`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch user permissions');
  },

  async updateUserPermissions(id: string, request: UpdateUserPermissionsRequest): Promise<void> {
    const response = await apiRequest(`/users/${id}/permissions`, {
      method: 'PUT',
      body: JSON.stringify({ ...request, userId: id }),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to update user permissions');
    }
  }
};

// Branches API
export const branchesApi = {
  async getBranches(params: BranchSearchRequest = {}): Promise<PagedResult<BranchDto>> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<PagedResult<BranchDto>>(`/branches?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch branches');
  },

  async getBranch(id: number): Promise<BranchDto> {
    const response = await apiRequest<BranchDto>(`/branches/${id}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch branch');
  },

  async createBranch(branch: CreateBranchRequest): Promise<BranchDto> {
    const response = await apiRequest<BranchDto>('/branches', {
      method: 'POST',
      body: JSON.stringify(branch),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to create branch');
  },

  async updateBranch(id: number, branch: UpdateBranchRequest): Promise<BranchDto> {
    const response = await apiRequest<BranchDto>(`/branches/${id}`, {
      method: 'PUT',
      body: JSON.stringify({ ...branch, id }),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to update branch');
  },

  async getBranchInventory(id: number): Promise<BranchInventorySummaryDto> {
    const response = await apiRequest<BranchInventorySummaryDto>(`/branches/${id}/inventory`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch branch inventory');
  },

  async getBranchStaff(id: number): Promise<BranchStaffDto> {
    const response = await apiRequest<BranchStaffDto>(`/branches/${id}/staff`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch branch staff');
  },

  async getBranchPerformance(id: number, params: {
    date?: string;
  } = {}): Promise<BranchPerformanceDto> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<BranchPerformanceDto>(`/branches/${id}/performance?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch branch performance');
  },

  async deleteBranch(id: number): Promise<void> {
    const response = await apiRequest(`/branches/${id}`, {
      method: 'DELETE',
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to delete branch');
    }
  },

  async getBranchTransactions(id: number, params: {
    fromDate?: string;
    toDate?: string;
    transactionType?: string;
    pageNumber?: number;
    pageSize?: number;
  } = {}): Promise<PagedResult<BranchTransactionDto>> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<PagedResult<BranchTransactionDto>>(`/branches/${id}/transactions?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch branch transactions');
  }
};

// Suppliers API
export const suppliersApi = {
  async getSuppliers(params: SupplierSearchRequest = {}): Promise<PagedResult<SupplierDto>> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<PagedResult<SupplierDto>>(`/suppliers?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch suppliers');
  },

  async getSupplier(id: number): Promise<SupplierDto> {
    const response = await apiRequest<SupplierDto>(`/suppliers/${id}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch supplier');
  },

  async createSupplier(supplier: CreateSupplierRequest): Promise<SupplierDto> {
    const response = await apiRequest<SupplierDto>('/suppliers', {
      method: 'POST',
      body: JSON.stringify(supplier),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to create supplier');
  },

  async updateSupplier(id: number, supplier: UpdateSupplierRequest): Promise<void> {
    const response = await apiRequest(`/suppliers/${id}`, {
      method: 'PUT',
      body: JSON.stringify({ ...supplier, id }),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to update supplier');
    }
  },

  async deleteSupplier(id: number): Promise<void> {
    const response = await apiRequest(`/suppliers/${id}`, {
      method: 'DELETE',
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to delete supplier');
    }
  },

  async getSupplierProducts(id: number): Promise<SupplierProductsDto> {
    const response = await apiRequest<SupplierProductsDto>(`/suppliers/${id}/products`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch supplier products');
  },

  async getSupplierBalance(id: number): Promise<SupplierBalanceDto> {
    const response = await apiRequest<SupplierBalanceDto>(`/suppliers/${id}/balance`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch supplier balance');
  },

  async updateSupplierBalance(id: number, request: UpdateSupplierBalanceRequest): Promise<void> {
    const response = await apiRequest(`/suppliers/${id}/balance`, {
      method: 'PUT',
      body: JSON.stringify({ ...request, supplierId: id }),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to update supplier balance');
    }
  },

  async getSupplierTransactions(id: number, params: {
    fromDate?: string;
    toDate?: string;
    transactionType?: string;
    pageNumber?: number;
    pageSize?: number;
  } = {}): Promise<SupplierTransactionDto[]> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<SupplierTransactionDto[]>(`/suppliers/${id}/transactions?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch supplier transactions');
  }
};

// Inventory API
export const inventoryApi = {
  async getInventory(productId: number, branchId: number): Promise<InventoryDto> {
    const response = await apiRequest<InventoryDto>(`/inventory/product/${productId}/branch/${branchId}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch inventory');
  },

  async getBranchInventory(branchId: number, includeZeroStock: boolean = false): Promise<InventoryDto[]> {
    const queryString = new URLSearchParams({
      includeZeroStock: includeZeroStock.toString()
    }).toString();
    
    const response = await apiRequest<InventoryDto[]>(`/inventory/branch/${branchId}?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch branch inventory');
  },

  async getLowStockItems(branchId: number): Promise<InventoryDto[]> {
    const response = await apiRequest<InventoryDto[]>(`/inventory/branch/${branchId}/low-stock`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch low stock items');
  },

  async checkStockAvailability(productId: number, branchId: number, requestedQuantity: number): Promise<StockAvailabilityDto> {
    const queryString = new URLSearchParams({
      productId: productId.toString(),
      branchId: branchId.toString(),
      requestedQuantity: requestedQuantity.toString()
    }).toString();
    
    const response = await apiRequest<StockAvailabilityDto>(`/inventory/check-availability?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to check stock availability');
  },

  async addInventory(request: AddInventoryRequest): Promise<void> {
    const response = await apiRequest('/inventory/add', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to add inventory');
    }
  },

  async adjustInventory(request: AdjustInventoryRequest): Promise<void> {
    const response = await apiRequest('/inventory/adjust', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to adjust inventory');
    }
  },

  async transferInventory(request: TransferInventoryRequest): Promise<void> {
    const response = await apiRequest('/inventory/transfer', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to transfer inventory');
    }
  },

  async getInventoryMovements(params: InventoryMovementSearchRequest = {}): Promise<PagedResult<InventoryMovementDto>> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<PagedResult<InventoryMovementDto>>(`/inventory/movements?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch inventory movements');
  }
};

// Reports API
export const reportsApi = {
  async getDailySalesSummary(branchId: number, date: string): Promise<DailySalesSummaryReport> {
    const queryString = new URLSearchParams({
      branchId: branchId.toString(),
      date: date
    }).toString();
    
    const response = await apiRequest<DailySalesSummaryReport>(`/reports/daily-sales-summary?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch daily sales summary');
  },

  async getCashReconciliation(branchId: number, date: string): Promise<CashReconciliationReport> {
    const queryString = new URLSearchParams({
      branchId: branchId.toString(),
      date: date
    }).toString();
    
    const response = await apiRequest<CashReconciliationReport>(`/reports/cash-reconciliation?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch cash reconciliation report');
  },

  async getInventoryMovementReport(branchId: number, fromDate: string, toDate: string): Promise<InventoryMovementReport> {
    const queryString = new URLSearchParams({
      branchId: branchId.toString(),
      fromDate: fromDate,
      toDate: toDate
    }).toString();
    
    const response = await apiRequest<InventoryMovementReport>(`/reports/inventory-movement?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch inventory movement report');
  },

  async getProfitAnalysisReport(fromDate: string, toDate: string, branchId?: number, categoryType?: string): Promise<ProfitAnalysisReport> {
    const params: Record<string, string> = {
      fromDate: fromDate,
      toDate: toDate
    };
    
    if (branchId) {
      params.branchId = branchId.toString();
    }
    
    if (categoryType) {
      params.categoryType = categoryType;
    }
    
    const queryString = new URLSearchParams(params).toString();
    
    const response = await apiRequest<ProfitAnalysisReport>(`/reports/profit-analysis?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch profit analysis report');
  },

  async getCustomerAnalysisReport(fromDate: string, toDate: string, branchId?: number, topCustomersCount: number = 20): Promise<CustomerAnalysisReport> {
    const params: Record<string, string> = {
      fromDate: fromDate,
      toDate: toDate,
      topCustomersCount: topCustomersCount.toString()
    };
    
    if (branchId) {
      params.branchId = branchId.toString();
    }
    
    const queryString = new URLSearchParams(params).toString();
    
    const response = await apiRequest<CustomerAnalysisReport>(`/reports/customer-analysis?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch customer analysis report');
  },

  async getSupplierBalanceReport(asOfDate?: string): Promise<SupplierBalanceReport> {
    const queryString = asOfDate 
      ? new URLSearchParams({ asOfDate }).toString()
      : '';
    
    const response = await apiRequest<SupplierBalanceReport>(`/reports/supplier-balance${queryString ? `?${queryString}` : ''}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch supplier balance report');
  },

  async getInventoryValuationReport(branchId?: number, asOfDate?: string): Promise<InventoryValuationReport> {
    const params: Record<string, string> = {};
    
    if (branchId) {
      params.branchId = branchId.toString();
    }
    
    if (asOfDate) {
      params.asOfDate = asOfDate;
    }
    
    const queryString = Object.keys(params).length > 0 
      ? new URLSearchParams(params).toString()
      : '';
    
    const response = await apiRequest<InventoryValuationReport>(`/reports/inventory-valuation${queryString ? `?${queryString}` : ''}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch inventory valuation report');
  },

  async getTaxReport(fromDate: string, toDate: string, branchId?: number): Promise<TaxReport> {
    const params: Record<string, string> = {
      fromDate: fromDate,
      toDate: toDate
    };
    
    if (branchId) {
      params.branchId = branchId.toString();
    }
    
    const queryString = new URLSearchParams(params).toString();
    
    const response = await apiRequest<TaxReport>(`/reports/tax-report?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch tax report');
  },

  async getTransactionLogReport(branchId: number, date: string): Promise<TransactionLogReport> {
    const queryString = new URLSearchParams({
      branchId: branchId.toString(),
      date: date
    }).toString();
    
    const response = await apiRequest<TransactionLogReport>(`/reports/transaction-log?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch transaction log report');
  },

  async getReportTypes(): Promise<ReportTypeDto[]> {
    const response = await apiRequest<ReportTypeDto[]>('/reports/types');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch report types');
  },

  async exportToExcel(request: ExportReportRequest): Promise<Blob> {
    const response = await fetch(`${API_BASE_URL}/reports/export-excel`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getAuthToken()}`
      },
      body: JSON.stringify(request)
    });

    if (!response.ok) {
      throw new Error(`Failed to export to Excel: ${response.statusText}`);
    }

    return await response.blob();
  },

  async exportToPdf(request: ExportReportRequest): Promise<Blob> {
    const response = await fetch(`${API_BASE_URL}/reports/export-pdf`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getAuthToken()}`
      },
      body: JSON.stringify(request)
    });

    if (!response.ok) {
      throw new Error(`Failed to export to PDF: ${response.statusText}`);
    }

    return await response.blob();
  }
};

// Purchase Orders API
export const purchaseOrdersApi = {
  async createPurchaseOrder(request: CreatePurchaseOrderRequest): Promise<PurchaseOrderDto> {
    const response = await apiRequest<PurchaseOrderDto>('/purchaseorders', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to create purchase order');
  },

  async getPurchaseOrder(id: number): Promise<PurchaseOrderDto> {
    const response = await apiRequest<PurchaseOrderDto>(`/purchaseorders/${id}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch purchase order');
  },

  async searchPurchaseOrders(params: PurchaseOrderSearchRequest = {}): Promise<{items: PurchaseOrderDto[], total: number}> {
    const response = await apiRequest<PurchaseOrderDto[]>('/purchaseorders/search', {
      method: 'POST',
      body: JSON.stringify(params),
    });
    
    if (response.success && response.data) {
      return {
        items: response.data,
        total: response.data.length // Use array length as fallback for total count
      };
    }
    
    throw new Error(response.message || 'Failed to search purchase orders');
  },

  async receivePurchaseOrder(request: ReceivePurchaseOrderRequest): Promise<void> {
    const response = await apiRequest('/purchaseorders/receive', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to receive purchase order');
    }
  }
};

// Labels API
export const labelsApi = {
  async generateProductLabelZpl(productId: number, copies: number = 1): Promise<string> {
    const queryString = new URLSearchParams({
      copies: copies.toString()
    }).toString();
    
    const response = await apiRequest<string>(`/labels/${productId}/zpl?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to generate label ZPL');
  },

  async printProductLabel(productId: number, copies: number = 1): Promise<void> {
    const queryString = new URLSearchParams({
      copies: copies.toString()
    }).toString();
    
    const response = await apiRequest(`/labels/${productId}/print?${queryString}`, {
      method: 'POST',
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to print label');
    }
  },

  async decodeQrPayload(payload: string): Promise<Product> {
    const request: DecodeQrRequest = {
      payload: payload
    };
    
    const response = await apiRequest<Product>('/labels/decode-qr', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to decode QR payload');
  }
};

// Branch Management API interfaces
export interface BranchDto {
  id: number;
  name: string;
  code: string;
  address?: string;
  phone?: string;
  managerName?: string;
  isHeadquarters: boolean;
  createdAt: string;
  isActive: boolean;
}

export interface CreateBranchRequest {
  name: string;
  code: string;
  address?: string;
  phone?: string;
  managerName?: string;
  isHeadquarters: boolean;
}

export interface UpdateBranchRequest {
  id: number;
  name: string;
  code: string;
  address?: string;
  phone?: string;
  managerName?: string;
  isHeadquarters: boolean;
}

export interface BranchSearchRequest {
  searchTerm?: string;
  code?: string;
  isHeadquarters?: boolean;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface BranchInventorySummaryDto {
  branchId: number;
  branchName: string;
  branchCode: string;
  totalProducts: number;
  totalWeight: number;
  totalValue: number;
  lowStockItems: number;
  outOfStockItems: number;
  topItems: BranchInventoryItemDto[];
}

export interface BranchInventoryItemDto {
  productId: number;
  productCode: string;
  productName: string;
  quantityOnHand: number;
  weightOnHand: number;
  estimatedValue: number;
  isLowStock: boolean;
  isOutOfStock: boolean;
}

export interface BranchStaffDto {
  branchId: number;
  branchName: string;
  staff: BranchStaffMemberDto[];
  totalStaffCount: number;
}

export interface BranchStaffMemberDto {
  userId: string;
  userName: string;
  fullName: string;
  email: string;
  role: string;
  assignedDate: string;
  isActive: boolean;
}

export interface BranchPerformanceDto {
  branchId: number;
  branchName: string;
  branchCode: string;
  reportDate: string;
  dailySales: number;
  dailyTransactions: number;
  monthlySales: number;
  monthlyTransactions: number;
  averageTransactionValue: number;
  activeCustomers: number;
  inventoryTurnover: number;
  recentTransactions: BranchTransactionDto[];
}

export interface BranchTransactionDto {
  transactionId: number;
  transactionNumber: string;
  transactionDate: string;
  transactionType: string;
  totalAmount: number;
  customerName: string;
  cashierName: string;
}

// Supplier Management API interfaces
export interface SupplierDto {
  id: number;
  companyName: string;
  contactPersonName?: string;
  phone?: string;
  email?: string;
  address?: string;
  taxRegistrationNumber?: string;
  commercialRegistrationNumber?: string;
  creditLimit: number;
  currentBalance: number;
  paymentTermsDays: number;
  creditLimitEnforced: boolean;
  paymentTerms?: string;
  notes?: string;
  lastTransactionDate?: string;
  createdAt: string;
  isActive: boolean;
}

export interface CreateSupplierRequest {
  companyName: string;
  contactPersonName?: string;
  phone?: string;
  email?: string;
  address?: string;
  taxRegistrationNumber?: string;
  commercialRegistrationNumber?: string;
  creditLimit: number;
  paymentTermsDays: number;
  creditLimitEnforced: boolean;
  paymentTerms?: string;
  notes?: string;
}

export interface UpdateSupplierRequest {
  id: number;
  companyName: string;
  contactPersonName?: string;
  phone?: string;
  email?: string;
  address?: string;
  taxRegistrationNumber?: string;
  commercialRegistrationNumber?: string;
  creditLimit: number;
  paymentTermsDays: number;
  creditLimitEnforced: boolean;
  paymentTerms?: string;
  notes?: string;
}

export interface SupplierSearchRequest {
  searchTerm?: string;
  taxRegistrationNumber?: string;
  commercialRegistrationNumber?: string;
  creditLimitEnforced?: boolean;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface SupplierBalanceDto {
  supplierId: number;
  supplierName: string;
  creditLimit: number;
  currentBalance: number;
  availableCredit: number;
  paymentTermsDays: number;
  creditLimitEnforced: boolean;
  lastTransactionDate?: string;
  recentTransactions: SupplierTransactionDto[];
}

export interface SupplierTransactionDto {
  transactionId: number;
  transactionNumber: string;
  transactionDate: string;
  transactionType: string;
  amount: number;
  balanceAfterTransaction: number;
  notes?: string;
}

export interface UpdateSupplierBalanceRequest {
  supplierId: number;
  amount: number;
  transactionType: string; // "payment", "adjustment", "credit"
  notes?: string;
}

export interface SupplierProductsDto {
  supplierId: number;
  supplierName: string;
  products: SupplierProductDto[];
  totalProductCount: number;
}

export interface SupplierProductDto {
  productId: number;
  productCode: string;
  productName: string;
  categoryType: string;
  weight: number;
  createdAt: string;
  isActive: boolean;
}

// Inventory Management API interfaces
export interface InventoryDto {
  id: number;
  productId: number;
  productName: string;
  productCode: string;
  branchId: number;
  branchName: string;
  quantityOnHand: number;
  weightOnHand: number;
  minimumStockLevel: number;
  maximumStockLevel: number;
  reorderPoint: number;
  lastCountDate: string;
  isLowStock: boolean;
}

export interface StockAvailabilityDto {
  productId: number;
  branchId: number;
  requestedQuantity: number;
  availableQuantity: number;
  isAvailable: boolean;
  shortfallQuantity: number;
}

export interface AddInventoryRequest {
  productId: number;
  branchId: number;
  quantity: number;
  weight: number;
  movementType: string; // "Purchase", "Return", "Adjustment", etc.
  referenceNumber?: string;
  unitCost?: number;
  notes?: string;
}

export interface AdjustInventoryRequest {
  productId: number;
  branchId: number;
  newQuantity: number;
  newWeight: number;
  reason: string;
}

export interface TransferInventoryRequest {
  productId: number;
  fromBranchId: number;
  toBranchId: number;
  quantity: number;
  weight: number;
  transferNumber: string;
  notes?: string;
}

export interface InventoryMovementSearchRequest {
  productId?: number;
  branchId?: number;
  fromDate?: string;
  toDate?: string;
  movementType?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface InventoryMovementDto {
  id: number;
  productId: number;
  productName: string;
  productCode: string;
  branchId: number;
  branchName: string;
  movementType: string;
  referenceNumber?: string;
  quantityChange: number;
  weightChange: number;
  quantityBalance: number;
  weightBalance: number;
  unitCost?: number;
  notes?: string;
  createdAt: string;
  createdBy: string;
}

// Reports Management API interfaces
export interface ReportTypeDto {
  code: string;
  name: string;
  requiresManagerRole: boolean;
}

export interface ExportReportRequest {
  reportType: string;
  reportName: string;
  reportDataJson: string;
}

export interface DailySalesSummaryReport {
  branchId: number;
  branchName: string;
  reportDate: string;
  totalSales: number;
  totalReturns: number;
  netSales: number;
  transactionCount: number;
  categoryBreakdown: CategorySalesBreakdown[];
  paymentMethodBreakdown: PaymentMethodBreakdown[];
}

export interface CategorySalesBreakdown {
  category: string; // ProductCategoryType
  totalSales: number;
  totalWeight: number;
  transactionCount: number;
}

export interface PaymentMethodBreakdown {
  paymentMethod: string; // PaymentMethod enum
  amount: number;
  transactionCount: number;
}

export interface CashReconciliationReport {
  branchId: number;
  branchName: string;
  reportDate: string;
  openingBalance: number;
  cashSales: number;
  cashReturns: number;
  expectedClosingBalance: number;
  actualClosingBalance: number;
  cashOverShort: number;
}

export interface InventoryMovementReport {
  branchId: number;
  branchName: string;
  fromDate: string;
  toDate: string;
  movements: InventoryMovementSummary[];
}

export interface InventoryMovementSummary {
  productId: number;
  productName: string;
  category: string; // ProductCategoryType
  openingQuantity: number;
  openingWeight: number;
  purchases: number;
  sales: number;
  returns: number;
  adjustments: number;
  transfers: number;
  closingQuantity: number;
  closingWeight: number;
}

export interface ProfitAnalysisReport {
  branchId?: number;
  branchName?: string;
  fromDate: string;
  toDate: string;
  totalRevenue: number;
  totalCostOfGoodsSold: number;
  grossProfit: number;
  grossProfitMargin: number;
  productAnalysis: ProductProfitAnalysis[];
}

export interface ProductProfitAnalysis {
  productId: number;
  productName: string;
  category: string; // ProductCategoryType
  revenue: number;
  costOfGoodsSold: number;
  grossProfit: number;
  grossProfitMargin: number;
  quantitySold: number;
}

export interface CustomerAnalysisReport {
  branchId?: number;
  branchName?: string;
  fromDate: string;
  toDate: string;
  topCustomers: CustomerAnalysisSummary[];
  totalCustomerSales: number;
  totalUniqueCustomers: number;
}

export interface CustomerAnalysisSummary {
  customerId: number;
  customerName: string;
  totalPurchases: number;
  transactionCount: number;
  averageTransactionValue: number;
  lastPurchaseDate: string;
}

export interface SupplierBalanceReport {
  asOfDate: string;
  supplierBalances: SupplierBalanceSummary[];
  totalPayables: number;
}

export interface SupplierBalanceSummary {
  supplierId: number;
  supplierName: string;
  currentBalance: number;
  overdueAmount: number;
  daysOverdue: number;
  lastPaymentDate: string;
}

export interface InventoryValuationReport {
  branchId?: number;
  branchName?: string;
  asOfDate: string;
  items: InventoryValuationSummary[];
  totalInventoryValue: number;
}

export interface InventoryValuationSummary {
  productId: number;
  productName: string;
  category: string; // ProductCategoryType
  karatType: string; // KaratType enum
  quantityOnHand: number;
  weightOnHand: number;
  currentGoldRate: number;
  estimatedValue: number;
}

export interface TaxReport {
  branchId?: number;
  branchName?: string;
  fromDate: string;
  toDate: string;
  taxSummaries: TaxSummary[];
  totalTaxCollected: number;
}

export interface TaxSummary {
  taxName: string;
  taxCode: string;
  taxRate: number;
  taxableAmount: number;
  taxAmount: number;
}

export interface TransactionLogReport {
  branchId: number;
  branchName: string;
  reportDate: string;
  transactions: TransactionLogEntry[];
}

export interface TransactionLogEntry {
  transactionNumber: string;
  transactionDate: string;
  transactionType: string; // TransactionType enum
  customerName?: string;
  cashierName: string;
  totalAmount: number;
  status: string; // TransactionStatus enum
}

// Purchase Orders Management API interfaces
export interface PurchaseOrderDto {
  id: number;
  purchaseOrderNumber: string;
  supplierId: number;
  supplierName?: string;
  branchId: number;
  branchName?: string;
  orderDate: string;
  expectedDeliveryDate?: string;
  actualDeliveryDate?: string;
  totalAmount: number;
  amountPaid: number;
  outstandingBalance: number;
  status: string;
  paymentStatus: string;
  terms?: string;
  notes?: string;
  items: PurchaseOrderItemDto[];
}

export interface PurchaseOrderItemDto {
  id: number;
  productId: number;
  productCode?: string;
  productName?: string;
  quantityOrdered: number;
  quantityReceived: number;
  weightOrdered: number;
  weightReceived: number;
  unitCost: number;
  lineTotal: number;
  status: string;
  notes?: string;
}

export interface CreatePurchaseOrderRequest {
  supplierId: number;
  branchId: number;
  expectedDeliveryDate?: string;
  terms?: string;
  notes?: string;
  items: CreatePurchaseOrderItemRequest[];
}

export interface CreatePurchaseOrderItemRequest {
  productId: number;
  quantityOrdered: number;
  weightOrdered: number;
  unitCost: number;
  notes?: string;
}

export interface ReceivePurchaseOrderRequest {
  purchaseOrderId: number;
  items: ReceivePurchaseOrderItemDto[];
}

export interface ReceivePurchaseOrderItemDto {
  purchaseOrderItemId: number;
  quantityReceived: number;
  weightReceived: number;
}

export interface PurchaseOrderSearchRequest {
  purchaseOrderNumber?: string;
  supplierId?: number;
  branchId?: number;
  status?: string;
  fromDate?: string;
  toDate?: string;
  pageNumber?: number;
  pageSize?: number;
}

// Labels/Printing Management API interfaces
export interface DecodeQrRequest {
  payload: string;
}

export interface PrintLabelRequest {
  productId: number;
  copies?: number;
}

export interface GenerateZplRequest {
  productId: number;
  copies?: number;
}

// Customer API interfaces
export interface Customer {
  id: number;
  customerNumber: string;
  fullName: string;
  mobileNumber?: string;
  email?: string;
  address?: string;
  city?: string;
  dateOfBirth?: string;
  anniversaryDate?: string;
  loyaltyTier: number;
  defaultDiscountPercentage?: number;
  makingChargesWaived: boolean;
  totalPurchases: number;
  lastPurchaseDate?: string;
  customerSince: string;
  isActive: boolean;
  notes?: string;
}

// Transactions API interfaces
export interface TransactionItem {
  id: number;
  productId: number;
  productName: string;
  productCode: string;
  karatType: string;
  quantity: number;
  unitWeight: number;
  totalWeight: number;
  goldRatePerGram: number;
  unitPrice: number;
  makingChargesAmount: number;
  discountPercentage: number;
  discountAmount: number;
  lineTotal: number;
}

export interface Transaction {
  id: number;
  transactionNumber: string;
  transactionType: 'Sale' | 'Return' | 'Repair';
  transactionDate: string;
  branchId: number;
  branchName: string;
  customerId?: number;
  customerName?: string;
  cashierName: string;
  approvedByName?: string;
  subtotal: number;
  totalMakingCharges: number;
  totalTaxAmount: number;
  discountAmount: number;
  totalAmount: number;
  amountPaid: number;
  changeGiven: number;
  paymentMethod: 'Cash' | 'Card' | 'BankTransfer' | 'Cheque';
  status: 'Completed' | 'Pending' | 'Cancelled' | 'Refunded';
  items: TransactionItem[];
}

export interface SaleRequest {
  branchId: number;
  customerId?: number;
  items: Array<{
    productId: number;
    quantity: number;
    customDiscountPercentage?: number;
  }>;
  amountPaid: number;
  paymentMethod: 'Cash' | 'Card' | 'BankTransfer' | 'Cheque';
}

export const transactionsApi = {
  async processSale(sale: SaleRequest): Promise<Transaction> {
    const response = await apiRequest<Transaction>('/transactions/sale', {
      method: 'POST',
      body: JSON.stringify(sale),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to process sale');
  },

  async getTransaction(id: number): Promise<Transaction> {
    const response = await apiRequest<Transaction>(`/transactions/${id}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch transaction');
  },

  async searchTransactions(params: {
    branchId?: number;
    transactionNumber?: string;
    transactionType?: 'Sale' | 'Return' | 'Repair';
    status?: string;
    customerId?: number;
    fromDate?: string;
    toDate?: string;
    pageNumber?: number;
    pageSize?: number;
  } = {}): Promise<{
    items: Transaction[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
  }> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<{
      items: Transaction[];
      totalCount: number;
      pageNumber: number;
      pageSize: number;
    }>(`/transactions/search?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to search transactions');
  }
};

// Customers API
export const customersApi = {
  async getCustomers(params: {
    searchTerm?: string;
    mobileNumber?: string;
    email?: string;
    pageNumber?: number;
    pageSize?: number;
  } = {}): Promise<{
    items: Customer[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
  }> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<{
      items: Customer[];
      totalCount: number;
      pageNumber: number;
      pageSize: number;
    }>(`/customers?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch customers');
  },

  async getCustomer(id: number): Promise<Customer> {
    const response = await apiRequest<Customer>(`/customers/${id}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch customer');
  },

  async createCustomer(customer: Omit<Customer, 'id' | 'customerNumber' | 'customerSince' | 'totalPurchases' | 'lastPurchaseDate'>): Promise<Customer> {
    const response = await apiRequest<Customer>('/customers', {
      method: 'POST',
      body: JSON.stringify(customer),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to create customer');
  },

  async updateCustomer(id: number, customer: Partial<Omit<Customer, 'id' | 'customerNumber' | 'customerSince' | 'totalPurchases' | 'lastPurchaseDate'>>): Promise<Customer> {
    const response = await apiRequest<Customer>(`/customers/${id}`, {
      method: 'PUT',
      body: JSON.stringify(customer),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to update customer');
  }
};

// Lookups API for enum data
export const lookupsApi = {
  async getAllLookups(): Promise<ApiLookupsResponse> {
    const response = await apiRequest<ApiLookupsResponse>('/lookups');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch lookup data');
  },

  async getTransactionTypes(): Promise<EnumLookupDto[]> {
    const response = await apiRequest<EnumLookupDto[]>('/lookups/transaction-types');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch transaction types');
  },

  async getPaymentMethods(): Promise<EnumLookupDto[]> {
    const response = await apiRequest<EnumLookupDto[]>('/lookups/payment-methods');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch payment methods');
  },

  async getTransactionStatuses(): Promise<EnumLookupDto[]> {
    const response = await apiRequest<EnumLookupDto[]>('/lookups/transaction-statuses');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch transaction statuses');
  },

  async getKaratTypes(): Promise<EnumLookupDto[]> {
    const response = await apiRequest<EnumLookupDto[]>('/lookups/karat-types');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch karat types');
  },

  async getProductCategoryTypes(): Promise<EnumLookupDto[]> {
    const response = await apiRequest<EnumLookupDto[]>('/lookups/product-category-types');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch product category types');
  },

  async getChargeTypes(): Promise<EnumLookupDto[]> {
    const response = await apiRequest<EnumLookupDto[]>('/lookups/charge-types');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch charge types');
  }
};

// Initialize auth token on module load
if (typeof window !== 'undefined') {
  const token = localStorage.getItem(STORAGE_KEYS.AUTH_TOKEN);
  if (token) {
    setAuthToken(token);
  }
}

export default {
  auth: authApi,
  products: productsApi,
  pricing: pricingApi,
  users: usersApi,
  branches: branchesApi,
  suppliers: suppliersApi,
  inventory: inventoryApi,
  reports: reportsApi,
  purchaseOrders: purchaseOrdersApi,
  labels: labelsApi,
  transactions: transactionsApi,
  customers: customersApi,
  lookups: lookupsApi,
};

/**
 * API Service for DijaGold POS System
 * Handles all communication with the backend API
 */

import { API_CONFIG, STORAGE_KEYS } from '../config/environment';
import { EnumLookupDto } from '../types/lookups';

// Base API configuration
const API_BASE_URL = `${API_CONFIG.BASE_URL}/api`;

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
    
    console.log('Login API response:', response);
    
    if (response.success && response.data) {
      console.log('Login successful, user data:', response.data.user);
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
    
    console.log('getCurrentUser API response:', response);
    
    if (response.success && response.data) {
      console.log('User data from API:', response.data);
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
    
    console.log('Refresh token API response:', response);
    
    if (response.success && response.data) {
      console.log('Token refreshed, user data:', response.data.user);
      setAuthToken(response.data.token);
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to refresh token');
  },

  async debugUsers(): Promise<any> {
    const response = await apiRequest('/auth/debug/users');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to get debug user information');
  }
};

// Lookup DTOs - Updated to match backend structure
export interface BaseLookupDto {
  id: number;
  name: string;
  description?: string;
  sortOrder: number;
  isActive: boolean;
}

export interface ProductCategoryTypeLookupDto extends BaseLookupDto {}

export interface KaratTypeLookupDto extends BaseLookupDto {}

export interface SubCategoryLookupDto extends BaseLookupDto {}

export interface FinancialTransactionTypeLookupDto extends BaseLookupDto {}

export interface PaymentMethodLookupDto extends BaseLookupDto {}

export interface FinancialTransactionStatusLookupDto extends BaseLookupDto {}

export interface ChargeTypeLookupDto extends BaseLookupDto {}

export interface RepairStatusLookupDto extends BaseLookupDto {}

export interface RepairPriorityLookupDto extends BaseLookupDto {}

export interface OrderTypeLookupDto extends BaseLookupDto {}

export interface OrderStatusLookupDto extends BaseLookupDto {}

export interface BusinessEntityTypeLookupDto extends BaseLookupDto {}

// Products API
export interface Product {
  id: number;
  productCode: string;
  name: string;
  categoryTypeId: number; // ProductCategoryType enum value
  categoryType?: ProductCategoryTypeLookupDto;
  karatTypeId: number; // KaratType enum value
  karatType?: KaratTypeLookupDto;
  weight: number;
  brand?: string;
  designStyle?: string;
  subCategoryId?: number;
  subCategory?: SubCategoryLookupDto;
  shape?: string;
  purityCertificateNumber?: string;
  countryOfOrigin?: string;
  yearOfMinting?: number;
  faceValue?: number;
  hasNumismaticValue?: boolean;
  makingChargesApplicable: boolean;
  productMakingChargesTypeId?: number;
  productMakingChargesValue?: number;
  useProductMakingCharges: boolean;
  supplierId?: number;
  createdAt: string;
  isActive: boolean;
}

export interface ProductSearchRequest {
  searchTerm?: string;
  categoryTypeId?: number;
  karatTypeId?: number;
  brand?: string;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface ProductPricingDto {
  productId: number;
  productName: string;
  currentGoldRate: number;
  estimatedBasePrice: number;
  estimatedMakingCharges: number;
  estimatedTotalPrice: number;
  priceCalculatedAt: string;
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

  async getProductPricing(id: number, quantity: number = 1, customerId?: number): Promise<ProductPricingDto> {
    const queryParams = new URLSearchParams({
      quantity: quantity.toString(),
      ...(customerId && { customerId: customerId.toString() })
    });
    
    const response = await apiRequest<ProductPricingDto>(`/products/${id}/pricing?${queryParams}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch product pricing');
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
  karatTypeId: number; // KaratType enum value (1, 2, 3, 4)
  ratePerGram: number;
  effectiveFrom: string;
  effectiveTo?: string;
  isCurrent: boolean;
  createdAt: string;
  createdBy: string;
  karatType?: {
    id: number;
    name: string;
    description?: string;
    sortOrder: number;
    isActive: boolean;
  };
}

export interface MakingCharges {
  id: number;
  name: string;
  productCategory: number; // ProductCategoryType enum value
  subCategory?: string;
  chargeType: number; // ChargeType enum value (1 = Percentage, 2 = Fixed)
  chargeValue: number;
  effectiveFrom: string;
  effectiveTo?: string;
  isCurrent: boolean;
  createdAt: string;
  createdBy: string;
}

export interface TaxConfigurationDto {
  id: number;
  taxName: string;
  taxCode: string;
  taxType: number; // ChargeType enum value (1 = Percentage, 2 = Fixed)
  taxRate: number;
  isMandatory: boolean;
  effectiveFrom: string;
  effectiveTo?: string;
  isCurrent: boolean;
  displayOrder: number;
  createdAt: string;
  createdBy: string;
}

export const pricingApi = {
  async getGoldRates(): Promise<GoldRate[]> {
    const response = await apiRequest<any[]>('/pricing/gold-rates');
    
    if (response.success && response.data) {
      // Map backend response to frontend camelCase (handling both PascalCase and camelCase)
      return response.data.map((rate: any) => ({
        id: rate.id || rate.Id,
        karatTypeId: rate.karatTypeId || rate.KaratTypeId,
        ratePerGram: rate.ratePerGram || rate.RatePerGram,
        effectiveFrom: rate.effectiveFrom || rate.EffectiveFrom,
        effectiveTo: rate.effectiveTo || rate.EffectiveTo,
        isCurrent: rate.isCurrent || rate.IsCurrent,
        createdAt: rate.createdAt || rate.CreatedAt,
        createdBy: rate.createdBy || rate.CreatedBy,
        karatType: rate.karatType || rate.KaratType ? {
          id: (rate.karatType || rate.KaratType).id || (rate.karatType || rate.KaratType).Id,
          name: (rate.karatType || rate.KaratType).name || (rate.karatType || rate.KaratType).Name,
          description: (rate.karatType || rate.KaratType).description || (rate.karatType || rate.KaratType).Description,
          sortOrder: (rate.karatType || rate.KaratType).sortOrder || (rate.karatType || rate.KaratType).SortOrder,
          isActive: (rate.karatType || rate.KaratType).isActive || (rate.karatType || rate.KaratType).IsActive
        } : undefined
      }));
    }
    
    throw new Error(response.message || 'Failed to fetch gold rates');
  },

  async updateGoldRates(goldRates: {
    karatTypeId: number;
    ratePerGram: number;
    effectiveFrom: string;
  }[]): Promise<void> {
    // Map frontend camelCase to backend PascalCase
    const requestData = {
      GoldRates: goldRates.map(rate => ({
        KaratTypeId: rate.karatTypeId,
        RatePerGram: rate.ratePerGram,
        EffectiveFrom: rate.effectiveFrom
      }))
    };
    
    const response = await apiRequest<void>('/pricing/gold-rates', {
      method: 'POST',
      body: JSON.stringify(requestData),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to update gold rates');
    }
  },

  async getMakingCharges(): Promise<MakingCharges[]> {
    const response = await apiRequest<any[]>('/pricing/making-charges');
    
    if (response.success && response.data) {
      // Map backend response to frontend camelCase (handling both PascalCase and camelCase)
      return response.data.map((charge: any) => ({
        id: charge.id || charge.Id,
        name: charge.name || charge.Name,
        productCategory: charge.productCategoryId || charge.ProductCategoryId,
        subCategory: charge.subCategory || charge.SubCategory,
        chargeType: charge.chargeTypeId || charge.ChargeTypeId,
        chargeValue: charge.chargeValue || charge.ChargeValue,
        effectiveFrom: charge.effectiveFrom || charge.EffectiveFrom,
        effectiveTo: charge.effectiveTo || charge.EffectiveTo,
        isCurrent: charge.isCurrent || charge.IsCurrent,
        createdAt: charge.createdAt || charge.CreatedAt,
        createdBy: charge.createdBy || charge.CreatedBy
      }));
    }
    
    throw new Error(response.message || 'Failed to fetch making charges');
  },

  async updateMakingCharges(charges: {
    id?: number;
    name: string;
    productCategory: number;
    subCategory?: string;
    chargeType: number;
    chargeValue: number;
    effectiveFrom: string;
  }): Promise<void> {
    // Map frontend camelCase to backend PascalCase
    const requestData = {
      Id: charges.id,
      Name: charges.name,
      ProductCategoryId: charges.productCategory,
      SubCategory: charges.subCategory,
      ChargeTypeId: charges.chargeType,
      ChargeValue: charges.chargeValue,
      EffectiveFrom: charges.effectiveFrom
    };

    const response = await apiRequest<void>('/pricing/making-charges', {
      method: 'POST',
      body: JSON.stringify(requestData),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to update making charges');
    }
  },

  async getTaxConfigurations(): Promise<TaxConfigurationDto[]> {
    const response = await apiRequest<any[]>('/pricing/taxes');
    
    if (response.success && response.data) {
      // Map backend response to frontend camelCase (handling both PascalCase and camelCase)
      return response.data.map((tax: any) => ({
        id: tax.id || tax.Id,
        taxName: tax.taxName || tax.TaxName,
        taxCode: tax.taxCode || tax.TaxCode,
        taxType: tax.taxTypeId || tax.TaxTypeId,
        taxRate: tax.taxRate || tax.TaxRate,
        isMandatory: tax.isMandatory || tax.IsMandatory,
        effectiveFrom: tax.effectiveFrom || tax.EffectiveFrom,
        effectiveTo: tax.effectiveTo || tax.EffectiveTo,
        isCurrent: tax.isCurrent || tax.IsCurrent,
        displayOrder: tax.displayOrder || tax.DisplayOrder,
        createdAt: tax.createdAt || tax.CreatedAt,
        createdBy: tax.createdBy || tax.CreatedBy
      }));
    }
    
    throw new Error(response.message || 'Failed to fetch tax configurations');
  },

  async updateTaxConfiguration(taxConfig: {
    id?: number;
    taxName: string;
    taxCode: string;
    taxType: number;
    taxRate: number;
    isMandatory: boolean;
    effectiveFrom: string;
    displayOrder: number;
  }): Promise<void> {
    // Map frontend camelCase to backend PascalCase
    const requestData = {
      Id: taxConfig.id,
      TaxName: taxConfig.taxName,
      TaxCode: taxConfig.taxCode,
      TaxTypeId: taxConfig.taxType, // This should be ChargeType enum value
      TaxRate: taxConfig.taxRate,
      IsMandatory: taxConfig.isMandatory,
      EffectiveFrom: taxConfig.effectiveFrom,
      DisplayOrder: taxConfig.displayOrder
    };

    const response = await apiRequest<void>('/pricing/taxes', {
      method: 'POST',
      body: JSON.stringify(requestData),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to update tax configuration');
    }
  },
};

// User Management API interfaces
export interface UserDto {
  Id: string;
  UserName: string;
  FullName: string;
  Email: string;
  EmployeeCode?: string;
  Roles: string[];
  BranchId?: number;
  BranchName?: string;
  IsActive: boolean;
  CreatedAt: string;
  LastLoginAt?: string;
  EmailConfirmed: boolean;
  LockoutEnabled: boolean;
  LockoutEnd?: string;
}

export interface CreateUserRequest {
  UserName: string;
  FullName: string;
  Email: string;
  EmployeeCode?: string;
  Password: string;
  Roles: string[];
  BranchId?: number;
  IsActive: boolean;
}

export interface UpdateUserRequest {
  Id: string;
  FullName: string;
  Email: string;
  EmployeeCode?: string;
  BranchId?: number;
}

export interface UpdateUserRoleRequest {
  UserId: string;
  Roles: string[];
}

export interface UpdateUserStatusRequest {
  UserId: string;
  IsActive: boolean;
  Reason?: string;
}

export interface UserSearchRequest {
  searchTerm?: string;
  Role?: string;
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
  Id: number;
  Timestamp: string;
  Action: string;
  EntityType: string;
  EntityId: string;
  Details: string;
  BranchName?: string;
  IpAddress: string;
}

export interface ResetPasswordRequest {
  UserId: string;
  NewPassword: string;
  ForcePasswordChange: boolean;
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
      body: JSON.stringify(user),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to update user');
    }
  },

  async updateUserRoles(id: string, request: UpdateUserRoleRequest): Promise<void> {
    const response = await apiRequest(`/users/${id}/roles`, {
      method: 'PUT',
      body: JSON.stringify(request),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to update user roles');
    }
  },

  async updateUserStatus(id: string, request: UpdateUserStatusRequest): Promise<void> {
    const response = await apiRequest(`/users/${id}/status`, {
      method: 'PUT',
      body: JSON.stringify(request),
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
      body: JSON.stringify(request),
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
      body: JSON.stringify(request),
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

// Cash Drawer API
export const cashDrawerApi = {
  async openDrawer(branchId: number, openingBalance: number, date?: string, notes?: string): Promise<CashDrawerBalance> {
    const response = await apiRequest<CashDrawerBalance>('/cashdrawer/open', {
      method: 'POST',
      body: JSON.stringify({
        branchId,
        openingBalance,
        date,
        notes
      })
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to open cash drawer');
  },

  async closeDrawer(branchId: number, actualClosingBalance: number, date?: string, notes?: string): Promise<CashDrawerBalance> {
    const response = await apiRequest<CashDrawerBalance>('/cashdrawer/close', {
      method: 'POST',
      body: JSON.stringify({
        branchId,
        actualClosingBalance,
        date,
        notes
      })
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to close cash drawer');
  },

  async getBalance(branchId: number, date: string): Promise<CashDrawerBalance> {
    const queryString = new URLSearchParams({
      branchId: branchId.toString(),
      date: date
    }).toString();
    
    const response = await apiRequest<CashDrawerBalance>(`/cashdrawer/balance?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to get cash drawer balance');
  },

  async getOpeningBalance(branchId: number, date: string): Promise<number> {
    const queryString = new URLSearchParams({
      branchId: branchId.toString(),
      date: date
    }).toString();
    
    const response = await apiRequest<number>(`/cashdrawer/opening-balance?${queryString}`);
    
    if (response.success && response.data !== undefined) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to get opening balance');
  },

  async isDrawerOpen(branchId: number, date: string): Promise<boolean> {
    const queryString = new URLSearchParams({
      branchId: branchId.toString(),
      date: date
    }).toString();
    
    const response = await apiRequest<boolean>(`/cashdrawer/is-open?${queryString}`);
    
    if (response.success && response.data !== undefined) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to check drawer status');
  },

  async getBalances(branchId: number, fromDate: string, toDate: string): Promise<CashDrawerBalance[]> {
    const queryString = new URLSearchParams({
      branchId: branchId.toString(),
      fromDate,
      toDate
    }).toString();
    
    const response = await apiRequest<CashDrawerBalance[]>(`/cashdrawer/balances?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to get cash drawer balances');
  },

  async settleShift(branchId: number, actualClosingBalance: number, settledAmount: number, date?: string, settlementNotes?: string, notes?: string): Promise<CashDrawerBalance> {
    const response = await apiRequest<CashDrawerBalance>('/cashdrawer/settle-shift', {
      method: 'POST',
      body: JSON.stringify({
        branchId,
        actualClosingBalance,
        settledAmount,
        date,
        settlementNotes,
        notes
      })
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to settle shift');
  },

  async refreshExpectedClosingBalance(branchId: number, date?: string): Promise<CashDrawerBalance> {
    const queryString = new URLSearchParams({
      branchId: branchId.toString(),
      ...(date && { date })
    }).toString();
    
    const response = await apiRequest<CashDrawerBalance>(`/cashdrawer/refresh-balance?${queryString}`, {
      method: 'POST'
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to refresh cash drawer balance');
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
    const response = await fetch(`${API_BASE_URL}/reports/export/excel`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getAuthToken()}`
      },
      body: JSON.stringify(request)
    });

    if (!response.ok) {
      let errorMessage = `HTTP ${response.status}: ${response.statusText}`;
      try {
        const errorData = await response.json();
        if (errorData.message) {
          errorMessage = errorData.message;
        }
      } catch {
        // If we can't parse the error response, use the default message
      }
      throw new Error(`Failed to export to Excel: ${errorMessage}`);
    }

    const blob = await response.blob();
    if (!blob || blob.size === 0) {
      throw new Error('Server returned empty file');
    }

    return blob;
  },

  async exportToPdf(request: ExportReportRequest): Promise<Blob> {
    const response = await fetch(`${API_BASE_URL}/reports/export/pdf`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getAuthToken()}`
      },
      body: JSON.stringify(request)
    });

    if (!response.ok) {
      let errorMessage = `HTTP ${response.status}: ${response.statusText}`;
      try {
        const errorData = await response.json();
        if (errorData.message) {
          errorMessage = errorData.message;
        }
      } catch {
        // If we can't parse the error response, use the default message
      }
      throw new Error(`Failed to export to PDF: ${errorMessage}`);
    }

    const blob = await response.blob();
    if (!blob || blob.size === 0) {
      throw new Error('Server returned empty file');
    }

    return blob;
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
  paymentMethod: number; // PaymentMethod enum value
  amount: number;
  transactionCount: number;
}

export interface CashDrawerBalance {
  id: number;
  branchId: number;
  balanceDate: string;
  openingBalance: number;
  expectedClosingBalance: number;
  actualClosingBalance?: number;
  openedByUserId?: string;
  closedByUserId?: string;
  openedAt?: string;
  closedAt?: string;
  notes?: string;
  status: number; // 1=Open, 2=Closed, 3=PendingReconciliation
  cashOverShort?: number;
  createdAt: string;
  createdBy?: string;
  modifiedAt?: string;
  modifiedBy?: string;
  isActive: boolean;
  settledAmount?: number;
  carriedForwardAmount?: number;
  settlementNotes?: string;
}

export interface CashReconciliationReport {
  branchId: number;
  reportDate: string;
  openingBalance: number;
  cashSales: number;
  cashReturns: number;
  cashRepairs: number;
  expectedClosingBalance: number;
  actualClosingBalance: number;
  cashOverShort: number;
}

export interface InventoryMovementReport {
  branchId: number;
  fromDate: string;
  toDate: string;
  movements: InventoryMovementSummary[];
}

export interface InventoryMovementSummary {
  productId: number;
  productName: string;
  category: number; // ProductCategoryType
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
  currentBalance: number;
  overdueAmount: number;
  daysOverdue: number;
  lastPaymentDate: string;
}

export interface InventoryValuationReport {
  branchId?: number;
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
  fullName: string;
  nationalId?: string;
  mobileNumber?: string;
  email?: string;
  address?: string;
  registrationDate: string;
  loyaltyTier: number;
  loyaltyPoints: number;
  totalPurchaseAmount: number;
  defaultDiscountPercentage: number;
  makingChargesWaived: boolean;
  notes?: string;
  lastPurchaseDate?: string;
  totalOrders: number;
  createdAt: string;
  isActive: boolean;
}

export interface CreateCustomerRequest {
  fullName: string;
  nationalId?: string;
  mobileNumber?: string;
  email?: string;
  address?: string;
  loyaltyTier: number;
  defaultDiscountPercentage: number;
  makingChargesWaived: boolean;
  notes?: string;
}

export interface UpdateCustomerRequest {
  id: number;
  fullName: string;
  nationalId?: string;
  mobileNumber?: string;
  email?: string;
  address?: string;
  loyaltyTier: number;
  defaultDiscountPercentage: number;
  makingChargesWaived: boolean;
  notes?: string;
}

export interface CustomerSearchRequest {
  searchTerm?: string;
  nationalId?: string;
  mobileNumber?: string;
  email?: string;
  loyaltyTier?: number;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface CustomerOrderDto {
  orderId: number;
  orderNumber: string;
  orderDate: string;
  orderType: string;
  totalAmount: number;
  cashierName: string;
}

export interface CustomerOrdersHistoryDto {
  customerId: number;
  customerName: string;
  orders: CustomerOrderDto[];
  totalAmount: number;
  totalOrderCount: number;
}

export interface CustomerLoyaltyDto {
  customerId: number;
  customerName: string;
  currentTier: number;
  currentPoints: number;
  pointsToNextTier: number;
  totalPurchaseAmount: number;
  defaultDiscountPercentage: number;
  makingChargesWaived: boolean;
  lastPurchaseDate?: string;
  totalOrders: number;
}

export interface UpdateCustomerLoyaltyRequest {
  customerId: number;
  loyaltyTier: number;
  loyaltyPoints: number;
  defaultDiscountPercentage: number;
  makingChargesWaived: boolean;
}

// Financial Transactions API interfaces (New Architecture)
export interface OrderItem {
  id: number;
  orderId: number;
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

export interface FinancialTransactionTax {
  id: number;
  financialTransactionId: number;
  taxConfigurationId: number;
  taxName: string;
  taxCode: string;
  taxRate: number;
  taxableAmount: number;
  taxAmount: number;
}

export interface FinancialTransaction {
  id: number;
  transactionNumber: string;
  transactionTypeId: number;
  transactionType: 'Sale' | 'Return' | 'Repair';
  transactionDate: string;
  branchId: number;
  branchCode?: string;
  customerId?: number;
  customerName?: string;
  customerMobile?: string;
  customerEmail?: string;
  processedByUserId?: string;
  processedByName: string;
  approvedByUserId?: string;
  approvedByName?: string;
  orderId?: number;
  order?: {
    id: number;
    orderNumber: string;
    orderDate: string;
    orderTypeId: number;
    orderType?: OrderTypeLookupDto;
    items: OrderItem[];
  };
  subtotal: number;
  totalMakingCharges: number;
  totalTaxAmount: number;
  totalDiscountAmount?: number;
  discountAmount: number;
  totalAmount: number;
  amountPaid: number;
  changeGiven: number;
  paymentMethodId: number;
  paymentMethod: string;
  statusId: number;
  status: 'Completed' | 'Pending' | 'Cancelled' | 'Voided';
  statusDisplayName?: string;
  receiptPrinted?: boolean;
  generalLedgerPosted?: boolean;
  notes?: string;
  taxes?: FinancialTransactionTax[];
  // Repair-specific fields
  repairDescription?: string;
  estimatedCompletionDate?: string;
  // Return-specific fields
  returnReason?: string;
  originalTransactionId?: number;
  originalTransaction?: FinancialTransaction;
}

// Legacy Transaction interface for backward compatibility
export interface Transaction extends Omit<FinancialTransactionDto, 'order'> {
  items: OrderItem[]; // Alias for order.items
  cashierName: string; // Alias for processedByName
  cashierEmployeeCode?: string; // Employee code for the cashier
  customerMobile?: string; // Customer mobile number
  customerEmail?: string; // Customer email
  branchCode?: string; // Branch code
  order?: {
    id: number;
    orderNumber: string;
    orderDate: string;
    orderTypeId: number;
    orderType?: OrderTypeLookupDto;
    items: OrderItem[];
  };
}

// New API interfaces for Orders and FinancialTransactions
export interface OrderDto {
  id: number;
  orderNumber: string;
  orderDate: string;
  orderTypeId: number;
  orderType?: OrderTypeLookupDto;
  orderTypeDescription: string;
  branchId: number;
  customerId?: number;
  customerName?: string;
  cashierId: string;
  cashierName: string;
  statusId: number;
  status?: OrderStatusLookupDto;
  statusDescription: string;
  notes?: string;
  estimatedCompletionDate?: string;
  financialTransactionId?: number;
  financialTransactionNumber?: string;
  items: OrderItemDto[];
  createdAt: string;
  updatedAt?: string;
}

export interface OrderItemDto {
  id: number;
  orderId: number;
  productId: number;
  productName: string;
  productCode: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  discountPercentage: number;
  discountAmount: number;
  finalPrice: number;
  makingCharges: number;
  taxAmount: number;
  totalAmount: number;
  karatType?: string;
  weight?: number;
  notes?: string;
}

// Helper function to convert OrderItemDto to OrderItem for backward compatibility
function convertOrderItemDtoToOrderItem(dto: OrderItemDto): OrderItem {
  return {
    id: dto.id,
    orderId: dto.orderId,
    productId: dto.productId,
    productName: dto.productName,
    productCode: dto.productCode,
    karatType: dto.karatType || '',
    quantity: dto.quantity,
    unitWeight: dto.weight || 0,
    totalWeight: (dto.weight || 0) * dto.quantity,
    goldRatePerGram: dto.unitPrice / (dto.weight || 1), // Approximate calculation
    unitPrice: dto.unitPrice,
    makingChargesAmount: dto.makingCharges,
    discountPercentage: dto.discountPercentage,
    discountAmount: dto.discountAmount,
    lineTotal: dto.totalAmount || dto.finalPrice
  };
}

// Helper function to convert OrderItemDto[] to OrderItem[]
export function convertOrderItemDtosToOrderItems(dtos: OrderItemDto[]): OrderItem[] {
  return dtos.map(convertOrderItemDtoToOrderItem);
}

// Helper function to convert FinancialTransactionDto to Transaction for backward compatibility
export function convertFinancialTransactionDtoToTransaction(dto: FinancialTransactionDto): Transaction {
  return {
    ...dto,
    items: convertOrderItemDtosToOrderItems(dto.order?.items || []),
    cashierName: dto.processedByUserName,
    order: dto.order ? {
      id: dto.order.id,
      orderNumber: dto.order.orderNumber,
      orderDate: dto.order.orderDate,
      orderTypeId: dto.order.orderTypeId,
      orderType: dto.order.orderType,
      items: convertOrderItemDtosToOrderItems(dto.order.items)
    } : undefined
  };
}



export interface CreateSaleOrderRequestDto {
  branchId: number;
  customerId?: number;
  goldRateId?: number;
  notes?: string;
  items: CreateOrderItemRequestDto[];
  amountPaid: number;
  paymentMethodId: number;
  paymentNotes?: string;
}

export interface CreateOrderItemRequestDto {
  productId: number;
  quantity: number;
  customDiscountPercentage?: number;
  notes?: string;
}

export interface CreateRepairOrderRequestDto {
  branchId: number;
  customerId?: number;
  repairDescription: string;
  repairAmount: number;
  amountPaid: number;
  paymentMethodId: number;
  estimatedCompletionDate?: string;
  priorityId: number; // RepairPriority enum value
  assignedTechnicianId?: number;
  technicianNotes?: string;
}

export interface FinancialTransactionDto {
  id: number;
  transactionNumber: string;
  transactionDate: string;
  branchId: number;
  transactionTypeId: number;
  transactionType?: FinancialTransactionTypeLookupDto;
  transactionTypeDescription: string;
  businessEntityId?: number;
  businessEntityType?: BusinessEntityTypeLookupDto;
  processedByUserId: string;
  processedByUserName: string;
  approvedByUserId?: string;
  approvedByUserName?: string;
  subtotal: number;
  totalTaxAmount: number;
  totalDiscountAmount: number;
  totalAmount: number;
  amountPaid: number;
  changeGiven: number;
  paymentMethodId: number;
  paymentMethod?: PaymentMethodLookupDto;
  paymentMethodDescription: string;
  statusId: number;
  status?: FinancialTransactionStatusLookupDto;
  statusDescription: string;
  originalTransactionId?: number;
  reversalReason?: string;
  notes?: string;
  receiptPrinted: boolean;
  generalLedgerPosted: boolean;
  createdAt: string;
  updatedAt?: string;
  // Additional fields for UI compatibility
  customerId?: number;
  customerName?: string;
  orderId?: number;
  order?: OrderDto;
  repairJobId?: number;
  repairJob?: RepairJobDto;
  taxes?: FinancialTransactionTax[];
  totalMakingCharges?: number;
  discountAmount?: number;
  repairDescription?: string;
  estimatedCompletionDate?: string;
  returnReason?: string;
}

export interface RepairJobDto {
  id: number;
  financialTransactionId?: number;
  financialTransactionNumber?: string;
  statusId: number; // RepairStatus enum value
  status?: RepairStatusLookupDto;
  statusDisplayName: string;
  priorityId: number; // RepairPriority enum value
  priority?: RepairPriorityLookupDto;
  priorityDisplayName: string;
  assignedTechnicianId?: number;
  assignedTechnicianName?: string;
  startedDate?: string;
  completedDate?: string;
  readyForPickupDate?: string;
  deliveredDate?: string;
  technicianNotes?: string;
  actualCost?: number;
  materialsUsed?: string;
  hoursSpent?: number;
  qualityCheckedBy?: number;
  qualityCheckerName?: string;
  qualityCheckDate?: string;
  customerNotified: boolean;
  customerNotificationDate?: string;
  createdAt: string;
  createdBy: string;
  createdByName: string;
  repairDescription: string;
  repairAmount: number;
  amountPaid: number;
  estimatedCompletionDate?: string;
  customerId?: number;
  customerName?: string;
  customerPhone?: string;
  branchId: number;
}

export interface RepairOrderResultDto {
  order?: OrderDto;
  financialTransaction?: FinancialTransactionDto;
  repairJob?: RepairJobDto;
}

export interface UpdateOrderRequestDto {
  statusId: number;
  notes?: string;
  estimatedCompletionDate?: string;
  returnReason?: string;
}

export interface OrderSummaryDto {
  totalOrders: number;
  totalValue: number;
  orderTypeCounts: { [key: number]: number };
  statusCounts: { [key: number]: number };
}

export interface OrderSearchRequestDto {
  branchId?: number;
  orderTypeId?: number;
  statusId?: number;
  fromDate?: string;
  toDate?: string;
  orderNumber?: string;
  customerId?: number;
  cashierId?: string;
  page?: number;
  pageSize?: number;
}

export const ordersApi = {
  async createSaleOrder(request: CreateSaleOrderRequestDto): Promise<OrderDto> {
    const response = await apiRequest<OrderDto>('/orders/sale', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to create sale order');
  },

  async createRepairOrder(request: CreateRepairOrderRequestDto): Promise<RepairOrderResultDto> {
    const response = await apiRequest<RepairOrderResultDto>('/orders/repair', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to create repair order');
  },

  async getOrder(id: number): Promise<OrderDto> {
    const response = await apiRequest<OrderDto>(`/orders/${id}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch order');
  },

  async getOrderByNumber(orderNumber: string, branchId: number): Promise<OrderDto> {
    const queryString = new URLSearchParams({
      branchId: branchId.toString()
    }).toString();
    
    const response = await apiRequest<OrderDto>(`/orders/number/${orderNumber}?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch order by number');
  },

  async searchOrders(params: OrderSearchRequestDto = {}): Promise<{
    items: OrderDto[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
  }> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<{
      items: OrderDto[];
      totalCount: number;
      page: number;
      pageSize: number;
      totalPages: number;
    }>(`/orders/search?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to search orders');
  },

  async getOrderSummary(params: {
    branchId?: number;
    fromDate?: string;
    toDate?: string;
  } = {}): Promise<OrderSummaryDto> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<OrderSummaryDto>(`/orders/summary?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to get order summary');
  },

  async getCustomerOrders(customerId: number, params: {
    fromDate?: string;
    toDate?: string;
  } = {}): Promise<OrderDto[]> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<OrderDto[]>(`/orders/customer/${customerId}?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to get customer orders');
  },

  async getCashierOrders(cashierId: string, params: {
    fromDate?: string;
    toDate?: string;
  } = {}): Promise<OrderDto[]> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<OrderDto[]>(`/orders/cashier/${cashierId}?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to get cashier orders');
  },

  async updateOrder(id: number, request: UpdateOrderRequestDto): Promise<OrderDto> {
    const response = await apiRequest<OrderDto>(`/orders/${id}`, {
      method: 'PUT',
      body: JSON.stringify(request),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to update order');
  }
};

// Financial Transaction API interfaces
export interface CreateFinancialTransactionRequestDto {
  branchId: number;
  transactionTypeId: number;
  businessEntityId?: number;
  businessEntityTypeId: number;
  subtotal: number;
  totalTaxAmount: number;
  totalDiscountAmount: number;
  totalAmount: number;
  amountPaid: number;
  changeGiven: number;
  paymentMethodId: number;
  notes?: string;
  approvedByUserId?: string;
}

export interface FinancialTransactionSearchRequestDto {
  branchId?: number;
  transactionTypeId?: number;
  statusId?: number;
  fromDate?: string;
  toDate?: string;
  transactionNumber?: string;
  processedByUserId?: string;
  businessEntityId?: number;
  businessEntityTypeId?: number;
  page?: number;
  pageSize?: number;
}

export interface FinancialTransactionSummaryDto {
  totalTransactions: number;
  totalAmount: number;
  totalTaxAmount: number;
  totalDiscountAmount: number;
  transactionTypeCounts: { [key: number]: number };
  statusCounts: { [key: number]: number };
}

export interface VoidFinancialTransactionRequestDto {
  reason: string;
}

export const financialTransactionsApi = {
  async createFinancialTransaction(request: CreateFinancialTransactionRequestDto): Promise<FinancialTransactionDto> {
    const response = await apiRequest<FinancialTransactionDto>('/financialtransactions', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to create financial transaction');
  },

  async getFinancialTransaction(id: number): Promise<FinancialTransactionDto> {
    const response = await apiRequest<FinancialTransactionDto>(`/financialtransactions/${id}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch financial transaction');
  },

  async getFinancialTransactionByNumber(transactionNumber: string, branchId: number): Promise<FinancialTransactionDto> {
    const queryString = new URLSearchParams({
      branchId: branchId.toString()
    }).toString();
    
    const response = await apiRequest<FinancialTransactionDto>(`/financialtransactions/number/${transactionNumber}?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch financial transaction by number');
  },

  async searchFinancialTransactions(params: FinancialTransactionSearchRequestDto = {}): Promise<{
    items: FinancialTransactionDto[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
  }> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<{
      items: FinancialTransactionDto[];
      totalCount: number;
      page: number;
      pageSize: number;
      totalPages: number;
    }>(`/financialtransactions/search?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to search financial transactions');
  },

  async getFinancialTransactionSummary(params: {
    branchId?: number;
    fromDate?: string;
    toDate?: string;
  } = {}): Promise<FinancialTransactionSummaryDto> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<FinancialTransactionSummaryDto>(`/financialtransactions/summary?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to get financial transaction summary');
  },

  async voidFinancialTransaction(id: number, request: VoidFinancialTransactionRequestDto): Promise<FinancialTransactionDto> {
    const response = await apiRequest<FinancialTransactionDto>(`/financialtransactions/${id}/void`, {
      method: 'POST',
      body: JSON.stringify(request),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to void financial transaction');
  },

  async markReceiptPrinted(id: number): Promise<boolean> {
    const response = await apiRequest<boolean>(`/financialtransactions/${id}/mark-receipt-printed`, {
      method: 'POST',
    });
    
    if (response.success) {
      return true;
    }
    
    throw new Error(response.message || 'Failed to mark receipt as printed');
  },

  async markGeneralLedgerPosted(id: number): Promise<boolean> {
    const response = await apiRequest<boolean>(`/financialtransactions/${id}/mark-gl-posted`, {
      method: 'POST',
    });
    
    if (response.success) {
      return true;
    }
    
    throw new Error(response.message || 'Failed to mark general ledger as posted');
  },

  async generateBrowserReceipt(transactionId: number): Promise<{
    receiptData: any;
    template: any;
    htmlTemplate: string;
    cssStyles: string;
    transactionNumber: string;
    transactionDate: string;
    transactionType: string;
  }> {
    const response = await apiRequest<{
      receiptData: any;
      template: any;
      htmlTemplate: string;
      cssStyles: string;
      transactionNumber: string;
      transactionDate: string;
      transactionType: string;
    }>('/financialtransactions/generate-browser-receipt', {
      method: 'POST',
      body: JSON.stringify({
        transactionId,
        includeHtml: true,
        includeCss: true
      }),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to generate browser receipt');
  },

  async debugCalculation(request: any): Promise<any> {
    const response = await apiRequest<any>('/financialtransactions/debug-calculation', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to debug calculation');
  }
};

// Initialize auth token on module load
if (typeof window !== 'undefined') {
  const token = localStorage.getItem(STORAGE_KEYS.AUTH_TOKEN);
  if (token) {
    setAuthToken(token);
  }
}

// ProductOwnership API interfaces
export interface ProductOwnershipDto {
  id: number;
  productId: number;
  productName: string;
  productCode: string;
  branchId: number;
  supplierId?: number;
  purchaseOrderId?: number;
  purchaseOrderNumber?: string;
  customerPurchaseId?: number;
  customerPurchaseNumber?: string;
  totalQuantity: number;
  totalWeight: number;
  ownedQuantity: number;
  ownedWeight: number;
  ownershipPercentage: number;
  totalCost: number;
  amountPaid: number;
  outstandingAmount: number;
  isActive: boolean;
  createdAt: string;
}

export interface ProductOwnershipRequest {
  productId: number;
  branchId: number;
  supplierId?: number;
  purchaseOrderId?: number;
  customerPurchaseId?: number;
  totalQuantity: number;
  totalWeight: number;
  ownedQuantity: number;
  ownedWeight: number;
  totalCost: number;
  amountPaid: number;
}

export interface OwnershipMovementDto {
  id: number;
  productOwnershipId: number;
  movementType: string;
  movementDate: string;
  referenceNumber?: string;
  quantityChange: number;
  weightChange: number;
  amountChange: number;
  ownedQuantityAfter: number;
  ownedWeightAfter: number;
  amountPaidAfter: number;
  ownershipPercentageAfter: number;
  notes?: string;
  createdByUserId: string;
  createdAt: string;
}

export interface CreateOwnershipMovementRequest {
  productOwnershipId: number;
  movementType: string;
  quantityChange: number;
  weightChange: number;
  amountChange: number;
  referenceNumber?: string;
  notes?: string;
}

export interface OwnershipValidationResult {
  canSell: boolean;
  message: string;
  ownedQuantity: number;
  ownedWeight: number;
  totalQuantity: number;
  totalWeight: number;
  ownershipPercentage: number;
  warnings: string[];
}

export interface OwnershipAlertDto {
  type: string;
  message: string;
  severity: string;
  productId: number;
  productName: string;
  supplierId?: number;
  ownershipPercentage: number;
  outstandingAmount: number;
  createdAt: string;
}

export interface ConvertRawGoldRequest {
  rawGoldProductId: number;
  branchId: number;
  weightToConvert: number;
  quantityToConvert: number;
  newProducts: NewProductFromRawGold[];
}

export interface NewProductFromRawGold {
  productId: number;
  quantity: number;
  weight: number;
}

export interface ValidateOwnershipRequest {
  productId: number;
  branchId: number;
  requestedQuantity: number;
}

export interface UpdateOwnershipPaymentRequest {
  productOwnershipId: number;
  paymentAmount: number;
  referenceNumber: string;
}

export interface UpdateOwnershipSaleRequest {
  productId: number;
  branchId: number;
  soldQuantity: number;
  referenceNumber: string;
}

export interface ConvertRawGoldResponse {
  success: boolean;
  message: string;
}

// Missing interfaces for Repair Jobs and Technicians
export interface CreateRepairJobRequestDto {
  financialTransactionId: number;
  priorityId: number; // RepairPriority enum value
  assignedTechnicianId?: number;
  technicianNotes?: string;
}

export interface UpdateRepairJobStatusRequestDto {
  statusId: number; // RepairStatus enum value
  technicianNotes?: string;
  actualCost?: number;
  materialsUsed?: string;
  hoursSpent?: number;
  additionalPaymentAmount?: number;
  paymentMethodId?: number; // PaymentMethod enum value
}

export interface AssignTechnicianRequestDto {
  technicianId: number;
  technicianNotes?: string;
}

export interface CompleteRepairRequestDto {
  actualCost: number;
  technicianNotes?: string;
  materialsUsed?: string;
  hoursSpent?: number;
  additionalPaymentAmount?: number;
  paymentMethodId?: number; // PaymentMethod enum value
}

export interface DeliverRepairRequestDto {
  deliveryNotes?: string;
  customerNotified?: boolean;
  additionalPaymentAmount?: number;
  paymentMethodId?: number; // PaymentMethod enum value
}

export interface CancelRepairRequestDto {
  reason: string;
}

export interface RepairJobSearchRequestDto {
  branchId?: number;
  statusId?: number; // RepairStatus enum value
  priorityId?: number; // RepairPriority enum value
  assignedTechnicianId?: number;
  customerId?: number;
  transactionNumber?: string;
  fromDate?: string;
  toDate?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface RepairJobStatisticsDto {
  totalJobs: number;
  pendingJobs: number;
  inProgressJobs: number;
  completedJobs: number;
  readyForPickupJobs: number;
  deliveredJobs: number;
  cancelledJobs: number;
  totalRevenue: number;
  averageCompletionTime: number;
  jobsByPriority: { [key: string]: number };
  jobsByTechnician: { [key: string]: number };
}

export interface TechnicianDto {
  id: number;
  fullName: string;
  phoneNumber: string;
  email?: string;
  specialization?: string;
  isActive: boolean;
  branchId: number;
  createdAt: string;
  createdBy: string;
  createdByName: string;
}

export interface CreateTechnicianRequestDto {
  fullName: string;
  phoneNumber: string;
  email?: string;
  specialization?: string;
  branchId: number;
}

export interface UpdateTechnicianRequestDto {
  fullName: string;
  phoneNumber: string;
  email?: string;
  specialization?: string;
  isActive: boolean;
  branchId: number;
}

export interface TechnicianSearchRequestDto {
  searchTerm?: string;
  isActive?: boolean;
  branchId?: number;
  pageNumber?: number;
  pageSize?: number;
}



// Customers API
export const customersApi = {
  async getCustomers(params: CustomerSearchRequest = {}): Promise<PagedResult<Customer>> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<PagedResult<Customer>>(`/customers?${queryString}`);
    
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

  async createCustomer(customer: CreateCustomerRequest): Promise<Customer> {
    const response = await apiRequest<Customer>('/customers', {
      method: 'POST',
      body: JSON.stringify(customer),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to create customer');
  },

  async updateCustomer(id: number, customer: UpdateCustomerRequest): Promise<Customer> {
    const response = await apiRequest<Customer>(`/customers/${id}`, {
      method: 'PUT',
      body: JSON.stringify(customer),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to update customer');
  },

  async deleteCustomer(id: number): Promise<void> {
    const response = await apiRequest(`/customers/${id}`, {
      method: 'DELETE',
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to delete customer');
    }
  },

  async getCustomerOrders(id: number, params: {
    fromDate?: string;
    toDate?: string;
  } = {}): Promise<CustomerOrdersHistoryDto> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<CustomerOrdersHistoryDto>(`/customers/${id}/Orders?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch customer orders');
  },

  async getCustomerLoyalty(id: number): Promise<CustomerLoyaltyDto> {
    const response = await apiRequest<CustomerLoyaltyDto>(`/customers/${id}/loyalty`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch customer loyalty');
  },

  async updateCustomerLoyalty(id: number, request: UpdateCustomerLoyaltyRequest): Promise<CustomerLoyaltyDto> {
    const response = await apiRequest<CustomerLoyaltyDto>(`/customers/${id}/loyalty`, {
      method: 'PUT',
      body: JSON.stringify(request),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to update customer loyalty');
  },

  async searchCustomers(params: {
    searchTerm: string;
    limit?: number;
  }): Promise<Customer[]> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<Customer[]>(`/customers/search?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to search customers');
  }
};

export const lookupsApi = {

  async getTransactionTypes(): Promise<FinancialTransactionTypeLookupDto[]> {
    const response = await apiRequest<FinancialTransactionTypeLookupDto[]>('/lookups/transaction-types');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch transaction types');
  },

  async getPaymentMethods(): Promise<PaymentMethodLookupDto[]> {
    const response = await apiRequest<PaymentMethodLookupDto[]>('/lookups/payment-methods');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch payment methods');
  },

  async getTransactionStatuses(): Promise<FinancialTransactionStatusLookupDto[]> {
    const response = await apiRequest<FinancialTransactionStatusLookupDto[]>('/lookups/transaction-statuses');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch transaction statuses');
  },

  async getKaratTypes(): Promise<KaratTypeLookupDto[]> {
    const response = await apiRequest<KaratTypeLookupDto[]>('/lookups/karat-types');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch karat types');
  },

  async getProductCategoryTypes(): Promise<ProductCategoryTypeLookupDto[]> {
    const response = await apiRequest<ProductCategoryTypeLookupDto[]>('/lookups/product-category-types');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch product category types');
  },

  async getChargeTypes(): Promise<ChargeTypeLookupDto[]> {
    const response = await apiRequest<ChargeTypeLookupDto[]>('/lookups/charge-types');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch charge types');
  },

  async getRepairStatuses(): Promise<RepairStatusLookupDto[]> {
    const response = await apiRequest<RepairStatusLookupDto[]>('/lookups/repair-statuses');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch repair statuses');
  },

  async getRepairPriorities(): Promise<RepairPriorityLookupDto[]> {
    const response = await apiRequest<RepairPriorityLookupDto[]>('/lookups/repair-priorities');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch repair priorities');
  },

  async getOrderTypes(): Promise<OrderTypeLookupDto[]> {
    const response = await apiRequest<OrderTypeLookupDto[]>('/lookups/order-types');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch order types');
  },

  async getOrderStatuses(): Promise<OrderStatusLookupDto[]> {
    const response = await apiRequest<OrderStatusLookupDto[]>('/lookups/order-statuses');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch order statuses');
  },

  async getBusinessEntityTypes(): Promise<BusinessEntityTypeLookupDto[]> {
    const response = await apiRequest<BusinessEntityTypeLookupDto[]>('/lookups/business-entity-types');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch business entity types');
  },

  async getSubCategories(categoryId: number): Promise<SubCategoryLookupDto[]> {
    const response = await apiRequest<SubCategoryLookupDto[]>(`/lookups/sub-categories/${categoryId}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch sub-categories');
  },

  async getTaxConfigurations(): Promise<TaxConfigurationDto[]> {
    const response = await apiRequest<TaxConfigurationDto[]>('/pricing/taxes');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch tax configurations');
  }
};

export const repairJobsApi = {
  async createRepairJob(request: CreateRepairJobRequestDto): Promise<RepairJobDto> {
    const response = await apiRequest<RepairJobDto>('/repairjobs', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to create repair job');
  },

  async getRepairJob(id: number): Promise<RepairJobDto> {
    const response = await apiRequest<RepairJobDto>(`/repairjobs/${id}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch repair job');
  },

  async getRepairJobByFinancialTransactionId(financialTransactionId: number): Promise<RepairJobDto> {
    const response = await apiRequest<RepairJobDto>(`/repairjobs/by-financial-transaction/${financialTransactionId}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch repair job');
  },

  async updateRepairJobStatus(id: number, request: UpdateRepairJobStatusRequestDto): Promise<void> {
    const response = await apiRequest(`/repairjobs/${id}/status`, {
      method: 'PUT',
      body: JSON.stringify(request),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to update repair job status');
    }
  },

  async assignTechnician(id: number, request: AssignTechnicianRequestDto): Promise<void> {
    const response = await apiRequest(`/repairjobs/${id}/assign`, {
      method: 'PUT',
      body: JSON.stringify(request),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to assign technician');
    }
  },

  async completeRepair(id: number, request: CompleteRepairRequestDto): Promise<void> {
    const response = await apiRequest(`/repairjobs/${id}/complete`, {
      method: 'PUT',
      body: JSON.stringify(request),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to complete repair');
    }
  },

  async markReadyForPickup(id: number): Promise<void> {
    const response = await apiRequest(`/repairjobs/${id}/ready-for-pickup`, {
      method: 'PUT',
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to mark repair ready for pickup');
    }
  },

  async deliverRepair(id: number, request: DeliverRepairRequestDto): Promise<void> {
    const response = await apiRequest(`/repairjobs/${id}/deliver`, {
      method: 'PUT',
      body: JSON.stringify(request),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to deliver repair');
    }
  },

  async cancelRepair(id: number, request: CancelRepairRequestDto): Promise<void> {
    const response = await apiRequest(`/repairjobs/${id}/cancel`, {
      method: 'PUT',
      body: JSON.stringify(request),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to cancel repair');
    }
  },

  async searchRepairJobs(params: RepairJobSearchRequestDto = {}): Promise<{
    items: RepairJobDto[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
  }> {
    const queryString = new URLSearchParams(
      Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .map(([key, value]) => [key, String(value)])
    ).toString();
    
    const response = await apiRequest<{
      items: RepairJobDto[];
      totalCount: number;
      pageNumber: number;
      pageSize: number;
      totalPages: number;
    }>(`/repairjobs/search?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to search repair jobs');
  },

  async getRepairJobStatistics(branchId?: number, fromDate?: string, toDate?: string): Promise<RepairJobStatisticsDto> {
    const params: Record<string, string> = {};
    
    if (branchId) {
      params.branchId = branchId.toString();
    }
    
    if (fromDate) {
      params.fromDate = fromDate;
    }
    
    if (toDate) {
      params.toDate = toDate;
    }
    
    const queryString = Object.keys(params).length > 0 
      ? new URLSearchParams(params).toString()
      : '';
    
    const response = await apiRequest<RepairJobStatisticsDto>(`/repairjobs/statistics${queryString ? `?${queryString}` : ''}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch repair job statistics');
  },

  async getRepairJobsByStatus(statusId: number, branchId?: number): Promise<RepairJobDto[]> {
    const queryString = branchId 
      ? new URLSearchParams({ branchId: branchId.toString() }).toString()
      : '';
    
    const response = await apiRequest<RepairJobDto[]>(`/repairjobs/by-status/${statusId}${queryString ? `?${queryString}` : ''}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch repair jobs by status');
  },

  async getRepairJobsByTechnician(technicianId: number, branchId?: number): Promise<RepairJobDto[]> {
    const queryString = branchId 
      ? new URLSearchParams({ branchId: branchId.toString() }).toString()
      : '';
    
    const response = await apiRequest<RepairJobDto[]>(`/repairjobs/by-technician/${technicianId}${queryString ? `?${queryString}` : ''}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch repair jobs by technician');
  },

  async getOverdueRepairJobs(branchId?: number): Promise<RepairJobDto[]> {
    const queryString = branchId 
      ? new URLSearchParams({ branchId: branchId.toString() }).toString()
      : '';
    
    const response = await apiRequest<RepairJobDto[]>(`/repairjobs/overdue${queryString ? `?${queryString}` : ''}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch overdue repair jobs');
  },

  async getRepairJobsDueToday(branchId?: number): Promise<RepairJobDto[]> {
    const queryString = branchId 
      ? new URLSearchParams({ branchId: branchId.toString() }).toString()
      : '';
    
    const response = await apiRequest<RepairJobDto[]>(`/repairjobs/due-today${queryString ? `?${queryString}` : ''}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch repair jobs due today');
  }
};

export const techniciansApi = {
  async createTechnician(request: CreateTechnicianRequestDto): Promise<TechnicianDto> {
    const response = await apiRequest<TechnicianDto>('/technicians', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to create technician');
  },

  async getTechnician(id: number): Promise<TechnicianDto> {
    const response = await apiRequest<TechnicianDto>(`/technicians/${id}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch technician');
  },

  async updateTechnician(id: number, request: UpdateTechnicianRequestDto): Promise<void> {
    const response = await apiRequest(`/technicians/${id}`, {
      method: 'PUT',
      body: JSON.stringify(request),
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to update technician');
    }
  },

  async deleteTechnician(id: number): Promise<void> {
    const response = await apiRequest(`/technicians/${id}`, {
      method: 'DELETE',
    });
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to delete technician');
    }
  },

  async searchTechnicians(params: TechnicianSearchRequestDto = {}): Promise<{
    items: TechnicianDto[];
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
      items: TechnicianDto[];
      totalCount: number;
      page: number;
      pageSize: number;
      totalPages: number;
    }>(`/technicians/search?${queryString}`);
    
    if (response.success && response.data) {
      // Map backend response to frontend expected format
      return {
        items: response.data.items,
        totalCount: response.data.totalCount,
        pageNumber: response.data.page,
        pageSize: response.data.pageSize
      };
    }
    
    throw new Error(response.message || 'Failed to search technicians');
  },

  async getActiveTechnicians(branchId?: number): Promise<TechnicianDto[]> {
    const queryString = branchId 
      ? new URLSearchParams({ branchId: branchId.toString() }).toString()
      : '';
    
    const response = await apiRequest<TechnicianDto[]>(`/technicians/active${queryString ? `?${queryString}` : ''}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch active technicians');
  },

  async getTechniciansByBranch(branchId: number): Promise<TechnicianDto[]> {
    const response = await apiRequest<TechnicianDto[]>(`/technicians/by-branch/${branchId}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch technicians by branch');
  }
};

// Helper functions for data conversion between FinancialTransactionDto and Transaction
// Note: These functions already exist above, so we're just adding the missing ones here



// ProductOwnership API
export const productOwnershipApi = {
  async createOrUpdateOwnership(request: ProductOwnershipRequest): Promise<ProductOwnershipDto> {
    const response = await apiRequest<ProductOwnershipDto>('/productownership', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to create/update product ownership');
  },

  async getProductOwnershipList(branchId: number, params: {
    searchTerm?: string;
    supplierId?: number;
    pageNumber?: number;
    pageSize?: number;
  } = {}): Promise<{
    items: ProductOwnershipDto[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
  }> {
    const queryParams = new URLSearchParams({
      branchId: branchId.toString(),
      ...Object.entries(params)
        .filter(([_, value]) => value !== undefined)
        .reduce((acc, [key, value]) => ({ ...acc, [key]: String(value) }), {})
    }).toString();
    
    const response = await apiRequest<{
      items: ProductOwnershipDto[];
      totalCount: number;
      pageNumber: number;
      pageSize: number;
      totalPages: number;
    }>(`/productownership/list?${queryParams}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch product ownership list');
  },

  async validateOwnership(request: ValidateOwnershipRequest): Promise<OwnershipValidationResult> {
    const response = await apiRequest<OwnershipValidationResult>('/productownership/validate', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to validate ownership');
  },

  async updateOwnershipAfterPayment(request: UpdateOwnershipPaymentRequest): Promise<boolean> {
    const response = await apiRequest<boolean>('/productownership/payment', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    
    if (response.success) {
      return true;
    }
    
    throw new Error(response.message || 'Failed to update ownership after payment');
  },

  async updateOwnershipAfterSale(request: UpdateOwnershipSaleRequest): Promise<boolean> {
    const response = await apiRequest<boolean>('/productownership/sale', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    
    if (response.success) {
      return true;
    }
    
    throw new Error(response.message || 'Failed to update ownership after sale');
  },

  async convertRawGoldToProducts(request: ConvertRawGoldRequest): Promise<ConvertRawGoldResponse> {
    const response = await apiRequest<ConvertRawGoldResponse>('/productownership/convert-raw-gold', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to convert raw gold to products');
  },

  async getOwnershipAlerts(branchId?: number): Promise<OwnershipAlertDto[]> {
    const queryString = branchId 
      ? new URLSearchParams({ branchId: branchId.toString() }).toString()
      : '';
    
    const response = await apiRequest<OwnershipAlertDto[]>(`/productownership/alerts${queryString ? `?${queryString}` : ''}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch ownership alerts');
  },

  async getProductOwnership(productId: number, branchId: number): Promise<ProductOwnershipDto[]> {
    const response = await apiRequest<ProductOwnershipDto[]>(`/productownership/product/${productId}/branch/${branchId}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch product ownership');
  },

  async getOwnershipMovements(productOwnershipId: number): Promise<OwnershipMovementDto[]> {
    const response = await apiRequest<OwnershipMovementDto[]>(`/productownership/movements/${productOwnershipId}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch ownership movements');
  },

  async getLowOwnershipProducts(threshold: number = 0.5): Promise<ProductOwnershipDto[]> {
    const queryString = new URLSearchParams({
      threshold: threshold.toString()
    }).toString();
    
    const response = await apiRequest<ProductOwnershipDto[]>(`/productownership/low-ownership?${queryString}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch low ownership products');
  },

  async getProductsWithOutstandingPayments(): Promise<ProductOwnershipDto[]> {
    const response = await apiRequest<ProductOwnershipDto[]>('/productownership/outstanding-payments');
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to fetch products with outstanding payments');
  }
};

// Additional helper functions for data conversion
export const mapTransactionTypeStringToId = (type: string): number => {
  switch (type.toLowerCase()) {
    case 'sale': return 1;
    case 'return': return 2;
    case 'repair': return 3;
    default: return 1;
  }
};

export const mapStatusStringToId = (status: string): number => {
  switch (status.toLowerCase()) {
    case 'completed': return 1;
    case 'pending': return 2;
    case 'cancelled': return 3;
    case 'voided': return 4;
    default: return 1;
  }
};

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
  customers: customersApi,
  lookups: lookupsApi,
  repairJobs: repairJobsApi,
  technicians: techniciansApi,
  orders: ordersApi,
  financialTransactions: financialTransactionsApi,
  productOwnership: productOwnershipApi,
  cashDrawer: cashDrawerApi,
};

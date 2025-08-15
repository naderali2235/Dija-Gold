/**
 * API Service for DijaGold POS System
 * Handles all communication with the backend API
 */

import { API_CONFIG, STORAGE_KEYS } from '../config/environment';

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
  }
};

// Products API
export interface Product {
  id: number;
  productCode: string;
  name: string;
  categoryType: 'Ring' | 'Chain' | 'Necklace' | 'Earrings' | 'Bangles' | 'Bracelet' | 'Bullion' | 'Coins' | 'Other';
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
  transactions: transactionsApi,
  customers: customersApi,
};

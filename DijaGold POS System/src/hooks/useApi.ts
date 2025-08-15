/**
 * React hooks for API integration
 * Provides convenient hooks for data fetching with loading states and error handling
 */

import { useState, useEffect, useCallback } from 'react';
import api, { ApiResponse } from '../services/api';

// Generic hook for API state management
export interface ApiState<T> {
  data: T | null;
  loading: boolean;
  error: string | null;
}

export function useApiState<T>(initialData: T | null = null): [
  ApiState<T>,
  {
    setData: (data: T | null) => void;
    setLoading: (loading: boolean) => void;
    setError: (error: string | null) => void;
    reset: () => void;
  }
] {
  const [state, setState] = useState<ApiState<T>>({
    data: initialData,
    loading: false,
    error: null,
  });

  const setData = useCallback((data: T | null) => {
    setState(prev => ({ ...prev, data, error: null }));
  }, []);

  const setLoading = useCallback((loading: boolean) => {
    setState(prev => ({ ...prev, loading }));
  }, []);

  const setError = useCallback((error: string | null) => {
    setState(prev => ({ ...prev, error, loading: false }));
  }, []);

  const reset = useCallback(() => {
    setState({ data: initialData, loading: false, error: null });
  }, [initialData]);

  return [state, { setData, setLoading, setError, reset }];
}

// Hook for async API calls
export function useApiCall<T, P extends any[] = []>(
  apiFunction: (...args: P) => Promise<T>
) {
  const [state, { setData, setLoading, setError, reset }] = useApiState<T>();

  const execute = useCallback(async (...args: P) => {
    try {
      setLoading(true);
      setError(null);
      const result = await apiFunction(...args);
      setData(result);
      return result;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'An error occurred';
      setError(errorMessage);
      throw error;
    } finally {
      setLoading(false);
    }
  }, [apiFunction, setData, setLoading, setError]);

  return {
    ...state,
    execute,
    reset,
  };
}

// Products hooks
export function useProducts() {
  return useApiCall(api.products.getProducts);
}

export function useProduct() {
  return useApiCall(api.products.getProduct);
}

export function useCreateProduct() {
  return useApiCall(api.products.createProduct);
}

export function useUpdateProduct() {
  return useApiCall(api.products.updateProduct);
}

export function useDeleteProduct() {
  return useApiCall(api.products.deleteProduct);
}

// Pricing hooks
export function useGoldRates() {
  const [state, { setData, setLoading, setError }] = useApiState<any[]>([]);

  const fetchRates = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const rates = await api.pricing.getGoldRates();
      setData(rates);
      return rates;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to fetch gold rates';
      setError(errorMessage);
      throw error;
    } finally {
      setLoading(false);
    }
  }, [setData, setLoading, setError]);

  const updateRates = useCallback(async (rates: Array<{
    karat: string;
    buyRate: number;
    sellRate: number;
  }>) => {
    try {
      setLoading(true);
      setError(null);
      await api.pricing.updateGoldRates(rates);
      // Refresh rates after update
      await fetchRates();
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to update gold rates';
      setError(errorMessage);
      throw error;
    } finally {
      setLoading(false);
    }
  }, [fetchRates, setLoading, setError]);

  // Auto-fetch on mount
  useEffect(() => {
    fetchRates();
  }, [fetchRates]);

  return {
    ...state,
    fetchRates,
    updateRates,
  };
}

// Transaction hooks
export function useProcessSale() {
  return useApiCall(api.transactions.processSale);
}

export function useTransaction() {
  return useApiCall(api.transactions.getTransaction);
}

export function useSearchTransactions() {
  return useApiCall(api.transactions.searchTransactions);
}

// Authentication hooks
export function useLogin() {
  return useApiCall(api.auth.login);
}

export function useCurrentUser() {
  const [state, { setData, setLoading, setError }] = useApiState<any>(null);

  const fetchUser = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const user = await api.auth.getCurrentUser();
      setData(user);
      return user;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to fetch user info';
      setError(errorMessage);
      // Don't throw here since this might be called during app initialization
      return null;
    } finally {
      setLoading(false);
    }
  }, [setData, setLoading, setError]);

  const logout = useCallback(async () => {
    try {
      await api.auth.logout();
      setData(null);
    } catch (error) {
      // Even if logout fails on the server, clear local state
      setData(null);
    }
  }, [setData]);

  return {
    ...state,
    fetchUser,
    logout,
  };
}

// Hook for paginated data
export function usePaginatedApi<T>(
  apiFunction: (params: any) => Promise<{
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
  }>,
  initialParams: any = {}
) {
  const [state, { setData, setLoading, setError }] = useApiState<{
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
  }>({ items: [], totalCount: 0, page: 1, pageSize: 20 });

  const [params, setParams] = useState(initialParams);

  const fetchData = useCallback(async (newParams?: any) => {
    try {
      setLoading(true);
      setError(null);
      const finalParams = newParams || params;
      const result = await apiFunction(finalParams);
      setData(result);
      return result;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'An error occurred';
      setError(errorMessage);
      throw error;
    } finally {
      setLoading(false);
    }
  }, [apiFunction, params, setData, setLoading, setError]);

  const updateParams = useCallback((newParams: any) => {
    setParams({ ...params, ...newParams });
  }, [params]);

  const nextPage = useCallback(() => {
    const newPage = (state.data?.page || 1) + 1;
    updateParams({ page: newPage });
  }, [state.data?.page, updateParams]);

  const prevPage = useCallback(() => {
    const newPage = Math.max((state.data?.page || 1) - 1, 1);
    updateParams({ page: newPage });
  }, [state.data?.page, updateParams]);

  const setPage = useCallback((page: number) => {
    updateParams({ page: Math.max(page, 1) });
  }, [updateParams]);

  // Fetch data when params change
  useEffect(() => {
    fetchData(params);
  }, [params]);

  return {
    ...state,
    params,
    fetchData,
    updateParams,
    nextPage,
    prevPage,
    setPage,
    hasNextPage: state.data ? (state.data.page * state.data.pageSize) < state.data.totalCount : false,
    hasPrevPage: state.data ? state.data.page > 1 : false,
  };
}

// Customer hooks
export function useCustomers() {
  return useApiCall(api.customers.getCustomers);
}

export function useCustomer() {
  return useApiCall(api.customers.getCustomer);
}

export function useCreateCustomer() {
  return useApiCall(api.customers.createCustomer);
}

export default {
  useApiState,
  useApiCall,
  useProducts,
  useProduct,
  useCreateProduct,
  useUpdateProduct,
  useDeleteProduct,
  useGoldRates,
  useProcessSale,
  useTransaction,
  useSearchTransactions,
  useLogin,
  useCurrentUser,
  usePaginatedApi,
  useCustomers,
  useCustomer,
  useCreateCustomer,
};

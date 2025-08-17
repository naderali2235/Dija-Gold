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
    pageNumber: number;
    pageSize: number;
    totalPages: number;
  }>,
  initialParams: any = {}
) {
  const [state, { setData, setLoading, setError }] = useApiState<{
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
  }>({ items: [], totalCount: 0, pageNumber: 1, pageSize: 20, totalPages: 0 });

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
    const newPage = (state.data?.pageNumber || 1) + 1;
    updateParams({ pageNumber: newPage });
  }, [state.data?.pageNumber, updateParams]);

  const prevPage = useCallback(() => {
    const newPage = Math.max((state.data?.pageNumber || 1) - 1, 1);
    updateParams({ pageNumber: newPage });
  }, [state.data?.pageNumber, updateParams]);

  const setPage = useCallback((page: number) => {
    updateParams({ pageNumber: Math.max(page, 1) });
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
    hasNextPage: state.data ? (state.data.pageNumber * state.data.pageSize) < state.data.totalCount : false,
    hasPrevPage: state.data ? state.data.pageNumber > 1 : false,
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

export function useUpdateCustomer() {
  return useApiCall(api.customers.updateCustomer);
}

// Users Management hooks
export function useUsers() {
  return useApiCall(api.users.getUsers);
}

export function useUser() {
  return useApiCall(api.users.getUser);
}

export function useCreateUser() {
  return useApiCall(api.users.createUser);
}

export function useUpdateUser() {
  return useApiCall(api.users.updateUser);
}

export function useUpdateUserRoles() {
  return useApiCall(api.users.updateUserRoles);
}

export function useUpdateUserStatus() {
  return useApiCall(api.users.updateUserStatus);
}

export function useUserActivity() {
  return useApiCall(api.users.getUserActivity);
}

export function useResetPassword() {
  return useApiCall(api.users.resetPassword);
}

export function useUserPermissions() {
  return useApiCall(api.users.getUserPermissions);
}

export function useUpdateUserPermissions() {
  return useApiCall(api.users.updateUserPermissions);
}

// Specialized Users hook with pagination
export function usePaginatedUsers(initialParams: any = {}) {
  return usePaginatedApi(api.users.getUsers, {
    pageNumber: 1,
    pageSize: 20,
    isActive: true,
    ...initialParams
  });
}

// Enhanced Auth hooks
export function useRefreshToken() {
  return useApiCall(api.auth.refreshToken);
}

// Branches Management hooks
export function useBranches() {
  return useApiCall(api.branches.getBranches);
}

export function useBranch() {
  return useApiCall(api.branches.getBranch);
}

export function useCreateBranch() {
  return useApiCall(api.branches.createBranch);
}

export function useUpdateBranch() {
  return useApiCall(api.branches.updateBranch);
}

export function useDeleteBranch() {
  return useApiCall(api.branches.deleteBranch);
}

export function useBranchStaff() {
  return useApiCall(api.branches.getBranchStaff);
}

export function useBranchPerformance() {
  return useApiCall(api.branches.getBranchPerformance);
}

export function useBranchTransactions() {
  return useApiCall(api.branches.getBranchTransactions);
}

// Specialized Branches hook with pagination
export function usePaginatedBranches(initialParams: any = {}) {
  return usePaginatedApi(api.branches.getBranches, {
    pageNumber: 1,
    pageSize: 20,
    isActive: true,
    ...initialParams
  });
}

// Suppliers Management hooks
export function useSuppliers() {
  return useApiCall(api.suppliers.getSuppliers);
}

export function useSupplier() {
  return useApiCall(api.suppliers.getSupplier);
}

export function useCreateSupplier() {
  return useApiCall(api.suppliers.createSupplier);
}

export function useUpdateSupplier() {
  return useApiCall(api.suppliers.updateSupplier);
}

export function useDeleteSupplier() {
  return useApiCall(api.suppliers.deleteSupplier);
}

export function useSupplierProducts() {
  return useApiCall(api.suppliers.getSupplierProducts);
}

export function useSupplierBalance() {
  return useApiCall(api.suppliers.getSupplierBalance);
}

export function useUpdateSupplierBalance() {
  return useApiCall(api.suppliers.updateSupplierBalance);
}

export function useSupplierTransactions() {
  return useApiCall(api.suppliers.getSupplierTransactions);
}

// Specialized Suppliers hook with pagination
export function usePaginatedSuppliers(initialParams: any = {}) {
  return usePaginatedApi(api.suppliers.getSuppliers, {
    pageNumber: 1,
    pageSize: 20,
    isActive: true,
    ...initialParams
  });
}

// Inventory Management hooks
export function useInventory() {
  return useApiCall(api.inventory.getInventory);
}

export function useBranchInventory() {
  return useApiCall(api.inventory.getBranchInventory);
}

export function useLowStockItems() {
  return useApiCall(api.inventory.getLowStockItems);
}

export function useCheckStockAvailability() {
  return useApiCall(api.inventory.checkStockAvailability);
}

export function useAddInventory() {
  return useApiCall(api.inventory.addInventory);
}

export function useAdjustInventory() {
  return useApiCall(api.inventory.adjustInventory);
}

export function useTransferInventory() {
  return useApiCall(api.inventory.transferInventory);
}

export function useInventoryMovements() {
  return useApiCall(api.inventory.getInventoryMovements);
}

// Specialized Inventory hook with pagination
export function usePaginatedInventoryMovements(initialParams: any = {}) {
  return usePaginatedApi(api.inventory.getInventoryMovements, {
    pageNumber: 1,
    pageSize: 20,
    ...initialParams
  });
}

// Reports Management hooks
export function useDailySalesSummary() {
  return useApiCall(api.reports.getDailySalesSummary);
}

export function useCashReconciliation() {
  return useApiCall(api.reports.getCashReconciliation);
}

export function useInventoryMovementReport() {
  return useApiCall(api.reports.getInventoryMovementReport);
}

export function useProfitAnalysisReport() {
  return useApiCall(api.reports.getProfitAnalysisReport);
}

export function useCustomerAnalysisReport() {
  return useApiCall(api.reports.getCustomerAnalysisReport);
}

export function useSupplierBalanceReport() {
  return useApiCall(api.reports.getSupplierBalanceReport);
}

export function useInventoryValuationReport() {
  return useApiCall(api.reports.getInventoryValuationReport);
}

export function useTaxReport() {
  return useApiCall(api.reports.getTaxReport);
}

export function useTransactionLogReport() {
  return useApiCall(api.reports.getTransactionLogReport);
}

export function useReportTypes() {
  return useApiCall(api.reports.getReportTypes);
}

export function useExportToExcel() {
  return useApiCall(api.reports.exportToExcel);
}

export function useExportToPdf() {
  return useApiCall(api.reports.exportToPdf);
}

// Labels/Printing Management hooks
export function useGenerateProductLabelZpl() {
  return useApiCall(api.labels.generateProductLabelZpl);
}

export function usePrintProductLabel() {
  return useApiCall(api.labels.printProductLabel);
}

export function useDecodeQrPayload() {
  return useApiCall(api.labels.decodeQrPayload);
}

// Lookups API hooks
export function useAllLookups() {
  return useApiCall(api.lookups.getAllLookups);
}

export function useTransactionTypes() {
  return useApiCall(api.lookups.getTransactionTypes);
}

export function usePaymentMethods() {
  return useApiCall(api.lookups.getPaymentMethods);
}

export function useTransactionStatuses() {
  return useApiCall(api.lookups.getTransactionStatuses);
}

export function useKaratTypes() {
  return useApiCall(api.lookups.getKaratTypes);
}

export function useProductCategoryTypes() {
  return useApiCall(api.lookups.getProductCategoryTypes);
}

export function useChargeTypes() {
  return useApiCall(api.lookups.getChargeTypes);
}

// Purchase Orders hooks
export function useCreatePurchaseOrder() {
  return useApiCall(api.purchaseOrders.createPurchaseOrder);
}

export function usePurchaseOrder() {
  return useApiCall(api.purchaseOrders.getPurchaseOrder);
}

export function useSearchPurchaseOrders() {
  return useApiCall(api.purchaseOrders.searchPurchaseOrders);
}

export function useReceivePurchaseOrder() {
  return useApiCall(api.purchaseOrders.receivePurchaseOrder);
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
  useRefreshToken,
  usePaginatedApi,
  useCustomers,
  useCustomer,
  useCreateCustomer,
  useUsers,
  useUser,
  useCreateUser,
  useUpdateUser,
  useUpdateUserRoles,
  useUpdateUserStatus,
  useUserActivity,
  useResetPassword,
  useUserPermissions,
  useUpdateUserPermissions,
  usePaginatedUsers,
  useBranches,
  useBranch,
  useCreateBranch,
  useUpdateBranch,
  useDeleteBranch,
  useBranchInventory,
  useBranchStaff,
  useBranchPerformance,
  useBranchTransactions,
  usePaginatedBranches,
  useSuppliers,
  useSupplier,
  useCreateSupplier,
  useUpdateSupplier,
  useDeleteSupplier,
  useSupplierProducts,
  useSupplierBalance,
  useUpdateSupplierBalance,
  useSupplierTransactions,
  usePaginatedSuppliers,
  useInventory,
  useLowStockItems,
  useCheckStockAvailability,
  useAddInventory,
  useAdjustInventory,
  useTransferInventory,
  useInventoryMovements,
  usePaginatedInventoryMovements,
  useDailySalesSummary,
  useCashReconciliation,
  useInventoryMovementReport,
  useProfitAnalysisReport,
  useCustomerAnalysisReport,
  useSupplierBalanceReport,
  useInventoryValuationReport,
  useTaxReport,
  useTransactionLogReport,
  useReportTypes,
  useExportToExcel,
  useExportToPdf,
  useGenerateProductLabelZpl,
  usePrintProductLabel,
  useDecodeQrPayload,
  useAllLookups,
  useTransactionTypes,
  usePaymentMethods,
  useTransactionStatuses,
  useKaratTypes,
  useProductCategoryTypes,
  useChargeTypes,
};

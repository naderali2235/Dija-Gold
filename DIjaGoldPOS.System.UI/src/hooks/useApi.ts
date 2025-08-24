/**
 * React hooks for API integration
 * Provides convenient hooks for data fetching with loading states and error handling
 */

import { useState, useEffect, useCallback, useRef } from 'react';
import api, { 
  ApiResponse, 
  GoldRate, 
  MakingCharges, 
  TaxConfigurationDto,
  // Lookup DTOs for navigation properties
  BaseLookupDto,
  ProductCategoryTypeLookupDto,
  KaratTypeLookupDto,
  SubCategoryLookupDto,
  FinancialTransactionTypeLookupDto,
  PaymentMethodLookupDto,
  FinancialTransactionStatusLookupDto,
  ChargeTypeLookupDto,
  RepairStatusLookupDto,
  RepairPriorityLookupDto,
  OrderTypeLookupDto,
  OrderStatusLookupDto,
  BusinessEntityTypeLookupDto
} from '../services/api';
import { EnumLookupDto } from '../types/lookups';

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

export function useProductPricing() {
  return useApiCall(api.products.getProductPricing);
}

// Specialized Products hook with pagination
export function usePaginatedProducts(initialParams: any = {}) {
  const wrapperFunction = async (params: any) => {
    const result = await api.products.getProducts(params);
    return {
      ...result,
      totalPages: Math.ceil(result.totalCount / result.pageSize)
    };
  };
  
  return usePaginatedApi(wrapperFunction, {
    pageNumber: 1,
    pageSize: 20,
    isActive: true,
    ...initialParams
  });
}

// Pricing hooks
export function useGoldRates() {
  const [state, { setData, setLoading, setError }] = useApiState<GoldRate[]>([]);

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
    karatTypeId: number;
    ratePerGram: number;
    effectiveFrom: string;
  }>) => {
    try {
      setLoading(true);
      setError(null);
      await api.pricing.updateGoldRates(rates);
      // Don't auto-refresh here - let the calling component handle refresh
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to update gold rates';
      setError(errorMessage);
      throw error;
    } finally {
      setLoading(false);
    }
  }, [setLoading, setError]);

  // Don't auto-fetch on mount - only fetch when explicitly called

  return {
    ...state,
    fetchRates,
    updateRates,
  };
}

export function useMakingCharges() {
  const [state, { setData, setLoading, setError }] = useApiState<MakingCharges[]>([]);

  const fetchCharges = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const charges = await api.pricing.getMakingCharges();
      setData(charges);
      return charges;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to fetch making charges';
      setError(errorMessage);
      throw error;
    } finally {
      setLoading(false);
    }
  }, [setData, setLoading, setError]);

  const updateCharges = useCallback(async (charges: {
    id?: number;
    name: string;
    productCategory: number;
    subCategory?: string;
    chargeType: number;
    chargeValue: number;
    effectiveFrom: string;
  }) => {
    try {
      setLoading(true);
      setError(null);
      await api.pricing.updateMakingCharges(charges);
      // Don't auto-refresh here - let the calling component handle refresh
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to update making charges';
      setError(errorMessage);
      throw error;
    } finally {
      setLoading(false);
    }
  }, [setLoading, setError]);

  // Don't auto-fetch on mount - only fetch when explicitly called

  return {
    ...state,
    fetchCharges,
    updateCharges,
  };
}

export function useTaxConfigurations() {
  const [state, { setData, setLoading, setError }] = useApiState<TaxConfigurationDto[]>([]);

  const fetchTaxConfigurations = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const taxConfigurations = await api.pricing.getTaxConfigurations();
      setData(taxConfigurations);
      return taxConfigurations;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to fetch tax configurations';
      setError(errorMessage);
      throw error;
    } finally {
      setLoading(false);
    }
  }, [setData, setLoading, setError]);

  const updateTaxConfiguration = useCallback(async (taxConfig: {
    id?: number;
    taxName: string;
    taxCode: string;
    taxType: number;
    taxRate: number;
    isMandatory: boolean;
    effectiveFrom: string;
    displayOrder: number;
  }) => {
    try {
      setLoading(true);
      setError(null);
      await api.pricing.updateTaxConfiguration(taxConfig);
      // Don't auto-refresh here - let the calling component handle refresh
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to update tax configuration';
      setError(errorMessage);
      throw error;
    } finally {
      setLoading(false);
    }
  }, [setLoading, setError]);

  // Don't auto-fetch on mount - only fetch when explicitly called

  return {
    ...state,
    fetchTaxConfigurations,
    updateTaxConfiguration,
  };
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
  const paramsRef = useRef(params);
  
  // Keep ref in sync with params
  useEffect(() => {
    paramsRef.current = params;
  }, [params]);

  const fetchData = useCallback(async (newParams?: any) => {
    try {
      setLoading(true);
      setError(null);
      const finalParams = newParams || paramsRef.current;
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
  }, [apiFunction, setData, setLoading, setError]);

  const updateParams = useCallback((newParams: any) => {
    setParams((prevParams: any) => ({ ...prevParams, ...newParams }));
  }, []);

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
    fetchData();
  }, [params]); // fetchData is stable, only depend on params

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

export function useDeleteCustomer() {
  return useApiCall(api.customers.deleteCustomer);
}

export function useGetCustomerOrders() {
  return useApiCall(api.customers.getCustomerOrders);
}

export function useCustomerLoyalty() {
  return useApiCall(api.customers.getCustomerLoyalty);
}

export function useUpdateCustomerLoyalty() {
  return useApiCall(api.customers.updateCustomerLoyalty);
}

export function useSearchCustomers() {
  return useApiCall(api.customers.searchCustomers);
}

// Specialized Customers hook with pagination
export function usePaginatedCustomers(initialParams: any = {}) {
  return usePaginatedApi(api.customers.getCustomers, {
    pageNumber: 1,
    pageSize: 20,
    isActive: true,
    ...initialParams
  });
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
  const [state, { setData, setLoading, setError }] = useApiState<KaratTypeLookupDto[]>([]);

  const fetchKaratTypes = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const karatTypes = await api.lookups.getKaratTypes();
      setData(karatTypes);
      return karatTypes;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to fetch karat types';
      setError(errorMessage);
      throw error;
    } finally {
      setLoading(false);
    }
  }, [setData, setLoading, setError]);

  // Don't auto-fetch on mount - only fetch when explicitly called

  return {
    ...state,
    fetchKaratTypes,
  };
}

export function useProductCategoryTypes() {
  const [state, { setData, setLoading, setError }] = useApiState<ProductCategoryTypeLookupDto[]>([]);

  const fetchCategories = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const categories = await api.lookups.getProductCategoryTypes();
      setData(categories);
      return categories;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to fetch product category types';
      setError(errorMessage);
      throw error;
    } finally {
      setLoading(false);
    }
  }, [setData, setLoading, setError]);

  // Don't auto-fetch on mount - only fetch when explicitly called

  return {
    ...state,
    fetchCategories,
  };
}

export function useChargeTypes() {
  const [state, { setData, setLoading, setError }] = useApiState<ChargeTypeLookupDto[]>([]);

  const fetchChargeTypes = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const chargeTypes = await api.lookups.getChargeTypes();
      setData(chargeTypes);
      return chargeTypes;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to fetch charge types';
      setError(errorMessage);
      throw error;
    } finally {
      setLoading(false);
    }
  }, [setData, setLoading, setError]);

  // Don't auto-fetch on mount - only fetch when explicitly called

  return {
    ...state,
    fetchChargeTypes,
  };
}





// Purchase Orders hooks
export function useCreatePurchaseOrder() {
  return useApiCall(api.purchaseOrders.createPurchaseOrder);
}

export function usePurchaseOrder() {
  return useApiCall(api.purchaseOrders.getPurchaseOrder);
}

export function useSearchPurchaseOrders() {
  const wrapperFunction = async (params: any) => {
    const result = await api.purchaseOrders.searchPurchaseOrders(params);
    return {
      items: result.items,
      totalCount: result.total,
      pageNumber: params.pageNumber || 1,
      pageSize: params.pageSize || 20,
      totalPages: Math.ceil(result.total / (params.pageSize || 20))
    };
  };
  
  return useApiCall(wrapperFunction);
}

export function useReceivePurchaseOrder() {
  return useApiCall(api.purchaseOrders.receivePurchaseOrder);
}

// Orders hooks (new architecture)
export function useCreateSaleOrder() {
  return useApiCall(api.orders.createSaleOrder);
}

export function useCreateRepairOrder() {
  return useApiCall(api.orders.createRepairOrder);
}

export function useOrder() {
  return useApiCall(api.orders.getOrder);
}

export function useSearchOrders() {
  return useApiCall(api.orders.searchOrders);
}

export function useOrderSummary() {
  return useApiCall(api.orders.getOrderSummary);
}

export function useCustomerOrders() {
  return useApiCall(api.orders.getCustomerOrders);
}

export function useCashierOrders() {
  return useApiCall(api.orders.getCashierOrders);
}

export function useUpdateOrder() {
  return useApiCall(api.orders.updateOrder);
}

// Financial Transactions hooks (new architecture)
export function useFinancialTransaction() {
  return useApiCall(api.financialTransactions.getFinancialTransaction);
}

export function useSearchFinancialTransactions() {
  return useApiCall(api.financialTransactions.searchFinancialTransactions);
}

export function useVoidFinancialTransaction() {
  return useApiCall(api.financialTransactions.voidFinancialTransaction);
}

export function useMarkReceiptPrinted() {
  return useApiCall(api.financialTransactions.markReceiptPrinted);
}

export function useGenerateBrowserReceipt() {
  return useApiCall(api.financialTransactions.generateBrowserReceipt);
}

export default {
  useApiState,
  useApiCall,
  useProducts,
  useProduct,
  useCreateProduct,
  useUpdateProduct,
  useDeleteProduct,
  useProductPricing,
  usePaginatedProducts,
  useGoldRates,
  useMakingCharges,
  useTaxConfigurations,
  useCreateSaleOrder,
  useCreateRepairOrder,
  useOrder,
  useSearchOrders,
  useOrderSummary,
  useCustomerOrders,
  useCashierOrders,
  useUpdateOrder,
  useFinancialTransaction,
  useSearchFinancialTransactions,
  useVoidFinancialTransaction,
  useMarkReceiptPrinted,
  useGenerateBrowserReceipt,
  useLogin,
  useCurrentUser,
  useRefreshToken,
  usePaginatedApi,
  useCustomers,
  useCustomer,
  useCreateCustomer,
  useUpdateCustomer,
  useDeleteCustomer,
  useGetCustomerOrders,
  useCustomerLoyalty,
  useUpdateCustomerLoyalty,
  useSearchCustomers,
  usePaginatedCustomers,
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
  useTransactionTypes,
  usePaymentMethods,
  useTransactionStatuses,
  useKaratTypes,
  useProductCategoryTypes,
  useChargeTypes,
  useRepairStatuses,
  useRepairPriorities,
  // Repair Jobs hooks
  useCreateRepairJob,
  useRepairJob,
  useRepairJobByFinancialTransactionId,
  useUpdateRepairJobStatus,
  useAssignTechnician,
  useCompleteRepair,
  useMarkReadyForPickup,
  useDeliverRepair,
  useCancelRepair,
  useSearchRepairJobs,
  useRepairJobStatistics,
  useRepairJobsByStatus,
  useRepairJobsByTechnician,
  useOverdueRepairJobs,
  useRepairJobsDueToday,
  usePaginatedRepairJobs,
  // Technicians hooks
  useActiveTechnicians,
  useTechnician,
  useCreateTechnician,
  useUpdateTechnician,
  useDeleteTechnician,
  useSearchTechnicians,
};

// Repair Jobs hooks
export function useCreateRepairJob() {
  return useApiCall(api.repairJobs.createRepairJob);
}

export function useRepairJob() {
  return useApiCall(api.repairJobs.getRepairJob);
}

export function useRepairJobByFinancialTransactionId() {
  return useApiCall(api.repairJobs.getRepairJobByFinancialTransactionId);
}

export function useUpdateRepairJobStatus() {
  return useApiCall(api.repairJobs.updateRepairJobStatus);
}

export function useAssignTechnician() {
  return useApiCall(api.repairJobs.assignTechnician);
}

export function useCompleteRepair() {
  return useApiCall(api.repairJobs.completeRepair);
}

export function useMarkReadyForPickup() {
  return useApiCall(api.repairJobs.markReadyForPickup);
}

export function useDeliverRepair() {
  return useApiCall(api.repairJobs.deliverRepair);
}

export function useCancelRepair() {
  return useApiCall(api.repairJobs.cancelRepair);
}

export function useSearchRepairJobs() {
  return useApiCall(api.repairJobs.searchRepairJobs);
}

export function useRepairJobStatistics() {
  return useApiCall(api.repairJobs.getRepairJobStatistics);
}

export function useRepairJobsByStatus() {
  return useApiCall(api.repairJobs.getRepairJobsByStatus);
}

export function useRepairJobsByTechnician() {
  return useApiCall(api.repairJobs.getRepairJobsByTechnician);
}

export function useOverdueRepairJobs() {
  return useApiCall(api.repairJobs.getOverdueRepairJobs);
}

export function useRepairJobsDueToday() {
  return useApiCall(api.repairJobs.getRepairJobsDueToday);
}

// Specialized Repair Jobs hook with pagination
export function usePaginatedRepairJobs(initialParams: any = {}) {
  return usePaginatedApi(api.repairJobs.searchRepairJobs, {
    pageNumber: 1,
    pageSize: 20,
    ...initialParams
  });
}

// Technicians hooks
export function useActiveTechnicians() {
  return useApiCall(api.technicians.getActiveTechnicians);
}

export function useTechnician() {
  return useApiCall(api.technicians.getTechnician);
}

export function useCreateTechnician() {
  return useApiCall(api.technicians.createTechnician);
}

export function useUpdateTechnician() {
  return useApiCall(api.technicians.updateTechnician);
}

export function useDeleteTechnician() {
  return useApiCall(api.technicians.deleteTechnician);
}

export function useSearchTechnicians() {
  return useApiCall(api.technicians.searchTechnicians);
}

// Lookups hooks
export function useRepairStatuses() {
  return useApiCall(api.lookups.getRepairStatuses);
}

export function useRepairPriorities() {
  return useApiCall(api.lookups.getRepairPriorities);
}

// ProductOwnership hooks
export function useCreateOrUpdateOwnership() {
  return useApiCall(api.productOwnership.createOrUpdateOwnership);
}

export function useGetProductOwnershipList() {
  return useApiCall(api.productOwnership.getProductOwnershipList);
}

export function useValidateOwnership() {
  return useApiCall(api.productOwnership.validateOwnership);
}

export function useUpdateOwnershipAfterPayment() {
  return useApiCall(api.productOwnership.updateOwnershipAfterPayment);
}

export function useUpdateOwnershipAfterSale() {
  return useApiCall(api.productOwnership.updateOwnershipAfterSale);
}

export function useConvertRawGoldToProducts() {
  return useApiCall(api.productOwnership.convertRawGoldToProducts);
}

export function useGetOwnershipAlerts() {
  return useApiCall(api.productOwnership.getOwnershipAlerts);
}

export function useGetProductOwnership() {
  return useApiCall(api.productOwnership.getProductOwnership);
}

export function useGetOwnershipMovements() {
  return useApiCall(api.productOwnership.getOwnershipMovements);
}

export function useGetLowOwnershipProducts() {
  return useApiCall(api.productOwnership.getLowOwnershipProducts);
}

export function useGetProductsWithOutstandingPayments() {
  return useApiCall(api.productOwnership.getProductsWithOutstandingPayments);
}

// Specialized ProductOwnership hook with pagination
export function usePaginatedProductOwnership(branchId: number, initialParams: any = {}) {
  return usePaginatedApi(
    (params) => api.productOwnership.getProductOwnershipList(branchId, params),
    {
      pageNumber: 1,
      pageSize: 20,
      ...initialParams
    }
  );
}

// Cash Drawer Management hooks
export function useOpenCashDrawer() {
  return useApiCall(api.cashDrawer.openDrawer);
}

export function useCloseCashDrawer() {
  return useApiCall(api.cashDrawer.closeDrawer);
}

export function useGetCashDrawerBalance() {
  return useApiCall(api.cashDrawer.getBalance);
}

export function useGetCashDrawerOpeningBalance() {
  return useApiCall(api.cashDrawer.getOpeningBalance);
}

export function useIsCashDrawerOpen() {
  return useApiCall(api.cashDrawer.isDrawerOpen);
}

export function useGetCashDrawerBalances() {
  return useApiCall(api.cashDrawer.getBalances);
}

export function useSettleCashDrawerShift() {
  return useApiCall(api.cashDrawer.settleShift);
}

export function useRefreshCashDrawerBalance() {
  return useApiCall(api.cashDrawer.refreshExpectedClosingBalance);
}

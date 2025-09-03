// Customers hooks (modular facade) built on core hooks and consolidated API
import { useApiCall, usePaginatedApi } from './core';
import { customersApi, type Customer, type CustomerSearchRequest, type PagedResult } from '../../services/api';

export function useCustomers() {
  return useApiCall<PagedResult<Customer>, [CustomerSearchRequest | undefined]>(customersApi.getCustomers as any);
}

export function useSearchCustomers() {
  return useApiCall<Customer[], [CustomerSearchRequest | undefined]>(customersApi.searchCustomers as any);
}

// Optional paginated helper mirroring useApi.ts signature
export function usePaginatedCustomers(initialParams: CustomerSearchRequest = {}) {
  const wrapper = async (params: CustomerSearchRequest) => {
    const result = await customersApi.getCustomers(params);
    return {
      ...result,
      totalPages: Math.ceil(result.totalCount / result.pageSize),
    };
  };
  return usePaginatedApi(wrapper, { pageNumber: 1, pageSize: 20, isActive: true, ...initialParams });
}

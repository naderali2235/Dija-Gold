// Customer Purchase hooks (modular facade)
import { useApiCall, usePaginatedApi } from './core';
import {
  customerPurchaseApi,
  type CustomerPurchaseDto,
  type CustomerPurchaseSearchRequest,
  type CustomerPurchaseSummaryDto,
  type CreateCustomerPurchaseRequest,
} from '../../services/api/customerPurchase';

export function useCreateCustomerPurchase() {
  return useApiCall<CustomerPurchaseDto, [CreateCustomerPurchaseRequest]>(customerPurchaseApi.createPurchase);
}

export function useCustomerPurchase() {
  return useApiCall<CustomerPurchaseDto, [number]>(customerPurchaseApi.getPurchase);
}

export function useCustomerPurchaseByNumber() {
  return useApiCall<CustomerPurchaseDto, [string]>(customerPurchaseApi.getPurchaseByNumber);
}

export function useUpdateCustomerPurchasePayment() {
  return useApiCall<CustomerPurchaseDto, [number, number]>(customerPurchaseApi.updatePaymentStatus);
}

export function useCancelCustomerPurchase() {
  return useApiCall<boolean, [number]>(customerPurchaseApi.cancelPurchase);
}

export function useCustomerPurchaseSummary() {
  return useApiCall<CustomerPurchaseSummaryDto, [string, string, number?]>(customerPurchaseApi.getPurchaseSummary as any);
}

export function useCustomerPurchases() {
  return useApiCall<CustomerPurchaseDto[], [number]>(customerPurchaseApi.getPurchasesByCustomer);
}

export function useBranchPurchases() {
  return useApiCall<CustomerPurchaseDto[], [number]>(customerPurchaseApi.getPurchasesByBranch);
}

export function usePurchasesByDateRange() {
  return useApiCall<CustomerPurchaseDto[], [string, string]>((from: string, to: string) =>
    customerPurchaseApi.getPurchasesByDateRange(from, to)
  );
}

export function usePaginatedCustomerPurchases(initial: CustomerPurchaseSearchRequest = {}) {
  return usePaginatedApi(customerPurchaseApi.searchPurchases, {
    pageNumber: 1,
    pageSize: 20,
    ...initial,
  });
}

// Modular customers API shim that maps to the consolidated customersApi in services/api.ts
import { customersApi } from '../api';
export { customersApi };
export type {
  Customer,
  CustomerSearchRequest,
  CustomerOrdersHistoryDto,
  CustomerOrderDto,
  CustomerLoyaltyDto,
  UpdateCustomerLoyaltyRequest,
  CreateCustomerRequest,
  UpdateCustomerRequest,
  PagedResult,
} from '../api';

export default customersApi;

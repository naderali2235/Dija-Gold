// Modular customer purchase API shim that maps to the consolidated customerPurchaseApi in services/api.ts
import { customerPurchaseApi } from '../api';
export { customerPurchaseApi };
export type {
  CustomerPurchaseDto,
  CustomerPurchaseItemDto,
  CreateCustomerPurchaseRequest,
  CustomerPurchaseSearchRequest,
  CustomerPurchaseSummaryDto,
} from '../api';

export default customerPurchaseApi;

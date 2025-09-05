// Treasury API client (modular)
// Mirrors backend TreasuryController and Models/TreasuryTransaction

import { API_CONFIG, STORAGE_KEYS } from '../../config/environment';
import type { SupplierTransactionDto } from '../api';

// Local minimal apiRequest to avoid coupling with monolithic api.ts
const API_BASE_URL = `${API_CONFIG.BASE_URL}/api`;
let authToken: string | null = null;
const getAuthToken = (): string | null => {
  if (!authToken) authToken = localStorage.getItem(STORAGE_KEYS.AUTH_TOKEN);
  return authToken;
};

// Backend returns plain JSON (number/array/object), not an ApiResponse wrapper
async function apiRequest<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
  const url = `${API_BASE_URL}${endpoint}`;
  const token = getAuthToken();
  const defaultHeaders: HeadersInit = { 'Content-Type': 'application/json' };
  if (token) (defaultHeaders as any).Authorization = `Bearer ${token}`;

  const config: RequestInit = { ...options, headers: { ...defaultHeaders, ...options.headers } };

  const response = await fetch(url, config);
  // Attempt to parse JSON; handle empty responses gracefully
  const text = await response.text();
  const data = text ? (JSON.parse(text) as T) : (undefined as unknown as T);
  if (!response.ok) {
    if (response.status === 401) {
      localStorage.removeItem(STORAGE_KEYS.AUTH_TOKEN);
    }
    const message = (data as any)?.message || `HTTP ${response.status}: ${response.statusText}`;
    throw new Error(message);
  }
  return data;
}

// Enums aligned with backend Models/TreasuryTransaction.cs
export enum TreasuryTransactionDirection {
  Credit = 1, // increases balance
  Debit = 2,  // decreases balance
}

export enum TreasuryTransactionType {
  Adjustment = 1,
  FeedFromCashDrawer = 2,
  SupplierPayment = 3,
  TransferIn = 4,
  TransferOut = 5,
}

// DTO aligned with backend TreasuryTransaction model
export interface TreasuryTransaction {
  id: number;
  treasuryAccountId: number;
  amount: number;
  direction: TreasuryTransactionDirection;
  type: TreasuryTransactionType;
  referenceType?: string | null;
  referenceId?: string | null;
  notes?: string | null;
  performedAt: string; // ISO date
  performedByUserId?: string | null;
  isDeleted: boolean;
}

// Requests matching controller action shapes
export interface AdjustRequest {
  amount: number;
  direction: TreasuryTransactionDirection;
  reason?: string;
}

export interface FeedFromCashDrawerRequest {
  date: string; // ISO date
  notes?: string;
}

export interface PaySupplierRequest {
  supplierId: number;
  amount: number;
  notes?: string;
}


export const treasuryApi = {
  async getBranchBalance(branchId: number): Promise<number> {
    const res = await apiRequest<number>(`/treasury/branches/${branchId}/balance`);
    return res;
  },

  async adjust(branchId: number, request: AdjustRequest): Promise<TreasuryTransaction> {
    const res = await apiRequest<TreasuryTransaction>(`/treasury/branches/${branchId}/adjust`, {
      method: 'POST',
      body: JSON.stringify(request),
    });
    return res;
  },

  async feedFromCashDrawer(branchId: number, request: FeedFromCashDrawerRequest): Promise<TreasuryTransaction> {
    const res = await apiRequest<TreasuryTransaction>(`/treasury/branches/${branchId}/feed-from-cashdrawer`, {
      method: 'POST',
      body: JSON.stringify(request),
    });
    return res;
  },

  async getTransactions(params: { branchId: number; from?: string; to?: string; type?: TreasuryTransactionType }): Promise<TreasuryTransaction[]> {
    const { branchId, ...query } = params;
    const qs = new URLSearchParams(
      Object.entries(query)
        .filter(([, v]) => v !== undefined && v !== null)
        .map(([k, v]) => [k, String(v)])
    ).toString();
    const url = qs ? `/treasury/branches/${branchId}/transactions?${qs}` : `/treasury/branches/${branchId}/transactions`;
    const res = await apiRequest<TreasuryTransaction[]>(url);
    return res;
  },

  async paySupplier(branchId: number, request: PaySupplierRequest): Promise<{ treasuryTxn: TreasuryTransaction; supplierTxn: SupplierTransactionDto }> {
    const res = await apiRequest<{ treasuryTxn: TreasuryTransaction; supplierTxn: SupplierTransactionDto }>(`/treasury/branches/${branchId}/pay-supplier`, {
      method: 'POST',
      body: JSON.stringify(request),
    });
    return res;
  },
};

export default treasuryApi;

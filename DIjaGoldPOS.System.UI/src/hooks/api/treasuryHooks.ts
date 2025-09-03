import { useCallback, useEffect, useMemo, useState } from 'react';
import treasuryApi, { TreasuryTransaction, TreasuryTransactionType } from '../../services/api/treasury';

export function useTreasuryBalance(branchId?: number) {
  const [balance, setBalance] = useState<number | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const canQuery = typeof branchId === 'number' && branchId > 0;

  const fetchBalance = useCallback(async () => {
    if (!canQuery) return;
    setLoading(true); setError(null);
    try {
      const b = await treasuryApi.getBranchBalance(branchId as number);
      setBalance(b);
    } catch (e: any) {
      setError(e?.message || 'Failed to load balance');
    } finally {
      setLoading(false);
    }
  }, [branchId, canQuery]);

  useEffect(() => {
    if (canQuery) fetchBalance();
  }, [canQuery, fetchBalance]);

  return { balance, loading, error, refetch: fetchBalance };
}

export function useTreasuryTransactions(params?: {
  branchId?: number;
  from?: string;
  to?: string;
  type?: TreasuryTransactionType;
}) {
  const [data, setData] = useState<TreasuryTransaction[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const query = useMemo(() => ({
    branchId: params?.branchId,
    from: params?.from,
    to: params?.to,
    type: params?.type,
  }), [params?.branchId, params?.from, params?.to, params?.type]);

  const canQuery = typeof query.branchId === 'number' && (query.branchId as number) > 0;

  const fetchTransactions = useCallback(async () => {
    if (!canQuery) return;
    setLoading(true); setError(null);
    try {
      const list = await treasuryApi.getTransactions({
        branchId: query.branchId as number,
        from: query.from,
        to: query.to,
        type: query.type,
      });
      setData(list);
    } catch (e: any) {
      setError(e?.message || 'Failed to load transactions');
    } finally {
      setLoading(false);
    }
  }, [canQuery, query.branchId, query.from, query.to, query.type]);

  useEffect(() => {
    if (canQuery) fetchTransactions();
  }, [canQuery, fetchTransactions]);

  return { data, loading, error, refetch: fetchTransactions };
}

// Optional mutation hooks for reuse
export function useAdjustTreasury() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const mutate = useCallback(async (branchId: number, req: Parameters<typeof treasuryApi.adjust>[1]) => {
    setLoading(true); setError(null);
    try {
      const result = await treasuryApi.adjust(branchId, req);
      return result;
    } catch (e: any) {
      setError(e?.message || 'Failed to adjust treasury');
      throw e;
    } finally {
      setLoading(false);
    }
  }, []);

  return { mutate, loading, error };
}

export function useFeedFromCashDrawer() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const mutate = useCallback(async (branchId: number, req: Parameters<typeof treasuryApi.feedFromCashDrawer>[1]) => {
    setLoading(true); setError(null);
    try {
      const result = await treasuryApi.feedFromCashDrawer(branchId, req);
      return result;
    } catch (e: any) {
      setError(e?.message || 'Failed to feed from cash drawer');
      throw e;
    } finally {
      setLoading(false);
    }
  }, []);

  return { mutate, loading, error };
}

export function usePaySupplier() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const mutate = useCallback(async (branchId: number, req: Parameters<typeof treasuryApi.paySupplier>[1]) => {
    setLoading(true); setError(null);
    try {
      const result = await treasuryApi.paySupplier(branchId, req);
      return result;
    } catch (e: any) {
      setError(e?.message || 'Failed to pay supplier');
      throw e;
    } finally {
      setLoading(false);
    }
  }, []);

  return { mutate, loading, error };
}

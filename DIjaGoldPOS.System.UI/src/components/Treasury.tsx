import React, { useMemo, useState, useEffect } from 'react';
import { TreasuryTransactionDirection, TreasuryTransactionType } from '../services/api/treasury';
import { useTreasuryBalance, useTreasuryTransactions, useAdjustTreasury, useFeedFromCashDrawer } from '../hooks';
import { Button } from './ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from './ui/select';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from './ui/table';
import { Tabs, TabsContent, TabsList, TabsTrigger } from './ui/tabs';
import { Separator } from './ui/separator';
import { useAuth } from './AuthContext';
import { branchesApi } from '../services/api';

type Props = {
  // Optional branchId; if not provided, user can enter manually
  branchId?: number;
};

const formatMoney = (v: number) => new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(v);
const formatDate = (iso?: string) => (iso ? new Date(iso).toLocaleString() : '');

const Treasury: React.FC<Props> = ({ branchId: initialBranchId }) => {
  const { user } = useAuth();
  const [branchId, setBranchId] = useState<number>((initialBranchId ?? user?.branch?.id ?? 1));
  const [branchDisplayName, setBranchDisplayName] = useState<string>(user?.branch?.name || '');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [from, setFrom] = useState<string>(() => new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().slice(0, 10));
  const [to, setTo] = useState<string>(() => new Date().toISOString().slice(0, 10));
  const [type, setType] = useState<string>('');

  // Forms
  const [adjAmount, setAdjAmount] = useState<string>('');
  const [adjDirection, setAdjDirection] = useState<TreasuryTransactionDirection>(TreasuryTransactionDirection.Credit);
  const [adjReason, setAdjReason] = useState<string>('');

  const [feedDate, setFeedDate] = useState<string>(() => new Date().toISOString().slice(0, 10));
  const [feedNotes, setFeedNotes] = useState<string>('');

  // Removed manual supplier payment state; payments are handled via PO flow

  const typeOptions = useMemo(() => [
    { value: 'all', label: 'All' },
    { value: String(TreasuryTransactionType.Adjustment), label: 'Adjustment' },
    { value: String(TreasuryTransactionType.FeedFromCashDrawer), label: 'Feed From Cash Drawer' },
    { value: String(TreasuryTransactionType.SupplierPayment), label: 'Supplier Payment' },
    { value: String(TreasuryTransactionType.TransferIn), label: 'Transfer In' },
    { value: String(TreasuryTransactionType.TransferOut), label: 'Transfer Out' },
  ], []);

  const directionOptions = useMemo(() => [
    { value: TreasuryTransactionDirection.Credit, label: 'Credit (+)' },
    { value: TreasuryTransactionDirection.Debit, label: 'Debit (-)' },
  ], []);

  const canQuery = branchId > 0;

  // Hooks
  const { balance, loading: balLoading, error: balError, refetch: refetchBalance } = useTreasuryBalance(canQuery ? (branchId as number) : undefined);
  const { data: txData, loading: txLoading, error: txError, refetch: refetchTx } = useTreasuryTransactions({
    branchId: canQuery ? (branchId as number) : undefined,
    from: from || undefined,
    to: to || undefined,
    type: type ? (Number(type) as TreasuryTransactionType) : undefined,
  });
  const { mutate: adjust, loading: adjLoading, error: adjError } = useAdjustTreasury();
  const { mutate: feed, loading: feedLoading, error: feedError } = useFeedFromCashDrawer();
  // Removed manual supplier payment hooks; payments are handled via PO flow

  // Removed suppliers fetch; no manual supplier payment in Treasury

  // Default to user's branch on load (CashDrawer behavior)
  useEffect(() => {
    if (user?.branch?.id) {
      setBranchId(user.branch.id);
      if (user.branch.name) setBranchDisplayName(user.branch.name);
    }
  }, [user?.branch?.id]);

  // If display name missing but we have ID, fetch the branch name for display (like CashDrawer)
  useEffect(() => {
    const ensureBranchName = async () => {
      try {
        if (!branchDisplayName && branchId > 0) {
          const branch = await branchesApi.getBranch(branchId as number);
          if (branch?.name) setBranchDisplayName(branch.name);
        }
      } catch (err) {
        // Silent fail
        console.warn('Unable to load branch name for display:', err);
      }
    };
    ensureBranchName();
  }, [branchDisplayName, branchId]);

  // No local sync needed; render directly from txData

  // Auto-refetch when branch changes so defaults take effect immediately
  useEffect(() => {
    if (branchId && branchId > 0) {
      refetchBalance();
      refetchTx();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [branchId]);

  

  const onAdjust = async () => {
    if (!canQuery) return;
    const amount = Number(adjAmount);
    if (!amount || isNaN(amount)) { setError('Enter a valid adjustment amount'); return; }
    setLoading(true); setError(null);
    try {
      await adjust(branchId as number, { amount, direction: adjDirection, reason: adjReason || undefined });
      setAdjAmount(''); setAdjReason('');
      await refetchBalance();
      await refetchTx();
    } catch (e: any) {
      setError(e?.message || 'Failed to create adjustment');
    } finally {
      setLoading(false);
    }
  };

  const onFeed = async () => {
    if (!canQuery) return;
    if (!feedDate) { setError('Select a date'); return; }
    setLoading(true); setError(null);
    try {
      await feed(branchId as number, { date: new Date(feedDate).toISOString(), notes: feedNotes || undefined });
      setFeedNotes('');
      await refetchBalance();
      await refetchTx();
    } catch (e: any) {
      setError(e?.message || 'Failed to feed from cash drawer');
    } finally {
      setLoading(false);
    }
  };

  // Removed manual supplier payment handler
  return (
    <div className="space-y-6 p-4">
      <Card>
        <CardHeader className="flex flex-row items-center justify-between space-y-0">
          <div>
            <CardTitle>Treasury</CardTitle>
            <CardDescription>Manage branch treasury balance and transactions</CardDescription>
          </div>
          <div className="flex items-center gap-3">
            <div className="flex items-center gap-2 min-w-[260px]">
              <Label>Branch</Label>
              <div className="w-60 px-3 py-2 rounded border bg-muted text-sm">{branchDisplayName || user?.branch?.name || '—'}</div>
            </div>
            <Button variant="secondary" onClick={() => refetchBalance()} disabled={!canQuery || balLoading || loading}>
              Refresh Balance
            </Button>
            {typeof balance === 'number' && (
              <div className="text-sm text-muted-foreground">
                Balance: <span className="font-semibold">{formatMoney(balance)}</span>
              </div>
            )}
          </div>
        </CardHeader>
      </Card>

      <Tabs defaultValue="transactions">
        <TabsList>
          <TabsTrigger value="transactions">Transactions</TabsTrigger>
          <TabsTrigger value="adjust">Adjust</TabsTrigger>
          <TabsTrigger value="feed">Feed From Cash Drawer</TabsTrigger>
          {/* Removed manual Pay Supplier tab; payments are handled via PO flow */}
        </TabsList>

        <TabsContent value="transactions">
          <Card>
            <CardHeader>
              <CardTitle>Transactions</CardTitle>
              <CardDescription>Filter and review treasury transactions</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex flex-wrap items-end gap-4">
                <div className="flex flex-col gap-1">
                  <Label htmlFor="from">From</Label>
                  <Input id="from" type="date" value={from} onChange={(e) => setFrom(e.target.value)} />
                </div>
                <div className="flex flex-col gap-1">
                  <Label htmlFor="to">To</Label>
                  <Input id="to" type="date" value={to} onChange={(e) => setTo(e.target.value)} />
                </div>
                <div className="flex flex-col gap-1 min-w-[220px]">
                  <Label>Type</Label>
                  <Select value={type || 'all'} onValueChange={(v) => setType(v === 'all' ? '' : v)}>
                    <SelectTrigger>
                      <SelectValue placeholder="All" />
                    </SelectTrigger>
                    <SelectContent>
                      {typeOptions.map((o) => (
                        <SelectItem key={o.value} value={String(o.value)}>{o.label}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <Button onClick={() => refetchTx()} disabled={!canQuery || txLoading || loading}>Refresh</Button>
              </div>
              <Separator />
              <div className="overflow-x-auto">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Date</TableHead>
                      <TableHead>Type</TableHead>
                      <TableHead className="text-right">Amount</TableHead>
                      <TableHead>Direction</TableHead>
                      <TableHead>Notes</TableHead>
                      <TableHead>Reference</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {(txData || []).map((t, idx) => (
                      <TableRow key={idx}>
                        <TableCell>{formatDate(t.performedAt)}</TableCell>
                        <TableCell>{TreasuryTransactionType[t.type]}</TableCell>
                        <TableCell className="text-right">{formatMoney(t.amount)}</TableCell>
                        <TableCell>{TreasuryTransactionDirection[t.direction]}</TableCell>
                        <TableCell>{t.notes || ''}</TableCell>
                        <TableCell>{t.referenceType ? `${t.referenceType}#${t.referenceId}` : ''}</TableCell>
                      </TableRow>
                    ))}
                    {(txData || []).length === 0 && (
                      <TableRow>
                        <TableCell colSpan={6} className="text-center text-muted-foreground">
                          No transactions
                        </TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="adjust">
          <Card>
            <CardHeader>
              <CardTitle>Adjust Balance</CardTitle>
              <CardDescription>Create a manual credit/debit adjustment</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="flex flex-wrap items-end gap-4">
                <div className="flex flex-col gap-1">
                  <Label htmlFor="adjAmount">Amount</Label>
                  <Input id="adjAmount" type="number" value={adjAmount} onChange={(e) => setAdjAmount(e.target.value)} className="w-40" />
                </div>
                <div className="flex flex-col gap-1 min-w-[220px]">
                  <Label>Direction</Label>
                  <Select value={String(adjDirection)} onValueChange={(v) => setAdjDirection(Number(v) as TreasuryTransactionDirection)}>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {directionOptions.map((o) => (
                        <SelectItem key={o.value} value={String(o.value)}>{o.label}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div className="flex flex-col gap-1 min-w-[280px] flex-1">
                  <Label htmlFor="adjReason">Reason</Label>
                  <Input id="adjReason" type="text" value={adjReason} onChange={(e) => setAdjReason(e.target.value)} />
                </div>
                <Button onClick={onAdjust} disabled={!canQuery || loading}>Submit</Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="feed">
          <Card>
            <CardHeader>
              <CardTitle>Feed From Cash Drawer</CardTitle>
              <CardDescription>Transfer money from cash drawer to treasury</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="flex flex-wrap items-end gap-4">
                <div className="flex flex-col gap-1">
                  <Label htmlFor="feedDate">Date</Label>
                  <Input id="feedDate" type="date" value={feedDate} onChange={(e) => setFeedDate(e.target.value)} />
                </div>
                <div className="flex flex-col gap-1 min-w-[280px] flex-1">
                  <Label htmlFor="feedNotes">Notes</Label>
                  <Input id="feedNotes" type="text" value={feedNotes} onChange={(e) => setFeedNotes(e.target.value)} />
                </div>
                <Button onClick={onFeed} disabled={!canQuery || loading}>Submit</Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Removed Pay Supplier content */}
      </Tabs>

      {error && (<div className="text-sm text-destructive">Error: {error}</div>)}
      {!error && (balError || txError || adjError || feedError) && (
        <div className="text-sm text-destructive">Error: {balError || txError || adjError || feedError}</div>
      )}
    </div>
  );
};

export default Treasury;

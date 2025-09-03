import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { NumberInput } from './ui/number-input';
import { Label } from './ui/label';
import { Textarea } from './ui/textarea';
import { Alert, AlertDescription } from './ui/alert';
import { Badge } from './ui/badge';
import { Separator } from './ui/separator';
import { useAuth } from './AuthContext';
import { cashDrawerApi, CashDrawerBalance, branchesApi } from '../services/api';
import { formatCurrency, formatDate } from './utils/currency';

export default function CashDrawer() {
  const { user: currentUser, isManager } = useAuth();
  
  // State management
  const [selectedBranchId, setSelectedBranchId] = useState<number>(currentUser?.branch?.id || 1);
  const [branchDisplayName, setBranchDisplayName] = useState<string>(currentUser?.branch?.name || '');
  // Helper function to format date as YYYY-MM-DD in local timezone
  const formatDateLocal = (date: Date) => {
    return date.getFullYear() + '-' + 
      String(date.getMonth() + 1).padStart(2, '0') + '-' + 
      String(date.getDate()).padStart(2, '0');
  };

  const [selectedDate, setSelectedDate] = useState(formatDateLocal(new Date()));
  const [openingBalance, setOpeningBalance] = useState<string>('');
  const [actualClosingBalance, setActualClosingBalance] = useState<string>('');
  const [notes, setNotes] = useState<string>('');
  const [currentBalance, setCurrentBalance] = useState<CashDrawerBalance | null>(null);
  const [isDrawerOpen, setIsDrawerOpen] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string>('');
  const [success, setSuccess] = useState<string>('');
  
  // Settlement state
  const [settledAmount, setSettledAmount] = useState<string>('');
  const [settlementNotes, setSettlementNotes] = useState<string>('');

  // Expected opening balance state
  const [expectedOpeningBalance, setExpectedOpeningBalance] = useState<number | null>(null);
  const [loadingExpectedBalance, setLoadingExpectedBalance] = useState<boolean>(false);

  // Update selectedBranchId when user changes
  useEffect(() => {
    if (currentUser?.branch?.id) {
      setSelectedBranchId(currentUser.branch.id);
      setBranchDisplayName(currentUser.branch.name);
    }
  }, [currentUser?.branch?.id]);

  // If branch object is missing but we have an ID, fetch the branch name for display
  useEffect(() => {
    const ensureBranchName = async () => {
      try {
        if (!branchDisplayName && selectedBranchId) {
          const branch = await branchesApi.getBranch(selectedBranchId);
          if (branch?.name) {
            setBranchDisplayName(branch.name);
          }
        }
      } catch (err) {
        // Silent fail, keep placeholder
        console.warn('Unable to load branch name for display:', err);
      }
    };
    ensureBranchName();
  }, [branchDisplayName, selectedBranchId]);

  // Load current balance on component mount and date change
  useEffect(() => {
    loadCurrentBalance();
    loadExpectedOpeningBalance();
  }, [selectedBranchId, selectedDate]);

  // Auto-set settlement amount to expected closing balance when balance loads
  useEffect(() => {
    if (currentBalance?.expectedClosingBalance) {
      setSettledAmount(currentBalance.expectedClosingBalance.toString());
    }
  }, [currentBalance]);

  // Auto-refresh balance when component loads if drawer is open
  useEffect(() => {
    const autoRefreshBalance = async () => {
      if (isDrawerOpen && currentBalance && selectedBranchId) {
        try {
          const refreshedBalance = await cashDrawerApi.refreshExpectedClosingBalance(selectedBranchId, selectedDate);
          setCurrentBalance(refreshedBalance);
        } catch (err) {
          console.warn('Auto-refresh failed:', err);
          // Don't show error to user for auto-refresh
        }
      }
    };

    autoRefreshBalance();
  }, [isDrawerOpen, selectedBranchId, selectedDate]); // Only run when these change

  const loadExpectedOpeningBalance = async () => {
    try {
      setLoadingExpectedBalance(true);
      const balance = await cashDrawerApi.getOpeningBalance(selectedBranchId, selectedDate);
      setExpectedOpeningBalance(balance);
    } catch (err) {
      console.error('Error loading expected opening balance:', err);
      // Don't show error to user for this, just log it
    } finally {
      setLoadingExpectedBalance(false);
    }
  };

  const loadCurrentBalance = async () => {
    try {
      setLoading(true);
      setError('');
      
      // Check if drawer is open
      const drawerOpen = await cashDrawerApi.isDrawerOpen(selectedBranchId, selectedDate);
      setIsDrawerOpen(drawerOpen);
      
      if (drawerOpen) {
        // Get current balance
        const balance = await cashDrawerApi.getBalance(selectedBranchId, selectedDate);
        setCurrentBalance(balance);
      } else {
        setCurrentBalance(null);
      }
    } catch (err) {
      console.error('Error loading current balance:', err);
      setError('Failed to load current balance');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDrawer = async () => {
    if (!openingBalance || parseFloat(openingBalance) < 0) {
      setError('Please enter a valid opening balance');
      return;
    }

    try {
      setLoading(true);
      setError('');
      setSuccess('');

      const balance = await cashDrawerApi.openDrawer(
        selectedBranchId,
        parseFloat(openingBalance),
        selectedDate,
        notes || undefined
      );

      setCurrentBalance(balance);
      setIsDrawerOpen(true);
      setSuccess('Cash drawer opened successfully!');
      setOpeningBalance('');
      setNotes('');
    } catch (err: any) {
      console.error('Error opening drawer:', err);
      setError(err.message || 'Failed to open cash drawer');
    } finally {
      setLoading(false);
    }
  };

  const handleCloseDrawer = async () => {
    if (!actualClosingBalance || parseFloat(actualClosingBalance) < 0) {
      setError('Please enter a valid closing balance');
      return;
    }

    try {
      setLoading(true);
      setError('');
      setSuccess('');

      const balance = await cashDrawerApi.closeDrawer(
        selectedBranchId,
        parseFloat(actualClosingBalance),
        selectedDate,
        notes || undefined
      );

      setCurrentBalance(balance);
      setIsDrawerOpen(false);
      setSuccess('Cash drawer closed successfully!');
      setActualClosingBalance('');
      setNotes('');
    } catch (err: any) {
      console.error('Error closing drawer:', err);
      setError(err.message || 'Failed to close cash drawer');
    } finally {
      setLoading(false);
    }
  };

  const handleRefreshBalance = async () => {
    try {
      setLoading(true);
      setError('');
      setSuccess('');

      const refreshedBalance = await cashDrawerApi.refreshExpectedClosingBalance(selectedBranchId, selectedDate);
      setCurrentBalance(refreshedBalance);
      setSuccess('Cash drawer balance refreshed successfully!');
    } catch (err: any) {
      console.error('Error refreshing balance:', err);
      setError(err.message || 'Failed to refresh balance');
    } finally {
      setLoading(false);
    }
  };

  const handleSettleShift = async () => {
    if (!actualClosingBalance || parseFloat(actualClosingBalance) < 0) {
      setError('Please enter a valid closing balance');
      return;
    }

    const expectedClosing = currentBalance?.expectedClosingBalance || 0;
    
    // Settlement amount must equal the expected closing balance
    if (!settledAmount || parseFloat(settledAmount) !== expectedClosing) {
      setError(`Settlement amount must be exactly ${formatCurrency(expectedClosing)} (Expected Closing Balance)`);
      return;
    }

    if (parseFloat(actualClosingBalance) < parseFloat(settledAmount)) {
      setError('Actual closing balance must be at least equal to the settlement amount');
      return;
    }

    try {
      setLoading(true);
      setError('');
      setSuccess('');

      const balance = await cashDrawerApi.settleShift(
        selectedBranchId,
        parseFloat(actualClosingBalance),
        parseFloat(settledAmount),
        selectedDate,
        settlementNotes || undefined,
        notes || undefined
      );

      setCurrentBalance(balance);
      setIsDrawerOpen(false);
      const expectedAmount = parseFloat(settledAmount);
      const carriedForward = parseFloat(actualClosingBalance) - expectedAmount;
      setSuccess(`Shift settled successfully! ${formatCurrency(expectedAmount)} settled (Expected Closing). ${formatCurrency(carriedForward)} carried forward to next day.`);
      setActualClosingBalance('');
      setSettledAmount('');
      setSettlementNotes('');
      setNotes('');
    } catch (err: any) {
      console.error('Error settling shift:', err);
      setError(err.message || 'Failed to settle shift');
    } finally {
      setLoading(false);
    }
  };

  const getStatusBadge = (status: number) => {
    switch (status) {
      case 1:
        return <Badge variant="default" className="bg-green-500">Open</Badge>;
      case 2:
        return <Badge variant="secondary">Closed</Badge>;
      case 3:
        return <Badge variant="destructive">Pending Reconciliation</Badge>;
      default:
        return <Badge variant="outline">Unknown</Badge>;
    }
  };

  return (
    <div className="container mx-auto p-6 space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-3xl font-bold">Cash Drawer Management</h1>
        <Badge variant={isDrawerOpen ? "default" : "secondary"}>
          {isDrawerOpen ? 'Drawer Open' : 'Drawer Closed'}
        </Badge>
      </div>

      {/* Error and Success Messages */}
      {error && (
        <Alert variant="destructive">
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}
      
      {success && (
        <Alert>
          <AlertDescription>{success}</AlertDescription>
        </Alert>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Branch and Date Selection */}
        <Card>
          <CardHeader>
            <CardTitle>Configuration</CardTitle>
            <CardDescription>Current branch and date for cash drawer operations</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <Label htmlFor="branch">Branch</Label>
              <Input
                id="branch"
                value={branchDisplayName || currentUser?.branch?.name || 'Unknown Branch'}
                readOnly
                className="w-full p-2 border rounded-md bg-gray-50 cursor-not-allowed"
              />
            </div>
            
            <div>
              <Label htmlFor="date">Date</Label>
              <Input
                id="date"
                type="date"
                value={selectedDate}
                onChange={(e) => setSelectedDate(e.target.value)}
              />
            </div>

            <div>
              <Label htmlFor="expectedOpeningBalance">Expected Opening Balance</Label>
              <div className="flex items-center gap-2">
                <Input
                  id="expectedOpeningBalance"
                  value={loadingExpectedBalance ? 'Loading...' : (expectedOpeningBalance !== null ? formatCurrency(expectedOpeningBalance) : 'No balance found')}
                  readOnly
                  className="w-full p-2 border rounded-md bg-gray-50 cursor-not-allowed"
                />
                <Button
                  variant="outline"
                  size="sm"
                  onClick={loadExpectedOpeningBalance}
                  disabled={loadingExpectedBalance}
                  className="h-10 px-3"
                  title="Refresh expected opening balance"
                >
                  🔄
                </Button>
              </div>
              <p className="text-xs text-gray-600 mt-1">
                This is the amount that should be in the drawer when opening (carried forward from previous day)
              </p>
            </div>
          </CardContent>
        </Card>

        {/* Current Balance Display */}
        <Card>
          <CardHeader>
            <CardTitle>Current Balance</CardTitle>
            <CardDescription>Today's cash drawer status</CardDescription>
          </CardHeader>
          <CardContent>
            {loading ? (
              <div className="text-center py-4">Loading...</div>
            ) : currentBalance ? (
              <div className="space-y-3">
                <div className="flex justify-between">
                  <span>Status:</span>
                  {getStatusBadge(currentBalance.status)}
                </div>
                <div className="flex justify-between">
                  <span>Opening Balance:</span>
                  <span className="font-semibold">{formatCurrency(currentBalance.openingBalance)}</span>
                </div>
                <div className="flex justify-between items-center">
                  <span>Expected Closing:</span>
                  <div className="flex items-center gap-2">
                    <span className="font-semibold">{formatCurrency(currentBalance.expectedClosingBalance)}</span>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={handleRefreshBalance}
                      disabled={loading}
                      className="h-6 px-2 text-xs"
                      title="Refresh expected closing balance to include recent transactions"
                    >
                      🔄
                    </Button>
                  </div>
                </div>
                {currentBalance.actualClosingBalance && (
                  <>
                    <div className="flex justify-between">
                      <span>Actual Closing:</span>
                      <span className="font-semibold">{formatCurrency(currentBalance.actualClosingBalance)}</span>
                    </div>
                    <div className="flex justify-between">
                      <span>Cash Over/Short:</span>
                      <span className={`font-semibold ${currentBalance.cashOverShort && currentBalance.cashOverShort !== 0 ? 'text-red-600' : 'text-green-600'}`}>
                        {formatCurrency(currentBalance.cashOverShort || 0)}
                      </span>
                    </div>
                    {currentBalance.settledAmount && (
                      <>
                        <Separator className="my-2" />
                        <div className="flex justify-between">
                          <span>Settled Amount:</span>
                          <span className="font-semibold text-orange-600">{formatCurrency(currentBalance.settledAmount)}</span>
                        </div>
                        <div className="flex justify-between">
                          <span>Carried Forward:</span>
                          <span className="font-semibold text-blue-600">{formatCurrency(currentBalance.carriedForwardAmount || 0)}</span>
                        </div>
                      </>
                    )}
                  </>
                )}
                {currentBalance.settlementNotes && (
                  <div className="mt-3 p-2 bg-orange-50 rounded">
                    <span className="text-sm text-orange-700">Settlement: {currentBalance.settlementNotes}</span>
                  </div>
                )}
                {currentBalance.notes && (
                  <div className="mt-3 p-2 bg-gray-50 rounded">
                    <span className="text-sm text-gray-600">Notes: {currentBalance.notes}</span>
                  </div>
                )}
              </div>
            ) : (
              <div className="text-center py-4 text-gray-500">
                No cash drawer balance found for this date
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      <Separator />

      {/* Open Drawer Section */}
      {!isDrawerOpen && (
        <Card>
          <CardHeader>
            <CardTitle>Open Cash Drawer</CardTitle>
            <CardDescription>Start the day by opening the cash drawer with initial balance</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <Label htmlFor="openingBalance">Opening Balance (EGP)</Label>
              <NumberInput
                id="openingBalance"
                value={openingBalance}
                onChange={(value) => setOpeningBalance(value)}
                placeholder="Enter opening balance"
                allowDecimal={true}
                allowNegative={false}
                maxDecimals={2}
              />
              {expectedOpeningBalance !== null && (
                <p className="text-xs text-blue-600 mt-1">
                  💡 Expected opening balance: {formatCurrency(expectedOpeningBalance)} (from previous day's settlement)
                </p>
              )}
            </div>
            
            <div>
              <Label htmlFor="openNotes">Notes (Optional)</Label>
              <Textarea
                id="openNotes"
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                placeholder="Add any notes about the opening balance"
                rows={3}
              />
            </div>
            
            <Button 
              onClick={handleOpenDrawer} 
              disabled={loading || !openingBalance}
              className="w-full"
            >
              {loading ? 'Opening...' : 'Open Cash Drawer'}
            </Button>
          </CardContent>
        </Card>
      )}

      {/* Close Drawer Section */}
      {isDrawerOpen && (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Regular Close Drawer */}
          <Card>
            <CardHeader>
              <CardTitle>Close Cash Drawer</CardTitle>
              <CardDescription>Simply close the drawer without settlement</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="p-3 bg-blue-50 rounded-md">
                <p className="text-sm text-blue-800">
                  <strong>Expected Closing Balance:</strong> {formatCurrency(currentBalance?.expectedClosingBalance || 0)}
                </p>
                <p className="text-xs text-blue-600 mt-1">
                  This is calculated based on opening balance + cash sales + cash repairs
                </p>
              </div>
              
              <div>
                <Label htmlFor="actualClosingBalance">Actual Closing Balance (EGP)</Label>
                <NumberInput
                  id="actualClosingBalance"
                  value={actualClosingBalance}
                  onChange={(value) => setActualClosingBalance(value)}
                  placeholder="Enter actual cash count"
                  allowDecimal={true}
                  allowNegative={false}
                  maxDecimals={2}
                />
              </div>
              
              <div>
                <Label htmlFor="closeNotes">Notes (Optional)</Label>
                <Textarea
                  id="closeNotes"
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  placeholder="Add any notes about discrepancies or issues"
                  rows={3}
                />
              </div>
              
              <Button 
                onClick={handleCloseDrawer} 
                disabled={loading || !actualClosingBalance}
                className="w-full"
                variant="destructive"
              >
                {loading ? 'Closing...' : 'Close Cash Drawer'}
              </Button>
            </CardContent>
          </Card>

          {/* Settle Shift */}
          <Card>
            <CardHeader>
              <CardTitle>Settle Shift</CardTitle>
              <CardDescription>Settle the expected closing balance and carry forward any excess cash to next day</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="p-3 bg-green-50 rounded-md">
                <div className="flex justify-between items-start">
                  <div>
                    <p className="text-sm text-green-800">
                      <strong>Expected Closing Balance:</strong> {formatCurrency(currentBalance?.expectedClosingBalance || 0)}
                    </p>
                    <p className="text-xs text-green-600 mt-1">
                      Settlement will remove the expected closing balance (sales + repairs revenue). Any excess cash will be carried forward to next day.
                    </p>
                  </div>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={handleRefreshBalance}
                    disabled={loading}
                    className="h-6 px-2 text-xs ml-2"
                    title="Refresh expected closing balance to include recent transactions"
                  >
                    🔄
                  </Button>
                </div>
              </div>
              
              <div>
                <Label htmlFor="settleActualBalance">Actual Closing Balance (EGP)</Label>
                <NumberInput
                  id="settleActualBalance"
                  value={actualClosingBalance}
                  onChange={(value) => setActualClosingBalance(value)}
                  placeholder="Enter total cash in drawer"
                  allowDecimal={true}
                  allowNegative={false}
                  maxDecimals={2}
                />
              </div>

              <div>
                <Label htmlFor="settledAmount">Settlement Amount (EGP)</Label>
                <NumberInput
                  id="settledAmount"
                  value={settledAmount}
                  onChange={() => {}} // Read-only
                  placeholder="Auto-calculated from expected closing"
                  allowDecimal={true}
                  allowNegative={false}
                  maxDecimals={2}
                  disabled={true}
                  className="bg-gray-50 cursor-not-allowed"
                />
                <p className="text-xs text-blue-600 mt-1">
                  ⓘ Settlement amount is automatically set to Expected Closing Balance
                </p>
                {actualClosingBalance && settledAmount && (
                  <p className="text-xs text-gray-600 mt-1">
                    <strong>Carry Forward:</strong> {formatCurrency(parseFloat(actualClosingBalance) - parseFloat(settledAmount || '0'))}
                  </p>
                )}
              </div>

              <div>
                <Label htmlFor="settlementNotes">Settlement Notes</Label>
                <Textarea
                  id="settlementNotes"
                  value={settlementNotes}
                  onChange={(e) => setSettlementNotes(e.target.value)}
                  placeholder="Reason for settlement (e.g., Bank deposit, Cash safe)"
                  rows={2}
                />
              </div>
              
              <div>
                <Label htmlFor="shiftNotes">General Notes (Optional)</Label>
                <Textarea
                  id="shiftNotes"
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  placeholder="Add any notes about the shift"
                  rows={2}
                />
              </div>
              
              <Button 
                onClick={handleSettleShift} 
                disabled={loading || !actualClosingBalance || !settledAmount}
                className="w-full"
                variant="default"
              >
                {loading ? 'Settling...' : 'Settle Shift'}
              </Button>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );
}

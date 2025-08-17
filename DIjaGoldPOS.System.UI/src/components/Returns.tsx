import React, { useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Badge } from './ui/badge';
import { Separator } from './ui/separator';
import { Textarea } from './ui/textarea';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from './ui/table';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from './ui/dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from './ui/select';
import {
  Search,
  RotateCcw,
  AlertCircle,
  CheckCircle,
  Clock,
  Receipt,
  ArrowLeft,
} from 'lucide-react';
import { formatCurrency } from './utils/currency';
import { useAuth } from './AuthContext';
import { useSearchTransactions, useTransactionTypes, useTransactionStatuses } from '../hooks/useApi';
import { Transaction as ApiTransaction } from '../services/api';
import { EnumMapper } from '../types/enums';

interface Transaction {
  id: string;
  transactionNumber: string;
  date: string;
  customerName: string;
  items: TransactionItem[];
  total: number;
  status: 'completed' | 'returned' | 'partially_returned';
  canReturn: boolean;
}

interface TransactionItem {
  id: string;
  name: string;
  category: string;
  weight: number;
  rate: number;
  quantity: number;
  total: number;
  returned: boolean;
  returnReason?: string;
}

export default function Returns() {
  const { isManager, user } = useAuth();
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedTransaction, setSelectedTransaction] = useState<Transaction | null>(null);
  const [returnItems, setReturnItems] = useState<string[]>([]);
  const [returnReason, setReturnReason] = useState('');
  const [isProcessingReturn, setIsProcessingReturn] = useState(false);

  // API hooks
  const { execute: searchTransactions, loading: transactionsLoading } = useSearchTransactions();
  const { data: transactionTypesData, execute: fetchTransactionTypes } = useTransactionTypes();
  const { data: transactionStatusesData, execute: fetchTransactionStatuses } = useTransactionStatuses();
  const [transactions, setTransactions] = useState<Transaction[]>([]);

  // Fetch transactions on component mount
  React.useEffect(() => {
    // Fetch lookup data
    fetchTransactionTypes();
    fetchTransactionStatuses();
    
    const fetchTransactions = async () => {
      try {
        if (user?.branch?.id) {
          const result = await searchTransactions({
            branchId: user.branch.id,
            transactionType: 'Sale',
            status: 'Completed',
            pageSize: 50,
          });
          
          // Transform API transactions to component format
          const transformedTransactions: Transaction[] = result.items.map((apiTransaction: ApiTransaction) => ({
            id: apiTransaction.id.toString(),
            transactionNumber: apiTransaction.transactionNumber,
            date: apiTransaction.transactionDate,
            customerName: apiTransaction.customerName || 'Walk-in Customer',
            total: apiTransaction.totalAmount,
            status: apiTransaction.status.toLowerCase() as 'completed' | 'returned' | 'partially_returned',
            canReturn: apiTransaction.status === 'Completed',
            items: apiTransaction.items.map((item, index) => ({
              id: item.id.toString(),
              name: item.productName,
              category: item.productCode,
              weight: item.totalWeight,
              rate: item.goldRatePerGram,
              quantity: item.quantity,
              total: item.lineTotal,
              returned: false,
            })),
          }));
          
          setTransactions(transformedTransactions);
        }
      } catch (error) {
        console.error('Failed to fetch transactions:', error);
        // Set empty array on error - user will see "no transactions found" message
        setTransactions([]);
      }
    };

    fetchTransactions();
  }, [user?.branch?.id, searchTransactions, fetchTransactionTypes, fetchTransactionStatuses]);

  const filteredTransactions = transactions.filter(
    transaction =>
      transaction.transactionNumber.toLowerCase().includes(searchQuery.toLowerCase()) ||
      transaction.customerName.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleReturnToggle = (itemId: string) => {
    if (returnItems.includes(itemId)) {
      setReturnItems(returnItems.filter(id => id !== itemId));
    } else {
      setReturnItems([...returnItems, itemId]);
    }
  };

  const handleProcessReturn = async () => {
    if (!isManager) {
      alert('Only managers can process returns');
      return;
    }

    if (returnItems.length === 0) {
      alert('Please select items to return');
      return;
    }

    if (!returnReason.trim()) {
      alert('Please provide a reason for the return');
      return;
    }

    setIsProcessingReturn(true);
    
    try {
      // TODO: Replace with actual return API call when available
      // const returnRequest = {
      //   transactionId: selectedTransaction?.id,
      //   items: returnItems,
      //   reason: returnReason,
      //   processedBy: user?.id,
      // };
      // await processReturn(returnRequest);
      
      // Simulate API call for now
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      alert('Return processed successfully!');
      setSelectedTransaction(null);
      setReturnItems([]);
      setReturnReason('');
    } catch (error) {
      alert('Failed to process return. Please try again.');
      console.error('Return processing error:', error);
    } finally {
      setIsProcessingReturn(false);
    }
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'completed':
        return <Badge variant="default" className="bg-green-100 text-green-800">Completed</Badge>;
      case 'returned':
        return <Badge variant="secondary" className="bg-orange-100 text-orange-800">Returned</Badge>;
      case 'partially_returned':
        return <Badge variant="outline" className="bg-yellow-100 text-yellow-800">Partially Returned</Badge>;
      default:
        return <Badge variant="outline">{status}</Badge>;
    }
  };

  if (selectedTransaction) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button
            variant="outline"
            onClick={() => setSelectedTransaction(null)}
            className="touch-target hover:bg-[#F4E9B1] transition-colors"
          >
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Search
          </Button>
          <div>
            <h1 className="text-3xl text-[#0D1B2A]">Process Return</h1>
            <p className="text-muted-foreground">Transaction: {selectedTransaction.transactionNumber}</p>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Transaction Details */}
          <div className="lg:col-span-2">
            <Card className="pos-card">
              <CardHeader>
                <CardTitle>Transaction Details</CardTitle>
                <CardDescription>
                  Customer: {selectedTransaction.customerName} | 
                  Date: {new Date(selectedTransaction.date).toLocaleDateString()}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Return</TableHead>
                      <TableHead>Item</TableHead>
                      <TableHead>Weight</TableHead>
                      <TableHead>Rate</TableHead>
                      <TableHead>Total</TableHead>
                      <TableHead>Status</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {selectedTransaction.items.map((item) => (
                      <TableRow key={item.id}>
                        <TableCell>
                          <input
                            type="checkbox"
                            checked={returnItems.includes(item.id)}
                            onChange={() => handleReturnToggle(item.id)}
                            disabled={item.returned || !isManager}
                            className="h-4 w-4 text-[#D4AF37] focus:ring-[#D4AF37] border-gray-300 rounded"
                          />
                        </TableCell>
                        <TableCell>
                          <div>
                            <p className="font-medium">{item.name}</p>
                            <p className="text-sm text-muted-foreground">{item.category}</p>
                          </div>
                        </TableCell>
                        <TableCell>{item.weight}g</TableCell>
                        <TableCell>{formatCurrency(item.rate)}/g</TableCell>
                        <TableCell>{formatCurrency(item.total)}</TableCell>
                        <TableCell>
                          {item.returned ? (
                            <div>
                              <Badge variant="secondary" className="bg-orange-100 text-orange-800">Returned</Badge>
                              {item.returnReason && (
                                <p className="text-xs text-muted-foreground mt-1">{item.returnReason}</p>
                              )}
                            </div>
                          ) : (
                            <Badge variant="default" className="bg-green-100 text-green-800">Active</Badge>
                          )}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>
          </div>

          {/* Return Processing */}
          <div>
            <Card className="pos-card">
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <RotateCcw className="h-5 w-5 text-[#D4AF37]" />
                  Return Processing
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                {!isManager && (
                  <div className="p-3 bg-yellow-50 border border-yellow-200 rounded-lg">
                    <div className="flex items-center gap-2">
                      <AlertCircle className="h-4 w-4 text-yellow-600" />
                      <p className="text-sm text-yellow-800">Manager approval required for returns</p>
                    </div>
                  </div>
                )}

                <div className="space-y-2">
                  <Label>Return Reason</Label>
                  <Select value={returnReason} onValueChange={setReturnReason}>
                    <SelectTrigger>
                      <SelectValue placeholder="Select reason" />
                    </SelectTrigger>
                    <SelectContent className="bg-white border-gray-200 shadow-lg">
                      <SelectItem value="defective" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Defective Product</SelectItem>
                      <SelectItem value="size_issue" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Size Issue</SelectItem>
                      <SelectItem value="customer_dissatisfied" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Customer Dissatisfied</SelectItem>
                      <SelectItem value="wrong_item" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Wrong Item Delivered</SelectItem>
                      <SelectItem value="quality_issue" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Quality Issue</SelectItem>
                      <SelectItem value="other" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Other</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                {returnReason === 'other' && (
                  <div className="space-y-2">
                    <Label>Additional Details</Label>
                    <Textarea 
                      placeholder="Please provide additional details..."
                      className="min-h-20"
                    />
                  </div>
                )}

                <Separator />

                <div className="space-y-2">
                  <div className="flex justify-between">
                    <span>Items Selected:</span>
                    <span>{returnItems.length}</span>
                  </div>
                  <div className="flex justify-between font-semibold">
                    <span>Return Amount:</span>
                    <span>
                      {formatCurrency(
                        selectedTransaction.items
                          .filter(item => returnItems.includes(item.id))
                          .reduce((sum, item) => sum + item.total, 0)
                      )}
                    </span>
                  </div>
                </div>

                <Button
                  className="w-full touch-target"
                  variant="golden"
                  onClick={handleProcessReturn}
                  disabled={!isManager || returnItems.length === 0 || !returnReason || isProcessingReturn}
                >
                  {isProcessingReturn ? (
                    <>
                      <Clock className="mr-2 h-4 w-4 animate-spin" />
                      Processing...
                    </>
                  ) : (
                    <>
                      <CheckCircle className="mr-2 h-4 w-4" />
                      Process Return
                    </>
                  )}
                </Button>
              </CardContent>
            </Card>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl text-[#0D1B2A]">Returns & Exchanges</h1>
          <p className="text-muted-foreground">Search and process product returns</p>
        </div>
      </div>

      {/* Search Section */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Search className="h-5 w-5 text-[#D4AF37]" />
            Transaction Search
          </CardTitle>
          <CardDescription>
            Search by transaction number or customer name
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="relative">
            <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Enter transaction number or customer name..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-10 touch-target"
            />
          </div>
        </CardContent>
      </Card>

      {/* Transaction Results */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle>Search Results</CardTitle>
          <CardDescription>
            {filteredTransactions.length} transaction(s) found
          </CardDescription>
        </CardHeader>
        <CardContent>
          {filteredTransactions.length === 0 ? (
            <div className="text-center py-8">
              <Receipt className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <p className="text-muted-foreground">No transactions found</p>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Transaction #</TableHead>
                  <TableHead>Customer</TableHead>
                  <TableHead>Date</TableHead>
                  <TableHead>Total</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredTransactions.map((transaction) => (
                  <TableRow key={transaction.id}>
                    <TableCell className="font-medium">
                      {transaction.transactionNumber}
                    </TableCell>
                    <TableCell>{transaction.customerName}</TableCell>
                    <TableCell>
                      {new Date(transaction.date).toLocaleDateString()}
                    </TableCell>
                    <TableCell>{formatCurrency(transaction.total)}</TableCell>
                    <TableCell>{getStatusBadge(transaction.status)}</TableCell>
                    <TableCell>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => setSelectedTransaction(transaction)}
                        disabled={!transaction.canReturn}
                        className="touch-target hover:bg-[#F4E9B1] transition-colors"
                      >
                        <RotateCcw className="mr-2 h-4 w-4" />
                        {transaction.canReturn ? 'Process Return' : 'View Only'}
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
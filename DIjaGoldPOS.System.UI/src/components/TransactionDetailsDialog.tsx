import React, { useState } from 'react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from './ui/dialog';
import { Button } from './ui/button';
import { Badge } from './ui/badge';
import { Separator } from './ui/separator';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from './ui/table';
import {
  Receipt,
  Printer,
  Loader2,
  X,
  Calendar,
  User,
  CreditCard,
  Building,
} from 'lucide-react';
import { formatCurrency } from './utils/currency';
import { Transaction } from '../services/api';
import { EnumMapper } from '../types/enums';

interface TransactionDetailsDialogProps {
  transaction: Transaction | null;
  isOpen: boolean;
  onClose: () => void;
  onPrint: (transactionId: number) => Promise<void>;
}

export default function TransactionDetailsDialog({
  transaction,
  isOpen,
  onClose,
  onPrint,
}: TransactionDetailsDialogProps) {
  const [isPrinting, setIsPrinting] = useState(false);

  if (!transaction) return null;

  const handlePrint = async () => {
    setIsPrinting(true);
    try {
      await onPrint(transaction.id);
    } finally {
      setIsPrinting(false);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto bg-white">
        <DialogHeader className="pb-4">
          <div className="flex items-center justify-between">
            <div>
              <DialogTitle className="text-2xl font-bold text-gray-800">
                Transaction Details
              </DialogTitle>
              <DialogDescription className="text-gray-600">
                Transaction #{transaction.transactionNumber}
              </DialogDescription>
            </div>
            <Button
              variant="ghost"
              size="sm"
              onClick={onClose}
              className="h-8 w-8 p-0"
            >
              <X className="h-4 w-4" />
            </Button>
          </div>
        </DialogHeader>

        <div className="space-y-6">
          {/* Transaction Header */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 p-4 bg-gray-50 rounded-lg">
            <div className="space-y-1">
              <p className="text-sm font-medium text-gray-500">Transaction Number</p>
              <p className="font-mono text-lg font-bold text-gray-800">
                {transaction.transactionNumber}
              </p>
            </div>
            <div className="space-y-1">
              <p className="text-sm font-medium text-gray-500">Date & Time</p>
              <p className="text-gray-800">
                {new Date(transaction.transactionDate).toLocaleDateString('en-US', {
                  year: 'numeric',
                  month: 'long',
                  day: 'numeric',
                })}
              </p>
              <p className="text-sm text-gray-600">
                {new Date(transaction.transactionDate).toLocaleTimeString('en-US', {
                  hour: '2-digit',
                  minute: '2-digit',
                  hour12: true,
                })}
              </p>
            </div>
            <div className="space-y-1">
              <p className="text-sm font-medium text-gray-500">Type</p>
              <Badge 
                variant={transaction.transactionType === 'Sale' ? 'default' : 'secondary'}
                className={transaction.transactionType === 'Sale' ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'}
              >
                {transaction.transactionType}
              </Badge>
            </div>
            <div className="space-y-1">
              <p className="text-sm font-medium text-gray-500">Status</p>
              <Badge 
                variant={transaction.status === 'Completed' ? 'default' : 'secondary'}
                className={transaction.status === 'Completed' ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'}
              >
                {transaction.statusDisplayName || transaction.status}
              </Badge>
            </div>
          </div>

          {/* Customer & Payment Info */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="space-y-4">
              <div className="flex items-center gap-2">
                <User className="h-5 w-5 text-gray-500" />
                <h3 className="text-lg font-semibold text-gray-800">Customer Information</h3>
              </div>
              <div className="p-4 border rounded-lg">
                {transaction.customerName ? (
                  <div className="space-y-2">
                    <p className="font-medium text-gray-800">{transaction.customerName}</p>
                    {transaction.customerMobile && (
                      <p className="text-sm text-gray-600">Phone: {transaction.customerMobile}</p>
                    )}
                    {transaction.customerEmail && (
                      <p className="text-sm text-gray-600">Email: {transaction.customerEmail}</p>
                    )}
                  </div>
                ) : (
                  <p className="text-gray-500 italic">Walk-in Customer</p>
                )}
              </div>
            </div>

            <div className="space-y-4">
              <div className="flex items-center gap-2">
                <CreditCard className="h-5 w-5 text-gray-500" />
                <h3 className="text-lg font-semibold text-gray-800">Payment Information</h3>
              </div>
              <div className="p-4 border rounded-lg space-y-2">
                <div className="flex justify-between">
                  <span className="text-gray-600">Payment Method:</span>
                  <span className="font-medium">{transaction.paymentMethod}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Amount Paid:</span>
                  <span className="font-medium">{formatCurrency(transaction.amountPaid)}</span>
                </div>
                {transaction.changeGiven !== 0 && (
                  <div className="flex justify-between">
                    <span className="text-gray-600">Change Given:</span>
                    <span className={`font-medium ${transaction.changeGiven > 0 ? 'text-green-600' : 'text-red-600'}`}>
                      {transaction.changeGiven > 0 ? '+' : ''}{formatCurrency(transaction.changeGiven)}
                    </span>
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* Branch & Cashier Info */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="space-y-4">
              <div className="flex items-center gap-2">
                <Building className="h-5 w-5 text-gray-500" />
                <h3 className="text-lg font-semibold text-gray-800">Branch Information</h3>
              </div>
              <div className="p-4 border rounded-lg">
                <p className="font-medium text-gray-800">{transaction.branchName}</p>
                <p className="text-sm text-gray-600">Branch Code: {transaction.branchCode}</p>
              </div>
            </div>

            <div className="space-y-4">
              <div className="flex items-center gap-2">
                <User className="h-5 w-5 text-gray-500" />
                <h3 className="text-lg font-semibold text-gray-800">Cashier Information</h3>
              </div>
              <div className="p-4 border rounded-lg">
                <p className="font-medium text-gray-800">{transaction.cashierName}</p>
                <p className="text-sm text-gray-600">Employee Code: {transaction.cashierEmployeeCode}</p>
              </div>
            </div>
          </div>

          {/* Items Table */}
          <div className="space-y-4">
            <h3 className="text-lg font-semibold text-gray-800">Items</h3>
            <div className="border rounded-lg overflow-hidden">
              <Table>
                <TableHeader>
                  <TableRow className="bg-gray-50">
                    <TableHead className="font-medium">Product</TableHead>
                    <TableHead className="font-medium text-right">Weight</TableHead>
                    <TableHead className="font-medium text-right">Rate</TableHead>
                    <TableHead className="font-medium text-right">Making Charges</TableHead>
                    <TableHead className="font-medium text-right">Quantity</TableHead>
                    <TableHead className="font-medium text-right">Unit Price</TableHead>
                    <TableHead className="font-medium text-right">Total</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {transaction.items?.map((item, index) => (
                    <TableRow key={index}>
                      <TableCell>
                        <div>
                          <p className="font-medium">{item.productName}</p>
                                                   <p className="text-sm text-gray-500">
                           {item.productCode} • {item.karatType}
                         </p>
                        </div>
                      </TableCell>
                                             <TableCell className="text-right">{item.totalWeight}g</TableCell>
                       <TableCell className="text-right">{formatCurrency(item.goldRatePerGram)}/g</TableCell>
                       <TableCell className="text-right">{formatCurrency(item.makingChargesAmount)}</TableCell>
                       <TableCell className="text-right">{item.quantity}</TableCell>
                       <TableCell className="text-right">{formatCurrency(item.unitPrice)}</TableCell>
                       <TableCell className="text-right font-medium">
                         {formatCurrency(item.lineTotal)}
                       </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          </div>

          {/* Bill Summary */}
          <div className="space-y-4">
            <h3 className="text-lg font-semibold text-gray-800">Bill Summary</h3>
            <div className="p-4 border rounded-lg space-y-3">
              <div className="flex justify-between">
                <span className="text-gray-600">Subtotal (Gold Value):</span>
                <span>{formatCurrency(transaction.subtotal)}</span>
              </div>
              {transaction.totalMakingCharges && transaction.totalMakingCharges > 0 && (
                <div className="flex justify-between">
                  <span className="text-gray-600">Making Charges:</span>
                  <span>{formatCurrency(transaction.totalMakingCharges)}</span>
                </div>
              )}
              {transaction.totalDiscountAmount && transaction.totalDiscountAmount > 0 && (
                <div className="flex justify-between text-green-600">
                  <span>Discount:</span>
                  <span>-{formatCurrency(transaction.totalDiscountAmount)}</span>
                </div>
              )}
              <div className="flex justify-between">
                <span className="text-gray-600">Tax:</span>
                <span>{formatCurrency(transaction.totalTaxAmount)}</span>
              </div>
              <Separator />
              <div className="flex justify-between text-lg font-bold">
                <span>Total:</span>
                <span>{formatCurrency(transaction.totalAmount)}</span>
              </div>
            </div>
          </div>

          {/* Notes */}
          {transaction.notes && (
            <div className="space-y-2">
              <h3 className="text-lg font-semibold text-gray-800">Notes</h3>
              <div className="p-4 border rounded-lg bg-gray-50">
                <p className="text-gray-700">{transaction.notes}</p>
              </div>
            </div>
          )}

          {/* Action Buttons */}
          <div className="flex items-center justify-end gap-3 pt-4 border-t">
            <Button
              variant="outline"
              onClick={onClose}
              className="hover:bg-gray-100"
            >
              Close
            </Button>
            <Button
              onClick={handlePrint}
              disabled={isPrinting}
              className="bg-blue-600 hover:bg-blue-700 text-white"
            >
              {isPrinting ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Printing...
                </>
              ) : (
                <>
                  <Printer className="mr-2 h-4 w-4" />
                  Print Receipt
                </>
              )}
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}

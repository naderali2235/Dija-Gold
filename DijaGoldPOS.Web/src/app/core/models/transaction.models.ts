import { TransactionType, PaymentMethod, TransactionStatus, KaratType } from './enums';

export interface Transaction {
  id: number;
  transactionNumber: string;
  transactionType: TransactionType;
  transactionDate: string;
  branchId: number;
  branchName: string;
  customerId?: number;
  customerName?: string;
  cashierName: string;
  approvedByName?: string;
  subtotal: number;
  totalMakingCharges: number;
  totalTaxAmount: number;
  discountAmount: number;
  totalAmount: number;
  amountPaid: number;
  changeGiven: number;
  paymentMethod: PaymentMethod;
  status: TransactionStatus;
  returnReason?: string;
  repairDescription?: string;
  estimatedCompletionDate?: string;
  receiptPrinted: boolean;
  items: TransactionItem[];
  taxes: TransactionTax[];
}

export interface TransactionItem {
  id: number;
  productId: number;
  productName: string;
  productCode: string;
  karatType: KaratType;
  quantity: number;
  unitWeight: number;
  totalWeight: number;
  goldRatePerGram: number;
  unitPrice: number;
  makingChargesAmount: number;
  discountPercentage: number;
  discountAmount: number;
  lineTotal: number;
}

export interface TransactionTax {
  id: number;
  taxName: string;
  taxCode: string;
  taxRate: number;
  taxableAmount: number;
  taxAmount: number;
}

export interface SaleTransactionRequest {
  branchId: number;
  customerId?: number;
  items: SaleItemRequest[];
  amountPaid: number;
  paymentMethod: PaymentMethod;
}

export interface SaleItemRequest {
  productId: number;
  quantity: number;
  customDiscountPercentage?: number;
}

export interface ReturnTransactionRequest {
  originalTransactionId: number;
  returnReason: string;
  returnAmount: number;
  items: ReturnItemRequest[];
}

export interface ReturnItemRequest {
  originalTransactionItemId: number;
  returnQuantity: number;
}

export interface RepairTransactionRequest {
  branchId: number;
  customerId?: number;
  repairDescription: string;
  repairAmount: number;
  estimatedCompletionDate?: string;
  amountPaid: number;
  paymentMethod: PaymentMethod;
}

export interface TransactionSearchRequest {
  branchId?: number;
  transactionNumber?: string;
  transactionType?: TransactionType;
  status?: TransactionStatus;
  customerId?: number;
  cashierId?: string;
  fromDate?: string;
  toDate?: string;
  minAmount?: number;
  maxAmount?: number;
  pageNumber?: number;
  pageSize?: number;
}

export interface CancelTransactionRequest {
  transactionId: number;
  reason: string;
}

export interface ReprintReceiptRequest {
  transactionId: number;
  copies?: number;
}

export interface TransactionReceipt {
  receiptContent: string;
  printedSuccessfully: boolean;
  printError?: string;
}

export interface TransactionSummary {
  transactionCount: number;
  totalAmount: number;
  totalTax: number;
  averageTransactionValue: number;
  transactionsByType: { [key in TransactionType]?: number };
  amountsByPaymentMethod: { [key in PaymentMethod]?: number };
}

// POS specific models
export interface PosCart {
  items: PosCartItem[];
  subtotal: number;
  makingCharges: number;
  discountAmount: number;
  taxAmount: number;
  total: number;
  customerId?: number;
  customerName?: string;
}

export interface PosCartItem {
  product: any; // Will be filled with Product interface
  quantity: number;
  unitPrice: number;
  makingCharges: number;
  discountPercentage: number;
  discountAmount: number;
  lineTotal: number;
  availableStock: number;
}

export interface PosPayment {
  amountPaid: number;
  paymentMethod: PaymentMethod;
  changeGiven: number;
}
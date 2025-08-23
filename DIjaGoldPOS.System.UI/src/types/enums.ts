/**
 * Lookup interfaces that match the backend API
 * This file centralizes all lookup definitions to avoid hardcoded values throughout the application
 */

// Lookup data structure from API
export interface EnumLookupDto {
  value: number;
  name: string;
  displayName: string;
  code: string;
}

// Lookup collections from API
export interface ApiLookupsResponse {
  transactionTypes: EnumLookupDto[];
  paymentMethods: EnumLookupDto[];
  transactionStatuses: EnumLookupDto[];
  chargeTypes: EnumLookupDto[];
  karatTypes: EnumLookupDto[];
  productCategoryTypes: EnumLookupDto[];
  repairStatuses: EnumLookupDto[];
  repairPriorities: EnumLookupDto[];
  orderTypes: EnumLookupDto[];
  orderStatuses: EnumLookupDto[];
  businessEntityTypes: EnumLookupDto[];
  financialTransactionTypes: EnumLookupDto[];
  financialTransactionStatuses: EnumLookupDto[];
}

// Constants for lookup table IDs (matching backend LookupTableConstants)
export const LookupTableConstants = {
  // Order Types
  OrderTypeSale: 1,
  OrderTypeReturn: 2,
  OrderTypeExchange: 3,
  OrderTypeLayaway: 4,
  OrderTypeReservation: 5,
  OrderTypeRepair: 6,

  // Order Statuses
  OrderStatusPending: 1,
  OrderStatusCompleted: 2,
  OrderStatusCancelled: 3,
  OrderStatusRefunded: 4,

  // Financial Transaction Types
  FinancialTransactionTypeSale: 1,
  FinancialTransactionTypeReturn: 2,
  FinancialTransactionTypeRepair: 3,
  FinancialTransactionTypeExchange: 4,
  FinancialTransactionTypeRefund: 5,
  FinancialTransactionTypeAdjustment: 6,
  FinancialTransactionTypeVoid: 7,

  // Financial Transaction Statuses
  FinancialTransactionStatusPending: 1,
  FinancialTransactionStatusCompleted: 2,
  FinancialTransactionStatusCancelled: 3,
  FinancialTransactionStatusRefunded: 4,
  FinancialTransactionStatusVoided: 5,
  FinancialTransactionStatusReversed: 6,

  // Business Entity Types
  BusinessEntityTypeCustomer: 1,
  BusinessEntityTypeSupplier: 2,
  BusinessEntityTypeBranch: 3,
  BusinessEntityTypeOrder: 4,

  // Repair Statuses
  RepairStatusPending: 1,
  RepairStatusInProgress: 2,
  RepairStatusCompleted: 3,
  RepairStatusReadyForPickup: 4,
  RepairStatusDelivered: 5,
  RepairStatusCancelled: 6,

  // Repair Priorities
  RepairPriorityLow: 1,
  RepairPriorityMedium: 2,
  RepairPriorityHigh: 3,
  RepairPriorityUrgent: 4,

  // Karat Types
  KaratType18K: 1,
  KaratType21K: 2,
  KaratType22K: 3,
  KaratType24K: 4,

  // Product Category Types
  ProductCategoryTypeGoldJewelry: 1,
  ProductCategoryTypeBullion: 2,
  ProductCategoryTypeGoldCoins: 3,

  // Transaction Types
  TransactionTypeSale: 1,
  TransactionTypeReturn: 2,
  TransactionTypeRepair: 3,

  // Payment Methods
  PaymentMethodCash: 1,

  // Transaction Statuses
  TransactionStatusPending: 1,
  TransactionStatusCompleted: 2,
  TransactionStatusCancelled: 3,
  TransactionStatusRefunded: 4,
  TransactionStatusVoided: 5,

  // Charge Types
  ChargeTypePercentage: 1,
  ChargeTypeFixedAmount: 2
} as const;

// String literal types for backward compatibility with existing interfaces
export type TransactionTypeString = 'Sale' | 'Return' | 'Repair';
export type PaymentMethodString = 'Cash' | 'Card' | 'BankTransfer' | 'Cheque';
export type TransactionStatusString = 'Pending' | 'Completed' | 'Cancelled' | 'Refunded' | 'Voided';
export type KaratTypeString = '18K' | '21K' | '22K' | '24K';
export type ProductCategoryTypeString = 'GoldJewelry' | 'Bullion' | 'Coins';

// Backward compatibility types
export type ProductCategoryType = ProductCategoryTypeString;
export type KaratType = KaratTypeString;
export type ChargeType = 'Percentage' | 'FixedAmount';

// Helper functions for working with lookup data
export class LookupHelper {
  // Find lookup item by value
  static findById<T extends EnumLookupDto>(lookups: T[], value: number): T | undefined {
    return lookups.find(item => item.value === value);
  }

  // Find lookup item by name
  static findByName<T extends EnumLookupDto>(lookups: T[], name: string): T | undefined {
    return lookups.find(item => item.name === name);
  }

  // Get display name by value
  static getDisplayName(lookups: EnumLookupDto[], value: number): string {
    const item = this.findById(lookups, value);
    return item?.displayName || 'Unknown';
  }

  // Get name by value
  static getName(lookups: EnumLookupDto[], value: number): string {
    const item = this.findById(lookups, value);
    return item?.name || 'Unknown';
  }

  // Get value by name
  static getValue(lookups: EnumLookupDto[], name: string): number | undefined {
    const item = this.findByName(lookups, name);
    return item?.value;
  }

  // Helper to convert lookup data to simple arrays for dropdowns
  static lookupToSelectOptions(lookupData: EnumLookupDto[]): Array<{value: string, label: string}> {
    return lookupData.map(item => {
      // Convert backend karat format (K21) to frontend format (21K) for consistency
      let value = item.name;
      if (item.name.match(/^K\d+$/)) {
        value = item.name.replace(/^K(\d+)$/, '$1K');
      }
      
      return {
        value: value,
        label: item.displayName
      };
    });
  }

  static lookupToSelectOptionsWithValues(lookupData: EnumLookupDto[]): Array<{value: number, label: string, code: string}> {
    return lookupData.map(item => ({
      value: item.value,
      label: item.displayName,
      code: item.name
    }));
  }

  // Helper to convert frontend karat format to backend format
  static karatToBackendFormat(karatString: string): string {
    // Convert "21K" to "K21" format for backend
    if (karatString.match(/^\d+K$/)) {
      return karatString.replace(/^(\d+)K$/, 'K$1');
    }
    return karatString;
  }
}

// Legacy mappings for existing UI components that use different category names
export const LegacyCategoryMapping = {
  // Map backend ProductCategoryType to legacy UI category names
  backendToLegacy: new Map<ProductCategoryTypeString, string[]>([
    ['GoldJewelry', ['Ring', 'Chain', 'Necklace', 'Earrings', 'Bangles', 'Bracelet', 'Other']],
    ['Bullion', ['Bullion']],
    ['Coins', ['Coins']]
  ]),
  
  // Map legacy UI category names to backend ProductCategoryType
  legacyToBackend: new Map<string, ProductCategoryTypeString>([
    ['Ring', 'GoldJewelry'],
    ['Chain', 'GoldJewelry'],
    ['Necklace', 'GoldJewelry'],
    ['Earrings', 'GoldJewelry'],
    ['Bangles', 'GoldJewelry'],
    ['Bracelet', 'GoldJewelry'],
    ['Other', 'GoldJewelry'],
    ['Bullion', 'Bullion'],
    ['Coins', 'Coins']
  ]),

  // Get all legacy categories as a flat array
  getAllLegacyCategories(): string[] {
    const categories: string[] = [];
    this.backendToLegacy.forEach(legacyCategories => {
      categories.push(...legacyCategories);
    });
    return categories;
  },

  // Get backend category for a legacy category name
  getBackendCategory(legacyCategory: string): ProductCategoryTypeString | undefined {
    return this.legacyToBackend.get(legacyCategory);
  },

  // Get legacy categories for a backend category
  getLegacyCategories(backendCategory: ProductCategoryTypeString): string[] {
    return this.backendToLegacy.get(backendCategory) || [];
  }
};

// EnumMapper class for backward compatibility with existing components
export class EnumMapper {
  // Map enum values to display names
  static getDisplayName(lookups: EnumLookupDto[], value: number): string {
    return LookupHelper.getDisplayName(lookups, value);
  }

  // Map enum values to names
  static getName(lookups: EnumLookupDto[], value: number): string {
    return LookupHelper.getName(lookups, value);
  }

  // Map names to enum values
  static getValue(lookups: EnumLookupDto[], name: string): number | undefined {
    return LookupHelper.getValue(lookups, name);
  }

  // Convert lookup data to select options
  static toSelectOptions(lookupData: EnumLookupDto[]): Array<{value: string, label: string}> {
    return LookupHelper.lookupToSelectOptions(lookupData);
  }

  // Convert lookup data to select options with numeric values
  static toSelectOptionsWithValues(lookupData: EnumLookupDto[]): Array<{value: number, label: string, code: string}> {
    return LookupHelper.lookupToSelectOptionsWithValues(lookupData);
  }

  // Product category mapping
  static getProductCategoryName(categoryType: number, lookups: EnumLookupDto[]): string {
    return this.getDisplayName(lookups, categoryType);
  }

  // Karat type mapping
  static getKaratTypeName(karatType: number, lookups: EnumLookupDto[]): string {
    return this.getDisplayName(lookups, karatType);
  }

  // Charge type mapping
  static getChargeTypeName(chargeType: number, lookups: EnumLookupDto[]): string {
    return this.getDisplayName(lookups, chargeType);
  }

  // Transaction type mapping
  static getTransactionTypeName(transactionType: number, lookups: EnumLookupDto[]): string {
    return this.getDisplayName(lookups, transactionType);
  }

  // Transaction status mapping
  static getTransactionStatusName(status: number, lookups: EnumLookupDto[]): string {
    return this.getDisplayName(lookups, status);
  }

  // Payment method mapping
  static getPaymentMethodName(paymentMethod: number, lookups: EnumLookupDto[]): string {
    return this.getDisplayName(lookups, paymentMethod);
  }

  // Lookup to select options (backward compatibility)
  static lookupToSelectOptions(lookupData: EnumLookupDto[]): Array<{value: string, label: string}> {
    return LookupHelper.lookupToSelectOptions(lookupData);
  }

  // Product category enum conversions
  static productCategoryEnumToString(enumValue: number): ProductCategoryTypeString {
    switch (enumValue) {
      case LookupTableConstants.ProductCategoryTypeGoldJewelry:
        return 'GoldJewelry';
      case LookupTableConstants.ProductCategoryTypeBullion:
        return 'Bullion';
      case LookupTableConstants.ProductCategoryTypeGoldCoins:
        return 'Coins';
      default:
        return 'GoldJewelry';
    }
  }

  static productCategoryStringToEnum(stringValue: ProductCategoryTypeString): number {
    switch (stringValue) {
      case 'GoldJewelry':
        return LookupTableConstants.ProductCategoryTypeGoldJewelry;
      case 'Bullion':
        return LookupTableConstants.ProductCategoryTypeBullion;
      case 'Coins':
        return LookupTableConstants.ProductCategoryTypeGoldCoins;
      default:
        return LookupTableConstants.ProductCategoryTypeGoldJewelry;
    }
  }

  // Karat type enum conversions
  static karatEnumToString(enumValue: number): KaratTypeString {
    switch (enumValue) {
      case LookupTableConstants.KaratType18K:
        return '18K';
      case LookupTableConstants.KaratType21K:
        return '21K';
      case LookupTableConstants.KaratType22K:
        return '22K';
      case LookupTableConstants.KaratType24K:
        return '24K';
      default:
        return '22K';
    }
  }

  static karatStringToEnum(stringValue: KaratTypeString): number {
    switch (stringValue) {
      case '18K':
        return LookupTableConstants.KaratType18K;
      case '21K':
        return LookupTableConstants.KaratType21K;
      case '22K':
        return LookupTableConstants.KaratType22K;
      case '24K':
        return LookupTableConstants.KaratType24K;
      default:
        return LookupTableConstants.KaratType22K;
    }
  }

  // Payment method enum conversions
  static paymentMethodEnumToString(enumValue: number): PaymentMethodString {
    switch (enumValue) {
      case LookupTableConstants.PaymentMethodCash:
        return 'Cash';
      default:
        return 'Cash';
    }
  }

  static paymentMethodStringToEnum(stringValue: PaymentMethodString): number {
    switch (stringValue) {
      case 'Cash':
        return LookupTableConstants.PaymentMethodCash;
      case 'Card':
        return 2; // Assuming Card has ID 2
      case 'BankTransfer':
        return 3; // Assuming BankTransfer has ID 3
      case 'Cheque':
        return 4; // Assuming Cheque has ID 4
      default:
        return LookupTableConstants.PaymentMethodCash;
    }
  }
};

/**
 * Enum types and lookup interfaces that match the backend API
 * This file centralizes all enum definitions to avoid hardcoded values throughout the application
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
}

// TypeScript enums that match backend C# enums exactly
export enum TransactionType {
  Sale = 1,
  Return = 2,
  Repair = 3
}

export enum PaymentMethod {
  Cash = 1
}

export enum TransactionStatus {
  Pending = 1,
  Completed = 2,
  Cancelled = 3,
  Refunded = 4,
  Voided = 5
}

export enum ChargeType {
  Percentage = 1,
  FixedAmount = 2
}

export enum KaratType {
  K18 = 18,
  K21 = 21,
  K22 = 22,
  K24 = 24
}

export enum ProductCategoryType {
  GoldJewelry = 1,
  Bullion = 2,
  Coins = 3
}

// String literal types for backward compatibility with existing interfaces
export type TransactionTypeString = 'Sale' | 'Return' | 'Repair';
export type PaymentMethodString = 'Cash' | 'Card' | 'BankTransfer' | 'Cheque';
export type TransactionStatusString = 'Pending' | 'Completed' | 'Cancelled' | 'Refunded' | 'Voided';
export type KaratTypeString = '18K' | '21K' | '22K' | '24K';
export type ProductCategoryTypeString = 'GoldJewelry' | 'Bullion' | 'Coins';

// Mapping functions between frontend display format and backend enum values
export class EnumMapper {
  // Karat type mappings
  static karatEnumToString(karatEnum: KaratType): KaratTypeString {
    switch (karatEnum) {
      case KaratType.K18: return '18K';
      case KaratType.K21: return '21K';
      case KaratType.K22: return '22K';
      case KaratType.K24: return '24K';
      default: throw new Error(`Unknown karat type: ${karatEnum}`);
    }
  }

  static karatStringToEnum(karatString: KaratTypeString): KaratType {
    switch (karatString) {
      case '18K': return KaratType.K18;
      case '21K': return KaratType.K21;
      case '22K': return KaratType.K22;
      case '24K': return KaratType.K24;
      default: throw new Error(`Unknown karat string: ${karatString}`);
    }
  }

  // Transaction type mappings
  static transactionTypeEnumToString(typeEnum: TransactionType): TransactionTypeString {
    switch (typeEnum) {
      case TransactionType.Sale: return 'Sale';
      case TransactionType.Return: return 'Return';
      case TransactionType.Repair: return 'Repair';
      default: throw new Error(`Unknown transaction type: ${typeEnum}`);
    }
  }

  static transactionTypeStringToEnum(typeString: TransactionTypeString): TransactionType {
    switch (typeString) {
      case 'Sale': return TransactionType.Sale;
      case 'Return': return TransactionType.Return;
      case 'Repair': return TransactionType.Repair;
      default: throw new Error(`Unknown transaction type string: ${typeString}`);
    }
  }

  // Payment method mappings
  static paymentMethodEnumToString(methodEnum: PaymentMethod): PaymentMethodString {
    switch (methodEnum) {
      case PaymentMethod.Cash: return 'Cash';
      default: throw new Error(`Unknown payment method: ${methodEnum}`);
    }
  }

  static paymentMethodStringToEnum(methodString: PaymentMethodString): PaymentMethod {
    switch (methodString) {
      case 'Cash': return PaymentMethod.Cash;
      // Note: Card, BankTransfer, Cheque are UI-only for now, defaulting to Cash
      case 'Card':
      case 'BankTransfer': 
      case 'Cheque':
        return PaymentMethod.Cash; // Backend only supports Cash currently
      default: throw new Error(`Unknown payment method string: ${methodString}`);
    }
  }

  // Transaction status mappings
  static transactionStatusEnumToString(statusEnum: TransactionStatus): TransactionStatusString {
    switch (statusEnum) {
      case TransactionStatus.Pending: return 'Pending';
      case TransactionStatus.Completed: return 'Completed';
      case TransactionStatus.Cancelled: return 'Cancelled';
      case TransactionStatus.Refunded: return 'Refunded';
      case TransactionStatus.Voided: return 'Voided';
      default: throw new Error(`Unknown transaction status: ${statusEnum}`);
    }
  }

  static transactionStatusStringToEnum(statusString: TransactionStatusString): TransactionStatus {
    switch (statusString) {
      case 'Pending': return TransactionStatus.Pending;
      case 'Completed': return TransactionStatus.Completed;
      case 'Cancelled': return TransactionStatus.Cancelled;
      case 'Refunded': return TransactionStatus.Refunded;
      case 'Voided': return TransactionStatus.Voided;
      default: throw new Error(`Unknown transaction status string: ${statusString}`);
    }
  }

  // Product category mappings
  static productCategoryEnumToString(categoryEnum: ProductCategoryType): ProductCategoryTypeString {
    switch (categoryEnum) {
      case ProductCategoryType.GoldJewelry: return 'GoldJewelry';
      case ProductCategoryType.Bullion: return 'Bullion';
      case ProductCategoryType.Coins: return 'Coins';
      default: throw new Error(`Unknown product category: ${categoryEnum}`);
    }
  }

  static productCategoryStringToEnum(categoryString: ProductCategoryTypeString): ProductCategoryType {
    switch (categoryString) {
      case 'GoldJewelry': return ProductCategoryType.GoldJewelry;
      case 'Bullion': return ProductCategoryType.Bullion;
      case 'Coins': return ProductCategoryType.Coins;
      default: throw new Error(`Unknown product category string: ${categoryString}`);
    }
  }

  // Helper to convert lookup data to simple arrays for dropdowns
  static lookupToSelectOptions(lookupData: EnumLookupDto[]): Array<{value: string, label: string}> {
    return lookupData.map(item => ({
      value: item.name,
      label: item.displayName
    }));
  }

  static lookupToSelectOptionsWithValues(lookupData: EnumLookupDto[]): Array<{value: number, label: string, code: string}> {
    return lookupData.map(item => ({
      value: item.value,
      label: item.displayName,
      code: item.name
    }));
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

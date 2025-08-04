// Enums matching the backend API

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
  Refunded = 4
}

export enum ChargeType {
  Percentage = 1,
  FixedAmount = 2
}

export enum UserRole {
  Manager = 'Manager',
  Cashier = 'Cashier'
}

// Helper functions for enum display
export class EnumHelper {
  static getKaratTypeDisplay(karat: KaratType): string {
    return `${karat}K Gold`;
  }

  static getProductCategoryDisplay(category: ProductCategoryType): string {
    switch (category) {
      case ProductCategoryType.GoldJewelry:
        return 'Gold Jewelry';
      case ProductCategoryType.Bullion:
        return 'Bullion';
      case ProductCategoryType.Coins:
        return 'Coins';
      default:
        return 'Unknown';
    }
  }

  static getTransactionTypeDisplay(type: TransactionType): string {
    switch (type) {
      case TransactionType.Sale:
        return 'Sale';
      case TransactionType.Return:
        return 'Return';
      case TransactionType.Repair:
        return 'Repair';
      default:
        return 'Unknown';
    }
  }

  static getTransactionStatusDisplay(status: TransactionStatus): string {
    switch (status) {
      case TransactionStatus.Pending:
        return 'Pending';
      case TransactionStatus.Completed:
        return 'Completed';
      case TransactionStatus.Cancelled:
        return 'Cancelled';
      case TransactionStatus.Refunded:
        return 'Refunded';
      default:
        return 'Unknown';
    }
  }

  static getTransactionStatusColor(status: TransactionStatus): string {
    switch (status) {
      case TransactionStatus.Pending:
        return 'warning';
      case TransactionStatus.Completed:
        return 'success';
      case TransactionStatus.Cancelled:
        return 'danger';
      case TransactionStatus.Refunded:
        return 'info';
      default:
        return 'secondary';
    }
  }

  static getChargeTypeDisplay(type: ChargeType): string {
    switch (type) {
      case ChargeType.Percentage:
        return 'Percentage';
      case ChargeType.FixedAmount:
        return 'Fixed Amount';
      default:
        return 'Unknown';
    }
  }
}
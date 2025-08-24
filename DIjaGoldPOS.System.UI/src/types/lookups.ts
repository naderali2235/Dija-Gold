/**
 * TypeScript interfaces for lookup DTOs that match the C# backend DTOs
 */

// Base interface for all lookup DTOs
export interface BaseLookupDto {
  id: number;
  name: string;
  description?: string;
  sortOrder: number;
  isActive: boolean;
}

// Specific lookup DTOs
export interface KaratTypeLookupDto extends BaseLookupDto {}

export interface FinancialTransactionTypeLookupDto extends BaseLookupDto {}

export interface PaymentMethodLookupDto extends BaseLookupDto {}

export interface FinancialTransactionStatusLookupDto extends BaseLookupDto {}

export interface ChargeTypeLookupDto extends BaseLookupDto {}

export interface ProductCategoryTypeLookupDto extends BaseLookupDto {}

export interface RepairStatusLookupDto extends BaseLookupDto {}

export interface RepairPriorityLookupDto extends BaseLookupDto {}

export interface OrderTypeLookupDto extends BaseLookupDto {}

export interface OrderStatusLookupDto extends BaseLookupDto {}

export interface BusinessEntityTypeLookupDto extends BaseLookupDto {}

export interface SubCategoryLookupDto extends BaseLookupDto {}

// Legacy interface for backward compatibility (will be removed)
export interface EnumLookupDto {
  value: number;
  name: string;
  displayName: string;
  code: string;
}

// Helper class for lookup operations
export class LookupHelper {
  static getDisplayName(lookups: BaseLookupDto[], id: number): string {
    const lookup = lookups.find(l => l.id === id);
    return lookup?.name || 'Unknown';
  }

  static getById(lookups: BaseLookupDto[], id: number): BaseLookupDto | undefined {
    return lookups.find(l => l.id === id);
  }

  static getActive(lookups: BaseLookupDto[]): BaseLookupDto[] {
    return lookups.filter(l => l.isActive);
  }

  static getValue(lookups: BaseLookupDto[], name: string): number | undefined {
    const lookup = lookups.find(l => l.name.toLowerCase() === name.toLowerCase());
    return lookup?.id;
  }

  static toSelectOptionsByName(lookups: BaseLookupDto[]): Array<{value: string, label: string}> {
    return lookups.map(lookup => ({
      value: lookup.name,
      label: lookup.name
    }));
  }

  static toSelectOptionsById(lookups: BaseLookupDto[]): Array<{value: number, label: string}> {
    return lookups.map(lookup => ({
      value: lookup.id,
      label: lookup.name
    }));
  }
}

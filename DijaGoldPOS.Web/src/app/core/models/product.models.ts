import { KaratType, ProductCategoryType } from './enums';

export interface Product {
  id: number;
  productCode: string;
  name: string;
  categoryType: ProductCategoryType;
  karatType: KaratType;
  weight: number;
  brand?: string;
  designStyle?: string;
  subCategory?: string;
  shape?: string;
  purityCertificateNumber?: string;
  countryOfOrigin?: string;
  yearOfMinting?: number;
  faceValue?: number;
  hasNumismaticValue?: boolean;
  makingChargesApplicable: boolean;
  supplierId?: number;
  supplierName?: string;
  createdAt: string;
  isActive: boolean;
}

export interface CreateProductRequest {
  productCode: string;
  name: string;
  categoryType: ProductCategoryType;
  karatType: KaratType;
  weight: number;
  brand?: string;
  designStyle?: string;
  subCategory?: string;
  shape?: string;
  purityCertificateNumber?: string;
  countryOfOrigin?: string;
  yearOfMinting?: number;
  faceValue?: number;
  hasNumismaticValue?: boolean;
  makingChargesApplicable: boolean;
  supplierId?: number;
}

export interface UpdateProductRequest extends CreateProductRequest {
  id: number;
}

export interface ProductSearchRequest {
  searchTerm?: string;
  categoryType?: ProductCategoryType;
  karatType?: KaratType;
  brand?: string;
  subCategory?: string;
  supplierId?: number;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface ProductWithInventory extends Product {
  inventory: ProductInventory[];
  totalQuantityOnHand: number;
  totalWeightOnHand: number;
}

export interface ProductInventory {
  branchId: number;
  branchName: string;
  quantityOnHand: number;
  weightOnHand: number;
  minimumStockLevel: number;
  isLowStock: boolean;
}

export interface ProductPricing {
  productId: number;
  productName: string;
  currentGoldRate: number;
  estimatedBasePrice: number;
  estimatedMakingCharges: number;
  estimatedTotalPrice: number;
  priceCalculatedAt: string;
}

export interface PriceCalculationRequest {
  productId: number;
  quantity?: number;
  customerId?: number;
}

export interface PriceCalculationResult {
  productId: number;
  quantity: number;
  goldValue: number;
  makingChargesAmount: number;
  subTotal: number;
  discountAmount: number;
  taxableAmount: number;
  taxes: TaxCalculation[];
  totalTaxAmount: number;
  finalTotal: number;
  calculatedAt: string;
}

export interface TaxCalculation {
  taxName: string;
  taxRate: number;
  taxableAmount: number;
  taxAmount: number;
}
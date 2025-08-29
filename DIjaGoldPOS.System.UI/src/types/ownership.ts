/**
 * TypeScript interfaces for Ownership Consolidation and Weighted Average Costing
 */

// Ownership Consolidation Types
export interface ConsolidationResult {
  success: boolean;
  message: string;
  consolidatedOwnershipId: number;
  originalOwnershipIds: number[];
  totalQuantity: number;
  weightedAverageCost: number;
  totalValue: number;
}

export interface ConsolidationOpportunity {
  productId: number;
  productName: string;
  supplierId: number;
  supplierName: string;
  branchId: number;
  branchName: string;
  ownershipRecords: OwnershipRecord[];
  potentialSavings: number;
  totalQuantity: number;
  averageCost: number;
}

export interface OwnershipRecord {
  id: number;
  productId: number;
  supplierId: number;
  branchId: number;
  quantity: number;
  unitCost: number;
  totalValue: number;
  purchaseDate: string;
  purchaseOrderId?: number;
}

export interface WeightedAverageCost {
  totalQuantity: number;
  totalValue: number;
  weightedAverageCost: number;
  costBreakdown: CostBreakdown[];
}

export interface CostBreakdown {
  ownershipId: number;
  quantity: number;
  unitCost: number;
  totalValue: number;
  weightContribution: number;
}

// Weighted Average Costing Types
export interface WeightedAverageCostResult {
  productId: number;
  productName: string;
  branchId: number;
  branchName: string;
  totalQuantity: number;
  totalValue: number;
  weightedAverageCost: number;
  costSources: CostSource[];
  calculationDate: string;
}

export interface CostSource {
  sourceType: 'PurchaseOrder' | 'Manufacturing' | 'Transfer';
  sourceId: number;
  quantity: number;
  unitCost: number;
  totalValue: number;
  weight: number;
  date: string;
}

export interface ProductCostAnalysis {
  productId: number;
  productName: string;
  branchId: number;
  branchName: string;
  currentCost: number;
  weightedAverageCost: number;
  fifoCost: number;
  lifoCost: number;
  recommendedCostingMethod: 'WeightedAverage' | 'FIFO' | 'LIFO';
  costVariance: number;
  costVariancePercentage: number;
  totalQuantityAvailable: number;
  totalValue: number;
}

export interface FifoCostResult {
  productId: number;
  branchId: number;
  requestedQuantity: number;
  availableQuantity: number;
  fifoCost: number;
  costLayers: CostLayer[];
  totalCost: number;
}

export interface LifoCostResult {
  productId: number;
  branchId: number;
  requestedQuantity: number;
  availableQuantity: number;
  lifoCost: number;
  costLayers: CostLayer[];
  totalCost: number;
}

export interface CostLayer {
  ownershipId: number;
  quantity: number;
  unitCost: number;
  totalValue: number;
  date: string;
  sourceType: string;
  sourceId: number;
}

// Manufacturing Raw Materials Types
export interface ProductManufactureRawMaterial {
  id: number;
  productManufactureId: number;
  productId: number;
  productName: string;
  quantityUsed: number;
  unitCost: number;
  totalCost: number;
  contributionPercentage: number;
  sourceOwnershipId: number;
  sourceType: 'PurchaseOrder' | 'Manufacturing' | 'Transfer';
  sourceId: number;
  notes?: string;
}

export interface CreateProductManufactureRawMaterialDto {
  productId: number;
  quantityUsed: number;
  unitCost: number;
  contributionPercentage: number;
  sourceOwnershipId: number;
  sourceType: 'PurchaseOrder' | 'Manufacturing' | 'Transfer';
  sourceId: number;
  notes?: string;
}

export interface EnhancedProductManufactureDto {
  id: number;
  productId: number;
  productName: string;
  branchId: number;
  branchName: string;
  quantityProduced: number;
  unitCost: number;
  totalCost: number;
  laborCost: number;
  overheadCost: number;
  rawMaterialCost: number;
  manufacturingDate: string;
  status: string;
  notes?: string;
  rawMaterials: ProductManufactureRawMaterial[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateEnhancedProductManufactureDto {
  productId: number;
  branchId: number;
  quantityProduced: number;
  laborCost: number;
  overheadCost: number;
  manufacturingDate: string;
  notes?: string;
  rawMaterials: CreateProductManufactureRawMaterialDto[];
}

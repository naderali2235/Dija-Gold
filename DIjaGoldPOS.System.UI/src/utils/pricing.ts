import { Product, Customer, ProductPricingDto } from '../services/api';
import { GoldRate, MakingCharges } from '../services/api';
import { EnumMapper, ProductCategoryType, KaratType } from '../types/enums';
import api from '../services/api';

export interface PricingCalculation {
  goldValue: number;
  makingChargesAmount: number;
  totalPrice: number;
  goldRate: number;
  makingChargesRate?: number;
  makingChargesType?: 'percentage' | 'fixed';
  discountAmount: number;
  subtotal: number;
  taxableAmount: number;
  totalTaxAmount: number;
  finalTotal: number;
}

/**
 * Calculate product pricing using real API data
 * This matches the backend PricingService calculation logic
 */
export function calculateProductPricing(
  product: Product,
  goldRates: GoldRate[],
  makingCharges: MakingCharges[],
  quantity: number = 1,
  customer?: Customer | null,
  taxConfigurations?: any[]
): PricingCalculation {
  // Check if gold rates are loaded - return default values if not
  if (!goldRates || goldRates.length === 0) {
    return {
      goldValue: 0,
      makingChargesAmount: 0,
      totalPrice: 0,
      goldRate: 0,
      makingChargesRate: undefined,
      makingChargesType: undefined,
      discountAmount: 0,
      subtotal: 0,
      taxableAmount: 0,
      totalTaxAmount: 0,
      finalTotal: 0
    };
  }

  // Find the current gold rate for the product's karat type
  // Filter by EffectiveTo == null to match backend logic
  const goldRateData = goldRates.find(rate => 
    rate.karatType === product.karatType && 
    rate.effectiveTo == null
  );
  
  if (!goldRateData) {
    // Instead of throwing an error, return default values
    // This prevents the initial render errors when data is still loading
    return {
      goldValue: 0,
      makingChargesAmount: 0,
      totalPrice: 0,
      goldRate: 0,
      makingChargesRate: undefined,
      makingChargesType: undefined,
      discountAmount: 0,
      subtotal: 0,
      taxableAmount: 0,
      totalTaxAmount: 0,
      finalTotal: 0
    };
  }

  const goldRate = goldRateData.ratePerGram;
  const totalWeight = product.weight * quantity;
  const goldValue = totalWeight * goldRate;

  let makingChargesAmount = 0;
  let makingChargesRate: number | undefined;
  let makingChargesType: 'percentage' | 'fixed' | undefined;

  // Calculate making charges if applicable
  if (product.makingChargesApplicable) {
    // Check if product has specific making charges defined
    if (product.useProductMakingCharges && 
        product.productMakingChargesTypeId && 
        product.productMakingChargesValue) {
      
      // Use product-specific making charges
      const chargeType = product.productMakingChargesTypeId;
      makingChargesRate = product.productMakingChargesValue;
      makingChargesType = chargeType === 1 ? 'percentage' : 'fixed';

      if (chargeType === 1) { // Percentage
        makingChargesAmount = goldValue * (product.productMakingChargesValue / 100);
      } else { // Fixed amount
        makingChargesAmount = product.productMakingChargesValue * quantity;
      }
    } else {
      // Use pricing-level making charges
      // First try to find specific subcategory match
      let chargesData = makingCharges.find(charge => 
        charge.productCategory === product.categoryType && 
        charge.subCategory === product.subCategory &&
        charge.effectiveTo == null
      );

      // Fallback to general category match (where SubCategory is null)
      if (!chargesData) {
        chargesData = makingCharges.find(charge => 
          charge.productCategory === product.categoryType && 
          !charge.subCategory &&
          charge.effectiveTo == null
        );
      }

      if (chargesData) {
        makingChargesRate = chargesData.chargeValue;
        makingChargesType = chargesData.chargeType === 1 ? 'percentage' : 'fixed';

        if (chargesData.chargeType === 1) { // Percentage
          makingChargesAmount = goldValue * (chargesData.chargeValue / 100);
        } else { // Fixed amount
          makingChargesAmount = chargesData.chargeValue * quantity;
        }
      }
    }
  }

  // Apply customer discounts if applicable
  let discountAmount = 0;
  if (customer) {
    // Apply discount percentage only if making charges are not waived
    if (customer.defaultDiscountPercentage && customer.defaultDiscountPercentage > 0 && !customer.makingChargesWaived) {
      discountAmount = (goldValue + makingChargesAmount) * (customer.defaultDiscountPercentage / 100);
    }
    
    // Waive making charges if customer has that privilege
    if (customer.makingChargesWaived) {
      discountAmount += makingChargesAmount;
    }
    
    // Validate that total discount doesn't exceed making charges
    if (discountAmount > makingChargesAmount) {
      console.warn(`Customer discount ${discountAmount.toFixed(2)} exceeds making charges ${makingChargesAmount.toFixed(2)}. Capping discount to making charges amount.`);
      
      // Cap the discount to the making charges amount
      discountAmount = makingChargesAmount;
    }
  }

  const subtotal = goldValue + makingChargesAmount;
  const taxableAmount = subtotal - discountAmount;

  // Calculate taxes if tax configurations are provided
  let totalTaxAmount = 0;
  if (taxConfigurations) {
    const mandatoryTaxes = taxConfigurations.filter(tax => tax.isMandatory && tax.isCurrent);
    mandatoryTaxes.forEach(taxConfig => {
      if (taxConfig.taxType === 1) { // Percentage
        totalTaxAmount += taxableAmount * (taxConfig.taxRate / 100);
      } else { // Fixed amount
        totalTaxAmount += taxConfig.taxRate * quantity;
      }
    });
  }

  const finalTotal = taxableAmount + totalTaxAmount;

  return {
    goldValue,
    makingChargesAmount,
    totalPrice: taxableAmount, // This is the taxable amount (before taxes)
    goldRate,
    makingChargesRate,
    makingChargesType,
    discountAmount,
    subtotal,
    taxableAmount,
    totalTaxAmount,
    finalTotal // This matches backend's FinalTotal
  };
}

/**
 * Get current gold rate for a specific karat type
 */
export function getCurrentGoldRate(goldRates: GoldRate[], karatType: number): number {
  const rateData = goldRates.find(rate => 
    rate.karatType === karatType && 
    rate.effectiveTo == null
  );
  return rateData?.ratePerGram || 0;
}

/**
 * Get current making charges for a product category and subcategory
 */
export function getCurrentMakingCharges(
  makingCharges: MakingCharges[], 
  categoryType: number, 
  subCategory?: string
): MakingCharges | undefined {
  // First try specific subcategory match
  let charges = makingCharges.find(charge => 
    charge.productCategory === categoryType && 
    charge.subCategory === subCategory &&
    charge.isCurrent
  );

  // Fallback to general category match
  if (!charges) {
    charges = makingCharges.find(charge => 
      charge.productCategory === categoryType && 
      !charge.subCategory &&
      charge.isCurrent
    );
  }

  return charges;
}

/**
 * Format making charges display text
 */
export function formatMakingCharges(charges: MakingCharges): string {
  if (charges.chargeType === 1) { // Percentage
    return `${charges.chargeValue}%`;
  } else { // Fixed
    return `${charges.chargeValue.toFixed(2)} EGP`;
  }
}

/**
 * Get accurate pricing from backend API
 * This ensures consistency with backend calculations
 */
export async function getProductPricingFromAPI(
  productId: number, 
  quantity: number = 1, 
  customerId?: number
): Promise<ProductPricingDto> {
  try {
    return await api.products.getProductPricing(productId, quantity, customerId);
  } catch (error) {
    console.error('Error fetching pricing from API:', error);
    throw error;
  }
}

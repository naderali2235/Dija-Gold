using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Models;

using DijaGoldPOS.API.Shared;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Pricing calculation service implementation
/// </summary>
public class PricingService : IPricingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PricingService> _logger;
    private readonly IAuditService _auditService;
    private readonly IConfiguration _configuration;

    public PricingService(
        ApplicationDbContext context, 
        ILogger<PricingService> logger,
        IAuditService auditService,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _auditService = auditService;
        _configuration = configuration;
    }

    /// <summary>
    /// Calculate price for a product based on current rates
    /// Formula: Final Price = (Gold Weight × Daily Rate × Karat Factor) + Making Charges + Taxes - Discounts
    /// </summary>
    public async Task<PriceCalculationResult> CalculatePriceAsync(Product product, decimal quantity, int? customerId = null)
    {
        try
        {
            var result = new PriceCalculationResult();
            
            // Get current gold rate
            var goldRate = await GetCurrentGoldRateAsync(product.KaratTypeId);
            if (goldRate == null)
            {
                throw new InvalidOperationException($"No current gold rate found for {product.KaratTypeId}");
            }
            result.GoldRateUsed = goldRate;

            // Calculate total weight
            var totalWeight = product.Weight * quantity;
            
            // Calculate gold value
            result.GoldValue = totalWeight * goldRate.RatePerGram;

            // Calculate making charges if applicable
            if (product.MakingChargesApplicable)
            {
                // Check if product has specific making charges defined
                if (product.UseProductMakingCharges && 
                    product.ProductMakingChargesTypeId.HasValue && 
                    product.ProductMakingChargesValue.HasValue)
                {
                    // Use product-specific making charges
                    var chargeTypeId = product.ProductMakingChargesTypeId.Value;
                    
                    if (chargeTypeId == LookupTableConstants.ChargeTypePercentage)
                    {
                        result.MakingChargesAmount = result.GoldValue * (product.ProductMakingChargesValue.Value / 100m);
                    }
                    else // Fixed amount
                    {
                        result.MakingChargesAmount = product.ProductMakingChargesValue.Value * quantity;
                    }
                    
                    _logger.LogInformation("Using product-specific making charges: {ChargeTypeId} {ChargeValue}", 
                        chargeTypeId, product.ProductMakingChargesValue.Value);
                }
                else
                {
                    // Use pricing-level making charges
                    var makingCharges = await GetCurrentMakingChargesAsync(product.CategoryTypeId, product.SubCategoryId);
                    if (makingCharges != null)
                    {
                        result.MakingChargesUsed = makingCharges;
                        
                        if (makingCharges.ChargeTypeId == LookupTableConstants.ChargeTypePercentage)
                        {
                            result.MakingChargesAmount = result.GoldValue * (makingCharges.ChargeValue / 100m);
                        }
                        else // Fixed amount
                        {
                            result.MakingChargesAmount = makingCharges.ChargeValue * quantity;
                        }
                        
                        _logger.LogInformation("Using pricing-level making charges: {ChargeTypeId} {ChargeValue}", 
                            makingCharges.ChargeTypeId, makingCharges.ChargeValue);
                    }
                }
            }

            // Calculate subtotal
            result.SubTotal = result.GoldValue + result.MakingChargesAmount;

            // Apply customer loyalty discount if applicable
            // 
            // BUSINESS RULES FOR CUSTOMER DISCOUNTS:
            // 1. Percentage Discount: Applied to subtotal (Gold Value + Making Charges)
            // 2. Making Charges Waiver: Completely waives making charges (VIP benefit)
            // 3. Mutual Exclusivity: If making charges are waived, no percentage discount is applied
            // 4. Discount Cap: Total discount cannot exceed making charges amount
            // 5. Configurable Rules: Both rules can be enabled/disabled via configuration
            //
            // EXAMPLES:
            // - Gold Value: 5000 EGP, Making Charges: 500 EGP, Customer has 10% discount
            //   Result: Discount = 550 EGP (10% of 5500), but capped to 500 EGP (making charges)
            // - Gold Value: 5000 EGP, Making Charges: 500 EGP, Customer has making charges waived
            //   Result: Discount = 500 EGP (making charges waived), no percentage discount applied
            // - Gold Value: 5000 EGP, Making Charges: 500 EGP, Customer has 5% discount + making charges waived
            //   Result: Discount = 500 EGP (only making charges waived, no percentage discount)
            //
            Customer? customer = null;
            if (customerId.HasValue)
            {
                customer = await _context.Customers.FindAsync(customerId.Value);
                result.CustomerInfo = customer;
                
                if (customer != null)
                {
                    // Get business rule configurations
                    var preventPercentageDiscountWhenMakingChargesWaived = _configuration.GetValue<bool>("BusinessRules:PreventPercentageDiscountWhenMakingChargesWaived", true);
                    var capDiscountToMakingCharges = _configuration.GetValue<bool>("BusinessRules:CapDiscountToMakingCharges", true);
                    
                    // Apply discount percentage only if making charges are not waived (configurable rule)
                    // This prevents double benefits: either percentage discount OR making charges waiver, not both
                    if (customer.DefaultDiscountPercentage > 0 && (!preventPercentageDiscountWhenMakingChargesWaived || !customer.MakingChargesWaived))
                    {
                        result.DiscountAmount = result.SubTotal * (customer.DefaultDiscountPercentage / 100m);
                    }
                    
                    // Waive making charges if customer has that privilege
                    // This is typically for VIP customers who get making charges completely waived
                    if (customer.MakingChargesWaived)
                    {
                        result.DiscountAmount += result.MakingChargesAmount;
                    }
                    
                    // Validate that total discount doesn't exceed making charges (configurable rule)
                    // This ensures discounts don't exceed the value of making charges, protecting profitability
                    if (capDiscountToMakingCharges && result.DiscountAmount > result.MakingChargesAmount)
                    {
                        _logger.LogWarning("Customer discount {DiscountAmount:F2} exceeds making charges {MakingCharges:F2} for customer {CustomerId}. Capping discount to making charges amount.", 
                            result.DiscountAmount, result.MakingChargesAmount, customer.Id);
                        
                        // Cap the discount to the making charges amount
                        result.DiscountAmount = result.MakingChargesAmount;
                        
                        // Log this as a business rule violation for audit purposes
                        await _auditService.LogAsync(
                            "SYSTEM",
                            "PRICING_VALIDATION",
                            "CustomerDiscount",
                            customer.Id.ToString(),
                            $"Discount capped from {result.DiscountAmount:F2} to {result.MakingChargesAmount:F2} (making charges amount) for customer {customer.FullName}"
                        );
                    }
                }
            }

            // Calculate taxable amount
            result.TaxableAmount = result.SubTotal - result.DiscountAmount;

            // Calculate taxes
            var taxConfigurations = await GetCurrentTaxConfigurationsAsync();
            foreach (var taxConfig in taxConfigurations.Where(t => t.IsMandatory))
            {
                var taxCalculation = new TaxCalculation
                {
                    TaxConfigurationId = taxConfig.Id,
                    TaxName = taxConfig.TaxName,
                    TaxRate = taxConfig.TaxRate,
                    TaxableAmount = result.TaxableAmount
                };

                if (taxConfig.TaxTypeId == LookupTableConstants.ChargeTypePercentage)
                {
                    taxCalculation.TaxAmount = result.TaxableAmount * (taxConfig.TaxRate / 100m);
                }
                else // Fixed amount
                {
                    taxCalculation.TaxAmount = taxConfig.TaxRate * quantity;
                }

                result.Taxes.Add(taxCalculation);
                result.TotalTaxAmount += taxCalculation.TaxAmount;
            }

            // Calculate final total
            result.FinalTotal = result.TaxableAmount + result.TotalTaxAmount;

            _logger.LogInformation("=== PRICING SERVICE DEBUG for {ProductCode} ===", product.ProductCode);
            _logger.LogInformation("Gold Value: {GoldValue:F2} (Weight: {Weight:F3}g × Rate: {Rate:F2})", 
                result.GoldValue, totalWeight, goldRate);
            _logger.LogInformation("Making Charges: {MakingCharges:F2}", result.MakingChargesAmount);
            _logger.LogInformation("SubTotal: {SubTotal:F2}", result.SubTotal);
            _logger.LogInformation("Customer Discount: {Discount:F2}", result.DiscountAmount);
            _logger.LogInformation("Taxable Amount: {TaxableAmount:F2}", result.TaxableAmount);
            _logger.LogInformation("Total Tax: {TotalTax:F2}", result.TotalTaxAmount);
            _logger.LogInformation("Final Total: {FinalTotal:F2}", result.FinalTotal);
            _logger.LogInformation("=== END PRICING DEBUG ===");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating price for product {ProductId}", product.Id);
            throw;
        }
    }

    /// <summary>
    /// Get current gold rate for specific karat type
    /// </summary>
    public async Task<GoldRate?> GetCurrentGoldRateAsync(int karatTypeId)
    {
        return await _context.GoldRates
            .Where(gr => gr.KaratTypeId == karatTypeId && gr.EffectiveTo == null)
            .OrderByDescending(gr => gr.EffectiveFrom)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get current making charges for product category
    /// </summary>
    public async Task<MakingCharges?> GetCurrentMakingChargesAsync(int categoryTypeId, int? subCategoryId = null)
    {
        var query = _context.MakingCharges
            .Where(mc => mc.ProductCategoryId == categoryTypeId && mc.IsCurrent);

        // First try to find specific subcategory match
        if (subCategoryId.HasValue)
        {
            var specificMatch = await query
                .Where(mc => mc.SubCategoryId == subCategoryId.Value)
                .OrderByDescending(mc => mc.EffectiveFrom)
                .FirstOrDefaultAsync();
            
            if (specificMatch != null)
                return specificMatch;
        }

        // Fallback to general category match (where SubCategoryId is null)
        return await query
            .Where(mc => mc.SubCategoryId == null)
            .OrderByDescending(mc => mc.EffectiveFrom)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get current tax configurations
    /// </summary>
    public async Task<List<TaxConfiguration>> GetCurrentTaxConfigurationsAsync()
    {
        return await _context.TaxConfigurations
            .Where(tc => tc.IsCurrent)
            .OrderBy(tc => tc.DisplayOrder)
            .ToListAsync();
    }

    /// <summary>
    /// Update gold rates (Manager only)
    /// </summary>
    public async Task<bool> UpdateGoldRatesAsync(List<GoldRateUpdate> goldRateUpdates, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var update in goldRateUpdates)
            {
                // End current rate
                var currentRates = await _context.GoldRates
                    .Where(gr => gr.KaratTypeId == update.KaratTypeId && gr.IsCurrent)
                    .ToListAsync();

                foreach (var currentRate in currentRates)
                {
                    currentRate.IsCurrent = false;
                    currentRate.EffectiveTo = update.EffectiveFrom.AddMinutes(-1);
                    currentRate.ModifiedBy = userId;
                    currentRate.ModifiedAt = DateTime.UtcNow;
                }

                // Create new rate
                var newRate = new GoldRate
                {
                    KaratTypeId = update.KaratTypeId,
                    RatePerGram = update.RatePerGram,
                    EffectiveFrom = update.EffectiveFrom,
                    IsCurrent = true,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.GoldRates.AddAsync(newRate);

                // Log audit trail
                await _auditService.LogAsync(
                    userId,
                    "UPDATE_GOLD_RATE",
                    "GoldRate",
                    newRate.Id.ToString(),
                    $"Updated gold rate for karat type {update.KaratTypeId} to {update.RatePerGram} EGP/gram",
                    oldValues: currentRates.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(currentRates.First()) : null,
                    newValues: System.Text.Json.JsonSerializer.Serialize(newRate)
                );
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Gold rates updated by user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating gold rates for user {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Update making charges (Manager only)
    /// </summary>
    public async Task<bool> UpdateMakingChargesAsync(MakingChargesUpdate update, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (update.Id.HasValue)
            {
                // Update existing
                var existing = await _context.MakingCharges.FindAsync(update.Id.Value);
                if (existing == null)
                    return false;

                // End current version
                existing.IsCurrent = false;
                existing.EffectiveTo = update.EffectiveFrom.AddMinutes(-1);
                existing.ModifiedBy = userId;
                existing.ModifiedAt = DateTime.UtcNow;
            }
            else
            {
                // End any current charges for the same category/subcategory
                var currentCharges = await _context.MakingCharges
                    .Where(mc => mc.ProductCategoryId == update.ProductCategoryId 
                              && mc.SubCategoryId == update.SubCategoryId 
                              && mc.IsCurrent)
                    .ToListAsync();

                foreach (var currentCharge in currentCharges)
                {
                    currentCharge.IsCurrent = false;
                    currentCharge.EffectiveTo = update.EffectiveFrom.AddMinutes(-1);
                    currentCharge.ModifiedBy = userId;
                    currentCharge.ModifiedAt = DateTime.UtcNow;
                }
            }

            // Create new version
            var newCharges = new MakingCharges
            {
                Name = update.Name,
                ProductCategoryId = update.ProductCategoryId,
                SubCategoryId = update.SubCategoryId,
                SubCategory = update.SubCategory, // Keep for backward compatibility
                ChargeTypeId = update.ChargeTypeId,
                ChargeValue = update.ChargeValue,
                EffectiveFrom = update.EffectiveFrom,
                IsCurrent = true,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.MakingCharges.AddAsync(newCharges);

            // Log audit trail
            await _auditService.LogAsync(
                userId,
                "UPDATE_MAKING_CHARGES",
                "MakingCharges",
                newCharges.Id.ToString(),
                $"Updated making charges for product category {update.ProductCategoryId}",
                newValues: System.Text.Json.JsonSerializer.Serialize(newCharges)
            );

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Making charges updated by user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating making charges for user {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Update tax configuration (Manager only)
    /// </summary>
    public async Task<bool> UpdateTaxConfigurationAsync(TaxConfigurationUpdate update, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Business logic validation: Check for overlapping effective dates for the same tax code
            var overlappingConfigs = await _context.TaxConfigurations
                .Where(tc => tc.TaxCode == update.TaxCode && 
                           tc.Id != (update.Id ?? 0) && // Exclude current record if updating
                           tc.EffectiveTo == null && // Only check current/active configurations
                           tc.EffectiveFrom <= update.EffectiveFrom) // Check for overlap
                .ToListAsync();

            if (overlappingConfigs.Any())
            {
                _logger.LogWarning("Tax code {TaxCode} already has an active configuration with overlapping effective dates", update.TaxCode);
                return false;
            }

            if (update.Id.HasValue)
            {
                // Update existing configuration
                var existing = await _context.TaxConfigurations.FindAsync(update.Id.Value);
                if (existing == null)
                    return false;

                // Store old values for audit
                var oldValues = System.Text.Json.JsonSerializer.Serialize(existing);

                // Update the existing configuration
                existing.TaxName = update.TaxName;
                existing.TaxTypeId = update.TaxTypeId;
                existing.TaxRate = update.TaxRate;
                existing.IsMandatory = update.IsMandatory;
                existing.EffectiveFrom = update.EffectiveFrom;
                existing.DisplayOrder = update.DisplayOrder;
                existing.ModifiedBy = userId;
                existing.ModifiedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Log audit trail
                await _auditService.LogAsync(
                    userId,
                    "UPDATE_TAX_CONFIGURATION",
                    "TaxConfiguration",
                    existing.Id.ToString(),
                    $"Updated tax configuration for {update.TaxName} ({update.TaxCode})",
                    oldValues: oldValues,
                    newValues: System.Text.Json.JsonSerializer.Serialize(existing)
                );
            }
            else
            {
                // Create new configuration
                var newConfig = new TaxConfiguration
                {
                    TaxName = update.TaxName,
                    TaxCode = update.TaxCode,
                    TaxTypeId = update.TaxTypeId,
                    TaxRate = update.TaxRate,
                    IsMandatory = update.IsMandatory,
                    EffectiveFrom = update.EffectiveFrom,
                    DisplayOrder = update.DisplayOrder,
                    IsCurrent = true,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.TaxConfigurations.AddAsync(newConfig);
                await _context.SaveChangesAsync();

                // Log audit trail
                await _auditService.LogAsync(
                    userId,
                    "CREATE_TAX_CONFIGURATION",
                    "TaxConfiguration",
                    newConfig.Id.ToString(),
                    $"Created new tax configuration for {update.TaxName} ({update.TaxCode})",
                    newValues: System.Text.Json.JsonSerializer.Serialize(newConfig)
                );
            }

            await transaction.CommitAsync();

            _logger.LogInformation("Tax configuration updated by user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating tax configuration for user {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Create new version of tax configuration (Manager only)
    /// </summary>
    public async Task<bool> CreateTaxConfigurationVersionAsync(TaxConfigurationUpdate update, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // End current active configuration with the same tax code
            var currentConfigs = await _context.TaxConfigurations
                .Where(tc => tc.TaxCode == update.TaxCode && tc.IsCurrent)
                .ToListAsync();

            foreach (var currentConfig in currentConfigs)
            {
                currentConfig.IsCurrent = false;
                currentConfig.EffectiveTo = update.EffectiveFrom.AddMinutes(-1);
                currentConfig.ModifiedBy = userId;
                currentConfig.ModifiedAt = DateTime.UtcNow;
            }

            // Create new version
            var newConfig = new TaxConfiguration
            {
                TaxName = update.TaxName,
                TaxCode = update.TaxCode,
                TaxTypeId = update.TaxTypeId,
                TaxRate = update.TaxRate,
                IsMandatory = update.IsMandatory,
                EffectiveFrom = update.EffectiveFrom,
                DisplayOrder = update.DisplayOrder,
                IsCurrent = true,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.TaxConfigurations.AddAsync(newConfig);
            await _context.SaveChangesAsync();

            // Log audit trail
            await _auditService.LogAsync(
                userId,
                "CREATE_TAX_CONFIGURATION_VERSION",
                "TaxConfiguration",
                newConfig.Id.ToString(),
                $"Created new version of tax configuration for {update.TaxName} ({update.TaxCode})",
                newValues: System.Text.Json.JsonSerializer.Serialize(newConfig)
            );

            await transaction.CommitAsync();

            _logger.LogInformation("New tax configuration version created by user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating tax configuration version for user {UserId}", userId);
            return false;
        }
    }
}

using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Models.Enums;
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

    public PricingService(
        ApplicationDbContext context, 
        ILogger<PricingService> logger,
        IAuditService auditService)
    {
        _context = context;
        _logger = logger;
        _auditService = auditService;
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
            var goldRate = await GetCurrentGoldRateAsync(product.KaratType);
            if (goldRate == null)
            {
                throw new InvalidOperationException($"No current gold rate found for {product.KaratType}");
            }
            result.GoldRateUsed = goldRate;

            // Calculate total weight
            var totalWeight = product.Weight * quantity;
            
            // Calculate gold value
            result.GoldValue = totalWeight * goldRate.RatePerGram;

            // Calculate making charges if applicable
            if (product.MakingChargesApplicable)
            {
                var makingCharges = await GetCurrentMakingChargesAsync(product.CategoryType, product.SubCategory);
                if (makingCharges != null)
                {
                    result.MakingChargesUsed = makingCharges;
                    
                    if (makingCharges.ChargeType == ChargeType.Percentage)
                    {
                        result.MakingChargesAmount = result.GoldValue * (makingCharges.ChargeValue / 100m);
                    }
                    else // Fixed amount
                    {
                        result.MakingChargesAmount = makingCharges.ChargeValue * quantity;
                    }
                }
            }

            // Calculate subtotal
            result.SubTotal = result.GoldValue + result.MakingChargesAmount;

            // Apply customer loyalty discount if applicable
            Customer? customer = null;
            if (customerId.HasValue)
            {
                customer = await _context.Customers.FindAsync(customerId.Value);
                result.CustomerInfo = customer;
                
                if (customer != null)
                {
                    // Apply discount percentage
                    if (customer.DefaultDiscountPercentage > 0)
                    {
                        result.DiscountAmount = result.SubTotal * (customer.DefaultDiscountPercentage / 100m);
                    }
                    
                    // Waive making charges if customer has that privilege
                    if (customer.MakingChargesWaived)
                    {
                        result.DiscountAmount += result.MakingChargesAmount;
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

                if (taxConfig.TaxType == ChargeType.Percentage)
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

            _logger.LogInformation("Price calculated for product {ProductCode}: {FinalTotal} EGP", 
                product.ProductCode, result.FinalTotal);

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
    public async Task<GoldRate?> GetCurrentGoldRateAsync(KaratType karatType)
    {
        return await _context.GoldRates
            .Where(gr => gr.KaratType == karatType && gr.IsCurrent)
            .OrderByDescending(gr => gr.EffectiveFrom)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get current making charges for product category
    /// </summary>
    public async Task<MakingCharges?> GetCurrentMakingChargesAsync(ProductCategoryType categoryType, string? subCategory = null)
    {
        var query = _context.MakingCharges
            .Where(mc => mc.ProductCategory == categoryType && mc.IsCurrent);

        // First try to find specific subcategory match
        if (!string.IsNullOrEmpty(subCategory))
        {
            var specificMatch = await query
                .Where(mc => mc.SubCategory == subCategory)
                .OrderByDescending(mc => mc.EffectiveFrom)
                .FirstOrDefaultAsync();
            
            if (specificMatch != null)
                return specificMatch;
        }

        // Fallback to general category match (where SubCategory is null)
        return await query
            .Where(mc => mc.SubCategory == null)
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
                    .Where(gr => gr.KaratType == update.KaratType && gr.IsCurrent)
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
                    KaratType = update.KaratType,
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
                    $"Updated gold rate for {update.KaratType} to {update.RatePerGram} EGP/gram",
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
                    .Where(mc => mc.ProductCategory == update.ProductCategory 
                              && mc.SubCategory == update.SubCategory 
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
                ProductCategory = update.ProductCategory,
                SubCategory = update.SubCategory,
                ChargeType = update.ChargeType,
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
                $"Updated making charges for {update.ProductCategory}",
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
}
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Models.Enums;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Interface for pricing calculation service
/// </summary>
public interface IPricingService
{
    /// <summary>
    /// Calculate price for a product based on current rates
    /// </summary>
    /// <param name="product">Product to calculate price for</param>
    /// <param name="quantity">Quantity to calculate</param>
    /// <param name="customerId">Customer ID for loyalty discounts</param>
    /// <returns>Price calculation result</returns>
    Task<PriceCalculationResult> CalculatePriceAsync(Product product, decimal quantity, int? customerId = null);
    
    /// <summary>
    /// Get current gold rate for specific karat type
    /// </summary>
    /// <param name="karatType">Karat type</param>
    /// <returns>Current gold rate</returns>
    Task<GoldRate?> GetCurrentGoldRateAsync(KaratType karatType);
    
    /// <summary>
    /// Get current making charges for product category
    /// </summary>
    /// <param name="categoryType">Product category</param>
    /// <param name="subCategory">Sub-category (optional)</param>
    /// <returns>Current making charges</returns>
    Task<MakingCharges?> GetCurrentMakingChargesAsync(ProductCategoryType categoryType, string? subCategory = null);
    
    /// <summary>
    /// Get current tax configurations
    /// </summary>
    /// <returns>List of active tax configurations</returns>
    Task<List<TaxConfiguration>> GetCurrentTaxConfigurationsAsync();
    
    /// <summary>
    /// Update gold rates (Manager only)
    /// </summary>
    /// <param name="goldRateUpdates">List of gold rate updates</param>
    /// <param name="userId">User performing the update</param>
    /// <returns>Success status</returns>
    Task<bool> UpdateGoldRatesAsync(List<GoldRateUpdate> goldRateUpdates, string userId);
    
    /// <summary>
    /// Update making charges (Manager only)
    /// </summary>
    /// <param name="makingChargesUpdate">Making charges update</param>
    /// <param name="userId">User performing the update</param>
    /// <returns>Success status</returns>
    Task<bool> UpdateMakingChargesAsync(MakingChargesUpdate makingChargesUpdate, string userId);
    
    /// <summary>
    /// Update tax configuration (Manager only)
    /// </summary>
    /// <param name="taxConfigurationUpdate">Tax configuration update</param>
    /// <param name="userId">User performing the update</param>
    /// <returns>Success status</returns>
    Task<bool> UpdateTaxConfigurationAsync(TaxConfigurationUpdate taxConfigurationUpdate, string userId);

    /// <summary>
    /// Create new version of tax configuration (Manager only)
    /// </summary>
    /// <param name="taxConfigurationUpdate">Tax configuration update</param>
    /// <param name="userId">User performing the update</param>
    /// <returns>Success status</returns>
    Task<bool> CreateTaxConfigurationVersionAsync(TaxConfigurationUpdate taxConfigurationUpdate, string userId);
}

/// <summary>
/// Price calculation result
/// </summary>
public class PriceCalculationResult
{
    public decimal GoldValue { get; set; }
    public decimal MakingChargesAmount { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public List<TaxCalculation> Taxes { get; set; } = new();
    public decimal TotalTaxAmount { get; set; }
    public decimal FinalTotal { get; set; }
    public GoldRate? GoldRateUsed { get; set; }
    public MakingCharges? MakingChargesUsed { get; set; }
    public Customer? CustomerInfo { get; set; }
}

/// <summary>
/// Tax calculation detail
/// </summary>
public class TaxCalculation
{
    public int TaxConfigurationId { get; set; }
    public string TaxName { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
}

/// <summary>
/// Gold rate update model
/// </summary>
public class GoldRateUpdate
{
    public KaratType KaratType { get; set; }
    public decimal RatePerGram { get; set; }
    public DateTime EffectiveFrom { get; set; }
}

/// <summary>
/// Making charges update model
/// </summary>
public class MakingChargesUpdate
{
    public int? Id { get; set; } // Null for new, Id for update
    public string Name { get; set; } = string.Empty;
    public ProductCategoryType ProductCategory { get; set; }
    public string? SubCategory { get; set; }
    public ChargeType ChargeType { get; set; }
    public decimal ChargeValue { get; set; }
    public DateTime EffectiveFrom { get; set; }
}

/// <summary>
/// Tax configuration update model
/// </summary>
public class TaxConfigurationUpdate
{
    public int? Id { get; set; } // Null for new, Id for update
    public string TaxName { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public ChargeType TaxType { get; set; }
    public decimal TaxRate { get; set; }
    public bool IsMandatory { get; set; } = true;
    public DateTime EffectiveFrom { get; set; }
    public int DisplayOrder { get; set; } = 1;
}
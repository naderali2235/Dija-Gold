namespace DijaGoldPOS.API.Models.Enums;

/// <summary>
/// Represents the different product categories in the gold retail business
/// </summary>
public enum ProductCategoryType
{
    /// <summary>Gold Jewelry - rings, necklaces, bracelets, earrings, etc.</summary>
    GoldJewelry = 1,
    
    /// <summary>Bullion - bars, ingots (0.25g to 100g)</summary>
    Bullion = 2,
    
    /// <summary>Coins - gold coins (0.25g to 1g)</summary>
    Coins = 3
}
using DijaGoldPOS.API.Models.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models.LookupModels;

/// <summary>
/// Lookup table for business entity types (Customer, Supplier, Branch)
/// </summary>
[Table("BusinessEntityTypes", Schema = "Lookup")]
public class BusinessEntityTypeLookup : BaseEntity
{
    /// <summary>
    /// Entity type code (CUSTOMER, SUPPLIER, BRANCH)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Whether this entity type can make purchases
    /// </summary>
    public bool CanPurchase { get; set; } = false;

    /// <summary>
    /// Whether this entity type can sell
    /// </summary>
    public bool CanSell { get; set; } = false;

    /// <summary>
    /// Whether this entity type has credit limits
    /// </summary>
    public bool HasCreditLimit { get; set; } = false;
    public string Description { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// Lookup table for payment methods (Cash, Card, Bank Transfer, etc.)
/// </summary>
[Table("PaymentMethods", Schema = "Lookup")]
public class PaymentMethodLookup : BaseEntity
{
    /// <summary>
    /// Payment method code (CASH, CARD, BANK_TRANSFER)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Whether this payment method requires immediate settlement
    /// </summary>
    public bool RequiresImmediateSettlement { get; set; } = true;

    /// <summary>
    /// Whether this payment method supports partial payments
    /// </summary>
    public bool SupportsPartialPayments { get; set; } = false;

    /// <summary>
    /// Maximum transaction amount for this payment method
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxTransactionAmount { get; set; }

    /// <summary>
    /// Processing fee percentage for this payment method
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal? ProcessingFeePercentage { get; set; }

    /// <summary>
    /// Whether this payment method is electronic
    /// </summary>
    public bool IsElectronic { get; set; } = false;
    public string Description { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; } = 0;
}

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DijaGoldPOS.API.Models.BranchModels;
using DijaGoldPOS.API.Models.LookupTables;
using DijaGoldPOS.API.Models.Shared;

namespace DijaGoldPOS.API.Models.SupplierModels;

/// <summary>
/// Aggregated view of supplier gold balances by karat type
/// </summary>
public class SupplierGoldBalance : BaseEntity
{
    /// <summary>
    /// Supplier ID
    /// </summary>
    public int SupplierId { get; set; }
    
    /// <summary>
    /// Branch ID
    /// </summary>
    public int BranchId { get; set; }
    
    /// <summary>
    /// Karat type
    /// </summary>
    public int KaratTypeId { get; set; }
    
    /// <summary>
    /// Total weight received from this supplier (in grams)
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal TotalWeightReceived { get; set; }
    
    /// <summary>
    /// Total weight paid for to this supplier (in grams)
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal TotalWeightPaidFor { get; set; }
    
    /// <summary>
    /// Outstanding weight debt to supplier (TotalWeightReceived - TotalWeightPaidFor)
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal OutstandingWeightDebt => TotalWeightReceived - TotalWeightPaidFor;
    
    /// <summary>
    /// Gold balance in favor of merchant (negative debt, positive credit)
    /// Positive value means merchant has credit with supplier
    /// Negative value means merchant owes supplier
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal MerchantGoldBalance { get; set; } = 0;
    
    /// <summary>
    /// Total monetary value of outstanding debt
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingMonetaryValue { get; set; }
    
    /// <summary>
    /// Average cost per gram for this karat with this supplier
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal AverageCostPerGram { get; set; }
    
    /// <summary>
    /// Last transaction date
    /// </summary>
    public DateTime? LastTransactionDate { get; set; }
    
    // Navigation properties
    [JsonIgnore]
    public virtual Supplier Supplier { get; set; } = null!;
    
    [JsonIgnore]
    public virtual Branch Branch { get; set; } = null!;
    
    [JsonIgnore]
    public virtual KaratTypeLookup KaratType { get; set; } = null!;
}

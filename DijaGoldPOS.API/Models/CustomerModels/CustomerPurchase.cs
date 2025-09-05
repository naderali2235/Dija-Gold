using DijaGoldPOS.API.Models.BranchModels;
using DijaGoldPOS.API.Models.FinancialModels;
using DijaGoldPOS.API.Models.LookupModels;
using DijaGoldPOS.API.Models.Shared;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models.CustomerModels;

/// <summary>
/// Represents a direct gold purchase transaction from a customer
/// </summary>
[Table("CustomerPurchases", Schema = "Customer")]
public class CustomerPurchase : BaseEntity
{
    /// <summary>
    /// Unique purchase number
    /// </summary>
    public string PurchaseNumber { get; set; } = string.Empty;

    /// <summary>
    /// Customer ID who is selling gold to us
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Branch where the purchase was made
    /// </summary>
    public int BranchId { get; set; }

    /// <summary>
    /// Date and time of the purchase
    /// </summary>
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Payment method used for this purchase
    /// </summary>
    public int PaymentMethodId { get; set; }

    /// <summary>
    /// Total amount paid to customer
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Amount actually paid (may be less due to deductions)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }

    /// <summary>
    /// Total weight of gold purchased (in grams)
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal TotalWeight { get; set; }

    /// <summary>
    /// Average purity of the gold purchased
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal AveragePurity { get; set; }

    /// <summary>
    /// Testing method used (Acid Test, Electronic, X-Ray)
    /// </summary>
    public string TestingMethod { get; set; } = "Electronic";

    /// <summary>
    /// Testing fee charged (if any)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TestingFee { get; set; } = 0;

    /// <summary>
    /// Processing fee charged (if any)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ProcessingFee { get; set; } = 0;

    /// <summary>
    /// Other deductions from the total amount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal OtherDeductions { get; set; } = 0;

    /// <summary>
    /// Purchase status (Pending, Completed, Cancelled)
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// User who processed this purchase
    /// </summary>
    public string ProcessedByUserId { get; set; } = string.Empty;


    /// <summary>
    /// User who approved this purchase (if required)
    /// </summary>
    public string? ApprovedByUserId { get; set; }

    /// <summary>
    /// Approval date and time
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Additional notes about the purchase
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Photos of the gold items (JSON array of URLs)
    /// </summary>
    public string? Photos { get; set; }

    /// <summary>
    /// Receipt number generated for this purchase
    /// </summary>
    public string? ReceiptNumber { get; set; }

    /// <summary>
    /// Whether this purchase affects our inventory
    /// </summary>
    public bool AffectsInventory { get; set; } = true;

    /// <summary>
    /// Inventory adjustment reason if applicable
    /// </summary>
    public string? InventoryAdjustmentReason { get; set; }

    /// <summary>
    /// Reference to related financial transaction
    /// </summary>
    public int? FinancialTransactionId { get; set; }

    // Navigation Properties
    /// <summary>
    /// Customer who sold the gold
    /// </summary>
    [JsonIgnore]
    public virtual Customer Customer { get; set; } = null!;

    /// <summary>
    /// Branch where purchase was made
    /// </summary>
    [JsonIgnore]
    public virtual Branch Branch { get; set; } = null!;

    /// <summary>
    /// Payment method used
    /// </summary>
    [JsonIgnore]
    public virtual PaymentMethodLookup PaymentMethod { get; set; } = null!;

    /// <summary>
    /// Individual items in this purchase
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<CustomerPurchaseItem> CustomerPurchaseItems { get; set; } = new List<CustomerPurchaseItem>();

    /// <summary>
    /// User who processed the purchase
    /// </summary>
    [JsonIgnore]
    public virtual ApplicationUser ProcessedByUser { get; set; } = null!;

    /// <summary>
    /// User who approved the purchase
    /// </summary>
    [JsonIgnore]
    public virtual ApplicationUser? ApprovedByUser { get; set; }

    /// <summary>
    /// Related financial transaction
    /// </summary>
    [JsonIgnore]
    public virtual FinancialTransaction? FinancialTransaction { get; set; }
}

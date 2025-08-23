using DijaGoldPOS.API.Models.LookupTables;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents a sales order (separate from financial transaction)
/// </summary>
public class Order : BaseEntity
{
    /// <summary>
    /// Order number (sequential, unique per branch)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of order
    /// </summary>
    [Required]
    public int OrderTypeId { get; set; }
    
    /// <summary>
    /// Order date and time
    /// </summary>
    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Branch where order was created
    /// </summary>
    [Required]
    public int BranchId { get; set; }
    
    /// <summary>
    /// Customer ID
    /// </summary>
    public int? CustomerId { get; set; }
    
    /// <summary>
    /// Cashier who created the order
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string CashierId { get; set; } = string.Empty;
    
    /// <summary>
    /// Manager who approved (for returns/exchanges)
    /// </summary>
    [MaxLength(450)]
    public string? ApprovedByUserId { get; set; }
    
    /// <summary>
    /// Order status
    /// </summary>
    [Required]
    public int StatusId { get; set; }
    
    /// <summary>
    /// Reference to original order (for returns/exchanges)
    /// </summary>
    public int? OriginalOrderId { get; set; }
    
    /// <summary>
    /// Return/exchange reason
    /// </summary>
    [MaxLength(500)]
    public string? ReturnReason { get; set; }
    
    /// <summary>
    /// Additional notes for the order
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Estimated completion date (for exchanges)
    /// </summary>
    public DateTime? EstimatedCompletionDate { get; set; }
    
    /// <summary>
    /// Gold rate used for this order
    /// </summary>
    public int? GoldRateId { get; set; }
    
    /// <summary>
    /// Reference to financial transaction
    /// </summary>
    public int? FinancialTransactionId { get; set; }
    
    /// <summary>
    /// Navigation property to branch
    /// </summary>
    [JsonIgnore]
    public virtual Branch Branch { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to customer
    /// </summary>
    [JsonIgnore]
    public virtual Customer? Customer { get; set; }
    
    /// <summary>
    /// Navigation property to cashier
    /// </summary>
    [JsonIgnore]
    public virtual ApplicationUser Cashier { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to approving manager
    /// </summary>
    [JsonIgnore]
    public virtual ApplicationUser? ApprovedByUser { get; set; }
    
    /// <summary>
    /// Navigation property to gold rate used
    /// </summary>
    [JsonIgnore]
    public virtual GoldRate? GoldRate { get; set; }
    
    /// <summary>
    /// Navigation property to financial transaction
    /// </summary>
    [JsonIgnore]
    public virtual FinancialTransaction? FinancialTransaction { get; set; }
    
    /// <summary>
    /// Navigation property to original order (for returns/exchanges)
    /// </summary>
    [JsonIgnore]
    public virtual Order? OriginalOrder { get; set; }
    
    /// <summary>
    /// Navigation property to return/exchange orders
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<Order> RelatedOrders { get; set; } = new List<Order>();
    
    /// <summary>
    /// Navigation property to order type lookup
    /// </summary>
    [JsonIgnore]
    public virtual OrderTypeLookup OrderType { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to order status lookup
    /// </summary>
    [JsonIgnore]
    public virtual OrderStatusLookup Status { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to order items
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

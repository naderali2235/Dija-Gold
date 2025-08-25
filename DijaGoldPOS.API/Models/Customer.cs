using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents a customer with optional loyalty program features
/// </summary>
public class Customer : BaseEntity
{
    /// <summary>
    /// Full customer name (required)
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// National ID number (optional, unique if provided)
    /// </summary>
    public string? NationalId { get; set; }

    /// <summary>
    /// Egyptian mobile number (optional, unique if provided)
    /// </summary>
    public string? MobileNumber { get; set; }

    /// <summary>
    /// Email address (optional, unique if provided)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Customer address (optional)
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Registration date
    /// </summary>
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Loyalty tier level (1-5, 1 being basic)
    /// </summary>
    public int LoyaltyTier { get; set; } = 1;

    /// <summary>
    /// Loyalty points accumulated
    /// </summary>
    public int LoyaltyPoints { get; set; } = 0;

    /// <summary>
    /// Total purchase amount (for loyalty calculation)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPurchaseAmount { get; set; } = 0;

    /// <summary>
    /// Default discount percentage for this customer
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal DefaultDiscountPercentage { get; set; } = 0;

    /// <summary>
    /// Whether making charges are waived for this customer
    /// </summary>
    public bool MakingChargesWaived { get; set; } = false;

    /// <summary>
    /// Special notes about the customer
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Date of last purchase (nullable)
    /// </summary>
    public DateTime? LastPurchaseDate { get; set; }

    /// <summary>
    /// Total number of transactions for this customer
    /// </summary>
    public int TotalOrders { get; set; } = 0;

    /// <summary>
    /// Navigation property to customer orders
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    /// <summary>
    /// Navigation property to customer purchases (when customer sells gold to the merchant)
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<CustomerPurchase> CustomerPurchases { get; set; } = new List<CustomerPurchase>();
}

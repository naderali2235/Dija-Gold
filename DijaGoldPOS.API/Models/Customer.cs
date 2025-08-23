using System.ComponentModel.DataAnnotations;
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
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// National ID number (optional, unique if provided)
    /// </summary>
    [MaxLength(20)]
    public string? NationalId { get; set; }
    
    /// <summary>
    /// Egyptian mobile number (optional, unique if provided)
    /// </summary>
    [MaxLength(15)]
    public string? MobileNumber { get; set; }
    
    /// <summary>
    /// Email address (optional, unique if provided)
    /// </summary>
    [MaxLength(100)]
    public string? Email { get; set; }
    
    /// <summary>
    /// Customer address (optional)
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }
    
    /// <summary>
    /// Registration date
    /// </summary>
    [Required]
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
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Date of last purchase (nullable)
    /// </summary>
    public DateTime? LastPurchaseDate { get; set; }
    
    /// <summary>
    /// Total number of transactions for this customer
    /// </summary>
    public int TotalTransactions { get; set; } = 0;
    
    /// <summary>
    /// Navigation property to customer orders
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
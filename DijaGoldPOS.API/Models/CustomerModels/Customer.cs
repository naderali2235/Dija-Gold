using DijaGoldPOS.API.Models.SalesModels;
using DijaGoldPOS.API.Models.Shared;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models.CustomerModels;

/// <summary>
/// Represents a customer with loyalty program features and purchase history
/// </summary>
[Table("Customers", Schema = "Customer")]
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
    /// Date of birth for age-based services
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Gender (Male, Female, Other, PreferNotToSay)
    /// </summary>
    public string? Gender { get; set; }

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
    /// Total weight of gold purchased (in grams)
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal TotalGoldPurchased { get; set; } = 0;

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
    /// Customer category (VIP, Regular, New, etc.)
    /// </summary>
    public string CustomerCategory { get; set; } = "Regular";

    /// <summary>
    /// Preferred contact method (Email, SMS, Phone, WhatsApp)
    /// </summary>
    public string? PreferredContactMethod { get; set; }

    /// <summary>
    /// Language preference for communications
    /// </summary>
    public string LanguagePreference { get; set; } = "Arabic";

    /// <summary>
    /// Special notes about the customer
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Customer preferences in JSON format
    /// </summary>
    public string? Preferences { get; set; }

    /// <summary>
    /// Whether customer agreed to marketing communications
    /// </summary>
    public bool MarketingConsent { get; set; } = false;

    /// <summary>
    /// Last transaction date for activity tracking
    /// </summary>
    public DateTime? LastTransactionDate { get; set; }

    /// <summary>
    /// Customer status (Active, Inactive, Suspended, Blacklisted)
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Credit limit for this customer (if applicable)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? CreditLimit { get; set; }

    /// <summary>
    /// Current outstanding balance
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingBalance { get; set; } = 0;

    /// <summary>
    /// KYC (Know Your Customer) status
    /// </summary>
    public string KycStatus { get; set; } = "Pending";

    /// <summary>
    /// KYC completion date
    /// </summary>
    public DateTime? KycCompletedDate { get; set; }

    /// <summary>
    /// Documents submitted for KYC (JSON array)
    /// </summary>
    public string? KycDocuments { get; set; }

    /// <summary>
    /// Last purchase date (computed from orders and purchases)
    /// </summary>
    [NotMapped]
    public DateTime? LastPurchaseDate => 
        new DateTime[] { 
            Orders.Any() ? Orders.Max(o => o.OrderDate) : DateTime.MinValue,
            CustomerPurchases.Any() ? CustomerPurchases.Max(cp => cp.PurchaseDate) : DateTime.MinValue
        }.Max() == DateTime.MinValue ? null : new DateTime[] { 
            Orders.Any() ? Orders.Max(o => o.OrderDate) : DateTime.MinValue,
            CustomerPurchases.Any() ? CustomerPurchases.Max(cp => cp.PurchaseDate) : DateTime.MinValue
        }.Max();

    /// <summary>
    /// Total number of orders placed by this customer
    /// </summary>
    [NotMapped]
    public int TotalOrders => Orders.Count();

    // Navigation Properties
    /// <summary>
    /// Orders placed by this customer
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    /// <summary>
    /// Direct gold purchases from this customer
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<CustomerPurchase> CustomerPurchases { get; set; } = new List<CustomerPurchase>();

    /// <summary>
    /// Repair jobs for this customer
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<RepairJob> RepairJobs { get; set; } = new List<RepairJob>();
}

using DijaGoldPOS.API.Models.ProductModels;
using DijaGoldPOS.API.Models.PurchaseOrderModels;
using DijaGoldPOS.API.Models.Shared;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models.SupplierModels;

/// <summary>
/// Represents a supplier/vendor in the system
/// </summary>
[Table("Suppliers", Schema = "Supplier")]
public class Supplier : BaseEntity
{
    /// <summary>
    /// Unique supplier code for identification
    /// </summary>
    public string SupplierCode { get; set; } = string.Empty;

    /// <summary>
    /// Company name
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Trade name (if different from company name)
    /// </summary>
    public string? TradeName { get; set; }

    /// <summary>
    /// Primary contact person name
    /// </summary>
    public string? ContactPersonName { get; set; }

    /// <summary>
    /// Contact person title/position
    /// </summary>
    public string? ContactPersonTitle { get; set; }

    /// <summary>
    /// Primary phone number
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Secondary phone number
    /// </summary>
    public string? AlternatePhone { get; set; }

    /// <summary>
    /// Email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Website URL
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Physical address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// City
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// State/Province
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Postal code
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Country
    /// </summary>
    public string? Country { get; set; } = "Egypt";

    /// <summary>
    /// Tax registration number
    /// </summary>
    public string? TaxRegistrationNumber { get; set; }

    /// <summary>
    /// Commercial registration number
    /// </summary>
    public string? CommercialRegistrationNumber { get; set; }

    /// <summary>
    /// VAT registration number
    /// </summary>
    public string? VatRegistrationNumber { get; set; }

    /// <summary>
    /// Bank account information (JSON format)
    /// </summary>
    public string? BankAccountInfo { get; set; }

    /// <summary>
    /// Credit limit approved for this supplier
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal CreditLimit { get; set; } = 0;

    /// <summary>
    /// Current outstanding balance
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentBalance { get; set; } = 0;

    /// <summary>
    /// Payment terms in days (Net 30, Net 60, etc.)
    /// </summary>
    public int PaymentTermsDays { get; set; } = 30;

    /// <summary>
    /// Early payment discount percentage
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal EarlyPaymentDiscountPercentage { get; set; } = 0;

    /// <summary>
    /// Days within which early payment discount applies
    /// </summary>
    public int EarlyPaymentDiscountDays { get; set; } = 10;

    /// <summary>
    /// Supplier category (Gold, Jewelry, Equipment, Services)
    /// </summary>
    public string Category { get; set; } = "Gold";

    /// <summary>
    /// Supplier rating (1-5 stars)
    /// </summary>
    public int Rating { get; set; } = 3;

    /// <summary>
    /// Supplier status (Active, Inactive, Suspended, Blacklisted)
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Whether this supplier is preferred
    /// </summary>
    public bool IsPreferred { get; set; } = false;

    /// <summary>
    /// Whether this supplier is approved for gold purchases
    /// </summary>
    public bool IsApprovedForGold { get; set; } = true;

    /// <summary>
    /// Lead time for orders in days
    /// </summary>
    public int LeadTimeDays { get; set; } = 7;

    /// <summary>
    /// Minimum order value
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinimumOrderValue { get; set; }

    /// <summary>
    /// Maximum order value without special approval
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaximumOrderValue { get; set; }

    /// <summary>
    /// Delivery terms (FOB, CIF, etc.)
    /// </summary>
    public string? DeliveryTerms { get; set; }

    /// <summary>
    /// Quality certifications (JSON array)
    /// </summary>
    public string? QualityCertifications { get; set; }

    /// <summary>
    /// Insurance information
    /// </summary>
    public string? InsuranceInfo { get; set; }

    /// <summary>
    /// Contract start date
    /// </summary>
    public DateTime? ContractStartDate { get; set; }

    /// <summary>
    /// Contract end date
    /// </summary>
    public DateTime? ContractEndDate { get; set; }

    /// <summary>
    /// Last transaction date
    /// </summary>
    public DateTime? LastTransactionDate { get; set; }

    /// <summary>
    /// Total purchase amount from this supplier
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPurchaseAmount { get; set; } = 0;

    /// <summary>
    /// Total number of orders placed
    /// </summary>
    public int TotalOrdersCount { get; set; } = 0;

    /// <summary>
    /// Average order value
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal AverageOrderValue { get; set; } = 0;

    /// <summary>
    /// On-time delivery percentage
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal OnTimeDeliveryPercentage { get; set; } = 100;

    /// <summary>
    /// Quality rating percentage
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal QualityRatingPercentage { get; set; } = 100;

    /// <summary>
    /// Additional notes about the supplier
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Supplier documents (JSON array of document info)
    /// </summary>
    public string? Documents { get; set; }

    /// <summary>
    /// Emergency contact information
    /// </summary>
    public string? EmergencyContact { get; set; }

    /// <summary>
    /// Preferred communication method
    /// </summary>
    public string PreferredCommunicationMethod { get; set; } = "Email";

    /// <summary>
    /// Language preference
    /// </summary>
    public string LanguagePreference { get; set; } = "Arabic";

    /// <summary>
    /// Time zone
    /// </summary>
    public string TimeZone { get; set; } = "Africa/Cairo";

    /// <summary>
    /// Whether this is a system-managed supplier (cannot be deleted)
    /// </summary>
    public bool IsSystemSupplier { get; set; } = false;

    /// <summary>
    /// Whether credit limit is enforced for this supplier
    /// </summary>
    public bool CreditLimitEnforced { get; set; } = true;

    /// <summary>
    /// Payment terms description (e.g., "Net 30", "COD", etc.)
    /// </summary>
    public string PaymentTerms { get; set; } = "Net 30";

    /// <summary>
    /// Contact person name (alias for ContactPersonName)
    /// </summary>
    [NotMapped]
    public string? ContactPerson => ContactPersonName;

    /// <summary>
    /// Phone number (alias for Phone)
    /// </summary>
    [NotMapped]
    public string? PhoneNumber => Phone;

    // Navigation Properties
    /// <summary>
    /// Products supplied by this supplier
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    /// <summary>
    /// Purchase orders from this supplier
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    /// <summary>
    /// Raw gold purchase orders from this supplier
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<RawGoldPurchaseOrder> RawGoldPurchaseOrders { get; set; } = new List<RawGoldPurchaseOrder>();

    /// <summary>
    /// Financial transactions with this supplier
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<SupplierTransaction> SupplierTransactions { get; set; } = new List<SupplierTransaction>();

    /// <summary>
    /// Gold balance records for this supplier
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<SupplierGoldBalance> GoldBalances { get; set; } = new List<SupplierGoldBalance>();

    /// <summary>
    /// Product ownerships with this supplier
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<ProductOwnership> ProductOwnerships { get; set; } = new List<ProductOwnership>();
}

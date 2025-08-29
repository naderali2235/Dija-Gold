
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents a supplier with credit management features
/// </summary>
public class Supplier : BaseEntity
{
    /// <summary>
    /// Company name
    /// </summary>


    public string CompanyName { get; set; } = string.Empty;
    
    /// <summary>
    /// Contact person name
    /// </summary>

    public string? ContactPersonName { get; set; }
    
    /// <summary>
    /// Primary phone number
    /// </summary>

    public string? Phone { get; set; }
    
    /// <summary>
    /// Email address
    /// </summary>

    public string? Email { get; set; }
    
    /// <summary>
    /// Company address
    /// </summary>

    public string? Address { get; set; }
    
    /// <summary>
    /// Tax registration number
    /// </summary>

    public string? TaxRegistrationNumber { get; set; }
    
    /// <summary>
    /// Commercial registration number
    /// </summary>

    public string? CommercialRegistrationNumber { get; set; }
    
    /// <summary>
    /// Credit limit in Egyptian Pounds
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal CreditLimit { get; set; } = 0;
    
    /// <summary>
    /// Current balance (positive = we owe them, negative = they owe us)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentBalance { get; set; } = 0;
    
    /// <summary>
    /// Payment terms in days
    /// </summary>
    public int PaymentTermsDays { get; set; } = 30;
    
    /// <summary>
    /// Whether credit limit enforcement is active
    /// </summary>
    public bool CreditLimitEnforced { get; set; } = true;
    
    /// <summary>
    /// Special terms and conditions
    /// </summary>

    public string? PaymentTerms { get; set; }
    
    /// <summary>
    /// Notes about the supplier
    /// </summary>

    public string? Notes { get; set; }
    
    /// <summary>
    /// Date of last transaction with this supplier (nullable)
    /// </summary>
    public DateTime? LastTransactionDate { get; set; }
    
    /// <summary>
    /// Indicates if this is a system supplier that cannot be deleted (e.g., DijaGold itself)
    /// </summary>
    public bool IsSystemSupplier { get; set; } = false;
    
    /// <summary>
    /// Alias for ContactPersonName for compatibility
    /// </summary>
    public string? ContactPerson => ContactPersonName;
    
    /// <summary>
    /// Alias for Phone for compatibility
    /// </summary>
    public string? PhoneNumber => Phone;
    
    /// <summary>
    /// Navigation property to products from this supplier
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    
    /// <summary>
    /// Navigation property to purchase orders
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    /// <summary>
    /// Navigation property to supplier transactions
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<SupplierTransaction> SupplierTransactions { get; set; } = new List<SupplierTransaction>();

    /// <summary>
    /// Navigation property to product ownerships for this supplier
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<ProductOwnership> ProductOwnerships { get; set; } = new List<ProductOwnership>();

    /// <summary>
    /// Navigation property to raw gold purchase orders from this supplier
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<RawGoldPurchaseOrder> RawGoldPurchaseOrders { get; set; } = new List<RawGoldPurchaseOrder>();
}

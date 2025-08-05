using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents a supplier with credit management features
/// </summary>
public class Supplier : BaseEntity
{
    /// <summary>
    /// Company name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;
    
    /// <summary>
    /// Contact person name
    /// </summary>
    [MaxLength(100)]
    public string? ContactPersonName { get; set; }
    
    /// <summary>
    /// Primary phone number
    /// </summary>
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    /// <summary>
    /// Email address
    /// </summary>
    [MaxLength(100)]
    public string? Email { get; set; }
    
    /// <summary>
    /// Company address
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }
    
    /// <summary>
    /// Tax registration number
    /// </summary>
    [MaxLength(50)]
    public string? TaxRegistrationNumber { get; set; }
    
    /// <summary>
    /// Commercial registration number
    /// </summary>
    [MaxLength(50)]
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
    [MaxLength(1000)]
    public string? PaymentTerms { get; set; }
    
    /// <summary>
    /// Notes about the supplier
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Date of last transaction with this supplier (nullable)
    /// </summary>
    public DateTime? LastTransactionDate { get; set; }
    
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
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    
    /// <summary>
    /// Navigation property to purchase orders
    /// </summary>
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
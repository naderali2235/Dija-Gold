using DijaGoldPOS.API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents tax configuration with versioning for Egyptian tax compliance
/// </summary>
public class TaxConfiguration : BaseEntity
{
    /// <summary>
    /// Tax name (e.g., "VAT", "Sales Tax")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TaxName { get; set; } = string.Empty;
    
    /// <summary>
    /// Tax code for reference
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string TaxCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of tax (percentage or fixed amount)
    /// </summary>
    [Required]
    public ChargeType TaxType { get; set; }
    
    /// <summary>
    /// Tax rate (percentage) or fixed amount in EGP
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10,4)")]
    public decimal TaxRate { get; set; }
    
    /// <summary>
    /// Whether this tax is mandatory
    /// </summary>
    public bool IsMandatory { get; set; } = true;
    
    /// <summary>
    /// Effective start date for this tax configuration
    /// </summary>
    [Required]
    public DateTime EffectiveFrom { get; set; }
    
    /// <summary>
    /// Effective end date for this tax configuration (null if current)
    /// </summary>
    public DateTime? EffectiveTo { get; set; }
    
    /// <summary>
    /// Whether this is the current active tax configuration
    /// </summary>
    public bool IsCurrent { get; set; } = true;
    
    /// <summary>
    /// Display order for multiple taxes
    /// </summary>
    public int DisplayOrder { get; set; } = 1;
    
    /// <summary>
    /// Navigation property to transaction taxes
    /// </summary>
    public virtual ICollection<TransactionTax> TransactionTaxes { get; set; } = new List<TransactionTax>();
}
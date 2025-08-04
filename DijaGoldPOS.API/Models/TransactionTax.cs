using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents taxes applied to a transaction
/// </summary>
public class TransactionTax : BaseEntity
{
    /// <summary>
    /// Transaction this tax belongs to
    /// </summary>
    [Required]
    public int TransactionId { get; set; }
    
    /// <summary>
    /// Tax configuration used
    /// </summary>
    [Required]
    public int TaxConfigurationId { get; set; }
    
    /// <summary>
    /// Tax rate applied (captured at transaction time)
    /// </summary>
    [Column(TypeName = "decimal(10,4)")]
    public decimal TaxRate { get; set; }
    
    /// <summary>
    /// Taxable amount (base amount on which tax is calculated)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxableAmount { get; set; }
    
    /// <summary>
    /// Calculated tax amount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }
    
    /// <summary>
    /// Navigation property to transaction
    /// </summary>
    public virtual Transaction Transaction { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to tax configuration
    /// </summary>
    public virtual TaxConfiguration TaxConfiguration { get; set; } = null!;
}
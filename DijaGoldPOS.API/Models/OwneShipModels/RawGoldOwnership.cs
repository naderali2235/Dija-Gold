using DijaGoldPOS.API.Models.BranchModels;
using DijaGoldPOS.API.Models.LookupTables;
using DijaGoldPOS.API.Models.PurchaseOrderModels;
using DijaGoldPOS.API.Models.Shared;
using DijaGoldPOS.API.Models.SupplierModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models.OwneShipModels;

/// <summary>
/// Tracks ownership of raw gold by karat type, supplier, and branch
/// </summary>
public class RawGoldOwnership : BaseEntity
{
    [Required]
    public int KaratTypeId { get; set; }
    
    [Required]
    public int BranchId { get; set; }
    
    [Required]
    public int SupplierId { get; set; }
    
    public int? RawGoldPurchaseOrderId { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal TotalWeight { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal OwnedWeight { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal OwnershipPercentage { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingAmount { get; set; }

    public string? Notes { get; set; }

    // Navigation properties
    public virtual KaratTypeLookup KaratType { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
    public virtual Supplier Supplier { get; set; } = null!;
    public virtual RawGoldPurchaseOrder? RawGoldPurchaseOrder { get; set; }
}

using DijaGoldPOS.API.Models.BranchModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models.FinancialModels;

public class TreasuryAccount
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentBalance { get; set; }

    [MaxLength(3)]
    public string CurrencyCode { get; set; } = "EGP"; // default single currency

    public bool IsActive { get; set; } = true;

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(450)] public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    [MaxLength(450)] public string? UpdatedByUserId { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; } = false;
}

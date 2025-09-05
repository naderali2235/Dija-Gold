using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models.FinancialModels;

public enum TreasuryTransactionDirection
{
    Credit = 1, // increases balance
    Debit = 2   // decreases balance
}

public enum TreasuryTransactionType
{
    Adjustment = 1,
    FeedFromCashDrawer = 2,
    SupplierPayment = 3,
    TransferIn = 4,
    TransferOut = 5
}

public class TreasuryTransaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TreasuryAccountId { get; set; }
    public TreasuryAccount TreasuryAccount { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public TreasuryTransactionDirection Direction { get; set; }

    [Required]
    public TreasuryTransactionType Type { get; set; }

    // Optional linkage to external entities (e.g., cash drawer, supplier payment)
    public string? ReferenceType { get; set; }
    public string? ReferenceId { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(450)] public string? PerformedByUserId { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; } = false;
}

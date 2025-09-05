using DijaGoldPOS.API.Models.BranchModels;
using DijaGoldPOS.API.Models.Shared;

namespace DijaGoldPOS.API.Models.FinancialModels;

/// <summary>
/// Represents daily cash drawer balance for a branch
/// </summary>
public class CashDrawerBalance : BaseEntity
{
    /// <summary>
    /// Branch ID
    /// </summary>
    public int BranchId { get; set; }
    
    /// <summary>
    /// Branch navigation property
    /// </summary>
    public virtual Branch Branch { get; set; } = null!;
    
    /// <summary>
    /// Date of the balance record
    /// </summary>

    public DateTime BalanceDate { get; set; }
    
    /// <summary>
    /// Opening balance for the day
    /// </summary>


    public decimal OpeningBalance { get; set; }
    
    /// <summary>
    /// Expected closing balance based on transactions
    /// </summary>


    public decimal ExpectedClosingBalance { get; set; }
    
    /// <summary>
    /// Actual closing balance counted by staff
    /// </summary>

    public decimal? ActualClosingBalance { get; set; }
    
    /// <summary>
    /// Cash over/short amount (Actual - Expected)
    /// </summary>
    public decimal CashOverShort => (ActualClosingBalance ?? 0) - ExpectedClosingBalance;
    
    /// <summary>
    /// User who opened the drawer
    /// </summary>
    public string? OpenedByUserId { get; set; }
    
    /// <summary>
    /// User who closed the drawer
    /// </summary>
    public string? ClosedByUserId { get; set; }
    
    /// <summary>
    /// Time when drawer was opened
    /// </summary>
    public DateTime? OpenedAt { get; set; }
    
    /// <summary>
    /// Time when drawer was closed
    /// </summary>
    public DateTime? ClosedAt { get; set; }
    
    /// <summary>
    /// Notes about the day's cash handling
    /// </summary>

    public string? Notes { get; set; }
    
    /// <summary>
    /// Status of the cash drawer for the day
    /// </summary>
    public CashDrawerStatus Status { get; set; } = CashDrawerStatus.Open;
    
    /// <summary>
    /// Amount settled (removed) during shift closure
    /// </summary>
    public decimal? SettledAmount { get; set; }
    
    /// <summary>
    /// Amount carried forward to next day
    /// </summary>
    public decimal? CarriedForwardAmount { get; set; }
    
    /// <summary>
    /// Settlement notes/reason
    /// </summary>

    public string? SettlementNotes { get; set; }
}

/// <summary>
/// Status of cash drawer for a given day
/// </summary>
public enum CashDrawerStatus
{
    /// <summary>
    /// Drawer is open and active
    /// </summary>
    Open = 1,
    
    /// <summary>
    /// Drawer has been closed for the day
    /// </summary>
    Closed = 2,
    
    /// <summary>
    /// Drawer reconciliation is pending
    /// </summary>
    PendingReconciliation = 3
}

namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Cash drawer balance DTO for display operations
/// </summary>
public class CashDrawerBalanceDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public DateTime BalanceDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ExpectedClosingBalance { get; set; }
    public decimal ActualClosingBalance { get; set; }
    public decimal CashOverShort { get; set; }
    public string OpenedByUserId { get; set; } = string.Empty;
    public string ClosedByUserId { get; set; } = string.Empty;
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal SettledAmount { get; set; }
    public decimal CarriedForwardAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? SettlementNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
}

/// <summary>
/// Cash drawer summary DTO
/// </summary>
public class CashDrawerSummaryDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime BalanceDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ExpectedClosingBalance { get; set; }
    public decimal ActualClosingBalance { get; set; }
    public decimal CashOverShort { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsReconciled { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal TodaySales { get; set; }
    public decimal TodayDeposits { get; set; }
    public decimal TodayWithdrawals { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Create cash drawer balance request DTO
/// </summary>
public class CreateCashDrawerBalanceRequestDto
{
    public int BranchId { get; set; }
    public DateTime BalanceDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Update cash drawer balance request DTO
/// </summary>
public class UpdateCashDrawerBalanceRequestDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public DateTime BalanceDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ActualClosingBalance { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal CashDeposits { get; set; }
    public decimal CashWithdrawals { get; set; }
    public decimal SettledAmount { get; set; }
    public decimal CarriedForwardAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? SettlementNotes { get; set; }
}

/// <summary>
/// Cash drawer balance search request DTO
/// </summary>
public class CashDrawerBalanceSearchRequestDto
{
    public int? BranchId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Process cash drawer settlement request DTO
/// </summary>
public class ProcessCashDrawerSettlementRequestDto
{
    public int CashDrawerBalanceId { get; set; }
    public decimal ActualClosingBalance { get; set; }
    public decimal SettledAmount { get; set; }
    public decimal CarriedForwardAmount { get; set; }
    public decimal VarianceAmount { get; set; }
    public string? VarianceReason { get; set; }
    public string? SettlementNotes { get; set; }
    public string ProcessedBy { get; set; } = string.Empty;
}

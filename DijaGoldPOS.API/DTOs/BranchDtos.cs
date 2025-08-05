using System.ComponentModel.DataAnnotations;

namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Branch DTO for list/display operations
/// </summary>
public class BranchDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? ManagerName { get; set; }
    public bool IsHeadquarters { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Create branch request DTO
/// </summary>
public class CreateBranchRequestDto
{
    [Required(ErrorMessage = "Branch name is required")]
    [StringLength(100, ErrorMessage = "Branch name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Branch code is required")]
    [StringLength(20, ErrorMessage = "Branch code cannot exceed 20 characters")]
    public string Code { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? Phone { get; set; }

    [StringLength(100, ErrorMessage = "Manager name cannot exceed 100 characters")]
    public string? ManagerName { get; set; }

    public bool IsHeadquarters { get; set; } = false;
}

/// <summary>
/// Update branch request DTO
/// </summary>
public class UpdateBranchRequestDto : CreateBranchRequestDto
{
    [Required(ErrorMessage = "Branch ID is required")]
    public int Id { get; set; }
}

/// <summary>
/// Branch search request DTO
/// </summary>
public class BranchSearchRequestDto
{
    public string? SearchTerm { get; set; }
    public string? Code { get; set; }
    public bool? IsHeadquarters { get; set; }
    public bool? IsActive { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Branch inventory summary DTO
/// </summary>
public class BranchInventorySummaryDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public int TotalProducts { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal TotalValue { get; set; }
    public int LowStockItems { get; set; }
    public int OutOfStockItems { get; set; }
    public List<BranchInventoryItemDto> TopItems { get; set; } = new();
}

/// <summary>
/// Branch inventory item DTO
/// </summary>
public class BranchInventoryItemDto
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal QuantityOnHand { get; set; }
    public decimal WeightOnHand { get; set; }
    public decimal EstimatedValue { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsOutOfStock { get; set; }
}

/// <summary>
/// Branch staff DTO
/// </summary>
public class BranchStaffDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public List<BranchStaffMemberDto> Staff { get; set; } = new();
    public int TotalStaffCount { get; set; }
}

/// <summary>
/// Branch staff member DTO
/// </summary>
public class BranchStaffMemberDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Branch performance metrics DTO
/// </summary>
public class BranchPerformanceDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public decimal DailySales { get; set; }
    public int DailyTransactions { get; set; }
    public decimal MonthlySales { get; set; }
    public int MonthlyTransactions { get; set; }
    public decimal AverageTransactionValue { get; set; }
    public int ActiveCustomers { get; set; }
    public decimal InventoryTurnover { get; set; }
    public List<BranchTransactionDto> RecentTransactions { get; set; } = new();
}

/// <summary>
/// Branch transaction DTO
/// </summary>
public class BranchTransactionDto
{
    public int TransactionId { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CashierName { get; set; } = string.Empty;
}
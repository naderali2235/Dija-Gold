using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models.BranchModels;

namespace DijaGoldPOS.API.IRepositories;

/// <summary>
/// Repository interface for Branch entity with specific business methods
/// </summary>
public interface IBranchRepository : IRepository<Branch>
{
    /// <summary>
    /// Get branch by code
    /// </summary>
    /// <param name="branchCode">Branch code</param>
    /// <returns>Branch or null if not found</returns>
    Task<Branch?> GetByCodeAsync(string branchCode);

    /// <summary>
    /// Get all active branches
    /// </summary>
    /// <returns>List of active branches</returns>
    Task<List<Branch>> GetActiveBranchesAsync();

    /// <summary>
    /// Get branches with their inventory summary
    /// </summary>
    /// <returns>List of branches with inventory counts</returns>
    Task<List<(Branch Branch, int ProductCount, decimal TotalStockValue)>> GetBranchesWithInventorySummaryAsync();

    /// <summary>
    /// Get branches with their daily sales
    /// </summary>
    /// <param name="date">Date for sales summary</param>
    /// <returns>List of branches with daily sales</returns>
    Task<List<(Branch Branch, decimal DailySales, int TransactionCount)>> GetBranchesWithDailySalesAsync(DateTime date);

    /// <summary>
    /// Check if branch code exists
    /// </summary>
    /// <param name="branchCode">Branch code to check</param>
    /// <param name="excludeId">Branch ID to exclude (for updates)</param>
    /// <returns>True if branch code exists</returns>
    Task<bool> BranchCodeExistsAsync(string branchCode, int? excludeId = null);

    /// <summary>
    /// Get branch users count
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <returns>Number of users in the branch</returns>
    Task<int> GetUserCountAsync(int branchId);

    /// <summary>
    /// Get branch performance metrics
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>Branch performance metrics</returns>
    Task<(decimal TotalSales, int TransactionCount, int ProductsSold, decimal AverageTransactionValue)> GetPerformanceMetricsAsync(int branchId, DateTime fromDate, DateTime toDate);
}

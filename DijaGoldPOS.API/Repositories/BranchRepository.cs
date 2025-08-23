using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Shared;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository implementation for Branch entity with specific business methods
/// </summary>
public class BranchRepository : Repository<Branch>, IBranchRepository
{
    public BranchRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get branch by code
    /// </summary>
    public async Task<Branch?> GetByCodeAsync(string branchCode)
    {
        return await _dbSet
            .Include(b => b.Users)
            .Include(b => b.InventoryItems)
            .FirstOrDefaultAsync(b => b.Code == branchCode);
    }

    /// <summary>
    /// Get all active branches
    /// </summary>
    public async Task<List<Branch>> GetActiveBranchesAsync()
    {
        return await _dbSet
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Get branches with their inventory summary
    /// </summary>
    public async Task<List<(Branch Branch, int ProductCount, decimal TotalStockValue)>> GetBranchesWithInventorySummaryAsync()
    {
        var branches = await _dbSet
            .Include(b => b.InventoryItems)
                .ThenInclude(i => i.Product)
            .Where(b => b.IsActive)
            .ToListAsync();

        var result = new List<(Branch Branch, int ProductCount, decimal TotalStockValue)>();

        foreach (var branch in branches)
        {
            var productCount = branch.InventoryItems.Count;
            var totalStockValue = branch.InventoryItems.Sum(i => i.WeightOnHand * 100); // Simplified calculation
            
            result.Add((branch, productCount, totalStockValue));
        }

        return result;
    }

    /// <summary>
    /// Get branches with their daily sales
    /// </summary>
    public async Task<List<(Branch Branch, decimal DailySales, int TransactionCount)>> GetBranchesWithDailySalesAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var branchSales = await _context.FinancialTransactions
            .Include(t => t.Branch)
            .Where(t => t.TransactionDate >= startOfDay && 
                       t.TransactionDate < endOfDay &&
                       t.TransactionTypeId == LookupTableConstants.FinancialTransactionTypeSale)
            .GroupBy(t => t.Branch)
            .Select(g => new
            {
                Branch = g.Key,
                DailySales = g.Sum(t => t.TotalAmount),
                TransactionCount = g.Count()
            })
            .ToListAsync();

        return branchSales.Select(bs => (bs.Branch, bs.DailySales, bs.TransactionCount)).ToList();
    }

    /// <summary>
    /// Check if branch code exists
    /// </summary>
    public async Task<bool> BranchCodeExistsAsync(string branchCode, int? excludeId = null)
    {
        var query = _dbSet.Where(b => b.Code == branchCode);
        
        if (excludeId.HasValue)
        {
            query = query.Where(b => b.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Get branch users count
    /// </summary>
    public async Task<int> GetUserCountAsync(int branchId)
    {
        return await _context.Users
            .CountAsync(u => u.BranchId == branchId);
    }

    /// <summary>
    /// Get branch performance metrics
    /// </summary>
    public async Task<(decimal TotalSales, int TransactionCount, int ProductsSold, decimal AverageTransactionValue)> GetPerformanceMetricsAsync(int branchId, DateTime fromDate, DateTime toDate)
    {
        var financialTransactions = await _context.FinancialTransactions
            .Where(t => t.BranchId == branchId &&
                       t.TransactionDate >= fromDate &&
                       t.TransactionDate <= toDate &&
                       t.TransactionTypeId == LookupTableConstants.FinancialTransactionTypeSale)
            .ToListAsync();

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.BranchId == branchId &&
                       o.OrderDate >= fromDate &&
                       o.OrderDate <= toDate &&
                       o.OrderTypeId == LookupTableConstants.OrderTypeSale)
            .ToListAsync();

        var totalSales = financialTransactions.Sum(t => t.TotalAmount);
        var transactionCount = financialTransactions.Count;
        var productsSold = orders.SelectMany(o => o.OrderItems).Sum(oi => (int)oi.Quantity);
        var averageTransactionValue = transactionCount > 0 ? totalSales / transactionCount : 0;

        return (totalSales, transactionCount, productsSold, averageTransactionValue);
    }
}
using DijaGoldPOS.API.Models;

using DijaGoldPOS.API.Data;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository implementation for financial transaction operations
/// </summary>
public class FinancialTransactionRepository : Repository<FinancialTransaction>, IFinancialTransactionRepository
{
    public FinancialTransactionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<FinancialTransaction?> GetByTransactionNumberAsync(string transactionNumber, int branchId)
    {
        return await _context.FinancialTransactions
            .Include(ft => ft.Branch)
            .Include(ft => ft.ProcessedByUser)
            .Include(ft => ft.ApprovedByUser)
            .Include(ft => ft.TransactionType)
            .Include(ft => ft.PaymentMethod)
            .Include(ft => ft.Status)
            .Include(ft => ft.BusinessEntityType)
            .FirstOrDefaultAsync(ft => ft.TransactionNumber == transactionNumber && ft.BranchId == branchId);
    }

    public async Task<List<FinancialTransaction>> GetByBusinessEntityAsync(int businessEntityId, int businessEntityTypeId)
    {
        return await _context.FinancialTransactions
            .Include(ft => ft.Branch)
            .Include(ft => ft.ProcessedByUser)
            .Include(ft => ft.TransactionType)
            .Include(ft => ft.PaymentMethod)
            .Include(ft => ft.Status)
            .Include(ft => ft.BusinessEntityType)
            .Where(ft => ft.BusinessEntityId == businessEntityId && ft.BusinessEntityTypeId == businessEntityTypeId)
            .OrderByDescending(ft => ft.TransactionDate)
            .ToListAsync();
    }

    public async Task<(List<FinancialTransaction> Transactions, int TotalCount)> SearchAsync(
        int? branchId = null,
        int? transactionTypeId = null,
        int? statusId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? transactionNumber = null,
        string? processedByUserId = null,
        int? businessEntityId = null,
        int? businessEntityTypeId = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = _context.FinancialTransactions
            .Include(ft => ft.Branch)
            .Include(ft => ft.ProcessedByUser)
            .Include(ft => ft.ApprovedByUser)
            .Include(ft => ft.TransactionType)
            .Include(ft => ft.PaymentMethod)
            .Include(ft => ft.Status)
            .Include(ft => ft.BusinessEntityType)
            .AsQueryable();

        // Apply filters
        if (branchId.HasValue)
            query = query.Where(ft => ft.BranchId == branchId.Value);

        if (transactionTypeId.HasValue)
            query = query.Where(ft => ft.TransactionTypeId == transactionTypeId.Value);

        if (statusId.HasValue)
            query = query.Where(ft => ft.StatusId == statusId.Value);

        if (fromDate.HasValue)
            query = query.Where(ft => ft.TransactionDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(ft => ft.TransactionDate <= toDate.Value);

        if (!string.IsNullOrEmpty(transactionNumber))
            query = query.Where(ft => ft.TransactionNumber.Contains(transactionNumber));

        if (!string.IsNullOrEmpty(processedByUserId))
            query = query.Where(ft => ft.ProcessedByUserId == processedByUserId);

        if (businessEntityId.HasValue)
            query = query.Where(ft => ft.BusinessEntityId == businessEntityId.Value);

        if (businessEntityTypeId.HasValue)
            query = query.Where(ft => ft.BusinessEntityTypeId == businessEntityTypeId.Value);

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination and ordering
        var transactions = await query
            .OrderByDescending(ft => ft.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (transactions, totalCount);
    }

    public async Task<FinancialTransactionSummary> GetSummaryAsync(int? branchId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.FinancialTransactions.AsQueryable();

        // Apply filters
        if (branchId.HasValue)
            query = query.Where(ft => ft.BranchId == branchId.Value);

        if (fromDate.HasValue)
            query = query.Where(ft => ft.TransactionDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(ft => ft.TransactionDate <= toDate.Value);

        // Get summary data
        var summary = new FinancialTransactionSummary
        {
            TotalTransactions = await query.CountAsync(),
            TotalAmount = await query.SumAsync(ft => ft.TotalAmount),
            TotalTaxAmount = await query.SumAsync(ft => ft.TotalTaxAmount),
            TotalDiscountAmount = await query.SumAsync(ft => ft.TotalDiscountAmount)
        };

        // Get transaction type counts
        var transactionTypeCounts = await query
            .GroupBy(ft => ft.TransactionTypeId)
            .Select(g => new { TransactionTypeId = g.Key, Count = g.Count() })
            .ToListAsync();

        foreach (var item in transactionTypeCounts)
        {
            summary.TransactionTypeCounts[item.TransactionTypeId] = item.Count;
        }

        // Get status counts
        var statusCounts = await query
            .GroupBy(ft => ft.StatusId)
            .Select(g => new { StatusId = g.Key, Count = g.Count() })
            .ToListAsync();

        foreach (var item in statusCounts)
        {
            summary.StatusCounts[item.StatusId] = item.Count;
        }

        return summary;
    }

    public async Task<string> GetNextTransactionNumberAsync(int branchId)
    {
        var lastTransaction = await _context.FinancialTransactions
            .Where(ft => ft.BranchId == branchId)
            .OrderByDescending(ft => ft.TransactionNumber)
            .FirstOrDefaultAsync();

        if (lastTransaction == null)
        {
            return $"{branchId:D3}-{DateTime.UtcNow:yyyyMMdd}-0001";
        }

        // Extract the sequence number from the last transaction number
        var parts = lastTransaction.TransactionNumber.Split('-');
        if (parts.Length >= 3 && int.TryParse(parts[2], out var lastSequence))
        {
            var nextSequence = lastSequence + 1;
            return $"{branchId:D3}-{DateTime.UtcNow:yyyyMMdd}-{nextSequence:D4}";
        }

        // Fallback if parsing fails
        return $"{branchId:D3}-{DateTime.UtcNow:yyyyMMdd}-0001";
    }

    public async Task<List<FinancialTransaction>> GetReversalTransactionsAsync(int originalTransactionId)
    {
        return await _context.FinancialTransactions
            .Include(ft => ft.Branch)
            .Include(ft => ft.ProcessedByUser)
            .Include(ft => ft.TransactionType)
            .Include(ft => ft.PaymentMethod)
            .Include(ft => ft.Status)
            .Include(ft => ft.BusinessEntityType)
            .Where(ft => ft.OriginalTransactionId == originalTransactionId)
            .OrderByDescending(ft => ft.TransactionDate)
            .ToListAsync();
    }

    public async Task<List<FinancialTransaction>> GetByDateRangeAsync(int branchId, DateTime fromDate, DateTime toDate)
    {
        return await _context.FinancialTransactions
            .Include(ft => ft.Branch)
            .Include(ft => ft.ProcessedByUser)
            .Include(ft => ft.ApprovedByUser)
            .Include(ft => ft.TransactionType)
            .Include(ft => ft.PaymentMethod)
            .Include(ft => ft.Status)
            .Include(ft => ft.BusinessEntityType)
            .Where(ft => ft.BranchId == branchId && ft.TransactionDate >= fromDate && ft.TransactionDate <= toDate)
            .OrderByDescending(ft => ft.TransactionDate)
            .ToListAsync();
    }

    public async Task<List<FinancialTransaction>> GetByUserAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.FinancialTransactions
            .Include(ft => ft.Branch)
            .Include(ft => ft.TransactionType)
            .Include(ft => ft.PaymentMethod)
            .Include(ft => ft.Status)
            .Include(ft => ft.BusinessEntityType)
            .Where(ft => ft.ProcessedByUserId == userId);

        if (fromDate.HasValue)
            query = query.Where(ft => ft.TransactionDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(ft => ft.TransactionDate <= toDate.Value);

        return await query
            .OrderByDescending(ft => ft.TransactionDate)
            .ToListAsync();
    }
}

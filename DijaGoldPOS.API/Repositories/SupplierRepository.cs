using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository implementation for Supplier entity with specific business methods
/// </summary>
public class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    public SupplierRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get suppliers with outstanding balances
    /// </summary>
    public async Task<List<Supplier>> GetSuppliersWithOutstandingBalancesAsync()
    {
        return await _dbSet
            .Where(s => s.CurrentBalance > 0)
            .OrderByDescending(s => s.CurrentBalance)
            .ToListAsync();
    }

    /// <summary>
    /// Get suppliers near credit limit
    /// </summary>
    public async Task<List<Supplier>> GetSuppliersNearCreditLimitAsync(decimal warningPercentage = 0.8m)
    {
        return await _dbSet
            .Where(s => s.CreditLimit > 0 && 
                       s.CurrentBalance >= (s.CreditLimit * warningPercentage) &&
                       s.CurrentBalance < s.CreditLimit)
            .OrderByDescending(s => s.CurrentBalance / s.CreditLimit)
            .ToListAsync();
    }

    /// <summary>
    /// Get suppliers over credit limit
    /// </summary>
    public async Task<List<Supplier>> GetSuppliersOverCreditLimitAsync()
    {
        return await _dbSet
            .Where(s => s.CreditLimit > 0 && s.CurrentBalance > s.CreditLimit)
            .OrderByDescending(s => s.CurrentBalance - s.CreditLimit)
            .ToListAsync();
    }

    /// <summary>
    /// Search suppliers by company name or contact info
    /// </summary>
    public async Task<List<Supplier>> SearchAsync(string searchTerm)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        
        return await _dbSet
            .Where(s => s.CompanyName.ToLower().Contains(lowerSearchTerm) ||
                       (s.ContactPerson != null && s.ContactPerson.ToLower().Contains(lowerSearchTerm)) ||
                       (s.Email != null && s.Email.ToLower().Contains(lowerSearchTerm)) ||
                       (s.PhoneNumber != null && s.PhoneNumber.Contains(searchTerm)))
            .OrderBy(s => s.CompanyName)
            .ToListAsync();
    }

    /// <summary>
    /// Get supplier purchase history summary
    /// </summary>
    public async Task<(decimal TotalPurchases, int PurchaseOrderCount, decimal OutstandingBalance)> GetPurchaseHistorySummaryAsync(int supplierId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.PurchaseOrders.Where(po => po.SupplierId == supplierId);

        if (fromDate.HasValue)
        {
            query = query.Where(po => po.OrderDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(po => po.OrderDate <= toDate.Value);
        }

        var summary = await query
            .GroupBy(po => po.SupplierId)
            .Select(g => new
            {
                TotalPurchases = g.Sum(po => po.TotalAmount),
                PurchaseOrderCount = g.Count(),
                OutstandingBalance = g.Sum(po => po.OutstandingBalance)
            })
            .FirstOrDefaultAsync();

        return summary != null 
            ? (summary.TotalPurchases, summary.PurchaseOrderCount, summary.OutstandingBalance)
            : (0, 0, 0);
    }

    /// <summary>
    /// Update supplier balance
    /// </summary>
    public async Task<Supplier?> UpdateBalanceAsync(int supplierId, decimal balanceChange)
    {
        var supplier = await GetByIdAsync(supplierId);
        
        if (supplier != null)
        {
            supplier.CurrentBalance += balanceChange;
            
            // Update last transaction date
            supplier.LastTransactionDate = DateTime.UtcNow;
            
            Update(supplier);
        }
        
        return supplier;
    }

    /// <summary>
    /// Get top suppliers by purchase volume
    /// </summary>
    public async Task<List<(Supplier Supplier, decimal TotalPurchases, int PurchaseOrderCount)>> GetTopSuppliersAsync(int topCount, DateTime fromDate, DateTime toDate)
    {
        var results = await _context.PurchaseOrders
            .Include(po => po.Supplier)
            .Where(po => po.OrderDate >= fromDate && po.OrderDate <= toDate)
            .GroupBy(po => po.Supplier)
            .Select(g => new
            {
                Supplier = g.Key,
                TotalPurchases = g.Sum(po => po.TotalAmount),
                PurchaseOrderCount = g.Count()
            })
            .OrderByDescending(x => x.TotalPurchases)
            .Take(topCount)
            .ToListAsync();

        return results.Select(r => (r.Supplier!, r.TotalPurchases, r.PurchaseOrderCount)).ToList();
    }

    /// <summary>
    /// Check if supplier can make additional purchases based on credit limit
    /// </summary>
    public async Task<bool> CanMakeAdditionalPurchaseAsync(int supplierId, decimal additionalAmount)
    {
        var supplier = await GetByIdAsync(supplierId);
        
        if (supplier == null)
            return false;

        // If no credit limit is set, allow the purchase
        if (supplier.CreditLimit <= 0)
            return true;

        // Check if the new balance would exceed the credit limit
        return (supplier.CurrentBalance + additionalAmount) <= supplier.CreditLimit;
    }
}
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Shared;
using DijaGoldPOS.API.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository implementation for order operations
/// </summary>
public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, int branchId)
    {
        return await _dbSet
            .Include(o => o.Branch)
            .Include(o => o.Customer)
            .Include(o => o.Cashier)
            .Include(o => o.ApprovedByUser)
            .Include(o => o.GoldRate)
            .Include(o => o.FinancialTransaction)
            .Include(o => o.OrderType)
            .Include(o => o.Status)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.KaratType)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && o.BranchId == branchId);
    }

    public async Task<List<Order>> GetByCustomerAsync(int customerId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _dbSet
            .Include(o => o.Branch)
            .Include(o => o.Cashier)
            .Include(o => o.FinancialTransaction)
            .Include(o => o.OrderType)
            .Include(o => o.Status)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.KaratType)
            .Where(o => o.CustomerId == customerId);

        if (fromDate.HasValue)
            query = query.Where(o => o.OrderDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(o => o.OrderDate <= toDate.Value);

        return await query
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetByCashierAsync(string cashierId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _dbSet
            .Include(o => o.Branch)
            .Include(o => o.Customer)
            .Include(o => o.FinancialTransaction)
            .Include(o => o.OrderType)
            .Include(o => o.Status)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.KaratType)
            .Where(o => o.CashierId == cashierId);

        if (fromDate.HasValue)
            query = query.Where(o => o.OrderDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(o => o.OrderDate <= toDate.Value);

        return await query
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<(List<Order> Orders, int TotalCount)> SearchAsync(
        int? branchId = null,
        int? orderTypeId = null,
        int? statusId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? orderNumber = null,
        int? customerId = null,
        string? cashierId = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = _dbSet
            .Include(o => o.Branch)
            .Include(o => o.Customer)
            .Include(o => o.Cashier)
            .Include(o => o.FinancialTransaction)
            .Include(o => o.OrderType)
            .Include(o => o.Status)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.KaratType)
            .AsQueryable();

        // Apply filters
        if (branchId.HasValue)
            query = query.Where(o => o.BranchId == branchId.Value);

        if (orderTypeId.HasValue)
            query = query.Where(o => o.OrderTypeId == orderTypeId.Value);

        if (statusId.HasValue)
            query = query.Where(o => o.StatusId == statusId.Value);

        if (fromDate.HasValue)
            query = query.Where(o => o.OrderDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(o => o.OrderDate <= toDate.Value);

        if (!string.IsNullOrEmpty(orderNumber))
            query = query.Where(o => o.OrderNumber.Contains(orderNumber));

        if (customerId.HasValue)
            query = query.Where(o => o.CustomerId == customerId.Value);

        if (!string.IsNullOrEmpty(cashierId))
            query = query.Where(o => o.CashierId == cashierId);

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination and ordering
        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (orders, totalCount);
    }

    public async Task<OrderSummary> GetSummaryAsync(int? branchId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Orders.AsQueryable();

        if (branchId.HasValue)
            query = query.Where(o => o.BranchId == branchId.Value);

        if (fromDate.HasValue)
            query = query.Where(o => o.OrderDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(o => o.OrderDate <= toDate.Value);

        var summary = new OrderSummary
        {
            TotalOrders = await query.CountAsync(),
            TotalValue = await query.SumAsync(o => o.OrderItems.Sum(oi => oi.TotalAmount))
        };

        // Get order type counts
        var typeCounts = await query
            .GroupBy(o => o.OrderTypeId)
            .Select(g => new { TypeId = g.Key, Count = g.Count() })
            .ToListAsync();

        foreach (var typeCount in typeCounts)
        {
            summary.OrderTypeCounts[typeCount.TypeId] = typeCount.Count;
        }

        // Get status counts
        var statusCounts = await query
            .GroupBy(o => o.StatusId)
            .Select(g => new { StatusId = g.Key, Count = g.Count() })
            .ToListAsync();

        foreach (var statusCount in statusCounts)
        {
            summary.StatusCounts[statusCount.StatusId] = statusCount.Count;
        }

        return summary;
    }

    public async Task<string> GetNextOrderNumberAsync(int branchId)
    {
        var lastOrder = await _context.Orders
            .Where(o => o.BranchId == branchId)
            .OrderByDescending(o => o.OrderNumber)
            .FirstOrDefaultAsync();

        if (lastOrder == null)
        {
            return $"ORD{branchId:D3}0001";
        }

        // Extract the numeric part and increment
        var numericPart = lastOrder.OrderNumber.Substring(6); // Skip "ORDxxx"
        if (int.TryParse(numericPart, out int lastNumber))
        {
            return $"ORD{branchId:D3}{(lastNumber + 1):D4}";
        }

        return $"ORD{branchId:D3}0001";
    }

    public async Task<List<Order>> GetRelatedOrdersAsync(int originalOrderId)
    {
        return await _context.Orders
            .Include(o => o.Branch)
            .Include(o => o.Customer)
            .Include(o => o.Cashier)
            .Include(o => o.FinancialTransaction)
            .Include(o => o.OrderType)
            .Include(o => o.Status)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.KaratType)
            .Where(o => o.OriginalOrderId == originalOrderId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetByDateRangeAsync(int branchId, DateTime fromDate, DateTime toDate)
    {
        return await _context.Orders
            .Include(o => o.Branch)
            .Include(o => o.Customer)
            .Include(o => o.Cashier)
            .Include(o => o.FinancialTransaction)
            .Include(o => o.OrderType)
            .Include(o => o.Status)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.KaratType)
            .Where(o => o.BranchId == branchId && o.OrderDate >= fromDate && o.OrderDate <= toDate)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetByStatusAsync(int statusId, int? branchId = null)
    {
        var query = _context.Orders
            .Include(o => o.Branch)
            .Include(o => o.Customer)
            .Include(o => o.Cashier)
            .Include(o => o.FinancialTransaction)
            .Include(o => o.OrderType)
            .Include(o => o.Status)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.KaratType)
            .Where(o => o.StatusId == statusId);

        if (branchId.HasValue)
            query = query.Where(o => o.BranchId == branchId.Value);

        return await query
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetByTypeAsync(int orderTypeId, int? branchId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Orders
            .Include(o => o.Branch)
            .Include(o => o.Customer)
            .Include(o => o.Cashier)
            .Include(o => o.FinancialTransaction)
            .Include(o => o.OrderType)
            .Include(o => o.Status)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.KaratType)
            .Where(o => o.OrderTypeId == orderTypeId);

        if (branchId.HasValue)
            query = query.Where(o => o.BranchId == branchId.Value);

        if (fromDate.HasValue)
            query = query.Where(o => o.OrderDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(o => o.OrderDate <= toDate.Value);

        return await query
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetWithFinancialTransactionsAsync(int branchId, DateTime fromDate, DateTime toDate)
    {
        return await _context.Orders
            .Include(o => o.Branch)
            .Include(o => o.Customer)
            .Include(o => o.Cashier)
            .Include(o => o.FinancialTransaction)
            .Include(o => o.OrderType)
            .Include(o => o.Status)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.KaratType)
            .Where(o => o.BranchId == branchId && 
                       o.OrderDate >= fromDate && 
                       o.OrderDate <= toDate &&
                       o.FinancialTransactionId != null)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetPendingPaymentAsync(int? branchId = null)
    {
        var query = _context.Orders
            .Include(o => o.Branch)
            .Include(o => o.Customer)
            .Include(o => o.Cashier)
            .Include(o => o.OrderType)
            .Include(o => o.Status)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.KaratType)
            .Where(o => o.FinancialTransactionId == null && o.StatusId == LookupTableConstants.OrderStatusCompleted);

        if (branchId.HasValue)
            query = query.Where(o => o.BranchId == branchId.Value);

        return await query
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetReadyForPickupAsync(int? branchId = null)
    {
        var query = _context.Orders
            .Include(o => o.Branch)
            .Include(o => o.Customer)
            .Include(o => o.Cashier)
            .Include(o => o.FinancialTransaction)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.StatusId == LookupTableConstants.OrderStatusCompleted);

        if (branchId.HasValue)
            query = query.Where(o => o.BranchId == branchId.Value);

        return await query
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }
}

using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository implementation for CustomerPurchase entity
/// </summary>
public class CustomerPurchaseRepository : Repository<CustomerPurchase>, ICustomerPurchaseRepository
{
    public CustomerPurchaseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<CustomerPurchase?> GetByNumberAsync(string purchaseNumber)
    {
        return await _context.CustomerPurchases
            .Where(cp => cp.PurchaseNumber == purchaseNumber)
            .Include(cp => cp.Customer)
            .Include(cp => cp.Branch)
            .Include(cp => cp.PaymentMethod)
            .FirstOrDefaultAsync();
    }

    public async Task<CustomerPurchase?> GetWithItemsAsync(int id)
    {
        return await _context.CustomerPurchases
            .Where(cp => cp.Id == id)
            .Include(cp => cp.Customer)
            .Include(cp => cp.Branch)
            .Include(cp => cp.PaymentMethod)
            .Include(cp => cp.CustomerPurchaseItems)
                .ThenInclude(cpi => cpi.Product)
            .FirstOrDefaultAsync();
    }

    public async Task<List<CustomerPurchase>> GetByCustomerAsync(int customerId)
    {
        return await _context.CustomerPurchases
            .Where(cp => cp.CustomerId == customerId)
            .Include(cp => cp.Branch)
            .Include(cp => cp.PaymentMethod)
            .Include(cp => cp.CustomerPurchaseItems)
            .OrderByDescending(cp => cp.PurchaseDate)
            .ToListAsync();
    }

    public async Task<List<CustomerPurchase>> GetByBranchAsync(int branchId)
    {
        return await _context.CustomerPurchases
            .Where(cp => cp.BranchId == branchId)
            .Include(cp => cp.Customer)
            .Include(cp => cp.PaymentMethod)
            .Include(cp => cp.CustomerPurchaseItems)
            .OrderByDescending(cp => cp.PurchaseDate)
            .ToListAsync();
    }

    public async Task<List<CustomerPurchase>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _context.CustomerPurchases
            .Where(cp => cp.PurchaseDate >= fromDate && cp.PurchaseDate <= toDate)
            .Include(cp => cp.Customer)
            .Include(cp => cp.Branch)
            .Include(cp => cp.PaymentMethod)
            .Include(cp => cp.CustomerPurchaseItems)
            .OrderByDescending(cp => cp.PurchaseDate)
            .ToListAsync();
    }

    public async Task<string> GetNextPurchaseNumberAsync()
    {
        var lastPurchase = await _context.CustomerPurchases
            .OrderByDescending(cp => cp.PurchaseNumber)
            .FirstOrDefaultAsync();

        if (lastPurchase == null)
        {
            return "CP-00001";
        }

        var lastNumber = int.Parse(lastPurchase.PurchaseNumber.Split('-')[1]);
        return $"CP-{(lastNumber + 1):D5}";
    }
}

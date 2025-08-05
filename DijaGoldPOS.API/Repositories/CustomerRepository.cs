using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository implementation for Customer entity with specific business methods
/// </summary>
public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get customer by national ID
    /// </summary>
    public async Task<Customer?> GetByNationalIdAsync(string nationalId)
    {
        return await _dbSet
            .Include(c => c.Transactions)
            .FirstOrDefaultAsync(c => c.NationalId == nationalId);
    }

    /// <summary>
    /// Get customer by mobile number
    /// </summary>
    public async Task<Customer?> GetByMobileNumberAsync(string mobileNumber)
    {
        return await _dbSet
            .Include(c => c.Transactions)
            .FirstOrDefaultAsync(c => c.MobileNumber == mobileNumber);
    }

    /// <summary>
    /// Get customer by email
    /// </summary>
    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(c => c.Transactions)
            .FirstOrDefaultAsync(c => c.Email == email);
    }

    /// <summary>
    /// Search customers by name, mobile, or email
    /// </summary>
    public async Task<List<Customer>> SearchAsync(string searchTerm)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        
        return await _dbSet
            .Where(c => c.FullName.ToLower().Contains(lowerSearchTerm) ||
                       (c.MobileNumber != null && c.MobileNumber.Contains(searchTerm)) ||
                       (c.Email != null && c.Email.ToLower().Contains(lowerSearchTerm)) ||
                       (c.NationalId != null && c.NationalId.Contains(searchTerm)))
            .OrderBy(c => c.FullName)
            .ToListAsync();
    }

    /// <summary>
    /// Get top customers by purchase amount
    /// </summary>
    public async Task<List<Customer>> GetTopCustomersAsync(int topCount = 10, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _dbSet.AsQueryable();

        if (fromDate.HasValue || toDate.HasValue)
        {
            query = query.Include(c => c.Transactions.Where(t => 
                (!fromDate.HasValue || t.TransactionDate >= fromDate.Value) &&
                (!toDate.HasValue || t.TransactionDate <= toDate.Value)));
        }

        return await query
            .OrderByDescending(c => c.TotalPurchaseAmount)
            .Take(topCount)
            .ToListAsync();
    }

    /// <summary>
    /// Get customers with loyalty discounts
    /// </summary>
    public async Task<List<Customer>> GetLoyaltyCustomersAsync()
    {
        return await _dbSet
            .Where(c => c.DefaultDiscountPercentage > 0 || c.MakingChargesWaived)
            .OrderByDescending(c => c.DefaultDiscountPercentage)
            .ToListAsync();
    }

    /// <summary>
    /// Check if national ID exists
    /// </summary>
    public async Task<bool> NationalIdExistsAsync(string nationalId, int? excludeId = null)
    {
        var query = _dbSet.Where(c => c.NationalId == nationalId);
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Check if mobile number exists
    /// </summary>
    public async Task<bool> MobileNumberExistsAsync(string mobileNumber, int? excludeId = null)
    {
        var query = _dbSet.Where(c => c.MobileNumber == mobileNumber);
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Check if email exists
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        var query = _dbSet.Where(c => c.Email == email);
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Update customer purchase statistics
    /// </summary>
    public async Task<Customer?> UpdatePurchaseStatisticsAsync(int customerId, decimal transactionAmount)
    {
        var customer = await GetByIdAsync(customerId);
        
        if (customer != null)
        {
            customer.TotalPurchaseAmount += transactionAmount;
            customer.LastPurchaseDate = DateTime.UtcNow;
            customer.TotalTransactions += 1;
            
            // Update loyalty tier based on total purchases (example logic)
            // 1=Basic, 2=Bronze, 3=Silver, 4=Gold, 5=Platinum
            if (customer.TotalPurchaseAmount >= 100000m) // 100,000 EGP
            {
                customer.LoyaltyTier = 4; // Gold
                customer.DefaultDiscountPercentage = 10m;
            }
            else if (customer.TotalPurchaseAmount >= 50000m) // 50,000 EGP
            {
                customer.LoyaltyTier = 3; // Silver
                customer.DefaultDiscountPercentage = 5m;
            }
            else if (customer.TotalPurchaseAmount >= 10000m) // 10,000 EGP
            {
                customer.LoyaltyTier = 2; // Bronze
                customer.DefaultDiscountPercentage = 2m;
            }
            else
            {
                customer.LoyaltyTier = 1; // Basic
            }
            
            Update(customer);
        }
        
        return customer;
    }
}
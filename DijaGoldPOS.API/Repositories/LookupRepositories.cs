using Microsoft.EntityFrameworkCore;
using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models.LookupTables;
using DijaGoldPOS.API.IRepositories;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository implementation for karat type lookups
/// </summary>
public class KaratTypeLookupRepository : IKaratTypeLookupRepository
{
    private readonly ApplicationDbContext _context;

    public KaratTypeLookupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<KaratTypeLookupDto>> GetAllActiveAsync()
    {
        return await _context.KaratTypeLookups
            .Where(k => k.IsActive)
            .OrderBy(k => k.SortOrder)
            .ThenBy(k => k.Name)
            .Select(k => new KaratTypeLookupDto
            {
                Id = k.Id,
                Name = k.Name,
                Description = k.Description,
                SortOrder = k.SortOrder,
                IsActive = k.IsActive
            })
            .ToListAsync();
    }
}

/// <summary>
/// Repository implementation for financial transaction type lookups
/// </summary>
public class FinancialTransactionTypeLookupRepository : IFinancialTransactionTypeLookupRepository
{
    private readonly ApplicationDbContext _context;

    public FinancialTransactionTypeLookupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FinancialTransactionTypeLookupDto>> GetAllActiveAsync()
    {
        return await _context.FinancialTransactionTypeLookups
            .Where(ft => ft.IsActive)
            .OrderBy(ft => ft.SortOrder)
            .ThenBy(ft => ft.Name)
            .Select(ft => new FinancialTransactionTypeLookupDto
            {
                Id = ft.Id,
                Name = ft.Name,
                Description = ft.Description,
                SortOrder = ft.SortOrder,
                IsActive = ft.IsActive
            })
            .ToListAsync();
    }
}

/// <summary>
/// Repository implementation for payment method lookups
/// </summary>
public class PaymentMethodLookupRepository : IPaymentMethodLookupRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentMethodLookupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PaymentMethodLookupDto>> GetAllActiveAsync()
    {
        return await _context.PaymentMethodLookups
            .Where(pm => pm.IsActive)
            .OrderBy(pm => pm.SortOrder)
            .ThenBy(pm => pm.Name)
            .Select(pm => new PaymentMethodLookupDto
            {
                Id = pm.Id,
                Name = pm.Name,
                Description = pm.Description,
                SortOrder = pm.SortOrder,
                IsActive = pm.IsActive
            })
            .ToListAsync();
    }
}

/// <summary>
/// Repository implementation for financial transaction status lookups
/// </summary>
public class FinancialTransactionStatusLookupRepository : IFinancialTransactionStatusLookupRepository
{
    private readonly ApplicationDbContext _context;

    public FinancialTransactionStatusLookupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FinancialTransactionStatusLookupDto>> GetAllActiveAsync()
    {
        return await _context.FinancialTransactionStatusLookups
            .Where(fts => fts.IsActive)
            .OrderBy(fts => fts.SortOrder)
            .ThenBy(fts => fts.Name)
            .Select(fts => new FinancialTransactionStatusLookupDto
            {
                Id = fts.Id,
                Name = fts.Name,
                Description = fts.Description,
                SortOrder = fts.SortOrder,
                IsActive = fts.IsActive
            })
            .ToListAsync();
    }
}

/// <summary>
/// Repository implementation for charge type lookups
/// </summary>
public class ChargeTypeLookupRepository : IChargeTypeLookupRepository
{
    private readonly ApplicationDbContext _context;

    public ChargeTypeLookupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ChargeTypeLookupDto>> GetAllActiveAsync()
    {
        return await _context.ChargeTypeLookups
            .Where(ct => ct.IsActive)
            .OrderBy(ct => ct.SortOrder)
            .ThenBy(ct => ct.Name)
            .Select(ct => new ChargeTypeLookupDto
            {
                Id = ct.Id,
                Name = ct.Name,
                Description = ct.Description,
                SortOrder = ct.SortOrder,
                IsActive = ct.IsActive
            })
            .ToListAsync();
    }
}

/// <summary>
/// Repository implementation for product category type lookups
/// </summary>
public class ProductCategoryTypeLookupRepository : IProductCategoryTypeLookupRepository
{
    private readonly ApplicationDbContext _context;

    public ProductCategoryTypeLookupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductCategoryTypeLookupDto>> GetAllActiveAsync()
    {
        return await _context.ProductCategoryTypeLookups
            .Where(pct => pct.IsActive)
            .OrderBy(pct => pct.SortOrder)
            .ThenBy(pct => pct.Name)
            .Select(pct => new ProductCategoryTypeLookupDto
            {
                Id = pct.Id,
                Name = pct.Name,
                Description = pct.Description,
                SortOrder = pct.SortOrder,
                IsActive = pct.IsActive
            })
            .ToListAsync();
    }
}

/// <summary>
/// Repository implementation for repair status lookups
/// </summary>
public class RepairStatusLookupRepository : IRepairStatusLookupRepository
{
    private readonly ApplicationDbContext _context;

    public RepairStatusLookupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RepairStatusLookupDto>> GetAllActiveAsync()
    {
        return await _context.RepairStatusLookups
            .Where(rs => rs.IsActive)
            .OrderBy(rs => rs.SortOrder)
            .ThenBy(rs => rs.Name)
            .Select(rs => new RepairStatusLookupDto
            {
                Id = rs.Id,
                Name = rs.Name,
                Description = rs.Description,
                SortOrder = rs.SortOrder,
                IsActive = rs.IsActive
            })
            .ToListAsync();
    }
}

/// <summary>
/// Repository implementation for repair priority lookups
/// </summary>
public class RepairPriorityLookupRepository : IRepairPriorityLookupRepository
{
    private readonly ApplicationDbContext _context;

    public RepairPriorityLookupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RepairPriorityLookupDto>> GetAllActiveAsync()
    {
        return await _context.RepairPriorityLookups
            .Where(rp => rp.IsActive)
            .OrderBy(rp => rp.SortOrder)
            .ThenBy(rp => rp.Name)
            .Select(rp => new RepairPriorityLookupDto
            {
                Id = rp.Id,
                Name = rp.Name,
                Description = rp.Description,
                SortOrder = rp.SortOrder,
                IsActive = rp.IsActive
            })
            .ToListAsync();
    }
}

/// <summary>
/// Repository implementation for order type lookups
/// </summary>
public class OrderTypeLookupRepository : IOrderTypeLookupRepository
{
    private readonly ApplicationDbContext _context;

    public OrderTypeLookupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderTypeLookupDto>> GetAllActiveAsync()
    {
        return await _context.OrderTypeLookups
            .Where(ot => ot.IsActive)
            .OrderBy(ot => ot.SortOrder)
            .ThenBy(ot => ot.Name)
            .Select(ot => new OrderTypeLookupDto
            {
                Id = ot.Id,
                Name = ot.Name,
                Description = ot.Description,
                SortOrder = ot.SortOrder,
                IsActive = ot.IsActive
            })
            .ToListAsync();
    }
}

/// <summary>
/// Repository implementation for order status lookups
/// </summary>
public class OrderStatusLookupRepository : IOrderStatusLookupRepository
{
    private readonly ApplicationDbContext _context;

    public OrderStatusLookupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderStatusLookupDto>> GetAllActiveAsync()
    {
        return await _context.OrderStatusLookups
            .Where(os => os.IsActive)
            .OrderBy(os => os.SortOrder)
            .ThenBy(os => os.Name)
            .Select(os => new OrderStatusLookupDto
            {
                Id = os.Id,
                Name = os.Name,
                Description = os.Description,
                SortOrder = os.SortOrder,
                IsActive = os.IsActive
            })
            .ToListAsync();
    }
}

/// <summary>
/// Repository implementation for business entity type lookups
/// </summary>
public class BusinessEntityTypeLookupRepository : IBusinessEntityTypeLookupRepository
{
    private readonly ApplicationDbContext _context;

    public BusinessEntityTypeLookupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BusinessEntityTypeLookupDto>> GetAllActiveAsync()
    {
        return await _context.BusinessEntityTypeLookups
            .Where(bet => bet.IsActive)
            .OrderBy(bet => bet.SortOrder)
            .ThenBy(bet => bet.Name)
            .Select(bet => new BusinessEntityTypeLookupDto
            {
                Id = bet.Id,
                Name = bet.Name,
                Description = bet.Description,
                SortOrder = bet.SortOrder,
                IsActive = bet.IsActive
            })
            .ToListAsync();
    }
}

/// <summary>
/// Repository implementation for sub-category lookups
/// </summary>
public class SubCategoryLookupRepository : ISubCategoryLookupRepository
{
    private readonly ApplicationDbContext _context;

    public SubCategoryLookupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SubCategoryLookupDto>> GetAllActiveAsync()
    {
        return await _context.SubCategoryLookups
            .Where(sc => sc.IsActive)
            .OrderBy(sc => sc.SortOrder)
            .ThenBy(sc => sc.Name)
            .Select(sc => new SubCategoryLookupDto
            {
                Id = sc.Id,
                Name = sc.Name,
                Description = sc.Description,
                SortOrder = sc.SortOrder,
                IsActive = sc.IsActive
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<SubCategoryLookupDto>> GetByCategoryIdAsync(int categoryId)
    {
        return await _context.SubCategoryLookups
            .Where(sc => sc.IsActive && sc.CategoryTypeId == categoryId)
            .OrderBy(sc => sc.SortOrder)
            .ThenBy(sc => sc.Name)
            .Select(sc => new SubCategoryLookupDto
            {
                Id = sc.Id,
                Name = sc.Name,
                Description = sc.Description,
                SortOrder = sc.SortOrder,
                IsActive = sc.IsActive
            })
            .ToListAsync();
    }
}

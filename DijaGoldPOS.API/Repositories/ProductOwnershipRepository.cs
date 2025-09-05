using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models.ProductModels;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository implementation for ProductOwnership entity
/// </summary>
public class ProductOwnershipRepository : Repository<ProductOwnership>, IProductOwnershipRepository
{
    public ProductOwnershipRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<ProductOwnership>> GetByProductAndBranchAsync(int productId, int branchId)
    {
        return await _context.ProductOwnerships
            .Where(po => po.ProductId == productId && 
                        po.BranchId == branchId && 
                        po.IsActive)
            .Include(po => po.Product)
            .Include(po => po.Supplier)
            .Include(po => po.PurchaseOrder)
            .Include(po => po.CustomerPurchase)
            .ToListAsync();
    }

    public async Task<List<ProductOwnership>> GetBySupplierAsync(int supplierId)
    {
        return await _context.ProductOwnerships
            .Where(po => po.SupplierId == supplierId && po.IsActive)
            .Include(po => po.Product)
            .Include(po => po.Branch)
            .Include(po => po.PurchaseOrder)
            .ToListAsync();
    }

    public async Task<List<ProductOwnership>> GetByPurchaseOrderAsync(int purchaseOrderId)
    {
        return await _context.ProductOwnerships
            .Where(po => po.PurchaseOrderId == purchaseOrderId && po.IsActive)
            .Include(po => po.Product)
            .Include(po => po.Branch)
            .Include(po => po.Supplier)
            .ToListAsync();
    }

    public async Task<List<ProductOwnership>> GetByCustomerPurchaseAsync(int customerPurchaseId)
    {
        return await _context.ProductOwnerships
            .Where(po => po.CustomerPurchaseId == customerPurchaseId && po.IsActive)
            .Include(po => po.Product)
            .Include(po => po.Branch)
            .ToListAsync();
    }

    public async Task<(decimal TotalOwnedQuantity, decimal TotalOwnedWeight, decimal TotalQuantity, decimal TotalWeight)> GetOwnershipSummaryAsync(int productId)
    {
        var ownerships = await _context.ProductOwnerships
            .Where(po => po.ProductId == productId && po.IsActive)
            .ToListAsync();

        var totalOwnedQuantity = ownerships.Sum(po => po.OwnedQuantity);
        var totalOwnedWeight = ownerships.Sum(po => po.OwnedWeight);
        var totalQuantity = ownerships.Sum(po => po.TotalQuantity);
        var totalWeight = ownerships.Sum(po => po.TotalWeight);

        return (totalOwnedQuantity, totalOwnedWeight, totalQuantity, totalWeight);
    }

    public async Task<List<ProductOwnership>> GetLowOwnershipProductsAsync(decimal threshold = 50m)
    {
        return await _context.ProductOwnerships
            .Where(po => po.IsActive && 
                        po.OwnershipPercentage < threshold &&
                        po.TotalQuantity > 0)
            .Include(po => po.Product)
            .Include(po => po.Supplier)
            .Include(po => po.Branch)
            .ToListAsync();
    }

    public async Task<List<ProductOwnership>> GetProductsWithOutstandingPaymentsAsync()
    {
        return await _context.ProductOwnerships
            .Where(po => po.IsActive && 
                        po.OutstandingAmount > 0)
            .Include(po => po.Product)
            .Include(po => po.Supplier)
            .Include(po => po.Branch)
            .ToListAsync();
    }

    public async Task<List<ProductOwnership>> GetByBranchAsync(int branchId)
    {
        return await _context.ProductOwnerships
            .Where(po => po.BranchId == branchId && po.IsActive)
            .Include(po => po.Product)
            .Include(po => po.Supplier)
            .Include(po => po.PurchaseOrder)
            .Include(po => po.CustomerPurchase)
            .ToListAsync();
    }

    public async Task<ProductOwnership?> GetWithDetailsAsync(int id)
    {
        return await _context.ProductOwnerships
            .Where(po => po.Id == id)
            .Include(po => po.Product)
            .Include(po => po.Supplier)
            .Include(po => po.Branch)
            .Include(po => po.PurchaseOrder)
            .Include(po => po.CustomerPurchase)
            .Include(po => po.OwnershipMovements.OrderByDescending(om => om.MovementDate))
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get ownership records with pagination and filtering
    /// </summary>
    public async Task<(List<ProductOwnership> Items, int TotalCount)> GetWithPaginationAsync(
        int branchId,
        string? searchTerm = null,
        int? supplierId = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var query = _context.ProductOwnerships
            .Where(po => po.BranchId == branchId && po.IsActive)
            .Include(po => po.Product)
            .Include(po => po.Supplier)
            .Include(po => po.Branch)
            .Include(po => po.PurchaseOrder)
            .Include(po => po.CustomerPurchase)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(po => 
                po.Product.Name.Contains(searchTerm) ||
                po.Product.ProductCode.Contains(searchTerm) ||
                (po.Supplier != null && po.Supplier.CompanyName.Contains(searchTerm)) ||
                (po.PurchaseOrder != null && po.PurchaseOrder.PurchaseOrderNumber.Contains(searchTerm))
            );
        }

        // Apply supplier filter
        if (supplierId.HasValue)
        {
            query = query.Where(po => po.SupplierId == supplierId.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var items = await query
            .OrderByDescending(po => po.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}

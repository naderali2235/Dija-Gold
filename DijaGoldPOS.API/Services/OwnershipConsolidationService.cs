using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models.ProductModels;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service for consolidating ownership records for the same product from the same supplier
/// </summary>
public interface IOwnershipConsolidationService
{
    /// <summary>
    /// Consolidate ownership records for a specific product and supplier
    /// </summary>
    Task<ConsolidationResultDto> ConsolidateOwnershipAsync(int productId, int supplierId, int branchId);
    
    /// <summary>
    /// Consolidate all ownership records for a supplier across all products
    /// </summary>
    Task<List<ConsolidationResultDto>> ConsolidateSupplierOwnershipAsync(int supplierId, int branchId);
    
    /// <summary>
    /// Get consolidation opportunities (products with multiple ownership records from same supplier)
    /// </summary>
    Task<List<ConsolidationOpportunityDto>> GetConsolidationOpportunitiesAsync(int branchId);
    
    /// <summary>
    /// Calculate weighted average cost for ownership consolidation
    /// </summary>
    Task<WeightedAverageCostDto> CalculateWeightedAverageCostAsync(List<int> ownershipIds);
}

public class OwnershipConsolidationService : IOwnershipConsolidationService
{
    private readonly IProductOwnershipRepository _ownershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OwnershipConsolidationService> _logger;

    public OwnershipConsolidationService(
        IProductOwnershipRepository ownershipRepository,
        IUnitOfWork unitOfWork,
        ILogger<OwnershipConsolidationService> logger)
    {
        _ownershipRepository = ownershipRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ConsolidationResultDto> ConsolidateOwnershipAsync(int productId, int supplierId, int branchId)
    {
        try
        {
            _logger.LogInformation("Starting ownership consolidation for ProductId: {ProductId}, SupplierId: {SupplierId}, BranchId: {BranchId}", 
                productId, supplierId, branchId);

            // Get all ownership records for this product, supplier, and branch
            var ownerships = await _ownershipRepository.GetByProductAndBranchAsync(productId, branchId);
            var supplierOwnerships = ownerships.Where(o => o.SupplierId == supplierId && o.IsActive).ToList();

            if (supplierOwnerships.Count <= 1)
            {
                return new ConsolidationResultDto
                {
                    Success = false,
                    Message = "No consolidation needed - only one or no ownership records found",
                    ConsolidatedRecords = 0
                };
            }

            // Calculate weighted average cost
            var weightedAverage = await CalculateWeightedAverageCostAsync(supplierOwnerships.Select(o => o.Id).ToList());

            // Create consolidated ownership record
            var consolidatedOwnership = new ProductOwnership
            {
                ProductId = productId,
                BranchId = branchId,
                SupplierId = supplierId,
                PurchaseOrderId = null, // Consolidated record doesn't link to specific PO
                TotalQuantity = supplierOwnerships.Sum(o => o.TotalQuantity),
                TotalWeight = supplierOwnerships.Sum(o => o.TotalWeight),
                OwnedQuantity = supplierOwnerships.Sum(o => o.OwnedQuantity),
                OwnedWeight = supplierOwnerships.Sum(o => o.OwnedWeight),
                TotalCost = supplierOwnerships.Sum(o => o.TotalCost),
                AmountPaid = supplierOwnerships.Sum(o => o.AmountPaid),
                OutstandingAmount = supplierOwnerships.Sum(o => o.OutstandingAmount),
                Notes = $"Consolidated from {supplierOwnerships.Count} ownership records on {DateTime.UtcNow:yyyy-MM-dd}",
                IsActive = true
            };

            // Recalculate ownership percentage
            consolidatedOwnership.OwnershipPercentage = consolidatedOwnership.TotalQuantity > 0 
                ? consolidatedOwnership.OwnedQuantity / consolidatedOwnership.TotalQuantity 
                : 0;

            // Add consolidated record
            await _ownershipRepository.AddAsync(consolidatedOwnership);

            // Mark original records as inactive (soft delete)
            foreach (var ownership in supplierOwnerships)
            {
                ownership.IsActive = false;
                ownership.Notes += $" [Consolidated on {DateTime.UtcNow:yyyy-MM-dd HH:mm}]";
                await _ownershipRepository.UpdateAsync(ownership);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully consolidated {Count} ownership records into one", supplierOwnerships.Count);

            return new ConsolidationResultDto
            {
                Success = true,
                Message = $"Successfully consolidated {supplierOwnerships.Count} ownership records",
                ConsolidatedRecords = supplierOwnerships.Count,
                NewOwnershipId = consolidatedOwnership.Id,
                WeightedAverageCost = weightedAverage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consolidating ownership for ProductId: {ProductId}, SupplierId: {SupplierId}", 
                productId, supplierId);
            throw;
        }
    }

    public async Task<List<ConsolidationResultDto>> ConsolidateSupplierOwnershipAsync(int supplierId, int branchId)
    {
        var results = new List<ConsolidationResultDto>();
        
        try
        {
            var supplierOwnerships = await _ownershipRepository.GetBySupplierAsync(supplierId);
            var branchOwnerships = supplierOwnerships.Where(o => o.BranchId == branchId && o.IsActive).ToList();
            
            // Group by product
            var productGroups = branchOwnerships.GroupBy(o => o.ProductId);
            
            foreach (var group in productGroups.Where(g => g.Count() > 1))
            {
                var result = await ConsolidateOwnershipAsync(group.Key, supplierId, branchId);
                results.Add(result);
            }
            
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consolidating supplier ownership for SupplierId: {SupplierId}", supplierId);
            throw;
        }
    }

    public async Task<List<ConsolidationOpportunityDto>> GetConsolidationOpportunitiesAsync(int branchId)
    {
        try
        {
            var allOwnerships = await _ownershipRepository.GetByBranchAsync(branchId);
            var activeOwnerships = allOwnerships.Where(o => o.IsActive && o.SupplierId.HasValue).ToList();
            
            // Group by product and supplier to find consolidation opportunities
            var opportunities = activeOwnerships
                .GroupBy(o => new { o.ProductId, o.SupplierId })
                .Where(g => g.Count() > 1)
                .Select(g => new ConsolidationOpportunityDto
                {
                    ProductId = g.Key.ProductId,
                    SupplierId = g.Key.SupplierId!.Value,
                    ProductName = g.First().Product?.Name ?? "Unknown",
                    SupplierName = g.First().Supplier?.CompanyName ?? "Unknown",
                    RecordCount = g.Count(),
                    TotalQuantity = g.Sum(o => o.TotalQuantity),
                    TotalWeight = g.Sum(o => o.TotalWeight),
                    TotalCost = g.Sum(o => o.TotalCost),
                    OutstandingAmount = g.Sum(o => o.OutstandingAmount),
                    PotentialSavings = CalculatePotentialSavings(g.ToList())
                })
                .OrderByDescending(o => o.RecordCount)
                .ToList();
            
            return opportunities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consolidation opportunities for BranchId: {BranchId}", branchId);
            throw;
        }
    }

    public async Task<WeightedAverageCostDto> CalculateWeightedAverageCostAsync(List<int> ownershipIds)
    {
        try
        {
            var ownerships = new List<ProductOwnership>();
            foreach (var id in ownershipIds)
            {
                var ownership = await _ownershipRepository.GetByIdAsync(id);
                if (ownership != null)
                    ownerships.Add(ownership);
            }

            if (!ownerships.Any())
            {
                return new WeightedAverageCostDto { WeightedAverageCostPerGram = 0, TotalWeight = 0, TotalCost = 0 };
            }

            var totalWeight = ownerships.Sum(o => o.TotalWeight);
            var totalCost = ownerships.Sum(o => o.TotalCost);
            var weightedAverageCostPerGram = totalWeight > 0 ? totalCost / totalWeight : 0;

            return new WeightedAverageCostDto
            {
                WeightedAverageCostPerGram = weightedAverageCostPerGram,
                TotalWeight = totalWeight,
                TotalCost = totalCost,
                RecordCount = ownerships.Count,
                CostBreakdown = ownerships.Select(o => new CostBreakdownDto
                {
                    OwnershipId = o.Id,
                    Weight = o.TotalWeight,
                    Cost = o.TotalCost,
                    CostPerGram = o.TotalWeight > 0 ? o.TotalCost / o.TotalWeight : 0,
                    PurchaseOrderNumber = o.PurchaseOrder?.PurchaseOrderNumber ?? "N/A"
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating weighted average cost");
            throw;
        }
    }

    private decimal CalculatePotentialSavings(List<ProductOwnership> ownerships)
    {
        // Calculate potential administrative savings from consolidation
        // This is a simplified calculation - you can enhance based on business rules
        var recordCount = ownerships.Count;
        var avgProcessingCost = 50m; // Estimated cost per ownership record maintenance
        return (recordCount - 1) * avgProcessingCost;
    }
}

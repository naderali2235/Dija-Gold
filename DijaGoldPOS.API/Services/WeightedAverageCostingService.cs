using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models.ProductModels;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service for calculating weighted average costs for products from multiple sources
/// </summary>
public interface IWeightedAverageCostingService
{
    /// <summary>
    /// Calculate weighted average cost for a product across all ownership records
    /// </summary>
    Task<WeightedAverageCostResultDto> CalculateProductWeightedAverageCostAsync(int productId, int branchId);
    
    /// <summary>
    /// Calculate weighted average cost for manufacturing using multiple raw materials
    /// </summary>
    Task<WeightedAverageCostResultDto> CalculateManufacturingWeightedCostAsync(List<ProductManufactureRawMaterialDto> rawMaterials);
    
    /// <summary>
    /// Update product costs based on weighted average costing method
    /// </summary>
    Task<bool> UpdateProductCostWithWeightedAverageAsync(int productId, int branchId);
    
    /// <summary>
    /// Get cost analysis for a product showing all cost sources
    /// </summary>
    Task<ProductCostAnalysisDto> GetProductCostAnalysisAsync(int productId, int branchId);
    
    /// <summary>
    /// Calculate FIFO (First In, First Out) cost for comparison
    /// </summary>
    Task<FifoCostResultDto> CalculateFifoCostAsync(int productId, int branchId, decimal requestedQuantity);
    
    /// <summary>
    /// Calculate LIFO (Last In, First Out) cost for comparison
    /// </summary>
    Task<LifoCostResultDto> CalculateLifoCostAsync(int productId, int branchId, decimal requestedQuantity);
}

public class WeightedAverageCostingService : IWeightedAverageCostingService
{
    private readonly IProductOwnershipRepository _ownershipRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WeightedAverageCostingService> _logger;

    public WeightedAverageCostingService(
        IProductOwnershipRepository ownershipRepository,
        IProductRepository productRepository,
        IPurchaseOrderRepository purchaseOrderRepository,
        IUnitOfWork unitOfWork,
        ILogger<WeightedAverageCostingService> logger)
    {
        _ownershipRepository = ownershipRepository;
        _productRepository = productRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<WeightedAverageCostResultDto> CalculateProductWeightedAverageCostAsync(int productId, int branchId)
    {
        try
        {
            _logger.LogInformation("Calculating weighted average cost for ProductId: {ProductId}, BranchId: {BranchId}", 
                productId, branchId);

            var ownerships = await _ownershipRepository.GetByProductAndBranchAsync(productId, branchId);
            var activeOwnerships = ownerships.Where(o => o.IsActive && o.OwnedQuantity > 0).ToList();

            if (!activeOwnerships.Any())
            {
                return new WeightedAverageCostResultDto
                {
                    Success = false,
                    Message = "No active ownership records found for this product",
                    WeightedAverageCostPerGram = 0,
                    WeightedAverageCostPerUnit = 0
                };
            }

            // Calculate weighted average based on owned quantities and costs
            var totalOwnedWeight = activeOwnerships.Sum(o => o.OwnedWeight);
            var totalOwnedQuantity = activeOwnerships.Sum(o => o.OwnedQuantity);
            var totalOwnedCost = activeOwnerships.Sum(o => o.TotalCost * (o.OwnedQuantity / o.TotalQuantity));

            var weightedAvgCostPerGram = totalOwnedWeight > 0 ? totalOwnedCost / totalOwnedWeight : 0;
            var weightedAvgCostPerUnit = totalOwnedQuantity > 0 ? totalOwnedCost / totalOwnedQuantity : 0;

            var costSources = activeOwnerships.Select(o => new CostSourceDto
            {
                SourceId = o.Id,
                SourceType = "ProductOwnership",
                PurchaseOrderNumber = o.PurchaseOrder?.PurchaseOrderNumber ?? "N/A",
                SupplierName = o.Supplier?.CompanyName ?? "Unknown",
                Quantity = o.OwnedQuantity,
                Weight = o.OwnedWeight,
                TotalCost = o.TotalCost * (o.OwnedQuantity / o.TotalQuantity),
                CostPerGram = o.OwnedWeight > 0 ? (o.TotalCost * (o.OwnedQuantity / o.TotalQuantity)) / o.OwnedWeight : 0,
                CostPerUnit = o.OwnedQuantity > 0 ? (o.TotalCost * (o.OwnedQuantity / o.TotalQuantity)) / o.OwnedQuantity : 0,
                ContributionPercentage = totalOwnedQuantity > 0 ? o.OwnedQuantity / totalOwnedQuantity : 0,
                PurchaseDate = o.CreatedAt
            }).OrderBy(cs => cs.PurchaseDate).ToList();

            return new WeightedAverageCostResultDto
            {
                Success = true,
                Message = $"Calculated weighted average cost from {activeOwnerships.Count} ownership records",
                WeightedAverageCostPerGram = weightedAvgCostPerGram,
                WeightedAverageCostPerUnit = weightedAvgCostPerUnit,
                TotalQuantity = totalOwnedQuantity,
                TotalWeight = totalOwnedWeight,
                TotalCost = totalOwnedCost,
                CostSources = costSources
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating weighted average cost for ProductId: {ProductId}", productId);
            throw;
        }
    }

    public async Task<WeightedAverageCostResultDto> CalculateManufacturingWeightedCostAsync(List<ProductManufactureRawMaterialDto> rawMaterials)
    {
        try
        {
            if (!rawMaterials.Any())
            {
                return new WeightedAverageCostResultDto
                {
                    Success = false,
                    Message = "No raw materials provided",
                    WeightedAverageCostPerGram = 0
                };
            }

            var totalWeight = rawMaterials.Sum(rm => rm.ConsumedWeight);
            var totalCost = rawMaterials.Sum(rm => rm.TotalRawMaterialCost);
            var weightedAvgCostPerGram = totalWeight > 0 ? totalCost / totalWeight : 0;

            var costSources = rawMaterials.Select(rm => new CostSourceDto
            {
                SourceId = rm.PurchaseOrderItemId,
                SourceType = "RawMaterial",
                PurchaseOrderNumber = rm.PurchaseOrderNumber,
                SupplierName = rm.SupplierName,
                Weight = rm.ConsumedWeight,
                TotalCost = rm.TotalRawMaterialCost,
                CostPerGram = rm.CostPerGram,
                ContributionPercentage = rm.ContributionPercentage,
                KaratType = rm.KaratTypeName
            }).OrderBy(cs => cs.SequenceOrder).ToList();

            return new WeightedAverageCostResultDto
            {
                Success = true,
                Message = $"Calculated manufacturing weighted cost from {rawMaterials.Count} raw material sources",
                WeightedAverageCostPerGram = weightedAvgCostPerGram,
                TotalWeight = totalWeight,
                TotalCost = totalCost,
                CostSources = costSources
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating manufacturing weighted cost");
            throw;
        }
    }

    public async Task<bool> UpdateProductCostWithWeightedAverageAsync(int productId, int branchId)
    {
        try
        {
            var weightedCost = await CalculateProductWeightedAverageCostAsync(productId, branchId);
            
            if (!weightedCost.Success)
            {
                _logger.LogWarning("Cannot update product cost - weighted average calculation failed for ProductId: {ProductId}", productId);
                return false;
            }

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Product not found for ProductId: {ProductId}", productId);
                return false;
            }

            // Update product with weighted average cost
            // CostPrice is computed from StandardCost, so update that instead
            product.StandardCost = weightedCost.WeightedAverageCostPerUnit;
            // You might want to add a field for cost per gram if needed
            // product.CostPerGram = weightedCost.WeightedAverageCostPerGram;

            await _productRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated product cost for ProductId: {ProductId} with weighted average cost: {Cost}", 
                productId, weightedCost.WeightedAverageCostPerUnit);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product cost with weighted average for ProductId: {ProductId}", productId);
            throw;
        }
    }

    public async Task<ProductCostAnalysisDto> GetProductCostAnalysisAsync(int productId, int branchId)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID {productId} not found");
            }

            var weightedAverage = await CalculateProductWeightedAverageCostAsync(productId, branchId);
            var fifo = await CalculateFifoCostAsync(productId, branchId, 1); // Calculate for 1 unit
            var lifo = await CalculateLifoCostAsync(productId, branchId, 1); // Calculate for 1 unit

            return new ProductCostAnalysisDto
            {
                ProductId = productId,
                ProductName = product.Name,
                ProductCode = product.ProductCode,
                CurrentCostPrice = product.CostPrice,
                WeightedAverageCost = weightedAverage,
                FifoCost = fifo,
                LifoCost = lifo,
                RecommendedCostingMethod = DetermineRecommendedMethod(weightedAverage, fifo, lifo),
                AnalysisDate = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product cost analysis for ProductId: {ProductId}", productId);
            throw;
        }
    }

    public async Task<FifoCostResultDto> CalculateFifoCostAsync(int productId, int branchId, decimal requestedQuantity)
    {
        try
        {
            var ownerships = await _ownershipRepository.GetByProductAndBranchAsync(productId, branchId);
            var activeOwnerships = ownerships
                .Where(o => o.IsActive && o.OwnedQuantity > 0)
                .OrderBy(o => o.CreatedAt) // FIFO - oldest first
                .ToList();

            var fifoCost = CalculateCostByMethod(activeOwnerships, requestedQuantity, "FIFO");
            
            return new FifoCostResultDto
            {
                Success = fifoCost.Success,
                Message = fifoCost.Message,
                TotalCost = fifoCost.TotalCost,
                CostPerUnit = fifoCost.CostPerUnit,
                CostPerGram = fifoCost.CostPerGram,
                UsedSources = fifoCost.UsedSources
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating FIFO cost for ProductId: {ProductId}", productId);
            throw;
        }
    }

    public async Task<LifoCostResultDto> CalculateLifoCostAsync(int productId, int branchId, decimal requestedQuantity)
    {
        try
        {
            var ownerships = await _ownershipRepository.GetByProductAndBranchAsync(productId, branchId);
            var activeOwnerships = ownerships
                .Where(o => o.IsActive && o.OwnedQuantity > 0)
                .OrderByDescending(o => o.CreatedAt) // LIFO - newest first
                .ToList();

            var lifoCost = CalculateCostByMethod(activeOwnerships, requestedQuantity, "LIFO");
            
            return new LifoCostResultDto
            {
                Success = lifoCost.Success,
                Message = lifoCost.Message,
                TotalCost = lifoCost.TotalCost,
                CostPerUnit = lifoCost.CostPerUnit,
                CostPerGram = lifoCost.CostPerGram,
                UsedSources = lifoCost.UsedSources
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating LIFO cost for ProductId: {ProductId}", productId);
            throw;
        }
    }

    private CostCalculationResult CalculateCostByMethod(List<ProductOwnership> ownerships, decimal requestedQuantity, string method)
    {
        var usedSources = new List<CostSourceDto>();
        var totalCost = 0m;
        var remainingQuantity = requestedQuantity;

        foreach (var ownership in ownerships)
        {
            if (remainingQuantity <= 0) break;

            var quantityToUse = Math.Min(remainingQuantity, ownership.OwnedQuantity);
            var costForThisSource = (ownership.TotalCost * (ownership.OwnedQuantity / ownership.TotalQuantity)) * (quantityToUse / ownership.OwnedQuantity);
            
            usedSources.Add(new CostSourceDto
            {
                SourceId = ownership.Id,
                SourceType = "ProductOwnership",
                PurchaseOrderNumber = ownership.PurchaseOrder?.PurchaseOrderNumber ?? "N/A",
                SupplierName = ownership.Supplier?.CompanyName ?? "Unknown",
                Quantity = quantityToUse,
                Weight = ownership.OwnedWeight * (quantityToUse / ownership.OwnedQuantity),
                TotalCost = costForThisSource,
                CostPerUnit = ownership.OwnedQuantity > 0 ? costForThisSource / quantityToUse : 0,
                PurchaseDate = ownership.CreatedAt
            });

            totalCost += costForThisSource;
            remainingQuantity -= quantityToUse;
        }

        var success = remainingQuantity <= 0;
        var totalWeight = usedSources.Sum(s => s.Weight);

        return new CostCalculationResult
        {
            Success = success,
            Message = success ? $"{method} cost calculated successfully" : $"Insufficient quantity available for {method} calculation",
            TotalCost = totalCost,
            CostPerUnit = requestedQuantity > 0 ? totalCost / requestedQuantity : 0,
            CostPerGram = totalWeight > 0 ? totalCost / totalWeight : 0,
            UsedSources = usedSources
        };
    }

    private string DetermineRecommendedMethod(WeightedAverageCostResultDto weightedAvg, FifoCostResultDto fifo, LifoCostResultDto lifo)
    {
        // Simple logic to recommend costing method
        // In practice, this would be more sophisticated based on business rules
        
        if (!weightedAvg.Success) return "FIFO"; // Fallback
        
        var costs = new[]
        {
            ("WeightedAverage", weightedAvg.WeightedAverageCostPerUnit),
            ("FIFO", fifo.Success ? fifo.CostPerUnit : decimal.MaxValue),
            ("LIFO", lifo.Success ? lifo.CostPerUnit : decimal.MaxValue)
        };

        // For gold business, weighted average is often preferred for consistency
        return "WeightedAverage";
    }

    private class CostCalculationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal TotalCost { get; set; }
        public decimal CostPerUnit { get; set; }
        public decimal CostPerGram { get; set; }
        public List<CostSourceDto> UsedSources { get; set; } = new();
    }
}

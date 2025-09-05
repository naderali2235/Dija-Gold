using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Models.ProductModels;
using DijaGoldPOS.API.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service implementation for product ownership management
/// </summary>
public class ProductOwnershipService : IProductOwnershipService
{
    private readonly IProductOwnershipRepository _productOwnershipRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductOwnershipService> _logger;

    public ProductOwnershipService(
        IProductOwnershipRepository productOwnershipRepository,
        IProductRepository productRepository,
        IInventoryRepository inventoryRepository,
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProductOwnershipService> logger)
    {
        _productOwnershipRepository = productOwnershipRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Create or update product ownership
    /// </summary>
    public async Task<ProductOwnershipDto> CreateOrUpdateOwnershipAsync(ProductOwnershipRequest request, string userId)
    {
        try
        {
            _logger.LogInformation("Creating/updating product ownership for ProductId: {ProductId}, BranchId: {BranchId}", 
                request.ProductId, request.BranchId);

            // Validate product exists
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                throw new ArgumentException($"Product with ID {request.ProductId} not found");
            }

            // Check if ownership record already exists
            var existingOwnerships = await _productOwnershipRepository.GetByProductAndBranchAsync(request.ProductId, request.BranchId);
            var existingOwnership = existingOwnerships.FirstOrDefault(o => 
                o.SupplierId == request.SupplierId && 
                o.PurchaseOrderId == request.PurchaseOrderId && 
                o.CustomerPurchaseId == request.CustomerPurchaseId);

            ProductOwnership ownership;
            if (existingOwnership != null)
            {
                // Update existing ownership
                _logger.LogInformation("Updating existing ownership record ID: {OwnershipId}", existingOwnership.Id);
                
                existingOwnership.TotalQuantity += request.TotalQuantity;
                existingOwnership.TotalWeight += request.TotalWeight;
                existingOwnership.OwnedQuantity += request.OwnedQuantity;
                existingOwnership.OwnedWeight += request.OwnedWeight;
                existingOwnership.TotalCost += request.TotalCost;
                existingOwnership.AmountPaid += request.AmountPaid;
                existingOwnership.OutstandingAmount = existingOwnership.TotalCost - existingOwnership.AmountPaid;
                existingOwnership.OwnershipPercentage = existingOwnership.TotalQuantity > 0 
                    ? (existingOwnership.OwnedQuantity / existingOwnership.TotalQuantity) * 100
                    : 0;
                existingOwnership.ModifiedAt = DateTime.UtcNow;
                existingOwnership.ModifiedBy = userId;

                ownership = existingOwnership;
            }
            else
            {
                // Create new ownership record
                _logger.LogInformation("Creating new ownership record");
                ownership = _mapper.Map<ProductOwnership>(request);
                ownership.CreatedBy = userId;
                ownership.ModifiedBy = userId;
                
                await _productOwnershipRepository.AddAsync(ownership);
            }

            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<ProductOwnershipDto>(ownership);
            _logger.LogInformation("Successfully created/updated ownership for ProductId: {ProductId}", request.ProductId);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating product ownership for ProductId: {ProductId}", request.ProductId);
            throw;
        }
    }

    /// <summary>
    /// Validate product ownership for sales
    /// </summary>
    public async Task<OwnershipValidationResult> ValidateProductOwnershipAsync(int productId, int branchId, decimal requestedQuantity)
    {
        try
        {
            _logger.LogInformation("Validating product ownership for ProductId: {ProductId}, BranchId: {BranchId}, RequestedQuantity: {RequestedQuantity}", 
                productId, branchId, requestedQuantity);

            var ownerships = await _productOwnershipRepository.GetByProductAndBranchAsync(productId, branchId);
            var activeOwnerships = ownerships.Where(o => o.IsActive).ToList();

            if (!activeOwnerships.Any())
            {
                return new OwnershipValidationResult
                {
                    CanSell = false,
                    Message = "No active ownership records found for this product and branch",
                    Warnings = new List<string> { "Product may not be available for sale" }
                };
            }

            var totalOwnedQuantity = activeOwnerships.Sum(o => o.OwnedQuantity);
            var totalQuantity = activeOwnerships.Sum(o => o.TotalQuantity);
            var totalWeight = activeOwnerships.Sum(o => o.TotalWeight);
            var totalOwnedWeight = activeOwnerships.Sum(o => o.OwnedWeight);
            var ownershipPercentage = totalQuantity > 0 ? (totalOwnedQuantity / totalQuantity) * 100 : 0;

            var warnings = new List<string>();

            // Check if we have enough owned quantity
            if (requestedQuantity > totalOwnedQuantity)
            {
                return new OwnershipValidationResult
                {
                    CanSell = false,
                    Message = $"Insufficient owned quantity. Available: {totalOwnedQuantity}, Requested: {requestedQuantity}",
                    OwnedQuantity = totalOwnedQuantity,
                    TotalQuantity = totalQuantity,
                    OwnedWeight = totalOwnedWeight,
                    TotalWeight = totalWeight,
                    OwnershipPercentage = ownershipPercentage,
                    Warnings = warnings
                };
            }

            // Check ownership percentage warnings
            if (ownershipPercentage < 50)
            {
                warnings.Add($"Low ownership percentage: {ownershipPercentage:F2}%");
            }

            // Check for outstanding payments
            var outstandingPayments = activeOwnerships.Where(o => o.OutstandingAmount > 0).ToList();
            if (outstandingPayments.Any())
            {
                var totalOutstanding = outstandingPayments.Sum(o => o.OutstandingAmount);
                warnings.Add($"Outstanding payments: ${totalOutstanding:F2}");
            }

            _logger.LogInformation("Ownership validation completed. CanSell: {CanSell}, OwnedQuantity: {OwnedQuantity}", 
                true, totalOwnedQuantity);

            return new OwnershipValidationResult
            {
                CanSell = true,
                Message = "Product ownership validated successfully",
                OwnedQuantity = totalOwnedQuantity,
                TotalQuantity = totalQuantity,
                OwnedWeight = totalOwnedWeight,
                TotalWeight = totalWeight,
                OwnershipPercentage = ownershipPercentage,
                Warnings = warnings
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating product ownership for ProductId: {ProductId}", productId);
            throw;
        }
    }

    /// <summary>
    /// Update ownership after payment
    /// </summary>
    public async Task<bool> UpdateOwnershipAfterPaymentAsync(int productOwnershipId, decimal paymentAmount, string referenceNumber, string userId)
    {
        try
        {
            _logger.LogInformation("Updating ownership after payment. OwnershipId: {OwnershipId}, PaymentAmount: {PaymentAmount}", 
                productOwnershipId, paymentAmount);

            var ownership = await _productOwnershipRepository.GetByIdAsync(productOwnershipId);
            if (ownership == null)
            {
                _logger.LogWarning("Product ownership not found. ID: {OwnershipId}", productOwnershipId);
                return false;
            }

            if (!ownership.IsActive)
            {
                _logger.LogWarning("Product ownership is not active. ID: {OwnershipId}", productOwnershipId);
                return false;
            }

            // Calculate new owned quantities based on payment percentage
            var paymentPercentage = paymentAmount / ownership.TotalCost;
            var newOwnedQuantity = ownership.TotalQuantity * paymentPercentage;
            var newOwnedWeight = ownership.TotalWeight * paymentPercentage;

            // Update ownership record
            ownership.AmountPaid += paymentAmount;
            ownership.OwnedQuantity = newOwnedQuantity;
            ownership.OwnedWeight = newOwnedWeight;
            ownership.OutstandingAmount = ownership.TotalCost - ownership.AmountPaid;
            ownership.OwnershipPercentage = ownership.TotalQuantity > 0 
                ? (ownership.OwnedQuantity / ownership.TotalQuantity) * 100
                : 0;
            ownership.ModifiedAt = DateTime.UtcNow;
            ownership.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully updated ownership after payment. OwnershipId: {OwnershipId}", productOwnershipId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ownership after payment. OwnershipId: {OwnershipId}", productOwnershipId);
            throw;
        }
    }

    /// <summary>
    /// Update ownership after sale
    /// </summary>
    public async Task<bool> UpdateOwnershipAfterSaleAsync(int productId, int branchId, decimal soldQuantity, string referenceNumber, string userId)
    {
        try
        {
            _logger.LogInformation("Updating ownership after sale. ProductId: {ProductId}, BranchId: {BranchId}, SoldQuantity: {SoldQuantity}", 
                productId, branchId, soldQuantity);

            var ownerships = await _productOwnershipRepository.GetByProductAndBranchAsync(productId, branchId);
            var activeOwnerships = ownerships.Where(o => o.IsActive && o.OwnedQuantity > 0).ToList();

            if (!activeOwnerships.Any())
            {
                _logger.LogWarning("No active ownership records found for sale. ProductId: {ProductId}, BranchId: {BranchId}", 
                    productId, branchId);
                return false;
            }

            var remainingQuantity = soldQuantity;

            foreach (var ownership in activeOwnerships.OrderBy(o => o.CreatedAt))
            {
                if (remainingQuantity <= 0) break;

                var quantityToDeduct = Math.Min(remainingQuantity, ownership.OwnedQuantity);
                var weightToDeduct = (quantityToDeduct / ownership.OwnedQuantity) * ownership.OwnedWeight;

                // Update ownership record
                ownership.OwnedQuantity -= quantityToDeduct;
                ownership.OwnedWeight -= weightToDeduct;
                ownership.OwnershipPercentage = ownership.TotalQuantity > 0 
                    ? (ownership.OwnedQuantity / ownership.TotalQuantity) * 100
                    : 0;
                ownership.ModifiedAt = DateTime.UtcNow;
                ownership.ModifiedBy = userId;

                remainingQuantity -= quantityToDeduct;
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully updated ownership after sale. ProductId: {ProductId}, BranchId: {BranchId}", 
                productId, branchId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ownership after sale. ProductId: {ProductId}, BranchId: {BranchId}", 
                productId, branchId);
            throw;
        }
    }

    ///// <summary>
    ///// Convert raw gold to products (Scenario 2)
    ///// </summary>
    //public async Task<(bool Success, string Message)> ConvertRawGoldToProductsAsync(ConvertRawGoldRequest request, string userId)
    //{
    //    try
    //    {
    //        _logger.LogInformation("Converting raw gold to products. RawGoldProductId: {RawGoldProductId}, WeightToConvert: {WeightToConvert}", 
    //            request.RawGoldProductId, request.WeightToConvert);

    //        // Validate raw gold product exists and has sufficient ownership
    //        var rawGoldOwnerships = await _productOwnershipRepository.GetByProductAndBranchAsync(request.RawGoldProductId, request.BranchId);
    //        var totalOwnedWeight = rawGoldOwnerships.Where(o => o.IsActive).Sum(o => o.OwnedWeight);

    //        if (totalOwnedWeight < request.WeightToConvert)
    //        {
    //            var message = $"Insufficient raw gold weight. Available: {totalOwnedWeight}g, Required: {request.WeightToConvert}g";
    //            _logger.LogWarning(message);
    //            return (false, message);
    //        }

    //        // Validate new products exist
    //        foreach (var newProduct in request.NewProducts)
    //        {
    //            var product = await _productRepository.GetByIdAsync(newProduct.ProductId);
    //            if (product == null)
    //            {
    //                var message = $"Product with ID {newProduct.ProductId} not found";
    //                _logger.LogWarning(message);
    //                return (false, message);
    //            }
    //        }

    //        // Calculate total weight of new products
    //        var totalNewProductWeight = request.NewProducts.Sum(p => p.Weight);
    //        if (Math.Abs(totalNewProductWeight - request.WeightToConvert) > 0.001m) // Allow small tolerance
    //        {
    //            var message = $"Weight mismatch. Raw gold weight: {request.WeightToConvert}g, New products total weight: {totalNewProductWeight}g";
    //            _logger.LogWarning(message);
    //            return (false, message);
    //        }

    //        // Deduct from raw gold ownership
    //        var remainingWeight = request.WeightToConvert;
    //        var rawGoldOwnershipsList = rawGoldOwnerships.Where(o => o.IsActive && o.OwnedWeight > 0).OrderBy(o => o.CreatedAt).ToList();

    //        foreach (var ownership in rawGoldOwnershipsList)
    //        {
    //            if (remainingWeight <= 0) break;

    //            var weightToDeduct = Math.Min(remainingWeight, ownership.OwnedWeight);
    //            var quantityToDeduct = (weightToDeduct / ownership.OwnedWeight) * ownership.OwnedQuantity;

    //            // Update raw gold ownership
    //            ownership.OwnedQuantity -= quantityToDeduct;
    //            ownership.OwnedWeight -= weightToDeduct;
    //            ownership.OwnershipPercentage = ownership.TotalQuantity > 0 
    //                ? (ownership.OwnedQuantity / ownership.TotalQuantity) * 100
    //                : 0;
    //            ownership.ModifiedAt = DateTime.UtcNow;
    //            ownership.ModifiedBy = userId;

    //            remainingWeight -= weightToDeduct;
    //        }

    //        // Ensure there are new products to create
    //        if (request.NewProducts == null || request.NewProducts.Count == 0)
    //        {
    //            var message = "No new products specified for conversion.";
    //            _logger.LogWarning(message);
    //            return (false, message);
    //        }

    //        // Create ownership records for new products
    //        foreach (var newProduct in request.NewProducts)
    //        {
    //            var newOwnership = new ProductOwnership
    //            {
    //                ProductId = newProduct.ProductId,
    //                BranchId = request.BranchId,
    //                TotalQuantity = newProduct.Quantity,
    //                TotalWeight = newProduct.Weight,
    //                OwnedQuantity = newProduct.Quantity,
    //                OwnedWeight = newProduct.Weight,
    //                OwnershipPercentage = 100, // Fully owned since converted from owned raw gold (stored as 0-100)
    //                TotalCost = 0, // No additional cost for conversion
    //                AmountPaid = 0,
    //                OutstandingAmount = 0,
    //                IsActive = true,
    //                CreatedBy = userId,
    //                ModifiedBy = userId,
    //                CreatedAt = DateTime.UtcNow,
    //                ModifiedAt = DateTime.UtcNow
    //            };

    //            await _productOwnershipRepository.AddAsync(newOwnership);
    //        }

    //        await _unitOfWork.SaveChangesAsync();

    //        var success = $"Successfully converted {request.WeightToConvert}g of raw gold to {request.NewProducts.Count} products";
    //        _logger.LogInformation(success);
    //        return (true, success);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error converting raw gold to products. RawGoldProductId: {RawGoldProductId}", 
    //            request.RawGoldProductId);
    //        throw;
    //    }
    //}

    /// <summary>
    /// Get ownership alerts
    /// </summary>
    public async Task<List<OwnershipAlertDto>> GetOwnershipAlertsAsync(int? branchId = null)
    {
        try
        {
            _logger.LogInformation("Getting ownership alerts. BranchId: {BranchId}", branchId);

            var alerts = new List<OwnershipAlertDto>();

            // Get low ownership products
            var lowOwnershipProducts = await _productOwnershipRepository.GetLowOwnershipProductsAsync(50m);
            foreach (var ownership in lowOwnershipProducts)
            {
                if (branchId.HasValue && ownership.BranchId != branchId.Value) continue;

                alerts.Add(new OwnershipAlertDto
                {
                    Type = "LowOwnership",
                    Message = $"Low ownership percentage: {ownership.OwnershipPercentage:F2}%",
                    Severity = ownership.OwnershipPercentage < 25 ? "High" : "Medium",
                    ProductId = ownership.ProductId,
                    ProductName = ownership.Product.Name,
                    SupplierId = ownership.SupplierId,
                    SupplierName = ownership.Supplier?.CompanyName,
                    OwnershipPercentage = ownership.OwnershipPercentage,
                    OutstandingAmount = ownership.OutstandingAmount,
                    CreatedAt = ownership.CreatedAt
                });
            }

            // Get products with outstanding payments
            var outstandingProducts = await _productOwnershipRepository.GetProductsWithOutstandingPaymentsAsync();
            foreach (var ownership in outstandingProducts)
            {
                if (branchId.HasValue && ownership.BranchId != branchId.Value) continue;

                alerts.Add(new OwnershipAlertDto
                {
                    Type = "OutstandingPayment",
                    Message = $"Outstanding payment: ${ownership.OutstandingAmount:F2}",
                    Severity = ownership.OutstandingAmount > 10000 ? "High" : "Medium",
                    ProductId = ownership.ProductId,
                    ProductName = ownership.Product.Name,
                    SupplierId = ownership.SupplierId,
                    SupplierName = ownership.Supplier?.CompanyName,
                    OwnershipPercentage = ownership.OwnershipPercentage,
                    OutstandingAmount = ownership.OutstandingAmount,
                    CreatedAt = ownership.CreatedAt
                });
            }

            // Get supplier credit alerts
            await AddSupplierCreditAlertsAsync(alerts, branchId);

            _logger.LogInformation("Retrieved {AlertCount} total alerts (ownership + supplier credit)", alerts.Count);
            return alerts.OrderByDescending(a => a.CreatedAt).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ownership alerts");
            throw;
        }
    }

    /// <summary>
    /// Add supplier credit alerts to the ownership alerts list
    /// </summary>
    private async Task AddSupplierCreditAlertsAsync(List<OwnershipAlertDto> alerts, int? branchId = null)
    {
        try
        {
            // Get suppliers near credit limit (80% threshold)
            var nearLimitSuppliers = await _supplierRepository.GetSuppliersNearCreditLimitAsync(0.8m);
            foreach (var supplier in nearLimitSuppliers)
            {
                if (!supplier.IsActive) continue;

                var utilizationPercentage = supplier.CreditLimit > 0
                    ? (supplier.CurrentBalance / supplier.CreditLimit) * 100
                    : 0;

                var availableCredit = supplier.CreditLimit - supplier.CurrentBalance;

                alerts.Add(new OwnershipAlertDto
                {
                    Type = "SupplierNearCreditLimit",
                    Message = $"Supplier near credit limit: {utilizationPercentage:F1}% utilized, Available: {availableCredit:C}",
                    Severity = utilizationPercentage >= 95 ? "High" : "Medium",
                    ProductId = 0, // Not product-specific
                    ProductName = string.Empty,
                    SupplierId = supplier.Id,
                    SupplierName = supplier.CompanyName,
                    OwnershipPercentage = 0.0m,
                    OutstandingAmount = supplier.CurrentBalance,
                    CreatedAt = supplier.LastTransactionDate ?? supplier.CreatedAt
                });
            }

            // Get suppliers over credit limit
            var overLimitSuppliers = await _supplierRepository.GetSuppliersOverCreditLimitAsync();
            foreach (var supplier in overLimitSuppliers)
            {
                if (!supplier.IsActive) continue;

                var overLimitAmount = supplier.CurrentBalance - supplier.CreditLimit;

                alerts.Add(new OwnershipAlertDto
                {
                    Type = "SupplierOverCreditLimit",
                    Message = $"Supplier over credit limit by {overLimitAmount:C}. Current balance: {supplier.CurrentBalance:C}",
                    Severity = "Critical",
                    ProductId = 0, // Not product-specific
                    ProductName = string.Empty,
                    SupplierId = supplier.Id,
                    SupplierName = supplier.CompanyName,
                    OwnershipPercentage = 0.0m,
                    OutstandingAmount = supplier.CurrentBalance,
                    CreatedAt = supplier.LastTransactionDate ?? supplier.CreatedAt
                });
            }

            _logger.LogInformation("Added supplier credit alerts. Near limit: {NearLimitCount}, Over limit: {OverLimitCount}",
                nearLimitSuppliers.Count, overLimitSuppliers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding supplier credit alerts");
            // Don't throw here as it shouldn't break the main ownership alerts
        }
    }

    /// <summary>
    /// Get ownership details for a product
    /// </summary>
    public async Task<List<ProductOwnershipDto>> GetProductOwnershipAsync(int productId, int branchId)
    {
        try
        {
            _logger.LogInformation("Getting product ownership details. ProductId: {ProductId}, BranchId: {BranchId}", 
                productId, branchId);

            var ownerships = await _productOwnershipRepository.GetByProductAndBranchAsync(productId, branchId);
            var activeOwnerships = ownerships.Where(o => o.IsActive).ToList();

            var result = _mapper.Map<List<ProductOwnershipDto>>(activeOwnerships);
            
            _logger.LogInformation("Retrieved {Count} ownership records for ProductId: {ProductId}", 
                result.Count, productId);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product ownership details. ProductId: {ProductId}", productId);
            throw;
        }
    }

    /// <summary>
    /// Get ownership movements
    /// </summary>
    public async Task<List<OwnershipMovementDto>> GetOwnershipMovementsAsync(int productOwnershipId)
    {
        try
        {
            _logger.LogInformation("Getting ownership movements. ProductOwnershipId: {ProductOwnershipId}", 
                productOwnershipId);

            var ownership = await _productOwnershipRepository.GetWithDetailsAsync(productOwnershipId);
            if (ownership == null)
            {
                _logger.LogWarning("Product ownership not found. ID: {ProductOwnershipId}", productOwnershipId);
                return new List<OwnershipMovementDto>();
            }

            var movements = ownership.OwnershipMovements.OrderByDescending(m => m.MovementDate).ToList();
            var result = _mapper.Map<List<OwnershipMovementDto>>(movements);

            _logger.LogInformation("Retrieved {Count} movement records for ProductOwnershipId: {ProductOwnershipId}", 
                result.Count, productOwnershipId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ownership movements. ProductOwnershipId: {ProductOwnershipId}", 
                productOwnershipId);
            throw;
        }
    }

    /// <summary>
    /// Get low ownership products
    /// </summary>
    public async Task<List<ProductOwnershipDto>> GetLowOwnershipProductsAsync(decimal threshold = 0.5m)
    {
        try
        {
            _logger.LogInformation("Getting low ownership products. Threshold: {Threshold}%", threshold);

            var lowOwnershipProducts = await _productOwnershipRepository.GetLowOwnershipProductsAsync(threshold);
            var result = _mapper.Map<List<ProductOwnershipDto>>(lowOwnershipProducts);

            _logger.LogInformation("Retrieved {Count} low ownership products", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low ownership products");
            throw;
        }
    }

    /// <summary>
    /// Get products with outstanding payments
    /// </summary>
    public async Task<List<ProductOwnershipDto>> GetProductsWithOutstandingPaymentsAsync()
    {
        try
        {
            _logger.LogInformation("Getting products with outstanding payments");

            var outstandingProducts = await _productOwnershipRepository.GetProductsWithOutstandingPaymentsAsync();
            var result = _mapper.Map<List<ProductOwnershipDto>>(outstandingProducts);

            _logger.LogInformation("Retrieved {Count} products with outstanding payments", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products with outstanding payments");
            throw;
        }
    }

    /// <summary>
    /// Get product ownership list with pagination and filtering
    /// </summary>
    public async Task<(List<ProductOwnershipDto> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetProductOwnershipListAsync(
        int branchId,
        string? searchTerm = null,
        int? supplierId = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting product ownership list. BranchId: {BranchId}, Page: {PageNumber}, Size: {PageSize}", 
                branchId, pageNumber, pageSize);

            var (items, totalCount) = await _productOwnershipRepository.GetWithPaginationAsync(
                branchId, searchTerm, supplierId, pageNumber, pageSize);

            var mappedItems = _mapper.Map<List<ProductOwnershipDto>>(items);
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            _logger.LogInformation("Retrieved {Count} ownership records out of {TotalCount} for BranchId: {BranchId}", 
                mappedItems.Count, totalCount, branchId);

            return (mappedItems, totalCount, pageNumber, pageSize, totalPages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product ownership list. BranchId: {BranchId}", branchId);
            throw;
        }
    }

    /// <summary>
    /// Validate product sale and check for unpaid supplier balances
    /// </summary>
    public async Task<(bool CanSell, string Message, List<string> Warnings)> ValidateProductSaleAsync(int productId, int branchId, decimal requestedQuantity)
    {
        try
        {
            var warnings = new List<string>();
            
            // Get ownership records for this product and branch
            var ownerships = await _productOwnershipRepository.GetByProductAndBranchAsync(productId, branchId);
            
            foreach (var ownership in ownerships.Where(o => o.IsActive && o.OutstandingAmount > 0))
            {
                var supplier = await _supplierRepository.GetByIdAsync(ownership.SupplierId ?? 0);
                var supplierName = supplier?.CompanyName ?? "Unknown Supplier";
                
                if (ownership.AmountPaid == 0)
                {
                    warnings.Add($"WARNING: Product from {supplierName} is completely UNPAID (Outstanding: ${ownership.OutstandingAmount:F2})");
                }
                else if (ownership.OutstandingAmount > 0)
                {
                    warnings.Add($"WARNING: Product from {supplierName} is PARTIALLY PAID (Outstanding: ${ownership.OutstandingAmount:F2}, Paid: ${ownership.AmountPaid:F2})");
                }
            }

            return (true, warnings.Any() ? "Sale allowed with payment warnings" : "Sale validated successfully", warnings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating product sale for product {ProductId}", productId);
            return (false, "Error validating sale", new List<string> { "System error occurred during validation" });
        }
    }

    /// <summary>
    /// Get products with unpaid supplier balances (sales risk report)
    /// </summary>
    public async Task<List<ProductSaleRiskDto>> GetProductsWithUnpaidSupplierBalancesAsync(int? branchId = null)
    {
        try
        {
            var query = _productOwnershipRepository.GetQueryable()
                .Where(po => po.IsActive && po.OutstandingAmount > 0);

            if (branchId.HasValue)
            {
                query = query.Where(po => po.BranchId == branchId.Value);
            }

            var ownerships = await query
                .Include(po => po.Product)
                .Include(po => po.Branch)
                .Include(po => po.Supplier)
                .ToListAsync();

            var riskProducts = ownerships
                .GroupBy(po => new { po.ProductId, po.BranchId })
                .Select(g => new ProductSaleRiskDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.First().Product?.Name ?? "Unknown Product",
                    ProductCode = g.First().Product?.ProductCode ?? "Unknown Code",
                    BranchId = g.Key.BranchId,
                    BranchName = g.First().Branch?.Name ?? "Unknown Branch",
                    AvailableQuantity = g.Sum(po => po.OwnedQuantity),
                    TotalOutstandingAmount = g.Sum(po => po.OutstandingAmount),
                    UnpaidSuppliers = g.Select(po => new UnpaidSupplierDto
                    {
                        SupplierId = po.SupplierId ?? 0,
                        SupplierName = po.Supplier?.CompanyName ?? "Unknown Supplier",
                        OutstandingAmount = po.OutstandingAmount,
                        AmountPaid = po.AmountPaid,
                        TotalCost = po.TotalCost,
                        PaymentStatus = po.AmountPaid == 0 ? "Unpaid" : 
                                       po.OutstandingAmount == 0 ? "Paid" : "Partial"
                    }).ToList()
                })
                .ToList();

            return riskProducts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products with unpaid supplier balances");
            return new List<ProductSaleRiskDto>();
        }
    }
}

using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Transaction processing service implementation
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TransactionService> _logger;
    private readonly IPricingService _pricingService;
    private readonly IInventoryService _inventoryService;
    private readonly IReceiptService _receiptService;
    private readonly IAuditService _auditService;

    public TransactionService(
        ApplicationDbContext context,
        ILogger<TransactionService> logger,
        IPricingService pricingService,
        IInventoryService inventoryService,
        IReceiptService receiptService,
        IAuditService auditService)
    {
        _context = context;
        _logger = logger;
        _pricingService = pricingService;
        _inventoryService = inventoryService;
        _receiptService = receiptService;
        _auditService = auditService;
    }

    /// <summary>
    /// Process a sale transaction
    /// </summary>
    public async Task<TransactionResult> ProcessSaleAsync(SaleTransactionRequest saleRequest, string userId)
    {
        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Validate request
            var validationResult = await ValidateSaleRequestAsync(saleRequest);
            if (!validationResult.IsValid)
            {
                return new TransactionResult
                {
                    IsSuccess = false,
                    ErrorMessage = validationResult.ErrorMessage
                };
            }

            // Generate transaction number
            var transactionNumber = await GenerateTransactionNumberAsync(saleRequest.BranchId, TransactionType.Sale);

            // Get user and branch info
            var user = await _context.Users.FindAsync(userId);
            var branch = await _context.Branches.FindAsync(saleRequest.BranchId);
            if (user == null || branch == null)
            {
                return new TransactionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid user or branch"
                };
            }

            // Create transaction
            var transaction = new Transaction
            {
                TransactionNumber = transactionNumber,
                TransactionType = TransactionType.Sale,
                TransactionDate = DateTime.UtcNow,
                BranchId = saleRequest.BranchId,
                CustomerId = saleRequest.CustomerId,
                CashierId = userId,
                PaymentMethod = saleRequest.PaymentMethod,
                AmountPaid = saleRequest.AmountPaid,
                Status = TransactionStatus.Pending,
                CreatedByUserId = userId
            };

            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync(); // Save to get transaction ID

            // Process transaction items
            var transactionItems = new List<TransactionItem>();
            decimal subtotal = 0;
            decimal totalMakingCharges = 0;
            decimal totalDiscountAmount = 0;
            var allTaxes = new List<TransactionTax>();

            foreach (var itemRequest in saleRequest.Items)
            {
                var product = await _context.Products.FindAsync(itemRequest.ProductId);
                if (product == null)
                {
                    return new TransactionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Product {itemRequest.ProductId} not found"
                    };
                }

                // Check inventory availability
                var hasStock = await _inventoryService.CheckStockAvailabilityAsync(
                    itemRequest.ProductId, saleRequest.BranchId, itemRequest.Quantity);
                
                if (!hasStock)
                {
                    return new TransactionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Insufficient stock for product {product.Name}"
                    };
                }

                // Calculate pricing
                var priceCalculation = await _pricingService.CalculatePriceAsync(
                    product, itemRequest.Quantity, saleRequest.CustomerId);

                // Apply custom discount if provided
                if (itemRequest.CustomDiscountPercentage.HasValue && itemRequest.CustomDiscountPercentage.Value > 0)
                {
                    var customDiscount = priceCalculation.SubTotal * (itemRequest.CustomDiscountPercentage.Value / 100m);
                    priceCalculation.DiscountAmount = Math.Max(priceCalculation.DiscountAmount, customDiscount);
                    priceCalculation.TaxableAmount = priceCalculation.SubTotal - priceCalculation.DiscountAmount;
                    
                    // Recalculate taxes
                    priceCalculation.Taxes.Clear();
                    priceCalculation.TotalTaxAmount = 0;
                    var taxConfigs = await _pricingService.GetCurrentTaxConfigurationsAsync();
                    foreach (var taxConfig in taxConfigs.Where(t => t.IsMandatory))
                    {
                        var taxAmount = taxConfig.TaxType == ChargeType.Percentage ?
                            priceCalculation.TaxableAmount * (taxConfig.TaxRate / 100m) :
                            taxConfig.TaxRate * itemRequest.Quantity;

                        priceCalculation.Taxes.Add(new TaxCalculation
                        {
                            TaxConfigurationId = taxConfig.Id,
                            TaxName = taxConfig.TaxName,
                            TaxRate = taxConfig.TaxRate,
                            TaxableAmount = priceCalculation.TaxableAmount,
                            TaxAmount = taxAmount
                        });
                        priceCalculation.TotalTaxAmount += taxAmount;
                    }
                    priceCalculation.FinalTotal = priceCalculation.TaxableAmount + priceCalculation.TotalTaxAmount;
                }

                var transactionItem = new TransactionItem
                {
                    TransactionId = transaction.Id,
                    ProductId = itemRequest.ProductId,
                    Quantity = itemRequest.Quantity,
                    UnitWeight = product.Weight,
                    TotalWeight = product.Weight * itemRequest.Quantity,
                    GoldRatePerGram = priceCalculation.GoldRateUsed?.RatePerGram ?? 0,
                    UnitPrice = priceCalculation.GoldValue / itemRequest.Quantity,
                    MakingChargesId = priceCalculation.MakingChargesUsed?.Id,
                    MakingChargesAmount = priceCalculation.MakingChargesAmount,
                    DiscountPercentage = itemRequest.CustomDiscountPercentage ?? 
                        (priceCalculation.CustomerInfo?.DefaultDiscountPercentage ?? 0),
                    DiscountAmount = priceCalculation.DiscountAmount,
                    LineTotal = priceCalculation.TaxableAmount,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                transactionItems.Add(transactionItem);
                await _context.TransactionItems.AddAsync(transactionItem);

                // Accumulate totals
                subtotal += priceCalculation.SubTotal;
                totalMakingCharges += priceCalculation.MakingChargesAmount;
                totalDiscountAmount += priceCalculation.DiscountAmount;

                // Create tax records
                foreach (var tax in priceCalculation.Taxes)
                {
                    var transactionTax = new TransactionTax
                    {
                        TransactionId = transaction.Id,
                        TaxConfigurationId = tax.TaxConfigurationId,
                        TaxRate = tax.TaxRate,
                        TaxableAmount = tax.TaxableAmount,
                        TaxAmount = tax.TaxAmount,
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow
                    };

                    allTaxes.Add(transactionTax);
                    await _context.TransactionTaxes.AddAsync(transactionTax);
                }
            }

            // Update transaction totals
            transaction.Subtotal = subtotal;
            transaction.TotalMakingCharges = totalMakingCharges;
            transaction.DiscountAmount = totalDiscountAmount;
            transaction.TotalTaxAmount = allTaxes.Sum(t => t.TaxAmount);
            transaction.TotalAmount = subtotal + totalMakingCharges + transaction.TotalTaxAmount - totalDiscountAmount;

            // Validate payment
            if (transaction.AmountPaid < transaction.TotalAmount)
            {
                return new TransactionResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Insufficient payment. Required: {transaction.TotalAmount:C}, Paid: {transaction.AmountPaid:C}"
                };
            }

            transaction.ChangeGiven = transaction.AmountPaid - transaction.TotalAmount;
            transaction.Status = TransactionStatus.Completed;

            // Reserve inventory
            var inventoryReserved = await _inventoryService.ReserveInventoryAsync(transactionItems, saleRequest.BranchId, userId);
            if (!inventoryReserved)
            {
                return new TransactionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Failed to reserve inventory"
                };
            }

            // Update customer loyalty points if applicable
            if (saleRequest.CustomerId.HasValue)
            {
                await UpdateCustomerLoyaltyAsync(saleRequest.CustomerId.Value, transaction.TotalAmount);
            }

            await _context.SaveChangesAsync();

            // Generate receipt
            var receipt = await _receiptService.GenerateReceiptContentAsync(transaction);

            // Mark transaction as completed
            transaction.ReceiptPrinted = true;
            await _context.SaveChangesAsync();

            await dbTransaction.CommitAsync();

            // Log audit trail
            await _auditService.LogAsync(
                userId,
                "SALE_TRANSACTION",
                "Transaction",
                transaction.Id.ToString(),
                $"Sale transaction completed: {transaction.TransactionNumber}",
                newValues: System.Text.Json.JsonSerializer.Serialize(new { 
                    TransactionNumber = transaction.TransactionNumber,
                    TotalAmount = transaction.TotalAmount,
                    ItemCount = transactionItems.Count
                }),
                branchId: saleRequest.BranchId,
                transactionId: transaction.Id
            );

            _logger.LogInformation("Sale transaction {TransactionNumber} completed successfully by user {UserId}", 
                transaction.TransactionNumber, userId);

            return new TransactionResult
            {
                IsSuccess = true,
                Transaction = transaction,
                ReceiptContent = receipt
            };
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            _logger.LogError(ex, "Error processing sale transaction for user {UserId}", userId);
            return new TransactionResult
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred while processing the transaction"
            };
        }
    }

    /// <summary>
    /// Process a return transaction
    /// </summary>
    public async Task<TransactionResult> ProcessReturnAsync(ReturnTransactionRequest returnRequest, string userId, string managerId)
    {
        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Get original transaction
            var originalTransaction = await _context.Transactions
                .Include(t => t.TransactionItems)
                .ThenInclude(ti => ti.Product)
                .Include(t => t.Branch)
                .FirstOrDefaultAsync(t => t.Id == returnRequest.OriginalTransactionId);

            if (originalTransaction == null)
            {
                return new TransactionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Original transaction not found"
                };
            }

            // Validate return eligibility
            var validationResult = await ValidateReturnRequestAsync(returnRequest, originalTransaction);
            if (!validationResult.IsValid)
            {
                return new TransactionResult
                {
                    IsSuccess = false,
                    ErrorMessage = validationResult.ErrorMessage
                };
            }

            // Generate return transaction number
            var transactionNumber = await GenerateTransactionNumberAsync(originalTransaction.BranchId, TransactionType.Return);

            // Create return transaction
            var returnTransaction = new Transaction
            {
                TransactionNumber = transactionNumber,
                TransactionType = TransactionType.Return,
                TransactionDate = DateTime.UtcNow,
                BranchId = originalTransaction.BranchId,
                CustomerId = originalTransaction.CustomerId,
                CashierId = userId,
                ApprovedBy = managerId,
                OriginalTransactionId = originalTransaction.Id,
                ReturnReason = returnRequest.ReturnReason,
                PaymentMethod = PaymentMethod.Cash,
                TotalAmount = returnRequest.ReturnAmount,
                AmountPaid = 0, // Return gives money back
                ChangeGiven = -returnRequest.ReturnAmount, // Negative change = refund
                Status = TransactionStatus.Completed,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Transactions.AddAsync(returnTransaction);
            await _context.SaveChangesAsync();

            // Process return items
            var returnItems = new List<TransactionItem>();
            foreach (var returnItemRequest in returnRequest.Items)
            {
                var originalItem = originalTransaction.TransactionItems
                    .FirstOrDefault(ti => ti.Id == returnItemRequest.OriginalTransactionItemId);

                if (originalItem == null)
                {
                    return new TransactionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Original transaction item {returnItemRequest.OriginalTransactionItemId} not found"
                    };
                }

                if (returnItemRequest.ReturnQuantity > originalItem.Quantity)
                {
                    return new TransactionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Cannot return more than originally purchased for {originalItem.Product?.Name}"
                    };
                }

                var returnItem = new TransactionItem
                {
                    TransactionId = returnTransaction.Id,
                    ProductId = originalItem.ProductId,
                    Quantity = returnItemRequest.ReturnQuantity,
                    UnitWeight = originalItem.UnitWeight,
                    TotalWeight = originalItem.UnitWeight * returnItemRequest.ReturnQuantity,
                    GoldRatePerGram = originalItem.GoldRatePerGram,
                    UnitPrice = originalItem.UnitPrice,
                    MakingChargesId = originalItem.MakingChargesId,
                    MakingChargesAmount = originalItem.MakingChargesAmount * (returnItemRequest.ReturnQuantity / originalItem.Quantity),
                    DiscountPercentage = originalItem.DiscountPercentage,
                    DiscountAmount = originalItem.DiscountAmount * (returnItemRequest.ReturnQuantity / originalItem.Quantity),
                    LineTotal = originalItem.LineTotal * (returnItemRequest.ReturnQuantity / originalItem.Quantity),
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                returnItems.Add(returnItem);
                await _context.TransactionItems.AddAsync(returnItem);
            }

            // Release inventory back to stock
            var inventoryReleased = await _inventoryService.ReleaseInventoryAsync(returnItems, originalTransaction.BranchId, userId);
            if (!inventoryReleased)
            {
                return new TransactionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Failed to return inventory to stock"
                };
            }

            await _context.SaveChangesAsync();

            // Generate return receipt
            var receipt = await _receiptService.GenerateReturnReceiptContentAsync(returnTransaction);
            returnTransaction.ReceiptPrinted = true;
            await _context.SaveChangesAsync();

            await dbTransaction.CommitAsync();

            // Log audit trail
            await _auditService.LogAsync(
                userId,
                "RETURN_TRANSACTION",
                "Transaction",
                returnTransaction.Id.ToString(),
                $"Return transaction completed: {returnTransaction.TransactionNumber}",
                newValues: System.Text.Json.JsonSerializer.Serialize(new { 
                    TransactionNumber = returnTransaction.TransactionNumber,
                    ReturnAmount = returnRequest.ReturnAmount,
                    OriginalTransaction = originalTransaction.TransactionNumber,
                    Reason = returnRequest.ReturnReason
                }),
                branchId: originalTransaction.BranchId,
                transactionId: returnTransaction.Id
            );

            _logger.LogInformation("Return transaction {TransactionNumber} completed by user {UserId}, approved by {ManagerId}", 
                returnTransaction.TransactionNumber, userId, managerId);

            return new TransactionResult
            {
                IsSuccess = true,
                Transaction = returnTransaction,
                ReceiptContent = receipt
            };
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            _logger.LogError(ex, "Error processing return transaction for user {UserId}", userId);
            return new TransactionResult
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred while processing the return"
            };
        }
    }

    /// <summary>
    /// Process a repair transaction
    /// </summary>
    public async Task<TransactionResult> ProcessRepairAsync(RepairTransactionRequest repairRequest, string userId)
    {
        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Generate transaction number
            var transactionNumber = await GenerateTransactionNumberAsync(repairRequest.BranchId, TransactionType.Repair);

            // Create repair transaction
            var transaction = new Transaction
            {
                TransactionNumber = transactionNumber,
                TransactionType = TransactionType.Repair,
                TransactionDate = DateTime.UtcNow,
                BranchId = repairRequest.BranchId,
                CustomerId = repairRequest.CustomerId,
                CashierId = userId,
                RepairDescription = repairRequest.RepairDescription,
                EstimatedCompletionDate = repairRequest.EstimatedCompletionDate,
                PaymentMethod = repairRequest.PaymentMethod,
                TotalAmount = repairRequest.RepairAmount,
                AmountPaid = repairRequest.AmountPaid,
                ChangeGiven = repairRequest.AmountPaid - repairRequest.RepairAmount,
                Status = TransactionStatus.Completed,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            // Generate repair receipt
            var receipt = await _receiptService.GenerateRepairReceiptContentAsync(transaction);
            transaction.ReceiptPrinted = true;
            await _context.SaveChangesAsync();

            await dbTransaction.CommitAsync();

            // Log audit trail
            await _auditService.LogAsync(
                userId,
                "REPAIR_TRANSACTION",
                "Transaction",
                transaction.Id.ToString(),
                $"Repair transaction completed: {transaction.TransactionNumber}",
                newValues: System.Text.Json.JsonSerializer.Serialize(new { 
                    TransactionNumber = transaction.TransactionNumber,
                    RepairAmount = repairRequest.RepairAmount,
                    Description = repairRequest.RepairDescription,
                    EstimatedCompletion = repairRequest.EstimatedCompletionDate
                }),
                branchId: repairRequest.BranchId,
                transactionId: transaction.Id
            );

            _logger.LogInformation("Repair transaction {TransactionNumber} completed by user {UserId}", 
                transaction.TransactionNumber, userId);

            return new TransactionResult
            {
                IsSuccess = true,
                Transaction = transaction,
                ReceiptContent = receipt
            };
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            _logger.LogError(ex, "Error processing repair transaction for user {UserId}", userId);
            return new TransactionResult
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred while processing the repair transaction"
            };
        }
    }

    /// <summary>
    /// Get transaction by ID
    /// </summary>
    public async Task<Transaction?> GetTransactionAsync(int transactionId)
    {
        return await _context.Transactions
            .Include(t => t.Branch)
            .Include(t => t.Customer)
            .Include(t => t.Cashier)
            .Include(t => t.ApprovedByUser)
            .Include(t => t.TransactionItems)
            .ThenInclude(ti => ti.Product)
            .Include(t => t.TransactionTaxes)
            .ThenInclude(tt => tt.TaxConfiguration)
            .FirstOrDefaultAsync(t => t.Id == transactionId);
    }

    /// <summary>
    /// Get transaction by transaction number
    /// </summary>
    public async Task<Transaction?> GetTransactionByNumberAsync(string transactionNumber, int branchId)
    {
        return await _context.Transactions
            .Include(t => t.Branch)
            .Include(t => t.Customer)
            .Include(t => t.Cashier)
            .Include(t => t.ApprovedByUser)
            .Include(t => t.TransactionItems)
            .ThenInclude(ti => ti.Product)
            .Include(t => t.TransactionTaxes)
            .ThenInclude(tt => tt.TaxConfiguration)
            .FirstOrDefaultAsync(t => t.TransactionNumber == transactionNumber && t.BranchId == branchId);
    }

    /// <summary>
    /// Search transactions
    /// </summary>
    public async Task<(List<Transaction> Transactions, int TotalCount)> SearchTransactionsAsync(TransactionSearchRequest searchRequest)
    {
        var query = _context.Transactions
            .Include(t => t.Branch)
            .Include(t => t.Customer)
            .Include(t => t.Cashier)
            .AsQueryable();

        // Apply filters
        if (searchRequest.BranchId.HasValue)
            query = query.Where(t => t.BranchId == searchRequest.BranchId.Value);

        if (!string.IsNullOrEmpty(searchRequest.TransactionNumber))
            query = query.Where(t => t.TransactionNumber.Contains(searchRequest.TransactionNumber));

        if (searchRequest.TransactionType.HasValue)
            query = query.Where(t => t.TransactionType == searchRequest.TransactionType.Value);

        if (searchRequest.Status.HasValue)
            query = query.Where(t => t.Status == searchRequest.Status.Value);

        if (searchRequest.CustomerId.HasValue)
            query = query.Where(t => t.CustomerId == searchRequest.CustomerId.Value);

        if (!string.IsNullOrEmpty(searchRequest.CashierId))
            query = query.Where(t => t.CashierId == searchRequest.CashierId);

        if (searchRequest.FromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= searchRequest.FromDate.Value);

        if (searchRequest.ToDate.HasValue)
            query = query.Where(t => t.TransactionDate <= searchRequest.ToDate.Value);

        if (searchRequest.MinAmount.HasValue)
            query = query.Where(t => t.TotalAmount >= searchRequest.MinAmount.Value);

        if (searchRequest.MaxAmount.HasValue)
            query = query.Where(t => t.TotalAmount <= searchRequest.MaxAmount.Value);

        var totalCount = await query.CountAsync();

        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((searchRequest.PageNumber - 1) * searchRequest.PageSize)
            .Take(searchRequest.PageSize)
            .ToListAsync();

        return (transactions, totalCount);
    }

    /// <summary>
    /// Cancel/void a transaction
    /// </summary>
    public async Task<bool> CancelTransactionAsync(int transactionId, string reason, string userId, string managerId)
    {
        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var transaction = await GetTransactionAsync(transactionId);
            if (transaction == null)
            {
                _logger.LogWarning("Transaction {TransactionId} not found for cancellation", transactionId);
                return false;
            }

            if (transaction.Status == TransactionStatus.Cancelled)
            {
                _logger.LogWarning("Transaction {TransactionId} already cancelled", transactionId);
                return false;
            }

            // Release inventory if it was a sale
            if (transaction.TransactionType == TransactionType.Sale && transaction.TransactionItems.Any())
            {
                var inventoryReleased = await _inventoryService.ReleaseInventoryAsync(
                    transaction.TransactionItems.ToList(), transaction.BranchId, userId);
                
                if (!inventoryReleased)
                {
                    _logger.LogError("Failed to release inventory for cancelled transaction {TransactionId}", transactionId);
                    return false;
                }
            }

            // Update transaction status
            transaction.Status = TransactionStatus.Cancelled;
            transaction.ReturnReason = reason;
            transaction.ApprovedBy = managerId;
            transaction.ModifiedBy = userId;
            transaction.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            // Log audit trail
            await _auditService.LogAsync(
                userId,
                "CANCEL_TRANSACTION",
                "Transaction",
                transaction.Id.ToString(),
                $"Transaction cancelled: {reason}",
                oldValues: "Status: Completed",
                newValues: "Status: Cancelled",
                branchId: transaction.BranchId,
                transactionId: transaction.Id
            );

            _logger.LogInformation("Transaction {TransactionNumber} cancelled by user {UserId}, approved by {ManagerId}", 
                transaction.TransactionNumber, userId, managerId);

            return true;
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            _logger.LogError(ex, "Error cancelling transaction {TransactionId}", transactionId);
            return false;
        }
    }

    /// <summary>
    /// Generate next transaction number for branch
    /// </summary>
    public async Task<string> GenerateTransactionNumberAsync(int branchId, TransactionType transactionType)
    {
        var branch = await _context.Branches.FindAsync(branchId);
        var branchCode = branch?.Code ?? "UNK";
        var typePrefix = transactionType switch
        {
            TransactionType.Sale => "S",
            TransactionType.Return => "R",
            TransactionType.Repair => "RP",
            _ => "T"
        };

        var today = DateTime.UtcNow.Date;
        var datePrefix = today.ToString("yyyyMMdd");

        // Get last transaction number for today
        var lastTransaction = await _context.Transactions
            .Where(t => t.BranchId == branchId && 
                       t.TransactionType == transactionType && 
                       t.TransactionDate.Date == today)
            .OrderByDescending(t => t.TransactionNumber)
            .FirstOrDefaultAsync();

        int sequenceNumber = 1;
        if (lastTransaction != null)
        {
            // Extract sequence number from last transaction
            var parts = lastTransaction.TransactionNumber.Split('-');
            if (parts.Length > 3 && int.TryParse(parts[3], out int lastSequence))
            {
                sequenceNumber = lastSequence + 1;
            }
        }

        return $"{branchCode}-{typePrefix}-{datePrefix}-{sequenceNumber:D4}";
    }

    #region Private Helper Methods

    private async Task<(bool IsValid, string? ErrorMessage)> ValidateSaleRequestAsync(SaleTransactionRequest request)
    {
        if (!request.Items.Any())
            return (false, "No items in transaction");

        if (request.AmountPaid <= 0)
            return (false, "Invalid payment amount");

        // Validate all products exist
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var existingProducts = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .CountAsync();

        if (existingProducts != productIds.Count)
            return (false, "One or more products not found");

        return (true, null);
    }

    private Task<(bool IsValid, string? ErrorMessage)> ValidateReturnRequestAsync(
        ReturnTransactionRequest request, Transaction originalTransaction)
    {
        if (originalTransaction.TransactionType != TransactionType.Sale)
            return Task.FromResult<(bool IsValid, string? ErrorMessage)>((false, "Can only return from sale transactions"));

        if (originalTransaction.Status != TransactionStatus.Completed)
            return Task.FromResult<(bool IsValid, string? ErrorMessage)>((false, "Original transaction is not completed"));

        // Check return policy timeframe (configurable)
        var maxReturnDays = 7; // Should come from configuration
        if ((DateTime.UtcNow - originalTransaction.TransactionDate).TotalDays > maxReturnDays)
            return Task.FromResult<(bool IsValid, string? ErrorMessage)>((false, $"Return period of {maxReturnDays} days has expired"));

        return Task.FromResult<(bool IsValid, string? ErrorMessage)>((true, (string?)null));
    }

    private async Task UpdateCustomerLoyaltyAsync(int customerId, decimal transactionAmount)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer != null)
        {
            customer.TotalPurchaseAmount += transactionAmount;
            customer.LoyaltyPoints += (int)(transactionAmount / 100); // 1 point per 100 EGP
            
            // Update loyalty tier based on total purchases
            customer.LoyaltyTier = customer.TotalPurchaseAmount switch
            {
                >= 100000 => 5, // Platinum
                >= 50000 => 4,  // Gold
                >= 25000 => 3,  // Silver
                >= 10000 => 2,  // Bronze
                _ => 1          // Basic
            };

            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Void and Reverse Operations

    /// <summary>
    /// Void a pending transaction
    /// </summary>
    public async Task<TransactionResult> VoidTransactionAsync(int transactionId, string reason, string userId)
    {
        try
        {
            var transaction = await _context.Transactions
                .Include(t => t.TransactionItems)
                .ThenInclude(ti => ti.Product)
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction == null)
            {
                return new TransactionResult
                {
                    Success = false,
                    Message = "Transaction not found"
                };
            }

            // Can only void pending transactions
            if (transaction.Status != TransactionStatus.Pending)
            {
                return new TransactionResult
                {
                    Success = false,
                    Message = "Only pending transactions can be voided"
                };
            }

            // Update transaction status
            transaction.Status = TransactionStatus.Voided;
            transaction.Notes = string.IsNullOrEmpty(transaction.Notes) 
                ? $"VOIDED: {reason}" 
                : $"{transaction.Notes} | VOIDED: {reason}";

            // Release inventory for each item
            foreach (var item in transaction.TransactionItems)
            {
                await _inventoryService.AddInventoryAsync(
                    item.ProductId,
                    transaction.BranchId,
                    item.Quantity,
                    item.Weight,
                    $"Voided transaction {transaction.TransactionNumber}",
                    transaction.TransactionNumber,
                    null, // unitCost - not applicable for void
                    userId
                );
            }

            await _context.SaveChangesAsync();

            // Log audit
            await _auditService.LogAsync(
                userId,
                "VOID",
                "Transaction",
                transaction.Id.ToString(),
                $"Voided transaction {transaction.TransactionNumber}: {reason}",
                branchId: transaction.BranchId
            );

            return new TransactionResult
            {
                Success = true,
                Message = "Transaction voided successfully",
                TransactionId = transaction.Id,
                TransactionNumber = transaction.TransactionNumber
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding transaction {TransactionId}", transactionId);
            return new TransactionResult
            {
                Success = false,
                Message = "An error occurred while voiding the transaction"
            };
        }
    }

    /// <summary>
    /// Create a reverse transaction
    /// </summary>
    public async Task<TransactionResult> CreateReverseTransactionAsync(int originalTransactionId, string reason, string userId, string managerId)
    {
        try
        {
            var originalTransaction = await _context.Transactions
                .Include(t => t.TransactionItems)
                .ThenInclude(ti => ti.Product)
                .Include(t => t.Customer)
                .FirstOrDefaultAsync(t => t.Id == originalTransactionId);

            if (originalTransaction == null)
            {
                return new TransactionResult
                {
                    Success = false,
                    Message = "Original transaction not found"
                };
            }

            // Can only reverse completed sales
            if (originalTransaction.Status != TransactionStatus.Completed || originalTransaction.TransactionType != TransactionType.Sale)
            {
                return new TransactionResult
                {
                    Success = false,
                    Message = "Only completed sales can be reversed"
                };
            }

            // Create reverse transaction
            var reverseTransaction = new Transaction
            {
                TransactionNumber = await GenerateTransactionNumberAsync(originalTransaction.BranchId, TransactionType.Return),
                TransactionType = TransactionType.Return,
                Status = TransactionStatus.Completed,
                TransactionDate = DateTime.UtcNow,
                BranchId = originalTransaction.BranchId,
                CustomerId = originalTransaction.CustomerId,
                CreatedByUserId = userId,
                ApprovedByUserId = managerId,
                OriginalTransactionId = originalTransactionId,
                SubtotalAmount = -originalTransaction.SubtotalAmount,
                TaxAmount = -originalTransaction.TaxAmount,
                DiscountAmount = -originalTransaction.DiscountAmount,
                TotalAmount = -originalTransaction.TotalAmount,
                PaymentMethod = originalTransaction.PaymentMethod,
                Notes = $"REVERSE of {originalTransaction.TransactionNumber}: {reason}",
                IsActive = true
            };

            _context.Transactions.Add(reverseTransaction);
            await _context.SaveChangesAsync();

            // Create reverse transaction items
            foreach (var originalItem in originalTransaction.TransactionItems)
            {
                var reverseItem = new TransactionItem
                {
                    TransactionId = reverseTransaction.Id,
                    ProductId = originalItem.ProductId,
                    Quantity = originalItem.Quantity,
                    Weight = originalItem.Weight,
                    UnitPrice = originalItem.UnitPrice,
                    MakingChargesAmount = -originalItem.MakingChargesAmount,
                    TotalPrice = -originalItem.TotalPrice,
                    IsActive = true
                };

                _context.TransactionItems.Add(reverseItem);

                // Restore inventory
                await _inventoryService.AddInventoryAsync(
                    originalItem.ProductId,
                    originalTransaction.BranchId,
                    originalItem.Quantity,
                    originalItem.Weight,
                    $"Reverse transaction {reverseTransaction.TransactionNumber}",
                    reverseTransaction.TransactionNumber,
                    null, // unitCost - not applicable for reverse
                    userId
                );
            }

            // Update customer loyalty (reduce points and purchase amount)
            if (originalTransaction.Customer != null)
            {
                var customer = originalTransaction.Customer;
                customer.TotalPurchaseAmount -= originalTransaction.TotalAmount;
                customer.TotalTransactions += 1; // Increment for the return transaction
                
                // Recalculate loyalty tier
                customer.LoyaltyTier = customer.TotalPurchaseAmount switch
                {
                    >= 100000 => 5,
                    >= 50000 => 4,
                    >= 25000 => 3,
                    >= 10000 => 2,
                    _ => 1
                };
            }

            await _context.SaveChangesAsync();

            // Log audit
            await _auditService.LogAsync(
                userId,
                "REVERSE",
                "Transaction",
                reverseTransaction.Id.ToString(),
                $"Created reverse transaction {reverseTransaction.TransactionNumber} for original {originalTransaction.TransactionNumber}: {reason}",
                branchId: originalTransaction.BranchId
            );

            return new TransactionResult
            {
                Success = true,
                Message = "Reverse transaction created successfully",
                TransactionId = reverseTransaction.Id,
                TransactionNumber = reverseTransaction.TransactionNumber
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reverse transaction for {OriginalTransactionId}", originalTransactionId);
            return new TransactionResult
            {
                Success = false,
                Message = "An error occurred while creating the reverse transaction"
            };
        }
    }

    /// <summary>
    /// Check if transaction can be voided
    /// </summary>
    public async Task<(bool CanVoid, string? ErrorMessage)> CanVoidTransactionAsync(int transactionId)
    {
        try
        {
            var transaction = await _context.Transactions.FindAsync(transactionId);
            
            if (transaction == null)
            {
                return (false, "Transaction not found");
            }

            if (transaction.Status != TransactionStatus.Pending)
            {
                return (false, "Only pending transactions can be voided");
            }

            if (!transaction.IsActive)
            {
                return (false, "Transaction is already inactive");
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking void eligibility for transaction {TransactionId}", transactionId);
            return (false, "An error occurred while checking void eligibility");
        }
    }

    /// <summary>
    /// Check if transaction can be reversed
    /// </summary>
    public async Task<(bool CanReverse, string? ErrorMessage)> CanReverseTransactionAsync(int transactionId)
    {
        try
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == transactionId);
            
            if (transaction == null)
            {
                return (false, "Transaction not found");
            }

            if (transaction.Status != TransactionStatus.Completed)
            {
                return (false, "Only completed transactions can be reversed");
            }

            if (transaction.TransactionType != TransactionType.Sale)
            {
                return (false, "Only sales transactions can be reversed");
            }

            if (!transaction.IsActive)
            {
                return (false, "Transaction is already inactive");
            }

            // Check if already reversed
            var existingReverse = await _context.Transactions
                .AnyAsync(t => t.OriginalTransactionId == transactionId && t.TransactionType == TransactionType.Return);
            
            if (existingReverse)
            {
                return (false, "Transaction has already been reversed");
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking reverse eligibility for transaction {TransactionId}", transactionId);
            return (false, "An error occurred while checking reverse eligibility");
        }
    }

    #endregion
}
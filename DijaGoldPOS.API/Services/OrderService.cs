using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Shared;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Models.SalesModels;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service implementation for order operations
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IFinancialTransactionService _financialTransactionService;
    private readonly IRepairJobService _repairJobService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IFinancialTransactionService financialTransactionService,
        IRepairJobService repairJobService,
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _financialTransactionService = financialTransactionService;
        _repairJobService = repairJobService;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request, string userId)
    {
        try
        {
            // Validate and create order
            ValidateOrderRequest(request);
            var orderNumber = await _orderRepository.GetNextOrderNumberAsync(request.BranchId);
            var order = CreateOrderEntity(request, userId, orderNumber);
            AddOrderItems(order, request.Items);

            // Save order and log
            await SaveOrderAsync(order, userId, orderNumber);

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            throw;
        }
    }

    /// <summary>
    /// Validate the order creation request
    /// </summary>
    private static void ValidateOrderRequest(CreateOrderRequest request)
    {
        if (request.Items == null || !request.Items.Any())
        {
            throw new ArgumentException("Order must have at least one item");
        }
    }

    /// <summary>
    /// Create the Order entity from the request
    /// </summary>
    private static Order CreateOrderEntity(CreateOrderRequest request, string userId, string orderNumber)
    {
        return new Order
        {
            OrderNumber = orderNumber,
            OrderTypeId = request.OrderTypeId,
            OrderDate = DateTime.UtcNow,
            BranchId = request.BranchId,
            CustomerId = request.CustomerId,
            GoldRateId = request.GoldRateId,
            StatusId = LookupTableConstants.OrderStatusCompleted,
            CashierId = userId,
            Notes = request.Notes,
            EstimatedCompletionDate = request.EstimatedCompletionDate
        };
    }

    /// <summary>
    /// Add order items to the order
    /// </summary>
    private static void AddOrderItems(Order order, List<CreateOrderItemRequest> items)
    {
        foreach (var itemRequest in items)
        {
            var orderItem = new OrderItem
            {
                ProductId = itemRequest.ProductId,
                Quantity = itemRequest.Quantity,
                UnitPrice = 0, // Will be calculated based on product and gold rate
                TotalPrice = 0, // Will be calculated
                DiscountPercentage = itemRequest.CustomDiscountPercentage ?? 0,
                DiscountAmount = 0, // Will be calculated
                FinalPrice = 0, // Will be calculated
                MakingCharges = 0, // Will be calculated
                TaxAmount = 0, // Will be calculated
                TotalAmount = 0, // Will be calculated
                Notes = itemRequest.Notes
            };

            order.OrderItems.Add(orderItem);
        }
    }

    /// <summary>
    /// Save the order and log the action
    /// </summary>
    private async Task SaveOrderAsync(Order order, string userId, string orderNumber)
    {
        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // Audit log
        await _auditService.LogActionAsync(
            userId,
            "Create",
            "Order",
            order.Id.ToString(),
            $"Created order {orderNumber}");

        _logger.LogInformation("Order {OrderNumber} created successfully by user {UserId}",
            orderNumber, userId);
    }

    public async Task<Order?> GetOrderAsync(int orderId)
    {
        return await _orderRepository.GetByIdAsync(orderId);
    }

    public async Task<Order?> GetOrderByNumberAsync(string orderNumber, int branchId)
    {
        return await _orderRepository.GetByOrderNumberAsync(orderNumber, branchId);
    }

    public async Task<(List<Order> Orders, int TotalCount)> SearchOrdersAsync(OrderSearchRequest searchRequest)
    {
        return await _orderRepository.SearchAsync(
            searchRequest.BranchId,
            searchRequest.OrderTypeId,
            searchRequest.StatusId,
            searchRequest.FromDate,
            searchRequest.ToDate,
            searchRequest.OrderNumber,
            searchRequest.CustomerId,
            searchRequest.CashierId,
            searchRequest.Page,
            searchRequest.PageSize);
    }

    public async Task<Order?> UpdateOrderAsync(int orderId, UpdateOrderRequest request, string userId)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return null;

            // Store old values for audit
            var oldValues = new
            {
                order.StatusId,
                order.Notes,
                order.EstimatedCompletionDate,
                order.ReturnReason
            };

            // Update fields if provided
            if (request.StatusId != 0)
                order.StatusId = request.StatusId;

            if (request.Notes != null)
                order.Notes = request.Notes;

            if (request.EstimatedCompletionDate.HasValue)
                order.EstimatedCompletionDate = request.EstimatedCompletionDate.Value;

            if (request.ReturnReason != null)
                order.ReturnReason = request.ReturnReason;

            // Update audit fields
            order.ModifiedAt = DateTime.UtcNow;
            order.ModifiedBy = userId;

            // Save changes
            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogActionAsync(
                userId,
                "Update",
                "Order",
                orderId.ToString(),
                $"Updated order {order.OrderNumber}");

            _logger.LogInformation("Order {OrderNumber} updated successfully by user {UserId}", 
                order.OrderNumber, userId);

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId}", orderId);
            throw;
        }
    }

    public async Task<OrderPaymentResult> ProcessOrderPaymentAsync(int orderId, ProcessOrderPaymentRequest request, string userId)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return new OrderPaymentResult { IsSuccess = false, ErrorMessage = "Order not found" };

            if (order.FinancialTransactionId.HasValue)
                return new OrderPaymentResult { IsSuccess = false, ErrorMessage = "Order already has a payment transaction" };

            // Calculate order totals (this would typically involve product pricing logic)
            var subtotal = order.OrderItems.Sum(oi => oi.TotalAmount);
            var totalTaxAmount = order.OrderItems.Sum(oi => oi.TaxAmount);
            var totalDiscountAmount = order.OrderItems.Sum(oi => oi.DiscountAmount);
            var totalAmount = subtotal + totalTaxAmount - totalDiscountAmount;

            // Create financial transaction
            var financialTransactionRequest = new CreateFinancialTransactionRequest
            {
                BranchId = order.BranchId,
                TransactionTypeId = LookupTableConstants.FinancialTransactionTypeSale,
                BusinessEntityId = order.Id,
                BusinessEntityTypeId = LookupTableConstants.BusinessEntityTypeOrder,
                Subtotal = subtotal,
                TotalTaxAmount = totalTaxAmount,
                TotalDiscountAmount = totalDiscountAmount,
                TotalAmount = totalAmount,
                AmountPaid = request.AmountPaid,
                ChangeGiven = request.AmountPaid - totalAmount,
                PaymentMethodId = request.PaymentMethodId,
                Notes = request.Notes,
                ApprovedByUserId = null // No approval needed for current workflow
            };

            var financialTransaction = await _financialTransactionService.CreateFinancialTransactionAsync(financialTransactionRequest, userId);

            // Link financial transaction to order
            order.FinancialTransactionId = financialTransaction.Id;
            order.StatusId = LookupTableConstants.OrderStatusCompleted; // Mark as completed since payment is processed
            order.ModifiedAt = DateTime.UtcNow;
            order.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogActionAsync(
                userId,
                "ProcessPayment",
                "Order",
                orderId.ToString(),
                $"Processed payment for order {order.OrderNumber}. Amount: {request.AmountPaid}");

            _logger.LogInformation("Payment processed for order {OrderNumber} by user {UserId}. Amount: {Amount}", 
                order.OrderNumber, userId, request.AmountPaid);

            return new OrderPaymentResult 
            { 
                IsSuccess = true, 
                Order = order, 
                FinancialTransaction = financialTransaction 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for order {OrderId}", orderId);
            return new OrderPaymentResult { IsSuccess = false, ErrorMessage = "An error occurred while processing payment" };
        }
    }

    public async Task<OrderResult> CreateReturnOrderAsync(int originalOrderId, CreateReturnOrderRequest request, string userId, string managerId)
    {
        try
        {
            var originalOrder = await _orderRepository.GetByIdAsync(originalOrderId);
            if (originalOrder == null)
                return new OrderResult { IsSuccess = false, ErrorMessage = "Original order not found" };

            // Validate if order can be returned
            var (canReturn, errorMessage) = await CanReturnOrderAsync(originalOrderId);
            if (!canReturn)
                return new OrderResult { IsSuccess = false, ErrorMessage = errorMessage };

            // Create return order
            var returnOrderRequest = new CreateOrderRequest
            {
                BranchId = originalOrder.BranchId,
                OrderTypeId = LookupTableConstants.OrderTypeReturn,
                CustomerId = originalOrder.CustomerId,
                GoldRateId = originalOrder.GoldRateId,
                Notes = request.Notes,
                Items = request.Items.Select(i => new CreateOrderItemRequest
                {
                    ProductId = originalOrder.OrderItems.First(oi => oi.Id == i.OriginalOrderItemId).ProductId,
                    Quantity = i.QuantityToReturn,
                    Notes = i.ReturnReason
                }).ToList()
            };

            var returnOrder = await CreateOrderAsync(returnOrderRequest, userId);

            // Link to original order
            returnOrder.OriginalOrderId = originalOrderId;
            returnOrder.ReturnReason = request.ReturnReason;
            await _unitOfWork.SaveChangesAsync();

            // Update original order status
            originalOrder.StatusId = LookupTableConstants.OrderStatusRefunded;
            originalOrder.ModifiedAt = DateTime.UtcNow;
            originalOrder.ModifiedBy = userId;
            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogActionAsync(
                userId,
                "CreateReturn",
                "Order",
                originalOrderId.ToString(),
                $"Created return order {returnOrder.OrderNumber} for {originalOrder.OrderNumber}. Reason: {request.ReturnReason}");

            _logger.LogInformation("Return order {ReturnNumber} created for {OriginalNumber} by user {UserId}", 
                returnOrder.OrderNumber, originalOrder.OrderNumber, userId);

            return new OrderResult { IsSuccess = true, Order = returnOrder };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating return order for {OriginalOrderId}", originalOrderId);
            return new OrderResult { IsSuccess = false, ErrorMessage = "An error occurred while creating the return order" };
        }
    }

    public async Task<OrderResult> CreateExchangeOrderAsync(int originalOrderId, CreateExchangeOrderRequest request, string userId, string managerId)
    {
        try
        {
            var originalOrder = await _orderRepository.GetByIdAsync(originalOrderId);
            if (originalOrder == null)
                return new OrderResult { IsSuccess = false, ErrorMessage = "Original order not found" };

            // Validate if order can be exchanged
            var (canExchange, errorMessage) = await CanExchangeOrderAsync(originalOrderId);
            if (!canExchange)
                return new OrderResult { IsSuccess = false, ErrorMessage = errorMessage };

            // Create exchange order
            var exchangeOrderRequest = new CreateOrderRequest
            {
                BranchId = originalOrder.BranchId,
                OrderTypeId = LookupTableConstants.OrderTypeExchange,
                CustomerId = originalOrder.CustomerId,
                GoldRateId = originalOrder.GoldRateId,
                Notes = request.Notes,
                EstimatedCompletionDate = request.EstimatedCompletionDate,
                Items = request.Items.Select(i => new CreateOrderItemRequest
                {
                    ProductId = i.NewProductId,
                    Quantity = i.NewQuantity,
                    Notes = i.ExchangeReason
                }).ToList()
            };

            var exchangeOrder = await CreateOrderAsync(exchangeOrderRequest, userId);

            // Link to original order
            exchangeOrder.OriginalOrderId = originalOrderId;
            await _unitOfWork.SaveChangesAsync();

            // Update original order status
            originalOrder.StatusId = LookupTableConstants.OrderStatusCompleted;
            originalOrder.ModifiedAt = DateTime.UtcNow;
            originalOrder.ModifiedBy = userId;
            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogActionAsync(
                userId,
                "CreateExchange",
                "Order",
                originalOrderId.ToString(),
                $"Created exchange order {exchangeOrder.OrderNumber} for {originalOrder.OrderNumber}. Reason: {request.ExchangeReason}");

            _logger.LogInformation("Exchange order {ExchangeNumber} created for {OriginalNumber} by user {UserId}", 
                exchangeOrder.OrderNumber, originalOrder.OrderNumber, userId);

            return new OrderResult { IsSuccess = true, Order = exchangeOrder };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating exchange order for {OriginalOrderId}", originalOrderId);
            return new OrderResult { IsSuccess = false, ErrorMessage = "An error occurred while creating the exchange order" };
        }
    }

    public async Task<OrderResult> CancelOrderAsync(int orderId, string reason, string userId, string managerId)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return new OrderResult { IsSuccess = false, ErrorMessage = "Order not found" };

            // Validate if order can be cancelled
            var (canCancel, errorMessage) = await CanCancelOrderAsync(orderId);
            if (!canCancel)
                return new OrderResult { IsSuccess = false, ErrorMessage = errorMessage };

            // Store old values for audit
            var oldValues = new { order.StatusId, order.Notes };

            // Update order status
            order.StatusId = LookupTableConstants.OrderStatusCancelled;
            order.Notes = string.IsNullOrEmpty(order.Notes) ? reason : $"{order.Notes}; Cancelled: {reason}";
            order.ModifiedAt = DateTime.UtcNow;
            order.ModifiedBy = userId;

            // Save changes
            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogActionAsync(
                userId,
                "Cancel",
                "Order",
                orderId.ToString(),
                $"Cancelled order {order.OrderNumber}. Reason: {reason}");

            _logger.LogInformation("Order {OrderNumber} cancelled successfully by user {UserId}", 
                order.OrderNumber, userId);

            return new OrderResult { IsSuccess = true, Order = order };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
            return new OrderResult { IsSuccess = false, ErrorMessage = "An error occurred while cancelling the order" };
        }
    }

    public async Task<(bool CanReturn, string? ErrorMessage)> CanReturnOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return (false, "Order not found");

        if (order.StatusId != LookupTableConstants.OrderStatusCompleted)
            return (false, "Only completed orders can be returned");

        // Add additional business rules as needed
        // For example, check if order is within returnable time window
        var timeSinceOrder = DateTime.UtcNow - order.OrderDate;
        if (timeSinceOrder.TotalDays > 30)
            return (false, "Orders can only be returned within 30 days");

        return (true, null);
    }

    public async Task<(bool CanExchange, string? ErrorMessage)> CanExchangeOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return (false, "Order not found");

        if (order.StatusId != LookupTableConstants.OrderStatusCompleted)
            return (false, "Only completed orders can be exchanged");

        // Add additional business rules as needed
        var timeSinceOrder = DateTime.UtcNow - order.OrderDate;
        if (timeSinceOrder.TotalDays > 60)
            return (false, "Orders can only be exchanged within 60 days");

        return (true, null);
    }

    public async Task<(bool CanCancel, string? ErrorMessage)> CanCancelOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return (false, "Order not found");

        if (order.StatusId == LookupTableConstants.OrderStatusCancelled)
            return (false, "Order is already cancelled");

        if (order.StatusId == LookupTableConstants.OrderStatusCompleted)
            return (false, "Completed orders cannot be cancelled");

        return (true, null);
    }

    public async Task<OrderSummary> GetOrderSummaryAsync(int? branchId, DateTime? fromDate, DateTime? toDate)
    {
        var repoSummary = await _orderRepository.GetSummaryAsync(branchId, fromDate, toDate);
        
        return new OrderSummary
        {
            TotalOrders = repoSummary.TotalOrders,
            TotalValue = repoSummary.TotalValue,
            OrderTypeCounts = repoSummary.OrderTypeCounts,
            StatusCounts = repoSummary.StatusCounts
        };
    }

    public async Task<string> GenerateNextOrderNumberAsync(int branchId)
    {
        return await _orderRepository.GetNextOrderNumberAsync(branchId);
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, int statusId, string userId)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return false;

            var oldStatusId = order.StatusId;
            order.StatusId = statusId;
            order.ModifiedAt = DateTime.UtcNow;
            order.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogActionAsync(
                userId,
                "UpdateStatus",
                "Order",
                orderId.ToString(),
                $"Updated order {order.OrderNumber} status from {oldStatusId} to {statusId}");

            _logger.LogInformation("Order {OrderNumber} status updated from {OldStatus} to {NewStatus} by user {UserId}", 
                order.OrderNumber, oldStatusId, statusId, userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status for {OrderId}", orderId);
            return false;
        }
    }

    public async Task<RepairOrderResult> CreateRepairOrderAsync(CreateRepairOrderRequest request, string userId)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Step 1: Create repair order
            var orderRequest = new CreateOrderRequest
            {
                BranchId = request.BranchId,
                OrderTypeId = LookupTableConstants.OrderTypeRepair, // We need to add this constant
                CustomerId = request.CustomerId,
                Notes = request.RepairDescription,
                EstimatedCompletionDate = request.EstimatedCompletionDate,
                Items = new List<CreateOrderItemRequest>() // Repair orders typically don't have product items
            };

            var order = await CreateOrderAsync(orderRequest, userId);

            // Step 2: Create financial transaction
            var financialTransactionRequest = new CreateFinancialTransactionRequest
            {
                BranchId = request.BranchId,
                TransactionTypeId = LookupTableConstants.FinancialTransactionTypeRepair,
                BusinessEntityId = order.Id,
                BusinessEntityTypeId = LookupTableConstants.BusinessEntityTypeOrder,
                Subtotal = request.RepairAmount,
                TotalTaxAmount = 0, // Repairs typically don't have tax
                TotalDiscountAmount = 0,
                TotalAmount = request.RepairAmount,
                AmountPaid = request.AmountPaid,
                ChangeGiven = request.AmountPaid - request.RepairAmount,
                PaymentMethodId = request.PaymentMethodId,
                Notes = $"Repair payment for order {order.OrderNumber}"
            };

            var financialTransaction = await _financialTransactionService.CreateFinancialTransactionAsync(financialTransactionRequest, userId);

            // Step 3: Link financial transaction to order
            order.FinancialTransactionId = financialTransaction.Id;
            await _unitOfWork.SaveChangesAsync();

            // Step 4: Create repair job
            var createRepairJobRequest = new CreateRepairJobRequestDto
            {
                FinancialTransactionId = financialTransaction.Id,
                PriorityId = request.PriorityId,
                AssignedTechnicianId = request.AssignedTechnicianId,
                TechnicianNotes = request.TechnicianNotes
            };

            var (repairJobSuccess, repairJobError, repairJob) = await _repairJobService.CreateRepairJobAsync(createRepairJobRequest, userId);

            if (!repairJobSuccess)
            {
                await transaction.RollbackAsync();
                return new RepairOrderResult 
                { 
                    IsSuccess = false, 
                    ErrorMessage = repairJobError ?? "Failed to create repair job" 
                };
            }

            await transaction.CommitAsync();

            _logger.LogInformation("Repair order {OrderNumber} with repair job {RepairJobId} created successfully by user {UserId}",
                order.OrderNumber, repairJob?.Id, userId);

            return new RepairOrderResult
            {
                IsSuccess = true,
                Order = order,
                FinancialTransaction = financialTransaction,
                RepairJob = repairJob
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating repair order");
            return new RepairOrderResult
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred while creating the repair order"
            };
        }
    }
}

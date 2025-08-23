using DijaGoldPOS.API.Models;


namespace DijaGoldPOS.API.Services;

/// <summary>
/// Interface for order processing service
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="request">Order creation request</param>
    /// <param name="userId">User creating the order</param>
    /// <returns>Created order</returns>
    Task<Order> CreateOrderAsync(CreateOrderRequest request, string userId);

    /// <summary>
    /// Get order by ID
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>Order with details</returns>
    Task<Order?> GetOrderAsync(int orderId);

    /// <summary>
    /// Get order by order number
    /// </summary>
    /// <param name="orderNumber">Order number</param>
    /// <param name="branchId">Branch ID</param>
    /// <returns>Order with details</returns>
    Task<Order?> GetOrderByNumberAsync(string orderNumber, int branchId);

    /// <summary>
    /// Search orders
    /// </summary>
    /// <param name="searchRequest">Search criteria</param>
    /// <returns>List of orders</returns>
    Task<(List<Order> Orders, int TotalCount)> SearchOrdersAsync(OrderSearchRequest searchRequest);

    /// <summary>
    /// Update order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="request">Update request</param>
    /// <param name="userId">User performing update</param>
    /// <returns>Updated order</returns>
    Task<Order?> UpdateOrderAsync(int orderId, UpdateOrderRequest request, string userId);

    /// <summary>
    /// Process order payment
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="request">Payment request</param>
    /// <param name="userId">User processing payment</param>
    /// <returns>Payment result</returns>
    Task<OrderPaymentResult> ProcessOrderPaymentAsync(int orderId, ProcessOrderPaymentRequest request, string userId);

    /// <summary>
    /// Create return order
    /// </summary>
    /// <param name="originalOrderId">Original order ID</param>
    /// <param name="request">Return request</param>
    /// <param name="userId">User creating return</param>
    /// <param name="managerId">Manager approving return</param>
    /// <returns>Return order result</returns>
    Task<OrderResult> CreateReturnOrderAsync(int originalOrderId, CreateReturnOrderRequest request, string userId, string managerId);

    /// <summary>
    /// Create exchange order
    /// </summary>
    /// <param name="originalOrderId">Original order ID</param>
    /// <param name="request">Exchange request</param>
    /// <param name="userId">User creating exchange</param>
    /// <param name="managerId">Manager approving exchange</param>
    /// <returns>Exchange order result</returns>
    Task<OrderResult> CreateExchangeOrderAsync(int originalOrderId, CreateExchangeOrderRequest request, string userId, string managerId);

    /// <summary>
    /// Cancel order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="reason">Cancellation reason</param>
    /// <param name="userId">User cancelling order</param>
    /// <param name="managerId">Manager approving cancellation</param>
    /// <returns>Cancellation result</returns>
    Task<OrderResult> CancelOrderAsync(int orderId, string reason, string userId, string managerId);

    /// <summary>
    /// Validate if an order can be returned
    /// </summary>
    /// <param name="orderId">Order ID to check</param>
    /// <returns>Validation result</returns>
    Task<(bool CanReturn, string? ErrorMessage)> CanReturnOrderAsync(int orderId);

    /// <summary>
    /// Validate if an order can be exchanged
    /// </summary>
    /// <param name="orderId">Order ID to check</param>
    /// <returns>Validation result</returns>
    Task<(bool CanExchange, string? ErrorMessage)> CanExchangeOrderAsync(int orderId);

    /// <summary>
    /// Validate if an order can be cancelled
    /// </summary>
    /// <param name="orderId">Order ID to check</param>
    /// <returns>Validation result</returns>
    Task<(bool CanCancel, string? ErrorMessage)> CanCancelOrderAsync(int orderId);

    /// <summary>
    /// Get order summary
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>Order summary</returns>
    Task<OrderSummary> GetOrderSummaryAsync(int? branchId, DateTime? fromDate, DateTime? toDate);

    /// <summary>
    /// Generate next order number for a branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <returns>Next order number</returns>
    Task<string> GenerateNextOrderNumberAsync(int branchId);

    /// <summary>
    /// Update order status
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="statusId">New status ID</param>
    /// <param name="userId">User updating status</param>
    /// <returns>Success status</returns>
    Task<bool> UpdateOrderStatusAsync(int orderId, int statusId, string userId);

    /// <summary>
    /// Create repair order with financial transaction and repair job
    /// </summary>
    /// <param name="request">Repair order request</param>
    /// <param name="userId">User creating the repair</param>
    /// <returns>Repair order result with repair job</returns>
    Task<RepairOrderResult> CreateRepairOrderAsync(CreateRepairOrderRequest request, string userId);
}

// Service request classes moved to OrderServiceRequests.cs

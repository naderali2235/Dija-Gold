using DijaGoldPOS.API.Models;


namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository interface for order operations
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    /// Get order by order number and branch
    /// </summary>
    /// <param name="orderNumber">Order number</param>
    /// <param name="branchId">Branch ID</param>
    /// <returns>Order</returns>
    Task<Order?> GetByOrderNumberAsync(string orderNumber, int branchId);

    /// <summary>
    /// Get orders by customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>List of orders</returns>
    Task<List<Order>> GetByCustomerAsync(int customerId, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Get orders by cashier
    /// </summary>
    /// <param name="cashierId">Cashier ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>List of orders</returns>
    Task<List<Order>> GetByCashierAsync(string cashierId, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Search orders with filters
    /// </summary>
    /// <param name="branchId">Branch ID filter</param>
    /// <param name="orderTypeId">Order type ID filter</param>
    /// <param name="statusId">Status ID filter</param>
    /// <param name="fromDate">From date filter</param>
    /// <param name="toDate">To date filter</param>
    /// <param name="orderNumber">Order number filter</param>
    /// <param name="customerId">Customer ID filter</param>
    /// <param name="cashierId">Cashier ID filter</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated orders</returns>
    Task<(List<Order> Orders, int TotalCount)> SearchAsync(
        int? branchId = null,
        int? orderTypeId = null,
        int? statusId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? orderNumber = null,
        int? customerId = null,
        string? cashierId = null,
        int page = 1,
        int pageSize = 20);

    /// <summary>
    /// Get order summary
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>Order summary</returns>
    Task<OrderSummary> GetSummaryAsync(int? branchId = null, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Get next order number for a branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <returns>Next order number</returns>
    Task<string> GetNextOrderNumberAsync(int branchId);

    /// <summary>
    /// Get orders by original order (for returns/exchanges)
    /// </summary>
    /// <param name="originalOrderId">Original order ID</param>
    /// <returns>List of related orders</returns>
    Task<List<Order>> GetRelatedOrdersAsync(int originalOrderId);

    /// <summary>
    /// Get orders by date range
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>List of orders</returns>
    Task<List<Order>> GetByDateRangeAsync(int branchId, DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Get orders by status
    /// </summary>
    /// <param name="statusId">Order status ID</param>
    /// <param name="branchId">Branch ID</param>
    /// <returns>List of orders</returns>
    Task<List<Order>> GetByStatusAsync(int statusId, int? branchId = null);

    /// <summary>
    /// Get orders by type
    /// </summary>
    /// <param name="orderTypeId">Order type ID</param>
    /// <param name="branchId">Branch ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>List of orders</returns>
    Task<List<Order>> GetByTypeAsync(int orderTypeId, int? branchId = null, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Get orders with financial transactions
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>List of orders with financial transactions</returns>
    Task<List<Order>> GetWithFinancialTransactionsAsync(int branchId, DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Get orders pending payment
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <returns>List of orders pending payment</returns>
    Task<List<Order>> GetPendingPaymentAsync(int? branchId = null);

    /// <summary>
    /// Get orders ready for pickup
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <returns>List of orders ready for pickup</returns>
    Task<List<Order>> GetReadyForPickupAsync(int? branchId = null);
}

/// <summary>
/// Order summary for reporting
/// </summary>
public class OrderSummary
{
    public int TotalOrders { get; set; }
    public decimal TotalValue { get; set; }
    public Dictionary<int, int> OrderTypeCounts { get; set; } = new(); // Changed from OrderType to int
    public Dictionary<int, int> StatusCounts { get; set; } = new(); // Changed from OrderStatus to int
}

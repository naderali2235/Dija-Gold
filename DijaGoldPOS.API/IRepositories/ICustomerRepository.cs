using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.IRepositories;

/// <summary>
/// Repository interface for Customer entity with specific business methods
/// </summary>
public interface ICustomerRepository : IRepository<Customer>
{
    /// <summary>
    /// Get customer by national ID
    /// </summary>
    /// <param name="nationalId">National ID</param>
    /// <returns>Customer or null if not found</returns>
    Task<Customer?> GetByNationalIdAsync(string nationalId);

    /// <summary>
    /// Get customer by mobile number
    /// </summary>
    /// <param name="mobileNumber">Mobile number</param>
    /// <returns>Customer or null if not found</returns>
    Task<Customer?> GetByMobileNumberAsync(string mobileNumber);

    /// <summary>
    /// Get customer by email
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>Customer or null if not found</returns>
    Task<Customer?> GetByEmailAsync(string email);

    /// <summary>
    /// Search customers by name, mobile, or email
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>List of matching customers</returns>
    Task<List<Customer>> SearchAsync(string searchTerm);

    /// <summary>
    /// Get top customers by purchase amount
    /// </summary>
    /// <param name="topCount">Number of top customers to return</param>
    /// <param name="fromDate">From date (optional)</param>
    /// <param name="toDate">To date (optional)</param>
    /// <returns>List of top customers</returns>
    Task<List<Customer>> GetTopCustomersAsync(int topCount = 10, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Get customers with loyalty discounts
    /// </summary>
    /// <returns>List of customers with active loyalty discounts</returns>
    Task<List<Customer>> GetLoyaltyCustomersAsync();

    /// <summary>
    /// Check if national ID exists
    /// </summary>
    /// <param name="nationalId">National ID to check</param>
    /// <param name="excludeId">Customer ID to exclude (for updates)</param>
    /// <returns>True if national ID exists</returns>
    Task<bool> NationalIdExistsAsync(string nationalId, int? excludeId = null);

    /// <summary>
    /// Check if mobile number exists
    /// </summary>
    /// <param name="mobileNumber">Mobile number to check</param>
    /// <param name="excludeId">Customer ID to exclude (for updates)</param>
    /// <returns>True if mobile number exists</returns>
    Task<bool> MobileNumberExistsAsync(string mobileNumber, int? excludeId = null);

    /// <summary>
    /// Check if email exists
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="excludeId">Customer ID to exclude (for updates)</param>
    /// <returns>True if email exists</returns>
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);

    /// <summary>
    /// Update customer purchase statistics
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="transactionAmount">Transaction amount to add</param>
    /// <returns>Updated customer</returns>
    Task<Customer?> UpdatePurchaseStatisticsAsync(int customerId, decimal transactionAmount);
}

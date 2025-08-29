using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Shared;

namespace DijaGoldPOS.API.IServices;

/// <summary>
/// Service interface for customer business operations
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Get paginated customers with search filters
    /// </summary>
    /// <param name="searchRequest">Search parameters</param>
    /// <returns>Paginated customer results</returns>
    Task<PagedResult<CustomerDto>> GetCustomersAsync(CustomerSearchRequestDto searchRequest);

    /// <summary>
    /// Get customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer DTO or null if not found</returns>
    Task<CustomerDto?> GetCustomerByIdAsync(int id);

    /// <summary>
    /// Create a new customer
    /// </summary>
    /// <param name="request">Customer creation request</param>
    /// <returns>Created customer DTO</returns>
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequestDto request);

    /// <summary>
    /// Update an existing customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="request">Customer update request</param>
    /// <returns>Updated customer DTO</returns>
    Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerRequestDto request);

    /// <summary>
    /// Soft delete a customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Success indicator</returns>
    Task<bool> DeleteCustomerAsync(int id);

    /// <summary>
    /// Get customer orders history
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="fromDate">From date (optional)</param>
    /// <param name="toDate">To date (optional)</param>
    /// <returns>Customer orders history</returns>
    Task<CustomerOrdersHistoryDto> GetCustomerOrdersAsync(int customerId, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Get customer loyalty information
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Customer loyalty DTO</returns>
    Task<CustomerLoyaltyDto> GetCustomerLoyaltyAsync(int customerId);

    /// <summary>
    /// Update customer loyalty status
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="request">Loyalty update request</param>
    /// <returns>Updated loyalty DTO</returns>
    Task<CustomerLoyaltyDto> UpdateCustomerLoyaltyAsync(int customerId, UpdateCustomerLoyaltyRequestDto request);

    /// <summary>
    /// Search customers by term
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="limit">Maximum results</param>
    /// <returns>List of matching customers</returns>
    Task<List<CustomerDto>> SearchCustomersAsync(string searchTerm, int limit = 10);

    /// <summary>
    /// Check if customer exists by national ID
    /// </summary>
    /// <param name="nationalId">National ID</param>
    /// <param name="excludeId">Customer ID to exclude</param>
    /// <returns>True if exists</returns>
    Task<bool> CustomerExistsByNationalIdAsync(string nationalId, int? excludeId = null);

    /// <summary>
    /// Check if customer exists by mobile number
    /// </summary>
    /// <param name="mobileNumber">Mobile number</param>
    /// <param name="excludeId">Customer ID to exclude</param>
    /// <returns>True if exists</returns>
    Task<bool> CustomerExistsByMobileNumberAsync(string mobileNumber, int? excludeId = null);

    /// <summary>
    /// Check if customer exists by email
    /// </summary>
    /// <param name="email">Email</param>
    /// <param name="excludeId">Customer ID to exclude</param>
    /// <returns>True if exists</returns>
    Task<bool> CustomerExistsByEmailAsync(string email, int? excludeId = null);
}

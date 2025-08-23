using DijaGoldPOS.API.Models;


namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository interface for GoldRate entity with specific business methods
/// </summary>
public interface IGoldRateRepository : IRepository<GoldRate>
{
    /// <summary>
    /// Get current active gold rate for a specific karat type
    /// </summary>
    /// <param name="karatTypeId">Karat type ID</param>
    /// <returns>Current gold rate or null if not found</returns>
    Task<GoldRate?> GetCurrentRateAsync(int karatTypeId);

    /// <summary>
    /// Get current active gold rates for all karat types
    /// </summary>
    /// <returns>Dictionary with karat type ID as key and current rate as value</returns>
    Task<Dictionary<int, GoldRate>> GetCurrentRatesAsync();

    /// <summary>
    /// Get gold rate effective at a specific date and time
    /// </summary>
    /// <param name="karatTypeId">Karat type ID</param>
    /// <param name="effectiveDate">Effective date and time</param>
    /// <returns>Gold rate effective at the specified date or null if not found</returns>
    Task<GoldRate?> GetRateAtDateAsync(int karatTypeId, DateTime effectiveDate);

    /// <summary>
    /// Get rate history for a specific karat type
    /// </summary>
    /// <param name="karatTypeId">Karat type ID</param>
    /// <param name="fromDate">From date (optional)</param>
    /// <param name="toDate">To date (optional)</param>
    /// <returns>List of gold rates ordered by effective date descending</returns>
    Task<List<GoldRate>> GetRateHistoryAsync(int karatTypeId, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Get latest rate update by user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Latest gold rate updated by the user</returns>
    Task<GoldRate?> GetLatestRateByUserAsync(string userId);

    /// <summary>
    /// Check if a rate exists for the same karat type and effective date
    /// </summary>
    /// <param name="karatTypeId">Karat type ID</param>
    /// <param name="effectiveFrom">Effective from date</param>
    /// <param name="excludeId">Rate ID to exclude (for updates)</param>
    /// <returns>True if rate exists</returns>
    Task<bool> RateExistsAsync(int karatTypeId, DateTime effectiveFrom, int? excludeId = null);

    /// <summary>
    /// Deactivate previous rates when a new rate becomes effective
    /// </summary>
    /// <param name="karatTypeId">Karat type ID</param>
    /// <param name="newEffectiveFrom">New rate effective from date</param>
    /// <returns>Number of rates updated</returns>
    Task<int> DeactivatePreviousRatesAsync(int karatTypeId, DateTime newEffectiveFrom);

    /// <summary>
    /// Get rate changes summary for reporting
    /// </summary>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>List of rate changes with percentage changes</returns>
    Task<List<(GoldRate Rate, decimal? PreviousRate, decimal? PercentageChange)>> GetRateChangesAsync(DateTime fromDate, DateTime toDate);
}
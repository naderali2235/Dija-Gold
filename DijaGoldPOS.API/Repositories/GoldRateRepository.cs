using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository implementation for GoldRate entity with specific business methods
/// </summary>
public class GoldRateRepository : Repository<GoldRate>, IGoldRateRepository
{
    public GoldRateRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get current active gold rate for a specific karat type
    /// </summary>
    public async Task<GoldRate?> GetCurrentRateAsync(KaratType karatType)
    {
        return await _dbSet
            .Where(gr => gr.KaratType == karatType && gr.EffectiveTo == null)
            .OrderByDescending(gr => gr.EffectiveFrom)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get current active gold rates for all karat types
    /// </summary>
    public async Task<Dictionary<KaratType, GoldRate>> GetCurrentRatesAsync()
    {
        var currentRates = await _dbSet
            .Where(gr => gr.EffectiveTo == null)
            .GroupBy(gr => gr.KaratType)
            .Select(g => g.OrderByDescending(gr => gr.EffectiveFrom).First())
            .ToListAsync();

        return currentRates.ToDictionary(gr => gr.KaratType, gr => gr);
    }

    /// <summary>
    /// Get gold rate effective at a specific date and time
    /// </summary>
    public async Task<GoldRate?> GetRateAtDateAsync(KaratType karatType, DateTime effectiveDate)
    {
        return await _dbSet
            .Where(gr => gr.KaratType == karatType &&
                        gr.EffectiveFrom <= effectiveDate &&
                        (gr.EffectiveTo == null || gr.EffectiveTo > effectiveDate))
            .OrderByDescending(gr => gr.EffectiveFrom)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get rate history for a specific karat type
    /// </summary>
    public async Task<List<GoldRate>> GetRateHistoryAsync(KaratType karatType, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _dbSet.Where(gr => gr.KaratType == karatType);

        if (fromDate.HasValue)
        {
            query = query.Where(gr => gr.EffectiveFrom >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(gr => gr.EffectiveFrom <= toDate.Value);
        }

        return await query
            .OrderByDescending(gr => gr.EffectiveFrom)
            .ToListAsync();
    }

    /// <summary>
    /// Get latest rate update by user
    /// </summary>
    public async Task<GoldRate?> GetLatestRateByUserAsync(string userId)
    {
        return await _dbSet
            .Where(gr => gr.CreatedBy == userId)
            .OrderByDescending(gr => gr.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Check if a rate exists for the same karat type and effective date
    /// </summary>
    public async Task<bool> RateExistsAsync(KaratType karatType, DateTime effectiveFrom, int? excludeId = null)
    {
        var query = _dbSet
            .Where(gr => gr.KaratType == karatType && gr.EffectiveFrom == effectiveFrom);

        if (excludeId.HasValue)
        {
            query = query.Where(gr => gr.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Deactivate previous rates when a new rate becomes effective
    /// </summary>
    public async Task<int> DeactivatePreviousRatesAsync(KaratType karatType, DateTime newEffectiveFrom)
    {
        var previousRates = await _dbSet
            .Where(gr => gr.KaratType == karatType && 
                        gr.EffectiveFrom < newEffectiveFrom && 
                        gr.EffectiveTo == null)
            .ToListAsync();

        foreach (var rate in previousRates)
        {
            rate.EffectiveTo = newEffectiveFrom;
        }

        return previousRates.Count;
    }

    /// <summary>
    /// Get rate changes summary for reporting
    /// </summary>
    public async Task<List<(GoldRate Rate, decimal? PreviousRate, decimal? PercentageChange)>> GetRateChangesAsync(DateTime fromDate, DateTime toDate)
    {
        var rates = await _dbSet
            .Where(gr => gr.EffectiveFrom >= fromDate && gr.EffectiveFrom <= toDate)
            .OrderBy(gr => gr.KaratType)
            .ThenBy(gr => gr.EffectiveFrom)
            .ToListAsync();

        var result = new List<(GoldRate Rate, decimal? PreviousRate, decimal? PercentageChange)>();

        foreach (var rate in rates)
        {
            var previousRate = await _dbSet
                .Where(gr => gr.KaratType == rate.KaratType && 
                            gr.EffectiveFrom < rate.EffectiveFrom)
                .OrderByDescending(gr => gr.EffectiveFrom)
                .FirstOrDefaultAsync();

            decimal? percentageChange = null;
            if (previousRate != null && previousRate.RatePerGram > 0)
            {
                percentageChange = ((rate.RatePerGram - previousRate.RatePerGram) / previousRate.RatePerGram) * 100;
            }

            result.Add((rate, previousRate?.RatePerGram, percentageChange));
        }

        return result;
    }
}
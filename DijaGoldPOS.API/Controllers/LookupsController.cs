using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DijaGoldPOS.API.Models.Enums;
using System.ComponentModel;
using System.Reflection;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Controller for managing lookup data and enum values
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LookupsController : ControllerBase
{
    /// <summary>
    /// Get all available enum values for the application
    /// </summary>
    /// <returns>Lookup data for all enums</returns>
    [HttpGet]
    public ActionResult<object> GetLookups()
    {
        try
        {
            var lookups = new
            {
                TransactionTypes = GetEnumLookup<TransactionType>(),
                PaymentMethods = GetEnumLookup<PaymentMethod>(),
                TransactionStatuses = GetEnumLookup<TransactionStatus>(),
                ChargeTypes = GetEnumLookup<ChargeType>(),
                KaratTypes = GetEnumLookup<KaratType>(),
                ProductCategoryTypes = GetEnumLookup<ProductCategoryType>()
            };

            return Ok(new { success = true, data = lookups });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to fetch lookup data", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for transaction types
    /// </summary>
    [HttpGet("transaction-types")]
    public ActionResult<IEnumerable<EnumLookupDto>> GetTransactionTypes()
    {
        try
        {
            var data = GetEnumLookup<TransactionType>();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to fetch transaction types", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for payment methods
    /// </summary>
    [HttpGet("payment-methods")]
    public ActionResult<IEnumerable<EnumLookupDto>> GetPaymentMethods()
    {
        try
        {
            var data = GetEnumLookup<PaymentMethod>();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to fetch payment methods", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for transaction statuses
    /// </summary>
    [HttpGet("transaction-statuses")]
    public ActionResult<IEnumerable<EnumLookupDto>> GetTransactionStatuses()
    {
        try
        {
            var data = GetEnumLookup<TransactionStatus>();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to fetch transaction statuses", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for charge types
    /// </summary>
    [HttpGet("charge-types")]
    public ActionResult<IEnumerable<EnumLookupDto>> GetChargeTypes()
    {
        try
        {
            var data = GetEnumLookup<ChargeType>();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to fetch charge types", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for karat types
    /// </summary>
    [HttpGet("karat-types")]
    public ActionResult<IEnumerable<EnumLookupDto>> GetKaratTypes()
    {
        try
        {
            var data = GetEnumLookup<KaratType>();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to fetch karat types", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for product category types
    /// </summary>
    [HttpGet("product-category-types")]
    public ActionResult<IEnumerable<EnumLookupDto>> GetProductCategoryTypes()
    {
        try
        {
            var data = GetEnumLookup<ProductCategoryType>();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to fetch product category types", error = ex.Message });
        }
    }

    /// <summary>
    /// Generic method to convert enum to lookup data
    /// </summary>
    /// <typeparam name="T">Enum type</typeparam>
    /// <returns>List of enum lookup DTOs</returns>
    private static List<EnumLookupDto> GetEnumLookup<T>() where T : struct, System.Enum
    {
        return Enum.GetValues<T>()
            .Select(enumValue => new EnumLookupDto
            {
                Value = Convert.ToInt32(enumValue),
                Name = enumValue.ToString(),
                DisplayName = GetEnumDescription(enumValue) ?? enumValue.ToString(),
                Code = enumValue.ToString()
            })
            .OrderBy(x => x.Value)
            .ToList();
    }

    /// <summary>
    /// Get description attribute value from enum if available
    /// </summary>
    /// <param name="enumValue">Enum value</param>
    /// <returns>Description or null</returns>
    private static string? GetEnumDescription(Enum enumValue)
    {
        var field = enumValue.GetType().GetField(enumValue.ToString());
        if (field == null) return null;

        var attribute = field.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description;
    }
}

/// <summary>
/// DTO for enum lookup data
/// </summary>
public class EnumLookupDto
{
    /// <summary>
    /// Enum numeric value
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Enum name (e.g., "K18", "Sale")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display-friendly name for UI
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Code for API usage
    /// </summary>
    public string Code { get; set; } = string.Empty;
}

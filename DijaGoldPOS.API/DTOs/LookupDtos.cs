namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Base DTO for lookup entities
/// </summary>
public abstract class BaseLookupDto
{
    /// <summary>
    /// Lookup ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Lookup name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Lookup description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Sort order for display
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Whether the lookup is active
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for karat type lookup data
/// </summary>
public class KaratTypeLookupDto : BaseLookupDto
{
}

/// <summary>
/// DTO for financial transaction type lookup data
/// </summary>
public class FinancialTransactionTypeLookupDto : BaseLookupDto
{
}

/// <summary>
/// DTO for payment method lookup data
/// </summary>
public class PaymentMethodLookupDto : BaseLookupDto
{
}

/// <summary>
/// DTO for financial transaction status lookup data
/// </summary>
public class FinancialTransactionStatusLookupDto : BaseLookupDto
{
}

/// <summary>
/// DTO for charge type lookup data
/// </summary>
public class ChargeTypeLookupDto : BaseLookupDto
{
}

/// <summary>
/// DTO for product category type lookup data
/// </summary>
public class ProductCategoryTypeLookupDto : BaseLookupDto
{
}

/// <summary>
/// DTO for repair status lookup data
/// </summary>
public class RepairStatusLookupDto : BaseLookupDto
{
}

/// <summary>
/// DTO for repair priority lookup data
/// </summary>
public class RepairPriorityLookupDto : BaseLookupDto
{
}

/// <summary>
/// DTO for order type lookup data
/// </summary>
public class OrderTypeLookupDto : BaseLookupDto
{
}

/// <summary>
/// DTO for order status lookup data
/// </summary>
public class OrderStatusLookupDto : BaseLookupDto
{
}

/// <summary>
/// DTO for business entity type lookup data
/// </summary>
public class BusinessEntityTypeLookupDto : BaseLookupDto
{
}

/// <summary>
/// DTO for sub-category lookup data
/// </summary>
public class SubCategoryLookupDto : BaseLookupDto
{
}

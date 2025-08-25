namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Transaction status lookup DTO
/// </summary>
public class TransactionStatusLookupDto : BaseLookupDto
{
}

/// <summary>
/// Transaction type lookup DTO
/// </summary>
public class TransactionTypeLookupDto : BaseLookupDto
{
}

/// <summary>
/// Financial transaction type lookup DTO
/// </summary>
public class FinancialTransactionTypeLookupDto : BaseLookupDto
{
}

/// <summary>
/// Payment method lookup DTO
/// </summary>
public class PaymentMethodLookupDto : BaseLookupDto
{
}

/// <summary>
/// Financial transaction status lookup DTO
/// </summary>
public class FinancialTransactionStatusLookupDto : BaseLookupDto
{
}

/// <summary>
/// Karat type lookup DTO
/// </summary>
public class KaratTypeLookupDto : BaseLookupDto
{
}

/// <summary>
/// Charge type lookup DTO
/// </summary>
public class ChargeTypeLookupDto : BaseLookupDto
{
}

/// <summary>
/// Product category type lookup DTO
/// </summary>
public class ProductCategoryTypeLookupDto : BaseLookupDto
{
}

/// <summary>
/// Repair status lookup DTO
/// </summary>
public class RepairStatusLookupDto : BaseLookupDto
{
}

/// <summary>
/// Repair priority lookup DTO
/// </summary>
public class RepairPriorityLookupDto : BaseLookupDto
{
}

/// <summary>
/// Order type lookup DTO
/// </summary>
public class OrderTypeLookupDto : BaseLookupDto
{
}

/// <summary>
/// Order status lookup DTO
/// </summary>
public class OrderStatusLookupDto : BaseLookupDto
{
}

/// <summary>
/// Business entity type lookup DTO
/// </summary>
public class BusinessEntityTypeLookupDto : BaseLookupDto
{
}

/// <summary>
/// Sub-category lookup DTO
/// </summary>
public class SubCategoryLookupDto : BaseLookupDto
{
}

/// <summary>
/// Base lookup DTO for common lookup operations
/// </summary>
public class BaseLookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}
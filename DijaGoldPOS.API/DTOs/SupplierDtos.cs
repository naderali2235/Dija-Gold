

namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Supplier DTO for list/display operations
/// </summary>
public class SupplierDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPersonName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxRegistrationNumber { get; set; }
    public string? CommercialRegistrationNumber { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public int PaymentTermsDays { get; set; }
    public bool CreditLimitEnforced { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystemSupplier { get; set; }
}

/// <summary>
/// Create supplier request DTO
/// </summary>
public class CreateSupplierRequestDto
{


    public string CompanyName { get; set; } = string.Empty;


    public string? ContactPersonName { get; set; }


    public string? Phone { get; set; }

    public string? Email { get; set; }


    public string? Address { get; set; }


    public string? TaxRegistrationNumber { get; set; }


    public string? CommercialRegistrationNumber { get; set; }

    public decimal CreditLimit { get; set; } = 0;

    public int PaymentTermsDays { get; set; } = 30;

    public bool CreditLimitEnforced { get; set; } = true;


    public string? PaymentTerms { get; set; }


    public string? Notes { get; set; }
}

/// <summary>
/// Update supplier request DTO
/// </summary>
public class UpdateSupplierRequestDto : CreateSupplierRequestDto
{

    public int Id { get; set; }
}

/// <summary>
/// Supplier search request DTO
/// </summary>
public class SupplierSearchRequestDto
{
    public string? SearchTerm { get; set; }
    public string? TaxRegistrationNumber { get; set; }
    public string? CommercialRegistrationNumber { get; set; }
    public bool? CreditLimitEnforced { get; set; }
    public bool? IsActive { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Supplier balance DTO
/// </summary>
public class SupplierBalanceDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal AvailableCredit { get; set; }
    public int PaymentTermsDays { get; set; }
    public bool CreditLimitEnforced { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public List<SupplierTransactionDto> RecentTransactions { get; set; } = new();
}

/// <summary>
/// Supplier transaction DTO
/// </summary>
public class SupplierTransactionDto
{
    public int TransactionId { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceAfterTransaction { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Update supplier balance request DTO
/// </summary>
public class UpdateSupplierBalanceRequestDto
{

    public int SupplierId { get; set; }


    public decimal Amount { get; set; }


    public string TransactionType { get; set; } = string.Empty; // "payment", "adjustment", "credit"


    public string? ReferenceNumber { get; set; }


    public string? Notes { get; set; }
}

/// <summary>
/// Supplier products DTO
/// </summary>
public class SupplierProductsDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public List<SupplierProductDto> Products { get; set; } = new();
    public int TotalProductCount { get; set; }
}

/// <summary>
/// Individual supplier product DTO
/// </summary>
public class SupplierProductDto
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryType { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Supplier credit alert DTO
/// </summary>
public class SupplierCreditAlertDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal AvailableCredit { get; set; }
    public decimal CreditUtilizationPercentage { get; set; }
    public string AlertType { get; set; } = string.Empty; // "near_limit", "over_limit", "warning"
    public string Severity { get; set; } = string.Empty; // "low", "medium", "high", "critical"
    public string Message { get; set; } = string.Empty;
    public string? ContactPersonName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Supplier credit validation result
/// </summary>
public class SupplierCreditValidationResult
{
    public bool CanPurchase { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal AvailableCredit { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal CreditLimit { get; set; }
    public bool CreditLimitEnforced { get; set; }
    public List<string> Warnings { get; set; } = new();
}

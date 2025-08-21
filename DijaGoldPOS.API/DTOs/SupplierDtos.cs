using System.ComponentModel.DataAnnotations;

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
}

/// <summary>
/// Create supplier request DTO
/// </summary>
public class CreateSupplierRequestDto
{
    [Required(ErrorMessage = "Company name is required")]
    [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Contact person name cannot exceed 100 characters")]
    public string? ContactPersonName { get; set; }

    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string? Email { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    [StringLength(50, ErrorMessage = "Tax registration number cannot exceed 50 characters")]
    public string? TaxRegistrationNumber { get; set; }

    [StringLength(50, ErrorMessage = "Commercial registration number cannot exceed 50 characters")]
    public string? CommercialRegistrationNumber { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Credit limit cannot be negative")]
    public decimal CreditLimit { get; set; } = 0;

    [Range(0, 365, ErrorMessage = "Payment terms must be between 0 and 365 days")]
    public int PaymentTermsDays { get; set; } = 30;

    public bool CreditLimitEnforced { get; set; } = true;

    [StringLength(1000, ErrorMessage = "Payment terms cannot exceed 1000 characters")]
    public string? PaymentTerms { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// Update supplier request DTO
/// </summary>
public class UpdateSupplierRequestDto : CreateSupplierRequestDto
{
    [Required(ErrorMessage = "Supplier ID is required")]
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
    [Required(ErrorMessage = "Supplier ID is required")]
    public int SupplierId { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Transaction type is required")]
    public string TransactionType { get; set; } = string.Empty; // "payment", "adjustment", "credit"

    [StringLength(100, ErrorMessage = "Reference number cannot exceed 100 characters")]
    public string? ReferenceNumber { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
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
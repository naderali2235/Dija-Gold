using System.ComponentModel.DataAnnotations;

namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Customer DTO for list/display operations
/// </summary>
public class CustomerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime RegistrationDate { get; set; }
    public int LoyaltyTier { get; set; }
    public int LoyaltyPoints { get; set; }
    public decimal TotalPurchaseAmount { get; set; }
    public decimal DefaultDiscountPercentage { get; set; }
    public bool MakingChargesWaived { get; set; }
    public string? Notes { get; set; }
    public DateTime? LastPurchaseDate { get; set; }
    public int TotalTransactions { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Create customer request DTO
/// </summary>
public class CreateCustomerRequestDto
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "National ID cannot exceed 20 characters")]
    public string? NationalId { get; set; }

    [StringLength(15, ErrorMessage = "Mobile number cannot exceed 15 characters")]
    public string? MobileNumber { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string? Email { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    [Range(1, 5, ErrorMessage = "Loyalty tier must be between 1 and 5")]
    public int LoyaltyTier { get; set; } = 1;

    [Range(0, 100, ErrorMessage = "Default discount percentage must be between 0 and 100")]
    public decimal DefaultDiscountPercentage { get; set; } = 0;

    public bool MakingChargesWaived { get; set; } = false;

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// Update customer request DTO
/// </summary>
public class UpdateCustomerRequestDto : CreateCustomerRequestDto
{
    [Required(ErrorMessage = "Customer ID is required")]
    public int Id { get; set; }
}

/// <summary>
/// Customer search request DTO
/// </summary>
public class CustomerSearchRequestDto
{
    public string? SearchTerm { get; set; }
    public string? NationalId { get; set; }
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }
    public int? LoyaltyTier { get; set; }
    public bool? IsActive { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Customer transaction history DTO
/// </summary>
public class CustomerTransactionHistoryDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public List<CustomerTransactionDto> Transactions { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public int TotalTransactionCount { get; set; }
}

/// <summary>
/// Individual customer transaction DTO
/// </summary>
public class CustomerTransactionDto
{
    public int TransactionId { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string CashierName { get; set; } = string.Empty;
}

/// <summary>
/// Customer loyalty status DTO
/// </summary>
public class CustomerLoyaltyDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int CurrentTier { get; set; }
    public int CurrentPoints { get; set; }
    public int PointsToNextTier { get; set; }
    public decimal TotalPurchaseAmount { get; set; }
    public decimal DefaultDiscountPercentage { get; set; }
    public bool MakingChargesWaived { get; set; }
    public DateTime? LastPurchaseDate { get; set; }
    public int TotalTransactions { get; set; }
}

/// <summary>
/// Update customer loyalty request DTO
/// </summary>
public class UpdateCustomerLoyaltyRequestDto
{
    [Required(ErrorMessage = "Customer ID is required")]
    public int CustomerId { get; set; }

    [Range(1, 5, ErrorMessage = "Loyalty tier must be between 1 and 5")]
    public int LoyaltyTier { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Loyalty points cannot be negative")]
    public int LoyaltyPoints { get; set; }

    [Range(0, 100, ErrorMessage = "Default discount percentage must be between 0 and 100")]
    public decimal DefaultDiscountPercentage { get; set; }

    public bool MakingChargesWaived { get; set; }
}
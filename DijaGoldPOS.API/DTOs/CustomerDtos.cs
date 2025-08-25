

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
    public int TotalOrders { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Create customer request DTO
/// </summary>
public class CreateCustomerRequestDto
{


    public string FullName { get; set; } = string.Empty;


    public string? NationalId { get; set; }


    public string? MobileNumber { get; set; }

    //[EmailAddress(ErrorMessage = "Invalid email address")]

    public string? Email { get; set; }


    public string? Address { get; set; }


    public int LoyaltyTier { get; set; } = 1;


    public decimal DefaultDiscountPercentage { get; set; } = 0;

    public bool MakingChargesWaived { get; set; } = false;


    public string? Notes { get; set; }
}

/// <summary>
/// Update customer request DTO
/// </summary>
public class UpdateCustomerRequestDto : CreateCustomerRequestDto
{

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
/// Customer Order history DTO
/// </summary>
public class CustomerOrdersHistoryDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public List<CustomerOrderDto> Orders { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public int TotalOrderCount { get; set; }
}

/// <summary>
/// Individual customer Order DTO
/// </summary>
public class CustomerOrderDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string OrderType { get; set; } = string.Empty;
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
    public int TotalOrders { get; set; }
}

/// <summary>
/// Update customer loyalty request DTO
/// </summary>
public class UpdateCustomerLoyaltyRequestDto
{

    public int CustomerId { get; set; }


    public int LoyaltyTier { get; set; }


    public int LoyaltyPoints { get; set; }


    public decimal DefaultDiscountPercentage { get; set; }

    public bool MakingChargesWaived { get; set; }
}

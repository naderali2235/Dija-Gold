namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// DTO for products with unpaid supplier balances that pose sales risk
/// </summary>
public class ProductSaleRiskDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal AvailableQuantity { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
    public List<UnpaidSupplierDto> UnpaidSuppliers { get; set; } = new();
}

/// <summary>
/// DTO for unpaid supplier information
/// </summary>
public class UnpaidSupplierDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal OutstandingAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal TotalCost { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}

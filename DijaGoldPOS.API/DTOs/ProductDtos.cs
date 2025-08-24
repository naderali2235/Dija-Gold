using System.ComponentModel.DataAnnotations;

namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Product DTO for list/display operations
/// </summary>
public class ProductDto
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CategoryTypeId { get; set; }
    public ProductCategoryTypeLookupDto? CategoryType { get; set; }
    public int KaratTypeId { get; set; }
    public KaratTypeLookupDto? KaratType { get; set; }
    public decimal Weight { get; set; }
    public string? Brand { get; set; }
    public string? DesignStyle { get; set; }
    public int? SubCategoryId { get; set; }
    public SubCategoryLookupDto? SubCategory { get; set; }
    public string? Shape { get; set; }
    public string? PurityCertificateNumber { get; set; }
    public string? CountryOfOrigin { get; set; }
    public int? YearOfMinting { get; set; }
    public decimal? FaceValue { get; set; }
    public bool? HasNumismaticValue { get; set; }
    public bool MakingChargesApplicable { get; set; }
    public int? ProductMakingChargesTypeId { get; set; }
    public decimal? ProductMakingChargesValue { get; set; }
    public bool UseProductMakingCharges { get; set; }
    public int? SupplierId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Create product request DTO
/// </summary>
public class CreateProductRequestDto
{
    [Required(ErrorMessage = "Product code is required")]
    [StringLength(50, ErrorMessage = "Product code cannot exceed 50 characters")]
    public string ProductCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category type is required")]
    public int CategoryTypeId { get; set; }

    [Required(ErrorMessage = "Karat type is required")]
    public int KaratTypeId { get; set; }

    [Required(ErrorMessage = "Weight is required")]
    [Range(0.001, 999999.999, ErrorMessage = "Weight must be between 0.001 and 999999.999 grams")]
    public decimal Weight { get; set; }

    [StringLength(100, ErrorMessage = "Brand cannot exceed 100 characters")]
    public string? Brand { get; set; }

    [StringLength(100, ErrorMessage = "Design/Style cannot exceed 100 characters")]
    public string? DesignStyle { get; set; }

    public int? SubCategoryId { get; set; }

    [StringLength(50, ErrorMessage = "Sub-category cannot exceed 50 characters")]
    public string? SubCategory { get; set; } // Legacy field for backward compatibility

    [StringLength(50, ErrorMessage = "Shape cannot exceed 50 characters")]
    public string? Shape { get; set; }

    [StringLength(100, ErrorMessage = "Purity certificate number cannot exceed 100 characters")]
    public string? PurityCertificateNumber { get; set; }

    [StringLength(50, ErrorMessage = "Country of origin cannot exceed 50 characters")]
    public string? CountryOfOrigin { get; set; }

    [Range(1900, 2100, ErrorMessage = "Year of minting must be between 1900 and 2100")]
    public int? YearOfMinting { get; set; }

    [Range(0, 999999.99, ErrorMessage = "Face value must be between 0 and 999999.99")]
    public decimal? FaceValue { get; set; }

    public bool? HasNumismaticValue { get; set; }

    public bool MakingChargesApplicable { get; set; } = true;

    public int? ProductMakingChargesTypeId { get; set; }
    
    [Range(0, 999999.9999, ErrorMessage = "Product making charges value must be between 0 and 999999.9999")]
    public decimal? ProductMakingChargesValue { get; set; }
    
    public bool UseProductMakingCharges { get; set; } = false;

    public int? SupplierId { get; set; }
}

/// <summary>
/// Update product request DTO
/// </summary>
public class UpdateProductRequestDto : CreateProductRequestDto
{
    [Required(ErrorMessage = "Product ID is required")]
    public int Id { get; set; }
}

/// <summary>
/// Product search request DTO
/// </summary>
public class ProductSearchRequestDto
{
    public string? SearchTerm { get; set; }
    public int? CategoryTypeId { get; set; }
    public int? KaratTypeId { get; set; }
    public string? Brand { get; set; }
    public int? SubCategoryId { get; set; }
    public string? SubCategory { get; set; } // Legacy field for backward compatibility
    public int? SupplierId { get; set; }
    public bool? IsActive { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Product with inventory DTO
/// </summary>
public class ProductWithInventoryDto : ProductDto
{
    public List<ProductInventoryDto> Inventory { get; set; } = new();
    public decimal TotalQuantityOnHand { get; set; }
    public decimal TotalWeightOnHand { get; set; }
}

/// <summary>
/// Product inventory DTO
/// </summary>
public class ProductInventoryDto
{
    public int BranchId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal WeightOnHand { get; set; }
    public decimal MinimumStockLevel { get; set; }
    public bool IsLowStock { get; set; }
}

/// <summary>
/// Bulk product update request DTO
/// </summary>
public class BulkProductUpdateRequestDto
{
    public List<int> ProductIds { get; set; } = new();
    public string Action { get; set; } = string.Empty; // "activate", "deactivate", "update_supplier"
    public object? Parameters { get; set; }
}

/// <summary>
/// Product pricing info DTO
/// </summary>
public class ProductPricingDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal CurrentGoldRate { get; set; }
    public decimal EstimatedBasePrice { get; set; }
    public decimal EstimatedMakingCharges { get; set; }
    public decimal EstimatedTotalPrice { get; set; }
    public DateTime PriceCalculatedAt { get; set; }
}

/// <summary>
/// Request to decode a scanned QR payload
/// </summary>
public class DecodeQrRequestDto
{
    [Required]
    public string Payload { get; set; } = string.Empty;
}
namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Gold rate DTO for display operations
/// </summary>
public class GoldRateDto
{
    public int Id { get; set; }
    public int KaratTypeId { get; set; }
    public string KaratType { get; set; } = string.Empty;
    public decimal RatePerGram { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsCurrent { get; set; }
    public string Source { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
}

/// <summary>
/// Create gold rate request DTO
/// </summary>
public class CreateGoldRateRequestDto
{
    public int KaratTypeId { get; set; }
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string Source { get; set; } = string.Empty;
}

/// <summary>
/// Update gold rate request DTO
/// </summary>
public class UpdateGoldRateRequestDto : CreateGoldRateRequestDto
{
    public int Id { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Gold rate search request DTO
/// </summary>
public class GoldRateSearchRequestDto
{
    public int? KaratTypeId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Current gold rates DTO
/// </summary>
public class CurrentGoldRatesDto
{
    public int KaratTypeId { get; set; }
    public string KaratType { get; set; } = string.Empty;
    public decimal CurrentRate { get; set; }
    public DateTime LastUpdated { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}

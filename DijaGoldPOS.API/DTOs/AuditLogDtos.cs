namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Audit log DTO for displaying audit information
/// </summary>
public class AuditLogDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? SessionId { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? FinancialTransactionId { get; set; }
    public int? OrderId { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Details { get; set; }
    
    // Additional formatting properties
    public string TimestampFormatted { get; set; } = string.Empty;
    public bool HasError { get; set; }
    public string ActionCategory { get; set; } = string.Empty;
    public string SeverityLevel { get; set; } = string.Empty;
}

/// <summary>
/// Audit log search request DTO
/// </summary>
public class AuditLogSearchRequestDto
{
    public string? UserId { get; set; }
    public string? Action { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public int? BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? HasError { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Audit log summary DTO
/// </summary>
public class AuditLogSummaryDto
{
    public int TotalLogs { get; set; }
    public int ErrorLogs { get; set; }
    public int SuccessLogs { get; set; }
    public DateTime? LastActivity { get; set; }
    public Dictionary<string, int> ActionCounts { get; set; } = new();
    public Dictionary<string, int> UserActivityCounts { get; set; } = new();
    public List<AuditLogDto> RecentLogs { get; set; } = new();
}

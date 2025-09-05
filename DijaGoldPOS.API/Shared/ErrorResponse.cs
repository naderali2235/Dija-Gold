using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Shared;

/// <summary>
/// Standardized error response model following RFC 7807 Problem Details specification
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// A URI reference that identifies the problem type
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// A short, human-readable summary of the problem type
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The HTTP status code
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>
    /// A human-readable explanation specific to this occurrence of the problem
    /// </summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    /// <summary>
    /// A URI reference that identifies the specific occurrence of the problem
    /// </summary>
    [JsonPropertyName("instance")]
    public string? Instance { get; set; }

    /// <summary>
    /// Trace ID for correlation and debugging
    /// </summary>
    [JsonPropertyName("traceId")]
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// HTTP method that caused the error
    /// </summary>
    [JsonPropertyName("method")]
    public string? Method { get; set; }

    /// <summary>
    /// User-friendly message that can be displayed to end users
    /// </summary>
    [JsonPropertyName("userFriendlyMessage")]
    public string? UserFriendlyMessage { get; set; }

    /// <summary>
    /// Validation errors (for validation failures)
    /// </summary>
    [JsonPropertyName("validationErrors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    /// <summary>
    /// Additional error-specific extensions
    /// </summary>
    [JsonPropertyName("extensions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Extensions { get; set; }

    /// <summary>
    /// Suggested actions for the user to resolve the error
    /// </summary>
    [JsonPropertyName("suggestedActions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? SuggestedActions { get; set; }

    /// <summary>
    /// Help URL for more information about this error
    /// </summary>
    [JsonPropertyName("helpUrl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HelpUrl { get; set; }
}

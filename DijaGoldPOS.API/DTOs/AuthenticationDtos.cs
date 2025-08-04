using System.ComponentModel.DataAnnotations;

namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Login request DTO
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// Username or email
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Remember login
    /// </summary>
    public bool RememberMe { get; set; } = false;
}

/// <summary>
/// Login response DTO
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// JWT token
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token expiry date
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// User information
    /// </summary>
    public UserInfoDto User { get; set; } = new();
}

/// <summary>
/// User information DTO
/// </summary>
public class UserInfoDto
{
    /// <summary>
    /// User ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Full name
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Employee code
    /// </summary>
    public string? EmployeeCode { get; set; }

    /// <summary>
    /// User roles
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Branch information
    /// </summary>
    public BranchInfoDto? Branch { get; set; }

    /// <summary>
    /// Last login date
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// Branch information DTO
/// </summary>
public class BranchInfoDto
{
    /// <summary>
    /// Branch ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Branch name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Branch code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is headquarters
    /// </summary>
    public bool IsHeadquarters { get; set; }
}

/// <summary>
/// Change password request DTO
/// </summary>
public class ChangePasswordRequestDto
{
    /// <summary>
    /// Current password
    /// </summary>
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// New password
    /// </summary>
    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Confirm new password
    /// </summary>
    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// API response wrapper
/// </summary>
/// <typeparam name="T">Response data type</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Success status
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Response data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error details
    /// </summary>
    public object? Errors { get; set; }

    /// <summary>
    /// Create success response
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Create error response
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string message, object? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}

/// <summary>
/// API response for non-generic responses
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Create success response
    /// </summary>
    public static ApiResponse SuccessResponse(string message = "Success")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    /// <summary>
    /// Create error response
    /// </summary>
    public new static ApiResponse ErrorResponse(string message, object? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
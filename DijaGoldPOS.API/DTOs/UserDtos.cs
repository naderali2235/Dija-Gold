using System.ComponentModel.DataAnnotations;

namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// User DTO for admin operations
/// </summary>
public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public List<string> Roles { get; set; } = new();
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTime? LockoutEnd { get; set; }
}

/// <summary>
/// Create user request DTO
/// </summary>
public class CreateUserRequestDto
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Employee code cannot exceed 20 characters")]
    public string? EmployeeCode { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "At least one role is required")]
    public List<string> Roles { get; set; } = new();

    public int? BranchId { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Update user request DTO
/// </summary>
public class UpdateUserRequestDto
{
    [Required(ErrorMessage = "User ID is required")]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Employee code cannot exceed 20 characters")]
    public string? EmployeeCode { get; set; }

    public int? BranchId { get; set; }
}

/// <summary>
/// Update user role request DTO
/// </summary>
public class UpdateUserRoleRequestDto
{
    [Required(ErrorMessage = "User ID is required")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "At least one role is required")]
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// Update user status request DTO
/// </summary>
public class UpdateUserStatusRequestDto
{
    [Required(ErrorMessage = "User ID is required")]
    public string UserId { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string? Reason { get; set; }
}

/// <summary>
/// User search request DTO
/// </summary>
public class UserSearchRequestDto
{
    public string? SearchTerm { get; set; }
    public string? Role { get; set; }
    public int? BranchId { get; set; }
    public bool? IsActive { get; set; } = true;
    public bool? IsLocked { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// User activity log DTO
/// </summary>
public class UserActivityDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<UserActivityLogDto> Activities { get; set; } = new();
    public int TotalActivities { get; set; }
}

/// <summary>
/// Individual user activity log DTO
/// </summary>
public class UserActivityLogDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string IpAddress { get; set; } = string.Empty;
}

/// <summary>
/// Reset password request DTO
/// </summary>
public class ResetPasswordRequestDto
{
    [Required(ErrorMessage = "User ID is required")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string NewPassword { get; set; } = string.Empty;

    public bool ForcePasswordChange { get; set; } = true;
}

/// <summary>
/// User permissions DTO
/// </summary>
public class UserPermissionsDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public Dictionary<string, bool> FeatureAccess { get; set; } = new();
}

/// <summary>
/// Update user permissions request DTO
/// </summary>
public class UpdateUserPermissionsRequestDto
{
    [Required(ErrorMessage = "User ID is required")]
    public string UserId { get; set; } = string.Empty;

    public List<string> Permissions { get; set; } = new();
    
    public Dictionary<string, bool> FeatureAccess { get; set; } = new();
}
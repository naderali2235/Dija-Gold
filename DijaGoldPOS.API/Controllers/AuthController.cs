using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Authentication controller for login, logout, and user management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IAuditService auditService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// User login
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT token and user information</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));
            }

            var user = await _userManager.FindByNameAsync(request.Username) 
                ?? await _userManager.FindByEmailAsync(request.Username);

            if (user == null || !user.IsActive)
            {
                await _auditService.LogLoginAsync(
                    request.Username,
                    request.Username,
                    GetClientIpAddress(),
                    GetUserAgent(),
                    false,
                    "User not found or inactive"
                );

                return Unauthorized(ApiResponse.ErrorResponse("Invalid credentials"));
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                await _auditService.LogLoginAsync(
                    user.Id,
                    user.FullName,
                    GetClientIpAddress(),
                    GetUserAgent(),
                    false,
                    result.IsLockedOut ? "Account locked" : "Invalid password"
                );

                if (result.IsLockedOut)
                {
                    return Unauthorized(ApiResponse.ErrorResponse("Account is locked due to multiple failed attempts"));
                }

                return Unauthorized(ApiResponse.ErrorResponse("Invalid credentials"));
            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate JWT token
            var token = await _tokenService.GenerateTokenAsync(user, roles);

            // Update last login date
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Log successful login
            await _auditService.LogLoginAsync(
                user.Id,
                user.FullName,
                GetClientIpAddress(),
                GetUserAgent(),
                true
            );

            var response = new LoginResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(8), // Should match JWT configuration
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Username = user.UserName ?? "",
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    EmployeeCode = user.EmployeeCode,
                    Roles = roles.ToList(),
                    Branch = user.Branch != null ? new BranchInfoDto
                    {
                        Id = user.Branch.Id,
                        Name = user.Branch.Name,
                        Code = user.Branch.Code,
                        IsHeadquarters = user.Branch.IsHeadquarters
                    } : null,
                    LastLoginAt = user.LastLoginAt
                }
            };

            _logger.LogInformation("User {UserId} logged in successfully", user.Id);

            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response, "Login successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", request.Username);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred during login"));
        }
    }

    /// <summary>
    /// User logout
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "";

            if (!string.IsNullOrEmpty(userId))
            {
                await _auditService.LogLogoutAsync(
                    userId,
                    userName,
                    GetClientIpAddress(),
                    GetUserAgent()
                );

                _logger.LogInformation("User {UserId} logged out", userId);
            }

            await _signInManager.SignOutAsync();

            return Ok(ApiResponse.SuccessResponse("Logout successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred during logout"));
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="request">Password change request</param>
    /// <returns>Success message</returns>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse.ErrorResponse("User not authenticated"));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized(ApiResponse.ErrorResponse("User not found"));
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ApiResponse.ErrorResponse("Password change failed", errors));
            }

            // Log password change
            await _auditService.LogAsync(
                userId,
                "CHANGE_PASSWORD",
                "ApplicationUser",
                userId,
                "Password changed successfully"
            );

            _logger.LogInformation("User {UserId} changed password successfully", userId);

            return Ok(ApiResponse.SuccessResponse("Password changed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while changing password"));
        }
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    /// <returns>Current user details</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse.ErrorResponse("User not authenticated"));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized(ApiResponse.ErrorResponse("User not found"));
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userInfo = new UserInfoDto
            {
                Id = user.Id,
                Username = user.UserName ?? "",
                FullName = user.FullName,
                Email = user.Email ?? "",
                EmployeeCode = user.EmployeeCode,
                Roles = roles.ToList(),
                Branch = user.Branch != null ? new BranchInfoDto
                {
                    Id = user.Branch.Id,
                    Name = user.Branch.Name,
                    Code = user.Branch.Code,
                    IsHeadquarters = user.Branch.IsHeadquarters
                } : null,
                LastLoginAt = user.LastLoginAt
            };

            return Ok(ApiResponse<UserInfoDto>.SuccessResponse(userInfo));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user information");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving user information"));
        }
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    /// <returns>New JWT token</returns>
    [HttpPost("refresh-token")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse.ErrorResponse("User not authenticated"));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return Unauthorized(ApiResponse.ErrorResponse("User not found or inactive"));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = await _tokenService.GenerateTokenAsync(user, roles);

            var response = new LoginResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(8),
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Username = user.UserName ?? "",
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    EmployeeCode = user.EmployeeCode,
                    Roles = roles.ToList(),
                    Branch = user.Branch != null ? new BranchInfoDto
                    {
                        Id = user.Branch.Id,
                        Name = user.Branch.Name,
                        Code = user.Branch.Code,
                        IsHeadquarters = user.Branch.IsHeadquarters
                    } : null,
                    LastLoginAt = user.LastLoginAt
                }
            };

            _logger.LogInformation("Token refreshed for user {UserId}", userId);

            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response, "Token refreshed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while refreshing token"));
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Get client IP address
    /// </summary>
    private string? GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    /// <summary>
    /// Get user agent
    /// </summary>
    private string? GetUserAgent()
    {
        return HttpContext.Request.Headers["User-Agent"].ToString();
    }

    #endregion
}
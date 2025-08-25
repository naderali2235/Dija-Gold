using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Services;
using DijaGoldPOS.API.Shared;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Users controller for enhanced user administration
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ManagerOnly")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<UsersController> _logger;
    private readonly IMapper _mapper;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
        IAuditService auditService,
        ILogger<UsersController> logger,
        IMapper mapper)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _auditService = auditService;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all users with optional filtering and pagination
    /// </summary>
    /// <param name="searchRequest">Search parameters</param>
    /// <returns>List of users</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] UserSearchRequestDto searchRequest)
    {
        try
        {
            var query = _context.Users.Include(u => u.Branch).AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchRequest.SearchTerm))
            {
                var searchTerm = searchRequest.SearchTerm.ToLower();
                query = query.Where(u => 
                    u.UserName!.ToLower().Contains(searchTerm) ||
                    u.FullName.ToLower().Contains(searchTerm) ||
                    u.Email!.ToLower().Contains(searchTerm) ||
                    (u.EmployeeCode != null && u.EmployeeCode.ToLower().Contains(searchTerm))
                );
            }

            if (searchRequest.BranchId.HasValue)
            {
                query = query.Where(u => u.BranchId == searchRequest.BranchId.Value);
            }

            if (searchRequest.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == searchRequest.IsActive.Value);
            }

            if (searchRequest.IsLocked.HasValue)
            {
                if (searchRequest.IsLocked.Value)
                {
                    query = query.Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTime.UtcNow);
                }
                else
                {
                    query = query.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTime.UtcNow);
                }
            }

            // Apply sorting
            query = query.OrderBy(u => u.FullName);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var users = await query
                .Skip((searchRequest.PageNumber - 1) * searchRequest.PageSize)
                .Take(searchRequest.PageSize)
                .ToListAsync();

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    EmployeeCode = user.EmployeeCode,
                    Roles = roles.ToList(),
                    BranchId = user.BranchId,
                    BranchName = user.Branch?.Name,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    EmailConfirmed = user.EmailConfirmed,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd?.DateTime
                });
            }

            // Filter by role if specified
            if (!string.IsNullOrWhiteSpace(searchRequest.Role))
            {
                userDtos = userDtos.Where(u => u.Roles.Contains(searchRequest.Role)).ToList();
                totalCount = userDtos.Count; // Recalculate total for role filtering
            }

            var result = new PagedResult<UserDto>
            {
                Items = userDtos,
                TotalCount = totalCount,
                PageNumber = searchRequest.PageNumber,
                PageSize = searchRequest.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / searchRequest.PageSize)
            };

            return Ok(ApiResponse<PagedResult<UserDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving users"));
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(string id)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResponse("User not found"));
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                FullName = user.FullName,
                Email = user.Email ?? "",
                EmployeeCode = user.EmployeeCode,
                Roles = roles.ToList(),
                BranchId = user.BranchId,
                BranchName = user.Branch?.Name,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd?.DateTime
            };

            return Ok(ApiResponse<UserDto>.SuccessResponse(userDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the user"));
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="request">User creation request</param>
    /// <returns>Created user</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
    {
        try
        {
            // Check if username already exists
            var existingUser = await _userManager.FindByNameAsync(request.UserName);
            if (existingUser != null)
            {
                return BadRequest(ApiResponse.ErrorResponse("A user with this username already exists"));
            }

            // Check if email already exists
            existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(ApiResponse.ErrorResponse("A user with this email already exists"));
            }

            // Check if employee code already exists
            if (!string.IsNullOrWhiteSpace(request.EmployeeCode))
            {
                existingUser = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeCode == request.EmployeeCode);
                if (existingUser != null)
                {
                    return BadRequest(ApiResponse.ErrorResponse("A user with this employee code already exists"));
                }
            }

            // Validate branch exists if specified
            if (request.BranchId.HasValue)
            {
                var branchExists = await _context.Branches.AnyAsync(b => b.Id == request.BranchId.Value && b.IsActive);
                if (!branchExists)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Specified branch does not exist or is inactive"));
                }
            }

            // Validate roles exist
            foreach (var roleName in request.Roles)
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    return BadRequest(ApiResponse.ErrorResponse($"Role '{roleName}' does not exist"));
                }
            }

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                FullName = request.FullName,
                Email = request.Email,
                EmployeeCode = request.EmployeeCode,
                BranchId = request.BranchId,
                IsActive = request.IsActive,
                EmailConfirmed = true, // Auto-confirm for admin-created users
                LockoutEnabled = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(ApiResponse.ErrorResponse($"Failed to create user: {errors}"));
            }

            // Add roles
            var roleResult = await _userManager.AddToRolesAsync(user, request.Roles);
            if (!roleResult.Succeeded)
            {
                // If role assignment fails, delete the user
                await _userManager.DeleteAsync(user);
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                return BadRequest(ApiResponse.ErrorResponse($"Failed to assign roles: {errors}"));
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                currentUserId,
                "CREATE",
                "User",
                user.Id,
                $"Created user: {user.FullName} ({user.UserName}) with roles: {string.Join(", ", request.Roles)}"
            );

            // Reload user with branch info
            var createdUser = await _context.Users
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                FullName = user.FullName,
                Email = user.Email ?? "",
                EmployeeCode = user.EmployeeCode,
                Roles = request.Roles,
                BranchId = user.BranchId,
                BranchName = createdUser?.Branch?.Name,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd?.DateTime
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, 
                ApiResponse<UserDto>.SuccessResponse(userDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while creating the user"));
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">User update request</param>
    /// <returns>Updated user</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequestDto request)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResponse("User not found"));
            }

            // Check if email already exists (excluding current user)
            if (request.Email != user.Email)
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null && existingUser.Id != id)
                {
                    return BadRequest(ApiResponse.ErrorResponse("A user with this email already exists"));
                }
            }

            // Check if employee code already exists (excluding current user)
            if (request.EmployeeCode != user.EmployeeCode && !string.IsNullOrWhiteSpace(request.EmployeeCode))
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeCode == request.EmployeeCode && u.Id != id);
                if (existingUser != null)
                {
                    return BadRequest(ApiResponse.ErrorResponse("A user with this employee code already exists"));
                }
            }

            // Validate branch exists if specified
            if (request.BranchId.HasValue)
            {
                var branchExists = await _context.Branches.AnyAsync(b => b.Id == request.BranchId.Value && b.IsActive);
                if (!branchExists)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Specified branch does not exist or is inactive"));
                }
            }

            // Update user properties
            user.FullName = request.FullName;
            user.Email = request.Email;
            user.EmployeeCode = request.EmployeeCode;
            user.BranchId = request.BranchId;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(ApiResponse.ErrorResponse($"Failed to update user: {errors}"));
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                currentUserId,
                "UPDATE",
                "User",
                user.Id,
                $"Updated user: {user.FullName} ({user.UserName})"
            );

            // Reload user with branch info and roles
            var updatedUser = await _context.Users
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Id == id);

            var roles = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                FullName = user.FullName,
                Email = user.Email ?? "",
                EmployeeCode = user.EmployeeCode,
                Roles = roles.ToList(),
                BranchId = user.BranchId,
                BranchName = updatedUser?.Branch?.Name,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd?.DateTime
            };

            return Ok(ApiResponse<UserDto>.SuccessResponse(userDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID {UserId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating the user"));
        }
    }

    /// <summary>
    /// Update user roles
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Role update request</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}/roles")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserRoles(string id, [FromBody] UpdateUserRoleRequestDto request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResponse("User not found"));
            }

            // Validate roles exist
            foreach (var roleName in request.Roles)
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    return BadRequest(ApiResponse.ErrorResponse($"Role '{roleName}' does not exist"));
                }
            }

            // Get current roles
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove from current roles
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                    return BadRequest(ApiResponse.ErrorResponse($"Failed to remove current roles: {errors}"));
                }
            }

            // Add new roles
            if (request.Roles.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, request.Roles);
                if (!addResult.Succeeded)
                {
                    var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    return BadRequest(ApiResponse.ErrorResponse($"Failed to assign new roles: {errors}"));
                }
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                currentUserId,
                "UPDATE",
                "UserRoles",
                user.Id,
                $"Updated roles for user: {user.FullName} ({user.UserName}) - Old roles: [{string.Join(", ", currentRoles)}], New roles: [{string.Join(", ", request.Roles)}]"
            );

            return Ok(ApiResponse.SuccessResponse("User roles updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating roles for user with ID {UserId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating user roles"));
        }
    }

    /// <summary>
    /// Update user status (enable/disable)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Status update request</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserStatus(string id, [FromBody] UpdateUserStatusRequestDto request)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResponse("User not found"));
            }

            // Prevent disabling self
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            if (id == currentUserId && !request.IsActive)
            {
                return BadRequest(ApiResponse.ErrorResponse("You cannot disable your own account"));
            }

            user.IsActive = request.IsActive;

            // If disabling user, also lock them out
            if (!request.IsActive)
            {
                user.LockoutEnd = DateTime.UtcNow.AddYears(100); // Effective permanent lockout
            }
            else
            {
                user.LockoutEnd = null; // Remove lockout
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                "UPDATE",
                "UserStatus",
                user.Id,
                $"Updated status for user: {user.FullName} ({user.UserName}) - Status: {(request.IsActive ? "Active" : "Inactive")}" +
                (string.IsNullOrWhiteSpace(request.Reason) ? "" : $", Reason: {request.Reason}")
            );

            return Ok(ApiResponse.SuccessResponse($"User status updated to {(request.IsActive ? "active" : "inactive")} successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for user with ID {UserId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating user status"));
        }
    }

    /// <summary>
    /// Get user activity log
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="fromDate">From date (optional)</param>
    /// <param name="toDate">To date (optional)</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>User activity log</returns>
    [HttpGet("{id}/activity")]
    [ProducesResponseType(typeof(ApiResponse<UserActivityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserActivity(
        string id,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResponse("User not found"));
            }

            var query = _context.AuditLogs
                .Include(a => a.Branch)
                .Where(a => a.UserId == id);

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= toDate.Value);
            }

            query = query.OrderByDescending(a => a.Timestamp);

            var totalCount = await query.CountAsync();

            var activities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new UserActivityLogDto
                {
                    Id = (int)a.Id,
                    Timestamp = a.Timestamp,
                    Action = a.Action,
                    EntityType = a.EntityType ?? string.Empty,
                    EntityId = a.EntityId ?? string.Empty,
                    Details = a.Description,
                    BranchName = a.Branch != null ? a.Branch.Name : null,
                    IpAddress = a.IpAddress ?? ""
                })
                .ToListAsync();

            var result = new UserActivityDto
            {
                UserId = user.Id,
                UserName = user.UserName ?? "",
                FullName = user.FullName,
                Activities = activities,
                TotalActivities = totalCount
            };

            return Ok(ApiResponse<UserActivityDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activity log for user with ID {UserId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving user activity"));
        }
    }

    /// <summary>
    /// Reset user password
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Password reset request</param>
    /// <returns>Success response</returns>
    [HttpPost("{id}/reset-password")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResponse("User not found"));
            }

            // Generate password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Reset password
            var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(ApiResponse.ErrorResponse($"Failed to reset password: {errors}"));
            }

            // Force password change if requested
            if (request.ForcePasswordChange)
            {
                // You would implement a mechanism to force password change on next login
                // This could be a custom property or claim
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                currentUserId,
                "UPDATE",
                "UserPassword",
                user.Id,
                $"Reset password for user: {user.FullName} ({user.UserName})" +
                (request.ForcePasswordChange ? " - Force change on next login" : "")
            );

            return Ok(ApiResponse.SuccessResponse("Password reset successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user with ID {UserId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while resetting password"));
        }
    }

    /// <summary>
    /// Get user permissions
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User permissions</returns>
    [HttpGet("{id}/permissions")]
    [ProducesResponseType(typeof(ApiResponse<UserPermissionsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserPermissions(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResponse("User not found"));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            // Extract permissions from claims
            var permissions = claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();

            // Create feature access dictionary based on roles
            var featureAccess = new Dictionary<string, bool>
            {
                { "CanViewReports", roles.Contains("Manager") || roles.Contains("Admin") },
                { "CanManageInventory", roles.Contains("Manager") || roles.Contains("Admin") },
                { "CanProcessRefunds", roles.Contains("Manager") || roles.Contains("Admin") },
                { "CanManageUsers", roles.Contains("Admin") },
                { "CanAccessAuditLogs", roles.Contains("Manager") || roles.Contains("Admin") },
                { "CanManagePricing", roles.Contains("Manager") || roles.Contains("Admin") },
                { "CanProcessSales", roles.Contains("Cashier") || roles.Contains("Manager") || roles.Contains("Admin") }
            };

            var permissionsDto = new UserPermissionsDto
            {
                UserId = user.Id,
                UserName = user.UserName ?? "",
                FullName = user.FullName,
                Roles = roles.ToList(),
                Permissions = permissions,
                FeatureAccess = featureAccess
            };

            return Ok(ApiResponse<UserPermissionsDto>.SuccessResponse(permissionsDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions for user with ID {UserId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving user permissions"));
        }
    }

    /// <summary>
    /// Update user permissions
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Permissions update request</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}/permissions")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserPermissions(string id, [FromBody] UpdateUserPermissionsRequestDto request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResponse("User not found"));
            }

            // Get current permission claims
            var currentClaims = await _userManager.GetClaimsAsync(user);
            var currentPermissions = currentClaims.Where(c => c.Type == "permission").ToList();

            // Remove current permission claims
            if (currentPermissions.Any())
            {
                var removeResult = await _userManager.RemoveClaimsAsync(user, currentPermissions);
                if (!removeResult.Succeeded)
                {
                    var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                    return BadRequest(ApiResponse.ErrorResponse($"Failed to remove current permissions: {errors}"));
                }
            }

            // Add new permission claims
            var newClaims = request.Permissions.Select(p => new Claim("permission", p)).ToList();
            
            // Add feature access claims
            foreach (var feature in request.FeatureAccess)
            {
                newClaims.Add(new Claim($"feature:{feature.Key}", feature.Value.ToString()));
            }

            if (newClaims.Any())
            {
                var addResult = await _userManager.AddClaimsAsync(user, newClaims);
                if (!addResult.Succeeded)
                {
                    var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    return BadRequest(ApiResponse.ErrorResponse($"Failed to add new permissions: {errors}"));
                }
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                currentUserId,
                "UPDATE",
                "UserPermissions",
                user.Id,
                $"Updated permissions for user: {user.FullName} ({user.UserName}) - Permissions: [{string.Join(", ", request.Permissions)}]"
            );

            return Ok(ApiResponse.SuccessResponse("User permissions updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating permissions for user with ID {UserId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating user permissions"));
        }
    }
}

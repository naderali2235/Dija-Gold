using DijaGoldPOS.API.Models.Shared;

namespace DijaGoldPOS.API.IServices;

/// <summary>
/// Interface for JWT token service
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generate JWT token for user
    /// </summary>
    /// <param name="user">Application user</param>
    /// <param name="roles">User roles</param>
    /// <returns>JWT token string</returns>
    Task<string> GenerateTokenAsync(ApplicationUser user, IList<string> roles);
    
    /// <summary>
    /// Validate JWT token
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool ValidateToken(string token);
    
    /// <summary>
    /// Get user ID from JWT token
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>User ID if token is valid, null otherwise</returns>
    string? GetUserIdFromToken(string token);
}

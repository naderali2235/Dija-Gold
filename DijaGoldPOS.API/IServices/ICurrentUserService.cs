namespace DijaGoldPOS.API.IServices;

/// <summary>
/// Provides information about the current authenticated user.
/// Falls back to system defaults when no user context is available.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's unique identifier, or "system" if unavailable.
    /// </summary>
    string UserId { get; }

    /// <summary>
    /// Gets the current user's username, or "system" if unavailable.
    /// </summary>
    string UserName { get; }

    /// <summary>
    /// Gets the current user's branch ID, or null if unavailable.
    /// </summary>
    int? BranchId { get; }

    /// <summary>
    /// Gets the current user's branch name, or null if unavailable.
    /// </summary>
    string? BranchName { get; }
}



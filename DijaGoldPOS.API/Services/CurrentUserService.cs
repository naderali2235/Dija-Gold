using DijaGoldPOS.API.IServices;
using System.Security.Claims;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Default implementation of <see cref="ICurrentUserService"/> using <see cref="IHttpContextAccessor"/>.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                return httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
            }
            return "system";
        }
    }

    public string UserName
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                return httpContext.User.Identity?.Name ?? "system";
            }
            return "system";
        }
    }

    public int? BranchId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var branchIdClaim = httpContext.User.FindFirstValue("BranchId");
                if (int.TryParse(branchIdClaim, out int branchId))
                {
                    return branchId;
                }
            }
            return null;
        }
    }

    public string? BranchName
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                return httpContext.User.FindFirstValue("BranchName");
            }
            return null;
        }
    }
}



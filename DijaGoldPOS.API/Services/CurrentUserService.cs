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
}



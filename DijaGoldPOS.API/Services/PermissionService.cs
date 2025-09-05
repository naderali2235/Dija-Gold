using DijaGoldPOS.API.IServices;

namespace DijaGoldPOS.API.Services;

public class PermissionService : IPermissionService
{
    public async Task<bool> HasPermissionAsync(string userId, string permission)
    {
        // Stub implementation
        return await Task.FromResult(true);
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userId)
    {
        // Stub implementation
        return await Task.FromResult(new List<string>());
    }
}

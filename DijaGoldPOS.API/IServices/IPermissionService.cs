namespace DijaGoldPOS.API.IServices;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string userId, string permission);
    Task<IEnumerable<string>> GetUserPermissionsAsync(string userId);
}

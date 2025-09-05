namespace DijaGoldPOS.API.IServices;

public interface IRoleService
{
    Task<IEnumerable<string>> GetAllRolesAsync();
    Task<bool> RoleExistsAsync(string roleName);
    Task<bool> CreateRoleAsync(string roleName);
}

using DijaGoldPOS.API.IServices;
using Microsoft.AspNetCore.Identity;

namespace DijaGoldPOS.API.Services;

public class RoleService : IRoleService
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleService(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<IEnumerable<string>> GetAllRolesAsync()
    {
        return _roleManager.Roles.Select(r => r.Name!).ToList();
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await _roleManager.RoleExistsAsync(roleName);
    }

    public async Task<bool> CreateRoleAsync(string roleName)
    {
        var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
        return result.Succeeded;
    }
}

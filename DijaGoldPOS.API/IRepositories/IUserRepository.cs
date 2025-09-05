using DijaGoldPOS.API.Models.Shared;

namespace DijaGoldPOS.API.IRepositories;

/// <summary>
/// Repository interface for User operations
/// </summary>
public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(string id);
    Task<IEnumerable<ApplicationUser>> GetAllAsync();
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<ApplicationUser?> GetByEmployeeCodeAsync(string employeeCode);
    Task<IEnumerable<ApplicationUser>> GetByBranchIdAsync(int branchId);
    Task<IEnumerable<ApplicationUser>> GetByRoleAsync(string roleName);
    Task<bool> ExistsAsync(string email);
    Task<bool> IsEmployeeCodeUniqueAsync(string employeeCode, string? excludeUserId = null);
}

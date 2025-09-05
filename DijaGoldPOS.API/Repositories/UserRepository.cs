using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models.Shared;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository implementation for User operations
/// </summary>
public class UserRepository : IUserRepository
{
    protected readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUser?> GetByIdAsync(string id)
    {
        return await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Branch)
            .Where(u => u.IsActive)
            .ToListAsync();
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
    }

    public async Task<ApplicationUser?> GetByEmployeeCodeAsync(string employeeCode)
    {
        return await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.EmployeeCode == employeeCode && u.IsActive);
    }

    public async Task<IEnumerable<ApplicationUser>> GetByBranchIdAsync(int branchId)
    {
        return await _context.Users
            .Include(u => u.Branch)
            .Where(u => u.BranchId == branchId && u.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApplicationUser>> GetByRoleAsync(string roleName)
    {
        return await _context.Users
            .Include(u => u.Branch)
            .Where(u => u.UserRoles.Any(ur => ur.RoleId == _context.Roles.Where(r => r.Name == roleName).Select(r => r.Id).FirstOrDefault()) && u.IsActive)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email);
    }

    public async Task<bool> IsEmployeeCodeUniqueAsync(string employeeCode, string? excludeUserId = null)
    {
        var query = _context.Users
            .Where(u => u.EmployeeCode == employeeCode);

        if (!string.IsNullOrEmpty(excludeUserId))
        {
            query = query.Where(u => u.Id != excludeUserId);
        }

        return !await query.AnyAsync();
    }
}

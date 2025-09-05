using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models.Shared;

namespace DijaGoldPOS.API.IServices;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<IEnumerable<UserDto>> GetUsersByBranchAsync(int branchId);
    Task<UserDto> CreateUserAsync(CreateUserRequestDto request);
    Task<UserDto> UpdateUserAsync(string userId, UpdateUserRequestDto request);
    Task<bool> DeleteUserAsync(string userId);
    Task<bool> UpdateUserStatusAsync(string userId, bool isActive);
    Task<IEnumerable<UserDto>> SearchUsersAsync(UserSearchRequestDto request);
}

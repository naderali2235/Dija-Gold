using DijaGoldPOS.API.DTOs;

namespace DijaGoldPOS.API.IServices;

public interface IBranchService
{
    Task<IEnumerable<BranchDto>> GetAllBranchesAsync();
    Task<BranchDto?> GetBranchByIdAsync(int id);
    Task<BranchDto> CreateBranchAsync(CreateBranchRequestDto request);
    Task<BranchDto> UpdateBranchAsync(int id, UpdateBranchRequestDto request);
    Task<bool> DeleteBranchAsync(int id);
}

using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Models.BranchModels;

namespace DijaGoldPOS.API.Services;

public class BranchService : IBranchService
{
	private readonly IUnitOfWork unitOfWork;
	private readonly IMapper _mapper;

    public BranchService(IUnitOfWork unitOfWork, IMapper mapper)
    {
		this.unitOfWork = unitOfWork;
		_mapper = mapper;
    }

    public async Task<IEnumerable<BranchDto>> GetAllBranchesAsync()
    {
        var branches = await unitOfWork.Branches.GetAllAsync();
        return _mapper.Map<IEnumerable<BranchDto>>(branches);
    }

    public async Task<BranchDto?> GetBranchByIdAsync(int id)
    {
        var branch = await unitOfWork.Branches.GetByIdAsync(id);
        return branch != null ? _mapper.Map<BranchDto>(branch) : null;
    }

    public async Task<BranchDto> CreateBranchAsync(CreateBranchRequestDto request)
    {
        var branch = _mapper.Map<Branch>(request);
        await unitOfWork.Branches.AddAsync(branch);
        await unitOfWork.SaveChangesAsync();
        return _mapper.Map<BranchDto>(branch);
    }

    public async Task<BranchDto> UpdateBranchAsync(int id, UpdateBranchRequestDto request)
    {
        var branch = await unitOfWork.Branches.GetByIdAsync(id);
        if (branch == null) throw new ArgumentException("Branch not found", nameof(id));
        
        _mapper.Map(request, branch);
        await unitOfWork.Branches.UpdateAsync(branch);
        await unitOfWork.SaveChangesAsync();
        return _mapper.Map<BranchDto>(branch);
    }

    public async Task<bool> DeleteBranchAsync(int id)
    {
        var branch = await unitOfWork.Branches.GetByIdAsync(id);
        if (branch == null) return false;
        
        await unitOfWork.Branches.DeleteAsync(branch.Id);
        await unitOfWork.SaveChangesAsync();
        return true;
    }
}

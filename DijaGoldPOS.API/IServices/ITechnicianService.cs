using DijaGoldPOS.API.DTOs;

namespace DijaGoldPOS.API.IServices;

/// <summary>
/// Service interface for technician operations
/// </summary>
public interface ITechnicianService
{
    /// <summary>
    /// Create a new technician
    /// </summary>
    Task<(bool IsSuccess, string? ErrorMessage, TechnicianDto? Technician)> CreateTechnicianAsync(CreateTechnicianRequestDto request, string userId);

    /// <summary>
    /// Get technician by ID
    /// </summary>
    Task<TechnicianDto?> GetTechnicianAsync(int technicianId);

    /// <summary>
    /// Update technician
    /// </summary>
    Task<(bool IsSuccess, string? ErrorMessage)> UpdateTechnicianAsync(int technicianId, UpdateTechnicianRequestDto request, string userId);

    /// <summary>
    /// Delete technician (soft delete by setting IsActive to false)
    /// </summary>
    Task<(bool IsSuccess, string? ErrorMessage)> DeleteTechnicianAsync(int technicianId, string userId);

    /// <summary>
    /// Search technicians
    /// </summary>
    Task<(List<TechnicianDto> Items, int TotalCount)> SearchTechniciansAsync(TechnicianSearchRequestDto request);

    /// <summary>
    /// Get all active technicians
    /// </summary>
    Task<List<TechnicianDto>> GetActiveTechniciansAsync(int? branchId = null);

    /// <summary>
    /// Get technicians by branch
    /// </summary>
    Task<List<TechnicianDto>> GetTechniciansByBranchAsync(int branchId);
}

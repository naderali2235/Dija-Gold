using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Models;

using DijaGoldPOS.API.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service for technician operations
/// </summary>
public class TechnicianService : ITechnicianService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<TechnicianService> _logger;

    public TechnicianService(
        ApplicationDbContext context,
        IAuditService auditService,
        ILogger<TechnicianService> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new technician
    /// </summary>
    public async Task<(bool IsSuccess, string? ErrorMessage, TechnicianDto? Technician)> CreateTechnicianAsync(CreateTechnicianRequestDto request, string userId)
    {
        try
        {
            // Check if technician with same phone number already exists
            var existingTechnician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.PhoneNumber == request.PhoneNumber && t.IsActive);

            if (existingTechnician != null)
            {
                return (false, "A technician with this phone number already exists", null);
            }

            var technician = new Technician
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Specialization = request.Specialization,
                BranchId = request.BranchId,
                IsActive = true,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Technicians.AddAsync(technician);
            await _context.SaveChangesAsync();

            // Log audit
            await _auditService.LogAsync(
                userId,
                "CREATE_TECHNICIAN",
                "Technician",
                technician.Id.ToString(),
                $"Created technician: {technician.FullName}",
                newValues: System.Text.Json.JsonSerializer.Serialize(new { 
                    FullName = technician.FullName,
                    PhoneNumber = technician.PhoneNumber,
                    BranchId = technician.BranchId
                }),
                branchId: technician.BranchId
            );

            var technicianDto = await MapToTechnicianDtoAsync(technician);

            _logger.LogInformation("Technician {TechnicianId} created by user {UserId}", 
                technician.Id, userId);

            return (true, null, technicianDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating technician");
            return (false, "An error occurred while creating the technician", null);
        }
    }

    /// <summary>
    /// Get technician by ID
    /// </summary>
    public async Task<TechnicianDto?> GetTechnicianAsync(int technicianId)
    {
        try
        {
            var technician = await _context.Technicians
                .Include(t => t.Branch)
                .FirstOrDefaultAsync(t => t.Id == technicianId);

            if (technician == null)
            {
                return null;
            }

            return await MapToTechnicianDtoAsync(technician);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving technician {TechnicianId}", technicianId);
            return null;
        }
    }

    /// <summary>
    /// Update technician
    /// </summary>
    public async Task<(bool IsSuccess, string? ErrorMessage)> UpdateTechnicianAsync(int technicianId, UpdateTechnicianRequestDto request, string userId)
    {
        try
        {
            var technician = await _context.Technicians.FindAsync(technicianId);

            if (technician == null)
            {
                return (false, "Technician not found");
            }

            // Check if phone number is being changed and if it conflicts with another technician
            if (technician.PhoneNumber != request.PhoneNumber)
            {
                var existingTechnician = await _context.Technicians
                    .FirstOrDefaultAsync(t => t.PhoneNumber == request.PhoneNumber && t.Id != technicianId && t.IsActive);

                if (existingTechnician != null)
                {
                    return (false, "A technician with this phone number already exists");
                }
            }

            var oldValues = new { 
                FullName = technician.FullName,
                PhoneNumber = technician.PhoneNumber,
                Email = technician.Email,
                Specialization = technician.Specialization,
                IsActive = technician.IsActive,
                BranchId = technician.BranchId
            };

            technician.FullName = request.FullName;
            technician.PhoneNumber = request.PhoneNumber;
            technician.Email = request.Email;
            technician.Specialization = request.Specialization;
            technician.IsActive = request.IsActive;
            technician.BranchId = request.BranchId;
            technician.ModifiedBy = userId;
            technician.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log audit
            await _auditService.LogAsync(
                userId,
                "UPDATE_TECHNICIAN",
                "Technician",
                technician.Id.ToString(),
                $"Updated technician: {technician.FullName}",
                oldValues: System.Text.Json.JsonSerializer.Serialize(oldValues),
                newValues: System.Text.Json.JsonSerializer.Serialize(new { 
                    FullName = technician.FullName,
                    PhoneNumber = technician.PhoneNumber,
                    Email = technician.Email,
                    Specialization = technician.Specialization,
                    IsActive = technician.IsActive,
                    BranchId = technician.BranchId
                }),
                branchId: technician.BranchId
            );

            _logger.LogInformation("Technician {TechnicianId} updated by user {UserId}", 
                technician.Id, userId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating technician {TechnicianId}", technicianId);
            return (false, "An error occurred while updating the technician");
        }
    }

    /// <summary>
    /// Delete technician (soft delete by setting IsActive to false)
    /// </summary>
    public async Task<(bool IsSuccess, string? ErrorMessage)> DeleteTechnicianAsync(int technicianId, string userId)
    {
        try
        {
            var technician = await _context.Technicians.FindAsync(technicianId);

            if (technician == null)
            {
                return (false, "Technician not found");
            }

            // Check if technician has active repair jobs
            var activeRepairJobs = await _context.RepairJobs
                .AnyAsync(rj => rj.AssignedTechnicianId == technicianId && 
                               (rj.StatusId == LookupTableConstants.RepairStatusPending || rj.StatusId == LookupTableConstants.RepairStatusInProgress));

            if (activeRepairJobs)
            {
                return (false, "Cannot delete technician with active repair jobs");
            }

            technician.IsActive = false;
            technician.ModifiedBy = userId;
            technician.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log audit
            await _auditService.LogAsync(
                userId,
                "DELETE_TECHNICIAN",
                "Technician",
                technician.Id.ToString(),
                $"Deleted technician: {technician.FullName}",
                oldValues: System.Text.Json.JsonSerializer.Serialize(new { 
                    FullName = technician.FullName,
                    IsActive = true
                }),
                newValues: System.Text.Json.JsonSerializer.Serialize(new { 
                    FullName = technician.FullName,
                    IsActive = false
                }),
                branchId: technician.BranchId
            );

            _logger.LogInformation("Technician {TechnicianId} deleted by user {UserId}", 
                technician.Id, userId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting technician {TechnicianId}", technicianId);
            return (false, "An error occurred while deleting the technician");
        }
    }

    /// <summary>
    /// Search technicians
    /// </summary>
    public async Task<(List<TechnicianDto> Items, int TotalCount)> SearchTechniciansAsync(TechnicianSearchRequestDto request)
    {
        try
        {
            var query = _context.Technicians
                .Include(t => t.Branch)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(t => 
                    t.FullName.ToLower().Contains(searchTerm) ||
                    t.PhoneNumber.Contains(searchTerm) ||
                    (t.Email != null && t.Email.ToLower().Contains(searchTerm)) ||
                    (t.Specialization != null && t.Specialization.ToLower().Contains(searchTerm))
                );
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(t => t.IsActive == request.IsActive.Value);
            }

            if (request.BranchId.HasValue)
            {
                query = query.Where(t => t.BranchId == request.BranchId.Value);
            }

            var totalCount = await query.CountAsync();

            var technicians = await query
                .OrderBy(t => t.FullName)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var technicianDtos = new List<TechnicianDto>();
            foreach (var technician in technicians)
            {
                technicianDtos.Add(await MapToTechnicianDtoAsync(technician));
            }

            return (technicianDtos, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching technicians");
            return (new List<TechnicianDto>(), 0);
        }
    }

    /// <summary>
    /// Get all active technicians
    /// </summary>
    public async Task<List<TechnicianDto>> GetActiveTechniciansAsync(int? branchId = null)
    {
        try
        {
            var query = _context.Technicians
                .Include(t => t.Branch)
                .Where(t => t.IsActive)
                .AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(t => t.BranchId == branchId.Value);
            }

            var technicians = await query
                .OrderBy(t => t.FullName)
                .ToListAsync();

            var technicianDtos = new List<TechnicianDto>();
            foreach (var technician in technicians)
            {
                technicianDtos.Add(await MapToTechnicianDtoAsync(technician));
            }

            return technicianDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active technicians");
            return new List<TechnicianDto>();
        }
    }

    /// <summary>
    /// Get technicians by branch
    /// </summary>
    public async Task<List<TechnicianDto>> GetTechniciansByBranchAsync(int branchId)
    {
        try
        {
            var technicians = await _context.Technicians
                .Include(t => t.Branch)
                .Where(t => t.BranchId == branchId && t.IsActive)
                .OrderBy(t => t.FullName)
                .ToListAsync();

            var technicianDtos = new List<TechnicianDto>();
            foreach (var technician in technicians)
            {
                technicianDtos.Add(await MapToTechnicianDtoAsync(technician));
            }

            return technicianDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving technicians for branch {BranchId}", branchId);
            return new List<TechnicianDto>();
        }
    }

    /// <summary>
    /// Map technician entity to DTO
    /// </summary>
    private async Task<TechnicianDto> MapToTechnicianDtoAsync(Technician technician)
    {
        return new TechnicianDto
        {
            Id = technician.Id,
            FullName = technician.FullName,
            PhoneNumber = technician.PhoneNumber,
            Email = technician.Email,
            Specialization = technician.Specialization,
            IsActive = technician.IsActive,
            BranchId = technician.BranchId,
            BranchName = technician.Branch?.Name,
            CreatedAt = technician.CreatedAt,
            CreatedBy = technician.CreatedBy,
            CreatedByName = await GetUserNameAsync(technician.CreatedBy)
        };
    }

    /// <summary>
    /// Get user name by user ID
    /// </summary>
    private async Task<string> GetUserNameAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user?.FullName ?? userId;
    }
}

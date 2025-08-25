

namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Technician DTO for display operations
/// </summary>
public class TechnicianDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Specialization { get; set; }
    public bool IsActive { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
}

/// <summary>
/// Create technician request DTO
/// </summary>
public class CreateTechnicianRequestDto
{


    public string FullName { get; set; } = string.Empty;



    public string PhoneNumber { get; set; } = string.Empty;

    public string? Email { get; set; }


    public string? Specialization { get; set; }

    public int? BranchId { get; set; }
}

/// <summary>
/// Update technician request DTO
/// </summary>
public class UpdateTechnicianRequestDto
{
    public int Id { get; set; }


    public string FullName { get; set; } = string.Empty;



    public string PhoneNumber { get; set; } = string.Empty;

    public string? Email { get; set; }


    public string? Specialization { get; set; }

    public bool IsActive { get; set; } = true;
    public int? BranchId { get; set; }
}

/// <summary>
/// Technician search request DTO
/// </summary>
public class TechnicianSearchRequestDto
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public int? BranchId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

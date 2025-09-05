using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models.BranchModels;
using DijaGoldPOS.API.Models.Shared;

namespace DijaGoldPOS.API.Mappings;

/// <summary>
/// AutoMapper profile for Core module entities
/// </summary>
public class CoreMappingProfile : Profile
{
    public CoreMappingProfile()
    {
        ConfigureBranchMappings();
        ConfigureUserMappings();
        ConfigureAuditLogMappings();
    }

    private void ConfigureBranchMappings()
    {
        // Branch mappings
        CreateMap<Branch, BranchDto>()
            .ForMember(dest => dest.UserCount, opt => opt.MapFrom(src => src.Users.Count))
            .ForMember(dest => dest.LastActivity, opt => opt.MapFrom(src => src.ModifiedAt ?? src.CreatedAt));

        CreateMap<CreateBranchRequestDto, Branch>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Users, opt => opt.Ignore())
            .ForMember(dest => dest.FinancialTransactions, opt => opt.Ignore())
            .ForMember(dest => dest.Orders, opt => opt.Ignore())
            .ForMember(dest => dest.CashDrawerBalances, opt => opt.Ignore())
            .ForMember(dest => dest.ProductOwnerships, opt => opt.Ignore())
            .ForMember(dest => dest.InventoryItems, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerPurchases, opt => opt.Ignore());

        CreateMap<UpdateBranchRequestDto, Branch>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Users, opt => opt.Ignore())
            .ForMember(dest => dest.FinancialTransactions, opt => opt.Ignore())
            .ForMember(dest => dest.Orders, opt => opt.Ignore())
            .ForMember(dest => dest.CashDrawerBalances, opt => opt.Ignore())
            .ForMember(dest => dest.ProductOwnerships, opt => opt.Ignore())
            .ForMember(dest => dest.InventoryItems, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerPurchases, opt => opt.Ignore());
    }

    private void ConfigureUserMappings()
    {
        // ApplicationUser mappings
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : null))
            .ForMember(dest => dest.RoleNames, opt => opt.Ignore()) // Will be populated separately
            .ForMember(dest => dest.LastLoginFormatted, opt => opt.MapFrom(src => src.LastLoginAt.HasValue ? src.LastLoginAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null))
            .ForMember(dest => dest.IsOnline, opt => opt.MapFrom(src => src.LastLoginAt.HasValue && src.LastLoginAt.Value > DateTime.UtcNow.AddMinutes(-30)));

        CreateMap<CreateUserRequestDto, ApplicationUser>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.Email.ToUpperInvariant()))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpperInvariant()))
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.PhoneNumber)))
            .ForMember(dest => dest.SecurityStamp, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.ConcurrencyStamp, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Branch, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
            .ForMember(dest => dest.Orders, opt => opt.Ignore())
            .ForMember(dest => dest.FinancialTransactions, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedFinancialTransactions, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedOrders, opt => opt.Ignore());

        CreateMap<UpdateUserDto, ApplicationUser>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserName, opt => opt.Ignore())
            .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpperInvariant()))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyStamp, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.Branch, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
            .ForMember(dest => dest.Orders, opt => opt.Ignore())
            .ForMember(dest => dest.FinancialTransactions, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedFinancialTransactions, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedOrders, opt => opt.Ignore());
    }

    private void ConfigureAuditLogMappings()
    {
        // AuditLog mappings
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : src.UserName))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : src.BranchName))
            .ForMember(dest => dest.TimestampFormatted, opt => opt.MapFrom(src => src.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")))
            .ForMember(dest => dest.HasError, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.ErrorMessage)))
            .ForMember(dest => dest.ActionCategory, opt => opt.MapFrom(src => DetermineActionCategory(src.Action)))
            .ForMember(dest => dest.SeverityLevel, opt => opt.MapFrom(src => DetermineSeverityLevel(src.Action, src.ErrorMessage)));
    }

    private static string DetermineActionCategory(string action)
    {
        return action.ToUpperInvariant() switch
        {
            "CREATE" or "INSERT" => "Data Creation",
            "UPDATE" or "MODIFY" => "Data Modification",
            "DELETE" or "REMOVE" => "Data Deletion",
            "LOGIN" or "LOGOUT" => "Authentication",
            "ACCESS" or "VIEW" => "Data Access",
            "EXPORT" or "PRINT" => "Data Export",
            "APPROVE" or "REJECT" => "Workflow",
            _ => "General"
        };
    }

    private static string DetermineSeverityLevel(string action, string? errorMessage)
    {
        if (!string.IsNullOrEmpty(errorMessage))
            return "Error";

        return action.ToUpperInvariant() switch
        {
            "DELETE" or "REMOVE" => "High",
            "CREATE" or "UPDATE" or "APPROVE" => "Medium",
            "LOGIN" or "LOGOUT" or "ACCESS" or "VIEW" => "Low",
            _ => "Medium"
        };
    }
}

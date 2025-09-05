using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models.BranchModels;
using DijaGoldPOS.API.Models.Shared;

namespace DijaGoldPOS.API.Mappings;

/// <summary>
/// AutoMapper profile for ApplicationUser entity and related DTOs
/// </summary>
public class UserProfile : Profile
{
    public UserProfile()
    {
        // Entity to DTO mappings
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.UserName))
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.EmployeeCode, o => o.MapFrom(s => s.EmployeeCode))
            .ForMember(d => d.Roles, o => o.Ignore()) // Would need custom resolver
            .ForMember(d => d.BranchId, o => o.MapFrom(s => s.BranchId))
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch != null ? s.Branch.Name : null))
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.LastLoginAt, o => o.MapFrom(s => s.LastLoginAt))
            .ForMember(d => d.EmailConfirmed, o => o.MapFrom(s => s.EmailConfirmed))
            .ForMember(d => d.LockoutEnabled, o => o.MapFrom(s => s.LockoutEnabled))
            .ForMember(d => d.LockoutEnd, o => o.MapFrom(s => s.LockoutEnd));

        CreateMap<ApplicationUser, UserInfoDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.UserName))
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.EmployeeCode, o => o.MapFrom(s => s.EmployeeCode))
            .ForMember(d => d.Roles, o => o.Ignore()) // Would need custom resolver
            .ForMember(d => d.Branch, o => o.MapFrom(s => s.Branch))
            .ForMember(d => d.LastLoginAt, o => o.MapFrom(s => s.LastLoginAt));

        CreateMap<Branch, BranchInfoDto>();

        // DTO to Entity mappings
        CreateMap<CreateUserRequestDto, ApplicationUser>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.UserName))
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.EmployeeCode, o => o.MapFrom(s => s.EmployeeCode))
            .ForMember(d => d.BranchId, o => o.MapFrom(s => s.BranchId))
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive))
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.LastLoginAt, o => o.Ignore())
            .ForMember(d => d.EmailConfirmed, o => o.Ignore())
            .ForMember(d => d.LockoutEnabled, o => o.Ignore())
            .ForMember(d => d.LockoutEnd, o => o.Ignore())
            .ForMember(d => d.NormalizedUserName, o => o.Ignore())
            .ForMember(d => d.NormalizedEmail, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.SecurityStamp, o => o.Ignore())
            .ForMember(d => d.ConcurrencyStamp, o => o.Ignore())
            .ForMember(d => d.PhoneNumber, o => o.Ignore())
            .ForMember(d => d.PhoneNumberConfirmed, o => o.Ignore())
            .ForMember(d => d.TwoFactorEnabled, o => o.Ignore())
            .ForMember(d => d.AccessFailedCount, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.Orders, o => o.Ignore())
            .ForMember(d => d.FinancialTransactions, o => o.Ignore())
            .ForMember(d => d.ApprovedOrders, o => o.Ignore())
            .ForMember(d => d.AuditLogs, o => o.Ignore());

        CreateMap<UpdateUserRequestDto, ApplicationUser>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserName, o => o.Ignore())
            .ForMember(d => d.LastLoginAt, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.EmailConfirmed, o => o.Ignore())
            .ForMember(d => d.LockoutEnabled, o => o.Ignore())
            .ForMember(d => d.LockoutEnd, o => o.Ignore())
            .ForMember(d => d.NormalizedUserName, o => o.Ignore())
            .ForMember(d => d.NormalizedEmail, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.SecurityStamp, o => o.Ignore())
            .ForMember(d => d.ConcurrencyStamp, o => o.Ignore())
            .ForMember(d => d.PhoneNumber, o => o.Ignore())
            .ForMember(d => d.PhoneNumberConfirmed, o => o.Ignore())
            .ForMember(d => d.TwoFactorEnabled, o => o.Ignore())
            .ForMember(d => d.AccessFailedCount, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.Orders, o => o.Ignore())
            .ForMember(d => d.FinancialTransactions, o => o.Ignore())
            .ForMember(d => d.ApprovedOrders, o => o.Ignore())
            .ForMember(d => d.AuditLogs, o => o.Ignore())
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.EmployeeCode, o => o.MapFrom(s => s.EmployeeCode))
            .ForMember(d => d.BranchId, o => o.MapFrom(s => s.BranchId));

        // Search mappings
        CreateMap<UserSearchRequestDto, ApplicationUser>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserName, o => o.Ignore())
            .ForMember(d => d.FullName, o => o.Ignore())
            .ForMember(d => d.Email, o => o.Ignore())
            .ForMember(d => d.EmployeeCode, o => o.Ignore())
            .ForMember(d => d.BranchId, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.LastLoginAt, o => o.Ignore())
            .ForMember(d => d.EmailConfirmed, o => o.Ignore())
            .ForMember(d => d.LockoutEnabled, o => o.Ignore())
            .ForMember(d => d.LockoutEnd, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.Orders, o => o.Ignore())
            .ForMember(d => d.FinancialTransactions, o => o.Ignore())
            .ForMember(d => d.ApprovedOrders, o => o.Ignore())
            .ForMember(d => d.AuditLogs, o => o.Ignore());

        // Status update mapping
        CreateMap<UpdateUserStatusRequestDto, ApplicationUser>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.UserId))
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive))
            .ForMember(d => d.UserName, o => o.Ignore())
            .ForMember(d => d.FullName, o => o.Ignore())
            .ForMember(d => d.Email, o => o.Ignore())
            .ForMember(d => d.EmployeeCode, o => o.Ignore())
            .ForMember(d => d.BranchId, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.LastLoginAt, o => o.Ignore())
            .ForMember(d => d.EmailConfirmed, o => o.Ignore())
            .ForMember(d => d.LockoutEnabled, o => o.Ignore())
            .ForMember(d => d.LockoutEnd, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.Orders, o => o.Ignore())
            .ForMember(d => d.FinancialTransactions, o => o.Ignore())
            .ForMember(d => d.ApprovedOrders, o => o.Ignore())
            .ForMember(d => d.AuditLogs, o => o.Ignore());

        // Password reset mapping
        CreateMap<ResetPasswordRequestDto, ApplicationUser>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.UserId))
            .ForMember(d => d.UserName, o => o.Ignore())
            .ForMember(d => d.FullName, o => o.Ignore())
            .ForMember(d => d.Email, o => o.Ignore())
            .ForMember(d => d.EmployeeCode, o => o.Ignore())
            .ForMember(d => d.BranchId, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.LastLoginAt, o => o.Ignore())
            .ForMember(d => d.EmailConfirmed, o => o.Ignore())
            .ForMember(d => d.LockoutEnabled, o => o.Ignore())
            .ForMember(d => d.LockoutEnd, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.Orders, o => o.Ignore())
            .ForMember(d => d.FinancialTransactions, o => o.Ignore())
            .ForMember(d => d.ApprovedOrders, o => o.Ignore())
            .ForMember(d => d.AuditLogs, o => o.Ignore());

        // Activity and audit mappings
        CreateMap<AuditLog, UserActivityLogDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Timestamp, o => o.MapFrom(s => s.Timestamp))
            .ForMember(d => d.Action, o => o.MapFrom(s => s.Action))
            .ForMember(d => d.EntityType, o => o.MapFrom(s => s.EntityType))
            .ForMember(d => d.EntityId, o => o.MapFrom(s => s.EntityId))
            .ForMember(d => d.Details, o => o.MapFrom(s => s.Details))
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.BranchName))
            .ForMember(d => d.IpAddress, o => o.MapFrom(s => s.IpAddress));

        CreateMap<ApplicationUser, UserActivityDto>()
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.UserName))
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.Activities, o => o.Ignore()) // Would need custom resolver
            .ForMember(d => d.TotalActivities, o => o.Ignore()); // Would need custom resolver

        // Permissions mapping
        CreateMap<ApplicationUser, UserPermissionsDto>()
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.UserName))
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.Roles, o => o.Ignore()) // Would need custom resolver
            .ForMember(d => d.Permissions, o => o.Ignore()) // Would need custom resolver
            .ForMember(d => d.FeatureAccess, o => o.Ignore()); // Would need custom resolver
    }
}

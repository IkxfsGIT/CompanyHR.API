using AutoMapper;
using CompanyHR.API.Models;
using CompanyHR.API.DTOs;

namespace CompanyHR.API.Mappings;

/// <summary>
/// Профиль маппинга для AutoMapper
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Маппинг для Employee
        CreateMap<Employee, EmployeeDto>()
            .ForMember(dest => dest.FullName, 
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.YearsInCompany,
                opt => opt.MapFrom(src => (DateTime.UtcNow.Year - src.HireDate.Year)))
            .ReverseMap();

        CreateMap<CreateEmployeeDto, Employee>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateEmployeeDto, Employee>()
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Маппинг для Position
        CreateMap<Position, PositionDto>().ReverseMap();
        CreateMap<CreatePositionDto, Position>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        CreateMap<UpdatePositionDto, Position>()
            .ForMember(dest => dest.PositionId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Маппинг для Department (как модель)
        CreateMap<Department, DepartmentDto>().ReverseMap();
        CreateMap<CreateDepartmentDto, Department>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        CreateMap<UpdateDepartmentDto, Department>()
            .ForMember(dest => dest.DepartmentId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Маппинг для User и связанных DTO
        CreateMap<User, UserInfoDto>()
            .ForMember(dest => dest.FullName, 
                opt => opt.MapFrom(src => $"{src.Employee.FirstName} {src.Employee.LastName}"))
            .ForMember(dest => dest.Position, 
                opt => opt.MapFrom(src => src.Employee.Position))
            .ForMember(dest => dest.Department, 
                opt => opt.MapFrom(src => src.Employee.Department));

        CreateMap<RegisterDto, Employee>()
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());

        // Маппинг для AuditLog
        CreateMap<AuditLog, AuditLogDto>().ReverseMap();

        // Маппинг для RefreshToken
        CreateMap<RefreshToken, RefreshTokenDto>().ReverseMap();
    }
}

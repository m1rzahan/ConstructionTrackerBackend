using AutoMapper;
using ConstructionTracker.Authorization.Users;
using ConstructionTracker.ConstructionTracker.Dto;
using ConstructionTracker.Entities;

namespace ConstructionTracker.ConstructionTracker.AutoMapper
{
    public class ConstructionTrackerMappingProfile : Profile
    {
        public ConstructionTrackerMappingProfile()
        {
            // User mappings
            CreateMap<User, ConstructionTrackerUserDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.EmailAddress))
                .ForMember(dest => dest.CompanyName, opt => opt.Ignore()); // Set manually in service

            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.LastName));

            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.LastName));

            // Company mappings
            CreateMap<Company, CompanyDto>();
            CreateMap<CreateCompanyDto, Company>();
            CreateMap<UpdateCompanyDto, Company>();

            // Project mappings
            CreateMap<Project, ProjectDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.Ignore()) // Set manually in service
                .ForMember(dest => dest.AssignedUsers, opt => opt.Ignore()); // Set manually in service

            CreateMap<Project, ProjectSummaryDto>();

            CreateMap<CreateProjectDto, Project>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ProjectStatus.Planning))
                .ForMember(dest => dest.Progress, opt => opt.MapFrom(src => 0));

            CreateMap<UpdateProjectDto, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore());

            // UserProject mappings
            CreateMap<UserProject, UserProjectDto>();
            CreateMap<CreateUserProjectDto, UserProject>();

            // ActivityLog mappings
            CreateMap<ActivityLog, ActivityLogDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
                    src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : ""))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => 
                    src.Project != null ? src.Project.Name : ""))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => 
                    src.Company != null ? src.Company.Name : ""));

            CreateMap<CreateActivityLogDto, ActivityLog>();

            // QrCodeScan mappings
            CreateMap<QrCodeScan, QrCodeScanDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
                    src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : ""))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => 
                    src.Project != null ? src.Project.Name : ""));

            CreateMap<CreateQrCodeScanDto, QrCodeScan>();

            // ProjectMaterial mappings
            CreateMap<ProjectMaterial, ProjectMaterialDto>();
            CreateMap<CreateProjectMaterialDto, ProjectMaterial>();
            CreateMap<UpdateProjectMaterialDto, ProjectMaterial>();
        }
    }

    // Additional DTOs needed for mappings
    public class CompanyDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string TaxNumber { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateCompanyDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string TaxNumber { get; set; }
    }

    public class UpdateCompanyDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string TaxNumber { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserProjectDto
    {
        public long UserId { get; set; }
        public string ProjectId { get; set; }
        public string Role { get; set; }
        public string AssignedDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateUserProjectDto
    {
        public long UserId { get; set; }
        public string ProjectId { get; set; }
        public string Role { get; set; }
    }

    public class ProjectMaterialDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProjectId { get; set; }
        public string Unit { get; set; }
        public decimal RequiredQuantity { get; set; }
        public decimal UsedQuantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalCost { get; set; }
        public string Supplier { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Status { get; set; }
    }

    public class CreateProjectMaterialDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProjectId { get; set; }
        public string Unit { get; set; }
        public decimal RequiredQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Supplier { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
    }

    public class UpdateProjectMaterialDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public decimal RequiredQuantity { get; set; }
        public decimal UsedQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Supplier { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Status { get; set; }
    }
} 
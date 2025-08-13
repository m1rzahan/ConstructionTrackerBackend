using Abp.Application.Services;
using Abp.Application.Services.Dto;
using ConstructionTracker.ConstructionTracker.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConstructionTracker.ConstructionTracker.Services
{
    public interface IPersonnelAppService : IApplicationService
    {
        // Basic CRUD operations
        Task<PagedResultDto<ConstructionTrackerUserDto>> GetAllPersonnelAsync(PagedPersonnelRequestDto input);
        Task<ConstructionTrackerUserDto> GetPersonnelAsync(long id);
        Task<ConstructionTrackerUserDto> CreatePersonnelAsync(CreatePersonnelDto input);
        Task<ConstructionTrackerUserDto> UpdatePersonnelAsync(UpdatePersonnelDto input);
        Task DeletePersonnelAsync(long id);

        // Personnel projects
        Task<List<ProjectSummaryDto>> GetPersonnelProjectsAsync(long personnelId);
        Task<PagedResultDto<QrCodeScanDto>> GetPersonnelScansAsync(long personnelId, PagedAndSortedResultRequestDto input);

        // Statistics
        Task<PersonnelStatsDto> GetPersonnelStatsAsync(long personnelId);
        Task<GeneralPersonnelStatsDto> GetGeneralPersonnelStatsAsync(Guid? companyId = null);

        // Filtering and searching
        Task<List<ConstructionTrackerUserDto>> GetActivePersonnelAsync();
        Task<List<ConstructionTrackerUserDto>> GetCompanyPersonnelAsync(Guid companyId);
        Task<List<ConstructionTrackerUserDto>> GetProjectPersonnelAsync(Guid projectId);

        // Personnel management
        Task ResetPersonnelPasswordAsync(long personnelId, string newPassword);
        Task AssignPersonnelToProjectAsync(long personnelId, Guid projectId, string role);
        Task RemovePersonnelFromProjectAsync(long personnelId, Guid projectId);
    }
}

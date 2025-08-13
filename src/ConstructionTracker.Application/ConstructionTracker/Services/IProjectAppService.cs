using Abp.Application.Services;
using Abp.Application.Services.Dto;
using ConstructionTracker.ConstructionTracker.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConstructionTracker.ConstructionTracker.Services
{
    public interface IProjectAppService : IApplicationService
    {
        // CRUD Operations
        Task<ProjectDto> GetAsync(Guid id);
        Task<PagedResultDto<ProjectDto>> GetAllAsync(PagedAndSortedResultRequestDto input);
        Task<List<ProjectSummaryDto>> GetUserProjectsAsync(long userId);
        Task<ProjectDto> CreateAsync(CreateProjectDto input);
        Task<ProjectDto> UpdateAsync(UpdateProjectDto input);
        Task DeleteAsync(Guid id);

        // Project Management
        Task<List<ConstructionTrackerUserDto>> GetProjectUsersAsync(Guid projectId);
        Task AssignUserToProjectAsync(Guid projectId, long userId, string role);
        Task RemoveUserFromProjectAsync(Guid projectId, long userId);
        Task UpdateProjectProgressAsync(Guid projectId, decimal progress);
        Task CompleteProjectAsync(Guid projectId);

        // Statistics
        Task<ProjectStatsDto> GetProjectStatsAsync(Guid? companyId = null);
        Task<List<ProjectSummaryDto>> GetActiveProjectsAsync();
        Task<List<ProjectSummaryDto>> GetDelayedProjectsAsync();
    }
} 